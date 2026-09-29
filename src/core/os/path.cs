// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using filepathlite = @internal.filepathlite_package;
using syscall = syscall_package;
using @internal;
using fs = go.io.fs_package;
using go.io;

partial class os_package {

// MkdirAll creates a directory named path,
// along with any necessary parents, and returns nil,
// or else returns an error.
// The permission bits perm (before umask) are used for all
// directories that MkdirAll creates.
// If path is already a directory, MkdirAll does nothing
// and returns nil.
public static error MkdirAll(@string path, FileMode perm) {
    // Fast path: if we can tell whether path is a directory or file, stop with success or error.
    var (dir, err) = Stat(path);
    if (err == default!) {
        if (dir.IsDir()) {
            return default!;
        }
        return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "mkdir"u8, Path: path, Err: syscall.ENOTDIR)));
    }
    // Slow path: make sure parent exists and then call Mkdir for path.
    // Extract the parent folder from path by first removing any trailing
    // path separator and then scanning backward until finding a path
    // separator or reaching the beginning of the string.
    nint i = len(path) - 1;
    while (i >= 0 && IsPathSeparator(path[i])) {
        i--;
    }
    while (i >= 0 && !IsPathSeparator(path[i])) {
        i--;
    }
    if (i < 0) {
        i = 0;
    }
    // If there is a parent directory, and it is not the volume name,
    // recurse to ensure parent directory exists.
    {
        @string parent = path.slice(0, i); if (len(parent) > len(filepathlite.VolumeName(path))) {
            err = MkdirAll(parent, perm);
            if (err != default!) {
                return err;
            }
        }
    }
    // Parent now exists; invoke Mkdir and use its result.
    err = Mkdir(path, perm);
    if (err != default!) {
        // Handle arguments like "foo/." by
        // double-checking that directory doesn't exist.
        var (dirΔ1, err1) = Lstat(path);
        if (err1 == default! && dirΔ1.IsDir()) {
            return default!;
        }
        return err;
    }
    return default!;
}

// RemoveAll removes path and any children it contains.
// It removes everything it can but returns the first error
// it encounters. If the path does not exist, RemoveAll
// returns nil (no error).
// If there is an error, it will be of type [*PathError].
public static error RemoveAll(@string path) {
    return removeAll(path);
}

// endsWithDot reports whether the final component of path is ".".
internal static bool endsWithDot(@string path) {
    if (path == "."u8) {
        return true;
    }
    if (len(path) >= 2 && path[len(path) - 1] == (rune)'.' && IsPathSeparator(path[len(path) - 2])) {
        return true;
    }
    return false;
}

} // end os_package
