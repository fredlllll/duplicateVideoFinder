using duplicateVideoFinder;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace duplicateVideoFinderWindowsGUI
{
    public partial class FrmSelectFilesToKeep : Form
    {
        DuplicateFinderResult dfr;
        int currentGenIndex = 0;
        List<List<FileInfo>> CurrentGen
        {
            get { return dfr.dupeListsByGenerator[currentGenIndex]; }
        }
        List<FileInfo> currentDupes;

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

        Task SetCurrentDupes(List<FileInfo> dupes)
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
                    Image thumb = MakeThumb(f);
                    this.BeginInvoke(new Action(() =>
                    {
                        lstFiles.LargeImageList.Images.Add(f.FullName, thumb);

                        var li = new ListViewItem();
                        li.Text = f.Name;
                        li.Checked = true;
                        li.ImageKey = f.FullName;
                        li.Tag = f;

                        lstFiles.Items.Add(li);
                    }));
                }
            }));
        }

        public void SetDuplicates(DuplicateFinderResult dfr)
        {
            this.dfr = dfr;
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
            if (cg.Count > 0)
            {
                var tmp = cg[cg.Count - 1];
                cg.RemoveAt(cg.Count - 1);
                SetCurrentDupes(tmp).Start();
            }
            else
            {
                MessageBox.Show("Thats it! No more dupes");
                this.Close();
            }
        }
    }
}