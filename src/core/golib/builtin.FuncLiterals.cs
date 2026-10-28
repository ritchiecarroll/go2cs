// builtin.FuncLiterals.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;

namespace go;

// ---------------------------------------------------------------------------------------------
// FUNC LITERALS -- the `func` overloads an immediately-invoked Go function literal is spelled through.
//
// WHAT LIVES HERE
//   An ARITY LADDER of identity functions, one `Action` and one `Func<…, TResult>` rung for each of 0 to 16
//   parameters. Each returns the delegate it is given, so the converter writes Go's `func() int {…}()` as
//   `func(nint () => {…})()`: the result type where Go writes it, the parameters on the lambda, and the
//   call on the returned delegate (visual-parity train seat #27, F1; COORD's ruling of option A).
//
// WHY AN IDENTITY FUNCTION
//   A C# lambda cannot be invoked where it is written (CS0149); it must first become a delegate. The
//   converter used to cast it, `((Func<nint>)(() => {…}))()`. Passing it to a generic identity function
//   lets C# take the delegate type from the lambda itself: an explicitly typed lambda has a natural type,
//   and an explicit return type fixes TResult to the Go result exactly (`nint`, never C#'s `int`).
//
// WHY THE RUNGS NEVER COMPETE
//   A lambda with an explicit return type converts only to a `Func`, and a block lambda that returns no
//   value only to an `Action`, so for any one literal exactly one rung applies.
//
// THE NAME
//   `func` is a Go keyword, so no Go identifier is ever spelled that way, and it is not a C# keyword.
//
// EXTENSION RULE
//   One more rung per arity, both forms. A literal of more than 16 parameters keeps the cast form; the
//   converter does not route it here.
// ---------------------------------------------------------------------------------------------

public static partial class builtin
{
    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 0 parameter(s) and no result.</summary>
    public static Action func(Action f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 0 parameter(s) and a result.</summary>
    public static Func<TResult> func<TResult>(Func<TResult> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 1 parameter(s) and no result.</summary>
    public static Action<T1> func<T1>(Action<T1> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 1 parameter(s) and a result.</summary>
    public static Func<T1, TResult> func<T1, TResult>(Func<T1, TResult> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 2 parameter(s) and no result.</summary>
    public static Action<T1, T2> func<T1, T2>(Action<T1, T2> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 2 parameter(s) and a result.</summary>
    public static Func<T1, T2, TResult> func<T1, T2, TResult>(Func<T1, T2, TResult> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 3 parameter(s) and no result.</summary>
    public static Action<T1, T2, T3> func<T1, T2, T3>(Action<T1, T2, T3> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 3 parameter(s) and a result.</summary>
    public static Func<T1, T2, T3, TResult> func<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 4 parameter(s) and no result.</summary>
    public static Action<T1, T2, T3, T4> func<T1, T2, T3, T4>(Action<T1, T2, T3, T4> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 4 parameter(s) and a result.</summary>
    public static Func<T1, T2, T3, T4, TResult> func<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 5 parameter(s) and no result.</summary>
    public static Action<T1, T2, T3, T4, T5> func<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 5 parameter(s) and a result.</summary>
    public static Func<T1, T2, T3, T4, T5, TResult> func<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 6 parameter(s) and no result.</summary>
    public static Action<T1, T2, T3, T4, T5, T6> func<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 6 parameter(s) and a result.</summary>
    public static Func<T1, T2, T3, T4, T5, T6, TResult> func<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 7 parameter(s) and no result.</summary>
    public static Action<T1, T2, T3, T4, T5, T6, T7> func<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 7 parameter(s) and a result.</summary>
    public static Func<T1, T2, T3, T4, T5, T6, T7, TResult> func<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 8 parameter(s) and no result.</summary>
    public static Action<T1, T2, T3, T4, T5, T6, T7, T8> func<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 8 parameter(s) and a result.</summary>
    public static Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 9 parameter(s) and no result.</summary>
    public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> func<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 9 parameter(s) and a result.</summary>
    public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 10 parameter(s) and no result.</summary>
    public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 10 parameter(s) and a result.</summary>
    public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 11 parameter(s) and no result.</summary>
    public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 11 parameter(s) and a result.</summary>
    public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 12 parameter(s) and no result.</summary>
    public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 12 parameter(s) and a result.</summary>
    public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 13 parameter(s) and no result.</summary>
    public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 13 parameter(s) and a result.</summary>
    public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 14 parameter(s) and no result.</summary>
    public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 14 parameter(s) and a result.</summary>
    public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 15 parameter(s) and no result.</summary>
    public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 15 parameter(s) and a result.</summary>
    public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 16 parameter(s) and no result.</summary>
    public static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> f) => f;

    /// <summary>Returns <paramref name="f"/>: an immediately-invoked Go function literal of 16 parameter(s) and a result.</summary>
    public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> f) => f;
}
