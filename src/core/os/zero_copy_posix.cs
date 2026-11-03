// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || js || wasip1 || windows
namespace go;

using Δio = io_package;
using syscall = syscall_package;

partial class os_package {

// wrapSyscallError takes an error and a syscall name. If the error is
// a syscall.Errno, it wraps it in an os.SyscallError using the syscall name.
internal static error wrapSyscallError(@string name, error err) {
    {
        var (_, ok) = err._<syscall.Errno>(ᐧ); if (ok) {
            err = NewSyscallError(name, err);
        }
    }
    return err;
}

// tryLimitedReader tries to assert the io.Reader to io.LimitedReader, it returns the io.LimitedReader,
// the underlying io.Reader and the remaining amount of bytes if the assertion succeeds,
// otherwise it just returns the original io.Reader and the theoretical unlimited remaining amount of bytes.
internal static (ж<Δio.LimitedReader>, Δio.Reader, int64) tryLimitedReader(Δio.Reader r) {
    int64 remain = 9223372036854775807L; // by default, copy until EOF

    var (lr, ok) = r._<ж<Δio.LimitedReader>>(ᐧ);
    if (!ok) {
        return (default, r, remain);
    }

    remain = lr.N;
    return (lr, lr.R, remain);
}

} // end os_package
