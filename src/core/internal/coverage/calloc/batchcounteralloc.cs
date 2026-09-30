// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

partial class calloc_package {

// This package contains a simple "batch" allocator for allocating
// coverage counters (slices of uint32 basically), for working with
// coverage data files. Collections of counter arrays tend to all be
// live/dead over the same time period, so a good fit for batch
// allocation.
[GoType] partial struct BatchCounterAlloc {
    internal slice<uint32> pool;
}

[GoRecv] public static slice<uint32> AllocateCounters(this ref BatchCounterAlloc ca, nint n) {
    const nint chunk = 8192;
    if (n > cap(ca.pool)) {
        nint siz = chunk;
        if (n > chunk) {
            siz = n;
        }
        ca.pool = new slice<uint32>(siz);
    }
    var rv = ca.pool.slice(0, n);
    ca.pool = ca.pool.slice(n);
    return rv;
}

} // end calloc_package
