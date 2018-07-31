using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel.Design;
using GameArchives;
using System.IO;
using FolderSelect;

namespace LibArchiveExplorer
{
  [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
  public partial class PackageView : UserControl
  {
    private IDirectory currentDirectory;
    private AbstractPackage currentPackage;

    private List<PackageView> children;
    private PackageManager pm;
    private PackageView parent = null;

    private EditorWindow ew;

    public delegate void FileOpenEventHandler(IFile file);
    public delegate void RemoveTabEventHandler(TabPage t);

    public event FileOpenEventHandler OnFileOpen;
    public event RemoveTabEventHandler OnRemoveTab;

    public PackageView(AbstractPackage pkg, PackageManager pm)
    {
      InitializeComponent();
      InitCustomComponents();

      this.pm = pm;
      children = new List<PackageView>(0);
      currentPackage = pkg;
      currentDirectory = currentPackage.RootDirectory;
      filePropertyGrid.SelectedObject = currentPackage;
      ResetBreadcrumbs();
      FillFileView();
    }

    private void InitCustomComponents()
    {
      this.filePropertyGrid.ToolbarVisible = false;
    }

    /// <summary>
    /// Sets the display mode of the files in the view (e.g., icons, details, list...)
    /// </summary>
    public void SetView(View v)
    {
      fileView.View = v;
    }

    /// <summary>
    /// Navigates the view to the given directory.
    /// </summary>
    private void SetCurrentDir(IDirectory dir)
    {
      currentDirectory = dir;
      FillFileView();
      ResetBreadcrumbs();
    }

    /// <summary>
    /// Add a package as a child of this package.
    /// This means the given package will be closed if this package is closed.
    /// </summary>
    public void AddChildPackage(PackageView p)
    {
      this.children.Add(p);
      p.SetParentPackage(this);
    }

    private void RemoveChildPackage(PackageView p)
    {
      this.children.Remove(p);
    }
    
    private void SetParentPackage(PackageView p)
    {
      parent = p;
    }

    public void Unload()
    {
      parent?.RemoveChildPackage(this);
      ew?.Close();
      var childrenFixed = children.ToArray();
      foreach(var pkg in childrenFixed)
      {
        pkg.Unload();
      }
      currentPackage?.Dispose();
      currentPackage = null;
      currentDirectory = null;
      FillFileView();
      ResetBreadcrumbs();
      OnRemoveTab?.Invoke(Tag as TabPage);
    }

    private void FillFileView()
    {
      fileView.Items.Clear();
      if (currentDirectory != null)
      {
        foreach (var dir in currentDirectory.Dirs)
        {
          var item = new ListViewItem(new string[] { dir.Name, "Directory" });
          item.Tag = dir;
          item.ImageIndex = 0;
          fileView.Items.Add(item);
        }
        foreach (var file in currentDirectory.Files)
        {
          var item = new ListViewItem(new string[] { file.Name, "File", file.Size.HumanReadableFileSize() });
          item.Tag = file;
          item.ImageIndex = 1;
          fileView.Items.Add(item);
        }
      }
    }

    private void ResetBreadcrumbs()
    {
      var dir = currentDirectory;
      var i = 1;
      var breadcrumbs = new List<Button>();
      flowLayoutPanel1.Controls.Clear();
      while (dir != null)
      {
        var button = new Button();
        breadcrumbs.Insert(0, button);
        button.AutoSize = true;
        button.Margin = new System.Windows.Forms.Padding(0);
        button.Name = $"breadcrumb{i}";
        button.Size = new System.Drawing.Size(3, 23);
        button.TabIndex = i;
        button.Text = dir.Name;
        button.UseVisualStyleBackColor = true;
        button.Tag = dir;
        button.Click += (x, y) => SetCurrentDir(button.Tag as IDirectory);
        dir = dir.Parent;
        i++;
      }
      foreach (var btn in breadcrumbs)
      {
        flowLayoutPanel1.Controls.Add(btn);
      }
      flowLayoutPanel1.PerformLayout();
      PerformLayout();
    }

    private void ExtractDir(IDirectory dir, string path, IProgress<string> p)
    {
      foreach (IFile f in dir.Files)
      {
        p.Report("Extracting " + f.Name);
        f.ExtractTo(Path.Combine(path, SafeName(f.Name)));
      }
      foreach (IDirectory d in dir.Dirs)
      {
        string newPath = Path.Combine(path, SafeName(d.Name));
        Directory.CreateDirectory(newPath);
        ExtractDir(d, newPath, p);
      }
    }

    private string SafeName(string name)
    {
      name = name.Replace("\\", "").Replace("/", "").Replace(":", "").Replace("*", "")
        .Replace("?", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");
      if (name == ".." || name == ".")
      {
        name = "(" + name + ")";
      }
      return name;
    }

    private void ExtractItems(string path, IEnumerable<IFSNode> nodes, IProgress<string> p)
    {
      foreach (IFSNode i in nodes)
      {
        if (i is IDirectory)
        {
          string newPath = Path.Combine(path, SafeName((i as IDirectory).Name));
          Directory.CreateDirectory(newPath);
          ExtractDir((i as IDirectory), newPath, p);
        }
        else
        {
          p.Report("Extracting " + (i as IFile).Name);
          (i as IFile).ExtractTo(Path.Combine(path, SafeName((i as IFile).Name)));
        }
      }
    }

    private void fileView_DoubleClick(object sender, EventArgs e)
    {
      if (fileView.SelectedItems.Count == 1)
      {
        if (fileView.SelectedItems[0].Tag is IDirectory)
        {
          SetCurrentDir(fileView.SelectedItems[0].Tag as IDirectory);
        }
        else if (fileView.SelectedItems[0].Tag is IFile)
        {
          pm.LoadFile(fileView.SelectedItems[0].Tag as IFile, this);
        }
      }
    }

    private async void extractItemsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (!pm.Ready)
      {
        MessageBox.Show("Please wait for the current operation to complete.");
        return;
      }
      var fb = new FolderSelectDialog();
      if (fb.ShowDialog())
      {
        pm.SetBusyState("Extracting files...");
        var items = new List<IFSNode>(fileView.SelectedItems.Count);
        foreach(ListViewItem selItem in fileView.SelectedItems)
        {
          if (selItem.Tag is IFSNode)
            items.Add(selItem.Tag as IFSNode);
        }
        var progress = new Progress<string>((status) => { pm.SetBusyState(status); });
        try {
          await Task.Run(() => ExtractItems(fb.FileName, items, progress));
        }
        catch(Exception ex)
        {
          MessageBox.Show("Error when extracting files:" + Environment.NewLine + ex.Message);
        }
        pm.SetReady();
      }
    }

    private void openSelectedArchiveToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if(fileView.SelectedItems.Count != 1)
      {
        MessageBox.Show("Please select just one archive to open.");
        return;
      }
      if(fileView.SelectedItems[0].Tag is IFile)
      {
        pm.LoadFile(fileView.SelectedItems[0].Tag as IFile, this);
      }
    }

    private void fileView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
    {
      if(fileView.SelectedItems.Count == 1)
      {
        filePropertyGrid.SelectedObject = fileView.SelectedItems[0].Tag;
      }
      else
      {
        filePropertyGrid.SelectedObject = currentPackage;
      }
    }

    private void openEditorButton_Click(object sender, EventArgs e)
    {
      try
      {
        if (ew == null)
        {
          ew = new EditorWindow(currentPackage);
          ew.FormClosed += (o, ev) => ew = null;
        }
        ew.Show(this);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    private void printExtendedInfoToolStripMenuItem_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem selItem in fileView.SelectedItems)
      {
        if (selItem.Tag is IFile)
        {
          var f = selItem.Tag as IFile;
          var sb = new StringBuilder();
          foreach(var kvp in f.ExtendedInfo)
          {
            sb.AppendFormat("{0}: {1}\r\n", kvp.Key, kvp.Value.ToString());
          }
          MessageBox.Show(sb.ToString());
        }
      }
    }
  }
}
