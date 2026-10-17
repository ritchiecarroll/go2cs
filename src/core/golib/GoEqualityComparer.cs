// GoEqualityComparer.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace go;

/// <summary>
/// Projects Go value equality — <see cref="builtin.AreEqual(object?, object?)"/> — as an
/// <see cref="IEqualityComparer{T}"/>, for the containers that compare keys themselves.
/// </summary>
/// <typeparam name="T">Key type; an interface type or <see cref="object"/> (see <see cref="GoEqualityComparer.ForKeys{T}"/>).</typeparam>
/// <remarks>
/// <para>
/// Go compares interface values by (dynamic type, dynamic value), and that ONE relation serves both
/// <c>==</c> and map-key lookup — a Go map keyed by an interface finds an entry under exactly the
/// values <c>==</c> calls equal. In the conversion those two had diverged: emitted <c>==</c> routes
/// through <see cref="builtin.AreEqual(object?, object?)"/>, which unwraps the generated adapters, while
/// <c>map&lt;TKey, TValue&gt;</c>'s backing <see cref="Dictionary{TKey, TValue}"/> used the DEFAULT
/// comparer and compared the wrappers.
/// </para>
/// <para>
/// That gap is observable because an interface value's wrapper is not stable: the same Go dynamic value
/// is presented through whichever adapter the static interface it is currently held in calls for, so
/// asserting an <c>Object</c> to a narrower <c>dependency</c> yields a DIFFERENT wrapper object over the
/// same receiver box. Under the default comparer the asserted value could no longer find its own entry
/// in the map it came out of. That is the shape <c>go/types</c>' <c>initorder.dependencyGraph</c> is
/// built on — <c>M[dependency]</c> assembled from the keys of <c>objMap[Object]</c>, then <c>objMap</c>
/// indexed with the asserted value — where the missed lookup returned a nil <c>*declInfo</c> and the next
/// field access nil-panicked the whole type checker. Guarded by the <c>InterfaceAssertionMapKey</c>
/// behavioral test.
/// </para>
/// <para>
/// Deliberately delegating to <see cref="builtin.AreEqual(object?, object?)"/> rather than restating the
/// relation: that method is golib's single definition of Go equality, and it already carries the three
/// adapter tiers, the dynamic-type check and the IEEE-754 float rule. A second copy here is exactly the
/// drift this fix exists to remove.
/// </para>
/// </remarks>
internal sealed class GoEqualityComparer<T> : IEqualityComparer<T>
{
    /// <summary>Gets the singleton comparer for <typeparamref name="T"/>.</summary>
    public static readonly GoEqualityComparer<T> Default = new();

    private GoEqualityComparer()
    {
    }

