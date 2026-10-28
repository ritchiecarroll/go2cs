// GoReflect.VariadicRegistry.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace go;

// ---------------------------------------------------------------------------------------------
// The compiled variadic trampolines: reflect's typed call (InvokeVariadic) and typed make
// (MakeFunc) of a variadic func value, each closed over one Actionꓸꓸꓸ/Funcꓸꓸꓸ instantiation by
// COMPILED code rather than by MakeGenericMethod.
//
// Both families close a generic trampoline over the delegate's own parameter types. The JIT does
// that for any types; Native AOT does it only for an instantiation it compiled, and a value type
// (go.@string, a converted struct, uintptr) gets no shared code, so `reflect.ValueOf(join).Call`
// on `func(string, ...string) string` threw NotSupportedException ("missing native code") where
// Go prints the joined string. go2cs-gen calls one of these for every variadic signature that can
// reach reflect from its compilation: a func value converted to an interface (the escape
// registry's module initializer) and a variadic method's arms (the IGoMethodSet part), so the
// instantiation is compiled and registered before reflect can hold the value. A signature that
// is not registered still closes by MakeGenericMethod: always under the JIT, and under Native
// AOT for reference-type arguments (shared code), the boundary docs/PLAN-golib-full-trim.md
// calls stage 3d.
// ---------------------------------------------------------------------------------------------
public static partial class GoReflect
{
    // One per family instantiation: the call and make trampolines, both functions of the type alone.
    private sealed record VariadicTrampolines(Func<Delegate, object?[], Array, object?> Call, Func<Func<object?[], object?>, Delegate> Make);

    private static readonly ConcurrentDictionary<Type, VariadicTrampolines> s_variadicTrampolines = new();

    private static void registerVariadic(Type familyType, Func<Delegate, object?[], Array, object?> call, Func<Func<object?[], object?>, Delegate> make)
    {
        if (!s_variadicTrampolines.ContainsKey(familyType))
            s_variadicTrampolines.TryAdd(familyType, new VariadicTrampolines(call, make));
    }

    // The registered trampolines of a family instantiation, or null when none was registered. A miss first runs the escape
    // registries loaded assemblies deferred (GoDeferredRegistrations.EscapeRegistry), which hold the registrations.
    private static VariadicTrampolines? registeredVariadic(Type familyType) =>
        s_variadicTrampolines.TryGetValue(familyType, out VariadicTrampolines? trampolines) ||
        GoDeferredRegistrations.EscapeRegistry.Run() && s_variadicTrampolines.TryGetValue(familyType, out trampolines)
            ? trampolines
            : null;

    // Whether compiled code registered the family instantiation; GolibTests reads it.
    internal static bool IsVariadicRegistered(Type familyType) => s_variadicTrampolines.ContainsKey(familyType);

    // The family instantiation of a variadic signature. Under Native AOT a value-type instantiation exists only when some
    // compiled code constructed it (a registration does); one that does not is refused by name.
    [UnconditionalSuppressMessage("Trimming", "IL2055", Justification = GoTypeRegistry.RegisteredGenericJustification)]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050", Justification = global::go.GoTypeRegistry.AotGuardedJustification)]
    private static Type variadicFamilyType(Type delegateType, Type familyDefinition, Type[] typeArguments)
    {
        try
        {
            return familyDefinition.MakeGenericType(typeArguments);
        }
        catch (NotSupportedException)
        {
            throw refuseVariadic(delegateType, familyDefinition, typeArguments);
        }
    }

