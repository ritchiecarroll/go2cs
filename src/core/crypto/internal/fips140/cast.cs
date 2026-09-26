// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using godebug = go.crypto.@internal.fips140deps.godebug_package;
using errors = errors_package;
using strings = strings_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for go:linkname
using go.crypto.@internal.fips140deps;

partial class fips140_package {

// fatal is [runtime.fatal], pushed via linkname.
//
//go:linkname fatal crypto/internal/fips140.fatal
[global::System.Diagnostics.StackTraceHidden] internal static void fatal(@string _) {
    go.runtime_package.fips_fatal(_);
}

// failfipscast is a GODEBUG key allowing simulation of a CAST or PCT failure,
// as required during FIPS 140-3 functional testing. The value is the whole name
// of the target CAST or PCT.
internal static @string failfipscast = godebug.Value("#failfipscast"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string simulatedCastFailureˢ = "simulated CAST failure"u8;

// CAST runs the named Cryptographic Algorithm Self-Test (if operated in FIPS
// mode) and aborts the program (stopping the module input/output and entering
// the "error state") if the self-test fails.
//
// CASTs are mandatory self-checks that must be performed by FIPS 140-3 modules
// before the algorithm is used. See Implementation Guidance 10.3.A.
//
// The name must not contain commas, colons, hashes, or equal signs.
//
// If a package p calls CAST from its init function, an import of p should also
// be added to crypto/internal/fips140test. If a package p calls CAST on the first
// use of the algorithm, an invocation of that algorithm should be added to
// fipstest.TestConditionals.
public static void CAST(@string name, Func<error> f) {
    if (strings.ContainsAny(name, ",#=:"u8)) {
        throw panic("fips: invalid self-test name: " + name);
    }
    if (!Enabled) {
        return;
    }
    var err = f();
    if (name == failfipscast) {
        err = errors.New(simulatedCastFailureˢ);
    }
    if (err != default!) {
        fatal("FIPS 140-3 self-test failed: "u8 + name + ": "u8 + err.Error());
        throw panic("unreachable");
    }
    if (debug) {
        println((@string)"FIPS 140-3 self-test passed:"u8, name);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string simulatedPctFailureˢ = "simulated PCT failure"u8;

// PCT runs the named Pairwise Consistency Test (if operated in FIPS mode) and
// aborts the program (stopping the module input/output and entering the "error
// state") if the test fails.
//
// PCTs are mandatory for every generated (but not imported) key pair, including
// ephemeral keys (which effectively doubles the cost of key establishment). See
// Implementation Guidance 10.3.A Additional Comment 1.
//
// The name must not contain commas, colons, hashes, or equal signs.
//
// If a package p calls PCT during key generation, an invocation of that
// function should be added to fipstest.TestConditionals.
public static void PCT(@string name, Func<error> f) {
    if (strings.ContainsAny(name, ",#=:"u8)) {
        throw panic("fips: invalid self-test name: " + name);
    }
    if (!Enabled) {
        return;
    }
    var err = f();
    if (name == failfipscast) {
        err = errors.New(simulatedPctFailureˢ);
    }
    if (err != default!) {
        fatal("FIPS 140-3 self-test failed: "u8 + name + ": "u8 + err.Error());
        throw panic("unreachable");
    }
    if (debug) {
        println((@string)"FIPS 140-3 PCT passed:"u8, name);
    }
}

} // end fips140_package
