// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements API tests across platforms and should never have a build
// constraint.
namespace go;

using Δos = os_package;
using testing = testing_package;
using static go.net_package;
using time = time_package;

partial class net_internal_test_package {

// The full stack test cases for IPConn have been moved to the
// following:
//	golang.org/x/net/ipv4
//	golang.org/x/net/ipv6
//	golang.org/x/net/icmp
internal static (slice<byte>, Action) packetConnTestData(ж<testing.T> Ꮡt, @string network) {
    if (!testableNetwork(network)) {
        return (default!, () => {
            Ꮡt.Logf("skipping %s test"u8, network);
        });
    }
    return (slice<byte>("PACKETCONN TEST"u8), default!);
}

internal partial struct TestPacketConn_type /*dyn*/ {
    internal @string net;
    internal @string addr1;
    internal @string addr2;
}

public static void TestPacketConn(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        slice<TestPacketConn_type> packetConnTests = new TestPacketConn_type[]{
            new("udp"u8, "127.0.0.1:0"u8, "127.0.0.1:0"u8),
            new("unixgram"u8, testUnixAddr(new net_test_package.testing_TжTB(Ꮡt)), testUnixAddr(new net_test_package.testing_TжTB(Ꮡt)))
        }.slice();
        void closer(global::go.net_package.PacketConn c, @string net, @string addr1, @string addr2) {
            c.Close();
            var exprᴛ1 = net;
            if (exprᴛ1 == "unixgram"u8) {
                Δos.Remove(addr1);
                Δos.Remove(addr2);
            }

        }
        foreach (var (_, vᴛ1) in packetConnTests) {
            ref var tt = ref heap(new TestPacketConn_type(), out var Ꮡtt);
            tt = vᴛ1;

            var (wb, skipOrFatalFn) = packetConnTestData(Ꮡt, tt.net);
            if (skipOrFatalFn != default!) {
                skipOrFatalFn();
                continue;
            }
            var (c1, err) = ListenPacket(tt.net, tt.addr1);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var closerʗ1 = closer;
            defer(closerʗ1, c1, tt.net, tt.addr1, tt.addr2, ref ᒐ);
            c1.LocalAddr();
            (var c2, err) = ListenPacket(tt.net, tt.addr2);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var closerʗ2 = closer;
            defer(closerʗ2, c2, tt.net, tt.addr1, tt.addr2, ref ᒐ);
            c2.LocalAddr();
            var rb2 = new slice<byte>(128);
            {
                var (_, errΔ1) = c1.WriteTo(wb, c2.LocalAddr()); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
            {
                var (_, _, errΔ2) = c2.ReadFrom(rb2); if (errΔ2 != default!) {
                    Ꮡt.Fatal(errΔ2);
                }
            }
            {
                var (_, errΔ3) = c2.WriteTo(wb, c1.LocalAddr()); if (errΔ3 != default!) {
                    Ꮡt.Fatal(errΔ3);
                }
            }
            var rb1 = new slice<byte>(128);
            {
                var (_, _, errΔ4) = c1.ReadFrom(rb1); if (errΔ4 != default!) {
                    Ꮡt.Fatal(errΔ4);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestConnAndPacketConn_type /*dyn*/ {
    internal @string net;
    internal @string addr1;
    internal @string addr2;
}

public static void TestConnAndPacketConn(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        slice<TestConnAndPacketConn_type> packetConnTests = new TestConnAndPacketConn_type[]{
            new("udp"u8, "127.0.0.1:0"u8, "127.0.0.1:0"u8),
            new("unixgram"u8, testUnixAddr(new net_test_package.testing_TжTB(Ꮡt)), testUnixAddr(new net_test_package.testing_TжTB(Ꮡt)))
        }.slice();
        void closer(global::go.net_package.PacketConn c, @string net, @string addr1, @string addr2) {
            c.Close();
            var exprᴛ1 = net;
            if (exprᴛ1 == "unixgram"u8) {
                Δos.Remove(addr1);
                Δos.Remove(addr2);
            }

        }
        foreach (var (_, vᴛ1) in packetConnTests) {
            ref var tt = ref heap(new TestConnAndPacketConn_type(), out var Ꮡtt);
            tt = vᴛ1;

            slice<byte> wb = default!;
            (wb, var skipOrFatalFn) = packetConnTestData(Ꮡt, tt.net);
            if (skipOrFatalFn != default!) {
                skipOrFatalFn();
                continue;
            }
            var (c1, err) = ListenPacket(tt.net, tt.addr1);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var closerʗ1 = closer;
            defer(closerʗ1, c1, tt.net, tt.addr1, tt.addr2, ref ᒐ);
            c1.LocalAddr();
            (var c2, err) = Dial(tt.net, c1.LocalAddr().String());
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var c2ʗ1 = c2;
            defer(() => c2ʗ1.Close(), ref ᒐ);
            c2.LocalAddr();
            c2.RemoteAddr();
            {
                var (_, errΔ1) = c2.Write(wb); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
            var rb1 = new slice<byte>(128);
            {
                var (_, _, errΔ2) = c1.ReadFrom(rb1); if (errΔ2 != default!) {
                    Ꮡt.Fatal(errΔ2);
                }
            }
            global::go.net_package.ΔAddr dst = default!;
            var exprᴛ2 = tt.net;
            if (exprᴛ2 == "unixgram"u8) {
                continue;
            }
            else { /* default: */
                dst = c2.LocalAddr();
            }

            {
                var (_, errΔ3) = c1.WriteTo(wb, dst); if (errΔ3 != default!) {
                    Ꮡt.Fatal(errΔ3);
                }
            }
            var rb2 = new slice<byte>(128);
            {
                var (_, errΔ4) = c2.Read(rb2); if (errΔ4 != default!) {
                    Ꮡt.Fatal(errΔ4);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end net_internal_test_package
