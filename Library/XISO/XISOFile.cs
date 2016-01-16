using GameArchives.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameArchives.XISO
{
  /// <summary>
  /// Xbox (360) ISO Files
  /// </summary>
  class XISOFile : IFile
  {
    public bool Compressed => false;

    public ulong CompressedSize => Size;

    public string Name { get; }

    public IDirectory Parent { get; }

    public ulong Size => (ulong)size;

    public byte[] GetBytes()
    {
      using (Stream s = GetStream())
        return s.ReadBytes((int)s.Length);
    }

    public Stream GetStream()
    {
      return new OffsetStream(iso, offset, size);
    }

    private Stream iso;
    private long offset;
    private long size;

    internal XISOFile(string name, IDirectory parent, Stream iso, long offset, long size)
    {
      Parent = parent;
      Name = name;
      this.size = size;
      this.iso = iso;
      this.offset = offset;
    }
  }
}
