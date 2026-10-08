// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.io;

using errors = errors_package;
using bytealg = @internal.bytealg_package;
using slices = slices_package;
using @internal;

partial class fs_package {

// ReadDirFS is the interface implemented by a file system
// that provides an optimized implementation of [ReadDir].
partial interface ReadDirFS :
    FS
{
    // ReadDir reads the named directory
    // and returns a list of directory entries sorted by filename.
    (slice<DirEntry>, error) ReadDir(@string name);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string notImplementedˢ = "not implemented"u8;

// ReadDir reads the named directory
// and returns a list of directory entries sorted by filename.
//
// If fs implements [ReadDirFS], ReadDir calls fs.ReadDir.
// Otherwise ReadDir calls fs.Open and uses ReadDir and Close
// on the returned file.
public static (slice<DirEntry>, error) ReadDir(FS fsys, @string name) {
    GoFrame ᒐ = default;
    try {
        {
            var (fsysΔ1, okΔ1) = fsys._<ReadDirFS>(ᐧ); if (okΔ1) {
                return fsysΔ1.ReadDir(name);
            }
        }
        var (@file, err) = fsys.Open(name);
        if (err != default!) {
            return (default!, err);
        }
        var fileʗ1 = @file;
        defer(() => fileʗ1.Close(), ref ᒐ);
        var (dir, ok) = @file._<ReadDirFile>(ᐧ);
        if (!ok) {
            return (default!, new PathErrorжerror(Ꮡ(new PathError(Op: "readdir"u8, Path: name, Err: errors.New(notImplementedˢ)))));
        }
        (var list, err) = dir.ReadDir(-1);
        slices.SortFunc(list, (DirEntry a, DirEntry b) => bytealg.CompareString(a.Name(), b.Name()));
        return (list, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// dirInfo is a DirEntry based on a FileInfo.
partial struct dirInfo {
    internal FileInfo fileInfo;
}

internal static bool IsDir(this dirInfo di) {
    return di.fileInfo.IsDir();
}

internal static FileMode Type(this dirInfo di) {
    return di.fileInfo.Mode().Type();
}

internal static (FileInfo, error) Info(this dirInfo di) {
    return (di.fileInfo, default!);
}

internal static @string Name(this dirInfo di) {
    return di.fileInfo.Name();
}

internal static @string String(this dirInfo di) {
    return FormatDirEntry(di);
}

// FileInfoToDirEntry returns a [DirEntry] that returns information from info.
// If info is nil, FileInfoToDirEntry returns nil.
public static DirEntry FileInfoToDirEntry(FileInfo info) {
    if (info == default!) {
        return default!;
    }
    return new dirInfo(fileInfo: info);
}

} // end fs_package
