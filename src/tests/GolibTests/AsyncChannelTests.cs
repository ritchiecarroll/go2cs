using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

/// <summary>
/// <c>channel&lt;T&gt;.ReceiveAsync</c> / <c>SendAsync</c>: the awaitable channel operations a C# caller (a UI
/// thread, an ASP.NET request) uses instead of the thread-blocking <c>Receive</c> / <c>Send</c>.
/// </summary>
/// <remarks>
/// They must keep Go's channel semantics exactly, because they share the channel's wait queues with Go
/// code: an unbuffered channel is a rendezvous (neither side completes alone), a buffered one drains before
/// a close reads as <c>(zero, false)</c>, a send on a closed channel panics, waiters are served first in,
/// first out, and a nil channel never becomes ready. Cancellation is all-or-nothing: a canceled operation
/// did not happen (no value taken, none delivered), and one that happened is never reported canceled.
/// And a pending operation holds no thread.
/// </remarks>
[TestClass]
public class AsyncChannelTests
{
    private static readonly TimeSpan s_settle = TimeSpan.FromMilliseconds(100);
    private const int TimeoutMs = 10000;

    private static async Task<T> Within<T>(Task<T> task)
    {
        Task first = await Task.WhenAny(task, Task.Delay(TimeoutMs));
        Assert.AreSame(task, first, "the awaited channel operation did not complete");
        return await task;
    }

    private static async Task Within(Task task)
    {
        Task first = await Task.WhenAny(task, Task.Delay(TimeoutMs));
        Assert.AreSame(task, first, "the awaited channel operation did not complete");
        await task;
    }

    [TestMethod]
    public async Task ReceiveAsyncWaitsForASendOnAnUnbufferedChannel()
    {
        channel<int> ch = new(0);
        Task<int> received = ch.ReceiveAsync();

        await Task.Delay(s_settle);
        Assert.IsFalse(received.IsCompleted, "a receive on an empty unbuffered channel completed with no sender");

        Task sender = Task.Run(() => ch.Send(42));

        Assert.AreEqual(42, await Within(received));
        await Within(sender);
    }

    [TestMethod]
    public async Task SendAsyncIsARendezvousOnAnUnbufferedChannel()
    {
        channel<int> ch = new(0);
        Task sent = ch.SendAsync(7);

        await Task.Delay(s_settle);
        Assert.IsFalse(sent.IsCompleted, "a send on an unbuffered channel completed with no receiver");

        Assert.AreEqual(7, ch.Receive());
        await Within(sent);
    }

    [TestMethod]
    public async Task SendAsyncCompletesAtOnceWhenTheBufferHasRoom()
    {
        channel<int> ch = new(1);

        Task sent = ch.SendAsync(5);

        Assert.IsTrue(sent.IsCompletedSuccessfully, "a send with buffer room did not complete at once");
        Assert.IsTrue(ch.TryReceive(out int value) && value == 5);

        Task blocked = ch.SendAsync(6);
        Task blockedToo = ch.SendAsync(7);
        Assert.IsTrue(blocked.IsCompletedSuccessfully, "the buffer's one slot was free again");
        await Task.Delay(s_settle);
        Assert.IsFalse(blockedToo.IsCompleted, "a send on a full buffer completed");

        Assert.AreEqual(6, ch.Receive());
        await Within(blockedToo);
        Assert.AreEqual(7, ch.Receive());
    }

    [TestMethod]
    public async Task AClosedChannelDrainsThenReadsZeroAndNotOk()
    {
        channel<int> ch = new(2);
        ch.Send(1);
        ch.Close();

        (int value, bool ok) = await Within(ch.ReceiveAsync(true));
        Assert.IsTrue(ok && value == 1, "a closed channel's buffered value comes first, with ok");

        (value, ok) = await Within(ch.ReceiveAsync(true));
        Assert.IsTrue(!ok && value == 0, "a closed, drained channel reads (zero, false)");

        Assert.AreEqual(0, await Within(ch.ReceiveAsync()));
    }

    [TestMethod]
    public async Task ClosingWakesAPendingReceiveWithZeroAndNotOk()
    {
        channel<@string> ch = new(0);
        Task<(@string val, bool ok)> received = ch.ReceiveAsync(true);

        await Task.Delay(s_settle);
        ch.Close();

        (@string value, bool ok) = await Within(received);
        Assert.IsFalse(ok);
        Assert.IsTrue(value == "", "the zero value of a Go string is \"\"");
    }

    [TestMethod]
    public async Task ASendOnAClosedChannelPanics()
    {
        channel<int> closed = new(1);
        closed.Close();

        PanicException now = await Assert.ThrowsExceptionAsync<PanicException>(() => closed.SendAsync(1));
        StringAssert.Contains(now.Message, "send on closed channel");

        channel<int> ch = new(0);
        Task pending = ch.SendAsync(1);
        await Task.Delay(s_settle);
        ch.Close();

        PanicException later = await Assert.ThrowsExceptionAsync<PanicException>(() => Within(pending));
        StringAssert.Contains(later.Message, "send on closed channel");
    }

