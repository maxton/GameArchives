namespace LibArchiveExplorer
{
    partial class PackageView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PackageView));
            this.label1 = new System.Windows.Forms.Label();
            this.fileViewContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.extractSelectedItems = new System.Windows.Forms.ToolStripMenuItem();
            this.openSelectedArchiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.largeImageList = new System.Windows.Forms.ImageList(this.components);
            this.smallImageList = new System.Windows.Forms.ImageList(this.components);
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.fileView = new System.Windows.Forms.ListView();
            this.nameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.typeColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.sizeColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.fileInspectorTabControl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.filePropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.openEditorButton = new System.Windows.Forms.Button();
            this.printExtendedInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileViewContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.fileInspectorTabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(0);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.label1.Size = new System.Drawing.Size(29, 21);
            this.label1.TabIndex = 5;
            this.label1.Text = "Path";
            // 
            // fileViewContextMenu
            // 
            this.fileViewContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extractSelectedItems,
            this.openSelectedArchiveToolStripMenuItem,
            this.printExtendedInfoToolStripMenuItem});
            this.fileViewContextMenu.Name = "fileViewContextMenu";
            this.fileViewContextMenu.Size = new System.Drawing.Size(197, 92);
            // 
            // extractSelectedItems
            // 
            this.extractSelectedItems.Name = "extractSelectedItems";
            this.extractSelectedItems.Size = new System.Drawing.Size(196, 22);
            this.extractSelectedItems.Text = "Extract selected items...";
            this.extractSelectedItems.Click += new System.EventHandler(this.extractItemsToolStripMenuItem_Click);
            // 
            // openSelectedArchiveToolStripMenuItem
            // 
            this.openSelectedArchiveToolStripMenuItem.Name = "openSelectedArchiveToolStripMenuItem";
            this.openSelectedArchiveToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.openSelectedArchiveToolStripMenuItem.Text = "Open selected archive";
            this.openSelectedArchiveToolStripMenuItem.Click += new System.EventHandler(this.openSelectedArchiveToolStripMenuItem_Click);
            // 
            // largeImageList
            // 
            this.largeImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("largeImageList.ImageStream")));
            this.largeImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.largeImageList.Images.SetKeyName(0, "Folder_large.png");
            this.largeImageList.Images.SetKeyName(1, "File_large.png");
            // 
            // smallImageList
            // 
            this.smallImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("smallImageList.ImageStream")));
            this.smallImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.smallImageList.Images.SetKeyName(0, "Folder_small.png");
            this.smallImageList.Images.SetKeyName(1, "File_small.png");
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Location = new System.Drawing.Point(29, 0);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(643, 26);
            this.flowLayoutPanel1.TabIndex = 6;
            // 
            // fileView
            // 
            this.fileView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumn,
            this.typeColumn,
            this.sizeColumn});
            this.fileView.ContextMenuStrip = this.fileViewContextMenu;
            this.fileView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileView.FullRowSelect = true;
            this.fileView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.fileView.HideSelection = false;
            this.fileView.LargeImageList = this.largeImageList;
            this.fileView.Location = new System.Drawing.Point(0, 0);
            this.fileView.Name = "fileView";
            this.fileView.Size = new System.Drawing.Size(430, 388);
            this.fileView.SmallImageList = this.smallImageList;
            this.fileView.TabIndex = 7;
            this.fileView.UseCompatibleStateImageBehavior = false;
            this.fileView.View = System.Windows.Forms.View.Details;
            this.fileView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.fileView_ItemSelectionChanged);
            this.fileView.DoubleClick += new System.EventHandler(this.fileView_DoubleClick);
            // 
            // nameColumn
            // 
            this.nameColumn.Text = "Name";
            this.nameColumn.Width = 270;
            // 
            // typeColumn
            // 
            this.typeColumn.Text = "Type";
            // 
            // sizeColumn
            // 
            this.sizeColumn.Text = "Size";
            this.sizeColumn.Width = 80;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(3, 32);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.fileView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.fileInspectorTabControl);
            this.splitContainer1.Size = new System.Drawing.Size(669, 388);
            this.splitContainer1.SplitterDistance = 430;
            this.splitContainer1.TabIndex = 11;
            // 
            // fileInspectorTabControl
            // 
            this.fileInspectorTabControl.Controls.Add(this.tabPage1);
            this.fileInspectorTabControl.Controls.Add(this.tabPage2);
            this.fileInspectorTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileInspectorTabControl.Location = new System.Drawing.Point(0, 0);
            this.fileInspectorTabControl.Name = "fileInspectorTabControl";
            this.fileInspectorTabControl.SelectedIndex = 0;
            this.fileInspectorTabControl.Size = new System.Drawing.Size(235, 388);
            this.fileInspectorTabControl.TabIndex = 10;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.filePropertyGrid);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(227, 362);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Properties";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // filePropertyGrid
            // 
            this.filePropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filePropertyGrid.HelpVisible = false;
            this.filePropertyGrid.Location = new System.Drawing.Point(3, 3);
            this.filePropertyGrid.Name = "filePropertyGrid";
            this.filePropertyGrid.Size = new System.Drawing.Size(221, 356);
            this.filePropertyGrid.TabIndex = 9;
            this.filePropertyGrid.ToolbarVisible = false;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.openEditorButton);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(227, 362);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Tools";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // openEditorButton
            // 
            this.openEditorButton.Location = new System.Drawing.Point(6, 6);
            this.openEditorButton.Name = "openEditorButton";
            this.openEditorButton.Size = new System.Drawing.Size(75, 23);
            this.openEditorButton.TabIndex = 0;
            this.openEditorButton.Text = "Open Editor";
            this.openEditorButton.UseVisualStyleBackColor = true;
            this.openEditorButton.Click += new System.EventHandler(this.openEditorButton_Click);
            // 
            // printExtendedInfoToolStripMenuItem
            // 
            this.printExtendedInfoToolStripMenuItem.Name = "printExtendedInfoToolStripMenuItem";
            this.printExtendedInfoToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.printExtendedInfoToolStripMenuItem.Text = "Print extended info";
            this.printExtendedInfoToolStripMenuItem.Click += new System.EventHandler(this.printExtendedInfoToolStripMenuItem_Click);
            // 
            // PackageView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "PackageView";
            this.Size = new System.Drawing.Size(675, 423);
            this.fileViewContextMenu.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.fileInspectorTabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.ContextMenuStrip fileViewContextMenu;
        private System.Windows.Forms.ToolStripMenuItem extractSelectedItems;
        private System.Windows.Forms.ImageList largeImageList;
        private System.Windows.Forms.ImageList smallImageList;
        private System.Windows.Forms.ToolStripMenuItem openSelectedArchiveToolStripMenuItem;
        private System.Windows.Forms.ListView fileView;
        private System.Windows.Forms.ColumnHeader nameColumn;
        private System.Windows.Forms.ColumnHeader typeColumn;
        private System.Windows.Forms.ColumnHeader sizeColumn;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl fileInspectorTabControl;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.PropertyGrid filePropertyGrid;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button openEditorButton;
        private System.Windows.Forms.ToolStripMenuItem printExtendedInfoToolStripMenuItem;
    }
}
