#nullable enable

using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;
using isync = go.@internal.sync_package;

namespace GolibTests;

/// <summary>
/// Go's other half of the conditional handoff: a waiter that has already waited parks at the HEAD of its
/// word's queue (semacquire1's lifo, which sync.Mutex's lockSlow passes as queueLifo = waitStartTime != 0).
/// </summary>
/// <remarks>
/// <para>
/// In starvation mode Unlock releases with handoff, and Go hands the permit to the first waiter only if it can
/// still take it (semrelease1: <c>if handoff &amp;&amp; cansemacquire(addr)</c>). A newcomer's easy case
/// (semacquire1's first <c>cansemacquire</c>) can take the permit in between. The first waiter is then only
/// readied, fails to acquire, and queues again. Because it has waited before, it queues with lifo, at the head,
/// so it still wins the next round. With a FIFO-only queue it went behind every later waiter, and the longest
/// waiter could be passed over once per round.
/// </para>
/// <para>
/// The race is made deterministic. The test holds the word's bucket lock (read from the private table by
/// reflection) while a releaser has added its permit and waits for that lock; the test thread takes the permit
/// as the newcomer; then it lets the releaser go. The waiters park through internal/sync's own
/// <c>runtime_SemacquireMutex</c>, the path sync.Mutex takes, so the arm also checks that the companion passes
/// lifo on.
/// </para>
/// </remarks>
[TestClass]
public class RuntimeSemaphoreLifoTests
{
    private static readonly TimeSpan Bound = TimeSpan.FromSeconds(30);

    [TestMethod]
    public void TheLongestWaiterWinsTheRoundAfterANewcomerStealsTheHandoff()
    {
        heap<uint32>(out var word);
        int[] order = [-1, -1, -1];
        int winners = 0;
        Thread[] waiters = new Thread[3];

        // W0 has waited before (lifo); W1 and W2 are first-time waiters (not lifo). Each parks before the next
        // starts, so the queue is W0, W1, W2 on any implementation.
        for (int k = 0; k < waiters.Length; k++)
        {
            int id = k;
            bool lifo = k == 0;

            waiters[k] = new Thread(() =>
            {
                SemacquireMutex(word, lifo);
                order[Interlocked.Increment(ref winners) - 1] = id;
            }) { IsBackground = true, Name = $"lifo-waiter-{id}" };

            waiters[k].Start();
            WaitFor(() => RuntimeSemaphore.Waiters(word) == k + 1, $"W{id} never parked");
        }

        object bucket = SemaTable()[word]!;
        Thread releaser = new(() => RuntimeSemaphore.Release(word, handoff: true)) { IsBackground = true, Name = "lifo-releaser" };

        Monitor.Enter(bucket);

        try
        {
            releaser.Start();

            // The releaser adds its permit before it looks for the bucket, then waits for the lock held here.
            WaitFor(() => Volatile.Read(ref word.Value) == 1, "the releaser never added its permit");

            // The newcomer: an easy-case acquire, which needs no bucket lock, takes the permit.
            SemacquireMutex(word, lifo: false);
        }
        finally
        {
            Monitor.Exit(bucket);
        }

        Assert.IsTrue(releaser.Join(Bound), "the releaser did not finish");

        // The releaser found no permit to hand over, so it only readied W0. W0 lost, and queues again.
        WaitFor(() => RuntimeSemaphore.Waiters(word) == 3, "W0 did not queue again after losing the permit");
        Assert.AreEqual(0, Volatile.Read(ref winners), "no waiter may have acquired: the newcomer holds the permit");

        // The newcomer unlocks in starvation mode: the next round.
        RuntimeSemaphore.Release(word, handoff: true);
        WaitFor(() => Volatile.Read(ref winners) == 1, "no waiter won the next round");

        int first = order[0];

        // Let the other two go, so no thread is left parked.
        RuntimeSemaphore.Release(word, handoff: false);
        RuntimeSemaphore.Release(word, handoff: false);

        foreach (Thread waiter in waiters)
            Assert.IsTrue(waiter.Join(Bound), $"{waiter.Name} was not woken at the end");

        Assert.AreEqual(0, first,
            $"W{first} won the round after the steal, ahead of W0, the longest waiter: a waiter that has waited before " +
            "must queue again at the head (Go's semacquire1 lifo), or a starving Mutex passes it over once per round");
        Assert.AreEqual(0u, word.Value, "every permit released was acquired");
    }

    private static void SemacquireMutex(ж<uint32> word, bool lifo) =>
        s_semacquireMutex.Invoke(null, [word, lifo, (nint)0]);

    private static readonly MethodInfo s_semacquireMutex =
        typeof(isync).GetMethod("runtime_SemacquireMutex", BindingFlags.NonPublic | BindingFlags.Static)!;

    private static void WaitFor(Func<bool> condition, string failure)
    {
        SpinWait spin = default;
        DateTime deadline = DateTime.UtcNow + Bound;

        while (!condition())
        {
            Assert.IsTrue(DateTime.UtcNow < deadline, failure);
            spin.SpinOnce();
        }
    }

    private static IDictionary SemaTable() =>
        (IDictionary)typeof(RuntimeSemaphore).GetField("semaTable", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
}
