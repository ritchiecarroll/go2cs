// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static path_package;
using Δruntime = runtime_package;
using testing = testing_package;

partial class path_test_package {

partial struct PathTest {
    internal @string path, result;
}

// Already clean
// Remove trailing slash
// Remove doubled slash
// Remove . elements
// Remove .. elements
// Combinations
internal static slice<PathTest> cleantests = new PathTest[]{
    new(""u8, "."u8),
    new("abc"u8, "abc"u8),
    new("abc/def"u8, "abc/def"u8),
    new("a/b/c"u8, "a/b/c"u8),
    new("."u8, "."u8),
    new(".."u8, ".."u8),
    new("../.."u8, "../.."u8),
    new("../../abc"u8, "../../abc"u8),
    new("/abc"u8, "/abc"u8),
    new("/"u8, "/"u8),
    new("abc/"u8, "abc"u8),
    new("abc/def/"u8, "abc/def"u8),
    new("a/b/c/"u8, "a/b/c"u8),
    new("./"u8, "."u8),
    new("../"u8, ".."u8),
    new("../../"u8, "../.."u8),
    new("/abc/"u8, "/abc"u8),
    new("abc//def//ghi"u8, "abc/def/ghi"u8),
    new("//abc"u8, "/abc"u8),
    new("///abc"u8, "/abc"u8),
    new("//abc//"u8, "/abc"u8),
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
    new("abc/./../def"u8, "def"u8),
    new("abc//./../def"u8, "def"u8),
    new("abc/../../././../def"u8, "../../def"u8)
}.slice();

public static void TestClean(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in cleantests) {
        {
            @string s = Clean(test.path); if (s != test.result) {
                Ꮡt.Errorf("Clean(%q) = %q, want %q"u8, test.path, s, test.result);
            }
        }
        {
            @string s = Clean(test.result); if (s != test.result) {
                Ꮡt.Errorf("Clean(%q) = %q, want %q"u8, test.result, s, test.result);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object skippingMallocCountInˢ = (@string)"skipping malloc count in short mode"u8;
private static readonly object skippingAllocsPerRunˢ = (@string)"skipping AllocsPerRun checks; GOMAXPROCS>1"u8;

public static void TestCleanMallocs(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingMallocCountInˢ);
    }
    if (Δruntime.GOMAXPROCS(0) > 1) {
        Ꮡt.Log(skippingAllocsPerRunˢ);
        return;
    }
    foreach (var (_, vᴛ1) in cleantests) {
        ref var test = ref heap(new PathTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        var allocs = testing.AllocsPerRun(100, () => {
            Clean(testʗ1.result);
        });
        if (allocs > 0D) {
            Ꮡt.Errorf("Clean(%q): %v allocs, want zero"u8, test.result, allocs);
        }
    }
}

partial struct SplitTest {
    internal @string path, dir, @file;
}

internal static slice<SplitTest> splittests = new SplitTest[]{
    new("a/b"u8, "a/"u8, "b"u8),
    new("a/b/"u8, "a/b/"u8, ""u8),
    new("a/"u8, "a/"u8, ""u8),
    new("a"u8, ""u8, "a"u8),
    new("/"u8, "/"u8, ""u8)
}.slice();

public static void TestSplit(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in splittests) {
        {
            var (d, f) = Split(test.path); if (d != test.dir || f != test.@file) {
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
internal static slice<JoinTest> jointests = new JoinTest[]{
    new(new @string[]{}.slice(), ""u8),
    new(new @string[]{""u8}.slice(), ""u8),
    new(new @string[]{"a"u8}.slice(), "a"u8),
    new(new @string[]{"a"u8, "b"u8}.slice(), "a/b"u8),
    new(new @string[]{"a"u8, ""u8}.slice(), "a"u8),
    new(new @string[]{""u8, "b"u8}.slice(), "b"u8),
    new(new @string[]{"/"u8, "a"u8}.slice(), "/a"u8),
    new(new @string[]{"/"u8, ""u8}.slice(), "/"u8),
    new(new @string[]{"a/"u8, "b"u8}.slice(), "a/b"u8),
    new(new @string[]{"a/"u8, ""u8}.slice(), "a"u8),
    new(new @string[]{""u8, ""u8}.slice(), ""u8)
}.slice();

public static void TestJoin(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in jointests) {
        {
            @string p = Join(test.elem.ꓸꓸꓸ); if (p != test.path) {
                Ꮡt.Errorf("Join(%q) = %q, want %q"u8, test.elem, p, test.path);
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
            @string x = Ext(test.path); if (x != test.ext) {
                Ꮡt.Errorf("Ext(%q) = %q, want %q"u8, test.path, x, test.ext);
            }
        }
    }
}

// Already clean
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

public static void TestBase(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in basetests) {
        {
            @string s = Base(test.path); if (s != test.result) {
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
    new("////"u8, "/"u8),
    new("/foo"u8, "/"u8),
    new("x/"u8, "x"u8),
    new("abc"u8, "."u8),
    new("abc/def"u8, "abc"u8),
    new("abc////def"u8, "abc"u8),
    new("a/b/.x"u8, "a/b"u8),
    new("a/b/c."u8, "a/b"u8),
    new("a/b/c.x"u8, "a/b"u8)
}.slice();

public static void TestDir(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in dirtests) {
        {
            @string s = Dir(test.path); if (s != test.result) {
                Ꮡt.Errorf("Dir(%q) = %q, want %q"u8, test.path, s, test.result);
            }
        }
    }
}

partial struct IsAbsTest {
    internal @string path;
    internal bool isAbs;
}

internal static slice<IsAbsTest> isAbsTests = new IsAbsTest[]{
    new(""u8, false),
    new("/"u8, true),
    new("/usr/bin/gcc"u8, true),
    new(".."u8, false),
    new("/a/../bb"u8, true),
    new("."u8, false),
    new("./"u8, false),
    new("lala"u8, false)
}.slice();

public static void TestIsAbs(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in isAbsTests) {
        {
            var r = IsAbs(test.path); if (r != test.isAbs) {
                Ꮡt.Errorf("IsAbs(%q) = %v, want %v"u8, test.path, r, test.isAbs);
            }
        }
    }
}

} // end path_test_package
