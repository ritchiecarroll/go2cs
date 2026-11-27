// TwinOrderTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using go2cs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenTests;

/// <summary>
/// M:N row 7, the ref-blocked calls (DATA-mn-row7-refblock.md, COORD's ruling): the Go-order move. Go orders only calls,
/// method calls, receives and logical operators; a variable's read is unordered against a call, and gc reads it AFTER
/// the calls (go1.26.11, src/tools/mnspike2/MnGoOrder). A parking call that only pure reads (and a ref-like value) are
/// evaluated before now moves ahead of its statement and parks, and its twin reads the operand as gc does, after the
/// call's write. The plain method reads first (C# order: the disclosed plain-path gap), so these rows compare the twin
/// with Go's value, not with the plain method's. A call evaluated first still pins the parking call (the residual), and
/// a call that cannot move because a ref is read after it in an if header awaits in place, reading through the box.
/// </summary>
[TestClass]
public class TwinOrderTests
{
    private const string Source = """
        using System;
        using static go.builtin;

        namespace go;

        public static partial class order_package
        {
            internal static channel<nint> ch;

            // A ref-like first argument, as the u8 format of a Printf: it pins whatever follows unless the call moves.
            internal static nint show(ReadOnlySpan<byte> format, nint a, nint b) => a * 10 + b;

            internal static double showF(ReadOnlySpan<byte> format, double a, nint b) => a * 10 + b;

            // Writes through the box and can park (the twin awaits it); a run never blocks.
            public static nint Set(ж<nint> Ꮡp, nint v, bool block) { ref var p = ref Ꮡp.DerefOrNull(); p = v; if (block) { ᐸꟷ(ch); } return 3; }

            internal static nint other() => 0;

            public static nint local()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                return show("local"u8, a, Set(Ꮡa, 2, false));
            }

            public static nint unary()
            {
                ref var u = ref heap<nint>(1, out var Ꮡu);
                return show("unary"u8, -u, Set(Ꮡu, 2, false));
            }

            public static nint binaryLeft()
            {
                ref var b = ref heap<nint>(1, out var Ꮡb);
                return show("binary"u8, 0, b + Set(Ꮡb, 2, false));
            }

            public static double cast()
            {
                ref var c = ref heap<nint>(1, out var Ꮡc);
                return showF("cast"u8, (double)c, Set(Ꮡc, 2, false));
            }

            // The residual: a CALL evaluated first stays ordered before the parking call, so it cannot move.
            public static nint callFirst()
            {
                ref var d = ref heap<nint>(1, out var Ꮡd);
                return show("call"u8, other() + d, Set(Ꮡd, 2, false));
            }

            // Inline re-derivation: an if header cannot move its call, and reads the ref after it.
            public static nint ifHeader()
            {
                ref var e = ref heap<nint>(1, out var Ꮡe);
                if (Set(Ꮡe, 2, false) == 3 && e == 2) {
                    return 1;
                }
                return 0;
            }

            // Row 19 rule 1, debug/elf's NewFile: a struct construction stored through a ref, each argument a CAST of a
            // parking call. The first call moves, and the second sees `(nint)(int)ᴛ`, a cast of a value already
            // spilled, before it: a pure read, so it moves too. Before the rule the cast read as an earlier evaluation.
            public partial struct Hdr { public nint A; public nint B; }

            public static nint castArgs()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                ref var h = ref heap<Hdr>(default, out var Ꮡh);
                h = new Hdr(A: (nint)(int)Set(Ꮡa, 2, false), B: (nint)(int)Set(Ꮡa, 5, false));
                return h.A * 10 + h.B;
            }

            // Row 19 rule 1, math/big's nat.random: the same through a binary operator's left operand.
            public static nint castOperand()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                ref var h = ref heap<Hdr>(default, out var Ꮡh);
                h.A = (nint)(int)Set(Ꮡa, 2, false) | (nint)(int)Set(Ꮡa, 5, false) << 4;
                return h.A;
            }

            // Row 19 rule 1 over a COPYING cast (P3's read): `use(string(f()), h())`, f returning a shared []byte "old" and h
            // writing b[0] = 'N'. gc converts after h runs and prints "Nld" (go1.26.11); the twin moves h too, so its
            // conversion reads the spilled slice after h's write, as gc does. The plain method converts first ("old").
            internal static slice<byte> shared = new(new byte[] { (byte)'o', (byte)'l', (byte)'d' });

            public static slice<byte> bytesOf(bool block) { if (block) { ᐸꟷ(ch); } return shared; }

            public static nint poke(bool block) { if (block) { ᐸꟷ(ch); } shared[0] = (byte)'N'; return 0; }

            internal static @string use(@string s, nint k) => s;

            public static @string castCopy()
            {
                ref var s = ref heap<@string>(default, out var Ꮡs);
                s = use((@string)bytesOf(false), poke(false));
                return s;
            }

            // Row 19 rule 2, encoding/gob's getBaseType(""u8, t.Elem()): a u8 literal handed to an @string parameter, then
            // a parking call that cannot move ahead of its statement (gob's sits inside a call that moves; here, an if
            // header), so it awaits in place. The argument C# holds is the @string the literal converts to, not the
            // span, so nothing ref-like lives across the await.
            public static nint named(@string s, nint a, bool block) { if (block) { ᐸꟷ(ch); } return len(s) * 10 + a; }

            public static nint u8Named()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                if (named("ab"u8, Set(Ꮡa, 2, false), false) == 23) {
                    return 1;
                }
                return 0;
            }

            // Row 19 rule 2's boundary, fmt's Sprintf (whose format is an sstring): the same literal handed to a ref struct
            // parameter IS held across the await, so the call stays plain and the site is ref-blocked by name. A while
            // header, which runs again each iteration, keeps the call in place (an if header moves it: row 19 M6).
            public static nint spanned(ReadOnlySpan<byte> s, nint a, bool block) { if (block) { ᐸꟷ(ch); } return s.Length * 10 + a; }

            public static nint u8Spanned()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                while (spanned("ab"u8, Set(Ꮡa, 2, false), false) != 23) {
                    return 0;
                }
                return 1;
            }
        }
        """;

