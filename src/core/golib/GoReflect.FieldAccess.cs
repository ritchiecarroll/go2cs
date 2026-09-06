// GoReflect.FieldAccess.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using go.golib;
using static go2cs.Symbols;

namespace go;

// ---------------------------------------------------------------------------------------------
// FIELD ACCESS — reaching INSIDE a value, and handing back something writable.
//
// WHAT LIVES HERE
//   Reading and writing a pointer box's value slot; projecting a converted struct's Go-visible
//   FIELDS out of its C# fields; and minting the alias boxes that make `reflect.Value.Field(i)`,
//   `.Index(i)`, `.Slice(…)` and `.SetMapIndex(…)` write THROUGH to real storage.
//
// ADDRESSABILITY IS THE WHOLE PROBLEM
//   `reflect` in Go hands out an addressable `Value` for a field of an addressable struct, and a
//   write through it lands in the original. Boxed reflection in .NET gives you a COPY: read a
//   struct field with `FieldInfo.GetValue`, write it back, and you have written to a copy. So every
//   "alias box" here is a `ж<F>` built over a REF ACCESSOR — a DynamicMethod that walks
//   `ValueSlot` and `ldflda`s down the field path — rather than over a fetched value. That is the
//   contract: if a member in this file returns a box, a write through that box must be visible in
//   the parent, and any change that reintroduces a copy anywhere on the path silently breaks it.
//
//   The accessor is cached and DOUBLES AS THE POINTER-IDENTITY TOKEN, which is what preserves Go's
//   `&s.f == &s.f` — `ж` equality compares the source object and the field accessor delegate, so
//   the cache is load-bearing for correctness and not only for speed. `FieldRef<T>.Create` cannot
//   serve here: its IL hardcodes the box's own `m_val`, so it cannot reach a nested parent (a
//   field of a field, or a field of an element).
//
// THE `ValueSlot`-NOT-`Value` RULE, BOTH DIRECTIONS
//   Reads and writes both go through `ж<T>.ValueSlot`. Reads must not panic on a slot HOLDING a
//   nil value — Go's `*(&p)` where `p` is a nil `*T` yields nil, it does not dereference — and
//   writes must assign THROUGH the ref-returning property so they land in the real slot, alias
//   boxes included. A structurally nil box still panics on write, exactly like Go's nil-pointer
//   store, and the canonical typed-nil singleton is therefore write-protected.
//
//   The trap that rule avoids: a nil test that PEEKS AT THE VALUE calls a struct-field or
//   array-element reference "nil" whenever the referenced field's type is a reference type,
//   because such a box leaves its own `m_val` an unused default — and then hands back `default(T)`
//   in place of the field's actual value.
//
// WHAT COUNTS AS A GO FIELD IS NOT WHAT COUNTS AS A C# FIELD
//   `GoFields` projects, in metadata order, and every rule below exists because some converted
//   shape needs it:
//     * a defined-type-over-struct wrapper exposes the UNDERLYING struct's fields, so the
//       projection descends through its single `m_value`;
//     * a promoted embed is stored as a `ж<T>` backing box under a marker-prefixed name, and Go
//       sees it as a field named after the embedded TYPE — so the path records a "box hop" and
//       dereferences through it;
//     * the converter's blank renames (`_`, `__`, …) all map back to Go's single `_`;
//     * compiler backing fields and `[GoReflectCompanion]` bridge fields are not Go fields at all.
//   One documented exposure: a REAL Go field literally named `__` is indistinguishable from a
//   renamed blank here — the same class of collision as any marker-shaped identifier.
//
// PERFORMANCE SHAPE
//   Every generic entry point is a cached open-generic `MakeGenericMethod` + `CreateDelegate`, keyed
//   so the reflection is paid once per type (or type pair) and never per value. `reflect.Value.Field`
//   and `.Index` sit under `fmt`'s `%v` of any struct or slice, so a per-call `Invoke` here is a
//   per-element cost across the whole corpus. Keep new members in that shape.
// ---------------------------------------------------------------------------------------------
public static partial class GoReflect
{
    // ==== POINTER SLOTS — reading and writing THROUGH a box ====
    // Shared by the reflect and reflectlite bridges, and the foundation the field and element
    // aliases further down are built on: those mint a ж<F>, and this is what a caller holding one
    // as `object` reads and writes it with.

    // Cached ref-accessor reads/writes of a ж<T> box's value slot, keyed by the closed box type.
    // ValueSlot (not Value) both ways: reads must not panic on a slot HOLDING a nil value (Go's
    // `*(&p)` yields nil, no dereference), and writes assign THROUGH the ref-returning property so
    // they land in the real slot — field-ref and array-element alias boxes included, which is what
    // Field(i)/Index(i) addressability builds on in the next increment. Writes to a structurally
    // nil box panic exactly like Go's nil-pointer store (blessing condition Q1a: the canonical
    // typed-nil singleton is write-protected).
    private static readonly ConcurrentDictionary<Type, Func<object, object?>> s_slotReaders = new();
    private static readonly ConcurrentDictionary<Type, Action<object, object?>> s_slotWriters = new();

