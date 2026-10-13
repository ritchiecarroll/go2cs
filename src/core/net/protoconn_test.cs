// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements API tests across platforms and will never have a build
// tag.
namespace go;

using testenv = @internal.testenv_package;
using Δos = os_package;
using Δruntime = runtime_package;
using testing = testing_package;
using time = time_package;
using @internal;
using static go.net_package;

partial class net_internal_test_package {

// The full stack test cases for IPConn have been moved to the
// following:
//	golang.org/x/net/ipv4
//	golang.org/x/net/ipv6
//	golang.org/x/net/icmp
public static void TestTCPListenerSpecificMethods(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
        }

        var (la, err) = ResolveTCPAddr(tcp4ˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var ln, err) = ListenTCP(tcp4ˢ, la);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        ln.Addr();
        mustSetDeadline(new net_test_package.testing_TжTB(Ꮡt), ln.SetDeadline, 30 * time.ΔNanosecond);

        {
            var (c, errΔ1) = ln.Accept(); if (errΔ1 != default!){
                if (!errΔ1._<ΔError>().Timeout()) {
                    Ꮡt.Fatal(errΔ1);
                }
            } else {
                c.Close();
            }
        }
        {
            var (c, errΔ2) = ln.AcceptTCP(); if (errΔ2 != default!){
                if (!errΔ2._<ΔError>().Timeout()) {
                    Ꮡt.Fatal(errΔ2);
                }
            } else {
                c.of(global::go.net_package.TCPConn.Ꮡconn).Close();
            }
        }

