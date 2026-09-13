// zsyscall_windows_privilege_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The privilege-adjustment member of the STRUCT-PASSING class: a generated wrapper that hands the
// kernel the address of a managed struct whose fields are not where the native record wants them.
// Same seam, same remedy shape, as syscall/windows/zsyscall_windows_impl.cs (GetTimeZoneInformation,
// findFirstFile1/findNextFile1, Process32First/Next) -- read that file's header for the class.
//
// WHAT BREAKS, MEASURED rather than reasoned. os's TestDirectorySymbolicLink calls
// enableCurrentThreadPrivilege("SeCreateSymbolicLinkPrivilege"), which fills a TOKEN_PRIVILEGES and
// passes `&tp` to adjustTokenPrivileges. Native TOKEN_PRIVILEGES is 16 bytes -- a DWORD count
// followed by ONE INLINE LUID_AND_ATTRIBUTES (LUID{DWORD,LONG} + DWORD). The converted struct holds
// its Privileges as a golib `array<LUID_AND_ATTRIBUTES>`, which is a 16-byte value wrapper around a
// managed `T[]` REFERENCE plus a window (low, length), so the record is 24 bytes and the privilege
// the kernel reads is a heap pointer. A standalone probe replicating the converted emission and a
// raw-P/Invoke control in the same process, on the same thread, measured it exactly:
//
//   managed sizeof(TOKEN_PRIVILEGES) = 24                          (native = 16)
//   CONTROL ground-truth LUID           Low=0x00000023 High=0x00000000
//   CONTROL AdjustTokenPrivileges       ret=True gle=0 -- privilege GRANTED
//   CONVERTED managed tp.PrivilegeCount = 1
//   CONVERTED managed tp.Privileges[0].Luid = Low=0x00000023 High=0x00000000   <-- CORRECT
//   CONVERTED bytes at the address handed to the kernel:
//       01 00 00 00 | 00 00 00 00 | 40 01 52 bb 82 01 00 00 | 00 00 00 00 | 01 00 00 00
//       ^PrivilegeCount ^padding    ^the array<> T[] REFERENCE            ^m_low ^m_length
//   CONVERTED AdjustTokenPrivileges err = 1300 (ERROR_NOT_ALL_ASSIGNED)
//
// So the kernel read LUID.LowPart = 0xbb520140 and LUID.HighPart = 0x182 -- the low and high halves
// of a GC-heap address -- and Attributes = 0 (the array's m_low). advapi32 correctly reports that it
// could not assign a privilege nobody has, and the test reports a SKIP whose message names a host
// capability. That is the worst shape this class takes after a silent wrong answer: the failure
// blames the machine, and the machine is innocent -- Go's own suite grants the very same privilege on
// the very same box, and so does the control above.
//
// WHAT THE MEASUREMENT ALSO RULES OUT, which is why it was worth taking. The obvious rival
// explanation was that `&tp.Privileges[0].Luid` -- emitted as
// `Ꮡtp.at(TOKEN_PRIVILEGES.ᏑPrivileges, 0).of(LUID_AND_ATTRIBUTES.ᏑLuid)` -- detaches, leaving
// LookupPrivilegeValue to fill a box nobody reads and tp holding a zero LUID. It does not: the
// managed read-back above is the true LUID, byte for byte the control's. golib's FieldRefBox over
// ElemRefBox genuinely aliases the array's backing storage, and NOTHING is owed at that layer. The
// two roots are observationally identical from the error code alone -- both end in
// ERROR_NOT_ALL_ASSIGNED -- and only reading BOTH sides of the boundary separates them.
//
// THE REMEDY, and why it is smaller than the class's other members. Those transcribe into a
// [LibraryImport] of their own because the kernel WRITES a large record they must then copy back.
// Here the record is 16 bytes and mostly IN, so nothing is gained by leaving the package's own call
// machinery: the LazyProc and Syscall6 stay exactly as generated, and the only change is that the
// three pointer arguments now name blittable stack images instead of managed objects. Keeping
// Syscall6 also keeps the error semantics IDENTICAL for free -- Go's //sys line carries the `[true]`
// flag, so `err` is `errnoErr(e1)` on success as well as failure (errno 0 maps to EINVAL, which is
// precisely what the public AdjustTokenPrivileges wrapper in security_windows.cs tests for). A
// hand-rolled P/Invoke would have had to reproduce that, and could have got it wrong.
//
// THE ONE PLACE THIS IS NARROWER THAN THE NATIVE API. Go declares `Privileges [1]LUID_AND_ATTRIBUTES`
// and a caller wanting more must allocate a larger buffer and cast -- a route the conversion does not
// have. The mirror is therefore the native image of the DECLARED type, one privilege wide, and the
// count handed to the kernel is clamped to what the mirror actually carries. In Go an over-large
// PrivilegeCount on this type reads past the array too; clamping is the memory-safe reading of the
// same declaration, not a new limit.

