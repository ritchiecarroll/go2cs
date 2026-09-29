// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || (js && wasm) || wasip1
namespace go;

using poll = @internal.poll_package;
using unix = @internal.syscall.unix_package;
using fs = go.io.fs_package;
using runtime = runtime_package;
using atomic = go.sync.atomic_package;
using syscall = syscall_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for go:linkname
using @internal;
using @internal.syscall;
using go.io;
using go.sync;

partial class os_package {

internal static UntypedInt _UTIME_OMIT => /* unix.UTIME_OMIT */ -2;

// fixLongPath is a noop on non-Windows platforms.
internal static @string fixLongPath(@string path) {
    return path;
}

internal static error rename(@string oldname, @string newname) {
    var (fi, err) = Lstat(newname);
    if (err == default! && fi.IsDir()) {
        // There are two independent errors this function can return:
        // one for a bad oldname, and one for a bad newname.
        // At this point we've determined the newname is bad.
        // But just in case oldname is also bad, prioritize returning
        // the oldname error because that's what we did historically.
        // However, if the old name and new name are not the same, yet
        // they refer to the same file, it implies a case-only
        // rename on a case-insensitive filesystem, which is ok.
        {
            var (ofi, errΔ1) = Lstat(oldname); if (errΔ1 != default!){
                {
                    var (pe, ok) = errΔ1._<ж<PathError>>(ᐧ); if (ok) {
                        errΔ1 = pe.Value.Err;
                    }
                }
                return new LinkErrorжerror(Ꮡ(new LinkError("rename"u8, oldname, newname, errΔ1)));
            } else 
            if (newname == oldname || !SameFile(fi, ofi)) {
                return new LinkErrorжerror(Ꮡ(new LinkError("rename"u8, oldname, newname, syscall.EEXIST)));
            }
        }
    }
    err = ignoringEINTR(() => syscall.Rename(oldname, newname));
    if (err != default!) {
        return new LinkErrorжerror(Ꮡ(new LinkError("rename"u8, oldname, newname, err)));
    }
    return default!;
}

// file is the real representation of *File.
// The extra level of indirection ensures that no clients of os
// can overwrite this data, which could cause the finalizer
// to close the wrong file descriptor.
[GoType] partial struct @file {
    internal poll.FD pfd;
    internal @string name;
    internal atomic.Pointer<dirInfo> dirinfo; // nil unless directory being read
    internal bool nonblock;                    // whether we set nonblocking mode
    internal bool stdoutOrErr;                    // whether this is stdout or stderr
    internal bool appendMode;                    // whether file is opened for appending
}

// Fd returns the integer Unix file descriptor referencing the open file.
// If f is closed, the file descriptor becomes invalid.
// If f is garbage collected, a finalizer may close the file descriptor,
// making it invalid; see [runtime.SetFinalizer] for more information on when
// a finalizer might be run. On Unix systems this will cause the [File.SetDeadline]
// methods to stop working.
// Because file descriptors can be reused, the returned file descriptor may
// only be closed through the [File.Close] method of f, or by its finalizer during
// garbage collection. Otherwise, during garbage collection the finalizer
// may close an unrelated file descriptor with the same (reused) number.
//
// As an alternative, see the f.SyscallConn method.
public static uintptr Fd(this ж<File> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    if (Ꮡf == nil) {
        return ~((uintptr)0);
    }
    // If we put the file descriptor into nonblocking mode,
    // then set it to blocking mode before we return it,
    // because historically we have always returned a descriptor
    // opened in blocking mode. The File will continue to work,
    // but any blocking operation will tie up a thread.
    if (f.nonblock) {
        Ꮡf.of(File.Ꮡpfd).SetBlocking();
    }
    return (uintptr)f.pfd.Sysfd;
}

// NewFile returns a new File with the given file descriptor and
// name. The returned value will be nil if fd is not a valid file
// descriptor. On Unix systems, if the file descriptor is in
// non-blocking mode, NewFile will attempt to return a pollable File
// (one for which the SetDeadline methods work).
//
// After passing it to NewFile, fd may become invalid under the same
// conditions described in the comments of the Fd method, and the same
// constraints apply.
public static ж<File> NewFile(uintptr fd, @string name) {
    nint fdi = (nint)fd;
    if (fdi < 0) {
        return default!;
    }
    var (flags, err) = unix.Fcntl(fdi, syscall.F_GETFL, 0);
    if (err != default!) {
        flags = 0;
    }
    var f = newFile(fdi, name, kindNewFile, unix.HasNonblockFlag(flags));
    f.Value.appendMode = (nint)(flags & (nint)syscall.O_APPEND) != 0;
    return f;
}

// net_newUnixFile is a hidden entry point called by net.conn.File.
// This is used so that a nonblocking network connection will become
// blocking if code calls the Fd method. We don't want that for direct
// calls to NewFile: passing a nonblocking descriptor to NewFile should
// remain nonblocking if you get it back using Fd. But for net.conn.File
// the call to NewFile is hidden from the user. Historically in that case
// the Fd method has returned a blocking descriptor, and we want to
// retain that behavior because existing code expects it and depends on it.
//
//go:linkname net_newUnixFile net.newUnixFile
public static ж<File> net_newUnixFile(nint fd, @string name) {
    if (fd < 0) {
        throw panic("invalid FD");
    }
    return newFile(fd, name, kindSock, true);
}

[GoType("num:nint")] partial struct newFileKind;

internal static newFileKind kindNewFile => /* iota */ 0;
internal static newFileKind kindOpenFile => 1;
internal static newFileKind kindPipe => 2;
internal static newFileKind kindSock => 3;
internal static newFileKind kindNoPoll => 4;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fileˢ = "file"u8;

// newFile is like NewFile, but if called from OpenFile or Pipe
// (as passed in the kind parameter) it tries to add the file to
// the runtime poller.
internal static ж<File> newFile(nint fd, @string name, newFileKind kind, bool nonBlocking) {
    var f = Ꮡ(new File(Ꮡ(new @file(
        pfd: new poll.FD(
            Sysfd: fd,
            IsStream: true,
            ZeroReadIsEOF: true
        ),
        name: name,
        stdoutOrErr: fd == 1 || fd == 2
    ))
    ));
    var pollable = kind == kindOpenFile || kind == kindPipe || kind == kindSock || nonBlocking;
    // Things like regular files and FIFOs in kqueue on *BSD/Darwin
    // may not work properly (or accurately according to its manual).
    // As a result, we should avoid adding those to the kqueue-based
    // netpoller. Check out #19093, #24164, and #66239 for more contexts.
    //
    // If the fd was passed to us via any path other than OpenFile,
    // we assume those callers know what they were doing, so we won't
    // perform this check and allow it to be added to the kqueue.
    if (kind == kindOpenFile) {
        var exprᴛ1 = runtime.GOOS;
        if (exprᴛ1 == "darwin"u8 || exprᴛ1 == "ios"u8 || exprᴛ1 == "dragonfly"u8 || exprᴛ1 == "freebsd"u8 || exprᴛ1 == "netbsd"u8 || exprᴛ1 == "openbsd"u8) {
            ref var st = ref heap(new syscall.Stat_t(), out var Ꮡst);
            var err = ignoringEINTR(() => syscall.Fstat(fd, Ꮡst));
            var typ = (uint16)(st.Mode & (uint16)syscall.S_IFMT);
            if (err == default! && (typ == syscall.S_IFREG || typ == syscall.S_IFDIR)) {
                // Don't try to use kqueue with regular files on *BSDs.
                // On FreeBSD a regular file is always
                // reported as ready for writing.
                // On Dragonfly, NetBSD and OpenBSD the fd is signaled
                // only once as ready (both read and write).
                // Issue 19093.
                // Also don't add directories to the netpoller.
                pollable = false;
            }
            if ((runtime.GOOS == "darwin"u8 || runtime.GOOS == "ios"u8) && typ == syscall.S_IFIFO) {
                // In addition to the behavior described above for regular files,
                // on Darwin, kqueue does not work properly with fifos:
                // closing the last writer does not cause a kqueue event
                // for any readers. See issue #24164.
                pollable = false;
            }
        }

    }
    var clearNonBlock = false;
    if (pollable) {
        // The descriptor is already in non-blocking mode.
        // We only set f.nonblock if we put the file into
        // non-blocking mode.
        if (nonBlocking){
            // See the comments on net_newUnixFile.
            if (kind == kindSock) {
                f.Value.nonblock = true; // tell Fd to return blocking descriptor
            }
        } else 
        {
            var err = syscall.SetNonblock(fd, true); if (err == default!){
                f.Value.nonblock = true;
                clearNonBlock = true;
            } else {
                pollable = false;
            }
        }
    }
    // An error here indicates a failure to register
    // with the netpoll system. That can happen for
    // a file descriptor that is not supported by
    // epoll/kqueue; for example, disk files on
    // Linux systems. We assume that any real error
    // will show up in later I/O.
    // We do restore the blocking behavior if it was set by us.
    {
        var pollErr = f.of(File.Ꮡpfd).Init(fileˢ, pollable); if (pollErr != default! && clearNonBlock) {
            {
                var err = syscall.SetNonblock(fd, false); if (err == default!) {
                    f.Value.nonblock = false;
                }
            }
        }
    }
    runtime.SetFinalizer((~f).@file.OrTypedNil(), ((Func<ж<@file>, error>)(close)));
    return f;
}

internal static void sigpipe() {
    runtime.os_sigpipe();
}

// implemented in package runtime

// epipecheck raises SIGPIPE if we get an EPIPE error on standard
// output or standard error. See the SIGPIPE docs in os/signal, and
// issue 11845.
internal static void epipecheck(ref File @file, error e) {
    if (AreEqual(e, syscall.EPIPE) && @file.stdoutOrErr) {
        sigpipe();
    }
}

// DevNull is the name of the operating system's “null device.”
// On Unix-like systems, it is "/dev/null"; on Windows, "NUL".
public static readonly @string DevNull = "/dev/null"u8;

// openFileNolog is the Unix implementation of OpenFile.
// Changes here should be reflected in openDirAt and openDirNolog, if relevant.
internal static (ж<File>, error) openFileNolog(@string name, nint flag, FileMode perm) {
    var setSticky = false;
    if (!supportsCreateWithStickyBit && (nint)(flag & O_CREATE) != 0 && (FileMode)(perm & ModeSticky) != 0) {
        {
            var (_, err) = Stat(name); if (IsNotExist(err)) {
                setSticky = true;
            }
        }
    }
    nint r = default!;
    ref var s = ref heap(new poll.SysFile(), out var Ꮡs);
    ref var e = ref heap<error>(out var Ꮡe);
    // We have to check EINTR here, per issues 11180 and 39237.
    ignoringEINTR(() => {
        (r, Ꮡs.Value, Ꮡe.ValueSlot) = open(name, (nint)(flag | (nint)syscall.O_CLOEXEC), syscallMode(perm));
        return Ꮡe.ValueSlot;
    });
    if (e != default!) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "open"u8, Path: name, Err: e))));
    }
    // open(2) itself won't handle the sticky bit on *BSD and Solaris
    if (setSticky) {
        setStickyBit(name);
    }
    // There's a race here with fork/exec, which we are
    // content to live with. See ../syscall/exec_unix.go.
    if (!supportsCloseOnExec) {
        syscall.CloseOnExec(r);
    }
    var f = newFile(r, name, kindOpenFile, unix.HasNonblockFlag(flag));
    f.Value.pfd.SysFile = s;
    return (f, default!);
}

