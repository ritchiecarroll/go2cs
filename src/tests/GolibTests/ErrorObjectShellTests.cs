// ErrorObjectShellTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Reflection;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;

namespace GolibTests;

// Stand-ins for converted Go errors, one per receiver shape the binder resolves: a value receiver on a value type and
// a value receiver reached through a pointer. Their Error methods are Go-style extensions, which is how the binder
// finds a converted method. The types are unique to this class because the binder's decisions are process-wide and
// never cleared. The pointer receiver is ErrorShellCarrierEqualityTests' refErrTarget: a second ж-receiver Error in
// this assembly reorders the extension scan that error<T> over a box CLASS (error.As(new StandardBox<X>(...))) still
// takes its by-value method from, and that test's third shell then binds another type's Error and throws.
internal readonly struct objShellCode(int code)
{
    internal readonly int m_code = code;
}

internal struct objShellVal
{
    internal int N;
}

internal static class ErrorObjectShellTestExtensions
{
    internal static @string Error(this objShellCode e) => "code(" + e.m_code + ")";

    internal static @string Error(this objShellVal v) => "val(" + v.N + ")";
}

/// <summary>
/// <see cref="error"/>'s object shell: the <c>error</c> the binder builds without closing <see cref="error{T}"/> over
/// the dynamic value, which is the only <c>error</c> a Native AOT image can build for a type it did not compile
/// <see cref="error{T}"/> for ahead of time.
/// </summary>
/// <remarks>
/// Under the JIT the binder takes it first for a value-typed error, so the first test reaches it through the public
/// entry point. A pointer box takes <see cref="error{T}"/> first and falls to the object shell only when that cannot
/// be closed, which never happens under the JIT, so the pointer tests build the shell through the binder's own object
/// tier, the code Native AOT runs. Each shell is held to <see cref="error{T}"/>'s answers over the same value.
/// </remarks>
[TestClass]
public class ErrorObjectShellTests
{
    private static GoInterfaceShellAttribute Spec => typeof(error).GetCustomAttribute<GoInterfaceShellAttribute>()!;

    // The binder's object tier for (valueType, error), as Native AOT reaches it.
    private static error ObjectShell(Type valueType, object value)
    {
        MethodInfo build = typeof(AdapterBinder).GetMethod("BuildObjectShellFactory", BindingFlags.NonPublic | BindingFlags.Static)!;
        Func<object, object>? factory = (Func<object, object>?)build.Invoke(null, [valueType, Spec]);

        Assert.IsNotNull(factory, $"the object tier must bind {valueType.Name} to error");

        return (error)factory(value);
    }

    // error<T>'s and the object shell's %v/%T overload, read the same way for both.
    private static string Format(error shell, string format) =>
        (string)shell.GetType().GetMethod("ToString", [typeof(string), typeof(IFormatProvider)])!.Invoke(shell, [format, null])!;

    [TestMethod]
    public void ErrorDeclaresAnObjectShell()
    {
        Assert.IsNotNull(Spec.ObjectShell, "error must declare a shell that closes no generic type");
        Assert.IsFalse(Spec.ObjectShell!.ContainsGenericParameters);
        Assert.IsNotNull(Spec.ObjectShell.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
            [typeof(object), typeof(GoShellBinding)], null), "the binder finds the object shell by its (object, GoShellBinding) constructor");
    }

    [TestMethod]
    public void AValueTypedErrorAnswersAsErrorOfT()
    {
        object value = new objShellCode(7);

        Assert.IsTrue(AdapterBinder.TryCreate(value, typeof(error), out object? built), "a value with Error() is an error");

        error shell = (error)built;
        error generic = error.As(new objShellCode(7));

        Assert.AreSame(Spec.ObjectShell, shell.GetType(), "a value-typed error binds the object shell first");
        Assert.AreEqual("code(7)", shell.Error().ToString());
        Assert.AreSame(value, ((IInterfaceAdapter)shell).Value, "the shell holds the boxed value it was built over");
        Assert.IsTrue(AreEqual(shell, generic), "the object shell and error<T> over one Go value are one interface value");
        Assert.AreEqual(Format(generic, "T"), Format(shell, "T"));
        Assert.AreEqual(Format(generic, "v"), Format(shell, "v"));
    }

    [TestMethod]
    public void APointerReceiverErrorAnswersAsErrorOfT()
    {
        ж<refErrTarget> box = new StandardBox<refErrTarget>(new refErrTarget());

        error shell = ObjectShell(box.GetType(), box);
        error generic = error.As(box);

        Assert.AreSame(Spec.ObjectShell, shell.GetType());
        Assert.AreEqual("ref", shell.Error().ToString());
        Assert.AreSame(box, ((IInterfaceAdapter)shell).Value, "a pointer-backed shell carries the ж box, not the pointee");
        Assert.IsTrue(AreEqual(shell, generic), "the object shell and error<T> over one box are one Go pointer");
        Assert.AreEqual(Format(generic, "T"), Format(shell, "T"));
        Assert.AreEqual(Format(generic, "v"), Format(shell, "v"));
    }

    [TestMethod]
    public void AValueReceiverReachedThroughAPointerReadsThePointeeAtEachCall()
    {
        ж<objShellVal> box = new StandardBox<objShellVal>(new objShellVal { N = 1 });

        error shell = ObjectShell(box.GetType(), box);

        Assert.AreEqual("val(1)", shell.Error().ToString());

        // Go copies the pointee when the method is called, not when the interface value is made.
        box.Value.N = 2;

        Assert.AreEqual("val(2)", shell.Error().ToString());
        Assert.AreEqual(error.As(box).Error().ToString(), shell.Error().ToString());
        Assert.AreEqual(Format(error.As(box), "T"), Format(shell, "T"));
    }
}
