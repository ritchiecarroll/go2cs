// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using log = log_package;
using net = net_package;
using httptest = global::go.net.http.httptest_package;
using reflect = reflect_package;
using runtime = runtime_package;
using strings = strings_package;
using sync = sync_package;
using atomic = global::go.sync.atomic_package;
using testing = testing_package;
using time = time_package;
using global::go.net.http;
using global::go.sync;
using static global::go.net.rpc_package;
using Δhttp = global::go.net.http_package;

partial class rpc_internal_test_package {

internal static ж<global::go.net.rpc_package.Server> newServer;
internal static @string serverAddr;
internal static @string newServerAddr;
internal static @string httpServerAddr;
internal static readonly ж<sync.Once> Ꮡonce = new StaticBox<sync.Once>(default(sync.Once));
internal static ref sync.Once once => ref ((StaticBox<sync.Once>)Ꮡonce).ValueSlot;
internal static readonly ж<sync.Once> ᏑnewOnce = new StaticBox<sync.Once>(default(sync.Once));
internal static ref sync.Once newOnce => ref ((StaticBox<sync.Once>)ᏑnewOnce).ValueSlot;
internal static readonly ж<sync.Once> ᏑhttpOnce = new StaticBox<sync.Once>(default(sync.Once));
internal static ref sync.Once httpOnce => ref ((StaticBox<sync.Once>)ᏑhttpOnce).ValueSlot;

internal static ref readonly @string newHttpPath => ref newHttpPathᵇ;
private static readonly @string newHttpPathᵇ = "/foo"u8;

public partial struct Args {
    public nint A, B;
}

public partial struct Reply {
    public nint C;
}

public partial struct Arith /*num:nint*/;

// Some of Arith's methods have value args, some have pointer args. That's deliberate.
public static error Add(this ref Arith t, Args args, ж<Reply> Ꮡreply) {
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

public static error Div(this ref Arith t, Args args, ж<Reply> Ꮡreply) {
    ref var reply = ref Ꮡreply.DerefOrNull();

    if (args.B == 0) {
        return errors.New(divideByZeroˢ);
    }
    reply.C = quo(args.A, args.B);
    return default;
}

public static error String(this ref Arith t, ж<Args> Ꮡargs, ж<@string> Ꮡreply) {
    ref var args = ref Ꮡargs.DerefOrNull();
    ref var reply = ref Ꮡreply.DerefOrNull();

    reply = fmt.Sprintf("%d+%d=%d"u8, args.A, args.B, args.A + args.B);
    return default;
}

public static error /*err*/ Scan(this ref Arith t, @string args, ж<Reply> reply) {
    error err = default;

    (_, err) = fmt.Sscan(args, reply.ᏑC);
    return err;
}

public static error Error(this ref Arith t, ж<Args> args, ж<Reply> reply) {
    throw panic("ERROR");
}

public static error SleepMilli(this ref Arith t, ж<Args> Ꮡargs, ж<Reply> reply) {
    ref var args = ref Ꮡargs.DerefOrNull();

    time.Sleep(((time.Duration)(int64)args.A) * time.Millisecond);
    return default;
}

internal partial struct hidden /*num:nint*/;

internal static error Exported(this ref hidden t, Args args, ж<Reply> Ꮡreply) {
    ref var reply = ref Ꮡreply.DerefOrNull();

    reply.C = args.A + args.B;
    return default;
}

public partial struct Embed {
    /*embed*/ internal hidden hidden;
}

public partial struct BuiltinTypes {
}

public static error Map(this BuiltinTypes _, ж<Args> Ꮡargs, ж<map<nint, nint>> Ꮡreply) {
    ref var args = ref Ꮡargs.DerefOrNull();
    ref var reply = ref Ꮡreply.DerefOrNull();

    (reply)[args.A] = args.B;
    return default;
}

public static error Slice(this BuiltinTypes _, ж<Args> Ꮡargs, ж<slice<nint>> Ꮡreply) {
    ref var args = ref Ꮡargs.DerefOrNull();
    ref var reply = ref Ꮡreply.DerefOrNull();

    reply = append(reply, args.A, args.B);
    return default;
}

public static error Array(this BuiltinTypes _, ж<Args> Ꮡargs, /*[2]*/ ж<array<nint>> Ꮡreply) {
    ref var args = ref Ꮡargs.DerefOrNull();
    ref var reply = ref Ꮡreply.DerefOrNull();

    (reply)[0] = args.A;
    (reply)[1] = args.B;
    return default;
}

internal static (net.Listener, @string) listenTCP() {
    var (l, err) = net.Listen(tcpˢ, "127.0.0.1:0"); // any available address
    if (err != nil) {
        log.Fatalf("net.Listen tcp :0: %v", err);
    }
    return (l, l.Addr().String());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string netRpcArithˢ => ref netRpcArithˢᵇ;
private static readonly @string netRpcArithˢᵇ = "net.rpc.Arith"u8;
internal static readonly object testRpcServerListeningOnˢ = (@string)"Test RPC server listening on"u8;

internal static partial void startServer() {
    Register(@new<Arith>());
    Register(@new<Embed>());
    RegisterName(netRpcArithˢ, @new<Arith>());
    Register(new BuiltinTypes());

    net.Listener l = default;
    (l, serverAddr) = listenTCP();
    log.Println(testRpcServerListeningOnˢ, serverAddr);
    goǃ(Accept, l);

    HandleHTTP();
    ᏑhttpOnce.Do(startHttpServer);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string newServerArithˢ => ref newServerArithˢᵇ;
private static readonly @string newServerArithˢᵇ = "newServer.Arith"u8;
internal static readonly object newServerTestRpcServerˢ = (@string)"NewServer test RPC server listening on"u8;
internal static ref readonly @string barˢ => ref barˢᵇ;
private static readonly @string barˢᵇ = "/bar"u8;

internal static partial void startNewServer() {
    newServer = NewServer();
    newServer.Register(@new<Arith>());
    newServer.Register(@new<Embed>());
    newServer.RegisterName(netRpcArithˢ, @new<Arith>());
    newServer.RegisterName(newServerArithˢ, @new<Arith>());

    net.Listener l = default;
    (l, newServerAddr) = listenTCP();
    log.Println(newServerTestRpcServerˢ, newServerAddr);
    goǃ(newServer.Accept, l);

    newServer.HandleHTTP(newHttpPath, barˢ);
    ᏑhttpOnce.Do(startHttpServer);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testHttpRpcServerˢ = (@string)"Test HTTP RPC server listening on"u8;

internal static void startHttpServer() {
    var server = httptest.NewServer(default);
    httpServerAddr = server.Listener.Addr().String();
    log.Println(testHttpRpcServerˢ, httpServerAddr);
}

public static void TestRPC(ж<testing.T> t) {
    Ꮡonce.Do(startServer);
    testRPC(t, serverAddr);
    ᏑnewOnce.Do(startNewServer);
    testRPC(t, newServerAddr);
    testNewServerRPC(t, newServerAddr);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object dialingˢ = (@string)"dialing"u8;
internal static ref readonly @string arithAddˢ => ref arithAddˢᵇ;
private static readonly @string arithAddˢᵇ = "Arith.Add"u8;
internal static ref readonly @string embedExportedˢ => ref embedExportedˢᵇ;
private static readonly @string embedExportedˢᵇ = "Embed.Exported"u8;
internal static ref readonly @string arithBadOperationˢ => ref arithBadOperationˢᵇ;
private static readonly @string arithBadOperationˢᵇ = "Arith.BadOperation"u8;
internal static readonly object badOperationExpectedˢ = (@string)"BadOperation: expected error"u8;
internal static ref readonly @string rpcCanTFindMethodˢ => ref rpcCanTFindMethodˢᵇ;
private static readonly @string rpcCanTFindMethodˢᵇ = "rpc: can't find method "u8;
internal static ref readonly @string arithUnknownˢ => ref arithUnknownˢᵇ;
private static readonly @string arithUnknownˢᵇ = "Arith.Unknown"u8;
internal static readonly object expectedErrorCallingˢ = (@string)"expected error calling unknown service"u8;
internal static ref readonly @string methodˢ => ref methodˢᵇ;
private static readonly @string methodˢᵇ = "method"u8;
internal static readonly object expectedErrorAboutMethodˢ = (@string)"expected error about method; got"u8;
internal static ref readonly @string arithMulˢ => ref arithMulˢᵇ;
private static readonly @string arithMulˢᵇ = "Arith.Mul"u8;
internal static ref readonly @string arithDivˢ => ref arithDivˢᵇ;
private static readonly @string arithDivˢᵇ = "Arith.Div"u8;
internal static readonly object divExpectedErrorˢ = (@string)"Div: expected error"u8;
internal static readonly object divExpectedDivideByZeroˢ = (@string)"Div: expected divide by zero error; got"u8;
internal static readonly object expectedErrorCallingˢ2 = (@string)"expected error calling Arith.Add with wrong arg type"u8;
internal static ref readonly @string typeˢ => ref typeˢᵇ;
private static readonly @string typeˢᵇ = "type"u8;
internal static readonly object expectedErrorAboutTypeˢ = (@string)"expected error about type; got"u8;
internal static ref readonly @string arithScanˢ => ref arithScanˢᵇ;
private static readonly @string arithScanˢᵇ = "Arith.Scan"u8;
internal static ref readonly @string arithStringˢ => ref arithStringˢᵇ;
private static readonly @string arithStringˢᵇ = "Arith.String"u8;
internal static ref readonly @string netRpcArithAddˢ => ref netRpcArithAddˢᵇ;
private static readonly @string netRpcArithAddˢᵇ = "net.rpc.Arith.Add"u8;

internal static void testRPC(ж<testing.T> t, @string addr) {
    GoFrame ᒐ = default;
    try {
        var (client, err) = Dial(tcpˢ, addr);
        if (err != nil) {
            t.Fatal(dialingˢ, err);
        }
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);

        // Synchronous calls
        var args = Ꮡ(new Args(7, 8));
        var reply = @new<Reply>();
        err = client.Call(arithAddˢ, args.OrTypedNil(), reply.OrTypedNil());
        if (err != nil) {
            t.Errorf("Add: expected no error but got string %q", err.Error());
        }
        if (reply.C != args.A + args.B) {
            t.Errorf("Add: expected %d got %d", reply.C, args.A + args.B);
        }

        // Methods exported from unexported embedded structs
        args = Ꮡ(new Args(7, 0));
        reply = @new<Reply>();
        err = client.Call(embedExportedˢ, args.OrTypedNil(), reply.OrTypedNil());
        if (err != nil) {
            t.Errorf("Add: expected no error but got string %q", err.Error());
        }
        if (reply.C != args.A + args.B) {
            t.Errorf("Add: expected %d got %d", reply.C, args.A + args.B);
        }

        // Nonexistent method
        args = Ꮡ(new Args(7, 0));
        reply = @new<Reply>();
        err = client.Call(arithBadOperationˢ, args.OrTypedNil(), reply.OrTypedNil());
        // expect an error
        if (err == nil) {
            t.Error(badOperationExpectedˢ);
        } else if (!strings.HasPrefix(err.Error(), rpcCanTFindMethodˢ)) {
            t.Errorf("BadOperation: expected can't find method error; got %q", err);
        }

        // Unknown service
        args = Ꮡ(new Args(7, 8));
        reply = @new<Reply>();
        err = client.Call(arithUnknownˢ, args.OrTypedNil(), reply.OrTypedNil());
        if (err == nil) {
            t.Error(expectedErrorCallingˢ);
        } else if (!strings.Contains(err.Error(), methodˢ)) {
            t.Error(expectedErrorAboutMethodˢ, err);
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
            t.Errorf("Add: expected %d got %d", addReply.C, args.A + args.B);
        }

        mulCall = ᐸꟷ(mulCall.Done);
        if (mulCall.Error != nil) {
            t.Errorf("Mul: expected no error but got string %q", mulCall.Error.Error());
        }
        if (mulReply.C != args.A * args.B) {
            t.Errorf("Mul: expected %d got %d", mulReply.C, args.A * args.B);
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

        // Bad type.
        reply = @new<Reply>();
        err = client.Call(arithAddˢ, reply.OrTypedNil(), reply.OrTypedNil()); // args, reply would be the correct thing to use
        if (err == nil) {
            t.Error(expectedErrorCallingˢ2);
        } else if (!strings.Contains(err.Error(), typeˢ)) {
            t.Error(expectedErrorAboutTypeˢ, err);
        }

        // Non-struct argument
        const nint Val = 12345;
        ref var str = ref heap<@string>(fmt.Sprint((nint)(Val)), out var Ꮡstr);
        reply = @new<Reply>();
        err = client.Call(arithScanˢ, Ꮡstr, reply.OrTypedNil());
        if (err != nil) {
            t.Errorf("Scan: expected no error but got string %q", err.Error());
        } else if (reply.C != Val) {
            t.Errorf("Scan: expected %d got %d", (nint)(Val), reply.C);
        }

        // Non-struct reply
        args = Ꮡ(new Args(27, 35));
        str = ""u8;
        err = client.Call(arithStringˢ, args.OrTypedNil(), Ꮡstr);
        if (err != nil) {
            t.Errorf("String: expected no error but got string %q", err.Error());
        }
        @string expect = fmt.Sprintf("%d+%d=%d"u8, args.A, args.B, args.A + args.B);
        if (str != expect) {
            t.Errorf("String: expected %s got %s", expect, str);
        }

        args = Ꮡ(new Args(7, 8));
        reply = @new<Reply>();
        err = client.Call(arithMulˢ, args.OrTypedNil(), reply.OrTypedNil());
        if (err != nil) {
            t.Errorf("Mul: expected no error but got string %q", err.Error());
        }
        if (reply.C != args.A * args.B) {
            t.Errorf("Mul: expected %d got %d", reply.C, args.A * args.B);
        }

        // ServiceName contain "." character
        args = Ꮡ(new Args(7, 8));
        reply = @new<Reply>();
        err = client.Call(netRpcArithAddˢ, args.OrTypedNil(), reply.OrTypedNil());
        if (err != nil) {
            t.Errorf("Add: expected no error but got string %q", err.Error());
        }
        if (reply.C != args.A + args.B) {
            t.Errorf("Add: expected %d got %d", reply.C, args.A + args.B);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string newServerArithAddˢ => ref newServerArithAddˢᵇ;
private static readonly @string newServerArithAddˢᵇ = "newServer.Arith.Add"u8;

internal static void testNewServerRPC(ж<testing.T> t, @string addr) {
    GoFrame ᒐ = default;
    try {
        var (client, err) = Dial(tcpˢ, addr);
        if (err != nil) {
            t.Fatal(dialingˢ, err);
        }
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);

        // Synchronous calls
        var args = Ꮡ(new Args(7, 8));
        var reply = @new<Reply>();
        err = client.Call(newServerArithAddˢ, args.OrTypedNil(), reply.OrTypedNil());
        if (err != nil) {
            t.Errorf("Add: expected no error but got string %q", err.Error());
        }
        if (reply.C != args.A + args.B) {
            t.Errorf("Add: expected %d got %d", reply.C, args.A + args.B);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestHTTP(ж<testing.T> t) {
    Ꮡonce.Do(startServer);
    testHTTPRPC(t, ""u8);
    ᏑnewOnce.Do(startNewServer);
    testHTTPRPC(t, newHttpPath);
}

internal static void testHTTPRPC(ж<testing.T> t, @string path) {
    GoFrame ᒐ = default;
    try {
        ж<global::go.net.rpc_package.Client> client = default;
        error err = default;
        if (path == ""u8) {
            (client, err) = DialHTTP(tcpˢ, httpServerAddr);
        } else {
            (client, err) = DialHTTPPath(tcpˢ, httpServerAddr, path);
        }
        if (err != nil) {
            t.Fatal(dialingˢ, err);
        }
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);

        // Synchronous calls
        var args = Ꮡ(new Args(7, 8));
        var reply = @new<Reply>();
        err = client.Call(arithAddˢ, args.OrTypedNil(), reply.OrTypedNil());
        if (err != nil) {
            t.Errorf("Add: expected no error but got string %q", err.Error());
        }
        if (reply.C != args.A + args.B) {
            t.Errorf("Add: expected %d got %d", reply.C, args.A + args.B);
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

public static void TestBuiltinTypes(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        Ꮡonce.Do(startServer);

        var (client, err) = DialHTTP(tcpˢ, httpServerAddr);
        if (err != nil) {
            t.Fatal(dialingˢ, err);
        }
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);

        // Map
        var args = Ꮡ(new Args(7, 8));
        ref var replyMap = ref heap<map<nint, nint>>(new map<nint, nint>{}, out var ᏑreplyMap);
        err = client.Call(builtinTypesMapˢ, args.OrTypedNil(), ᏑreplyMap);
        if (err != nil) {
            t.Errorf("Map: expected no error but got string %q", err.Error());
        }
        if (replyMap[args.A] != args.B) {
            t.Errorf("Map: expected %d got %d", args.B, replyMap[args.A]);
        }

        // Slice
        args = Ꮡ(new Args(7, 8));
        ref var replySlice = ref heap<slice<nint>>(slice<nint>([]), out var ᏑreplySlice);
        err = client.Call(builtinTypesSliceˢ, args.OrTypedNil(), ᏑreplySlice);
        if (err != nil) {
            t.Errorf("Slice: expected no error but got string %q", err.Error());
        }
        {
            var e = slice<nint>([args.A, args.B]); if (!reflect.DeepEqual(replySlice, e)) {
                t.Errorf("Slice: expected %v got %v", e, replySlice);
            }
        }

        // Array
        args = Ꮡ(new Args(7, 8));
        ref var replyArray = ref heap<array<nint>>(new nint[]{}.array(2), out var ᏑreplyArray);
        err = client.Call(builtinTypesArrayˢ, args.OrTypedNil(), ᏑreplyArray);
        if (err != nil) {
            t.Errorf("Array: expected no error but got string %q", err.Error());
        }
        {
            var e = new nint[]{args.A, args.B}.array(); if (!reflect.DeepEqual(replyArray, e)) {
                t.Errorf("Array: expected %v got %v", e, replyArray);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// CodecEmulator provides a client-like api and a ServerCodec interface.
// Can be used to test ServeRequest.
public partial struct CodecEmulator {
    internal ж<global::go.net.rpc_package.Server> server;
    internal @string serviceMethod;
    internal ж<Args> args;
    internal ж<Reply> reply;
    internal error err;
}

public static error Call(this ж<CodecEmulator> Ꮡcodec, @string serviceMethod, ж<Args> Ꮡargs, ж<Reply> Ꮡreply) {
    ref var codec = ref Ꮡcodec.DerefOrNull();
    ref var args = ref Ꮡargs.DerefOrNull();
    ref var reply = ref Ꮡreply.DerefOrNull();

    codec.serviceMethod = serviceMethod;
    codec.args = Ꮡargs;
    codec.reply = Ꮡreply;
    codec.err = default;
    error serverError = default;
    if (codec.server == nil) {
        serverError = ServeRequest(new rpc_internal_test_package.CodecEmulatorжServerCodec(Ꮡcodec));
    } else {
        serverError = codec.server.ServeRequest(new rpc_internal_test_package.CodecEmulatorжServerCodec(Ꮡcodec));
    }
    if (codec.err == nil && serverError != nil) {
        codec.err = serverError;
    }
    return codec.err;
}

public static error ReadRequestHeader(this ref CodecEmulator codec, ж<global::go.net.rpc_package.Request> Ꮡreq) {
    ref var req = ref Ꮡreq.DerefOrNull();

    req.ServiceMethod = codec.serviceMethod;
    req.Seq = 0;
    return default;
}

public static error ReadRequestBody(this ref CodecEmulator codec, any argv) {
    if (codec.args == nil) {
        return io.ErrUnexpectedEOF;
    }
    (argv._<ж<Args>>()).Value = codec.args.Value;
    return default;
}

public static error WriteResponse(this ref CodecEmulator codec, ж<global::go.net.rpc_package.Response> Ꮡresp, any reply) {
    ref var resp = ref Ꮡresp.DerefOrNull();

    if (resp.Error != ""u8) {
        codec.err = errors.New(resp.Error);
    } else {
        codec.reply.Value = (reply._<ж<Reply>>()).Value;
    }
    return default;
}

public static error Close(this ref CodecEmulator codec) {
    return default;
}

public static void TestServeRequest(ж<testing.T> t) {
    Ꮡonce.Do(startServer);
    testServeRequest(t, nil);
    ᏑnewOnce.Do(startNewServer);
    testServeRequest(t, newServer);
}

internal static void testServeRequest(ж<testing.T> t, ж<global::go.net.rpc_package.Server> server) {
    GoFrame ᒐ = default;
    try {
        ref var client = ref heap<CodecEmulator>(new CodecEmulator(server: server), out var Ꮡclient);
        defer(() => Ꮡclient.Value.Close(), ref ᒐ);

        var args = Ꮡ(new Args(7, 8));
        var reply = @new<Reply>();
        var err = Ꮡclient.Call(arithAddˢ, args, reply);
        if (err != nil) {
            t.Errorf("Add: expected no error but got string %q", err.Error());
        }
        if (reply.C != args.A + args.B) {
            t.Errorf("Add: expected %d got %d", reply.C, args.A + args.B);
        }

        err = Ꮡclient.Call(arithAddˢ, nil, reply);
        if (err == nil) {
            t.Errorf("expected error calling Arith.Add with nil arg");
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public partial struct ΔReplyNotPointer /*num:nint*/;
public partial struct ΔArgNotPublic /*num:nint*/;
public partial struct ΔReplyNotPublic /*num:nint*/;
public partial struct ΔNeedsPtrType /*num:nint*/;
public partial struct local {
}

public static error ReplyNotPointer(this ref ΔReplyNotPointer t, ж<Args> args, Reply reply) {
    return default;
}

public static error ArgNotPublic(this ref ΔArgNotPublic t, ж<local> args, ж<Reply> reply) {
    return default;
}

public static error ReplyNotPublic(this ref ΔReplyNotPublic t, ж<Args> args, ж<local> reply) {
    return default;
}

public static error NeedsPtrType(this ref ΔNeedsPtrType t, ж<Args> args, ж<Reply> reply) {
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorRegisteringˢ = (@string)"expected error registering ReplyNotPointer"u8;
internal static readonly object expectedErrorRegisteringˢ2 = (@string)"expected error registering ArgNotPublic"u8;
internal static readonly object expectedErrorRegisteringˢ3 = (@string)"expected error registering ReplyNotPublic"u8;
internal static readonly object expectedErrorRegisteringˢ4 = (@string)"expected error registering NeedsPtrType"u8;
internal static ref readonly @string pointerˢ => ref pointerˢᵇ;
private static readonly @string pointerˢᵇ = "pointer"u8;
internal static readonly object expectedHintWhenˢ = (@string)"expected hint when registering NeedsPtrType"u8;

// Check that registration handles lots of bad methods and a type with no suitable methods.
public static void TestRegistrationError(ж<testing.T> t) {
    var err = Register(@new<ΔReplyNotPointer>());
    if (err == nil) {
        t.Error(expectedErrorRegisteringˢ);
    }
    err = Register(@new<ΔArgNotPublic>());
    if (err == nil) {
        t.Error(expectedErrorRegisteringˢ2);
    }
    err = Register(@new<ΔReplyNotPublic>());
    if (err == nil) {
        t.Error(expectedErrorRegisteringˢ3);
    }
    err = Register((ΔNeedsPtrType)0);
    if (err == nil) {
        t.Error(expectedErrorRegisteringˢ4);
    } else if (!strings.Contains(err.Error(), pointerˢ)) {
        t.Error(expectedHintWhenˢ);
    }
}

public partial struct WriteFailCodec /*num:nint*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string failˢ => ref failˢᵇ;
private static readonly @string failˢᵇ = "fail"u8;

public static error WriteRequest(this WriteFailCodec _Δp0, ж<global::go.net.rpc_package.Request> _Δp1, any _Δp2) {
    // the panic caused by this error used to not unlock a lock.
    return errors.New(failˢ);
}

public static error ReadResponseHeader(this WriteFailCodec _Δp0, ж<global::go.net.rpc_package.Response> _Δp1) {
    select();
    return default;
}

public static error ReadResponseBody(this WriteFailCodec _Δp0, any _Δp1) {
    select();
    return default;
}

public static error Close(this WriteFailCodec _) {
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object deadlockˢ = (@string)"deadlock"u8;

public static partial void TestSendDeadlock(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var client = NewClientWithCodec((WriteFailCodec)0);
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);

        var done = new channel<bool>(0);
        var clientʗ2 = client;
        var doneʗ1 = done;
        goǃ(() => {
            testSendDeadlock(clientʗ2);
            testSendDeadlock(clientʗ2);
            doneʗ1.ᐸꟷ(true);
        });
        var selᴛ1 = done;
        var selᴛ2 = time.After((time.Duration)(5000000000L));
        switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
        case 0 when selᴛ1.ꟷᐳ(out _): {
            return;
        }
        case 1 when selᴛ2.ꟷᐳ(out _): {
            t.Fatal(deadlockˢ);
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testSendDeadlock(ж<global::go.net.rpc_package.Client> client) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            recover();
        }, ref ᒐ);
        var args = Ꮡ(new Args(7, 8));
        var reply = @new<Reply>();
        client.Call(arithAddˢ, args.OrTypedNil(), reply.OrTypedNil());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static (ж<global::go.net.rpc_package.Client>, error) dialDirect() {
    return Dial(tcpˢ, serverAddr);
}

internal static (ж<global::go.net.rpc_package.Client>, error) dialHTTP() {
    return DialHTTP(tcpˢ, httpServerAddr);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errorDialingˢ = (@string)"error dialing"u8;

internal static float64 countMallocs(Func<(ж<global::go.net.rpc_package.Client>, error)> dial, ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        Ꮡonce.Do(startServer);
        var (client, err) = dial();
        if (err != nil) {
            t.Fatal(errorDialingˢ, err);
        }
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);

        var args = Ꮡ(new Args(7, 8));
        var reply = @new<Reply>();
        var argsʗ1 = args;
        var clientʗ2 = client;
        var replyʗ1 = reply;
        return testing.AllocsPerRun(100, () => {
            var errΔ1 = clientʗ2.Call(arithAddˢ, argsʗ1.OrTypedNil(), replyʗ1.OrTypedNil());
            if (errΔ1 != nil) {
                t.Errorf("Add: expected no error but got string %q", errΔ1.Error());
            }
            if (replyʗ1.C != argsʗ1.A + argsʗ1.B) {
                t.Errorf("Add: expected %d got %d", replyʗ1.C, argsʗ1.A + argsʗ1.B);
            }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingMallocCountInˢ = (@string)"skipping malloc count in short mode"u8;
internal static readonly object skippingGomaxprocs1ˢ = (@string)"skipping; GOMAXPROCS>1"u8;

public static void TestCountMallocs(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(skippingMallocCountInˢ);
    }
    if (runtime.GOMAXPROCS(0) > 1) {
        t.Skip(skippingGomaxprocs1ˢ);
    }
    fmt.Printf("mallocs per rpc round trip: %v\n"u8, countMallocs(dialDirect, t));
}

public static void TestCountMallocsOverHTTP(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(skippingMallocCountInˢ);
    }
    if (runtime.GOMAXPROCS(0) > 1) {
        t.Skip(skippingGomaxprocs1ˢ);
    }
    fmt.Printf("mallocs per HTTP rpc round trip: %v\n"u8, countMallocs(dialHTTP, t));
}

internal partial struct writeCrasher {
    internal channel<bool> done;
}

internal static error Close(this writeCrasher _) {
    return default;
}

internal static (nint, error) Read(this ref writeCrasher w, slice<byte> p) {
    ᐸꟷ(w.done);
    return (0, io.EOF);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fakeWriteFailureˢ => ref fakeWriteFailureˢᵇ;
private static readonly @string fakeWriteFailureˢᵇ = "fake write failure"u8;

internal static (nint, error) Write(this writeCrasher _, slice<byte> p) {
    return (0, errors.New(fakeWriteFailureˢ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;
internal static readonly object expectedErrorˢ = (@string)"expected error"u8;
internal static readonly object unexpectedValueOfErrorˢ = (@string)"unexpected value of error:"u8;

public static void TestClientWriteError(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var w = Ꮡ(new writeCrasher(done: new channel<bool>(0)));
        var c = NewClient(new rpc_internal_test_package.writeCrasherжReadWriteCloser(w));
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);

        ref var res = ref heap<bool>(false, out var Ꮡres);
        var err = c.Call(fooˢ, (nint)1, Ꮡres);
        if (err == nil) {
            t.Fatal(expectedErrorˢ);
        }
        if (err.Error() != "fake write failure"u8) {
            t.Error(unexpectedValueOfErrorˢ, err);
        }
        w.done.ᐸꟷ(true);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object arithErrorˢ = (@string)"arith error:"u8;

public static void TestTCPClose(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        Ꮡonce.Do(startServer);

        var (client, err) = dialHTTP();
        if (err != nil) {
            t.Fatalf("dialing: %v", err);
        }
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);

        var args = new Args(17, 8);
        ref var reply = ref heap(new Reply(), out var Ꮡreply);
        err = client.Call(arithMulˢ, args, Ꮡreply);
        if (err != nil) {
            t.Fatal(arithErrorˢ, err);
        }
        t.Logf("Arith: %d*%d=%d\n", args.A, args.B, reply);
        if (reply.C != args.A * args.B) {
            t.Errorf("Add: expected %d got %d", reply.C, args.A * args.B);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object closeErrorˢ = (@string)"close error:"u8;

public static void TestErrorAfterClientClose(ж<testing.T> t) {
    Ꮡonce.Do(startServer);

    var (client, err) = dialHTTP();
    if (err != nil) {
        t.Fatalf("dialing: %v", err);
    }
    err = client.Close();
    if (err != nil) {
        t.Fatal(closeErrorˢ, err);
    }
    err = client.Call(arithAddˢ, Ꮡ(new Args(7, 9)), @new<Reply>());
    if (!AreEqual(err, ErrShutdown)) {
        t.Errorf("Forever: expected ErrShutdown got %v", err);
    }
}

// Tests the fix to issue 11221. Without the fix, this loops forever or crashes.
public static void TestAcceptExitAfterListenerClose(ж<testing.T> t) {
    var newServer = NewServer();
    newServer.Register(@new<Arith>());
    newServer.RegisterName(netRpcArithˢ, @new<Arith>());
    newServer.RegisterName(newServerArithˢ, @new<Arith>());

    net.Listener l = default;
    (l, _) = listenTCP();
    l.Close();
    newServer.Accept(l);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string arithSleepMilliˢ => ref arithSleepMilliˢᵇ;
private static readonly @string arithSleepMilliˢᵇ = "Arith.SleepMilli"u8;

public static partial void TestShutdown(ж<testing.T> t) {
    net.Listener l = default;
    (l, _) = listenTCP();
    var ch = new channel<net.Conn>(1);
    var chʗ1 = ch;
    var lʗ1 = l;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            var lʗ2 = lʗ1;
            defer(() => lʗ2.Close(), ref ᒐ);
            var (cΔ1, errΔ1) = lʗ1.Accept();
            if (errΔ1 != nil) {
                t.Error(errΔ1);
            }
            chʗ1.ᐸꟷ(cΔ1);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    var (c, err) = net.Dial(tcpˢ, l.Addr().String());
    if (err != nil) {
        t.Fatal(err);
    }
    var c1 = ᐸꟷ(ch);
    if (c1 == nil) {
        t.Fatal(err);
    }

    var newServer = NewServer();
    newServer.Register(@new<Arith>());
    var newServerʗ1 = newServer;
    goǃ(newServerʗ1.ServeConn, new rpc_internal_test_package.net_ConnᴠReadWriteCloser(c1));

    var args = Ꮡ(new Args(7, 8));
    var reply = @new<Reply>();
    var client = NewClient(new rpc_internal_test_package.net_ConnᴠReadWriteCloser(c));
    err = client.Call(arithAddˢ, args.OrTypedNil(), reply.OrTypedNil());
    if (err != nil) {
        t.Fatal(err);
    }

    // On an unloaded system 10ms is usually enough to fail 100% of the time
    // with a broken server. On a loaded system, a broken server might incorrectly
    // be reported as passing, but we're OK with that kind of flakiness.
    // If the code is correct, this test will never fail, regardless of timeout.
    args.A = 10; // 10 ms
    var done = new channel<ж<global::go.net.rpc_package.ΔCall>>(1);
    var call = client.Go(arithSleepMilliˢ, args.OrTypedNil(), reply.OrTypedNil(), done);
    c._<ж<net.TCPConn>>().CloseWrite();
    ᐸꟷ(done);
    if (call.Error != nil) {
        t.Fatal(err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errorDialingˢ2 = (@string)"error dialing:"u8;

internal static void benchmarkEndToEnd(Func<(ж<global::go.net.rpc_package.Client>, error)> dial, ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        Ꮡonce.Do(startServer);
        var (client, err) = dial();
        if (err != nil) {
            Ꮡb.Fatal(errorDialingˢ2, err);
        }
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);

        // Synchronous calls
        var args = Ꮡ(new Args(7, 8));
        b.ResetTimer();

        var argsʗ1 = args;
        var clientʗ2 = client;
        Ꮡb.RunParallel((ж<testing.PB> pb) => {
            var reply = @new<Reply>();
            while (pb.Next()) {
                var errΔ1 = clientʗ2.Call(arithAddˢ, argsʗ1.OrTypedNil(), reply.OrTypedNil());
                if (errΔ1 != nil) {
                    Ꮡb.Fatalf("rpc error: Add: expected no error but got string %q", errΔ1.Error());
                }
                if (reply.C != argsʗ1.A + argsʗ1.B) {
                    Ꮡb.Fatalf("rpc error: Add: expected %d got %d", reply.C, argsʗ1.A + argsʗ1.B);
                }
            }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void benchmarkEndToEndAsync(Func<(ж<global::go.net.rpc_package.Client>, error)> dial, ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        if (b.N == 0) {
            return;
        }
        const nint MaxConcurrentCalls = 100;
        Ꮡonce.Do(startServer);
        var (client, err) = dial();
        if (err != nil) {
            Ꮡb.Fatal(errorDialingˢ2, err);
        }
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);

        // Asynchronous calls
        var args = Ꮡ(new Args(7, 8));
        nint procs = 4 * runtime.GOMAXPROCS(-1);
        ref var send = ref heap<int32>((int32)b.N, out var Ꮡsend);
        ref var recv = ref heap<int32>((int32)b.N, out var Ꮡrecv);
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(procs);
        var gate = new channel<bool>(MaxConcurrentCalls);
        var res = new channel<ж<global::go.net.rpc_package.ΔCall>>(MaxConcurrentCalls);
        b.ResetTimer();

        for (nint p = 0; p < procs; p++) {
            var argsʗ1 = args;
            var clientʗ2 = client;
            var gateʗ1 = gate;
            var resʗ1 = res;
            goǃ(() => {
                while (atomic.AddInt32(Ꮡsend, -1) >= 0) {
                    gateʗ1.ᐸꟷ(true);
                    var reply = @new<Reply>();
                    clientʗ2.Go(arithAddˢ, argsʗ1.OrTypedNil(), reply.OrTypedNil(), resʗ1);
                }
            });
            var gateʗ2 = gate;
            var resʗ2 = res;
            goǃ(() => {
                foreach (var call in resʗ2) {
                    nint A = call.Args._<ж<Args>>().A;
                    nint B = call.Args._<ж<Args>>().B;
                    nint C = call.Reply._<ж<Reply>>().C;
                    if (A + B != C) {
                        Ꮡb.Errorf("incorrect reply: Add: expected %d got %d", A + B, C);
                        return;
                    }
                    ᐸꟷ(gateʗ2);
                    if (atomic.AddInt32(Ꮡrecv, -1) == 0) {
                        close(resʗ2);
                    }
                }
                Ꮡwg.Done();
            });
        }
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkEndToEnd(ж<testing.B> b) {
    benchmarkEndToEnd(dialDirect, b);
}

public static void BenchmarkEndToEndHTTP(ж<testing.B> b) {
    benchmarkEndToEnd(dialHTTP, b);
}

public static void BenchmarkEndToEndAsync(ж<testing.B> b) {
    benchmarkEndToEndAsync(dialDirect, b);
}

public static void BenchmarkEndToEndAsyncHTTP(ж<testing.B> b) {
    benchmarkEndToEndAsync(dialHTTP, b);
}

} // end rpc_internal_test_package
