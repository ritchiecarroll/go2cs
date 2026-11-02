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
internal static ref readonly @string dirˢ => ref dirˢᵇ;
private static readonly @string dirˢᵇ = "dir"u8;
internal static ref readonly @string parentLinkˢ => ref parentLinkˢᵇ;
private static readonly @string parentLinkˢᵇ = "parent-link"u8;
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;
internal static ref readonly @string sneakyParentLinkˢ => ref sneakyParentLinkˢᵇ;
private static readonly @string sneakyParentLinkˢᵇ = "sneaky-parent-link"u8;
internal static ref readonly @string absLinkˢ => ref absLinkˢᵇ;
private static readonly @string absLinkˢᵇ = "abs-link"u8;
internal static ref readonly @string relLinkˢ => ref relLinkˢᵇ;
private static readonly @string relLinkˢᵇ = "rel-link"u8;
internal static ref readonly @string relSubLinkˢ => ref relSubLinkˢᵇ;
private static readonly @string relSubLinkˢᵇ = "rel-sub-link"u8;

public static void TestDirFSReadLink(ж<Δtesting.T> t) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(t));

    @string root = t.TempDir();
    @string subdir = filepath.Join(root, dirˢ);
    {
        var err = Mkdir(subdir, 511); if (err != nil) {
            t.Fatal(err);
        }
    }
    var links = new map<@string, @string>{
        [filepath.Join(root, parentLinkˢ)] = filepath.Join("..", fooˢ),
        [filepath.Join(root, sneakyParentLinkˢ)] = filepath.Join(dirˢ, "..", "..", fooˢ),
        [filepath.Join(root, absLinkˢ)] = filepath.Join(root, fooˢ),
        [filepath.Join(root, relLinkˢ)] = "foo",
        [filepath.Join(root, relSubLinkˢ)] = filepath.Join(dirˢ, fooˢ),
        [filepath.Join(subdir, parentLinkˢ)] = filepath.Join("..", fooˢ)
    };
    foreach (var (newname, oldname) in links) {
        {
            var err = Symlink(oldname, newname); if (err != nil) {
                t.Fatal(err);
            }
        }
    }

    var fsys = DirFS(root);
    var want = new map<@string, @string>{
        ["rel-link"] = "foo",
        ["rel-sub-link"] = filepath.Join(dirˢ, fooˢ),
        ["dir/parent-link"] = filepath.Join("..", fooˢ),
        ["parent-link"] = filepath.Join("..", fooˢ),
        ["sneaky-parent-link"] = filepath.Join(dirˢ, "..", "..", fooˢ),
        ["abs-link"] = filepath.Join(root, fooˢ)
    };
    foreach (var (name, wantΔ1) in want) {
        var (got, err) = fs.ReadLink(fsys, name);
        if (got != wantΔ1 || err != nil) {
            t.Errorf("fs.ReadLink(fsys, %q) = %q, %v; want %q, <nil>", name, got, err, wantΔ1);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string linkˢ => ref linkˢᵇ;
private static readonly @string linkˢᵇ = "link"u8;

public static void TestDirFSLstat(ж<Δtesting.T> t) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(t));

    @string root = t.TempDir();
    @string subdir = filepath.Join(root, dirˢ);
    {
        var err = Mkdir(subdir, 511); if (err != nil) {
            t.Fatal(err);
        }
    }
    {
        var err = Symlink(dirˢ, filepath.Join(root, linkˢ)); if (err != nil) {
            t.Fatal(err);
        }
    }

    var fsys = DirFS(root);
    var want = new map<@string, fs.FileMode>{
        ["link"] = fs.ModeSymlink,
        ["dir"] = fs.ModeDir
    };
    foreach (var (name, wantΔ1) in want) {
        var (info, err) = fs.Lstat(fsys, name);
        fs.FileMode got = default;
        if (info != nil) {
            got = info.Mode().Type();
        }
        if (got != wantΔ1 || err != nil) {
            t.Errorf("fs.Lstat(fsys, %q).Mode().Type() = %v, %v; want %v, <nil>", name, got, err, wantΔ1);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string symlinkRootˢ => ref symlinkRootˢᵇ;
private static readonly @string symlinkRootˢᵇ = "SymlinkRoot"u8;
internal static ref readonly @string symlinkPresentˢ => ref symlinkPresentˢᵇ;
private static readonly @string symlinkPresentˢᵇ = "SymlinkPresent"u8;

public static void TestDirFSWalkDir(ж<Δtesting.T> t) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(t));

    @string root = t.TempDir();
    @string subdir = filepath.Join(root, dirˢ);
    {
        var err = Mkdir(subdir, 511); if (err != nil) {
            t.Fatal(err);
        }
    }
    {
        var err = Symlink(dirˢ, filepath.Join(root, linkˢ)); if (err != nil) {
            t.Fatal(err);
        }
    }
    {
        var err = WriteFile(filepath.Join(root, dirˢ, "a"), default, 438); if (err != nil) {
            t.Fatal(err);
        }
    }
    var fsys = DirFS(root);

    var fsysʗ1 = fsys;
    t.Run(symlinkRootˢ, (ж<Δtesting.T> tΔ1) => {
        var wantTypes = new map<@string, fs.FileMode>{
            ["link"] = fs.ModeDir,
            ["link/a"] = 0
        };
        var marks = new map<@string, nint>();
        var marksʗ1 = marks;
        var wantTypesʗ1 = wantTypes;
        var err = fs.WalkDir(fsysʗ1, linkˢ, (@string path, fs.DirEntry entry, error errΔ1) => {
            marksʗ1[path]++;
            {
                var (want, ok) = wantTypesʗ1[path, ꟷ]; if (!ok) {
                    tΔ1.Errorf("Unexpected path %q in walk", path);
                } else {
                    var got = entry.Type(); if (got != want) {
                        tΔ1.Errorf("%s entry type = %v; want %v", path, got, want);
                    }
                }
            }
            if (errΔ1 != nil) {
                tΔ1.Errorf("%s: %v", path, errΔ1);
            }
            return default;
        });
        if (err != nil) {
            tΔ1.Fatal(err);
        }
        foreach (var (path, _) in wantTypes) {
            {
                nint got = marks[path]; if (got != 1) {
                    tΔ1.Errorf("%s visited %d times; expected 1", path, got);
                }
            }
        }
    });

    var fsysʗ2 = fsys;
    t.Run(symlinkPresentˢ, (ж<Δtesting.T> tΔ2) => {
        var wantTypes = new map<@string, fs.FileMode>{
            ["."] = fs.ModeDir,
            ["dir"] = fs.ModeDir,
            ["dir/a"] = 0,
            ["link"] = fs.ModeSymlink
        };
        var marks = new map<@string, nint>();
        var marksʗ2 = marks;
        var wantTypesʗ2 = wantTypes;
        var err = fs.WalkDir(fsysʗ2, ".", (@string path, fs.DirEntry entry, error errΔ1) => {
            marksʗ2[path]++;
            {
                var (want, ok) = wantTypesʗ2[path, ꟷ]; if (!ok) {
                    tΔ2.Errorf("Unexpected path %q in walk", path);
                } else {
                    var got = entry.Type(); if (got != want) {
                        tΔ2.Errorf("%s entry type = %v; want %v", path, got, want);
                    }
                }
            }
            if (errΔ1 != nil) {
                tΔ2.Errorf("%s: %v", path, errΔ1);
            }
            return default;
        });
        if (err != nil) {
            tΔ2.Fatal(err);
        }
        foreach (var (path, _) in wantTypes) {
            {
                nint got = marks[path]; if (got != 1) {
                    tΔ2.Errorf("%s visited %d times; expected 1", path, got);
                }
            }
        }
    });
}

} // end os_test_package
