// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || wasip1 || windows
namespace go;

using Δio = io_package;
using runtime = runtime_package;
using syscall = syscall_package;
using fs = go.io.fs_package;

partial class os_package {

internal static error removeAll(@string path) {
    GoFrame ᒐ = default;
    try {
        if (path == ""u8) {
            // fail silently to retain compatibility with previous behavior
            // of RemoveAll. See issue 28830.
            return default!;
        }

        // The rmdir system call does not permit removing ".",
        // so we don't permit it either.
        if (endsWithDot(path)) {
            return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "RemoveAll"u8, Path: path, Err: syscall.EINVAL)));
        }

        // Simple case: if Remove works, we're done.
        var err = Remove(path);
        if (err == default! || IsNotExist(err)) {
            return default!;
        }

        // RemoveAll recurses by deleting the path base from
        // its parent directory
        var (parentDir, @base) = splitPath(path);

        nint flag = O_RDONLY;
        if (runtime.GOOS == "windows"u8) {
            // On Windows, the process might not have read permission on the parent directory,
            // but still can delete files in it. See https://go.dev/issue/74134.
            // We can open a file even if we don't have read permission by passing the
            // O_WRONLY | O_RDWR flag, which is mapped to FILE_READ_ATTRIBUTES.
            flag = (nint)(O_WRONLY | O_RDWR);
        }
        (var parent, err) = OpenFile(parentDir, flag, 0);
        if (IsNotExist(err)) {
            // If parent does not exist, base cannot exist. Fail silently
            return default!;
        }
        if (err != default!) {
            return err;
        }
        var parentʗ1 = parent;
        defer(() => parentʗ1.Close(), ref ᒐ);

        {
            var errΔ1 = removeAllFrom((syscallꓸHandle)parent.Fd(), @base); if (errΔ1 != default!) {
                {
                    var (pathErr, ok) = errΔ1._<ж<PathError>>(ᐧ); if (ok) {
                        pathErr.Value.Path = parentDir + ((@string)(rune)PathSeparator) + (~pathErr).Path;
                        errΔ1 = new fs.PathErrorжerror(pathErr);
                    }
                }
                return errΔ1;
            }
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static error removeAllFrom(syscallꓸHandle parentFd, @string @base) {
    // Simple case: if Unlink (aka remove) works, we're done.
    var err = removefileat(parentFd, @base);
    if (err == default! || IsNotExist(err)) {
        return default!;
    }

    // EISDIR means that we have a directory, and we need to
    // remove its contents.
    // EPERM or EACCES means that we don't have write permission on
    // the parent directory, but this entry might still be a directory
    // whose contents need to be removed.
    // Otherwise just return the error.
    if (!AreEqual(err, syscall.EISDIR) && !AreEqual(err, syscall.EPERM) && !AreEqual(err, syscall.EACCES)) {
        return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "unlinkat"u8, Path: @base, Err: err)));
    }
    var uErr = err;

    // Remove the directory's entries.
    error recurseErr = default!;
    while (ᐧ) {
        const nint reqSize = 1024;
        nint respSize = default!;

        // Open the directory to recurse into.
        var (@file, errΔ1) = openDirAt(parentFd, @base);
        if (errΔ1 != default!) {
            if (IsNotExist(errΔ1)) {
                return default!;
            }
            if (AreEqual(errΔ1, syscall.ENOTDIR)) {
                // Not a directory; return the error from the removefileat.
                return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "unlinkat"u8, Path: @base, Err: uErr)));
            }
            {
                var (_, ok) = errΔ1._<errSymlink>(ᐧ); if (ok) {
                    // Not a user-visible error.
                    errΔ1 = uErr;
                }
            }
            recurseErr = new fs.PathErrorжerror(Ꮡ(new PathError(Op: "openfdat"u8, Path: @base, Err: errΔ1)));
            break;
        }

        while (ᐧ) {
            nint numErr = 0;

            var (names, readErr) = @file.Readdirnames(reqSize);
            // Errors other than EOF should stop us from continuing.
            if (readErr != default! && !AreEqual(readErr, Δio.EOF)) {
                @file.Close();
                if (IsNotExist(readErr)) {
                    return default!;
                }
                return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "readdirnames"u8, Path: @base, Err: readErr)));
            }

            respSize = len(names);
            foreach (var (_, name) in names) {
                var errΔ2 = removeAllFrom((syscallꓸHandle)@file.Fd(), name);
                if (errΔ2 != default!) {
                    {
                        var (pathErr, ok) = errΔ2._<ж<PathError>>(ᐧ); if (ok) {
                            pathErr.Value.Path = @base + ((@string)(rune)PathSeparator) + (~pathErr).Path;
                        }
                    }
                    numErr++;
                    if (recurseErr == default!) {
                        recurseErr = errΔ2;
                    }
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
        @file.Close();

        // Finish when the end of the directory is reached
        if (respSize < reqSize) {
            break;
        }
    }

    // Remove the directory itself.
    var unlinkError = removedirat(parentFd, @base);
    if (unlinkError == default! || IsNotExist(unlinkError)) {
        return default!;
    }

    if (recurseErr != default!) {
        return recurseErr;
    }
    return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "unlinkat"u8, Path: @base, Err: unlinkError)));
}

// openDirAt opens a directory name relative to the directory referred to by
// the file descriptor dirfd. If name is anything but a directory (this
// includes a symlink to one), it should return an error. Other than that this
// should act like openFileNolog.
//
// This acts like openFileNolog rather than OpenFile because
// we are going to (try to) remove the file.
// The contents of this file are not relevant for test caching.
internal static (ж<File>, error) openDirAt(syscallꓸHandle dirfd, @string name) {
    var (fd, err) = rootOpenDir(dirfd, name);
    if (err != default!) {
        return (default!, err);
    }
    return newDirFile(fd, name);
}

} // end os_package
