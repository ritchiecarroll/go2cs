// security_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The TOKEN-INFORMATION members of the native-boundary class, in internal/syscall/windows --- the
// UNREPAIRED TWINS of members cured one package over.
//
// The class, its failure mode and its remedy are documented once, in
// syscall/windows/security_windows.cs (getInfo, GetTokenUser, GetTokenPrimaryGroup); that file is
// the reference and this one does not restate it. Its own words for the fork: not a wrapper handing
// the kernel a non-blittable struct by ADDRESS, and not a wrapper with a `**T` OUT-parameter, but a
// KERNEL BYTE BUFFER THE CALLER REINTERPRETS. No wrapper is at fault, so no mirror-the-wrapper
// remedy applies. These three members are the same fork, the same three Go lines, and the same
// measured consumer (os/user).
//
// ⚠ WHY THIS FILE EXISTS AT ALL, when the twin was cured on 2026-08: the cure was applied to the
// package whose suite reached it. `internal/syscall/windows` declares its OWN TOKEN_GROUPS,
// SID_AND_ATTRIBUTES and SID_IDENTIFIER_AUTHORITY and its OWN getTokenInfo loop, and nothing
// reached them until Go 1.24 moved os/user's group lookup onto the process token. The class was
// closed; this package's copy of it was not.
//
// WHAT WAS MEASURED HERE (i9, c5f7b4b90d and ad7795475a, at 4586b299a0):
//
//   AllGroups           `slice bounds out of range [::20] with capacity 14`. The token reports 20
//                       groups; the alias over the length-1 managed array reads 14. os/user
//                       TestGroupIds, on the 1.24 process-token path
//   GetSidIdentifierAuthority
//                       `Fatal error. System.AccessViolationException` at go.array<byte>.Clone(),
//                       through os/user isServiceAccount <- lookupUsernameAndDomain <-
//                       newUserFromSid.
//                       A process kill that ended the test host and left 494 leaves with no C# side
//
// ONE ROOT, ONE LINE APART. Both begin at security_windows.cs:201's `(ж<TOKEN_GROUPS>)(uintptr)(i)`
// and its sibling at :236 --- a kernel-filled byte buffer CAST to a managed record whose field is a
// MANAGED REFERENCE, so the first read of that field fabricates an object reference out of raw
// kernel bytes. AllGroups then reads the fabricated array's length (14) instead of the buffer's
// entries (20); GetSidIdentifierAuthority dereferences the fabrication immediately and dies. The
// alias in AllGroups is not the defect and a bigger alias would not fix it.
//
// THE REMEDY IS THE TWIN'S, PORTED, and its two halves are stated there rather than here:
//
//  1. TYPE. The machine words ARE valid PSIDs --- the layout is right and only the type is
//     wrong --- so they are read as what they are, through a [StructLayout(Sequential)] mirror,
//     and wrapped as
//     NATIVE boxes over those addresses. `syscall.SID` is Go's `struct{}`, an opaque handle managed
//     code never reads through, so a native box is not merely safe but exactly right.
//  2. LIFETIME. Windows appends the SID bytes INSIDE the buffer it filled and the Sid fields point
//     AT THEM, so the payload is self-referential and the addresses are meaningful only while the
//     buffer stays put. The buffer is therefore allocated on the PINNED OBJECT HEAP and anchored to
//     each SID that names it by a ConditionalWeakTable --- alive and still-at-that-address become
//     the same statement, and the buffer is collected exactly when Go's own would be.
//
// ⚠ AND WHY NOT COPY THE SIDs OUT, which is the reading the word "transcribe" invites: copying
// needs each SID's LENGTH, and the only way to ask is GetLengthSid --- a kernel call THROUGH the
// very address whose validity is the question. The twin's header settles it in one sentence and
// this file
// follows it: pin the buffer, do not copy. `SID_IDENTIFIER_AUTHORITY` is the one member that IS
// copied, and for the opposite reason --- it is six plain bytes with nothing self-referential in
// them, and Go returns it BY VALUE.
//
// WHAT IS DIFFERENT FROM THE TWIN, stated because the twin's file cannot be read as covering it:
// TOKEN_USER holds ONE SIDAndAttributes, TOKEN_GROUPS holds GroupCount of them in a VARIABLE-LENGTH
// trailing array. So GetTokenGroups sizes the managed `Groups` to GroupCount and transcribes every
// entry --- after which AllGroups needs no address at all, and becomes a plain managed slice over a
// correctly sized array. That is the whole of AllGroups' repair: the defect was upstream of it.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Hand-owned (no security_windows_impl.go exists, so a reconvert never regenerates this file). The
// four declarations it replaces are registered in the converter's manualConversionFuncs, which is
// what turns each generated body into a placeholder.
[module: go.GoManualConversion]

