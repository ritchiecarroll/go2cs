using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;
using Δruntime = go.runtime_package;

namespace GolibTests;

// A failed type assertion panicked with a string built in golib ("interface conversion: interface {} is
// int, not string"). Go panics with a *runtime.TypeAssertionError (iface.go panicdottypeE / panicdottypeI,
// error.go): recover() yields a value that satisfies runtime.Error and error, whose Error() text is the same
// sentence. golib sits under the runtime and cannot name the type, so it raises through
// RuntimeErrorPanic.TypeAssertion, and the runtime registers the constructor (panicvalues_impl.cs).
[TestClass]
public class RuntimeTypeAssertionTests
{
    [ClassInitialize]
    public static void RunRuntimeModuleInitializer(TestContext _) =>
        RuntimeHelpers.RunModuleConstructor(typeof(Δruntime).Module.ModuleHandle);

    private static object? PanicValue(Action fn)
    {
        object? recovered = null;
        GoFrame frame = default;

        try
        {
            frame.Push(() => recovered = recover());
            fn();
        }
        catch (Exception ex) when (GoFrame.IsPanic(ex, out PanicException? p))
        {
            GoFrame.Capture(p);
        }
        finally
        {
            frame.Run();
        }

        return recovered;
    }

    private static void AssertTypeAssertionError(object? recovered, string want, string site)
    {
        Assert.IsNotNull(recovered, $"{site}: did not panic");
        Assert.AreEqual("*runtime.TypeAssertionError", GetGoTypeName(recovered), $"{site}: the recovered value's Go type");

        Assert.IsTrue(recovered._<Δruntime.ΔError>(out Δruntime.ΔError? runtimeError),
            $"{site}: recovered value {recovered} (type {recovered.GetType().FullName}) does not implement runtime.Error");

        Assert.AreEqual(want, runtimeError!.Error().ToString(), $"{site}: runtime.Error text");
        Assert.IsTrue(recovered._<error>(out error? _), $"{site}: does not implement error");
    }

    [TestMethod]
    public void AConcreteMismatchFromAnAnyIsATypeAssertionError()
    {
        object boxed = (nint)7;
        AssertTypeAssertionError(PanicValue(() => boxed._<@string>()),
            "interface conversion: interface {} is int, not string", "int asserted to string");
    }

    [TestMethod]
    public void ANilSourceIsATypeAssertionError()
    {
        object? nothing = null;
        AssertTypeAssertionError(PanicValue(() => nothing!._<@string>()),
            "interface conversion: interface {} is nil, not string", "nil asserted to string");
    }

    [TestMethod]
    public void AnErrorSourceIsATypeAssertionError()
    {
        error err = go.errors_package.New("boom");
        AssertTypeAssertionError(PanicValue(() => err._(typeof(@string))),
            "interface conversion: error is *errors.errorString, not string", "error asserted to string");
    }

    [TestMethod]
    public void AnAssertionToAnInterfaceIsATypeAssertionError()
    {
        object boxed = (nint)7;
        AssertTypeAssertionError(PanicValue(() => boxed._<Δruntime.ΔError>()),
            "interface conversion: interface {} is int, not runtime.Error", "int asserted to runtime.Error");
    }
}
