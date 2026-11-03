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
internal static bool hasUSER = os.Getenv("USER") != ""u8;
internal static bool hasHOME = os.Getenv("HOME") != ""u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object userNotImplementedˢ = (@string)"user: not implemented; skipping tests"u8;

internal static partial void checkUser(ж<testing.T> t) {
    t.Helper();
    if (!userImplemented) {
        t.Skip(userNotImplementedˢ);
    }
}

public static void TestCurrent(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        nint old = userBuffer;
        defer(() => {
            userBuffer = old;
        }, ref ᒐ);
        userBuffer = 1; // force use of retry code
        var (u, err) = Current();
        if (err != nil) {
            if (hasCgo || (hasUSER && hasHOME)) {
                t.Fatalf("Current: %v (got %#v)", err, u.OrTypedNil());
            } else {
                t.Skipf("skipping: %v", err);
            }
        }
        if (u.HomeDir == ""u8) {
            t.Errorf("didn't get a HomeDir");
        }
        if (u.Username == ""u8) {
            t.Errorf("didn't get a username");
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

internal static void compare(ж<testing.T> t, ж<global::go.os.user_package.User> Ꮡwant, ж<global::go.os.user_package.User> Ꮡgot) {
    ref var want = ref Ꮡwant.DerefOrNull();
    ref var got = ref Ꮡgot.DerefOrNull();

    if (want.Uid != got.Uid) {
        t.Errorf("got Uid=%q; want %q", got.Uid, want.Uid);
    }
    if (want.Username != got.Username) {
        t.Errorf("got Username=%q; want %q", got.Username, want.Username);
    }
    if (want.Name != got.Name) {
        t.Errorf("got Name=%q; want %q", got.Name, want.Name);
    }
    if (want.HomeDir != got.HomeDir) {
        t.Errorf("got HomeDir=%q; want %q", got.HomeDir, want.HomeDir);
    }
    if (want.Gid != got.Gid) {
        t.Errorf("got Gid=%q; want %q", got.Gid, want.Gid);
    }
}

public static void TestLookup(ж<testing.T> t) {
    checkUser(t);

    var (want, err) = Current();
    if (err != nil) {
        if (hasCgo || (hasUSER && hasHOME)) {
            t.Fatalf("Current: %v", err);
        } else {
            t.Skipf("skipping: %v", err);
        }
    }

    // TODO: Lookup() has a fast path that calls Current() and returns if the
    // usernames match, so this test does not exercise very much. It would be
    // good to try and test finding a different user than the current user.
    (var got, err) = Lookup(want.Username);
    if (err != nil) {
        t.Fatalf("Lookup: %v", err);
    }
    compare(t, want, got);
}

public static void TestLookupId(ж<testing.T> t) {
    checkUser(t);

    var (want, err) = Current();
    if (err != nil) {
        if (hasCgo || (hasUSER && hasHOME)) {
            t.Fatalf("Current: %v", err);
        } else {
            t.Skipf("skipping: %v", err);
        }
    }

    (var got, err) = LookupId(want.Uid);
    if (err != nil) {
        t.Fatalf("LookupId: %v", err);
    }
    compare(t, want, got);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object userGroupNotImplementedˢ = (@string)"user: group not implemented; skipping test"u8;

internal static partial void checkGroup(ж<testing.T> t) {
    t.Helper();
    if (!groupImplemented) {
        t.Skip(userGroupNotImplementedˢ);
    }
}

public static void TestLookupGroup(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        nint old = groupBuffer;
        defer(() => {
            groupBuffer = old;
        }, ref ᒐ);
        groupBuffer = 1; // force use of retry code
        checkGroup(t);

        var (user, err) = Current();
        if (err != nil) {
            if (hasCgo || (hasUSER && hasHOME)) {
                t.Fatalf("Current: %v", err);
            } else {
                t.Skipf("skipping: %v", err);
            }
        }

        (var g1, err) = LookupGroupId(user.Gid);
        if (err != nil) {
            // NOTE(rsc): Maybe the group isn't defined. That's fine.
            // On my OS X laptop, rsc logs in with group 5000 even
            // though there's no name for group 5000. Such is Unix.
            t.Logf("LookupGroupId(%q): %v", user.Gid, err);
            return;
        }
        if (g1.Gid != user.Gid) {
            t.Errorf("LookupGroupId(%q).Gid = %s; want %s", user.Gid, g1.Gid, user.Gid);
        }

        (var g2, err) = LookupGroup(g1.Name);
        if (err != nil) {
            t.Fatalf("LookupGroup(%q): %v", g1.Name, err);
        }
        if (g1.Gid != g2.Gid || g1.Name != g2.Name) {
            t.Errorf("LookupGroup(%q) = %+v; want %+v", g1.Name, g2.OrTypedNil(), g1.OrTypedNil());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object userGroupListNotˢ = (@string)"user: group list not implemented; skipping test"u8;

internal static partial void checkGroupList(ж<testing.T> t) {
    t.Helper();
    if (!groupListImplemented) {
        t.Skip(userGroupListNotˢ);
    }
}

public static void TestGroupIds(ж<testing.T> t) {
    checkGroupList(t);

    var (user, err) = Current();
    if (err != nil) {
        if (hasCgo || (hasUSER && hasHOME)) {
            t.Fatalf("Current: %v", err);
        } else {
            t.Skipf("skipping: %v", err);
        }
    }

    (var gids, err) = user.GroupIds();
    if (err != nil) {
        t.Fatalf("%+v.GroupIds(): %v", user.OrTypedNil(), err);
    }
    if (!slices.Contains(gids, user.Gid)) {
        t.Errorf("%+v.GroupIds() = %v; does not contain user GID %s", user.OrTypedNil(), gids, user.Gid);
    }
}

} // end user_internal_test_package
