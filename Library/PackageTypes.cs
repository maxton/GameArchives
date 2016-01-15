using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameArchives
{
  class PackageType
  {
    /// <summary>
    /// The common name of this package type.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The common file extensions of this package type.
    /// </summary>
    /// <remarks>
    /// These should be of the format:
    /// *.ext
    /// As expected by an OpenFileDialog filter list.
    /// </remarks>
    public string[] Extensions { get; }

    /// <summary>
    /// Given a full path to a file, determines whether the file is
    /// of this package type.
    /// </summary>
    public Func<string, bool> CheckPath { get; }

    /// <summary>
    /// Given a stream which points to a file, determines whether the file
    /// is of this package type.
    /// </summary>
    public Func<Stream, bool> CheckStream { get; }

    /// <summary>
    /// Given a string which is the path to a valid package, opens it as this
    /// package type, returning the package instance.
    /// </summary>
    public Func<string, AbstractPackage> Load { get; }

    PackageType(string name, string[] extensions,
      Func<string, bool> path, Func<Stream, bool> stream, Func<string, AbstractPackage> load)
    {
      Name = name;
      Extensions = extensions;
      CheckPath = path;
      CheckStream = stream;
      Load = load;
    }

    public static PackageType[] Types =>
    new PackageType[] {
      new PackageType("Ark/Hdr Package", 
        new string[] { "*.hdr" }, 
        Ark.ArkPackage.IsArk,
        Ark.ArkPackage.IsArk,
        Ark.ArkPackage.FromPath),
      new PackageType("STFS Package",
        new string[] {"*.*"},
        STFS.STFSPackage.IsSTFS, 
        STFS.STFSPackage.IsSTFS, 
        STFS.STFSPackage.Open),
      new PackageType("FSAR Archive", 
        new string[] { "*.far" }, 
        FSAR.FSARPackage.IsFSAR, 
        FSAR.FSARPackage.IsFSAR, 
        FSAR.FSARPackage.FromFile),
      new PackageType("FSG-FILE-SYSTEM",
        new string[] { "*.img", "*.img.part000", "*.img.part0" },
        FSGIMG.FSGIMGPackage.IsFSGIMG,
        FSGIMG.FSGIMGPackage.IsFSGIMG,
        FSGIMG.FSGIMGPackage.OpenFile),
      //new PackageType("", new string[] { }, null, null, null)
    };
  }
}
