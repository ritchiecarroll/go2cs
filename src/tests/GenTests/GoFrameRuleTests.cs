// GoFrameRuleTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// GO2CS0016, the GoFrame rule (M:N item 8; docs/phase4/DATA-mn-spike-2.md 9.8 and 10.6): golib's <c>GoFrame</c> is a
/// plain struct, so the compiler no longer refuses a frame that escapes, and the source generator refuses every use but a
/// local initialised to <c>default</c>, a <c>ref</c> parameter or argument, and the receiver of its own members.
/// </summary>
/// <remarks>
/// <para>
/// Each arm compiles C# against the REAL golib (the plain-struct GoFrame) and runs EVERY generator go2cs-gen ships, not the
/// rule's own type by name, so a refusal row is red wherever no generator reports the code. Each refused input is valid
/// C#: the arm asserts the compilation has no error of its own, so what it reads is the rule and nothing else. The line a
/// refusal must name carries the comment <c>// &lt;- GO2CS0016</c>.
/// </para>
/// <para>
/// RED before the rule existed: every refusal arm failed (no GO2CS0016 anywhere) and the acceptance arm passed.
/// </para>
/// </remarks>
[TestClass]
public class GoFrameRuleTests
{
    private const string Code = "GO2CS0016";

    private const string SiteMarker = "// <- " + Code;

    private const string Usings =
        """
        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using go;
        using static go.builtin;

        """;

    [TestMethod]
    public void AcceptsTheConvertedShapes()
    {
        // The converter's emission for a deferring function and a deferring func literal, a ref-parameter helper that
        // passes the frame on by ref, its own members, typeof and nameof, and an async method holding a frame across an
        // await (the reason GoFrame stopped being a ref struct).
        Result result = Run(Usings +
            """
            public static class pkg
            {
                public static void f()
                {
                    GoFrame ᒐ = default;
                    try
                    {
                        defer(() => { }, ref ᒐ);
                        Action literal = () =>
                        {
                            GoFrame ᒐ1 = default(GoFrame);
                            try { defer(() => { }, ref ᒐ1); }
                            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                            finally { ᒐ1.Run(); }
                        };
                        literal();
                        helper(ref ᒐ);
                        _ = ᒐ.Count + nameof(ᒐ).Length + typeof(GoFrame).Name.Length;
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }

                private static void helper(ref GoFrame frame)
                {
                    frame.Push(() => { });
                    defer(() => { }, ref frame);
                }

                public static async Task twin()
                {
                    GoFrame ᒐ = default;
                    try
                    {
                        defer(() => { }, ref ᒐ);
                        await Task.Yield();
                    }
                    finally { ᒐ.Run(); }
                }
            }
            """);

        Assert.AreEqual(0, result.Refusals.Length, "accepted shapes were refused:\n" + string.Join("\n", result.Refusals));
    }

    [TestMethod]
    public void RefusesAByValueParameter() => AssertRefused("takes a GoFrame by value",
        """
        public static class pkg
        {
            public static void take(GoFrame frame) { } // <- GO2CS0016
        }
        """);

    [TestMethod]
    public void RefusesAnInParameter() => AssertRefused("takes a GoFrame as 'in'",
        """
        public static class pkg
        {
            public static int take(in GoFrame frame) => frame.Count; // <- GO2CS0016
        }
        """);

    [TestMethod]
    public void RefusesAReturn() => AssertRefused("returns a GoFrame",
        """
        public static class pkg
        {
            public static GoFrame make() => default; // <- GO2CS0016
        }
        """);

    [TestMethod]
    public void RefusesAField() => AssertRefused("stores a GoFrame",
        """
        public sealed class holder
        {
            public GoFrame frame; // <- GO2CS0016
        }
        """);

    [TestMethod]
    public void RefusesAProperty() => AssertRefused("stores a GoFrame",
        """
        public sealed class holder
        {
            public GoFrame Frame { get; set; } // <- GO2CS0016
        }
        """);

    [TestMethod]
    public void RefusesAnArrayElement() => AssertRefused("as an array element",
        """
        public static class pkg
        {
            public static int count()
            {
                var frames = new GoFrame[2]; // <- GO2CS0016
                return frames.Length;
            }
        }
        """);

