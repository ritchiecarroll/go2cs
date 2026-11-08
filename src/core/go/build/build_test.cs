// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using testenv = global::go.@internal.testenv_package;
using io = io_package;
using maps = maps_package;
using os = os_package;
using filepath = global::go.path.filepath_package;
using reflect = reflect_package;
using runtime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using fs = global::go.io.fs_package;
using global::go.@internal;
using global::go.path;
using static global::go.go.build_package;
using ꓸꓸꓸstring = Span<@string>;

partial class build_internal_test_package {

public static void TestMain(ж<testing.M> m) {
    Default.GOROOT = testenv.GOROOT(default);
    os.Exit(m.Run());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string defaultˢ => ref defaultˢᵇ;
private static readonly @string defaultˢᵇ = "default"u8;
internal static ref readonly @string modifiedˢ => ref modifiedˢᵇ;
private static readonly @string modifiedˢᵇ = "modified"u8;

public static partial void TestMatch(ж<testing.T> t) {
    ref var ctxt = ref heap<global::go.go.build_package.Context>(Default, out var Ꮡctxt);
    @string what = defaultˢ;
    void match(@string tag, map<@string, bool> want) {
        t.Helper();
        var m = new map<@string, bool>();
        if (!Ꮡctxt.matchAuto(tag, m)) {
            t.Errorf("%s context should match %s, does not", what, tag);
        }
        if (!maps.Equal<map<@string, bool>, map<@string, bool>, @string, bool>(m, want)) {
            t.Errorf("%s tags = %v, want %v", tag, m, want);
        }
    }
    void nomatch(@string tag, map<@string, bool> want) {
        t.Helper();
        var m = new map<@string, bool>();
        if (Ꮡctxt.matchAuto(tag, m)) {
            t.Errorf("%s context should NOT match %s, does", what, tag);
        }
        if (!maps.Equal<map<@string, bool>, map<@string, bool>, @string, bool>(m, want)) {
            t.Errorf("%s tags = %v, want %v", tag, m, want);
        }
    }

    match(runtime.GOOS + ","u8 + runtime.GOARCH, new map<@string, bool>{[runtime.GOOS] = true, [runtime.GOARCH] = true});
    match(runtime.GOOS + ","u8 + runtime.GOARCH + ",!foo"u8, new map<@string, bool>{[runtime.GOOS] = true, [runtime.GOARCH] = true, ["foo"] = true});
    nomatch(runtime.GOOS + ","u8 + runtime.GOARCH + ",foo"u8, new map<@string, bool>{[runtime.GOOS] = true, [runtime.GOARCH] = true, ["foo"] = true});

    what = modifiedˢ;
    ctxt.BuildTags = ["foo"];
    match(runtime.GOOS + ","u8 + runtime.GOARCH, new map<@string, bool>{[runtime.GOOS] = true, [runtime.GOARCH] = true});
    match(runtime.GOOS + ","u8 + runtime.GOARCH + ",foo"u8, new map<@string, bool>{[runtime.GOOS] = true, [runtime.GOARCH] = true, ["foo"] = true});
    nomatch(runtime.GOOS + ","u8 + runtime.GOARCH + ",!foo"u8, new map<@string, bool>{[runtime.GOOS] = true, [runtime.GOARCH] = true, ["foo"] = true});
    match(runtime.GOOS + ","u8 + runtime.GOARCH + ",!bar"u8, new map<@string, bool>{[runtime.GOOS] = true, [runtime.GOARCH] = true, ["bar"] = true});
    nomatch(runtime.GOOS + ","u8 + runtime.GOARCH + ",bar"u8, new map<@string, bool>{[runtime.GOOS] = true, [runtime.GOARCH] = true, ["bar"] = true});
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataOtherˢ => ref testdataOtherˢᵇ;
private static readonly @string testdataOtherˢᵇ = "testdata/other"u8;
internal static ref readonly @string fileˢ => ref fileˢᵇ;
private static readonly @string fileˢᵇ = "./file"u8;
internal static readonly object fileˢ2 = (@string)"file"u8;
internal static ref readonly @string testdataOtherFileˢ => ref testdataOtherFileˢᵇ;
private static readonly @string testdataOtherFileˢᵇ = "testdata/other/file"u8;

public static void TestDotSlashImport(ж<testing.T> t) {
    var (p, err) = ImportDir(testdataOtherˢ, 0);
    if (err != nil) {
        t.Fatal(err);
    }
    if (len(p.Imports) != 1 || p.Imports[0] != "./file") {
        t.Fatalf("testdata/other: Imports=%v, want [./file]", p.Imports);
    }

    (var p1, err) = Import(fileˢ, testdataOtherˢ, 0);
    if (err != nil) {
        t.Fatal(err);
    }
    if (p1.Name != "file"u8) {
        t.Fatalf("./file: Name=%q, want %q", p1.Name, fileˢ2);
    }
    @string dir = filepath.Clean(testdataOtherFileˢ); // Clean to use \ on Windows
    if (p1.Dir != dir) {
        t.Fatalf("./file: Dir=%q, want %q", p1.Name, dir);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object importReturnedNilErrorˢ = (@string)@"Import("""") returned nil error."u8;
internal static readonly object importReturnedNilPackageˢ = (@string)@"Import("""") returned nil package."u8;

public static void TestEmptyImport(ж<testing.T> t) {
    var (p, err) = Import(""u8, testenv.GOROOT(new build_internal_test_package.testing_TжTB(t)), FindOnly);
    if (err == nil) {
        t.Fatal(importReturnedNilErrorˢ);
    }
    if (p == nil) {
        t.Fatal(importReturnedNilPackageˢ);
    }
    if (p.ImportPath != ""u8) {
        t.Fatalf("ImportPath=%q, want %q.", p.ImportPath, (@string)""u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataEmptyˢ => ref testdataEmptyˢᵇ;
private static readonly @string testdataEmptyˢᵇ = "testdata/empty"u8;
internal static readonly object importTestdataEmptyDidˢ = (@string)@"Import(""testdata/empty"") did not return NoGoError."u8;

public static void TestEmptyFolderImport(ж<testing.T> t) {
    var (_, err) = Import(".", testdataEmptyˢ, 0);
    var (_, ok) = err._<ж<global::go.go.build_package.NoGoError>>(ᐧ); if (!ok) {
        t.Fatal(importTestdataEmptyDidˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataMultiˢ => ref testdataMultiˢᵇ;
private static readonly @string testdataMultiˢᵇ = "testdata/multi"u8;
internal static readonly object importTestdataMultiDidˢ = (@string)@"Import(""testdata/multi"") did not return MultiplePackageError."u8;
internal static ref readonly @string mainˢ => ref mainˢᵇ;
private static readonly @string mainˢᵇ = "main"u8;

public static void TestMultiplePackageImport(ж<testing.T> t) {
    var (pkg, err) = Import(".", testdataMultiˢ, 0);

    var (mpe, ok) = err._<ж<global::go.go.build_package.MultiplePackageError>>(ᐧ);
    if (!ok) {
        t.Fatal(importTestdataMultiDidˢ);
    }
    var want = Ꮡ(new MultiplePackageError(
        Dir: filepath.FromSlash(testdataMultiˢ),
        Packages: ["main", "test_package"],
        Files: ["file.go", "file_appengine.go"]
    ));
    if (!reflect.DeepEqual(mpe.OrTypedNil(), want.OrTypedNil())) {
        t.Errorf("err = %#v; want %#v", mpe.OrTypedNil(), want.OrTypedNil());
    }

    // TODO(#45999): Since the name is ambiguous, pkg.Name should be left empty.
    @string wantName = mainˢ; if (pkg.Name != wantName) {
        t.Errorf("pkg.Name = %q; want %q", pkg.Name, wantName);
    }

    var wantGoFiles = slice<@string>(["file.go", "file_appengine.go"]); if (!slices.Equal<slice<@string>, @string>(pkg.GoFiles, wantGoFiles)) {
        t.Errorf("pkg.GoFiles = %q; want %q", pkg.GoFiles, wantGoFiles);
    }

    var wantInvalidFiles = slice<@string>(["file_appengine.go"]); if (!slices.Equal<slice<@string>, @string>(pkg.InvalidGoFiles, wantInvalidFiles)) {
        t.Errorf("pkg.InvalidGoFiles = %q; want %q", pkg.InvalidGoFiles, wantInvalidFiles);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goBuildˢ => ref goBuildˢᵇ;
private static readonly @string goBuildˢᵇ = "go/build"u8;

public static void TestLocalDirectory(ж<testing.T> t) {
    if (runtime.GOOS == "ios"u8) {
        t.Skipf("skipping on %s/%s, no valid GOROOT", runtime.GOOS, runtime.GOARCH);
    }

    var (cwd, err) = os.Getwd();
    if (err != nil) {
        t.Fatal(err);
    }

    (var p, err) = ImportDir(cwd, 0);
    if (err != nil) {
        t.Fatal(err);
    }
    if (p.ImportPath != "go/build"u8) {
        t.Fatalf("ImportPath=%q, want %q", p.ImportPath, goBuildˢ);
    }
}

partial struct shouldBuildTestsᴛ1 /*dyn*/ {
    internal @string name;
    internal @string content;
    internal map<@string, bool> tags;
    internal bool binaryOnly;
    internal bool shouldBuild;
    internal error err;
}
internal static slice<shouldBuildTestsᴛ1> shouldBuildTests = [
    new(
        name: "Yes",
        content: "// +build yes\n\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["yes"] = true},
        shouldBuild: true
    ),
    new(
        name: "Yes2",
        content: "//go:build yes\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["yes"] = true},
        shouldBuild: true
    ),
    new(
        name: "Or",
        content: "// +build no yes\n\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["yes"] = true, ["no"] = true},
        shouldBuild: true
    ),
    new(
        name: "Or2",
        content: "//go:build no || yes\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["yes"] = true, ["no"] = true},
        shouldBuild: true
    ),
    new(
        name: "And",
        content: "// +build no,yes\n\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["yes"] = true, ["no"] = true},
        shouldBuild: false
    ),
    new(
        name: "And2",
        content: "//go:build no && yes\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["yes"] = true, ["no"] = true},
        shouldBuild: false
    ),
    new(
        name: "Cgo",
        content: "// +build cgo\n\n"u8 + "// Copyright The Go Authors.\n\n"u8 + "// This package implements parsing of tags like\n"u8 + "// +build tag1\n"u8 + "package build"u8,
        tags: new map<@string, bool>{["cgo"] = true},
        shouldBuild: false
    ),
    new(
        name: "Cgo2",
        content: "//go:build cgo\n"u8 + "// Copyright The Go Authors.\n\n"u8 + "// This package implements parsing of tags like\n"u8 + "// +build tag1\n"u8 + "package build"u8,
        tags: new map<@string, bool>{["cgo"] = true},
        shouldBuild: false
    ),
    new(
        name: "AfterPackage",
        content: "// Copyright The Go Authors.\n\n"u8 + "package build\n\n"u8 + "// shouldBuild checks tags given by lines of the form\n"u8 + "// +build tag\n"u8 + "//go:build tag\n"u8 + "func shouldBuild(content []byte)\n"u8,
        tags: new map<@string, bool>{},
        shouldBuild: true
    ),
    new(
        name: "TooClose",
        content: "// +build yes\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{},
        shouldBuild: true
    ),
    new(
        name: "TooClose2",
        content: "//go:build yes\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["yes"] = true},
        shouldBuild: true
    ),
    new(
        name: "TooCloseNo",
        content: "// +build no\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{},
        shouldBuild: true
    ),
    new(
        name: "TooCloseNo2",
        content: "//go:build no\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["no"] = true},
        shouldBuild: false
    ),
    new(
        name: "BinaryOnly",
        content: "//go:binary-only-package\n"u8 + "// +build yes\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{},
        binaryOnly: true,
        shouldBuild: true
    ),
    new(
        name: "BinaryOnly2",
        content: "//go:binary-only-package\n"u8 + "//go:build no\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["no"] = true},
        binaryOnly: true,
        shouldBuild: false
    ),
    new(
        name: "ValidGoBuild",
        content: "// +build yes\n\n"u8 + "//go:build no\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["no"] = true},
        shouldBuild: false
    ),
    new(
        name: "MissingBuild2",
        content: "/* */\n"u8 + "// +build yes\n\n"u8 + "//go:build no\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["no"] = true},
        shouldBuild: false
    ),
    new(
        name: "Comment1",
        content: "/*\n"u8 + "//go:build no\n"u8 + "*/\n\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{},
        shouldBuild: true
    ),
    new(
        name: "Comment2",
        content: "/*\n"u8 + "text\n"u8 + "*/\n\n"u8 + "//go:build no\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["no"] = true},
        shouldBuild: false
    ),
    new(
        name: "Comment3",
        content: "/*/*/ /* hi *//* \n"u8 + "text\n"u8 + "*/\n\n"u8 + "//go:build no\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["no"] = true},
        shouldBuild: false
    ),
    new(
        name: "Comment4",
        content: "/**///go:build no\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{},
        shouldBuild: true
    ),
    new(
        name: "Comment5",
        content: "/**/\n"u8 + "//go:build no\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["no"] = true},
        shouldBuild: false
    )
];

public static void TestShouldBuild(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(shouldBuildTests)) {
        ref var tt = ref heap(new shouldBuildTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            var ctx = Ꮡ(new Context(BuildTags: ["yes"]));
            var tags = new map<@string, bool>{};
            var (shouldBuild, binaryOnly, err) = ctx.shouldBuild(slice<byte>(ttʗ1.content), tags);
            if (shouldBuild != ttʗ1.shouldBuild || binaryOnly != ttʗ1.binaryOnly || !maps.Equal<map<@string, bool>, map<@string, bool>, @string, bool>(tags, ttʗ1.tags) || !AreEqual(err, ttʗ1.err)) {
                tΔ1.Errorf("mismatch:\n"u8 + "have shouldBuild=%v, binaryOnly=%v, tags=%v, err=%v\n"u8 + "want shouldBuild=%v, binaryOnly=%v, tags=%v, err=%v"u8,
                    shouldBuild, binaryOnly, tags, err,
                    ttʗ1.shouldBuild, ttʗ1.binaryOnly, ttʗ1.tags, ttʗ1.err);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloLinuxGoˢ => ref helloLinuxGoˢᵇ;
private static readonly @string helloLinuxGoˢᵇ = "hello_linux.go"u8;

public static void TestGoodOSArchFile(ж<testing.T> t) {
    var ctx = Ꮡ(new Context(BuildTags: ["linux"], GOOS: "darwin"));
    var m = new map<@string, bool>{};
    var want = new map<@string, bool>{["linux"] = true};
    if (!ctx.goodOSArchFile(helloLinuxGoˢ, m)) {
        t.Errorf("goodOSArchFile(hello_linux.go) = false, want true");
    }
    if (!maps.Equal<map<@string, bool>, map<@string, bool>, @string, bool>(m, want)) {
        t.Errorf("goodOSArchFile(hello_linux.go) tags = %v, want %v", m, want);
    }
}

internal partial struct readNopCloser {
    /*embed*/ public io_package.Reader Reader;
}

internal static error Close(this readNopCloser r) {
    return default;
}

internal static global::go.go.build_package.Context ctxtP9 = new Context(GOARCH: "arm", GOOS: "plan9");
internal static global::go.go.build_package.Context ctxtAndroid = new Context(GOARCH: "arm", GOOS: "android");

partial struct matchFileTestsᴛ1 /*dyn*/ {
    internal global::go.go.build_package.Context ctxt;
    internal @string name;
    internal @string data;
    internal bool match;
}
internal static slice<matchFileTestsᴛ1> matchFileTests = [
    new(ctxtP9, "foo_arm.go", "", true),
    new(ctxtP9, "foo1_arm.go", "// +build linux\n\npackage main\n", false),
    new(ctxtP9, "foo_darwin.go", "", false),
    new(ctxtP9, "foo.go", "", true),
    new(ctxtP9, "foo1.go", "// +build linux\n\npackage main\n", false),
    new(ctxtP9, "foo.badsuffix", "", false),
    new(ctxtAndroid, "foo_linux.go", "", true),
    new(ctxtAndroid, "foo_android.go", "", true),
    new(ctxtAndroid, "foo_plan9.go", "", false),
    new(ctxtAndroid, "android.go", "", true),
    new(ctxtAndroid, "plan9.go", "", true),
    new(ctxtAndroid, "plan9_test.go", "", true),
    new(ctxtAndroid, "arm.s", "", true),
    new(ctxtAndroid, "amd64.s", "", true)
];

public static void TestMatchFile(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(matchFileTests)) {
        ref var tt = ref heap(new matchFileTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        ref var ctxt = ref heap<global::go.go.build_package.Context>(tt.ctxt, out var Ꮡctxt);
        var ttʗ1 = tt;
        ctxt.OpenFile = (io.ReadCloser r, error err) (@string path) => {
            if (path != "x+"u8 + ttʗ1.name) {
                t.Fatalf("OpenFile asked for %q, expected %q", path, "x+" + ttʗ1.name);
            }
            return (new build_internal_test_package.readNopCloserжReadCloser(Ꮡ(new readNopCloser(new build_internal_test_package.strings_ReaderжReader(strings.NewReader(ttʗ1.data))))), default);
        };
        ctxt.JoinPath = @string (params ꓸꓸꓸstring elemʗp) => {
            var elem = elemʗp.slice();
            return strings.Join(elem, "+");
        };
        var (match, err) = Ꮡctxt.MatchFile("x", tt.name);
        if (match != tt.match || err != nil) {
            t.Fatalf("MatchFile(%q) = %v, %v, want %v, nil", tt.name, match, err, tt.match);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string cmdInternalObjfileˢ => ref cmdInternalObjfileˢᵇ;
private static readonly @string cmdInternalObjfileˢᵇ = "cmd/internal/objfile"u8;
internal static ref readonly @string srcCmdInternalObjfileˢ => ref srcCmdInternalObjfileˢᵇ;
private static readonly @string srcCmdInternalObjfileˢᵇ = "src/cmd/internal/objfile"u8;
internal static readonly object srcCmdInternalObjfileˢ2 = (@string)".../src/cmd/internal/objfile"u8;

public static void TestImportCmd(ж<testing.T> t) {
    if (runtime.GOOS == "ios"u8) {
        t.Skipf("skipping on %s/%s, no valid GOROOT", runtime.GOOS, runtime.GOARCH);
    }

    var (p, err) = Import(cmdInternalObjfileˢ, ""u8, 0);
    if (err != nil) {
        t.Fatal(err);
    }
    if (!strings.HasSuffix(filepath.ToSlash(p.Dir), srcCmdInternalObjfileˢ)) {
        t.Fatalf("Import cmd/internal/objfile returned Dir=%q, want %q", filepath.ToSlash(p.Dir), srcCmdInternalObjfileˢ2);
    }
}

internal static @string expandSrcDirPath = filepath.Join(((@string)(rune)filepath.Separator) + "projects"u8, "src", "add");

partial struct expandSrcDirTestsᴛ1 /*dyn*/ {
    internal @string input, expected;
}
internal static slice<expandSrcDirTestsᴛ1> expandSrcDirTests = [
    new("-L ${SRCDIR}/libs -ladd", "-L /projects/src/add/libs -ladd"),
    new("${SRCDIR}/add_linux_386.a -pthread -lstdc++", "/projects/src/add/add_linux_386.a -pthread -lstdc++"),
    new("Nothing to expand here!", "Nothing to expand here!"),
    new("$", "$"),
    new("$$", "$$"),
    new("${", "${"),
    new("$}", "$}"),
    new("$FOO ${BAR}", "$FOO ${BAR}"),
    new("Find me the $SRCDIRECTORY.", "Find me the $SRCDIRECTORY."),
    new("$SRCDIR is missing braces", "$SRCDIR is missing braces")
];

public static void TestExpandSrcDir(ж<testing.T> t) {
    foreach (var (_, test) in range(expandSrcDirTests)) {
        var (output, _) = expandSrcDir(test.input, expandSrcDirPath);
        if (output != test.expected) {
            t.Errorf("%q expands to %q with SRCDIR=%q when %q is expected", test.input, output, expandSrcDirPath, test.expected);
        } else {
            t.Logf("%q expands to %q with SRCDIR=%q", test.input, output, expandSrcDirPath);
        }
    }
}

internal partial struct TestShellSafety_tests /*dyn*/ {
    internal @string input, srcdir, expected;
    internal bool result;
}

public static void TestShellSafety(ж<testing.T> t) {
    var tests = slice<TestShellSafety_tests>([
        new("-I${SRCDIR}/../include", "/projects/src/issue 11868", "-I/projects/src/issue 11868/../include", true),
        new("-I${SRCDIR}", "~wtf$@%^", "-I~wtf$@%^", true),
        new("-X${SRCDIR}/1,${SRCDIR}/2", "/projects/src/issue 11868", "-X/projects/src/issue 11868/1,/projects/src/issue 11868/2", true),
        new("-I/tmp -I/tmp", "/tmp2", "-I/tmp -I/tmp", true),
        new("-I/tmp", "/tmp/[0]", "-I/tmp", true),
        new("-I${SRCDIR}/dir", "/tmp/[0]", "-I/tmp/[0]/dir", false),
        new("-I${SRCDIR}/dir", "/tmp/go go", "-I/tmp/go go/dir", true),
        new("-I${SRCDIR}/dir dir", "/tmp/go", "-I/tmp/go/dir dir", true)
    ]);
    foreach (var (_, test) in range(tests)) {
        var (output, ok) = expandSrcDir(test.input, test.srcdir);
        if (ok != test.result) {
            t.Errorf("Expected %t while %q expands to %q with SRCDIR=%q; got %t", test.result, test.input, output, test.srcdir, ok);
        }
        if (output != test.expected) {
            t.Errorf("Expected %q while %q expands with SRCDIR=%q; got %q", test.expected, test.input, test.srcdir, output);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string srcGoBuildˢ => ref srcGoBuildˢᵇ;
private static readonly @string srcGoBuildˢᵇ = "src/go/build"u8;
internal static ref readonly @string cannotFindPackageˢ => ref cannotFindPackageˢᵇ;
private static readonly @string cannotFindPackageˢᵇ = "cannot find package"u8;
internal static ref readonly @string cannotFindPackageErrorˢ => ref cannotFindPackageErrorˢᵇ;
private static readonly @string cannotFindPackageErrorˢᵇ = @"""cannot find package"" error"u8;
internal static ref readonly @string isNotInStdˢ => ref isNotInStdˢᵇ;
private static readonly @string isNotInStdˢᵇ = "is not in std"u8;
internal static ref readonly @string cannotFindPackageOrIsNotˢ => ref cannotFindPackageOrIsNotˢᵇ;
private static readonly @string cannotFindPackageOrIsNotˢᵇ = @"""cannot find package"" or ""is not in std"" error"u8;

internal partial struct TestImportDirNotExist_tests /*dyn*/ {
    internal @string label;
    internal @string path, srcDir;
    internal global::go.go.build_package.ImportMode mode;
}

// Want to get a "cannot find package" error when directory for package does not exist.
// There should be valid partial information in the returned non-nil *Package.
public static void TestImportDirNotExist(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveGoBuild(new build_internal_test_package.testing_TжTB(t)); // Need 'go list' internally.
        ref var ctxt = ref heap<global::go.go.build_package.Context>(Default, out var Ꮡctxt);

        @string emptyDir = t.TempDir();

        ctxt.GOPATH = emptyDir;
        ctxt.Dir = emptyDir;

        var tests = slice<TestImportDirNotExist_tests>([
            new("Import(full, 0)", "go/build/doesnotexist", "", 0),
            new("Import(local, 0)", "./doesnotexist", filepath.Join(ctxt.GOROOT, srcGoBuildˢ), 0),
            new("Import(full, FindOnly)", "go/build/doesnotexist", "", FindOnly),
            new("Import(local, FindOnly)", "./doesnotexist", filepath.Join(ctxt.GOROOT, srcGoBuildˢ), FindOnly)
        ]);

        defer(os.Setenv, go111moduleˢ, os.Getenv(go111moduleˢ), ref ᒐ);

        foreach (var (_, GO111MODULE) in range(slice<@string>(["off", "on"]))) {
            var testsʗ1 = tests;
            t.Run("GO111MODULE="u8 + GO111MODULE, (ж<testing.T> tΔ1) => {
                os.Setenv(go111moduleˢ, GO111MODULE);

                foreach (var (_, test) in range(testsʗ1)) {
                    var (p, err) = Ꮡctxt.Import(test.path, test.srcDir, test.mode);

                    var errOk = (err != nil && strings.HasPrefix(err.Error(), cannotFindPackageˢ));
                    @string wantErr = cannotFindPackageErrorˢ;
                    if (test.srcDir == ""u8) {
                        if (err != nil && strings.Contains(err.Error(), isNotInStdˢ)) {
                            errOk = true;
                        }
                        wantErr = cannotFindPackageOrIsNotˢ;
                    }
                    if (!errOk) {
                        tΔ1.Errorf("%s got error: %q, want %s", test.label, err, wantErr);
                    }
                    // If an error occurs, build.Import is documented to return
                    // a non-nil *Package containing partial information.
                    if (p == nil) {
                        tΔ1.Fatalf(@"%s got nil p, want non-nil *Package", test.label);
                    }
                    // Verify partial information in p.
                    if (p.ImportPath != "go/build/doesnotexist"u8) {
                        tΔ1.Errorf(@"%s got p.ImportPath: %q, want ""go/build/doesnotexist""", test.label, p.ImportPath);
                    }
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string offˢ => ref offˢᵇ;
private static readonly @string offˢᵇ = "off"u8;
internal static ref readonly @string testdataWithvendorˢ => ref testdataWithvendorˢᵇ;
private static readonly @string testdataWithvendorˢᵇ = "testdata/withvendor"u8;
internal static ref readonly @string srcABˢ => ref srcABˢᵇ;
private static readonly @string srcABˢᵇ = "src/a/b"u8;
internal static ref readonly @string aVendorCDˢ => ref aVendorCDˢᵇ;
private static readonly @string aVendorCDˢᵇ = "a/vendor/c/d"u8;

public static void TestImportVendor(ж<testing.T> t) {
    testenv.MustHaveSource(new build_internal_test_package.testing_TжTB(t));

    t.Setenv(go111moduleˢ, offˢ);

    ref var ctxt = ref heap<global::go.go.build_package.Context>(Default, out var Ꮡctxt);
    var (wd, err) = os.Getwd();
    if (err != nil) {
        t.Fatal(err);
    }
    ctxt.GOPATH = filepath.Join(wd, testdataWithvendorˢ);
    (var p, err) = Ꮡctxt.Import("c/d", filepath.Join(ctxt.GOPATH, srcABˢ), 0);
    if (err != nil) {
        t.Fatalf("cannot find vendored c/d from testdata src/a/b directory: %v", err);
    }
    @string want = aVendorCDˢ;
    if (p.ImportPath != want) {
        t.Fatalf("Import succeeded but found %q, want %q", p.ImportPath, want);
    }
}

public static void BenchmarkImportVendor(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    testenv.MustHaveSource(new build_internal_test_package.testing_BжTB(Ꮡb));

    Ꮡb.Setenv(go111moduleˢ, offˢ);

    ref var ctxt = ref heap<global::go.go.build_package.Context>(Default, out var Ꮡctxt);
    var (wd, err) = os.Getwd();
    if (err != nil) {
        Ꮡb.Fatal(err);
    }
    ctxt.GOPATH = filepath.Join(wd, testdataWithvendorˢ);
    @string dir = filepath.Join(ctxt.GOPATH, srcABˢ);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        var (_, errΔ1) = Ꮡctxt.Import("c/d", dir, 0);
        if (errΔ1 != nil) {
            Ꮡb.Fatalf("cannot find vendored c/d from testdata src/a/b directory: %v", errΔ1);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string xComYZˢ => ref xComYZˢᵇ;
private static readonly @string xComYZˢᵇ = "x.com/y/z"u8;
internal static ref readonly @string vendorTreeˢ => ref vendorTreeˢᵇ;
private static readonly @string vendorTreeˢᵇ = " (vendor tree)"u8;

public static void TestImportVendorFailure(ж<testing.T> t) {
    testenv.MustHaveSource(new build_internal_test_package.testing_TжTB(t));

    t.Setenv(go111moduleˢ, offˢ);

    ref var ctxt = ref heap<global::go.go.build_package.Context>(Default, out var Ꮡctxt);
    var (wd, err) = os.Getwd();
    if (err != nil) {
        t.Fatal(err);
    }
    ctxt.GOPATH = filepath.Join(wd, testdataWithvendorˢ);
    (var p, err) = Ꮡctxt.Import(xComYZˢ, filepath.Join(ctxt.GOPATH, srcABˢ), 0);
    if (err == nil) {
        t.Fatalf("found made-up package x.com/y/z in %s", p.Dir);
    }

    @string e = err.Error();
    if (!strings.Contains(e, vendorTreeˢ)) {
        t.Fatalf("error on failed import does not mention GOROOT/src/vendor directory:\n%s", e);
    }
}

public static void TestImportVendorParentFailure(ж<testing.T> t) {
    testenv.MustHaveSource(new build_internal_test_package.testing_TжTB(t));

    t.Setenv(go111moduleˢ, offˢ);

    ref var ctxt = ref heap<global::go.go.build_package.Context>(Default, out var Ꮡctxt);
    var (wd, err) = os.Getwd();
    if (err != nil) {
        t.Fatal(err);
    }
    ctxt.GOPATH = filepath.Join(wd, testdataWithvendorˢ);
    // This import should fail because the vendor/c directory has no source code.
    (var p, err) = Ꮡctxt.Import("c", filepath.Join(ctxt.GOPATH, srcABˢ), 0);
    if (err == nil) {
        t.Fatalf("found empty parent in %s", p.Dir);
    }
    if (p != nil && p.Dir != ""u8) {
        t.Fatalf("decided to use %s", p.Dir);
    }
    @string e = err.Error();
    if (!strings.Contains(e, vendorTreeˢ)) {
        t.Fatalf("error on failed import does not mention GOROOT/src/vendor directory:\n%s", e);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goproxyˢ => ref goproxyˢᵇ;
private static readonly @string goproxyˢᵇ = "GOPROXY"u8;
internal static ref readonly @string srcExampleComPˢ => ref srcExampleComPˢᵇ;
private static readonly @string srcExampleComPˢᵇ = "src/example.com/p"u8;
internal static ref readonly @string srcExampleComPPGoˢ => ref srcExampleComPPGoˢᵇ;
private static readonly @string srcExampleComPPGoˢᵇ = "src/example.com/p/p.go"u8;
internal static ref readonly @string goModFileNotFoundInˢ => ref goModFileNotFoundInˢᵇ;
private static readonly @string goModFileNotFoundInˢᵇ = "go.mod file not found in current directory or any parent directory"u8;
internal static ref readonly @string exampleComPˢ => ref exampleComPˢᵇ;
private static readonly @string exampleComPˢᵇ = "example.com/p"u8;
internal static readonly object importingPackageWhenNoGoˢ = (@string)"importing package when no go.mod is present succeeded unexpectedly"u8;

// Check that a package is loaded in module mode if GO111MODULE=on, even when
// no go.mod file is present. It should fail to resolve packages outside std.
// Verifies golang.org/issue/34669.
public static void TestImportPackageOutsideModule(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new build_internal_test_package.testing_TжTB(t));

    // Disable module fetching for this test so that 'go list' fails quickly
    // without trying to find the latest version of a module.
    t.Setenv(goproxyˢ, offˢ);

    // Create a GOPATH in a temporary directory. We don't use testdata
    // because it's in GOROOT, which interferes with the module heuristic.
    @string gopath = t.TempDir();
    {
        var err = os.MkdirAll(filepath.Join(gopath, srcExampleComPˢ), 511); if (err != nil) {
            t.Fatal(err);
        }
    }
    {
        var err = os.WriteFile(filepath.Join(gopath, srcExampleComPPGoˢ), slice<byte>("package p"u8), 438); if (err != nil) {
            t.Fatal(err);
        }
    }

    t.Setenv(go111moduleˢ, "on");
    t.Setenv(gopathˢ, gopath);
    ref var ctxt = ref heap<global::go.go.build_package.Context>(Default, out var Ꮡctxt);
    ctxt.GOPATH = gopath;
    ctxt.Dir = filepath.Join(gopath, srcExampleComPˢ);

    @string want = goModFileNotFoundInˢ;
    {
        var (_, err) = Ꮡctxt.Import(exampleComPˢ, gopath, FindOnly); if (err == nil) {
            t.Fatal(importingPackageWhenNoGoˢ);
        } else {
            @string errStr = err.Error(); if (!strings.Contains(errStr, want)) {
                t.Fatalf("error when importing package when no go.mod is present: got %q; want %q", errStr, want);
            } else {
                t.Logf(@"ctxt.Import(""example.com/p"", _, FindOnly): %v", err);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataDocˢ => ref testdataDocˢᵇ;
private static readonly @string testdataDocˢᵇ = "testdata/doc"u8;

// TestIssue23594 prevents go/build from regressing and populating Package.Doc
// from comments in test files.
public static void TestIssue23594(ж<testing.T> t) {
    // Package testdata/doc contains regular and external test files
    // with comments attached to their package declarations. The names of the files
    // ensure that we see the comments from the test files first.
    var (p, err) = ImportDir(testdataDocˢ, 0);
    if (err != nil) {
        t.Fatalf("could not import testdata: %v", err);
    }

    if (p.Doc != "Correct"u8) {
        t.Fatalf("incorrectly set .Doc to %q", p.Doc);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataBadsˢ => ref testdataBadsˢᵇ;
private static readonly @string testdataBadsˢᵇ = "testdata/bads"u8;

// TestIssue56509 tests that go/build does not add non-go files to InvalidGoFiles
// when they have unparsable comments.
public static void TestIssue56509(ж<testing.T> t) {
    // The directory testdata/bads contains a .s file that has an unparsable
    // comment. (go/build parses initial comments in non-go files looking for
    // //go:build or //+go build comments).
    var (p, err) = ImportDir(testdataBadsˢ, 0);
    if (err == nil) {
        t.Fatalf("could not import testdata/bads: %v", err);
    }

    if (len(p.InvalidGoFiles) != 0) {
        t.Fatalf("incorrectly added non-go file to InvalidGoFiles");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gonoproxyˢ => ref gonoproxyˢᵇ;
private static readonly @string gonoproxyˢᵇ = "GONOPROXY"u8;
internal static ref readonly @string noneˢ => ref noneˢᵇ;
private static readonly @string noneˢᵇ = "none"u8;
internal static ref readonly @string exampleComHelloˢ => ref exampleComHelloˢᵇ;
private static readonly @string exampleComHelloˢᵇ = "example.com/hello"u8;
internal static readonly object unexpectedSuccessˢ = (@string)"unexpected success"u8;
internal static ref readonly @string goGet1ˢ => ref goGet1ˢᵇ;
private static readonly @string goGet1ˢᵇ = "://...?go-get=1"u8;

// TestMissingImportErrorRepetition checks that when an unknown package is
// imported, the package path is only shown once in the error.
// Verifies golang.org/issue/34752.
public static void TestMissingImportErrorRepetition(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new build_internal_test_package.testing_TжTB(t)); // need 'go list' internally
    @string tmp = t.TempDir();
    var errΔ1 = os.WriteFile(filepath.Join(tmp, goModˢ), slice<byte>("module m"u8), 438); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }
    t.Setenv(go111moduleˢ, "on");
    t.Setenv(goproxyˢ, offˢ);
    t.Setenv(gonoproxyˢ, noneˢ);

    ref var ctxt = ref heap<global::go.go.build_package.Context>(Default, out var Ꮡctxt);
    ctxt.Dir = tmp;

    @string pkgPath = exampleComHelloˢ;
    var (_, err) = Ꮡctxt.Import(pkgPath, tmp, FindOnly);
    if (err == nil) {
        t.Fatal(unexpectedSuccessˢ);
    }

    // Don't count the package path with a URL like https://...?go-get=1.
    // See golang.org/issue/35986.
    @string errStr = strings.ReplaceAll(err.Error(), "://"u8 + pkgPath + "?go-get=1"u8, goGet1ˢ);

    // Also don't count instances in suggested "go get" or similar commands
    // (see https://golang.org/issue/41576). The suggested command typically
    // follows a semicolon.
    (errStr, _, _) = strings.Cut(errStr, ";");

    nint n = strings.Count(errStr, pkgPath); if (n != 1) {
        t.Fatalf("package path %q appears in error %d times; should appear once\nerror: %v", pkgPath, n, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataCgoDisabledˢ => ref testdataCgoDisabledˢᵇ;
private static readonly @string testdataCgoDisabledˢᵇ = "testdata/cgo_disabled"u8;

// TestCgoImportsIgnored checks that imports in cgo files are not included
// in the imports list when cgo is disabled.
// Verifies golang.org/issue/35946.
public static void TestCgoImportsIgnored(ж<testing.T> t) {
    ref var ctxt = ref heap<global::go.go.build_package.Context>(Default, out var Ꮡctxt);
    ctxt.CgoEnabled = false;
    var (p, err) = Ꮡctxt.ImportDir(testdataCgoDisabledˢ, 0);
    if (err != nil) {
        t.Fatal(err);
    }
    foreach (var (_, path) in range(p.Imports)) {
        if (path == "should/be/ignored"u8) {
            t.Errorf("found import %q in ignored cgo file", path);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string armˢ => ref armˢᵇ;
private static readonly @string armˢᵇ = "arm"u8;
internal static ref readonly @string netbsdˢ => ref netbsdˢᵇ;
private static readonly @string netbsdˢᵇ = "netbsd"u8;
internal static ref readonly @string testdataAlltagsˢ => ref testdataAlltagsˢᵇ;
private static readonly @string testdataAlltagsˢᵇ = "testdata/alltags"u8;
internal static ref readonly @string amd64ˢ => ref amd64ˢᵇ;
private static readonly @string amd64ˢᵇ = "amd64"u8;
internal static ref readonly @string linuxˢ => ref linuxˢᵇ;
private static readonly @string linuxˢᵇ = "linux"u8;

// Issue #52053. Check that if there is a file x_GOOS_GOARCH.go that both
// GOOS and GOARCH show up in the Package.AllTags field. We test both the
// case where the file matches and where the file does not match.
// The latter case used to fail, incorrectly omitting GOOS.
public static void TestAllTags(ж<testing.T> t) {
    ref var ctxt = ref heap<global::go.go.build_package.Context>(Default, out var Ꮡctxt);
    ctxt.GOARCH = armˢ;
    ctxt.GOOS = netbsdˢ;
    var (p, err) = Ꮡctxt.ImportDir(testdataAlltagsˢ, 0);
    if (err != nil) {
        t.Fatal(err);
    }
    var want = slice<@string>(["arm", "netbsd"]);
    if (!slices.Equal<slice<@string>, @string>(p.AllTags, want)) {
        t.Errorf("AllTags = %v, want %v", p.AllTags, want);
    }
    var wantFiles = slice<@string>(["alltags.go", "x_netbsd_arm.go"]);
    if (!slices.Equal<slice<@string>, @string>(p.GoFiles, wantFiles)) {
        t.Errorf("GoFiles = %v, want %v", p.GoFiles, wantFiles);
    }

    ctxt.GOARCH = amd64ˢ;
    ctxt.GOOS = linuxˢ;
    (p, err) = Ꮡctxt.ImportDir(testdataAlltagsˢ, 0);
    if (err != nil) {
        t.Fatal(err);
    }
    if (!slices.Equal<slice<@string>, @string>(p.AllTags, want)) {
        t.Errorf("AllTags = %v, want %v", p.AllTags, want);
    }
    wantFiles = ["alltags.go"];
    if (!slices.Equal<slice<@string>, @string>(p.GoFiles, wantFiles)) {
        t.Errorf("GoFiles = %v, want %v", p.GoFiles, wantFiles);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataNonSourceTagsˢ => ref testdataNonSourceTagsˢᵇ;
private static readonly @string testdataNonSourceTagsˢᵇ = "testdata/non_source_tags"u8;

public static void TestAllTagsNonSourceFile(ж<testing.T> t) {
    var (p, err) = ᏑDefault.ImportDir(testdataNonSourceTagsˢ, 0);
    if (err != nil) {
        t.Fatal(err);
    }
    if (len(p.AllTags) > 0) {
        t.Errorf("AllTags = %v, want empty", p.AllTags);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataDirectivesˢ => ref testdataDirectivesˢᵇ;
private static readonly @string testdataDirectivesˢᵇ = "testdata/directives"u8;
internal static ref readonly @string testdataDirectivesˢ2 => ref testdataDirectivesˢ2ᵇ;
private static readonly @string testdataDirectivesˢ2ᵇ = "testdata/directives/"u8;
internal static ref readonly @string testdataDirectivesˢ3 => ref testdataDirectivesˢ3ᵇ;
private static readonly @string testdataDirectivesˢ3ᵇ = @"testdata\\directives\\"u8;
internal static ref readonly @string directivesˢ => ref directivesˢᵇ;
private static readonly @string directivesˢᵇ = "Directives"u8;
internal static ref readonly @string goMain1Testdataˢ => ref goMain1Testdataˢᵇ;
private static readonly @string goMain1Testdataˢᵇ = @"[{""//go:main1"" ""testdata/directives/a.go:1:1""} {""//go:plant"" ""testdata/directives/eve.go:1:1""}]"u8;
internal static ref readonly @string testDirectivesˢ => ref testDirectivesˢᵇ;
private static readonly @string testDirectivesˢᵇ = "TestDirectives"u8;
internal static ref readonly @string goTest1Testdataˢ => ref goTest1Testdataˢᵇ;
private static readonly @string goTest1Testdataˢᵇ = @"[{""//go:test1"" ""testdata/directives/a_test.go:1:1""} {""//go:test2"" ""testdata/directives/b_test.go:1:1""}]"u8;
internal static ref readonly @string xTestDirectivesˢ => ref xTestDirectivesˢᵇ;
private static readonly @string xTestDirectivesˢᵇ = "XTestDirectives"u8;
internal static ref readonly @string goXtest1Testdataˢ => ref goXtest1Testdataˢᵇ;
private static readonly @string goXtest1Testdataˢᵇ = @"[{""//go:xtest1"" ""testdata/directives/c_test.go:1:1""} {""//go:xtest2"" ""testdata/directives/d_test.go:1:1""} {""//go:xtest3"" ""testdata/directives/d_test.go:2:1""}]"u8;

public static partial void TestDirectives(ж<testing.T> t) {
    var (p, err) = ImportDir(testdataDirectivesˢ, 0);
    if (err != nil) {
        t.Fatalf("could not import testdata: %v", err);
    }

    void check(@string name, slice<global::go.go.build_package.Directive> list, @string want) {
        if (runtime.GOOS == "windows"u8) {
            want = strings.ReplaceAll(want, testdataDirectivesˢ2, testdataDirectivesˢ3);
        }
        t.Helper();
        @string s = fmt.Sprintf("%q"u8, list);
        if (s != want) {
            t.Errorf("%s = %s, want %s", name, s, want);
        }
    }
    check(directivesˢ, p.Directives,
        goMain1Testdataˢ);
    check(testDirectivesˢ, p.TestDirectives,
        goTest1Testdataˢ);
    check(xTestDirectivesˢ, p.XTestDirectives,
        goXtest1Testdataˢ);
}

} // end build_internal_test_package
