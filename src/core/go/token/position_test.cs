// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using rand = math.rand_package;
using slices = slices_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using io = io_package;
using math;
using static global::go.go.token_package;

partial class token_internal_test_package {

internal static void checkPos(ж<testing.T> t, @string msg, global::go.go.token_package.ΔPosition got, global::go.go.token_package.ΔPosition want) {
    if (got.Filename != want.Filename) {
        t.Errorf("%s: got filename = %q; want %q", msg, got.Filename, want.Filename);
    }
    if (got.Offset != want.Offset) {
        t.Errorf("%s: got offset = %d; want %d", msg, got.Offset, want.Offset);
    }
    if (got.Line != want.Line) {
        t.Errorf("%s: got line = %d; want %d", msg, got.Line, want.Line);
    }
    if (got.Column != want.Column) {
        t.Errorf("%s: got column = %d; want %d", msg, got.Column, want.Column);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nilNoPosˢ => ref nilNoPosˢᵇ;
private static readonly @string nilNoPosˢᵇ = "nil NoPos"u8;
internal static ref readonly @string fsetNoPosˢ => ref fsetNoPosˢᵇ;
private static readonly @string fsetNoPosˢᵇ = "fset NoPos"u8;

public static void TestNoPos(ж<testing.T> t) {
    if (NoPos.IsValid()) {
        t.Errorf("NoPos should not be valid");
    }
    ж<global::go.go.token_package.FileSet> fset = default;
    checkPos(t, nilNoPosˢ, fset.Position(NoPos), new ΔPosition());
    fset = NewFileSet();
    checkPos(t, fsetNoPosˢ, fset.Position(NoPos), new ΔPosition());
}

partial struct testsᴛ1 /*dyn*/ {
    internal @string filename;
    internal slice<byte> source; // may be nil
    internal nint size;
    internal slice<nint> lines;
}
internal static slice<testsᴛ1> tests = [
    new("a", [], 0, []),
    new("b", slice<byte>("01234"u8), 5, [0]),
    new("c", slice<byte>("\n\n\n\n\n\n\n\n\n"u8), 9, [0, 1, 2, 3, 4, 5, 6, 7, 8]),
    new("d", default, 100, [0, 5, 10, 20, 30, 70, 71, 72, 80, 85, 90, 99]),
    new("e", default, 777, [0, 80, 100, 120, 130, 180, 267, 455, 500, 567, 620]),
    new("f", slice<byte>("package p\n\nimport \"fmt\""u8), 23, [0, 10, 11]),
    new("g", slice<byte>("package p\n\nimport \"fmt\"\n"u8), 24, [0, 10, 11]),
    new("h", slice<byte>("package p\n\nimport \"fmt\"\n "u8), 25, [0, 10, 11, 24])
];

internal static (nint, nint) linecol(slice<nint> lines, nint offs) {
    nint prevLineOffs = 0;
    foreach (var (line, lineOffs) in range(lines)) {
        if (offs < lineOffs) {
            return (line, offs - prevLineOffs + 1);
        }
        prevLineOffs = lineOffs;
    }
    return (len(lines), offs - prevLineOffs + 1);
}

internal static void verifyPositions(ж<testing.T> t, ж<global::go.go.token_package.FileSet> fset, ж<global::go.go.token_package.ΔFile> Ꮡf, slice<nint> lines) {
    ref var f = ref Ꮡf.DerefOrNull();

    for (nint offs = 0; offs < f.Size(); offs++) {
        global::go.go.token_package.ΔPos p = f.Pos(offs);
        nint offs2 = f.Offset(p);
        if (offs2 != offs) {
            t.Errorf("%s, Offset: got offset %d; want %d", f.Name(), offs2, offs);
        }
        var (line, col) = linecol(lines, offs);
        @string msg = fmt.Sprintf("%s (offs = %d, p = %d)"u8, f.Name(), offs, p);
        checkPos(t, msg, Ꮡf.Position(f.Pos(offs)), new ΔPosition(f.Name(), offs, line, col));
        checkPos(t, msg, fset.Position(p), new ΔPosition(f.Name(), offs, line, col));
    }
}

internal static slice<byte> makeTestSource(nint size, slice<nint> lines) {
    var src = new slice<byte>(size);
    foreach (var (_, offs) in range(lines)) {
        if (offs > 0) {
            src[offs - 1] = (byte)'\n';
        }
    }
    return src;
}

public static void TestPositions(ж<testing.T> t) {
    const nint delta = 7; // a non-zero base offset increment
    var fset = NewFileSet();
    foreach (var (_, test) in range(tests)) {
        // verify consistency of test case
        if (test.source != nil && len(test.source) != test.size) {
            t.Errorf("%s: inconsistent test case: got file size %d; want %d", test.filename, len(test.source), test.size);
        }

        // add file and verify name and size
        var f = fset.AddFile(test.filename, fset.Base() + delta, test.size);
        if (f.Name() != test.filename) {
            t.Errorf("got filename %q; want %q", f.Name(), test.filename);
        }
        if (f.Size() != test.size) {
            t.Errorf("%s: got file size %d; want %d", f.Name(), f.Size(), test.size);
        }
        if (fset.File(f.Pos(0)) != f) {
            t.Errorf("%s: f.Pos(0) was not found in f", f.Name());
        }

        // add lines individually and verify all positions
        foreach (var (i, offset) in range(test.lines)) {
            f.AddLine(offset);
            if (f.LineCount() != i + 1) {
                t.Errorf("%s, AddLine: got line count %d; want %d", f.Name(), f.LineCount(), i + 1);
            }
            // adding the same offset again should be ignored
            f.AddLine(offset);
            if (f.LineCount() != i + 1) {
                t.Errorf("%s, AddLine: got unchanged line count %d; want %d", f.Name(), f.LineCount(), i + 1);
            }
            verifyPositions(t, fset, f, test.lines.slice(0, i + 1));
        }

        // add lines with SetLines and verify all positions
        var ok = f.SetLines(test.lines); if (!ok) {
            t.Errorf("%s: SetLines failed", f.Name());
        }
        if (f.LineCount() != len(test.lines)) {
            t.Errorf("%s, SetLines: got line count %d; want %d", f.Name(), f.LineCount(), len(test.lines));
        }
        if (!slices.Equal<slice<nint>, nint>(f.Lines(), test.lines)) {
            t.Errorf("%s, Lines after SetLines(v): got %v; want %v", f.Name(), f.Lines(), test.lines);
        }
        verifyPositions(t, fset, f, test.lines);

        // add lines with SetLinesForContent and verify all positions
        var src = test.source;
        if (src == nil) {
            // no test source available - create one from scratch
            src = makeTestSource(test.size, test.lines);
        }
        f.SetLinesForContent(src);
        if (f.LineCount() != len(test.lines)) {
            t.Errorf("%s, SetLinesForContent: got line count %d; want %d", f.Name(), f.LineCount(), len(test.lines));
        }
        verifyPositions(t, fset, f, test.lines);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;
internal static ref readonly @string barˢ => ref barˢᵇ;
private static readonly @string barˢᵇ = "bar"u8;

public static void TestLineInfo(ж<testing.T> t) {
    var fset = NewFileSet();
    var f = fset.AddFile(fooˢ, fset.Base(), 500);
    var lines = slice<nint>([0, 42, 77, 100, 210, 220, 277, 300, 333, 401]);
    // add lines individually and provide alternative line information
    foreach (var (_, offs) in range(lines)) {
        f.AddLine(offs);
        f.AddLineInfo(offs, barˢ, 42);
    }
    // verify positions for all offsets
    for (nint offs = 0; offs <= f.Size(); offs++) {
        global::go.go.token_package.ΔPos p = f.Pos(offs);
        var (_, col) = linecol(lines, offs);
        @string msg = fmt.Sprintf("%s (offs = %d, p = %d)"u8, f.Name(), offs, p);
        checkPos(t, msg, f.Position(f.Pos(offs)), new ΔPosition("bar", offs, 42, col));
        checkPos(t, msg, fset.Position(p), new ΔPosition("bar", offs, 42, col));
    }
}

public static void TestFiles(ж<testing.T> t) {
    var fset = NewFileSet();
    foreach (var (i, test) in range(tests)) {
        nint @base = fset.Base();
        if (i % 2 == 1) {
            // Setting a negative base is equivalent to
            // fset.Base(), so test some of each.
            @base = -1;
        }
        fset.AddFile(test.filename, @base, test.size);
        nint j = 0;
        fset.Iterate((ж<global::go.go.token_package.ΔFile> f) => {
            if (f.Name() != tests[j].filename) {
                t.Errorf("got filename = %s; want %s", f.Name(), tests[j].filename);
            }
            j++;
            return true;
        });
        if (j != i + 1) {
            t.Errorf("got %d files; want %d", j, i + 1);
        }
    }
}

// FileSet.File should return nil if Pos is past the end of the FileSet.
public static void TestFileSetPastEnd(ж<testing.T> t) {
    var fset = NewFileSet();
    foreach (var (_, test) in range(tests)) {
        fset.AddFile(test.filename, fset.Base(), test.size);
    }
    var f = fset.File((global::go.go.token_package.ΔPos)fset.Base()); if (f != nil) {
        t.Errorf("got %v, want nil", f.OrTypedNil());
    }
}

public static void TestFileSetCacheUnlikely(ж<testing.T> t) {
    var fset = NewFileSet();
    var offsets = new map<@string, nint>();
    foreach (var (_, test) in range(tests)) {
        offsets[test.filename] = fset.Base();
        fset.AddFile(test.filename, fset.Base(), test.size);
    }
    foreach (var (@file, pos) in offsets) {
        var f = fset.File((global::go.go.token_package.ΔPos)pos);
        if (f.Name() != @file) {
            t.Errorf("got %q at position %d, want %q", f.Name(), pos, @file);
        }
    }
}

// issue 4345. Test that concurrent use of FileSet.Pos does not trigger a
// race in the FileSet position cache.
public static partial void TestFileSetRace(ж<testing.T> t) {
    var fset = NewFileSet();
    for (nint i = 0; i < 100; i++) {
        fset.AddFile(fmt.Sprintf("file-%d"u8, i), fset.Base(), 1031);
    }
    var max = (int32)fset.Base();
    ref var stop = ref heap(new sync.WaitGroup(), out var Ꮡstop);
    var r = rand.New(rand.NewSource(7));
    for (nint i = 0; i < 2; i++) {
        var rΔ1 = rand.New(rand.NewSource(r.Int63()));
        Ꮡstop.Add(1);
        var fsetʗ1 = fset;
        var rʗ1 = rΔ1;
        goǃ(() => {
            for (nint iΔ1 = 0; iΔ1 < 1000; iΔ1++) {
                fsetʗ1.Position((global::go.go.token_package.ΔPos)(nint)rʗ1.Int31n(max));
            }
            Ꮡstop.Done();
        });
    }
    Ꮡstop.Wait();
}

// issue 16548. Test that concurrent use of File.AddLine and FileSet.PositionFor
// does not trigger a race in the FileSet position cache.
public static partial void TestFileSetRace2(ж<testing.T> t) {
    const nint N = 1000;
    ж<global::go.go.token_package.FileSet> fset = NewFileSet();
    ж<global::go.go.token_package.ΔFile> @file = fset.AddFile(""u8, -1, N);
    channel<nint> ch = new channel<nint>(2);

    var chʗ1 = ch;
    var fileʗ1 = @file;
    goǃ(() => {
        for (nint i = 0; i < N; i++) {
            fileʗ1.AddLine(i);
        }
        chʗ1.ᐸꟷ(1);
    });

    var chʗ2 = ch;
    var fileʗ2 = @file;
    var fsetʗ1 = fset;
    goǃ(() => {
        global::go.go.token_package.ΔPos pos = fileʗ2.Pos(0);
        for (nint i = 0; i < N; i++) {
            fsetʗ1.PositionFor(pos, false);
        }
        chʗ2.ᐸꟷ(1);
    });

    ᐸꟷ(ch);
    ᐸꟷ(ch);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string positionForUnadjustedˢ => ref positionForUnadjustedˢᵇ;
private static readonly @string positionForUnadjustedˢᵇ = "1. PositionFor unadjusted"u8;
internal static ref readonly @string positionForAdjustedˢ => ref positionForAdjustedˢᵇ;
private static readonly @string positionForAdjustedˢᵇ = "1. PositionFor adjusted"u8;
internal static ref readonly @string positionˢ => ref positionˢᵇ;
private static readonly @string positionˢᵇ = "1. Position"u8;
internal static ref readonly @string positionForUnadjustedˢ2 => ref positionForUnadjustedˢ2ᵇ;
private static readonly @string positionForUnadjustedˢ2ᵇ = "2. PositionFor unadjusted"u8;
internal static ref readonly @string positionForAdjustedˢ2 => ref positionForAdjustedˢ2ᵇ;
private static readonly @string positionForAdjustedˢ2ᵇ = "3. PositionFor adjusted"u8;
internal static ref readonly @string positionˢ2 => ref positionˢ2ᵇ;
private static readonly @string positionˢ2ᵇ = "3. Position"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string filenameᶜ => ref filenameᶜᵇ;
private static readonly @string filenameᶜᵇ = "foo"u8;

public static void TestPositionFor(ж<testing.T> t) {
    var src = slice<byte>("""

foo
b
ar
//line :100
foobar
//line bar:3
done

"""u8.Raw());

    @string filename = filenameᶜ;
    var fset = NewFileSet();
    var f = fset.AddFile(filename, fset.Base(), len(src));
    f.SetLinesForContent(src);

    // verify position info
    foreach (var (i, offs) in range(f.lines)) {
        var got1 = f.PositionFor(f.Pos(offs), false);
        var got2 = f.PositionFor(f.Pos(offs), true);
        var got3 = f.Position(f.Pos(offs));
        var want = new ΔPosition(filename, offs, i + 1, 1);
        checkPos(t, positionForUnadjustedˢ, got1, want);
        checkPos(t, positionForAdjustedˢ, got2, want);
        checkPos(t, positionˢ, got3, want);
    }

    // manually add //line info on lines l1, l2
    UntypedInt l1 = 5;
    UntypedInt l2 = 7;
    f.AddLineInfo(f.lines[l1 - 1], ""u8, 100);
    f.AddLineInfo(f.lines[l2 - 1], barˢ, 3);

    // unadjusted position info must remain unchanged
    foreach (var (i, offs) in range(f.lines)) {
        var got1 = f.PositionFor(f.Pos(offs), false);
        var want = new ΔPosition(filename, offs, i + 1, 1);
        checkPos(t, positionForUnadjustedˢ2, got1, want);
    }

    // adjusted position info should have changed
    foreach (var (i, offs) in range(f.lines)) {
        var got2 = f.PositionFor(f.Pos(offs), true);
        var got3 = f.Position(f.Pos(offs));
        var want = new ΔPosition(filename, offs, i + 1, 1);
        // manually compute wanted filename and line
        nint line = want.Line;
        if (i + 1 >= l1) {
            want.Filename = ""u8;
            want.Line = line - (nint)l1 + 100;
        }
        if (i + 1 >= l2) {
            want.Filename = barˢ;
            want.Line = line - (nint)l2 + 3;
        }
        checkPos(t, positionForAdjustedˢ2, got2, want);
        checkPos(t, positionˢ2, got3, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string inputˢ => ref inputˢᵇ;
private static readonly @string inputˢᵇ = "input"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ => ref srcᶜᵇ;
private static readonly @string srcᶜᵇ = "one\ntwo\nthree\n"u8;

public static void TestLineStart(ж<testing.T> t) {
    @string src = srcᶜ;
    var fset = NewFileSet();
    var f = fset.AddFile(inputˢ, -1, len(src));
    f.SetLinesForContent(slice<byte>(src));

    for (nint line = 1; line <= 3; line++) {
        global::go.go.token_package.ΔPos pos = f.LineStart(line);
        var position = fset.Position(pos);
        if (position.Line != line || position.Column != 1) {
            t.Errorf("LineStart(%d) returned wrong pos %d: %s", line, pos, position);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fileAˢ => ref fileAˢᵇ;
private static readonly @string fileAˢᵇ = "fileA"u8;
internal static ref readonly @string fileBˢ => ref fileBˢᵇ;
private static readonly @string fileBˢᵇ = "fileB"u8;
internal static ref readonly @string fileA14ˢ => ref fileA14ˢᵇ;
private static readonly @string fileA14ˢᵇ = "fileA:1:4"u8;
internal static ref readonly @string fileB14ˢ => ref fileB14ˢᵇ;
private static readonly @string fileB14ˢᵇ = "fileB:1:4"u8;

public static void TestRemoveFile(ж<testing.T> t) {
    var contentA = slice<byte>("this\nis\nfileA"u8);
    var contentB = slice<byte>("this\nis\nfileB"u8);
    var fset = NewFileSet();
    var a = fset.AddFile(fileAˢ, -1, len(contentA));
    a.SetLinesForContent(contentA);
    var b = fset.AddFile(fileBˢ, -1, len(contentB));
    b.SetLinesForContent(contentB);

    var fsetʗ1 = fset;
    void checkPos(global::go.go.token_package.ΔPos pos, @string want) {
        @string got = fsetʗ1.Position(pos).String(); if (got != want) {
            t.Errorf("Position(%d) = %s, want %s", pos, got, want);
        }
    }
    var fsetʗ2 = fset;
    void checkNumFiles(nint want) {
        nint got = 0;
        fsetʗ2.Iterate((ж<global::go.go.token_package.ΔFile> _) => {
            got++;
            return true;
        });
        if (got != want) {
            t.Errorf("Iterate called %d times, want %d", got, want);
        }
    }

    global::go.go.token_package.ΔPos apos3 = a.Pos(3);
    global::go.go.token_package.ΔPos bpos3 = b.Pos(3);
    checkPos(apos3, fileA14ˢ);
    checkPos(bpos3, fileB14ˢ);
    checkNumFiles(2);

    // After removal, queries on fileA fail.
    fset.RemoveFile(a);
    checkPos(apos3, "-");
    checkPos(bpos3, fileB14ˢ);
    checkNumFiles(1);

    // idempotent / no effect
    fset.RemoveFile(a);
    checkPos(apos3, "-");
    checkPos(bpos3, fileB14ˢ);
    checkNumFiles(1);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string filenameᶜ1 => ref filenameᶜ1ᵇ;
private static readonly @string filenameᶜ1ᵇ = "test.go"u8;

internal partial struct TestFileAddLineColumnInfo_tests /*dyn*/ {
    internal @string name;
    internal slice<global::go.go.token_package.lineInfo> infos;
    internal slice<global::go.go.token_package.lineInfo> want;
}

public static void TestFileAddLineColumnInfo(ж<testing.T> t) {
    @string filename = filenameᶜ1;
    UntypedInt filesize = 100;

    var tests = slice<TestFileAddLineColumnInfo_tests>([
        new(
            name: "normal",
            infos: [
                new(Offset: 10, Filename: filename, Line: 2, Column: 1),
                new(Offset: 50, Filename: filename, Line: 3, Column: 1),
                new(Offset: 80, Filename: filename, Line: 4, Column: 2)
            ],
            want: [
                new(Offset: 10, Filename: filename, Line: 2, Column: 1),
                new(Offset: 50, Filename: filename, Line: 3, Column: 1),
                new(Offset: 80, Filename: filename, Line: 4, Column: 2)
            ]
        ),
        new(
            name: "offset1 == file size",
            infos: [
                new(Offset: filesize, Filename: filename, Line: 2, Column: 1)
            ],
            want: default
        ),
        new(
            name: "offset1 > file size",
            infos: [
                new(Offset: filesize + 1, Filename: filename, Line: 2, Column: 1)
            ],
            want: default
        ),
        new(
            name: "offset2 == file size",
            infos: [
                new(Offset: 10, Filename: filename, Line: 2, Column: 1),
                new(Offset: filesize, Filename: filename, Line: 3, Column: 1)
            ],
            want: [
                new(Offset: 10, Filename: filename, Line: 2, Column: 1)
            ]
        ),
        new(
            name: "offset2 > file size",
            infos: [
                new(Offset: 10, Filename: filename, Line: 2, Column: 1),
                new(Offset: filesize + 1, Filename: filename, Line: 3, Column: 1)
            ],
            want: [
                new(Offset: 10, Filename: filename, Line: 2, Column: 1)
            ]
        ),
        new(
            name: "offset2 == offset1",
            infos: [
                new(Offset: 10, Filename: filename, Line: 2, Column: 1),
                new(Offset: 10, Filename: filename, Line: 3, Column: 1)
            ],
            want: [
                new(Offset: 10, Filename: filename, Line: 2, Column: 1)
            ]
        ),
        new(
            name: "offset2 < offset1",
            infos: [
                new(Offset: 10, Filename: filename, Line: 2, Column: 1),
                new(Offset: 9, Filename: filename, Line: 3, Column: 1)
            ],
            want: [
                new(Offset: 10, Filename: filename, Line: 2, Column: 1)
            ]
        )
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var test = ref heap(new TestFileAddLineColumnInfo_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            var fs = NewFileSet();
            var f = fs.AddFile(filename, -1, filesize);
            foreach (var (_, info) in range(testʗ1.infos)) {
                f.AddLineColumnInfo(info.Offset, info.Filename, info.Line, info.Column);
            }
            if (!slices.Equal<slice<global::go.go.token_package.lineInfo>, global::go.go.token_package.lineInfo>(f.infos, testʗ1.want)) {
                tΔ1.Errorf("\ngot %+v, \nwant %+v", f.infos, testʗ1.want);
            }
        });
    }
}

public static void TestIssue57490(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // If debug is set, this test is expected to panic.
        if (debug) {
            defer(() => {
                if (recover() == nil) {
                    t.Errorf("got no panic");
                }
            }, ref ᒐ);
        }

        UntypedInt fsize = 5;
        var fset = NewFileSet();
        nint @base = fset.Base();
        var f = fset.AddFile("f", @base, fsize);

        // out-of-bounds positions must not lead to a panic when calling f.Offset
        {
            nint got = f.Offset(NoPos); if (got != 0) {
                t.Errorf("offset = %d, want %d", got, (nint)0);
            }
        }
        {
            nint got = f.Offset((global::go.go.token_package.ΔPos)(-1)); if (got != 0) {
                t.Errorf("offset = %d, want %d", got, (nint)0);
            }
        }
        {
            nint got = f.Offset((global::go.go.token_package.ΔPos)(@base + (nint)fsize + 1)); if (got != fsize) {
                t.Errorf("offset = %d, want %d", got, (nint)(fsize));
            }
        }

        // out-of-bounds offsets must not lead to a panic when calling f.Pos
        {
            global::go.go.token_package.ΔPos got = f.Pos(-1); if (got != ((global::go.go.token_package.ΔPos)@base)) {
                t.Errorf("pos = %d, want %d", got, @base);
            }
        }
        {
            global::go.go.token_package.ΔPos got = f.Pos(fsize + 1); if (got != ((global::go.go.token_package.ΔPos)(@base + (nint)fsize))) {
                t.Errorf("pos = %d, want %d", got, @base + (nint)fsize);
            }
        }

        // out-of-bounds Pos values must not lead to a panic when calling f.Position
        @string want = fmt.Sprintf("%s:1:1"u8, f.Name());
        {
            @string got = f.Position((global::go.go.token_package.ΔPos)(-1)).String(); if (got != want) {
                t.Errorf("position = %s, want %s", got, want);
            }
        }
        want = fmt.Sprintf("%s:1:%d"u8, f.Name(), (nint)(fsize + 1));
        {
            @string got = f.Position((global::go.go.token_package.ΔPos)(fsize + 1)).String(); if (got != want) {
                t.Errorf("position = %s, want %s", got, want);
            }
        }

        // check invariants
        UntypedInt xsize = /* fsize + 5 */ 10;
        for (nint offset = -xsize; offset < xsize; offset++) {
            nint want1 = f.Offset((global::go.go.token_package.ΔPos)(f.@base + offset));
            {
                nint got = f.Offset(f.Pos(offset)); if (got != want1) {
                    t.Errorf("offset = %d, want %d", got, want1);
                }
            }

            global::go.go.token_package.ΔPos want2 = f.Pos(offset);
            {
                global::go.go.token_package.ΔPos got = f.Pos(f.Offset(want2)); if (got != want2) {
                    t.Errorf("pos = %d, want %d", got, want2);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string afterAddFileAbˢ => ref afterAddFileAbˢᵇ;
private static readonly @string afterAddFileAbˢᵇ = "after AddFile [AB]"u8;
internal static ref readonly @string a14B510ˢ => ref a14B510ˢᵇ;
private static readonly @string a14B510ˢᵇ = "{A:1-4 B:5-10}"u8;
internal static ref readonly @string afterAddExistingFilesˢ => ref afterAddExistingFilesˢᵇ;
private static readonly @string afterAddExistingFilesˢᵇ = "after AddExistingFiles []"u8;
internal static ref readonly @string afterAddExistingFilesˢ2 => ref afterAddExistingFilesˢ2ᵇ;
private static readonly @string afterAddExistingFilesˢ2ᵇ = "after AddExistingFiles [CADC]"u8;
internal static ref readonly @string a14B510C100105D200205ˢ => ref a14B510C100105D200205ˢᵇ;
private static readonly @string a14B510C100105D200205ˢᵇ = "{A:1-4 B:5-10 C:100-105 D:200-205}"u8;
internal static ref readonly @string afterAddFileEˢ => ref afterAddFileEˢᵇ;
private static readonly @string afterAddFileEˢᵇ = "after AddFile [E]"u8;
internal static ref readonly @string a14B510C100105D200205Eˢ => ref a14B510C100105D200205Eˢᵇ;
private static readonly @string a14B510C100105D200205Eˢᵇ = "{A:1-4 B:5-10 C:100-105 D:200-205 E:206-209}"u8;

public static partial void TestFileSet_AddExistingFiles(ж<testing.T> t) {
    var fset = NewFileSet();

    var fsetʗ1 = fset;
    void check(@string descr, @string want) {
        t.Helper();
        @string got = fsetString(fsetʗ1); if (got != want) {
            t.Errorf("%s: got %s, want %s", descr, got, want);
        }
    }

    var fileA = fset.AddFile("A", -1, 3);
    var fileB = fset.AddFile("B", -1, 5);
    _ = fileB;
    check(afterAddFileAbˢ, a14B510ˢ);

    fset.AddExistingFiles(); // noop
    check(afterAddExistingFilesˢ, a14B510ˢ);

    var fileC = NewFileSet().AddFile("C", 100, 5);
    var fileD = NewFileSet().AddFile("D", 200, 5);
    fset.AddExistingFiles(fileC, fileA, fileD, fileC);
    check(afterAddExistingFilesˢ2, a14B510C100105D200205ˢ);

    var fileE = fset.AddFile("E", -1, 3);
    _ = fileE;
    check(afterAddFileEˢ, a14B510C100105D200205Eˢ);
}

internal static @string fsetString(ж<global::go.go.token_package.FileSet> fset) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    Ꮡbuf.WriteRune('{');
    @string sep = ""u8;
    fset.Iterate((ж<global::go.go.token_package.ΔFile> f) => {
        fmt.Fprintf(new token_test_package.strings_BuilderжWriter(Ꮡbuf), "%s%s:%d-%d"u8, sep, f.Name(), f.Base(), f.End());
        sep = " ";
        return true;
    });
    Ꮡbuf.WriteRune('}');
    return buf.String();
}

// Test that File() does not return the already removed file, while used concurrently.
public static partial void TestRemoveFileRace(ж<testing.T> t) {
    var fset = NewFileSet();

    // Create bunch of files.
    slice<ж<global::go.go.token_package.ΔFile>> files = default;
    foreach (var i in range(20000)) {
        var f = fset.AddFile("f", -1, (i + 1) * 10);
        files = append(files, f);
    }

    // governor goroutine
    var (race1, race2) = (new channel<ж<global::go.go.token_package.ΔFile>>(0), new channel<ж<global::go.go.token_package.ΔFile>>(0));
    var start = new channel<EmptyStruct>(0);
    var filesʗ1 = files;
    var race1ʗ1 = race1;
    var race2ʗ1 = race2;
    var startʗ1 = start;
    goǃ(() => {
        foreach (var (_, f) in range(filesʗ1)) {
            ᐸꟷ(startʗ1);
            race1ʗ1.ᐸꟷ(f);
            race2ʗ1.ᐸꟷ(f);
        }
        ᐸꟷ(startʗ1); // unlock main test goroutine
        close(race1ʗ1);
        close(race2ʗ1);
    });

    var fsetʗ1 = fset;
    var race1ʗ2 = race1;
    goǃ(() => {
        foreach (var f in race1ʗ2) {
            fsetʗ1.File(((global::go.go.token_package.ΔPos)f.Base()) + 5); // populates s.last with f
        }
    });

    start.ᐸꟷ(new EmptyStruct());
    foreach (var f in race2) {
        fset.RemoveFile(f);
        var got = fset.File(((global::go.go.token_package.ΔPos)f.Base()) + 5);
        if (got != nil) {
            t.Fatalf("file was not removed correctly");
        }
        start.ᐸꟷ(new EmptyStruct());
    }
}

public static void TestRemovedFileFileReturnsNil(ж<testing.T> t) {
    var filesᴷ = new stackAppend<ж<global::go.go.token_package.ΔFile>>();

    var fset = NewFileSet();

    // Create bunch of files.
    slice<ж<global::go.go.token_package.ΔFile>> files = default;
    foreach (var i in range(1000)) {
        var f = fset.AddFile("f", -1, (i + 1) * 100);
        files = append(filesᴷ, files, f);
    }

    var filesʗ1 = files;
    rand.Shuffle(len(files), (nint i, nint j) => {
        (filesʗ1[i], filesʗ1[j]) = (filesʗ1[j], filesʗ1[i]);
    });

    foreach (var (_, f) in range(files)) {
        fset.RemoveFile(f);
        var got = fset.File(((global::go.go.token_package.ΔPos)f.Base()) + 10); if (got != nil) {
            t.Fatalf("file was not removed correctly; got file with base: %v", got.Base());
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aGoˢ => ref aGoˢᵇ;
private static readonly @string aGoˢᵇ = "a.go"u8;

public static void TestFile_End(ж<testing.T> t) {
    var f = NewFileSet().AddFile(aGoˢ, 100, 42);
    @string got = fmt.Sprintf("%d, %d"u8, f.Base(), f.End());
    @string want = "100, 142"; if (got != want) {
        t.Errorf("Base, End = %s, want %s", got, want);
    }
}

} // end token_internal_test_package
