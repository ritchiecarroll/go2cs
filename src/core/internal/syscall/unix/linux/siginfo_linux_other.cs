// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build linux && !(mips || mipsle || mips64 || mips64le)
namespace go.@internal.syscall;

partial class unix_package {

partial struct siErrnoCode {
    public int32 Errno;
    public int32 Code;
}

} // end unix_package
