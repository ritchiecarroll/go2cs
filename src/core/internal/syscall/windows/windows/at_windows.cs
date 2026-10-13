// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class windows_package {

// Openat flags not supported by syscall.Open.
//
// These are invented values.
//
// When adding a new flag here, add an unexported version to
// the set of invented O_ values in syscall/types_windows.go
// to avoid overlap.
public static UntypedInt O_DIRECTORY => 0x100000; // target must be a directory
public static UntypedInt O_NOFOLLOW_ANY => 0x20000000; // disallow symlinks anywhere in the path
public static UntypedInt O_OPEN_REPARSE => 0x40000000; // FILE_OPEN_REPARSE_POINT, used by Lstat

public static (syscallꓸHandle, error e1) Openat(syscallꓸHandle dirfd, @string name, nint flag, uint32 perm) {
    if (len(name) == 0) {
        return (syscall.InvalidHandle, syscall.ERROR_FILE_NOT_FOUND);
    }

    uint32 access = default!;
    uint32 options = default!;
    var exprᴛ1 = (nint)(flag & (nint)((nint)((nint)(UntypedInt)(syscall.O_RDONLY | syscall.O_WRONLY) | (nint)syscall.O_RDWR)));
    if (exprᴛ1 == syscall.O_RDONLY) {
        // FILE_GENERIC_READ includes FILE_LIST_DIRECTORY.
        access = FILE_GENERIC_READ;
    }
    else if (exprᴛ1 == syscall.O_WRONLY) {
        access = FILE_GENERIC_WRITE;
        options |= (uint32)(FILE_NON_DIRECTORY_FILE);
    }
    else if (exprᴛ1 == syscall.O_RDWR) {
        access = (uint32)((uint32)FILE_GENERIC_READ | (uint32)FILE_GENERIC_WRITE);
        options |= (uint32)(FILE_NON_DIRECTORY_FILE);
    }
    else { /* default: */
        // Stat opens files without requesting read or write permissions,
        // but we still need to request SYNCHRONIZE.
        access = SYNCHRONIZE;
    }
    if ((nint)(flag & (nint)syscall.O_CREAT) != 0) {
        access |= (uint32)(FILE_GENERIC_WRITE);
    }
    if ((nint)(flag & (nint)syscall.O_APPEND) != 0) {
        access |= (uint32)(FILE_APPEND_DATA);
        // Remove FILE_WRITE_DATA access unless O_TRUNC is set,
        // in which case we need it to truncate the file.
        if ((nint)(flag & (nint)syscall.O_TRUNC) == 0) {
            access &= unchecked((uint32)~(uint32)(FILE_WRITE_DATA));
        }
    }
    if ((nint)(flag & (nint)O_DIRECTORY) != 0) {
        options |= (uint32)(FILE_DIRECTORY_FILE);
        access |= (uint32)(FILE_LIST_DIRECTORY);
    }
    if ((nint)(flag & (nint)syscall.O_SYNC) != 0) {
        options |= (uint32)(FILE_WRITE_THROUGH);
    }
    // Allow File.Stat.
    access |= (uint32)((uint32)((UntypedInt)(STANDARD_RIGHTS_READ | FILE_READ_ATTRIBUTES) | (uint32)FILE_READ_EA));

    var objAttrs = Ꮡ(new OBJECT_ATTRIBUTES(nil));
    if ((nint)(flag & (nint)O_NOFOLLOW_ANY) != 0) {
        objAttrs.Value.Attributes |= (uint32)(OBJ_DONT_REPARSE);
    }
    if ((nint)(flag & (nint)syscall.O_CLOEXEC) == 0) {
        objAttrs.Value.Attributes |= (uint32)(OBJ_INHERIT);
    }
    {
        var errΔ1 = objAttrs.init(dirfd, name); if (errΔ1 != default!) {
            return (syscall.InvalidHandle, errΔ1);
        }
    }

    if ((nint)(flag & (nint)O_OPEN_REPARSE) != 0) {
        options |= (uint32)(FILE_OPEN_REPARSE_POINT);
    }

    // We don't use FILE_OVERWRITE/FILE_OVERWRITE_IF, because when opening
    // a file with FILE_ATTRIBUTE_READONLY these will replace an existing
    // file with a new, read-only one.
    //
    // Instead, we ftruncate the file after opening when O_TRUNC is set.
    uint32 disposition = default!;
    switch (ᐧ) {
    case {} when (nint)(flag & (nint)((nint)((nint)syscall.O_CREAT | (nint)syscall.O_EXCL))) == (nint)((nint)((nint)syscall.O_CREAT | (nint)syscall.O_EXCL)): {
        disposition = FILE_CREATE;
        options |= (uint32)(FILE_OPEN_REPARSE_POINT); // don't follow symlinks
        break;
    }
    case {} when (nint)(flag & (nint)syscall.O_CREAT) == syscall.O_CREAT: {
        disposition = FILE_OPEN_IF;
        break;
    }
    default: {
        disposition = FILE_OPEN;
        break;
    }}

    var fileAttrs = (uint32)FILE_ATTRIBUTE_NORMAL;
    if ((uint32)(perm & (uint32)syscall.S_IWRITE) == 0) {
        fileAttrs = FILE_ATTRIBUTE_READONLY;
    }

    ref var h = ref heap(new syscallꓸHandle(), out var Ꮡh);
    var err = NtCreateFile(
        Ꮡh,
        (uint32)((uint32)SYNCHRONIZE | access),
        objAttrs,
        Ꮡ(new IO_STATUS_BLOCK(nil)),
        nil,
        fileAttrs,
        (uint32)((UntypedInt)(FILE_SHARE_READ | FILE_SHARE_WRITE) | (uint32)FILE_SHARE_DELETE),
        disposition,
        (uint32)((uint32)((uint32)FILE_SYNCHRONOUS_IO_NONALERT | (uint32)FILE_OPEN_FOR_BACKUP_INTENT) | options),
        0,
        0);
    if (err != default!) {
        return (h, ntCreateFileError(err, flag));
    }

    if ((nint)(flag & (nint)syscall.O_TRUNC) != 0) {
        err = syscall.Ftruncate(h, 0);
        if (err != default!) {
            syscall.CloseHandle(h);
            return (syscall.InvalidHandle, err);
        }
    }

    return (h, default!);
}

