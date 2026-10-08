// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package gccgoimporter implements Import for gccgo-generated object files.
namespace go.go.@internal;

// import "go/internal/gccgoimporter"
using bytes = bytes_package;
using elf = debug.elf_package;
using fmt = fmt_package;
using types = global::go.go.types_package;
using xcoff = global::go.@internal.xcoff_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using debug;
using fs = global::go.io.fs_package;
using global::go.@internal;
using global::go.go;
using global::go.io;
using path;

partial class gccgoimporter_package {

// A PackageInit describes an imported package that needs initialization.
partial struct PackageInit {
    public @string Name; // short package name
    public @string InitFunc; // name of init function
    public nint Priority;   // priority of init function, see InitData.Priority
}

// The gccgo-specific init data for a package.
partial struct InitData {
    // Initialization priority of this package relative to other packages.
    // This is based on the maximum depth of the package's dependency graph;
    // it is guaranteed to be greater than that of its dependencies.
    public nint Priority;
    // The list of packages which this package depends on to be initialized,
    // including itself if needed. This is the subset of the transitive closure of
    // the package's dependencies that need initialization.
    public slice<PackageInit> Inits;
}

// Locate the file from which to read export data.
// This is intended to replicate the logic in gofrontend.
internal static (@string, error) findExportFile(slice<@string> searchpaths, @string pkgpath) {
    foreach (var (_, spath) in searchpaths) {
        @string pkgfullpath = filepath.Join(spath, pkgpath);
        var (pkgdir, name) = filepath.Split(pkgfullpath);
        foreach (var (_, filepathΔ1) in new @string[]{
            pkgfullpath,
            pkgfullpath + ".gox"u8,
            pkgdir + "lib"u8 + name + ".so"u8,
            pkgdir + "lib"u8 + name + ".a"u8,
            pkgfullpath + ".o"u8
        }.array()) {
            var (fi, err) = os.Stat(filepathΔ1);
            if (err == default! && !fi.IsDir()) {
                return (filepathΔ1, default!);
            }
        }
    }
    return ("", fmt.Errorf("%s: could not find export data (tried %s)"u8, pkgpath, strings.Join(searchpaths, ":"u8)));
}

internal static readonly @string gccgov1Magic = "v1;\n"u8;
internal static readonly @string gccgov2Magic = "v2;\n"u8;
internal static readonly @string gccgov3Magic = "v3;\n"u8;
internal static readonly @string goimporterMagic = "\n$$ "u8;
internal static readonly @string archiveMagic = "!<ar"u8;
internal static readonly @string aixbigafMagic = "<big"u8;

// Opens the export data file at the given path. If this is an ELF file,
// searches for and opens the .go_export section. If this is an archive,
// reads the export data from the first member, which is assumed to be an ELF file.
// This is intended to replicate the logic in gofrontend.
internal static (io.ReadSeeker reader, io.Closer closer, error err) openExportFile(@string fpath) {
    io.ReadSeeker reader = default!;
    io.Closer closer = default!;
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var err = ref Ꮡerr.ValueSlot;

        (var f, err) = os.Open(fpath);
        if (err != default!) {
            goto ᒐdone;
        }
        closer = new os_FileжCloser(f);
        var fʗ1 = f;
        defer(() => {
            if (Ꮡerr.ValueSlot != default! && closer != default!) {
                fʗ1.Close();
            }
        }, ref ᒐ);
        array<byte> magic = new(4);
        (_, err) = f.ReadAt(magic[..], 0);
        if (err != default!) {
            goto ᒐdone;
        }
        io.ReaderAt objreader = default!;
        var exprᴛ1 = ((sstring)(magic[..]));
        if (exprᴛ1 == gccgov1Magic || exprᴛ1 == gccgov2Magic || exprᴛ1 == gccgov3Magic || exprᴛ1 == goimporterMagic) {
            reader = new os_FileжReadSeeker(f);
            goto ᒐdone;
        }
        if (exprᴛ1 == archiveMagic || exprᴛ1 == aixbigafMagic) {
            (reader, err) = arExportData(new os_FileжReadSeeker(f));
            goto ᒐdone;
        }
        { /* default: */
            objreader = new os_FileжReaderAt(f);
        }

        // Raw export data.
        (var ef, err) = elf.NewFile(objreader);
        if (err == default!) {
            var sec = ef.Section(goExportˢ);
            if (sec == nil) {
                err = fmt.Errorf("%s: .go_export section not found"u8, fpath);
                goto ᒐdone;
            }
            reader = sec.Open();
            goto ᒐdone;
        }
        (var xf, err) = xcoff.NewFile(objreader);
        if (err == default!) {
            var sdat = xf.CSect(goExportˢ);
            if (sdat == default!) {
                err = fmt.Errorf("%s: .go_export section not found"u8, fpath);
                goto ᒐdone;
            }
            reader = new bytes_ReaderжReadSeeker(bytes.NewReader(sdat));
            goto ᒐdone;
        }
        err = fmt.Errorf("%s: unrecognized file format"u8, fpath);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (reader, closer, Ꮡerr.ValueSlot);
}

// type Importer is a methodless func type — rendered inline as its base delegate

internal partial interface GetImporter_type /*dyn*/ {
    @string Name();
}

public static Func<map<@string, ж<types.Package>>, @string, @string, Func<@string, (io.ReadCloser, error)>, (ж<types.Package>, error)> GetImporter(slice<@string> searchpaths, map<ж<types.Package>, InitData> initmap) {
    var initmapʗ1 = initmap;
    var searchpathsʗ1 = searchpaths;
    return (map<@string, ж<types.Package>> imports, @string pkgpath, @string srcDir, Func<@string, (io.ReadCloser, error)> lookup) => {
        ж<types.Package> pkg = default!;
        error err = default!;
        GoFrame ᒐ = default;
        try {
            // TODO(gri): Use srcDir.
            // Or not. It's possible that srcDir will fade in importance as
            // the go command and other tools provide a translation table
            // for relative imports (like ./foo or vendored imports).
            if (pkgpath == "unsafe"u8) {
                (pkg, err) = (types.Unsafe, default!); goto ᒐdone;
            }
            io.ReadSeeker reader = default!;
            @string fpath = default!;
            io.ReadCloser rc = default!;
            if (lookup != default!) {
                {
                    var p = imports[pkgpath]; if (p != nil && p.Complete()) {
                        (pkg, err) = (p, default!); goto ᒐdone;
                    }
                }
                (rc, err) = lookup(pkgpath);
                if (err != default!) {
                    (pkg, err) = (default!, err); goto ᒐdone;
                }
            }
            if (rc != default!){
                var rcʗ1 = rc;
                defer(() => rcʗ1.Close(), ref ᒐ);
                var (rs, ok) = rc._<io.ReadSeeker>(ᐧ);
                if (!ok) {
                    (pkg, err) = (default!, fmt.Errorf("gccgo importer requires lookup to return an io.ReadSeeker, have %T"u8, rc)); goto ᒐdone;
                }
                reader = rs;
                fpath = "<lookup "u8 + pkgpath + ">"u8;
                // Take name from Name method (like on os.File) if present.
                {
                    var (n, okΔ1) = rc._<GetImporter_type>(ᐧ); if (okΔ1) {
                        fpath = n.Name();
                    }
                }
            } else {
                (fpath, err) = findExportFile(searchpathsʗ1, pkgpath);
                if (err != default!) {
                    (pkg, err) = (default!, err); goto ᒐdone;
                }
                var (r, closer, errΔ1) = openExportFile(fpath);
                if (errΔ1 != default!) {
                    (pkg, err) = (default!, errΔ1); goto ᒐdone;
                }
                if (closer != default!) {
                    var closerʗ1 = closer;
                    defer(() => closerʗ1.Close(), ref ᒐ);
                }
                reader = r;
            }
            @string magics = default!;
            (magics, err) = readMagic(reader);
            if (err != default!) {
                goto ᒐdone;
            }
            if (magics == archiveMagic || magics == aixbigafMagic) {
                (reader, err) = arExportData(reader);
                if (err != default!) {
                    goto ᒐdone;
                }
                (magics, err) = readMagic(reader);
                if (err != default!) {
                    goto ᒐdone;
                }
            }
            var exprᴛ1 = magics;
            if (exprᴛ1 == gccgov1Magic || exprᴛ1 == gccgov2Magic || exprᴛ1 == gccgov3Magic) {
                ref var p = ref heap(new parser(), out var Ꮡp);
                Ꮡp.init(fpath, reader, imports);
                pkg = Ꮡp.parsePackage();
                if (initmapʗ1 != default!) {
                    initmapʗ1[pkg] = p.initdata;
                }
            }
            else { /* default: */
                err = fmt.Errorf("unrecognized magic string: %q"u8, // Excluded for now: Standard gccgo doesn't support this import format currently.
 // case goimporterMagic:
 // 	var data []byte
 // 	data, err = io.ReadAll(reader)
 // 	if err != nil {
 // 		return
 // 	}
 // 	var n int
 // 	n, pkg, err = importer.ImportData(imports, data)
 // 	if err != nil {
 // 		return
 // 	}
 // 	if initmap != nil {
 // 		suffixreader := bytes.NewReader(data[n:])
 // 		var p parser
 // 		p.init(fpath, suffixreader, nil)
 // 		p.parseInitData()
 // 		initmap[pkg] = p.initdata
 // 	}
 magics);
            }

            goto ᒐdone;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        ᒐdone: return (pkg, err);
    };
}

// readMagic reads the four bytes at the start of a ReadSeeker and
// returns them as a string.
internal static (@string, error) readMagic(io.ReadSeeker reader) {
    array<byte> magic = new(4);
    {
        var (_, err) = reader.Read(magic[..]); if (err != default!) {
            return ("", err);
        }
    }
    {
        var (_, err) = reader.Seek(0, io.SeekStart); if (err != default!) {
            return ("", err);
        }
    }
    return (((@string)(magic[..])), default!);
}

} // end gccgoimporter_package
