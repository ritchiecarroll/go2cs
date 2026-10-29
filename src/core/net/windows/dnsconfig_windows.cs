// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using windows = @internal.syscall.windows_package;
using syscall = syscall_package;
using time = time_package;
using @internal.syscall;

partial class net_package {

internal static ж<dnsConfig> /*conf*/ dnsReadConfig(@string ignoredFilename) {
    ж<dnsConfig> conf = default;
    GoFrame ᒐ = default;
    try {
        conf = Ꮡ(new dnsConfig(
            ndots: 1,
            timeout: (time.Duration)(5000000000L),
            attempts: 2
        ));
        defer(() => {
            if (len(conf.servers) == 0) {
                conf.servers = defaultNS;
            }
        }, ref ᒐ);
        var (aas, err) = adapterAddresses();
        if (err != nil) {
            goto ᒐdone;
        }

        foreach (var (_, aa) in aas) {
            // Only take interfaces whose OperStatus is IfOperStatusUp(0x01) into DNS configs.
            if (aa.OperStatus != windows.IfOperStatusUp) {
                continue;
            }

            // Only take interfaces which have at least one gateway
            if (aa.FirstGatewayAddress == nil) {
                continue;
            }

            for (var dns = aa.FirstDnsServerAddress; dns != nil; dns = dns.Next) {
                var (sa, errΔ1) = dns.Address.Sockaddr.Sockaddr();
                if (errΔ1 != nil) {
                    continue;
                }
                IP ip = default;
                switch (sa.type()) {
                case ж<syscall.SockaddrInet4> saΔ1: {
                    ip = IPv4(saΔ1.Addr[0], saΔ1.Addr[1], saΔ1.Addr[2], saΔ1.Addr[3]);
                    break;
                }
                case ж<syscall.SockaddrInet6> saΔ1: {
                    ip = new IP(IPv6len);
                    copy(ip, saΔ1.Addr[..]);
                    if (ip[0] == 0xfe && ip[1] == 0xc0) {
                        // fec0/10 IPv6 addresses are site local anycast DNS
                        // addresses Microsoft sets by default if no other
                        // IPv6 DNS address is set. Site local anycast is
                        // deprecated since 2004, see
                        // https://datatracker.ietf.org/doc/html/rfc3879
                        continue;
                    }
                    break;
                }
                default: {
                    var saΔ1 = sa;
                    // Unexpected type.
                    continue;
                    break;
                }}
                conf.servers = append(conf.servers, JoinHostPort(ip.String(), "53"u8));
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return conf;
}

} // end net_package
