namespace ArchiveExplorer
{
  partial class ArchiveExplorer
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ArchiveExplorer));
      this.menuStrip1 = new System.Windows.Forms.MenuStrip();
      this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.detailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.iconsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.listToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
      this.label1 = new System.Windows.Forms.Label();
      this.fileView = new System.Windows.Forms.ListView();
      this.nameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.typeColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.sizeColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.fileViewContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.extractFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.largeImageList = new System.Windows.Forms.ImageList(this.components);
      this.smallImageList = new System.Windows.Forms.ImageList(this.components);
      this.menuStrip1.SuspendLayout();
      this.fileViewContextMenu.SuspendLayout();
      this.SuspendLayout();
      // 
      // menuStrip1
      // 
      this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem});
      this.menuStrip1.Location = new System.Drawing.Point(0, 0);
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.Size = new System.Drawing.Size(480, 24);
      this.menuStrip1.TabIndex = 0;
      this.menuStrip1.Text = "menuStrip1";
      // 
      // fileToolStripMenuItem
      // 
      this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.closeToolStripMenuItem,
            this.exitToolStripMenuItem});
      this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
      this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
      this.fileToolStripMenuItem.Text = "File";
      // 
      // openToolStripMenuItem
      // 
      this.openToolStripMenuItem.Name = "openToolStripMenuItem";
      this.openToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
      this.openToolStripMenuItem.Text = "Open...";
      this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
      // 
      // closeToolStripMenuItem
      // 
      this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
      this.closeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
      this.closeToolStripMenuItem.Text = "Close";
      this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
      // 
      // exitToolStripMenuItem
      // 
      this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
      this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
      this.exitToolStripMenuItem.Text = "Exit";
      this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
      // 
      // viewToolStripMenuItem
      // 
      this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.detailsToolStripMenuItem,
            this.iconsToolStripMenuItem,
            this.listToolStripMenuItem});
      this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
      this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
      this.viewToolStripMenuItem.Text = "View";
      // 
      // detailsToolStripMenuItem
      // 
      this.detailsToolStripMenuItem.Checked = true;
      this.detailsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
      this.detailsToolStripMenuItem.Name = "detailsToolStripMenuItem";
      this.detailsToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
      this.detailsToolStripMenuItem.Text = "Details";
      this.detailsToolStripMenuItem.Click += new System.EventHandler(this.detailsToolStripMenuItem_Click);
      // 
      // iconsToolStripMenuItem
      // 
      this.iconsToolStripMenuItem.Name = "iconsToolStripMenuItem";
      this.iconsToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
      this.iconsToolStripMenuItem.Text = "Icons";
      this.iconsToolStripMenuItem.Click += new System.EventHandler(this.iconsToolStripMenuItem_Click);
      // 
      // listToolStripMenuItem
      // 
      this.listToolStripMenuItem.Name = "listToolStripMenuItem";
      this.listToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
      this.listToolStripMenuItem.Text = "List";
      this.listToolStripMenuItem.Click += new System.EventHandler(this.listToolStripMenuItem_Click);
      // 
      // flowLayoutPanel1
      // 
      this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.flowLayoutPanel1.Location = new System.Drawing.Point(41, 27);
      this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
      this.flowLayoutPanel1.Name = "flowLayoutPanel1";
      this.flowLayoutPanel1.Size = new System.Drawing.Size(427, 26);
      this.flowLayoutPanel1.TabIndex = 3;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 27);
      this.label1.Margin = new System.Windows.Forms.Padding(0);
      this.label1.Name = "label1";
      this.label1.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
      this.label1.Size = new System.Drawing.Size(29, 21);
      this.label1.TabIndex = 0;
      this.label1.Text = "Path";
      // 
      // fileView
      // 
      this.fileView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.fileView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumn,
            this.typeColumn,
            this.sizeColumn});
      this.fileView.ContextMenuStrip = this.fileViewContextMenu;
      this.fileView.FullRowSelect = true;
      this.fileView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.fileView.HideSelection = false;
      this.fileView.LargeImageList = this.largeImageList;
      this.fileView.Location = new System.Drawing.Point(12, 59);
      this.fileView.Name = "fileView";
      this.fileView.Size = new System.Drawing.Size(456, 359);
      this.fileView.SmallImageList = this.smallImageList;
      this.fileView.TabIndex = 4;
      this.fileView.UseCompatibleStateImageBehavior = false;
      this.fileView.View = System.Windows.Forms.View.Details;
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
      // fileViewContextMenu
      // 
      this.fileViewContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extractFilesToolStripMenuItem});
      this.fileViewContextMenu.Name = "fileViewContextMenu";
      this.fileViewContextMenu.Size = new System.Drawing.Size(189, 26);
      // 
      // extractFilesToolStripMenuItem
      // 
      this.extractFilesToolStripMenuItem.Name = "extractFilesToolStripMenuItem";
      this.extractFilesToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
      this.extractFilesToolStripMenuItem.Text = "Extract selected files...";
      this.extractFilesToolStripMenuItem.Click += new System.EventHandler(this.extractFilesToolStripMenuItem_Click);
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
      // ArchiveExplorer
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(480, 430);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.fileView);
      this.Controls.Add(this.flowLayoutPanel1);
      this.Controls.Add(this.menuStrip1);
      this.MainMenuStrip = this.menuStrip1;
      this.Name = "ArchiveExplorer";
      this.Text = "ArchiveExplorer";
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      this.fileViewContextMenu.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.MenuStrip menuStrip1;
    private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ListView fileView;
    private System.Windows.Forms.ColumnHeader nameColumn;
    private System.Windows.Forms.ColumnHeader typeColumn;
    private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem detailsToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem iconsToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem listToolStripMenuItem;
    private System.Windows.Forms.ImageList smallImageList;
    private System.Windows.Forms.ImageList largeImageList;
    private System.Windows.Forms.ColumnHeader sizeColumn;
    private System.Windows.Forms.ContextMenuStrip fileViewContextMenu;
    private System.Windows.Forms.ToolStripMenuItem extractFilesToolStripMenuItem;
  }
}

