// Unzip class for .NET 3.5 Client Profile or Mono 2.10
// Written by Alexey Yakovlev <yallie@yandex.ru>
// https://github.com/yallie/unzip

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Internals
{
	/// <summary>
	/// Unzip helper class.
	/// </summary>
	internal class Unzip : IDisposable
	{
		private const int EntrySignature = 0x02014B50;

		private const int FileSignature = 0x04034b50;

		private const int DirectorySignature = 0x06054B50;

		private const int BufferSize = 16 * 1024;

		/// <summary>
		/// Zip archive entry.
		/// </summary>
		public class Entry
		{
			/// <summary>
			/// Gets or sets the name of a file or a directory.
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			/// Gets or sets the comment.
			/// </summary>
			public string Comment { get; set; }

			/// <summary>
			/// Gets or sets the CRC32.
			/// </summary>
			public int Crc32 { get; set; }

			/// <summary>
			/// Gets or sets the compressed size of the file.
			/// </summary>
			public int CompressedSize { get; set; }

			/// <summary>
			/// Gets or sets the original size of the file.
			/// </summary>
			public int OriginalSize { get; set; }

			/// <summary>
			/// Gets or sets a value indicating whether this <see cref="Entry" /> is deflated.
			/// </summary>
			public bool Deflated { get; set; }

			/// <summary>
			/// Gets a value indicating whether this <see cref="Entry" /> is a directory.
			/// </summary>
			public bool IsDirectory { get { return Name.EndsWith("/"); } }

			/// <summary>
			/// Gets or sets the timestamp.
			/// </summary>
			public DateTime Timestamp { get; set; }

			/// <summary>
			/// Gets a value indicating whether this <see cref="Entry" /> is a file.
			/// </summary>
			public bool IsFile { get { return !IsDirectory; } }

			[EditorBrowsable(EditorBrowsableState.Never)]
			public int HeaderOffset { get; set; }

			[EditorBrowsable(EditorBrowsableState.Never)]
			public int DataOffset { get; set; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Unzip" /> class.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		public Unzip(string fileName)
			: this(File.OpenRead(fileName))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Unzip" /> class.
		/// </summary>
		/// <param name="stream">The stream.</param>
		public Unzip(Stream stream)
		{
			Stream = stream;
			Reader = new BinaryReader(Stream);
		}

		private Stream Stream { get; set; }

		private BinaryReader Reader { get; set; }

		/// <summary>
		/// Performs application-defined tasks associated with
		/// freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			if (Stream != null)
			{
				Stream.Dispose();
				Stream = null;
			}

			if (Reader != null)
			{
				Reader.Close();
				Reader = null;
			}
		}

		/// <summary>
		/// Extracts the contents of the zip file to the given directory.
		/// </summary>
		/// <param name="directoryName">Name of the directory.</param>
		public void ExtractToDirectory(string directoryName)
		{
			foreach (var entry in Entries.Where(e => !e.IsDirectory))
			{
				// create target directory for the file
				var fileName = Path.Combine(directoryName, entry.Name);
				var dirName = Path.GetDirectoryName(fileName);
				Directory.CreateDirectory(dirName);

				// save file
				Extract(entry.Name, fileName);
			}
		}

		/// <summary>
		/// Extracts the specified file to the specified name.
		/// </summary>
		/// <param name="fileName">Name of the file in zip archive.</param>
		/// <param name="outputFileName">Name of the output file.</param>
		public void Extract(string fileName, string outputFileName)
		{
			var entry = GetEntry(fileName);

			using (var outStream = File.Create(outputFileName))
			{
				Extract(entry, outStream);
			}

			File.SetLastWriteTime(outputFileName, entry.Timestamp);
		}

		private Entry GetEntry(string fileName)
		{
			fileName = fileName.Replace("\\", "/").Trim().TrimStart('/');
			var entry = Entries.Where(e => e.Name == fileName).FirstOrDefault();

			if (entry == null)
			{
				throw new FileNotFoundException("File not found in the archive: " + fileName);
			}

			return entry;
		}

		/// <summary>
		/// Extracts the specified file to the output <see cref="Stream"/>.
		/// </summary>
		/// <param name="fileName">Name of the file in zip archive.</param>
		/// <param name="outputStream">The output stream.</param>
		public void Extract(string fileName, Stream outputStream)
		{
			Extract(GetEntry(fileName), outputStream);
		}

		/// <summary>
		/// Extracts the specified entry.
		/// </summary>
		/// <param name="entry">Zip file entry to extract.</param>
		/// <param name="outputStream">The stream to write the data to.</param>
		/// <exception cref="System.InvalidOperationException"> is thrown when the file header signature doesn't match.</exception>
		public void Extract(Entry entry, Stream outputStream)
		{
			// check file signature
			Stream.Seek(entry.HeaderOffset, SeekOrigin.Begin);
			if (Reader.ReadInt32() != FileSignature)
			{
				throw new InvalidOperationException("File signature doesn't match.");
			}

			// move to file data
			Stream.Seek(entry.DataOffset, SeekOrigin.Begin);
			var inputStream = Stream;
			if (entry.Deflated)
			{
				inputStream = new DeflateStream(Stream, CompressionMode.Decompress, true);
			}

			// allocate buffer
			var count = entry.OriginalSize;
			var bufferSize = Math.Min(BufferSize, entry.OriginalSize);
			var buffer = new byte[bufferSize];

			while (count > 0)
			{
				// decompress data
				var read = inputStream.Read(buffer, 0, bufferSize);
				if (read == 0)
				{
					break;
				}

				// copy to the output stream
				outputStream.Write(buffer, 0, read);
				count -= read;
			}
		}

		/// <summary>
		/// Gets the file names.
		/// </summary>
		public IEnumerable<string> FileNames
		{
			get
			{
				return Entries.Select(e => e.Name).Where(f => !f.EndsWith("/")).OrderBy(f => f);
			}
		}

		private Entry[] entries;

		/// <summary>
		/// Gets zip file entries.
		/// </summary>
		public IEnumerable<Entry> Entries
		{
			get
			{
				if (entries == null)
				{
					entries = ReadZipEntries().ToArray();
				}

				return entries;
			}
		}

		private IEnumerable<Entry> ReadZipEntries()
		{
			if (Stream.Length < 22)
			{
				yield break;
			}

			Stream.Seek(-22, SeekOrigin.End);

			// find directory signature
			while (Reader.ReadInt32() != DirectorySignature)
			{
				if (Stream.Position <= 5)
				{
					yield break;
				}

				// move 1 byte back
				Stream.Seek(-5, SeekOrigin.Current);
			}

			// read directory properties
			Stream.Seek(6, SeekOrigin.Current);
			var entries = Reader.ReadUInt16();
			var difSize = Reader.ReadInt32();
			var dirOffset = Reader.ReadUInt32();
			Stream.Seek(dirOffset, SeekOrigin.Begin);

			// read directory entries
			for (int i = 0; i < entries; i++)
			{
				if (Reader.ReadInt32() != EntrySignature)
				{
					continue;
				}

				// read file properties
				Reader.ReadInt32();
				bool utf8 = (Reader.ReadInt16() & 0x0800) != 0;
				short method = Reader.ReadInt16();
				int timestamp = Reader.ReadInt32();
				int crc32 = Reader.ReadInt32();
				int compressedSize = Reader.ReadInt32();
				int fileSize = Reader.ReadInt32();
				short fileNameSize = Reader.ReadInt16();
				short extraSize = Reader.ReadInt16();
				short commentSize = Reader.ReadInt16();
				int headerOffset = Reader.ReadInt32();
				Reader.ReadInt32();
				int fileHeaderOffset = Reader.ReadInt32();
				var fileNameBytes = Reader.ReadBytes(fileNameSize);
				Stream.Seek(extraSize, SeekOrigin.Current);
				var fileCommentBytes = Reader.ReadBytes(commentSize);
				var fileDataOffset = CalculateFileDataOffset(fileHeaderOffset);

				// decode zip file entry
				var encoder = utf8 ? Encoding.UTF8 : Encoding.Default;
				yield return new Entry
				{
					Name = encoder.GetString(fileNameBytes),
					Comment = encoder.GetString(fileCommentBytes),
					Crc32 = crc32,
					CompressedSize = compressedSize,
					OriginalSize = fileSize,
					HeaderOffset = fileHeaderOffset,
					DataOffset = fileDataOffset,
					Deflated = method == 8,
					Timestamp = ConvertToDateTime(timestamp)
				};
			}
		}

		private int CalculateFileDataOffset(int fileHeaderOffset)
		{
			var position = Stream.Position;
			Stream.Seek(fileHeaderOffset + 26, SeekOrigin.Begin);
			var fileNameSize = Reader.ReadInt16();
			var extraSize = Reader.ReadInt16();

			var fileOffset = (int)Stream.Position + fileNameSize + extraSize;
			Stream.Seek(position, SeekOrigin.Begin);
			return fileOffset;
		}

		/// <summary>
		/// Converts DOS timestamp to a <see cref="DateTime"/> instance.
		/// </summary>
		/// <param name="dosTimestamp">The dos timestamp.</param>
		/// <returns>The <see cref="DateTime"/> instance.</returns>
		public static DateTime ConvertToDateTime(int dosTimestamp)
		{
			return new DateTime((dosTimestamp >> 25) + 1980, (dosTimestamp >> 21) & 15, (dosTimestamp >> 16) & 31,
				(dosTimestamp >> 11) & 31, (dosTimestamp >> 5) & 63, (dosTimestamp & 31) * 2);
		}
	}
}