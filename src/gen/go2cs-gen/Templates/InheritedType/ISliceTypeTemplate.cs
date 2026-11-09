// ISliceTypeTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using static go2cs.Symbols;

namespace go2cs.Templates.InheritedType;

internal static class ISliceTypeTemplate
{
    // A named []byte (`[]uint8` is the same Go type) satisfies Go's `~[]byte | ~string` union constraint, which the
    // converter emits as IByteSeq<TSelf, byte> (InheritedTypeTemplate declares the interface for it).
    public static bool IsByteSlice(string targetTypeName) => targetTypeName is "byte" or "uint8";

    // ICloneable's member. A named slice whose Go type declares its OWN `Clone` method (jsontext's `func (v Value)
    // Clone() Value`) implements it explicitly: an instance member is bound before the extension method Go's Clone
    // becomes, so a public `object Clone()` shadowed Go's method and `v.Clone()` returned an object. The named array
    // wrapper does the same (IArrayTypeTemplate.CloneMembers).
    private static string CloneMember(ICollection<string> goMethods) =>
        goMethods.Contains("Clone") ?
            "object global::System.ICloneable.Clone() => ((global::System.ICloneable)m_value).Clone();" :
            "public object Clone() => ((global::System.ICloneable)m_value).Clone();";

    // IByteSeq<TSelf, byte> for a named []byte, so it can be the type argument of a `~[]byte | ~string` parameter
    // (TS readiness C16). Length, the Range indexer and the two slice(...) members above implement it implicitly,
    // returning the named type as Go's sub-slice does. The element read and the read-only spread are explicit, as
    // golib's slice<T> declares them: the public indexer returns a ref and the public spread a writable span.
    // TryGetSlice hands out the wrapper's own window, so `[]byte(v)` and `Value(b)` share v's backing, as Go's do.
    private static string ByteSeqMembers(string targetTypeName) => !IsByteSlice(targetTypeName) ? "" :
        $$"""


                byte IByteSeq<byte>.this[nint index] => m_value[index];

                global::System.ReadOnlySpan<byte> IByteSeq<byte>.{{EllipsisOperator}} => ToSpan();

                bool IByteSeq<byte>.TryGetSlice(out slice<byte> window)
                {
                    window = m_value;
                    return true;
                }

                bool IByteSeq<byte>.TryGetString(out @string value)
                {
                    value = default;
                    return false;
                }
        """;

