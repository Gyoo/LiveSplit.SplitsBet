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

namespace LiveSplit.SplitsBet
{
    public class SplitsBetComponent : LogicComponent
    {
        #region Properties

        public Settings Settings { get; set; }
        protected LiveSplitState State { get; set; }
        public Dictionary<String, Action<TwitchChat.User, string>> Commands { get; set; }
        private Dictionary<string, Tuple<TimeSpan, double>>[] Bets { get; set; }
        private Dictionary<string, int>[] Scores { get; set; }
        private Time SegmentBeginning { get; set; }
        private bool CanBet { get; set; }

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
            SegmentBeginning = new Time();
            Bets = new Dictionary<string, Tuple<TimeSpan, double>>[State.Run.Count];
            Scores = new Dictionary<string, int>[State.Run.Count];
            CanBet = false;

            /*Adding available commands*/
            Commands.Add("bet", Bet);
            Commands.Add("checkbet", CheckBet);
            Commands.Add("unbet", UnBet);
            Commands.Add("betcommands", BetCommands);
            Commands.Add("score", Score);
            Commands.Add("highscore", Highscore);
            Commands.Add("start", EnableBets);
            Commands.Add("stop", DisableBets);

            /*Setting Livesplit events*/
            State.OnStart += StartBets;
            State.OnSplit += CalculateScore;
            State.OnUndoSplit += RollbackScore;
            State.OnSkipSplit += CopyScore;
            State.OnReset += State_OnReset;

            /*Bot is ready !*/
            Start();
        }

        #endregion

        #region Methods

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

        private void SendMessage(string message)
        {
            Twitch.Instance.Chat.SendMessage("/me " + message);
        }

        #endregion

        #region Commands
        //TODO Errors management with Twitch messages, will be useful for alpha release in case of bugs
        
        private void Bet(TwitchChat.User user, string argument)
        {
            if (!CanBet) return;
            switch (State.CurrentPhase)
            {
                case TimerPhase.NotRunning:
                    SendMessage("Timer is not running, bets are closed");
                    return;
                case TimerPhase.Paused:
                    SendMessage("Timer is paused, bets are paused too");
                    return;
                case TimerPhase.Ended:
                    SendMessage("Run is ended, there is nothing to bet!");
                    return;
            }

            if (Bets[State.CurrentSplitIndex].ContainsKey(user.Name))
            {
                SendMessage(user.Name + ", You already bet, silly!");
                return;
            }

            var percentage = GetTime((State.CurrentTime - SegmentBeginning)).Value.TotalSeconds / GetTime(State.CurrentSplit.BestSegmentTime).Value.TotalSeconds;
            
            if (percentage > 0.75)
            {
                SendMessage("Too late to bet for this split, wait for the next one!");
                return;
            }

            try
            {
                if (argument.ToLower().Contains("kappa"))
                {
                    argument = "4:20.69";
                    SendMessage(user.Name + " bet 4:20.69 Kappa");
                }

                var time = TimeSpanParser.Parse(argument);
                if (Settings.UseGlobalTime) time -= GetTime(SegmentBeginning).Value;
                if (time.CompareTo(Settings.MinimumTime) <= 0)
                {
                    SendMessage(user.Name + ", Nice try, but it's invalid");
                    return;
                }
                var t = new Tuple<TimeSpan, double>(time, Math.Exp(-2 * Math.Pow(percentage, 2)));
                Bets[State.CurrentSplitIndex].Add(user.Name, t);
            }
            catch
            {
                SendMessage(user.Name + ", Invalid time, please retry");
            }
        }

        private void CheckBet(TwitchChat.User user, string argument)
        {
            if (!CanBet) return;
            switch (State.CurrentPhase)
            {
                case TimerPhase.NotRunning:
                    SendMessage("Timer is not running, bets are closed");
                    return;
                case TimerPhase.Ended:
                    SendMessage("The run has ended, nothing to check!");
                    return;
            }

            if (Bets[State.CurrentSplitIndex].ContainsKey(user.Name))
            {
                var timeFormatter = new ShortTimeFormatter();
                var time = Bets[State.CurrentSplitIndex][user.Name].Item1;
                var formattedTime = timeFormatter.Format(time);
                SendMessage(user.Name + ", Your bet for " + State.CurrentSplit.Name + " is " + formattedTime);
            }
            else
            {
                SendMessage(user.Name + ", You didn't bet for this split yet!");
            }
        }

