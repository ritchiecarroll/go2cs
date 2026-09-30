// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using syscall = syscall_package;

partial class time_package {

// for testing: whatever interrupts a sleep
internal static void interrupt() {
}

internal static (uintptr, error) open(@string name) {
    var (fd, err) = syscall.Open(name, syscall.O_RDONLY, 0);
    if (err != default!) {
        // This condition solves issue https://go.dev/issue/50248
        if (AreEqual(err, syscall.ERROR_PATH_NOT_FOUND)) {
            err = syscall.ENOENT;
        }
        return (0, err);
    }
    return ((uintptr)fd, default!);
}

internal static (nint, error) read(uintptr fd, slice<byte> buf) {
    return syscall.Read(((syscallꓸHandle)fd), buf);
}

internal static void closefd(uintptr fd) {
    syscall.Close(((syscallꓸHandle)fd));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string shortReadˢ = "short read"u8;

internal static error preadn(uintptr fd, slice<byte> buf, nint off) {
    nint whence = seekStart;
    if (off < 0) {
        whence = seekEnd;
    }
    {
        var (_, err) = syscall.Seek(((syscallꓸHandle)fd), (int64)off, whence); if (err != default!) {
            return err;
        }
    }
    while (len(buf) > 0) {
        var (m, err) = syscall.Read(((syscallꓸHandle)fd), buf);
        if (m <= 0) {
            if (err == default!) {
                return errors.New(shortReadˢ);
            }
            return err;
        }
        buf = buf.slice(m);
    }
    return default!;
}

} // end time_package
