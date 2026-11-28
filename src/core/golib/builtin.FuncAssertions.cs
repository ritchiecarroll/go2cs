// builtin.FuncAssertions.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using go.golib;

namespace go;

// Type assertion to a FUNC type: `x.(F)`, `f, ok := x.(F)` and `case F:` where F is a Go func type, emitted as
// `x._ᶠ<F>()`, `x._ᶠ<F>(ᐧ)` and `ᴛ._ᶠ<F>(out var f)` (ᶠ marks a func value, as in go2cs-gen's `Sprintfᶠ`).
//
// The general `_<T>` annotates T with DynamicallyAccessedMembers(PublicMethods | ...), which an interface or struct
// target needs and a delegate does not: nothing here looks a delegate's members up. On a delegate T that annotation
// still reaches Delegate.CreateDelegate's public static overloads, which carry RequiresUnreferencedCode, so the trim
// analyzer flagged every func-type assertion in a converted program (IL2026 and IL2111, three of each a site), at
// build, since the converter's executable csproj sets PublishTrimmed. These forms carry no annotation and answer
// exactly what `_<T>` answers for a delegate T: the tiers of TryTypeAssert that a delegate can reach, in its order.
public static partial class builtin
{
    /// <summary>
    /// Type asserts <paramref name="target"/> as the func type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The delegate the Go func type converts to.</typeparam>
    /// <param name="target">Source value to type assert.</param>
    /// <returns><paramref name="target"/> as <typeparamref name="T"/>, if successful.</returns>
    public static T _ᶠ<T>(this object target) where T : Delegate
    {
        if (TryFuncAssert(target, out T? value))
            return value!;

        // A shell over a null func keeps its carrier: the panic names the func type, not a nil interface.
        while (target is IInterfaceAdapter interfaceAdapter)
            target = (interfaceAdapter.Value ?? GoReflect.NilFuncOf(interfaceAdapter))!;

        throw RuntimeErrorPanic.TypeAssertion(typeof(object), target, typeof(T));
    }

    /// <summary>
    /// Type asserts <paramref name="target"/>, whose Go static type is the interface <typeparamref name="TSource"/>, as
    /// the func type <typeparamref name="T"/>; a failure panics with that source type in the text, as Go's does.
    /// </summary>
    /// <typeparam name="TSource">Static interface type of <paramref name="target"/>.</typeparam>
    /// <typeparam name="T">The delegate the Go func type converts to.</typeparam>
    /// <param name="target">Source value to type assert.</param>
    /// <returns><paramref name="target"/> as <typeparamref name="T"/>, if successful.</returns>
    public static T _ᶠ<TSource, T>(this TSource target) where T : Delegate
    {
        if (TryFuncAssert(target, out T? value))
            return value!;

        throw RuntimeErrorPanic.TypeAssertion(typeof(TSource), target, typeof(T));
    }

    /// <summary>
    /// Attempts type assert of <paramref name="target"/> as the func type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The delegate the Go func type converts to.</typeparam>
    /// <param name="target">Source value to type assert.</param>
    /// <param name="_">Overload discriminator for different return type, <see cref="ꟷ"/>.</param>
    /// <returns>Tuple of <paramref name="target"/> as <typeparamref name="T"/> and success boolean.</returns>
    public static (T?, bool) _ᶠ<T>(this object target, bool _) where T : Delegate =>
        TryFuncAssert(target, out T? value) ? (value, true) : (null, false);

    /// <summary>
    /// Attempts type assert of <paramref name="target"/> as the func type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The delegate the Go func type converts to.</typeparam>
    /// <param name="target">Source value to type assert.</param>
    /// <param name="value">Value as <typeparamref name="T"/>, if successful; otherwise <c>null</c>, Go's nil func.</param>
    /// <returns><c>true</c> if type assertion was successful; otherwise, <c>false</c>.</returns>
    public static bool _ᶠ<T>(this object target, out T? value) where T : Delegate =>
        TryFuncAssert(target, out value);

    // TryTypeAssert's tiers for a delegate T, in its order, with the ones a delegate cannot reach left out: the Go
    // string arm (T is never @string), the interface tiers (T is never an interface) and the dynamic-struct copy (T is
    // never a value type). A success may carry null: Go's typed nil func, asserted to its own type.
    private static bool TryFuncAssert<T>(object? target, out T? value) where T : Delegate
    {
        bool isAdapter = target is IGoAdapter;

        // The Go dynamic value, never the wrapper; a shell over a null func stands for the typed nil func it was built from.
        while (isAdapter && target is IInterfaceAdapter interfaceAdapter)
        {
            target = interfaceAdapter.Value ?? GoReflect.NilFuncOf(interfaceAdapter);
            isAdapter = target is IGoAdapter;
        }

        // A nil interface asserts as a failure (Go: `f, ok := nil.(F)` is ok=false).
        if (target is null)
        {
            value = null;
            return false;
        }

        if (target is T typedTarget)
        {
            value = typedTarget;
            return true;
        }

        // A value adapter over a null func stands for its typed nil func.
        if (isAdapter && target is IValueAdapter { Value: null } && GoReflect.NilFuncOf(target) is { } behind)
        {
            target = behind;
            isAdapter = false;
        }

        // The canonical nil func asserts to exactly its own delegate type, yielding Go's nil func.
        if (target is NilFuncValue nilFunc)
        {
            value = null;
            return nilFunc.Type == typeof(T);
        }

        // An interface value made from a pointer or from a value this assembly cannot partial unwraps to its dynamic value.
        if (isAdapter && target is IжAdapter { Box: T box })
        {
            value = box;
            return true;
        }

        if (isAdapter && target is IValueAdapter { Value: T wrapped })
        {
            value = wrapped;
            return true;
        }

        value = null;
        return false;
    }
}
