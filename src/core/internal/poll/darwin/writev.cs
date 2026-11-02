// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
namespace go.@internal;

using io = io_package;
using Δruntime = runtime_package;
using Δsyscall = syscall_package;

partial class poll_package {

// Writev wraps the writev system call.
public static (int64, error) Writev(this ж<FD> Ꮡfd, ж<slice<slice<byte>>> Ꮡv) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();
        ref var v = ref Ꮡv.DerefOrNull();

        {
            var errΔ1 = Ꮡfd.writeLock(); if (errΔ1 != nil) {
                return (0, errΔ1);
            }
        }
        ᒐd1 = true;
        {
            var errΔ2 = fd.pd.prepareWrite(fd.isFile); if (errΔ2 != nil) {
                return (0, errΔ2);
            }
        }

        slice<Δsyscall.Iovec> iovecs = default;
        if (fd.iovecs != nil) {
            iovecs = fd.iovecs.ValueSlot;
        }
        // TODO: read from sysconf(_SC_IOV_MAX)? The Linux default is
        // 1024 and this seems conservative enough for now. Darwin's
        // UIO_MAXIOV also seems to be 1024.
        nint maxVec = 1024;
        if (Δruntime.GOOS == "aix"u8 || Δruntime.GOOS == "solaris"u8) {
            // IOV_MAX is set to XOPEN_IOV_MAX on AIX and Solaris.
            maxVec = 16;
        }

        int64 n = default;
        error err = default;
        while (len(v) > 0) {
            iovecs = iovecs[..0];
            foreach (var (_, chunk) in v) {
                if (len(chunk) == 0) {
                    continue;
                }
                iovecs = append(iovecs, newIovecWithBase(Ꮡ(chunk, 0)));
                if (fd.IsStream && len(chunk) > 1 << 30) {
                    iovecs[len(iovecs) - 1].SetLen(1 << 30);
                    break; // continue chunk on next writev
                }
                iovecs[len(iovecs) - 1].SetLen(len(chunk));
                if (len(iovecs) == maxVec) {
                    break;
                }
            }
            if (len(iovecs) == 0) {
                break;
            }
            if (fd.iovecs == nil) {
                fd.iovecs = @new<slice<Δsyscall.Iovec>>();
            }
            fd.iovecs.ValueSlot = iovecs; // cache

            uintptr wrote = default;
            (wrote, err) = writev(fd.Sysfd, iovecs);
            if (wrote == ~(uintptr)0) {
                wrote = 0;
            }
            TestHookDidWritev((nint)wrote);
            n += (int64)wrote;
            consume(ref (Ꮡv).DerefOrNull(), (int64)wrote);
            clear(iovecs);
            if (err != nil) {
                if (AreEqual(err, Δsyscall.EINTR)) {
                    continue;
                }
                if (AreEqual(err, Δsyscall.EAGAIN)) {
                    {
                        err = fd.pd.waitWrite(fd.isFile); if (err == nil) {
                            continue;
                        }
                    }
                }
                break;
            }
            if (n == 0) {
                err = io.ErrUnexpectedEOF;
                break;
            }
        }
        return (n, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡfd.writeUnlock(); ᒐ.Run(); }
}

} // end poll_package