// The mirrors and the buffer walk are pointer work. Declared rather than inherited --- see
// zsyscall_windows_version_impl.cs.
[module: go.GoRequiresUnsafe]

namespace go.@internal.syscall;

using runtime = go.runtime_package;
using syscall = go.syscall_package;
using @unsafe = go.unsafe_package;
using static windows_package;

partial class windows_package
{
    // ---- the native mirrors -------------------------------------------------------------------

    // SID_AND_ATTRIBUTES exactly as Windows lays it out: a machine word where the converted record
    // holds a managed `ж<syscall.SID>` reference, then the DWORD. Declaring the mirror rather than
    // reading at hand-computed offsets is what keeps the walk right on a 32-bit target, where PSID
    // is four bytes and Attributes moves with it --- and it is what makes the stride below
    // `sizeof(NativeSIDAndAttributes)` rather than a literal 16.
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeSIDAndAttributes
    {
        public nuint Sid;
        public uint32 Attributes;
    }

    // TOKEN_GROUPS as Windows lays it out: the count, then the entries INLINE. Go declares the
    // trailing array as [1]SID_AND_ATTRIBUTES for the same reason Windows declares it
    // [ANYSIZE_ARRAY] --- it is a header for a variable-length payload --- so `Groups` here is the
    // FIRST entry and the rest follow it at the mirror's own stride. The struct's alignment places
    // Groups at the offset the kernel uses, which is why no offset appears in this file.
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeTokenGroups
    {
        public uint32 GroupCount;
        public NativeSIDAndAttributes Groups;
    }

    // SID_IDENTIFIER_AUTHORITY is six bytes and nothing else, which is the whole reason this one is
    // copied rather than pinned: there is no address inside it to keep valid.
    private const int sidIdentifierAuthorityLength = 6;

    // ---- the buffer, and the anchor that keeps it still ---------------------------------------

    // Keeps a kernel-filled token buffer alive for exactly as long as any SID that points INTO it.
    // Weak on the SID, so the buffer is collected the moment nothing can reach a SID naming it ---
    // which is precisely when Go's own buffer would become garbage. The buffer sits on the Pinned
    // Object Heap, so "alive" and "still at that address" are the same statement.
    private static readonly ConditionalWeakTable<object, object> s_tokenInfoAnchors = new();

    // Wraps a raw PSID the kernel wrote into `buffer` as the opaque native handle the rest of this
    // package already expects, and ties the buffer's lifetime to it.
    //
    // A zero address answers the nil pointer, matching Go's `(*SID)(unsafe.Pointer(uintptr(0)))`,
    // and is returned WITHOUT an anchor on purpose: nil is golib's canonical shared NilBox, so
    // anchoring a buffer to it would key every nil SID in the process to one entry.
    private static ж<syscall.SID> nativeSid(nuint address, byte[] buffer)
    {
        if (address == 0)
        {
            return default!;
        }

        var sid = (ж<syscall.SID>)(uintptr)address;
        s_tokenInfoAnchors.AddOrUpdate(sid, buffer);
        return sid;
    }

