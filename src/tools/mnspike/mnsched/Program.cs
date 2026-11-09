// The P2 arms (the spike plan, COORD-approved 2026-11-09). Each arm prints one ARM line, PASS or FAIL, and exits 0 or 1.
// --no-injection and --no-handoff switch a mechanism off, so each mechanism's arm can be shown failing without it.
//   a1  preemption parity: on ONE P, a goroutine spins on a flag that a second, queued goroutine sets. Go 1.14+
//       preempts the spinner; today's 1:1 model has the OS do it. Here carrier injection must, or it livelocks.
//   a2  the untwinned park: on ONE P, a goroutine blocks SYNCHRONOUSLY on a channel receive (no twin) while the sender
//       is queued behind it. The handoff must free the P, or it deadlocks (the sizing note's §4.E trap).
//   a3  twinned scale: N goroutines park on one channel through the twin path; the program must hold no thread per
//       parked goroutine (carriers stay at about GOMAXPROCS).
//   a4  mixed paths: twinned and untwinned senders and receivers ping-pong on shared channels; no wakeup may be lost.
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MnSched;

public static class Program
{
    public static int Main(string[] args)
    {
        string arm = args.FirstOrDefault() ?? "";
        bool noInjection = args.Contains("--no-injection"), noHandoff = args.Contains("--no-handoff");
        int n = args.SkipWhile(a => a != "--n").Skip(1).Select(int.Parse).FirstOrDefault(100_000);
        var timeout = TimeSpan.FromSeconds(10);

        Scheduler Make(int procs) => new(new Options { Procs = procs, Injection = !noInjection, Handoff = !noHandoff });

        string mode = $"injection={(noInjection ? "off" : "on")} handoff={(noHandoff ? "off" : "on")}";
        var sw = Stopwatch.StartNew();
        bool pass; string detail;

        switch (arm)
        {
            case "a1":
            {
                Scheduler s = Make(1);
                var done = new WaitGroup(s);
                done.Add(2);
                int flag = 0;
                s.Go(() => { while (Volatile.Read(ref flag) == 0) { } done.Done(); return default; });   // spins, never parks
                s.Go(() => { Volatile.Write(ref flag, 1); done.Done(); return default; });
                pass = done.Wait(timeout);
                detail = $"injections={s.Injections} carriers={s.CarrierCount}";
                break;
            }
            case "a2":
            {
                Scheduler s = Make(1);
                var ch = new Chan<int>(s);
                var done = new WaitGroup(s);
                done.Add(2);
                int got = 0;
                s.Go(() => { got = ch.Recv(); done.Done(); return default; });          // untwinned: blocks the thread
                s.Go(async () => { await ch.SendAsync(42); done.Done(); });
                pass = done.Wait(timeout) && got == 42;
                detail = $"got={got} handoffs={s.Handoffs} carriers={s.CarrierCount}";
                break;
            }
            case "a3":
            {
                int procs = Environment.ProcessorCount;
                Scheduler s = Make(procs);
                var ch = new Chan<int>(s);
                var parked = new WaitGroup(s);
                var finished = new WaitGroup(s);
                parked.Add(n); finished.Add(n);
                long sum = 0;
                for (int i = 0; i < n; i++)
                    s.Go(async () => { parked.Done(); int v = await ch.RecvAsync(); Interlocked.Add(ref sum, v); finished.Done(); });

                bool allParked = parked.Wait(TimeSpan.FromSeconds(120));
                double parkSeconds = sw.Elapsed.TotalSeconds;
                Process self = Process.GetCurrentProcess(); self.Refresh();
                long privateMb = self.PrivateMemorySize64 / (1024 * 1024); int threads = self.Threads.Count;
                for (int i = 1; i <= n; i++) ch.Send(i);                                      // main: not a carrier
                bool allDone = finished.Wait(TimeSpan.FromSeconds(120));
                long expected = (long)n * (n + 1) / 2;
                int carriersAtEnd = s.CarrierCount;
                Thread.Sleep(500);                                         // surplus carriers retire after 100 ms idle
                int carriersSettled = s.CarrierCount;
                pass = allParked && allDone && Interlocked.Read(ref sum) == expected && carriersSettled <= procs + 2;
                detail = $"n={n} parked-in={parkSeconds:F2}s private={privateMb}MB threads-while-parked={threads} carriers-at-end={carriersAtEnd} carriers-settled={carriersSettled} procs={procs} injections={s.Injections} gc-pause={GC.GetTotalPauseDuration().TotalMilliseconds:F0}ms sum-ok={Interlocked.Read(ref sum) == expected}";
                break;
            }
            case "a4":
            {
                Scheduler s = Make(2);
                const int pairs = 500, rounds = 200;
                var done = new WaitGroup(s);
                done.Add(pairs * 2);
                long hits = 0;
                for (int p = 0; p < pairs; p++)
                {
                    var ping = new Chan<int>(s); var pong = new Chan<int>(s);
                    bool twinA = p % 2 == 0, twinB = p % 3 == 0;
                    if (twinA)
                        s.Go(async () => { for (int r = 0; r < rounds; r++) { await ping.SendAsync(r); await pong.RecvAsync(); } done.Done(); });
                    else
                        s.Go(() => { for (int r = 0; r < rounds; r++) { ping.Send(r); pong.Recv(); } done.Done(); return default; });
                    if (twinB)
                        s.Go(async () => { for (int r = 0; r < rounds; r++) { await ping.RecvAsync(); Interlocked.Increment(ref hits); await pong.SendAsync(r); } done.Done(); });
                    else
                        s.Go(() => { for (int r = 0; r < rounds; r++) { ping.Recv(); Interlocked.Increment(ref hits); pong.Send(r); } done.Done(); return default; });
                }

                pass = done.Wait(TimeSpan.FromSeconds(60)) && Interlocked.Read(ref hits) == pairs * rounds;
                detail = $"hits={Interlocked.Read(ref hits)}/{pairs * rounds} handoffs={s.Handoffs} carriers={s.CarrierCount}";
                break;
            }
            default:
                Console.WriteLine("usage: mnsched a1|a2|a3|a4 [--no-injection] [--no-handoff] [--n N]");
                return 2;
        }

        Console.WriteLine($"ARM {arm} {(pass ? "PASS" : "FAIL")} ({mode}) {detail} wall={sw.Elapsed.TotalSeconds:F2}s");
        Console.Out.Flush();
        // A failed arm leaves goroutines blocked by design: exit the process rather than join them.
        Environment.Exit(pass ? 0 : 1);
        return 0;
    }
}
