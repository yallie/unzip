Unzip
=====
https://github.com/yallie/unzip

Description
-----------

This is a tiny self-contained Unzip helper class for .NET Framework v.3.5 Client Profile.
To use it, simply copy the file and add it to your project.

Usage
-----

```C#
using (var unzip = new Unzip("exepack-1.0.zip"))
{
	// list all files
	foreach (var fileName in unzip.FileNames)
	{
		Console.WriteLine(fileName);
	}

	// extract single file to a specified location
	unzip.Extract("exepack.exe", "unpacked\exepack.exe");

	// extract file to a stream
	unzip.Extract("exepack.exe", stream);

	// extract all files from zip archive to a directory
	unzip.ExtractToDirectory(targetDirectory);
}
```