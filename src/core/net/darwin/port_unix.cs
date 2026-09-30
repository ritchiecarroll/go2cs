// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || js || wasip1
// Read system port mappings from /etc/services
namespace go;

using bytealg = @internal.bytealg_package;
using Δsync = sync_package;
using @internal;

partial class net_package {

internal static ж<Δsync.Once> ᏑonceReadServices = new StandardBox<Δsync.Once>(default(Δsync.Once));
internal static ref Δsync.Once onceReadServices => ref ᏑonceReadServices.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string etcServicesˢ = "/etc/services"u8;

internal static void readServices() {
    GoFrame ᒐ = default;
    try {
        var (Δfile, err) = open(etcServicesˢ);
        if (err != default!) {
            return;
        }
        var fileʗ1 = Δfile;
        defer(fileʗ1.close, ref ᒐ);
        for (var (line, ok) = Δfile.readLine(); ok; (line, ok) = Δfile.readLine()) {
            // "http 80/tcp www www-http # World Wide Web HTTP"
            {
                nint i = bytealg.IndexByteString(line, (rune)'#'); if (i >= 0) {
                    line = line.slice(0, i);
                }
            }
            var f = getFields(line);
            if (len(f) < 2) {
                continue;
            }
            @string portnet = f[1]; // "80/tcp"
            var (port, j, okΔ1) = dtoi(portnet);
            if (!okΔ1 || port <= 0 || j >= len(portnet) || portnet[j] != (rune)'/') {
                continue;
            }
            @string netw = portnet.slice(j + 1); // "tcp"
            var (m, ok1) = services[netw, ꟷ];
            if (!ok1) {
                m = new map<@string, nint>();
                services[netw] = m;
            }
            for (nint i = 0; i < len(f); i++) {
                if (i != 1) {
                    // f[1] was port/net
                    m[f[i]] = port;
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// goLookupPort is the native Go implementation of LookupPort.
internal static (nint port, error err) goLookupPort(@string network, @string service) {
    ᏑonceReadServices.Do(readServices);
    return lookupPortMap(network, service);
}

} // end net_package
