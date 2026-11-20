// Copyright 2026 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using bytes = bytes_package;
using math = math_package;
using bits = go.math.bits_package;
using @unsafe = unsafe_package;
using go.math;

partial class libc_package {

internal static Tsize_t strlenIndexByte(uintptr s) {
    var p = s;
    for (;;) {
        var chunk = 4096 - p % 4096;
        var b = @unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)p), chunk);
        nint i = bytes.IndexByte(b, 0); if (i >= 0) {
            return (Tsize_t)(p + (uintptr)i - s);
        }
        p += chunk;
    }
}

internal static UntypedInt strlenWordSize => /* bits.UintSize / 8 */ 8;
internal static UntypedInt strlenLowOnes => /* math.MaxUint / 0xff */ 72340172838076673;
internal static UntypedInt strlenHighBits => /* strlenLowOnes << 7 */ 9259542123273814144;

internal static Tsize_t strlenWords(uintptr s) {
    var p = s;
    for (; p % (uintptr)strlenWordSize != 0; p++) {
        if (~@unsafe.Pointer.To<byte>((@unsafe.Pointer)p) == 0) {
            return (Tsize_t)(p - s);
        }
    }
    for (;;) {
        nuint x = ~@unsafe.Pointer.To<nuint>((@unsafe.Pointer)p);
        if (((x - (nuint)strlenLowOnes) & ~x & (nuint)strlenHighBits) != 0) {
            break;
        }
        p += strlenWordSize;
    }
    while (~@unsafe.Pointer.To<byte>((@unsafe.Pointer)p) != 0) {
        p++;
    }
    return (Tsize_t)(p - s);
}

} // end libc_package
