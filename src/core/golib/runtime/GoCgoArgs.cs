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
    /// <see cref="StructPointer{T}(ж{T}, string, out nuint)"/> for a wrapper that holds a call's
    /// <see cref="GoCgoImages"/> (cgo Phase 3, increment 3.4, the struct image door: DESIGN-cgo-phase3-marshaling 19.7 and
    /// 19.9). A struct pointer that is a reinterpret view of a Go struct of another type, which no door can pin, crosses
    /// as an IMAGE of its root: <see cref="GoCgoStructPointer.Pinned"/>, with <paramref name="pin"/> over the image
    /// buffer, which the wrapper's <c>fixed</c> holds and <see cref="GoCgoImages.CopyBack"/> decodes back after the call
    /// (net's <c>getnameinfo</c> over <c>(*C.struct_sockaddr)(unsafe.Pointer(&amp;sa))</c>). Every other pointer takes the
    /// plain door, and one it hands C as a mirror is claimed in the set, so a root imaged and mirrored in one call is
    /// refused by name (P3's B5).
    /// </summary>
    public static GoCgoStructPointer StructPointer<T>(ж<T>? pointer, string function, ref GoCgoImages images, out nuint address, out GoCgoPin pin)
    {
        pin = default;

        if (pointer is not null && GoPinTable.Any && TryPinnerHeld(pointer, function, out pin))
        {
            address = 0;
            images.ClaimOther(OtherKeys(pointer, pin), function);
            return GoCgoStructPointer.Pinned;
        }

        if (pointer is NativeBox<T> native && !IsGenuinelyNative(native) && native.AliasesAnOrderToken && ImageRoot(native, function) is { } root)
        {
            // An image is a copy: C keeping its address after the call would keep the copy's, not the Go value's (S6).
            if (GoPinTable.Any && GoPinTable.IsPinned(root.Key))
                throw PinnerHeldRefused(function, typeof(T), "a view of a Go value no door can hand over in place");

            address = 0;
            pin = images.Image(GoCgoImages.ImageKeys(root.Key, root.Value), root.Box, root.Value, root.Type, root.Size, Math.Max(root.Size, NativeStructMarshal.ImageSizeOf(typeof(T)) ?? (nuint)Unsafe.SizeOf<T>()), function);
            return GoCgoStructPointer.Pinned;
        }

        GoCgoStructPointer kind = StructPointer(pointer, function, out address);

        if (kind == GoCgoStructPointer.Mirror && pointer is not null)
            images.ClaimOther(OtherKeys(pointer, pin), function);

        return kind;
    }

    /// <summary>
    /// <see cref="ScalarPointer{T}(ж{T}, string, out nuint, out GoCgoPin)"/> for a wrapper that holds a call's
    /// <see cref="GoCgoImages"/>: the scalar's root is claimed in the set, so a root one parameter images and this one
    /// mirrors or pins is refused by name (P3's B5).
    /// </summary>
    public static GoCgoStructPointer ScalarPointer<T>(ж<T>? pointer, string function, ref GoCgoImages images, out nuint address, out GoCgoPin pin)
    {
        GoCgoStructPointer kind = ScalarPointer(pointer, function, out address, out pin);

        if (kind is GoCgoStructPointer.Mirror or GoCgoStructPointer.Pinned && pointer is not null)
            images.ClaimOther(OtherKeys(pointer, pin), function);

        return kind;
    }

    // The allocations a pointer another door hands C names (P3's B5, keyed as an image is): its root, and the storage it
    // pins, so a pointer into an array inside an imaged root (`&sa.Addr[0]`, or `(*C.int)(unsafe.Pointer(&sa.Addr))`,
    // whose view names neither sa's box nor its view) is seen to name that root's memory.
    private static object[] OtherKeys<T>(ж<T> pointer, in GoCgoPin pin)
    {
        object root = pointer.ReferentObject;

        return pin.Storage is { } storage && !ReferenceEquals(storage, root) ? [root, storage] : [root];
    }

    // The root a struct view's order token names, when the door can image it: the token resolves to a live Go box (an
    // offset added to it is refused by name, P3's B4), whose value is a struct NativeStructMarshal carries both ways (a
    // root holding a Go pointer is refused by name), with no published unsafe.Pointer word over it (B-10). Null when the
    // token resolves to nothing, so the plain door's refusal stands.
    private static (object Key, IGoReflectBox Box, object Value, Type Type, nuint Size)? ImageRoot<T>(NativeBox<T> view, string function)
    {
        nuint number = view.NativeAddress;
        nuint offset = 0;

        object? root = ManagedPointerTokens.IsTokenArithmetic(number)
            ? ManagedPointerTokens.ResolveArithmeticBase(number, out offset)
            : ManagedPointerTokens.Resolve(number);

        if (root is not IGoReflectBox box || box.IsNilPointer)
            return null;

        if (offset != 0)
            throw Refused(function, typeof(T), $"a view {offset} bytes into a Go value no door can pin; go2cs images only a whole Go value, so it refuses rather than hand C the wrong bytes");

        if (box.ReadBoxSlot() is not { } value || NativeStructMarshal.ImageSizeOf(value.GetType()) is not { } size)
            throw Refused(function, typeof(T), "a view of a Go value that holds Go pointers (or has no image in memory), which C must not see");

        if (GoPointerWords.AnyTable && GoPointerWords.HoldsAnyEntry(root))
            throw GoPointerToUnpinnedGoPointer();

        return (root is INilPointer nilable ? nilable.ReferentObject : root, box, value, value.GetType(), size);
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
                // The element's own number: an element of a native window over a view (`&h.name[0]`) is named by its
                // identity, and a managed element by 0 (P3's review of 1b, item 2).
                nuint number = RootNumber(pointer);

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

            // A REINTERPRET view, `(*C.char)(unsafe.Pointer(&b[4]))`: the address is its root's, and C may write past one
            // T into the root's storage (a char * C fills). A one-T stack mirror would take those bytes into the stack
            // and copy one back (3.2's first hosted run: goindexfill read "g" and six zeros). The root's storage is
            // pinned instead, when its box names it; a root no door can name keeps the mirror, as before.
            case IReinterpretView { ReinterpretRoot: { } viewRoot } when TryPinReinterpretRoot(viewRoot, out pin):
                address = 0;
                return GoCgoStructPointer.Pinned;

            // A root no door can pin, WIDER than one T (`(*C.char)(unsafe.Pointer(&s.n))`, s holding a Go reference): a
            // one-T mirror would take C's bytes past the first into the stack, silently (P3's review of 1b, item 3). Refused
            // by name. A root of one T's size keeps the mirror: there is nothing past it for C to reach.
            case IReinterpretView { ReinterpretRoot: IReinterpretSource { PointeeSize: var rootSize } and not INativeRooted { IsGenuinelyNativeRoot: true } }
                when rootSize > Unsafe.SizeOf<T>():
                throw Refused(function, typeof(T), $"a reinterpret view of a {rootSize}-byte Go value whose storage go2cs cannot hold still, and C may write past one {typeof(T).Name} into it");

            default:
                // A nil pointer crosses as nil (StructPointer below): the pin table is global, so it reaches here whenever any
                // Pinner holds a pin anywhere, and TryPinnerHeld reads the pointer.
                if (pointer is not null && GoPinTable.Any && TryPinnerHeld(pointer, function, out pin))
                {
                    address = 0;
                    return GoCgoStructPointer.Pinned;
                }

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

        RefusePinnerHeldCopy(slot, typeof(ж<U>), function);

        if (slot is NativeBox<ж<U>> native)
        {
            if (!IsGenuinelyNative(native))
                throw Refused(function, typeof(ж<U>), "a native pointer minted from managed storage (a reinterpret view, an order token or a pinned managed address), which C must not write through");

            word = native.NativeAddress;
            return GoCgoStructPointer.Native;
        }

        // An element of a Go array of C pointers (`C.count(&argv[0])`): the mirror is ONE stack word, and C may read or fill
        // the array past it, silently (P3's review of 1b, item 1). Refused by name, as StructPointer refuses a struct
        // element; handing C the whole array, pinned, is a later seat.
        if (slot is ElemRefBox<ж<U>>)
            throw Refused(function, typeof(ж<U>), "an element of a Go array of C pointers, which C may read or fill past the one word go2cs would hand it (handing C the whole array is not supported yet)");

        RequireManagedSlot(slot);
        original = slot.ValueSlot;
        word = PointerMember(original);
        return GoCgoStructPointer.Mirror;
    }

    /// <summary>
    /// <see cref="OutParameter{U}(ж{ж{U}}, string, out nuint, out ж{U})"/>, taking also an element of a Go array of C
    /// pointers (cgo Phase 3, .5 cgo item 2, the argv shape: DESIGN-cgo-phase3-marshaling 18.12.3, H's verdict (A)).
    /// <c>C.f(&amp;argv[0])</c> lets C read or fill the whole array, so an element of managed storage crosses as a word
    /// mirror of the whole backing array (<paramref name="mirror"/>), pinned for the call by <paramref name="pin"/> at the
    /// element's word (<see cref="GoCgoStructPointer.Pinned"/>); the wrapper checks every word C wrote against the call's
    /// pins and copies the words back (<see cref="GoCgoWordMirror"/>). An element of a native window over C memory
    /// (<c>&amp;s[0]</c>, <c>s</c> an <c>unsafe.Slice</c> of a C <c>char *[]</c>) crosses by its own address, uncopied
    /// (<see cref="GoCgoStructPointer.Native"/>).
    /// </summary>
    /// <remarks>
    /// The mirror costs the backing array's length on every call, not the count C uses, and a re-slice keeps its whole
    /// backing array (docs/Limitations.md#cgo).
    /// </remarks>
    public static GoCgoStructPointer OutParameter<U>(ж<ж<U>>? slot, string function, out nuint word, out ж<U>? original, out GoCgoPin pin, out GoCgoWordMirror? mirror)
    {
        pin = default;
        mirror = null;

        if (slot is not ElemRefBox<ж<U>> element)
            return OutParameter(slot, function, out word, out original);

        RefusePinnerHeldCopy(slot, typeof(ж<U>), function);

        word = 0;
        original = null;

        if (element.NativeElementAddress is var address and not 0)
        {
            word = address;
            return GoCgoStructPointer.Native;
        }

        if (!element.TryGetElementStorage(out ж<U>[]? backing, out nint index) || index >= backing.Length)
            throw Refused(function, typeof(ж<U>), "an element of a Go array of C pointers whose storage go2cs cannot copy for the call");

        mirror = new GoCgoWordMirror(backing, static address => new NativeBox<U>(address));
        pin = mirror.PinAt(index);
        return GoCgoStructPointer.Pinned;
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
    /// Called inside the wrapper's <c>fixed</c>, after the call, for each word C returned (a pointer result, an
    /// out-parameter's word) against each pinned argument: a word that lies in that argument's Go storage is refused by
    /// name (P3's review of 1b, item 4). Minted as a native box, it would name Go memory that moves once the <c>fixed</c>
    /// ends; cgo's rule that C keeps no Go pointer makes the refusal right. A number argument pins nothing.
    /// </summary>
    public static unsafe void RequireWordOutsidePin(nuint word, in GoCgoPin pin, byte* pinned, string function)
    {
        if (word != 0 && pin.Holds(pinned, word))
            throw RuntimeErrorPanic.ErrorString($"cgo: C.{function} returned an address inside the Go memory it was handed, and go2cs cannot represent a pointer into Go memory that C returned, so it refuses the result");
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

    /// <summary>
    /// <see cref="PointerMemberBack{U}(nuint, ж{U}, nuint)"/>, asked also against the stack copies the wrapper handed C
    /// (cgo Phase 3, .5 cgo item 1(b): DESIGN-cgo-phase3-marshaling 18.12.3): an address that is a copy's own, of C's
    /// pointee type, is the box that copy stands for (Go's <c>out == &amp;a</c>); any other address inside a copy names a
    /// stack slot that dies with the call, and is refused by name, never minted as C memory.
    /// </summary>
    public static ж<U>? PointerMemberBack<U>(nuint address, ж<U>? original, nuint copiedIn, ReadOnlySpan<GoCgoStackCopy> copies, string function)
    {
        if (address == 0)
            return null;

        if (address == copiedIn)
            return original;

        foreach (ref readonly GoCgoStackCopy copy in copies)
        {
            if (copy.TryIdentity(address, out ж<U>? box))
                return box;
        }

        foreach (ref readonly GoCgoStackCopy copy in copies)
        {
            if (copy.Holds(address))
                throw RuntimeErrorPanic.ErrorString($"cgo: C.{function} returned an address inside the stack copy of a Go argument that is not that argument as a pointer to {typeof(U).Name}, and go2cs cannot represent a pointer into that copy, so it refuses the result");
        }

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

        // A NUMBER of Go memory (design 20.4 S4): `unsafe.Pointer(uintptr(unsafe.Pointer(&x)))`, in one expression or
        // through a variable, with or without an offset. It crosses as the pin of the storage it names, found through
        // the record its mint left, never trusted as an address.
        if (PinNumber(number, function) is { } numberPin)
            return numberPin;

        string? what = GoMemory(pointer, out nuint address);

        if (what is not null)
            throw VoidRefused(function, what);

        return new GoCgoPin(address);
    }

    /// <summary>
    /// <see cref="PinVoidPointer(ж{uintptr}, string)"/>, taking also an element of a Go array of C pointers handed over
    /// as a void pointer (<c>C.f(unsafe.Pointer(&amp;argv[0]))</c>, .5 cgo item 2): it crosses as a word mirror of the
    /// whole backing array, as the typed door's (<see cref="OutParameter{U}(ж{ж{U}}, string, out nuint, out ж{U}, out GoCgoPin, out GoCgoWordMirror)"/>),
    /// and the wrapper checks and copies back its words. Anything else is the void door's as before.
    /// </summary>
    public static GoCgoPin PinVoidPointer(ж<uintptr>? pointer, string function, out GoCgoWordMirror? mirror)
    {
        mirror = null;

        if (pointer is IUnsafePointer { RetainedSource: ICgoWordElement element } && element.TryCgoWordMirror(out GoCgoWordMirror? words, out nint index))
        {
            RefusePinnerHeldCopy(element, typeof(uintptr), function);
            mirror = words;
            return words.PinAt(index);
        }

        return PinVoidPointer(pointer, function);
    }

    // A number of Go memory handed to a void pointer (design 20.4 S4, with P3's conditions and COORD's ruling on Q3: both
    // forms). Null when the number names no Go memory golib knows, so the door goes on to its other tests (C memory crosses
    // as its number). The number is never trusted as an address: the record its mint left names the storage, the storage
    // is pinned for the call as any void pointer's is, and the offset is bounded by that storage.
    // - An exact address the registry resolves (`uintptr(unsafe.Pointer(&x))`): its box's storage, at the box's slot.
    // - An interior address inside a pin golib holds (`... + off`): that pin's storage, at the offset, inside its extent.
    // - An entry whose box has been collected: refused by name.
    // A number past a pin's end crosses as its number and pins nothing, as the pin-extent index rules (end-exclusive,
    // docs/phase4/DESIGN-cgo-pin-extents.md): one past the end is the end cursor of a begin/end pair, whose begin pins the
    // storage. Residue (stated in the design, 20.4 S4): a number further past, or one whose collected entry the sweep has
    // already removed, names nothing golib can see and crosses as C memory, as a wild pointer does under Go; and a token id
    // reused after its box died can name another live box (ids wrap at 2^29), remote and no worse than Go's use-after-free.
    private static unsafe GoCgoPin? PinNumber(nuint number, string function)
    {
        if (ManagedPointerTokens.HoldsCollectedEntry(number))
            throw VoidRefused(function, "a number naming a Go value that has been collected");

        if (ManagedPointerTokens.Resolve(number) is { } registered && registered is not INativeRooted { IsGenuinelyNativeRoot: true })
        {
            if (!TryPinTarget(registered, number, out Array? root, out nint offset))
                throw VoidRefused(function, "a number naming Go memory whose storage go2cs cannot hold still for the call");

            RequireCrossable(root);
            return new GoCgoPin(root, offset);
        }

        if (PinExtents.Find(number) is { } held && held.PinnedTarget is Array heldRoot && held.TryExtent(out nuint start, out nuint end) && number >= start && number < end)
        {
            RequireCrossable(heldRoot);
            return new GoCgoPin(heldRoot, (nint)(number - start));
        }

        return null;
    }

    /// <summary>
    /// What C receives for a 64-bit integer parameter (design 20.4 S4 and 20.10, P3's reads): the integer as it is,
    /// unless it is a TOKEN go2cs MINTED, the number it gives <c>uintptr(unsafe.Pointer(p))</c> for a Go value that has no
    /// address (it holds Go references). Go would hand C that value's address; a token names no memory, so C would receive
    /// silent garbage, and the call is refused by name instead. Any other integer crosses, a real address of Go memory
    /// included: an integer is what Go lets C see.
    /// </summary>
    /// <remarks>
    /// The bit pattern alone is not the test (P3's MUST on 53d4da880e): one 64-bit value in four has bit 63 set and bit
    /// 47 clear, so a hash or an opaque id would be refused where Go passes it. A tagged number is refused only when the
    /// registry holds an entry for it, live or collected. Every token the box-to-uintptr operator mints is registered
    /// there (its reference-bearing arm, ж.cs), so a minted token is always seen; a tagged number never minted crosses.
    /// </remarks>
    public static ulong Word(ulong value, string function)
    {
        if (ManagedPointerTokens.IsTaggedToken((nuint)value) && (ManagedPointerTokens.HoldsCollectedEntry((nuint)value) || ManagedPointerTokens.Resolve((nuint)value) is not null))
            throw RuntimeErrorPanic.ErrorString($"cgo: the pointer-sized integer handed to C.{function} is go2cs's token for a Go value that has no address (it holds Go references), not that value's address; C would receive a number that names no memory, so go2cs refuses the call");

        return value;
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

        // The pinned native box is the carried box itself, or the first one up its derivations: a field of a view, a view
        // of a view, an element of unsafe.Slice over a view and unsafe.Add over any of them each retain the box they
        // were cut from (NativeDerivation.Kept), and the view's pin holds every byte they name.
        for (object? hop = carried; hop is not null && hop is not INativeRooted { IsGenuinelyNativeRoot: true }; hop = DerivedFrom(hop))
        {
            if (hop is not IGoReflectBox { NativeAddress: not 0 } || hop is not IManagedPinHolder { ManagedPin: { } pin })
                continue;

            if (pin.PinnedTarget is not Array pinnedArray || pinnedArray.Length == 0)
                return false;

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

    // The pin for a reinterpret view's root, as the element door pins an element: its storage and the root's offset in it.
    // False when no door can name the storage (the caller keeps its mirror); a token and storage holding Go pointers are
    // refused with cgo's text.
    private static bool TryPinReinterpretRoot(object root, out GoCgoPin pin)
    {
        pin = default;

        if (root is INativeRooted { IsGenuinelyNativeRoot: true })
            return false;

        nuint number = RootNumber(root);

        if (ManagedPointerTokens.IsTaggedToken(number))
            throw GoPointerToUnpinnedGoPointer();

        if (!TryPinTarget(root, number, out Array? storage, out nint offset))
            return false;

        RequireCrossable(storage);
        pin = new GoCgoPin(storage, offset);
        return true;
    }

    // The address a reinterpret view's root names: a native box's own, an element of a native array window's (which has no
    // native box of its own: `&h.name[0]` over a view), else 0 for a managed box, which TryPinTarget reads from its slot.
    private static nuint RootNumber(object root) =>
        root is IGoReflectBox { NativeAddress: not 0 and var native } ? native :
        root is INativeIdentity { NativeIdentityOrZero: var identity } ? identity : 0;

    // The box a derived box was cut from (one hop of NativeDerivation.HeldSource's walk), or null at a root.
    // An element of a native array window (`&h.name[0]` over a view) derives from nothing itself and is anchored to the
    // window, which derives from the view: both hops are taken.
    private static object? DerivedFrom(object box) =>
        (box is INativeRooted { NativeRootAddress: not 0 } native ? native.NativeDerivedFrom : null) ??
        (box is INativeViewAnchor { AnchoredRoot: { } root } ? root : null);

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

    // cgo S6 (DESIGN-cgo-phase3-marshaling 21.3 (b), 21.7 SHOULD 3): a pointer into a value pinned with runtime.Pinner, which
    // C may KEEP past the call, never crosses as a stack mirror or an image, whose address is a copy's. It crosses in place,
    // held still by the Pinner's own handle (and the wrapper's `fixed` for the call), when its CLR layout is its C layout:
    // the storage holds no reference (the S1 condition). A pinned value that fails the test is refused by name. False when
    // nothing pinned the pointer's value, so the door's call-scoped answer stands.
    private static bool TryPinnerHeld<T>(ж<T> pointer, string function, out GoCgoPin pin)
    {
        pin = default;

        if (pointer.IsNilPointer || pointer is NativeBox<T> { NativeAddress: not 0 } native && IsGenuinelyNative(native))
            return false;

        bool held = GoPinTable.IsPinned(pointer.ReferentObject);
        bool located = TryPinTarget(pointer, 0, out Array? root, out nint offset);

        if (located)
            held |= GoPinTable.HoldsStorage(root!);

        if (!held)
            return false;

        if (!located || RuntimeHelpers.IsReferenceOrContainsReferences<T>() || NativeStructMarshal.ContainsReferences(root!.GetType().GetElementType()!))
            throw PinnerHeldRefused(function, typeof(T), "a Go value whose Go layout is not its C layout");

        RequireCrossable(root!);
        pin = new GoCgoPin(root!, offset);
        return true;
    }

    // cgo S6, P3's MUST 1 on 2ce6812305: the out-parameter door and the void door's word-mirror arm hand C a COPY of a Go
    // pointer slot (one stack word, or a mirror of the whole array of C pointers). C keeping that address after the call
    // keeps a dead stack word or a mirror the wrapper copies back once. A slot pinned with runtime.Pinner says C may keep
    // it, and a slot holds a reference here, so it has no in-place answer: refused by name. Unpinned slots are unchanged.
    private static void RefusePinnerHeldCopy(object box, Type type, string function)
    {
        if (GoPinTable.Any && box is INilPointer { IsNilPointer: false } pointer && GoPinTable.IsPinned(pointer.ReferentObject))
            throw PinnerHeldRefused(function, type, "a Go pointer slot go2cs hands C as a copy (a stack word, or a word mirror of its array)");
    }

    private static PanicException PinnerHeldRefused(string function, Type type, string what) =>
        RuntimeErrorPanic.ErrorString($"cgo: the {type.Name} pointer handed to C.{function} is pinned with runtime.Pinner, so C may keep it past the call, and it is {what}; go2cs cannot hand C that value's own memory, so it refuses the call (docs/Limitations.md#cgo)");

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
        else if (pointer is IReinterpretView { ReinterpretRoot: { } viewRoot })
        {
            // `(*C.char)(unsafe.Pointer(&h.name[0]))`: a reinterpret view is its root's address, so the root names the
            // storage, as it does for the typed door.
            carried = viewRoot;
            number = RootNumber(viewRoot);
        }
        else
        {
            carried = pointer;
            number = RootNumber(pointer);
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
    /// part 1c: docs/phase4/DESIGN-cgo-phase3-marshaling.md 18.11).
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
    // the view) is refused before any read or write: its CLR slot is not where the field's bytes are (18.11, review item M1).
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
