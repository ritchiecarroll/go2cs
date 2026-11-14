// sortbench: what N3's caller-local rule costs on a hot callback path (the re-review's must-fix 2).
// Under M:N, main runs as a twin; under the caller-local rule a sort that calls its less/cmp through a func value
// gets a twin, so main's twin calls the SORT'S TWIN: every recursion is a twin-to-twin call (golib's GoTask builder),
// and every less/cmp call first tests the delegate's target for a pair (the non-generic test, P3), then calls plain.
// The value never parks (a comparison), so it is never a pair: this is the pure overhead the rule adds.
//   P   sort.Slice's shape, plain: quicksort + insertion sort over an index less(i, j), as go2cs emits the plain body
//   T   the same as twins: async GoTask bodies, awaited recursion, the pair test before every less call
//   PG  slices.SortFunc's shape, plain: generic over T with cmp(a, b) int
//   TG  the same as generic twins
//   Tt  the pair test alone (non-generic base): d.Target is GoFuncPair
// ns per element sorted is the floor of 7 timed passes in this process (each pass sorts a fresh copy).
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using go.golib;

// The design's non-generic golib base (section 4), modelled here: the test a twin makes before a dynamic call.
abstract class GoFuncPair
{
    public abstract Delegate Twin { get; }
}

static class Plain
{
    public static void Slice(int[] x, Func<int, int, bool> less) => Quick(x, 0, x.Length, less);

    static void Quick(int[] x, int a, int b, Func<int, int, bool> less)
    {
        while (b - a > 12)
        {
            int p = Partition(x, a, b, less);
            if (p - a < b - p)
            {
                Quick(x, a, p, less);
                a = p + 1;
            }
            else
            {
                Quick(x, p + 1, b, less);
                b = p;
            }
        }

        Insertion(x, a, b, less);
    }

    static int Partition(int[] x, int a, int b, Func<int, int, bool> less)
    {
        int m = a + (b - a) / 2;
        (x[m], x[b - 1]) = (x[b - 1], x[m]);
        int store = a;
        for (int i = a; i < b - 1; i++)
        {
            if (less(i, b - 1))
            {
                (x[i], x[store]) = (x[store], x[i]);
                store++;
            }
        }

        (x[store], x[b - 1]) = (x[b - 1], x[store]);
        return store;
    }

    static void Insertion(int[] x, int a, int b, Func<int, int, bool> less)
    {
        for (int i = a + 1; i < b; i++)
            for (int j = i; j > a && less(j, j - 1); j--)
                (x[j], x[j - 1]) = (x[j - 1], x[j]);
    }

    public static void SortFunc<T>(T[] s, Func<T, T, int> cmp) => QuickG(s, 0, s.Length, cmp);

    static void QuickG<T>(T[] s, int a, int b, Func<T, T, int> cmp)
    {
        while (b - a > 12)
        {
            int m = a + (b - a) / 2;
            (s[m], s[b - 1]) = (s[b - 1], s[m]);
            int store = a;
            for (int i = a; i < b - 1; i++)
            {
                if (cmp(s[i], s[b - 1]) < 0)
                {
                    (s[i], s[store]) = (s[store], s[i]);
                    store++;
                }
            }

            (s[store], s[b - 1]) = (s[b - 1], s[store]);
            if (store - a < b - store)
            {
                QuickG(s, a, store, cmp);
                a = store + 1;
            }
            else
            {
                QuickG(s, store + 1, b, cmp);
                b = store;
            }
        }

        for (int i = a + 1; i < b; i++)
            for (int j = i; j > a && cmp(s[j], s[j - 1]) < 0; j--)
                (s[j], s[j - 1]) = (s[j - 1], s[j]);
    }
}

static class Twin
{
    // The twin's dynamic call: one non-generic type test, then the plain call (a comparison is never a pair).
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool Less(Func<int, int, bool> less, int i, int j) => less.Target is GoFuncPair ? Slow(less, i, j) : less(i, j);

    [MethodImpl(MethodImplOptions.NoInlining)]
    static bool Slow(Func<int, int, bool> less, int i, int j) => less(i, j);

    public static async GoTask Sliceꞏtwin(int[] x, Func<int, int, bool> less) => await Quickꞏtwin(x, 0, x.Length, less);

