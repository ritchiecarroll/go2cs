// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package httpproxy provides support for HTTP proxy determination
// based on environment variables, as provided by net/http's
// ProxyFromEnvironment function.
//
// The API is not subject to the Go 1 compatibility promise and may change at
// any time.
namespace go.vendor.golang.org.x.net.http;

using errors = errors_package;
using fmt = fmt_package;
using net = net_package;
using netip = go.net.netip_package;
using url = go.net.url_package;
using os = os_package;
using strings = strings_package;
using utf8 = unicode.utf8_package;
using idna = go.vendor.golang.org.x.net.idna_package;
using go.net;
using go.vendor.golang.org.x.net;
using unicode;
using ꓸꓸꓸstring = Span<@string>;

partial class httpproxy_package {

// Config holds configuration for HTTP proxy settings. See
// FromEnvironment for details.
[GoType] partial struct Config {
    // HTTPProxy represents the value of the HTTP_PROXY or
    // http_proxy environment variable. It will be used as the proxy
    // URL for HTTP requests unless overridden by NoProxy.
    public @string HTTPProxy;
    // HTTPSProxy represents the HTTPS_PROXY or https_proxy
    // environment variable. It will be used as the proxy URL for
    // HTTPS requests unless overridden by NoProxy.
    public @string HTTPSProxy;
    // NoProxy represents the NO_PROXY or no_proxy environment
    // variable. It specifies a string that contains comma-separated values
    // specifying hosts that should be excluded from proxying. Each value is
    // represented by an IP address prefix (1.2.3.4), an IP address prefix in
    // CIDR notation (1.2.3.4/8), a domain name, or a special DNS label (*).
    // An IP address prefix and domain name can also include a literal port
    // number (1.2.3.4:80).
    // A domain name matches that name and all subdomains. A domain name with
    // a leading "." matches subdomains only. For example "foo.com" matches
    // "foo.com" and "bar.foo.com"; ".y.com" matches "x.y.com" but not "y.com".
    // A single asterisk (*) indicates that no proxying should be done.
    // A best effort is made to parse the string and errors are
    // ignored.
    public @string NoProxy;
    // CGI holds whether the current process is running
    // as a CGI handler (FromEnvironment infers this from the
    // presence of a REQUEST_METHOD environment variable).
    // When this is set, ProxyForURL will return an error
    // when HTTPProxy applies, because a client could be
    // setting HTTP_PROXY maliciously. See https://golang.org/s/cgihttpproxy.
    public bool CGI;
}

// config holds the parsed configuration for HTTP proxy settings.
[GoType] partial struct config {
    // Config represents the original configuration as defined above.
    public partial ref Config Config { get; }
    // httpsProxy is the parsed URL of the HTTPSProxy if defined.
    internal ж<url.URL> httpsProxy;
    // httpProxy is the parsed URL of the HTTPProxy if defined.
    internal ж<url.URL> httpProxy;
    // ipMatchers represent all values in the NoProxy that are IP address
    // prefixes or an IP address in CIDR notation.
    internal slice<matcher> ipMatchers;
    // domainMatchers represent all values in the NoProxy that are a domain
    // name or hostname & domain name
    internal slice<matcher> domainMatchers;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string httpProxyˢ = "HTTP_PROXY"u8;
private static readonly @string httpProxyˢ2 = "http_proxy"u8;
private static readonly @string httpsProxyˢ = "HTTPS_PROXY"u8;
private static readonly @string httpsProxyˢ2 = "https_proxy"u8;
private static readonly @string noProxyˢ = "NO_PROXY"u8;
private static readonly @string noProxyˢ2 = "no_proxy"u8;
private static readonly @string requestMethodˢ = "REQUEST_METHOD"u8;

// FromEnvironment returns a Config instance populated from the
// environment variables HTTP_PROXY, HTTPS_PROXY and NO_PROXY (or the
// lowercase versions thereof).
//
// The environment values may be either a complete URL or a
// "host[:port]", in which case the "http" scheme is assumed. An error
// is returned if the value is a different form.
public static ж<Config> FromEnvironment() {
    return Ꮡ(new Config(
        HTTPProxy: getEnvAny(httpProxyˢ, httpProxyˢ2),
        HTTPSProxy: getEnvAny(httpsProxyˢ, httpsProxyˢ2),
        NoProxy: getEnvAny(noProxyˢ, noProxyˢ2),
        CGI: os.Getenv(requestMethodˢ) != ""u8
    ));
}

internal static @string getEnvAny(params ꓸꓸꓸstring namesʗp) {
    var names = namesʗp.sslice();

    foreach (var (_, n) in names) {
        {
            @string val = os.Getenv(n); if (val != ""u8) {
                return val;
            }
        }
    }
    return ""u8;
}

// ProxyFunc returns a function that determines the proxy URL to use for
// a given request URL. Changing the contents of cfg will not affect
// proxy functions created earlier.
//
// A nil URL and nil error are returned if no proxy is defined in the
// environment, or a proxy should not be used for the given request, as
// defined by NO_PROXY.
//
// As a special case, if req.URL.Host is "localhost" or a loopback address
// (with or without a port number), then a nil URL and nil error will be returned.
[GoRecv] public static Func<ж<url.URL>, (ж<url.URL>, error)> ProxyFunc(this ref Config cfg) {
    // Preprocess the Config settings for more efficient evaluation.
    var cfg1 = Ꮡ(new config(
        Config: cfg
    ));
    cfg1.init();
    return cfg1.proxyForURL;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string refusingToUseHttpProxyˢ = "refusing to use HTTP_PROXY value in CGI environment; see golang.org/s/cgihttpproxy"u8;

[GoRecv] internal static (ж<url.URL>, error) proxyForURL(this ref config cfg, ж<url.URL> ᏑreqURL) {
    ref var reqURL = ref ᏑreqURL.DerefOrNull();

    ж<url.URL> proxy = default!;
    if (reqURL.Scheme == "https"u8){
        proxy = cfg.httpsProxy;
    } else 
    if (reqURL.Scheme == "http"u8) {
        proxy = cfg.httpProxy;
        if (proxy != nil && cfg.CGI) {
            return (default!, errors.New(refusingToUseHttpProxyˢ));
        }
    }
    if (proxy == nil) {
        return (default!, default!);
    }
    if (!cfg.useProxy(canonicalAddr(ᏑreqURL))) {
        return (default!, default!);
    }
    return (proxy, default!);
}

internal static (ж<url.URL>, error) parseProxy(@string proxy) {
    if (proxy == ""u8) {
        return (default!, default!);
    }
    var (proxyURL, err) = url.Parse(proxy);
    if (err != default! || (~proxyURL).Scheme == ""u8 || (~proxyURL).Host == ""u8) {
        // proxy was bogus. Try prepending "http://" to it and
        // see if that parses correctly. If not, we fall
        // through and complain about the original one.
        {
            var (proxyURLΔ1, errΔ1) = url.Parse("http://"u8 + proxy); if (errΔ1 == default!) {
                return (proxyURLΔ1, default!);
            }
        }
    }
    if (err != default!) {
        return (default!, fmt.Errorf("invalid proxy address %q: %v"u8, proxy, err));
    }
    return (proxyURL, default!);
}

// useProxy reports whether requests to addr should use a proxy,
// according to the NO_PROXY or no_proxy environment variable.
// addr is always a canonicalAddr with a host and port.
[GoRecv] internal static bool useProxy(this ref config cfg, @string addr) {
    if (len(addr) == 0) {
        return true;
    }
    var (host, port, err) = net.SplitHostPort(addr);
    if (err != default!) {
        return false;
    }
    if (host == "localhost"u8) {
        return false;
    }
    (var nip, err) = netip.ParseAddr(host);
    net.IP ip = default!;
    if (err == default!) {
        ip = ((net.IP)nip.AsSlice());
        if (ip.IsLoopback()) {
            return false;
        }
    }
    addr = strings.ToLower(strings.TrimSpace(host));
    if (ip != default!) {
        foreach (var (_, m) in cfg.ipMatchers) {
            if (m.match(addr, port, ip)) {
                return false;
            }
        }
    }
    foreach (var (_, m) in cfg.domainMatchers) {
        if (m.match(addr, port, ip)) {
            return false;
        }
    }
    return true;
}

[GoRecv] internal static void init(this ref config c) {
    {
        var (parsed, err) = parseProxy(c.HTTPProxy); if (err == default!) {
            c.httpProxy = parsed;
        }
    }
    {
        var (parsed, err) = parseProxy(c.HTTPSProxy); if (err == default!) {
            c.httpsProxy = parsed;
        }
    }
    foreach (var (_, vᴛ1) in strings.Split(c.NoProxy, ","u8)) {
        var p = vᴛ1;

        p = strings.ToLower(strings.TrimSpace(p));
        if (len(p) == 0) {
            continue;
        }
        if (p == "*"u8) {
            c.ipMatchers = new matcher[]{new allMatch(nil)}.slice();
            c.domainMatchers = new matcher[]{new allMatch(nil)}.slice();
            return;
        }
        // IPv4/CIDR, IPv6/CIDR
        {
            var (_, pnet, errΔ1) = net.ParseCIDR(p); if (errΔ1 == default!) {
                c.ipMatchers = append(c.ipMatchers, (matcher)(new cidrMatch(cidr: pnet)));
                continue;
            }
        }
        // IPv4:port, [IPv6]:port
        var (phost, pport, err) = net.SplitHostPort(p);
        if (err == default!){
            if (len(phost) == 0) {
                // There is no host part, likely the entry is malformed; ignore.
                continue;
            }
            if (phost[0] == (rune)'[' && phost[len(phost) - 1] == (rune)']') {
                phost = phost.slice(1, len(phost) - 1);
            }
        } else {
            phost = p;
        }
        // IPv4, IPv6
        {
            var pip = net.ParseIP(phost); if (pip != default!) {
                c.ipMatchers = append(c.ipMatchers, (matcher)(new ipMatch(ip: pip, port: pport)));
                continue;
            }
        }
        if (len(phost) == 0) {
            // There is no host part, likely the entry is malformed; ignore.
            continue;
        }
        // domain.com or domain.com:80
        // foo.com matches bar.foo.com
        // .domain.com or .domain.com:port
        // *.domain.com or *.domain.com:port
        if (strings.HasPrefix(phost, "*."u8)) {
            phost = phost[1..];
        }
        var matchHost = false;
        if (phost[0] != (rune)'.') {
            matchHost = true;
            phost = "."u8 + phost;
        }
        {
            var (v, errΔ1) = idnaASCII(phost); if (errΔ1 == default!) {
                phost = v;
            }
        }
        c.domainMatchers = append(c.domainMatchers, (matcher)(new domainMatch(host: phost, port: pport, matchHost: matchHost)));
    }
}

internal static map<@string, @string> portMap = new map<@string, @string>{
    ["http"u8] = "80"u8,
    ["https"u8] = "443"u8,
    ["socks5"u8] = "1080"u8
};

// canonicalAddr returns url.Host but always with a ":port" suffix
internal static @string canonicalAddr(ж<url.URL> Ꮡurl) {
    ref var urlΔ1 = ref Ꮡurl.DerefOrNull();

    @string addr = urlΔ1.Hostname();
    {
        var (v, err) = idnaASCII(addr); if (err == default!) {
            addr = v;
        }
    }
    @string port = urlΔ1.Port();
    if (port == ""u8) {
        port = portMap[urlΔ1.Scheme];
    }
    return net.JoinHostPort(addr, port);
}

// Given a string of the form "host", "host:port", or "[ipv6::address]:port",
// return true if the string includes a port.
internal static bool hasPort(@string s) {
    return strings.LastIndex(s, ":"u8) > strings.LastIndex(s, "]"u8);
}

internal static (@string, error) idnaASCII(@string v) {
    // TODO: Consider removing this check after verifying performance is okay.
    // Right now punycode verification, length checks, context checks, and the
    // permissible character tests are all omitted. It also prevents the ToASCII
    // call from salvaging an invalid IDN, when possible. As a result it may be
    // possible to have two IDNs that appear identical to the user where the
    // ASCII-only version causes an error downstream whereas the non-ASCII
    // version does not.
    // Note that for correct ASCII IDNs ToASCII will only do considerably more
    // work, but it will not cause an allocation.
    if (isASCII(v)) {
        return (v, default!);
    }
    return idna.Lookup.ToASCII(v);
}

internal static bool isASCII(@string s) {
    for (nint i = 0; i < len(s); i++) {
        if (s[i] >= utf8.RuneSelf) {
            return false;
        }
    }
    return true;
}

// matcher represents the matching rule for a given value in the NO_PROXY list
[GoType] partial interface matcher {
    // match returns true if the host and optional port or ip and optional port
    // are allowed
    bool match(@string host, @string port, net.IP ip);
}

// allMatch matches on all possible inputs
[GoType] partial struct allMatch {
}

internal static bool match(this allMatch a, @string host, @string port, net.IP ip) {
    return true;
}

[GoType] partial struct cidrMatch {
    internal ж<net.IPNet> cidr;
}

internal static bool match(this cidrMatch m, @string host, @string port, net.IP ip) {
    return m.cidr.Contains(ip);
}

[GoType] partial struct ipMatch {
    internal net.IP ip;
    internal @string port;
}

internal static bool match(this ipMatch m, @string host, @string port, net.IP ip) {
    if (m.ip.Equal(ip)) {
        return m.port == ""u8 || m.port == port;
    }
    return false;
}

[GoType] partial struct domainMatch {
    internal @string host;
    internal @string port;
    internal bool matchHost;
}

internal static bool match(this domainMatch m, @string host, @string port, net.IP ip) {
    if (ip != default!) {
        return false;
    }
    if (strings.HasSuffix(host, m.host) || (m.matchHost && host == m.host[1..])) {
        return m.port == ""u8 || m.port == port;
    }
    return false;
}

} // end httpproxy_package
