// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package embed provides access to files embedded in the running Go program.
//
// Go source files that import "embed" can use the //go:embed directive
// to initialize a variable of type string, []byte, or [FS] with the contents of
// files read from the package directory or subdirectories at compile time.
//
// For example, here are three ways to embed a file named hello.txt
// and then print its contents at run time.
//
// Embedding one file into a string:
//
//	import _ "embed"
//
//	//go:embed hello.txt
//	var s string
//	print(s)
//
// Embedding one file into a slice of bytes:
//
//	import _ "embed"
//
//	//go:embed hello.txt
//	var b []byte
//	print(string(b))
//
// Embedded one or more files into a file system:
//
//	import "embed"
//
//	//go:embed hello.txt
//	var f embed.FS
//	data, _ := f.ReadFile("hello.txt")
//	print(string(data))
//
// # Directives
//
// A //go:embed directive above a variable declaration specifies which files to embed,
// using one or more path.Match patterns.
//
// The directive must immediately precede a line containing the declaration of a single variable.
// Only blank lines and ‘//’ line comments are permitted between the directive and the declaration.
//
// The type of the variable must be a string type, or a slice of a byte type,
// or [FS] (or an alias of [FS]).
//
// For example:
//
//	package server
//
//	import "embed"
//
//	// content holds our static web server content.
//	//go:embed image/* template/*
//	//go:embed html/index.html
//	var content embed.FS
//
// The Go build system will recognize the directives and arrange for the declared variable
// (in the example above, content) to be populated with the matching files from the file system.
//
// The //go:embed directive accepts multiple space-separated patterns for
// brevity, but it can also be repeated, to avoid very long lines when there are
// many patterns. The patterns are interpreted relative to the package directory
// containing the source file. The path separator is a forward slash, even on
// Windows systems. Patterns may not contain ‘.’ or ‘..’ or empty path elements,
// nor may they begin or end with a slash. To match everything in the current
// directory, use ‘*’ instead of ‘.’. To allow for naming files with spaces in
// their names, patterns can be written as Go double-quoted or back-quoted
// string literals.
//
// If a pattern names a directory, all files in the subtree rooted at that directory are
// embedded (recursively), except that files with names beginning with ‘.’ or ‘_’
// are excluded. So the variable in the above example is almost equivalent to:
//
//	// content is our static web server content.
//	//go:embed image template html/index.html
//	var content embed.FS
//
// The difference is that ‘image/*’ embeds ‘image/.tempfile’ while ‘image’ does not.
// Neither embeds ‘image/dir/.tempfile’.
//
// If a pattern begins with the prefix ‘all:’, then the rule for walking directories is changed
// to include those files beginning with ‘.’ or ‘_’. For example, ‘all:image’ embeds
// both ‘image/.tempfile’ and ‘image/dir/.tempfile’.
//
// The //go:embed directive can be used with both exported and unexported variables,
// depending on whether the package wants to make the data available to other packages.
// It can only be used with variables at package scope, not with local variables.
//
// Patterns must not match files outside the package's module, such as ‘.git/*’, symbolic links,
// 'vendor/', or any directories containing go.mod (these are separate modules).
// Patterns must not match files whose names include the special punctuation characters  " * < > ? ` ' | / \ and :.
// Matches for empty directories are ignored. After that, each pattern in a //go:embed line
// must match at least one file or non-empty directory.
//
// If any patterns are invalid or have invalid matches, the build will fail.
//
// # Strings and Bytes
//
// The //go:embed line for a variable of type string or []byte can have only a single pattern,
// and that pattern can match only a single file. The string or []byte is initialized with
// the contents of that file.
//
// The //go:embed directive requires importing "embed", even when using a string or []byte.
// In source files that don't refer to [embed.FS], use a blank import (import _ "embed").
//
// # File Systems
//
// For embedding a single file, a variable of type string or []byte is often best.
// The [FS] type enables embedding a tree of files, such as a directory of static
// web server content, as in the example above.
//
// FS implements the [io/fs] package's [FS] interface, so it can be used with any package that
// understands file systems, including [net/http], [text/template], and [html/template].
//
// For example, given the content variable in the example above, we can write:
//
//	http.Handle("/static/", http.StripPrefix("/static/", http.FileServer(http.FS(content))))
//
//	template.ParseFS(content, "*.tmpl")
//
// # Tools
//
// To support tools that analyze Go packages, the patterns found in //go:embed lines
// are available in “go list” output. See the EmbedPatterns, TestEmbedPatterns,
// and XTestEmbedPatterns fields in the “go help list” output.
namespace go;

