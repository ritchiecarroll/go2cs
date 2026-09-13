// lookup_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementations of the two os/user lookups that read a netapi32 LEVEL RECORD back
// out of the buffer syscall.NetUserGetInfo publishes.
//
// WHY THESE TWO AND NOT THE FILE. Every other declaration in lookup_windows.go reads managed values
// and converts faithfully; only these two walk a native record. Hand-owning the file wholesale would
// freeze correct conversions for no gain, so the two functions are registered individually in the
// converter's manualConversionFuncs ("os/user") and supplied here.
//
// WHAT GOES WRONG WITHOUT THIS. syscall.NetUserGetInfo is a `**byte` out-parameter member of the
// ptrout class: it hands back a netapi32 buffer whose address the wrapper publishes into the
// caller's pointer. Go then reads that buffer as a LEVEL RECORD --
//
//     (*UserInfo10)(unsafe.Pointer(p))   // usri10_full_name
//     (*UserInfo4)(unsafe.Pointer(p))    // usri4_primary_group_id
//
// -- and the CONVERTED records carry `ж<uint16>` fields where native USER_INFO_10 / USER_INFO_4
// carry raw LPWSTRs. Reinterpreting a native-backed box keeps the address model (golib's
// Reinterpret answers a NativeBox when IsNative), so `~i` copies kernel bytes into slots the
// collector reads as OBJECT REFERENCES. That is a fabricated managed reference: a CLR type-safety
// break, and strictly worse than the nil these sites read today, which is merely wrong and
// contained. It is the same seam as net.adapterAddresses / ADDRINFOW / WSAPROTOCOL_INFOW, one
// structure smaller, and it takes the same remedy -- blittable [StructLayout(Sequential)] mirrors
// and an explicit transcription at the boundary, before anything managed reads through.
//
// A NOTE ON PrimaryGroupID, because it looks exempt and is not. lookupUserPrimaryGroup reads only a
// `uint32`, so it appears to dodge the pointer problem. It does not: `~i` materializes the WHOLE
// UserInfo4 value, fabricating every `ж<uint16>` in it on the way to the one integer -- and the
// field's OFFSET differs between the native record and the managed one anyway. Both defects are
// answered by reading through the mirror instead.
//
// OWNERSHIP. The buffer is netapi32's and is released with NetApiBufferFree. The converted bodies
// deferred that; here it is an eager finally, so the native memory never outlives the transcription
// -- the mirror-is-a-local doctrine the other hand-owns in this class established.

using System;
using System.Runtime.InteropServices;

[module: go.GoManualConversion]

// The mirrors and the addresses read through them are pointer work.
[module: go.GoRequiresUnsafe]

namespace go.os;

using windows = @internal.syscall.windows_package;
using syscall = syscall_package;
using fmt = fmt_package;

partial class user_package
{
    // ---- Native mirrors -------------------------------------------------------------------------
    //
    // Field-for-field USER_INFO_10 / USER_INFO_4 from lmaccess.h, in declaration order. Sequential
    // layout with default packing reproduces the C structs exactly on x64: every uint -> pointer
    // transition pads to the 8-byte boundary in both, so no explicit Pack or FieldOffset is owed.

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeUserInfo10
    {
        public ushort* Name;
        public ushort* Comment;
        public ushort* UsrComment;
        public ushort* FullName;
    }

    // LOCALGROUP_USERS_INFO_0: one LPWSTR, eight bytes on x64 -- and the stride the returned ARRAY
    // is indexed by. The converted LocalGroupUserInfo0 is the same single field as a `ж<uint16>`,
    // i.e. a managed reference, which is why the array cannot be walked as that record.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeLocalGroupUserInfo0
    {
        public ushort* Name;
    }

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeUserInfo4
    {
        public ushort* Name;
        public ushort* Password;
        public uint PasswordAge;
        public uint Priv;
        public ushort* HomeDir;
        public ushort* Comment;
        public uint Flags;
        public ushort* ScriptPath;
        public uint AuthFlags;
        public ushort* FullName;
        public ushort* UsrComment;
        public ushort* Parms;
        public ushort* Workstations;
        public uint LastLogon;
        public uint LastLogoff;
        public uint AcctExpires;
        public uint MaxStorage;
        public uint UnitsPerWeek;
        public byte* LogonHours;
        public uint BadPwCount;
        public uint NumLogons;
        public ushort* LogonServer;
        public uint CountryCode;
        public uint CodePage;
        public void* UserSid;
        public uint PrimaryGroupID;
        public ushort* Profile;
        public ushort* HomeDirDrive;
        public uint PasswordExpired;
    }

