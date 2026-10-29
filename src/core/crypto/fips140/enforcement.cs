// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using godebug = go.@internal.godebug_package;
/* blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) */ // for linkname
using go.@internal;

partial class fips140_package {

// WithoutEnforcement disables strict FIPS 140-3 enforcement while executing f.
// Calling WithoutEnforcement without strict enforcement enabled
// (GODEBUG=fips140=only is not set or already inside of a call to
// WithoutEnforcement) is a no-op.
//
// WithoutEnforcement is inherited by any goroutines spawned while executing f.
//
// As this disables enforcement, it should be applied carefully to tightly
// scoped functions.
public static void WithoutEnforcement(Action f) {
    GoFrame ᒐ = default;
    try {
        if (!Enabled() || !Enforced()) {
            f();
            return;
        }
        setBypass();
        defer(unsetBypass, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static bool enabled = godebug.New("fips140"u8).Value() == "only"u8;

// Enforced indicates if strict FIPS 140-3 enforcement is enabled. Strict
// enforcement is enabled when a program is run with GODEBUG=fips140=only and
// enforcement has not been disabled by a call to [WithoutEnforcement].
public static bool Enforced() {
    return enabled && !isBypassed();
}

//go:linkname setBypass
/*linkname*/ internal static partial void setBypass() {
    go.runtime_package.fips140_setBypass();
}

//go:linkname isBypassed
/*linkname*/ internal static partial bool isBypassed() {
    return go.runtime_package.fips140_isBypassed();
}

//go:linkname unsetBypass
/*linkname*/ internal static partial void unsetBypass() {
    go.runtime_package.fips140_unsetBypass();
}

} // end fips140_package
