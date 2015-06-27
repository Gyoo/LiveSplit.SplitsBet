using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LiveSplit.Model;
using System.Xml;
using LiveSplit.TimeFormatters;
using LiveSplit.Model.Comparisons;

namespace LiveSplit.SplitsBet
{
    public partial class Settings : UserControl
    {
        public String Path { get; set; }
        public LiveSplitState LivesplitState { get; set; }

        public bool CanUnBet { get; set; }
        public int UnBetPenalty { get; set; }
        public TimeSpan MinimumTime { get; set; }
        public int NbScores { get; set; }
        public TimingMethod? OverridenTimingMethod { get { return ParseTimingMethod(Method); } }
        public String Method { get; set; }
        public bool UseGlobalTime { get; set; }
        public bool AllowMods { get; set; }
        public bool SingleLineScores { get; set; }
        public string TimeToShow { get; set; }
        public int Delay { get; set; }
        public bool ParentSubSplits { get; set; }
        public string msgEnable { get; set; }
        public string msgDisable { get; set; }
        public string msgReset { get; set; }
        public string msgTimerNotRunning { get; set; }
        public string msgTimerPaused { get; set; }
        public string msgTimerEnded { get; set; }
        public string msgNoScore { get; set; }
        public string msgNoHighscore { get; set; }
        public string msgNoUnbet { get; set; }
        public string msgUnbetTimerEnd { get; set; }
        public string msgCheckTimerEnd { get; set; }
        public string msgTooLateToBet { get; set; }

        private ITimeFormatter Formatter { get; set; }

        public Settings()
        {
            //Init setting to default values
            InitializeComponent();
            CanUnBet = true;
            UnBetPenalty = 50;
            MinimumTime = new TimeSpan(0, 0, 1);
            NbScores = 5;
            UseGlobalTime = false;
            Method = "Current Timing Method";
            AllowMods = false;
            SingleLineScores = false;
            TimeToShow = "Best Segments";
            Delay = 0;
            ParentSubSplits = false;

            //Init messages to default values
            msgEnable = "SplitsBet enabled!";
            msgDisable = "SplitsBet disabled!";
            msgReset = "Run is kill. RIP :(";
            msgTimerNotRunning = "Timer is not running; bets are closed;";
            msgTimerPaused = "Timer is paused; bets are paused too.";
            msgTimerEnded = "Run is over; there is nothing to bet on!";
            msgNoScore = "Timer is not running; no score available.";
            msgNoHighscore = "No highscore yet!";
            msgNoUnbet = "You can't unbet :(";
            msgUnbetTimerEnd = "The run has ended, you can't unbet!";
            msgCheckTimerEnd = "The run has ended; nothing to check!";
            msgTooLateToBet = "Too late to bet for this split; wait for the next one!";

            Formatter = new RegularTimeFormatter();

            //DataBindings for Settings
            chkCancelBets.DataBindings.Add("Checked", this, "CanUnBet", false, DataSourceUpdateMode.OnPropertyChanged);
            txtCancelingPenalty.DataBindings.Add("Text", this, "UnBetPenalty", false, DataSourceUpdateMode.OnPropertyChanged);
            numScores.DataBindings.Add("Value", this, "NbScores", false, DataSourceUpdateMode.OnPropertyChanged);
            chkGlobalTime.DataBindings.Add("Checked", this, "UseGlobalTime", false, DataSourceUpdateMode.OnPropertyChanged);
            cmbTimingMethod.DataBindings.Add("SelectedItem", this, "OverridenTimingMethod", false, DataSourceUpdateMode.OnPropertyChanged);
            chkAllowMods.DataBindings.Add("Checked", this, "AllowMods", false, DataSourceUpdateMode.OnPropertyChanged);
            chkSingleLineScores.DataBindings.Add("Checked", this, "SingleLineScores", false, DataSourceUpdateMode.OnPropertyChanged);
            cmbTimeToShow.DataBindings.Add("SelectedItem", this, "TimeToShow", false, DataSourceUpdateMode.OnPropertyChanged);
            txtDelay.DataBindings.Add("Text", this, "Delay", false, DataSourceUpdateMode.OnPropertyChanged);
            chkSubsplits.DataBindings.Add("Checked", this, "ParentSubSplits", false, DataSourceUpdateMode.OnPropertyChanged);

            //DataBindings for Bot Messages
            txtMsgEnable.DataBindings.Add("Text", this, "msgEnable", false, DataSourceUpdateMode.OnPropertyChanged);
            txtMsgDisable.DataBindings.Add("Text", this, "msgDisable", false, DataSourceUpdateMode.OnPropertyChanged);
            txtMsgReset.DataBindings.Add("Text", this, "msgReset", false, DataSourceUpdateMode.OnPropertyChanged);
            txtMsgTimerNotRunning.DataBindings.Add("Text", this, "msgTimerNotRunning", false, DataSourceUpdateMode.OnPropertyChanged);
            txtMsgTimerPaused.DataBindings.Add("Text", this, "msgTimerPaused", false, DataSourceUpdateMode.OnPropertyChanged);
            txtMsgTimerEnded.DataBindings.Add("Text", this, "msgTimerEnded", false, DataSourceUpdateMode.OnPropertyChanged);
            txtMsgNoScore.DataBindings.Add("Text", this, "msgNoScore", false, DataSourceUpdateMode.OnPropertyChanged);
            txtMsgNoHighscore.DataBindings.Add("Text", this, "msgNoHighscore", false, DataSourceUpdateMode.OnPropertyChanged);
            txtMsgNoUnbet.DataBindings.Add("Text", this, "msgNoUnbet", false, DataSourceUpdateMode.OnPropertyChanged);
            txtMsgUnbetTimerEnd.DataBindings.Add("Text", this, "msgUnbetTimerEnd", false, DataSourceUpdateMode.OnPropertyChanged);
            txtMsgCheckTimerEnd.DataBindings.Add("Text", this, "msgCheckTimerEnd", false, DataSourceUpdateMode.OnPropertyChanged);
            txtMsgTooLateToBet.DataBindings.Add("Text", this, "msgTooLateToBet", false, DataSourceUpdateMode.OnPropertyChanged);

            this.Load += Settings_Load;
        }