// ntCreateFileError maps error returns from NTCreateFile to user-visible errors.
internal static error ntCreateFileError(error err, nint flag) {
    var (s, ok) = err._<NTStatus>(ᐧ);
    if (!ok) {
        // Shouldn't really be possible, NtCreateFile always returns NTStatus.
        return err;
    }
    var exprᴛ1 = s;
    if (exprᴛ1 == STATUS_REPARSE_POINT_ENCOUNTERED) {
        return syscall.ELOOP;
    }
    if (exprᴛ1 == STATUS_NOT_A_DIRECTORY) {
        // ENOTDIR is the errno returned by open when O_DIRECTORY is specified
        // and the target is not a directory.
        //
        // NtCreateFile can return STATUS_NOT_A_DIRECTORY under other circumstances,
        // such as when opening "file/" where "file" is not a directory.
        // (This might be Windows version dependent.)
        //
        // Only map STATUS_NOT_A_DIRECTORY to ENOTDIR when O_DIRECTORY is specified.
        if ((nint)(flag & (nint)O_DIRECTORY) != 0) {
            return syscall.ENOTDIR;
        }
    }
    else if (exprᴛ1 == STATUS_FILE_IS_A_DIRECTORY) {
        return syscall.EISDIR;
    }
    return s.Errno();
}

