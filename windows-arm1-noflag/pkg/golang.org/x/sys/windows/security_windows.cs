// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using syscall = syscall_package;
using @unsafe = unsafe_package;
using FieldOffsetAttribute = global::System.Runtime.InteropServices.FieldOffsetAttribute;
using LayoutKind = global::System.Runtime.InteropServices.LayoutKind;
using StructLayoutAttribute = global::System.Runtime.InteropServices.StructLayoutAttribute;
using go.arrays;
using go.golib;
using static windows_package;

partial class windows_package {

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

public static (@string, error) TranslateAccountName(@string username, uint32 from, uint32 to, nint initSize) {
    var (u, e) = UTF16PtrFromString(username);
    if (e != nil) {
        return ("", e);
    }
    ref var n = ref heap<uint32>((uint32)50, out var Ꮡn);
    for (;;) {
        var b = new slice<uint16>((nint)(n));
        e = TranslateName(u, from, to, Ꮡ(b, 0), Ꮡn);
        if (e == nil) {
            return (UTF16ToString(b.slice(0, (nint)(n))), default);
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

partial struct UserInfo10 {
    public ж<uint16> Name;
    public ж<uint16> Comment;
    public ж<uint16> UsrComment;
    public ж<uint16> FullName;
}

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

[global::go.golib.GoInlineLayout(Size = 6, Align = 1), StructLayout(LayoutKind.Explicit, Size = 6)] partial struct SidIdentifierAuthority {
    [FieldOffset(0)] public array6<byte> Value;
}

public static readonly ж<SidIdentifierAuthority> ᏑSECURITY_NULL_SID_AUTHORITY = new StaticBox<SidIdentifierAuthority>(new SidIdentifierAuthority(new array6<byte>(0, 0, 0, 0, 0, 0)));
public static ref SidIdentifierAuthority SECURITY_NULL_SID_AUTHORITY => ref ((StaticBox<SidIdentifierAuthority>)ᏑSECURITY_NULL_SID_AUTHORITY).ValueSlot;
public static readonly ж<SidIdentifierAuthority> ᏑSECURITY_WORLD_SID_AUTHORITY = new StaticBox<SidIdentifierAuthority>(new SidIdentifierAuthority(new array6<byte>(0, 0, 0, 0, 0, 1)));
public static ref SidIdentifierAuthority SECURITY_WORLD_SID_AUTHORITY => ref ((StaticBox<SidIdentifierAuthority>)ᏑSECURITY_WORLD_SID_AUTHORITY).ValueSlot;
public static readonly ж<SidIdentifierAuthority> ᏑSECURITY_LOCAL_SID_AUTHORITY = new StaticBox<SidIdentifierAuthority>(new SidIdentifierAuthority(new array6<byte>(0, 0, 0, 0, 0, 2)));
public static ref SidIdentifierAuthority SECURITY_LOCAL_SID_AUTHORITY => ref ((StaticBox<SidIdentifierAuthority>)ᏑSECURITY_LOCAL_SID_AUTHORITY).ValueSlot;
public static readonly ж<SidIdentifierAuthority> ᏑSECURITY_CREATOR_SID_AUTHORITY = new StaticBox<SidIdentifierAuthority>(new SidIdentifierAuthority(new array6<byte>(0, 0, 0, 0, 0, 3)));
public static ref SidIdentifierAuthority SECURITY_CREATOR_SID_AUTHORITY => ref ((StaticBox<SidIdentifierAuthority>)ᏑSECURITY_CREATOR_SID_AUTHORITY).ValueSlot;
public static readonly ж<SidIdentifierAuthority> ᏑSECURITY_NON_UNIQUE_AUTHORITY = new StaticBox<SidIdentifierAuthority>(new SidIdentifierAuthority(new array6<byte>(0, 0, 0, 0, 0, 4)));
public static ref SidIdentifierAuthority SECURITY_NON_UNIQUE_AUTHORITY => ref ((StaticBox<SidIdentifierAuthority>)ᏑSECURITY_NON_UNIQUE_AUTHORITY).ValueSlot;
public static readonly ж<SidIdentifierAuthority> ᏑSECURITY_NT_AUTHORITY = new StaticBox<SidIdentifierAuthority>(new SidIdentifierAuthority(new array6<byte>(0, 0, 0, 0, 0, 5)));
public static ref SidIdentifierAuthority SECURITY_NT_AUTHORITY => ref ((StaticBox<SidIdentifierAuthority>)ᏑSECURITY_NT_AUTHORITY).ValueSlot;
public static readonly ж<SidIdentifierAuthority> ᏑSECURITY_MANDATORY_LABEL_AUTHORITY = new StaticBox<SidIdentifierAuthority>(new SidIdentifierAuthority(new array6<byte>(0, 0, 0, 0, 0, 16)));
public static ref SidIdentifierAuthority SECURITY_MANDATORY_LABEL_AUTHORITY => ref ((StaticBox<SidIdentifierAuthority>)ᏑSECURITY_MANDATORY_LABEL_AUTHORITY).ValueSlot;

public static UntypedInt SECURITY_NULL_RID => 0;
public static UntypedInt SECURITY_WORLD_RID => 0;
public static UntypedInt SECURITY_LOCAL_RID => 0;
public static UntypedInt SECURITY_CREATOR_OWNER_RID => 0;
public static UntypedInt SECURITY_CREATOR_GROUP_RID => 1;
public static UntypedInt SECURITY_DIALUP_RID => 1;
public static UntypedInt SECURITY_NETWORK_RID => 2;
public static UntypedInt SECURITY_BATCH_RID => 3;
public static UntypedInt SECURITY_INTERACTIVE_RID => 4;
public static UntypedInt SECURITY_LOGON_IDS_RID => 5;
public static UntypedInt SECURITY_SERVICE_RID => 6;
public static UntypedInt SECURITY_LOCAL_SYSTEM_RID => 18;
public static UntypedInt SECURITY_BUILTIN_DOMAIN_RID => 32;
public static UntypedInt SECURITY_PRINCIPAL_SELF_RID => 10;
public static UntypedInt SECURITY_CREATOR_OWNER_SERVER_RID => 0x2;
public static UntypedInt SECURITY_CREATOR_GROUP_SERVER_RID => 0x3;
public static UntypedInt SECURITY_LOGON_IDS_RID_COUNT => 0x3;
public static UntypedInt SECURITY_ANONYMOUS_LOGON_RID => 0x7;
public static UntypedInt SECURITY_PROXY_RID => 0x8;
public static UntypedInt SECURITY_ENTERPRISE_CONTROLLERS_RID => 0x9;
public static UntypedInt SECURITY_SERVER_LOGON_RID => /* SECURITY_ENTERPRISE_CONTROLLERS_RID */ 9;
public static UntypedInt SECURITY_AUTHENTICATED_USER_RID => 0xb;
public static UntypedInt SECURITY_RESTRICTED_CODE_RID => 0xc;
public static UntypedInt SECURITY_NT_NON_UNIQUE_RID => 0x15;

public static UntypedInt DOMAIN_ALIAS_RID_ADMINS => 0x220;
public static UntypedInt DOMAIN_ALIAS_RID_USERS => 0x221;
public static UntypedInt DOMAIN_ALIAS_RID_GUESTS => 0x222;
public static UntypedInt DOMAIN_ALIAS_RID_POWER_USERS => 0x223;
public static UntypedInt DOMAIN_ALIAS_RID_ACCOUNT_OPS => 0x224;
public static UntypedInt DOMAIN_ALIAS_RID_SYSTEM_OPS => 0x225;
public static UntypedInt DOMAIN_ALIAS_RID_PRINT_OPS => 0x226;
public static UntypedInt DOMAIN_ALIAS_RID_BACKUP_OPS => 0x227;
public static UntypedInt DOMAIN_ALIAS_RID_REPLICATOR => 0x228;
public static UntypedInt DOMAIN_ALIAS_RID_RAS_SERVERS => 0x229;
public static UntypedInt DOMAIN_ALIAS_RID_PREW2KCOMPACCESS => 0x22a;
public static UntypedInt DOMAIN_ALIAS_RID_REMOTE_DESKTOP_USERS => 0x22b;
public static UntypedInt DOMAIN_ALIAS_RID_NETWORK_CONFIGURATION_OPS => 0x22c;
public static UntypedInt DOMAIN_ALIAS_RID_INCOMING_FOREST_TRUST_BUILDERS => 0x22d;
public static UntypedInt DOMAIN_ALIAS_RID_MONITORING_USERS => 0x22e;
public static UntypedInt DOMAIN_ALIAS_RID_LOGGING_USERS => 0x22f;
public static UntypedInt DOMAIN_ALIAS_RID_AUTHORIZATIONACCESS => 0x230;
public static UntypedInt DOMAIN_ALIAS_RID_TS_LICENSE_SERVERS => 0x231;
public static UntypedInt DOMAIN_ALIAS_RID_DCOM_USERS => 0x232;
public static UntypedInt DOMAIN_ALIAS_RID_IUSERS => 0x238;
public static UntypedInt DOMAIN_ALIAS_RID_CRYPTO_OPERATORS => 0x239;
public static UntypedInt DOMAIN_ALIAS_RID_CACHEABLE_PRINCIPALS_GROUP => 0x23b;
public static UntypedInt DOMAIN_ALIAS_RID_NON_CACHEABLE_PRINCIPALS_GROUP => 0x23c;
public static UntypedInt DOMAIN_ALIAS_RID_EVENT_LOG_READERS_GROUP => 0x23d;
public static UntypedInt DOMAIN_ALIAS_RID_CERTSVC_DCOM_ACCESS_GROUP => 0x23e;

partial struct SID {
}

public static (ж<SID>, error) StringToSid(@string s) {
    GoFrame ᒐ = default;
    try {
        ref var sid = ref heap<ж<SID>>(out var Ꮡsid);
        var (p, e) = UTF16PtrFromString(s);
        if (e != nil) {
            return (default, e);
        }
        e = ConvertStringSidToSid(p, Ꮡsid);
        if (e != nil) {
            return (default, e);
        }
        defer(LocalFree, (Handle)(uintptr)sid, ref ᒐ);
        return sid.Copy();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static (ж<SID> sid, @string domain, uint32 accType, error err) LookupSID(@string system, @string account) {
    ж<SID> sid = default;
    ref var accType = ref heap(new uint32(), out var ᏑaccType);

    if (len(account) == 0) {
        return (default, "", 0, syscall.EINVAL);
    }
    var (acc, e) = UTF16PtrFromString(account);
    if (e != nil) {
        return (default, "", 0, e);
    }
    ж<uint16> sys = default;
    if (len(system) > 0) {
        (sys, e) = UTF16PtrFromString(system);
        if (e != nil) {
            return (default, "", 0, e);
        }
    }
    ref var n = ref heap<uint32>((uint32)50, out var Ꮡn);
    ref var dn = ref heap<uint32>((uint32)50, out var Ꮡdn);
    for (;;) {
        var b = new slice<byte>((nint)(n));
        var db = new slice<uint16>((nint)(dn));
        sid = Ꮡ(b, 0).Reinterpret<byte, SID>();
        e = LookupAccountName(sys, acc, sid, Ꮡn, Ꮡ(db, 0), Ꮡdn, ᏑaccType);
        if (e == nil) {
            return (sid, UTF16ToString(db), accType, default);
        }
        if (!AreEqual(e, ERROR_INSUFFICIENT_BUFFER)) {
            return (default, "", 0, e);
        }
        if (n <= (uint32)len(b)) {
            return (default, "", 0, e);
        }
    }
}

} partial class windowsᴛSID {

public static @string String(this ж<SID> Ꮡsid) {
    GoFrame ᒐ = default;
    try {
        ref var s = ref heap<ж<uint16>>(out var Ꮡs);
        var e = ConvertSidToStringSid(Ꮡsid, Ꮡs);
        if (e != nil) {
            return "";
        }
        defer(LocalFree, (Handle)(uintptr)s, ref ᒐ);
        return UTF16ToString(array<uint16>.AliasPointer(s, 256).slice());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static nint Len(this ж<SID> Ꮡsid) {
    return (nint)GetLengthSid(Ꮡsid);
}

public static (ж<SID>, error) Copy(this ж<SID> Ꮡsid) {
    var b = new slice<byte>(Ꮡsid.Len());
    var sid2 = Ꮡ(b, 0).Reinterpret<byte, SID>();
    var e = CopySid((uint32)len(b), sid2, Ꮡsid);
    if (e != nil) {
        return (default, e);
    }
    return (sid2, default);
}

public static SidIdentifierAuthority IdentifierAuthority(this ж<SID> Ꮡsid) {
    return getSidIdentifierAuthority(Ꮡsid).Value;
}

public static uint8 SubAuthorityCount(this ж<SID> Ꮡsid) {
    return getSidSubAuthorityCount(Ꮡsid).Value;
}

public static uint32 SubAuthority(this ж<SID> Ꮡsid, uint32 idx) {
    if (idx >= (uint32)Ꮡsid.SubAuthorityCount()) {
        throw panic("sub-authority index out of range");
    }
    return getSidSubAuthority(Ꮡsid, idx).Value;
}

public static bool IsValid(this ж<SID> Ꮡsid) {
    return isValidSid(Ꮡsid);
}

public static bool ΔEquals(this ж<SID> Ꮡsid, ж<SID> sid2) {
    return EqualSid(Ꮡsid, sid2);
}

public static bool IsWellKnown(this ж<SID> Ꮡsid, WELL_KNOWN_SID_TYPE sidType) {
    return isWellKnownSid(Ꮡsid, sidType);
}

public static (@string account, @string domain, uint32 accType, error err) LookupAccount(this ж<SID> Ꮡsid, @string system) {
    ref var accType = ref heap(new uint32(), out var ᏑaccType);
    error err = default;

    ж<uint16> sys = default;
    if (len(system) > 0) {
        (sys, err) = UTF16PtrFromString(system);
        if (err != nil) {
            return ("", "", 0, err);
        }
    }
    ref var n = ref heap<uint32>((uint32)50, out var Ꮡn);
    ref var dn = ref heap<uint32>((uint32)50, out var Ꮡdn);
    for (;;) {
        var b = new slice<uint16>((nint)(n));
        var db = new slice<uint16>((nint)(dn));
        var e = LookupAccountSid(sys, Ꮡsid, Ꮡ(b, 0), Ꮡn, Ꮡ(db, 0), Ꮡdn, ᏑaccType);
        if (e == nil) {
            return (UTF16ToString(b), UTF16ToString(db), accType, default);
        }
        if (!AreEqual(e, ERROR_INSUFFICIENT_BUFFER)) {
            return ("", "", 0, e);
        }
        if (n <= (uint32)len(b)) {
            return ("", "", 0, e);
        }
    }
}

} partial class windows_package {

partial struct WELL_KNOWN_SID_TYPE /*num:uint32*/;

public static UntypedInt WinNullSid => 0;
public static UntypedInt WinWorldSid => 1;
public static UntypedInt WinLocalSid => 2;
public static UntypedInt WinCreatorOwnerSid => 3;
public static UntypedInt WinCreatorGroupSid => 4;
public static UntypedInt WinCreatorOwnerServerSid => 5;
public static UntypedInt WinCreatorGroupServerSid => 6;
public static UntypedInt WinNtAuthoritySid => 7;
public static UntypedInt WinDialupSid => 8;
public static UntypedInt WinNetworkSid => 9;
public static UntypedInt WinBatchSid => 10;
public static UntypedInt WinInteractiveSid => 11;
public static UntypedInt WinServiceSid => 12;
public static UntypedInt WinAnonymousSid => 13;
public static UntypedInt WinProxySid => 14;
public static UntypedInt WinEnterpriseControllersSid => 15;
public static UntypedInt WinSelfSid => 16;
public static UntypedInt WinAuthenticatedUserSid => 17;
public static UntypedInt WinRestrictedCodeSid => 18;
public static UntypedInt WinTerminalServerSid => 19;
public static UntypedInt WinRemoteLogonIdSid => 20;
public static UntypedInt WinLogonIdsSid => 21;
public static UntypedInt WinLocalSystemSid => 22;
public static UntypedInt WinLocalServiceSid => 23;
public static UntypedInt WinNetworkServiceSid => 24;
public static UntypedInt WinBuiltinDomainSid => 25;
public static UntypedInt WinBuiltinAdministratorsSid => 26;
public static UntypedInt WinBuiltinUsersSid => 27;
public static UntypedInt WinBuiltinGuestsSid => 28;
public static UntypedInt WinBuiltinPowerUsersSid => 29;
public static UntypedInt WinBuiltinAccountOperatorsSid => 30;
public static UntypedInt WinBuiltinSystemOperatorsSid => 31;
public static UntypedInt WinBuiltinPrintOperatorsSid => 32;
public static UntypedInt WinBuiltinBackupOperatorsSid => 33;
public static UntypedInt WinBuiltinReplicatorSid => 34;
public static UntypedInt WinBuiltinPreWindows2000CompatibleAccessSid => 35;
public static UntypedInt WinBuiltinRemoteDesktopUsersSid => 36;
public static UntypedInt WinBuiltinNetworkConfigurationOperatorsSid => 37;
public static UntypedInt WinAccountAdministratorSid => 38;
public static UntypedInt WinAccountGuestSid => 39;
public static UntypedInt WinAccountKrbtgtSid => 40;
public static UntypedInt WinAccountDomainAdminsSid => 41;
public static UntypedInt WinAccountDomainUsersSid => 42;
public static UntypedInt WinAccountDomainGuestsSid => 43;
public static UntypedInt WinAccountComputersSid => 44;
public static UntypedInt WinAccountControllersSid => 45;
public static UntypedInt WinAccountCertAdminsSid => 46;
public static UntypedInt WinAccountSchemaAdminsSid => 47;
public static UntypedInt WinAccountEnterpriseAdminsSid => 48;
public static UntypedInt WinAccountPolicyAdminsSid => 49;
public static UntypedInt WinAccountRasAndIasServersSid => 50;
public static UntypedInt WinNTLMAuthenticationSid => 51;
public static UntypedInt WinDigestAuthenticationSid => 52;
public static UntypedInt WinSChannelAuthenticationSid => 53;
public static UntypedInt WinThisOrganizationSid => 54;
public static UntypedInt WinOtherOrganizationSid => 55;
public static UntypedInt WinBuiltinIncomingForestTrustBuildersSid => 56;
public static UntypedInt WinBuiltinPerfMonitoringUsersSid => 57;
public static UntypedInt WinBuiltinPerfLoggingUsersSid => 58;
public static UntypedInt WinBuiltinAuthorizationAccessSid => 59;
public static UntypedInt WinBuiltinTerminalServerLicenseServersSid => 60;
public static UntypedInt WinBuiltinDCOMUsersSid => 61;
public static UntypedInt WinBuiltinIUsersSid => 62;
public static UntypedInt WinIUserSid => 63;
public static UntypedInt WinBuiltinCryptoOperatorsSid => 64;
public static UntypedInt WinUntrustedLabelSid => 65;
public static UntypedInt WinLowLabelSid => 66;
public static UntypedInt WinMediumLabelSid => 67;
public static UntypedInt WinHighLabelSid => 68;
public static UntypedInt WinSystemLabelSid => 69;
public static UntypedInt WinWriteRestrictedCodeSid => 70;
public static UntypedInt WinCreatorOwnerRightsSid => 71;
public static UntypedInt WinCacheablePrincipalsGroupSid => 72;
public static UntypedInt WinNonCacheablePrincipalsGroupSid => 73;
public static UntypedInt WinEnterpriseReadonlyControllersSid => 74;
public static UntypedInt WinAccountReadonlyControllersSid => 75;
public static UntypedInt WinBuiltinEventLogReadersGroup => 76;
public static UntypedInt WinNewEnterpriseReadonlyControllersSid => 77;
public static UntypedInt WinBuiltinCertSvcDComAccessGroup => 78;
public static UntypedInt WinMediumPlusLabelSid => 79;
public static UntypedInt WinLocalLogonSid => 80;
public static UntypedInt WinConsoleLogonSid => 81;
public static UntypedInt WinThisOrganizationCertificateSid => 82;
public static UntypedInt WinApplicationPackageAuthoritySid => 83;
public static UntypedInt WinBuiltinAnyPackageSid => 84;
public static UntypedInt WinCapabilityInternetClientSid => 85;
public static UntypedInt WinCapabilityInternetClientServerSid => 86;
public static UntypedInt WinCapabilityPrivateNetworkClientServerSid => 87;
public static UntypedInt WinCapabilityPicturesLibrarySid => 88;
public static UntypedInt WinCapabilityVideosLibrarySid => 89;
public static UntypedInt WinCapabilityMusicLibrarySid => 90;
public static UntypedInt WinCapabilityDocumentsLibrarySid => 91;
public static UntypedInt WinCapabilitySharedUserCertificatesSid => 92;
public static UntypedInt WinCapabilityEnterpriseAuthenticationSid => 93;
public static UntypedInt WinCapabilityRemovableStorageSid => 94;
public static UntypedInt WinBuiltinRDSRemoteAccessServersSid => 95;
public static UntypedInt WinBuiltinRDSEndpointServersSid => 96;
public static UntypedInt WinBuiltinRDSManagementServersSid => 97;
public static UntypedInt WinUserModeDriversSid => 98;
public static UntypedInt WinBuiltinHyperVAdminsSid => 99;
public static UntypedInt WinAccountCloneableControllersSid => 100;
public static UntypedInt WinBuiltinAccessControlAssistanceOperatorsSid => 101;
public static UntypedInt WinBuiltinRemoteManagementUsersSid => 102;
public static UntypedInt WinAuthenticationAuthorityAssertedSid => 103;
public static UntypedInt WinAuthenticationServiceAssertedSid => 104;
public static UntypedInt WinLocalAccountSid => 105;
public static UntypedInt WinLocalAccountAndAdministratorSid => 106;
public static UntypedInt WinAccountProtectedUsersSid => 107;
public static UntypedInt WinCapabilityAppointmentsSid => 108;
public static UntypedInt WinCapabilityContactsSid => 109;
public static UntypedInt WinAccountDefaultSystemManagedSid => 110;
public static UntypedInt WinBuiltinDefaultSystemManagedGroupSid => 111;
public static UntypedInt WinBuiltinStorageReplicaAdminsSid => 112;
public static UntypedInt WinAccountKeyAdminsSid => 113;
public static UntypedInt WinAccountEnterpriseKeyAdminsSid => 114;
public static UntypedInt WinAuthenticationKeyTrustSid => 115;
public static UntypedInt WinAuthenticationKeyPropertyMFASid => 116;
public static UntypedInt WinAuthenticationKeyPropertyAttestationSid => 117;
public static UntypedInt WinAuthenticationFreshKeyAuthSid => 118;
public static UntypedInt WinBuiltinDeviceOwnersSid => 119;

public static (ж<SID>, error) CreateWellKnownSid(WELL_KNOWN_SID_TYPE sidType) {
    return CreateWellKnownDomainSid(sidType, nil);
}

public static (ж<SID>, error) CreateWellKnownDomainSid(WELL_KNOWN_SID_TYPE sidType, ж<SID> domainSid) {
    ref var n = ref heap<uint32>((uint32)50, out var Ꮡn);
    for (;;) {
        var b = new slice<byte>((nint)(n));
        var sid = Ꮡ(b, 0).Reinterpret<byte, SID>();
        var err = createWellKnownSid(sidType, domainSid, sid, Ꮡn);
        if (err == nil) {
            return (sid, default);
        }
        if (!AreEqual(err, ERROR_INSUFFICIENT_BUFFER)) {
            return (default, err);
        }
        if (n <= (uint32)len(b)) {
            return (default, err);
        }
    }
}

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

public static UntypedInt SE_GROUP_MANDATORY => 0x00000001;
public static UntypedInt SE_GROUP_ENABLED_BY_DEFAULT => 0x00000002;
public static UntypedInt SE_GROUP_ENABLED => 0x00000004;
public static UntypedInt SE_GROUP_OWNER => 0x00000008;
public static UntypedInt SE_GROUP_USE_FOR_DENY_ONLY => 0x00000010;
public static UntypedInt SE_GROUP_INTEGRITY => 0x00000020;
public static UntypedInt SE_GROUP_INTEGRITY_ENABLED => 0x00000040;
public static UntypedInt SE_GROUP_LOGON_ID => 0xC0000000;
public static UntypedInt SE_GROUP_RESOURCE => 0x20000000;
public static UntypedInt SE_GROUP_VALID_ATTRIBUTES => /* SE_GROUP_MANDATORY | SE_GROUP_ENABLED_BY_DEFAULT | SE_GROUP_ENABLED | SE_GROUP_OWNER | SE_GROUP_USE_FOR_DENY_ONLY | SE_GROUP_LOGON_ID | SE_GROUP_RESOURCE | SE_GROUP_INTEGRITY | SE_GROUP_INTEGRITY_ENABLED */ 3758096511;

public static UntypedInt SE_PRIVILEGE_ENABLED_BY_DEFAULT => 0x00000001;
public static UntypedInt SE_PRIVILEGE_ENABLED => 0x00000002;
public static UntypedInt SE_PRIVILEGE_REMOVED => 0x00000004;
public static UntypedInt SE_PRIVILEGE_USED_FOR_ACCESS => 0x80000000;
public static UntypedInt SE_PRIVILEGE_VALID_ATTRIBUTES => /* SE_PRIVILEGE_ENABLED_BY_DEFAULT | SE_PRIVILEGE_ENABLED | SE_PRIVILEGE_REMOVED | SE_PRIVILEGE_USED_FOR_ACCESS */ 2147483655;

public static UntypedInt TokenPrimary => 1;
public static UntypedInt TokenImpersonation => 2;

public static UntypedInt SecurityAnonymous => 0;
public static UntypedInt SecurityIdentification => 1;
public static UntypedInt SecurityImpersonation => 2;
public static UntypedInt SecurityDelegation => 3;

partial struct LUID {
    public uint32 LowPart;
    public int32 HighPart;
}

partial struct LUIDAndAttributes {
    public LUID Luid;
    public uint32 Attributes;
}

partial struct SIDAndAttributes {
    public ж<SID> Sid;
    public uint32 Attributes;
}

partial struct Tokenuser {
    public SIDAndAttributes User;
}

partial struct Tokenprimarygroup {
    public ж<SID> PrimaryGroup;
}

partial struct Tokengroups {
    public uint32 GroupCount;
    public array<SIDAndAttributes> Groups = new(1);
}

} partial class windowsᴛTokengroups {

public static slice<SIDAndAttributes> AllGroups(this ref Tokengroups g) {
    return array<SIDAndAttributes>.AliasPointer(Ꮡ(g.Groups, 0), 268435455).slice(0, (nint)(g.GroupCount), (nint)(g.GroupCount));
}

} partial class windows_package {

partial struct Tokenprivileges {
    public uint32 PrivilegeCount;
    public array<LUIDAndAttributes> Privileges = new(1);
}

} partial class windowsᴛTokenprivileges {

public static slice<LUIDAndAttributes> AllPrivileges(this ref Tokenprivileges p) {
    return array<LUIDAndAttributes>.AliasPointer(Ꮡ(p.Privileges, 0), 134217727).slice(0, (nint)(p.PrivilegeCount), (nint)(p.PrivilegeCount));
}

} partial class windows_package {

partial struct Tokenmandatorylabel {
    public SIDAndAttributes Label;
}

} partial class windowsᴛTokenmandatorylabel {

public static uint32 Size(this ref Tokenmandatorylabel tml) {
    return (uint32)/* unsafe.Sizeof(Tokenmandatorylabel{}) */ (uintptr)16 + GetLengthSid(tml.Label.Sid);
}

} partial class windows_package {

partial struct Token /*num:uintptr*/;

public static (Token, error) OpenCurrentProcessToken() {
    ref var token = ref heap(new Token(), out var Ꮡtoken);
    var err = OpenProcessToken(CurrentProcess(), TOKEN_QUERY, Ꮡtoken);
    return (token, err);
}

public static Token GetCurrentProcessToken() {
    return (Token)(~(uintptr)(4 - 1));
}

public static Token GetCurrentThreadToken() {
    return (Token)(~(uintptr)(5 - 1));
}

public static Token GetCurrentThreadEffectiveToken() {
    return (Token)(~(uintptr)(6 - 1));
}

} partial class windowsᴛToken {

public static error Close(this Token t) {
    return CloseHandle((Handle)(uintptr)t);
}

internal static (@unsafe.Pointer, error) getInfo(this Token t, uint32 @class, nint initSize) {
    ref var n = ref heap<uint32>((uint32)initSize, out var Ꮡn);
    for (;;) {
        var b = new slice<byte>((nint)(n));
        var e = GetTokenInformation(t, @class, Ꮡ(b, 0), (uint32)len(b), Ꮡn);
        if (e == nil) {
            return (@unsafe.Pointer.FromPinnedBox(Ꮡ(b, 0)), default);
        }
        if (!AreEqual(e, ERROR_INSUFFICIENT_BUFFER)) {
            return (default, e);
        }
        if (n <= (uint32)len(b)) {
            return (default, e);
        }
    }
}

public static (ж<Tokenuser>, error) GetTokenUser(this Token t) {
    var (i, e) = t.getInfo(TokenUser, 50);
    if (e != nil) {
        return (default, e);
    }
    return (@unsafe.Pointer.To<Tokenuser>(i), default);
}

public static (ж<Tokengroups>, error) GetTokenGroups(this Token t) {
    var (i, e) = t.getInfo(TokenGroups, 50);
    if (e != nil) {
        return (default, e);
    }
    return (@unsafe.Pointer.To<Tokengroups>(i), default);
}

public static (ж<Tokenprimarygroup>, error) GetTokenPrimaryGroup(this Token t) {
    var (i, e) = t.getInfo(TokenPrimaryGroup, 50);
    if (e != nil) {
        return (default, e);
    }
    return (@unsafe.Pointer.To<Tokenprimarygroup>(i), default);
}

public static (@string, error) GetUserProfileDirectory(this Token t) {
    ref var n = ref heap<uint32>((uint32)100, out var Ꮡn);
    for (;;) {
        var b = new slice<uint16>((nint)(n));
        var e = windows_package.GetUserProfileDirectory(t, Ꮡ(b, 0), Ꮡn);
        if (e == nil) {
            return (UTF16ToString(b), default);
        }
        if (!AreEqual(e, ERROR_INSUFFICIENT_BUFFER)) {
            return ("", e);
        }
        if (n <= (uint32)len(b)) {
            return ("", e);
        }
    }
}

public static bool IsElevated(this Token token) {
    ref var isElevated = ref heap(new uint32(), out var ᏑisElevated);
    ref var outLen = ref heap(new uint32(), out var ᏑoutLen);
    var err = GetTokenInformation(token, TokenElevation, ᏑisElevated.Reinterpret<uint32, byte>(), (uint32)/* unsafe.Sizeof(isElevated) */ (uintptr)4, ᏑoutLen);
    if (err != nil) {
        return false;
    }
    return outLen == (uint32)/* unsafe.Sizeof(isElevated) */ (uintptr)4 && isElevated != 0;
}

public static (Token, error) GetLinkedToken(this Token token) {
    ref var linkedToken = ref heap(new Token(), out var ᏑlinkedToken);
    ref var outLen = ref heap(new uint32(), out var ᏑoutLen);
    var err = GetTokenInformation(token, TokenLinkedToken, ᏑlinkedToken.Reinterpret<Token, byte>(), (uint32)/* unsafe.Sizeof(linkedToken) */ (uintptr)8, ᏑoutLen);
    if (err != nil) {
        return ((Token)0, err);
    }
    return (linkedToken, default);
}

} partial class windows_package {

public static (@string, error) GetSystemDirectory() {
    var n = (uint32)MAX_PATH;
    for (;;) {
        var b = new slice<uint16>((nint)(n));
        var (l, e) = getSystemDirectory(Ꮡ(b, 0), n);
        if (e != nil) {
            return ("", e);
        }
        if (l <= n) {
            return (UTF16ToString(b.slice(0, (nint)(l))), default);
        }
        n = l;
    }
}

public static (@string, error) GetWindowsDirectory() {
    var n = (uint32)MAX_PATH;
    for (;;) {
        var b = new slice<uint16>((nint)(n));
        var (l, e) = getWindowsDirectory(Ꮡ(b, 0), n);
        if (e != nil) {
            return ("", e);
        }
        if (l <= n) {
            return (UTF16ToString(b.slice(0, (nint)(l))), default);
        }
        n = l;
    }
}

public static (@string, error) GetSystemWindowsDirectory() {
    var n = (uint32)MAX_PATH;
    for (;;) {
        var b = new slice<uint16>((nint)(n));
        var (l, e) = getSystemWindowsDirectory(Ꮡ(b, 0), n);
        if (e != nil) {
            return ("", e);
        }
        if (l <= n) {
            return (UTF16ToString(b.slice(0, (nint)(l))), default);
        }
        n = l;
    }
}

} partial class windowsᴛToken {

public static (bool, error) IsMember(this Token t, ж<SID> sid) {
    ref var b = ref heap(new int32(), out var Ꮡb);
    var e = checkTokenMembership(t, sid, Ꮡb); if (e != nil) {
        return (false, e);
    }
    return (b != 0, default);
}

public static (bool isRestricted, error err) IsRestricted(this Token t) {
    bool isRestricted = default;
    error err = default;

    (isRestricted, err) = isTokenRestricted(t);
    if (!isRestricted && AreEqual(err, syscall.EINVAL)) {
        err = default;
    }
    return (isRestricted, err);
}

} partial class windows_package {

public static UntypedInt WTS_CONSOLE_CONNECT => 0x1;
public static UntypedInt WTS_CONSOLE_DISCONNECT => 0x2;
public static UntypedInt WTS_REMOTE_CONNECT => 0x3;
public static UntypedInt WTS_REMOTE_DISCONNECT => 0x4;
public static UntypedInt WTS_SESSION_LOGON => 0x5;
public static UntypedInt WTS_SESSION_LOGOFF => 0x6;
public static UntypedInt WTS_SESSION_LOCK => 0x7;
public static UntypedInt WTS_SESSION_UNLOCK => 0x8;
public static UntypedInt WTS_SESSION_REMOTE_CONTROL => 0x9;
public static UntypedInt WTS_SESSION_CREATE => 0xa;
public static UntypedInt WTS_SESSION_TERMINATE => 0xb;

public static UntypedInt WTSActive => 0;
public static UntypedInt WTSConnected => 1;
public static UntypedInt WTSConnectQuery => 2;
public static UntypedInt WTSShadow => 3;
public static UntypedInt WTSDisconnected => 4;
public static UntypedInt WTSIdle => 5;
public static UntypedInt WTSListen => 6;
public static UntypedInt WTSReset => 7;
public static UntypedInt WTSDown => 8;
public static UntypedInt WTSInit => 9;

partial struct WTSSESSION_NOTIFICATION {
    public uint32 Size;
    public uint32 SessionID;
}

partial struct WTS_SESSION_INFO {
    public uint32 SessionID;
    public ж<uint16> WindowStationName;
    public uint32 State;
}

partial struct ACL {
    internal byte aclRevision;
    internal byte sbz1;
    internal uint16 aclSize;
    public uint16 AceCount;
    internal uint16 sbz2;
}

partial struct SECURITY_DESCRIPTOR {
    internal byte revision;
    internal byte sbz1;
    internal SECURITY_DESCRIPTOR_CONTROL control;
    internal ж<SID> owner;
    internal ж<SID> group;
    internal ж<ACL> sacl;
    internal ж<ACL> dacl;
}

partial struct SECURITY_QUALITY_OF_SERVICE {
    public uint32 Length;
    public uint32 ImpersonationLevel;
    public byte ContextTrackingMode;
    public byte EffectiveOnly;
}

public static UntypedInt SECURITY_STATIC_TRACKING => 0;
public static UntypedInt SECURITY_DYNAMIC_TRACKING => 1;

partial struct SecurityAttributes {
    public uint32 Length;
    public ж<SECURITY_DESCRIPTOR> SecurityDescriptor;
    public uint32 InheritHandle;
}

partial struct SE_OBJECT_TYPE /*num:uint32*/;

public static UntypedInt SE_UNKNOWN_OBJECT_TYPE => 0;
public static UntypedInt SE_FILE_OBJECT => 1;
public static UntypedInt SE_SERVICE => 2;
public static UntypedInt SE_PRINTER => 3;
public static UntypedInt SE_REGISTRY_KEY => 4;
public static UntypedInt SE_LMSHARE => 5;
public static UntypedInt SE_KERNEL_OBJECT => 6;
public static UntypedInt SE_WINDOW_OBJECT => 7;
public static UntypedInt SE_DS_OBJECT => 8;
public static UntypedInt SE_DS_OBJECT_ALL => 9;
public static UntypedInt SE_PROVIDER_DEFINED_OBJECT => 10;
public static UntypedInt SE_WMIGUID_OBJECT => 11;
public static UntypedInt SE_REGISTRY_WOW64_32KEY => 12;
public static UntypedInt SE_REGISTRY_WOW64_64KEY => 13;

partial struct SECURITY_INFORMATION /*num:uint32*/;

public static UntypedInt OWNER_SECURITY_INFORMATION => 0x00000001;
public static UntypedInt GROUP_SECURITY_INFORMATION => 0x00000002;
public static UntypedInt DACL_SECURITY_INFORMATION => 0x00000004;
public static UntypedInt SACL_SECURITY_INFORMATION => 0x00000008;
public static UntypedInt LABEL_SECURITY_INFORMATION => 0x00000010;
public static UntypedInt ATTRIBUTE_SECURITY_INFORMATION => 0x00000020;
public static UntypedInt SCOPE_SECURITY_INFORMATION => 0x00000040;
public static UntypedInt BACKUP_SECURITY_INFORMATION => 0x00010000;
public static UntypedInt PROTECTED_DACL_SECURITY_INFORMATION => 0x80000000;
public static UntypedInt PROTECTED_SACL_SECURITY_INFORMATION => 0x40000000;
public static UntypedInt UNPROTECTED_DACL_SECURITY_INFORMATION => 0x20000000;
public static UntypedInt UNPROTECTED_SACL_SECURITY_INFORMATION => 0x10000000;

partial struct SECURITY_DESCRIPTOR_CONTROL /*num:uint16*/;

public static UntypedInt SE_OWNER_DEFAULTED => 0x0001;
public static UntypedInt SE_GROUP_DEFAULTED => 0x0002;
public static UntypedInt SE_DACL_PRESENT => 0x0004;
public static UntypedInt SE_DACL_DEFAULTED => 0x0008;
public static UntypedInt SE_SACL_PRESENT => 0x0010;
public static UntypedInt SE_SACL_DEFAULTED => 0x0020;
public static UntypedInt SE_DACL_AUTO_INHERIT_REQ => 0x0100;
public static UntypedInt SE_SACL_AUTO_INHERIT_REQ => 0x0200;
public static UntypedInt SE_DACL_AUTO_INHERITED => 0x0400;
public static UntypedInt SE_SACL_AUTO_INHERITED => 0x0800;
public static UntypedInt SE_DACL_PROTECTED => 0x1000;
public static UntypedInt SE_SACL_PROTECTED => 0x2000;
public static UntypedInt SE_RM_CONTROL_VALID => 0x4000;
public static UntypedInt SE_SELF_RELATIVE => 0x8000;

partial struct ACCESS_MASK /*num:uint32*/;

public static UntypedInt DELETE => 0x00010000;
public static UntypedInt READ_CONTROL => 0x00020000;
public static UntypedInt WRITE_DAC => 0x00040000;
public static UntypedInt WRITE_OWNER => 0x00080000;
public static UntypedInt SYNCHRONIZE => 0x00100000;
public static UntypedInt STANDARD_RIGHTS_REQUIRED => 0x000F0000;
public static UntypedInt STANDARD_RIGHTS_READ => /* READ_CONTROL */ 131072;
public static UntypedInt STANDARD_RIGHTS_WRITE => /* READ_CONTROL */ 131072;
public static UntypedInt STANDARD_RIGHTS_EXECUTE => /* READ_CONTROL */ 131072;
public static UntypedInt STANDARD_RIGHTS_ALL => 0x001F0000;
public static UntypedInt SPECIFIC_RIGHTS_ALL => 0x0000FFFF;
public static UntypedInt ACCESS_SYSTEM_SECURITY => 0x01000000;
public static UntypedInt MAXIMUM_ALLOWED => 0x02000000;
public static UntypedInt GENERIC_READ => 0x80000000;
public static UntypedInt GENERIC_WRITE => 0x40000000;
public static UntypedInt GENERIC_EXECUTE => 0x20000000;
public static UntypedInt GENERIC_ALL => 0x10000000;

partial struct ACCESS_MODE /*num:uint32*/;

public static UntypedInt NOT_USED_ACCESS => 0;
public static UntypedInt GRANT_ACCESS => 1;
public static UntypedInt SET_ACCESS => 2;
public static UntypedInt DENY_ACCESS => 3;
public static UntypedInt REVOKE_ACCESS => 4;
public static UntypedInt SET_AUDIT_SUCCESS => 5;
public static UntypedInt SET_AUDIT_FAILURE => 6;

public static UntypedInt NO_INHERITANCE => 0x0;
public static UntypedInt SUB_OBJECTS_ONLY_INHERIT => 0x1;
public static UntypedInt SUB_CONTAINERS_ONLY_INHERIT => 0x2;
public static UntypedInt SUB_CONTAINERS_AND_OBJECTS_INHERIT => 0x3;
public static UntypedInt INHERIT_NO_PROPAGATE => 0x4;
public static UntypedInt INHERIT_ONLY => 0x8;
public static UntypedInt INHERITED_ACCESS_ENTRY => 0x10;
public static UntypedInt INHERITED_PARENT => 0x10000000;
public static UntypedInt INHERITED_GRANDPARENT => 0x20000000;
public static UntypedInt OBJECT_INHERIT_ACE => 0x1;
public static UntypedInt CONTAINER_INHERIT_ACE => 0x2;
public static UntypedInt NO_PROPAGATE_INHERIT_ACE => 0x4;
public static UntypedInt INHERIT_ONLY_ACE => 0x8;
public static UntypedInt INHERITED_ACE => 0x10;
public static UntypedInt VALID_INHERIT_FLAGS => 0x1F;

partial struct MULTIPLE_TRUSTEE_OPERATION /*num:uint32*/;

public static UntypedInt NO_MULTIPLE_TRUSTEE => 0;
public static UntypedInt TRUSTEE_IS_IMPERSONATE => 1;

partial struct TRUSTEE_FORM /*num:uint32*/;

public static UntypedInt TRUSTEE_IS_SID => 0;
public static UntypedInt TRUSTEE_IS_NAME => 1;
public static UntypedInt TRUSTEE_BAD_FORM => 2;
public static UntypedInt TRUSTEE_IS_OBJECTS_AND_SID => 3;
public static UntypedInt TRUSTEE_IS_OBJECTS_AND_NAME => 4;

partial struct TRUSTEE_TYPE /*num:uint32*/;

public static UntypedInt TRUSTEE_IS_UNKNOWN => 0;
public static UntypedInt TRUSTEE_IS_USER => 1;
public static UntypedInt TRUSTEE_IS_GROUP => 2;
public static UntypedInt TRUSTEE_IS_DOMAIN => 3;
public static UntypedInt TRUSTEE_IS_ALIAS => 4;
public static UntypedInt TRUSTEE_IS_WELL_KNOWN_GROUP => 5;
public static UntypedInt TRUSTEE_IS_DELETED => 6;
public static UntypedInt TRUSTEE_IS_INVALID => 7;
public static UntypedInt TRUSTEE_IS_COMPUTER => 8;

public static UntypedInt ACE_OBJECT_TYPE_PRESENT => 0x1;
public static UntypedInt ACE_INHERITED_OBJECT_TYPE_PRESENT => 0x2;

partial struct EXPLICIT_ACCESS {
    public ACCESS_MASK AccessPermissions;
    public ACCESS_MODE AccessMode;
    public uint32 Inheritance;
    public TRUSTEE Trustee;
}

partial struct ACE_HEADER {
    public uint8 AceType;
    public uint8 AceFlags;
    public uint16 AceSize;
}

partial struct ACCESS_ALLOWED_ACE {
    public ACE_HEADER Header;
    public ACCESS_MASK Mask;
    public uint32 SidStart;
}

public static UntypedInt ACCESS_ALLOWED_ACE_TYPE => 0;
public static UntypedInt ACCESS_DENIED_ACE_TYPE => 1;

partial struct TrusteeValue /*num:uintptr*/;

public static TrusteeValue TrusteeValueFromString(@string str) {
    return (TrusteeValue)(uintptr)StringToUTF16Ptr(str);
}

public static TrusteeValue TrusteeValueFromSID(ж<SID> sid) {
    return (TrusteeValue)(uintptr)sid;
}

public static TrusteeValue TrusteeValueFromObjectsAndSid(ж<OBJECTS_AND_SID> objectsAndSid) {
    return (TrusteeValue)(uintptr)objectsAndSid;
}

public static TrusteeValue TrusteeValueFromObjectsAndName(ж<OBJECTS_AND_NAME> objectsAndName) {
    return (TrusteeValue)(uintptr)objectsAndName;
}

partial struct TRUSTEE {
    public ж<TRUSTEE> MultipleTrustee;
    public MULTIPLE_TRUSTEE_OPERATION MultipleTrusteeOperation;
    public TRUSTEE_FORM TrusteeForm;
    public TRUSTEE_TYPE TrusteeType;
    public TrusteeValue TrusteeValue;
}

partial struct OBJECTS_AND_SID {
    public uint32 ObjectsPresent;
    public GUID ObjectTypeGuid;
    public GUID InheritedObjectTypeGuid;
    public ж<SID> Sid;
}

partial struct OBJECTS_AND_NAME {
    public uint32 ObjectsPresent;
    public SE_OBJECT_TYPE ObjectType;
    public ж<uint16> ObjectTypeName;
    public ж<uint16> InheritedObjectTypeName;
    public ж<uint16> Name;
}

} partial class windowsᴛSECURITY_DESCRIPTOR {

public static (SECURITY_DESCRIPTOR_CONTROL control, uint32 revision, error err) Control(this ж<SECURITY_DESCRIPTOR> Ꮡsd) {
    ref var control = ref heap(new SECURITY_DESCRIPTOR_CONTROL(), out var Ꮡcontrol);
    ref var revision = ref heap(new uint32(), out var Ꮡrevision);
    error err = default;

    err = getSecurityDescriptorControl(Ꮡsd, Ꮡcontrol, Ꮡrevision);
    return (control, revision, err);
}

public static error SetControl(this ж<SECURITY_DESCRIPTOR> Ꮡsd, SECURITY_DESCRIPTOR_CONTROL controlBitsOfInterest, SECURITY_DESCRIPTOR_CONTROL controlBitsToSet) {
    return setSecurityDescriptorControl(Ꮡsd, controlBitsOfInterest, controlBitsToSet);
}

public static (uint8 control, error err) RMControl(this ж<SECURITY_DESCRIPTOR> Ꮡsd) {
    ref var control = ref heap(new uint8(), out var Ꮡcontrol);
    error err = default;

    err = getSecurityDescriptorRMControl(Ꮡsd, Ꮡcontrol);
    return (control, err);
}

public static void SetRMControl(this ж<SECURITY_DESCRIPTOR> Ꮡsd, uint8 rmControlʗp) {
    ref var rmControl = ref heap(rmControlʗp, out var ᏑrmControl);

    setSecurityDescriptorRMControl(Ꮡsd, ᏑrmControl);
}

public static (ж<ACL> dacl, bool defaulted, error err) DACL(this ж<SECURITY_DESCRIPTOR> Ꮡsd) {
    ref var dacl = ref heap<ж<ACL>>(out var Ꮡdacl);
    bool defaulted = default;
    error err = default;

    bool present = default;
    err = getSecurityDescriptorDacl(Ꮡsd, ref present, Ꮡdacl, ref defaulted);
    if (!present) {
        err = ERROR_OBJECT_NOT_FOUND;
    }
    return (dacl, defaulted, err);
}

public static error SetDACL(this ж<SECURITY_DESCRIPTOR> ᏑabsoluteSD, ж<ACL> dacl, bool present, bool defaulted) {
    return setSecurityDescriptorDacl(ᏑabsoluteSD, present, dacl, defaulted);
}

public static (ж<ACL> sacl, bool defaulted, error err) SACL(this ж<SECURITY_DESCRIPTOR> Ꮡsd) {
    ref var sacl = ref heap<ж<ACL>>(out var Ꮡsacl);
    bool defaulted = default;
    error err = default;

    bool present = default;
    err = getSecurityDescriptorSacl(Ꮡsd, ref present, Ꮡsacl, ref defaulted);
    if (!present) {
        err = ERROR_OBJECT_NOT_FOUND;
    }
    return (sacl, defaulted, err);
}

public static error SetSACL(this ж<SECURITY_DESCRIPTOR> ᏑabsoluteSD, ж<ACL> sacl, bool present, bool defaulted) {
    return setSecurityDescriptorSacl(ᏑabsoluteSD, present, sacl, defaulted);
}

public static (ж<SID> owner, bool defaulted, error err) Owner(this ж<SECURITY_DESCRIPTOR> Ꮡsd) {
    ref var owner = ref heap<ж<SID>>(out var Ꮡowner);
    bool defaulted = default;
    error err = default;

    err = getSecurityDescriptorOwner(Ꮡsd, Ꮡowner, ref defaulted);
    return (owner, defaulted, err);
}

public static error SetOwner(this ж<SECURITY_DESCRIPTOR> ᏑabsoluteSD, ж<SID> owner, bool defaulted) {
    return setSecurityDescriptorOwner(ᏑabsoluteSD, owner, defaulted);
}

public static (ж<SID> group, bool defaulted, error err) Group(this ж<SECURITY_DESCRIPTOR> Ꮡsd) {
    ref var group = ref heap<ж<SID>>(out var Ꮡgroup);
    bool defaulted = default;
    error err = default;

    err = getSecurityDescriptorGroup(Ꮡsd, Ꮡgroup, ref defaulted);
    return (group, defaulted, err);
}

public static error SetGroup(this ж<SECURITY_DESCRIPTOR> ᏑabsoluteSD, ж<SID> group, bool defaulted) {
    return setSecurityDescriptorGroup(ᏑabsoluteSD, group, defaulted);
}

public static uint32 Length(this ж<SECURITY_DESCRIPTOR> Ꮡsd) {
    return getSecurityDescriptorLength(Ꮡsd);
}

public static bool IsValid(this ж<SECURITY_DESCRIPTOR> Ꮡsd) {
    return isValidSecurityDescriptor(Ꮡsd);
}

public static @string String(this ж<SECURITY_DESCRIPTOR> Ꮡsd) {
    GoFrame ᒐ = default;
    try {
        ref var sddl = ref heap<ж<uint16>>(out var Ꮡsddl);
        var err = convertSecurityDescriptorToStringSecurityDescriptor(Ꮡsd, 1, 0xff, Ꮡsddl, nil);
        if (err != nil) {
            return "";
        }
        defer(LocalFree, (Handle)(uintptr)sddl, ref ᒐ);
        return UTF16PtrToString(sddl);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static (ж<SECURITY_DESCRIPTOR> absoluteSD, error err) ToAbsolute(this ж<SECURITY_DESCRIPTOR> ᏑselfRelativeSD) {
    ж<SECURITY_DESCRIPTOR> absoluteSD = default;
    error err = default;

    (var control, _, err) = ᏑselfRelativeSD.Control();
    if (err != nil) {
        return (absoluteSD, err);
    }
    if ((SECURITY_DESCRIPTOR_CONTROL)(control & (uint16)SE_SELF_RELATIVE) == 0) {
        err = ERROR_INVALID_PARAMETER;
        return (absoluteSD, err);
    }
    ref var absoluteSDSize = ref heap(new uint32(), out var ᏑabsoluteSDSize);
    ref var daclSize = ref heap(new uint32(), out var ᏑdaclSize);
    ref var saclSize = ref heap(new uint32(), out var ᏑsaclSize);
    ref var ownerSize = ref heap(new uint32(), out var ᏑownerSize);
    ref var groupSize = ref heap(new uint32(), out var ᏑgroupSize);
    err = makeAbsoluteSD(ᏑselfRelativeSD, nil, ᏑabsoluteSDSize,
        nil, ᏑdaclSize, nil, ᏑsaclSize, nil, ᏑownerSize, nil, ᏑgroupSize);
    switch (err) {
    case var ᴛ when AreEqual(ᴛ, ERROR_INSUFFICIENT_BUFFER): {
        break;
    }
    case var ᴛ when AreEqual(ᴛ, default): {
        return (default, ERROR_INTERNAL_ERROR);
    }
    default: {
        return (default, err);
    }}

    if (absoluteSDSize > 0) {
        absoluteSD = @new<SECURITY_DESCRIPTOR>();
        if (/* unsafe.Sizeof(*absoluteSD) */ (uintptr)40 < (uintptr)absoluteSDSize) {
            throw panic("sizeof(SECURITY_DESCRIPTOR) too small");
        }
    }
    ж<ACL> dacl = default;
    ж<ACL> sacl = default;
    ж<SID> owner = default;
    ж<SID> group = default;
    if (daclSize > 0) {
        dacl = @unsafe.SliceData(new slice<byte>((nint)(daclSize))).Reinterpret<byte, ACL>();
    }
    if (saclSize > 0) {
        sacl = @unsafe.SliceData(new slice<byte>((nint)(saclSize))).Reinterpret<byte, ACL>();
    }
    if (ownerSize > 0) {
        owner = @unsafe.SliceData(new slice<byte>((nint)(ownerSize))).Reinterpret<byte, SID>();
    }
    if (groupSize > 0) {
        group = @unsafe.SliceData(new slice<byte>((nint)(groupSize))).Reinterpret<byte, SID>();
    }
    err = makeAbsoluteSD(ᏑselfRelativeSD, absoluteSD, ᏑabsoluteSDSize,
        dacl, ᏑdaclSize, sacl, ᏑsaclSize, owner, ᏑownerSize, group, ᏑgroupSize);
    if (err != nil) {
        return (default, err);
    }
    (var absControl, _, err) = absoluteSD.Control();
    if (err != nil) {
        throw panic("absoluteSD: " + err.Error());
    }
    if ((SECURITY_DESCRIPTOR_CONTROL)(absControl & (uint16)SE_SELF_RELATIVE) != 0) {
        throw panic("absoluteSD not in absolute format");
    }
    if (absoluteSD.dacl != dacl) {
        throw panic("dacl pointer mismatch");
    }
    if (absoluteSD.sacl != sacl) {
        throw panic("sacl pointer mismatch");
    }
    if (absoluteSD.owner != owner) {
        throw panic("owner pointer mismatch");
    }
    if (absoluteSD.group != group) {
        throw panic("group pointer mismatch");
    }
    absoluteSD.dacl = dacl;
    absoluteSD.sacl = sacl;
    absoluteSD.owner = owner;
    absoluteSD.group = group;

    return (absoluteSD, err);
}

public static (ж<SECURITY_DESCRIPTOR> selfRelativeSD, error err) ToSelfRelative(this ж<SECURITY_DESCRIPTOR> ᏑabsoluteSD) {
    ж<SECURITY_DESCRIPTOR> selfRelativeSD = default;
    error err = default;

    (var control, _, err) = ᏑabsoluteSD.Control();
    if (err != nil) {
        return (selfRelativeSD, err);
    }
    if ((SECURITY_DESCRIPTOR_CONTROL)(control & (uint16)SE_SELF_RELATIVE) != 0) {
        err = ERROR_INVALID_PARAMETER;
        return (selfRelativeSD, err);
    }
    ref var selfRelativeSDSize = ref heap(new uint32(), out var ᏑselfRelativeSDSize);
    err = makeSelfRelativeSD(ᏑabsoluteSD, nil, ᏑselfRelativeSDSize);
    switch (err) {
    case var ᴛ when AreEqual(ᴛ, ERROR_INSUFFICIENT_BUFFER): {
        break;
    }
    case var ᴛ when AreEqual(ᴛ, default): {
        return (default, ERROR_INTERNAL_ERROR);
    }
    default: {
        return (default, err);
    }}

    if (selfRelativeSDSize > 0) {
        selfRelativeSD = Ꮡ(new slice<byte>((nint)(selfRelativeSDSize)), 0).Reinterpret<byte, SECURITY_DESCRIPTOR>();
    }
    err = makeSelfRelativeSD(ᏑabsoluteSD, selfRelativeSD, ᏑselfRelativeSDSize);
    return (selfRelativeSD, err);
}

internal static ж<SECURITY_DESCRIPTOR> copySelfRelativeSecurityDescriptor(this ж<SECURITY_DESCRIPTOR> ᏑselfRelativeSD) {
    nint sdLen = (nint)ᏑselfRelativeSD.Length();
    const nint min = /* int(unsafe.Sizeof(SECURITY_DESCRIPTOR{})) */ 40;
    if (sdLen < min) {
        sdLen = min;
    }

    var src = @unsafe.Slice(ᏑselfRelativeSD.Reinterpret<SECURITY_DESCRIPTOR, byte>(), sdLen);
    const nint psize = /* int(unsafe.Sizeof(uintptr(0))) */ 8;
    var alloc = new slice<uintptr>((sdLen + psize - 1) / psize);
    var dst = @unsafe.Slice(Ꮡ(alloc, 0).Reinterpret<uintptr, byte>(), sdLen);
    copy(dst, src);
    return Ꮡ(dst, 0).Reinterpret<byte, SECURITY_DESCRIPTOR>();
}

} partial class windows_package {

public static (ж<SECURITY_DESCRIPTOR> sd, error err) SecurityDescriptorFromString(@string sddl) {
    ж<SECURITY_DESCRIPTOR> sd = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var winHeapSD = ref heap<ж<SECURITY_DESCRIPTOR>>(out var ᏑwinHeapSD);
        err = convertStringSecurityDescriptorToSecurityDescriptor(sddl, 1, ᏑwinHeapSD, nil);
        if (err != nil) {
            goto ᒐdone;
        }
        defer(LocalFree, (Handle)(uintptr)winHeapSD, ref ᒐ);
        (sd, err) = (winHeapSD.copySelfRelativeSecurityDescriptor(), default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (sd, err);
}

public static (ж<SECURITY_DESCRIPTOR> sd, error err) GetSecurityInfo(Handle handle, SE_OBJECT_TYPE objectType, SECURITY_INFORMATION securityInformation) {
    ж<SECURITY_DESCRIPTOR> sd = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var winHeapSD = ref heap<ж<SECURITY_DESCRIPTOR>>(out var ᏑwinHeapSD);
        err = getSecurityInfo(handle, objectType, securityInformation, nil, nil, nil, nil, ᏑwinHeapSD);
        if (err != nil) {
            goto ᒐdone;
        }
        defer(LocalFree, (Handle)(uintptr)winHeapSD, ref ᒐ);
        (sd, err) = (winHeapSD.copySelfRelativeSecurityDescriptor(), default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (sd, err);
}

public static (ж<SECURITY_DESCRIPTOR> sd, error err) GetNamedSecurityInfo(@string objectName, SE_OBJECT_TYPE objectType, SECURITY_INFORMATION securityInformation) {
    ж<SECURITY_DESCRIPTOR> sd = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var winHeapSD = ref heap<ж<SECURITY_DESCRIPTOR>>(out var ᏑwinHeapSD);
        err = getNamedSecurityInfo(objectName, objectType, securityInformation, nil, nil, nil, nil, ᏑwinHeapSD);
        if (err != nil) {
            goto ᒐdone;
        }
        if (winHeapSD == nil) {
            (sd, err) = (default, default); goto ᒐdone;
        }
        defer(LocalFree, (Handle)(uintptr)winHeapSD, ref ᒐ);
        (sd, err) = (winHeapSD.copySelfRelativeSecurityDescriptor(), default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (sd, err);
}

public static (ж<SECURITY_DESCRIPTOR> sd, error err) BuildSecurityDescriptor(ж<TRUSTEE> owner, ж<TRUSTEE> group, slice<EXPLICIT_ACCESS> accessEntries, slice<EXPLICIT_ACCESS> auditEntries, ж<SECURITY_DESCRIPTOR> mergedSecurityDescriptor) {
    ж<SECURITY_DESCRIPTOR> sd = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var winHeapSD = ref heap<ж<SECURITY_DESCRIPTOR>>(out var ᏑwinHeapSD);
        ref var winHeapSDSize = ref heap(new uint32(), out var ᏑwinHeapSDSize);
        ж<EXPLICIT_ACCESS> firstAccessEntry = default;
        if (len(accessEntries) > 0) {
            firstAccessEntry = Ꮡ(accessEntries, 0);
        }
        ж<EXPLICIT_ACCESS> firstAuditEntry = default;
        if (len(auditEntries) > 0) {
            firstAuditEntry = Ꮡ(auditEntries, 0);
        }
        err = buildSecurityDescriptor(owner, group, (uint32)len(accessEntries), firstAccessEntry, (uint32)len(auditEntries), firstAuditEntry, mergedSecurityDescriptor, ᏑwinHeapSDSize, ᏑwinHeapSD);
        if (err != nil) {
            goto ᒐdone;
        }
        defer(LocalFree, (Handle)(uintptr)winHeapSD, ref ᒐ);
        (sd, err) = (winHeapSD.copySelfRelativeSecurityDescriptor(), default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (sd, err);
}

public static (ж<SECURITY_DESCRIPTOR> absoluteSD, error err) NewSecurityDescriptor() {
    ж<SECURITY_DESCRIPTOR> absoluteSD = default;
    error err = default;

    absoluteSD = Ꮡ(new SECURITY_DESCRIPTOR());
    err = initializeSecurityDescriptor(absoluteSD, 1);
    return (absoluteSD, err);
}

public static (ж<ACL> acl, error err) ACLFromEntries(slice<EXPLICIT_ACCESS> explicitEntries, ж<ACL> mergedACL) {
    ж<ACL> acl = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ж<EXPLICIT_ACCESS> firstExplicitEntry = default;
        if (len(explicitEntries) > 0) {
            firstExplicitEntry = Ꮡ(explicitEntries, 0);
        }
        ref var winHeapACL = ref heap<ж<ACL>>(out var ᏑwinHeapACL);
        err = setEntriesInAcl((uint32)len(explicitEntries), firstExplicitEntry, mergedACL, ᏑwinHeapACL);
        if (err != nil) {
            goto ᒐdone;
        }
        defer(LocalFree, (Handle)(uintptr)winHeapACL, ref ᒐ);
        var aclBytes = new slice<byte>(winHeapACL.aclSize);
        copy(aclBytes, @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(winHeapACL), 0, len(aclBytes), len(aclBytes), 2147483647));
        (acl, err) = (Ꮡ(aclBytes, 0).Reinterpret<byte, ACL>(), default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (acl, err);
}

} // end windows_package
