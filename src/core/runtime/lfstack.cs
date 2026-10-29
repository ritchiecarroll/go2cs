// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Lock-free stack.
namespace go;

using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

// lfstack is the head of a lock-free stack.
//
// The zero value of lfstack is an empty list.
//
// This stack is intrusive. Nodes must embed lfnode as the first field.
//
// The stack does not keep GC-visible pointers to nodes, so the caller
// must ensure the nodes are allocated outside the Go heap.
partial struct lfstack /*num:uint64*/;

internal static void push(this ж<lfstack> Ꮡhead, ж<lfnode> Ꮡnode) {
    ref var node = ref Ꮡnode.DerefOrNull();

    node.pushcnt++;
    var @new = lfstackPack(Ꮡnode, node.pushcnt);
    for (;;) {
        var old = atomic.Load64(Ꮡhead.Reinterpret<lfstack, uint64>());
        node.next = old;
        if (atomic.Cas64(Ꮡhead.Reinterpret<lfstack, uint64>(), old, @new)) {
            break;
        }
    }
}

internal static @unsafe.Pointer pop(this ж<lfstack> Ꮡhead) {
    uint32 backoff = default;
    // TODO: tweak backoff parameters on other architectures.
    if (GOARCH == "arm64"u8) {
        backoff = 128;
    }
    for (;;) {
        var old = atomic.Load64(Ꮡhead.Reinterpret<lfstack, uint64>());
        if (old == 0) {
            return default;
        }
        var node = lfstackUnpack(old);
        var next = atomic.Load64(node.of(lfnode.Ꮡnext));
        if (atomic.Cas64(Ꮡhead.Reinterpret<lfstack, uint64>(), old, next)) {
            return @unsafe.Pointer.FromPinnedBox(node);
        }

        // Use a backoff approach to reduce demand to the shared memory location
        // decreases memory contention and allows for other threads to make quicker
        // progress.
        // Read more in this Arm blog post:
        // https://community.arm.com/arm-community-blogs/b/architectures-and-processors-blog/posts/multi-threaded-applications-arm
        procyield(backoff);
        // Increase backoff time.
        backoff += backoff / 2;
    }
}

internal static bool empty(this ж<lfstack> Ꮡhead) {
    return atomic.Load64(Ꮡhead.Reinterpret<lfstack, uint64>()) == 0;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lfstackNodeAllocatedFromˢ = "lfstack node allocated from the heap"u8;

// lfnodeValidate panics if node is not a valid address for use with
// lfstack.push. This only needs to be called when node is allocated.
internal static void lfnodeValidate(ж<lfnode> node) {
    {
        var (@base, _, _) = findObject((uintptr)node, 0, 0); if (@base != 0) {
            @throw(lfstackNodeAllocatedFromˢ);
        }
    }
    lfstackPack(node, ~(uintptr)0);
}

internal static uint64 lfstackPack(ж<lfnode> node, uintptr cnt) {
    return (uint64)taggedPointerPack(@unsafe.Pointer.FromPinnedBox(node), (uintptr)(cnt & (uintptr)((1 << (int)(tagBits)) - 1)));
}

internal static ж<lfnode> lfstackUnpack(uint64 val) {
    return (ж<lfnode>)(uintptr)(((taggedPointer)val).pointer());
}

} // end runtime_package
