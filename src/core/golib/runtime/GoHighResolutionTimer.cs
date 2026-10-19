// GoHighResolutionTimer.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace go.golib;

/// <summary>
/// A Windows high-resolution waitable timer, wrapped as a <see cref="WaitHandle"/> so the managed wait
/// primitives block on it directly: the object Go's windows runtime creates for its sub-tick waits
/// (runtime/os_windows.go, <c>CreateWaitableTimerExW</c> with <c>CREATE_WAITABLE_TIMER_HIGH_RESOLUTION</c>,
/// Windows 10 1803 and later).
/// </summary>
/// <remarks>
/// A process that never asked for a finer timer resolution waits a whole 15.6 ms tick on
/// <see cref="Thread.Sleep(int)"/> or a timed <see cref="WaitHandle.WaitOne(int)"/>; this timer waits within
/// about half a millisecond. golib's one wrapper: the timer service (<see cref="GoTimer"/>), the Gosched
/// escalation and windows <c>runtime.usleep</c> wait on it. Elsewhere there is no such object, and
/// <see cref="TryCreate"/> answers null.
/// </remarks>
public sealed partial class GoHighResolutionTimer : WaitHandle
{
    private const uint CreateHighResolution = 0x2;
    private const uint TimerAllAccess = 0x1F0003;

    private GoHighResolutionTimer(nint handle)
    {
        SafeWaitHandle = new SafeWaitHandle(handle, ownsHandle: true);
    }

    /// <summary>
    /// Creates a timer, or answers null where none exists: not windows, or a windows before 1803, which
    /// rejects the high-resolution flag (a plain waitable timer would be no finer than the coarse wait).
    /// </summary>
    public static GoHighResolutionTimer? TryCreate()
    {
        if (!OperatingSystem.IsWindows())
            return null;

        nint handle;

        try
        {
            handle = CreateWaitableTimerExW(0, null, CreateHighResolution, TimerAllAccess);
        }
        catch (Exception)
        {
            return null;
        }

        return handle == 0 ? null : new GoHighResolutionTimer(handle);
    }

    /// <summary>
    /// Arms the timer to signal <paramref name="nanos"/> from now; false when the arm fails, and the caller
    /// then waits the coarse way.
    /// </summary>
    /// <remarks>
    /// A NEGATIVE due time is a relative interval in 100 ns units. The truncation can only wake the waiter
    /// early, by under 100 ns, which a caller waiting for a deadline absorbs by waiting again.
    /// </remarks>
    public bool Arm(long nanos)
    {
        long dueTime = -(nanos / 100);

        if (dueTime == 0)
            dueTime = -1;

        // The reference count a SafeWaitHandle parameter would take, taken where a reader can see it: without
        // it a concurrent Dispose could close the handle between the read and the kernel's use of it.
        bool addedRef = false;

        try
        {
            SafeWaitHandle.DangerousAddRef(ref addedRef);

            return SetWaitableTimer(SafeWaitHandle.DangerousGetHandle(), ref dueTime, 0, 0, 0, resume: false);
        }
        finally
        {
            if (addedRef)
                SafeWaitHandle.DangerousRelease();
        }
    }

    /// <summary>
    /// Waits <paramref name="nanos"/> on a timer created for this call; false when none can be created or
    /// armed, and the caller then waits the coarse way.
    /// </summary>
    public static bool TrySleep(long nanos)
    {
        using GoHighResolutionTimer? timer = TryCreate();

        if (timer is null || !timer.Arm(nanos))
            return false;

        timer.WaitOne();
        return true;
    }

    [LibraryImport("kernel32.dll", EntryPoint = "CreateWaitableTimerExW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial nint CreateWaitableTimerExW(nint timerAttributes, string? timerName, uint flags, uint desiredAccess);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetWaitableTimer(nint timer, ref long dueTime, int period, nint completionRoutine, nint completionArg, [MarshalAs(UnmanagedType.Bool)] bool resume);
}
