// Copyright 2026 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

partial class libc_package {

public static int8 /*val*/ AtomicLoadPInt8(uintptr addr) {
    return (int8)a_load_8(addr);
}

public static int16 /*val*/ AtomicLoadPInt16(uintptr addr) {
    return (int16)a_load_16(addr);
}

public static byte AtomicLoadPUint8(uintptr addr) {
    return (byte)a_load_8(addr);
}

public static uint16 AtomicLoadPUint16(uintptr addr) {
    return (uint16)a_load_16(addr);
}

} // end libc_package
