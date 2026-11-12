// zsyscall_windows_ntfile_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// THE os.Root DOOR -- NtCreateFile and NtOpenFile, the two ntdll wrappers every os.Root operation
// on Windows goes through, and the hop-new members of the syscall STRUCT-PASSING class.
//
// The class, its failure mode and its remedy are documented once, in
// syscall/windows/zsyscall_windows_impl.cs, and restated for a second member in this directory's
// zsyscall_windows_version_impl.cs (rtlGetVersion), which is this file's precedent. The short form:
// a converted struct holding golib `array<T>` or `ж<T>` fields is a CLR record with MANAGED
// REFERENCES where the native record has a raw pointer or inline storage, so its address -- if it
// even has one -- does not name the bytes the kernel expects.
//
// THIS ONE, in numbers. OBJECT_ATTRIBUTES is 48 bytes on x64 (ULONG Length, 4 pad, HANDLE
// RootDirectory, PUNICODE_STRING ObjectName, ULONG Attributes, 4 pad, PVOID SecurityDescriptor,
// PVOID SecurityQualityOfService). That 48 is not read off documentation: the conversion computes
// it for itself and types_windows.cs:129 folds `unsafe.Sizeof(*o)` to the literal 48 inside
// OBJECT_ATTRIBUTES.init, which every caller runs before the call, and os/windows/root_windows.cs
// assigns the same 48 at its own site. The converted record (types_windows.cs:108) cannot have that
// layout: ObjectName, SecurityDescriptor and SecurityQoS are `ж<T>`, so it is an auto-layout object
// holding three managed references. Its ObjectName's pointee is reference-bearing too --
// NTUnicodeString.Buffer is `ж<uint16>` (string_windows.cs:11) -- which is the second level this
// file also owes.
//
// WHAT THE SITE DOES TODAY, and why this is a LOUD defect rather than a silent one. A
// reference-bearing pointee has no pinnable slot, so `(uintptr)` on its box answers an ORDER TOKEN
// rather than an address, and syscall/windows/dll_windows.cs's token door refuses it before the
// trampoline runs: "argument 2 is a managed pointer token, not an address ... Hand-own this wrapper
// against a blittable mirror (see zsyscall_windows_version_impl.cs)". That message names this file.
// i9 measured the consequence at row 48 -- the refusal is a panic, it takes goroutine 1 with it, and
// the 494 leaves behind TestRootConsistencyCreate have never had a C# side at all.
//
// WHY ARGUMENT 2 AND NOTHING ELSE. The other pointer arguments are blittable pointees and their
// boxes have real, pinned addresses: `handle` is ΔHandle (num:uintptr), `iosb` is IO_STATUS_BLOCK
// (NTStatus num:uint32 plus uintptr, types_windows.cs:102) and `allocationSize` is int64. The door
// fires at 2 and only at 2, which is the measurement that bounds this file's scope: the delta from
// each generated body is exactly ONE THING, the memory the third argument names.
//
// NO COPY OF THE PATH TEXT, deliberately. NTUnicodeString.Buffer points at UTF-16 the caller
// already holds, and uint16 IS blittable -- so that box has a pinnable slot and `(uintptr)` on it
// answers a real address, the same way the generated wrapper's own `ᴋNN` locals do. The mirror
// therefore stores that address and the wrapper KeepAlives the box across the call, which is the
// corpus's existing contract for a pointer argument, rather than inventing a second copy of the
// path with its own lifetime to get wrong.
//
// ONE DELIBERATE DIVERGENCE FROM NOTHING, stated so the parity is visible: the CALL is unchanged
// from each generated body -- the same LazyProc, the same syscall.Syscall12 / syscall.Syscall6
// arity and argument order, the same NTStatus conversion, the same discarded r2 and errno. Only
// the third argument's memory differs. rtlGetVersion's header gives the reason to keep the
// generated trampoline rather than reach for [LibraryImport]: Go resolves ntdll.dll through
// sysdll.Add, which pins the load to the system directory.

using System;
using System.Runtime.InteropServices;

// Hand-owned (no zsyscall_windows_ntfile_impl.go exists, so a reconvert never regenerates this
// file). The two declarations it replaces are registered in the converter's manualConversionFuncs,
// which is what turns the generated bodies into placeholders.
[module: go.GoManualConversion]

// The mirrors and the addresses taken through them are pointer work.
[module: go.GoRequiresUnsafe]

namespace go.@internal.syscall;

using syscall = go.syscall_package;

