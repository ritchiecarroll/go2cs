// TwinRefAcrossAwaitTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using go2cs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenTests;

/// <summary>
/// M:N (experimental, opt-in), N3's CS9217 / CS8178 / CS8934 cut (COORD's approval, 2026-11-15): the shapes that kept net/http's twins
/// from compiling. A ref local cannot live across an await, so a twin derives each one again after it (Rule R); these
/// pin the three gaps in that: a root the converter reassigns in its own re-alias form (C1), an awaited value stored
/// into a ref target (C2), and a paired literal with an explicit return type (C3) -- and C1's refusal, by name, of a ref
/// that cannot be derived again and is live across an await. No cure copies a value out and back.
/// </summary>
[TestClass]
public class TwinRefAcrossAwaitTests
{
    private const string Header = """
        using System;
        using static go.builtin;

        namespace go;

        public static partial class refs_package
        {
            internal static channel<nint> ch;

            public struct T { public nint x; public Bag b; public nint Add(nint k) => x + k; }

            // golib's map<K, V> is a readonly struct: its indexer takes `this` by reference.
            public readonly struct Bag { public nint this[nint i] => i; }

            // A method with a by-ref receiver (net/http's req.closeBody()): C# 13/14 accept a ref local's plain field
            // reads and writes after an await, but NOT passing it by reference, so the shapes below use this.
            public static void closeBody(this ref T r) { r.x = 0; }

            internal static nint parksN() { ᐸꟷ(ch); return 1; }

            internal static object parksO() { ᐸꟷ(ch); return "x"; }

            internal static (nint, object) parks2() { ᐸꟷ(ch); return (1, "y"); }

            // A ref-returning field read (the field-access generator's `GoFieldAccess.get_F(ж<T>)` shape).
            internal static ref nint xOf(ж<T> p) => ref p.DerefOrNull().x;

            internal static readonly object gate = new();

            internal static bool parksB() { ᐸꟷ(ch); return true; }

            internal static T parksT() { ᐸꟷ(ch); return default; }

            internal static nint Next(this T r, nint k) => r.x + k;

        """;

    private const string Footer = "}\n";

    // S1 (C1): the root reassigned in the converter's re-alias form, then an await, then a use of the ref.
    private const string ReAliased = """
            public static void reAliased(ж<T> Ꮡreq)
            {
                try
                {
                    ref var req = ref Ꮡreq.DerefOrNull();
                    Ꮡreq = Ꮡ(new T()); req = ref Ꮡreq.DerefOrNull();
                    ᐸꟷ(ch);
                    req.closeBody();
                }
                finally { }
            }

        """;

    // S1 (C1) over a loop back edge (net/http's redirect loop): the use comes before the await in the text, and after
    // it in the next iteration.
    private const string ReAliasedLoop = """
            public static void reAliasedLoop(ж<T> Ꮡreq)
            {
                try
                {
                    ref var req = ref Ꮡreq.DerefOrNull();
                    for (nint i = 0; i < 2; i++)
                    {
                        req.closeBody();
                        Ꮡreq = Ꮡ(new T()); req = ref Ꮡreq.DerefOrNull();
                        ᐸꟷ(ch);
                    }
                }
                finally { }
            }

        """;

    // S2 (C2): an awaited value stored into a heap-boxed ref local, in the converter's prologue for a function that
    // defers (net/http's readRequest: the box declared outside a try, the ref local inside it). Inside a try, C# must
    // keep the store target's reference across the await, which a ref local cannot be.
    private const string IntoRef = """
            public static object intoRef()
            {
                heap<object>(out var Ꮡv);
                try
                {
                    ref var v = ref Ꮡv.ValueSlot;
                    v = parksO();
                    return v;
                }
                finally { }
            }

        """;

    // S2 (C2): an awaited tuple stored into a field of a ref local, beside a plain local, in the same prologue
    // (net/http's transferReader: `(t.Trailer, err) = fixTrailer(…)`).
    private const string IntoField = """
            public static object intoField(ж<T> Ꮡt)
            {
                heap<object>(out var Ꮡy);
                try
                {
                    ref var t = ref Ꮡt.DerefOrNull();
                    ref var y = ref Ꮡy.ValueSlot;
                    (t.x, y) = parks2();
                    return y;
                }
                finally { }
            }

        """;

