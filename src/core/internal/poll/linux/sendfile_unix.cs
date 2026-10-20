// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || linux || solaris
namespace go.@internal;

using io = io_package;
using Δruntime = runtime_package;
using Δsyscall = syscall_package;

partial class poll_package {

// SendFile wraps the sendfile system call.
//
// It copies data from src (a file descriptor) to dstFD,
// starting at the current position of src.
// It updates the current position of src to after the
// copied data.
//
// If size is zero, it copies the rest of src.
// Otherwise, it copies up to size bytes.
//
// The handled return parameter indicates whether SendFile
// was able to handle some or all of the operation.
// If handled is false, sendfile was unable to perform the copy,
// has not modified the source or destination,
// and the caller should perform the copy using a fallback implementation.
public static (int64 n, error err, bool handled) SendFile(ж<FD> ᏑdstFD, uintptr src, int64 size) {
    int64 n = default!;
    error err = default!;
    bool handled = default!;

    {
        @string goos = Δruntime.GOOS; if (goos == "linux"u8 || goos == "android"u8) {
            // Linux's sendfile doesn't require any setup:
            // It sends from the current position of the source file and
            // updates the position of the source after sending.
            return sendFile(ᏑdstFD, (nint)src, nil, size);
        }
    }

    // Non-Linux sendfile implementations don't use the current position of the source file,
    // so we need to look up the position, pass it explicitly, and adjust it after
    // sendfile returns.
    (var start, err) = ignoringEINTR2((int64, error) () => Δsyscall.Seek((nint)src, 0, io.SeekCurrent));
    if (err != default!) {
        return (0, err, false);
    }

    ref var pos = ref heap<int64>(out var Ꮡpos);
    pos = start;
    (n, err, handled) = sendFile(ᏑdstFD, (nint)src, Ꮡpos, size);
    if (n > 0) {
        ignoringEINTR2((int64, error) () => Δsyscall.Seek((nint)src, start + n, io.SeekStart));
    }
    return (n, err, handled);
}

// sendFile wraps the sendfile system call.
internal static (int64 written, error err, bool handled) sendFile(ж<FD> ᏑdstFD, nint src, ж<int64> Ꮡoffset, int64 size) {
    int64 written = default!;
    error err = default!;
    bool handled = default!;
    GoFrame ᒐ = default;
    try {
        ref var dstFD = ref ᏑdstFD.DerefOrNull();

        defer(() => {
            TestHookDidSendFile(ᏑdstFD, (uintptr)src, written, err, handled);
        }, ref ᒐ);
        {
            var errΔ1 = ᏑdstFD.writeLock(); if (errΔ1 != default!) {
                (written, err, handled) = (0, errΔ1, false); goto ᒐdone;
            }
        }
        defer(ᏑdstFD.writeUnlock, ref ᒐ);

        {
            var errΔ2 = dstFD.pd.prepareWrite(dstFD.isFile); if (errΔ2 != default!) {
                (written, err, handled) = (0, errΔ2, false); goto ᒐdone;
            }
        }

        nint dst = dstFD.Sysfd;
        while (ᐧ) {
            // Some platforms support passing 0 to read to the end of the source,
            // but all platforms support just writing a large value.
            //
            // Limit the maximum size to fit in an int32, to avoid any possible overflow.
            nint chunk = (nint)(2147483648L - 1);
            if (size > 0) {
                chunk = (nint)min(size - written, (int64)chunk);
            }
            nint n = default!;
            (n, err) = sendFileChunk(dst, src, Ꮡoffset, chunk, written);
            if (n > 0) {
                written += (int64)n;
            }
            var exprᴛ1 = err;
            if (AreEqual(exprᴛ1, default!)) {
                // We're done if sendfile copied no bytes
                // (we're at the end of the source)
                // or if we have a size limit and have reached it.
                //
                // If sendfile copied some bytes and we don't have a size limit,
                // try again to see if there is more data to copy.
                if (n == 0 || (size > 0 && written >= size)) {
                    (written, err, handled) = (written, default!, true); goto ᒐdone;
                }
            }
            else if (AreEqual(exprᴛ1, Δsyscall.EAGAIN)) {
                // *BSD and Darwin can return EAGAIN with n > 0,
                // so check to see if the write has completed.
                // So far as we know all other platforms only
                // return EAGAIN when n == 0, but checking is harmless.
                if (size > 0 && written >= size) {
                    (written, err, handled) = (written, default!, true); goto ᒐdone;
                }
                {
                    err = dstFD.pd.waitWrite(dstFD.isFile); if (err != default!) {
                        (written, err, handled) = (written, err, true); goto ᒐdone;
                    }
                }
            }
            else if (AreEqual(exprᴛ1, Δsyscall.EINTR)) {
                // Retry.
            }
            else if (AreEqual(exprᴛ1, Δsyscall.ENOSYS) || AreEqual(exprᴛ1, Δsyscall.EOPNOTSUPP) || AreEqual(exprᴛ1, Δsyscall.EINVAL)) {
                // ENOSYS indicates no kernel support for sendfile.
                // EINVAL indicates a FD type that does not support sendfile.
                //
                // On Linux, copy_file_range can return EOPNOTSUPP when copying
                // to a NFS file (issue #40731); check for it here just in case.
                (written, err, handled) = (written, err, written > 0); goto ᒐdone;
            }
            else { /* default: */
                // We want to handle ENOTSUP like EOPNOTSUPP.
                // It's a pain to put it as a switch case
                // because on Linux systems ENOTSUP == EOPNOTSUPP,
                // so the compiler complains about a duplicate case.
                if (AreEqual(err, Δsyscall.ENOTSUP)) {
                    (written, err, handled) = (written, err, written > 0); goto ᒐdone;
                }

                // Not a retryable error.
                (written, err, handled) = (written, err, true); goto ᒐdone;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (written, err, handled);
}

internal static (nint n, error err) sendFileChunk(nint dst, nint src, ж<int64> Ꮡoffset, nint size, int64 written) {
    nint n = default!;
    error err = default!;
    ref var offset = ref Ꮡoffset.DerefOrNull();

    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "linux"u8 || exprᴛ1 == "android"u8) {
        // The offset is always nil on Linux.
        (n, err) = Δsyscall.Sendfile(dst, src, Ꮡoffset, size);
    }
    else if (exprᴛ1 == "solaris"u8 || exprᴛ1 == "illumos"u8) {
        // Trust the offset, not the return value from sendfile.
        var start = offset;
        (n, err) = Δsyscall.Sendfile(dst, src, Ꮡoffset, size);
        n = (nint)(offset - start);
        // A quirk on Solaris/illumos: sendfile claims to support out_fd
        // as a regular file but returns EINVAL when the out_fd
        // is not a socket of SOCK_STREAM, while it actually sends
        // out data anyway and updates the file offset.
        //
        // Another quirk: sendfile transfers data and returns EINVAL when being
        // asked to transfer bytes more than the actual file size. For instance,
        // the source file is wrapped in an io.LimitedReader with larger size
        // than the actual file size.
        //
        // To handle these cases we ignore EINVAL if any call to sendfile was
        // able to send data.
        if (AreEqual(err, Δsyscall.EINVAL) && (n > 0 || written > 0)) {
            err = default!;
        }
    }
    else { /* default: */
        var start = offset;
        (n, err) = Δsyscall.Sendfile(dst, src, Ꮡoffset, size);
        if (n > 0) {
            // The BSD implementations of syscall.Sendfile don't
            // update the offset parameter (despite it being a *int64).
            //
            // Trust the return value from sendfile, not the offset.
            offset = start + (int64)n;
        }
    }
    return (n, err);
}

} // end poll_package
