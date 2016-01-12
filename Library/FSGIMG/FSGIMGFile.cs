/*
 * FSGIMGFile.cs
 * 
 * Copyright (c) 2015,2016, maxton. All rights reserved.
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
