// LiteralSiteBench.cs - seat #52: does a hoisted string literal measure faster than a plain one IN SITU?
//
// One Go-shaped kernel per literal SITE CLASS. The forms differ only at the literal site, and the literal's result
// sits on the loop's critical path: its length, first byte or the predicate it feeds decides the next iteration's
// index (G's #52 method: an overlapped loop hides the cost, a dependent one shows it). Hand-written C# against golib
// (and the converted strings package for C1), not a converted program, so PerformanceRunner never picks it up.
//
// The FORM is a compile-time choice, so each form is its own binary:
//   HOISTED  -> the -hoist-literals form, a `static readonly` field per literal (F1, and F5, its A/A copy)
//   (none)   -> the plain form, a bare C# literal through golib's StringLiteralCache (F2; F3 is the same source
//               built against a golib whose cache is the reconstructed D6 table)
// Two kernels exist in one form only: `c1i` (plain: C1 through a hand-written interceptor, F4) and `c4u8` (hoisted:
// C4 as -hoist-literals really emits a composite element, the literal inline as a u8 span).
//
// Usage: LiteralSiteBench <c1|c1i|c2|c3|c4|c4u8|c5|c6|control>
// Output: `checksum:` (equal for one class across every form), `ns_per_eval:` and `bytes_per_eval:`, measured over a
// timed window after a warm-up, with allocations from GC.GetAllocatedBytesForCurrentThread.

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace go;

using strings = strings_package;

internal static partial class LiteralSiteBench
{
    private const int Chunk = 1 << 16;
    private const int WarmupMs = 600;
    private const int WindowMs = 400;
    private const string Usage = "usage: LiteralSiteBench <c1|c1i|c2|c3|c4|c4u8|c5|c6|control>";

#if HOISTED
    // Hoisted @string literals (single allocation; Go keeps these in RODATA)
    private static readonly @string goBuildˢ = "//go:build"u8;
    private static readonly @string boolˢ = "bool"u8;
    private static readonly @string goosˢ = "goos"u8;
    private static readonly @string goarchˢ = "goarch"u8;
    private static readonly @string wordˢ = "word"u8;
    private static readonly @string buildˢ = "build"u8;
    private static readonly @string nameˢ = "name"u8;
    private static readonly @string lineˢ = "line"u8;
    private static readonly object helloˢ = (@string)"hello"u8;
#endif

    // The input: StringMatch's lines and six more, built at run time so nothing folds. Sixteen, so the index wraps
    // with a mask rather than a division on the critical path.
    private static readonly slice<@string> lines = build(
        "//go:build linux && amd64", "// regular comment line", "package main", "import \"fmt\"",
        "func main() {", "\tx := true", "}", "//go:build windows", "var y = false", "// another comment",
        "type T struct{}", "//go:build ignore", "const c = 1", "return nil", "// trailing", "x++");

    private static slice<@string> build(params string[] values)
    {
        var result = new @string[values.Length];

        for (int i = 0; i < values.Length; i++)
            result[i] = new @string(System.Text.Encoding.UTF8.GetBytes(values[i]));

        return slice<@string>(result);
    }

    private struct rec
    {
        public @string Name;
        public nint N;

        public rec(@string Name = default!, nint N = default!)
        {
            this.Name = Name;
            this.N = N;
        }
    }

    private static readonly rec[] recs = new rec[8];

    // C2's callee: kindName from StringMatch, a small function returning a literal, CALLED from the loop.
    private static @string kindName(nint k)
    {
        switch (k)
        {
            case 1 or 2:
#if HOISTED
                return boolˢ;
#else
                return "bool";
#endif
            case 3:
#if HOISTED
                return goosˢ;
#else
                return "goos";
#endif
            case 4:
#if HOISTED
                return goarchˢ;
#else
                return "goarch";
#endif
        }

#if HOISTED
        return wordˢ;
#else
        return "word";
#endif
    }

    // C6's callee: a cheap function taking an any slot.
    internal static nint sink(object o) => o is @string s ? len(s) & 1 : 0;

    // C1: an argument to an exact @string parameter: strings.HasPrefix(line, "//go:build").
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static nint c1(int n)
    {
        nint idx = 0, acc = 0;

        for (int i = 0; i < n; i++)
        {
            @string line = lines[idx];
#if HOISTED
            nint hit = strings.HasPrefix(line, goBuildˢ) ? 2 : 0;
#else
            nint hit = strings.HasPrefix(line, "//go:build") ? 2 : 0;
#endif
            acc += hit;
            idx = (idx + 1 + hit) & 15;
        }

        return acc + idx;
    }

    // C2: a return from a small function called from the loop, with no literal in the loop itself.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static nint c2(int n)
    {
        nint idx = 0, acc = 0;

        for (int i = 0; i < n; i++)
        {
            nint k = len(kindName(idx & 7));
            acc += k;
            idx = (idx + k) & 15;
        }

        return acc + idx;
    }

