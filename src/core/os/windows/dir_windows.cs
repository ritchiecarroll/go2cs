// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using windows = @internal.syscall.windows_package;
using Δio = io_package;
using fs = go.io.fs_package;
using runtime = runtime_package;
using Δsync = sync_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using @internal.syscall;
using go.io;

partial class os_package {

// Auxiliary information if the File describes a directory
partial struct dirInfo {
    internal Δsync.Mutex mu;
    // buf is a slice pointer so the slice header
    // does not escape to the heap when returning
    // buf to dirBufPool.
    internal ж<slice<byte>> buf; // buffer for directory I/O
    internal nint bufp;    // location of next record in buf
    internal syscallꓸHandle h;
    internal uint32 vol;
    internal uint32 @class; // type of entries in buf
    internal @string path; // absolute directory path, empty if the file system supports FILE_ID_BOTH_DIR_INFO
}

internal static UntypedInt dirBufSize => /* 64 * 1024 */ 65536; // 64kB

// The buffer must be at least a block long.
internal static ж<Δsync.Pool> ᏑdirBufPool = new StandardBox<Δsync.Pool>(new Δsync.Pool(
    New: () => {
        var buf = new slice<byte>(dirBufSize);
        return Ꮡ(buf);
    }
));
internal static ref Δsync.Pool dirBufPool => ref ᏑdirBufPool.Value;

internal static void close(this ref dirInfo d) {
    d.h = 0;
    if (d.buf != nil) {
        ᏑdirBufPool.Put(d.buf.OrTypedNil());
        d.buf = default!;
    }
}

// allowReadDirFileID indicates whether File.readdir should try to use FILE_ID_BOTH_DIR_INFO
// if the underlying file system supports it.
// Useful for testing purposes.
internal static ж<bool> ᏑallowReadDirFileID = new StandardBox<bool>(true);
internal static ref bool allowReadDirFileID => ref ᏑallowReadDirFileID.Value;

internal static void init(this ж<dirInfo> Ꮡd, syscallꓸHandle h) {
    ref var d = ref Ꮡd.DerefOrNull();

    d.h = h;
    d.@class = windows.FileFullDirectoryRestartInfo;
    // The previous settings are enough to read the directory entries.
    // The following code is only needed to support os.SameFile.
    // It is safe to query d.vol once and reuse the value.
    // Hard links are not allowed to reference files in other volumes.
    // Junctions and symbolic links can reference files and directories in other volumes,
    // but the reparse point should still live in the parent volume.
    ref var flags = ref heap(new uint32(), out var Ꮡflags);
    var err = windows.GetVolumeInformationByHandle(h, nil, 0, Ꮡd.of(dirInfo.Ꮡvol), nil, Ꮡflags, nil, 0);
    if (err != default!) {
        d.vol = 0; // Set to zero in case Windows writes garbage to it.
        // If we can't get the volume information, we can't use os.SameFile,
        // but we can still read the directory entries.
        return;
    }
    if ((uint32)(flags & (uint32)windows.FILE_SUPPORTS_OBJECT_IDS) == 0) {
        // The file system does not support object IDs, no need to continue.
        return;
    }
    if (allowReadDirFileID && (uint32)(flags & (uint32)windows.FILE_SUPPORTS_OPEN_BY_FILE_ID) != 0){
        // Use FileIdBothDirectoryRestartInfo if available as it returns the file ID
        // without the need to open the file.
        d.@class = windows.FileIdBothDirectoryRestartInfo;
    } else {
        // If FileIdBothDirectoryRestartInfo is not available but objects IDs are supported,
        // get the directory path so that os.SameFile can use it to open the file
        // and retrieve the file ID.
        (d.path, _) = windows.FinalPath(h, windows.FILE_NAME_OPENED);
    }
}

// go2cs generated this placeholder — func readdir is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

partial struct dirEntry {
    internal ж<fileStat> fs;
}

internal static @string Name(this dirEntry de) {
    return de.fs.Name();
}

internal static bool IsDir(this dirEntry de) {
    return de.fs.IsDir();
}

internal static FileMode Type(this dirEntry de) {
    return de.fs.Mode().Type();
}

internal static (FileInfo, error) Info(this dirEntry de) {
    return (new fileStatжFileInfo(de.fs), default!);
}

internal static @string String(this dirEntry de) {
    return fs.FormatDirEntry(de);
}

} // end os_package
