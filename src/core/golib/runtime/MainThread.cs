// MainThread.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Threading;

namespace go.golib;

/// <summary>
/// Runs a converted program's <c>main</c> as Go's main goroutine: on a thread whose stack reserves what Go's main
/// goroutine may grow to.
/// </summary>
/// <remarks>
/// <para>
/// Go's goroutine stacks grow, to 1 GB on a 64-bit platform (runtime.main sets <c>maxstacksize</c>), so a deep
/// recursion in <c>main</c> is ordinary Go. A .NET thread's stack is fixed when the thread starts, and the process's
/// startup thread gets the operating system's default: 1 MiB on Windows, the <c>ulimit -s</c> value (usually 8 MiB) on
/// linux, 8 MiB on macOS. A converted <c>main</c> that ran there died with an uncatchable "Stack overflow." where Go
/// prints its result (C46: two typescript-go parse units, and Behavioral MainDeepRecursion).
/// </para>
/// <para>
/// So the generated entry point hands <c>main</c> to <see cref="Run"/>, which starts one thread reserving
/// <see cref="StackReserve"/>, lends it goroutine 1 (<see cref="Goroutine.EnterAsMain"/>, the converted-test host's
/// mechanism), and waits for it. A reservation is address space, not memory: pages are committed as the stack reaches
/// them. Everything after <c>main</c> returns is unchanged, because the startup thread then returns from the entry
/// point exactly as it did when it ran <c>main</c> itself: the <see cref="MainExit"/> tail, an exit through
/// <see cref="MainExit.Exit"/>, and the backstop for a panic.
/// </para>
/// <para>
/// One Go rule outranks the stack: "All init functions are run on the startup thread. Calling LockOSThread from an
/// init function will cause the main function to be invoked on that thread" (runtime.LockOSThread). A package's
/// <c>init</c> runs before the entry point, on the startup thread, so when it has locked the main goroutine there,
/// <see cref="Run"/> calls <c>main</c> on that thread instead, with that thread's stack.
/// </para>
/// <para>
/// The thread is not named, as the startup thread that ran <c>main</c> before was not; other goroutines' threads are
/// named <c>goroutine-N</c> for a debugger. On linux an unnamed thread keeps the name it inherits from the startup
/// thread, the process's.
/// </para>
/// </remarks>
public static class MainThread
{
    /// <summary>
    /// The stack the main goroutine's thread reserves: 1 GiB, the converted-test host's per-test reservation
    /// (testing's TestExecution), which covers Go's 1 GB limit.
    /// </summary>
    public const int StackReserve = 1024 * 1024 * 1024;

    /// <summary>Runs <paramref name="main"/> as the main goroutine and returns when it returns.</summary>
    public static void Run(Action main)
    {
        ArgumentNullException.ThrowIfNull(main);

        // An init function locked the main goroutine to this, the startup thread: Go invokes main here.
        if (Goroutine.Current is { IsMain: true, LockedToThread: true })
        {
            main();
            return;
        }

        Thread thread = new(() =>
        {
            using Goroutine.Scope scope = Goroutine.EnterAsMain();
            main();
        }, StackReserve)
        {
            IsBackground = true
        };

        thread.Start();
        thread.Join();
    }

    /// <summary>
    /// Starts <paramref name="main"/> as the main goroutine on the same kind of thread and returns at once, with a task
    /// that completes with its result, or faults with what escaped it.
    /// </summary>
    /// <remarks>
    /// For a host that waits for main against a deadline: the converted-test host runs Go's <c>TestMain</c> and
    /// <c>M.Run</c> here, as Go runs them on the main goroutine, while its startup thread waits on the package timeout.
    /// The startup thread is a waiter there and never runs Go code, so an init function's LockOSThread has no thread to
    /// keep main on; the host ran TestMain off the startup thread before this existed, too.
    /// </remarks>
    public static System.Threading.Tasks.Task<T> Start<T>(Func<T> main)
    {
        ArgumentNullException.ThrowIfNull(main);

        System.Threading.Tasks.TaskCompletionSource<T> completion = new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

        Thread thread = new(() =>
        {
            try
            {
                using Goroutine.Scope scope = Goroutine.EnterAsMain();
                completion.SetResult(main());
            }
            catch (Exception ex)
            {
                completion.SetException(ex);
            }
        }, StackReserve)
        {
            IsBackground = true
        };

        thread.Start();

        return completion.Task;
    }
}
