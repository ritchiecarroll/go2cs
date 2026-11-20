// Copyright 2026 The Memory Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using @unsafe = unsafe_package;
using unix = golang.org.x.sys.unix_package;
using golang.org.x.sys;

partial class memory_package {

internal const bool canDecommit = true;

internal static UntypedInt madviseAdvice => /* unix.MADV_DONTNEED */ 4;

internal static void decommit(uintptr p, nint size) {
    if (size <= osPageSize) {
        return;
    }

    nint n = size - osPageSize;
    unix.Madvise(@unsafe.Pointer.View<byte>((@unsafe.Pointer)(p + (uintptr)osPageSize), 0, n, n, 1125899906842623), madviseAdvice);
}

internal static error recommit(uintptr _Δp0, nint _Δp1) { return default; }

} // end memory_package
