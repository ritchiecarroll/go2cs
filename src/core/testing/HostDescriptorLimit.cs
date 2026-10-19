// HostDescriptorLimit.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.InteropServices;

// go2cs HAND-OWNED (whole file) — part of the Phase-4 test host (see TestRunner.cs).
[module: go.GoManualConversion]

namespace go.testing_runtime;

/// <summary>
/// Snapshots the process's RLIMIT_NOFILE before a top-level test and restores it after — host
/// hygiene, invisible inside a test.
/// </summary>
/// <remarks>
/// <para>
/// syscall's TestPrlimitFileLimit (syscall_linux_test.go:720-734) lowers the process's SOFT
/// RLIMIT_NOFILE to 43 and never restores the process limit: its deferred Store puts back only
/// syscall's cached origRlimitNofile pointer. A Go test binary lives inside 43 descriptors for the rest
/// of its run. The managed test host does not: the CLR's own thread start needs descriptors the lowered
/// limit no longer admits, so the NEXT test's goroutine start threw OutOfMemoryException out of
/// Thread.StartCore and the host died with 19 of syscall's rows unrun (go1.24.13 Linux leg,
/// claude/g-linux-leg-evidence b84e5d5bfa). The mechanism is measured by GolibTests'
/// LinuxDescriptorLimitTests.
/// </para>
/// <para>
/// The limit a test SETS is still the limit that test RUNS under, so a test that asserts on it reads
/// exactly what Go reads; only the NEXT test starts from the limit the run started with. The HARD limit
/// is never raised: a restore that the kernel refuses (a test that lowered the hard limit below the
/// snapshot's soft value) is reported back to the caller rather than retried.
/// </para>
/// </remarks>
internal static class HostDescriptorLimit
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RLimit
    {
        public ulong Cur;
        public ulong Max;
    }

    // RLIMIT_NOFILE: 7 on linux (asm-generic/resource.h), 8 on darwin (sys/resource.h). rlim_t is a
    // 64-bit unsigned on both, so the struct above is the kernel's layout on each.
    private static int ResourceId => OperatingSystem.IsMacOS() ? 8 : 7;

    internal static bool Applies => OperatingSystem.IsLinux() || OperatingSystem.IsMacOS();

    [DllImport("libc", SetLastError = true, EntryPoint = "getrlimit")]
    private static extern int getrlimit(int resource, out RLimit rlim);

    [DllImport("libc", SetLastError = true, EntryPoint = "setrlimit")]
    private static extern int setrlimit(int resource, in RLimit rlim);

    /// <summary>The current RLIMIT_NOFILE, or <c>null</c> where it does not apply or cannot be read.</summary>
    internal static RLimit? Snapshot()
    {
        if (!Applies)
            return null;

        return getrlimit(ResourceId, out RLimit limit) == 0 ? limit : null;
    }

    /// <summary>
    /// The current RLIMIT_NOFILE when it differs from <paramref name="snapshot"/>, or <c>null</c> when it does not, cannot
    /// be read, or there is no snapshot. Read before <see cref="Restore"/> by a caller that reports the move.
    /// </summary>
    internal static RLimit? Moved(RLimit? snapshot)
    {
        if (snapshot is not RLimit wanted || getrlimit(ResourceId, out RLimit current) != 0)
            return null;

        return current.Cur == wanted.Cur && current.Max == wanted.Max ? null : current;
    }

    /// <summary>
    /// Puts <paramref name="snapshot"/> back if the limit moved. Returns <c>null</c> when nothing needed
    /// restoring or the restore succeeded, else the errno the kernel refused it with.
    /// </summary>
    internal static int? Restore(RLimit? snapshot)
    {
        if (snapshot is not RLimit wanted || getrlimit(ResourceId, out RLimit current) != 0)
            return null;

        if (current.Cur == wanted.Cur && current.Max == wanted.Max)
            return null;

        return setrlimit(ResourceId, in wanted) == 0 ? null : Marshal.GetLastPInvokeError();
    }
}
