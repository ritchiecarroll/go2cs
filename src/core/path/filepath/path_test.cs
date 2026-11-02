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
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.path;
using io;
using static go.path.filepath_internal_test_package;

partial class filepath_test_package {

partial struct PathTest {
    internal @string path, result;
}

internal static slice<PathTest> cleantests = slice<PathTest>([
    // Already clean
    new("abc", "abc"),
    new("abc/def", "abc/def"),
    new("a/b/c", "a/b/c"),
    new(".", "."),
    new("..", ".."),
    new("../..", "../.."),
    new("../../abc", "../../abc"),
    new("/abc", "/abc"),
    new("/", "/"),

    // Empty is current dir
    new("", "."),

    // Remove trailing slash
    new("abc/", "abc"),
    new("abc/def/", "abc/def"),
    new("a/b/c/", "a/b/c"),
    new("./", "."),
    new("../", ".."),
    new("../../", "../.."),
    new("/abc/", "/abc"),

    // Remove doubled slash
    new("abc//def//ghi", "abc/def/ghi"),
    new("abc//", "abc"),

    // Remove . elements
    new("abc/./def", "abc/def"),
    new("/./abc/def", "/abc/def"),
    new("abc/.", "abc"),

    // Remove .. elements
    new("abc/def/ghi/../jkl", "abc/def/jkl"),
    new("abc/def/../ghi/../jkl", "abc/jkl"),
    new("abc/def/..", "abc"),
    new("abc/def/../..", "."),
    new("/abc/def/../..", "/"),
    new("abc/def/../../..", ".."),
    new("/abc/def/../../..", "/"),
    new("abc/def/../../../ghi/jkl/../../../mno", "../../mno"),
    new("/../abc", "/abc"),
    new("a/../b:/../../c", @"../c"),

    // Combinations
    new("abc/./../def", "def"),
    new("abc//./../def", "def"),
    new("abc/../../././../def", "../../def")
]);

internal static slice<PathTest> nonwincleantests = slice<PathTest>([
    // Remove leading doubled slash
    new("//abc", "/abc"),
    new("///abc", "/abc"),
    new("//abc//", "/abc")
]);

internal static slice<PathTest> wincleantests = slice<PathTest>([
    new(@"c:", @"c:."),
    new(@"c:\", @"c:\"),
    new(@"c:\abc", @"c:\abc"),
    new(@"c:abc\..\..\.\.\..\def", @"c:..\..\def"),
    new(@"c:\abc\def\..\..", @"c:\"),
    new(@"c:\..\abc", @"c:\abc"),
    new(@"c:..\abc", @"c:..\abc"),
    new(@"c:\b:\..\..\..\d", @"c:\d"),
    new(@"\", @"\"),
    new(@"/", @"\"),
    new(@"\\i\..\c$", @"\\i\..\c$"),
    new(@"\\i\..\i\c$", @"\\i\..\i\c$"),
    new(@"\\i\..\I\c$", @"\\i\..\I\c$"),
    new(@"\\host\share\foo\..\bar", @"\\host\share\bar"),
    new(@"//host/share/foo/../baz", @"\\host\share\baz"),
    new(@"\\host\share\foo\..\..\..\..\bar", @"\\host\share\bar"),
    new(@"\\.\C:\a\..\..\..\..\bar", @"\\.\C:\bar"),
    new(@"\\.\C:\\\\a", @"\\.\C:\a"),
    new(@"\\a\b\..\c", @"\\a\b\c"),
    new(@"\\a\b", @"\\a\b"),
    new(@".\c:", @".\c:"),
    new(@".\c:\foo", @".\c:\foo"),
    new(@".\c:foo", @".\c:foo"),
    new(@"//abc", @"\\abc"),
    new(@"///abc", @"\\\abc"),
    new(@"//abc//", @"\\abc\\"),
    new(@"\\?\C:\", @"\\?\C:\"),
    new(@"\\?\C:\a", @"\\?\C:\a"),

    // Don't allow cleaning to move an element with a colon to the start of the path.
    new(@"a/../c:", @".\c:"),
    new(@"a\..\c:", @".\c:"),
    new(@"a/../c:/a", @".\c:\a"),
    new(@"a/../../c:", @"..\c:"),
    new(@"foo:bar", @"foo:bar"),

    // Don't allow cleaning to create a Root Local Device path like \??\a.
    new(@"/a/../??/a", @"\.\??\a")
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingMallocCountInˢ = (@string)"skipping malloc count in short mode"u8;
internal static readonly object skippingAllocsPerRunˢ = (@string)"skipping AllocsPerRun checks; GOMAXPROCS>1"u8;

public static void TestClean(ж<testing.T> t) {
    var tests = cleantests;
    if (runtime.GOOS == "windows"u8) {
        foreach (var i in range(tests)) {
            tests[i].result = filepath.FromSlash(tests[i].result);
        }
        tests = appendꓸꓸꓸ(tests, wincleantests);
    } else {
        tests = appendꓸꓸꓸ(tests, nonwincleantests);
    }
    foreach (var (_, test) in tests) {
        {
            @string s = filepath.Clean(test.path); if (s != test.result) {
                t.Errorf("Clean(%q) = %q, want %q", test.path, s, test.result);
            }
        }
        {
            @string s = filepath.Clean(test.result); if (s != test.result) {
                t.Errorf("Clean(%q) = %q, want %q", test.result, s, test.result);
            }
        }
    }

    if (testing.Short()) {
        t.Skip(skippingMallocCountInˢ);
    }
    if (runtime.GOMAXPROCS(0) > 1) {
        t.Log(skippingAllocsPerRunˢ);
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
            t.Errorf("Clean(%q): %v allocs, want zero", test.result, allocs);
        }
    }
}

partial struct IsLocalTest {
    internal @string path;
    internal bool isLocal;
}

internal static slice<IsLocalTest> islocaltests = slice<IsLocalTest>([
    new("", false),
    new(".", true),
    new("..", false),
    new("../a", false),
    new("/", false),
    new("/a", false),
    new("/a/../..", false),
    new("a", true),
    new("a/../a", true),
    new("a/", true),
    new("a/.", true),
    new("a/./b/./c", true),
    new(@"a/../b:/../../c", false)
]);

internal static slice<IsLocalTest> winislocaltests = slice<IsLocalTest>([
    new("NUL", false),
    new("nul", false),
    new("nul ", false),
    new("nul.", false),
    new("a/nul:", false),
    new("a/nul : a", false),
    new("com0", true),
    new("com1", false),
    new("com2", false),
    new("com3", false),
    new("com4", false),
    new("com5", false),
    new("com6", false),
    new("com7", false),
    new("com8", false),
    new("com9", false),
    new("com¹", false),
    new("com²", false),
    new("com³", false),
    new("com¹ : a", false),
    new("cOm1", false),
    new("lpt1", false),
    new("LPT1", false),
    new("lpt³", false),
    new("./nul", false),
    new(@"\", false),
    new(@"\a", false),
    new(@"C:", false),
    new(@"C:\a", false),
    new(@"..\a", false),
    new(@"a/../c:", false),
    new(@"CONIN$", false),
    new(@"conin$", false),
    new(@"CONOUT$", false),
    new(@"conout$", false),
    new(@"dollar$", true) // not a special file name
]);

internal static slice<IsLocalTest> plan9islocaltests = slice<IsLocalTest>([
    new("#a", false)
]);

public static void TestIsLocal(ж<testing.T> t) {
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
                t.Errorf("IsLocal(%q) = %v, want %v", test.path, got, test.isLocal);
            }
        }
    }
}

partial struct LocalizeTest {
    internal @string path;
    internal @string want;
}

internal static slice<LocalizeTest> localizetests = slice<LocalizeTest>([
    new("", ""),
    new(".", "."),
    new("..", ""),
    new("a/..", ""),
    new("/", ""),
    new("/a", ""),
    new(((@string)(new byte[]{0x61, 0xff, 0x62})), ""),
    new("a/", ""),
    new("a/./b", ""),
    new("\x00", ""),
    new("a", "a"),
    new("a/b/c", "a/b/c")
]);

internal static slice<LocalizeTest> plan9localizetests = slice<LocalizeTest>([
    new("#a", ""),
    new(@"a\b:c", @"a\b:c")
]);

internal static slice<LocalizeTest> unixlocalizetests = slice<LocalizeTest>([
    new("#a", "#a"),
    new(@"a\b:c", @"a\b:c")
]);

internal static slice<LocalizeTest> winlocalizetests = slice<LocalizeTest>([
    new("#a", "#a"),
    new("c:", ""),
    new(@"a\b", ""),
    new(@"a:b", ""),
    new(@"a/b:c", ""),
    new(@"NUL", ""),
    new(@"a/NUL", ""),
    new(@"./com1", ""),
    new(@"a/nul/b", "")
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string errorˢ => ref errorˢᵇ;
private static readonly @string errorˢᵇ = "error"u8;

public static void TestLocalize(ж<testing.T> t) {
    var tests = localizetests;
    var exprᴛ1 = runtime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "plan9"u8: {
        tests = appendꓸꓸꓸ(tests, plan9localizetests);
        break;
    }
    case {} when exprᴛ1 == "windows"u8: {
        tests = appendꓸꓸꓸ(tests, winlocalizetests);
        foreach (var i in range(tests)) {
            tests[i].want = filepath.FromSlash(tests[i].want);
        }
        break;
    }
    default: {
        tests = appendꓸꓸꓸ(tests, unixlocalizetests);
        break;
    }}
    foreach (var (_, test) in tests) {
        var (got, err) = filepath.Localize(test.path);
        @string wantErr = nilˢ;
        if (test.want == ""u8) {
            wantErr = errorˢ;
        }
        if (got != test.want || ((err == nil) != (test.want != ""u8))) {
            t.Errorf("IsLocal(%q) = %q, %v want %q, %v", test.path, got, err, test.want, wantErr);
        }
    }
}

internal static UntypedInt sep => /* filepath.Separator */ 92;

internal static slice<PathTest> slashtests = slice<PathTest>([
    new("", ""),
    new("/", (@string)(rune)sep),
    new("/a/b", (@string)slice<byte>([sep, (byte)'a', sep, (byte)'b'])),
    new("a//b", (@string)slice<byte>([(byte)'a', sep, sep, (byte)'b']))
]);

public static void TestFromAndToSlash(ж<testing.T> t) {
    foreach (var (_, test) in slashtests) {
        {
            @string s = filepath.FromSlash(test.path); if (s != test.result) {
                t.Errorf("FromSlash(%q) = %q, want %q", test.path, s, test.result);
            }
        }
        {
            @string s = filepath.ToSlash(test.result); if (s != test.path) {
                t.Errorf("ToSlash(%q) = %q, want %q", test.result, s, test.path);
            }
        }
    }
}

partial struct SplitListTest {
    internal @string list;
    internal slice<@string> result;
}

internal static UntypedInt lsep => /* filepath.ListSeparator */ 59;

internal static slice<SplitListTest> splitlisttests = slice<SplitListTest>([
    new("", slice<@string>([])),
    new((@string)slice<byte>([(byte)'a', lsep, (byte)'b']), slice<@string>(["a", "b"])),
    new((@string)slice<byte>([lsep, (byte)'a', lsep, (byte)'b']), slice<@string>(["", "a", "b"]))
]);

internal static slice<SplitListTest> winsplitlisttests = slice<SplitListTest>([
    // quoted
    new(@"""a""", slice<@string>([@"a"])),

    // semicolon
    new(@""";""", slice<@string>([@";"])),
    new(@"""a;b""", slice<@string>([@"a;b"])),
    new(@""";"";", slice<@string>([@";", @""])),
    new(@";"";""", slice<@string>([@"", @";"])),

    // partially quoted
    new(@"a"";""b", slice<@string>([@"a;b"])),
    new(@"a; """"b", slice<@string>([@"a", @" b"])),
    new(@"""a;b", slice<@string>([@"a;b"])),
    new(@"""""a;b", slice<@string>([@"a", @"b"])),
    new(@"""""""a;b", slice<@string>([@"a;b"])),
    new(@"""""""""a;b", slice<@string>([@"a", @"b"])),
    new(@"a"";b", slice<@string>([@"a;b"])),
    new(@"a;b"";c", slice<@string>([@"a", @"b;c"])),
    new(@"""a"";b"";c", slice<@string>([@"a", @"b;c"]))
]);

public static void TestSplitList(ж<testing.T> t) {
    var tests = splitlisttests;
    if (runtime.GOOS == "windows"u8) {
        tests = appendꓸꓸꓸ(tests, winsplitlisttests);
    }
    foreach (var (_, test) in tests) {
        {
            var l = filepath.SplitList(test.list); if (!slices.Equal<slice<@string>, @string>(l, test.result)) {
                t.Errorf("SplitList(%#q) = %#q, want %#q", test.list, l, test.result);
            }
        }
    }
}

partial struct SplitTest {
    internal @string path, dir, @file;
}

internal static slice<SplitTest> unixsplittests = slice<SplitTest>([
    new("a/b", "a/", "b"),
    new("a/b/", "a/b/", ""),
    new("a/", "a/", ""),
    new("a", "", "a"),
    new("/", "/", "")
]);

internal static slice<SplitTest> winsplittests = slice<SplitTest>([
    new(@"c:", @"c:", @""),
    new(@"c:/", @"c:/", @""),
    new(@"c:/foo", @"c:/", @"foo"),
    new(@"c:/foo/bar", @"c:/foo/", @"bar"),
    new(@"//host/share", @"//host/share", @""),
    new(@"//host/share/", @"//host/share/", @""),
    new(@"//host/share/foo", @"//host/share/", @"foo"),
    new(@"\\host\share", @"\\host\share", @""),
    new(@"\\host\share\", @"\\host\share\", @""),
    new(@"\\host\share\foo", @"\\host\share\", @"foo")
]);

public static void TestSplit(ж<testing.T> t) {
    slice<SplitTest> splittests = default;
    splittests = unixsplittests;
    if (runtime.GOOS == "windows"u8) {
        splittests = appendꓸꓸꓸ(splittests, winsplittests);
    }
    foreach (var (_, test) in splittests) {
        {
            var (d, f) = filepath.Split(test.path); if (d != test.dir || f != test.@file) {
                t.Errorf("Split(%q) = %q, %q, want %q, %q", test.path, d, f, test.dir, test.@file);
            }
        }
    }
}

partial struct JoinTest {
    internal slice<@string> elem;
    internal @string path;
}

internal static slice<JoinTest> jointests = slice<JoinTest>([
    // zero parameters
    new(slice<@string>([]), ""),

    // one parameter
    new(slice<@string>([""]), ""),
    new(slice<@string>(["/"]), "/"),
    new(slice<@string>(["a"]), "a"),

    // two parameters
    new(slice<@string>(["a", "b"]), "a/b"),
    new(slice<@string>(["a", ""]), "a"),
    new(slice<@string>(["", "b"]), "b"),
    new(slice<@string>(["/", "a"]), "/a"),
    new(slice<@string>(["/", "a/b"]), "/a/b"),
    new(slice<@string>(["/", ""]), "/"),
    new(slice<@string>(["/a", "b"]), "/a/b"),
    new(slice<@string>(["a", "/b"]), "a/b"),
    new(slice<@string>(["/a", "/b"]), "/a/b"),
    new(slice<@string>(["a/", "b"]), "a/b"),
    new(slice<@string>(["a/", ""]), "a"),
    new(slice<@string>(["", ""]), ""),

    // three parameters
    new(slice<@string>(["/", "a", "b"]), "/a/b")
]);

internal static slice<JoinTest> nonwinjointests = slice<JoinTest>([
    new(slice<@string>(["//", "a"]), "/a")
]);

internal static slice<JoinTest> winjointests = slice<JoinTest>([
    new(slice<@string>([@"directory", @"file"]), @"directory\file"),
    new(slice<@string>([@"C:\Windows\", @"System32"]), @"C:\Windows\System32"),
    new(slice<@string>([@"C:\Windows\", @""]), @"C:\Windows"),
    new(slice<@string>([@"C:\", @"Windows"]), @"C:\Windows"),
    new(slice<@string>([@"C:", @"a"]), @"C:a"),
    new(slice<@string>([@"C:", @"a\b"]), @"C:a\b"),
    new(slice<@string>([@"C:", @"a", @"b"]), @"C:a\b"),
    new(slice<@string>([@"C:", @"", @"b"]), @"C:b"),
    new(slice<@string>([@"C:", @"", @"", @"b"]), @"C:b"),
    new(slice<@string>([@"C:", @""]), @"C:."),
    new(slice<@string>([@"C:", @"", @""]), @"C:."),
    new(slice<@string>([@"C:", @"\a"]), @"C:\a"),
    new(slice<@string>([@"C:", @"", @"\a"]), @"C:\a"),
    new(slice<@string>([@"C:.", @"a"]), @"C:a"),
    new(slice<@string>([@"C:a", @"b"]), @"C:a\b"),
    new(slice<@string>([@"C:a", @"b", @"d"]), @"C:a\b\d"),
    new(slice<@string>([@"\\host\share", @"foo"]), @"\\host\share\foo"),
    new(slice<@string>([@"\\host\share\foo"]), @"\\host\share\foo"),
    new(slice<@string>([@"//host/share", @"foo/bar"]), @"\\host\share\foo\bar"),
    new(slice<@string>([@"\"]), @"\"),
    new(slice<@string>([@"\", @""]), @"\"),
    new(slice<@string>([@"\", @"a"]), @"\a"),
    new(slice<@string>([@"\\", @"a"]), @"\\a"),
    new(slice<@string>([@"\", @"a", @"b"]), @"\a\b"),
    new(slice<@string>([@"\\", @"a", @"b"]), @"\\a\b"),
    new(slice<@string>([@"\", @"\\a\b", @"c"]), @"\a\b\c"),
    new(slice<@string>([@"\\a", @"b", @"c"]), @"\\a\b\c"),
    new(slice<@string>([@"\\a\", @"b", @"c"]), @"\\a\b\c"),
    new(slice<@string>([@"//", @"a"]), @"\\a"),
    new(slice<@string>([@"a:\b\c", @"x\..\y:\..\..\z"]), @"a:\b\z"),
    new(slice<@string>([@"\", @"??\a"]), @"\.\??\a")
]);

public static void TestJoin(ж<testing.T> t) {
    if (runtime.GOOS == "windows"u8) {
        jointests = appendꓸꓸꓸ(jointests, winjointests);
    } else {
        jointests = appendꓸꓸꓸ(jointests, nonwinjointests);
    }
    foreach (var (_, test) in jointests) {
        @string expected = filepath.FromSlash(test.path);
        {
            @string p = filepath.Join(test.elem.ꓸꓸꓸ); if (p != expected) {
                t.Errorf("join(%q) = %q, want %q", test.elem, p, expected);
            }
        }
    }
}

partial struct ExtTest {
    internal @string path, ext;
}

internal static slice<ExtTest> exttests = slice<ExtTest>([
    new("path.go", ".go"),
    new("path.pb.go", ".go"),
    new("a.dir/b", ""),
    new("a.dir/b.go", ".go"),
    new("a.dir/", "")
]);

public static void TestExt(ж<testing.T> t) {
    foreach (var (_, test) in exttests) {
        {
            @string x = filepath.Ext(test.path); if (x != test.ext) {
                t.Errorf("Ext(%q) = %q, want %q", test.path, x, test.ext);
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
    "testdata",
    slice<ж<Node>>([
        Ꮡ(new Node("a", default, 0)),
        Ꮡ(new Node("b", slice<ж<Node>>([]), 0)),
        Ꮡ(new Node("c", default, 0)),
        Ꮡ(new Node(
            "d",
            slice<ж<Node>>([
                Ꮡ(new Node("x", default, 0)),
                Ꮡ(new Node("y", slice<ж<Node>>([]), 0)),
                Ꮡ(new Node(
                    "z",
                    slice<ж<Node>>([
                        Ꮡ(new Node("u", default, 0)),
                        Ꮡ(new Node("v", default, 0))
                    ]),
                    0))
            ]),
            0))
    ]),
    0
));

internal static void walkTree(ж<Node> Ꮡn, @string path, Action<@string, ж<Node>> f) {
    ref var n = ref Ꮡn.DerefOrNull();

    f(path, Ꮡn);
    foreach (var (_, e) in n.entries) {
        walkTree(e, filepath.Join(path, e.name), f);
    }
}

internal static void makeTree(ж<testing.T> t) {
    walkTree(tree, tree.name, (@string path, ж<Node> n) => {
        if (n.entries == nil) {
            var (fd, err) = os.Create(path);
            if (err != nil) {
                t.Errorf("makeTree: %v", err);
                return;
            }
            fd.Close();
        } else {
            os.Mkdir(path, 504);
        }
    });
}

internal static void markTree(ж<Node> n) {
    walkTree(n, ""u8, (@string path, ж<Node> nΔ1) => {
        nΔ1.mark++;
    });
}

internal static void checkMarks(ж<testing.T> t, bool report) {
    walkTree(tree, tree.name, (@string path, ж<Node> n) => {
        if (n.mark != 1 && report) {
            t.Errorf("node %s mark = %d; expected 1", path, n.mark);
        }
        n.mark = 0;
    });
}

// Assumes that each node name is unique. Good enough for a test.
// If clear is true, any incoming error is cleared before return. The errors
// are always accumulated, though.
internal static error mark(fs.DirEntry d, error err, ж<slice<error>> Ꮡerrors, bool clear) {
    ref var errors = ref Ꮡerrors.DerefOrNull();

    @string name = d.Name();
    walkTree(tree, tree.name, (@string path, ж<Node> n) => {
        if (n.name == name) {
            n.mark++;
        }
    });
    if (err != nil) {
        errors = append(errors, err);
        if (clear) {
            return default;
        }
        return err;
    }
    return default;
}

// tempDirCanonical returns a temporary directory for the test to use, ensuring
// that the returned path does not contain symlinks.
internal static @string tempDirCanonical(ж<testing.T> t) {
    @string dir = t.TempDir();

    var (cdir, err) = filepath.EvalSymlinks(dir);
    if (err != nil) {
        t.Errorf("tempDirCanonical: %v", err);
    }

    return cdir;
}

public static void TestWalk(ж<testing.T> t) {
    var walk = (@string root, Func<@string, fs.DirEntry, error, error> fn) => filepath.Walk(root, (@string path, fs.FileInfo info, error err) => fn(path, fs.FileInfoToDirEntry(info), err));
    testWalk(t, walk, 1);
}

public static void TestWalkDir(ж<testing.T> t) {
    testWalk(t, filepath.WalkDir, 2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string permErrˢ => ref permErrˢᵇ;
private static readonly @string permErrˢᵇ = "PermErr"u8;
internal static readonly object skippingAsRootˢ = (@string)"skipping as root"u8;
internal static readonly object skippingInShortModeˢ = (@string)"skipping in short mode"u8;

internal static void testWalk(ж<testing.T> t, Func<@string, Func<@string, fs.DirEntry, error, error>, error> walk, nint errVisit) {
    t.Chdir(t.TempDir());

    makeTree(t);
    ref var errors = ref heap<slice<error>>(new slice<error>(0, 10), out var Ꮡerrors);
    var clear = true;
    var markFn = (@string path, fs.DirEntry d, error errΔ1) => mark(d, errΔ1, Ꮡerrors, clear);
    // Expect no errors.
    var err = walk(tree.name, new Func<@string, fs.DirEntry, error, error>(markFn));
    if (err != nil) {
        t.Fatalf("no error expected, found: %s", err);
    }
    if (len(errors) != 0) {
        t.Fatalf("unexpected errors: %s", errors);
    }
    checkMarks(t, true);
    errors = errors[0..0];

    var markFnʗ1 = markFn;
    t.Run(permErrˢ, (ж<testing.T> tΔ1) => {
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
        os.Chmod(filepath.Join(tree.name, tree.entries[1].name), 0);
        os.Chmod(filepath.Join(tree.name, tree.entries[3].name), 0);

        // 3) capture errors, expect two.
        // mark respective subtrees manually
        markTree(tree.entries[1]);
        markTree(tree.entries[3]);
        // correct double-marking of directory itself
        tree.entries[1].mark -= errVisit;
        tree.entries[3].mark -= errVisit;
        var errΔ2 = walk(tree.name, new Func<@string, fs.DirEntry, error, error>(markFnʗ1));
        if (errΔ2 != nil) {
            tΔ1.Fatalf("expected no error return from Walk, got %s", errΔ2);
        }
        if (len(Ꮡerrors.ValueSlot) != 2) {
            tΔ1.Errorf("expected 2 errors, got %d: %s", len(Ꮡerrors.ValueSlot), Ꮡerrors.ValueSlot);
        }
        // the inaccessible subtrees were marked manually
        checkMarks(tΔ1, true);
        Ꮡerrors.ValueSlot = Ꮡerrors.ValueSlot[0..0];

        // 4) capture errors, stop after first error.
        // mark respective subtrees manually
        markTree(tree.entries[1]);
        markTree(tree.entries[3]);
        // correct double-marking of directory itself
        tree.entries[1].mark -= errVisit;
        tree.entries[3].mark -= errVisit;
        clear = false; // error will stop processing
        errΔ2 = walk(tree.name, new Func<@string, fs.DirEntry, error, error>(markFnʗ1));
        if (errΔ2 == nil) {
            tΔ1.Fatalf("expected error return from Walk");
        }
        if (len(Ꮡerrors.ValueSlot) != 1) {
            tΔ1.Errorf("expected 1 error, got %d: %s", len(Ꮡerrors.ValueSlot), Ꮡerrors.ValueSlot);
        }
        // the inaccessible subtrees were marked manually
        checkMarks(tΔ1, false);
        Ꮡerrors.ValueSlot = Ꮡerrors.ValueSlot[0..0];

        // restore permissions
        os.Chmod(filepath.Join(tree.name, tree.entries[1].name), 504);
        os.Chmod(filepath.Join(tree.name, tree.entries[3].name), 504);
    });
}

internal static void touch(ж<testing.T> t, @string name) {
    var (f, err) = os.Create(name);
    if (err != nil) {
        t.Fatal(err);
    }
    {
        var errΔ1 = f.Close(); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string dirˢ => ref dirˢᵇ;
private static readonly @string dirˢᵇ = "dir"u8;
internal static ref readonly @string dirFoo1ˢ => ref dirFoo1ˢᵇ;
private static readonly @string dirFoo1ˢᵇ = "dir/foo1"u8;
internal static ref readonly @string dirFoo2ˢ => ref dirFoo2ˢᵇ;
private static readonly @string dirFoo2ˢᵇ = "dir/foo2"u8;
internal static ref readonly @string foo2ˢ => ref foo2ˢᵇ;
private static readonly @string foo2ˢᵇ = "foo2"u8;
internal static ref readonly @string foo1ˢ => ref foo1ˢᵇ;
private static readonly @string foo1ˢᵇ = "foo1"u8;
internal static ref readonly @string walkˢ => ref walkˢᵇ;
private static readonly @string walkˢᵇ = "Walk"u8;
internal static ref readonly @string walkDirˢ => ref walkDirˢᵇ;
private static readonly @string walkDirˢᵇ = "WalkDir"u8;

public static void TestWalkSkipDirOnFile(ж<testing.T> t) {
    @string td = t.TempDir();

    {
        var err = os.MkdirAll(filepath.Join(td, dirˢ), 493); if (err != nil) {
            t.Fatal(err);
        }
    }
    touch(t, filepath.Join(td, dirFoo1ˢ));
    touch(t, filepath.Join(td, dirFoo2ˢ));

    var sawFoo2 = false;
    error walker(@string path) {
        if (strings.HasSuffix(path, foo2ˢ)) {
            sawFoo2 = true;
        }
        if (strings.HasSuffix(path, foo1ˢ)) {
            return filepath.SkipDir;
        }
        return default;
    }
    var walkerʗ1 = walker;
    var walkFn = (@string path, fs.FileInfo _Δp1, error _Δp2) => walkerʗ1(path);
    var walkerʗ2 = walker;
    var walkDirFn = (@string path, fs.DirEntry _Δp1, error _Δp2) => walkerʗ2(path);

    void check(ж<testing.T> tΔ1, Func<@string, error> walk, @string root) {
        tΔ1.Helper();
        sawFoo2 = false;
        var err = walk(root);
        if (err != nil) {
            tΔ1.Fatal(err);
        }
        if (sawFoo2) {
            tΔ1.Errorf("SkipDir on file foo1 did not block processing of foo2");
        }
    }

    var checkʗ1 = check;
    var walkFnʗ1 = walkFn;
    t.Run(walkˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ2) => {
        var walkFnʗ2 = walkFnʗ1;
        var Walk = (@string root) => filepath.Walk(td, new Func<@string, fs.FileInfo, error, error>(walkFnʗ2));
        checkʗ1(tΔ2, Walk, td);
        checkʗ1(tΔ2, Walk, filepath.Join(td, dirˢ));
    });
    var checkʗ2 = check;
    var walkDirFnʗ1 = walkDirFn;
    t.Run(walkDirˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ3) => {
        var walkDirFnʗ2 = walkDirFnʗ1;
        var WalkDir = (@string root) => filepath.WalkDir(td, new Func<@string, fs.DirEntry, error, error>(walkDirFnʗ2));
        checkʗ2(tΔ3, WalkDir, td);
        checkʗ2(tΔ3, WalkDir, filepath.Join(td, dirˢ));
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string subdirˢ => ref subdirˢᵇ;
private static readonly @string subdirˢᵇ = "subdir"u8;
internal static ref readonly @string dir2ˢ => ref dir2ˢᵇ;
private static readonly @string dir2ˢᵇ = "dir2"u8;
internal static ref readonly @string foo3ˢ => ref foo3ˢᵇ;
private static readonly @string foo3ˢᵇ = "foo3"u8;
internal static ref readonly @string foo4ˢ => ref foo4ˢᵇ;
private static readonly @string foo4ˢᵇ = "foo4"u8;
internal static ref readonly @string barˢ => ref barˢᵇ;
private static readonly @string barˢᵇ = "bar"u8;
internal static ref readonly @string lastˢ => ref lastˢᵇ;
private static readonly @string lastˢᵇ = "last"u8;

public static void TestWalkSkipAllOnFile(ж<testing.T> t) {
    @string td = t.TempDir();

    {
        var err = os.MkdirAll(filepath.Join(td, dirˢ, subdirˢ), 493); if (err != nil) {
            t.Fatal(err);
        }
    }
    {
        var err = os.MkdirAll(filepath.Join(td, dir2ˢ), 493); if (err != nil) {
            t.Fatal(err);
        }
    }

    touch(t, filepath.Join(td, dirˢ, foo1ˢ));
    touch(t, filepath.Join(td, dirˢ, foo2ˢ));
    touch(t, filepath.Join(td, dirˢ, subdirˢ, foo3ˢ));
    touch(t, filepath.Join(td, dirˢ, foo4ˢ));
    touch(t, filepath.Join(td, dir2ˢ, barˢ));
    touch(t, filepath.Join(td, lastˢ));

    var remainingWereSkipped = true;
    error walker(@string path) {
        if (strings.HasSuffix(path, foo2ˢ)) {
            return filepath.SkipAll;
        }

        if (strings.HasSuffix(path, foo3ˢ) || strings.HasSuffix(path, foo4ˢ) || strings.HasSuffix(path, barˢ) || strings.HasSuffix(path, lastˢ)) {
            remainingWereSkipped = false;
        }
        return default;
    }

    var walkerʗ1 = walker;
    var walkFn = (@string path, fs.FileInfo _Δp1, error _Δp2) => walkerʗ1(path);
    var walkerʗ2 = walker;
    var walkDirFn = (@string path, fs.DirEntry _Δp1, error _Δp2) => walkerʗ2(path);

    void check(ж<testing.T> tΔ1, Func<@string, error> walk, @string root) {
        tΔ1.Helper();
        remainingWereSkipped = true;
        {
            var err = walk(root); if (err != nil) {
                tΔ1.Fatal(err);
            }
        }
        if (!remainingWereSkipped) {
            tΔ1.Errorf("SkipAll on file foo2 did not block processing of remaining files and directories");
        }
    }

    var checkʗ1 = check;
    var walkFnʗ1 = walkFn;
    t.Run(walkˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ2) => {
        var walkFnʗ2 = walkFnʗ1;
        var Walk = (@string _) => filepath.Walk(td, new Func<@string, fs.FileInfo, error, error>(walkFnʗ2));
        checkʗ1(tΔ2, Walk, td);
        checkʗ1(tΔ2, Walk, filepath.Join(td, dirˢ));
    });
    var checkʗ2 = check;
    var walkDirFnʗ1 = walkDirFn;
    t.Run(walkDirˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ3) => {
        var walkDirFnʗ2 = walkDirFnʗ1;
        var WalkDir = (@string _) => filepath.WalkDir(td, new Func<@string, fs.DirEntry, error, error>(walkDirFnʗ2));
        checkʗ2(tΔ3, WalkDir, td);
        checkʗ2(tΔ3, WalkDir, filepath.Join(td, dirˢ));
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;
internal static ref readonly @string bazˢ => ref bazˢᵇ;
private static readonly @string bazˢᵇ = "baz"u8;
internal static ref readonly @string statErrorˢ => ref statErrorˢᵇ;
private static readonly @string statErrorˢᵇ = "stat-error"u8;
internal static ref readonly @string someStatErrorˢ => ref someStatErrorˢᵇ;
private static readonly @string someStatErrorˢᵇ = "some stat error"u8;

public static void TestWalkFileError(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string td = t.TempDir();

        touch(t, filepath.Join(td, fooˢ));
        touch(t, filepath.Join(td, barˢ));
        @string dir = filepath.Join(td, dirˢ);
        {
            var errΔ1 = os.MkdirAll(filepath.Join(td, dirˢ), 493); if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
        }
        touch(t, filepath.Join(dir, bazˢ));
        touch(t, filepath.Join(dir, statErrorˢ));
        defer(() => {
            filepath_internal_test_package.LstatP.ValueSlot = os.Lstat;
        }, ref ᒐ);
        var statErr = errors.New(someStatErrorˢ);
        var statErrʗ1 = statErr;
        filepath_internal_test_package.LstatP.ValueSlot = (fs.FileInfo, error) (@string path) => {
            if (strings.HasSuffix(path, statErrorˢ)) {
                return (default, statErrʗ1);
            }
            return os.Lstat(path);
        };
        var got = new map<@string, error>{};
        var gotʗ1 = got;
        var err = filepath.Walk(td, (@string path, fs.FileInfo fi, error errΔ2) => {
            var (rel, _) = filepath.Rel(td, path);
            gotʗ1[filepath.ToSlash(rel)] = errΔ2;
            return default;
        });
        if (err != nil) {
            t.Errorf("Walk error: %v", err);
        }
        var want = new map<@string, error>{["."] = default, ["foo"] = default, ["bar"] = default, ["dir"] = default, ["dir/baz"] = default, ["dir/stat-error"] = statErr
        };
        if (!reflect.DeepEqual(got, want)) {
            t.Errorf("Walked %#v; want %#v", got, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string linkˢ => ref linkˢᵇ;
private static readonly @string linkˢᵇ = "link"u8;
internal static ref readonly @string abslinkˢ => ref abslinkˢᵇ;
private static readonly @string abslinkˢᵇ = "abslink"u8;
internal static ref readonly @string linklinkˢ => ref linklinkˢᵇ;
private static readonly @string linklinkˢᵇ = "linklink"u8;

internal partial struct TestWalkSymlinkRoot_type /*dyn*/ {
    internal @string desc;
    internal @string root;
    internal slice<@string> want;
    internal slice<@string> buggyGOOS;
}

public static void TestWalkSymlinkRoot(ж<testing.T> t) {
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(t));

    @string td = t.TempDir();
    @string dir = filepath.Join(td, dirˢ);
    {
        var err = os.MkdirAll(filepath.Join(td, dirˢ), 493); if (err != nil) {
            t.Fatal(err);
        }
    }
    touch(t, filepath.Join(dir, fooˢ));

    @string link = filepath.Join(td, linkˢ);
    {
        var err = os.Symlink(dirˢ, link); if (err != nil) {
            t.Fatal(err);
        }
    }

    @string abslink = filepath.Join(td, abslinkˢ);
    {
        var err = os.Symlink(dir, abslink); if (err != nil) {
            t.Fatal(err);
        }
    }

    @string linklink = filepath.Join(td, linklinkˢ);
    {
        var err = os.Symlink(linkˢ, linklink); if (err != nil) {
            t.Fatal(err);
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
    foreach (var (_, vᴛ1) in slice<TestWalkSymlinkRoot_type>([
        new(
            desc: "no slash",
            root: link,
            want: slice<@string>([link])
        ),
        new(
            desc: "slash",
            root: link + ((@string)(rune)filepath.Separator),
            want: slice<@string>([link, filepath.Join(link, fooˢ)])
        ),
        new(
            desc: "abs no slash",
            root: abslink,
            want: slice<@string>([abslink])
        ),
        new(
            desc: "abs with slash",
            root: abslink + ((@string)(rune)filepath.Separator),
            want: slice<@string>([abslink, filepath.Join(abslink, fooˢ)])
        ),
        new(
            desc: "double link no slash",
            root: linklink,
            want: slice<@string>([linklink])
        ),
        new(
            desc: "double link with slash",
            root: linklink + ((@string)(rune)filepath.Separator),
            want: slice<@string>([linklink, filepath.Join(linklink, fooˢ)]),
            buggyGOOS: slice<@string>(["darwin", "ios"]) // https://go.dev/issue/59586

        )
    ])) {
        ref var tt = ref heap(new TestWalkSymlinkRoot_type(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.desc, (ж<testing.T> tΔ1) => {
            ref var walked = ref heap<slice<@string>>(out var Ꮡwalked);
            var err = filepath.Walk(ttʗ1.root, error (@string path, fs.FileInfo info, error errΔ1) => {
                if (errΔ1 != nil) {
                    return errΔ1;
                }
                tΔ1.Logf("%#q: %v", path, info.Mode());
                Ꮡwalked.ValueSlot = append(Ꮡwalked.ValueSlot, filepath.Clean(path));
                return default;
            });
            if (err != nil) {
                tΔ1.Fatal(err);
            }

            if (!slices.Equal<slice<@string>, @string>(Ꮡwalked.ValueSlot, ttʗ1.want)) {
                tΔ1.Logf("Walk(%#q) visited %#q; want %#q", ttʗ1.root, Ꮡwalked.ValueSlot, ttʗ1.want);
                if (slices.Contains(ttʗ1.buggyGOOS, runtime.GOOS)) {
                    tΔ1.Logf("(ignoring known bug on %v)", runtime.GOOS);
                } else {
                    tΔ1.Fail();
                }
            }
        });
    }
}

internal static slice<PathTest> basetests = slice<PathTest>([
    new("", "."),
    new(".", "."),
    new("/.", "."),
    new("/", "/"),
    new("////", "/"),
    new("x/", "x"),
    new("abc", "abc"),
    new("abc/def", "def"),
    new("a/b/.x", ".x"),
    new("a/b/c.", "c."),
    new("a/b/c.x", "c.x")
]);

internal static slice<PathTest> winbasetests = slice<PathTest>([
    new(@"c:\", @"\"),
    new(@"c:.", @"."),
    new(@"c:\a\b", @"b"),
    new(@"c:a\b", @"b"),
    new(@"c:a\b\c", @"c"),
    new(@"\\host\share\", @"\"),
    new(@"\\host\share\a", @"a"),
    new(@"\\host\share\a\b", @"b")
]);

public static void TestBase(ж<testing.T> t) {
    var tests = basetests;
    if (runtime.GOOS == "windows"u8) {
        // make unix tests work on windows
        foreach (var i in range(tests)) {
            tests[i].result = filepath.Clean(tests[i].result);
        }
        // add windows specific tests
        tests = appendꓸꓸꓸ(tests, winbasetests);
    }
    foreach (var (_, test) in tests) {
        {
            @string s = filepath.Base(test.path); if (s != test.result) {
                t.Errorf("Base(%q) = %q, want %q", test.path, s, test.result);
            }
        }
    }
}

internal static slice<PathTest> dirtests = slice<PathTest>([
    new("", "."),
    new(".", "."),
    new("/.", "/"),
    new("/", "/"),
    new("/foo", "/"),
    new("x/", "x"),
    new("abc", "."),
    new("abc/def", "abc"),
    new("a/b/.x", "a/b"),
    new("a/b/c.", "a/b"),
    new("a/b/c.x", "a/b")
]);

internal static slice<PathTest> nonwindirtests = slice<PathTest>([
    new("////", "/")
]);

internal static slice<PathTest> windirtests = slice<PathTest>([
    new(@"c:\", @"c:\"),
    new(@"c:.", @"c:."),
    new(@"c:\a\b", @"c:\a"),
    new(@"c:a\b", @"c:a"),
    new(@"c:a\b\c", @"c:a\b"),
    new(@"\\host\share", @"\\host\share"),
    new(@"\\host\share\", @"\\host\share\"),
    new(@"\\host\share\a", @"\\host\share\"),
    new(@"\\host\share\a\b", @"\\host\share\a"),
    new(@"\\\\", @"\\\\")
]);

public static void TestDir(ж<testing.T> t) {
    var tests = dirtests;
    if (runtime.GOOS == "windows"u8) {
        // make unix tests work on windows
        foreach (var i in range(tests)) {
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
                t.Errorf("Dir(%q) = %q, want %q", test.path, s, test.result);
            }
        }
    }
}

partial struct IsAbsTest {
    internal @string path;
    internal bool isAbs;
}

internal static slice<IsAbsTest> isabstests = slice<IsAbsTest>([
    new("", false),
    new("/", true),
    new("/usr/bin/gcc", true),
    new("..", false),
    new("/a/../bb", true),
    new(".", false),
    new("./", false),
    new("lala", false)
]);

internal static slice<IsAbsTest> winisabstests = slice<IsAbsTest>([
    new(@"C:\", true),
    new(@"c\", false),
    new(@"c::", false),
    new(@"c:", false),
    new(@"/", false),
    new(@"\", false),
    new(@"\Windows", false),
    new(@"c:a\b", false),
    new(@"c:\a\b", true),
    new(@"c:/a/b", true),
    new(@"\\host\share", true),
    new(@"\\host\share\", true),
    new(@"\\host\share\foo", true),
    new(@"//host/share/foo/bar", true),
    new(@"\\?\a\b\c", true),
    new(@"\??\a\b\c", true)
]);

public static void TestIsAbs(ж<testing.T> t) {
    var testsᴷ = new stackAppend<IsAbsTest>();

    slice<IsAbsTest> tests = default;
    if (runtime.GOOS == "windows"u8) {
        tests = appendꓸꓸꓸ(tests, winisabstests);
        // All non-windows tests should fail, because they have no volume letter.
        foreach (var (_, test) in isabstests) {
            tests = append(testsᴷ, tests, new IsAbsTest(test.path, false));
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
                t.Errorf("IsAbs(%q) = %v, want %v", test.path, r, test.isAbs);
            }
        }
    }
}

partial struct EvalSymlinksTest {
    // If dest is empty, the path is created; otherwise the dest is symlinked to the path.
    internal @string path, dest;
}

public static readonly ж<slice<EvalSymlinksTest>> ᏑEvalSymlinksTestDirs = new StaticBox<slice<EvalSymlinksTest>>(slice<EvalSymlinksTest>([
    new("test", ""),
    new("test/dir", ""),
    new("test/dir/link3", "../../"),
    new("test/link1", "../test"),
    new("test/link2", "dir"),
    new("test/linkabs", "/"),
    new("test/link4", "../test2"),
    new("test2", "test/dir"),
    // Issue 23444.
    new("src", ""),
    new("src/pool", ""),
    new("src/pool/test", ""),
    new("src/versions", ""),
    new("src/versions/current", "../../version"),
    new("src/versions/v1", ""),
    new("src/versions/v1/modules", ""),
    new("src/versions/v1/modules/test", "../../../pool/test"),
    new("version", "src/versions/v1")
]));
public static ref slice<EvalSymlinksTest> EvalSymlinksTestDirs => ref ((StaticBox<slice<EvalSymlinksTest>>)ᏑEvalSymlinksTestDirs).ValueSlot;

public static readonly ж<slice<EvalSymlinksTest>> ᏑEvalSymlinksTests = new StaticBox<slice<EvalSymlinksTest>>(slice<EvalSymlinksTest>([
    new("test", "test"),
    new("test/dir", "test/dir"),
    new("test/dir/../..", "."),
    new("test/link1", "test"),
    new("test/link2", "test/dir"),
    new("test/link1/dir", "test/dir"),
    new("test/link2/..", "test"),
    new("test/dir/link3", "."),
    new("test/link2/link3/test", "test"),
    new("test/linkabs", "/"),
    new("test/link4/..", "test"),
    new("src/versions/current/modules/test", "src/pool/test")
]));
public static ref slice<EvalSymlinksTest> EvalSymlinksTests => ref ((StaticBox<slice<EvalSymlinksTest>>)ᏑEvalSymlinksTests).ValueSlot;

// simpleJoin builds a file name from the directory and path.
// It does not use Join because we don't want ".." to be evaluated.
internal static @string simpleJoin(@string dir, @string path) {
    return dir + ((@string)(rune)filepath.Separator) + path;
}

internal static void testEvalSymlinks(ж<testing.T> t, @string path, @string want) {
    var (have, err) = filepath.EvalSymlinks(path);
    if (err != nil) {
        t.Errorf("EvalSymlinks(%q) error: %v", path, err);
        return;
    }
    if (filepath.Clean(have) != filepath.Clean(want)) {
        t.Errorf("EvalSymlinks(%q) returns %q, want %q", path, have, want);
    }
}

internal static void testEvalSymlinksAfterChdir(ж<testing.T> t, @string wd, @string path, @string want) {
    t.Chdir(wd);
    var (have, err) = filepath.EvalSymlinks(path);
    if (err != nil) {
        t.Errorf("EvalSymlinks(%q) in %q directory error: %v", path, wd, err);
        return;
    }
    if (filepath.Clean(have) != filepath.Clean(want)) {
        t.Errorf("EvalSymlinks(%q) in %q directory returns %q, want %q", path, wd, have, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object evalSymlinkForTmpDirˢ = (@string)"eval symlink for tmp dir:"u8;
internal static ref readonly @string testˢ => ref testˢᵇ;
private static readonly @string testˢᵇ = "test"u8;

public static void TestEvalSymlinks(ж<testing.T> t) {
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(t));

    @string tmpDir = t.TempDir();

    // /tmp may itself be a symlink! Avoid the confusion, although
    // it means trusting the thing we're testing.
    error err = default;
    (tmpDir, err) = filepath.EvalSymlinks(tmpDir);
    if (err != nil) {
        t.Fatal(evalSymlinkForTmpDirˢ, err);
    }

    // Create the symlink farm using relative paths.
    foreach (var (_, d) in EvalSymlinksTestDirs) {
        error errΔ1 = default;
        @string path = simpleJoin(tmpDir, d.path);
        if (d.dest == ""u8) {
            errΔ1 = os.Mkdir(path, 493);
        } else {
            errΔ1 = os.Symlink(d.dest, path);
        }
        if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
    }

    // Evaluate the symlink farm.
    foreach (var (_, test) in EvalSymlinksTests) {
        @string path = simpleJoin(tmpDir, test.path);

        @string dest = simpleJoin(tmpDir, test.dest);
        if (filepath.IsAbs(test.dest) || os.IsPathSeparator(test.dest[0])) {
            dest = test.dest;
        }
        testEvalSymlinks(t, path, dest);

        // test EvalSymlinks(".")
        testEvalSymlinksAfterChdir(t, path, ".", ".");

        // test EvalSymlinks("C:.") on Windows
        if (runtime.GOOS == "windows"u8) {
            @string volDot = filepath.VolumeName(tmpDir) + "."u8;
            testEvalSymlinksAfterChdir(t, path, volDot, volDot);
        }

        // test EvalSymlinks(".."+path)
        @string dotdotPath = simpleJoin("..", test.dest);
        if (filepath.IsAbs(test.dest) || os.IsPathSeparator(test.dest[0])) {
            dotdotPath = test.dest;
        }
        testEvalSymlinksAfterChdir(t,
            simpleJoin(tmpDir, testˢ),
            simpleJoin("..", test.path),
            dotdotPath);

        // test EvalSymlinks(p) where p is relative path
        testEvalSymlinksAfterChdir(t, tmpDir, test.path, test.dest);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string notexistˢ => ref notexistˢᵇ;
private static readonly @string notexistˢᵇ = "notexist"u8;

public static void TestEvalSymlinksIsNotExist(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(t));
        t.Chdir(t.TempDir());

        var (_, err) = filepath.EvalSymlinks(notexistˢ);
        if (!os.IsNotExist(err)) {
            t.Errorf("expected the file is not found, got %v\n", err);
        }

        err = os.Symlink(notexistˢ, linkˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        defer(os.Remove, linkˢ, ref ᒐ);

        (_, err) = filepath.EvalSymlinks(linkˢ);
        if (!os.IsNotExist(err)) {
            t.Errorf("expected the file is not found, got %v\n", err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string linkToDirˢ => ref linkToDirˢᵇ;
private static readonly @string linkToDirˢᵇ = "link_to_dir"u8;
internal static ref readonly @string fileˢ => ref fileˢᵇ;
private static readonly @string fileˢᵇ = "file"u8;
internal static ref readonly @string link1ˢ => ref link1ˢᵇ;
private static readonly @string link1ˢᵇ = "link1"u8;
internal static ref readonly @string link2ˢ => ref link2ˢᵇ;
private static readonly @string link2ˢᵇ = "link2"u8;

internal partial struct TestIssue13582_tests /*dyn*/ {
    internal @string path, want;
}

public static void TestIssue13582(ж<testing.T> t) {
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(t));

    @string tmpDir = t.TempDir();

    @string dir = filepath.Join(tmpDir, dirˢ);
    var err = os.Mkdir(dir, 493);
    if (err != nil) {
        t.Fatal(err);
    }
    @string linkToDir = filepath.Join(tmpDir, linkToDirˢ);
    err = os.Symlink(dir, linkToDir);
    if (err != nil) {
        t.Fatal(err);
    }
    @string @file = filepath.Join(linkToDir, fileˢ);
    err = os.WriteFile(@file, default, 420);
    if (err != nil) {
        t.Fatal(err);
    }
    @string link1 = filepath.Join(linkToDir, link1ˢ);
    err = os.Symlink(@file, link1);
    if (err != nil) {
        t.Fatal(err);
    }
    @string link2 = filepath.Join(linkToDir, link2ˢ);
    err = os.Symlink(link1, link2);
    if (err != nil) {
        t.Fatal(err);
    }

    // /tmp may itself be a symlink!
    (var realTmpDir, err) = filepath.EvalSymlinks(tmpDir);
    if (err != nil) {
        t.Fatal(err);
    }
    @string realDir = filepath.Join(realTmpDir, dirˢ);
    @string realFile = filepath.Join(realDir, fileˢ);

    var tests = slice<TestIssue13582_tests>([
        new(dir, realDir),
        new(linkToDir, realDir),
        new(@file, realFile),
        new(link1, realFile),
        new(link2, realFile)
    ]);
    foreach (var (i, test) in tests) {
        var (have, errΔ1) = filepath.EvalSymlinks(test.path);
        if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        if (have != test.want) {
            t.Errorf("test#%d: EvalSymlinks(%q) returns %q, want %q", i, test.path, have, test.want);
        }
    }
}

// Issue 57905.
public static void TestRelativeSymlinkToAbsolute(ж<testing.T> t) {
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(t));
    // Not parallel: uses t.Chdir.

    @string tmpDir = t.TempDir();
    t.Chdir(tmpDir);

    // Create "link" in the current working directory as a symlink to an arbitrary
    // absolute path. On macOS, this path is likely to begin with a symlink
    // itself: generally either in /var (symlinked to "private/var") or /tmp
    // (symlinked to "private/tmp").
    {
        var errΔ1 = os.Symlink(tmpDir, linkˢ); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
    }
    t.Logf(@"os.Symlink(%q, ""link"")", tmpDir);

    var (p, err) = filepath.EvalSymlinks(linkˢ);
    if (err != nil) {
        t.Fatalf(@"EvalSymlinks(""link""): %v", err);
    }
    (var want, err) = filepath.EvalSymlinks(tmpDir);
    if (err != nil) {
        t.Fatalf(@"EvalSymlinks(%q): %v", tmpDir, err);
    }
    if (p != want) {
        t.Errorf(@"EvalSymlinks(""link"") = %q; want %q", p, want);
    }
    t.Logf(@"EvalSymlinks(""link"") = %q", p);
}

// Test directories relative to temporary directory.
// The tests are run in absTestDirs[0].
internal static slice<@string> absTestDirs = slice<@string>([
    "a",
    "a/b",
    "a/b/c"
]);

// Test paths relative to temporary directory. $ expands to the directory.
// The tests are run in absTestDirs[0].
// We create absTestDirs first.
internal static slice<@string> absTests = slice<@string>([
    ".",
    "b",
    "b/",
    "../a",
    "../a/b",
    "../a/b/./c/../../.././a",
    "../a/b/./c/../../.././a/",
    "$",
    "$/.",
    "$/a/../a/b",
    "$/a/b/c/../../.././a",
    "$/a/b/c/../../.././a/"
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object mkdirFailedˢ = (@string)"Mkdir failed: "u8;
internal static readonly object chdirFailedˢ = (@string)"chdir failed: "u8;

public static void TestAbs(ж<testing.T> t) {
    var extraᴷ = new stackAppend<@string>();

    @string root = t.TempDir();
    t.Chdir(root);

    foreach (var (_, dir) in absTestDirs) {
        var errΔ1 = os.Mkdir(dir, 511);
        if (errΔ1 != nil) {
            t.Fatal(mkdirFailedˢ, errΔ1);
        }
    }

    // Make sure the global absTests slice is not
    // modified by multiple invocations of TestAbs.
    var tests = absTests;
    if (runtime.GOOS == "windows"u8) {
        @string vol = filepath.VolumeName(root);
        slice<@string> extra = default;
        foreach (var (_, vᴛ1) in absTests) {
            var path = vᴛ1;

            if (strings.Contains(path, "$")) {
                continue;
            }
            path = vol + path;
            extra = append(extraᴷ, extra, path);
        }
        tests = appendꓸꓸꓸ(slices.Clip<slice<@string>, @string>(tests), extra);
    }

    var err = os.Chdir(absTestDirs[0]);
    if (err != nil) {
        t.Fatal(chdirFailedˢ, err);
    }

    foreach (var (_, vᴛ2) in tests) {
        var path = vᴛ2;

        path = strings.ReplaceAll(path, "$", root);
        var (info, errΔ2) = os.Stat(path);
        if (errΔ2 != nil) {
            t.Errorf("%s: %s", path, errΔ2);
            continue;
        }

        (var abspath, errΔ2) = filepath.Abs(path);
        if (errΔ2 != nil) {
            t.Errorf("Abs(%q) error: %v", path, errΔ2);
            continue;
        }
        (var absinfo, errΔ2) = os.Stat(abspath);
        if (errΔ2 != nil || !os.SameFile(absinfo, info)) {
            t.Errorf("Abs(%q)=%q, not the same file", path, abspath);
        }
        if (!filepath.IsAbs(abspath)) {
            t.Errorf("Abs(%q)=%q, not an absolute path", path, abspath);
        }
        if (filepath.IsAbs(abspath) && abspath != filepath.Clean(abspath)) {
            t.Errorf("Abs(%q)=%q, isn't clean", path, abspath);
        }
    }
}

// Empty path needs to be special-cased on Windows. See golang.org/issue/24441.
// We test it separately from all other absTests because the empty string is not
// a valid path, so it can't be used with os.Stat.
public static void TestAbsEmptyString(ж<testing.T> t) {
    @string root = t.TempDir();
    t.Chdir(root);

    var (info, err) = os.Stat(root);
    if (err != nil) {
        t.Fatalf("%s: %s", root, err);
    }

    (var abspath, err) = filepath.Abs(""u8);
    if (err != nil) {
        t.Fatalf(@"Abs("""") error: %v", err);
    }
    (var absinfo, err) = os.Stat(abspath);
    if (err != nil || !os.SameFile(absinfo, info)) {
        t.Errorf(@"Abs("""")=%q, not the same file", abspath);
    }
    if (!filepath.IsAbs(abspath)) {
        t.Errorf(@"Abs("""")=%q, not an absolute path", abspath);
    }
    if (filepath.IsAbs(abspath) && abspath != filepath.Clean(abspath)) {
        t.Errorf(@"Abs("""")=%q, isn't clean", abspath);
    }
}

partial struct RelTests {
    internal @string root, path, want;
}

internal static slice<RelTests> reltests = slice<RelTests>([
    new("a/b", "a/b", "."),
    new("a/b/.", "a/b", "."),
    new("a/b", "a/b/.", "."),
    new("./a/b", "a/b", "."),
    new("a/b", "./a/b", "."),
    new("ab/cd", "ab/cde", "../cde"),
    new("ab/cd", "ab/c", "../c"),
    new("a/b", "a/b/c/d", "c/d"),
    new("a/b", "a/b/../c", "../c"),
    new("a/b/../c", "a/b", "../b"),
    new("a/b/c", "a/c/d", "../../c/d"),
    new("a/b", "c/d", "../../c/d"),
    new("a/b/c/d", "a/b", "../.."),
    new("a/b/c/d", "a/b/", "../.."),
    new("a/b/c/d/", "a/b", "../.."),
    new("a/b/c/d/", "a/b/", "../.."),
    new("../../a/b", "../../a/b/c/d", "c/d"),
    new("/a/b", "/a/b", "."),
    new("/a/b/.", "/a/b", "."),
    new("/a/b", "/a/b/.", "."),
    new("/ab/cd", "/ab/cde", "../cde"),
    new("/ab/cd", "/ab/c", "../c"),
    new("/a/b", "/a/b/c/d", "c/d"),
    new("/a/b", "/a/b/../c", "../c"),
    new("/a/b/../c", "/a/b", "../b"),
    new("/a/b/c", "/a/c/d", "../../c/d"),
    new("/a/b", "/c/d", "../../c/d"),
    new("/a/b/c/d", "/a/b", "../.."),
    new("/a/b/c/d", "/a/b/", "../.."),
    new("/a/b/c/d/", "/a/b", "../.."),
    new("/a/b/c/d/", "/a/b/", "../.."),
    new("/../../a/b", "/../../a/b/c/d", "c/d"),
    new(".", "a/b", "a/b"),
    new(".", "..", ".."),
    new("", "../../.", "../.."),

    // can't do purely lexically
    new("..", ".", "err"),
    new("..", "a", "err"),
    new("../..", "..", "err"),
    new("a", "/a", "err"),
    new("/a", "a", "err")
]);

internal static slice<RelTests> winreltests = slice<RelTests>([
    new(@"C:a\b\c", @"C:a/b/d", @"..\d"),
    new(@"C:\", @"D:\", @"err"),
    new(@"C:", @"D:", @"err"),
    new(@"C:\Projects", @"c:\projects\src", @"src"),
    new(@"C:\Projects", @"c:\projects", @"."),
    new(@"C:\Projects\a\..", @"c:\projects", @"."),
    new(@"\\host\share", @"\\host\share\file.txt", @"file.txt")
]);

public static void TestRel(ж<testing.T> t) {
    var tests = appendꓸꓸꓸ(slice<RelTests>([]), reltests);
    if (runtime.GOOS == "windows"u8) {
        foreach (var i in range(tests)) {
            tests[i].want = filepath.FromSlash(tests[i].want);
        }
        tests = appendꓸꓸꓸ(tests, winreltests);
    }
    foreach (var (_, test) in tests) {
        var (got, err) = filepath.Rel(test.root, test.path);
        if (test.want == "err"u8) {
            if (err == nil) {
                t.Errorf("Rel(%q, %q)=%q, want error", test.root, test.path, got);
            }
            continue;
        }
        if (err != nil) {
            t.Errorf("Rel(%q, %q): want %q, got error: %s", test.root, test.path, test.want, err);
        }
        if (got != test.want) {
            t.Errorf("Rel(%q, %q)=%q, want %q", test.root, test.path, got, test.want);
        }
    }
}

partial struct VolumeNameTest {
    internal @string path;
    internal @string vol;
}

internal static slice<VolumeNameTest> volumenametests = slice<VolumeNameTest>([
    new(@"c:/foo/bar", @"c:"),
    new(@"c:", @"c:"),
    new(@"c:\", @"c:"),
    new(@"2:", @"2:"),
    new(@"", @""),
    new(@"\\\host", @"\\\host"),
    new(@"\\\host\", @"\\\host"),
    new(@"\\\host\share", @"\\\host"),
    new(@"\\\host\\share", @"\\\host"),
    new(@"\\host", @"\\host"),
    new(@"//host", @"\\host"),
    new(@"\\host\", @"\\host\"),
    new(@"//host/", @"\\host\"),
    new(@"\\host\share", @"\\host\share"),
    new(@"//host/share", @"\\host\share"),
    new(@"\\host\share\", @"\\host\share"),
    new(@"//host/share/", @"\\host\share"),
    new(@"\\host\share\foo", @"\\host\share"),
    new(@"//host/share/foo", @"\\host\share"),
    new(@"\\host\share\\foo\\\bar\\\\baz", @"\\host\share"),
    new(@"//host/share//foo///bar////baz", @"\\host\share"),
    new(@"\\host\share\foo\..\bar", @"\\host\share"),
    new(@"//host/share/foo/../bar", @"\\host\share"),
    new(@"//.", @"\\."),
    new(@"//./", @"\\.\"),
    new(@"//./NUL", @"\\.\NUL"),
    new(@"//?", @"\\?"),
    new(@"//?/", @"\\?\"),
    new(@"//?/NUL", @"\\?\NUL"),
    new(@"/??", @"\??"),
    new(@"/??/", @"\??\"),
    new(@"/??/NUL", @"\??\NUL"),
    new(@"//./a/b", @"\\.\a"),
    new(@"//./C:", @"\\.\C:"),
    new(@"//./C:/", @"\\.\C:"),
    new(@"//./C:/a/b/c", @"\\.\C:"),
    new(@"//./UNC/host/share/a/b/c", @"\\.\UNC\host\share"),
    new(@"//./UNC/host", @"\\.\UNC\host"),
    new(@"//./UNC/host\", @"\\.\UNC\host\"),
    new(@"//./UNC", @"\\.\UNC"),
    new(@"//./UNC/", @"\\.\UNC\"),
    new(@"\\?\x", @"\\?\x"),
    new(@"\??\x", @"\??\x")
]);

public static void TestVolumeName(ж<testing.T> t) {
    if (runtime.GOOS != "windows"u8) {
        return;
    }
    foreach (var (_, v) in volumenametests) {
        {
            @string vol = filepath.VolumeName(v.path); if (vol != v.vol) {
                t.Errorf("VolumeName(%q)=%q, want %q", v.path, vol, v.vol);
            }
        }
    }
}

public static void TestDriveLetterInEvalSymlinks(ж<testing.T> t) {
    if (runtime.GOOS != "windows"u8) {
        return;
    }
    var (wd, _) = os.Getwd();
    if (len(wd) < 3) {
        t.Errorf("Current directory path %q is too short", wd);
    }
    @string lp = strings.ToLower(wd);
    @string up = strings.ToUpper(wd);
    var (flp, err) = filepath.EvalSymlinks(lp);
    if (err != nil) {
        t.Fatalf("EvalSymlinks(%q) failed: %q", lp, err);
    }
    (var fup, err) = filepath.EvalSymlinks(up);
    if (err != nil) {
        t.Fatalf("EvalSymlinks(%q) failed: %q", up, err);
    }
    if (flp != fup) {
        t.Errorf("Results of EvalSymlinks do not match: %q and %q", flp, fup);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string srcˢ => ref srcˢᵇ;
private static readonly @string srcˢᵇ = "src"u8;
internal static ref readonly @string unicodeˢ => ref unicodeˢᵇ;
private static readonly @string unicodeˢᵇ = "unicode"u8;
internal static ref readonly @string utf16ˢ => ref utf16ˢᵇ;
private static readonly @string utf16ˢᵇ = "utf16"u8;
internal static ref readonly @string utf8ˢ => ref utf8ˢᵇ;
private static readonly @string utf8ˢᵇ = "utf8"u8;
internal static readonly object filepathWalkOutOfOrderˢ = (@string)"filepath.Walk out of order - utf8 before utf16"u8;

public static void TestBug3486(ж<testing.T> t) {
    // https://golang.org/issue/3486
    if (runtime.GOOS == "ios"u8) {
        t.Skipf("skipping on %s/%s", runtime.GOOS, runtime.GOARCH);
    }
    @string root = filepath.Join(testenv.GOROOT(new filepath_test_package.testing_TжTB(t)), srcˢ, unicodeˢ);
    @string utf16 = filepath.Join(root, utf16ˢ);
    @string utf8 = filepath.Join(root, utf8ˢ);
    var seenUTF16 = false;
    var seenUTF8 = false;
    var err = filepath.Walk(root, error (@string pth, fs.FileInfo info, error errΔ1) => {
        if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }

        var exprᴛ1 = pth;
        switch (ᐧ) {
        case {} when exprᴛ1 == utf16: {
            seenUTF16 = true;
            return filepath.SkipDir;
        }
        case {} when exprᴛ1 == utf8: {
            if (!seenUTF16) {
                t.Fatal(filepathWalkOutOfOrderˢ);
            }
            seenUTF8 = true;
            break;
        }}
        return default;
    });
    if (err != nil) {
        t.Fatal(err);
    }
    if (!seenUTF8) {
        t.Fatalf("%q not seen", utf8);
    }
}

internal static void testWalkSymlink(ж<testing.T> t, Func<@string, @string, error> mklink) {
    @string tmpdir = t.TempDir();
    t.Chdir(tmpdir);

    var err = mklink(tmpdir, linkˢ);
    if (err != nil) {
        t.Fatal(err);
    }

    ref var visited = ref heap<slice<@string>>(out var Ꮡvisited);
    err = filepath.Walk(tmpdir, (@string path, fs.FileInfo info, error errΔ1) => {
        if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        (var rel, errΔ1) = filepath.Rel(tmpdir, path);
        if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        Ꮡvisited.ValueSlot = append(Ꮡvisited.ValueSlot, rel);
        return default;
    });
    if (err != nil) {
        t.Fatal(err);
    }
    slices.Sort<slice<@string>, @string>(visited);
    var want = slice<@string>([".", "link"]);
    if (fmt.Sprintf("%q"u8, visited) != fmt.Sprintf("%q"u8, want)) {
        t.Errorf("unexpected paths visited %q, want %q", visited, want);
    }
}

public static void TestWalkSymlink(ж<testing.T> t) {
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(t));
    testWalkSymlink(t, os.Symlink);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fileTxtˢ => ref fileTxtˢᵇ;
private static readonly @string fileTxtˢᵇ = "file.txt"u8;

public static void TestIssue29372(ж<testing.T> t) {
    @string tmpDir = t.TempDir();

    @string path = filepath.Join(tmpDir, fileTxtˢ);
    var err = os.WriteFile(path, default, 420);
    if (err != nil) {
        t.Fatal(err);
    }

    @string pathSeparator = (@string)(rune)filepath.Separator;
    var tests = slice<@string>([
        path + strings.Repeat(pathSeparator, 1),
        path + strings.Repeat(pathSeparator, 2),
        path + strings.Repeat(pathSeparator, 1) + "."u8,
        path + strings.Repeat(pathSeparator, 2) + "."u8,
        path + strings.Repeat(pathSeparator, 1) + ".."u8,
        path + strings.Repeat(pathSeparator, 2) + ".."u8
    ]);

    foreach (var (i, test) in tests) {
        (_, err) = filepath.EvalSymlinks(test);
        if (!AreEqual(err, syscall.ENOTDIR)) {
            t.Fatalf("test#%d: want %q, got %q", i, syscall.ENOTDIR, err);
        }
    }
}

// Issue 30520 part 1.
public static void TestEvalSymlinksAboveRoot(ж<testing.T> t) {
    var ddᴷ = new stackAppend<@string>();

    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(t));

    t.Parallel();

    @string tmpDir = t.TempDir();

    var (evalTmpDir, err) = filepath.EvalSymlinks(tmpDir);
    if (err != nil) {
        t.Fatal(err);
    }

    {
        var errΔ1 = os.Mkdir(filepath.Join(evalTmpDir, "a"), 511); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
    }
    {
        var errΔ2 = os.Symlink(filepath.Join(evalTmpDir, "a"), filepath.Join(evalTmpDir, "b")); if (errΔ2 != nil) {
            t.Fatal(errΔ2);
        }
    }
    {
        var errΔ3 = os.WriteFile(filepath.Join(evalTmpDir, "a", fileˢ), default, 438); if (errΔ3 != nil) {
            t.Fatal(errΔ3);
        }
    }

    // Count the number of ".." elements to get to the root directory.
    @string vol = filepath.VolumeName(evalTmpDir);
    nint c = strings.Count(evalTmpDir.slice(len(vol)), (@string)(rune)os.PathSeparator);
    slice<@string> dd = default;
    for (nint i = 0; i < c + 2; i++) {
        dd = append(ddᴷ, dd, (@string)"..");
    }

    @string wantSuffix = strings.Join(slice<@string>(["a", "file"]), (@string)(rune)os.PathSeparator);

    // Try different numbers of "..".
    foreach (var (_, i) in slice<nint>([c, c + 1, c + 2])) {
        @string check = strings.Join(slice<@string>([evalTmpDir, strings.Join(dd.slice(0, i), (@string)(rune)os.PathSeparator), evalTmpDir.slice(len(vol) + 1), "b", "file"]), (@string)(rune)os.PathSeparator);
        var (resolved, errΔ4) = filepath.EvalSymlinks(check);
        switch (ᐧ) {
        case {} when runtime.GOOS == "darwin"u8 && errors.Is(errΔ4, fs.ErrNotExist): {
            // On darwin, the temp dir is sometimes cleaned up mid-test (issue 37910).
            testenv.SkipFlaky(new filepath_test_package.testing_TжTB(t), 37910);
            break;
        }
        case {} when errΔ4 != nil: {
            t.Errorf("EvalSymlinks(%q) failed: %v", check, errΔ4);
            break;
        }
        case {} when !strings.HasSuffix(resolved, wantSuffix): {
            t.Errorf("EvalSymlinks(%q) = %q does not end with %q", check, resolved, wantSuffix);
            break;
        }
        default: {
            t.Logf("EvalSymlinks(%q) = %q", check, resolved);
            break;
        }}
    }
}

// Issue 30520 part 2.
public static void TestEvalSymlinksAboveRootChdir(ж<testing.T> t) {
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(t));
    t.Chdir(t.TempDir());

    @string subdir = filepath.Join("a", "b");
    {
        var err = os.MkdirAll(subdir, 511); if (err != nil) {
            t.Fatal(err);
        }
    }
    {
        var err = os.Symlink(subdir, "c"); if (err != nil) {
            t.Fatal(err);
        }
    }
    {
        var err = os.WriteFile(filepath.Join(subdir, fileˢ), default, 438); if (err != nil) {
            t.Fatal(err);
        }
    }

    subdir = filepath.Join("d", "e", "f");
    {
        var err = os.MkdirAll(subdir, 511); if (err != nil) {
            t.Fatal(err);
        }
    }
    {
        var err = os.Chdir(subdir); if (err != nil) {
            t.Fatal(err);
        }
    }

    @string check = filepath.Join("..", "..", "..", "c", fileˢ);
    @string wantSuffix = filepath.Join("a", "b", fileˢ);
    {
        var (resolved, err) = filepath.EvalSymlinks(check); if (err != nil) {
            t.Errorf("EvalSymlinks(%q) failed: %v", check, err);
        } else if (!strings.HasSuffix(resolved, wantSuffix)) {
            t.Errorf("EvalSymlinks(%q) = %q does not end with %q", check, resolved, wantSuffix);
        } else {
            t.Logf("EvalSymlinks(%q) = %q", check, resolved);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string badˢ => ref badˢᵇ;
private static readonly @string badˢᵇ = "bad"u8;
internal static ref readonly @string nextˢ => ref nextˢᵇ;
private static readonly @string nextˢᵇ = "next"u8;

public static void TestIssue51617(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string dir = t.TempDir();
        foreach (var (_, sub) in slice<@string>(["a", filepath.Join("a", badˢ), filepath.Join("a", nextˢ)])) {
            {
                var errΔ1 = os.Mkdir(filepath.Join(dir, sub), 493); if (errΔ1 != nil) {
                    t.Fatal(errΔ1);
                }
            }
        }
        @string bad = filepath.Join(dir, "a", badˢ);
        {
            var errΔ2 = os.Chmod(bad, 0); if (errΔ2 != nil) {
                t.Fatal(errΔ2);
            }
        }
        defer(os.Chmod, bad, (fs.FileMode)448, ref ᒐ); // avoid errors on cleanup
        ref var saw = ref heap<slice<@string>>(out var Ꮡsaw);
        var err = filepath.WalkDir(dir, error (@string path, fs.DirEntry d, error errΔ3) => {
            if (errΔ3 != nil) {
                return filepath.SkipDir;
            }
            if (d.IsDir()) {
                var (rel, errΔ4) = filepath.Rel(dir, path);
                if (errΔ4 != nil) {
                    t.Fatal(errΔ4);
                }
                Ꮡsaw.ValueSlot = append(Ꮡsaw.ValueSlot, rel);
            }
            return default;
        });
        if (err != nil) {
            t.Fatal(err);
        }
        var want = slice<@string>([".", "a", filepath.Join("a", badˢ), filepath.Join("a", nextˢ)]);
        if (!slices.Equal<slice<@string>, @string>(saw, want)) {
            t.Errorf("got directories %v, want %v", saw, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestEscaping(ж<testing.T> t) {
    @string dir = t.TempDir();
    t.Chdir(t.TempDir());

    foreach (var (_, p) in slice<@string>([
        filepath.Join(dir, "x")
    ])) {
        if (!filepath.IsLocal(p)) {
            continue;
        }
        var (f, err) = os.Create(p);
        if (err != nil) {
            f.Close();
        }
        (var ents, err) = os.ReadDir(dir);
        if (err != nil) {
            t.Fatal(err);
        }
        foreach (var (_, e) in ents) {
            t.Fatalf("found: %v", e.Name());
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorGotNilˢ = (@string)"expected error, got nil"u8;

public static void TestEvalSymlinksTooManyLinks(ж<testing.T> t) {
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(t));
    @string dir = filepath.Join(t.TempDir(), dirˢ);
    var err = os.Symlink(dir, dir);
    if (err != nil) {
        t.Fatal(err);
    }
    (_, err) = filepath.EvalSymlinks(dir);
    if (err == nil) {
        t.Fatal(expectedErrorGotNilˢ);
    }
}

public static void BenchmarkIsLocal(ж<testing.B> b) {
    var tests = islocaltests;
    if (runtime.GOOS == "windows"u8) {
        tests = appendꓸꓸꓸ(tests, winislocaltests);
    }
    if (runtime.GOOS == "plan9"u8) {
        tests = appendꓸꓸꓸ(tests, plan9islocaltests);
    }
    while (b.Loop()) {
        foreach (var (_, test) in tests) {
            filepath.IsLocal(test.path);
        }
    }
}

} // end filepath_test_package
