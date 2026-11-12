// ActionExtensions.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace go.golib;

/// <summary>
/// Defines extension methods for actions.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)] public static class ActionExtensions
{
    /// <summary>
    /// Execute an action on the thread pool after a specified number of milliseconds.
    /// </summary>
    /// <param name="action">The action to be executed.</param>
    /// <param name="delay">The amount of time to wait before execution, in milliseconds.</param>
    /// <param name="cancellationToken">The token used to cancel execution.</param>
    /// <param name="exceptionAction">The action to be performed if an exception is thrown from the action.</param>
    /// <remarks>
    /// End users should attach to the <see cref="TaskScheduler.UnobservedTaskException"/> event to log exceptions if the
    /// <paramref name="exceptionAction"/> is not defined.
    /// </remarks>
    public static void DelayAndExecute(this Action action, int delay, CancellationToken cancellationToken, Action<Exception>? exceptionAction = null)
    {
        new Action<CancellationToken>(_ => action()).DelayAndExecute(delay, cancellationToken, exceptionAction);
    }

    /// <summary>
    /// Execute a cancellable action on the thread pool after a specified number of milliseconds.
    /// </summary>
    /// <param name="action">The action to be executed.</param>
    /// <param name="delay">The amount of time to wait before execution, in milliseconds.</param>
    /// <param name="cancellationToken">The token used to cancel execution.</param>
    /// <param name="exceptionAction">The action to be performed if an exception is thrown from the action.</param>
    /// <remarks>
    /// End users should attach to the <see cref="TaskScheduler.UnobservedTaskException"/> event to log exceptions if the
    /// <paramref name="exceptionAction"/> is not defined.
    /// </remarks>
    public static void DelayAndExecute(this Action<CancellationToken> action, int delay, CancellationToken cancellationToken, Action<Exception>? exceptionAction = null)
    {
        Task.Delay(delay, cancellationToken)
            .ContinueWith(task => action(cancellationToken), cancellationToken)
            .ContinueWith(task =>
                {
                    if (task.Exception is null)
                        return;

                    if (exceptionAction is null)
                        throw task.Exception;

                    exceptionAction(task.Exception);
                },
                cancellationToken,
                TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default);
    }

    /// <summary>
    /// Execute an action on the thread pool after a specified number of milliseconds.
    /// </summary>
    /// <param name="action">The action to be executed.</param>
    /// <param name="delay">The amount of time to wait before execution, in milliseconds.</param>
    /// <param name="exceptionAction">The action to be performed if an exception is thrown from the action.</param>
    /// <returns>
    /// A function to call which will cancel the operation.
    /// Cancel function returns true if <paramref name="action"/> is cancelled in time, false if not.
    /// </returns>
    /// <remarks>
    /// End users should attach to the <see cref="TaskScheduler.UnobservedTaskException"/> event to log exceptions if the
    /// <paramref name="exceptionAction"/> is not defined.
    /// </remarks>
    public static Func<bool> DelayAndExecute(this Action action, int delay, Action<Exception>? exceptionAction = null)
    {
        return new Action<CancellationToken>(_ => action()).DelayAndExecute(delay, exceptionAction);
    }

    /// <summary>
    /// Execute a cancellable action on the thread pool after a specified number of milliseconds.
    /// </summary>
    /// <param name="action">The action to be executed.</param>
    /// <param name="delay">The amount of time to wait before execution, in milliseconds.</param>
    /// <param name="exceptionAction">The action to be performed if an exception is thrown from the action.</param>
    /// <returns>
    /// A function to call which will cancel the operation.
    /// Cancel function returns true if <paramref name="action"/> is cancelled, false if not.
    /// </returns>
    /// <remarks>
    /// End users should attach to the <see cref="TaskScheduler.UnobservedTaskException"/> event to log exceptions if the
    /// <paramref name="exceptionAction"/> is not defined.
    /// </remarks>
    public static Func<bool> DelayAndExecute(this Action<CancellationToken> action, int delay, Action<Exception>? exceptionAction = null)
    {
        // All this state complexity ensures that the token source
        // is not disposed until after the action finishes executing;
        // otherwise, token.ThrowIfCancellationRequested() might unexpectedly
        // throw an ObjectDisposedException if used in the action
        const int NotCancelled = 0;
        const int Cancelling = 1;
        const int Cancelled = 2;
        const int Disposing = 3;

        CancellationTokenSource tokenSource = new();
        CancellationToken token = tokenSource.Token;
        int state = NotCancelled;

        bool cancelFunc()
        {
            // if (state == NotCancelled)
            //     state = Cancelling;
            // else
            //     return false;
            //
            // tokenSource.Cancel();
            //
            // if (state == Cancelling)
            //     state = Cancelled;
            // else if (state == Disposing)
            //     tokenSource.Dispose();
            //
            // return true;

            int previousState = Interlocked.CompareExchange(ref state, Cancelling, NotCancelled);

            if (previousState != NotCancelled)
                return false;

            tokenSource.Cancel();

            previousState = Interlocked.CompareExchange(ref state, Cancelled, Cancelling);

            // If the state changed to Disposing while cancelFunc was cancelling,
            // executeAction will prevent the race condition by not calling
            // tokenSource.Dispose() so it must be called here instead
            if (previousState == Disposing)
                tokenSource.Dispose();

            return true;
        }

        Action<CancellationToken> executeAction = _ =>
        {
            try
            {
                if (!token.IsCancellationRequested)
                    action(token);
            }
            finally
            {
                // int previousState = state;
                // state = Disposing;
                //
                // if (previousState != Cancelling)
                //     tokenSource.Dispose();

                int previousState = Interlocked.Exchange(ref state, Disposing);

                // The Cancelling state is the only state in which it is not
                // safe to dispose on this thread because Cancelling means that
                // cancelFunc is in the process of calling tokenSource.Cancel()
                if (previousState != Cancelling)
                    tokenSource.Dispose();
            }
        };

        executeAction.DelayAndExecute(delay, token, exceptionAction);

        return cancelFunc;
    }
}