    [TestMethod]
    public void ALocalReadBeforeAParkingCallIsReadAfterItInTheTwin_AsGc()
    {
        (string twins, Assembly assembly) = Build();

        StringAssert.Contains(twins, "await Setꞏtwin(Ꮡa, 2, false)", "the parking call awaits");
        Assert.AreEqual((nint)23, Twin(assembly, "local"), "gc reads a after the call: 2*10+3");
        Assert.AreEqual((nint)13, Plain(assembly, "local"), "the plain method reads first, C# order: the disclosed gap");
    }

    [TestMethod]
    public void AUnaryOperandIsReadAfterTheCall_AsGc() =>
        Assert.AreEqual((nint)(-17), Twin(Build().assembly, "unary"), "-2*10+3");

    [TestMethod]
    public void ABinaryLeftOperandIsReadAfterTheCall_AsGc() =>
        Assert.AreEqual((nint)5, Twin(Build().assembly, "binaryLeft"), "0*10 + (2+3)");

    [TestMethod]
    public void ANumericCastIsReadAfterTheCall_AsGc() =>
        Assert.AreEqual(23.0, Twin(Build().assembly, "cast"), "(double)2*10+3");

    [TestMethod]
    public void ACallEvaluatedFirstStillPinsTheParkingCall_TheResidual()
    {
        (string twins, Assembly assembly) = Build();

        StringAssert.Contains(twins, "Set(Ꮡd, 2, false)", "the parking call stays plain: other() runs first in Go too");
        Assert.AreEqual(Plain(assembly, "callFirst"), Twin(assembly, "callFirst"));
    }

