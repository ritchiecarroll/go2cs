// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build aix || dragonfly || freebsd || (js && wasm) || wasip1 || linux || netbsd || openbsd || solaris
namespace go;

using byteorder = @internal.byteorder_package;
using goarch = @internal.goarch_package;
using Δio = io_package;
using runtime = runtime_package;
using Δsync = sync_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using @internal;
using fs = go.io.fs_package;
using go.sync;

partial class os_package {

// Auxiliary information if the File describes a directory
partial struct dirInfo {
    internal Δsync.Mutex mu;
    internal ж<slice<byte>> buf; // buffer for directory I/O
    internal nint nbuf;    // length of buf; return value from Getdirentries
    internal nint bufp;    // location of next record in buf.
}

internal static UntypedInt blockSize => 8192;

// The buffer must be at least a block long.
internal static ж<Δsync.Pool> ᏑdirBufPool = new StandardBox<Δsync.Pool>(new Δsync.Pool(
    New: () => {
        var buf = new slice<byte>(blockSize);
        return Ꮡ(buf);
    }
));
internal static ref Δsync.Pool dirBufPool => ref ᏑdirBufPool.Value;

internal static void close(this ref dirInfo d) {
    if (d.buf != nil) {
        ᏑdirBufPool.Put(d.buf.OrTypedNil());
        d.buf = default!;
    }
}

internal static (slice<@string> names, slice<DirEntry> dirents, slice<FileInfo> infos, error err) readdir(this ж<File> Ꮡf, nint n, readdirMode mode) {
    slice<@string> names = default!;
    slice<DirEntry> dirents = default!;
    slice<FileInfo> infos = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var f = ref Ꮡf.DerefOrNull();

        // If this file has no dirInfo, create one.
        var d = Ꮡf.of(File.Ꮡdirinfo).Load();
        if (d == nil) {
            d = @new<dirInfo>();
            Ꮡf.of(File.Ꮡdirinfo).Store(d);
        }
        d.of(dirInfo.Ꮡmu).Lock();
        var dʗ1 = d;
        defer(dʗ1.of(dirInfo.Ꮡmu).Unlock, ref ᒐ);
        if ((~d).buf == nil) {
            d.Value.buf = ᏑdirBufPool.Get()._<ж<slice<byte>>>();
        }
        // Change the meaning of n for the implementation below.
        //
        // The n above was for the public interface of "if n <= 0,
        // Readdir returns all the FileInfo from the directory in a
        // single slice".
        //
        // But below, we use only negative to mean looping until the
        // end and positive to mean bounded, with positive
        // terminating at 0.
        if (n == 0) {
            n = -1;
        }
        while (n != 0) {
            // Refill the buffer if necessary
            if ((~d).bufp >= (~d).nbuf) {
                d.Value.bufp = 0;
                error errno = default!;
                (d.Value.nbuf, errno) = Ꮡf.of(File.Ꮡpfd).ReadDirent((~d).buf.ValueSlot);
                runtime.KeepAlive(Ꮡf.OrTypedNil());
                if (errno != default!) {
                    (names, dirents, infos, err) = (names, dirents, infos, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "readdirent"u8, Path: f.name, Err: errno)))); goto ᒐdone;
                }
                if ((~d).nbuf <= 0) {
                    // Optimization: we can return the buffer to the pool, there is nothing else to read.
                    ᏑdirBufPool.Put((~d).buf.OrTypedNil());
                    d.Value.buf = default!;
                    break; // EOF
                }
            }
            // Drain the buffer
            var buf = ((~d).buf.ValueSlot).slice((~d).bufp, (~d).nbuf);
            var (reclen, ok) = direntReclen(buf);
            if (!ok || reclen > (uint64)len(buf)) {
                break;
            }
            var rec = buf.slice(0, (nint)(reclen));
            d.Value.bufp += (nint)reclen;
            (var ino, ok) = direntIno(rec);
            if (!ok) {
                break;
            }
            // When building to wasip1, the host runtime might be running on Windows
            // or might expose a remote file system which does not have the concept
            // of inodes. Therefore, we cannot make the assumption that it is safe
            // to skip entries with zero inodes.
            // Some Linux filesystems (old XFS, FUSE) can return valid files with zero inodes.
            if (ino == 0 && runtime.GOOS != "linux"u8 && runtime.GOOS != "wasip1"u8) {
                continue;
            }
            const uint64 namoff = /* uint64(unsafe.Offsetof(syscall.Dirent{}.Name)) */ 19;
            (var namlen, ok) = direntNamlen(rec);
            if (!ok || namoff + namlen > (uint64)len(rec)) {
                break;
            }
            var name = rec.slice((nint)(namoff), (nint)(namoff + namlen));
            foreach (var (i, c) in name) {
                if (c == 0) {
                    name = name.slice(0, i);
                    break;
                }
            }
            // Check for useless names before allocating a string.
            if (((sstring)name) == "."u8 || ((sstring)name) == ".."u8) {
                continue;
            }
            if (n > 0) {
                // see 'n == 0' comment above
                n--;
            }
            if (mode == readdirName){
                names = append(names, ((@string)name));
            } else 
            if (mode == readdirDirEntry){
                var (de, errΔ1) = newUnixDirent(f.name, ((@string)name), direntType(rec));
                if (IsNotExist(errΔ1)) {
                    // File disappeared between readdir and stat.
                    // Treat as if it didn't exist.
                    continue;
                }
                if (errΔ1 != default!) {
                    (names, dirents, infos, err) = (default!, dirents, default!, errΔ1); goto ᒐdone;
                }
                dirents = append(dirents, de);
            } else {
                var (info, errΔ2) = lstat(f.name + "/"u8 + ((@string)name));
                if (IsNotExist(errΔ2)) {
                    // File disappeared between readdir + stat.
                    // Treat as if it didn't exist.
                    continue;
                }
                if (errΔ2 != default!) {
                    (names, dirents, infos, err) = (default!, default!, infos, errΔ2); goto ᒐdone;
                }
                infos = append(infos, info);
            }
        }
        if (n > 0 && len(names) + len(dirents) + len(infos) == 0) {
            (names, dirents, infos, err) = (default!, default!, default!, Δio.EOF); goto ᒐdone;
        }
        (names, dirents, infos, err) = (names, dirents, infos, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (names, dirents, infos, err);
}

