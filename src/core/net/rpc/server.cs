// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package rpc provides access to the exported methods of an object across a
network or other I/O connection.  A server registers an object, making it visible
as a service with the name of the type of the object.  After registration, exported
methods of the object will be accessible remotely.  A server may register multiple
objects (services) of different types but it is an error to register multiple
objects of the same type.

Only methods that satisfy these criteria will be made available for remote access;
other methods will be ignored:

  - the method's type is exported.
  - the method is exported.
  - the method has two arguments, both exported (or builtin) types.
  - the method's second argument is a pointer.
  - the method has return type error.

In effect, the method must look schematically like

	func (t *T) MethodName(argType T1, replyType *T2) error

where T1 and T2 can be marshaled by encoding/gob.
These requirements apply even if a different codec is used.
(In the future, these requirements may soften for custom codecs.)

The method's first argument represents the arguments provided by the caller; the
second argument represents the result parameters to be returned to the caller.
The method's return value, if non-nil, is passed back as a string that the client
sees as if created by [errors.New].  If an error is returned, the reply parameter
will not be sent back to the client.

The server may handle requests on a single connection by calling [ServeConn].  More
typically it will create a network listener and call [Accept] or, for an HTTP
listener, [HandleHTTP] and [http.Serve].

A client wishing to use the service establishes a connection and then invokes
[NewClient] on the connection.  The convenience function [Dial] ([DialHTTP]) performs
both steps for a raw network connection (an HTTP connection).  The resulting
[Client] object has two methods, [Call] and Go, that specify the service and method to
call, a pointer containing the arguments, and a pointer to receive the result
parameters.

The Call method waits for the remote call to complete while the Go method
launches the call asynchronously and signals completion using the Call
structure's Done channel.

Unless an explicit codec is set up, package [encoding/gob] is used to
transport the data.

Here is a simple example.  A server wishes to export an object of type Arith:

	package server

	import "errors"

	type Args struct {
		A, B int
	}

	type Quotient struct {
		Quo, Rem int
	}

	type Arith int

	func (t *Arith) Multiply(args *Args, reply *int) error {
		*reply = args.A * args.B
		return nil
	}

	func (t *Arith) Divide(args *Args, quo *Quotient) error {
		if args.B == 0 {
			return errors.New("divide by zero")
		}
		quo.Quo = args.A / args.B
		quo.Rem = args.A % args.B
		return nil
	}

The server calls (for HTTP service):

	arith := new(Arith)
	rpc.Register(arith)
	rpc.HandleHTTP()
	l, err := net.Listen("tcp", ":1234")
	if err != nil {
		log.Fatal("listen error:", err)
	}
	go http.Serve(l, nil)

At this point, clients can see a service "Arith" with methods "Arith.Multiply" and
"Arith.Divide".  To invoke one, a client first dials the server:

	client, err := rpc.DialHTTP("tcp", serverAddress + ":1234")
	if err != nil {
		log.Fatal("dialing:", err)
	}

Then it can make a remote call:

	// Synchronous call
	args := &server.Args{7,8}
	var reply int
	err = client.Call("Arith.Multiply", args, &reply)
	if err != nil {
		log.Fatal("arith error:", err)
	}
	fmt.Printf("Arith: %d*%d=%d", args.A, args.B, reply)

or

	// Asynchronous call
	quotient := new(Quotient)
	divCall := client.Go("Arith.Divide", args, quotient, nil)
	replyCall := <-divCall.Done	// will be equal to divCall
	// check errors, print, etc.

A server implementation will often provide a simple, type-safe wrapper for the
client.

The net/rpc package is frozen and is not accepting new features.
*/
namespace go.net;

using bufio = bufio_package;
using gob = encoding.gob_package;
using errors = errors_package;
using token = global::go.go.token_package;
using io = io_package;
using log = log_package;
using net = net_package;
using Δhttp = global::go.net.http_package;
using reflect = reflect_package;
using strings = strings_package;
using sync = sync_package;
using encoding;
using global::go.go;
using global::go.net;

