Unzip
=====
* http://nuget.org/packages/unzip
* https://github.com/yallie/unzip

Description
-----------

This is a tiny (~300 lines with comments) self-contained Unzip helper class for .NET Framework v3.5
Client Profile or Mono 2.10. To use it, simply include Unzip.cs into your C# project or install Unzip package from Nuget:

* [Install-Package Unzip](http://nuget.org/packages/unzip)

Usage
-----

```C#
using (var unzip = new Unzip("zyan-sources.zip"))
{
	// list all files in the archive
	foreach (var fileName in unzip.FileNames)
	{
		Console.WriteLine(fileName);
	}

	// extract single file to a specified location
	unzip.Extract(@"source\Zyan.Communication\ZyanConnection.cs", "test.cs");

	// extract file to a stream
	unzip.Extract(@"source\Zyan.Communication\ZyanProxy.cs", stream);

	// extract all files from zip archive to a directory
	unzip.ExtractToDirectory(outputDirectory);
}
```

Alternatives
------------

* [System.IO.Compression.ZipArchive](http://msdn.microsoft.com/en-us/library/system.io.compression.ziparchive.aspx) for .NET Framework v4.5 projects.
* [SharpGIS.UnZipper](http://nuget.org/packages/SharpGIS.UnZipper) Nuget package for Silverlight and Windows Phone projects. MS-PL. ~8k. 
* [ZipStorer](http://zipstorer.codeplex.com/) library for zip compression and decompression. MS-PL. ~33k.

Full-featured libraries
-----------------------

* [SharpZipLib](http://www.icsharpcode.net/opensource/sharpziplib/) supports Zip, GZip, Tar and BZip2 archives. GPL. ~200k.
* [DotNetZip](http://dotnetzip.codeplex.com/) supports Silverlight and Compact framework, AES encryption. MS-PL. ~250-480k.
* [SharpCompress](http://sharpcompress.codeplex.com/) supports Zip, Gzip, Tar, Rar and 7z. MS-PL. ~440K.

Thanks
------

* [Yannic Staudt](https://github.com/pysco68) for contributing a bugfix
* [Per Lundberg](https://github.com/perlun) for several contributions
* [Damien Guard](https://github.com/damieng) for CRC32 computation algorithm
