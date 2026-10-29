// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.os;

using errors = errors_package;
using fmt = fmt_package;
using windows = @internal.syscall.windows_package;
using registry = @internal.syscall.windows.registry_package;
using runtime = runtime_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using @internal.syscall;
using @internal.syscall.windows;

partial class user_package {

internal static (bool, error) isDomainJoined() {
    ref var domain = ref heap<ж<uint16>>(out var Ꮡdomain);
    ref var status = ref heap(new uint32(), out var Ꮡstatus);
    var err = syscall.NetGetJoinInformation(nil, Ꮡdomain, Ꮡstatus);
    if (err != nil) {
        return (false, err);
    }
    syscall.NetApiBufferFree(domain.Reinterpret<uint16, byte>());
    return (status == syscall.NetSetupDomainName, default);
}

internal static (@string, error) lookupFullNameDomain(@string domainAndUser) {
    return syscall.TranslateAccountName(domainAndUser,
        syscall.NameSamCompatible, syscall.NameDisplay, 50);
}

// go2cs generated this placeholder — func lookupFullNameServer is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static (@string, error) lookupFullName(@string domain, @string username, @string domainAndUser) {
    var (joined, err) = isDomainJoined();
    if (err == nil && joined) {
        var (nameΔ1, errΔ1) = lookupFullNameDomain(domainAndUser);
        if (errΔ1 == nil) {
            return (nameΔ1, default);
        }
    }
    (var name, err) = lookupFullNameServer(domain, username);
    if (err == nil) {
        return (name, default);
    }
    // domain worked neither as a domain nor as a server
    // could be domain server unavailable
    // pretend username is fullname
    return (username, default);
}

// getProfilesDirectory retrieves the path to the root directory
// where user profiles are stored.
internal static (@string, error) getProfilesDirectory() {
    ref var n = ref heap<uint32>((uint32)100, out var Ꮡn);
    for (;;) {
        var b = new slice<uint16>((nint)(n));
        var e = windows.GetProfilesDirectory(Ꮡ(b, 0), Ꮡn);
        if (e == nil) {
            return (syscall.UTF16ToString(b), default);
        }
        if (!AreEqual(e, syscall.ERROR_INSUFFICIENT_BUFFER)) {
            return ("", e);
        }
        if (n <= (uint32)len(b)) {
            return ("", e);
        }
    }
}

internal static bool isServiceAccount(ж<syscall.SID> sid) {
    if (!windows.IsValidSid(sid)) {
        // We don't accept SIDs from the public API, so this should never happen.
        // Better be on the safe side and validate anyway.
        return false;
    }
    // The following RIDs are considered service user accounts as per
    // https://learn.microsoft.com/en-us/windows/win32/secauthz/well-known-sids and
    // https://learn.microsoft.com/en-us/windows/win32/services/service-user-accounts:
    // - "S-1-5-18": LocalSystem
    // - "S-1-5-19": LocalService
    // - "S-1-5-20": NetworkService
    if (windows.GetSidSubAuthorityCount(sid) != windows.SID_REVISION || windows.GetSidIdentifierAuthority(sid) != windows.SECURITY_NT_AUTHORITY) {
        return false;
    }
    switch (windows.GetSidSubAuthority(sid, 0)) {
    case var ᴛ when ᴛ == windows.SECURITY_LOCAL_SYSTEM_RID || ᴛ == windows.SECURITY_LOCAL_SERVICE_RID || ᴛ == windows.SECURITY_NETWORK_SERVICE_RID: {
        return true;
    }}
    return false;
}

internal static bool isValidUserAccountType(ж<syscall.SID> sid, uint32 sidType) {
    switch (sidType) {
    case var ᴛ when ᴛ == syscall.SidTypeUser: {
        return true;
    }
    case var ᴛ when ᴛ == syscall.SidTypeWellKnownGroup: {
        return isServiceAccount(sid);
    }}
    return false;
}

internal static bool isValidGroupAccountType(uint32 sidType) {
    switch (sidType) {
    case var ᴛ when ᴛ == syscall.SidTypeGroup: {
        return true;
    }
    case var ᴛ when ᴛ == syscall.SidTypeWellKnownGroup: {
        // Some well-known groups are also considered service accounts,
        // so isValidUserAccountType would return true for them.
        // We have historically allowed them in LookupGroup and LookupGroupId,
        // so don't treat them as invalid here.
        return true;
    }
    case var ᴛ when ᴛ == syscall.SidTypeAlias: {
        // https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-samr/7b2aeb27-92fc-41f6-8437-deb65d950921#gt_0387e636-5654-4910-9519-1f8326cf5ec0
        // SidTypeAlias should also be treated as a group type next to SidTypeGroup
        // and SidTypeWellKnownGroup:
        // "alias object -> resource group: A group object..."
        //
        // Tests show that "Administrators" can be considered of type SidTypeAlias.
        return true;
    }}
    return false;
}

