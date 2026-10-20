// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go2cs NATIVE FORK (hand-owned; replaces the converted poolqueue.go output). The fork is confined to
// poolDequeue's SLOT REPRESENTATION — the packed head/tail, the fullness test, the CAS protocol and
// the whole poolChain half below are Go's, unchanged.
//
// Go's poolDequeue is a lock-free ring of `eface` slots — the raw two-word {type, value} form of an
// `any` — and its entire ownership protocol hangs on the TYPE word: a slot is empty iff typ == nil,
// and a consumer publishes "I am done with this slot" by atomically storing nil into typ alone. That
// is raw-metal on a MANAGED referent. An `any` under the CLR is ONE reference, so the literal
// conversion reinterprets the two-word struct as an `any` and the typ word ends up doing double duty
// as both the type tag and the value. Two failures follow at once: a slot read back through the
// reinterpretation surfaces the type word as the value ("panic: interface conversion: interface {} is
// unsafe.Pointer, not int"), and the empty-slot sentinel — a nil unsafe.Pointer — is indistinguishable
// from a stored value of that type, so pushHead and popTail disagree about who owns a slot and the
// ring corrupts or wedges. TestPoolChain/TestPoolDequeue crashed the test host on it.
//
// The fork follows the ruled precedent (sync's Mutex/notifyList; src/archived/Baseline-vs-FullConversion.md,
// "Hand-owning a package to make it OPERATIONAL"): reimplement the SEMANTIC on managed primitives,
// never emulate the mechanism. A slot becomes one managed reference — which is exactly what an `any`
// is here — and null is the empty-slot sentinel, a state no stored value can forge. That also
// collapses Go's two-step release (write the value word, then publish by storing nil into the type
// word) into a single Volatile.Write of null: with one word there is nothing to tear, so that one
// write both clears the value and hands the slot back to the producer.
using System.Threading;
using go.sync; // atomic.Uint64/Pointer's operations are extension methods in this namespace
using atomic = go.sync.atomic_package;

// Hand-owned native fork of the converted poolqueue.go output — the converter skips regenerating a
// file that carries this marker, so a -stdlib reconvert preserves it (see
// containsManualConversionMarker).
[module: go.GoManualConversion]

namespace go;

