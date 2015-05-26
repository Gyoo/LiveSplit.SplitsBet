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

            /*Adding available commands*/
            Commands.Add("bet", Bet);
            Commands.Add("checkbet", CheckBet);
            Commands.Add("betcommands", BetCommands);
            Commands.Add("score", Score);
            Commands.Add("highscore", Highscore);

            /*Setting Livesplit events*/
            State.OnStart += new EventHandler(StartBets);
            State.OnSplit += new EventHandler(CalculateScore);
            State.OnUndoSplit += new EventHandler(RollbackScore);
            State.OnSkipSplit += new EventHandler(CopyScore);
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
            
            //TODO Check if bot is already connected (In case SplitsBet is added, removed and readded)
            Twitch.Instance.ConnectToChat(Twitch.Instance.ChannelName);
            Twitch.Instance.Chat.OnMessage += OnMessage;
        }

        #endregion

        #region Commands

        private void Bet(TwitchChat.User user, string argument)
        {
            //TODO Manage Game Time
            if (State.CurrentPhase == TimerPhase.Running)
            {
                double Percentage = (State.CurrentTime - SegmentBeginning).RealTime.Value.TotalSeconds / State.CurrentSplit.BestSegmentTime.RealTime.Value.TotalSeconds;
                if (Percentage < 0.9)
                {
                    if (!Bets[State.CurrentSplitIndex].ContainsKey(user.Name))
                    {
                        try
                        {
                            //TODO !bet 0:00 should be refused
                            var time = TimeSpanParser.Parse(argument);
                            var t = new Tuple<TimeSpan, double>(time, Math.Exp(-2 * Math.Pow(Percentage, 2)));
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

        private void BetCommands(TwitchChat.User user, string argument)
        {
            string ret = "";
            foreach (KeyValuePair<string, Action<TwitchChat.User, string>> entry in Commands)
            {
                ret += "!" + entry.Key + " ";
            }
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
                    Commands[splits[0].ToLower()].Invoke(message.User, splits.Length > 1 ? splits[1] : "");
                }
                catch { }
            }
            try
            {
                Commands["anymessage"].Invoke(message.User, "");
            }
            catch { }
        }

        private void StartBets(object sender, EventArgs e)
        {
            SegmentBeginning = State.CurrentTime;
            Bets[State.CurrentSplitIndex] = new Dictionary<string, Tuple<TimeSpan, double>>();
            var timeFormatter = new ShortTimeFormatter();
            var timeFormatted = timeFormatter.Format(State.CurrentSplit.BestSegmentTime.RealTime);
            Twitch.Instance.Chat.SendMessage("/me Place your bets for " + State.CurrentSplit.Name + "! Best segment for this split is " + timeFormatted);
        }

        private void CalculateScore(object sender, EventArgs e)
        {
            Time Segment = State.CurrentTime - SegmentBeginning;
            Scores[State.CurrentSplitIndex - 1] = State.CurrentSplitIndex > 1 ? Scores[State.CurrentSplitIndex - 2] : new Dictionary<string, int>();
            foreach (KeyValuePair<string, Tuple<TimeSpan, double>> entry in Bets[State.CurrentSplitIndex - 1])
            {
                if (Scores[State.CurrentSplitIndex - 1].ContainsKey(entry.Key))
                {
                    Scores[State.CurrentSplitIndex - 1][entry.Key] += (int)(entry.Value.Item2 * (int)Segment.RealTime.Value.TotalSeconds * Math.Exp(-(Math.Pow((int)Segment.RealTime.Value.TotalSeconds - (int)entry.Value.Item1.TotalSeconds, 2) / (int)Segment.RealTime.Value.TotalSeconds)));
                }
                else Scores[State.CurrentSplitIndex - 1].Add(entry.Key, (int)(entry.Value.Item2 * (int)Segment.RealTime.Value.TotalSeconds * Math.Exp(-(Math.Pow((int)Segment.RealTime.Value.TotalSeconds - (int)entry.Value.Item1.TotalSeconds, 2) / (int)Segment.RealTime.Value.TotalSeconds))));
            }
            StartBets(sender, e);
        }

        private void ShowScore()
        {
            var orderedScores = Scores[State.CurrentSplitIndex - 1].OrderByDescending(x => x.Value);

            foreach (var entry in orderedScores)
            {
                Twitch.Instance.Chat.SendMessage("/me " + entry.Key + ": " + entry.Value);
                //TODO Show Delta score as well (Ex : "Gyoo : 420 (+42)" )
            }
        }

        private void RollbackScore(object sender, EventArgs e)
        {
            Scores[State.CurrentSplitIndex].Clear();
        }

        private void CopyScore(object sender, EventArgs e)
        {
            Scores[State.CurrentSplitIndex - 1] = Scores[State.CurrentSplitIndex - 2];
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
