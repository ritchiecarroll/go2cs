// IArrayViewTypeTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using static go2cs.Common;
using static go2cs.Symbols;

namespace go2cs.Templates.InheritedType;

/// <summary>
/// Implements <c>IArray&lt;T&gt;</c> on a defined type whose underlying is ITSELF an array-backed
/// [GoType] wrapper — <c>type pallocBits pageBits</c> where <c>type pageBits [8]uint64</c>. Go code
/// calls <c>len(b)</c> / indexes such a type directly, which needs <c>IArray</c> on the WRAPPER
/// (golib <c>len(IArray)</c> — CS1503 otherwise; runtime mpallocbits.go).
/// </summary>
/// <remarks>
/// Every member delegates through the private ref accessor <c>view</c>, NOT the copying <c>Value</c>
/// property: the underlying array wrapper's backing is LAZILY allocated (<c>m_value ??= new …</c>),
/// so a zero-valued instance reached through a value copy would allocate on the copy and silently
/// drop writes (the historical pallocBits lost-writes trap). <c>view</c> first touches
/// <c>m_value.Value</c> — allocating the backing in THIS wrapper's own storage (<c>m_value</c> is
/// mutable for this branch) — then returns <c>ref m_value</c>, so element refs land in the real,
/// now-shared <c>T[]</c>. Subsequent struct copies (e.g. the <c>(pageBits)(b)</c> reinterpret
/// conversion) share that same backing array by reference.
/// </remarks>
internal static class IArrayViewTypeTemplate
{
    // The members this wrapper declares for its own use under an ASCII name a Go method can also declare (R7; see
    // ISliceTypeTemplate.ClaimableMembers): a claimed member moves to its explicit IArray / IArray<T> form, or to ΔSlice,
    // and a claimed Clone leaves the copy to ValueCloneMethod alone, the spelling the converter then uses at its copy sites
    // (valueCloneSuffix), exactly as IArrayTypeTemplate.CloneMembers does. GenTests' NamedSliceArrayClaimedMemberTests.
    internal static readonly string[] ClaimableMembers = ["Assign", "Clone", "GetEnumerator", "Length", "Slice", "Source", "ToSpan"];

    private static string Claimable(ICollection<string> goMethods, string name, string own, string claimed) =>
        goMethods.Contains(name) ? claimed : own;

