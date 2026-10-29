// GoTypeRegistry.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace go;

/// <summary>
/// The members golib reaches by reflection on a Go type, and the registration that keeps them in a trimmed publish.
/// </summary>
/// <remarks>
/// <para>
/// golib reflects over a Go type it receives as a VALUE: <c>object.GetType()</c> on whatever a program hands to
/// <c>reflect</c>, <c>fmt</c>, a type assertion or an interface conversion. No annotation can follow that flow, so
/// under a full trim the trimmer keeps a type but not the field and constructor METADATA golib needs (measured,
/// docs/PLAN-golib-full-trim.md section 4: Native AOT stripped <c>@string</c>'s, <c>uintptr</c>'s and
/// <c>internal/cpu</c>'s <c>option</c>'s fields).
/// </para>
/// <para>
/// Every Go type is therefore REGISTERED: go2cs-gen emits, in each package class, an empty module initializer
/// carrying one <see cref="DynamicDependencyAttribute"/> per Go type it generates for (<see cref="StructMembers"/>,
/// <see cref="WrapperMembers"/> or <see cref="InterfaceMembers"/>), and <see cref="RegisterGolibTypes"/> below lists
/// golib's own. Both trimmers honor a dynamic dependency of a kept method, and a module initializer is always kept, so
/// nothing executes at run time. An interface adapter registers itself on its CONSTRUCTOR (its wrapped field and its
/// constructors, the two things golib reads of one), so an adapter a program never builds costs nothing. An attribute
/// on the TYPE does not do this: measured, Native AOT ignores it for this flow.
/// </para>
/// <para>
/// golib's reflection sites carry <c>UnconditionalSuppressMessage</c> with <see cref="RegisteredTypeJustification"/>
/// or <see cref="RegisteredGenericJustification"/>. What keeps the justification true: GenTests' registry guard (every
/// type the generator generates for is registered), GolibTests' corpus guard (every Go type and adapter in a loaded
/// converted assembly is registered), and at run time <see cref="GoFieldMetadata"/>, which refuses by name a struct
/// whose fields are missing. An assembly with no registration (hand-owned, or built before it) is kept whole under
/// <c>TrimMode=partial</c> and is refused by that tripwire under a full trim, exactly as before.
/// </para>
/// </remarks>
public static class GoTypeRegistry
{
    /// <summary>
    /// A Go struct: its fields, constructors and properties (field-level record members, a struct's zero value, the
    /// declared property an embed's tag reads), and the interfaces it declares.
    /// </summary>
    public const DynamicallyAccessedMemberTypes StructMembers =
        DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields |
        DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors |
        DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties |
        DynamicallyAccessedMemberTypes.Interfaces;

    /// <summary>
    /// A generated wrapper over another type (a named slice, map, channel, array, numeric, function or pointer): a
    /// struct's members plus its public methods, where its nil-equality operator lives.
    /// </summary>
    public const DynamicallyAccessedMemberTypes WrapperMembers = StructMembers | DynamicallyAccessedMemberTypes.PublicMethods;

    /// <summary>A Go interface: its methods and the interfaces it embeds (its Go type string).</summary>
    public const DynamicallyAccessedMemberTypes InterfaceMembers =
        DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.Interfaces;

    /// <summary>The justification golib's reflection sites over a Go type cite.</summary>
    public const string RegisteredTypeJustification =
        "The Type is a Go type reached from a Go value. Every Go type and interface adapter is registered for trimming " +
        "(GoTypeRegistry: go2cs-gen's per-package module initializer and golib's own list carry a DynamicDependency for " +
        "the members read here), the registry guards fail when one is missing, and GoFieldMetadata refuses by name a " +
        "struct whose fields were removed anyway.";

    /// <summary>The justification golib's run-time generic instantiations cite (trimming only; IL3050 stands).</summary>
    public const string RegisteredGenericJustification =
        "The generic definition made here is golib's own or the BCL's (a delegate, tuple or golib family, a golib " +
        "helper, a fixed-array wrapper's constructor), and its type arguments are Go types, every one registered " +
        "(GoTypeRegistry), so what a definition asks of its arguments is kept. Native AOT's dynamic-code question " +
        "(IL3050) is separate and is not answered here.";

