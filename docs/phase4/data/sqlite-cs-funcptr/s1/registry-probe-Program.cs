// Prototype of golib's function-pointer registry for ccgo code (sqlite-cs S1). JIT and Native AOT.
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

// ---- the registry (golib) ----
public static class GoFuncPtr
{
    // Numbers live where no data pointer can: the kernel half on 64-bit (x64 canonical kernel addresses; arm64 bit 55
    // set, which TTBR1 maps, even under top-byte-ignore), 16-aligned, so a number dereferenced as data faults and a
    // number is never 0 or ^0 (SQLITE_TRANSIENT).
    private const ulong Base = 0xFFFF_F000_0000_0000UL;
    private static readonly ConcurrentDictionary<Delegate, nuint> s_numbers = new();   // Delegate.Equals: same type, method, target
    private static Delegate?[] s_delegates = new Delegate?[64];
    private static int s_count;
    private static readonly object s_lock = new();

    /// <summary>__ccgo_fp(f), and cFuncPointer(f): the number of a func value. One number per function.</summary>
    public static nuint Of(Delegate? d)
    {
        if (d is null) return 0;
        return s_numbers.GetOrAdd(d, static d =>
        {
            lock (s_lock)
            {
                if (s_count == s_delegates.Length) Array.Resize(ref s_delegates, s_count * 2);
                s_delegates[s_count] = d;
                return (nuint)(Base + ((ulong)s_count++ << 4));
            }
        });
    }

    /// <summary>The ccgo recovery *(*F)(unsafe.Pointer(&amp;struct{ f uintptr }{n})): the func value a number stands for.</summary>
    public static F? Get<F>(nuint n) where F : Delegate
    {
        if (n == 0) return null;                                   // Go's nil func
        ulong u = n;
        if (u < Base || (u & 0xF) != 0 || (long)((u - Base) >> 4) >= Volatile.Read(ref s_count))
            throw new InvalidOperationException($"0x{u:x} is not a function pointer go2cs minted");
        Delegate d = s_delegates[(int)((u - Base) >> 4)]!;
        return d as F ?? throw new InvalidCastException($"function pointer 0x{u:x} holds {d.GetType()} and is called as {typeof(F)}");
    }
}

public sealed class TLS { }

public static class Program
{
    static int Compare(TLS tls, nuint a, nuint b) => a < b ? -1 : a > b ? 1 : 0;
    static int Other(TLS tls, nuint a, nuint b) => 0;
    delegate int NamedCmp(TLS tls, nuint a, nuint b);

    static int fail;
    static void Check(string what, bool ok, string detail = "") { Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {what} {detail}"); if (!ok) fail++; }
    static string Try(Action a) { try { a(); return "no exception"; } catch (Exception e) { return e.GetType().Name + ": " + e.Message; } }

    public static int Main()
    {
        // MINT: one number per function, from any site, even though each site makes a new delegate
        nuint n1 = GoFuncPtr.Of(new Func<TLS, nuint, nuint, int>(Compare));
        nuint n2 = GoFuncPtr.Of(new Func<TLS, nuint, nuint, int>(Compare));
        nuint n3 = GoFuncPtr.Of(new Func<TLS, nuint, nuint, int>(Other));
        Check("one number per function across mint sites (busy.go's h == cFuncPointer(busyHandler))", n1 == n2 && n1 != n3, $"0x{(ulong)n1:x} 0x{(ulong)n3:x}");
        Check("a number is 16-aligned in the kernel half, never 0 and never ^0 (SQLITE_TRANSIENT)", n1 != 0 && (ulong)n1 != ulong.MaxValue && ((ulong)n1 & 0xF) == 0 && (ulong)n1 >= 0xFFFF_F000_0000_0000UL);
        nuint nNamed = GoFuncPtr.Of(new NamedCmp(Compare));
        Check("CONTROL: the same function minted under ANOTHER delegate type gets another number (so the converter types every mint by the function's own Go signature)", nNamed != n1);

        // RECOVERY: the very delegate, so an M:N twin lookup keyed on it still finds its twin
        var f = GoFuncPtr.Get<Func<TLS, nuint, nuint, int>>(n1)!;
        Check("recovery calls the function", f(new TLS(), 1, 2) == -1);
        Check("recovery returns the minted instance (identity for N3's twin lookup)", ReferenceEquals(f, GoFuncPtr.Get<Func<TLS, nuint, nuint, int>>(n1)));
        Check("zero is Go's nil func", GoFuncPtr.Get<Func<TLS, nuint, nuint, int>>(0) is null);
        string wrong = Try(() => GoFuncPtr.Get<Func<TLS, nuint, int>>(n1));
        Check("a recovery under another signature (a C cast) fails loudly, naming both types", wrong.StartsWith("InvalidCastException"), wrong);
        string stale = Try(() => GoFuncPtr.Get<Func<TLS, nuint, nuint, int>>((nuint)0xFFFF_F000_0000_1000UL));
        Check("an unknown number fails loudly", stale.StartsWith("InvalidOperationException"), stale);
        string data = Try(() => GoFuncPtr.Get<Func<TLS, nuint, nuint, int>>((nuint)0x7f00_1234_5670UL));
        Check("a data pointer used as a function pointer fails loudly", data.StartsWith("InvalidOperationException"), data);

        // cost (informational)
        var cmp = new Func<TLS, nuint, nuint, int>(Compare);
        var sw = Stopwatch.StartNew(); nuint sink = 0;
        for (int i = 0; i < 1_000_000; i++) sink ^= GoFuncPtr.Of(cmp);
        double mintNs = sw.Elapsed.TotalNanoseconds / 1_000_000;
        sw.Restart(); int s2 = 0; var tls = new TLS();
        for (int i = 0; i < 1_000_000; i++) s2 += GoFuncPtr.Get<Func<TLS, nuint, nuint, int>>(n1)!(tls, 2, 1);
        double getNs = sw.Elapsed.TotalNanoseconds / 1_000_000;
        Console.WriteLine($"COST mint (a lookup of an existing delegate) {mintNs:F1} ns; recover and call {getNs:F1} ns ({sink != 1}, {s2})");
        Console.WriteLine($"failed: {fail}");
        return fail;
    }
}
