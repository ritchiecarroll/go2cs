// ж.NativeDerivation.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;

namespace go;

// What a native box DERIVED from another pointer keeps of it (cgo Phase 3 increment 3.1, the review's findings;
// docs/phase4/DESIGN-cgo-phase3-marshaling.md 16.11).
//
// A native box is an address. One minted over MANAGED storage holds what keeps that storage where the address says it
// is: a pin, or the box it was reinterpreted from. A box derived from it (a field, a second reinterpret, an offset, an
// element of an unsafe.Slice over it) was minted from the address alone, so the pin could be released under a pointer
// still in use, and every cgo door took the derived box for C memory. The rule here is one line: a box derived from a
// pointer that is not C memory alone RETAINS that pointer. The root, its pin and its source then live as long as the
// derived box, and a door refuses it by the test it already makes (a native box with a retained source is not C
// memory alone). A native box's retained source has one production reader, the cgo doors.
internal static class NativeDerivation
{
    // What a box derived from `root` retains: `root` itself, unless it is a native box over C memory alone. The other
    // native kinds (a window over a native array) retain nothing here, as before.
    internal static object? Kept(object? root) => root switch
    {
        null => null,
        INativeRooted rooted => rooted.IsGenuinelyNativeRoot ? null : root,
        IGoReflectBox { NativeAddress: not 0 } => null,
        _ => root
    };

    // The number-to-pointer conversion's own case: its number RESOLVED to a live Go box of another pointee type, and
    // the native box it mints over that number is a view of that box's pinned storage. A flagged order token keeps its
    // own carrier, which no door lets through.
    // A resolved box that itself names NATIVE memory (an element or field over a native window) keeps nothing, as
    // before: what it names is not storage this box holds.
    internal static object? KeptResolved(object? resolved, bool aliasesAnOrderToken) =>
        aliasesAnOrderToken || resolved is INativeIdentity { NativeIdentityOrZero: not 0 } ? null : resolved;

    // unsafe.Add over an unsafe.Pointer that carries its box: the box the moved pointer carries, a native box AT the
    // new value that retains what the pointer carried. Null where there is nothing to keep (the pointer carried
    // nothing, or C memory alone), and for a token, whose arithmetic keeps its own route and which no door lets
    // through.
    internal static ж<byte>? Moved(object? carried, nuint value)
    {
        if (value == 0 || ManagedPointerTokens.IsTaggedToken(value) || Kept(carried) is not { } kept)
            return null;

        return new NativeBox<byte>(value, retainedSource: kept);
    }

    // THE ORDER TOKEN OF A DERIVED BOX (2026-11-14), read by unsafe.Pointer's equality and hash and by nothing else.
    //
    // A managed box is identified by its order token (allocation base + Go offset), which is stable and needs no
    // pin. A native box is identified by its address. A native box DERIVED from a managed box is the same Go pointer
    // as a managed pointer to that place (`unsafe.Add(p, 0)` is p; `unsafe.Add(unsafe.Pointer(&a[0]), 8)` is
    // `unsafe.Pointer(&a[1])`), so the two must answer ONE identity, or they compare unequal and hash apart. This
    // answers the derived box's place in the token space of what it was derived from: the source's token plus the
    // distance from the source's address to the box's own. The source of a derived box is held still for the box's
    // life (the derivation asked its number, which is the pin moment), so its address is the one the distance was
    // measured from. A chain of derivations is walked to its first managed source.
    //
    // A box over C memory alone, over an order token, or derived from a source with no address answers its own
    // number, as before.
    internal static nuint Token(INativeRooted box)
    {
        nuint address = box.NativeRootAddress;

        switch (box.NativeDerivedFrom)
        {
            case INativeRooted { NativeRootAddress: var from and not 0 } inner:
                return unchecked(Token(inner) + (address - from));

            case IGoReflectBox { NativeAddress: 0, IsNilPointer: false, NamesZeroBase: false } source when source is INilPointer tokened:
                nuint held = source.NativeArgumentAddress();

                return held == 0 || ManagedPointerTokens.IsTaggedToken(held) ? address : unchecked(tokened.PointerOrderToken + (address - held));

            default:
                return address;
        }
    }
}

// Whether a box names native memory, asked without its pointee type.
internal interface INativeIdentity
{
    nuint NativeIdentityOrZero { get; }
}

public abstract partial class ж<T> : INativeIdentity
{
    nuint INativeIdentity.NativeIdentityOrZero => NativeIdentity;
}

// The root an unsafe.Slice over a pointer that is not C memory alone keeps, with no word added to slice<T>: a
// native-backed slice's array field is unused (it holds the shared empty array), so such a slice is given its OWN
// zero-length array, and that array is tied to the root here. A re-slice passes the array along, so the root lives as
// long as any slice cut from the first, and an element or SliceData of one is minted retaining the root.
internal static class NativeSliceAnchors<T>
{
    private static readonly ConditionalWeakTable<T[], object> s_roots = new();

    // The anchor array for a slice over `root`'s memory, or null when there is nothing to keep.
    internal static T[]? For(object? root)
    {
        if (root is null)
            return null;

        T[] anchor = new T[0];
        s_roots.Add(anchor, root);

        return anchor;
    }

    // A slice over foreign pointer WORDS is read into a fresh managed array (NativePointerWords.Window), which keeps
    // nothing of the memory it was read from. Over a pointer that is not C memory alone, that array is tied to the
    // root here, so the slice keeps the view it was read over (unsafe.Slice with a pointer element; DESIGN-interior-
    // pointer-root 4.5). A slice with no elements keeps nothing.
    internal static void Keep(in slice<T> words, object? root)
    {
        if (root is not null && words.m_array is { Length: > 0 } array)
            s_roots.AddOrUpdate(array, root);
    }

    // The root a native-backed slice keeps, or null: one reference compare for every slice that keeps none.
    internal static object? Of(in slice<T> view) =>
        view.IsNativeBacked && !ReferenceEquals(view.m_array, Array.Empty<T>()) && s_roots.TryGetValue(view.m_array, out object? root)
            ? root
            : null;
}
