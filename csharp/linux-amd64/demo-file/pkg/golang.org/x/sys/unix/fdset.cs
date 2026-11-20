// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using static unix_package;

partial class unix_package {

} partial class unixᴛFdSet {

public static void Set(this ref FdSet fds, nint fd) {
    fds.Bits[fd / (nint)NFDBITS] |= (int64)(((int64)1 << (int)((uintptr)fd % (uintptr)NFDBITS)));
}

public static void Clear(this ref FdSet fds, nint fd) {
    fds.Bits[fd / (nint)NFDBITS] &= ~(int64)(((int64)1 << (int)((uintptr)fd % (uintptr)NFDBITS)));
}

public static bool IsSet(this ref FdSet fds, nint fd) {
    return (fds.Bits[fd / (nint)NFDBITS] & ((int64)1 << (int)((uintptr)fd % (uintptr)NFDBITS))) != 0;
}

public static void Zero(this ref FdSet fds) {
    clear(fds.Bits[..]);
}

} // end unixᴛFdSet