    /// <inheritdoc />
    public bool Equals(T? x, T? y)
    {
        object? left = GoEqualityComparer.RootOf(x);
        object? right = GoEqualityComparer.RootOf(y);

        // Unwrapping is idempotent, so handing AreEqual the roots asks it exactly the question it
        // would have asked itself. The identity leg short-circuits the reflective operator lookup
        // for the dominant case — a key probed against itself — and cannot change an answer: two
        // references to one instance are equal under every tier below it.
        return ReferenceEquals(left, right) || builtin.AreEqual(left, right);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Hashes the UNWRAPPED root, which is what keeps the hash consistent with <see cref="Equals"/>:
    /// equal keys unwrap to values of one dynamic type that the type's own <c>==</c>/<c>Equals</c>
    /// calls equal, so they hash alike. It is also the rule the compile-time adapters
    /// <c>ImplementGenerator</c> emits already use (<c>m_box.GetHashCode()</c>) — the runtime shells
    /// simply never had it.
    /// </remarks>
    public int GetHashCode(T obj)
    {
        object? root = GoEqualityComparer.RootOf(obj);

        // RootOf's own null guard (its doc comment: "keeps an adapter over a nil named-func
        // delegate in its wrapper view") means a nil-wrapped map key reaches here as the SHELL,
        // still not the null. Left unguarded, the shell's generated GetHashCode override calls
        // m_value.GetHashCode() on a null m_value and crashes — Go's real answer for hashing an
        // unhashable dynamic value is a panic too, but a SPECIFIC one ("hash of unhashable type
        // X", verified against go1.23.12: storing a nil-wrapped named-func-type value as a map
        // key panics exactly this way). Rhymes with the registererr chip's Defect B
        // (reflectPointerToken): the same null-Value shell reaching a path built only for the
        // non-null shape.
        if (root is IValueAdapter { Value: null } && GoReflect.ValueAdapterWrappedType(root.GetType()) is { } wrapped)
            throw new PanicException($"runtime error: hash of unhashable type {builtin.GetGoTypeName(wrapped)}");

        // Go hashes an interface key's DYNAMIC value, and a slice, map or func there is unhashable:
        // every map operation panics rather than answering (runtime.TestEmptyMapWithInterfaceKey).
        // The BCL would hash the slice/map struct's fields, or the delegate, and carry on.
        GoEqualityComparer.CheckHashableRoot(root);

        return root?.GetHashCode() ?? 0;
    }
}

/// <summary>
/// Non-generic helpers for <see cref="GoEqualityComparer{T}"/>.
/// </summary>
internal static class GoEqualityComparer
{
    /// <summary>
    /// Gets the Go equality comparer to back a map keyed by <typeparamref name="T"/>, or <c>null</c> to
    /// keep <see cref="Dictionary{TKey, TValue}"/>'s default.
    /// </summary>
    /// <typeparam name="T">Map key type.</typeparam>
    /// <remarks>
    /// Scoped to the key types that can actually CARRY an adapter — an interface, or <c>any</c>. A
    /// concrete key (<c>@string</c>, an integer, a converted struct) is never wrapped, so it would
    /// answer identically while giving up <see cref="EqualityComparer{T}"/>'s devirtualized fast path;
    /// the test is a JIT-time constant per instantiation, so the choice costs nothing at run time.
    /// </remarks>
    public static IEqualityComparer<T>? ForKeys<T>()
    {
        if (typeof(T).IsInterface || typeof(T) == typeof(object))
            return GoEqualityComparer<T>.Default;

        // A FLOAT-kinded key needs Go's IEEE rule, which Dictionary's default comparer deliberately
        // does NOT use: BCL `Double.Equals` reports NaN equal to NaN so that a NaN stored in a
        // collection can be found again, while Go's map applies `==` unchanged — so a NaN key is
        // never equal to anything, INCLUDING an existing NaN key. `m[NaN] = 1` twice therefore
        // stores TWO entries in Go and one here, and in Go neither can ever be read back or deleted.
        // fmt's own TestSprintf reads that difference out: `%v` of `map[float64]int{NaN: 1, NaN: 1}`
        // printed `map[NaN:1]` against Go's `map[NaN:1 NaN:1]`.
        //
        // The comparers below are per-representation and non-boxing, so an ordinary float-keyed map
        // keeps a direct call where routing it through the interface arm above would box every
        // probe. `==` is the whole implementation, because C#'s float `==` IS the IEEE relation Go's
        // map applies; the hash stays the type's own, and a NaN that hashes consistently while
        // comparing unequal builds exactly the same-bucket/never-equal chain Go's map builds for it.
        //
        // Scoped to the raw representations, with the residual stated rather than covered
        // speculatively (the r39d rule): a NAMED float type's wrapper, and a struct or array that
        // CONTAINS a float, still compare through their generated equality, which inherits the BCL
        // rule. No measured consumer reaches those, and covering them would mean routing every
        // struct-keyed map through the reflective relation.
        if (typeof(T) == typeof(double))
            return (IEqualityComparer<T>)(object)s_float64Keys;

        if (typeof(T) == typeof(float))
            return (IEqualityComparer<T>)(object)s_float32Keys;

        if (typeof(T) == typeof(System.Numerics.Complex))
            return (IEqualityComparer<T>)(object)s_complex128Keys;

        if (typeof(T) == typeof(complex64))
            return (IEqualityComparer<T>)(object)s_complex64Keys;

        return null;
    }

