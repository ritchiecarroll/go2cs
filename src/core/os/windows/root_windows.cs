// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build windows
global using sysfdType = go.syscall_package.ΔHandle;

namespace go;

using errors = errors_package;
using filepathlite = @internal.filepathlite_package;
using stringslite = @internal.stringslite_package;
using windows = @internal.syscall.windows_package;
using runtime = runtime_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.syscall;
using fs = go.io.fs_package;

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

    var (fd, err) = doInRoot<syscallꓸHandle>(ref (Ꮡr).DerefOrNull(), name, rootOpenDir);
    if (err != default!) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "openat"u8, Path: name, Err: err))));
    }
    return newRoot(fd, joinPath(r.Name(), name));
}

// rootOpenFileNolog is Root.OpenFile.
internal static (ж<File>, error) rootOpenFileNolog(ж<Root> Ꮡroot, @string name, nint flag, FileMode perm) {
    ref var root = ref Ꮡroot.DerefOrNull();

    var (fd, err) = doInRoot(ref (Ꮡroot).DerefOrNull(), name, (syscallꓸHandle, error) (syscallꓸHandle parent, @string nameΔ1) => openat(parent, nameΔ1, flag, perm));
    if (err != default!) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "openat"u8, Path: name, Err: err))));
    }
    return (newFile(fd, joinPath(root.Name(), name), fileˢ), default!);
}

internal static (syscallꓸHandle, error) openat(syscallꓸHandle dirfd, @string name, nint flag, FileMode perm) {
    var (h, err) = windows.Openat(dirfd, name, (nint)((nint)(flag | (nint)syscall.O_CLOEXEC) | (nint)windows.O_NOFOLLOW_ANY), syscallMode(perm));
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
            0,
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
    var (h, err) = openat(parent, name, (nint)((nint)(UntypedInt)(syscall.O_RDONLY | syscall.O_CLOEXEC) | (nint)windows.O_DIRECTORY), 0);
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
    if (len(name) > 0 && IsPathSeparator(name[len(name) - 1])) {
        // When a filename ends with a path separator,
        // Lstat behaves like Stat.
        //
        // This behavior is not based on a principled decision here,
        // merely the empirical evidence that Lstat behaves this way.
        lstat = false;
    }
    var (fi, err) = doInRoot(ref r, name, (FileInfo, error) (syscallꓸHandle parent, @string n) => {
        GoFrame ᒐ = default;
        try {
            var (fd, errΔ1) = openat(parent, n, windows.O_OPEN_REPARSE, 0);
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

internal static error mkdirat(syscallꓸHandle dirfd, @string name, FileMode perm) {
    return windows.Mkdirat(dirfd, name, syscallMode(perm));
}

internal static error removeat(syscallꓸHandle dirfd, @string name) {
    return windows.Deleteat(dirfd, name);
}

} // end os_package
