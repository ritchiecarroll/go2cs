// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.http;

using bytes = bytes_package;
using errors = errors_package;
using io = io_package;
using http = go.net.http_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using go.net;
using static go.net.http.fcgi_package;

partial class fcgi_internal_test_package {

partial struct sizeTestsᴛ1 /*dyn*/ {
    internal uint32 size;
    internal slice<byte> bytes;
}
internal static slice<sizeTestsᴛ1> sizeTests = [
    new(0, [0x00]),
    new(127, [0x7F]),
    new(128, [0x80, 0x00, 0x00, 0x80]),
    new(1000, [0x80, 0x00, 0x03, 0xE8]),
    new(33554431, [0x81, 0xFF, 0xFF, 0xFF])
];

public static void TestSize(ж<testing.T> t) {
    var b = new slice<byte>(4);
    foreach (var (i, test) in range(sizeTests)) {
        nint n = encodeSize(b, test.size);
        if (!bytes.Equal(b.slice(0, n), test.bytes)) {
            t.Errorf("%d expected %x, encoded %x", i, test.bytes, b);
        }
        (var size, n) = readSize(test.bytes);
        if (size != test.size) {
            t.Errorf("%d expected %d, read %d", i, test.size, size);
        }
        if (len(test.bytes) != n) {
            t.Errorf("%d did not consume all the bytes", i);
        }
    }
}

partial struct streamTestsᴛ1 /*dyn*/ {
    internal @string desc;
    internal global::go.net.http.fcgi_package.recType recType;
    internal uint16 reqId;
    internal slice<byte> content;
    internal slice<byte> raw;
}
internal static slice<streamTestsᴛ1> streamTests = slice<streamTestsᴛ1>([
    new("single record", typeStdout, 1, default,
        [1, (byte)typeStdout, 0, 1, 0, 0, 0, 0]
    ),
    // this data will have to be split into two records
    new("two records", typeStdin, 300, new slice<byte>(66000),
        bytes.Join(slice<slice<byte>>([
            // header for the first record
            [1, (byte)typeStdin, 0x01, 0x2C, 0xFF, 0xFF, 1, 0],
            new slice<byte>(65536),
            // header for the second
            [1, (byte)typeStdin, 0x01, 0x2C, 0x01, 0xD1, 7, 0],
            new slice<byte>(472),
            // header for the empty record
            [1, (byte)typeStdin, 0x01, 0x2C, 0, 0, 0, 0]
        ]),
            default)
    )
]);

internal partial struct nilCloser {
    /*embed*/ public io_package.ReadWriter ReadWriter;
}

internal static error Close(this ref nilCloser c) { return default; }

public static void TestStreams(ж<testing.T> t) {
    ref var rec = ref heap(new global::go.net.http.fcgi_package.record(), out var Ꮡrec);
outer:
    foreach (var (_, test) in range(streamTests)) {
        var buf = bytes.NewBuffer(test.raw);
        slice<byte> content = default;
        while (buf.Len() > 0) {
            var err = Ꮡrec.read(new fcgi_internal_test_package.bytes_BufferжReader(buf)); if (err != nil) {
                t.Errorf("%s: error reading record: %v", test.desc, err);
                goto continue_outer;
            }
            content = appendꓸꓸꓸ(content, rec.content());
        }
        if (rec.h.Type != test.recType) {
            t.Errorf("%s: got type %d expected %d", test.desc, rec.h.Type, test.recType);
            continue;
        }
        if (rec.h.Id != test.reqId) {
            t.Errorf("%s: got request ID %d expected %d", test.desc, rec.h.Id, test.reqId);
            continue;
        }
        if (!bytes.Equal(content, test.content)) {
            t.Errorf("%s: read wrong content", test.desc);
            continue;
        }
        buf.Reset();
        var c = newConn(new fcgi_internal_test_package.nilCloserжReadWriteCloser(Ꮡ(new nilCloser(new fcgi_internal_test_package.bytes_BufferжReadWriter(buf)))));
        var w = newWriter(c, test.recType, test.reqId);
        {
            var (_, err) = w.Value.Writer.Value.Write(test.content); if (err != nil) {
                t.Errorf("%s: error writing record: %v", test.desc, err);
                continue;
            }
        }
        {
            var err = w.Close(); if (err != nil) {
                t.Errorf("%s: error closing stream: %v", test.desc, err);
                continue;
            }
        }
        if (!bytes.Equal(buf.Bytes(), test.raw)) {
            t.Errorf("%s: wrote wrong content", test.desc);
        }
continue_outer:;
    }
break_outer:;
}

internal partial struct writeOnlyConn {
    internal slice<byte> buf;
}

internal static (nint, error) Write(this ref writeOnlyConn c, slice<byte> p) {
    c.buf = appendꓸꓸꓸ(c.buf, p);
    return (len(p), default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string connIsWriteOnlyˢ => ref connIsWriteOnlyˢᵇ;
private static readonly @string connIsWriteOnlyˢᵇ = "conn is write-only"u8;

internal static (nint, error) Read(this ref writeOnlyConn c, slice<byte> p) {
    return (0, errors.New(connIsWriteOnlyˢ));
}

internal static error Close(this ref writeOnlyConn c) {
    return default;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ => ref wantᶜᵇ;
private static readonly @string wantᶜᵇ = ((@string)(new byte[]{0x01, 0x0a, 0x00, 0x00, 0x00, 0x12, 0x06, 0x00, 0x0f, 0x01, 0x46, 0x43, 0x47, 0x49, 0x5f, 0x4d, 0x50, 0x58, 0x53, 0x5f, 0x43, 0x4f, 0x4e, 0x4e, 0x53, 0x31, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x0a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}));

public static void TestGetValues(ж<testing.T> t) {
    ref var rec = ref heap(new global::go.net.http.fcgi_package.record(), out var Ꮡrec);
    rec.h.Type = typeGetValues;

    var wc = @new<writeOnlyConn>();
    var c = newChild(new fcgi_internal_test_package.writeOnlyConnжReadWriteCloser(wc), default);
    var err = c.handleRecord(Ꮡrec);
    if (err != nil) {
        t.Fatalf("handleRecord: %v", err);
    }

    @string want = wantᶜ;
    @string got = (@string)wc.buf; if (got != want) {
        t.Errorf(" got: %q\nwant: %q\n", got, want);
    }
}

internal static slice<byte> nameValuePair11(@string nameData, @string valueData) {
    return bytes.Join(
        slice<slice<byte>>([
            slice<byte>([(byte)len(nameData), (byte)len(valueData)]),
            slice<byte>(nameData),
            slice<byte>(valueData)
        ]),
        default);
}

internal static slice<byte> makeRecord(global::go.net.http.fcgi_package.recType recordType, uint16 requestId, slice<byte> contentData) {
    var requestIdB1 = (byte)(requestId >> 8);
    var requestIdB0 = (byte)requestId;

    nint contentLength = len(contentData);
    var contentLengthB1 = (byte)(contentLength >> 8);
    var contentLengthB0 = (byte)contentLength;
    return bytes.Join([
        [1, (byte)recordType, requestIdB1, requestIdB0, contentLengthB1,
            contentLengthB0, 0, 0],
        contentData
    ],
        default);
}

// a series of FastCGI records that start a request and begin sending the
// request body
internal static slice<byte> streamBeginTypeStdin = bytes.Join(slice<slice<byte>>([
    // set up request 1
    makeRecord(typeBeginRequest, 1,
        [0, (byte)roleResponder, 0, 0, 0, 0, 0, 0]),
    // add required parameters to request 1
    makeRecord(typeParams, 1, nameValuePair11("REQUEST_METHOD", "GET")),
    makeRecord(typeParams, 1, nameValuePair11("SERVER_PROTOCOL", "HTTP/1.1")),
    makeRecord(typeParams, 1, default),
    // begin sending body of request 1
    makeRecord(typeStdin, 1, slice<byte>("0123456789abcdef"u8))
]),
    default);

partial struct cleanUpTestsᴛ1 /*dyn*/ {
    internal slice<byte> input;
    internal error err;
}
internal static slice<cleanUpTestsᴛ1> cleanUpTests;
internal static void initᴛcleanUpTests() { cleanUpTests = slice<cleanUpTestsᴛ1>([
    // confirm that child.handleRecord closes req.pw after aborting req
    new(
        bytes.Join(slice<slice<byte>>([
            streamBeginTypeStdin,
            makeRecord(typeAbortRequest, 1, default)
        ]),
            default),
        ErrRequestAborted
    ),
    // confirm that child.serve closes all pipes after error reading record
    new(
        bytes.Join([
            streamBeginTypeStdin,
            default
        ],
            default),
        ErrConnClosed
    )
]); }

internal partial struct nopWriteCloser {
    /*embed*/ public io_package.Reader Reader;
}

internal static (nint, error) Write(this nopWriteCloser _, slice<byte> buf) {
    return (len(buf), default);
}

internal static error Close(this nopWriteCloser _) {
    return default;
}

// Test that child.serve closes the bodies of aborted requests and closes the
// bodies of all requests before returning. Causes deadlock if either condition
// isn't met. See issue 6934.
public static void TestChildServeCleansUp(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(cleanUpTests)) {
        ref var tt = ref heap(new cleanUpTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var input = new slice<byte>(len(tt.input));
        copy(input, tt.input);
        var rc = new nopWriteCloser(new fcgi_internal_test_package.bytes_ReaderжReader(bytes.NewReader(input)));
        var done = new channel<EmptyStruct>(0);
        var doneʗ1 = done;
        var ttʗ1 = tt;
        var c = newChild(rc, new fcgi_internal_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            // block on reading body of request
            var (_, err) = io.Copy(io.Discard, new fcgi_internal_test_package.io_ReadCloserᴠReader(r.Body));
            if (!AreEqual(err, ttʗ1.err)) {
                t.Errorf("Expected %#v, got %#v", ttʗ1.err, err);
            }
            // not reached if body of request isn't closed
            close(doneʗ1);
        })));
        c.serve();
        // wait for body of request to be closed or all goroutines to block
        ᐸꟷ(done);
    }
}

internal partial struct rwNopCloser {
    /*embed*/ public io_package.Reader Reader;
    /*embed*/ public io_package.Writer Writer;
}

internal static error Close(this rwNopCloser _) {
    return default;
}

// Verifies it doesn't crash. 	Issue 11824.
public static void TestMalformedParams(ж<testing.T> t) {
    var input = slice<byte>([
        // beginRequest, requestId=1, contentLength=8, role=1, keepConn=1
        1, 1, 0, 1, 0, 8, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0,
        // params, requestId=1, contentLength=10, k1Len=50, v1Len=50 (malformed, wrong length)
        1, 4, 0, 1, 0, 10, 0, 0, 50, 50, 3, 4, 5, 6, 7, 8, 9, 10,
        // end of params
        1, 4, 0, 1, 0, 0, 0, 0
    ]);
    var rw = new rwNopCloser(new fcgi_internal_test_package.bytes_ReaderжReader(bytes.NewReader(input)), io.Discard);
    var c = newChild(rw, new http.ServeMuxжΔHandler(http.DefaultServeMux));
    c.serve();
}

// a series of FastCGI records that start and end a request
internal static slice<byte> streamFullRequestStdin = bytes.Join(slice<slice<byte>>([
    // set up request
    makeRecord(typeBeginRequest, 1,
        [0, (byte)roleResponder, 0, 0, 0, 0, 0, 0]),
    // add required parameters
    makeRecord(typeParams, 1, nameValuePair11("REQUEST_METHOD", "GET")),
    makeRecord(typeParams, 1, nameValuePair11("SERVER_PROTOCOL", "HTTP/1.1")),
    // set optional parameters
    makeRecord(typeParams, 1, nameValuePair11("REMOTE_USER", "jane.doe")),
    makeRecord(typeParams, 1, nameValuePair11("QUERY_STRING", "/foo/bar")),
    makeRecord(typeParams, 1, default),
    // begin sending body of request
    makeRecord(typeStdin, 1, slice<byte>("0123456789abcdef"u8)),
    // end request
    makeRecord(typeEndRequest, 1, default)
]),
    default);

partial struct envVarTestsᴛ1 /*dyn*/ {
    internal slice<byte> input;
    internal @string envVar;
    internal @string expectedVal;
    internal bool expectedFilteredOut;
}
internal static slice<envVarTestsᴛ1> envVarTests = [
    new(
        streamFullRequestStdin,
        "REMOTE_USER",
        "jane.doe",
        false
    ),
    new(
        streamFullRequestStdin,
        "QUERY_STRING",
        "",
        true
    )
];

// Test that environment variables set for a request can be
// read by a handler. Ensures that variables not set will not
// be exposed to a handler.
public static void TestChildServeReadsEnvVars(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(envVarTests)) {
        ref var tt = ref heap(new envVarTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var input = new slice<byte>(len(tt.input));
        copy(input, tt.input);
        var rc = new nopWriteCloser(new fcgi_internal_test_package.bytes_ReaderжReader(bytes.NewReader(input)));
        var done = new channel<EmptyStruct>(0);
        var doneʗ1 = done;
        var ttʗ1 = tt;
        var c = newChild(rc, new fcgi_internal_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            var env = ProcessEnv(r);
            var (_, ok) = env[ttʗ1.envVar, ꟷ]; if (ok && ttʗ1.expectedFilteredOut) {
                t.Errorf("Expected environment variable %s to not be set, but set to %s",
                    ttʗ1.envVar, env[ttʗ1.envVar]);
            } else if (env[ttʗ1.envVar] != ttʗ1.expectedVal) {
                t.Errorf("Expected %s, got %s", ttʗ1.expectedVal, env[ttʗ1.envVar]);
            }
            close(doneʗ1);
        })));
        c.serve();
        ᐸꟷ(done);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gopherˢ => ref gopherˢᵇ;
