// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build windows

// sysfdType is the native type of a file handle
// (int on Unix, syscall.Handle on Windows),
// permitting helper functions to be written portably.
global using sysfdType = go.syscall_package.ΔHandle;

namespace go;

using errors = errors_package;
using filepathlite = @internal.filepathlite_package;
using stringslite = @internal.stringslite_package;
using windows = @internal.syscall.windows_package;
using runtime = runtime_package;
using syscall = syscall_package;
using time = time_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.syscall;
using fs = go.io.fs_package;
using go.io;

partial class os_package {

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string fixedPrefixᶜ = @"\\?\?"u8;

// rootCleanPath uses GetFullPathName to perform lexical path cleaning.
//
// On Windows, file names are lexically cleaned at the start of a file operation.
// For example, on Windows the path `a\..\b` is exactly equivalent to `b` alone,
// even if `a` does not exist or is not a directory.
//
// We use the Windows API function GetFullPathName to perform this cleaning.
// We could do this ourselves, but there are a number of subtle behaviors here,
// and deferring to the OS maintains consistency.
// (For example, `a\.\` cleans to `a\`.)
//
// GetFullPathName operates on absolute paths, and our input path is relative.
// We make the path absolute by prepending a fixed prefix of \\?\?\.
//
// We want to detect paths which use .. components to escape the root.
// We do this by ensuring the cleaned path still begins with \\?\?\.
// We catch the corner case of a path which includes a ..\?\. component
// by rejecting any input paths which contain a ?, which is not a valid character
// in a Windows filename.
internal static (@string, error) rootCleanPath(@string s, slice<@string> prefix, slice<@string> suffix) {
    // Reject paths which include a ? component (see above).
    if (stringslite.IndexByte(s, (rune)'?') >= 0) {
        return ("", windows.ERROR_INVALID_NAME);
    }

    @string fixedPrefix = fixedPrefixᶜ;
    var buf = slice<byte>(fixedPrefix);
    foreach (var (_, p) in prefix) {
        buf = append(buf, (byte)((rune)'\\'));
        buf = appendꓸꓸꓸ(buf, slice<byte>(p));
    }
    buf = append(buf, (byte)((rune)'\\'));
    buf = appendꓸꓸꓸ(buf, slice<byte>(s));
    foreach (var (_, p) in suffix) {
        buf = append(buf, (byte)((rune)'\\'));
        buf = appendꓸꓸꓸ(buf, slice<byte>(p));
    }
    s = ((@string)buf);

    (s, var err) = syscall.FullPath(s);
    if (err != default!) {
        return ("", err);
    }

    (s, var ok) = stringslite.CutPrefix(s, fixedPrefix);
    if (!ok) {
        return ("", errPathEscapes);
    }
    s = stringslite.TrimPrefix(s, @"\"u8);
    if (s == ""u8) {
        s = "."u8;
    }

    if (!filepathlite.IsLocal(s)) {
        return ("", errPathEscapes);
    }

    return (s, default!);
}

// openRootNolog is OpenRoot.
internal static (ж<Root>, error) openRootNolog(@string name) {
    if (name == ""u8) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "open"u8, Path: name, Err: syscall.ENOENT))));
    }
    @string path = fixLongPath(name);
    var (fd, err) = syscall.Open(path, (nint)((nint)syscall.O_RDONLY | (nint)syscall.O_CLOEXEC), 0);
    if (err != default!) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "open"u8, Path: name, Err: err))));
    }
    return newRoot(fd, name);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string notADirectoryˢ = "not a directory"u8;

