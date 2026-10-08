// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.net;

using bytes = bytes_package;
using binary = encoding.binary_package;
using io = io_package;
using rand = math.rand_package;
using net = net_package;
using runtime = runtime_package;
using sync = sync_package;
using testing = testing_package;
using time = time_package;
using System.Runtime.CompilerServices;
using encoding;
using math;

partial class nettest_package {

// type MakePipe is a methodless func type — rendered inline as its base delegate

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string basicIOˢ = "BasicIO"u8;
private static readonly @string pingPongˢ = "PingPong"u8;
private static readonly @string racyReadˢ = "RacyRead"u8;
private static readonly @string racyWriteˢ = "RacyWrite"u8;
private static readonly @string readTimeoutˢ = "ReadTimeout"u8;
private static readonly @string writeTimeoutˢ = "WriteTimeout"u8;
private static readonly @string pastTimeoutˢ = "PastTimeout"u8;
private static readonly @string presentTimeoutˢ = "PresentTimeout"u8;
private static readonly @string futureTimeoutˢ = "FutureTimeout"u8;
private static readonly @string closeTimeoutˢ = "CloseTimeout"u8;
private static readonly @string concurrentMethodsˢ = "ConcurrentMethods"u8;

// TestConn tests that a net.Conn implementation properly satisfies the interface.
// The tests should not produce any false positives, but may experience
// false negatives. Thus, some issues may only be detected when the test is
// run multiple times. For maximal effectiveness, run the tests under the
// race detector.
public static void TestConn(ж<testing.T> Ꮡt, Func<(net.Conn, net.Conn, Action, error)> mp) {
    Ꮡt.Run(basicIOˢ, (ж<testing.T> tΔ1) => {
        timeoutWrapper(tΔ1, mp, new Action<ж<testing.T>, net.Conn, net.Conn>(testBasicIO));
    });
    Ꮡt.Run(pingPongˢ, (ж<testing.T> tΔ2) => {
        timeoutWrapper(tΔ2, mp, new Action<ж<testing.T>, net.Conn, net.Conn>(testPingPong));
    });
    Ꮡt.Run(racyReadˢ, (ж<testing.T> tΔ3) => {
        timeoutWrapper(tΔ3, mp, new Action<ж<testing.T>, net.Conn, net.Conn>(testRacyRead));
    });
    Ꮡt.Run(racyWriteˢ, (ж<testing.T> tΔ4) => {
        timeoutWrapper(tΔ4, mp, new Action<ж<testing.T>, net.Conn, net.Conn>(testRacyWrite));
    });
    Ꮡt.Run(readTimeoutˢ, (ж<testing.T> tΔ5) => {
        timeoutWrapper(tΔ5, mp, new Action<ж<testing.T>, net.Conn, net.Conn>(testReadTimeout));
    });
    Ꮡt.Run(writeTimeoutˢ, (ж<testing.T> tΔ6) => {
        timeoutWrapper(tΔ6, mp, new Action<ж<testing.T>, net.Conn, net.Conn>(testWriteTimeout));
    });
    Ꮡt.Run(pastTimeoutˢ, (ж<testing.T> tΔ7) => {
        timeoutWrapper(tΔ7, mp, new Action<ж<testing.T>, net.Conn, net.Conn>(testPastTimeout));
    });
    Ꮡt.Run(presentTimeoutˢ, (ж<testing.T> tΔ8) => {
        timeoutWrapper(tΔ8, mp, new Action<ж<testing.T>, net.Conn, net.Conn>(testPresentTimeout));
    });
    Ꮡt.Run(futureTimeoutˢ, (ж<testing.T> tΔ9) => {
        timeoutWrapper(tΔ9, mp, new Action<ж<testing.T>, net.Conn, net.Conn>(testFutureTimeout));
    });
    Ꮡt.Run(closeTimeoutˢ, (ж<testing.T> tΔ10) => {
        timeoutWrapper(tΔ10, mp, new Action<ж<testing.T>, net.Conn, net.Conn>(testCloseTimeout));
    });
    Ꮡt.Run(concurrentMethodsˢ, (ж<testing.T> tΔ11) => {
        timeoutWrapper(tΔ11, mp, new Action<ж<testing.T>, net.Conn, net.Conn>(testConcurrentMethods));
    });
}

// type connTester is a methodless func type — rendered inline as its base delegate

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object testTimedOutTerminatingˢ = (@string)"test timed out; terminating pipe"u8;

internal static void timeoutWrapper(ж<testing.T> Ꮡt, Func<(net.Conn, net.Conn, Action, error)> mp, Action<ж<testing.T>, net.Conn, net.Conn> f) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Helper();
        var (c1, c2, stop, err) = mp();
        if (err != default!) {
            Ꮡt.Fatalf("unable to make pipe: %v"u8, err);
        }
        ref var once = ref heap(new sync.Once(), out var Ꮡonce);
        var stopʗ1 = stop;
        defer(Ꮡonce.Do, () => {
            stopʗ1();
        }, ref ᒐ);
        var stopʗ2 = stop;
        var timer = time.AfterFunc(time.ΔMinute, () => {
            var stopʗ3 = stopʗ2;
            Ꮡonce.Do(() => {
                Ꮡt.Error(testTimedOutTerminatingˢ);
                stopʗ3();
            });
        });
        var timerʗ1 = timer;
        defer(() => timerʗ1.Stop(), ref ᒐ);
        f(Ꮡt, c1, c2);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object transmittedDataDiffersˢ = (@string)"transmitted data differs"u8;

// testBasicIO tests that the data sent on c1 is properly received on c2.
internal static partial void testBasicIO(ж<testing.T> Ꮡt, net.Conn c1, net.Conn c2) {
    var want = new slice<byte>((1 << (int)(20)));
    rand.New(rand.NewSource(0)).Read(want);
    var dataCh = new channel<slice<byte>>(0);
    var wantʗ1 = want;
    goǃ(() => {
        var rd = bytes.NewReader(wantʗ1);
        {
            var err = chunkedCopy(new net_ConnᴠWriter(c1), new bytes_ReaderжReader(rd)); if (err != default!) {
                Ꮡt.Errorf("unexpected c1.Write error: %v"u8, err);
            }
        }
        {
            var err = c1.Close(); if (err != default!) {
                Ꮡt.Errorf("unexpected c1.Close error: %v"u8, err);
            }
        }
    });
    var dataChʗ1 = dataCh;
    goǃ(() => {
        var wr = @new<bytes.Buffer>();
        {
            var err = chunkedCopy(new bytes_BufferжWriter(wr), new net_ConnᴠReader(c2)); if (err != default!) {
                Ꮡt.Errorf("unexpected c2.Read error: %v"u8, err);
            }
        }
        {
            var err = c2.Close(); if (err != default!) {
                Ꮡt.Errorf("unexpected c2.Close error: %v"u8, err);
            }
        }
        dataChʗ1.ᐸꟷ(wr.Bytes());
    });
    {
        var got = ᐸꟷ(dataCh); if (!bytes.Equal(got, want)) {
            Ꮡt.Error(transmittedDataDiffersˢ);
        }
    }
}

// testPingPong tests that the two endpoints can synchronously send data to
// each other in a typical request-response pattern.
internal static partial void testPingPong(ж<testing.T> Ꮡt, net.Conn c1, net.Conn c2) {
    GoFrame ᒐ = default;
    try {
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        defer(Ꮡwg.Wait, ref ᒐ);
        void pingPonger(net.Conn c) {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                var buf = new slice<byte>(8);
                uint64 prev = default!;
                while (ᐧ) {
                    {
                        var (_, err) = io.ReadFull(new net_ConnᴠReader(c), buf); if (err != default!) {
                            if (AreEqual(err, io.EOF)) {
                                break;
                            }
                            Ꮡt.Errorf("unexpected Read error: %v"u8, err);
                        }
                    }
                    var v = binary.LittleEndian.Uint64(buf);
                    binary.LittleEndian.PutUint64(buf, v + 1);
                    if (prev != 0 && prev + 2 != v) {
                        Ꮡt.Errorf("mismatching value: got %d, want %d"u8, v, prev + 2);
                    }
                    prev = v;
                    if (v == 1000) {
                        break;
                    }
                    {
                        var (_, err) = c.Write(buf); if (err != default!) {
                            Ꮡt.Errorf("unexpected Write error: %v"u8, err);
                            break;
                        }
                    }
                }
                {
                    var err = c.Close(); if (err != default!) {
                        Ꮡt.Errorf("unexpected Close error: %v"u8, err);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }
        Ꮡwg.Add(2);
        var pingPongerʗ1 = pingPonger;
        goǃ(pingPongerʗ1, c1);
        var pingPongerʗ2 = pingPonger;
        goǃ(pingPongerʗ2, c2);
        // Start off the chain reaction.
        {
            var (_, err) = c1.Write(new slice<byte>(8)); if (err != default!) {
                Ꮡt.Errorf("unexpected c1.Write error: %v"u8, err);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// testRacyRead tests that it is safe to mutate the input Read buffer
// immediately after cancelation has occurred.
internal static partial void testRacyRead(ж<testing.T> Ꮡt, net.Conn c1, net.Conn c2) {
    GoFrame ᒐ = default;
    try {
        goǃ((ᴛ1, ᴛ2) => chunkedCopy(ᴛ1, ᴛ2), new net_ConnᴠWriter(c2), new rand_RandжReader(rand.New(rand.NewSource(0))));
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        defer(Ꮡwg.Wait, ref ᒐ);
        c1.SetReadDeadline(time.Now().Add(time.Millisecond));
        for (nint i = 0; i < 10; i++) {
            Ꮡwg.Add(1);
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    var b1 = new slice<byte>(1024);
                    var b2 = new slice<byte>(1024);
                    for (nint j = 0; j < 100; j++) {
                        var (_, err) = c1.Read(b1);
                        copy(b1, b2); // Mutate b1 to trigger potential race
                        if (err != default!) {
                            checkForTimeoutError(Ꮡt, err);
                            c1.SetReadDeadline(time.Now().Add(time.Millisecond));
                        }
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// testRacyWrite tests that it is safe to mutate the input Write buffer
// immediately after cancelation has occurred.
internal static partial void testRacyWrite(ж<testing.T> Ꮡt, net.Conn c1, net.Conn c2) {
    GoFrame ᒐ = default;
    try {
        goǃ((ᴛ1, ᴛ2) => chunkedCopy(ᴛ1, ᴛ2), io.Discard, new net_ConnᴠReader(c2));
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        defer(Ꮡwg.Wait, ref ᒐ);
        c1.SetWriteDeadline(time.Now().Add(time.Millisecond));
        for (nint i = 0; i < 10; i++) {
            Ꮡwg.Add(1);
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    var b1 = new slice<byte>(1024);
                    var b2 = new slice<byte>(1024);
                    for (nint j = 0; j < 100; j++) {
                        var (_, err) = c1.Write(b1);
                        copy(b1, b2); // Mutate b1 to trigger potential race
                        if (err != default!) {
                            checkForTimeoutError(Ꮡt, err);
                            c1.SetWriteDeadline(time.Now().Add(time.Millisecond));
                        }
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// testReadTimeout tests that Read timeouts do not affect Write.
internal static partial void testReadTimeout(ж<testing.T> Ꮡt, net.Conn c1, net.Conn c2) {
    goǃ((ᴛ1, ᴛ2) => chunkedCopy(ᴛ1, ᴛ2), io.Discard, new net_ConnᴠReader(c2));
    c1.SetReadDeadline(aLongTimeAgo);
    var (_, err) = c1.Read(new slice<byte>(1024));
    checkForTimeoutError(Ꮡt, err);
    {
        var (_, errΔ1) = c1.Write(new slice<byte>(1024)); if (errΔ1 != default!) {
            Ꮡt.Errorf("unexpected Write error: %v"u8, errΔ1);
        }
    }
}

// testWriteTimeout tests that Write timeouts do not affect Read.
internal static partial void testWriteTimeout(ж<testing.T> Ꮡt, net.Conn c1, net.Conn c2) {
    goǃ((ᴛ1, ᴛ2) => chunkedCopy(ᴛ1, ᴛ2), new net_ConnᴠWriter(c2), new rand_RandжReader(rand.New(rand.NewSource(0))));
    c1.SetWriteDeadline(aLongTimeAgo);
    var (_, err) = c1.Write(new slice<byte>(1024));
    checkForTimeoutError(Ꮡt, err);
    {
        var (_, errΔ1) = c1.Read(new slice<byte>(1024)); if (errΔ1 != default!) {
            Ꮡt.Errorf("unexpected Read error: %v"u8, errΔ1);
        }
    }
}

// testPastTimeout tests that a deadline set in the past immediately times out
// Read and Write requests.
internal static partial void testPastTimeout(ж<testing.T> Ꮡt, net.Conn c1, net.Conn c2) {
    goǃ((ᴛ1, ᴛ2) => chunkedCopy(ᴛ1, ᴛ2), new net_ConnᴠWriter(c2), new net_ConnᴠReader(c2));
    testRoundtrip(Ꮡt, c1);
    c1.SetDeadline(aLongTimeAgo);
    var (n, err) = c1.Write(new slice<byte>(1024));
    if (n != 0) {
        Ꮡt.Errorf("unexpected Write count: got %d, want 0"u8, n);
    }
    checkForTimeoutError(Ꮡt, err);
    (n, err) = c1.Read(new slice<byte>(1024));
    if (n != 0) {
        Ꮡt.Errorf("unexpected Read count: got %d, want 0"u8, n);
    }
    checkForTimeoutError(Ꮡt, err);
    testRoundtrip(Ꮡt, c1);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object readTimedOutBeforeˢ = (@string)"Read timed out before deadline is set"u8;
private static readonly object writeTimedOutBeforeˢ = (@string)"Write timed out before deadline is set"u8;

// testPresentTimeout tests that a past deadline set while there are pending
// Read and Write operations immediately times out those operations.
internal static partial void testPresentTimeout(ж<testing.T> Ꮡt, net.Conn c1, net.Conn c2) {
    GoFrame ᒐ = default;
    try {
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        defer(Ꮡwg.Wait, ref ᒐ);
        Ꮡwg.Add(3);
        var deadlineSet = new channel<bool>(1);
        var deadlineSetʗ1 = deadlineSet;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                time.Sleep(100 * time.Millisecond);
                deadlineSetʗ1.ᐸꟷ(true);
                c1.SetReadDeadline(aLongTimeAgo);
                c1.SetWriteDeadline(aLongTimeAgo);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var deadlineSetʗ2 = deadlineSet;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                var (n, err) = c1.Read(new slice<byte>(1024));
                if (n != 0) {
                    Ꮡt.Errorf("unexpected Read count: got %d, want 0"u8, n);
                }
                checkForTimeoutError(Ꮡt, err);
                if (len(deadlineSetʗ2) == 0) {
                    Ꮡt.Error(readTimedOutBeforeˢ);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var deadlineSetʗ3 = deadlineSet;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                error err = default!;
                while (err == default!) {
                    (_, err) = c1.Write(new slice<byte>(1024));
                }
                checkForTimeoutError(Ꮡt, err);
                if (len(deadlineSetʗ3) == 0) {
                    Ꮡt.Error(writeTimedOutBeforeˢ);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// testFutureTimeout tests that a future deadline will eventually time out
// Read and Write operations.
internal static partial void testFutureTimeout(ж<testing.T> Ꮡt, net.Conn c1, net.Conn c2) {
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(2);
    c1.SetDeadline(time.Now().Add(100 * time.Millisecond));
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            var (_, err) = c1.Read(new slice<byte>(1024));
            checkForTimeoutError(Ꮡt, err);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            error err = default!;
            while (err == default!) {
                (_, err) = c1.Write(new slice<byte>(1024));
            }
            checkForTimeoutError(Ꮡt, err);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    Ꮡwg.Wait();
    goǃ((ᴛ1, ᴛ2) => chunkedCopy(ᴛ1, ᴛ2), new net_ConnᴠWriter(c2), new net_ConnᴠReader(c2));
    resyncConn(Ꮡt, c1);
    testRoundtrip(Ꮡt, c1);
}

// testCloseTimeout tests that calling Close immediately times out pending
// Read and Write operations.
internal static partial void testCloseTimeout(ж<testing.T> Ꮡt, net.Conn c1, net.Conn c2) {
    GoFrame ᒐ = default;
    try {
        goǃ((ᴛ1, ᴛ2) => chunkedCopy(ᴛ1, ᴛ2), new net_ConnᴠWriter(c2), new net_ConnᴠReader(c2));
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        defer(Ꮡwg.Wait, ref ᒐ);
        Ꮡwg.Add(3);
        // Test for cancelation upon connection closure.
        c1.SetDeadline(neverTimeout);
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                time.Sleep(100 * time.Millisecond);
                c1.Close();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                error err = default!;
                var buf = new slice<byte>(1024);
                while (err == default!) {
                    (_, err) = c1.Read(buf);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                error err = default!;
                var buf = new slice<byte>(1024);
                while (err == default!) {
                    (_, err) = c1.Write(buf);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object skippingOnPlan9SeeHttpsˢ = (@string)"skipping on plan9; see https://golang.org/issue/20489"u8;

// testConcurrentMethods tests that the methods of net.Conn can safely
// be called concurrently.
internal static partial void testConcurrentMethods(ж<testing.T> Ꮡt, net.Conn c1, net.Conn c2) {
    if (runtime.GOOS == "plan9"u8) {
        Ꮡt.Skip(skippingOnPlan9SeeHttpsˢ);
    }
    goǃ((ᴛ1, ᴛ2) => chunkedCopy(ᴛ1, ᴛ2), new net_ConnᴠWriter(c2), new net_ConnᴠReader(c2));
    // The results of the calls may be nonsensical, but this should
    // not trigger a race detector warning.
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    for (nint i = 0; i < 100; i++) {
        Ꮡwg.Add(7);
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                c1.Read(new slice<byte>(1024));
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                c1.Write(new slice<byte>(1024));
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                c1.SetDeadline(time.Now().Add(10 * time.Millisecond));
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                c1.SetReadDeadline(aLongTimeAgo);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                c1.SetWriteDeadline(aLongTimeAgo);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                c1.LocalAddr();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                c1.RemoteAddr();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait(); // At worst, the deadline is set 10ms into the future
    resyncConn(Ꮡt, c1);
    testRoundtrip(Ꮡt, c1);
}

// checkForTimeoutError checks that the error satisfies the Error interface
// and that Timeout returns true.
internal static void checkForTimeoutError(ж<testing.T> Ꮡt, error err) {
    Ꮡt.Helper();
    {
        var (nerr, ok) = err._<netꓸError>(ᐧ); if (ok){
            if (!nerr.Timeout()) {
                if (runtime.GOOS == "windows"u8 && runtime.GOARCH == "arm64"u8 && Ꮡt.Name() == "TestTestConn/TCP/RacyRead"u8){
                    Ꮡt.Logf("ignoring known failure mode on windows/arm64; see https://go.dev/issue/52893"u8);
                } else {
                    Ꮡt.Errorf("got error: %v, want err.Timeout() = true"u8, nerr);
                }
            }
        } else {
            Ꮡt.Errorf("got %T: %v, want net.Error"u8, err, err);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string sᶜ = "Hello, world!"u8;

// testRoundtrip writes something into c and reads it back.
// It assumes that everything written into c is echoed back to itself.
internal static void testRoundtrip(ж<testing.T> Ꮡt, net.Conn c) {
    Ꮡt.Helper();
    {
        var err = c.SetDeadline(neverTimeout); if (err != default!) {
            Ꮡt.Errorf("roundtrip SetDeadline error: %v"u8, err);
        }
    }
    @string s = sᶜ;
    var buf = slice<byte>(s);
    {
        var (_, err) = c.Write(buf); if (err != default!) {
            Ꮡt.Errorf("roundtrip Write error: %v"u8, err);
        }
    }
    {
        var (_, err) = io.ReadFull(new net_ConnᴠReader(c), buf); if (err != default!) {
            Ꮡt.Errorf("roundtrip Read error: %v"u8, err);
        }
    }
    if (((sstring)buf) != s) {
        Ꮡt.Errorf("roundtrip data mismatch: got %q, want %q"u8, buf, s);
    }
}

// resyncConn resynchronizes the connection into a sane state.
// It assumes that everything written into c is echoed back to itself.
// It assumes that 0xff is not currently on the wire or in the read buffer.
internal static partial void resyncConn(ж<testing.T> Ꮡt, net.Conn c) {
    Ꮡt.Helper();
    c.SetDeadline(neverTimeout);
    var errCh = new channel<error>(0);
    var errChʗ1 = errCh;
    goǃ(() => {
        var (_, err) = c.Write(new byte[]{0xff}.slice());
        errChʗ1.ᐸꟷ(err);
    });
    var buf = new slice<byte>(1024);
    while (ᐧ) {
        var (n, err) = c.Read(buf);
        if (n > 0 && bytes.IndexByte(buf.slice(0, n), 0xff) == n - 1) {
            break;
        }
        if (err != default!) {
            Ꮡt.Errorf("unexpected Read error: %v"u8, err);
            break;
        }
    }
    {
        var err = ᐸꟷ(errCh); if (err != default!) {
            Ꮡt.Errorf("unexpected Write error: %v"u8, err);
        }
    }
}

internal partial struct chunkedCopy_dst /*dyn*/ {
    /*embed*/ public io_package.Writer Writer;
}

internal partial struct chunkedCopy_src /*dyn*/ {
    /*embed*/ public io_package.Reader Reader;
}

// chunkedCopy copies from r to w in fixed-width chunks to avoid
// causing a Write that exceeds the maximum packet size for packet-based
// connections like "unixpacket".
// We assume that the maximum packet size is at least 1024.
internal static error chunkedCopy(io.Writer w, io.Reader r) {
    var b = new slice<byte>(1024);
    var (_, err) = io.CopyBuffer(new chunkedCopy_dst(w), new chunkedCopy_src(r), b);
    return err;
}

} // end nettest_package
