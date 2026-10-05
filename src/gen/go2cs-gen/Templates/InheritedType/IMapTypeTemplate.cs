// IMapTypeTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using static go2cs.Symbols;

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
    //
    // goMethods are the Go methods declared on the type, and its own name (InheritedTypeTemplate.
    // GoMethodNames). A member whose name one of them claims moves to its explicit IDictionary or
    // ICollection implementation, so the Go method -- an extension -- is what a call binds, and golib
    // still reaches the member through the interface; Set, which no interface declares, is dropped (its
    // door, MapWrapperSet, is always here).
    public static string Generate(string structName, string constructorName, string keyTypeName, string valueTypeName, ICollection<string> goMethods, string mapValue = "m_value", Func<string, string>? storeMap = null) =>
        GenerateBody(structName, constructorName, keyTypeName, valueTypeName, goMethods, mapValue, (storeMap ?? (made => made))($"new map<{keyTypeName}, {valueTypeName}>(size)"),
            mapValue == "m_value" ? "set => m_value[key] = value;" : $"set {{ map<{keyTypeName}, {valueTypeName}> target = {mapValue}; target[key] = value; }}");

    private static string GenerateBody(string structName, string constructorName, string keyTypeName, string valueTypeName, ICollection<string> goMethods, string mapValue, string sizedMap, string indexerSetter)
    {
        string dictionary = $"global::System.Collections.Generic.IDictionary<{keyTypeName}, {valueTypeName}>";
        string collection = $"global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<{keyTypeName}, {valueTypeName}>>";

        // The public form, or -- when a Go method claims the name -- the same member implemented explicitly.
        string Member(string name, string returnType, string interfaceName, string signature, string body) =>
            goMethods.Contains(name) ?
                $"{returnType} {interfaceName}.{name}({signature}) => {body};" :
                $"public {returnType} {name}({signature}) => {body};";

        string set = goMethods.Contains("Set") ? "" :
            $$"""

                /// <summary>The nested-map write under its plain name, for a map whose type declares no Go method Set.</summary>
                public void Set({{keyTypeName}} key, {{valueTypeName}} value) => {{mapValue}}.Set(key, value);
        """;

        return $$"""
        
                public nint Length => ((IMap){{mapValue}}).Length;
                
                public bool IsNil => ((IMap){{mapValue}}).IsNil;

                /// <summary>
                /// maps.Clone of a NAMED map is that named type (Go: `func Clone[M ~map[K]V](m M) M`, whose
                /// worker returns `clone(m).(M)`). golib's default CloneMap builds a plain map, so the
                /// assertion failed (logrus' `maps.Clone(entry.Data)` panicked "is map, not logrus.Fields").
                /// The copy reads this wrapper's map through the same expression every other member does.
                /// </summary>
                IMap IMap.CloneMap() => IsNil ? default({{structName}}) : new {{structName}}(new map<{{keyTypeName}}, {{valueTypeName}}>({{mapValue}}));

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

                {{Member("Add", "void", dictionary, $"{keyTypeName} key, {valueTypeName} value", $"{mapValue}.Add(key, value)")}}

                /// <summary>
                /// The write a nested map assignment needs: `m[k1][k2] = v` assigns through `m[k1]`, an rvalue
                /// struct, where an indexer SETTER is CS1612, so the converter emits a method call, which writes
                /// through to the shared store (a named map element was CS1501 without it). It is this door
                /// when the element's Go type declares a method Set -- the converter reads that off go/types --
                /// and the plain Set below otherwise, which this wrapper then drops: a Go method is an extension,
                /// and a same-named instance member would run in its place (objx's Map.Set, a selector path walk,
                /// became a one-key write). No Go identifier can spell this name.
                /// </summary>
                public void {{MapWrapperSet}}({{keyTypeName}} key, {{valueTypeName}} value) => {{mapValue}}.Set(key, value);
        {{set}}
                
                {{Member("Remove", "bool", dictionary, $"{keyTypeName} key", $"{mapValue}.Remove(key)")}}
                
                {{Member("Clear", "void", collection, "", $"{mapValue}.Clear()")}}
                
                {{Member("TryGetValue", "bool", dictionary, $"{keyTypeName} key, out {valueTypeName} value", $"{mapValue}.TryGetValue(key, out value)")}}
                
                {{Member("ContainsKey", "bool", dictionary, $"{keyTypeName} key", $"{mapValue}.ContainsKey(key)")}}
                
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
}
