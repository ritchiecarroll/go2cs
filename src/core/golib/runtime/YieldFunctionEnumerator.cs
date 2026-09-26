// YieldFunctionEnumerator.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace go.golib;

/// <summary>
/// Go's range-over-func (<c>for v := range seq</c>) as a C# <c>foreach</c>: a PUSH sequence function
/// adapted into a PULL enumerator over golib's <see cref="Coro"/>.
/// </summary>
/// <remarks>
/// <para>
/// Go runs <c>seq</c> as an ordinary call on the ranging goroutine, with the loop body as its
/// <c>yield</c>. The conversion keeps the loop a <c>foreach</c> (so <c>break</c>, <c>continue</c> and
/// <c>return</c> stay C#'s own), which needs <c>seq</c> on a second stack. That stack is a
/// <see cref="Coro"/> -- the primitive <c>iter.Pull</c> stands on -- so <c>seq</c> runs on a real
/// goroutine that joins the ranging goroutine's synctest bubble and parks with Go's accounting when it
/// hands a value over. A thread-pool task stood here before: <c>seq</c> ran outside any bubble and any
/// goroutine identity (its <c>time.Now</c> read the real clock inside <c>synctest.Run</c> --
/// internal/synctest's TestIteratorPush), and a panic in <c>seq</c> faulted the task unobserved while
/// the loop simply ended.
/// </para>
/// <para>
/// A PANIC or a <c>runtime.Goexit</c> in <c>seq</c> is captured on the coro side and rethrown on the
/// ranging side, type and all -- so the ranging function's defers can recover the panic, and a Goexit
/// ends the ranging goroutine -- which is what Go gives it by running <c>seq</c> on that goroutine,
/// and the same hand-back <c>iter.Pull</c> performs with its <c>panicValue</c>.
/// </para>
/// <para>
/// An EARLY EXIT (<c>break</c>, <c>return</c>, or a panic in the loop body) disposes the enumerator,
/// which switches back in once with <c>yield</c> answering false, so <c>seq</c> unwinds to completion
/// (its defers run) and the coro's thread ends. A <c>seq</c> that keeps yielding after <c>false</c>
/// gets Go's panic for it, raised where the loop ends.
/// </para>
/// <para>
/// Residual, stated: <c>seq</c> runs on the coro's goroutine rather than on the ranging goroutine
/// itself, so its goroutine identity differs and <c>runtime.NumGoroutine</c> counts one more while the
/// loop is live -- Go's own <c>iter.Pull</c> has the same property; a converter lowering of the loop
/// into a direct call of <c>seq</c> is what would close it.
/// </para>
/// </remarks>
internal class YieldFunctionEnumerable<T>(Action<Func<T, bool>> enumerator) : IEnumerable<T>
{
    public IEnumerator<T> GetEnumerator()
    {
        return new YieldFunctionEnumerator(enumerator);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private sealed class YieldFunctionEnumerator(Action<Func<T, bool>> seq) : IEnumerator<T>
    {
        // Every field is written by one side and read by the other only across a Coro switch, whose
        // semaphore handoff orders the accesses; exactly one side runs at any time.
        private Coro? m_coro;
        private bool m_hasValue;
        private bool m_stopped;
        private bool m_done;
        private ExceptionDispatchInfo? m_failure;

        public T Current { get; private set; } = default!;

        object IEnumerator.Current => Current!;

        public bool MoveNext()
        {
            if (m_done)
                return false;

            // Created on the first pull, from the ranging goroutine, so the coro joins its bubble.
            m_coro ??= Coro.Start(run);
            m_hasValue = false;
            m_coro.Switch();
            rethrowFailure();

            return m_hasValue;
        }

        // The coro body: seq, to completion or failure.
        private void run()
        {
            try
            {
                seq(yield);
            }
            catch (Exception ex)
            {
                m_failure = ExceptionDispatchInfo.Capture(ex);
            }
            finally
            {
                m_done = true;
            }
        }

        // seq's yield: hand the value to the ranging side and wait for the next pull, or for the loop's
        // end, which answers false.
        private bool yield(T value)
        {
            if (m_stopped)
                throw RuntimeErrorPanic.RangeFunctionContinued();

            Current = value;
            m_hasValue = true;
            m_coro!.Switch();

            return !m_stopped;
        }

        private void rethrowFailure()
        {
            if (m_failure is not { } failure)
                return;

            m_failure = null;
            failure.Throw();
        }

        public void Reset()
        {
            throw new NotSupportedException("Reset is not supported for this enumerator");
        }

        public void Dispose()
        {
            // Never pulled, or already finished: seq is not running, nothing to unwind.
            if (m_coro is null || m_done)
                return;

            // The loop ended early: resume seq once with yield answering false, so it unwinds.
            m_stopped = true;
            m_coro.Switch();
            rethrowFailure();
        }
    }
}
