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

[GoType] public partial struct Args {
    public nint A, B;
}

[GoType] public partial struct Reply {
    public nint C;
}

[GoType("num:nint")] public partial struct Arith;

[GoType] public partial struct ArithAddResp {
    [GoTag(@"json:""id""")]
    public any Id;
    [GoTag(@"json:""result""")]
    public Reply Result;
    [GoTag(@"json:""error""")]
    public any Error;
}

[GoRecv] public static error Add(this ref Arith t, ж<Args> Ꮡargs, ж<Reply> Ꮡreply) {
    ref var args = ref Ꮡargs.DerefOrNull();
    ref var reply = ref Ꮡreply.DerefOrNull();

    reply.C = args.A + args.B;
    return default!;
}

[GoRecv] public static error Mul(this ref Arith t, ж<Args> Ꮡargs, ж<Reply> Ꮡreply) {
    ref var args = ref Ꮡargs.DerefOrNull();
    ref var reply = ref Ꮡreply.DerefOrNull();

    reply.C = args.A * args.B;
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string divideByZeroˢ = "divide by zero"u8;

[GoRecv] public static error Div(this ref Arith t, ж<Args> Ꮡargs, ж<Reply> Ꮡreply) {
    ref var args = ref Ꮡargs.DerefOrNull();
    ref var reply = ref Ꮡreply.DerefOrNull();

    if (args.B == 0) {
        return errors.New(divideByZeroˢ);
    }
    reply.C = args.A / args.B;
    return default!;
}

[GoRecv] public static error Error(this ref Arith t, ж<Args> Ꮡargs, ж<Reply> Ꮡreply) {
    throw panic("ERROR");
}

[GoType] public partial struct BuiltinTypes {
}

public static error Map(this BuiltinTypes _, nint i, ж<map<nint, nint>> Ꮡreply) {
    ref var reply = ref Ꮡreply.DerefOrNull();

    (reply)[i] = i;
    return default!;
}

public static error Slice(this BuiltinTypes _, nint i, ж<slice<nint>> Ꮡreply) {
    ref var reply = ref Ꮡreply.DerefOrNull();

    reply = append(reply, i);
    return default!;
}

public static error Array(this BuiltinTypes _, nint i, [GoArrayDims(1)] ж<array<nint>> Ꮡreply) {
    ref var reply = ref Ꮡreply.DerefOrNull();

    (reply)[0] = i;
    return default!;
}

[GoInit] internal static void init() {
    rpc.Register(@new<Arith>());
    rpc.Register(new BuiltinTypes(nil));
}

public static void TestServerNoParams(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (cli, srv) = net.Pipe();
        var cliʗ1 = cli;
        defer(() => cliʗ1.Close(), ref ᒐ);
        goǃ(ServeConn, new jsonrpc_internal_test_package.net_ConnᴠReadWriteCloser(srv));
        var dec = json.NewDecoder(new jsonrpc_internal_test_package.net_ConnᴠReader(cli));
        fmt.Fprintf(new jsonrpc_internal_test_package.net_ConnᴠWriter(cli), @"{""method"": ""Arith.Add"", ""id"": ""123""}"u8);
        ref var resp = ref heap(new ArithAddResp(), out var Ꮡresp);
        {
            var err = dec.Decode(Ꮡresp); if (err != default!) {
                Ꮡt.Fatalf("Decode after no params: %s"u8, err);
            }
        }
        if (resp.Error == default!) {
            Ꮡt.Fatalf("Expected error, got nil"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestServerEmptyMessage(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (cli, srv) = net.Pipe();
        var cliʗ1 = cli;
        defer(() => cliʗ1.Close(), ref ᒐ);
        goǃ(ServeConn, new jsonrpc_internal_test_package.net_ConnᴠReadWriteCloser(srv));
        var dec = json.NewDecoder(new jsonrpc_internal_test_package.net_ConnᴠReader(cli));
        fmt.Fprintf(new jsonrpc_internal_test_package.net_ConnᴠWriter(cli), "{}"u8);
        ref var resp = ref heap(new ArithAddResp(), out var Ꮡresp);
        {
            var err = dec.Decode(Ꮡresp); if (err != default!) {
                Ꮡt.Fatalf("Decode after empty: %s"u8, err);
            }
        }
        if (resp.Error == default!) {
            Ꮡt.Fatalf("Expected error, got nil"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestServer(ж<testing.T> Ꮡt) {
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
            if (err != default!) {
                Ꮡt.Fatalf("Decode: %s"u8, err);
            }
            if (resp.Error != default!) {
                Ꮡt.Fatalf("resp.Error: %s"u8, resp.Error);
            }
            if (resp.Id._<@string>() != ((@string)(rune)i)) {
                Ꮡt.Fatalf("resp: bad id %q want %q"u8, resp.Id._<@string>(), ((@string)(rune)i));
            }
            if (resp.Result.C != 2 * i + 1) {
                Ꮡt.Fatalf("resp: bad result: %d+%d=%d"u8, i, i + 1, resp.Result.C);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string arithAddˢ = "Arith.Add"u8;
internal static readonly @string arithMulˢ = "Arith.Mul"u8;
internal static readonly @string arithDivˢ = "Arith.Div"u8;
internal static readonly object divExpectedErrorˢ = (@string)"Div: expected error"u8;
internal static readonly object divExpectedDivideByZeroˢ = (@string)"Div: expected divide by zero error; got"u8;

public static void TestClient(ж<testing.T> Ꮡt) {
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
        if (err != default!) {
            Ꮡt.Errorf("Add: expected no error but got string %q"u8, err.Error());
        }
        if ((~reply).C != (~args).A + (~args).B) {
            Ꮡt.Errorf("Add: got %d expected %d"u8, (~reply).C, (~args).A + (~args).B);
        }
        args = Ꮡ(new Args(7, 8));
        reply = @new<Reply>();
        err = client.Call(arithMulˢ, args.OrTypedNil(), reply.OrTypedNil());
        if (err != default!) {
            Ꮡt.Errorf("Mul: expected no error but got string %q"u8, err.Error());
        }
        if ((~reply).C != (~args).A * (~args).B) {
            Ꮡt.Errorf("Mul: got %d expected %d"u8, (~reply).C, (~args).A * (~args).B);
        }
        // Out of order.
        args = Ꮡ(new Args(7, 8));
        var mulReply = @new<Reply>();
        var mulCall = client.Go(arithMulˢ, args.OrTypedNil(), mulReply.OrTypedNil(), default!);
        var addReply = @new<Reply>();
        var addCall = client.Go(arithAddˢ, args.OrTypedNil(), addReply.OrTypedNil(), default!);
        addCall = ᐸꟷ((~addCall).Done);
        if ((~addCall).Error != default!) {
            Ꮡt.Errorf("Add: expected no error but got string %q"u8, (~addCall).Error.Error());
        }
        if ((~addReply).C != (~args).A + (~args).B) {
            Ꮡt.Errorf("Add: got %d expected %d"u8, (~addReply).C, (~args).A + (~args).B);
        }
        mulCall = ᐸꟷ((~mulCall).Done);
        if ((~mulCall).Error != default!) {
            Ꮡt.Errorf("Mul: expected no error but got string %q"u8, (~mulCall).Error.Error());
        }
        if ((~mulReply).C != (~args).A * (~args).B) {
            Ꮡt.Errorf("Mul: got %d expected %d"u8, (~mulReply).C, (~args).A * (~args).B);
        }
        // Error test
        args = Ꮡ(new Args(7, 0));
        reply = @new<Reply>();
        err = client.Call(arithDivˢ, args.OrTypedNil(), reply.OrTypedNil());
        // expect an error: zero divide
        if (err == default!){
            Ꮡt.Error(divExpectedErrorˢ);
        } else 
        if (err.Error() != "divide by zero"u8) {
            Ꮡt.Error(divExpectedDivideByZeroˢ, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string builtinTypesMapˢ = "BuiltinTypes.Map"u8;
internal static readonly @string builtinTypesSliceˢ = "BuiltinTypes.Slice"u8;
internal static readonly @string builtinTypesArrayˢ = "BuiltinTypes.Array"u8;

public static void TestBuiltinTypes(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (cli, srv) = net.Pipe();
        goǃ(ServeConn, new jsonrpc_internal_test_package.net_ConnᴠReadWriteCloser(srv));
        var client = NewClient(new jsonrpc_internal_test_package.net_ConnᴠReadWriteCloser(cli));
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);
        // Map
        nint arg = 7;
        ref var replyMap = ref heap<map<nint, nint>>(out var ᏑreplyMap);
        replyMap = new map<nint, nint>{};
        var err = client.Call(builtinTypesMapˢ, arg, ᏑreplyMap);
        if (err != default!) {
            Ꮡt.Errorf("Map: expected no error but got string %q"u8, err.Error());
        }
        if (replyMap[arg] != arg) {
            Ꮡt.Errorf("Map: expected %d got %d"u8, arg, replyMap[arg]);
        }
        // Slice
        ref var replySlice = ref heap<slice<nint>>(out var ᏑreplySlice);
        replySlice = new nint[]{}.slice();
        err = client.Call(builtinTypesSliceˢ, arg, ᏑreplySlice);
        if (err != default!) {
            Ꮡt.Errorf("Slice: expected no error but got string %q"u8, err.Error());
        }
        {
            var e = new nint[]{arg}.slice(); if (!reflect.DeepEqual(replySlice, e)) {
                Ꮡt.Errorf("Slice: expected %v got %v"u8, e, replySlice);
            }
        }
        // Array
        ref var replyArray = ref heap<array<nint>>(out var ᏑreplyArray);
        replyArray = new nint[]{}.array(1);
        err = client.Call(builtinTypesArrayˢ, arg, ᏑreplyArray);
        if (err != default!) {
            Ꮡt.Errorf("Array: expected no error but got string %q"u8, err.Error());
        }
        {
            var e = new nint[]{arg}.array(); if (!reflect.DeepEqual(replyArray, e)) {
                Ꮡt.Errorf("Array: expected %v got %v"u8, e, replyArray);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestMalformedInput(ж<testing.T> Ꮡt) {
    var (cli, srv) = net.Pipe();
    var cliʗ1 = cli;
    goǃ(ᴛ1 => cliʗ1.Write(ᴛ1), slice<byte>(@"{id:1}"u8)); // invalid json
    ServeConn(new jsonrpc_internal_test_package.net_ConnᴠReadWriteCloser(srv)); // must return, not loop
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorˢ = (@string)"expected error"u8;

public static void TestMalformedOutput(ж<testing.T> Ꮡt) {
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
        if (err == default!) {
            Ꮡt.Error(expectedErrorˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string methodArithAddId123ˢ = @"{""method"": ""Arith.Add"", ""id"": ""123"", ""params"": []}"u8;

[GoType("dyn")] internal partial struct TestServerErrorHasNullResult_conn {
    [GoEmbedded] public io_package.Reader Reader;
    [GoEmbedded] public io_package.Writer Writer;
    [GoEmbedded] public io_package.Closer Closer;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string valueTextᶜ = "the value we don't want to see"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string errorTextᶜ = "some error"u8;

public static void TestServerErrorHasNullResult(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var @out = ref heap(new strings.Builder(), out var Ꮡout);
    var sc = NewServerCodec(new TestServerErrorHasNullResult_conn(
        Reader: new jsonrpc_internal_test_package.strings_ReaderжReader(strings.NewReader(methodArithAddId123ˢ)),
        Writer: new jsonrpc_internal_test_package.strings_BuilderжWriter(Ꮡout),
        Closer: new jsonrpc_internal_test_package.io_ReadCloserᴠCloser(io.NopCloser(default!))
    ));
    var r = @new<rpc.Request>();
    {
        var errΔ1 = sc.ReadRequestHeader(r); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    @string valueText = valueTextᶜ;
    @string errorText = errorTextᶜ;
    var err = sc.WriteResponse(Ꮡ(new rpc.Response(
        ServiceMethod: "Method"u8,
        Seq: 1,
        Error: errorText
    )), valueText);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!strings.Contains(@out.String(), errorText)) {
        Ꮡt.Fatalf("Response didn't contain expected error %q: %s"u8, errorText, Ꮡout);
    }
    if (strings.Contains(@out.String(), valueText)) {
        Ꮡt.Errorf("Response contains both an error and value: %s"u8, Ꮡout);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unexpectedErrorˢ = "unexpected error!"u8;

public static void TestUnexpectedError(ж<testing.T> Ꮡt) {
    var (cli, srv) = myPipe();
    var cliʗ1 = cli;
    goǃ(ᴛ1 => (~cliʗ1).PipeWriter.CloseWithError(ᴛ1), errors.New(unexpectedErrorˢ)); // reader will get this error
    ServeConn(new jsonrpc_internal_test_package.pipeжReadWriteCloser(srv)); // must return, not loop
}

// Copied from package net.
internal static (ж<pipe>, ж<pipe>) myPipe() {
    var (r1, w1) = io.Pipe();
    var (r2, w2) = io.Pipe();
    return (Ꮡ(new pipe(r1, w2)), Ꮡ(new pipe(r2, w1)));
}

[GoType] internal partial struct pipe {
    public partial ref ж<io_package.PipeReader> PipeReader { get; }
    public partial ref ж<io_package.PipeWriter> PipeWriter { get; }
}

[GoType("num:nint")] internal partial struct pipeAddr;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pipeˢ = "pipe"u8;

internal static @string Network(this pipeAddr _) {
    return pipeˢ;
}

internal static @string String(this pipeAddr _) {
    return pipeˢ;
}

[GoRecv] internal static error Close(this ref pipe p) {
    var err = p.PipeReader.Close();
    var err1 = p.PipeWriter.Close();
    if (err == default!) {
        err = err1;
    }
    return err;
}

[GoRecv] internal static netꓸAddr LocalAddr(this ref pipe p) {
    return ((pipeAddr)0);
}

[GoRecv] internal static netꓸAddr RemoteAddr(this ref pipe p) {
    return ((pipeAddr)0);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string netPipeDoesNotSupportˢ = "net.Pipe does not support timeouts"u8;

[GoRecv] internal static error SetTimeout(this ref pipe p, int64 nsec) {
    return errors.New(netPipeDoesNotSupportˢ);
}

[GoRecv] internal static error SetReadTimeout(this ref pipe p, int64 nsec) {
    return errors.New(netPipeDoesNotSupportˢ);
}

[GoRecv] internal static error SetWriteTimeout(this ref pipe p, int64 nsec) {
    return errors.New(netPipeDoesNotSupportˢ);
}

} // end jsonrpc_internal_test_package