    private static readonly Float64KeyComparer s_float64Keys = new();
    private static readonly Float32KeyComparer s_float32Keys = new();
    private static readonly Complex128KeyComparer s_complex128Keys = new();
    private static readonly Complex64KeyComparer s_complex64Keys = new();

    private sealed class Float64KeyComparer : IEqualityComparer<double>
    {
        public bool Equals(double x, double y) => x == y;

        public int GetHashCode(double value) => value.GetHashCode();
    }

    private sealed class Float32KeyComparer : IEqualityComparer<float>
    {
        public bool Equals(float x, float y) => x == y;

        public int GetHashCode(float value) => value.GetHashCode();
    }

    private sealed class Complex128KeyComparer : IEqualityComparer<System.Numerics.Complex>
    {
        public bool Equals(System.Numerics.Complex x, System.Numerics.Complex y) => x == y;

        public int GetHashCode(System.Numerics.Complex value) => value.GetHashCode();
    }

    private sealed class Complex64KeyComparer : IEqualityComparer<complex64>
    {
        public bool Equals(complex64 x, complex64 y) => x == y;

        public int GetHashCode(complex64 value) => value.GetHashCode();
    }

    /// <summary>
    /// Unwraps the generated adapter tiers to the Go dynamic value beneath them.
    /// </summary>
    /// <param name="value">Value to unwrap.</param>
    /// <remarks>
    /// Mirrors the unwrap sequence at the head of <see cref="builtin.AreEqual(object?, object?)"/> —
    /// interface adapters to exhaustion, then the pointer tier, then the value tier — including its
    /// null guard on <see cref="IValueAdapter.Value"/>, which keeps an adapter over a nil named-func
    /// delegate in its wrapper view exactly as Go keeps such an interface non-nil.
    /// </remarks>
    public static object? RootOf(object? value)
    {
        while (value is IInterfaceAdapter interfaceAdapter)
            value = interfaceAdapter.Value;

        if (value is IжAdapter pointerAdapter)
            value = pointerAdapter.Box;

        if (value is IValueAdapter { Value: not null } valueAdapter)
            value = valueAdapter.Value;

        return value;
    }

    private static readonly ConcurrentDictionary<Type, Type?> s_unhashableTypes = new();
    private static readonly ConcurrentDictionary<Type, bool> s_signedZeroTypes = new();

    /// <summary>
    /// Panics with Go's text when an interface key's dynamic value cannot be hashed, on the paths where
    /// Go refuses the key WITHOUT hashing it: a lookup or delete on an EMPTY or NIL map, which is where a
    /// Dictionary never hashes at all and so needs the explicit call (mapKeyError on the Used() == 0 path).
    /// </summary>
    /// <param name="key">The key, adapters and all.</param>
    /// <remarks>
    /// Go 1.26 raises <c>maps.unhashableTypeError</c> here (<c>hash of unhashable type: T</c>, no
    /// <c>runtime error: </c> prefix; see <see cref="go.golib.RuntimeErrorPanic.UnhashableKey"/>), where Go 1.24
    /// raised the prefixed <c>errorString</c>. The hash path keeps <c>errorString</c> in both releases
    /// and is <see cref="CheckHashableRoot"/>.
    /// </remarks>
    public static void CheckHashable(object? key)
    {
        if (UnhashableTypeOf(RootOf(key)) is { } unhashable)
            throw go.golib.RuntimeErrorPanic.UnhashableKey(unhashable);
    }

    /// <summary>
    /// Panics with Go's text when the HASH of an interface key's dynamic value is refused: runtime's
    /// <c>typehash</c>/<c>nilinterhash</c>, which a populated map's lookup, delete and every assignment
    /// reach. The value is <c>runtime.errorString</c>, so the text keeps its <c>runtime error: </c> prefix
    /// in Go 1.24 and 1.26 alike.
    /// </summary>
    internal static void CheckHashableRoot(object? root)
    {
        if (UnhashableTypeOf(root) is { } unhashable)
            throw new PanicException($"runtime error: hash of unhashable type {GoReflect.GoTypeName(unhashable)}");
    }

