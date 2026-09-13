// zsyscall_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The syscall STRUCT-PASSING class, in the shape that had no remedy until the source-retention
// seam existed: a record that reaches its wrapper as a `*byte` with its managed identity already
// discarded. Mechanism and history:
// docs/phase4/BOARD-next-validation-candidates.md, "RETRACTED — `os`'s REGRESSION is a HOST
// CAPABILITY, and the killer is SHARE_INFO_2".
//
// WHAT BREAKS. `NetShareAdd` is handed the address of a `SHARE_INFO_2`, which holds four `ж<uint16>`
// pointer fields and four `uint32`s. The CLR auto-layouts a struct containing references, so it
// groups the references FIRST: dumped by reflection from the built assembly the record is 48 bytes
// against the native 56, and every field lands somewhere it does not belong —
//
//   | native SHARE_INFO_2 (x64) | off | C# storage actually there | value handed to netapi32   |
//   |---------------------------|-----|---------------------------|----------------------------|
//   | LPWSTR shi2_netname       |   0 | Netname (object ref)      | a managed ref, read as runes|
//   | DWORD  shi2_type          |   8 | low half of Remark (nil)  | 0                          |
//   | LPWSTR shi2_remark        |  16 | Path (object ref)         | a managed reference        |
//   | DWORD  shi2_permissions   |  24 | low half of Passwd (nil)  | 0                          |
//   | DWORD  shi2_max_uses      |  28 | high half of Passwd (nil) | 0                          |
//   | DWORD  shi2_current_uses  |  32 | Type                      | 0x40000000                 |
//   | LPWSTR shi2_path          |  40 | MaxUses (=1), CurrentUses | 0x0000000000000001         |
//   | LPWSTR shi2_passwd        |  48 | past the end of the record| whatever follows on the heap|
//
// netapi32 dereferences shi2_path — the pointer value 1 — and the PROCESS dies with 0xC0000005.
// Proven without go2cs by a standalone probe: a blittable [StructLayout(Sequential)] record with
// real LPWSTRs returns rc=0 and genuinely creates the share; object references at the NATIVE
// offsets SURVIVE with rc=123 (ERROR_INVALID_NAME), because a managed reference is at least a
// readable address; only the measured go2cs layout faults. So the fault is the field REORDERING,
// not the presence of managed references.
//
// WHY THE ORDINARY REMEDY DID NOT REACH IT, AND WHAT CHANGED. Every other member of this class
// (syscall's GetTimeZoneInformation, findFirstFile1/findNextFile1, Process32First/Next, the
// sockaddr family) is repaired by hand-owning the wrapper against a blittable mirror and copying
// field-for-field at the boundary. That needs the wrapper to SEE the struct, and here it never
// does: the only caller in all of GOROOT is os's TestNetworkSymbolicLink, which writes
// `(*byte)(unsafe.Pointer(&p))`. That converts to `Ꮡp.Reinterpret<windows.SHARE_INFO_2, byte>()`,
// and `Reinterpret` correctly REFUSES to alias a reference-bearing struct as `byte`, falling back
// to the raw-address route.
//
// The address route has a recovery seam — the provenance record, which the certchain hand-own uses
// to recover its opaque extra-policy pointer (`ManagedPointerTokens.Resolve(scalar)`). It was
// MEASURED not to serve this shape, and that measurement is the reason this file changed:
// Resolve validates on read ("alive AND still pinned there"), a reference-bearing pointee has no
// pinnable storage, so no pin is taken, so IsPinnedAt is false and Resolve answers null. A probe
// with a reference-FREE control resolving on the same run is on the board entry.
//
// So the source is now remembered against the DERIVED box instead of against a number —
// `PointerExtensions.Reinterpret`'s unpinnable arm, recovered here by
// `ManagedPointerTokens.ReinterpretSource`. That is the ordinary mirror-and-copy remedy with the
// missing half restored, not a new mechanism: nothing is fabricated out of a raw address (the
// route ж.PointerExtensions.cs names a CLR type-safety break), because the real managed record is
// in hand before a single byte is transcribed.
//
// WHAT IS STILL DECLARED RATHER THAN DONE. Two paths cannot be served and say so loudly instead of
// handing netapi32 a record it would dereference:
//
//   * a level other than 2 — the buffer's SHAPE is the level, and nothing in GOROOT passes another
//     one, so guessing would be the whole defect class again one struct over; and
//   * a level-2 buffer whose source cannot be recovered — a genuinely native SHARE_INFO_2 built by
//     native code would land here, and there is no such caller in the corpus. Passing the scalar
//     through unchanged is what the certchain wrapper does for its own unrecognized scalars, and it
//     is right there because an unrecognized scalar IS an address; here it is exactly the fatal
//     path, so this one throws.
//
// THE BOUNDARY OF THE CLASS, so it is readable in place. `internal/syscall/windows` holds four more
// wrappers of the same shape. None is hand-owned here — the board's standing rule is to fix a
// censused wrapper when a suite reaches it, not speculatively — and each is repairable by the
// ORDINARY mirror remedy, because each receives the struct as a typed pointer rather than through a
// byte reinterpret:
//
//   | Wrapper                          | Non-blittable struct                                   | Reached by                                    |
//   |----------------------------------|--------------------------------------------------------|-----------------------------------------------|
//   | NetShareAdd (THIS FILE)          | SHARE_INFO_2 (Netname, Remark, Path, Passwd)             | os's TestNetworkSymbolicLink — the only caller |
//   | Module32First / Module32Next     | ModuleEntry32 (array<uint16> Module, ExePath)            | syscall's own suite                            |
//   | GetFileInformationByHandleEx     | FILE_ID_BOTH_DIR_INFO / FILE_FULL_DIR_INFO               | os's readdir — ALREADY ANSWERED, and the worked |
//   |                                  | (array<uint16> names)                                    | precedent: os/windows/dir_windows_impl.cs reads |
//   |                                  |                                                          | the kernel buffer at NATIVE offsets            |
//   | WSASendMsg / WSARecvMsg          | WSAMsg (ж<syscall.WSABuf>)                               | net's UDP OOB path                             |
//   | NetUserGetLocalGroups            | ж<ж<byte>> out-buffer                                    | os/user                                        |
//
// ⚠ GetAdaptersAddresses used to head that list and was WITHDRAWN from it on 2026-08-17: it never
// belonged. It takes a byte BUFFER (`(*IpAdapterAddresses)(unsafe.Pointer(&b[0]))`) and fills it,
// which is what a byte buffer is for, so the wrapper is correct and stays auto-converted. The defect
// was entirely in the CALLER — net.adapterAddresses walking that buffer AS the record — and it takes
// the readReparseLink / dir_windows_impl fork instead: net/windows/interface_windows_impl.cs holds
// the buffer in native memory and transcribes the whole chain. The lesson for the rows that remain is
// that "which struct is non-blittable" does not by itself say where the repair goes; who OWNS the
// memory the struct is read out of does.
//
// VERIFIED AT VALUE LEVEL, as the class demands — "it no longer crashes" proves nothing, because a
// mirror with wrong offsets returns garbage without faulting. The oracle is os's own
// TestNetworkSymbolicLink, which does not merely call this: it Stats the share through its UNC path,
// requires os.SameFile agreement with the local directory, creates a symlink INTO the share, reads
// it back and resolves it with filepath.EvalSymlinks — and its deferred NetShareDel is a t.Fatal, so
// the share must really have been created and must really be removable. The row agrees with Go on a
// host where the Server service is reachable.

