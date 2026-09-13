// zsyscall_windows_version_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The OS-VERSION member of the syscall struct-passing class, and the one EVERY Windows TCP dial
// reaches.
//
// The class, its failure mode and its remedy are documented once, in
// syscall/windows/zsyscall_windows_impl.cs (GetTimeZoneInformation, findFirstFile1/findNextFile1,
// Process32First/Process32Next); that file is the reference and this one does not restate it. The
// short form: a converted struct holding golib `array<T>` or `ж<T>` fields is a CLR auto-layout
// record with MANAGED REFERENCES where the native record has INLINE storage, so handing the kernel
// its address makes the kernel write the native record's full length over a much smaller managed
// object.
//
// THIS ONE, in numbers. OSVERSIONINFOW is 276 bytes: five DWORDs and then szCSDVersion INLINE as
// WCHAR[128] (20 + 256). That number is not read off documentation alone -- the conversion computes
// it for itself, and version_windows.cs:37 assigns the folded `unsafe.Sizeof(info)` literal 276 to
// osVersionInfoSize before the call, which is also the value RtlGetVersion validates. The converted
// _OSVERSIONINFOW (version_windows.cs:20) cannot have that layout: its csdVersion is
// `array<uint16>`, a readonly struct over a `uint16[]`, so the record CONTAINS A MANAGED REFERENCE
// and is roughly two dozen bytes with CLR auto-layout free to place its fields anywhere. The
// per-field offsets are not restated here because the remedy does not depend on them: it replaces
// the record the kernel sees, whole.
//
// TWO FAILURE MODES, one before the pointer-storage repair and one after -- worth separating,
// because the repair changes this site's symptom rather than causing its defect. Before it,
// `(uintptr)Ꮡinfo` fell through to `fixed (void* ptr = &value.Value)` and ntdll received a REAL
// address: 276 bytes written over a much smaller managed object, and -- since auto-layout groups
// references first, as measured for SHARE_INFO_2 one file over -- the very first DWORD lands on an
// object reference the GC will later follow. Silent, and blamed downstream. After it, a
// reference-bearing pointee has no pinnable slot, so StandardBox answers PointerStorage.None, the
// order-token arm fires, and ntdll is handed a token rather than an address: 0xC0000005 inside
// ntdll!RtlGetVersion, immediately, measured on the fleet's Windows leg. The repair makes the
// corruption loud; this file is what removes it, and landing one without the other is what the
// re-seat's condition exists to prevent.
//
// WHO REACHES IT. version() has two callers, both behind a sync.Once in this same file's Go
// original: initTCPKeepAlive (version_windows.cs:48, on the error path of its probe socket) and
// SupportTCPInitialRTONoSYNRetransmissions (version_windows.cs:98). net's Windows TCP dial consults
// those on the way to setting keep-alive options, so the first dial in a process runs this.
//
// THE CENSUS ROW THAT WAS MISSING. This package's zsyscall_windows_impl.cs header enumerates the
// same-shape wrappers left for the suite that would reach them -- and rtlGetVersion is not among
// them. The gap is not arbitrary: that list was built from wrappers this package's OWN suite
// reaches, and rtlGetVersion is reached from net, one package out. Three of the four rows it does
// list have since been cured elsewhere in this directory (Module32First/Module32Next in
// zsyscall_windows_module_impl.cs, WSASendMsg/WSARecvMsg in zsyscall_windows_wsa_impl.cs,
// NetUserGetLocalGroups in zsyscall_windows_ptrout_impl.cs), on the same convention this file
// follows: the curing file records the movement, the reference header is not rewritten for each.
//
// ONE DELIBERATE DIVERGENCE from the GetTimeZoneInformation precedent, stated rather than quietly
// taken: that wrapper calls through [LibraryImport], this one keeps the generated body's own
// `syscall.Syscall(procRtlGetVersion.Addr(), ...)` trampoline. Two reasons. The delta from the
// generated wrapper is then exactly one thing -- the memory the pointer argument names -- which is
// the smallest change that can fix this; and Go resolves ntdll.dll through sysdll.Add, which pins
// the load to the system directory, where a [LibraryImport] would substitute the default P/Invoke
// probe. Neither is a claim that [LibraryImport] would be wrong here; the mirror is the shape the
// ruling named, and the call vehicle is the part this file gets to choose.

using System;
using System.Runtime.InteropServices;

// Hand-owned (no zsyscall_windows_version_impl.go exists, so a reconvert never regenerates this
// file). The declaration it replaces is registered in the converter's manualConversionFuncs, which
// is what turns the generated body into a placeholder.
[module: go.GoManualConversion]

// The mirror's `fixed` buffer and its address are pointer work. Declared rather than inherited --
// see net_windows_impl.cs.
[module: go.GoRequiresUnsafe]

