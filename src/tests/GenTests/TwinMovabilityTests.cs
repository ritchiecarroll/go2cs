// TwinMovabilityTests.cs - Gbtc
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
/// M:N row 19, the movability rules (docs/phase4/DESIGN-mn-row19-movability.md): more of what is evaluated before a
/// parking call lets it move ahead of its statement and park, in Go's order. Each rule has rows that were ref-blocked
/// before it and now await, checked by value against what Go computes, and a near-miss that stays blocked by name.
/// </summary>
[TestClass]
public class TwinMovabilityTests
{
    private const string Source = """
        using System;
        using static go.builtin;

        namespace go;

        public static partial class move_package
        {
            internal static channel<nint> ch;

            // Writes through the box and can park (the twin awaits it); a run never blocks.
            public static nint Set(ж<nint> Ꮡp, nint v, bool block) { ref var p = ref Ꮡp.DerefOrNull(); p = v; if (block) { ᐸꟷ(ch); } return 3; }

            internal static nint counter;

            internal static nint other() { counter++; return 0; }

            // A ref-like first argument, as the u8 format of a Printf: it pins whatever follows unless the call moves.
            internal static nint show(ReadOnlySpan<byte> format, nint a, nint b) => a * 10 + b;

            internal static nint showS(ReadOnlySpan<byte> format, @string s, nint b) => len(s) * 10 + b;

            internal static nint showP(ReadOnlySpan<byte> format, slice<byte> s, nint b) => len(s) * 10 + b;

            // M7: the converter's constant shapes. A typed Go constant is an expression-bodied static property over a
            // literal; a Go string constant is a ref readonly static property over a static readonly field.
            internal static nint kindC => 2;

            internal static ref readonly @string fmtˢ => ref fmtˢᵇ;
            private static readonly @string fmtˢᵇ = "abcd"u8;

            // The M7 near-miss: a static property whose getter calls is not a constant.
            internal static nint notConst => other();

            public static nint constTyped()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                return show("t"u8, kindC, Set(Ꮡa, 2, false));
            }

            public static nint constString()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                return showS("s"u8, fmtˢ, Set(Ꮡa, 2, false));
            }

            public static nint constDefault()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                return showP("d"u8, default, Set(Ꮡa, 2, false));
            }

            public static nint notConstGetter()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                return show("n"u8, notConst, Set(Ꮡa, 2, false));
            }

            // M5: a throw evaluates its operand, then throws.
            public static nint throwArg()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                throw new InvalidOperationException(show("x"u8, a, Set(Ꮡa, 2, false)).ToString());
            }

            // M6: an if condition runs once, before either branch.
            public static nint ifCond()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                if (show("i"u8, a, Set(Ꮡa, 2, false)) == 23) {
                    return 1;
                }
                return 0;
            }

            // The statement-list rule's near-misses: an else-if's condition runs only when the first is false, and a
            // brace-less body is not a member of a statement list. Both stay blocked by name; constants keep Go's value.
            public static nint elseIf()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                if (counter == 99) {
                    return 5;
                }
                else if (show("e"u8, 1, Set(Ꮡa, 2, false)) == 13) {
                    return 1;
                }
                return 0;
            }

            public static nint braceless()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                if (counter != 99)
                    return show("b"u8, 1, Set(Ꮡa, 2, false));
                return 0;
            }

            // M3: a tuple literal's elements evaluate left to right, as a Go multi-value return's operands do.
            public static (nint, nint) tupleElems()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                return (a, show("u"u8, 1, Set(Ꮡa, 2, false)));
            }

            // M4: a compound store into a plain local reads the local after the call, as gc reads s in `s += f()`.
            public static nint compoundLocal()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                nint n = 1;
                n += show("c"u8, 1, Set(Ꮡa, 2, false));
                return n;
            }

            // The M4 near-miss: a compound store into a field stays blocked by name.
            internal static nint total;

            public static nint compoundField()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                total += show("f"u8, 1, Set(Ꮡa, 2, false));
                return total;
            }
        }
        """;

