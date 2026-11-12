// funcArity.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace go;

// A Go NON-variadic func type used as a value lowers to a plain `Func<…>`/`Action<…>`, emitted
// UNQUALIFIED so it binds whatever delegate of that arity is in scope. The BCL family stops at
// `System.Func` with 16 parameters and `System.Action` with 16; Go has no such ceiling, so a wider
// signature named nothing at all and failed with CS0305. Go's own `reflect` test suite drives one:
// `abi_test.go`'s `callArgsManyFloat64` takes a `func` of 20 parameters returning 19 results, which
// lowers to a `Func<…>` of 21 type arguments once the results collapse into a tuple.
//
// These continue the ladder from 17 through 24 parameters, declared in `go` rather than `System`,
// and they need NO converter change to be reached: C# name lookup is ARITY-aware, so a `Func<…>`
// written inside `namespace go.<pkg>` finds a `go.Func` only at an arity `go` actually declares and
// otherwise keeps walking out to the `using System;` that supplies the BCL's. That is also why
// every arity the BCL ALREADY has is deliberately absent here — declaring one would not merely
// duplicate it, it would shadow it corpus-wide (`go` is the nearer scope) and would be an outright
// CS0104 ambiguity anywhere both namespaces are imported side by side. The shapes below are
// `System.Func`/`System.Action`'s exactly, variance included, so the two halves of one ladder are
// indistinguishable at a call site.
//
// The ceiling is 24 because that is well past what Go code plausibly writes — `reflect`'s
// deliberately-extreme ABI probe sits at 20 — and each rung costs a type nothing references. Add
// rungs when a real signature needs them, in this file, contiguously.
//
// ONE rendering is NOT covered, and it needs the converter rather than this file: a Go type ALIAS
// whose target is a func type emits its `global using` right-hand side at COMPILATION scope, where
// the delegate name is rooted EXPLICITLY as `System.Func`/`System.Action` (see `delegateRoot` in
// `typeNameResolution.go`, and the `PackageAliasRootedTypeArgs` golden). Past arity 16 that names
// nothing, and no `go.Func` can be found from there. No package in the corpus reaches it today.

/// <summary>Represents a Go func type with 17 parameters and a result: <c>func(T1, …, T17) TResult</c>.</summary>
[EditorBrowsable(EditorBrowsableState.Never)] public delegate TResult Func<
    in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10,
    in T11, in T12, in T13, in T14, in T15, in T16, in T17, out TResult>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10,
    T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17);

/// <summary>Represents a Go func type with 18 parameters and a result: <c>func(T1, …, T18) TResult</c>.</summary>
[EditorBrowsable(EditorBrowsableState.Never)] public delegate TResult Func<
    in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10,
    in T11, in T12, in T13, in T14, in T15, in T16, in T17, in T18, out TResult>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10,
    T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18);

/// <summary>Represents a Go func type with 19 parameters and a result: <c>func(T1, …, T19) TResult</c>.</summary>
[EditorBrowsable(EditorBrowsableState.Never)] public delegate TResult Func<
    in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10,
    in T11, in T12, in T13, in T14, in T15, in T16, in T17, in T18, in T19, out TResult>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10,
    T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19);

/// <summary>Represents a Go func type with 20 parameters and a result: <c>func(T1, …, T20) TResult</c>.</summary>
[EditorBrowsable(EditorBrowsableState.Never)] public delegate TResult Func<
    in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10,
    in T11, in T12, in T13, in T14, in T15, in T16, in T17, in T18, in T19, in T20, out TResult>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10,
    T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20);

/// <summary>Represents a Go func type with 21 parameters and a result: <c>func(T1, …, T21) TResult</c>.</summary>
[EditorBrowsable(EditorBrowsableState.Never)] public delegate TResult Func<
    in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10,
    in T11, in T12, in T13, in T14, in T15, in T16, in T17, in T18, in T19, in T20,
    in T21, out TResult>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10,
    T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20,
    T21 arg21);

/// <summary>Represents a Go func type with 22 parameters and a result: <c>func(T1, …, T22) TResult</c>.</summary>
[EditorBrowsable(EditorBrowsableState.Never)] public delegate TResult Func<
    in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10,
    in T11, in T12, in T13, in T14, in T15, in T16, in T17, in T18, in T19, in T20,
    in T21, in T22, out TResult>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10,
    T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20,
    T21 arg21, T22 arg22);

