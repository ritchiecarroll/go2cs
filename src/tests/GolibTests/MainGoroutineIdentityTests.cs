using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using go.testing_runtime;

namespace GolibTests;

// The converted-test host runs TestMain AS the main goroutine, and the host's own waiting thread is
// not a second one.
//
// Go's testing.(*M).Run -- and the TestMain that calls it -- execute on the main goroutine; the
// package deadline is a timer (testing.go's startAlarm). The host inverts the threads: TestHost.Run
// parks the process's main thread on the deadline and hands the run to a pool thread. Until
// 2026-09-04 that pool thread carried no goroutine identity at all, so TestMain's own traceback was
// headed `goroutine 0` (an id Go never mints) while the REAL goroutine 1 -- registered by golib's
// module initializer on the main thread, which then runs no Go code -- was rendered by every
// runtime.Stack(all) as a frameless foreign block. net/http's goroutine-leak check
// (main_test.go's interestingGoroutines: keep-unless-contains over each block minus its header) can
// drop a block only by its text, a frameless block has none, so the host counted ITSELF as a leaked
// goroutine and TestMain exited 1 over a 1,345/1,345 record. Measured on the row's own TestMain,
// Release + tiered, Linux; the fix is Goroutine.EnterAsMain adopted by TestHost.RunTests.
//
// Two axes, each with its own arm: the HOST-DRIVEN shape (the row's), and the golib primitive's own
// contract (scoped, registers nothing, inert on a thread that already has an identity).
[TestClass]
public class MainGoroutineIdentityTests
{
    private static string CaptureStack(bool all)
    {
        byte[] storage = new byte[256 * 1024];
        slice<byte> buf = new(storage);
        nint written = runtime_package.Stack(buf, all);

        return Encoding.UTF8.GetString(storage, 0, (int)written);
    }

