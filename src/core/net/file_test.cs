// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δos = os_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using Δsync = sync_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.net_package;
using time = time_package;

partial class net_internal_test_package {

// The full stack test cases for IPConn have been moved to the
// following:
//      golang.org/x/net/ipv4
//      golang.org/x/net/ipv6
//      golang.org/x/net/icmp

[GoType("dyn")] partial struct fileConnTestsᴛ1 {
    internal @string network;
}
internal static slice<fileConnTestsᴛ1> fileConnTests = new fileConnTestsᴛ1[]{
    new("tcp"u8),
    new("udp"u8),
    new("unix"u8),
    new("unixpacket"u8)
}.slice();

public static void TestFileConn(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8 || exprᴛ1 == "windows"u8 || exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8) {
            Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
        }

        foreach (var (_, tt) in fileConnTests) {
            if (!testableNetwork(tt.network)) {
                Ꮡt.Logf("skipping %s test"u8, tt.network);
                continue;
            }
            @string network = default!;
            @string address = default!;
            var exprᴛ2 = tt.network;
            if (exprᴛ2 == "udp"u8) {
                var c = newLocalPacketListener(new net_test_package.testing_TжTB(Ꮡt), tt.network);
                var cʗ1 = c;
                defer(() => cʗ1.Close(), ref ᒐ);
                network = c.LocalAddr().Network();
                address = c.LocalAddr().String();
            }
            else { /* default: */
                var handler = (ж<localServer> lsΔ1, global::go.net_package.Listener ln) => {
                    GoFrame ᒐ = default;
                    try {
                        var (c, errΔ3) = ln.Accept();
                        if (errΔ3 != default!) {
                            return;
                        }
                        var cʗ2 = c;
                        defer(() => cʗ2.Close(), ref ᒐ);
                        array<byte> b = new(1);
                        c.Read(b[..]);
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                };
                var ls = newLocalServer(new net_test_package.testing_TжTB(Ꮡt), tt.network);
                var lsʗ1 = ls;
                defer(() => lsʗ1.teardown(), ref ᒐ);
                {
                    var errΔ4 = ls.buildup(handler); if (errΔ4 != default!) {
                        Ꮡt.Fatal(errΔ4);
                    }
                }
                network = (~ls).Listener.Addr().Network();
                address = (~ls).Listener.Addr().String();
            }

            var (c1, err) = Dial(network, address);
            if (err != default!) {
                {
                    var perr = parseDialError(err); if (perr != default!) {
                        Ꮡt.Error(perr);
                    }
                }
                Ꮡt.Fatal(err);
            }
            var addr = c1.LocalAddr();
            ж<Δos.File> f = default!;
            switch (c1.type()) {
            case ж<global::go.net_package.TCPConn> c1Δ1: {
                (f, err) = c1Δ1.of(global::go.net_package.TCPConn.Ꮡconn).File();
                break;
            }
            case ж<global::go.net_package.UDPConn> c1Δ1: {
                (f, err) = c1Δ1.of(global::go.net_package.UDPConn.Ꮡconn).File();
                break;
            }
            case ж<global::go.net_package.UnixConn> c1Δ1: {
                (f, err) = c1Δ1.of(global::go.net_package.UnixConn.Ꮡconn).File();
                break;
            }}
            {
                var errΔ1 = c1.Close(); if (errΔ1 != default!) {
                    {
                        var perr = parseCloseError(errΔ1, false); if (perr != default!) {
                            Ꮡt.Error(perr);
                        }
                    }
                    Ꮡt.Error(errΔ1);
                }
            }
            if (err != default!) {
                {
                    var perr = parseCommonError(err); if (perr != default!) {
                        Ꮡt.Error(perr);
                    }
                }
                Ꮡt.Fatal(err);
            }
            (var c2, err) = FileConn(f);
            {
                var errΔ2 = f.Close(); if (errΔ2 != default!) {
                    Ꮡt.Error(errΔ2);
                }
            }
            if (err != default!) {
                {
                    var perr = parseCommonError(err); if (perr != default!) {
                        Ꮡt.Error(perr);
                    }
                }
                Ꮡt.Fatal(err);
            }
            var c2ʗ1 = c2;
            defer(() => c2ʗ1.Close(), ref ᒐ);
            {
                var (_, errΔ3) = c2.Write(slice<byte>("FILECONN TEST"u8)); if (errΔ3 != default!) {
                    {
                        var perr = parseWriteError(errΔ3); if (perr != default!) {
                            Ꮡt.Error(perr);
                        }
                    }
                    Ꮡt.Fatal(errΔ3);
                }
            }
            if (!reflect.DeepEqual(c2.LocalAddr(), addr)) {
                Ꮡt.Fatalf("got %#v; want %#v"u8, c2.LocalAddr(), addr);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static slice<fileConnTestsᴛ1> fileListenerTests = new fileConnTestsᴛ1[]{
    new("tcp"u8),
    new("unix"u8),
    new("unixpacket"u8)
}.slice();

public static partial void TestFileListener(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8 || exprᴛ1 == "windows"u8 || exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8) {
            Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
        }

        foreach (var (_, tt) in fileListenerTests) {
            if (!testableNetwork(tt.network)) {
                Ꮡt.Logf("skipping %s test"u8, tt.network);
                continue;
            }
            var ln1 = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tt.network);
            var exprᴛ2 = tt.network;
            if (exprᴛ2 == "unix"u8 || exprᴛ2 == "unixpacket"u8) {
                defer(Δos.Remove, ln1.Addr().String(), ref ᒐ);
            }

            var addr = ln1.Addr();
            ж<Δos.File> f = default!;
            error err = default!;
            switch (ln1.type()) {
            case ж<global::go.net_package.TCPListener> ln1Δ1: {
                (f, err) = ln1Δ1.File();
                break;
            }
            case ж<global::go.net_package.UnixListener> ln1Δ1: {
                (f, err) = ln1Δ1.File();
                break;
            }}
            var exprᴛ3 = tt.network;
            if (exprᴛ3 == "unix"u8 || exprᴛ3 == "unixpacket"u8) {
                var ln1ʗ1 = ln1;
                defer(() => ln1ʗ1.Close(), ref ᒐ); // UnixListener.Close calls syscall.Unlink internally
            }
            else { /* default: */
                {
                    var errΔ2 = ln1.Close(); if (errΔ2 != default!) {
                        Ꮡt.Error(errΔ2);
                    }
                }
            }

            if (err != default!) {
                {
                    var perr = parseCommonError(err); if (perr != default!) {
                        Ꮡt.Error(perr);
                    }
                }
                Ꮡt.Fatal(err);
            }
            (var ln2, err) = FileListener(f);
            {
                var errΔ3 = f.Close(); if (errΔ3 != default!) {
                    Ꮡt.Error(errΔ3);
                }
            }
            if (err != default!) {
                {
                    var perr = parseCommonError(err); if (perr != default!) {
                        Ꮡt.Error(perr);
                    }
                }
                Ꮡt.Fatal(err);
            }
            var ln2ʗ1 = ln2;
            defer(() => ln2ʗ1.Close(), ref ᒐ);
            ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
            Ꮡwg.Add(1);
            var ln2ʗ2 = ln2;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    var (cΔ1, errΔ4) = Dial(ln2ʗ2.Addr().Network(), ln2ʗ2.Addr().String());
                    if (errΔ4 != default!) {
                        {
                            var perr = parseDialError(errΔ4); if (perr != default!) {
                                Ꮡt.Error(perr);
                            }
                        }
                        Ꮡt.Error(errΔ4);
                        return;
                    }
                    cΔ1.Close();
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
            (var c, err) = ln2.Accept();
            if (err != default!) {
                {
                    var perr = parseAcceptError(err); if (perr != default!) {
                        Ꮡt.Error(perr);
                    }
                }
                Ꮡt.Fatal(err);
            }
            c.Close();
            Ꮡwg.Wait();
            if (!reflect.DeepEqual(ln2.Addr(), addr)) {
                Ꮡt.Fatalf("got %#v; want %#v"u8, ln2.Addr(), addr);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static slice<fileConnTestsᴛ1> filePacketConnTests = new fileConnTestsᴛ1[]{
    new("udp"u8),
    new("unixgram"u8)
}.slice();

public static void TestFilePacketConn(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8 || exprᴛ1 == "windows"u8 || exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8) {
            Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
        }

        foreach (var (_, tt) in filePacketConnTests) {
            if (!testableNetwork(tt.network)) {
                Ꮡt.Logf("skipping %s test"u8, tt.network);
                continue;
            }
            var c1 = newLocalPacketListener(new net_test_package.testing_TжTB(Ꮡt), tt.network);
            var exprᴛ2 = tt.network;
            if (exprᴛ2 == "unixgram"u8) {
                defer(Δos.Remove, c1.LocalAddr().String(), ref ᒐ);
            }

            var addr = c1.LocalAddr();
            ж<Δos.File> f = default!;
            error err = default!;
            switch (c1.type()) {
            case ж<global::go.net_package.UDPConn> c1Δ1: {
                (f, err) = c1Δ1.of(global::go.net_package.UDPConn.Ꮡconn).File();
                break;
            }
            case ж<global::go.net_package.UnixConn> c1Δ1: {
                (f, err) = c1Δ1.of(global::go.net_package.UnixConn.Ꮡconn).File();
                break;
            }}
            {
                var errΔ1 = c1.Close(); if (errΔ1 != default!) {
                    {
                        var perr = parseCloseError(errΔ1, false); if (perr != default!) {
                            Ꮡt.Error(perr);
                        }
                    }
                    Ꮡt.Error(errΔ1);
                }
            }
            if (err != default!) {
                {
                    var perr = parseCommonError(err); if (perr != default!) {
                        Ꮡt.Error(perr);
                    }
                }
                Ꮡt.Fatal(err);
            }
            (var c2, err) = FilePacketConn(f);
            {
                var errΔ2 = f.Close(); if (errΔ2 != default!) {
                    Ꮡt.Error(errΔ2);
                }
            }
            if (err != default!) {
                {
                    var perr = parseCommonError(err); if (perr != default!) {
                        Ꮡt.Error(perr);
                    }
                }
                Ꮡt.Fatal(err);
            }
            var c2ʗ1 = c2;
            defer(() => c2ʗ1.Close(), ref ᒐ);
            {
                var (_, errΔ3) = c2.WriteTo(slice<byte>("FILEPACKETCONN TEST"u8), addr); if (errΔ3 != default!) {
                    {
                        var perr = parseWriteError(errΔ3); if (perr != default!) {
                            Ꮡt.Error(perr);
                        }
                    }
                    Ꮡt.Fatal(errΔ3);
                }
            }
            if (!reflect.DeepEqual(c2.LocalAddr(), addr)) {
                Ꮡt.Fatalf("got %#v; want %#v"u8, c2.LocalAddr(), addr);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object tcpNotSupportedˢ = (@string)"tcp not supported"u8;

// Issue 24483.
public static partial void TestFileCloseRace(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8 || exprᴛ1 == "windows"u8 || exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8) {
            Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
        }

        if (!testableNetwork(tcpˢ)) {
            Ꮡt.Skip(tcpNotSupportedˢ);
        }
        var handler = (ж<localServer> lsΔ1, global::go.net_package.Listener ln) => {
            GoFrame ᒐ = default;
            try {
                var (c, err) = ln.Accept();
                if (err != default!) {
                    return;
                }
                var cʗ1 = c;
                defer(() => cʗ1.Close(), ref ᒐ);
                array<byte> b = new(1);
                c.Read(b[..]);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        };
        var ls = newLocalServer(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lsʗ1 = ls;
        defer(() => lsʗ1.teardown(), ref ᒐ);
        {
            var err = ls.buildup(handler); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        const nint tries = 100;
        for (nint i = 0; i < tries; i++) {
            var (c1, err) = Dial((~ls).Listener.Addr().Network(), (~ls).Listener.Addr().String());
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var tc = c1._<ж<global::go.net_package.TCPConn>>();
            ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
            Ꮡwg.Add(2);
            var tcʗ1 = tc;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    var (f, errΔ1) = tcʗ1.of(global::go.net_package.TCPConn.Ꮡconn).File();
                    if (errΔ1 == default!) {
                        f.Close();
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
            var c1ʗ1 = c1;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    c1ʗ1.Close();
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
            Ꮡwg.Wait();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end net_internal_test_package
