//******************************************************************************************************
//  FuncValueFieldTests.cs - Gbtc
//
//  The func-value seat (COORD's approval of 2026-11-21, docs/phase4/inbox/G/20261121T233319Z-COORD.md): P5's S2 arm 3
//  found a table-driven main ([]struct{name string; run func()}) never twinned, because N3's pair test reached only a
//  call through a bare identifier. A field or element callee is now tested too, read ONCE into a temp, and a shape the
//  test still cannot reach reports GO2CS0020.
//******************************************************************************************************

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static GenTests.TwinBoxTests;

namespace GenTests;

[TestClass]
public class FuncValueFieldTests
{
    private const string Shapes = """
            public partial struct entry { public nint id; public Action run; }

            internal static nint next;

            internal static nint nextIndex() { next++; return 0; }

            public static void parks() { ᐸꟷ(ch); }

            public static void runField(ж<entry> Ꮡe)
            {
                ref var e = ref Ꮡe.DerefOrNull();
                e.run();
            }

            public static void runElem(slice<Action> fs, nint i)
            {
                fs[i]();
            }

            public static void runIndexOnce(slice<Action> fs)
            {
                fs[nextIndex()]();
            }

            public static void runMap(map<nint, Action> m)
            {
                m[1]();
            }

            public static void runTable()
            {
                var rows = slice<entry>([new entry { id = 1, run = parks }]);
                rows[0].run();
                var steps = slice<Action>([parks, parks]);
                steps[1]();
            }

            public partial struct job { public Action run; }

            public static void runJob()
            {
                var j = new job(run: parks);
                j.run();
            }

            internal static nint arg() => 20;

            public static nint parksInt(nint v) { ᐸꟷ(ch); return v * 2; }

            public static nint runValue(slice<Func<nint, nint>> fs)
            {
                nint x = fs[nextIndex()](arg());
                return x;
            }

            internal static Action pick() => parks;

            public static void runResult()
            {
                pick()();
                pick()();
            }

        """;

    [TestMethod]
    public void AFieldCalleeIsTestedForAPairReadOnce()
    {
        (string twins, Diagnostic[] errors) = RunWith(null, null, Shapes);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "runFieldꞏtwin", "a function that calls a func value in a field is twinned (the caller rule)");
        StringAssert.Contains(twins, "var ᴛfv", "a field or element callee is read once into a temp");
        StringAssert.Contains(twins, "?.Target is global::go.golib.GoFuncPair", "and tested for a pair");
    }

    [TestMethod]
    public void AnElementCalleeIsTestedAndItsIndexRunsOnce()
    {
        (string twins, Diagnostic[] errors) = RunWith(null, null, Shapes);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "runElemꞏtwin", "an element callee twins its caller");
        StringAssert.Contains(twins, "runMapꞏtwin", "a map element callee twins its caller");

        // The index expression is evaluated exactly once: it appears in the read, never in the test or the call.
        string body = twins[twins.IndexOf("runIndexOnceꞏtwin", StringComparison.Ordinal)..];
        body = body[..Math.Min(body.Length, body.IndexOf("\n    }", StringComparison.Ordinal) is var end and > 0 ? end : body.Length)];
        Assert.AreEqual(1, body.Split("nextIndex()").Length - 1, "nextIndex() is called once: " + body);
    }

    [TestMethod]
    public void AnImplicitGroupStoredInAFieldOrElementBecomesAPair()
    {
        // P5's table-driven main: the converter stores a method group with no `new D(F)` (a struct literal's field, a
        // slice literal's element). In the twin it becomes the same cached pair as fix 5's `new D(F)`, so the stored
        // value carries the twin the call test looks for.
        (string twins, Diagnostic[] errors) = RunWith(null, null, Shapes);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        int start = twins.IndexOf("runTableꞏtwin", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, "runTable is twinned");
        string body = twins[start..];
        body = body[..Math.Min(body.Length, body.IndexOf("\n    }", StringComparison.Ordinal) is var end and > 0 ? end : body.Length)];
        Assert.AreEqual(3, body.Split("??= new global::System.Action(new ").Length - 1, "the field's and both elements' groups are pairs: " + body);
        Assert.IsFalse(body.Contains("run = parks", StringComparison.Ordinal), "no plain group is stored: " + body);
    }

    [TestMethod]
    public void AOneFieldStructLiteralsGroupIsPaired()
    {
        // Go `job{run: work}` converts to `new job(work)`: a one-argument creation that is a STRUCT, not fix 5's `new D(F)`,
        // so its argument is an implicit group and becomes a pair (P3's SHOULD 1 on the seat: it was skipped by count).
        (string twins, Diagnostic[] errors) = RunWith(null, null, Shapes);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        string body = Body(twins, "runJobꞏtwin");
        StringAssert.Contains(body, "new job(run: (ꞏpair", "the struct's argument is replaced: " + body);
        StringAssert.Contains(body, "??= new global::System.Action(new ", "by the cached pair: " + body);
    }

    [TestMethod]
    public void AValueCallReadsItsCalleeBeforeItsArguments()
    {
        // The value path (a call whose result is used) spills the read ahead of the expression: the callee's index is
        // evaluated once and BEFORE the arguments, as Go evaluates the function value first. MnTableMain's value row runs
        // it and prints the order Go prints.
        (string twins, Diagnostic[] errors) = RunWith(null, null, Shapes);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        string body = Body(twins, "runValueꞏtwin");
        Assert.AreEqual(1, body.Split("nextIndex()").Length - 1, "nextIndex() is written once: " + body);
        int read = body.IndexOf("var ᴛfv", StringComparison.Ordinal), index = body.IndexOf("nextIndex()", StringComparison.Ordinal), argument = body.IndexOf("arg()", StringComparison.Ordinal);
        Assert.IsTrue(read >= 0 && read < index && index < argument, "the read, holding the index, precedes the argument: " + body);
        StringAssert.Contains(body, "global::System.Func<nint, global::go.golib.GoTask<nint>>", "the twin is awaited for its result: " + body);
    }

    private static string Body(string twins, string name)
    {
        int start = twins.IndexOf(name, StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, name + " is emitted");
        string body = twins[start..];
        return body[..Math.Min(body.Length, body.IndexOf("\n    }", StringComparison.Ordinal) is var end and > 0 ? end : body.Length)];
    }

    [TestMethod]
    public void ACallResultCalleeReportsGo2cs0020()
    {
        (_, Diagnostic[] errors) = RunWith(null, null, Shapes);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Diagnostic[] uncovered = [.. (LastGeneratorDiagnostics ?? []).Where(diagnostic => diagnostic.Id == "GO2CS0020")];
        Assert.AreEqual(1, uncovered.Length, "one GO2CS0020, for runResult only: " + string.Join("\n", uncovered.Select(d => d.ToString())));
        StringAssert.Contains(uncovered[0].GetMessage(), "runResult", "it names the function");
        StringAssert.Contains(uncovered[0].GetMessage(), "a call's result", "and the shape");
        StringAssert.Contains(uncovered[0].GetMessage(), "2 such call(s)", "ONCE per function: both of runResult's calls in one report");
        Assert.AreEqual(DiagnosticSeverity.Warning, uncovered[0].Severity, "a warning by default, as GO2CS0014/0015");
    }
}
