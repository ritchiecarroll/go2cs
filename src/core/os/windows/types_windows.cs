// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using filepathlite = @internal.filepathlite_package;
using godebug = @internal.godebug_package;
using windows = @internal.syscall.windows_package;
using Δsync = sync_package;
using syscall = syscall_package;
using time = time_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.syscall;
using fs = go.io.fs_package;
using go.io;

partial class os_package {

// A fileStat is the implementation of FileInfo returned by Stat and Lstat.
partial struct fileStat {
    internal @string name;
    // from ByHandleFileInformation, Win32FileAttributeData, Win32finddata, and GetFileInformationByHandleEx
    public uint32 FileAttributes;
    public syscall.Filetime CreationTime;
    public syscall.Filetime LastAccessTime;
    public syscall.Filetime LastWriteTime;
    public uint32 FileSizeHigh;
    public uint32 FileSizeLow;
    // from Win32finddata and GetFileInformationByHandleEx
    public uint32 ReparseTag;
    // what syscall.GetFileType returns
    internal uint32 filetype;
    // used to implement SameFile
    public partial ref sync_package.Mutex Mutex { get; }
    internal @string path;
    internal uint32 vol;
    internal uint32 idxhi;
    internal uint32 idxlo;
    internal bool appendNameToPath;
}

// newFileStatFromGetFileInformationByHandle calls GetFileInformationByHandle
// to gather all required information about the file handle h.
internal static (ж<fileStat> fs, error err) newFileStatFromGetFileInformationByHandle(@string path, syscallꓸHandle h) {
    error err = default!;

    ref var d = ref heap(new syscall.ByHandleFileInformation(), out var Ꮡd);
    err = syscall.GetFileInformationByHandle(h, Ꮡd);
    if (err != default!) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "GetFileInformationByHandle"u8, Path: path, Err: err))));
    }
    ref var reparseTag = ref heap(new uint32(), out var ᏑreparseTag);
    if ((uint32)(d.FileAttributes & (uint32)syscall.FILE_ATTRIBUTE_REPARSE_POINT) != 0) {
        ref var ti = ref heap(new windows.FILE_ATTRIBUTE_TAG_INFO(), out var Ꮡti);
        err = windows.GetFileInformationByHandleEx(h, windows.FileAttributeTagInfo, Ꮡti.Reinterpret<windows.FILE_ATTRIBUTE_TAG_INFO, byte>(), (uint32)/* unsafe.Sizeof(ti) */ (uintptr)8);
        if (err != default!) {
            return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "GetFileInformationByHandleEx"u8, Path: path, Err: err))));
        }
        reparseTag = ti.ReparseTag;
    }
    return (Ꮡ(new fileStat(
        name: filepathlite.Base(path),
        FileAttributes: d.FileAttributes,
        CreationTime: d.CreationTime,
        LastAccessTime: d.LastAccessTime,
        LastWriteTime: d.LastWriteTime,
        FileSizeHigh: d.FileSizeHigh,
        FileSizeLow: d.FileSizeLow,
        vol: d.VolumeSerialNumber,
        idxhi: d.FileIndexHigh,
        idxlo: d.FileIndexLow,
        ReparseTag: reparseTag
    )), default!);
}

// fileStat.path is used by os.SameFile to decide if it needs
// to fetch vol, idxhi and idxlo. But these are already set,
// so set fileStat.path to "" to prevent os.SameFile doing it again.

// newFileStatFromWin32FileAttributeData copies all required information
// from syscall.Win32FileAttributeData d into the newly created fileStat.
internal static ж<fileStat> newFileStatFromWin32FileAttributeData(ref syscall.Win32FileAttributeData d) {
    return Ꮡ(new fileStat(
        FileAttributes: d.FileAttributes,
        CreationTime: d.CreationTime,
        LastAccessTime: d.LastAccessTime,
        LastWriteTime: d.LastWriteTime,
        FileSizeHigh: d.FileSizeHigh,
        FileSizeLow: d.FileSizeLow
    ));
}

