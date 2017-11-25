using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.IO.Compression;
using GameArchives.Common;

namespace GameArchives.PKF
{
  public class PKFFile : IFile
  {
    public long Size { get; }
    public long CompressedSize { get; }
    public bool Compressed { get; }

    public IDictionary<string, object> ExtendedInfo { get; }
    public Stream Stream => GetStream();
    public string Name { get; }
    public IDirectory Parent { get; }

    private long offset;
    private Stream archive;

    public PKFFile(string name, IDirectory parent, long size, bool compressed, long compressedSize, long offset, Stream archive)
    {
      Name = name;
      Parent = parent;
      Size = size;
      Compressed = compressed;
      CompressedSize = compressedSize;

      this.offset = offset;
      this.archive = archive;
      
      ExtendedInfo = new Dictionary<string, object>();
    }

    public byte[] GetBytes()
    {
      byte[] bytes = new byte[CompressedSize];
      if (CompressedSize > Int32.MaxValue)
        throw new NotSupportedException("Can't read bytes for file larger than int32 max, yet.");

      using (Stream stream = this.GetStream())
      {
        stream.Read(bytes, 0, (int)CompressedSize);
      }

      return bytes;
    }

    public Stream GetStream()
    {
      if (Compressed)
        return new DeflateStream(new OffsetStream(archive, offset + 2, CompressedSize - 2), CompressionMode.Decompress);
      else
        return new OffsetStream(archive, offset, CompressedSize);
    }
  }
}
