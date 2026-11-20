// Copyright 2023 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class libc_package {

internal static uint32 a_load_8(uintptr addr) {
    return (uint32)(~@unsafe.Pointer.To<byte>((@unsafe.Pointer)addr));
}

internal static uint32 a_load_16(uintptr addr) {
    if ((uintptr)(addr & 1) != 0) {
        throw panic(fmt.Errorf("unaligned atomic 16 bit access at %#0x"u8, addr));
    }

    return (uint32)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)addr));
}

internal static void a_store_8(uintptr addr, byte b) {
    (@unsafe.Pointer.To<byte>((@unsafe.Pointer)addr)).Value = b;
}

internal static void a_store_16(uintptr addr, uint16 n) {
    if ((uintptr)(addr & 1) != 0) {
        throw panic(fmt.Errorf("unaligned atomic 16 bit access at %#0x"u8, addr));
    }

    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)addr)).Value = n;
}

} // end libc_package
