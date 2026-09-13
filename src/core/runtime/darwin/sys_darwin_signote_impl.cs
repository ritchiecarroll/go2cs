// sys_darwin_signote_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The darwin run layer's increment 4 (2026-09-03): runtime's own pipe, read and write1 -- the three
// primitives the async-signal-safe signal note (os_darwin.go: sigNoteSetup / sigNoteWakeup /
// sigNoteSleep) is built on, and the ones runtime's own throw and print write through (writeErr ->
// write1). Go implements each as an assembly trampoline that reads its arguments from the caller's
// ABI0 frame: pipe takes the address of a [2]int32 the callee fills, and read / write1 take the
// address of the FIRST argument, the other two lying in the next stack slots.
//
// The converted bodies reproduce the addresses and hand them to the keystone's args-struct
// dispatcher (golib/GoLibcCall.cs), which walks the pointee's FIELDS into registers:
//
//   - pipe's pointee is a golib array<int32>, whose first field is a managed reference -- refused
//     by name (`libcCall(pipe): field 'm_array' of array`1 is a Int32[], which this dispatcher
//     cannot place in an integer register`), the SignalPrimitives death the first full darwin
//     behavioral census measured on osx-x64;
//   - read's and write1's pointee is the fd alone (int32 / uintptr: one field), so libc receives
//     the fd in the first register and whatever the second and third registers held -- a junk
//     buffer and length. On x64 that is EFAULT or a stray write; on arm64 it is plausibly the
//     exit-138 SIGBUS the same census recorded with NOTHING on stderr, and nothing CAN reach
//     stderr on this platform until write1 works, because writeErr is write1.
//
// This file gives the three their arguments the way libc wants them: pipe through an 8-byte native
// buffer (Marshal, since converted packages compile without unsafe blocks), read and write through
// the buffer's native address -- the uintptr conversion of the unsafe.Pointer argument, which pins
// the managed box and registers the pin (golib ж<T>'s address model) -- with the box kept alive
// across the call as the converted bodies did. Return contracts are Go's trampolines': pipe returns
// (r, w, errno); read and write1 return the byte count, or -errno on failure (the NEGL in
// read_trampoline / write_trampoline).
//
// Registered as `"runtime": {"pipe", "read", "write1": goosDarwin}` in manualTypeOperations.go.
// Scope: the darwin flavour alone; the linux flavour's read/write/pipe2 are raw-syscall bodies the
// keystone never sees. The durable form the keystone's own message names -- a per-symbol layout
// record (pipe: an out-buffer of two ints; read/write: three contiguous args) -- replaces these
// three bodies later with no corpus change, since the emission is the same three placeholders
// either way.

using System;
using System.Runtime.InteropServices;
using go;

[module: go.GoManualConversion]

namespace go;

using @unsafe = unsafe_package;

partial class runtime_package
{
    [DllImport("libc", EntryPoint = "pipe", SetLastError = true)]
    private static extern int signote_pipe(nint fds);

    [DllImport("libc", EntryPoint = "read", SetLastError = true)]
    private static extern nint signote_read(int fd, nint buf, nint count);

    [DllImport("libc", EntryPoint = "write", SetLastError = true)]
    private static extern nint signote_write(int fd, nint buf, nint count);

    // pipe(2) into a native fd pair. Go: `pipe() (r, w int32, errno int32)`, errno 0 on success.
    internal static (int32 r, int32 w, int32 errno) pipe()
    {
        nint fds = Marshal.AllocHGlobal(8);

        try
        {
            Marshal.WriteInt32(fds, 0, -1);
            Marshal.WriteInt32(fds, 4, -1);

            if (signote_pipe(fds) != 0)
            {
                return (-1, -1, Marshal.GetLastPInvokeError());
            }

            return (Marshal.ReadInt32(fds, 0), Marshal.ReadInt32(fds, 4), 0);
        }
        finally
        {
            Marshal.FreeHGlobal(fds);
        }
    }

    // read(2). Go: `read(fd int32, p unsafe.Pointer, n int32) int32`, the count or -errno.
    internal static int32 read(int32 fd, @unsafe.Pointer p, int32 n)
    {
        nint buf = (nint)(nuint)(uintptr)p;
        nint got = signote_read(fd, buf, n);
        int32 ret = got < 0 ? (int32)(-Marshal.GetLastPInvokeError()) : (int32)got;
        KeepAlive(p);
        return ret;
    }

    // write(2). Go: `write1(fd uintptr, p unsafe.Pointer, n int32) int32`, the count or -errno.
    internal static int32 write1(uintptr fd, @unsafe.Pointer p, int32 n)
    {
        nint buf = (nint)(nuint)(uintptr)p;
        nint put = signote_write((int)(nuint)fd, buf, n);
        int32 ret = put < 0 ? (int32)(-Marshal.GetLastPInvokeError()) : (int32)put;
        KeepAlive(p);
        return ret;
    }
}
