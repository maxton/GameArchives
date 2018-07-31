using GameArchives;
using GameArchives.Common;
using GameArchives.XISO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibArchiveExplorer
{
  public partial class EditorWindow : Form
  {
    private AbstractPackage _pkg;
    private List<OffsetFile> _allFiles;

    public EditorWindow(AbstractPackage pkg)
    {
      InitializeComponent();
      _pkg = pkg;
      Resize += (o,e) => discUsageGraphic.Invalidate();
      LoadPackage();
    }

    /// <summary>
    /// Draws a graphic that looks like the old Disk Defragmenter disk usage image.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="evt"></param>
    private void DrawGraphic(object sender, PaintEventArgs evt)
    {
      evt.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
      foreach (var file in _allFiles ?? new List<OffsetFile>())
      {
        var rect = new RectangleF
        {
          X = file.DataLocation * discUsageGraphic.Width / (float)_pkg.Size,
          Y = 0,
          Height = discUsageGraphic.Height,
          Width = file.Size * discUsageGraphic.Width / (float)_pkg.Size
        };
        evt.Graphics.FillRectangle(Brushes.Gray, rect);
      }
      evt.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
      foreach (ListViewItem f in listView1.SelectedItems)
      {
        var file = f.Tag as OffsetFile;
        var rect = new RectangleF
        {
          X = file.DataLocation * discUsageGraphic.Width / (float)_pkg.Size,
          Y = -1,
          Height = discUsageGraphic.Height + 2,
          Width = Math.Max(file.Size * discUsageGraphic.Width / (float)_pkg.Size, 1)
        };
        evt.Graphics.FillRectangle(Brushes.Black, rect);
      }
    }

    /// <summary>
    /// Load the package.
    /// </summary>
    private void LoadPackage()
    {
      if (_pkg.FileType.IsSubclassOrEqual(typeof(OffsetFile)))
      {
        discUsageGraphic.Invalidate();
        _allFiles = _pkg.GetAllFiles<OffsetFile>();
        var totalSize = _allFiles.Sum((f) => f.Size);
        discUsageBar.Value = ((int)(100 * totalSize / _pkg.Size)).Clamp(discUsageBar.Minimum, discUsageBar.Maximum);
        _allFiles.Sort((f, f2) => Math.Sign(f.DataLocation - f2.DataLocation));
        _allFiles.ForEach(f =>
          listView1.Items.Add(new ListViewItem(new string[] { f.Name })
          {
            Tag = f
          }));

        lblPackageSize.Text = _pkg.Size.HumanReadableFileSize();
        lblTotalFiles.Text = _allFiles.Count.ToString();
        lblTotalFileSize.Text = totalSize.HumanReadableFileSize();
        actionsGroupBox.Enabled = _pkg.Writeable;
      }
      else
      {
        throw new NotImplementedException("Editor support is not implemented for this package type");
      }
    }

    private void SelectFileAtByte(long b)
    {
      foreach(ListViewItem item in listView1.SelectedItems)
      {
        item.Selected = false;
        item.Focused = false;
      }
      foreach(ListViewItem item in listView1.Items)
      {
        var file = item.Tag as OffsetFile;
        if(file.DataLocation <= b && file.DataLocation + file.Size >= b)
        {
          listView1.FocusedItem = item;
          item.Selected = true;
          item.Focused = true;
          item.EnsureVisible();
          break;
        }
      }
    }

    /// <summary>
    /// Closes the currently opened package.
    /// </summary>
    private void ClosePackage()
    {
      _pkg = null;
      listView1.Items.Clear();
      _allFiles?.Clear();
      discUsageGraphic.Invalidate();
      discUsageBar.Value = 0;
    }
    

    private void toolStripMenuItem1_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void listView1_SelectedIndexChanged(object sender, EventArgs e)
    {
      discUsageGraphic.Invalidate();
    }
    
    private void discUsageGraphic_Click(object sender, MouseEventArgs e)
    {
      SelectFileAtByte((e as MouseEventArgs).X * _pkg.Size / discUsageGraphic.Width);
    }

    private void button1_Click(object sender, EventArgs e)
    {
      if(listView1.SelectedItems.Count != 1)
      {
        MessageBox.Show(this, "Please select only one item.");
        return;
      }
      var file = listView1.SelectedItems[0].Tag as OffsetFile;
      var x = new OpenFileDialog();
      if(x.ShowDialog() == DialogResult.OK)
      {
        var newFile = Util.LocalFile(x.FileName);
        if(MessageBox.Show("This will PERMANENTLY modify the archive."
          +Environment.NewLine+"Are you sure you want to continue?","Confirm Action", MessageBoxButtons.YesNo)
          == DialogResult.Yes)
        {
          try
          {
            if((_pkg as MutablePackage)?.FileReplaceCheck(file, newFile) == true
              && (_pkg as MutablePackage)?.TryReplaceFile(file, newFile) == true)
            {
              MessageBox.Show("Successfully replaced.");
            }
            else
            {
              throw new Exception("Failed");
            }
          }
          catch(Exception ex)
          {
            MessageBox.Show(ex.Message);
          }
        }
      }
    }
  }
}