internal static (ж<File>, error) openDirNolog(@string name) {
    nint r = default!;
    ref var s = ref heap(new poll.SysFile(), out var Ꮡs);
    ref var e = ref heap<error>(out var Ꮡe);
    ignoringEINTR(() => {
        (r, Ꮡs.Value, Ꮡe.ValueSlot) = open(name, (nint)((nint)(nint)(O_RDONLY | (nint)syscall.O_CLOEXEC) | (nint)syscall.O_DIRECTORY), 0);
        return Ꮡe.ValueSlot;
    });
    if (e != default!) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "open"u8, Path: name, Err: e))));
    }
    if (!supportsCloseOnExec) {
        syscall.CloseOnExec(r);
    }
    var f = newFile(r, name, kindNoPoll, false);
    f.Value.pfd.SysFile = s;
    return (f, default!);
}

internal static error close(this ж<@file> Ꮡfile) {
    ref var @file = ref Ꮡfile.DerefOrNull();

    if (Ꮡfile == nil) {
        return syscall.EINVAL;
    }
    {
        var info = Ꮡfile.of(@file.Ꮡdirinfo).Swap(nil); if (info != nil) {
            info.close();
        }
    }
    error err = default!;
    {
        var e = Ꮡfile.of(@file.Ꮡpfd).Close(); if (e != default!) {
            if (AreEqual(e, poll.ErrFileClosing)) {
                e = ErrClosed;
            }
            err = new fs.PathErrorжerror(Ꮡ(new PathError(Op: "close"u8, Path: @file.name, Err: e)));
        }
    }
    // no need for a finalizer anymore
    runtime.SetFinalizer(Ꮡfile.OrTypedNil(), default!);
    return err;
}

