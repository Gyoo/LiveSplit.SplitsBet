using LiveSplit.Model;
using LiveSplit.Model.Input;
using LiveSplit.Options;
using LiveSplit.SplitsBet;
using LiveSplit.UI.Components;
using LiveSplit.Web.Share;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LiveSplit.TimeFormatters;
using System.Windows.Forms;
using System.ComponentModel;
using LiveSplit.Model.Comparisons;
using LiveSplit.UI;

namespace LiveSplit.SplitsBet
{
    public class SplitsBetComponent : LogicComponent
    {
        #region Properties

        public Settings Settings { get; set; }
        protected LiveSplitState State { get; set; }
        protected int SplitIndex { get; set; }
        protected int BetIndex { get; set; }

        //Commands : where the commands starting with an excalamation mark are stored. Key is the command, Value is the related method
        public Dictionary<String, Action<TwitchChat.User, string>> Commands { get; set; }

        //Bets : Array of dictionaries that contains all the bets from all the players for every segment. The array's length is the number of segments, and the dictionary contains the name of the player as a key, and a tuple with the time of the bet and the score coefficient
        private Dictionary<string, Tuple<TimeSpan, double>>[] Bets { get; set; }
        private Dictionary<string, TimeSpan> SpecialBets { get; set; }
        private Dictionary<string, int>[] Scores { get; set; }
        private Time[] SegmentBeginning { get; set; }
        private bool ActiveSpecialBets { get; set; }
        private bool EndOfRun { get; set; }

        public override string ComponentName
        {
            get { return "Splits Bet Bot"; }
        }

        #endregion

        #region Constructors

        public SplitsBetComponent(LiveSplitState state)
        {
            /*Initializing variables*/
            Settings = new Settings()
            {
                LivesplitState = state
            };
            Commands = new Dictionary<string, Action<TwitchChat.User, string>>();
            State = state;
            State.ComparisonRenamed += State_ComparisonRenamed;
            SpecialBets = new Dictionary<string, TimeSpan>();
            ActiveSpecialBets = false;
            EndOfRun = false;
            // Arrays are initialized in a separate method so their size can be updated in the Update() method when new splits are loaded.
            initArrays();

            /*Adding global commands*/
            Commands.Add("betcommands", BetCommands);
            Commands.Add("version", Version);
            Commands.Add("checkbet", CheckBet);
            Commands.Add("unbet", UnBet);
            Commands.Add("score", Score);
            Commands.Add("highscore", Highscore);
            Commands.Add("specialbet", SpecialBet);
            Commands.Add("stop", DisableBets);

            /*Setting Livesplit events*/
            State.OnStart += StartBets;
            State.OnSplit += State_OnSplit;
            State.OnUndoSplit += RollbackScore;
            State.OnSkipSplit += CopyScore;
            State.OnReset += ResetSplitsBet;

            SplitIndex = State.CurrentSplitIndex;

            /*Bot is ready !*/
            Start();
            SendMessage(Settings.msgEnable);
        }

        #endregion

        #region Methods

        private void initArrays()
        {
            if (Settings.ParentSubSplits)
            {
                int i = 0;
                foreach (Segment s in State.Run)
                {
                    if (s.Name.Substring(0, 1) != "-") i++;
                }
                SegmentBeginning = new Time[i];
                Bets = new Dictionary<string, Tuple<TimeSpan, double>>[i];
                Scores = new Dictionary<string, int>[i];
            }
            else
            {
                SegmentBeginning = new Time[State.Run.Count];
                Bets = new Dictionary<string, Tuple<TimeSpan, double>>[State.Run.Count];
                Scores = new Dictionary<string, int>[State.Run.Count];
            }
            BetIndex = 0;
        }
        /**
         * Connection of the bot to Twitch
         */
        public void Start()
        {
            if (!Twitch.Instance.IsLoggedIn)
            {
                var thread = new Thread(() => Twitch.Instance.VerifyLogin()) { ApartmentState = ApartmentState.STA };
                thread.Start();
                thread.Join();
            }

            if (!Twitch.Instance.ConnectedChats.ContainsKey(Twitch.Instance.ChannelName))
            {
                Twitch.Instance.ConnectToChat(Twitch.Instance.ChannelName);
            }

            Twitch.Instance.Chat.OnMessage += OnMessage;
        }