private static readonly @string gopherˢᵇ = "gopher"u8;

internal partial struct TestResponseWriterSniffsContentType_type /*dyn*/ {
    internal @string name;
    internal @string body;
    internal @string wantCT;
}

public static void TestResponseWriterSniffsContentType(ж<testing.T> t) {
    slice<TestResponseWriterSniffsContentType_type> tests = slice<TestResponseWriterSniffsContentType_type>([
        new(
            name: "no body",
            wantCT: "text/plain; charset=utf-8"
        ),
        new(
            name: "html",
            body: "<html><head><title>test page</title></head><body>This is a body</body></html>",
            wantCT: "text/html; charset=utf-8"
        ),
        new(
            name: "text",
            body: strings.Repeat(gopherˢ, 86),
            wantCT: "text/plain; charset=utf-8"
        ),
        new(
            name: "jpg",
            body: ((@string)(new byte[]{0xff, 0xd8, 0xff})) + strings.Repeat("B", 1024),
            wantCT: "image/jpeg"
        )
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestResponseWriterSniffsContentType_type(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            var input = new slice<byte>(len(streamFullRequestStdin));
            copy(input, streamFullRequestStdin);
            var rc = new nopWriteCloser(new fcgi_internal_test_package.bytes_ReaderжReader(bytes.NewReader(input)));
            var done = new channel<EmptyStruct>(0);
            ref var resp = ref heap<ж<global::go.net.http.fcgi_package.response>>(out var Ꮡresp);
            var doneʗ1 = done;
            var ttʗ2 = ttʗ1;
            var c = newChild(rc, new fcgi_internal_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
                io.WriteString(new fcgi_internal_test_package.http_ResponseWriterᴠWriter(w), ttʗ2.body);
                Ꮡresp.ValueSlot = w._<ж<global::go.net.http.fcgi_package.response>>();
                close(doneʗ1);
            })));
            c.serve();
            ᐸꟷ(done);
            @string got = Ꮡresp.ValueSlot.Header().Get(contentTypeˢ); if (got != ttʗ1.wantCT) {
                tΔ1.Errorf("got a Content-Type of %q; expected it to start with %q", got, ttʗ1.wantCT);
            }
        });
    }
}

