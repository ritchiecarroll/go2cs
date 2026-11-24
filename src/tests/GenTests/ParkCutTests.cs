// ParkCutTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static GenTests.TwinBoxTests;

namespace GenTests;

/// <summary>
/// M:N, N1 (docs/phase4/DESIGN-mn-nopark.md, section 5): edges that are not parks for the generator's purposes. A WEAK
/// member's twin parks, but a call to it never makes its caller a twin; a CUT site is never a park at all. Each row was
/// red before the code.
/// </summary>
[TestClass]
public class ParkCutTests
{
    private const string Weak = "[global::go.golib.GoParkWeak]";

    // A member that parks (lockish), and three callers: one whose only park is lockish, one that also parks itself, and
    // one that calls a member that cannot park (quiet). Internal, so no caller gets a forwarder (R6).
    private const string Callers = """
            public static void lockish() { ᐸꟷ(ch); }

            public static nint quiet(nint a) => a + 1;

            internal static void onlyLock() { lockish(); }

            internal static void both() { lockish(); ᐸꟷ(ch); }

            internal static nint usesQuiet() => quiet(1);

        """;

    // A member that parks (parker), a caller whose only park is it, and one that also parks itself.
    private const string Cuts = """
            public static void parker() { ᐸꟷ(ch); }

            internal static void cutter() { parker(); }

            internal static void cutBoth() { parker(); ᐸꟷ(ch); }

        """;

    private static Dictionary<string, string> Props(string? weak = null, string? cut = null)
    {
        Dictionary<string, string> props = [];

        if (weak is not null)
            props["build_property.GoTwinsWeak"] = weak;

        if (cut is not null)
            props["build_property.GoTwinsCut"] = cut;

        return props;
    }

    private static Diagnostic[] Stale() => [.. (LastGeneratorDiagnostics ?? []).Where(diagnostic => diagnostic.Id == "GO2CS0019")];

    private static string StaleText() => string.Join("\n", Stale().Select(diagnostic => diagnostic.GetMessage()));

    // The text of a twin: from its declaration to the next blank line.
    private static string TwinOf(string twins, string name)
    {
        int start = twins.IndexOf(name + "ꞏtwin(", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, name + " has a twin");
        int end = twins.IndexOf("#line default", start, StringComparison.Ordinal);
        return end < 0 ? twins.Substring(start) : twins.Substring(start, end - start);
    }

    // The attribute lines directly above a member's twin declaration (a twin's `#line` directive sits between them).
    private static string MarksOf(string twins, string name)
    {
        int at = twins.IndexOf(name + "ꞏtwin(", StringComparison.Ordinal);
        Assert.IsTrue(at >= 0, name + " has a twin");
        string[] lines = twins.Substring(0, twins.LastIndexOf('\n', at) + 1).Replace("\r", "").Split('\n');
        List<string> marks = [];

        for (int i = lines.Length - 1; i >= 0; i--)
        {
            string line = lines[i].Trim();

            if (line.Length == 0 && i == lines.Length - 1)
                continue;

            if (!line.StartsWith('[') && !(line.StartsWith("#line ", StringComparison.Ordinal) && line != "#line default"))
                break;

            marks.Add(line);
        }

        return string.Join("\n", marks);
    }

