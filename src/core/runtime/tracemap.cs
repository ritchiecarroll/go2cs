// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Simple append-only thread-safe hash map for tracing.
// Provides a mapping between variable-length data and a
// unique ID. Subsequent puts of the same data will return
// the same ID. The zero value is ready to use.
//
// Uses a region-based allocation scheme internally, and
// reset clears the whole map.
//
// It avoids doing any high-level Go operations so it's safe
// to use even in sensitive contexts.
namespace go;

using cpu = @internal.cpu_package;
using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

partial struct traceMap {
    internal atomic.UnsafePointer root; // *traceMapNode (can't use generics because it's notinheap)
    internal cpu.CacheLinePad _;
    internal atomic.Uint64 seq;
    internal cpu.CacheLinePad __;
    internal traceRegionAlloc mem;
}

// traceMapNode is an implementation of a lock-free append-only hash-trie
// (a trie of the hash bits).
//
// Key features:
//   - 4-ary trie. Child nodes are indexed by the upper 2 (remaining) bits of the hash.
//     For example, top level uses bits [63:62], next level uses [61:60] and so on.
//   - New nodes are placed at the first empty level encountered.
//   - When the first child is added to a node, the existing value is not moved into a child.
//     This means that you must check the key at each level, not just at the leaf.
//   - No deletion or rebalancing.
//   - Intentionally devolves into a linked list on hash collisions (the hash bits will all
//     get shifted out during iteration, and new nodes will just be appended to the 0th child).
partial struct traceMapNode {
    internal sys.NotInHeap _;
    internal array<atomic.UnsafePointer> children = new(4); // *traceMapNode (can't use generics because it's notinheap)
    internal uintptr hash;
    internal uint64 id;
    internal slice<byte> data;
}

// stealID steals an ID from the table, ensuring that it will not
// appear in the table anymore.
internal static uint64 stealID(this ж<traceMap> Ꮡtab) {
    return Ꮡtab.of(traceMap.Ꮡseq).Add(1);
}

// put inserts the data into the table.
//
// It's always safe for callers to noescape data because put copies its bytes.
//
// Returns a unique ID for the data and whether this is the first time
// the data has been added to the map.
internal static (uint64, bool) put(this ж<traceMap> Ꮡtab, @unsafe.Pointer data, uintptr size) {
    if (size == 0) {
        return (0, false);
    }
    var hash = memhash(data, 0, size);
    ж<traceMapNode> newNode = default!;
    var m = Ꮡtab.of(traceMap.Ꮡroot);
    var hashIter = hash;
    while (ᐧ) {
        var n = (ж<traceMapNode>)(uintptr)(m.Load());
        if (n == nil) {
            // Try to insert a new map node. We may end up discarding
            // this node if we fail to insert because it turns out the
            // value is already in the map.
            //
            // The discard will only happen if two threads race on inserting
            // the same value. Both might create nodes, but only one will
            // succeed on insertion. If two threads race to insert two
            // different values, then both nodes will *always* get inserted,
            // because the equality checking below will always fail.
            //
            // Performance note: contention on insertion is likely to be
            // higher for small maps, but since this data structure is
            // append-only, either the map stays small because there isn't
            // much activity, or the map gets big and races to insert on
            // the same node are much less likely.
            if (newNode == nil) {
                newNode = Ꮡtab.newTraceMapNode(data, size, hash, Ꮡtab.of(traceMap.Ꮡseq).Add(1));
            }
            if (m.CompareAndSwapNoWB(nil, @unsafe.Pointer.FromPinnedBox(newNode))) {
                return ((~newNode).id, true);
            }
            // Reload n. Because pointers are only stored once,
            // we must have lost the race, and therefore n is not nil
            // anymore.
            n = (ж<traceMapNode>)(uintptr)(m.Load());
        }
        if ((~n).hash == hash && (uintptr)len((~n).data) == size) {
            if (memequal(@unsafe.Pointer.FromPinnedBox(Ꮡ((~n).data, 0)), data, size)) {
                return ((~n).id, false);
            }
        }
        m = n.at(traceMapNode.Ꮡchildren, (ulong)((hashIter >> (int)((8 * goarch.PtrSize - 2)))));
        hashIter <<= (int)(2);
    }
}

// go2cs generated this placeholder — func newTraceMapNode is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// reset drops all allocated memory from the table and resets it.
//
// The caller must ensure that there are no put operations executing concurrently
// with this function.
internal static void reset(this ж<traceMap> Ꮡtab) {
    Ꮡtab.of(traceMap.Ꮡroot).Store(nil);
    Ꮡtab.of(traceMap.Ꮡseq).Store(0);
    Ꮡtab.of(traceMap.Ꮡmem).drop();
}

} // end runtime_package
