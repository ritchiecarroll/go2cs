// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cookiejar implements an in-memory RFC 6265-compliant http.CookieJar.
namespace go.net.http;

using cmp = cmp_package;
using errors = errors_package;
using fmt = fmt_package;
using net = net_package;
using http = go.net.http_package;
using ascii = go.net.http.@internal.ascii_package;
using url = go.net.url_package;
using slices = slices_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;
using go.net;
using go.net.http.@internal;

partial class cookiejar_package {

// PublicSuffixList provides the public suffix of a domain. For example:
//   - the public suffix of "example.com" is "com",
//   - the public suffix of "foo1.foo2.foo3.co.uk" is "co.uk", and
//   - the public suffix of "bar.pvt.k12.ma.us" is "pvt.k12.ma.us".
//
// Implementations of PublicSuffixList must be safe for concurrent use by
// multiple goroutines.
//
// An implementation that always returns "" is valid and may be useful for
// testing but it is not secure: it means that the HTTP server for foo.com can
// set a cookie for bar.com.
//
// A public suffix list implementation is in the package
// golang.org/x/net/publicsuffix.
[GoType] partial interface PublicSuffixList :
    fmt.Stringer
{
    // PublicSuffix returns the public suffix of domain.
    //
    // TODO: specify which of the caller and callee is responsible for IP
    // addresses, for leading and trailing dots, for case sensitivity, and
    // for IDN/Punycode.
    @string PublicSuffix(@string domain);
}

// Options are the options for creating a new Jar.
[GoType] partial struct Options {
    // PublicSuffixList is the public suffix list that determines whether
    // an HTTP server can set a cookie for a domain.
    //
    // A nil value is valid and may be useful for testing but it is not
    // secure: it means that the HTTP server for foo.co.uk can set a cookie
    // for bar.co.uk.
    public PublicSuffixList PublicSuffixList;
}

// Jar implements the http.CookieJar interface from the net/http package.
[GoType] partial struct Jar {
    internal PublicSuffixList psList;
    // mu locks the remaining fields.
    internal sync.Mutex mu;
    // entries is a set of entries, keyed by their eTLD+1 and subkeyed by
    // their name/domain/path.
    internal map<@string, map<@string, entry>> entries;
    // nextSeqNum is the next sequence number assigned to a new cookie
    // created SetCookies.
    internal uint64 nextSeqNum;
}

// New returns a new cookie jar. A nil [*Options] is equivalent to a zero
// Options.
public static (ж<Jar>, error) New(ж<Options> Ꮡo) {
    ref var o = ref Ꮡo.DerefOrNull();

    var jar = Ꮡ(new Jar(
        entries: new map<@string, map<@string, entry>>()
    ));
    if (Ꮡo != nil) {
        jar.Value.psList = o.PublicSuffixList;
    }
    return (jar, default!);
}

// entry is the internal representation of a cookie.
//
// This struct type is not used outside of this package per se, but the exported
// fields are those of RFC 6265.
[GoType] partial struct entry {
    public @string Name;
    public @string Value;
    public bool Quoted;
    public @string Domain;
    public @string Path;
    public @string SameSite;
    public bool Secure;
    public bool HttpOnly;
    public bool Persistent;
    public bool HostOnly;
    public time.Time Expires;
    public time.Time Creation;
    public time.Time LastAccess;
    // seqNum is a sequence number so that Cookies returns cookies in a
    // deterministic order, even for cookies that have equal Path length and
    // equal Creation time. This simplifies testing.
    internal uint64 seqNum;
}

// id returns the domain;path;name triple of e as an id.
[GoRecv] internal static @string id(this ref entry e) {
    return fmt.Sprintf("%s;%s;%s"u8, e.Domain, e.Path, e.Name);
}

// shouldSend determines whether e's cookie qualifies to be included in a
// request to host/path. It is the caller's responsibility to check if the
// cookie is expired.
[GoRecv] internal static bool shouldSend(this ref entry e, bool https, @string host, @string path) {
    return e.domainMatch(host) && e.pathMatch(path) && (https || !e.Secure);
}

// domainMatch checks whether e's Domain allows sending e back to host.
// It differs from "domain-match" of RFC 6265 section 5.1.3 because we treat
// a cookie with an IP address in the Domain always as a host cookie.
[GoRecv] internal static bool domainMatch(this ref entry e, @string host) {
    if (e.Domain == host) {
        return true;
    }
    return !e.HostOnly && hasDotSuffix(host, e.Domain);
}

// pathMatch implements "path-match" according to RFC 6265 section 5.1.4.
[GoRecv] internal static bool pathMatch(this ref entry e, @string requestPath) {
    if (requestPath == e.Path) {
        return true;
    }
    if (strings.HasPrefix(requestPath, e.Path)) {
        if (e.Path[len(e.Path) - 1] == (rune)'/'){
            return true; // The "/any/" matches "/any/path" case.
        } else 
        if (requestPath[len(e.Path)] == (rune)'/') {
            return true; // The "/any" matches "/any/path" case.
        }
    }
    return false;
}

// hasDotSuffix reports whether s ends in "."+suffix.
internal static bool hasDotSuffix(@string s, @string suffix) {
    return len(s) > len(suffix) && s[len(s) - len(suffix) - 1] == (rune)'.' && s.slice(len(s) - len(suffix)) == suffix;
}

// Cookies implements the Cookies method of the [http.CookieJar] interface.
//
// It returns an empty slice if the URL's scheme is not HTTP or HTTPS.
public static slice<ж<httpꓸCookie>> /*cookies*/ Cookies(this ж<Jar> Ꮡj, ж<url.URL> Ꮡu) {
    return Ꮡj.cookies(Ꮡu, time.Now());
}

// cookies is like Cookies but takes the current time as a parameter.
internal static slice<ж<httpꓸCookie>> /*cookies*/ cookies(this ж<Jar> Ꮡj, ж<url.URL> Ꮡu, time.Time now) {
    slice<ж<httpꓸCookie>> cookies = default!;
    GoFrame ᒐ = default;
    try {
        ref var j = ref Ꮡj.DerefOrNull();
        ref var u = ref Ꮡu.DerefOrNull();

        if (u.Scheme != "http"u8 && u.Scheme != "https"u8) {
            goto ᒐdone;
        }
        var (host, err) = canonicalHost(u.Host);
        if (err != default!) {
            goto ᒐdone;
        }
        @string key = jarKey(host, j.psList);
        j.mu.Lock();
        defer(Ꮡj.of(Jar.Ꮡmu).Unlock, ref ᒐ);
        var submap = j.entries[key];
        if (submap == default!) {
            goto ᒐdone;
        }
        var https = u.Scheme == "https"u8;
        @string path = u.Path;
        if (path == ""u8) {
            path = "/"u8;
        }
        var modified = false;
        slice<entry> selected = default!;
        foreach (var (id, vᴛ1) in submap) {
            var e = vᴛ1;

            if (e.Persistent && !e.Expires.After(now)) {
                delete(submap, id);
                modified = true;
                continue;
            }
            if (!e.shouldSend(https, host, path)) {
                continue;
            }
            e.LastAccess = now;
            submap[id] = e;
            selected = append(selected, e);
            modified = true;
        }
        if (modified) {
            if (len(submap) == 0){
                delete(j.entries, key);
            } else {
                j.entries[key] = submap;
            }
        }
        // sort according to RFC 6265 section 5.4 point 2: by longest
        // path and then by earliest creation time.
        slices.SortFunc(selected, (entry a, entry b) => {
            {
                nint r = cmp.Compare(b.Path, a.Path); if (r != 0) {
                    return r;
                }
            }
            {
                nint r = a.Creation.Compare(b.Creation); if (r != 0) {
                    return r;
                }
            }
            return cmp.Compare(a.seqNum, b.seqNum);
        });
        foreach (var (_, e) in selected) {
            cookies = append(cookies, Ꮡ(new httpꓸCookie(Name: e.Name, Value: e.Value, Quoted: e.Quoted)));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return cookies;
}

// SetCookies implements the SetCookies method of the [http.CookieJar] interface.
//
// It does nothing if the URL's scheme is not HTTP or HTTPS.
public static void SetCookies(this ж<Jar> Ꮡj, ж<url.URL> Ꮡu, slice<ж<httpꓸCookie>> cookies) {
    Ꮡj.setCookies(Ꮡu, cookies, time.Now());
}

// setCookies is like SetCookies but takes the current time as parameter.
internal static void setCookies(this ж<Jar> Ꮡj, ж<url.URL> Ꮡu, slice<ж<httpꓸCookie>> cookies, time.Time now) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var j = ref Ꮡj.DerefOrNull();
        ref var u = ref Ꮡu.DerefOrNull();

        if (len(cookies) == 0) {
            return;
        }
        if (u.Scheme != "http"u8 && u.Scheme != "https"u8) {
            return;
        }
        var (host, err) = canonicalHost(u.Host);
        if (err != default!) {
            return;
        }
        @string key = jarKey(host, j.psList);
        @string defPath = defaultPath(u.Path);
        j.mu.Lock();
        ᒐd1 = true;
        var submap = j.entries[key];
        var modified = false;
        foreach (var (_, cookie) in cookies) {
            var (e, remove, errΔ1) = j.newEntry(cookie, now, defPath, host);
            if (errΔ1 != default!) {
                continue;
            }
            @string id = e.id();
            if (remove) {
                if (submap != default!) {
                    {
                        var (_, ok) = submap[id, ꟷ]; if (ok) {
                            delete(submap, id);
                            modified = true;
                        }
                    }
                }
                continue;
            }
            if (submap == default!) {
                submap = new map<@string, entry>();
            }
            {
                var (old, ok) = submap[id, ꟷ]; if (ok){
                    e.Creation = old.Creation;
                    e.seqNum = old.seqNum;
                } else {
                    e.Creation = now;
                    e.seqNum = j.nextSeqNum;
                    j.nextSeqNum++;
                }
            }
            e.LastAccess = now;
            submap[id] = e;
            modified = true;
        }
        if (modified) {
            if (len(submap) == 0){
                delete(j.entries, key);
            } else {
                j.entries[key] = submap;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡj.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// canonicalHost strips port from host if present and returns the canonicalized
// host name.
internal static (@string, error) canonicalHost(@string host) {
    error err = default!;
    if (hasPort(host)) {
        (host, _, err) = net.SplitHostPort(host);
        if (err != default!) {
            return ("", err);
        }
    }
    // Strip trailing dot from fully qualified domain names.
    host = strings.TrimSuffix(host, "."u8);
    (var encoded, err) = toASCII(host);
    if (err != default!) {
        return ("", err);
    }
    // We know this is ascii, no need to check.
    var (lower, _) = ascii.ToLower(encoded);
    return (lower, default!);
}

// hasPort reports whether host contains a port number. host may be a host
// name, an IPv4 or an IPv6 address.
internal static bool hasPort(@string host) {
    nint colons = strings.Count(host, ":"u8);
    if (colons == 0) {
        return false;
    }
    if (colons == 1) {
        return true;
    }
    return host[0] == (rune)'[' && strings.Contains(host, "]:"u8);
}

// jarKey returns the key to use for a jar.
internal static @string jarKey(@string host, PublicSuffixList psl) {
    if (isIP(host)) {
        return host;
    }
    nint i = default!;
    if (psl == default!){
        i = strings.LastIndex(host, "."u8);
        if (i <= 0) {
            return host;
        }
    } else {
        @string suffix = psl.PublicSuffix(host);
        if (suffix == host) {
            return host;
        }
        i = len(host) - len(suffix);
        if (i <= 0 || host[i - 1] != (rune)'.') {
            // The provided public suffix list psl is broken.
            // Storing cookies under host is a safe stopgap.
            return host;
        }
    }
    // Only len(suffix) is used to determine the jar key from
    // here on, so it is okay if psl.PublicSuffix("www.buggy.psl")
    // returns "com" as the jar key is generated from host.
    nint prevDot = strings.LastIndex(host.slice(0, i - 1), "."u8);
    return host.slice(prevDot + 1);
}

// isIP reports whether host is an IP address.
internal static bool isIP(@string host) {
    if (strings.ContainsAny(host, ":%"u8)) {
        // Probable IPv6 address.
        // Hostnames can't contain : or %, so this is definitely not a valid host.
        // Treating it as an IP is the more conservative option, and avoids the risk
        // of interpreting ::1%.www.example.com as a subdomain of www.example.com.
        return true;
    }
    return net.ParseIP(host) != default!;
}

// defaultPath returns the directory part of a URL's path according to
// RFC 6265 section 5.1.4.
internal static @string defaultPath(@string path) {
    if (len(path) == 0 || path[0] != (rune)'/') {
        return "/"u8; // Path is empty or malformed.
    }
    nint i = strings.LastIndex(path, "/"u8); // Path starts with "/", so i != -1.
    if (i == 0) {
        return "/"u8; // Path has the form "/abc".
    }
    return path.slice(0, i); // Path is either of form "/abc/xyz" or "/abc/xyz/".
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sameSiteˢ = "SameSite"u8;
internal static readonly @string sameSiteStrictˢ = "SameSite=Strict"u8;
internal static readonly @string sameSiteLaxˢ = "SameSite=Lax"u8;

// newEntry creates an entry from an http.Cookie c. now is the current time and
// is compared to c.Expires to determine deletion of c. defPath and host are the
// default-path and the canonical host name of the URL c was received from.
//
// remove records whether the jar should delete this cookie, as it has already
// expired with respect to now. In this case, e may be incomplete, but it will
// be valid to call e.id (which depends on e's Name, Domain and Path).
//
// A malformed c.Domain will result in an error.
[GoRecv] internal static (entry e, bool remove, error err) newEntry(this ref Jar j, ж<httpꓸCookie> Ꮡc, time.Time now, @string defPath, @string host) {
    entry e = default!;
    error err = default!;

    ref var c = ref Ꮡc.DerefOrNull();
    e.Name = c.Name;
    if (c.Path == ""u8 || c.Path[0] != (rune)'/'){
        e.Path = defPath;
    } else {
        e.Path = c.Path;
    }
    (e.Domain, e.HostOnly, err) = j.domainAndType(host, c.Domain);
    if (err != default!) {
        return (e, false, err);
    }
    // MaxAge takes precedence over Expires.
    if (c.MaxAge < 0){
        return (e, true, default!);
    } else 
    if (c.MaxAge > 0){
        e.Expires = now.Add(((time.Duration)(int64)c.MaxAge) * time.ΔSecond);
        e.Persistent = true;
    } else {
        if (c.Expires.IsZero()){
            e.Expires = endOfTime;
            e.Persistent = false;
        } else {
            if (!c.Expires.After(now)) {
                return (e, true, default!);
            }
            e.Expires = c.Expires;
            e.Persistent = true;
        }
    }
    e.Value = c.Value;
    e.Quoted = c.Quoted;
    e.Secure = c.Secure;
    e.HttpOnly = c.HttpOnly;
    var exprᴛ1 = c.SameSite;
    if (exprᴛ1 == http.SameSiteDefaultMode) {
        e.SameSite = sameSiteˢ;
    }
    else if (exprᴛ1 == http.SameSiteStrictMode) {
        e.SameSite = sameSiteStrictˢ;
    }
    else if (exprᴛ1 == http.SameSiteLaxMode) {
        e.SameSite = sameSiteLaxˢ;
    }

    return (e, false, default!);
}

internal static error errIllegalDomain = errors.New("cookiejar: illegal cookie domain attribute"u8);
internal static error errMalformedDomain = errors.New("cookiejar: malformed cookie domain attribute"u8);

// endOfTime is the time when session (non-persistent) cookies expire.
// This instant is representable in most date/time formats (not just
// Go's time.Time) and should be far enough in the future.
internal static time.Time endOfTime = time.Date(9999, 12, 31, 23, 59, 59, 0, time.ΔUTC);

// domainAndType determines the cookie's domain and hostOnly attribute.
[GoRecv] internal static (@string, bool, error) domainAndType(this ref Jar j, @string host, @string domain) {
    if (domain == ""u8) {
        // No domain attribute in the SetCookie header indicates a
        // host cookie.
        return (host, true, default!);
    }
    if (isIP(host)) {
        // RFC 6265 is not super clear here, a sensible interpretation
        // is that cookies with an IP address in the domain-attribute
        // are allowed.
        // RFC 6265 section 5.2.3 mandates to strip an optional leading
        // dot in the domain-attribute before processing the cookie.
        //
        // Most browsers don't do that for IP addresses, only curl
        // (version 7.54) and IE (version 11) do not reject a
        //     Set-Cookie: a=1; domain=.127.0.0.1
        // This leading dot is optional and serves only as hint for
        // humans to indicate that a cookie with "domain=.bbc.co.uk"
        // would be sent to every subdomain of bbc.co.uk.
        // It just doesn't make sense on IP addresses.
        // The other processing and validation steps in RFC 6265 just
        // collapse to:
        if (host != domain) {
            return ("", false, errIllegalDomain);
        }
        // According to RFC 6265 such cookies should be treated as
        // domain cookies.
        // As there are no subdomains of an IP address the treatment
        // according to RFC 6265 would be exactly the same as that of
        // a host-only cookie. Contemporary browsers (and curl) do
        // allows such cookies but treat them as host-only cookies.
        // So do we as it just doesn't make sense to label them as
        // domain cookies when there is no domain; the whole notion of
        // domain cookies requires a domain name to be well defined.
        return (host, true, default!);
    }
    // From here on: If the cookie is valid, it is a domain cookie (with
    // the one exception of a public suffix below).
    // See RFC 6265 section 5.2.3.
    domain = strings.TrimPrefix(domain, "."u8);
    if (len(domain) == 0 || domain[0] == (rune)'.') {
        // Received either "Domain=." or "Domain=..some.thing",
        // both are illegal.
        return ("", false, errMalformedDomain);
    }
    (domain, var isASCII) = ascii.ToLower(domain);
    if (!isASCII) {
        // Received non-ASCII domain, e.g. "perché.com" instead of "xn--perch-fsa.com"
        return ("", false, errMalformedDomain);
    }
    if (domain[len(domain) - 1] == (rune)'.') {
        // We received stuff like "Domain=www.example.com.".
        // Browsers do handle such stuff (actually differently) but
        // RFC 6265 seems to be clear here (e.g. section 4.1.2.3) in
        // requiring a reject.  4.1.2.3 is not normative, but
        // "Domain Matching" (5.1.3) and "Canonicalized Host Names"
        // (5.1.2) are.
        return ("", false, errMalformedDomain);
    }
    // See RFC 6265 section 5.3 #5.
    if (j.psList != default!) {
        {
            @string ps = j.psList.PublicSuffix(domain); if (ps != ""u8 && !hasDotSuffix(domain, ps)) {
                if (host == domain) {
                    // This is the one exception in which a cookie
                    // with a domain attribute is a host cookie.
                    return (host, true, default!);
                }
                return ("", false, errIllegalDomain);
            }
        }
    }
    // The domain must domain-match host: www.mycompany.com cannot
    // set cookies for .ourcompetitors.com.
    if (host != domain && !hasDotSuffix(host, domain)) {
        return ("", false, errIllegalDomain);
    }
    return (domain, false, default!);
}

} // end cookiejar_package
