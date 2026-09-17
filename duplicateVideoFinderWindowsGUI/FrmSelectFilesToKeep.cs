using duplicateVideoFinder;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace duplicateVideoFinderWindowsGUI
{
    public partial class FrmSelectFilesToKeep : Form
    {
        DuplicateFinderResult dfr;
        readonly Queue<string> pendingGenIds = new Queue<string>();
        string currentGenId = "";
        FileInfo currentKeeper;

        List<DupeFileCollection> CurrentGen
        {
            get
            {
                if (dfr != null && dfr.dupesByGenerator.Count > 0 && !string.IsNullOrEmpty(currentGenId))
                {
                    return dfr.dupesByGenerator[currentGenId];
                }
                return null;
            }
        }

        // decreases each time a new batch starts (SetCurrentDupes or Next),
        // so the UI-thread Invoke can tell that a stale batch is no longer current
        int nextBatchId = 0;

        public FrmSelectFilesToKeep()
        {
            InitializeComponent();
        }

        Bitmap MakeThumb(FileInfo fi)
        {
            try
            {
                ShellFile shell = ShellFile.FromFilePath(fi.FullName);
                return shell.Thumbnail.ExtraLargeBitmap;
            }
            catch
            {
                // no shell thumbnail for this file: show it without an image
                return null;
            }
        }

        void SetCurrentDupes(DupeFileCollection dupes)
        {
            FileInfo toKeep = DuplicateKeeper.GetFileToKeep(dupes);

            // identifier for this batch; later batches invalidate the queued adds of this one
            int batchId = --nextBatchId;

            Task.Run(() =>
            {
                var additions = new List<Func<ListViewItem>>();
                foreach (var f in dupes)
                {
                    if (!f.Exists)
                    {
                        continue;
                    }
                    Image thumb = MakeThumb(f);
                    bool isKeeper = toKeep != null && f.FullName == toKeep.FullName;

                    var fileName = f.Name;
                    var file = f;
                    string tooltip = "s: " + FormatFileSize(f.Length) + " f:" + f.DirectoryName + (isKeeper ? "\r\n(Recommended: keep this one)" : "");
                    var thumbKey = file.FullName;

                    additions.Add(() =>
                    {
                        var li = new ListViewItem();
                        li.Text = fileName;
                        li.Checked = true;
                        li.ToolTipText = tooltip;
                        li.Tag = file;
                        if (thumb != null)
                        {
                            try
                            {
                                lstFiles.LargeImageList.Images.Add(thumbKey, thumb);
                                li.ImageKey = thumbKey;
                            }
                            catch
                            {
                                // duplicate key or image-list problem: show without an image
                            }
                        }
                        if (isKeeper)
                        {
                            li.BackColor = Color.LightSteelBlue;
                        }
                        return li;
                    });
                }

                this.Invoke(new Action(() =>
                {
                    if (batchId != nextBatchId)
                    {
                        return; // a newer batch superseded this one while we were making thumbs
                    }
                    lstFiles.Items.Clear();
                    if (lstFiles.LargeImageList == null)
                    {
                        var imgList = new ImageList();
                        imgList.ImageSize = new Size(128, 128);
                        imgList.ColorDepth = ColorDepth.Depth24Bit;
                        imgList.TransparentColor = Color.Transparent;
                        lstFiles.LargeImageList = imgList;
                    }
                    lstFiles.LargeImageList.Images.Clear();

                    foreach (var add in additions)
                    {
                        ListViewItem li;
                        try
                        {
                            li = add();
                        }
                        catch
                        {
                            continue;
                        }
                        if (li != null)
                        {
                            lstFiles.Items.Add(li);
                        }
                    }
                    currentKeeper = toKeep;
                }));
            });
        }

        string FormatFileSize(long bytes)
        {
            double len = bytes;
            string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "YB" };
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }

        public void SetDuplicates(DuplicateFinderResult dfr)
        {
            this.dfr = dfr;
            foreach (var kv in dfr.dupesByGenerator)
            {
                if (kv.Value.Count > 0)
                {
                    pendingGenIds.Enqueue(kv.Key);
                }
            }
            btnNext_Click(null, null);
        }

        private void btnSelectBest_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem li in lstFiles.Items)
            {
                var fi = li.Tag as FileInfo;
                li.Checked = currentKeeper != null && fi != null && fi.FullName == currentKeeper.FullName;
            }
        }

        private void ShowNextDupeGroup()
        {
            while (true)
            {
                var cg = CurrentGen;
                if (cg != null && cg.Count > 0)
                {
                    //pop next dupecollection
                    var tmp = cg[cg.Count - 1];
                    cg.RemoveAt(cg.Count - 1);

                    int existingFiles = tmp.Count(f => f.Exists);
                    if (existingFiles <= 1)
                    {
                        //if only 1 file or less exist skip it
                        continue;
                    }
                    SetCurrentDupes(tmp);
                    Text = "(" + currentGenId + ") " + cg.Count + " Potential Dupes Remaining";
                    return;
                }

                // current generator exhausted: move to the next one with dupes
                if (pendingGenIds.Count > 0)
                {
                    currentGenId = pendingGenIds.Dequeue();
                    continue;
                }

                MessageBox.Show("Thats it! No more dupes");
                this.Close();
                return;
            }
        }

        private async void btnNext_Click(object sender, EventArgs e)
        {
            btnNext.Enabled = false;
            // invalidate any still-running thumbnail batch so stale items don't land in the next group
            unchecked { nextBatchId--; }

            List<string> toDelete = new List<string>();
            foreach (ListViewItem li in lstFiles.Items)
            {
                if (!li.Checked)
                {
                    var fi = li.Tag as FileInfo;
                    if (fi != null)
                    {
                        toDelete.Add(fi.FullName);
                    }
                }
            }

            List<string> failed = new List<string>();
            if (toDelete.Count > 0)
            {
                await Task.Run(() =>
                {
                    foreach (string path in toDelete)
                    {
                        try
                        {
                            File.Delete(path);
                        }
                        catch (Exception)
                        {
                            failed.Add(path);
                        }
                    }
                });
            }

            btnNext.Enabled = true;
            foreach (string path in failed)
            {
                MessageBox.Show("Failed to delete '" + path + "'");
            }

            ShowNextDupeGroup();
        }
    }
}