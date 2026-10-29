// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// A DirEntry is an entry read from a directory
// (using the [ReadDir] function or a [File.ReadDir] method).
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
        return (default, ErrInvalid);
    }
    var (_, _, infos, err) = Ꮡf.readdir(n, readdirFileInfo);
    if (infos == nil) {
        // Readdir has historically always returned a non-nil empty slice, never nil,
        // even on error (except misuse with nil receiver above).
        // Keep it that way to avoid breaking overly sensitive callers.
        infos = slice<FileInfo>([]);
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
    slice<@string> names = default;
    error err = default;

    if (Ꮡf == nil) {
        return (default, ErrInvalid);
    }
    (names, _, _, err) = Ꮡf.readdir(n, readdirName);
    if (names == nil) {
        // Readdirnames has historically always returned a non-nil empty slice, never nil,
        // even on error (except misuse with nil receiver above).
        // Keep it that way to avoid breaking overly sensitive callers.
        names = slice<@string>([]);
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
        return (default, ErrInvalid);
    }
    var (_, dirents, _, err) = Ꮡf.readdir(n, readdirDirEntry);
    if (dirents == nil) {
        // Match Readdir and Readdirnames: don't return nil slices.
        dirents = slice<DirEntry>([]);
    }
    return (dirents, err);
}

// ReadDir reads the named directory,
// returning all its directory entries sorted by filename.
// If an error occurs reading the directory,
// ReadDir returns the entries it was able to read before the error,
// along with the error.
public static (slice<DirEntry>, error) ReadDir(@string name) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = openDir(name);
        if (err != nil) {
            return (default, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        (var dirs, err) = f.ReadDir(-1);
        slices.SortFunc(dirs, (DirEntry a, DirEntry b) => bytealg.CompareString(a.Name(), b.Name()));
        return (dirs, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
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
            if (err != nil) {
                return err;
            }

            (var fpath, err) = filepathlite.Localize(path);
            if (err != nil) {
                return err;
            }
            ref var newPath = ref heap<@string>(joinPath(dir, fpath), out var ᏑnewPath);

            switch (d.Type()) {
            case var ᴛ when ᴛ == ModeDir: {
                return MkdirAll(newPath, 511);
            }
            case var ᴛ when ᴛ == ModeSymlink: {
                var (target, errΔ3) = fs.ReadLink(fsys, path);
                if (errΔ3 != nil) {
                    return errΔ3;
                }
                return Symlink(target, newPath);
            }
            case var ᴛ when ᴛ == (fs.FileMode)(0): {
                var (r, errΔ4) = fsys.Open(path);
                if (errΔ4 != nil) {
                    return errΔ4;
                }
                var rʗ1 = r;
                defer(() => rʗ1.Close(), ref ᒐ);
                (var info, errΔ4) = r.Stat();
                if (errΔ4 != nil) {
                    return errΔ4;
                }
                (var w, errΔ4) = OpenFile(newPath, (nint)((nint)(nint)(O_CREATE | O_EXCL) | O_WRONLY), (fs.FileMode)(438 | (fs.FileMode)(info.Mode() & 511)));
                if (errΔ4 != nil) {
                    return errΔ4;
                }

                {
                    var (_, errΔ5) = Δio.Copy(new FileжWriter(w), new fs_FileᴠReader(r)); if (errΔ5 != nil) {
                        w.Close();
                        return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "Copy"u8, Path: newPath, Err: errΔ5)));
                    }
                }
                return w.Close();
            }
            default: {
                return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "CopyFS"u8, Path: path, Err: ErrInvalid)));
            }}
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
        finally { ᒐ.Run(); }
    });
}

} // end os_package
