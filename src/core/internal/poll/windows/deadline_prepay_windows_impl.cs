// deadline_prepay_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written: the windows part of the first read deadline's pre-pay (../deadline_prepay_impl.cs says
// why and what every flavor pays). On windows the deadline's wake does not end the read. execIO still
// owns a pending overlapped operation, so it cancels it (cancelIO: CancelIoEx, then
// pollDesc.waitCanceled until the kernel posts the aborted completion), harvests it (GetOverlappedResult
// on a file or pipe, WSAGetOverlappedResult on a socket), classifies the result against errno values
// (ERROR_OPERATION_ABORTED maps back to the deadline error), and unpins the operation. The aborted
// completion reaches this package on a CLR IO thread through syscall's completion callback, golib's
// GoAsyncIO.Signal and the descriptor's readiness sink. None of that runs before a process's first
// deadline expires, so it compiled after the deadline instant: the i9 read the first deadline 6.2-7.8 ms
// late cold and 1.3-3.4 ms late after a first completion with only the methods prepared (895445983a),
// against Go's 0.4-0.8, and its JIT list named the 24 methods still compiled between the fire and the
// return.
//
// What runs, once per process, inside the poller's start:
//   - RuntimeHelpers.PrepareMethod over this package's cancel, wait and wake methods, the readiness
//     sink, and the syscall and golib methods they call;
//   - on a spare operation nobody submitted: its overlapped's field view, compared with itself, and
//     CancelIoEx on it, which finds no record for it and answers ERROR_NOT_FOUND without calling the
//     kernel; then a pin and unpin of it on a spare FD's read pinner, as execIO's defer does;
//   - the comparisons a woken read makes, on values nobody holds: the poll error codes, the deadline
//     error against itself, and an errno error against the errno values Read and execIO test. Each
//     errno comparison runs twice, because golib compares an error with an errno by reflection, and the
//     invoke stub for the errno's == is built on its second use (named, as a per-call cost, on the BOARD);
//   - syscall.LoadCancelIoEx, Go's own loader for CancelIoEx's kernel32 entry point; it resolves the
//     address CancelIoEx calls through and opens nothing;
//   - internal/syscall/windows' module initialization, which registers that package's part (as syscall
//     registers its own) with GoAsyncIO.AddDeadlinePathPrepay before the poller runs every part;
//   - one real completion (COORD's ruling of 2026-10-20 16:47Z, after the i9 read the cut before this
//     step): a one-byte write and read on a private named pipe pair this step creates, uses and closes.
//     A cold first deadline stayed 4.2-4.8 ms late with every listed method compiled, while one real
//     completion through the poller first (the probe's io mode) brought it to Go's, so the remaining cost
//     looks like the process's first completion on a CLR IO thread, which no method list shows. The
//     i9's reading of this step (8967b1b1da, JIT) confirmed it: a cold first deadline 0.4-1.2 ms late,
//     against Go's 0.7-0.9. Its cost is about 8.5 ms added to the first pollable FD.Init, once per
//     process (the probe's cold setup, which opens the first pipe pair: 29.1-29.9 ms against 20.3-21.5
//     without the step). A process whose first I/O is a real completion pays about the same either way
//     (the probe's first write and read: 31.7-32.1 ms against 30.7-31.3), so for it the step moves that
//     cost to FD.Init rather than adding to it. The pair's FDs
//     are set up as FD.Init would set them, except that their poll descriptors come straight from
//     runtime_pollOpen: FD.Init would re-enter the serverInit.Do this runs inside. Their completion modes
//     are left as created, so the read's completion is delivered through the port even though the byte
//     is already there. Nothing of the pair survives this step.
//     The write and the read each carry a deadline one second out (G's review). On the normal path
//     the byte is there at once and neither fires. Where something holds the I/O (an I/O-intercepting
//     filter, say), the deadline cancels it as any deadline does and the step gives up, instead of
//     every process's first FD.Init waiting forever. A port that never posts even the cancelled
//     operation's completion still hangs it, as it would hang every deadline in the program.
//     The pipe takes an owner-only security descriptor (COORD's ruling, as defense in depth): only its
//     owner may open it, where the default would also let Everyone read it. If the descriptor cannot
//     be built its field stays 0, which asks for the default, and G found the default safe for this pipe.
//     Building it adds about 2 ms to the first FD.Init (the i9's smoke run of bac5dfbd2e), and our own
//     CreateFile passes it there.
// Apart from that pair, nothing here cancels, waits on or completes a real operation.