    // constructorName is structName without a generic type's parameters (a C# constructor never carries them).
    // goMethods are the Go methods declared on the type (InheritedTypeTemplate.GoMethodNames).
    public static string Generate(string structName, string constructorName, string typeName, string targetTypeName, ICollection<string> goMethods) =>
        $$"""
        
                public {{targetTypeName}}[] Source => m_value;

                /// <summary>Field-ref accessor for the ж<T>.of() pointer-reinterpret projection —
                /// `(*[][]byte)(buf)` emits `Ꮡbuf.of({{structName}}.Ꮡm_value)`, a true aliasing view.</summary>
                internal static ref slice<{{targetTypeName}}> Ꮡm_value(ref {{structName}} instance) => ref instance.m_value;

                /// <summary>ISliceWrap factory — wraps a window in this named slice type, sharing backing.</summary>
                public static {{structName}} Wrap(in slice<{{targetTypeName}}> source) => new {{structName}}(source);
                    
                public ISlice<{{targetTypeName}}> Append({{targetTypeName}}[] elems) => m_value.Append(elems);
                    
                public bool IsNil => ((ISlice)m_value).IsNil;

                public nint Low => ((ISlice)m_value).Low;
                
                public nint High => ((ISlice)m_value).High;
                
                public nint Capacity => ((ISlice)m_value).Capacity;
                
                public nint Available => ((ISlice)m_value).Available;
                
                public nint Length => ((IArray)m_value).Length;
                
                global::System.Array IArray.Source => ((IArray)m_value).Source!;
                
                object? IArray.this[nint index]
                {
                    get => ((IArray)m_value)[index];
                    set => ((IArray)m_value)[index] = value;
                }
                    
                public ref {{targetTypeName}} this[nint index] => ref m_value[index];

                public ref {{targetTypeName}} this[int index] => ref m_value[index];

                // An unsigned index forwards UNNARROWED: slice<T>'s ulong indexer checks it first.
                public ref {{targetTypeName}} this[ulong index] => ref m_value[index];
        
                // Slicing a named slice KEEPS the named type (Go: nat[a:b] IS a nat - math/big's
                // `u[s:].norm()` bound slice<Word> instead, CS1929 x21); the wrapper shares the
                // same backing window.
                public {{structName}} this[global::System.Range range] => new {{structName}}(m_value[range]);

                // The same sub-slice when a bound is not a constant that fits int32 (a C# Range cannot
                // carry a negative bound or one past int32): golib's sentinel-free pair, which panics as Go
                // does, still keeping the named type (S-c R1-A).
                public {{structName}} slice(nint low) => new {{structName}}(m_value.slice(low));

                public {{structName}} slice(nint low, nint high) => new {{structName}}(m_value.slice(low, high));

                ISlice<{{targetTypeName}}> ISlice<{{targetTypeName}}>.this[global::System.Range range] => m_value[range];
                
                public {{structName}} Slice(int start, int length) => new {{structName}}(m_value.Slice(start, length));
                
                ISlice<{{targetTypeName}}> ISlice<{{targetTypeName}}>.Slice(int start, int length) => m_value.Slice(start, length);
                
                public {{structName}} Slice(nint start, nint length) => new {{structName}}(m_value.Slice(start, length));
                
                ISlice<{{targetTypeName}}> ISlice<{{targetTypeName}}>.Slice(nint start, nint length) => m_value.Slice(start, length);
        
                public global::System.Span<{{targetTypeName}}> {{EllipsisOperator}} => ToSpan();

                public global::System.Span<{{targetTypeName}}> ToSpan() => m_value.ToSpan();

                // EXPLICIT, exactly as golib's own slice<T> declares it, and for the same reason
                // the enumerator below is explicit: this is the boxing, interface-typed path, and
                // the public surface is the ISlice<T> overload above. It also has to be — with
                // T = any (`type SE []any` in fmt's suite, `type fileOps []any` in archive/tar's)
                // `object[]` and `T[]` are the SAME parameter list, so two public Appends is
                // CS0111. That collided for every `[]any` named slice, which is why the two
                // packages sat behind one diagnostic.
                ISlice? ISlice.Append(object[] elems) => ((ISlice)m_value).Append(elems);

                // Forwards the CONCRETE struct enumerator over the VALUES, as slice<T> enumerates them:
                // `foreach` binds GetEnumerator by pattern, so a named slice type enumerates with zero heap
                // traffic exactly like the slice<T> it wraps. Go's range pairs are builtin.range(x).
                public global::go.slice<{{targetTypeName}}>.Enumerator GetEnumerator() => m_value.GetEnumerator();

                global::System.Collections.Generic.IEnumerator<{{targetTypeName}}> global::System.Collections.Generic.IEnumerable<{{targetTypeName}}>.GetEnumerator() => ((global::System.Collections.Generic.IEnumerable<{{targetTypeName}}>)m_value).GetEnumerator();

                global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => ((global::System.Collections.IEnumerable)m_value).GetEnumerator();

                public bool Equals(IArray<{{targetTypeName}}>? other) => m_value.Equals(other);

                public bool Equals(ISlice<{{targetTypeName}}>? other) => m_value.Equals(other);

                {{CloneMember(goMethods)}}{{ByteSeqMembers(targetTypeName)}}

                public static {{structName}} Make(nint p1 = 0, nint p2 = -1) => new {{structName}}(p1, p2);
        
                public {{constructorName}}(nint length, nint capacity = -1, nint low = 0) => m_value = new {{typeName}}(length, capacity, low);
        """;
}