    [TestMethod]
    public void RefusesACopyIntoALocal() => AssertRefused("is copied",
        """
        public static class pkg
        {
            public static void f()
            {
                GoFrame ᒐ = default;
                var copy = ᒐ; // <- GO2CS0016
                copy.Run();
                ᒐ.Run();
            }
        }
        """);

    [TestMethod]
    public void RefusesAnAssignment() => AssertRefused("is assigned",
        """
        public static class pkg
        {
            public static void f(ref GoFrame other)
            {
                GoFrame ᒐ = default;
                ᒐ = other; // <- GO2CS0016
                ᒐ.Run();
            }
        }
        """);

    [TestMethod]
    public void RefusesBoxing() => AssertRefused("is boxed",
        """
        public static class pkg
        {
            public static object f()
            {
                GoFrame ᒐ = default;
                object boxed = ᒐ; // <- GO2CS0016
                return boxed;
            }
        }
        """);

    [TestMethod]
    public void RefusesAGenericArgument()
    {
        AssertRefused("as a generic argument",
            """
            public static class pkg
            {
                public static int f() => new List<GoFrame>().Count; // <- GO2CS0016
            }
            """);

        // Inferred: the call passes the frame by ref, and T is GoFrame without the type ever being written.
        AssertRefused("as a generic argument",
            """
            public static class pkg
            {
                private static void keep<T>(ref T value) { }

                public static void f()
                {
                    GoFrame ᒐ = default;
                    keep(ref ᒐ); // <- GO2CS0016
                    ᒐ.Run();
                }
            }
            """);
    }

    [TestMethod]
    public void RefusesALambdaCapture() => AssertRefused("captured by a lambda or local function",
        """
        public static class pkg
        {
            public static void f()
            {
                GoFrame ᒐ = default;
                Action later = () => ᒐ.Run(); // <- GO2CS0016
                later();
            }
        }
        """);

    [TestMethod]
    public void RefusesALocalFunctionCapture() => AssertRefused("captured by a lambda or local function",
        """
        public static class pkg
        {
            public static void f()
            {
                GoFrame ᒐ = default;
                run();
                void run() => ᒐ.Run(); // <- GO2CS0016
            }
        }
        """);

    [TestMethod]
    public void RefusesARefLocal() => AssertRefused("is a ref local",
        """
        public static class pkg
        {
            public static void f()
            {
                GoFrame ᒐ = default;
                ref GoFrame alias = ref ᒐ; // <- GO2CS0016
                alias.Run();
            }
        }
        """);

    [TestMethod]
    public void AcceptsARefArgumentInACallThatDoesNotBindYet()
    {
        // A generator binds the compilation WITHOUT the other generators' output. Converted code names generated members
        // (FieldAccessGenerator's field boxes, `g.ᏑcgoCtxt` in runtime's cgocallbackg1), so in the rule's view such a
        // call does not bind, and the frame's `ref` argument must still be read as the allowed shape it is. The input's
        // own error stands for the member a generator would have added.
        Result result = Run(Usings +
            """
            public sealed class g { }

            public static class pkg
            {
                public static void f(g gp)
                {
                    GoFrame ᒐ = default;
                    try
                    {
                        defer((g gpΔ1) => { var p = gpΔ1.ᏑnotYetGenerated; }, gp, ref ᒐ);
                    }
                    finally { ᒐ.Run(); }
                }
            }
            """);

        Assert.IsTrue(result.CompileErrors.Any(error => error.Id == "CS1061"), "the input must stand for a call that does not bind:\n" +
            string.Join("\n", result.CompileErrors));
        Assert.AreEqual(0, result.Refusals.Length, "a ref argument in a call that does not bind was refused:\n" + string.Join("\n", result.Refusals));
    }

    [TestMethod]
    public void RefusesAByValueExtensionReceiver() => AssertRefused("is passed by value",
        """
        public static class frames
        {
            public static int Ext(this GoFrame frame) => frame.Count;
        }

        public static class pkg
        {
            public static int f()
            {
                GoFrame ᒐ = default;
                return ᒐ.Ext(); // <- GO2CS0016
            }
        }
        """);

    [TestMethod]
    public void RefusesAGlobalUsingAlias()
    {
        // A global alias lets every file of the project name the type without the word GoFrame in it.
        const string Source =
            """
            global using Frame = go.GoFrame; // <- GO2CS0016
            """;

        AssertRefusedAt(Run(Source), "global using alias", Source, 0);
    }

