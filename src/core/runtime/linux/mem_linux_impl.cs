// mem_linux_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// The Linux memory primitives runtime/mem_linux.go's OS bodies reach, with exact managed forms
// (increment 6 of the runtime row, 2026-09-05): sysMmap/sysMunmap (cgo_mmap.go declares them as
// assembly; every sysAllocOS/sysReserveOS/sysMapOS/sysUsedOS/sysFreeOS path is an mmap/munmap),
// madvise (sysUnusedOS/sysHugePageOS/sysNoHugePageOS/sysFaultOS), and usleep (sys_linux_amd64.s;
// it sits behind freezetheworld and therefore behind every Go throw). mprotect is NOT here: the
// converted wrapper already reaches the kernel through syscall.Syscall6 (the funnel).
//
// Per-GOOS on purpose: the partials these body are declared in the linux flavour (cgo_mmap.cs,
// stubs2.cs), so a flat file would have no defining declaration to complete on the other targets
// (CS0759). Windows (VirtualAlloc) and darwin (the funnel) are their own rows.
//
// The memory these map is NATIVE and outside the CLR heap; what the runtime then does with it is
// its own affair -- a struct that carries managed references reinterpreted over it is the E3
// class (Q58's design), not this file's. Errors return as Go's err (errno), never as exceptions.
//
// Hand-owned: there is no mem_linux_impl.go, so a -stdlib reconvert never regenerates this file.

using System.Runtime.InteropServices;
using System.Threading;
using go.golib;
using @unsafe = go.unsafe_package;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    [DllImport("libc", EntryPoint = "mmap", SetLastError = true)]
    private static extern nint libc_mmap(nint addr, nuint length, int prot, int flags, int fd, long offset);

    [DllImport("libc", EntryPoint = "munmap", SetLastError = true)]
    private static extern int libc_munmap(nint addr, nuint length);

    [DllImport("libc", EntryPoint = "madvise", SetLastError = true)]
    private static extern int libc_madvise(nint addr, nuint length, int advice);

    // sysMmap calls the mmap system call. Go's err is 0 or the positive errno.
    internal static partial (@unsafe.Pointer Δp, nint err) sysMmap(@unsafe.Pointer addr, uintptr n, int32 prot, int32 flags, int32 fd, uint32 off)
    {
        nint r = libc_mmap((nint)(nuint)(uintptr)addr, (nuint)n, (int)prot, (int)flags, (int)fd, (long)(ulong)(uint)off);
        if (r == -1) {
            return (nil, (nint)Marshal.GetLastPInvokeError());
        }
        return (new @unsafe.Pointer((nuint)r), 0);
    }

    // sysMunmap calls the munmap system call.
    internal static partial void sysMunmap(@unsafe.Pointer addr, uintptr n)
    {
        libc_munmap((nint)(nuint)(uintptr)addr, (nuint)n);
    }

    // madvise returns 0 on success or the negative errno, as the assembly stub does.
    internal static partial int32 madvise(@unsafe.Pointer addr, uintptr n, int32 flags)
    {
        int r = libc_madvise((nint)(nuint)(uintptr)addr, (nuint)n, (int)flags);
        return r == 0 ? 0 : (int32)(-Marshal.GetLastPInvokeError());
    }

    // usleep sleeps for the given number of microseconds. Thread.Sleep has millisecond granularity,
    // so a sub-millisecond request spins its way there instead of rounding down to zero.
    internal static partial void usleep(uint32 usec)
    {
        if (usec >= 1000) {
            Thread.Sleep((int)(usec / 1000));
            return;
        }
        long until = System.Diagnostics.Stopwatch.GetTimestamp() + (long)(usec * (System.Diagnostics.Stopwatch.Frequency / 1_000_000.0));
        SpinWait spinner = default;
        while (System.Diagnostics.Stopwatch.GetTimestamp() < until) {
            spinner.SpinOnce();
        }
    }

    // ---- the guard's view (GolibTests RuntimeMemoryFamilyTests) ----

    /// <summary>Maps n bytes anonymous read/write, writes a byte pattern, reads it back, advises DONTNEED, unmaps; returns the address and whether the round trip held.</summary>
    public static unsafe (nuint address, bool roundTrip, nint firstErr, int madviseRc) GoSysMmapProbe(nuint n)
    {
        var (p, err) = sysMmap(nil, (uintptr)n, (int32)(_PROT_READ | _PROT_WRITE), (int32)(_MAP_ANON | _MAP_PRIVATE), -1, 0);
        if (err != 0) {
            return (0, false, err, 0);
        }
        nuint address = (uintptr)p;
        byte* bytes = (byte*)address;
        for (nuint i = 0; i < n; i += 4096) {
            bytes[i] = (byte)(i / 4096 + 1);
        }
        bool ok = true;
        for (nuint i = 0; i < n; i += 4096) {
            ok &= bytes[i] == (byte)(i / 4096 + 1);
        }
        int rc = (int)madvise(p, (uintptr)n, (int32)_MADV_DONTNEED);
        sysMunmap(p, (uintptr)n);
        return (address, ok, 0, rc);
    }

    /// <summary>The errno sysMmap answers for an impossible map (a length of zero).</summary>
    public static nint GoSysMmapErrnoProbe()
    {
        var (_, err) = sysMmap(nil, 0, (int32)(_PROT_READ | _PROT_WRITE), (int32)(_MAP_ANON | _MAP_PRIVATE), -1, 0);
        return err;
    }

    /// <summary>Sleeps for usec through the runtime's usleep and returns the elapsed microseconds.</summary>
    public static long GoUsleepProbe(uint usec)
    {
        long start = System.Diagnostics.Stopwatch.GetTimestamp();
        usleep((uint32)usec);
        return (System.Diagnostics.Stopwatch.GetTimestamp() - start) * 1_000_000 / System.Diagnostics.Stopwatch.Frequency;
    }
}