    // An unregistered signature's trampoline, closed by MakeGenericMethod: always under the JIT, and under Native AOT
    // only over reference types, whose shared code exists. A value-type family is refused by name there, as a Go panic
    // naming the family and the registry, never the runtime's missing-native-code exception.
    [UnconditionalSuppressMessage("Trimming", "IL2060", Justification = GoTypeRegistry.RegisteredGenericJustification)]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050", Justification = global::go.GoTypeRegistry.AotGuardedJustification)]
    private static TDelegate closeVariadicTrampoline<TDelegate>(Type delegateType, Type familyDefinition, string trampoline, Type[] typeArguments)
        where TDelegate : Delegate
    {
        if (!golib.TypeExtensions.IsDynamicCodeSupported && System.Array.Exists(typeArguments, static argument => argument.IsValueType))
            throw refuseVariadic(delegateType, familyDefinition, typeArguments);

        try
        {
            return typeof(GoReflect).GetMethod(trampoline, BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(typeArguments).CreateDelegate<TDelegate>();
        }
        catch (NotSupportedException)
        {
            throw refuseVariadic(delegateType, familyDefinition, typeArguments);
        }
    }

    private static PanicException refuseVariadic(Type delegateType, Type familyDefinition, Type[] typeArguments)
    {
        string family = familyDefinition.Name[..familyDefinition.Name.IndexOf('`')];
        string kind = family.StartsWith("Func", StringComparison.Ordinal) ? "Func" : "Action";

        return builtin.panic($"go2cs: reflect cannot call or make the variadic func {GoTypeName(delegateType)}: its family " +
            $"{family}<{string.Join(", ", System.Array.ConvertAll(typeArguments, GoTypeName))}> has no compiled trampoline (no " +
            $"GoReflect.RegisterVariadic{kind} registration from go2cs-gen's variadic registry) and this runtime cannot generate code");
    }

    // Called by go2cs-gen's generated code only: each names one Actionꓸꓸꓸ/Funcꓸꓸꓸ instantiation, the
    // type arguments in the family's own order (fixed parameters, the tail's element, the result).

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RegisterVariadicFunc<TArg, TResult>() =>
        registerVariadic(typeof(Funcꓸꓸꓸ<TArg, TResult>), callVariadicFunc0<TArg, TResult>, makeVariadicFunc0<TArg, TResult>);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RegisterVariadicFunc<T1, TArg, TResult>() =>
        registerVariadic(typeof(Funcꓸꓸꓸ<T1, TArg, TResult>), callVariadicFunc1<T1, TArg, TResult>, makeVariadicFunc1<T1, TArg, TResult>);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RegisterVariadicFunc<T1, T2, TArg, TResult>() =>
        registerVariadic(typeof(Funcꓸꓸꓸ<T1, T2, TArg, TResult>), callVariadicFunc2<T1, T2, TArg, TResult>, makeVariadicFunc2<T1, T2, TArg, TResult>);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RegisterVariadicFunc<T1, T2, T3, TArg, TResult>() =>
        registerVariadic(typeof(Funcꓸꓸꓸ<T1, T2, T3, TArg, TResult>), callVariadicFunc3<T1, T2, T3, TArg, TResult>, makeVariadicFunc3<T1, T2, T3, TArg, TResult>);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RegisterVariadicFunc<T1, T2, T3, T4, TArg, TResult>() =>
        registerVariadic(typeof(Funcꓸꓸꓸ<T1, T2, T3, T4, TArg, TResult>), callVariadicFunc4<T1, T2, T3, T4, TArg, TResult>, makeVariadicFunc4<T1, T2, T3, T4, TArg, TResult>);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RegisterVariadicFunc<T1, T2, T3, T4, T5, TArg, TResult>() =>
        registerVariadic(typeof(Funcꓸꓸꓸ<T1, T2, T3, T4, T5, TArg, TResult>), callVariadicFunc5<T1, T2, T3, T4, T5, TArg, TResult>, makeVariadicFunc5<T1, T2, T3, T4, T5, TArg, TResult>);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RegisterVariadicFunc<T1, T2, T3, T4, T5, T6, TArg, TResult>() =>
        registerVariadic(typeof(Funcꓸꓸꓸ<T1, T2, T3, T4, T5, T6, TArg, TResult>), callVariadicFunc6<T1, T2, T3, T4, T5, T6, TArg, TResult>, makeVariadicFunc6<T1, T2, T3, T4, T5, T6, TArg, TResult>);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RegisterVariadicFunc<T1, T2, T3, T4, T5, T6, T7, TArg, TResult>() =>
        registerVariadic(typeof(Funcꓸꓸꓸ<T1, T2, T3, T4, T5, T6, T7, TArg, TResult>), callVariadicFunc7<T1, T2, T3, T4, T5, T6, T7, TArg, TResult>, makeVariadicFunc7<T1, T2, T3, T4, T5, T6, T7, TArg, TResult>);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RegisterVariadicFunc<T1, T2, T3, T4, T5, T6, T7, T8, TArg, TResult>() =>
        registerVariadic(typeof(Funcꓸꓸꓸ<T1, T2, T3, T4, T5, T6, T7, T8, TArg, TResult>), callVariadicFunc8<T1, T2, T3, T4, T5, T6, T7, T8, TArg, TResult>, makeVariadicFunc8<T1, T2, T3, T4, T5, T6, T7, T8, TArg, TResult>);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RegisterVariadicAction<TArg>() =>
        registerVariadic(typeof(Actionꓸꓸꓸ<TArg>), callVariadicAction0<TArg>, makeVariadicAction0<TArg>);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RegisterVariadicAction<T1, TArg>() =>
        registerVariadic(typeof(Actionꓸꓸꓸ<T1, TArg>), callVariadicAction1<T1, TArg>, makeVariadicAction1<T1, TArg>);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RegisterVariadicAction<T1, T2, TArg>() =>
        registerVariadic(typeof(Actionꓸꓸꓸ<T1, T2, TArg>), callVariadicAction2<T1, T2, TArg>, makeVariadicAction2<T1, T2, TArg>);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RegisterVariadicAction<T1, T2, T3, TArg>() =>
        registerVariadic(typeof(Actionꓸꓸꓸ<T1, T2, T3, TArg>), callVariadicAction3<T1, T2, T3, TArg>, makeVariadicAction3<T1, T2, T3, TArg>);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RegisterVariadicAction<T1, T2, T3, T4, TArg>() =>
        registerVariadic(typeof(Actionꓸꓸꓸ<T1, T2, T3, T4, TArg>), callVariadicAction4<T1, T2, T3, T4, TArg>, makeVariadicAction4<T1, T2, T3, T4, TArg>);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RegisterVariadicAction<T1, T2, T3, T4, T5, TArg>() =>
        registerVariadic(typeof(Actionꓸꓸꓸ<T1, T2, T3, T4, T5, TArg>), callVariadicAction5<T1, T2, T3, T4, T5, TArg>, makeVariadicAction5<T1, T2, T3, T4, T5, TArg>);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RegisterVariadicAction<T1, T2, T3, T4, T5, T6, TArg>() =>
        registerVariadic(typeof(Actionꓸꓸꓸ<T1, T2, T3, T4, T5, T6, TArg>), callVariadicAction6<T1, T2, T3, T4, T5, T6, TArg>, makeVariadicAction6<T1, T2, T3, T4, T5, T6, TArg>);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RegisterVariadicAction<T1, T2, T3, T4, T5, T6, T7, TArg>() =>
        registerVariadic(typeof(Actionꓸꓸꓸ<T1, T2, T3, T4, T5, T6, T7, TArg>), callVariadicAction7<T1, T2, T3, T4, T5, T6, T7, TArg>, makeVariadicAction7<T1, T2, T3, T4, T5, T6, T7, TArg>);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RegisterVariadicAction<T1, T2, T3, T4, T5, T6, T7, T8, TArg>() =>
        registerVariadic(typeof(Actionꓸꓸꓸ<T1, T2, T3, T4, T5, T6, T7, T8, TArg>), callVariadicAction8<T1, T2, T3, T4, T5, T6, T7, T8, TArg>, makeVariadicAction8<T1, T2, T3, T4, T5, T6, T7, T8, TArg>);
}
