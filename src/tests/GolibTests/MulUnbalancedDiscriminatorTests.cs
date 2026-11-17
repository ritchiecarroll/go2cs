using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
// Namespace import (not just the alias) so big_package's extension methods -- SetBits, Mul -- are in
// extension-method scope; an alias alone does not participate in extension lookup.
using go.math;
using big = go.math.big_package;

namespace GolibTests;

/// <summary>
/// S0 of <c>docs/phase4/DESIGN-readmemstats-surface.md</c> §5.4 — the <c>math/big</c> discriminator.
/// </summary>
[TestClass]
public class MulUnbalancedDiscriminatorTests
{
    // WHY THIS EXISTS, and why it is a measurement rather than a guard.
    //
    // `math/big`'s TestMulUnbalanced reads MemStats.TotalAlloc around ONE nat.mul inside a
    // GOMAXPROCS(1) window and bounds the delta at 10x the input size. On the converted side the
    // ratio reads 51, and the row (224 of math/big's 226 verdicts ride behind it) has TWO candidate
    // roots that no measurement had yet separated:
    //
    //   (1) the converted nat.mul genuinely allocates ~5x what Go's does;
    //   (2) GetTotalAllocatedBytes(precise: false) is PROCESS-WIDE and unsynchronized, so the window
    //       catches other threads' allocations where Go's GOMAXPROCS(1) window catches almost none.
    //
    // Three numbers around one window separate them, and the design fixed the decision rule in
    // advance (§5.4) so this probe cannot be read after the fact to say what one hoped:
    //
    //   P = GC.GetTotalAllocatedBytes(precise: true) delta  -- process-wide, exact
    //   T = GC.GetAllocatedBytesForCurrentThread() delta    -- this thread only, pollution-free
    //   C = AllocationCounter.CurrentThreadCount delta      -- golib OBJECTS on this thread
    //
    //   T ~= P (within a few %)  => root (1). The window's allocation is the code under test's own;
    //                               the row routes to the zh-box reduction arc, and T / inputSize
    //                               says directly whether it could ever clear the 10x bound.
    //   T << P                   => root (2). The process-wide read is catching other threads and
    //                               the test's premise does not hold in the managed model; §5.5
    //                               routes that to reopening scheduler OQ8, NOT to a disclosure.
    //
    // A per-thread read is SOUND here (§5.2): Goroutine.Start gives each goroutine its own dedicated
    // thread for its whole life, the converted test host runs each test body on its own thread, and
    // nat.mul spawns nothing -- so the whole measured window runs on one thread. Soundness as a
    // MEASUREMENT is not soundness as a FIELD, though: §5.3 refuses to redefine TotalAlloc per-thread
    // (expvar publishes it over HTTP as a process quantity, and net/textproto's banked row measures
    // across it), which is exactly why this is an experiment and can run before anything ships.
    //
    // Faithfulness to the row. Go's test builds x = rndNat(50000) and y = rndNat(40) -- 50,000 and 40
    // WORDS -- and computes inputSize = (len(x)+len(y)) * _S = 400,320 bytes. `nat` is
    // package-internal, so this probe drives the same code path through the public surface:
    // Int.SetBits takes a raw little-endian []Word, and Int.Mul with distinct operands reaches
    // nat.mul with the same unbalanced shape. The words are a deterministic xorshift rather than
    // rndNat's math/rand, because nat.mul's allocation behavior is a function of LENGTH, not value,
    // and a deterministic operand makes the reading reproducible. P is reported against the row's
    // own banked readings (20,487,200 B at r57a; 20,499,128 B at zh-box A3) as the CONTROL that this
    // probe reproduces the row at all -- a P far from those would mean the probe is measuring
    // something else and no conclusion about T follows.

    private const int XWords = 50_000;
    private const int YWords = 40;
    private const int WordBytes = 8;                                  // math/big's _S on a 64-bit host
    private const long InputSize = (XWords + YWords) * (long)WordBytes; // 400,320 -- Go's inputSize
    private const long GoBound = 10;                                   // the test's ratio bound

    private static slice<big.Word> Words(int count, ulong seed)
    {
        slice<big.Word> words = new(count);
        ulong state = seed;

        for (int i = 0; i < count; i++)
        {
            // xorshift64 -- deterministic, never zero, so the top word survives nat.norm().
            state ^= state << 13;
            state ^= state >> 7;
            state ^= state << 17;
            words[i] = (big.Word)(nuint)state;
        }

        return words;
    }