        void Settings_Load(object sender, EventArgs e)
        {
            //Settings
            txtMinBetTime.Text = Formatter.Format(MinimumTime);
            cmbTimeToShow.Items.Clear();
            cmbTimeToShow.Items.AddRange(LivesplitState.Run.Comparisons.Where(x => x != BestSplitTimesComparisonGenerator.ComparisonName).ToArray());
            if (!cmbTimeToShow.Items.Contains(TimeToShow))
                cmbTimeToShow.Items.Add(TimeToShow);
            if (LivesplitState.Layout.Components.Any(x => x.ComponentName == "Subsplits"))
                chkSubsplits.Enabled = true;
            else
            {
                chkSubsplits.Enabled = false;
                ParentSubSplits = false;
            }

            // Bot Messages
            txtMsgEnable.Text = msgEnable;
            txtMsgDisable.Text = msgDisable;
            txtMsgReset.Text = msgReset;
            txtMsgTimerEnded.Text = msgTimerEnded;
            txtMsgTimerNotRunning.Text = msgTimerNotRunning;
            txtMsgTimerPaused.Text = msgTimerPaused;
            txtMsgNoScore.Text = msgNoScore;
            txtMsgNoHighscore.Text = msgNoHighscore;
            txtMsgNoUnbet.Text = msgNoUnbet;
            txtMsgUnbetTimerEnd.Text = msgUnbetTimerEnd;
            txtMsgCheckTimerEnd.Text = msgCheckTimerEnd;
            txtMsgTooLateToBet.Text = msgTooLateToBet;
        }

        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            var settingsNode = document.CreateElement("Settings");
            settingsNode.AppendChild(ToElement(document, "Version", SplitsBetFactory.VersionString));

            //Settings
            settingsNode.AppendChild(ToElement(document, "UnBet", CanUnBet));
            settingsNode.AppendChild(ToElement(document, "UnBetPenalty", UnBetPenalty));
            settingsNode.AppendChild(ToElement(document, "MinimumTime", MinimumTime));
            settingsNode.AppendChild(ToElement(document, "NbScores", NbScores));
            settingsNode.AppendChild(ToElement(document, "UseGlobalTime", UseGlobalTime));
            settingsNode.AppendChild(ToElement(document, "TimingMethod", Method));
            settingsNode.AppendChild(ToElement(document, "AllowMods", AllowMods));
            settingsNode.AppendChild(ToElement(document, "SingleLineScores", SingleLineScores));
            settingsNode.AppendChild(ToElement(document, "TimeToShow", TimeToShow));
            settingsNode.AppendChild(ToElement(document, "Delay", Delay));
            settingsNode.AppendChild(ToElement(document, "ParentSubSplits", ParentSubSplits));

