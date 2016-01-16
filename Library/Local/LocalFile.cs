using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameArchives.Local
{
  /// <summary>
  /// Represents a file on the local filesystem.
  /// </summary>
  class LocalFile : IFile
  {
    public bool Compressed => false;
    public long CompressedSize => Size;
    public long Size { get; }
    public string Name { get; }
    public IDirectory Parent { get; }

    private string path;

    internal LocalFile(IDirectory parent, string path)
    {
      Parent = parent;
      this.path = path;
      Size = new FileInfo(path).Length;
      this.Name = Path.GetFileName(path);
    }

    public byte[] GetBytes()
    {
      return File.ReadAllBytes(path);
    }
    public Stream GetStream()
    {
      return File.OpenRead(path);
    }
  }
}
