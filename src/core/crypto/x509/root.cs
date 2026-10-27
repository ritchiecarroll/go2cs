// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using godebug = go.@internal.godebug_package;
using sync = sync_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using go.@internal;

partial class x509_package {

// systemRoots should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/breml/rootcerts
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname systemRoots
internal static readonly ж<sync.Once> Ꮡonce = new StaticBox<sync.Once>(default(sync.Once));
internal static ref sync.Once once => ref ((StaticBox<sync.Once>)Ꮡonce).ValueSlot;
internal static readonly ж<sync.RWMutex> ᏑsystemRootsMu = new StaticBox<sync.RWMutex>(default(sync.RWMutex));
internal static ref sync.RWMutex systemRootsMu => ref ((StaticBox<sync.RWMutex>)ᏑsystemRootsMu).ValueSlot;
public static ж<CertPool> systemRoots;
internal static error systemRootsErr;
internal static bool fallbacksSet;
internal static bool useFallbackRoots;

internal static ж<CertPool> systemRootsPool() {
    GoFrame ᒐ = default;
    try {
        Ꮡonce.Do(initSystemRoots);
        ᏑsystemRootsMu.RLock();
        defer(ᏑsystemRootsMu.RUnlock, ref ᒐ);
        return systemRoots;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static void initSystemRoots() {
    GoFrame ᒐ = default;
    try {
        ᏑsystemRootsMu.Lock();
        defer(ᏑsystemRootsMu.Unlock, ref ᒐ);

        var fallbackRoots = systemRoots;
        (systemRoots, systemRootsErr) = loadSystemRoots();
        if (systemRootsErr != default!) {
            systemRoots = default!;
        }

        if (fallbackRoots == nil) {
            return; // no fallbacks to try
        }

        var systemCertsAvail = systemRoots != nil && (systemRoots.len() > 0 || (~systemRoots).systemPool);

        if (!useFallbackRoots && systemCertsAvail) {
            return;
        }

        if (useFallbackRoots && systemCertsAvail) {
            x509usefallbackroots.IncNonDefault(); // overriding system certs with fallback certs.
        }

        (systemRoots, systemRootsErr) = (fallbackRoots, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static ж<godebug.Setting> x509usefallbackroots = godebug.New("x509usefallbackroots"u8);

// SetFallbackRoots sets the roots to use during certificate verification, if no
// custom roots are specified and a platform verifier or a system certificate
// pool is not available (for instance in a container which does not have a root
// certificate bundle). SetFallbackRoots will panic if roots is nil.
//
// SetFallbackRoots may only be called once, if called multiple times it will
// panic.
//
// The fallback behavior can be forced on all platforms, even when there is a
// system certificate pool, by setting GODEBUG=x509usefallbackroots=1 (note that
// on Windows and macOS this will disable usage of the platform verification
// APIs and cause the pure Go verifier to be used). Setting
// x509usefallbackroots=1 without calling SetFallbackRoots has no effect.
public static void SetFallbackRoots(ж<CertPool> Ꮡroots) {
    GoFrame ᒐ = default;
    try {
        ref var roots = ref Ꮡroots.DerefOrNull();

        if (Ꮡroots == nil) {
            throw panic("roots must be non-nil");
        }

        ᏑsystemRootsMu.Lock();
        defer(ᏑsystemRootsMu.Unlock, ref ᒐ);

        if (fallbacksSet) {
            throw panic("SetFallbackRoots has already been called");
        }
        fallbacksSet = true;

        // Handle case when initSystemRoots was not yet executed.
        // We handle that specially instead of calling loadSystemRoots, to avoid
        // spending excessive amount of cpu here, since the SetFallbackRoots in most cases
        // is going to be called at program startup.
        if (systemRoots == nil && systemRootsErr == default!) {
            systemRoots = Ꮡroots;
            useFallbackRoots = x509usefallbackroots.Value() == "1"u8;
            return;
        }

        Ꮡonce.Do(() => {
            throw panic("unreachable");
        }); // asserts that system roots were indeed loaded before.

        var forceFallbackRoots = x509usefallbackroots.Value() == "1"u8;
        var systemCertsAvail = systemRoots != nil && (systemRoots.len() > 0 || (~systemRoots).systemPool);

        if (!forceFallbackRoots && systemCertsAvail) {
            return;
        }

        if (forceFallbackRoots && systemCertsAvail) {
            x509usefallbackroots.IncNonDefault(); // overriding system certs with fallback certs.
        }

        (systemRoots, systemRootsErr) = (Ꮡroots, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end x509_package
