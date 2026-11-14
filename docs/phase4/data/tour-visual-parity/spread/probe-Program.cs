// The multi-value spread scout's probes. Each section prints what it shows; Main returns the FAIL count.
using System;
using System.Runtime.CompilerServices;
using fmtA = fmtA_package;           // option A: runtime spread inside the variadic
#pragma warning disable CS8981 // fmt and lib are go2cs's lower-case package aliases
using fmt = fmt_package;             // options B/C: per-arity generic overloads beside the params form
using lib = lib_package;             // option D: a fixed-arity callee in ANOTHER package, reached by a static extension

// ---- the callee packages, shaped as go2cs emits them (a static class; `...any` is `params Span<object>`) ----
public static class fmtA_package
{
    public static string Println(params Span<object> a)
    {
        // A: one argument that is an ITuple is a multi-value spread (Go has no tuple values).
        if (a.Length == 1 && a[0] is ITuple t)
        {
            var spread = new object[t.Length];
            for (int i = 0; i < t.Length; i++) spread[i] = t[i]!;
            return Join(spread);
        }
        return Join(a.ToArray());
    }
    internal static string Join(object[] a) => string.Join(" ", a);
}

public static partial class fmt_package
{
    public static string Println(params Span<object> a) => string.Join(" ", a.ToArray());
}

// B/C: what a generator (B) or a hand-owned fmt *_impl.cs (C) adds, one overload per arity.
public static partial class fmt_package
{
    public static string Println<T1, T2>((T1, T2) t) => Println(t.Item1!, t.Item2!);
    public static string Println<T1, T2, T3>((T1, T2, T3) t) => Println(t.Item1!, t.Item2!, t.Item3!);
    public static string Println<T1, T2, T3, T4>((T1, T2, T3, T4) t) => Println(t.Item1!, t.Item2!, t.Item3!, t.Item4!);
}

public static class lib_package
{
    public static string Must(nint v, Exception? err) => err is null ? $"ok {v}" : "panic";
}

public interface IComposer { string Compose(nint a, nint b, nint c, nint d); }
public sealed class Composer : IComposer { public string Compose(nint a, nint b, nint c, nint d) => $"{a}{b}{c}{d}"; }

// D: C# 14 extension members in the CALLER's assembly: a STATIC member on another package's static class, and an
// INSTANCE member on an interface.
public static class CallerSpreads
{
    extension(lib_package)
    {
        public static string Must((nint, Exception?) t) => lib_package.Must(t.Item1, t.Item2);
    }

    extension(IComposer d)
    {
        public string Compose((nint, nint, nint, nint) t) => d.Compose(t.Item1, t.Item2, t.Item3, t.Item4);
    }
}

public delegate string PrintFn(params Span<object> a);

public static class Program
{
    static (nint x, nint y) split(nint sum) { var x = sum * 4 / 9; return (x, sum - x); }
    static (nint, nint, nint, nint) four() => (1, 2, 3, 4);
    static (nint, Exception?) parse() => (42, null);

    static int fail;
    static void Check(string what, bool ok, string detail) { Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {what}: {detail}"); if (!ok) fail++; }

    public static int Main()
    {
        // A
        Check("A: fmtA.Println(split(17)) compiles (the tuple boxes into the one params slot) and spreads at run time", fmtA.Println(split(17)) == "7 10", fmtA.Println(split(17)));
        Check("A: two ordinary arguments unchanged", fmtA.Println(1, "x") == "1 x", fmtA.Println(1, "x"));
        object deliberate = (1, 2);   // C# code (never converted Go) that means ONE value
        Check("A: HAZARD reproduced -- a deliberate single ValueTuple from C# is spread too", fmtA.Println(deliberate) == "1 2", fmtA.Println(deliberate));

        // B/C
        Check("B/C: fmt.Println(split(17)) binds the generic arity-2 overload", fmt.Println(split(17)) == "7 10", fmt.Println(split(17)));
        Check("B/C: arity 4 (the Tour's methods/images.go)", fmt.Println(four()) == "1 2 3 4", fmt.Println(four()));
        Check("B/C: one ordinary argument still binds params", fmt.Println(5) == "5", fmt.Println(5));
        Check("B/C: no arguments still binds params", fmt.Println() == "", "\"" + fmt.Println() + "\"");
        PrintFn f = fmt.Println;      // a method group to the params delegate: the generic overloads do not interfere
        Check("B/C: method group conversion to the variadic delegate", f(1, 2) == "1 2", f(1, 2));
        Check("B/C: a deliberate C# tuple stays ONE value (no runtime guess)", fmt.Println(deliberate) == "(1, 2)", fmt.Println(deliberate));

        // D
        Check("D: lib.Must(parse()) binds a C# 14 static extension declared in the caller, through the using alias", lib.Must(parse()) == "ok 42", lib.Must(parse()));
        IComposer c = new Composer();
        Check("D: an interface method spread binds an instance extension", c.Compose(four()) == "1234", c.Compose(four()));
        Check("D: the ordinary call is untouched", lib.Must(7, null) == "ok 7", lib.Must(7, null));

        // Evaluation order, against Go's (gc go1.26.10): 1. pick()(g()) -> pick, g;  2. p.scaled(g2()) -> 9234;  3. h(g3()) -> 12
        var log = new System.Collections.Generic.List<string>();
        Func<Func<nint, nint, nint>> pick = () => { log.Add("pick"); return (a, b) => a + b; };
        Func<(nint, nint)> g = () => { log.Add("g"); return (1, 2); };
        var (t1, t2) = g(); var r1 = pick()(t1, t2);                        // TODAY'S temp form
        Check("order 1, REPRODUCES today's defect: the temp form calls g() before pick() (Go: pick, then g)", string.Join(",", log) == "g,pick", string.Join(",", log));
        log.Clear();
        var pv = pick(); var r1b = pv(g().Item1, 0);                        // what an inline form would keep: callee first
        Check("order 1, callee-first form", string.Join(",", log) == "pick,g", string.Join(",", log));
        var p = new Pair { a = 1, b = 2 };
        Func<(nint, nint)> g2 = () => { p.a = 9; return (3, 4); };
        var (u1, u2) = g2(); var r2t = p.scaled(u1, u2);                     // temp form: p read after g2()
        p = new Pair { a = 1, b = 2 };
        var r2i = p.scaled(g2());                                            // inline form: the receiver copy is taken first
        Check("order 2, temp form matches gc (9234)", r2t == 9234, $"{r2t}");
        Check("order 2, REPRODUCES the inline hazard: the receiver copy is read before g2() (gc: 9234)", r2i == 1234, $"{r2i}");
        return fail;
    }
}

public struct Pair
{
    public nint a, b;
    public nint scaled(nint x, nint y) => a * 1000 + b * 100 + x * 10 + y;
}
public static class PairSpread
{
    // the overload a value-receiver method spread would need
    public static nint scaled(this Pair p, (nint, nint) t) => p.scaled(t.Item1, t.Item2);
}