        private void newSplit()
        {
            EndOfRun = false;
            if (!Commands.ContainsKey("bet")) Commands.Add("bet", Bet); //Prevents players from betting between a !start and the next split, if !start is made during the run.
            try
            {
                SegmentBeginning[BetIndex] = State.CurrentTime;
                Bets[BetIndex] = new Dictionary<string, Tuple<TimeSpan, double>>();
                var timeFormatter = new ShortTimeFormatter();
                var comparison = State.Run.Comparisons.Contains(Settings.TimeToShow) ? Settings.TimeToShow : Run.PersonalBestComparisonName;
                var previousTime = lastParent(SplitIndex) >= 0
                    ? GetTime(State.Run[lastParent(SplitIndex)].Comparisons[comparison])
                    : TimeSpan.Zero;
                Time timeToFormat = new Time(new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 0));
                timeToFormat += State.Run[nextParent(SplitIndex)].Comparisons[comparison];
                var timeFormatted = timeFormatter.Format(GetTime(timeToFormat) - previousTime);
                string ret = "Place your bets for " + State.Run[nextParent(SplitIndex)].Name + "! ";
                if (TimeSpanParser.Parse(timeFormatted) > TimeSpan.Zero && comparison != "None")
                {
                    ret += CompositeComparisons.GetShortComparisonName(comparison) + " segment for this split is " + timeFormatted + " ";
                }
                if (Settings.UseGlobalTime) ret += "And remember that global time is used to bet!";

                SendMessage(ret);
            }
            catch (Exception ex) { LogException(ex); }
        }

        private int nextParent(int split)
        {
            int i = split;
            for (; i < State.Run.Count(); i++)
            {
                if (!Settings.ParentSubSplits || State.Run[i].Name.Substring(0, 1) != "-") break;
            }
            return i;
        }

        private int lastParent(int split)
        {
            int i = split - 1;
            for (; i >= 0; i--)
            {
                if (!Settings.ParentSubSplits || State.Run[i].Name.Substring(0, 1) != "-") break;
            }
            return i;
        }

        private void ShowScore()
        {
            try
            {
                string singleLine = "";
                var orderedScores = Scores[BetIndex].OrderByDescending(x => x.Value);
                int scoresShown = 0;
                if (orderedScores.Count() > 0)
                {
                    if (!Settings.SingleLineScores)
                        SendMessage("Top " + ((Settings.NbScores > orderedScores.Count()) ? orderedScores.Count() : Settings.NbScores));
                    else
                        singleLine += "Top " + ((Settings.NbScores > orderedScores.Count()) ? orderedScores.Count() : Settings.NbScores) + ": ";
                    foreach (var entry in orderedScores)
                    {
                        var delta = 0;
                        if (BetIndex - 1 >= 0 && Scores[BetIndex - 1].ContainsKey(entry.Key))
                        {
                            delta = entry.Value - Scores[BetIndex - 1][entry.Key];
                        }
                        if (!Settings.SingleLineScores)
                            SendMessage(entry.Key + ": " + entry.Value + (delta != 0 ? (" (" + (delta < 0 ? "-" : "+") + delta + ")") : ""));
                        else
                            singleLine += entry.Key + ": " + entry.Value + (delta != 0 ? (" (" + (delta < 0 ? "-" : "+") + delta + ")") : "") + " || ";
                        if (++scoresShown >= Settings.NbScores) break;
                    }
                    if (Settings.SingleLineScores) SendMessage(singleLine.Substring(0, singleLine.Length - 4));
                }
            }
            catch (Exception ex) { LogException(ex); }
        }

