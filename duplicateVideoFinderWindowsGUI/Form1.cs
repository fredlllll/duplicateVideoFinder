using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using duplicateVideoFinder;

namespace duplicateVideoFinderWindowsGUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            this.InitializeComponent();
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            DirectoryInfo di = new DirectoryInfo(txtDirectory.Text);

            IDuplicateFinder finder = new HashDuplicateFinder();
            finder.OnProgress += Finder_OnProgress;

            var t = await Task.Factory.StartNew(() =>
            {
                Console.WriteLine("what the fuck");
                return finder.FindDuplicates(di);
            });

            txtResult.Text = "";
            foreach (var dupes in t)
            {
                txtResult.AppendText("-----\n");
                foreach (var fi in dupes)
                {
                    txtResult.AppendText(fi.FullName + "\n");
                }
            }

            btnStart.Enabled = true;
        }

        DateTime lastProgress = DateTime.Now;
        private void Finder_OnProgress(duplicateVideoFinder.Progresses.IProgress progress)
        {
            if ((DateTime.Now - lastProgress).TotalSeconds > 1)
            {
                this.BeginInvoke(new Action(() =>
                {
                    this.Text = "Progress: " + progress.ToString();
                }));
                lastProgress = DateTime.Now;
            }
        }
    }
}
