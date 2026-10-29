// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using filepathlite = @internal.filepathlite_package;
using godebug = @internal.godebug_package;
using poll = @internal.poll_package;
using windows = @internal.syscall.windows_package;
using runtime = runtime_package;
using Δsync = sync_package;
using atomic = go.sync.atomic_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.syscall;
using fs = go.io.fs_package;
using go.io;
using go.sync;

partial class os_package {

// This matches the value in syscall/syscall_windows.go.
internal static UntypedInt _UTIME_OMIT => -1;

// file is the real representation of *File.
// The extra level of indirection ensures that no clients of os
// can overwrite this data, which could cause the finalizer
// to close the wrong file descriptor.
partial struct @file {
    internal poll.FD pfd;
    internal @string name;
    internal atomic.Pointer<dirInfo> dirinfo; // nil unless directory being read
    internal bool appendMode;                 // whether file is opened for appending
}

// fd is the Windows implementation of Fd.
internal static uintptr fd(this ж<File> Ꮡfile) {
    ref var @file = ref Ꮡfile.DerefOrNull();

    if (Ꮡfile == nil) {
        return (uintptr)syscall.InvalidHandle;
    }
    // Try to disassociate the file from the runtime poller.
    // File.Fd doesn't return an error, so we don't have a way to
    // report it. We just ignore it. It's up to the caller to call
    // it when there are no concurrent IO operations.
    _ = Ꮡfile.of(File.Ꮡpfd).DisassociateIOCP();
    return (uintptr)@file.pfd.Sysfd;
}

// newFileKind describes the kind of file to newFile.
partial struct newFileKind /*num:nint*/;

// kindNewFile means that the descriptor was passed to us via NewFile.
internal static newFileKind kindNewFile => /* iota */ 0;
// kindOpenFile means that the descriptor was opened using
// Open, Create, or OpenFile.
internal static newFileKind kindOpenFile => 1;
// kindPipe means that the descriptor was opened using Pipe.
internal static newFileKind kindPipe => 2;
// kindSock means that the descriptor is a network file descriptor
// that was created from net package and was opened using net_newUnixFile.
internal static newFileKind kindSock => 3;
// kindConsole means that the descriptor is a console handle.
internal static newFileKind kindConsole => 4;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fileˢ = "file"u8;
internal static readonly @string consoleˢ = "console"u8;
internal static readonly @string pipeˢ = "pipe"u8;
internal static readonly @string fileNetˢ = "file+net"u8;

// newFile returns a new File with the given file handle and name.
// Unlike NewFile, it does not check that h is syscall.InvalidHandle.
// If nonBlocking is true, it tries to add the file to the runtime poller.
internal static ж<File> newFile(syscallꓸHandle h, @string name, newFileKind kind, bool nonBlocking) {
    @string typ = fileˢ;
    switch (kind) {
    case var ᴛ when ᴛ == kindNewFile || ᴛ == kindOpenFile: {
        var (t, err) = syscall.GetFileType(h);
        if (err != nil || t == syscall.FILE_TYPE_CHAR) {
            ref var m = ref heap(new uint32(), out var Ꮡm);
            if (syscall.GetConsoleMode(h, Ꮡm) == nil) {
                typ = consoleˢ;
                // Console handles are always blocking.
                break;
            }
        } else if (t == syscall.FILE_TYPE_PIPE) {
            typ = pipeˢ;
        }
        // NewFile doesn't know if a handle is blocking or non-blocking,
        // so we try to detect that here. This call may block/ if the handle
        // is blocking and there is an outstanding I/O operation.
        //
        // Avoid doing this for Stdin, which is almost always blocking and might
        // be in use by other process when the "os" package is initializing.
        // See go.dev/issue/75949 and go.dev/issue/76391.
        if (kind == kindNewFile && h != syscall.Stdin) {
            (nonBlocking, _) = windows.IsNonblock(h);
        }
        break;
    }
    case var ᴛ when ᴛ == kindPipe: {
        typ = pipeˢ;
        break;
    }
    case var ᴛ when ᴛ == kindSock: {
        typ = fileNetˢ;
        break;
    }
    case var ᴛ when ᴛ == kindConsole: {
        typ = consoleˢ;
        break;
    }
    default: {
        throw panic("newFile with unknown kind");
        break;
    }}

    // Completion notification modes are shared by all handles to the file
    // object. Preserve them for handles passed to NewFile, since other users
    // of the file object may rely on those modes. See go.dev/issue/80979.
    var f = Ꮡ(new File(Ꮡ(new @file(
        pfd: new poll.FD(
            Sysfd: h,
            IsStream: true,
            ZeroReadIsEOF: true,
            KeepFileCompletionModes: kind == kindNewFile
        ),
        name: name
    ))
    ));
    runtime.SetFinalizer(f.@file.OrTypedNil(), ((Func<ж<@file>, error>)(close)));

    // Ignore initialization errors.
    // Assume any problems will show up in later I/O.
    f.of(File.Ꮡpfd).Init(typ, nonBlocking);
    return f;
}

// newConsoleFile creates new File that will be used as console.
internal static ж<File> newConsoleFile(syscallꓸHandle h, @string name) {
    return newFile(h, name, kindConsole, false);
}

// newFileFromNewFile is called by [NewFile].
internal static ж<File> newFileFromNewFile(uintptr fd, @string name) {
    var h = (syscallꓸHandle)fd;
    if (h == syscall.InvalidHandle) {
        return default;
    }
    return newFile(h, name, kindNewFile, false);
}

// net_newWindowsFile is a hidden entry point called by net.conn.File.
// This is used so that the File.pfd.close method calls [syscall.Closesocket]
// instead of [syscall.CloseHandle].
//
//go:linkname net_newWindowsFile net.newWindowsFile
public static ж<File> net_newWindowsFile(syscallꓸHandle h, @string name) {
    if (h == syscall.InvalidHandle) {
        throw panic("invalid FD");
    }
    return newFile(h, name, kindSock, true);
}

internal static void epipecheck(ref File @file, error e) {
}

// DevNull is the name of the operating system's “null device.”
// On Unix-like systems, it is "/dev/null"; on Windows, "NUL".
public static readonly @string DevNull = "NUL"u8;

// openFileNolog is the Windows implementation of OpenFile.
internal static (ж<File>, error) openFileNolog(@string name, nint flag, FileMode perm) {
    if (name == ""u8) {
        return (default, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "open"u8, Path: name, Err: syscall.ENOENT))));
    }
    @string path = fixLongPath(name);
    var (r, err) = syscall.Open(path, flag | (nint)syscall.O_CLOEXEC, syscallMode(perm));
    if (err != nil) {
        return (default, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "open"u8, Path: name, Err: err))));
    }
    var nonblocking = (flag & (nint)windows.O_FILE_FLAG_OVERLAPPED) != 0;
    return (newFile(r, name, kindOpenFile, nonblocking), default);
}

