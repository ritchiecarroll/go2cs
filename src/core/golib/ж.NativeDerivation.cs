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
internal interface IHeldStorage
{
    // The bytes of Go storage this box holds still from `address` to the end of the object it pinned, or 0 when it
    // holds nothing still at that address. Asked by the array-pointer view doors before they view a Go value of
    // another type as native memory (docs/phase4/DESIGN-array-pointer-doors.md 4.2, V2): a window is cut only over
    // bytes this answers for, and it is anchored on this box, whose pin is released when the box is collected.
    nuint HeldBytesFrom(nuint address);

    // The Go storage this box holds still, from its first byte to its end, or false when it holds none: nothing was
    // pinned, the storage cannot be pinned, or the pin holds storage the value no longer has. Asked for a pointer
    // DERIVED from this box (an offset, an element of a view of it): its address is inside the storage and is no
    // address the registry knows, so the box cannot be asked about that address alone (design 13.2).
    bool TryHeldExtent(out nuint start, out nuint end);
}

// A view of memory that keeps a root: an element of a native-backed slice, or a window over a native array. The root
// is what the view was cut over (NativeSliceAnchors), null for a view of C memory alone.
internal interface INativeViewAnchor
{
    object? AnchoredRoot { get; }
}

internal static class NativeDerivation
{
    // What a box derived from `root` retains: `root` itself, unless it is a native box over C memory alone. The other
    // native kinds (a window over a native array) retain nothing here, as before.
    //
    // ONE HOP (2026-11-15, docs/phase4/DESIGN-array-pointer-doors.md 15). A native box that owns no pin holds its
    // source and nothing else, so a box derived from it retains that SOURCE in its place. Before, each step of
    // `for ... { p = unsafe.Add(p, 1) }` retained the box of the step before: N steps held N boxes, Token walked N
    // links to compare two pointers, and HeldSource stopped short of the Go value past its depth bound, so a view at
    // the walked address lost the value's end. The order token is the same either way: it is the source's token plus
    // the sum of the distances, and the sum from the source to this address does not depend on the steps taken. The
    // loop serves a chain no mint makes now (one built link by link).
    internal static object? Kept(object? root)
    {
        if (root is INativeRooted { NativeRetainedInItsPlace: { } source })
        {
            while (source is INativeRooted { NativeRetainedInItsPlace: { } next })
                source = next;

            return source;
        }

        return root switch
        {
            null => null,
            INativeRooted rooted => rooted.IsGenuinelyNativeRoot ? null : root,
            IGoReflectBox { NativeAddress: not 0 } => null,
            _ => root
        };
    }

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

    // The first MANAGED box `kept` was derived from, for the array-pointer view doors at an address inside a Go value
    // (design 13.2). A chain of derivations is walked as Token walks it, and through a view as well: an element of
    // a native-backed slice, or a native array window, that keeps a root was cut over that root. Null for C memory
    // alone and for a root that is no managed box, where nothing here knows the storage's end.
    internal static IHeldStorage? HeldSource(object? kept)
    {
        for (int hop = 0; kept is not null && hop < 64; hop++)
        {
            if (kept is INativeRooted { NativeRootAddress: not 0 } native)
            {
                kept = native.NativeDerivedFrom;
                continue;
            }

            if (kept is INativeViewAnchor { AnchoredRoot: { } root })
            {
                kept = root;
                continue;
            }

            return kept is IHeldStorage storage && kept is INativeIdentity { NativeIdentityOrZero: 0 } &&
                   kept is IGoReflectBox { IsNilPointer: false, NamesZeroBase: false }
                ? storage
                : null;
        }

        return null;
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
    // measured from. A chain of derivations is walked to its first managed source, in a LOOP: the distances are
    // summed link by link, and the first link that is not derived from a native address answers the base. It was a
    // call per link, and a pointer advanced 1,000,000 steps overflowed the stack when it was compared (Kept keeps
    // such a pointer one link long now; the loop is for a chain built any other way).
    //
    // A box over C memory alone, over an order token, or derived from a source with no address answers its own
    // number, as before.
    internal static nuint Token(INativeRooted box)
    {
        nuint address = box.NativeRootAddress;
        nuint moved = 0;
        object? from = box.NativeDerivedFrom;

        while (from is INativeRooted { NativeRootAddress: var at and not 0 } inner)
        {
            moved = unchecked(moved + (address - at));
            address = at;
            from = inner.NativeDerivedFrom;
        }

        if (from is IGoReflectBox { NativeAddress: 0, IsNilPointer: false, NamesZeroBase: false } source && source is INilPointer tokened)
        {
            nuint held = source.NativeArgumentAddress();

            if (held != 0 && !ManagedPointerTokens.IsTaggedToken(held))
                return unchecked(tokened.PointerOrderToken + (address - held) + moved);
        }

        return unchecked(address + moved);
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
// zero-length array, and that array is tied to the root in GoBoxTables.SliceRoots. A re-slice passes the array along,
// so the root lives as long as any slice cut from the first, and an element or SliceData of one is minted retaining
// the root.
internal static class NativeSliceAnchors<T>
{
    // The anchor array for a slice over `root`'s memory, or null when there is nothing to keep.
    internal static T[]? For(object? root)
    {
        if (root is null)
            return null;

        T[] anchor = new T[0];
        GoBoxTables.SliceRoots.Add(anchor, root);

        return anchor;
    }

    // A slice over foreign pointer WORDS is read into a fresh managed array (NativePointerWords.Window), which keeps
    // nothing of the memory it was read from. Over a pointer that is not C memory alone, that array is tied to the
    // root here, so the slice keeps the view it was read over (unsafe.Slice with a pointer element; DESIGN-interior-
    // pointer-root 4.5). A slice with no elements keeps nothing.
    internal static void Keep(in slice<T> words, object? root)
    {
        if (root is not null && words.m_array is { Length: > 0 } array)
            GoBoxTables.SliceRoots.AddOrUpdate(array, root);
    }

    // The root tied to an anchor array, or null.
    internal static object? OfAnchor(T[]? anchor) =>
        anchor is not null && GoBoxTables.SliceRoots.TryGetValue(anchor, out object? root) ? root : null;

    // The root a native-backed slice keeps, or null: one reference compare for every slice that keeps none.
    internal static object? Of(in slice<T> view) =>
        view.IsNativeBacked && !ReferenceEquals(view.m_array, Array.Empty<T>()) && GoBoxTables.SliceRoots.TryGetValue(view.m_array, out object? root)
            ? root
            : null;
}
