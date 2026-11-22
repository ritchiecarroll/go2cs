using System;
using System.IO;
using System.Linq;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// M:N PROTOTYPE, N3 (docs/phase4/DESIGN-mn-func-values.md, section 4 and fix 3 of 9.3): a pair-bound func value
/// stands for its PLAIN delegate at every golib reader of <c>Delegate.Method</c> or <c>Delegate.Target</c>, through
/// one unwrap rule, <see cref="GoFuncPair.Underlying"/>. A pair is made by hand here, as the generator will make it.
/// </summary>
[TestClass]
public class GoFuncPairTests
{
    private sealed class IntPair(Func<int, int> plain, Func<int, GoTask<int>> twin) : GoFuncPair
    {
        public override Delegate Plain => plain;

        public override Delegate Twin => twin;

        public int Invoke(int a) => plain(a);
    }

    private static int Inc(int a) => a + 1;

    private static GoTask<int> Incꞏtwin(int a) => new(a + 1);

    private static Func<int, int> PairBound(Func<int, int> plain, Func<int, GoTask<int>> twin) => new IntPair(plain, twin).Invoke;

    [TestMethod]
    public void APairOverAStaticFunctionAnswersThatFunctionsWord()
    {
        Func<int, int> plain = Inc;
        Func<int, int> handed = PairBound(Inc, Incꞏtwin);

        Assert.AreEqual(2, handed(1), "the handed-out delegate calls the plain body");
        Assert.AreEqual(GoFuncCookie.Of(plain), GoFuncCookie.Of(handed), "S1's one word per static function holds through a pair");
    }

    [TestMethod]
    public void APairAnswersItsPlainFunctionsProgramCounter()
    {
        Func<int, int> plain = Inc;
        Func<int, int> handed = PairBound(Inc, Incꞏtwin);

        Assert.AreEqual(GoSyntheticPC.Of(plain), GoSyntheticPC.Of(handed), "FuncForPC and %p read the plain function, not Invoke");
    }

    [TestMethod]
    public void APairOverALiteralIsNotAMethodValue()
    {
        int k = 3;
        Func<int, int> literal = a => a + k;
        Func<int, int> handed = PairBound(literal, a => new GoTask<int>(a + k));

        Assert.IsFalse(ManagedPointerTokens.IsMethodValue(literal), "control: a literal is not a method value");
        Assert.IsFalse(ManagedPointerTokens.IsMethodValue(handed), "a pair over a literal must not gain -fm");
    }

    [TestMethod]
    public void UnderlyingSeesThroughARebindOverAPair()
    {
        Func<int, int> handed = PairBound(Inc, Incꞏtwin);
        Func<int, int> rebound = (Func<int, int>)Delegate.CreateDelegate(typeof(Func<int, int>), handed, handed.GetType().GetMethod("Invoke")!);

        Assert.AreEqual(nameof(Inc), GoFuncPair.Underlying(handed).Method.Name);
        Assert.AreEqual(nameof(Inc), GoFuncPair.Underlying(rebound).Method.Name);
    }

    // S5 of the design as a grep gate: golib has exactly ONE `Method.Name == "Invoke"` chase, in GoFuncPair.cs.
    [TestMethod]
    public void GolibHasOneUnwrapRule()
    {
        string? dir = AppContext.BaseDirectory;
        while (dir is not null && !Directory.Exists(Path.Combine(dir, "src", "core", "golib")))
            dir = Path.GetDirectoryName(dir);

        Assert.IsNotNull(dir, "the repository root above the test's output");
        string golib = Path.Combine(dir!, "src", "core", "golib");

        string[] chases = Directory.EnumerateFiles(golib, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") &&
                           !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .SelectMany(path => File.ReadLines(path).Where(line => line.Contains("Method.Name == \"Invoke\"")).Select(_ => Path.GetFileName(path)))
            .ToArray();

        Assert.IsTrue(chases.Length > 0, "control: the census reads golib's sources");
        CollectionAssert.AreEqual(new[] { "GoFuncPair.cs" }, chases, "one unwrap rule: " + string.Join(", ", chases));
    }
}
