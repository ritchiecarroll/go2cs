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
        using go;
        using static go.builtin;

        [assembly: GoImplement<global::go.move_package.Buf, global::go.move_package.Writer>(Pointer = true)]
        [assembly: GoImplement<global::go.move_package.Val, global::go.move_package.Namer>]
        [assembly: GoImplement<global::go.move_package.Namer, global::go.move_package.Writer>]

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

            // M8: a deconstruction into new locals moves as a local declaration does (mime's `var (r, errΔ1) = …`).
            internal static (nint, nint) pairS(ReadOnlySpan<byte> format, nint a, nint b) => (a, b);

            public static nint deconNew()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                var (r, e) = pairS("d"u8, 1, Set(Ꮡa, 2, false));
                return r * 100 + e * 10 + a;
            }

            // The M8 near-miss: a deconstruction that mixes a new local with an existing target stays blocked by name.
            public static nint deconMixed()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                nint e = 0;
                (var r, e) = pairS("m"u8, 1, Set(Ꮡa, 2, false));
                return r * 100 + e * 10 + a;
            }

            // M2: the converter's form of a Go interface conversion of a variable is a generated adapter over it.
            public partial struct Buf { public nint id; }

            public partial interface Writer { nint Write(nint p); }

            internal static nint Write(this ж<Buf> Ꮡb, nint p) => Ꮡb.DerefOrNull().id * 10 + p;

            public partial struct Val { public nint id; }

            public partial interface Namer { nint Write(nint p); }

            internal static nint Write(this Val v, nint p) => v.id * 10 + p;

            internal static nint useW(ReadOnlySpan<byte> format, Writer w, nint b) => w.Write(b);

            public static nint SetP(ж<ж<Buf>> Ꮡp, ж<Buf> v, bool block) { Ꮡp.DerefOrNull() = v; if (block) { ᐸꟷ(ch); } return 3; }

            public static nint SetN(ж<Namer> Ꮡp, Namer v, bool block) { Ꮡp.DerefOrNull() = v; if (block) { ᐸꟷ(ch); } return 3; }

            // A pointer adapter: gc converts `w` after the call reassigns it.
            public static nint ptrAdapter()
            {
                heap(new Buf(id: 1), out var Ꮡb1);
                heap(new Buf(id: 2), out var Ꮡb2);
                ref var pb = ref heap<ж<Buf>>(Ꮡb1, out var Ꮡpb);
                return useW("p"u8, new BufжWriter(pb), SetP(Ꮡpb, Ꮡb2, false));
            }

            // A value adapter (an interface converted to another interface, fmt's `new fmt_StateᴠWriter(s)`) copies its
            // operand: gc reads `n` after the call reassigns it.
            public static nint valAdapter()
            {
                ref var n = ref heap<Namer>(new Val(id: 1), out var Ꮡn);
                return useW("v"u8, new NamerᴠWriter(n), SetN(Ꮡn, new Val(id: 2), false));
            }

            // The M2 near-miss: an adapter over a CALL is not a pure read; the call is ordered before the parking call.
            internal static ж<Buf> pick(ж<Buf> b) { counter++; return b; }

            public static nint adapterOverCall()
            {
                heap(new Buf(id: 1), out var Ꮡb1);
                ref var a = ref heap<nint>(1, out var Ꮡa);
                return useW("c"u8, new BufжWriter(pick(Ꮡb1)), Set(Ꮡa, 2, false));
            }

            // M1: an earlier call spills in Go order, into its own temporary, and the parking call moves after it.
            internal static nint show3(ReadOnlySpan<byte> format, nint x, nint y, nint z) => x * 100 + y * 10 + z;

            internal static nint bump(ж<nint> Ꮡp) { Ꮡp.DerefOrNull() += 1; return 7; }

            internal static nint mul(ж<nint> Ꮡp, nint k) { ref var p = ref Ꮡp.DerefOrNull(); p *= k; return p; }

            // Go: bump runs, then Set, and a is read after both: 7, 5, 3. C# order reads a between them: 7, 2, 3.
            public static nint earlierCall()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                return show3("m"u8, bump(Ꮡa), a, Set(Ꮡa, 5, false));
            }

            // Two earlier calls keep their text order (Go's call order): 2, then 6.
            public static nint twoEarlier()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                return show3("o"u8, mul(Ꮡa, 2), mul(Ꮡa, 3), Set(Ꮡa, 4, false));
            }

            // The M1 near-misses: an earlier call whose result is used by ref (a spill would copy it) and one whose result
            // is ref-like (it cannot live across the await) stay blockers, by name.
            internal static ref nint slot(ж<nint> Ꮡp) => ref Ꮡp.DerefOrNull();

            internal static nint showR(ReadOnlySpan<byte> format, ref nint x, nint b) { x += 100; return b; }

            public static nint refArg()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                nint r = showR("r"u8, ref slot(Ꮡa), Set(Ꮡa, 2, false));
                return a * 10 + r;
            }

            internal static ReadOnlySpan<byte> spanOf() => "xy"u8;

            internal static nint showSpan(ReadOnlySpan<byte> format, ReadOnlySpan<byte> s, nint b) => s.Length * 10 + b;

            public static nint spanArg()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                return showSpan("y"u8, spanOf(), Set(Ꮡa, 2, false));
            }

            // M9: a logical operator whose right operand must await is lowered to an if, through a fresh temporary.
            public static bool andReturn()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                return a == 1 && show("r"u8, a, Set(Ꮡa, 2, false)) == 23;
            }

            public static bool orReturn()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                return a != 1 || show("o"u8, a, Set(Ꮡa, 2, false)) == 23;
            }

            public static bool andLocal()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                bool r = a == 1 && show("l"u8, a, Set(Ꮡa, 2, false)) == 23;
                return r;
            }

            public static bool orStore()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                bool r = false;
                r = a != 1 || show("s"u8, a, Set(Ꮡa, 2, false)) == 23;
                return r;
            }

            public static bool chainAnd()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                return a == 1 && counter >= 0 && show("c"u8, a, Set(Ꮡa, 2, false)) == 23;
            }

            public static bool chainOrAnd()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                return a != 1 || (counter >= 0 && show("d"u8, a, Set(Ꮡa, 2, false)) == 23);
            }

            // M9's near-misses: a right operand that awaits nothing, and one whose await stays in place, stay plain.
            public static bool andNoAwait()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                return a == 1 && show("n"u8, 1, 2) == 12;
            }

            public static bool andInPlace()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                return a == 1 && Set(Ꮡa, 2, false) == 3;
            }

            // A right operand with a side effect, and one that panics, behind a left operand that short-circuits.
            internal static nint boom() => throw new InvalidOperationException("boom");

            public static nint andSideEffect()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                bool r = a != 1 && show("e"u8, other(), Set(Ꮡa, 2, false)) == 3;
                return counter * 10 + (r ? 1 : 0) + a * 100;
            }

            public static nint orPanic()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                bool r = a == 1 || show("p"u8, boom(), Set(Ꮡa, 2, false)) == 3;
                return (r ? 1 : 0) + a * 100;
            }

            // A store whose right operand reads the target: Go passes the OLD value.
            internal static bool check(ReadOnlySpan<byte> format, bool old, nint b) => !old;

            public static bool targetRead()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                bool ok = false;
                ok = a == 1 && check("k"u8, ok, Set(Ꮡa, 5, false));
                return ok;
            }

            // The statement-list rule's near-miss for M9: a brace-less body is not lowered and stays blocked by name.
            public static bool bracelessAnd()
            {
                ref var a = ref heap<nint>(1, out var Ꮡa);
                if (counter != 99)
                    return counter >= 0 && show("q"u8, 1, Set(Ꮡa, 2, false)) == 13;
                return false;
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

    [TestMethod]
    public void ADeconstructionIntoNewLocalsMoves_M8()
    {
        (string twins, Assembly assembly) = Build();

        Assert.IsFalse(twins.Contains("|deconNew|Set|"), "no ref-blocked row names deconNew\n" + twins);
        Assert.AreEqual((nint)132, Twin(assembly, "deconNew"), "r=1, e=3, a=2");

        StringAssert.Contains(twins, "|deconMixed|Set|ref-like-before|store-left:TupleExpression", "a mixed deconstruction stays blocked by name\n" + twins);
        Assert.AreEqual((nint)132, Twin(assembly, "deconMixed"));
    }

    [TestMethod]
    public void AnAdapterOverAPureReadIsAPureRead_M2()
    {
        (string twins, Assembly assembly) = Build();

        Assert.IsFalse(twins.Contains("|ptrAdapter|SetP|") || twins.Contains("|valAdapter|SetN|"), "no ref-blocked row names the adapter rows\n" + twins);
        Assert.AreEqual((nint)23, Twin(assembly, "ptrAdapter"), "gc converts the pointer after the call: 2*10+3");
        Assert.AreEqual((nint)13, Plain(assembly, "ptrAdapter"), "the plain method converts first, C# order");
        Assert.AreEqual((nint)23, Twin(assembly, "valAdapter"), "gc converts the interface after the call: 2*10+3");
        Assert.AreEqual((nint)13, Plain(assembly, "valAdapter"), "the plain method converts first, C# order");

        StringAssert.Contains(twins, "|adapterOverCall|Set|ref-like-before|earlier:ObjectCreationExpression", "an adapter over a call is ordered\n" + twins);
        Assert.AreEqual((nint)13, Twin(assembly, "adapterOverCall"));
    }

    [TestMethod]
    public void AnEarlierCallSpillsInGoOrder_M1()
    {
        (string twins, Assembly assembly) = Build();

        Assert.IsFalse(twins.Contains("|earlierCall|Set|") || twins.Contains("|twoEarlier|Set|"), "no ref-blocked row names the M1 rows\n" + twins);
        Assert.AreEqual((nint)753, Twin(assembly, "earlierCall"), "gc: bump, Set, then the read of a: 7, 5, 3");
        Assert.AreEqual((nint)723, Plain(assembly, "earlierCall"), "the plain method reads a between the calls, C# order");
        Assert.AreEqual((nint)263, Twin(assembly, "twoEarlier"), "the spills keep text order: 2, 6, 3");
    }

    [TestMethod]
    public void AnEarlierCallUsedByRefOrRefLikeStaysABlocker_M1NearMiss()
    {
        (string twins, Assembly assembly) = Build();

        StringAssert.Contains(twins, "|refArg|Set|ref-like-before|earlier:InvocationExpression", "a by-ref result would be copied\n" + twins);
        StringAssert.Contains(twins, "|spanArg|Set|ref-like-before|earlier:InvocationExpression", "a ref-like result cannot cross the await\n" + twins);
        Assert.AreEqual((nint)1023, Twin(assembly, "refArg"), "Set wrote 2, the ref parameter added 100 to a: 102*10+3");
        Assert.AreEqual((nint)23, Twin(assembly, "spanArg"));
    }

    [TestMethod]
    public void ALogicalOperatorWhoseRightOperandMustAwaitIsLowered_M9()
    {
        (string twins, Assembly assembly) = Build();

        foreach (string name in new[] { "andReturn", "orReturn", "andLocal", "orStore", "chainAnd", "chainOrAnd", "andSideEffect", "orPanic", "targetRead" })
            Assert.IsFalse(twins.Contains("|" + name + "|Set|"), "no ref-blocked row names " + name + "\n" + twins);

        StringAssert.Contains(twins, "bool ᴛtwin", "the operator's temporary takes the operator's own type");

        foreach (string name in new[] { "andReturn", "orReturn", "andLocal", "orStore", "chainAnd", "chainOrAnd" })
        {
            Assert.AreEqual(true, Twin(assembly, name), name + ": gc reads a after the call, 2*10+3 == 23");
            Assert.AreEqual(false, Plain(assembly, name), name + ": the plain method reads a first, C# order");
        }
    }

    [TestMethod]
    public void ALoweredRightOperandRunsOnlyWhenGoRunsIt_M9()
    {
        (string twins, Assembly assembly) = Build();

        Assert.AreEqual((nint)100, Twin(assembly, "andSideEffect"), "the left operand is false: other() never runs, Set never writes");
        Assert.AreEqual((nint)101, Twin(assembly, "orPanic"), "the left operand is true: boom() never runs");
        Assert.AreEqual(true, Twin(assembly, "targetRead"), "the right operand reads the target's old value, false");
    }

    [TestMethod]
    public void ARightOperandThatNeedNotMoveIsNotLowered_M9NearMiss()
    {
        (string twins, Assembly assembly) = Build();

        StringAssert.Contains(twins, "andNoAwaitꞏtwin() => new global::go.golib.GoTask<bool>(andNoAwait());", "nothing awaits: the twin is the plain method\n" + twins);
        StringAssert.Contains(twins, "return a == 1 && (await Setꞏtwin(Ꮡa, 2, false)) == 3;", "the await stays in place\n" + twins);
        StringAssert.Contains(twins, "|bracelessAnd|Set|ref-like-before|", "a brace-less body stays blocked by name\n" + twins);
        Assert.AreEqual(true, Twin(assembly, "andNoAwait"));
        Assert.AreEqual(true, Twin(assembly, "andInPlace"));
        Assert.AreEqual(true, Twin(assembly, "bracelessAnd"));
    }

    private static readonly CSharpParseOptions Options = new(LanguageVersion.CSharp13);

    private static (string twins, Assembly assembly) Build()
    {
        CSharpCompilation compilation = CSharpCompilation.Create("twinmove",
            [CSharpSyntaxTree.ParseText(Source, Options, path: @"C:\twinmove\move.cs")],
            References(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        Dictionary<string, string> properties = new() { ["build_property.GoTwins"] = "true" };

        CSharpGeneratorDriver.Create([new RecvGenerator(), new FieldAccessGenerator(), new TypeGenerator(), new ImplementGenerator(), new TwinGenerator()], parseOptions: Options,
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
