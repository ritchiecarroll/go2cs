// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;
using static runtime_package;

partial class runtime_package {

// The types in this file are exact copies of the types in list.go, but with
// unsafe.Pointer replaced with uintptr for use where write barriers must be
// avoided, such as uses of muintptr, puintptr, guintptr.
//
// Objects in these lists must be kept alive via another real reference.

// listHeadManual points to the head of an intrusive doubly-linked list of
// objects.
//
// Prior to use, you must call init to store the offset of listNodeManual fields.
//
// Every object in the list should be the same type.
partial struct listHeadManual {
    internal uintptr obj;
    internal bool initialized;
    internal uintptr nodeOffset;
}

} partial class runtimeᴛlistHeadManual {

// init initializes the list head. off is the offset (via unsafe.Offsetof) of
// the listNodeManual field in the objects in the list.
internal static void init(this ref listHeadManual head, uintptr off) {
    head.initialized = true;
    head.nodeOffset = off;
}

} partial class runtime_package {

// listNodeManual is the linked list node for objects in a listHeadManual list.
//
// listNodeManual must be stored as a field in objects placed in the linked list.
// The offset of the field is registered via listHeadManual.init.
//
// For example:
//
//	type foo struct {
//		val int
//
//		node listNodeManual
//	}
//
// var fooHead listHeadManual
// fooHead.init(unsafe.Offsetof(foo{}.node))
partial struct listNodeManual {
    internal uintptr prev;
    internal uintptr next;
}

} partial class runtimeᴛlistHeadManual {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string runtimeUninitializedˢ => ref runtimeUninitializedˢᵇ;
private static readonly @string runtimeUninitializedˢᵇ = "runtime: uninitialized listHead"u8;

internal static ж<listNodeManual> getNode(this ref listHeadManual head, @unsafe.Pointer p) {
    if (!head.initialized) {
        @throw(runtimeUninitializedˢ);
    }

    if (p == nil) {
        return default;
    }
    return (ж<listNodeManual>)(uintptr)(@unsafe.Add(p, head.nodeOffset));
}

// Returns true if the list is empty.
internal static bool empty(this ref listHeadManual head) {
    return head.obj == 0;
}

// Returns the head of the list without removing it.
internal static @unsafe.Pointer head(this ref listHeadManual head) {
    return (@unsafe.Pointer)head.obj;
}

// Push p onto the front of the list.
internal static void push(this ref listHeadManual head, @unsafe.Pointer p) {
    // p becomes the head of the list.

    // ... so p's next is the current head.
    var pNode = head.getNode(p);
    pNode.next = head.obj;

    // ... and the current head's prev is p.
    if (head.obj != 0) {
        var headNode = head.getNode((@unsafe.Pointer)head.obj);
        headNode.prev = (uintptr)p;
    }

    head.obj = (uintptr)p;
}

// Pop removes the head of the list.
internal static @unsafe.Pointer pop(this ref listHeadManual head) {
    if (head.obj == 0) {
        return default;
    }

    // Return the head of the list.
    @unsafe.Pointer p = (@unsafe.Pointer)head.obj;

    // ... so the new head is p's next.
    var pNode = head.getNode(p);
    head.obj = pNode.next;
    // p is no longer on the list. Clear next to remove unused references.
    // N.B. as the head, prev must already be nil.
    pNode.next = 0;

    // ... and the new head no longer has a prev.
    if (head.obj != 0) {
        var headNode = head.getNode((@unsafe.Pointer)head.obj);
        headNode.prev = 0;
    }

    return p;
}

// Remove p from the middle of the list.
internal static void remove(this ref listHeadManual head, @unsafe.Pointer p) {
    if ((@unsafe.Pointer)head.obj == p) {
        // Use pop to ensure head is updated when removing the head.
        head.pop();
        return;
    }

    var pNode = head.getNode(p);
    var prevNode = head.getNode((@unsafe.Pointer)pNode.prev);
    var nextNode = head.getNode((@unsafe.Pointer)pNode.next);

    // Link prev to next.
    if (prevNode != nil) {
        prevNode.next = pNode.next;
    }
    // Link next to prev.
    if (nextNode != nil) {
        nextNode.prev = pNode.prev;
    }

    pNode.prev = 0;
    pNode.next = 0;
}

} // end runtimeᴛlistHeadManual
