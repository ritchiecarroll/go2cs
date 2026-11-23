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
