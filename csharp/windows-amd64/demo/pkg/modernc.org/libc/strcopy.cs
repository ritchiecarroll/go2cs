// Copyright 2026 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using bytes = bytes_package;
using @unsafe = unsafe_package;

partial class libc_package {

internal static Tsize_t strnlen(uintptr s, Tsize_t n) {
    var p = s;
    while (n != 0) {
        var chunk = 4096 - p % 4096;
        if ((uintptr)n < chunk) {
            chunk = (uintptr)n;
        }
        nint i = bytes.IndexByte(@unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)p), chunk), 0); if (i >= 0) {
            return (Tsize_t)(p + (uintptr)i - s);
        }
        p += chunk;
        n -= ((Tsize_t)chunk);
    }
    return (Tsize_t)(p - s);
}

internal static uintptr strncat(uintptr dest, uintptr src, Tsize_t n) {
    var d = dest;
    for (nint i = 0; i < 16 && ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)d) != 0; i++) {
        d++;
    }
    if (~@unsafe.Pointer.To<byte>((@unsafe.Pointer)d) != 0) {
        d += (uintptr)strlen(d);
    }
    for (nint i = 0; i < 16 && n != 0; i++) {
        var c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)src);
        if (c == 0) {
            break;
        }

        (@unsafe.Pointer.To<byte>((@unsafe.Pointer)d)).Value = c;
        d++;
        src++;
        n--;
    }
    if (n == 0 || ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)src) == 0) {
        (@unsafe.Pointer.To<byte>((@unsafe.Pointer)d)).Value = 0;
        return dest;
    }

    var k = strnlen(src, n);
    var b = @unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)d), k + 1);
    copy(b, @unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)src), k));
    b[k] = 0;
    return dest;
}

} // end libc_package
