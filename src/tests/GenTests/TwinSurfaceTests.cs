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
    // machine, and its one `ꞏtwin` member is a forwarder (R3), not an absence. No caller in its assembly awaits it. The
    // forwarder carries the mark exactly when the member cannot park (COORD's ruling on P8, (iii)).
    internal static void AssertForwarderOnly(string twins, string name, string message, bool parks = false)
    {
        string[] lines = twins.Split('\n');
        int[] declared = Enumerable.Range(0, lines.Length).Where(index => lines[index].Contains(name + "ꞏtwin(", StringComparison.Ordinal)).ToArray();

        Assert.AreEqual(1, declared.Length, message + ": exactly one " + name + "ꞏtwin, its forwarder, and no await of it");
        string line = lines[declared[0]];
        Assert.IsFalse(line.Contains("async ", StringComparison.Ordinal), message + ": no state machine");
        Assert.IsTrue(line.Contains("; return default; }", StringComparison.Ordinal) || line.Contains("=> new global::go.golib.GoTask<", StringComparison.Ordinal),
            message + ": the member is a forwarder");
        Assert.AreEqual(!parks, declared[0] > 0 && lines[declared[0] - 1].Contains(Forwarder, StringComparison.Ordinal),
            message + (parks ? ": it parks, so its forwarder is unmarked" : ": it cannot park, so its forwarder is marked"));
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
        StringAssert.Contains(twins, "Addꞏtwin(nint first, global::go.slice<object> restʗp) => new", "the slot takes the Go shape, a slice");
        StringAssert.Contains(twins, ".Add(first, restʗp.ꓸꓸꓸ));", "and its body spreads it to the plain member");
    }

    // F7: a Go type named Task, returned by a function that cannot park and by one that parks; a BCL Task beside them.
    private const string GoTaskType = """
            public partial struct Task { public nint v; }

            public static Task mk(nint v) => new Task { v = v };

            public static Task mkp() { ᐸꟷ(ch); return default; }

            public static System.Threading.Tasks.Task real() => System.Threading.Tasks.Task.CompletedTask;

        """;

    [TestMethod]
    public void F7_AGoTypeNamedTaskIsNotAsyncShaped()
    {
        (string twins, Diagnostic[] errors) = RunWith(null, null, GoTaskType);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "global::go.golib.GoTask<Task> mkꞏtwin(nint v) => new global::go.golib.GoTask<Task>(mk(v));", "forwarded");
        StringAssert.Contains(twins, "async global::go.golib.GoTask<Task> mkpꞏtwin()", "twinned");
        StringAssert.Contains(twins, "// refused async-shaped=1 ", "only the BCL Task is async-shaped");
        StringAssert.Contains(twins, "|real|GUARD:async-shaped", "and a converted file never holds one: the guard names it");
    }

    // F8: a generic function (E1), beside the report row of each other class.
    private const string Generic = """
            public static T ident<T>(T v) => v;

        """;

    [TestMethod]
    public void F8_AGenericFunctionIsE1ByName()
    {
        (string twins, Diagnostic[] errors) = RunWith(null, null, Generic, CannotPark, CallsAdd);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.AreEqual(0, Lines(twins, "identꞏtwin("), "no twin and no forwarder: E1 is excluded at the freeze");
        StringAssert.Contains(twins, "|ident|E1\r", "named E1 in the report");
        StringAssert.Contains(twins, "|add|forwarder:no-park\r", "a forwarder's row names why");
        StringAssert.Contains(twins, "|useAdd|twin\r", "a twin's row");
        Assert.AreEqual(0, Lines(twins, "|hidden|"), "an internal function is not in the population: no row");
    }

    private static Diagnostic[] Guard(string id) => LastGeneratorDiagnostics!.Where(diagnostic => diagnostic.Id == id).ToArray();

    [TestMethod]
    public void F9_TheGuardNamesAPopulationMemberWithNoTwinAndNoForwarder()
    {
        // The seam drops add's forwarder: the guard must name add, and nothing else.
        (_, Diagnostic[] errors) = RunWith(null, new() { ["build_property.GoTwinsGuardDrop"] = "add", ["build_property.GoTwinsArm"] = "true" }, CannotPark, CallsAdd, Generic);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Diagnostic[] guard = Guard("GO2CS0017");
        Assert.AreEqual(1, guard.Length, string.Join("\n", guard.Select(diagnostic => diagnostic.ToString())));
        Assert.AreEqual(DiagnosticSeverity.Error, guard[0].Severity);
        StringAssert.Contains(guard[0].GetMessage(), "add(", "it names the member");
        StringAssert.Contains(guard[0].GetMessage(), "forwarder-dropped", "and the reason");
    }

    [TestMethod]
    public void F9_TheGuardIsSilentOnAWholeSet_TheControl()
    {
        (_, Diagnostic[] errors) = RunWith(null, null, CannotPark, CallsAdd, Generic, BodyRefused, PointerReceivers, SStringPilot, Variadic);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.AreEqual(0, Guard("GO2CS0017").Length, "every member is twinned, forwarded or E1");
    }

    [TestMethod]
    public void F9_TheGuardNamesAnAsyncShapedResult()
    {
        (_, Diagnostic[] errors) = RunWith(null, null, GoTaskType);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Diagnostic[] guard = Guard("GO2CS0017");
        Assert.AreEqual(1, guard.Length, string.Join("\n", guard.Select(diagnostic => diagnostic.ToString())));
        StringAssert.Contains(guard[0].GetMessage(), "real(", "the BCL Task, and not the Go type named Task");
    }

    // F10: O2, no partial sets. A switch that selects part of the twin set is refused with twins on, unless the build is
    // a prototype arm (GoTwinsArm=true). With no switch the set is whole: interfaces get the hidden shape, every member.
    private const string AnInterface = """
            public partial interface IShape { nint Area(); }

        """;

    [TestMethod]
    public void F10_APartialSetSwitchIsRefusedByName()
    {
        RunWith(null, new() { ["build_property.GoTwinsIface"] = "hidden" }, AnInterface);
        Diagnostic[] refused = Guard("GO2CS0018");
        Assert.AreEqual(1, refused.Length, string.Join("\n", refused.Select(diagnostic => diagnostic.ToString())));
        Assert.AreEqual(DiagnosticSeverity.Error, refused[0].Severity);
        StringAssert.Contains(refused[0].GetMessage(), "GoTwinsIface", "it names the property");

        RunWith(null, new() { ["build_property.GoTwinsGo"] = "false" }, AnInterface);
        StringAssert.Contains(Guard("GO2CS0018").Single().GetMessage(), "GoTwinsGo", "each switch of the list");
    }

    [TestMethod]
    public void F10_TheArmSwitchesAreRefusedOutsideAnArm()
    {
        RunWith(null, new() { ["build_property.GoTwinsForwarders"] = "false", ["build_property.GoTwinsGuardWarn"] = "true", ["build_property.GoTwinsGuardDrop"] = "add" }, CannotPark);
        string[] named = Guard("GO2CS0018").Select(diagnostic => diagnostic.GetMessage()).ToArray();

        Assert.AreEqual(3, named.Length, string.Join("\n", named));
        Assert.IsTrue(named.Any(message => message.Contains("GoTwinsForwarders")) && named.Any(message => message.Contains("GoTwinsGuardWarn")) &&
                      named.Any(message => message.Contains("GoTwinsGuardDrop")), string.Join("\n", named));
    }

    // Every switch that changes what the generator writes is refused alone, by name, and the message says how to fix the
    // build. The box arms' copies (GoTwinsBoxCopy, GoTwinsBoxArgCopy) are wrong on purpose, so they matter most.
    [TestMethod]
    [DataRow("GoTwinsIface", "hidden")]
    [DataRow("GoTwinsIfaceAll", "true")]
    [DataRow("GoTwinsIfaceUnmark", "go.box_package.IShape.Area")]
    [DataRow("GoTwinsGo", "false")]
    [DataRow("GoTwinsPairs", "false")]
    [DataRow("GoTwinsCallerRule", "false")]
    [DataRow("GoTwinsForwarders", "false")]
    [DataRow("GoTwinsGuardWarn", "true")]
    [DataRow("GoTwinsGuardDrop", "add")]
    [DataRow("GoTwinsBoxRecv", "false")]
    [DataRow("GoTwinsBoxForwarders", "false")]
    [DataRow("GoTwinsBoxCopy", "true")]
    [DataRow("GoTwinsBoxParam", "false")]
    [DataRow("GoTwinsBoxArgCopy", "true")]
    public void F10_EveryArmSwitchIsRefusedAloneWithItsFix(string name, string value)
    {
        RunWith(null, new() { ["build_property." + name] = value }, CannotPark);
        Diagnostic[] refused = Guard("GO2CS0018");

        Assert.AreEqual(1, refused.Length, name + ": " + string.Join("\n", refused.Select(diagnostic => diagnostic.ToString())));
        Assert.AreEqual(DiagnosticSeverity.Error, refused[0].Severity);
        StringAssert.Contains(refused[0].GetMessage(), "'" + name + "'", "it names the property");
        StringAssert.Contains(refused[0].GetMessage(), "GoTwinsArm=true", "and the fix");

        RunWith(null, new() { ["build_property." + name] = value, ["build_property.GoTwinsArm"] = "true" }, CannotPark);
        Assert.AreEqual(0, Guard("GO2CS0018").Length, name + ": a prototype arm admits it");
    }

    [TestMethod]
    public void F10_TheTwoSwitchesThatChangeNothingGeneratedAreAdmitted()
    {
        RunWith(null, new() { ["build_property.GoTwinsSiblings"] = "memo", ["build_property.GoTwinsTiming"] = "true" }, CannotPark);

        Assert.AreEqual(0, Guard("GO2CS0018").Length, "GoTwinsSiblings picks how siblings are read, GoTwinsTiming adds a stopwatch");
    }

    [TestMethod]
    public void F10_TheClosureArmSwitchesDoWhatTheySay()
    {
        // GoTwinsForwarders=false: the bytes-and-wall control reproduces the build before 7a: no forwarder, and no guard.
        (string twins, _) = RunWith(null, new() { ["build_property.GoTwinsForwarders"] = "false", ["build_property.GoTwinsArm"] = "true" }, CannotPark);
        Assert.AreEqual(0, Lines(twins, Forwarder), "no forwarder");
        Assert.AreEqual(0, Guard("GO2CS0017").Length, "and no guard");

        // GoTwinsGuardWarn=true: the guard reports as a warning, so a census build runs to the end.
        RunWith(null, new() { ["build_property.GoTwinsGuardDrop"] = "add", ["build_property.GoTwinsGuardWarn"] = "true", ["build_property.GoTwinsArm"] = "true" }, CannotPark);
        Assert.AreEqual(DiagnosticSeverity.Warning, Guard("GO2CS0017").Single().Severity);
    }

    [TestMethod]
    public void F10_AnArmMaySetTheSwitches()
    {
        RunWith(null, new() { ["build_property.GoTwinsIface"] = "hidden", ["build_property.GoTwinsGo"] = "false", ["build_property.GoTwinsArm"] = "true" }, AnInterface);

        Assert.AreEqual(0, Guard("GO2CS0018").Length, "GoTwinsArm=true admits the prototype's switches");
    }

    [TestMethod]
    public void F10_WithNoSwitchTheSetIsWhole()
    {
        (string twins, Diagnostic[] errors) = RunWith(null, null, AnInterface);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.AreEqual(0, Guard("GO2CS0018").Length);
        StringAssert.Contains(twins, "interface IShapeꞏtwin : global::go.box_package.IShape", "the hidden twin interface, over the plain one");
        StringAssert.Contains(twins, "Areaꞏtwin() => new global::go.golib.GoTask<nint>(((global::go.box_package.IShape)this).Area());",
            "a slot for every member, parking or not, whose body calls the plain member");
    }

    // P8 (11.4): the twin set does not depend on which lowering the plain member got. Three builds of one package: the
    // base, (a) the sstring pilot list emptied (the converter writes @string), and (b) one pointer receiver flipped from
    // `this ref T` to `this ж<T>` by a body edit (an added nil check). Members that park and members that cannot.
    private const string P8Base = """
            public static nint slen(sstring s) => 0;

            public static void sput(sstring s, nint n) { ᐸꟷ(ch); }

            public static void pset(this ref T t, nint v) { t.x = v; }

            public static void pbk(this ref T t) { ᐸꟷ(ch); t.x = 1; }

        """;

    private const string P8NoPilot = """
            public static nint slen(@string s) => 0;

            public static void sput(@string s, nint n) { ᐸꟷ(ch); }

            public static void pset(this ref T t, nint v) { t.x = v; }

            public static void pbk(this ref T t) { ᐸꟷ(ch); t.x = 1; }

        """;

    private const string P8Flipped = """
            public static nint slen(sstring s) => 0;

            public static void sput(sstring s, nint n) { ᐸꟷ(ch); }

            public static void pset(this ж<T> Ꮡt, nint v) { if (Ꮡt is null) return; ref var t = ref Ꮡt.DerefOrNull(); t.x = v; }

            public static void pbk(this ref T t) { ᐸꟷ(ch); t.x = 1; }

        """;

    // Each twin member's name, parameter and result types and names, and its attributes, less the implementation marks
    // (a state machine's, nullable context): the surface a caller binds. M:N item 7d's [GoTwinOf] stamp and the
    // [DynamicDependency] that keeps its origin through trimming are implementation marks too (train 3): the rewriter
    // writes them on every STATE-MACHINE twin, for the runtime to name a parked frame's Go function and line, and no
    // caller binds them. A forwarder never parks, so it carries neither. [GoParkForwarder] stays in: it is binding advice.
    private static string[] TwinMetadata(string shapes)
    {
        (_, Diagnostic[] errors) = RunWith(null, null, shapes);
        Assert.AreEqual(0, errors.Length, Errors(errors));

        System.Reflection.Assembly built = System.Reflection.Assembly.Load(LastImage!);
        string[] internals = ["AsyncStateMachineAttribute", "NullableContextAttribute", "NullableAttribute", "DebuggerStepThroughAttribute", "ScopedRefAttribute",
                              "GoTwinOfAttribute", "DynamicDependencyAttribute"];

        return built.GetType("go.box_package", throwOnError: true)!
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(method => method.Name.EndsWith("ꞏtwin", StringComparison.Ordinal))
            .Select(method => method.Name + "(" + string.Join(", ", method.GetParameters().Select(parameter => parameter.ParameterType + " " + parameter.Name)) + ") -> " +
                              method.ReturnType + " [" + string.Join(", ", method.CustomAttributes.Select(attribute => attribute.AttributeType.Name)
                                  .Where(name => !internals.Contains(name)).OrderBy(name => name, StringComparer.Ordinal)) + "]")
            .OrderBy(line => line, StringComparer.Ordinal).ToArray();
    }

    [TestMethod]
    public void P8_TheTwinSetDoesNotDependOnThePlainLowering()
    {
        string[] baseline = TwinMetadata(P8Base), noPilot = TwinMetadata(P8NoPilot), flipped = TwinMetadata(P8Flipped);
        string Show(string[] lines) => string.Join("\n", lines);

        Assert.AreEqual(4, baseline.Length, Show(baseline));
        Assert.AreEqual(Show(baseline), Show(flipped), "(b) one receiver flipped from ref to ж: identical, attributes included");

        // (a) under COORD's ruling (iii): sput parks, so its forwarder under the pilot is unmarked, as its state-machine
        // twin with @string is. Identical, the mark included.
        Assert.AreEqual(Show(baseline), Show(noPilot), "(a) the sstring pilot list emptied: identical, attributes included");
    }

    // COORD's ruling on P8 (iii): a library member refused a state machine that CAN park (the unsafe modifier, a park in
    // its body) gets an UNMARKED forwarder; a member that cannot park gets a marked one.
    private const string LibraryRefusedParker = """
        using System;
        using static go.builtin;

        namespace go;

        public static partial class lib_package
        {
            internal static channel<nint> lch;

            public static unsafe nint upark(nint a) { ᐸꟷ(lch); return a; }

            public static nint quick(nint a) => a;
        }
        """;

    private const string CallsLibrary = """
            public static nint useLib()
            {
                nint r = lib_package.upark(1) + lib_package.quick(2);
                return r;
            }

        """;

    [TestMethod]
    public void P8_ARefusedMemberThatParksGetsAnUnmarkedForwarderThatCallersAwait()
    {
        LibraryTwins = true;

        try
        {
            (string twins, Diagnostic[] errors) = RunWith(LibraryRefusedParker, null, CallsLibrary);

            Assert.AreEqual(0, errors.Length, Errors(errors));
            StringAssert.Contains(twins, "useLibꞏtwin()", "the caller parks through upark's unmarked forwarder: a twin");
            StringAssert.Contains(twins, "uparkꞏtwin(1)", "and it awaits that forwarder");
            Assert.IsFalse(twins.Contains("quickꞏtwin(", StringComparison.Ordinal), "a marked forwarder is never awaited");
        }
        finally
        {
            LibraryTwins = false;
        }
    }

    // Arm S (the shipping default, F10's whole set) at closure scale, 2026-11-21: 20 CS0535. Under the hidden shape an
    // implementer whose ONE parking member is forwarded to its twin declares `: Iꞏtwin`, and every other slot of Iꞏtwin
    // (every member, R7) has no implementation (reflectlite's rtype : Typeꞏtwin; sync's MutexжLocker : Lockerꞏtwin).
    // ImplementGenerator writes the explicit implementations the slot forwarders follow, as a converted project builds.
    private const string ImplementerSource = """
        using System;
        using static go.builtin;

        [assembly: go.GoImplement<global::go.work_package.Worker, global::go.work_package.IWork>]

        namespace go;

        public static partial class work_package
        {
            internal static channel<nint> ch;

            public partial interface IWork { void Wait(); nint Size(); }

            public partial struct Worker { public nint n; }

            public static void Wait(this Worker w) { ᐸꟷ(ch); }

            public static nint Size(this Worker w) => w.n;

            public static nint useWork(IWork x)
            {
                x.Wait();
                return x.Size();
            }
        }
        """;

    [TestMethod]
    public void S_AnImplementerForwardingOneSlotStillImplementsEveryHiddenSlot()
    {
        Microsoft.CodeAnalysis.CSharp.CSharpParseOptions options = new(Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp13);
        Microsoft.CodeAnalysis.CSharp.CSharpCompilation compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create("twinwork",
            [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(ImplementerSource, options, path: @"C:\twinwork\main.cs")], References(),
            new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        // No switch: the shipping default, the hidden shape with a slot for every member.
        Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver.Create(
                [new go2cs.ImplementGenerator(), new go2cs.RecvGenerator(), new go2cs.FieldAccessGenerator(), new go2cs.TypeGenerator(), new go2cs.TwinGenerator()],
                parseOptions: options, optionsProvider: new BuildProperties(new() { ["build_property.GoTwins"] = "true" }))
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        string twins = string.Join("\n", output.SyntaxTrees.Skip(compilation.SyntaxTrees.Length).Select(tree => tree.ToString()));
        Diagnostic[] errors = output.Emit(System.IO.Stream.Null).Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();

        StringAssert.Contains(twins, "interface IWorkꞏtwin", "the hidden twin interface");
        StringAssert.Contains(twins, "IWorkꞏtwin.Waitꞏtwin()", "Worker forwards its parking slot to Wait's twin");
        Assert.AreEqual(0, errors.Length, Errors(errors));
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
        AssertForwarderOnly(twins, "pcb", "a copy-bound receiver is refused its box twin", parks: true);
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

    // P3's read of 4afbe98217, finding 1: a public `static extern` member of a converted file. It has no body, so it
    // was never a candidate: no twin, no forwarder and no guard row (outside the set, silently). Admitted as a
    // candidate it is refused `native`, and its forwarder must not copy `extern` (CS0179: an extern with a body).
    private const string ExternMember = """
            [System.Runtime.InteropServices.DllImport("kernel32")]
            public static extern int GetCurrentProcessId();

        """;

    [TestMethod]
    public void F2_APublicExternMemberGetsAForwarderWithoutExtern()
    {
        (string twins, Diagnostic[] errors) = RunWith(null, null, ExternMember);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "public static global::go.golib.GoTask<int> GetCurrentProcessIdꞏtwin() => new global::go.golib.GoTask<int>(GetCurrentProcessId());",
            "a forwarder, without `extern`");
        StringAssert.Contains(twins, "|GetCurrentProcessId|forwarder:native", "in the population, as a native refusal's forwarder");
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
