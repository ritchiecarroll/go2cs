// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δpath = path_package;
using static path_package;
using Δruntime = runtime_package;
using testing = testing_package;

partial class path_test_package {

partial struct PathTest {
    internal @string path, result;
}

internal static slice<PathTest> cleantests = [
    // Already clean
    new("", "."),
    new("abc", "abc"),
    new("abc/def", "abc/def"),
    new("a/b/c", "a/b/c"),
    new(".", "."),
    new("..", ".."),
    new("../..", "../.."),
    new("../../abc", "../../abc"),
    new("/abc", "/abc"),
    new("/", "/"),

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
    new("//abc", "/abc"),
    new("///abc", "/abc"),
    new("//abc//", "/abc"),
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

    // Combinations
    new("abc/./../def", "def"),
    new("abc//./../def", "def"),
    new("abc/../../././../def", "../../def")
];

public static void TestClean(ж<testing.T> t) {
    foreach (var (_, test) in range(cleantests)) {
        {
            @string s = Clean(test.path); if (s != test.result) {
                t.Errorf("Clean(%q) = %q, want %q", test.path, s, test.result);
            }
        }
        {
            @string s = Clean(test.result); if (s != test.result) {
                t.Errorf("Clean(%q) = %q, want %q", test.result, s, test.result);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object skippingMallocCountInˢ = (@string)"skipping malloc count in short mode"u8;
private static readonly object skippingAllocsPerRunˢ = (@string)"skipping AllocsPerRun checks; GOMAXPROCS>1"u8;

public static void TestCleanMallocs(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(skippingMallocCountInˢ);
    }
    if (Δruntime.GOMAXPROCS(0) > 1) {
        t.Log(skippingAllocsPerRunˢ);
        return;
    }

    foreach (var (_, vᴛ1) in range(cleantests)) {
        ref var test = ref heap(new PathTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        var allocs = testing.AllocsPerRun(100, () => {
            Clean(testʗ1.result);
        });
        if (allocs > 0D) {
            t.Errorf("Clean(%q): %v allocs, want zero", test.result, allocs);
        }
    }
}

partial struct SplitTest {
    internal @string path, dir, @file;
}

internal static slice<SplitTest> splittests = [
    new("a/b", "a/", "b"),
    new("a/b/", "a/b/", ""),
    new("a/", "a/", ""),
    new("a", "", "a"),
    new("/", "/", "")
];

public static void TestSplit(ж<testing.T> t) {
    foreach (var (_, test) in range(splittests)) {
        var (d, f) = Split(test.path); if (d != test.dir || f != test.@file) {
            t.Errorf("Split(%q) = %q, %q, want %q, %q", test.path, d, f, test.dir, test.@file);
        }
    }
}

partial struct JoinTest {
    internal slice<@string> elem;
    internal @string path;
}

internal static slice<JoinTest> jointests = [
    // zero parameters
    new([], ""),

    // one parameter
    new([""], ""),
    new(["a"], "a"),

    // two parameters
    new(["a", "b"], "a/b"),
    new(["a", ""], "a"),
    new(["", "b"], "b"),
    new(["/", "a"], "/a"),
    new(["/", ""], "/"),
    new(["a/", "b"], "a/b"),
    new(["a/", ""], "a"),
    new(["", ""], "")
];

public static void TestJoin(ж<testing.T> t) {
    foreach (var (_, test) in range(jointests)) {
        @string p = Join(test.elem.ꓸꓸꓸ); if (p != test.path) {
            t.Errorf("Join(%q) = %q, want %q", test.elem, p, test.path);
        }
    }
}

partial struct ExtTest {
    internal @string path, ext;
}

internal static slice<ExtTest> exttests = [
    new("path.go", ".go"),
    new("path.pb.go", ".go"),
    new("a.dir/b", ""),
    new("a.dir/b.go", ".go"),
    new("a.dir/", "")
];

public static void TestExt(ж<testing.T> t) {
    foreach (var (_, test) in range(exttests)) {
        @string x = Ext(test.path); if (x != test.ext) {
            t.Errorf("Ext(%q) = %q, want %q", test.path, x, test.ext);
        }
    }
}

internal static slice<PathTest> basetests = [
    // Already clean
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
];

public static void TestBase(ж<testing.T> t) {
    foreach (var (_, test) in range(basetests)) {
        @string s = Base(test.path); if (s != test.result) {
            t.Errorf("Base(%q) = %q, want %q", test.path, s, test.result);
        }
    }
}

internal static slice<PathTest> dirtests = [
    new("", "."),
    new(".", "."),
    new("/.", "/"),
    new("/", "/"),
    new("////", "/"),
    new("/foo", "/"),
    new("x/", "x"),
    new("abc", "."),
    new("abc/def", "abc"),
    new("abc////def", "abc"),
    new("a/b/.x", "a/b"),
    new("a/b/c.", "a/b"),
    new("a/b/c.x", "a/b")
];

public static void TestDir(ж<testing.T> t) {
    foreach (var (_, test) in range(dirtests)) {
        @string s = Dir(test.path); if (s != test.result) {
            t.Errorf("Dir(%q) = %q, want %q", test.path, s, test.result);
        }
    }
}

partial struct IsAbsTest {
    internal @string path;
    internal bool isAbs;
}

internal static slice<IsAbsTest> isAbsTests = [
    new("", false),
    new("/", true),
    new("/usr/bin/gcc", true),
    new("..", false),
    new("/a/../bb", true),
    new(".", false),
    new("./", false),
    new("lala", false)
];

public static void TestIsAbs(ж<testing.T> t) {
    foreach (var (_, test) in range(isAbsTests)) {
        var r = IsAbs(test.path); if (r != test.isAbs) {
            t.Errorf("IsAbs(%q) = %v, want %v", test.path, r, test.isAbs);
        }
    }
}

public static void BenchmarkJoin(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var parts = slice<@string>(["one", "two", "three", "four"]);
    @string s = parts[0];
    while (Ꮡb.Loop()) {
        parts[0] = s;
        s = Δpath.Join(parts.ꓸꓸꓸ);
        s = s[..3];
    }
}

} // end path_test_package
