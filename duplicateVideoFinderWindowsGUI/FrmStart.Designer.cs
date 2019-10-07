namespace duplicateVideoFinderWindowsGUI
{
    partial class FrmStart
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txtDirectory = new System.Windows.Forms.TextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.chkHash = new System.Windows.Forms.CheckBox();
            this.chkDuration = new System.Windows.Forms.CheckBox();
            this.chkThumb = new System.Windows.Forms.CheckBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.fbd = new System.Windows.Forms.FolderBrowserDialog();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Directory";
            // 
            // txtDirectory
            // 
            this.txtDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDirectory.Location = new System.Drawing.Point(12, 25);
            this.txtDirectory.Name = "txtDirectory";
            this.txtDirectory.Size = new System.Drawing.Size(429, 20);
            this.txtDirectory.TabIndex = 1;
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.Location = new System.Drawing.Point(474, 51);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(100, 25);
            this.btnStart.TabIndex = 2;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 132);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(562, 23);
            this.progressBar1.TabIndex = 3;
            // 
            // chkHash
            // 
            this.chkHash.AutoSize = true;
            this.chkHash.Location = new System.Drawing.Point(12, 59);
            this.chkHash.Name = "chkHash";
            this.chkHash.Size = new System.Drawing.Size(100, 17);
            this.chkHash.TabIndex = 4;
            this.chkHash.Text = "Check By Hash";
            this.chkHash.UseVisualStyleBackColor = true;
            // 
            // chkDuration
            // 
            this.chkDuration.AutoSize = true;
            this.chkDuration.Enabled = false;
            this.chkDuration.Location = new System.Drawing.Point(12, 82);
            this.chkDuration.Name = "chkDuration";
            this.chkDuration.Size = new System.Drawing.Size(115, 17);
            this.chkDuration.TabIndex = 5;
            this.chkDuration.Text = "Check By Duration";
            this.chkDuration.UseVisualStyleBackColor = true;
            // 
            // chkThumb
            // 
            this.chkThumb.AutoSize = true;
            this.chkThumb.Enabled = false;
            this.chkThumb.Location = new System.Drawing.Point(12, 105);
            this.chkThumb.Name = "chkThumb";
            this.chkThumb.Size = new System.Drawing.Size(108, 17);
            this.chkThumb.TabIndex = 6;
            this.chkThumb.Text = "Check By Thumb";
            this.chkThumb.UseVisualStyleBackColor = true;
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.Location = new System.Drawing.Point(447, 23);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(127, 23);
            this.btnSearch.TabIndex = 7;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // fbd
            // 
            this.fbd.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // FrmStart
            // 
            this.ClientSize = new System.Drawing.Size(586, 167);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.chkThumb);
            this.Controls.Add(this.chkDuration);
            this.Controls.Add(this.chkHash);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.txtDirectory);
            this.Controls.Add(this.label1);
            this.Name = "FrmStart";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtDirectory;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.CheckBox chkHash;
        private System.Windows.Forms.CheckBox chkDuration;
        private System.Windows.Forms.CheckBox chkThumb;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.FolderBrowserDialog fbd;
    }
}