// seek sets the offset for the next Read or Write on file to offset, interpreted
// according to whence: 0 means relative to the origin of the file, 1 means
// relative to the current offset, and 2 means relative to the end.
// It returns the new offset and an error, if any.
internal static (int64 ret, error err) seek(this ж<File> Ꮡf, int64 offset, nint whence) {
    int64 ret = default!;
    error err = default!;

    {
        var info = Ꮡf.of(File.Ꮡdirinfo).Swap(nil); if (info != nil) {
            // Free cached dirinfo, so we allocate a new one if we
            // access this file as a directory again. See #35767 and #37161.
            info.close();
        }
    }
    (ret, err) = Ꮡf.of(File.Ꮡpfd).Seek(offset, whence);
    runtime.KeepAlive(Ꮡf.OrTypedNil());
    return (ret, err);
}

// Truncate changes the size of the named file.
// If the file is a symbolic link, it changes the size of the link's target.
// If there is an error, it will be of type *PathError.
public static error Truncate(@string name, int64 size) {
    var e = ignoringEINTR(() => syscall.Truncate(name, size));
    if (e != default!) {
        return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "truncate"u8, Path: name, Err: e)));
    }
    return default!;
}

// Remove removes the named file or (empty) directory.
// If there is an error, it will be of type *PathError.
public static error Remove(@string name) {
    // System call interface forces us to know
    // whether name is a file or directory.
    // Try both: it is cheaper on average than
    // doing a Stat plus the right one.
    var e = ignoringEINTR(() => syscall.Unlink(name));
    if (e == default!) {
        return default!;
    }
    var e1 = ignoringEINTR(() => syscall.Rmdir(name));
    if (e1 == default!) {
        return default!;
    }
    // Both failed: figure out which error to return.
    // OS X and Linux differ on whether unlink(dir)
    // returns EISDIR, so can't use that. However,
    // both agree that rmdir(file) returns ENOTDIR,
    // so we can use that to decide which error is real.
    // Rmdir might also return ENOTDIR if given a bad
    // file path, like /etc/passwd/foo, but in that case,
    // both errors will be ENOTDIR, so it's okay to
    // use the error from unlink.
    if (!AreEqual(e1, syscall.ENOTDIR)) {
        e = e1;
    }
    return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "remove"u8, Path: name, Err: e)));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tmpdirˢ = "TMPDIR"u8;
