using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// THE POSITIVE CONTROL FOR THE Q44 §10.5 REGISTRY CENSUS.
//
// The census counts four arms at ж.cs's `uintptr -> ж<T>` operator. A count of zero in any arm is
// only a READING if that counter can be made to move; otherwise the zero came from never being
// wired, which is this tree's most-paid lesson. So every arm is driven DELIBERATELY here and each
// assertion is that the counter INCREASED across the call -- not that a total matches a guess, which
// is precisely the shape that cannot distinguish a wired counter from an unwired one.
//
// Gated on the census being enabled (`GO2CS_Q44_CENSUS`), because the counters are off by default
// and a control that silently passes with the instrument off would be worse than no control at all.
// With it off this reports NOT MEASURED rather than green.
[TestClass]
public class Q44RegistryCensusControlTests
{
    private struct RefBearing { internal string name; internal nint scalar; }
    private struct OtherType  { internal string other; internal nint scalar; }

    [TestInitialize]
    public void RequireTheCensusEnabled()
    {
        if (!Q44RegistryCensus.Enabled)
        {
            Assert.Inconclusive("NOT MEASURED: the Q44 census is off. Set GO2CS_Q44_CENSUS to run this control; " +
                                "a green here with the instrument off would be a lie about a counter nothing drove.");
        }
    }

    [TestMethod]
    public void Arm1_SamePointeeType_IsCounted()
    {
        ж<RefBearing> box = new StandardBox<RefBearing>(new RefBearing { name = "tcp", scalar = 0x5A5A });
        nuint token = box.PointerOrderToken;
        ManagedPointerTokens.Register(token, box);

        long before = Q44RegistryCensus.Snapshot().arm1;
        var back = (ж<RefBearing>)(uintptr)token;

        Assert.IsTrue(Q44RegistryCensus.Snapshot().arm1 > before, "arm 1's counter must move");
        Assert.AreSame(box, back, "and arm 1 must alias the SAME box -- the count is worthless if the arm is wrong");
    }

    [TestMethod]
    public void Arm2_DifferentPointeeTypeAtOffsetZero_IsCounted()
    {
        // The write's case, and the one §10.3 calls new work: the token names a live box whose pointee
        // type is not T. It reaches NEITHER arm 1's alias nor arm 3's refusal today.
        ж<RefBearing> box = new StandardBox<RefBearing>(new RefBearing { name = "udp" });
        nuint token = box.PointerOrderToken;
        ManagedPointerTokens.Register(token, box);

        long before = Q44RegistryCensus.Snapshot().arm2a;

        try
        {
            _ = (ж<OtherType>)(uintptr)token;
        }
        catch (PanicException)
        {
            // Counted before any refusal, so the arm registers either way.
        }

        Assert.IsTrue(Q44RegistryCensus.Snapshot().arm2a > before,
            "arm 2a's counter must move -- offset 0, different pointee type");
    }

    [TestMethod]
    public void Arm3_InsideALiveBlockButNotTheToken_IsCounted()
    {
        ж<RefBearing> box = new StandardBox<RefBearing>(new RefBearing { name = "ip" });
        nuint token = box.PointerOrderToken;
        ManagedPointerTokens.Register(token, box);

        long before = Q44RegistryCensus.Snapshot().arm3;

        Assert.ThrowsException<PanicException>(() => { _ = (ж<RefBearing>)(uintptr)(token + 8); },
            "token + 8 is arithmetic on a live token and must still refuse");

        Assert.IsTrue(Q44RegistryCensus.Snapshot().arm3 > before, "arm 3's counter must move");
    }

    [TestMethod]
    public unsafe void Arm4_ARealAddress_IsCounted()
    {
        long local = 0;
        long before = Q44RegistryCensus.Snapshot().arm4;

        _ = (ж<long>)(uintptr)(nuint)(&local);

        Assert.IsTrue(Q44RegistryCensus.Snapshot().arm4 > before, "arm 4's counter must move for a real address");
    }

