Unzip
=====
https://github.com/yallie/unzip

Description
-----------

This is a tiny (~300 lines with comments) self-contained Unzip helper class for .NET Framework v3.5
Client Profile or Mono 2.10. To use it, simply copy the file and add it to your project. 

* For .NET Framework v4.5 projects, use built-in [System.IO.Compression.ZipArchive](http://msdn.microsoft.com/en-us/library/system.io.compression.ziparchive.aspx) instead.
* For Silverlight and Windows Phone projects, use [SharpGIS.UnZipper](http://nuget.org/packages/SharpGIS.UnZipper) Nuget package. 

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