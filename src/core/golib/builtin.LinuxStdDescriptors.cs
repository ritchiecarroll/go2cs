// builtin.LinuxStdDescriptors.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace go;

// ---------------------------------------------------------------------------------------------
// LINUX STANDARD-DESCRIPTOR HYGIENE - the one piece of process setup that makes closing a
// standard stream behave in a converted program the way it does in the Go binary.
//
// WHY THIS EXISTS
//   A Go process holds exactly one file descriptor per standard stream: 0, 1, 2. Closing
//   os.Stdout therefore releases the last reference to the write end of a stdout pipe, and the
//   parent reading that pipe sees EOF immediately - an idiom Go programs (and Go's own os/exec
//   test suite, via its "close stdout to signal readiness" barriers) rely on.
//
//   A converted program is an ordinary .NET process, and the .NET runtime duplicates each
//   standard descriptor at startup - fcntl(0/1/2, F_DUPFD_CLOEXEC), observed unconditionally on
//   linux-x64 net10.0 with an empty Main and zero Console touches (strace, 2026-08-27). Those
//   duplicates hold the underlying pipe/socket description open for the life of the process, so
//   Go code closing fd 1 no longer releases the pipe: the parent's read blocks until the child
//   EXITS. The divergence is measured, not theoretical - the pipe-EOF-barrier witness reads EOF
//   1.1 ms after the child's close in native Go and 8.27 s (= child exit) in the unfixed
//   converted program, which is what held os/exec's signal family (TestWaitInterrupt/*) and any
//   other pipe-EOF-barrier test hung on Linux.
//
// WHAT IT DOES
//   At golib module initialization - the managed dawn of every converted process, before any
//   user code and before anything touches System.Console - close every descriptor above 2 that
//   (a) aliases the same /proc/self/fd target as descriptor 0, 1 or 2, and (b) carries
//   FD_CLOEXEC. Both conditions are load-bearing:
//
//   - An INHERITED descriptor can never carry FD_CLOEXEC (a close-on-exec fd by definition did
//     not survive the exec that started this process), so a deliberately passed extra file that
//     happens to duplicate a standard stream - Go's ExtraFiles contract, fds 3+ - is never
//     touched.
//   - At module-initialization time no managed user code has run, so every close-on-exec alias
//     of a standard description is the runtime's own startup duplicate. That timing IS the
//     safety contract: System.Console creates its own on-demand duplicates when first touched,
//     and sweeping aliases AFTER that point would close a live SafeFileHandle out from under it
//     (measured: Console dies with "Bad file descriptor" - which is why this runs first thing
//     in InitializeGoLib and must stay there).
//
// WHY CLOSING THEM IS SAFE
//   Measured, not argued (probe battery, 2026-08-27, linux-x64 net10.0): after closing the
//   startup duplicates at managed dawn, Console.Out/Error re-duplicate on demand and work,
//   stdin reads work, child processes spawn and report, GC + finalizer cycles disturb nothing,
//   and an strace over the full process lifecycle shows the runtime never operates on those
//   descriptors again - the only close() calls on them in the whole process life were ours.
//   Freed numbers are safely reused by later opens.
//
// WHAT IS DELIBERATELY NOT DONE
//   golib's own println/print diagnostics route through Console.Error, whose on-demand duplicate
//   of fd 2 would hold a STDERR pipe open past a Go-side os.Stderr.Close() - the same shape, one
//   stream over. No measured test needs stderr-close EOF propagation today; if a row ever does,
//   the durable move is routing println's bytes at fd 2 directly on Unix rather than suppressing
//   Console. Darwin has its own arm (no /proc there; aliases are found by fstat identity - see
//   builtin.DarwinStdDescriptors.cs), and Windows is untouched (handle semantics differ and the
//   banked os/exec row already proves pipe teardown there).
// ---------------------------------------------------------------------------------------------
public static partial class builtin
{
    private const int F_GETFD = 1;
    private const int FD_CLOEXEC = 1;

    [LibraryImport("libc", EntryPoint = "close")]
    private static partial int sys_close_fd(int fd);

    [LibraryImport("libc", EntryPoint = "fcntl")]
    private static partial int sys_fcntl_getfd(int fd, int cmd);

    [LibraryImport("libc", EntryPoint = "readlink", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint sys_readlink(string path, Span<byte> buffer, nuint size);

    // Called first thing from InitializeGoLib, golib's analogue of Go's runtime.osinit. The
    // before-any-Console-touch ordering is part of the safety contract above.
    private static void InitializeLinuxStdDescriptorHygiene()
    {
        if (!OperatingSystem.IsLinux())
            return;

        try
        {
            string stdinTarget = ReadDescriptorTarget(0);
            string stdoutTarget = ReadDescriptorTarget(1);
            string stderrTarget = ReadDescriptorTarget(2);

            if (stdinTarget.Length == 0 && stdoutTarget.Length == 0 && stderrTarget.Length == 0)
                return;

            foreach (string entry in Directory.GetFiles("/proc/self/fd"))
            {
                if (!int.TryParse(Path.GetFileName(entry), out int fd) || fd <= 2)
                    continue;

                string target = ReadDescriptorTarget(fd);

                if (target.Length == 0)
                    continue;

                if (target != stdinTarget && target != stdoutTarget && target != stderrTarget)
                    continue;

                int flags = sys_fcntl_getfd(fd, F_GETFD);

                if (flags < 0 || (flags & FD_CLOEXEC) == 0)
                    continue;

                sys_close_fd(fd);
            }
        }
        catch
        {
            // Defensive posture, same as InitializeWindowsLongPaths: an unusual host (no /proc,
            // a hardened container) simply keeps the runtime duplicates, and standard-stream
            // close semantics stay as they were - never take down module initialization for a
            // parity measure.
        }
    }

    private static string ReadDescriptorTarget(int fd)
    {
        Span<byte> buffer = stackalloc byte[256];
        nint length = sys_readlink($"/proc/self/fd/{fd.ToString()}", buffer, (nuint)buffer.Length);

        return length > 0 ? Encoding.UTF8.GetString(buffer[..(int)length]) : string.Empty;
    }
}
