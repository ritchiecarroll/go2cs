// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package jsonrpc implements a JSON-RPC 1.0 ClientCodec and ServerCodec
// for the rpc package.
// For JSON-RPC 2.0 support, see https://godoc.org/?q=json-rpc+2.0
namespace go.net.rpc;

using json = encoding.json_package;
using fmt = fmt_package;
using io = io_package;
using net = net_package;
using rpc = global::go.net.rpc_package;
using sync = sync_package;
using encoding;
using global::go.net;

partial class jsonrpc_package {

partial struct clientCodec {
    internal ж<json.Decoder> dec; // for reading JSON values
    internal ж<json.Encoder> enc; // for writing JSON values
    internal io.Closer c;
    // temporary work space
    internal clientRequest req;
    internal clientResponse resp;
    // JSON-RPC responses include the request id but not the request method.
    // Package rpc expects both.
    // We save the request method in pending when sending a request
    // and then look it up by request ID when filling out the rpc Response.
    internal sync.Mutex mutex;        // protects pending
    internal map<uint64, @string> pending; // map request id to method name
}

// NewClientCodec returns a new [rpc.ClientCodec] using JSON-RPC on conn.
public static rpc.ClientCodec NewClientCodec(io.ReadWriteCloser conn) {
    return new clientCodecжClientCodec(Ꮡ(new clientCodec(
        dec: json.NewDecoder(new io_ReadWriteCloserᴠReader(conn)),
        enc: json.NewEncoder(new io_ReadWriteCloserᴠWriter(conn)),
        c: new io_ReadWriteCloserᴠCloser(conn),
        pending: new map<uint64, @string>()
    )));
}

partial struct clientRequest {
    public @string Method; /*`json:"method"`*/
    public array<any> Params = new(1); /*`json:"params"`*/
    public uint64 Id; /*`json:"id"`*/
}

internal static error WriteRequest(this ж<clientCodec> Ꮡc, ж<rpc.Request> Ꮡr, any param) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var r = ref Ꮡr.DerefOrNull();

    c.mutex.Lock();
    c.pending[r.Seq] = r.ServiceMethod;
    c.mutex.Unlock();
    c.req.Method = r.ServiceMethod;
    c.req.Params[0] = param;
    c.req.Id = r.Seq;
    return c.enc.Encode(Ꮡc.of(clientCodec.Ꮡreq));
}

partial struct clientResponse {
    public uint64 Id; /*`json:"id"`*/
    public ж<json.RawMessage> Result; /*`json:"result"`*/
    public any Error; /*`json:"error"`*/
}

internal static void reset(this ref clientResponse r) {
    r.Id = 0;
    r.Result = default!;
    r.Error = default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unspecifiedErrorˢ = "unspecified error"u8;

internal static error ReadResponseHeader(this ж<clientCodec> Ꮡc, ж<rpc.Response> Ꮡr) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var r = ref Ꮡr.DerefOrNull();

    c.resp.reset();
    {
        var err = c.dec.Decode(Ꮡc.of(clientCodec.Ꮡresp)); if (err != default!) {
            return err;
        }
    }
    c.mutex.Lock();
    r.ServiceMethod = c.pending[c.resp.Id];
    delete(c.pending, c.resp.Id);
    c.mutex.Unlock();
    r.Error = ""u8;
    r.Seq = c.resp.Id;
    if (c.resp.Error != default! || c.resp.Result == nil) {
        var (x, ok) = c.resp.Error._<@string>(ᐧ);
        if (!ok) {
            return fmt.Errorf("invalid error %v"u8, c.resp.Error);
        }
        if (x == ""u8) {
            x = unspecifiedErrorˢ;
        }
        r.Error = x;
    }
    return default!;
}

internal static error ReadResponseBody(this ref clientCodec c, any x) {
    if (x == default!) {
        return default!;
    }
    return json.Unmarshal(c.resp.Result.ValueSlot, x);
}

internal static error Close(this ref clientCodec c) {
    return c.c.Close();
}

// NewClient returns a new [rpc.Client] to handle requests to the
// set of services at the other end of the connection.
public static ж<rpc.Client> NewClient(io.ReadWriteCloser conn) {
    return rpc.NewClientWithCodec(NewClientCodec(conn));
}

// Dial connects to a JSON-RPC server at the specified network address.
public static (ж<rpc.Client>, error) Dial(@string network, @string address) {
    var (conn, err) = net.Dial(network, address);
    if (err != default!) {
        return (default!, err);
    }
    return (NewClient(new net_ConnᴠReadWriteCloser(conn)), err);
}

} // end jsonrpc_package