    // Stage (c): a COMPOUND store through a ref-returning field read (runtime's `c.scanAlloc += heapSetType…(…)`): CS8178.
    private const string Compound = """
            public static void compound(ж<T> Ꮡt)
            {
                xOf(Ꮡt) += parksN();
            }

        """;

    // Stage (c): an await inside a `lock` (runtime's gcCycle): CS1996. Refused by name; its caller keeps its twin.
    private const string Locked = """
            public static void locked()
            {
                lock (gate)
                    parksN();
            }

            public static void callsLocked()
            {
                ᐸꟷ(ch);
                locked();
            }

        """;

    // Stage (c): a ref local read LATER in the same statement than an awaited call -- math/big's
    // `@new<Float>().SetPrec(n).SetMode(x.mode)` (a chain) and `probablyPrimeMillerRabin(…) && x.abs.probablyPrimeLucas(…)`
    // (a short circuit): CS9217. The ref is read after the await, before Rule R derives it again at the next statement.
    private const string UseAfterChain = """
            public static nint useAfterChain(ж<T> Ꮡt)
            {
                try
                {
                    ref var t = ref Ꮡt.DerefOrNull();
                    return parksT().Next(t.x);
                }
                finally { }
            }

        """;

    // crypto/rsa's `encryptOAEP(opts.Hash.New(), opts.Hash.New(), …, opts.Label)`: TWO moved calls, the second of
    // which reads the ref the first's await left stale; the ref is derived again after each.
    private const string UseAfterTwo = """
            public static nint useAfterTwo(ж<T> Ꮡt)
            {
                try
                {
                    ref var t = ref Ꮡt.DerefOrNull();
                    return sum3(t.x.parksOn(), t.x.parksOn(), t.x);
                }
                finally { }
            }

            internal static nint sum3(nint a, nint b, nint c) => a + b + c;

            internal static nint parksOn(this nint k) { ᐸꟷ(ch); return k; }

        """;

    // crypto/x509's `s.haveSum[sha256.Sum224(cert.Raw)]`: a struct field of a ref local is the RECEIVER of an indexer,
    // evaluated before the awaited index, so the ref would be held across the await.
    private const string IndexBefore = """
            public static nint indexBefore(ж<T> Ꮡt)
            {
                try
                {
                    ref var t = ref Ꮡt.DerefOrNull();
                    return t.b[parksN()];
                }
                finally { }
            }

        """;

    // net/http's `refererForURL(…, ref (req.URL).DerefOrNull(), req.Header.Get(…))`: a by-ref ARGUMENT evaluated before
    // an awaited later argument would hold its reference across the await (CS8178).
    private const string RefArgBefore = """
            public static nint refArgBefore(ж<T> Ꮡt)
            {
                return takesRef(ref Ꮡt.DerefOrNull(), parksN());
            }

            internal static nint takesRef(ref T r, nint k) => r.x + k;

        """;

    // net/http's h2_bundle `Ꮡsc.Value.logf(…, Ꮡsc.Value.conn.RemoteAddr(), …)`: the struct receiver is a ref-returning
    // property (ж<T>.Value), evaluated before an awaited argument (CS8178).
    private const string ValueRecv = """
            public static nint valueRecv(ж<T> Ꮡt)
            {
                return Ꮡt.Value.Add(parksN());
            }

        """;

    // MnMixedStorm's worker: `switch (select(…)) { case 1: ᐸꟷ(s.start); }`. A ref read in a NESTED statement (a case
    // body) is not evaluated with the switch's expression: Rule R derives the ref again before it, so the awaited
    // call stays awaited. 4db1a48034 read the whole switch and left the select plain (ref-blocked).
    private const string SwitchBody = """
            public static void switchBody(ж<T> Ꮡt)
            {
                try
                {
                    ref var t = ref Ꮡt.DerefOrNull();
                    switch (parksN())
                    {
                        case 1:
                            t.x = 2;
                            break;
                    }
                }
                finally { }
            }

        """;

