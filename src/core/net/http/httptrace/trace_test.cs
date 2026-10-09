// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.http;

using context = context_package;
using strings = strings_package;
using testing = testing_package;
using static go.net.http.httptrace_package;

partial class httptrace_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string netˢ = "net"u8;
internal static readonly @string addrˢ = "addr"u8;

public static void TestWithClientTrace(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    Action<@string, @string> connectStart(byte b) => (@string network, @string addr) => {
            Ꮡbuf.WriteByte(b);
        };
    var ctx = context.Background();
    var oldtrace = Ꮡ(new ClientTrace(
        ConnectStart: connectStart((rune)'O')
    ));
    ctx = WithClientTrace(ctx, oldtrace);
    var newtrace = Ꮡ(new ClientTrace(
        ConnectStart: connectStart((rune)'N')
    ));
    ctx = WithClientTrace(ctx, newtrace);
    var trace = ContextClientTrace(ctx);
    buf.Reset();
    (~trace).ConnectStart(netˢ, addrˢ);
    {
        @string got = buf.String();
        @string want = "NO"u8; if (got != want) {
            Ꮡt.Errorf("got %q; want %q"u8, got, want);
        }
    }
}

internal partial struct TestCompose_tests /*dyn*/ {
    internal ж<global::go.net.http.httptrace_package.ClientTrace> trace, old;
    internal @string want;
}

public static void TestCompose(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    nint testNum = default!;
    Action<@string, @string> connectStart(byte b) => (@string network, @string addr) => {
            if (addr != "addr"u8) {
                Ꮡt.Errorf(@"%d. args for %q case = %q, %q; want addr of ""addr"""u8, testNum, b, network, addr);
            }
            Ꮡbuf.WriteByte(b);
        };
    var tests = new array<TestCompose_tests>(3){
        [0] = new(
            want: "T"u8,
            trace: Ꮡ(new ClientTrace(
                ConnectStart: connectStart((rune)'T')
            ))
        ),
        [1] = new(
            want: "TO"u8,
            trace: Ꮡ(new ClientTrace(
                ConnectStart: connectStart((rune)'T')
            )),
            old: Ꮡ(new ClientTrace(ConnectStart: connectStart((rune)'O')))
        ),
        [2] = new(
            want: "O"u8,
            trace: Ꮡ(new ClientTrace(nil)),
            old: Ꮡ(new ClientTrace(ConnectStart: connectStart((rune)'O')))
        )
    };
    foreach (var (i, tt) in tests.ΔRangeSnapshot()) {
        testNum = i;
        buf.Reset();
        ref var tr = ref heap<global::go.net.http.httptrace_package.ClientTrace>(out var Ꮡtr);
        tr = tt.trace.Value;
        Ꮡtr.compose(tt.old);
        if (tr.ConnectStart != default!) {
            tr.ConnectStart(netˢ, addrˢ);
        }
        {
            @string got = buf.String(); if (got != tt.want) {
                Ꮡt.Errorf("%d. got = %q; want %q"u8, i, got, tt.want);
            }
        }
    }
}

} // end httptrace_internal_test_package
