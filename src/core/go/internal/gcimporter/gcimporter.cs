// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package gcimporter implements Import for gc-generated object files.
namespace go.go.@internal;

// import "go/internal/gcimporter"
using bufio = bufio_package;
using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using build = global::go.go.build_package;
using token = global::go.go.token_package;
using types = global::go.go.types_package;
using pkgbits = global::go.@internal.pkgbits_package;
using saferio = global::go.@internal.saferio_package;
using io = io_package;
using os = os_package;
using exec = global::go.os.exec_package;
using filepath = path.filepath_package;
using strings = strings_package;
using sync = sync_package;
using fs = global::go.io.fs_package;
using global::go.@internal;
using global::go.go;
using global::go.io;
using global::go.os;
using path;

partial class gcimporter_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸbytes() {
    builtin.initPackage(typeof(bytes_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸerrors() {
    builtin.initPackage(typeof(errors_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸgoꓸbuild() {
    builtin.initPackage(typeof(global::go.go.build_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() {
    builtin.initPackage(typeof(global::go.go.token_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸgoꓸtypes() {
    builtin.initPackage(typeof(global::go.go.types_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸinternalꓸpkgbits() {
    builtin.initPackage(typeof(global::go.@internal.pkgbits_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸinternalꓸsaferio() {
    builtin.initPackage(typeof(global::go.@internal.saferio_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸos() {
    builtin.initPackage(typeof(os_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸosꓸexec() {
    builtin.initPackage(typeof(global::go.os.exec_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() {
    builtin.initPackage(typeof(path.filepath_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸsync() {
    builtin.initPackage(typeof(sync_package));
}

// debugging/development support
internal const bool debug = false;

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
                            Ꮡerr.ValueSlot = errors.New(((@string)(~ee).Stderr));
                        }
                    }
                    return;
                }
                var exports = strings.Split(((@string)bytes.TrimSpace(output)), "\n"u8);
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

internal static array<@string> pkgExts = new @string[]{".a"u8, ".o"u8}.array(); // a file from the build cache will have no extension

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
        {
            var (abs, errΔ2) = filepath.Abs(srcDir); if (errΔ2 == default!) {
                // "x" -> "$GOPATH/pkg/$GOOS_$GOARCH/x.ext", "x"
                // Don't require the source files to be present.
                // see issue 14282
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
        noext = filepath.Join(srcDir, // "./x" -> "/this/directory/x.ext", "/this/directory/x"
 path);
        id = noext;
        break;
    }
    case {} when filepath.IsAbs(path): {
        noext = path;
        id = path;
        break;
    }}

    // for completeness only - go/build.Import
    // does not support absolute imports
    // "/x" -> "/x.ext", "/x"
    if (false) {
        // for debugging
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

// Import imports a gc-generated package given its import path and srcDir, adds
// the corresponding package object to the packages map, and returns the object.
// The packages map must contain all packages already imported.
public static (ж<types.Package> pkg, error err) Import(ж<token.FileSet> Ꮡfset, map<@string, ж<types.Package>> packages, @string path, @string srcDir, Func<@string, (io.ReadCloser, error)> lookup) {
    ж<types.Package> pkg = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        io.ReadCloser rc = default!;
        @string id = default!;
        if (lookup != default!){
            // With custom lookup specified, assume that caller has
            // converted path to a canonical import path for use in the map.
            if (path == "unsafe"u8) {
                (pkg, err) = (types.Unsafe, default!); goto ᒐdone;
            }
            id = path;
            // No need to re-import if the package was imported completely before.
            {
                pkg = packages[id]; if (pkg != nil && pkg.Complete()) {
                    goto ᒐdone;
                }
            }
            var (f, errΔ1) = lookup(path);
            if (errΔ1 != default!) {
                (pkg, err) = (default!, errΔ1); goto ᒐdone;
            }
            rc = f;
        } else {
            @string filename = default!;
            (filename, id, err) = FindPkg(path, srcDir);
            if (filename == ""u8) {
                if (path == "unsafe"u8) {
                    (pkg, err) = (types.Unsafe, default!); goto ᒐdone;
                }
                (pkg, err) = (default!, err); goto ᒐdone;
            }
            // no need to re-import if the package was imported completely before
            {
                pkg = packages[id]; if (pkg != nil && pkg.Complete()) {
                    goto ᒐdone;
                }
            }
            // open file
            ref var errΔ2 = ref heap<error>(out var ᏑerrΔ2);
            (var f, errΔ2) = os.Open(filename);
            if (errΔ2 != default!) {
                (pkg, err) = (default!, errΔ2); goto ᒐdone;
            }
            defer(() => {
                if (ᏑerrΔ2.ValueSlot != default!) {
                    // add file name to error
                    ᏑerrΔ2.ValueSlot = fmt.Errorf("%s: %v"u8, filename, ᏑerrΔ2.ValueSlot);
                }
            }, ref ᒐ);
            rc = new os_FileжReadCloser(f);
        }
        var rcʗ1 = rc;
        defer(() => rcʗ1.Close(), ref ᒐ);
        var buf = bufio.NewReader(rc);
        (var hdr, var size, err) = FindExportData(buf);
        if (err != default!) {
            goto ᒐdone;
        }
        var exprᴛ1 = hdr;
        if (exprᴛ1 == "$$\n"u8) {
            err = fmt.Errorf("import %q: old textual export format no longer supported (recompile library)"u8, path);
        }
        else if (exprᴛ1 == "$$B\n"u8) {
            byte exportFormatΔ1 = default!;
            {
                (exportFormatΔ1, err) = buf.ReadByte(); if (err != default!) {
                    goto ᒐdone;
                }
            }
            size--;
            switch (exportFormatΔ1) {
            case (rune)'u': {
// The unified export format starts with a 'u'; the indexed export
// format starts with an 'i'; and the older binary export format
// starts with a 'c', 'd', or 'v' (from "version"). Select
// appropriate importer.
                slice<byte> dataΔ3 = default!;
                io.Reader r = new bufio_ReaderжReader(buf);
                if (size >= 0){
                    {
                        (dataΔ3, err) = saferio.ReadData(r, (uint64)size); if (err != default!) {
                            goto ᒐdone;
                        }
                    }
                } else 
                {
                    (dataΔ3, err) = io.ReadAll(r); if (err != default!) {
                        goto ᒐdone;
                    }
                }
                @string s = ((@string)dataΔ3);
                s = s[..(int)(strings.LastIndex(s, "\n$$\n"u8))];
                var input = pkgbits.NewPkgDecoder(id, s);
                pkg = readUnifiedPackage(Ꮡfset, nil, packages, input);
                break;
            }
            case (rune)'i': {
                (pkg, err) = iImportData(Ꮡfset, packages, buf, id);
                break;
            }
            default: {
                err = fmt.Errorf("import %q: old binary export format no longer supported (recompile library)"u8, path);
                break;
            }}

        }
        else { /* default: */
            err = fmt.Errorf("import %q: unknown export data header: %q"u8, path, hdr);
        }

    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (pkg, err);
}

} // end gcimporter_package
