// ThreadCpuTime.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.InteropServices;

namespace go.golib;

/// <summary>
/// The CPU time another thread of this process has used: the budget a returning <c>main</c> gives a goroutine that is
/// running a panic's deferred calls (MainExit), in place of the 1000 turns runtime.main's yield loop gives it.
/// </summary>
/// <remarks>
/// <para>
/// A thread is named by a TOKEN taken on that thread (<see cref="CurrentThreadToken"/>), because only the thread itself
/// can name itself to the OS. The token is read on another thread through a <see cref="ThreadCpuReader"/>, opened once
/// for a wait and disposed when it ends. Documented per-thread APIs only:
/// <list type="bullet">
/// <item>windows: <c>GetCurrentThreadId</c>, then <c>OpenThread</c> and <c>GetThreadTimes</c>. The times advance at
/// the clock tick (15.6 ms by default), so a budget is spent in whole ticks.</item>
/// <item>linux: <c>gettid</c>, then <c>clock_gettime</c> on the thread's CPU clock id, built as glibc's
/// <c>pthread_getcpuclockid</c> builds it (<c>~tid &lt;&lt; 3 | CPUCLOCK_PERTHREAD_MASK | CPUCLOCK_SCHED</c>), so no
/// libpthread is needed. Nanosecond resolution.</item>
/// <item>darwin: <c>pthread_mach_thread_np</c>, then <c>thread_info(THREAD_BASIC_INFO)</c>. Microsecond resolution.</item>
/// </list>
/// </para>
/// <para>
/// Every failure is an answer, never an exception: a token of 0, a reader that cannot open, a read that fails. The
/// caller then keeps its wall-clock fallback. A thread that exited between taking its token and the read is one of
/// those failures.
/// </para>
/// </remarks>
internal static partial class ThreadCpuTime
{
    /// <summary>The calling thread's token, or 0 when this OS has no seam here or the call failed.</summary>
    internal static long CurrentThreadToken()
    {
        try
        {
            if (OperatingSystem.IsWindows())
                return GetCurrentThreadId();

            if (OperatingSystem.IsLinux())
                return LinuxThreadId();

            if (OperatingSystem.IsMacOS())
                return pthread_mach_thread_np(pthread_self());
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
        {
        }

        return 0;
    }

    /// <summary>Opens a reader of the CPU time of the thread <paramref name="token"/> names, or null when it cannot.</summary>
    internal static ThreadCpuReader? Open(long token)
    {
        if (token == 0)
            return null;

        try
        {
            if (OperatingSystem.IsWindows())
            {
                nint handle = OpenThread(ThreadQueryLimitedInformation, false, (uint)token);
                return handle == 0 ? null : new ThreadCpuReader(token, handle);
            }

            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                return new ThreadCpuReader(token, 0);
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
        {
        }

        return null;
    }

    // The CPU time of the thread a reader names, or false when it cannot be read.
    internal static unsafe bool TryRead(long token, nint handle, out TimeSpan cpu)
    {
        cpu = default;

        try
        {
            if (OperatingSystem.IsWindows())
            {
                if (!GetThreadTimes(handle, out _, out long exit, out long kernel, out long user))
                    return false;

                // A thread that has exited reports an exit time: it is not running anything a wait could give a turn to.
                if (exit != 0)
                    return false;

                cpu = TimeSpan.FromTicks(kernel + user); // 100 ns units, as TimeSpan's
                return true;
            }

            if (OperatingSystem.IsLinux())
            {
                // MAKE_THREAD_CPUCLOCK(tid, CPUCLOCK_SCHED): include/linux/posix-timers.h, as glibc's pthread_getcpuclockid.
                int clock = (~(int)token << 3) | CpuClockPerThreadMask | CpuClockSched;
                Timespec ts;

                if (clock_gettime(clock, &ts) != 0)
                    return false;

                cpu = TimeSpan.FromTicks(ts.Seconds * TimeSpan.TicksPerSecond + ts.Nanoseconds / 100);
                return true;
            }

            if (OperatingSystem.IsMacOS())
            {
                ThreadBasicInfo info;
                uint count = ThreadBasicInfoCount;

                if (thread_info((uint)token, ThreadBasicInfoFlavor, &info, ref count) != 0)
                    return false;

                long micros = ((long)info.UserSeconds + info.SystemSeconds) * 1_000_000 + info.UserMicroseconds + info.SystemMicroseconds;
                cpu = TimeSpan.FromTicks(micros * 10);
                return true;
            }
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
        {
        }

        return false;
    }

    // linux: gettid (glibc 2.30 and later), or the raw syscall where libc has no wrapper.
    private static long LinuxThreadId()
    {
        try
        {
            return gettid();
        }
        catch (EntryPointNotFoundException)
        {
            long number = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => 186,
                Architecture.Arm64 => 178,
                Architecture.X86 or Architecture.Arm => 224,
                _ => -1,
            };

            return number < 0 ? 0 : syscall(number);
        }
    }

    // ---- windows ----

    private const uint ThreadQueryLimitedInformation = 0x0800;

    [LibraryImport("kernel32.dll")]
    private static partial uint GetCurrentThreadId();

    [LibraryImport("kernel32.dll")]
    private static partial nint OpenThread(uint desiredAccess, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, uint threadId);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetThreadTimes(nint thread, out long creation, out long exit, out long kernel, out long user);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CloseHandle(nint handle);

    // ---- linux ----

    private const int CpuClockPerThreadMask = 4;
    private const int CpuClockSched = 2;

    [StructLayout(LayoutKind.Sequential)]
    private struct Timespec
    {
        public long Seconds;
        public long Nanoseconds;
    }

    [LibraryImport("libc")]
    private static partial int gettid();

    [LibraryImport("libc")]
    private static partial long syscall(long number);

    [LibraryImport("libc")]
    private static unsafe partial int clock_gettime(int clock, Timespec* time);

    // ---- darwin ----

    private const int ThreadBasicInfoFlavor = 3; // THREAD_BASIC_INFO
    private const uint ThreadBasicInfoCount = 10; // sizeof(thread_basic_info) / sizeof(natural_t)

    [StructLayout(LayoutKind.Sequential)]
    private struct ThreadBasicInfo
    {
        public int UserSeconds;
        public int UserMicroseconds;
        public int SystemSeconds;
        public int SystemMicroseconds;
        public int CpuUsage;
        public int Policy;
        public int RunState;
        public int Flags;
        public int SuspendCount;
        public int SleepTime;
    }

    [LibraryImport("libc")]
    private static partial nint pthread_self();

    [LibraryImport("libc")]
    private static partial uint pthread_mach_thread_np(nint thread);

    [LibraryImport("libc")]
    private static unsafe partial int thread_info(uint thread, int flavor, ThreadBasicInfo* info, ref uint count);
}

/// <summary>
/// Reads one thread's CPU time for the length of a wait (<see cref="ThreadCpuTime"/>). On windows it holds the thread's
/// handle, closed by <see cref="Dispose"/>; elsewhere it holds nothing.
/// </summary>
internal sealed class ThreadCpuReader : IDisposable
{
    private readonly long m_token;
    private nint m_handle;

    internal ThreadCpuReader(long token, nint handle)
    {
        m_token = token;
        m_handle = handle;
    }

    /// <summary>The thread's CPU time now, or false when it cannot be read (it exited, or the call failed).</summary>
    internal bool TryRead(out TimeSpan cpu) => ThreadCpuTime.TryRead(m_token, m_handle, out cpu);

    public void Dispose()
    {
        nint handle = m_handle;
        m_handle = 0;

        if (handle != 0 && OperatingSystem.IsWindows())
            ThreadCpuTime.CloseHandle(handle);
    }
}