/// <summary>Represents a Go func type with 23 parameters and a result: <c>func(T1, …, T23) TResult</c>.</summary>
[EditorBrowsable(EditorBrowsableState.Never)] public delegate TResult Func<
    in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10,
    in T11, in T12, in T13, in T14, in T15, in T16, in T17, in T18, in T19, in T20,
    in T21, in T22, in T23, out TResult>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10,
    T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20,
    T21 arg21, T22 arg22, T23 arg23);

/// <summary>Represents a Go func type with 24 parameters and a result: <c>func(T1, …, T24) TResult</c>.</summary>
[EditorBrowsable(EditorBrowsableState.Never)] public delegate TResult Func<
    in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10,
    in T11, in T12, in T13, in T14, in T15, in T16, in T17, in T18, in T19, in T20,
    in T21, in T22, in T23, in T24, out TResult>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10,
    T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20,
    T21 arg21, T22 arg22, T23 arg23, T24 arg24);

/// <summary>Represents a Go func type with 17 parameters and no result: <c>func(T1, …, T17)</c>.</summary>
[EditorBrowsable(EditorBrowsableState.Never)] public delegate void Action<
    in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10,
    in T11, in T12, in T13, in T14, in T15, in T16, in T17>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10,
    T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17);

/// <summary>Represents a Go func type with 18 parameters and no result: <c>func(T1, …, T18)</c>.</summary>
[EditorBrowsable(EditorBrowsableState.Never)] public delegate void Action<
    in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10,
    in T11, in T12, in T13, in T14, in T15, in T16, in T17, in T18>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10,
    T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18);

/// <summary>Represents a Go func type with 19 parameters and no result: <c>func(T1, …, T19)</c>.</summary>
[EditorBrowsable(EditorBrowsableState.Never)] public delegate void Action<
    in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10,
    in T11, in T12, in T13, in T14, in T15, in T16, in T17, in T18, in T19>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10,
    T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19);

/// <summary>Represents a Go func type with 20 parameters and no result: <c>func(T1, …, T20)</c>.</summary>
[EditorBrowsable(EditorBrowsableState.Never)] public delegate void Action<
    in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10,
    in T11, in T12, in T13, in T14, in T15, in T16, in T17, in T18, in T19, in T20>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10,
    T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20);

/// <summary>Represents a Go func type with 21 parameters and no result: <c>func(T1, …, T21)</c>.</summary>
[EditorBrowsable(EditorBrowsableState.Never)] public delegate void Action<
    in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10,
    in T11, in T12, in T13, in T14, in T15, in T16, in T17, in T18, in T19, in T20,
    in T21>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10,
    T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20,
    T21 arg21);

/// <summary>Represents a Go func type with 22 parameters and no result: <c>func(T1, …, T22)</c>.</summary>
[EditorBrowsable(EditorBrowsableState.Never)] public delegate void Action<
    in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10,
    in T11, in T12, in T13, in T14, in T15, in T16, in T17, in T18, in T19, in T20,
    in T21, in T22>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10,
    T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20,
    T21 arg21, T22 arg22);

/// <summary>Represents a Go func type with 23 parameters and no result: <c>func(T1, …, T23)</c>.</summary>
[EditorBrowsable(EditorBrowsableState.Never)] public delegate void Action<
    in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10,
    in T11, in T12, in T13, in T14, in T15, in T16, in T17, in T18, in T19, in T20,
    in T21, in T22, in T23>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10,
    T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20,
    T21 arg21, T22 arg22, T23 arg23);

/// <summary>Represents a Go func type with 24 parameters and no result: <c>func(T1, …, T24)</c>.</summary>
[EditorBrowsable(EditorBrowsableState.Never)] public delegate void Action<
    in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10,
    in T11, in T12, in T13, in T14, in T15, in T16, in T17, in T18, in T19, in T20,
    in T21, in T22, in T23, in T24>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10,
    T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20,
    T21 arg21, T22 arg22, T23 arg23, T24 arg24);
