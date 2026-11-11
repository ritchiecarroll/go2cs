// stubs2_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The linux runtime's raw file-descriptor syscalls: read, write1, open and closefd (stubs2.go), plus
// pipe2 and mincore (os_linux.go), whose bodies are assembly in sys_linux_amd64.s.
//
// WHY. The six are bodyless partials whose only bodies are assembly, so each compiled to
// PartialStubGenerator's throwing stub. On linux at master (2026-09-26) TestBadOpen,
// TestMincoreErrorSign and TestNonblockingPipe each ended in that stub's NotImplementedException
// (open, mincore and pipe2 respectively), read by the harness as infrastructure-error.
//
// WHAT THESE BODIES ARE. Each calls the libc function of the same name and returns what Go's
// assembly returns: read, write1 and mincore give the result or the NEGATIVE errno; open and closefd
// give the result or -1 (the stubs map every error to -1); pipe2 stores both fds and returns the
// errno word as 0 or the negative errno. libc's own wrappers are the kernel calls the assembly makes
// (open is openat(AT_FDCWD, ...) in both), so no semantics are added or lost.
//
// A buffer or name is pinned for the call through the box's own uintptr conversion
// (ж<T>.EnsureStableAddress, the FromPinnedBox door) and held reachable until the call returns.
//
// ALSO HERE: sched_getaffinity and osyield (os_linux.go) and exit (stubs2.go), three more partials
// whose only bodies are assembly. Each threw from its generated stub on linux/amd64 and linux/arm64:
// the public SetDefaultGOMAXPROCS reaches sched_getaffinity through getCPUCount with no branch on the
// way, the spin loops call osyield only under contention, and sysAllocOS calls exit(2) when mmap fails
// with EACCES or EAGAIN.
//   - sched_getaffinity is the RAW syscall, not libc's wrapper of the same name. The kernel answers
//     the number of bytes it wrote, which getCPUCount slices its buffer by; the wrapper answers 0,
//     and a body over it would count no CPU at all. The number is per arch.
//   - osyield is libc's sched_yield, as the windows and darwin bodies are their hosts' yields.
//   - exit is the raw exit_group, as Go's is: the process ends there, with no managed shutdown to
//     wait on. Its caller is a failure path inside the allocator.
//
// Hand-owned (no stubs2_impl.go exists, so a reconvert never regenerates this file).

