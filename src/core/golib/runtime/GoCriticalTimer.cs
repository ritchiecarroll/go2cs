// GoCriticalTimer.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace

using System;
using System.Runtime.InteropServices;

namespace go.golib;

/// <summary>
/// darwin's precise short wait: a one-shot kqueue <c>EVFILT_TIMER</c> with <c>NOTE_CRITICAL</c>, which asks the
/// kernel for minimal timer coalescing (sys/event.h). The Gosched escalation waits on it on macOS, as it waits on
/// <see cref="GoHighResolutionTimer"/> on Windows.
/// </summary>
/// <remarks>
/// On the hosted macOS runners every plain sleep coalesces: <see cref="System.Threading.Thread.Sleep(int)"/>,
/// <c>nanosleep</c> and <c>mach_wait_until</c> asked for 1 ms read about 9 ms at p50 on osx-arm64, and a p90 of
/// 2.9-6.1 ms on osx-x64. This timer read p50 1.02 / p99 1.05 ms (arm64) and 1.11 / 1.27 ms (x64), 500 waits each
/// (docs/phase4/DESIGN-darwin-gosched-clock.md). The kqueue is created and closed per wait, a few microseconds
/// against the wait, so no per-thread descriptor exists to classify or leak. Every failure answers false, and the
/// caller keeps its coarse sleep.
/// </remarks>
internal static partial class GoCriticalTimer
{
    private static volatile bool s_unavailable;

    /// <summary>
    /// Waits about <paramref name="nanoseconds"/> (rounded down to whole microseconds, at least one), or answers false
    /// without waiting where the timer cannot be made: not macOS, or kqueue missing or refusing. An interrupted wait
    /// (EINTR) answers true: the caller's purpose, leaving the run queue, was served.
    /// </summary>
    internal static bool TrySleep(long nanoseconds)
    {
        if (s_unavailable || !OperatingSystem.IsMacOS())
            return false;

        int kq;

        try
        {
            kq = kqueue();
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
        {
            s_unavailable = true;
            return false;
        }

        if (kq < 0)
            return false;

        try
        {
            Kevent change = new()
            {
                Ident = 1,
                Filter = EvfiltTimer,
                Flags = EvAdd | EvOneshot,
                Fflags = NoteUseconds | NoteCritical,
                Data = (nint)Math.Max(1, nanoseconds / 1000)
            };

            int fired = kevent(kq, in change, 1, out Kevent result, 1, 0);

            if (fired == 1)
                return (result.Flags & EvError) == 0;

            return fired < 0 && Marshal.GetLastPInvokeError() == Eintr;
        }
        finally
        {
            close(kq);
        }
    }

    /// <summary>
    /// A WAKEABLE precise wait for one long-lived thread (GoTimer's service thread): a kqueue holding an
    /// <c>EVFILT_USER</c> event for the wake, as Go's own darwin netpoll breaks its kevent wait, beside the same
    /// one-shot <c>EVFILT_TIMER</c> with <c>NOTE_CRITICAL</c> <see cref="TrySleep"/> uses. <see cref="Wait"/> returns at
    /// the due time or as soon as <see cref="Wake"/> is called from any thread, including before the wait began. Null
    /// from <see cref="TryCreate"/> where the kqueue cannot be made, and false from <see cref="Wait"/> on any failure:
    /// the caller then keeps its coarse wait.
    /// </summary>
    internal sealed class Waiter
    {
        private const nuint TimerIdent = 1;
        private const nuint WakeIdent = 2;

        private readonly int m_kq;

        private Waiter(int kq) => m_kq = kq;

        internal static Waiter? TryCreate()
        {
            if (s_unavailable || !OperatingSystem.IsMacOS())
                return null;

            try
            {
                int kq = kqueue();

                if (kq < 0)
                    return null;

                Kevent wake = new() { Ident = WakeIdent, Filter = EvfiltUser, Flags = EvAdd | EvClear };

                if (kevent(kq, in wake, 1, out _, 0, 0) < 0)
                {
                    close(kq);
                    return null;
                }

                return new Waiter(kq);
            }
            catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
            {
                s_unavailable = true;
                return null;
            }
        }

        /// <summary>Waits about <paramref name="nanoseconds"/> (whole microseconds, at least one), or until woken.</summary>
        internal bool Wait(long nanoseconds)
        {
            Kevent timer = new()
            {
                Ident = TimerIdent,
                Filter = EvfiltTimer,
                Flags = EvAdd | EvOneshot,
                Fflags = NoteUseconds | NoteCritical,
                Data = (nint)Math.Max(1, nanoseconds / 1000)
            };

            int fired = kevent(m_kq, in timer, 1, out Kevent result, 1, 0);

            if (fired == 1)
                return (result.Flags & EvError) == 0;

            return fired < 0 && Marshal.GetLastPInvokeError() == Eintr;
        }

        /// <summary>Ends the current or the next <see cref="Wait"/> at once.</summary>
        internal void Wake()
        {
            Kevent trigger = new() { Ident = WakeIdent, Filter = EvfiltUser, Fflags = NoteTrigger };
            kevent(m_kq, in trigger, 1, out _, 0, 0);
        }
    }

    // struct kevent (sys/event.h), 64-bit: uintptr_t ident; int16_t filter; uint16_t flags; uint32_t fflags;
    // intptr_t data; void *udata -- 32 bytes.
    [StructLayout(LayoutKind.Sequential)]
    private struct Kevent
    {
        public nuint Ident;
        public short Filter;
        public ushort Flags;
        public uint Fflags;
        public nint Data;
        public nint Udata;
    }

    private const short EvfiltTimer = -7;
    private const short EvfiltUser = -10;
    private const ushort EvAdd = 0x0001;
    private const ushort EvOneshot = 0x0010;
    private const ushort EvClear = 0x0020;
    private const uint NoteTrigger = 0x01000000;
    private const ushort EvError = 0x4000;
    private const uint NoteUseconds = 0x00000002;
    private const uint NoteCritical = 0x00000020;
    private const int Eintr = 4;

    [LibraryImport("libc", SetLastError = true)]
    private static partial int kqueue();

    [LibraryImport("libc", SetLastError = true)]
    private static partial int kevent(int kq, in Kevent changelist, int nchanges, out Kevent eventlist, int nevents,
        nint timeout);

    [LibraryImport("libc")]
    private static partial int close(int fd);
}
