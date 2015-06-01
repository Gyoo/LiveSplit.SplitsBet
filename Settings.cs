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

            chkCancelBets.DataBindings.Add("Checked", this, "CanUnBet", false, DataSourceUpdateMode.OnPropertyChanged);
            txtCancelingPenalty.DataBindings.Add("Text", this, "UnBetPenalty", false, DataSourceUpdateMode.OnPropertyChanged);
            txtMinBetTime.DataBindings.Add("Text", this, "MinimumTime", false, DataSourceUpdateMode.OnPropertyChanged);
            numScores.DataBindings.Add("Value", this, "NbScores", false, DataSourceUpdateMode.OnPropertyChanged);
            chkGlobalTime.DataBindings.Add("Checked", this, "UseGlobalTime", false, DataSourceUpdateMode.OnPropertyChanged);
            cmbTimingMethod.DataBindings.Add("SelectedItem", this, "OverridenTimingMethod", false, DataSourceUpdateMode.OnPropertyChanged);
            chkAllowMods.DataBindings.Add("Checked", this, "AllowMods", false, DataSourceUpdateMode.OnPropertyChanged);
            chkSingleLineScores.DataBindings.Add("Checked", this, "SingleLineScores", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            var settingsNode = document.CreateElement("Settings");
            settingsNode.AppendChild(ToElement(document, "Version", "0.3"));

            settingsNode.AppendChild(ToElement(document, "UnBet", CanUnBet));
            settingsNode.AppendChild(ToElement(document, "UnbetPenalty", UnBetPenalty));
            settingsNode.AppendChild(ToElement(document, "MinimumTime", MinimumTime));
            settingsNode.AppendChild(ToElement(document, "NbScores", NbScores));
            settingsNode.AppendChild(ToElement(document, "UseGlobalTime", UseGlobalTime));
            settingsNode.AppendChild(ToElement(document, "TimingMethod", Method));
            settingsNode.AppendChild(ToElement(document, "AllowMods", AllowMods));
            settingsNode.AppendChild(ToElement(document, "SingleLineScores", SingleLineScores));
            return settingsNode;
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            if (settings["Version"] != null && settings["Version"].InnerText == "0.3")
            {
                CanUnBet = bool.Parse(settings["UnBet"].InnerText);
                UnBetPenalty = int.Parse(settings["UnbetPenalty"].InnerText);
                MinimumTime = TimeSpanParser.Parse(settings["MinimumTime"].InnerText);
                NbScores = int.Parse(settings["NbScores"].InnerText);
                UseGlobalTime = bool.Parse(settings["UseGlobalTime"].InnerText);
                Method = settings["TimingMethod"].InnerText;
                AllowMods = bool.Parse(settings["AllowMods"].InnerText);
                SingleLineScores = bool.Parse(settings["SingleLineScores"].InnerText);
            }
            else
            {
                // Default values
                CanUnBet = true;
                UnBetPenalty = 50;
                MinimumTime = new TimeSpan(0, 0, 1);
                NbScores = 5;
                UseGlobalTime = false;
                Method = "Current Timing Method";
                AllowMods = false;
                SingleLineScores = false;
            }

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

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar) && !Char.IsControl(e.KeyChar))
                e.Handled = true;
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar) && !Char.IsControl(e.KeyChar))
                e.Handled = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            txtCancelingPenalty.Enabled = chkCancelBets.Checked;
        }
    }
}
