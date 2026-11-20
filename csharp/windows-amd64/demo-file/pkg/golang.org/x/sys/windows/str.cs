// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

partial class windows_package {

internal static @string itoa(nint val) {
    if (val < 0) {
        return "-"u8 + itoa(-val);
    }
    array<byte> buf = new(32);
    nint i = len(buf) - 1;
    while (val >= 10) {
        buf[i] = (byte)(val % 10 + '0');
        i--;
        val /= 10;
    }
    buf[i] = (byte)(val + '0');
    return (@string)(buf.slice(i));
}

} // end windows_package
