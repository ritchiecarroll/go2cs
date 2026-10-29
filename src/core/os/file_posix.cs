// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || (js && wasm) || wasip1 || windows
namespace go;

using testlog = @internal.testlog_package;
using runtime = runtime_package;
using syscall = syscall_package;
using time = time_package;
using @internal;
using fs = go.io.fs_package;
using go.io;

partial class os_package {

// Close closes the [File], rendering it unusable for I/O.
// On files that support [File.SetDeadline], any pending I/O operations will
// be canceled and return immediately with an [ErrClosed] error.
// Close will return an error if it has already been called.
public static error Close(this ж<File> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    if (Ꮡf == nil) {
        return ErrInvalid;
    }
    return f.@file.close();
}

// read reads up to len(b) bytes from the File.
// It returns the number of bytes read and an error, if any.
internal static (nint n, error err) read(this ж<File> Ꮡf, slice<byte> b) {
    nint n = default;
    error err = default;

    (n, err) = Ꮡf.of(File.Ꮡpfd).Read(b);
    runtime.KeepAlive(Ꮡf.OrTypedNil());
    return (n, err);
}

// pread reads len(b) bytes from the File starting at byte offset off.
// It returns the number of bytes read and the error, if any.
// EOF is signaled by a zero count with err set to nil.
internal static (nint n, error err) pread(this ж<File> Ꮡf, slice<byte> b, int64 off) {
    nint n = default;
    error err = default;

    (n, err) = Ꮡf.of(File.Ꮡpfd).Pread(b, off);
    runtime.KeepAlive(Ꮡf.OrTypedNil());
    return (n, err);
}

// write writes len(b) bytes to the File.
// It returns the number of bytes written and an error, if any.
internal static (nint n, error err) write(this ж<File> Ꮡf, slice<byte> b) {
    nint n = default;
    error err = default;

    (n, err) = Ꮡf.of(File.Ꮡpfd).Write(b);
    runtime.KeepAlive(Ꮡf.OrTypedNil());
    return (n, err);
}

// pwrite writes len(b) bytes to the File starting at byte offset off.
// It returns the number of bytes written and an error, if any.
internal static (nint n, error err) pwrite(this ж<File> Ꮡf, slice<byte> b, int64 off) {
    nint n = default;
    error err = default;

    (n, err) = Ꮡf.of(File.Ꮡpfd).Pwrite(b, off);
    runtime.KeepAlive(Ꮡf.OrTypedNil());
    return (n, err);
}

// syscallMode returns the syscall-specific mode bits from Go's portable mode bits.
internal static uint32 /*o*/ syscallMode(FileMode i) {
    uint32 o = default;

    o |= (uint32)((uint32)i.Perm());
    if ((FileMode)(i & ModeSetuid) != 0) {
        o |= (uint32)(syscall.S_ISUID);
    }
    if ((FileMode)(i & ModeSetgid) != 0) {
        o |= (uint32)(syscall.S_ISGID);
    }
    if ((FileMode)(i & ModeSticky) != 0) {
        o |= (uint32)(syscall.S_ISVTX);
    }
    // No mapping for Go's ModeTemporary (plan9 only).
    return o;
}

// See docs in file.go:Chmod.
internal static error chmod(@string name, FileMode mode) {
    @string longName = fixLongPath(name);
    var e = ignoringEINTR(() => syscall.Chmod(longName, syscallMode(mode)));
    if (e != nil) {
        return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "chmod"u8, Path: name, Err: e)));
    }
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string chmodˢ = "chmod"u8;

// See docs in file.go:(*File).Chmod.
internal static error chmod(this ж<File> Ꮡf, FileMode mode) {
    ref var f = ref Ꮡf.DerefOrNull();

    {
        var err = Ꮡf.checkValid(chmodˢ); if (err != nil) {
            return err;
        }
    }
    {
        var e = Ꮡf.of(File.Ꮡpfd).Fchmod(syscallMode(mode)); if (e != nil) {
            return f.wrapErr(chmodˢ, e);
        }
    }
    return default;
}

// Chown changes the numeric uid and gid of the named file.
// If the file is a symbolic link, it changes the uid and gid of the link's target.
// A uid or gid of -1 means to not change that value.
// If there is an error, it will be of type [*PathError].
//
// On Windows or Plan 9, Chown always returns the [syscall.EWINDOWS] or
// [syscall.EPLAN9] error, wrapped in [*PathError].
public static error Chown(@string name, nint uid, nint gid) {
    var e = ignoringEINTR(() => syscall.Chown(name, uid, gid));
    if (e != nil) {
        return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "chown"u8, Path: name, Err: e)));
    }
    return default;
}

// Lchown changes the numeric uid and gid of the named file.
// If the file is a symbolic link, it changes the uid and gid of the link itself.
// If there is an error, it will be of type [*PathError].
//
// On Windows, it always returns the [syscall.EWINDOWS] error, wrapped
// in [*PathError].
public static error Lchown(@string name, nint uid, nint gid) {
    var e = ignoringEINTR(() => syscall.Lchown(name, uid, gid));
    if (e != nil) {
        return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "lchown"u8, Path: name, Err: e)));
    }
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string chownˢ = "chown"u8;

