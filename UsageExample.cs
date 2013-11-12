// Unzip class usage example
// Written by Alexey Yakovlev <yallie@yandex.ru>
// https://github.com/yallie/unzip

using System;
using System.Linq;

namespace Internals
{
	internal struct Program
	{
		private static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("Syntax: unzip Archive.zip TargetDirectory");
				return;
			}

			var archiveName = args.First();
			var outputDirectory = args.Last();

			using (var unzip = new Unzip(archiveName))
			{
				Console.WriteLine("Listing files in the archive:");
				ListFiles(unzip);

				Console.WriteLine("Extracting files from the archive:");
				unzip.ExtractProgress += (s, e) => Console.WriteLine("{0} of {1}: {2}", e.CurrentFile, e.TotalFiles, e.FileName);
				unzip.ExtractToDirectory(outputDirectory);
			}
		}

		private static void ListFiles(Unzip unzip)
		{
			var tab = unzip.Entries.Any(e => e.IsDirectory) ? "\t" : string.Empty;

			foreach (var entry in unzip.Entries.OrderBy(e => e.Name))
			{
				if (entry.IsFile)
				{
					Console.WriteLine(tab + "{0}: {1} -> {2}", entry.Name, entry.CompressedSize, entry.OriginalSize);
					continue;
				}

				Console.WriteLine(entry.Name);
			}

			Console.WriteLine();
		}
	}
}
