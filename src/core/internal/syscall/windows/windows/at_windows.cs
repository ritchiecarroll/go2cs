// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using oserror = go.@internal.oserror_package;
using stringslite = go.@internal.stringslite_package;
using runtime = runtime_package;
using structs = structs_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using System.Runtime.InteropServices;
using go.@internal;

partial class windows_package {

// Openat flags supported by syscall.Open.
public static UntypedInt O_DIRECTORY => 0x04000; // target must be a directory

// Openat flags not supported by syscall.Open.
//
// These are invented values, use values in the 33-63 bit range
// to avoid overlap with flags and attributes supported by [syscall.Open].
//
// When adding a new flag here, add an unexported version to
// the set of invented O_ values in syscall/types_windows.go
// to avoid overlap.
public static UntypedInt O_NOFOLLOW_ANY => 0x200000000; // disallow symlinks anywhere in the path
public static UntypedInt O_WRITE_ATTRS => 0x800000000;  // FILE_WRITE_ATTRIBUTES, used by Chmod

public static (syscallꓸHandle, error e1) Openat(syscallꓸHandle dirfd, @string name, uint64 flag, uint32 perm) {
    if (len(name) == 0) {
        return (syscall.InvalidHandle, syscall.ERROR_FILE_NOT_FOUND);
    }

    uint32 access = default;
    uint32 options = default;
    // Map Win32 file flags to NT create options.
    var fileFlags = (uint32)flag & (uint32)FileFlagsMask;
    if ((fileFlags & ~(uint32)ValidFileFlagsMask) != 0) {
        return (syscall.InvalidHandle, oserror.ErrInvalid);
    }
    if ((fileFlags & (uint32)O_FILE_FLAG_OVERLAPPED) == 0) {
        options |= (uint32)(FILE_SYNCHRONOUS_IO_NONALERT);
    }
    if ((fileFlags & (uint32)O_FILE_FLAG_DELETE_ON_CLOSE) != 0) {
        access |= (uint32)(DELETE);
    }
    void setOptionFlag(uint32 ntFlag, uint32 win32Flag) {
        if ((fileFlags & win32Flag) != 0) {
            options |= (uint32)(ntFlag);
        }
    }
    setOptionFlag(FILE_NO_INTERMEDIATE_BUFFERING, O_FILE_FLAG_NO_BUFFERING);
    setOptionFlag(FILE_WRITE_THROUGH, O_FILE_FLAG_WRITE_THROUGH);
    setOptionFlag(FILE_SEQUENTIAL_ONLY, O_FILE_FLAG_SEQUENTIAL_SCAN);
    setOptionFlag(FILE_RANDOM_ACCESS, O_FILE_FLAG_RANDOM_ACCESS);
    setOptionFlag(FILE_OPEN_FOR_BACKUP_INTENT, O_FILE_FLAG_BACKUP_SEMANTICS);
    setOptionFlag(FILE_SESSION_AWARE, O_FILE_FLAG_SESSION_AWARE);
    setOptionFlag(FILE_DELETE_ON_CLOSE, O_FILE_FLAG_DELETE_ON_CLOSE);
    setOptionFlag(FILE_OPEN_NO_RECALL, O_FILE_FLAG_OPEN_NO_RECALL);
    setOptionFlag(FILE_OPEN_REPARSE_POINT, O_FILE_FLAG_OPEN_REPARSE_POINT);

    switch ((uint64)(flag & ((uint64)((UntypedInt)(syscall.O_RDONLY | syscall.O_WRONLY) | (uint64)syscall.O_RDWR)))) {
    case var ᴛ when ᴛ == syscall.O_RDONLY: {
        // FILE_GENERIC_READ includes FILE_LIST_DIRECTORY.
        access |= (uint32)(FILE_GENERIC_READ);
        break;
    }
    case var ᴛ when ᴛ == syscall.O_WRONLY: {
        access |= (uint32)(FILE_GENERIC_WRITE);
        options |= (uint32)(FILE_NON_DIRECTORY_FILE);
        break;
    }
    case var ᴛ when ᴛ == syscall.O_RDWR: {
        access |= (uint32)((uint32)((uint32)FILE_GENERIC_READ | (uint32)FILE_GENERIC_WRITE));
        options |= (uint32)(FILE_NON_DIRECTORY_FILE);
        break;
    }
    default: {
        // Stat opens files without requesting read or write permissions,
        // but we still need to request SYNCHRONIZE.
        access |= (uint32)(SYNCHRONIZE);
        break;
    }}
    if ((flag & (uint64)syscall.O_CREAT) != 0) {
        access |= (uint32)(FILE_GENERIC_WRITE);
    }
    if ((fileFlags & (uint32)O_FILE_FLAG_NO_BUFFERING) != 0) {
        // Disable buffering implies no implicit append access.
        access &= unchecked((uint32)~(uint32)(FILE_APPEND_DATA));
    }
    if ((flag & (uint64)syscall.O_APPEND) != 0) {
        access |= (uint32)(FILE_APPEND_DATA);
        // Remove FILE_WRITE_DATA access unless O_TRUNC is set,
        // in which case we need it to truncate the file.
        if ((flag & (uint64)syscall.O_TRUNC) == 0) {
            access &= unchecked((uint32)~(uint32)(FILE_WRITE_DATA));
        }
    }
    if ((flag & (uint64)O_DIRECTORY) != 0) {
        options |= (uint32)(FILE_DIRECTORY_FILE);
        access |= (uint32)(FILE_LIST_DIRECTORY);
    }
    if ((flag & (uint64)syscall.O_SYNC) != 0) {
        options |= (uint32)(FILE_WRITE_THROUGH);
    }
    if ((flag & (uint64)O_WRITE_ATTRS) != 0) {
        access |= (uint32)(FILE_WRITE_ATTRIBUTES);
    }
    // Allow File.Stat.
    access |= (uint32)((uint32)((UntypedInt)(STANDARD_RIGHTS_READ | FILE_READ_ATTRIBUTES) | (uint32)FILE_READ_EA));

    var objAttrs = Ꮡ(new OBJECT_ATTRIBUTES());
    if ((flag & (uint64)O_NOFOLLOW_ANY) != 0) {
        objAttrs.Attributes |= (uint32)(OBJ_DONT_REPARSE);
    }
    if ((flag & (uint64)syscall.O_CLOEXEC) == 0) {
        objAttrs.Attributes |= (uint32)(OBJ_INHERIT);
    }
    if ((fileFlags & (uint32)O_FILE_FLAG_POSIX_SEMANTICS) == 0) {
        objAttrs.Attributes |= (uint32)(OBJ_CASE_INSENSITIVE);
    }
    {
        var errΔ1 = objAttrs.init(dirfd, name); if (errΔ1 != nil) {
            return (syscall.InvalidHandle, errΔ1);
        }
    }

    // We don't use FILE_OVERWRITE/FILE_OVERWRITE_IF, because when opening
    // a file with FILE_ATTRIBUTE_READONLY these will replace an existing
    // file with a new, read-only one.
    //
    // Instead, we ftruncate the file after opening when O_TRUNC is set.
    uint32 disposition = default;
    switch (ᐧ) {
    case {} when (uint64)(flag & ((uint64)((uint64)syscall.O_CREAT | (uint64)syscall.O_EXCL))) == (uint64)((uint64)((uint64)syscall.O_CREAT | (uint64)syscall.O_EXCL)): {
        disposition = FILE_CREATE;
        options |= (uint32)(FILE_OPEN_REPARSE_POINT); // don't follow symlinks
        break;
    }
    case {} when (flag & (uint64)syscall.O_CREAT) == syscall.O_CREAT: {
        disposition = FILE_OPEN_IF;
        break;
    }
    default: {
        disposition = FILE_OPEN;
        break;
    }}

    var fileAttrs = (uint32)FILE_ATTRIBUTE_NORMAL;
    if ((perm & (uint32)syscall.S_IWRITE) == 0) {
        fileAttrs = FILE_ATTRIBUTE_READONLY;
    }

    ref var h = ref heap(new syscallꓸHandle(), out var Ꮡh);
    error err = default;
    if (TestOpenatFallback && (flag & (uint64)O_NOFOLLOW_ANY) != 0) {
        err = STATUS_INVALID_PARAMETER;
    } else {
        err = NtCreateFile(
            Ꮡh,
            (uint32)SYNCHRONIZE | access,
            objAttrs,
            Ꮡ(new IO_STATUS_BLOCK()),
            nil,
            fileAttrs,
            (uint32)((UntypedInt)(FILE_SHARE_READ | FILE_SHARE_WRITE) | (uint32)FILE_SHARE_DELETE),
            disposition,
            (uint32)FILE_OPEN_FOR_BACKUP_INTENT | options,
            nil,
            0);
    }
    if (AreEqual(err, STATUS_INVALID_PARAMETER) && (flag & (uint64)O_NOFOLLOW_ANY) != 0) {
        (h, err) = openatFallback(name, (uint32)SYNCHRONIZE | access, objAttrs.Value, fileAttrs, disposition, (uint32)FILE_OPEN_FOR_BACKUP_INTENT | options);
    }
    if (err != nil) {
        return (h, ntCreateFileError(err, flag));
    }

    if ((flag & (uint64)syscall.O_TRUNC) != 0) {
        err = syscall.Ftruncate(h, 0);
        if (AreEqual(err, ERROR_INVALID_PARAMETER)) {
            // ERROR_INVALID_PARAMETER means truncation is not supported on this file handle.
            // Unix's O_TRUNC specification says to ignore O_TRUNC on named pipes and terminal devices.
            // We do the same here.
            {
                var (t, err1) = syscall.GetFileType(h); if (err1 == nil && (t == syscall.FILE_TYPE_PIPE || t == syscall.FILE_TYPE_CHAR)) {
                    err = default;
                }
            }
        }
        if (err != nil) {
            syscall.CloseHandle(h);
            return (syscall.InvalidHandle, err);
        }
    }

    return (h, default);
}

// TestOpenatFallback should only be used for testing purposes.
// When set, Openat simulates a system that does not support OBJ_DONT_REPARSE.
public static readonly ж<bool> ᏑTestOpenatFallback = new StaticBox<bool>(default(bool));
public static ref bool TestOpenatFallback => ref ((StaticBox<bool>)ᏑTestOpenatFallback).ValueSlot;

// openatFallback implements O_NOFOLLOW_ANY for a single path component on
// Windows versions that do not support OBJ_DONT_REPARSE (including Windows 10
// build 10240). See go.dev/issue/78131.
internal static (syscallꓸHandle, error) openatFallback(@string name, uint32 access, OBJECT_ATTRIBUTES attrsʗp, uint32 fileAttrs, uint32 disposition, uint32 options) {
    GoFrame ᒐ = default;
    try {
        ref var attrs = ref heap(attrsʗp, out var Ꮡattrs);

        // FILE_OPEN_REPARSE_POINT only prevents following the final component.
        // All current production callers that request O_NOFOLLOW_ANY pass a
        // single component: os.Root splits paths (including symlink targets)
        // before opening each component, and RemoveAll passes a basename or a
        // directory entry name relative to an open parent directory. Thus this
        // fallback also supports multi-component paths passed to those APIs.
        // Reject other paths here rather than weaken O_NOFOLLOW_ANY: this is
        // not a general replacement for OBJ_DONT_REPARSE.
        if (attrs.RootDirectory == 0 || name == ".."u8 || stringslite.IndexByte(name, (byte)'\\') >= 0 || stringslite.IndexByte(name, (byte)'/') >= 0 || stringslite.IndexByte(name, (byte)':') >= 0) {
            return (syscall.InvalidHandle, STATUS_INVALID_PARAMETER);
        }
        attrs.Attributes &= unchecked((uint32)~(uint32)(OBJ_DONT_REPARSE));
        ref var h = ref heap(new syscallꓸHandle(), out var Ꮡh);
        var err = NtCreateFile(
            Ꮡh, access, Ꮡattrs, Ꮡ(new IO_STATUS_BLOCK()), nil, fileAttrs,
            (uint32)((UntypedInt)(FILE_SHARE_READ | FILE_SHARE_WRITE) | (uint32)FILE_SHARE_DELETE), disposition,
            (options | (uint32)FILE_OPEN_REPARSE_POINT) & ~(uint32)FILE_DELETE_ON_CLOSE, nil, 0);
        if (err != nil) {
            return (syscall.InvalidHandle, err);
        }
        // Inspect the handle, not the path, before truncation or delete-on-close.
        // Skip this check if opening the reparse point itself was requested,
        // or if O_CREAT|O_EXCL requires creating a new file without following links.
        if ((options & (uint32)FILE_OPEN_REPARSE_POINT) == 0) {
            ref var info = ref heap(new syscall.ByHandleFileInformation(), out var Ꮡinfo);
            err = syscall.GetFileInformationByHandle(h, Ꮡinfo);
            if (err == nil && (info.FileAttributes & (uint32)syscall.FILE_ATTRIBUTE_REPARSE_POINT) != 0) {
                err = STATUS_REPARSE_POINT_ENCOUNTERED;
            }
            if (err != nil) {
                syscall.CloseHandle(h);
                return (syscall.InvalidHandle, err);
            }
        }
        if ((options & (uint32)FILE_DELETE_ON_CLOSE) == 0) {
            return (h, default);
        }
        defer(syscall.CloseHandle, h, ref ᒐ);

        // Only enable deletion after checking for a reparse point. An empty
        // name relative to h reopens the same file, even if it has been renamed
        // or its directory entry has been replaced since the first open.
        // The file already exists, including when created with O_EXCL.
        attrs.RootDirectory = h;
        attrs.ObjectName = Ꮡ(new NTUnicodeString());
        ref var dh = ref heap(new syscallꓸHandle(), out var Ꮡdh);
        err = NtOpenFile(
            Ꮡdh, access, Ꮡattrs, Ꮡ(new IO_STATUS_BLOCK()),
            (uint32)((UntypedInt)(FILE_SHARE_READ | FILE_SHARE_WRITE) | (uint32)FILE_SHARE_DELETE),
            options | (uint32)FILE_OPEN_REPARSE_POINT);
        if (err != nil) {
            return (syscall.InvalidHandle, err);
        }
        return (dh, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// ntCreateFileError maps error returns from NTCreateFile to user-visible errors.
internal static error ntCreateFileError(error err, uint64 flag) {
    var (s, ok) = err._<NTStatus>(ᐧ);
    if (!ok) {
        // The Openat fallback can also return Win32 errors.
        return err;
    }
    switch (s) {
    case var ᴛ when ᴛ == STATUS_REPARSE_POINT_ENCOUNTERED: {
        return syscall.ELOOP;
    }
    case var ᴛ when ᴛ == STATUS_NOT_A_DIRECTORY: {
        // ENOTDIR is the errno returned by open when O_DIRECTORY is specified
        // and the target is not a directory.
        //
        // NtCreateFile can return STATUS_NOT_A_DIRECTORY under other circumstances,
        // such as when opening "file/" where "file" is not a directory.
        // (This might be Windows version dependent.)
        //
        // Only map STATUS_NOT_A_DIRECTORY to ENOTDIR when O_DIRECTORY is specified.
        if ((flag & (uint64)O_DIRECTORY) != 0) {
            return syscall.ENOTDIR;
        }
        break;
    }
    case var ᴛ when ᴛ == STATUS_FILE_IS_A_DIRECTORY: {
        return syscall.EISDIR;
    }
    case var ᴛ when ᴛ == STATUS_OBJECT_NAME_COLLISION: {
        return syscall.EEXIST;
    }}
    return s.Errno();
}

public static error Mkdirat(syscallꓸHandle dirfd, @string name, uint32 mode) {
    var objAttrs = Ꮡ(new OBJECT_ATTRIBUTES());
    {
        var errΔ1 = objAttrs.init(dirfd, name); if (errΔ1 != nil) {
            return errΔ1;
        }
    }
    ref var h = ref heap(new syscallꓸHandle(), out var Ꮡh);
    var err = NtCreateFile(
        Ꮡh,
        FILE_GENERIC_READ,
        objAttrs,
        Ꮡ(new IO_STATUS_BLOCK()),
        nil,
        syscall.FILE_ATTRIBUTE_NORMAL,
        (uint32)((UntypedInt)(syscall.FILE_SHARE_READ | syscall.FILE_SHARE_WRITE) | (uint32)syscall.FILE_SHARE_DELETE),
        FILE_CREATE,
        (uint32)((uint32)FILE_DIRECTORY_FILE | (uint32)FILE_OPEN_REPARSE_POINT),
        nil,
        0);
    if (err != nil) {
        return ntCreateFileError(err, 0);
    }
    syscall.CloseHandle(h);
    return default;
}

public static error Deleteat(syscallꓸHandle dirfd, @string name, uint32 options) {
    GoFrame ᒐ = default;
    try {
        if (name == "."u8) {
            // NtOpenFile's documentation isn't explicit about what happens when deleting ".".
            // Make this an error consistent with that of POSIX.
            return syscall.EINVAL;
        }
        var objAttrs = Ꮡ(new OBJECT_ATTRIBUTES());
        {
            var errΔ1 = objAttrs.init(dirfd, name); if (errΔ1 != nil) {
                return errΔ1;
            }
        }
        ref var h = ref heap(new syscallꓸHandle(), out var Ꮡh);
        var err = NtOpenFile(
            Ꮡh,
            (uint32)((uint32)FILE_READ_ATTRIBUTES | (uint32)DELETE),
            objAttrs,
            Ꮡ(new IO_STATUS_BLOCK()),
            (uint32)((UntypedInt)(FILE_SHARE_DELETE | FILE_SHARE_READ) | (uint32)FILE_SHARE_WRITE),
            (uint32)((uint32)FILE_OPEN_REPARSE_POINT | (uint32)FILE_OPEN_FOR_BACKUP_INTENT) | options);
        if (err != nil) {
            {
                var (ntStatus, ok) = err._<NTStatus>(ᐧ); if (!ok || ntStatus != STATUS_ACCESS_DENIED) {
                    return ntCreateFileError(err, 0);
                }
            }

            // Access denied, try opening with DELETE only.
            // This may succeed if the file has restrictive permissions
            // but the caller has delete child permission on the parent directory.
            err = NtOpenFile(
                Ꮡh,
                DELETE,
                objAttrs,
                Ꮡ(new IO_STATUS_BLOCK()),
                (uint32)((UntypedInt)(FILE_SHARE_DELETE | FILE_SHARE_READ) | (uint32)FILE_SHARE_WRITE),
                (uint32)((uint32)FILE_OPEN_REPARSE_POINT | (uint32)FILE_OPEN_FOR_BACKUP_INTENT) | options);
            if (err != nil) {
                return ntCreateFileError(err, 0);
            }
        }
        defer(syscall.CloseHandle, h, ref ᒐ);

        if (TestDeleteatFallback) {
            return deleteatFallback(h);
        }

        const uint32 FileDispositionInformationEx = 64;

        // First, attempt to delete the file using POSIX semantics
        // (which permit a file to be deleted while it is still open).
        // This matches the behavior of DeleteFileW.
        //
        // The following call uses features available on different Windows versions:
        // - FILE_DISPOSITION_INFORMATION_EX: Windows 10, version 1607 (aka RS1)
        // - FILE_DISPOSITION_POSIX_SEMANTICS: Windows 10, version 1607 (aka RS1)
        // - FILE_DISPOSITION_IGNORE_READONLY_ATTRIBUTE: Windows 10, version 1809 (aka RS5)
        //
        // Also, some file systems, like FAT32, don't support POSIX semantics.
        err = NtSetInformationFile(
            h,
            Ꮡ(new IO_STATUS_BLOCK()),
            @unsafe.Pointer.FromPinnedBox(Ꮡ(new FILE_DISPOSITION_INFORMATION_EX(
                Flags: (uint32)((UntypedInt)((UntypedInt)(FILE_DISPOSITION_DELETE | FILE_DISPOSITION_FORCE_IMAGE_SECTION_CHECK) | FILE_DISPOSITION_POSIX_SEMANTICS) |
                    // This differs from DeleteFileW, but matches os.Remove's
                    // behavior on Unix platforms of permitting deletion of
                    // read-only files.
                    (uint32)FILE_DISPOSITION_IGNORE_READONLY_ATTRIBUTE)
            ))),
            (uint32)/* unsafe.Sizeof(FILE_DISPOSITION_INFORMATION_EX{}) */ (uintptr)4,
            FileDispositionInformationEx);
        switch (err) {
        case var ᴛ when AreEqual(ᴛ, default): {
            return default;
        }
        case var ᴛ when AreEqual(ᴛ, STATUS_INVALID_INFO_CLASS) || AreEqual(ᴛ, STATUS_INVALID_PARAMETER) || AreEqual(ᴛ, STATUS_NOT_SUPPORTED): { // the file system doesn't support FILE_DISPOSITION_INFORMATION_EX or one of the flags
            // the operating system doesn't support FileDispositionInformationEx
            // the operating system doesn't support one of the flags
            return deleteatFallback(h);
        }
        default: {
            return err._<NTStatus>().Errno();
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// TestDeleteatFallback should only be used for testing purposes.
// When set, [Deleteat] uses the fallback path unconditionally.
public static readonly ж<bool> ᏑTestDeleteatFallback = new StaticBox<bool>(default(bool));
public static ref bool TestDeleteatFallback => ref ((StaticBox<bool>)ᏑTestDeleteatFallback).ValueSlot;

// deleteatFallback is a deleteat implementation that strives
// for compatibility with older Windows versions and file systems
// over performance.
internal static error deleteatFallback(syscallꓸHandle h) {
    ref var data = ref heap(new syscall.ByHandleFileInformation(), out var Ꮡdata);
    {
        var err = syscall.GetFileInformationByHandle(h, Ꮡdata); if (err == nil && (data.FileAttributes & (uint32)syscall.FILE_ATTRIBUTE_READONLY) != 0) {
            // Remove read-only attribute. Reopen the file, as it was previously open without FILE_WRITE_ATTRIBUTES access
            // in order to maximize compatibility in the happy path.
            var (wh, errΔ1) = ReOpenFile(h,
                FILE_WRITE_ATTRIBUTES,
                (uint32)((UntypedInt)(FILE_SHARE_READ | FILE_SHARE_WRITE) | (uint32)FILE_SHARE_DELETE),
                (uint32)((uint32)syscall.FILE_FLAG_OPEN_REPARSE_POINT | (uint32)syscall.FILE_FLAG_BACKUP_SEMANTICS));
            if (errΔ1 != nil) {
                return errΔ1;
            }
            errΔ1 = SetFileInformationByHandle(
                wh,
                FileBasicInfo,
                @unsafe.Pointer.FromPinnedBox(Ꮡ(new FILE_BASIC_INFO(
                    FileAttributes: (data.FileAttributes & ~(uint32)FILE_ATTRIBUTE_READONLY)
                ))),
                (uint32)/* unsafe.Sizeof(FILE_BASIC_INFO{}) */ (uintptr)40);
            syscall.CloseHandle(wh);
            if (errΔ1 != nil) {
                return errΔ1;
            }
        }
    }

    return SetFileInformationByHandle(
        h,
        FileDispositionInfo,
        @unsafe.Pointer.FromPinnedBox(Ꮡ(new FILE_DISPOSITION_INFO(
            DeleteFile: true
        ))),
        (uint32)/* unsafe.Sizeof(FILE_DISPOSITION_INFO{}) */ (uintptr)1);
}

public static error Renameat(syscallꓸHandle olddirfd, @string oldpath, syscallꓸHandle newdirfd, @string newpath) {
    GoFrame ᒐ = default;
    try {
        var objAttrs = Ꮡ(new OBJECT_ATTRIBUTES());
        {
            var errΔ1 = objAttrs.init(olddirfd, oldpath); if (errΔ1 != nil) {
                return errΔ1;
            }
        }
        ref var h = ref heap(new syscallꓸHandle(), out var Ꮡh);
        var err = NtOpenFile(
            Ꮡh,
            (uint32)((uint32)SYNCHRONIZE | (uint32)DELETE),
            objAttrs,
            Ꮡ(new IO_STATUS_BLOCK()),
            (uint32)((UntypedInt)(FILE_SHARE_DELETE | FILE_SHARE_READ) | (uint32)FILE_SHARE_WRITE),
            (uint32)((UntypedInt)(FILE_OPEN_REPARSE_POINT | FILE_OPEN_FOR_BACKUP_INTENT) | (uint32)FILE_SYNCHRONOUS_IO_NONALERT));
        if (err != nil) {
            return ntCreateFileError(err, 0);
        }
        defer(syscall.CloseHandle, h, ref ᒐ);

        ref var renameInfoEx = ref heap<FILE_RENAME_INFORMATION_EX>(new FILE_RENAME_INFORMATION_EX(
            Flags: (uint32)((uint32)FILE_RENAME_REPLACE_IF_EXISTS | (uint32)FILE_RENAME_POSIX_SEMANTICS),
            RootDirectory: newdirfd
        ), out var ᏑrenameInfoEx);
        (var p16, err) = syscall.UTF16FromString(newpath);
        if (err != nil) {
            return err;
        }
        if (len(p16) > len(renameInfoEx.FileName)) {
            return syscall.EINVAL;
        }
        copy(renameInfoEx.FileName[..], p16);
        renameInfoEx.FileNameLength = (uint32)((len(p16) - 1) * 2);

        const uint32 FileRenameInformation = 10;
        const uint32 FileRenameInformationEx = 65;
        err = NtSetInformationFile(
            h,
            Ꮡ(new IO_STATUS_BLOCK()),
            @unsafe.Pointer.FromPinnedBox(ᏑrenameInfoEx),
            (uint32)/* unsafe.Sizeof(FILE_RENAME_INFORMATION_EX{}) */ (uintptr)544,
            FileRenameInformationEx);
        if (err == nil) {
            return default;
        }

        // If the prior rename failed, the filesystem might not support
        // POSIX semantics (for example, FAT), or might not have implemented
        // FILE_RENAME_INFORMATION_EX.
        //
        // Try again.
        ref var renameInfo = ref heap<FILE_RENAME_INFORMATION>(new FILE_RENAME_INFORMATION(
            ReplaceIfExists: true,
            RootDirectory: newdirfd
        ), out var ᏑrenameInfo);
        copy(renameInfo.FileName[..], p16);
        renameInfo.FileNameLength = renameInfoEx.FileNameLength;

        err = NtSetInformationFile(
            h,
            Ꮡ(new IO_STATUS_BLOCK()),
            @unsafe.Pointer.FromPinnedBox(ᏑrenameInfo),
            (uint32)/* unsafe.Sizeof(FILE_RENAME_INFORMATION{}) */ (uintptr)544,
            FileRenameInformation);
        {
            var (st, ok) = err._<NTStatus>(ᐧ); if (ok) {
                return st.Errno();
            }
        }
        return err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static error Linkat(syscallꓸHandle olddirfd, @string oldpath, syscallꓸHandle newdirfd, @string newpath) {
    GoFrame ᒐ = default;
    try {
        var objAttrs = Ꮡ(new OBJECT_ATTRIBUTES());
        {
            var errΔ1 = objAttrs.init(olddirfd, oldpath); if (errΔ1 != nil) {
                return errΔ1;
            }
        }
        ref var h = ref heap(new syscallꓸHandle(), out var Ꮡh);
        var err = NtOpenFile(
            Ꮡh,
            (uint32)((uint32)SYNCHRONIZE | (uint32)FILE_WRITE_ATTRIBUTES),
            objAttrs,
            Ꮡ(new IO_STATUS_BLOCK()),
            (uint32)((UntypedInt)(FILE_SHARE_DELETE | FILE_SHARE_READ) | (uint32)FILE_SHARE_WRITE),
            (uint32)((UntypedInt)(FILE_OPEN_REPARSE_POINT | FILE_OPEN_FOR_BACKUP_INTENT) | (uint32)FILE_SYNCHRONOUS_IO_NONALERT));
        if (err != nil) {
            return ntCreateFileError(err, 0);
        }
        defer(syscall.CloseHandle, h, ref ᒐ);

        ref var linkInfo = ref heap<FILE_LINK_INFORMATION>(new FILE_LINK_INFORMATION(
            RootDirectory: newdirfd
        ), out var ᏑlinkInfo);
        (var p16, err) = syscall.UTF16FromString(newpath);
        if (err != nil) {
            return err;
        }
        if (len(p16) > len(linkInfo.FileName)) {
            return syscall.EINVAL;
        }
        copy(linkInfo.FileName[..], p16);
        linkInfo.FileNameLength = (uint32)((len(p16) - 1) * 2);

        const uint32 FileLinkInformation = 11;
        err = NtSetInformationFile(
            h,
            Ꮡ(new IO_STATUS_BLOCK()),
            @unsafe.Pointer.FromPinnedBox(ᏑlinkInfo),
            (uint32)/* unsafe.Sizeof(FILE_LINK_INFORMATION{}) */ (uintptr)544,
            FileLinkInformation);
        {
            var (st, ok) = err._<NTStatus>(ᐧ); if (ok) {
                return st.Errno();
            }
        }
        return err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// SymlinkatFlags configure Symlinkat.
//
// Symbolic links have two properties: They may be directory or file links,
// and they may be absolute or relative.
//
// The Windows API defines flags describing these properties
// (SYMBOLIC_LINK_FLAG_DIRECTORY and SYMLINK_FLAG_RELATIVE),
// but the flags are passed to different system calls and
// do not have distinct values, so we define our own enumeration
// that permits expressing both.
partial struct SymlinkatFlags /*num:nuint*/;

public static SymlinkatFlags SYMLINKAT_DIRECTORY => /* SymlinkatFlags(1 << iota) */ 1;
public static SymlinkatFlags SYMLINKAT_RELATIVE => 2;

public static error Symlinkat(@string oldname, syscallꓸHandle newdirfd, @string newname, SymlinkatFlags flags) {
    // Temporarily acquire symlink-creating privileges if possible.
    // This is the behavior of CreateSymbolicLinkW.
    //
    // (When passed the SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE flag,
    // CreateSymbolicLinkW ignores errors in acquiring privileges, as we do here.)
    return withPrivilege("SeCreateSymbolicLinkPrivilege"u8, () => symlinkat(oldname, newdirfd, newname, flags));
}

// https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/ntifs/ns-ntifs-_reparse_data_buffer
[StructLayout(LayoutKind.Explicit, Size = 20)] internal partial struct symlinkat_reparseDataBufferT /*dyn*/ {
    [FieldOffset(0)] internal readonly structs.HostLayout _;
    [FieldOffset(0)] public uint32 ReparseTag;
    [FieldOffset(4)] public uint16 ReparseDataLength;
    [FieldOffset(6)] public uint16 Reserved;
    [FieldOffset(8)] public uint16 SubstituteNameOffset;
    [FieldOffset(10)] public uint16 SubstituteNameLength;
    [FieldOffset(12)] public uint16 PrintNameOffset;
    [FieldOffset(14)] public uint16 PrintNameLength;
    [FieldOffset(16)] public uint32 Flags;
}

internal static error symlinkat(@string oldname, syscallꓸHandle newdirfd, @string newname, SymlinkatFlags flags) {
    GoFrame ᒐ = default;
    try {
        var (oldnameu16, err) = syscall.UTF16FromString(oldname);
        if (err != nil) {
            return err;
        }
        oldnameu16 = oldnameu16.slice(0, len(oldnameu16) - 1); // trim off terminal NUL

        uint32 options = default;
        if ((SymlinkatFlags)(flags & SYMLINKAT_DIRECTORY) != 0) {
            options |= (uint32)(FILE_DIRECTORY_FILE);
        } else {
            options |= (uint32)(FILE_NON_DIRECTORY_FILE);
        }

        var objAttrs = Ꮡ(new OBJECT_ATTRIBUTES());
        {
            var errΔ1 = objAttrs.init(newdirfd, newname); if (errΔ1 != nil) {
                return errΔ1;
            }
        }
        ref var h = ref heap(new syscallꓸHandle(), out var Ꮡh);
        err = NtCreateFile(
            Ꮡh,
            (uint32)((UntypedInt)(SYNCHRONIZE | FILE_WRITE_ATTRIBUTES) | (uint32)DELETE),
            objAttrs,
            Ꮡ(new IO_STATUS_BLOCK()),
            nil,
            syscall.FILE_ATTRIBUTE_NORMAL,
            0,
            FILE_CREATE,
            (uint32)((UntypedInt)(FILE_OPEN_REPARSE_POINT | FILE_OPEN_FOR_BACKUP_INTENT) | (uint32)FILE_SYNCHRONOUS_IO_NONALERT) | options,
            nil,
            0);
        if (err != nil) {
            return ntCreateFileError(err, 0);
        }
        defer(syscall.CloseHandle, h, ref ᒐ);

        const uint16 headerSize = /* uint16(unsafe.Offsetof(reparseDataBufferT{}.SubstituteNameOffset)) */ 8;
        const uint16 bufferSize = /* uint16(unsafe.Sizeof(reparseDataBufferT{})) */ 20;

        // Data buffer containing a SymbolicLinkReparseBuffer followed by the link target.
        var rdbbuf = new slice<byte>((uint16)(bufferSize + (uint16)(2 * len(oldnameu16))));

        var rdb = Ꮡ(rdbbuf, 0).Reinterpret<byte, symlinkat_reparseDataBufferT>();
        rdb.ReparseTag = syscall.IO_REPARSE_TAG_SYMLINK;
        rdb.ReparseDataLength = (uint16)((uint16)len(rdbbuf) - (uint16)headerSize);
        rdb.SubstituteNameOffset = 0;
        rdb.SubstituteNameLength = (uint16)(2 * len(oldnameu16));
        rdb.PrintNameOffset = 0;
        rdb.PrintNameLength = rdb.SubstituteNameLength;
        if ((SymlinkatFlags)(flags & SYMLINKAT_RELATIVE) != 0) {
            rdb.Flags = SYMLINK_FLAG_RELATIVE;
        }

        var namebuf = rdbbuf[(int)(bufferSize)..];
        copy(namebuf, @unsafe.String(Ꮡ(oldnameu16, 0).Reinterpret<uint16, byte>(), 2 * len(oldnameu16)));

        err = syscall.DeviceIoControl(
            h,
            FSCTL_SET_REPARSE_POINT,
            Ꮡ(rdbbuf, 0),
            (uint32)len(rdbbuf),
            nil,
            0,
            nil,
            nil);
        if (err != nil) {
            // Creating the symlink has failed, so try to remove the file.
            const uint32 FileDispositionInformation = 13;
            NtSetInformationFile(
                h,
                Ꮡ(new IO_STATUS_BLOCK()),
                @unsafe.Pointer.FromPinnedBox(Ꮡ(new FILE_DISPOSITION_INFORMATION(
                    DeleteFile: true
                ))),
                (uint32)/* unsafe.Sizeof(FILE_DISPOSITION_INFORMATION{}) */ (uintptr)1,
                FileDispositionInformation);
            return err;
        }

        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// withPrivilege temporariliy acquires the named privilege and runs f.
// If the privilege cannot be acquired it runs f anyway,
// which should fail with an appropriate error.
internal static error withPrivilege(@string privilege, Func<error> f) {
    GoFrame ᒐ = default;
    try {
        runtime.LockOSThread();
        defer(runtime.UnlockOSThread, ref ᒐ);

        var err = ImpersonateSelf(SecurityImpersonation);
        if (err != nil) {
            return f();
        }
        defer(() => RevertToSelf(), ref ᒐ);

        (var curThread, err) = GetCurrentThread();
        if (err != nil) {
            return f();
        }
        ref var token = ref heap(new syscall.Token(), out var Ꮡtoken);
        err = OpenThreadToken(curThread, (uint32)((uint32)syscall.TOKEN_QUERY | (uint32)TOKEN_ADJUST_PRIVILEGES), false, Ꮡtoken);
        if (err != nil) {
            return f();
        }
        defer(syscall.CloseHandle, (syscallꓸHandle)(uintptr)token, ref ᒐ);

        (var privStr, err) = syscall.UTF16PtrFromString(privilege);
        if (err != nil) {
            return f();
        }
        ref var tokenPriv = ref heap(new TOKEN_PRIVILEGES(), out var ᏑtokenPriv);
        err = LookupPrivilegeValue(nil, privStr, ᏑtokenPriv.at(TOKEN_PRIVILEGES.ᏑPrivileges, 0).of(LUID_AND_ATTRIBUTES.ᏑLuid));
        if (err != nil) {
            return f();
        }

        tokenPriv.PrivilegeCount = 1;
        tokenPriv.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
        err = AdjustTokenPrivileges(token, false, ᏑtokenPriv, 0, nil, nil);
        if (err != nil) {
            return f();
        }

        return f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

} // end windows_package
