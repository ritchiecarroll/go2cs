// GoCgoArgs.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace go.golib;

/// <summary>What a cgo wrapper hands C for one pointer to a C struct (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.1).</summary>
[EditorBrowsable(EditorBrowsableState.Never)] public enum GoCgoStructPointer
{
    /// <summary>A nil pointer: C receives 0.</summary>
    Nil,

    /// <summary>
    /// One whole Go struct in managed memory. The wrapper copies it into a stack mirror, hands C the mirror's address,
    /// and copies the mirror back after the call. cgo forbids C from keeping a Go pointer past the call, so a stack
    /// mirror is exactly the memory Go lets C see.
    /// </summary>
    Mirror,

    /// <summary>A struct in native memory: its own address is handed over, uncopied, and C may keep it.</summary>
    Native,

    /// <summary>
    /// A C scalar in Go memory (an element of a Go array, or a typed view over Go storage): handed over by its own
    /// address inside storage the wrapper's <c>fixed</c> holds still for the call (cgo Phase 3, increment 3.2, seat 1b,
    /// design 18.12), uncopied, so what C writes is what Go reads. Only a scalar pointee takes it: its layout is one
    /// value, the same in C and in the CLR.
    /// </summary>
    Pinned
}

/// <summary>
/// The one place a generated cgo wrapper asks what a pointer argument is (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §5.2,
/// read through §11.1). Every refusal names the C function, so a program that reaches one learns which call and why.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)] public static class GoCgoArgs
{
    /// <summary>
    /// How to hand C a pointer to a C struct. A whole struct in managed memory is mirrored: a standard box, or a field
    /// reference, since cgo makes a struct field's own bytes the memory C may touch. A genuinely native struct passes
    /// its address. Everything else is refused before the call: an ELEMENT of a Go array or slice (cgo makes the whole
    /// array the memory C may touch, more than one struct's mirror can hold; Phase 3's pin scope), a native box minted
    /// from managed storage (a reinterpret view, an order token or a pinned managed address, whose address C must not
    /// write through), a field of a struct in native memory, and any box kind this does not classify.
    /// </summary>
    public static GoCgoStructPointer StructPointer<T>(ж<T>? pointer, string function, out nuint address)
    {
        address = 0;

        if (pointer is null || pointer.IsNilPointer)
            return GoCgoStructPointer.Nil;

        switch (pointer)
        {
            case NativeBox<T> native:
                if (!IsGenuinelyNative(native))
                    throw Refused(function, typeof(T), "a native pointer minted from managed storage (a reinterpret view, an order token or a pinned managed address), which C must not write through");

                address = native.NativeAddress;
                return GoCgoStructPointer.Native;

            case StandardBox<T>:
                RequireNoPublishedWord(pointer);
                return GoCgoStructPointer.Mirror;

            case FieldRefBox<T> field:
                if (((INativeRooted)field).IsNativeRooted)
                    throw Refused(function, typeof(T), "a field of a struct in native memory, which Phase 2 does not yet pass");

                RequireNoPublishedWord(pointer);
                return GoCgoStructPointer.Mirror;

            case ElemRefBox<T>:
                throw Refused(function, typeof(T), "an element of a Go array or slice: cgo lets C touch the whole array, which one struct's mirror cannot hold (Phase 3)");

            default:
                throw Refused(function, typeof(T), $"a {pointer.GetType().Name}, a pointer kind Phase 2 does not classify");
        }
    }

    /// <summary>
    /// How to hand C a pointer to one C SCALAR (cgo Phase 3, increment 3.2, seat 1b: design 18.12 item 2). As
    /// <see cref="StructPointer{T}"/> for nil, C memory and one whole scalar in a box or a field (a stack mirror). An
    /// element of a Go array, and a typed view over Go storage, are <see cref="GoCgoStructPointer.Pinned"/>: the box
    /// names its storage, and <paramref name="pin"/> is the pin the wrapper holds with a <c>fixed</c> for the call. Storage
    /// that holds Go pointers, a token for it, and a published <c>unsafe.Pointer</c> word over it are refused with cgo's
    /// text, as the void door refuses them.
    /// </summary>
    public static GoCgoStructPointer ScalarPointer<T>(ж<T>? pointer, string function, out nuint address, out GoCgoPin pin)
    {
        pin = default;

        switch (pointer)
        {
            case ElemRefBox<T>:
            case NativeBox<T> native when !IsGenuinelyNative(native):
                nuint number = pointer is NativeBox<T> view ? view.NativeAddress : 0;

                if (ManagedPointerTokens.IsTaggedToken(number))
                    throw GoPointerToUnpinnedGoPointer();

                // Storage the box cannot name keeps Phase 2's refusal, word for word: a native box minted from managed
                // storage, or an element of a Go array.
                if (!TryPinTarget(pointer, number, out Array? root, out nint offset))
                {
                    throw pointer is NativeBox<T>
                        ? Refused(function, typeof(T), "a native pointer minted from managed storage (a reinterpret view, an order token or a pinned managed address), which C must not write through")
                        : Refused(function, typeof(T), "an element of a Go array or slice whose storage go2cs cannot hold still for the call");
                }

                RequireCrossable(root);
                pin = new GoCgoPin(root, offset);
                address = 0;
                return GoCgoStructPointer.Pinned;

            default:
                return StructPointer(pointer, function, out address);
        }
    }

    /// <summary>
    /// The out-parameter, ruled shape (i) (cgo Phase 3, increment 3.2, seat 1b: design 4 and 18.12 item 4): a pointer
    /// to one C pointer (<c>&amp;db</c>, <c>char **</c>). A slot in Go memory crosses as one stack word, as a pointer
    /// MEMBER of a mirror does (<see cref="GoCgoStructPointer.Mirror"/>): <paramref name="word"/> is copied in by
    /// <see cref="PointerMember{U}"/> (nil 0, a C pointer its address, a Go pointer cgo's panic), and the wrapper copies
    /// the word back after the call by <see cref="PointerMemberBack{U}"/> against <paramref name="original"/>. A slot in C
    /// memory (<c>&amp;l.next</c> over a C struct) is handed over by its own address, uncopied
    /// (<see cref="GoCgoStructPointer.Native"/>). A nil slot is C's NULL.
    /// </summary>
    public static GoCgoStructPointer OutParameter<U>(ж<ж<U>>? slot, string function, out nuint word, out ж<U>? original)
    {
        word = 0;
        original = null;

        if (slot is null || slot.IsNilPointer)
            return GoCgoStructPointer.Nil;

        if (slot is NativeBox<ж<U>> native)
        {
            if (!IsGenuinelyNative(native))
                throw Refused(function, typeof(ж<U>), "a native pointer minted from managed storage (a reinterpret view, an order token or a pinned managed address), which C must not write through");

            word = native.NativeAddress;
            return GoCgoStructPointer.Native;
        }

        RequireManagedSlot(slot);
        original = slot.ValueSlot;
        word = PointerMember(original);
        return GoCgoStructPointer.Mirror;
    }

    /// <summary>
    /// A pointer member's value as it is copied into a mirror: nil is 0 and a genuinely native pointer is its address.
    /// A pointer into Go memory panics with cgo's own message: Go's cgocheck makes the same refusal at run time, and
    /// here it is always on. (Go prefixes the message with the caller's file and line, which a wrapper cannot know.)
    /// </summary>
    public static nuint PointerMember<U>(ж<U>? member)
    {
        if (member is null || member.IsNilPointer)
            return 0;

        if (member is NativeBox<U> native && IsGenuinelyNative(native))
            return native.NativeAddress;

        throw RuntimeErrorPanic.ErrorString("argument of cgo function has Go pointer to unpinned Go pointer");
    }

    /// <summary>
    /// A pointer member's value as it is copied back out of a mirror: 0 is nil, the address the member was copied in as
    /// is the ORIGINAL pointer (so a member C left alone keeps its identity), and any other address is C memory.
    /// </summary>
    public static ж<U>? PointerMemberBack<U>(nuint address, ж<U>? original, nuint copiedIn)
    {
        if (address == 0)
            return null;

        if (address == copiedIn)
            return original;

        return new NativeBox<U>(address);
    }

    // ---- cgo Phase 3, increment 3.1 (docs/phase4/DESIGN-cgo-phase3-marshaling.md §16): a void pointer, and cmd/cgo's
    // own helpers. Increment 3.1 hands C no Go memory. Every pointer into Go memory that reaches one of these doors is
    // refused by name, before the call, and each refusal names the increment that admits it.

    /// <summary>
    /// What C receives for a <c>void *</c> argument: 0 for nil, and the number itself for C memory. A pointer into Go
    /// memory is refused before the call, in each form it can arrive in (§13.4, M3): an <c>unsafe.Pointer</c> that
    /// carries its box, a token that names no memory, and a number the pin and token registry knows as a live Go
    /// box's. A number the registry does not know crosses as it is, which is what C memory is.
    /// </summary>
    /// <remarks>
    /// The registry is the test because the mint is the record: every conversion of a Go pointer to a number registers
    /// it (the box-to-uintptr operator's three arms), and an entry resolves only while its box is alive and still names
    /// that number. A number whose box has died is not Go memory any more, and one whose pin was released is the residue
    /// §13.4 names, in code that is already invalid Go.
    /// </remarks>
    public static nuint VoidPointer(ж<uintptr>? pointer, string function)
    {
        string? what = GoMemory(pointer, out nuint address);

        if (what is not null)
            throw RuntimeErrorPanic.ErrorString($"cgo: the void pointer handed to C.{function} is {what}, and handing C a pointer into Go memory is not supported yet, so go2cs refuses the call (docs/Limitations.md#cgo)");

        return address;
    }

    // ---- cgo Phase 3, increment 3.2, seat 1b (docs/phase4/DESIGN-cgo-phase3-marshaling.md 18 and 18.12): the PINNED
    // class. Go memory crosses when the BOX it was handed in names its storage (M2), never on a number (M3). The door
    // answers the storage and the byte offset, and the wrapper pins that storage with a call-scoped `fixed` around the
    // call: no GCHandle, no finalizer, no index entry (18's pin path).

    /// <summary>
    /// What C receives for a <c>void *</c> argument, as a pin the wrapper holds for the call (design 18.12): nil and C
    /// memory pin nothing and cross as their number; Go memory whose box names its storage crosses as an address inside
    /// that storage, held still by the wrapper's <c>fixed</c>. Refused before the call: a Go value that holds Go
    /// pointers, and a token for one (cgo's own panic); storage that holds a published <c>unsafe.Pointer</c> word
    /// (cgocheck's text, B-10); and every NUMBER of Go memory, as <see cref="VoidPointer"/> refuses it.
    /// </summary>
    public static GoCgoPin PinVoidPointer(ж<uintptr>? pointer, string function)
    {
        if (pointer is null || pointer.IsNilPointer)
            return default;

        nuint number = pointer.Value.Value;

        if (pointer is IUnsafePointer { RetainedSource: { } carried } && carried is not INativeRooted { IsGenuinelyNativeRoot: true })
            return PinCarried(carried, number, function);

        // A token names a Go value that holds Go references (the pointer-to-unpinned-pointer class, cgo's own panic).
        if (ManagedPointerTokens.IsTaggedToken(number))
            throw GoPointerToUnpinnedGoPointer();

        string? what = GoMemory(pointer, out nuint address);

        if (what is not null)
            throw VoidRefused(function, what);

        return new GoCgoPin(address);
    }

    // The pin for a pointer that carries its box. The box decides (M2): its storage, found through the derivations it
    // was minted by, and the offset inside it.
    private static unsafe GoCgoPin PinCarried(object carried, nuint number, string function)
    {
        // A field or view over a struct in C memory, reached through a Go pointer: as 3.1 refused it.
        if (carried is INativeRooted { IsNativeRooted: true } && carried is IGoReflectBox { NativeAddress: 0 })
            throw VoidRefused(function, "a pointer reached through a Go pointer into C memory (a field, or a view over managed storage)");

        // An order token, carried or aliased: a Go value that holds Go references, which has no address to hand over.
        if (ManagedPointerTokens.IsTaggedToken(number))
            throw GoPointerToUnpinnedGoPointer();

        if (!TryPinTarget(carried, number, out Array? root, out nint offset))
            throw VoidRefused(function, "a pointer into Go memory whose storage go2cs cannot hold still for the call");

        RequireCrossable(root);
        return new GoCgoPin(root, offset);
    }

    // The storage a carried box's address is in, and the byte offset inside it.
    //   1. A box that already holds its storage still (an address-take's pin, a derivation's source, a reinterpret
    //      view's root): the pinned array, and the number's distance from its first byte. The pin holds the array where
    //      the number says it is, so the distance is the one the wrapper's `fixed` will see.
    //   2. A native box over a pinned managed buffer: the buffer's array, the same way.
    //   3. A managed box that holds nothing still (a FromBox mint): its own pinnable storage, and its slot's distance
    //      from the storage's first byte, read under a `fixed` here, since its number was read under one that ended.
    private static unsafe bool TryPinTarget(object carried, nuint number, [NotNullWhen(true)] out Array? root, out nint offset)
    {
        root = null;
        offset = 0;

        if (NativeDerivation.HeldSource(carried) is { } held && held.TryHeldTarget(out object target, out nuint start, out nuint end) &&
            target is Array heldArray && number >= start && number < end)
        {
            root = heldArray;
            offset = (nint)(number - start);
            return true;
        }

        if (carried is IGoReflectBox { NativeAddress: not 0 } && carried is IManagedPinHolder { ManagedPin: { } pin } &&
            pin.PinnedTarget is Array pinnedArray && pinnedArray.Length > 0)
        {
            nuint first = (nuint)pin.Pointer;
            nuint last = (nuint)(nint)Marshal.UnsafeAddrOfPinnedArrayElement(pinnedArray, pinnedArray.Length);

            if (number >= first && number < last)
            {
                root = pinnedArray;
                offset = (nint)(number - first);
                return true;
            }

            return false;
        }

        if (carried is IGoReflectBox { NativeAddress: 0, IsNilPointer: false, NamesZeroBase: false } box &&
            carried is INilPointer { PinnableStorage: Array storage } && storage.Length > 0)
        {
            fixed (byte* data = &MemoryMarshal.GetArrayDataReference(storage))
            {
                nuint slot = box.TransientSlotAddress();
                nuint last = (nuint)(nint)Marshal.UnsafeAddrOfPinnedArrayElement(storage, storage.Length);

                if (slot < (nuint)data || slot >= last)
                    return false;

                root = storage;
                offset = (nint)(slot - (nuint)data);
                return true;
            }
        }

        return false;
    }

    // What makes pinned Go storage unfit to hand C: Go pointers in it. A value that holds references (the element type
    // of its storage), and a published unsafe.Pointer word (B-10: GoPointerWords holds an entry over it), are both
    // cgocheck's "Go pointer to unpinned Go pointer". The whole storage is asked, since cgo lets C touch the whole object.
    private static void RequireCrossable(Array root)
    {
        if (root.GetType().GetElementType() is not { } element || NativeStructMarshal.ContainsReferences(element) || GoPointerWords.HoldsAnyEntry(root))
            throw GoPointerToUnpinnedGoPointer();
    }

    private static PanicException GoPointerToUnpinnedGoPointer() =>
        RuntimeErrorPanic.ErrorString("argument of cgo function has Go pointer to unpinned Go pointer");

    private static PanicException VoidRefused(string function, string what) =>
        RuntimeErrorPanic.ErrorString($"cgo: the void pointer handed to C.{function} is {what}, and handing C a pointer into Go memory is not supported yet, so go2cs refuses the call (docs/Limitations.md#cgo)");

    /// <summary>
    /// The address one of cmd/cgo's helpers reads, for a helper that takes a <c>void *</c> (<c>C.GoBytes</c>): 0 for
    /// nil and the number itself for C memory. Go memory is refused by name: a Go function may read it, but reading it
    /// here needs the source classification of increment 3.2 (§16.7, Q2).
    /// </summary>
    public static nuint HelperVoidPointer(ж<uintptr>? pointer, string helper)
    {
        string? what = GoMemory(pointer, out nuint address);

        if (what is not null)
            throw HelperRefused(helper, what);

        return address;
    }

    /// <summary>
    /// The address one of cmd/cgo's helpers reads, for a helper that takes a typed C pointer (<c>C.GoString</c>,
    /// <c>C.GoStringN</c>): 0 for nil and a genuinely native pointer's address. Any other pointer is Go memory, refused
    /// by name as <see cref="HelperVoidPointer"/> refuses it.
    /// </summary>
    public static nuint HelperPointer<T>(ж<T>? pointer, string helper)
    {
        if (pointer is null || pointer.IsNilPointer)
            return 0;

        if (pointer is NativeBox<T> native && IsGenuinelyNative(native))
            return native.NativeAddress;

        throw HelperRefused(helper, "a pointer into Go memory");
    }

    // ---- cmd/cgo's helpers over Go memory (seat 1b, design 18.12 item 3 with P3's confirm (A)): the helpers are Go
    // functions, so they read Go memory as Go does, byte by byte, with no word-table lookup. The storage the box names is
    // held still by a `fixed` here for the copy. A number of Go memory stays refused, as everywhere (M3).

    /// <summary><c>C.GoString</c> over a typed C pointer: C memory as before, and Go memory read from its pinned storage.</summary>
    public static unsafe @string GoStringOf<T>(ж<T>? pointer, string helper)
    {
        if (!TryHelperPin(pointer, helper, out Array? root, out nint offset, out nuint address))
            return GoString(address);

        fixed (byte* data = &MemoryMarshal.GetArrayDataReference(root))
        {
            ReadOnlySpan<byte> rest = new(data + offset, checked((int)(HelperExtent(root, data) - offset)));
            int length = rest.IndexOf((byte)0);

            if (length < 0)
                throw HelperReadRefused(helper, "Go memory with no NUL before the end of its storage, which Go would read past");

            return new @string(rest[..length]);
        }
    }

    /// <summary><c>C.GoStringN</c> over a typed C pointer: C memory as before, and Go memory read from its pinned storage.</summary>
    public static unsafe @string GoStringNOf<T>(ж<T>? pointer, int length, string helper)
    {
        if (length <= 0)
            return GoStringN(0, length);

        if (!TryHelperPin(pointer, helper, out Array? root, out nint offset, out nuint address))
            return GoStringN(address, length);

        fixed (byte* data = &MemoryMarshal.GetArrayDataReference(root))
            return new @string(HelperBytes(root, data, offset, length, helper));
    }

    /// <summary><c>C.GoBytes</c> over a void pointer: C memory as before, and Go memory read from its pinned storage.</summary>
    public static unsafe slice<byte> GoBytesOf(ж<uintptr>? pointer, int length, string helper)
    {
        if (length <= 0)
            return GoBytes(0, length);

        if (!TryHelperPin(pointer, helper, out Array? root, out nint offset, out nuint address))
            return GoBytes(address, length);

        fixed (byte* data = &MemoryMarshal.GetArrayDataReference(root))
            return new slice<byte>(HelperBytes(root, data, offset, length, helper));
    }

    // Whether a helper's pointer is Go memory whose box names its storage (then the root and the offset), or nil and C
    // memory (then false and its address). Refused: a number of Go memory, a token, and storage that holds references,
    // whose bytes are not the value Go would read.
    private static bool TryHelperPin<T>(ж<T>? pointer, string helper, [NotNullWhen(true)] out Array? root, out nint offset, out nuint address)
    {
        root = null;
        offset = 0;
        address = 0;

        if (pointer is null || pointer.IsNilPointer)
            return false;

        object? carried;
        nuint number;

        if (pointer is IUnsafePointer unsafePointer)
        {
            carried = unsafePointer.RetainedSource;
            number = ((ж<uintptr>)(object)pointer).Value.Value;
        }
        else
        {
            carried = pointer;
            number = pointer is NativeBox<T> native ? native.NativeAddress : 0;
        }

        if (carried is null || carried is INativeRooted { IsGenuinelyNativeRoot: true })
        {
            address = pointer is IUnsafePointer ? HelperVoidPointer((ж<uintptr>)(object)pointer, helper) : HelperPointer(pointer, helper);
            return false;
        }

        if (ManagedPointerTokens.IsTaggedToken(number))
            throw HelperRefused(helper, "a token for a Go value that has no address (it holds Go references)");

        if (!TryPinTarget(carried, number, out root, out offset))
            throw HelperReadRefused(helper, "a pointer into Go memory whose storage go2cs cannot hold still for the read");

        if (root.GetType().GetElementType() is not { } element || NativeStructMarshal.ContainsReferences(element))
            throw HelperReadRefused(helper, "Go memory that holds Go references, whose bytes are not the value Go would read");

        return true;
    }

    // The end of a pinned root's bytes, as an offset from its first byte.
    private static unsafe nint HelperExtent(Array root, byte* data) =>
        (nint)Marshal.UnsafeAddrOfPinnedArrayElement(root, root.Length) - (nint)data;

    // `length` bytes at `offset` in a pinned root. Go would read past the end of the object; the managed object is all the
    // storage there is, so a read past its end is refused by name.
    private static unsafe ReadOnlySpan<byte> HelperBytes(Array root, byte* data, nint offset, int length, string helper)
    {
        if (offset + length > HelperExtent(root, data))
            throw HelperReadRefused(helper, $"a read of {length} bytes past the end of the Go storage it points into");

        return new ReadOnlySpan<byte>(data + offset, length);
    }

    /// <summary>
    /// Go's <c>*p</c> for <c>p</c> a <c>**C.T</c>, the load go2cs emits on the cgo route (cgo Phase 3, increment 3.2,
    /// seat 1c: docs/phase4/DESIGN-cgo-phase3-marshaling.md 18.11).
    /// </summary>
    /// <remarks>
    /// A native box (<c>&amp;l.next</c> over a C struct in C memory, at the field's Go offset) names a WORD, which the
    /// reference floor refuses to read as a managed reference: the word is read as 1a's field read reads it
    /// (CgoPointerWords). A field view whose source is native had no known Go offset and is refused by name. Any other
    /// box is a managed slot, read as the converter reads one today: a nil box is Go's nil dereference, and the slot
    /// itself is <c>ValueSlot</c>, so a slot that holds a nil pointer loads nil (<c>.Value</c> would read that as a nil
    /// box and panic).
    /// </remarks>
    public static ж<U> LoadPointer<U>(ж<ж<U>> p)
    {
        if (p is NativeBox<ж<U>> native)
            return CgoPointerWords.ReadWord(native);

        RequireManagedSlot(p);
        return p.ValueSlot;
    }

    /// <summary>
    /// Go's <c>*p = v</c> for <c>p</c> a <c>**C.T</c>, the store beside <see cref="LoadPointer{U}"/>: a word of C memory
    /// takes 0 for nil and the address of C memory alone, and a Go pointer is refused by name.
    /// </summary>
    public static void StorePointer<U>(ж<ж<U>> p, ж<U> value)
    {
        if (p is NativeBox<ж<U>> native)
        {
            CgoPointerWords.WriteWord(native, value);
            return;
        }

        RequireManagedSlot(p);
        p.ValueSlot = value;
    }

    // A nil p (IsNilPointer: the box itself, never a box that holds nil) is Go's nil dereference. A field view over a native source (no known Go offset; ж.cs's nativeFieldView kept
    // the view) is refused before any read or write: its CLR slot is not where the field's bytes are (18.11 [P3 M1]).
    private static void RequireManagedSlot<U>(ж<ж<U>>? p)
    {
        if (p is null || p.IsNilPointer)
            throw RuntimeErrorPanic.NilPointerDereference();

        if (p is INativeRooted { IsNativeRooted: true })
        {
            throw p is FieldRefBox<ж<U>> view
                ? view.NoKnownGoOffset()
                : RuntimeErrorPanic.NativeStructLayoutMismatch(typeof(ж<U>), "a view of native memory with no known Go offset");
        }
    }

    /// <summary>
    /// The number of bytes <c>C.CString</c> allocates: the string's bytes and a NUL. It runs before the allocation, so a
    /// string whose bytes hold a published <c>unsafe.Pointer</c> word is refused here, with nothing allocated (B-10).
    /// </summary>
    public static nuint StringSize(@string s)
    {
        (byte[]? array, int offset) = s.Backing;

        if (array is not null && GoPointerWords.HoldsAnyEntry(array, (nuint)offset, (nuint)(offset + s.Length)))
            throw RuntimeErrorPanic.ErrorString(CopyIntoCText("CString"));

        return (nuint)s.Length + 1;
    }

    /// <summary>
    /// The number of bytes <c>C.CBytes</c> allocates: the slice's length. It runs before the allocation, so a slice whose
    /// bytes hold a published <c>unsafe.Pointer</c> word is refused here, with nothing allocated (B-10).
    /// </summary>
    public static nuint BytesSize(slice<byte> b)
    {
        nint length = b.ToSpan().Length;

        if (b.m_array is { } array && GoPointerWords.HoldsAnyEntry(array, (nuint)b.Low, (nuint)(b.Low + length)))
            throw RuntimeErrorPanic.ErrorString(CopyIntoCText("CBytes"));

        return (nuint)length;
    }

    /// <summary>
    /// B-10 for an inline array member a struct's mirror copies (design 18.7 finding 2): go2cs keeps an array's elements
    /// in the array's own storage, not in the struct's, so the copy-in asks that storage before it copies the elements
    /// RAW into the mirror. A published <c>unsafe.Pointer</c> word among them is cgo's panic.
    /// </summary>
    public static void RequireNoPublishedWordIn<E>(in array<E> elements)
    {
        if (GoPointerWords.AnyTable && elements.m_array is { } storage && GoPointerWords.HoldsAnyEntry(storage))
            throw GoPointerToUnpinnedGoPointer();
    }

    // C.CString and C.CBytes copy Go bytes into C memory, where no word-table entry follows: a published Go pointer among
    // them would be a Go pointer stored into C memory, which W2(a) refuses (design 18.7 finding 2).
    private static string CopyIntoCText(string helper) =>
        $"cgo: C.{helper} copies Go memory that holds a Go pointer: storing a Go pointer into C memory is refused (C memory must not hold a Go pointer)";

    // B-10 for a struct the wrapper mirrors (design 18.7 finding 2): a by-value array or uintptr field is copied RAW into
    // the mirror, so a published unsafe.Pointer word over the struct's bytes would reach C. The struct's own range of
    // its storage is asked; storage that cannot hold words (it holds references, or nothing pins it) has no entry.
    private static void RequireNoPublishedWord<T>(ж<T> pointer)
    {
        if (GoPointerWords.AnyTable && TryPinTarget(pointer, 0, out Array? root, out nint offset) &&
            GoPointerWords.HoldsAnyEntry(root, (nuint)offset, (nuint)(offset + Unsafe.SizeOf<T>())))
        {
            throw GoPointerToUnpinnedGoPointer();
        }
    }

    /// <summary>
    /// The address the package's own <c>malloc</c> returned, or the end of the program for a nil one, with the text
    /// cmd/cgo's own stub ends it with (<c>runtime: C malloc failed</c>).
    /// </summary>
    public static nuint Allocated(nint address)
    {
        if (address == 0)
            FatalReport.Fatal("runtime: C malloc failed", userFault: false);

        return (nuint)address;
    }

    /// <summary>Copies a Go string's BYTES and a NUL to C memory. No text encoding is on the path (§13.7, S5).</summary>
    public static unsafe void CopyString(@string s, nuint address)
    {
        ReadOnlySpan<byte> bytes = s.ToSpan();
        bytes.CopyTo(new Span<byte>((void*)address, bytes.Length));
        ((byte*)address)[bytes.Length] = 0;
    }

    /// <summary>Copies a Go byte slice to C memory.</summary>
    public static unsafe void CopyBytes(slice<byte> b, nuint address)
    {
        ReadOnlySpan<byte> bytes = b.ToSpan();
        bytes.CopyTo(new Span<byte>((void*)address, bytes.Length));
    }

    /// <summary>
    /// <c>C.GoString</c>: the bytes at a C string's address, up to its NUL, as a Go string. A nil pointer is the empty
    /// string, as Go's <c>runtime.gostring</c> answers it.
    /// </summary>
    public static unsafe @string GoString(nuint address)
    {
        if (address == 0)
            return new @string();

        byte* text = (byte*)address;
        nint length = 0;

        while (text[length] != 0)
            length++;

        return new @string(new ReadOnlySpan<byte>(text, checked((int)length)));
    }

    /// <summary>
    /// <c>C.GoStringN</c>: <paramref name="length"/> bytes at an address as a Go string. A length of zero is the empty
    /// string and reads nothing. A negative length ends the program as Go's <c>runtime.gostringn</c> does: it asks the
    /// allocator for that many bytes, and the runtime throws <c>out of memory</c>.
    /// </summary>
    public static unsafe @string GoStringN(nuint address, int length)
    {
        if (length == 0)
            return new @string();

        if (length < 0)
            FatalReport.Fatal("out of memory", userFault: false);

        if (address == 0)
            throw RuntimeErrorPanic.ErrorString("invalid memory address or nil pointer dereference");

        return new @string(new ReadOnlySpan<byte>((void*)address, length));
    }

    /// <summary>
    /// <c>C.GoBytes</c>: <paramref name="length"/> bytes at an address as a new Go byte slice. A length of zero is an
    /// empty slice that is not nil, and a negative one panics, both as Go's <c>runtime.gobytes</c> does.
    /// </summary>
    public static unsafe slice<byte> GoBytes(nuint address, int length)
    {
        if (length < 0)
            throw RuntimeErrorPanic.ErrorString("gobytes: length out of range");

        if (length == 0)
            return new slice<byte>(Array.Empty<byte>());

        if (address == 0)
            throw RuntimeErrorPanic.ErrorString("invalid memory address or nil pointer dereference");

        return new slice<byte>(new ReadOnlySpan<byte>((void*)address, length));
    }

    // What a void pointer carries when it is Go memory, or null with its number when it is nil or C memory.
    private static string? GoMemory(ж<uintptr>? pointer, out nuint address)
    {
        address = 0;

        if (pointer is null || pointer.IsNilPointer)
            return null;

        nuint number = pointer.Value.Value;

        // A pointer that carries its box, as the converter's mints of unsafe.Pointer(p) and unsafe.Pointer(&x) do: the
        // box says what the memory is. Only a native box over C memory alone goes on to the number's own test.
        if (pointer is IUnsafePointer { RetainedSource: { } carried } && carried is not INativeRooted { IsGenuinelyNativeRoot: true })
        {
            return carried is INativeRooted { IsNativeRooted: true }
                ? "a pointer reached through a Go pointer into C memory (a field, or a view over managed storage)"
                : "a pointer into Go memory";
        }

        // By the number's own bits (bit 63 set, bit 47 clear), which no user address has. A C sentinel such as
        // (void *)-1 has both set, and crosses.
        if (ManagedPointerTokens.IsTaggedToken(number))
            return "a token for a Go value that has no address (it holds Go references)";

        // A registered element of C memory alone is not Go memory: an element box registers its address when it is
        // converted to a number, whatever memory it names (C2). Only a box that keeps no root qualifies (G1).
        if (ManagedPointerTokens.Resolve(number) is { } registered && registered is not INativeRooted { IsGenuinelyNativeRoot: true })
            return "the address of Go memory (a Go variable, field or element)";

        // Inside Go memory that golib holds pinned: an interior number, or one whose pin the exact registry never saw
        // (docs/phase4/DESIGN-cgo-pin-extents.md). Under 3.1 a hit only refuses, so the buffer found need not be held.
        if (PinExtents.Find(number) is not null)
            return "an address inside Go memory golib holds pinned (a number built from a Go pointer)";

        address = number;
        return null;
    }

    // Whether the registry knows a number as a live Go box's (a token, or the address a Go pointer was converted to), or a
    // live pin's extent holds it (runtime/PinExtents.cs).
    private static bool IsKnownGoAddress(nuint address) =>
        ManagedPointerTokens.IsTaggedToken(address) || ManagedPointerTokens.Resolve(address) is not null || PinExtents.Find(address) is not null;

    // A helper's read of Go memory that go2cs refuses for what the memory is, not because the read is unsupported.
    private static PanicException HelperReadRefused(string helper, string what) =>
        RuntimeErrorPanic.ErrorString($"cgo: C.{helper} over Go memory: the pointer it was handed is {what}, so go2cs refuses the read (docs/Limitations.md#cgo)");

    private static PanicException HelperRefused(string helper, string what) =>
        RuntimeErrorPanic.ErrorString($"cgo: C.{helper} over Go memory: the pointer it was handed is {what}. Reading Go memory through cmd/cgo's helpers is not supported yet, so go2cs refuses the call (docs/Limitations.md#cgo)");

    // A native box says what it was minted WITH, and a native box minted from a bare NUMBER was minted with nothing:
    // (*T)(unsafe.Pointer(&array)) over a Go array of another element type goes through the number, and comes back as a
    // native box with no source, no token and no pin, over pinned Go storage. The number is still in the registry, so
    // the registry is asked too (Phase 3 increment 3.1; first read on the hosted end-to-end leg 43330710728, where
    // C.GoString read a Go array through such a box).
    private static bool IsGenuinelyNative<T>(NativeBox<T> native) =>
        native.RetainedSource is null && !native.AliasesAnOrderToken && !native.PinsManagedStorage && !IsKnownGoAddress(native.NativeAddress);

    private static PanicException Refused(string function, Type pointee, string what) =>
        RuntimeErrorPanic.ErrorString($"cgo: the {pointee.Name} pointer handed to C.{function} is {what}, so go2cs refuses the call");
}
