using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using go;
using static go.builtin;

// Prints the first go.sort frame above a sort.Slice comparator, as the converted runtime resolves it (runtime.Caller):
// "<file>.go:<line>" when go.sort's .pdb is beside the application, "none" when it is not. The file:line of a std frame
// comes from the PACKAGE's symbol file and nowhere else, so this is what a NuGet consumer's traceback can show.
public static class Program
{
    public static int Main()
    {
        slice<nint> s = new nint[] { 3, 1, 2 }.slice();
        string found = "none";

        sort_package.Slice(s, (nint i, nint j) =>
        {
            for (nint skip = 1; skip < 8 && found == "none"; skip++)
            {
                (uintptr _, @string file, nint line, bool ok) = runtime_package.Caller(skip);
                string name = System.IO.Path.GetFileName(file.ToString());

                if (ok && line > 0 && (name == "zsortfunc.go" || name == "slice.go"))
                    found = $"{name}:{line}";
            }

            return s[i] < s[j];
        });

        System.Console.WriteLine("PACKAGE-SYMBOLS: " + found);
        System.Console.WriteLine("GOSCHED-ESCALATION: " + MeasureGoschedEscalation());
        return 0;
    }

    // The Gosched escalation's clock as THIS deployment binds it (C1, 2026-11-08, COORD ruling for the .3 release
    // aot-smoke).
    // runtime.Gosched escalates every 64th consecutive inert yield to a 1 ms wait: on macOS a kqueue timer with
    // NOTE_CRITICAL, reached through golib's [LibraryImport("libc")], with Thread.Sleep(1) as the fallback when the
    // import does not bind (~9 ms on osx-arm64; docs/phase4/DESIGN-darwin-gosched-clock.md). 6,400 lone calls on a
    // fresh thread make about 100 escalations, and the slowest 1 in 64 calls are those escalations: their p50 and max
    // are printed.
    // MEASURED, NOT JUDGED: the driver prints the line and never fails on it.
    private static string MeasureGoschedEscalation()
    {
        const int calls = 64 * 100;
        double[] ms = new double[calls];

        var worker = new Thread(() =>
        {
            for (int i = 0; i < calls; i++)
            {
                long started = Stopwatch.GetTimestamp();
                runtime_package.Gosched();
                ms[i] = (Stopwatch.GetTimestamp() - started) * 1000.0 / Stopwatch.Frequency;
            }
        });

        worker.Start();
        worker.Join();

        System.Array.Sort(ms);
        int slowest = calls / 64;
        double p50 = ms[calls - slowest + slowest / 2];

        return $"slowest {slowest} of {calls} calls p50={p50:F3} ms max={ms[calls - 1]:F3} ms " +
            $"({RuntimeInformation.RuntimeIdentifier})";
    }
}
