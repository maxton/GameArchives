using GameArchives.Common;
using System.IO;

namespace GameArchives.FSGIMG
{
  class FSGIMGFile : IFile
  {
    public bool Compressed => false;
    public ulong CompressedSize => Size;
    public string Name { get; }
    public IDirectory Parent { get; }
    public ulong Size { get; }

    private Stream img_file;
    private ulong data_offset;

    public FSGIMGFile(string name, IDirectory parent, Stream img, ulong offset, ulong size)
    {
      Name = name;
      Parent = parent;
      Size = size;
      img_file = img;
      data_offset = offset;
    }

    public byte[] GetBytes()
    {
      //TODO: support sizes > 2GiB
      return GetStream().ReadBytes((int)Size);
    }

    public Stream GetStream()
    {
      return new OffsetStream(img_file, (long)data_offset, (long)Size);
    }
  }
}
