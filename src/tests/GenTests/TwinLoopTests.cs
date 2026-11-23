// TwinLoopTests.cs - Gbtc
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
/// M:N row 7 piece 6 (DATA-mn-row7-r5.md): the two loop shapes the whole standard library built with twins first
/// reached. (1) Go's two-variable range is a DECONSTRUCTING foreach (ForEachVariableStatementSyntax); a body entered
/// after its header awaited must derive the live ref locals again (Rule H), as the one-variable form's does. (2) An
/// await in a for statement's iterator is a statement expression (no parentheses), followed in the iterator list by
/// the live ref locals derived again, so the condition reads them fresh and `continue` (plain or a labeled
/// `goto continue_x`) still runs the iterator. Each row EXECUTES the twin and compares it with the plain method.
/// </summary>
[TestClass]
public class TwinLoopTests
{
    private const string Source = """
        using System;
        using static go.builtin;

        namespace go;

        public static partial class loop_package
        {
            internal static channel<nint> ch;

            public partial struct N { public nint v; }

            // Each parks only when asked: the twin awaits it, and a run never blocks.
            internal static slice<nint> items(bool block) { if (block) { ᐸꟷ(ch); } return new slice<nint>(new nint[] { 1, 2, 3, 4 }); }

            internal static void advance(ж<N> Ꮡq, bool block) { ref var q = ref Ꮡq.DerefOrNull(); if (block) { ᐸꟷ(ch); } q.v++; }

            // (1) A two-variable range whose header awaits; its body writes through the live ref local, and the write
            // is read back after the loop.
            public static nint rangeWrite(ж<N> Ꮡp)
            {
                ref var p = ref Ꮡp.DerefOrNull();

                foreach (var (i, v) in range(items(false))) {
                    p.v += v * (i + 1);
                }

                return p.v;
            }

            // (1) The control: the one-variable form, which Rule H already served.
            public static nint rangeWriteOne(ж<N> Ꮡp)
            {
                ref var p = ref Ꮡp.DerefOrNull();

                foreach (var e in range(items(false))) {
                    p.v += e.Item2;
                }

                return p.v;
            }

            // (2a) The iterator parks; the condition reads the ref local right after it; a plain continue.
            public static nint forContinue(ж<N> Ꮡp)
            {
                ref var p = ref Ꮡp.DerefOrNull();
                nint sum = 0;

                for (; p.v < 10; advance(Ꮡp, false)) {
                    if (p.v % 3 == 0) {
                        continue;
                    }
                    sum += p.v;
                }

                return sum * 100 + p.v;
            }

            // (2b) A labeled continue from an inner loop, spelled as the converter spells it.
            public static nint forLabeled(ж<N> Ꮡp)
            {
                ref var p = ref Ꮡp.DerefOrNull();
                nint sum = 0;

        outer:
                for (; p.v < 12; advance(Ꮡp, false)) {
                    for (nint k = 0; k < 3; k++) {
                        if ((p.v + k) % 5 == 0) {
                            goto continue_outer;
                        }
                    }
                    sum += p.v;
        continue_outer:;
                }

                return sum * 100 + p.v;
            }

            internal static bool ready(bool block) { if (block) { ᐸꟷ(ch); } return true; }

            // (3) An else-if whose condition parks and then reads the ref local in the same condition (archive/zip's
            // checksumReader.Read, go/types' conversion): the read follows the call before any statement derives p again.
            public static nint elseIf(ж<N> Ꮡp)
            {
                ref var p = ref Ꮡp.DerefOrNull();

                if (p.v > 100) {
                    return -1;
                } else if (ready(false) && p.v == 1) {
                    p.v = 7;
                }

                return p.v;
            }
        }
        """;