    [TestMethod]
    public void AnIfHeaderAwaitsInPlaceAndReadsTheRefThroughTheBox()
    {
        (string twins, Assembly assembly) = Build();

        StringAssert.Contains(twins, "if ((await Setꞏtwin(Ꮡe, 2, false)) == 3 && (Ꮡe.ValueSlot) == 2)", "inline re-derivation");
        Assert.AreEqual((nint)1, Twin(assembly, "ifHeader"));
    }

    [TestMethod]
    public void ACastOfACallAlreadySpilledDoesNotPinTheNextCall_Row19Rule1()
    {
        (string twins, Assembly assembly) = Build();

        StringAssert.Contains(twins, "await Setꞏtwin(Ꮡa, 5, false)", "the second argument's call awaits: the cast before it reads a spilled value");
        Assert.AreEqual((nint)33, Twin(assembly, "castArgs"), "3*10+3");
        Assert.AreEqual((nint)51, Twin(assembly, "castOperand"), "3 | 3<<4");
        Assert.AreEqual("Nld", Twin(assembly, "castCopy").ToString(), "gc converts after the later call writes the shared bytes");
        Assert.IsFalse(twins.Contains("|castArgs|Set|") || twins.Contains("|castOperand|Set|") || twins.Contains("|castCopy|poke|"), "no ref-blocked row names these functions\n" + twins);
    }

    [TestMethod]
    public void AU8LiteralConvertedToAStringParameterIsNotRefLike_Row19Rule2()
    {
        (string twins, Assembly assembly) = Build();

        StringAssert.Contains(twins, "(await namedꞏtwin(\"ab\"u8, (await Setꞏtwin(Ꮡa, 2, false)), false))", "the parking call awaits in place");
        Assert.IsFalse(twins.Contains("|u8Named|Set|"), "no ref-blocked row names u8Named\n" + twins);
        Assert.AreEqual((nint)1, Twin(assembly, "u8Named"), "the if header compared len(\"ab\")*10+3 with 23");

        StringAssert.Contains(twins, "|u8Spanned|Set|ref-like-before|", "the ref struct parameter's literal still blocks the call, by name");
        Assert.AreEqual((nint)1, Twin(assembly, "u8Spanned"));
    }

    private static readonly CSharpParseOptions Options = new(LanguageVersion.CSharp13);

    private static (string twins, Assembly assembly) Build()
    {
        CSharpCompilation compilation = CSharpCompilation.Create("twinorder",
            [CSharpSyntaxTree.ParseText(Source, Options, path: @"C:\twinorder\order.cs")],
            References(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        Dictionary<string, string> properties = new() { ["build_property.GoTwins"] = "true" };

        CSharpGeneratorDriver.Create([new RecvGenerator(), new FieldAccessGenerator(), new TypeGenerator(), new TwinGenerator()], parseOptions: Options,
                optionsProvider: new BuildProperties(properties))
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        string twins = string.Join("\n", output.SyntaxTrees.Skip(compilation.SyntaxTrees.Length).Select(tree => tree.ToString()));

        using MemoryStream image = new();
        Microsoft.CodeAnalysis.Emit.EmitResult built = output.Emit(image);
        Assert.IsTrue(built.Success, string.Join("\n", built.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)) + "\n" + twins);

        return (twins, Assembly.Load(image.ToArray()));
    }

    private static object Plain(Assembly assembly, string name) =>
        Package(assembly).GetMethod(name, BindingFlags.Public | BindingFlags.Static)!.Invoke(null, null)!;

    private static object Twin(Assembly assembly, string name)
    {
        object task = Package(assembly).GetMethod(name + "ꞏtwin", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, null)!;
        object awaiter = task.GetType().GetMethod("GetAwaiter")!.Invoke(task, null)!;

        Assert.IsTrue((bool)awaiter.GetType().GetProperty("IsCompleted")!.GetValue(awaiter)!, "nothing in the run parks, so the twin completes");
        return awaiter.GetType().GetMethod("GetResult")!.Invoke(awaiter, null)!;
    }

    private static Type Package(Assembly assembly) => assembly.GetType("go.order_package", throwOnError: true)!;

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
