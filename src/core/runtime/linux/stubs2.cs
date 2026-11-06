// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !aix && !darwin && !js && !openbsd && !plan9 && !solaris && !wasip1 && !windows
namespace go;

using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

// read calls the read system call.
// It returns a non-negative number of bytes written or a negative errno value.
internal static partial int32 read(int32 fd, @unsafe.Pointer Δp, int32 n);

internal static partial int32 closefd(int32 fd);

internal static partial void exit(int32 code);
internal static partial void usleep(uint32 usec);

//go:nosplit
internal static void usleep_no_g(uint32 usec) {
    usleep(usec);
}

// write1 calls the write system call.
// It returns a non-negative number of bytes written or a negative errno value.
//
//go:noescape
internal static partial int32 write1(uintptr fd, @unsafe.Pointer Δp, int32 n);

//go:noescape
internal static partial int32 open(ж<byte> name, int32 mode, int32 perm);

// return value is only set on linux to be used in osinit().
internal static partial int32 madvise(@unsafe.Pointer addr, uintptr n, int32 flags);

// exitThread terminates the current thread, writing *wait = freeMStack when
// the stack is safe to reclaim.
//
//go:noescape
internal static partial void exitThread(ж<atomic.Uint32> wait);

} // end runtime_package
