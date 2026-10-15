// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

using goarch = go.@internal.goarch_package;
using @unsafe = unsafe_package;
using go.@internal;

partial class atomic_package {

//go:nosplit
internal static uint8 goXchg8(ж<uint8> Ꮡaddr, uint8 v) {
    // Align down to 4 bytes and use 32-bit CAS.
    var addr32 = (ж<uint32>)(uintptr)((@unsafe.Pointer)((uintptr)((uintptr)Ꮡaddr & ~(uintptr)3)));
    var shift = ((uintptr)((uintptr)Ꮡaddr & 3));
    if (goarch.BigEndian) {
        shift = (uintptr)(shift ^ 3);
    }
    shift = shift * 8;
    var word = ((uint32)v).Lsh((uint64)(shift));
    var mask = ((uint32)0xFF).Lsh((uint64)(shift));

    while (ᐧ) {
        var old = addr32.Value; // Read the old 32-bit value
        // Clear the old 8 bits then insert the new value
        if (Cas(addr32, old, (uint32)(((uint32)(old & ~mask)) | word))) {
            // Return the old 8-bit value
            return (uint8)(((uint32)(old & mask)).Rsh((uint64)(shift)));
        }
    }
}

} // end atomic_package
