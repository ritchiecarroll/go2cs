using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using go;
using go.golib;

static class Bench
{
    const int N = 200_000;

    [MethodImpl(MethodImplOptions.NoInlining)]
    static double AtDepth(int depth, Func<double> body) => depth == 0 ? body() : AtDepth(depth - 1, body) + 0;

    static readonly bool s_noWalk = Environment.GetEnvironmentVariable("A8_NOWALK") == "1";
    [MethodImpl(MethodImplOptions.NoInlining)]
    static PanicException Factory() => s_noWalk ? Baseline() : RuntimeErrorPanic.IndexOutOfRange(5L, 3L);

    [MethodImpl(MethodImplOptions.NoInlining)]
    static PanicException Baseline() => new PanicException(string.Format("runtime error: index out of range [{0}] with length {1}", 5L, 3L));

    static double Time(Func<PanicException> make, bool throwIt)
    {
        long sink = 0;
        for (int i = 0; i < 2000; i++) sink += make().GetHashCode() & 1;
        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < N; i++)
        {
            if (throwIt)
            {
                try { throw make(); } catch (PanicException e) { sink += e.GetHashCode() & 1; }
            }
            else sink += make().GetHashCode() & 1;
        }
        sw.Stop();
        GC.KeepAlive(sink);
        return sw.Elapsed.TotalNanoseconds / N;
    }

    static void Main(string[] args)
    {
        Console.WriteLine($"TieredCompilation={Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") ?? "default"}");
        foreach (int depth in new[] { 5, 50 })
        {
            double baseNs = AtDepth(depth, () => Time(Baseline, false));
            double factNs = AtDepth(depth, () => Time(Factory, false));
            double baseThrow = AtDepth(depth, () => Time(Baseline, true));
            double factThrow = AtDepth(depth, () => Time(Factory, true));
            Console.WriteLine($"depth {depth,3}: construct {baseNs,8:F0} ns -> factory {factNs,8:F0} ns (+{factNs - baseNs:F0}); throw+catch {baseThrow,8:F0} ns -> {factThrow,8:F0} ns (+{factThrow - baseThrow:F0}, x{factThrow / baseThrow:F2})");
        }
    }
}
