// GoReflect.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using go.golib;

namespace go;

/// <summary>
/// Managed backing for the native reflection bridge (Phase 4). Go's <c>reflect</c>/<c>internal/abi</c>
/// read an interface's <c>{type,data}</c> words through <c>unsafe.Pointer</c> to reach a runtime type
/// descriptor that has no managed form; this helper reconstructs the Go <c>reflect.Kind</c> of a value
/// from its managed <see cref="Type"/>, and provides a descriptor↔<see cref="Type"/> side table so the
/// hand-owned <c>abi.TypeOf</c> entry point can carry the real managed type on the (otherwise synthetic)
/// <c>abi.Type</c> box. See <c>docs/phase4/DESIGN-reflection-bridge.md</c>.
/// </summary>
/// <remarks>
/// <para>
/// This primary file answers the FIRST question the bridge asks of anything: what IS this type. The
/// Kind ordinals, the descriptor side table, the classification itself, comparability, and the
/// container element/key types — plus <see cref="TryAdapterWrappedType"/>, the unwrap that lets a
/// generated interface adapter answer as the Go value it stands for rather than as itself.
/// </para>
/// <para>
/// The class is <c>partial</c> and the remaining four concerns each have a file and a banner. They
/// are ordered here the way the bridge uses them:
/// </para>
/// <list type="bullet">
/// <item><c>GoReflect.TypeNaming.cs</c> — what the type is CALLED in Go
/// (<c>String</c>/<c>Name</c>/<c>PkgPath</c>, <c>%T</c>).</item>
/// <item><c>GoReflect.TypeLayout.cs</c> — its SHAPE (size, alignment, array dims, func
/// signature).</item>
/// <item><c>GoReflect.ValueMarshalling.cs</c> — producing a VALUE of it, by conversion
/// (<c>Set</c>/<c>Convert</c>) or fabrication (<c>Zero</c>/<c>New</c>/<c>MakeSlice</c>).</item>
/// <item><c>GoReflect.FieldAccess.cs</c> — reaching INSIDE a value, and handing back something a
/// write goes through (<c>Field</c>/<c>Index</c>/<c>Slice</c>/<c>SetMapIndex</c>).</item>
/// </list>
/// <para>
/// Splitting moved whole members and changed no logic. Everything is one <c>GoReflect</c> type to the
/// compiler and to every caller, and the static caches are independent of one another, so nothing
/// depends on which file a member is declared in.
/// </para>
/// <para>
/// The rule that binds the four together: <see cref="KindOf"/> here, <c>GoTypeName</c> in TypeNaming
/// and <see cref="ElementType"/> here must UNWRAP a generated adapter identically. They each call
/// <see cref="TryAdapterWrappedType"/> for that reason. An adapter that answered as itself in any one
/// of the three would report a C# class Go cannot name, and the three answers would stop describing
/// the same type.
/// </para>
/// </remarks>
public static partial class GoReflect
{
    // Go reflect.Kind numbering (internal/abi and reflect define these identically, 0..26).
    public const int Invalid = 0, Bool = 1, Int = 2, Int8 = 3, Int16 = 4, Int32 = 5, Int64 = 6;
    public const int Uint = 7, Uint8 = 8, Uint16 = 9, Uint32 = 10, Uint64 = 11, Uintptr = 12;
    public const int Float32 = 13, Float64 = 14, Complex64 = 15, Complex128 = 16;
    public const int Array = 17, Chan = 18, Func = 19, Interface = 20, Map = 21, Pointer = 22;
    public const int Slice = 23, String = 24, Struct = 25, UnsafePointer = 26;

