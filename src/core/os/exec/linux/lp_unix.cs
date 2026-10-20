// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
namespace go.os;

using errors = errors_package;
using unix = @internal.syscall.unix_package;
using fs = go.io.fs_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using syscall = syscall_package;
using @internal;
using @internal.syscall;
using go.io;
using path;

partial class exec_package {

// ErrNotFound is the error resulting if a path search failed to find an executable file.
public static error ErrNotFound = errors.New("executable file not found in $PATH"u8);

internal static error findExecutable(@string @file) {
    var (d, err) = os.Stat(@file);
    if (err != default!) {
        return err;
    }
    var m = d.Mode();
    if (m.IsDir()) {
        return syscall.EISDIR;
    }
    err = unix.Eaccess(@file, unix.X_OK);
    // ENOSYS means Eaccess is not available or not implemented.
    // EPERM can be returned by Linux containers employing seccomp.
    // In both cases, fall back to checking the permission bits.
    if (err == default! || (!AreEqual(err, syscall.ENOSYS) && !AreEqual(err, syscall.EPERM))) {
        return err;
    }
    if ((fs.FileMode)(m & 73) != 0) {
        return default!;
    }
    return fs.ErrPermission;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pathˢ = "PATH"u8;

internal static (@string, error) lookPath(@string @file) {
    // NOTE(rsc): I wish we could use the Plan 9 behavior here
    // (only bypass the path if file begins with / or ./ or ../)
    // but that would not match all the Unix shells.

    {
        var err = validateLookPath(@file); if (err != default!) {
            return ("", new ΔErrorжerror(Ꮡ(new ΔError(@file, err))));
        }
    }

    if (strings.Contains(@file, "/"u8)) {
        var err = findExecutable(@file);
        if (err == default!) {
            return (@file, default!);
        }
        return ("", new ΔErrorжerror(Ꮡ(new ΔError(@file, err))));
    }
    @string path = os.Getenv(pathˢ);
    foreach (var (_, vᴛ1) in filepath.SplitList(path)) {
        var dir = vᴛ1;

        if (dir == ""u8) {
            // Unix shell semantics: path element "" means "."
            dir = "."u8;
        }
        @string pathΔ1 = filepath.Join(dir, @file);
        {
            var err = findExecutable(pathΔ1); if (err == default!) {
                if (!filepath.IsAbs(pathΔ1)) {
                    if (execerrdot.Value() != "0"u8) {
                        return (pathΔ1, new ΔErrorжerror(Ꮡ(new ΔError(@file, ErrDot))));
                    }
                    execerrdot.IncNonDefault();
                }
                return (pathΔ1, default!);
            }
        }
    }
    return ("", new ΔErrorжerror(Ꮡ(new ΔError(@file, ErrNotFound))));
}

// lookExtensions is a no-op on non-Windows platforms, since
// they do not restrict executables to specific extensions.
internal static (@string, error) lookExtensions(@string path, @string dir) {
    return (path, default!);
}

} // end exec_package
