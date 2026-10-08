// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class syscall_package {

// A RawConn is a raw network connection.
partial interface RawConn {
    // Control invokes f on the underlying connection's file
    // descriptor or handle.
    // The file descriptor fd is guaranteed to remain valid while
    // f executes but not after f returns.
    error Control(Action<uintptr> f);
    // Read invokes f on the underlying connection's file
    // descriptor or handle; f is expected to try to read from the
    // file descriptor.
    // If f returns true, Read returns. Otherwise Read blocks
    // waiting for the connection to be ready for reading and
    // tries again repeatedly.
    // The file descriptor is guaranteed to remain valid while f
    // executes but not after f returns.
    error Read(Func<uintptr, bool> f);
    // Write is like Read but for writing.
    error Write(Func<uintptr, bool> f);
}

// Conn is implemented by some types in the net and os packages to provide
// access to the underlying file descriptor or handle.
partial interface Conn {
    // SyscallConn returns a raw network connection.
    (RawConn, error) SyscallConn();
}

} // end syscall_package