    [TestMethod]
    public void TheMintCounterMoves()
    {
        ж<RefBearing> box = new StandardBox<RefBearing>(new RefBearing { name = "unix" });
        long before = Q44RegistryCensus.Snapshot().mints;

        ManagedPointerTokens.Register(box.PointerOrderToken, box);

        Assert.IsTrue(Q44RegistryCensus.Snapshot().mints > before,
            "the projection mint must be counted (RegisterPinned is a DIFFERENT mint and deliberately is not)");
    }

    [TestMethod]
    public void TheCensusCanActuallyREPORT_TheFileAppears()
    {
        // ⚠ THE ARM THAT WAS MISSING, AND IT COST A MEASUREMENT. The first version of this control
        // proved all four arms fire and said nothing about whether the census could REPORT: the dump
        // went to stderr from a ProcessExit hook, and the MSTest host swallowed it -- every counter
        // wired, every arm green, and zero census lines in the log. A counter that moves into a
        // channel nobody reads is the same defect as a counter that never moves, and harder to see.
        // ⚠ ITS OWN FILE, and this is a defect this control already caused once. The first version
        // used the census's configured OutputPath and DELETED it before dumping -- so running the
        // control inside a census run wiped the census's own file mid-flight and re-wrote it with a
        // partial block. An instrument's control must not be able to damage the instrument's output.
        string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                                             $"q44-census-control-{Environment.ProcessId}.txt");

        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);

        Environment.SetEnvironmentVariable("GO2CS_Q44_CENSUS_FILE_OVERRIDE", path);

        try
        {
            Q44RegistryCensus.DumpTo(path);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GO2CS_Q44_CENSUS_FILE_OVERRIDE", null);
        }

        Assert.IsTrue(System.IO.File.Exists(path), $"the census must write {path}; a dump nobody can read is not a report");

        string[] lines = System.IO.File.ReadAllLines(path);
        Assert.IsTrue(lines.Length >= 2, "the dump must carry at least the totals line and the reconciliation line");
        StringAssert.StartsWith(lines[0], "Q44CENSUS ", "the totals line must be first and greppable");
        Assert.IsTrue(Array.Exists(lines, l => l.StartsWith("Q44CENSUS-RECONCILES", StringComparison.Ordinal)),
            "the reconciliation must be RECORDED, not merely computed -- a census whose exhaustiveness is not in the artifact cannot be checked later");

        // ⚠ THE FOLD RULE MUST BE IN THE ARTIFACT, because a reader who does not know it gets a
        // plausible WRONG number rather than an error. The partial flush made one process write
        // several blocks, they are CUMULATIVE SNAPSHOTS rather than increments, and i9 read a row
        // 1.96x high by summing them (mailbox c62ca28686) -- correctly, by the method that had been
        // right until the flush existed. Nothing in the output said the shape had changed. It says
        // so now, and this arm is what keeps it saying so.
        Assert.IsTrue(Array.Exists(lines, l => l.StartsWith("Q44CENSUS-FOLD", StringComparison.Ordinal)),
            "every block must state the fold rule -- LAST block per file, summed across files; an output that " +
            "invites the naive sum is an instrument defect, not a reader error");
    }

    [TestMethod]
    public void ACensusPathWithoutPidIsMadePerProcess_BecauseASharedPathDESTROYSBlocks()
    {
        // ⚠ MEASURED, not reasoned: two processes writing ONE census path, and the second's first
        // write truncated the first's entire census -- 19 blocks gone, no error, no report. A row
        // that then reads like a small measured one is a DESTROYED one, which is this instrument's
        // own falsifier. So a configured path without {pid} is made per-process rather than shared.
        //
        // A timestamp heuristic was tried first and DISCARDED: its verdict depends on how often the
        // OTHER process happens to write, and three instruments in a row could not exercise the
        // ordering that breaks it. This arm guards the STRUCTURAL property instead, which no
        // ordering or cadence can defeat.
        const string key = "GO2CS_Q44_CENSUS_FILE";
        string saved = Environment.GetEnvironmentVariable(key);
        try
        {
            string dir = System.IO.Path.GetTempPath();
            string pid = Environment.ProcessId.ToString();

            Environment.SetEnvironmentVariable(key, System.IO.Path.Combine(dir, "q44-noPidToken.txt"));
            string resolved = Q44RegistryCensus.OutputPath;
            StringAssert.Contains(resolved, pid,
                "a path carrying no {pid} must still resolve PER PROCESS -- a shared path loses a whole " +
                "process's census silently, which is worse than any filename surprise");
            Assert.AreNotEqual(System.IO.Path.Combine(dir, "q44-noPidToken.txt"), resolved,
                "the configured path must have been rewritten, not returned as given");

            // And the token form is honoured EXACTLY -- the already-correct case must not move.
            Environment.SetEnvironmentVariable(key, System.IO.Path.Combine(dir, "q44-{pid}-token.txt"));
            Assert.AreEqual(System.IO.Path.Combine(dir, $"q44-{pid}-token.txt"), Q44RegistryCensus.OutputPath,
                "a caller who wrote {pid} gets exactly that substitution and no second rewrite");
        }
        finally
        {
            Environment.SetEnvironmentVariable(key, saved);
        }
    }

    [TestMethod]
    public void TheArmsAreExhaustive_TheSumReconcilesWithTheConversionCount()
    {
        // The property that makes every other count meaningful: each conversion lands in exactly one
        // arm. If the sum drifts from the conversion count the classification is not exhaustive, and
        // no per-arm number below it means anything.
        var s = Q44RegistryCensus.Snapshot();

        Assert.AreEqual(s.conversions, s.arm1 + s.arm2a + s.arm2b + s.arm3 + s.arm4,
            "the arms must sum to the conversions -- the classification is exhaustive or the census is broken");
    }

    // ---- The two guards the 2026-09-08 neutrality fix owes, each RED on the code it replaced ----

    // ⚠ A NEUTRALITY GUARD STOOD HERE AND IS GONE (2026-09-08), with its whole measurement kept
    // because the next person to want one will want it for the same reason.
    //
    // It asserted "one conversion enters Resolve exactly once", which DID discriminate: it read
    // Expected:<1>. Actual:<2> on the classifier that flipped a banked row. Two earlier
    // formulations did not, and both are worth knowing: "a conversion must not change the
    // registered count" FAILS ON CORRECT CODE, because the one resolve the operator legitimately
    // performs evicts a dead weak entry whether the census is on or off; and counting evictions
    // cannot see a second call either, since two resolves of one token cannot evict twice.
    //
    // What retired it is that its instrument was a counter on the hot path of `Resolve`, taken
    // 264,167 times in a single roster row -- so the thing built to prove the census does no extra
    // work WAS extra work, and i9 named it as the next perturbation candidate off the very diff
    // that removed the previous one. An instrument built out of the thing under test cannot
    // independently measure it, and here it could not even be neutral while trying.
    //
    // The gate is the banked `os` row: it discriminates in about 50 seconds per direction and has
    // now caught two successive states of this census. A unit proxy cannot beat that and can
    // perturb what it measures, so there is deliberately no replacement.

    [TestMethod]
    public void The2a2bDiscriminatorUsesTheRegistrysOwnProjection_NotACopyOfIt()
    {
        // ⚠ THE CLASSIFIER GUARD. The discriminator asks "is this number the box's own token, i.e.
        // offset 0?" -- the same question ManagedPointerTokens.CurrentToken answers when Resolve
        // validates an entry. The census carried a TWO-ARM COPY of that rule (INilPointer, IChannel,
        // else 0) while CurrentToken has a third arm for anything else. A registered object
        // implementing neither interface therefore projected to 0, compared unequal to its own
        // token, and was filed 2b -- the SOUND bucket, the one the design says must not move -- when
        // it is 2a, the defect bucket. A census that files its target under "nothing to do here"
        // is worse than one that misses it. This drives exactly that object and requires 2a.
        object plain = new object();
        nuint token = ManagedPointerTokens.CurrentToken(plain);

        if (token == 0)
            Assert.Inconclusive("NOT MEASURED: this object projected to 0, so it cannot be registered");

        ManagedPointerTokens.Register(token, plain);
        Assert.AreSame(plain, ManagedPointerTokens.Resolve(token),
            "the control's premise: the plain object must actually resolve, or the arm is never reached");

        long a2aBefore = Q44RegistryCensus.Snapshot().arm2a;
        long a2bBefore = Q44RegistryCensus.Snapshot().arm2b;

        var _ = (ж<OtherType>)(uintptr)token;

        Assert.IsTrue(Q44RegistryCensus.Snapshot().arm2a > a2aBefore,
            "a resolve at the box's OWN token is offset 0 and must be filed 2a, whatever interfaces the box implements");
        Assert.AreEqual(a2bBefore, Q44RegistryCensus.Snapshot().arm2b,
            "and must NOT be filed 2b -- 2b is the sound bucket, and a defect hidden there is a defect the census reports as absent");

        GC.KeepAlive(plain);
    }

    [TestMethod]
    public void TheCensusWroteAStartBlockAtModuleInit_SoAZeroIsAMeasurement()
    {
        // ⚠ THE OTHER END OF COORD RULING 3 (82c60cec4). The flush closed "the host died before the
        // exit hook"; this closes "the process did nothing". Every other block is written BECAUSE
        // work happened -- the flush at conversion 1, the flush every 250,000, the exit hook -- so a
        // process that armed and converted nothing left NO FILE, and "no file" read identically to
        // the gate being unset, to golib never loading, and to a failed write. i9 confirmed the
        // pipeline keeps no process record to disambiguate them from outside (d6306f2d12), so the
        // disambiguation has to be IN the artifact.
        //
        // This asserts the mechanism in a REAL process rather than through a proxy: golib's module
        // initializer ran before any test in this assembly, so if the start block works at all its
        // line is already on disk and its block is the FIRST one in the file.
        //
        // BOTH candidate paths are checked rather than just OutputPath, because OutputPath re-reads
        // GO2CS_Q44_CENSUS_FILE at call time and sibling controls in this class set and restore it --
        // a control whose verdict depends on class ORDER is the failure this file already carries a
        // lesson about.
        string[] candidates =
        {
            Q44RegistryCensus.OutputPath,
            System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"q44-census-{Environment.ProcessId}.txt"),
        };

        string found = null;
        string[] lines = null;

        foreach (string candidate in candidates)
        {
            if (!System.IO.File.Exists(candidate))
                continue;

            string[] read = System.IO.File.ReadAllLines(candidate);

            if (Array.FindIndex(read, static l => l.StartsWith("Q44CENSUS-START ", StringComparison.Ordinal)) < 0)
                continue;

            found = candidate;
            lines = read;
            break;
        }

        Assert.IsNotNull(found,
            "no census file carrying a Q44CENSUS-START line exists for this process -- the arm-time block " +
            "did not get written, so a zero-conversion row is still indistinguishable from an unarmed one. " +
            "Looked at: " + string.Join(", ", candidates));

        int start = Array.FindIndex(lines, static l => l.StartsWith("Q44CENSUS-START ", StringComparison.Ordinal));
        int firstTotals = Array.FindIndex(lines, static l => l.StartsWith("Q44CENSUS", StringComparison.Ordinal)
                                                          && l.Contains(" conversions=", StringComparison.Ordinal));

        Assert.IsTrue(firstTotals >= 0, "the start block must carry a totals line, or no reader can fold it");
        Assert.IsTrue(firstTotals < start,
            "the totals line must come FIRST in the block -- an existing control requires it greppable at the head, " +
            "and it caught exactly this ordering the first time it ran");

        // The start block's own totals must be the zeros it claims. Reading them from the FIRST block
        // rather than the last is the point: later blocks are cumulative snapshots of real work.
        StringAssert.Contains(lines[firstTotals], "conversions=0",
            "the arm-time block must report zero conversions -- if it reports work, it was not written at arm time");
    }
}
