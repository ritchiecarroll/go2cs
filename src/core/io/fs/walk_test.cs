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
        walkTree(e, pathpkg.Join(path, (~e).name), f);
    }
}

internal static fs.FS makeTree() {
    var fsys = new fstest.MapFS(new map<@string, ж<fstest.MapFile>>{});
    var fsysʗ1 = fsys;
    walkTree(tree, (~tree).name, (@string path, ж<Node> n) => {
        if ((~n).entries == default!){
            fsysʗ1[path] = Ꮡ(new fstest.MapFile(nil));
        } else {
            fsysʗ1[path] = Ꮡ(new fstest.MapFile(Mode: ModeDir));
        }
    });
    return fsys;
}

// Assumes that each node name is unique. Good enough for a test.
// If clear is true, any incoming error is cleared before return. The errors
// are always accumulated, though.
internal static error mark(fs.DirEntry entry, error err, ж<slice<error>> Ꮡerrors, bool clear) {
    ref var errors = ref Ꮡerrors.DerefOrNull();

    @string name = entry.Name();
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

public static void TestWalkDir(ж<testing.T> Ꮡt) {
    Ꮡt.Chdir(Ꮡt.TempDir());
    var fsys = makeTree();
    ref var errors = ref heap<slice<error>>(out var Ꮡerrors);
    errors = new slice<error>(0, 10);
    var clear = true;
    var markFn = (@string path, fs.DirEntry entry, error errΔ1) => mark(entry, errΔ1, Ꮡerrors, clear);
    // Expect no errors.
    var err = WalkDir(fsys, "."u8, new Func<@string, fs.DirEntry, error, error>(markFn));
    if (err != default!) {
        Ꮡt.Fatalf("no error expected, found: %s"u8, err);
    }
    if (len(errors) != 0) {
        Ꮡt.Fatalf("unexpected errors: %s"u8, errors);
    }
    walkTree(tree, (~tree).name, (@string path, ж<Node> n) => {
        if ((~n).mark != 1) {
            Ꮡt.Errorf("node %s mark = %d; expected 1"u8, path, (~n).mark);
        }
        n.Value.mark = 0;
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string badˢ = "bad"u8;
private static readonly @string nextˢ = "next"u8;

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
        var err = WalkDir(os.DirFS(dir), "."u8, error (@string path, fs.DirEntry d, error errΔ3) => {
            if (errΔ3 != default!) {
                return filepath.SkipDir;
            }
            if (d.IsDir()) {
                Ꮡsaw.ValueSlot = append(Ꮡsaw.ValueSlot, path);
            }
            return default!;
        });
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var want = new @string[]{"."u8, "a"u8, "a/bad"u8, "a/next"u8}.slice();
        if (!slices.Equal<slice<@string>, @string>(saw, want)) {
            Ꮡt.Errorf("got directories %v, want %v"u8, saw, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end fs_test_package
