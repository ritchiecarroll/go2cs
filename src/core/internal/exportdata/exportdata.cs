// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package exportdata implements common utilities for finding
// and reading gc-generated object files.
namespace go.@internal;

// This file should be kept in sync with src/cmd/compile/internal/gc/obj.go .
using bufio = bufio_package;
using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using build = global::go.go.build_package;
using saferio = global::go.@internal.saferio_package;
using io = io_package;
using os = os_package;
using exec = global::go.os.exec_package;
using filepath = path.filepath_package;
using strings = strings_package;
using sync = global::go.sync_package;
using fs = global::go.io.fs_package;
using global::go;
using global::go.@internal;
using global::go.go;
using global::go.io;
using global::go.os;
using path;

partial class exportdata_package {

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string markerᶜ = "\n$$\n"u8;

// ReadUnified reads the contents of the unified export data from a reader r
// that contains the contents of a GC-created archive file.
//
// On success, the reader will be positioned after the end-of-section marker "\n$$\n".
//
// Supported GC-created archive files have 4 layers of nesting:
//   - An archive file containing a package definition file.
//   - The package definition file contains headers followed by a data section.
//     Headers are lines (≤ 4kb) that do not start with "$$".
//   - The data section starts with "$$B\n" followed by export data followed
//     by an end of section marker "\n$$\n". (The section start "$$\n" is no
//     longer supported.)
//   - The export data starts with a format byte ('u') followed by the <data> in
//     the given format. (See ReadExportDataHeader for older formats.)
//
// Putting this together, the bytes in a GC-created archive files are expected
// to look like the following.
// See cmd/internal/archive for more details on ar file headers.
//
// | <!arch>\n             | ar file signature
// | __.PKGDEF...size...\n | ar header for __.PKGDEF including size.
// | go object <...>\n     | objabi header
// | <optional headers>\n  | other headers such as build id
// | $$B\n                 | binary format marker
// | u<data>\n             | unified export <data>
// | $$\n                  | end-of-section marker
// | [optional padding]    | padding byte (0x0A) if size is odd
// | [ar file header]      | other ar files
// | [ar file data]        |
public static (slice<byte> data, error err) ReadUnified(ж<bufio.Reader> Ꮡr) {
    slice<byte> data = default!;
    error err = default!;
    ref var r = ref Ꮡr.DerefOrNull();

    // We historically guaranteed headers at the default buffer size (4096) work.
    // This ensures we can use ReadSlice throughout.
    const nint minBufferSize = 4096;
    Ꮡr = bufio.NewReaderSize(new bufio_ReaderжReader(Ꮡr), minBufferSize); r = ref Ꮡr.DerefOrNull();

    (var size, err) = FindPackageDefinition(Ꮡr);
    if (err != default!) {
        return (data, err);
    }
    nint n = size;

    (var objapi, var headers, err) = ReadObjectHeaders(Ꮡr);
    if (err != default!) {
        return (data, err);
    }
    n -= len(objapi);
    foreach (var (_, h) in headers) {
        n -= len(h);
    }

    (var hdrlen, err) = ReadExportDataHeader(Ꮡr);
    if (err != default!) {
        return (data, err);
    }
    n -= hdrlen;

    // size also includes the end of section marker. Remove that many bytes from the end.
    @string marker = markerᶜ;
    n -= len(marker);

    if (n < 0) {
        err = fmt.Errorf("invalid size (%d) in the archive file: %d bytes remain without section headers (recompile package)"u8, size, n);
        return (data, err);
    }

    // Read n bytes from buf.
    (data, err) = saferio.ReadData(new bufio_ReaderжReader(Ꮡr), (uint64)n);
    if (err != default!) {
        return (data, err);
    }

    // Check for marker at the end.
    array<byte> suffix = new(4); /* len(marker) */
    (_, err) = io.ReadFull(new bufio_ReaderжReader(Ꮡr), suffix[..]);
    if (err != default!) {
        return (data, err);
    }
    {
        @string s = (@string)(suffix[..]); if (s != marker) {
            err = fmt.Errorf("read %q instead of end-of-section marker (%q)"u8, s, marker);
            return (data, err);
        }
    }

    return (data, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string pkgdefˢ = "__.PKGDEF"u8;

// FindPackageDefinition positions the reader r at the beginning of a package
// definition file ("__.PKGDEF") within a GC-created archive by reading
// from it, and returns the size of the package definition file in the archive.
//
// The reader must be positioned at the start of the archive file before calling
// this function, and "__.PKGDEF" is assumed to be the first file in the archive.
//
// See cmd/internal/archive for details on the archive format.
public static (nint size, error err) FindPackageDefinition(ж<bufio.Reader> Ꮡr) {
    nint size = default!;
    error err = default!;
    ref var r = ref Ꮡr.DerefOrNull();

    // Uses ReadSlice to limit risk of malformed inputs.
    // Read first line to make sure this is an object file.
    (var line, err) = r.ReadSlice((rune)'\n');
    if (err != default!) {
        err = fmt.Errorf("can't find export data (%v)"u8, err);
        return (size, err);
    }

    // Is the first line an archive file signature?
    if (((sstring)line) != "!<arch>\n"u8) {
        err = fmt.Errorf("not the start of an archive file (%q)"u8, line);
        return (size, err);
    }

    // package export block should be first
    size = readArchiveHeader(Ꮡr, pkgdefˢ);
    if (size <= 0) {
        err = fmt.Errorf("not a package file"u8);
        return (size, err);
    }

    return (size, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string goObjectˢ = "go object "u8;

// ReadObjectHeaders reads object headers from the reader. Object headers are
// lines that do not start with an end-of-section marker "$$". The first header
// is the objabi header. On success, the reader will be positioned at the beginning
// of the end-of-section marker.
//
// It returns an error if any header does not fit in r.Size() bytes.
public static (@string objapi, slice<@string> headers, error err) ReadObjectHeaders(ж<bufio.Reader> Ꮡr) {
    @string objapi = default!;
    slice<@string> headers = default!;
    error err = default!;
    ref var r = ref Ꮡr.DerefOrNull();

    // line is a temporary buffer for headers.
    // Use bounded reads (ReadSlice, Peek) to limit risk of malformed inputs.
    slice<byte> line = default!;

    // objapi header should be the first line
    {
        (line, err) = r.ReadSlice((rune)'\n'); if (err != default!) {
            err = fmt.Errorf("can't find export data (%v)"u8, err);
            return (objapi, headers, err);
        }
    }
    objapi = (@string)line;

    // objapi header begins with "go object ".
    if (!strings.HasPrefix(objapi, goObjectˢ)) {
        err = fmt.Errorf("not a go object file: %s"u8, objapi);
        return (objapi, headers, err);
    }

    // process remaining object header lines
    while (ᐧ) {
        // check for an end of section marker "$$"
        (line, err) = r.Peek(2);
        if (err != default!) {
            return (objapi, headers, err);
        }
        if (((sstring)line) == "$$"u8) {
            return (objapi, headers, err); // stop
        }

        // read next header
        (line, err) = r.ReadSlice((rune)'\n');
        if (err != default!) {
            return (objapi, headers, err);
        }
        headers = append(headers, (@string)line);
    }
}

// ReadExportDataHeader reads the export data header and format from r.
// It returns the number of bytes read, or an error if the format is no longer
// supported or it failed to read.
//
// The only currently supported format is binary export data in the
// unified export format.
public static (nint n, error err) ReadExportDataHeader(ж<bufio.Reader> Ꮡr) {
    nint n = default!;
    error err = default!;
    ref var r = ref Ꮡr.DerefOrNull();

    // Read export data header.
    (var line, err) = r.ReadSlice((rune)'\n');
    if (err != default!) {
        return (n, err);
    }

    @string hdr = (@string)line;
    var exprᴛ1 = hdr;
    if (exprᴛ1 == "$$\n"u8) {
        err = fmt.Errorf("old textual export format no longer supported (recompile package)"u8);
        return (n, err);
    }
    if (exprᴛ1 == "$$B\n"u8) {
        byte format = default!;
        (format, err) = r.ReadByte();
        if (err != default!) {
            return (n, err);
        }
        // The unified export format starts with a 'u'.
        switch (format) {
        case (rune)'u': {
            break;
        }
        default: {
            // Older no longer supported export formats include:
            // indexed export format which started with an 'i'; and
            // the older binary export format which started with a 'c',
            // 'd', or 'v' (from "version").
            err = fmt.Errorf("binary export format %q is no longer supported (recompile package)"u8, format);
            return (n, err);
        }}
    }
    else { /* default: */
        err = fmt.Errorf("unknown export data header: %q"u8, hdr);
        return (n, err);
    }

    n = len(hdr) + 1; // + 1 is for 'u'
    return (n, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string pathIsEmptyˢ = "path is empty"u8;

// FindPkg returns the filename and unique package id for an import
// path based on package information provided by build.Import (using
// the build.Default build.Context). A relative srcDir is interpreted
// relative to the current working directory.
public static (@string filename, @string id, error err) FindPkg(@string path, @string srcDir) {
    @string filename = default!;
    @string id = default!;
    error err = default!;

    if (path == ""u8) {
        return ("", "", errors.New(pathIsEmptyˢ));
    }

    @string noext = default!;
    switch (ᐧ) {
    default: {
        // "x" -> "$GOPATH/pkg/$GOOS_$GOARCH/x.ext", "x"
        // Don't require the source files to be present.
        {
            var (abs, errΔ2) = filepath.Abs(srcDir); if (errΔ2 == default!) { // see issue 14282
                srcDir = abs;
            }
        }
        ж<build.Package> bp = default!;
        (bp, err) = build.Import(path, srcDir, (build.ImportMode)(build.FindOnly | build.AllowBinary));
        if ((~bp).PkgObj == ""u8){
            if ((~bp).Goroot && (~bp).Dir != ""u8) {
                (filename, err) = lookupGorootExport((~bp).Dir);
                if (err == default!) {
                    (_, err) = os.Stat(filename);
                }
                if (err == default!) {
                    return (filename, (~bp).ImportPath, default!);
                }
            }
            goto notfound;
        } else {
            noext = strings.TrimSuffix((~bp).PkgObj, ".a"u8);
        }
        id = bp.Value.ImportPath;
        break;
    }
    case {} when build.IsLocalImport(path): {
        // "./x" -> "/this/directory/x.ext", "/this/directory/x"
        noext = filepath.Join(srcDir, path);
        id = noext;
        break;
    }
    case {} when filepath.IsAbs(path): {
        // for completeness only - go/build.Import
        // does not support absolute imports
        // "/x" -> "/x.ext", "/x"
        noext = path;
        id = path;
        break;
    }}

    if (false) { // for debugging
        if (path != id) {
            fmt.Printf("%s -> %s\n"u8, path, id);
        }
    }

    // try extensions
    foreach (var (_, ext) in pkgExts.ΔRangeSnapshot()) {
        filename = noext + ext;
        var (f, statErr) = os.Stat(filename);
        if (statErr == default! && !f.IsDir()) {
            return (filename, id, default!);
        }
        if (err == default!) {
            err = statErr;
        }
    }

notfound:
    if (err == default!) {
        return ("", path, fmt.Errorf("can't find import: %q"u8, path));
    }
    return ("", path, fmt.Errorf("can't find import: %q: %w"u8, path, err));
}

internal static array<@string> pkgExts = new @string[]{".a"u8, ".o"u8}.array(); // a file from the build cache will have no extension

internal static ж<sync.Map> ᏑexportMap = new StandardBox<sync.Map>(default(sync.Map));
internal static ref sync.Map exportMap => ref ᏑexportMap.Value; // package dir → func() (string, error)

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string binˢ = "bin"u8;
private static readonly @string listˢ = "list"u8;
private static readonly @string exportˢ = "-export"u8;
private static readonly @string exportˢ2 = "{{.Export}}"u8;

// lookupGorootExport returns the location of the export data
// (normally found in the build cache, but located in GOROOT/pkg
// in prior Go releases) for the package located in pkgDir.
//
// (We use the package's directory instead of its import path
// mainly to simplify handling of the packages in src/vendor
// and cmd/vendor.)
internal static (@string, error) lookupGorootExport(@string pkgDir) {
    var (f, ok) = ᏑexportMap.Load(pkgDir);
    if (!ok) {
        ref var listOnce = ref heap(new sync.Once(), out var ᏑlistOnce);
        @string exportPath = default!;
        ref var err = ref heap<error>(out var Ꮡerr);
        (f, _) = ᏑexportMap.LoadOrStore(pkgDir, (@string, error) () => {
            ᏑlistOnce.Do(() => {
                var cmd = exec.Command(filepath.Join(build.Default.GOROOT, binˢ, "go"), listˢ, exportˢ, "-f", exportˢ2, pkgDir);
                cmd.Value.Dir = build.Default.GOROOT;
                cmd.Value.Env = append(os.Environ(), "PWD="u8 + (~cmd).Dir, "GOROOT=" + build.Default.GOROOT);
                slice<byte> output = default!;
                (output, Ꮡerr.ValueSlot) = cmd.Output();
                if (Ꮡerr.ValueSlot != default!) {
                    {
                        var (ee, okΔ1) = Ꮡerr.ValueSlot._<ж<exec.ExitError>>(ᐧ); if (okΔ1 && len((~ee).Stderr) > 0) {
                            Ꮡerr.ValueSlot = errors.New((@string)(~ee).Stderr);
                        }
                    }
                    return;
                }

                var exports = strings.Split((@string)bytes.TrimSpace(output), "\n"u8);
                if (len(exports) != 1) {
                    Ꮡerr.ValueSlot = fmt.Errorf("go list reported %d exports; expected 1"u8, len(exports));
                    return;
                }

                exportPath = exports[0];
            });

            return (exportPath, Ꮡerr.ValueSlot);
        });
    }

    return f._<Func<(@string, error)>>()();
}

} // end exportdata_package
