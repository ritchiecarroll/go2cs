// builtin.DeferRegistrations.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;

namespace go;

// ---------------------------------------------------------------------------------------------
// DEFER REGISTRATIONS — every `defer` overload, i.e. the C# spelling of Go's `defer f(a, b, …)`.
//
// WHAT LIVES HERE
//   An ARITY LADDER of `defer<T1…Tn>(Action<T1…Tn>, T1 arg1, …, ref GoFrame frame, bool recovers = false)` rungs from 1
//   to 16 parameters, each with a `Func<…, TResult>` twin that discards the result, plus a
//   nullary rung. Every rung has the same one-line body: push a closure onto the frame.
//
// WHY THE TRAILING `ref GoFrame` PARAMETER
//   Go's `defer` is scoped to the ENCLOSING FUNCTION, so something has to name which function is
//   registering. That something is the function's own GoFrame local — the ref struct holding this
//   call's defer list, declared beside an inline body inside try/catch/finally (see GoFrame.cs).
//   It is passed by reference because a ref struct cannot be captured and must not be copied.
//
// WHY A LADDER AND NOT ONE VARIADIC METHOD
//   Same reason as the goroutine ladder: Go evaluates a deferred call's arguments AT THE `defer`
//   STATEMENT and runs the call later, so every argument must be captured at registration time. A
//   `params object[]` form would do it while boxing every value argument — on a path that runs on
//   function exit throughout the corpus. The generic rungs capture without boxing and let the JIT
//   see the concrete delegate, so the ladder is a deliberate performance shape, not clutter.
//
// WHY THE NAME IS PLAIN `defer` AND NOT `deferǃ`
//   It was `deferǃ` (U+01C3, a legal C# identifier character where `!` is not) for exactly one
//   reason: the retired execution-context lambda took a `defer`-named DELEGATE PARAMETER, and the
//   two could not share a name in one scope. There is no such parameter now, `defer` is a Go
//   KEYWORD so no Go identifier is ever spelled that way, and `defer` is not a C# keyword — so the
//   bang bought nothing and is gone. Its siblings keep theirs: `goǃ` cannot be `go`, the root
//   namespace of every converted file, and `makeǃ` cannot be `make`, a predeclared Go identifier
//   a package may shadow.
//
// WHY THE TRAILING `recovers` PARAMETER
//   Go's recover works only when called DIRECTLY by the deferred function, and whether a function is the
//   deferred one depends on how it was called. A defer whose callee calls recover() passes `recovers: true`, and
//   the rung registers its call through GoFrame.Recovering, which marks the thread just before the call; the
//   callee's first line takes the mark (GoFrame.TakeDeferredCall). Every other defer passes nothing and is
//   registered exactly as before.

