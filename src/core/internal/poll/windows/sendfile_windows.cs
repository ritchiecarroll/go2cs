// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using io = io_package;
using Δsyscall = syscall_package;

partial class poll_package {

// SendFile wraps the TransmitFile call.
public static (int64 written, error err, bool handled) SendFile(ж<FD> Ꮡfd, uintptr src, int64 size) {
    int64 written = default!;
    heap<error>(out var Ꮡerr);
    bool handled = default!;
    GoFrame ᒐ = default;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();
        ref var err = ref Ꮡerr.ValueSlot;

        defer(() => {
            TestHookDidSendFile(Ꮡfd, 0, written, Ꮡerr.ValueSlot, written > 0);
        }, ref ᒐ);
        if (fd.kind == kindPipe) {
            // TransmitFile does not work with pipes
            (written, err, handled) = (0, Δsyscall.ESPIPE, false); goto ᒐdone;
        }
        var hsrc = ((syscallꓸHandle)src);
        {
            var (ft, _) = Δsyscall.GetFileType(hsrc); if (ft == Δsyscall.FILE_TYPE_PIPE) {
                (written, err, handled) = (0, Δsyscall.ESPIPE, false); goto ᒐdone;
            }
        }

        {
            var errΔ1 = Ꮡfd.writeLock(); if (errΔ1 != default!) {
                (written, err, handled) = (0, errΔ1, false); goto ᒐdone;
            }
        }
        defer(Ꮡfd.writeUnlock, ref ᒐ);

        // Get the file size so we don't read past the end of the file.
        ref var fi = ref heap(new Δsyscall.ByHandleFileInformation(), out var Ꮡfi);
        {
            var errΔ2 = Δsyscall.GetFileInformationByHandle(hsrc, Ꮡfi); if (errΔ2 != default!) {
                (written, err, handled) = (0, errΔ2, false); goto ᒐdone;
            }
        }
        var fileSize = ((int64)fi.FileSizeHigh << (int)(32)) + (int64)fi.FileSizeLow;
        (var startpos, err) = Δsyscall.Seek(hsrc, 0, io.SeekCurrent);
        if (err != default!) {
            (written, err, handled) = (0, err, false); goto ᒐdone;
        }
        var maxSize = fileSize - startpos;
        if (size <= 0){
            size = maxSize;
        } else {
            size = min(size, maxSize);
        }

        defer(() => {
            if (written > 0) {
                // Some versions of Windows (Windows 10 1803) do not set
                // file position after TransmitFile completes.
                // So just use Seek to set file position.
                var (_, serr) = Δsyscall.Seek(hsrc, startpos + written, io.SeekStart);
                if (Ꮡerr.ValueSlot != default!) {
                    Ꮡerr.ValueSlot = serr;
                }
            }
        }, ref ᒐ);

        // TransmitFile can be invoked in one call with at most
        // 2,147,483,646 bytes: the maximum value for a 32-bit integer minus 1.
        // See https://docs.microsoft.com/en-us/windows/win32/api/mswsock/nf-mswsock-transmitfile
        const int64 maxChunkSizePerCall = /* int64(0x7fffffff - 1) */ 2147483646;

        while (size > 0) {
            var chunkSize = maxChunkSizePerCall;
            if (chunkSize > size) {
                chunkSize = size;
            }

            var (n, errΔ3) = Ꮡfd.execIO((rune)'w', (ж<operation> o) => {
                var off = startpos + written;
                o.Value.o.OffsetHigh = (uint32)((off >> (int)(32)));
                o.Value.o.Offset = (uint32)off;
                var errΔ4 = Δsyscall.TransmitFile(Ꮡfd.Value.Sysfd, hsrc, (uint32)chunkSize, 0, o.of(operation.Ꮡo), nil, Δsyscall.TF_WRITE_BEHIND);
                if (errΔ4 != default!) {
                    return ((uint32)(0), errΔ4);
                }
                return ((uint32)chunkSize, default!);
            });
            if (errΔ3 != default!) {
                (written, err, handled) = (written, errΔ3, written > 0); goto ᒐdone;
            }

            size -= (int64)n;
            written += (int64)n;
        }

        (written, err, handled) = (written, default!, written > 0);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (written, Ꮡerr.ValueSlot, handled);
}

} // end poll_package
