using GameArchives.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameArchives.XISO
{
  /// <summary>
  /// Xbox (360) ISO Directory
  /// </summary>
  class XISODirectory : DefaultDirectory
  {
    public XISODirectory(IDirectory parent, string name) : base(parent, name)
    { }
  }
}
