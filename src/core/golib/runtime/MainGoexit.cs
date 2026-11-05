// MainGoexit.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace go.golib;

/// <summary>
/// <c>runtime.Goexit</c> called from the MAIN goroutine, once main's deferred calls have all run.
/// </summary>
/// <remarks>
/// <para>
/// Go ends <c>main</c> without returning and keeps running the other goroutines. When none of them can run any more,
/// its dead-program check (<c>checkdead</c>, runtime/proc.go) ends the process with status 2: <c>fatal error: no
/// goroutines (main called runtime.Goexit) - deadlock!</c> when none is left, or <c>all goroutines are asleep -
/// deadlock!</c> when the ones left are all blocked. Until then another goroutine can end it with <c>os.Exit</c> or a
/// panic.
/// </para>
/// <para>
/// Here the Goexit travels as a <see cref="GoexitException"/> marked <see cref="GoexitException.FromMain"/>: every
/// deferring frame on main's stack catches it, runs its deferred calls and raises it again (<c>GoFrame</c>). Past the
/// outermost one it reaches golib's unhandled-exception backstop on the main thread, or, for a test binary's TestMain,
/// the test host's catch around it, and parks there (<see cref="Park"/>). The main thread then waits for the other
/// user goroutines to end and reports the fatal error Go reports. The other report, "all goroutines are asleep", comes
/// from the goroutines themselves: a goroutine in a forever wait polls the dead-program check, which counts an exited
/// main as blocked for good (<c>Goroutine.MarkMainExited</c>). docs/DESIGN-goexit-main.md.
/// </para>
/// </remarks>
public static class MainGoexit
{
    // The poll period of the park: the wait is for other threads to finish, so its latency is what the fatal report
    // adds after the last goroutine ends.
    private static readonly TimeSpan PollPeriod = TimeSpan.FromMilliseconds(1);

    /// <summary>Go's text for a main that called Goexit with no goroutine left (runtime/proc.go, checkdead).</summary>
    public const string NoGoroutines = "no goroutines (main called runtime.Goexit) - deadlock!";

    /// <summary>
    /// Whether <paramref name="exception"/> is main's Goexit, directly or wrapped by machinery it travelled through
    /// (a faulted Task's <see cref="AggregateException"/>, reflection's invocation wrapper).
    /// </summary>
    public static bool Is(Exception? exception)
    {
        for (Exception? current = exception; current is not null;)
        {
            if (current is GoexitException { FromMain: true })
                return true;

            current = current is AggregateException { InnerExceptions.Count: 1 } aggregate ? aggregate.InnerExceptions[0] : current.InnerException;
        }

        return false;
    }

    /// <summary>
    /// Parks the thread main's Goexit ended on until no other user goroutine is left, then reports Go's fatal error and
    /// exits with status 2. Never returns.
    /// </summary>
    [DoesNotReturn]
    public static void Park()
    {
        Goroutine.MarkMainExited();

        // main's registry entry is never removed, so the other user goroutines are the user count less one.
        while (Goroutine.UserCount > 1 || MainExit.Panicking)
        {
            // A fatal report has begun on another goroutine (a panic, a deadlock, a throw): its owner ends the process,
            // and Go's checkdead never runs beside it.
            if (MainExit.Panicking)
                Thread.Sleep(Timeout.Infinite);

            Thread.Sleep(PollPeriod);
        }

        MainExit.BeginFatal(2);

        // Go writes the line alone: there is no goroutine left to trace.
        string report = $"fatal error: {NoGoroutines}\n";

        try
        {
            Console.Error.Write(report);
            Console.Error.Flush();
        }
        catch (Exception)
        {
        }

        CrashReport.WriteToCrashOutput(report);
        MainExit.FatalReported();
        MainExit.Exit(2);
    }
}
