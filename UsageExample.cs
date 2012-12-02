// Unzip class usage example
// Written by Alexey Yakovlev <yallie@yandex.ru>

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
				ListFiles(unzip);

				unzip.ExtractToDirectory(outputDirectory);
			}
		}

		private static void ListFiles(Unzip unzip)
		{
			var tab = unzip.Entries.Where(e => e.IsDirectory).Any() ? "\t" : string.Empty;

			foreach (var entry in unzip.Entries.OrderBy(e => e.Name))
			{
				if (entry.IsFile)
				{
					Console.WriteLine(tab + "{0}: {1} -> {2}", entry.Name, entry.CompressedSize, entry.OriginalSize);
					continue;
				}

				// directory
				Console.WriteLine(entry.Name);
			}
		}
	}
}
