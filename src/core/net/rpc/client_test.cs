// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using errors = errors_package;
using fmt = fmt_package;
using net = net_package;
using strings = strings_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static global::go.net.rpc_package;

partial class rpc_internal_test_package {

[GoType] internal partial struct shutdownCodec {
    internal channel<nint> responded;
    internal bool closed;
}

[GoRecv] internal static error WriteRequest(this ref shutdownCodec c, ж<global::go.net.rpc_package.Request> _Δp1, any _Δp2) {
    return default!;
}

[GoRecv] internal static error ReadResponseBody(this ref shutdownCodec c, any _) {
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string shutdownCodecˢ = "shutdownCodec ReadResponseHeader"u8;

[GoRecv] internal static error ReadResponseHeader(this ref shutdownCodec c, ж<global::go.net.rpc_package.Response> _) {
    c.responded.ᐸꟷ(1);
    return errors.New(shutdownCodecˢ);
}

[GoRecv] internal static error Close(this ref shutdownCodec c) {
    c.closed = true;
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object clientCloseDidNotCloseˢ = (@string)"client.Close did not close codec"u8;

public static void TestCloseCodec(ж<testing.T> Ꮡt) {
    var codec = Ꮡ(new shutdownCodec(responded: new channel<nint>(0)));
    var client = NewClientWithCodec(new rpc_internal_test_package.shutdownCodecжClientCodec(codec));
    ᐸꟷ((~codec).responded);
    client.Close();
    if (!(~codec).closed) {
        Ꮡt.Error(clientCloseDidNotCloseˢ);
    }
}

// Test that errors in gob shut down the connection. Issue 7689.
[GoType] public partial struct R {
    internal slice<byte> msg; // Not exported, so R does not work with gob.
}

[GoType] public partial struct S {
}

[GoRecv] public static error Recv(this ref S s, ж<EmptyStruct> Ꮡnul, ж<R> Ꮡreply) {
    ref var reply = ref Ꮡreply.DerefOrNull();

    reply = new R(slice<byte>("foo"u8));
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noErrorˢ = (@string)"no error"u8;
internal static readonly @string readingBodyUnexpectedEofˢ = "reading body unexpected EOF"u8;
internal static readonly object expectedReadingBodyˢ = (@string)"expected `reading body unexpected EOF', got"u8;
internal static readonly @string tcpˢ = "tcp"u8;
internal static readonly @string sRecvˢ = "S.Recv"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestGobError(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var errΔ1 = recover();
            if (errΔ1 == default!) {
                Ꮡt.Fatal(noErrorˢ);
            }
            if (!strings.Contains(errΔ1._<error>().Error(), readingBodyUnexpectedEofˢ)) {
                Ꮡt.Fatal(expectedReadingBodyˢ, errΔ1);
            }
        }, ref ᒐ);
        Register(@new<S>());
        var (listen, err) = net.Listen(tcpˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            throw panic(err);
        }
        goǃ(Accept, listen);
        (var client, err) = Dial(tcpˢ, listen.Addr().String());
        if (err != default!) {
            throw panic(err);
        }
        ref var reply = ref heap(new Reply(), out var Ꮡreply);
        err = client.Call(sRecvˢ, Ꮡ(new EmptyStruct()), Ꮡreply);
        if (err != default!) {
            throw panic(err);
        }
        fmt.Printf("%#v\n"u8, reply);
        client.Close();
        listen.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end rpc_internal_test_package
