// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static os_package;
using slices = slices_package;
using strings = strings_package;
using Δtesting = testing_package;
using static go.os_internal_test_package;

partial class os_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string allTheArgsˢ => ref allTheArgsˢᵇ;
private static readonly @string allTheArgsˢᵇ = "all the args"u8;
internal static ref readonly @string nargsˢ => ref nargsˢᵇ;
private static readonly @string nargsˢᵇ = "NARGS"u8;
internal static ref readonly @string pidˢ => ref pidˢᵇ;
private static readonly @string pidˢᵇ = "PID"u8;
internal static ref readonly @string argument1ˢ => ref argument1ˢᵇ;
private static readonly @string argument1ˢᵇ = "ARGUMENT1"u8;
internal static ref readonly @string usrGopherˢ => ref usrGopherˢᵇ;
private static readonly @string usrGopherˢᵇ = "/usr/gopher"u8;
internal static ref readonly @string valueOfHˢ => ref valueOfHˢᵇ;
private static readonly @string valueOfHˢᵇ = "(Value of H)"u8;
internal static ref readonly @string usrFooˢ => ref usrFooˢᵇ;
private static readonly @string usrFooˢᵇ = "/usr/foo"u8;
internal static ref readonly @string underscoreˢ => ref underscoreˢᵇ;
private static readonly @string underscoreˢᵇ = "underscore"u8;

// testGetenv gives us a controlled set of variables for testing Expand.
internal static @string testGetenv(@string s) {
    var exprᴛ1 = s;
    switch (ᐧ) {
    case {} when exprᴛ1 == "*"u8: {
        return allTheArgsˢ;
    }
    case {} when exprᴛ1 == "#"u8: {
        return nargsˢ;
    }
    case {} when exprᴛ1 == "$"u8: {
        return pidˢ;
    }
    case {} when exprᴛ1 == "1"u8: {
        return argument1ˢ;
    }
    case {} when exprᴛ1 == "HOME"u8: {
        return usrGopherˢ;
    }
    case {} when exprᴛ1 == "H"u8: {
        return valueOfHˢ;
    }
    case {} when exprᴛ1 == "home_1"u8: {
        return usrFooˢ;
    }
    case {} when exprᴛ1 == "_"u8: {
        return underscoreˢ;
    }}
    return ""u8;
}

partial struct expandTestsᴛ1 /*dyn*/ {
    internal @string @in, @out;
}
internal static slice<expandTestsᴛ1> expandTests = slice<expandTestsᴛ1>([
    new("", ""),
    new("$*", "all the args"),
    new("$$", "PID"),
    new("${*}", "all the args"),
    new("$1", "ARGUMENT1"),
    new("${1}", "ARGUMENT1"),
    new("now is the time", "now is the time"),
    new("$HOME", "/usr/gopher"),
    new("$home_1", "/usr/foo"),
    new("${HOME}", "/usr/gopher"),
    new("${H}OME", "(Value of H)OME"),
    new("A$$$#$1$H$home_1*B", "APIDNARGSARGUMENT1(Value of H)/usr/foo*B"),
    new("start$+middle$^end$", "start$+middle$^end$"),
    new("mixed$|bag$$$", "mixed$|bagPID$"),
    new("$", "$"),
    new("$}", "$}"),
    new("${", ""), // invalid syntax; eat up the characters
    new("${}", "") // invalid syntax; eat up the characters
]);

public static void TestExpand(ж<Δtesting.T> t) {
    foreach (var (_, test) in expandTests) {
        @string result = Expand(test.@in, testGetenv);
        if (result != test.@out) {
            t.Errorf("Expand(%q)=%q; expected %q", test.@in, result, test.@out);
        }
    }
}

internal static any global;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string noopˢ => ref noopˢᵇ;
private static readonly @string noopˢᵇ = "noop"u8;
internal static ref readonly @string tickTickTickTickˢ => ref tickTickTickTickˢᵇ;
private static readonly @string tickTickTickTickˢᵇ = "tick tick tick tick"u8;
internal static ref readonly @string multipleˢ => ref multipleˢᵇ;
private static readonly @string multipleˢᵇ = "multiple"u8;
internal static ref readonly @string aAAAˢ => ref aAAAˢᵇ;
private static readonly @string aAAAˢᵇ = "$a $a $a $a"u8;
internal static ref readonly @string boomˢ => ref boomˢᵇ;
private static readonly @string boomˢᵇ = "boom"u8;

public static void BenchmarkExpand(ж<Δtesting.B> b) {
    b.Run(noopˢ, (ж<Δtesting.B> bΔ1) => {
        @string s = default;
        bΔ1.ReportAllocs();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            s = Expand(tickTickTickTickˢ, (@string _) => ""u8);
        }
        global = s;
    });
    b.Run(multipleˢ, (ж<Δtesting.B> bΔ2) => {
        @string s = default;
        bΔ2.ReportAllocs();
        for (nint i = 0; i < (~bΔ2).N; i++) {
            s = Expand(aAAAˢ, (@string _) => boomˢ);
        }
        global = s;
    });
}

