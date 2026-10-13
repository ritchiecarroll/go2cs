// builtin.GoroutineLaunchers.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using go.golib;

namespace go;

// ---------------------------------------------------------------------------------------------
// GOROUTINE LAUNCHERS — every `goǃ` overload, i.e. the C# spelling of Go's `go f(a, b, …)`.
//
// WHAT LIVES HERE
//   `goǃ(Action)` and `goǃ(WaitCallback)` for the no-argument case, then an ARITY LADDER of
//   `goǃ<T1…Tn>(Action<T1…Tn>, T1 arg1, …)` rungs from 1 to 16 parameters, each paired with a
//   `Func<…, TResult>` twin that discards the result (Go lets you `go` a value-returning call and
//   throws the value away). Every rung has the same one-line body: hand a closure to
//   `Goroutine.Start`.
//
// WHY A LADDER AND NOT ONE VARIADIC METHOD
//   Go evaluates a goroutine's arguments AT THE `go` STATEMENT and passes them by value; only the
//   call itself is deferred to the new goroutine. Reproducing that in C# means capturing each
//   argument at the launch site. A single `goǃ(Delegate, params object[])` would do it — at the
//   cost of boxing every value argument and a reflective invoke, on a path converted code hits in
//   hot loops. The generic rungs stay allocation-light (one closure, no boxing) and let the JIT
//   see the concrete delegate, so the ladder is a deliberate performance shape, not clutter.
//
// THE CALLER-INFO PARAMETERS
//   Every rung ends in `[CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0`, which the
//   C# compiler fills in with the `go` statement's own position, so converted code passes nothing. They name the `go`
//   SITE, whose creator Goroutine resolves once and caches: walking the stack at every `go` made a goroutine's creation
//   cost grow with its creator's stack depth (go-cmp's 65,536-level cyclic Transformer test: 386 s, Go 0.3 s).
//
// EXTENDING IT
//   Go's own limit is the language's, not ours: 16 rungs simply exceeds anything real code passes
//   to one call. Add rung 17 only if a corpus package actually needs it, and add BOTH forms — the
//   `Action` rung and the result-discarding `Func` twin — or the converter will find one shape
//   missing at exactly the arity it needs. Keep the bodies mechanical; anything with real logic in
//   it belongs in builtin.cs or in `Goroutine` itself.
// ---------------------------------------------------------------------------------------------
public static partial class builtin
{
    /// <summary>
    /// Execute Go routine.
    /// </summary>
    /// <param name="action">Routine to execute.</param>
    // The following is basically "go!". We use the Unicode bang-type character
    // as a valid C# identifier symbol, where the standard "!" is not. This is
    // to disambiguate the method name from the namespace.
    public static void goǃ(WaitCallback action, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => action(null), action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>
    /// Execute Go routine.
    /// </summary>
    /// <param name="action">Routine to execute.</param>
    // The following is basically "go!". We use the Unicode bang-type character
    // as a valid C# identifier symbol, where the standard "!" is not. This is
    // to disambiguate the method name from the namespace.
    public static void goǃ(Action action, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(action, ᴛcallerFile, ᴛcallerLine);
    }

    // ** Go Routine Handlers with Parameters **

    /// <summary>
    /// Executes a Go routine with one parameter.
    /// </summary>
    /// <typeparam name="T">First parameter type.</typeparam>
    /// <param name="action">Target defer action.</param>
    /// <param name="arg">First parameter.</param>
    public static void goǃ<T>(Action<T> action, T arg, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => action(arg), action, ᴛcallerFile, ᴛcallerLine);
    }

    #region [ goǃ<T1, T2, ... T16> Implementations ]

