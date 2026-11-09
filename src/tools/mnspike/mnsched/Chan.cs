// A Go channel for the P2 prototype, with BOTH park paths:
//   - the twin path: RecvAsync / SendAsync return awaitables; a blocked operation stores the async method's
//     continuation in the waiter and returns the carrier; the peer that completes it calls Scheduler.Ready;
//   - the untwinned path: Recv / Send block the THREAD, through the scheduler's handoff (EnterBlocked / ExitBlocked).
// A waiter of either kind can be completed by an operation of either kind.
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MnSched;

public sealed class Chan<T>
{
    private sealed class Waiter
    {
        public T Value = default!;
        public bool Ok;
        public Action? Continuation;            // the twin path
        public ManualResetEventSlim? Event;     // the untwinned path
    }

    private readonly Scheduler m_s;
    private readonly object m_lock = new();
    private readonly int m_cap;
    private readonly Queue<T> m_buf = new();
    private readonly LinkedList<Waiter> m_recvq = new();
    private readonly LinkedList<Waiter> m_sendq = new();
    private bool m_closed;

    public Chan(Scheduler s, int capacity = 0) { m_s = s; m_cap = capacity; }

    private void Complete(Waiter w)
    {
        if (w.Continuation is { } cont) m_s.Ready(cont);
        else w.Event!.Set();
    }

    // Under m_lock. A receive that needs no wait: buffered data, a waiting sender, or a closed channel.
    private bool TryRecvLocked(out T value, out bool ok)
    {
        if (m_buf.Count > 0)
        {
            value = m_buf.Dequeue(); ok = true;
            if (m_sendq.First is { } node) { m_sendq.RemoveFirst(); m_buf.Enqueue(node.Value.Value); Complete(node.Value); }
            return true;
        }

        if (m_sendq.First is { } sender)
        {
            m_sendq.RemoveFirst();
            value = sender.Value.Value; ok = true;
            Complete(sender.Value);
            return true;
        }

        if (m_closed) { value = default!; ok = false; return true; }
        value = default!; ok = false;
        return false;
    }

    // Under m_lock. A send that needs no wait: a waiting receiver, or buffer room.
    private bool TrySendLocked(T value)
    {
        if (m_closed) throw new InvalidOperationException("send on closed channel");

        if (m_recvq.First is { } receiver)
        {
            m_recvq.RemoveFirst();
            receiver.Value.Value = value; receiver.Value.Ok = true;
            Complete(receiver.Value);
            return true;
        }

        if (m_buf.Count < m_cap) { m_buf.Enqueue(value); return true; }
        return false;
    }

    // ---- the untwinned path ------------------------------------------------------------------------------------
    public T Recv()
    {
        Waiter w;
        lock (m_lock)
        {
            if (TryRecvLocked(out T v, out _)) return v;
            w = new Waiter { Event = new ManualResetEventSlim(false) };
            m_recvq.AddLast(w);
        }

        m_s.EnterBlocked();
        w.Event!.Wait();
        m_s.ExitBlocked();
        return w.Value;
    }

    public void Send(T value)
    {
        Waiter w;
        lock (m_lock)
        {
            if (TrySendLocked(value)) return;
            w = new Waiter { Value = value, Event = new ManualResetEventSlim(false) };
            m_sendq.AddLast(w);
        }

        m_s.EnterBlocked();
        w.Event!.Wait();
        m_s.ExitBlocked();
    }

    public void Close()
    {
        lock (m_lock)
        {
            m_closed = true;
            while (m_recvq.First is { } node) { m_recvq.RemoveFirst(); node.Value.Ok = false; Complete(node.Value); }
        }
    }

    // ---- the twin path ---------------------------------------------------------------------------------------------
    public RecvAwaitable RecvAsync() => new(this);

    public SendAwaitable SendAsync(T value) => new(this, value);

    public sealed class RecvAwaitable : ICriticalNotifyCompletion
    {
        private readonly Chan<T> m_c;
        private Waiter? m_w;
        private T m_value = default!;
        private bool m_done;

        internal RecvAwaitable(Chan<T> c) => m_c = c;

        public RecvAwaitable GetAwaiter() => this;

        public bool IsCompleted
        {
            get
            {
                lock (m_c.m_lock) m_done = m_c.TryRecvLocked(out m_value, out _);
                return m_done;
            }
        }

        public T GetResult() => m_done ? m_value : m_w!.Value;

        public void OnCompleted(Action continuation) => UnsafeOnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation)
        {
            lock (m_c.m_lock)
            {
                if (m_c.TryRecvLocked(out m_value, out _)) { m_done = true; m_c.m_s.Ready(continuation); return; }
                m_w = new Waiter { Continuation = continuation };
                m_c.m_recvq.AddLast(m_w);
            }
        }
    }

    public sealed class SendAwaitable : ICriticalNotifyCompletion
    {
        private readonly Chan<T> m_c;
        private readonly T m_value;

        internal SendAwaitable(Chan<T> c, T value) { m_c = c; m_value = value; }

        public SendAwaitable GetAwaiter() => this;

        public bool IsCompleted { get { lock (m_c.m_lock) return m_c.TrySendLocked(m_value); } }

        public void GetResult() { }

        public void OnCompleted(Action continuation) => UnsafeOnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation)
        {
            lock (m_c.m_lock)
            {
                if (m_c.TrySendLocked(m_value)) { m_c.m_s.Ready(continuation); return; }
                m_c.m_sendq.AddLast(new Waiter { Value = m_value, Continuation = continuation });
            }
        }
    }
}

// sync.WaitGroup for the prototype: Done from any path; WaitAsync for twins; Wait for a thread (the handoff applies on
// a carrier, and a plain block elsewhere, as for the program's main thread).
public sealed class WaitGroup
{
    private readonly Scheduler m_s;
    private readonly object m_lock = new();
    private readonly List<Action> m_asyncWaiters = new();
    private readonly ManualResetEventSlim m_zero = new(true);
    private long m_count;

    public WaitGroup(Scheduler s) => m_s = s;

    public void Add(long delta)
    {
        lock (m_lock)
        {
            m_count += delta;
            if (m_count < 0) throw new InvalidOperationException("negative WaitGroup counter");
            if (m_count > 0) { m_zero.Reset(); return; }
            m_zero.Set();
            foreach (Action a in m_asyncWaiters) m_s.Ready(a);
            m_asyncWaiters.Clear();
        }
    }

    public void Done() => Add(-1);

    public bool Wait(TimeSpan timeout)
    {
        if (m_zero.IsSet) return true;
        m_s.EnterBlocked();
        bool ok = m_zero.Wait(timeout);
        m_s.ExitBlocked();
        return ok;
    }

    public WaitAwaitable WaitAsync() => new(this);

    public sealed class WaitAwaitable : ICriticalNotifyCompletion
    {
        private readonly WaitGroup m_wg;
        internal WaitAwaitable(WaitGroup wg) => m_wg = wg;
        public WaitAwaitable GetAwaiter() => this;
        public bool IsCompleted { get { lock (m_wg.m_lock) return m_wg.m_count == 0; } }
        public void GetResult() { }
        public void OnCompleted(Action continuation) => UnsafeOnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation)
        {
            lock (m_wg.m_lock)
            {
                if (m_wg.m_count == 0) { m_wg.m_s.Ready(continuation); return; }
                m_wg.m_asyncWaiters.Add(continuation);
            }
        }
    }
}
