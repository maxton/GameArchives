using GameArchives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchiveExplorer
{
  public class PackageManager
  {
    private static PackageManager _pm;
    public static PackageManager GetInstance()
    {
      if(_pm == null)
      {
        return (_pm = new PackageManager());
      }
      return _pm;
    }
    
    public bool Ready { get; private set; }

    public Func<IFile, Task<PackageView>> Loader { get; set; }

    private System.Windows.Forms.ToolStripStatusLabel statusLabel;
    private System.Windows.Forms.ToolStripStatusLabel spinner;
    private PackageManager()
    {
      Ready = true;
    }

    public System.Windows.Forms.ToolStripStatusLabel Spinner
    {
      set { spinner = value; }
    }
    public System.Windows.Forms.ToolStripStatusLabel StatusLabel
    {
      set { statusLabel = value; }
    }

    public void SetReady()
    {
      Ready = true;
      if(spinner != null)
        spinner.Visible = false;
      if (statusLabel != null)
        statusLabel.Text = "Ready.";
    }

    public void SetBusyState(string busyState)
    {
      Ready = false;
      if (spinner != null)
        spinner.Visible = true;
      if (statusLabel != null)
        statusLabel.Text = busyState;
    }

    public async void LoadFile(IFile f, PackageView owner = null)
    {
      if(Loader == null) return;
      var pkgView = await Loader(f);
      if(owner != null && pkgView != null)
      {
        owner.AddChildPackage(pkgView);
      }
    }
  }
}
