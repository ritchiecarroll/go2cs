// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Support for memory sanitizer. See runtime/cgo/mmap.go.
//go:build (linux && (amd64 || arm64 || loong64)) || (freebsd && amd64)
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

// _cgo_mmap is filled in by runtime/cgo when it is linked into the
// program, so it is only non-nil when using cgo.
//
//go:linkname _cgo_mmap _cgo_mmap
internal static @unsafe.Pointer _cgo_mmap;

// _cgo_munmap is filled in by runtime/cgo when it is linked into the
// program, so it is only non-nil when using cgo.
//
//go:linkname _cgo_munmap _cgo_munmap
internal static @unsafe.Pointer _cgo_munmap;

// mmap is used to route the mmap system call through C code when using cgo, to
// support sanitizer interceptors. Don't allow stack splits, since this function
// (used by sysAlloc) is called in a lot of low-level parts of the runtime and
// callers often assume it won't acquire any locks.
//
//go:nosplit
internal static (@unsafe.Pointer, nint) mmap(@unsafe.Pointer addr, uintptr n, int32 prot, int32 flags, int32 fd, uint32 off) {
    if (_cgo_mmap != nil) {
        // Make ret a uintptr so that writing to it in the
        // function literal does not trigger a write barrier.
        // A write barrier here could break because of the way
        // that mmap uses the same value both as a pointer and
        // an errno value.
        uintptr ret = default;
        systemstack(() => {
            ret = callCgoMmap(addr, n, prot, flags, fd, off);
        });
        if (ret < 4096) {
            return (default, (nint)ret);
        }
        return ((@unsafe.Pointer)ret, 0);
    }
    return sysMmap(addr, n, prot, flags, fd, off);
}

internal static void munmap(@unsafe.Pointer addr, uintptr n) {
    if (_cgo_munmap != nil) {
        systemstack(() => {
            callCgoMunmap(addr, n);
        });
        return;
    }
    sysMunmap(addr, n);
}

// sysMmap calls the mmap system call. It is implemented in assembly.
internal static partial (@unsafe.Pointer Δp, nint err) sysMmap(@unsafe.Pointer addr, uintptr n, int32 prot, int32 flags, int32 fd, uint32 off);

// callCgoMmap calls the mmap function in the runtime/cgo package
// using the GCC calling convention. It is implemented in assembly.
internal static partial uintptr callCgoMmap(@unsafe.Pointer addr, uintptr n, int32 prot, int32 flags, int32 fd, uint32 off);

// sysMunmap calls the munmap system call. It is implemented in assembly.
internal static partial void sysMunmap(@unsafe.Pointer addr, uintptr n);

// callCgoMunmap calls the munmap function in the runtime/cgo package
// using the GCC calling convention. It is implemented in assembly.
internal static partial void callCgoMunmap(@unsafe.Pointer addr, uintptr n);

} // end runtime_package