    [TestMethod]
    public void RefusesACopyInAFileThatNeverNamesGoFrame()
    {
        // A typeless lambda parameter takes its type from a delegate declared in ANOTHER file, so the file holding the
        // copy never names the type: the rule must still read it.
        const string Declaring = Usings +
            """
            namespace frames;

            public delegate void FrameAction(ref GoFrame frame);
            """;

        const string Using =
            """
            namespace consumer;

            public static class pkg
            {
                public static frames.FrameAction make() => (ref f) =>
                {
                    var copy = f; // <- GO2CS0016
                    copy.Run();
                };
            }
            """;

        Result result = Run(Declaring, Using);
        AssertRefusedAt(result, "is copied", Using, 1);
    }

    [TestMethod]
    public void IsAnErrorThatCannotBeConfigured()
    {
        Result result = Run(Usings +
            """
            public static class pkg
            {
                public static void take(GoFrame frame) { }
            }
            """);

        Diagnostic refusal = result.Refusals.FirstOrDefault() ?? throw new AssertFailedException($"no {Code} was reported");

        Assert.AreEqual(DiagnosticSeverity.Error, refusal.Severity);
        Assert.AreEqual(DiagnosticSeverity.Error, refusal.Descriptor.DefaultSeverity);
        CollectionAssert.Contains(refusal.Descriptor.CustomTags.ToList(), WellKnownDiagnosticTags.NotConfigurable,
            "the rule must not be configurable: an .editorconfig severity, NoWarn or a pragma could otherwise turn it off");
    }

    private static void AssertRefused(string fragment, string body)
    {
        string source = Usings + body;
        AssertRefusedAt(Run(source), fragment, source, 0);
    }

    // The refusal must be reported on the marked line of source number `sourceIndex`, with the fragment in its message, and
    // the input must otherwise compile.
    private static void AssertRefusedAt(Result result, string fragment, string source, int sourceIndex)
    {
        Assert.AreEqual(0, result.CompileErrors.Length, "the input must be valid C#:\n" + string.Join("\n", result.CompileErrors));

        string[] lines = source.Replace("\r\n", "\n").Split('\n');
        int line = Array.FindIndex(lines, text => text.Contains(SiteMarker));
        Assert.IsTrue(line >= 0, "the input marks no site");

        string path = FileName(sourceIndex);
        bool found = result.Refusals.Any(diagnostic =>
            diagnostic.Location.GetLineSpan().Path == path &&
            diagnostic.Location.GetLineSpan().StartLinePosition.Line == line &&
            diagnostic.GetMessage().Contains(fragment, StringComparison.Ordinal));

        Assert.IsTrue(found, $"no {Code} containing \"{fragment}\" at {path}:{line + 1}; reported:\n" +
            (result.Refusals.Length == 0 ? "(none)" : string.Join("\n", result.Refusals)));
    }

    private static string FileName(int index) => $"input{index}.cs";

    private sealed record Result(ImmutableArray<Diagnostic> Refusals, ImmutableArray<Diagnostic> CompileErrors);

    private static readonly CSharpParseOptions s_parseOptions = new(LanguageVersion.Preview);

    private static Result Run(params string[] sources)
    {
        CSharpCompilation compilation = CSharpCompilation.Create("frames",
            sources.Select((source, index) => CSharpSyntaxTree.ParseText(source, s_parseOptions, FileName(index))),
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(Generators(), parseOptions: s_parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out ImmutableArray<Diagnostic> generatorDiagnostics);

        return new Result(
            [.. generatorDiagnostics.Where(diagnostic => diagnostic.Id == Code)],
            [.. output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)]);
    }

    // Every generator go2cs-gen ships, found by its [Generator] attribute.
    private static IEnumerable<ISourceGenerator> Generators() =>
        typeof(LiteralInterceptorGenerator).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && type.GetCustomAttributes(typeof(GeneratorAttribute), false).Length > 0)
            .Select(type => Activator.CreateInstance(type) switch
            {
                IIncrementalGenerator incremental => incremental.AsSourceGenerator(),
                ISourceGenerator source => source,
                _ => throw new InvalidOperationException($"{type} is marked [Generator] and is no generator")
            });

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.GoFrame).Assembly.Location));
    }
}
