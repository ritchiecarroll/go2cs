// GoZeroSize.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable StaticMemberInGenericType

using System;
using System.Reflection;

namespace go;

/// <summary>
/// Whether <typeparamref name="T"/> is a Go ZERO-SIZE type — one whose values carry no state and
/// therefore occupy no storage — and, when it is, the single shared element every value of it is.
/// </summary>
/// <remarks>
/// <para>
/// Go's <c>struct{}</c> (and any struct built only from such fields) has size 0, so
/// <c>make([]struct{}, n)</c> allocates NOTHING for any <c>n</c> up to <c>math.MaxInt</c> —
/// <c>mallocgc(0, …)</c> returns the address of the runtime's global <c>zerobase</c> and charges no
/// malloc. That is not a corner case dressed up as one: <c>slices.Concat</c>, <c>slices.Repeat</c> and
/// their tests use <c>[]struct{}</c> precisely BECAUSE it lets them exercise the length arithmetic at
/// <c>MaxInt</c> without touching memory. golib allocated a real backing array for the same
/// expression and panicked <c>makeslice: len out of range</c> at
/// <see cref="Array.MaxLength"/> — a length ceiling Go does not have here, because Go has nothing to
/// allocate.
/// </para>
/// <para>
/// <b>The predicate is a SHAPE question, not a size question.</b> <c>Unsafe.SizeOf&lt;T&gt;()</c>
/// cannot answer it: C# gives an empty struct one byte, so every zero-size Go type measures 1 here.
/// What survives the conversion faithfully is the FIELD SET — a Go struct is zero-size exactly when it
/// has no fields of nonzero size, and the emitted C# struct carries the same fields — so the
/// classification asks that instead, recursively. A type with no instance fields at all is the base
/// case and the common one (<see cref="EmptyStruct"/>, and every <c>[GoType] partial struct noCopy
/// { }</c> the converter emits for a named <c>struct{}</c>).
/// </para>
/// <para>
/// <b>Known and deliberate divergence:</b> Go's OTHER zero-size shape is the zero-length array
/// (<c>[0]T</c>, and <c>[N]struct{}</c>). go2cs emits a Go array as <see cref="array{T}"/>, whose
/// backing is a managed reference field, so such a type classifies here as NON-zero-size and keeps the
/// allocating path. That is the honest answer for the representation as it stands rather than a gap
/// papered over: nothing in the converted corpus builds a slice of them at a length no array could
/// hold, and claiming zero-size for a type whose C# shape genuinely carries a reference would put a
/// wrong element ref in front of every consumer.
/// </para>
/// </remarks>
/// <typeparam name="T">Element type to classify.</typeparam>
internal static class GoZeroSizeFacts<T>
{
    /// <summary>
    /// Whether <typeparamref name="T"/> occupies no storage in Go. A <c>static readonly</c> per closed
    /// <typeparamref name="T"/>, so every gate written against it folds at JIT time and no ordinary
    /// element type pays for the branch.
    /// </summary>
    internal static readonly bool IsZeroSize = GoZeroSizeFacts.Classify(typeof(T));

    /// <summary>
    /// The ONE element every zero-size value of <typeparamref name="T"/> is — golib's
    /// <c>zerobase</c>. It is also the non-null backing a storage-free slice carries, so
    /// <c>s == nil</c> stays false for a <c>make</c>d one exactly as it does for every other slice.
    /// Non-zero-size types get <see cref="Array.Empty{T}"/> and never read it.
    /// </summary>
    internal static readonly T[] Storage = IsZeroSize ? new T[1] : [];
}

/// <summary>
/// The zero-size rule itself, asked of a <see cref="Type"/>: the ONE source of the fact. <see cref="GoZeroSizeFacts{T}"/>
/// caches it per closed type, and <see cref="GoLayoutFacts{T}"/> asks it of each field type
/// its layout walk meets, which a generic cache cannot answer without closing a generic type per field.
/// </summary>
internal static class GoZeroSizeFacts
{
    /// <summary>Whether <paramref name="type"/> occupies no storage in Go (see <see cref="GoZeroSizeFacts{T}"/>).</summary>
    internal static bool Classify(Type type)
    {
        // A reference is a pointer-sized value in Go's terms and in .NET's; a primitive, enum or
        // pointer has a width by definition. Only a struct can be zero-size.
        if (!type.IsValueType || type.IsPrimitive || type.IsEnum || type.IsPointer)
            return false;

        // A generic type parameter reaching here would be an open type — it cannot, since T is
        // always closed at the point a static generic field initializes.
        foreach (FieldInfo field in GoFieldMetadata.InstanceFields(type))
        {
            if (!Classify(field.FieldType))
                return false;
        }

        // No instance fields, or every one of them zero-size: Go's own rule, and the recursion
        // terminates because a struct cannot contain itself.
        return true;
    }
}

