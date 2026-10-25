// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.os;

using os = os_package;
using slices = slices_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.os.user_package;

partial class user_internal_test_package {

internal static bool hasCgo = false;
internal static bool hasUSER = os.Getenv("USER"u8) != ""u8;
internal static bool hasHOME = os.Getenv("HOME"u8) != ""u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object userNotImplementedˢ = (@string)"user: not implemented; skipping tests"u8;

internal static partial void checkUser(ж<testing.T> Ꮡt) {
    Ꮡt.Helper();
    if (!userImplemented) {
        Ꮡt.Skip(userNotImplementedˢ);
    }
}

public static void TestCurrent(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        nint old = userBuffer;
        defer(() => {
            userBuffer = old;
        }, ref ᒐ);
        userBuffer = 1; // force use of retry code
        var (u, err) = Current();
        if (err != default!) {
            if (hasCgo || (hasUSER && hasHOME)){
                Ꮡt.Fatalf("Current: %v (got %#v)"u8, err, u.OrTypedNil());
            } else {
                Ꮡt.Skipf("skipping: %v"u8, err);
            }
        }
        if ((~u).HomeDir == ""u8) {
            Ꮡt.Errorf("didn't get a HomeDir"u8);
        }
        if ((~u).Username == ""u8) {
            Ꮡt.Errorf("didn't get a username"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkCurrent(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Benchmark current instead of Current because Current caches the result.
    for (nint i = 0; i < b.N; i++) {
        current();
    }
}

internal static void compare(ж<testing.T> Ꮡt, ж<global::go.os.user_package.User> Ꮡwant, ж<global::go.os.user_package.User> Ꮡgot) {
    ref var want = ref Ꮡwant.DerefOrNull();
    ref var got = ref Ꮡgot.DerefOrNull();

    if (want.Uid != got.Uid) {
        Ꮡt.Errorf("got Uid=%q; want %q"u8, got.Uid, want.Uid);
    }
    if (want.Username != got.Username) {
        Ꮡt.Errorf("got Username=%q; want %q"u8, got.Username, want.Username);
    }
    if (want.Name != got.Name) {
        Ꮡt.Errorf("got Name=%q; want %q"u8, got.Name, want.Name);
    }
    if (want.HomeDir != got.HomeDir) {
        Ꮡt.Errorf("got HomeDir=%q; want %q"u8, got.HomeDir, want.HomeDir);
    }
    if (want.Gid != got.Gid) {
        Ꮡt.Errorf("got Gid=%q; want %q"u8, got.Gid, want.Gid);
    }
}

public static void TestLookup(ж<testing.T> Ꮡt) {
    checkUser(Ꮡt);

    var (want, err) = Current();
    if (err != default!) {
        if (hasCgo || (hasUSER && hasHOME)){
            Ꮡt.Fatalf("Current: %v"u8, err);
        } else {
            Ꮡt.Skipf("skipping: %v"u8, err);
        }
    }

    // TODO: Lookup() has a fast path that calls Current() and returns if the
    // usernames match, so this test does not exercise very much. It would be
    // good to try and test finding a different user than the current user.
    (var got, err) = Lookup((~want).Username);
    if (err != default!) {
        Ꮡt.Fatalf("Lookup: %v"u8, err);
    }
    compare(Ꮡt, want, got);
}

public static void TestLookupId(ж<testing.T> Ꮡt) {
    checkUser(Ꮡt);

    var (want, err) = Current();
    if (err != default!) {
        if (hasCgo || (hasUSER && hasHOME)){
            Ꮡt.Fatalf("Current: %v"u8, err);
        } else {
            Ꮡt.Skipf("skipping: %v"u8, err);
        }
    }

    (var got, err) = LookupId((~want).Uid);
    if (err != default!) {
        Ꮡt.Fatalf("LookupId: %v"u8, err);
    }
    compare(Ꮡt, want, got);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object userGroupNotImplementedˢ = (@string)"user: group not implemented; skipping test"u8;

internal static partial void checkGroup(ж<testing.T> Ꮡt) {
    Ꮡt.Helper();
    if (!groupImplemented) {
        Ꮡt.Skip(userGroupNotImplementedˢ);
    }
}

public static void TestLookupGroup(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        nint old = groupBuffer;
        defer(() => {
            groupBuffer = old;
        }, ref ᒐ);
        groupBuffer = 1; // force use of retry code
        checkGroup(Ꮡt);

        var (user, err) = Current();
        if (err != default!) {
            if (hasCgo || (hasUSER && hasHOME)){
                Ꮡt.Fatalf("Current: %v"u8, err);
            } else {
                Ꮡt.Skipf("skipping: %v"u8, err);
            }
        }

        (var g1, err) = LookupGroupId((~user).Gid);
        if (err != default!) {
            // NOTE(rsc): Maybe the group isn't defined. That's fine.
            // On my OS X laptop, rsc logs in with group 5000 even
            // though there's no name for group 5000. Such is Unix.
            Ꮡt.Logf("LookupGroupId(%q): %v"u8, (~user).Gid, err);
            return;
        }
        if ((~g1).Gid != (~user).Gid) {
            Ꮡt.Errorf("LookupGroupId(%q).Gid = %s; want %s"u8, (~user).Gid, (~g1).Gid, (~user).Gid);
        }

        (var g2, err) = LookupGroup((~g1).Name);
        if (err != default!) {
            Ꮡt.Fatalf("LookupGroup(%q): %v"u8, (~g1).Name, err);
        }
        if ((~g1).Gid != (~g2).Gid || (~g1).Name != (~g2).Name) {
            Ꮡt.Errorf("LookupGroup(%q) = %+v; want %+v"u8, (~g1).Name, g2.OrTypedNil(), g1.OrTypedNil());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object userGroupListNotˢ = (@string)"user: group list not implemented; skipping test"u8;

internal static partial void checkGroupList(ж<testing.T> Ꮡt) {
    Ꮡt.Helper();
    if (!groupListImplemented) {
        Ꮡt.Skip(userGroupListNotˢ);
    }
}

public static void TestGroupIds(ж<testing.T> Ꮡt) {
    checkGroupList(Ꮡt);

    var (user, err) = Current();
    if (err != default!) {
        if (hasCgo || (hasUSER && hasHOME)){
            Ꮡt.Fatalf("Current: %v"u8, err);
        } else {
            Ꮡt.Skipf("skipping: %v"u8, err);
        }
    }

    (var gids, err) = user.GroupIds();
    if (err != default!) {
        Ꮡt.Fatalf("%+v.GroupIds(): %v"u8, user.OrTypedNil(), err);
    }
    if (!slices.Contains(gids, (~user).Gid)) {
        Ꮡt.Errorf("%+v.GroupIds() = %v; does not contain user GID %s"u8, user.OrTypedNil(), gids, (~user).Gid);
    }
}

} // end user_internal_test_package