// newFileStatFromFileIDBothDirInfo copies all required information
// from windows.FILE_ID_BOTH_DIR_INFO d into the newly created fileStat.
internal static ж<fileStat> newFileStatFromFileIDBothDirInfo(ж<windows.FILE_ID_BOTH_DIR_INFO> Ꮡd) {
    ref var d = ref Ꮡd.DerefOrNull();

    // The FILE_ID_BOTH_DIR_INFO MSDN documentations isn't completely correct.
    // FileAttributes can contain any file attributes that is currently set on the file,
    // not just the ones documented.
    // EaSize contains the reparse tag if the file is a reparse point.
    return Ꮡ(new fileStat(
        FileAttributes: d.FileAttributes,
        CreationTime: d.CreationTime,
        LastAccessTime: d.LastAccessTime,
        LastWriteTime: d.LastWriteTime,
        FileSizeHigh: (uint32)((d.EndOfFile >> (int)(32))),
        FileSizeLow: (uint32)d.EndOfFile,
        ReparseTag: d.EaSize,
        idxhi: (uint32)((d.FileID >> (int)(32))),
        idxlo: (uint32)d.FileID
    ));
}

// newFileStatFromFileFullDirInfo copies all required information
// from windows.FILE_FULL_DIR_INFO d into the newly created fileStat.
internal static ж<fileStat> newFileStatFromFileFullDirInfo(ж<windows.FILE_FULL_DIR_INFO> Ꮡd) {
    ref var d = ref Ꮡd.DerefOrNull();

    return Ꮡ(new fileStat(
        FileAttributes: d.FileAttributes,
        CreationTime: d.CreationTime,
        LastAccessTime: d.LastAccessTime,
        LastWriteTime: d.LastWriteTime,
        FileSizeHigh: (uint32)((d.EndOfFile >> (int)(32))),
        FileSizeLow: (uint32)d.EndOfFile,
        ReparseTag: d.EaSize
    ));
}

// newFileStatFromWin32finddata copies all required information
// from syscall.Win32finddata d into the newly created fileStat.
internal static ж<fileStat> newFileStatFromWin32finddata(ref syscall.Win32finddata d) {
    var fs = Ꮡ(new fileStat(
        FileAttributes: d.FileAttributes,
        CreationTime: d.CreationTime,
        LastAccessTime: d.LastAccessTime,
        LastWriteTime: d.LastWriteTime,
        FileSizeHigh: d.FileSizeHigh,
        FileSizeLow: d.FileSizeLow
    ));
    if ((uint32)(d.FileAttributes & (uint32)syscall.FILE_ATTRIBUTE_REPARSE_POINT) != 0) {
        // Per https://learn.microsoft.com/en-us/windows/win32/api/minwinbase/ns-minwinbase-win32_find_dataw:
        // “If the dwFileAttributes member includes the FILE_ATTRIBUTE_REPARSE_POINT
        // attribute, this member specifies the reparse point tag. Otherwise, this
        // value is undefined and should not be used.”
        fs.Value.ReparseTag = d.Reserved0;
    }
    return fs;
}

// isReparseTagNameSurrogate determines whether a tag's associated
// reparse point is a surrogate for another named entity (for example, a mounted folder).
//
// See https://learn.microsoft.com/en-us/windows/win32/api/winnt/nf-winnt-isreparsetagnamesurrogate
// and https://learn.microsoft.com/en-us/windows/win32/fileio/reparse-point-tags.
internal static bool isReparseTagNameSurrogate(this ref fileStat fs) {
    // True for IO_REPARSE_TAG_SYMLINK and IO_REPARSE_TAG_MOUNT_POINT.
    return (uint32)(fs.FileAttributes & (uint32)syscall.FILE_ATTRIBUTE_REPARSE_POINT) != 0 && (uint32)(fs.ReparseTag & 0x20000000) != 0;
}

internal static int64 Size(this ref fileStat fs) {
    return ((int64)fs.FileSizeHigh << (int)(32)) + (int64)fs.FileSizeLow;
}

internal static ж<godebug.Setting> winsymlink = godebug.New("winsymlink"u8);

internal static FileMode Mode(this ref fileStat fs) {
    var m = fs.mode();
    if (winsymlink.Value() == "0"u8) {
        var old = fs.modePreGo1_23();
        if (old != m) {
            winsymlink.IncNonDefault();
            m = old;
        }
    }
    return m;
}

