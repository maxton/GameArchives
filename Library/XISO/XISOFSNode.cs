using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameArchives.XISO
{
  /// <summary>
  /// Represents an element of an XISO file system.
  /// </summary>
  public interface XISOFSNode : IFSNode
  {
    /// <summary>
    /// The location of the filesystem entry node in the ISO.
    /// </summary>
    long EntryLocation { get; }
  }
}
