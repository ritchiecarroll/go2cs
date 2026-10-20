// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using errors = errors_package;
using fmt = fmt_package;
using url = go.net.url_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using go.net;
using go.sync;

partial class http_package {

// CrossOriginProtection implements protections against [Cross-Site Request
// Forgery (CSRF)] by rejecting non-safe cross-origin browser requests.
//
// Cross-origin requests are currently detected with the [Sec-Fetch-Site]
// header, available in all browsers since 2023, or by comparing the hostname of
// the [Origin] header with the Host header.
//
// The GET, HEAD, and OPTIONS methods are [safe methods] and are always allowed.
// It's important that applications do not perform any state changing actions
// due to requests with safe methods.
//
// Requests without Sec-Fetch-Site or Origin headers are currently assumed to be
// either same-origin or non-browser requests, and are allowed.
//
// The zero value of CrossOriginProtection is valid and has no trusted origins
// or bypass patterns.
//
// [Sec-Fetch-Site]: https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Sec-Fetch-Site
// [Origin]: https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Origin
// [Cross-Site Request Forgery (CSRF)]: https://developer.mozilla.org/en-US/docs/Web/Security/Attacks/CSRF
// [safe methods]: https://developer.mozilla.org/en-US/docs/Glossary/Safe/HTTP
partial struct CrossOriginProtection {
    internal atomic.Pointer<ServeMux> bypass;
    internal sync.RWMutex trustedMu;
    internal map<@string, bool> trusted;
    internal atomic.Pointer<ΔHandler> deny;
}

// NewCrossOriginProtection returns a new [CrossOriginProtection] value.
public static ж<CrossOriginProtection> NewCrossOriginProtection() {
    return Ꮡ(new CrossOriginProtection(nil));
}

// AddTrustedOrigin allows all requests with an [Origin] header
// which exactly matches the given value.
//
// Origin header values are of the form "scheme://host[:port]".
//
// AddTrustedOrigin can be called concurrently with other methods
// or request handling, and applies to future requests.
//
// [Origin]: https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Origin
public static error AddTrustedOrigin(this ж<CrossOriginProtection> Ꮡc, @string origin) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        var (u, err) = url.Parse(origin);
        if (err != default!) {
            return fmt.Errorf("invalid origin %q: %w"u8, origin, err);
        }
        if ((~u).Scheme == ""u8) {
            return fmt.Errorf("invalid origin %q: scheme is required"u8, origin);
        }
        if ((~u).Host == ""u8) {
            return fmt.Errorf("invalid origin %q: host is required"u8, origin);
        }
        if ((~u).Path != ""u8 || (~u).RawQuery != ""u8 || (~u).Fragment != ""u8) {
            return fmt.Errorf("invalid origin %q: path, query, and fragment are not allowed"u8, origin);
        }
        Ꮡc.of(CrossOriginProtection.ᏑtrustedMu).Lock();
        ᒐd1 = true;
        if (c.trusted == default!) {
            c.trusted = new map<@string, bool>();
        }
        c.trusted[origin] = true;
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.of(CrossOriginProtection.ᏑtrustedMu).Unlock(); ᒐ.Run(); }
}

partial struct noopHandler {
}

internal static void ServeHTTP(this noopHandler _Δp0, ResponseWriter _Δp1, ж<Request> _Δp2) {
}

internal static ΔHandler sentinelHandler = new noopHandlerжΔHandler(Ꮡ(new noopHandler(nil)));

// AddInsecureBypassPattern permits all requests that match the given pattern.
//
// The pattern syntax and precedence rules are the same as [ServeMux]. Only
// requests that match the pattern directly are permitted. Those that ServeMux
// would redirect to a pattern (e.g. after cleaning the path or adding a
// trailing slash) are not.
//
// AddInsecureBypassPattern panics if the pattern conflicts with one already
// registered, or if the pattern is syntactically invalid (for example, an
// improperly formed wildcard).
//
// AddInsecureBypassPattern can be called concurrently with other methods or
// request handling, and applies to future requests.
public static void AddInsecureBypassPattern(this ж<CrossOriginProtection> Ꮡc, @string pattern) {
    ж<ServeMux> bypass = default!;

    // Lazily initialize c.bypass
    while (ᐧ) {
        bypass = Ꮡc.of(CrossOriginProtection.Ꮡbypass).Load();
        if (bypass != nil) {
            break;
        }
        bypass = NewServeMux();
        if (Ꮡc.of(CrossOriginProtection.Ꮡbypass).CompareAndSwap(nil, bypass)) {
            break;
        }
    }

    bypass.Handle(pattern, sentinelHandler);
}

