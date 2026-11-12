// GoCgoArgs.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

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
    Native
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
                return GoCgoStructPointer.Mirror;

            case FieldRefBox<T> field:
                if (((INativeRooted)field).IsNativeRooted)
                    throw Refused(function, typeof(T), "a field of a struct in native memory, which Phase 2 does not yet pass");

                return GoCgoStructPointer.Mirror;

            case ElemRefBox<T>:
                throw Refused(function, typeof(T), "an element of a Go array or slice: cgo lets C touch the whole array, which one struct's mirror cannot hold (Phase 3)");

            default:
                throw Refused(function, typeof(T), $"a {pointer.GetType().Name}, a pointer kind Phase 2 does not classify");
        }
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

    private static bool IsGenuinelyNative<T>(NativeBox<T> native) =>
        native.RetainedSource is null && !native.AliasesAnOrderToken && !native.PinsManagedStorage;

    private static PanicException Refused(string function, Type pointee, string what) =>
        RuntimeErrorPanic.ErrorString($"cgo: the {pointee.Name} pointer handed to C.{function} is {what}, so go2cs refuses the call");
}
