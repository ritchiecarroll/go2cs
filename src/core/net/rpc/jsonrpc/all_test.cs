// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.rpc;

using json = encoding.json_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using net = net_package;
using rpc = global::go.net.rpc_package;
using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using encoding;
using global::go.net;
using static global::go.net.rpc.jsonrpc_package;

partial class jsonrpc_internal_test_package {

public partial struct Args {
    public nint A, B;
}

public partial struct Reply {
    public nint C;
}

public partial struct Arith /*num:nint*/;

public partial struct ArithAddResp {
    public any Id; /*`json:"id"`*/
    public Reply Result; /*`json:"result"`*/
    public any Error; /*`json:"error"`*/
}

public static error Add(this ref Arith t, ж<Args> Ꮡargs, ж<Reply> Ꮡreply) {
    ref var args = ref Ꮡargs.DerefOrNull();
    ref var reply = ref Ꮡreply.DerefOrNull();

    reply.C = args.A + args.B;
    return default;
}

public static error Mul(this ref Arith t, ж<Args> Ꮡargs, ж<Reply> Ꮡreply) {
    ref var args = ref Ꮡargs.DerefOrNull();
    ref var reply = ref Ꮡreply.DerefOrNull();

    reply.C = args.A * args.B;
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string divideByZeroˢ => ref divideByZeroˢᵇ;
private static readonly @string divideByZeroˢᵇ = "divide by zero"u8;

public static error Div(this ref Arith t, ж<Args> Ꮡargs, ж<Reply> Ꮡreply) {
    ref var args = ref Ꮡargs.DerefOrNull();
    ref var reply = ref Ꮡreply.DerefOrNull();

    if (args.B == 0) {
        return errors.New(divideByZeroˢ);
    }
    reply.C = quo(args.A, args.B);
    return default;
}

public static error Error(this ref Arith t, ж<Args> args, ж<Reply> reply) {
    throw panic("ERROR");
}

public partial struct BuiltinTypes {
}

public static error Map(this BuiltinTypes _, nint i, ж<map<nint, nint>> Ꮡreply) {
    ref var reply = ref Ꮡreply.DerefOrNull();

    (reply)[i] = i;
    return default;
}

public static error Slice(this BuiltinTypes _, nint i, ж<slice<nint>> Ꮡreply) {
    ref var reply = ref Ꮡreply.DerefOrNull();

    reply = append(reply, i);
    return default;
}

public static error Array(this BuiltinTypes _, nint i, /*[1]*/ ж<array<nint>> Ꮡreply) {
    ref var reply = ref Ꮡreply.DerefOrNull();

    (reply)[0] = i;
    return default;
}

[GoInit] internal static void init() {
    rpc.Register(@new<Arith>());
    rpc.Register(new BuiltinTypes());
}

public static partial void TestServerNoParams(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var (cli, srv) = net.Pipe();
        var cliʗ1 = cli;
        defer(() => cliʗ1.Close(), ref ᒐ);
        goǃ(ServeConn, new jsonrpc_internal_test_package.net_ConnᴠReadWriteCloser(srv));
        var dec = json.NewDecoder(new jsonrpc_internal_test_package.net_ConnᴠReader(cli));

        fmt.Fprintf(new jsonrpc_internal_test_package.net_ConnᴠWriter(cli), @"{""method"": ""Arith.Add"", ""id"": ""123""}"u8);
        ref var resp = ref heap(new ArithAddResp(), out var Ꮡresp);
        var err = dec.Decode(Ꮡresp); if (err != nil) {
            t.Fatalf("Decode after no params: %s", err);
        }
        if (resp.Error == nil) {
            t.Fatalf("Expected error, got nil");
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial void TestServerEmptyMessage(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var (cli, srv) = net.Pipe();
        var cliʗ1 = cli;
        defer(() => cliʗ1.Close(), ref ᒐ);
        goǃ(ServeConn, new jsonrpc_internal_test_package.net_ConnᴠReadWriteCloser(srv));
        var dec = json.NewDecoder(new jsonrpc_internal_test_package.net_ConnᴠReader(cli));

        fmt.Fprintf(new jsonrpc_internal_test_package.net_ConnᴠWriter(cli), "{}"u8);
        ref var resp = ref heap(new ArithAddResp(), out var Ꮡresp);
        var err = dec.Decode(Ꮡresp); if (err != nil) {
            t.Fatalf("Decode after empty: %s", err);
        }
        if (resp.Error == nil) {
            t.Fatalf("Expected error, got nil");
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial void TestServer(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var (cli, srv) = net.Pipe();
        var cliʗ1 = cli;
        defer(() => cliʗ1.Close(), ref ᒐ);
        goǃ(ServeConn, new jsonrpc_internal_test_package.net_ConnᴠReadWriteCloser(srv));
        var dec = json.NewDecoder(new jsonrpc_internal_test_package.net_ConnᴠReader(cli));

        // Send hand-coded requests to server, parse responses.
        for (nint i = 0; i < 10; i++) {
            fmt.Fprintf(new jsonrpc_internal_test_package.net_ConnᴠWriter(cli), @"{""method"": ""Arith.Add"", ""id"": ""\u%04d"", ""params"": [{""A"": %d, ""B"": %d}]}"u8, i, i, i + 1);
            ref var resp = ref heap(new ArithAddResp(), out var Ꮡresp);
            var err = dec.Decode(Ꮡresp);
            if (err != nil) {
                t.Fatalf("Decode: %s", err);
            }
            if (resp.Error != nil) {
                t.Fatalf("resp.Error: %s", resp.Error);
            }
            if (resp.Id._<@string>() != ((@string)(rune)i)) {
                t.Fatalf("resp: bad id %q want %q", resp.Id._<@string>(), (@string)(rune)i);
            }
            if (resp.Result.C != 2 * i + 1) {
                t.Fatalf("resp: bad result: %d+%d=%d", i, i + 1, resp.Result.C);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string arithAddˢ => ref arithAddˢᵇ;
private static readonly @string arithAddˢᵇ = "Arith.Add"u8;
internal static ref readonly @string arithMulˢ => ref arithMulˢᵇ;
private static readonly @string arithMulˢᵇ = "Arith.Mul"u8;
internal static ref readonly @string arithDivˢ => ref arithDivˢᵇ;
private static readonly @string arithDivˢᵇ = "Arith.Div"u8;
internal static readonly object divExpectedErrorˢ = (@string)"Div: expected error"u8;
internal static readonly object divExpectedDivideByZeroˢ = (@string)"Div: expected divide by zero error; got"u8;

public static partial void TestClient(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // Assume server is okay (TestServer is above).
        // Test client against server.
        var (cli, srv) = net.Pipe();
        goǃ(ServeConn, new jsonrpc_internal_test_package.net_ConnᴠReadWriteCloser(srv));

        var client = NewClient(new jsonrpc_internal_test_package.net_ConnᴠReadWriteCloser(cli));
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);

        // Synchronous calls
        var args = Ꮡ(new Args(7, 8));
        var reply = @new<Reply>();
        var err = client.Call(arithAddˢ, args.OrTypedNil(), reply.OrTypedNil());
        if (err != nil) {
            t.Errorf("Add: expected no error but got string %q", err.Error());
        }
        if (reply.C != args.A + args.B) {
            t.Errorf("Add: got %d expected %d", reply.C, args.A + args.B);
        }

        args = Ꮡ(new Args(7, 8));
        reply = @new<Reply>();
        err = client.Call(arithMulˢ, args.OrTypedNil(), reply.OrTypedNil());
        if (err != nil) {
            t.Errorf("Mul: expected no error but got string %q", err.Error());
        }
        if (reply.C != args.A * args.B) {
            t.Errorf("Mul: got %d expected %d", reply.C, args.A * args.B);
        }

        // Out of order.
        args = Ꮡ(new Args(7, 8));
        var mulReply = @new<Reply>();
        var mulCall = client.Go(arithMulˢ, args.OrTypedNil(), mulReply.OrTypedNil(), default);
        var addReply = @new<Reply>();
        var addCall = client.Go(arithAddˢ, args.OrTypedNil(), addReply.OrTypedNil(), default);

        addCall = ᐸꟷ(addCall.Done);
        if (addCall.Error != nil) {
            t.Errorf("Add: expected no error but got string %q", addCall.Error.Error());
        }
        if (addReply.C != args.A + args.B) {
            t.Errorf("Add: got %d expected %d", addReply.C, args.A + args.B);
        }

        mulCall = ᐸꟷ(mulCall.Done);
        if (mulCall.Error != nil) {
            t.Errorf("Mul: expected no error but got string %q", mulCall.Error.Error());
        }
        if (mulReply.C != args.A * args.B) {
            t.Errorf("Mul: got %d expected %d", mulReply.C, args.A * args.B);
        }

        // Error test
        args = Ꮡ(new Args(7, 0));
        reply = @new<Reply>();
        err = client.Call(arithDivˢ, args.OrTypedNil(), reply.OrTypedNil());
        // expect an error: zero divide
        if (err == nil) {
            t.Error(divExpectedErrorˢ);
        } else if (err.Error() != "divide by zero"u8) {
            t.Error(divExpectedDivideByZeroˢ, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string builtinTypesMapˢ => ref builtinTypesMapˢᵇ;
private static readonly @string builtinTypesMapˢᵇ = "BuiltinTypes.Map"u8;
internal static ref readonly @string builtinTypesSliceˢ => ref builtinTypesSliceˢᵇ;
private static readonly @string builtinTypesSliceˢᵇ = "BuiltinTypes.Slice"u8;
internal static ref readonly @string builtinTypesArrayˢ => ref builtinTypesArrayˢᵇ;
private static readonly @string builtinTypesArrayˢᵇ = "BuiltinTypes.Array"u8;

public static partial void TestBuiltinTypes(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var (cli, srv) = net.Pipe();
        goǃ(ServeConn, new jsonrpc_internal_test_package.net_ConnᴠReadWriteCloser(srv));

        var client = NewClient(new jsonrpc_internal_test_package.net_ConnᴠReadWriteCloser(cli));
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);

        // Map
        nint arg = 7;
        ref var replyMap = ref heap<map<nint, nint>>(new map<nint, nint>{}, out var ᏑreplyMap);
        var err = client.Call(builtinTypesMapˢ, arg, ᏑreplyMap);
        if (err != nil) {
            t.Errorf("Map: expected no error but got string %q", err.Error());
        }
        if (replyMap[arg] != arg) {
            t.Errorf("Map: expected %d got %d", arg, replyMap[arg]);
        }

        // Slice
        ref var replySlice = ref heap<slice<nint>>(slice<nint>([]), out var ᏑreplySlice);
        err = client.Call(builtinTypesSliceˢ, arg, ᏑreplySlice);
        if (err != nil) {
            t.Errorf("Slice: expected no error but got string %q", err.Error());
        }
        {
            var e = slice<nint>([arg]); if (!reflect.DeepEqual(replySlice, e)) {
                t.Errorf("Slice: expected %v got %v", e, replySlice);
            }
        }

        // Array
        ref var replyArray = ref heap<array<nint>>(new nint[]{}.array(1), out var ᏑreplyArray);
        err = client.Call(builtinTypesArrayˢ, arg, ᏑreplyArray);
        if (err != nil) {
            t.Errorf("Array: expected no error but got string %q", err.Error());
        }
        {
            var e = new nint[]{arg}.array(); if (!reflect.DeepEqual(replyArray, e)) {
                t.Errorf("Array: expected %v got %v", e, replyArray);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial void TestMalformedInput(ж<testing.T> t) {
    var (cli, srv) = net.Pipe();
    var cliʗ1 = cli;
    goǃ(ᴛ1 => cliʗ1.Write(ᴛ1), slice<byte>(@"{id:1}"u8));                       // invalid json
    ServeConn(new jsonrpc_internal_test_package.net_ConnᴠReadWriteCloser(srv)); // must return, not loop
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorˢ = (@string)"expected error"u8;

public static partial void TestMalformedOutput(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var (cli, srv) = net.Pipe();
        var srvʗ1 = srv;
        goǃ(ᴛ1 => srvʗ1.Write(ᴛ1), slice<byte>(@"{""id"":0,""result"":null,""error"":null}"u8));
        goǃ(ᴛ1 => io.ReadAll(ᴛ1), new jsonrpc_internal_test_package.net_ConnᴠReader(srv));

        var client = NewClient(new jsonrpc_internal_test_package.net_ConnᴠReadWriteCloser(cli));
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);

        var args = Ꮡ(new Args(7, 8));
        var reply = @new<Reply>();
        var err = client.Call(arithAddˢ, args.OrTypedNil(), reply.OrTypedNil());
        if (err == nil) {
            t.Error(expectedErrorˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string methodArithAddId123ˢ => ref methodArithAddId123ˢᵇ;
private static readonly @string methodArithAddId123ˢᵇ = @"{""method"": ""Arith.Add"", ""id"": ""123"", ""params"": []}"u8;

internal partial struct TestServerErrorHasNullResult_conn /*dyn*/ {
    /*embed*/ public io_package.Reader Reader;
    /*embed*/ public io_package.Writer Writer;
    /*embed*/ public io_package.Closer Closer;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string valueTextᶜ => ref valueTextᶜᵇ;
private static readonly @string valueTextᶜᵇ = "the value we don't want to see"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string errorTextᶜ => ref errorTextᶜᵇ;
private static readonly @string errorTextᶜᵇ = "some error"u8;

public static void TestServerErrorHasNullResult(ж<testing.T> t) {
    ref var @out = ref heap(new strings.Builder(), out var Ꮡout);
    var sc = NewServerCodec(new TestServerErrorHasNullResult_conn(
        Reader: new jsonrpc_internal_test_package.strings_ReaderжReader(strings.NewReader(methodArithAddId123ˢ)),
        Writer: new jsonrpc_internal_test_package.strings_BuilderжWriter(Ꮡout),
        Closer: new jsonrpc_internal_test_package.io_ReadCloserᴠCloser(io.NopCloser(default))
    ));
    var r = @new<rpc.Request>();
    var errΔ1 = sc.ReadRequestHeader(r); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }
    @string valueText = valueTextᶜ;
    @string errorText = errorTextᶜ;
    var err = sc.WriteResponse(Ꮡ(new rpc.Response(
        ServiceMethod: "Method",
        Seq: 1,
        Error: errorText
    )), valueText);
    if (err != nil) {
        t.Fatal(err);
    }
    if (!strings.Contains(@out.String(), errorText)) {
        t.Fatalf("Response didn't contain expected error %q: %s", errorText, Ꮡout);
    }
    if (strings.Contains(@out.String(), valueText)) {
        t.Errorf("Response contains both an error and value: %s", Ꮡout);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unexpectedErrorˢ => ref unexpectedErrorˢᵇ;
private static readonly @string unexpectedErrorˢᵇ = "unexpected error!"u8;

public static partial void TestUnexpectedError(ж<testing.T> t) {
    var (cli, srv) = myPipe();
    var cliʗ1 = cli;
    goǃ((ᴛ0, ᴛ1) => ᴛ0.CloseWithError(ᴛ1), cliʗ1.PipeWriter, errors.New(unexpectedErrorˢ)); // reader will get this error
    ServeConn(new jsonrpc_internal_test_package.pipeжReadWriteCloser(srv));                 // must return, not loop
}

// Copied from package net.
internal static (ж<pipe>, ж<pipe>) myPipe() {
    var (r1, w1) = io.Pipe();
    var (r2, w2) = io.Pipe();

    return (Ꮡ(new pipe(r1, w2)), Ꮡ(new pipe(r2, w1)));
}

internal partial struct pipe {
    /*embed*/ public ж<io_package.PipeReader> PipeReader;
    /*embed*/ public ж<io_package.PipeWriter> PipeWriter;
}

internal partial struct pipeAddr /*num:nint*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string pipeˢ => ref pipeˢᵇ;
private static readonly @string pipeˢᵇ = "pipe"u8;

internal static @string Network(this pipeAddr _) {
    return pipeˢ;
}

internal static @string String(this pipeAddr _) {
    return pipeˢ;
}

internal static error Close(this ref pipe p) {
    var err = p.PipeReader.Close();
    var err1 = p.PipeWriter.Close();
    if (err == nil) {
        err = err1;
    }
    return err;
}

internal static netꓸAddr LocalAddr(this ref pipe p) {
    return ((pipeAddr)0);
}

internal static netꓸAddr RemoteAddr(this ref pipe p) {
    return ((pipeAddr)0);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string netPipeDoesNotSupportˢ => ref netPipeDoesNotSupportˢᵇ;
private static readonly @string netPipeDoesNotSupportˢᵇ = "net.Pipe does not support timeouts"u8;

internal static error SetTimeout(this ref pipe p, int64 nsec) {
    return errors.New(netPipeDoesNotSupportˢ);
}

internal static error SetReadTimeout(this ref pipe p, int64 nsec) {
    return errors.New(netPipeDoesNotSupportˢ);
}

internal static error SetWriteTimeout(this ref pipe p, int64 nsec) {
    return errors.New(netPipeDoesNotSupportˢ);
}

} // end jsonrpc_internal_test_package