namespace go.@internal.syscall;

using syscall = go.syscall_package;

partial class windows_package
{
    // szCSDVersion's element count, as both OSVERSIONINFOW and the converted _OSVERSIONINFOW
    // declare it. A `fixed` buffer needs a compile-time constant, and the converted struct's own
    // `new(128)` is a run-time field initializer.
    private const int csdVersionLength = 128;

    // The documented native size, and the same 276 version() writes into osVersionInfoSize. Stated
    // so the mirror's own layout is checked at the boundary rather than assumed -- the failure this
    // file exists to prevent is a silent offset.
    private const int NativeOsVersionInfoWSize = 276;

    /// <summary>
    /// <c>OSVERSIONINFOW</c> exactly as ntdll lays it out: 276 bytes, the CSD string inline as
    /// <c>WCHAR[128]</c>.
    /// </summary>
    /// <remarks>
    /// <c>fixed</c> is what keeps that buffer inline -- a C# array field would be another managed
    /// reference, which is the whole bug -- so the struct is blittable and needs no marshalling
    /// layer. Every field is 4-byte aligned and the buffer is 2-byte aligned, so sequential layout
    /// reproduces the native offsets (0, 4, 8, 12, 16, 20) with no padding anywhere.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeOsVersionInfoW
    {
        public uint32 OSVersionInfoSize;
        public uint32 MajorVersion;
        public uint32 MinorVersion;
        public uint32 BuildNumber;
        public uint32 PlatformId;
        public fixed uint16 CSDVersion[csdVersionLength];
    }

    /// <summary>
    /// Fills the caller's <c>_OSVERSIONINFOW</c> from ntdll through a blittable mirror.
    /// </summary>
    /// <remarks>
    /// Go's signature is preserved exactly. RtlGetVersion always succeeds -- Go's own comment above
    /// the //sys line says so, and the generated wrapper discards the status accordingly -- so this
    /// returns void and reports nothing the caller did not already have.
    /// </remarks>
    internal static unsafe void rtlGetVersion(ж<_OSVERSIONINFOW> Ꮡinfo)
    {
        if (sizeof(NativeOsVersionInfoW) != NativeOsVersionInfoWSize)
        {
            throw new InvalidOperationException(
                $"internal/syscall/windows: the NativeOsVersionInfoW mirror is " +
                $"{sizeof(NativeOsVersionInfoW)} bytes where ntdll writes " +
                $"{NativeOsVersionInfoWSize} -- every field past the first would come from the " +
                "wrong offset.");
        }

        ref _OSVERSIONINFOW managed = ref Ꮡinfo.Value;

        NativeOsVersionInfoW native = default;

        // The CALLER's value, not the mirror's size: Go sets this to unsafe.Sizeof(info) before the
        // call and RtlGetVersion validates it, so substituting anything else would change what the
        // kernel is asked. The two agree at 276, and the assertion above is what keeps them
        // agreeing.
        native.OSVersionInfoSize = managed.osVersionInfoSize;

        // The CALL is unchanged from the generated body -- same LazyProc, same syscall.Syscall,
        // same discarded status -- and only the memory the first argument names is different.
        // No KeepAlive is owed: the only pointer handed over addresses this stack local, which
        // the GC does not move and which outlives the call by construction.
        syscall.Syscall(procRtlGetVersion.Addr(), 1, (uintptr)(void*)(&native), 0, 0);

        managed.osVersionInfoSize = native.OSVersionInfoSize;
        managed.majorVersion = native.MajorVersion;
        managed.minorVersion = native.MinorVersion;
        managed.buildNumber = native.BuildNumber;
        managed.platformId = native.PlatformId;

        // Copied whole, NULs included, for the reason the GetTimeZoneInformation twin spells out: a
        // caller reads such a field as UTF16ToString(csdVersion[:]), which stops at the first NUL,
        // and Windows pads the remainder -- copying only up to the terminator would leave stale
        // runes behind it. No caller in GOROOT reads this field; it is transcribed anyway so the
        // wrapper's contract stays "fills the record" rather than "fills the three fields version()
        // happens to read".
        copyNativeCsdVersion(native.CSDVersion, ref managed.csdVersion);
    }

    // Copies the native WCHAR[128] into the converted struct's `array<uint16>` field. The
    // destination is (re)allocated when it is not already that long, so a record that reached here
    // as `default` -- its field initializer never having run -- is filled rather than dereferenced
    // through a null backing.
    private static unsafe void copyNativeCsdVersion(uint16* source, ref array<uint16> destination)
    {
        if (destination.Length != csdVersionLength)
        {
            destination = new array<uint16>(csdVersionLength);
        }

        for (nint i = 0; i < csdVersionLength; i++)
        {
            destination[i] = source[i];
        }
    }
}
