// Unzip class usage example
// Written by Alexey Yakovlev <yallie@yandex.ru>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Internals
{
	struct Program
	{
		static void Main()
		{
			using (var unzip = new Unzip("zssample.zip"))
			{
				Console.WriteLine("Files: {0}", string.Join(", ", unzip.FileNames.ToArray()));
   
				foreach (var entry in unzip.Entries.Where(e => !e.IsDirectory))
				{
					Console.WriteLine("{0}: {1} -> {2}", entry.FileName, entry.FileSize, entry.CompressedSize);
   
					var fileName = Path.Combine("Unzipped", entry.FileName);
					var dirName = Path.GetDirectoryName(fileName);
					Directory.CreateDirectory(dirName);
   
					using (var outStream = File.Create(fileName))
					{
						unzip.Decompress(entry.FileName, outStream);
					}
				}
			}
		}
	}
}
