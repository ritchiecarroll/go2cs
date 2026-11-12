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
            throw RuntimeErrorPanic.ErrorString($"cgo: the void pointer handed to C.{function} is {what}, which go2cs does not hand to C until cgo Phase 3's increment 3.2, so it refuses the call");

        return address;
    }

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

    /// <summary>The number of bytes <c>C.CString</c> allocates: the string's bytes and a NUL.</summary>
    public static nuint StringSize(@string s) => (nuint)s.Length + 1;

    /// <summary>The number of bytes <c>C.CBytes</c> allocates: the slice's length.</summary>
    public static nuint BytesSize(slice<byte> b) => (nuint)b.ToSpan().Length;

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
        // box says what the memory is. C memory is a native box over C memory alone, whose number is its address.
        if (pointer is IUnsafePointer { RetainedSource: { } carried })
        {
            if (carried is INativeRooted { IsGenuinelyNativeRoot: true })
            {
                address = number;
                return null;
            }

            return carried is INativeRooted { IsNativeRooted: true }
                ? "a pointer reached through a Go pointer into C memory (a field, or a view over managed storage)"
                : "a pointer into Go memory";
        }

        // By the number's own bits (bit 63 set, bit 47 clear), which no user address has. A C sentinel such as
        // (void *)-1 has both set, and crosses.
        if (ManagedPointerTokens.IsTaggedToken(number))
            return "a token for a Go value that has no address (it holds Go references)";

        if (ManagedPointerTokens.Resolve(number) is not null)
            return "the address of Go memory (a Go variable, field or element)";

        address = number;
        return null;
    }

    private static PanicException HelperRefused(string helper, string what) =>
        RuntimeErrorPanic.ErrorString($"cgo: C.{helper} over Go memory: the pointer it was handed is {what}. Reading Go memory through cmd/cgo's helpers is cgo Phase 3's increment 3.2; until then go2cs refuses the call");

    private static bool IsGenuinelyNative<T>(NativeBox<T> native) =>
        native.RetainedSource is null && !native.AliasesAnOrderToken && !native.PinsManagedStorage;

    private static PanicException Refused(string function, Type pointee, string what) =>
        RuntimeErrorPanic.ErrorString($"cgo: the {pointee.Name} pointer handed to C.{function} is {what}, so go2cs refuses the call");
}