    static async GoTask Quickꞏtwin(int[] x, int a, int b, Func<int, int, bool> less)
    {
        while (b - a > 12)
        {
            int p = await Partitionꞏtwin(x, a, b, less);
            if (p - a < b - p)
            {
                await Quickꞏtwin(x, a, p, less);
                a = p + 1;
            }
            else
            {
                await Quickꞏtwin(x, p + 1, b, less);
                b = p;
            }
        }

        await Insertionꞏtwin(x, a, b, less);
    }

    static async GoTask<int> Partitionꞏtwin(int[] x, int a, int b, Func<int, int, bool> less)
    {
        int m = a + (b - a) / 2;
        (x[m], x[b - 1]) = (x[b - 1], x[m]);
        int store = a;
        for (int i = a; i < b - 1; i++)
        {
            if (Less(less, i, b - 1))
            {
                (x[i], x[store]) = (x[store], x[i]);
                store++;
            }
        }

        (x[store], x[b - 1]) = (x[b - 1], x[store]);
        await default(GoTask);
        return store;
    }

    static async GoTask Insertionꞏtwin(int[] x, int a, int b, Func<int, int, bool> less)
    {
        for (int i = a + 1; i < b; i++)
            for (int j = i; j > a && Less(less, j, j - 1); j--)
                (x[j], x[j - 1]) = (x[j - 1], x[j]);

        await default(GoTask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int Cmp<T>(Func<T, T, int> cmp, T a, T b) => cmp.Target is GoFuncPair ? SlowG(cmp, a, b) : cmp(a, b);

    [MethodImpl(MethodImplOptions.NoInlining)]
    static int SlowG<T>(Func<T, T, int> cmp, T a, T b) => cmp(a, b);

    public static async GoTask SortFuncꞏtwin<T>(T[] s, Func<T, T, int> cmp) => await QuickGꞏtwin(s, 0, s.Length, cmp);

    static async GoTask QuickGꞏtwin<T>(T[] s, int a, int b, Func<T, T, int> cmp)
    {
        while (b - a > 12)
        {
            int m = a + (b - a) / 2;
            (s[m], s[b - 1]) = (s[b - 1], s[m]);
            int store = a;
            for (int i = a; i < b - 1; i++)
            {
                if (Cmp(cmp, s[i], s[b - 1]) < 0)
                {
                    (s[i], s[store]) = (s[store], s[i]);
                    store++;
                }
            }

            (s[store], s[b - 1]) = (s[b - 1], s[store]);
            if (store - a < b - store)
            {
                await QuickGꞏtwin(s, a, store, cmp);
                a = store + 1;
            }
            else
            {
                await QuickGꞏtwin(s, store + 1, b, cmp);
                b = store;
            }
        }

        for (int i = a + 1; i < b; i++)
            for (int j = i; j > a && Cmp(cmp, s[j], s[j - 1]) < 0; j--)
                (s[j], s[j - 1]) = (s[j - 1], s[j]);
    }
}

// The decomposition arm: the same twins with NO pair test (the dynamic call is plain). This is also the HOISTED
// shape: a test made once per func-value variable that is never reassigned, at the twin's entry.
static class TwinNoTest
{
    public static async GoTask Sliceꞏtwin(int[] x, Func<int, int, bool> less) => await Quickꞏtwin(x, 0, x.Length, less);

    static async GoTask Quickꞏtwin(int[] x, int a, int b, Func<int, int, bool> less)
    {
        while (b - a > 12)
        {
            int p = await Partitionꞏtwin(x, a, b, less);
            if (p - a < b - p)
            {
                await Quickꞏtwin(x, a, p, less);
                a = p + 1;
            }
            else
            {
                await Quickꞏtwin(x, p + 1, b, less);
                b = p;
            }
        }

        await Insertionꞏtwin(x, a, b, less);
    }

    static async GoTask<int> Partitionꞏtwin(int[] x, int a, int b, Func<int, int, bool> less)
    {
        int m = a + (b - a) / 2;
        (x[m], x[b - 1]) = (x[b - 1], x[m]);
        int store = a;
        for (int i = a; i < b - 1; i++)
        {
            if (less(i, b - 1))
            {
                (x[i], x[store]) = (x[store], x[i]);
                store++;
            }
        }

        (x[store], x[b - 1]) = (x[b - 1], x[store]);
        await default(GoTask);
        return store;
    }

    static async GoTask Insertionꞏtwin(int[] x, int a, int b, Func<int, int, bool> less)
    {
        for (int i = a + 1; i < b; i++)
            for (int j = i; j > a && less(j, j - 1); j--)
                (x[j], x[j - 1]) = (x[j - 1], x[j]);

        await default(GoTask);
    }

    public static async GoTask SortFuncꞏtwin<T>(T[] s, Func<T, T, int> cmp) => await QuickGꞏtwin(s, 0, s.Length, cmp);

    static async GoTask QuickGꞏtwin<T>(T[] s, int a, int b, Func<T, T, int> cmp)
    {
        while (b - a > 12)
        {
            int m = a + (b - a) / 2;
            (s[m], s[b - 1]) = (s[b - 1], s[m]);
            int store = a;
            for (int i = a; i < b - 1; i++)
            {
                if (cmp(s[i], s[b - 1]) < 0)
                {
                    (s[i], s[store]) = (s[store], s[i]);
                    store++;
                }
            }

            (s[store], s[b - 1]) = (s[b - 1], s[store]);
            if (store - a < b - store)
            {
                await QuickGꞏtwin(s, a, store, cmp);
                a = store + 1;
            }
            else
            {
                await QuickGꞏtwin(s, store + 1, b, cmp);
                b = store;
            }
        }

        for (int i = a + 1; i < b; i++)
            for (int j = i; j > a && cmp(s[j], s[j - 1]) < 0; j--)
                (s[j], s[j - 1]) = (s[j - 1], s[j]);
    }
}

static class Program
{
    const int Len = 100_000;
    static int sink;
    static object? osink;

    static double Measure(int[] source, Action<int[]> sort)
    {
        var work = new int[source.Length];
        Array.Copy(source, work, source.Length);
        sort(work);
        double best = double.MaxValue;

        for (int pass = 0; pass < 7; pass++)
        {
            Array.Copy(source, work, source.Length);
            var sw = Stopwatch.StartNew();
            sort(work);
            sw.Stop();
            best = Math.Min(best, sw.Elapsed.TotalNanoseconds / source.Length);
            for (int i = 1; i < work.Length; i++)
                if (work[i - 1] > work[i])
                    throw new InvalidOperationException("not sorted");
        }

        return best;
    }

    static void Main()
    {
        var rng = new Random(20261113);
        int[] source = new int[Len];
        for (int i = 0; i < Len; i++)
            source[i] = rng.Next();

        int[] current = [];
        Func<int, int, bool> less = (i, j) => current[i] < current[j];
        Func<int, int, int> cmp = (a, b) => a.CompareTo(b);

        Console.WriteLine($"runtime aot={!RuntimeFeature.IsDynamicCodeSupported} N={Len}");

        double p = Measure(source, x => { current = x; Plain.Slice(x, less); });
        double t = Measure(source, x => { current = x; Twin.Sliceꞏtwin(x, less).GetAwaiter().GetResult(); });
        double pg = Measure(source, x => Plain.SortFunc(x, cmp));
        double tg = Measure(source, x => Twin.SortFuncꞏtwin(x, cmp).GetAwaiter().GetResult());

        Console.WriteLine($"P   sort.Slice shape, plain        {p,8:F2} ns/elem");
        Console.WriteLine($"T   sort.Slice shape, twins        {t,8:F2} ns/elem  x{t / p:F2}");
        Console.WriteLine($"PG  slices.SortFunc shape, plain   {pg,8:F2} ns/elem");
        Console.WriteLine($"TG  slices.SortFunc shape, twins   {tg,8:F2} ns/elem  x{tg / pg:F2}");

        double tn = Measure(source, x => { current = x; TwinNoTest.Sliceꞏtwin(x, less).GetAwaiter().GetResult(); });
        double tng = Measure(source, x => TwinNoTest.SortFuncꞏtwin(x, cmp).GetAwaiter().GetResult());
        Console.WriteLine($"TN  sort.Slice, twins, no test     {tn,8:F2} ns/elem  x{tn / p:F2}");
        Console.WriteLine($"TNG SortFunc, twins, no test       {tng,8:F2} ns/elem  x{tng / pg:F2}");

        // The pair test alone, per call: Target read + non-generic type test.
        const int N = 20_000_000;
        Func<int, int, bool> d = less;
        double tt = double.MaxValue;
        for (int pass = 0; pass < 7; pass++)
        {
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < N; i++)
                sink += d.Target is GoFuncPair ? 1 : 0;
            sw.Stop();
            tt = Math.Min(tt, sw.Elapsed.TotalNanoseconds / N);
        }

        Console.WriteLine($"Tt  pair test (non-generic)        {tt,8:F2} ns/call");
        GC.KeepAlive(osink);
        Console.WriteLine(sink == 42 ? "" : "done");
    }
}
