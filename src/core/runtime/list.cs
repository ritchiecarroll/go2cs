// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

// listHead points to the head of an intrusive doubly-linked list.
//
// Prior to use, you must call init to store the offset of listNode fields.
//
// Every object in the list should be the same type.
partial struct listHead {
    internal @unsafe.Pointer obj;
    internal bool initialized;
    internal uintptr nodeOffset;
}

// init initializes the list head. off is the offset (via unsafe.Offsetof) of
// the listNode field in the objects in the list.
internal static void init(this ref listHead head, uintptr off) {
    head.initialized = true;
    head.nodeOffset = off;
}

// listNode is the linked list node for objects in a listHead list.
//
// listNode must be stored as a field in objects placed in the linked list. The
// offset of the field is registered via listHead.init.
//
// For example:
//
//	type foo struct {
//		val int
//
//		node listNode
//	}
//
// var fooHead listHead
// fooHead.init(unsafe.Offsetof(foo{}.node))
partial struct listNode {
    internal @unsafe.Pointer prev;
    internal @unsafe.Pointer next;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeUninitializedˢ = "runtime: uninitialized listHead"u8;

internal static ж<listNode> getNode(this ref listHead head, @unsafe.Pointer Δp) {
    if (!head.initialized) {
        @throw(runtimeUninitializedˢ);
    }

    if (Δp == nil) {
        return default!;
    }
    return (ж<listNode>)(uintptr)(@unsafe.Add(Δp, head.nodeOffset));
}

// Returns true if the list is empty.
internal static bool empty(this ref listHead head) {
    return head.obj == nil;
}

// Returns the head of the list without removing it.
internal static @unsafe.Pointer head(this ref listHead head) {
    return head.obj;
}

// Push p onto the front of the list.
internal static void push(this ref listHead head, @unsafe.Pointer Δp) {
    // p becomes the head of the list.

    // ... so p's next is the current head.
    var pNode = head.getNode(Δp);
    pNode.Value.next = head.obj;

    // ... and the current head's prev is p.
    if (head.obj != nil) {
        var headNode = head.getNode(head.obj);
        headNode.Value.prev = Δp;
    }

    head.obj = Δp;
}

// Pop removes the head of the list.
internal static @unsafe.Pointer pop(this ref listHead head) {
    if (head.obj == nil) {
        return default!;
    }

    // Return the head of the list.
    @unsafe.Pointer Δp = head.obj;

    // ... so the new head is p's next.
    var pNode = head.getNode(Δp);
    head.obj = pNode.Value.next;
    // p is no longer on the list. Clear next to remove unused references.
    // N.B. as the head, prev must already be nil.
    pNode.Value.next = default!;

    // ... and the new head no longer has a prev.
    if (head.obj != nil) {
        var headNode = head.getNode(head.obj);
        headNode.Value.prev = default!;
    }

    return Δp;
}

// Remove p from the middle of the list.
internal static void remove(this ref listHead head, @unsafe.Pointer Δp) {
    if (head.obj == Δp) {
        // Use pop to ensure head is updated when removing the head.
        head.pop();
        return;
    }

    var pNode = head.getNode(Δp);
    var prevNode = head.getNode((~pNode).prev);
    var nextNode = head.getNode((~pNode).next);

    // Link prev to next.
    if (prevNode != nil) {
        prevNode.Value.next = pNode.Value.next;
    }
    // Link next to prev.
    if (nextNode != nil) {
        nextNode.Value.prev = pNode.Value.prev;
    }

    pNode.Value.prev = default!;
    pNode.Value.next = default!;
}

} // end runtime_package
