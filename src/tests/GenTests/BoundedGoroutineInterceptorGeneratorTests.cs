// BoundedGoroutineInterceptorGeneratorTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// BoundedGoroutineInterceptorGenerator: a <c>go</c> statement the converter proved cannot recurse is recorded in
/// package_info.cs by the position of its <c>goǃ</c> call, and the generator starts exactly that call on the bounded
/// reservation, through the same rung's <c>GoStack</c>-marked twin, with the call's own caller information. A record
/// that does not land on a call of an unmarked rung is GO2CS0010, never a skipped record, so a stale record cannot put an
/// unproved goroutine on the small reservation.
/// </summary>
/// <remarks>
/// Each arm runs the REAL generator over an in-memory compilation, then EMITS and RUNS it: golib's stand-in launchers
/// report which rung ran, with which reservation and from which line, so an interception is proved by what the program
/// does, not by the generated text. The planted-stale arm was RED with the generator's landing check disabled (the record
/// was skipped and the build went green), and green with it restored.
/// </remarks>
[TestClass]
public class BoundedGoroutineInterceptorGeneratorTests
{
    // golib's shapes: the unmarked rungs, each ending with the caller-information pair, and their GoStack-marked twins.
    // A launcher runs its goroutine inline and appends "<reservation>:<rung>@<caller line>" to Trace.
    private const string Golib =
        """
        using System.Runtime.CompilerServices;

        namespace go
        {
            public enum GoStack { Default, Bounded }

            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public sealed class GoBoundedGoroutineAttribute(string file, int line, int column) : System.Attribute
            {
                public string File => file;
                public int Line => line;
                public int Column => column;
            }

            public static class builtin
            {
                public static string Trace = "";

                private static void Note(GoStack stack, string rung, int line) => Trace += (Trace.Length > 0 ? "," : "") + $"{(stack == GoStack.Bounded ? "bounded" : "default")}:{rung}@{line}";

                public static void goǃ(System.Threading.WaitCallback action, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0) { Note(GoStack.Default, "WaitCallback", ᴛcallerLine); action(null); }
                public static void goǃ(System.Action action, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0) { Note(GoStack.Default, "Action", ᴛcallerLine); action(); }
                public static void goǃ<T>(System.Action<T> action, T arg, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0) { Note(GoStack.Default, "Action1", ᴛcallerLine); action(arg); }
                public static void goǃ<T1, T2>(System.Action<T1, T2> action, T1 arg1, T2 arg2, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0) { Note(GoStack.Default, "Action2", ᴛcallerLine); action(arg1, arg2); }
                public static void goǃ<T, TResult>(System.Func<T, TResult> action, T arg, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0) { Note(GoStack.Default, "Func1", ᴛcallerLine); action(arg); }

                public static void goǃ(System.Threading.WaitCallback action, GoStack stack, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0) { Note(stack, "WaitCallback", ᴛcallerLine); action(null); }
                public static void goǃ(System.Action action, GoStack stack, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0) { Note(stack, "Action", ᴛcallerLine); action(); }
                public static void goǃ<T>(System.Action<T> action, T arg, GoStack stack, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0) { Note(stack, "Action1", ᴛcallerLine); action(arg); }
                public static void goǃ<T1, T2>(System.Action<T1, T2> action, T1 arg1, T2 arg2, GoStack stack, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0) { Note(stack, "Action2", ᴛcallerLine); action(arg1, arg2); }
                public static void goǃ<T, TResult>(System.Func<T, TResult> action, T arg, GoStack stack, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0) { Note(stack, "Func1", ᴛcallerLine); action(arg); }

                public static void other(System.Action action) => action();
            }
        }
        """;

    private static readonly CSharpParseOptions s_parseOptions =
        new CSharpParseOptions(LanguageVersion.Latest).WithFeatures([new("InterceptorsNamespaces", "go2cs.Interceptors")]);

