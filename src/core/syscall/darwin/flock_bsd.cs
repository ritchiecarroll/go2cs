// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || netbsd || openbsd
namespace go;

using @unsafe = unsafe_package;

partial class syscall_package {

// FcntlFlock performs a fcntl syscall for the [F_GETLK], [F_SETLK] or [F_SETLKW] command.
public static error FcntlFlock(uintptr fd, nint cmd, ж<Flock_t> lk) {
    var (_, err) = fcntlPtr((nint)fd, cmd, @unsafe.Pointer.FromPinnedBox(lk));
    return err;
}

} // end syscall_package