    public static string Generate(string objectName, string typeName, string targetTypeName, ICollection<string> goMethods) =>
        $$"""

                // Every member below reads the underlying wrapper's backing through its Value (view.Value), never a member
                // by name: a Go method on the UNDERLYING named array takes that name (R7), so `view.Source` bound the
                // extension method Go's Source became (CS0428, NamedSliceMethodShadow's V over Vec). The copy goes through
                // the underlying's ValueCloneMethod, which every array and view wrapper declares.
                // View over the underlying array wrapper: first touches m_value.Value ON THE FIELD
                // (mutable — no defensive copy), materializing the lazily-allocated backing in THIS
                // wrapper's own storage; the returned VALUE copy then shares that heap T[] by
                // reference, so element refs taken through it land in the real storage (a struct
                // member cannot ref-return its own field — CS8170 — and does not need to).
                private {{typeName}} view
                {
                    get
                    {
                        _ = m_value.{{WrapperValueMember(typeName)}};
                        return m_value;
                    }
                }

                {{Claimable(goMethods, "Source", $"public {targetTypeName}[] Source => view.{WrapperValueMember(typeName)}.Source;", $"{targetTypeName}[] IArray<{targetTypeName}>.Source => view.{WrapperValueMember(typeName)}.Source;")}}

                {{Claimable(goMethods, "Length", $"public nint Length => view.{WrapperValueMember(typeName)}.Length;", $"nint IArray.Length => view.{WrapperValueMember(typeName)}.Length;")}}

                global::System.Array IArray.Source => ((IArray)view).Source!;

                object? IArray.this[nint index]
                {
                    get => ((IArray)view)[index];
                    set => ((IArray)view)[index] = value;
                }

                public ref {{targetTypeName}} this[nint index] => ref view[index];

                public ref {{targetTypeName}} this[int index] => ref view[(nint)index];

                // An unsigned index forwards UNNARROWED to the underlying wrapper's ulong indexer.
                public ref {{targetTypeName}} this[ulong index] => ref view[index];

                // Go's slice of the array, `x[:]` / `x[lo:hi]` (and `p[:]` through a pointer): the same members
                // IArrayTypeTemplate gives a type defined DIRECTLY over an array, forwarded through the view so the
                // result shares this wrapper's backing. A chain two or more definitions deep (runtime's
                // `type PallocBits pallocBits` over `type pallocBits pageBits`) had only the element indexers, so
                // `want[:][2]` read `[..]` against an nint indexer (CS1503 ×9, runtime's mpallocbits_test.go).
                public slice<{{targetTypeName}}> this[global::System.Range range] => view[range];

                // The same sub-slice when a bound is not a constant that fits int32 — see IArrayTypeTemplate.
                public slice<{{targetTypeName}}> slice(nint low) => view.slice(low);

                public slice<{{targetTypeName}}> slice(nint low, nint high) => view.slice(low, high);

                public slice<{{targetTypeName}}> slice(nint low, nint high, nint max) => view.slice(low, high, max);

                public slice<{{targetTypeName}}> {{(goMethods.Contains("Slice") ? "ΔSlice" : "Slice")}}(nint start, nint length) => view.{{WrapperValueMember(typeName)}}.Slice(start, length);

                public global::System.Span<{{targetTypeName}}> {{EllipsisOperator}} => view.{{WrapperValueMember(typeName)}}.ToSpan();

                {{Claimable(goMethods, "ToSpan",
                    $"public global::System.Span<{targetTypeName}> ToSpan() => view.{WrapperValueMember(typeName)}.ToSpan();",
                    $"global::System.Span<{targetTypeName}> IArray<{targetTypeName}>.ToSpan() => view.{WrapperValueMember(typeName)}.ToSpan();")}}

                // Forwards the CONCRETE struct enumerator over the VALUES — see IArrayTypeTemplate.
                {{Claimable(goMethods, "GetEnumerator", $"public global::go.array<{targetTypeName}>.Enumerator GetEnumerator() => view.{WrapperValueMember(typeName)}.GetEnumerator();", "")}}

                global::System.Collections.Generic.IEnumerator<{{targetTypeName}}> global::System.Collections.Generic.IEnumerable<{{targetTypeName}}>.GetEnumerator() => ((global::System.Collections.Generic.IEnumerable<{{targetTypeName}}>)view).GetEnumerator();

                global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => ((global::System.Collections.IEnumerable)view).GetEnumerator();

                // Go's range-expression copy — see IArrayTypeTemplate.
                public global::go.array<{{targetTypeName}}>.RangeSnapshot {{RangeSnapshotMethod}}() => view.{{RangeSnapshotMethod}}();

                {{(goMethods.Contains("Clone") ?
                    $"public {objectName} {ValueCloneMethod}() => new {objectName}(view.{ValueCloneMethod}());\r\n\r\n        object global::System.ICloneable.Clone() => {ValueCloneMethod}();" :
                    $"public {objectName} Clone() => new {objectName}(view.{ValueCloneMethod}());\r\n\r\n        // Uniform value-copy member name a generated struct clone calls on every\r\n        // clone-needing field (see GoValueCloneAttribute); copy SITES use Clone().\r\n        public {objectName} {ValueCloneMethod}() => Clone();\r\n\r\n        object global::System.ICloneable.Clone() => Clone();")}}

                // Go's whole assignment into this array's own storage, through `view` so the underlying wrapper's lazy
                // backing lands in THIS wrapper's field (see IArrayTypeTemplate's Assign; P5's Q2).
                public void {{IArrayTypeTemplate.AssignMethod(goMethods)}}({{objectName}} source)
                {
                    var target = view.{{WrapperValueMember(typeName)}};
                    target.Assign(source.view.{{WrapperValueMember(typeName)}});
                }

                {{objectName}} global::go.golib.IGoValueAssign<{{objectName}}>.AssignedFrom(in {{objectName}} source)
                {
                    {{objectName}} target = this;
                    target.{{IArrayTypeTemplate.AssignMethod(goMethods)}}(source);
                    return target;
                }
        """;
}