using System;
using System.Runtime.InteropServices;

// Hand-owned (no zsyscall_windows_privilege_impl.go exists, so a reconvert never regenerates this
// file). The declaration it replaces is registered in the converter's manualConversionFuncs, which
// is what turns the generated body into a placeholder.
[module: go.GoManualConversion]

// The mirrors and their addresses are pointer work. Declared rather than inherited -- see
// net_windows_impl.cs.
[module: go.GoRequiresUnsafe]

namespace go.@internal.syscall;

using syscall = go.syscall_package;

partial class windows_package
{
    // ---- the native images, field-for-field ----------------------------------------------------
    //
    // Every field is a DWORD or LONG, so natural alignment is 4 and the records are 8 / 12 / 16
    // bytes with no padding anywhere -- the same numbers advapi32 documents.

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeLuid
    {
        public uint32 LowPart;
        public int32 HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeLuidAndAttributes
    {
        public NativeLuid Luid;
        public uint32 Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeTokenPrivileges
    {
        public uint32 PrivilegeCount;
        public NativeLuidAndAttributes Privileges;
    }

    /// <summary>
    /// Native transcription of the generated <c>adjustTokenPrivileges</c> wrapper — see the file
    /// header for why it cannot be a literal conversion.
    /// </summary>
    /// <remarks>
    /// The call itself is unchanged from the generated body (same <c>LazyProc</c>, same
    /// <c>Syscall6</c>, same <c>errnoErr(e1)</c> on every path); only the memory the three pointer
    /// arguments name is different. <c>newstate</c>, <c>prevstate</c> and <c>returnlen</c> are all
    /// optional in Go and in the native API, and each is passed as a null pointer when the caller
    /// supplies nil — which is what os's only caller does for the last two.
    /// </remarks>
    internal static unsafe (uint32 ret, error err) adjustTokenPrivileges(syscall.Token token, bool disableAllPrivileges, ж<TOKEN_PRIVILEGES> Ꮡnewstate, uint32 buflen, ж<TOKEN_PRIVILEGES> Ꮡprevstate, ж<uint32> Ꮡreturnlen) {
        uint32 _p0 = 0;
        if (disableAllPrivileges) {
            _p0 = 1;
        }

        NativeTokenPrivileges newstate = default;
        NativeTokenPrivileges prevstate = default;
        uint32 returnlen = 0;

        bool haveNewstate = Ꮡnewstate != nil;
        bool havePrevstate = Ꮡprevstate != nil;
        bool haveReturnlen = Ꮡreturnlen != nil;

        // Both mirrors are SEEDED from the caller, so the pair behaves exactly like handing the
        // kernel the caller's own memory: what advapi32 writes lands, and what it leaves alone comes
        // back unchanged. That is what makes the unconditional copy-back below correct on the
        // failure paths too — ERROR_INSUFFICIENT_BUFFER sets ReturnLength and nothing else, and a
        // hard failure sets neither.
        if (haveNewstate) {
            newstate = toNativeTokenPrivileges(Ꮡnewstate.Value);

            // The mirror is ONE privilege wide because the declared Go type is. Clamping here — at
            // the one argument the kernel READS — keeps an over-large count from walking off the end
            // of the record. In Go the same count reads past the same `[1]LUID_AND_ATTRIBUTES`, so
            // this is the memory-safe reading of the declaration rather than a narrower contract.
            if (newstate.PrivilegeCount > 1) {
                newstate.PrivilegeCount = 1;
            }
        }

        if (havePrevstate) {
            prevstate = toNativeTokenPrivileges(Ꮡprevstate.Value);
        }

        if (haveReturnlen) {
            returnlen = Ꮡreturnlen.Value;
        }

        // buflen measures the PREVIOUS-STATE buffer in bytes, and the caller's number measures a
        // MANAGED TOKEN_PRIVILEGES — 24 bytes describing a record the kernel never sees. The mirror
        // is the buffer that actually crosses, so its own size is the honest budget; a caller that
        // deliberately under-budgets to ask for ERROR_INSUFFICIENT_BUFFER plus a returnlen still
        // gets exactly that, because the smaller of the two is what goes across.
        uint32 nativeBuflen = 0;

        if (havePrevstate) {
            nativeBuflen = (uint32)sizeof(NativeTokenPrivileges);

            if (buflen < nativeBuflen) {
                nativeBuflen = buflen;
            }
        }

        uintptr newstateArg = haveNewstate ? (uintptr)(void*)(&newstate) : default;
        uintptr prevstateArg = havePrevstate ? (uintptr)(void*)(&prevstate) : default;
        uintptr returnlenArg = haveReturnlen ? (uintptr)(void*)(&returnlen) : default;

        var (r0, _, e1) = syscall.Syscall6(procAdjustTokenPrivileges.Addr(), 6, (uintptr)token, (uintptr)_p0, newstateArg, (uintptr)nativeBuflen, prevstateArg, returnlenArg);

        uint32 ret = (uint32)r0;
        error err = errnoErr(e1);

        // UNCONDITIONAL, because the mirrors were seeded: on a success (including the partial
        // ERROR_NOT_ALL_ASSIGNED, which still fills prevstate) this publishes what advapi32 wrote,
        // and on a failure it writes back the caller's own bytes unchanged. A `ret != 0` guard here
        // would look safer and be wrong — ERROR_INSUFFICIENT_BUFFER returns FALSE and still sets
        // ReturnLength, which is the whole point of asking with a short buffer.
        if (havePrevstate) {
            fromNativeTokenPrivileges(prevstate, ref Ꮡprevstate.Value);
        }

        if (haveReturnlen) {
            Ꮡreturnlen.Value = returnlen;
        }

        return (ret, err);
    }

    // Managed -> native, VERBATIM. The clamp lives at the one call site where it is load-bearing
    // (newstate, the only argument the kernel reads), so this stays a pure transcription and the
    // prevstate seeding does not quietly rewrite a count on the way in.
    private static NativeTokenPrivileges toNativeTokenPrivileges(TOKEN_PRIVILEGES value) {
        NativeTokenPrivileges native = default;

        native.PrivilegeCount = value.PrivilegeCount;

        if (value.Privileges.Length >= 1) {
            LUID_AND_ATTRIBUTES entry = value.Privileges[0];

            native.Privileges.Luid.LowPart = entry.Luid.LowPart;
            native.Privileges.Luid.HighPart = entry.Luid.HighPart;
            native.Privileges.Attributes = entry.Attributes;
        }

        return native;
    }

    // Native -> managed, for the prevstate OUT record. The count the kernel reports is preserved
    // verbatim even when it exceeds the one entry this record can hold, because it is the caller's
    // only signal that the previous state was wider than the declared type — the same information
    // Go's caller would read out of the DWORD.
    private static void fromNativeTokenPrivileges(NativeTokenPrivileges native, ref TOKEN_PRIVILEGES value) {
        value.PrivilegeCount = native.PrivilegeCount;

        if (value.Privileges.Length < 1) {
            return;
        }

        value.Privileges[0] = new LUID_AND_ATTRIBUTES{
            Luid = new LUID{
                LowPart = native.Privileges.Luid.LowPart,
                HighPart = native.Privileges.Luid.HighPart
            },
            Attributes = native.Privileges.Attributes
        };
    }
}
