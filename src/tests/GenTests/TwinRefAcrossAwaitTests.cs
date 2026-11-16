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
/// M:N PROTOTYPE, N3's CS9217 / CS8178 / CS8934 cut (COORD's approval, 2026-11-15): the shapes that kept net/http's twins
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

            public struct T { public nint x; }

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
    public void AnAwaitInsideALockRefusesTheTwinByName()
    {
        (string twins, Diagnostic[] errors) = Run(Locked);

        Assert.AreEqual(0, errors.Length, string.Join("\n", errors.Select(error => error.ToString())));
        Assert.IsFalse(twins.Contains("lockedꞏtwin(", StringComparison.Ordinal), "locked is refused a twin");
        StringAssert.Contains(twins, "|locked|await-in-lock", "the report's refusal row names it");
        StringAssert.Contains(twins, "callsLockedꞏtwin(", "its caller keeps its twin and calls it plain");
    }

    [TestMethod]
    public void AnUnrederivableRefLiveAcrossAnAwaitRefusesTheTwinByName()
    {
        (string twins, Diagnostic[] errors) = Run(Stale);

        Assert.AreEqual(0, errors.Length, string.Join("\n", errors.Select(error => error.ToString())));
        Assert.IsFalse(twins.Contains("staleꞏtwin(", StringComparison.Ordinal), "stale is refused a twin");
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