using System;
using System.Runtime.InteropServices;

// Hand-owned (no zsyscall_windows_impl.go exists, so a reconvert never regenerates this file);
// the converter drops NetShareAdd from zsyscall_windows.cs via manualConversionFuncs and leaves a
// placeholder comment where its body was.
[module: go.GoManualConversion]

// The native mirror is addressed and filled through pointers. Declared rather than inherited.
[module: go.GoRequiresUnsafe]

namespace go.@internal.syscall;

using syscall = syscall_package;

partial class windows_package
{
    /// <summary>
    /// The native <c>SHARE_INFO_2</c> netapi32 reads — 56 bytes on x64, with <c>LPWSTR</c> fields as
    /// plain machine words so the CLR cannot reorder anything.
    /// </summary>
    /// <remarks>
    /// Sequential layout with natural alignment reproduces the documented offsets exactly
    /// (0, 8, 16, 24, 28, 32, 40, 48); the boundary asserts the total against the documented 56 so a
    /// future edit that perturbs the shape fails here rather than inside netapi32. A LOCAL of the
    /// wrapper, never a field: the mirror lives for the duration of one call, which is the whole of
    /// this record's documented lifetime (<c>NetShareAdd</c> copies what it needs).
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeShareInfo2
    {
        public nuint Netname;
        public uint Type;
        public nuint Remark;
        public uint Permissions;
        public uint MaxUses;
        public uint CurrentUses;
        public nuint Path;
        public nuint Passwd;
    }

    // The documented native size on x64. Stated so the mirror's own layout is checked at the
    // boundary rather than assumed — the failure this file exists to prevent is a silent offset.
    private const int NativeShareInfo2Size = 56;

