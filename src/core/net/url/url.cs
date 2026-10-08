// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package url parses URLs and implements query escaping.
namespace go.net;

// See RFC 3986. This package generally follows RFC 3986, except where
// it deviates for compatibility reasons. When sending changes, first
// search old issues for history on decisions. Unit tests should also
// contain references to issue numbers with details.
using errors = errors_package;
using fmt = fmt_package;
using godebug = @internal.godebug_package;
using maps = maps_package;
using netip = go.net.netip_package;
using path = path_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using @internal;
using go.net;
using iter = iter_package;
using ꓸꓸꓸstring = Span<@string>;

partial class url_package {

// Error reports an error and the operation and URL that caused it.
partial struct ΔError {
    public @string Op;
    public @string URL;
    public error Err;
}

public static error Unwrap(this ref ΔError e) {
    return e.Err;
}

public static @string Error(this ref ΔError e) {
    return fmt.Sprintf("%s %q: %s"u8, e.Op, e.URL, e.Err);
}

internal partial interface Timeout_type /*dyn*/ {
    bool Timeout();
}

public static bool Timeout(this ref ΔError e) {
    var (t, ok) = e.Err._<Timeout_type>(ᐧ);
    return ok && t.Timeout();
}

internal partial interface Temporary_type /*dyn*/ {
    bool Temporary();
}

public static bool Temporary(this ref ΔError e) {
    var (t, ok) = e.Err._<Temporary_type>(ᐧ);
    return ok && t.Temporary();
}

internal static readonly @string upperhex = "0123456789ABCDEF"u8;

internal static bool ishex(byte c) {
    switch (ᐧ) {
    case {} when (rune)'0' <= c && c <= (rune)'9': {
        return true;
    }
    case {} when (rune)'a' <= c && c <= (rune)'f': {
        return true;
    }
    case {} when (rune)'A' <= c && c <= (rune)'F': {
        return true;
    }}

    return false;
}

internal static byte unhex(byte c) {
    switch (ᐧ) {
    case {} when (rune)'0' <= c && c <= (rune)'9': {
        return (byte)(c - (rune)'0');
    }
    case {} when (rune)'a' <= c && c <= (rune)'f': {
        return (byte)(c - (rune)'a' + 10);
    }
    case {} when (rune)'A' <= c && c <= (rune)'F': {
        return (byte)(c - (rune)'A' + 10);
    }}

    return 0;
}

partial struct encoding /*num:nint*/;

internal static encoding encodePath => /* 1 + iota */ 1;
internal static encoding encodePathSegment => 2;
internal static encoding encodeHost => 3;
internal static encoding encodeZone => 4;
internal static encoding encodeUserPassword => 5;
internal static encoding encodeQueryComponent => 6;
internal static encoding encodeFragment => 7;

partial struct EscapeError /*@string*/;

public static @string Error(this EscapeError e) {
    return "invalid URL escape "u8 + strconv.Quote(((@string)e));
}

partial struct InvalidHostError /*@string*/;

public static @string Error(this InvalidHostError e) {
    return "invalid character "u8 + strconv.Quote(((@string)e)) + " in host name"u8;
}

// Return true if the specified character should be escaped when
// appearing in a URL string, according to RFC 3986.
//
// Please be informed that for now shouldEscape does not check all
// reserved characters correctly. See golang.org/issue/5684.
internal static bool shouldEscape(byte c, encoding mode) {
    // §2.3 Unreserved characters (alphanum)
    if ((rune)'a' <= c && c <= (rune)'z' || (rune)'A' <= c && c <= (rune)'Z' || (rune)'0' <= c && c <= (rune)'9') {
        return false;
    }
    if (mode == encodeHost || mode == encodeZone) {
        // §3.2.2 Host allows
        //	sub-delims = "!" / "$" / "&" / "'" / "(" / ")" / "*" / "+" / "," / ";" / "="
        // as part of reg-name.
        // We add : because we include :port as part of host.
        // We add [ ] because we include [ipv6]:port as part of host.
        // We add < > because they're the only characters left that
        // we could possibly allow, and Parse will reject them if we
        // escape them (because hosts can't use %-encoding for
        // ASCII bytes).
        switch (c) {
        case (rune)'!' or (rune)'$' or (rune)'&' or (rune)'\'' or (rune)'(' or (rune)')' or (rune)'*' or (rune)'+' or (rune)',' or (rune)';' or (rune)'=' or (rune)':' or (rune)'[' or (rune)']' or (rune)'<' or (rune)'>' or (rune)'"': {
            return false;
        }}

    }
    switch (c) {
    case (rune)'-' or (rune)'_' or (rune)'.' or (rune)'~': {
        return false;
    }
    case (rune)'$' or (rune)'&' or (rune)'+' or (rune)',' or (rune)'/' or (rune)':' or (rune)';' or (rune)'=' or (rune)'?' or (rune)'@': {
        var exprᴛ1 = mode;
        if (exprᴛ1 == encodePath) {
            return c == (rune)'?';
        }
        if (exprᴛ1 == encodePathSegment) {
            return c == (rune)'/' || c == (rune)';' || c == (rune)',' || c == (rune)'?';
        }
        if (exprᴛ1 == encodeUserPassword) {
            return c == (rune)'@' || c == (rune)'/' || c == (rune)'?' || c == (rune)':';
        }
        if (exprᴛ1 == encodeQueryComponent) {
            return true;
        }
        if (exprᴛ1 == encodeFragment) {
            return false;
        }

        break;
    }}

    // §2.3 Unreserved characters (mark)
    // §2.2 Reserved characters (reserved)
    // Different sections of the URL allow a few of
    // the reserved characters to appear unescaped.
    // §3.3
    // The RFC allows : @ & = + $ but saves / ; , for assigning
    // meaning to individual path segments. This package
    // only manipulates the path as a whole, so we allow those
    // last three as well. That leaves only ? to escape.
    // §3.3
    // The RFC allows : @ & = + $ but saves / ; , for assigning
    // meaning to individual path segments.
    // §3.2.1
    // The RFC allows ';', ':', '&', '=', '+', '$', and ',' in
    // userinfo, so we must escape only '@', '/', and '?'.
    // The parsing of userinfo treats ':' as special so we must escape
    // that too.
    // §3.4
    // The RFC reserves (so we must escape) everything.
    // §4.1
    // The RFC text is silent but the grammar allows
    // everything, so escape nothing.
    if (mode == encodeFragment) {
        // RFC 3986 §2.2 allows not escaping sub-delims. A subset of sub-delims are
        // included in reserved from RFC 2396 §2.2. The remaining sub-delims do not
        // need to be escaped. To minimize potential breakage, we apply two restrictions:
        // (1) we always escape sub-delims outside of the fragment, and (2) we always
        // escape single quote to avoid breaking callers that had previously assumed that
        // single quotes would be escaped. See issue #19917.
        switch (c) {
        case (rune)'!' or (rune)'(' or (rune)')' or (rune)'*': {
            return false;
        }}

    }
    // Everything else must be escaped.
    return true;
}

// QueryUnescape does the inverse transformation of [QueryEscape],
// converting each 3-byte encoded substring of the form "%AB" into the
// hex-decoded byte 0xAB.
// It returns an error if any % is not followed by two hexadecimal
// digits.
public static (@string, error) QueryUnescape(@string s) {
    return unescape(s, encodeQueryComponent);
}

// PathUnescape does the inverse transformation of [PathEscape],
// converting each 3-byte encoded substring of the form "%AB" into the
// hex-decoded byte 0xAB. It returns an error if any % is not followed
// by two hexadecimal digits.
//
// PathUnescape is identical to [QueryUnescape] except that it does not
// unescape '+' to ' ' (space).
public static (@string, error) PathUnescape(@string s) {
    return unescape(s, encodePathSegment);
}

// unescape unescapes a string; the mode specifies
// which section of the URL string is being unescaped.
internal static (@string, error) unescape(@string s, encoding mode) {
    // Count %, check that they're well-formed.
    nint n = 0;
    var hasPlus = false;
    for (nint i = 0; i < len(s); ) {
        switch (s[i]) {
        case (rune)'%': {
            n++;
            if (i + 2 >= len(s) || !ishex(s[i + 1]) || !ishex(s[i + 2])) {
                s = s.slice(i);
                if (len(s) > 3) {
                    s = s[..3];
                }
                return ("", ((EscapeError)s));
            }
            if (mode == encodeHost && unhex(s[i + 1]) < 8 && s.slice(i, i + 3) != "%25") {
                // Per https://tools.ietf.org/html/rfc3986#page-21
                // in the host component %-encoding can only be used
                // for non-ASCII bytes.
                // But https://tools.ietf.org/html/rfc6874#section-2
                // introduces %25 being allowed to escape a percent sign
                // in IPv6 scoped-address literals. Yay.
                return ("", ((EscapeError)(s.slice(i, i + 3))));
            }
            if (mode == encodeZone) {
                // RFC 6874 says basically "anything goes" for zone identifiers
                // and that even non-ASCII can be redundantly escaped,
                // but it seems prudent to restrict %-escaped bytes here to those
                // that are valid host name bytes in their unescaped form.
                // That is, you can use escaping in the zone identifier but not
                // to introduce bytes you couldn't just write directly.
                // But Windows puts spaces here! Yay.
                var v = (byte)((byte)(unhex(s[i + 1]) << (int)(4)) | unhex(s[i + 2]));
                if (s.slice(i, i + 3) != "%25" && v != (rune)' ' && shouldEscape(v, encodeHost)) {
                    return ("", ((EscapeError)(s.slice(i, i + 3))));
                }
            }
            i += 3;
            break;
        }
        case (rune)'+': {
            hasPlus = mode == encodeQueryComponent;
            i++;
            break;
        }
        default: {
            if ((mode == encodeHost || mode == encodeZone) && s[i] < 0x80 && shouldEscape(s[i], mode)) {
                return ("", ((InvalidHostError)(s.slice(i, i + 1))));
            }
            i++;
            break;
        }}

    }
    if (n == 0 && !hasPlus) {
        return (s, default!);
    }
    ref var t = ref heap(new strings.Builder(), out var Ꮡt);
    Ꮡt.Grow(len(s) - 2 * n);
    for (nint i = 0; i < len(s); i++) {
        switch (s[i]) {
        case (rune)'%': {
            Ꮡt.WriteByte((byte)((byte)(unhex(s[i + 1]) << (int)(4)) | unhex(s[i + 2])));
            i += 2;
            break;
        }
        case (rune)'+': {
            if (mode == encodeQueryComponent){
                Ꮡt.WriteByte((rune)' ');
            } else {
                Ꮡt.WriteByte((rune)'+');
            }
            break;
        }
        default: {
            Ꮡt.WriteByte(s[i]);
            break;
        }}

    }
    return (t.String(), default!);
}

// QueryEscape escapes the string so it can be safely placed
// inside a [URL] query.
public static @string QueryEscape(@string s) {
    return escape(s, encodeQueryComponent);
}

// PathEscape escapes the string so it can be safely placed inside a [URL] path segment,
// replacing special characters (including /) with %XX sequences as needed.
public static @string PathEscape(@string s) {
    return escape(s, encodePathSegment);
}

internal static @string escape(@string s, encoding mode) {
    nint spaceCount = 0;
    nint hexCount = 0;
    for (nint i = 0; i < len(s); i++) {
        var c = s[i];
        if (shouldEscape(c, mode)) {
            if (c == (rune)' ' && mode == encodeQueryComponent){
                spaceCount++;
            } else {
                hexCount++;
            }
        }
    }
    if (spaceCount == 0 && hexCount == 0) {
        return s;
    }
    array<byte> buf = new(64);
    slice<byte> t = default!;
    nint @required = len(s) + 2 * hexCount;
    if (@required <= len(buf)){
        t = buf.slice(0, @required);
    } else {
        t = new slice<byte>(@required);
    }
    if (hexCount == 0) {
        copy(t, s);
        for (nint i = 0; i < len(s); i++) {
            if (s[i] == (rune)' ') {
                t[i] = (rune)'+';
            }
        }
        return ((@string)t);
    }
    nint j = 0;
    for (nint i = 0; i < len(s); i++) {
        {
            var c = s[i];
            switch (ᐧ) {
            case {} when c == (rune)' ' && mode == encodeQueryComponent: {
                t[j] = (rune)'+';
                j++;
                break;
            }
            case {} when shouldEscape(c, mode): {
                t[j] = (rune)'%';
                t[j + 1] = upperhex[(c >> (int)(4))];
                t[j + 2] = upperhex[(byte)(c & 15)];
                j += 3;
                break;
            }
            default: {
                t[j] = s[i];
                j++;
                break;
            }}
        }

    }
    return ((@string)t);
}

// A URL represents a parsed URL (technically, a URI reference).
//
// The general form represented is:
//
//	[scheme:][//[userinfo@]host][/]path[?query][#fragment]
//
// URLs that do not start with a slash after the scheme are interpreted as:
//
//	scheme:opaque[?query][#fragment]
//
// The Host field contains the host and port subcomponents of the URL.
// When the port is present, it is separated from the host with a colon.
// When the host is an IPv6 address, it must be enclosed in square brackets:
// "[fe80::1]:80". The [net.JoinHostPort] function combines a host and port
// into a string suitable for the Host field, adding square brackets to
// the host when necessary.
//
// Note that the Path field is stored in decoded form: /%47%6f%2f becomes /Go/.
// A consequence is that it is impossible to tell which slashes in the Path were
// slashes in the raw URL and which were %2f. This distinction is rarely important,
// but when it is, the code should use the [URL.EscapedPath] method, which preserves
// the original encoding of Path.
//
// The RawPath field is an optional field which is only set when the default
// encoding of Path is different from the escaped path. See the EscapedPath method
// for more details.
//
// URL's String method uses the EscapedPath method to obtain the path.
partial struct URL {
    public @string Scheme;
    public @string Opaque;   // encoded opaque data
    public ж<Userinfo> User; // username and password information
    public @string Host;   // host or host:port (see Hostname and Port methods)
    public @string Path;   // path (relative paths may omit leading slash)
    public @string RawPath;   // encoded path hint (see EscapedPath method)
    public bool OmitHost;      // do not emit empty host (authority)
    public bool ForceQuery;      // append a query ('?') even if RawQuery is empty
    public @string RawQuery;   // encoded query values, without '?'
    public @string Fragment;   // fragment for references, without '#'
    public @string RawFragment;   // encoded fragment hint (see EscapedFragment method)
}

// User returns a [Userinfo] containing the provided username
// and no password set.
public static ж<Userinfo> User(@string username) {
    return Ꮡ(new Userinfo(username, ""u8, false));
}

// UserPassword returns a [Userinfo] containing the provided username
// and password.
//
// This functionality should only be used with legacy web sites.
// RFC 2396 warns that interpreting Userinfo this way
// “is NOT RECOMMENDED, because the passing of authentication
// information in clear text (such as URI) has proven to be a
// security risk in almost every case where it has been used.”
public static ж<Userinfo> UserPassword(@string username, @string password) {
    return Ꮡ(new Userinfo(username, password, true));
}

// The Userinfo type is an immutable encapsulation of username and
// password details for a [URL]. An existing Userinfo value is guaranteed
// to have a username set (potentially empty, as allowed by RFC 2396),
// and optionally a password.
partial struct Userinfo {
    internal @string username;
    internal @string password;
    internal bool passwordSet;
}

// Username returns the username.
public static @string Username(this ж<Userinfo> Ꮡu) {
    ref var u = ref Ꮡu.DerefOrNull();

    if (Ꮡu == nil) {
        return ""u8;
    }
    return u.username;
}

// Password returns the password in case it is set, and whether it is set.
public static (@string, bool) Password(this ж<Userinfo> Ꮡu) {
    ref var u = ref Ꮡu.DerefOrNull();

    if (Ꮡu == nil) {
        return ("", false);
    }
    return (u.password, u.passwordSet);
}

// String returns the encoded userinfo information in the standard form
// of "username[:password]".
public static @string String(this ж<Userinfo> Ꮡu) {
    ref var u = ref Ꮡu.DerefOrNull();

    if (Ꮡu == nil) {
        return ""u8;
    }
    @string s = escape(u.username, encodeUserPassword);
    if (u.passwordSet) {
        s += ":"u8 + escape(u.password, encodeUserPassword);
    }
    return s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string missingProtocolSchemeˢ = "missing protocol scheme"u8;

// Maybe rawURL is of the form scheme:path.
// (Scheme must be [a-zA-Z][a-zA-Z0-9+.-]*)
// If so, return scheme, path; else return "", rawURL.
internal static (@string scheme, @string path, error err) getScheme(@string rawURL) {
    for (nint i = 0; i < len(rawURL); i++) {
        var c = rawURL[i];
        switch (ᐧ) {
        case {} when (rune)'a' <= c && c <= (rune)'z' || (rune)'A' <= c && c <= (rune)'Z': {
            break;
        }
        case {} when (rune)'0' <= c && c <= (rune)'9' || c == (rune)'+' || c == (rune)'-' || c == (rune)'.': {
            if (i == 0) {
                // do nothing
                return ("", rawURL, default!);
            }
            break;
        }
        case {} when c is (rune)':': {
            if (i == 0) {
                return ("", "", errors.New(missingProtocolSchemeˢ));
            }
            return (rawURL.slice(0, i), rawURL.slice(i + 1), default!);
        }
        default: {
            return ("", rawURL, default!);
        }}

    }
    // we have encountered an invalid character,
    // so there is no valid scheme
    return ("", rawURL, default!);
}

// Parse parses a raw url into a [URL] structure.
//
// The url may be relative (a path, without a host) or absolute
// (starting with a scheme). Trying to parse a hostname and path
// without a scheme is invalid but may not necessarily return an
// error, due to parsing ambiguities.
public static (ж<URL>, error) Parse(@string rawURL) {
    // Cut off #frag
    ref var u = ref heap<@string>(out var Ꮡu);
    (u, var frag, _) = strings.Cut(rawURL, "#"u8);
    var (url, err) = parse(u, false);
    if (err != default!) {
        return (default!, new ΔErrorжerror(Ꮡ(new ΔError("parse"u8, u, err))));
    }
    if (frag == ""u8) {
        return (url, default!);
    }
    {
        err = url.setFragment(frag); if (err != default!) {
            return (default!, new ΔErrorжerror(Ꮡ(new ΔError("parse"u8, rawURL, err))));
        }
    }
    return (url, default!);
}

// ParseRequestURI parses a raw url into a [URL] structure. It assumes that
// url was received in an HTTP request, so the url is interpreted
// only as an absolute URI or an absolute path.
// The string url is assumed not to have a #fragment suffix.
// (Web browsers strip #fragment before sending the URL to a web server.)
public static (ж<URL>, error) ParseRequestURI(@string rawURL) {
    var (url, err) = parse(rawURL, true);
    if (err != default!) {
        return (default!, new ΔErrorжerror(Ꮡ(new ΔError("parse"u8, rawURL, err))));
    }
    return (url, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string netUrlInvalidControlˢ = "net/url: invalid control character in URL"u8;
internal static readonly @string emptyUrlˢ = "empty url"u8;
internal static readonly @string invalidUriForRequestˢ = "invalid URI for request"u8;
internal static readonly @string firstPathSegmentInUrlˢ = "first path segment in URL cannot contain colon"u8;

// parse parses a URL from a string in one of two contexts. If
// viaRequest is true, the URL is assumed to have arrived via an HTTP request,
// in which case only absolute URLs or path-absolute relative URLs are allowed.
// If viaRequest is false, all forms of relative URLs are allowed.
internal static (ж<URL>, error) parse(@string rawURL, bool viaRequest) {
    @string rest = default!;
    error err = default!;
    if (stringContainsCTLByte(rawURL)) {
        return (default!, errors.New(netUrlInvalidControlˢ));
    }
    if (rawURL == ""u8 && viaRequest) {
        return (default!, errors.New(emptyUrlˢ));
    }
    var url = @new<URL>();
    if (rawURL == "*"u8) {
        url.Value.Path = "*"u8;
        return (url, default!);
    }
    // Split off possible leading "http:", "mailto:", etc.
    // Cannot contain escaped characters.
    {
        (url.Value.Scheme, rest, err) = getScheme(rawURL); if (err != default!) {
            return (default!, err);
        }
    }
    url.Value.Scheme = strings.ToLower((~url).Scheme);
    if (strings.HasSuffix(rest, "?"u8) && strings.Count(rest, "?"u8) == 1){
        url.Value.ForceQuery = true;
        rest = rest.slice(0, len(rest) - 1);
    } else {
        (rest, url.Value.RawQuery, _) = strings.Cut(rest, "?"u8);
    }
    if (!strings.HasPrefix(rest, "/"u8)) {
        if ((~url).Scheme != ""u8) {
            // We consider rootless paths per RFC 3986 as opaque.
            url.Value.Opaque = rest;
            return (url, default!);
        }
        if (viaRequest) {
            return (default!, errors.New(invalidUriForRequestˢ));
        }
        // Avoid confusion with malformed schemes, like cache_object:foo/bar.
        // See golang.org/issue/16822.
        //
        // RFC 3986, §3.3:
        // In addition, a URI reference (Section 4.1) may be a relative-path reference,
        // in which case the first path segment cannot contain a colon (":") character.
        {
            var (segment, _, _) = strings.Cut(rest, "/"u8); if (strings.Contains(segment, ":"u8)) {
                // First path segment has colon. Not allowed in relative URL.
                return (default!, errors.New(firstPathSegmentInUrlˢ));
            }
        }
    }
    if (((~url).Scheme != ""u8 || !viaRequest && !strings.HasPrefix(rest, "///"u8)) && strings.HasPrefix(rest, "//"u8)){
        @string authority = default!;
        (authority, rest) = (rest[2..], "");
        {
            nint i = strings.Index(authority, "/"u8); if (i >= 0) {
                (authority, rest) = (authority.slice(0, i), authority.slice(i));
            }
        }
        (url.Value.User, url.Value.Host, err) = parseAuthority(authority);
        if (err != default!) {
            return (default!, err);
        }
    } else 
    if ((~url).Scheme != ""u8 && strings.HasPrefix(rest, "/"u8)) {
        // OmitHost is set to true when rawURL has an empty host (authority).
        // See golang.org/issue/46059.
        url.Value.OmitHost = true;
    }
    // Set Path and, optionally, RawPath.
    // RawPath is a hint of the encoding of Path. We don't want to set it if
    // the default escaping of Path is equivalent, to help make sure that people
    // don't rely on it in general.
    {
        var errΔ1 = url.setPath(rest); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    return (url, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string netUrlInvalidUserinfoˢ = "net/url: invalid userinfo"u8;

internal static (ж<Userinfo> user, @string host, error err) parseAuthority(@string authority) {
    ж<Userinfo> user = default!;
    @string host = default!;
    error err = default!;

    nint i = strings.LastIndex(authority, "@"u8);
    if (i < 0){
        (host, err) = parseHost(authority);
    } else {
        (host, err) = parseHost(authority.slice(i + 1));
    }
    if (err != default!) {
        return (default!, "", err);
    }
    if (i < 0) {
        return (default!, host, default!);
    }
    @string userinfo = authority.slice(0, i);
    if (!validUserinfo(userinfo)) {
        return (default!, "", errors.New(netUrlInvalidUserinfoˢ));
    }
    if (!strings.Contains(userinfo, ":"u8)){
        {
            (userinfo, err) = unescape(userinfo, encodeUserPassword); if (err != default!) {
                return (default!, "", err);
            }
        }
        user = User(userinfo);
    } else {
        var (username, password, _) = strings.Cut(userinfo, ":"u8);
        {
            (username, err) = unescape(username, encodeUserPassword); if (err != default!) {
                return (default!, "", err);
            }
        }
        {
            (password, err) = unescape(password, encodeUserPassword); if (err != default!) {
                return (default!, "", err);
            }
        }
        user = UserPassword(username, password);
    }
    return (user, host, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string missingInHostˢ = "missing ']' in host"u8;
internal static readonly @string invalidIpLiteralˢ = "invalid IP-literal"u8;

// parseHost parses host as an authority without user
// information. That is, as host[:port].
internal static (@string, error) parseHost(@string host) {
    {
        nint openBracketIdx = strings.LastIndex(host, "["u8); if (openBracketIdx != -1){
            // Parse an IP-Literal in RFC 3986 and RFC 6874.
            // E.g., "[fe80::1]", "[fe80::1%25en0]", "[fe80::1]:80".
            nint closeBracketIdx = strings.LastIndex(host, "]"u8);
            if (closeBracketIdx < 0) {
                return ("", errors.New(missingInHostˢ));
            }
            @string colonPort = host.slice(closeBracketIdx + 1);
            if (!validOptionalPort(colonPort)) {
                return ("", fmt.Errorf("invalid port %q after host"u8, colonPort));
            }
            var (unescapedColonPort, errΔ1) = unescape(colonPort, encodeHost);
            if (errΔ1 != default!) {
                return ("", errΔ1);
            }
            @string hostname = host.slice(openBracketIdx + 1, closeBracketIdx);
            @string unescapedHostname = default!;
            // RFC 6874 defines that %25 (%-encoded percent) introduces
            // the zone identifier, and the zone identifier can use basically
            // any %-encoding it likes. That's different from the host, which
            // can only %-encode non-ASCII bytes.
            // We do impose some restrictions on the zone, to avoid stupidity
            // like newlines.
            nint zoneIdx = strings.Index(hostname, "%25"u8);
            if (zoneIdx >= 0){
                var (hostPart, errΔ2) = unescape(hostname.slice(0, zoneIdx), encodeHost);
                if (errΔ2 != default!) {
                    return ("", errΔ2);
                }
                (var zonePart, errΔ2) = unescape(hostname.slice(zoneIdx), encodeZone);
                if (errΔ2 != default!) {
                    return ("", errΔ2);
                }
                unescapedHostname = hostPart + zonePart;
            } else {
                error errΔ3 = default!;
                (unescapedHostname, errΔ3) = unescape(hostname, encodeHost);
                if (errΔ3 != default!) {
                    return ("", errΔ3);
                }
            }
            // Per RFC 3986, only a host identified by a valid
            // IPv6 address can be enclosed by square brackets.
            // This excludes any IPv4, but notably not IPv4-mapped addresses.
            (var addr, errΔ1) = netip.ParseAddr(unescapedHostname);
            if (errΔ1 != default!) {
                return ("", fmt.Errorf("invalid host: %w"u8, errΔ1));
            }
            if (addr.Is4()) {
                return ("", errors.New(invalidIpLiteralˢ));
            }
            return ("[" + unescapedHostname + "]" + unescapedColonPort, default!);
        } else 
        {
            nint i = strings.LastIndex(host, ":"u8); if (i != -1) {
                @string colonPort = host.slice(i);
                if (!validOptionalPort(colonPort)) {
                    return ("", fmt.Errorf("invalid port %q after host"u8, colonPort));
                }
            }
        }
    }
    error err = default!;
    {
        (host, err) = unescape(host, encodeHost); if (err != default!) {
            return ("", err);
        }
    }
    return (host, default!);
}

// setPath sets the Path and RawPath fields of the URL based on the provided
// escaped path p. It maintains the invariant that RawPath is only specified
// when it differs from the default encoding of the path.
// For example:
// - setPath("/foo/bar")   will set Path="/foo/bar" and RawPath=""
// - setPath("/foo%2fbar") will set Path="/foo/bar" and RawPath="/foo%2fbar"
// setPath will return an error only if the provided path contains an invalid
// escaping.
//
// setPath should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/sagernet/sing
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname badSetPath net/url.(*URL).setPath
internal static error setPath(this ref URL u, @string p) {
    var (path, err) = unescape(p, encodePath);
    if (err != default!) {
        return err;
    }
    u.Path = path;
    {
        @string escp = escape(path, encodePath); if (p == escp){
            // Default encoding is fine.
            u.RawPath = ""u8;
        } else {
            u.RawPath = p;
        }
    }
    return default!;
}

// for linkname because we cannot linkname methods directly
internal static partial error badSetPath(ж<URL> _Δp0, @string _Δp1);

// EscapedPath returns the escaped form of u.Path.
// In general there are multiple possible escaped forms of any path.
// EscapedPath returns u.RawPath when it is a valid escaping of u.Path.
// Otherwise EscapedPath ignores u.RawPath and computes an escaped
// form on its own.
// The [URL.String] and [URL.RequestURI] methods use EscapedPath to construct
// their results.
// In general, code should call EscapedPath instead of
// reading u.RawPath directly.
public static @string EscapedPath(this ref URL u) {
    if (u.RawPath != ""u8 && validEncoded(u.RawPath, encodePath)) {
        var (p, err) = unescape(u.RawPath, encodePath);
        if (err == default! && p == u.Path) {
            return u.RawPath;
        }
    }
    if (u.Path == "*"u8) {
        return "*"u8; // don't escape (Issue 11202)
    }
    return escape(u.Path, encodePath);
}

// validEncoded reports whether s is a valid encoded path or fragment,
// according to mode.
// It must not contain any bytes that require escaping during encoding.
internal static bool validEncoded(@string s, encoding mode) {
    for (nint i = 0; i < len(s); i++) {
        // RFC 3986, Appendix A.
        // pchar = unreserved / pct-encoded / sub-delims / ":" / "@".
        // shouldEscape is not quite compliant with the RFC,
        // so we check the sub-delims ourselves and let
        // shouldEscape handle the others.
        switch (s[i]) {
        case (rune)'!' or (rune)'$' or (rune)'&' or (rune)'\'' or (rune)'(' or (rune)')' or (rune)'*' or (rune)'+' or (rune)',' or (rune)';' or (rune)'=' or (rune)':' or (rune)'@': {
            break;
        }
        case (rune)'[' or (rune)']': {
            break;
        }
        case (rune)'%': {
            break;
        }
        default: {
            if (shouldEscape(s[i], // ok
 // ok - not specified in RFC 3986 but left alone by modern browsers
 // ok - percent encoded, will decode
 mode)) {
                return false;
            }
            break;
        }}

    }
    return true;
}

// setFragment is like setPath but for Fragment/RawFragment.
internal static error setFragment(this ref URL u, @string f) {
    var (frag, err) = unescape(f, encodeFragment);
    if (err != default!) {
        return err;
    }
    u.Fragment = frag;
    {
        @string escf = escape(frag, encodeFragment); if (f == escf){
            // Default encoding is fine.
            u.RawFragment = ""u8;
        } else {
            u.RawFragment = f;
        }
    }
    return default!;
}

// EscapedFragment returns the escaped form of u.Fragment.
// In general there are multiple possible escaped forms of any fragment.
// EscapedFragment returns u.RawFragment when it is a valid escaping of u.Fragment.
// Otherwise EscapedFragment ignores u.RawFragment and computes an escaped
// form on its own.
// The [URL.String] method uses EscapedFragment to construct its result.
// In general, code should call EscapedFragment instead of
// reading u.RawFragment directly.
public static @string EscapedFragment(this ref URL u) {
    if (u.RawFragment != ""u8 && validEncoded(u.RawFragment, encodeFragment)) {
        var (f, err) = unescape(u.RawFragment, encodeFragment);
        if (err == default! && f == u.Fragment) {
            return u.RawFragment;
        }
    }
    return escape(u.Fragment, encodeFragment);
}

// validOptionalPort reports whether port is either an empty string
// or matches /^:\d*$/
internal static bool validOptionalPort(@string port) {
    if (port == ""u8) {
        return true;
    }
    if (port[0] != (rune)':') {
        return false;
    }
    foreach (var (_, b) in port[1..]) {
        if (b < (rune)'0' || b > (rune)'9') {
            return false;
        }
    }
    return true;
}

// String reassembles the [URL] into a valid URL string.
// The general form of the result is one of:
//
//	scheme:opaque?query#fragment
//	scheme://userinfo@host/path?query#fragment
//
// If u.Opaque is non-empty, String uses the first form;
// otherwise it uses the second form.
// Any non-ASCII characters in host are escaped.
// To obtain the path, String uses u.EscapedPath().
//
// In the second form, the following rules apply:
//   - if u.Scheme is empty, scheme: is omitted.
//   - if u.User is nil, userinfo@ is omitted.
//   - if u.Host is empty, host/ is omitted.
//   - if u.Scheme and u.Host are empty and u.User is nil,
//     the entire scheme://userinfo@host/ is omitted.
//   - if u.Host is non-empty and u.Path begins with a /,
//     the form host/path does not add its own /.
//   - if u.RawQuery is empty, ?query is omitted.
//   - if u.Fragment is empty, #fragment is omitted.
public static @string String(this ref URL u) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    nint n = len(u.Scheme);
    if (u.Opaque != ""u8){
        n += len(u.Opaque);
    } else {
        if (!u.OmitHost && (u.Scheme != ""u8 || u.Host != ""u8 || u.User != nil)) {
            @string username = u.User.Username();
            var (password, _) = u.User.Password();
            n += len(username) + len(password) + len(u.Host);
        }
        n += len(u.Path);
    }
    n += len(u.RawQuery) + len(u.RawFragment);
    n += len(":" + "//" + "//" + ":" + "@" + "/" + "./" + "?" + "#");
    Ꮡbuf.Grow(n);
    if (u.Scheme != ""u8) {
        Ꮡbuf.WriteString(u.Scheme);
        Ꮡbuf.WriteByte((rune)':');
    }
    if (u.Opaque != ""u8){
        Ꮡbuf.WriteString(u.Opaque);
    } else {
        if (u.Scheme != ""u8 || u.Host != ""u8 || u.User != nil) {
            if (u.OmitHost && u.Host == ""u8 && u.User == nil){
            } else {
                // omit empty host
                if (u.Host != ""u8 || u.Path != ""u8 || u.User != nil) {
                    Ꮡbuf.WriteString("//"u8);
                }
                {
                    var ui = u.User; if (ui != nil) {
                        Ꮡbuf.WriteString(ui.String());
                        Ꮡbuf.WriteByte((rune)'@');
                    }
                }
                {
                    @string h = u.Host; if (h != ""u8) {
                        Ꮡbuf.WriteString(escape(h, encodeHost));
                    }
                }
            }
        }
        @string path = u.EscapedPath();
        if (path != ""u8 && path[0] != (rune)'/' && u.Host != ""u8) {
            Ꮡbuf.WriteByte((rune)'/');
        }
        if (buf.Len() == 0) {
            // RFC 3986 §4.2
            // A path segment that contains a colon character (e.g., "this:that")
            // cannot be used as the first segment of a relative-path reference, as
            // it would be mistaken for a scheme name. Such a segment must be
            // preceded by a dot-segment (e.g., "./this:that") to make a relative-
            // path reference.
            {
                var (segment, _, _) = strings.Cut(path, "/"u8); if (strings.Contains(segment, ":"u8)) {
                    Ꮡbuf.WriteString("./"u8);
                }
            }
        }
        Ꮡbuf.WriteString(path);
    }
    if (u.ForceQuery || u.RawQuery != ""u8) {
        Ꮡbuf.WriteByte((rune)'?');
        Ꮡbuf.WriteString(u.RawQuery);
    }
    if (u.Fragment != ""u8) {
        Ꮡbuf.WriteByte((rune)'#');
        Ꮡbuf.WriteString(u.EscapedFragment());
    }
    return buf.String();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xxxxxˢ = "xxxxx"u8;

// Redacted is like [URL.String] but replaces any password with "xxxxx".
// Only the password in u.User is redacted.
public static @string Redacted(this ж<URL> Ꮡu) {
    ref var u = ref Ꮡu.DerefOrNull();

    if (Ꮡu == nil) {
        return ""u8;
    }
    var ru = u;
    {
        var (_, has) = ru.User.Password(); if (has) {
            ru.User = UserPassword(ru.User.Username(), xxxxxˢ);
        }
    }
    return ru.String();
}

partial struct Values /*map[@string, slice<@string>]*/;

// Get gets the first value associated with the given key.
// If there are no values associated with the key, Get returns
// the empty string. To access multiple values, use the map
// directly.
public static @string Get(this Values v, @string key) {
    var vs = v[key];
    if (len(vs) == 0) {
        return ""u8;
    }
    return vs[0];
}

// Set sets the key to value. It replaces any existing
// values.
public static void Set(this Values v, @string key, @string value) {
    v[key] = new @string[]{value}.slice();
}

// Add adds the value to key. It appends to any existing
// values associated with key.
public static void Add(this Values v, @string key, @string value) {
    v[key] = append(v[key], value);
}

// Del deletes the values associated with key.
public static void Del(this Values v, @string key) {
    delete(v, key);
}

// Has checks whether a given key is set.
public static bool Has(this Values v, @string key) {
    var (_, ok) = v[key, ꟷ];
    return ok;
}

// ParseQuery parses the URL-encoded query string and returns
// a map listing the values specified for each key.
// ParseQuery always returns a non-nil map containing all the
// valid query parameters found; err describes the first decoding error
// encountered, if any.
//
// Query is expected to be a list of key=value settings separated by ampersands.
// A setting without an equals sign is interpreted as a key set to an empty
// value.
// Settings containing a non-URL-encoded semicolon are considered invalid.
public static (Values, error) ParseQuery(@string query) {
    var m = new Values(0);
    var err = parseQuery(m, query);
    return (m, err);
}

internal static ж<godebug.Setting> urlmaxqueryparams = godebug.New("urlmaxqueryparams"u8);

internal static UntypedInt defaultMaxParams => 10000;

internal static bool urlParamsWithinMax(nint @params) {
    var withinDefaultMax = @params <= defaultMaxParams;
    if (urlmaxqueryparams.Value() == ""u8) {
        return withinDefaultMax;
    }
    var (customMax, err) = strconv.Atoi(urlmaxqueryparams.Value());
    if (err != default!) {
        return withinDefaultMax;
    }
    var withinCustomMax = customMax == 0 || @params < customMax;
    if (withinDefaultMax != withinCustomMax) {
        urlmaxqueryparams.IncNonDefault();
    }
    return withinCustomMax;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string numberOfUrlQueryˢ = "number of URL query parameters exceeded limit"u8;

internal static error /*err*/ parseQuery(Values m, @string query) {
    error err = default!;

    if (!urlParamsWithinMax(strings.Count(query, "&"u8) + 1)) {
        return errors.New(numberOfUrlQueryˢ);
    }
    while (query != ""u8) {
        @string key = default!;
        (key, query, _) = strings.Cut(query, "&"u8);
        if (strings.Contains(key, ";"u8)) {
            err = fmt.Errorf("invalid semicolon separator in query"u8);
            continue;
        }
        if (key == ""u8) {
            continue;
        }
        (key, var value, _) = strings.Cut(key, "="u8);
        (key, var err1) = QueryUnescape(key);
        if (err1 != default!) {
            if (err == default!) {
                err = err1;
            }
            continue;
        }
        (value, err1) = QueryUnescape(value);
        if (err1 != default!) {
            if (err == default!) {
                err = err1;
            }
            continue;
        }
        m[key] = append(m[key], value);
    }
    return err;
}

// Encode encodes the values into “URL encoded” form
// ("bar=baz&foo=quux") sorted by key.
public static @string Encode(this Values v) {
    if (len(v) == 0) {
        return ""u8;
    }
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    foreach (var (_, k) in slices.Sorted(maps.Keys<Values, @string, slice<@string>>(v))) {
        var vs = v[k];
        @string keyEscaped = QueryEscape(k);
        foreach (var (_, vΔ1) in vs) {
            if (buf.Len() > 0) {
                Ꮡbuf.WriteByte((rune)'&');
            }
            Ꮡbuf.WriteString(keyEscaped);
            Ꮡbuf.WriteByte((rune)'=');
            Ꮡbuf.WriteString(QueryEscape(vΔ1));
        }
    }
    return buf.String();
}

// resolvePath applies special path segments from refs and applies
// them to base, per RFC 3986.
internal static @string resolvePath(@string @base, @string @ref) {
    @string full = default!;
    if (@ref == ""u8){
        full = @base;
    } else 
    if (@ref[0] != (rune)'/'){
        nint i = strings.LastIndex(@base, "/"u8);
        full = @base.slice(0, i + 1) + @ref;
    } else {
        full = @ref;
    }
    if (full == ""u8) {
        return ""u8;
    }
    @string elem = default!;
    ref var dst = ref heap(new strings.Builder(), out var Ꮡdst);
    var first = true;
    @string remaining = full;
    // We want to return a leading '/', so write it now.
    Ꮡdst.WriteByte((rune)'/');
    var found = true;
    while (found) {
        (elem, remaining, found) = strings.Cut(remaining, "/"u8);
        if (elem == "."u8) {
            first = false;
            // drop
            continue;
        }
        if (elem == ".."u8){
            // Ignore the leading '/' we already wrote.
            @string str = dst.String()[1..];
            nint index = strings.LastIndexByte(str, (rune)'/');
            dst.Reset();
            Ꮡdst.WriteByte((rune)'/');
            if (index == -1){
                first = true;
            } else {
                Ꮡdst.WriteString(str.slice(0, index));
            }
        } else {
            if (!first) {
                Ꮡdst.WriteByte((rune)'/');
            }
            Ꮡdst.WriteString(elem);
            first = false;
        }
    }
    if (elem == "."u8 || elem == ".."u8) {
        Ꮡdst.WriteByte((rune)'/');
    }
    // We wrote an initial '/', but we don't want two.
    @string r = dst.String();
    if (len(r) > 1 && r[1] == (rune)'/') {
        r = r[1..];
    }
    return r;
}

// IsAbs reports whether the [URL] is absolute.
// Absolute means that it has a non-empty scheme.
public static bool IsAbs(this ref URL u) {
    return u.Scheme != ""u8;
}

// Parse parses a [URL] in the context of the receiver. The provided URL
// may be relative or absolute. Parse returns nil, err on parse
// failure, otherwise its return value is the same as [URL.ResolveReference].
public static (ж<URL>, error) Parse(this ref URL u, @string @ref) {
    var (refURL, err) = Parse(@ref);
    if (err != default!) {
        return (default!, err);
    }
    return (u.ResolveReference(refURL), default!);
}

// ResolveReference resolves a URI reference to an absolute URI from
// an absolute base URI u, per RFC 3986 Section 5.2. The URI reference
// may be relative or absolute. ResolveReference always returns a new
// [URL] instance, even if the returned URL is identical to either the
// base or reference. If ref is an absolute URL, then ResolveReference
// ignores base and returns a copy of ref.
public static ж<URL> ResolveReference(this ref URL u, ж<URL> Ꮡref) {
    ref var @ref = ref Ꮡref.DerefOrNull();

    ref var url = ref heap<URL>(out var Ꮡurl);
    url = @ref;
    if (@ref.Scheme == ""u8) {
        url.Scheme = u.Scheme;
    }
    if (@ref.Scheme != ""u8 || @ref.Host != ""u8 || @ref.User != nil) {
        // The "absoluteURI" or "net_path" cases.
        // We can ignore the error from setPath since we know we provided a
        // validly-escaped path.
        url.setPath(resolvePath(@ref.EscapedPath(), ""u8));
        return Ꮡurl;
    }
    if (@ref.Opaque != ""u8) {
        url.User = default!;
        url.Host = ""u8;
        url.Path = ""u8;
        return Ꮡurl;
    }
    if (@ref.Path == ""u8 && !@ref.ForceQuery && @ref.RawQuery == ""u8) {
        url.RawQuery = u.RawQuery;
        if (@ref.Fragment == ""u8) {
            url.Fragment = u.Fragment;
            url.RawFragment = u.RawFragment;
        }
    }
    if (@ref.Path == ""u8 && u.Opaque != ""u8) {
        url.Opaque = u.Opaque;
        url.User = default!;
        url.Host = ""u8;
        url.Path = ""u8;
        return Ꮡurl;
    }
    // The "abs_path" or "rel_path" cases.
    url.Host = u.Host;
    url.User = u.User;
    url.setPath(resolvePath(u.EscapedPath(), @ref.EscapedPath()));
    return Ꮡurl;
}

// Query parses RawQuery and returns the corresponding values.
// It silently discards malformed value pairs.
// To check errors use [ParseQuery].
public static Values Query(this ref URL u) {
    var (v, _) = ParseQuery(u.RawQuery);
    return v;
}

// RequestURI returns the encoded path?query or opaque?query
// string that would be used in an HTTP request for u.
public static @string RequestURI(this ref URL u) {
    @string result = u.Opaque;
    if (result == ""u8){
        result = u.EscapedPath();
        if (result == ""u8) {
            result = "/"u8;
        }
    } else {
        if (strings.HasPrefix(result, "//"u8)) {
            result = u.Scheme + ":"u8 + result;
        }
    }
    if (u.ForceQuery || u.RawQuery != ""u8) {
        result += "?"u8 + u.RawQuery;
    }
    return result;
}

// Hostname returns u.Host, stripping any valid port number if present.
//
// If the result is enclosed in square brackets, as literal IPv6 addresses are,
// the square brackets are removed from the result.
public static @string Hostname(this ref URL u) {
    var (host, _) = splitHostPort(u.Host);
    return host;
}

// Port returns the port part of u.Host, without the leading colon.
//
// If u.Host doesn't contain a valid numeric port, Port returns an empty string.
public static @string Port(this ref URL u) {
    var (_, port) = splitHostPort(u.Host);
    return port;
}

// splitHostPort separates host and port. If the port is not valid, it returns
// the entire input as host, and it doesn't check the validity of the host.
// Unlike net.SplitHostPort, but per RFC 3986, it requires ports to be numeric.
internal static (@string host, @string port) splitHostPort(@string hostPort) {
    @string host = default!;
    @string port = default!;

    host = hostPort;
    nint colon = strings.LastIndexByte(host, (rune)':');
    if (colon != -1 && validOptionalPort(host.slice(colon))) {
        (host, port) = (host.slice(0, colon), host.slice(colon + 1));
    }
    if (strings.HasPrefix(host, "["u8) && strings.HasSuffix(host, "]"u8)) {
        host = host.slice(1, len(host) - 1);
    }
    return (host, port);
}

// Marshaling interface implementations.
// Would like to implement MarshalText/UnmarshalText but that will change the JSON representation of URLs.
public static (slice<byte> text, error err) MarshalBinary(this ref URL u) {
    return u.AppendBinary(default!);
}

public static (slice<byte>, error) AppendBinary(this ref URL u, slice<byte> b) {
    return (append(b, u.String().ꓸꓸꓸ), default!);
}

public static error UnmarshalBinary(this ref URL u, slice<byte> text) {
    var (u1, err) = Parse(((@string)text));
    if (err != default!) {
        return err;
    }
    u = u1.Value;
    return default!;
}

// JoinPath returns a new [URL] with the provided path elements joined to
// any existing path and the resulting path cleaned of any ./ or ../ elements.
// Any sequences of multiple / characters will be reduced to a single /.
public static ж<URL> JoinPath(this ref URL u, params ꓸꓸꓸstring elemʗp) {
    var elem = elemʗp.slice();

    elem = appendꓸꓸꓸ(new @string[]{u.EscapedPath()}.slice(), elem);
    @string p = default!;
    if (!strings.HasPrefix(elem[0], "/"u8)){
        // Return a relative path if u is relative,
        // but ensure that it contains no ../ elements.
        elem[0] = "/" + elem[0];
        p = path.Join(elem.ꓸꓸꓸ)[1..];
    } else {
        p = path.Join(elem.ꓸꓸꓸ);
    }
    // path.Join will remove any trailing slashes.
    // Preserve at least one.
    if (strings.HasSuffix(elem[len(elem) - 1], "/"u8) && !strings.HasSuffix(p, "/"u8)) {
        p += "/"u8;
    }
    ref var url = ref heap<URL>(out var Ꮡurl);
    url = u;
    url.setPath(p);
    return Ꮡurl;
}

// validUserinfo reports whether s is a valid userinfo string per RFC 3986
// Section 3.2.1:
//
//	userinfo    = *( unreserved / pct-encoded / sub-delims / ":" )
//	unreserved  = ALPHA / DIGIT / "-" / "." / "_" / "~"
//	sub-delims  = "!" / "$" / "&" / "'" / "(" / ")"
//	              / "*" / "+" / "," / ";" / "="
//
// It doesn't validate pct-encoded. The caller does that via func unescape.
internal static bool validUserinfo(@string s) {
    foreach (var (_, r) in s) {
        if ((rune)'A' <= r && r <= (rune)'Z') {
            continue;
        }
        if ((rune)'a' <= r && r <= (rune)'z') {
            continue;
        }
        if ((rune)'0' <= r && r <= (rune)'9') {
            continue;
        }
        switch (r) {
        case (rune)'-' or (rune)'.' or (rune)'_' or (rune)':' or (rune)'~' or (rune)'!' or (rune)'$' or (rune)'&' or (rune)'\'' or (rune)'(' or (rune)')' or (rune)'*' or (rune)'+' or (rune)',' or (rune)';' or (rune)'=' or (rune)'%' or (rune)'@': {
            continue;
            break;
        }
        default: {
            return false;
        }}

    }
    return true;
}

// stringContainsCTLByte reports whether s contains any ASCII control character.
internal static bool stringContainsCTLByte(@string s) {
    for (nint i = 0; i < len(s); i++) {
        var b = s[i];
        if (b < (rune)' ' || b == 0x7f) {
            return true;
        }
    }
    return false;
}

// JoinPath returns a [URL] string with the provided path elements joined to
// the existing path of base and the resulting path cleaned of any ./ or ../ elements.
public static (@string result, error err) JoinPath(@string @base, params ꓸꓸꓸstring elemʗp) {
    @string result = default!;
    error err = default!;
    var elem = elemʗp.sslice();

    (var url, err) = Parse(@base);
    if (err != default!) {
        return (result, err);
    }
    result = url.JoinPath(elem.ꓸꓸꓸ).String();
    return (result, err);
}

} // end url_package