    private static IEnumerable<MetadataReference> CoreReferences()
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        return
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Threading.ThreadPool.dll"))
        ];
    }

    private sealed record Run(Compilation Updated, ImmutableArray<Diagnostic> GeneratorDiagnostics, ImmutableArray<GeneratedSourceResult> Sources);

    // The converted file: main_package.Body() runs `body` and answers golib's trace. Every `goǃ(` the body holds is
    // written on a line of its own, so a statement's line is the line its `goǃ` text starts on.
    private static string MainSource(string body) =>
        $$"""
        using static go.builtin;

        namespace go
        {
            public static partial class main_package
            {
                private struct hidden { public int n; }

                private static int Leaf(int n) => n + 1;

                public static string Body()
                {
                    builtin.Trace = "";
                    {{body}}
                    return builtin.Trace;
                }
            }
        }
        """;

    // The position of the nth (0-based) `goǃ(` in the converted file: its 1-based line and UTF-16 column.
    private static (int line, int column) Site(string source, int nth)
    {
        int index = -1;

        for (int i = 0; i <= nth; i++)
        {
            index = source.IndexOf("goǃ(", index + 1, StringComparison.Ordinal);
            Assert.IsTrue(index >= 0, $"the fixture holds no goǃ call #{nth}");
        }

        string before = source.Substring(0, index);
        int line = before.Count(c => c == '\n') + 1;
        int column = index - (before.LastIndexOf('\n') + 1) + 1;

        return (line, column);
    }

    private static string Record(string file, (int line, int column) site) => $"[assembly: go.GoBoundedGoroutine(\"{file}\", {site.line}, {site.column})]";

    private static Run Generate(string mainSource, IEnumerable<string> records, IIncrementalGenerator? sibling = null, string mainPath = "main.cs")
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            "bounded-goroutine-test",
            [
                CSharpSyntaxTree.ParseText(Golib, s_parseOptions, path: "golib_stand_in_impl.cs"),
                CSharpSyntaxTree.ParseText(string.Join("\r\n", records), s_parseOptions, path: "package_info.cs"),
                CSharpSyntaxTree.ParseText(mainSource, s_parseOptions, path: mainPath)
            ],
            CoreReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        ISourceGenerator[] generators = sibling is null
            ? [new BoundedGoroutineInterceptorGenerator().AsSourceGenerator()]
            : [new BoundedGoroutineInterceptorGenerator().AsSourceGenerator(), sibling.AsSourceGenerator()];

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generators, parseOptions: s_parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation updated, out ImmutableArray<Diagnostic> diagnostics);

        return new Run(updated, diagnostics, [.. driver.GetRunResult().Results.SelectMany(result => result.GeneratedSources)]);
    }

    // Emits the compilation, runs main_package.Body() and answers golib's trace.
    private static string Execute(Compilation compilation)
    {
        using MemoryStream image = new();
        EmitResult emitted = compilation.Emit(image);

        Assert.IsTrue(emitted.Success, $"the fixture must emit clean: {string.Join("; ", emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))}");

        Assembly assembly = Assembly.Load(image.ToArray());

        return (string)assembly.GetType("go.main_package")!.GetMethod("Body")!.Invoke(null, null)!;
    }

    private static string[] Mismatches(Run run) =>
        [.. run.GeneratorDiagnostics.Where(diagnostic => diagnostic.Id == "GO2CS0010").Select(diagnostic => diagnostic.GetMessage())];

    [TestMethod]
    [DataRow("goǃ(() => { });", "Action", DisplayName = "a parameterless lambda")]
    [DataRow("goǃ(ᴛ1 => Leaf(ᴛ1), 4);", "Func1", DisplayName = "a one-argument expression lambda whose call returns a value")]
    [DataRow("goǃ(ᴛ1 => { Leaf(ᴛ1); }, 4);", "Action1", DisplayName = "a one-argument block lambda")]
    [DataRow("goǃ((ᴛ1, ᴛ2) => { }, new hidden(), 2);", "Action2", DisplayName = "a generic rung over a private type no generated file can name")]
    [DataRow("goǃ(Leaf, 4);", "Func1", DisplayName = "a method group with a result")]
    public void ARecordedGoStatementStartsOnTheBoundedReservationFromItsOwnLine(string statement, string rung)
    {
        string source = MainSource(statement);
        (int line, int column) site = Site(source, 0);
        Run run = Generate(source, [Record("main.cs", site)]);

        Assert.AreEqual(0, run.GeneratorDiagnostics.Length, string.Join("; ", run.GeneratorDiagnostics));
        Assert.AreEqual(1, run.Sources.Length, "one generated file for the compilation's records");
        Assert.AreEqual($"bounded:{rung}@{site.line}", Execute(run.Updated));
    }

    // Only the recorded call moves: the unrecorded one beside it keeps the default reservation.
    [TestMethod]
    public void AGoStatementWithoutARecordKeepsTheDefaultReservation()
    {
        string source = MainSource("goǃ(() => { });\r\n                goǃ(() => { });");
        (int line, int column) recorded = Site(source, 1);
        Run run = Generate(source, [Record("main.cs", recorded)]);

        Assert.AreEqual(0, run.GeneratorDiagnostics.Length, string.Join("; ", run.GeneratorDiagnostics));
        Assert.AreEqual($"default:Action@{recorded.line - 1},bounded:Action@{recorded.line}", Execute(run.Updated));
    }

    // The control: without a record nothing is generated and every call keeps the default reservation.
    [TestMethod]
    public void WithoutRecordsNothingIsGenerated()
    {
        string source = MainSource("goǃ(() => { });");
        Run run = Generate(source, []);

        Assert.AreEqual(0, run.GeneratorDiagnostics.Length);
        Assert.AreEqual(0, run.Sources.Length);
        Assert.AreEqual($"default:Action@{Site(source, 0).line}", Execute(run.Updated));
    }

    // A sibling generator's member, like a field property GoFieldAccess records: every generator reads the same input, so
    // to this one the goroutine's body names a member that does not exist yet. The call still binds one rung.
    private sealed class AddsAMember : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context) =>
            context.RegisterSourceOutput(context.CompilationProvider, static (production, _) =>
                production.AddSource("added.g.cs", "namespace go { public static partial class main_package { public static int Added => 7; } }"));
    }

    [TestMethod]
    [DataRow("goǃ(() => { int x = Added; });", "Action", DisplayName = "a lambda body naming a sibling generator's member")]
    [DataRow("goǃ(ᴛ1 => Leaf(ᴛ1), Added);", "Func1", DisplayName = "an argument the sibling supplies, to an expression lambda")]
    [DataRow("goǃ(ᴛ1 => { Leaf(ᴛ1); }, Added);", "Action1", DisplayName = "an argument the sibling supplies, to a block lambda")]
    [DataRow("goǃ(Leaf, Added);", "Func1", DisplayName = "a method group with a result, beside a sibling member")]
    public void ACallThatBindsOnlyAfterASiblingGeneratorIsStillIntercepted(string statement, string rung)
    {
        string source = MainSource(statement);
        (int line, int column) site = Site(source, 0);
        Run run = Generate(source, [Record("main.cs", site)], sibling: new AddsAMember());

        Assert.AreEqual(0, run.GeneratorDiagnostics.Length, string.Join("; ", run.GeneratorDiagnostics));
        Assert.AreEqual($"bounded:{rung}@{site.line}", Execute(run.Updated));
    }

    // (a) of the proof: a record that does not land on a call of an unmarked goǃ rung fails the build at the record.
    [TestMethod]
    public void AStaleRecordFailsTheBuild()
    {
        string source = MainSource("int unrelated = Leaf(1);\r\n                goǃ(() => { });");
        (int line, int column) site = Site(source, 0);
        Run run = Generate(source, [Record("main.cs", (site.line - 1, site.column))]);

        string[] mismatches = Mismatches(run);

        Assert.AreEqual(1, mismatches.Length, string.Join("; ", run.GeneratorDiagnostics));
        StringAssert.Contains(mismatches[0], "does not land on the first token of a goǃ call");
        Assert.AreEqual(DiagnosticSeverity.Error, run.GeneratorDiagnostics.Single().Severity);
        Assert.AreEqual("package_info.cs", Path.GetFileName(run.GeneratorDiagnostics.Single().Location.SourceTree?.FilePath ?? run.GeneratorDiagnostics.Single().Location.GetLineSpan().Path));
    }

    [TestMethod]
    [DataRow(0, 1, "does not land on the first token of a goǃ call", DisplayName = "one column past the call")]
    [DataRow(0, -1, "does not land on the first token of a goǃ call", DisplayName = "one column before the call")]
    public void ARecordOffTheCallsFirstTokenFailsTheBuild(int lineShift, int columnShift, string reason)
    {
        string source = MainSource("goǃ(() => { });");
        (int line, int column) site = Site(source, 0);
        Run run = Generate(source, [Record("main.cs", (site.line + lineShift, site.column + columnShift))]);

        string[] mismatches = Mismatches(run);

        Assert.AreEqual(1, mismatches.Length, string.Join("; ", run.GeneratorDiagnostics));
        StringAssert.Contains(mismatches[0], reason);
    }

    [TestMethod]
    public void ARecordOfAFileTheCompilationLacksFailsTheBuild()
    {
        string source = MainSource("goǃ(() => { });");
        Run run = Generate(source, [Record("other.cs", Site(source, 0))]);

        string[] mismatches = Mismatches(run);

        Assert.AreEqual(1, mismatches.Length, string.Join("; ", run.GeneratorDiagnostics));
        StringAssert.Contains(mismatches[0], "names no file of this compilation");
    }

    // A call of a marked rung or of another method is not one this record can mean.
    [TestMethod]
    [DataRow("goǃ(() => { }, GoStack.Bounded);", "not of an unmarked goǃ rung", DisplayName = "a GoStack-marked rung")]
    public void ARecordOnAnotherCallFailsTheBuild(string statement, string reason)
    {
        string source = MainSource(statement);
        Run run = Generate(source, [Record("main.cs", Site(source, 0))]);

        string[] mismatches = Mismatches(run);

        Assert.AreEqual(1, mismatches.Length, string.Join("; ", run.GeneratorDiagnostics));
        StringAssert.Contains(mismatches[0], reason);
    }

    // The interceptor is a frame of its own in go2cs-gen's interceptor namespace, which golib's creator walk skips.
    [TestMethod]
    public void TheInterceptorLivesInTheInterceptorNamespaceAsAHiddenFrame()
    {
        string source = MainSource("goǃ(() => { });");
        Run run = Generate(source, [Record("main.cs", Site(source, 0))]);
        string generated = run.Sources.Single().SourceText.ToString();

        StringAssert.Contains(generated, "namespace go2cs.Interceptors");
        StringAssert.Contains(generated, "global::System.Diagnostics.StackTraceHidden");
        StringAssert.Contains(generated, "global::go.GoStack.Bounded");
    }
}