    private static Type? UnhashableTypeOf(object? root)
    {
        // The dominant dynamic types are hashable and answer without the per-type cache.
        if (root is null or IConvertible or @string)
            return null;

        return s_unhashableTypes.GetOrAdd(root.GetType(), static t => unhashableWithin(t, 0));
    }

    // The type Go's mapKeyError names: the slice, map or func itself, found by recursing through
    // struct fields and array elements -- so `struct{ s []int }` reports `[]int`, as Go does. An
    // interface-typed FIELD is not followed (its dynamic value is not in the type); Go would hash
    // it, and that residual is stated rather than walked by value here.
    private static Type? unhashableWithin(Type type, int depth)
    {
        if (depth > 64)
            return null;

        switch (GoReflect.KindOf(type))
        {
            case GoReflect.Slice:
            case GoReflect.Map:
            case GoReflect.Func:
                return type;
            case GoReflect.Array:
                return GoReflect.ElementType(type) is { } element ? unhashableWithin(element, depth + 1) : null;
            case GoReflect.Struct:
                foreach (GoReflect.GoFieldInfo field in GoReflect.GoFields(type))
                {
                    if (unhashableWithin(field.Type, depth + 1) is { } unhashable)
                        return unhashable;
                }

                return null;
            default:
                return null;
        }
    }

    /// <summary>
    /// Reports whether a key of <paramref name="keyType"/> can hold a SIGNED ZERO -- the one case
    /// where two keys Go's <c>==</c> calls equal are still distinguishable, so an overwrite must
    /// replace the stored key (Go's NeedKeyUpdate; strings are also in Go's set, but no Go program
    /// can observe which of two equal strings a map kept).
    /// </summary>
    /// <param name="keyType">A map key type.</param>
    public static bool MayHoldSignedZero(Type keyType) =>
        s_signedZeroTypes.GetOrAdd(keyType, static t => signedZeroWithin(t, 0));

    private static bool signedZeroWithin(Type type, int depth)
    {
        if (depth > 64)
            return false;

        switch (GoReflect.KindOf(type))
        {
            case GoReflect.Float32:
            case GoReflect.Float64:
            case GoReflect.Complex64:
            case GoReflect.Complex128:
            case GoReflect.Interface:
                return true;
            case GoReflect.Array:
                return GoReflect.ElementType(type) is { } element && signedZeroWithin(element, depth + 1);
            case GoReflect.Struct:
                foreach (GoReflect.GoFieldInfo field in GoReflect.GoFields(type))
                {
                    if (signedZeroWithin(field.Type, depth + 1))
                        return true;
                }

                return false;
            default:
                return false;
        }
    }

    /// <summary>
    /// Reports whether a key VALUE may carry a zero whose sign an overwrite has to preserve. Exact for
    /// a raw float or complex; conservative (true) for a struct, an array or a named float, which
    /// only costs such a key an extra remove-and-add on overwrite.
    /// </summary>
    /// <param name="key">The key, adapters and all.</param>
    public static bool KeyMayCarryZero(object? key)
    {
        // The raw dynamic values an interface key overwhelmingly holds, before any adapter unwrap.
        switch (key)
        {
            case null:
                return false;
            case double value:
                return value == 0;
            case @string:
                return false;
        }

        object? root = RootOf(key);

        return root switch
        {
            null => false,
            double value => value == 0,
            float value => value == 0,
            System.Numerics.Complex value => value.Real == 0 || value.Imaginary == 0,
            complex64 value => value.Real == 0 || value.Imaginary == 0,
            IConvertible or @string => false,
            _ => MayHoldSignedZero(root.GetType())
        };
    }
}

public static partial class GoReflect
{
    /// <summary>
    /// Panics with Go's "hash of unhashable type" text when a map key's dynamic value cannot be
    /// hashed -- for reflect's nil-map arms, which answer without reaching the map at all.
    /// </summary>
    /// <param name="key">The marshalled key.</param>
    public static void CheckMapKeyHashable(object? key) => GoEqualityComparer.CheckHashable(key);
}
