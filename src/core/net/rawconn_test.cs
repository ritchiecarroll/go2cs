// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using Δruntime = runtime_package;
using testing = testing_package;
using time = time_package;
using static go.net_package;
using syscall = syscall_package;

partial class net_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tcpˢ2 = "TCP"u8;
internal static readonly object writeShouldReturnAnErrorˢ = (@string)"Write should return an error"u8;
internal static readonly object writeShouldnTCallOpˢ = (@string)"Write shouldn't call op"u8;
internal static readonly object readShouldReturnAnErrorˢ = (@string)"Read should return an error"u8;
internal static readonly object readShouldnTCallOpˢ = (@string)"Read shouldn't call op"u8;
internal static readonly @string deadlineˢ = "Deadline"u8;
internal static readonly object writeShouldFailˢ = (@string)"Write should fail"u8;
internal static readonly object readShouldFailˢ = (@string)"Read should fail"u8;

public static void TestRawConnReadWrite(ж<testing.T> Ꮡt) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "plan9"u8 || exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8) {
        Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
    }

    Ꮡt.Run(tcpˢ2, (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            var handler = (ж<localServer> lsΔ1, global::go.net_package.Listener ln) => {
                GoFrame ᒐ = default;
                try {
                    var (cΔ1, errΔ1) = ln.Accept();
                    if (errΔ1 != default!) {
                        tΔ1.Error(errΔ1);
                        return;
                    }
                    var cʗ1 = cΔ1;
                    defer(() => cʗ1.Close(), ref ᒐ);
                    (var ccΔ1, errΔ1) = ln._<ж<global::go.net_package.TCPListener>>().SyscallConn();
                    if (errΔ1 != default!) {
                        tΔ1.Fatal(errΔ1);
                    }
                    var called = false;
                    var op = (uintptr _Δp0) => {
                        called = true;
                        return true;
                    };
                    errΔ1 = ccΔ1.Write(op);
                    if (errΔ1 == default!) {
                        tΔ1.Error(writeShouldReturnAnErrorˢ);
                    }
                    if (called) {
                        tΔ1.Error(writeShouldnTCallOpˢ);
                    }
                    called = false;
                    errΔ1 = ccΔ1.Read(op);
                    if (errΔ1 == default!) {
                        tΔ1.Error(readShouldReturnAnErrorˢ);
                    }
                    if (called) {
                        tΔ1.Error(readShouldnTCallOpˢ);
                    }
                    array<byte> bΔ1 = new(32);
                    (var nΔ1, errΔ1) = cΔ1.Read(bΔ1[..]);
                    if (errΔ1 != default!) {
                        tΔ1.Error(errΔ1);
                        return;
                    }
                    {
                        var (_, errΔ2) = cΔ1.Write(bΔ1.slice(0, nΔ1)); if (errΔ2 != default!) {
                            tΔ1.Error(errΔ2);
                            return;
                        }
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            };
            var ls = newLocalServer(new net_test_package.testing_TжTB(tΔ1), tcpˢ);
            var lsʗ1 = ls;
            defer(() => lsʗ1.teardown(), ref ᒐ);
            {
                var errΔ3 = ls.buildup(handler); if (errΔ3 != default!) {
                    tΔ1.Fatal(errΔ3);
                }
            }
            var (c, err) = Dial((~ls).Listener.Addr().Network(), (~ls).Listener.Addr().String());
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            var cʗ2 = c;
            defer(() => cʗ2.Close(), ref ᒐ);
            (var cc, err) = c._<ж<global::go.net_package.TCPConn>>().SyscallConn();
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            var data = slice<byte>("HELLO-R-U-THERE"u8);
            {
                var errΔ1 = writeRawConn(cc, data); if (errΔ1 != default!) {
                    tΔ1.Fatal(errΔ1);
                }
            }
            array<byte> b = new(32);
            (var n, err) = readRawConn(cc, b[..]);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            if (bytes.Compare(b.slice(0, n), data) != 0) {
                tΔ1.Fatalf("got %q; want %q"u8, b.slice(0, n), data);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    Ꮡt.Run(deadlineˢ, (ж<testing.T> tΔ2) => {
        GoFrame ᒐ = default;
        try {
            var exprᴛ2 = Δruntime.GOOS;
            if (exprᴛ2 == "windows"u8) {
                tΔ2.Skipf("not supported on %s"u8, Δruntime.GOOS);
            }

            var ln = newLocalListener(new net_test_package.testing_TжTB(tΔ2), tcpˢ);
            var lnʗ1 = ln;
            defer(() => lnʗ1.Close(), ref ᒐ);
            var (c, err) = Dial(ln.Addr().Network(), ln.Addr().String());
            if (err != default!) {
                tΔ2.Fatal(err);
            }
            var cʗ3 = c;
            defer(() => cʗ3.Close(), ref ᒐ);
            (var cc, err) = c._<ж<global::go.net_package.TCPConn>>().SyscallConn();
            if (err != default!) {
                tΔ2.Fatal(err);
            }
            array<byte> b = new(1);
            c.SetDeadline(noDeadline);
            {
                var errΔ1 = c.SetDeadline(time.Now().Add(-1)); if (errΔ1 != default!) {
                    tΔ2.Fatal(errΔ1);
                }
            }
            {
                err = writeRawConn(cc, b[..]); if (err == default!) {
                    tΔ2.Fatal(writeShouldFailˢ);
                }
            }
            {
                var perr = parseWriteError(err); if (perr != default!) {
                    tΔ2.Error(perr);
                }
            }
            if (!isDeadlineExceeded(err)) {
                tΔ2.Errorf("got %v; want timeout"u8, err);
            }
            {
                (_, err) = readRawConn(cc, b[..]); if (err == default!) {
                    tΔ2.Fatal(readShouldFailˢ);
                }
            }
            {
                var perr = parseReadError(err); if (perr != default!) {
                    tΔ2.Error(perr);
                }
            }
            if (!isDeadlineExceeded(err)) {
                tΔ2.Errorf("got %v; want timeout"u8, err);
            }
            c.SetReadDeadline(noDeadline);
            {
                var errΔ2 = c.SetReadDeadline(time.Now().Add(-1)); if (errΔ2 != default!) {
                    tΔ2.Fatal(errΔ2);
                }
            }
            {
                (_, err) = readRawConn(cc, b[..]); if (err == default!) {
                    tΔ2.Fatal(readShouldFailˢ);
                }
            }
            {
                var perr = parseReadError(err); if (perr != default!) {
                    tΔ2.Error(perr);
                }
            }
            if (!isDeadlineExceeded(err)) {
                tΔ2.Errorf("got %v; want timeout"u8, err);
            }
            c.SetWriteDeadline(noDeadline);
            {
                var errΔ3 = c.SetWriteDeadline(time.Now().Add(-1)); if (errΔ3 != default!) {
                    tΔ2.Fatal(errΔ3);
                }
            }
            {
                err = writeRawConn(cc, b[..]); if (err == default!) {
                    tΔ2.Fatal(writeShouldFailˢ);
                }
            }
            {
                var perr = parseWriteError(err); if (perr != default!) {
                    tΔ2.Error(perr);
                }
            }
            if (!isDeadlineExceeded(err)) {
                tΔ2.Errorf("got %v; want timeout"u8, err);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object controlAfterCloseShouldˢ = (@string)"Control after Close should fail"u8;

public static void TestRawConnControl(ж<testing.T> Ꮡt) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "plan9"u8 || exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8) {
        Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
    }

    Ꮡt.Run(tcpˢ2, (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            var ln = newLocalListener(new net_test_package.testing_TжTB(tΔ1), tcpˢ);
            var lnʗ1 = ln;
            defer(() => lnʗ1.Close(), ref ᒐ);
            var (cc1, err) = ln._<ж<global::go.net_package.TCPListener>>().SyscallConn();
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            {
                var errΔ1 = controlRawConn(cc1, ln.Addr()); if (errΔ1 != default!) {
                    tΔ1.Fatal(errΔ1);
                }
            }
            (var c, err) = Dial(ln.Addr().Network(), ln.Addr().String());
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            var cʗ1 = c;
            defer(() => cʗ1.Close(), ref ᒐ);
            (var cc2, err) = c._<ж<global::go.net_package.TCPConn>>().SyscallConn();
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            {
                var errΔ2 = controlRawConn(cc2, c.LocalAddr()); if (errΔ2 != default!) {
                    tΔ1.Fatal(errΔ2);
                }
            }
            ln.Close();
            {
                var errΔ3 = controlRawConn(cc1, ln.Addr()); if (errΔ3 == default!) {
                    tΔ1.Fatal(controlAfterCloseShouldˢ);
                }
            }
            c.Close();
            {
                var errΔ4 = controlRawConn(cc2, c.LocalAddr()); if (errΔ4 == default!) {
                    tΔ1.Fatal(controlAfterCloseShouldˢ);
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

} // end net_internal_test_package
