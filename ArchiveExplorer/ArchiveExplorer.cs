/*
 * ArchiveExplorer.cs
 * 
 * Copyright (c) 2015,2016, maxton. All rights reserved.
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3.0 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; If not, see
 * <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GameArchives;
using System.IO;
using FolderSelect;

namespace ArchiveExplorer
{
  public partial class ArchiveExplorer : Form
  {
    private IDirectory currentDirectory;
    private AbstractPackage currentPackage;

    public ArchiveExplorer()
    {
      InitializeComponent();
      string[] args = Environment.GetCommandLineArgs();
      if (args.Length > 1)
      {
        if(File.Exists(args[1]))
        {
          LoadFile(args[1]);
        }
      }
    }

    private string HumanReadableFileSize(ulong size)
    {
      if(size > (1024 * 1024 * 1024))
      {
        return (size / (double)(1024 * 1024 * 1024)).ToString("F") + " GiB";
      }
      else if(size > (1024 * 1024))
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

    /// <summary>
    /// Deal with a new file.
    /// </summary>
    /// <param name="file"></param>
    private void LoadFile(string file)
    {
      try
      {
        UnloadPackage();
        currentPackage = PackageReader.ReadPackageFromFile(file);
        if (currentPackage != null)
        {
          Text = Application.ProductName + " - " + currentPackage.FileName;
          currentDirectory = currentPackage.RootDirectory;
          ResetBreadcrumbs();
          FillFileView();
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Could not load archive!" + Environment.NewLine + ex.Message, "Error");
      }
    }//LoadFile
    
    private void SetCurrentDir(IDirectory dir)
    {
      currentDirectory = dir;
      FillFileView();
      ResetBreadcrumbs();
    }

    private void UnloadPackage()
    {
      Text = Application.ProductName;
      currentPackage?.Dispose();
      currentPackage = null;
      currentDirectory = null;
      FillFileView();
      ResetBreadcrumbs();
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

    private void ExtractDir(IDirectory dir, string path)
    {
      foreach(IFile f in dir.Files)
      {
        f.ExtractTo(Path.Combine(path, SafeName(f.Name)));
      }
      foreach(IDirectory d in dir.Dirs)
      {
        string newPath = Path.Combine(path, SafeName(d.Name));
        Directory.CreateDirectory(newPath);
        ExtractDir(d, newPath);
      }
    }

    private string SafeName(string name)
    {
      name = name.Replace("\\", "").Replace("/", "").Replace(":", "").Replace("*", "")
        .Replace("?", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");
      if(name == ".." || name == ".")
      {
        name = "(" + name + ")";
      }
      return name;
    }

    private void openToolStripMenuItem_Click(object sender, EventArgs e)
    {
      var of = new OpenFileDialog();
      of.Title = "Select package to open.";
      of.Filter += "All Files (*.*)|*.*";
      of.Filter += "|STFS Package (*.*)|*.*";
      of.Filter += "|Ark Package (*.hdr)|*.hdr";
      of.Filter += "|FSAR Package (*.far)|*.far";
      if (of.ShowDialog() == DialogResult.OK)
      {
        LoadFile(of.FileName);
      }
    }

    private void detailsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (!detailsToolStripMenuItem.Checked)
      {
        detailsToolStripMenuItem.Checked = true;
        iconsToolStripMenuItem.Checked = false;
        listToolStripMenuItem.Checked = false;
        fileView.View = View.Details;
      }
    }

    private void iconsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (!iconsToolStripMenuItem.Checked)
      {
        detailsToolStripMenuItem.Checked = false;
        iconsToolStripMenuItem.Checked = true;
        listToolStripMenuItem.Checked = false;
        fileView.View = View.LargeIcon;
      }
    }

    private void listToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (!listToolStripMenuItem.Checked)
      {
        detailsToolStripMenuItem.Checked = false;
        iconsToolStripMenuItem.Checked = false;
        listToolStripMenuItem.Checked = true;
        fileView.View = View.List;
      }
    }

    private void fileView_DoubleClick(object sender, EventArgs e)
    {
      if(fileView.SelectedItems.Count == 1)
      {
        if(fileView.SelectedItems[0].Tag is IDirectory)
        {
          SetCurrentDir(fileView.SelectedItems[0].Tag as IDirectory);
        }
      }
    }

    private void extractItemsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      var fb = new FolderSelectDialog();
      if(fb.ShowDialog())
      {
        foreach(ListViewItem i in fileView.SelectedItems)
        {
          if (i.Tag is IDirectory)
          {
            string newPath = Path.Combine(fb.FileName, SafeName((i.Tag as IDirectory).Name));
            Directory.CreateDirectory(newPath);
            ExtractDir((i.Tag as IDirectory), newPath);
          }
          else
          {
            (i.Tag as IFile).ExtractTo(Path.Combine(fb.FileName, (i.Tag as IFile).Name));
          }
        }
      }
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private void closeToolStripMenuItem_Click(object sender, EventArgs e)
    {
      UnloadPackage();
    }
  }
}