    private static string[] Blocks(string dump) =>
        dump.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);

    private static string Header(string block)
    {
        int newline = block.IndexOf('\n');
        return newline < 0 ? block : block[..newline];
    }

    // Go's filter, transcribed from net/http/main_test.go: a block is dropped when the text BELOW
    // its header contains one of these; the main goroutine is dropped only because its own stack
    // carries `interestingGoroutines`.
    private static readonly string[] GoLeakFilter =
    [
        "testing.(*M).before.func1",
        "os/signal.signal_recv",
        "created by net.startServer",
        "created by testing.RunTests",
        "closeWriteAndWait",
        "testing.Main(",
        "runtime.goexit",
        "created by runtime.gc",
        "interestingGoroutines",
        "runtime.MHeap_Scavenger",
    ];

    // The HEADERS of the blocks Go's filter would keep. Headers are retained (Go discards them) so
    // an assertion can name WHICH goroutine survived rather than only that one did.
    private static string[] SurvivingHeaders(string dump)
    {
        return Blocks(dump)
            .Where(block =>
            {
                int newline = block.IndexOf('\n');
                string stack = newline < 0 ? "" : block[(newline + 1)..].Trim();
                return stack.Length > 0 && !GoLeakFilter.Any(needle => stack.Contains(needle, StringComparison.Ordinal));
            })
            .Select(Header)
            .ToArray();
    }

    // Named EXACTLY as Go's helper and never inlined: the filter drops the calling goroutine by
    // matching `interestingGoroutines` against its rendered frame names, and this method is that
    // frame here.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string interestingGoroutines() => CaptureStack(all: true);

    // THE GUARD -- the row's shape, driven through the real host. Three assertions, each naming its
    // own defect: (a) TestMain's own block is headed `goroutine 1` (it runs AS the main goroutine,
    // not as an identity-less `goroutine 0`); (b) exactly one block in the whole dump is headed
    // `goroutine 1` (the parked host thread is not rendered as a second, frameless main); (c) Go's
    // transcribed filter keeps no block headed `goroutine 1`. (c) is restricted to the main goroutine
    // deliberately: a goroutine leaked by a sibling test class in this process has another id and is
    // that class's defect, not this guard's subject.
    //
    // Positive control (measured at the cut): with the EnterAsMain line removed from
    // TestHost.RunTests, (a) reads `goroutine 0 [running]:` and (b) finds the placeholder block --
    // the pre-fix shape exactly.
    [TestMethod]
    public void TestMainRunsAsTheMainGoroutineAndTheHostThreadIsNotASecondOne()
    {
        string? dump = null;
        TestRegistry registry = new("guard", []);

        registry.SetTestMain(_ => dump = interestingGoroutines());

        TestHost.Run(registry, []);

        Assert.IsNotNull(dump, "TestMain never ran, so nothing was measured");

        Console.WriteLine(dump);

        string[] blocks = Blocks(dump!);

        Assert.IsTrue(blocks.Length > 0, "runtime.Stack(all) rendered nothing");

        // (a) the calling block IS the main goroutine.
        Assert.AreEqual("goroutine 1 [running]:", Header(blocks[0]),
            "TestMain did not run as the main goroutine -- the host handed the run to a thread with no goroutine identity");

        // (b) the host's waiting thread is not a second main.
        string[] mainHeaded = blocks.Where(block => Header(block).StartsWith("goroutine 1 [", StringComparison.Ordinal)).ToArray();

        Assert.AreEqual(1, mainHeaded.Length,
            "the main goroutine was rendered more than once -- the parked host thread surfaced as a frameless second `goroutine 1` block:\n" +
            string.Join("\n---\n", mainHeaded));

        // (c) the row's own algorithm keeps no main goroutine.
        string[] survivors = SurvivingHeaders(dump!);

        Assert.IsFalse(survivors.Any(header => header.StartsWith("goroutine 1 [", StringComparison.Ordinal)),
            "Go's leak filter kept the main goroutine; surviving headers:\n" + string.Join("\n", survivors));
    }

    // The primitive's contract: adopting the main identity mints nothing, is not "on a goroutine"
    // (runtime.Goexit stays gated exactly as on the registering thread), and ends with the scope.
    [TestMethod]
    public void AdoptingTheMainIdentityIsScopedAndRegistersNothing() => AssertAdoptionIsScoped(afterBaseline: null);

    // An earlier test's goroutine retiring inside this one's window (StragglerStaging), staged.
    [TestMethod]
    public void AdoptionStaysScopedWhileAnEarlierGoroutineRetires()
    {
        for (int i = 0; i < StragglerStaging.Iterations; i++)
            AssertAdoptionIsScoped(StragglerStaging.Stage());
    }

    private static void AssertAdoptionIsScoped(Action? afterBaseline)
    {
        Goroutine? before = Goroutine.Current;
        int count = Goroutine.Count;
        afterBaseline?.Invoke();

        using (Goroutine.EnterAsMain())
        {
            Goroutine? current = Goroutine.Current;

            Assert.IsNotNull(current, "no identity inside the scope");
            Assert.IsTrue(current!.IsMain, "the identity inside the scope is not the main goroutine");
            Assert.IsFalse(Goroutine.OnGoroutine, "the main goroutine reads as being on a goroutine -- the Goexit gate would open");
            Assert.AreEqual(count, Goroutine.Count, "adopting the main identity minted a goroutine");
        }

        Assert.AreSame(before, Goroutine.Current, "the adopted identity outlived its scope");
        Assert.AreEqual(count, Goroutine.Count, "disposing the adoption scope changed the goroutine count");
    }

    // A thread already running a goroutine keeps that identity: EnterAsMain nests like Enter, so a
    // host calling it from inside a goroutine body can neither re-label the goroutine as main nor
    // retire it when the inert scope ends. On a dedicated thread so the MSTest thread's own state
    // is not the variable.
    [TestMethod]
    public void AThreadAlreadyOnAGoroutineKeepsItsIdentity() => AssertAGoroutineKeepsItsIdentity(afterBaseline: null);

    // An earlier test's goroutine retiring inside this one's window (StragglerStaging), staged.
    [TestMethod]
    public void AGoroutineKeepsItsIdentityWhileAnEarlierGoroutineRetires()
    {
        for (int i = 0; i < StragglerStaging.Iterations; i++)
            AssertAGoroutineKeepsItsIdentity(StragglerStaging.Stage());
    }

    private static void AssertAGoroutineKeepsItsIdentity(Action? afterBaseline)
    {
        Exception? failure = null;

        Thread thread = new(() =>
        {
            try
            {
                using Goroutine.Scope goroutine = Goroutine.Enter();

                Goroutine? mine = Goroutine.Current;
                int count = Goroutine.Count;
                afterBaseline?.Invoke();

                Assert.IsNotNull(mine);
                Assert.IsFalse(mine!.IsMain);

                using (Goroutine.EnterAsMain())
                {
                    Assert.AreSame(mine, Goroutine.Current, "EnterAsMain re-labeled a running goroutine as main");
                    Assert.AreEqual(count, Goroutine.Count);
                }

                Assert.AreSame(mine, Goroutine.Current, "the inert scope retired the goroutine it never minted");
                Assert.AreEqual(count, Goroutine.Count);
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.Start();
        thread.Join();

        if (failure is not null)
            Assert.Fail(failure.ToString());
    }
}
