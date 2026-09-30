// The bare-walk CONTROL for c2-osr-stackwalk: the same shape (a hot loop with a try/catch, compiled through
// OSR under tiered compilation; depths 5 and 50) with NO golib. Mode `walk` builds a System.Exception after
// a `new StackTrace(1, false)` walk of the live stack, `nowalk` builds it without the walk.
//   DOTNET_TieredCompilation=1 ./bin/Release/net10.0/bare-walk walk
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

static class Bench
{
    const int N = 200_000;

    [MethodImpl(MethodImplOptions.NoInlining)]
    static double AtDepth(int depth, Func<double> body) => depth == 0 ? body() : AtDepth(depth - 1, body) + 0;

    [MethodImpl(MethodImplOptions.NoInlining)]
    static Exception Walk()
    {
        StackTrace trace = new(1, false);
        for (int i = 0; i < trace.FrameCount; i++)
        {
            MethodBase m = trace.GetFrame(i)?.GetMethod();
            if (m?.DeclaringType is { } t && t.FullName == "never") break;
        }
        return new Exception("x");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static Exception NoWalk() => new Exception("x");

    static double Time(Func<Exception> make, bool throwIt)
    {
        long sink = 0;
        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < N; i++)
        {
            if (throwIt) { try { throw make(); } catch (Exception e) { sink += e.GetHashCode() & 1; } }
            else sink += make().GetHashCode() & 1;
        }
        GC.KeepAlive(sink);
        return sw.Elapsed.TotalNanoseconds / N;
    }

    static void Main(string[] args)
    {
        Func<Exception> make = args[0] == "walk" ? Walk : NoWalk;
        foreach (int depth in new[] { 5, 50 })
            Console.WriteLine($"{args[0]} depth {depth}: {AtDepth(depth, () => Time(make, false)):F0} ns, throw {AtDepth(depth, () => Time(make, true)):F0} ns");
    }
}
