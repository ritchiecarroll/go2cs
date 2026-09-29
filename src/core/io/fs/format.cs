// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.io;

using time = time_package;

partial class fs_package {

// FormatFileInfo returns a formatted version of info for human readability.
// Implementations of [FileInfo] can call this from a String method.
// The output for a file named "hello.go", 100 bytes, mode 0o644, created
// January 1, 1970 at noon is
//
//	-rw-r--r-- 100 1970-01-01 12:00:00 hello.go
public static @string FormatFileInfo(FileInfo info) {
    @string name = info.Name();
    var b = new slice<byte>(0, 40 + len(name));
    b = append(b, info.Mode().String().ꓸꓸꓸ);
    b = append(b, (byte)((rune)' '));
    var size = info.Size();
    uint64 usize = default!;
    if (size >= 0){
        usize = (uint64)size;
    } else {
        b = append(b, (byte)((rune)'-'));
        usize = (uint64)(-size);
    }
    array<byte> buf = new(20);
    nint i = len(buf) - 1;
    while (usize >= 10) {
        var q = usize / 10;
        buf[i] = (byte)((rune)'0' + usize - q * 10);
        i--;
        usize = q;
    }
    buf[i] = (byte)((rune)'0' + usize);
    b = appendꓸꓸꓸ(b, buf.slice(i));
    b = append(b, (byte)((rune)' '));
    b = append(b, info.ModTime().Format(time.DateTime).ꓸꓸꓸ);
    b = append(b, (byte)((rune)' '));
    b = append(b, name.ꓸꓸꓸ);
    if (info.IsDir()) {
        b = append(b, (byte)((rune)'/'));
    }
    return ((@string)b);
}

// FormatDirEntry returns a formatted version of dir for human readability.
// Implementations of [DirEntry] can call this from a String method.
// The outputs for a directory named subdir and a file named hello.go are:
//
//	d subdir/
//	- hello.go
public static @string FormatDirEntry(DirEntry dir) {
    @string name = dir.Name();
    var b = new slice<byte>(0, 5 + len(name));
    // The Type method does not return any permission bits,
    // so strip them from the string.
    @string mode = dir.Type().String();
    mode = mode.slice(0, len(mode) - 9);
    b = append(b, mode.ꓸꓸꓸ);
    b = append(b, (byte)((rune)' '));
    b = append(b, name.ꓸꓸꓸ);
    if (dir.IsDir()) {
        b = append(b, (byte)((rune)'/'));
    }
    return ((@string)b);
}

} // end fs_package
