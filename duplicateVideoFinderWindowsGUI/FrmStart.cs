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
        bool ffmpegReady;

        public FrmStart()
        {
            this.InitializeComponent();
            chkDuration.Enabled = false; // enabled once FFmpeg is ready
            toolTips.SetToolTip(chkDuration, "Check for duplicates using the video duration (needs FFmpeg, downloaded at startup)");

            nextForm = new FrmSelectFilesToKeep();
            nextForm.FormClosed += NextForm_FormClosed;
        }

        public void SetFFmpegReady(bool ready)
        {
            ffmpegReady = ready;
            chkDuration.Enabled = ready;
            startButtonEnableCheck();
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

            IDuplicateFinder finder = new DuplicateFinder(gens.ToArray(), di, chkTopDir.Checked, chkDeleteCache.Checked);
            finder.OnProgress += Finder_OnProgress;

            DuplicateFinderResult dupes;
            try
            {
                dupes = await Task.Factory.StartNew(() =>
                {
                    return finder.FindDuplicates();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Scan failed: " + ex.Message);
                //re-enable inputs so the user can try again
                btnSearch.Enabled = true;
                chkHash.Enabled = true;
                chkDuration.Enabled = ffmpegReady;
                chkTopDir.Enabled = true;
                chkDeleteCache.Enabled = true;
                startButtonEnableCheck();
                return;
            }

            this.Hide();
            nextForm.Show();
            nextForm.SetDuplicates(dupes);
        }

        private readonly object progressLock = new object();
        DateTime lastProgress = DateTime.Now;
        private void Finder_OnProgress(duplicateVideoFinder.Progresses.IProgress progress)
        {
            bool shouldUpdate;
            lock (progressLock)
            {
                shouldUpdate = progress is duplicateVideoFinder.Progresses.BasicProgress || (DateTime.Now - lastProgress).TotalSeconds > 0.25;
                if (shouldUpdate)
                {
                    lastProgress = DateTime.Now;
                }
            }
            if (shouldUpdate)
            {
                try
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        this.Text = "Progress: " + progress.ToString();
                        this.progressBar1.Value = (int)(100 * progress.Progress);
                    }));
                }
                catch (InvalidOperationException)
                {
                    // handle is gone, form is closing
                }
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

            btnStart.Enabled = count > 0 && Directory.Exists(txtDirectory.Text);
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