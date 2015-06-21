﻿using System;
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

        private ITimeFormatter Formatter { get; set; }

        public Settings()
        {
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

            Formatter = new RegularTimeFormatter();

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

            this.Load += Settings_Load;
        }

        void Settings_Load(object sender, EventArgs e)
        {
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
        }

        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            var settingsNode = document.CreateElement("Settings");
            settingsNode.AppendChild(ToElement(document, "Version", SplitsBetFactory.VersionString));

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
            return settingsNode;
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
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
    }
}