internal static (ж<File>, error) openDirNolog(@string name) {
    return openFileNolog(name, (nint)(O_RDONLY | (nint)windows.O_DIRECTORY), 0);
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
    error err = default;
    {
        var e = Ꮡfile.of(@file.Ꮡpfd).Close(); if (e != nil) {
            if (AreEqual(e, poll.ErrFileClosing)) {
                e = ErrClosed;
            }
            err = new fs.PathErrorжerror(Ꮡ(new PathError(Op: "close"u8, Path: @file.name, Err: e)));
        }
    }

    // no need for a finalizer anymore
    runtime.SetFinalizer(Ꮡfile.OrTypedNil(), default);
    return err;
}

// seek sets the offset for the next Read or Write on file to offset, interpreted
// according to whence: 0 means relative to the origin of the file, 1 means
// relative to the current offset, and 2 means relative to the end.
// It returns the new offset and an error, if any.
internal static (int64 ret, error err) seek(this ж<File> Ꮡf, int64 offset, nint whence) {
    int64 ret = default;
    error err = default;

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
public static error Truncate(@string name, int64 size) {
    GoFrame ᒐ = default;
    try {
        var (f, e) = OpenFile(name, O_WRONLY, 438);
        if (e != nil) {
            return e;
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        var e1 = f.Truncate(size);
        if (e1 != nil) {
            return e1;
        }
        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// Remove removes the named file or directory.
// If there is an error, it will be of type [*PathError].
public static error Remove(@string name) {
    var (p, e) = syscall.UTF16PtrFromString(fixLongPath(name));
    if (e != nil) {
        return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "remove"u8, Path: name, Err: e)));
    }

    // Go file interface forces us to know whether
    // name is a file or directory. Try both.
    e = syscall.DeleteFile(p);
    if (e == nil) {
        return default;
    }
    var e1 = syscall.RemoveDirectory(p);
    if (e1 == nil) {
        return default;
    }

    // Both failed: figure out which error to return.
    if (!AreEqual(e1, e)) {
        var (a, e2) = syscall.GetFileAttributes(p);
        if (e2 != nil) {
            e = e2;
        } else {
            if ((a & (uint32)syscall.FILE_ATTRIBUTE_DIRECTORY) != 0) {
                e = e1;
            } else if ((a & (uint32)syscall.FILE_ATTRIBUTE_READONLY) != 0) {
                {
                    e1 = syscall.SetFileAttributes(p, a & ~(uint32)syscall.FILE_ATTRIBUTE_READONLY); if (e1 == nil) {
                        {
                            e = syscall.DeleteFile(p); if (e == nil) {
                                return default;
                            }
                        }
                    }
                }
            }
        }
    }
    return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "remove"u8, Path: name, Err: e)));
}

