// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !unix
namespace go;

using Δio = io_package;
using runtime = runtime_package;
using syscall = syscall_package;
using fs = go.io.fs_package;
using go.io;

partial class os_package {

internal static error removeAll(@string path) {
    if (path == ""u8) {
        // fail silently to retain compatibility with previous behavior
        // of RemoveAll. See issue 28830.
        return default!;
    }

    // The rmdir system call permits removing "." on Plan 9,
    // so we don't permit it to remain consistent with the
    // "at" implementation of RemoveAll.
    if (endsWithDot(path)) {
        return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "RemoveAll"u8, Path: path, Err: syscall.EINVAL)));
    }

    // Simple case: if Remove works, we're done.
    var err = Remove(path);
    if (err == default! || IsNotExist(err)) {
        return default!;
    }

    // Otherwise, is this a directory we need to recurse into?
    var (dir, serr) = Lstat(path);
    if (serr != default!) {
        {
            var (serrΔ1, ok) = serr._<ж<PathError>>(ᐧ); if (ok && (IsNotExist((~serrΔ1).Err) || AreEqual((~serrΔ1).Err, syscall.ENOTDIR))) {
                return default!;
            }
        }
        return serr;
    }
    if (!dir.IsDir()) {
        // Not a directory; return the error from Remove.
        return err;
    }

    // Remove contents & return first error.
    err = default!;
    while (ᐧ) {
        var (fd, errΔ1) = Open(path);
        if (errΔ1 != default!) {
            if (IsNotExist(errΔ1)) {
                // Already deleted by someone else.
                return default!;
            }
            return errΔ1;
        }

        const nint reqSize = 1024;
        slice<@string> names = default!;
        error readErr = default!;

        while (ᐧ) {
            nint numErr = 0;
            (names, readErr) = fd.Readdirnames(reqSize);

            foreach (var (_, name) in names) {
                var err1Δ1 = RemoveAll(path + ((@string)(rune)PathSeparator) + name);
                if (errΔ1 == default!) {
                    errΔ1 = err1Δ1;
                }
                if (err1Δ1 != default!) {
                    numErr++;
                }
            }

            // If we can delete any entry, break to start new iteration.
            // Otherwise, we discard current names, get next entries and try deleting them.
            if (numErr != reqSize) {
                break;
            }
        }

        // Removing files from the directory may have caused
        // the OS to reshuffle it. Simply calling Readdirnames
        // again may skip some entries. The only reliable way
        // to avoid this is to close and re-open the
        // directory. See issue 20841.
        fd.Close();

        if (AreEqual(readErr, Δio.EOF)) {
            break;
        }
        // If Readdirnames returned an error, use it.
        if (errΔ1 == default!) {
            errΔ1 = readErr;
        }
        if (len(names) == 0) {
            break;
        }

        // We don't want to re-open unnecessarily, so if we
        // got fewer than request names from Readdirnames, try
        // simply removing the directory now. If that
        // succeeds, we are done.
        if (len(names) < reqSize) {
            var err1Δ2 = Remove(path);
            if (err1Δ2 == default! || IsNotExist(err1Δ2)) {
                return default!;
            }

            if (errΔ1 != default!) {
                // We got some error removing the
                // directory contents, and since we
                // read fewer names than we requested
                // there probably aren't more files to
                // remove. Don't loop around to read
                // the directory again. We'll probably
                // just get the same error.
                return errΔ1;
            }
        }
    }

    // Remove directory.
    var err1 = Remove(path);
    if (err1 == default! || IsNotExist(err1)) {
        return default!;
    }
    if (runtime.GOOS == "windows"u8 && IsPermission(err1)) {
        {
            var (fs, errΔ2) = Stat(path); if (errΔ2 == default!) {
                {
                    errΔ2 = Chmod(path, ((FileMode)(uint32)((nint)(128 | (nint)(uint32)fs.Mode())))); if (errΔ2 == default!) {
                        err1 = Remove(path);
                    }
                }
            }
        }
    }
    if (err == default!) {
        err = err1;
    }
    return err;
}

} // end os_package
