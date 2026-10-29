// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !windows
// Read system DNS config from /etc/resolv.conf
namespace go;

using bytealg = @internal.bytealg_package;
using stringslite = @internal.stringslite_package;
using netip = net.netip_package;
using time = time_package;
using @internal;
using fs = go.io.fs_package;
using go.io;
using net;

partial class net_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ndotsˢ = "ndots:"u8;
internal static readonly @string timeoutˢ = "timeout:"u8;
internal static readonly @string attemptsˢ = "attempts:"u8;

// See resolv.conf(5) on a Linux machine.
internal static ж<dnsConfig> dnsReadConfig(@string filename) {
    GoFrame ᒐ = default;
    try {
        var conf = Ꮡ(new dnsConfig(
            ndots: 1,
            timeout: (time.Duration)(5000000000L),
            attempts: 2
        ));
        var (Δfile, err) = open(filename);
        if (err != nil) {
            conf.servers = defaultNS;
            conf.search = dnsDefaultSearch();
            conf.err = err;
            return conf;
        }
        var fileʗ1 = Δfile;
        defer(fileʗ1.close, ref ᒐ);
        {
            var (fi, errΔ1) = Δfile.ΔΔfile.Stat(); if (errΔ1 == nil) {
                conf.mtime = fi.ModTime();
            } else {
                conf.servers = defaultNS;
                conf.search = dnsDefaultSearch();
                conf.err = errΔ1;
                return conf;
            }
        }
        for (var (line, ok) = Δfile.readLine(); ok; (line, ok) = Δfile.readLine()) {
            if (len(line) > 0 && (line[0] == ';' || line[0] == '#')) {
                // comment.
                continue;
            }
            var f = getFields(line);
            if (len(f) < 1) {
                continue;
            }
            var exprᴛ1 = f[0];
            switch (ᐧ) {
            case {} when exprᴛ1 == "nameserver"u8: { // add one name server
                if (len(f) > 1 && len(conf.servers) < 3) { // small, but the standard limit
                    // One more check: make sure server name is
                    // just an IP address. Otherwise we need DNS
                    // to look it up.
                    {
                        var (_, errΔ3) = netip.ParseAddr(f[1]); if (errΔ3 == nil) {
                            conf.servers = append(conf.servers, JoinHostPort(f[1], "53"u8));
                        }
                    }
                }
                break;
            }
            case {} when exprᴛ1 == "domain"u8: { // set search path to just this domain
                if (len(f) > 1) {
                    conf.search = slice<@string>([ensureRooted(f[1])]);
                }
                break;
            }
            case {} when exprᴛ1 == "search"u8: { // set search path to given servers
                conf.search = new slice<@string>(0, len(f) - 1);
                for (nint i = 1; i < len(f); i++) {
                    @string name = ensureRooted(f[i]);
                    if (name == "."u8) {
                        continue;
                    }
                    conf.search = append(conf.search, name);
                }
                break;
            }
            case {} when exprᴛ1 == "options"u8: { // magic options
                foreach (var (_, s) in f[1..]) {
                    switch (ᐧ) {
                    case {} when stringslite.HasPrefix(s, ndotsˢ): {
                        var (n, _, _) = dtoi(s[6..]);
                        if (n < 0) {
                            n = 0;
                        } else if (n > 15) {
                            n = 15;
                        }
                        conf.ndots = n;
                        break;
                    }
                    case {} when stringslite.HasPrefix(s, timeoutˢ): {
                        var (n, _, _) = dtoi(s[8..]);
                        if (n < 1) {
                            n = 1;
                        }
                        conf.timeout = ((time.Duration)(int64)n) * time.ΔSecond;
                        break;
                    }
                    case {} when stringslite.HasPrefix(s, attemptsˢ): {
                        var (n, _, _) = dtoi(s[9..]);
                        if (n < 1) {
                            n = 1;
                        }
                        conf.attempts = n;
                        break;
                    }
                    case {} when s == "rotate"u8: {
                        conf.rotate = true;
                        break;
                    }
                    case {} when s == "single-request"u8 || s == "single-request-reopen"u8: {
                        // Linux option:
                        // http://man7.org/linux/man-pages/man5/resolv.conf.5.html
                        // "By default, glibc performs IPv4 and IPv6 lookups in parallel [...]
                        //  This option disables the behavior and makes glibc
                        //  perform the IPv6 and IPv4 requests sequentially."
                        conf.singleRequest = true;
                        break;
                    }
                    case {} when s == "use-vc"u8 || s == "usevc"u8 || s == "tcp"u8: {
                        // Linux (use-vc), FreeBSD (usevc) and OpenBSD (tcp) option:
                        // http://man7.org/linux/man-pages/man5/resolv.conf.5.html
                        // "Sets RES_USEVC in _res.options.
                        //  This option forces the use of TCP for DNS resolutions."
                        // https://www.freebsd.org/cgi/man.cgi?query=resolv.conf&sektion=5&manpath=freebsd-release-ports
                        // https://man.openbsd.org/resolv.conf.5
                        conf.useTCP = true;
                        break;
                    }
                    case {} when s == "trust-ad"u8: {
                        conf.trustAD = true;
                        break;
                    }
                    case {} when s == "edns0"u8: {
                        // We use EDNS by default.
                        // Ignore this option.
                        break;
                    }
                    case {} when s == "no-reload"u8: {
                        conf.noReload = true;
                        break;
                    }
                    default: {
                        conf.unknownOpt = true;
                        break;
                    }}
                }
                break;
            }
            case {} when exprᴛ1 == "lookup"u8: {
                // OpenBSD option:
                // https://www.openbsd.org/cgi-bin/man.cgi/OpenBSD-current/man5/resolv.conf.5
                // "the legal space-separated values are: bind, file, yp"
                conf.lookup = f[1..];
                break;
            }
            default: {
                conf.unknownOpt = true;
                break;
            }}
        }
        if (len(conf.servers) == 0) {
            conf.servers = defaultNS;
        }
        if (len(conf.search) == 0) {
            conf.search = dnsDefaultSearch();
        }
        return conf;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static slice<@string> dnsDefaultSearch() {
    var (hn, err) = getHostname();
    if (err != nil) {
        // best effort
        return default;
    }
    {
        nint i = bytealg.IndexByteString(hn, (byte)'.'); if (i >= 0 && i < len(hn) - 1) {
            return slice<@string>([ensureRooted(hn.slice(i + 1))]);
        }
    }
    return default;
}

internal static @string ensureRooted(@string s) {
    if (len(s) > 0 && s[len(s) - 1] == '.') {
        return s;
    }
    return s + "."u8;
}

} // end net_package
