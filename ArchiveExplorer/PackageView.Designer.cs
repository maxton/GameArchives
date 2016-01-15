namespace ArchiveExplorer
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
      this.fileView = new System.Windows.Forms.ListView();
      this.nameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.typeColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.sizeColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
      this.fileViewContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.extractSelectedItems = new System.Windows.Forms.ToolStripMenuItem();
      this.smallImageList = new System.Windows.Forms.ImageList(this.components);
      this.largeImageList = new System.Windows.Forms.ImageList(this.components);
      this.fileViewContextMenu.SuspendLayout();
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
      this.fileView.Location = new System.Drawing.Point(0, 32);
      this.fileView.Name = "fileView";
      this.fileView.Size = new System.Drawing.Size(484, 384);
      this.fileView.SmallImageList = this.smallImageList;
      this.fileView.TabIndex = 7;
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
      // flowLayoutPanel1
      // 
      this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.flowLayoutPanel1.Location = new System.Drawing.Point(29, 0);
      this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
      this.flowLayoutPanel1.Name = "flowLayoutPanel1";
      this.flowLayoutPanel1.Size = new System.Drawing.Size(455, 26);
      this.flowLayoutPanel1.TabIndex = 6;
      // 
      // fileViewContextMenu
      // 
      this.fileViewContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extractSelectedItems});
      this.fileViewContextMenu.Name = "fileViewContextMenu";
      this.fileViewContextMenu.Size = new System.Drawing.Size(197, 48);
      // 
      // extractSelectedItems
      // 
      this.extractSelectedItems.Name = "extractSelectedItems";
      this.extractSelectedItems.Size = new System.Drawing.Size(196, 22);
      this.extractSelectedItems.Text = "Extract selected items...";
      this.extractSelectedItems.Click += new System.EventHandler(this.extractItemsToolStripMenuItem_Click);
      // 
      // smallImageList
      // 
      this.smallImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("smallImageList.ImageStream")));
      this.smallImageList.TransparentColor = System.Drawing.Color.Transparent;
      this.smallImageList.Images.SetKeyName(0, "Folder_small.png");
      this.smallImageList.Images.SetKeyName(1, "File_small.png");
      // 
      // largeImageList
      // 
      this.largeImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("largeImageList.ImageStream")));
      this.largeImageList.TransparentColor = System.Drawing.Color.Transparent;
      this.largeImageList.Images.SetKeyName(0, "Folder_large.png");
      this.largeImageList.Images.SetKeyName(1, "File_large.png");
      // 
      // PackageView
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.label1);
      this.Controls.Add(this.fileView);
      this.Controls.Add(this.flowLayoutPanel1);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "PackageView";
      this.Size = new System.Drawing.Size(487, 419);
      this.fileViewContextMenu.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ListView fileView;
    private System.Windows.Forms.ColumnHeader nameColumn;
    private System.Windows.Forms.ColumnHeader typeColumn;
    private System.Windows.Forms.ColumnHeader sizeColumn;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    private System.Windows.Forms.ContextMenuStrip fileViewContextMenu;
    private System.Windows.Forms.ToolStripMenuItem extractSelectedItems;
    private System.Windows.Forms.ImageList largeImageList;
    private System.Windows.Forms.ImageList smallImageList;
  }
}
