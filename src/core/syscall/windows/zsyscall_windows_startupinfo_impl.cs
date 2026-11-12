// zsyscall_windows_startupinfo_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The STARTUPINFOW member of the syscall struct-passing class, and the one that kept syscall's
// banked row from re-banking at 1.24.13.
//
// The class, its failure mode and its remedy are documented once, in zsyscall_windows_impl.cs
// (GetTimeZoneInformation, findFirstFile1/findNextFile1, Process32First/Process32Next); that file is
// the reference and this one does not restate it. This member differs in WHAT makes the record
// reference-bearing: not an inline character array but POINTER fields. Go's StartupInfo holds
// `*uint16` (the reserved slot, Desktop, Title) and `*byte` (the second reserved slot), and each
// converts to a `ж<T>` field -- a managed reference where the native record has a raw address. So
// the converted StartupInfo is a CLR auto-layout record with object references in it, and its
// address is not an address the kernel may write STARTUPINFOW's 104 bytes over.
//
// MEASURED, syscall row at 3469154a95 (2026-09-22): TestGetStartupInfo panics in the boundary's
// own refusal before any native code runs -- `syscall: argument 0 is a managed pointer token, not
// an address -- the pointee is reference-bearing` (dll_windows.cs refuseManagedPointerTokens). That
// refusal is the correct answer for the generated body; this file is what the refusal asks for.
// The row read 64 matched + 1 diverged; syscall is banked at 65.
//
// THE POINTER FIELDS ARE FAITHFUL, not approximated. GetStartupInfoW fills lpDesktop and lpTitle
// with addresses of strings in the process's own startup parameters, which live, unmoved, for the
// life of the process. A pointer the kernel hands out into native memory is exactly what golib's
// native-backed box kind models: `(ж<T>)(uintptr)address` is a NativeBox that reads and writes
// through that address, compares by it, and is nil for a zero address -- Go's
// `(*T)(unsafe.Pointer(uintptr(0))) == nil`. The reserved slots are transcribed the same way so the
// wrapper's contract stays "fills the record"; Go's blank fields cannot be read by any caller, but
// the record is the kernel's, whole.
//
// THE CALL is unchanged from the generated body -- same LazyProc, same syscall.Syscall trampoline,
// same discarded result (GetStartupInfoW returns void) -- and only the memory the argument names is
// different, on the zsyscall_windows_version_impl.cs precedent (internal/syscall/windows).

using System;

// Hand-owned (no zsyscall_windows_startupinfo_impl.go exists, so a reconvert never regenerates this
// file). The declaration it replaces is registered in the converter's manualConversionFuncs, which
// is what turns the generated body into a placeholder.
[module: go.GoManualConversion]

// The mirror and its address are pointer work. Declared rather than inherited -- see
// net_windows_impl.cs.
[module: go.GoRequiresUnsafe]

namespace go;

partial class syscall_package
{
    // The mirror is NOT declared here: exec_windows.cs (hand-owned, the StartProcess seam) already
    // carries NativeStartupInfoW, the blittable STARTUPINFOW in this same partial class, and one
    // record has one mirror. Its pointer-sized members are IntPtr -- raw addresses, never managed
    // references -- and sequential layout with natural alignment reproduces the native offsets,
    // including the padding after cb and after cbReserved2 on 64-bit.

    // The documented native size for this process's pointer width. Stated so the mirror's layout is
    // checked at the boundary rather than assumed -- the failure this class exists to prevent is a
    // silent offset.
    private static int NativeStartupInfoWSize => IntPtr.Size == 8 ? 104 : 68;

    /// <summary>
    /// Fills the caller's <c>StartupInfo</c> from kernel32 through a blittable mirror.
    /// </summary>
    /// <remarks>
    /// Go's signature is preserved exactly: GetStartupInfoW returns nothing and cannot fail, so the
    /// generated wrapper discards Syscall's results and so does this one.
    /// </remarks>
    internal static unsafe void getStartupInfo(ж<StartupInfo> ᏑstartupInfo)
    {
        if (sizeof(NativeStartupInfoW) != NativeStartupInfoWSize)
        {
            throw new InvalidOperationException(
                $"syscall: the NativeStartupInfoW mirror is {sizeof(NativeStartupInfoW)} bytes where " +
                $"kernel32 writes {NativeStartupInfoWSize} -- every field past the first would come " +
                "from the wrong offset.");
        }

        ref StartupInfo managed = ref ᏑstartupInfo.Value;

        // GetStartupInfoW is output-only (it does not read cb), so the mirror starts zeroed rather
        // than seeded from the caller's record.
        NativeStartupInfoW native = default;

        // No KeepAlive is owed: the only pointer handed over addresses this stack local, which the
        // GC does not move and which outlives the call by construction.
        Syscall(procGetStartupInfoW.Addr(), 1, (uintptr)(void*)(&native), 0, 0);

        managed.Cb = native.Cb;
        managed._ = (ж<uint16>)(uintptr)(nuint)native.Reserved;
        managed.Desktop = (ж<uint16>)(uintptr)(nuint)native.Desktop;
        managed.Title = (ж<uint16>)(uintptr)(nuint)native.Title;
        managed.X = native.X;
        managed.Y = native.Y;
        managed.XSize = native.XSize;
        managed.YSize = native.YSize;
        managed.XCountChars = native.XCountChars;
        managed.YCountChars = native.YCountChars;
        managed.FillAttribute = native.FillAttribute;
        managed.Flags = native.Flags;
        managed.ShowWindow = native.ShowWindow;
        managed.__ = native.Reserved2Length;
        managed.___ = (ж<byte>)(uintptr)(nuint)native.Reserved2;
        managed.StdInput = (Handle)(uintptr)(nuint)native.StdInput;
        managed.StdOutput = (Handle)(uintptr)(nuint)native.StdOutput;
        managed.StdErr = (Handle)(uintptr)(nuint)native.StdErr;
    }
}
