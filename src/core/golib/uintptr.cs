// uintptr.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Numerics;

namespace go;

/// <summary>
/// Represents the Go <c>uintptr</c> type: an unsigned integer wide enough to hold a pointer's bits.
/// </summary>
/// <remarks>
/// It is a distinct STRUCT rather than a compile-time alias of <see cref="UIntPtr"/> (<c>nuint</c>),
/// because an alias erases Go's type distinction between <c>uint</c> and <c>uintptr</c> — and the two
/// are DISTINCT types in Go: both may appear in one type switch, <c>%T</c> reports them differently,
/// and conversion between them is explicit. As a struct it keeps that identity: a boxed
/// <c>uintptr</c> has its own dynamic type, type-switch <c>case uintptr:</c> labels match exactly,
/// and overloads can distinguish the two.
/// <para>
/// The value is a pure NUMBER (single <c>nuint</c> field, no managed-reference slot): the
/// pointer-carrying job belongs to <c>unsafe.Pointer</c> and the managed-referent manual types.
/// The only user-defined conversions are the implicit bridge to/from <c>nuint</c> — every other
/// numeric conversion composes through C#'s standard conversions on the <c>nuint</c> side (an
/// explicit <c>(uintptr)intValue</c> or <c>(int)uintptrValue</c> chains a standard step with the
/// bridge). The full operator surface is declared so <c>uintptr</c>-typed expressions KEEP the
/// <c>uintptr</c> type (a bridge-only design would decay arithmetic results to <c>nuint</c>,
/// silently changing dynamic types in inference and boxing positions).
/// </para>
/// <para>
/// <see cref="Value"/> is deliberately a PUBLIC mutable field: atomic call sites
/// (<see cref="System.Threading.Interlocked"/>/<see cref="System.Threading.Volatile"/>, e.g. the
/// runtime's manual lock model in <c>lock_sema_impl.cs</c>) must target the inner storage —
/// <c>ref key.Value</c> — since the intrinsics cannot take a reference to a user struct.
/// </para>
/// </remarks>
public struct uintptr : IEquatable<uintptr>, IComparable<uintptr>, IComparable, IFormattable,
    // Generic-math interfaces so uintptr satisfies converter-emitted numeric union constraints
    // (a Go `~uint64 | ~uintptr`-style type set maps to this interface list) as a type ARGUMENT -
    // the operators below already exist; without the declarations an instantiation like reflect's
    // rangeNum<uintptr, uint64> is CS0315 x9 even though every member binds.
    IAdditionOperators<uintptr, uintptr, uintptr>, ISubtractionOperators<uintptr, uintptr, uintptr>,
    IMultiplyOperators<uintptr, uintptr, uintptr>, IDivisionOperators<uintptr, uintptr, uintptr>,
    IModulusOperators<uintptr, uintptr, uintptr>, IBitwiseOperators<uintptr, uintptr, uintptr>,
    IShiftOperators<uintptr, int, uintptr>, IEqualityOperators<uintptr, uintptr, bool>,
    IComparisonOperators<uintptr, uintptr, bool>, IIncrementOperators<uintptr>, IDecrementOperators<uintptr>,
    // Go defines unary `-x` on every numeric type as the wrap-around `0 - x`; the operator below
    // already implements it, and the lifted Arithmetic constraint set now demands the interface
    // (reflect's rangeNum<uintptr, uint64>, CS0315).
    IUnaryNegationOperators<uintptr, uintptr>,
    IGoEquatable
{
    // Pointer-width unsigned value of the struct 'uintptr' — public for Interlocked/Volatile seams
    public nuint Value;

    /// <summary>
    /// Creates a new <see cref="uintptr"/> from a <c>nuint</c> value.
    /// </summary>
    public uintptr(nuint value)
    {
        Value = value;
    }

    public bool Equals(uintptr other) => Value == other.Value;

    public override bool Equals(object? obj)
    {
        // Deliberately NO nuint arm: a boxed uintptr and a boxed uint (nuint) are DISTINCT Go
        // dynamic types — accepting nuint here re-erased the distinction asymmetrically
        // (adversarial review D4: a map[any] entry keyed uintptr was found by a uint probe).
        return obj is uintptr other && Equals(other);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public int CompareTo(uintptr other) => Value.CompareTo(other.Value);

    public int CompareTo(object? obj) => obj is uintptr other ? CompareTo(other) :
        throw new ArgumentException($"Object must be of type {nameof(uintptr)}");

    public override string ToString() => Value.ToString();

    public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);

    // Implicit bridge to/from 'nuint' (see remarks).
    public static implicit operator uintptr(nuint value) => new(value);

    public static implicit operator nuint(uintptr value) => value.Value;

    // Smaller unsigned sources get their own IMPLICIT operators (value-safe): although each
    // composes through the nuint bridge in implicit contexts, an EXPLICIT `(uintptr)u32` cast
    // sees uint32→long / uint32→ulong / uint32→nuint all as implicit-standard pre-hops to the
    // declared operators, and C# calls the long/ulong pair ambiguous (CS0457). An exact-source
    // operator wins resolution outright.
    public static implicit operator uintptr(uint8 value) => new(value);

    public static implicit operator uintptr(uint16 value) => new(value);

    public static implicit operator uintptr(uint32 value) => new(value);

    public static implicit operator uintptr(char value) => new(value);

    // A named untyped constant renders as the golib UntypedInt wrapper; Go assigns untyped
    // consts to uintptr freely, and two user-defined conversions never chain — bridge directly.
    public static implicit operator uintptr(UntypedInt value) => new((nuint)value);

    // INBOUND signed types and uint64 do NOT reach the implicit bridge in explicit casts (their
    // standard conversions to nuint are explicit, and Roslyn's candidate rules reject the chain
    // — `(uintptr)someInt` was CS0030 with the bridge alone). Go casts these constantly
    // (`uintptr(i)`), so declare exact-source operators (unchecked, Go semantics).
    public static explicit operator uintptr(int8 value) => new(unchecked((nuint)value));

    public static explicit operator uintptr(int16 value) => new(unchecked((nuint)value));

    public static explicit operator uintptr(int32 value) => new(unchecked((nuint)value));

    public static explicit operator uintptr(int64 value) => new(unchecked((nuint)value));

    public static explicit operator uintptr(nint value) => new(unchecked((nuint)value));

    public static explicit operator uintptr(uint64 value) => new(unchecked((nuint)value));

    // OUTBOUND casts get the FULL exact-target matrix. A partial set is unstable under C#'s
    // candidate rules: with only some targets declared, a cast to an undeclared unsigned target
    // finds MULTIPLE viable candidates through standard hops (`(uint)u` saw to-long vs to-ulong,
    // then to-long vs the bridge — CS0457), while signed targets have NO path through the
    // implicit bridge at all (no implicit relation with nuint — CS0030). An exact-target
    // operator wins resolution outright in every case, and emitted code casts uintptr to
    // everything (`(nint)(i)` element indexes, `(int)(x)` shift counts, `(uint64)p` hashes).
    public static explicit operator int8(uintptr value) => unchecked((int8)value.Value);

    public static explicit operator int16(uintptr value) => unchecked((int16)value.Value);

    public static explicit operator int32(uintptr value) => unchecked((int32)value.Value);

    public static explicit operator int64(uintptr value) => unchecked((int64)value.Value);

    public static explicit operator nint(uintptr value) => unchecked((nint)value.Value);

    public static explicit operator uint8(uintptr value) => unchecked((uint8)value.Value);

    public static explicit operator uint16(uintptr value) => unchecked((uint16)value.Value);

    public static explicit operator uint32(uintptr value) => unchecked((uint32)value.Value);

    public static explicit operator uint64(uintptr value) => value.Value;

    // Floating targets need exact operators too: double/float have implicit conversions FROM
    // every integer type, so every integer-returning operator above is a viable candidate for
    // `(float64)u` and the set ties (CS0457). Go converts uintptr→float in scavenger/pacer math.
    public static explicit operator float32(uintptr value) => value.Value;

    public static explicit operator float64(uintptr value) => value.Value;

    // Inbound floats symmetrically (`uintptr(someFloat)` is legal Go): with the inbound integer
    // set declared and no exact float source, resolution needs a most-encompassing source among
    // the integer params and none exists (long ⇹ ulong) — CS0457 (adversarial review D3).
    public static explicit operator uintptr(float32 value) => new(unchecked((nuint)value));

    public static explicit operator uintptr(float64 value) => new(unchecked((nuint)value));

    // Raw-pointer interop (unsafe seams in golib and emitted reinterprets)
    public static unsafe explicit operator uintptr(void* value) => new((nuint)value);

    public static unsafe explicit operator void*(uintptr value) => (void*)value.Value;

    // Comparisons
    // Go's == through an interface (builtin.AreEqual): this type's own operator, explicitly, so no member is added.
    bool IGoEquatable.GoEquals(object? other) => other is uintptr value && this == value;

    public static bool operator ==(uintptr left, uintptr right) => left.Value == right.Value;

    public static bool operator !=(uintptr left, uintptr right) => left.Value != right.Value;

    public static bool operator <(uintptr left, uintptr right) => left.Value < right.Value;

    public static bool operator <=(uintptr left, uintptr right) => left.Value <= right.Value;

    public static bool operator >(uintptr left, uintptr right) => left.Value > right.Value;

    public static bool operator >=(uintptr left, uintptr right) => left.Value >= right.Value;

    // Arithmetic — results stay 'uintptr'-typed (Go semantics; unchecked wraparound like Go)
    public static uintptr operator +(uintptr left, uintptr right) => new(unchecked(left.Value + right.Value));

    public static uintptr operator -(uintptr left, uintptr right) => new(unchecked(left.Value - right.Value));

    public static uintptr operator *(uintptr left, uintptr right) => new(unchecked(left.Value * right.Value));

    public static uintptr operator /(uintptr left, uintptr right) => new(left.Value / right.Value);

    public static uintptr operator %(uintptr left, uintptr right) => new(left.Value % right.Value);

    public static uintptr operator ++(uintptr value) => new(unchecked(value.Value + 1));

    public static uintptr operator --(uintptr value) => new(unchecked(value.Value - 1));

    // Unary forms for hand-written code hygiene: emitted code never contains `-u` (the
    // converter rewrites unsigned negation as `((T)0 - x)`), but without these a manual `-u`
    // is CS0035-ambiguous through the float bridges and `+u` silently decays to ulong.
    public static uintptr operator -(uintptr value) => new(unchecked((nuint)0 - value.Value));

    public static uintptr operator +(uintptr value) => value;

    // Bitwise
    public static uintptr operator &(uintptr left, uintptr right) => new(left.Value & right.Value);

    public static uintptr operator |(uintptr left, uintptr right) => new(left.Value | right.Value);

    public static uintptr operator ^(uintptr left, uintptr right) => new(left.Value ^ right.Value);

    public static uintptr operator ~(uintptr value) => new(~value.Value);

    public static uintptr operator <<(uintptr value, int shift) => new(value.Value << shift);

    public static uintptr operator >>(uintptr value, int shift) => new(value.Value >> shift);

    /// <summary>
    /// Shifts the bits of a <see cref="uintptr"/> right by the specified count (unsigned - required by
    /// <see cref="IShiftOperators{TSelf, TOther, TResult}"/>; identical to <c>&gt;&gt;</c> on an unsigned value).
    /// </summary>
    public static uintptr operator >>>(uintptr value, int shift) => new(value.Value >>> shift);

    // Handle comparisons between 'nil' and struct 'uintptr' (Go nil comparison idiom)
    public static bool operator ==(uintptr value, NilType nil) => value.Value == 0;

    public static bool operator !=(uintptr value, NilType nil) => !(value == nil);

    public static bool operator ==(NilType nil, uintptr value) => value == nil;

    public static bool operator !=(NilType nil, uintptr value) => value != nil;

    public static implicit operator uintptr(NilType nil) => default;
}