partial class windows_package
{
    // The documented native sizes, and the 48 the conversion folds for itself at
    // types_windows.cs:129. Stated so the mirrors are CHECKED at the boundary rather than assumed:
    // the failure this file exists to prevent is a silent offset.
    private const int NativeObjectAttributesSize = 48;
    private const int NativeNTUnicodeStringSize = 16;

    /// <summary>
    /// <c>UNICODE_STRING</c> exactly as ntdll lays it out: two USHORTs, four bytes of padding, and
    /// a raw <c>PWSTR</c>.
    /// </summary>
    /// <remarks>
    /// The padding is not written out: sequential layout's own alignment rule places the pointer at
    /// offset 8 on x64, which is where ntdll reads it. <c>sizeof</c> is asserted before use.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeNTUnicodeString
    {
        public uint16 Length;
        public uint16 MaximumLength;
        public nuint Buffer;
    }

    /// <summary>
    /// <c>OBJECT_ATTRIBUTES</c> exactly as ntdll lays it out: 48 bytes on x64, every pointer field
    /// a raw address.
    /// </summary>
    /// <remarks>
    /// Every field is a raw integer or address -- no <c>ж&lt;T&gt;</c>, no <c>array&lt;T&gt;</c> --
    /// so the struct is blittable and needs no marshalling layer, which is the whole point.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeObjectAttributes
    {
        public uint32 Length;
        public nuint RootDirectory;
        public nuint ObjectName;
        public uint32 Attributes;
        public nuint SecurityDescriptor;
        public nuint SecurityQualityOfService;
    }

    /// <summary>
    /// Fills <paramref name="native"/> (and the <paramref name="name"/> it points at) from the
    /// caller's <c>OBJECT_ATTRIBUTES</c>, and answers the address to hand ntdll.
    /// </summary>
    /// <remarks>
    /// Both out-parameters are the CALLER's stack locals, so everything the kernel reads lives in
    /// the calling frame and outlives the call by construction -- no pin, no anchor, no lifetime to
    /// get wrong. The two boxes returned are the ones the kernel reads THROUGH: the caller keeps
    /// them alive across the call, exactly as the generated body keeps its own pointer arguments
    /// alive.
    /// </remarks>
    private static unsafe (uintptr address, ж<NTUnicodeString> objectName, ж<uint16> text) prepareObjectAttributes(
        ж<OBJECT_ATTRIBUTES> Ꮡoa,
        NativeNTUnicodeString* name,
        NativeObjectAttributes* native)
    {
        if (sizeof(NativeObjectAttributes) != NativeObjectAttributesSize ||
            sizeof(NativeNTUnicodeString) != NativeNTUnicodeStringSize)
        {
            throw new InvalidOperationException(
                $"internal/syscall/windows: the ntdll mirrors are " +
                $"{sizeof(NativeObjectAttributes)} and {sizeof(NativeNTUnicodeString)} bytes where " +
                $"ntdll reads {NativeObjectAttributesSize} and {NativeNTUnicodeStringSize} -- " +
                "every field past the first would come from the wrong offset.");
        }

        *name = default;
        *native = default;

        // Go's own wrappers pass whatever pointer they are given, nil included; so does this one.
        if (Ꮡoa == nil)
        {
            return (default(uintptr), default!, default!);
        }

        ref OBJECT_ATTRIBUTES managed = ref Ꮡoa.Value;

        // The two fields no GOROOT caller sets. A `ж<T>` here would be an order token, and handing
        // ntdll a token is precisely the defect this file removes -- so an unexpected value is
        // REFUSED BY NAME rather than passed. Unreachable from the corpus as it stands
        // (at_windows.cs and root_windows.cs construct with nil or with ObjectName alone); it
        // exists so that a caller which starts setting one is loud on its first run instead of
        // corrupting a kernel read.
        if (managed.SecurityDescriptor != nil || managed.SecurityQoS != nil)
        {
            throw new NotSupportedException(
                "internal/syscall/windows: OBJECT_ATTRIBUTES.SecurityDescriptor and " +
                "SecurityQoS are not transcribed by this wrapper. Both are nil at every caller in " +
                "GOROOT; a caller that sets either needs its own blittable mirror here, not a " +
                "managed address handed to ntdll.");
        }

        // The CALLER's Length, not the mirror's size: Go assigns unsafe.Sizeof(*o) before the call
        // and ntdll validates it, so substituting anything else would change what the kernel is
        // asked. The two agree at 48, and the assertion above is what keeps them agreeing.
        native->Length = managed.Length;
        native->RootDirectory = (nuint)(uintptr)managed.RootDirectory;
        native->Attributes = managed.Attributes;
        native->SecurityDescriptor = 0;
        native->SecurityQualityOfService = 0;

        ж<NTUnicodeString> objectName = managed.ObjectName;

        if (objectName == nil)
        {
            native->ObjectName = 0;
            return ((uintptr)(void*)native, default!, default!);
        }

        ref NTUnicodeString managedName = ref objectName.Value;
        ж<uint16> text = managedName.Buffer;

        name->Length = managedName.Length;
        name->MaximumLength = managedName.MaximumLength;

        // uint16 is blittable, so THIS box has a pinnable slot and `(uintptr)` answers a real
        // address rather than an order token -- which is why the door fires at argument 2 and not
        // through it. The caller keeps the box alive across the call.
        name->Buffer = text == nil ? (nuint)0 : (nuint)(uintptr)text;

        native->ObjectName = (nuint)name;

        return ((uintptr)(void*)native, objectName, text);
    }