// SetDenyHandler sets a handler to invoke when a request is rejected.
// The default error handler responds with a 403 Forbidden status.
//
// SetDenyHandler can be called concurrently with other methods
// or request handling, and applies to future requests.
//
// Check does not call the error handler.
public static void SetDenyHandler(this ж<CrossOriginProtection> Ꮡc, ΔHandler hʗp) {
    ref var h = ref heap(hʗp, out var Ꮡh);

    if (h == default!) {
        Ꮡc.of(CrossOriginProtection.Ꮡdeny).Store(nil);
        return;
    }
    Ꮡc.of(CrossOriginProtection.Ꮡdeny).Store(Ꮡh);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string secFetchSiteˢ = "Sec-Fetch-Site"u8;
internal static readonly @string originˢ = "Origin"u8;

// Check applies cross-origin checks to a request.
// It returns an error if the request should be rejected.
public static error Check(this ж<CrossOriginProtection> Ꮡc, ж<Request> Ꮡreq) {
    ref var req = ref Ꮡreq.DerefOrNull();

    var exprᴛ1 = req.Method;
    if (exprᴛ1 == "GET"u8 || exprᴛ1 == "HEAD"u8 || exprᴛ1 == "OPTIONS"u8) {
        // Safe methods are always allowed.
        return default!;
    }

    var exprᴛ2 = req.Header.Get(secFetchSiteˢ);
    if (exprᴛ2 == ""u8) {
        // No Sec-Fetch-Site header is present.
        // Fallthrough to check the Origin header.
    }
    else if (exprᴛ2 == "same-origin"u8 || exprᴛ2 == "none"u8) {
        return default!;
    }
    else { /* default: */
        if (Ꮡc.isRequestExempt(Ꮡreq)) {
            return default!;
        }
        return errCrossOriginRequest;
    }

    @string origin = req.Header.Get(originˢ);
    if (origin == ""u8) {
        // Neither Sec-Fetch-Site nor Origin headers are present.
        // Either the request is same-origin or not a browser request.
        return default!;
    }

    {
        var (o, err) = url.Parse(origin); if (err == default! && (~o).Host == req.Host) {
            // The Origin header matches the Host header. Note that the Host header
            // doesn't include the scheme, so we don't know if this might be an
            // HTTP→HTTPS cross-origin request. We fail open, since all modern
            // browsers support Sec-Fetch-Site since 2023, and running an older
            // browser makes a clear security trade-off already. Sites can mitigate
            // this with HTTP Strict Transport Security (HSTS).
            return default!;
        }
    }

    if (Ꮡc.isRequestExempt(Ꮡreq)) {
        return default!;
    }
    return errCrossOriginRequestFromOldBrowser;
}

internal static error errCrossOriginRequest = errors.New("cross-origin request detected from Sec-Fetch-Site header"u8);
internal static error errCrossOriginRequestFromOldBrowser = errors.New("cross-origin request detected, and/or browser is out of date: "u8 + "Sec-Fetch-Site is missing, and Origin does not match Host"u8);

// isRequestExempt checks the bypasses which require taking a lock, and should
// be deferred until the last moment.
internal static bool isRequestExempt(this ж<CrossOriginProtection> Ꮡc, ж<Request> Ꮡreq) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();
        ref var req = ref Ꮡreq.DerefOrNull();

        {
            var bypass = Ꮡc.of(CrossOriginProtection.Ꮡbypass).Load(); if (bypass != nil) {
                {
                    var (h, _) = bypass.Handler(Ꮡreq); if (AreEqual(h, sentinelHandler)) {
                        // The request matches a bypass pattern.
                        return true;
                    }
                }
            }
        }

        Ꮡc.of(CrossOriginProtection.ᏑtrustedMu).RLock();
        ᒐd1 = true;
        @string origin = req.Header.Get(originˢ);
        // The request matches a trusted origin.
        return origin != ""u8 && c.trusted[origin];
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.of(CrossOriginProtection.ᏑtrustedMu).RUnlock(); ᒐ.Run(); }
}

// Handler returns a handler that applies cross-origin checks
// before invoking the handler h.
//
// If a request fails cross-origin checks, the request is rejected
// with a 403 Forbidden status or handled with the handler passed
// to [CrossOriginProtection.SetDenyHandler].
public static ΔHandler Handler(this ж<CrossOriginProtection> Ꮡc, ΔHandler h) {
    return new HandlerFuncᴠΔHandler(new HandlerFunc((ResponseWriter w, ж<Request> r) => {
        {
            var err = Ꮡc.Check(r); if (err != default!) {
                {
                    var deny = Ꮡc.of(CrossOriginProtection.Ꮡdeny).Load(); if (deny != nil) {
                        (deny.ValueSlot).ServeHTTP(w, r);
                        return;
                    }
                }
                Error(w, err.Error(), StatusForbidden);
                return;
            }
        }
        h.ServeHTTP(w, r);
    }));
}

} // end http_package