        {
            var (f, errΔ3) = ln.File(); if (errΔ3 != default!){
                condFatalf(Ꮡt, fileNetˢ, "%v"u8, errΔ3);
            } else {
                f.Close();
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestTCPConnSpecificMethods(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (la, err) = ResolveTCPAddr(tcp4ˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var ln, err) = ListenTCP(tcp4ˢ, la);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var ch = new channel<error>(1);
        var chʗ1 = ch;
        var handler = (ж<localServer> lsΔ1, global::go.net_package.Listener lnΔ1) => {
            lsΔ1.transponder((~lsΔ1).Listener, chʗ1.WithDirection(GoChanDir.Send));
        };
        var ls = (Ꮡ(new streamListener(Listener: new global::go.net_package.TCPListenerжListener(ln)))).newLocalServer();
        var lsʗ1 = ls;
        defer(() => lsʗ1.teardown(), ref ᒐ);
        {
            var errΔ1 = ls.buildup(handler); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }

        (var ra, err) = ResolveTCPAddr(tcp4ˢ, (~ls).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var c, err) = DialTCP(tcp4ˢ, nil, ra);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.of(global::go.net_package.TCPConn.Ꮡconn).Close(), ref ᒐ);
        c.SetKeepAlive(false);
        c.SetKeepAlivePeriod((time.Duration)(3000000000L));
        c.SetLinger(0);
        c.SetNoDelay(false);
        c.of(global::go.net_package.TCPConn.Ꮡconn).LocalAddr();
        c.of(global::go.net_package.TCPConn.Ꮡconn).RemoteAddr();
        c.of(global::go.net_package.TCPConn.Ꮡconn).SetDeadline(time.Now().Add(someTimeout));
        c.of(global::go.net_package.TCPConn.Ꮡconn).SetReadDeadline(time.Now().Add(someTimeout));
        c.of(global::go.net_package.TCPConn.Ꮡconn).SetWriteDeadline(time.Now().Add(someTimeout));

        {
            var (_, errΔ2) = c.of(global::go.net_package.TCPConn.Ꮡconn).Write(slice<byte>("TCPCONN TEST"u8)); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        var rb = new slice<byte>(128);
        {
            var (_, errΔ3) = c.of(global::go.net_package.TCPConn.Ꮡconn).Read(rb); if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
        }

        foreach (var errΔ4 in ch) {
            Ꮡt.Error(errΔ4);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestUDPConnSpecificMethods(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (la, err) = ResolveUDPAddr(udp4ˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var c, err) = ListenUDP(udp4ˢ, la);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.of(global::go.net_package.UDPConn.Ꮡconn).Close(), ref ᒐ);
        c.of(global::go.net_package.UDPConn.Ꮡconn).LocalAddr();
        c.of(global::go.net_package.UDPConn.Ꮡconn).RemoteAddr();
        c.of(global::go.net_package.UDPConn.Ꮡconn).SetDeadline(time.Now().Add(someTimeout));
        c.of(global::go.net_package.UDPConn.Ꮡconn).SetReadDeadline(time.Now().Add(someTimeout));
        c.of(global::go.net_package.UDPConn.Ꮡconn).SetWriteDeadline(time.Now().Add(someTimeout));
        c.of(global::go.net_package.UDPConn.Ꮡconn).SetReadBuffer(2048);
        c.of(global::go.net_package.UDPConn.Ꮡconn).SetWriteBuffer(2048);

        var wb = slice<byte>("UDPCONN TEST"u8);
        var rb = new slice<byte>(128);
        {
            var (_, errΔ1) = c.WriteToUDP(wb, c.of(global::go.net_package.UDPConn.Ꮡconn).LocalAddr()._<ж<global::go.net_package.UDPAddr>>()); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        {
            var (_, _, errΔ2) = c.ReadFromUDP(rb); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        {
            var (_, _, errΔ3) = c.WriteMsgUDP(wb, default!, c.of(global::go.net_package.UDPConn.Ꮡconn).LocalAddr()._<ж<global::go.net_package.UDPAddr>>()); if (errΔ3 != default!) {
                condFatalf(Ꮡt, c.of(global::go.net_package.UDPConn.Ꮡconn).LocalAddr().Network(), "%v"u8, errΔ3);
            }
        }
        {
            var (_, _, _, _, errΔ4) = c.ReadMsgUDP(rb, default!); if (errΔ4 != default!) {
                condFatalf(Ꮡt, c.of(global::go.net_package.UDPConn.Ꮡconn).LocalAddr().Network(), "%v"u8, errΔ4);
            }
        }

        {
            var (f, errΔ5) = c.of(global::go.net_package.UDPConn.Ꮡconn).File(); if (errΔ5 != default!){
                condFatalf(Ꮡt, fileNetˢ, "%v"u8, errΔ5);
            } else {
                f.Close();
            }
        }

        defer(() => {
            {
                var p = recover(); if (p != default!) {
                    Ꮡt.Fatalf("panicked: %v"u8, p);
                }
            }
        }, ref ᒐ);

        c.WriteToUDP(wb, nil);
        c.WriteMsgUDP(wb, default!, nil);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ip4ˢ = "ip4"u8;
internal static readonly object skippingIp4NotSupportedˢ = (@string)"skipping: ip4 not supported"u8;
internal static readonly @string ip4Icmpˢ = "ip4:icmp"u8;

public static void TestIPConnSpecificMethods(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (!testableNetwork(ip4ˢ)) {
            Ꮡt.Skip(skippingIp4NotSupportedˢ);
        }

        var (la, err) = ResolveIPAddr(ip4ˢ, "127.0.0.1"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var c, err) = ListenIP(ip4Icmpˢ, la);
        if (testenv.SyscallIsNotSupported(err)){
            // May be inside a container that disallows creating a socket or
            // not running as root.
            Ꮡt.Skipf("skipping: %v"u8, err);
        } else 
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.of(global::go.net_package.IPConn.Ꮡconn).Close(), ref ᒐ);
        c.of(global::go.net_package.IPConn.Ꮡconn).LocalAddr();
        c.of(global::go.net_package.IPConn.Ꮡconn).RemoteAddr();
        c.of(global::go.net_package.IPConn.Ꮡconn).SetDeadline(time.Now().Add(someTimeout));
        c.of(global::go.net_package.IPConn.Ꮡconn).SetReadDeadline(time.Now().Add(someTimeout));
        c.of(global::go.net_package.IPConn.Ꮡconn).SetWriteDeadline(time.Now().Add(someTimeout));
        c.of(global::go.net_package.IPConn.Ꮡconn).SetReadBuffer(2048);
        c.of(global::go.net_package.IPConn.Ꮡconn).SetWriteBuffer(2048);

        {
            var (f, errΔ1) = c.of(global::go.net_package.IPConn.Ꮡconn).File(); if (errΔ1 != default!){
                condFatalf(Ꮡt, fileNetˢ, "%v"u8, errΔ1);
            } else {
                f.Close();
            }
        }

        defer(() => {
            {
                var p = recover(); if (p != default!) {
                    Ꮡt.Fatalf("panicked: %v"u8, p);
                }
            }
        }, ref ᒐ);

        var wb = slice<byte>("IPCONN TEST"u8);
        c.WriteToIP(wb, nil);
        c.WriteMsgIP(wb, default!, nil);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unixTestˢ = (@string)"unix test"u8;

public static void TestUnixListenerSpecificMethods(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (!testableNetwork(unixˢ)) {
            Ꮡt.Skip(unixTestˢ);
        }

        @string addr = testUnixAddr(new net_test_package.testing_TжTB(Ꮡt));
        var (la, err) = ResolveUnixAddr(unixˢ, addr);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var ln, err) = ListenUnix(unixˢ, la);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        defer(Δos.Remove, addr, ref ᒐ);
        ln.Addr();
        mustSetDeadline(new net_test_package.testing_TжTB(Ꮡt), ln.SetDeadline, 30 * time.ΔNanosecond);

        {
            var (c, errΔ1) = ln.Accept(); if (errΔ1 != default!){
                if (!errΔ1._<ΔError>().Timeout()) {
                    Ꮡt.Fatal(errΔ1);
                }
            } else {
                c.Close();
            }
        }
        {
            var (c, errΔ2) = ln.AcceptUnix(); if (errΔ2 != default!){
                if (!errΔ2._<ΔError>().Timeout()) {
                    Ꮡt.Fatal(errΔ2);
                }
            } else {
                c.of(global::go.net_package.UnixConn.Ꮡconn).Close();
            }
        }

        {
            var (f, errΔ3) = ln.File(); if (errΔ3 != default!){
                condFatalf(Ꮡt, fileNetˢ, "%v"u8, errΔ3);
            } else {
                f.Close();
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unixgramTestˢ = (@string)"unixgram test"u8;

public static void TestUnixConnSpecificMethods(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (!testableNetwork(unixgramˢ)) {
            Ꮡt.Skip(unixgramTestˢ);
        }

        @string addr1 = testUnixAddr(new net_test_package.testing_TжTB(Ꮡt));
        @string addr2 = testUnixAddr(new net_test_package.testing_TжTB(Ꮡt));
        @string addr3 = testUnixAddr(new net_test_package.testing_TжTB(Ꮡt));

        var (a1, err) = ResolveUnixAddr(unixgramˢ, addr1);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var c1, err) = DialUnix(unixgramˢ, a1, nil);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var c1ʗ1 = c1;
        defer(() => c1ʗ1.of(global::go.net_package.UnixConn.Ꮡconn).Close(), ref ᒐ);
        defer(Δos.Remove, addr1, ref ᒐ);
        c1.of(global::go.net_package.UnixConn.Ꮡconn).LocalAddr();
        c1.of(global::go.net_package.UnixConn.Ꮡconn).RemoteAddr();
        c1.of(global::go.net_package.UnixConn.Ꮡconn).SetDeadline(time.Now().Add(someTimeout));
        c1.of(global::go.net_package.UnixConn.Ꮡconn).SetReadDeadline(time.Now().Add(someTimeout));
        c1.of(global::go.net_package.UnixConn.Ꮡconn).SetWriteDeadline(time.Now().Add(someTimeout));
        c1.of(global::go.net_package.UnixConn.Ꮡconn).SetReadBuffer(2048);
        c1.of(global::go.net_package.UnixConn.Ꮡconn).SetWriteBuffer(2048);

        (var a2, err) = ResolveUnixAddr(unixgramˢ, addr2);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var c2, err) = DialUnix(unixgramˢ, a2, nil);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var c2ʗ1 = c2;
        defer(() => c2ʗ1.of(global::go.net_package.UnixConn.Ꮡconn).Close(), ref ᒐ);
        defer(Δos.Remove, addr2, ref ᒐ);
        c2.of(global::go.net_package.UnixConn.Ꮡconn).LocalAddr();
        c2.of(global::go.net_package.UnixConn.Ꮡconn).RemoteAddr();
        c2.of(global::go.net_package.UnixConn.Ꮡconn).SetDeadline(time.Now().Add(someTimeout));
        c2.of(global::go.net_package.UnixConn.Ꮡconn).SetReadDeadline(time.Now().Add(someTimeout));
        c2.of(global::go.net_package.UnixConn.Ꮡconn).SetWriteDeadline(time.Now().Add(someTimeout));
        c2.of(global::go.net_package.UnixConn.Ꮡconn).SetReadBuffer(2048);
        c2.of(global::go.net_package.UnixConn.Ꮡconn).SetWriteBuffer(2048);

        (var a3, err) = ResolveUnixAddr(unixgramˢ, addr3);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var c3, err) = ListenUnixgram(unixgramˢ, a3);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var c3ʗ1 = c3;
        defer(() => c3ʗ1.of(global::go.net_package.UnixConn.Ꮡconn).Close(), ref ᒐ);
        defer(Δos.Remove, addr3, ref ᒐ);
        c3.of(global::go.net_package.UnixConn.Ꮡconn).LocalAddr();
        c3.of(global::go.net_package.UnixConn.Ꮡconn).RemoteAddr();
        c3.of(global::go.net_package.UnixConn.Ꮡconn).SetDeadline(time.Now().Add(someTimeout));
        c3.of(global::go.net_package.UnixConn.Ꮡconn).SetReadDeadline(time.Now().Add(someTimeout));
        c3.of(global::go.net_package.UnixConn.Ꮡconn).SetWriteDeadline(time.Now().Add(someTimeout));
        c3.of(global::go.net_package.UnixConn.Ꮡconn).SetReadBuffer(2048);
        c3.of(global::go.net_package.UnixConn.Ꮡconn).SetWriteBuffer(2048);

        var wb = slice<byte>("UNIXCONN TEST"u8);
        var rb1 = new slice<byte>(128);
        var rb2 = new slice<byte>(128);
        var rb3 = new slice<byte>(128);
        {
            var (_, _, errΔ1) = c1.WriteMsgUnix(wb, default!, a2); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        {
            var (_, _, _, _, errΔ2) = c2.ReadMsgUnix(rb2, default!); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        {
            var (_, errΔ3) = c2.WriteToUnix(wb, a1); if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
        }
        {
            var (_, _, errΔ4) = c1.ReadFromUnix(rb1); if (errΔ4 != default!) {
                Ꮡt.Fatal(errΔ4);
            }
        }
        {
            var (_, errΔ5) = c3.WriteToUnix(wb, a1); if (errΔ5 != default!) {
                Ꮡt.Fatal(errΔ5);
            }
        }
        {
            var (_, _, errΔ6) = c1.ReadFromUnix(rb1); if (errΔ6 != default!) {
                Ꮡt.Fatal(errΔ6);
            }
        }
        {
            var (_, errΔ7) = c2.WriteToUnix(wb, a3); if (errΔ7 != default!) {
                Ꮡt.Fatal(errΔ7);
            }
        }
        {
            var (_, _, errΔ8) = c3.ReadFromUnix(rb3); if (errΔ8 != default!) {
                Ꮡt.Fatal(errΔ8);
            }
        }

        {
            var (f, errΔ9) = c1.of(global::go.net_package.UnixConn.Ꮡconn).File(); if (errΔ9 != default!){
                condFatalf(Ꮡt, fileNetˢ, "%v"u8, errΔ9);
            } else {
                f.Close();
            }
        }

        defer(() => {
            {
                var p = recover(); if (p != default!) {
                    Ꮡt.Fatalf("panicked: %v"u8, p);
                }
            }
        }, ref ᒐ);

        c1.WriteToUnix(wb, nil);
        c1.WriteMsgUnix(wb, default!, nil);
        c3.WriteToUnix(wb, nil);
        c3.WriteMsgUnix(wb, default!, nil);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end net_internal_test_package