            //Bot Messages
            settingsNode.AppendChild(ToElement(document, "msgEnable", msgEnable));
            settingsNode.AppendChild(ToElement(document, "msgDisable", msgDisable));
            settingsNode.AppendChild(ToElement(document, "msgReset", msgReset));
            settingsNode.AppendChild(ToElement(document, "msgTimerEnded", msgTimerEnded));
            settingsNode.AppendChild(ToElement(document, "msgTimerNotRunning", msgTimerNotRunning));
            settingsNode.AppendChild(ToElement(document, "msgTimerPaused", msgTimerPaused));
            settingsNode.AppendChild(ToElement(document, "msgNoScore", msgNoScore));
            settingsNode.AppendChild(ToElement(document, "msgNoHighscore", msgNoHighscore));
            settingsNode.AppendChild(ToElement(document, "msgNoUnbet", msgNoUnbet));
            settingsNode.AppendChild(ToElement(document, "msgUnbetTimerEnd", msgUnbetTimerEnd));
            settingsNode.AppendChild(ToElement(document, "msgCheckTimerEnd", msgCheckTimerEnd));
            settingsNode.AppendChild(ToElement(document, "msgTooLateToBet", msgTooLateToBet));
            return settingsNode;
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            //Settings
            if(settings["UnBet"] != null) CanUnBet = bool.Parse(settings["UnBet"].InnerText);
            else CanUnBet = true;
            if(settings["UnBetPenalty"] != null) UnBetPenalty = int.Parse(settings["UnBetPenalty"].InnerText);
            else UnBetPenalty = 50;
            if(settings["MinimumTime"] != null) MinimumTime = TimeSpanParser.Parse(settings["MinimumTime"].InnerText);
            else MinimumTime = new TimeSpan(0, 0, 1);
            if(settings["NbScores"] != null) NbScores = int.Parse(settings["NbScores"].InnerText);
            else NbScores = 5;
            if(settings["UseGlobalTime"] != null) UseGlobalTime = bool.Parse(settings["UseGlobalTime"].InnerText);
            else UseGlobalTime = false;
            if(settings["TimingMethod"] != null) Method = settings["TimingMethod"].InnerText;
            else Method = "Current Timing Method";
            if(settings["AllowMods"] != null) AllowMods = bool.Parse(settings["AllowMods"].InnerText);
            else AllowMods = false;
            if(settings["SingleLineScores"] != null) SingleLineScores = bool.Parse(settings["SingleLineScores"].InnerText);
            else SingleLineScores = false;
            if(settings["TimeToShow"] != null) TimeToShow = settings["TimeToShow"].InnerText;
            else TimeToShow = "Best Segments";
            if (settings["Delay"] != null) Delay = int.Parse(settings["Delay"].InnerText);
            else Delay = 0;
            if (settings["ParentSubSplits"] != null) ParentSubSplits = bool.Parse(settings["ParentSubSplits"].InnerText);
            else ParentSubSplits = false;

            //Bot messages
            if (settings["msgEnable"] != null) msgEnable = settings["msgEnable"].InnerText;
            else msgEnable = "SplitsBet enabled!";
            if (settings["msgDisable"] != null) msgDisable = settings["msgDisable"].InnerText;
            else msgDisable = "SplitsBet disabled!";
            if (settings["msgReset"] != null) msgReset = settings["msgReset"].InnerText;
            else msgReset = "Run is kill. RIP :(";
            if (settings["msgTimerEnded"] != null) msgTimerEnded = settings["msgTimerEnded"].InnerText;
            else msgTimerEnded = "Run is over; there is nothing to bet on!";
            if (settings["msgTimerNotRunning"] != null) msgTimerNotRunning = settings["msgTimerNotRunning"].InnerText;
            else msgTimerNotRunning = "Timer is not running; bets are closed;";
            if (settings["msgTimerPaused"] != null) msgTimerPaused = settings["msgTimerPaused"].InnerText;
            else msgTimerPaused = "Timer is paused; bets are paused too.";
            if (settings["msgNoScore"] != null) msgNoScore = settings["msgNoScore"].InnerText;
            else msgNoScore = "Timer is not running; no score available.";
            if (settings["msgNoHighscore"] != null) msgNoHighscore = settings["msgNoHighscore"].InnerText;
            else msgNoHighscore = "No highscore yet!";
            if (settings["msgNoUnbet"] != null) msgNoUnbet = settings["msgNoUnbet"].InnerText;
            else msgNoUnbet = "You can't unbet :(";
            if (settings["msgUnbetTimerEnd"] != null) msgUnbetTimerEnd = settings["msgUnbetTimerEnd"].InnerText;
            else msgUnbetTimerEnd = "The run has ended, you can't unbet!";
            if (settings["msgCheckTimerEnd"] != null) msgCheckTimerEnd = settings["msgCheckTimerEnd"].InnerText;
            else msgCheckTimerEnd = "The run has ended; nothing to check!";
            if (settings["msgTooLateToBet"] != null) msgTooLateToBet = settings["msgTooLateToBet"].InnerText;
            else msgTooLateToBet = "Too late to bet for this split; wait for the next one!";
        }

        private TimingMethod? ParseTimingMethod(String method)
        {
            TimingMethod? timingMethod = null;
            if (method == "Real Time")
                timingMethod = TimingMethod.RealTime;
            else if (method == "Game Time")
                timingMethod = TimingMethod.GameTime;
            return timingMethod;
        }

        private XmlElement ToElement<T>(XmlDocument document, String name, T value)
        {
            var element = document.CreateElement(name);
            element.InnerText = value.ToString();
            return element;
        }

        private void txtCancelingPenalty_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar) && !Char.IsControl(e.KeyChar))
                e.Handled = true;
        }

        private void txtMinBetTime_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar) && !Char.IsControl(e.KeyChar) && !e.KeyChar.Equals(':'))
                e.Handled = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            txtCancelingPenalty.Enabled = chkCancelBets.Checked;
        }

        private void txtMinBetTime_Leave(object sender, EventArgs e)
        {
            try
            {
                MinimumTime = TimeSpanParser.Parse(txtMinBetTime.Text);
            }
            finally
            {
                txtMinBetTime.Text = Formatter.Format(MinimumTime);
            }
        }

        private void txtDelay_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar) && !Char.IsControl(e.KeyChar))
                e.Handled = true;
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }
    }
}
