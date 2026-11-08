// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements a typechecker test harness. The packages specified
// in tests are typechecked. Error messages reported by the typechecker are
// compared against the errors expected in the test files.
//
// Expected errors are indicated in the test files by putting comments
// of the form /* ERROR pattern */ or /* ERRORx pattern */ (or a similar
// //-style line comment) immediately following the tokens where errors
// are reported. There must be exactly one blank before and after the
// ERROR/ERRORx indicator, and the pattern must be a properly quoted Go
// string.
//
// The harness will verify that each ERROR pattern is a substring of the
// error reported at that source position, and that each ERRORx pattern
// is a regular expression matching the respective error.
// Consecutive comments may be used to indicate multiple errors reported
// at the same position.
//
// For instance, the following test source indicates that an "undeclared"
// error should be reported for the undeclared variable x:
//
//	package p
//	func f() {
//		_ = x /* ERROR "undeclared" */ + 1
//	}
namespace go.go;

using bytes = bytes_package;
using flag = flag_package;
using fmt = fmt_package;
using ast = global::go.go.ast_package;
using parser = global::go.go.parser_package;
using scanner = global::go.go.scanner_package;
using token = global::go.go.token_package;
using buildcfg = global::go.@internal.buildcfg_package;
using testenv = global::go.@internal.testenv_package;
using errors = global::go.@internal.types.errors_package;
using os = os_package;
using filepath = global::go.path.filepath_package;
using reflect = reflect_package;
using regexp = regexp_package;
using runtime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using static global::go.go.types_package;
using @unsafe = unsafe_package;
using fs = io.fs_package;
using global::go.@internal;
using global::go.@internal.types;
using global::go.go;
using global::go.path;
using io;
using static global::go.go.types_internal_test_package;
using types = global::go.go.types_package;

