// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using bytealg = @internal.bytealg_package;
using fs = go.io.fs_package;
using netip = net.netip_package;
using Δsync = sync_package;
using time = time_package;
using @internal;
using go.io;
using net;

partial class net_package {

internal static time.Duration cacheMaxAge => /* 5 * time.Second */ 5000000000;

internal static @string parseLiteralIP(@string addr) {
    var (ip, err) = netip.ParseAddr(addr);
    if (err != default!) {
        return ""u8;
    }
    return ip.String();
}

[GoType] partial struct byName {
    internal slice<@string> addrs;
    internal @string canonicalName;
}

// hosts contains known host entries.

[GoType("dyn")] partial struct hostsᴛ1 {
    public partial ref sync_package.Mutex Mutex { get; }
    // Key for the list of literal IP addresses must be a host
    // name. It would be part of DNS labels, a FQDN or an absolute
    // FQDN.
    // For now the key is converted to lower case for convenience.
    internal map<@string, byName> byName;
    // Key for the list of host names must be a literal IP address
    // including IPv6 address with zone identifier.
    // We don't support old-classful IP address notation.
    internal map<@string, slice<@string>> byAddr;
    internal time.Time expire;
    internal @string path;
    internal time.Time mtime;
    internal int64 size;
}
internal static ж<hostsᴛ1> Ꮡhosts = new StandardBox<hostsᴛ1>(new hostsᴛ1(nil));
internal static ref hostsᴛ1 hosts => ref Ꮡhosts.Value;

internal static void readHosts() {
    GoFrame ᒐ = default;
    try {
        var now = time.Now();
        @string hp = hostsFilePath;
        if (now.Before(hosts.expire) && hosts.path == hp && len(hosts.byName) > 0) {
            return;
        }
        var (mtime, size, err) = stat(hp);
        if (err == default! && hosts.path == hp && hosts.mtime.Equal(mtime) && hosts.size == size) {
            hosts.expire = now.Add(cacheMaxAge);
            return;
        }
        var hs = new map<@string, byName>();
        var @is = new map<@string, slice<@string>>();
        (var Δfile, err) = open(hp);
        if (err != default!) {
            if (!errors.Is(err, fs.ErrNotExist) && !errors.Is(err, fs.ErrPermission)) {
                return;
            }
        }
        if (Δfile != nil) {
            var fileʗ1 = Δfile;
            defer(fileʗ1.close, ref ᒐ);
            for (var (line, ok) = Δfile.readLine(); ok; (line, ok) = Δfile.readLine()) {
                {
                    nint i = bytealg.IndexByteString(line, (rune)'#'); if (i >= 0) {
                        // Discard comments.
                        line = line.slice(0, i);
                    }
                }
                var f = getFields(line);
                if (len(f) < 2) {
                    continue;
                }
                @string addr = parseLiteralIP(f[0]);
                if (addr == ""u8) {
                    continue;
                }
                @string canonical = default!;
                for (nint i = 1; i < len(f); i++) {
                    @string name = absDomainName(f[i]);
                    var h = slice<byte>(f[i]);
                    lowerASCIIBytes(h);
                    @string key = absDomainName(((@string)h));
                    if (i == 1) {
                        canonical = key;
                    }
                    @is[addr] = append(@is[addr], name);
                    {
                        var (v, okΔ1) = hs[key, ꟷ]; if (okΔ1) {
                            hs[key] = new byName(
                                addrs: append(v.addrs, addr),
                                canonicalName: v.canonicalName
                            );
                            continue;
                        }
                    }
                    hs[key] = new byName(
                        addrs: new @string[]{addr}.slice(),
                        canonicalName: canonical
                    );
                }
            }
        }
        // Update the data cache.
        hosts.expire = now.Add(cacheMaxAge);
        hosts.path = hp;
        hosts.byName = hs;
        hosts.byAddr = @is;
        hosts.mtime = mtime;
        hosts.size = size;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// lookupStaticHost looks up the addresses and the canonical name for the given host from /etc/hosts.
internal static (slice<@string>, @string) lookupStaticHost(@string host) {
    GoFrame ᒐ = default;
    try {
        Ꮡhosts.of(hostsᴛ1.ᏑMutex).Lock();
        defer(Ꮡhosts.of(hostsᴛ1.ᏑMutex).Unlock, ref ᒐ);
        readHosts();
        if (len(hosts.byName) != 0) {
            if (hasUpperCase(host)) {
                var lowerHost = slice<byte>(host);
                lowerASCIIBytes(lowerHost);
                host = ((@string)lowerHost);
            }
            {
                var (byName, ok) = hosts.byName[absDomainName(host), ꟷ]; if (ok) {
                    var ipsCp = new slice<@string>(len(byName.addrs));
                    copy(ipsCp, byName.addrs);
                    return (ipsCp, byName.canonicalName);
                }
            }
        }
        return (default!, "");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// lookupStaticAddr looks up the hosts for the given address from /etc/hosts.
internal static slice<@string> lookupStaticAddr(@string addr) {
    GoFrame ᒐ = default;
    try {
        Ꮡhosts.of(hostsᴛ1.ᏑMutex).Lock();
        defer(Ꮡhosts.of(hostsᴛ1.ᏑMutex).Unlock, ref ᒐ);
        readHosts();
        addr = parseLiteralIP(addr);
        if (addr == ""u8) {
            return default!;
        }
        if (len(hosts.byAddr) != 0) {
            {
                var (hostsΔ1, ok) = hosts.byAddr[addr, ꟷ]; if (ok) {
                    var hostsCp = new slice<@string>(len(hostsΔ1));
                    copy(hostsCp, hostsΔ1);
                    return hostsCp;
                }
            }
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

} // end net_package
