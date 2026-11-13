// Times the first use of one family type (assembly load + type load + JIT), and reports the process's RSS.
// Built once per family variant; argument: none.
using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;


public static class Program
{
    public static int Main()
    {
        long t0 = Stopwatch.GetTimestamp();
        int v = Use();
        long t1 = Stopwatch.GetTimestamp();
        string rss = "?";
        try { foreach (string line in File.ReadLines("/proc/self/status")) if (line.StartsWith("VmRSS:")) rss = line[6..].Trim(); } catch { }
        Console.WriteLine($"first-use-us {(t1 - t0) * 1_000_000 / Stopwatch.Frequency} value {v} rss {rss}");
        return v == 9 ? 0 : 1;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int Use()
    {
        var a = new sbyte[201];
        a[5] = 4;
        var b = (sbyte[])a.Clone();
        b[6] = 5;
        return a[5] + b[6] + (a.AsSpan().SequenceEqual(b) ? 100 : 0);
    }
}