        private void UnBet(TwitchChat.User user, string argument)
        {
            if (!CanBet) return;
            if (!Settings.CanUnBet)
            {
                SendMessage("You can't unbet :(");
                return;
            }
            switch (State.CurrentPhase)
            {
                case TimerPhase.NotRunning:
                    SendMessage("Timer is not running, bets are closed");
                    return;
                case TimerPhase.Ended:
                    SendMessage("The run has ended, nothing to unbet!");
                    return;
            }

            if (State.CurrentSplitIndex - 1 < 0)
            {
                SendMessage(user.Name + ", You have got no points to spend on undoing your bet yet!");
                return;
            }

            if (!Bets[State.CurrentSplitIndex].ContainsKey(user.Name))
            {
                SendMessage(user.Name + ", You didn't bet for this split yet!");
                return;
            }

            if (Scores[State.CurrentSplitIndex-1].ContainsKey(user.Name) && Scores[State.CurrentSplitIndex - 1][user.Name] < Settings.UnBetPenalty)
            {
                SendMessage(user.Name + ", You need " + Settings.UnBetPenalty + " points to undo your bet and just got " + Scores[State.CurrentSplitIndex - 1][user.Name] + ".");
                return;
            }

            if (State.CurrentSplitIndex >= 0 && Scores[State.CurrentSplitIndex] != null) {
                Scores[State.CurrentSplitIndex][user.Name] -= Settings.UnBetPenalty;
            }
            Bets[State.CurrentSplitIndex].Remove(user.Name);
        }

        private void BetCommands(TwitchChat.User user, string argument)
        {
            var ret = Commands
                .Select(x => "!" + x.Key)
                .Aggregate((a,b) => a + " " + b);

            SendMessage(ret);
        }

        private void Score(TwitchChat.User user, string argument)
        {
            if (!CanBet) return;
            if (State.CurrentPhase == TimerPhase.NotRunning)
            {
                SendMessage("Timer is not running, no score available");
                return;
            }
            if (State.CurrentSplitIndex == 0 || !Scores[State.CurrentSplitIndex - 1].ContainsKey(user.Name))
            {
                SendMessage(user.Name + "'s score is 0");
                return;
            }
            SendMessage(user.Name + "'s score is " + Scores[State.CurrentSplitIndex - 1][user.Name]);
        }

        private void Highscore(TwitchChat.User user, string argument)
        {
            if (!CanBet) return;
            if (State.CurrentPhase == TimerPhase.NotRunning)
            {
                SendMessage("Timer is not running, no score available");
                return;
            }
            if (State.CurrentSplitIndex > 0)
            {
                var orderedScores = Scores[State.CurrentSplitIndex - 1].OrderByDescending(x => x.Value);
                SendMessage(orderedScores.ToList()[0].Key + "'s score is " + orderedScores.ToList()[0].Value);
            }
            else SendMessage("No highscore yet!");
        }

        private void EnableBets(TwitchChat.User user, string argument)
        {
            if (user.Badges.HasFlag(TwitchChat.ChatBadges.Broadcaster))
            {
                if (!CanBet)
                {
                    CanBet = true;
                    SendMessage("SplitsBet enabled !");
                    if (State.CurrentPhase != TimerPhase.NotRunning)
                    {
                        for (int i = 0; i < State.CurrentSplitIndex - 1; i++)
                        {
                            if (null == Scores[i])
                            {
                                Scores[i] = new Dictionary<string, int>(Scores[i - 1]);
                                Bets[i] = new Dictionary<string, Tuple<TimeSpan, double>>();
                            }
                        }
                    }
                }
                else SendMessage("SplitsBet already enabled");
            }
            else SendMessage("You're not allowed to start the bets !");
        }

        private void DisableBets(TwitchChat.User user, string argument)
        {
            if (user.Badges.HasFlag(TwitchChat.ChatBadges.Broadcaster))
            {
                if (CanBet)
                {
                    CanBet = false;
                    SendMessage("SplitsBet disabled !");
                }
                else SendMessage("SplitsBet already disabled");
            }
            else SendMessage("You're not allowed to stop the bets !");
        }

        #endregion

        #region Events

        private void OnMessage(object sender, TwitchChat.Message message)
        {
            if (message.Text.StartsWith("!"))
            {
                try
                {
                    var splits = message.Text.Substring(1).Split(new char[] { ' ' }, 2);
                    if (Commands.ContainsKey(splits[0].ToLower())) {
                        Commands[splits[0].ToLower()].Invoke(message.User, splits.Length > 1 ? splits[1] : "");
                    }
                }
                catch (Exception e) { LogException(e); }
            }
            //else try
            //{
            //    var cmd = Commands["anymessage"];
            //    if (cmd != null) {
            //        cmd.Invoke(message.User, "");
            //    }
            //}
            //catch (Exception e) { Log.Error(e); }
            /* ^ Is this code really useful ? ^ */
        }