    /// <summary>
    /// Executes a Go routine with two parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <param name="action">Target defer action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    public static void goǃ<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => action(arg1, arg2), action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>
    /// Executes a Go routine with three parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    public static void goǃ<T1, T2, T3>(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => action(arg1, arg2, arg3), action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>
    /// Executes a Go routine with four parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    public static void goǃ<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => action(arg1, arg2, arg3, arg4), action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>
    /// Executes a Go routine with five parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => action(arg1, arg2, arg3, arg4, arg5), action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>
    /// Executes a Go routine with six parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => action(arg1, arg2, arg3, arg4, arg5, arg6), action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>
    /// Executes a Go routine with seven parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7), action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>
    /// Executes a Go routine with eight parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8), action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>
    /// Executes a Go routine with nine parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9), action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>
    /// Executes a Go routine with ten parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <typeparam name="T10">Tenth parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="arg10">Tenth parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10), action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>
    /// Executes a Go routine with eleven parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <typeparam name="T10">Tenth parameter type.</typeparam>
    /// <typeparam name="T11">Eleventh parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="arg10">Tenth parameter.</param>
    /// <param name="arg11">Eleventh parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11), action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>
    /// Executes a Go routine with twelve parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <typeparam name="T10">Tenth parameter type.</typeparam>
    /// <typeparam name="T11">Eleventh parameter type.</typeparam>
    /// <typeparam name="T12">Twelfth parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="arg10">Tenth parameter.</param>
    /// <param name="arg11">Eleventh parameter.</param>
    /// <param name="arg12">Twelfth parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12), action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>
    /// Executes a Go routine with thirteen parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <typeparam name="T10">Tenth parameter type.</typeparam>
    /// <typeparam name="T11">Eleventh parameter type.</typeparam>
    /// <typeparam name="T12">Twelfth parameter type.</typeparam>
    /// <typeparam name="T13">Thirteenth parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="arg10">Tenth parameter.</param>
    /// <param name="arg11">Eleventh parameter.</param>
    /// <param name="arg12">Twelfth parameter.</param>
    /// <param name="arg13">Thirteenth parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13), action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>
    /// Executes a Go routine with fourteen parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <typeparam name="T10">Tenth parameter type.</typeparam>
    /// <typeparam name="T11">Eleventh parameter type.</typeparam>
    /// <typeparam name="T12">Twelfth parameter type.</typeparam>
    /// <typeparam name="T13">Thirteenth parameter type.</typeparam>
    /// <typeparam name="T14">Fourteenth parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="arg10">Tenth parameter.</param>
    /// <param name="arg11">Eleventh parameter.</param>
    /// <param name="arg12">Twelfth parameter.</param>
    /// <param name="arg13">Thirteenth parameter.</param>
    /// <param name="arg14">Fourteenth parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14), action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>
    /// Executes a Go routine with fifteen parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <typeparam name="T10">Tenth parameter type.</typeparam>
    /// <typeparam name="T11">Eleventh parameter type.</typeparam>
    /// <typeparam name="T12">Twelfth parameter type.</typeparam>
    /// <typeparam name="T13">Thirteenth parameter type.</typeparam>
    /// <typeparam name="T14">Fourteenth parameter type.</typeparam>
    /// <typeparam name="T15">Fifteenth parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="arg10">Tenth parameter.</param>
    /// <param name="arg11">Eleventh parameter.</param>
    /// <param name="arg12">Twelfth parameter.</param>
    /// <param name="arg13">Thirteenth parameter.</param>
    /// <param name="arg14">Fourteenth parameter.</param>
    /// <param name="arg15">Fifteenth parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15), action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>
    /// Executes a Go routine with sixteen parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <typeparam name="T10">Tenth parameter type.</typeparam>
    /// <typeparam name="T11">Eleventh parameter type.</typeparam>
    /// <typeparam name="T12">Twelfth parameter type.</typeparam>
    /// <typeparam name="T13">Thirteenth parameter type.</typeparam>
    /// <typeparam name="T14">Fourteenth parameter type.</typeparam>
    /// <typeparam name="T15">Fifteenth parameter type.</typeparam>
    /// <typeparam name="T16">Sixteenth parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="arg10">Tenth parameter.</param>
    /// <param name="arg11">Eleventh parameter.</param>
    /// <param name="arg12">Twelfth parameter.</param>
    /// <param name="arg13">Thirteenth parameter.</param>
    /// <param name="arg14">Fourteenth parameter.</param>
    /// <param name="arg15">Fifteenth parameter.</param>
    /// <param name="arg16">Sixteenth parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16), action, ᴛcallerFile, ᴛcallerLine);
    }


    /// <summary>
    /// Starts a goroutine on a VALUE-RETURNING function, discarding the result - Go discards the
    /// return values of `go f(...)` exactly as it does a deferred call's. Without these Func twins a
    /// value-returning literal or method group cannot bind the Action overloads above (CS8934 /
    /// CS0407; net's `go func(ln Listener) (retErr error) {…}(ln)`). Mirrors the defer Func twins,
    /// including their deliberate omission of a NULLARY twin: `Func&lt;TResult&gt;` could never win
    /// overload resolution against the non-generic <c>goǃ(Action)</c> - a non-generic candidate beats
    /// a generic one - so a nullary value-returning callee is the converter's discarding wrap to
    /// handle (<c>goǃ(() =&gt; nib())</c>), exactly as it is for defer.
    /// </summary>
    public static void goǃ<T, TResult>(Func<T, TResult> action, T arg, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => { action(arg); }, action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>Starts a goroutine on a value-returning function, discarding the result.</summary>
    public static void goǃ<T1, T2, TResult>(Func<T1, T2, TResult> action, T1 arg1, T2 arg2, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => { action(arg1, arg2); }, action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>Starts a goroutine on a value-returning function, discarding the result.</summary>
    public static void goǃ<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> action, T1 arg1, T2 arg2, T3 arg3, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => { action(arg1, arg2, arg3); }, action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>Starts a goroutine on a value-returning function, discarding the result.</summary>
    public static void goǃ<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => { action(arg1, arg2, arg3, arg4); }, action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>Starts a goroutine on a value-returning function, discarding the result.</summary>
    public static void goǃ<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => { action(arg1, arg2, arg3, arg4, arg5); }, action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>Starts a goroutine on a value-returning function, discarding the result.</summary>
    public static void goǃ<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => { action(arg1, arg2, arg3, arg4, arg5, arg6); }, action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>Starts a goroutine on a value-returning function, discarding the result.</summary>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => { action(arg1, arg2, arg3, arg4, arg5, arg6, arg7); }, action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>Starts a goroutine on a value-returning function, discarding the result.</summary>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => { action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8); }, action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>Starts a goroutine on a value-returning function, discarding the result.</summary>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => { action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9); }, action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>Starts a goroutine on a value-returning function, discarding the result.</summary>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => { action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10); }, action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>Starts a goroutine on a value-returning function, discarding the result.</summary>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => { action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11); }, action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>Starts a goroutine on a value-returning function, discarding the result.</summary>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => { action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12); }, action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>Starts a goroutine on a value-returning function, discarding the result.</summary>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => { action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13); }, action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>Starts a goroutine on a value-returning function, discarding the result.</summary>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => { action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14); }, action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>Starts a goroutine on a value-returning function, discarding the result.</summary>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => { action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15); }, action, ᴛcallerFile, ᴛcallerLine);
    }

    /// <summary>Starts a goroutine on a value-returning function, discarding the result.</summary>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.Start(() => { action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16); }, action, ᴛcallerFile, ᴛcallerLine);
    }

    #endregion
}