internal static readonly @string dataLocalTmpˢ = "/data/local/tmp"u8;
internal static readonly @string tmpˢ = "/tmp"u8;

internal static @string tempDir() {
    @string dir = Getenv(tmpdirˢ);
    if (dir == ""u8) {
        if (runtime.GOOS == "android"u8){
            dir = dataLocalTmpˢ;
        } else {
            dir = tmpˢ;
        }
    }
    return dir;
}

// Link creates newname as a hard link to the oldname file.
// If there is an error, it will be of type *LinkError.
public static error Link(@string oldname, @string newname) {
    var e = ignoringEINTR(() => syscall.Link(oldname, newname));
    if (e != default!) {
        return new LinkErrorжerror(Ꮡ(new LinkError("link"u8, oldname, newname, e)));
    }
    return default!;
}

// Symlink creates newname as a symbolic link to oldname.
// On Windows, a symlink to a non-existent oldname creates a file symlink;
// if oldname is later created as a directory the symlink will not work.
// If there is an error, it will be of type *LinkError.
public static error Symlink(@string oldname, @string newname) {
    var e = ignoringEINTR(() => syscall.Symlink(oldname, newname));
    if (e != default!) {
        return new LinkErrorжerror(Ꮡ(new LinkError("symlink"u8, oldname, newname, e)));
    }
    return default!;
}

