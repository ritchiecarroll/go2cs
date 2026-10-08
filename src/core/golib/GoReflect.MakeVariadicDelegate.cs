// GoReflect.MakeVariadicDelegate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Reflection;

namespace go;

// ---------------------------------------------------------------------------------------------
// reflect.MakeFunc of a VARIADIC func type -- the reverse of InvokeVariadic's typed family.
//
// A converted Go variadic func lowers to an Actionꓸꓸꓸ/Funcꓸꓸꓸ delegate whose tail is `params
// Span<TArg>` (variadic.cs). MakeGoFuncDelegate's expression-tree factory cannot carry that
// byref-like parameter, so a LAMBDA in a typed makeVariadicFunc{N}/makeVariadicAction{N}
// trampoline carries it instead: the Span tail is packed into a slice<TArg> (Go's variadic
// collection -- `f(1, 2, 3)` -> `[]int{2, 3}`) and handed to the invoker as the last argument,
// the exact shape the invoker's `ins` describes (NumIn fixed params, the variadic one a slice).
// The families stop at MaxVariadicFixedParameters (8), like the forward call family they mirror.
// ---------------------------------------------------------------------------------------------
public static partial class GoReflect
{
    // The per-delegate-type factory for a variadic func delegate -- the reverse of
    // buildVariadicInvoker. Mirrors its type-argument extraction exactly (the family's type
    // arguments ARE the delegate's own parameter types), then closes the matching make-trampoline.
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2060", Justification = GoTypeRegistry.RegisteredGenericJustification)]
    internal static Func<Func<object?[], object?>, Delegate> BuildVariadicMakeFactory(
        Type delegateType, MethodInfo invoke, ParameterInfo[] parameters)
    {
        int fixedCount = parameters.Length - 1;
        Type tailParameter = parameters[^1].ParameterType;

        if (fixedCount < 0 || !tailParameter.IsGenericType || tailParameter.GetGenericTypeDefinition() != typeof(Span<>))
        {
            throw new NotImplementedException(
                $"reflect: MakeFunc of '{GoTypeName(delegateType)}' -- its tail is not a Span<T> variadic tail");
        }

        if (fixedCount > MaxVariadicFixedParameters)
        {
            throw new NotImplementedException(
                $"reflect: MakeFunc of a variadic func with {fixedCount} fixed parameters is not implemented -- " +
                $"golib's Actionꓸꓸꓸ/Funcꓸꓸꓸ families stop at {MaxVariadicFixedParameters} (no demonstrated consumer beyond that)");
        }

        bool hasResult = invoke.ReturnType != typeof(void);

        Type[] typeArguments = new Type[fixedCount + (hasResult ? 2 : 1)];
        for (int i = 0; i < fixedCount; i++)
            typeArguments[i] = parameters[i].ParameterType;
        typeArguments[fixedCount] = tailParameter.GetGenericArguments()[0];
        if (hasResult)
            typeArguments[^1] = invoke.ReturnType;

        MethodInfo trampoline = typeof(GoReflect)
            .GetMethod($"{(hasResult ? "makeVariadicFunc" : "makeVariadicAction")}{fixedCount}",
                BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(typeArguments);

        return trampoline.CreateDelegate<Func<Func<object?[], object?>, Delegate>>();
    }

    // The make family: a lambda per (arity, Action/Func) that carries the Span<TArg> tail a
    // delegate factory cannot, packs it into slice<TArg>, and forwards to the invoker -- the
    // reverse of callVariadicFunc{N}/callVariadicAction{N} one file over.

    private static Delegate makeVariadicFunc0<TArg, TResult>(Func<object?[], object?> invoker)
    { return (Funcꓸꓸꓸ<TArg, TResult>)((Span<TArg> tail) => (TResult)invoker([new slice<TArg>(tail)])!); }

    private static Delegate makeVariadicFunc1<T1, TArg, TResult>(Func<object?[], object?> invoker)
    { return (Funcꓸꓸꓸ<T1, TArg, TResult>)((T1 a1, Span<TArg> tail) => (TResult)invoker([a1, new slice<TArg>(tail)])!); }

    private static Delegate makeVariadicFunc2<T1, T2, TArg, TResult>(Func<object?[], object?> invoker)
    { return (Funcꓸꓸꓸ<T1, T2, TArg, TResult>)((T1 a1, T2 a2, Span<TArg> tail) => (TResult)invoker([a1, a2, new slice<TArg>(tail)])!); }

    private static Delegate makeVariadicFunc3<T1, T2, T3, TArg, TResult>(Func<object?[], object?> invoker)
    { return (Funcꓸꓸꓸ<T1, T2, T3, TArg, TResult>)((T1 a1, T2 a2, T3 a3, Span<TArg> tail) => (TResult)invoker([a1, a2, a3, new slice<TArg>(tail)])!); }

    private static Delegate makeVariadicFunc4<T1, T2, T3, T4, TArg, TResult>(Func<object?[], object?> invoker)
    { return (Funcꓸꓸꓸ<T1, T2, T3, T4, TArg, TResult>)((T1 a1, T2 a2, T3 a3, T4 a4, Span<TArg> tail) => (TResult)invoker([a1, a2, a3, a4, new slice<TArg>(tail)])!); }

    private static Delegate makeVariadicFunc5<T1, T2, T3, T4, T5, TArg, TResult>(Func<object?[], object?> invoker)
    { return (Funcꓸꓸꓸ<T1, T2, T3, T4, T5, TArg, TResult>)((T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, Span<TArg> tail) => (TResult)invoker([a1, a2, a3, a4, a5, new slice<TArg>(tail)])!); }

    private static Delegate makeVariadicFunc6<T1, T2, T3, T4, T5, T6, TArg, TResult>(Func<object?[], object?> invoker)
    { return (Funcꓸꓸꓸ<T1, T2, T3, T4, T5, T6, TArg, TResult>)((T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, Span<TArg> tail) => (TResult)invoker([a1, a2, a3, a4, a5, a6, new slice<TArg>(tail)])!); }

    private static Delegate makeVariadicFunc7<T1, T2, T3, T4, T5, T6, T7, TArg, TResult>(Func<object?[], object?> invoker)
    { return (Funcꓸꓸꓸ<T1, T2, T3, T4, T5, T6, T7, TArg, TResult>)((T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, Span<TArg> tail) => (TResult)invoker([a1, a2, a3, a4, a5, a6, a7, new slice<TArg>(tail)])!); }

    private static Delegate makeVariadicFunc8<T1, T2, T3, T4, T5, T6, T7, T8, TArg, TResult>(Func<object?[], object?> invoker)
    { return (Funcꓸꓸꓸ<T1, T2, T3, T4, T5, T6, T7, T8, TArg, TResult>)((T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, Span<TArg> tail) => (TResult)invoker([a1, a2, a3, a4, a5, a6, a7, a8, new slice<TArg>(tail)])!); }

    private static Delegate makeVariadicAction0<TArg>(Func<object?[], object?> invoker)
    { return (Actionꓸꓸꓸ<TArg>)((Span<TArg> tail) => invoker([new slice<TArg>(tail)])); }

    private static Delegate makeVariadicAction1<T1, TArg>(Func<object?[], object?> invoker)
    { return (Actionꓸꓸꓸ<T1, TArg>)((T1 a1, Span<TArg> tail) => invoker([a1, new slice<TArg>(tail)])); }

    private static Delegate makeVariadicAction2<T1, T2, TArg>(Func<object?[], object?> invoker)
    { return (Actionꓸꓸꓸ<T1, T2, TArg>)((T1 a1, T2 a2, Span<TArg> tail) => invoker([a1, a2, new slice<TArg>(tail)])); }

    private static Delegate makeVariadicAction3<T1, T2, T3, TArg>(Func<object?[], object?> invoker)
    { return (Actionꓸꓸꓸ<T1, T2, T3, TArg>)((T1 a1, T2 a2, T3 a3, Span<TArg> tail) => invoker([a1, a2, a3, new slice<TArg>(tail)])); }

    private static Delegate makeVariadicAction4<T1, T2, T3, T4, TArg>(Func<object?[], object?> invoker)
    { return (Actionꓸꓸꓸ<T1, T2, T3, T4, TArg>)((T1 a1, T2 a2, T3 a3, T4 a4, Span<TArg> tail) => invoker([a1, a2, a3, a4, new slice<TArg>(tail)])); }

    private static Delegate makeVariadicAction5<T1, T2, T3, T4, T5, TArg>(Func<object?[], object?> invoker)
    { return (Actionꓸꓸꓸ<T1, T2, T3, T4, T5, TArg>)((T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, Span<TArg> tail) => invoker([a1, a2, a3, a4, a5, new slice<TArg>(tail)])); }

    private static Delegate makeVariadicAction6<T1, T2, T3, T4, T5, T6, TArg>(Func<object?[], object?> invoker)
    { return (Actionꓸꓸꓸ<T1, T2, T3, T4, T5, T6, TArg>)((T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, Span<TArg> tail) => invoker([a1, a2, a3, a4, a5, a6, new slice<TArg>(tail)])); }

    private static Delegate makeVariadicAction7<T1, T2, T3, T4, T5, T6, T7, TArg>(Func<object?[], object?> invoker)
    { return (Actionꓸꓸꓸ<T1, T2, T3, T4, T5, T6, T7, TArg>)((T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, Span<TArg> tail) => invoker([a1, a2, a3, a4, a5, a6, a7, new slice<TArg>(tail)])); }

    private static Delegate makeVariadicAction8<T1, T2, T3, T4, T5, T6, T7, T8, TArg>(Func<object?[], object?> invoker)
    { return (Actionꓸꓸꓸ<T1, T2, T3, T4, T5, T6, T7, T8, TArg>)((T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, Span<TArg> tail) => invoker([a1, a2, a3, a4, a5, a6, a7, a8, new slice<TArg>(tail)])); }

}
