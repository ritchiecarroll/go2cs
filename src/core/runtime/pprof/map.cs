// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using @unsafe = unsafe_package;

partial class pprof_package {

// A profMap is a map from (stack, tag) to mapEntry.
// It grows without bound, but that's assumed to be OK.
partial struct profMap {
    internal map<uintptr, ж<profMapEntry>> hash;
    internal ж<profMapEntry> all;
    internal ж<profMapEntry> last;
    internal slice<profMapEntry> free;
    internal slice<uintptr> freeStk;
}

// A profMapEntry is a single entry in the profMap.
partial struct profMapEntry {
    internal ж<profMapEntry> nextHash; // next in hash list
    internal ж<profMapEntry> nextAll; // next in list of all entries
    internal slice<uintptr> stk;
    internal @unsafe.Pointer tag;
    internal int64 count;
}

internal static ж<profMapEntry> lookup(this ref profMap m, slice<uint64> stk, @unsafe.Pointer tag) {
    // Compute hash of (stk, tag).
    var h = (uintptr)0;
    foreach (var (_, x) in stk) {
        h = (uintptr)((h << (int)(8)) | ((h >> (int)((8 * (/* unsafe.Sizeof(h) */ (uintptr)8 - 1))))));
        h += (uintptr)x * 41;
    }
    h = (uintptr)((h << (int)(8)) | ((h >> (int)((8 * (/* unsafe.Sizeof(h) */ (uintptr)8 - 1))))));
    h += (uintptr)tag * 41;
    // Find entry if present.
    ж<profMapEntry> last = default!;
Search:
    for (var eΔ1 = m.hash[h]; eΔ1 != nil; (last, eΔ1) = (eΔ1, eΔ1.Value.nextHash)) {
        if (len((~eΔ1).stk) != len(stk) || (~eΔ1).tag != tag) {
            continue;
        }
        foreach (var (j, _) in stk) {
            if ((~eΔ1).stk[j] != (uintptr)stk[j]) {
                goto continue_Search;
            }
        }
        // Move to front.
        if (last != nil) {
            last.Value.nextHash = eΔ1.Value.nextHash;
            eΔ1.Value.nextHash = m.hash[h];
            m.hash[h] = eΔ1;
        }
        return eΔ1;
continue_Search:;
    }
break_Search:;
    // Add new entry.
    if (len(m.free) < 1) {
        m.free = new slice<profMapEntry>(128);
    }
    var e = Ꮡ(m.free, 0);
    m.free = m.free[1..];
    e.Value.nextHash = m.hash[h];
    e.Value.tag = tag;
    if (len(m.freeStk) < len(stk)) {
        m.freeStk = new slice<uintptr>(1024);
    }
    // Limit cap to prevent append from clobbering freeStk.
    e.Value.stk = m.freeStk.slice(0, len(stk), len(stk));
    m.freeStk = m.freeStk.slice(len(stk));
    foreach (var (j, _) in stk) {
        e.Value.stk[j] = (uintptr)stk[j];
    }
    if (m.hash == default!) {
        m.hash = new map<uintptr, ж<profMapEntry>>();
    }
    m.hash[h] = e;
    if (m.all == nil){
        m.all = e;
        m.last = e;
    } else {
        m.last.Value.nextAll = e;
        m.last = e;
    }
    return e;
}

} // end pprof_package