// EXTENDING IT
//   Add a rung only when a corpus package actually needs that arity, and add BOTH forms — the
//   `Action` rung and the result-discarding `Func` twin — or a value-returning method group will
//   fail to bind at exactly that arity (CS0407). Keep the bodies mechanical.
// ---------------------------------------------------------------------------------------------
public static partial class builtin
{
    /// <summary>
    /// Registers a deferred call taking no arguments.
    /// </summary>
    /// <param name="action">Deferred call.</param>
    /// <param name="frame">Registering function's frame.</param>
    public static void defer(Action action, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(action, recovers));
    }

    /// <summary>
    /// Registers a deferred call with 1 parameter, evaluated now and passed on exit.
    /// </summary>
    /// <typeparam name="T">First parameter type.</typeparam>
    /// <param name="action">Deferred call.</param>
    /// <param name="arg">First parameter.</param>
    /// <param name="frame">Registering function's frame.</param>
    public static void defer<T>(Action<T> action, T arg, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => action(arg), recovers));
    }

    /// <summary>Registers a deferred call to a value-returning function, discarding the result.</summary>
    public static void defer<T, TResult>(Func<T, TResult> action, T arg, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => { action(arg); }, recovers));
    }

    /// <summary>
    /// Registers a deferred call with 2 parameters, evaluated now and passed on exit.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <param name="action">Deferred call.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="frame">Registering function's frame.</param>
    public static void defer<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => action(arg1, arg2), recovers));
    }

    /// <summary>Registers a deferred call to a value-returning function, discarding the result.</summary>
    public static void defer<T1, T2, TResult>(Func<T1, T2, TResult> action, T1 arg1, T2 arg2, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => { action(arg1, arg2); }, recovers));
    }

    /// <summary>
    /// Registers a deferred call with 3 parameters, evaluated now and passed on exit.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <param name="action">Deferred call.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="frame">Registering function's frame.</param>
    public static void defer<T1, T2, T3>(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => action(arg1, arg2, arg3), recovers));
    }

    /// <summary>Registers a deferred call to a value-returning function, discarding the result.</summary>
    public static void defer<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> action, T1 arg1, T2 arg2, T3 arg3, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => { action(arg1, arg2, arg3); }, recovers));
    }

    /// <summary>
    /// Registers a deferred call with 4 parameters, evaluated now and passed on exit.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <param name="action">Deferred call.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="frame">Registering function's frame.</param>
    public static void defer<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => action(arg1, arg2, arg3, arg4), recovers));
    }

    /// <summary>Registers a deferred call to a value-returning function, discarding the result.</summary>
    public static void defer<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => { action(arg1, arg2, arg3, arg4); }, recovers));
    }

    /// <summary>
    /// Registers a deferred call with 5 parameters, evaluated now and passed on exit.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <param name="action">Deferred call.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="frame">Registering function's frame.</param>
    public static void defer<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => action(arg1, arg2, arg3, arg4, arg5), recovers));
    }

    /// <summary>Registers a deferred call to a value-returning function, discarding the result.</summary>
    public static void defer<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => { action(arg1, arg2, arg3, arg4, arg5); }, recovers));
    }

    /// <summary>
    /// Registers a deferred call with 6 parameters, evaluated now and passed on exit.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <param name="action">Deferred call.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="frame">Registering function's frame.</param>
    public static void defer<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => action(arg1, arg2, arg3, arg4, arg5, arg6), recovers));
    }

    /// <summary>Registers a deferred call to a value-returning function, discarding the result.</summary>
    public static void defer<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => { action(arg1, arg2, arg3, arg4, arg5, arg6); }, recovers));
    }

    /// <summary>
    /// Registers a deferred call with 7 parameters, evaluated now and passed on exit.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <param name="action">Deferred call.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="frame">Registering function's frame.</param>
    public static void defer<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7), recovers));
    }

    /// <summary>Registers a deferred call to a value-returning function, discarding the result.</summary>
    public static void defer<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => { action(arg1, arg2, arg3, arg4, arg5, arg6, arg7); }, recovers));
    }

    /// <summary>
    /// Registers a deferred call with 8 parameters, evaluated now and passed on exit.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <param name="action">Deferred call.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="frame">Registering function's frame.</param>
    public static void defer<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8), recovers));
    }

    /// <summary>Registers a deferred call to a value-returning function, discarding the result.</summary>
    public static void defer<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => { action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8); }, recovers));
    }

    /// <summary>
    /// Registers a deferred call with 9 parameters, evaluated now and passed on exit.
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
    /// <param name="action">Deferred call.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="frame">Registering function's frame.</param>
    public static void defer<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9), recovers));
    }

    /// <summary>Registers a deferred call to a value-returning function, discarding the result.</summary>
    public static void defer<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => { action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9); }, recovers));
    }

    /// <summary>
    /// Registers a deferred call with 10 parameters, evaluated now and passed on exit.
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
    /// <param name="action">Deferred call.</param>
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
    /// <param name="frame">Registering function's frame.</param>
    public static void defer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10), recovers));
    }

    /// <summary>Registers a deferred call to a value-returning function, discarding the result.</summary>
    public static void defer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => { action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10); }, recovers));
    }

    /// <summary>
    /// Registers a deferred call with 11 parameters, evaluated now and passed on exit.
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
    /// <param name="action">Deferred call.</param>
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
    /// <param name="frame">Registering function's frame.</param>
    public static void defer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11), recovers));
    }

    /// <summary>Registers a deferred call to a value-returning function, discarding the result.</summary>
    public static void defer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => { action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11); }, recovers));
    }

    /// <summary>
    /// Registers a deferred call with 12 parameters, evaluated now and passed on exit.
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
    /// <param name="action">Deferred call.</param>
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
    /// <param name="frame">Registering function's frame.</param>
    public static void defer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12), recovers));
    }

    /// <summary>Registers a deferred call to a value-returning function, discarding the result.</summary>
    public static void defer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => { action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12); }, recovers));
    }

    /// <summary>
    /// Registers a deferred call with 13 parameters, evaluated now and passed on exit.
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
    /// <param name="action">Deferred call.</param>
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
    /// <param name="frame">Registering function's frame.</param>
    public static void defer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13), recovers));
    }

    /// <summary>Registers a deferred call to a value-returning function, discarding the result.</summary>
    public static void defer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => { action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13); }, recovers));
    }

    /// <summary>
    /// Registers a deferred call with 14 parameters, evaluated now and passed on exit.
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
    /// <param name="action">Deferred call.</param>
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
    /// <param name="frame">Registering function's frame.</param>
    public static void defer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14), recovers));
    }

    /// <summary>Registers a deferred call to a value-returning function, discarding the result.</summary>
    public static void defer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => { action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14); }, recovers));
    }

    /// <summary>
    /// Registers a deferred call with 15 parameters, evaluated now and passed on exit.
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
    /// <param name="action">Deferred call.</param>
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
    /// <param name="frame">Registering function's frame.</param>
    public static void defer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15), recovers));
    }

    /// <summary>Registers a deferred call to a value-returning function, discarding the result.</summary>
    public static void defer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => { action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15); }, recovers));
    }

    /// <summary>
    /// Registers a deferred call with 16 parameters, evaluated now and passed on exit.
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
    /// <param name="action">Deferred call.</param>
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
    /// <param name="frame">Registering function's frame.</param>
    public static void defer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16), recovers));
    }

    /// <summary>Registers a deferred call to a value-returning function, discarding the result.</summary>
    public static void defer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, ref GoFrame frame, bool recovers = false)
    {
        frame.Push(GoFrame.Recovering(() => { action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16); }, recovers));
    }
}
