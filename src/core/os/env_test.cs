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
internal static readonly @string allTheArgsˢ = "all the args"u8;
internal static readonly @string nargsˢ = "NARGS"u8;
internal static readonly @string pidˢ = "PID"u8;
internal static readonly @string argument1ˢ = "ARGUMENT1"u8;
internal static readonly @string usrGopherˢ = "/usr/gopher"u8;
internal static readonly @string valueOfHˢ = "(Value of H)"u8;
internal static readonly @string usrFooˢ = "/usr/foo"u8;
internal static readonly @string underscoreˢ = "underscore"u8;

// testGetenv gives us a controlled set of variables for testing Expand.
internal static @string testGetenv(@string s) {
    var exprᴛ1 = s;
    if (exprᴛ1 == "*"u8) {
        return allTheArgsˢ;
    }
    if (exprᴛ1 == "#"u8) {
        return nargsˢ;
    }
    if (exprᴛ1 == "$"u8) {
        return pidˢ;
    }
    if (exprᴛ1 == "1"u8) {
        return argument1ˢ;
    }
    if (exprᴛ1 == "HOME"u8) {
        return usrGopherˢ;
    }
    if (exprᴛ1 == "H"u8) {
        return valueOfHˢ;
    }
    if (exprᴛ1 == "home_1"u8) {
        return usrFooˢ;
    }
    if (exprᴛ1 == "_"u8) {
        return underscoreˢ;
    }

    return ""u8;
}

// invalid syntax; eat up the characters
// invalid syntax; eat up the characters

[GoType("dyn")] partial struct expandTestsᴛ1 {
    internal @string @in, @out;
}
internal static slice<expandTestsᴛ1> expandTests = new expandTestsᴛ1[]{
    new(""u8, ""u8),
    new("$*"u8, "all the args"u8),
    new("$$"u8, "PID"u8),
    new("${*}"u8, "all the args"u8),
    new("$1"u8, "ARGUMENT1"u8),
    new("${1}"u8, "ARGUMENT1"u8),
    new("now is the time"u8, "now is the time"u8),
    new("$HOME"u8, "/usr/gopher"u8),
    new("$home_1"u8, "/usr/foo"u8),
    new("${HOME}"u8, "/usr/gopher"u8),
    new("${H}OME"u8, "(Value of H)OME"u8),
    new("A$$$#$1$H$home_1*B"u8, "APIDNARGSARGUMENT1(Value of H)/usr/foo*B"u8),
    new("start$+middle$^end$"u8, "start$+middle$^end$"u8),
    new("mixed$|bag$$$"u8, "mixed$|bagPID$"u8),
    new("$"u8, "$"u8),
    new("$}"u8, "$}"u8),
    new("${"u8, ""u8),
    new("${}"u8, ""u8)
}.slice();

public static void TestExpand(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, test) in expandTests) {
        @string result = Expand(test.@in, testGetenv);
        if (result != test.@out) {
            Ꮡt.Errorf("Expand(%q)=%q; expected %q"u8, test.@in, result, test.@out);
        }
    }
}

internal static any global;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noopˢ = "noop"u8;
internal static readonly @string tickTickTickTickˢ = "tick tick tick tick"u8;
internal static readonly @string multipleˢ = "multiple"u8;
internal static readonly @string aAAAˢ = "$a $a $a $a"u8;
internal static readonly @string boomˢ = "boom"u8;

public static void BenchmarkExpand(ж<Δtesting.B> Ꮡb) {
    Ꮡb.Run(noopˢ, (ж<Δtesting.B> bΔ1) => {
        @string s = default!;
        bΔ1.ReportAllocs();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            s = Expand(tickTickTickTickˢ, (@string _) => ""u8);
        }
        global = s;
    });
    Ꮡb.Run(multipleˢ, (ж<Δtesting.B> bΔ2) => {
        @string s = default!;
        bΔ2.ReportAllocs();
        for (nint i = 0; i < (~bΔ2).N; i++) {
            s = Expand(aAAAˢ, (@string _) => boomˢ);
        }
        global = s;
    });
}