// newRoot returns a new Root.
// If fd is not a directory, it closes it and returns an error.
internal static (ж<Root>, error) newRoot(syscallꓸHandle fd, @string name) {
    // Check that this is a directory.
    //
    // If we get any errors here, ignore them; worst case we create a Root
    // which returns errors when you try to use it.
    ref var fi = ref heap(new syscall.ByHandleFileInformation(), out var Ꮡfi);
    var err = syscall.GetFileInformationByHandle(fd, Ꮡfi);
    if (err == default! && (uint32)(fi.FileAttributes & (uint32)syscall.FILE_ATTRIBUTE_DIRECTORY) == 0) {
        syscall.CloseHandle(fd);
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "open"u8, Path: name, Err: errors.New(notADirectoryˢ)))));
    }

    var r = Ꮡ(new Root(Ꮡ(new root(
        fd: fd,
        name: name
    ))
    ));
    runtime.SetFinalizer((~r).root.OrTypedNil(), ((Func<ж<root>, error>)(Close)));
    return (r, default!);
}

// openRootInRoot is Root.OpenRoot.
internal static (ж<Root>, error) openRootInRoot(ж<Root> Ꮡr, @string name) {
    ref var r = ref Ꮡr.DerefOrNull();

    var (fd, err) = doInRoot(ref (Ꮡr).DerefOrNull(), name, 0, default!, (syscallꓸHandle, error) (syscallꓸHandle parent, @string nameΔ1, bool endsInSlash) => rootOpenDir(parent, nameΔ1));
    if (err != default!) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "openat"u8, Path: name, Err: err))));
    }
    return newRoot(fd, joinPath(r.Name(), name));
}

// rootOpenFileNolog is Root.OpenFile.
internal static (ж<File>, error) rootOpenFileNolog(ж<Root> Ꮡroot, @string name, nint flag, FileMode perm) {
    ref var root = ref Ꮡroot.DerefOrNull();

    var (fd, err) = doInRoot(ref (Ꮡroot).DerefOrNull(), name, doInRootNoHandleTerminalSlash, default!, (syscallꓸHandle, error) (syscallꓸHandle parent, @string nameΔ1, bool endsInSlash) => {
        if (endsInSlash) {
            flag |= (nint)(windows.O_DIRECTORY);
        }
        return openat(parent, nameΔ1, (uint64)flag, perm);
    });
    if (err != default!) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "openat"u8, Path: name, Err: err))));
    }
    var nonblocking = (nint)(flag & (nint)windows.O_FILE_FLAG_OVERLAPPED) != 0;
    return (newFile(fd, joinPath(root.Name(), name), kindOpenFile, nonblocking), default!);
}

internal static (syscallꓸHandle, error) openat(syscallꓸHandle dirfd, @string name, uint64 flag, FileMode perm) {
    var (h, err) = windows.Openat(dirfd, name, (uint64)((uint64)(flag | (uint64)syscall.O_CLOEXEC) | (uint64)windows.O_NOFOLLOW_ANY), syscallMode(perm));
    if (AreEqual(err, syscall.ELOOP) || AreEqual(err, syscall.ENOTDIR)) {
        {
            var (link, errΔ1) = readReparseLinkAt(dirfd, name); if (errΔ1 == default!) {
                return (syscall.InvalidHandle, ((errSymlink)link));
            }
        }
    }
    return (h, err);
}

