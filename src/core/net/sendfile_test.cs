// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using context = context_package;
using sha256 = crypto.sha256_package;
using hex = encoding.hex_package;
using errors = errors_package;
using fmt = fmt_package;
using poll = @internal.poll_package;
using Δio = io_package;
using rand = math.rand_package;
using Δos = os_package;
using Δruntime = runtime_package;
using strconv = strconv_package;
using Δsync = sync_package;
using testing = testing_package;
using time = time_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using crypto;
using encoding;
using fs = go.io.fs_package;
using hash = hash_package;
using math;
using static go.net_package;

partial class net_internal_test_package {

internal static readonly @string newton = "../testdata/Isaac.Newton-Opticks.txt"u8;
internal static UntypedInt newtonLen => 567198;
internal static readonly @string newtonSHA256 = "d4a9ac22462b35e7821a4f2706c211093da678620a8f9997989ee7cf8d507bbd"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object internalPollSendFileˢ = (@string)"internal/poll.SendFile called multiple times, want one call"u8;
internal static readonly object internalPollSendFileWasˢ = (@string)"internal/poll.SendFile was not called, want it to be"u8;
internal static readonly object internalPollSendFileDidˢ = (@string)"internal/poll.SendFile did not handle the write, want it to, error:"u8;
internal static readonly object internalPollSendFileˢ2 = (@string)"internal.poll.SendFile called with unexpected FD"u8;

// expectSendfile runs f, and verifies that internal/poll.SendFile successfully handles
// a write to wantConn during f's execution.
//
// On platforms where supportsSendfile is false, expectSendfile runs f but does not
// expect a call to SendFile.
internal static void expectSendfile(ж<testing.T> Ꮡt, global::go.net_package.Conn wantConn, Action f) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Helper();
        if (!supportsSendfile) {
            f();
            return;
        }
        var orig = poll.TestHookDidSendFile;
        var origʗ1 = orig;
        defer(() => {
            poll.TestHookDidSendFile = origʗ1;
        }, ref ᒐ);
        bool called = default!;
        bool gotHandled = default!;
        ref var gotFD = ref heap<ж<poll.FD>>(out var ᏑgotFD);
        ref var gotErr = ref heap<error>(out var ᏑgotErr);
        poll.TestHookDidSendFile = (ж<poll.FD> dstFD, nint src, int64 written, error err, bool handled) => {
            if (called) {
                Ꮡt.Error(internalPollSendFileˢ);
            }
            called = true;
            gotHandled = handled;
            ᏑgotFD.ValueSlot = dstFD;
            ᏑgotErr.ValueSlot = err;
        };
        f();
        if (!called) {
            Ꮡt.Error(internalPollSendFileWasˢ);
            return;
        }
        if (!gotHandled) {
            Ꮡt.Error(internalPollSendFileDidˢ, gotErr);
            return;
        }
        if ((~wantConn._<ж<global::go.net_package.TCPConn>>()).fd.of(global::go.net_package.netFD.Ꮡpfd) != gotFD) {
            Ꮡt.Error(internalPollSendFileˢ2);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestSendfile(ж<testing.T> Ꮡt) {
    testSendfile(Ꮡt, newton, newtonSHA256, newtonLen, 0);
}

public static void TestSendfileWithExactLimit(ж<testing.T> Ꮡt) {
    testSendfile(Ꮡt, newton, newtonSHA256, newtonLen, newtonLen);
}

public static void TestSendfileWithLimitLargerThanFile(ж<testing.T> Ꮡt) {
    testSendfile(Ꮡt, newton, newtonSHA256, newtonLen, newtonLen * 2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingOnNonAmd64AndNonˢ = (@string)"skipping on non-amd64 and non-arm64 platforms"u8;
internal static readonly object skipItDuringShortTestingˢ = (@string)"Skip it during short testing"u8;

public static void TestSendfileWithLargeFile(ж<testing.T> Ꮡt) {
    // Some platforms are not capable of handling large files with sendfile
    // due to limited system resource, so we only run this test on amd64 and
    // arm64 for the moment.
    if (Δruntime.GOARCH != "amd64"u8 && Δruntime.GOARCH != "arm64"u8) {
        Ꮡt.Skip(skippingOnNonAmd64AndNonˢ);
    }
    // Also skip it during short testing.
    if (testing.Short()) {
        Ꮡt.Skip(skipItDuringShortTestingˢ);
    }
    // We're using 1<<31 - 1 as the chunk size for sendfile currently,
    // make an edge case file that is 1 byte bigger than that.
    var f = createTempFile(new net_test_package.testing_TжTB(Ꮡt), 2147483648L);
    // For big file like this, only verify the transmission of the file,
    // skip the content check.
    testSendfile(Ꮡt, f.Name(), ""u8, 2147483648L, 0);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object retrievedDataHashDidNotˢ = (@string)"retrieved data hash did not match"u8;

internal static partial void testSendfile(ж<testing.T> Ꮡt, @string filePath, @string fileHash, int64 size, int64 limit) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var errc = new channel<error>(1);
        var errcʗ1 = errc;
        goǃ([MethodImpl(MethodImplOptions.NoInlining)] (global::go.net_package.Listener lnΔ1) => {
            // Wait for a connection.
            var (conn, errΔ1) = lnΔ1.Accept();
            if (errΔ1 != default!) {
                errcʗ1.ᐸꟷ(errΔ1);
                builtin.close(errcʗ1);
                return;
            }
            var connʗ1 = conn;
            var errcʗ2 = errcʗ1;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(ᴛ1 => builtin.close(ᴛ1), errcʗ2, ref ᒐ);
                    var connʗ2 = connʗ1;
                    defer(() => connʗ2.Close(), ref ᒐ);
                    ref var errΔ2 = ref heap<error>(out var ᏑerrΔ2);
                    (var f, ᏑerrΔ2.ValueSlot) = Δos.Open(filePath);
                    if (ᏑerrΔ2.ValueSlot != default!) {
                        errcʗ2.ᐸꟷ(ᏑerrΔ2.ValueSlot);
                        return;
                    }
                    var fʗ1 = f;
                    defer(() => fʗ1.Close(), ref ᒐ);
                    // Return file data using io.Copy, which should use
                    // sendFile if available.
                    int64 sbytes = default!;
                    var exprᴛ1 = Δruntime.GOOS;
                    if (exprᴛ1 == "windows"u8) {
                        (sbytes, ᏑerrΔ2.ValueSlot) = Δio.Copy(new net_test_package.net_ConnᴠWriter(connʗ1), // Windows is not using sendfile for some reason:
 // https://go.dev/issue/67042
 new net_test_package.os_FileжReader(f));
                    }
                    else { /* default: */
                        var connʗ3 = connʗ1;
                        var fʗ2 = f;
                        expectSendfile(Ꮡt, connʗ1, () => {
                            if (limit > 0){
                                (sbytes, ᏑerrΔ2.ValueSlot) = Δio.CopyN(new net_test_package.net_ConnᴠWriter(connʗ3), new net_test_package.os_FileжReader(fʗ2), limit);
                                if (AreEqual(ᏑerrΔ2.ValueSlot, Δio.EOF) && limit > size) {
                                    ᏑerrΔ2.ValueSlot = default!;
                                }
                            } else {
                                (sbytes, ᏑerrΔ2.ValueSlot) = Δio.Copy(new net_test_package.net_ConnᴠWriter(connʗ3), new net_test_package.os_FileжReader(fʗ2));
                            }
                        });
                    }

                    if (ᏑerrΔ2.ValueSlot != default!) {
                        errcʗ2.ᐸꟷ(ᏑerrΔ2.ValueSlot);
                        return;
                    }
                    if (sbytes != size) {
                        errcʗ2.ᐸꟷ(fmt.Errorf("sent %d bytes; expected %d"u8, sbytes, size));
                        return;
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }, ln);
        // Connect to listener to retrieve file and verify digest matches
        // expected.
        var (c, err) = Dial(tcpˢ, ln.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        var h = sha256.New();
        (var rbytes, err) = Δio.Copy(h, new net_test_package.net_ConnᴠReader(c));
        if (err != default!) {
            Ꮡt.Error(err);
        }
        if (rbytes != size) {
            Ꮡt.Errorf("received %d bytes; expected %d"u8, rbytes, size);
        }
        if (len(fileHash) > 0 && hex.EncodeToString(h.Sum(default!)) != newtonSHA256) {
            Ꮡt.Error(retrievedDataHashDidNotˢ);
        }
        foreach (var errΔ3 in errc) {
            Ꮡt.Error(errΔ3);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string producedˢ = "Produced "u8;

public static partial void TestSendfileParts(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var errc = new channel<error>(1);
        var errcʗ1 = errc;
        goǃ([MethodImpl(MethodImplOptions.NoInlining)] (global::go.net_package.Listener lnΔ1) => {
            // Wait for a connection.
            var (conn, errΔ1) = lnΔ1.Accept();
            if (errΔ1 != default!) {
                errcʗ1.ᐸꟷ(errΔ1);
                builtin.close(errcʗ1);
                return;
            }
            var connʗ1 = conn;
            var errcʗ2 = errcʗ1;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(ᴛ1 => builtin.close(ᴛ1), errcʗ2, ref ᒐ);
                    var connʗ2 = connʗ1;
                    defer(() => connʗ2.Close(), ref ᒐ);
                    ref var errΔ2 = ref heap<error>(out var ᏑerrΔ2);
                    (var f, ᏑerrΔ2.ValueSlot) = Δos.Open(newton);
                    if (ᏑerrΔ2.ValueSlot != default!) {
                        errcʗ2.ᐸꟷ(ᏑerrΔ2.ValueSlot);
                        return;
                    }
                    var fʗ1 = f;
                    defer(() => fʗ1.Close(), ref ᒐ);
                    for (nint i = 0; i < 3; i++) {
                        // Return file data using io.CopyN, which should use
                        // sendFile if available.
                        var connʗ3 = connʗ1;
                        var fʗ2 = f;
                        expectSendfile(Ꮡt, connʗ1, () => {
                            (_, ᏑerrΔ2.ValueSlot) = Δio.CopyN(new net_test_package.net_ConnᴠWriter(connʗ3), new net_test_package.os_FileжReader(fʗ2), 3);
                        });
                        if (ᏑerrΔ2.ValueSlot != default!) {
                            errcʗ2.ᐸꟷ(ᏑerrΔ2.ValueSlot);
                            return;
                        }
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }, ln);
        var (c, err) = Dial(tcpˢ, ln.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        var buf = @new<bytes.Buffer>();
        buf.ReadFrom(new net_test_package.net_ConnᴠReader(c));
        {
            @string want = producedˢ;
            @string have = buf.String(); if (have != want) {
                Ꮡt.Errorf("unexpected server reply %q, want %q"u8, have, want);
            }
        }
        foreach (var errΔ3 in errc) {
            Ꮡt.Error(errΔ3);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial void TestSendfileSeeked(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        const int64 seekTo = /* 65 << 10 */ 66560;
        UntypedInt sendSize = /* 10 << 10 */ 10240;
        var errc = new channel<error>(1);
        var errcʗ1 = errc;
        goǃ([MethodImpl(MethodImplOptions.NoInlining)] (global::go.net_package.Listener lnΔ1) => {
            // Wait for a connection.
            var (conn, errΔ1) = lnΔ1.Accept();
            if (errΔ1 != default!) {
                errcʗ1.ᐸꟷ(errΔ1);
                builtin.close(errcʗ1);
                return;
            }
            var connʗ1 = conn;
            var errcʗ2 = errcʗ1;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(ᴛ1 => builtin.close(ᴛ1), errcʗ2, ref ᒐ);
                    var connʗ2 = connʗ1;
                    defer(() => connʗ2.Close(), ref ᒐ);
                    ref var errΔ2 = ref heap<error>(out var ᏑerrΔ2);
                    (var f, ᏑerrΔ2.ValueSlot) = Δos.Open(newton);
                    if (ᏑerrΔ2.ValueSlot != default!) {
                        errcʗ2.ᐸꟷ(ᏑerrΔ2.ValueSlot);
                        return;
                    }
                    var fʗ1 = f;
                    defer(() => fʗ1.Close(), ref ᒐ);
                    {
                        var (_, errΔ3) = f.Seek(seekTo, Δio.SeekStart); if (errΔ3 != default!) {
                            errcʗ2.ᐸꟷ(errΔ3);
                            return;
                        }
                    }
                    var connʗ3 = connʗ1;
                    var fʗ2 = f;
                    expectSendfile(Ꮡt, connʗ1, () => {
                        (_, ᏑerrΔ2.ValueSlot) = Δio.CopyN(new net_test_package.net_ConnᴠWriter(connʗ3), new net_test_package.os_FileжReader(fʗ2), sendSize);
                    });
                    if (ᏑerrΔ2.ValueSlot != default!) {
                        errcʗ2.ᐸꟷ(ᏑerrΔ2.ValueSlot);
                        return;
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }, ln);
        var (c, err) = Dial(tcpˢ, ln.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        var buf = @new<bytes.Buffer>();
        buf.ReadFrom(new net_test_package.net_ConnᴠReader(c));
        if (buf.Len() != sendSize) {
            Ꮡt.Errorf("Got %d bytes; want %d"u8, buf.Len(), (nint)(sendSize));
        }
        foreach (var errΔ4 in errc) {
            Ꮡt.Error(errΔ4);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readDidNotTimeOutˢ = (@string)"Read did not time out"u8;

// Test that sendfile doesn't put a pipe into blocking mode.
public static partial void TestSendfilePipe(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8 || exprᴛ1 == "windows"u8 || exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8) {
            Ꮡt.Skipf("skipping on %s"u8, // These systems don't support deadlines on pipes.
 Δruntime.GOOS);
        }

        Ꮡt.Parallel();
        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var (r, w, err) = Δos.Pipe();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var wʗ1 = w;
        defer(() => wʗ1.Close(), ref ᒐ);
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        var copied = new channel<bool>(0);
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(1);
        var copiedʗ1 = copied;
        var lnʗ2 = ln;
        var rʗ2 = r;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                // Accept a connection and copy 1 byte from the read end of
                // the pipe to the connection. This will call into sendfile.
                defer(Ꮡwg.Done, ref ᒐ);
                var (conn, errΔ1) = lnʗ2.Accept();
                if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                    return;
                }
                var connʗ1 = conn;
                defer(() => connʗ1.Close(), ref ᒐ);
                // The comment above states that this should call into sendfile,
                // but empirically it doesn't seem to do so at this time.
                // If it does, or does on some platforms, this CopyN should be wrapped
                // in expectSendfile.
                (_, errΔ1) = Δio.CopyN(new net_test_package.net_ConnᴠWriter(conn), new net_test_package.os_FileжReader(rʗ2), 1);
                if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                    return;
                }
                // Signal the main goroutine that we've copied the byte.
                builtin.close(copiedʗ1);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        Ꮡwg.Add(1);
        var wʗ2 = w;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                // Write 1 byte to the write end of the pipe.
                defer(Ꮡwg.Done, ref ᒐ);
                var (_, errΔ2) = wʗ2.Write(new byte[]{(rune)'a'}.slice());
                if (errΔ2 != default!) {
                    Ꮡt.Error(errΔ2);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        Ꮡwg.Add(1);
        var lnʗ3 = ln;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                // Connect to the server started two goroutines up and
                // discard any data that it writes.
                defer(Ꮡwg.Done, ref ᒐ);
                var (conn, errΔ3) = Dial(tcpˢ, lnʗ3.Addr().String());
                if (errΔ3 != default!) {
                    Ꮡt.Error(errΔ3);
                    return;
                }
                var connʗ2 = conn;
                defer(() => connʗ2.Close(), ref ᒐ);
                Δio.Copy(Δio.Discard, new net_test_package.net_ConnᴠReader(conn));
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        // Wait for the byte to be copied, meaning that sendfile has
        // been called on the pipe.
        ᐸꟷ(copied);
        // Set a very short deadline on the read end of the pipe.
        {
            var errΔ4 = r.SetDeadline(time.Now().Add(time.Microsecond)); if (errΔ4 != default!) {
                Ꮡt.Fatal(errΔ4);
            }
        }
        Ꮡwg.Add(1);
        var wʗ3 = w;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                // Wait for much longer than the deadline and write a byte
                // to the pipe.
                defer(Ꮡwg.Done, ref ᒐ);
                time.Sleep(50 * time.Millisecond);
                wʗ3.Write(new byte[]{(rune)'b'}.slice());
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        // If this read does not time out, the pipe was incorrectly
        // put into blocking mode.
        (_, err) = r.Read(new slice<byte>(1));
        if (err == default!){
            Ꮡt.Error(readDidNotTimeOutˢ);
        } else 
        if (!Δos.IsTimeout(err)) {
            Ꮡt.Errorf("got error %v, expected a time out"u8, err);
        }
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 43822: tests that returns EOF when conn write timeout.
public static partial void TestSendfileOnWriteTimeoutExceeded(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var errc = new channel<error>(1);
        var errcʗ1 = errc;
        goǃ(error (global::go.net_package.Listener lnΔ1) => {
            error retErr = default!;
            GoFrame ᒐ = default;
            try {
                var errcʗ2 = errcʗ1;
                defer(() => {
                    errcʗ2.ᐸꟷ(retErr);
                    builtin.close(errcʗ2);
                }, ref ᒐ);
                var (connΔ1, errΔ1) = lnΔ1.Accept();
                if (errΔ1 != default!) {
                    retErr = errΔ1; goto ᒐdone1;
                }
                var connʗ1 = connΔ1;
                defer(() => connʗ1.Close(), ref ᒐ);
                // Set the write deadline in the past(1h ago). It makes
                // sure that it is always write timeout.
                {
                    var errΔ2 = connΔ1.SetWriteDeadline(time.Now().Add((time.Duration)(-3600000000000L))); if (errΔ2 != default!) {
                        retErr = errΔ2; goto ᒐdone1;
                    }
                }
                (var f, errΔ1) = Δos.Open(newton);
                if (errΔ1 != default!) {
                    retErr = errΔ1; goto ᒐdone1;
                }
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);
                // We expect this to use sendfile, but as of the time this comment was written
                // poll.SendFile on an FD past its timeout can return an error indicating that
                // it didn't handle the operation, resulting in a non-sendfile retry.
                // So don't use expectSendfile here.
                (_, errΔ1) = Δio.Copy(new net_test_package.net_ConnᴠWriter(connΔ1), new net_test_package.os_FileжReader(f));
                if (errors.Is(errΔ1, Δos.ErrDeadlineExceeded)) {
                    retErr = default!; goto ᒐdone1;
                }
                if (errΔ1 == default!) {
                    errΔ1 = fmt.Errorf("expected ErrDeadlineExceeded, but got nil"u8);
                }
                retErr = errΔ1; goto ᒐdone1;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
            ᒐdone1: return retErr;
        }, ln);
        var (conn, err) = Dial(tcpˢ, ln.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var connʗ2 = conn;
        defer(() => connʗ2.Close(), ref ᒐ);
        (var n, err) = Δio.Copy(Δio.Discard, new net_test_package.net_ConnᴠReader(conn));
        if (err != default!) {
            Ꮡt.Fatalf("expected nil error, but got %v"u8, err);
        }
        if (n != 0) {
            Ꮡt.Fatalf("expected receive zero, but got %d byte(s)"u8, n);
        }
        {
            var errΔ3 = ᐸꟷ(errc); if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testTxtˢ = "test.txt"u8;

public static partial void BenchmarkSendfileZeroBytes(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);

        var (ctx, cancel) = context.WithCancel(context.Background());
        defer(Ꮡwg.Wait, ref ᒐ);
        var ln = newLocalListener(new net_test_package.testing_BжTB(Ꮡb), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var (tempFile, err) = Δos.CreateTemp(Ꮡb.TempDir(), testTxtˢ);
        if (err != default!) {
            Ꮡb.Fatalf("failed to create temp file: %v"u8, err);
        }
        var tempFileʗ1 = tempFile;
        defer(() => tempFileʗ1.Close(), ref ᒐ);
        @string fileName = tempFile.Name();
        nint dataSize = b.N;
        Ꮡwg.Add(1);
        goǃ((ж<Δos.File> f) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                for (nint i = 0; i < dataSize; i++) {
                    {
                        var (_, errΔ1) = f.Write(new byte[]{1}.slice()); if (errΔ1 != default!) {
                            Ꮡb.Errorf("failed to write: %v"u8, errΔ1);
                            return;
                        }
                    }
                    if (i % 1000 == 0) {
                        f.Sync();
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, tempFile);
        b.ResetTimer();
        b.ReportAllocs();
        Ꮡwg.Add(1);
        var ctxʗ1 = ctx;
        goǃ((global::go.net_package.Listener lnΔ1, @string fileNameΔ1) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                var (connΔ1, errΔ2) = lnΔ1.Accept();
                if (errΔ2 != default!) {
                    Ꮡb.Errorf("failed to accept: %v"u8, errΔ2);
                    return;
                }
                var connʗ1 = connΔ1;
                defer(() => connʗ1.Close(), ref ᒐ);
                (var f, errΔ2) = Δos.OpenFile(fileNameΔ1, Δos.O_RDONLY, 432);
                if (errΔ2 != default!) {
                    Ꮡb.Errorf("failed to open file: %v"u8, errΔ2);
                    return;
                }
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);
                while (ᐧ) {
                    if (ctxʗ1.Err() != default!) {
                        return;
                    }
                    {
                        var (_, errΔ3) = Δio.Copy(new net_test_package.net_ConnᴠWriter(connΔ1), new net_test_package.os_FileжReader(f)); if (errΔ3 != default!) {
                            Ꮡb.Errorf("failed to copy: %v"u8, errΔ3);
                            return;
                        }
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, ln, fileName);
        (var conn, err) = Dial(tcpˢ, ln.Addr().String());
        if (err != default!) {
            Ꮡb.Fatalf("failed to dial: %v"u8, err);
        }
        var connʗ2 = conn;
        defer(() => connʗ2.Close(), ref ᒐ);
        (var n, err) = Δio.CopyN(Δio.Discard, new net_test_package.net_ConnᴠReader(conn), (int64)dataSize);
        if (err != default!) {
            Ꮡb.Fatalf("failed to copy: %v"u8, err);
        }
        if (n != (int64)dataSize) {
            Ꮡb.Fatalf("expected %d copied bytes, but got %d"u8, dataSize, n);
        }
        cancel();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fileToTcpˢ = "file-to-tcp"u8;
internal static readonly @string fileToUnixˢ = "file-to-unix"u8;

public static void BenchmarkSendFile(ж<testing.B> Ꮡb) {
    if (Δruntime.GOOS == "windows"u8) {
        // TODO(panjf2000): Windows has not yet implemented FileConn,
        //		remove this when it's implemented in https://go.dev/issues/9503.
        Ꮡb.Skipf("skipping on %s"u8, Δruntime.GOOS);
    }
    Ꮡb.Run(fileToTcpˢ, (ж<testing.B> bΔ1) => {
        benchmarkSendFile(bΔ1, tcpˢ);
    });
    Ꮡb.Run(fileToUnixˢ, (ж<testing.B> bΔ2) => {
        benchmarkSendFile(bΔ2, unixˢ);
    });
}

internal static void benchmarkSendFile(ж<testing.B> Ꮡb, @string proto) {
    for (nint i = 0; i <= 10; i++) {
        nint size = ((nint)1).Lsh((int64)((i + 10)));
        var bench = new sendFileBench(
            proto: proto,
            chunkSize: size
        );
        var benchʗ1 = bench;
        Ꮡb.Run(strconv.Itoa(size), (ж<testing.B> p1) => benchʗ1.benchSendFile(p1));
    }
}

internal partial struct sendFileBench {
    internal @string proto;
    internal nint chunkSize;
}

internal static void benchSendFile(this sendFileBench bench, ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        nint fileSize = b.N * bench.chunkSize;
        var f = createTempFile(new net_test_package.testing_BжTB(Ꮡb), (int64)fileSize);
        var (client, server) = spawnTestSocketPair(new net_test_package.testing_BжTB(Ꮡb), bench.proto);
        var serverʗ1 = server;
        defer(() => serverʗ1.Close(), ref ᒐ);
        var (cleanUp, err) = startTestSocketPeer(new net_test_package.testing_BжTB(Ꮡb), client, "r"u8, bench.chunkSize, fileSize);
        if (err != default!) {
            client.Close();
            Ꮡb.Fatal(err);
        }
        var cleanUpʗ1 = cleanUp;
        defer(cleanUpʗ1, new net_test_package.testing_BжTB(Ꮡb), ref ᒐ);
        b.ReportAllocs();
        b.SetBytes((int64)bench.chunkSize);
        b.ResetTimer();
        // Data go from file to socket via sendfile(2).
        (var sent, err) = Δio.Copy(new net_test_package.net_ConnᴠWriter(server), new net_test_package.os_FileжReader(f));
        if (err != default!) {
            Ꮡb.Fatalf("failed to copy data with sendfile, error: %v"u8, err);
        }
        if (sent != (int64)fileSize) {
            Ꮡb.Fatalf("bytes sent mismatch, got: %d, want: %d"u8, sent, fileSize);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sendfileBenchˢ = "sendfile-bench"u8;

internal static ж<Δos.File> createTempFile(testing.TB tb, int64 size) {
    var (f, err) = Δos.CreateTemp(tb.TempDir(), sendfileBenchˢ);
    if (err != default!) {
        tb.Fatalf("failed to create temporary file: %v"u8, err);
    }
    var fʗ1 = f;
    tb.Cleanup(() => {
        fʗ1.Close();
    });
    {
        var (_, errΔ1) = Δio.CopyN(new Δos.FileжWriter(f), newRandReader(tb), size); if (errΔ1 != default!) {
            tb.Fatalf("failed to fill the file with random data: %v"u8, errΔ1);
        }
    }
    {
        var (_, errΔ2) = f.Seek(0, Δio.SeekStart); if (errΔ2 != default!) {
            tb.Fatalf("failed to rewind the file: %v"u8, errΔ2);
        }
    }
    return f;
}

internal static Δio.Reader newRandReader(testing.TB tb) {
    var seed = time.Now().UnixNano();
    tb.Logf("Deterministic RNG seed based on timestamp: 0x%x"u8, seed);
    return new net_test_package.rand_RandжReader(rand.New(rand.NewSource(seed)));
}

} // end net_internal_test_package
