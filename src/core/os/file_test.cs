// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testenv = @internal.testenv_package;
using fs = go.io.fs_package;
using static os_package;
using filepath = go.path.filepath_package;
using Δtesting = testing_package;
using @internal;
using go.io;
using go.path;
using static go.os_internal_test_package;

partial class os_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dirˢ = "dir"u8;
internal static readonly @string parentLinkˢ = "parent-link"u8;
internal static readonly @string fooˢ = "foo"u8;
internal static readonly @string sneakyParentLinkˢ = "sneaky-parent-link"u8;
internal static readonly @string absLinkˢ = "abs-link"u8;
internal static readonly @string relLinkˢ = "rel-link"u8;
internal static readonly @string relSubLinkˢ = "rel-sub-link"u8;

public static void TestDirFSReadLink(ж<Δtesting.T> Ꮡt) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(Ꮡt));

    @string root = Ꮡt.TempDir();
    @string subdir = filepath.Join(root, dirˢ);
    {
        var err = Mkdir(subdir, 511); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    var links = new map<@string, @string>{
        [filepath.Join(root, parentLinkˢ)] = filepath.Join(".."u8, fooˢ),
        [filepath.Join(root, sneakyParentLinkˢ)] = filepath.Join(dirˢ, "..", "..", fooˢ),
        [filepath.Join(root, absLinkˢ)] = filepath.Join(root, fooˢ),
        [filepath.Join(root, relLinkˢ)] = "foo"u8,
        [filepath.Join(root, relSubLinkˢ)] = filepath.Join(dirˢ, fooˢ),
        [filepath.Join(subdir, parentLinkˢ)] = filepath.Join(".."u8, fooˢ)
    };
    foreach (var (newname, oldname) in links) {
        {
            var err = Symlink(oldname, newname); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
    }

    var fsys = DirFS(root);
    var want = new map<@string, @string>{
        ["rel-link"u8] = "foo"u8,
        ["rel-sub-link"u8] = filepath.Join(dirˢ, fooˢ),
        ["dir/parent-link"u8] = filepath.Join(".."u8, fooˢ),
        ["parent-link"u8] = filepath.Join(".."u8, fooˢ),
        ["sneaky-parent-link"u8] = filepath.Join(dirˢ, "..", "..", fooˢ),
        ["abs-link"u8] = filepath.Join(root, fooˢ)
    };
    foreach (var (name, wantΔ1) in want) {
        var (got, err) = fs.ReadLink(fsys, name);
        if (got != wantΔ1 || err != default!) {
            Ꮡt.Errorf("fs.ReadLink(fsys, %q) = %q, %v; want %q, <nil>"u8, name, got, err, wantΔ1);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string linkˢ = "link"u8;

public static void TestDirFSLstat(ж<Δtesting.T> Ꮡt) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(Ꮡt));

    @string root = Ꮡt.TempDir();
    @string subdir = filepath.Join(root, dirˢ);
    {
        var err = Mkdir(subdir, 511); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        var err = Symlink(dirˢ, filepath.Join(root, linkˢ)); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }

    var fsys = DirFS(root);
    var want = new map<@string, fs.FileMode>{
        ["link"u8] = fs.ModeSymlink,
        ["dir"u8] = fs.ModeDir
    };
    foreach (var (name, wantΔ1) in want) {
        var (info, err) = fs.Lstat(fsys, name);
        fs.FileMode got = default!;
        if (info != default!) {
            got = info.Mode().Type();
        }
        if (got != wantΔ1 || err != default!) {
            Ꮡt.Errorf("fs.Lstat(fsys, %q).Mode().Type() = %v, %v; want %v, <nil>"u8, name, got, err, wantΔ1);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string symlinkRootˢ = "SymlinkRoot"u8;
internal static readonly @string symlinkPresentˢ = "SymlinkPresent"u8;

public static void TestDirFSWalkDir(ж<Δtesting.T> Ꮡt) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(Ꮡt));

    @string root = Ꮡt.TempDir();
    @string subdir = filepath.Join(root, dirˢ);
    {
        var err = Mkdir(subdir, 511); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        var err = Symlink(dirˢ, filepath.Join(root, linkˢ)); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        var err = WriteFile(filepath.Join(root, dirˢ, "a"), default!, 438); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    var fsys = DirFS(root);

    var fsysʗ1 = fsys;
    Ꮡt.Run(symlinkRootˢ, (ж<Δtesting.T> tΔ1) => {
        var wantTypes = new map<@string, fs.FileMode>{
            ["link"u8] = fs.ModeDir,
            ["link/a"u8] = 0
        };
        var marks = new map<@string, nint>();
        var marksʗ1 = marks;
        var wantTypesʗ1 = wantTypes;
        var err = fs.WalkDir(fsysʗ1, linkˢ, (@string path, fs.DirEntry entry, error errΔ1) => {
            marksʗ1[path]++;
            {
                var (want, ok) = wantTypesʗ1[path, ꟷ]; if (!ok){
                    tΔ1.Errorf("Unexpected path %q in walk"u8, path);
                } else 
                {
                    var got = entry.Type(); if (got != want) {
                        tΔ1.Errorf("%s entry type = %v; want %v"u8, path, got, want);
                    }
                }
            }
            if (errΔ1 != default!) {
                tΔ1.Errorf("%s: %v"u8, path, errΔ1);
            }
            return default!;
        });
        if (err != default!) {
            tΔ1.Fatal(err);
        }
        foreach (var (path, _) in wantTypes) {
            {
                nint got = marks[path]; if (got != 1) {
                    tΔ1.Errorf("%s visited %d times; expected 1"u8, path, got);
                }
            }
        }
    });

    var fsysʗ2 = fsys;
    Ꮡt.Run(symlinkPresentˢ, (ж<Δtesting.T> tΔ2) => {
        var wantTypes = new map<@string, fs.FileMode>{
            ["."u8] = fs.ModeDir,
            ["dir"u8] = fs.ModeDir,
            ["dir/a"u8] = 0,
            ["link"u8] = fs.ModeSymlink
        };
        var marks = new map<@string, nint>();
        var marksʗ2 = marks;
        var wantTypesʗ2 = wantTypes;
        var err = fs.WalkDir(fsysʗ2, "."u8, (@string path, fs.DirEntry entry, error errΔ1) => {
            marksʗ2[path]++;
            {
                var (want, ok) = wantTypesʗ2[path, ꟷ]; if (!ok){
                    tΔ2.Errorf("Unexpected path %q in walk"u8, path);
                } else 
                {
                    var got = entry.Type(); if (got != want) {
                        tΔ2.Errorf("%s entry type = %v; want %v"u8, path, got, want);
                    }
                }
            }
            if (errΔ1 != default!) {
                tΔ2.Errorf("%s: %v"u8, path, errΔ1);
            }
            return default!;
        });
        if (err != default!) {
            tΔ2.Fatal(err);
        }
        foreach (var (path, _) in wantTypes) {
            {
                nint got = marks[path]; if (got != 1) {
                    tΔ2.Errorf("%s visited %d times; expected 1"u8, path, got);
                }
            }
        }
    });
}

} // end os_test_package
