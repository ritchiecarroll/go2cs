// sys_darwin_mmap_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// runtime.mmap / runtime.munmap on darwin, hand-owned (G's ruling, 2026-10-19; manualConversionFuncs["runtime"],
// goosDarwin): the OS mapping primitives every darwin sys*OS path reaches.
//
// WHY. Go's bodies reach libc through libcCall(mmap_trampoline, &args). The converted args struct carries
// unsafe.Pointer fields, so it has no pinnable storage and libcCall refuses it by name (libccall_impl.cs, "a
// reference-bearing args struct"): no darwin allocation could map memory. These make the trampolines' libc calls
// directly, with their contract: mmap answers (nil, errno) when libc answers MAP_FAILED, and munmap's failure is
// fatal, as the trampoline's deliberate fault is.
//
// REGISTRATION. Every range mmap maps is recorded in golib's GoOsMemory, and munmap releases its range first, so
// an address the kernel hands to another thread's mmap the moment it is unmapped is never unregistered after it.
// The runtime OWNS that memory, so a pointer word in it is a Go pointer, while a word in any other native memory
// is a foreign address that builtin.NativeArrayPointer reads as one. linux registers at sysMmap / sysMunmap
// (mem_linux_impl.cs), windows at sysAllocOS / sysReserveOS / sysFreeOS (mem_windows_impl.cs).
//
// Hand-owned: there is no sys_darwin_mmap_impl.go, so a -stdlib reconvert never regenerates this file.

using System.Runtime.InteropServices;
using @unsafe = go.unsafe_package;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    [DllImport("libc", EntryPoint = "mmap", SetLastError = true)]
    private static extern nint libc_mmap(nint addr, nuint length, int prot, int flags, int fd, long offset);

    [DllImport("libc", EntryPoint = "munmap", SetLastError = true)]
    private static extern int libc_munmap(nint addr, nuint length);

    // mmap is used to do low-level memory allocation via mmap.
    internal static (@unsafe.Pointer, nint) mmap(@unsafe.Pointer addr, uintptr n, int32 prot, int32 flags, int32 fd, uint32 off)
    {
        // The trampoline loads the offset with MOVL: a uint32, zero-extended into off_t.
        nint r = libc_mmap((nint)(nuint)(uintptr)addr, (nuint)n, (int)prot, (int)flags, (int)fd, (long)(ulong)off);

        if (r == -1)
            return (nil, (nint)Marshal.GetLastPInvokeError());

        GoOsMemory.Register((nuint)r, (nuint)n);
        return (new @unsafe.Pointer((nuint)r), 0);
    }

    internal static void munmap(@unsafe.Pointer addr, uintptr n)
    {
        GoOsMemory.Unregister((nuint)(uintptr)addr, (nuint)n);

        if (libc_munmap((nint)(nuint)(uintptr)addr, (nuint)n) != 0)
            @throw("runtime: munmap failed"u8);
    }

    // TEST SEAMS, named as the windows flavour's (mem_windows_impl.cs), so one GolibTests arm drives each: Go's
    // converted bodies (mem_darwin.cs), which map and unmap through mmap / munmap above.
    // Block bodies: a declaration line that names sysAllocOS would read to TestManualConversionRegistrationsHaveBodies
    // as this file declaring it, and on this flavour Go's converted body is the one.
    public static nuint GoSysAllocOS(nuint n)
    {
        return ((uintptr)sysAllocOS(n, ""u8)).Value;
    }

    public static void GoSysFreeOS(nuint v, nuint n)
    {
        sysFreeOS(new @unsafe.Pointer(v), n);
    }

    public static nuint GoSysReserveOS(nuint v, nuint n)
    {
        return ((uintptr)sysReserveOS(new @unsafe.Pointer(v), n, ""u8)).Value;
    }
}
