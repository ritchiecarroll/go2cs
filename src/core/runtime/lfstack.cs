// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Lock-free stack.
namespace go;

using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

partial struct lfstack /*num:uint64*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lfstackPushˢ = "lfstack.push"u8;

internal static void push(this ж<lfstack> Ꮡhead, ж<lfnode> Ꮡnode) {
    ref var node = ref Ꮡnode.DerefOrNull();

    node.pushcnt++;
    var @new = lfstackPack(Ꮡnode, node.pushcnt);
    {
        var node1 = lfstackUnpack(@new); if (node1 != Ꮡnode) {
            print((@string)"runtime: lfstack.push invalid packing: node="u8, Ꮡnode.OrTypedNil(), (@string)" cnt="u8, ((Δhex)(uint64)node.pushcnt), (@string)" packed="u8, ((Δhex)@new), (@string)" -> node="u8, node1.OrTypedNil(), (@string)"\n"u8);
            @throw(lfstackPushˢ);
        }
    }
    while (ᐧ) {
        var old = atomic.Load64(Ꮡhead.Reinterpret<lfstack, uint64>());
        node.next = old;
        if (atomic.Cas64(Ꮡhead.Reinterpret<lfstack, uint64>(), old, @new)) {
            break;
        }
    }
}

internal static @unsafe.Pointer pop(this ж<lfstack> Ꮡhead) {
    while (ᐧ) {
        var old = atomic.Load64(Ꮡhead.Reinterpret<lfstack, uint64>());
        if (old == 0) {
            return default!;
        }
        var node = lfstackUnpack(old);
        var next = atomic.Load64(node.of(lfnode.Ꮡnext));
        if (atomic.Cas64(Ꮡhead.Reinterpret<lfstack, uint64>(), old, next)) {
            return @unsafe.Pointer.FromPinnedBox(node);
        }
    }
}

internal static bool empty(this ж<lfstack> Ꮡhead) {
    return atomic.Load64(Ꮡhead.Reinterpret<lfstack, uint64>()) == 0;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lfstackNodeAllocatedFromˢ = "lfstack node allocated from the heap"u8;
internal static readonly @string badLfnodeAddressˢ = "bad lfnode address"u8;

// lfnodeValidate panics if node is not a valid address for use with
// lfstack.push. This only needs to be called when node is allocated.
internal static void lfnodeValidate(ж<lfnode> Ꮡnode) {
    {
        var (@base, _, _) = findObject((uintptr)Ꮡnode, 0, 0); if (@base != 0) {
            @throw(lfstackNodeAllocatedFromˢ);
        }
    }
    if (lfstackUnpack(lfstackPack(Ꮡnode, ~(uintptr)0)) != Ꮡnode) {
        printlock();
        println((@string)"runtime: bad lfnode address"u8, ((Δhex)(uint64)(uintptr)Ꮡnode));
        @throw(badLfnodeAddressˢ);
    }
}

internal static uint64 lfstackPack(ж<lfnode> Ꮡnode, uintptr cnt) {
    return (uint64)taggedPointerPack(@unsafe.Pointer.FromPinnedBox(Ꮡnode), cnt);
}

internal static ж<lfnode> lfstackUnpack(uint64 val) {
    return (ж<lfnode>)(uintptr)(((taggedPointer)val).pointer());
}

} // end runtime_package
