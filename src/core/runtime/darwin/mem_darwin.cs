// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

// Don't split the stack as this function may be invoked without a valid G,
// which prevents us from allocating more stack.
//
//go:nosplit
internal static @unsafe.Pointer sysAllocOS(uintptr n, @string _) {
    var (v, err) = mmap(nil, n, (int32)((int32)_PROT_READ | (int32)_PROT_WRITE), (int32)((int32)_MAP_ANON | (int32)_MAP_PRIVATE), -1, 0);
    if (err != 0) {
        return default;
    }
    return v;
}

internal static void sysUnusedOS(@unsafe.Pointer v, uintptr n) {
    // MADV_FREE_REUSABLE is like MADV_FREE except it also propagates
    // accounting information about the process to task_info.
    madvise(v, n, _MADV_FREE_REUSABLE);
}

internal static void sysUsedOS(@unsafe.Pointer v, uintptr n) {
    // MADV_FREE_REUSE is necessary to keep the kernel's accounting
    // accurate. If called on any memory region that hasn't been
    // MADV_FREE_REUSABLE'd, it's a no-op.
    madvise(v, n, _MADV_FREE_REUSE);
}

internal static void sysHugePageOS(@unsafe.Pointer v, uintptr n) {
}

internal static void sysNoHugePageOS(@unsafe.Pointer v, uintptr n) {
}

internal static void sysHugePageCollapseOS(@unsafe.Pointer v, uintptr n) {
}

// Don't split the stack as this function may be invoked without a valid G,
// which prevents us from allocating more stack.
//
//go:nosplit
internal static void sysFreeOS(@unsafe.Pointer v, uintptr n) {
    munmap(v, n);
}

internal static void sysFaultOS(@unsafe.Pointer v, uintptr n) {
    mmap(v, n, _PROT_NONE, (int32)((UntypedInt)(_MAP_ANON | _MAP_PRIVATE) | (int32)_MAP_FIXED), -1, 0);
}

internal static @unsafe.Pointer sysReserveOS(@unsafe.Pointer v, uintptr n, @string _) {
    var (Δp, err) = mmap(v, n, _PROT_NONE, (int32)((int32)_MAP_ANON | (int32)_MAP_PRIVATE), -1, 0);
    if (err != 0) {
        return default;
    }
    return Δp;
}

internal static UntypedInt _ENOMEM => 12;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeOutOfMemoryˢ = "runtime: out of memory"u8;
internal static readonly @string runtimeCannotMapPagesInˢ = "runtime: cannot map pages in arena address space"u8;

internal static void sysMapOS(@unsafe.Pointer v, uintptr n, @string _) {
    var (Δp, err) = mmap(v, n, (int32)((int32)_PROT_READ | (int32)_PROT_WRITE), (int32)((UntypedInt)(_MAP_ANON | _MAP_FIXED) | (int32)_MAP_PRIVATE), -1, 0);
    if (err == _ENOMEM) {
        @throw(runtimeOutOfMemoryˢ);
    }
    if (Δp != v || err != 0) {
        print((@string)"runtime: mmap("u8, @unsafe.Pointer.OrTypedNil(v), (@string)", "u8, n, (@string)") returned "u8, @unsafe.Pointer.OrTypedNil(Δp), (@string)", "u8, err, (@string)"\n"u8);
        @throw(runtimeCannotMapPagesInˢ);
    }
}

internal static bool needZeroAfterSysUnusedOS() {
    return true;
}

} // end runtime_package
