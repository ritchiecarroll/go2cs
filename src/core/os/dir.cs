// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using DirEntry = go.io.fs_package.DirEntry;

namespace go;

using bytealg = @internal.bytealg_package;
using filepathlite = @internal.filepathlite_package;
using Δio = io_package;
using fs = go.io.fs_package;
using slices = slices_package;
using @internal;
using go.io;

partial class os_package {

partial struct readdirMode /*num:nint*/;

internal static readdirMode readdirName => /* iota */ 0;
internal static readdirMode readdirDirEntry => 1;
internal static readdirMode readdirFileInfo => 2;

// Readdir reads the contents of the directory associated with file and
// returns a slice of up to n [FileInfo] values, as would be returned
// by [Lstat], in directory order. Subsequent calls on the same file will yield
// further FileInfos.
//
// If n > 0, Readdir returns at most n FileInfo structures. In this case, if
// Readdir returns an empty slice, it will return a non-nil error
// explaining why. At the end of a directory, the error is [io.EOF].
//
// If n <= 0, Readdir returns all the FileInfo from the directory in
// a single slice. In this case, if Readdir succeeds (reads all
// the way to the end of the directory), it returns the slice and a
// nil error. If it encounters an error before the end of the
// directory, Readdir returns the FileInfo read until that point
// and a non-nil error.
//
// Most clients are better served by the more efficient ReadDir method.
public static (slice<FileInfo>, error) Readdir(this ж<File> Ꮡf, nint n) {
    if (Ꮡf == nil) {
        return (default!, ErrInvalid);
    }
    var (_, _, infos, err) = Ꮡf.readdir(n, readdirFileInfo);
    if (infos == default!) {
        // Readdir has historically always returned a non-nil empty slice, never nil,
        // even on error (except misuse with nil receiver above).
        // Keep it that way to avoid breaking overly sensitive callers.
        infos = new FileInfo[]{}.slice();
    }
    return (infos, err);
}

// Readdirnames reads the contents of the directory associated with file
// and returns a slice of up to n names of files in the directory,
// in directory order. Subsequent calls on the same file will yield
// further names.
//
// If n > 0, Readdirnames returns at most n names. In this case, if
// Readdirnames returns an empty slice, it will return a non-nil error
// explaining why. At the end of a directory, the error is [io.EOF].
//
// If n <= 0, Readdirnames returns all the names from the directory in
// a single slice. In this case, if Readdirnames succeeds (reads all
// the way to the end of the directory), it returns the slice and a
// nil error. If it encounters an error before the end of the
// directory, Readdirnames returns the names read until that point and
// a non-nil error.
public static (slice<@string> names, error err) Readdirnames(this ж<File> Ꮡf, nint n) {
    slice<@string> names = default!;
    error err = default!;

    if (Ꮡf == nil) {
        return (default!, ErrInvalid);
    }
    (names, _, _, err) = Ꮡf.readdir(n, readdirName);
    if (names == default!) {
        // Readdirnames has historically always returned a non-nil empty slice, never nil,
        // even on error (except misuse with nil receiver above).
        // Keep it that way to avoid breaking overly sensitive callers.
        names = new @string[]{}.slice();
    }
    return (names, err);
}

// ReadDir reads the contents of the directory associated with the file f
// and returns a slice of [DirEntry] values in directory order.
// Subsequent calls on the same file will yield later DirEntry records in the directory.
//
// If n > 0, ReadDir returns at most n DirEntry records.
// In this case, if ReadDir returns an empty slice, it will return an error explaining why.
// At the end of a directory, the error is [io.EOF].
//
// If n <= 0, ReadDir returns all the DirEntry records remaining in the directory.
// When it succeeds, it returns a nil error (not io.EOF).
public static (slice<DirEntry>, error) ReadDir(this ж<File> Ꮡf, nint n) {
    if (Ꮡf == nil) {
        return (default!, ErrInvalid);
    }
    var (_, dirents, _, err) = Ꮡf.readdir(n, readdirDirEntry);
    if (dirents == default!) {
        // Match Readdir and Readdirnames: don't return nil slices.
        dirents = new DirEntry[]{}.slice();
    }
    return (dirents, err);
}

// testingForceReadDirLstat forces ReadDir to call Lstat, for testing that code path.
// This can be difficult to provoke on some Unix systems otherwise.
internal static ж<bool> ᏑtestingForceReadDirLstat = new StandardBox<bool>(default(bool));
internal static ref bool testingForceReadDirLstat => ref ᏑtestingForceReadDirLstat.Value;

// ReadDir reads the named directory,
// returning all its directory entries sorted by filename.
// If an error occurs reading the directory,
// ReadDir returns the entries it was able to read before the error,
// along with the error.
public static (slice<DirEntry>, error) ReadDir(@string name) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = openDir(name);
        if (err != default!) {
            return (default!, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var dirs, err) = f.ReadDir(-1);
        slices.SortFunc(dirs, (DirEntry a, DirEntry b) => bytealg.CompareString(a.Name(), b.Name()));
        return (dirs, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// CopyFS copies the file system fsys into the directory dir,
// creating dir if necessary.
//
// Files are created with mode 0o666 plus any execute permissions
// from the source, and directories are created with mode 0o777
// (before umask).
//
// CopyFS will not overwrite existing files. If a file name in fsys
// already exists in the destination, CopyFS will return an error
// such that errors.Is(err, fs.ErrExist) will be true.
//
// Symbolic links in fsys are not supported. A *PathError with Err set
// to ErrInvalid is returned when copying from a symbolic link.
//
// Symbolic links in dir are followed.
//
// New files added to fsys (including if dir is a subdirectory of fsys)
// while CopyFS is running are not guaranteed to be copied.
//
// Copying stops at and returns the first error encountered.
public static error CopyFS(@string dir, fs.FS fsys) {
    return fs.WalkDir(fsys, "."u8, error (@string path, fs.DirEntry d, error err) => {
        GoFrame ᒐ = default;
        try {
            if (err != default!) {
                return err;
            }
            (var fpath, err) = filepathlite.Localize(path);
            if (err != default!) {
                return err;
            }
            ref var newPath = ref heap<@string>(out var ᏑnewPath);
            newPath = joinPath(dir, fpath);
            if (d.IsDir()) {
                return MkdirAll(newPath, 511);
            }
            // TODO(panjf2000): handle symlinks with the help of fs.ReadLinkFS
            // 		once https://go.dev/issue/49580 is done.
            //		we also need filepathlite.IsLocal from https://go.dev/cl/564295.
            if (!d.Type().IsRegular()) {
                return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "CopyFS"u8, Path: path, Err: ErrInvalid)));
            }
            (var r, err) = fsys.Open(path);
            if (err != default!) {
                return err;
            }
            var rʗ1 = r;
            defer(() => rʗ1.Close(), ref ᒐ);
            (var info, err) = r.Stat();
            if (err != default!) {
                return err;
            }
            (var w, err) = OpenFile(newPath, (nint)((nint)(nint)(O_CREATE | O_EXCL) | O_WRONLY), (fs.FileMode)(438 | (fs.FileMode)(info.Mode() & 511)));
            if (err != default!) {
                return err;
            }
            {
                var (_, errΔ1) = Δio.Copy(new FileжWriter(w), new fs_FileᴠReader(r)); if (errΔ1 != default!) {
                    w.Close();
                    return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "Copy"u8, Path: newPath, Err: errΔ1)));
                }
            }
            return w.Close();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    });
}

} // end os_package
