// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using testenv = @internal.testenv_package;
using io = io_package;
using maps = maps_package;
using os = os_package;
using filepath = global::go.path.filepath_package;
using reflect = reflect_package;
using runtime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using fs = global::go.io.fs_package;
using global::go.path;
using static global::go.go.build_package;
using ꓸꓸꓸstring = Span<@string>;

partial class build_internal_test_package {

public static void TestMain(ж<testing.M> Ꮡm) {
    Default.GOROOT = testenv.GOROOT(default!);
    os.Exit(Ꮡm.Run());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string defaultˢ = "default"u8;
internal static readonly @string modifiedˢ = "modified"u8;

public static void TestMatch(ж<testing.T> Ꮡt) {
    ref var ctxt = ref heap<global::go.go.build_package.Context>(out var Ꮡctxt);
    ctxt = Default;
    @string what = defaultˢ;
    void match(@string tag, map<@string, bool> want) {
        Ꮡt.Helper();
        var m = new map<@string, bool>();
        if (!Ꮡctxt.matchAuto(tag, m)) {
            Ꮡt.Errorf("%s context should match %s, does not"u8, what, tag);
        }
        if (!maps.Equal<map<@string, bool>, map<@string, bool>, @string, bool>(m, want)) {
            Ꮡt.Errorf("%s tags = %v, want %v"u8, tag, m, want);
        }
    }
    void nomatch(@string tag, map<@string, bool> want) {
        Ꮡt.Helper();
        var m = new map<@string, bool>();
        if (Ꮡctxt.matchAuto(tag, m)) {
            Ꮡt.Errorf("%s context should NOT match %s, does"u8, what, tag);
        }
        if (!maps.Equal<map<@string, bool>, map<@string, bool>, @string, bool>(m, want)) {
            Ꮡt.Errorf("%s tags = %v, want %v"u8, tag, m, want);
        }
    }
    match(runtime.GOOS + ","u8 + runtime.GOARCH, new map<@string, bool>{[runtime.GOOS] = true, [runtime.GOARCH] = true});
    match(runtime.GOOS + ","u8 + runtime.GOARCH + ",!foo"u8, new map<@string, bool>{[runtime.GOOS] = true, [runtime.GOARCH] = true, ["foo"u8] = true});
    nomatch(runtime.GOOS + ","u8 + runtime.GOARCH + ",foo"u8, new map<@string, bool>{[runtime.GOOS] = true, [runtime.GOARCH] = true, ["foo"u8] = true});
    what = modifiedˢ;
    ctxt.BuildTags = new @string[]{"foo"u8}.slice();
    match(runtime.GOOS + ","u8 + runtime.GOARCH, new map<@string, bool>{[runtime.GOOS] = true, [runtime.GOARCH] = true});
    match(runtime.GOOS + ","u8 + runtime.GOARCH + ",foo"u8, new map<@string, bool>{[runtime.GOOS] = true, [runtime.GOARCH] = true, ["foo"u8] = true});
    nomatch(runtime.GOOS + ","u8 + runtime.GOARCH + ",!foo"u8, new map<@string, bool>{[runtime.GOOS] = true, [runtime.GOARCH] = true, ["foo"u8] = true});
    match(runtime.GOOS + ","u8 + runtime.GOARCH + ",!bar"u8, new map<@string, bool>{[runtime.GOOS] = true, [runtime.GOARCH] = true, ["bar"u8] = true});
    nomatch(runtime.GOOS + ","u8 + runtime.GOARCH + ",bar"u8, new map<@string, bool>{[runtime.GOOS] = true, [runtime.GOARCH] = true, ["bar"u8] = true});
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataOtherˢ = "testdata/other"u8;
internal static readonly @string fileˢ = "./file"u8;
internal static readonly object fileˢ2 = (@string)"file"u8;
internal static readonly @string testdataOtherFileˢ = "testdata/other/file"u8;

public static void TestDotSlashImport(ж<testing.T> Ꮡt) {
    var (p, err) = ImportDir(testdataOtherˢ, 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (len((~p).Imports) != 1 || (~p).Imports[0] != "./file") {
        Ꮡt.Fatalf("testdata/other: Imports=%v, want [./file]"u8, (~p).Imports);
    }
    (var p1, err) = Import(fileˢ, testdataOtherˢ, 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if ((~p1).Name != "file"u8) {
        Ꮡt.Fatalf("./file: Name=%q, want %q"u8, (~p1).Name, fileˢ2);
    }
    @string dir = filepath.Clean(testdataOtherFileˢ); // Clean to use \ on Windows
    if ((~p1).Dir != dir) {
        Ꮡt.Fatalf("./file: Dir=%q, want %q"u8, (~p1).Name, dir);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object importReturnedNilErrorˢ = (@string)@"Import("""") returned nil error."u8;
internal static readonly object importReturnedNilPackageˢ = (@string)@"Import("""") returned nil package."u8;

public static void TestEmptyImport(ж<testing.T> Ꮡt) {
    var (p, err) = Import(""u8, testenv.GOROOT(new build_internal_test_package.testing_TжTB(Ꮡt)), FindOnly);
    if (err == default!) {
        Ꮡt.Fatal(importReturnedNilErrorˢ);
    }
    if (p == nil) {
        Ꮡt.Fatal(importReturnedNilPackageˢ);
    }
    if ((~p).ImportPath != ""u8) {
        Ꮡt.Fatalf("ImportPath=%q, want %q."u8, (~p).ImportPath, (@string)""u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataEmptyˢ = "testdata/empty"u8;
internal static readonly object importTestdataEmptyDidˢ = (@string)@"Import(""testdata/empty"") did not return NoGoError."u8;

public static void TestEmptyFolderImport(ж<testing.T> Ꮡt) {
    var (_, err) = Import("."u8, testdataEmptyˢ, 0);
    {
        var (_, ok) = err._<ж<global::go.go.build_package.NoGoError>>(ᐧ); if (!ok) {
            Ꮡt.Fatal(importTestdataEmptyDidˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataMultiˢ = "testdata/multi"u8;
internal static readonly object importTestdataMultiDidˢ = (@string)@"Import(""testdata/multi"") did not return MultiplePackageError."u8;
internal static readonly @string mainˢ = "main"u8;

public static void TestMultiplePackageImport(ж<testing.T> Ꮡt) {
    var (pkg, err) = Import("."u8, testdataMultiˢ, 0);
    var (mpe, ok) = err._<ж<global::go.go.build_package.MultiplePackageError>>(ᐧ);
    if (!ok) {
        Ꮡt.Fatal(importTestdataMultiDidˢ);
    }
    var want = Ꮡ(new MultiplePackageError(
        Dir: filepath.FromSlash(testdataMultiˢ),
        Packages: new @string[]{"main"u8, "test_package"u8}.slice(),
        Files: new @string[]{"file.go"u8, "file_appengine.go"u8}.slice()
    ));
    if (!reflect.DeepEqual(mpe.OrTypedNil(), want.OrTypedNil())) {
        Ꮡt.Errorf("err = %#v; want %#v"u8, mpe.OrTypedNil(), want.OrTypedNil());
    }
    // TODO(#45999): Since the name is ambiguous, pkg.Name should be left empty.
    {
        @string wantName = mainˢ; if ((~pkg).Name != wantName) {
            Ꮡt.Errorf("pkg.Name = %q; want %q"u8, (~pkg).Name, wantName);
        }
    }
    {
        var wantGoFiles = new @string[]{"file.go"u8, "file_appengine.go"u8}.slice(); if (!slices.Equal<slice<@string>, @string>((~pkg).GoFiles, wantGoFiles)) {
            Ꮡt.Errorf("pkg.GoFiles = %q; want %q"u8, (~pkg).GoFiles, wantGoFiles);
        }
    }
    {
        var wantInvalidFiles = new @string[]{"file_appengine.go"u8}.slice(); if (!slices.Equal<slice<@string>, @string>((~pkg).InvalidGoFiles, wantInvalidFiles)) {
            Ꮡt.Errorf("pkg.InvalidGoFiles = %q; want %q"u8, (~pkg).InvalidGoFiles, wantInvalidFiles);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goBuildˢ = "go/build"u8;

public static void TestLocalDirectory(ж<testing.T> Ꮡt) {
    if (runtime.GOOS == "ios"u8) {
        Ꮡt.Skipf("skipping on %s/%s, no valid GOROOT"u8, runtime.GOOS, runtime.GOARCH);
    }
    var (cwd, err) = os.Getwd();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var p, err) = ImportDir(cwd, 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if ((~p).ImportPath != "go/build"u8) {
        Ꮡt.Fatalf("ImportPath=%q, want %q"u8, (~p).ImportPath, goBuildˢ);
    }
}


[GoType("dyn")] partial struct shouldBuildTestsᴛ1 {
    internal @string name;
    internal @string content;
    internal map<@string, bool> tags;
    internal bool binaryOnly;
    internal bool shouldBuild;
    internal error err;
}
internal static slice<shouldBuildTestsᴛ1> shouldBuildTests = new shouldBuildTestsᴛ1[]{
    new(
        name: "Yes"u8,
        content: "// +build yes\n\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["yes"u8] = true},
        shouldBuild: true
    ),
    new(
        name: "Yes2"u8,
        content: "//go:build yes\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["yes"u8] = true},
        shouldBuild: true
    ),
    new(
        name: "Or"u8,
        content: "// +build no yes\n\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["yes"u8] = true, ["no"u8] = true},
        shouldBuild: true
    ),
    new(
        name: "Or2"u8,
        content: "//go:build no || yes\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["yes"u8] = true, ["no"u8] = true},
        shouldBuild: true
    ),
    new(
        name: "And"u8,
        content: "// +build no,yes\n\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["yes"u8] = true, ["no"u8] = true},
        shouldBuild: false
    ),
    new(
        name: "And2"u8,
        content: "//go:build no && yes\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["yes"u8] = true, ["no"u8] = true},
        shouldBuild: false
    ),
    new(
        name: "Cgo"u8,
        content: "// +build cgo\n\n"u8 + "// Copyright The Go Authors.\n\n"u8 + "// This package implements parsing of tags like\n"u8 + "// +build tag1\n"u8 + "package build"u8,
        tags: new map<@string, bool>{["cgo"u8] = true},
        shouldBuild: false
    ),
    new(
        name: "Cgo2"u8,
        content: "//go:build cgo\n"u8 + "// Copyright The Go Authors.\n\n"u8 + "// This package implements parsing of tags like\n"u8 + "// +build tag1\n"u8 + "package build"u8,
        tags: new map<@string, bool>{["cgo"u8] = true},
        shouldBuild: false
    ),
    new(
        name: "AfterPackage"u8,
        content: "// Copyright The Go Authors.\n\n"u8 + "package build\n\n"u8 + "// shouldBuild checks tags given by lines of the form\n"u8 + "// +build tag\n"u8 + "//go:build tag\n"u8 + "func shouldBuild(content []byte)\n"u8,
        tags: new map<@string, bool>{},
        shouldBuild: true
    ),
    new(
        name: "TooClose"u8,
        content: "// +build yes\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{},
        shouldBuild: true
    ),
    new(
        name: "TooClose2"u8,
        content: "//go:build yes\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["yes"u8] = true},
        shouldBuild: true
    ),
    new(
        name: "TooCloseNo"u8,
        content: "// +build no\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{},
        shouldBuild: true
    ),
    new(
        name: "TooCloseNo2"u8,
        content: "//go:build no\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["no"u8] = true},
        shouldBuild: false
    ),
    new(
        name: "BinaryOnly"u8,
        content: "//go:binary-only-package\n"u8 + "// +build yes\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{},
        binaryOnly: true,
        shouldBuild: true
    ),
    new(
        name: "BinaryOnly2"u8,
        content: "//go:binary-only-package\n"u8 + "//go:build no\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["no"u8] = true},
        binaryOnly: true,
        shouldBuild: false
    ),
    new(
        name: "ValidGoBuild"u8,
        content: "// +build yes\n\n"u8 + "//go:build no\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["no"u8] = true},
        shouldBuild: false
    ),
    new(
        name: "MissingBuild2"u8,
        content: "/* */\n"u8 + "// +build yes\n\n"u8 + "//go:build no\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["no"u8] = true},
        shouldBuild: false
    ),
    new(
        name: "Comment1"u8,
        content: "/*\n"u8 + "//go:build no\n"u8 + "*/\n\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{},
        shouldBuild: true
    ),
    new(
        name: "Comment2"u8,
        content: "/*\n"u8 + "text\n"u8 + "*/\n\n"u8 + "//go:build no\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["no"u8] = true},
        shouldBuild: false
    ),
    new(
        name: "Comment3"u8,
        content: "/*/*/ /* hi *//* \n"u8 + "text\n"u8 + "*/\n\n"u8 + "//go:build no\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["no"u8] = true},
        shouldBuild: false
    ),
    new(
        name: "Comment4"u8,
        content: "/**///go:build no\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{},
        shouldBuild: true
    ),
    new(
        name: "Comment5"u8,
        content: "/**/\n"u8 + "//go:build no\n"u8 + "package main\n"u8,
        tags: new map<@string, bool>{["no"u8] = true},
        shouldBuild: false
    )
}.slice();

public static void TestShouldBuild(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in shouldBuildTests) {
        ref var tt = ref heap(new shouldBuildTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            var ctx = Ꮡ(new Context(BuildTags: new @string[]{"yes"u8}.slice()));
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
internal static readonly @string helloLinuxGoˢ = "hello_linux.go"u8;

public static void TestGoodOSArchFile(ж<testing.T> Ꮡt) {
    var ctx = Ꮡ(new Context(BuildTags: new @string[]{"linux"u8}.slice(), GOOS: "darwin"u8));
    var m = new map<@string, bool>{};
    var want = new map<@string, bool>{["linux"u8] = true};
    if (!ctx.goodOSArchFile(helloLinuxGoˢ, m)) {
        Ꮡt.Errorf("goodOSArchFile(hello_linux.go) = false, want true"u8);
    }
    if (!maps.Equal<map<@string, bool>, map<@string, bool>, @string, bool>(m, want)) {
        Ꮡt.Errorf("goodOSArchFile(hello_linux.go) tags = %v, want %v"u8, m, want);
    }
}

[GoType] internal partial struct readNopCloser {
    [GoEmbedded] public io_package.Reader Reader;
}

internal static error Close(this readNopCloser r) {
    return default!;
}

internal static global::go.go.build_package.Context ctxtP9 = new Context(GOARCH: "arm"u8, GOOS: "plan9"u8);
internal static global::go.go.build_package.Context ctxtAndroid = new Context(GOARCH: "arm"u8, GOOS: "android"u8);


[GoType("dyn")] partial struct matchFileTestsᴛ1 {
    internal global::go.go.build_package.Context ctxt;
    internal @string name;
    internal @string data;
    internal bool match;
}
internal static slice<matchFileTestsᴛ1> matchFileTests = new matchFileTestsᴛ1[]{
    new(ctxtP9, "foo_arm.go"u8, ""u8, true),
    new(ctxtP9, "foo1_arm.go"u8, "// +build linux\n\npackage main\n"u8, false),
    new(ctxtP9, "foo_darwin.go"u8, ""u8, false),
    new(ctxtP9, "foo.go"u8, ""u8, true),
    new(ctxtP9, "foo1.go"u8, "// +build linux\n\npackage main\n"u8, false),
    new(ctxtP9, "foo.badsuffix"u8, ""u8, false),
    new(ctxtAndroid, "foo_linux.go"u8, ""u8, true),
    new(ctxtAndroid, "foo_android.go"u8, ""u8, true),
    new(ctxtAndroid, "foo_plan9.go"u8, ""u8, false),
    new(ctxtAndroid, "android.go"u8, ""u8, true),
    new(ctxtAndroid, "plan9.go"u8, ""u8, true),
    new(ctxtAndroid, "plan9_test.go"u8, ""u8, true),
    new(ctxtAndroid, "arm.s"u8, ""u8, true),
    new(ctxtAndroid, "amd64.s"u8, ""u8, true)
}.slice();

public static void TestMatchFile(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in matchFileTests) {
        ref var tt = ref heap(new matchFileTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        ref var ctxt = ref heap<global::go.go.build_package.Context>(out var Ꮡctxt);
        ctxt = tt.ctxt;
        var ttʗ1 = tt;
        ctxt.OpenFile = (io.ReadCloser r, error err) (@string path) => {
            if (path != "x+"u8 + ttʗ1.name) {
                Ꮡt.Fatalf("OpenFile asked for %q, expected %q"u8, path, "x+" + ttʗ1.name);
            }
            return (new build_internal_test_package.readNopCloserжReadCloser(Ꮡ(new readNopCloser(new build_internal_test_package.strings_ReaderжReader(strings.NewReader(ttʗ1.data))))), default!);
        };
        ctxt.JoinPath = @string (params ꓸꓸꓸstring elemʗp) => {
            var elem = elemʗp.slice();
            return strings.Join(elem, "+"u8);
        };
        var (match, err) = Ꮡctxt.MatchFile("x"u8, tt.name);
        if (match != tt.match || err != default!) {
            Ꮡt.Fatalf("MatchFile(%q) = %v, %v, want %v, nil"u8, tt.name, match, err, tt.match);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cmdInternalObjfileˢ = "cmd/internal/objfile"u8;
internal static readonly @string srcCmdInternalObjfileˢ = "src/cmd/internal/objfile"u8;
internal static readonly object srcCmdInternalObjfileˢ2 = (@string)".../src/cmd/internal/objfile"u8;

public static void TestImportCmd(ж<testing.T> Ꮡt) {
    if (runtime.GOOS == "ios"u8) {
        Ꮡt.Skipf("skipping on %s/%s, no valid GOROOT"u8, runtime.GOOS, runtime.GOARCH);
    }
    var (p, err) = Import(cmdInternalObjfileˢ, ""u8, 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!strings.HasSuffix(filepath.ToSlash((~p).Dir), srcCmdInternalObjfileˢ)) {
        Ꮡt.Fatalf("Import cmd/internal/objfile returned Dir=%q, want %q"u8, filepath.ToSlash((~p).Dir), srcCmdInternalObjfileˢ2);
    }
}

internal static @string expandSrcDirPath = filepath.Join(((@string)(rune)filepath.Separator) + "projects"u8, "src", "add");


[GoType("dyn")] partial struct expandSrcDirTestsᴛ1 {
    internal @string input, expected;
}
internal static slice<expandSrcDirTestsᴛ1> expandSrcDirTests = new expandSrcDirTestsᴛ1[]{
    new("-L ${SRCDIR}/libs -ladd"u8, "-L /projects/src/add/libs -ladd"u8),
    new("${SRCDIR}/add_linux_386.a -pthread -lstdc++"u8, "/projects/src/add/add_linux_386.a -pthread -lstdc++"u8),
    new("Nothing to expand here!"u8, "Nothing to expand here!"u8),
    new("$"u8, "$"u8),
    new("$$"u8, "$$"u8),
    new("${"u8, "${"u8),
    new("$}"u8, "$}"u8),
    new("$FOO ${BAR}"u8, "$FOO ${BAR}"u8),
    new("Find me the $SRCDIRECTORY."u8, "Find me the $SRCDIRECTORY."u8),
    new("$SRCDIR is missing braces"u8, "$SRCDIR is missing braces"u8)
}.slice();

public static void TestExpandSrcDir(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in expandSrcDirTests) {
        var (output, _) = expandSrcDir(test.input, expandSrcDirPath);
        if (output != test.expected){
            Ꮡt.Errorf("%q expands to %q with SRCDIR=%q when %q is expected"u8, test.input, output, expandSrcDirPath, test.expected);
        } else {
            Ꮡt.Logf("%q expands to %q with SRCDIR=%q"u8, test.input, output, expandSrcDirPath);
        }
    }
}

[GoType("dyn")] internal partial struct TestShellSafety_tests {
    internal @string input, srcdir, expected;
    internal bool result;
}

public static void TestShellSafety(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestShellSafety_tests[]{
        new("-I${SRCDIR}/../include"u8, "/projects/src/issue 11868"u8, "-I/projects/src/issue 11868/../include"u8, true),
        new("-I${SRCDIR}"u8, "~wtf$@%^"u8, "-I~wtf$@%^"u8, true),
        new("-X${SRCDIR}/1,${SRCDIR}/2"u8, "/projects/src/issue 11868"u8, "-X/projects/src/issue 11868/1,/projects/src/issue 11868/2"u8, true),
        new("-I/tmp -I/tmp"u8, "/tmp2"u8, "-I/tmp -I/tmp"u8, true),
        new("-I/tmp"u8, "/tmp/[0]"u8, "-I/tmp"u8, true),
        new("-I${SRCDIR}/dir"u8, "/tmp/[0]"u8, "-I/tmp/[0]/dir"u8, false),
        new("-I${SRCDIR}/dir"u8, "/tmp/go go"u8, "-I/tmp/go go/dir"u8, true),
        new("-I${SRCDIR}/dir dir"u8, "/tmp/go"u8, "-I/tmp/go/dir dir"u8, true)
    }.slice();
    foreach (var (_, test) in tests) {
        var (output, ok) = expandSrcDir(test.input, test.srcdir);
        if (ok != test.result) {
            Ꮡt.Errorf("Expected %t while %q expands to %q with SRCDIR=%q; got %t"u8, test.result, test.input, output, test.srcdir, ok);
        }
        if (output != test.expected) {
            Ꮡt.Errorf("Expected %q while %q expands with SRCDIR=%q; got %q"u8, test.expected, test.input, test.srcdir, output);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string srcGoBuildˢ = "src/go/build"u8;
internal static readonly @string cannotFindPackageˢ = "cannot find package"u8;
internal static readonly @string cannotFindPackageErrorˢ = @"""cannot find package"" error"u8;
internal static readonly @string isNotInStdˢ = "is not in std"u8;
internal static readonly @string cannotFindPackageOrIsNotˢ = @"""cannot find package"" or ""is not in std"" error"u8;

[GoType("dyn")] internal partial struct TestImportDirNotExist_tests {
    internal @string label;
    internal @string path, srcDir;
    internal global::go.go.build_package.ImportMode mode;
}

// Want to get a "cannot find package" error when directory for package does not exist.
// There should be valid partial information in the returned non-nil *Package.
public static void TestImportDirNotExist(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveGoBuild(new build_internal_test_package.testing_TжTB(Ꮡt)); // Need 'go list' internally.
        ref var ctxt = ref heap<global::go.go.build_package.Context>(out var Ꮡctxt);
        ctxt = Default;
        @string emptyDir = Ꮡt.TempDir();
        ctxt.GOPATH = emptyDir;
        ctxt.Dir = emptyDir;
        var tests = new TestImportDirNotExist_tests[]{
            new("Import(full, 0)"u8, "go/build/doesnotexist"u8, ""u8, 0),
            new("Import(local, 0)"u8, "./doesnotexist"u8, filepath.Join(ctxt.GOROOT, srcGoBuildˢ), 0),
            new("Import(full, FindOnly)"u8, "go/build/doesnotexist"u8, ""u8, FindOnly),
            new("Import(local, FindOnly)"u8, "./doesnotexist"u8, filepath.Join(ctxt.GOROOT, srcGoBuildˢ), FindOnly)
        }.slice();
        defer(os.Setenv, go111moduleˢ, os.Getenv(go111moduleˢ), ref ᒐ);
        foreach (var (_, GO111MODULE) in new @string[]{"off"u8, "on"u8}.slice()) {
            var testsʗ1 = tests;
            Ꮡt.Run("GO111MODULE="u8 + GO111MODULE, (ж<testing.T> tΔ1) => {
                os.Setenv(go111moduleˢ, GO111MODULE);
                foreach (var (_, test) in testsʗ1) {
                    var (p, err) = Ꮡctxt.Import(test.path, test.srcDir, test.mode);
                    var errOk = (err != default! && strings.HasPrefix(err.Error(), cannotFindPackageˢ));
                    @string wantErr = cannotFindPackageErrorˢ;
                    if (test.srcDir == ""u8) {
                        if (err != default! && strings.Contains(err.Error(), isNotInStdˢ)) {
                            errOk = true;
                        }
                        wantErr = cannotFindPackageOrIsNotˢ;
                    }
                    if (!errOk) {
                        tΔ1.Errorf("%s got error: %q, want %s"u8, test.label, err, wantErr);
                    }
                    // If an error occurs, build.Import is documented to return
                    // a non-nil *Package containing partial information.
                    if (p == nil) {
                        tΔ1.Fatalf(@"%s got nil p, want non-nil *Package"u8, test.label);
                    }
                    // Verify partial information in p.
                    if ((~p).ImportPath != "go/build/doesnotexist"u8) {
                        tΔ1.Errorf(@"%s got p.ImportPath: %q, want ""go/build/doesnotexist"""u8, test.label, (~p).ImportPath);
                    }
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string offˢ = "off"u8;
internal static readonly @string testdataWithvendorˢ = "testdata/withvendor"u8;
internal static readonly @string srcABˢ = "src/a/b"u8;
internal static readonly @string aVendorCDˢ = "a/vendor/c/d"u8;

public static void TestImportVendor(ж<testing.T> Ꮡt) {
    testenv.MustHaveSource(new build_internal_test_package.testing_TжTB(Ꮡt));
    Ꮡt.Setenv(go111moduleˢ, offˢ);
    ref var ctxt = ref heap<global::go.go.build_package.Context>(out var Ꮡctxt);
    ctxt = Default;
    var (wd, err) = os.Getwd();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ctxt.GOPATH = filepath.Join(wd, testdataWithvendorˢ);
    (var p, err) = Ꮡctxt.Import("c/d"u8, filepath.Join(ctxt.GOPATH, srcABˢ), 0);
    if (err != default!) {
        Ꮡt.Fatalf("cannot find vendored c/d from testdata src/a/b directory: %v"u8, err);
    }
    @string want = aVendorCDˢ;
    if ((~p).ImportPath != want) {
        Ꮡt.Fatalf("Import succeeded but found %q, want %q"u8, (~p).ImportPath, want);
    }
}

public static void BenchmarkImportVendor(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    testenv.MustHaveSource(new build_internal_test_package.testing_BжTB(Ꮡb));
    Ꮡb.Setenv(go111moduleˢ, offˢ);
    ref var ctxt = ref heap<global::go.go.build_package.Context>(out var Ꮡctxt);
    ctxt = Default;
    var (wd, err) = os.Getwd();
    if (err != default!) {
        Ꮡb.Fatal(err);
    }
    ctxt.GOPATH = filepath.Join(wd, testdataWithvendorˢ);
    @string dir = filepath.Join(ctxt.GOPATH, srcABˢ);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        var (_, errΔ1) = Ꮡctxt.Import("c/d"u8, dir, 0);
        if (errΔ1 != default!) {
            Ꮡb.Fatalf("cannot find vendored c/d from testdata src/a/b directory: %v"u8, errΔ1);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xComYZˢ = "x.com/y/z"u8;
internal static readonly @string vendorTreeˢ = " (vendor tree)"u8;

public static void TestImportVendorFailure(ж<testing.T> Ꮡt) {
    testenv.MustHaveSource(new build_internal_test_package.testing_TжTB(Ꮡt));
    Ꮡt.Setenv(go111moduleˢ, offˢ);
    ref var ctxt = ref heap<global::go.go.build_package.Context>(out var Ꮡctxt);
    ctxt = Default;
    var (wd, err) = os.Getwd();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ctxt.GOPATH = filepath.Join(wd, testdataWithvendorˢ);
    (var p, err) = Ꮡctxt.Import(xComYZˢ, filepath.Join(ctxt.GOPATH, srcABˢ), 0);
    if (err == default!) {
        Ꮡt.Fatalf("found made-up package x.com/y/z in %s"u8, (~p).Dir);
    }
    @string e = err.Error();
    if (!strings.Contains(e, vendorTreeˢ)) {
        Ꮡt.Fatalf("error on failed import does not mention GOROOT/src/vendor directory:\n%s"u8, e);
    }
}

public static void TestImportVendorParentFailure(ж<testing.T> Ꮡt) {
    testenv.MustHaveSource(new build_internal_test_package.testing_TжTB(Ꮡt));
    Ꮡt.Setenv(go111moduleˢ, offˢ);
    ref var ctxt = ref heap<global::go.go.build_package.Context>(out var Ꮡctxt);
    ctxt = Default;
    var (wd, err) = os.Getwd();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ctxt.GOPATH = filepath.Join(wd, testdataWithvendorˢ);
    // This import should fail because the vendor/c directory has no source code.
    (var p, err) = Ꮡctxt.Import("c"u8, filepath.Join(ctxt.GOPATH, srcABˢ), 0);
    if (err == default!) {
        Ꮡt.Fatalf("found empty parent in %s"u8, (~p).Dir);
    }
    if (p != nil && (~p).Dir != ""u8) {
        Ꮡt.Fatalf("decided to use %s"u8, (~p).Dir);
    }
    @string e = err.Error();
    if (!strings.Contains(e, vendorTreeˢ)) {
        Ꮡt.Fatalf("error on failed import does not mention GOROOT/src/vendor directory:\n%s"u8, e);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goproxyˢ = "GOPROXY"u8;
internal static readonly @string srcExampleComPˢ = "src/example.com/p"u8;
internal static readonly @string srcExampleComPPGoˢ = "src/example.com/p/p.go"u8;
internal static readonly @string goModFileNotFoundInˢ = "go.mod file not found in current directory or any parent directory"u8;
internal static readonly @string exampleComPˢ = "example.com/p"u8;
internal static readonly object importingPackageWhenNoGoˢ = (@string)"importing package when no go.mod is present succeeded unexpectedly"u8;

// Check that a package is loaded in module mode if GO111MODULE=on, even when
// no go.mod file is present. It should fail to resolve packages outside std.
// Verifies golang.org/issue/34669.
public static void TestImportPackageOutsideModule(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testenv.MustHaveGoBuild(new build_internal_test_package.testing_TжTB(Ꮡt));
    // Disable module fetching for this test so that 'go list' fails quickly
    // without trying to find the latest version of a module.
    Ꮡt.Setenv(goproxyˢ, offˢ);
    // Create a GOPATH in a temporary directory. We don't use testdata
    // because it's in GOROOT, which interferes with the module heuristic.
    @string gopath = Ꮡt.TempDir();
    {
        var err = os.MkdirAll(filepath.Join(gopath, srcExampleComPˢ), 511); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        var err = os.WriteFile(filepath.Join(gopath, srcExampleComPPGoˢ), slice<byte>("package p"u8), 438); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    Ꮡt.Setenv(go111moduleˢ, "on"u8);
    Ꮡt.Setenv(gopathˢ, gopath);
    ref var ctxt = ref heap<global::go.go.build_package.Context>(out var Ꮡctxt);
    ctxt = Default;
    ctxt.GOPATH = gopath;
    ctxt.Dir = filepath.Join(gopath, srcExampleComPˢ);
    @string want = goModFileNotFoundInˢ;
    {
        var (_, err) = Ꮡctxt.Import(exampleComPˢ, gopath, FindOnly); if (err == default!){
            Ꮡt.Fatal(importingPackageWhenNoGoˢ);
        } else 
        {
            @string errStr = err.Error(); if (!strings.Contains(errStr, want)){
                Ꮡt.Fatalf("error when importing package when no go.mod is present: got %q; want %q"u8, errStr, want);
            } else {
                Ꮡt.Logf(@"ctxt.Import(""example.com/p"", _, FindOnly): %v"u8, err);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataDocˢ = "testdata/doc"u8;

// TestIssue23594 prevents go/build from regressing and populating Package.Doc
// from comments in test files.
public static void TestIssue23594(ж<testing.T> Ꮡt) {
    // Package testdata/doc contains regular and external test files
    // with comments attached to their package declarations. The names of the files
    // ensure that we see the comments from the test files first.
    var (p, err) = ImportDir(testdataDocˢ, 0);
    if (err != default!) {
        Ꮡt.Fatalf("could not import testdata: %v"u8, err);
    }
    if ((~p).Doc != "Correct"u8) {
        Ꮡt.Fatalf("incorrectly set .Doc to %q"u8, (~p).Doc);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataBadsˢ = "testdata/bads"u8;

// TestIssue56509 tests that go/build does not add non-go files to InvalidGoFiles
// when they have unparsable comments.
public static void TestIssue56509(ж<testing.T> Ꮡt) {
    // The directory testdata/bads contains a .s file that has an unparsable
    // comment. (go/build parses initial comments in non-go files looking for
    // //go:build or //+go build comments).
    var (p, err) = ImportDir(testdataBadsˢ, 0);
    if (err == default!) {
        Ꮡt.Fatalf("could not import testdata/bads: %v"u8, err);
    }
    if (len((~p).InvalidGoFiles) != 0) {
        Ꮡt.Fatalf("incorrectly added non-go file to InvalidGoFiles"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gonoproxyˢ = "GONOPROXY"u8;
internal static readonly @string noneˢ = "none"u8;
internal static readonly @string exampleComHelloˢ = "example.com/hello"u8;
internal static readonly object unexpectedSuccessˢ = (@string)"unexpected success"u8;
internal static readonly @string goGet1ˢ = "://...?go-get=1"u8;

// TestMissingImportErrorRepetition checks that when an unknown package is
// imported, the package path is only shown once in the error.
// Verifies golang.org/issue/34752.
public static void TestMissingImportErrorRepetition(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testenv.MustHaveGoBuild(new build_internal_test_package.testing_TжTB(Ꮡt)); // need 'go list' internally
    @string tmp = Ꮡt.TempDir();
    {
        var errΔ1 = os.WriteFile(filepath.Join(tmp, goModˢ), slice<byte>("module m"u8), 438); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    Ꮡt.Setenv(go111moduleˢ, "on"u8);
    Ꮡt.Setenv(goproxyˢ, offˢ);
    Ꮡt.Setenv(gonoproxyˢ, noneˢ);
    ref var ctxt = ref heap<global::go.go.build_package.Context>(out var Ꮡctxt);
    ctxt = Default;
    ctxt.Dir = tmp;
    @string pkgPath = exampleComHelloˢ;
    var (_, err) = Ꮡctxt.Import(pkgPath, tmp, FindOnly);
    if (err == default!) {
        Ꮡt.Fatal(unexpectedSuccessˢ);
    }
    // Don't count the package path with a URL like https://...?go-get=1.
    // See golang.org/issue/35986.
    @string errStr = strings.ReplaceAll(err.Error(), "://"u8 + pkgPath + "?go-get=1"u8, goGet1ˢ);
    // Also don't count instances in suggested "go get" or similar commands
    // (see https://golang.org/issue/41576). The suggested command typically
    // follows a semicolon.
    (errStr, _, _) = strings.Cut(errStr, ";"u8);
    {
        nint n = strings.Count(errStr, pkgPath); if (n != 1) {
            Ꮡt.Fatalf("package path %q appears in error %d times; should appear once\nerror: %v"u8, pkgPath, n, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataCgoDisabledˢ = "testdata/cgo_disabled"u8;

// TestCgoImportsIgnored checks that imports in cgo files are not included
// in the imports list when cgo is disabled.
// Verifies golang.org/issue/35946.
public static void TestCgoImportsIgnored(ж<testing.T> Ꮡt) {
    ref var ctxt = ref heap<global::go.go.build_package.Context>(out var Ꮡctxt);
    ctxt = Default;
    ctxt.CgoEnabled = false;
    var (p, err) = Ꮡctxt.ImportDir(testdataCgoDisabledˢ, 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    foreach (var (_, path) in (~p).Imports) {
        if (path == "should/be/ignored"u8) {
            Ꮡt.Errorf("found import %q in ignored cgo file"u8, path);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string armˢ = "arm"u8;
internal static readonly @string netbsdˢ = "netbsd"u8;
internal static readonly @string testdataAlltagsˢ = "testdata/alltags"u8;
internal static readonly @string amd64ˢ = "amd64"u8;
internal static readonly @string linuxˢ = "linux"u8;

// Issue #52053. Check that if there is a file x_GOOS_GOARCH.go that both
// GOOS and GOARCH show up in the Package.AllTags field. We test both the
// case where the file matches and where the file does not match.
// The latter case used to fail, incorrectly omitting GOOS.
public static void TestAllTags(ж<testing.T> Ꮡt) {
    ref var ctxt = ref heap<global::go.go.build_package.Context>(out var Ꮡctxt);
    ctxt = Default;
    ctxt.GOARCH = armˢ;
    ctxt.GOOS = netbsdˢ;
    var (p, err) = Ꮡctxt.ImportDir(testdataAlltagsˢ, 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var want = new @string[]{"arm"u8, "netbsd"u8}.slice();
    if (!slices.Equal<slice<@string>, @string>((~p).AllTags, want)) {
        Ꮡt.Errorf("AllTags = %v, want %v"u8, (~p).AllTags, want);
    }
    var wantFiles = new @string[]{"alltags.go"u8, "x_netbsd_arm.go"u8}.slice();
    if (!slices.Equal<slice<@string>, @string>((~p).GoFiles, wantFiles)) {
        Ꮡt.Errorf("GoFiles = %v, want %v"u8, (~p).GoFiles, wantFiles);
    }
    ctxt.GOARCH = amd64ˢ;
    ctxt.GOOS = linuxˢ;
    (p, err) = Ꮡctxt.ImportDir(testdataAlltagsˢ, 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!slices.Equal<slice<@string>, @string>((~p).AllTags, want)) {
        Ꮡt.Errorf("AllTags = %v, want %v"u8, (~p).AllTags, want);
    }
    wantFiles = new @string[]{"alltags.go"u8}.slice();
    if (!slices.Equal<slice<@string>, @string>((~p).GoFiles, wantFiles)) {
        Ꮡt.Errorf("GoFiles = %v, want %v"u8, (~p).GoFiles, wantFiles);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataNonSourceTagsˢ = "testdata/non_source_tags"u8;

public static void TestAllTagsNonSourceFile(ж<testing.T> Ꮡt) {
    var (p, err) = ᏑDefault.ImportDir(testdataNonSourceTagsˢ, 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (len((~p).AllTags) > 0) {
        Ꮡt.Errorf("AllTags = %v, want empty"u8, (~p).AllTags);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataDirectivesˢ = "testdata/directives"u8;
internal static readonly @string testdataDirectivesˢ2 = "testdata/directives/"u8;
internal static readonly @string testdataDirectivesˢ3 = @"testdata\\directives\\"u8;
internal static readonly @string directivesˢ = "Directives"u8;
internal static readonly @string goMain1Testdataˢ = @"[{""//go:main1"" ""testdata/directives/a.go:1:1""} {""//go:plant"" ""testdata/directives/eve.go:1:1""}]"u8;
internal static readonly @string testDirectivesˢ = "TestDirectives"u8;
internal static readonly @string goTest1Testdataˢ = @"[{""//go:test1"" ""testdata/directives/a_test.go:1:1""} {""//go:test2"" ""testdata/directives/b_test.go:1:1""}]"u8;
internal static readonly @string xTestDirectivesˢ = "XTestDirectives"u8;
internal static readonly @string goXtest1Testdataˢ = @"[{""//go:xtest1"" ""testdata/directives/c_test.go:1:1""} {""//go:xtest2"" ""testdata/directives/d_test.go:1:1""} {""//go:xtest3"" ""testdata/directives/d_test.go:2:1""}]"u8;

public static void TestDirectives(ж<testing.T> Ꮡt) {
    var (p, err) = ImportDir(testdataDirectivesˢ, 0);
    if (err != default!) {
        Ꮡt.Fatalf("could not import testdata: %v"u8, err);
    }
    void check(@string name, slice<global::go.go.build_package.Directive> list, @string want) {
        if (runtime.GOOS == "windows"u8) {
            want = strings.ReplaceAll(want, testdataDirectivesˢ2, testdataDirectivesˢ3);
        }
        Ꮡt.Helper();
        @string s = fmt.Sprintf("%q"u8, list);
        if (s != want) {
            Ꮡt.Errorf("%s = %s, want %s"u8, name, s, want);
        }
    }
    check(directivesˢ, (~p).Directives,
        goMain1Testdataˢ);
    check(testDirectivesˢ, (~p).TestDirectives,
        goTest1Testdataˢ);
    check(xTestDirectivesˢ, (~p).XTestDirectives,
        goXtest1Testdataˢ);
}

} // end build_internal_test_package