        /*The CanBet check might break stuff if you enable the bets in the middle of a run. If someone has a better solution, go for it*/
        private void StartBets(object sender, EventArgs e)
        {
            if (CanBet)
            {
                try
                {
                    SegmentBeginning = State.CurrentTime;
                    Bets[State.CurrentSplitIndex] = new Dictionary<string, Tuple<TimeSpan, double>>();
                    var timeFormatter = new ShortTimeFormatter();
                    var timeFormatted = timeFormatter.Format(GetTime(State.CurrentSplit.BestSegmentTime));
                    SendMessage("Place your bets for " + State.CurrentSplit.Name + "! Best segment for this split is " + timeFormatted + (Settings.UseGlobalTime?" but remember that global time is used !":""));
                }
                catch (Exception ex) { LogException(ex); }
            }
        }

        private void CalculateScore(object sender, EventArgs e)
        {
            if (CanBet)
            {
                try
                {
                    var segment = State.CurrentTime - SegmentBeginning;
                    var timeFormatter = new ShortTimeFormatter();
                    SendMessage("Time for this split was " + timeFormatter.Format(GetTime(segment)));
                    Scores[State.CurrentSplitIndex - 1] = Scores[State.CurrentSplitIndex - 1] ?? (State.CurrentSplitIndex > 1 ? new Dictionary<string, int>(Scores[State.CurrentSplitIndex - 2]) : new Dictionary<string, int>());
                    foreach (KeyValuePair<string, Tuple<TimeSpan, double>> entry in Bets[State.CurrentSplitIndex - 1])
                    {
                        if (Scores[State.CurrentSplitIndex - 1].ContainsKey(entry.Key))
                        {
                            Scores[State.CurrentSplitIndex - 1][entry.Key] += (int)(entry.Value.Item2 * (int)GetTime(segment).Value.TotalSeconds * Math.Exp(-(Math.Pow((int)GetTime(segment).Value.TotalSeconds - (int)entry.Value.Item1.TotalSeconds, 2) / (int)GetTime(segment).Value.TotalSeconds)));
                        }
                        else Scores[State.CurrentSplitIndex - 1].Add(entry.Key, (int)(entry.Value.Item2 * (int)GetTime(segment).Value.TotalSeconds * Math.Exp(-(Math.Pow((int)GetTime(segment).Value.TotalSeconds - (int)entry.Value.Item1.TotalSeconds, 2) / (int)GetTime(segment).Value.TotalSeconds))));
                    }
                    Scores[State.CurrentSplitIndex] = new Dictionary<string, int>(Scores[State.CurrentSplitIndex - 1]);
                    ShowScore();
                    StartBets(sender, e);
                }
                catch (Exception ex) { LogException(ex); }
            }
        }

        private void ShowScore()
        {
            if (CanBet)
            {
                try
                {
                    var orderedScores = Scores[State.CurrentSplitIndex - 1].OrderByDescending(x => x.Value);
                    int scoresShown = 0;
                    SendMessage("Top " + ((Settings.NbScores > orderedScores.Count()) ? orderedScores.Count() : Settings.NbScores));
                    foreach (var entry in orderedScores)
                    {
                        var delta = 0;
                        if (State.CurrentSplitIndex - 2 >= 0 && Scores[State.CurrentSplitIndex - 2].ContainsKey(entry.Key))
                        {
                            delta = entry.Value - Scores[State.CurrentSplitIndex - 2][entry.Key];
                        }
                        SendMessage(entry.Key + ": " + entry.Value + (delta != 0 ? (" (" + (delta < 0 ? "-" : "+") + delta + ")") : ""));
                        if (++scoresShown >= Settings.NbScores) break;
                    }
                }
                catch (Exception ex) { LogException(ex); }
            }
        }

        private void RollbackScore(object sender, EventArgs e)
        {
            try { 
                Scores[State.CurrentSplitIndex].Clear(); 
            } 
            catch (Exception ex) { 
                LogException(ex); 
            };
        }

        private void CopyScore(object sender, EventArgs e)
        {
            if (CanBet)
            {
                try
                {
                    Scores[State.CurrentSplitIndex - 1] = new Dictionary<string, int>(Scores[State.CurrentSplitIndex - 2]);
                    Bets[State.CurrentSplitIndex] = new Dictionary<string, Tuple<TimeSpan, double>>();
                }
                catch (Exception ex) { LogException(ex); }
            }
        }

        private void ResetSplitsBet()
        {
            try
            {
                Array.Clear(Bets, 0, Bets.Length);
                Array.Clear(Scores, 0, Scores.Length);
            }
            catch (Exception ex) { LogException(ex); }
        }

        void State_OnReset(object sender, TimerPhase value)
        {
            if(CanBet) try { ResetSplitsBet(); } catch (Exception ex) { LogException(ex); }
        }

        private TimeSpan? GetTime(Time segment) {
            return segment[Settings.OverridenTimingMethod ?? State.CurrentTimingMethod];
        }

        private void LogException(Exception e) {
            Log.Error(e);
            SendMessage("An error occured - please look into the system event log for details.");
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
        }

        public override void Dispose()
        {
            Twitch.Instance.Chat.OnMessage -= OnMessage;
        }

        #endregion
    }
}