using System;
using System.Runtime.InteropServices;
using go.golib;
using @unsafe = go.unsafe_package;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    [DllImport("libc", EntryPoint = "read", SetLastError = true)]
    private static extern nint libc_read(int fd, nint buf, nuint count);

    [DllImport("libc", EntryPoint = "write", SetLastError = true)]
    private static extern nint libc_write(int fd, nint buf, nuint count);

    [DllImport("libc", EntryPoint = "open", SetLastError = true)]
    private static extern int libc_open(nint pathname, int flags, int mode);

    [DllImport("libc", EntryPoint = "close", SetLastError = true)]
    private static extern int libc_close(int fd);

    [DllImport("libc", EntryPoint = "pipe2", SetLastError = true)]
    private static extern unsafe int libc_pipe2(int* pipefd, int flags);

    [DllImport("libc", EntryPoint = "mincore", SetLastError = true)]
    private static extern int libc_mincore(nint addr, nuint length, nint vec);

    private static int32 negativeErrno() => (int32)(-Marshal.GetLastPInvokeError());

    // stubs2.go: read returns the count or the negative errno.
    internal static partial int32 read(int32 fd, @unsafe.Pointer Δp, int32 n)
    {
        nint r = libc_read(fd, (nint)(nuint)(uintptr)Δp, (nuint)(uint)n);
        System.GC.KeepAlive(Δp);
        return r < 0 ? negativeErrno() : (int32)r;
    }

    // stubs2.go: write1 returns the count or the negative errno.
    internal static partial int32 write1(uintptr fd, @unsafe.Pointer Δp, int32 n)
    {
        nint r = libc_write((int)(nuint)fd, (nint)(nuint)(uintptr)Δp, (nuint)(uint)n);
        System.GC.KeepAlive(Δp);
        return r < 0 ? negativeErrno() : (int32)r;
    }

    // stubs2.go: open returns the fd, or -1 on any error.
    internal static partial int32 open(ж<byte> name, int32 mode, int32 perm)
    {
        int r = libc_open((nint)(nuint)(uintptr)name, mode, perm);
        System.GC.KeepAlive(name);
        return r < 0 ? -1 : r;
    }

    // stubs2.go: closefd returns 0, or -1 on any error.
    internal static partial int32 closefd(int32 fd) => libc_close(fd) < 0 ? -1 : 0;

    // os_linux.go: pipe2 returns both fds and the errno word, 0 or the negative errno.
    internal static partial (int32 r, int32 w, int32 errno) pipe2(int32 flags)
    {
        unsafe
        {
            int* fds = stackalloc int[2];
            if (libc_pipe2(fds, flags) < 0)
                return (0, 0, negativeErrno());
            return (fds[0], fds[1], 0);
        }
    }

    // os_linux.go: mincore returns 0 or the negative errno.
    internal static partial int32 mincore(@unsafe.Pointer addr, uintptr n, ж<byte> dst)
    {
        int r = libc_mincore((nint)(nuint)(uintptr)addr, (nuint)n, (nint)(nuint)(uintptr)dst);
        System.GC.KeepAlive(dst);
        return r < 0 ? negativeErrno() : 0;
    }

    // The sched_getaffinity number by arch (Go's sys_linux_amd64.s and sys_linux_arm64.s): 204 at amd64,
    // 123 at arm64. Crossed, 204 is getsockname at arm64 and 123 is setfsgid at amd64, a call that changes
    // the process. runtime sits below the syscall flavour, so this is an arch switch, as
    // sigprocmaskNumberFor is. null for an arch with no mapping; sched_getaffinity refuses it by name.
    internal static long? schedGetaffinityNumberFor(Architecture arch) => arch switch
    {
        Architecture.X64 => 204,
        Architecture.Arm64 => 123,
        _ => null,
    };

    [DllImport("libc", EntryPoint = "sched_yield")]
    private static extern int libc_sched_yield();

    // os_linux.go: sched_getaffinity returns the number of bytes the kernel wrote into buf, or the
    // negative errno. libc's syscall answers -1 and sets errno, which is turned back into Go's form.
    internal static partial int32 sched_getaffinity(uintptr pid, uintptr len, ж<byte> buf)
    {
        Architecture arch = RuntimeInformation.ProcessArchitecture;
        long number = schedGetaffinityNumberFor(arch) ??
            throw new PlatformNotSupportedException($"runtime.sched_getaffinity: no sched_getaffinity number for {arch}; refused rather than misread");

        long r = sys_syscall(number, (long)(nuint)pid, (long)(nuint)len, (nint)(nuint)(uintptr)buf);
        System.GC.KeepAlive(buf);
        return r < 0 ? negativeErrno() : (int32)r;
    }

    // os_linux.go: osyield gives up the processor (sched_yield).
    internal static partial void osyield() => libc_sched_yield();

    // stubs2.go: exit ends the process with this status and does not return (exit_group). An arch with
    // no number still ends the process, through the host, and says why on stderr first.
    internal static partial void exit(int32 code)
    {
        Architecture arch = RuntimeInformation.ProcessArchitecture;
        var numbers = dieNumbersFor(arch);

        if (numbers is null)
        {
            System.Console.Error.WriteLine("fatal: runtime.exit: no exit_group number for " + arch + "; exiting through the host with status " + code);
            System.Environment.Exit(code);
            return;
        }

        sys_syscall(numbers.Value.exitGroup, code, 0, 0);
    }

    // ---- the guard's view (GolibTests RuntimeFdSyscallTests) ----

    private static string failureOf(Exception ex) => $"{ex.GetType().Name}: {ex.Message}";

    /// <summary>
    /// TestBadOpen's sequence: open a path that does not exist, then read, write and close fd -1.
    /// Go answers -1, -EBADF, -EBADF and -1.
    /// </summary>
    public static (int open, int read, int write, int close, string? failure) GoBadOpenProbe()
    {
        try
        {
            var path = slice<byte>("/notreallyafile\0"u8);
            int fd = (int)open(Ꮡ(path, 0), 0, 0);
            ref var buf = ref heap(new array<byte>(32), out var Ꮡbuf);
            @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡbuf.at<byte>(0));
            int r = (int)read(-1, p, 32);
            int w = (int)write1(~(uintptr)0, p, 32);
            int c = (int)closefd(-1);
            return (fd, r, w, c, null);
        }
        catch (Exception ex)
        {
            return (0, 0, 0, 0, failureOf(ex));
        }
    }

    /// <summary>
    /// nonblockingPipe's pipe2, then one byte written through write1 and read back through read, then
    /// both ends closed. Returns the errno word, what was read, and the two close results.
    /// </summary>
    public static (int errno, int wrote, int readCount, byte readByte, int closeR, int closeW, string? failure) GoPipe2RoundTripProbe()
    {
        try
        {
            var (rfd, wfd, errno) = pipe2((int32)(_O_NONBLOCK | _O_CLOEXEC));
            if (errno != 0)
                return (errno, 0, 0, 0, 0, 0, null);

            ref var bout = ref heap((byte)42, out var Ꮡout);
            ref var bin = ref heap(new byte(), out var Ꮡin);
            int wrote = (int)write1((uintptr)wfd, @unsafe.Pointer.FromPinnedBox(Ꮡout), 1);
            int got = (int)read(rfd, @unsafe.Pointer.FromPinnedBox(Ꮡin), 1);
            int cr = (int)closefd(rfd);
            int cw = (int)closefd(wfd);
            return (0, wrote, got, bin, cr, cw, null);
        }
        catch (Exception ex)
        {
            return (0, 0, 0, 0, 0, 0, failureOf(ex));
        }
    }

    /// <summary>TestMincoreErrorSign: mincore over a misaligned address. Go answers -EINVAL (-22).</summary>
    public static (int result, string? failure) GoMincoreErrorSignProbe()
    {
        try
        {
            ref var dst = ref heap(new byte(), out var Ꮡdst);
            @unsafe.Pointer misaligned = @unsafe.Add(@unsafe.Pointer.FromPinnedBox(@new<int32>()), 1);
            return ((int)mincore(misaligned, 1, Ꮡdst), null);
        }
        catch (Exception ex)
        {
            return (0, failureOf(ex));
        }
    }

    // ---- sched_getaffinity, osyield and exit: the guard's view (GolibTests RuntimeLinuxSchedExitTests) ----

    /// <summary>
    /// GolibTests' probe for sched_getaffinity's per-arch number: schedGetaffinityNumberFor's answer, or null
    /// for an unmapped arch. Pure: no syscall.
    /// </summary>
    public static long? GoSchedGetaffinityNumberProbe(Architecture arch) => schedGetaffinityNumberFor(arch);

    /// <summary>
    /// getCPUCount's own call, sched_getaffinity(0, 8192, buf). Returns what the call returned and the
    /// number of bits set in that many bytes of the buffer.
    /// </summary>
    public static (int result, int bits, string? failure) GoSchedGetaffinityProbe()
    {
        try
        {
            ref var buf = ref heap(new array<byte>(8192), out var Ꮡbuf);
            int r = (int)sched_getaffinity(0, (uintptr)8192, Ꮡbuf.at<byte>(0));
            int bits = 0;

            for (int i = 0; i < r; i++)
                bits += System.Numerics.BitOperations.PopCount(buf[i]);

            return (r, bits, null);
        }
        catch (Exception ex)
        {
            return (0, 0, failureOf(ex));
        }
    }

    /// <summary>sched_getaffinity over a 1-byte buffer, which the kernel refuses. Go answers -EINVAL (-22).</summary>
    public static (int result, string? failure) GoSchedGetaffinityErrorSignProbe()
    {
        try
        {
            ref var dst = ref heap(new byte(), out var Ꮡdst);
            return ((int)sched_getaffinity(0, 1, Ꮡdst), null);
        }
        catch (Exception ex)
        {
            return (0, failureOf(ex));
        }
    }

    /// <summary>getCPUCount, the converted caller that slices its buffer by sched_getaffinity's result.</summary>
    public static (int count, string? failure) GoGetCPUCountProbe()
    {
        try
        {
            return ((int)getCPUCount(), null);
        }
        catch (Exception ex)
        {
            return (0, failureOf(ex));
        }
    }

    /// <summary>osyield, <paramref name="times"/> times over. Null when every call returned.</summary>
    public static string? GoOsyieldProbe(int times)
    {
        try
        {
            for (int i = 0; i < times; i++)
                osyield();

            return null;
        }
        catch (Exception ex)
        {
            return failureOf(ex);
        }
    }

    /// <summary>exit(code). It does not return: the process ends with that status.</summary>
    public static void GoExitProbe(int code) => exit((int32)code);
}
