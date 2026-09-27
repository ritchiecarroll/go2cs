using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Go 1.21's <c>panic(nil)</c>: recover() observes a <c>*runtime.PanicNilError</c> unless
/// GODEBUG=panicnil=1. golib sits below the runtime and cannot name PanicNilError, so golib's panic asks
/// a hook the runtime registers (panicvalues_impl.cs, the same inversion as IntegerDivideByZeroValue).
/// Before it, golib wrapped the nil itself: runtime's TestPanicNil read recover() = nil where Go reads a
/// *PanicNilError, and its check's re-panic of that nil killed the subtest as a nil dereference.
/// </summary>
[TestClass]
public class NilPanicHookTests
{
    private sealed class Marker;

    [TestMethod]
    public void ANilPanicTakesTheRegisteredNilPanicValue()
    {
        Func<object?>? saved = builtin.NilPanicValue;
        Marker marker = new();

        try
        {
            builtin.NilPanicValue = () => marker;

            Assert.AreSame(marker, builtin.panic(null!).State, "panic(nil) carries the value the runtime supplies");
            Assert.AreEqual((@string)"x", builtin.panic((@string)"x").State, "a non-nil panic value is untouched");
        }
        finally
        {
            builtin.NilPanicValue = saved;
        }
    }

    [TestMethod]
    public void WithNoRuntimeRegisteredANilPanicStaysNil()
    {
        Func<object?>? saved = builtin.NilPanicValue;

        try
        {
            builtin.NilPanicValue = null;
            Assert.IsNull(builtin.panic(null!).State, "a program that never loads the runtime keeps the pre-1.21 nil");
        }
        finally
        {
            builtin.NilPanicValue = saved;
        }
    }
}
