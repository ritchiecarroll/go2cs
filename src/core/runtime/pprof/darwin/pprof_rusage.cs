// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
namespace go.runtime;

using fmt = fmt_package;
using io = io_package;
using runtime = runtime_package;
using syscall = syscall_package;

partial class pprof_package {

// Adds MaxRSS to platforms that are supported.
internal static void addMaxRSS(io.Writer w) {
    uintptr rssToBytes = default;
    var exprᴛ1 = runtime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "aix"u8 || exprᴛ1 == "android"u8 || exprᴛ1 == "dragonfly"u8 || exprᴛ1 == "freebsd"u8 || exprᴛ1 == "linux"u8 || exprᴛ1 == "netbsd"u8 || exprᴛ1 == "openbsd"u8: {
        rssToBytes = 1024;
        break;
    }
    case {} when exprᴛ1 == "darwin"u8 || exprᴛ1 == "ios"u8: {
        rssToBytes = 1;
        break;
    }
    case {} when exprᴛ1 == "illumos"u8 || exprᴛ1 == "solaris"u8: {
        rssToBytes = (uintptr)syscall.Getpagesize();
        break;
    }
    default: {
        throw panic("unsupported OS");
        break;
    }}

    ref var rusage = ref heap(new syscall.Rusage(), out var Ꮡrusage);
    var err = syscall.Getrusage(syscall.RUSAGE_SELF, Ꮡrusage);
    if (err == nil) {
        fmt.Fprintf(w, "# MaxRSS = %d\n"u8, (uintptr)rusage.Maxrss * rssToBytes);
    }
}

} // end pprof_package
