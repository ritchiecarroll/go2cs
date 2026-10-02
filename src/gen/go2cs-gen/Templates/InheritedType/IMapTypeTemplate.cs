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
    public static string Generate(string structName, string constructorName, string keyTypeName, string valueTypeName) =>
        $$"""
        
                public nint Length => ((IMap)m_value).Length;
                
                public bool IsNil => ((IMap)m_value).IsNil;
                
                /// <summary>ISupportMake factory — a made named map wraps a made concrete map.</summary>
                public static {{structName}} Make(nint p1, nint p2) => new {{structName}}(map<{{keyTypeName}}, {{valueTypeName}}>.Make(p1, p2));

                /// <summary>Capacity form — `make(NamedMap, n)` emits `new NamedMap(n)` (socktest's Sockets).</summary>
                public {{constructorName}}(nint size) => m_value = new map<{{keyTypeName}}, {{valueTypeName}}>(size);

                public int Count => m_value.Count;
                
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
                    get => m_value[key];
                    set => m_value[key] = value;
                }
                
                public ({{valueTypeName}}, bool) this[{{keyTypeName}} key, bool _] => m_value[key, _];

                /// <summary>Shaped-zero read — an element type whose Go zero carries run-time shape (a fixed-size array) takes its zero from the call site.</summary>
                public {{valueTypeName}} this[{{keyTypeName}} key, global::System.Func<{{valueTypeName}}> zero] => m_value[key, zero];

                /// <summary>Comma-ok shaped-zero read.</summary>
                public ({{valueTypeName}}, bool) this[{{keyTypeName}} key, global::System.Func<{{valueTypeName}}> zero, bool _] => m_value[key, zero, _];

                public void Add({{keyTypeName}} key, {{valueTypeName}} value) => m_value.Add(key, value);
                
                public bool Remove({{keyTypeName}} key) => m_value.Remove(key);
                
                public void Clear() => m_value.Clear();
                
                public bool TryGetValue({{keyTypeName}} key, out {{valueTypeName}} value) => m_value.TryGetValue(key, out value);
                
                public bool ContainsKey({{keyTypeName}} key) => m_value.ContainsKey(key);
                
                global::System.Collections.Generic.ICollection<{{keyTypeName}}> global::System.Collections.Generic.IDictionary<{{keyTypeName}}, {{valueTypeName}}>.Keys => ((global::System.Collections.Generic.IDictionary<{{keyTypeName}}, {{valueTypeName}}>)m_value).Keys;

                global::System.Collections.Generic.ICollection<{{valueTypeName}}> global::System.Collections.Generic.IDictionary<{{keyTypeName}}, {{valueTypeName}}>.Values => ((global::System.Collections.Generic.IDictionary<{{keyTypeName}}, {{valueTypeName}}>)m_value).Values;
                
                void global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>.Add(global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}> item) => ((global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>)m_value).Add(item);
                
                bool global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>.Contains(global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}> item) => ((global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>)m_value).Contains(item);
                
                void global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>.CopyTo(global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>[] array, int arrayIndex) => ((global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>)m_value).CopyTo(array, arrayIndex);
                
                bool global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>.Remove(global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}> item) => ((global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>)m_value).Remove(item);
                
                bool global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>.IsReadOnly => false;
                
                public global::System.Collections.Generic.IEnumerator<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>> GetEnumerator() => ((global::System.Collections.Generic.IEnumerable<global::System.Collections.Generic.KeyValuePair<{{keyTypeName}}, {{valueTypeName}}>>)m_value).GetEnumerator();
                
                global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        """;
}