internal partial struct signalingNopWriteCloser {
    /*embed*/ public io_package.ReadCloser ReadCloser;
    internal channel<bool> closed;
}

// Go method set entry for the promoted 'ReadCloser.Read()' - provided ONLY by the embedded
// interface field in *signalingNopWriteCloser's method set; see the pointer-only satisfaction record.
internal static (nint, error) Read(this signalingNopWriteCloser recvᴛ, slice<byte> p) => recvᴛ.ReadCloser.Read(p);

internal static (nint, error) Write(this ref signalingNopWriteCloser _, slice<byte> buf) {
    return (len(buf), default);
}

internal static error Close(this ref signalingNopWriteCloser rc) {
    close(rc.closed);
    return rc.ReadCloser.Close();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object fastCGIChildClosedˢ = (@string)"FastCGI child closed connection"u8;

// Test whether server properly closes connection when processing slow
// requests
public static partial void TestSlowRequest(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var (pr, pw) = io.Pipe();

        var writerDone = new channel<EmptyStruct>(0);
        var pwʗ1 = pw;
        var writerDoneʗ1 = writerDone;
        goǃ(() => {
            foreach (var (_, buf) in range(slice<slice<byte>>([
                streamBeginTypeStdin,
                makeRecord(typeStdin, 1, default)
            ]))) {
                pwʗ1.Write(buf);
                time.Sleep(100 * time.Millisecond);
            }
            close(writerDoneʗ1);
        });
        var pwʗ2 = pw;
        var writerDoneʗ2 = writerDone;
        defer(() => {
            ᐸꟷ(writerDoneʗ2);
            pwʗ2.Close();
        }, ref ᒐ);

        var rc = Ꮡ(new signalingNopWriteCloser(new io.PipeReaderжReadCloser(pr), new channel<bool>(0)));
        var handlerDone = new channel<bool>(0);

        var handlerDoneʗ1 = handlerDone;
        var c = newChild(new fcgi_internal_test_package.signalingNopWriteCloserжReadWriteCloser(rc), new fcgi_internal_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.WriteHeader(200);
            close(handlerDoneʗ1);
        })));
        c.serve();

        ᐸꟷ(handlerDone);
        ᐸꟷ(rc.closed);
        t.Log(fastCGIChildClosedˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end fcgi_internal_test_package
