using System;
using GameArchives.Common;

namespace GameArchives.FSGIMG
{
  class FSGIMGDirectory : DefaultDirectory
  {
    public long Offset { get; }
    public FSGIMGDirectory(IDirectory parent, string filename, long offset) : base(parent, filename)
    {
      Offset = offset;
    }
  }
}
