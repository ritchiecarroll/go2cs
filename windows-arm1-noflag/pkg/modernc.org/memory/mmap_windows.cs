// Copyright 2017 The Memory Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using syscall = golang.org.x.sys.windows_package;
using os = os_package;
using golang.org.x.sys;
using windows = golang.org.x.sys.windows_package;

partial class memory_package {

internal static UntypedInt _MEM_COMMIT => 0x1000;
internal static UntypedInt _MEM_RESERVE => 0x2000;
internal static UntypedInt _MEM_DECOMMIT => 0x4000;
internal static UntypedInt _MEM_RELEASE => 0x8000;
internal static UntypedInt _PAGE_READWRITE => 0x0004;
internal static UntypedInt _PAGE_NOACCESS => 0x0001;

internal static UntypedInt pageSizeLog => 16;

internal const bool canCarve = false;

internal static UntypedInt mmapGranularity => /* pageSize */ 65536;

internal static ж<syscall.LazyDLL> modkernel32 = syscall.NewLazySystemDLL("kernel32.dll");
internal static nint osPageMask;
internal static void initᴛosPageMask() { osPageMask = osPageSize - 1; }
internal static nint osPageSize = os.Getpagesize();
internal static ж<syscall.LazyProc> procVirtualAlloc = modkernel32.NewProc("VirtualAlloc");
internal static ж<syscall.LazyProc> procVirtualFree = modkernel32.NewProc("VirtualFree");

internal static (uintptr, nint, error) mmap(nint size) {
    size = roundup(size, pageSize);
    var (addr, _, err) = procVirtualAlloc.Call(0, (uintptr)size, (uintptr)((uintptr)_MEM_COMMIT | (uintptr)_MEM_RESERVE), _PAGE_READWRITE);
    if (err._<windowsꓸErrno>() != 0 || addr == 0) {
        return (addr, size, err);
    }
    return (addr, size, default);
}

internal static error unmap(uintptr addr, nint size) {
    var (r, _, err) = procVirtualFree.Call(addr, 0, _MEM_RELEASE);
    if (r == 0) {
        return err;
    }

    return default;
}

} // end memory_package
