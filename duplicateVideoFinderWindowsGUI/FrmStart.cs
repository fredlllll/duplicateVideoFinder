using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using duplicateVideoFinder;
using duplicateVideoFinder.MetricGenerators;

namespace duplicateVideoFinderWindowsGUI
{
    public partial class FrmStart : Form
    {
        FrmSelectFilesToKeep nextForm;

        public FrmStart()
        {
            this.InitializeComponent();
            nextForm = new FrmSelectFilesToKeep();
            nextForm.FormClosed += NextForm_FormClosed;
        }

        private void NextForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Close(); //so the program ends when we close the other form
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            DirectoryInfo di = new DirectoryInfo(txtDirectory.Text);

            var gens = new List<AMetricGenerator>();
            //TODO: maybe make this generic with reflection?
            if (chkHash.Checked)
            {
                gens.Add(new HashMetricGenerator());
            }
            if (chkDuration.Checked)
            {
                gens.Add(new DurationMetricGenerator());
            }
            if (chkThumb.Checked)
            {
                //gens.Add(new ThumbMetricGenerator());
            }

            IDuplicateFinder finder = new DuplicateFinder(gens.ToArray());
            finder.OnProgress += Finder_OnProgress;

            var dupes = await Task.Factory.StartNew(() =>
            {
                Console.WriteLine("what the fuck");
                return finder.FindDuplicates(di, chkTopDir.Checked);
            });

            this.Hide();
            nextForm.Show();
            nextForm.SetDuplicates(dupes);


            btnStart.Enabled = true;
        }

        DateTime lastProgress = DateTime.Now;
        private void Finder_OnProgress(duplicateVideoFinder.Progresses.IProgress progress)
        {
            if ((DateTime.Now - lastProgress).TotalSeconds > 0.25 || progress is duplicateVideoFinder.Progresses.BasicProgress)
            {
                this.BeginInvoke(new Action(() =>
                {
                    this.Text = "Progress: " + progress.ToString();
                    this.progressBar1.Value = (int)(100 * progress.Progress);
                }));
                lastProgress = DateTime.Now;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                txtDirectory.Text = fbd.SelectedPath;
            }
        }
    }
}
