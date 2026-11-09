// G4, the M:N spike's delegate-table cost (docs/phase4/DESIGN-mn-scheduler.md §5 O6: "dynamic dispatch: func values").
// A twin that reaches a Go func value holds a DELEGATE (net/http's HandlerFunc is one) and must find the target's
// TWIN to stay on the twin path. Three candidate tables, each measured as the extra cost of one dynamic call:
//   T1  Dictionary<MethodInfo, invoker>   keyed by the delegate's Method (named functions and method values only)
//   T2  ConditionalWeakTable<delegate, twin> keyed by the delegate INSTANCE, registered when the delegate is made: the
//       only one that serves func LITERALS (a closure's compiler-generated class cannot carry a twin method, so its
//       twin lambda is registered beside it); its registration cost is measured too
//   T3  a per-delegate-type static cache keyed by the method handle (named functions and method values only)
// R0 is R2's baseline: the same literal made without a registration.
// Arms, 10^7 calls each, one process per arm: A0 a plain synchronous delegate call (the baseline); T1, T2 and T3 lookup
// plus the twin's call; C+ T3 with a calibrated ~20 ns spin inside the lookup (the positive control: the instrument
// must read it); R2 T2's registration per delegate; RT one park/resume round trip on the P2 scheduler (two twinned
// goroutines ping-ponging on unbuffered channels), the cost a lookup should be read against.
// Usage: twindispatch <arm> ; prints "ARM <arm> <ns-per-op> ns"
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MnSched;

namespace TwinDispatch;

public sealed class Writer { public long Sum; }

public delegate void HandlerFunc(Writer w, int r);           // the visible func type
public delegate ValueTask HandlerFuncTwin(Writer w, int r);  // its twin's shape

public static class Handlers
{
    public static void Hello(Writer w, int r) => w.Sum += r;                                  // visible
    public static ValueTask Hello_twin(Writer w, int r) { w.Sum += r; return default; }     // its twin
}

public static class Program
{
    private const int Calls = 10_000_000;

    // T1
    private static readonly Dictionary<MethodInfo, Func<object?, Writer, int, ValueTask>> s_t1 = new();
    // T2
    private static readonly ConditionalWeakTable<HandlerFunc, HandlerFuncTwin> s_t2 = new();
    // T3: the per-delegate-type static cache
    private static class TwinCache<TDelegate> { public static readonly Dictionary<IntPtr, Func<object?, Writer, int, ValueTask>> Map = new(); }

    private static int s_spinIterations;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Func<object?, Writer, int, ValueTask>? T3(HandlerFunc f, bool spin)
    {
        if (spin) Thread.SpinWait(s_spinIterations);
        return TwinCache<HandlerFunc>.Map.TryGetValue(f.Method.MethodHandle.Value, out var twin) ? twin : null;
    }

    public static int Main(string[] args)
    {
        string arm = args.Length > 0 ? args[0] : "";
        var w = new Writer();
        HandlerFunc f = Handlers.Hello;
        Func<object?, Writer, int, ValueTask> invoker = static (_, wr, r) => Handlers.Hello_twin(wr, r);
        s_t1[f.Method] = invoker;
        TwinCache<HandlerFunc>.Map[f.Method.MethodHandle.Value] = invoker;
        s_t2.Add(f, Handlers.Hello_twin);
        Calibrate();

        // Warm every path once.
        for (int i = 0; i < 100_000; i++) { f(w, 1); _ = s_t1[f.Method](null, w, 1); _ = s_t2.TryGetValue(f, out var tw) ? tw(w, 1) : default; _ = T3(f, false)!(null, w, 1); }

        double ns;
        var sw = Stopwatch.StartNew();
        switch (arm)
        {
            case "A0":
                for (int i = 0; i < Calls; i++) f(w, i);
                ns = PerOp(sw, Calls);
                break;
            case "T1":
                for (int i = 0; i < Calls; i++) { if (s_t1.TryGetValue(f.Method, out var t)) Await(t(f.Target, w, i)); else f(w, i); }
                ns = PerOp(sw, Calls);
                break;
            case "T2":
                for (int i = 0; i < Calls; i++) { if (s_t2.TryGetValue(f, out var t)) Await(t(w, i)); else f(w, i); }
                ns = PerOp(sw, Calls);
                break;
            case "T3":
                for (int i = 0; i < Calls; i++) { var t = T3(f, false); if (t is not null) Await(t(f.Target, w, i)); else f(w, i); }
                ns = PerOp(sw, Calls);
                break;
            case "C+":
                for (int i = 0; i < Calls; i++) { var t = T3(f, true); if (t is not null) Await(t(f.Target, w, i)); else f(w, i); }
                ns = PerOp(sw, Calls);
                break;
            case "R2":
            {
                // A func literal made in a loop: each creation registers its twin (the generator's emission at the site).
                const int made = 2_000_000;
                var keep = new HandlerFunc[1024];
                sw.Restart();
                for (int i = 0; i < made; i++)
                {
                    int captured = i;
                    HandlerFunc lit = (wr, r) => wr.Sum += r + captured;
                    s_t2.AddOrUpdate(lit, (wr, r) => { wr.Sum += r + captured; return default; });
                    keep[i & 1023] = lit;
                }
                ns = PerOp(sw, made);
                break;
            }
            case "R0":
            {
                // R2's baseline: the same func literal made in the same loop, with no twin registered.
                const int made = 2_000_000;
                var keep = new HandlerFunc[1024];
                sw.Restart();
                for (int i = 0; i < made; i++)
                {
                    int captured = i;
                    HandlerFunc lit = (wr, r) => wr.Sum += r + captured;
                    keep[i & 1023] = lit;
                }
                ns = PerOp(sw, made);
                break;
            }
            case "RT":
            {
                const int rounds = 1_000_000;
                var s = new Scheduler(new Options { Procs = 2 });
                var ping = new Chan<int>(s); var pong = new Chan<int>(s);
                var done = new MnSched.WaitGroup(s);
                done.Add(2);
                sw.Restart();
                s.Go(async () => { for (int r = 0; r < rounds; r++) { await ping.SendAsync(r); await pong.RecvAsync(); } done.Done(); });
                s.Go(async () => { for (int r = 0; r < rounds; r++) { await ping.RecvAsync(); await pong.SendAsync(r); } done.Done(); });
                done.Wait(TimeSpan.FromMinutes(5));
                ns = PerOp(sw, rounds);
                break;
            }
            default:
                Console.WriteLine("usage: twindispatch A0|T1|T2|T3|C+|R0|R2|RT");
                return 2;
        }

        Console.WriteLine($"ARM {arm} {ns:F2} ns (sum {w.Sum & 0xff})");
        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Await(ValueTask t) { if (!t.IsCompletedSuccessfully) t.AsTask().GetAwaiter().GetResult(); }

    private static double PerOp(Stopwatch sw, long ops) => sw.Elapsed.TotalNanoseconds / ops;

    // SpinWait(k)'s cost differs by machine: pick k so one call is about 20 ns.
    private static void Calibrate()
    {
        s_spinIterations = 1;
        for (int k = 1; k < 4096; k *= 2)
        {
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 200_000; i++) Thread.SpinWait(k);
            if (sw.Elapsed.TotalNanoseconds / 200_000 >= 20) { s_spinIterations = k; return; }
        }
    }
}
