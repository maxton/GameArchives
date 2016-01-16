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

namespace ArchiveExplorer
{
  [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
  public partial class PackageView : UserControl
  {
    private IDirectory currentDirectory;
    private AbstractPackage currentPackage;

    private List<PackageView> children;
    private PackageManager pm;

    public PackageView(AbstractPackage pkg)
    {
      InitializeComponent();
      pm = PackageManager.GetInstance();
      children = new List<PackageView>(0);
      currentPackage = pkg;
      currentDirectory = currentPackage.RootDirectory;
      ResetBreadcrumbs();
      FillFileView();
    }

    public void SetView(View v)
    {
      fileView.View = v;
    }

    private string HumanReadableFileSize(long size)
    {
      if (size > (1024 * 1024 * 1024))
      {
        return (size / (double)(1024 * 1024 * 1024)).ToString("F") + " GiB";
      }
      else if (size > (1024 * 1024))
      {
        return (size / (double)(1024 * 1024)).ToString("F") + " MiB";
      }
      else if (size > 1024)
      {
        return (size / 1024.0).ToString("F") + " KiB";
      }
      else
      {
        return size.ToString() + " B";
      }
    }

    private void SetCurrentDir(IDirectory dir)
    {
      currentDirectory = dir;
      FillFileView();
      ResetBreadcrumbs();
    }

    public void Unload()
    {
      foreach(var pkg in children)
      {
        pkg.Unload();
      }
      currentPackage?.Dispose();
      currentPackage = null;
      currentDirectory = null;
      FillFileView();
      ResetBreadcrumbs();
      (this.FindForm() as ArchiveExplorer).RemoveTab(this.Tag as TabPage);
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
          var item = new ListViewItem(new string[] { file.Name, "File", HumanReadableFileSize(file.Size) });
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
          (i as IFile).ExtractTo(Path.Combine(path, (i as IFile).Name));
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
          pm.LoadFile(fileView.SelectedItems[0].Tag as IFile, currentPackage);
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
        pm.LoadFile(fileView.SelectedItems[0].Tag as IFile, currentPackage);
      }
    }
  }
}
