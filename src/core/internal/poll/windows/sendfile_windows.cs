// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using io = io_package;
using Δsyscall = syscall_package;

partial class poll_package {

// SendFile wraps the TransmitFile call.
public static (int64 written, error err) SendFile(ж<FD> Ꮡfd, syscallꓸHandle src, int64 n) {
    int64 written = default!;
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();
        ref var err = ref Ꮡerr.ValueSlot;

        defer(() => {
            TestHookDidSendFile(Ꮡfd, 0, written, Ꮡerr.ValueSlot, written > 0);
        }, ref ᒐ);
        if (fd.kind == kindPipe) {
            // TransmitFile does not work with pipes
            (written, err) = (0, Δsyscall.ESPIPE); goto ᒐdone;
        }
        {
            var (ft, _) = Δsyscall.GetFileType(src); if (ft == Δsyscall.FILE_TYPE_PIPE) {
                (written, err) = (0, Δsyscall.ESPIPE); goto ᒐdone;
            }
        }

        {
            var errΔ1 = Ꮡfd.writeLock(); if (errΔ1 != default!) {
                (written, err) = (0, errΔ1); goto ᒐdone;
            }
        }
        defer(Ꮡfd.writeUnlock, ref ᒐ);

        var o = Ꮡfd.of(FD.Ꮡwop);
        o.Value.handle = src;

        // TODO(brainman): skip calling syscall.Seek if OS allows it
        (var curpos, err) = Δsyscall.Seek((~o).handle, 0, io.SeekCurrent);
        if (err != default!) {
            (written, err) = (0, err); goto ᒐdone;
        }

        if (n <= 0) { // We don't know the size of the file so infer it.
            // Find the number of bytes offset from curpos until the end of the file.
            (n, err) = Δsyscall.Seek((~o).handle, -curpos, io.SeekEnd);
            if (err != default!) {
                goto ᒐdone;
            }
            // Now seek back to the original position.
            {
                (_, err) = Δsyscall.Seek((~o).handle, curpos, io.SeekStart); if (err != default!) {
                    goto ᒐdone;
                }
            }
        }

        // TransmitFile can be invoked in one call with at most
        // 2,147,483,646 bytes: the maximum value for a 32-bit integer minus 1.
        // See https://docs.microsoft.com/en-us/windows/win32/api/mswsock/nf-mswsock-transmitfile
        const int64 maxChunkSizePerCall = /* int64(0x7fffffff - 1) */ 2147483646;

        while (n > 0) {
            var chunkSize = maxChunkSizePerCall;
            if (chunkSize > n) {
                chunkSize = n;
            }

            o.Value.qty = (uint32)chunkSize;
            o.Value.o.Offset = (uint32)curpos;
            o.Value.o.OffsetHigh = (uint32)((curpos >> (int)(32)));

            var (nw, errΔ2) = execIO(o, (ж<operation> oΔ1) => Δsyscall.TransmitFile((~(~oΔ1).fd).Sysfd, (~oΔ1).handle, (~oΔ1).qty, 0, oΔ1.of(operation.Ꮡo), nil, Δsyscall.TF_WRITE_BEHIND));
            if (errΔ2 != default!) {
                (written, err) = (written, errΔ2); goto ᒐdone;
            }

            curpos += (int64)nw;

            // Some versions of Windows (Windows 10 1803) do not set
            // file position after TransmitFile completes.
            // So just use Seek to set file position.
            {
                (_, errΔ2) = Δsyscall.Seek((~o).handle, curpos, io.SeekStart); if (errΔ2 != default!) {
                    (written, err) = (written, errΔ2); goto ᒐdone;
                }
            }

            n -= (int64)nw;
            written += (int64)nw;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (written, Ꮡerr.ValueSlot);
}

} // end poll_package
