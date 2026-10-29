// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd
namespace go.@internal;

using Δsyscall = syscall_package;

partial class poll_package {

internal static Δsyscall.Iovec newIovecWithBase(ж<byte> @base) {
    return new Δsyscall.Iovec(Base: @base);
}

} // end poll_package
