namespace LibArchiveExplorer
{
    partial class EditorWindow
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
            this.discUsageGraphic = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.discUsageBar = new System.Windows.Forms.ProgressBar();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblTotalFileSize = new System.Windows.Forms.Label();
            this.lblTotalFiles = new System.Windows.Forms.Label();
            this.lblPackageSize = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.actionsGroupBox = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.discUsageGraphic)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.actionsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // discUsageGraphic
            // 
            this.discUsageGraphic.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
            this.discUsageGraphic.BackColor = System.Drawing.Color.White;
            this.discUsageGraphic.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.discUsageGraphic.Location = new System.Drawing.Point(12, 25);
            this.discUsageGraphic.Name = "discUsageGraphic";
            this.discUsageGraphic.Size = new System.Drawing.Size(560, 64);
            this.discUsageGraphic.TabIndex = 0;
            this.discUsageGraphic.TabStop = false;
            this.discUsageGraphic.Paint += new System.Windows.Forms.PaintEventHandler(this.DrawGraphic);
            this.discUsageGraphic.MouseDown += new System.Windows.Forms.MouseEventHandler(this.discUsageGraphic_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Disc Usage:";
            // 
            // discUsageBar
            // 
            this.discUsageBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
            this.discUsageBar.Enabled = false;
            this.discUsageBar.Location = new System.Drawing.Point(12, 95);
            this.discUsageBar.Name = "discUsageBar";
            this.discUsageBar.Size = new System.Drawing.Size(560, 23);
            this.discUsageBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.discUsageBar.TabIndex = 2;
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(12, 124);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(394, 225);
            this.listView1.TabIndex = 3;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 345;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Package size:";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.lblTotalFileSize);
            this.groupBox1.Controls.Add(this.lblTotalFiles);
            this.groupBox1.Controls.Add(this.lblPackageSize);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(413, 249);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(160, 99);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Package Info";
            // 
            // lblTotalFileSize
            // 
            this.lblTotalFileSize.AutoSize = true;
            this.lblTotalFileSize.Location = new System.Drawing.Point(97, 64);
            this.lblTotalFileSize.Name = "lblTotalFileSize";
            this.lblTotalFileSize.Size = new System.Drawing.Size(23, 13);
            this.lblTotalFileSize.TabIndex = 9;
            this.lblTotalFileSize.Text = "0 B";
            // 
            // lblTotalFiles
            // 
            this.lblTotalFiles.AutoSize = true;
            this.lblTotalFiles.Location = new System.Drawing.Point(97, 47);
            this.lblTotalFiles.Name = "lblTotalFiles";
            this.lblTotalFiles.Size = new System.Drawing.Size(13, 13);
            this.lblTotalFiles.TabIndex = 8;
            this.lblTotalFiles.Text = "0";
            // 
            // lblPackageSize
            // 
            this.lblPackageSize.AutoSize = true;
            this.lblPackageSize.Location = new System.Drawing.Point(97, 30);
            this.lblPackageSize.Name = "lblPackageSize";
            this.lblPackageSize.Size = new System.Drawing.Size(23, 13);
            this.lblPackageSize.TabIndex = 7;
            this.lblPackageSize.Text = "0 B";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 64);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Total file size:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(36, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Total files:";
            // 
            // actionsGroupBox
            // 
            this.actionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.actionsGroupBox.Controls.Add(this.button1);
            this.actionsGroupBox.Enabled = false;
            this.actionsGroupBox.Location = new System.Drawing.Point(413, 124);
            this.actionsGroupBox.Name = "actionsGroupBox";
            this.actionsGroupBox.Size = new System.Drawing.Size(159, 119);
            this.actionsGroupBox.TabIndex = 6;
            this.actionsGroupBox.TabStop = false;
            this.actionsGroupBox.Text = "Actions";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 19);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(147, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Replace File";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // EditorWindow
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 361);
            this.Controls.Add(this.actionsGroupBox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.discUsageBar);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.discUsageGraphic);
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "EditorWindow";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Package Editor";
            ((System.ComponentModel.ISupportInitialize)(this.discUsageGraphic)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.actionsGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox discUsageGraphic;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar discUsageBar;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblTotalFileSize;
        private System.Windows.Forms.Label lblTotalFiles;
        private System.Windows.Forms.Label lblPackageSize;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox actionsGroupBox;
    }
}