using System;
using System.Runtime.CompilerServices;
using MonotonicClock = go.golib.MonotonicClock;
using time = go.time_package;

[module: go.GoManualConversion]

namespace go.@internal;

using windows = go.@internal.syscall.windows_package;
using Δsyscall = syscall_package;

partial class poll_package
{
    static partial void prepayPlatformDeadlinePath()
    {
        prepare(
            readinessSink(0), (Action<ж<FD>, ж<operation>>)cancelIO, (pollDescModeAction)waitCanceled,
            runtime_pollWaitCanceled, netpollReady, golib.GoAsyncIO.Signal, Δsyscall.CancelIoEx, Δsyscall.Syscall,
            windows.GetOverlappedResult, windows.WSAGetOverlappedResult);

        ж<operation> o = Ꮡ(new operation());
        o.DerefOrNull();
        ж<Δsyscall.Overlapped> overlapped = o.of(operation.Ꮡo);
        overlapped.Equals(o.of(operation.Ꮡo));
        overlapped.Equals((object)o.of(operation.Ꮡo));
        Δsyscall.CancelIoEx(Δsyscall.InvalidHandle, overlapped);

        ж<FD> spare = Ꮡ(new FD());
        spare.DerefOrNull().pin('r', o.OrTypedNil());
        spare.of(FD.ᏑreadPinner).Unpin();

        _ = (pollErrTimeout, pollErrClosing);
        AreEqual(ErrDeadlineExceeded, ErrDeadlineExceeded);

        error aborted = Δsyscall.ERROR_OPERATION_ABORTED;
        for (int use = 0; use < 2; use++)
        {
            AreEqual(aborted, Δsyscall.ERROR_OPERATION_ABORTED);
            AreEqual(aborted, Δsyscall.ERROR_HANDLE_EOF);
            AreEqual(aborted, Δsyscall.ERROR_BROKEN_PIPE);
            AreEqual(aborted, windows.ERROR_IO_INCOMPLETE);
        }

        Δsyscall.LoadCancelIoEx();
        RuntimeHelpers.RunModuleConstructor(typeof(windows).Module.ModuleHandle);

        prepayFirstCompletion();
    }

    // CreateNamedPipe flags the converted windows package does not name: refuse to open an instance of a
    // pipe someone else already created under this name, and refuse clients from other machines.
    private const uint32 fileFlagFirstPipeInstance = 0x00080000;
    private const uint32 pipeRejectRemoteClients = 0x00000008;

    // The pipe's security descriptor: a protected DACL whose one entry grants all access to the pipe's
    // owner (OWNER RIGHTS), so no other account can open it.
    private const string ownerOnlySDDL = "D:P(A;;GA;;;OW)";
    private const uint32 sddlRevision1 = 1;

    // How long the pipe's write and read may take before the step gives up.
    private static readonly time.Duration pipeIOTimeout = time.ΔSecond;