    // C3: a map index with a literal key: counts["build"]++.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static nint c3(int n)
    {
        var counts = new map<@string, nint>{};
        nint idx = 0;

        for (int i = 0; i < n; i++)
        {
#if HOISTED
            nint c = counts[buildˢ]++;
#else
            nint c = counts["build"]++;
#endif
            idx = (idx + 1 + (c & 1)) & 15;
        }

#if HOISTED
        return counts[buildˢ] + idx;
#else
        return counts["build"] + idx;
#endif
    }

    // C4: a struct-field store from a composite literal, T{Name: "name"}, into a heap element.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static nint c4(int n)
    {
        nint idx = 0, acc = 0;

        for (int i = 0; i < n; i++)
        {
            ref rec r = ref recs[idx & 7];
#if HOISTED
            r = new rec(Name: nameˢ, N: idx);
#else
            r = new rec(Name: "name", N: idx);
#endif
            nint k = len(r.Name) + (nint)(r.Name[0] & 1);
            acc += k;
            idx = (idx + k) & 15;
        }

        return acc + idx;
    }

#if HOISTED
    // C4 as -hoist-literals really emits it: a composite element is Tier B, so the literal stays inline as u8 and
    // copies a fresh backing array at every evaluation.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static nint c4u8(int n)
    {
        nint idx = 0, acc = 0;

        for (int i = 0; i < n; i++)
        {
            ref rec r = ref recs[idx & 7];
            r = new rec(Name: "name"u8, N: idx);
            nint k = len(r.Name) + (nint)(r.Name[0] & 1);
            acc += k;
            idx = (idx + k) & 15;
        }

        return acc + idx;
    }
#endif

    // C5: a local assignment in the loop, s := "line", then a use of s.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static nint c5(int n)
    {
        nint idx = 0, acc = 0;

        for (int i = 0; i < n; i++)
        {
#if HOISTED
            @string s = lineˢ;
#else
            @string s = "line";
#endif
            nint k = len(s) + (nint)(s[(int)(idx & 3)] & 1);
            acc += k;
            idx = (idx + k) & 15;
        }

        return acc + idx;
    }

    // C6: an any-slot argument to a cheap object-taking callee. Hoisted: a pre-boxed field; plain: go2cs-gen's
    // generated interceptor writes its own per-file pre-boxed static into the slot.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static nint c6(int n)
    {
        nint idx = 0, acc = 0;

        for (int i = 0; i < n; i++)
        {
#if HOISTED
            nint k = sink(helloˢ);
#else
            nint k = sink("hello");
#endif
            acc += k;
            idx = (idx + 1 + k) & 15;
        }

        return acc + idx;
    }

    // CONTROL: a comparison against a literal, emitted identically in both forms (the u8 span operators).
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static nint control(int n)
    {
        nint idx = 0, acc = 0;

        for (int i = 0; i < n; i++)
        {
            nint k = lines[idx] == "package main"u8 ? 2 : 0;
            acc += k;
            idx = (idx + 1 + k) & 15;
        }

        return acc + idx;
    }

    private static Func<int, nint>? Pick(string kernel) => kernel switch
    {
        "c1" => c1,
#if !HOISTED
        "c1i" => c1i,
#endif
        "c2" => c2,
        "c3" => c3,
        "c4" => c4,
#if HOISTED
        "c4u8" => c4u8,
#endif
        "c5" => c5,
        "c6" => c6,
        "control" => control,
        _ => null
    };

    internal static int Main(string[] args)
    {
        Func<int, nint>? f = args.Length == 1 ? Pick(args[0]) : null;

        if (f is null)
        {
            Console.Error.WriteLine(Usage);
            return 2;
        }

        nint checksum = f(1000);

        Stopwatch warm = Stopwatch.StartNew();

        while (warm.ElapsedMilliseconds < WarmupMs)
            f(Chunk);

        long evals = 0;
        long bytes0 = GC.GetAllocatedBytesForCurrentThread();
        long t0 = Stopwatch.GetTimestamp();
        long t1;

        do
        {
            f(Chunk);
            evals += Chunk;
            t1 = Stopwatch.GetTimestamp();
        }
        while ((t1 - t0) * 1000 < WindowMs * Stopwatch.Frequency);

        long bytes1 = GC.GetAllocatedBytesForCurrentThread();
        double ns = (t1 - t0) * 1e9 / Stopwatch.Frequency / evals;
        double perEval = (double)(bytes1 - bytes0) / evals;

        Console.WriteLine($"checksum: {checksum}");
        Console.WriteLine($"evals: {evals}");
        Console.WriteLine($"ns_per_eval: {ns:F4}");
        Console.WriteLine($"bytes_per_eval: {perEval:F3}");
        return 0;
    }
}
