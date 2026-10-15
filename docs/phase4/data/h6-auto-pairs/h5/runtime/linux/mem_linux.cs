// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

internal static UntypedInt _EACCES => 13;
internal static UntypedInt _EINVAL => 22;

// Don't split the stack as this method may be invoked without a valid G, which
// prevents us from allocating more stack.
//
//go:nosplit
internal static @unsafe.Pointer sysAllocOS(uintptr n, @string vmaName) {
    var (Δp, err) = mmap(nil, n, (int32)((int32)_PROT_READ | (int32)_PROT_WRITE), (int32)((int32)_MAP_ANON | (int32)_MAP_PRIVATE), -1, 0);
    if (err != 0) {
        if (err == _EACCES) {
            print((@string)"runtime: mmap: access denied\n"u8);
            exit(2);
        }
        if (err == _EAGAIN) {
            print((@string)"runtime: mmap: too much locked memory (check 'ulimit -l').\n"u8);
            exit(2);
        }
        return default!;
    }
    setVMAName(Δp, n, vmaName);
    return Δp;
}

internal static ж<uint32> ᏑadviseUnused = new StandardBox<uint32>((uint32)_MADV_FREE);
internal static ref uint32 adviseUnused => ref ᏑadviseUnused.Value;

internal static UntypedInt madviseUnsupported => 0;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unalignedSysUnusedˢ = "unaligned sysUnused"u8;
internal static readonly @string unusedˢ = "unused"u8;
internal static readonly @string runtimeCannotDisableˢ = "runtime: cannot disable permissions in address space"u8;