        /**
         * Use this method to send a message with the bot
         */
        private void SendMessage(string message)
        {
            Twitch.Instance.Chat.SendMessage("/me " + message);
        }

        static void Delay(int ms, EventHandler action)
        {
            var tmp = new System.Windows.Forms.Timer { Interval = ms };
            tmp.Tick += new EventHandler((o, e) => tmp.Enabled = false);
            tmp.Tick += action;
            tmp.Enabled = true;
        }

        private TimeSpan? GetTime(Time segment)
        {
            return segment[Settings.OverridenTimingMethod ?? State.CurrentTimingMethod];
        }

        private void LogException(Exception e)
        {
            Log.Error(e);
            SendMessage("An error occured - please look into the system event log for details.");
        }

        private int getScore(KeyValuePair<string, Tuple<TimeSpan, double>> entry, TimeSpan? segmentTimeSpan)
        {
            double percentage = (Math.Floor(segmentTimeSpan.Value.TotalSeconds) - Math.Floor(entry.Value.Item1.TotalSeconds)) / Math.Floor(segmentTimeSpan.Value.TotalSeconds);
            return (int)Math.Floor(entry.Value.Item2 * Math.Floor(segmentTimeSpan.Value.TotalSeconds) * Math.Exp(-Math.Floor(segmentTimeSpan.Value.TotalSeconds)*(Math.Pow(percentage, 2))));
        }

        #endregion

        #region Commands
        //Commands usually have self explaining names

        /**
         * Register a bet for the player "user"
         */
        private void Bet(TwitchChat.User user, string argument)
        {
            //Check the state of the timer. It must be running to set a bet
            if (SplitIndex < 0)
            {
                SendMessage(Settings.msgTimerNotRunning);
                return;
            }
            if (State.CurrentPhase == TimerPhase.Paused)
            {
                SendMessage(Settings.msgTimerPaused);
                return;
            }
            if (SplitIndex >= State.Run.Count)
            {
                SendMessage(Settings.msgTimerEnded);
                return;
            }

            //You can't bet again if you have already bet once, unless you type !unbet, which erases your bet (and some of your points)
            if (Bets[BetIndex].ContainsKey(user.Name))
            {
                SendMessage(user.Name + ": You already bet!");
                return;
            }

            double percentage;
            var timeFormatted = new ShortTimeFormatter().Format(GetTime(State.Run[SplitIndex].BestSegmentTime));
            //If no glod is set, percentage is kept to 0. There's no way to set a limit so better not fix an arbitrary one.
            if (TimeSpanParser.Parse(timeFormatted) > TimeSpan.Zero)
                percentage = (GetTime(State.CurrentTime - SegmentBeginning[BetIndex])).Value.TotalSeconds / GetTime(State.Run[BetIndex].BestSegmentTime).Value.TotalSeconds;
            else
                percentage = 0;

            //Forbids the bets if the time of the segment reaches over 75% of the best segment
            if (percentage > 0.75)
            {
                SendMessage(Settings.msgTooLateToBet);
                return;
            }

            try
            {
                //nice meme
                if (argument.ToLower().StartsWith("kappa"))
                {
                    argument = "4:20.69";
                    SendMessage(user.Name + " bet 4:20.69 Kappa");
                }

                //Parsing the time and checking if it's valid
                var time = TimeSpanParser.Parse(argument);
                if (Settings.UseGlobalTime) time -= GetTime(SegmentBeginning[SplitIndex]).Value;
                if (time.CompareTo(Settings.MinimumTime) <= 0)
                {
                    SendMessage(user.Name + ": Invalid time, please retry.");
                    return;
                }

                //Add the bet to its Dictionary
                var t = new Tuple<TimeSpan, double>(time, Math.Exp(-2 * Math.Pow(percentage, 4)));
                Bets[BetIndex].Add(user.Name, t);
            }
            catch
            {
                SendMessage(user.Name + ": Invalid time, please retry.");
            }
        }