public static void TestConsistentEnviron(ж<Δtesting.T> Ꮡt) {
    var e0 = Environ();
    for (nint i = 0; i < 10; i++) {
        var e1 = Environ();
        if (!slices.Equal<slice<@string>, @string>(e0, e1)) {
            Ꮡt.Fatalf("environment changed"u8);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object setenvDidnTSetˢ = (@string)"Setenv didn't set TestUnsetenv"u8;
internal static readonly object unsetenvDidnTClearˢ = (@string)"Unsetenv didn't clear TestUnsetenv"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string testKeyᶜ = "GO_TEST_UNSETENV"u8;

public static void TestUnsetenv(ж<Δtesting.T> Ꮡt) {
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
        var err = Setenv(testKey, "1"u8); if (err != default!) {
            Ꮡt.Fatalf("Setenv: %v"u8, err);
        }
    }
    if (!set()) {
        Ꮡt.Error(setenvDidnTSetˢ);
    }
    {
        var err = Unsetenv(testKey); if (err != default!) {
            Ꮡt.Fatalf("Unsetenv: %v"u8, err);
        }
    }
    if (set()) {
        Ꮡt.Fatal(unsetenvDidnTClearˢ);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string testKeyᶜ1 = "GO_TEST_CLEARENV"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string testValueᶜ = "1"u8;

public static void TestClearenv(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        @string testKey = testKeyᶜ1;
        @string testValue = testValueᶜ;
        // reset env
        defer((slice<@string> origEnv) => {
            foreach (var (_, pair) in origEnv) {
                // Environment variables on Windows can begin with =
                // https://devblogs.microsoft.com/oldnewthing/20100506-00/?p=14133
                nint i = strings.Index(pair[1..], "="u8) + 1;
                {
                    var err = Setenv(pair.slice(0, i), pair.slice(i + 1)); if (err != default!) {
                        Ꮡt.Errorf("Setenv(%q, %q) failed during reset: %v"u8, pair.slice(0, i), pair.slice(i + 1), err);
                    }
                }
            }
        }, Environ(), ref ᒐ);
        {
            var err = Setenv(testKey, testValue); if (err != default!) {
                Ꮡt.Fatalf("Setenv(%q, %q) failed: %v"u8, testKey, testValue, err);
            }
        }
        {
            var (_, ok) = LookupEnv(testKey); if (!ok) {
                Ꮡt.Errorf("Setenv(%q, %q) didn't set $%s"u8, testKey, testValue, testKey);
            }
        }
        Clearenv();
        {
            var (val, ok) = LookupEnv(testKey); if (ok) {
                Ꮡt.Errorf("Clearenv() didn't clear $%s, remained with value %q"u8, testKey, val);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string virusˢ = "virus"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string smallpoxᶜ = "SMALLPOX"u8;

public static void TestLookupEnv(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string smallpox = smallpoxᶜ; // No one has smallpox.
        var (value, ok) = LookupEnv(smallpox); // Should not exist.
        if (ok || value != ""u8) {
            Ꮡt.Fatalf("%s=%q"u8, smallpox, value);
        }
        defer(Unsetenv, (@string)smallpox, ref ᒐ);
        var err = Setenv(smallpox, virusˢ);
        if (err != default!) {
            Ꮡt.Fatalf("failed to release smallpox virus"u8);
        }
        (_, ok) = LookupEnv(smallpox);
        if (!ok) {
            Ꮡt.Errorf("smallpox release failed; world remains safe but LookupEnv is broken"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// On Windows, Environ was observed to report keys with a single leading "=".
// Check that they are properly reported by LookupEnv and can be set by SetEnv.
// See https://golang.org/issue/49886.
public static void TestEnvironConsistency(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    foreach (var (_, kv) in Environ()) {
        nint i = strings.Index(kv, "="u8);
        if (i == 0) {
            // We observe in practice keys with a single leading "=" on Windows.
            // TODO(#49886): Should we consume only the first leading "=" as part
            // of the key, or parse through arbitrarily many of them until a non-=,
            // or try each possible key/value boundary until LookupEnv succeeds?
            i = strings.Index(kv[1..], "="u8) + 1;
        }
        if (i < 0) {
            Ꮡt.Errorf("Environ entry missing '=': %q"u8, kv);
        }
        @string k = kv.slice(0, i);
        @string v = kv.slice(i + 1);
        var (v2, ok) = LookupEnv(k);
        if (ok && v == v2){
            Ꮡt.Logf("LookupEnv(%q) = %q, %t"u8, k, v2, ok);
        } else {
            Ꮡt.Errorf("Environ contains %q, but LookupEnv(%q) = %q, %t"u8, kv, k, v2, ok);
        }
        // Since k=v is already present in the environment,
        // setting it should be a no-op.
        {
            var err = Setenv(k, v); if (err == default!){
                Ꮡt.Logf("Setenv(%q, %q)"u8, k, v);
            } else {
                Ꮡt.Errorf("Environ contains %q, but SetEnv(%q, %q) = %q"u8, kv, k, v, err);
            }
        }
    }
}

} // end os_test_package
