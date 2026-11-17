// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
using go;

// Hand-finished conversion (Phase 4 — os operational on Windows).
//
// Go's (*File).readdir walks the raw buffer GetFileInformationByHandleEx fills by REINTERPRETING it
// as a Go struct:
//
//	entry := unsafe.Pointer(&(*d.buf)[d.bufp])
//	info := (*windows.FILE_ID_BOTH_DIR_INFO)(entry)
//	nameslice = unsafe.Slice(&info.FileName[0], info.FileNameLength/2)
//
// FILE_ID_BOTH_DIR_INFO is a MANAGED-REFERENT struct in the conversion — its inline Go arrays
// (`ShortName [12]uint16`, the variable-length trailing `FileName [1]uint16`) become golib
// `array<uint16>` OBJECT REFERENCES, 8 bytes each where the OS wrote 24 and 2 bytes inline. So the
// managed layout does not describe the bytes at all: `&info.FileName[0]` addressed a zero-length
// array (System.IndexOutOfRangeException on the FIRST directory read — path/filepath.Glob,
// os.ReadDir, os.File.Readdirnames, and every test that walks a testdata directory), the fields
// after ShortName sit at the wrong offsets, and merely COPYING the reinterpreted struct hands the
// GC a fabricated object reference. No converter or golib change can rescue this: a managed array
// reference can never be laid out like an inline OS array. This is the "raw metal on non-native
// types" arm of the conversion fork (see src/archived/Baseline-vs-FullConversion.md).
//
// So readdir is hand-owned: it decodes the two directory-entry layouts straight out of the byte
// slice at their documented offsets and never materializes a managed struct over OS memory. Every
// read is an ordinary bounds-checked managed read of `(*d.buf)` — there is no pointer, no pinning
// and no unsafe block in this file. The converter skips the auto form via the manualConversionFuncs
// registry (go2cs/manualTypeOperations.go); the module marker below makes go2cs skip re-converting
// this file wholesale, and the overlay restores it over auto output on every reconversion.
//
// Field offsets are taken from the GO declarations in internal/syscall/windows (which the buffer
// bytes match for every field Go reads — Go widens Win32's `CCHAR ShortNameLength` to uint32, which
// shifts only ShortName, a field neither Go nor this file reads):
//
//	FILE_ID_BOTH_DIR_INFO                      FILE_FULL_DIR_INFO
//	  0  NextEntryOffset uint32                  0  NextEntryOffset uint32
//	  4  FileIndex       uint32                  4  FileIndex       uint32
//	  8  CreationTime    Filetime                8  CreationTime    Filetime
//	 16  LastAccessTime  Filetime               16  LastAccessTime  Filetime
//	 24  LastWriteTime   Filetime               24  LastWriteTime   Filetime
//	 32  ChangeTime      Filetime               32  ChangeTime      Filetime
//	 40  EndOfFile       uint64                 40  EndOfFile       uint64
//	 48  AllocationSize  uint64                 48  AllocationSize  uint64
//	 56  FileAttributes  uint32                 56  FileAttributes  uint32
//	 60  FileNameLength  uint32                 60  FileNameLength  uint32
//	 64  EaSize          uint32                 64  EaSize          uint32
//	 68  ShortNameLength uint32                 68  FileName        [1]uint16
//	 72  ShortName       [12]uint16
//	 96  FileID          uint64
//	104  FileName        [1]uint16

[module: GoManualConversion]

namespace go;

using windows = @internal.syscall.windows_package;
using Δio = io_package;
using fs = go.io.fs_package;
using Δruntime = runtime_package;
using syscall = syscall_package;
using @internal.syscall;
using go.io;
using go.sync;
using static os_package;