        private void CheckBet(TwitchChat.User user, string argument)
        {
            if (SplitIndex < 0)
            {
                SendMessage(Settings.msgTimerNotRunning);
                return;
            }
            if (SplitIndex >= State.Run.Count)
            {
                SendMessage(Settings.msgCheckTimerEnd);
                return;
            }

            if (Bets[SplitIndex].ContainsKey(user.Name))
            {
                var timeFormatter = new ShortTimeFormatter();
                var time = Bets[SplitIndex][user.Name].Item1;
                var formattedTime = timeFormatter.Format(time);
                SendMessage(user.Name + ": Your bet for " + State.Run[SplitIndex].Name + " is " + formattedTime);
            }
            else
            {
                SendMessage(user.Name + ": You didn't bet for this split yet!");
            }
        }

        private void UnBet(TwitchChat.User user, string argument)
        {
            if (!Settings.CanUnBet)
            {
                SendMessage(Settings.msgNoUnbet);
                return;
            }

            if (SplitIndex < 0)
            {
                SendMessage(Settings.msgTimerNotRunning);
                return;
            }
            if (SplitIndex >= State.Run.Count)
            {
                SendMessage(Settings.msgUnbetTimerEnd);
                return;
            }

            if (SplitIndex - 1 < 0)
            {
                SendMessage(user.Name + ": You have no points to spend on undoing your bet yet!");
                return;
            }

            if (!Bets[BetIndex].ContainsKey(user.Name))
            {
                SendMessage(user.Name + ": You didn't bet for this split yet!");
                return;
            }

            if (!Scores[BetIndex - 1].ContainsKey(user.Name) || Scores[BetIndex - 1][user.Name] < Settings.UnBetPenalty)
            {
                SendMessage(user.Name + ": You need " + Settings.UnBetPenalty + " points to undo your bet but only have " + (Scores[BetIndex - 1].ContainsKey(user.Name)?Scores[SplitIndex - 1][user.Name]:0) + ".");
                return;
            }

            if (BetIndex >= 0 && Scores[BetIndex - 1] != null)
            {
                Scores[BetIndex-1][user.Name] -= Settings.UnBetPenalty;
            }
            Bets[BetIndex].Remove(user.Name);
        }

        private void BetCommands(TwitchChat.User user, string argument)
        {
            var ret = Commands
                .Select(x => "!" + x.Key)
                .Aggregate((a, b) => a + " " + b);

            SendMessage(ret);
        }

        private void Score(TwitchChat.User user, string argument)
        {
            if (SplitIndex < 0)
            {
                SendMessage(Settings.msgNoScore);
                return;
            }
            if (SplitIndex == 0 || !Scores[BetIndex - 1].ContainsKey(user.Name))
            {
                SendMessage(user.Name + "'s score is 0");
                return;
            }
            SendMessage(user.Name + "'s score is " + Scores[BetIndex - 1][user.Name]);
        }

        private void Highscore(TwitchChat.User user, string argument)
        {
            if (BetIndex < 0)
            {
                SendMessage(Settings.msgNoScore);
                return;
            }
            if (BetIndex > 0 && Scores[BetIndex - 1].Count > 0)
            {
                var orderedScores = Scores[BetIndex - 1].OrderByDescending(x => x.Value);
                SendMessage(orderedScores.ToList()[0].Key + "'s score is " + orderedScores.ToList()[0].Value);
            }
            else SendMessage(Settings.msgNoHighscore);
        }