    /// <summary>
    /// Adds a share, transcribing the caller's converted <c>SHARE_INFO_2</c> into the native record
    /// netapi32 expects.
    /// </summary>
    /// <remarks>
    /// Go's signature is preserved exactly (<c>neterr error</c>) and a real <c>NET_API_STATUS</c> is
    /// returned as a <c>syscall.Errno</c> the way the generated wrapper did, so Go's own caller-side
    /// handling — its <c>ERROR_ACCESS_DENIED</c> / <c>NERR_ServerNotStarted</c> skip — behaves
    /// exactly as it does under Go. The two unserviceable paths THROW rather than return an error,
    /// because they are properties of the conversion rather than runtime conditions a caller could
    /// retry around, and returning a plausible <c>NERR_*</c> would let a caller mistake one for the
    /// other.
    /// </remarks>
    public static unsafe error /*neterr*/ NetShareAdd(ж<uint16> ᏑserverName, uint32 level, ж<byte> Ꮡbuf, ж<uint16> ᏑparmErr)
    {
        if (level != 2)
        {
            throw new NotSupportedException(
                $"internal/syscall/windows: NetShareAdd level {(uint)level} is not supported by the " +
                "converted runtime — the buffer's SHAPE is the level, and only level 2 (SHARE_INFO_2) " +
                "has a transcription here because it is the only level any caller in GOROOT passes. " +
                "See docs/phase4/BOARD-next-validation-candidates.md, \"RETRACTED — os's REGRESSION " +
                "is a HOST CAPABILITY, and the killer is SHARE_INFO_2\".");
        }

        if (ManagedPointerTokens.ReinterpretSource(Ꮡbuf) is not ж<SHARE_INFO_2> Ꮡinfo)
        {
            throw new NotSupportedException(
                "internal/syscall/windows: NetShareAdd was handed a level-2 buffer whose SHARE_INFO_2 " +
                "could not be recovered. A converted `(*byte)(unsafe.Pointer(&p))` records its source " +
                "against the derived pointer (PointerExtensions.Reinterpret's unpinnable arm); a " +
                "pointer with no such record is either genuinely native or came from a route this " +
                "wrapper cannot read, and handing netapi32 the scalar as a record is the access " +
                "violation this transcription exists to prevent. See " +
                "docs/phase4/BOARD-next-validation-candidates.md, \"RETRACTED — os's REGRESSION is a " +
                "HOST CAPABILITY, and the killer is SHARE_INFO_2\".");
        }

        if (sizeof(NativeShareInfo2) != NativeShareInfo2Size)
        {
            throw new InvalidOperationException(
                $"internal/syscall/windows: the NativeShareInfo2 mirror is {sizeof(NativeShareInfo2)} " +
                $"bytes where netapi32 reads {NativeShareInfo2Size} — every field past the first would " +
                "come from the wrong offset.");
        }

        ref SHARE_INFO_2 managed = ref Ꮡinfo.Value;

        NativeShareInfo2 native = default;
        void* netname = null;
        void* remark = null;
        void* path = null;
        void* passwd = null;

        try
        {
            if (managed.Netname != nil)
            {
                netname = allocUtf16z(managed.Netname);
                native.Netname = (nuint)netname;
            }

            native.Type = (uint)managed.Type;

            if (managed.Remark != nil)
            {
                remark = allocUtf16z(managed.Remark);
                native.Remark = (nuint)remark;
            }

            native.Permissions = (uint)managed.Permissions;
            native.MaxUses = (uint)managed.MaxUses;
            native.CurrentUses = (uint)managed.CurrentUses;

            if (managed.Path != nil)
            {
                path = allocUtf16z(managed.Path);
                native.Path = (nuint)path;
            }

            if (managed.Passwd != nil)
            {
                passwd = allocUtf16z(managed.Passwd);
                native.Passwd = (nuint)passwd;
            }

            // The two remaining pointer arguments are reference-FREE pointees, so golib's address
            // route pins them and reports real storage — nothing to transcribe. They are held in
            // named locals across the call for the same reason every generated funnel call site
            // holds its pointer arguments (see syscall/windows/dll_windows.cs's soundness note).
            var ᴋ0 = ᏑserverName;
            var ᴋ1 = ᏑparmErr;

            var (r0, _, _) = syscall.Syscall6(procNetShareAdd.Addr(), 4, (uintptr)ᴋ0, (uintptr)level, (uintptr)(void*)(&native), (uintptr)ᴋ1, 0, 0);

            System.GC.KeepAlive(ᴋ0);
            System.GC.KeepAlive(ᴋ1);

            if (r0 != 0)
                return ((syscall.Errno)r0);

            return default!;
        }
        finally
        {
            // Freed here, always: SHARE_INFO_2 is input-only for NetShareAdd — netapi32 copies the
            // share's name, path and remark into its own store during the call — so nothing native
            // escapes and the share outlives these allocations.
            if (netname != null)
                NativeMemory.Free(netname);

            if (remark != null)
                NativeMemory.Free(remark);

            if (path != null)
                NativeMemory.Free(path);

            if (passwd != null)
                NativeMemory.Free(passwd);
        }
    }

    // Copies a NUL-terminated managed UTF-16 run (syscall.UTF16PtrFromString's `*uint16`, an element
    // pointer into a managed []uint16) into one native WCHAR block, terminator included. The
    // terminator is part of the data, so the length is found by walking to it — through a `fixed`,
    // which holds the backing array still for exactly the copy. Same shape as the certchain
    // hand-own's helper of the same name; duplicated rather than shared because the two live in
    // different assemblies and neither wants a public surface for it.
    private static unsafe void* allocUtf16z(ж<uint16> text)
    {
        fixed (uint16* source = &text.Value)
        {
            nint length = 0;

            while (source[length] != 0)
                length++;

            uint16* allocation = (uint16*)NativeMemory.Alloc((nuint)(length + 1) * sizeof(uint16));

            for (nint i = 0; i <= length; i++)
                allocation[i] = source[i];

            return allocation;
        }
    }
}
