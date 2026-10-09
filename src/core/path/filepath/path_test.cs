// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.path;

using errors = errors_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using fs = io.fs_package;
using os = os_package;
using filepath = go.path.filepath_package;
using reflect = reflect_package;
using runtime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using syscall = syscall_package;
using testing = testing_package;
using @internal;
using go.path;
using io;
using static go.path.filepath_internal_test_package;

partial class filepath_test_package {

partial struct PathTest {
    internal @string path, result;
}

// Already clean
// Empty is current dir
// Remove trailing slash
// Remove doubled slash
// Remove . elements
// Remove .. elements
// Combinations
internal static slice<PathTest> cleantests = new PathTest[]{
    new("abc"u8, "abc"u8),
    new("abc/def"u8, "abc/def"u8),
    new("a/b/c"u8, "a/b/c"u8),
    new("."u8, "."u8),
    new(".."u8, ".."u8),
    new("../.."u8, "../.."u8),
    new("../../abc"u8, "../../abc"u8),
    new("/abc"u8, "/abc"u8),
    new("/"u8, "/"u8),
    new(""u8, "."u8),
    new("abc/"u8, "abc"u8),
    new("abc/def/"u8, "abc/def"u8),
    new("a/b/c/"u8, "a/b/c"u8),
    new("./"u8, "."u8),
    new("../"u8, ".."u8),
    new("../../"u8, "../.."u8),
    new("/abc/"u8, "/abc"u8),
    new("abc//def//ghi"u8, "abc/def/ghi"u8),
    new("abc//"u8, "abc"u8),
    new("abc/./def"u8, "abc/def"u8),
    new("/./abc/def"u8, "/abc/def"u8),
    new("abc/."u8, "abc"u8),
    new("abc/def/ghi/../jkl"u8, "abc/def/jkl"u8),
    new("abc/def/../ghi/../jkl"u8, "abc/jkl"u8),
    new("abc/def/.."u8, "abc"u8),
    new("abc/def/../.."u8, "."u8),
    new("/abc/def/../.."u8, "/"u8),
    new("abc/def/../../.."u8, ".."u8),
    new("/abc/def/../../.."u8, "/"u8),
    new("abc/def/../../../ghi/jkl/../../../mno"u8, "../../mno"u8),
    new("/../abc"u8, "/abc"u8),
    new("a/../b:/../../c"u8, @"../c"u8),
    new("abc/./../def"u8, "def"u8),
    new("abc//./../def"u8, "def"u8),
    new("abc/../../././../def"u8, "../../def"u8)
}.slice();

// Remove leading doubled slash
internal static slice<PathTest> nonwincleantests = new PathTest[]{
    new("//abc"u8, "/abc"u8),
    new("///abc"u8, "/abc"u8),
    new("//abc//"u8, "/abc"u8)
}.slice();

// Don't allow cleaning to move an element with a colon to the start of the path.
// Don't allow cleaning to create a Root Local Device path like \??\a.
internal static slice<PathTest> wincleantests = new PathTest[]{
    new(@"c:"u8, @"c:."u8),
    new(@"c:\"u8, @"c:\"u8),
    new(@"c:\abc"u8, @"c:\abc"u8),
    new(@"c:abc\..\..\.\.\..\def"u8, @"c:..\..\def"u8),
    new(@"c:\abc\def\..\.."u8, @"c:\"u8),
    new(@"c:\..\abc"u8, @"c:\abc"u8),
    new(@"c:..\abc"u8, @"c:..\abc"u8),
    new(@"c:\b:\..\..\..\d"u8, @"c:\d"u8),
    new(@"\"u8, @"\"u8),
    new(@"/"u8, @"\"u8),
    new(@"\\i\..\c$"u8, @"\\i\..\c$"u8),
    new(@"\\i\..\i\c$"u8, @"\\i\..\i\c$"u8),
    new(@"\\i\..\I\c$"u8, @"\\i\..\I\c$"u8),
    new(@"\\host\share\foo\..\bar"u8, @"\\host\share\bar"u8),
    new(@"//host/share/foo/../baz"u8, @"\\host\share\baz"u8),
    new(@"\\host\share\foo\..\..\..\..\bar"u8, @"\\host\share\bar"u8),
    new(@"\\.\C:\a\..\..\..\..\bar"u8, @"\\.\C:\bar"u8),
    new(@"\\.\C:\\\\a"u8, @"\\.\C:\a"u8),
    new(@"\\a\b\..\c"u8, @"\\a\b\c"u8),
    new(@"\\a\b"u8, @"\\a\b"u8),
    new(@".\c:"u8, @".\c:"u8),
    new(@".\c:\foo"u8, @".\c:\foo"u8),
    new(@".\c:foo"u8, @".\c:foo"u8),
    new(@"//abc"u8, @"\\abc"u8),
    new(@"///abc"u8, @"\\\abc"u8),
    new(@"//abc//"u8, @"\\abc\\"u8),
    new(@"\\?\C:\"u8, @"\\?\C:\"u8),
    new(@"\\?\C:\a"u8, @"\\?\C:\a"u8),
    new(@"a/../c:"u8, @".\c:"u8),
    new(@"a\..\c:"u8, @".\c:"u8),
    new(@"a/../c:/a"u8, @".\c:\a"u8),
    new(@"a/../../c:"u8, @"..\c:"u8),
    new(@"foo:bar"u8, @"foo:bar"u8),
    new(@"/a/../??/a"u8, @"\.\??\a"u8)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingMallocCountInˢ = (@string)"skipping malloc count in short mode"u8;
internal static readonly object skippingAllocsPerRunˢ = (@string)"skipping AllocsPerRun checks; GOMAXPROCS>1"u8;

public static void TestClean(ж<testing.T> Ꮡt) {
    var tests = cleantests;
    if (runtime.GOOS == "windows"u8){
        foreach (var (i, _) in tests) {
            tests[i].result = filepath.FromSlash(tests[i].result);
        }
        tests = appendꓸꓸꓸ(tests, wincleantests);
    } else {
        tests = appendꓸꓸꓸ(tests, nonwincleantests);
    }
    foreach (var (_, test) in tests) {
        {
            @string s = filepath.Clean(test.path); if (s != test.result) {
                Ꮡt.Errorf("Clean(%q) = %q, want %q"u8, test.path, s, test.result);
            }
        }
        {
            @string s = filepath.Clean(test.result); if (s != test.result) {
                Ꮡt.Errorf("Clean(%q) = %q, want %q"u8, test.result, s, test.result);
            }
        }
    }
    if (testing.Short()) {
        Ꮡt.Skip(skippingMallocCountInˢ);
    }
    if (runtime.GOMAXPROCS(0) > 1) {
        Ꮡt.Log(skippingAllocsPerRunˢ);
        return;
    }
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new PathTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        var allocs = testing.AllocsPerRun(100, () => {
            filepath.Clean(testʗ1.result);
        });
        if (allocs > 0D) {
            Ꮡt.Errorf("Clean(%q): %v allocs, want zero"u8, test.result, allocs);
        }
    }
}

partial struct IsLocalTest {
    internal @string path;
    internal bool isLocal;
}

internal static slice<IsLocalTest> islocaltests = new IsLocalTest[]{
    new(""u8, false),
    new("."u8, true),
    new(".."u8, false),
    new("../a"u8, false),
    new("/"u8, false),
    new("/a"u8, false),
    new("/a/../.."u8, false),
    new("a"u8, true),
    new("a/../a"u8, true),
    new("a/"u8, true),
    new("a/."u8, true),
    new("a/./b/./c"u8, true),
    new(@"a/../b:/../../c"u8, false)
}.slice();

// not a special file name
internal static slice<IsLocalTest> winislocaltests = new IsLocalTest[]{
    new("NUL"u8, false),
    new("nul"u8, false),
    new("nul "u8, false),
    new("nul."u8, false),
    new("a/nul:"u8, false),
    new("a/nul : a"u8, false),
    new("com0"u8, true),
    new("com1"u8, false),
    new("com2"u8, false),
    new("com3"u8, false),
    new("com4"u8, false),
    new("com5"u8, false),
    new("com6"u8, false),
    new("com7"u8, false),
    new("com8"u8, false),
    new("com9"u8, false),
    new("com¹"u8, false),
    new("com²"u8, false),
    new("com³"u8, false),
    new("com¹ : a"u8, false),
    new("cOm1"u8, false),
    new("lpt1"u8, false),
    new("LPT1"u8, false),
    new("lpt³"u8, false),
    new("./nul"u8, false),
    new(@"\"u8, false),
    new(@"\a"u8, false),
    new(@"C:"u8, false),
    new(@"C:\a"u8, false),
    new(@"..\a"u8, false),
    new(@"a/../c:"u8, false),
    new(@"CONIN$"u8, false),
    new(@"conin$"u8, false),
    new(@"CONOUT$"u8, false),
    new(@"conout$"u8, false),
    new(@"dollar$"u8, true)
}.slice();

internal static slice<IsLocalTest> plan9islocaltests = new IsLocalTest[]{
    new("#a"u8, false)
}.slice();

public static void TestIsLocal(ж<testing.T> Ꮡt) {
    var tests = islocaltests;
    if (runtime.GOOS == "windows"u8) {
        tests = appendꓸꓸꓸ(tests, winislocaltests);
    }
    if (runtime.GOOS == "plan9"u8) {
        tests = appendꓸꓸꓸ(tests, plan9islocaltests);
    }
    foreach (var (_, test) in tests) {
        {
            var got = filepath.IsLocal(test.path); if (got != test.isLocal) {
                Ꮡt.Errorf("IsLocal(%q) = %v, want %v"u8, test.path, got, test.isLocal);
            }
        }
    }
}

partial struct LocalizeTest {
    internal @string path;
    internal @string want;
}

internal static slice<LocalizeTest> localizetests = new LocalizeTest[]{
    new(""u8, ""u8),
    new("."u8, "."u8),
    new(".."u8, ""u8),
    new("a/.."u8, ""u8),
    new("/"u8, ""u8),
    new("/a"u8, ""u8),
    new(((@string)(new byte[]{0x61, 0xff, 0x62})), ""u8),
    new("a/"u8, ""u8),
    new("a/./b"u8, ""u8),
    new("\x00"u8, ""u8),
    new("a"u8, "a"u8),
    new("a/b/c"u8, "a/b/c"u8)
}.slice();

internal static slice<LocalizeTest> plan9localizetests = new LocalizeTest[]{
    new("#a"u8, ""u8),
    new(@"a\b:c"u8, @"a\b:c"u8)
}.slice();

internal static slice<LocalizeTest> unixlocalizetests = new LocalizeTest[]{
    new("#a"u8, "#a"u8),
    new(@"a\b:c"u8, @"a\b:c"u8)
}.slice();

internal static slice<LocalizeTest> winlocalizetests = new LocalizeTest[]{
    new("#a"u8, "#a"u8),
    new("c:"u8, ""u8),
    new(@"a\b"u8, ""u8),
    new(@"a:b"u8, ""u8),
    new(@"a/b:c"u8, ""u8),
    new(@"NUL"u8, ""u8),
    new(@"a/NUL"u8, ""u8),
    new(@"./com1"u8, ""u8),
    new(@"a/nul/b"u8, ""u8)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string errorˢ = "error"u8;

public static void TestLocalize(ж<testing.T> Ꮡt) {
    var tests = localizetests;
    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "plan9"u8) {
        tests = appendꓸꓸꓸ(tests, plan9localizetests);
    }
    else if (exprᴛ1 == "windows"u8) {
        tests = appendꓸꓸꓸ(tests, winlocalizetests);
        foreach (var (i, _) in tests) {
            tests[i].want = filepath.FromSlash(tests[i].want);
        }
    }
    else { /* default: */
        tests = appendꓸꓸꓸ(tests, unixlocalizetests);
    }

    foreach (var (_, test) in tests) {
        var (got, err) = filepath.Localize(test.path);
        @string wantErr = nilˢ;
        if (test.want == ""u8) {
            wantErr = errorˢ;
        }
        if (got != test.want || ((err == default!) != (test.want != ""u8))) {
            Ꮡt.Errorf("IsLocal(%q) = %q, %v want %q, %v"u8, test.path, got, err, test.want, wantErr);
        }
    }
}

internal static UntypedInt sep => /* filepath.Separator */ 92;

internal static slice<PathTest> slashtests = new PathTest[]{
    new(""u8, ""u8),
    new("/"u8, ((@string)(rune)sep)),
    new("/a/b"u8, ((@string)new byte[]{sep, (rune)'a', sep, (rune)'b'}.slice())),
    new("a//b"u8, ((@string)new byte[]{(rune)'a', sep, sep, (rune)'b'}.slice()))
}.slice();

public static void TestFromAndToSlash(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in slashtests) {
        {
            @string s = filepath.FromSlash(test.path); if (s != test.result) {
                Ꮡt.Errorf("FromSlash(%q) = %q, want %q"u8, test.path, s, test.result);
            }
        }
        {
            @string s = filepath.ToSlash(test.result); if (s != test.path) {
                Ꮡt.Errorf("ToSlash(%q) = %q, want %q"u8, test.result, s, test.path);
            }
        }
    }
}

partial struct SplitListTest {
    internal @string list;
    internal slice<@string> result;
}

internal static UntypedInt lsep => /* filepath.ListSeparator */ 59;

internal static slice<SplitListTest> splitlisttests = new SplitListTest[]{
    new(""u8, new @string[]{}.slice()),
    new(((@string)new byte[]{(rune)'a', lsep, (rune)'b'}.slice()), new @string[]{"a"u8, "b"u8}.slice()),
    new(((@string)new byte[]{lsep, (rune)'a', lsep, (rune)'b'}.slice()), new @string[]{""u8, "a"u8, "b"u8}.slice())
}.slice();

// quoted
// semicolon
// partially quoted
internal static slice<SplitListTest> winsplitlisttests = new SplitListTest[]{
    new(@"""a"""u8, new @string[]{@"a"u8}.slice()),
    new(@""";"""u8, new @string[]{@";"u8}.slice()),
    new(@"""a;b"""u8, new @string[]{@"a;b"u8}.slice()),
    new(@""";"";"u8, new @string[]{@";"u8, @""u8}.slice()),
    new(@";"";"""u8, new @string[]{@""u8, @";"u8}.slice()),
    new(@"a"";""b"u8, new @string[]{@"a;b"u8}.slice()),
    new(@"a; """"b"u8, new @string[]{@"a"u8, @" b"u8}.slice()),
    new(@"""a;b"u8, new @string[]{@"a;b"u8}.slice()),
    new(@"""""a;b"u8, new @string[]{@"a"u8, @"b"u8}.slice()),
    new(@"""""""a;b"u8, new @string[]{@"a;b"u8}.slice()),
    new(@"""""""""a;b"u8, new @string[]{@"a"u8, @"b"u8}.slice()),
    new(@"a"";b"u8, new @string[]{@"a;b"u8}.slice()),
    new(@"a;b"";c"u8, new @string[]{@"a"u8, @"b;c"u8}.slice()),
    new(@"""a"";b"";c"u8, new @string[]{@"a"u8, @"b;c"u8}.slice())
}.slice();

public static void TestSplitList(ж<testing.T> Ꮡt) {
    var tests = splitlisttests;
    if (runtime.GOOS == "windows"u8) {
        tests = appendꓸꓸꓸ(tests, winsplitlisttests);
    }
    foreach (var (_, test) in tests) {
        {
            var l = filepath.SplitList(test.list); if (!slices.Equal<slice<@string>, @string>(l, test.result)) {
                Ꮡt.Errorf("SplitList(%#q) = %#q, want %#q"u8, test.list, l, test.result);
            }
        }
    }
}

partial struct SplitTest {
    internal @string path, dir, @file;
}

internal static slice<SplitTest> unixsplittests = new SplitTest[]{
    new("a/b"u8, "a/"u8, "b"u8),
    new("a/b/"u8, "a/b/"u8, ""u8),
    new("a/"u8, "a/"u8, ""u8),
    new("a"u8, ""u8, "a"u8),
    new("/"u8, "/"u8, ""u8)
}.slice();

internal static slice<SplitTest> winsplittests = new SplitTest[]{
    new(@"c:"u8, @"c:"u8, @""u8),
    new(@"c:/"u8, @"c:/"u8, @""u8),
    new(@"c:/foo"u8, @"c:/"u8, @"foo"u8),
    new(@"c:/foo/bar"u8, @"c:/foo/"u8, @"bar"u8),
    new(@"//host/share"u8, @"//host/share"u8, @""u8),
    new(@"//host/share/"u8, @"//host/share/"u8, @""u8),
    new(@"//host/share/foo"u8, @"//host/share/"u8, @"foo"u8),
    new(@"\\host\share"u8, @"\\host\share"u8, @""u8),
    new(@"\\host\share\"u8, @"\\host\share\"u8, @""u8),
    new(@"\\host\share\foo"u8, @"\\host\share\"u8, @"foo"u8)
}.slice();

public static void TestSplit(ж<testing.T> Ꮡt) {
    slice<SplitTest> splittests = default!;
    splittests = unixsplittests;
    if (runtime.GOOS == "windows"u8) {
        splittests = appendꓸꓸꓸ(splittests, winsplittests);
    }
    foreach (var (_, test) in splittests) {
        {
            var (d, f) = filepath.Split(test.path); if (d != test.dir || f != test.@file) {
                Ꮡt.Errorf("Split(%q) = %q, %q, want %q, %q"u8, test.path, d, f, test.dir, test.@file);
            }
        }
    }
}

partial struct JoinTest {
    internal slice<@string> elem;
    internal @string path;
}

// zero parameters
// one parameter
// two parameters
// three parameters
internal static slice<JoinTest> jointests = new JoinTest[]{
    new(new @string[]{}.slice(), ""u8),
    new(new @string[]{""u8}.slice(), ""u8),
    new(new @string[]{"/"u8}.slice(), "/"u8),
    new(new @string[]{"a"u8}.slice(), "a"u8),
    new(new @string[]{"a"u8, "b"u8}.slice(), "a/b"u8),
    new(new @string[]{"a"u8, ""u8}.slice(), "a"u8),
    new(new @string[]{""u8, "b"u8}.slice(), "b"u8),
    new(new @string[]{"/"u8, "a"u8}.slice(), "/a"u8),
    new(new @string[]{"/"u8, "a/b"u8}.slice(), "/a/b"u8),
    new(new @string[]{"/"u8, ""u8}.slice(), "/"u8),
    new(new @string[]{"/a"u8, "b"u8}.slice(), "/a/b"u8),
    new(new @string[]{"a"u8, "/b"u8}.slice(), "a/b"u8),
    new(new @string[]{"/a"u8, "/b"u8}.slice(), "/a/b"u8),
    new(new @string[]{"a/"u8, "b"u8}.slice(), "a/b"u8),
    new(new @string[]{"a/"u8, ""u8}.slice(), "a"u8),
    new(new @string[]{""u8, ""u8}.slice(), ""u8),
    new(new @string[]{"/"u8, "a"u8, "b"u8}.slice(), "/a/b"u8)
}.slice();

internal static slice<JoinTest> nonwinjointests = new JoinTest[]{
    new(new @string[]{"//"u8, "a"u8}.slice(), "/a"u8)
}.slice();

internal static slice<JoinTest> winjointests = new JoinTest[]{
    new(new @string[]{@"directory"u8, @"file"u8}.slice(), @"directory\file"u8),
    new(new @string[]{@"C:\Windows\"u8, @"System32"u8}.slice(), @"C:\Windows\System32"u8),
    new(new @string[]{@"C:\Windows\"u8, @""u8}.slice(), @"C:\Windows"u8),
    new(new @string[]{@"C:\"u8, @"Windows"u8}.slice(), @"C:\Windows"u8),
    new(new @string[]{@"C:"u8, @"a"u8}.slice(), @"C:a"u8),
    new(new @string[]{@"C:"u8, @"a\b"u8}.slice(), @"C:a\b"u8),
    new(new @string[]{@"C:"u8, @"a"u8, @"b"u8}.slice(), @"C:a\b"u8),
    new(new @string[]{@"C:"u8, @""u8, @"b"u8}.slice(), @"C:b"u8),
    new(new @string[]{@"C:"u8, @""u8, @""u8, @"b"u8}.slice(), @"C:b"u8),
    new(new @string[]{@"C:"u8, @""u8}.slice(), @"C:."u8),
    new(new @string[]{@"C:"u8, @""u8, @""u8}.slice(), @"C:."u8),
    new(new @string[]{@"C:"u8, @"\a"u8}.slice(), @"C:\a"u8),
    new(new @string[]{@"C:"u8, @""u8, @"\a"u8}.slice(), @"C:\a"u8),
    new(new @string[]{@"C:."u8, @"a"u8}.slice(), @"C:a"u8),
    new(new @string[]{@"C:a"u8, @"b"u8}.slice(), @"C:a\b"u8),
    new(new @string[]{@"C:a"u8, @"b"u8, @"d"u8}.slice(), @"C:a\b\d"u8),
    new(new @string[]{@"\\host\share"u8, @"foo"u8}.slice(), @"\\host\share\foo"u8),
    new(new @string[]{@"\\host\share\foo"u8}.slice(), @"\\host\share\foo"u8),
    new(new @string[]{@"//host/share"u8, @"foo/bar"u8}.slice(), @"\\host\share\foo\bar"u8),
    new(new @string[]{@"\"u8}.slice(), @"\"u8),
    new(new @string[]{@"\"u8, @""u8}.slice(), @"\"u8),
    new(new @string[]{@"\"u8, @"a"u8}.slice(), @"\a"u8),
    new(new @string[]{@"\\"u8, @"a"u8}.slice(), @"\\a"u8),
    new(new @string[]{@"\"u8, @"a"u8, @"b"u8}.slice(), @"\a\b"u8),
    new(new @string[]{@"\\"u8, @"a"u8, @"b"u8}.slice(), @"\\a\b"u8),
    new(new @string[]{@"\"u8, @"\\a\b"u8, @"c"u8}.slice(), @"\a\b\c"u8),
    new(new @string[]{@"\\a"u8, @"b"u8, @"c"u8}.slice(), @"\\a\b\c"u8),
    new(new @string[]{@"\\a\"u8, @"b"u8, @"c"u8}.slice(), @"\\a\b\c"u8),
    new(new @string[]{@"//"u8, @"a"u8}.slice(), @"\\a"u8),
    new(new @string[]{@"a:\b\c"u8, @"x\..\y:\..\..\z"u8}.slice(), @"a:\b\z"u8),
    new(new @string[]{@"\"u8, @"??\a"u8}.slice(), @"\.\??\a"u8)
}.slice();

public static void TestJoin(ж<testing.T> Ꮡt) {
    if (runtime.GOOS == "windows"u8){
        jointests = appendꓸꓸꓸ(jointests, winjointests);
    } else {
        jointests = appendꓸꓸꓸ(jointests, nonwinjointests);
    }
    foreach (var (_, test) in jointests) {
        @string expected = filepath.FromSlash(test.path);
        {
            @string p = filepath.Join(test.elem.ꓸꓸꓸ); if (p != expected) {
                Ꮡt.Errorf("join(%q) = %q, want %q"u8, test.elem, p, expected);
            }
        }
    }
}

partial struct ExtTest {
    internal @string path, ext;
}

internal static slice<ExtTest> exttests = new ExtTest[]{
    new("path.go"u8, ".go"u8),
    new("path.pb.go"u8, ".go"u8),
    new("a.dir/b"u8, ""u8),
    new("a.dir/b.go"u8, ".go"u8),
    new("a.dir/"u8, ""u8)
}.slice();

public static void TestExt(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in exttests) {
        {
            @string x = filepath.Ext(test.path); if (x != test.ext) {
                Ꮡt.Errorf("Ext(%q) = %q, want %q"u8, test.path, x, test.ext);
            }
        }
    }
}

partial struct Node {
    internal @string name;
    internal slice<ж<Node>> entries; // nil if the entry is a file
    internal nint mark;
}

internal static ж<Node> tree = Ꮡ(new Node(
    "testdata"u8,
    new ж<Node>[]{
        Ꮡ(new Node("a"u8, default!, 0)),
        Ꮡ(new Node("b"u8, new ж<Node>[]{}.slice(), 0)),
        Ꮡ(new Node("c"u8, default!, 0)),
        Ꮡ(new Node(
            "d"u8,
            new ж<Node>[]{
                Ꮡ(new Node("x"u8, default!, 0)),
                Ꮡ(new Node("y"u8, new ж<Node>[]{}.slice(), 0)),
                Ꮡ(new Node(
                    "z"u8,
                    new ж<Node>[]{
                        Ꮡ(new Node("u"u8, default!, 0)),
                        Ꮡ(new Node("v"u8, default!, 0))
                    }.slice(),
                    0))
            }.slice(),
            0))
    }.slice(),
    0
));

internal static void walkTree(ж<Node> Ꮡn, @string path, Action<@string, ж<Node>> f) {
    ref var n = ref Ꮡn.DerefOrNull();

    f(path, Ꮡn);
    foreach (var (_, e) in n.entries) {
        walkTree(e, filepath.Join(path, (~e).name), f);
    }
}

internal static void makeTree(ж<testing.T> Ꮡt) {
    walkTree(tree, (~tree).name, (@string path, ж<Node> n) => {
        if ((~n).entries == default!){
            var (fd, err) = os.Create(path);
            if (err != default!) {
                Ꮡt.Errorf("makeTree: %v"u8, err);
                return;
            }
            fd.Close();
        } else {
            os.Mkdir(path, 504);
        }
    });
}

internal static void markTree(ж<Node> Ꮡn) {
    walkTree(Ꮡn, ""u8, (@string path, ж<Node> nΔ1) => {
        nΔ1.Value.mark++;
    });
}

internal static void checkMarks(ж<testing.T> Ꮡt, bool report) {
    walkTree(tree, (~tree).name, (@string path, ж<Node> n) => {
        if ((~n).mark != 1 && report) {
            Ꮡt.Errorf("node %s mark = %d; expected 1"u8, path, (~n).mark);
        }
        n.Value.mark = 0;
    });
}

// Assumes that each node name is unique. Good enough for a test.
// If clear is true, any incoming error is cleared before return. The errors
// are always accumulated, though.
internal static error mark(fs.DirEntry d, error err, ж<slice<error>> Ꮡerrors, bool clear) {
    ref var errors = ref Ꮡerrors.DerefOrNull();

    @string name = d.Name();
    walkTree(tree, (~tree).name, (@string path, ж<Node> n) => {
        if ((~n).name == name) {
            n.Value.mark++;
        }
    });
    if (err != default!) {
        errors = append(errors, err);
        if (clear) {
            return default!;
        }
        return err;
    }
    return default!;
}

// tempDirCanonical returns a temporary directory for the test to use, ensuring
// that the returned path does not contain symlinks.
internal static @string tempDirCanonical(ж<testing.T> Ꮡt) {
    @string dir = Ꮡt.TempDir();
    var (cdir, err) = filepath.EvalSymlinks(dir);
    if (err != default!) {
        Ꮡt.Errorf("tempDirCanonical: %v"u8, err);
    }
    return cdir;
}

public static void TestWalk(ж<testing.T> Ꮡt) {
    var walk = (@string root, Func<@string, fs.DirEntry, error, error> fn) => filepath.Walk(root, (@string path, fs.FileInfo info, error err) => fn(path, fs.FileInfoToDirEntry(info), err));
    testWalk(Ꮡt, walk, 1);
}

public static void TestWalkDir(ж<testing.T> Ꮡt) {
    testWalk(Ꮡt, filepath.WalkDir, 2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string permErrˢ = "PermErr"u8;
internal static readonly object skippingAsRootˢ = (@string)"skipping as root"u8;
internal static readonly object skippingInShortModeˢ = (@string)"skipping in short mode"u8;

internal static void testWalk(ж<testing.T> Ꮡt, Func<@string, Func<@string, fs.DirEntry, error, error>, error> walk, nint errVisit) {
    Ꮡt.Chdir(Ꮡt.TempDir());
    makeTree(Ꮡt);
    ref var errors = ref heap<slice<error>>(out var Ꮡerrors);
    errors = new slice<error>(0, 10);
    var clear = true;
    var markFn = (@string path, fs.DirEntry d, error errΔ1) => mark(d, errΔ1, Ꮡerrors, clear);
    // Expect no errors.
    var err = walk((~tree).name, new Func<@string, fs.DirEntry, error, error>(markFn));
    if (err != default!) {
        Ꮡt.Fatalf("no error expected, found: %s"u8, err);
    }
    if (len(errors) != 0) {
        Ꮡt.Fatalf("unexpected errors: %s"u8, errors);
    }
    checkMarks(Ꮡt, true);
    errors = errors[0..0];
    var markFnʗ1 = markFn;
    Ꮡt.Run(permErrˢ, (ж<testing.T> tΔ1) => {
        // Test permission errors. Only possible if we're not root
        // and only on some file systems (AFS, FAT).  To avoid errors during
        // all.bash on those file systems, skip during go test -short.
        // Chmod is not supported on wasip1.
        if (runtime.GOOS == "windows"u8 || runtime.GOOS == "wasip1"u8) {
            tΔ1.Skip("skipping on " + runtime.GOOS);
        }
        if (os.Getuid() == 0) {
            tΔ1.Skip(skippingAsRootˢ);
        }
        if (testing.Short()) {
            tΔ1.Skip(skippingInShortModeˢ);
        }
        // introduce 2 errors: chmod top-level directories to 0
        os.Chmod(filepath.Join((~tree).name, (~(~tree).entries[1]).name), 0);
        os.Chmod(filepath.Join((~tree).name, (~(~tree).entries[3]).name), 0);
        // 3) capture errors, expect two.
        // mark respective subtrees manually
        markTree((~tree).entries[1]);
        markTree((~tree).entries[3]);
        // correct double-marking of directory itself
        (~tree).entries[1].Value.mark -= errVisit;
        (~tree).entries[3].Value.mark -= errVisit;
        var errΔ2 = walk((~tree).name, new Func<@string, fs.DirEntry, error, error>(markFnʗ1));
        if (errΔ2 != default!) {
            tΔ1.Fatalf("expected no error return from Walk, got %s"u8, errΔ2);
        }
        if (len(Ꮡerrors.ValueSlot) != 2) {
            tΔ1.Errorf("expected 2 errors, got %d: %s"u8, len(Ꮡerrors.ValueSlot), Ꮡerrors.ValueSlot);
        }
        // the inaccessible subtrees were marked manually
        checkMarks(tΔ1, true);
        Ꮡerrors.ValueSlot = Ꮡerrors.ValueSlot[0..0];
        // 4) capture errors, stop after first error.
        // mark respective subtrees manually
        markTree((~tree).entries[1]);
        markTree((~tree).entries[3]);
        // correct double-marking of directory itself
        (~tree).entries[1].Value.mark -= errVisit;
        (~tree).entries[3].Value.mark -= errVisit;
        clear = false; // error will stop processing
        errΔ2 = walk((~tree).name, new Func<@string, fs.DirEntry, error, error>(markFnʗ1));
        if (errΔ2 == default!) {
            tΔ1.Fatalf("expected error return from Walk"u8);
        }
        if (len(Ꮡerrors.ValueSlot) != 1) {
            tΔ1.Errorf("expected 1 error, got %d: %s"u8, len(Ꮡerrors.ValueSlot), Ꮡerrors.ValueSlot);
        }
        // the inaccessible subtrees were marked manually
        checkMarks(tΔ1, false);
        Ꮡerrors.ValueSlot = Ꮡerrors.ValueSlot[0..0];
        // restore permissions
        os.Chmod(filepath.Join((~tree).name, (~(~tree).entries[1]).name), 504);
        os.Chmod(filepath.Join((~tree).name, (~(~tree).entries[3]).name), 504);
    });
}

internal static void touch(ж<testing.T> Ꮡt, @string name) {
    var (f, err) = os.Create(name);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var errΔ1 = f.Close(); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dirˢ = "dir"u8;
internal static readonly @string dirFoo1ˢ = "dir/foo1"u8;
internal static readonly @string dirFoo2ˢ = "dir/foo2"u8;
internal static readonly @string foo2ˢ = "foo2"u8;
internal static readonly @string foo1ˢ = "foo1"u8;
internal static readonly @string walkˢ = "Walk"u8;
internal static readonly @string walkDirˢ = "WalkDir"u8;

public static void TestWalkSkipDirOnFile(ж<testing.T> Ꮡt) {
    @string td = Ꮡt.TempDir();
    {
        var err = os.MkdirAll(filepath.Join(td, dirˢ), 493); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    touch(Ꮡt, filepath.Join(td, dirFoo1ˢ));
    touch(Ꮡt, filepath.Join(td, dirFoo2ˢ));
    var sawFoo2 = false;
    error walker(@string path) {
        if (strings.HasSuffix(path, foo2ˢ)) {
            sawFoo2 = true;
        }
        if (strings.HasSuffix(path, foo1ˢ)) {
            return filepath.SkipDir;
        }
        return default!;
    }
    var walkerʗ1 = walker;
    var walkFn = (@string path, fs.FileInfo _Δp1, error _Δp2) => walkerʗ1(path);
    var walkerʗ2 = walker;
    var walkDirFn = (@string path, fs.DirEntry _Δp1, error _Δp2) => walkerʗ2(path);
    void check(ж<testing.T> tΔ1, Func<@string, error> walk, @string root) {
        tΔ1.Helper();
        sawFoo2 = false;
        var err = walk(root);
        if (err != default!) {
            tΔ1.Fatal(err);
        }
        if (sawFoo2) {
            tΔ1.Errorf("SkipDir on file foo1 did not block processing of foo2"u8);
        }
    }
    var checkʗ1 = check;
    var walkFnʗ1 = walkFn;
    Ꮡt.Run(walkˢ, (ж<testing.T> tΔ2) => {
        var walkFnʗ2 = walkFnʗ1;
        var Walk = (@string root) => filepath.Walk(td, new Func<@string, fs.FileInfo, error, error>(walkFnʗ2));
        checkʗ1(tΔ2, Walk, td);
        checkʗ1(tΔ2, Walk, filepath.Join(td, dirˢ));
    });
    var checkʗ2 = check;
    var walkDirFnʗ1 = walkDirFn;
    Ꮡt.Run(walkDirˢ, (ж<testing.T> tΔ3) => {
        var walkDirFnʗ2 = walkDirFnʗ1;
        var WalkDir = (@string root) => filepath.WalkDir(td, new Func<@string, fs.DirEntry, error, error>(walkDirFnʗ2));
        checkʗ2(tΔ3, WalkDir, td);
        checkʗ2(tΔ3, WalkDir, filepath.Join(td, dirˢ));
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string subdirˢ = "subdir"u8;
internal static readonly @string dir2ˢ = "dir2"u8;
internal static readonly @string foo3ˢ = "foo3"u8;
internal static readonly @string foo4ˢ = "foo4"u8;
internal static readonly @string barˢ = "bar"u8;
internal static readonly @string lastˢ = "last"u8;

public static void TestWalkSkipAllOnFile(ж<testing.T> Ꮡt) {
    @string td = Ꮡt.TempDir();
    {
        var err = os.MkdirAll(filepath.Join(td, dirˢ, subdirˢ), 493); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        var err = os.MkdirAll(filepath.Join(td, dir2ˢ), 493); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    touch(Ꮡt, filepath.Join(td, dirˢ, foo1ˢ));
    touch(Ꮡt, filepath.Join(td, dirˢ, foo2ˢ));
    touch(Ꮡt, filepath.Join(td, dirˢ, subdirˢ, foo3ˢ));
    touch(Ꮡt, filepath.Join(td, dirˢ, foo4ˢ));
    touch(Ꮡt, filepath.Join(td, dir2ˢ, barˢ));
    touch(Ꮡt, filepath.Join(td, lastˢ));
    var remainingWereSkipped = true;
    error walker(@string path) {
        if (strings.HasSuffix(path, foo2ˢ)) {
            return filepath.SkipAll;
        }
        if (strings.HasSuffix(path, foo3ˢ) || strings.HasSuffix(path, foo4ˢ) || strings.HasSuffix(path, barˢ) || strings.HasSuffix(path, lastˢ)) {
            remainingWereSkipped = false;
        }
        return default!;
    }
    var walkerʗ1 = walker;
    var walkFn = (@string path, fs.FileInfo _Δp1, error _Δp2) => walkerʗ1(path);
    var walkerʗ2 = walker;
    var walkDirFn = (@string path, fs.DirEntry _Δp1, error _Δp2) => walkerʗ2(path);
    void check(ж<testing.T> tΔ1, Func<@string, error> walk, @string root) {
        tΔ1.Helper();
        remainingWereSkipped = true;
        {
            var err = walk(root); if (err != default!) {
                tΔ1.Fatal(err);
            }
        }
        if (!remainingWereSkipped) {
            tΔ1.Errorf("SkipAll on file foo2 did not block processing of remaining files and directories"u8);
        }
    }
    var checkʗ1 = check;
    var walkFnʗ1 = walkFn;
    Ꮡt.Run(walkˢ, (ж<testing.T> tΔ2) => {
        var walkFnʗ2 = walkFnʗ1;
        var Walk = (@string _) => filepath.Walk(td, new Func<@string, fs.FileInfo, error, error>(walkFnʗ2));
        checkʗ1(tΔ2, Walk, td);
        checkʗ1(tΔ2, Walk, filepath.Join(td, dirˢ));
    });
    var checkʗ2 = check;
    var walkDirFnʗ1 = walkDirFn;
    Ꮡt.Run(walkDirˢ, (ж<testing.T> tΔ3) => {
        var walkDirFnʗ2 = walkDirFnʗ1;
        var WalkDir = (@string _) => filepath.WalkDir(td, new Func<@string, fs.DirEntry, error, error>(walkDirFnʗ2));
        checkʗ2(tΔ3, WalkDir, td);
        checkʗ2(tΔ3, WalkDir, filepath.Join(td, dirˢ));
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = "foo"u8;
internal static readonly @string bazˢ = "baz"u8;
internal static readonly @string statErrorˢ = "stat-error"u8;
internal static readonly @string someStatErrorˢ = "some stat error"u8;

public static void TestWalkFileError(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string td = Ꮡt.TempDir();
        touch(Ꮡt, filepath.Join(td, fooˢ));
        touch(Ꮡt, filepath.Join(td, barˢ));
        @string dir = filepath.Join(td, dirˢ);
        {
            var errΔ1 = os.MkdirAll(filepath.Join(td, dirˢ), 493); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        touch(Ꮡt, filepath.Join(dir, bazˢ));
        touch(Ꮡt, filepath.Join(dir, statErrorˢ));
        defer(() => {
            filepath_internal_test_package.LstatP.ValueSlot = os.Lstat;
        }, ref ᒐ);
        var statErr = errors.New(someStatErrorˢ);
        var statErrʗ1 = statErr;
        filepath_internal_test_package.LstatP.ValueSlot = (fs.FileInfo, error) (@string path) => {
            if (strings.HasSuffix(path, statErrorˢ)) {
                return (default!, statErrʗ1);
            }
            return os.Lstat(path);
        };
        var got = new map<@string, error>{};
        var gotʗ1 = got;
        var err = filepath.Walk(td, (@string path, fs.FileInfo fi, error errΔ2) => {
            var (rel, _) = filepath.Rel(td, path);
            gotʗ1[filepath.ToSlash(rel)] = errΔ2;
            return default!;
        });
        if (err != default!) {
            Ꮡt.Errorf("Walk error: %v"u8, err);
        }
        var want = new map<@string, error>{["."u8] = default!, ["foo"u8] = default!, ["bar"u8] = default!, ["dir"u8] = default!, ["dir/baz"u8] = default!, ["dir/stat-error"u8] = statErr
        };
        if (!reflect.DeepEqual(got, want)) {
            Ꮡt.Errorf("Walked %#v; want %#v"u8, got, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string linkˢ = "link"u8;
internal static readonly @string abslinkˢ = "abslink"u8;
internal static readonly @string linklinkˢ = "linklink"u8;

internal partial struct TestWalkSymlinkRoot_type /*dyn*/ {
    internal @string desc;
    internal @string root;
    internal slice<@string> want;
    internal slice<@string> buggyGOOS;
}

public static void TestWalkSymlinkRoot(ж<testing.T> Ꮡt) {
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(Ꮡt));
    @string td = Ꮡt.TempDir();
    @string dir = filepath.Join(td, dirˢ);
    {
        var err = os.MkdirAll(filepath.Join(td, dirˢ), 493); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    touch(Ꮡt, filepath.Join(dir, fooˢ));
    @string link = filepath.Join(td, linkˢ);
    {
        var err = os.Symlink(dirˢ, link); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    @string abslink = filepath.Join(td, abslinkˢ);
    {
        var err = os.Symlink(dir, abslink); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    @string linklink = filepath.Join(td, linklinkˢ);
    {
        var err = os.Symlink(linkˢ, linklink); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    // Per https://pubs.opengroup.org/onlinepubs/9699919799.2013edition/basedefs/V1_chap04.html#tag_04_12:
    // “A pathname that contains at least one non- <slash> character and that ends
    // with one or more trailing <slash> characters shall not be resolved
    // successfully unless the last pathname component before the trailing <slash>
    // characters names an existing directory [...].”
    //
    // Since Walk does not traverse symlinks itself, its behavior should depend on
    // whether the path passed to Walk ends in a slash: if it does not end in a slash,
    // Walk should report the symlink itself (since it is the last pathname component);
    // but if it does end in a slash, Walk should walk the directory to which the symlink
    // refers (since it must be fully resolved before walking).
    foreach (var (_, tt) in new TestWalkSymlinkRoot_type[]{
        new(
            desc: "no slash"u8,
            root: link,
            want: new @string[]{link}.slice()
        ),
        new(
            desc: "slash"u8,
            root: link + ((@string)(rune)filepath.Separator),
            want: new @string[]{link, filepath.Join(link, fooˢ)}.slice()
        ),
        new(
            desc: "abs no slash"u8,
            root: abslink,
            want: new @string[]{abslink}.slice()
        ),
        new(
            desc: "abs with slash"u8,
            root: abslink + ((@string)(rune)filepath.Separator),
            want: new @string[]{abslink, filepath.Join(abslink, fooˢ)}.slice()
        ),
        new(
            desc: "double link no slash"u8,
            root: linklink,
            want: new @string[]{linklink}.slice()
        ),
        new(
            desc: "double link with slash"u8,
            root: linklink + ((@string)(rune)filepath.Separator),
            want: new @string[]{linklink, filepath.Join(linklink, fooˢ)}.slice(),
            buggyGOOS: new @string[]{"darwin"u8, "ios"u8}.slice() // https://go.dev/issue/59586

        )
    }.slice()) {
        ref var ttΔ1 = ref heap<TestWalkSymlinkRoot_type>(out var ᏑttΔ1);
        ttΔ1 = tt;
        var ttʗ1 = ttΔ1;
        Ꮡt.Run(ttΔ1.desc, (ж<testing.T> tΔ1) => {
            ref var walked = ref heap<slice<@string>>(out var Ꮡwalked);
            var err = filepath.Walk(ttʗ1.root, error (@string path, fs.FileInfo info, error errΔ1) => {
                if (errΔ1 != default!) {
                    return errΔ1;
                }
                tΔ1.Logf("%#q: %v"u8, path, info.Mode());
                Ꮡwalked.ValueSlot = append(Ꮡwalked.ValueSlot, filepath.Clean(path));
                return default!;
            });
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            if (!slices.Equal<slice<@string>, @string>(Ꮡwalked.ValueSlot, ttʗ1.want)) {
                tΔ1.Logf("Walk(%#q) visited %#q; want %#q"u8, ttʗ1.root, Ꮡwalked.ValueSlot, ttʗ1.want);
                if (slices.Contains(ttʗ1.buggyGOOS, runtime.GOOS)){
                    tΔ1.Logf("(ignoring known bug on %v)"u8, runtime.GOOS);
                } else {
                    tΔ1.Fail();
                }
            }
        });
    }
}

internal static slice<PathTest> basetests = new PathTest[]{
    new(""u8, "."u8),
    new("."u8, "."u8),
    new("/."u8, "."u8),
    new("/"u8, "/"u8),
    new("////"u8, "/"u8),
    new("x/"u8, "x"u8),
    new("abc"u8, "abc"u8),
    new("abc/def"u8, "def"u8),
    new("a/b/.x"u8, ".x"u8),
    new("a/b/c."u8, "c."u8),
    new("a/b/c.x"u8, "c.x"u8)
}.slice();

internal static slice<PathTest> winbasetests = new PathTest[]{
    new(@"c:\"u8, @"\"u8),
    new(@"c:."u8, @"."u8),
    new(@"c:\a\b"u8, @"b"u8),
    new(@"c:a\b"u8, @"b"u8),
    new(@"c:a\b\c"u8, @"c"u8),
    new(@"\\host\share\"u8, @"\"u8),
    new(@"\\host\share\a"u8, @"a"u8),
    new(@"\\host\share\a\b"u8, @"b"u8)
}.slice();

public static void TestBase(ж<testing.T> Ꮡt) {
    var tests = basetests;
    if (runtime.GOOS == "windows"u8) {
        // make unix tests work on windows
        foreach (var (i, _) in tests) {
            tests[i].result = filepath.Clean(tests[i].result);
        }
        // add windows specific tests
        tests = appendꓸꓸꓸ(tests, winbasetests);
    }
    foreach (var (_, test) in tests) {
        {
            @string s = filepath.Base(test.path); if (s != test.result) {
                Ꮡt.Errorf("Base(%q) = %q, want %q"u8, test.path, s, test.result);
            }
        }
    }
}

internal static slice<PathTest> dirtests = new PathTest[]{
    new(""u8, "."u8),
    new("."u8, "."u8),
    new("/."u8, "/"u8),
    new("/"u8, "/"u8),
    new("/foo"u8, "/"u8),
    new("x/"u8, "x"u8),
    new("abc"u8, "."u8),
    new("abc/def"u8, "abc"u8),
    new("a/b/.x"u8, "a/b"u8),
    new("a/b/c."u8, "a/b"u8),
    new("a/b/c.x"u8, "a/b"u8)
}.slice();

internal static slice<PathTest> nonwindirtests = new PathTest[]{
    new("////"u8, "/"u8)
}.slice();

internal static slice<PathTest> windirtests = new PathTest[]{
    new(@"c:\"u8, @"c:\"u8),
    new(@"c:."u8, @"c:."u8),
    new(@"c:\a\b"u8, @"c:\a"u8),
    new(@"c:a\b"u8, @"c:a"u8),
    new(@"c:a\b\c"u8, @"c:a\b"u8),
    new(@"\\host\share"u8, @"\\host\share"u8),
    new(@"\\host\share\"u8, @"\\host\share\"u8),
    new(@"\\host\share\a"u8, @"\\host\share\"u8),
    new(@"\\host\share\a\b"u8, @"\\host\share\a"u8),
    new(@"\\\\"u8, @"\\\\"u8)
}.slice();

public static void TestDir(ж<testing.T> Ꮡt) {
    var tests = dirtests;
    if (runtime.GOOS == "windows"u8){
        // make unix tests work on windows
        foreach (var (i, _) in tests) {
            tests[i].result = filepath.Clean(tests[i].result);
        }
        // add windows specific tests
        tests = appendꓸꓸꓸ(tests, windirtests);
    } else {
        tests = appendꓸꓸꓸ(tests, nonwindirtests);
    }
    foreach (var (_, test) in tests) {
        {
            @string s = filepath.Dir(test.path); if (s != test.result) {
                Ꮡt.Errorf("Dir(%q) = %q, want %q"u8, test.path, s, test.result);
            }
        }
    }
}

partial struct IsAbsTest {
    internal @string path;
    internal bool isAbs;
}

internal static slice<IsAbsTest> isabstests = new IsAbsTest[]{
    new(""u8, false),
    new("/"u8, true),
    new("/usr/bin/gcc"u8, true),
    new(".."u8, false),
    new("/a/../bb"u8, true),
    new("."u8, false),
    new("./"u8, false),
    new("lala"u8, false)
}.slice();

internal static slice<IsAbsTest> winisabstests = new IsAbsTest[]{
    new(@"C:\"u8, true),
    new(@"c\"u8, false),
    new(@"c::"u8, false),
    new(@"c:"u8, false),
    new(@"/"u8, false),
    new(@"\"u8, false),
    new(@"\Windows"u8, false),
    new(@"c:a\b"u8, false),
    new(@"c:\a\b"u8, true),
    new(@"c:/a/b"u8, true),
    new(@"\\host\share"u8, true),
    new(@"\\host\share\"u8, true),
    new(@"\\host\share\foo"u8, true),
    new(@"//host/share/foo/bar"u8, true),
    new(@"\\?\a\b\c"u8, true),
    new(@"\??\a\b\c"u8, true)
}.slice();

public static void TestIsAbs(ж<testing.T> Ꮡt) {
    slice<IsAbsTest> tests = default!;
    if (runtime.GOOS == "windows"u8){
        tests = appendꓸꓸꓸ(tests, winisabstests);
        // All non-windows tests should fail, because they have no volume letter.
        foreach (var (_, test) in isabstests) {
            tests = append(tests, new IsAbsTest(test.path, false));
        }
        // All non-windows test should work as intended if prefixed with volume letter.
        foreach (var (_, test) in isabstests) {
            tests = append(tests, new IsAbsTest("c:"u8 + test.path, test.isAbs));
        }
    } else {
        tests = isabstests;
    }
    foreach (var (_, test) in tests) {
        {
            var r = filepath.IsAbs(test.path); if (r != test.isAbs) {
                Ꮡt.Errorf("IsAbs(%q) = %v, want %v"u8, test.path, r, test.isAbs);
            }
        }
    }
}

partial struct EvalSymlinksTest {
    // If dest is empty, the path is created; otherwise the dest is symlinked to the path.
    internal @string path, dest;
}

// Issue 23444.
public static slice<EvalSymlinksTest> EvalSymlinksTestDirs = new EvalSymlinksTest[]{
    new("test"u8, ""u8),
    new("test/dir"u8, ""u8),
    new("test/dir/link3"u8, "../../"u8),
    new("test/link1"u8, "../test"u8),
    new("test/link2"u8, "dir"u8),
    new("test/linkabs"u8, "/"u8),
    new("test/link4"u8, "../test2"u8),
    new("test2"u8, "test/dir"u8),
    new("src"u8, ""u8),
    new("src/pool"u8, ""u8),
    new("src/pool/test"u8, ""u8),
    new("src/versions"u8, ""u8),
    new("src/versions/current"u8, "../../version"u8),
    new("src/versions/v1"u8, ""u8),
    new("src/versions/v1/modules"u8, ""u8),
    new("src/versions/v1/modules/test"u8, "../../../pool/test"u8),
    new("version"u8, "src/versions/v1"u8)
}.slice();

public static slice<EvalSymlinksTest> EvalSymlinksTests = new EvalSymlinksTest[]{
    new("test"u8, "test"u8),
    new("test/dir"u8, "test/dir"u8),
    new("test/dir/../.."u8, "."u8),
    new("test/link1"u8, "test"u8),
    new("test/link2"u8, "test/dir"u8),
    new("test/link1/dir"u8, "test/dir"u8),
    new("test/link2/.."u8, "test"u8),
    new("test/dir/link3"u8, "."u8),
    new("test/link2/link3/test"u8, "test"u8),
    new("test/linkabs"u8, "/"u8),
    new("test/link4/.."u8, "test"u8),
    new("src/versions/current/modules/test"u8, "src/pool/test"u8)
}.slice();

// simpleJoin builds a file name from the directory and path.
// It does not use Join because we don't want ".." to be evaluated.
internal static @string simpleJoin(@string dir, @string path) {
    return dir + ((@string)(rune)filepath.Separator) + path;
}

internal static void testEvalSymlinks(ж<testing.T> Ꮡt, @string path, @string want) {
    var (have, err) = filepath.EvalSymlinks(path);
    if (err != default!) {
        Ꮡt.Errorf("EvalSymlinks(%q) error: %v"u8, path, err);
        return;
    }
    if (filepath.Clean(have) != filepath.Clean(want)) {
        Ꮡt.Errorf("EvalSymlinks(%q) returns %q, want %q"u8, path, have, want);
    }
}

internal static void testEvalSymlinksAfterChdir(ж<testing.T> Ꮡt, @string wd, @string path, @string want) {
    Ꮡt.Chdir(wd);
    var (have, err) = filepath.EvalSymlinks(path);
    if (err != default!) {
        Ꮡt.Errorf("EvalSymlinks(%q) in %q directory error: %v"u8, path, wd, err);
        return;
    }
    if (filepath.Clean(have) != filepath.Clean(want)) {
        Ꮡt.Errorf("EvalSymlinks(%q) in %q directory returns %q, want %q"u8, path, wd, have, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object evalSymlinkForTmpDirˢ = (@string)"eval symlink for tmp dir:"u8;
internal static readonly @string testˢ = "test"u8;

public static void TestEvalSymlinks(ж<testing.T> Ꮡt) {
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(Ꮡt));
    @string tmpDir = Ꮡt.TempDir();
    // /tmp may itself be a symlink! Avoid the confusion, although
    // it means trusting the thing we're testing.
    error err = default!;
    (tmpDir, err) = filepath.EvalSymlinks(tmpDir);
    if (err != default!) {
        Ꮡt.Fatal(evalSymlinkForTmpDirˢ, err);
    }
    // Create the symlink farm using relative paths.
    foreach (var (_, d) in EvalSymlinksTestDirs) {
        error errΔ1 = default!;
        @string path = simpleJoin(tmpDir, d.path);
        if (d.dest == ""u8){
            errΔ1 = os.Mkdir(path, 493);
        } else {
            errΔ1 = os.Symlink(d.dest, path);
        }
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    // Evaluate the symlink farm.
    foreach (var (_, test) in EvalSymlinksTests) {
        @string path = simpleJoin(tmpDir, test.path);
        @string dest = simpleJoin(tmpDir, test.dest);
        if (filepath.IsAbs(test.dest) || os.IsPathSeparator(test.dest[0])) {
            dest = test.dest;
        }
        testEvalSymlinks(Ꮡt, path, dest);
        // test EvalSymlinks(".")
        testEvalSymlinksAfterChdir(Ꮡt, path, "."u8, "."u8);
        // test EvalSymlinks("C:.") on Windows
        if (runtime.GOOS == "windows"u8) {
            @string volDot = filepath.VolumeName(tmpDir) + "."u8;
            testEvalSymlinksAfterChdir(Ꮡt, path, volDot, volDot);
        }
        // test EvalSymlinks(".."+path)
        @string dotdotPath = simpleJoin(".."u8, test.dest);
        if (filepath.IsAbs(test.dest) || os.IsPathSeparator(test.dest[0])) {
            dotdotPath = test.dest;
        }
        testEvalSymlinksAfterChdir(Ꮡt,
            simpleJoin(tmpDir, testˢ),
            simpleJoin(".."u8, test.path),
            dotdotPath);
        // test EvalSymlinks(p) where p is relative path
        testEvalSymlinksAfterChdir(Ꮡt, tmpDir, test.path, test.dest);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string notexistˢ = "notexist"u8;

public static void TestEvalSymlinksIsNotExist(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(Ꮡt));
        Ꮡt.Chdir(Ꮡt.TempDir());
        var (_, err) = filepath.EvalSymlinks(notexistˢ);
        if (!os.IsNotExist(err)) {
            Ꮡt.Errorf("expected the file is not found, got %v\n"u8, err);
        }
        err = os.Symlink(notexistˢ, linkˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        defer(os.Remove, linkˢ, ref ᒐ);
        (_, err) = filepath.EvalSymlinks(linkˢ);
        if (!os.IsNotExist(err)) {
            Ꮡt.Errorf("expected the file is not found, got %v\n"u8, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string linkToDirˢ = "link_to_dir"u8;
internal static readonly @string fileˢ = "file"u8;
internal static readonly @string link1ˢ = "link1"u8;
internal static readonly @string link2ˢ = "link2"u8;

internal partial struct TestIssue13582_tests /*dyn*/ {
    internal @string path, want;
}

public static void TestIssue13582(ж<testing.T> Ꮡt) {
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(Ꮡt));
    @string tmpDir = Ꮡt.TempDir();
    @string dir = filepath.Join(tmpDir, dirˢ);
    var err = os.Mkdir(dir, 493);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string linkToDir = filepath.Join(tmpDir, linkToDirˢ);
    err = os.Symlink(dir, linkToDir);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string @file = filepath.Join(linkToDir, fileˢ);
    err = os.WriteFile(@file, default!, 420);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string link1 = filepath.Join(linkToDir, link1ˢ);
    err = os.Symlink(@file, link1);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string link2 = filepath.Join(linkToDir, link2ˢ);
    err = os.Symlink(link1, link2);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // /tmp may itself be a symlink!
    (var realTmpDir, err) = filepath.EvalSymlinks(tmpDir);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string realDir = filepath.Join(realTmpDir, dirˢ);
    @string realFile = filepath.Join(realDir, fileˢ);
    var tests = new TestIssue13582_tests[]{
        new(dir, realDir),
        new(linkToDir, realDir),
        new(@file, realFile),
        new(link1, realFile),
        new(link2, realFile)
    }.slice();
    foreach (var (i, test) in tests) {
        var (have, errΔ1) = filepath.EvalSymlinks(test.path);
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
        if (have != test.want) {
            Ꮡt.Errorf("test#%d: EvalSymlinks(%q) returns %q, want %q"u8, i, test.path, have, test.want);
        }
    }
}

// Issue 57905.
public static void TestRelativeSymlinkToAbsolute(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(Ꮡt));
    // Not parallel: uses t.Chdir.
    @string tmpDir = Ꮡt.TempDir();
    Ꮡt.Chdir(tmpDir);
    // Create "link" in the current working directory as a symlink to an arbitrary
    // absolute path. On macOS, this path is likely to begin with a symlink
    // itself: generally either in /var (symlinked to "private/var") or /tmp
    // (symlinked to "private/tmp").
    {
        var errΔ1 = os.Symlink(tmpDir, linkˢ); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    Ꮡt.Logf(@"os.Symlink(%q, ""link"")"u8, tmpDir);
    var (p, err) = filepath.EvalSymlinks(linkˢ);
    if (err != default!) {
        Ꮡt.Fatalf(@"EvalSymlinks(""link""): %v"u8, err);
    }
    (var want, err) = filepath.EvalSymlinks(tmpDir);
    if (err != default!) {
        Ꮡt.Fatalf(@"EvalSymlinks(%q): %v"u8, tmpDir, err);
    }
    if (p != want) {
        Ꮡt.Errorf(@"EvalSymlinks(""link"") = %q; want %q"u8, p, want);
    }
    Ꮡt.Logf(@"EvalSymlinks(""link"") = %q"u8, p);
}

// Test directories relative to temporary directory.
// The tests are run in absTestDirs[0].
internal static slice<@string> absTestDirs = new @string[]{
    "a"u8,
    "a/b"u8,
    "a/b/c"u8
}.slice();

// Test paths relative to temporary directory. $ expands to the directory.
// The tests are run in absTestDirs[0].
// We create absTestDirs first.
internal static slice<@string> absTests = new @string[]{
    "."u8,
    "b"u8,
    "b/"u8,
    "../a"u8,
    "../a/b"u8,
    "../a/b/./c/../../.././a"u8,
    "../a/b/./c/../../.././a/"u8,
    "$"u8,
    "$/."u8,
    "$/a/../a/b"u8,
    "$/a/b/c/../../.././a"u8,
    "$/a/b/c/../../.././a/"u8
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object mkdirFailedˢ = (@string)"Mkdir failed: "u8;
internal static readonly object chdirFailedˢ = (@string)"chdir failed: "u8;

public static void TestAbs(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string root = Ꮡt.TempDir();
    Ꮡt.Chdir(root);
    foreach (var (_, dir) in absTestDirs) {
        var errΔ1 = os.Mkdir(dir, 511);
        if (errΔ1 != default!) {
            Ꮡt.Fatal(mkdirFailedˢ, errΔ1);
        }
    }
    // Make sure the global absTests slice is not
    // modified by multiple invocations of TestAbs.
    var tests = absTests;
    if (runtime.GOOS == "windows"u8) {
        @string vol = filepath.VolumeName(root);
        slice<@string> extra = default!;
        foreach (var (_, vᴛ1) in absTests) {
            var path = vᴛ1;

            if (strings.Contains(path, "$"u8)) {
                continue;
            }
            path = vol + path;
            extra = append(extra, path);
        }
        tests = appendꓸꓸꓸ(slices.Clip<slice<@string>, @string>(tests), extra);
    }
    var err = os.Chdir(absTestDirs[0]);
    if (err != default!) {
        Ꮡt.Fatal(chdirFailedˢ, err);
    }
    foreach (var (_, vᴛ2) in tests) {
        var path = vᴛ2;

        path = strings.ReplaceAll(path, "$"u8, root);
        var (info, errΔ2) = os.Stat(path);
        if (errΔ2 != default!) {
            Ꮡt.Errorf("%s: %s"u8, path, errΔ2);
            continue;
        }
        (var abspath, errΔ2) = filepath.Abs(path);
        if (errΔ2 != default!) {
            Ꮡt.Errorf("Abs(%q) error: %v"u8, path, errΔ2);
            continue;
        }
        (var absinfo, errΔ2) = os.Stat(abspath);
        if (errΔ2 != default! || !os.SameFile(absinfo, info)) {
            Ꮡt.Errorf("Abs(%q)=%q, not the same file"u8, path, abspath);
        }
        if (!filepath.IsAbs(abspath)) {
            Ꮡt.Errorf("Abs(%q)=%q, not an absolute path"u8, path, abspath);
        }
        if (filepath.IsAbs(abspath) && abspath != filepath.Clean(abspath)) {
            Ꮡt.Errorf("Abs(%q)=%q, isn't clean"u8, path, abspath);
        }
    }
}

// Empty path needs to be special-cased on Windows. See golang.org/issue/24441.
// We test it separately from all other absTests because the empty string is not
// a valid path, so it can't be used with os.Stat.
public static void TestAbsEmptyString(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string root = Ꮡt.TempDir();
    Ꮡt.Chdir(root);
    var (info, err) = os.Stat(root);
    if (err != default!) {
        Ꮡt.Fatalf("%s: %s"u8, root, err);
    }
    (var abspath, err) = filepath.Abs(""u8);
    if (err != default!) {
        Ꮡt.Fatalf(@"Abs("""") error: %v"u8, err);
    }
    (var absinfo, err) = os.Stat(abspath);
    if (err != default! || !os.SameFile(absinfo, info)) {
        Ꮡt.Errorf(@"Abs("""")=%q, not the same file"u8, abspath);
    }
    if (!filepath.IsAbs(abspath)) {
        Ꮡt.Errorf(@"Abs("""")=%q, not an absolute path"u8, abspath);
    }
    if (filepath.IsAbs(abspath) && abspath != filepath.Clean(abspath)) {
        Ꮡt.Errorf(@"Abs("""")=%q, isn't clean"u8, abspath);
    }
}

partial struct RelTests {
    internal @string root, path, want;
}

// can't do purely lexically
internal static slice<RelTests> reltests = new RelTests[]{
    new("a/b"u8, "a/b"u8, "."u8),
    new("a/b/."u8, "a/b"u8, "."u8),
    new("a/b"u8, "a/b/."u8, "."u8),
    new("./a/b"u8, "a/b"u8, "."u8),
    new("a/b"u8, "./a/b"u8, "."u8),
    new("ab/cd"u8, "ab/cde"u8, "../cde"u8),
    new("ab/cd"u8, "ab/c"u8, "../c"u8),
    new("a/b"u8, "a/b/c/d"u8, "c/d"u8),
    new("a/b"u8, "a/b/../c"u8, "../c"u8),
    new("a/b/../c"u8, "a/b"u8, "../b"u8),
    new("a/b/c"u8, "a/c/d"u8, "../../c/d"u8),
    new("a/b"u8, "c/d"u8, "../../c/d"u8),
    new("a/b/c/d"u8, "a/b"u8, "../.."u8),
    new("a/b/c/d"u8, "a/b/"u8, "../.."u8),
    new("a/b/c/d/"u8, "a/b"u8, "../.."u8),
    new("a/b/c/d/"u8, "a/b/"u8, "../.."u8),
    new("../../a/b"u8, "../../a/b/c/d"u8, "c/d"u8),
    new("/a/b"u8, "/a/b"u8, "."u8),
    new("/a/b/."u8, "/a/b"u8, "."u8),
    new("/a/b"u8, "/a/b/."u8, "."u8),
    new("/ab/cd"u8, "/ab/cde"u8, "../cde"u8),
    new("/ab/cd"u8, "/ab/c"u8, "../c"u8),
    new("/a/b"u8, "/a/b/c/d"u8, "c/d"u8),
    new("/a/b"u8, "/a/b/../c"u8, "../c"u8),
    new("/a/b/../c"u8, "/a/b"u8, "../b"u8),
    new("/a/b/c"u8, "/a/c/d"u8, "../../c/d"u8),
    new("/a/b"u8, "/c/d"u8, "../../c/d"u8),
    new("/a/b/c/d"u8, "/a/b"u8, "../.."u8),
    new("/a/b/c/d"u8, "/a/b/"u8, "../.."u8),
    new("/a/b/c/d/"u8, "/a/b"u8, "../.."u8),
    new("/a/b/c/d/"u8, "/a/b/"u8, "../.."u8),
    new("/../../a/b"u8, "/../../a/b/c/d"u8, "c/d"u8),
    new("."u8, "a/b"u8, "a/b"u8),
    new("."u8, ".."u8, ".."u8),
    new(".."u8, "."u8, "err"u8),
    new(".."u8, "a"u8, "err"u8),
    new("../.."u8, ".."u8, "err"u8),
    new("a"u8, "/a"u8, "err"u8),
    new("/a"u8, "a"u8, "err"u8)
}.slice();

internal static slice<RelTests> winreltests = new RelTests[]{
    new(@"C:a\b\c"u8, @"C:a/b/d"u8, @"..\d"u8),
    new(@"C:\"u8, @"D:\"u8, @"err"u8),
    new(@"C:"u8, @"D:"u8, @"err"u8),
    new(@"C:\Projects"u8, @"c:\projects\src"u8, @"src"u8),
    new(@"C:\Projects"u8, @"c:\projects"u8, @"."u8),
    new(@"C:\Projects\a\.."u8, @"c:\projects"u8, @"."u8),
    new(@"\\host\share"u8, @"\\host\share\file.txt"u8, @"file.txt"u8)
}.slice();

public static void TestRel(ж<testing.T> Ꮡt) {
    var tests = appendꓸꓸꓸ(new RelTests[]{}.slice(), reltests);
    if (runtime.GOOS == "windows"u8) {
        foreach (var (i, _) in tests) {
            tests[i].want = filepath.FromSlash(tests[i].want);
        }
        tests = appendꓸꓸꓸ(tests, winreltests);
    }
    foreach (var (_, test) in tests) {
        var (got, err) = filepath.Rel(test.root, test.path);
        if (test.want == "err"u8) {
            if (err == default!) {
                Ꮡt.Errorf("Rel(%q, %q)=%q, want error"u8, test.root, test.path, got);
            }
            continue;
        }
        if (err != default!) {
            Ꮡt.Errorf("Rel(%q, %q): want %q, got error: %s"u8, test.root, test.path, test.want, err);
        }
        if (got != test.want) {
            Ꮡt.Errorf("Rel(%q, %q)=%q, want %q"u8, test.root, test.path, got, test.want);
        }
    }
}

partial struct VolumeNameTest {
    internal @string path;
    internal @string vol;
}

internal static slice<VolumeNameTest> volumenametests = new VolumeNameTest[]{
    new(@"c:/foo/bar"u8, @"c:"u8),
    new(@"c:"u8, @"c:"u8),
    new(@"c:\"u8, @"c:"u8),
    new(@"2:"u8, @"2:"u8),
    new(@""u8, @""u8),
    new(@"\\\host"u8, @"\\\host"u8),
    new(@"\\\host\"u8, @"\\\host"u8),
    new(@"\\\host\share"u8, @"\\\host"u8),
    new(@"\\\host\\share"u8, @"\\\host"u8),
    new(@"\\host"u8, @"\\host"u8),
    new(@"//host"u8, @"\\host"u8),
    new(@"\\host\"u8, @"\\host\"u8),
    new(@"//host/"u8, @"\\host\"u8),
    new(@"\\host\share"u8, @"\\host\share"u8),
    new(@"//host/share"u8, @"\\host\share"u8),
    new(@"\\host\share\"u8, @"\\host\share"u8),
    new(@"//host/share/"u8, @"\\host\share"u8),
    new(@"\\host\share\foo"u8, @"\\host\share"u8),
    new(@"//host/share/foo"u8, @"\\host\share"u8),
    new(@"\\host\share\\foo\\\bar\\\\baz"u8, @"\\host\share"u8),
    new(@"//host/share//foo///bar////baz"u8, @"\\host\share"u8),
    new(@"\\host\share\foo\..\bar"u8, @"\\host\share"u8),
    new(@"//host/share/foo/../bar"u8, @"\\host\share"u8),
    new(@"//."u8, @"\\."u8),
    new(@"//./"u8, @"\\.\"u8),
    new(@"//./NUL"u8, @"\\.\NUL"u8),
    new(@"//?"u8, @"\\?"u8),
    new(@"//?/"u8, @"\\?\"u8),
    new(@"//?/NUL"u8, @"\\?\NUL"u8),
    new(@"/??"u8, @"\??"u8),
    new(@"/??/"u8, @"\??\"u8),
    new(@"/??/NUL"u8, @"\??\NUL"u8),
    new(@"//./a/b"u8, @"\\.\a"u8),
    new(@"//./C:"u8, @"\\.\C:"u8),
    new(@"//./C:/"u8, @"\\.\C:"u8),
    new(@"//./C:/a/b/c"u8, @"\\.\C:"u8),
    new(@"//./UNC/host/share/a/b/c"u8, @"\\.\UNC\host\share"u8),
    new(@"//./UNC/host"u8, @"\\.\UNC\host"u8),
    new(@"//./UNC/host\"u8, @"\\.\UNC\host\"u8),
    new(@"//./UNC"u8, @"\\.\UNC"u8),
    new(@"//./UNC/"u8, @"\\.\UNC\"u8),
    new(@"\\?\x"u8, @"\\?\x"u8),
    new(@"\??\x"u8, @"\??\x"u8)
}.slice();

public static void TestVolumeName(ж<testing.T> Ꮡt) {
    if (runtime.GOOS != "windows"u8) {
        return;
    }
    foreach (var (_, v) in volumenametests) {
        {
            @string vol = filepath.VolumeName(v.path); if (vol != v.vol) {
                Ꮡt.Errorf("VolumeName(%q)=%q, want %q"u8, v.path, vol, v.vol);
            }
        }
    }
}

public static void TestDriveLetterInEvalSymlinks(ж<testing.T> Ꮡt) {
    if (runtime.GOOS != "windows"u8) {
        return;
    }
    var (wd, _) = os.Getwd();
    if (len(wd) < 3) {
        Ꮡt.Errorf("Current directory path %q is too short"u8, wd);
    }
    @string lp = strings.ToLower(wd);
    @string up = strings.ToUpper(wd);
    var (flp, err) = filepath.EvalSymlinks(lp);
    if (err != default!) {
        Ꮡt.Fatalf("EvalSymlinks(%q) failed: %q"u8, lp, err);
    }
    (var fup, err) = filepath.EvalSymlinks(up);
    if (err != default!) {
        Ꮡt.Fatalf("EvalSymlinks(%q) failed: %q"u8, up, err);
    }
    if (flp != fup) {
        Ꮡt.Errorf("Results of EvalSymlinks do not match: %q and %q"u8, flp, fup);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string srcˢ = "src"u8;
internal static readonly @string unicodeˢ = "unicode"u8;
internal static readonly @string utf16ˢ = "utf16"u8;
internal static readonly @string utf8ˢ = "utf8"u8;
internal static readonly object filepathWalkOutOfOrderˢ = (@string)"filepath.Walk out of order - utf8 before utf16"u8;

public static void TestBug3486(ж<testing.T> Ꮡt) {
    // https://golang.org/issue/3486
    if (runtime.GOOS == "ios"u8) {
        Ꮡt.Skipf("skipping on %s/%s"u8, runtime.GOOS, runtime.GOARCH);
    }
    @string root = filepath.Join(testenv.GOROOT(new filepath_test_package.testing_TжTB(Ꮡt)), srcˢ, unicodeˢ);
    @string utf16 = filepath.Join(root, utf16ˢ);
    @string utf8 = filepath.Join(root, utf8ˢ);
    var seenUTF16 = false;
    var seenUTF8 = false;
    var err = filepath.Walk(root, error (@string pth, fs.FileInfo info, error errΔ1) => {
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
        var exprᴛ1 = pth;
        if (exprᴛ1 == utf16) {
            seenUTF16 = true;
            return filepath.SkipDir;
        }
        if (exprᴛ1 == utf8) {
            if (!seenUTF16) {
                Ꮡt.Fatal(filepathWalkOutOfOrderˢ);
            }
            seenUTF8 = true;
        }

        return default!;
    });
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!seenUTF8) {
        Ꮡt.Fatalf("%q not seen"u8, utf8);
    }
}

internal static void testWalkSymlink(ж<testing.T> Ꮡt, Func<@string, @string, error> mklink) {
    @string tmpdir = Ꮡt.TempDir();
    Ꮡt.Chdir(tmpdir);
    var err = mklink(tmpdir, linkˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var visited = ref heap<slice<@string>>(out var Ꮡvisited);
    err = filepath.Walk(tmpdir, (@string path, fs.FileInfo info, error errΔ1) => {
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
        (var rel, errΔ1) = filepath.Rel(tmpdir, path);
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
        Ꮡvisited.ValueSlot = append(Ꮡvisited.ValueSlot, rel);
        return default!;
    });
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    slices.Sort<slice<@string>, @string>(visited);
    var want = new @string[]{"."u8, "link"u8}.slice();
    if (fmt.Sprintf("%q"u8, visited) != fmt.Sprintf("%q"u8, want)) {
        Ꮡt.Errorf("unexpected paths visited %q, want %q"u8, visited, want);
    }
}

public static void TestWalkSymlink(ж<testing.T> Ꮡt) {
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(Ꮡt));
    testWalkSymlink(Ꮡt, os.Symlink);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fileTxtˢ = "file.txt"u8;

public static void TestIssue29372(ж<testing.T> Ꮡt) {
    @string tmpDir = Ꮡt.TempDir();
    @string path = filepath.Join(tmpDir, fileTxtˢ);
    var err = os.WriteFile(path, default!, 420);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string pathSeparator = ((@string)(rune)filepath.Separator);
    var tests = new @string[]{
        path + strings.Repeat(pathSeparator, 1),
        path + strings.Repeat(pathSeparator, 2),
        path + strings.Repeat(pathSeparator, 1) + "."u8,
        path + strings.Repeat(pathSeparator, 2) + "."u8,
        path + strings.Repeat(pathSeparator, 1) + ".."u8,
        path + strings.Repeat(pathSeparator, 2) + ".."u8
    }.slice();
    foreach (var (i, test) in tests) {
        (_, err) = filepath.EvalSymlinks(test);
        if (!AreEqual(err, syscall.ENOTDIR)) {
            Ꮡt.Fatalf("test#%d: want %q, got %q"u8, i, syscall.ENOTDIR, err);
        }
    }
}

// Issue 30520 part 1.
public static void TestEvalSymlinksAboveRoot(ж<testing.T> Ꮡt) {
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(Ꮡt));
    Ꮡt.Parallel();
    @string tmpDir = Ꮡt.TempDir();
    var (evalTmpDir, err) = filepath.EvalSymlinks(tmpDir);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var errΔ1 = os.Mkdir(filepath.Join(evalTmpDir, "a"), 511); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    {
        var errΔ2 = os.Symlink(filepath.Join(evalTmpDir, "a"), filepath.Join(evalTmpDir, "b")); if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
    }
    {
        var errΔ3 = os.WriteFile(filepath.Join(evalTmpDir, "a", fileˢ), default!, 438); if (errΔ3 != default!) {
            Ꮡt.Fatal(errΔ3);
        }
    }
    // Count the number of ".." elements to get to the root directory.
    @string vol = filepath.VolumeName(evalTmpDir);
    nint c = strings.Count(evalTmpDir.slice(len(vol)), ((@string)(rune)os.PathSeparator));
    slice<@string> dd = default!;
    for (nint i = 0; i < c + 2; i++) {
        dd = append(dd, ".."u8);
    }
    @string wantSuffix = strings.Join(new @string[]{"a"u8, "file"u8}.slice(), ((@string)(rune)os.PathSeparator));
    // Try different numbers of "..".
    foreach (var (_, i) in new nint[]{c, c + 1, c + 2}.slice()) {
        @string check = strings.Join(new @string[]{evalTmpDir, strings.Join(dd.slice(0, i), ((@string)(rune)os.PathSeparator)), evalTmpDir.slice(len(vol) + 1), "b"u8, "file"u8}.slice(), ((@string)(rune)os.PathSeparator));
        var (resolved, errΔ4) = filepath.EvalSymlinks(check);
        switch (ᐧ) {
        case {} when runtime.GOOS == "darwin"u8 && errors.Is(errΔ4, fs.ErrNotExist): {
            testenv.SkipFlaky(new filepath_test_package.testing_TжTB(Ꮡt), // On darwin, the temp dir is sometimes cleaned up mid-test (issue 37910).
 37910);
            break;
        }
        case {} when errΔ4 != default!: {
            Ꮡt.Errorf("EvalSymlinks(%q) failed: %v"u8, check, errΔ4);
            break;
        }
        case {} when !strings.HasSuffix(resolved, wantSuffix): {
            Ꮡt.Errorf("EvalSymlinks(%q) = %q does not end with %q"u8, check, resolved, wantSuffix);
            break;
        }
        default: {
            Ꮡt.Logf("EvalSymlinks(%q) = %q"u8, check, resolved);
            break;
        }}

    }
}

// Issue 30520 part 2.
public static void TestEvalSymlinksAboveRootChdir(ж<testing.T> Ꮡt) {
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(Ꮡt));
    Ꮡt.Chdir(Ꮡt.TempDir());
    @string subdir = filepath.Join("a"u8, "b");
    {
        var err = os.MkdirAll(subdir, 511); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        var err = os.Symlink(subdir, "c"u8); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        var err = os.WriteFile(filepath.Join(subdir, fileˢ), default!, 438); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    subdir = filepath.Join("d"u8, "e", "f");
    {
        var err = os.MkdirAll(subdir, 511); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        var err = os.Chdir(subdir); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    @string check = filepath.Join(".."u8, "..", "..", "c", fileˢ);
    @string wantSuffix = filepath.Join("a"u8, "b", fileˢ);
    {
        var (resolved, err) = filepath.EvalSymlinks(check); if (err != default!){
            Ꮡt.Errorf("EvalSymlinks(%q) failed: %v"u8, check, err);
        } else 
        if (!strings.HasSuffix(resolved, wantSuffix)){
            Ꮡt.Errorf("EvalSymlinks(%q) = %q does not end with %q"u8, check, resolved, wantSuffix);
        } else {
            Ꮡt.Logf("EvalSymlinks(%q) = %q"u8, check, resolved);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badˢ = "bad"u8;
internal static readonly @string nextˢ = "next"u8;

public static void TestIssue51617(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string dir = Ꮡt.TempDir();
        foreach (var (_, sub) in new @string[]{"a"u8, filepath.Join("a"u8, badˢ), filepath.Join("a"u8, nextˢ)}.slice()) {
            {
                var errΔ1 = os.Mkdir(filepath.Join(dir, sub), 493); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
        }
        @string bad = filepath.Join(dir, "a", badˢ);
        {
            var errΔ2 = os.Chmod(bad, 0); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        defer(os.Chmod, bad, (fs.FileMode)(448), ref ᒐ); // avoid errors on cleanup
        ref var saw = ref heap<slice<@string>>(out var Ꮡsaw);
        var err = filepath.WalkDir(dir, error (@string path, fs.DirEntry d, error errΔ3) => {
            if (errΔ3 != default!) {
                return filepath.SkipDir;
            }
            if (d.IsDir()) {
                var (rel, errΔ4) = filepath.Rel(dir, path);
                if (errΔ4 != default!) {
                    Ꮡt.Fatal(errΔ4);
                }
                Ꮡsaw.ValueSlot = append(Ꮡsaw.ValueSlot, rel);
            }
            return default!;
        });
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var want = new @string[]{"."u8, "a"u8, filepath.Join("a"u8, badˢ), filepath.Join("a"u8, nextˢ)}.slice();
        if (!slices.Equal<slice<@string>, @string>(saw, want)) {
            Ꮡt.Errorf("got directories %v, want %v"u8, saw, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestEscaping(ж<testing.T> Ꮡt) {
    @string dir = Ꮡt.TempDir();
    Ꮡt.Chdir(Ꮡt.TempDir());
    foreach (var (_, p) in new @string[]{
        filepath.Join(dir, "x")
    }.slice()) {
        if (!filepath.IsLocal(p)) {
            continue;
        }
        var (f, err) = os.Create(p);
        if (err != default!) {
            f.Close();
        }
        (var ents, err) = os.ReadDir(dir);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        foreach (var (_, e) in ents) {
            Ꮡt.Fatalf("found: %v"u8, e.Name());
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorGotNilˢ = (@string)"expected error, got nil"u8;

public static void TestEvalSymlinksTooManyLinks(ж<testing.T> Ꮡt) {
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(Ꮡt));
    @string dir = filepath.Join(Ꮡt.TempDir(), dirˢ);
    var err = os.Symlink(dir, dir);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (_, err) = filepath.EvalSymlinks(dir);
    if (err == default!) {
        Ꮡt.Fatal(expectedErrorGotNilˢ);
    }
}

} // end filepath_test_package