// Chown changes the numeric uid and gid of the named file.
// If there is an error, it will be of type [*PathError].
//
// On Windows, it always returns the [syscall.EWINDOWS] error, wrapped
// in [*PathError].
public static error Chown(this ж<File> Ꮡf, nint uid, nint gid) {
    ref var f = ref Ꮡf.DerefOrNull();

    {
        var err = Ꮡf.checkValid(chownˢ); if (err != nil) {
            return err;
        }
    }
    {
        var e = Ꮡf.of(File.Ꮡpfd).Fchown(uid, gid); if (e != nil) {
            return f.wrapErr(chownˢ, e);
        }
    }
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string truncateˢ = "truncate"u8;

// Truncate changes the size of the file.
// It does not change the I/O offset.
// If there is an error, it will be of type [*PathError].
public static error Truncate(this ж<File> Ꮡf, int64 size) {
    ref var f = ref Ꮡf.DerefOrNull();

    {
        var err = Ꮡf.checkValid(truncateˢ); if (err != nil) {
            return err;
        }
    }
    {
        var e = Ꮡf.of(File.Ꮡpfd).Ftruncate(size); if (e != nil) {
            return f.wrapErr(truncateˢ, e);
        }
    }
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string syncˢ = "sync"u8;

// Sync commits the current contents of the file to stable storage.
// Typically, this means flushing the file system's in-memory copy
// of recently written data to disk.
public static error Sync(this ж<File> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    {
        var err = Ꮡf.checkValid(syncˢ); if (err != nil) {
            return err;
        }
    }
    {
        var e = Ꮡf.of(File.Ꮡpfd).Fsync(); if (e != nil) {
            return f.wrapErr(syncˢ, e);
        }
    }
    return default;
}

// Chtimes changes the access and modification times of the named
// file, similar to the Unix utime() or utimes() functions.
// A zero [time.Time] value will leave the corresponding file time unchanged.
//
// The underlying filesystem may truncate or round the values to a
// less precise time unit.
// If there is an error, it will be of type [*PathError].
public static error Chtimes(@string name, time.Time atime, time.Time mtime) {
    var utimes = chtimesUtimes(atime, mtime);
    {
        var e = syscall.UtimesNano(fixLongPath(name), utimes[0..]); if (e != nil) {
            return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "chtimes"u8, Path: name, Err: e)));
        }
    }
    return default;
}

internal static array<syscall.Timespec> chtimesUtimes(time.Time atime, time.Time mtime) {
    ref var utimes = ref heap(new array<syscall.Timespec>(2), out var Ꮡutimes);
    void set(nint i, time.Time t) {
        if (t.IsZero()) {
            Ꮡutimes.Value[i] = new syscall.Timespec(Sec: _UTIME_OMIT, Nsec: _UTIME_OMIT);
        } else {
            Ꮡutimes.Value[i] = syscall.NsecToTimespec(t.UnixNano());
        }
    }
    set(0, atime);
    set(1, mtime);
    return utimes.Clone();
}

// Chdir changes the current working directory to the file,
// which must be a directory.
// If there is an error, it will be of type [*PathError].
public static error Chdir(this ж<File> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    {
        var err = Ꮡf.checkValid(chdirˢ); if (err != nil) {
            return err;
        }
    }
    {
        var e = Ꮡf.of(File.Ꮡpfd).Fchdir(); if (e != nil) {
            return f.wrapErr(chdirˢ, e);
        }
    }
    {
        var log = testlog.Logger(); if (log != nil) {
            var (wd, err) = Getwd();
            if (err == nil) {
                log.Chdir(wd);
            }
        }
    }
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string setDeadlineˢ = "SetDeadline"u8;

// setDeadline sets the read and write deadline.
internal static error setDeadline(this ж<File> Ꮡf, time.Time t) {
    {
        var err = Ꮡf.checkValid(setDeadlineˢ); if (err != nil) {
            return err;
        }
    }
    return Ꮡf.of(File.Ꮡpfd).SetDeadline(t);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string setReadDeadlineˢ = "SetReadDeadline"u8;

// setReadDeadline sets the read deadline.
internal static error setReadDeadline(this ж<File> Ꮡf, time.Time t) {
    {
        var err = Ꮡf.checkValid(setReadDeadlineˢ); if (err != nil) {
            return err;
        }
    }
    return Ꮡf.of(File.Ꮡpfd).SetReadDeadline(t);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string setWriteDeadlineˢ = "SetWriteDeadline"u8;

// setWriteDeadline sets the write deadline.
internal static error setWriteDeadline(this ж<File> Ꮡf, time.Time t) {
    {
        var err = Ꮡf.checkValid(setWriteDeadlineˢ); if (err != nil) {
            return err;
        }
    }
    return Ꮡf.of(File.Ꮡpfd).SetWriteDeadline(t);
}

// checkValid checks whether f is valid for use.
// If not, it returns an appropriate error, perhaps incorporating the operation name op.
internal static error checkValid(this ж<File> Ꮡf, @string op) {
    if (Ꮡf == nil) {
        return ErrInvalid;
    }
    return default;
}

// ignoringEINTR makes a function call and repeats it if it returns an
// EINTR error. This appears to be required even though we install all
// signal handlers with SA_RESTART: see #22838, #38033, #38836, #40846.
// Also #20400 and #36644 are issues in which a signal handler is
// installed without setting SA_RESTART. None of these are the common case,
// but there are enough of them that it seems that we can't avoid
// an EINTR loop.
internal static error ignoringEINTR(Func<error> fn) {
    for (;;) {
        var err = fn();
        if (!AreEqual(err, syscall.EINTR)) {
            return err;
        }
    }
}

// ignoringEINTR2 is ignoringEINTR, but returning an additional value.
internal static (T, error) ignoringEINTR2<T>(Func<(T, error)> fn) {
    for (;;) {
        var (v, err) = fn();
        if (!AreEqual(err, syscall.EINTR)) {
            return (v, err);
        }
    }
}

} // end os_package
