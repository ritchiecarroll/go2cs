// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using syscall = syscall_package;

partial class unix_package {

/*linkname*/ internal static partial syscall.Errno /*err*/ gettimeofday(ж<Timeval> tv) {
    ж<syscall.Timeval> ᴛb1 = default;
    if (tv != nil) {
        ref var ᴛv1 = ref heap(new syscall.Timeval(), out ᴛb1);
        ᴛv1.Sec = tv.Value.Sec;
        ᴛv1.Usec = tv.Value.Usec;
    }
    var ᴛ1 = syscall.gettimeofday(ᴛb1);
    if (tv != nil) {
        tv.Value.Sec = ᴛb1.Value.Sec;
        tv.Value.Usec = ᴛb1.Value.Usec;
    }
    return (syscall.Errno)(uintptr)ᴛ1;
}

} // end unix_package