internal static (@string, error) readReparseLinkAt(syscallꓸHandle dirfd, @string name) {
    GoFrame ᒐ = default;
    try {
        var (objectName, err) = windows.NewNTUnicodeString(name);
        if (err != default!) {
            return ("", err);
        }
        var objAttrs = Ꮡ(new windows.OBJECT_ATTRIBUTES(
            ObjectName: objectName
        ));
        if (dirfd != syscall.InvalidHandle) {
            objAttrs.Value.RootDirectory = dirfd;
        }
        objAttrs.Value.Length = (uint32)/* unsafe.Sizeof(*objAttrs) */ (uintptr)48;
        ref var h = ref heap(new syscallꓸHandle(), out var Ꮡh);
        err = windows.NtCreateFile(
            Ꮡh,
            windows.FILE_GENERIC_READ,
            objAttrs,
            Ꮡ(new windows.IO_STATUS_BLOCK(nil)),
            nil,
            (uint32)syscall.FILE_ATTRIBUTE_NORMAL,
            (uint32)((UntypedInt)(syscall.FILE_SHARE_READ | syscall.FILE_SHARE_WRITE) | (uint32)syscall.FILE_SHARE_DELETE),
            windows.FILE_OPEN,
            (uint32)((uint32)windows.FILE_SYNCHRONOUS_IO_NONALERT | (uint32)windows.FILE_OPEN_REPARSE_POINT),
            nil,
            0);
        if (err != default!) {
            return ("", err);
        }
        defer(syscall.CloseHandle, h, ref ᒐ);
        return readReparseLinkHandle(h);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static (syscallꓸHandle, error) rootOpenDir(syscallꓸHandle parent, @string name) {
    var (h, err) = openat(parent, name, (uint64)((UntypedInt)(syscall.O_RDONLY | syscall.O_CLOEXEC) | (uint64)windows.O_DIRECTORY), 0);
    if (AreEqual(err, syscall.ERROR_FILE_NOT_FOUND)) {
        // Windows returns:
        //   - ERROR_PATH_NOT_FOUND if any path compoenent before the leaf
        //     does not exist or is not a directory.
        //   - ERROR_FILE_NOT_FOUND if the leaf does not exist.
        //
        // This differs from Unix behavior, which is:
        //   - ENOENT if any path component does not exist, including the leaf.
        //   - ENOTDIR if any path component before the leaf is not a directory.
        //
        // We map syscall.ENOENT to ERROR_FILE_NOT_FOUND and syscall.ENOTDIR
        // to ERROR_PATH_NOT_FOUND, but the Windows errors don't quite match.
        //
        // For consistency with os.Open, convert ERROR_FILE_NOT_FOUND here into
        // ERROR_PATH_NOT_FOUND, since we're opening a non-leaf path component.
        err = syscall.ERROR_PATH_NOT_FOUND;
    }
    return (h, err);
}

internal static (FileInfo, error) rootStat(ref Root r, @string name, bool lstat) {
    nuint flags = default!;
    if (lstat) {
        // Follow symlinks in the last path component when the path
        // ends with a path separator.
        //
        // This is not the usual behavior for Windows path resolution,
        // but empirically os.Lstat behaves this way.
        flags = doInRootAlwaysResolveTerminalSlash;
    }
    var (fi, err) = doInRoot(ref r, name, flags, default!, (FileInfo, error) (syscallꓸHandle parent, @string n, bool endsInSlash) => {
        GoFrame ᒐ = default;
        try {
            var (fd, errΔ1) = openat(parent, n, windows.O_FILE_FLAG_OPEN_REPARSE_POINT, 0);
            if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
            defer(syscall.CloseHandle, fd, ref ᒐ);
            (var fiΔ1, errΔ1) = statHandle(name, fd);
            if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
            if (!lstat && fiΔ1._<ж<fileStat>>().isReparseTagNameSurrogate()) {
                var (link, errΔ2) = readReparseLinkHandle(fd);
                if (errΔ2 != default!) {
                    return (default!, errΔ2);
                }
                return (default!, ((errSymlink)link));
            }
            return (fiΔ1, default!);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    });
    if (err != default!) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "statat"u8, Path: name, Err: err))));
    }
    return (fi, default!);
}

