// zsyscall_windows_file_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementations of readFile and writeFile, the kernel32 members of the OVERLAPPED
// submit seam. The seam itself -- the per-operation record, the completion-port binding, the
// readiness signal -- is zsyscall_windows_wsa_impl.cs, and its header is the design. This file adds
// no machinery; it routes the two file wrappers through that one.
//
// WHAT THE GENERATED BODIES DID. internal/poll issues every read and write on an overlapped FILE
// handle -- a named pipe, or a file opened with O_FILE_FLAG_OVERLAPPED -- through execIO, exactly as
// it issues WSARecv and WSASend on a socket: `syscall.ReadFile(fd.Sysfd, buf, &qty, &o.o)`. The
// generated wrappers handed the kernel `(uintptr)Ꮡoverlapped`, the transient interior address of
// the pooled, reference-bearing `operation`, and created NO operation record. With no record the
// handle was never bound to the CLR's completion port, so the kernel's completion had nowhere to
// arrive -- the TransmitFile defect over again (see its declaration). Measured on the i9 at hop
// 908642b2d4 with docs/phase4/probes/p2-named-pipe-deadline, all three steps hang where Go returns
// at once:
//
//   A. a 50 ms read deadline with no writer. The poller wakes on the deadline and cancels with
//      CancelIoEx(&o.o), which finds no record and answers ERROR_NOT_FOUND ("already completed"),
//      so runtime_pollWaitCanceled waits for a completion that never comes.
//   B. one write, then one read. os.NewFile keeps the handle's completion modes
//      (KeepFileCompletionModes, go.dev/issue/80979), so a SYNCHRONOUS success still owes a
//      completion packet and execIO waits for it -- on a port the handle was never bound to.
//   C. Close under a pending read: the same cancel path as A, reached from pd.evict.
//
// os TestVariousDeadlines1Proc/named_overlapped_pipe (new at Go 1.26) is A at scale.
//
// WHICH CALLS TAKE THE SEAM. Only an overlapped call on a descriptor the poller OWNS -- one with a
// readiness sink, registered by runtime_pollOpen (GoAsyncIO.HasReadiness). That is every execIO
// submit on a pollable FD, including one DisassociateIOCP has since taken off the port: the sink
// stays until pollClose, and the seam's own factory builds an unbound record for a marked
// descriptor that submits on the caller's event. Every other call keeps the generated body, and two
// populations depend on that:
//
//   - A nil overlapped: every synchronous read and write in the corpus.
//   - An overlapped on a descriptor the poller does not own: Pread/Pwrite on a BLOCKING handle,
//     which pass `&o.o` only to carry the file offset, and an FD whose Init left it unpolled (the
//     KeepFileCompletionModes query failing). Binding such a handle would route its completions to a
//     port nobody waits on, and the seam's header explains why a foreign overlapped arriving there is
//     misread rather than ignored.
//
// THE FILE OFFSET TRAVELS IN THE OVERLAPPED, as it does for TransmitFile: execIO writes fd.offset
// into o.o.Offset/OffsetHigh before every submit, and Rearm hands back a zeroed control block, so
// the offset is carried across. A pipe ignores it; an overlapped regular file reads from it.
//
// The harvest twin is internal/syscall/windows' GetOverlappedResult, hand-owned beside
// WSAGetOverlappedResult under the same predicate, so the result is read from the control block the
// kernel wrote rather than from the managed `o.o`.

using System;
using System.Threading;

using GoAsyncIO = go.golib.GoAsyncIO;

// Hand-owned (no zsyscall_windows_file_impl.go exists, so a reconvert never regenerates this file).
// The declarations it replaces are registered in the converter's manualConversionFuncs, which is what
// turns their generated bodies into placeholders.
[module: go.GoManualConversion]

// NativeOverlapped* and the buffer address are pointer work. Declared rather than inherited -- see
// zsyscall_windows_impl.cs.
[module: go.GoRequiresUnsafe]

#pragma warning disable CA1416 // NativeOverlapped is windows-only; this file compiles only into the windows flavour

namespace go;

partial class syscall_package
{
    internal static error /*err*/ readFile(ΔHandle handle, slice<byte> buf, ж<uint32> Ꮡdone, ж<Overlapped> Ꮡoverlapped) =>
        fileIO(procReadFile, wsaModeRead, handle, buf, Ꮡdone, Ꮡoverlapped);

    internal static error /*err*/ writeFile(ΔHandle handle, slice<byte> buf, ж<uint32> Ꮡdone, ж<Overlapped> Ꮡoverlapped) =>
        fileIO(procWriteFile, wsaModeWrite, handle, buf, Ꮡdone, Ꮡoverlapped);

    // ReadFile and WriteFile share one signature, so one body serves both; the mode is the
    // poller's ('r' / 'w') and names which waiter the completion wakes.
    private static unsafe error fileIO(ж<LazyProc> proc, nint mode, ΔHandle handle, slice<byte> buf, ж<uint32> Ꮡdone, ж<Overlapped> Ꮡoverlapped) {
        if (Ꮡoverlapped == nil || !GoAsyncIO.HasReadiness((nuint)(uintptr)handle)) {
            // The generated body, unchanged (see the file header for who reaches it).
            ж<byte> _p0 = default!;
            if (len(buf) > 0) {
                _p0 = Ꮡ(buf, 0);
            }
            var (r0, _, e0) = SyscallN(proc.Addr(), (uintptr)handle, (uintptr)_p0, (uintptr)len(buf), (uintptr)Ꮡdone, (uintptr)Ꮡoverlapped);
            GC.KeepAlive(_p0);
            GC.KeepAlive(Ꮡdone);
            GC.KeepAlive(Ꮡoverlapped);
            return r0 == 0 ? errnoErr(e0) : default!;
        }

        OverlappedOp operation = operationFor(handle, Ꮡoverlapped, mode);
        NativeOverlapped* native = operation.Rearm();

        // `unchecked` because NativeOverlapped spells both halves `int` while Go spells them DWORD.
        native->OffsetLow = unchecked((int)Ꮡoverlapped.Value.Offset);
        native->OffsetHigh = unchecked((int)Ꮡoverlapped.Value.OffsetHigh);

        // The caller's buffer is pinned by its element box, and the box is retained by the record for
        // the FLIGHT (after Rearm, which clears the previous submit's pins) -- the seam header's rule.
        byte* buffer = null;

        if (len(buf) > 0) {
            ж<byte> Ꮡbuf = Ꮡ(buf, 0);
            buffer = (byte*)(void*)Ꮡbuf;
            operation.Pin(Ꮡbuf);
        }

        // Written only during the synchronous portion; an asynchronous count comes back through the
        // harvest. So a call-local native, copied back, as WSARecv does.
        uint32 done = 0;

        var (r1, _, e1) = SyscallN(proc.Addr(), (uintptr)handle, (uintptr)(void*)buffer, (uintptr)len(buf), (uintptr)(void*)(&done), (uintptr)(void*)native);

        if (Ꮡdone != nil) {
            Ꮡdone.Value = done;
        }

        return r1 == 0 ? errnoErr(e1) : default!;
    }
}
