// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using errors = errors_package;
using fmt = fmt_package;
using godebug = go.@internal.godebug_package;
using log = log_package;
using net = net_package;
using ascii = go.net.http.@internal.ascii_package;
using textproto = go.net.textproto_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;
using go.@internal;
using go.net;
using go.net.http.@internal;

partial class http_package {

internal static ж<godebug.Setting> httpcookiemaxnum = godebug.New("httpcookiemaxnum"u8);

// A Cookie represents an HTTP cookie as sent in the Set-Cookie header of an
// HTTP response or the Cookie header of an HTTP request.
//
// See https://tools.ietf.org/html/rfc6265 for details.
partial struct ΔCookie {
    public @string Name;
    public @string Value;
    public bool Quoted; // indicates whether the Value was originally quoted
    public @string Path;   // optional
    public @string Domain;   // optional
    public time.Time Expires; // optional
    public @string RawExpires;   // for reading cookies only
    // MaxAge=0 means no 'Max-Age' attribute specified.
    // MaxAge<0 means delete cookie now, equivalently 'Max-Age: 0'
    // MaxAge>0 means Max-Age attribute present and given in seconds
    public nint MaxAge;
    public bool Secure;
    public bool HttpOnly;
    public SameSite SameSite;
    public bool Partitioned;
    public @string Raw;
    public slice<@string> Unparsed; // Raw text of unparsed attribute-value pairs
}

partial struct SameSite /*num:nint*/;

public static SameSite SameSiteDefaultMode => /* iota + 1 */ 1;
public static SameSite SameSiteLaxMode => 2;
public static SameSite SameSiteStrictMode => 3;
public static SameSite SameSiteNoneMode => 4;

internal static error errBlankCookie = errors.New("http: blank cookie"u8);
internal static error errEqualNotFoundInCookie = errors.New("http: '=' not found in cookie"u8);
internal static error errInvalidCookieName = errors.New("http: invalid cookie name"u8);
internal static error errInvalidCookieValue = errors.New("http: invalid cookie value"u8);
internal static error errCookieNumLimitExceeded = errors.New("http: number of cookies exceeded limit"u8);

internal static UntypedInt defaultCookieMaxNum => 3000;

internal static bool cookieNumWithinMax(nint cookieNum) {
    var withinDefaultMax = cookieNum <= defaultCookieMaxNum;
    if (httpcookiemaxnum.Value() == ""u8) {
        return withinDefaultMax;
    }
    {
        var (customMax, err) = strconv.Atoi(httpcookiemaxnum.Value()); if (err == default!) {
            var withinCustomMax = customMax == 0 || cookieNum <= customMax;
            if (withinDefaultMax != withinCustomMax) {
                httpcookiemaxnum.IncNonDefault();
            }
            return withinCustomMax;
        }
    }
    return withinDefaultMax;
}

// ParseCookie parses a Cookie header value and returns all the cookies
// which were set in it. Since the same cookie name can appear multiple times
// the returned Values can contain more than one value for a given key.
public static (slice<ж<ΔCookie>>, error) ParseCookie(@string line) {
    if (!cookieNumWithinMax(strings.Count(line, ";"u8) + 1)) {
        return (default!, errCookieNumLimitExceeded);
    }
    var parts = strings.Split(textproto.TrimString(line), ";"u8);
    if (builtin.len(parts) == 1 && parts[0] == "") {
        return (default!, errBlankCookie);
    }
    var cookies = new slice<ж<ΔCookie>>(0, builtin.len(parts));
    foreach (var (_, vᴛ1) in parts) {
        var s = vᴛ1;

        s = textproto.TrimString(s);
        ref var name = ref heap<@string>(out var Ꮡname);
        ref var value = ref heap<@string>(out var Ꮡvalue);
        (name, value, var found) = strings.Cut(s, "="u8);
        if (!found) {
            return (default!, errEqualNotFoundInCookie);
        }
        if (!isCookieNameValid(name)) {
            return (default!, errInvalidCookieName);
        }
        ref var quoted = ref heap<bool>(out var Ꮡquoted);
        (value, quoted, found) = parseCookieValue(value, true);
        if (!found) {
            return (default!, errInvalidCookieValue);
        }
        cookies = append(cookies, Ꮡ(new ΔCookie(Name: name, Value: value, Quoted: quoted)));
    }
    return (cookies, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mon02Jan2006150405Mstˢ = "Mon, 02-Jan-2006 15:04:05 MST"u8;

// ParseSetCookie parses a Set-Cookie header value and returns a cookie.
// It returns an error on syntax error.
public static (ж<ΔCookie>, error) ParseSetCookie(@string line) {
    var parts = strings.Split(textproto.TrimString(line), ";"u8);
    if (builtin.len(parts) == 1 && parts[0] == "") {
        return (default!, errBlankCookie);
    }
    parts[0] = textproto.TrimString(parts[0]);
    ref var name = ref heap<@string>(out var Ꮡname);
    ref var value = ref heap<@string>(out var Ꮡvalue);
    (name, value, var ok) = strings.Cut(parts[0], "="u8);
    if (!ok) {
        return (default!, errEqualNotFoundInCookie);
    }
    name = textproto.TrimString(name);
    if (!isCookieNameValid(name)) {
        return (default!, errInvalidCookieName);
    }
    ref var quoted = ref heap<bool>(out var Ꮡquoted);
    (value, quoted, ok) = parseCookieValue(value, true);
    if (!ok) {
        return (default!, errInvalidCookieValue);
    }
    var c = Ꮡ(new ΔCookie(
        Name: name,
        Value: value,
        Quoted: quoted,
        Raw: line
    ));
    for (nint i = 1; i < builtin.len(parts); i++) {
        parts[i] = textproto.TrimString(parts[i]);
        if (builtin.len(parts[i]) == 0) {
            continue;
        }
        var (attr, val, _) = strings.Cut(parts[i], "="u8);
        var (lowerAttr, isASCII) = ascii.ToLower(attr);
        if (!isASCII) {
            continue;
        }
        (val, _, ok) = parseCookieValue(val, false);
        if (!ok) {
            c.Value.Unparsed = append((~c).Unparsed, parts[i]);
            continue;
        }
        var exprᴛ1 = lowerAttr;
        if (exprᴛ1 == "samesite"u8) {
            var (lowerVal, asciiΔ2) = ascii.ToLower(val);
            if (!asciiΔ2) {
                c.Value.SameSite = SameSiteDefaultMode;
                continue;
            }
            var exprᴛ2 = lowerVal;
            if (exprᴛ2 == "lax"u8) {
                c.Value.SameSite = SameSiteLaxMode;
            }
            else if (exprᴛ2 == "strict"u8) {
                c.Value.SameSite = SameSiteStrictMode;
            }
            else if (exprᴛ2 == "none"u8) {
                c.Value.SameSite = SameSiteNoneMode;
            }
            else { /* default: */
                c.Value.SameSite = SameSiteDefaultMode;
            }

            continue;
        }
        else if (exprᴛ1 == "secure"u8) {
            c.Value.Secure = true;
            continue;
        }
        else if (exprᴛ1 == "httponly"u8) {
            c.Value.HttpOnly = true;
            continue;
        }
        else if (exprᴛ1 == "domain"u8) {
            c.Value.Domain = val;
            continue;
        }
        else if (exprᴛ1 == "max-age"u8) {
            do {
                var (secs, err) = strconv.Atoi(val);
                if (err != default! || secs != 0 && val[0] == (rune)'0') {
                    break;
                }
                if (secs <= 0) {
                    secs = -1;
                }
                c.Value.MaxAge = secs;
                goto continueᴛ1;
            } while (false);
        }
        else if (exprᴛ1 == "expires"u8) {
            do {
                c.Value.RawExpires = val;
                var (exptime, err) = time.Parse(time.RFC1123, val);
                if (err != default!) {
                    (exptime, err) = time.Parse(mon02Jan2006150405Mstˢ, val);
                    if (err != default!) {
                        c.Value.Expires = new time.Time(nil);
                        break;
                    }
                }
                c.Value.Expires = exptime.UTC();
                goto continueᴛ1;
            } while (false);
        }
        else if (exprᴛ1 == "path"u8) {
            c.Value.Path = val;
            continue;
        }
        else if (exprᴛ1 == "partitioned"u8) {
            c.Value.Partitioned = true;
            continue;
        }

        c.Value.Unparsed = append((~c).Unparsed, parts[i]);
continueᴛ1:;
    }
    return (c, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string setCookieˢ = "Set-Cookie"u8;

// readSetCookies parses all "Set-Cookie" values from
// the header h and returns the successfully parsed Cookies.
//
// If the amount of cookies exceeds CookieNumLimit, and httpcookielimitnum
// GODEBUG option is not explicitly turned off, this function will silently
// fail and return an empty slice.
internal static slice<ж<ΔCookie>> readSetCookies(ΔHeader h) {
    nint cookieCount = builtin.len(h[setCookieˢ]);
    if (cookieCount == 0) {
        return new ж<ΔCookie>[]{}.slice();
    }
    // Cookie limit was unfortunately introduced at a later point in time.
    // As such, we can only fail by returning an empty slice rather than
    // explicit error.
    if (!cookieNumWithinMax(cookieCount)) {
        return new ж<ΔCookie>[]{}.slice();
    }
    var cookies = new slice<ж<ΔCookie>>(0, cookieCount);
    foreach (var (_, line) in h[setCookieˢ]) {
        {
            var (cookie, err) = ParseSetCookie(line); if (err == default!) {
                cookies = append(cookies, cookie);
            }
        }
    }
    return cookies;
}

// SetCookie adds a Set-Cookie header to the provided [ResponseWriter]'s headers.
// The provided cookie must have a valid Name. Invalid cookies may be
// silently dropped.
public static void SetCookie(ResponseWriter w, ж<ΔCookie> Ꮡcookie) {
    {
        @string v = Ꮡcookie.String(); if (v != ""u8) {
            w.Header().Add(setCookieˢ, v);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pathˢ = "; Path="u8;
internal static readonly @string domainˢ = "; Domain="u8;
internal static readonly @string expiresˢ = "; Expires="u8;
internal static readonly @string maxAgeˢ = "; Max-Age="u8;
internal static readonly @string maxAge0ˢ = "; Max-Age=0"u8;
internal static readonly @string httpOnlyˢ = "; HttpOnly"u8;
internal static readonly @string secureˢ = "; Secure"u8;
internal static readonly @string sameSiteNoneˢ = "; SameSite=None"u8;
internal static readonly @string sameSiteLaxˢ = "; SameSite=Lax"u8;
internal static readonly @string sameSiteStrictˢ = "; SameSite=Strict"u8;
internal static readonly @string partitionedˢ = "; Partitioned"u8;

// String returns the serialization of the cookie for use in a [Cookie]
// header (if only Name and Value are set) or a Set-Cookie response
// header (if other fields are set).
// If c is nil or c.Name is invalid, the empty string is returned.
public static @string String(this ж<ΔCookie> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (Ꮡc == nil || !isCookieNameValid(c.Name)) {
        return ""u8;
    }
    // extraCookieLength derived from typical length of cookie attributes
    // see RFC 6265 Sec 4.1.
    const nint extraCookieLength = 110;
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    Ꮡb.Grow(builtin.len(c.Name) + builtin.len(c.Value) + builtin.len(c.Domain) + builtin.len(c.Path) + extraCookieLength);
    Ꮡb.WriteString(c.Name);
    Ꮡb.WriteRune((rune)'=');
    Ꮡb.WriteString(sanitizeCookieValue(c.Value, c.Quoted));
    if (builtin.len(c.Path) > 0) {
        Ꮡb.WriteString(pathˢ);
        Ꮡb.WriteString(sanitizeCookiePath(c.Path));
    }
    if (builtin.len(c.Domain) > 0) {
        if (validCookieDomain(c.Domain)){
            // A c.Domain containing illegal characters is not
            // sanitized but simply dropped which turns the cookie
            // into a host-only cookie. A leading dot is okay
            // but won't be sent.
            @string d = c.Domain;
            if (d[0] == (rune)'.') {
                d = d[1..];
            }
            Ꮡb.WriteString(domainˢ);
            Ꮡb.WriteString(d);
        } else {
            log.Printf("net/http: invalid Cookie.Domain %q; dropping domain attribute"u8, c.Domain);
        }
    }
    array<byte> buf = new(29); /* builtin.len(TimeFormat) */
    if (validCookieExpires(c.Expires)) {
        Ꮡb.WriteString(expiresˢ);
        Ꮡb.Write(c.Expires.UTC().AppendFormat(buf[..0], TimeFormat));
    }
    if (c.MaxAge > 0){
        Ꮡb.WriteString(maxAgeˢ);
        Ꮡb.Write(strconv.AppendInt(buf[..0], (int64)c.MaxAge, 10));
    } else 
    if (c.MaxAge < 0) {
        Ꮡb.WriteString(maxAge0ˢ);
    }
    if (c.HttpOnly) {
        Ꮡb.WriteString(httpOnlyˢ);
    }
    if (c.Secure) {
        Ꮡb.WriteString(secureˢ);
    }
    var exprᴛ1 = c.SameSite;
    if (exprᴛ1 == SameSiteDefaultMode) {
    }
    else if (exprᴛ1 == SameSiteNoneMode) {
        Ꮡb.WriteString(sameSiteNoneˢ);
    }
    else if (exprᴛ1 == SameSiteLaxMode) {
        Ꮡb.WriteString(sameSiteLaxˢ);
    }
    else if (exprᴛ1 == SameSiteStrictMode) {
        Ꮡb.WriteString(sameSiteStrictˢ);
    }

    // Skip, default mode is obtained by not emitting the attribute.
    if (c.Partitioned) {
        Ꮡb.WriteString(partitionedˢ);
    }
    return b.String();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpNilCookieˢ = "http: nil Cookie"u8;
internal static readonly @string httpInvalidCookieNameˢ = "http: invalid Cookie.Name"u8;
internal static readonly @string httpInvalidCookieExpiresˢ = "http: invalid Cookie.Expires"u8;
internal static readonly @string httpInvalidCookieDomainˢ = "http: invalid Cookie.Domain"u8;
internal static readonly @string httpPartitionedCookiesˢ = "http: partitioned cookies must be set with Secure"u8;

// Valid reports whether the cookie is valid.
public static error Valid(this ж<ΔCookie> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (Ꮡc == nil) {
        return errors.New(httpNilCookieˢ);
    }
    if (!isCookieNameValid(c.Name)) {
        return errors.New(httpInvalidCookieNameˢ);
    }
    if (!c.Expires.IsZero() && !validCookieExpires(c.Expires)) {
        return errors.New(httpInvalidCookieExpiresˢ);
    }
    for (nint i = 0; i < builtin.len(c.Value); i++) {
        if (!validCookieValueByte(c.Value[i])) {
            return fmt.Errorf("http: invalid byte %q in Cookie.Value"u8, c.Value[i]);
        }
    }
    if (builtin.len(c.Path) > 0) {
        for (nint i = 0; i < builtin.len(c.Path); i++) {
            if (!validCookiePathByte(c.Path[i])) {
                return fmt.Errorf("http: invalid byte %q in Cookie.Path"u8, c.Path[i]);
            }
        }
    }
    if (builtin.len(c.Domain) > 0) {
        if (!validCookieDomain(c.Domain)) {
            return errors.New(httpInvalidCookieDomainˢ);
        }
    }
    if (c.Partitioned) {
        if (!c.Secure) {
            return errors.New(httpPartitionedCookiesˢ);
        }
    }
    return default!;
}

// readCookies parses all "Cookie" values from the header h and
// returns the successfully parsed Cookies.
//
// If filter isn't empty, only cookies of that name are returned.
//
// If the amount of cookies exceeds CookieNumLimit, and httpcookielimitnum
// GODEBUG option is not explicitly turned off, this function will silently
// fail and return an empty slice.
internal static slice<ж<ΔCookie>> readCookies(ΔHeader h, @string filter) {
    var lines = h[cookieˢ];
    if (builtin.len(lines) == 0) {
        return new ж<ΔCookie>[]{}.slice();
    }
    // Cookie limit was unfortunately introduced at a later point in time.
    // As such, we can only fail by returning an empty slice rather than
    // explicit error.
    nint cookieCount = 0;
    foreach (var (_, line) in lines) {
        cookieCount += strings.Count(line, ";"u8) + 1;
    }
    if (!cookieNumWithinMax(cookieCount)) {
        return new ж<ΔCookie>[]{}.slice();
    }
    var cookies = new slice<ж<ΔCookie>>(0, builtin.len(lines) + strings.Count(lines[0], ";"u8));
    foreach (var (_, vᴛ1) in lines) {
        var line = vᴛ1;

        line = textproto.TrimString(line);
        @string part = default!;
        while (builtin.len(line) > 0) {
            // continue since we have rest
            (part, line, _) = strings.Cut(line, ";"u8);
            part = textproto.TrimString(part);
            if (part == ""u8) {
                continue;
            }
            ref var name = ref heap<@string>(out var Ꮡname);
            ref var val = ref heap<@string>(out var Ꮡval);
            (name, val, _) = strings.Cut(part, "="u8);
            name = textproto.TrimString(name);
            if (!isCookieNameValid(name)) {
                continue;
            }
            if (filter != ""u8 && filter != name) {
                continue;
            }
            ref var quoted = ref heap<bool>(out var Ꮡquoted);
            (val, quoted, var ok) = parseCookieValue(val, true);
            if (!ok) {
                continue;
            }
            cookies = append(cookies, Ꮡ(new ΔCookie(Name: name, Value: val, Quoted: quoted)));
        }
    }
    return cookies;
}

// validCookieDomain reports whether v is a valid cookie domain-value.
internal static bool validCookieDomain(@string v) {
    if (isCookieDomainName(v)) {
        return true;
    }
    if (net.ParseIP(v) != default! && !strings.Contains(v, ":"u8)) {
        return true;
    }
    return false;
}

// validCookieExpires reports whether v is a valid cookie expires-value.
internal static bool validCookieExpires(time.Time t) {
    // IETF RFC 6265 Section 5.1.1.5, the year must not be less than 1601
    return t.Year() >= 1601;
}

// isCookieDomainName reports whether s is a valid domain name or a valid
// domain name with a leading dot '.'.  It is almost a direct copy of
// package net's isDomainName.
internal static bool isCookieDomainName(@string s) {
    if (builtin.len(s) == 0) {
        return false;
    }
    if (builtin.len(s) > 255) {
        return false;
    }
    if (s[0] == (rune)'.') {
        // A cookie a domain attribute may start with a leading dot.
        s = s[1..];
    }
    var last = (byte)(rune)'.';
    var ok = false; // Ok once we've seen a letter.
    nint partlen = 0;
    for (nint i = 0; i < builtin.len(s); i++) {
        var c = s[i];
        switch (ᐧ) {
        default: {
            return false;
        }
        case {} when (rune)'a' <= c && c <= (rune)'z' || (rune)'A' <= c && c <= (rune)'Z': {
            ok = true;
            partlen++;
            break;
        }
        case {} when (rune)'0' <= c && c <= (rune)'9': {
            partlen++;
            break;
        }
        case {} when c is (rune)'-': {
            if (last == (rune)'.') {
                // No '_' allowed here (in contrast to package net).
                // fine
                // Byte before dash cannot be dot.
                return false;
            }
            partlen++;
            break;
        }
        case {} when c is (rune)'.': {
            if (last == (rune)'.' || last == (rune)'-') {
                // Byte before dot cannot be dot, dash.
                return false;
            }
            if (partlen > 63 || partlen == 0) {
                return false;
            }
            partlen = 0;
            break;
        }}

        last = c;
    }
    if (last == (rune)'-' || partlen > 63) {
        return false;
    }
    return ok;
}

internal static ж<strings.Replacer> cookieNameSanitizer = strings.NewReplacer("\n"u8, "-", "\r", "-");

internal static @string sanitizeCookieName(@string n) {
    return cookieNameSanitizer.Replace(n);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cookieValueˢ = "Cookie.Value"u8;

// sanitizeCookieValue produces a suitable cookie-value from v.
// It receives a quoted bool indicating whether the value was originally
// quoted.
// https://tools.ietf.org/html/rfc6265#section-4.1.1
//
//	cookie-value      = *cookie-octet / ( DQUOTE *cookie-octet DQUOTE )
//	cookie-octet      = %x21 / %x23-2B / %x2D-3A / %x3C-5B / %x5D-7E
//	          ; US-ASCII characters excluding CTLs,
//	          ; whitespace DQUOTE, comma, semicolon,
//	          ; and backslash
//
// We loosen this as spaces and commas are common in cookie values
// thus we produce a quoted cookie-value if v contains commas or spaces.
// See https://golang.org/issue/7243 for the discussion.
internal static @string sanitizeCookieValue(@string v, bool quoted) {
    v = sanitizeOrWarn(cookieValueˢ, validCookieValueByte, v);
    if (builtin.len(v) == 0) {
        return v;
    }
    if (strings.ContainsAny(v, " ,"u8) || quoted) {
        return @""""u8 + v + @""""u8;
    }
    return v;
}

internal static bool validCookieValueByte(byte b) {
    return 0x20 <= b && b < 0x7f && b != (rune)'"' && b != (rune)';' && b != (rune)'\\';
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cookiePathˢ = "Cookie.Path"u8;

// path-av           = "Path=" path-value
// path-value        = <any CHAR except CTLs or ";">
internal static @string sanitizeCookiePath(@string v) {
    return sanitizeOrWarn(cookiePathˢ, validCookiePathByte, v);
}

internal static bool validCookiePathByte(byte b) {
    return 0x20 <= b && b < 0x7f && b != (rune)';';
}

internal static @string sanitizeOrWarn(@string fieldName, Func<byte, bool> valid, @string v) {
    var ok = true;
    for (nint i = 0; i < builtin.len(v); i++) {
        if (valid(v[i])) {
            continue;
        }
        log.Printf("net/http: invalid byte %q in %s; dropping invalid bytes"u8, v[i], fieldName);
        ok = false;
        break;
    }
    if (ok) {
        return v;
    }
    var buf = new slice<byte>(0, builtin.len(v));
    for (nint i = 0; i < builtin.len(v); i++) {
        {
            var b = v[i]; if (valid(b)) {
                buf = append(buf, b);
            }
        }
    }
    return ((@string)buf);
}

// parseCookieValue parses a cookie value according to RFC 6265.
// If allowDoubleQuote is true, parseCookieValue will consider that it
// is parsing the cookie-value;
// otherwise, it will consider that it is parsing a cookie-av value
// (cookie attribute-value).
//
// It returns the parsed cookie value, a boolean indicating whether the
// parsing was successful, and a boolean indicating whether the parsed
// value was enclosed in double quotes.
internal static (@string value, bool quoted, bool ok) parseCookieValue(@string raw, bool allowDoubleQuote) {
    bool quoted = default!;

    // Strip the quotes, if present.
    if (allowDoubleQuote && builtin.len(raw) > 1 && raw[0] == (rune)'"' && raw[builtin.len(raw) - 1] == (rune)'"') {
        raw = raw.slice(1, builtin.len(raw) - 1);
        quoted = true;
    }
    for (nint i = 0; i < builtin.len(raw); i++) {
        if (!validCookieValueByte(raw[i])) {
            return ("", quoted, false);
        }
    }
    return (raw, quoted, true);
}

internal static bool isCookieNameValid(@string raw) {
    if (raw == ""u8) {
        return false;
    }
    return strings.IndexFunc(raw, isNotToken) < 0;
}

} // end http_package
