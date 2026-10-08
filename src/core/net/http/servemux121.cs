// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

// This file implements ServeMux behavior as in Go 1.21.
// The behavior is controlled by a GODEBUG setting.
// Most of this code is derived from commit 08e35cc334.
// Changes are minimal: aside from the different receiver type,
// they mostly involve renaming functions, usually by unexporting them.
// servemux121.go exists solely to provide a snapshot of
// the pre-Go 1.22 ServeMux implementation for backwards compatibility.
// Do not modify this file, it should remain frozen.
using godebug = go.@internal.godebug_package;
using url = go.net.url_package;
using sort = sort_package;
using strings = strings_package;
using sync = sync_package;
using go.@internal;
using go.net;

partial class http_package {

internal static ж<godebug.Setting> httpmuxgo121 = godebug.New("httpmuxgo121"u8);

internal static bool use121;

// Read httpmuxgo121 once at startup, since dealing with changes to it during
// program execution is too complex and error-prone.
[GoInit] internal static void initΔ1() {
    if (httpmuxgo121.Value() == "1"u8) {
        use121 = true;
        httpmuxgo121.IncNonDefault();
    }
}

// serveMux121 holds the state of a ServeMux needed for Go 1.21 behavior.
partial struct serveMux121 {
    internal sync.RWMutex mu;
    internal map<@string, muxEntry> m;
    internal slice<muxEntry> es; // slice of entries sorted from longest to shortest.
    internal bool hosts;       // whether any patterns contain hostnames
}

partial struct muxEntry {
    internal ΔHandler h;
    internal @string pattern;
}

// Formerly ServeMux.Handle.
internal static void handle(this ж<serveMux121> Ꮡmux, @string pattern, ΔHandler handler) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var mux = ref Ꮡmux.DerefOrNull();

        Ꮡmux.of(serveMux121.Ꮡmu).Lock();
        ᒐd1 = true;
        if (pattern == ""u8) {
            throw panic("http: invalid pattern");
        }
        if (handler == default!) {
            throw panic("http: nil handler");
        }
        {
            var (_, exist) = mux.m[pattern, ꟷ]; if (exist) {
                throw panic("http: multiple registrations for " + pattern);
            }
        }
        if (mux.m == default!) {
            mux.m = new map<@string, muxEntry>();
        }
        var e = new muxEntry(h: handler, pattern: pattern);
        mux.m[pattern] = e;
        if (pattern[builtin.len(pattern) - 1] == (rune)'/') {
            mux.es = appendSorted(mux.es, e);
        }
        if (pattern[0] != (rune)'/') {
            mux.hosts = true;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡmux.of(serveMux121.Ꮡmu).Unlock(); ᒐ.Run(); }
}

internal static slice<muxEntry> appendSorted(slice<muxEntry> es, muxEntry e) {
    nint n = builtin.len(es);
    var eʗ1 = e;
    nint i = sort.Search(n, (nint iΔ1) => builtin.len(es[iΔ1].pattern) < builtin.len(eʗ1.pattern));
    if (i == n) {
        return append(es, e);
    }
    // we now know that i points at where we want to insert
    es = append(es, new muxEntry(nil)); // try to grow the slice in place, any entry works.
    copy(es.slice(i + 1), es.slice(i)); // Move shorter entries down
    es[i] = e;
    return es;
}

// Formerly ServeMux.HandleFunc.
internal static void handleFunc(this ж<serveMux121> Ꮡmux, @string pattern, Action<ResponseWriter, ж<Request>> handler) {
    if (handler == default!) {
        throw panic("http: nil handler");
    }
    Ꮡmux.handle(pattern, new HandlerFuncᴠΔHandler(NilSafeDelegateConversion<HandlerFunc, Action<ResponseWriter, ж<Request>>>(handler)));
}

