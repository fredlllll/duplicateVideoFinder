using duplicateVideoFinder;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;

namespace duplicateVideoFinderWindowsGUI
{
    public partial class FrmSelectFilesToKeep : Form
    {
        DuplicateFinderResult dfr;
        string currentGenId = "";
        List<DupeFileCollection> CurrentGen
        {
            get
            {
                if (dfr.dupesByGenerator.Count > 0)
                {
                    return dfr.dupesByGenerator[currentGenId];
                }
                return null;
            }
        }
        DupeFileCollection currentDupes;

        public FrmSelectFilesToKeep()
        {
            InitializeComponent();
        }

        Bitmap MakeThumb(FileInfo fi)
        {
            ShellFile shell = ShellFile.FromFilePath(fi.FullName);
            var tttt = shell.Properties.System.Video;
            return shell.Thumbnail.ExtraLargeBitmap;


            /*var tmpPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".jpg");
            Conversion.Snapshot(fi.FullName, tmpPath, TimeSpan.FromSeconds(15)).Start().Wait();
            Bitmap daImg = new Bitmap(128, 128);
            using (var g = Graphics.FromImage(daImg))
            using (Bitmap daThumb = new Bitmap(tmpPath))
            {
                g.DrawImage(daThumb, new Rectangle(0, 0, 128, 128));
                g.Flush();
            }
            File.Delete(tmpPath);

            return daImg;*/
        }

        Task SetCurrentDupes(DupeFileCollection dupes)
        {
            return new Task(new Action(() =>
            {
                currentDupes = dupes;

                this.Invoke(new Action(() =>
                {
                    lstFiles.Items.Clear();
                    if (lstFiles.LargeImageList == null)
                    {
                        var imgList = new ImageList();
                        imgList.ImageSize = new Size(128, 128);
                        imgList.ColorDepth = ColorDepth.Depth24Bit;
                        imgList.TransparentColor = Color.Transparent;
                        lstFiles.LargeImageList = imgList;
                    }
                    lstFiles.LargeImageList.Images.Clear(); //lets hope the finalizer calls dispose on all these bitmaps...
                }));


                for (int i = 0; i < dupes.Count; i++)
                {
                    var f = dupes[i];
                    if (f.Exists)
                    {
                        Image thumb = MakeThumb(f);
                        this.BeginInvoke(new Action(() =>
                        {
                            lstFiles.LargeImageList.Images.Add(f.FullName, thumb);

                            var li = new ListViewItem();
                            li.Text = f.Name;
                            li.Checked = true;
                            li.ImageKey = f.FullName;
                            li.ToolTipText = "s: " + FormatFileSize(f.Length) + " f:" + f.DirectoryName;
                            li.Tag = f;

                            lstFiles.Items.Add(li);
                        }));
                    }
                }
            }));
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

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }

        public void SetDuplicates(DuplicateFinderResult dfr)
        {
            this.dfr = dfr;
            this.currentGenId = dfr.dupesByGenerator.First().Key;
            btnNext_Click(null, null);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem li in lstFiles.Items)
            {
                if (!li.Checked)
                {
                    File.Delete((li.Tag as FileInfo).FullName);
                }
            }
            var cg = CurrentGen;
            while (true)
            {
                if (cg != null && cg.Count > 0)
                {
                    //pop next dupecollection
                    var tmp = cg[cg.Count - 1];
                    cg.RemoveAt(cg.Count - 1);

                    int existingFiles = 0;
                    foreach (var f in tmp)
                    {
                        if (f.Exists)
                        {
                            existingFiles++;
                        }
                    }
                    if (existingFiles <= 1)
                    {
                        //if only 1 file or less exist skip it
                        continue;
                    }
                    SetCurrentDupes(tmp).Start();
                    Text = cg.Count + " Potential Dupes Remaining";
                }
                else
                {
                    MessageBox.Show("Thats it! No more dupes");
                    this.Close();
                }
                break;
            }
        }
    }
}