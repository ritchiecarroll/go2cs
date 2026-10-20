// TypedNilOf.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;

namespace go;

/// <summary>
/// The typed-nil boundary for a value of TYPE-PARAMETER type on its way into an empty interface (<c>any</c>): the
/// generic sibling of <see cref="PointerExtensions.OrTypedNil{T}"/> and <see cref="NilFuncExtensions.OrTypedNilFunc{TDelegate}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Go's nil pointer inside an interface is a VALUE with a dynamic type, and a type parameter does not change that:
/// in <c>func show[E any](e E) { fmt.Sprintf("%T", e) }</c>, <c>show[*P](nil)</c> prints <c>*main.P</c> and
/// <c>any(e) == nil</c> is false. The converter treats a pointer or func into <c>any</c> where it can see the type
/// (<c>.OrTypedNil()</c>), but inside a generic body the static type is <c>E</c>, so the null crossed bare and read as
/// Go's nil interface: errors_test's <c>testAsType[E compError]</c> named its subtests <c>AsType[&lt;nil&gt;]</c>
/// where Go names them <c>AsType[*fs.PathError]</c>.
/// </para>
/// <para>
/// The converter emits this at every empty-interface conversion of a value whose static type is a type parameter
/// whose type set can hold such a value. A NON-empty interface needs it not: converting a type parameter to one is
/// itself what keeps the parameter's C# constraint (methodSetConstraintNeed.go), and a constrained type argument is
/// never a bare golib pointer box, while the adapters a pointer reaches a non-empty interface through already
/// null-coalesce to the canonical typed nil.
/// </para>
/// <para>
/// Cost: the non-null path is one null test on the value (folded away by the JIT for a value-type <c>T</c>, where the
/// box is the one the conversion always made); the null path reads one static, decided once per <c>T</c>.
/// </para>
/// </remarks>
public static class TypedNilOfExtensions
{
    /// <summary>
    /// <paramref name="value"/>, or, when it is null, the canonical typed nil of <typeparamref name="T"/>: the pointer's
    /// <see cref="ж{T}.NilBox"/>, the func type's carrier, or <c>unsafe.Pointer</c>'s nil. An interface type argument (or
    /// any type with no typed nil) keeps the null, which is Go's nil interface.
    /// </summary>
    public static object? OrTypedNilOf<T>(this T value) => value is not null ? value : CanonicalNil<T>.Value;

    /// <summary>
    /// Installed by the hand-owned unsafe package: its canonical nil <c>unsafe.Pointer</c>, which golib cannot name.
    /// </summary>
    public static object? UnsafePointerNil { get; set; }

    // Decided once per T (AOT-safe: no MakeGenericType, only the existing typed-Type entry points).
    private static class CanonicalNil<T>
    {
        internal static readonly object? Value = compute(typeof(T));

        private static object? compute(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ж<>))
                return GoReflect.CanonicalNilPointer(type);

            if (typeof(Delegate).IsAssignableFrom(type))
                return GoReflect.CanonicalNilFunc(type);

            if (typeof(IUnsafePointer).IsAssignableFrom(type))
            {
                // The bridge is registered by the unsafe module's initializer, which a null value of its type need not
                // have run yet.
                RuntimeHelpers.RunModuleConstructor(type.Module.ModuleHandle);
                return UnsafePointerNil;
            }

            return null;
        }
    }
}