    // net/http's server.cs `case {} when isCommonNetReadError(err): …; default: … c.rwc …`: an await in a CASE GUARD runs
    // before the sections' bodies, so each section derives the ref again on entry, as for an awaited switch expression.
    private const string GuardAwait = """
            public static void guardAwait(ж<T> Ꮡt, nint k)
            {
                try
                {
                    ref var t = ref Ꮡt.DerefOrNull();
                    switch (k)
                    {
                        case 1 when parksB():
                            return;
                        default:
                            t.closeBody();
                            break;
                    }
                }
                finally { }
            }

        """;

    private const string UseAfterAnd = """
            public static bool useAfterAnd(ж<T> Ꮡt)
            {
                try
                {
                    ref var t = ref Ꮡt.DerefOrNull();
                    return parksB() && t.x > 0;
                }
                finally { }
            }

        """;

    // S3 (C3): a can-park literal with an EXPLICIT return type, created in a twin body, so it is paired.
    private const string Typed = """
            public static nint typed()
            {
                ᐸꟷ(ch);
                Func<(nint, nint)> g = (nint, nint) () => { ᐸꟷ(ch); return (1, 2); };
                return g().Item1;
            }

        """;

    // C1's REFUSAL: the root reassigned WITHOUT the re-alias, so the ref names the old pointee; it is live across an
    // await. No sound re-derivation exists, so the function is refused a twin by name instead of compiling wrong.
    private const string Stale = """
            public static void stale(ж<T> Ꮡreq)
            {
                try
                {
                    ref var req = ref Ꮡreq.DerefOrNull();
                    Ꮡreq = Ꮡ(new T());
                    ᐸꟷ(ch);
                    req.closeBody();
                }
                finally { }
            }

        """;