partial class os_package {

// Byte offsets of the directory-entry fields this file reads. See the layout table above.
internal const int dirEntryNextEntryOffsetOff = 0;
private const int dirEntryCreationTimeOff = 8;
private const int dirEntryLastAccessTimeOff = 16;
private const int dirEntryLastWriteTimeOff = 24;
private const int dirEntryEndOfFileOff = 40;
private const int dirEntryFileAttributesOff = 56;
internal const int dirEntryFileNameLengthOff = 60;
private const int dirEntryEaSizeOff = 64;
internal const int fileIDBothDirInfoFileIDOff = 96;
internal const int fileIDBothDirInfoFileNameOff = 104;
internal const int fileFullDirInfoFileNameOff = 68;

// readEntryUint32 / readEntryUint64 read one little-endian field of the directory entry starting at
// `entry` in the buffer. Both go through the bounds-checked slice indexer, so a short or truncated
// buffer surfaces as an ordinary index panic rather than reading past the OS data.
internal static uint32 readEntryUint32(slice<byte> buf, nint entry, int fieldOffset) {
    nint at = entry + fieldOffset;
    return (uint32)((uint32)buf[at] | ((uint32)buf[at + 1] << 8) | ((uint32)buf[at + 2] << 16) | ((uint32)buf[at + 3] << 24));
}

private static uint64 readEntryUint64(slice<byte> buf, nint entry, int fieldOffset) {
    return (uint64)readEntryUint32(buf, entry, fieldOffset) | ((uint64)readEntryUint32(buf, entry, fieldOffset + 4) << 32);
}

// readEntryFiletime reads a syscall.Filetime (two little-endian uint32s: low then high).
private static syscall.Filetime readEntryFiletime(slice<byte> buf, nint entry, int fieldOffset) {
    return new syscall.Filetime(
        LowDateTime: readEntryUint32(buf, entry, fieldOffset),
        HighDateTime: readEntryUint32(buf, entry, fieldOffset + 4));
}

// readEntryName reads the entry's trailing UTF-16 name — Go's
// `unsafe.Slice(&info.FileName[0], info.FileNameLength/2)` — as a managed uint16 slice copied out of
// the buffer. FileNameLength counts BYTES, so the rune count is half of it.
internal static slice<uint16> readEntryName(slice<byte> buf, nint entry, int nameOffset, uint32 nameLengthBytes) {
    nint runes = (nint)(nameLengthBytes / 2);
    slice<uint16> name = new slice<uint16>(runes);
    nint at = entry + nameOffset;
    for (nint i = 0; i < runes; i++) {
        name[i] = (uint16)((uint32)buf[at] | ((uint32)buf[at + 1] << 8));
        at += 2;
    }
    return name;
}

// newFileStatFromDirEntryBytes builds the fileStat for one directory entry, reading the shared
// leading fields both layouts have in common; `fileIDOffset` is the FILE_ID_BOTH_DIR_INFO FileID
// offset, or -1 for FILE_FULL_DIR_INFO, which carries no file ID. Mirrors
// newFileStatFromFileIDBothDirInfo / newFileStatFromFileFullDirInfo, whose auto forms remain but are
// unreachable now that this file owns the only caller (their parameter is the unusable reinterpret).
internal static ж<fileStat> newFileStatFromDirEntryBytes(slice<byte> buf, nint entry, int fileIDOffset) {
    uint64 endOfFile = readEntryUint64(buf, entry, dirEntryEndOfFileOff);
    // The FILE_ID_BOTH_DIR_INFO MSDN documentation isn't completely correct. FileAttributes can
    // contain any file attributes currently set on the file, not just the documented ones, and
    // EaSize contains the reparse tag if the file is a reparse point.
    ж<fileStat> f = Ꮡ(new fileStat(
        FileAttributes: readEntryUint32(buf, entry, dirEntryFileAttributesOff),
        CreationTime: readEntryFiletime(buf, entry, dirEntryCreationTimeOff),
        LastAccessTime: readEntryFiletime(buf, entry, dirEntryLastAccessTimeOff),
        LastWriteTime: readEntryFiletime(buf, entry, dirEntryLastWriteTimeOff),
        FileSizeHigh: (uint32)(endOfFile >> 32),
        FileSizeLow: (uint32)endOfFile,
        ReparseTag: readEntryUint32(buf, entry, dirEntryEaSizeOff)));

    if (fileIDOffset >= 0) {
        uint64 fileID = readEntryUint64(buf, entry, fileIDOffset);
        f.Value.idxhi = (uint32)(fileID >> 32);
        f.Value.idxlo = (uint32)fileID;
    }

    return f;
}

} partial class osᴛFile {

internal static (slice<@string> names, slice<DirEntry> dirents, slice<FileInfo> infos, error err) readdir(this ж<File> Ꮡfile, nint n, readdirMode mode) {
    slice<@string> names = default!;
    slice<DirEntry> dirents = default!;
    slice<FileInfo> infos = default!;
    error err = default!;

    ref var @file = ref Ꮡfile.Value;

    // If this file has no dirInfo, create one.
    ж<dirInfo> d = default!;
    while (true) {
        d = Ꮡfile.of(File.Ꮡdirinfo).Load();
        if (d != nil) {
            break;
        }
        d = @new<dirInfo>();
        d.init(@file.pfd.Sysfd);
        if (Ꮡfile.of(File.Ꮡdirinfo).CompareAndSwap(nil, d)) {
            break;
        }
        // We lost the race: try again.
        d.close();
    }

    d.of(dirInfo.Ꮡmu).Lock();
    try {
        if ((~d).buf == nil) {
            d.Value.buf = ᏑdirBufPool.Get()._<ж<slice<byte>>>();
        }
        var wantAll = n <= 0;
        if (wantAll) {
            n = -1;
        }
        while (n != 0) {
            // Refill the buffer if necessary.
            if ((~d).bufp == 0) {
                err = windows.GetFileInformationByHandleEx(@file.pfd.Sysfd, (~d).@class, Ꮡ(((~d).buf.ValueSlot), 0), (uint32)len((~d).buf.ValueSlot));
                Δruntime.KeepAlive(Ꮡfile);
                if (err != default!) {
                    if (AreEqual(err, syscall.ERROR_NO_MORE_FILES)) {
                        // Optimization: we can return the buffer to the pool, there is nothing
                        // else to read.
                        ᏑdirBufPool.Put((~d).buf);
                        d.Value.buf = default!;
                        break;
                    }
                    if (AreEqual(err, syscall.ERROR_FILE_NOT_FOUND) && ((~d).@class == windows.FileIdBothDirectoryRestartInfo || (~d).@class == windows.FileFullDirectoryRestartInfo)) {
                        // GetFileInformationByHandleEx doesn't document the return error codes
                        // when the info class is FileIdBothDirectoryRestartInfo, but MS-FSA
                        // 2.1.5.6.3 specifies that the underlying file system driver should
                        // return STATUS_NO_SUCH_FILE when reading an empty root directory, which
                        // Windows maps to ERROR_FILE_NOT_FOUND. See go.dev/issue/61159.
                        break;
                    }
                    {
                        var (s, _) = Ꮡfile.Stat();
                        if (s != default! && !s.IsDir()) {
                            err = new fs.PathErrorжerror(Ꮡ(new PathError(Op: "readdir"u8, Path: @file.name, Err: syscall.ENOTDIR)));
                        } else {
                            err = new fs.PathErrorжerror(Ꮡ(new PathError(Op: "GetFileInformationByHandleEx"u8, Path: @file.name, Err: err)));
                        }
                    }
                    return (names, dirents, infos, err);
                }
                if ((~d).@class == windows.FileIdBothDirectoryRestartInfo) {
                    d.Value.@class = windows.FileIdBothDirectoryInfo;
                } else if ((~d).@class == windows.FileFullDirectoryRestartInfo) {
                    d.Value.@class = windows.FileFullDirectoryInfo;
                }
            }
            // Drain the buffer.
            bool islast = false;
            while (n != 0 && !islast) {
                slice<byte> buf = (~d).buf.ValueSlot;
                nint entry = (~d).bufp;
                bool hasFileID = (~d).@class == windows.FileIdBothDirectoryInfo;
                int nameOffset = hasFileID ? fileIDBothDirInfoFileNameOff : fileFullDirInfoFileNameOff;

                uint32 nextEntryOffset = readEntryUint32(buf, entry, dirEntryNextEntryOffsetOff);
                slice<uint16> nameslice = readEntryName(buf, entry, nameOffset, readEntryUint32(buf, entry, dirEntryFileNameLengthOff));

                d.Value.bufp += (nint)nextEntryOffset;
                islast = nextEntryOffset == 0;
                if (islast) {
                    d.Value.bufp = 0;
                }
                if ((len(nameslice) == 1 && nameslice[0] == (rune)'.') || (len(nameslice) == 2 && nameslice[0] == (rune)'.' && nameslice[1] == (rune)'.')) {
                    // Ignore "." and ".." and avoid allocating a string for them.
                    continue;
                }
                @string name = syscall.UTF16ToString(nameslice);
                if (mode == readdirName) {
                    names = append(names, name);
                } else {
                    ж<fileStat> f = newFileStatFromDirEntryBytes(buf, entry, hasFileID ? fileIDBothDirInfoFileIDOff : -1);
                    if (!hasFileID && (~d).path != ""u8) {
                        // Defer appending the entry name to the parent directory path until it is
                        // really needed, to avoid allocating a string that may not be used. It is
                        // currently only used in os.SameFile.
                        f.Value.appendNameToPath = true;
                        f.Value.path = d.Value.path;
                    }
                    f.Value.name = name;
                    f.Value.vol = d.Value.vol;
                    if (mode == readdirDirEntry) {
                        dirents = append(dirents, (DirEntry)(new dirEntry(f)));
                    } else {
                        infos = append(infos, (FileInfo)(new fileStatжFileInfo(f)));
                    }
                }
                n--;
            }
        }
        if (!wantAll && len(names) + len(dirents) + len(infos) == 0) {
            return (default!, default!, default!, Δio.EOF);
        }
        return (names, dirents, infos, default!);
    }
    finally {
        d.of(dirInfo.Ꮡmu).Unlock();
    }
}

} partial class os_package {

} // end os_package