    [TestMethod]
    public void N1_0_WithoutAnEntryTheCallerOfAParkingMemberIsTwinned()
    {
        // The control for N1-1: today's rule.
        (string twins, Diagnostic[] errors) = RunWith(null, null, Callers);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "onlyLockꞏtwin(", "onlyLock parks through lockish");
        Assert.IsFalse(twins.Contains(Weak, StringComparison.Ordinal), "no weak mark without an entry");
        StringAssert.Contains(twins, "// weak-sites=0 cut-sites=0", "the report counts none");
    }

    [TestMethod]
    public void N1_1_ACallerWhoseOnlyParkIsAWeakMemberIsNotTwinned()
    {
        (string twins, Diagnostic[] errors) = RunWith(null, Props(weak: "go.box_package.lockish"), Callers);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("onlyLockꞏtwin(", StringComparison.Ordinal), "onlyLock's only park is weak, so it is not a twin");
        StringAssert.Contains(TwinOf(twins, "lockish"), "await ", "the weak member's own twin still parks");
        StringAssert.Contains(MarksOf(twins, "lockish"), Weak, "and carries [GoParkWeak]");
        Assert.IsFalse(MarksOf(twins, "lockish").Contains("[global::go.golib.GoParkForwarder]", StringComparison.Ordinal), "never both marks");
        Assert.AreEqual(0, Stale().Length, StaleText());
    }

    [TestMethod]
    public void N1_2_ACallerTwinnedForAnotherReasonAwaitsTheWeakMember()
    {
        (string twins, Diagnostic[] errors) = RunWith(null, Props(weak: "go.box_package.lockish"), Callers);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(TwinOf(twins, "both"), "await lockishꞏtwin()", "both is a twin for its own receive, and awaits lockish");
        StringAssert.Contains(twins, "// weak-sites=2 cut-sites=0", "two direct calls would have parked but for the weak target");
    }

    [TestMethod]
    public void N1_3_ACutSiteStaysPlainAndDoesNotTwinItsCaller()
    {
        (string twins, Diagnostic[] errors) = RunWith(null, Props(cut: "go.box_package.cutter->go.box_package.parker;go.box_package.cutBoth->go.box_package.parker"), Cuts);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("cutterꞏtwin(", StringComparison.Ordinal), "cutter's only park was the cut site");
        string cutBoth = TwinOf(twins, "cutBoth");
        Assert.IsFalse(cutBoth.Contains("parkerꞏtwin", StringComparison.Ordinal), "inside a twin, the cut site is a plain call");
        StringAssert.Contains(cutBoth, "parker();", "the plain call");
        Assert.IsFalse(MarksOf(twins, "parker").Contains(Weak, StringComparison.Ordinal), "a cut marks nothing on the callee");
        StringAssert.Contains(twins, "// weak-sites=0 cut-sites=2", "the report counts both cut sites");
        Assert.AreEqual(0, Stale().Length, StaleText());
    }

    // N1-3b, the converter's method-holder form (methodHolder.go, GlyphMethodSuffix): a library method's body is a plain
    // static `<Name>⁔(/*this*/ recv, …)` and the Go-named extension method is a one-line forwarder to it. A key names
    // the Go method, so it binds the BODY, whose receiver is its first parameter: fmt's stdlib rows are written that
    // way (`go.fmt_package.pp.handleMethods`), and each read "names no such call site" in the real fmt build before.
    private const string SuffixCuts = """
            public partial struct P { public nint v; }

            public static void parker() { ᐸꟷ(ch); }

            internal static void sfx⁔(/*this*/ ж<P> Ꮡp) { parker(); }

            internal static void sfx(this ж<P> Ꮡp) => sfx⁔(Ꮡp);

            internal static void sfxBoth⁔(/*this*/ ref P p) { parker(); ᐸꟷ(ch); }

            internal static void sfxBoth(this ref P p) => sfxBoth⁔(ref p);

        """;

    [TestMethod]
    public void N1_3b_AKeyNamesTheGoMethodSoItBindsTheSuffixBody()
    {
        (string twins, Diagnostic[] errors) = RunWith(null, Props(cut: "go.box_package.P.sfx->go.box_package.parker;go.box_package.P.sfxBoth->go.box_package.parker"), SuffixCuts);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.AreEqual(0, Stale().Length, "both keys bind their body: " + StaleText());
        Assert.IsFalse(twins.Contains("sfx⁔ꞏtwin(", StringComparison.Ordinal), "the body's only park was the cut site");
        string body = TwinOf(twins, "sfxBoth⁔");
        Assert.IsFalse(body.Contains("parkerꞏtwin", StringComparison.Ordinal), "inside the body's twin, the cut site is a plain call");
        StringAssert.Contains(body, "parker();", "the plain call");
        StringAssert.Contains(twins, "// weak-sites=0 cut-sites=2", "the report counts both cut sites");
    }

    // N1-1/2/4 over the method-holder form (P3's MUST, row 7 read 2026-11-24): a weak entry on a library method names the
    // forwarder and the body by one key. The forwarder's one call is its own body, and that call is neither weak nor
    // cuttable: the forwarder parks through it, so its twin exists, carries [GoParkWeak] and never [GoParkForwarder], and
    // a caller twinned anyway awaits it. Without that, the forwarder stops parking and every caller holds its carrier.
    private const string SuffixWeak = """
            public partial struct P { public nint v; }

            public static void sfxBoth⁔(/*this*/ ж<P> Ꮡp) { ᐸꟷ(ch); Ꮡp.Value.v++; }

            public static void sfxBoth(this ж<P> Ꮡp) => sfxBoth⁔(Ꮡp);

            internal static void onlySfx(ж<P> Ꮡp) { Ꮡp.sfxBoth(); }

            internal static void sfxAndPark(ж<P> Ꮡp) { Ꮡp.sfxBoth(); ᐸꟷ(ch); }

        """;

    [TestMethod]
    public void N1_1b_AWeakSuffixMethodKeepsItsForwarderParkingAndMarkedWeak()
    {
        (string twins, Diagnostic[] errors) = RunWith(null, Props(weak: "go.box_package.P.sfxBoth"), SuffixWeak);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.AreEqual(0, Stale().Length, "a live weak entry: " + StaleText());
        string marks = MarksOf(twins, "sfxBoth");
        StringAssert.Contains(marks, Weak, "the forwarder's twin carries the weak mark");
        Assert.IsFalse(marks.Contains("GoParkForwarder", StringComparison.Ordinal), "the forwarder still parks, so it is not a marked forwarder");
        StringAssert.Contains(TwinOf(twins, "sfxBoth"), "sfxBoth⁔ꞏtwin(", "the forwarder's twin awaits its body");
        Assert.IsFalse(twins.Contains("onlySfxꞏtwin(", StringComparison.Ordinal), "N1-1: a caller whose only park is the weak member is plain");
        StringAssert.Contains(TwinOf(twins, "sfxAndPark"), "sfxBothꞏtwin(", "N1-2: a caller twinned anyway awaits the weak forwarder");
    }

    private const string SuffixWeakLibrary = """
        using System;
        using static go.builtin;

        namespace go;

        public static partial class lib_package
        {
            internal static channel<nint> ch;

            public partial struct P { public nint v; }

            public static void sfxBoth⁔(/*this*/ ж<P> Ꮡp) { ᐸꟷ(ch); Ꮡp.Value.v++; }

            public static void sfxBoth(this ж<P> Ꮡp) => sfxBoth⁔(Ꮡp);
        }
        """;

    private const string UsesSuffixLibrary = """
            internal static void useSfx(ж<lib_package.P> Ꮡp) { lib_package.sfxBoth(Ꮡp); }

            internal static void useSfxAndPark(ж<lib_package.P> Ꮡp) { lib_package.sfxBoth(Ꮡp); ᐸꟷ(ch); }

        """;

    [TestMethod]
    public void N1_4c_AWeakSuffixMethodCrossesAssembliesOnItsForwardersTwin()
    {
        LibraryTwins = true;
        LibraryProperties = Props(weak: "go.lib_package.P.sfxBoth");

        try
        {
            (string twins, Diagnostic[] errors) = RunWith(SuffixWeakLibrary, null, UsesSuffixLibrary);

            Assert.AreEqual(0, errors.Length, Errors(errors));
            Assert.IsFalse(twins.Contains("useSfxꞏtwin(", StringComparison.Ordinal), "the forwarder's [GoParkWeak] keeps useSfx plain");
            StringAssert.Contains(TwinOf(twins, "useSfxAndPark"), "sfxBothꞏtwin(", "a caller twinned anyway awaits the forwarder's twin");
        }
        finally
        {
            LibraryTwins = false;
            LibraryProperties = null;
        }
    }

    // P3's SHOULD (row 7 read): a cut site inside a go literal is awaited by the rewriter when not cut, so a cut there is
    // live, never "dead".
    private const string CutInLiteral = """
            public static void parker() { ᐸꟷ(ch); }

            internal static void launches() { goǃ(() => { parker(); }); ᐸꟷ(ch); }

        """;

    [TestMethod]
    public void N1_3c_ACutInsideAGoLiteralIsLive()
    {
        (string plain, Diagnostic[] plainErrors) = RunWith(null, null, CutInLiteral);
        Assert.AreEqual(0, plainErrors.Length, Errors(plainErrors));
        StringAssert.Contains(TwinOf(plain, "launches"), "parkerꞏtwin", "CONTROL: uncut, the literal awaits parker");

        (string twins, Diagnostic[] errors) = RunWith(null, Props(cut: "go.box_package.launches->go.box_package.parker"), CutInLiteral);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(TwinOf(twins, "launches").Contains("parkerꞏtwin", StringComparison.Ordinal), "cut, the literal's call is plain");
        Assert.AreEqual(0, Stale().Length, "the cut does something, so it is not dead: " + StaleText());
    }

    // A library's member that parks, with its twin, marked weak or not; compiled first and referenced by image.
    private static string Library(bool weak) => $$"""
        using System;

        namespace go;

        public static partial class lib_package
        {
            public static void wl() { }

            {{(weak ? "[global::go.golib.GoParkWeak]" : "")}}
            public static global::go.golib.GoTask wlꞏtwin() => default;
        }
        """;

    private const string UsesLibrary = """
            internal static void useWl() { lib_package.wl(); }

        """;

    [TestMethod]
    public void N1_4_AWeakMarkInMetadataKeepsAnotherAssemblysCallerPlain()
    {
        (string twins, Diagnostic[] errors) = RunWith(Library(weak: true), null, UsesLibrary);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("useWlꞏtwin(", StringComparison.Ordinal), "the library's [GoParkWeak] keeps useWl plain");
        StringAssert.Contains(twins, "// weak-sites=1 cut-sites=0", "the report counts the weak call");

        // CONTROL (COORD's ruling 7): the same member without the mark twins its caller.
        (string control, Diagnostic[] controlErrors) = RunWith(Library(weak: false), null, UsesLibrary);

        Assert.AreEqual(0, controlErrors.Length, Errors(controlErrors));
        StringAssert.Contains(TwinOf(control, "useWl"), "await lib_package.wlꞏtwin()", "unmarked, useWl twins and awaits it");
    }

    // sync's shape: a `this ref` method whose only twin takes the receiver's BOX (`Lockꞏtwin(this ж<Mutex>)`), marked weak.
    private static string BoxLibrary(bool weak) => $$"""
        using System;

        namespace go;

        public static partial class lib_package
        {
            public partial struct M
            {
                public nint v;

                // A converted struct's equality, as TypeGenerator gives it: the consumer's struct H compares its field.
                public static bool operator ==(M a, M b) => a.v == b.v;
                public static bool operator !=(M a, M b) => a.v != b.v;
                public override bool Equals(object? obj) => obj is M other && other == this;
                public override int GetHashCode() => v.GetHashCode();
            }

            public static void bx(this ref M m) { }

            {{(weak ? "[global::go.golib.GoParkWeak]" : "")}}
            public static global::go.golib.GoTask bxꞏtwin(this ж<M> Ꮡm) => default;
        }
        """;

    private const string UsesBoxLibrary = """
            public partial struct H { public lib_package.M mu; }

            internal static void useBx(ж<H> Ꮡh)
            {
                ref var h = ref Ꮡh.DerefOrNull();
                h.mu.bx();
            }

            internal static void useBxToo(ж<H> Ꮡh)
            {
                ref var h = ref Ꮡh.DerefOrNull();
                h.mu.bx();
                ᐸꟷ(ch);
            }

        """;

    [TestMethod]
    public void N1_4b_AWeakBoxTwinKeepsItsCallerPlainAndATwinCallerAwaitsIt()
    {
        (string twins, Diagnostic[] errors) = RunWith(BoxLibrary(weak: true), null, UsesBoxLibrary);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("useBxꞏtwin(", StringComparison.Ordinal), "the weak box twin keeps useBx plain");
        StringAssert.Contains(TwinOf(twins, "useBxToo"), "bxꞏtwin()", "a caller twinned for its own receive still awaits the box twin");

        // CONTROL: unmarked, the box twin twins its caller.
        (string control, Diagnostic[] controlErrors) = RunWith(BoxLibrary(weak: false), null, UsesBoxLibrary);

        Assert.AreEqual(0, controlErrors.Length, Errors(controlErrors));
        StringAssert.Contains(TwinOf(control, "useBx"), "bxꞏtwin()", "unmarked, useBx twins and awaits it");
    }

    [TestMethod]
    public void N1_5_AStaleEntryIsReportedOnEachArm()
    {
        (_, Diagnostic[] errors) = RunWith(null, Props(
            weak: "go.box_package.nosuch;go.box_package.quiet",
            cut: "go.box_package.nosuch->go.box_package.parker;go.box_package.cutter->go.box_package.lockish;go.box_package.usesQuiet->go.box_package.quiet;notapair"),
            Callers, Cuts);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        string text = StaleText();

        StringAssert.Contains(text, "'go.box_package.nosuch' (GoTwinsWeak) is a stale N1 entry: names no member of this assembly", "W, no member");
        StringAssert.Contains(text, "'go.box_package.quiet' (GoTwinsWeak) is a stale N1 entry: dead", "W, dead: quiet cannot park");
        StringAssert.Contains(text, "'go.box_package.nosuch->go.box_package.parker' (GoTwinsCut) is a stale N1 entry: names no member of this assembly", "C, no caller");
        StringAssert.Contains(text, "'go.box_package.cutter->go.box_package.lockish' (GoTwinsCut) is a stale N1 entry: names no such call site", "C, no site");
        StringAssert.Contains(text, "'go.box_package.usesQuiet->go.box_package.quiet' (GoTwinsCut) is a stale N1 entry: dead", "C, dead: quiet does not park");
        StringAssert.Contains(text, "'notapair' (GoTwinsCut) is a stale N1 entry: not a Caller->Callee pair", "a cut without an arrow");
        Assert.AreEqual(6, Stale().Length, text);
        Assert.IsTrue(Stale().All(diagnostic => diagnostic.Severity == DiagnosticSeverity.Warning), "a warning");
    }

    // A package class the stdlib table names, with none of the members its rows name: every row binds in this build
    // (DeclaresPackage) and every one is stale, which is how the table cannot rot.
    private const string FakeFmt = """
        }

        public static partial class fmt_package
        {
            internal static void unrelated() { }

        """;

    [TestMethod]
    public void N1_5_TheStdlibTableBindsOnlyInItsPackageAndReportsAStaleRow()
    {
        RunWith(null, null, Callers);
        Assert.AreEqual(0, Stale().Length, "box_package declares no package the table names: " + StaleText());

        (_, Diagnostic[] errors) = RunWith(null, null, Callers, FakeFmt);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.AreEqual(5, Stale().Length, "the table's five rows: " + StaleText());
        StringAssert.Contains(StaleText(), "'go.fmt_package.pp.handleMethods->go.fmt_package.Stringer.String' (stdlib) is a stale N1 entry: names no member", "a row by name");
    }

    // N1-5b over the shape the converter emits (DESIGN-mn-nopark.md 3.3): a promoted call `w.Lock()` on a struct that
    // embeds the lock is written through the embedded field, `Ꮡw.of(W.ᏑMutex).Lock()`, in the same package and across
    // packages alike, so it reaches the weak member itself and never the generated [GoPromoted] forwarder. Here `lk` is
    // the weak member, `E` the embedded field, and `w.E.lk()` the walked call a twin turns into the field box's twin.
    private const string Embedded = """
            public partial struct E { public nint v; }

            public static void lk(this ref E e) { ᐸꟷ(ch); e.v++; }

            [GoType] public partial struct W { /*embed*/ public E E; }

            internal static void onlyEmbedded(ж<W> Ꮡw)
            {
                ref var w = ref Ꮡw.DerefOrNull();
                w.E.lk();
            }

            internal static void embeddedToo(ж<W> Ꮡw)
            {
                ref var w = ref Ꮡw.DerefOrNull();
                w.E.lk();
                ᐸꟷ(ch);
            }

        """;

    [TestMethod]
    public void N1_5b_ACallThroughAnEmbeddedWeakMemberIsWeakAsTheMemberIs()
    {
        (string control, Diagnostic[] controlErrors) = RunWith(null, null, Embedded);

        Assert.AreEqual(0, controlErrors.Length, Errors(controlErrors));
        StringAssert.Contains(TwinOf(control, "onlyEmbedded"), "lkꞏtwin()", "CONTROL: unlisted, the walked call parks through the field box");

        (string twins, Diagnostic[] errors) = RunWith(null, Props(weak: "go.box_package.E.lk"), Embedded);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("onlyEmbeddedꞏtwin(", StringComparison.Ordinal), "a plain caller is not twinned by the weak member");
        StringAssert.Contains(TwinOf(twins, "embeddedToo"), "Field‿Tokens.ᏑE‿).lkꞏtwin()", "a twin caller awaits it through the field box, as `t.mu.Lock()`");
        StringAssert.Contains(MarksOf(twins, "lk"), Weak, "the member's twin carries the mark");
    }

    // N1-5c (DESIGN-mn-nopark.md 3.3): an interface member over a weak member stays a park. Its implementers are open
    // ended, so a caller through the interface twins for that call, where the same call on the concrete type would not.
    private const string Interface = """
            public partial interface ILocker { void lk(); }

            public partial struct L { public nint v; }

            public static void lk(this L l) { ᐸꟷ(ch); }

            internal static void viaInterface(ILocker l) { l.lk(); }

            internal static void viaConcrete(L l) { l.lk(); }

        """;

    [TestMethod]
    public void N1_5c_ACallThroughAnInterfaceOverAWeakMemberStillTwinsItsCaller()
    {
        (string twins, Diagnostic[] errors) = RunWith(null, Props(weak: "go.box_package.L.lk"), Interface);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("viaConcreteꞏtwin(", StringComparison.Ordinal), "on the concrete type the weak member keeps its caller plain");
        StringAssert.Contains(twins, "viaInterfaceꞏtwin(", "through the interface the call is still a park (3.3's named cost)");
    }

    [TestMethod]
    public void N1_6_NeitherPropertyIsRefusedAndThePopulationHolds()
    {
        (string twins, Diagnostic[] errors) = RunWith(null, Props(weak: "go.box_package.lockish", cut: "go.box_package.cutter->go.box_package.parker"), Callers, Cuts);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Diagnostic[] o2 = [.. (LastGeneratorDiagnostics ?? []).Where(diagnostic => diagnostic.Id is "GO2CS0018" or "GO2CS0017")];
        Assert.AreEqual(0, o2.Length, Errors(o2));

        // R1: every public member still has its twin or its forwarder.
        foreach (string name in new[] { "lockish", "quiet", "parker" })
            StringAssert.Contains(twins, name + "ꞏtwin(", name + " keeps its twin or forwarder");
    }
}
