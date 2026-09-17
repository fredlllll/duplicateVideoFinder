using System.Windows.Forms;

namespace duplicateVideoFinderWindowsGUI
{
    partial class FrmSelectFilesToKeep
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnNext = new System.Windows.Forms.Button();
            this.lstFiles = new System.Windows.Forms.ListView();
            this.btnSelectBest = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnNext
            // 
            this.btnNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNext.Location = new System.Drawing.Point(612, 12);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(180, 25);
            this.btnNext.TabIndex = 0;
            this.btnNext.Text = "Delete Unselected and Next";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // lstFiles
            // 
            this.lstFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstFiles.CheckBoxes = true;
            this.lstFiles.HideSelection = false;
            this.lstFiles.LabelWrap = false;
            this.lstFiles.Location = new System.Drawing.Point(12, 43);
            this.lstFiles.Name = "lstFiles";
            this.lstFiles.ShowGroups = false;
            this.lstFiles.ShowItemToolTips = true;
            this.lstFiles.Size = new System.Drawing.Size(780, 356);
            this.lstFiles.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lstFiles.TabIndex = 0;
            this.lstFiles.UseCompatibleStateImageBehavior = false;
            // 
            // btnSelectBest
            // 
            this.btnSelectBest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectBest.Location = new System.Drawing.Point(440, 12);
            this.btnSelectBest.Name = "btnSelectBest";
            this.btnSelectBest.Size = new System.Drawing.Size(164, 25);
            this.btnSelectBest.TabIndex = 1;
            this.btnSelectBest.Text = "Select Best";
            this.btnSelectBest.UseVisualStyleBackColor = true;
            this.btnSelectBest.Click += new System.EventHandler(this.btnSelectBest_Click);
            // 
            // FrmSelectFilesToKeep
            // 
            this.ClientSize = new System.Drawing.Size(804, 411);
            this.Controls.Add(this.btnSelectBest);
            this.Controls.Add(this.lstFiles);
            this.Controls.Add(this.btnNext);
            this.Name = "FrmSelectFilesToKeep";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private Button btnNext;
        #endregion

        private ListView lstFiles;
        private Button btnSelectBest;
    }
}