// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !plan9
namespace go;

using bytes = bytes_package;
using testenv = @internal.testenv_package;
using Δos = os_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using syscall = syscall_package;
using testing = testing_package;
using time = time_package;
using @internal;
using fs = go.io.fs_package;
using static go.net_package;

partial class net_internal_test_package {

public static partial void TestReadUnixgramWithUnnamedSocket(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (!testableNetwork(unixgramˢ)) {
            t.Skip(unixgramTestˢ);
        }
        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8: {
            t.Skipf("skipping: syscall.Socket not implemented on %s", Δruntime.GOOS);
            break;
        }}
        if (Δruntime.GOOS == "openbsd"u8) {
            testenv.SkipFlaky(new net_test_package.testing_TжTB(t), 15157);
        }

        ref var addr = ref heap<@string>(testUnixAddr(new net_test_package.testing_TжTB(t)), out var Ꮡaddr);
        var (la, err) = ResolveUnixAddr(unixgramˢ, addr);
        if (err != nil) {
            t.Fatal(err);
        }
        (var c, err) = ListenUnixgram(unixgramˢ, la);
        if (err != nil) {
            t.Fatal(err);
        }
        var addrʗ1 = addr;
        var cʗ1 = c;
        defer(() => {
            cʗ1.of(global::go.net_package.UnixConn.Ꮡconn).Close();
            Δos.Remove(addrʗ1);
        }, ref ᒐ);