using errors = errors_package;
using bytealg = @internal.bytealg_package;
using stringslite = @internal.stringslite_package;
using Δio = io_package;
using fs = go.io.fs_package;
using time = time_package;
using @internal;
using go.io;

partial class embed_package {

// An FS is a read-only collection of files, usually initialized with a //go:embed directive.
// When declared without a //go:embed directive, an FS is an empty file system.
//
// An FS is a read-only value, so it is safe to use from multiple goroutines
// simultaneously and also safe to assign values of type FS to each other.
//
// FS implements fs.FS, so it can be used with any package that understands
// file system interfaces, including net/http, text/template, and html/template.
//
// See the package documentation for more details about initializing an FS.
[GoType] partial struct FS {
    // The compiler knows the layout of this struct.
    // See cmd/compile/internal/staticdata's WriteEmbed.
    //
    // The files list is sorted by name but not by simple string comparison.
    // Instead, each file's name takes the form "dir/elem" or "dir/elem/".
    // The optional trailing slash indicates that the file is itself a directory.
    // The files list is sorted first by dir (if dir is missing, it is taken to be ".")
    // and then by base, so this list of files:
    //
    //	p
    //	q/
    //	q/r
    //	q/s/
    //	q/s/t
    //	q/s/u
    //	q/v
    //	w
    //
    // is actually sorted as:
    //
    //	p       # dir=.    elem=p
    //	q/      # dir=.    elem=q
    //	w       # dir=.    elem=w
    //	q/r     # dir=q    elem=r
    //	q/s/    # dir=q    elem=s
    //	q/v     # dir=q    elem=v
    //	q/s/t   # dir=q/s  elem=t
    //	q/s/u   # dir=q/s  elem=u
    //
    // This order brings directory contents together in contiguous sections
    // of the list, allowing a directory read to use binary search to find
    // the relevant sequence of entries.
    internal ж<slice<@file>> files;
}

// split splits the name into dir and elem as described in the
// comment in the FS struct above. isDir reports whether the
// final trailing slash was present, indicating that name is a directory.
internal static (@string dir, @string elem, bool isDir) split(@string name) {
    bool isDir = default!;

    (name, isDir) = stringslite.CutSuffix(name, "/"u8);
    nint i = bytealg.LastIndexByteString(name, (rune)'/');
    if (i < 0) {
        return (".", name, isDir);
    }
    return (name.slice(0, i), name.slice(i + 1), isDir);
}

internal static fs.ReadDirFS _ᴛ1ʗ = new FS(nil);
internal static fs.ReadFileFS _ᴛ2ʗ = new FS(nil);

// A file is a single file in the FS.
// It implements fs.FileInfo and fs.DirEntry.
[GoType] partial struct @file {
    // The compiler knows the layout of this struct.
    // See cmd/compile/internal/staticdata's WriteEmbed.
    internal @string name;
    internal @string data;
    internal array<byte> hash = new(16); // truncated SHA256 hash
}

internal static fs.FileInfo _ᴛ3ʗ = new fileжFileInfo(((ж<@file>)nil));
internal static fs.DirEntry _ᴛ4ʗ = new fileжDirEntry(((ж<@file>)nil));

[GoRecv] internal static @string Name(this ref @file f) {
    var (_, elem, _) = split(f.name);
    return elem;
}

[GoRecv] internal static int64 Size(this ref @file f) {
    return (int64)len(f.data);
}

[GoRecv] internal static time.Time ModTime(this ref @file f) {
    return new time.Time(nil);
}

[GoRecv] internal static bool IsDir(this ref @file f) {
    var (_, _, isDir) = split(f.name);
    return isDir;
}

[GoRecv] internal static any Sys(this ref @file f) {
    return default!;
}

[GoRecv] internal static fs.FileMode Type(this ref @file f) {
    return f.Mode().Type();
}

internal static (fs.FileInfo, error) Info(this ж<@file> Ꮡf) {
    return (new fileжFileInfo(Ꮡf), default!);
}

[GoRecv] internal static fs.FileMode Mode(this ref @file f) {
    if (f.IsDir()) {
        return (fs.FileMode)(fs.ModeDir | 365);
    }
    return 292;
}

internal static @string String(this ж<@file> Ꮡf) {
    return fs.FormatFileInfo(new fileжFileInfo(Ꮡf));
}

// dotFile is a file for the root directory,
// which is omitted from the files list in a FS.
internal static ж<@file> dotFile = Ꮡ(new @file(name: "./"u8));

// lookup returns the named file, or nil if it is not present.
internal static ж<@file> lookup(this FS f, @string name) {
    if (!fs.ValidPath(name)) {
        // The compiler should never emit a file with an invalid name,
        // so this check is not strictly necessary (if name is invalid,
        // we shouldn't find a match below), but it's a good backstop anyway.
        return default!;
    }
    if (name == "."u8) {
        return dotFile;
    }
    if (f.files == nil) {
        return default!;
    }
    // Binary search to find where name would be in the list,
    // and then check if name is at that position.
    var (dir, elem, _) = split(name);
    var files = f.files.ValueSlot;
    var filesʗ1 = files;
    nint i = sortSearch(len(files), (nint iΔ1) => {
        var (idir, ielem, _) = split(filesʗ1[iΔ1].name);
        return idir > dir || idir == dir && ielem >= elem;
    });
    if (i < len(files) && stringslite.TrimSuffix(files[i].name, "/"u8) == name) {
        return Ꮡ(files, i);
    }
    return default!;
}

// readDir returns the list of files corresponding to the directory dir.
internal static slice<@file> readDir(this FS f, @string dir) {
    if (f.files == nil) {
        return default!;
    }
    // Binary search to find where dir starts and ends in the list
    // and then return that slice of the list.
    var files = f.files.ValueSlot;
    var filesʗ1 = files;
    nint i = sortSearch(len(files), (nint iΔ1) => {
        var (idir, _, _) = split(filesʗ1[iΔ1].name);
        return idir >= dir;
    });
    var filesʗ2 = files;
    nint j = sortSearch(len(files), (nint jΔ1) => {
        var (jdir, _, _) = split(filesʗ2[jΔ1].name);
        return jdir > dir;
    });
    return files.slice(i, j);
}

// Open opens the named file for reading and returns it as an [fs.File].
//
// The returned file implements [io.Seeker] and [io.ReaderAt] when the file is not a directory.
public static (fs.File, error) Open(this FS f, @string name) {
    var @file = f.lookup(name);
    if (@file == nil) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new fs.PathError(Op: "open"u8, Path: name, Err: fs.ErrNotExist))));
    }
    if (@file.IsDir()) {
        return (new openDirжFile(Ꮡ(new openDir(@file, f.readDir(name), 0))), default!);
    }
    return (new openFileжFile(Ꮡ(new openFile(@file, 0))), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string notADirectoryˢ = "not a directory"u8;

// ReadDir reads and returns the entire named directory.
public static (slice<fs.DirEntry>, error) ReadDir(this FS f, @string name) {
    var (@file, err) = f.Open(name);
    if (err != default!) {
        return (default!, err);
    }
    var (dir, ok) = @file._<ж<openDir>>(ᐧ);
    if (!ok) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new fs.PathError(Op: "read"u8, Path: name, Err: errors.New(notADirectoryˢ)))));
    }
    var list = new slice<fs.DirEntry>(len((~dir).files));
    foreach (var (i, _) in list) {
        list[i] = new fileжDirEntry(Ꮡ((~dir).files, i));
    }
    return (list, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string isADirectoryˢ = "is a directory"u8;

// ReadFile reads and returns the content of the named file.
public static (slice<byte>, error) ReadFile(this FS f, @string name) {
    var (@file, err) = f.Open(name);
    if (err != default!) {
        return (default!, err);
    }
    var (ofile, ok) = @file._<ж<openFile>>(ᐧ);
    if (!ok) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new fs.PathError(Op: "read"u8, Path: name, Err: errors.New(isADirectoryˢ)))));
    }
    return (slice<byte>((~(~ofile).f).data), default!);
}