    [TestMethod]
    public void MulUnbalancedAllocationDiscriminator()
    {
        AllocationCounter.Enable();

        // GOMAXPROCS(1) is a remembered value here and caps nothing (scheduler OQ8, ratified) -- it is
        // set anyway so the probe's shape matches the test's, and so the reading below is the reading
        // the row itself would take.
        nint previousProcs = go.runtime_package.GOMAXPROCS(1);

        try
        {
            ж<big.Int> x = @new<big.Int>().SetBits(Words(XWords, 0x9E3779B97F4A7C15UL));
            ж<big.Int> y = @new<big.Int>().SetBits(Words(YWords, 0xD1B54A32D192ED03UL));

            Console.WriteLine($"[S0 §5.4] operands: x={XWords} words, y={YWords} words, inputSize={InputSize} B, bound={GoBound}x");
            Console.WriteLine($"[S0 §5.4] GC: server={System.Runtime.GCSettings.IsServerGC}, latency={System.Runtime.GCSettings.LatencyMode}, procs={Environment.ProcessorCount}");

            long firstT = 0, firstP = 0;

            for (int round = 0; round < 6; round++)
            {
                // Innermost instrument closes last on the way in and first on the way out, so each
                // window contains the one Mul and nothing an outer instrument charged.
                long p0 = GC.GetTotalAllocatedBytes(precise: true);
                long t0 = GC.GetAllocatedBytesForCurrentThread();
                long c0 = AllocationCounter.CurrentThreadCount;

                ж<big.Int> z = @new<big.Int>().Mul(x, y);

                long c1 = AllocationCounter.CurrentThreadCount;
                long t1 = GC.GetAllocatedBytesForCurrentThread();
                long p1 = GC.GetTotalAllocatedBytes(precise: true);

                GC.KeepAlive(z);

                long p = p1 - p0, t = t1 - t0, c = c1 - c0;

                if (round == 0)
                {
                    firstP = p;
                    firstT = t;
                }

                Console.WriteLine($"[S0 §5.4] round {round}{(round == 0 ? " (COLD -- carries JIT + static init)" : "")}: " +
                                  $"P={p:N0} B  T={t:N0} B  C={c:N0} objects  " +
                                  $"T/P={(p == 0 ? 0 : t * 100.0 / p):F2}%  P/input={p / (double)InputSize:F1}x  T/input={t / (double)InputSize:F1}x");
            }

            Console.WriteLine($"[S0 §5.4] control: the row's banked allocSize readings are 20,487,200 B (r57a) and 20,499,128 B (zh-box A3); cold P here = {firstP:N0} B, cold T = {firstT:N0} B");

            // The probe REPORTS; it does not rule. The only assertion is that all three instruments
            // answered at all, so a silent zero can never be read as a finding.
            Assert.IsTrue(firstP > 0, "P did not move -- the process-wide instrument reported nothing.");
            Assert.IsTrue(firstT > 0, "T did not move -- the per-thread instrument reported nothing.");
        }
        finally
        {
            go.runtime_package.GOMAXPROCS(previousProcs);
        }
    }

    [TestMethod]
    public void MulUnbalancedWallClockContext()
    {
        // Root (2) needs OTHER threads to allocate INSIDE the window, so the window's duration is
        // part of the evidence: a window measured in milliseconds can only catch milliseconds of
        // another thread's allocation. Reported, never asserted.
        ж<big.Int> x = @new<big.Int>().SetBits(Words(XWords, 0x9E3779B97F4A7C15UL));
        ж<big.Int> y = @new<big.Int>().SetBits(Words(YWords, 0xD1B54A32D192ED03UL));

        _ = @new<big.Int>().Mul(x, y);

        Stopwatch sw = Stopwatch.StartNew();

        for (int i = 0; i < 10; i++)
            GC.KeepAlive(@new<big.Int>().Mul(x, y));

        sw.Stop();

        Console.WriteLine($"[S0 §5.4] one Int.Mul window = {sw.Elapsed.TotalMilliseconds / 10:F3} ms (mean of 10, warm)");
    }
}
