// TwinSurfaceTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

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

    // F3: pointer receivers with no box twin: an expression body (byref-recv), one that parks nowhere, and a copy-bound
    // one that parks (byref-recv); a parking caller of the last through a box.
    private const string PointerReceivers = """
            public static nint px(this ref T t) => t.x;

            public static void pset(this ref T t, nint v) { t.x = v; }

            [GoCopyBound]
            public static void pcb(this ref T t) { ᐸꟷ(ch); }

            public static void callP(ж<T> Ꮡp)
            {
                Ꮡp.pcb();
                ᐸꟷ(ch);
            }

        """;

    // F5: sstring-pilot members (the converter writes the sstring member; StrGenerator adds its @string companion).
    private const string SStringPilot = """
            public static nint slen(sstring s) => 0;

            public static void sput(sstring s, nint n) { ᐸꟷ(ch); }

            public static void sw(this ref T t, sstring s) { t.x = 1; }

        """;

    // F4: a variadic function, as the converter lowers `...any` (`params ꓸꓸꓸany aʗp`, ꓸꓸꓸany = Span<any>).
    private const string Variadic = """
            public static nint count(nint first, params Span<object> restʗp) => first + restʗp.Length;

        """;

    [TestMethod]
    public void F4_AVariadicFunctionGetsASliceForwarderThatDeliversEveryElement()
    {
        (string twins, Diagnostic[] errors) = RunWith(null, null, Variadic);

        Assert.AreEqual(0, errors.Length, Errors(errors));

        // Run it: three elements in, three arrive. Without `.ꓸꓸꓸ` the slice binds as ONE params element, and 1 + 1 arrives.
        System.Reflection.Assembly built = System.Reflection.Assembly.Load(LastImage!);
        System.Reflection.MethodInfo forwarder = built.GetType("go.box_package", throwOnError: true)!.GetMethod("countꞏtwin")!;
        object task = forwarder.Invoke(null, [(nint)1, new golib::go.slice<object>(new object[] { "a", "b", "c" })])!;
        object awaiter = task.GetType().GetMethod("GetAwaiter")!.Invoke(task, null)!;

        Assert.AreEqual((nint)4, (nint)awaiter.GetType().GetMethod("GetResult")!.Invoke(awaiter, null)!, "every element is delivered");

        StringAssert.Contains(twins, "global::go.golib.GoTask<nint> countꞏtwin(nint first, global::go.slice<object> restʗp) => new global::go.golib.GoTask<nint>(count(first, restʗp.ꓸꓸꓸ));",
            "the twin takes the Go shape, a slice, and spreads it");
    }

    // F11: a variadic interface member and a plain implementer (no twin of its own, so ꞏcall falls back to the plain call).
    private const string VariadicIface = """
            public partial interface IAdder { nint Add(nint first, params Span<object> restʗp); }

            public partial class Adder : IAdder { public nint Add(nint first, params Span<object> restʗp) => first + restʗp.Length; }

        """;

    [TestMethod]
    public void F11_AVariadicInterfaceMemberGetsASliceSlotWhoseCallDeliversEveryElement()
    {
        (string twins, Diagnostic[] errors) = RunWith(null, new() { ["build_property.GoTwinsIface"] = "hidden", ["build_property.GoTwinsIfaceAll"] = "true" }, VariadicIface);

        Assert.AreEqual(0, errors.Length, Errors(errors));

        // Run the fallback: three elements in, three arrive at the plain member.
        System.Reflection.Assembly built = System.Reflection.Assembly.Load(LastImage!);
        Type face = built.GetType("go.box_package+IAdderꞏtwin", throwOnError: true)!;
        object adder = Activator.CreateInstance(built.GetType("go.box_package+Adder", throwOnError: true)!)!;
        object task = face.GetMethod("Addꞏcall")!.Invoke(null, [adder, (nint)1, new golib::go.slice<object>(new object[] { "a", "b", "c" })])!;
        object awaiter = task.GetType().GetMethod("GetAwaiter")!.Invoke(task, null)!;

        Assert.AreEqual((nint)4, (nint)awaiter.GetType().GetMethod("GetResult")!.Invoke(awaiter, null)!, "every element is delivered");
        StringAssert.Contains(twins, "Addꞏtwin(nint first, global::go.slice<object> restʗp);", "the slot takes the Go shape, a slice");
    }

    private static int Lines(string twins, string text) => twins.Split('\n').Count(line => line.Contains(text, StringComparison.Ordinal));

    [TestMethod]
    public void F3_APointerReceiverWithNoBoxTwinGetsABoxForwarder()
    {
        (string twins, Diagnostic[] errors) = RunWith(null, null, PointerReceivers);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "global::go.golib.GoTask<nint> pxꞏtwin(this global::go.ж<T> Ꮡt) => new global::go.golib.GoTask<nint>(px(ref Ꮡt.DerefOrNull()));",
            "an expression body: the forwarder takes the box and calls through DerefOrNull");
        StringAssert.Contains(twins, "global::go.golib.GoTask psetꞏtwin(this global::go.ж<T> Ꮡt, nint v) { pset(ref Ꮡt.DerefOrNull(), v); return default; }",
            "a receiver that parks nowhere");
        AssertForwarderOnly(twins, "pcb", "a copy-bound receiver is refused its box twin");
        StringAssert.Contains(twins, "pcbꞏtwin(this global::go.ж<T> Ꮡt)", "its forwarder takes the box all the same");
        StringAssert.Contains(twins, "callPꞏtwin(", "the caller parks: a twin, which calls pcb plain");
    }

    [TestMethod]
    public void F5_AnSStringPilotMemberGetsOneTwinOnItsStringMember()
    {
        (string twins, Diagnostic[] errors) = RunWith(null, null, SStringPilot);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "global::go.golib.GoTask<nint> slenꞏtwin(global::go.@string s) => new global::go.golib.GoTask<nint>(slen(s));",
            "the twin hangs on the @string member");
        StringAssert.Contains(twins, "global::go.golib.GoTask sputꞏtwin(global::go.@string s, nint n) { sput(s, n); return default; }",
            "a pilot member that parks: a forwarder until the rewriter retypes its body");
        StringAssert.Contains(twins, "swꞏtwin(this global::go.ж<T> Ꮡt, global::go.@string s) { sw(ref Ꮡt.DerefOrNull(), s); return default; }",
            "a pointer receiver and an sstring parameter together");
        Assert.AreEqual(1, Lines(twins, "slenꞏtwin("), "one twin per Go function");
        Assert.AreEqual(0, twins.Split('\n').Count(line => line.Contains("ꞏtwin(", StringComparison.Ordinal) && line.Contains("sstring", StringComparison.Ordinal)),
            "no twin on the sstring member (RecvGenerator's plain box overload of sw keeps its sstring, as it should)");
    }

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
