// IMapTypeTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go2cs.Templates.InheritedType;

internal static class IMapTypeTemplate
{
    // constructorName is structName without a generic type's parameters (a C# constructor never carries them).
    //
    // mapValue is the expression that reads the wrapper's map, and storeMap(made) the value the
    // capacity constructor stores into m_value. Both default to the inline field. A self-containing
    // map wrapper (InheritedTypeTemplate.HoldsMapInHolder) holds its map in a reference holder, reads it
    // through `Value` and stores a new holder; every other wrapper's emission is unchanged. The
    // indexer's setter writes through a LOCAL when mapValue is not the field: `Value[key] = v` assigns
    // into a property's return value (CS1612), while the copy shares the map's storage like the field.
    public static string Generate(string structName, string constructorName, string keyTypeName, string valueTypeName, string mapValue = "m_value", Func<string, string>? storeMap = null) =>
        GenerateBody(structName, constructorName, keyTypeName, valueTypeName, mapValue, (storeMap ?? (made => made))($"new map<{keyTypeName}, {valueTypeName}>(size)"),
            mapValue == "m_value" ? "set => m_value[key] = value;" : $"set {{ map<{keyTypeName}, {valueTypeName}> target = {mapValue}; target[key] = value; }}");

    private static string GenerateBody(string structName, string constructorName, string keyTypeName, string valueTypeName, string mapValue, string sizedMap, string indexerSetter) =>
        $$"""
        
                public nint Length => ((IMap){{mapValue}}).Length;
                
                public bool IsNil => ((IMap){{mapValue}}).IsNil;
                
                /// <summary>ISupportMake factory — a made named map wraps a made concrete map.</summary>
                public static {{structName}} Make(nint p1, nint p2) => new {{structName}}(map<{{keyTypeName}}, {{valueTypeName}}>.Make(p1, p2));

                /// <summary>Capacity form — `make(NamedMap, n)` emits `new NamedMap(n)` (socktest's Sockets).</summary>
                public {{constructorName}}(nint size) => m_value = {{sizedMap}};

                public int Count => {{mapValue}}.Count;
                
                /// <summary>
                /// READONLY, and that is what makes `f()[k] = v` legal — Go's own rule for a named
                /// map type, which IS a reference: `w.Header()[k] = v` is ordinary Go. A struct's
                /// indexer SET on an rvalue receiver is CS1612 ("cannot modify the return value …
                /// because it is not a variable") unless the member is readonly, because C# assumes
                /// the mutation would be lost to the temporary. Here nothing is lost: the setter
                /// writes through m_value, and m_value is a readonly field of golib's own `map` —
                /// itself a `readonly struct` wrapping the shared dictionary — so the write lands on
                /// storage the copy shares, exactly as Go's map header does. Marking the member
                /// readonly states that fact to the compiler; it changes no generated body.
                /// net/http's whole test suite sat behind this (`w.Header()[k] = v`, 6 sites).
                /// </summary>
                public readonly {{valueTypeName}} this[{{keyTypeName}} key]
                {
                    get => {{mapValue}}[key];
                    {{indexerSetter}}
                }
                
                public ({{valueTypeName}}, bool) this[{{keyTypeName}} key, bool _] => {{mapValue}}[key, _];

                /// <summary>Shaped-zero read — an element type whose Go zero carries run-time shape (a fixed-size array) takes its zero from the call site.</summary>
                public {{valueTypeName}} this[{{keyTypeName}} key, global::System.Func<{{valueTypeName}}> zero] => {{mapValue}}[key, zero];

                /// <summary>Comma-ok shaped-zero read.</summary>
                public ({{valueTypeName}}, bool) this[{{keyTypeName}} key, global::System.Func<{{valueTypeName}}> zero, bool _] => {{mapValue}}[key, zero, _];

                public void Add({{keyTypeName}} key, {{valueTypeName}} value) => {{mapValue}}.Add(key, value);

                /// <summary>
                /// The write a nested map assignment needs: `m[k1][k2] = v` assigns through `m[k1]`, an rvalue
                /// struct, where an indexer SETTER is CS1612, so the converter emits `m[k1].Set(k2, v)` -- a
                /// method call, which writes through to the shared store. golib's map has Set; a named map
                /// element needs it here too (it was CS1501). A Go method `Set(k K, v V)` with EXACTLY this
                /// key and value type would be shadowed by this member (C# prefers an instance method to the
                /// extension the method is emitted as), the same as for Add above; none exists in std, the
                /// behavioral corpus or go-cmp.
                /// </summary>
                public void Set({{keyTypeName}} key, {{valueTypeName}} value) => {{mapValue}}.Set(key, value);
                
                public bool Remove({{keyTypeName}} key) => {{mapValue}}.Remove(key);
                
                public void Clear() => {{mapValue}}.Clear();
                
                public bool TryGetValue({{keyTypeName}} key, out {{valueTypeName}} value) => {{mapValue}}.TryGetValue(key, out value);
                
                public bool ContainsKey({{keyTypeName}} key) => {{mapValue}}.ContainsKey(key);
                
                global::System.Collections.Generic.ICollection<{{keyTypeName}}> global::System.Collections.Generic.IDictionary<{{keyTypeName}}, {{valueTypeName}}>.Keys => ((global::System.Collections.Generic.IDictionary<{{keyTypeName}}, {{valueTypeName}}>){{mapValue}}).Keys;

                global::System.Collections.Generic.ICollection<{{valueTypeName}}> global::System.Collections.Generic.IDictionary<{{keyTypeName}}, {{valueTypeName}}>.Values => ((global::System.Collections.Generic.IDictionary<{{keyTypeName}}, {{valueTypeName}}>){{mapValue}}).Values;
                
                void global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>.Add(global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}> item) => ((global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>){{mapValue}}).Add(item);
                
                bool global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>.Contains(global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}> item) => ((global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>){{mapValue}}).Contains(item);
                
                void global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>.CopyTo(global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>[] array, int arrayIndex) => ((global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>){{mapValue}}).CopyTo(array, arrayIndex);
                
                bool global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>.Remove(global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}> item) => ((global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>){{mapValue}}).Remove(item);
                
                bool global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>.IsReadOnly => false;
                
                public global::System.Collections.Generic.IEnumerator<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>> GetEnumerator() => ((global::System.Collections.Generic.IEnumerable<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>){{mapValue}}).GetEnumerator();
                
                global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        """;
}