        var off = new channel<bool>(0);
        ref var data = ref heap<array<byte>>(new byte[]{1, 2, 3, 4, 5}.array(), out var Ꮡdata);
        var addrʗ2 = addr;
        var dataʗ1 = data;
        var offʗ1 = off;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                var offʗ2 = offʗ1;
                defer(() => {
                    offʗ2.ᐸꟷ(true);
                }, ref ᒐ);
                var (s, errΔ1) = syscall.Socket(syscall.AF_UNIX, syscall.SOCK_DGRAM, 0);
                if (errΔ1 != nil) {
                    t.Error(errΔ1);
                    return;
                }
                defer(syscall.Close, s, ref ᒐ);
                var rsa = Ꮡ(new syscall.SockaddrUnix(Name: addrʗ2));
                var errΔ2 = syscall.Sendto(s, dataʗ1[..], 0, new syscall.SockaddrUnixжΔSockaddr(rsa)); if (errΔ2 != nil) {
                    t.Error(errΔ2);
                    return;
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });

        ᐸꟷ(off);
        var b = new slice<byte>(64);
        c.of(global::go.net_package.UnixConn.Ꮡconn).SetReadDeadline(time.Now().Add(100 * time.Millisecond));
        (var n, var from, err) = c.ReadFrom(b);
        if (err != nil) {
            t.Fatal(err);
        }
        if (from != nil) {
            t.Fatalf("unexpected peer address: %v", from);
        }
        if (!bytes.Equal(b.slice(0, n), data[..])) {
            t.Fatalf("got %v; want %v", b.slice(0, n), data[..]);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestUnixgramZeroBytePayload(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (!testableNetwork(unixgramˢ)) {
            t.Skip(unixgramTestˢ);
        }

        var c1 = newLocalPacketListener(new net_test_package.testing_TжTB(t), unixgramˢ);
        defer(Δos.Remove, c1.LocalAddr().String(), ref ᒐ);
        var c1ʗ1 = c1;
        defer(() => c1ʗ1.Close(), ref ᒐ);

        var (c2, err) = Dial(unixgramˢ, c1.LocalAddr().String());
        if (err != nil) {
            t.Fatal(err);
        }
        defer(Δos.Remove, c2.LocalAddr().String(), ref ᒐ);
        var c2ʗ1 = c2;
        defer(() => c2ʗ1.Close(), ref ᒐ);

        foreach (var (_, genericRead) in range(slice<bool>([false, true]))) {
            var (n, errΔ1) = c2.Write(default);
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
            if (n != 0) {
                t.Errorf("got %d; want 0", n);
            }
            c1.SetReadDeadline(time.Now().Add(100 * time.Millisecond));
            array<byte> b = new(1);
            global::go.net_package.ΔAddr peer = default;
            if (genericRead) {
                (_, errΔ1) = c1._<Conn>().Read(b[..]);
            } else {
                (_, peer, errΔ1) = c1.ReadFrom(b[..]);
            }
            switch (errΔ1) {
            case var ᴛ when AreEqual(ᴛ, default): { // ReadFrom succeeds
                if (peer != nil) { // peer is connected-mode
                    t.Fatalf("unexpected peer address: %v", peer);
                }
                break;
            }
            default: { // Read may timeout, it depends on the platform
                if (!isDeadlineExceeded(errΔ1)) {
                    t.Fatal(errΔ1);
                }
                break;
            }}
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestUnixgramZeroByteBuffer(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (!testableNetwork(unixgramˢ)) {
            t.Skip(unixgramTestˢ);
        }
        // issue 4352: Recvfrom failed with "address family not
        // supported by protocol family" if zero-length buffer provided

        var c1 = newLocalPacketListener(new net_test_package.testing_TжTB(t), unixgramˢ);
        defer(Δos.Remove, c1.LocalAddr().String(), ref ᒐ);
        var c1ʗ1 = c1;
        defer(() => c1ʗ1.Close(), ref ᒐ);

        var (c2, err) = Dial(unixgramˢ, c1.LocalAddr().String());
        if (err != nil) {
            t.Fatal(err);
        }
        defer(Δos.Remove, c2.LocalAddr().String(), ref ᒐ);
        var c2ʗ1 = c2;
        defer(() => c2ʗ1.Close(), ref ᒐ);

        var b = slice<byte>("UNIXGRAM ZERO BYTE BUFFER TEST"u8);
        foreach (var (_, genericRead) in range(slice<bool>([false, true]))) {
            var (n, errΔ1) = c2.Write(b);
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
            if (n != len(b)) {
                t.Errorf("got %d; want %d", n, len(b));
            }
            c1.SetReadDeadline(time.Now().Add(100 * time.Millisecond));
            global::go.net_package.ΔAddr peer = default;
            if (genericRead) {
                (_, errΔ1) = c1._<Conn>().Read(default);
            } else {
                (_, peer, errΔ1) = c1.ReadFrom(default);
            }
            switch (errΔ1) {
            case var ᴛ when AreEqual(ᴛ, default): { // ReadFrom succeeds
                if (peer != nil) { // peer is connected-mode
                    t.Fatalf("unexpected peer address: %v", peer);
                }
                break;
            }
            default: { // Read may timeout, it depends on the platform
                if (!isDeadlineExceeded(errΔ1)) {
                    t.Fatal(errΔ1);
                }
                break;
            }}
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestUnixgramWrite(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (!testableNetwork(unixgramˢ)) {
            t.Skip(unixgramTestˢ);
        }

        @string addr = testUnixAddr(new net_test_package.testing_TжTB(t));
        var (laddr, err) = ResolveUnixAddr(unixgramˢ, addr);
        if (err != nil) {
            t.Fatal(err);
        }
        (var c, err) = ListenPacket(unixgramˢ, addr);
        if (err != nil) {
            t.Fatal(err);
        }
        defer(Δos.Remove, addr, ref ᒐ);
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);

        testUnixgramWriteConn(t, laddr);
        testUnixgramWritePacketConn(t, laddr);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testUnixgramWriteConn(ж<testing.T> t, ж<global::go.net_package.UnixAddr> raddr) {
    GoFrame ᒐ = default;
    try {
        var (c, err) = Dial(unixgramˢ, raddr.String());
        if (err != nil) {
            t.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);

        var b = slice<byte>("CONNECTED-MODE SOCKET"u8);
        var (_, errΔ1) = c._<ж<global::go.net_package.UnixConn>>().WriteToUnix(b, raddr); if (errΔ1 == nil) {
            t.Fatal(shouldFailˢ);
        } else if (!AreEqual(errΔ1._<ж<global::go.net_package.OpError>>().Err, ErrWriteToConnected)) {
            t.Fatalf("should fail as ErrWriteToConnected: %v", errΔ1);
        }
        (_, err) = c._<ж<global::go.net_package.UnixConn>>().WriteTo(b, new global::go.net_package.UnixAddrжΔAddr(raddr)); if (err == nil) {
            t.Fatal(shouldFailˢ);
        } else if (!AreEqual(err._<ж<global::go.net_package.OpError>>().Err, ErrWriteToConnected)) {
            t.Fatalf("should fail as ErrWriteToConnected: %v", err);
        }
        (_, _, err) = c._<ж<global::go.net_package.UnixConn>>().WriteMsgUnix(b, default, raddr); if (err == nil) {
            t.Fatal(shouldFailˢ);
        } else if (!AreEqual(err._<ж<global::go.net_package.OpError>>().Err, ErrWriteToConnected)) {
            t.Fatalf("should fail as ErrWriteToConnected: %v", err);
        }
        var (_, errΔ2) = c.Write(b); if (errΔ2 != nil) {
            t.Fatal(errΔ2);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testUnixgramWritePacketConn(ж<testing.T> t, ж<global::go.net_package.UnixAddr> raddr) {
    GoFrame ᒐ = default;
    try {
        @string addr = testUnixAddr(new net_test_package.testing_TжTB(t));
        var (c, err) = ListenPacket(unixgramˢ, addr);
        if (err != nil) {
            t.Fatal(err);
        }
        defer(Δos.Remove, addr, ref ᒐ);
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);

        var b = slice<byte>("UNCONNECTED-MODE SOCKET"u8);
        var (_, errΔ1) = c._<ж<global::go.net_package.UnixConn>>().WriteToUnix(b, raddr); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        var (_, errΔ2) = c.WriteTo(b, new global::go.net_package.UnixAddrжΔAddr(raddr)); if (errΔ2 != nil) {
            t.Fatal(errΔ2);
        }
        var (_, _, errΔ3) = c._<ж<global::go.net_package.UnixConn>>().WriteMsgUnix(b, default, raddr); if (errΔ3 != nil) {
            t.Fatal(errΔ3);
        }
        var (_, errΔ4) = c._<ж<global::go.net_package.UnixConn>>().of(global::go.net_package.UnixConn.Ꮡconn).Write(b); if (errΔ4 == nil) {
            t.Fatal(shouldFailˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestUnixConnLocalAndRemoteNames_type /*dyn*/ {
    internal global::go.net_package.ΔAddr got, want;
}

public static void TestUnixConnLocalAndRemoteNames(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (!testableNetwork(unixˢ)) {
            t.Skip(unixTestˢ);
        }

        var handler = (ж<localServer> ls, global::go.net_package.Listener ln) => {
        };
        foreach (var (_, vᴛ1) in range(slice<@string>(["", testUnixAddr(new net_test_package.testing_TжTB(t))]))) {
            ref var laddr = ref heap(new @string(), out var Ꮡladdr);
            laddr = vᴛ1;

            @string taddr = testUnixAddr(new net_test_package.testing_TжTB(t));
            var (ta, err) = ResolveUnixAddr(unixˢ, taddr);
            if (err != nil) {
                t.Fatal(err);
            }
            (var ln, err) = ListenUnix(unixˢ, ta);
            if (err != nil) {
                t.Fatal(err);
            }
            var ls = (Ꮡ(new streamListener(Listener: new global::go.net_package.UnixListenerжListener(ln)))).newLocalServer();
            var lsʗ1 = ls;
            defer(() => lsʗ1.teardown(), ref ᒐ);
            var errΔ1 = ls.buildup(handler); if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }

            (var la, err) = ResolveUnixAddr(unixˢ, laddr);
            if (err != nil) {
                t.Fatal(err);
            }
            (var c, err) = DialUnix(unixˢ, la, ta);
            if (err != nil) {
                t.Fatal(err);
            }
            var cʗ1 = c;
            var laʗ1 = la;
            var laddrʗ1 = laddr;
            defer(() => {
                GoFrame ᒐ = default;
                try {
                    cʗ1.of(global::go.net_package.UnixConn.Ꮡconn).Close();
                    if (laʗ1 != nil) {
                        defer(Δos.Remove, laddrʗ1, ref ᒐ);
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }, ref ᒐ);
            var (_, errΔ2) = c.of(global::go.net_package.UnixConn.Ꮡconn).Write(slice<byte>("UNIXCONN LOCAL AND REMOTE NAME TEST"u8)); if (errΔ2 != nil) {
                t.Fatal(errΔ2);
            }

            var exprᴛ1 = Δruntime.GOOS;
            switch (ᐧ) {
            case {} when exprᴛ1 == "android"u8 || exprᴛ1 == "linux"u8 || exprᴛ1 == "windows"u8: {
                if (laddr == ""u8) {
                    laddr = "@"; // autobind feature
                }
                break;
            }}
            array<TestUnixConnLocalAndRemoteNames_type> connAddrs = new TestUnixConnLocalAndRemoteNames_type[]{
                new(ln.Addr(), new global::go.net_package.UnixAddrжΔAddr(ta)),
                new(c.of(global::go.net_package.UnixConn.Ꮡconn).LocalAddr(), new global::go.net_package.UnixAddrжΔAddr(Ꮡ(new UnixAddr(Name: laddr, Net: "unix")))),
                new(c.of(global::go.net_package.UnixConn.Ꮡconn).RemoteAddr(), new global::go.net_package.UnixAddrжΔAddr(ta))
            }.array();
            foreach (var (_, ca) in range(connAddrs.ΔRangeSnapshot())) {
                if (!reflect.DeepEqual(ca.got, ca.want)) {
                    t.Fatalf("got %#v, expected %#v", ca.got, ca.want);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestUnixgramConnLocalAndRemoteNames_type /*dyn*/ {
    internal global::go.net_package.ΔAddr got, want;
}

public static void TestUnixgramConnLocalAndRemoteNames(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (!testableNetwork(unixgramˢ)) {
            t.Skip(unixgramTestˢ);
        }

        foreach (var (_, vᴛ1) in range(slice<@string>(["", testUnixAddr(new net_test_package.testing_TжTB(t))]))) {
            ref var laddr = ref heap(new @string(), out var Ꮡladdr);
            laddr = vᴛ1;

            @string taddr = testUnixAddr(new net_test_package.testing_TжTB(t));
            var (ta, err) = ResolveUnixAddr(unixgramˢ, taddr);
            if (err != nil) {
                t.Fatal(err);
            }
            (var c1, err) = ListenUnixgram(unixgramˢ, ta);
            if (err != nil) {
                t.Fatal(err);
            }
            var c1ʗ1 = c1;
            defer(() => {
                c1ʗ1.of(global::go.net_package.UnixConn.Ꮡconn).Close();
                Δos.Remove(taddr);
            }, ref ᒐ);

            ref var la = ref heap<ж<global::go.net_package.UnixAddr>>(out var Ꮡla);
            if (laddr != ""u8) {
                (la, err) = ResolveUnixAddr(unixgramˢ, laddr); if (err != nil) {
                    t.Fatal(err);
                }
            }
            (var c2, err) = DialUnix(unixgramˢ, la, ta);
            if (err != nil) {
                t.Fatal(err);
            }
            var c2ʗ1 = c2;
            var laddrʗ1 = laddr;
            defer(() => {
                GoFrame ᒐ = default;
                try {
                    c2ʗ1.of(global::go.net_package.UnixConn.Ꮡconn).Close();
                    if (Ꮡla.ValueSlot != nil) {
                        defer(Δos.Remove, laddrʗ1, ref ᒐ);
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }, ref ᒐ);

            var exprᴛ1 = Δruntime.GOOS;
            switch (ᐧ) {
            case {} when exprᴛ1 == "android"u8 || exprᴛ1 == "linux"u8: {
                if (laddr == ""u8) {
                    laddr = "@"; // autobind feature
                }
                break;
            }}

            array<TestUnixgramConnLocalAndRemoteNames_type> connAddrs = new TestUnixgramConnLocalAndRemoteNames_type[]{
                new(c1.of(global::go.net_package.UnixConn.Ꮡconn).LocalAddr(), new global::go.net_package.UnixAddrжΔAddr(ta)),
                new(c1.of(global::go.net_package.UnixConn.Ꮡconn).RemoteAddr(), default),
                new(c2.of(global::go.net_package.UnixConn.Ꮡconn).LocalAddr(), new global::go.net_package.UnixAddrжΔAddr(Ꮡ(new UnixAddr(Name: laddr, Net: "unixgram")))),
                new(c2.of(global::go.net_package.UnixConn.Ꮡconn).RemoteAddr(), new global::go.net_package.UnixAddrжΔAddr(ta))
            }.array();
            foreach (var (_, ca) in range(connAddrs.ΔRangeSnapshot())) {
                if (!reflect.DeepEqual(ca.got, ca.want)) {
                    t.Fatalf("got %#v; want %#v", ca.got, ca.want);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string listenˢ2 => ref listenˢ2ᵇ;
private static readonly @string listenˢ2ᵇ = "Listen"u8;
internal static ref readonly @string afterListenˢ => ref afterListenˢᵇ;
private static readonly @string afterListenˢᵇ = "after Listen"u8;
internal static ref readonly @string afterListenerCloseˢ => ref afterListenerCloseˢᵇ;
private static readonly @string afterListenerCloseˢᵇ = "after Listener close"u8;
internal static ref readonly @string fileListenerˢ => ref fileListenerˢᵇ;
private static readonly @string fileListenerˢᵇ = "FileListener"u8;
internal static ref readonly @string afterFileListenerˢ => ref afterFileListenerˢᵇ;
private static readonly @string afterFileListenerˢᵇ = "after FileListener"u8;
internal static ref readonly @string afterFileCloseˢ => ref afterFileCloseˢᵇ;
private static readonly @string afterFileCloseˢᵇ = "after File close"u8;
internal static ref readonly @string afterFileListenerCloseˢ => ref afterFileListenerCloseˢᵇ;
private static readonly @string afterFileListenerCloseˢᵇ = "after FileListener close"u8;
internal static ref readonly @string secondCloseˢ => ref secondCloseˢᵇ;
private static readonly @string secondCloseˢᵇ = "SecondClose"u8;
internal static ref readonly @string afterWritingTempFileˢ => ref afterWritingTempFileˢᵇ;
private static readonly @string afterWritingTempFileˢᵇ = "after writing temp file"u8;
internal static ref readonly @string afterSecondListenerCloseˢ => ref afterSecondListenerCloseˢᵇ;
private static readonly @string afterSecondListenerCloseˢᵇ = "after second Listener close"u8;
internal static ref readonly @string listenSetUnlinkOnCloseˢ => ref listenSetUnlinkOnCloseˢᵇ;
private static readonly @string listenSetUnlinkOnCloseˢᵇ = "Listen/SetUnlinkOnClose(true)"u8;
internal static ref readonly @string listenSetUnlinkOnCloseˢ2 => ref listenSetUnlinkOnCloseˢ2ᵇ;
private static readonly @string listenSetUnlinkOnCloseˢ2ᵇ = "Listen/SetUnlinkOnClose(false)"u8;
internal static ref readonly @string fileListenerˢ2 => ref fileListenerˢ2ᵇ;
private static readonly @string fileListenerˢ2ᵇ = "FileListener/SetUnlinkOnClose(true)"u8;
internal static ref readonly @string fileListenerˢ3 => ref fileListenerˢ3ᵇ;
private static readonly @string fileListenerˢ3ᵇ = "FileListener/SetUnlinkOnClose(false)"u8;

public static void TestUnixUnlink(ж<testing.T> t) {
    if (!testableNetwork(unixˢ)) {
        t.Skip(unixTestˢ);
    }
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8: {
        t.Skipf("skipping: %s does not support Unlink", Δruntime.GOOS);
        break;
    }}

    @string name = testUnixAddr(new net_test_package.testing_TжTB(t));

    ж<global::go.net_package.UnixListener> listen(ж<testing.T> tΔ1) {
        var (l, err) = Listen(unixˢ, name);
        if (err != nil) {
            tΔ1.Fatal(err);
        }
        return l._<ж<global::go.net_package.UnixListener>>();
    }
    (ж<Δos.File>, global::go.net_package.Listener) fileListener(ж<testing.T> tΔ2, ж<global::go.net_package.UnixListener> l) {
        var (f, err) = l.File();
        if (err != nil) {
            tΔ2.Fatal(err);
        }
        (var ln, err) = FileListener(f);
        if (err != nil) {
            tΔ2.Fatal(err);
        }
        return (f, ln);
    }
    void checkExists(ж<testing.T> tΔ3, @string desc) {
        var (_, err) = Δos.Stat(name); if (err != nil) {
            tΔ3.Fatalf("unix socket does not exist %s: %v", desc, err);
        }
    }
    void checkNotExists(ж<testing.T> tΔ4, @string desc) {
        var (_, err) = Δos.Stat(name); if (err == nil) {
            tΔ4.Fatalf("unix socket does exist %s: %v", desc, err);
        }
    }

    // Listener should remove on close.
    var checkExistsʗ1 = checkExists;
    var checkNotExistsʗ1 = checkNotExists;
    var listenʗ1 = listen;
    t.Run(listenˢ2, (ж<testing.T> tΔ5) => {
        var l = listenʗ1(tΔ5);
        checkExistsʗ1(tΔ5, afterListenˢ);
        l.Close();
        checkNotExistsʗ1(tΔ5, afterListenerCloseˢ);
    });

    // FileListener should not.
    var checkExistsʗ2 = checkExists;
    var checkNotExistsʗ2 = checkNotExists;
    var fileListenerʗ1 = fileListener;
    var listenʗ2 = listen;
    t.Run(fileListenerˢ, (ж<testing.T> tΔ6) => {
        var l = listenʗ2(tΔ6);
        var (f, l1) = fileListenerʗ1(tΔ6, l);
        checkExistsʗ2(tΔ6, afterFileListenerˢ);
        f.Close();
        checkExistsʗ2(tΔ6, afterFileCloseˢ);
        l1.Close();
        checkExistsʗ2(tΔ6, afterFileListenerCloseˢ);
        l.Close();
        checkNotExistsʗ2(tΔ6, afterListenerCloseˢ);
    });

    // Only first call to l.Close should remove.
    var checkExistsʗ3 = checkExists;
    var checkNotExistsʗ3 = checkNotExists;
    var listenʗ3 = listen;
    t.Run(secondCloseˢ, (ж<testing.T> tΔ7) => {
        var l = listenʗ3(tΔ7);
        checkExistsʗ3(tΔ7, afterListenˢ);
        l.Close();
        checkNotExistsʗ3(tΔ7, afterListenerCloseˢ);
        var err = Δos.WriteFile(name, slice<byte>("hello world"u8), 438); if (err != nil) {
            tΔ7.Fatalf("cannot recreate socket file: %v", err);
        }
        checkExistsʗ3(tΔ7, afterWritingTempFileˢ);
        l.Close();
        checkExistsʗ3(tΔ7, afterSecondListenerCloseˢ);
        Δos.Remove(name);
    });

    // SetUnlinkOnClose should do what it says.

    var checkExistsʗ4 = checkExists;
    var checkNotExistsʗ4 = checkNotExists;
    var listenʗ4 = listen;
    t.Run(listenSetUnlinkOnCloseˢ, (ж<testing.T> tΔ8) => {
        var l = listenʗ4(tΔ8);
        checkExistsʗ4(tΔ8, afterListenˢ);
        l.SetUnlinkOnClose(true);
        l.Close();
        checkNotExistsʗ4(tΔ8, afterListenerCloseˢ);
    });

    var checkExistsʗ5 = checkExists;
    var listenʗ5 = listen;
    t.Run(listenSetUnlinkOnCloseˢ2, (ж<testing.T> tΔ9) => {
        var l = listenʗ5(tΔ9);
        checkExistsʗ5(tΔ9, afterListenˢ);
        l.SetUnlinkOnClose(false);
        l.Close();
        checkExistsʗ5(tΔ9, afterListenerCloseˢ);
        Δos.Remove(name);
    });

    var checkExistsʗ6 = checkExists;
    var checkNotExistsʗ5 = checkNotExists;
    var fileListenerʗ2 = fileListener;
    var listenʗ6 = listen;
    t.Run(fileListenerˢ2, (ж<testing.T> tΔ10) => {
        var l = listenʗ6(tΔ10);
        var (f, l1) = fileListenerʗ2(tΔ10, l);
        checkExistsʗ6(tΔ10, afterFileListenerˢ);
        l1._<ж<global::go.net_package.UnixListener>>().SetUnlinkOnClose(true);
        f.Close();
        checkExistsʗ6(tΔ10, afterFileCloseˢ);
        l1.Close();
        checkNotExistsʗ5(tΔ10, afterFileListenerCloseˢ);
        l.Close();
    });

    var checkExistsʗ7 = checkExists;
    var fileListenerʗ3 = fileListener;
    var listenʗ7 = listen;
    t.Run(fileListenerˢ3, (ж<testing.T> tΔ11) => {
        var l = listenʗ7(tΔ11);
        var (f, l1) = fileListenerʗ3(tΔ11, l);
        checkExistsʗ7(tΔ11, afterFileListenerˢ);
        l1._<ж<global::go.net_package.UnixListener>>().SetUnlinkOnClose(false);
        f.Close();
        checkExistsʗ7(tΔ11, afterFileCloseˢ);
        l1.Close();
        checkExistsʗ7(tΔ11, afterFileListenerCloseˢ);
        l.Close();
    });
}

} // end net_internal_test_package
