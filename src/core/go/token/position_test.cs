// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using rand = math.rand_package;
using slices = slices_package;
using sync = sync_package;
using testing = testing_package;
using math;
using static global::go.go.token_package;

partial class token_internal_test_package {

internal static void checkPos(ж<testing.T> Ꮡt, @string msg, global::go.go.token_package.ΔPosition got, global::go.go.token_package.ΔPosition want) {
    if (got.Filename != want.Filename) {
        Ꮡt.Errorf("%s: got filename = %q; want %q"u8, msg, got.Filename, want.Filename);
    }
    if (got.Offset != want.Offset) {
        Ꮡt.Errorf("%s: got offset = %d; want %d"u8, msg, got.Offset, want.Offset);
    }
    if (got.Line != want.Line) {
        Ꮡt.Errorf("%s: got line = %d; want %d"u8, msg, got.Line, want.Line);
    }
    if (got.Column != want.Column) {
        Ꮡt.Errorf("%s: got column = %d; want %d"u8, msg, got.Column, want.Column);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nilNoPosˢ = "nil NoPos"u8;
internal static readonly @string fsetNoPosˢ = "fset NoPos"u8;

public static void TestNoPos(ж<testing.T> Ꮡt) {
    if (NoPos.IsValid()) {
        Ꮡt.Errorf("NoPos should not be valid"u8);
    }
    ж<global::go.go.token_package.FileSet> fset = default!;
    checkPos(Ꮡt, nilNoPosˢ, fset.Position(NoPos), new ΔPosition(nil));
    fset = NewFileSet();
    checkPos(Ꮡt, fsetNoPosˢ, fset.Position(NoPos), new ΔPosition(nil));
}


[GoType("dyn")] partial struct testsᴛ1 {
    internal @string filename;
    internal slice<byte> source; // may be nil
    internal nint size;
    internal slice<nint> lines;
}
internal static slice<testsᴛ1> tests = new testsᴛ1[]{
    new("a"u8, new byte[]{}.slice(), 0, new nint[]{}.slice()),
    new("b"u8, slice<byte>("01234"u8), 5, new nint[]{0}.slice()),
    new("c"u8, slice<byte>("\n\n\n\n\n\n\n\n\n"u8), 9, new nint[]{0, 1, 2, 3, 4, 5, 6, 7, 8}.slice()),
    new("d"u8, default!, 100, new nint[]{0, 5, 10, 20, 30, 70, 71, 72, 80, 85, 90, 99}.slice()),
    new("e"u8, default!, 777, new nint[]{0, 80, 100, 120, 130, 180, 267, 455, 500, 567, 620}.slice()),
    new("f"u8, slice<byte>("package p\n\nimport \"fmt\""u8), 23, new nint[]{0, 10, 11}.slice()),
    new("g"u8, slice<byte>("package p\n\nimport \"fmt\"\n"u8), 24, new nint[]{0, 10, 11}.slice()),
    new("h"u8, slice<byte>("package p\n\nimport \"fmt\"\n "u8), 25, new nint[]{0, 10, 11, 24}.slice())
}.slice();

internal static (nint, nint) linecol(slice<nint> lines, nint offs) {
    nint prevLineOffs = 0;
    foreach (var (line, lineOffs) in lines) {
        if (offs < lineOffs) {
            return (line, offs - prevLineOffs + 1);
        }
        prevLineOffs = lineOffs;
    }
    return (len(lines), offs - prevLineOffs + 1);
}

internal static void verifyPositions(ж<testing.T> Ꮡt, ж<global::go.go.token_package.FileSet> Ꮡfset, ж<global::go.go.token_package.ΔFile> Ꮡf, slice<nint> lines) {
    ref var f = ref Ꮡf.DerefOrNull();

    for (nint offs = 0; offs < f.Size(); offs++) {
        global::go.go.token_package.ΔPos p = f.Pos(offs);
        nint offs2 = f.Offset(p);
        if (offs2 != offs) {
            Ꮡt.Errorf("%s, Offset: got offset %d; want %d"u8, f.Name(), offs2, offs);
        }
        var (line, col) = linecol(lines, offs);
        @string msg = fmt.Sprintf("%s (offs = %d, p = %d)"u8, f.Name(), offs, p);
        checkPos(Ꮡt, msg, Ꮡf.Position(f.Pos(offs)), new ΔPosition(f.Name(), offs, line, col));
        checkPos(Ꮡt, msg, Ꮡfset.Position(p), new ΔPosition(f.Name(), offs, line, col));
    }
}

internal static slice<byte> makeTestSource(nint size, slice<nint> lines) {
    var src = new slice<byte>(size);
    foreach (var (_, offs) in lines) {
        if (offs > 0) {
            src[offs - 1] = (rune)'\n';
        }
    }
    return src;
}

public static void TestPositions(ж<testing.T> Ꮡt) {
    const nint delta = 7; // a non-zero base offset increment
    var fset = NewFileSet();
    foreach (var (_, test) in tests) {
        // verify consistency of test case
        if (test.source != default! && len(test.source) != test.size) {
            Ꮡt.Errorf("%s: inconsistent test case: got file size %d; want %d"u8, test.filename, len(test.source), test.size);
        }
        // add file and verify name and size
        var f = fset.AddFile(test.filename, fset.Base() + delta, test.size);
        if (f.Name() != test.filename) {
            Ꮡt.Errorf("got filename %q; want %q"u8, f.Name(), test.filename);
        }
        if (f.Size() != test.size) {
            Ꮡt.Errorf("%s: got file size %d; want %d"u8, f.Name(), f.Size(), test.size);
        }
        if (fset.File(f.Pos(0)) != f) {
            Ꮡt.Errorf("%s: f.Pos(0) was not found in f"u8, f.Name());
        }
        // add lines individually and verify all positions
        foreach (var (i, offset) in test.lines) {
            f.AddLine(offset);
            if (f.LineCount() != i + 1) {
                Ꮡt.Errorf("%s, AddLine: got line count %d; want %d"u8, f.Name(), f.LineCount(), i + 1);
            }
            // adding the same offset again should be ignored
            f.AddLine(offset);
            if (f.LineCount() != i + 1) {
                Ꮡt.Errorf("%s, AddLine: got unchanged line count %d; want %d"u8, f.Name(), f.LineCount(), i + 1);
            }
            verifyPositions(Ꮡt, fset, f, test.lines[0..(int)(i + 1)]);
        }
        // add lines with SetLines and verify all positions
        {
            var ok = f.SetLines(test.lines); if (!ok) {
                Ꮡt.Errorf("%s: SetLines failed"u8, f.Name());
            }
        }
        if (f.LineCount() != len(test.lines)) {
            Ꮡt.Errorf("%s, SetLines: got line count %d; want %d"u8, f.Name(), f.LineCount(), len(test.lines));
        }
        if (!slices.Equal<slice<nint>, nint>(f.Lines(), test.lines)) {
            Ꮡt.Errorf("%s, Lines after SetLines(v): got %v; want %v"u8, f.Name(), f.Lines(), test.lines);
        }
        verifyPositions(Ꮡt, fset, f, test.lines);
        // add lines with SetLinesForContent and verify all positions
        var src = test.source;
        if (src == default!) {
            // no test source available - create one from scratch
            src = makeTestSource(test.size, test.lines);
        }
        f.SetLinesForContent(src);
        if (f.LineCount() != len(test.lines)) {
            Ꮡt.Errorf("%s, SetLinesForContent: got line count %d; want %d"u8, f.Name(), f.LineCount(), len(test.lines));
        }
        verifyPositions(Ꮡt, fset, f, test.lines);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = "foo"u8;
internal static readonly @string barˢ = "bar"u8;

public static void TestLineInfo(ж<testing.T> Ꮡt) {
    var fset = NewFileSet();
    var f = fset.AddFile(fooˢ, fset.Base(), 500);
    var lines = new nint[]{0, 42, 77, 100, 210, 220, 277, 300, 333, 401}.slice();
    // add lines individually and provide alternative line information
    foreach (var (_, offs) in lines) {
        f.AddLine(offs);
        f.AddLineInfo(offs, barˢ, 42);
    }
    // verify positions for all offsets
    for (nint offs = 0; offs <= f.Size(); offs++) {
        global::go.go.token_package.ΔPos p = f.Pos(offs);
        var (_, col) = linecol(lines, offs);
        @string msg = fmt.Sprintf("%s (offs = %d, p = %d)"u8, f.Name(), offs, p);
        checkPos(Ꮡt, msg, f.Position(f.Pos(offs)), new ΔPosition("bar"u8, offs, 42, col));
        checkPos(Ꮡt, msg, fset.Position(p), new ΔPosition("bar"u8, offs, 42, col));
    }
}

public static void TestFiles(ж<testing.T> Ꮡt) {
    var fset = NewFileSet();
    foreach (var (i, test) in tests) {
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
                Ꮡt.Errorf("got filename = %s; want %s"u8, f.Name(), tests[j].filename);
            }
            j++;
            return true;
        });
        if (j != i + 1) {
            Ꮡt.Errorf("got %d files; want %d"u8, j, i + 1);
        }
    }
}

// FileSet.File should return nil if Pos is past the end of the FileSet.
public static void TestFileSetPastEnd(ж<testing.T> Ꮡt) {
    var fset = NewFileSet();
    foreach (var (_, test) in tests) {
        fset.AddFile(test.filename, fset.Base(), test.size);
    }
    {
        var f = fset.File(((global::go.go.token_package.ΔPos)fset.Base())); if (f != nil) {
            Ꮡt.Errorf("got %v, want nil"u8, f.OrTypedNil());
        }
    }
}

public static void TestFileSetCacheUnlikely(ж<testing.T> Ꮡt) {
    var fset = NewFileSet();
    var offsets = new map<@string, nint>();
    foreach (var (_, test) in tests) {
        offsets[test.filename] = fset.Base();
        fset.AddFile(test.filename, fset.Base(), test.size);
    }
    foreach (var (@file, pos) in offsets) {
        var f = fset.File(((global::go.go.token_package.ΔPos)pos));
        if (f.Name() != @file) {
            Ꮡt.Errorf("got %q at position %d, want %q"u8, f.Name(), pos, @file);
        }
    }
}

// issue 4345. Test that concurrent use of FileSet.Pos does not trigger a
// race in the FileSet position cache.
public static void TestFileSetRace(ж<testing.T> Ꮡt) {
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
                fsetʗ1.Position(((global::go.go.token_package.ΔPos)(nint)rʗ1.Int31n(max)));
            }
            Ꮡstop.Done();
        });
    }
    Ꮡstop.Wait();
}

