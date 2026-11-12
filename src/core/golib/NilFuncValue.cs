// NilFuncValue.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// The canonical typed nil for a FUNC type crossing into INTERFACE space — the delegate-shaped
/// sibling of <c>ж&lt;T&gt;.NilBox</c> (one nil encoding system-wide). A Go func emits as a
/// managed delegate whose nil IS <c>null</c>: correct in every func-typed slot, and type-erasing
/// inside an interface, where Go packs (type=func-type, value=nil) — a NON-nil interface whose
/// <c>%T</c> prints the func type and whose type assertion succeeds with a nil result.
/// </summary>
/// <remarks>
/// Minted ONLY by <see cref="GoReflect.CanonicalNilFunc"/> (interned per delegate type) at the
/// eface boundary — reflect's <c>packInterfaceValue</c> and internal/reflectlite's
/// <c>valueInterface</c> — for a null read out of a FUNC-kinded slot. Every read-back path
/// resolves it away: <see cref="GoReflect.GoDynamicTypeOf"/> answers the delegate type, the type
/// assertion (<c>builtin.TryTypeAssert</c>) succeeds against exactly that type with the null
/// delegate as the value, <see cref="GoReflect.TryMarshalAssignable"/> stores it as null, and
/// <see cref="GoReflect.IsNilGoValue"/> answers nil — so the carrier can never be stored into a
/// func-typed slot or observed as itself.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class NilFuncValue
{
    /// <summary>The managed delegate type this nil func carries — Go's eface type word.</summary>
    public Type Type { get; }

    internal NilFuncValue(Type type)
    {
        Type = type;
    }
}

/// <summary>
/// The FUNC boundary accessor — the delegate-shaped sibling of <c>ж&lt;T&gt;.OrTypedNil</c>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)] public static class NilFuncExtensions
{
    /// <summary>
    /// Boxes a func value for an EMPTY-interface slot, carrying its type when the value is nil.
    /// </summary>
    /// <typeparam name="TDelegate">The Go func type's C# delegate — the eface type word.</typeparam>
    /// <param name="value">The func value, which may be <c>null</c>.</param>
    /// <remarks>
    /// <para>
    /// Go's nil func inside an interface is a VALUE with a dynamic type: <c>any((func())(nil))</c> is
    /// a NON-nil interface, <c>%T</c> prints <c>func()</c>, and <c>reflect.TypeOf</c> answers that
    /// func type rather than nil. A Go func lowers to a managed delegate whose nil IS <c>null</c>,
    /// which is correct in every func-typed slot and type-erasing in an interface.
    /// </para>
    /// <para>
    /// A CAST cannot carry the type across that boundary the way the pointer box does: <c>ж&lt;T&gt;</c>
    /// is a value that holds its pointee type, so <c>(object)((ж&lt;T&gt;)nil)</c> keeps it, whereas
    /// <c>(object)(Action)null</c> is simply <c>null</c> — the cast is erased at the box. That is why
    /// the pointer boundary needs only a null-coalesce to a canonical box while the func boundary
    /// needs a carrier, and why the variadic-func CAST the converter already emits pins the type only
    /// for a NON-null value.
    /// </para>
    /// <para>
    /// The converter emits this at every func-into-<c>any</c> site whose value can be null (the
    /// <c>TypedNilFuncAccessor</c> symbol); a func literal or method group is never null and takes
    /// nothing. A NON-empty interface target needs it not — no Go interface but the empty one can be
    /// satisfied by a bare func type, since a func type has no methods.
    /// </para>
    /// </remarks>
    public static object OrTypedNilFunc<TDelegate>(this TDelegate? value)
        where TDelegate : Delegate
    {
        // The canonical instance, never a fresh carrier — two typed nils of one func type must
        // compare reference-equal wherever the comparison is an untyped object reference compare,
        // exactly as ж<T>.NilBox does for pointers.
        return (object?)value ?? GoReflect.CanonicalNilFunc(typeof(TDelegate));
    }
}
