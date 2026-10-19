// MustExtensions.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace go;

/// <summary>
/// The bridge from Go's errors-as-values to C#'s exceptions, for C# code that calls converted Go:
/// <c>Must()</c> returns a Go call's values when its <c>error</c> is nil and throws
/// <see cref="GoErrorException"/> when it is not.
/// </summary>
/// <remarks>
/// <para>
/// A converted Go function that returns <c>(T, error)</c> returns a C# tuple, so
/// <c>nint port = strconv.Atoi(text).Must();</c> is Go's
/// <c>port, err := strconv.Atoi(text); if err != nil { panic(err) }</c>. The name is Go's: like
/// <c>template.Must</c> and <c>regexp.MustCompile</c>, a failed <c>Must</c> PANICS with the error, which is
/// why <see cref="GoErrorException"/> is a <see cref="PanicException"/>.
/// </para>
/// <para>
/// These are C#-side conveniences only. The converter never emits them, so no converted code changes.
/// An error is nil as Go defines it: a non-nil interface that holds a nil pointer is NOT nil, and
/// <c>Must</c> throws for it, as Go's <c>err != nil</c> is true for it.
/// </para>
/// <para>
/// One overload per shape a converted Go function returns: a bare <c>error</c>, and one to three values
/// before the <c>error</c>. A Go function with more results is rare enough to deconstruct by hand.
/// </para>
/// </remarks>
public static class MustExtensions
{
    /// <summary>
    /// Throws <see cref="GoErrorException"/> when <paramref name="err"/> is not nil.
    /// </summary>
    /// <param name="err">A Go error result.</param>
    /// <exception cref="GoErrorException"><paramref name="err"/> is not nil.</exception>
    public static void Must(this error err)
    {
        if (err is not null)
            throw new GoErrorException(err);
    }

    /// <summary>
    /// Returns the value of a Go <c>(T, error)</c> result, or throws <see cref="GoErrorException"/>
    /// when its error is not nil.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="result">A Go <c>(T, error)</c> result.</param>
    /// <returns>The value, when the error is nil.</returns>
    /// <exception cref="GoErrorException">The error is not nil.</exception>
    public static T Must<T>(this (T value, error err) result)
    {
        if (result.err is not null)
            throw new GoErrorException(result.err);

        return result.value;
    }

    /// <summary>
    /// Returns the values of a Go <c>(T1, T2, error)</c> result, or throws
    /// <see cref="GoErrorException"/> when its error is not nil.
    /// </summary>
    /// <typeparam name="T1">Type of the first value.</typeparam>
    /// <typeparam name="T2">Type of the second value.</typeparam>
    /// <param name="result">A Go <c>(T1, T2, error)</c> result.</param>
    /// <returns>The values, when the error is nil.</returns>
    /// <exception cref="GoErrorException">The error is not nil.</exception>
    public static (T1, T2) Must<T1, T2>(this (T1 value1, T2 value2, error err) result)
    {
        if (result.err is not null)
            throw new GoErrorException(result.err);

        return (result.value1, result.value2);
    }

    /// <summary>
    /// Returns the values of a Go <c>(T1, T2, T3, error)</c> result, or throws
    /// <see cref="GoErrorException"/> when its error is not nil.
    /// </summary>
    /// <typeparam name="T1">Type of the first value.</typeparam>
    /// <typeparam name="T2">Type of the second value.</typeparam>
    /// <typeparam name="T3">Type of the third value.</typeparam>
    /// <param name="result">A Go <c>(T1, T2, T3, error)</c> result.</param>
    /// <returns>The values, when the error is nil.</returns>
    /// <exception cref="GoErrorException">The error is not nil.</exception>
    public static (T1, T2, T3) Must<T1, T2, T3>(this (T1 value1, T2 value2, T3 value3, error err) result)
    {
        if (result.err is not null)
            throw new GoErrorException(result.err);

        return (result.value1, result.value2, result.value3);
    }
}