// issue 16548. Test that concurrent use of File.AddLine and FileSet.PositionFor
// does not trigger a race in the FileSet position cache.
public static void TestFileSetRace2(ж<testing.T> Ꮡt) {
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
internal static readonly @string positionForUnadjustedˢ = "1. PositionFor unadjusted"u8;
internal static readonly @string positionForAdjustedˢ = "1. PositionFor adjusted"u8;
internal static readonly @string positionˢ = "1. Position"u8;
internal static readonly @string positionForUnadjustedˢ2 = "2. PositionFor unadjusted"u8;
internal static readonly @string positionForAdjustedˢ2 = "3. PositionFor adjusted"u8;
internal static readonly @string positionˢ2 = "3. Position"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string filenameᶜ = "foo"u8;

public static void TestPositionFor(ж<testing.T> Ꮡt) {
    var src = slice<byte>("""

foo
b
ar
//line :100
foobar
//line bar:3
done

"""u8);
    @string filename = filenameᶜ;
    var fset = NewFileSet();
    var f = fset.AddFile(filename, fset.Base(), len(src));
    f.SetLinesForContent(src);
    // verify position info
    foreach (var (i, offs) in (~f).lines) {
        var got1 = f.PositionFor(f.Pos(offs), false);
        var got2 = f.PositionFor(f.Pos(offs), true);
        var got3 = f.Position(f.Pos(offs));
        var want = new ΔPosition(filename, offs, i + 1, 1);
        checkPos(Ꮡt, positionForUnadjustedˢ, got1, want);
        checkPos(Ꮡt, positionForAdjustedˢ, got2, want);
        checkPos(Ꮡt, positionˢ, got3, want);
    }
    // manually add //line info on lines l1, l2
    UntypedInt l1 = 5;
    UntypedInt l2 = 7;
    f.AddLineInfo((~f).lines[l1 - 1], ""u8, 100);
    f.AddLineInfo((~f).lines[l2 - 1], barˢ, 3);
    // unadjusted position info must remain unchanged
    foreach (var (i, offs) in (~f).lines) {
        var got1 = f.PositionFor(f.Pos(offs), false);
        var want = new ΔPosition(filename, offs, i + 1, 1);
        checkPos(Ꮡt, positionForUnadjustedˢ2, got1, want);
    }
    // adjusted position info should have changed
    foreach (var (i, offs) in (~f).lines) {
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
        checkPos(Ꮡt, positionForAdjustedˢ2, got2, want);
        checkPos(Ꮡt, positionˢ2, got3, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string inputˢ = "input"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ = "one\ntwo\nthree\n"u8;

public static void TestLineStart(ж<testing.T> Ꮡt) {
    @string src = srcᶜ;
    var fset = NewFileSet();
    var f = fset.AddFile(inputˢ, -1, len(src));
    f.SetLinesForContent(slice<byte>(src));
    for (nint line = 1; line <= 3; line++) {
        global::go.go.token_package.ΔPos pos = f.LineStart(line);
        var position = fset.Position(pos);
        if (position.Line != line || position.Column != 1) {
            Ꮡt.Errorf("LineStart(%d) returned wrong pos %d: %s"u8, line, pos, position);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fileAˢ = "fileA"u8;
internal static readonly @string fileBˢ = "fileB"u8;
internal static readonly @string fileA14ˢ = "fileA:1:4"u8;
internal static readonly @string fileB14ˢ = "fileB:1:4"u8;

public static void TestRemoveFile(ж<testing.T> Ꮡt) {
    var contentA = slice<byte>("this\nis\nfileA"u8);
    var contentB = slice<byte>("this\nis\nfileB"u8);
    var fset = NewFileSet();
    var a = fset.AddFile(fileAˢ, -1, len(contentA));
    a.SetLinesForContent(contentA);
    var b = fset.AddFile(fileBˢ, -1, len(contentB));
    b.SetLinesForContent(contentB);
    var fsetʗ1 = fset;
    void checkPos(global::go.go.token_package.ΔPos pos, @string want) {
        {
            @string got = fsetʗ1.Position(pos).String(); if (got != want) {
                Ꮡt.Errorf("Position(%d) = %s, want %s"u8, pos, got, want);
            }
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
            Ꮡt.Errorf("Iterate called %d times, want %d"u8, got, want);
        }
    }
    global::go.go.token_package.ΔPos apos3 = a.Pos(3);
    global::go.go.token_package.ΔPos bpos3 = b.Pos(3);
    checkPos(apos3, fileA14ˢ);
    checkPos(bpos3, fileB14ˢ);
    checkNumFiles(2);
    // After removal, queries on fileA fail.
    fset.RemoveFile(a);
    checkPos(apos3, "-"u8);
    checkPos(bpos3, fileB14ˢ);
    checkNumFiles(1);
    // idempotent / no effect
    fset.RemoveFile(a);
    checkPos(apos3, "-"u8);
    checkPos(bpos3, fileB14ˢ);
    checkNumFiles(1);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string filenameᶜ1 = "test.go"u8;

[GoType("dyn")] internal partial struct TestFileAddLineColumnInfo_tests {
    internal @string name;
    internal slice<global::go.go.token_package.lineInfo> infos;
    internal slice<global::go.go.token_package.lineInfo> want;
}

public static void TestFileAddLineColumnInfo(ж<testing.T> Ꮡt) {
    @string filename = filenameᶜ1;
    UntypedInt filesize = 100;
    var tests = new TestFileAddLineColumnInfo_tests[]{
        new(
            name: "normal"u8,
            infos: new global::go.go.token_package.lineInfo[]{
                new(Offset: 10, Filename: filename, Line: 2, Column: 1),
                new(Offset: 50, Filename: filename, Line: 3, Column: 1),
                new(Offset: 80, Filename: filename, Line: 4, Column: 2)
            }.slice(),
            want: new global::go.go.token_package.lineInfo[]{
                new(Offset: 10, Filename: filename, Line: 2, Column: 1),
                new(Offset: 50, Filename: filename, Line: 3, Column: 1),
                new(Offset: 80, Filename: filename, Line: 4, Column: 2)
            }.slice()
        ),
        new(
            name: "offset1 == file size"u8,
            infos: new global::go.go.token_package.lineInfo[]{
                new(Offset: filesize, Filename: filename, Line: 2, Column: 1)
            }.slice(),
            want: default!
        ),
        new(
            name: "offset1 > file size"u8,
            infos: new global::go.go.token_package.lineInfo[]{
                new(Offset: filesize + 1, Filename: filename, Line: 2, Column: 1)
            }.slice(),
            want: default!
        ),
        new(
            name: "offset2 == file size"u8,
            infos: new global::go.go.token_package.lineInfo[]{
                new(Offset: 10, Filename: filename, Line: 2, Column: 1),
                new(Offset: filesize, Filename: filename, Line: 3, Column: 1)
            }.slice(),
            want: new global::go.go.token_package.lineInfo[]{
                new(Offset: 10, Filename: filename, Line: 2, Column: 1)
            }.slice()
        ),
        new(
            name: "offset2 > file size"u8,
            infos: new global::go.go.token_package.lineInfo[]{
                new(Offset: 10, Filename: filename, Line: 2, Column: 1),
                new(Offset: filesize + 1, Filename: filename, Line: 3, Column: 1)
            }.slice(),
            want: new global::go.go.token_package.lineInfo[]{
                new(Offset: 10, Filename: filename, Line: 2, Column: 1)
            }.slice()
        ),
        new(
            name: "offset2 == offset1"u8,
            infos: new global::go.go.token_package.lineInfo[]{
                new(Offset: 10, Filename: filename, Line: 2, Column: 1),
                new(Offset: 10, Filename: filename, Line: 3, Column: 1)
            }.slice(),
            want: new global::go.go.token_package.lineInfo[]{
                new(Offset: 10, Filename: filename, Line: 2, Column: 1)
            }.slice()
        ),
        new(
            name: "offset2 < offset1"u8,
            infos: new global::go.go.token_package.lineInfo[]{
                new(Offset: 10, Filename: filename, Line: 2, Column: 1),
                new(Offset: 9, Filename: filename, Line: 3, Column: 1)
            }.slice(),
            want: new global::go.go.token_package.lineInfo[]{
                new(Offset: 10, Filename: filename, Line: 2, Column: 1)
            }.slice()
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestFileAddLineColumnInfo_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.name, (ж<testing.T> tΔ1) => {
            var fs = NewFileSet();
            var f = fs.AddFile(filename, -1, filesize);
            foreach (var (_, info) in testʗ1.infos) {
                f.AddLineColumnInfo(info.Offset, info.Filename, info.Line, info.Column);
            }
            if (!slices.Equal<slice<global::go.go.token_package.lineInfo>, global::go.go.token_package.lineInfo>((~f).infos, testʗ1.want)) {
                tΔ1.Errorf("\ngot %+v, \nwant %+v"u8, (~f).infos, testʗ1.want);
            }
        });
    }
}

public static void TestIssue57490(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // If debug is set, this test is expected to panic.
        if (debug) {
            defer(() => {
                if (recover() == default!) {
                    Ꮡt.Errorf("got no panic"u8);
                }
            }, ref ᒐ);
        }
        UntypedInt fsize = 5;
        var fset = NewFileSet();
        nint @base = fset.Base();
        var f = fset.AddFile("f"u8, @base, fsize);
        // out-of-bounds positions must not lead to a panic when calling f.Offset
        {
            nint got = f.Offset(NoPos); if (got != 0) {
                Ꮡt.Errorf("offset = %d, want %d"u8, got, (nint)(0));
            }
        }
        {
            nint got = f.Offset(((global::go.go.token_package.ΔPos)(-1))); if (got != 0) {
                Ꮡt.Errorf("offset = %d, want %d"u8, got, (nint)(0));
            }
        }
        {
            nint got = f.Offset(((global::go.go.token_package.ΔPos)(@base + (nint)fsize + 1))); if (got != fsize) {
                Ꮡt.Errorf("offset = %d, want %d"u8, got, (nint)(fsize));
            }
        }
        // out-of-bounds offsets must not lead to a panic when calling f.Pos
        {
            global::go.go.token_package.ΔPos got = f.Pos(-1); if (got != ((global::go.go.token_package.ΔPos)@base)) {
                Ꮡt.Errorf("pos = %d, want %d"u8, got, @base);
            }
        }
        {
            global::go.go.token_package.ΔPos got = f.Pos(fsize + 1); if (got != ((global::go.go.token_package.ΔPos)(@base + (nint)fsize))) {
                Ꮡt.Errorf("pos = %d, want %d"u8, got, @base + (nint)fsize);
            }
        }
        // out-of-bounds Pos values must not lead to a panic when calling f.Position
        @string want = fmt.Sprintf("%s:1:1"u8, f.Name());
        {
            @string got = f.Position(((global::go.go.token_package.ΔPos)(-1))).String(); if (got != want) {
                Ꮡt.Errorf("position = %s, want %s"u8, got, want);
            }
        }
        want = fmt.Sprintf("%s:1:%d"u8, f.Name(), (nint)(fsize + 1));
        {
            @string got = f.Position(((global::go.go.token_package.ΔPos)(fsize + 1))).String(); if (got != want) {
                Ꮡt.Errorf("position = %s, want %s"u8, got, want);
            }
        }
        // check invariants
        UntypedInt xsize = /* fsize + 5 */ 10;
        for (nint offset = -xsize; offset < xsize; offset++) {
            nint want1 = f.Offset(((global::go.go.token_package.ΔPos)((~f).@base + offset)));
            {
                nint got = f.Offset(f.Pos(offset)); if (got != want1) {
                    Ꮡt.Errorf("offset = %d, want %d"u8, got, want1);
                }
            }
            global::go.go.token_package.ΔPos want2 = f.Pos(offset);
            {
                global::go.go.token_package.ΔPos got = f.Pos(f.Offset(want2)); if (got != want2) {
                    Ꮡt.Errorf("pos = %d, want %d"u8, got, want2);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end token_internal_test_package
