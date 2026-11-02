// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.io;

using static go.io.fs_package;
using os = os_package;
using pathpkg = path_package;
using filepath = go.path.filepath_package;
using slices = slices_package;
using testing = testing_package;
using fstest = go.testing.fstest_package;
using fs = go.io.fs_package;
using go.io;
using go.path;
using go.testing;

partial class fs_test_package {

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
        walkTree(e, pathpkg.Join(path, e.name), f);
    }
}

internal static fs.FS makeTree() {
    var fsys = new fstest.MapFS(new map<@string, ж<fstest.MapFile>>{});
    var fsysʗ1 = fsys;
    walkTree(tree, tree.name, (@string path, ж<Node> n) => {
        if (n.entries == nil) {
            fsysʗ1[path] = Ꮡ(new fstest.MapFile());
        } else {
            fsysʗ1[path] = Ꮡ(new fstest.MapFile(Mode: ModeDir));
        }
    });
    return new fstest_MapFSᴠFS(fsys);
}

// Assumes that each node name is unique. Good enough for a test.
// If clear is true, any incoming error is cleared before return. The errors
// are always accumulated, though.
internal static error mark(fs.DirEntry entry, error err, ж<slice<error>> Ꮡerrors, bool clear) {
    ref var errors = ref Ꮡerrors.DerefOrNull();

    @string name = entry.Name();
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

public static void TestWalkDir(ж<testing.T> t) {
    t.Chdir(t.TempDir());

    var fsys = makeTree();
    ref var errors = ref heap<slice<error>>(new slice<error>(0, 10), out var Ꮡerrors);
    var clear = true;
    var markFn = (@string path, fs.DirEntry entry, error errΔ1) => mark(entry, errΔ1, Ꮡerrors, clear);
    // Expect no errors.
    var err = WalkDir(fsys, ".", new Func<@string, fs.DirEntry, error, error>(markFn));
    if (err != nil) {
        t.Fatalf("no error expected, found: %s", err);
    }
    if (len(errors) != 0) {
        t.Fatalf("unexpected errors: %s", errors);
    }
    walkTree(tree, tree.name, (@string path, ж<Node> n) => {
        if (n.mark != 1) {
            t.Errorf("node %s mark = %d; expected 1", path, n.mark);
        }
        n.mark = 0;
    });
}

public static void TestWalkDirSymlink(ж<testing.T> t) {
    var fsys = new fstest.MapFS(new map<@string, ж<fstest.MapFile>>{
        ["link"] = Ꮡ(new fstest.MapFile(Data: slice<byte>("dir"u8), Mode: ModeSymlink)),
        ["dir/a"] = Ꮡ(new fstest.MapFile()),
        ["dir/b/c"] = Ꮡ(new fstest.MapFile()),
        ["dir/d"] = Ꮡ(new fstest.MapFile(Data: slice<byte>("b"u8), Mode: ModeSymlink))
    });

    var wantTypes = new map<@string, fs.FileMode>{
        ["link"] = ModeDir,
        ["link/a"] = 0,
        ["link/b"] = ModeDir,
        ["link/b/c"] = 0,
        ["link/d"] = ModeSymlink
    };
    var marks = new map<@string, nint>();
    var marksʗ1 = marks;
    var wantTypesʗ1 = wantTypes;
    var walkFn = error (@string path, fs.DirEntry entry, error errΔ1) => {
        marksʗ1[path]++;
        {
            var (want, ok) = wantTypesʗ1[path, ꟷ]; if (!ok) {
                t.Errorf("Unexpected path %q in walk", path);
            } else {
                var got = entry.Type(); if (got != want) {
                    t.Errorf("%s entry type = %v; want %v", path, got, want);
                }
            }
        }
        if (errΔ1 != nil) {
            t.Errorf("Walking %s: %v", path, errΔ1);
        }
        return default;
    };

    // Expect no errors.
    var err = WalkDir(new fstest_MapFSᴠFS(fsys), linkˢ, new Func<@string, fs.DirEntry, error, error>(walkFn));
    if (err != nil) {
        t.Fatalf("no error expected, found: %s", err);
    }
    foreach (var (path, _) in wantTypes) {
        {
            nint got = marks[path]; if (got != 1) {
                t.Errorf("%s visited %d times; expected 1", path, got);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string badˢ => ref badˢᵇ;
private static readonly @string badˢᵇ = "bad"u8;
private static ref readonly @string nextˢ => ref nextˢᵇ;
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
        var err = WalkDir(os.DirFS(dir), ".", error (@string path, fs.DirEntry d, error errΔ3) => {
            if (errΔ3 != nil) {
                return filepath.SkipDir;
            }
            if (d.IsDir()) {
                Ꮡsaw.ValueSlot = append(Ꮡsaw.ValueSlot, path);
            }
            return default;
        });
        if (err != nil) {
            t.Fatal(err);
        }
        var want = slice<@string>([".", "a", "a/bad", "a/next"]);
        if (!slices.Equal<slice<@string>, @string>(saw, want)) {
            t.Errorf("got directories %v, want %v", saw, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end fs_test_package