// An openFile is a regular file open for reading.
[GoType] partial struct openFile {
    internal ж<@file> f; // the file itself
    internal int64 offset; // current read offset
}

internal static Δio.Seeker _ᴛ5ʗ = new openFileжSeeker(((ж<openFile>)nil));
internal static Δio.ReaderAt _ᴛ6ʗ = new openFileжReaderAt(((ж<openFile>)nil));

[GoRecv] internal static error Close(this ref openFile f) {
    return default!;
}

[GoRecv] internal static (fs.FileInfo, error) Stat(this ref openFile f) {
    return (new fileжFileInfo(f.f), default!);
}

[GoRecv] internal static (nint, error) Read(this ref openFile f, slice<byte> b) {
    if (f.offset >= (int64)len((~f.f).data)) {
        return (0, Δio.EOF);
    }
    if (f.offset < 0) {
        return (0, new fs.PathErrorжerror(Ꮡ(new fs.PathError(Op: "read"u8, Path: (~f.f).name, Err: fs.ErrInvalid))));
    }
    nint n = copy(b, (~f.f).data.slice((nint)(f.offset)));
    f.offset += (int64)n;
    return (n, default!);
}

[GoRecv] internal static (int64, error) Seek(this ref openFile f, int64 offset, nint whence) {
    switch (whence) {
    case 0: {
        break;
    }
    case 1: {
        offset += f.offset;
        break;
    }
    case 2: {
        offset += (int64)len((~f.f).data);
        break;
    }}

    // offset += 0
    if (offset < 0 || offset > (int64)len((~f.f).data)) {
        return (0, new fs.PathErrorжerror(Ꮡ(new fs.PathError(Op: "seek"u8, Path: (~f.f).name, Err: fs.ErrInvalid))));
    }
    f.offset = offset;
    return (offset, default!);
}

