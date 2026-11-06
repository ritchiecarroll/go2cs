using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeExtensions = go.golib.TypeExtensions;

namespace GolibTests;

// golib's own extension methods are C# helpers (GoShift's Rsh/Lsh on the builtin integer types, slice and Must
// helpers), never Go methods: Go's predeclared types have empty method sets. The extension-method scan skipped
// golib's satellites by the prefix "go.golib." but not golib itself, whose assembly name is "golib", so those
// helpers entered every method-set question (a builtin uint64 "had" Rsh) and cost every converted program a
// GetTypes() over golib at start-up.
[TestClass]
public class ExtensionScanScopeTests
{
    [TestMethod]
    public void TheScanHoldsNoMethodDeclaredInGolib()
    {
        Assembly golib = typeof(TypeExtensions).Assembly;

        MethodInfo[] fromGolib = [.. TypeExtensions.GetExtensionMethods()
            .Select(static entry => entry.Item1)
            .Where(method => method.DeclaringType?.Assembly == golib)];

        Assert.AreEqual(0, fromGolib.Length,
            $"golib's helpers are not Go methods, yet the scan holds {fromGolib.Length} of them, e.g. " +
            string.Join(", ", fromGolib.Take(3).Select(static method => $"{method.DeclaringType!.Name}.{method.Name}")));
    }

    [TestMethod]
    public void ABuiltinIntegerHasNoGoMethods()
    {
        // Go: the predeclared uint64 has an empty method set, so no interface with a method is satisfied by it.
        Assert.IsNull(TypeExtensions.GetExtensionMethod(typeof(ulong), "Rsh"),
            "the builtin uint64 resolved golib's GoShift.Rsh helper as a method");
    }
}
