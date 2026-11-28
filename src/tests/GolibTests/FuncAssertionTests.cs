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

// A value adapter over a NULL func: a nil named func converted to an interface. Its delegate type is the declared type of
// the m_value field AdapterImplTemplate gives it, which is how golib finds the typed nil func it stands for.
internal sealed class funcAssertNilValueAdapter : IValueAdapter
{
    private readonly Action? m_value = null;

    public object? Value => m_value;
}

// go2cs-gen's generic runtime shell over a NULL func, closed over the delegate type: a nil func asserted to an interface.
internal sealed class funcAssertNilShell<T>(T? value) : IInterfaceAdapter where T : Delegate
{
    public object? Value => value;
}

// A live error value, for the two-argument form's text with a dynamic type to name.
internal sealed class funcAssertError : error
{
    public @string Error() => "funcAssertError";
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
            new funcAssertNilValueAdapter(),
            new funcAssertNilShell<Action>(null),
            new funcAssertInterfaceAdapter(new funcAssertNilShell<Func<nint, nint>>(null)),
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

        error liveError = new funcAssertError();
        Assert.AreEqual(Single(() => liveError._<error, T>()), Single(() => liveError._ᶠ<error, T>()), $"named live source, {label}");
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

        // A live value names its dynamic type between the source and the func type.
        string liveSource = Single(() => ((error)new funcAssertError())._ᶠ<error, Action>());
        StringAssert.Matches(liveSource, new System.Text.RegularExpressions.Regex(@"error is \S*funcAssertError, not func\(\)"));
    }

    [TestMethod]
    public void AShellOverANullFuncStandsForItsTypedNilFunc()
    {
        // The value adapter's carrier: the nil Action it stands for asserts to Action alone, and fails to name it.
        Assert.IsTrue(new funcAssertNilValueAdapter()._ᶠ<Action>(out Action? viaValue));
        Assert.IsNull(viaValue);
        StringAssert.Contains(Single(() => ((object)new funcAssertNilValueAdapter())._ᶠ<Func<nint, nint>>()), "is func(), not func(int) int");

        // The interface shell's, through a second shell, on the one-argument form's panic path too.
        object shell = new funcAssertInterfaceAdapter(new funcAssertNilShell<Action>(null));
        Assert.IsTrue(shell._ᶠ<Action>(out Action? viaShell));
        Assert.IsNull(viaShell);
        StringAssert.Contains(Single(() => shell._ᶠ<Func<nint, nint>>()), "interface {} is func(), not func(int) int");
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
