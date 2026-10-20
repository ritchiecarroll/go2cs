// IArrayTypeTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using static go2cs.Symbols;

namespace go2cs.Templates.InheritedType;

internal static class IArrayTypeTemplate
{
    // The by-value copy members. A named array whose Go type declares its OWN `Clone` method (Go 1.26
    // hash/fnv's sum128: `Clone() (hash.Cloner, error)`) gets no public Clone(): an instance member is
    // bound before the extension method Go's Clone becomes, so it shadowed Go's method and RecvGenerator's
    // forwarder `return d.Clone();` bound the array copy (CS0029). Such a type copies through
    // ValueCloneMethod alone, the spelling the converter then uses at its copy sites (valueCloneSuffix).
    // goMethods are the Go methods declared on the type (InheritedTypeTemplate.GoMethodNames), as the
    // channel and map templates read them.
    private static string CloneMembers(string structName, ICollection<string> goMethods) =>
        goMethods.Contains("Clone") ?
            $$"""
            // Go by-value copy. `Clone` is the Go type's own method, so the copy is
                    // {{ValueCloneMethod}} only (see GoValueCloneAttribute).
                    public {{structName}} {{ValueCloneMethod}}() => new {{structName}}(Value.Clone());

                    object global::System.ICloneable.Clone() => {{ValueCloneMethod}}();
            """ :
            $$"""
            public {{structName}} Clone() => new {{structName}}(Value.Clone());

                    // Uniform value-copy member name a generated struct clone calls on every
                    // clone-needing field (see GoValueCloneAttribute); copy SITES use Clone().
                    public {{structName}} {{ValueCloneMethod}}() => Clone();

                    object global::System.ICloneable.Clone() => Clone();
            """;

    public static string Generate(string structName, string typeName, string targetTypeName, string? targetTypeSize, ICollection<string> goMethods) =>
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
            
                // An unsigned index forwards UNNARROWED: array<T>'s ulong indexer checks it first.
                public ref {{targetTypeName}} this[ulong index] => ref Value[index];

                public slice<{{targetTypeName}}> this[global::System.Range range] => Value[range];

                // The same sub-slice when a bound is not a constant that fits int32: golib's sentinel-free
                // array<T> pair, checked against the array's length as Go does (S-c R1-A).
                public slice<{{targetTypeName}}> slice(nint low) => Value.slice(low);

                public slice<{{targetTypeName}}> slice(nint low, nint high) => Value.slice(low, high);

                // Go's 3-index `x[lo:hi:max]`: golib's array<T> overload, reached on the wrapper itself the way
                // the 2-index forms are (a pointer to this type is sliced as `p.Value.slice(…)`).
                public slice<{{targetTypeName}}> slice(nint low, nint high, nint max) => Value.slice(low, high, max);

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

                {{CloneMembers(structName, goMethods)}}

                public static {{structName}} Make(nint p1 = 0, nint p2 = -1) => new {{structName}}();
        """;
}
