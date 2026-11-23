//******************************************************************************************************
//  GoschedParkTests.cs - Gbtc
//
//  The Gosched-park seat (COORD's ruling of 2026-11-21, docs/phase4/inbox/G/20261121T231225Z-COORD.md): in a twin,
//  runtime.Gosched is a park point. runtime is a hand-owned stdlib package with no public hand twin under L-min, so
//  the generator maps the call to golib's builtin.Goschedꞏtwin (TwinGenerator.IsGoschedIntrinsic).
//******************************************************************************************************

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static GenTests.TwinBoxTests;

namespace GenTests;

[TestClass]
public class GoschedParkTests
{
    // The runtime package's Gosched as the hand-owned package declares it (src/core/runtime/managed_impl.cs): a public
    // static method of go.runtime_package, with no twin of its own.
    private const string Runtime = """
        namespace go;

        public static partial class runtime_package
        {
            public static void Gosched() { }
        }
        """;

    // A spinner whose only park is runtime.Gosched, and one that also receives.
    private const string Spinners = """
            public static void spin(nint n)
            {
                for (nint i = 0; i < n; i++)
                    runtime_package.Gosched();
            }

            public static void spinRecv(nint n)
            {
                ᐸꟷ(ch);
                runtime_package.Gosched();
            }

        """;

    [TestMethod]
    public void ATwinsGoschedCallAwaitsGolibsGoschedTwin()
    {
        (string twins, Diagnostic[] errors) = RunWith(Runtime, null, Spinners);

        Assert.AreEqual(0, errors.Length, Errors(errors));

        // A function whose only park is Gosched becomes a twin (a state machine, not a forwarder), and awaits golib's twin.
        StringAssert.Contains(twins, "async global::go.golib.GoTask spinꞏtwin(nint n)", "Gosched is a park point: spin is twinned");
        StringAssert.Contains(twins, "await global::go.builtin.Goschedꞏtwin()", "the call awaits builtin.Goschedꞏtwin in place");
        Assert.IsFalse(twins.Contains("runtime_package.Goschedꞏtwin", StringComparison.Ordinal), "never a runtime member that does not exist");
        StringAssert.Contains(twins, "async global::go.golib.GoTask spinRecvꞏtwin(nint n)", "a receive and a Gosched: twinned");
    }

    [TestMethod]
    public void WithoutTwinsNothingIsEmittedAndThePlainBodyIsUntouched()
    {
        // D4 (COORD's add): the plain caller's half. With twins off the generator emits no twin, so spin's plain body,
        // which calls runtime_package.Gosched, is the only spin there is.
        (string twins, Diagnostic[] errors) = RunWith(Runtime, new Dictionary<string, string> { ["build_property.GoTwins"] = "false" }, Spinners);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("spinꞏtwin", StringComparison.Ordinal), "no twin without GoTwins");
        Assert.IsFalse(twins.Contains("Goschedꞏtwin", StringComparison.Ordinal), "no await of the Gosched twin without GoTwins");
    }
}
