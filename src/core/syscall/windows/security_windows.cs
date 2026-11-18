// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Hand-owned (whole file). EVERYTHING here is the converted security_windows.go output verbatim
// -- see security_windows.cs.auto for the generated form to diff against -- EXCEPT the three
// members at the end of the file: getInfo, GetTokenUser and GetTokenPrimaryGroup. The marker is
// whole-file because that is the granularity available without a converter change; the A/B
// footprint against the .auto sibling is deliberately confined to those three bodies plus the
// private machinery they call.
//
// THE DEFECT, and which of the Windows-syscall forks it belongs to.
//
// This is the THIRD fork of the class: not a wrapper handing the kernel a non-blittable struct by
// ADDRESS (zsyscall_windows_impl.cs), and not a wrapper with a `**T` OUT-parameter the kernel
// fills (zsyscall_windows_ptrout_impl.cs), but a KERNEL BYTE BUFFER THE CALLER REINTERPRETS. No
// wrapper is at fault, so no mirror-the-wrapper remedy applies. The same shape as
// net.adapterAddresses (closed 2026-08-17 by transcription) and crypto/x509's CertContext chain
// walk; os/user is the third measured consumer.
//
//     b := make([]byte, n)                                  // a plain byte buffer -- fine
//     GetTokenInformation(t, class, &b[0], len(b), &n)      // kernel writes TOKEN_USER into it
//     return (*Tokenuser)(unsafe.Pointer(&b[0]))            // <- the reinterpret
//
// Go's `Tokenuser` is `struct{ User SIDAndAttributes }` and `SIDAndAttributes` is
// `struct{ Sid *SID; Attributes uint32 }`. Converted, `Sid` is a golib `ж<SID>` -- a MANAGED
// OBJECT REFERENCE, not an address -- so `(ж<Tokenuser>)(uintptr)(i)` builds a NativeBox whose
// `Value` is `Unsafe.AsRef<Tokenuser>(rawAddress)`, and the first read of `.User.Sid` FABRICATES
// an object reference out of the eight raw kernel-written bytes sitting there. The fault surfaces
// two frames later, at the first use of that reference, inside ж<T>'s uintptr operator:
// `Fatal error. System.AccessViolationException` at go.ж`1[SID].op_Implicit, reached from
// syscall.ConvertSidToStringSid <- syscall.SID.String <- os/user.current. It is a process kill,
// not a catchable failure, which is why the measurement saw ZERO verdicts rather than one failure
// (docs/phase4/CENSUS-testing-osuser-rows.md, JOB 1).
//
// THE REMEDY: transcribe the payload honestly, and hold the buffer still while doing it.
//
// Two halves, and the second is the one that is easy to miss.
//
//  1. TYPE. The eight bytes ARE a valid PSID -- the LAYOUT is right and only the TYPE is wrong.
//     So they are read as what they are (a machine word, via a [StructLayout(Sequential)] native
//     mirror) and wrapped as a NATIVE box over that address. Nothing is copied and nothing is
//     interpreted: `SID` is Go's `struct{}`, an OPAQUE HANDLE that managed code never reads
//     through (SID.Len asks the OS via GetLengthSid; SID.Copy and SID.String hand the address
//     straight back to the kernel), so a native box over the raw address is not merely safe here
//     but exactly right -- the same reasoning zsyscall_windows_ptrout_impl.cs states for
//     ConvertStringSidToSid, and the reason THAT wrapper's correctness was never in question
//     while its caller was corrupt. A zero address becomes the nil pointer, as in Go.
//
//  2. LIFETIME -- the half with no analogue in the other two forks. Windows appends the SID bytes
//     INSIDE the very buffer it filled, and the Sid field points AT THEM: the payload is
//     SELF-REFERENTIAL, so the address is only meaningful while the buffer stays where the kernel
//     saw it. An ordinary managed slice backing does not stay: golib pins on address-take, but the
//     pin lives on the pointer BOX (ж.cs EnsureStableAddress), and the syscall funnel's
//     GC.KeepAlive releases the last reference to that box as soon as the call returns -- after
//     which the collector may compact the array and the kernel's self-pointer dangles. That would
//     be intermittent, GC-timing-dependent, and blamed on anything but its cause. So the buffer is
//     allocated on the PINNED OBJECT HEAP (GC.AllocateArray(..., pinned: true)), where it never
//     moves for as long as it is reachable, and its reachability is tied to the SID box that
//     points into it by a ConditionalWeakTable -- the anchor pattern
//     zsyscall_windows_addrinfo_impl.cs established for the same problem. That removes the
//     question rather than timing it, and it needs no explicit unpin: the buffer is collected
//     normally once the last SID naming it is gone, which is precisely the Go lifetime.
//
// WHY NOT COPY THE SID OUT INSTEAD. Copying would need its length, and the only way to ask is
// GetLengthSid -- i.e. a kernel call THROUGH the very address whose validity is the question.
// Pinning the buffer answers it without asking.
//
// SCOPE. Exactly the two members that reinterpret a token buffer, plus their shared feeder.
// getInfo keeps its converted signature and semantics and is correct for the scalar token classes;
// it simply now hands back a pointer to a buffer that does not move. golib's NativeBox is
// UNCHANGED -- whether ж<T>.Value should refuse a non-blittable T outright is a real hardening
// question and a separate one, recorded for the board rather than cut here.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[module: go.GoManualConversion]

