// ParkStorm's TWIN on the P2 scheduler: src/tools/mnspike/parkstorm/main.go, line for line, as go2cs-gen would emit it
// under O6 (the goroutine body's park, `<-release`, becomes `await release.RecvAsync()`; main's waits stay synchronous on
// the program's main thread, which is not a carrier). It prints the same lines as the Go program, so
// Measure-ParkStorm.ps1 measures both the same way. Usage: parkstorm-twin <n> <holdSeconds>
using System;
using System.Diagnostics;
using System.Threading;
using MnSched;

namespace ParkStormTwin;

public static class Program
{
    public static int Main(string[] args)
    {
        int n = int.Parse(args[0]);
        int hold = int.Parse(args[1]);
        var s = new Scheduler(new Options());

        var release = new Chan<ValueTuple>(s);                    // make(chan struct{})
        var started = new WaitGroup(s);
        var finished = new WaitGroup(s);
        started.Add(n);
        finished.Add(n);

        var t0 = Stopwatch.StartNew();
        int tenth = Math.Max(n / 10, 1);

        for (int i = 0; i < n; i++)
        {
            s.Go(async () =>                                      // go func() { ... }()
            {
                started.Done();
                await release.RecvAsync();                        // <-release
                finished.Done();
            });

            if ((i + 1) % tenth == 0) Console.WriteLine($"STARTED {i + 1} of {n} in {t0.Elapsed.TotalSeconds:F2}s");
        }

        started.Wait(Timeout.InfiniteTimeSpan);
        Console.WriteLine($"PARKED {n} goroutines={s.Live + 1} in {t0.Elapsed.TotalSeconds:F2}s");

        Thread.Sleep(TimeSpan.FromSeconds(hold));                 // the harness samples memory and threads here

        var t1 = Stopwatch.StartNew();
        release.Close();                                          // close(release)
        finished.Wait(Timeout.InfiniteTimeSpan);
        Console.WriteLine($"RELEASED {n} in {t1.Elapsed.TotalSeconds:F2}s");
        return 0;
    }
}
