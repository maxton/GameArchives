/*
 * OffsetFile.cs
 * 
 * Copyright (c) 2016, maxton. All rights reserved.
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3.0 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; If not, see
 * <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameArchives.Common
{
  /// <summary>
  /// An uncompressed file which is simply a number of bytes at a certain offset in a stream.
  /// </summary>
  public class OffsetFile : IFile
  {
    public bool Compressed => false;
    public long CompressedSize => Size;
    public IDictionary<string, object> ExtendedInfo { get; }
    public string Name { get; }
    public IDirectory Parent { get; }
    public long Size { get; protected set; }
    public Stream Stream => GetStream();

    public long DataLocation => data_offset;

    private Stream img_file;
    private long data_offset;
    private const int BUFFER_SIZE = 8192;

    /// <summary>
    /// Constructs a new OffsetFile
    /// </summary>
    /// <param name="name">The name of the file, including extension</param>
    /// <param name="parent">The directory in which this file resides</param>
    /// <param name="img">Stream which contains this file</param>
    /// <param name="offset">Offset into the stream at which the file starts</param>
    /// <param name="size">Length in bytes of the file</param>
    public OffsetFile(string name, IDirectory parent, Stream img, long offset, long size)
    {
      Name = name;
      Parent = parent;
      Size = size;
      img_file = img;
      data_offset = offset;
      ExtendedInfo = new Dictionary<string, object>();
    }

    public byte[] GetBytes()
    {
      byte[] bytes;
      using (var stream = this.GetStream())
      {
        bytes = stream.ReadBytes((int)Size);
      }
      return bytes;
    }

    public Stream GetStream()
    {
      return new BufferedStream(new OffsetStream(img_file, data_offset, Size), BUFFER_SIZE);
    }
  }
}
