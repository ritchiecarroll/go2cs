// IArrayViewTypeTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

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
    public static string Generate(string objectName, string typeName, string targetTypeName) =>
        $$"""

                // View over the underlying array wrapper: first touches m_value.Value ON THE FIELD
                // (mutable — no defensive copy), materializing the lazily-allocated backing in THIS
                // wrapper's own storage; the returned VALUE copy then shares that heap T[] by
                // reference, so element refs taken through it land in the real storage (a struct
                // member cannot ref-return its own field — CS8170 — and does not need to).
                private {{typeName}} view
                {
                    get
                    {
                        _ = m_value.Value;
                        return m_value;
                    }
                }

                public {{targetTypeName}}[] Source => view.Source;

                public nint Length => view.Length;

                global::System.Array IArray.Source => ((IArray)view).Source!;

                object? IArray.this[nint index]
                {
                    get => ((IArray)view)[index];
                    set => ((IArray)view)[index] = value;
                }

                public ref {{targetTypeName}} this[nint index] => ref view[index];

                public ref {{targetTypeName}} this[int index] => ref view[(nint)index];

                public ref {{targetTypeName}} this[ulong index] => ref view[(nint)index];

                public global::System.Span<{{targetTypeName}}> {{EllipsisOperator}} => ToSpan();

                public global::System.Span<{{targetTypeName}}> ToSpan() => view.ToSpan();

                // Forwards the CONCRETE struct enumerator, not IEnumerator<(nint, T)> — see
                // IArrayTypeTemplate; the interface member becomes explicit so IArray<T>'s
                // IEnumerable<(nint, T)> is still satisfied on the boxing path.
                public global::go.array<{{targetTypeName}}>.Enumerator GetEnumerator() => view.GetEnumerator();

                global::System.Collections.Generic.IEnumerator<(nint, {{targetTypeName}})> global::System.Collections.Generic.IEnumerable<(nint, {{targetTypeName}})>.GetEnumerator() => ((global::System.Collections.Generic.IEnumerable<(nint, {{targetTypeName}})>)view).GetEnumerator();

                global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => ((global::System.Collections.IEnumerable)view).GetEnumerator();

                // Go's range-expression copy — see IArrayTypeTemplate.
                public global::go.array<{{targetTypeName}}>.RangeSnapshot {{RangeSnapshotMethod}}() => view.{{RangeSnapshotMethod}}();

                public {{objectName}} Clone() => new {{objectName}}(view.Clone());

                // Uniform value-copy member name a generated struct clone calls on every
                // clone-needing field (see GoValueCloneAttribute); copy SITES use Clone().
                public {{objectName}} {{ValueCloneMethod}}() => Clone();

                object global::System.ICloneable.Clone() => Clone();
        """;
}