/// <summary>
/// The element a non-nil ZERO-CAPACITY <see cref="slice{T}"/>'s data pointer names — golib's stand-in
/// for the <c>zerobase</c> Go's <c>make([]T, 0)</c> answers.
/// </summary>
/// <remarks>
/// <c>unsafe.SliceData</c> over a non-nil slice of capacity 0 returns "a non-nil pointer to an
/// unspecified memory address", and no correct program reaches an element through it, because there
/// is none. Minting element 0 of the slice's own backing named a slot that does not exist (a zero-length
/// backing, or one past the end of a <c>s[len:len]</c> window), so reading or converting the pointer
/// raised <see cref="IndexOutOfRangeException"/> — a host fault Go has no equivalent of. This element is
/// real storage instead: one slot per element type, never charged to a Go allocation, and for a
/// zero-size <typeparamref name="T"/> the very slot <see cref="GoZeroSizeFacts{T}.Storage"/> every
/// element of every such slice already is.
/// </remarks>
/// <typeparam name="T">Element type of the zero-capacity slice.</typeparam>
internal static class GoZeroCapacityElement<T>
{
    /// <summary>The shared element pointer.</summary>
    internal static readonly ж<T> Element = new ElemRefBox<T>(new slice<T>(GoZeroCapacitySlot<T>.Backing), 0);
}

/// <summary>
/// The one slot <see cref="GoZeroCapacityElement{T}"/> names. Its own class so that asking whether an
/// element reference is over it (<see cref="ж{T}.NamesZeroBase"/>) never mints the element box, an
/// allocation the counter would charge to whichever operation first asked.
/// </summary>
/// <typeparam name="T">Element type of the zero-capacity slice.</typeparam>
internal static class GoZeroCapacitySlot<T>
{
    /// <summary>The slot; an element reference over it names <see cref="GoZeroBase"/>.</summary>
    internal static readonly T[] Backing = GoZeroSizeFacts<T>.IsZeroSize ? GoZeroSizeFacts<T>.Storage : new T[1];
}

/// <summary>
/// golib's <c>runtime.zerobase</c>: the ONE address Go's mallocgc answers for every zero-byte
/// allocation, and the pointer every zero-size allocation here compares, hashes, orders and converts as.
/// </summary>
/// <remarks>
/// <para>
/// Go measured (go1.24.13, escaping values): <c>new(struct{})</c>, a named empty struct, a struct of
/// only zero-size fields, every element of a <c>[]struct{}</c> and the data word of a
/// <c>make([]T, 0)</c> are all <c>&amp;zerobase</c>, so they are one pointer, and a
/// <c>map[*struct{}]V</c> keeps one key for two <c>new</c>s. runtime's own
/// <c>var zerobase uintptr</c> is this box (runtime/malloc_impl.cs, a manualConversionVars
/// registration), which is what makes runtime's <c>ZeroBase</c> equal to them.
/// </para>
/// <para>
/// <b>Which pointers name it</b> (<see cref="ж{T}.NamesZeroBase"/>): a non-nil heap box of a zero-size
/// type (<see cref="StandardBox{T}"/>), and an element reference over the shared zero-size slot or the
/// zero-capacity slot (<see cref="GoZeroSizeFacts{T}.Storage"/>, <see cref="GoZeroCapacitySlot{T}"/>).
/// <b>Which never do</b>: a field reference (Go points a zero-size field at the end of its struct), a
/// native alias, a reinterpreting view, a zero-length ARRAY type (<see cref="GoZeroSizeFacts{T}"/>'s
/// documented divergence), and a <see cref="HandleBox{T}"/>, runtime's opaque <c>*Func</c>, which is
/// zero-size in Go but names a function rather than an allocation.
/// </para>
/// <para>
/// Box OBJECTS stay distinct: nothing about allocation changes, so every table keyed on a box object
/// answers as before. Only the answers to "which pointer is this" change: equality, hash, order token,
/// referent and address. runtime treats the referent as Go treats zerobase, outside every heap span:
/// SetFinalizer and AddCleanup register nothing and a Pinner ignores it.
/// </para>
/// </remarks>
internal static class GoZeroBase
{
    /// <summary>The storage: runtime's <c>zerobase</c> word.</summary>
    internal static readonly StandardBox<uintptr> Box = new(default(uintptr));

    /// <summary>The order token every zerobase pointer answers: <see cref="Box"/>'s own.</summary>
    internal static readonly nuint Token = Box.PointerOrderToken;

    /// <summary>The hash every zerobase pointer answers.</summary>
    internal static readonly int HashCode = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(Box);

    /// <summary>
    /// The address every zerobase pointer converts to: <see cref="Box"/>'s pinned slot, pinned and
    /// registered once by the ordinary conversion, so the number resolves back to <see cref="Box"/>.
    /// </summary>
    internal static readonly uintptr Address = (uintptr)(ж<uintptr>)Box;

    /// <summary>Whether <paramref name="referent"/> is the zerobase allocation.</summary>
    internal static bool Is(object? referent) => ReferenceEquals(referent, Box);
}