    [TestMethod]
    public void AConvertedConstantDoesNotPinTheParkingCall_M7()
    {
        (string twins, Assembly assembly) = Build();

        foreach (string name in new[] { "constTyped", "constString", "constDefault" })
            Assert.IsFalse(twins.Contains("|" + name + "|Set|"), "no ref-blocked row names " + name + "\n" + twins);

        StringAssert.Contains(twins, "await Setꞏtwin(Ꮡa, 2, false)", "the parking call awaits");
        Assert.AreEqual((nint)23, Twin(assembly, "constTyped"), "2*10+3");
        Assert.AreEqual((nint)43, Twin(assembly, "constString"), "len(\"abcd\")*10+3");
        Assert.AreEqual((nint)3, Twin(assembly, "constDefault"), "len(nil)*10+3");
    }

    [TestMethod]
    public void AStaticPropertyWhoseGetterCallsStillPinsTheParkingCall_M7NearMiss()
    {
        (string twins, Assembly assembly) = Build();

        StringAssert.Contains(twins, "|notConstGetter|Set|ref-like-before|earlier:IdentifierName", "the getter runs code, so it is ordered before the call\n" + twins);
        Assert.AreEqual(Plain(assembly, "notConstGetter"), Twin(assembly, "notConstGetter"));
    }

    [TestMethod]
    public void AThrowOperandMovesAndReadsAfterTheCall_M5()
    {
        (string twins, Assembly assembly) = Build();

        Assert.IsFalse(twins.Contains("|throwArg|Set|"), "no ref-blocked row names throwArg\n" + twins);
        Assert.AreEqual("23", Thrown(() => Twin(assembly, "throwArg")), "gc reads a after the call: 2*10+3");
    }

    [TestMethod]
    public void AnIfConditionMovesAheadOfTheIf_M6()
    {
        (string twins, Assembly assembly) = Build();

        Assert.IsFalse(twins.Contains("|ifCond|Set|"), "no ref-blocked row names ifCond\n" + twins);
        Assert.AreEqual((nint)1, Twin(assembly, "ifCond"), "the condition compared 2*10+3 with 23");
    }

    [TestMethod]
    public void AnElseIfAndABracelessBodyStayBlockedByName_StatementListRule()
    {
        (string twins, Assembly assembly) = Build();

        StringAssert.Contains(twins, "|elseIf|Set|ref-like-before|nested:IfStatement", "an else-if runs only after the first condition\n" + twins);
        StringAssert.Contains(twins, "|braceless|Set|ref-like-before|nested:ReturnStatement", "a brace-less body is no statement-list member\n" + twins);
        Assert.AreEqual((nint)1, Twin(assembly, "elseIf"));
        Assert.AreEqual((nint)13, Twin(assembly, "braceless"));
    }

    [TestMethod]
    public void ATupleElementIsOrderedLikeAnArgument_M3()
    {
        (string twins, Assembly assembly) = Build();

        Assert.IsFalse(twins.Contains("|tupleElems|Set|"), "no ref-blocked row names tupleElems\n" + twins);
        Assert.AreEqual(((nint)2, (nint)13), Twin(assembly, "tupleElems"), "gc reads a after the call: (2, 1*10+3)");
    }

    [TestMethod]
    public void ACompoundStoreIntoAPlainLocalMoves_M4()
    {
        (string twins, Assembly assembly) = Build();

        Assert.IsFalse(twins.Contains("|compoundLocal|Set|"), "no ref-blocked row names compoundLocal\n" + twins);
        Assert.AreEqual((nint)14, Twin(assembly, "compoundLocal"), "1 + 1*10+3");

        StringAssert.Contains(twins, "|compoundField|Set|ref-like-before|in:AddAssignmentExpression", "a field target stays blocked by name\n" + twins);
        Assert.AreEqual((nint)13, Twin(assembly, "compoundField"));
    }

    private static readonly CSharpParseOptions Options = new(LanguageVersion.CSharp13);

    private static (string twins, Assembly assembly) Build()
    {
        CSharpCompilation compilation = CSharpCompilation.Create("twinmove",
            [CSharpSyntaxTree.ParseText(Source, Options, path: @"C:\twinmove\move.cs")],
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

    // The message of the exception a call throws, through reflection's wrapper.
    private static string Thrown(Func<object> call)
    {
        try
        {
            call();
        }
        catch (TargetInvocationException wrapped) when (wrapped.InnerException is InvalidOperationException thrown)
        {
            return thrown.Message;
        }

        Assert.Fail("the call did not throw");
        return "";
    }

    private static Type Package(Assembly assembly) => assembly.GetType("go.move_package", throwOnError: true)!;

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
