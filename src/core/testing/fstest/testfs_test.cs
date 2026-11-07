// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.testing;

using errors = errors_package;
using testenv = @internal.testenv_package;
using fs = go.io.fs_package;
using os = os_package;
using filepath = go.path.filepath_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using go.io;
using go.path;
using static go.testing.fstest_package;

partial class fstest_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloLinkˢ => ref helloLinkˢᵇ;
private static readonly @string helloLinkˢᵇ = "hello.link"u8;
internal static ref readonly @string helloRelLinkˢ => ref helloRelLinkˢᵇ;
private static readonly @string helloRelLinkˢᵇ = "hello_rel.link"u8;

public static void TestSymlink(ж<testing.T> t) {
    testenv.MustHaveSymlink(new fstest_internal_test_package.testing_TжTB(t));

    @string tmp = t.TempDir();
    var tmpfs = os.DirFS(tmp);

    {
        var err = os.WriteFile(filepath.Join(tmp, helloˢ), slice<byte>("hello, world\n"u8), 420); if (err != nil) {
            t.Fatal(err);
        }
    }

    {
        var err = os.Symlink(filepath.Join(tmp, helloˢ), filepath.Join(tmp, helloLinkˢ)); if (err != nil) {
            t.Fatal(err);
        }
    }
    {
        var err = os.Symlink(helloˢ, filepath.Join(tmp, helloRelLinkˢ)); if (err != nil) {
            t.Fatal(err);
        }
    }

    {
        var err = TestFS(tmpfs, helloˢ, helloLinkˢ); if (err != nil) {
            t.Fatal(err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aBAˢ => ref aBAˢᵇ;
private static readonly @string aBAˢᵇ = "a-b/a"u8;

public static void TestDash(ж<testing.T> t) {
    var m = new MapFS(new map<@string, ж<global::go.testing.fstest_package.MapFile>>{
        ["a-b/a"] = Ꮡ(new global::go.testing.fstest_package.MapFile(Data: slice<byte>("a-b/a"u8)))
    });
    {
        var err = TestFS(new fstest_internal_test_package.fstest_MapFSᴠFS(m), aBAˢ); if (err != nil) {
            t.Error(err);
        }
    }
}

internal partial struct shuffledFS /*global::go.testing.fstest_package.MapFS*/;

internal static (fs.File, error) Open(this shuffledFS fsys, @string name) {
    var (f, err) = ((global::go.testing.fstest_package.MapFS)fsys).Open(name);
    if (err != nil) {
        return (default, err);
    }
    return (new fstest_internal_test_package.shuffledFileжFile(Ꮡ(new shuffledFile(File: f))), default);
}

internal partial struct shuffledFile {
    /*embed*/ public go.io.fs_package.File File;
}

internal static (slice<fs.DirEntry>, error) ReadDir(this ref shuffledFile f, nint n) {
    var (dirents, err) = f.File._<fs.ReadDirFile>().ReadDir(n);
    // Shuffle in a deterministic way, all we care about is making sure that the
    // list of directory entries is not is the lexicographic order.
    //
    // We do this to make sure that the TestFS test suite is not affected by the
    // order of directory entries.
    slices.SortFunc(dirents, (fs.DirEntry a, fs.DirEntry b) => strings.Compare(b.Name(), a.Name()));
    return (dirents, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tmpOneˢ => ref tmpOneˢᵇ;
private static readonly @string tmpOneˢᵇ = "tmp/one"u8;
internal static ref readonly @string tmpTwoˢ => ref tmpTwoˢᵇ;
private static readonly @string tmpTwoˢᵇ = "tmp/two"u8;
internal static ref readonly @string tmpThreeˢ => ref tmpThreeˢᵇ;
private static readonly @string tmpThreeˢᵇ = "tmp/three"u8;

public static void TestShuffledFS(ж<testing.T> t) {
    var fsys = new shuffledFS(new map<@string, ж<global::go.testing.fstest_package.MapFile>>{
        ["tmp/one"] = Ꮡ(new global::go.testing.fstest_package.MapFile(Data: slice<byte>("1"u8))),
        ["tmp/two"] = Ꮡ(new global::go.testing.fstest_package.MapFile(Data: slice<byte>("2"u8))),
        ["tmp/three"] = Ꮡ(new global::go.testing.fstest_package.MapFile(Data: slice<byte>("3"u8)))
    });
    {
        var err = TestFS(fsys, tmpOneˢ, tmpTwoˢ, tmpThreeˢ); if (err != nil) {
            t.Error(err);
        }
    }
}

// failPermFS is a filesystem that always fails with fs.ErrPermission.
internal partial struct failPermFS {
}

internal static (fs.File, error) Open(this failPermFS f, @string name) {
    if (!fs.ValidPath(name)) {
        return (default, new fs.PathErrorжerror(Ꮡ(new fs.PathError(Op: "open", Path: name, Err: fs.ErrInvalid))));
    }
    return (default, new fs.PathErrorжerror(Ꮡ(new fs.PathError(Op: "open", Path: name, Err: fs.ErrPermission))));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errorExpectedˢ = (@string)"error expected"u8;

// TestFS is expected to return a list of errors.
// Enforce that the list can be extracted for browsing.
internal partial interface TestTestFSWrappedErrors_wrapper /*dyn*/ :
    error
{
    slice<error> Unwrap();
}

public static void TestTestFSWrappedErrors(ж<testing.T> t) {
    var err = TestFS(new failPermFS());
    if (err == nil) {
        t.Fatal(errorExpectedˢ);
    }
    t.Logf("Error (expecting wrapped fs.ErrPermission):\n%v", err);

    if (!errors.Is(err, fs.ErrPermission)) {
        t.Errorf("error should be a wrapped ErrPermission: %#v", err);
    }
    var (errs, ok) = errors.AsType<TestTestFSWrappedErrors_wrapper>(err);
    if (!ok) {
        t.Errorf("caller should be able to extract the errors as a list: %#v", err);
    } else {
        foreach (var (_, errΔ1) in range(errs.Unwrap())) {
            // ErrPermission is expected
            // but any other error must be reported.
            if (!errors.Is(errΔ1, fs.ErrPermission)) {
                t.Errorf("unexpected error: %v", errΔ1);
            }
        }
    }
}

} // end fstest_internal_test_package
