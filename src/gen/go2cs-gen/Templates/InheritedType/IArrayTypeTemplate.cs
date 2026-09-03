// IArrayTypeTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using static go2cs.Symbols;

namespace go2cs.Templates.InheritedType;

internal static class IArrayTypeTemplate
{
    public static string Generate(string structName, string typeName, string targetTypeName, string? targetTypeSize) =>
        $$"""
                
                public {{targetTypeName}}[] Source => Value;
                
                public nint Length => Value.Length;
                
                global::System.Array IArray.Source => ((IArray)Value).Source!;
                
                object? IArray.this[nint index]
                {
                    get => ((IArray)Value)[index];
                    set => ((IArray)Value)[index] = value;
                }
                    
                public ref {{targetTypeName}} this[nint index] => ref Value[index];
            
                public ref {{targetTypeName}} this[int index] => ref Value[(nint)index];
            
                public ref {{targetTypeName}} this[ulong index] => ref Value[(nint)index];

                public slice<{{targetTypeName}}> this[global::System.Range range] => Value[range];

                public slice<{{targetTypeName}}> Slice(nint start, nint length) => Value.Slice(start, length);

                public global::System.Span<{{targetTypeName}}> {{EllipsisOperator}} => ToSpan();

                public global::System.Span<{{targetTypeName}}> ToSpan() => Value.ToSpan();

                // Forwards the CONCRETE struct enumerator, not IEnumerator<(nint, T)>: `foreach` binds
                // GetEnumerator by pattern, so a named array type ranges with zero heap traffic exactly
                // like the array<T> it wraps. Returning the interface here would box on every loop entry.
                // IArray<T> -> IEnumerable<(nint, T)> still needs the interface member, so it becomes
                // explicit — the boxing path, taken only when a consumer asks for the interface.
                public global::go.array<{{targetTypeName}}>.Enumerator GetEnumerator() => Value.GetEnumerator();

                global::System.Collections.Generic.IEnumerator<(nint, {{targetTypeName}})> global::System.Collections.Generic.IEnumerable<(nint, {{targetTypeName}})>.GetEnumerator() => ((global::System.Collections.Generic.IEnumerable<(nint, {{targetTypeName}})>)Value).GetEnumerator();

                global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => ((global::System.Collections.IEnumerable)Value).GetEnumerator();

                // Go's range-expression copy (see array<T>.{{RangeSnapshotMethod}}): a `for i, v := range r`
                // over a named array VALUE iterates a snapshot, and the snapshot is pooled rather than
                // allocated because it cannot outlive the loop.
                public global::go.array<{{targetTypeName}}>.RangeSnapshot {{RangeSnapshotMethod}}() => Value.{{RangeSnapshotMethod}}();

                public bool Equals(IArray<{{targetTypeName}}>? other) => Value.Equals(other);

                public {{structName}} Clone() => new {{structName}}(Value.Clone());

                // Uniform value-copy member name a generated struct clone calls on every
                // clone-needing field (see GoValueCloneAttribute); copy SITES use Clone().
                public {{structName}} {{ValueCloneMethod}}() => Clone();

                object global::System.ICloneable.Clone() => Clone();

                public static {{structName}} Make(nint p1 = 0, nint p2 = -1) => new {{structName}}();
        """;
}