partial class types_test_package {

internal static ж<bool> haltOnError = flag.Bool("halt", false, "halt on error");
internal static ж<bool> verifyErrors = flag.Bool("verify", false, "verify errors (rather than list them) in TestManual");

internal static ж<token.FileSet> fset = token.NewFileSet();

internal static (slice<ж<ast.File>>, slice<error>) parseFiles(ж<testing.T> t, slice<@string> filenames, slice<slice<byte>> srcs, parser.Mode mode) {
    var filesᴷ = new stackAppend<ж<ast.File>>(moves: true);

    slice<ж<ast.File>> files = default;
    slice<error> errlist = default;
    foreach (var (i, filename) in range(filenames)) {
        var (@file, err) = parser.ParseFile(fset, filename, srcs[i], mode);
        if (@file == nil) {
            t.Fatalf("%s: %s", filename, err);
        }
        files = append(filesᴷ, files, @file);
        if (err != nil) {
            var (list, _) = err._<scanner.ErrorList>(ᐧ); if (len(list) > 0) {
                foreach (var (_, errΔ1) in range<ж<scannerꓸError>>(list)) {
                    errlist = append(errlist, (error)(new types_test_package.scanner_ΔErrorжerror(errΔ1)));
                }
            } else {
                errlist = append(errlist, err);
            }
        }
    }
    files = move2heap(filesᴷ, files);
    return (files, errlist);
}

internal static (tokenꓸPosition, @string) unpackError(ж<token.FileSet> fset, error err) {
    switch (err.type()) {
    case ж<scannerꓸError> errΔ1: {
        return (errΔ1.Pos, errΔ1.Msg);
    }
    case typesꓸError errΔ1: {
        return (fset.Position(errΔ1.Pos), errΔ1.Msg);
    }}
    throw panic("unreachable");
}

// absDiff returns the absolute difference between x and y.
internal static nint absDiff(nint x, nint y) {
    if (x < y) {
        return y - x;
    }
    return x - y;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string prefixᶜ => ref prefixᶜᵇ;
private static readonly @string prefixᶜᵇ = "//"u8;

// parseFlags parses flags from the first line of the given source if the line
// starts with "//" (line comment) followed by "-" (possibly with spaces
// between). Otherwise the line is ignored.
internal static error parseFlags(slice<byte> src, ж<flag.FlagSet> flags) {
    // we must have a line comment that starts with a "-"
    @string prefix = prefixᶜ;
    if (!bytes.HasPrefix(src, slice<byte>(prefix))) {
        return default; // first line is not a line comment
    }
    src = src[(int)(len(prefix))..];
    nint i = bytes.Index(src, slice<byte>("-"u8)); if (i < 0 || len(bytes.TrimSpace(src.slice(0, i))) != 0) {
        return default; // comment doesn't start with a "-"
    }
    nint end = bytes.Index(src, slice<byte>("\n"u8));
    const nint maxLen = 256;
    if (end < 0 || end > maxLen) {
        return fmt.Errorf("flags comment line too long"u8);
    }

    return flags.Parse(strings.Fields((@string)(src.slice(0, end))));
}

// testFiles type-checks the package consisting of the given files, and
// compares the resulting errors with the ERROR annotations in the source.
// Except for manual tests, each package is type-checked twice, once without
// use of Alias types, and once with Alias types.
//
// The srcs slice contains the file content for the files named in the
// filenames slice. The colDelta parameter specifies the tolerance for position
// mismatch when comparing errors. The manual parameter specifies whether this
// is a 'manual' test.
//
// If provided, opts may be used to mutate the Config before type-checking.
internal static void testFiles(ж<testing.T> t, slice<@string> filenames, slice<slice<byte>> srcs, bool manual, params Span<Action<ж<types.Config>>> optsʗp) {
    var opts = optsʗp.sslice();

    // Alias types are enabled by default
    testFilesImpl(t, filenames, srcs, manual, opts.ꓸꓸꓸ);
    if (!manual) {
        t.Setenv(godebugˢ, gotypesalias0ˢ);
        testFilesImpl(t, filenames, srcs, manual, opts.ꓸꓸꓸ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noSourceFilesˢ = (@string)"no source files"u8;
internal static ref readonly @string noPackageˢ => ref noPackageˢᵇ;
private static readonly @string noPackageˢᵇ = "<no package>"u8;
internal static ref readonly @string traceˢ => ref traceˢᵇ;
private static readonly @string traceˢᵇ = "_Trace"u8;
internal static ref readonly @string langˢ => ref langˢᵇ;
private static readonly @string langˢᵇ = "lang"u8;
internal static ref readonly @string goexperimentˢ => ref goexperimentˢᵇ;
private static readonly @string goexperimentˢᵇ = "goexperiment"u8;
internal static ref readonly @string fakeImportCˢ => ref fakeImportCˢᵇ;
private static readonly @string fakeImportCˢᵇ = "fakeImportC"u8;
internal static ref readonly @string gotypesaliasˢ => ref gotypesaliasˢᵇ;
private static readonly @string gotypesaliasˢᵇ = "gotypesalias"u8;
internal static ref readonly @string erroRxˢ => ref erroRxˢᵇ;
private static readonly @string erroRxˢᵇ = "^ ERRORx? "u8;
internal static ref readonly @string errorˢ => ref errorˢᵇ;
private static readonly @string errorˢᵇ = " ERROR "u8;
internal static ref readonly @string erroRxˢ2 => ref erroRxˢ2ᵇ;
private static readonly @string erroRxˢ2ᵇ = " ERRORx "u8;

internal static void testFilesImpl(ж<testing.T> t, slice<@string> filenames, slice<slice<byte>> srcs, bool manual, params Span<Action<ж<types.Config>>> optsʗp) {
    GoFrame ᒐ = default;
    try {
        var opts = optsʗp.sslice();
        var indicesᴷ = new stackAppend<nint>();

        if (len(filenames) == 0) {
            t.Fatal(noSourceFilesˢ);
        }

        // parse files
        ref var errlist = ref heap<slice<error>>(out var Ꮡerrlist);
        (var files, errlist) = parseFiles(t, filenames, srcs, parser.AllErrors);
        @string pkgName = noPackageˢ;
        if (len(files) > 0) {
            pkgName = files[0].Name.Name;
        }
        var listErrors = manual && !verifyErrors.Value;
        if (listErrors && len(errlist) > 0) {
            t.Errorf("--- %s:", pkgName);
            foreach (var (_, err) in range(errlist)) {
                t.Error(err);
            }
        }

        // set up typechecker
        ref var conf = ref heap(new types.Config(), out var Ꮡconf);
        boolFieldAddr(Ꮡconf, traceˢ).Value = manual && testing.Verbose();
        conf.Importer = defaultImporter(fset);
        conf.Error = (error err) => {
            GoFrame ᒐ = default;
            try {
                if (haltOnError.Value) {
                    defer(ᴛ1 => throw panic(ᴛ1), err, ref ᒐ);
                }
                if (listErrors) {
                    t.Error(err);
                    return;
                }
                // Ignore secondary error messages starting with "\t";
                // they are clarifying messages for a primary error.
                if (!strings.Contains(err.Error(), ": \t")) {
                    Ꮡerrlist.ValueSlot = append(Ꮡerrlist.ValueSlot, err);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        };

        // apply custom configuration
        foreach (var (_, opt) in range(opts)) {
            opt(Ꮡconf);
        }

        // apply flag setting (overrides custom configuration)
        ref var goexperiment = ref heap(new @string(), out var Ꮡgoexperiment);
        ref var gotypesalias = ref heap(new @string(), out var Ꮡgotypesalias);
        var flags = flag.NewFlagSet(""u8, flag.PanicOnError);
        flags.StringVar(Ꮡconf.ᏑGoVersion, langˢ, ""u8, ""u8);
        flags.StringVar(Ꮡgoexperiment, goexperimentˢ, ""u8, ""u8);
        flags.BoolVar(Ꮡconf.ᏑFakeImportC, fakeImportCˢ, false, ""u8);
        flags.StringVar(Ꮡgotypesalias, gotypesaliasˢ, ""u8, ""u8);
        {
            var err = parseFlags(srcs[0], flags); if (err != nil) {
                t.Fatal(err);
            }
        }

        if (goexperiment != ""u8) {
            var revert = setGOEXPERIMENT(goexperiment);
            var revertʗ1 = revert;
            defer(revertʗ1, ref ᒐ);
        }

        // By default, gotypesalias is not set.
        if (gotypesalias != ""u8) {
            t.Setenv(godebugˢ, "gotypesalias="u8 + gotypesalias);
        }

        // Provide Config.Info with all maps so that info recording is tested.
        ref var info = ref heap<typesꓸInfo>(new typesꓸInfo(
            Types: new map<ast.Expr, types.TypeAndValue>(),
            Instances: new map<ж<ast.Ident>, types.Instance>(),
            Defs: new map<ж<ast.Ident>, types.Object>(),
            Uses: new map<ж<ast.Ident>, types.Object>(),
            Implicits: new map<ast.Node, types.Object>(),
            Selections: new map<ж<ast.SelectorExpr>, ж<types.Selection>>(),
            Scopes: new map<ast.Node, ж<typesꓸScope>>(),
            FileVersions: new map<ж<ast.File>, @string>()
        ), out var Ꮡinfo);

        // typecheck
        Ꮡconf.Check(pkgName, fset, files, Ꮡinfo);
        if (listErrors) {
            return;
        }

        // collect expected errors
        var errmap = new map<@string, map<nint, slice<comment>>>();
        foreach (var (i, filename) in range(filenames)) {
            var m = commentMap(srcs[i], regexp.MustCompile(erroRxˢ)); if (len(m) > 0) {
                errmap[filename] = m;
            }
        }

        // match against found errors
        slice<nint> indices = default; // list indices of matching errors, reused for each error
        foreach (var (_, err) in range(errlist)) {
            var (gotPos, gotMsg) = unpackError(fset, err);

            // find list of errors for the respective error line
            @string filename = gotPos.Filename;
            var filemap = errmap[filename];
            nint line = gotPos.Line;
            slice<comment> errList = default;
            if (filemap != nil) {
                errList = filemap[line];
            }

            // At least one of the errors in errList should match the current error.
            indices = indices[..0];
            foreach (var (i, want) in range(errList)) {
                var (pattern, substr) = strings.CutPrefix(want.text, errorˢ);
                if (!substr) {
                    bool found = default;
                    (pattern, found) = strings.CutPrefix(want.text, erroRxˢ2);
                    if (!found) {
                        throw panic("unreachable");
                    }
                }
                var (unquoted, errΔ1) = strconv.Unquote(strings.TrimSpace(pattern));
                if (errΔ1 != nil) {
                    t.Errorf("%s:%d:%d: invalid ERROR pattern (cannot unquote %s)", filename, line, want.col, pattern);
                    continue;
                }
                if (substr) {
                    if (!strings.Contains(gotMsg, unquoted)) {
                        continue;
                    }
                } else {
                    var (rx, errΔ2) = regexp.Compile(unquoted);
                    if (errΔ2 != nil) {
                        t.Errorf("%s:%d:%d: %v", filename, line, want.col, errΔ2);
                        continue;
                    }
                    if (!rx.MatchString(gotMsg)) {
                        continue;
                    }
                }
                indices = append(indicesᴷ, indices, i);
            }
            if (len(indices) == 0) {
                t.Errorf("%s: no error expected: %q", gotPos, gotMsg);
                continue;
            }
            // len(indices) > 0

            // If there are multiple matching errors, select the one with the closest column position.
            nint index = -1; // index of matching error
            nint delta = default;
            foreach (var (_, i) in range(indices)) {
                nint d = absDiff(gotPos.Column, errList[i].col); if (index < 0 || d < delta) {
                    (index, delta) = (i, d);
                }
            }

            // The closest column position must be within expected colDelta.
            const nint colDelta = 0; // go/types errors are positioned correctly
            if (delta > colDelta) {
                t.Errorf("%s: got col = %d; want %d", gotPos, gotPos.Column, errList[index].col);
            }

            // eliminate from errList
            nint n = len(errList) - 1; if (n > 0) {
                // not the last entry - slide entries down (don't reorder)
                copy(errList.slice(index), errList.slice(index + 1));
                filemap[line] = errList.slice(0, n);
            } else {
                // last entry - remove errList from filemap
                delete(filemap, line);
            }

            // if filemap is empty, eliminate from errmap
            if (len(filemap) == 0) {
                delete(errmap, filename);
            }
        }

        // there should be no expected errors left
        if (len(errmap) > 0) {
            t.Errorf("--- %s: unreported errors:", pkgName);
            foreach (var (filename, filemap) in errmap) {
                foreach (var (line, errList) in filemap) {
                    foreach (var (_, err) in range(errList)) {
                        t.Errorf("%s:%d:%d: %s", filename, line, err.col, err.text);
                    }
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string go116codeˢ => ref go116codeˢᵇ;
private static readonly @string go116codeˢᵇ = "go116code"u8;

internal static errors.Code readCode(typesꓸError err) {
    var v = reflect.ValueOf(err);
    return (errors.Code)(nint)v.FieldByName(go116codeˢ).Int();
}

// boolFieldAddr(conf, name) returns the address of the boolean field conf.<name>.
// For accessing unexported fields.
internal static ж<bool> boolFieldAddr(ж<types.Config> conf, @string name) {
    var v = reflect.Indirect(reflect.ValueOf(conf.OrTypedNil()));
    return (ж<bool>)(uintptr)(v.FieldByName(name).Addr().UnsafePointer());
}

// stringFieldAddr(conf, name) returns the address of the string field conf.<name>.
// For accessing unexported fields.
internal static ж<@string> stringFieldAddr(ж<types.Config> conf, @string name) {
    var v = reflect.Indirect(reflect.ValueOf(conf.OrTypedNil()));
    return (ж<@string>)(uintptr)(v.FieldByName(name).Addr().UnsafePointer());
}

// setGOEXPERIMENT overwrites the existing buildcfg.Experiment with a new one
// based on the provided goexperiment string. Calling the result function
// (typically via defer), reverts buildcfg.Experiment to the prior value.
// For testing use, only.
internal static Action setGOEXPERIMENT(@string goexperiment) {
    var (exp, err) = buildcfg.ParseGOEXPERIMENT(runtime.GOOS, runtime.GOARCH, goexperiment);
    if (err != nil) {
        throw panic(err);
    }
    ref var old = ref heap<buildcfg.ExperimentFlags>(buildcfg.Experiment, out var Ꮡold);
    buildcfg.Experiment = exp.Value;
    var oldʗ1 = old;
    return () => {
        buildcfg.Experiment = oldʗ1;
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataManualGoˢ => ref testdataManualGoˢᵇ;
private static readonly @string testdataManualGoˢᵇ = "testdata/manual.go"u8;
internal static readonly object testManualMustHaveOnlyˢ = (@string)"TestManual: must have only one directory argument"u8;

// TestManual is for manual testing of a package - either provided
// as a list of filenames belonging to the package, or a directory
// name containing the package files - after the test arguments
// (and a separating "--"). For instance, to test the package made
// of the files foo.go and bar.go, use:
//
//	go test -run Manual -- foo.go bar.go
//
// If no source arguments are provided, the file testdata/manual.go
// is used instead.
// Provide the -verify flag to verify errors against ERROR comments
// in the input files rather than having a list of errors reported.
// The accepted Go language version can be controlled with the -lang
// flag.
public static void TestManual(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(t));

    var filenames = flag.Args();
    if (len(filenames) == 0) {
        filenames = slice<@string>([filepath.FromSlash(testdataManualGoˢ)]);
    }

    var (info, err) = os.Stat(filenames[0]);
    if (err != nil) {
        t.Fatalf("TestManual: %v", err);
    }

    DefPredeclaredTestFuncs();
    if (info.IsDir()) {
        if (len(filenames) > 1) {
            t.Fatal(testManualMustHaveOnlyˢ);
        }
        testDir(t, filenames[0], true);
    } else {
        testPkg(t, filenames, true);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string packageLongconstConstSˢ => ref packageLongconstConstSˢᵇ;
private static readonly @string packageLongconstConstSˢᵇ = @"package longconst; const _ = %s /* ERROR ""constant overflow"" */; const _ = %s // ERROR ""excessively long constant"""u8;

public static void TestLongConstants(ж<testing.T> t) {
    @string format = packageLongconstConstSˢ;
    @string src = fmt.Sprintf(format, strings.Repeat("1", 9999), strings.Repeat("1", 10001));
    testFiles(t, ["longconst.go"], [slice<byte>(src)], false);
}

internal static Action<ж<types.Config>> withSizes(types.Sizes sizes) {
    return (ж<types.Config> cfg) => {
        cfg.Sizes = sizes;
    };
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ13 => ref srcᶜ13ᵇ;
private static readonly @string srcᶜ13ᵇ = @"package index; var s []byte; var _ = s[int64 /* ERRORx ""int64\\(1\\) << 40 \\(.*\\) overflows int"" */ (1) << 40]"u8;

// TestIndexRepresentability tests that constant index operands must
// be representable as int even if they already have a type that can
// represent larger values.
public static void TestIndexRepresentability(ж<testing.T> t) {
    @string src = srcᶜ13;
    testFiles(t, ["index.go"], [slice<byte>(src)], false, withSizes(new types.StdSizesжSizes(Ꮡ(new StdSizes(4, 4)))));
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ14 => ref srcᶜ14ᵇ;
private static readonly @string srcᶜ14ᵇ = @"package issue47243; var a uint64; var _ = a << uint64(4294967296)"u8;

public static void TestIssue47243_TypedRHS(ж<testing.T> t) {
    // The RHS of the shift expression below overflows uint on 32bit platforms,
    // but this is OK as it is explicitly typed.
    @string src = srcᶜ14; // uint64(1<<32)
    testFiles(t, ["p.go"], [slice<byte>(src)], false, withSizes(new types.StdSizesжSizes(Ꮡ(new StdSizes(4, 4)))));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string internalTypesTestdataˢ => ref internalTypesTestdataˢᵇ;
private static readonly @string internalTypesTestdataˢᵇ = "../../internal/types/testdata/check"u8;

public static void TestCheck(ж<testing.T> t) {
    DefPredeclaredTestFuncs();
    testDirFiles(t, internalTypesTestdataˢ, false);
}
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string internalTypesTestdataˢ2 => ref internalTypesTestdataˢ2ᵇ;
private static readonly @string internalTypesTestdataˢ2ᵇ = "../../internal/types/testdata/spec"u8;

public static void TestSpec(ж<testing.T> t) { testDirFiles(t, internalTypesTestdataˢ2, false); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string internalTypesTestdataˢ3 => ref internalTypesTestdataˢ3ᵇ;
private static readonly @string internalTypesTestdataˢ3ᵇ = "../../internal/types/testdata/examples"u8;

public static void TestExamples(ж<testing.T> t) { testDirFiles(t, internalTypesTestdataˢ3, false); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string internalTypesTestdataˢ4 => ref internalTypesTestdataˢ4ᵇ;
private static readonly @string internalTypesTestdataˢ4ᵇ = "../../internal/types/testdata/fixedbugs"u8;

public static void TestFixedbugs(ж<testing.T> t) { testDirFiles(t, internalTypesTestdataˢ4, false); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataLocalˢ => ref testdataLocalˢᵇ;
private static readonly @string testdataLocalˢᵇ = "testdata/local"u8;

public static void TestLocal(ж<testing.T> t) { testDirFiles(t, testdataLocalˢ, false); }

internal static void testDirFiles(ж<testing.T> t, @string dir, bool manual) {
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(t));
    dir = filepath.FromSlash(dir);

    var (fis, err) = os.ReadDir(dir);
    if (err != nil) {
        t.Error(err);
        return;
    }

    foreach (var (_, fi) in range(fis)) {
        @string path = filepath.Join(dir, fi.Name());

        // If fi is a directory, its files make up a single package.
        if (fi.IsDir()) {
            testDir(t, path, manual);
        } else {
            t.Run(filepath.Base(path), (ж<testing.T> tΔ1) => {
                testPkg(tΔ1, [path], manual);
            });
        }
    }
}

internal static void testDir(ж<testing.T> t, @string dir, bool manual) {
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(t));

    var (fis, err) = os.ReadDir(dir);
    if (err != nil) {
        t.Error(err);
        return;
    }

    slice<@string> filenames = default;
    foreach (var (_, fi) in range(fis)) {
        filenames = append(filenames, filepath.Join(dir, fi.Name()));
    }

    var filenamesʗ1 = filenames;
    t.Run(filepath.Base(dir), (ж<testing.T> tΔ1) => {
        testPkg(tΔ1, filenamesʗ1, manual);
    });
}

internal static void testPkg(ж<testing.T> t, slice<@string> filenames, bool manual) {
    var srcs = new slice<slice<byte>>(len(filenames));
    foreach (var (i, filename) in range(filenames)) {
        var (src, err) = os.ReadFile(filename);
        if (err != nil) {
            t.Fatalf("could not read %s: %v", filename, err);
        }
        srcs[i] = src;
    }
    testFiles(t, filenames, srcs, manual);
}

} // end types_test_package
