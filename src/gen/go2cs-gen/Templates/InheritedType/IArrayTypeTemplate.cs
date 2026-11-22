// IArrayTypeTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using static go2cs.Common;
using static go2cs.Symbols;

namespace go2cs.Templates.InheritedType;

internal static class IArrayTypeTemplate
{
    // The wrapper's underlying-value property: `Value`, or NamedWrapperValue for a type itself named Value
    // (CS0542; Common.WrapperValueMember).
    private static string ValueMember(string structName) => WrapperValueMember(structName);

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
                    public {{structName}} {{ValueCloneMethod}}() => new {{structName}}({{ValueMember(structName)}}.Clone());

                    object global::System.ICloneable.Clone() => {{ValueCloneMethod}}();
            """ :
            $$"""
            public {{structName}} Clone() => new {{structName}}({{ValueMember(structName)}}.Clone());

                    // Uniform value-copy member name a generated struct clone calls on every
                    // clone-needing field (see GoValueCloneAttribute); copy SITES use Clone().
                    public {{structName}} {{ValueCloneMethod}}() => Clone();

                    object global::System.ICloneable.Clone() => Clone();
            """;

    // Go's `a = b` on an EXISTING location of the named array type copies b's elements into a's own storage, so what
    // was taken from a beforehand still names a (docs/phase4/DESIGN-array-reassign-in-place.md; golib's
    // ArrayExtensions.Assign is the array<T> half). The Value getter materializes the backing, so the copy always has
    // storage to land in, and the wrapper is never replaced. A Go type that declares its OWN Assign method gets
    // ΔAssign instead, so the instance member cannot shadow Go's method (the Clone case above); the converter spells
    // the call the same way (arrayAssignMethod).
    // The explicit IGoValueAssign member is what a container's per-element walk and reflect's store call (golib's
    // IGoValueAssign): the copy of the wrapper shares its published backing, so the elements land in place.
    private static string AssignMembers(string structName, ICollection<string> goMethods) =>
        $$"""
        public void {{AssignMethod(goMethods)}}({{structName}} source)
                {
                    var target = {{ValueMember(structName)}};
                    target.Assign(source.{{ValueMember(structName)}});
                }

                {{structName}} global::go.golib.IGoValueAssign<{{structName}}>.AssignedFrom(in {{structName}} source)
                {
                    {{structName}} target = this;
                    target.{{AssignMethod(goMethods)}}(source);
                    return target;
                }
        """;

    internal static string AssignMethod(ICollection<string> goMethods) => goMethods.Contains("Assign") ? "ΔAssign" : "Assign";

    // The members this wrapper declares for its own use under an ASCII name a Go method can also declare (R7; the slice
    // template's ClaimableMembers says why a Go method of the same name must win). A claimed member moves to its explicit
    // golib interface form (IArray, IArray<T>, ISupportMake<TSelf>), or, for Slice, which no interface declares, to
    // ΔSlice. Clone and Assign keep their own rules above. GenTests' NamedSliceArrayClaimedMemberTests guards the list.
    internal static readonly string[] ClaimableMembers = ["Assign", "Clone", "GetEnumerator", "Length", "Make", "Slice", "Source", "ToSpan"];

    private static string Claimable(ICollection<string> goMethods, string name, string own, string claimed) =>
        goMethods.Contains(name) ? claimed : own;

    public static string Generate(string structName, string typeName, string targetTypeName, string? targetTypeSize, ICollection<string> goMethods) =>
        $$"""

                {{Claimable(goMethods, "Source",
                    $"public {targetTypeName}[] Source => {ValueMember(structName)};",
                    $"{targetTypeName}[] IArray<{targetTypeName}>.Source => {ValueMember(structName)};")}}

                {{Claimable(goMethods, "Length",
                    $"public nint Length => {ValueMember(structName)}.Length;",
                    $"nint IArray.Length => {ValueMember(structName)}.Length;")}}

                global::System.Array IArray.Source => ((IArray){{ValueMember(structName)}}).Source!;
                
                object? IArray.this[nint index]
                {
                    get => ((IArray){{ValueMember(structName)}})[index];
                    set => ((IArray){{ValueMember(structName)}})[index] = value;
                }
                    
