// Unzip class usage example
// Written by Alexey Yakovlev <yallie@yandex.ru>

using System;
using System.Linq;

namespace Internals
{
	struct Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("Syntax: unzip Archive.zip TargetDirectory");
				return;
			}

			Extract(args.First(), args.Last());
		}

		private static void Extract(string archiveName, string targetDirectory)
		{
			using (var unzip = new Unzip(archiveName))
			{
				foreach (var entry in unzip.Entries.OrderBy(e => e.Name))
				{
					if (entry.IsFile)
					{
						Console.WriteLine("{0}: {1} -> {2}", entry.Name, entry.OriginalSize, entry.CompressedSize);
					}
					else
					{
						Console.WriteLine("{0}: **directory**", entry.Name);
					}
				}

				unzip.ExtractToDirectory(targetDirectory);
			}
		}
	}
}
