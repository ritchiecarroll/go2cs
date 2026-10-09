// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using fmt = fmt_package;
using poll = @internal.poll_package;
using Δio = io_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using Δsync = sync_package;
using testing = testing_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.net_package;
using time = time_package;

partial class net_internal_test_package {

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string storyᶜ = "once upon a time in Gopherland ... "u8;

public static void TestBuffers_read(ж<testing.T> Ꮡt) {
    @string story = storyᶜ;
    ref var buffers = ref heap<global::go.net_package.Buffers>(out var Ꮡbuffers);
    buffers = new Buffers(new slice<byte>[]{
        slice<byte>("once "u8),
        slice<byte>("upon "u8),
        slice<byte>("a "u8),
        slice<byte>("time "u8),
        slice<byte>("in "u8),
        slice<byte>("Gopherland ... "u8)
    }.slice());
    var (got, err) = Δio.ReadAll(new global::go.net_package.BuffersжReader(Ꮡbuffers));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (((sstring)got) != story) {
        Ꮡt.Errorf("read %q; want %q"u8, got, story);
    }
    if (len(buffers) != 0) {
        Ꮡt.Errorf("len(buffers) = %d; want 0"u8, len(buffers));
    }
}

internal partial struct TestBuffers_consume_tests /*dyn*/ {
    internal global::go.net_package.Buffers @in;
    internal int64 consume;
    internal global::go.net_package.Buffers want;
}

public static void TestBuffers_consume(ж<testing.T> Ꮡt) {
    var tests = new TestBuffers_consume_tests[]{
        new(
            @in: new Buffers(new slice<byte>[]{slice<byte>("foo"u8), slice<byte>("bar"u8)}.slice()),
            consume: 0,
            want: new Buffers(new slice<byte>[]{slice<byte>("foo"u8), slice<byte>("bar"u8)}.slice())
        ),
        new(
            @in: new Buffers(new slice<byte>[]{slice<byte>("foo"u8), slice<byte>("bar"u8)}.slice()),
            consume: 2,
            want: new Buffers(new slice<byte>[]{slice<byte>("o"u8), slice<byte>("bar"u8)}.slice())
        ),
        new(
            @in: new Buffers(new slice<byte>[]{slice<byte>("foo"u8), slice<byte>("bar"u8)}.slice()),
            consume: 3,
            want: new Buffers(new slice<byte>[]{slice<byte>("bar"u8)}.slice())
        ),
        new(
            @in: new Buffers(new slice<byte>[]{slice<byte>("foo"u8), slice<byte>("bar"u8)}.slice()),
            consume: 4,
            want: new Buffers(new slice<byte>[]{slice<byte>("ar"u8)}.slice())
        ),
        new(
            @in: new Buffers(new slice<byte>[]{default!, default!, default!, slice<byte>("bar"u8)}.slice()),
            consume: 1,
            want: new Buffers(new slice<byte>[]{slice<byte>("ar"u8)}.slice())
        ),
        new(
            @in: new Buffers(new slice<byte>[]{default!, default!, default!, slice<byte>("foo"u8)}.slice()),
            consume: 0,
            want: new Buffers(new slice<byte>[]{slice<byte>("foo"u8)}.slice())
        ),
        new(
            @in: new Buffers(new slice<byte>[]{default!, default!, default!}.slice()),
            consume: 0,
            want: new Buffers(new slice<byte>[]{}.slice())
        )
    }.slice();
    foreach (var (i, tt) in tests) {
        var @in = tt.@in;
        @in.consume(tt.consume);
        if (!reflect.DeepEqual(@in, tt.want)) {
            Ꮡt.Errorf("%d. after consume(%d) = %+v, want %+v"u8, i, tt.consume, @in, tt.want);
        }
    }
}

public static void TestBuffers_WriteTo(ж<testing.T> Ꮡt) {
    foreach (var (_, name) in new @string[]{"WriteTo"u8, "Copy"u8}.slice()) {
        foreach (var (_, size) in new nint[]{0, 10, 1023, 1024, 1025}.slice()) {
            Ꮡt.Run(fmt.Sprintf("%s/%d"u8, name, size), (ж<testing.T> tΔ1) => {
                testBuffer_writeTo(tΔ1, size, name == "Copy"u8);
            });
        }
    }
}

internal partial struct testBuffer_writeTo_writeLog /*dyn*/ {
    public partial ref sync_package.Mutex Mutex { get; }
    internal slice<nint> log;
}

internal static void testBuffer_writeTo(ж<testing.T> Ꮡt, nint chunks, bool useCopy) {
    GoFrame ᒐ = default;
    try {
        var oldHook = poll.TestHookDidWritev;
        var oldHookʗ1 = oldHook;
        defer(() => {
            poll.TestHookDidWritev = oldHookʗ1;
        }, ref ᒐ);
        ref var writeLog = ref heap(new testBuffer_writeTo_writeLog(), out var ᏑwriteLog);
        poll.TestHookDidWritev = (nint size) => {
            ᏑwriteLog.of(testBuffer_writeTo_writeLog.ᏑMutex).Lock();
            ᏑwriteLog.Value.log = append(ᏑwriteLog.Value.log, size);
            ᏑwriteLog.of(testBuffer_writeTo_writeLog.ᏑMutex).Unlock();
        };
        ref var want = ref heap(new bytes.Buffer(), out var Ꮡwant);
        for (nint i = 0; i < chunks; i++) {
            want.WriteByte((byte)i);
        }

        withTCPConnPair(Ꮡt, error (ж<global::go.net_package.TCPConn> c) => {
            ref var buffers = ref heap<global::go.net_package.Buffers>(out var Ꮡbuffers);
            buffers = new global::go.net_package.Buffers(chunks);
            foreach (var (i, _) in buffers) {
                buffers[i] = Ꮡwant.Value.Bytes().slice(i, i + 1);
            }
            int64 n = default!;
            error err = default!;
            if (useCopy){
                (n, err) = Δio.Copy(new net_test_package.net_TCPConnжWriter(c), new global::go.net_package.BuffersжReader(Ꮡbuffers));
            } else {
                (n, err) = Ꮡbuffers.WriteTo(new net_test_package.net_TCPConnжWriter(c));
            }
            if (err != default!) {
                return err;
            }
            if (len(buffers) != 0) {
                return fmt.Errorf("len(buffers) = %d; want 0"u8, len(buffers));
            }
            if (n != (int64)Ꮡwant.Value.Len()) {
                return fmt.Errorf("Buffers.WriteTo returned %d; want %d"u8, n, Ꮡwant.Value.Len());
            }
            return default!;
        }, error (ж<global::go.net_package.TCPConn> c) => {
            var (all, err) = Δio.ReadAll(new net_test_package.net_TCPConnжReader(c));
            if (!bytes.Equal(all, Ꮡwant.Value.Bytes()) || err != default!) {
                return fmt.Errorf("client read %q, %v; want %q, nil"u8, all, err, Ꮡwant.Value.Bytes());
            }
            ᏑwriteLog.of(testBuffer_writeTo_writeLog.ᏑMutex).Lock(); // no need to unlock
            nint gotSum = default!;
            foreach (var (_, v) in ᏑwriteLog.Value.log) {
                gotSum += v;
            }
            nint wantSum = default!;
            var exprᴛ1 = Δruntime.GOOS;
            if (exprᴛ1 == "aix"u8 || exprᴛ1 == "android"u8 || exprᴛ1 == "darwin"u8 || exprᴛ1 == "ios"u8 || exprᴛ1 == "dragonfly"u8 || exprᴛ1 == "freebsd"u8 || exprᴛ1 == "illumos"u8 || exprᴛ1 == "linux"u8 || exprᴛ1 == "netbsd"u8 || exprᴛ1 == "openbsd"u8 || exprᴛ1 == "solaris"u8) {
                nint wantMinCalls = default!;
                wantSum = Ꮡwant.Value.Len();
                nint v = chunks;
                while (v > 0) {
                    wantMinCalls++;
                    v -= 1024;
                }
                if (len(ᏑwriteLog.Value.log) < wantMinCalls) {
                    Ꮡt.Errorf("write calls = %v < wanted min %v"u8, len(ᏑwriteLog.Value.log), wantMinCalls);
                }
            }
            else if (exprᴛ1 == "windows"u8) {
                nint wantCalls = default!;
                wantSum = Ꮡwant.Value.Len();
                if (wantSum > 0) {
                    wantCalls = 1; // windows will always do 1 syscall, unless sending empty buffer
                }
                if (len(ᏑwriteLog.Value.log) != wantCalls) {
                    Ꮡt.Errorf("write calls = %v; want %v"u8, len(ᏑwriteLog.Value.log), wantCalls);
                }
            }

            if (gotSum != wantSum) {
                Ꮡt.Errorf("writev call sum  = %v; want %v"u8, gotSum, wantSum);
            }
            return default!;
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noServerSideConnectionˢ = (@string)"no server side connection"u8;
internal static readonly object buffersWriteToClosedConnˢ = (@string)"Buffers.WriteTo(closed conn) succeeded, want error"u8;

public static partial void TestWritevError(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (Δruntime.GOOS == "windows"u8) {
            Ꮡt.Skipf("skipping the test: windows does not have problem sending large chunks of data"u8);
        }
        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var ch = new channel<global::go.net_package.Conn>(1);
        var chʗ1 = ch;
        var lnʗ1 = ln;
        defer(() => {
            lnʗ1.Close();
            foreach (var c in chʗ1) {
                c.Close();
            }
        }, ref ᒐ);
        var chʗ2 = ch;
        var lnʗ2 = ln;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => builtin.close(ᴛ1), chʗ2, ref ᒐ);
                var (c, errΔ1) = lnʗ2.Accept();
                if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                    return;
                }
                chʗ2.ᐸꟷ(c);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var (c1, err) = Dial(tcpˢ, ln.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var c1ʗ1 = c1;
        defer(() => c1ʗ1.Close(), ref ᒐ);
        var c2 = ᐸꟷ(ch);
        if (c2 == default!) {
            Ꮡt.Fatal(noServerSideConnectionˢ);
        }
        c2.Close();
        // 1 GB of data should be enough to notice the connection is gone.
        // Just a few bytes is not enough.
        // Arrange to reuse the same 1 MB buffer so that we don't allocate much.
        var buf = new slice<byte>((1 << (int)(20)));
        ref var buffers = ref heap<global::go.net_package.Buffers>(out var Ꮡbuffers);
        buffers = new global::go.net_package.Buffers((1 << (int)(10)));
        foreach (var (i, _) in buffers) {
            buffers[i] = buf;
        }
        {
            var (_, errΔ2) = Ꮡbuffers.WriteTo(new net_test_package.net_ConnᴠWriter(c1)); if (errΔ2 == default!) {
                Ꮡt.Fatal(buffersWriteToClosedConnˢ);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end net_internal_test_package
