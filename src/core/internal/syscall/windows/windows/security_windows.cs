// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using runtime = runtime_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class windows_package {

public static UntypedInt SecurityAnonymous => 0;
public static UntypedInt SecurityIdentification => 1;
public static UntypedInt SecurityImpersonation => 2;
public static UntypedInt SecurityDelegation => 3;

//sys	ImpersonateSelf(impersonationlevel uint32) (err error) = advapi32.ImpersonateSelf
//sys	RevertToSelf() (err error) = advapi32.RevertToSelf
//sys	ImpersonateLoggedOnUser(token syscall.Token) (err error) = advapi32.ImpersonateLoggedOnUser
//sys	LogonUser(username *uint16, domain *uint16, password *uint16, logonType uint32, logonProvider uint32, token *syscall.Token) (err error) = advapi32.LogonUserW
public static UntypedInt TOKEN_ADJUST_PRIVILEGES => 0x0020;
public static UntypedInt SE_PRIVILEGE_ENABLED => 0x00000002;

[GoType] partial struct LUID {
    public uint32 LowPart;
    public int32 HighPart;
}

[GoType] partial struct LUID_AND_ATTRIBUTES {
    public LUID Luid;
    public uint32 Attributes;
}

[GoType] partial struct TOKEN_PRIVILEGES {
    public uint32 PrivilegeCount;
    public array<LUID_AND_ATTRIBUTES> Privileges = new(1);
}

//sys	OpenThreadToken(h syscall.Handle, access uint32, openasself bool, token *syscall.Token) (err error) = advapi32.OpenThreadToken
//sys	LookupPrivilegeValue(systemname *uint16, name *uint16, luid *LUID) (err error) = advapi32.LookupPrivilegeValueW
//sys	adjustTokenPrivileges(token syscall.Token, disableAllPrivileges bool, newstate *TOKEN_PRIVILEGES, buflen uint32, prevstate *TOKEN_PRIVILEGES, returnlen *uint32) (ret uint32, err error) [true] = advapi32.AdjustTokenPrivileges
public static error AdjustTokenPrivileges(syscall.Token token, bool disableAllPrivileges, ж<TOKEN_PRIVILEGES> Ꮡnewstate, uint32 buflen, ж<TOKEN_PRIVILEGES> Ꮡprevstate, ж<uint32> Ꮡreturnlen) {
    var (ret, err) = adjustTokenPrivileges(token, disableAllPrivileges, Ꮡnewstate, buflen, Ꮡprevstate, Ꮡreturnlen);
    if (ret == 0) {
        // AdjustTokenPrivileges call failed
        return err;
    }
    // AdjustTokenPrivileges call succeeded
    if (AreEqual(err, syscall.EINVAL)) {
        // GetLastError returned ERROR_SUCCESS
        return default!;
    }
    return err;
}

//sys DuplicateTokenEx(hExistingToken syscall.Token, dwDesiredAccess uint32, lpTokenAttributes *syscall.SecurityAttributes, impersonationLevel uint32, tokenType TokenType, phNewToken *syscall.Token) (err error) = advapi32.DuplicateTokenEx
//sys SetTokenInformation(tokenHandle syscall.Token, tokenInformationClass uint32, tokenInformation uintptr, tokenInformationLength uint32) (err error) = advapi32.SetTokenInformation
[GoType] partial struct SID_AND_ATTRIBUTES {
    public ж<syscall.SID> Sid;
    public uint32 Attributes;
}

[GoType] partial struct TOKEN_MANDATORY_LABEL {
    public SID_AND_ATTRIBUTES Label;
}

[GoRecv] public static uint32 Size(this ref TOKEN_MANDATORY_LABEL tml) {
    return (uint32)/* unsafe.Sizeof(TOKEN_MANDATORY_LABEL{}) */ (uintptr)16 + syscall.GetLengthSid(tml.Label.Sid);
}

public static UntypedInt SE_GROUP_INTEGRITY => 0x00000020;

[GoType("num:uint32")] partial struct TokenType;

public static TokenType TokenPrimary => 1;
public static TokenType TokenImpersonation => 2;

//sys	GetProfilesDirectory(dir *uint16, dirLen *uint32) (err error) = userenv.GetProfilesDirectoryW
public static UntypedInt LG_INCLUDE_INDIRECT => 0x1;
public static UntypedInt MAX_PREFERRED_LENGTH => 0xFFFFFFFF;

[GoType] partial struct LocalGroupUserInfo0 {
    public ж<uint16> Name;
}

public static syscall.Errno NERR_UserNotFound => 2221;
public static syscall.Errno NERR_UserExists => 2224;

public static UntypedInt USER_PRIV_USER => 1;

[GoType] partial struct UserInfo1 {
    public ж<uint16> Name;
    public ж<uint16> Password;
    public uint32 PasswordAge;
    public uint32 Priv;
    public ж<uint16> HomeDir;
    public ж<uint16> Comment;
    public uint32 Flags;
    public ж<uint16> ScriptPath;
}