internal static error rootSymlink(ж<Root> Ꮡr, @string oldname, @string newname) {
    if (oldname == ""u8) {
        return syscall.EINVAL;
    }

    // CreateSymbolicLinkW converts volume-relative paths into absolute ones.
    // Do the same.
    if (filepathlite.VolumeNameLen(oldname) > 0 && !filepathlite.IsAbs(oldname)) {
        var (p, errΔ1) = syscall.FullPath(oldname);
        if (errΔ1 == default!) {
            oldname = p;
        }
    }

    // If oldname can be resolved to a directory in the root, create a directory link.
    // Otherwise, create a file link.
    windows.SymlinkatFlags flags = default!;
    if (filepathlite.VolumeNameLen(oldname) == 0 && !IsPathSeparator(oldname[0])) {
        // oldname is a path relative to the directory containing newname.
        // Prepend newname's directory to it to make a path relative to the root.
        // For example, if oldname=old and newname=a\new, destPath=a\old.
        @string destPath = oldname;
        {
            @string dir = dirname(newname); if (dir != "."u8) {
                destPath = dir + @"\"u8 + oldname;
            }
        }
        var (fi, errΔ2) = Ꮡr.Stat(destPath);
        if (errΔ2 == default! && fi.IsDir()) {
            flags |= (windows.SymlinkatFlags)(windows.SYMLINKAT_DIRECTORY);
        }
    }

    // Empirically, CreateSymbolicLinkW appears to set the relative flag iff
    // the target does not contain a volume name.
    if (filepathlite.VolumeNameLen(oldname) == 0) {
        flags |= (windows.SymlinkatFlags)(windows.SYMLINKAT_RELATIVE);
    }

    var (_, err) = doInRoot(ref (Ꮡr).DerefOrNull(), newname, 0, default!, (EmptyStruct, error) (syscallꓸHandle parent, @string name, bool endsInSlash) => (new EmptyStruct(), windows.Symlinkat(oldname, parent, name, flags)));
    if (err != default!) {
        return new LinkErrorжerror(Ꮡ(new LinkError("symlinkat"u8, oldname, newname, err)));
    }
    return default!;
}