    /// <summary>
    /// True for a kind whose descriptor's <c>arrayDims</c> describe its ELEMENT and therefore pass
    /// down UNSHIFTED — a pointer, a map, a slice or a channel. An ARRAY is deliberately absent: its
    /// dims lead with its OWN length, so it consumes the head and passes the tail.
    /// </summary>
    /// <remarks>
    /// This membership was written out four separate times — the converter's field-cargo walk,
    /// <c>Elem()</c>'s hand-down arm, <c>GoTypeName</c>'s container arms, and
    /// <c>structFieldDescriptor</c>'s filter — so adding slices and channels to the cargo model meant
    /// widening each one independently. The fourth was found only because a positive control STILL
    /// failed with the stamp visibly present in the emitted C#: "the stamp is there" is not "the
    /// stamp is read". Naming the rule makes the next kind a one-line edit at a name instead of a
    /// hunt for a list nothing calls. See docs/phase4/DESIGN-descriptor-cargo.md.
    /// </remarks>
    public static bool KindCarriesElementCargo(int kind) =>
        kind is Pointer or UnsafePointer or Map or Slice or Chan;

    // Maps each synthetic abi.Type descriptor box to the managed Type it stands for, so the hand-owned
    // reflect Type/Value methods (String/Name/Elem/Field/...) can recover Go type info from System.Type.
    // Keyed on the box object identity; weak so descriptors are not pinned.
    private static readonly ConditionalWeakTable<object, Type> s_sysTypes = new();

    /// <summary>
    /// Reports whether <paramref name="t"/> is a <see cref="ж{T}"/> box type — at any depth of its
    /// base chain — and yields the pointee type when it is.
    /// </summary>
    /// <remarks>
    /// The one shared answer to "is this runtime type a ж box?" (B1 §3.1's fix W, modeled on the
    /// <c>PointeeTypeOf</c> walk that was already correct). A one-level
    /// <c>GetGenericTypeDefinition() == typeof(ж&lt;&gt;)</c> test answers identically TODAY —
    /// every box's runtime type sits at depth 0 — and wrongly under the per-kind split, where a
    /// runtime instance is a subclass; the walk answers both worlds. ⚠ Callers that must treat
    /// <c>@unsafe.Pointer</c> specially probe <see cref="IUnsafePointer"/> BEFORE this — its chain
    /// carries <c>ж&lt;uintptr&gt;</c>, so this walk deliberately claims it as a box.
    /// </remarks>
    public static bool TryBoxPointee(Type? t, [NotNullWhen(true)] out Type? pointee)
    {
        for (Type? current = t; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(ж<>))
            {
                pointee = current.GetGenericArguments()[0];
                return true;
            }
        }