// The native mirrors and the buffer transcription are pointer work. Declared rather than
// inherited -- see zsyscall_windows_impl.cs.
[module: go.GoRequiresUnsafe]

namespace go;

using @unsafe = unsafe_package;
using static syscall_package;

partial class syscall_package {

public static UntypedInt STANDARD_RIGHTS_REQUIRED => 0xf0000;
public static UntypedInt STANDARD_RIGHTS_READ => 0x20000;
public static UntypedInt STANDARD_RIGHTS_WRITE => 0x20000;
public static UntypedInt STANDARD_RIGHTS_EXECUTE => 0x20000;
public static UntypedInt STANDARD_RIGHTS_ALL => 0x1F0000;

public static UntypedInt NameUnknown => 0;
public static UntypedInt NameFullyQualifiedDN => 1;
public static UntypedInt NameSamCompatible => 2;
public static UntypedInt NameDisplay => 3;
public static UntypedInt NameUniqueId => 6;
public static UntypedInt NameCanonical => 7;
public static UntypedInt NameUserPrincipal => 8;
public static UntypedInt NameCanonicalEx => 9;
public static UntypedInt NameServicePrincipal => 10;
public static UntypedInt NameDnsDomain => 12;

// This function returns 1 byte BOOLEAN rather than the 4 byte BOOL.
// https://learn.microsoft.com/en-gb/archive/blogs/drnick/windows-and-upn-format-credentials
//sys	TranslateName(accName *uint16, accNameFormat uint32, desiredNameFormat uint32, translatedName *uint16, nSize *uint32) (err error) [failretval&0xff==0] = secur32.TranslateNameW
//sys	GetUserNameEx(nameFormat uint32, nameBuffre *uint16, nSize *uint32) (err error) [failretval&0xff==0] = secur32.GetUserNameExW

// TranslateAccountName converts a directory service
// object name from one format to another.
public static (@string, error) TranslateAccountName(@string username, uint32 from, uint32 to, nint initSize) {
    var (u, e) = UTF16PtrFromString(username);
    if (e != default!) {
        return ("", e);
    }
    ref var n = ref heap<uint32>(out var Ꮡn);
    n = (uint32)50;
    while (ᐧ) {
        var b = new slice<uint16>((nint)(n));
        e = TranslateName(u, from, to, Ꮡ(b, 0), Ꮡn);
        if (e == default!) {
            return (UTF16ToString(b[..(int)(n)]), default!);
        }
        if (!AreEqual(e, ERROR_INSUFFICIENT_BUFFER)) {
            return ("", e);
        }
        if (n <= (uint32)len(b)) {
            return ("", e);
        }
    }
}

public static UntypedInt NetSetupUnknownStatus => iota;
public static UntypedInt NetSetupUnjoined => 1;
public static UntypedInt NetSetupWorkgroupName => 2;
public static UntypedInt NetSetupDomainName => 3;

[GoType] partial struct UserInfo10 {
    public ж<uint16> Name;
    public ж<uint16> Comment;
    public ж<uint16> UsrComment;
    public ж<uint16> FullName;
}

//sys	NetUserGetInfo(serverName *uint16, userName *uint16, level uint32, buf **byte) (neterr error) = netapi32.NetUserGetInfo
//sys	NetGetJoinInformation(server *uint16, name **uint16, bufType *uint32) (neterr error) = netapi32.NetGetJoinInformation
//sys	NetApiBufferFree(buf *byte) (neterr error) = netapi32.NetApiBufferFree
public static UntypedInt SidTypeUser => /* 1 + iota */ 1;
public static UntypedInt SidTypeGroup => 2;
public static UntypedInt SidTypeDomain => 3;
public static UntypedInt SidTypeAlias => 4;
public static UntypedInt SidTypeWellKnownGroup => 5;
public static UntypedInt SidTypeDeletedAccount => 6;
public static UntypedInt SidTypeInvalid => 7;
public static UntypedInt SidTypeUnknown => 8;
public static UntypedInt SidTypeComputer => 9;
public static UntypedInt SidTypeLabel => 10;

//sys	LookupAccountSid(systemName *uint16, sid *SID, name *uint16, nameLen *uint32, refdDomainName *uint16, refdDomainNameLen *uint32, use *uint32) (err error) = advapi32.LookupAccountSidW
//sys	LookupAccountName(systemName *uint16, accountName *uint16, sid *SID, sidLen *uint32, refdDomainName *uint16, refdDomainNameLen *uint32, use *uint32) (err error) = advapi32.LookupAccountNameW
//sys	ConvertSidToStringSid(sid *SID, stringSid **uint16) (err error) = advapi32.ConvertSidToStringSidW
//sys	ConvertStringSidToSid(stringSid *uint16, sid **SID) (err error) = advapi32.ConvertStringSidToSidW
//sys	GetLengthSid(sid *SID) (len uint32) = advapi32.GetLengthSid
//sys	CopySid(destSidLen uint32, destSid *SID, srcSid *SID) (err error) = advapi32.CopySid

// The security identifier (SID) structure is a variable-length
// structure used to uniquely identify users or groups.
[GoType] public partial struct SID {
}

// StringToSid converts a string-format security identifier
// sid into a valid, functional sid.
public static (ж<SID>, error) StringToSid(@string s) {
    GoFrame ᒐ = default;
    try {
        ref var sid = ref heap<ж<SID>>(out var Ꮡsid);
        var (p, e) = UTF16PtrFromString(s);
        if (e != default!) {
            return (default!, e);
        }
        e = ConvertStringSidToSid(p, Ꮡsid);
        if (e != default!) {
            return (default!, e);
        }
        defer(LocalFree, ((Handle)(uintptr)sid), ref ᒐ);
        return sid.Copy();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// LookupSID retrieves a security identifier sid for the account
// and the name of the domain on which the account was found.
// System specify target computer to search.
public static (ж<SID> sid, @string domain, uint32 accType, error err) LookupSID(@string system, @string account) {
    ж<SID> sid = default!;
    ref var accType = ref heap(new uint32(), out var ᏑaccType);

    if (len(account) == 0) {
        return (default!, "", 0, EINVAL);
    }
    var (acc, e) = UTF16PtrFromString(account);
    if (e != default!) {
        return (default!, "", 0, e);
    }
    ж<uint16> sys = default!;
    if (len(system) > 0) {
        (sys, e) = UTF16PtrFromString(system);
        if (e != default!) {
            return (default!, "", 0, e);
        }
    }
    ref var n = ref heap<uint32>(out var Ꮡn);
    n = (uint32)50;
    ref var dn = ref heap<uint32>(out var Ꮡdn);
    dn = (uint32)50;
    while (ᐧ) {
        var b = new slice<byte>((nint)(n));
        var db = new slice<uint16>((nint)(dn));
        sid = Ꮡ(b, 0).Reinterpret<byte, SID>();
        e = LookupAccountName(sys, acc, sid, Ꮡn, Ꮡ(db, 0), Ꮡdn, ᏑaccType);
        if (e == default!) {
            return (sid, UTF16ToString(db), accType, default!);
        }
        if (!AreEqual(e, ERROR_INSUFFICIENT_BUFFER)) {
            return (default!, "", 0, e);
        }
        if (n <= (uint32)len(b)) {
            return (default!, "", 0, e);
        }
    }
}

} partial class syscallᴛSID {

// String converts sid to a string format
// suitable for display, storage, or transmission.
public static (@string, error) String(this ж<SID> Ꮡsid) {
    GoFrame ᒐ = default;
    try {
        ref var s = ref heap<ж<uint16>>(out var Ꮡs);
        var e = ConvertSidToStringSid(Ꮡsid, Ꮡs);
        if (e != default!) {
            return ("", e);
        }
        defer(LocalFree, ((Handle)(uintptr)s), ref ᒐ);
        return (utf16PtrToString(s), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Len returns the length, in bytes, of a valid security identifier sid.
public static nint Len(this ж<SID> Ꮡsid) {
    return (nint)GetLengthSid(Ꮡsid);
}

// Copy creates a duplicate of security identifier sid.
public static (ж<SID>, error) Copy(this ж<SID> Ꮡsid) {
    var b = new slice<byte>(Ꮡsid.Len());
    var sid2 = Ꮡ(b, 0).Reinterpret<byte, SID>();
    var e = CopySid((uint32)len(b), sid2, Ꮡsid);
    if (e != default!) {
        return (default!, e);
    }
    return (sid2, default!);
}

// LookupAccount retrieves the name of the account for this sid
// and the name of the first domain on which this sid is found.
// System specify target computer to search for.
public static (@string account, @string domain, uint32 accType, error err) LookupAccount(this ж<SID> Ꮡsid, @string system) {
    ref var accType = ref heap(new uint32(), out var ᏑaccType);
    error err = default!;

    ж<uint16> sys = default!;
    if (len(system) > 0) {
        (sys, err) = UTF16PtrFromString(system);
        if (err != default!) {
            return ("", "", 0, err);
        }
    }
    ref var n = ref heap<uint32>(out var Ꮡn);
    n = (uint32)50;
    ref var dn = ref heap<uint32>(out var Ꮡdn);
    dn = (uint32)50;
    while (ᐧ) {
        var b = new slice<uint16>((nint)(n));
        var db = new slice<uint16>((nint)(dn));
        var e = LookupAccountSid(sys, Ꮡsid, Ꮡ(b, 0), Ꮡn, Ꮡ(db, 0), Ꮡdn, ᏑaccType);
        if (e == default!) {
            return (UTF16ToString(b), UTF16ToString(db), accType, default!);
        }
        if (!AreEqual(e, ERROR_INSUFFICIENT_BUFFER)) {
            return ("", "", 0, e);
        }
        if (n <= (uint32)len(b)) {
            return ("", "", 0, e);
        }
    }
}

} partial class syscall_package {

public static UntypedInt TOKEN_ASSIGN_PRIMARY => /* 1 << iota */ 1;
public static UntypedInt TOKEN_DUPLICATE => 2;
public static UntypedInt TOKEN_IMPERSONATE => 4;
public static UntypedInt TOKEN_QUERY => 8;
public static UntypedInt TOKEN_QUERY_SOURCE => 16;
public static UntypedInt TOKEN_ADJUST_PRIVILEGES => 32;
public static UntypedInt TOKEN_ADJUST_GROUPS => 64;
public static UntypedInt TOKEN_ADJUST_DEFAULT => 128;
public static UntypedInt TOKEN_ADJUST_SESSIONID => 256;
public static UntypedInt TOKEN_ALL_ACCESS => /* STANDARD_RIGHTS_REQUIRED |
	TOKEN_ASSIGN_PRIMARY |
	TOKEN_DUPLICATE |
	TOKEN_IMPERSONATE |
	TOKEN_QUERY |
	TOKEN_QUERY_SOURCE |
	TOKEN_ADJUST_PRIVILEGES |
	TOKEN_ADJUST_GROUPS |
	TOKEN_ADJUST_DEFAULT |
	TOKEN_ADJUST_SESSIONID */ 983551;
public static UntypedInt TOKEN_READ => /* STANDARD_RIGHTS_READ | TOKEN_QUERY */ 131080;
public static UntypedInt TOKEN_WRITE => /* STANDARD_RIGHTS_WRITE |
	TOKEN_ADJUST_PRIVILEGES |
	TOKEN_ADJUST_GROUPS |
	TOKEN_ADJUST_DEFAULT */ 131296;
public static UntypedInt TOKEN_EXECUTE => /* STANDARD_RIGHTS_EXECUTE */ 131072;

public static UntypedInt TokenUser => /* 1 + iota */ 1;
public static UntypedInt TokenGroups => 2;
public static UntypedInt TokenPrivileges => 3;
public static UntypedInt TokenOwner => 4;
public static UntypedInt TokenPrimaryGroup => 5;
public static UntypedInt TokenDefaultDacl => 6;
public static UntypedInt TokenSource => 7;
public static UntypedInt TokenType => 8;
public static UntypedInt TokenImpersonationLevel => 9;
public static UntypedInt TokenStatistics => 10;
public static UntypedInt TokenRestrictedSids => 11;
public static UntypedInt TokenSessionId => 12;
public static UntypedInt TokenGroupsAndPrivileges => 13;
public static UntypedInt TokenSessionReference => 14;
public static UntypedInt TokenSandBoxInert => 15;
public static UntypedInt TokenAuditPolicy => 16;
public static UntypedInt TokenOrigin => 17;
public static UntypedInt TokenElevationType => 18;
public static UntypedInt TokenLinkedToken => 19;
public static UntypedInt TokenElevation => 20;
public static UntypedInt TokenHasRestrictions => 21;
public static UntypedInt TokenAccessInformation => 22;
public static UntypedInt TokenVirtualizationAllowed => 23;
public static UntypedInt TokenVirtualizationEnabled => 24;
public static UntypedInt TokenIntegrityLevel => 25;
public static UntypedInt TokenUIAccess => 26;
public static UntypedInt TokenMandatoryPolicy => 27;
public static UntypedInt TokenLogonSid => 28;
public static UntypedInt MaxTokenInfoClass => 29;

[GoType] partial struct SIDAndAttributes {
    public ж<SID> Sid;
    public uint32 Attributes;
}

[GoType] partial struct Tokenuser {
    public SIDAndAttributes User;
}

[GoType] partial struct Tokenprimarygroup {
    public ж<SID> PrimaryGroup;
}

[GoType("num:uintptr")] public partial struct Token;

//sys	OpenProcessToken(h Handle, access uint32, token *Token) (err error) = advapi32.OpenProcessToken
//sys	GetTokenInformation(t Token, infoClass uint32, info *byte, infoLen uint32, returnedLen *uint32) (err error) = advapi32.GetTokenInformation
//sys	GetUserProfileDirectory(t Token, dir *uint16, dirLen *uint32) (err error) = userenv.GetUserProfileDirectoryW

// OpenCurrentProcessToken opens the access token
// associated with current process.
public static (Token, error) OpenCurrentProcessToken() {
    var (p, e) = GetCurrentProcess();
    if (e != default!) {
        return (0, e);
    }
    ref var t = ref heap(new Token(), out var Ꮡt);
    e = OpenProcessToken(p, TOKEN_QUERY, Ꮡt);
    if (e != default!) {
        return (0, e);
    }
    return (t, default!);
}

} partial class syscallᴛToken {

// Close releases access to access token.
public static error Close(this Token t) {
    return CloseHandle(((Handle)(uintptr)t));
}

} partial class syscall_package {

// ---- the token-information transcription (hand-owned; see the file header) -------------------

// TOKEN_USER and TOKEN_PRIMARY_GROUP exactly as Windows lays them out: a machine word where the
// converted record holds a managed `ж<SID>` reference, plus -- for TOKEN_USER -- the DWORD that
// follows it. Declaring the mirrors rather than reading at hand-computed offsets is what keeps the
// transcription right on a 32-bit target, where PSID is four bytes and Attributes moves with it.
[StructLayout(LayoutKind.Sequential)]
internal struct NativeSIDAndAttributes
{
    public nuint Sid;
    public uint32 Attributes;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeTokenuser
{
    public NativeSIDAndAttributes User;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeTokenprimarygroup
{
    public nuint PrimaryGroup;
}

// Keeps a kernel-filled token buffer alive for exactly as long as the SID that points INTO it.
// Windows appends the SID bytes inside the buffer it fills, so the two have one lifetime; the
// table is weak on the SID, so the buffer is collected the moment nothing can reach the SID any
// more -- which is precisely when Go's own buffer would become garbage. The buffer sits on the
// Pinned Object Heap, so "alive" and "still at that address" are the same statement.
private static readonly ConditionalWeakTable<object, object> s_tokenInfoAnchors = new();

// Wraps a raw PSID the kernel wrote into `buffer` as the opaque native handle the rest of this
// package already expects, and ties the buffer's lifetime to it.
//
// A zero address answers the nil pointer, matching Go's `(*SID)(unsafe.Pointer(uintptr(0)))`, and
// is returned WITHOUT an anchor on purpose: nil is golib's canonical shared NilBox, so anchoring
// a buffer to it would key every nil SID in the process to one entry.
internal static ж<SID> nativeSid(nuint address, byte[] buffer) {
    if (address == 0) {
        return default!;
    }
    var sid = (ж<SID>)(uintptr)address;
    s_tokenInfoAnchors.AddOrUpdate(sid, buffer);
    return sid;
}

// The shared feeder behind getInfo and the two members below. It is the converted getInfo loop
// verbatim but for WHERE THE BUFFER COMES FROM: the Pinned Object Heap, so the self-referential
// pointer GetTokenInformation writes into it can never go stale. See the file header, remedy
// half 2 -- this one line is the whole lifetime half of the fix.
internal static (byte[], error) getTokenInfoBuffer(Token t, uint32 @class, nint initSize) {
    ref var n = ref heap<uint32>(out var Ꮡn);
    n = (uint32)initSize;
    while (ᐧ) {
        byte[] buffer = System.GC.AllocateArray<byte>((int)n, pinned: true);
        var b = new slice<byte>(buffer);
        var e = GetTokenInformation(t, @class, Ꮡ(b, 0), (uint32)len(b), Ꮡn);
        if (e == default!) {
            return (buffer, default!);
        }
        if (!AreEqual(e, ERROR_INSUFFICIENT_BUFFER)) {
            return (default!, e);
        }
        if (n <= (uint32)len(b)) {
            return (default!, e);
        }
    }
}

} partial class syscallᴛToken {

// getInfo retrieves a specified type of information about an access token.
//
// Converted signature and semantics kept; the buffer behind the returned pointer is simply the
// non-moving one. This remains the faithful shape for the token classes whose payload is plain
// scalars -- the two classes whose payload points back into its own buffer do not come through
// here at all, they take the buffer directly and transcribe it (below).
internal static (@unsafe.Pointer, error) getInfo(this Token t, uint32 @class, nint initSize) {
    var (buffer, e) = getTokenInfoBuffer(t, @class, initSize);
    if (e != default!) {
        return (default!, e);
    }
    return (new @unsafe.Pointer(Ꮡ(new slice<byte>(buffer), 0)), default!);
}

// GetTokenUser retrieves access token t user account information.
//
// Transcribed rather than reinterpreted: Go's `(*Tokenuser)(i)` cannot be spelled here, because
// the field it lands on is a managed reference and the bytes under it are a machine address (file
// header). The record handed back is a real managed box whose Sid names the kernel's SID as the
// native handle it is. The kernel's own contract sizes the buffer -- a successful
// GetTokenInformation has written the whole TOKEN_USER -- which is the same guarantee Go's
// unchecked cast relies on, so no length test is added that Go does not make.
public static unsafe (ж<Tokenuser>, error) GetTokenUser(this Token t) {
    var (buffer, e) = getTokenInfoBuffer(t, TokenUser, 50);
    if (e != default!) {
        return (default!, e);
    }
    var Ꮡu = @new<Tokenuser>();
    fixed (byte* p = buffer) {
        NativeTokenuser* native = (NativeTokenuser*)p;
        Ꮡu.Value.User.Sid = nativeSid(native->User.Sid, buffer);
        Ꮡu.Value.User.Attributes = native->User.Attributes;
    }
    return (Ꮡu, default!);
}

// GetTokenPrimaryGroup retrieves access token t primary group information.
// A pointer to a SID structure representing a group that will become
// the primary group of any objects created by a process using this access token.
//
// The same transcription as GetTokenUser, one field shorter.
public static unsafe (ж<Tokenprimarygroup>, error) GetTokenPrimaryGroup(this Token t) {
    var (buffer, e) = getTokenInfoBuffer(t, TokenPrimaryGroup, 50);
    if (e != default!) {
        return (default!, e);
    }
    var Ꮡpg = @new<Tokenprimarygroup>();
    fixed (byte* p = buffer) {
        NativeTokenprimarygroup* native = (NativeTokenprimarygroup*)p;
        Ꮡpg.Value.PrimaryGroup = nativeSid(native->PrimaryGroup, buffer);
    }
    return (Ꮡpg, default!);
}

// GetUserProfileDirectory retrieves path to the
// root directory of the access token t user's profile.
public static (@string, error) GetUserProfileDirectory(this Token t) {
    ref var n = ref heap<uint32>(out var Ꮡn);
    n = (uint32)100;
    while (ᐧ) {
        var b = new slice<uint16>((nint)(n));
        var e = syscall_package.GetUserProfileDirectory(t, Ꮡ(b, 0), Ꮡn);
        if (e == default!) {
            return (UTF16ToString(b), default!);
        }
        if (!AreEqual(e, ERROR_INSUFFICIENT_BUFFER)) {
            return ("", e);
        }
        if (n <= (uint32)len(b)) {
            return ("", e);
        }
    }
}

} partial class syscall_package {

} // end syscall_package