// Formerly ServeMux.Handler.
internal static (ΔHandler h, @string pattern) findHandler(this ж<serveMux121> Ꮡmux, ж<Request> Ꮡr) {
    @string pattern = default!;

    ref var mux = ref Ꮡmux.DerefOrNull();
    ref var r = ref Ꮡr.DerefOrNull();
    // CONNECT requests are not canonicalized.
    if (r.Method == "CONNECT"u8) {
        // If r.URL.Path is /tree and its handler is not registered,
        // the /tree -> /tree/ redirect applies to CONNECT requests
        // but the path canonicalization does not.
        {
            var (u, ok) = Ꮡmux.redirectToPathSlash((~r.URL).Host, (~r.URL).Path, r.URL); if (ok) {
                return (RedirectHandler(u.String(), StatusMovedPermanently), (~u).Path);
            }
        }
        return Ꮡmux.handler(r.Host, (~r.URL).Path);
    }
    // All other requests have any port stripped and path cleaned
    // before passing to mux.handler.
    @string host = stripHostPort(r.Host);
    ref var path = ref heap<@string>(out var Ꮡpath);
    path = cleanPath((~r.URL).Path);
    // If the given path is /tree and its handler is not registered,
    // redirect for /tree/.
    {
        var (u, ok) = Ꮡmux.redirectToPathSlash(host, path, r.URL); if (ok) {
            return (RedirectHandler(u.String(), StatusMovedPermanently), (~u).Path);
        }
    }
    if (path != (~r.URL).Path) {
        (_, pattern) = Ꮡmux.handler(host, path);
        var u = Ꮡ(new url.URL(Path: path, RawQuery: (~r.URL).RawQuery));
        return (RedirectHandler(u.String(), StatusMovedPermanently), pattern);
    }
    return Ꮡmux.handler(host, (~r.URL).Path);
}

// handler is the main implementation of findHandler.
// The path is known to be in canonical form, except for CONNECT methods.
internal static (ΔHandler h, @string pattern) handler(this ж<serveMux121> Ꮡmux, @string host, @string path) {
    ΔHandler h = default!;
    @string pattern = default!;
    GoFrame ᒐ = default;
    try {
        ref var mux = ref Ꮡmux.DerefOrNull();

        Ꮡmux.of(serveMux121.Ꮡmu).RLock();
        defer(Ꮡmux.of(serveMux121.Ꮡmu).RUnlock, ref ᒐ);
        // Host-specific pattern takes precedence over generic ones
        if (mux.hosts) {
            (h, pattern) = mux.match(host + path);
        }
        if (h == default!) {
            (h, pattern) = mux.match(path);
        }
        if (h == default!) {
            (h, pattern) = (NotFoundHandler(), "");
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (h, pattern);
}

// Find a handler on a handler map given a path string.
// Most-specific (longest) pattern wins.
internal static (ΔHandler h, @string pattern) match(this ref serveMux121 mux, @string path) {
    // Check for exact match first.
    var (v, ok) = mux.m[path, ꟷ];
    if (ok) {
        return (v.h, v.pattern);
    }
    // Check for longest valid match.  mux.es contains all patterns
    // that end in / sorted from longest to shortest.
    foreach (var (_, e) in mux.es) {
        if (strings.HasPrefix(path, e.pattern)) {
            return (e.h, e.pattern);
        }
    }
    return (default!, "");
}

// redirectToPathSlash determines if the given path needs appending "/" to it.
// This occurs when a handler for path + "/" was already registered, but
// not for path itself. If the path needs appending to, it creates a new
// URL, setting the path to u.Path + "/" and returning true to indicate so.
internal static (ж<url.URL>, bool) redirectToPathSlash(this ж<serveMux121> Ꮡmux, @string host, @string path, ж<url.URL> Ꮡu) {
    ref var mux = ref Ꮡmux.DerefOrNull();
    ref var u = ref Ꮡu.DerefOrNull();

    Ꮡmux.of(serveMux121.Ꮡmu).RLock();
    var shouldRedirect = mux.shouldRedirectRLocked(host, path);
    Ꮡmux.of(serveMux121.Ꮡmu).RUnlock();
    if (!shouldRedirect) {
        return (Ꮡu, false);
    }
    path = path + "/"u8;
    Ꮡu = Ꮡ(new url.URL(Path: path, RawQuery: u.RawQuery)); u = ref Ꮡu.DerefOrNull();
    return (Ꮡu, true);
}

// shouldRedirectRLocked reports whether the given path and host should be redirected to
// path+"/". This should happen if a handler is registered for path+"/" but
// not path -- see comments at ServeMux.
internal static bool shouldRedirectRLocked(this ref serveMux121 mux, @string host, @string path) {
    var p = new @string[]{path, host + path}.slice();
    foreach (var (_, c) in p) {
        {
            var (_, exist) = mux.m[c, ꟷ]; if (exist) {
                return false;
            }
        }
    }
    nint n = builtin.len(path);
    if (n == 0) {
        return false;
    }
    foreach (var (_, c) in p) {
        {
            var (_, exist) = mux.m[c + "/"u8, ꟷ]; if (exist) {
                return path[n - 1] != (rune)'/';
            }
        }
    }
    return false;
}

} // end http_package