        pointee = null;
        return false;
    }

    /// <summary>Records the managed <see cref="Type"/> that a synthetic abi.Type descriptor box stands for.</summary>
    public static void Register(object descriptorBox, Type sysType)
    {
        if (descriptorBox is null || sysType is null)
            return;

        s_sysTypes.AddOrUpdate(descriptorBox, sysType);
    }

    /// <summary>Recovers the managed <see cref="Type"/> a descriptor box stands for, or <c>null</c>.</summary>
    public static Type? SysTypeOf(object? descriptorBox)
    {
        return descriptorBox is not null && s_sysTypes.TryGetValue(descriptorBox, out Type? t) ? t : null;
    }

    /// <summary>
    /// Resolves the managed <see cref="Type"/> that stands for a value's GO DYNAMIC TYPE, seeing
    /// through the runtime's interface-carrier wrappers: an <see cref="IInterfaceAdapter"/> chain
    /// unwraps to the original dynamic value, and a pointer-sourced <see cref="IжAdapter"/> yields
    /// the receiver box's type (<c>ж&lt;T&gt;</c> — Go's dynamic type is the <c>*T</c>, never the
    /// adapter class). The reflection bridge's <c>TypeOf</c>/<c>ValueOf</c> classify THIS type, so
    /// an adapter-held <c>*T</c> and a raw <c>ж&lt;T&gt;</c> intern to the same canonical
    /// <c>reflect.Type</c> and compare assignable by identity, exactly as in Go (R10).
    /// </summary>
    public static Type GoDynamicTypeOf(object value)
    {
        // The canonical nil func carries its delegate type — Go's eface type word for a nil
        // func inside an interface (`%T` prints `func()`, never the carrier class).
        if (value is NilFuncValue nilFunc)
            return nilFunc.Type;

        while (value is IInterfaceAdapter { Value: not null } interfaceAdapter)
            value = interfaceAdapter.Value;

        if (value is IжAdapter { Box: not null } pointerAdapter)
            return CanonicalBoxType(pointerAdapter.Box.GetType());

        // A value-sourced adapter (IValueAdapter) wraps a COPY of a struct this assembly cannot
        // partial — Go's dynamic type is that struct, never the adapter class. A wrapped DELEGATE
        // (a named func type) can itself be nil, and unlike a nil-valued STRUCT — whose boxed copy
        // is never null — a null delegate reference erases its own runtime type, so there is no
        // `.GetType()` to call. The adapter's own class always declares exactly one wrapped-value
        // field (ValueAdapterImplTemplate's `m_value`), and that field's declared TYPE is metadata,
        // present whether or not the field's current value is null — so a null Value falls back to
        // it rather than reporting the shell's own class as Go's dynamic type.
        if (value is IValueAdapter valueAdapter)
        {
            return valueAdapter.Value is {} wrapped
                ? wrapped.GetType()
                : ValueAdapterWrappedType(value.GetType()) ?? CanonicalBoxType(value.GetType());
        }

        return CanonicalBoxType(value.GetType());
    }

    private static readonly ConcurrentDictionary<Type, Type?> s_valueAdapterFieldTypes = new();

    /// <summary>
    /// The declared type of a generated <see cref="IValueAdapter"/> shell's wrapped-value field,
    /// read from the shell's own TYPE metadata rather than an instance — the one channel that
    /// still answers when the wrapped value itself is a null delegate.
    /// </summary>
    /// <remarks>
    /// Internal, not private: <c>builtin.TryTypeAssert</c> needs the same fallback for the
    /// identical reason — a C# type pattern (<c>Value: T wrapped</c>) never matches a null
    /// <c>Value</c>, so asserting a nil-wrapped delegate against its own delegate type has no
    /// runtime instance to pattern-match against either.
    /// </remarks>
    internal static Type? ValueAdapterWrappedType(Type adapterType)
    {
        return s_valueAdapterFieldTypes.GetOrAdd(adapterType, static t =>
            t.GetField("m_value", BindingFlags.Instance | BindingFlags.NonPublic)?.FieldType);
    }

    /// <summary>
    /// Canonicalizes a per-kind box class to the one <c>ж&lt;T&gt;</c> identity Go's <c>*T</c> has.
    /// </summary>
    /// <remarks>
    /// Under the B1 per-kind split, one Go pointer type is FOUR managed classes (standard, field
    /// reference, element reference, native address), and the R10 interning law above — one Go type,
    /// one canonical <c>reflect.Type</c> — must hold across them: <c>reflect.DeepEqual(&amp;x.f,
    /// &amp;table[i])</c> compares a field-ref box against a standard box and dies at DeepEqual's
    /// <c>v1.Type() != v2.Type()</c> gate if each kind interns its own descriptor
    /// (debug/plan9obj's <c>TestOpen</c> was the witness — four banked binary-format suites turned
    /// red on exactly this). The walked <c>ж&lt;T&gt;</c> base IS that identity.
    /// <c>unsafe.Pointer</c> is exempt by its marker, as at every walk site: it is Go's one NAMED
    /// pointer type, and its identity is its own.
    /// </remarks>
    internal static Type CanonicalBoxType(Type type)
    {
        if (typeof(IUnsafePointer).IsAssignableFrom(type))
            return type;

        for (Type? walk = type; walk is not null; walk = walk.BaseType)
        {
            if (walk.IsGenericType && walk.GetGenericTypeDefinition() == typeof(ж<>))
                return walk;
        }

        return type;
    }

    /// <summary>
    /// Classifies a managed <see cref="Type"/> to its Go <c>reflect.Kind</c> ordinal. A NAMED Go type
    /// (<c>type Celsius float64</c> → <c>[GoType("num:float64")]</c>, or a wrapper struct) reports its
    /// UNDERLYING kind, matching Go — the name is recovered separately from the type itself. A
    /// generated interface-implementation adapter CLASS classifies as the Go dynamic type it stands
    /// for (<c>*T</c> → Pointer; a value-sourced ᴠ-adapter → the wrapped struct's kind), mirroring
    /// <see cref="GoTypeName"/>'s unwrap (R10) — though value-level callers should prefer
    /// <see cref="GoDynamicTypeOf"/>, which resolves instance state the type alone cannot.
    /// </summary>
    public static int KindOf(Type? t)
    {
        if (t is null)
            return Invalid;

        if (TryAdapterWrappedType(t, out Type? adapterWrapped, out bool adapterPointerSourced))
            return adapterPointerSourced ? Pointer : KindOf(adapterWrapped);

        // Fast, exact matches for the built-in Go scalar representations.
        if (t == typeof(bool)) return Bool;
        if (t == typeof(nint)) return Int;                 // Go int
        if (t == typeof(sbyte)) return Int8;
        if (t == typeof(short)) return Int16;
        if (t == typeof(int)) return Int32;                // Go int32 / rune
        if (t == typeof(long)) return Int64;
        if (t == typeof(nuint)) return Uint;               // Go uint
        if (t == typeof(byte)) return Uint8;
        if (t == typeof(ushort)) return Uint16;
        if (t == typeof(uint)) return Uint32;
        if (t == typeof(ulong)) return Uint64;
        if (t == typeof(float)) return Float32;
        if (t == typeof(double)) return Float64;
        if (t == typeof(Complex)) return Complex128;
        // golib complex64 is its own hand-written struct (no [GoType] marker), not a narrowing
        // of System.Numerics.Complex — without this arm it classified as Struct and the
        // reflect walkers enumerated its private float fields (binary's Struct.Complex64).
        if (t == typeof(complex64)) return Complex64;
        if (t == typeof(@string)) return String;
        // `any` — the empty interface. System.Object reports IsInterface false, so without
        // this arm it fell through to Struct.
        if (t == typeof(object)) return Interface;
        // A C# System.String can reach reflection where a Go string literal boxed in a
        // deliberately-uncast position (a variadic `...any` argument) — a bare `"a"` rather than
        // `(@string)"a"`; treat it as a Go string so reflect.TypeOf(it).Kind() == String (fmt's doPrint
        // inter-argument spacing depends on it).
        if (t == typeof(string)) return String;
        if (t == typeof(uintptr)) return Uintptr;

        // A named numeric / string wrapper carries its underlying kind in [GoType("num:<kind>")] or
        // [GoType("@string")]; report the underlying kind (Name() recovers the name elsewhere).
        if (TryGoTypeDefinitionKind(t, out int defKind))
            return defKind;

        // @unsafe.Pointer answers by its marker — BEFORE the box classification, because its base
        // chain carries ж<uintptr> and the box arm would otherwise claim it as an ordinary pointer
        // (the load-bearing M-before-W order, DESIGN-zh-box-b1.md §3.1; the marker replaces the
        // old `BaseType == typeof(ж<uintptr>)` structural probe, which is ambiguous under B1's
        // per-kind split in both directions).
        if (typeof(IUnsafePointer).IsAssignableFrom(t)) return UnsafePointer;

        // golib generic containers → their Go kind, detected by open generic definition.
        if (t.IsGenericType)
        {
            Type gd = t.GetGenericTypeDefinition();

            if (gd == typeof(slice<>)) return Slice;
            if (gd == typeof(array<>)) return Array;
            if (gd == typeof(map<,>)) return Map;
            if (gd == typeof(channel<>)) return Chan;
        }

        // A ж box at any base-chain depth (fix W: today depth 0; a per-kind subclass under B1).
        if (TryBoxPointee(t, out _)) return Pointer;

        // A NAMED container type (`type S []byte`, `type M map[K]V`, `type P *T`, ...) is a
        // generated wrapper struct/class implementing the golib container interface — classify
        // STRUCTURALLY, never by parsing the [GoType] definition token (a converter-rendered C#
        // type string). Order matters: ISlice derives from IArray, so slices probe first.
        if (typeof(ISlice).IsAssignableFrom(t)) return Slice;
        if (typeof(IMap).IsAssignableFrom(t)) return Map;
        if (typeof(IChannel).IsAssignableFrom(t)) return Chan;
        if (typeof(IArray).IsAssignableFrom(t)) return Array;
        if (typeof(INilPointer).IsAssignableFrom(t) && !t.IsValueType) return Pointer;

        if (typeof(Delegate).IsAssignableFrom(t)) return Func;

        if (t.IsInterface) return Interface;

        // A converted Go struct is a [GoType] value type; anything else value-typed still reports Struct.
        if (t.IsValueType) return Struct;

        // REFERENCE-typed and none of the above. go2cs emits every Go struct as a C# VALUE type, so
        // this can never be a Go struct: it is an opaque managed handle — the backing object a
        // hand-owned shim holds in place of Go's own representation, `sync.Mutex`'s `SemaphoreSlim`
        // gate being the demonstrated case — and in the Go model a handle is one pointer word.
        //
        // Answering Struct here was the classification defect behind the only process-KILLING failure
        // mode this bridge has had. Struct is the one kind whose walks descend into the type's fields,
        // so it sent GoSizeOf/GoAlignOf (and StructFieldsComparable) into the CLR's OWN private fields
        // and from there into the BCL object graph, which is cyclic where a Go type graph cannot be:
        // `Named -> Mutex -> SemaphoreSlim -> TaskNode -> TaskNode` exhausted the stack in go/types'
        // TestSizeof and took the 44 verdicts alphabetically after it with the process. Reported as a
        // pointer the descent stops at the handle, which is both finite and Go's own answer — a Go
        // `sync.Mutex` is 8 bytes and so, now, is the converted one.
        return Pointer;
    }

    /// <summary>
    /// Reports whether values of the Go type that <paramref name="t"/> represents are comparable
    /// (usable with <c>==</c> / as a map key). Mirrors Go's rule: slices, maps and funcs are not
    /// comparable, nor is any struct or array that (transitively) contains one; every other kind —
    /// bool, numbers, string, pointer, channel, interface, unsafe.Pointer — is. The reflection bridge
    /// uses this to populate <c>abi.Type.Equal</c> on a synthesized descriptor, which both
    /// <c>reflect.Type.Comparable</c> and <c>internal/reflectlite</c>'s <c>Comparable</c> read as their
    /// comparability signal (<c>Equal != nil</c>) — and <c>errors.Is</c> gates its equality match on the
    /// latter, so a wrong answer here makes <c>errors.Is(err, sentinel)</c> silently return false.
    /// </summary>
    public static bool IsComparable(Type? t)
    {
        if (t is null)
            return false;

        switch (KindOf(t))
        {
            case Slice:
            case Map:
            case Func:
                return false;
            case Array:
                return IsComparable(ElementType(t));
            case Struct:
                return StructFieldsComparable(t);
            default:
                return true;
        }
    }

    // A struct is comparable iff every field is (Go). Recurses over the converted [GoType] struct's
    // instance fields; a field of slice/map/func kind — or a nested struct/array that contains one —
    // makes the whole struct non-comparable. Pointer/interface/chan fields stay comparable without
    // recursing into their referents.
    //
    // Termination rests on KindOf, not on Go: only the Struct and Array kinds recurse, Struct is now
    // answered for VALUE types alone, and C# forbids a value type from containing itself directly or
    // transitively (CS0523). Go's "no struct contains itself by value" rule says the same thing about
    // the source language, but it is the C# rule that binds here — the walk reads managed metadata,
    // and it was a managed reference classified as Struct that once made this descend forever.
    private static bool StructFieldsComparable(Type t)
    {
        foreach (FieldInfo f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (!IsComparable(f.FieldType))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Identifies a generated interface-implementation adapter class and recovers the Go dynamic
    /// type it stands in for: a pointer-sourced adapter (<see cref="IжAdapter"/>, wrapping the
    /// receiver box <c>ж&lt;T&gt;</c>) yields <c>T</c> with <paramref name="pointerSourced"/>
    /// <c>true</c>; a value-sourced foreign adapter (<c>{Struct}ᴠ{Iface}</c>, wrapping a copy)
    /// yields the struct type. Both are recovered from the adapter's single one-parameter
    /// constructor, so no name parsing of the wrapped type is involved.
    /// </summary>
    public static bool TryAdapterWrappedType(Type? t, [NotNullWhen(true)] out Type? wrapped, out bool pointerSourced)
    {
        wrapped = null;
        pointerSourced = false;

        if (t is null || t.IsValueType)
            return false;

        if (typeof(IжAdapter).IsAssignableFrom(t))
        {
            // The template's only constructor takes the wrapped receiver box ж<T>.
            foreach (ConstructorInfo ctor in t.GetConstructors())
            {
                ParameterInfo[] parameters = ctor.GetParameters();

                if (parameters.Length == 1 && parameters[0].ParameterType is { IsGenericType: true } boxType &&
                    boxType.GetGenericTypeDefinition() == typeof(ж<>))
                {
                    wrapped = boxType.GetGenericArguments()[0];
                    pointerSourced = true;
                    return true;
                }
            }

            return false;
        }

        // A value-sourced adapter declares IValueAdapter; its only constructor takes the wrapped
        // struct by value. (This was a NAME probe for the ᴠ infix plus a package-class nesting
        // check until the marker existed — the marker is exact where the name was a heuristic, and
        // it is the same signal the equality / type-assert / type-switch paths now gate on.)
        if (!typeof(IValueAdapter).IsAssignableFrom(t))
            return false;

        foreach (ConstructorInfo ctor in t.GetConstructors())
        {
            ParameterInfo[] parameters = ctor.GetParameters();

            if (parameters.Length == 1 && parameters[0].ParameterType.IsValueType)
            {
                wrapped = parameters[0].ParameterType;
                return true;
            }
        }

        return false;
    }

    private static readonly ConcurrentDictionary<Type, GoTypeAttribute?> s_goTypeMarkers = new();

    /// <summary>The type's own <c>[GoType]</c> marker, or <c>null</c> when it carries none.</summary>
    /// <remarks>
    /// Memoized per type, like <c>s_dynamicTypes</c> in <c>runtime/TypeExtensions</c> and for the
    /// same reason: which attributes a type declares is an IMMUTABLE fact about the type, but every
    /// read materializes fresh attribute instances — measured against live golib at 361 ns and
    /// 200 bytes for this one probe, and the reads sit under callers that cache nothing of their own
    /// (<see cref="KindOf"/> under every <c>reflect.ValueOf</c>/<c>Value.Field</c>/<c>Value.Elem</c>,
    /// <see cref="TryConvertTo"/> and <see cref="TryUnwrapWrapperValue"/> under the whole
    /// <c>Value.Set</c>/<c>SetMapIndex</c>/<c>Call</c>/<c>Convert</c> marshalling surface), so the
    /// cost was paid per VALUE rather than per type. Deliberately NOT registered with any
    /// assembly-load cache clear: unlike an extension-method scan, a loaded type's own attributes
    /// cannot change. Both markers are <c>AllowMultiple=false</c>, so the single declared attribute
    /// is the whole answer.
    /// </remarks>
    private static GoTypeAttribute? goTypeMarkerOf(Type t)
    {
        return s_goTypeMarkers.GetOrAdd(t, static type =>
            type.GetCustomAttributes(typeof(GoTypeAttribute), false) is [GoTypeAttribute marker] ? marker : null);
    }

    /// <summary>
    /// The Go element type of a managed container <see cref="Type"/> — <c>slice&lt;T&gt;</c>/
    /// <c>array&lt;T&gt;</c>/<c>channel&lt;T&gt;</c>/<c>ж&lt;T&gt;</c> → <c>T</c>, <c>map&lt;K,V&gt;</c> → <c>V</c>
    /// — for <c>reflect.Type.Elem()</c>; <c>null</c> if <paramref name="t"/> has no element type.
    /// </summary>
    public static Type? ElementType(Type? t)
    {
        if (t is null) return null;

        // A pointer-sourced adapter class stands for *T — its element type is T (R10), matching
        // the KindOf/GoTypeName unwrap.
        if (TryAdapterWrappedType(t, out Type? adapterWrapped, out bool adapterPointerSourced) && adapterPointerSourced)
            return adapterWrapped;

        if (t.IsGenericType)
        {
            Type gd = t.GetGenericTypeDefinition();
            Type[] a = t.GetGenericArguments();

            if (gd == typeof(map<,>)) return a[1];
            if (gd == typeof(slice<>) || gd == typeof(array<>) || gd == typeof(channel<>) || gd == typeof(ж<>)) return a[0];
        }

        // A NAMED container wrapper answers through the golib container interface it implements
        // (`type S []byte` → byte); a named pointer wrapper through IPointer<T>.
        if (ContainerInterfaceArguments(t, typeof(IMap<,>)) is { } mapArgs) return mapArgs[1];
        if (ContainerInterfaceArguments(t, typeof(ISlice<>)) is { } sliceArgs) return sliceArgs[0];
        if (ContainerInterfaceArguments(t, typeof(IArray<>)) is { } arrayArgs) return arrayArgs[0];
        if (ContainerInterfaceArguments(t, typeof(IChannel<>)) is { } chanArgs) return chanArgs[0];
        if (!t.IsValueType && ContainerInterfaceArguments(t, typeof(IPointer<>)) is { } ptrArgs) return ptrArgs[0];

        return null;
    }

    /// <summary>
    /// The Go KEY type of a map — <c>map&lt;K,V&gt;</c> (or a named map wrapper) → <c>K</c>;
    /// <c>null</c> if <paramref name="t"/> is not a map type. For <c>reflect.Type.Key()</c>.
    /// </summary>
    public static Type? KeyType(Type? t)
    {
        if (t is null) return null;

        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(map<,>))
            return t.GetGenericArguments()[0];

        return ContainerInterfaceArguments(t, typeof(IMap<,>)) is { } mapArgs ? mapArgs[0] : null;
    }

    // Resolves the closed generic container interface a named wrapper implements
    // (ISlice<T>/IMap<K,V>/IArray<T>/IChannel<T>/IPointer<T>) and returns its type arguments,
    // or null. The raw golib containers are matched by open generic definition BEFORE this is
    // consulted, so this only ever answers for generated wrapper types.
    private static Type[]? ContainerInterfaceArguments(Type t, Type genericInterfaceDefinition)
    {
        foreach (Type ifc in t.GetInterfaces())
        {
            if (ifc.IsGenericType && ifc.GetGenericTypeDefinition() == genericInterfaceDefinition)
                return ifc.GetGenericArguments();
        }

        return null;
    }

    // Reads a [GoType] definition string ("num:int32", "@string", "num:uintptr", ...) and maps its
    // underlying-kind token to a reflect.Kind. Returns false when the type carries no such definition
    // (a plain [GoType] struct, or a non-converted type).
    private static bool TryGoTypeDefinitionKind(Type t, out int kind)
    {
        kind = Invalid;

        if (goTypeMarkerOf(t) is not { } marker)
            return false;

        string def = marker.Definition;

        if (string.IsNullOrEmpty(def))
            return false; // a plain struct/interface marker — not a named-underlying wrapper

        string token = def.StartsWith("num:", StringComparison.Ordinal) ? def[4..] : def;

        kind = token switch
        {
            "bool" => Bool,
            // The converter renders the def token in the C# SPELLING of the underlying type, which
            // is not always the Go one, so every spelling that reaches here must map:
            //   * Go int/uint  -> nint/nuint   (num:nint is what `type X int` emits)
            //   * Go byte/rune -> byte/rune    (the predeclared aliases keep their own spelling,
            //     matching the `uint8`/`int32` aliases the generated csprojs declare)
            // A missing spelling is silent and broad: the wrapper falls through to Struct, so the
            // whole reflection bridge — fmt's %v, DeepEqual, encoding/* — sees a one-field struct.
            // `type nb byte` printed "{144}" instead of "144" (NarrowShiftVarCount).
            "int" or "nint" => Int, "int8" => Int8, "int16" => Int16, "int32" or "rune" => Int32, "int64" => Int64,
            "uint" or "nuint" => Uint, "uint8" or "byte" => Uint8, "uint16" => Uint16, "uint32" => Uint32, "uint64" => Uint64,
            "uintptr" => Uintptr,
            "float32" => Float32, "float64" => Float64,
            "complex64" => Complex64, "complex128" => Complex128,
            "@string" or "string" => String,
            _ => Invalid
        };

        return kind != Invalid;
    }
}