        private void EnableBets(TwitchChat.User user, string argument)
        {
            if (user.Badges.HasFlag(TwitchChat.ChatBadges.Broadcaster) || Settings.AllowMods && user.Badges.HasFlag(TwitchChat.ChatBadges.Moderator))
            {
                /*Adding bet related commands*/
                Commands.Add("checkbet", CheckBet);
                Commands.Add("unbet", UnBet);
                Commands.Add("score", Score);
                Commands.Add("highscore", Highscore);
                Commands.Add("specialbet", SpecialBet);

                Commands.Remove("start");
                Commands.Add("stop", DisableBets);

                /*Setting Livesplit events*/
                State.OnStart += StartBets;
                State.OnSplit += State_OnSplit;
                State.OnUndoSplit += RollbackScore;
                State.OnSkipSplit += CopyScore;
                State.OnReset += ResetSplitsBet;

                SplitIndex = State.CurrentSplitIndex;

                SendMessage(Settings.msgEnable);
                if (State.CurrentPhase != TimerPhase.NotRunning)
                {
                    for (int i = 0; i < SplitIndex; i++)
                    {
                        if (null == Scores[i])
                        {
                            Scores[i] = (i == 0 ? new Dictionary<string, int>() : new Dictionary<string, int>(Scores[i - 1]));
                        }
                    }
                    for (int i = 0; i <= SplitIndex; i++) Bets[i] = new Dictionary<string, Tuple<TimeSpan, double>>();

                }
            }
            else SendMessage("You're not allowed to start the bets!");
        }

        private void DisableBets(TwitchChat.User user, string argument)
        {
            if (user.Badges.HasFlag(TwitchChat.ChatBadges.Broadcaster) || Settings.AllowMods && user.Badges.HasFlag(TwitchChat.ChatBadges.Moderator))
            {
                /*Removing bet related commands*/
                Commands.Remove("bet");
                Commands.Remove("checkbet");
                Commands.Remove("unbet");
                Commands.Remove("score");
                Commands.Remove("highscore");
                Commands.Remove("specialbet");

                Commands.Remove("stop");
                Commands.Add("start", EnableBets);

                /*Removing events*/
                State.OnStart -= StartBets;
                State.OnSplit -= State_OnSplit;
                State.OnUndoSplit -= RollbackScore;
                State.OnSkipSplit -= CopyScore;
                State.OnReset -= ResetSplitsBet;
                SendMessage(Settings.msgDisable);
            }
            else SendMessage("You're not allowed to stop the bets!");
        }

        private void SpecialBet(TwitchChat.User user, string argument)
        {
            if (argument.ToLower().StartsWith("start"))
            {
                ActiveSpecialBets = true;
                return;
            }
            if (argument.ToLower().StartsWith("end") || argument.ToLower().StartsWith("stop"))
            {
                var args = argument.Split(new string[] { " " }, StringSplitOptions.None);
                if (args.Count() > 1)
                {
                    try
                    {
                        //TODO Accuracy better than seconds, special events are meant to be short (OoT Dampe < 1 min iirc, SM64 Secret Slide between 12.5s and 13s generally...)
                        var time = TimeSpanParser.Parse(args[1]);
                        Scores[SplitIndex] = Scores[SplitIndex] ?? (SplitIndex > 0 ? new Dictionary<string, int>(Scores[SplitIndex - 1]) : new Dictionary<string, int>());
                        foreach (KeyValuePair<string, TimeSpan> entry in SpecialBets)
                        {
                            if (Scores[SplitIndex].ContainsKey(entry.Key))
                            {
                                Scores[SplitIndex][entry.Key] += (int)(time.TotalSeconds * Math.Exp(-(Math.Pow((int)time.TotalSeconds - (int)entry.Value.TotalSeconds, 2) / (int)time.TotalSeconds)));
                            }
                            else Scores[SplitIndex].Add(entry.Key, (int)(time.TotalSeconds * Math.Exp(-(Math.Pow((int)time.TotalSeconds - (int)entry.Value.TotalSeconds, 2) / (int)time.TotalSeconds))));
                        }
                        SendMessage("Scores will be shown at the next split!");
                        //TODO Special ShowScore() for special bets ?
                        SpecialBets.Clear();
                    }
                    catch (Exception e)
                    {
                        LogException(e);
                    }

                }

                ActiveSpecialBets = false;
                return;
            }
            if (!ActiveSpecialBets)
            {
                SendMessage("No special bet active.");
            }
            switch (State.CurrentPhase)
            {
                case TimerPhase.NotRunning:
                    SendMessage(Settings.msgTimerNotRunning);
                    return;
                case TimerPhase.Paused:
                    SendMessage(Settings.msgTimerPaused);
                    return;
                case TimerPhase.Ended:
                    SendMessage(Settings.msgTimerEnded);
                    return;
            }

            if (SpecialBets.ContainsKey(user.Name))
            {
                SendMessage(user.Name + ": You already bet, silly!");
                return;
            }

            try
            {
                var time = TimeSpanParser.Parse(argument);
                if (time.CompareTo(Settings.MinimumTime) <= 0)
                {
                    SendMessage(user.Name + ": Invalid time, please retry.");
                    return;
                }
                SpecialBets.Add(user.Name, time);
            }
            catch
            {
                LogException(e);
            }
        }

