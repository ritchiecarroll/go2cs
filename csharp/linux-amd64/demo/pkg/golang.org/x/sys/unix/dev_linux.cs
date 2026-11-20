// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

partial class unix_package {

public static uint32 Major(uint64 dev) {
    var major = (uint32)((dev & 0x00000000000fff00) >> 8);
    major |= (uint32)((uint32)((dev & 0xfffff00000000000UL) >> 32));
    return major;
}

public static uint32 Minor(uint64 dev) {
    var minor = (uint32)((dev & 0x00000000000000ff) >> 0);
    minor |= (uint32)((uint32)((dev & 0x00000ffffff00000UL) >> 12));
    return minor;
}

public static uint64 Mkdev(uint32 major, uint32 minor) {
    var dev = ((uint64)major & 0x00000fff) << 8;
    dev |= (uint64)((((uint64)major & 0xfffff000U) << 32));
    dev |= (uint64)((((uint64)minor & 0x000000ff) << 0));
    dev |= (uint64)((((uint64)minor & 0xffffff00U) << 12));
    return dev;
}

} // end unix_package
