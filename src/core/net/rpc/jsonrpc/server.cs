// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.rpc;

using json = encoding.json_package;
using errors = errors_package;
using io = io_package;
using rpc = global::go.net.rpc_package;
using sync = sync_package;
using encoding;
using global::go.net;

partial class jsonrpc_package {

internal static error errMissingParams = errors.New("jsonrpc: request body missing params"u8);

partial struct serverCodec {
    internal ж<json.Decoder> dec; // for reading JSON values
    internal ж<json.Encoder> enc; // for writing JSON values
    internal io.Closer c;
    // temporary work space
    internal serverRequest req;
    // JSON-RPC clients can use arbitrary json values as request IDs.
    // Package rpc expects uint64 request IDs.
    // We assign uint64 sequence numbers to incoming requests
    // but save the original request ID in the pending map.
    // When rpc responds, we use the sequence number in
    // the response to find the original request ID.
    internal sync.Mutex mutex; // protects seq, pending
    internal uint64 seq;
    internal map<uint64, ж<json.RawMessage>> pending;
}

// NewServerCodec returns a new [rpc.ServerCodec] using JSON-RPC on conn.
public static rpc.ServerCodec NewServerCodec(io.ReadWriteCloser conn) {
    return new serverCodecжServerCodec(Ꮡ(new serverCodec(
        dec: json.NewDecoder(conn),
        enc: json.NewEncoder(conn),
        c: conn,
        pending: new map<uint64, ж<json.RawMessage>>()
    )));
}

partial struct serverRequest {
    public @string Method; /*`json:"method"`*/
    public ж<json.RawMessage> Params; /*`json:"params"`*/
    public ж<json.RawMessage> Id; /*`json:"id"`*/
}

internal static void reset(this ref serverRequest r) {
    r.Method = ""u8;
    r.Params = default!;
    r.Id = default!;
}

partial struct serverResponse {
    public ж<json.RawMessage> Id; /*`json:"id"`*/
    public any Result; /*`json:"result"`*/
    public any Error; /*`json:"error"`*/
}

internal static error ReadRequestHeader(this ж<serverCodec> Ꮡc, ж<rpc.Request> Ꮡr) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var r = ref Ꮡr.DerefOrNull();

    c.req.reset();
    {
        var err = c.dec.Decode(Ꮡc.of(serverCodec.Ꮡreq)); if (err != default!) {
            return err;
        }
    }
    r.ServiceMethod = c.req.Method;
    // JSON request id can be any JSON value;
    // RPC package expects uint64.  Translate to
    // internal uint64 and save JSON on the side.
    c.mutex.Lock();
    c.seq++;
    c.pending[c.seq] = c.req.Id;
    c.req.Id = default!;
    r.Seq = c.seq;
    c.mutex.Unlock();
    return default!;
}

internal static error ReadRequestBody(this ref serverCodec c, any x) {
    if (x == default!) {
        return default!;
    }
    if (c.req.Params == nil) {
        return errMissingParams;
    }
    // JSON params is array value.
    // RPC params is struct.
    // Unmarshal into array containing struct for now.
    // Should think about making RPC more general.
    ref var @params = ref heap(new array<any>(1), out var Ꮡparams);
    @params[0] = x;
    return json.Unmarshal(c.req.Params.ValueSlot, Ꮡparams);
}

internal static ж<json.RawMessage> Ꮡnull = new StandardBox<json.RawMessage>(((json.RawMessage)slice<byte>("null"u8)));
internal static ref json.RawMessage @null => ref Ꮡnull.ValueSlot;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidSequenceNumberInˢ = "invalid sequence number in response"u8;

internal static error WriteResponse(this ж<serverCodec> Ꮡc, ж<rpc.Response> Ꮡr, any x) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var r = ref Ꮡr.DerefOrNull();

    c.mutex.Lock();
    var (b, ok) = c.pending[r.Seq, ꟷ];
    if (!ok) {
        c.mutex.Unlock();
        return errors.New(invalidSequenceNumberInˢ);
    }
    delete(c.pending, r.Seq);
    c.mutex.Unlock();
    if (b == nil) {
        // Invalid request so no id. Use JSON null.
        b = Ꮡnull;
    }
    var resp = new serverResponse(Id: b);
    if (r.Error == ""u8){
        resp.Result = x;
    } else {
        resp.Error = r.Error;
    }
    return c.enc.Encode(resp);
}

internal static error Close(this ref serverCodec c) {
    return c.c.Close();
}

// ServeConn runs the JSON-RPC server on a single connection.
// ServeConn blocks, serving the connection until the client hangs up.
// The caller typically invokes ServeConn in a go statement.
public static void ServeConn(io.ReadWriteCloser conn) {
    rpc.ServeCodec(NewServerCodec(conn));
}

} // end jsonrpc_package