internal static error rename(@string oldname, @string newname) {
    var e = windows.Rename(fixLongPath(oldname), fixLongPath(newname));
    if (e != nil) {
        return new LinkErrorжerror(Ꮡ(new LinkError("rename"u8, oldname, newname, e)));
    }
    return default;
}

// Pipe returns a connected pair of Files; reads from r return bytes written to w.
// It returns the files and an error, if any. The Windows handles underlying
// the returned files are marked as inheritable by child processes.
public static (ж<File> r, ж<File> w, error err) Pipe() {
    array<syscallꓸHandle> p = new(2);
    var e = syscall.Pipe(p[..]);
    if (e != nil) {
        return (default, default, NewSyscallError(pipeˢ, e));
    }
    // syscall.Pipe always returns a non-blocking handle.
    return (newFile(p[0], "|0"u8, kindPipe, false), newFile(p[1], "|1"u8, kindPipe, false), default);
}

internal static Func<bool> useGetTempPath2 = Δsync.OnceValue(bool () => windows.ErrorLoadingGetTempPath2() == nil);

internal static @string tempDir() {
    var getTempPath = syscall.GetTempPath;
    if (useGetTempPath2()) {
        getTempPath = windows.GetTempPath2;
    }
    var n = (uint32)syscall.MAX_PATH;
    for (;;) {
        var b = new slice<uint16>((nint)(n));
        (n, _) = getTempPath((uint32)len(b), Ꮡ(b, 0));
        if (n > (uint32)len(b)) {
            continue;
        }
        if (n == 3 && b[1] == ':' && b[2] == '\\') {
            // Do nothing for path, like C:\.
        } else if (n > 0 && b[n - 1] == '\\') {
            // Otherwise remove terminating \.
            n--;
        }
        return syscall.UTF16ToString(b.slice(0, (nint)(n)));
    }
}

// Link creates newname as a hard link to the oldname file.
// If there is an error, it will be of type *LinkError.
public static error Link(@string oldname, @string newname) {
    var (n, err) = syscall.UTF16PtrFromString(fixLongPath(newname));
    if (err != nil) {
        return new LinkErrorжerror(Ꮡ(new LinkError("link"u8, oldname, newname, err)));
    }
    (var o, err) = syscall.UTF16PtrFromString(fixLongPath(oldname));
    if (err != nil) {
        return new LinkErrorжerror(Ꮡ(new LinkError("link"u8, oldname, newname, err)));
    }
    err = syscall.CreateHardLink(n, o, 0);
    if (err != nil) {
        return new LinkErrorжerror(Ꮡ(new LinkError("link"u8, oldname, newname, err)));
    }
    return default;
}

// Symlink creates newname as a symbolic link to oldname.
// On Windows, a symlink to a non-existent oldname creates a file symlink;
// if oldname is later created as a directory the symlink will not work.
// If there is an error, it will be of type *LinkError.
public static error Symlink(@string oldname, @string newname) {
    // '/' does not work in link's content
    oldname = filepathlite.FromSlash(oldname);

    // need the exact location of the oldname when it's relative to determine if it's a directory
    @string destpath = oldname;
    {
        @string v = filepathlite.VolumeName(oldname); if (v == ""u8) {
            if (len(oldname) > 0 && IsPathSeparator(oldname[0])) {
                // oldname is relative to the volume containing newname.
                {
                    v = filepathlite.VolumeName(newname); if (v != ""u8) {
                        // Prepend the volume explicitly, because it may be different from the
                        // volume of the current working directory.
                        destpath = v + oldname;
                    }
                }
            } else {
                // oldname is relative to newname.
                destpath = dirname(newname) + @"\"u8 + oldname;
            }
        }
    }

    var (fi, err) = Stat(destpath);
    var isdir = err == nil && fi.IsDir();

    (var n, err) = syscall.UTF16PtrFromString(fixLongPath(newname));
    if (err != nil) {
        return new LinkErrorжerror(Ꮡ(new LinkError("symlink"u8, oldname, newname, err)));
    }
    ж<uint16> o = default;
    if (filepathlite.IsAbs(oldname)) {
        (o, err) = syscall.UTF16PtrFromString(fixLongPath(oldname));
    } else {
        // Do not use fixLongPath on oldname for relative symlinks,
        // as it would turn the name into an absolute path thus making
        // an absolute symlink instead.
        // Notice that CreateSymbolicLinkW does not fail for relative
        // symlinks beyond MAX_PATH, so this does not prevent the
        // creation of an arbitrary long path name.
        (o, err) = syscall.UTF16PtrFromString(oldname);
    }
    if (err != nil) {
        return new LinkErrorжerror(Ꮡ(new LinkError("symlink"u8, oldname, newname, err)));
    }

    uint32 flags = windows.SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE;
    if (isdir) {
        flags |= (uint32)(syscall.SYMBOLIC_LINK_FLAG_DIRECTORY);
    }
    err = syscall.CreateSymbolicLink(n, o, flags);
    if (err != nil) {
        // the unprivileged create flag is unsupported
        // below Windows 10 (1703, v10.0.14972). retry without it.
        flags &= unchecked((uint32)~(uint32)(windows.SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE));
        err = syscall.CreateSymbolicLink(n, o, flags);
        if (err != nil) {
            return new LinkErrorжerror(Ꮡ(new LinkError("symlink"u8, oldname, newname, err)));
        }
    }
    return default;
}

