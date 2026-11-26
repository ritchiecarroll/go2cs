// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// This file is derived from Go, whose notice it carries above (license of record: LICENSES/BSD-3-Clause.txt):
//
// PROVENANCE:
// - writev: DERIVED FROM src/internal/poll/fd_writev_unix.go, Go 1.26.11, func writev. Go's retry loop, with the
//   iovec array marshalled by golib in place of the raw syscall.
//go:build dragonfly || freebsd || linux || netbsd || (openbsd && mips64)

// go2cs HAND-OWNED (whole-file replacement of the converted fd_writev_unix.go output) -- the
// STRUCT-PASSING seam for writev's iovec ARRAY, on the LINUX flavor.
//
// The converted body handed the kernel the address of a managed `slice<Iovec>`:
//
//     var ᴋ0 = Ꮡ(iovecs, 0);
//     (r, _, e) = syscall.Syscall(SYS_WRITEV, fd, (uintptr)ᴋ0, len(iovecs));
//
// `Iovec.Base` is a `ж<byte>` -- an OBJECT REFERENCE -- so the struct is non-blittable, the CLR
// gives it AUTO layout, and the kernel reads 16 bytes per element that are neither `{void*;
// size_t}` nor in that field order. It is the same class as Timezoneinformation (windows) and
// Msghdr (this flavor, sockaddr_linux_impl.cs), and its signature is diagnostic: the RIGHT COUNT
// of iovecs with GARBAGE contents. G measured it on the net row as ten 0x38s arriving where
// 0x00..0x09 were sent as ten one-byte iovecs -- wrong bytes, never a short write.
//
// The layout now lives behind syscall's GoWritevNative, which builds the blittable NativeIovec
// image already declared for the msghdr family and PINS each base rather than marshalling it (see
// that helper's comment for why `(uintptr)` on the box is the pin as well as the address). Only
// the layout moved: Go's EINTR retry loop stays here, where Go has it.
//
// Marked so a -stdlib reconvert cannot emit the Go version over it.
[module: go.GoManualConversion]

namespace go.@internal;

using Δsyscall = syscall_package;

partial class poll_package {

internal static (uintptr, error) writev(nint fd, slice<Δsyscall.Iovec> iovecs) {
    uintptr r = default!;
    Δsyscall.Errno e = default!;
    while (ᐧ) {
        (r, e) = Δsyscall.GoWritevNative(fd, iovecs);
        if (e != Δsyscall.EINTR) {
            break;
        }
    }
    if (e != 0) {
        return (r, e);
    }
    return (r, default!);
}

} // end poll_package
