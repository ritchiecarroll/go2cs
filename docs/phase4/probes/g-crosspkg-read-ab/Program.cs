// xpread: the read-path A/B of DESIGN-crosspkg-exported-box section 5. An exported package-level value variable read
// in a hot loop, in each declaration form the seat could emit. Two workloads: binary.LittleEndian.Uint32 (a zero-size
// struct var read on every call, with go2cs's own emission of the method body over golib's slice<byte>) and an
// exported nint counter read. Arms are interleaved per round; each reports ns per iteration, median of the rounds.
//   F   today's plain static field
//   B   the emitted box: public static ж<T> ᏑV = new StaticBox<T>(...); ref T V => ref ᏑV.Value
//   R   the same box declared static readonly ж<T>
//   S   static readonly StaticBox<T> (the sealed type), ref ᏑV.Value
//   V   static ж<T>, ref ᏑV.ValueSlot (no nil check)
//   SV  static readonly StaticBox<T>, ref ᏑV.ValueSlot
//   RC  static readonly ж<T>, ref ((StaticBox<T>)ᏑV).ValueSlot  -- the emitted form (the field keeps type ж<T>)
//   RU  static readonly ж<T>, ref Unsafe.As<StaticBox<T>>(ᏑV).ValueSlot (unchecked; not used)
//   RV  static readonly ж<T>, ref ᏑV.ValueSlot (no cast)
// Runs: jit/jit2 and aot/aot2 are the first six arms; jit3/jit4 and aot3/aot4 add RC, RU and RV.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using go;

public partial struct littleEndian { }

public static class Bin
{
    // go2cs's emission of encoding/binary's (littleEndian).Uint32
    public static uint Uint32(this littleEndian le, slice<byte> b)
    {
        _ = b[3];
        return (uint)b[0] | (uint)b[1] << 8 | (uint)b[2] << 16 | (uint)b[3] << 24;
    }
}

public static class F { public static littleEndian LE; public static nint N = 3; }
public static class B
{
    public static ж<littleEndian> ᏑLE = new StaticBox<littleEndian>(default(littleEndian));
    public static ref littleEndian LE => ref ᏑLE.Value;
    public static ж<nint> ᏑN = new StaticBox<nint>(3);
    public static ref nint N => ref ᏑN.Value;
}
public static class R
{
    public static readonly ж<littleEndian> ᏑLE = new StaticBox<littleEndian>(default(littleEndian));
    public static ref littleEndian LE => ref ᏑLE.Value;
    public static readonly ж<nint> ᏑN = new StaticBox<nint>(3);
    public static ref nint N => ref ᏑN.Value;
}
public static class S
{
    public static readonly StaticBox<littleEndian> ᏑLE = new StaticBox<littleEndian>(default(littleEndian));
    public static ref littleEndian LE => ref ᏑLE.Value;
    public static readonly StaticBox<nint> ᏑN = new StaticBox<nint>(3);
    public static ref nint N => ref ᏑN.Value;
}
public static class V
{
    public static ж<littleEndian> ᏑLE = new StaticBox<littleEndian>(default(littleEndian));
    public static ref littleEndian LE => ref ᏑLE.ValueSlot;
    public static ж<nint> ᏑN = new StaticBox<nint>(3);
    public static ref nint N => ref ᏑN.ValueSlot;
}
public static class RC
{
    public static readonly ж<littleEndian> ᏑLE = new StaticBox<littleEndian>(default(littleEndian));
    public static ref littleEndian LE => ref ((StaticBox<littleEndian>)ᏑLE).ValueSlot;
    public static readonly ж<nint> ᏑN = new StaticBox<nint>(3);
    public static ref nint N => ref ((StaticBox<nint>)ᏑN).ValueSlot;
}
public static class RU
{
    public static readonly ж<littleEndian> ᏑLE = new StaticBox<littleEndian>(default(littleEndian));
    public static ref littleEndian LE => ref System.Runtime.CompilerServices.Unsafe.As<StaticBox<littleEndian>>(ᏑLE).ValueSlot;
    public static readonly ж<nint> ᏑN = new StaticBox<nint>(3);
    public static ref nint N => ref System.Runtime.CompilerServices.Unsafe.As<StaticBox<nint>>(ᏑN).ValueSlot;
}
public static class RV
{
    public static readonly ж<littleEndian> ᏑLE = new StaticBox<littleEndian>(default(littleEndian));
    public static ref littleEndian LE => ref ᏑLE.ValueSlot;
    public static readonly ж<nint> ᏑN = new StaticBox<nint>(3);
    public static ref nint N => ref ᏑN.ValueSlot;
}
public static class SV
{
    public static readonly StaticBox<littleEndian> ᏑLE = new StaticBox<littleEndian>(default(littleEndian));
    public static ref littleEndian LE => ref ᏑLE.ValueSlot;
    public static readonly StaticBox<nint> ᏑN = new StaticBox<nint>(3);
    public static ref nint N => ref ᏑN.ValueSlot;
}

