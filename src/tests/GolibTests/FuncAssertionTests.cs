// FuncAssertionTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

#nullable enable

using System;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;

namespace GolibTests;

// A value-sourced interface value over a delegate, as AdapterImplTemplate emits it for a named func type with methods.
internal sealed class funcAssertValueAdapter(object? value) : IValueAdapter
{
    public object? Value => value;
}

// An interface created from an interface: Go's dynamic value is the one it wraps.
internal sealed class funcAssertInterfaceAdapter(object? value) : IInterfaceAdapter
{
    public object? Value => value;
}

/// <summary>
/// A func-type assertion is emitted through <c>_ᶠ</c>, whose delegate type argument carries no trim annotation. It
/// must answer exactly what the general <c>_&lt;T&gt;</c> answers for a delegate T, in every form: each value below is
/// asserted to a matching and a mismatching delegate by both, and the results compared, a failure's panic text
/// included.
/// </summary>
[TestClass]
public class FuncAssertionTests
{
    private static readonly Action s_live = () => { };
    private static readonly Func<nint, nint> s_double = x => x * 2;

    private static object?[] Values()
    {
        Action noAction = null!;
        Func<nint, nint> noFunc = null!;

        return
        [
            s_live,
            s_double,
            null,
            noAction.OrTypedNilFunc(),
            noFunc.OrTypedNilFunc(),
            new funcAssertInterfaceAdapter(s_live),
            new funcAssertInterfaceAdapter(new funcAssertInterfaceAdapter(s_double)),
            new funcAssertValueAdapter(s_live),
            new funcAssertValueAdapter(s_double),
            (@string)"go",
            (nint)5
        ];
    }

    private static void Same<T>(object? value) where T : Delegate
    {
        string label = $"{value?.GetType().Name ?? "nil"} to {typeof(T).Name}";

        (T? general, bool generalOk) = value!._<T>(ᐧ);
        (T? func, bool funcOk) = value!._ᶠ<T>(ᐧ);
        Assert.AreEqual(generalOk, funcOk, $"comma-ok, {label}");
        Assert.AreSame(general, func, $"comma-ok value, {label}");

        bool outGeneralOk = value!._<T>(out T? outGeneral);
        bool outFuncOk = value!._ᶠ<T>(out T? outFunc);
        Assert.AreEqual(outGeneralOk, outFuncOk, $"out form, {label}");
        Assert.AreSame(outGeneral, outFunc, $"out value, {label}");

        Assert.AreEqual(Single(() => value!._<T>()), Single(() => value!._ᶠ<T>()), $"single value, {label}");

        error? nilError = null;
        Assert.AreEqual(Single(() => nilError!._<error, T>()), Single(() => nilError!._ᶠ<error, T>()), $"named source, {label}");
    }

    private static string Single<T>(Func<T> assert)
    {
        try
        {
            return $"value {assert()?.GetHashCode()}";
        }
        catch (Exception ex)
        {
            return $"{ex.GetType().Name}: {ex.Message}";
        }
    }

    [TestMethod]
    public void EveryFormAnswersWhatTheGeneralAssertionAnswers()
    {
        foreach (object? value in Values())
        {
            Same<Action>(value);
            Same<Func<nint, nint>>(value);
        }
    }

    [TestMethod]
    public void AFailureNamesTheSourceTheFuncTypeAndTheDynamicType()
    {
        string anySource = Single(() => ((object)(nint)5)._ᶠ<Action>());
        StringAssert.Contains(anySource, "interface {} is int, not func()");

        error? nilError = null;
        string errorSource = Single(() => nilError!._ᶠ<error, Action>());
        StringAssert.Contains(errorSource, "error is nil, not func()");
    }

    [TestMethod]
    public void ATypedNilFuncAssertsToItsOwnTypeAlone()
    {
        Action none = null!;
        object nilFunc = none.OrTypedNilFunc();

        Assert.IsTrue(nilFunc._ᶠ<Action>(out Action? func));
        Assert.IsNull(func);
        Assert.IsFalse(nilFunc._ᶠ<Func<nint, nint>>(out _));
    }
}
