// zsyscall_windows_version_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// The VERSION member of the syscall struct-passing class -- the wrappers whose STRUCT cannot cross
// the managed/native boundary by address.
//
// The class, its failure mode and its remedy are documented once, in
// syscall/windows/zsyscall_windows_impl.cs (GetTimeZoneInformation, findFirstFile1/findNextFile1,
// Process32First/Process32Next); that file is the reference and this one does not restate it. The
// short form: a converted struct holding golib `array<T>` or `ж<T>` fields is a CLR auto-layout
// record with MANAGED REFERENCES where the native record has INLINE storage, so handing the kernel
// its address makes the kernel write the native record's full length over a much smaller managed
// object.
//
// THIS ONE, IN NUMBERS. `_OSVERSIONINFOW` (version_windows.cs) is 276 bytes: five uint32 --
// osVersionInfoSize, majorVersion, minorVersion, buildNumber, platformId, twenty bytes in all --
// followed by `csdVersion [128]uint16`, 256 bytes INLINE. The converted struct holds csdVersion as
// an `array<uint16>` MANAGED REFERENCE, so the CLR auto-layouts the record at roughly 32 bytes with
// that reference grouped ahead of the scalars, and the generated wrapper handed ntdll the address
// of it:
//
//     internal static void rtlGetVersion(ж<_OSVERSIONINFOW> Ꮡinfo) {
//         var ᴋ47 = Ꮡinfo;
//             syscall.Syscall(procRtlGetVersion.Addr(), 1, (uintptr)ᴋ47, 0, 0);
//         System.GC.KeepAlive(ᴋ47);
//         return;
//     }
//
// RtlGetVersion writes a full 276-byte record through that one argument.
//
// WHY THIS MEMBER WAS UNMASKED RATHER THAN REACHED, which is the part worth reading. Every other
// member of the class announced itself when a new suite first called it. This one has been on the
// Windows TCP dial path all along and reported nothing, because the class's QUIET shape was in
// force: while `(uintptr)` of a box PINNED it and published a real managed address, ntdll wrote 276
// bytes over a ~32-byte object and `version()` read majorVersion / minorVersion / buildNumber out of
// offsets the kernel had not written as those fields -- so it answered zeros, with no error and no
// fault. That is the Process32First shape, one record smaller: a quiet wrong ANSWER.
//
// The pointer-token cut (golib ж.cs, `operator uintptr`) ends the quiet shape. A pointee carrying
// managed references has no pinnable storage -- StandardBox keeps the value in a FIELD of the box
// object, which `fixed` cannot hold still, and the address it would hand out was measured going
// stale -- so the conversion now mints and registers the box's stable order TOKEN instead of
// publishing an address that was never an address. ntdll therefore writes through a token value and
// the process dies with 0xC0000005 at the first TCP dial. The cut is correct and stays; this wrapper
// was always wrong, and what changed is only that it stopped hiding. A defect that faults is
// strictly better than one that answers zeros.
//
// WHO REACHES IT. `version()` (version_windows.cs) is rtlGetVersion's only caller, and it is called
// from initTCPKeepAlive and from initSupportTCPInitialRTONoSYNRetransmissions -- both of which
// net.connect runs on EVERY Windows TCP dial, so no converted program could open a TCP connection.
// The guards are therefore two ordinary networking behavioral tests:
//
//     TcpLoopbackRoundTrip   and   NetDeadlineMatrix
//
// each of which failed its Output phase with `exit code mismatch: C# -1073741819 vs Go 0` before
// this file existed, and passes all four phases after it. "It no longer crashes" is not the property
// either one proves, though, and this class demands more than that: a mirror with the wrong offsets
// returns garbage without faulting, which is precisely the behaviour being replaced. The positive
// reading is that version() now answers the host's REAL Windows version, measured 10.0.26100 on the
// box this landed on.
//
// This wrapper receives the record as a TYPED `ж<_OSVERSIONINFOW>`, so the ordinary mirror remedy
// applies -- as it does to Module32First/Module32Next beside it, and as it does not to NetShareAdd,
// whose record arrives as a raw byte address with the managed identity already gone. The CALL is
// unchanged from the generated body: same LazyProc, same `syscall.Syscall`, same ignored NTSTATUS
// (Go's own comment above the //sys line: "According to documentation, RtlGetVersion function always
// succeeds"). Only the memory the second argument names is different.
//
// DELIBERATELY NOT COVERED: nothing else in this file, and nothing else in zsyscall_windows.cs. Only
// rtlGetVersion is registered in manualConversionFuncs and only rtlGetVersion is written here. Every
// other declaration in the generated file passes scalars, handles and byte pointers, which convert
// faithfully; this package's other members of the struct-passing class are hand-owned in their own
// companions (NetShareAdd, Module32First/Next, adjustTokenPrivileges, NetUserGetLocalGroups and the
// WSA family), and the standing census of what remains is in zsyscall_windows_impl.cs's header.

using System;
using System.Runtime.InteropServices;