    /// <summary>
    /// <c>NtCreateFile</c> against a blittable <c>OBJECT_ATTRIBUTES</c>.
    /// </summary>
    /// <remarks>
    /// Go's signature is preserved exactly, including the <c>ntstatus</c> return. The body is the
    /// generated one with its third argument's memory replaced.
    /// </remarks>
    public static unsafe error /*ntstatus*/ NtCreateFile(ж<syscall.Handle> Ꮡhandle, uint32 access, ж<OBJECT_ATTRIBUTES> Ꮡoa, ж<IO_STATUS_BLOCK> Ꮡiosb, ж<int64> ᏑallocationSize, uint32 attributes, uint32 share, uint32 disposition, uint32 options, uintptr eabuffer, uint32 ealength) {
        error ntstatus = default!;

        NativeNTUnicodeString name = default;
        NativeObjectAttributes native = default;

        var (oa, objectName, text) = prepareObjectAttributes(Ꮡoa, &name, &native);

        var ᴋhandle = Ꮡhandle;
        var ᴋiosb = Ꮡiosb;
        var ᴋallocationSize = ᏑallocationSize;

        var (r0, _, _) = syscall.Syscall12(procNtCreateFile.Addr(), 11, (uintptr)ᴋhandle, (uintptr)access, oa, (uintptr)ᴋiosb, (uintptr)ᴋallocationSize, (uintptr)attributes, (uintptr)share, (uintptr)disposition, (uintptr)options, (uintptr)eabuffer, (uintptr)ealength, 0);

        System.GC.KeepAlive(ᴋhandle);
        System.GC.KeepAlive(ᴋiosb);
        System.GC.KeepAlive(ᴋallocationSize);
        System.GC.KeepAlive(objectName);
        System.GC.KeepAlive(text);

        if (r0 != 0) {
            ntstatus = ((NTStatus)(uint32)r0);
        }
        return ntstatus;
    }

    /// <summary>
    /// <c>NtOpenFile</c> against a blittable <c>OBJECT_ATTRIBUTES</c>.
    /// </summary>
    /// <remarks>
    /// The twin of <see cref="NtCreateFile"/>, one arity down: same mirror, same contract, same
    /// single-argument delta from the generated body.
    /// </remarks>
    public static unsafe error /*ntstatus*/ NtOpenFile(ж<syscall.Handle> Ꮡhandle, uint32 access, ж<OBJECT_ATTRIBUTES> Ꮡoa, ж<IO_STATUS_BLOCK> Ꮡiosb, uint32 share, uint32 options) {
        error ntstatus = default!;

        NativeNTUnicodeString name = default;
        NativeObjectAttributes native = default;

        var (oa, objectName, text) = prepareObjectAttributes(Ꮡoa, &name, &native);

        var ᴋhandle = Ꮡhandle;
        var ᴋiosb = Ꮡiosb;

        var (r0, _, _) = syscall.Syscall6(procNtOpenFile.Addr(), 6, (uintptr)ᴋhandle, (uintptr)access, oa, (uintptr)ᴋiosb, (uintptr)share, (uintptr)options);

        System.GC.KeepAlive(ᴋhandle);
        System.GC.KeepAlive(ᴋiosb);
        System.GC.KeepAlive(objectName);
        System.GC.KeepAlive(text);

        if (r0 != 0) {
            ntstatus = ((NTStatus)(uint32)r0);
        }
        return ntstatus;
    }
}
