// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package gcimporter implements Import for gc-generated object files.
namespace go.go.@internal;

// import "go/internal/gcimporter"
using bufio = bufio_package;
using fmt = fmt_package;
using token = global::go.go.token_package;
using types = global::go.go.types_package;
using exportdata = global::go.@internal.exportdata_package;
using pkgbits = global::go.@internal.pkgbits_package;
using io = io_package;
using os = os_package;
using global::go.@internal;
using global::go.go;

partial class gcimporter_package {

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
            (filename, id, err) = exportdata.FindPkg(path, srcDir);
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
        (var data, err) = exportdata.ReadUnified(buf);
        if (err != default!) {
            err = fmt.Errorf("import %q: %v"u8, path, err);
            goto ᒐdone;
        }
        @string s = (@string)data;

        var input = pkgbits.NewPkgDecoder(id, s);
        pkg = readUnifiedPackage(Ꮡfset, nil, packages, input);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (pkg, err);
}

} // end gcimporter_package
