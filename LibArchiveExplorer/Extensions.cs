using GameArchives;
using System;
using System.Collections.Generic;

namespace LibArchiveExplorer
{
  static class Extensions
  {
    public static string HumanReadableFileSize(this long size)
    { 
      if (size > (1024 * 1024 * 1024))
      {
        return (size / (double)(1024 * 1024 * 1024)).ToString("F") + " GiB";
      }
      else if (size > (1024 * 1024))
      {
        return (size / (double)(1024 * 1024)).ToString("F") + " MiB";
      }
      else if (size > 1024)
      {
        return (size / 1024.0).ToString("F") + " KiB";
      }
      else
      {
        return size.ToString() + " B";
      }
    }

    public static bool IsSubclassOrEqual(this Type type, Type other)
    {
      return type.IsSubclassOf(other) || type == other;
    }

    public static int Clamp(this int value, int min, int max)
    {
      if (value < min) return min;
      if (value > max) return max;
      return value;
    }

    public static Tuple<long, long> GetLargestFreeBlock(this AbstractPackage pkg)
    {
      return new Tuple<long, long>(0, 0);
    }
  }
}
