// TwinSurfaceTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static GenTests.TwinBoxTests;

namespace GenTests;

/// <summary>
/// M:N, row 2 of the .6 plan (7a): the twin set of an opted-in assembly (docs/phase4/DESIGN-mn-twin-set.md, section 12).
/// Every public member of its converted code has a twin; a member the generator writes no state machine for gets a
/// FORWARDER, <c>[GoParkForwarder] Mꞏtwin(...)</c>, that calls the plain member and returns a completed task. A caller
/// binds the plain member of a marked twin and never awaits it. Each row was red before the code (12.2).
/// </summary>
[TestClass]
public class TwinSurfaceTests
{
    private const string Forwarder = "[global::go.golib.GoParkForwarder]";

    // For rows written before 7a that pin "refused a twin" or "parks nowhere" on a PUBLIC function: it has no state
    // machine, and its one `ꞏtwin` member is a forwarder (R3), not an absence. No caller awaits it.
    internal static void AssertForwarderOnly(string twins, string name, string message)
    {
        string[] lines = twins.Split('\n');
        int[] declared = Enumerable.Range(0, lines.Length).Where(index => lines[index].Contains(name + "ꞏtwin(", StringComparison.Ordinal)).ToArray();

        Assert.AreEqual(1, declared.Length, message + ": exactly one " + name + "ꞏtwin, its forwarder, and no await of it");
        Assert.IsFalse(lines[declared[0]].Contains("async ", StringComparison.Ordinal), message + ": no state machine");
        Assert.IsTrue(declared[0] > 0 && lines[declared[0] - 1].Contains(Forwarder, StringComparison.Ordinal), message + ": the member is a forwarder");
    }

    // F1: public functions that cannot park, with and without a result; an internal one beside them (R6: no forwarder).
    private const string CannotPark = """
            public static nint add(nint a, nint b) => a + b;

            public static void note(nint a) { a++; }

            internal static nint hidden(nint a) => a;

        """;

    // F2: public methods refused by their bodies or modifiers: an unsafe block (native, row 4) and the unsafe modifier.
    private const string BodyRefused = """
            public static nint nat(nint a)
            {
                unsafe { return a + sizeof(nint); }
            }

            public static unsafe nint uns(nint a) { ᐸꟷ(ch); return a; }

        """;

    // F6: a caller that parks, calling F1's function: its twin calls add plain.
    private const string CallsAdd = """
            public static nint useAdd()
            {
                nint r = add(1, 2);
                ᐸꟷ(ch);
                return r;
            }

        """;

    // F6 across assemblies: a referenced assembly's forwarder (as a twin-built library ships it) and its unmarked control.
    // MARK is replaced by the attribute or by nothing.
    private const string LibraryForwarder = """
        using System;

        namespace go;

        public static partial class lib_package
        {
            public static nint twice(nint a) => a * 2;

            [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
            MARK public static global::go.golib.GoTask<nint> twiceꞏtwin(nint a) => new global::go.golib.GoTask<nint>(twice(a));
        }
        """;

    private const string CallsTwice = """
            public static nint useTwice()
            {
                nint r = lib_package.twice(3);
                ᐸꟷ(ch);
                return r;
            }

        """;

    [TestMethod]
    public void F1_APublicFunctionThatCannotParkGetsAForwarder()
    {
        (string twins, Diagnostic[] errors) = RunWith(null, null, CannotPark);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, Forwarder, "a forwarder carries the mark");
        StringAssert.Contains(twins, "global::go.golib.GoTask<nint> addꞏtwin(nint a, nint b) => new global::go.golib.GoTask<nint>(add(a, b));",
            "a function with a result forwards it in a completed task");
        StringAssert.Contains(twins, "global::go.golib.GoTask noteꞏtwin(nint a) { note(a); return default; }", "a void function returns the completed task");
        Assert.IsFalse(twins.Contains("hiddenꞏtwin(", StringComparison.Ordinal), "an internal function is not in the population (R6)");
    }

    [TestMethod]
    public void F2_APublicMethodRefusedByItsBodyGetsAForwarder()
    {
        (string twins, Diagnostic[] errors) = RunWith(null, null, BodyRefused);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "global::go.golib.GoTask<nint> natꞏtwin(nint a) => new global::go.golib.GoTask<nint>(nat(a));", "native: a forwarder");
        StringAssert.Contains(twins, "unsafe global::go.golib.GoTask<nint> unsꞏtwin(nint a)", "the unsafe modifier: a forwarder that keeps it");
        StringAssert.Contains(twins, "// refused native=1", "the refusal is still counted");
    }

    [TestMethod]
    public void F6_ACallerCallsAForwardedFunctionPlain()
    {
        (string twins, Diagnostic[] errors) = RunWith(null, null, CannotPark, CallsAdd);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "useAddꞏtwin()", "the caller parks: a twin");
        Assert.IsFalse(twins.Contains("await addꞏtwin(", StringComparison.Ordinal), "the forwarder is never awaited");
    }

    [TestMethod]
    public void F6_AReferencedAssemblysForwarderIsNeverAwaited()
    {
        (string twins, Diagnostic[] errors) = RunWith(LibraryForwarder.Replace("MARK", Forwarder), null, CallsTwice);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "useTwiceꞏtwin()", "the caller parks: a twin");
        Assert.IsFalse(twins.Contains("twiceꞏtwin(3)", StringComparison.Ordinal), "a marked twin of another assembly binds plain");
    }

    [TestMethod]
    public void F6_AnUnmarkedTwinOfAReferencedAssemblyIsAwaited_TheControl()
    {
        (string twins, Diagnostic[] errors) = RunWith(LibraryForwarder.Replace("MARK", ""), null, CallsTwice);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "twiceꞏtwin(3)", "the same twin, unmarked, is awaited");
    }
}