    /// <summary>Reads the value held by a pointer box — a closed <c>ж&lt;T&gt;</c> or a generated named-pointer
    /// wrapper (<c>IPointer&lt;T&gt;</c>) — nil-safe (a nil box reads as the zero value).</summary>
    public static object? ReadPointerSlot(object box)
    {
        return s_slotReaders.GetOrAdd(box.GetType(), static boxType =>
        {
            (bool viaInterface, Type elem) = slotAccessorShape(boxType);
            MethodInfo reader = typeof(GoReflect).GetMethod(viaInterface ? nameof(readSlotViaInterface) : nameof(readSlot), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(elem);
            return reader.CreateDelegate<Func<object, object?>>();
        })(box);
    }

    /// <summary>Writes a value through a pointer box's slot ref (panics Go-style on a nil box).</summary>
    public static void WritePointerSlot(object box, object? value)
    {
        s_slotWriters.GetOrAdd(box.GetType(), static boxType =>
        {
            (bool viaInterface, Type elem) = slotAccessorShape(boxType);
            MethodInfo writer = typeof(GoReflect).GetMethod(viaInterface ? nameof(writeSlotViaInterface) : nameof(writeSlot), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(elem);
            return writer.CreateDelegate<Action<object, object?>>();
        })(box, value);
    }

    /// <summary>
    /// Resolves the pointee a managed type hands out through a value slot — a closed
    /// <c>ж&lt;T&gt;</c> reads its own slot, a generated named-pointer wrapper reads through
    /// <see cref="IPointer{T}"/>'s ref-returning <c>Value</c> — reporting <c>false</c> when
    /// <paramref name="boxType"/> is not a pointer box at all.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The NEGATIVE answer is the load-bearing one, and it names a real Go shape rather than a
    /// misuse. <see cref="KindOf"/> classifies every managed REFERENCE it does not otherwise
    /// recognize as <c>Pointer</c> — the reflection bridge's descent rule: an opaque handle a
    /// hand-owned shim holds in place of Go's own representation (<c>sync.Mutex</c>'s
    /// <c>SemaphoreSlim</c> gate, <c>sync.RWMutex</c>'s <c>RWState</c>) is one pointer word wide
    /// and is never looked inside. That rule has a VALUE-side twin, and this is where it is
    /// asked: such a handle has no pointee, because whatever it refers to has no Go
    /// representation at all — so "one word, do not descend into it" is also "no slot, do not
    /// read through it".
    /// </para>
    /// <para>
    /// A caller holding a Pointer-kind value must therefore ask here before dereferencing:
    /// <c>reflect.Value.Elem</c> (and its <c>internal/reflectlite</c> twin) answer the invalid
    /// Value for a handle, exactly as they do for a nil pointer. Reading anyway threw
    /// <c>Not a pointer box type</c> out of <see cref="ReadPointerSlot"/>, which took out every
    /// <c>reflect.DeepEqual</c> over a struct holding a <c>sync</c> primitive.
    /// </para>
    /// </remarks>
    public static bool TryPointerBoxElement(Type? boxType, [NotNullWhen(true)] out Type? elemType)
    {
        return tryPointerBoxShape(boxType, out _, out elemType);
    }

    // A raw closed ж<T> uses the ValueSlot pair; a generated named-pointer wrapper (a non-generic
    // class implementing IPointer<T>) routes through the interface's ref-returning Value.
    private static bool tryPointerBoxShape(Type? boxType, out bool viaInterface, [NotNullWhen(true)] out Type? elemType)
    {
        viaInterface = false;
        elemType = null;

        if (boxType is null)
            return false;

        // Fix W with the M exemption: a ж box at any base-chain depth takes the slot pair — the
        // severe B1 §3.1 site, where falling through to the interface arm silently flips
        // ReadPointerSlot/WritePointerSlot semantics (a ж<ж<T>> holding null starts panicking
        // where it succeeds — ValueSlot's own documented case). @unsafe.Pointer keeps its
        // interface route, exactly as it takes today.
        if (!typeof(IUnsafePointer).IsAssignableFrom(boxType) && TryBoxPointee(boxType, out Type? pointee))
        {
            elemType = pointee;
            return true;
        }

        if (ContainerInterfaceArguments(boxType, typeof(IPointer<>)) is not { } args)
            return false;

        viaInterface = true;
        elemType = args[0];
        return true;
    }

    // The reader/writer selection for a box whose shape the caller has already accepted. Reaching
    // the throw means a caller dereferenced without asking TryPointerBoxElement first — a bridge
    // invariant violation, not a Go state, so it stays loud.
    private static (bool viaInterface, Type elemType) slotAccessorShape(Type boxType)
    {
        if (!tryPointerBoxShape(boxType, out bool viaInterface, out Type? elemType))
            throw new InvalidOperationException($"Not a pointer box type: {boxType}");

        return (viaInterface, elemType);
    }

    private static object? readSlot<T>(object box)
    {
        return ((ж<T>)box).ValueSlot;
    }

    private static void writeSlot<T>(object box, object? value)
    {
        ж<T> typed = (ж<T>)box;

        if (typed.IsNilPointer)
            throw RuntimeErrorPanic.NilPointerDereference();

        typed.ValueSlot = (T)value!;
    }

    private static object? readSlotViaInterface<T>(object box)
    {
        IPointer<T> typed = (IPointer<T>)box;

        // STRUCTURAL nil first: there is no storage to read, so the pointee reads as the zero value.
        // Every other box resolves its REAL storage through Value — including a struct-field or
        // array-element reference, whose own `m_val` is an unused default. That default is the trap:
        // a nil test that PEEKS AT THE VALUE calls such a box nil whenever the referenced field's
        // type is a reference type, and hands back default(T) in place of the field's actual value.
        // So ж<T>.IsNull is STRUCTURAL for those kinds, and the case it still answers by value — a
        // standard box whose reference-typed pointee is legitimately null — has default(T) as the
        // correct answer anyway, which is why the fallback on the last line is safe.
        if (box is INilPointer { IsNilPointer: true })
            return default(T);

        return typed.IsNull ? default(T) : typed.Value;
    }

    private static void writeSlotViaInterface<T>(object box, object? value)
    {
        if (box is INilPointer { IsNilPointer: true })
            throw RuntimeErrorPanic.NilPointerDereference();

        ((IPointer<T>)box).Value = (T)value!;
    }

    // -------- Go struct-field projection (embeds, named-struct wrappers, blanks, companions) --------

    /// <summary>A struct's Go-visible field: its Go name, static Go type, exportedness, and access path.</summary>
    public readonly struct GoFieldInfo
    {
        /// <summary>The Go field name (`_` for a blank field, the embed's type name for an embedded field).</summary>
        public readonly string Name;

        /// <summary>The field's STATIC Go type (an embedded field reports the embedded type, not its backing box).</summary>
        public readonly Type Type;

        /// <summary>Go exportedness (uppercase first rune of <see cref="Name"/>).</summary>
        public readonly bool Exported;

        /// <summary>
        /// Whether this is an EMBEDDED field — Go's <c>struct{ T }</c> rather than
        /// <c>struct{ T T }</c>. The two are otherwise indistinguishable through this projection:
        /// an embed's Go field name IS its type name, so name and type match and only this flag
        /// separates them. <c>reflect</c>'s struct-identity walk compares it for exactly that
        /// reason (Go's <c>haveIdenticalUnderlyingType</c> ends each field with
        /// <c>tf.Embedded() != vf.Embedded()</c>), which is what it exists for.
        /// </summary>
        public readonly bool Embedded;

        /// <summary>
        /// The array dims this field's descriptor hands down through <c>Elem()</c>: its OWN when
        /// <see cref="Type"/> is an array kind and the declaring zero instance reveals them, the
        /// POINTEE's or the map ELEMENT's when the converter stamped <c>[GoArrayDims]</c> for a hop
        /// no zero instance can measure.
        /// </summary>
        public readonly nint[]? ArrayDims;

        /// <summary>
        /// The array dims of a map field's KEY, from the converter's <c>[GoMapKeyDims]</c> stamp —
        /// what <c>reflect.Type.Key()</c> hands down. Null for every field that is not a map with a
        /// fixed-size-array key.
        /// </summary>
        public readonly nint[]? KeyDims;

        /// <summary>
        /// The Go channel DIRECTION when <see cref="Type"/> is a channel kind and the declaring
        /// zero instance reveals it — the converter emits a directional field as an initializer
        /// (<c>= channel&lt;@string&gt;.SendOnly</c>), which is the same route the array dims take.
        /// </summary>
        public readonly GoChanDir ChanDir;

        /// <summary>The unified channel-value cargo of a channel-typed field (increment D), or <c>null</c>.</summary>
        public readonly ChanCargo? ChanCargo;

        /// <summary>
        /// The DESCRIPTOR CARRIER for this field's own Go type — the uninhabited interface holding
        /// the Go name of a defined-over-interface type the emission erased to a `using` alias, from
        /// the converter's <c>[GoDescriptorType(Self = ...)]</c> stamp. Null for every field whose
        /// managed type already carries its Go name, which is almost all of them.
        /// </summary>
        public readonly Type? DescriptorSelf;

        /// <summary>
        /// The field's raw Go struct TAG, verbatim — <c>asn1:"optional,explicit,tag:0"</c> — or the
        /// empty string when the field carries none. The converter emits every tagged field's tag as
        /// <c>[GoTag]</c> at the declaration, so this is the declared text, not a reconstruction.
        /// </summary>
        public readonly string Tag;

        // The C# access path from the declaring struct: each step is an instance field; a step
        // whose IsBoxHop flag is set holds a ж<T> promoted-embed box the path derefs through.
        internal readonly FieldInfo[] Path;

        /// <summary>The managed type that DECLARES this Go field -- the last hop of its accessor path -- which is
        /// the field's package of origin: a defined type over a foreign struct is a wrapper whose projection
        /// descends to the underlying struct's fields, and Go's <c>StructField.PkgPath</c> answers the
        /// declaring package, not the defined type's (increment E2).</summary>
        public Type? DeclaringType => Path is { Length: > 0 } ? Path[^1].DeclaringType : null;
        internal readonly bool[] BoxHop;

        internal GoFieldInfo(string name, Type type, nint[]? arrayDims, FieldInfo[] path, bool[] boxHop, string tag = "", bool embedded = false, GoChanDir chanDir = GoChanDir.Unstamped, nint[]? keyDims = null, Type? descriptorSelf = null, ChanCargo? chanCargo = null)
        {
            Name = name;
            Type = type;
            DescriptorSelf = descriptorSelf;
            Exported = name.Length > 0 && name != "_" && char.IsUpper(name[0]);
            ArrayDims = arrayDims;
            KeyDims = keyDims;
            ChanDir = chanDir;
            ChanCargo = chanCargo;
            Tag = tag;
            Embedded = embedded;
            Path = path;
            BoxHop = boxHop;
        }

        /// <summary>Reads this field's value from a live struct instance (path-following, boxed).</summary>
        public object? Read(object structValue)
        {
            object? current = structValue;

            for (int i = 0; i < Path.Length && current is not null; i++)
            {
                current = Path[i].GetValue(current);

                if (BoxHop[i] && current is not null)
                    current = ReadPointerSlot(current);
            }

            return current;
        }
    }

    private static readonly ConcurrentDictionary<Type, GoFieldInfo[]> s_goFields = new();

    /// <summary>
    /// The Go-visible fields of a converted struct type, in metadata order: unwraps a defined-
    /// type-over-struct wrapper's <c>m_value</c>, projects a promoted-embed backing box
    /// (<c>Ꮡʗ</c>-prefixed <c>ж&lt;T&gt;</c> field) as the embedded Go field of type <c>T</c>,
    /// maps the converter's blank renames (<c>_</c>, <c>__</c>, …) to Go's <c>"_"</c> (a REAL Go
    /// field named <c>__</c> is a documented exposure, same class as marker-shaped identifiers),
    /// and excludes compiler backing fields and <c>[GoReflectCompanion]</c>-marked bridge fields.
    /// </summary>
    public static GoFieldInfo[] GoFields(Type structType)
    {
        return s_goFields.GetOrAdd(structType, static t =>
        {
            List<GoFieldInfo> result = new();
            collectGoFields(t, [], [], result);
            return result.ToArray();
        });
    }

    private static void collectGoFields(Type t, FieldInfo[] prefixPath, bool[] prefixHops, List<GoFieldInfo> result)
    {
        FieldInfo[] fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // A defined-type-over-struct wrapper ([GoType("<underlying name>")], single private
        // m_value) exposes the UNDERLYING struct's fields on the named type in Go.
        if (fields is [{ Name: "m_value" } valueField] &&
            t.GetCustomAttributes(typeof(GoTypeAttribute), false) is [GoTypeAttribute { Definition.Length: > 0 } def] &&
            def.Definition != "dyn" && valueField.FieldType.IsValueType && KindOf(valueField.FieldType) == Struct)
        {
            collectGoFields(valueField.FieldType, [.. prefixPath, valueField], [.. prefixHops, false], result);
            return;
        }

        int first = result.Count;

        foreach (FieldInfo field in fields)
        {
            string name = field.Name;

            if (name.Contains("k__BackingField", StringComparison.Ordinal))
                continue;

            if (field.GetCustomAttributes(typeof(GoReflectCompanionAttribute), false).Length != 0)
                continue;

            // Promoted-embed backing field: `private T ʗName` → Go field `Name` of type T, with the
            // field's own type projected verbatim so a VALUE embed reports the struct and a POINTER
            // embed reports the `ж<T>` pointer, exactly as Go's `struct{ T }` / `struct{ *T }` do.
            // No hop: the embed is an INLINE field of the enclosing struct, so the accessor path
            // reads it directly. (It was a `ж<T>` BOX named `ᏑʗName` until 2026-08-14 — a shared box
            // that made a struct value copy alias its embed, the defect behind go/types'
            // type-parameter identity wall — and the projection then had to unwrap the box and mark
            // the extra pointer hop. Keying on the box shape is what tied this walk to that model:
            // the moment the embed became a field, an unrecognized `ʗName` fell through to the
            // generic arm and reported the Go field under its MANGLED name.)
            if (name.StartsWith(CapturedVarMarker, StringComparison.Ordinal))
            {
                string goName = name[CapturedVarMarker.Length..];
                nint[]? embedDims = KindOf(field.FieldType) == Array ? FieldArrayDims(t, field) : FieldStampedDims(field);
                GoChanDir embedDir = KindOf(field.FieldType) == Chan ? FieldChanDir(t, field) : GoChanDir.Unstamped;
                ChanCargo? embedCargo = KindOf(field.FieldType) == Chan ? FieldChanCargo(t, field) : null;
                result.Add(new GoFieldInfo(goName, field.FieldType, embedDims, [.. prefixPath, field], [.. prefixHops, false], embedTagOf(t, field, goName), embedded: true, chanDir: embedDir, keyDims: FieldMapKeyDims(field), descriptorSelf: FieldDescriptorType(field), chanCargo: embedCargo));
                continue;
            }

            string projected = name;

            if (projected.StartsWith(ShadowVarMarker, StringComparison.Ordinal))
                projected = projected[ShadowVarMarker.Length..];

            if (projected.Length > 0 && isAllUnderscores(projected))
                projected = "_";

            // An ARRAY field's dims come from the declaring type's zero instance, where the
            // converter's `= new(N)` initializer put them; every OTHER field's come from the
            // converter's [GoArrayDims] stamp, which is null unless the field reaches an array
            // through a pointer or a map element — the two hops no zero instance can measure.
            nint[]? dims = KindOf(field.FieldType) == Array ? FieldArrayDims(t, field) : FieldStampedDims(field);
            GoChanDir fieldDir = KindOf(field.FieldType) == Chan ? FieldChanDir(t, field) : GoChanDir.Unstamped;
            ChanCargo? fieldCargo = KindOf(field.FieldType) == Chan ? FieldChanCargo(t, field) : null;
            // A plain field the converter stamped [GoEmbedded] is a Go EMBEDDED field of a predeclared
            // type (`struct{ int }`): nothing to promote, so no `partial ref` shape to key on (E2b).
            result.Add(new GoFieldInfo(projected, field.FieldType, dims, [.. prefixPath, field], [.. prefixHops, false], goTagOf(field), embedded: field.IsDefined(typeof(GoEmbeddedAttribute), false), chanDir: fieldDir, keyDims: FieldMapKeyDims(field), descriptorSelf: FieldDescriptorType(field), chanCargo: fieldCargo));
        }

        reorderToGoDeclarationOrder(t, result, first);
    }

    // Field order is Go DECLARATION order, and CLR metadata order stops being that the moment a
    // struct EMBEDS: go2cs-gen mints every embed's ʗ backing field in a GENERATED partial, and
    // partial parts concatenate, so a Go declaration that embeds first carries its embed LAST in
    // metadata. Everything indexed (Field(i)/rtype.Field(i)/GoFieldOffsets — which pairs with this
    // projection BY INDEX) and everything ordered (fmt's %v walk, json's member order) reads this
    // walk, so the wrong order sent reflectlite's TestCanSetField index chains into an int field
    // and printed Talias2's embeds reversed under %#v.
    //
    // The surviving record of the declaration order is the generator's ALL-FIELDS constructor —
    // emitted from the declaration syntax, its parameters named exactly by the Go field names, in
    // Go order. The reorder applies exactly when an embedded field is present and a constructor
    // whose parameter names form a BIJECTION with the projected field names exists; an embed-free
    // struct's metadata order IS declaration order (the converter declares every field inline),
    // and a struct with no matching constructor keeps metadata order — today's behavior, never a
    // half-applied guess.
    private static void reorderToGoDeclarationOrder(Type t, List<GoFieldInfo> result, int first)
    {
        int count = result.Count - first;

        if (count < 2)
            return;

        bool hasEmbed = false;

        for (int i = first; i < result.Count; i++)
        {
            if (result[i].Embedded)
            {
                hasEmbed = true;
                break;
            }
        }

        if (!hasEmbed)
            return;

        Dictionary<string, int> byName = new(count, StringComparer.Ordinal);

        for (int i = first; i < result.Count; i++)
        {
            // Duplicate projected names (a blank-field struct projects several "_") cannot form a
            // bijection with parameter names; keep metadata order.
            if (!byName.TryAdd(result[i].Name, i))
                return;
        }

        foreach (ConstructorInfo ctor in t.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            ParameterInfo[] parameters = ctor.GetParameters();

            if (parameters.Length != count)
                continue;

            GoFieldInfo[] ordered = new GoFieldInfo[count];
            bool matched = true;

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].Name is { } name && byName.TryGetValue(name, out int index))
                {
                    ordered[i] = result[index];
                }
                else
                {
                    matched = false;
                    break;
                }
            }

