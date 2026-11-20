// Copyright 2026 The Memory Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using golang.org.x.sys;

partial class memory_package {

internal const bool canDecommit = true;

internal static void decommit(uintptr p, nint size) {
    if (size <= osPageSize) {
        return;
    }

    procVirtualFree.Call(p + (uintptr)osPageSize, (uintptr)(size - osPageSize), _MEM_DECOMMIT);
}

internal static error recommit(uintptr p, nint size) {
    if (size <= osPageSize) {
        return default;
    }

    var (addr, _, err) = procVirtualAlloc.Call(p + (uintptr)osPageSize, (uintptr)(size - osPageSize), _MEM_COMMIT, _PAGE_READWRITE);
    if (addr == 0) {
        return err;
    }

    return default;
}

} // end memory_package
