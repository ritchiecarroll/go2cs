// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

partial class unix_package {

/*linkname*/ public static partial (uintptr r1, uintptr r2) SyscallNoError(uintptr trap, uintptr a1, uintptr a2, uintptr a3) {
    var (ᴛ1, ᴛ2, ᴛ3) = go.syscall_package.Syscall((uintptr)trap, (uintptr)a1, (uintptr)a2, (uintptr)a3);
    return ((uintptr)(uintptr)ᴛ1, (uintptr)(uintptr)ᴛ2);
}

/*linkname*/ public static partial (uintptr r1, uintptr r2) RawSyscallNoError(uintptr trap, uintptr a1, uintptr a2, uintptr a3) {
    var (ᴛ1, ᴛ2, ᴛ3) = go.syscall_package.RawSyscall((uintptr)trap, (uintptr)a1, (uintptr)a2, (uintptr)a3);
    return ((uintptr)(uintptr)ᴛ1, (uintptr)(uintptr)ᴛ2);
}

} // end unix_package