    [TestMethod]
    public void ATwoVariableRangeWhoseHeaderParksDerivesItsRefsAgainAndKeepsTheWrite()
    {
        (string twins, Assembly assembly) = Build();

        StringAssert.Contains(twins, "foreach (var (i, v) in range((await itemsꞏtwin(false)))) {", "the header awaits");
        StringAssert.Contains(twins, "p = ref Ꮡp.DerefOrNull(); p.v += v * (i + 1);", "the deconstructing range's body derives p again on entry (Rule H)");
        Assert.AreEqual(Plain(assembly, "rangeWrite"), Twin(assembly, "rangeWrite"), "the write through p is read back after the loop");
    }

    [TestMethod]
    public void AOneVariableRangeWhoseHeaderParks_TheControl()
    {
        (string twins, Assembly assembly) = Build();

        StringAssert.Contains(twins, "p = ref Ꮡp.DerefOrNull(); p.v += e.Item2;", "Rule H, as before");
        Assert.AreEqual(Plain(assembly, "rangeWriteOne"), Twin(assembly, "rangeWriteOne"));
    }

    [TestMethod]
    public void AParkingIteratorStaysTheLoopsOwnAndAContinueRunsIt()
    {
        (string twins, Assembly assembly) = Build();

        StringAssert.Contains(twins, "for (; p.v < 10; await advanceꞏtwin(Ꮡp, false), p = ref Ꮡp.DerefOrNull())",
            "the await is a statement expression of the iterator, followed by p derived again");
        Assert.AreEqual(Plain(assembly, "forContinue"), Twin(assembly, "forContinue"), "continue runs the iterator; the condition reads p after the park");
    }

    [TestMethod]
    public void ALabeledContinueFromAnInnerLoopRunsTheParkingIterator()
    {
        (_, Assembly assembly) = Build();

        Assert.AreEqual(Plain(assembly, "forLabeled"), Twin(assembly, "forLabeled"));
    }

    [TestMethod]
    public void AnElseIfWhoseConditionReadsARefAfterAParkingCallKeepsThatCallPlain()
    {
        (string twins, Assembly assembly) = Build();

        StringAssert.Contains(twins, "} else if (ready(false) && p.v == 1) {",
            "the read of p follows the call in the else-if's own condition: the call stays plain (ref-blocked), as in an if's");
        Assert.AreEqual(Plain(assembly, "elseIf"), Twin(assembly, "elseIf"));
    }

    private static readonly CSharpParseOptions Options = new(LanguageVersion.CSharp13);

    private static (string twins, Assembly assembly) Build()
    {
        CSharpCompilation compilation = CSharpCompilation.Create("twinloop",
            [CSharpSyntaxTree.ParseText(Source, Options, path: @"C:\twinloop\loop.cs")],
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

    // A fresh box for each run, starting at v = 1.
    private static object Box(Assembly assembly)
    {
        Type n = assembly.GetType("go.loop_package+N", throwOnError: true)!;
        object value = Activator.CreateInstance(n)!;
        n.GetField("v")!.SetValue(value, (nint)1);

        System.Reflection.MethodInfo heap = typeof(golib::go.builtin).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method => method.Name == "Ꮡ" && method.IsGenericMethodDefinition && method.GetParameters() is [{ ParameterType.IsGenericParameter: true }]);

        return heap.MakeGenericMethod(n).Invoke(null, [value])!;
    }

    private static nint Plain(Assembly assembly, string name) =>
        (nint)Package(assembly).GetMethod(name, BindingFlags.Public | BindingFlags.Static)!.Invoke(null, [Box(assembly)])!;

    private static nint Twin(Assembly assembly, string name)
    {
        object task = Package(assembly).GetMethod(name + "ꞏtwin", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [Box(assembly)])!;
        object awaiter = task.GetType().GetMethod("GetAwaiter")!.Invoke(task, null)!;

        Assert.IsTrue((bool)awaiter.GetType().GetProperty("IsCompleted")!.GetValue(awaiter)!, "nothing in the run parks, so the twin completes");
        return (nint)awaiter.GetType().GetMethod("GetResult")!.Invoke(awaiter, null)!;
    }

    private static Type Package(Assembly assembly) => assembly.GetType("go.loop_package", throwOnError: true)!;

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
