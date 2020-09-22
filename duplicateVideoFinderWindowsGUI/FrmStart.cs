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
            //disable inputs
            btnSearch.Enabled = false;
            btnStart.Enabled = false;
            chkHash.Enabled = false;
            chkDuration.Enabled = false;
            chkThumb.Enabled = false;
            chkTopDir.Enabled = false;
            chkDeleteCache.Enabled = false;
            DirectoryInfo di = new DirectoryInfo(txtDirectory.Text);

            var gens = new List<AMetricGenerator>();
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

            IDuplicateFinder finder = new DuplicateFinder(gens.ToArray(), di, chkTopDir.Checked, chkDeleteCache.Checked);
            finder.OnProgress += Finder_OnProgress;

            var dupes = await Task.Factory.StartNew(() =>
            {
                return finder.FindDuplicates();
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
                startButtonEnableCheck();
            }
        }

        private void startButtonEnableCheck()
        {
            int count = 0;

            if (chkHash.Checked)
            {
                count++;
            }
            if (chkDuration.Checked)
            {
                count++;
            }
            if (chkThumb.Checked)
            {
                count++;
            }

            btnStart.Enabled = count > 0;
            if (!btnStart.Enabled)
            {
                return;
            }

            btnStart.Enabled = Directory.Exists(txtDirectory.Text);
        }

        private void genCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            startButtonEnableCheck();
        }

        private void txtDirectory_TextChanged(object sender, EventArgs e)
        {
            startButtonEnableCheck();
        }
    }
}
