namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

partial struct counter /*num:nint*/;

internal static nint Len(this counter c) {
    return (nint)c;
}

partial struct box {
    internal nint n;
}

internal static nint Len(this ref box b) {
    return b.n;
}

internal partial interface run_type /*dyn*/ {
    nint Len();
}

internal static nint run(nint n) {
    var values = slice<any>([(counter)3, Ꮡ(new box(n: 5))]);
    nint total = 0;

    for (nint i = 0; i < n; i++) {
        {
            var (v, ok) = values[0]._<run_type>(ᐧ); if (ok) {
                total += v.Len();
            }
        }

        {
            var (v, ok) = values[1]._<run_type>(ᐧ); if (ok) {
                total += v.Len();
            }
        }
    }

    return total;
}

internal static void Main() {
    // P1 AOT PROBE (scratch instrument, never a converted program): times the FIRST structural assert of each
    // shell tier, then a second round, in-process. Times are microseconds from Stopwatch.
    var values = slice<any>([(counter)3, Ꮡ(new box(n: 5))]);
    double us(long a, long b) => (b - a) * 1e6 / System.Diagnostics.Stopwatch.Frequency;
    long t0 = System.Diagnostics.Stopwatch.GetTimestamp();
    var (v0, ok0) = values[0]._<run_type>(ᐧ);
    long t1 = System.Diagnostics.Stopwatch.GetTimestamp();
    var (v1, ok1) = values[1]._<run_type>(ᐧ);
    long t2 = System.Diagnostics.Stopwatch.GetTimestamp();
    var (v2, ok2) = values[0]._<run_type>(ᐧ);
    var (v3, ok3) = values[1]._<run_type>(ᐧ);
    long t3 = System.Diagnostics.Stopwatch.GetTimestamp();
    nint total = run(1000);
    long t4 = System.Diagnostics.Stopwatch.GetTimestamp();
    double sinceStart = (DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalMilliseconds;
    System.Console.WriteLine($"ok {ok0} {ok1} {ok2} {ok3} len {v0.Len() + v1.Len() + v2.Len() + v3.Len()} total {total}");
    System.Console.WriteLine($"P1PROBE\tfirst_value_us\t{us(t0, t1):F0}\tfirst_ptr_us\t{us(t1, t2):F0}\tsecond_pair_us\t{us(t2, t3):F1}\trun1000_us\t{us(t3, t4):F0}\tmain_end_ms\t{sinceStart:F1}");
}

} // end main_package
