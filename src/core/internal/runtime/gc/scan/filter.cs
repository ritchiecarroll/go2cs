// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime.gc;

using @unsafe = unsafe_package;

partial class scan_package {

// FilterNil packs non-nil (non-zero) values in bufp together
// at the beginning of bufp, returning the length of the
// packed buffer. It treats bufp as an array of size n.
public static int32 FilterNil(ж<uintptr> Ꮡbufp, int32 n) {
    var buf = @unsafe.Slice(Ꮡbufp, (nint)n);
    nint lo = 0;
    nint hi = len(buf) - 1;
    while (lo < hi) {
        while (lo < hi && buf[hi] == 0) {
            hi--;
        }
        while (lo < hi && buf[lo] != 0) {
            lo++;
        }
        if (lo >= hi) {
            break;
        }
        buf[lo] = buf[hi];
        hi--;
    }
    if (hi >= 0 && buf[hi] == 0) {
        hi--;
    }
    return (int32)hi + 1;
}

} // end scan_package