    // Native AOT's dynamic-code question (IL3050), answered per site. golib builds with the AOT analyzer on
    // (golib.csproj EnableAotAnalyzer) and with no file-wide IL3050 pragma, so a NEW site that closes a generic over a
    // run-time type warns, and golib's 0-warning build refuses it. Every site that remains cites one of these. The census
    // of 1.26.10.2 seat #18 classified each, after NativeStructMarshal's leaf classifier (MakeGenericMethod over a
    // scalar) made a converted TLS program exit 2 in syscall.WSAIoctl under Native AOT.

    /// <summary>An IL3050 site with an AOT answer of its own.</summary>
    public const string AotGuardedJustification =
        "Guarded: generated code answers first, and the run-time instantiation runs only where dynamic code is supported " +
        "(RuntimeFeature.IsDynamicCodeSupported) or is caught, falling back or refusing by name (seat #18 census).";

    /// <summary>An IL3050 site only Go's reflect reaches.</summary>
    public const string AotReflectOnlyJustification =
        "Reached only through Go's reflect (Value.Convert, Value.Field or Method over a type with no generated arm, " +
        "FuncOf, StructOf, MakeFunc). Under Native AOT a value-type argument with no static code throws " +
        "NotSupportedException there: a stated limitation of reflect under AOT, not ordinary converted code (seat #18 census).";

    /// <summary>An IL3050 site whose type arguments are reference types.</summary>
    public const string AotReferenceTypeJustification =
        "The type arguments are reference types (interfaces, ж<T> boxes, object), whose instantiation runs on Native AOT's " +
        "shared canonical code, or a type whose MethodTable the live value already carries (seat #18 census).";

    /// <summary>An IL3050 site that constructs a type and compiles no code.</summary>
    public const string AotTypeOnlyJustification =
        "Type construction only, for a type a live value's own signature names (a variadic func's tail slice), so its " +
        "MethodTable is in the image; no generic code is closed (seat #18 census).";

    /// <summary>An IL3050 site no converted or generated code calls.</summary>
    public const string AotNotReachedJustification =
        "No converted or generated code calls this; it serves tests and hand-written callers on the JIT (seat #18 census).";

    /// <summary>GoTypeOps' one fallback, for a type no generated code names the operations of.</summary>
    public const string AotTypeOpsFallbackJustification =
        "Reached only for a type outside the builtin table with no generated IGoTypeOpsSource: go2cs-gen emits the source " +
        "for every converted struct whose package spells a pointer to it, the escape registry registers the operations of " +
        "another assembly's struct without it where a pointer to it escapes (seat #34), and a reference type's " +
        "instantiation runs on shared canonical code (trim stage 3c-2b). A value type no code names the operations of (a " +
        "struct whose own package reaches its pointer only through inference) answers NotSupportedException under Native " +
        "AOT (seat #18 census, site 2).";

    // golib's own Go types. Generic definitions are named open; GolibTests' registry guard lists what a closed
    // instantiation gets under Native AOT (docs/PLAN-golib-full-trim.md, stage 2).
    [ModuleInitializer]
    [DynamicDependency(StructMembers, typeof(@string))]
    [DynamicDependency(StructMembers, typeof(sstring))]
    [DynamicDependency(StructMembers, typeof(uintptr))]
    [DynamicDependency(StructMembers, typeof(complex64))]
    [DynamicDependency(StructMembers, typeof(EmptyStruct))]
    [DynamicDependency(WrapperMembers, typeof(slice<>))]
    [DynamicDependency(WrapperMembers, typeof(sslice<>))]
    [DynamicDependency(WrapperMembers, typeof(array<>))]
    [DynamicDependency(WrapperMembers, typeof(map<,>))]
    [DynamicDependency(WrapperMembers, typeof(channel<>))]
    [DynamicDependency(WrapperMembers, typeof(ж<>))]
    internal static void RegisterGolibTypes() { }
}
