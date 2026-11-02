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
internal static ref readonly @string netˢ => ref netˢᵇ;
private static readonly @string netˢᵇ = "net"u8;
internal static ref readonly @string addrˢ => ref addrˢᵇ;
private static readonly @string addrˢᵇ = "addr"u8;

public static void TestWithClientTrace(ж<testing.T> t) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    Action<@string, @string> connectStart(byte b) => (@string network, @string addr) => {
            Ꮡbuf.WriteByte(b);
        };

    var ctx = context.Background();
    var oldtrace = Ꮡ(new ClientTrace(
        ConnectStart: connectStart((byte)'O')
    ));
    ctx = WithClientTrace(ctx, oldtrace);
    var newtrace = Ꮡ(new ClientTrace(
        ConnectStart: connectStart((byte)'N')
    ));
    ctx = WithClientTrace(ctx, newtrace);
    var trace = ContextClientTrace(ctx);

    buf.Reset();
    trace.ConnectStart(netˢ, addrˢ);
    {
        @string got = buf.String();
        @string want = "NO"; if (got != want) {
            t.Errorf("got %q; want %q", got, want);
        }
    }
}

internal partial struct TestCompose_tests /*dyn*/ {
    internal ж<global::go.net.http.httptrace_package.ClientTrace> trace, old;
    internal @string want;
}

public static void TestCompose(ж<testing.T> t) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    nint testNum = default;

    Action<@string, @string> connectStart(byte b) => (@string network, @string addr) => {
            if (addr != "addr"u8) {
                t.Errorf(@"%d. args for %q case = %q, %q; want addr of ""addr""", testNum, b, network, addr);
            }
            Ꮡbuf.WriteByte(b);
        };

    var tests = new array<TestCompose_tests>(3){
        [0] = new(
            want: "T",
            trace: Ꮡ(new ClientTrace(
                ConnectStart: connectStart((byte)'T')
            ))
        ),
        [1] = new(
            want: "TO",
            trace: Ꮡ(new ClientTrace(
                ConnectStart: connectStart((byte)'T')
            )),
            old: Ꮡ(new ClientTrace(ConnectStart: connectStart((byte)'O')))
        ),
        [2] = new(
            want: "O",
            trace: Ꮡ(new ClientTrace()),
            old: Ꮡ(new ClientTrace(ConnectStart: connectStart((byte)'O')))
        )
    };
    foreach (var (i, tt) in tests.ΔRangeSnapshot()) {
        testNum = i;
        buf.Reset();

        ref var tr = ref heap<global::go.net.http.httptrace_package.ClientTrace>(tt.trace.Value, out var Ꮡtr);
        Ꮡtr.compose(tt.old);
        if (tr.ConnectStart != nil) {
            tr.ConnectStart(netˢ, addrˢ);
        }
        {
            @string got = buf.String(); if (got != tt.want) {
                t.Errorf("%d. got = %q; want %q", i, got, tt.want);
            }
        }
    }
}

} // end httptrace_internal_test_package