[GoType] partial struct UserInfo4 {
    public ж<uint16> Name;
    public ж<uint16> Password;
    public uint32 PasswordAge;
    public uint32 Priv;
    public ж<uint16> HomeDir;
    public ж<uint16> Comment;
    public uint32 Flags;
    public ж<uint16> ScriptPath;
    public uint32 AuthFlags;
    public ж<uint16> FullName;
    public ж<uint16> UsrComment;
    public ж<uint16> Parms;
    public ж<uint16> Workstations;
    public uint32 LastLogon;
    public uint32 LastLogoff;
    public uint32 AcctExpires;
    public uint32 MaxStorage;
    public uint32 UnitsPerWeek;
    public ж<byte> LogonHours;
    public uint32 BadPwCount;
    public uint32 NumLogons;
    public ж<uint16> LogonServer;
    public uint32 CountryCode;
    public uint32 CodePage;
    public ж<syscall.SID> UserSid;
    public uint32 PrimaryGroupID;
    public ж<uint16> Profile;
    public ж<uint16> HomeDirDrive;
    public uint32 PasswordExpired;
}

//sys	NetUserAdd(serverName *uint16, level uint32, buf *byte, parmErr *uint32) (neterr error) = netapi32.NetUserAdd
//sys	NetUserDel(serverName *uint16, userName *uint16) (neterr error) = netapi32.NetUserDel
//sys	NetUserGetLocalGroups(serverName *uint16, userName *uint16, level uint32, flags uint32, buf **byte, prefMaxLen uint32, entriesRead *uint32, totalEntries *uint32) (neterr error) = netapi32.NetUserGetLocalGroups

// GetSystemDirectory retrieves the path to current location of the system
// directory, which is typically, though not always, `C:\Windows\System32`.
//
//go:linkname GetSystemDirectory
[global::System.Diagnostics.StackTraceHidden] public static @string GetSystemDirectory() {
    return runtime.windows_GetSystemDirectory();
}

// Implemented in runtime package.

// GetUserName retrieves the user name of the current thread
// in the specified format.
public static (@string, error) GetUserName(uint32 format) {
    ref var n = ref heap<uint32>(out var Ꮡn);
    n = (uint32)50;
    while (ᐧ) {
        var b = new slice<uint16>((nint)(n));
        var e = syscall.GetUserNameEx(format, Ꮡ(b, 0), Ꮡn);
        if (e == default!) {
            return (syscall.UTF16ToString(b[..(int)(n)]), default!);
        }
        if (!AreEqual(e, syscall.ERROR_MORE_DATA)) {
            return ("", e);
        }
        if (n <= (uint32)len(b)) {
            return ("", e);
        }
    }
}

// go2cs generated this placeholder — func getTokenInfo is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

[GoType] partial struct TOKEN_GROUPS {
    public uint32 GroupCount;
    public array<SID_AND_ATTRIBUTES> Groups = new(1);
}

// go2cs generated this placeholder — func AllGroups is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func GetTokenGroups is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// https://learn.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-sid_identifier_authority
[GoType] partial struct SID_IDENTIFIER_AUTHORITY {
    public array<byte> Value = new(6);
}

public static UntypedInt SID_REVISION => 1;
public static UntypedInt SECURITY_LOCAL_SYSTEM_RID => 18;
public static UntypedInt SECURITY_LOCAL_SERVICE_RID => 19;
public static UntypedInt SECURITY_NETWORK_SERVICE_RID => 20;

public static SID_IDENTIFIER_AUTHORITY SECURITY_NT_AUTHORITY = new SID_IDENTIFIER_AUTHORITY(
    Value: new byte[]{0, 0, 0, 0, 0, 5}.array()
);

//sys	IsValidSid(sid *syscall.SID) (valid bool) = advapi32.IsValidSid
//sys	getSidIdentifierAuthority(sid *syscall.SID) (idauth uintptr) = advapi32.GetSidIdentifierAuthority
//sys	getSidSubAuthority(sid *syscall.SID, subAuthorityIdx uint32) (subAuth uintptr) = advapi32.GetSidSubAuthority
//sys	getSidSubAuthorityCount(sid *syscall.SID) (count uintptr) = advapi32.GetSidSubAuthorityCount
// The following GetSid* functions are marked as //go:nocheckptr because checkptr
// instrumentation can't see that the pointer returned by the syscall is pointing
// into the sid's memory, which is normally allocated on the Go heap. Therefore,
// the checkptr instrumentation would incorrectly flag the pointer dereference
// as pointing to an invalid allocation.
// Also, use runtime.KeepAlive to ensure that the sid is not garbage collected
// before the GetSid* functions return, as the Go GC is not aware that the
// pointers returned by the syscall are pointing into the sid's memory.
// go2cs generated this placeholder — func GetSidIdentifierAuthority is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

//go:nocheckptr
public static uint32 GetSidSubAuthority(ж<syscall.SID> Ꮡsid, uint32 subAuthorityIdx) {
    GoFrame ᒐ = default;
    try {
        defer(runtime.KeepAlive, Ꮡsid.OrTypedNil(), ref ᒐ);
        return ~(ж<uint32>)(uintptr)((@unsafe.Pointer)getSidSubAuthority(Ꮡsid, subAuthorityIdx));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

//go:nocheckptr
public static uint8 GetSidSubAuthorityCount(ж<syscall.SID> Ꮡsid) {
    GoFrame ᒐ = default;
    try {
        defer(runtime.KeepAlive, Ꮡsid.OrTypedNil(), ref ᒐ);
        return ~(ж<uint8>)(uintptr)((@unsafe.Pointer)getSidSubAuthorityCount(Ꮡsid));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

} // end windows_package
