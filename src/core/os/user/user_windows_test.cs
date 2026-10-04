// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.os;

using rand = crypto.rand_package;
using base64 = encoding.base64_package;
using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using windows = @internal.syscall.windows_package;
using testenv = @internal.testenv_package;
using os = os_package;
using exec = go.os.exec_package;
using runtime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using syscall = syscall_package;
using testing = testing_package;
using unicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.syscall;
using crypto;
using encoding;
using go.os;
using go.unicode;
using io = io_package;
using static go.os.user_package;

partial class user_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestDonTHaveˢ = (@string)"skipping test; don't have permission to create user"u8;
internal static readonly object userAlreadyExistsTryingˢ = (@string)"user already exists, trying again with a different name"u8;

// addUserAccount creates a local user account.
// It returns the name and password of the new account.
// Multiple programs or goroutines calling addUserAccount simultaneously will not choose the same directory.
internal static (@string name, @string password) addUserAccount(ж<testing.T> Ꮡt) {
    @string password = default!;

    Ꮡt.TempDir();
    @string pattern = Ꮡt.Name();
    // Windows limits the user name to 20 characters,
    // leave space for a 4 digits random suffix.
    UntypedInt maxNameLen = 20;
    UntypedInt suffixLen = 4;
    pattern = pattern.slice(0, min(len(pattern), (nint)(maxNameLen - suffixLen)));
    // Drop unusual characters from the account name.
    var mapper = (rune r) => {
        if (r < utf8.RuneSelf){
            if ((rune)'0' <= r && r <= (rune)'9' || (rune)'a' <= r && r <= (rune)'z' || (rune)'A' <= r && r <= (rune)'Z') {
                return r;
            }
        } else 
        if (unicode.IsLetter(r) || unicode.IsNumber(r)) {
            return r;
        }
        return -1;
    };
    pattern = strings.Map(mapper, pattern);
    // Generate a long random password.
    array<byte> pwd = new(33);
    rand.Read(pwd[..]);
    // Add special chars to ensure it satisfies password requirements.
    password = base64.StdEncoding.EncodeToString(pwd[..]) + "_-As@!%*(1)4#2"u8;
    var (password16, err) = syscall.UTF16PtrFromString(password);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    nint @try = 0;
    while (ᐧ) {
        // Calculate a random suffix to append to the user name.
        array<byte> suffix = new(2);
        rand.Read(suffix[..]);
        @string suffixStr = strconv.FormatUint((uint64)binary.LittleEndian.Uint16(suffix[..]), 10);
        @string nameΔ1 = pattern + suffixStr.slice(0, min(len(suffixStr), (nint)(suffixLen)));
        var (name16, errΔ1) = syscall.UTF16PtrFromString(nameΔ1);
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
        // Create user.
        ref var userInfo = ref heap<windows.UserInfo1>(out var ᏑuserInfo);
        userInfo = new windows.UserInfo1(
            Name: name16,
            Password: password16,
            Priv: windows.USER_PRIV_USER
        );
        errΔ1 = windows.NetUserAdd(nil, 1, ᏑuserInfo.Reinterpret<windows.UserInfo1, byte>(), nil);
        if (errors.Is(errΔ1, syscall.ERROR_ACCESS_DENIED)) {
            Ꮡt.Skip(skippingTestDonTHaveˢ);
        }
        // If the user already exists, try again with a different name.
        if (errors.Is(errΔ1, windows.NERR_UserExists)) {
            {
                @try++; if (@try < 1000) {
                    Ꮡt.Log(userAlreadyExistsTryingˢ);
                    continue;
                }
            }
        }
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("NetUserAdd failed: %v"u8, errΔ1);
        }
        // Delete the user when the test is done.
        var name16ʗ1 = name16;
        Ꮡt.Cleanup(() => {
            {
                var errΔ2 = windows.NetUserDel(nil, name16ʗ1); if (errΔ2 != default!) {
                    if (!errors.Is(errΔ2, windows.NERR_UserNotFound)) {
                        Ꮡt.Fatal(errΔ2);
                    }
                }
            }
        });
        return (nameΔ1, password);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingNonHermeticTestˢ = (@string)"skipping non-hermetic test outside of Go builders"u8;

// windowsTestAccount creates a test user and returns a token for that user.
// If the user already exists, it will be deleted and recreated.
// The caller is responsible for closing the token.
internal static (syscall.Token, ж<global::go.os.user_package.User>) windowsTestAccount(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (testenv.Builder() == ""u8) {
        // Adding and deleting users requires special permissions.
        // Even if we have them, we don't want to create users on
        // on dev machines, as they may not be cleaned up.
        // See https://dev.go/issue/70396.
        Ꮡt.Skip(skippingNonHermeticTestˢ);
    }
    var (name, password) = addUserAccount(Ꮡt);
    var (name16, err) = syscall.UTF16PtrFromString(name);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var pwd16, err) = syscall.UTF16PtrFromString(password);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var domain, err) = syscall.UTF16PtrFromString("."u8);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    const uint32 LOGON32_PROVIDER_DEFAULT = 0;
    const uint32 LOGON32_LOGON_INTERACTIVE = 2;
    ref var token = ref heap(new syscall.Token(), out var Ꮡtoken);
    {
        err = windows.LogonUser(name16, domain, pwd16, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, Ꮡtoken); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    Ꮡt.Cleanup(() => {
        Ꮡtoken.Value.Close();
    });
    (var usr, err) = Lookup(name);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    return (token, usr);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorˢ = (@string)"expected error"u8;

public static void TestImpersonatedSelf(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        runtime.LockOSThread();
        defer(runtime.UnlockOSThread, ref ᒐ);
        ref var err = ref heap<error>(out var Ꮡerr);
        (var want, err) = current();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var levels = new uint32[]{
            windows.SecurityAnonymous,
            windows.SecurityIdentification,
            windows.SecurityImpersonation,
            windows.SecurityDelegation
        }.slice();
        foreach (var (_, level) in levels) {
            var wantʗ1 = want;
            Ꮡt.Run(strconv.Itoa((nint)level), (ж<testing.T> tΔ1) => {
                GoFrame ᒐ = default;
                try {
                    {
                        Ꮡerr.ValueSlot = windows.ImpersonateSelf(level); if (Ꮡerr.ValueSlot != default!) {
                            tΔ1.Fatal(Ꮡerr.ValueSlot);
                        }
                    }
                    defer(() => windows.RevertToSelf(), ref ᒐ);
                    var (got, errΔ1) = current();
                    if (level == windows.SecurityAnonymous) {
                        // We can't get the process token when using an anonymous token,
                        // so we expect an error here.
                        if (errΔ1 == default!) {
                            tΔ1.Fatal(expectedErrorˢ);
                        }
                        return;
                    }
                    if (errΔ1 != default!) {
                        tΔ1.Fatal(errΔ1);
                    }
                    compare(tΔ1, wantʗ1, got);
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestImpersonated(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        runtime.LockOSThread();
        defer(runtime.UnlockOSThread, ref ᒐ);
        ref var err = ref heap<error>(out var Ꮡerr);
        (var want, err) = current();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        // Create a test user and log in as that user.
        var (token, _) = windowsTestAccount(Ꮡt);
        // Impersonate the test user.
        {
            err = windows.ImpersonateLoggedOnUser(token); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        defer(() => {
            Ꮡerr.ValueSlot = windows.RevertToSelf();
            if (Ꮡerr.ValueSlot != default!) {
                // If we can't revert to self, we can't continue testing.
                throw panic(Ꮡerr.ValueSlot);
            }
        }, ref ᒐ);
        (var got, err) = current();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        compare(Ꮡt, want, got);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goWantHelperProcessˢ = "GO_WANT_HELPER_PROCESS"u8;
internal static readonly @string netapi32Dllˢ = "netapi32.dll"u8;
internal static readonly @string testRunˢ = "-test.run=^TestCurrentNetapi32$"u8;

public static void TestCurrentNetapi32(ж<testing.T> Ꮡt) {
    if (os.Getenv(goWantHelperProcessˢ) == "1"u8) {
        // Test that Current does not load netapi32.dll.
        // First call Current.
        Current();
        // Then check if netapi32.dll is loaded.
        var (netapi32, errΔ1) = syscall.UTF16PtrFromString(netapi32Dllˢ);
        if (errΔ1 != default!) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "error: %s\n"u8, errΔ1.Error());
            os.Exit(9);
            return;
        }
        var (mod, _) = windows.GetModuleHandle(netapi32);
        if (mod != 0) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "netapi32.dll is loaded\n"u8);
            os.Exit(9);
            return;
        }
        os.Exit(0);
        return;
    }
    @string exe = testenv.Executable(new user_internal_test_package.testing_TжTB(Ꮡt));
    var cmd = testenv.CleanCmdEnv(exec.Command(exe, testRunˢ));
    cmd.Value.Env = append((~cmd).Env, "GO_WANT_HELPER_PROCESS=1"u8);
    var (@out, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("%v\n%s"u8, err, @out);
    }
}

public static void TestGroupIdsTestUser(ж<testing.T> Ꮡt) {
    // Create a test user and log in as that user.
    var (_, user) = windowsTestAccount(Ꮡt);
    var (gids, err) = user.GroupIds();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (err != default!) {
        Ꮡt.Fatalf("%+v.GroupIds(): %v"u8, user.OrTypedNil(), err);
    }
    if (!containsID(gids, (~user).Gid)) {
        Ꮡt.Errorf("%+v.GroupIds() = %v; does not contain user GID %s"u8, user.OrTypedNil(), gids, (~user).Gid);
    }
}


[GoType("dyn")] partial struct serviceAccountsᴛ1 {
    internal @string sid;
    internal @string name;
}
internal static slice<serviceAccountsᴛ1> serviceAccounts = new serviceAccountsᴛ1[]{
    new("S-1-5-18"u8, "NT AUTHORITY\\SYSTEM"u8),
    new("S-1-5-19"u8, "NT AUTHORITY\\LOCAL SERVICE"u8),
    new("S-1-5-20"u8, "NT AUTHORITY\\NETWORK SERVICE"u8)
}.slice();

public static void TestLookupServiceAccount(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    foreach (var (_, tt) in serviceAccounts) {
        var (u, err) = Lookup(tt.name);
        if (err != default!) {
            Ꮡt.Errorf("Lookup(%q): %v"u8, tt.name, err);
            continue;
        }
        if ((~u).Uid != tt.sid) {
            Ꮡt.Errorf("unexpected uid for %q; got %q, want %q"u8, (~u).Name, (~u).Uid, tt.sid);
        }
    }
}

public static void TestLookupIdServiceAccount(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    foreach (var (_, tt) in serviceAccounts) {
        var (u, err) = LookupId(tt.sid);
        if (err != default!) {
            Ꮡt.Errorf("LookupId(%q): %v"u8, tt.sid, err);
            continue;
        }
        if ((~u).Gid != tt.sid) {
            Ꮡt.Errorf("unexpected gid for %q; got %q, want %q"u8, (~u).Name, (~u).Gid, tt.sid);
        }
        if ((~u).Username != tt.name) {
            Ꮡt.Errorf("unexpected user name for %q; got %q, want %q"u8, (~u).Gid, (~u).Username, tt.name);
        }
    }
}

public static void TestLookupGroupServiceAccount(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    foreach (var (_, tt) in serviceAccounts) {
        var (u, err) = LookupGroup(tt.name);
        if (err != default!) {
            Ꮡt.Errorf("LookupGroup(%q): %v"u8, tt.name, err);
            continue;
        }
        if ((~u).Gid != tt.sid) {
            Ꮡt.Errorf("unexpected gid for %q; got %q, want %q"u8, (~u).Name, (~u).Gid, tt.sid);
        }
    }
}

public static void TestLookupGroupIdServiceAccount(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    foreach (var (_, tt) in serviceAccounts) {
        var (u, err) = LookupGroupId(tt.sid);
        if (err != default!) {
            Ꮡt.Errorf("LookupGroupId(%q): %v"u8, tt.sid, err);
            continue;
        }
        if ((~u).Gid != tt.sid) {
            Ꮡt.Errorf("unexpected gid for %q; got %q, want %q"u8, (~u).Name, (~u).Gid, tt.sid);
        }
    }
}

} // end user_internal_test_package
