// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file tests types.Check by using it to
// typecheck the standard library and tests.
namespace go.go;

using errors = errors_package;
using fmt = fmt_package;
using ast = global::go.go.ast_package;
using Δbuild = global::go.go.build_package;
using importer = global::go.go.importer_package;
using parser = global::go.go.parser_package;
using scanner = global::go.go.scanner_package;
using token = global::go.go.token_package;
using testenv = global::go.@internal.testenv_package;
using os = os_package;
using filepath = global::go.path.filepath_package;
using runtime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using time = time_package;
using static global::go.go.types_package;
using fs = io.fs_package;
using global::go.@internal;
using global::go.go;
using global::go.path;
using io;
using io = io_package;
using static global::go.go.types_internal_test_package;
using types = global::go.go.types_package;
using ꓸꓸꓸstring = Span<@string>;

partial class types_test_package {

// The cmd/*/internal packages may have been deleted as part of a binary
// release. Import from source instead.
//
// (See https://golang.org/issue/43232 and
// https://github.com/golang/build/blob/df58bbac082bc87c4a3cdfe336d1ffe60bbaa916/cmd/release/release.go#L533-L545.)
//
// Use the same importer for all std lib tests to
// avoid repeated importing of the same packages.
internal static types.Importer stdLibImporter = importer.ForCompiler(token.NewFileSet(), "source"u8, default!);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in short mode"u8;
internal static readonly @string srcˢ = "src"u8;
internal static readonly object packagesTypecheckedInˢ = (@string)"packages typechecked in"u8;

public static void TestStdlib(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(Ꮡt));
    // Collect non-test files.
    var dirFiles = new map<@string, slice<@string>>();
    @string root = filepath.Join(testenv.GOROOT(new types_test_package.testing_TжTB(Ꮡt)), srcˢ);
    var dirFilesʗ1 = dirFiles;
    walkPkgDirs(root, (@string dir, slice<@string> filenames) => {
        dirFilesʗ1[dir] = filenames;
    }, Ꮡt.Error);
    var c = Ꮡ(new stdlibChecker(
        dirFiles: dirFiles,
        pkgs: new map<@string, ж<futurePackage>>()
    ));
    var start = time.Now();
    // Though we read files while parsing, type-checking is otherwise CPU bound.
    //
    // This doesn't achieve great CPU utilization as many packages may block
    // waiting for a common import, but in combination with the non-deterministic
    // map iteration below this should provide decent coverage of concurrent
    // type-checking (see golang/go#47729).
    var cpulimit = new channel<EmptyStruct>(runtime.GOMAXPROCS(0));
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    foreach (var (dir, _) in dirFiles) {
        @string dirΔ1 = dir;
        cpulimit.ᐸꟷ(new EmptyStruct());
        Ꮡwg.Add(1);
        var cʗ1 = c;
        var cpulimitʗ1 = cpulimit;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                var cpulimitʗ2 = cpulimitʗ1;
                defer(() => {
                    Ꮡwg.Done();
                    ᐸꟷ(cpulimitʗ2);
                }, ref ᒐ);
                var (_, err) = cʗ1.getDirPackage(dirΔ1);
                if (err != default!) {
                    Ꮡt.Errorf("error checking %s: %v"u8, dirΔ1, err);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
    if (testing.Verbose()) {
        fmt.Println(len(dirFiles), packagesTypecheckedInˢ, time.Since(start));
    }
}

// stdlibChecker implements concurrent type-checking of the packages defined by
// dirFiles, which must define a closed set of packages (such as GOROOT/src).
[GoType] partial struct stdlibChecker {
    internal map<@string, slice<@string>> dirFiles; // non-test files per directory; must be pre-populated
    internal sync.Mutex mu;
    internal map<@string, ж<futurePackage>> pkgs; // future cache of type-checking results
}

// A futurePackage is a future result of type-checking.
[GoType] partial struct futurePackage {
    internal channel<EmptyStruct> done; // guards pkg and err
    internal ж<types.Package> pkg;
    internal error err;
}

[GoRecv] internal static (ж<types.Package>, error) Import(this ref stdlibChecker c, @string path) {
    throw panic("unimplemented: use ImportFrom");
}

internal static (ж<types.Package>, error) ImportFrom(this ж<stdlibChecker> Ꮡc, @string path, @string dir, types.ImportMode _) {
    if (path == "unsafe"u8) {
        // unsafe cannot be type checked normally.
        return (Unsafe, default!);
    }
    var (p, err) = Δbuild.ᏑDefault.Import(path, dir, Δbuild.FindOnly);
    if (err != default!) {
        return (default!, err);
    }
    (var pkg, err) = Ꮡc.getDirPackage((~p).Dir);
    if (pkg != nil) {
        // As long as pkg is non-nil, avoid redundant errors related to failed
        // imports. TestStdlib will collect errors once for each package.
        return (pkg, default!);
    }
    return (default!, err);
}

// getDirPackage gets the package defined in dir from the future cache.
//
// If this is the first goroutine requesting the package, getDirPackage
// type-checks.
internal static (ж<types.Package>, error) getDirPackage(this ж<stdlibChecker> Ꮡc, @string dir) {
    ref var c = ref Ꮡc.DerefOrNull();

    c.mu.Lock();
    var (fut, ok) = c.pkgs[dir, ꟷ];
    if (!ok){
        // First request for this package dir; type check.
        fut = Ꮡ(new futurePackage(
            done: new channel<EmptyStruct>(0)
        ));
        c.pkgs[dir] = fut;
        var (files, okΔ1) = c.dirFiles[dir, ꟷ];
        c.mu.Unlock();
        if (!okΔ1){
            fut.Value.err = fmt.Errorf("no files for %s"u8, dir);
        } else {
            // Using dir as the package path here may be inconsistent with the behavior
            // of a normal importer, but is sufficient as dir is by construction unique
            // to this package.
            (fut.Value.pkg, fut.Value.err) = typecheckFiles(dir, files, new types_test_package.stdlibCheckerжImporter(Ꮡc));
        }
        close((~fut).done);
    } else {
        // Otherwise, await the result.
        c.mu.Unlock();
        ᐸꟷ((~fut).done);
    }
    return ((~fut).pkg, (~fut).err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goBuildˢ = "go:build "u8;
internal static readonly @string skipˢ = "skip"u8;

// firstComment returns the contents of the first non-empty comment in
// the given file, "skip", or the empty string. No matter the present
// comments, if any of them contains a build tag, the result is always
// "skip". Only comments before the "package" token and within the first
// 4K of the file are considered.
internal static @string firstComment(@string filename) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(filename);
        if (err != default!) {
            return ""u8;
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        array<byte> src = new(4096); /* (4 << (int)(10)) */                      // read at most 4KB
        var (n, _) = f.Read(src[..]);
        @string first = default!;
        scanner.Scanner s = default!;
        s.Init(fset.AddFile(""u8, fset.Base(), n), src[..(int)(n)], default!, /* ignore errors */
 scanner.ScanComments);
        while (ᐧ) {
            var (_, tok, lit) = s.Scan();
            var exprᴛ1 = tok;
            if (exprᴛ1 == token.COMMENT) {
                if (lit[1] == (rune)'*') {
                    // remove trailing */ of multi-line comment
                    lit = lit[..(int)(len(lit) - 2)];
                }
                @string contents = strings.TrimSpace(lit[2..]);
                if (strings.HasPrefix(contents, goBuildˢ)) {
                    return skipˢ;
                }
                if (first == ""u8) {
                    first = contents; // contents may be "" but that's ok
                }
            }
            else if (exprᴛ1 == token.PACKAGE || exprᴛ1 == token.EOF) {
                return first;
            }

        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string versionˢ = "VERSION"u8;
internal static readonly @string goexperimentˢ2 = "-goexperiment"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string prefixᶜ3 = "-lang="u8;

// continue as we may still see build tags
internal static void testTestDir(ж<testing.T> Ꮡt, @string path, params ꓸꓸꓸstring ignoreʗp) {
    var ignore = ignoreʗp.sslice();

    ref var t = ref Ꮡt.DerefOrNull();
    var (files, err) = os.ReadDir(path);
    if (err != default!) {
        // cmd/distpack deletes GOROOT/test, so skip the test if it isn't present.
        // cmd/distpack also requires GOROOT/VERSION to exist, so use that to
        // suppress false-positive skips.
        {
            var (_, errΔ1) = os.Stat(filepath.Join(testenv.GOROOT(new types_test_package.testing_TжTB(Ꮡt)), testˢ)); if (os.IsNotExist(errΔ1)) {
                {
                    var (_, errΔ2) = os.Stat(filepath.Join(testenv.GOROOT(new types_test_package.testing_TжTB(Ꮡt)), versionˢ)); if (errΔ2 == default!) {
                        Ꮡt.Skipf("skipping: GOROOT/test not present"u8);
                    }
                }
            }
        }
        Ꮡt.Fatal(err);
    }
    var excluded = new map<@string, bool>();
    foreach (var (_, filename) in ignore) {
        excluded[filename] = true;
    }
    var fset = token.NewFileSet();
    foreach (var (_, f) in files) {
        // filter directory contents
        if (f.IsDir() || !strings.HasSuffix(f.Name(), ".go"u8) || excluded[f.Name()]) {
            continue;
        }
        // get per-file instructions
        var expectErrors = false;
        @string filename = filepath.Join(path, f.Name());
        @string goVersion = ""u8;
        {
            @string comment = firstComment(filename); if (comment != ""u8) {
                if (strings.Contains(comment, goexperimentˢ2)) {
                    continue; // ignore this file
                }
                var fields = strings.Fields(comment);
                var exprᴛ1 = fields[0];
                if (exprᴛ1 == "skip"u8 || exprᴛ1 == "compiledir"u8) {
                    continue; // ignore this file
                }
                else if (exprᴛ1 == "errorcheck"u8) {
                    expectErrors = true;
                    foreach (var (_, arg) in fields[1..]) {
                        if (arg == "-0"u8 || arg == "-+"u8 || arg == "-std"u8) {
                            // Marked explicitly as not expecting errors (-0),
                            // or marked as compiling runtime/stdlib, which is only done
                            // to trigger runtime/stdlib-only error output.
                            // In both cases, the code should typecheck.
                            expectErrors = false;
                            break;
                        }
                        @string prefix = prefixᶜ3;
                        if (strings.HasPrefix(arg, prefix)) {
                            goVersion = arg[(int)(len(prefix))..];
                        }
                    }
                }

            }
        }
        // parse and type-check file
        var (@file, errΔ3) = parser.ParseFile(fset, filename, default!, 0);
        if (errΔ3 == default!) {
            ref var conf = ref heap<types.Config>(out var Ꮡconf);
            conf = new Config(
                GoVersion: goVersion,
                Importer: stdLibImporter
            );
            (_, errΔ3) = Ꮡconf.Check(filename, fset, new ж<ast.File>[]{@file}.slice(), nil);
        }
        if (expectErrors){
            if (errΔ3 == default!) {
                Ꮡt.Errorf("expected errors but found none in %s"u8, filename);
            }
        } else {
            if (errΔ3 != default!) {
                Ꮡt.Error(errΔ3);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cmplxdivideGoˢ = "cmplxdivide.go"u8;
internal static readonly @string directiveGoˢ = "directive.go"u8;
internal static readonly @string directive2Goˢ = "directive2.go"u8;
internal static readonly @string embedfuncGoˢ = "embedfunc.go"u8;
internal static readonly @string embedversGoˢ = "embedvers.go"u8;
internal static readonly @string linkname2Goˢ = "linkname2.go"u8;
internal static readonly @string linkname3Goˢ = "linkname3.go"u8;

public static void TestStdTest(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(Ꮡt));
    if (testing.Short() && testenv.Builder() == ""u8) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    testTestDir(Ꮡt, filepath.Join(testenv.GOROOT(new types_test_package.testing_TжTB(Ꮡt)), testˢ),
        cmplxdivideGoˢ, // also needs file cmplxdivide1.go - ignore

        directiveGoˢ, // tests compiler rejection of bad directive placement - ignore

        directive2Goˢ, // tests compiler rejection of bad directive placement - ignore

        embedfuncGoˢ, // tests //go:embed

        embedversGoˢ, // tests //go:embed

        linkname2Goˢ, // go/types doesn't check validity of //go:xxx directives

        linkname3Goˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fixedbugsˢ = "fixedbugs"u8;
internal static readonly @string bug248Goˢ = "bug248.go"u8;
internal static readonly @string bug302Goˢ = "bug302.go"u8;
internal static readonly @string bug369Goˢ = "bug369.go"u8;
internal static readonly @string bug398Goˢ = "bug398.go"u8;
internal static readonly @string issue6889Goˢ = "issue6889.go"u8;
internal static readonly @string issue11362Goˢ = "issue11362.go"u8;
internal static readonly @string issue16369Goˢ = "issue16369.go"u8;
internal static readonly @string issue18459Goˢ = "issue18459.go"u8;
internal static readonly @string issue18882Goˢ = "issue18882.go"u8;
internal static readonly @string issue20027Goˢ = "issue20027.go"u8;
internal static readonly @string issue20529Goˢ = "issue20529.go"u8;
internal static readonly @string issue22200Goˢ = "issue22200.go"u8;
internal static readonly @string issue22200bGoˢ = "issue22200b.go"u8;
internal static readonly @string issue25507Goˢ = "issue25507.go"u8;
internal static readonly @string issue20780Goˢ = "issue20780.go"u8;
internal static readonly @string bug251Goˢ = "bug251.go"u8;
internal static readonly @string issue42058aGoˢ = "issue42058a.go"u8;
internal static readonly @string issue42058bGoˢ = "issue42058b.go"u8;
internal static readonly @string issue48097Goˢ = "issue48097.go"u8;
internal static readonly @string issue48230Goˢ = "issue48230.go"u8;
internal static readonly @string issue49767Goˢ = "issue49767.go"u8;
internal static readonly @string issue49814Goˢ = "issue49814.go"u8;
internal static readonly @string issue56103Goˢ = "issue56103.go"u8;
internal static readonly @string issue52697Goˢ = "issue52697.go"u8;
internal static readonly @string bug514Goˢ = "bug514.go"u8;
internal static readonly @string issue40954Goˢ = "issue40954.go"u8;
internal static readonly @string issue42032Goˢ = "issue42032.go"u8;
internal static readonly @string issue42076Goˢ = "issue42076.go"u8;
internal static readonly @string issue46903Goˢ = "issue46903.go"u8;
internal static readonly @string issue51733Goˢ = "issue51733.go"u8;
internal static readonly @string notinheap2Goˢ = "notinheap2.go"u8;
internal static readonly @string notinheap3Goˢ = "notinheap3.go"u8;

// go/types doesn't check validity of //go:xxx directives
public static void TestStdFixed(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(Ꮡt));
    if (testing.Short() && testenv.Builder() == ""u8) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    testTestDir(Ꮡt, filepath.Join(testenv.GOROOT(new types_test_package.testing_TжTB(Ꮡt)), testˢ, fixedbugsˢ),
        bug248Goˢ, bug302Goˢ, bug369Goˢ, // complex test instructions - ignore

        bug398Goˢ, // go/types doesn't check for anonymous interface cycles (go.dev/issue/56103)

        issue6889Goˢ, // gc-specific test

        issue11362Goˢ, // canonical import path check

        issue16369Goˢ, // go/types handles this correctly - not an issue

        issue18459Goˢ, // go/types doesn't check validity of //go:xxx directives

        issue18882Goˢ, // go/types doesn't check validity of //go:xxx directives

        issue20027Goˢ, // go/types does not have constraints on channel element size

        issue20529Goˢ, // go/types does not have constraints on stack size

        issue22200Goˢ, // go/types does not have constraints on stack size

        issue22200bGoˢ, // go/types does not have constraints on stack size

        issue25507Goˢ, // go/types does not have constraints on stack size

        issue20780Goˢ, // go/types does not have constraints on stack size

        bug251Goˢ, // go.dev/issue/34333 which was exposed with fix for go.dev/issue/34151

        issue42058aGoˢ, // go/types does not have constraints on channel element size

        issue42058bGoˢ, // go/types does not have constraints on channel element size

        issue48097Goˢ, // go/types doesn't check validity of //go:xxx directives, and non-init bodyless function

        issue48230Goˢ, // go/types doesn't check validity of //go:xxx directives

        issue49767Goˢ, // go/types does not have constraints on channel element size

        issue49814Goˢ, // go/types does not have constraints on array size

        issue56103Goˢ, // anonymous interface cycles; will be a type checker error in 1.22

        issue52697Goˢ, // go/types does not have constraints on stack size
 // These tests requires runtime/cgo.Incomplete, which is only available on some platforms.
 // However, go/types does not know about build constraints.

        bug514Goˢ,
        issue40954Goˢ,
        issue42032Goˢ,
        issue42076Goˢ,
        issue46903Goˢ,
        issue51733Goˢ,
        notinheap2Goˢ,
        notinheap3Goˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string kenˢ = "ken"u8;

public static void TestStdKen(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(Ꮡt));
    testTestDir(Ꮡt, filepath.Join(testenv.GOROOT(new types_test_package.testing_TжTB(Ꮡt)), testˢ, kenˢ));
}

// Package paths of excluded packages.
internal static map<@string, bool> excluded = new map<@string, bool>{
    ["builtin"u8] = true,
    ["cmd/compile/internal/ssa/_gen"u8] = true
};

// printPackageMu synchronizes the printing of type-checked package files in
// the typecheckFiles function.
//
// Without synchronization, package files may be interleaved during concurrent
// type-checking.
internal static ж<sync.Mutex> ᏑprintPackageMu = new StandardBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex printPackageMu => ref ᏑprintPackageMu.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object packageˢ = (@string)"package"u8;

// typecheckFiles typechecks the given package files.
internal static (ж<types.Package>, error) typecheckFiles(@string path, slice<@string> filenames, types.Importer importer) {
    var fset = token.NewFileSet();
    // Parse package files.
    slice<ж<ast.File>> files = default!;
    foreach (var (_, filename) in filenames) {
        var (@file, errΔ1) = parser.ParseFile(fset, filename, default!, parser.AllErrors);
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        files = append(files, @file);
    }
    if (testing.Verbose()) {
        ᏑprintPackageMu.Lock();
        fmt.Println(packageˢ, (~(~files[0]).Name).Name);
        foreach (var (_, filename) in filenames) {
            fmt.Println((@string)"\t"u8, filename);
        }
        ᏑprintPackageMu.Unlock();
    }
    // Typecheck package files.
    ref var errs = ref heap<slice<error>>(out var Ꮡerrs);
    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new Config(
        Error: (error errΔ2) => {
            Ꮡerrs.ValueSlot = append(Ꮡerrs.ValueSlot, errΔ2);
        },
        Importer: importer
    );
    ref var info = ref heap<typesꓸInfo>(out var Ꮡinfo);
    info = new typesꓸInfo(Uses: new map<ж<ast.Ident>, types.Object>());
    var (pkg, _) = Ꮡconf.Check(path, fset, files, Ꮡinfo);
    var err = errors.Join(errs.ꓸꓸꓸ);
    if (err != default!) {
        return (pkg, err);
    }
    // Perform checks of API invariants.
    // All Objects have a package, except predeclared ones.
    var errorError = Universe.Lookup(errorˢ3).Type().Underlying()._<ж<types.Interface>>().ExplicitMethod(0); // (error).Error
    foreach (var (id, obj) in info.Uses) {
        var predeclared = AreEqual(obj, Universe.Lookup(obj.Name())) || AreEqual(obj, errorError);
        if (predeclared == (obj.Pkg() != nil)) {
            var posn = fset.Position(id.Pos());
            if (predeclared){
                return (default!, fmt.Errorf("%s: predeclared object with package: %s"u8, posn, obj));
            } else {
                return (default!, fmt.Errorf("%s: user-defined object without package: %s"u8, posn, obj));
            }
        }
    }
    return (pkg, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string asmˢ = "_asm"u8;

// pkgFilenames returns the list of package filenames for the given directory.
internal static (slice<@string>, error) pkgFilenames(@string dir, bool includeTest) {
    ref var ctxt = ref heap<Δbuild.Context>(out var Ꮡctxt);
    ctxt = Δbuild.Default;
    ctxt.CgoEnabled = false;
    var (pkg, err) = Ꮡctxt.ImportDir(dir, 0);
    if (err != default!) {
        {
            var (_, nogo) = err._<ж<Δbuild.NoGoError>>(ᐧ); if (nogo) {
                return (default!, default!); // no *.go files, not an error
            }
        }
        return (default!, err);
    }
    if (excluded[(~pkg).ImportPath]) {
        return (default!, default!);
    }
    if (slices.Contains(strings.Split((~pkg).ImportPath, "/"u8), asmˢ)) {
        // Submodules where not all dependencies are available.
        // See go.dev/issue/46027.
        return (default!, default!);
    }
    slice<@string> filenames = default!;
    foreach (var (_, name) in (~pkg).GoFiles) {
        filenames = append(filenames, filepath.Join((~pkg).Dir, name));
    }
    if (includeTest) {
        foreach (var (_, name) in (~pkg).TestGoFiles) {
            filenames = append(filenames, filepath.Join((~pkg).Dir, name));
        }
    }
    return (filenames, default!);
}

internal static void walkPkgDirs(@string dir, Action<@string, slice<@string>> pkgh, Actionꓸꓸꓸ<any> errh) {
    var w = new walker(pkgh, errh);
    w.walk(dir);
}

[GoType] partial struct walker {
    internal Action<@string, slice<@string>> pkgh;
    internal Actionꓸꓸꓸ<any> errh;
}

[GoRecv] internal static void walk(this ref walker w, @string dir) {
    var (files, err) = os.ReadDir(dir);
    if (err != default!) {
        w.errh(err);
        return;
    }
    // apply pkgh to the files in directory dir
    // Don't get test files as these packages are imported.
    (var pkgFiles, err) = pkgFilenames(dir, false);
    if (err != default!) {
        w.errh(err);
        return;
    }
    if (pkgFiles != default!) {
        w.pkgh(dir, pkgFiles);
    }
    // traverse subdirectories, but don't walk into testdata
    foreach (var (_, f) in files) {
        if (f.IsDir() && f.Name() != "testdata"u8) {
            w.walk(filepath.Join(dir, f.Name()));
        }
    }
}

} // end types_test_package
