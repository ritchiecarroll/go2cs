// lookup_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.
//
// Parts of this file are derived from Go, whose notice they carry (license of record: LICENSES/BSD-3-Clause.txt):
//
// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// PROVENANCE, part by part:
// - lookupFullNameServer, lookupUserPrimaryGroup, listGroupsForUsernameAndDomain: DERIVED FROM
//   src/os/user/lookup_windows.go, Go 1.26.11, funcs lookupFullNameServer, lookupUserPrimaryGroup,
//   listGroupsForUsernameAndDomain. Go's statements and comments, with the netapi32 level records read out of native
//   memory.
// - copyNativeUtf16, readLocalGroupNames: original go2cs code. That read.

// Hand-written implementations of the three os/user lookups that read a netapi32 LEVEL RECORD back
// out of a buffer the API publishes: lookupFullNameServer and lookupUserPrimaryGroup from
// syscall.NetUserGetInfo, and listGroupsForUsernameAndDomain from NetUserGetLocalGroups, whose own
// fabrication route is documented at the member.
//
// WHY THESE THREE AND NOT THE FILE. Every other declaration in lookup_windows.go reads managed
// values and converts faithfully; only these three walk a native record. Hand-owning the file
// wholesale would freeze correct conversions for no gain, so the three functions are registered
// individually in the converter's manualConversionFuncs ("os/user") and supplied here -- three
// registry rows, three placeholders in lookup_windows.cs, three bodies below.
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
// -- the mirror-is-a-local doctrine the other hand-owns in this class established. In
// listGroupsForUsernameAndDomain the transcription is now the ONLY statement inside that try
// (readLocalGroupNames, at the bottom of this file), so the buffer is released BEFORE the SID
// lookups run rather than after them, and nothing past the finally reads native memory.

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
    // native stride explicitly and lifting each name is the whole fix; the group lookup below is
    // the converted body's, unchanged. The EMPTY-MEMBERSHIP answer is not: 1.24 stopped calling it
    // an error, and the split that follows from that is documented at the branch itself.
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
        err = windows.NetUserGetLocalGroups(nil, q, 0, windows.LG_INCLUDE_INDIRECT, Ꮡp0, windows.MAX_PREFERRED_LENGTH, ᏑentriesRead, ᏑtotalEntries);
        if (err != default!) {
            return (default!, err);
        }

        // THE DECISION the buffer is read by is readLocalGroupNames, below. It is a separate member
        // because it is the half of this function Go's own suite cannot reach here: TestGroupIds
        // needs an account, and row 46's runtime half was ruled unobservable on any box that cannot
        // create one. Everything the decision turns on is carried by (entriesRead, entries, domain,
        // username), so a guard can drive it with a hand-built argument tuple and no account, no
        // domain and no privilege.
        slice<@string> names = default!;

        try {
            (names, err) = readLocalGroupNames(entriesRead, (nuint)(uintptr)p0, domain, username);
        }
        finally {
            syscall.NetApiBufferFree(p0);
        }

        if (err != default!) {
            return (default!, err);
        }

        // The SID lookup stays OUTSIDE the seam, and deliberately: lookupGroupName is an API call
        // (syscall.LookupSID), so a seam that answered SIDs could only be driven against names that
        // resolve on the running host -- which is the account dependence the extraction exists to
        // remove. By here the names are copies (copyNativeUtf16 copies) and the buffer is already
        // released, so nothing below reads native memory. The loop is the converted body's.
        slice<@string> sids = default!;

        for (nint i = 0; i < len(names); i++) {
            var (sid, errΔ1) = lookupGroupName(names[i]);

            if (errΔ1 != default!) {
                return (default!, errΔ1);
            }

            sids = append(sids, sid);
        }

        return (sids, default!);
    }

    // The decision NetUserGetLocalGroups' published buffer is read by, and the whole of what this
    // hand-own decides: which of the two zero-group cases holds and, when neither does, the names
    // lifted off the native stride. `entries` is the buffer's ADDRESS rather than a typed pointer so
    // the mirror above stays private and a guard can still hand it an image it built itself.
    //
    // THE TWO CASES ARE NOT ONE CASE. Until 1.24 they shared an error, because upstream answered an
    // empty membership with an fmt.Errorf of its own and a published nil could hide inside it. 1.24
    // removed both halves of that: `entriesRead == 0` now returns nil, nil, and the paragraph that
    // justified the error -- the claim that NetUserGetLocalGroups() always returns a "None" group
    // for a user with no groups -- went with it. So:
    //
    //   entriesRead == 0   an EMPTY MEMBERSHIP, and upstream's answer for it is no error at all.
    //                      The caller ranges the nil slice zero times and appends the primary
    //                      group, which is the POSIX behaviour it wants.
    //
    //   entries == null    a PUBLISHED NIL with entries claimed to read. Upstream slices the buffer
    //                      here unconditionally and would fault; not doing that is the defect this
    //                      member was taken for. It stays an error, and it must NOT be the case
    //                      above -- answering "no groups" to a nil buffer is the silent wrong
    //                      answer, and now that the empty membership is not an error, sharing one
    //                      branch would produce it.
    //
    // The ORDER of the two tests is load-bearing and is what the guard's first two arms assert: an
    // empty membership is not an error whatever the buffer is, and a nil buffer with entries
    // CLAIMED is one. The error TEXT is asserted too -- it names entriesRead, domain and username,
    // and it is the only record a reader of a failed lookup has that the buffer, not the caller,
    // was what was wrong.
    //
    // Public for that guard (GolibTests). It widens no Go surface: Go has no such function, and
    // every converted member of this package is unchanged.
    public static unsafe (slice<@string>, error) readLocalGroupNames(uint32 entriesRead, nuint entries, @string domain, @string username) {
        NativeLocalGroupUserInfo0* records = (NativeLocalGroupUserInfo0*)entries;

        if (entriesRead == 0) {
            return (default!, default!);
        }

        if (records == null) {
            return (default!, fmt.Errorf("listGroupsForUsernameAndDomain: NetUserGetLocalGroups() published a nil buffer for %d entries for domain: %s, username: %s"u8, entriesRead, domain, username));
        }

        slice<@string> names = default!;

        for (uint32 i = 0; i < entriesRead; i++) {
            ushort* name = records[i].Name;

            if (name == null) {
                continue;
            }

            names = append(names, windows.UTF16PtrToString(copyNativeUtf16(name)));
        }

        return (names, default!);
    }
}