public static void TestConsistentEnviron(ж<Δtesting.T> t) {
    var e0 = Environ();
    for (nint i = 0; i < 10; i++) {
        var e1 = Environ();
        if (!slices.Equal<slice<@string>, @string>(e0, e1)) {
            t.Fatalf("environment changed");
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object setenvDidnTSetˢ = (@string)"Setenv didn't set TestUnsetenv"u8;
internal static readonly object unsetenvDidnTClearˢ = (@string)"Unsetenv didn't clear TestUnsetenv"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string testKeyᶜ => ref testKeyᶜᵇ;
private static readonly @string testKeyᶜᵇ = "GO_TEST_UNSETENV"u8;

public static void TestUnsetenv(ж<Δtesting.T> t) {
    @string testKey = testKeyᶜ;
    bool set() {
        @string prefix = testKey + "=";
        foreach (var (_, key) in Environ()) {
            if (strings.HasPrefix(key, prefix)) {
                return true;
            }
        }
        return false;
    }
    {
        var err = Setenv(testKey, "1"); if (err != nil) {
            t.Fatalf("Setenv: %v", err);
        }
    }
    if (!set()) {
        t.Error(setenvDidnTSetˢ);
    }
    {
        var err = Unsetenv(testKey); if (err != nil) {
            t.Fatalf("Unsetenv: %v", err);
        }
    }
    if (set()) {
        t.Fatal(unsetenvDidnTClearˢ);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string testKeyᶜ1 => ref testKeyᶜ1ᵇ;
private static readonly @string testKeyᶜ1ᵇ = "GO_TEST_CLEARENV"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string testValueᶜ => ref testValueᶜᵇ;
private static readonly @string testValueᶜᵇ = "1"u8;

public static void TestClearenv(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        @string testKey = testKeyᶜ1;
        @string testValue = testValueᶜ;

        // reset env
        defer((slice<@string> origEnv) => {
            foreach (var (_, pair) in origEnv) {
                // Environment variables on Windows can begin with =
                // https://devblogs.microsoft.com/oldnewthing/20100506-00/?p=14133
                nint i = strings.Index(pair[1..], "=") + 1;
                {
                    var err = Setenv(pair.slice(0, i), pair.slice(i + 1)); if (err != nil) {
                        t.Errorf("Setenv(%q, %q) failed during reset: %v", pair.slice(0, i), pair.slice(i + 1), err);
                    }
                }
            }
        }, Environ(), ref ᒐ);

        {
            var err = Setenv(testKey, testValue); if (err != nil) {
                t.Fatalf("Setenv(%q, %q) failed: %v", testKey, testValue, err);
            }
        }
        {
            var (_, ok) = LookupEnv(testKey); if (!ok) {
                t.Errorf("Setenv(%q, %q) didn't set $%s", testKey, testValue, testKey);
            }
        }
        Clearenv();
        {
            var (val, ok) = LookupEnv(testKey); if (ok) {
                t.Errorf("Clearenv() didn't clear $%s, remained with value %q", testKey, val);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string virusˢ => ref virusˢᵇ;
private static readonly @string virusˢᵇ = "virus"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string smallpoxᶜ => ref smallpoxᶜᵇ;
private static readonly @string smallpoxᶜᵇ = "SMALLPOX"u8;

public static void TestLookupEnv(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        @string smallpox = smallpoxᶜ;          // No one has smallpox.
        var (value, ok) = LookupEnv(smallpox); // Should not exist.
        if (ok || value != ""u8) {
            t.Fatalf("%s=%q", smallpox, value);
        }
        defer(Unsetenv, (@string)smallpox, ref ᒐ);
        var err = Setenv(smallpox, virusˢ);
        if (err != nil) {
            t.Fatalf("failed to release smallpox virus");
        }
        (_, ok) = LookupEnv(smallpox);
        if (!ok) {
            t.Errorf("smallpox release failed; world remains safe but LookupEnv is broken");
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// On Windows, Environ was observed to report keys with a single leading "=".
// Check that they are properly reported by LookupEnv and can be set by SetEnv.
// See https://golang.org/issue/49886.
public static void TestEnvironConsistency(ж<Δtesting.T> t) {
    t.Parallel();

    foreach (var (_, kv) in Environ()) {
        nint i = strings.Index(kv, "=");
        if (i == 0) {
            // We observe in practice keys with a single leading "=" on Windows.
            // TODO(#49886): Should we consume only the first leading "=" as part
            // of the key, or parse through arbitrarily many of them until a non-=,
            // or try each possible key/value boundary until LookupEnv succeeds?
            i = strings.Index(kv[1..], "=") + 1;
        }
        if (i < 0) {
            t.Errorf("Environ entry missing '=': %q", kv);
        }

        @string k = kv.slice(0, i);
        @string v = kv.slice(i + 1);
        var (v2, ok) = LookupEnv(k);
        if (!ok || v != v2) {
            t.Errorf("Environ contains %q, but LookupEnv(%q) = %q, %t", kv, k, v2, ok);
        }

        // Since k=v is already present in the environment,
        // setting it should be a no-op.
        {
            var err = Setenv(k, v); if (err != nil) {
                t.Errorf("Environ contains %q, but SetEnv(%q, %q) = %q", kv, k, v, err);
            }
        }
    }
}

} // end os_test_package