    private static void prepayFirstCompletion()
    {
        @string name = $@"\\.\pipe\go2cs-deadline-prepay-{Δsyscall.Getpid()}-{MonotonicClock.Nanoseconds()}";
        var (wname, err) = Δsyscall.UTF16PtrFromString(name);
        if (err != default!)
            return;

        ref var sa = ref heap(new Δsyscall.SecurityAttributes(), out var Ꮡsa);
        sa.Length = (uint32)/* unsafe.Sizeof(sa) */ (uintptr)24;
        sa.SecurityDescriptor = ownerOnlyDescriptor();

        var (server, errΔ1) = windows.CreateNamedPipe(wname,
            (uint32)(windows.PIPE_ACCESS_DUPLEX | windows.O_FILE_FLAG_OVERLAPPED) | fileFlagFirstPipeInstance,
            (uint32)(windows.PIPE_TYPE_BYTE | windows.PIPE_READMODE_BYTE) | pipeRejectRemoteClients, 1, 4096, 4096, 0, Ꮡsa);
        if (sa.SecurityDescriptor != 0)
            Δsyscall.LocalFree((Δsyscall.ΔHandle)sa.SecurityDescriptor);
        if (errΔ1 != default!)
            return;

        ж<FD> reader = privatePipeFD(server);
        if (reader == nil)
        {
            Δsyscall.CloseHandle(server);
            return;
        }

        try
        {
            var (client, errΔ2) = Δsyscall.CreateFile(wname, (uint32)(Δsyscall.GENERIC_READ | Δsyscall.GENERIC_WRITE), 0, nil,
                (uint32)Δsyscall.OPEN_EXISTING, (uint32)Δsyscall.FILE_FLAG_OVERLAPPED, 0);
            if (errΔ2 != default!)
                return;

            ж<FD> writer = privatePipeFD(client);
            if (writer == nil)
            {
                Δsyscall.CloseHandle(client);
                return;
            }

            try
            {
                time.Time giveUp = time.Now().Add(pipeIOTimeout);
                if (writer.SetWriteDeadline(giveUp) != default! || reader.SetReadDeadline(giveUp) != default!)
                    return;

                slice<byte> one = new(1);
                var (n, errΔ3) = writer.Write(one);
                if (n == 1 && errΔ3 == default!)
                    reader.Read(one);
            }
            finally
            {
                writer.Close();
            }
        }
        finally
        {
            reader.Close();
        }
    }

    // The owner-only security descriptor, built from ownerOnlySDDL in memory the caller frees with
    // LocalFree; 0 when it cannot be built.
    private static uintptr ownerOnlyDescriptor()
    {
        var (sddl, err) = Δsyscall.UTF16PtrFromString(ownerOnlySDDL);
        if (err != default!)
            return 0;

        ж<Δsyscall.LazyProc> convert = Δsyscall.NewLazyDLL("advapi32.dll")
            .NewProc("ConvertStringSecurityDescriptorToSecurityDescriptorW");
        if (convert.Find() != default!)
            return 0;

        ref var descriptor = ref heap(new uintptr(), out var Ꮡdescriptor);
        var (r1, _, _) = Δsyscall.SyscallN(convert.Addr(), (uintptr)sddl, (uintptr)sddlRevision1, (uintptr)Ꮡdescriptor, 0);
        System.GC.KeepAlive(sddl);
        System.GC.KeepAlive(Ꮡdescriptor);
        return r1 != 0 ? descriptor : (uintptr)0;
    }

    // An FD over a private pipe handle, as FD.Init("pipe", true) leaves one, with its poll descriptor
    // taken from runtime_pollOpen directly (see the file header). Nil when the poller refuses it.
    private static ж<FD> privatePipeFD(Δsyscall.ΔHandle handle)
    {
        var (ctx, errno) = runtime_pollOpen((uintptr)handle);
        if (errno != 0)
            return nil;

        ж<FD> fd = Ꮡ(new FD(Sysfd: handle, IsStream: true, ZeroReadIsEOF: true));
        ref FD f = ref fd.DerefOrNull();
        f.kind = kindPipe;
        f.isFile = true;
        f.isBlocking = false;
        f.pd.runtimeCtx = ctx;
        return fd;
    }

    private delegate void pollDescModeAction(ref pollDesc pd, nint mode);
}