        private void Version(TwitchChat.User user, string argument)
        {
            SendMessage("LiveSplit version " + SplitsBetFactory.VersionString + (SplitsBetFactory.VersionPostfix ?? ""));
        }

        #endregion

        #region Events

        void State_ComparisonRenamed(object sender, EventArgs e)
        {
            var args = (RenameEventArgs)e;
            if (Settings.TimeToShow == args.OldName)
            {
                Settings.TimeToShow = args.NewName;
                ((LiveSplitState)sender).Layout.HasChanged = true;
            }
        }
        void State_OnSplit(object sender, EventArgs e)
        {
            SplitIndex++;
            if (!Settings.ParentSubSplits || State.Run[SplitIndex - 1].Name.Substring(0, 1) != "-") CalculateScore(sender, e);
        }

        private void OnMessage(object sender, TwitchChat.Message message)
        {
            if (message.Text.StartsWith("!"))
            {
                try
                {
                    var splits = message.Text.Substring(1).Split(new char[] { ' ' }, 2);
                    if (Commands.ContainsKey(splits[0].ToLower()))
                    {
                        Commands[splits[0].ToLower()].Invoke(message.User, splits.Length > 1 ? splits[1] : "");
                    }
                }
                catch (Exception e) { LogException(e); }
            }
        }

        private void StartBets(object sender, EventArgs e)
        {
            BackgroundWorker invoker = new BackgroundWorker();
            invoker.DoWork += delegate
            {
                Thread.Sleep(TimeSpan.FromSeconds(Settings.Delay) - State.Run.Offset); //Offset is mostly negative
                SplitIndex = 0;
                newSplit();
            };
            invoker.RunWorkerAsync();
        }

        private void CalculateScore(object sender, EventArgs e)
        {
            BackgroundWorker invoker = new BackgroundWorker();
            invoker.DoWork += delegate
            {
                Thread.Sleep(TimeSpan.FromSeconds(Settings.Delay));
                try
                {
                    //TODO Hide the "Time for this split was..." message if the segment time is <= 0 (yes it can happen)
                    var segment = State.CurrentTime - SegmentBeginning[BetIndex];
                    //Add delay time to the last split
                    if (SplitIndex >= State.Run.Count())
                    {
                        segment += new Time(new TimeSpan(0, 0, Settings.Delay), new TimeSpan(0, 0, Settings.Delay));
                    }
                    var timeFormatter = new ShortTimeFormatter();
                    TimeSpan? segmentTimeSpan = GetTime(segment);
                    SendMessage("The time for this split was " + timeFormatter.Format(segmentTimeSpan));
                    Scores[BetIndex] = Scores[BetIndex] ?? (BetIndex > 0 ? new Dictionary<string, int>(Scores[BetIndex - 1]) : new Dictionary<string, int>());
                    foreach (KeyValuePair<string, Tuple<TimeSpan, double>> entry in Bets[BetIndex])
                    {
                        double score = getScore(entry, segmentTimeSpan);
                        if (Scores[BetIndex].ContainsKey(entry.Key))
                        {
                            Scores[BetIndex][entry.Key] += (int)score;
                        }
                        else Scores[BetIndex].Add(entry.Key, (int)score);
                    }
                    ShowScore();
                    if (++BetIndex < Scores.Count())
                    {
                        newSplit();
                    }
                    else EndOfRun = true;
                }
                catch (Exception ex) { LogException(ex); }
            };
            invoker.RunWorkerAsync();
        }