    // Lifts a NUL-terminated native LPWSTR into a managed, NUL-terminated array so the corpus's own
    // decoder (windows.UTF16PtrToString) reads it exactly as it reads any converted string pointer.
    // Copying rather than boxing the address is the whole point: what comes back must not alias
    // memory NetApiBufferFree is about to release. Mirrors copyNativeCanonname in
    // syscall/windows/zsyscall_windows_addrinfo_impl.cs.
    private static unsafe ж<uint16> copyNativeUtf16(ushort* source) {
        if (source == null) {
            return default!;
        }

        nint length = 0;

        while (source[length] != 0) {
            length++;
        }

        var text = new array<uint16>(length + 1);

        for (nint i = 0; i < length; i++) {
            text[i] = source[i];
        }

        return Ꮡ(text, 0);
    }

    // NetUserGetInfo level 10 -> usri10_full_name. Faithful to the converted body's control flow and
    // error values; only the READ of the returned buffer differs.
    internal static unsafe (@string, error) lookupFullNameServer(@string servername, @string username) {
        var (s, e) = syscall.UTF16PtrFromString(servername);
        if (e != default!) {
            return ("", e);
        }
        (var u, e) = syscall.UTF16PtrFromString(username);
        if (e != default!) {
            return ("", e);
        }
        ref var p = ref heap<ж<byte>>(out var Ꮡp);
        e = syscall.NetUserGetInfo(s, u, 10, Ꮡp);
        if (e != default!) {
            return ("", e);
        }

        try {
            // p is the native box the ptrout wrapper published, so its uintptr IS the netapi32
            // address -- not the managed-address answer the operator gives a heap-boxed pointer.
            NativeUserInfo10* info = (NativeUserInfo10*)(nuint)(uintptr)p;

            if (info == null) {
                return ("", default!);
            }

            return (windows.UTF16PtrToString(copyNativeUtf16(info->FullName)), default!);
        }
        finally {
            syscall.NetApiBufferFree(p);
        }
    }

    // NetUserGetInfo level 4 -> usri4_primary_group_id, formatted against the domain RID.
    internal static unsafe (@string, error) lookupUserPrimaryGroup(@string username, @string domain) {
        // get the domain RID
        var (sid, _, t, e) = syscall.LookupSID(""u8, domain);
        if (e != default!) {
            return ("", e);
        }
        if (t != syscall.SidTypeDomain) {
            return ("", fmt.Errorf("lookupUserPrimaryGroup: should be domain account type, not %d"u8, t));
        }
        (var domainRID, e) = sid.String();
        if (e != default!) {
            return ("", e);
        }
        // If the user has joined a domain use the RID of the default primary group
        // called "Domain Users":
        // https://support.microsoft.com/en-us/help/243330/well-known-security-identifiers-in-windows-operating-systems
        // SID: S-1-5-21domain-513
        //
        // The correct way to obtain the primary group of a domain user is
        // probing the user primaryGroupID attribute in the server Active Directory:
        // https://learn.microsoft.com/en-us/windows/win32/adschema/a-primarygroupid
        //
        // Note that the primary group of domain users should not be modified
        // on Windows for performance reasons, even if it's possible to do that.
        // The .NET Developer's Guide to Directory Services Programming - Page 409
        var (joined, err) = isDomainJoined();
        if (err == default! && joined) {
            return (domainRID + "-513", default!);
        }
        // For non-domain users call NetUserGetInfo() with level 4, which
        // in this case would not have any network overhead.
        // The primary group should not change from RID 513 here either
        // but the group will be called "None" instead:
        // https://www.adampalmer.me/iodigitalsec/2013/08/10/windows-null-session-enumeration/
        // "Group 'None' (RID: 513)"
        (var u, e) = syscall.UTF16PtrFromString(username);
        if (e != default!) {
            return ("", e);
        }
        (var d, e) = syscall.UTF16PtrFromString(domain);
        if (e != default!) {
            return ("", e);
        }
        ref var p = ref heap<ж<byte>>(out var Ꮡp);
        e = syscall.NetUserGetInfo(d, u, 4, Ꮡp);
        if (e != default!) {
            return ("", e);
        }

        try {
            NativeUserInfo4* info = (NativeUserInfo4*)(nuint)(uintptr)p;

            if (info == null) {
                return ("", default!);
            }

            // Read the one field through the mirror. Materializing the managed UserInfo4 to reach it
            // -- which is what the converted body's `(~i).PrimaryGroupID` did -- would fabricate a
            // managed reference for every LPWSTR in the record on the way to this integer.
            uint32 primaryGroupID = info->PrimaryGroupID;

            return (fmt.Sprintf("%s-%d"u8, domainRID, primaryGroupID), default!);
        }
        finally {
            syscall.NetApiBufferFree(p);
        }
    }

