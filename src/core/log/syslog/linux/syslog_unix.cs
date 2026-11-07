// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !windows && !plan9
namespace go.log;

using errors = errors_package;
using net = net_package;
using time = time_package;

partial class syslog_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unixSyslogDeliveryErrorˢ => ref unixSyslogDeliveryErrorˢᵇ;
private static readonly @string unixSyslogDeliveryErrorˢᵇ = "Unix syslog delivery error"u8;

// unixSyslog opens a connection to the syslog daemon running on the
// local machine using a Unix domain socket.
internal static (serverConn conn, error err) unixSyslog() {
    var logTypes = slice<@string>(["unixgram", "unix"]);
    var logPaths = slice<@string>(["/dev/log", "/var/run/syslog", "/var/run/log"]);
    foreach (var (_, network) in range(logTypes)) {
        foreach (var (_, path) in range(logPaths)) {
            var (connΔ1, errΔ1) = net.Dial(network, path);
            if (errΔ1 == nil) {
                return (new netConnжserverConn(Ꮡ(new netConn(conn: connΔ1, local: true))), default);
            }
        }
    }
    return (default, errors.New(unixSyslogDeliveryErrorˢ));
}

} // end syslog_package
