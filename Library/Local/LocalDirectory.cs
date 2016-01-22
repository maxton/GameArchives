using GameArchives.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GameArchives.Local
{
  /// <summary>
  /// Represents a directory in the local file system.
  /// </summary>
  public class LocalDirectory : DefaultDirectory
  {
    private readonly string path;

    /// <summary>
    /// Make a shallow instance of the given local directory.
    /// </summary>
    /// <param name="path">Location of the directory.</param>
    internal LocalDirectory(string path) : base(null, new DirectoryInfo(path).Name)
    {
      this.path = path;
      if (File.Exists(path))
      {
        throw new ArgumentException("Given path must point to directory, not file.");
      }
      foreach(string f in Directory.EnumerateFiles(path))
      {
        AddFile(new LocalFile(this, f));
      }
    }
    
    public override bool TryGetDirectory(string name, out IDirectory dir)
    {
      if(dirs.TryGetValue(name, out dir))
      {
        return true;
      }
      else if(Directory.Exists(Path.Combine(path, name)))
      {
        dir = new LocalDirectory(Path.Combine(path, name));
        dirs.Add(name, dir);
        return true;
      }
      return false;
    }
  }
}
