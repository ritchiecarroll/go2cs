using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

// A foreign goroutine's traceback block names its CREATOR -- Go's `created by <func> in goroutine N`
// line -- beneath the honest frameless placeholder.
//
// runtime.Stack(all) can walk only the calling thread's frames, so every other goroutine renders as a
// header plus `[stack unavailable ...]`. That block carried NO text a program could match: net/http's
// goroutine-leak filter (main_test.go's interestingGoroutines) keeps a block unless its text contains
// one of eleven substrings, three of which are `created by <func>` lines, so an otherwise-droppable
// goroutine survived every round -- and an operator reading the dump could not tell WHICH `go`
// statement left it behind (the net/http row's `goroutine 4 [chan receive]`, 2026-09-04, parked from
// the first test to the last with nothing to name it). The registry now records the creator at
// Goroutine.Start -- the function executing the `go` statement, located by identity above golib's
// own launcher frames -- and the parent goroutine's id, and Stack(all) prints them in Go's shape.
// The position line Go prints beneath it is omitted on purpose: it needs a file-info capture on
// every `go` statement, and nothing matches on it.
[TestClass]
public class GoroutineCreatorTests
{
    private static string CaptureStack(bool all)
    {
        byte[] storage = new byte[256 * 1024];
        slice<byte> buf = new(storage);
        nint written = runtime_package.Stack(buf, all);

        return Encoding.UTF8.GetString(storage, 0, (int)written);
    }

    // Never inlined: this frame IS the creator the block must name, so the JIT must not fold it
    // into the test method (which would make the assertion read the wrong -- though still
    // truthful -- name).
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void StartParkedGoroutine(channel<int> park) => goǃ(() => park.Receive());

    private static string? ForeignBlock(string dump, string status, string createdBy) =>
        dump.Split("\n\n").Skip(1).FirstOrDefault(b => b.StartsWith("goroutine ", StringComparison.Ordinal) && b.Contains($"[{status}]", StringComparison.Ordinal) &&
                                                         b.Split('\n').Contains(createdBy));

    // THE GUARD. The parked goroutine's block carries `created by GolibTests.GoroutineCreatorTests.
    // StartParkedGoroutine in goroutine <this thread's id>` -- the launcher rungs (`goǃ`) and
    // Goroutine.Start never name themselves, and the creator is the function that executed the `go`.
    [TestMethod]
    public void AForeignGoroutineBlockNamesItsCreator()
    {
        channel<int> park = new(0);

        using (Goroutine.Enter())
            AssertParkedGoroutineNamesItsCreator(park, Goroutine.Current!.Id);

        park.Close();
    }

    // The same guard with other goroutines already parked, as in a full GolibTests run, where earlier
    // tests leave goroutines behind: one parked on a chan send and one parked on a chan receive that a
    // different function created. The guard must find THIS test's goroutine, not the first parked one.
    [TestMethod]
    public void AForeignGoroutineBlockNamesItsCreatorAmongOtherParkedGoroutines()
    {
        channel<int> park = new(0), send = new(0), receive = new(0);

        using (Goroutine.Enter())
        {
            long myId = Goroutine.Current!.Id;

            PlantParkedGoroutines(send, receive);

            for (int i = 0; i < 400 && Goroutine.Snapshot().Count(g => g.ParentId == myId && g.State == GoroutineState.Parked) < 2; i++)
                System.Threading.Thread.Sleep(5);

            Assert.AreEqual(2, Goroutine.Snapshot().Count(g => g.ParentId == myId && g.State == GoroutineState.Parked), "the planted goroutines never parked");

            try
            {
                AssertParkedGoroutineNamesItsCreator(park, myId);
            }
            finally
            {
                park.Close();
                send.Receive(); // releases the sender; closing would panic it
                receive.Close();
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void PlantParkedGoroutines(channel<int> send, channel<int> receive)
    {
        goǃ(() => send.Send(1));
        goǃ(() => receive.Receive());
    }

    // Waits for THIS call's goroutine -- created here, by StartParkedGoroutine, on goroutine myId -- to
    // park, then reads the block whose created-by line is that one. Neither step may settle for another
    // parked goroutine: a full run leaves goroutines from earlier tests parked.
    private static void AssertParkedGoroutineNamesItsCreator(channel<int> park, long myId)
    {
        string createdBy = $"created by GolibTests.GoroutineCreatorTests.{nameof(StartParkedGoroutine)} in goroutine {myId}";

        StartParkedGoroutine(park);

        bool Mine(Goroutine g) => g.ParentId == myId && g.Creator?.Name == nameof(StartParkedGoroutine);

        for (int i = 0; i < 400 && !Goroutine.Snapshot().Any(g => Mine(g) && g.State == GoroutineState.Parked); i++)
            System.Threading.Thread.Sleep(5);

        Assert.IsTrue(Goroutine.Snapshot().Any(g => Mine(g) && g.State == GoroutineState.Parked), "the goroutine this test started never parked");

        string dump = CaptureStack(all: true);
        string? block = ForeignBlock(dump, "chan receive", createdBy);

        Console.WriteLine(dump);

        Assert.IsNotNull(block, $"no foreign block with a `chan receive` header and `{createdBy}` was rendered:\n{dump}");

        string[] lines = block!.Split('\n');

        Assert.IsTrue(lines.Length >= 3, $"expected header, placeholder and created-by lines:\n{block}");
        Assert.IsTrue(lines[1].StartsWith("[stack unavailable", StringComparison.Ordinal), $"the placeholder must stay first beneath the header:\n{block}");
        Assert.AreEqual(createdBy, lines[2],
            "the created-by line must name the function that executed the `go` statement and the goroutine it ran on");
        Assert.IsFalse(block.Contains("goǃ", StringComparison.Ordinal) || block.Contains("Goroutine.Start", StringComparison.Ordinal),
            "a launcher frame was named as the creator -- the identity walk stopped too early:\n" + block);
    }

    // The main goroutine carries no created-by line, as in Go (printcreatedby skips goid 1), and
    // neither does a thread a host entered directly -- there was no `go` statement.
    [TestMethod]
    public void TheMainGoroutineAndAHostEnteredThreadCarryNoCreator()
    {
        string dump;

        using (Goroutine.Enter())
            dump = CaptureStack(all: true);

        string? main = dump.Split("\n\n").FirstOrDefault(b => b.StartsWith("goroutine 1 [", StringComparison.Ordinal));

        if (main is not null)
            Assert.IsFalse(main.Contains("created by", StringComparison.Ordinal), "the main goroutine must not carry a created-by line:\n" + main);

        Goroutine? entered = null;
        string? enteredDump = null;
        System.Threading.Thread thread = new(() =>
        {
            using (Goroutine.Enter())
            {
                entered = Goroutine.Current;
                System.Threading.Thread.Sleep(50);
            }
        });
        thread.Start();
        for (int i = 0; i < 200 && entered is null; i++)
            System.Threading.Thread.Sleep(1);
        enteredDump = CaptureStack(all: true);
        thread.Join();

        Assert.IsNotNull(entered, "the host-entered thread never registered");
        Assert.IsNull(entered!.Creator, "a host-entered thread recorded a creator it never had");
    }
}
