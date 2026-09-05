// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Trace stack table and acquisition.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using atomic = @internal.runtime.atomic_package;

partial class runtime_package {

// traceTypeTable maps stack traces (arrays of PC's) to unique uint32 ids.
// It is lock-free for reading.
[GoType] partial struct traceTypeTable {
    internal traceMap tab;
}

// put returns a unique id for the type typ and caches it in the table,
// if it's seeing it for the first time.
//
// N.B. typ must be kept alive forever for this to work correctly.
internal static uint64 put(this ж<traceTypeTable> Ꮡt, ж<abi.Type> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if (Ꮡtyp == nil) {
        return 0;
    }
    // Insert the pointer to the type itself.
    var (id, _) = Ꮡt.of(traceTypeTable.Ꮡtab).put((uintptr)noescape(@unsafe.Pointer.FromBox(Ꮡ(typ))), goarch.PtrSize);
    System.GC.KeepAlive(Ꮡtyp);
    return id;
}

// dump writes all previously cached types to trace buffers and
// releases all memory and resets state. It must only be called once the caller
// can guarantee that there are no more writers to the table.
internal static void dump(this ж<traceTypeTable> Ꮡt, uintptr gen) {
    var w = unsafeTraceExpWriter(gen, nil, traceExperimentAllocFree);
    {
        var root = (ж<traceMapNode>)(uintptr)(Ꮡt.of(traceTypeTable.Ꮡtab).of(traceMap.Ꮡroot).Load()); if (root != nil) {
            w = dumpTypesRec(root, w);
        }
    }
    w.flush().end();
    Ꮡt.of(traceTypeTable.Ꮡtab).reset();
}

internal static traceExpWriter dumpTypesRec(ж<traceMapNode> Ꮡnode, traceExpWriter w) {
    ref var node = ref Ꮡnode.DerefOrNull();

    var typ = (ж<abi.Type>)(uintptr)(~Ꮡ(new @unsafe.Pointer((uintptr)Ꮡ(node.data, 0))));
    @string typName = toRType(typ).@string();
    // The maximum number of bytes required to hold the encoded type.
    nint maxBytes = (nint)(1 + 5 * traceBytesPerNumber) + len(typName);
    // Estimate the size of this record. This
    // bound is pretty loose, but avoids counting
    // lots of varint sizes.
    //
    // Add 1 because we might also write a traceAllocFreeTypesBatch byte.
    bool flushed = default!;
    (w, flushed) = w.ensure(1 + maxBytes);
    if (flushed) {
        // Annotate the batch as containing types.
        w.@byte((byte)traceAllocFreeTypesBatch);
    }
    // Emit type.
    w.varint((uint64)node.id);
    w.varint((uint64)(uintptr)typ);
    w.varint((uint64)typ.Size());
    w.varint((uint64)(~typ).PtrBytes);
    w.varint((uint64)len(typName));
    w.stringData(typName);
    // Recursively walk all child nodes.
    foreach (var (i, _) in node.children) {
        @unsafe.Pointer child = (uintptr)Ꮡnode.at(traceMapNode.Ꮡchildren, i).Load();
        if (child == nil) {
            continue;
        }
        w = dumpTypesRec((ж<traceMapNode>)(uintptr)(child), w);
    }
    return w;
}

} // end runtime_package
