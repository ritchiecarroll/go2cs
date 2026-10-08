// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package txtar implements a trivial text-based file archive format.
//
// The goals for the format are:
//
//   - be trivial enough to create and edit by hand.
//   - be able to store trees of text files describing go command test cases.
//   - diff nicely in git history and code reviews.
//
// Non-goals include being a completely general archive format,
// storing binary data, storing file modes, storing special files like
// symbolic links, and so on.
//
// # Txtar format
//
// A txtar archive is zero or more comment lines and then a sequence of file entries.
// Each file entry begins with a file marker line of the form "-- FILENAME --"
// and is followed by zero or more file content lines making up the file data.
// The comment or file content ends at the next file marker line.
// The file marker line must begin with the three-byte sequence "-- "
// and end with the three-byte sequence " --", but the enclosed
// file name can be surrounding by additional white space,
// all of which is stripped.
//
// If the txtar file is missing a trailing newline on the final line,
// parsers should consider a final newline to be present anyway.
//
// There are no possible syntax errors in a txtar archive.
namespace go.@internal;

using bytes = bytes_package;
using fmt = fmt_package;
using os = os_package;
using strings = strings_package;
using io = io_package;

partial class txtar_package {

// An Archive is a collection of files.
partial struct Archive {
    public slice<byte> Comment;
    public slice<File> Files;
}

// A File is a single file in an archive.
partial struct File {
    public @string Name; // name of file ("foo/bar.txt")
    public slice<byte> Data; // text content of file
}

// Format returns the serialized form of an Archive.
// It is assumed that the Archive data structure is well-formed:
// a.Comment and all a.File[i].Data contain no file marker lines,
// and all a.File[i].Name is non-empty.
public static slice<byte> Format(ж<Archive> Ꮡa) {
    ref var a = ref Ꮡa.DerefOrNull();

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    buf.Write(fixNL(a.Comment));
    foreach (var (_, f) in a.Files) {
        fmt.Fprintf(new bytes_BufferжWriter(Ꮡbuf), "-- %s --\n"u8, f.Name);
        buf.Write(fixNL(f.Data));
    }
    return buf.Bytes();
}

// ParseFile parses the named file as an archive.
public static (ж<Archive>, error) ParseFile(@string @file) {
    var (data, err) = os.ReadFile(@file);
    if (err != default!) {
        return (default!, err);
    }
    return (Parse(data), default!);
}

// Parse parses the serialized form of an Archive.
// The returned Archive holds slices of data.
public static ж<Archive> Parse(slice<byte> data) {
    var a = @new<Archive>();
    @string name = default!;
    (a.Value.Comment, name, data) = findFileMarker(data);
    while (name != ""u8) {
        var f = new File(name, default!);
        (f.Data, name, data) = findFileMarker(data);
        a.Value.Files = append((~a).Files, f);
    }
    return a;
}

internal static slice<byte> newlineMarker = slice<byte>("\n-- "u8);
internal static slice<byte> marker = slice<byte>("-- "u8);
internal static slice<byte> markerEnd = slice<byte>(" --"u8);

// findFileMarker finds the next file marker in data,
// extracts the file name, and returns the data before the marker,
// the file name, and the data after the marker.
// If there is no next marker, findFileMarker returns before = fixNL(data), name = "", after = nil.
internal static (slice<byte> before, @string name, slice<byte> after) findFileMarker(slice<byte> data) {
    @string name = default!;
    slice<byte> after = default!;

    nint i = default!;
    while (ᐧ) {
        {
            (name, after) = isMarker(data.slice(i)); if (name != ""u8) {
                return (data.slice(0, i), name, after);
            }
        }
        nint j = bytes.Index(data.slice(i), newlineMarker);
        if (j < 0) {
            return (fixNL(data), "", default!);
        }
        i += j + 1; // positioned at start of new possible marker
    }
}

// isMarker checks whether data begins with a file marker line.
// If so, it returns the name from the line and the data after the line.
// Otherwise it returns name == "" with an unspecified after.
internal static (@string name, slice<byte> after) isMarker(slice<byte> data) {
    slice<byte> after = default!;

    if (!bytes.HasPrefix(data, marker)) {
        return ("", default!);
    }
    {
        nint i = bytes.IndexByte(data, (rune)'\n'); if (i >= 0) {
            (data, after) = (data.slice(0, i), data.slice(i + 1));
        }
    }
    if (!(bytes.HasSuffix(data, markerEnd) && len(data) >= len(marker) + len(markerEnd))) {
        return ("", default!);
    }
    return (strings.TrimSpace(((@string)(data.slice(len(marker), len(data) - len(markerEnd))))), after);
}

// If data is empty or ends in \n, fixNL returns data.
// Otherwise fixNL returns a new slice consisting of data with a final \n added.
internal static slice<byte> fixNL(slice<byte> data) {
    if (len(data) == 0 || data[len(data) - 1] == (rune)'\n') {
        return data;
    }
    var d = new slice<byte>(len(data) + 1);
    copy(d, data);
    d[len(data)] = (rune)'\n';
    return d;
}

} // end txtar_package
