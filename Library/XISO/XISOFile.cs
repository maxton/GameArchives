using System.Collections.Generic;
using System.IO;
using GameArchives.Common;

namespace GameArchives.XISO
{
  public class XISOFile : OffsetFile, XISOFSNode
  {
    public long EntryLocation => (long) ExtendedInfo["EntryLocation"];
    public long DataLocation => (long)ExtendedInfo["DataLocation"];
    public XISOFile(string name, IDirectory parent, Stream img, long offset, long size, long entryLocation) 
      : base(name, parent, img, offset, size)
    {
      ExtendedInfo.Add("EntryLocation", entryLocation);
      ExtendedInfo.Add("DataLocation", offset);
    }

    internal void UpdateSize(long newSize)
    {
      Size = newSize;
    }
  }
}