internal static FileMode /*m*/ mode(this ref fileStat fs) {
    FileMode m = default!;

    if ((uint32)(fs.FileAttributes & (uint32)syscall.FILE_ATTRIBUTE_READONLY) != 0){
        m |= (FileMode)(292);
    } else {
        m |= (FileMode)(438);
    }
    // Windows reports the FILE_ATTRIBUTE_DIRECTORY bit for reparse points
    // that refer to directories, such as symlinks and mount points.
    // However, we follow symlink POSIX semantics and do not set the mode bits.
    // This allows users to walk directories without following links
    // by just calling "fi, err := os.Lstat(name); err == nil && fi.IsDir()".
    // Note that POSIX only defines the semantics for symlinks, not for
    // mount points or other surrogate reparse points, but we treat them
    // the same way for consistency. Also, mount points can contain infinite
    // loops, so it is not safe to walk them without special handling.
    if (!fs.isReparseTagNameSurrogate()) {
        if ((uint32)(fs.FileAttributes & (uint32)syscall.FILE_ATTRIBUTE_DIRECTORY) != 0) {
            m |= (fs.FileMode)((fs.FileMode)(ModeDir | 73));
        }
        var exprᴛ1 = fs.filetype;
        if (exprᴛ1 == syscall.FILE_TYPE_PIPE) {
            m |= (fs.FileMode)(ModeNamedPipe);
        }
        else if (exprᴛ1 == syscall.FILE_TYPE_CHAR) {
            m |= (fs.FileMode)((fs.FileMode)(ModeDevice | ModeCharDevice));
        }

    }
    if ((uint32)(fs.FileAttributes & (uint32)syscall.FILE_ATTRIBUTE_REPARSE_POINT) != 0) {
        var exprᴛ2 = fs.ReparseTag;
        if (exprᴛ2 == syscall.IO_REPARSE_TAG_SYMLINK) {
            m |= (fs.FileMode)(ModeSymlink);
        }
        else if (exprᴛ2 == windows.IO_REPARSE_TAG_AF_UNIX) {
            m |= (fs.FileMode)(ModeSocket);
        }
        else if (exprᴛ2 == windows.IO_REPARSE_TAG_DEDUP) {
        }
        else { /* default: */
            m |= (fs.FileMode)(ModeIrregular);
        }

    }
    // If the Data Deduplication service is enabled on Windows Server, its
    // Optimization job may convert regular files to IO_REPARSE_TAG_DEDUP
    // whenever that job runs.
    //
    // However, DEDUP reparse points remain similar in most respects to
    // regular files: they continue to support random-access reads and writes
    // of persistent data, and they shouldn't add unexpected latency or
    // unavailability in the way that a network filesystem might.
    //
    // Go programs may use ModeIrregular to filter out unusual files (such as
    // raw device files on Linux, POSIX FIFO special files, and so on), so
    // to avoid files changing unpredictably from regular to irregular we will
    // consider DEDUP files to be close enough to regular to treat as such.
    return m;
}

// modePreGo1_23 returns the FileMode for the fileStat, using the pre-Go 1.23
// logic for determining the file mode.
// The logic is subtle and not well-documented, so it is better to keep it
// separate from the new logic.
internal static FileMode /*m*/ modePreGo1_23(this ref fileStat fs) {
    FileMode m = default!;

    if ((uint32)(fs.FileAttributes & (uint32)syscall.FILE_ATTRIBUTE_READONLY) != 0){
        m |= (FileMode)(292);
    } else {
        m |= (FileMode)(438);
    }
    if (fs.ReparseTag == syscall.IO_REPARSE_TAG_SYMLINK || fs.ReparseTag == windows.IO_REPARSE_TAG_MOUNT_POINT) {
        return (FileMode)(m | ModeSymlink);
    }
    if ((uint32)(fs.FileAttributes & (uint32)syscall.FILE_ATTRIBUTE_DIRECTORY) != 0) {
        m |= (fs.FileMode)((fs.FileMode)(ModeDir | 73));
    }
    var exprᴛ1 = fs.filetype;
    if (exprᴛ1 == syscall.FILE_TYPE_PIPE) {
        m |= (fs.FileMode)(ModeNamedPipe);
    }
    else if (exprᴛ1 == syscall.FILE_TYPE_CHAR) {
        m |= (fs.FileMode)((fs.FileMode)(ModeDevice | ModeCharDevice));
    }

    if ((uint32)(fs.FileAttributes & (uint32)syscall.FILE_ATTRIBUTE_REPARSE_POINT) != 0) {
        if (fs.ReparseTag == windows.IO_REPARSE_TAG_AF_UNIX) {
            m |= (fs.FileMode)(ModeSocket);
        }
        if ((FileMode)(m & ModeType) == 0) {
            if (fs.ReparseTag == windows.IO_REPARSE_TAG_DEDUP){
            } else {
                // See comment in fs.Mode.
                m |= (fs.FileMode)(ModeIrregular);
            }
        }
    }
    return m;
}

