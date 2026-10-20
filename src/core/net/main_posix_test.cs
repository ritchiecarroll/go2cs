// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !plan9
namespace go;

using socktest = net.@internal.socktest_package;
using strings = strings_package;
using syscall = syscall_package;
using net.@internal;
using static go.net_package;

partial class net_internal_test_package {

internal static void enableSocketConnect() {
    Ꮡsw.Set(socktest.FilterConnect, default!);
}

internal static void disableSocketConnect(@string network) {
    var (net, _, _) = strings.Cut(network, ":"u8);
    Ꮡsw.Set(socktest.FilterConnect, (ж<socktest.Status> so) => {
        var exprᴛ1 = net;
        if (exprᴛ1 == "tcp4"u8) {
            if ((~so).Cookie.Family() == syscall.AF_INET && (~so).Cookie.Type() == syscall.SOCK_STREAM) {
                return (default!, syscall.EHOSTUNREACH);
            }
        }
        else if (exprᴛ1 == "udp4"u8) {
            if ((~so).Cookie.Family() == syscall.AF_INET && (~so).Cookie.Type() == syscall.SOCK_DGRAM) {
                return (default!, syscall.EHOSTUNREACH);
            }
        }
        else if (exprᴛ1 == "ip4"u8) {
            if ((~so).Cookie.Family() == syscall.AF_INET && (~so).Cookie.Type() == syscall.SOCK_RAW) {
                return (default!, syscall.EHOSTUNREACH);
            }
        }
        else if (exprᴛ1 == "tcp6"u8) {
            if ((~so).Cookie.Family() == syscall.AF_INET6 && (~so).Cookie.Type() == syscall.SOCK_STREAM) {
                return (default!, syscall.EHOSTUNREACH);
            }
        }
        else if (exprᴛ1 == "udp6"u8) {
            if ((~so).Cookie.Family() == syscall.AF_INET6 && (~so).Cookie.Type() == syscall.SOCK_DGRAM) {
                return (default!, syscall.EHOSTUNREACH);
            }
        }
        else if (exprᴛ1 == "ip6"u8) {
            if ((~so).Cookie.Family() == syscall.AF_INET6 && (~so).Cookie.Type() == syscall.SOCK_RAW) {
                return (default!, syscall.EHOSTUNREACH);
            }
        }
        return (default!, default!);
    });
}

} // end net_internal_test_package