public static class Program
{
    static slice<byte> buf = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }.slice();
    static long sink;

    [MethodImpl(MethodImplOptions.NoInlining)] static uint LeF(long n) { uint s = 0; for (long i = 0; i < n; i++) s += F.LE.Uint32(buf[(int)(i & 7)..]); return s; }
    [MethodImpl(MethodImplOptions.NoInlining)] static uint LeB(long n) { uint s = 0; for (long i = 0; i < n; i++) s += B.LE.Uint32(buf[(int)(i & 7)..]); return s; }
    [MethodImpl(MethodImplOptions.NoInlining)] static uint LeR(long n) { uint s = 0; for (long i = 0; i < n; i++) s += R.LE.Uint32(buf[(int)(i & 7)..]); return s; }
    [MethodImpl(MethodImplOptions.NoInlining)] static uint LeS(long n) { uint s = 0; for (long i = 0; i < n; i++) s += S.LE.Uint32(buf[(int)(i & 7)..]); return s; }
    [MethodImpl(MethodImplOptions.NoInlining)] static uint LeV(long n) { uint s = 0; for (long i = 0; i < n; i++) s += V.LE.Uint32(buf[(int)(i & 7)..]); return s; }
    [MethodImpl(MethodImplOptions.NoInlining)] static uint LeRC(long n) { uint s = 0; for (long i = 0; i < n; i++) s += RC.LE.Uint32(buf[(int)(i & 7)..]); return s; }
    [MethodImpl(MethodImplOptions.NoInlining)] static uint LeRU(long n) { uint s = 0; for (long i = 0; i < n; i++) s += RU.LE.Uint32(buf[(int)(i & 7)..]); return s; }
    [MethodImpl(MethodImplOptions.NoInlining)] static uint LeRV(long n) { uint s = 0; for (long i = 0; i < n; i++) s += RV.LE.Uint32(buf[(int)(i & 7)..]); return s; }
    [MethodImpl(MethodImplOptions.NoInlining)] static uint LeSV(long n) { uint s = 0; for (long i = 0; i < n; i++) s += SV.LE.Uint32(buf[(int)(i & 7)..]); return s; }

    [MethodImpl(MethodImplOptions.NoInlining)] static long NF(long n) { long s = 0; for (long i = 0; i < n; i++) { s += F.N; F.N ^= 1; } return s; }
    [MethodImpl(MethodImplOptions.NoInlining)] static long NB(long n) { long s = 0; for (long i = 0; i < n; i++) { s += B.N; B.N ^= 1; } return s; }
    [MethodImpl(MethodImplOptions.NoInlining)] static long NR(long n) { long s = 0; for (long i = 0; i < n; i++) { s += R.N; R.N ^= 1; } return s; }
    [MethodImpl(MethodImplOptions.NoInlining)] static long NS(long n) { long s = 0; for (long i = 0; i < n; i++) { s += S.N; S.N ^= 1; } return s; }
    [MethodImpl(MethodImplOptions.NoInlining)] static long NV(long n) { long s = 0; for (long i = 0; i < n; i++) { s += V.N; V.N ^= 1; } return s; }
    [MethodImpl(MethodImplOptions.NoInlining)] static long NRC(long n) { long s = 0; for (long i = 0; i < n; i++) { s += RC.N; RC.N ^= 1; } return s; }
    [MethodImpl(MethodImplOptions.NoInlining)] static long NRU(long n) { long s = 0; for (long i = 0; i < n; i++) { s += RU.N; RU.N ^= 1; } return s; }
    [MethodImpl(MethodImplOptions.NoInlining)] static long NRV(long n) { long s = 0; for (long i = 0; i < n; i++) { s += RV.N; RV.N ^= 1; } return s; }
    [MethodImpl(MethodImplOptions.NoInlining)] static long NSV(long n) { long s = 0; for (long i = 0; i < n; i++) { s += SV.N; SV.N ^= 1; } return s; }

    public static int Main(string[] args)
    {
        long n = args.Length > 0 ? long.Parse(args[0]) : 50_000_000;
        int rounds = args.Length > 1 ? int.Parse(args[1]) : 9;
        var arms = new (string name, Func<long, long> run)[]
        {
            ("le.F", k => LeF(k)), ("le.B", k => LeB(k)), ("le.R", k => LeR(k)), ("le.S", k => LeS(k)), ("le.V", k => LeV(k)), ("le.SV", k => LeSV(k)), ("le.RC", k => LeRC(k)), ("le.RU", k => LeRU(k)), ("le.RV", k => LeRV(k)),
            ("n.F", NF), ("n.B", NB), ("n.R", NR), ("n.S", NS), ("n.V", NV), ("n.SV", NSV), ("n.RC", NRC), ("n.RU", NRU), ("n.RV", NRV),
        };
        // warm-up: every arm, enough iterations for tier-1 (and OSR) under the default tiered JIT
        for (int w = 0; w < 3; w++) foreach (var a in arms) sink += a.run(n / 5);
        var times = arms.ToDictionary(a => a.name, a => new List<double>());
        for (int r = 0; r < rounds; r++)
        {
            var order = r % 2 == 0 ? arms : arms.Reverse().ToArray();
            foreach (var a in order)
            {
                var sw = Stopwatch.StartNew();
                sink += a.run(n);
                sw.Stop();
                times[a.name].Add(sw.Elapsed.TotalNanoseconds / n);
            }
        }
        foreach (var a in arms)
        {
            var t = times[a.name].OrderBy(x => x).ToList();
            Console.WriteLine($"{a.name}\tmedian={t[t.Count / 2]:F3}\tmin={t[0]:F3}\tmax={t[^1]:F3}\tns/iter");
        }
        Console.WriteLine($"sink={sink}");
        return 0;
    }
}
