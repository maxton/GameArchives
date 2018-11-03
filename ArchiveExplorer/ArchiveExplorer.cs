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
using LibArchiveExplorer;

namespace ArchiveExplorer
{
  public partial class ArchiveExplorer : Form
  {
    private PackageManager pm;

    public ArchiveExplorer()
    {
      InitializeComponent();
      pm = PackageManager.GetInstance();
      pm.Spinner = this.spinnerLabel;
      pm.StatusLabel = this.toolStripStatusLabel1;
      pm.Loader = LoadFile;
      pm.SetReady();
      string[] args = Environment.GetCommandLineArgs();
      if (args.Length > 1)
      {
        if(File.Exists(args[1]))
        {
          LoadFile(Util.LocalFile(args[1]));
        }
      }
    }

    private string passcode_popup(string prompt)
    {
      var form = new KeyRequestForm(prompt);
      return form.ShowDialog() == DialogResult.OK ? form.Passcode : "";
    }

    /// <summary>
    /// Deal with a new file.
    /// </summary>
    /// <param name="file"></param>
    private async Task<PackageView> LoadFile(IFile file)
    {
      var newPage = new TabPage();
      newPage.Text = "Loading...";
      packagesTabControl.Controls.Add(newPage);
      PackageView packageView = null;
      try
      {
        var newPackage = await Task.Run(() => PackageReader.ReadPackageFromFile(file, passcode_popup));
        packageView = new PackageView(newPackage, pm);
        packageView.OnRemoveTab += RemoveTab;
        newPage.Text = newPackage.FileName;
        newPage.Controls.Add(packageView);
        packageView.Tag = newPage;
        packageView.SetView(view);
        packageView.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        packageView.Dock = System.Windows.Forms.DockStyle.Fill;
        packageView.Location = new System.Drawing.Point(3, 3);
        packageView.Margin = new System.Windows.Forms.Padding(0);
        packageView.Name = "packageView";
        packageView.TabIndex = 0;
        packagesTabControl.SelectedTab = newPage;
      }
      catch (Exception ex)
      {
        packagesTabControl.Controls.Remove(newPage);
        MessageBox.Show("Could not load archive!" + Environment.NewLine + ex.Message, "Error");
      }
      return packageView;
    }//LoadFile

    public void RemoveTab(TabPage p)
    {
      packagesTabControl.Controls.Remove(p);
    }

    

    private void openToolStripMenuItem_Click(object sender, EventArgs e)
    {
      var of = new OpenFileDialog();
      of.Title = "Select package to open.";
      of.Filter += "All Files (*.*)|*.*|" + PackageReader.SupportedFormats;
      if (of.ShowDialog() == DialogResult.OK)
      {
        LoadFile(Util.LocalFile(of.FileName));
      }
    }

    private void detailsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (!detailsToolStripMenuItem.Checked)
      {
        detailsToolStripMenuItem.Checked = true;
        iconsToolStripMenuItem.Checked = false;
        listToolStripMenuItem.Checked = false;
        SetView(View.Details);
      }
    }

    private void iconsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (!iconsToolStripMenuItem.Checked)
      {
        detailsToolStripMenuItem.Checked = false;
        iconsToolStripMenuItem.Checked = true;
        listToolStripMenuItem.Checked = false;
        SetView(View.LargeIcon);
      }
    }

    private void listToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (!listToolStripMenuItem.Checked)
      {
        detailsToolStripMenuItem.Checked = false;
        iconsToolStripMenuItem.Checked = false;
        listToolStripMenuItem.Checked = true;
        SetView(View.List);
      }
    }

    private View view = View.Details;
    private void SetView(View v)
    {
      view = v;
      foreach(TabPage t in packagesTabControl.TabPages)
      {
        if(t.Controls.Count > 0)
          (t.Controls[0] as PackageView)?.SetView(v);
      }
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private void closeToolStripMenuItem_Click(object sender, EventArgs e)
    {
      (packagesTabControl.SelectedTab?.Controls[0] as PackageView)?.Unload();
    }

    private void tabControl1_Click(object sender, MouseEventArgs e)
    {
      var ctrl = sender as TabControl;
      if (e.Button == MouseButtons.Middle)
      {
        (ctrl.TabPages.Cast<TabPage>()
          .Where((t, i) => ctrl.GetTabRect(i).Contains(e.Location))
          .First().Controls[0] as PackageView).Unload();
      }
    }

    private void gitHubToolStripMenuItem_Click(object sender, EventArgs e)
    {
      System.Diagnostics.Process.Start("https://github.com/maxton/GameArchives");
    }

    private void packagesTabControl_DragDrop(object sender, DragEventArgs e)
    {
      var files = e.Data.GetData(DataFormats.FileDrop) as string[];
      foreach(string file in files)
      {
        LoadFile(Util.LocalFile(file));
      }
    }

    private void packagesTabControl_DragEnter(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop))
        e.Effect = DragDropEffects.Copy;
    }
  }
}