internal static error chmodat(syscallꓸHandle parent, @string name, FileMode mode) {
    GoFrame ᒐ = default;
    try {
        // Currently, on Windows os.Chmod("symlink") will act on "symlink",
        // not on any file it points to.
        //
        // This may or may not be the desired behavior: https://go.dev/issue/71492
        //
        // For now, be consistent with os.Symlink.
        // Passing O_FILE_FLAG_OPEN_REPARSE_POINT causes us to open the named file itself,
        // not any file that it links to.
        //
        // If we want to change this in the future, pass O_NOFOLLOW_ANY instead
        // and return errSymlink when encountering a symlink:
        //
        //     if err == syscall.ELOOP || err == syscall.ENOTDIR {
        //         if link, err := readReparseLinkAt(parent, name); err == nil {
        //                 return errSymlink(link)
        //         }
        //     }
        var (h, err) = windows.Openat(parent, name, (uint64)((UntypedInt)(syscall.O_CLOEXEC | windows.O_FILE_FLAG_OPEN_REPARSE_POINT) | (uint64)windows.O_WRITE_ATTRS), 0);
        if (err != default!) {
            return err;
        }
        defer(syscall.CloseHandle, h, ref ᒐ);

        ref var d = ref heap(new syscall.ByHandleFileInformation(), out var Ꮡd);
        {
            var errΔ1 = syscall.GetFileInformationByHandle(h, Ꮡd); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
        var attrs = d.FileAttributes;

        if ((FileMode)(mode & (uint32)syscall.S_IWRITE) != 0){
            attrs &= unchecked((uint32)~(uint32)(syscall.FILE_ATTRIBUTE_READONLY));
        } else {
            attrs |= (uint32)(syscall.FILE_ATTRIBUTE_READONLY);
        }
        if (attrs == d.FileAttributes) {
            return default!;
        }

        ref var fbi = ref heap(new windows.FILE_BASIC_INFO(), out var Ꮡfbi);
        fbi.FileAttributes = attrs;
        return windows.SetFileInformationByHandle(h, windows.FileBasicInfo, @unsafe.Pointer.FromPinnedBox(Ꮡfbi), (uint32)/* unsafe.Sizeof(fbi) */ (uintptr)40);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static error chownat(syscallꓸHandle parent, @string name, nint uid, nint gid) {
    return syscall.EWINDOWS; // matches syscall.Chown
}

internal static error lchownat(syscallꓸHandle parent, @string name, nint uid, nint gid) {
    return syscall.EWINDOWS; // matches syscall.Lchown
}

internal static error mkdirat(syscallꓸHandle dirfd, @string name, FileMode perm) {
    return windows.Mkdirat(dirfd, name, syscallMode(perm));
}

internal static error removeat(syscallꓸHandle dirfd, @string name) {
    return windows.Deleteat(dirfd, name, 0);
}

internal static error removefileat(syscallꓸHandle dirfd, @string name) {
    return windows.Deleteat(dirfd, name, windows.FILE_NON_DIRECTORY_FILE);
}

internal static error removedirat(syscallꓸHandle dirfd, @string name) {
    return windows.Deleteat(dirfd, name, windows.FILE_DIRECTORY_FILE);
}

internal static error chtimesat(syscallꓸHandle dirfd, @string name, time.Time atime, time.Time mtime) {
    GoFrame ᒐ = default;
    try {
        var (h, err) = windows.Openat(dirfd, name, (uint64)(8590458880L | (uint64)windows.O_WRITE_ATTRS), 0);
        if (AreEqual(err, syscall.ELOOP) || AreEqual(err, syscall.ENOTDIR)) {
            {
                var (link, errΔ1) = readReparseLinkAt(dirfd, name); if (errΔ1 == default!) {
                    return ((errSymlink)link);
                }
            }
        }
        if (err != default!) {
            return err;
        }
        defer(syscall.CloseHandle, h, ref ᒐ);
        ref var a = ref heap<syscall.Filetime>(out var Ꮡa);
        a = new syscall.Filetime(nil);
        ref var w = ref heap<syscall.Filetime>(out var Ꮡw);
        w = new syscall.Filetime(nil);
        if (!atime.IsZero()) {
            a = syscall.NsecToFiletime(atime.UnixNano());
        }
        if (!mtime.IsZero()) {
            w = syscall.NsecToFiletime(mtime.UnixNano());
        }
        return syscall.SetFileTime(h, nil, Ꮡa, Ꮡw);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static error renameat(syscallꓸHandle oldfd, @string oldname, syscallꓸHandle newfd, @string newname) {
    return windows.Renameat(oldfd, oldname, newfd, newname);
}

internal static error linkat(syscallꓸHandle oldfd, @string oldname, syscallꓸHandle newfd, @string newname) {
    return windows.Linkat(oldfd, oldname, newfd, newname);
}

// checkSymlink resolves the symlink name in parent,
// and returns errSymlink with the link contents.
//
// If name is not a symlink, return origError.
internal static error checkSymlink(syscallꓸHandle parent, @string name, error origError) {
    var (link, err) = readlinkat(parent, name);
    if (err != default!) {
        return origError;
    }
    return ((errSymlink)link);
}

internal static (@string, error) readlinkat(syscallꓸHandle dirfd, @string name) {
    GoFrame ᒐ = default;
    try {
        var (fd, err) = openat(dirfd, name, windows.O_FILE_FLAG_OPEN_REPARSE_POINT, 0);
        if (err != default!) {
            return ("", err);
        }
        defer(syscall.CloseHandle, fd, ref ᒐ);
        return readReparseLinkHandle(fd);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static (FileInfo, error) lstatat(syscallꓸHandle parent, @string name) {
    GoFrame ᒐ = default;
    try {
        var (fd, err) = openat(parent, name, windows.O_FILE_FLAG_OPEN_REPARSE_POINT, 0);
        if (err != default!) {
            return (default!, err);
        }
        defer(syscall.CloseHandle, fd, ref ᒐ);
        (var fi, err) = statHandle(name, fd);
        if (err != default!) {
            return (default!, err);
        }
        return (fi, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// isLink reports whether fi is a symlink or other surrogate reparse point (such as a junction).
internal static bool isLink(FileInfo fi) {
    var (fs, ok) = fi._<ж<fileStat>>(ᐧ);
    return ok && fs.isReparseTagNameSurrogate();
}

// isDirectoryLink reports whether fi (assumed to be a link) is a directory link.
// Windows links come in two flavors: file and directory. This function distinguishes
// between the two.
internal static bool isDirectoryLink(FileInfo fi) {
    var (fs, ok) = fi._<ж<fileStat>>(ᐧ);
    return ok && (uint32)((~fs).FileAttributes & (uint32)syscall.FILE_ATTRIBUTE_DIRECTORY) != 0;
}

} // end os_package