                // These element accessors and the Value getter they call both inline, so an element read is
                // golib's bounds check and a load, as array<T>'s own indexers are.
                public ref {{targetTypeName}} this[nint index]
                {
                    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                    get => ref {{ValueMember(structName)}}[index];
                }
            
                public ref {{targetTypeName}} this[int index]
                {
                    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                    get => ref {{ValueMember(structName)}}[(nint)index];
                }
            
                // An unsigned index forwards UNNARROWED: array<T>'s ulong indexer checks it first.
                public ref {{targetTypeName}} this[ulong index] => ref {{ValueMember(structName)}}[index];

                public slice<{{targetTypeName}}> this[global::System.Range range] => {{ValueMember(structName)}}[range];

                // The same sub-slice when a bound is not a constant that fits int32: golib's sentinel-free
                // array<T> pair, checked against the array's length as Go does (S-c R1-A).
                public slice<{{targetTypeName}}> slice(nint low) => {{ValueMember(structName)}}.slice(low);

                public slice<{{targetTypeName}}> slice(nint low, nint high) => {{ValueMember(structName)}}.slice(low, high);

                // Go's 3-index `x[lo:hi:max]`: golib's array<T> overload, reached on the wrapper itself the way
                // the 2-index forms are (a pointer to this type is sliced as `p.Value.slice(…)`).
                public slice<{{targetTypeName}}> slice(nint low, nint high, nint max) => {{ValueMember(structName)}}.slice(low, high, max);

                public slice<{{targetTypeName}}> {{(goMethods.Contains("Slice") ? "ΔSlice" : "Slice")}}(nint start, nint length) => {{ValueMember(structName)}}.Slice(start, length);

                public global::System.Span<{{targetTypeName}}> {{EllipsisOperator}} => {{ValueMember(structName)}}.ToSpan();

                {{Claimable(goMethods, "ToSpan",
                    $"public global::System.Span<{targetTypeName}> ToSpan() => {ValueMember(structName)}.ToSpan();",
                    $"global::System.Span<{targetTypeName}> IArray<{targetTypeName}>.ToSpan() => {ValueMember(structName)}.ToSpan();")}}

                // Forwards the CONCRETE struct enumerator over the VALUES, as array<T> enumerates them:
                // `foreach` binds GetEnumerator by pattern, so a named array type enumerates with zero heap
                // traffic exactly like the array<T> it wraps. Go's range pairs are builtin.range(x). A Go method
                // named GetEnumerator takes it away; the explicit IEnumerable forms below remain.
                {{Claimable(goMethods, "GetEnumerator", $"public global::go.array<{targetTypeName}>.Enumerator GetEnumerator() => {ValueMember(structName)}.GetEnumerator();", "")}}

                global::System.Collections.Generic.IEnumerator<{{targetTypeName}}> global::System.Collections.Generic.IEnumerable<{{targetTypeName}}>.GetEnumerator() => ((global::System.Collections.Generic.IEnumerable<{{targetTypeName}}>){{ValueMember(structName)}}).GetEnumerator();

                global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => ((global::System.Collections.IEnumerable){{ValueMember(structName)}}).GetEnumerator();

                // Go's range-expression copy (see array<T>.{{RangeSnapshotMethod}}): a `for i, v := range r`
                // over a named array VALUE iterates a snapshot, and the snapshot is pooled rather than
                // allocated because it cannot outlive the loop.
                public global::go.array<{{targetTypeName}}>.RangeSnapshot {{RangeSnapshotMethod}}() => {{ValueMember(structName)}}.{{RangeSnapshotMethod}}();

                public bool Equals(IArray<{{targetTypeName}}>? other) => {{ValueMember(structName)}}.Equals(other);

                {{CloneMembers(structName, goMethods)}}

                {{AssignMembers(structName, goMethods)}}

                {{Claimable(goMethods, "Make",
                    $"public static {structName} Make(nint p1 = 0, nint p2 = -1) => new {structName}();",
                    $"static {structName} ISupportMake<{structName}>.Make(nint p1, nint p2) => new {structName}();")}}
        """;
}
