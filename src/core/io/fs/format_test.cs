// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.io;

using static go.io.fs_package;
using testing = testing_package;
using time = time_package;
using fs = go.io.fs_package;
using go.io;

partial class fs_test_package {

// formatTest implements FileInfo to test FormatFileInfo,
// and implements DirEntry to test FormatDirEntry.
partial struct formatTest {
    internal @string name;
    internal int64 size;
    internal fs.FileMode mode;
    internal time.Time modTime;
    internal bool isDir;
}

internal static @string Name(this ref formatTest fs) {
    return fs.name;
}

internal static int64 Size(this ref formatTest fs) {
    return fs.size;
}

internal static fs.FileMode Mode(this ref formatTest fs) {
    return fs.mode;
}

internal static time.Time ModTime(this ref formatTest fs) {
    return fs.modTime;
}

internal static bool IsDir(this ref formatTest fs) {
    return fs.isDir;
}

internal static any Sys(this ref formatTest fs) {
    return default;
}

internal static fs.FileMode Type(this ref formatTest fs) {
    return fs.mode.Type();
}

internal static (fs.FileInfo, error) Info(this ж<formatTest> Ꮡfs) {
    return (new formatTestжFileInfo(Ꮡfs), default);
}

partial struct formatTestsᴛ1 /*dyn*/ {
    internal formatTest input;
    internal @string wantFileInfo;
    internal @string wantDirEntry;
}
internal static slice<formatTestsᴛ1> formatTests = slice<formatTestsᴛ1>([
    new(
        new formatTest(
            name: "hello.go",
            size: 100,
            mode: 420,
            modTime: time.Date(1970, time.January, 1, 12, 0, 0, 0, time.ΔUTC),
            isDir: false
        ),
        "-rw-r--r-- 100 1970-01-01 12:00:00 hello.go",
        "- hello.go"
    ),
    new(
        new formatTest(
            name: "home/gopher",
            size: 0,
            mode: (fs.FileMode)(ModeDir | 493),
            modTime: time.Date(1970, time.January, 1, 12, 0, 0, 0, time.ΔUTC),
            isDir: true
        ),
        "drwxr-xr-x 0 1970-01-01 12:00:00 home/gopher/",
        "d home/gopher/"
    ),
    new(
        new formatTest(
            name: "big",
            size: 0x7fffffffffffffffL,
            mode: (fs.FileMode)(ModeIrregular | 420),
            modTime: time.Date(1970, time.January, 1, 12, 0, 0, 0, time.ΔUTC),
            isDir: false
        ),
        "?rw-r--r-- 9223372036854775807 1970-01-01 12:00:00 big",
        "? big"
    ),
    new(
        new formatTest(
            name: "small",
            size: -9223372036854775808L,
            mode: (fs.FileMode)((fs.FileMode)(ModeSocket | ModeSetuid) | 420),
            modTime: time.Date(1970, time.January, 1, 12, 0, 0, 0, time.ΔUTC),
            isDir: false
        ),
        "Surw-r--r-- -9223372036854775808 1970-01-01 12:00:00 small",
        "S small"
    )
]);

public static void TestFormatFileInfo(ж<testing.T> t) {
    foreach (var (i, vᴛ1) in formatTests) {
        ref var test = ref heap(new formatTestsᴛ1(), out var Ꮡtest);
        test = vᴛ1;

        @string got = FormatFileInfo(new formatTestжFileInfo(Ꮡtest.of(formatTestsᴛ1.Ꮡinput)));
        if (got != test.wantFileInfo) {
            t.Errorf("%d: FormatFileInfo(%#v) = %q, want %q", i, test.input, got, test.wantFileInfo);
        }
    }
}

public static void TestFormatDirEntry(ж<testing.T> t) {
    foreach (var (i, vᴛ1) in formatTests) {
        ref var test = ref heap(new formatTestsᴛ1(), out var Ꮡtest);
        test = vᴛ1;

        @string got = FormatDirEntry(new formatTestжDirEntry(Ꮡtest.of(formatTestsᴛ1.Ꮡinput)));
        if (got != test.wantDirEntry) {
            t.Errorf("%d: FormatDirEntry(%#v) = %q, want %q", i, test.input, got, test.wantDirEntry);
        }
    }
}

} // end fs_test_package
