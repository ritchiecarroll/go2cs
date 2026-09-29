// mem_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// runtime.sysAllocOS / sysFreeOS on Windows, hand-owned (COORD ruling 2026-09-22;
// manualConversionFuncs["runtime"], goosWindows).
//
// WHY. Go's bodies call VirtualAlloc / VirtualFree through stdcall4 / stdcall3 -> asmcgocall, which has
// no managed body, so every sysAlloc threw NotImplementedException. On the runtime row that one root
// (first reached by TestAddrRangesAdd: NewAddrRanges -> addrRanges.init -> persistentalloc ->
// persistentalloc1 -> sysAlloc) died holding globalAlloc's lock, and 70 later lockers died on the
// abandoned-lock panic that quotes it.
//
// WHAT. The SAME kernel calls, made directly -- the linux flavour's precedent (mem_linux_impl.cs calls
// libc mmap/munmap rather than a managed allocator). VirtualAlloc(nil, n, MEM_COMMIT|MEM_RESERVE,
// PAGE_READWRITE) is Go's own request: committed, ZEROED, read-write pages aligned to the 64 KB
// allocation granularity (what persistentalloc1's page-aligned chunk arithmetic and sysAlloc's callers
// assume), or 0 on failure, which Go reports as nil. sysFreeOS is VirtualFree(v, 0, MEM_RELEASE),
// releasing the whole allocation exactly as Go does, and throws Go's own message on failure. A managed
// allocator (NativeMemory) would satisfy the alignment only by over-allocating and could not release a
// region some other VirtualAlloc produced; the kernel call pairs with every Windows region by
// construction. The pages are NATIVE and outside the CLR heap.
//
// Hand-owned: there is no mem_windows_impl.go, so a -stdlib reconvert never regenerates this file.

using System.Runtime.InteropServices;
using @unsafe = go.unsafe_package;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    private static extern nint VirtualAlloc(nint lpAddress, nuint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool VirtualFree(nint lpAddress, nuint dwSize, uint dwFreeType);

    internal static @unsafe.Pointer sysAllocOS(uintptr n)
    {
        nint p = VirtualAlloc(0, (nuint)n, (uint)(_MEM_COMMIT | _MEM_RESERVE), (uint)_PAGE_READWRITE);

        return p == 0 ? nil : new @unsafe.Pointer((nuint)p);
    }

    internal static void sysFreeOS(@unsafe.Pointer v, uintptr n)
    {
        if (!VirtualFree((nint)(nuint)(uintptr)v, 0, (uint)_MEM_RELEASE))
        {
            print((@string)"runtime: VirtualFree of "u8, n, (@string)" bytes failed with errno="u8, (uint32)Marshal.GetLastPInvokeError(), (@string)"\n"u8);
            @throw((@string)"runtime: failed to release pages"u8);
        }
    }

    // sysReserveOS / sysUsedOS / sysUnusedOS (W1, COORD ruling 2026-09-28): the page allocator's reserve,
    // commit and decommit, reached once a row gets past sysAlloc. Go's bodies, their kernel calls made
    // directly. sysReserveOS's converted body also wrote through its own parameter (`v = ...` on an
    // unsafe.Pointer PARAMETER emitted as `v.Value = ...`, a converter defect routed separately), which
    // this hand-own leaves behind.

    internal static @unsafe.Pointer sysReserveOS(@unsafe.Pointer v, uintptr n)
    {
        // v is just a hint. First try at v; this fails if any of [v, v+n) is already reserved.
        nint p = VirtualAlloc((nint)(nuint)(uintptr)v, (nuint)n, (uint)_MEM_RESERVE, (uint)_PAGE_READWRITE);

        if (p != 0)
            return new @unsafe.Pointer((nuint)p);

        // Next let the kernel choose the address.
        p = VirtualAlloc(0, (nuint)n, (uint)_MEM_RESERVE, (uint)_PAGE_READWRITE);

        return p == 0 ? nil : new @unsafe.Pointer((nuint)p);
    }

    internal static void sysUsedOS(@unsafe.Pointer v, uintptr n)
    {
        nint @base = (nint)(nuint)(uintptr)v;

        if (VirtualAlloc(@base, (nuint)n, (uint)_MEM_COMMIT, (uint)_PAGE_READWRITE) == @base)
            return;

        // Commit failed: usually a range merged from two VirtualAlloc reservations, which one VirtualAlloc
        // cannot span. Commit successively smaller pieces, exactly as Go does (see sysUnusedOS).
        nuint k = (nuint)n;

        while (k > 0)
        {
            nuint small = k;

            while (small >= 4096 && VirtualAlloc(@base, small, (uint)_MEM_COMMIT, (uint)_PAGE_READWRITE) == 0)
            {
                small /= 2;
                small &= ~(nuint)(4096 - 1);
            }

            if (small < 4096)
            {
                uint32 errno = (uint32)Marshal.GetLastPInvokeError();

                if (errno == _ERROR_NOT_ENOUGH_MEMORY || errno == _ERROR_COMMITMENT_LIMIT)
                {
                    print((@string)"runtime: VirtualAlloc of "u8, n, (@string)" bytes failed with errno="u8, errno, (@string)"\n"u8);
                    @throw((@string)"out of memory"u8);
                }

                print((@string)"runtime: VirtualAlloc of "u8, (uintptr)small, (@string)" bytes failed with errno="u8, errno, (@string)"\n"u8);
                @throw((@string)"runtime: failed to commit pages"u8);
            }

            @base += (nint)small;
            k -= small;
        }
    }

    internal static void sysUnusedOS(@unsafe.Pointer v, uintptr n)
    {
        nint @base = (nint)(nuint)(uintptr)v;

        if (VirtualFree(@base, (nuint)n, (uint)_MEM_DECOMMIT))
            return;

        // Decommit failed: usually memory merged from two different VirtualAlloc calls, and Windows lets
        // each VirtualFree handle pages from a single VirtualAlloc. Free successively smaller pieces until
        // something is freed, then repeat (Go's own O(n log n) walk).
        nuint remaining = (nuint)n;

        while (remaining > 0)
        {
            nuint small = remaining;

            while (small >= 4096 && !VirtualFree(@base, small, (uint)_MEM_DECOMMIT))
            {
                small /= 2;
                small &= ~(nuint)(4096 - 1);
            }

            if (small < 4096)
            {
                print((@string)"runtime: VirtualFree of "u8, (uintptr)small, (@string)" bytes failed with errno="u8, (uint32)Marshal.GetLastPInvokeError(), (@string)"\n"u8);
                @throw((@string)"runtime: failed to decommit pages"u8);
            }

            @base += (nint)small;
            remaining -= small;
        }
    }

    // TEST SEAMS (pinner_impl.cs's pattern; GolibTests is not in the InternalsVisibleTo grant).
    public static nuint GoSysAllocOS(nuint n) => ((uintptr)sysAllocOS(n)).Value;

    public static void GoSysFreeOS(nuint v, nuint n) => sysFreeOS(new @unsafe.Pointer(v), n);

    public static nuint GoSysReserveOS(nuint v, nuint n) => ((uintptr)sysReserveOS(new @unsafe.Pointer(v), n)).Value;

    public static void GoSysUsedOS(nuint v, nuint n) => sysUsedOS(new @unsafe.Pointer(v), n);

    public static void GoSysUnusedOS(nuint v, nuint n) => sysUnusedOS(new @unsafe.Pointer(v), n);
}
