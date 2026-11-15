// ж.InlineSlice.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System.ComponentModel;
using System.Runtime.CompilerServices;
using go.golib;

namespace go;

public abstract partial class ж<T>
{
    /// <summary>
    /// Go's <c>x.f[:]</c> for an array <c>f</c> held inline (a <c>go.arrays</c> member,
    /// docs/phase4/DESIGN-array-value-storage.md 7.7) in the storage this box names: the slice Go means, which
    /// ALIASES the struct's own bytes. The converter emits it over the field's address, <c>&amp;x.f</c>, and applies
    /// the expression's bounds to the result, so <c>x.f[lo:hi]</c> reads <c>Ꮡx.of(X.Ꮡf).ΔInlineSlice&lt;E&gt;()[lo..hi]</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An inline array is the struct's own memory, not a <c>T[]</c> a <see cref="slice{T}"/> can share, so the slice is a
    /// native window over that memory. The address is this box's own uintptr conversion, which is the pin moment
    /// (EnsureStableAddress stores the pin in the box) and the provenance record. The slice then KEEPS this box, through
    /// its anchor (NativeSliceAnchors), as every native window cut from a Go pointer does, so the box, its pin and the
    /// storage live as long as any slice cut from this one. The pin is released by the box's finalizer when nothing
    /// reaches it: the pin's lifetime is the box's, never a statement's.
    /// </para>
    /// <para>
    /// Not <c>unsafe.Slice((*E)(unsafe.Pointer(&amp;x.f)), N)</c>: that door asks the provenance record first, and an
    /// address the record resolves to a live Go box is read into a COPY (unsafe.Slice's managed fall-through), which
    /// is right for the pointers it exists for and wrong here, where the point is the alias. And a consumer of a
    /// foreign inline array need not import unsafe at all (go-winio's pkg/guid does not).
    /// </para>
    /// <para>
    /// The element must hold no managed reference: such an array cannot be pinned, and a native window cannot carry it.
    /// The converter emits this member only for such elements, so the refusal here is a guard, not a path.
    /// </para>
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public slice<TElem> ΔInlineSlice<TElem>()
    {
        int length = InlineArrayShape<T, TElem>.Length;

        if (length < 0)
            throw new PanicException($"inline array slice: {typeof(T).Name} is not an inline array of {typeof(TElem).Name}");

        if (IsNilPointer)
            throw RuntimeErrorPanic.NilPointerDereference();

        if (StorageKind is not PointerStorage.Pinnable && NativeAddress == 0)
            throw new PanicException("inline array slice: an array held inline in a struct that also holds a Go reference cannot be held still, so it cannot be sliced");

        nuint address = ((uintptr)this).Value;

        return slice<TElem>.OverNativeMemory(address, length, length, NativeSliceAnchors<TElem>.For(NativeDerivation.Kept(this)));
    }
}

// The shape of an inline array TArray of TElem: its Go length, or -1 when TArray is not an inline array whose storage is
// exactly that many TElem. Read once per type pair.
internal static class InlineArrayShape<TArray, TElem>
{
    internal static readonly int Length = Read();

    private static int Read()
    {
        if (default(TArray) is not IGoInlineArray array || RuntimeHelpers.IsReferenceOrContainsReferences<TArray>())
            return -1;

        long length = array.Length;

        return Unsafe.SizeOf<TArray>() == length * Unsafe.SizeOf<TElem>() ? (int)length : -1;
    }
}