    private static (string twins, Diagnostic[] errors) Run(params string[] shapes)
    {
        // C# 14, as every converted package builds (Directory.Build.props LangVersion): under Preview the compiler admits
        // ref locals across awaits that C# 14 rejects, so a Preview-only check hides the very CS9217 this pins.
        CSharpParseOptions options = new(LanguageVersion.CSharp13);
        string source = Header + string.Concat(shapes) + Footer;
        CSharpCompilation compilation = CSharpCompilation.Create("twinrefs",
            [CSharpSyntaxTree.ParseText(source, options, path: @"C:\twinrefs\main.cs")],
            References(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        Dictionary<string, string> properties = new() { ["build_property.GoTwins"] = "true" };

        CSharpGeneratorDriver.Create([new TwinGenerator()], parseOptions: options, optionsProvider: new BuildProperties(properties))
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        global::go2cs.Tests.TwinStampTests.AssertStampsHold(output); // 7d: every stamp agrees with the compiler

        string twins = string.Join("\n", output.SyntaxTrees.Skip(compilation.SyntaxTrees.Length).Select(tree => tree.ToString()));

        if (Environment.GetEnvironmentVariable("GO2CS_DUMP_TWINS") is { Length: > 0 } dump)
            File.WriteAllText(dump, twins);

        // EMIT, not GetDiagnostics: CS9217 (a ref local preserved across an await) is reported while the async state
        // machine is lowered, which binding alone never does -- a GetDiagnostics-only check passes every shape here.
        Microsoft.CodeAnalysis.Emit.EmitResult emitted = output.Emit(Stream.Null);

        return (twins, emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray());
    }

    private static void AssertTwinCompiles(string method, string shape)
    {
        (string twins, Diagnostic[] errors) = Run(shape);

        Assert.AreEqual(0, errors.Length, method + ": " + string.Join("\n", errors.Select(error => error.ToString())));
        StringAssert.Contains(twins, method + "ꞏtwin(", method + " is twinned");
    }

    [TestMethod]
    public void ARootReassignedInTheReAliasFormIsDerivedAgainAfterAnAwait() => AssertTwinCompiles("reAliased", ReAliased);

    [TestMethod]
    public void ARootReassignedInTheReAliasFormIsDerivedAgainOverALoopBackEdge() => AssertTwinCompiles("reAliasedLoop", ReAliasedLoop);

    [TestMethod]
    public void AnAwaitedValueStoredIntoARefLocalSpillsFirst() => AssertTwinCompiles("intoRef", IntoRef);

    [TestMethod]
    public void AnAwaitedTupleStoredIntoAFieldOfARefLocalSpillsFirst() => AssertTwinCompiles("intoField", IntoField);

    [TestMethod]
    public void APairedLiteralWithAnExplicitReturnTypeGetsATwinTypedTwinHalf() => AssertTwinCompiles("typed", Typed);

    [TestMethod]
    public void AnAwaitedValueStoredByACompoundAssignmentThroughARefSpillsFirst() => AssertTwinCompiles("compound", Compound);

    [TestMethod]
    public void ARefReadLaterInAChainThanAnAwaitIsDerivedAgainFirst() => AssertTwinCompiles("useAfterChain", UseAfterChain);

    [TestMethod]
    public void ARefReadAfterAnAwaitedShortCircuitOperandIsDerivedAgainFirst() => AssertTwinCompiles("useAfterAnd", UseAfterAnd);

    [TestMethod]
    public void ARefIsDerivedAgainAfterEachMovedCall() => AssertTwinCompiles("useAfterTwo", UseAfterTwo);

    [TestMethod]
    public void AStructReceiverReachingARefBeforeAnAwaitedIndexCompiles() => AssertTwinCompiles("indexBefore", IndexBefore);

    [TestMethod]
    public void AByRefArgumentBeforeAnAwaitedArgumentCompiles() => AssertTwinCompiles("refArgBefore", RefArgBefore);

    [TestMethod]
    public void AStructReceiverThroughARefReturningPropertyCompiles() => AssertTwinCompiles("valueRecv", ValueRecv);

    [TestMethod]
    public void AnAwaitInACaseGuardDerivesTheRefAgainInEachSection() => AssertTwinCompiles("guardAwait", GuardAwait);

    [TestMethod]
    public void ARefReadInANestedStatementLeavesTheAwaitInPlace()
    {
        AssertTwinCompiles("switchBody", SwitchBody);

        (string twins, _) = Run(SwitchBody);
        StringAssert.Contains(twins, "await parksNꞏtwin()", "the switch's call is still awaited, not left plain");
    }

    [TestMethod]
    public void AnAwaitInsideALockRefusesTheTwinByName()
    {
        (string twins, Diagnostic[] errors) = Run(Locked);

        Assert.AreEqual(0, errors.Length, string.Join("\n", errors.Select(error => error.ToString())));
        TwinSurfaceTests.AssertForwarderOnly(twins, "locked", "locked is refused a twin", parks: true);
        StringAssert.Contains(twins, "|locked|await-in-lock", "the report's refusal row names it");
        StringAssert.Contains(twins, "callsLockedꞏtwin(", "its caller keeps its twin and calls it plain");
    }

    [TestMethod]
    public void AnUnrederivableRefLiveAcrossAnAwaitRefusesTheTwinByName()
    {
        (string twins, Diagnostic[] errors) = Run(Stale);

        Assert.AreEqual(0, errors.Length, string.Join("\n", errors.Select(error => error.ToString())));
        TwinSurfaceTests.AssertForwarderOnly(twins, "stale", "stale is refused a twin", parks: true);
        StringAssert.Contains(twins, "|stale|ref-unrederivable", "the report's refusal row names the function and its reason");
    }

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.IGoEquatable).Assembly.Location));
    }

    private sealed class BuildProperties(Dictionary<string, string> properties) : Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptionsProvider
    {
        private readonly Options m_global = new(properties);

        public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GlobalOptions => m_global;

        public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GetOptions(SyntaxTree tree) => m_global;

        public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GetOptions(AdditionalText textFile) => m_global;

        private sealed class Options(Dictionary<string, string> values) : Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions
        {
            public override bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value) => values.TryGetValue(key, out value);
        }
    }
}
