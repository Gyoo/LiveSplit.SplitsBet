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
        public TimingMethod? OverridenTimingMethod { get; set; }

        public Settings()
        {
            InitializeComponent();
            checkBox1.DataBindings.Add("Checked", this, "CanUnBet", false, DataSourceUpdateMode.OnPropertyChanged);
            textBox1.DataBindings.Add("Text", this, "UnBetPenalty", false, DataSourceUpdateMode.OnPropertyChanged);
            textBox2.DataBindings.Add("Text", this, "MinimumTime", false, DataSourceUpdateMode.OnPropertyChanged);
            numericUpDown1.DataBindings.Add("Value", this, "NbScores", false, DataSourceUpdateMode.OnPropertyChanged);
            //comboBox1.DataBindings.Add("SelectedValue", this, "OverridenTimingMethod", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            var settingsNode = document.CreateElement("Settings");
            settingsNode.AppendChild(ToElement(document, "Version", "0.2"));
            settingsNode.AppendChild(ToElement(document, "UnBet", CanUnBet));
            settingsNode.AppendChild(ToElement(document, "UnbetPenalty", UnBetPenalty));
            settingsNode.AppendChild(ToElement(document, "MinimumTime", MinimumTime));
            settingsNode.AppendChild(ToElement(document, "NbScores", NbScores));
            return settingsNode;
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            CanUnBet = bool.Parse(settings["UnBet"].InnerText);
            UnBetPenalty = int.Parse(settings["UnbetPenalty"].InnerText);
            MinimumTime = TimeSpanParser.Parse(settings["MinimumTime"].InnerText);
            NbScores = int.Parse(settings["NbScores"].InnerText);
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
            textBox1.Enabled = checkBox1.Checked;
        }
    }
}
