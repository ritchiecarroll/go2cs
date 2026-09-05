// GoArrayDimsAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Carries the Go DIMENSION a descriptor must hand down through <c>Elem()</c>, outermost first
/// (<c>[4][8]byte</c> ⇒ <c>[GoArrayDims(4, 8)]</c>) — on a func PARAMETER, and on a struct FIELD
/// that reaches an array through a hop with no value and no initializer to read.
/// </summary>
/// <remarks>
/// <para>
/// A Go array's length is part of its TYPE, and it is the one part the managed emission cannot
/// carry: <c>[32]byte</c> renders as <see cref="array{T}"/> and C# has no const generic parameter
/// to hold the 32. Everywhere else the reflection bridge recovers it from a live source instead —
/// a value reveals its own length (<c>GoReflect.ArrayDimsOfValue</c>), and a struct FIELD recovers
/// it from the declaring type's zero instance, because the converter emits the dimension as a field
/// initializer (<c>= new(32)</c>) the generated parameterless constructor runs.
/// </para>
/// <para>
/// A func PARAMETER has neither: there is no value at a type-only position and no initializer to
/// read, and the emitted delegate type is a bare <c>Func&lt;array&lt;byte&gt;, bool&gt;</c> shared by
/// <c>func([32]byte) bool</c> and <c>func([64]byte) bool</c> alike. So the parameter position is
/// where the datum has to live. <c>GoReflect.FuncParamDims</c> reads it back off the delegate
/// INSTANCE (<c>Delegate.Method.GetParameters()</c>), which resolves for every shape go2cs emits —
/// a declared func used as a method group, a non-capturing lambda, a capturing lambda's
/// display-class method, and a natural-typed lambda — and <c>abi.TypeOf</c> stamps it as descriptor
/// cargo so <c>reflect.Type.In(i)</c> hands out an array type that knows its length.
/// </para>
/// <para>
/// A struct FIELD is the same position wherever its initializer cannot reach. The initializer route
/// above works for a field that IS an array, because the field's own zero value is the array; it
/// reaches nothing behind a POINTER (a nil <c>ж&lt;array&lt;T&gt;&gt;</c> has no pointee to measure)
/// and nothing inside a MAP (a nil map has no entry whose key or element could reveal a length). So
/// those two hops are stamped here instead, and the dims mean on a field exactly what they mean on a
/// descriptor: <b>what <c>Elem()</c> hands down</b> — the pointee's dims for a pointer of any depth,
/// the element's for a map. A map's KEY is the one accessor with no slot in that rule and carries
/// <see cref="GoMapKeyDimsAttribute"/> instead.
/// </para>
/// <para>
/// A named fixed-array TYPE is the third position, and it is the only one whose reader is the source
/// GENERATOR rather than the reflection bridge. <c>type nn [2][3]int</c> emits
/// <c>[GoType("[2]array&lt;nint&gt;")] partial struct nn;</c>, and go2cs-gen builds that wrapper's
/// backing lazily as <c>new array&lt;array&lt;nint&gt;&gt;(2)</c> — two elements of
/// <c>default(array&lt;nint&gt;)</c>, which is a LENGTH-ZERO array where Go has three zeroed ints.
/// The 3 is absent from the descriptor and cannot be recovered downstream from anything, because an
/// <see cref="array{T}"/>'s length is instance state and this site has no instance; so the converter
/// stamps the whole chain here, outermost first, and the generator builds an element factory from
/// everything after the first dimension. Same meaning as the other positions — the dims of what
/// <c>Elem()</c> hands down — reached one hop earlier, at construction rather than at description.
/// A NAMED element needs no stamp: its own wrapper allocates its own backing by this same route.
/// </para>
/// </remarks>
/// <para>
/// The dimensions are <b>64-bit</b>, because a Go array length is Go's <c>int</c> — 64-bit on a
/// 64-bit platform — and the standard library uses the full range. <c>runtime/vdso_linux.go</c>
/// declares <c>symstrings *[vdsoArrayMax]byte</c> with <c>vdsoArrayMax = 1&lt;&lt;50 - 1</c>: Go's
/// POINTER-TO-UNBOUNDED-ARRAY idiom, a type-level way to index arbitrary offsets off a pointer.
/// No such array is ever allocated — and none could be, on either runtime — but the DIMENSION is
/// part of the type, it is what <c>reflect.Type.Elem().Len()</c> answers, and this attribute is
/// the only place it survives for a field behind a pointer. A 32-bit carrier could not hold it:
/// the emission was a hard <c>CS1503</c> (<c>cannot convert from 'long' to 'int'</c>) the moment
/// anyone regenerated the linux corpus, which is how it was found.
/// </para>
/// <para>
/// <see cref="GoMapKeyDimsAttribute"/> is deliberately NOT widened with it. A map KEY is a VALUE
/// array, so a map whose key type had such a dimension could never hold an entry; the unbounded
/// idiom reaches the bridge only through a POINTER, which is this attribute's position, not that
/// one.
/// </para>
/// <para>
/// A DEFINED pointer-to-array type is the third position (increment E3 follow-up 7g): `type P *[0]byte`
/// emits as a go2cs-gen wrapper CLASS whose <c>[GoType("ж&lt;array&lt;byte&gt;&gt;")]</c> marker
/// spells the pointee's managed type and nothing of its length, so a NIL value (no pointee to
/// measure) and a live one (<c>[0]</c> read off the pointee) synthesized TWO descriptors of one Go
/// type -- reflect's TestConvert matrix saw <c>MyBytesArrayPtr0</c> twice. The converter stamps the
/// dims on the class itself and <c>abi.synthType</c> fills from the TYPE, the stamp DECIDING (a caller's
/// disagreeing dims are refused by name; <see cref="GoReflect.TypeStampedDims"/>);
/// <see cref="GoChanDirAttribute"/> is the twin for a defined channel type's direction.
/// </para>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class GoArrayDimsAttribute(params long[] dims) : Attribute
{
    /// <summary>The Go array dimensions, outermost first.</summary>
    public long[] Dims { get; } = dims;
}