// readInt returns the size-bytes unsigned integer in native byte order at offset off.
internal static (uint64 u, bool ok) readInt(slice<byte> b, uintptr off, uintptr size) {
    if (len(b) < (nint)(off + size)) {
        return (0, false);
    }
    if (goarch.BigEndian) {
        return (readIntBE(b.slice((nint)(off)), size), true);
    }
    return (readIntLE(b.slice((nint)(off)), size), true);
}

internal static uint64 readIntBE(slice<byte> b, uintptr size) {
    var exprᴛ1 = size;
    if (exprᴛ1 == 1) {
        return (uint64)b[0];
    }
    if (exprᴛ1 == 2) {
        return (uint64)byteorder.BEUint16(b);
    }
    if (exprᴛ1 == 4) {
        return (uint64)byteorder.BEUint32(b);
    }
    if (exprᴛ1 == 8) {
        return (uint64)byteorder.BEUint64(b);
    }
    { /* default: */
        throw panic("syscall: readInt with unsupported size");
    }

}

internal static uint64 readIntLE(slice<byte> b, uintptr size) {
    var exprᴛ1 = size;
    if (exprᴛ1 == 1) {
        return (uint64)b[0];
    }
    if (exprᴛ1 == 2) {
        return (uint64)byteorder.LEUint16(b);
    }
    if (exprᴛ1 == 4) {
        return (uint64)byteorder.LEUint32(b);
    }
    if (exprᴛ1 == 8) {
        return (uint64)byteorder.LEUint64(b);
    }
    { /* default: */
        throw panic("syscall: readInt with unsupported size");
    }

}

} // end os_package