// Hand-owned (no zsyscall_windows_version_impl.go exists, so a reconvert never regenerates this
// file). The one declaration it replaces is registered in the converter's manualConversionFuncs,
// which is what turns the generated body into a placeholder comment.
[module: go.GoManualConversion]

// The mirror's `fixed` buffer and its address are pointer work. Declared rather than inherited --
// see net_windows_impl.cs.
[module: go.GoRequiresUnsafe]

namespace go.@internal.syscall;

using syscall = go.syscall_package;

partial class windows_package
{
    // szCSDVersion's length as the RTL_OSVERSIONINFOW layout uses it. The converted struct's own
    // `new(128)` field initializer is not a compile-time constant, and a `fixed` buffer needs one.
    private const int csdVersionLength = 128;

    // What the record must measure for RtlGetVersion to accept the call: five uint32 (20 bytes) plus
    // 128 uint16 (256 bytes). Asserted against the mirror below rather than trusted.
    private const int osVersionInfoBytes = 276;

    // RTL_OSVERSIONINFOW exactly as ntdll lays it out: 276 bytes with szCSDVersion[128] inline.
    // `fixed` is what keeps it inline -- a C# array field would be another managed reference, which
    // is the whole bug -- so the struct is blittable and needs no marshalling layer. Every member is
    // 4- or 2-byte aligned and the scalars come first, so Sequential adds no padding anywhere and
    // the size is exactly 20 + 256.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeOsVersionInfoW
    {
        public uint32 OsVersionInfoSize;
        public uint32 MajorVersion;
        public uint32 MinorVersion;
        public uint32 BuildNumber;
        public uint32 PlatformId;
        public fixed uint16 CsdVersion[csdVersionLength];
    }

    /// <summary>
    /// Native transcription of the generated <c>rtlGetVersion</c> wrapper -- see the file header for
    /// why it cannot be a literal conversion.
    /// </summary>
    /// <remarks>
    /// Go ignores RtlGetVersion's NTSTATUS, so this does too: the record is copied back
    /// unconditionally, and a hypothetical failure leaves the caller the zeroed mirror -- which is
    /// what the generated body's unwritten box would have held as well.
    /// </remarks>
    internal static unsafe void rtlGetVersion(ж<_OSVERSIONINFOW> Ꮡinfo) {
        // Dereferenced BEFORE the call, deliberately. It is where the caller's dwOSVersionInfoSize
        // comes from, and taking it here puts a nil box's fault ahead of ntdll rather than after it,
        // which is the order Go faults in.
        ref var info = ref Ꮡinfo.Value;

        // The JIT folds `sizeof` to a constant and eliminates this branch, so the check survives
        // Release -- where a Debug.Assert would not -- at no run-time cost. A mirror whose size
        // drifted from the native record is the one failure this class cannot detect by observation:
        // it returns garbage instead of faulting, which is exactly the behaviour this file replaces.
        if (sizeof(NativeOsVersionInfoW) != osVersionInfoBytes) {
            throw new InvalidOperationException(
                $"NativeOsVersionInfoW is {sizeof(NativeOsVersionInfoW)} bytes; RTL_OSVERSIONINFOW is {osVersionInfoBytes}");
        }

        NativeOsVersionInfoW native = default;

        // dwOSVersionInfoSize is an INPUT that RtlGetVersion validates. The CALLER's value is passed
        // through unchanged rather than assumed: version() sets it from Go's `unsafe.Sizeof(info)`,
        // which the converter already folded to the native 276 (version_windows.cs), and forwarding
        // whatever the caller actually set is what keeps this a transcription of the wrapper rather
        // than a second opinion about the record.
        native.OsVersionInfoSize = info.osVersionInfoSize;

        syscall.Syscall(procRtlGetVersion.Addr(), 1, (uintptr)(void*)(&native), 0, 0);

        info.osVersionInfoSize = native.OsVersionInfoSize;
        info.majorVersion = native.MajorVersion;
        info.minorVersion = native.MinorVersion;
        info.buildNumber = native.BuildNumber;
        info.platformId = native.PlatformId;

        copyNativeCsdVersion(native.CsdVersion, ref info.csdVersion, csdVersionLength);
    }

    // Copies the native WCHAR[128] service-pack string into the converted struct's `array<uint16>`
    // field. Copied WHOLE, NULs included, for the reason the sibling companions' copies give: a Go
    // caller reads it as `UTF16ToString(info.csdVersion[:])`, which stops at the first NUL, and
    // stopping the copy at the terminator would leave stale runes behind it. The destination is
    // (re)allocated when it is not already that long, so a record that reached here as `default` --
    // its field initializer never having run -- is filled rather than dereferenced through a null
    // backing.
    private static unsafe void copyNativeCsdVersion(uint16* source, ref array<uint16> destination, nint length) {
        if (destination.Length != length) {
            destination = new array<uint16>(length);
        }

        for (nint i = 0; i < length; i++) {
            destination[i] = source[i];
        }
    }
}