partial class sync_package {

// poolDequeue is a lock-free fixed-size single-producer,
// multi-consumer queue. The single producer can both push and pop
// from the head, and consumers can pop from the tail.
//
// It has the added feature that it nils out unused slots to avoid
// unnecessary retention of objects. This is important for sync.Pool,
// but not typically a property considered in the literature.
[GoType] partial struct poolDequeue {
    // headTail packs together a 32-bit head index and a 32-bit
    // tail index. Both are indexes into vals modulo len(vals)-1.
    //
    // tail = index of oldest data in queue
    // head = index of next slot to fill
    //
    // Slots in the range [tail, head) are owned by consumers.
    // A consumer continues to own a slot outside this range until
    // it nils the slot, at which point ownership passes to the
    // producer.
    //
    // The head index is stored in the most-significant bits so
    // that we can atomically add to it and the overflow is
    // harmless.
    internal atomic.Uint64 headTail;
    // vals is a ring buffer of interface{} values stored in this
    // dequeue. The size of this must be a power of 2.
    //
    // vals[i].val is null if the slot is empty and non-null otherwise (Go keys this off the eface's
    // TYPE word instead — see the file header). A slot is still in use until *both* the tail index
    // has moved beyond it and val has been set to null. That null is written atomically by the
    // consumer and read atomically by the producer.
    internal slice<eface> vals;
}

// eface is Go's two-word {type, value} representation of an `any`. Under the CLR an `any` IS a single
// managed reference, so the slot holds that reference directly; see the file header for why the
// two-word form cannot be reinterpreted here.
[GoType] partial struct eface {
    internal any? val;
}

internal static readonly UntypedInt dequeueBits = 32;

// dequeueLimit is the maximum size of a poolDequeue.
//
// This must be at most (1<<dequeueBits)/2 because detecting fullness
// depends on wrapping around the ring buffer without wrapping around
// the index. We divide by 4 so this fits in an int on 32-bit.
internal static readonly UntypedInt dequeueLimit = /* (1 << dequeueBits) / 4 */ 1073741824;

// Go marks a slot that holds a NIL interface value with a typed nil, dequeueNil(nil), which it can
// tell apart from an empty slot because an empty slot's TYPE word is nil. Here the empty slot IS
// null, so the nil-value marker has to be a distinct non-null object of its own; the Go type stays
// declared because it is part of this package's shape (package_info.cs declares its accessibility).
[GoType("ж<EmptyStruct>")] partial class dequeueNil;

private static readonly any dequeueNilValue = new();

[GoRecv] internal static (uint32 head, uint32 tail) unpack(this ref poolDequeue d, uint64 ptrs) {
    uint32 head = default!;
    uint32 tail = default!;

    const uint64 mask = /* 1<<dequeueBits - 1 */ 4294967295;
    head = (uint32)((uint64)(((ptrs >> (int)(dequeueBits))) & mask));
    tail = (uint32)((uint64)(ptrs & mask));
    return (head, tail);
}

[GoRecv] internal static uint64 pack(this ref poolDequeue d, uint32 head, uint32 tail) {
    const uint32 mask = /* 1<<dequeueBits - 1 */ 4294967295;
    return (uint64)((((uint64)head << (int)(dequeueBits))) | (uint64)((uint32)(tail & mask)));
}

// pushHead adds val at the head of the queue. It returns false if the
// queue is full. It must only be called by a single producer.
internal static bool pushHead(this ж<poolDequeue> Ꮡd, any val) {
    ref var d = ref Ꮡd.Value;

    var ptrs = Ꮡd.of(poolDequeue.ᏑheadTail).Load();
    var (head, tail) = d.unpack(ptrs);
    if (unchecked((uint32)(tail + (uint32)len(d.vals))) == head) {
        // Queue is full.
        return false;
    }
    ref var slot = ref d.vals[(nint)((uint32)(head & (uint32)(len(d.vals) - 1)))];
    // Check if the head slot has been released by popTail.
    if (Volatile.Read(ref slot.val) is not null) {
        // Another goroutine is still cleaning up the tail, so
        // the queue is actually still full.
        return false;
    }
    // The head slot is free, so we own it. A nil interface value goes in as the marker, since null
    // means EMPTY here (Go stores dequeueNil(nil) for the same reason).
    // The write also acts as a store barrier for the value: the Add below publishes the slot, and no
    // consumer looks at it until it observes the incremented head.
    Volatile.Write(ref slot.val, val is null or NilType ? dequeueNilValue : val);
    // Increment head. This passes ownership of slot to popTail
    // and acts as a store barrier for writing the slot.
    Ꮡd.of(poolDequeue.ᏑheadTail).Add(((uint64)1 << (int)(dequeueBits)));
    return true;
}

// popHead removes and returns the element at the head of the queue.
// It returns false if the queue is empty. It must only be called by a
// single producer.
internal static (any, bool) popHead(this ж<poolDequeue> Ꮡd) {
    ref var d = ref Ꮡd.Value;

    nint index = 0;
    while (ᐧ) {
        var ptrs = Ꮡd.of(poolDequeue.ᏑheadTail).Load();
        var (head, tail) = d.unpack(ptrs);
        if (tail == head) {
            // Queue is empty.
            return (default!, false);
        }
        // Confirm tail and decrement head. We do this before
        // reading the value to take back ownership of this
        // slot.
        head--;
        var ptrs2 = d.pack(head, tail);
        if (Ꮡd.of(poolDequeue.ᏑheadTail).CompareAndSwap(ptrs, ptrs2)) {
            // We successfully took back slot.
            index = (nint)((uint32)(head & (uint32)(len(d.vals) - 1)));
            break;
        }
    }
    ref var slot = ref d.vals[index];
    any val = Volatile.Read(ref slot.val)!;
    if (ReferenceEquals(val, dequeueNilValue)) {
        val = default!;
    }
    // Zero the slot. Unlike popTail, this isn't racing with
    // pushHead, so we don't need to be careful here.
    Volatile.Write(ref slot.val, null);
    return (val, true);
}

// popTail removes and returns the element at the tail of the queue.
// It returns false if the queue is empty. It may be called by any
// number of consumers.
internal static (any, bool) popTail(this ж<poolDequeue> Ꮡd) {
    ref var d = ref Ꮡd.Value;

    nint index = 0;
    while (ᐧ) {
        var ptrs = Ꮡd.of(poolDequeue.ᏑheadTail).Load();
        var (head, tail) = d.unpack(ptrs);
        if (tail == head) {
            // Queue is empty.
            return (default!, false);
        }
        // Confirm head and tail (for our speculative check
        // above) and increment tail. If this succeeds, then
        // we own the slot at tail.
        var ptrs2 = d.pack(head, tail + 1);
        if (Ꮡd.of(poolDequeue.ᏑheadTail).CompareAndSwap(ptrs, ptrs2)) {
            // Success.
            index = (nint)((uint32)(tail & (uint32)(len(d.vals) - 1)));
            break;
        }
    }
    // We now own slot.
    ref var slot = ref d.vals[index];
    any val = Volatile.Read(ref slot.val)!;
    if (ReferenceEquals(val, dequeueNilValue)) {
        val = default!;
    }
    // Tell pushHead that we're done with this slot. Zeroing the
    // slot is also important so we don't leave behind references
    // that could keep this object live longer than necessary.
    //
    // Go writes the value word first and then publishes the release by atomically storing nil into
    // the type word; one managed reference does both in this single write (see the file header).
    Volatile.Write(ref slot.val, null);
    // At this point pushHead owns the slot.
    return (val, true);
}

// poolChain is a dynamically-sized version of poolDequeue.
//
// This is implemented as a doubly-linked list queue of poolDequeues
// where each dequeue is double the size of the previous one. Once a
// dequeue fills up, this allocates a new one and only ever pushes to
// the latest dequeue. Pops happen from the other end of the list and
// once a dequeue is exhausted, it gets removed from the list.
[GoType] partial struct poolChain {
    // head is the poolDequeue to push to. This is only accessed
    // by the producer, so doesn't need to be synchronized.
    internal ж<poolChainElt> head;
    // tail is the poolDequeue to popTail from. This is accessed
    // by consumers, so reads and writes must be atomic.
    internal atomic.Pointer<poolChainElt> tail;
}

[GoType] partial struct poolChainElt {
    /*embed*/ internal poolDequeue poolDequeue;
    // next and prev link to the adjacent poolChainElts in this
    // poolChain.
    //
    // next is written atomically by the producer and read
    // atomically by the consumer. It only transitions from nil to
    // non-nil.
    //
    // prev is written atomically by the consumer and read
    // atomically by the producer. It only transitions from
    // non-nil to nil.
    internal atomic.Pointer<poolChainElt> next, prev;
}

internal static void pushHead(this ж<poolChain> Ꮡc, any val) {
    ref var c = ref Ꮡc.Value;

    var d = c.head;
    if (d == nil) {
        // Initialize the chain.
        const nint initSize = 8; // Must be a power of 2
        d = @new<poolChainElt>();
        d.Value.vals = new slice<eface>(initSize);
        c.head = d;
        Ꮡc.of(poolChain.Ꮡtail).Store(d);
    }
    if (d.of(poolChainElt.ᏑpoolDequeue).pushHead(val)) {
        return;
    }
    // The current dequeue is full. Allocate a new one of twice
    // the size.
    nint newSize = len((~d).vals) * 2;
    if (newSize >= dequeueLimit) {
        // Can't make it any bigger.
        newSize = dequeueLimit;
    }
    var d2 = Ꮡ(new poolChainElt(nil));
    d2.of(poolChainElt.Ꮡprev).Store(d);
    d2.Value.vals = new slice<eface>(newSize);
    c.head = d2;
    d.of(poolChainElt.Ꮡnext).Store(d2);
    d2.of(poolChainElt.ᏑpoolDequeue).pushHead(val);
}

[GoRecv] internal static (any, bool) popHead(this ref poolChain c) {
    var d = c.head;
    while (d != nil) {
        {
            var (val, ok) = d.of(poolChainElt.ᏑpoolDequeue).popHead(); if (ok) {
                return (val, ok);
            }
        }
        // There may still be unconsumed elements in the
        // previous dequeue, so try backing up.
        d = d.of(poolChainElt.Ꮡprev).Load();
    }
    return (default!, false);
}

internal static (any, bool) popTail(this ж<poolChain> Ꮡc) {
    var d = Ꮡc.of(poolChain.Ꮡtail).Load();
    if (d == nil) {
        return (default!, false);
    }
    while (ᐧ) {
        // It's important that we load the next pointer
        // *before* popping the tail. In general, d may be
        // transiently empty, but if next is non-nil before
        // the pop and the pop fails, then d is permanently
        // empty, which is the only condition under which it's
        // safe to drop d from the chain.
        var d2 = d.of(poolChainElt.Ꮡnext).Load();
        {
            var (val, ok) = d.of(poolChainElt.ᏑpoolDequeue).popTail(); if (ok) {
                return (val, ok);
            }
        }
        if (d2 == nil) {
            // This is the only dequeue. It's empty right
            // now, but could be pushed to in the future.
            return (default!, false);
        }
        // The tail of the chain has been drained, so move on
        // to the next dequeue. Try to drop it from the chain
        // so the next pop doesn't have to look at the empty
        // dequeue again.
        if (Ꮡc.of(poolChain.Ꮡtail).CompareAndSwap(d, d2)) {
            // We won the race. Clear the prev pointer so
            // the garbage collector can collect the empty
            // dequeue and so popHead doesn't back up
            // further than necessary.
            d2.of(poolChainElt.Ꮡprev).Store(nil);
        }
        d = d2;
    }
}

} // end sync_package