    // The converted getTokenInfo loop verbatim but for WHERE THE BUFFER COMES FROM: the Pinned
    // Object Heap, so the self-referential pointers GetTokenInformation writes into it can never go
    // stale. This one line is the whole lifetime half of the fix.
    private static (byte[], error) getTokenInfoBuffer(syscall.Token t, uint32 @class, nint initSize)
    {
        ref var n = ref heap<uint32>(out var Ꮡn);
        n = (uint32)initSize;
        while (ᐧ)
        {
            byte[] buffer = System.GC.AllocateArray<byte>((int)n, pinned: true);
            var b = new slice<byte>(buffer);
            var e = syscall.GetTokenInformation(t, @class, Ꮡ(b, 0), (uint32)len(b), Ꮡn);
            if (e == default!)
            {
                return (buffer, default!);
            }
            if (!AreEqual(e, syscall.ERROR_INSUFFICIENT_BUFFER))
            {
                return (default!, e);
            }
            if (n <= (uint32)len(b))
            {
                return (default!, e);
            }
        }
    }

    // ---- the displaced members ----------------------------------------------------------------

    // getTokenInfo retrieves a specified type of information about an access token.
    //
    // Converted signature and semantics kept; the buffer behind the returned pointer is simply the
    // non-moving one. Its only caller in this package is GetTokenGroups, which takes the buffer
    // directly below --- this body is kept correct rather than deleted because the declaration is
    // package-internal API and a future caller reaching a moving buffer is the defect this file is
    // about.
    internal static (@unsafe.Pointer, error) getTokenInfo(syscall.Token t, uint32 @class,
        nint initSize)
    {
        var (buffer, e) = getTokenInfoBuffer(t, @class, initSize);
        if (e != default!)
        {
            return (default!, e);
        }
        return (@unsafe.Pointer.FromPinnedBox(Ꮡ(new slice<byte>(buffer), 0)), default!);
    }

    // GetTokenGroups retrieves group accounts associated with access token t.
    //
    // Transcribed rather than reinterpreted: Go's `(*TOKEN_GROUPS)(i)` cannot be spelled here,
    // because the field it lands on is a managed reference and the bytes under it are machine
    // addresses (file header). The record handed back is a real managed box whose Groups array is
    // sized to the count the kernel reported and whose every Sid names the kernel's SID as the
    // native handle it is.
    //
    // The kernel's own contract sizes the buffer --- a successful GetTokenInformation has written
    // the whole TOKEN_GROUPS and its GroupCount entries --- which is the same guarantee Go's
    // unchecked cast relies on, so no length test is added that Go does not make.
    public static (ж<TOKEN_GROUPS>, error) GetTokenGroups(syscall.Token t)
    {
        var (buffer, e) = getTokenInfoBuffer(t, syscall.TokenGroups, 50);
        if (e != default!)
        {
            return (default!, e);
        }

        return (transcribeTokenGroups(buffer), default!);
    }