internal static void sysUnusedOS(@unsafe.Pointer v, uintptr n) {
    if ((uintptr)((uintptr)v & (physPageSize - 1)) != 0 || (uintptr)(n & (physPageSize - 1)) != 0) {
        // madvise will round this to any physical page
        // *covered* by this range, so an unaligned madvise
        // will release more memory than intended.
        @throw(unalignedSysUnusedˢ);
    }

    var advise = atomic.Load(ᏑadviseUnused);
    if (debug.madvdontneed != 0 && advise != madviseUnsupported) {
        advise = _MADV_DONTNEED;
    }
    var exprᴛ1 = advise;
    var matchᴛ1 = false;
    if (exprᴛ1 == _MADV_FREE) { matchᴛ1 = true;
        do {
            if (madvise(v, n, _MADV_FREE) == 0) {
                break;
            }
            atomic.Store(ᏑadviseUnused, _MADV_DONTNEED);
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == _MADV_DONTNEED) {
        do {
            // MADV_FREE was added in Linux 4.5. Fall back on MADV_DONTNEED if it's
            // not supported.
            if (madvise(v, n, _MADV_DONTNEED) == 0) {
                break;
            }
            atomic.Store(ᏑadviseUnused, madviseUnsupported);
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == madviseUnsupported) { matchᴛ1 = true;
        // Since Linux 3.18, support for madvise is optional.
        // Fall back on mmap if it's not supported.
        // _MAP_ANON|_MAP_FIXED|_MAP_PRIVATE will unmap all the
        // pages in the old mapping, and remap the memory region.
        var (Δp, err) = mmap(v, n, (int32)((int32)_PROT_READ | (int32)_PROT_WRITE), (int32)((UntypedInt)(_MAP_ANON | _MAP_FIXED) | (int32)_MAP_PRIVATE), -1, 0);
        if (err == 0 && Δp != nil) {
            setVMAName(Δp, n, unusedˢ);
        }
    }

    if (debug.harddecommit > 0) {
        var (Δp, err) = mmap(v, n, _PROT_NONE, (int32)((UntypedInt)(_MAP_ANON | _MAP_FIXED) | (int32)_MAP_PRIVATE), -1, 0);
        if (Δp != v || err != 0) {
            @throw(runtimeCannotDisableˢ);
        }
        setVMAName(Δp, n, unusedˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeOutOfMemoryˢ = "runtime: out of memory"u8;
internal static readonly @string runtimeCannotRemapPagesˢ = "runtime: cannot remap pages in address space"u8;
internal static readonly @string usedˢ = "used"u8;

internal static void sysUsedOS(@unsafe.Pointer v, uintptr n) {
    if (debug.harddecommit > 0) {
        var (Δp, err) = mmap(v, n, (int32)((int32)_PROT_READ | (int32)_PROT_WRITE), (int32)((UntypedInt)(_MAP_ANON | _MAP_FIXED) | (int32)_MAP_PRIVATE), -1, 0);
        if (err == _ENOMEM) {
            @throw(runtimeOutOfMemoryˢ);
        }
        if (Δp != v || err != 0) {
            @throw(runtimeCannotRemapPagesˢ);
        }
        setVMAName(Δp, n, usedˢ);
        return;
    }
}

internal static void sysHugePageOS(@unsafe.Pointer v, uintptr n) {
    if (physHugePageSize != 0) {
        // Round v up to a huge page boundary.
        var beg = alignUp((uintptr)v, physHugePageSize);
        // Round v+n down to a huge page boundary.
        var end = alignDown((uintptr)v + n, physHugePageSize);

        if (beg < end) {
            madvise((@unsafe.Pointer)beg, end - beg, _MADV_HUGEPAGE);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unalignedSysNoHugePageOSˢ = "unaligned sysNoHugePageOS"u8;

internal static void sysNoHugePageOS(@unsafe.Pointer v, uintptr n) {
    if ((uintptr)((uintptr)v & (physPageSize - 1)) != 0) {
        // The Linux implementation requires that the address
        // addr be page-aligned, and allows length to be zero.
        @throw(unalignedSysNoHugePageOSˢ);
    }
    madvise(v, n, _MADV_NOHUGEPAGE);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unalignedˢ = "unaligned sysHugePageCollapseOS"u8;

internal static void sysHugePageCollapseOS(@unsafe.Pointer v, uintptr n) {
    if ((uintptr)((uintptr)v & (physPageSize - 1)) != 0) {
        // The Linux implementation requires that the address
        // addr be page-aligned, and allows length to be zero.
        @throw(unalignedˢ);
    }
    if (physHugePageSize == 0) {
        return;
    }
    // N.B. If you find yourself debugging this code, note that
    // this call can fail with EAGAIN because it's best-effort.
    // Also, when it returns an error, it's only for the last
    // huge page in the region requested.
    //
    // It can also sometimes return EINVAL if the corresponding
    // region hasn't been backed by physical memory. This is
    // difficult to guarantee in general, and it also means
    // there's no way to distinguish whether this syscall is
    // actually available. Oops.
    //
    // Anyway, that's why this call just doesn't bother checking
    // any errors.
    madvise(v, n, _MADV_COLLAPSE);
}

// Don't split the stack as this function may be invoked without a valid G,
// which prevents us from allocating more stack.
//
//go:nosplit
internal static void sysFreeOS(@unsafe.Pointer v, uintptr n) {
    munmap(v, n);
}

internal static void sysFaultOS(@unsafe.Pointer v, uintptr n) {
    mprotect(v, n, _PROT_NONE);
    madvise(v, n, _MADV_DONTNEED);
}

internal static @unsafe.Pointer sysReserveOS(@unsafe.Pointer v, uintptr n, @string vmaName) {
    var (Δp, err) = mmap(v, n, _PROT_NONE, (int32)((int32)_MAP_ANON | (int32)_MAP_PRIVATE), -1, 0);
    if (err != 0) {
        return default!;
    }
    setVMAName(Δp, n, vmaName);
    return Δp;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeCannotMapPagesInˢ = "runtime: cannot map pages in arena address space"u8;

internal static void sysMapOS(@unsafe.Pointer v, uintptr n, @string vmaName) {
    var (Δp, err) = mmap(v, n, (int32)((int32)_PROT_READ | (int32)_PROT_WRITE), (int32)((UntypedInt)(_MAP_ANON | _MAP_FIXED) | (int32)_MAP_PRIVATE), -1, 0);
    if (err == _ENOMEM) {
        @throw(runtimeOutOfMemoryˢ);
    }
    if (Δp != v || err != 0) {
        print((@string)"runtime: mmap("u8, @unsafe.Pointer.OrTypedNil(v), (@string)", "u8, n, (@string)") returned "u8, @unsafe.Pointer.OrTypedNil(Δp), (@string)", "u8, err, (@string)"\n"u8);
        @throw(runtimeCannotMapPagesInˢ);
    }
    setVMAName(Δp, n, vmaName);

    // Disable huge pages if the GODEBUG for it is set.
    //
    // Note that there are a few sysHugePage calls that can override this, but
    // they're all for GC metadata.
    if (debug.disablethp != 0) {
        sysNoHugePageOS(v, n);
    }
}

internal static bool needZeroAfterSysUnusedOS() {
    return debug.madvdontneed == 0;
}

} // end runtime_package
