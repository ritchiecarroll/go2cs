// tracemap_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// go2cs code converter hand-conversion of tracemap.go's newTraceMapNode.
//
// newTraceMapNode (traceMap.newTraceMapNode) is HAND-OWNED because Go's body lays a traceMapNode out
// in Go-layout memory. It takes the node's bytes and then the node itself from the table's
// traceRegionAlloc (tab.mem), whose blocks come from sysAlloc, and reinterprets the second
// allocation as a traceMapNode. That struct holds references (children, four atomic pointers, and
// data, a slice), so the managed host cannot place it at a byte offset in storage it does not lay
// out itself (the arm-2a class; see mranges_impl.cs). The converted body never got that far: the
// sysAlloc'd block has no managed data array, and indexing it threw IndexOutOfRangeException
// (traceregion.cs:88), which on the goroutines of the runtime row's TestTraceMapConcurrent ended the
// host.
//
// This body allocates the node as a managed object and copies the value's bytes into a managed
// slice, then sets id and hash as Go does. put links the node in by FromPinnedBox and reads it back
// through the same token, as it did before.
//
// DIVERGENCE: the nodes no longer come from tab.mem, so traceMap.reset (which drops tab.mem) does not
// free them; they are collected once reset has unlinked the root and no reader holds one. Go frees the
// region's blocks at reset. tab.mem now allocates nothing, so reset's drop has no blocks to free.
// newTraceMapNode was traceRegionAlloc.alloc's only converted caller.

using go.golib;
using @unsafe = go.unsafe_package;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    internal static ж<traceMapNode> newTraceMapNode(this ж<traceMap> Ꮡtab, @unsafe.Pointer data, uintptr size, uintptr hash, uint64 id)
    {
        // Create data array.
        slice<byte> copied = new slice<byte>((nint)size);
        copy(copied, @unsafe.Slice((ж<byte>)(uintptr)data, size));

        // Create metadata structure.
        ж<traceMapNode> meta = @new<traceMapNode>();
        meta.Value.data = copied;
        meta.Value.id = id;
        meta.Value.hash = hash;
        return meta;
    }
}
