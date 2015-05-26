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
        private TimeSpan MinimumTime { get; set; }
        private int UnBetPenalty { get; set; }
        private TimingMethod? OverridenTimingMethod { get; set; }
       
        public override string ComponentName
        {
            get { return "Splits Bet Bot"; }
        }

        #endregion

        #region Constructors

        public SplitsBetComponent(LiveSplitState state)
        {
            /*Initializing variables*/
            Settings = new Settings();
            Commands = new Dictionary<string, Action<TwitchChat.User, string>>();
            State = state;
            SegmentBeginning = new Time();
            Bets = new Dictionary<string, Tuple<TimeSpan, double>>[State.Run.Count];
            Scores = new Dictionary<string, int>[State.Run.Count];
            MinimumTime = new TimeSpan(0, 0, 0);//TODO get the minimum time from the settings
            UnBetPenalty = 50;//TODO get the penalty from the settings
            OverridenTimingMethod = null;//TODO get the timing method from the settings

            /*Adding available commands*/
            Commands.Add("bet", Bet);
            Commands.Add("checkbet", CheckBet);
            Commands.Add("unbet", UnBet);
            Commands.Add("betcommands", BetCommands);
            Commands.Add("score", Score);
            Commands.Add("highscore", Highscore);

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

            if (!Twitch.Instance.Chat.OnMessage.GetInvocationList().Contains((Action<object, TwitchChat.Message>) OnMessage))
            {
                Twitch.Instance.Chat.OnMessage += OnMessage;
            }
        }

        #endregion

        #region Commands

        private void Bet(TwitchChat.User user, string argument)
        {
            //TODO Manage Game Time
            if (State.CurrentPhase == TimerPhase.Running)
            {
                var percentage = GetTime((State.CurrentTime - SegmentBeginning)).Value.TotalSeconds / GetTime(State.CurrentSplit.BestSegmentTime).Value.TotalSeconds;
                if (percentage < 0.9)
                {
                    if (!Bets[State.CurrentSplitIndex].ContainsKey(user.Name))
                    {
                        try
                        {
                            if (argument.ToLower().Contains("Kappa"))
                            {
                                argument = "420:69";
                                Twitch.Instance.Chat.SendMessage("/me " + user.Name + " bet 420:69 Kappa");
                            }

                            var time = TimeSpanParser.Parse(argument);
                            if (time.CompareTo(MinimumTime) <= 0) {
                                Twitch.Instance.Chat.SendMessage("/me " + user.Name + ", Nice try, but it's invalid");
                            }
                            var t = new Tuple<TimeSpan, double>(time, Math.Exp(-2 * Math.Pow(percentage, 2)));
                            Bets[State.CurrentSplitIndex].Add(user.Name, t);
                        }
                        catch
                        {
                            Twitch.Instance.Chat.SendMessage("/me " + user.Name + ", Invalid time, please retry");
                        }
                    }
                    else Twitch.Instance.Chat.SendMessage("/me " + user.Name + ", You already bet, silly!");
                }
                else Twitch.Instance.Chat.SendMessage("/me Too late to bet for this split, wait for the next one!");
            }
            else Twitch.Instance.Chat.SendMessage("/me Timer is not running, bets are closed");
        }

        private void CheckBet(TwitchChat.User user, string argument)
        {
            if (State.CurrentPhase == TimerPhase.Running)
            {
                if (Bets[State.CurrentSplitIndex].ContainsKey(user.Name))
                    Twitch.Instance.Chat.SendMessage("/me " + user.Name + ", Your bet for " + State.CurrentSplit.Name + " is " + Bets[State.CurrentSplitIndex][user.Name].Item1);
                else
                    Twitch.Instance.Chat.SendMessage("/me " + user.Name + ", You didn't bet for this split yet!");
            }
            else Twitch.Instance.Chat.SendMessage("/me Timer is not running, bets are closed");
        }

        private void UnBet(TwitchChat.User user, string argument)
        {
            //TODO check if the runner allows undoing bets

            if (State.CurrentPhase != TimerPhase.Running) {
                Twitch.Instance.Chat.SendMessage("/me Timer is not running, bets are closed");
                return;
            }

            if (State.CurrentSplitIndex - 1 < 0) {
                //TODO make the runner decide what to do here
                /*
                Twitch.Instance.Chat.SendMessage("/me " + user.Name + ", You have got no points to spend on undoing your bet yet!");
                return;
                */
            }

            if (!Bets[State.CurrentSplitIndex].ContainsKey(user.Name)) {
                Twitch.Instance.Chat.SendMessage("/me " + user.Name + ", You didn't bet for this split yet!");
                return;
            }

            if (Scores[State.CurrentSplitIndex - 1][user.Name] < UnBetPenalty) {
                Twitch.Instance.Chat.SendMessage("/me " + user.Name + ", You need " + UnBetPenalty + " points to undo your bet and just got " + Scores[State.CurrentSplitIndex - 1][user.Name] + ".");
                return;
            }

            Scores[State.CurrentSplitIndex - 1][user.Name] -= UnBetPenalty;
            Bets[State.CurrentSplitIndex].Remove(user.Name);
        }

        private void BetCommands(TwitchChat.User user, string argument)
        {
            var ret = Commands
                .Select(x => "!" + x.Key)
                .Aggregate((a,b) => a + " " + b);

            Twitch.Instance.Chat.SendMessage("/me " + ret);
        }

        private void Score(TwitchChat.User user, string argument)
        {
            if (State.CurrentPhase == TimerPhase.Running)
            {
                Twitch.Instance.Chat.SendMessage("/me " + user.Name + "'s score is " + Scores[State.CurrentSplitIndex - 1][user.Name]);
            }
            else Twitch.Instance.Chat.SendMessage("/me Timer is not running, no score available");
        }

        private void Highscore(TwitchChat.User user, string argument)
        {
            if (State.CurrentPhase == TimerPhase.Running)
            {
                if (State.CurrentSplitIndex > 0)
                {
                    var orderedScores = Scores[State.CurrentSplitIndex - 1].OrderByDescending(x => x.Value);
                    Twitch.Instance.Chat.SendMessage("/me " + orderedScores.ToList()[0].Key + "'s score is " + orderedScores.ToList()[0].Value);
                }
                else Twitch.Instance.Chat.SendMessage("/me No highscore yet!");
            }
            else Twitch.Instance.Chat.SendMessage("/me Timer is not running, no score available");
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
                    var cmd = Commands[splits[0].ToLower()];
                    if (cmd != null) {
                        cmd.Invoke(message.User, splits.Length > 1 ? splits[1] : "");
                    }
                }
                catch { }
            }
            try
            {
                var cmd = Commands["anymessage"];
                if (cmd != null) {
                    cmd.Invoke(message.User, "");
                }
            }
            catch { }
        }

        private void StartBets(object sender, EventArgs e)
        {
            SegmentBeginning = State.CurrentTime;
            Bets[State.CurrentSplitIndex] = new Dictionary<string, Tuple<TimeSpan, double>>();
            var timeFormatter = new ShortTimeFormatter();
            var timeFormatted = timeFormatter.Format(GetTime(State.CurrentSplit.BestSegmentTime));
            Twitch.Instance.Chat.SendMessage("/me Place your bets for " + State.CurrentSplit.Name + "! Best segment for this split is " + timeFormatted);
        }

        private void CalculateScore(object sender, EventArgs e)
        {
            var segment = State.CurrentTime - SegmentBeginning;
            var timeFormatter = new ShortTimeFormatter();
            Twitch.Instance.Chat.SendMessage("/me Time for this split was " + timeFormatter.Format(GetTime(segment)));
            Scores[State.CurrentSplitIndex - 1] = State.CurrentSplitIndex > 1 ? new Dictionary<string, int>(Scores[State.CurrentSplitIndex - 2]) : new Dictionary<string, int>();
            foreach (KeyValuePair<string, Tuple<TimeSpan, double>> entry in Bets[State.CurrentSplitIndex - 1])
            {
                if (Scores[State.CurrentSplitIndex - 1].ContainsKey(entry.Key))
                {
                    Scores[State.CurrentSplitIndex - 1][entry.Key] += (int)(entry.Value.Item2 * (int)GetTime(segment).Value.TotalSeconds * Math.Exp(-(Math.Pow((int)GetTime(segment).Value.TotalSeconds - (int)entry.Value.Item1.TotalSeconds, 2) / (int)GetTime(segment).Value.TotalSeconds)));
                }
                else Scores[State.CurrentSplitIndex - 1].Add(entry.Key, (int)(entry.Value.Item2 * (int)GetTime(segment).Value.TotalSeconds * Math.Exp(-(Math.Pow((int)GetTime(segment).Value.TotalSeconds - (int)entry.Value.Item1.TotalSeconds, 2) / (int)GetTime(segment).Value.TotalSeconds))));
            }
            StartBets(sender, e);
        }

        private void ShowScore()
        {
            var orderedScores = Scores[State.CurrentSplitIndex - 1].OrderByDescending(x => x.Value);

            foreach (var entry in orderedScores)
            {
                var delta = 0;
                if (State.CurrentSplitIndex - 2 >= 0) {
                    delta = entry.Value - Scores[State.CurrentSplitIndex - 2][entry.Key];
                }
                Twitch.Instance.Chat.SendMessage("/me " + entry.Key + ": " + entry.Value + (delta != 0 ? (" (" + (delta < 0 ? "-" : "+") + delta + ")") : ""));
            }
        }

        private void RollbackScore(object sender, EventArgs e)
        {
            Scores[State.CurrentSplitIndex].Clear();
        }

        private void CopyScore(object sender, EventArgs e)
        {
            Scores[State.CurrentSplitIndex - 1] = new Dictionary<string, int>(Scores[State.CurrentSplitIndex - 2]);
            Bets[State.CurrentSplitIndex] = new Dictionary<string, Tuple<TimeSpan, double>>();
        }

        private void ResetSplitsBet()
        {
            Array.Clear(Bets, 0, Bets.Length);
            Array.Clear(Scores, 0, Scores.Length);
        }

        void State_OnReset(object sender, TimerPhase value)
        {
            ResetSplitsBet();
        }

        private TimeSpan? GetTime(Time segment) {
            return segment[OverridenTimingMethod ?? State.CurrentTimingMethod];
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
        }

        #endregion
    }
}
