// n3bench: what a func value costs under the two ways from a func value to its twin (stage (b), N3).
//   A0    a closure literal created and called, today's func value
//   D3c   the same literal created as a PAIR: plain + twin lambdas, a pair object, the delegate handed out bound to it
//   D3call  a call through the handed-out (pair-bound) delegate
//   D3s   a static function's pair, cached once: the call only
//   D3t   the twin-side test: is the delegate's Target a pair
//   D1hit a ConditionalWeakTable lookup by delegate instance (a known instance)
//   D1reg a fresh delegate's first lookup: miss, Delegate.Method read, method-keyed table, registration
//   M     Delegate.Method read on a fresh delegate
//   B0p/D3bp  a bound method value on a pointer receiver, today and as a pair (receiver read once, both halves bind it)
//   B0v/D3bv  the same on a value receiver: one copy in one closure class, shared by both halves of the pair
// ns per op is the floor of 7 timed passes in this process; bytes per op from the thread's allocation counter.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

sealed class FuncPair<T, R>(Func<T, R> plain, Func<T, Task<R>> twin)
{
    public readonly Func<T, R> Plain = plain;
    public readonly Func<T, Task<R>> Twin = twin;

    public R Invoke(T arg) => Plain(arg);
}

static class Statics
{
    public static int Inc(int a) => a + 1;

    public static Task<int> IncTwin(int a) => Task.FromResult(a + 1);

    public static readonly FuncPair<int, int> IncPair = new(Inc, IncTwin);
    public static readonly Func<int, int> IncValue = IncPair.Invoke;
}

// A receiver for the bound-method arms: Recv is a pointer receiver's target, RecvV a value receiver.
sealed class Recv(int k)
{
    public int K = k;

    public int M(int a) => a + K;

    public Task<int> MTwin(int a) => Task.FromResult(a + K);
}

struct RecvV(int k)
{
    public int K = k;

    public readonly int M(int a) => a + K;

    public readonly Task<int> MTwin(int a) => Task.FromResult(a + K);
}

static class Program
{
    const int N = 2_000_000;
    static int sink;
    static object? osink;
    static readonly ConditionalWeakTable<Delegate, object> table = new();
    static readonly Dictionary<RuntimeMethodHandle, object> byMethod = new();

    [MethodImpl(MethodImplOptions.NoInlining)]
    static Func<int, int> MakeA0(int captured) => a => a + captured;

    [MethodImpl(MethodImplOptions.NoInlining)]
    static Func<int, int> MakeD3(int captured)
    {
        Func<int, int> plain = a => a + captured;
        Func<int, Task<int>> twin = a => Task.FromResult(a + captured);
        return new FuncPair<int, int>(plain, twin).Invoke;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static Func<int, int> MakeB0p(Recv r) => r.M;

    [MethodImpl(MethodImplOptions.NoInlining)]
    static Func<int, int> MakeD3bp(Recv r)
    {
        var t = r;
        return new FuncPair<int, int>(t.M, t.MTwin).Invoke;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static Func<int, int> MakeB0v(RecvV r)
    {
        var copy = r;
        return a => copy.M(a);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static Func<int, int> MakeD3bv(RecvV r)
    {
        var copy = r;
        Func<int, int> plain = a => copy.M(a);
        Func<int, Task<int>> twin = a => copy.MTwin(a);
        return new FuncPair<int, int>(plain, twin).Invoke;
    }

    static (double ns, double bytes) Measure(Action<int> body)
    {
        body(1000);
        double best = double.MaxValue;
        long bytes = 0;

        for (int pass = 0; pass < 7; pass++)
        {
            long before = GC.GetAllocatedBytesForCurrentThread();
            var sw = Stopwatch.StartNew();
            body(N);
            sw.Stop();
            long after = GC.GetAllocatedBytesForCurrentThread();
            best = Math.Min(best, sw.Elapsed.TotalNanoseconds / N);
            bytes = after - before;
        }

        return (best, (double)bytes / N);
    }

    static void Main()
    {
        Func<int, int> known = MakeA0(3);
        table.Add(known, new object());
        Func<int, int> pairBound = MakeD3(3);
        Func<int, int> plainClosure = MakeA0(3);

        var arms = new (string name, Action<int> body)[]
        {
            ("A0     create+call closure", n => { for (int i = 0; i < n; i++) sink += MakeA0(i)(i); }),
            ("D3c    create pair+call", n => { for (int i = 0; i < n; i++) sink += MakeD3(i)(i); }),
            ("B0p    create+call bound ptr method", n => { var r = new Recv(3); for (int i = 0; i < n; i++) sink += MakeB0p(r)(i); }),
            ("D3bp   create pair+call, ptr method", n => { var r = new Recv(3); for (int i = 0; i < n; i++) sink += MakeD3bp(r)(i); }),
            ("B0v    create+call bound value method", n => { var r = new RecvV(3); for (int i = 0; i < n; i++) sink += MakeB0v(r)(i); }),
            ("D3bv   create pair+call, value method", n => { var r = new RecvV(3); for (int i = 0; i < n; i++) sink += MakeD3bv(r)(i); }),
            ("A0call call a closure", n => { var f = plainClosure; for (int i = 0; i < n; i++) sink += f(i); }),
            ("D3call call via the pair", n => { var f = pairBound; for (int i = 0; i < n; i++) sink += f(i); }),
            ("D3s    call a static's pair", n => { var f = Statics.IncValue; for (int i = 0; i < n; i++) sink += f(i); }),
            ("D3t    twin-side Target test", n => { var f = pairBound; for (int i = 0; i < n; i++) sink += f.Target is FuncPair<int, int> ? 1 : 0; }),
            ("D1hit  CWT lookup, known", n => { for (int i = 0; i < n; i++) { table.TryGetValue(known, out var v); osink = v; } }),
            ("M      Delegate.Method, fresh", n => { for (int i = 0; i < n; i++) osink = MakeA0(i).Method; }),
            ("D1reg  create+miss+Method+register", n =>
            {
                for (int i = 0; i < n; i++)
                {
                    var d = MakeA0(i);
                    if (!table.TryGetValue(d, out var twin))
                    {
                        var h = d.Method.MethodHandle;
                        if (!byMethod.TryGetValue(h, out twin)) { twin = new object(); byMethod[h] = twin; }
                        table.AddOrUpdate(d, twin);
                    }
                    osink = twin;
                }
            }),
        };

        Console.WriteLine($"runtime aot={!RuntimeFeature.IsDynamicCodeSupported} N={N}");

        foreach (var (name, body) in arms)
        {
            var (ns, bytes) = Measure(body);
            Console.WriteLine($"{name,-36} {ns,9:F2} ns {bytes,8:F1} B");
        }

        Console.WriteLine($"names: A0 method {plainClosure.Method.Name}; D3 handed-out method {pairBound.Method.Name}, target {pairBound.Target?.GetType().Name}");
        GC.KeepAlive(osink);
        Console.WriteLine(sink == 42 ? "" : "done");
    }
}