internal static (@string, error) readlink(@string name) {
    for (nint len = 128; ᐧ ; len *= 2) {
        var b = new slice<byte>(len);
        var bʗ1 = b;
        var (n, err) = ignoringEINTR2((nint, error) () => {
            var (ᴛ1, ᴛ2) = syscall.Readlink(name, bʗ1);
            return fixCount(ᴛ1, ᴛ2);
        });
        // buffer too small
        if ((runtime.GOOS == "aix"u8 || runtime.GOOS == "wasip1"u8) && AreEqual(err, syscall.ERANGE)) {
            continue;
        }
        if (err != default!) {
            return ("", new fs.PathErrorжerror(Ꮡ(new PathError(Op: "readlink"u8, Path: name, Err: err))));
        }
        if (n < len) {
            return (((@string)(b.slice(0, n))), default!);
        }
    }
}

[GoType] partial struct unixDirent {
    internal @string parent;
    internal @string name;
    internal FileMode typ;
    internal FileInfo info;
}

[GoRecv] internal static @string Name(this ref unixDirent d) {
    return d.name;
}

[GoRecv] internal static bool IsDir(this ref unixDirent d) {
    return d.typ.IsDir();
}

[GoRecv] internal static FileMode Type(this ref unixDirent d) {
    return d.typ;
}

[GoRecv] internal static (FileInfo, error) Info(this ref unixDirent d) {
    if (d.info != default!) {
        return (d.info, default!);
    }
    return lstat(d.parent + "/"u8 + d.name);
}

internal static @string String(this ж<unixDirent> Ꮡd) {
    return fs.FormatDirEntry(new unixDirentжDirEntry(Ꮡd));
}

internal static (DirEntry, error) newUnixDirent(@string parent, @string name, FileMode typ) {
    var ude = Ꮡ(new unixDirent(
        parent: parent,
        name: name,
        typ: typ
    ));
    if (typ != ~((fs.FileMode)((fs.FileMode)0)) && !testingForceReadDirLstat) {
        return (new unixDirentжDirEntry(ude), default!);
    }
    var (info, err) = lstat(parent + "/"u8 + name);
    if (err != default!) {
        return (default!, err);
    }
    ude.Value.typ = info.Mode().Type();
    ude.Value.info = info;
    return (new unixDirentжDirEntry(ude), default!);
}

} // end os_package
