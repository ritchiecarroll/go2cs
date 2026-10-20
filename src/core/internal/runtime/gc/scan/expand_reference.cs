// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime.gc;

using goarch = go.@internal.goarch_package;
using gc = go.@internal.runtime.gc_package;
using go.@internal;
using go.@internal.runtime;

partial class scan_package {

// ExpandReference is a reference implementation of an expander function
// that translates object mark bits into a bitmap of one bit per word of
// marked object, assuming the object is of the provided size class.
public static void ExpandReference(nint sizeClass, ж<gc.ObjMask> Ꮡpacked, ж<gc.PtrMask> Ꮡunpacked) {
    ref var packed = ref Ꮡpacked.DerefOrNull();
    ref var unpacked = ref Ꮡunpacked.DerefOrNull();

    // Look up the size and derive the number of objects in a span.
    // We're only concerned with small objects in single-page spans,
    // and gc.PtrMask enforces this by being statically sized to
    // accommodate only such spans.
    var size = (uintptr)gc.SizeClassToSize[sizeClass];
    var nObj = (uintptr)gc.SizeClassToNPages[sizeClass] * (uintptr)gc.PageSize / size;

    // f is the expansion factor. For example, if our objects are of size 48,
    // then each mark bit will translate into 6 (48/8 = 6) set bits in the
    // pointer bitmap.
    var f = size / (uintptr)goarch.PtrSize;
    foreach (var i in range<uintptr>(nObj)) {
        // Check if the object is marked.
        if ((uintptr)(packed[i / (uintptr)goarch.PtrBits] & ((uintptr)1 << (int)((i % (uintptr)goarch.PtrBits)))) == 0) {
            continue;
        }
        // Propagate that mark into the destination into one bit per the
        // expansion factor f, offset to the object's offset within the span.
        foreach (var j in range<uintptr>(f)) {
            var b = i * f + j; // i*f is the start bit for the object, j indexes into each corresponding word after.
            unpacked[b / (uintptr)goarch.PtrBits] |= (uintptr)(((uintptr)1 << (int)((b % (uintptr)goarch.PtrBits))));
        }
    }
}

} // end scan_package
