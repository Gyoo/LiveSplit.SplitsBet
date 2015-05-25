using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LiveSplit.SplitsBet
{
    public partial class Settings : UserControl
    {
        public String Path { get; set; }

        public Settings()
        {
            InitializeComponent();
            txtPath.DataBindings.Add("Text", this, "Path", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void btnPath_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog()
            {
                SelectedPath = Path
            };
            var result = fbd.ShowDialog();
            if (result == DialogResult.OK)
            {
                Path = fbd.SelectedPath;
                txtPath.Text = Path;
            }
        }

        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            var settingsNode = document.CreateElement("Settings");

            var pathNode = document.CreateElement("Path");
            pathNode.InnerText = Path;
            settingsNode.AppendChild(pathNode);

            return settingsNode;
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            Path = settings["Path"].InnerText;
        }
    }
}