internal static time.Time ModTime(this ref fileStat fs) {
    return time.Unix(0, fs.LastWriteTime.Nanoseconds());
}

// Sys returns syscall.Win32FileAttributeData for file fs.
internal static any Sys(this ref fileStat fs) {
    return Ꮡ(new syscall.Win32FileAttributeData(
        FileAttributes: fs.FileAttributes,
        CreationTime: fs.CreationTime,
        LastAccessTime: fs.LastAccessTime,
        LastWriteTime: fs.LastWriteTime,
        FileSizeHigh: fs.FileSizeHigh,
        FileSizeLow: fs.FileSizeLow
    ));
}

internal static error loadFileId(this ж<fileStat> Ꮡfs) {
    GoFrame ᒐ = default;
    try {
        ref var fs = ref Ꮡfs.DerefOrNull();

        Ꮡfs.of(fileStat.ᏑMutex).Lock();
        defer(Ꮡfs.of(fileStat.ᏑMutex).Unlock, ref ᒐ);
        if (fs.path == ""u8) {
            // already done
            return default!;
        }
        @string path = default!;
        if (fs.appendNameToPath){
            path = fixLongPath(fs.path + @"\"u8 + fs.name);
        } else {
            path = fs.path;
        }
        var (pathp, err) = syscall.UTF16PtrFromString(path);
        if (err != default!) {
            return err;
        }
        // Per https://learn.microsoft.com/en-us/windows/win32/fileio/reparse-points-and-file-operations,
        // “Applications that use the CreateFile function should specify the
        // FILE_FLAG_OPEN_REPARSE_POINT flag when opening the file if it is a reparse
        // point.”
        //
        // And per https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-createfilew,
        // “If the file is not a reparse point, then this flag is ignored.”
        //
        // So we set FILE_FLAG_OPEN_REPARSE_POINT unconditionally, since we want
        // information about the reparse point itself.
        //
        // If the file is a symlink, the symlink target should have already been
        // resolved when the fileStat was created, so we don't need to worry about
        // resolving symlink reparse points again here.
        var attrs = (uint32)((uint32)((uint32)syscall.FILE_FLAG_BACKUP_SEMANTICS | (uint32)syscall.FILE_FLAG_OPEN_REPARSE_POINT));
        (var h, err) = syscall.CreateFile(pathp, 0, 0, nil, syscall.OPEN_EXISTING, attrs, 0);
        if (err != default!) {
            return err;
        }
        defer(syscall.CloseHandle, h, ref ᒐ);
        ref var i = ref heap(new syscall.ByHandleFileInformation(), out var Ꮡi);
        err = syscall.GetFileInformationByHandle(h, Ꮡi);
        if (err != default!) {
            return err;
        }
        fs.path = ""u8;
        fs.vol = i.VolumeSerialNumber;
        fs.idxhi = i.FileIndexHigh;
        fs.idxlo = i.FileIndexLow;
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// saveInfoFromPath saves full path of the file to be used by os.SameFile later,
// and set name from path.
internal static error saveInfoFromPath(this ref fileStat fs, @string path) {
    fs.path = path;
    if (!filepathlite.IsAbs(fs.path)) {
        error err = default!;
        (fs.path, err) = syscall.FullPath(fs.path);
        if (err != default!) {
            return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "FullPath"u8, Path: path, Err: err)));
        }
    }
    fs.name = filepathlite.Base(path);
    return default!;
}

internal static bool sameFile(ж<fileStat> Ꮡfs1, ж<fileStat> Ꮡfs2) {
    ref var fs1 = ref Ꮡfs1.DerefOrNull();
    ref var fs2 = ref Ꮡfs2.DerefOrNull();

    var e = Ꮡfs1.loadFileId();
    if (e != default!) {
        return false;
    }
    e = Ꮡfs2.loadFileId();
    if (e != default!) {
        return false;
    }
    return fs1.vol == fs2.vol && fs1.idxhi == fs2.idxhi && fs1.idxlo == fs2.idxlo;
}

// For testing.
internal static time.Time atime(FileInfo fi) {
    return time.Unix(0, fi.Sys()._<ж<syscall.Win32FileAttributeData>>().of(syscall.Win32FileAttributeData.ᏑLastAccessTime).Nanoseconds());
}

} // end os_package
