// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package srcimporter implements importing directly
// from source files rather than installed packages.
namespace go.go.@internal;

// import "go/internal/srcimporter"
using fmt = fmt_package;
using ast = global::go.go.ast_package;
using build = global::go.go.build_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using types = global::go.go.types_package;
using io = io_package;
using os = os_package;
using exec = global::go.os.exec_package;
using filepath = path.filepath_package;
using strings = strings_package;
using sync = sync_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for go:linkname
using global::go.go;
using global::go.os;
using path;
using ꓸꓸꓸstring = Span<@string>;

partial class srcimporter_package {

// An Importer provides the context for importing packages from source code.
[GoType] partial struct Importer {
    internal ж<build.Context> ctxt;
    internal ж<token.FileSet> fset;
    internal types.Sizes sizes;
    internal map<@string, ж<types.Package>> packages;
}

// New returns a new Importer for the given context, file set, and map
// of packages. The context is used to resolve import paths to package paths,
// and identifying the files belonging to the package. If the context provides
// non-nil file system functions, they are used instead of the regular package
// os functions. The file set is used to track position information of package
// files; and imported packages are added to the packages map.
public static ж<Importer> New(ж<build.Context> Ꮡctxt, ж<token.FileSet> Ꮡfset, map<@string, ж<types.Package>> packages) {
    ref var ctxt = ref Ꮡctxt.DerefOrNull();

    return Ꮡ(new Importer(
        ctxt: Ꮡctxt,
        fset: Ꮡfset,
        sizes: types.SizesFor(ctxt.Compiler, ctxt.GOARCH), // uses go/types default if GOARCH not found

        packages: packages
    ));
}

// Importing is a sentinel taking the place in Importer.packages
// for a package that is in the process of being imported.
internal static ж<types.Package> Ꮡimporting = new StandardBox<types.Package>(default(types.Package));
internal static ref types.Package importing => ref Ꮡimporting.Value;

// Import(path) is a shortcut for ImportFrom(path, ".", 0).
public static (ж<types.Package>, error) Import(this ж<Importer> Ꮡp, @string path) {
    return Ꮡp.ImportFrom(path, "."u8, 0); // use "." rather than "" (see issue #24441)
}

// ImportFrom imports the package with the given import path resolved from the given srcDir,
// adds the new package to the set of packages maintained by the importer, and returns the
// package. Package path resolution and file system operations are controlled by the context
// maintained with the importer. The import mode must be zero but is otherwise ignored.
// Packages that are not comprised entirely of pure Go files may fail to import because the
// type checker may not be able to determine all exported entities (e.g. due to cgo dependencies).
public static (ж<types.Package>, error) ImportFrom(this ж<Importer> Ꮡp, @string path, @string srcDir, types.ImportMode mode) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        if (mode != 0) {
            throw panic("non-zero import mode");
        }
        {
            var (abs, errΔ1) = p.absPath(srcDir); if (errΔ1 == default!) {
                // see issue #14282
                srcDir = abs;
            }
        }
        var (bp, err) = p.ctxt.Import(path, srcDir, 0);
        if (err != default!) {
            return (default!, err); // err may be *build.NoGoError - return as is
        }
        // package unsafe is known to the type checker
        if ((~bp).ImportPath == "unsafe"u8) {
            return (types.Unsafe, default!);
        }
        // no need to re-import if the package was imported completely before
        var pkg = p.packages[(~bp).ImportPath];
        if (pkg != nil) {
            if (pkg == Ꮡimporting) {
                return (default!, fmt.Errorf("import cycle through package %q"u8, (~bp).ImportPath));
            }
            if (!pkg.Complete()) {
                // Package exists but is not complete - we cannot handle this
                // at the moment since the source importer replaces the package
                // wholesale rather than augmenting it (see #19337 for details).
                // Return incomplete package with error (see #16088).
                return (pkg, fmt.Errorf("reimported partially imported package %q"u8, (~bp).ImportPath));
            }
            return (pkg, default!);
        }
        p.packages[(~bp).ImportPath] = Ꮡimporting;
        var bpʗ1 = bp;
        defer(() => {
            // clean up in case of error
            // TODO(gri) Eventually we may want to leave a (possibly empty)
            // package in the map in all cases (and use that package to
            // identify cycles). See also issue 16088.
            if (Ꮡp.Value.packages[(~bpʗ1).ImportPath] == Ꮡimporting) {
                Ꮡp.Value.packages[(~bpʗ1).ImportPath] = default!;
            }
        }, ref ᒐ);
        slice<@string> filenames = default!;
        filenames = appendꓸꓸꓸ(filenames, (~bp).GoFiles);
        filenames = appendꓸꓸꓸ(filenames, (~bp).CgoFiles);
        (var files, err) = Ꮡp.parseFiles((~bp).Dir, filenames);
        if (err != default!) {
            return (default!, err);
        }
        // type-check package files
        ref var firstHardErr = ref heap<error>(out var ᏑfirstHardErr);
        ref var conf = ref heap<types.Config>(out var Ꮡconf);
        conf = new types.Config(
            IgnoreFuncBodies: true, // continue type-checking after the first error

            Error: (error errΔ2) => {
                if (ᏑfirstHardErr.ValueSlot == default! && !errΔ2._<typesꓸError>().Soft) {
                    ᏑfirstHardErr.ValueSlot = errΔ2;
                }
            },
            Importer: new ImporterжImporter(Ꮡp),
            Sizes: p.sizes
        );
        if (len((~bp).CgoFiles) > 0) {
            if ((~p.ctxt).OpenFile != default!){
                // cgo, gcc, pkg-config, etc. do not support
                // build.Context's VFS.
                conf.FakeImportC = true;
            } else {
                setUsesCgo(Ꮡconf);
                var (@file, errΔ3) = Ꮡp.cgo(bp);
                if (errΔ3 != default!) {
                    return (default!, fmt.Errorf("error processing cgo for package %q: %w"u8, (~bp).ImportPath, errΔ3));
                }
                files = append(files, @file);
            }
        }
        (pkg, err) = Ꮡconf.Check((~bp).ImportPath, p.fset, files, nil);
        if (err != default!) {
            // If there was a hard error it is possibly unsafe
            // to use the package as it may not be fully populated.
            // Do not return it (see also #20837, #20855).
            if (firstHardErr != default!) {
                pkg = default!;
                err = firstHardErr; // give preference to first hard error over any soft error
            }
            return (pkg, fmt.Errorf("type-checking package %q failed (%v)"u8, (~bp).ImportPath, err));
        }
        if (firstHardErr != default!) {
            // this can only happen if we have a bug in go/types
            throw panic("package is not safe yet no error was returned");
        }
        p.packages[(~bp).ImportPath] = pkg;
        return (pkg, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static (slice<ж<ast.File>>, error) parseFiles(this ж<Importer> Ꮡp, @string dir, slice<@string> filenames) {
    ref var p = ref Ꮡp.DerefOrNull();

    // use build.Context's OpenFile if there is one
    var open = p.ctxt.Value.OpenFile;
    if (open == default!) {
        open = (@string name) => {
            var (ᴛ1, ᴛ2) = os.Open(name);
            return (new os_FileжReadCloser(ᴛ1), ᴛ2);
        };
    }
    var files = new slice<ж<ast.File>>(len(filenames));
    var errors = new slice<error>(len(filenames));
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(len(filenames));
    foreach (var (i, filename) in filenames) {
        var errorsʗ1 = errors;
        var filesʗ1 = files;
        var openʗ1 = open;
        goǃ((nint iΔ1, @string filepath) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                var (src, err) = openʗ1(filepath);
                if (err != default!) {
                    errorsʗ1[iΔ1] = err; // open provides operation and filename in error
                    return;
                }
                (filesʗ1[iΔ1], errorsʗ1[iΔ1]) = parser.ParseFile(Ꮡp.Value.fset, filepath, src, parser.SkipObjectResolution);
                src.Close(); // ignore Close error - parsing may have succeeded which is all we need
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, i, Ꮡp.Value.joinPath(dir, filename));
    }
    Ꮡwg.Wait();
    // if there are errors, return the first one for deterministic results
    foreach (var (_, err) in errors) {
        if (err != default!) {
            return (default!, err);
        }
    }
    return (files, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string srcimporterˢ = "srcimporter"u8;
internal static readonly @string binˢ = "bin"u8;
internal static readonly @string cgoCppflagsˢ = "CGO_CPPFLAGS"u8;
internal static readonly @string cgoCflagsˢ = "CGO_CFLAGS"u8;
internal static readonly @string cgoGotypesGoˢ = "_cgo_gotypes.go"u8;

internal static (ж<ast.File>, error) cgo(this ж<Importer> Ꮡp, ж<build.Package> Ꮡbp) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();
        ref var bp = ref Ꮡbp.DerefOrNull();

        var (tmpdir, err) = os.MkdirTemp(""u8, srcimporterˢ);
        if (err != default!) {
            return (default!, err);
        }
        defer(os.RemoveAll, tmpdir, ref ᒐ);
        @string goCmd = "go"u8;
        if ((~p.ctxt).GOROOT != ""u8) {
            goCmd = filepath.Join((~p.ctxt).GOROOT, binˢ, "go");
        }
        var args = new @string[]{goCmd, "tool"u8, "cgo"u8, "-objdir"u8, tmpdir}.slice();
        if (bp.Goroot) {
            var exprᴛ1 = bp.ImportPath;
            if (exprᴛ1 == "runtime/cgo"u8) {
                args = append(args, "-import_runtime_cgo=false"u8, "-import_syscall=false");
            }
            else if (exprᴛ1 == "runtime/race"u8) {
                args = append(args, "-import_syscall=false"u8);
            }

        }
        args = append(args, "--"u8);
        args = appendꓸꓸꓸ(args, strings.Fields(os.Getenv(cgoCppflagsˢ)));
        args = appendꓸꓸꓸ(args, bp.CgoCPPFLAGS);
        if (len(bp.CgoPkgConfig) > 0) {
            var cmdΔ1 = exec.Command("pkg-config"u8, appendꓸꓸꓸ(new @string[]{"--cflags"u8}.slice(), bp.CgoPkgConfig).ꓸꓸꓸ);
            var (@out, errΔ1) = cmdΔ1.Output();
            if (errΔ1 != default!) {
                return (default!, fmt.Errorf("pkg-config --cflags: %w"u8, errΔ1));
            }
            args = appendꓸꓸꓸ(args, strings.Fields(((@string)@out)));
        }
        args = append(args, "-I"u8, tmpdir);
        args = appendꓸꓸꓸ(args, strings.Fields(os.Getenv(cgoCflagsˢ)));
        args = appendꓸꓸꓸ(args, bp.CgoCFLAGS);
        args = appendꓸꓸꓸ(args, bp.CgoFiles);
        var cmd = exec.Command(args[0], args[1..].ꓸꓸꓸ);
        cmd.Value.Dir = bp.Dir;
        {
            var errΔ2 = cmd.Run(); if (errΔ2 != default!) {
                return (default!, fmt.Errorf("go tool cgo: %w"u8, errΔ2));
            }
        }
        return parser.ParseFile(p.fset, filepath.Join(tmpdir, cgoGotypesGoˢ), default!, parser.SkipObjectResolution);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// context-controlled file system operations
[GoRecv] internal static (@string, error) absPath(this ref Importer p, @string path) {
    // TODO(gri) This should be using p.ctxt.AbsPath which doesn't
    // exist but probably should. See also issue #14282.
    return filepath.Abs(path);
}

[GoRecv] internal static bool isAbsPath(this ref Importer p, @string path) {
    {
        var f = p.ctxt.Value.IsAbsPath; if (f != default!) {
            return f(path);
        }
    }
    return filepath.IsAbs(path);
}

[GoRecv] internal static @string joinPath(this ref Importer p, params ꓸꓸꓸstring elemʗp) {
    var elem = elemʗp.slice();

    {
        var f = p.ctxt.Value.JoinPath; if (f != default!) {
            return f(elem.ꓸꓸꓸ);
        }
    }
    return filepath.Join(elem.ꓸꓸꓸ);
}

//go:linkname setUsesCgo go/types.srcimporter_setUsesCgo
[global::System.Diagnostics.StackTraceHidden] internal static void setUsesCgo(ж<types.Config> conf) {
    types.srcimporter_setUsesCgo(conf);
}

} // end srcimporter_package