    /// <summary>
    /// Transcribes a kernel-filled <c>TOKEN_GROUPS</c> buffer into a managed record. Internal, and
    /// SEPARATE FROM <see cref="GetTokenGroups"/> on purpose: this is the half that can be wrong,
    /// and the half above it is a kernel call no test can make without a token.
    /// </summary>
    /// <remarks>
    /// ⚠ THE SEAM IS THE POINT. Everything this file repairs happens HERE — the count read off the
    /// buffer, the stride, the PSIDs read as machine words, the anchor — and a guard that had to
    /// open a process token to reach it would be a guard that needs a privilege, an account and a
    /// reachable domain controller. Handed a buffer it built itself, a test drives every line of
    /// the transcription on any host. The kernel call stays outside, where nothing can be wrong
    /// about it that a value could reveal.
    ///
    /// The buffer is the caller's to keep alive and to have allocated non-moving; the anchor ties
    /// it to every SID minted from it, so a caller that drops its own reference still cannot
    /// outlive the addresses.
    /// </remarks>
    // PUBLIC rather than internal so the guard can reach it without an InternalsVisibleTo grant.
    // That is not a widening of any Go surface: `internal/syscall/windows` is import-restricted by
    // Go's own rule, so nothing outside the standard library can name this package at all.
    public static unsafe ж<TOKEN_GROUPS> transcribeTokenGroups(byte[] buffer)
    {
        var Ꮡg = @new<TOKEN_GROUPS>();

        fixed (byte* p = buffer)
        {
            NativeTokenGroups* native = (NativeTokenGroups*)p;
            uint32 count = native->GroupCount;

            Ꮡg.Value.GroupCount = count;
            Ꮡg.Value.Groups = new array<SID_AND_ATTRIBUTES>((nint)count);

            // &native->Groups is the FIRST entry; the rest follow at the mirror's own stride, which
            // is what pointer arithmetic on NativeSIDAndAttributes* gives without an offset here.
            NativeSIDAndAttributes* entries = &native->Groups;

            for (uint32 i = 0; i < count; i++)
            {
                Ꮡg.Value.Groups[(nint)i] = new SID_AND_ATTRIBUTES(
                    Sid: nativeSid(entries[i].Sid, buffer),
                    Attributes: entries[i].Attributes
                );
            }
        }

        return Ꮡg;
    }

} partial class windowsᴛTOKEN_GROUPS {

    // AllGroups returns the token's groups as a slice.
    //
    // ⚠ NO ADDRESS IS TAKEN HERE, and that is the repair. Go's body reinterprets `&g.Groups[0]`
    // as a huge array and reslices it to GroupCount, which is the honest spelling over a native
    // trailing array; over the CONVERTED record it aliased a length-1 managed array and read its
    // length instead of the buffer's entries. GetTokenGroups above has already transcribed every
    // entry into a correctly sized managed array, so the slice is simply that array --- len and cap
    // both GroupCount, as Go's `[:GroupCount:GroupCount]` gives.
    [GoRecv]
    public static slice<SID_AND_ATTRIBUTES> AllGroups(this ref TOKEN_GROUPS g)
    {
        return new slice<SID_AND_ATTRIBUTES>(g.Groups);
    }

} partial class windows_package {

    // GetSidIdentifierAuthority returns the identifier authority of the given SID.
    //
    // COPIED, where the token members are pinned, and the difference is the payload: this is six
    // plain bytes with no address inside them, and Go returns the record BY VALUE. The converted
    // body dereferenced the native address as a `ж<SID_IDENTIFIER_AUTHORITY>` whose only field is
    // an `array<byte>` MANAGED REFERENCE, so `.ΔClone()` followed a reference fabricated out of the
    // first machine word of the authority --- the AccessViolationException i9 measured at
    // go.array<byte>.Clone().
    //
    // Go's own comment above these members (security_windows.go) explains the KeepAlive: the
    // returned address points INTO the SID's memory, which the Go GC does not track. It is kept, in
    // the same shape the converted body used.
    public static unsafe SID_IDENTIFIER_AUTHORITY GetSidIdentifierAuthority(ж<syscall.SID> Ꮡsid)
    {
        GoFrame ᒐ = default;
        try
        {
            defer(runtime.KeepAlive, Ꮡsid.OrTypedNil(), ref ᒐ);

            uintptr address = getSidIdentifierAuthority(Ꮡsid);

            if (address == 0)
            {
                return default!;
            }

            var value = new array<byte>(sidIdentifierAuthorityLength);
            byte* source = (byte*)(nuint)address;

            for (nint i = 0; i < sidIdentifierAuthorityLength; i++)
            {
                value[i] = source[i];
            }

            return new SID_IDENTIFIER_AUTHORITY(Value: value);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    }
}