// openSymlink calls CreateFile Windows API with FILE_FLAG_OPEN_REPARSE_POINT
// parameter, so that Windows does not follow symlink, if path is a symlink.
// openSymlink returns opened file handle.
internal static (syscallꓸHandle, error) openSymlink(@string path) {
    var (p, err) = syscall.UTF16PtrFromString(path);
    if (err != nil) {
        return (0, err);
    }
    var attrs = (uint32)syscall.FILE_FLAG_BACKUP_SEMANTICS;
    // Use FILE_FLAG_OPEN_REPARSE_POINT, otherwise CreateFile will follow symlink.
    // See https://docs.microsoft.com/en-us/windows/desktop/FileIO/symbolic-link-effects-on-file-systems-functions#createfile-and-createfiletransacted
    attrs |= (uint32)(syscall.FILE_FLAG_OPEN_REPARSE_POINT);
    (var h, err) = syscall.CreateFile(p, 0, 0, nil, syscall.OPEN_EXISTING, attrs, 0);
    if (err != nil) {
        return (0, err);
    }
    return (h, default);
}

internal static ж<godebug.Setting> winreadlinkvolume = godebug.New("winreadlinkvolume"u8);

// normaliseLinkPath converts absolute paths returned by
// DeviceIoControl(h, FSCTL_GET_REPARSE_POINT, ...)
// into paths acceptable by all Windows APIs.
// For example, it converts
//
//	\??\C:\foo\bar into C:\foo\bar
//	\??\UNC\foo\bar into \\foo\bar
//	\??\Volume{abc}\ into \\?\Volume{abc}\
internal static (@string, error) normaliseLinkPath(@string path) {
    GoFrame ᒐ = default;
    try {
        if (len(path) < 4 || path[..4] != @"\??\") {
            // unexpected path, return it as is
            return (path, default);
        }
        // we have path that start with \??\
        @string s = path[4..];
        switch (ᐧ) {
        case {} when len(s) >= 2 && s[1] == (byte)':': { // \??\C:\foo\bar
            return (s, default);
        }
        case {} when len(s) >= 4 && s[..4] == @"UNC\": { // \??\UNC\foo\bar
            return (@"\\" + s[4..], default);
        }}

        // \??\Volume{abc}\
        if (winreadlinkvolume.Value() != "0"u8) {
            return (@"\\?\" + path[4..], default);
        }
        winreadlinkvolume.IncNonDefault();

        var (h, err) = openSymlink(path);
        if (err != nil) {
            return ("", err);
        }
        defer(syscall.CloseHandle, h, ref ᒐ);

        var buf = new slice<uint16>(100);
        for (;;) {
            var (n, errΔ1) = windows.GetFinalPathNameByHandle(h, Ꮡ(buf, 0), (uint32)len(buf), windows.VOLUME_NAME_DOS);
            if (errΔ1 != nil) {
                return ("", errΔ1);
            }
            if (n < (uint32)len(buf)) {
                break;
            }
            buf = new slice<uint16>((nint)(n));
        }
        s = syscall.UTF16ToString(buf);
        if (len(s) > 4 && s[..4] == @"\\?\") {
            s = s[4..];
            if (len(s) > 3 && s[..3] == @"UNC") {
                // return path like \\server\share\...
                return (@"\" + s[3..], default);
            }
            return (s, default);
        }
        return ("", errors.New("GetFinalPathNameByHandle returned unexpected path: "u8 + s));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// go2cs generated this placeholder — func readReparseLink is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func readReparseLinkHandle is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static (@string, error) readlink(@string name) {
    var (s, err) = readReparseLink(fixLongPath(name));
    if (err != nil) {
        return ("", new fs.PathErrorжerror(Ꮡ(new PathError(Op: "readlink"u8, Path: name, Err: err))));
    }
    return (s, default);
}

} // end os_package
