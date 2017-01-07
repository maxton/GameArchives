/*
 * PFSFile.cs
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
using System.IO;
using GameArchives.Common;

namespace GameArchives.PFS
{
  public class PFSFile : OffsetFile
  {
    public long InodeIdx => (long) ExtendedInfo["InodeIdx"];
    public PFSFile(string name, IDirectory parent, Stream img, long offset, long size, long inodeIdx) 
      : base(name, parent, img, offset, size)
    {
      ExtendedInfo.Add("InodeIdx", inodeIdx);
    }
  }
}