// lookupUsernameAndDomain obtains the username and domain for usid.
internal static (@string username, @string domain, uint32 sidType, error e) lookupUsernameAndDomain(ж<syscall.SID> usid) {
    @string username = default;
    @string domain = default;
    uint32 sidType = default;
    error e = default;

    (username, domain, sidType, e) = usid.LookupAccount(""u8);
    if (e != nil) {
        return ("", "", 0, e);
    }
    if (!isValidUserAccountType(usid, sidType)) {
        return ("", "", 0, fmt.Errorf("user: should be user account type, not %d"u8, sidType));
    }
    return (username, domain, sidType, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string profileImagePathˢ = "ProfileImagePath"u8;

// findHomeDirInRegistry finds the user home path based on the uid.
internal static (@string dir, error e) findHomeDirInRegistry(@string uid) {
    @string dir = default;
    error e = default;
    GoFrame ᒐ = default;
    try {
        (var k, e) = registry.OpenKey(registry.LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\"u8 + uid, registry.QUERY_VALUE);
        if (e != nil) {
            (dir, e) = ("", e); goto ᒐdone;
        }
        defer(() => k.Close(), ref ᒐ);
        (dir, _, e) = k.GetStringValue(profileImagePathˢ);
        if (e != nil) {
            (dir, e) = ("", e); goto ᒐdone;
        }
        (dir, e) = (dir, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (dir, e);
}

// lookupGroupName accepts the name of a group and retrieves the group SID.
internal static (@string, error) lookupGroupName(@string groupname) {
    var (sid, _, t, e) = syscall.LookupSID(""u8, groupname);
    if (e != nil) {
        return ("", e);
    }
    if (!isValidGroupAccountType(t)) {
        return ("", fmt.Errorf("lookupGroupName: should be group account type, not %d"u8, t));
    }
    return sid.String();
}

// go2cs generated this placeholder — func listGroupsForUsernameAndDomain is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static (ж<User>, error) newUser(@string uid, @string gid, @string dir, @string username, @string domain) {
    ref var domainAndUser = ref heap<@string>(domain + @"\"u8 + username, out var ᏑdomainAndUser);
    ref var name = ref heap<@string>(out var Ꮡname);
    (name, var e) = lookupFullName(domain, username, domainAndUser);
    if (e != nil) {
        return (default, e);
    }
    var u = Ꮡ(new User(
        Uid: uid,
        Gid: gid,
        Username: domainAndUser,
        Name: name,
        HomeDir: dir
    ));
    return (u, default);
}

// unused variables (in this implementation)
// modified during test to exercise code paths in the cgo implementation.
internal static nint userBuffer = 0;
internal static nint groupBuffer = 0;

internal static (ж<User>, error) current() {
    // Use runAsProcessOwner to ensure that we can access the process token
    // when calling syscall.OpenCurrentProcessToken if the current thread
    // is impersonating a different user. See https://go.dev/issue/68647.
    ref var usr = ref heap<ж<User>>(out var Ꮡusr);
    var err = runAsProcessOwner(error () => {
        GoFrame ᒐ = default;
        try {
            var (t, e) = syscall.OpenCurrentProcessToken();
            if (e != nil) {
                return e;
            }
            defer(() => t.Close(), ref ᒐ);
            (var u, e) = t.GetTokenUser();
            if (e != nil) {
                return e;
            }
            (var pg, e) = t.GetTokenPrimaryGroup();
            if (e != nil) {
                return e;
            }
            ref var uid = ref heap<@string>(out var Ꮡuid);
            (uid, e) = u.User.Sid.String();
            if (e != nil) {
                return e;
            }
            ref var gid = ref heap<@string>(out var Ꮡgid);
            (gid, e) = pg.PrimaryGroup.String();
            if (e != nil) {
                return e;
            }
            ref var dir = ref heap<@string>(out var Ꮡdir);
            (dir, e) = t.GetUserProfileDirectory();
            if (e != nil) {
                return e;
            }
            ref var username = ref heap<@string>(out var Ꮡusername);
            (username, e) = windows.GetUserName(syscall.NameSamCompatible);
            if (e != nil) {
                return e;
            }
            ref var displayName = ref heap<@string>(out var ᏑdisplayName);
            (displayName, e) = windows.GetUserName(syscall.NameDisplay);
            if (e != nil) {
                // Historically, the username is used as fallback
                // when the display name can't be retrieved.
                displayName = username;
            }
            Ꮡusr.ValueSlot = Ꮡ(new User(
                Uid: uid,
                Gid: gid,
                Username: username,
                Name: displayName,
                HomeDir: dir
            ));
            return default;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
        finally { ᒐ.Run(); }
    });
    return (usr, err);
}

// runAsProcessOwner runs f in the context of the current process owner,
// that is, removing any impersonation that may be in effect before calling f,
// and restoring the impersonation afterwards.
internal static error runAsProcessOwner(Func<error> f) {
    GoFrame ᒐ = default;
    try {
        ref var impersonationRollbackErr = ref heap<error>(out var ᏑimpersonationRollbackErr);
        runtime.LockOSThread();
        defer(() => {
            // If impersonation failed, the thread is running with the wrong token,
            // so it's better to terminate it.
            // This is achieved by not calling runtime.UnlockOSThread.
            if (ᏑimpersonationRollbackErr.ValueSlot != nil) {
                println((@string)"os/user: failed to revert to previous token:"u8, ᏑimpersonationRollbackErr.ValueSlot.Error());
                runtime.Goexit();
            } else {
                runtime.UnlockOSThread();
            }
        }, ref ᒐ);
        var (prevToken, isProcessToken, err) = getCurrentToken();
        if (err != nil) {
            return fmt.Errorf("os/user: failed to get current token: %w"u8, err);
        }
        defer(() => prevToken.Close(), ref ᒐ);
        if (!isProcessToken) {
            {
                err = windows.RevertToSelf(); if (err != nil) {
                    return fmt.Errorf("os/user: failed to revert to self: %w"u8, err);
                }
            }
            defer(() => {
                ᏑimpersonationRollbackErr.ValueSlot = windows.ImpersonateLoggedOnUser(prevToken);
            }, ref ᒐ);
        }
        return f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// getCurrentToken returns the current thread token, or
// the process token if the thread doesn't have a token.
internal static (syscall.Token t, bool isProcessToken, error err) getCurrentToken() {
    ref var t = ref heap(new syscall.Token(), out var Ꮡt);
    bool isProcessToken = default;
    error err = default;

    var (thread, _) = windows.GetCurrentThread();
    // Need TOKEN_DUPLICATE and TOKEN_IMPERSONATE to use the token in ImpersonateLoggedOnUser.
    err = windows.OpenThreadToken(thread, (uint32)((UntypedInt)(syscall.TOKEN_QUERY | syscall.TOKEN_DUPLICATE) | (uint32)syscall.TOKEN_IMPERSONATE), true, Ꮡt);
    if (errors.Is(err, windows.ERROR_NO_TOKEN)) {
        // Not impersonating, use the process token.
        isProcessToken = true;
        (t, err) = syscall.OpenCurrentProcessToken();
    }
    return (t, isProcessToken, err);
}

// go2cs generated this placeholder — func lookupUserPrimaryGroup is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static (ж<User>, error) newUserFromSid(ж<syscall.SID> usid) {
    var (username, domain, sidType, e) = lookupUsernameAndDomain(usid);
    if (e != nil) {
        return (default, e);
    }
    (var uid, e) = usid.String();
    if (e != nil) {
        return (default, e);
    }
    @string gid = default;
    if (sidType == syscall.SidTypeWellKnownGroup) {
        // The SID does not contain a domain; this function's domain variable has
        // been populated with the SID's identifier authority. This happens with
        // special service user accounts such as "NT AUTHORITY\LocalSystem".
        // In this case, gid is the same as the user SID.
        gid = uid;
    } else {
        (gid, e) = lookupUserPrimaryGroup(username, domain);
        if (e != nil) {
            return (default, e);
        }
    }
    // If this user has logged in at least once their home path should be stored
    // in the registry under the specified SID. References:
    // https://social.technet.microsoft.com/wiki/contents/articles/13895.how-to-remove-a-corrupted-user-profile-from-the-registry.aspx
    // https://support.asperasoft.com/hc/en-us/articles/216127438-How-to-delete-Windows-user-profiles
    //
    // The registry is the most reliable way to find the home path as the user
    // might have decided to move it outside of the default location,
    // (e.g. C:\users). Reference:
    // https://answers.microsoft.com/en-us/windows/forum/windows_7-security/how-do-i-set-a-home-directory-outside-cusers-for-a/aed68262-1bf4-4a4d-93dc-7495193a440f
    (var dir, e) = findHomeDirInRegistry(uid);
    if (e != nil) {
        // If the home path does not exist in the registry, the user might
        // have not logged in yet; fall back to using getProfilesDirectory().
        // Find the username based on a SID and append that to the result of
        // getProfilesDirectory(). The domain is not relevant here.
        (dir, e) = getProfilesDirectory();
        if (e != nil) {
            return (default, e);
        }
        dir += @"\"u8 + username;
    }
    return newUser(uid, gid, dir, username, domain);
}

internal static (ж<User>, error) lookupUser(@string username) {
    var (sid, _, t, e) = syscall.LookupSID(""u8, username);
    if (e != nil) {
        return (default, e);
    }
    if (!isValidUserAccountType(sid, t)) {
        return (default, fmt.Errorf("user: should be user account type, not %d"u8, t));
    }
    return newUserFromSid(sid);
}

internal static (ж<User>, error) lookupUserId(@string uid) {
    var (sid, e) = syscall.StringToSid(uid);
    if (e != nil) {
        return (default, e);
    }
    return newUserFromSid(sid);
}

internal static (ж<Group>, error) lookupGroup(@string groupname) {
    ref var sid = ref heap<@string>(out var Ꮡsid);
    (sid, var err) = lookupGroupName(groupname);
    if (err != nil) {
        return (default, err);
    }
    return (Ꮡ(new Group(Name: groupname, Gid: sid)), default);
}

internal static (ж<Group>, error) lookupGroupId(@string gid) {
    var (sid, err) = syscall.StringToSid(gid);
    if (err != nil) {
        return (default, err);
    }
    ref var groupname = ref heap<@string>(out var Ꮡgroupname);
    (groupname, _, var t, err) = sid.LookupAccount(""u8);
    if (err != nil) {
        return (default, err);
    }
    if (!isValidGroupAccountType(t)) {
        return (default, fmt.Errorf("lookupGroupId: should be group account type, not %d"u8, t));
    }
    return (Ꮡ(new Group(Name: groupname, Gid: gid)), default);
}

internal static (slice<@string>, error) listGroups(ref User user) {
    ref var sids = ref heap<slice<@string>>(out var Ꮡsids);
    {
        var (u, err) = Current(); if (err == nil && u.Uid == user.Uid) {
            // It is faster and more reliable to get the groups
            // of the current user from the current process token.
            var errΔ1 = runAsProcessOwner(error () => {
                GoFrame ᒐ = default;
                try {
                    var (t, errΔ2) = syscall.OpenCurrentProcessToken();
                    if (errΔ2 != nil) {
                        return errΔ2;
                    }
                    defer(() => t.Close(), ref ᒐ);
                    (var groups, errΔ2) = windows.GetTokenGroups(t);
                    if (errΔ2 != nil) {
                        return errΔ2;
                    }
                    foreach (var (_, g) in groups.AllGroups()) {
                        var (sid, errΔ3) = g.Sid.String();
                        if (errΔ3 != nil) {
                            return errΔ3;
                        }
                        Ꮡsids.ValueSlot = append(Ꮡsids.ValueSlot, sid);
                    }
                    return default;
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
                finally { ᒐ.Run(); }
            });
            if (errΔ1 != nil) {
                return (default, errΔ1);
            }
        } else {
            var (sid, errΔ4) = syscall.StringToSid(user.Uid);
            if (errΔ4 != nil) {
                return (default, errΔ4);
            }
            (var username, var domain, _, errΔ4) = lookupUsernameAndDomain(sid);
            if (errΔ4 != nil) {
                return (default, errΔ4);
            }
            (sids, errΔ4) = listGroupsForUsernameAndDomain(username, domain);
            if (errΔ4 != nil) {
                return (default, errΔ4);
            }
        }
    }
    // Add the primary group of the user to the list if it is not already there.
    // This is done only to comply with the POSIX concept of a primary group.
    foreach (var (_, sid) in sids) {
        if (sid == user.Gid) {
            return (sids, default);
        }
    }
    return (append(sids, user.Gid), default);
}

} // end user_package
