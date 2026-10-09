// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build windows
namespace go;

using windows = @internal.syscall.windows_package;
using Δos = os_package;
using reflect = reflect_package;
using testing = testing_package;
using @internal.syscall;
using fs = go.io.fs_package;
using go.io;
using static go.net_package;

partial class net_internal_test_package {

internal partial struct TestUnixConnLocalWindows_type /*dyn*/ {
    internal global::go.net_package.ΔAddr got, want;
}

public static void TestUnixConnLocalWindows(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (!windows.SupportUnixSocket()) {
            Ꮡt.Skip(unixTestˢ);
        }
        var handler = (ж<localServer> ls, global::go.net_package.Listener ln) => {
        };
        foreach (var (_, laddr) in new @string[]{""u8, testUnixAddr(new net_test_package.testing_TжTB(Ꮡt))}.slice()) {
            ref var laddrΔ1 = ref heap<@string>(out var ᏑladdrΔ1);
            laddrΔ1 = laddr;
            @string taddr = testUnixAddr(new net_test_package.testing_TжTB(Ꮡt));
            var (ta, err) = ResolveUnixAddr(unixˢ, taddr);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            (var ln, err) = ListenUnix(unixˢ, ta);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var ls = (Ꮡ(new streamListener(Listener: new global::go.net_package.UnixListenerжListener(ln)))).newLocalServer();
            var lsʗ1 = ls;
            defer(() => lsʗ1.teardown(), ref ᒐ);
            {
                var errΔ1 = ls.buildup(handler); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
            (var la, err) = ResolveUnixAddr(unixˢ, laddrΔ1);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            (var c, err) = DialUnix(unixˢ, la, ta);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var cʗ1 = c;
            var laʗ1 = la;
            defer(() => {
                GoFrame ᒐ = default;
                try {
                    cʗ1.of(global::go.net_package.UnixConn.Ꮡconn).Close();
                    if (laʗ1 != nil) {
                        defer(Δos.Remove, ᏑladdrΔ1.Value, ref ᒐ);
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }, ref ᒐ);
            {
                var (_, errΔ2) = c.of(global::go.net_package.UnixConn.Ꮡconn).Write(slice<byte>("UNIXCONN LOCAL AND REMOTE NAME TEST"u8)); if (errΔ2 != default!) {
                    Ꮡt.Fatal(errΔ2);
                }
            }
            if (laddrΔ1 == ""u8) {
                laddrΔ1 = "@"u8;
            }
            array<TestUnixConnLocalWindows_type> connAddrs = new TestUnixConnLocalWindows_type[]{
                new(ln.Addr(), new global::go.net_package.UnixAddrжΔAddr(ta)),
                new(c.of(global::go.net_package.UnixConn.Ꮡconn).LocalAddr(), new global::go.net_package.UnixAddrжΔAddr(Ꮡ(new UnixAddr(Name: laddrΔ1, Net: "unix"u8)))),
                new(c.of(global::go.net_package.UnixConn.Ꮡconn).RemoteAddr(), new global::go.net_package.UnixAddrжΔAddr(ta))
            }.array();
            foreach (var (_, ca) in connAddrs.ΔRangeSnapshot()) {
                if (!reflect.DeepEqual(ca.got, ca.want)) {
                    Ꮡt.Fatalf("got %#v, expected %#v"u8, ca.got, ca.want);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestModeSocket(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (!windows.SupportUnixSocket()) {
            Ꮡt.Skip(unixTestˢ);
        }
        @string addr = testUnixAddr(new net_test_package.testing_TжTB(Ꮡt));
        var (l, err) = Listen(unixˢ, addr);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var lʗ1 = l;
        defer(() => lʗ1.Close(), ref ᒐ);
        (var stat, err) = Δos.Stat(addr);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var mode = stat.Mode();
        if ((fs.FileMode)(mode & Δos.ModeSocket) == 0) {
            Ꮡt.Fatalf("%v should have ModeSocket"u8, mode);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end net_internal_test_package