public static error Mkdirat(syscallꓸHandle dirfd, @string name, uint32 mode) {
    var objAttrs = Ꮡ(new OBJECT_ATTRIBUTES(nil));
    {
        var errΔ1 = objAttrs.init(dirfd, name); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    ref var h = ref heap(new syscallꓸHandle(), out var Ꮡh);
    var err = NtCreateFile(
        Ꮡh,
        FILE_GENERIC_READ,
        objAttrs,
        Ꮡ(new IO_STATUS_BLOCK(nil)),
        nil,
        syscall.FILE_ATTRIBUTE_NORMAL,
        (uint32)((UntypedInt)(syscall.FILE_SHARE_READ | syscall.FILE_SHARE_WRITE) | (uint32)syscall.FILE_SHARE_DELETE),
        FILE_CREATE,
        FILE_DIRECTORY_FILE,
        0,
        0);
    if (err != default!) {
        return ntCreateFileError(err, 0);
    }
    syscall.CloseHandle(h);
    return default!;
}

public static error Deleteat(syscallꓸHandle dirfd, @string name) {
    GoFrame ᒐ = default;
    try {
        var objAttrs = Ꮡ(new OBJECT_ATTRIBUTES(nil));
        {
            var errΔ1 = objAttrs.init(dirfd, name); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
        ref var h = ref heap(new syscallꓸHandle(), out var Ꮡh);
        var err = NtOpenFile(
            Ꮡh,
            DELETE,
            objAttrs,
            Ꮡ(new IO_STATUS_BLOCK(nil)),
            (uint32)((UntypedInt)(FILE_SHARE_DELETE | FILE_SHARE_READ) | (uint32)FILE_SHARE_WRITE),
            (uint32)((uint32)FILE_OPEN_REPARSE_POINT | (uint32)FILE_OPEN_FOR_BACKUP_INTENT));
        if (err != default!) {
            return ntCreateFileError(err, 0);
        }
        defer(syscall.CloseHandle, h, ref ᒐ);

        const uint32 FileDispositionInformation = 13;
        const uint32 FileDispositionInformationEx = 64;

        // First, attempt to delete the file using POSIX semantics
        // (which permit a file to be deleted while it is still open).
        // This matches the behavior of DeleteFileW.
        err = NtSetInformationFile(
            h,
            Ꮡ(new IO_STATUS_BLOCK(nil)),
            (uintptr)Ꮡ(new FILE_DISPOSITION_INFORMATION_EX(
                Flags: (uint32)((UntypedInt)((UntypedInt)(FILE_DISPOSITION_DELETE | FILE_DISPOSITION_FORCE_IMAGE_SECTION_CHECK) | FILE_DISPOSITION_POSIX_SEMANTICS) |
                    // This differs from DeleteFileW, but matches os.Remove's
                    // behavior on Unix platforms of permitting deletion of
                    // read-only files.
                    (uint32)FILE_DISPOSITION_IGNORE_READONLY_ATTRIBUTE)
            )),
            (uint32)/* unsafe.Sizeof(FILE_DISPOSITION_INFORMATION_EX{}) */ (uintptr)4,
            FileDispositionInformationEx);
        var exprᴛ1 = err;
        if (AreEqual(exprᴛ1, default!)) {
            return default!;
        }
        if (AreEqual(exprᴛ1, STATUS_CANNOT_DELETE) || AreEqual(exprᴛ1, STATUS_DIRECTORY_NOT_EMPTY)) {
            return err._<NTStatus>().Errno();
        }

        // If the prior deletion failed, the filesystem either doesn't support
        // POSIX semantics (for example, FAT), or hasn't implemented
        // FILE_DISPOSITION_INFORMATION_EX.
        //
        // Try again.
        err = NtSetInformationFile(
            h,
            Ꮡ(new IO_STATUS_BLOCK(nil)),
            (uintptr)Ꮡ(new FILE_DISPOSITION_INFORMATION(
                DeleteFile: true
            )),
            (uint32)/* unsafe.Sizeof(FILE_DISPOSITION_INFORMATION{}) */ (uintptr)1,
            FileDispositionInformation);
        {
            var (st, ok) = err._<NTStatus>(ᐧ); if (ok) {
                return st.Errno();
            }
        }
        return err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

} // end windows_package