[GoRecv] internal static (nint, error) ReadAt(this ref openFile f, slice<byte> b, int64 offset) {
    if (offset < 0 || offset > (int64)len((~f.f).data)) {
        return (0, new fs.PathErrorжerror(Ꮡ(new fs.PathError(Op: "read"u8, Path: (~f.f).name, Err: fs.ErrInvalid))));
    }
    nint n = copy(b, (~f.f).data.slice((nint)(offset)));
    if (n < len(b)) {
        return (n, Δio.EOF);
    }
    return (n, default!);
}

// An openDir is a directory open for reading.
[GoType] partial struct openDir {
    internal ж<@file> f; // the directory file itself
    internal slice<@file> files; // the directory contents
    internal nint offset;   // the read offset, an index into the files slice
}

[GoRecv] internal static error Close(this ref openDir d) {
    return default!;
}

[GoRecv] internal static (fs.FileInfo, error) Stat(this ref openDir d) {
    return (new fileжFileInfo(d.f), default!);
}

[GoRecv] internal static (nint, error) Read(this ref openDir d, slice<byte> _) {
    return (0, new fs.PathErrorжerror(Ꮡ(new fs.PathError(Op: "read"u8, Path: (~d.f).name, Err: errors.New(isADirectoryˢ)))));
}

[GoRecv] internal static (slice<fs.DirEntry>, error) ReadDir(this ref openDir d, nint count) {
    nint n = len(d.files) - d.offset;
    if (n == 0) {
        if (count <= 0) {
            return (default!, default!);
        }
        return (default!, Δio.EOF);
    }
    if (count > 0 && n > count) {
        n = count;
    }
    var list = new slice<fs.DirEntry>(n);
    foreach (var (i, _) in list) {
        list[i] = new fileжDirEntry(Ꮡ(d.files, d.offset + i));
    }
    d.offset += n;
    return (list, default!);
}

// sortSearch is like sort.Search, avoiding an import.
internal static nint sortSearch(nint n, Func<nint, bool> f) {
    // Define f(-1) == false and f(n) == true.
    // Invariant: f(i-1) == false, f(j) == true.
    nint i = 0;
    nint j = n;
    while (i < j) {
        nint h = (nint)(((nuint)(i + j) >> (int)(1))); // avoid overflow when computing h
        // i ≤ h < j
        if (!f(h)){
            i = h + 1; // preserves f(i-1) == false
        } else {
            j = h; // preserves f(j) == true
        }
    }
    // i == j, f(i-1) == false, and f(j) (= f(i)) == true  =>  answer is i.
    return i;
}

} // end embed_package
