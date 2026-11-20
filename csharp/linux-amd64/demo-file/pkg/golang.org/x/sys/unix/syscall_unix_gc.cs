// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using syscall = syscall_package;

partial class unix_package {

/*linkname*/ public static partial (uintptr r1, uintptr r2, syscall.Errno err) Syscall(uintptr trap, uintptr a1, uintptr a2, uintptr a3) {
    var (ᴛ1, ᴛ2, ᴛ3) = syscall.Syscall((uintptr)trap, (uintptr)a1, (uintptr)a2, (uintptr)a3);
    return ((uintptr)(uintptr)ᴛ1, (uintptr)(uintptr)ᴛ2, (syscall.Errno)(uintptr)ᴛ3);
}
/*linkname*/ public static partial (uintptr r1, uintptr r2, syscall.Errno err) Syscall6(uintptr trap, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    var (ᴛ1, ᴛ2, ᴛ3) = syscall.Syscall6((uintptr)trap, (uintptr)a1, (uintptr)a2, (uintptr)a3, (uintptr)a4, (uintptr)a5, (uintptr)a6);
    return ((uintptr)(uintptr)ᴛ1, (uintptr)(uintptr)ᴛ2, (syscall.Errno)(uintptr)ᴛ3);
}
/*linkname*/ public static partial (uintptr r1, uintptr r2, syscall.Errno err) RawSyscall(uintptr trap, uintptr a1, uintptr a2, uintptr a3) {
    var (ᴛ1, ᴛ2, ᴛ3) = syscall.RawSyscall((uintptr)trap, (uintptr)a1, (uintptr)a2, (uintptr)a3);
    return ((uintptr)(uintptr)ᴛ1, (uintptr)(uintptr)ᴛ2, (syscall.Errno)(uintptr)ᴛ3);
}
/*linkname*/ public static partial (uintptr r1, uintptr r2, syscall.Errno err) RawSyscall6(uintptr trap, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    var (ᴛ1, ᴛ2, ᴛ3) = syscall.RawSyscall6((uintptr)trap, (uintptr)a1, (uintptr)a2, (uintptr)a3, (uintptr)a4, (uintptr)a5, (uintptr)a6);
    return ((uintptr)(uintptr)ᴛ1, (uintptr)(uintptr)ᴛ2, (syscall.Errno)(uintptr)ᴛ3);
}

} // end unix_package
