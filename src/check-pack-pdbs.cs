// check-pack-pdbs.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// The pack path guard's SYMBOL-FILE reader (check-pack-paths.ps1 runs it through `dotnet`, so it works on any host the
// pack runs on, Windows PowerShell 5.1 included, which has no System.Reflection.Metadata of its own).
//
// A portable .pdb stores each document name as separator-joined PARTS, so no byte search sees a path whole: the names
// are DECODED here. Every .pdb entry of every .nupkg in the feed is read, and one line is printed per finding:
//   ABSOLUTE<TAB><nupkg><TAB><entry>      a document name is a drive-letter or UNC path
//   UNREADABLE<TAB><nupkg><TAB><entry>    the entry is not a readable portable .pdb
// then one closing line, READ<TAB><count of .pdb entries read>, so a caller can tell a clean read from no read.
// Never prints a path. Usage: dotnet run --file check-pack-pdbs.cs -- <feed folder>

// A file-based app defaults to Native AOT, whose restore needs the ILCompiler packages: a download on a host without
// them, and a failed build offline. This reader needs nothing beyond the shared framework.
#:property PublishAot=false

using System.IO.Compression;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;

if (args.Length != 1 || !Directory.Exists(args[0]))
{
    Console.Error.WriteLine("usage: check-pack-pdbs.cs <feed folder>");
    return 2;
}

Regex absolute = new(@"^(?:[A-Za-z]:[\\/]|\\\\)");
int read = 0;

foreach (string nupkg in Directory.GetFiles(args[0], "*.nupkg").Order(StringComparer.Ordinal))
{
    using ZipArchive zip = ZipFile.OpenRead(nupkg);

    foreach (ZipArchiveEntry entry in zip.Entries)
    {
        if (!entry.FullName.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase))
            continue;

        MemoryStream buffer = new();

        using (Stream stream = entry.Open())
            stream.CopyTo(buffer);

        buffer.Position = 0;
        read++;
        string verdict;

        try
        {
            using MetadataReaderProvider provider = MetadataReaderProvider.FromPortablePdbStream(buffer);
            MetadataReader reader = provider.GetMetadataReader();
            verdict = reader.Documents.Any(handle => absolute.IsMatch(reader.GetString(reader.GetDocument(handle).Name))) ? "ABSOLUTE" : "";
        }
        catch (BadImageFormatException)
        {
            verdict = "UNREADABLE";
        }

        if (verdict.Length > 0)
            Console.WriteLine($"{verdict}\t{Path.GetFileName(nupkg)}\t{entry.FullName}");
    }
}

Console.WriteLine($"READ\t{read}");
return 0;