partial class rpc_package {

public static readonly @string DefaultRPCPath = "/_goRPC_"u8;
public static readonly @string DefaultDebugPath = "/debug/rpc"u8;

// Precompute the reflect type for error.
internal static reflectꓸType typeOfError = reflect.TypeFor<error>();

[GoType] public partial struct methodType {
    public partial ref sync_package.Mutex Mutex { get; } // protects counters
    internal reflectꓸMethod method;
    public reflectꓸType ArgType;
    public reflectꓸType ReplyType;
    internal nuint numCalls;
}

[GoType] public partial struct service {
    internal @string name;                // name of service
    internal reflectꓸValue rcvr;          // receiver of methods for the service
    internal reflectꓸType typ;           // type of the receiver
    internal map<@string, ж<methodType>> method; // registered methods
}

// Request is a header written before every RPC call. It is used internally
// but documented here as an aid to debugging, such as when analyzing
// network traffic.
[GoType] partial struct Request {
    public @string ServiceMethod;  // format: "Service.Method"
    public uint64 Seq;   // sequence number chosen by client
    internal ж<Request> next; // for free list in Server
}

// Response is a header written before every RPC return. It is used internally
// but documented here as an aid to debugging, such as when analyzing
// network traffic.
[GoType] partial struct Response {
    public @string ServiceMethod;   // echoes that of the Request
    public uint64 Seq;    // echoes that of the request
    public @string Error;   // error, if any.
    internal ж<Response> next; // for free list in Server
}

// Server represents an RPC Server.
[GoType] partial struct Server {
    internal sync.Map serviceMap;   // map[string]*service
    internal sync.Mutex reqLock; // protects freeReq
    internal ж<Request> freeReq;
    internal sync.Mutex respLock; // protects freeResp
    internal ж<Response> freeResp;
}

// NewServer returns a new [Server].
public static ж<Server> NewServer() {
    return Ꮡ(new Server(nil));
}

// DefaultServer is the default instance of [*Server].
public static ж<Server> DefaultServer = NewServer();

// Is this type exported or a builtin?
internal static bool isExportedOrBuiltinType(reflectꓸType t) {
    while (t.Kind() == reflect.ΔPointer) {
        t = t.Elem();
    }
    // PkgPath will be non-empty even for an exported type,
    // so we need to check the type name as well.
    return token.IsExported(t.Name()) || t.PkgPath() == ""u8;
}

// Register publishes in the server the set of methods of the
// receiver value that satisfy the following conditions:
//   - exported method of exported type
//   - two arguments, both of exported type
//   - the second argument is a pointer
//   - one return value, of type error
//
// It returns an error if the receiver is not an exported type or has
// no suitable methods. It also logs the error using package log.
// The client accesses each method using a string of the form "Type.Method",
// where Type is the receiver's concrete type.
public static error Register(this ж<Server> Ꮡserver, any rcvr) {
    return Ꮡserver.register(rcvr, ""u8, false);
}

// RegisterName is like [Register] but uses the provided name for the type
// instead of the receiver's concrete type.
public static error RegisterName(this ж<Server> Ꮡserver, @string name, any rcvr) {
    return Ꮡserver.register(rcvr, name, true);
}

// logRegisterError specifies whether to log problems during method registration.
// To debug registration, recompile the package with this set to true.
internal const bool logRegisterError = false;

internal static error register(this ж<Server> Ꮡserver, any rcvr, @string name, bool useName) {
    var s = @new<service>();
    s.Value.typ = reflect.TypeOf(rcvr);
    s.Value.rcvr = reflect.ValueOf(rcvr);
    @string sname = name;
    if (!useName) {
        sname = reflect.Indirect((~s).rcvr).Type().Name();
    }
    if (sname == ""u8) {
        @string sΔ1 = "rpc.Register: no service name for type "u8 + (~s).typ.String();
        log.Print(sΔ1);
        return errors.New(sΔ1);
    }
    if (!useName && !token.IsExported(sname)) {
        @string sΔ2 = "rpc.Register: type "u8 + sname + " is not exported"u8;
        log.Print(sΔ2);
        return errors.New(sΔ2);
    }
    s.Value.name = sname;
    // Install the methods
    s.Value.method = suitableMethods((~s).typ, logRegisterError);
    if (len((~s).method) == 0) {
        @string str = ""u8;
        // To help the user, see if a pointer receiver would work.
        var method = suitableMethods(reflect.PointerTo((~s).typ), false);
        if (len(method) != 0){
            str = "rpc.Register: type "u8 + sname + " has no exported methods of suitable type (hint: pass a pointer to value of that type)"u8;
        } else {
            str = "rpc.Register: type "u8 + sname + " has no exported methods of suitable type"u8;
        }
        log.Print(str);
        return errors.New(str);
    }
    {
        var (_, dup) = Ꮡserver.of(Server.ᏑserviceMap).LoadOrStore(sname, s.OrTypedNil()); if (dup) {
            return errors.New("rpc: service already defined: "u8 + sname);
        }
    }
    return default!;
}

// suitableMethods returns suitable Rpc methods of typ. It will log
// errors if logErr is true.
internal static map<@string, ж<methodType>> suitableMethods(reflectꓸType typ, bool logErr) {
    var methods = new map<@string, ж<methodType>>();
    for (nint m = 0; m < typ.NumMethod(); m++) {
        ref var method = ref heap<reflectꓸMethod>(out var Ꮡmethod);
        method = typ.Method(m);
        var mtype = method.Type;
        @string mname = method.Name;
        // Method must be exported.
        if (!method.IsExported()) {
            continue;
        }
        // Method needs three ins: receiver, *args, *reply.
        if (mtype.NumIn() != 3) {
            if (logErr) {
                log.Printf("rpc.Register: method %q has %d input parameters; needs exactly three\n"u8, mname, mtype.NumIn());
            }
            continue;
        }
        // First arg need not be a pointer.
        var argType = mtype.In(1);
        if (!isExportedOrBuiltinType(argType)) {
            if (logErr) {
                log.Printf("rpc.Register: argument type of method %q is not exported: %q\n"u8, mname, argType);
            }
            continue;
        }
        // Second arg must be a pointer.
        var replyType = mtype.In(2);
        if (replyType.Kind() != reflect.ΔPointer) {
            if (logErr) {
                log.Printf("rpc.Register: reply type of method %q is not a pointer: %q\n"u8, mname, replyType);
            }
            continue;
        }
        // Reply type must be exported.
        if (!isExportedOrBuiltinType(replyType)) {
            if (logErr) {
                log.Printf("rpc.Register: reply type of method %q is not exported: %q\n"u8, mname, replyType);
            }
            continue;
        }
        // Method needs one out.
        if (mtype.NumOut() != 1) {
            if (logErr) {
                log.Printf("rpc.Register: method %q has %d output parameters; needs exactly one\n"u8, mname, mtype.NumOut());
            }
            continue;
        }
        // The return type of the method must be error.
        {
            var returnType = mtype.Out(0); if (!AreEqual(returnType, typeOfError)) {
                if (logErr) {
                    log.Printf("rpc.Register: return type of method %q is %q, must be error\n"u8, mname, returnType);
                }
                continue;
            }
        }
        methods[mname] = Ꮡ(new methodType(method: method, ArgType: argType, ReplyType: replyType));
    }
    return methods;
}

// A value sent as a placeholder for the server's response value when the server
// receives an invalid request. It is never decoded by the client since the Response
// contains an error when it is used.
internal static EmptyStruct invalidRequest = new EmptyStruct();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object rpcWritingResponseˢ = (@string)"rpc: writing response:"u8;

internal static void sendResponse(this ж<Server> Ꮡserver, ж<sync.Mutex> Ꮡsending, ж<Request> Ꮡreq, any reply, ServerCodec codec, @string errmsg) {
    ref var req = ref Ꮡreq.DerefOrNull();

    var resp = Ꮡserver.getResponse();
    // Encode the response header
    resp.Value.ServiceMethod = req.ServiceMethod;
    if (errmsg != ""u8) {
        resp.Value.Error = errmsg;
        reply = invalidRequest;
    }
    resp.Value.Seq = req.Seq;
    Ꮡsending.Lock();
    var err = codec.WriteResponse(resp, reply);
    if (debugLog && err != default!) {
        log.Println(rpcWritingResponseˢ, err);
    }
    Ꮡsending.Unlock();
    Ꮡserver.freeResponse(resp);
}

public static nuint /*n*/ NumCalls(this ж<methodType> Ꮡm) {
    nuint n = default!;

    ref var m = ref Ꮡm.DerefOrNull();
    Ꮡm.of(methodType.ᏑMutex).Lock();
    n = m.numCalls;
    Ꮡm.of(methodType.ᏑMutex).Unlock();
    return n;
}

internal static void call(this ж<service> Ꮡs, ж<Server> Ꮡserver, ж<sync.Mutex> Ꮡsending, ж<sync.WaitGroup> Ꮡwg, ж<methodType> Ꮡmtype, ж<Request> Ꮡreq, reflectꓸValue argv, reflectꓸValue replyv, ServerCodec codec) {
    GoFrame ᒐ = default;
    try {
        ref var s = ref Ꮡs.DerefOrNull();
        ref var mtype = ref Ꮡmtype.DerefOrNull();

        if (Ꮡwg != nil) {
            defer(Ꮡwg.Done, ref ᒐ);
        }
        Ꮡmtype.of(methodType.ᏑMutex).Lock();
        mtype.numCalls++;
        Ꮡmtype.of(methodType.ᏑMutex).Unlock();
        var function = mtype.method.Func;
        // Invoke the method, providing a new value for the reply.
        var returnValues = function.Call(new reflectꓸValue[]{s.rcvr, argv, replyv}.slice());
        // The return value for the method is an error.
        var errInter = returnValues[0].Interface();
        @string errmsg = ""u8;
        if (errInter != default!) {
            errmsg = errInter._<error>().Error();
        }
        Ꮡserver.sendResponse(Ꮡsending, Ꮡreq, replyv.Interface(), codec, errmsg);
        Ꮡserver.freeRequest(Ꮡreq);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] partial struct gobServerCodec {
    internal io.ReadWriteCloser rwc;
    internal ж<gob.Decoder> dec;
    internal ж<gob.Encoder> enc;
    internal ж<bufio.Writer> encBuf;
    internal bool closed;
}

[GoRecv] internal static error ReadRequestHeader(this ref gobServerCodec c, ж<Request> Ꮡr) {
    return c.dec.Decode(Ꮡr.OrTypedNil());
}

[GoRecv] internal static error ReadRequestBody(this ref gobServerCodec c, any body) {
    return c.dec.Decode(body);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object rpcGobErrorEncodingˢ = (@string)"rpc: gob error encoding response:"u8;
internal static readonly object rpcGobErrorEncodingBodyˢ = (@string)"rpc: gob error encoding body:"u8;

[GoRecv] internal static error /*err*/ WriteResponse(this ref gobServerCodec c, ж<Response> Ꮡr, any body) {
    error err = default!;

    {
        err = c.enc.Encode(Ꮡr.OrTypedNil()); if (err != default!) {
            if (c.encBuf.Flush() == default!) {
                // Gob couldn't encode the header. Should not happen, so if it does,
                // shut down the connection to signal that the connection is broken.
                log.Println(rpcGobErrorEncodingˢ, err);
                c.Close();
            }
            return err;
        }
    }
    {
        err = c.enc.Encode(body); if (err != default!) {
            if (c.encBuf.Flush() == default!) {
                // Was a gob problem encoding the body but the header has been written.
                // Shut down the connection to signal that the connection is broken.
                log.Println(rpcGobErrorEncodingBodyˢ, err);
                c.Close();
            }
            return err;
        }
    }
    return c.encBuf.Flush();
}

[GoRecv] internal static error Close(this ref gobServerCodec c) {
    if (c.closed) {
        // Only call c.rwc.Close once; otherwise the semantics are undefined.
        return default!;
    }
    c.closed = true;
    return c.rwc.Close();
}

// ServeConn runs the server on a single connection.
// ServeConn blocks, serving the connection until the client hangs up.
// The caller typically invokes ServeConn in a go statement.
// ServeConn uses the gob wire format (see package gob) on the
// connection. To use an alternate codec, use [ServeCodec].
// See [NewClient]'s comment for information about concurrent access.
public static void ServeConn(this ж<Server> Ꮡserver, io.ReadWriteCloser conn) {
    var buf = bufio.NewWriter(conn);
    var srv = Ꮡ(new gobServerCodec(
        rwc: conn,
        dec: gob.NewDecoder(conn),
        enc: gob.NewEncoder(new bufio_WriterжWriter(buf)),
        encBuf: buf
    ));
    Ꮡserver.ServeCodec(new gobServerCodecжServerCodec(srv));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object rpcˢ = (@string)"rpc:"u8;

// ServeCodec is like [ServeConn] but uses the specified codec to
// decode requests and encode responses.
public static void ServeCodec(this ж<Server> Ꮡserver, ServerCodec codec) {
    var sending = @new<sync.Mutex>();
    var wg = @new<sync.WaitGroup>();
    while (ᐧ) {
        ref var argv = ref heap<reflectꓸValue>(out var Ꮡargv);
        ref var replyv = ref heap<reflectꓸValue>(out var Ꮡreplyv);
        (var service, var mtype, var req, argv, replyv, var keepReading, var err) = Ꮡserver.readRequest(codec);
        if (err != default!) {
            if (debugLog && !AreEqual(err, io.EOF)) {
                log.Println(rpcˢ, err);
            }
            if (!keepReading) {
                break;
            }
            // send a response if we actually managed to read a header.
            if (req != nil) {
                Ꮡserver.sendResponse(sending, req, invalidRequest, codec, err.Error());
                Ꮡserver.freeRequest(req);
            }
            continue;
        }
        wg.Add(1);
        var serviceʗ1 = service;
        goǃ(serviceʗ1.call, Ꮡserver, sending, wg, mtype, req, argv, replyv, codec);
    }
    // We've seen that there are no more requests.
    // Wait for responses to be sent before closing codec.
    wg.Wait();
    codec.Close();
}

// ServeRequest is like [ServeCodec] but synchronously serves a single request.
// It does not close the codec upon completion.
public static error ServeRequest(this ж<Server> Ꮡserver, ServerCodec codec) {
    var sending = @new<sync.Mutex>();
    var (service, mtype, req, argv, replyv, keepReading, err) = Ꮡserver.readRequest(codec);
    if (err != default!) {
        if (!keepReading) {
            return err;
        }
        // send a response if we actually managed to read a header.
        if (req != nil) {
            Ꮡserver.sendResponse(sending, req, invalidRequest, codec, err.Error());
            Ꮡserver.freeRequest(req);
        }
        return err;
    }
    service.call(Ꮡserver, sending, nil, mtype, req, argv, replyv, codec);
    return default!;
}

internal static ж<Request> getRequest(this ж<Server> Ꮡserver) {
    ref var server = ref Ꮡserver.DerefOrNull();

    server.reqLock.Lock();
    var req = server.freeReq;
    if (req == nil){
        req = @new<Request>();
    } else {
        server.freeReq = req.Value.next;
        req.Value = new Request(nil);
    }
    server.reqLock.Unlock();
    return req;
}

internal static void freeRequest(this ж<Server> Ꮡserver, ж<Request> Ꮡreq) {
    ref var server = ref Ꮡserver.DerefOrNull();
    ref var req = ref Ꮡreq.DerefOrNull();

    server.reqLock.Lock();
    req.next = server.freeReq;
    server.freeReq = Ꮡreq;
    server.reqLock.Unlock();
}

internal static ж<Response> getResponse(this ж<Server> Ꮡserver) {
    ref var server = ref Ꮡserver.DerefOrNull();

    server.respLock.Lock();
    var resp = server.freeResp;
    if (resp == nil){
        resp = @new<Response>();
    } else {
        server.freeResp = resp.Value.next;
        resp.Value = new Response(nil);
    }
    server.respLock.Unlock();
    return resp;
}

internal static void freeResponse(this ж<Server> Ꮡserver, ж<Response> Ꮡresp) {
    ref var server = ref Ꮡserver.DerefOrNull();
    ref var resp = ref Ꮡresp.DerefOrNull();

    server.respLock.Lock();
    resp.next = server.freeResp;
    server.freeResp = Ꮡresp;
    server.respLock.Unlock();
}

internal static (ж<service> service, ж<methodType> mtype, ж<Request> req, reflectꓸValue argv, reflectꓸValue replyv, bool keepReading, error err) readRequest(this ж<Server> Ꮡserver, ServerCodec codec) {
    ж<service> service = default!;
    ж<methodType> mtype = default!;
    ж<Request> req = default!;
    reflectꓸValue argv = new(nil);
    reflectꓸValue replyv = new(nil);
    bool keepReading = default!;
    error err = default!;

    (service, mtype, req, keepReading, err) = Ꮡserver.readRequestHeader(codec);
    if (err != default!) {
        if (!keepReading) {
            return (service, mtype, req, argv, replyv, keepReading, err);
        }
        // discard body
        codec.ReadRequestBody(default!);
        return (service, mtype, req, argv, replyv, keepReading, err);
    }
    // Decode the argument value.
    var argIsValue = false; // if true, need to indirect before calling.
    if ((~mtype).ArgType.Kind() == reflect.ΔPointer){
        argv = reflect.New((~mtype).ArgType.Elem());
    } else {
        argv = reflect.New((~mtype).ArgType);
        argIsValue = true;
    }
    // argv guaranteed to be a pointer now.
    {
        err = codec.ReadRequestBody(argv.Interface()); if (err != default!) {
            return (service, mtype, req, argv, replyv, keepReading, err);
        }
    }
    if (argIsValue) {
        argv = argv.Elem();
    }
    replyv = reflect.New((~mtype).ReplyType.Elem());
    var exprᴛ1 = (~mtype).ReplyType.Elem().Kind();
    if (exprᴛ1 == reflect.Map) {
        replyv.Elem().Set(reflect.MakeMap((~mtype).ReplyType.Elem()));
    }
    else if (exprᴛ1 == reflect.ΔSlice) {
        replyv.Elem().Set(reflect.MakeSlice((~mtype).ReplyType.Elem(), 0, 0));
    }

    return (service, mtype, req, argv, replyv, keepReading, err);
}

internal static (ж<service> svc, ж<methodType> mtype, ж<Request> req, bool keepReading, error err) readRequestHeader(this ж<Server> Ꮡserver, ServerCodec codec) {
    ж<service> svc = default!;
    ж<methodType> mtype = default!;
    ж<Request> req = default!;
    bool keepReading = default!;
    error err = default!;

    ref var server = ref Ꮡserver.DerefOrNull();
    // Grab the request header.
    req = Ꮡserver.getRequest();
    err = codec.ReadRequestHeader(req);
    if (err != default!) {
        req = default!;
        if (AreEqual(err, io.EOF) || AreEqual(err, io.ErrUnexpectedEOF)) {
            return (svc, mtype, req, keepReading, err);
        }
        err = errors.New("rpc: server cannot decode request: "u8 + err.Error());
        return (svc, mtype, req, keepReading, err);
    }
    // We read the header successfully. If we see an error now,
    // we can still recover and move on to the next request.
    keepReading = true;
    nint dot = strings.LastIndex((~req).ServiceMethod, "."u8);
    if (dot < 0) {
        err = errors.New("rpc: service/method request ill-formed: "u8 + (~req).ServiceMethod);
        return (svc, mtype, req, keepReading, err);
    }
    @string serviceName = (~req).ServiceMethod.slice(0, dot);
    @string methodName = (~req).ServiceMethod.slice(dot + 1);
    // Look up the request.
    var (svci, ok) = Ꮡserver.of(Server.ᏑserviceMap).Load(serviceName);
    if (!ok) {
        err = errors.New("rpc: can't find service "u8 + (~req).ServiceMethod);
        return (svc, mtype, req, keepReading, err);
    }
    svc = svci._<ж<service>>();
    mtype = (~svc).method[methodName];
    if (mtype == nil) {
        err = errors.New("rpc: can't find method "u8 + (~req).ServiceMethod);
    }
    return (svc, mtype, req, keepReading, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object rpcServeAcceptˢ = (@string)"rpc.Serve: accept:"u8;

// Accept accepts connections on the listener and serves requests
// for each incoming connection. Accept blocks until the listener
// returns a non-nil error. The caller typically invokes Accept in a
// go statement.
public static void Accept(this ж<Server> Ꮡserver, net.Listener lis) {
    while (ᐧ) {
        var (conn, err) = lis.Accept();
        if (err != default!) {
            log.Print(rpcServeAcceptˢ, err.Error());
            return;
        }
        goǃ(Ꮡserver.ServeConn, new net_ConnᴠReadWriteCloser(conn));
    }
}

// Register publishes the receiver's methods in the [DefaultServer].
public static error Register(any rcvr) {
    return DefaultServer.Register(rcvr);
}

// RegisterName is like [Register] but uses the provided name for the type
// instead of the receiver's concrete type.
public static error RegisterName(@string name, any rcvr) {
    return DefaultServer.RegisterName(name, rcvr);
}

// A ServerCodec implements reading of RPC requests and writing of
// RPC responses for the server side of an RPC session.
// The server calls [ServerCodec.ReadRequestHeader] and [ServerCodec.ReadRequestBody] in pairs
// to read requests from the connection, and it calls [ServerCodec.WriteResponse] to
// write a response back. The server calls [ServerCodec.Close] when finished with the
// connection. ReadRequestBody may be called with a nil
// argument to force the body of the request to be read and discarded.
// See [NewClient]'s comment for information about concurrent access.
[GoType] partial interface ServerCodec :
    io.Closer
{
    error ReadRequestHeader(ж<Request> _);
    error ReadRequestBody(any _);
    error WriteResponse(ж<Response> _Δp0, any _Δp1);
}

// ServeConn runs the [DefaultServer] on a single connection.
// ServeConn blocks, serving the connection until the client hangs up.
// The caller typically invokes ServeConn in a go statement.
// ServeConn uses the gob wire format (see package gob) on the
// connection. To use an alternate codec, use [ServeCodec].
// See [NewClient]'s comment for information about concurrent access.
public static void ServeConn(io.ReadWriteCloser conn) {
    DefaultServer.ServeConn(conn);
}

// ServeCodec is like [ServeConn] but uses the specified codec to
// decode requests and encode responses.
public static void ServeCodec(ServerCodec codec) {
    DefaultServer.ServeCodec(codec);
}

// ServeRequest is like [ServeCodec] but synchronously serves a single request.
// It does not close the codec upon completion.
public static error ServeRequest(ServerCodec codec) {
    return DefaultServer.ServeRequest(codec);
}

// Accept accepts connections on the listener and serves requests
// to [DefaultServer] for each incoming connection.
// Accept blocks; the caller typically invokes it in a go statement.
public static void Accept(net.Listener lis) {
    DefaultServer.Accept(lis);
}

// Can connect to RPC service using HTTP CONNECT to rpcPath.
internal static @string connected = "200 Connected to Go RPC"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contentTypeˢ = "Content-Type"u8;
internal static readonly @string textPlainCharsetUtf8ˢ = "text/plain; charset=utf-8"u8;
internal static readonly @string mustConnectˢ = "405 must CONNECT\n"u8;
internal static readonly object rpcHijackingˢ = (@string)"rpc hijacking "u8;

// ServeHTTP implements an [http.Handler] that answers RPC requests.
public static void ServeHTTP(this ж<Server> Ꮡserver, Δhttp.ResponseWriter w, ж<Δhttp.Request> Ꮡreq) {
    ref var req = ref Ꮡreq.DerefOrNull();

    if (req.Method != "CONNECT"u8) {
        w.Header().Set(contentTypeˢ, textPlainCharsetUtf8ˢ);
        w.WriteHeader(Δhttp.StatusMethodNotAllowed);
        io.WriteString(new http_ResponseWriterᴠWriter(w), mustConnectˢ);
        return;
    }
    var (conn, _, err) = w._<Δhttp.Hijacker>().Hijack();
    if (err != default!) {
        log.Print(rpcHijackingˢ, req.RemoteAddr, (@string)": "u8, err.Error());
        return;
    }
    io.WriteString(new net_ConnᴠWriter(conn), "HTTP/1.0 "u8 + connected + "\n\n"u8);
    Ꮡserver.ServeConn(new net_ConnᴠReadWriteCloser(conn));
}

// HandleHTTP registers an HTTP handler for RPC messages on rpcPath,
// and a debugging handler on debugPath.
// It is still necessary to invoke [http.Serve](), typically in a go statement.
public static void HandleHTTP(this ж<Server> Ꮡserver, @string rpcPath, @string debugPath) {
    Δhttp.Handle(rpcPath, new ServerжΔHandler(Ꮡserver));
    Δhttp.Handle(debugPath, new debugHTTP(Ꮡserver));
}

// HandleHTTP registers an HTTP handler for RPC messages to [DefaultServer]
// on [DefaultRPCPath] and a debugging handler on [DefaultDebugPath].
// It is still necessary to invoke [http.Serve](), typically in a go statement.
public static void HandleHTTP() {
    DefaultServer.HandleHTTP(DefaultRPCPath, DefaultDebugPath);
}

} // end rpc_package