            if (!matched)
                continue;

            for (int i = 0; i < count; i++)
                result[first + i] = ordered[i];

            return;
        }
    }

    // The declared Go struct tag of a converted field, or "" when it carries none. The converter
    // emits `[GoTag("…")]` (aliased to DescriptionAttribute) at every tagged field declaration —
    // it has done so all along, and until now nothing read it, which is why reflect.StructField.Tag
    // came back empty for every converted struct and every tag-driven decoder saw an untagged type.
    private static string goTagOf(FieldInfo field)
    {
        return field.GetCustomAttributes(typeof(GoTagAttribute), false) is [GoTagAttribute tag]
            ? tag.Description
            : "";
    }

    // The declared Go struct tag of an EMBEDDED field, which lives at a DIFFERENT declaration site
    // from every other field's and therefore needs its own read.
    //
    // An embed is emitted by the converter as a partial PROPERTY (`[GoTag("json:\"e,omitempty\"")]
    // public partial ref ж<Embed0b> Embed0b { get; }`) and by go2cs-gen as the backing FIELD the
    // property returns a ref to (`private ж<Embed0b> ʗEmbed0b;`). The generator does not carry the
    // declaration's attributes onto the field it mints, so reading the field alone reported EVERY
    // embedded field as untagged — silently, because "" is the right answer for most embeds. The
    // consequence is a tag-driven decoder seeing an untagged embed: encoding/json's TestMarshalEmbeds
    // emitted `"Embed0b":{…}` where Go emits `"e":{…}`, and marshalled the `json:"-"` embed it must
    // omit entirely.
    //
    // The property is the DECLARATION, so it is asked first; the field keeps the fallback so a
    // future generator that does propagate the attribute needs no change here.
    private static string embedTagOf(Type declaringType, FieldInfo field, string goName)
    {
        PropertyInfo? declaration = declaringType.GetProperty(goName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (declaration is not null && declaration.GetCustomAttributes(typeof(GoTagAttribute), false) is [GoTagAttribute tag])
            return tag.Description;

        return goTagOf(field);
    }

    private static bool isAllUnderscores(string name)
    {
        foreach (char c in name)
        {
            if (c != '_')
                return false;
        }

        return true;
    }

    // -------- addressable alias boxes (Field(i)/Index(i) write-through; the ref-accessor contract) --------

    private static readonly ConcurrentDictionary<(Type boxType, string fieldKey), Delegate> s_fieldAccessors = new();
    private static readonly ConcurrentDictionary<Type, Func<object, Delegate, object>> s_fieldBoxMakers = new();
    private static readonly ConcurrentDictionary<(Type boxType, Type elemType), Func<object, nint, object>> s_elementBoxMakers = new();
    private static readonly ConcurrentDictionary<Type, Func<object, int, object>> s_arrayElementBoxMakers = new();

    /// <summary>
    /// A field-alias <c>ж&lt;F&gt;</c> over a parent box's Go field: reads/writes route through the
    /// parent's <see cref="ж{T}.ValueSlot"/> and the projected field path, so nested parents
    /// (field-of-field, element parents) land in REAL storage — where <c>FieldRef&lt;T&gt;.Create</c>'s
    /// <c>m_val</c>-hardcoded IL would not. The cached accessor doubles as the ж equality-identity
    /// token, preserving Go's <c>&amp;s.f == &amp;s.f</c>.
    /// </summary>
    public static object FieldAliasBox(object parentBox, GoFieldInfo field)
    {
        Type boxType = parentBox.GetType();
        Delegate accessor = s_fieldAccessors.GetOrAdd((boxType, fieldPathKey(field)), _ => buildFieldAccessor(boxType, field));

        return s_fieldBoxMakers.GetOrAdd(field.Type, static ft =>
            typeof(GoReflect).GetMethod(nameof(makeFieldBox), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(ft).CreateDelegate<Func<object, Delegate, object>>())(parentBox, accessor);
    }

    private static object makeFieldBox<F>(object parentBox, Delegate accessor)
    {
        FieldRefFunc<F> fieldRef = (FieldRefFunc<F>)accessor;
        return new FieldRefBox<F>(parentBox, fieldRef, accessor);
    }

    private static string fieldPathKey(GoFieldInfo field)
    {
        string key = "";

        for (int i = 0; i < field.Path.Length; i++)
            key += (field.BoxHop[i] ? "*" : ".") + field.Path[i].Name;

        return key;
    }

    /// <summary>
    /// The name prefix every reflect-built field accessor carries. It is a CONTRACT, not a label:
    /// <c>ж&lt;T&gt;.PointerOrderToken</c> reads the field name back off the delegate to look up the
    /// field's Go offset, so that a field pointer reached through reflect and the same field pointer
    /// reached through converted code (which arrives under the generated <c>Ꮡ&lt;field&gt;</c>
    /// spelling) resolve to ONE offset and therefore token alike.
    /// </summary>
    internal const string FieldAccessorPrefix = "goref_";

    // DynamicMethod: (object box) => ref ((ж<S>)box).ValueSlot.path... — each plain step is an
    // ldflda; a box-hop step loads the ж<E> reference and re-enters through ITS ValueSlot.
    private static Delegate buildFieldAccessor(Type boxType, GoFieldInfo field)
    {
        DynamicMethod method = new(
            name: $"{FieldAccessorPrefix}{field.Name}",
            returnType: field.Type.MakeByRefType(),
            parameterTypes: [typeof(object)],
            m: typeof(GoReflect).Module,
            skipVisibility: true);

        ILGenerator il = method.GetILGenerator();

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Castclass, boxType);
        il.Emit(OpCodes.Callvirt, boxType.GetProperty(nameof(ж<int>.ValueSlot))!.GetGetMethod()!);

        for (int i = 0; i < field.Path.Length; i++)
        {
            if (!field.BoxHop[i])
            {
                il.Emit(OpCodes.Ldflda, field.Path[i]);
                continue;
            }

            il.Emit(OpCodes.Ldfld, field.Path[i]);
            il.Emit(OpCodes.Callvirt, field.Path[i].FieldType.GetProperty(nameof(ж<int>.ValueSlot))!.GetGetMethod()!);
        }

        il.Emit(OpCodes.Ret);

        return method.CreateDelegate(typeof(FieldRefFunc<>).MakeGenericType(field.Type));
    }

    /// <summary>
    /// An element-alias <c>ж&lt;E&gt;</c> over an ADDRESSABLE container box, via
    /// <see cref="ж{T}.at{TElem}(nint)"/> — which materializes a lazily-backed named-array
    /// wrapper on the REAL storage (the pallocBits lesson), validates the index, and yields a
    /// write-through element ref.
    /// </summary>
    public static object ElementAliasBoxOfBox(object containerBox, Type elemType, nint index)
    {
        Type boxType = containerBox.GetType();

        return s_elementBoxMakers.GetOrAdd((boxType, elemType), static key =>
        {
            Type containerType = key.boxType.GetGenericArguments()[0];
            return typeof(GoReflect).GetMethod(nameof(elementBoxViaAt), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(containerType, key.elemType).CreateDelegate<Func<object, nint, object>>();
        })(containerBox, index);
    }

    private static object elementBoxViaAt<C, E>(object box, nint index)
    {
        return ((ж<C>)box).at<E>(index);
    }

    /// <summary>
    /// An element-alias <c>ж&lt;E&gt;</c> over a DETACHED container VALUE (a slice result of
    /// <c>MakeSlice</c>, a slice read out of a slot): golib slices/arrays share their backing
    /// store across struct copies, so the ref still lands in real storage.
    /// </summary>
    public static object ElementAliasBoxOfValue(object containerValue, Type elemType, nint index)
    {
        return s_arrayElementBoxMakers.GetOrAdd(elemType, static et =>
            typeof(GoReflect).GetMethod(nameof(elementBoxOfArray), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(et).CreateDelegate<Func<object, int, object>>())(containerValue, (int)index);
    }

    private static object elementBoxOfArray<E>(object arrayValue, int index)
    {
        return new ElemRefBox<E>((IArray)arrayValue, index);
    }

    private static readonly ConcurrentDictionary<Type, Func<object, nint, nint, object>> s_sliceWindowMakers = new();

    /// <summary>
    /// A <c>slice&lt;E&gt;</c> window <c>[low:high]</c> over a container value that SHARES the
    /// source's backing store (Go slice semantics — <c>reflect.Value.Slice</c>): a raw slice
    /// re-windows, a raw array wraps its backing, a named slice wrapper windows its underlying
    /// view.
    /// </summary>
    public static object SliceWindow(object container, Type elemType, nint low, nint high)
    {
        return s_sliceWindowMakers.GetOrAdd(elemType, static et =>
            typeof(GoReflect).GetMethod(nameof(sliceWindow), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(et).CreateDelegate<Func<object, nint, nint, object>>())(container, low, high);
    }

    private static object sliceWindow<E>(object container, nint low, nint high)
    {
        return container switch
        {
            slice<E> s => s.slice(low, high),
            array<E> a => new slice<E>(a, low, high),
            ISlice<E> view => new slice<E>(view).slice(low, high),
            _ => throw new InvalidOperationException($"SliceWindow: unsupported container {container.GetType()}")
        };
    }

    private static readonly ConcurrentDictionary<Type, Func<object, nint, nint, nint, object>> s_sliceWindow3Makers = new();

    /// <summary>
    /// The FULL slice expression's window — <c>[low:high:max]</c>, Go's
    /// <c>reflect.Value.Slice3</c>. Identical to <see cref="SliceWindow(object,Type,nint,nint)"/>
    /// except that <paramref name="max"/> bounds the result's CAPACITY, which may end before the
    /// shared backing store does.
    /// </summary>
    /// <remarks>
    /// Bounds are RELATIVE to the source container, exactly as Go's <c>s[i:j:k]</c> is — so a
    /// window of a window composes without the caller tracking absolute offsets. The caller has
    /// already applied Go's <c>0 &lt;= i &lt;= j &lt;= k &lt;= cap</c> test and its panic text;
    /// the golib bounds check behind this is the backstop, not the contract.
    /// </remarks>
    public static object SliceWindow(object container, Type elemType, nint low, nint high, nint max)
    {
        return s_sliceWindow3Makers.GetOrAdd(elemType, static et =>
            typeof(GoReflect).GetMethod(nameof(sliceWindow3), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(et).CreateDelegate<Func<object, nint, nint, nint, object>>())(container, low, high, max);
    }

    private static object sliceWindow3<E>(object container, nint low, nint high, nint max)
    {
        return container switch
        {
            slice<E> s => s.Reslice(low, high, max),
            array<E> a => new slice<E>((E[])a, low, high, max),
            ISlice<E> view => new slice<E>(view).Reslice(low, high, max),
            _ => throw new InvalidOperationException($"SliceWindow: unsupported container {container.GetType()}")
        };
    }


    private static readonly ConcurrentDictionary<Type, Func<object?, nint, object?>> s_sliceGrowers = new();

    /// <summary>
    /// A slice with room for <paramref name="extra"/> more elements past its LENGTH, preserving
    /// that length and its contents (<c>reflect.Value.Grow</c>). Returns the source unchanged when
    /// the spare capacity already suffices — Go's <c>growslice</c> is only reached past the
    /// capacity, and reallocating early would silently detach a caller that still holds the old
    /// backing store.
    /// </summary>
    /// <remarks>
    /// Go does not specify the capacity a grow lands on (its <c>growslice</c> rounds to a size
    /// class), only that it is at least <c>len+extra</c>; this doubles, which is Go's own growth
    /// shape for the small sizes reflection callers reach. The result is a plain
    /// <c>slice&lt;E&gt;</c> even when the source was a named wrapper — the caller converts it
    /// back into the slot's own type, the same single convertibility relation <c>SetLen</c> uses.
    /// </remarks>
    public static object? GrowSlice(object? container, Type elemType, nint extra)
    {
        return s_sliceGrowers.GetOrAdd(elemType, static et =>
            typeof(GoReflect).GetMethod(nameof(growSlice), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(et).CreateDelegate<Func<object?, nint, object?>>())(container, extra);
    }

    private static object? growSlice<E>(object? container, nint extra)
    {
        slice<E> s = container switch
        {
            null => default,
            slice<E> raw => raw,
            ISlice<E> view => new slice<E>(view),
            _ => throw new InvalidOperationException($"GrowSlice: unsupported container {container.GetType()}")
        };

        nint length = s.Length;

        if (length + extra <= s.Capacity)
            return container;

        nint capacity = s.Capacity == 0 ? length + extra : s.Capacity;

        while (capacity < length + extra)
            capacity *= 2;

        E[] backing = new E[capacity];

        // Block copy, not an element loop: the demonstrated consumer (encoding/gob's decUint8Slice)
        // grows buffers past internal/saferio's 10 MiB chunk, where a per-element ref indexer walk
        // is orders of magnitude slower than the memmove a Span copy compiles to.
        s.ToSpan().CopyTo(backing);

        return new slice<E>(backing, 0, length);
    }

    private static readonly ConcurrentDictionary<(Type keyType, Type elemType), Action<object, object?, object?>> s_mapSetters = new();

    /// <summary>
    /// Stores a key/value pair through a live golib map — raw <c>map&lt;K,V&gt;</c> and named map
    /// wrappers both implement <c>IDictionary&lt;K,V&gt;</c> (<c>reflect.Value.SetMapIndex</c>).
    /// </summary>
    public static void SetMapEntry(object map, Type keyType, Type elemType, object? key, object? value)
    {
        s_mapSetters.GetOrAdd((keyType, elemType), static k =>
            typeof(GoReflect).GetMethod(nameof(setMapEntry), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(k.keyType, k.elemType).CreateDelegate<Action<object, object?, object?>>())(map, key, value);
    }

    private static void setMapEntry<K, V>(object map, object? key, object? value) where K : notnull
    {
        ((IDictionary<K, V>)map)[(K)key!] = (V)value!;
    }

    private static readonly ConcurrentDictionary<(Type keyType, Type elemType), Action<object, object?>> s_mapDeleters = new();

    /// <summary>
    /// Deletes the entry stored under a key in a live golib map — Go's <c>delete(m, k)</c>, reached
    /// from <c>reflect.Value.SetMapIndex</c> when the element Value is the ZERO Value.
    /// </summary>
    /// <remarks>
    /// Go gives <c>SetMapIndex</c> two operations behind one signature: an invalid <c>elem</c> DELETES
    /// the key, any other assigns it. That is not a detail of the API's shape — it is the only way
    /// <c>reflect</c> can delete at all, since there is no <c>Value.DeleteMapIndex</c>. Deleting an
    /// absent key is a no-op in Go, as is deleting from a nil map, so neither is an error here.
    /// </remarks>
    public static void DeleteMapEntry(object map, Type keyType, Type elemType, object? key)
    {
        s_mapDeleters.GetOrAdd((keyType, elemType), static k =>
            typeof(GoReflect).GetMethod(nameof(deleteMapEntry), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(k.keyType, k.elemType).CreateDelegate<Action<object, object?>>())(map, key);
    }

    private static void deleteMapEntry<K, V>(object map, object? key) where K : notnull
    {
        // IMap<K,V>'s Remove, not IDictionary's, so golib's dedicated NIL-key slot is reached — the
        // backing Dictionary cannot hold a null key and the non-generic surface cannot see that entry.
        ((IMap<K, V>)map).Remove((K)key!);
    }

    private static readonly ConcurrentDictionary<(Type keyType, Type elemType), Func<object, object?, (bool, object?)>> s_mapGetters = new();

    /// <summary>
    /// Reads the value stored under a key in a live golib map, reporting Go's comma-ok presence
    /// (<c>reflect.Value.MapIndex</c>, whose missing-key answer is the INVALID zero Value).
    /// </summary>
    /// <remarks>
    /// Routed through <see cref="IMap{TKey, TValue}"/>'s comma-ok indexer rather than the
    /// non-generic <see cref="IDictionary"/> walk <c>mapBacking</c> uses, so the NIL key is found
    /// like any other: the backing <see cref="Dictionary{TKey, TValue}"/> cannot hold one and golib
    /// keeps that entry in a dedicated slot, invisible to the non-generic surface.
    /// </remarks>
    public static bool TryGetMapEntry(object map, Type keyType, Type elemType, object? key, out object? value)
    {
        (bool present, object? found) = s_mapGetters.GetOrAdd((keyType, elemType), static k =>
            typeof(GoReflect).GetMethod(nameof(getMapEntry), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(k.keyType, k.elemType).CreateDelegate<Func<object, object?, (bool, object?)>>())(map, key);

        value = found;
        return present;
    }

    private static (bool present, object? value) getMapEntry<K, V>(object map, object? key) where K : notnull
    {
        (V value, bool present) = ((IMap<K, V>)map)[(K)key!, true];
        return (present, present ? value : null);
    }
}