    // NetUserGetLocalGroups -> the local groups this user belongs to.
    //
    // The THIRD fabrication route in this file, and the only one that is not a Reinterpret. The
    // converted body laid a `ReadOnlySpan<LocalGroupUserInfo0>` DIRECTLY over the netapi32 buffer:
    //
    //     new slice<LocalGroupUserInfo0>(new ReadOnlySpan<LocalGroupUserInfo0>(
    //         (LocalGroupUserInfo0*)(uintptr)(new @unsafe.Pointer(p0)), (int)entriesRead))
    //
    // LocalGroupUserInfo0's single field is a `ж<uint16>`, so that span reinterprets eight raw
    // kernel bytes per element as a managed OBJECT REFERENCE -- one fabricated reference per group,
    // and `entry.Name == nil` then tests a reference the collector never handed out. Walking the
    // native stride explicitly and lifting each name is the whole fix; the group lookup and error
    // values below are the converted body's, unchanged.
    internal static unsafe (slice<@string>, error) listGroupsForUsernameAndDomain(@string username, @string domain) {
        // Check if both the domain name and user should be used.
        @string query = default!;
        var (joined, err) = isDomainJoined();
        if (err == default! && joined && len(domain) != 0) {
            query = domain + @"\"u8 + username;
        } else {
            query = username;
        }
        (var q, err) = syscall.UTF16PtrFromString(query);
        if (err != default!) {
            return (default!, err);
        }
        ref var p0 = ref heap<ж<byte>>(out var Ꮡp0);
        ref var entriesRead = ref heap(new uint32(), out var ᏑentriesRead);
        ref var totalEntries = ref heap(new uint32(), out var ᏑtotalEntries);
        // https://learn.microsoft.com/en-us/windows/win32/api/lmaccess/nf-lmaccess-netusergetlocalgroups
        // NetUserGetLocalGroups() would return a list of LocalGroupUserInfo0
        // elements which hold the names of local groups where the user participates.
        // The list does not follow any sorting order.
        //
        // If no groups can be found for this user, NetUserGetLocalGroups() should
        // always return the SID of a single group called "None", which
        // also happens to be the primary group for the local user.
        err = windows.NetUserGetLocalGroups(nil, q, 0, windows.LG_INCLUDE_INDIRECT, Ꮡp0, windows.MAX_PREFERRED_LENGTH, ᏑentriesRead, ᏑtotalEntries);
        if (err != default!) {
            return (default!, err);
        }

        try {
            NativeLocalGroupUserInfo0* entries = (NativeLocalGroupUserInfo0*)(nuint)(uintptr)p0;

            // A published nil is the defect this member was taken for, and it must not read as an
            // empty membership: it takes the same error as a genuinely empty list rather than
            // silently answering "no groups".
            if (entriesRead == 0 || entries == null) {
                return (default!, fmt.Errorf("listGroupsForUsernameAndDomain: NetUserGetLocalGroups() returned an empty list for domain: %s, username: %s"u8, domain, username));
            }

            slice<@string> sids = default!;

            for (uint32 i = 0; i < entriesRead; i++) {
                ushort* name = entries[i].Name;

                if (name == null) {
                    continue;
                }

                var (sid, errΔ1) = lookupGroupName(windows.UTF16PtrToString(copyNativeUtf16(name)));

                if (errΔ1 != default!) {
                    return (default!, errΔ1);
                }

                sids = append(sids, sid);
            }

            return (sids, default!);
        }
        finally {
            syscall.NetApiBufferFree(p0);
        }
    }
}