    [TestMethod]
    public async Task PendingReceiversAreServedFirstInFirstOut()
    {
        channel<int> ch = new(0);
        Task<int> first = ch.ReceiveAsync();
        await Task.Delay(s_settle);
        Task<int> second = ch.ReceiveAsync();
        await Task.Delay(s_settle);

        ch.Send(1);
        ch.Send(2);

        Assert.AreEqual(1, await Within(first));
        Assert.AreEqual(2, await Within(second));
    }

    [TestMethod]
    public async Task AGoroutineAndAnAsyncCallerMeetOnOneChannel()
    {
        channel<int> requests = new(0);
        channel<int> replies = new(0);

        Goroutine.Start(() =>
        {
            int request = requests.Receive();
            replies.Send(request * 2);
        });

        await Within(requests.SendAsync(21));
        Assert.AreEqual(42, await Within(replies.ReceiveAsync()));
    }

    [TestMethod]
    public async Task ACanceledReceiveTakesNothing()
    {
        channel<int> ch = new(0);
        using CancellationTokenSource cts = new();
        Task<int> received = ch.ReceiveAsync(cts.Token);

        await Task.Delay(s_settle);
        cts.Cancel();

        await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => Within(received));

        // The canceled receiver left the queue: an unbuffered send now finds no one to hand to.
        Assert.IsFalse(ch.TrySend(9), "a canceled receive was still waiting on the channel");
    }

    [TestMethod]
    public async Task ACanceledSendDeliversNothing()
    {
        channel<int> ch = new(0);
        using CancellationTokenSource cts = new();
        Task sent = ch.SendAsync(9, cts.Token);

        await Task.Delay(s_settle);
        cts.Cancel();

        await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => Within(sent));
        Assert.IsFalse(ch.TryReceive(out _), "a canceled send's value reached the channel");
    }

    [TestMethod]
    public async Task AnAlreadyCanceledTokenPerformsNoOperation()
    {
        channel<int> ch = new(1);
        ch.Send(3);
        using CancellationTokenSource cts = new();
        cts.Cancel();

        await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => ch.ReceiveAsync(cts.Token));
        Assert.IsTrue(ch.TryReceive(out int value) && value == 3, "a receive canceled before it began took the value");

        await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => ch.SendAsync(4, cts.Token));
        Assert.IsFalse(ch.TryReceive(out _), "a send canceled before it began delivered its value");
    }

    // A cancellation that races deliveries must never lose or duplicate a value: every send either
    // completes and its value is received exactly once, or is canceled and its value never appears.
    [TestMethod]
    public async Task CancellationRacingDeliveryNeverLosesAValue()
    {
        const int Rounds = 2000;
        channel<int> ch = new(0);
        int delivered = 0;
        int received = 0;

        Task receiver = Task.Run(() =>
        {
            while (true)
            {
                (int value, bool ok) = ch.Receive(true);

                if (!ok)
                    return;

                received += value;
            }
        });

        int canceled = 0;

        for (int i = 0; i < Rounds; i++)
        {
            using CancellationTokenSource cts = new();
            Task sent = ch.SendAsync(1, cts.Token);

            // A varying head start for the receiver, so each claimant wins some rounds.
            Thread.SpinWait(i % 64 * 8);
            cts.Cancel();

            try
            {
                await Within(sent);
                delivered++;
            }
            catch (OperationCanceledException)
            {
                canceled++;
            }
        }

        ch.Close();
        await Within(receiver);

        Assert.AreEqual(delivered, received, $"a value was lost or duplicated by a racing cancellation ({delivered} delivered, {canceled} canceled)");
        Assert.IsTrue(delivered > 0 && canceled > 0, $"the race was not exercised: {delivered} delivered, {canceled} canceled");
    }

    [TestMethod]
    public async Task ANilChannelNeverBecomesReady()
    {
        channel<int> nilChannel = default;
        using CancellationTokenSource cts = new();
        Task<int> received = nilChannel.ReceiveAsync(cts.Token);
        Task sent = nilChannel.SendAsync(1, cts.Token);

        await Task.Delay(s_settle);
        Assert.IsFalse(received.IsCompleted || sent.IsCompleted, "an operation on a nil channel completed");

        cts.Cancel();
        await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => Within(received));
        await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => Within(sent));
    }

    // A pending operation is a queued waiter and a task, not a parked thread: a dedicated thread per
    // wait would add one thread per pending receive here.
    [TestMethod]
    public async Task PendingOperationsHoldNoThreads()
    {
        const int Pending = 500;
        channel<int>[] channels = new channel<int>[Pending];
        Task<int>[] receives = new Task<int>[Pending];
        int threadsBefore = Process.GetCurrentProcess().Threads.Count;

        for (int i = 0; i < Pending; i++)
        {
            channels[i] = new channel<int>(0);
            receives[i] = channels[i].ReceiveAsync();
        }

        await Task.Delay(s_settle);
        int threadsAfter = Process.GetCurrentProcess().Threads.Count;

        for (int i = 0; i < Pending; i++)
            channels[i].Close();

        await Within(Task.WhenAll(receives));
        Assert.IsTrue(threadsAfter - threadsBefore < Pending / 5, $"{Pending} pending receives grew the process from {threadsBefore} to {threadsAfter} threads");
    }
}