        private void RollbackScore(object sender, EventArgs e)
        {
            BackgroundWorker invoker = new BackgroundWorker();
            invoker.DoWork += delegate
            {
                Thread.Sleep(TimeSpan.FromSeconds(Settings.Delay));
                try
                {
                    if (BetIndex > 0 && nextParent(SplitIndex - 1) != nextParent(SplitIndex))
                    {
                        BetIndex--;
                        Scores[BetIndex + 1] = null;
                        Scores[BetIndex] = null;
                        if (BetIndex > 0) Scores[BetIndex] = new Dictionary<string, int>(Scores[BetIndex - 1]);
                        else Scores[BetIndex] = new Dictionary<string, int>();
                    }
                    SplitIndex--;
                }
                catch (Exception ex)
                {
                    LogException(ex);
                };
            };
            invoker.RunWorkerAsync();
        }

        private void CopyScore(object sender, EventArgs e)
        {
            BackgroundWorker invoker = new BackgroundWorker();
            invoker.DoWork += delegate
            {
                Thread.Sleep(TimeSpan.FromSeconds(Settings.Delay));
                SplitIndex++;
                try
                {
                    if (Commands.ContainsKey("bet")) Commands.Remove("bet"); //It's pointless to try to bet after a skipped split since no segment time will be set. Maybe only if you use split time instead of segment time ?
                    if (BetIndex > 0) Scores[BetIndex] = new Dictionary<string, int>(Scores[BetIndex - 1]);
                    else Scores[BetIndex] = new Dictionary<string, int>();
                    Bets[BetIndex] = new Dictionary<string, Tuple<TimeSpan, double>>();
                }
                catch (Exception ex) { LogException(ex); }
            };
            invoker.RunWorkerAsync();
        }

        private void ResetSplitsBet(object sender, TimerPhase value)
        {
            BackgroundWorker invoker = new BackgroundWorker();
            invoker.DoWork += delegate
            {
                Thread.Sleep(TimeSpan.FromSeconds(Settings.Delay));
                SplitIndex = -1;
                BetIndex = -1;
                try
                {
                    Array.Clear(Bets, 0, Bets.Length);
                    Array.Clear(Scores, 0, Scores.Length);
                    SpecialBets.Clear();
                    initArrays();
                }
                catch (Exception ex) { LogException(ex); }
                if (!EndOfRun) SendMessage(Settings.msgReset);
            };
            invoker.RunWorkerAsync();

        }

        #endregion

        #region Misc

        public override System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        public override System.Windows.Forms.Control GetSettingsControl(UI.LayoutMode mode)
        {
            return Settings;
        }

        public override void SetSettings(System.Xml.XmlNode settings)
        {
            Settings.SetSettings(settings);
        }

        public override void Update(UI.IInvalidator invalidator, Model.LiveSplitState state, float width, float height, UI.LayoutMode mode)
        {
            if (Scores.Count() != State.Run.Count) initArrays();
        }

        public override void Dispose()
        {
            Twitch.Instance.Chat.OnMessage -= OnMessage;

            State.OnStart -= StartBets;
            State.OnSplit -= State_OnSplit;
            State.OnUndoSplit -= RollbackScore;
            State.OnSkipSplit -= CopyScore;
            State.OnReset -= ResetSplitsBet;
        }

        #endregion
    }
}
