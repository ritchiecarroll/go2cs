// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bufio = bufio_package;
using bytes = bytes_package;
using rand = crypto.rand_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using crypto;
using static global::go.net.http_package;

partial class http_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string foobarˢ => ref foobarˢᵇ;
private static readonly @string foobarˢᵇ = "foobar"u8;

public static void TestBodyReadBadTrailer(ж<testing.T> t) {
    var b = Ꮡ(new body(
        src: new http_test_package.strings_ReaderжReader(strings.NewReader(foobarˢ)),
        hdr: true, // force reading the trailer
        r: bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader(""u8)))
    ));
    var buf = new slice<byte>(7);
    var (n, err) = b.Read(buf[..3]);
    @string got = (@string)(buf.slice(0, n));
    if (got != "foo"u8 || err != nil) {
        t.Fatalf(@"first Read = %d (%q), %v; want 3 (""foo"")", n, got, err);
    }

    (n, err) = b.Read(buf[..]);
    got = (@string)(buf.slice(0, n));
    if (got != "bar"u8 || err != nil) {
        t.Fatalf(@"second Read = %d (%q), %v; want 3 (""bar"")", n, got, err);
    }

    (n, err) = b.Read(buf[..]);
    got = (@string)(buf.slice(0, n));
    if (err == nil) {
        t.Errorf("final Read was successful (%q), expected error from trailer read", got);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string bodyHereContinuedˢ => ref bodyHereContinuedˢᵇ;
private static readonly @string bodyHereContinuedˢᵇ = "Body here\ncontinued"u8;

public static void TestFinalChunkedBodyReadEOF(ж<testing.T> t) {
    var (res, err) = ReadResponse(bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader(
        "HTTP/1.1 200 OK\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "\r\n"u8 + "0a\r\n"u8 + "Body here\n\r\n"u8 + "09\r\n"u8 + "continued\r\n"u8 + "0\r\n"u8 + "\r\n"u8))), nil);
    if (err != nil) {
        t.Fatal(err);
    }
    @string want = bodyHereContinuedˢ;
    var buf = new slice<byte>(builtin.len(want));
    (var n, err) = res.Body.Read(buf);
    if (n != builtin.len(want) || !AreEqual(err, io.EOF)) {
        t.Logf("body = %#v", res.Body);
        t.Errorf("Read = %v, %v; want %d, EOF", n, err, builtin.len(want));
    }
    if (((sstring)buf) != want) {
        t.Errorf("buf = %q; want %q", buf, want);
    }
}

internal partial struct TestDetectInMemoryReaders_tests /*dyn*/ {
    internal io.Reader r;
    internal bool want;
}

public static void TestDetectInMemoryReaders(ж<testing.T> t) {
    var (pr, _) = io.Pipe();
    var tests = slice<TestDetectInMemoryReaders_tests>([
        new(new io.PipeReaderжReader(pr), false),

        new(new http_test_package.bytes_ReaderжReader(bytes.NewReader(default)), true),
        new(new http_test_package.bytes_BufferжReader(bytes.NewBuffer(default)), true),
        new(new http_test_package.strings_ReaderжReader(strings.NewReader(""u8)), true),

        new(io.NopCloser(new io.PipeReaderжReader(pr)), false),

        new(io.NopCloser(new http_test_package.bytes_ReaderжReader(bytes.NewReader(default))), true),
        new(io.NopCloser(new http_test_package.bytes_BufferжReader(bytes.NewBuffer(default))), true),
        new(io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(""u8))), true)
    ]);
    foreach (var (i, tt) in range(tests)) {
        var got = isKnownInMemoryReader(tt.r);
        if (got != tt.want) {
            t.Errorf("%d: got = %v; want %v", i, got, tt.want);
        }
    }
}

internal partial struct mockTransferWriter {
    public io.Reader CalledReader;
    public bool WriteCalled;
}

internal static io.ReaderFrom _ᴛ1ʗ = new http_internal_test_package.mockTransferWriterжReaderFrom(((ж<mockTransferWriter>)nil));

internal static (int64, error) ReadFrom(this ref mockTransferWriter w, io.Reader r) {
    w.CalledReader = r;
    return io.Copy(io.Discard, r);
}

internal static (nint, error) Write(this ref mockTransferWriter w, slice<byte> p) {
    w.WriteCalled = true;
    return io.Discard.Write(p);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string netHttpNewfilefuncˢ => ref netHttpNewfilefuncˢᵇ;
private static readonly @string netHttpNewfilefuncˢᵇ = "net-http-newfilefunc"u8;
internal static readonly object didNotCallReadFromˢ = (@string)"did not call ReadFrom"u8;
internal static readonly object didNotInvokeWriteˢ = (@string)"did not invoke Write"u8;

internal partial struct TestTransferWriterWriteBodyReaderTypes_cases /*dyn*/ {
    internal @string name;
    internal Func<(io.Reader, Action, error)> bodyFunc;
    internal @string method;
    internal int64 contentLength;
    internal slice<@string> transferEncoding;
    internal bool limitedReader;
    internal reflectꓸType expectedReader;
    internal bool expectedWrite;
}

public static void TestTransferWriterWriteBodyReaderTypes(ж<testing.T> t) {
    var fileType = reflect.TypeFor<ж<os.File>>();
    var bufferType = reflect.TypeFor<ж<bytes.Buffer>>();

    var nBytes = (int64)((int64)1 << 10);
    var newFileFunc = (io.Reader r, Action done, error err) () => {
        Action done = default;
        error err = default;
        (var f, err) = os.CreateTemp(""u8, netHttpNewfilefuncˢ);
        if (err != nil) {
            return (default, default, err);
        }

        // Write some bytes to the file to enable reading.
        var (_, errΔ1) = io.CopyN(new os.FileжWriter(f), rand.Reader, nBytes); if (errΔ1 != nil) {
            return (default, default, fmt.Errorf("failed to write data to file: %v"u8, errΔ1));
        }
        var (_, errΔ2) = f.Seek(0, 0); if (errΔ2 != nil) {
            return (default, default, fmt.Errorf("failed to seek to front: %v"u8, errΔ2));
        }

        var fʗ1 = f;
        done = () => {
            fʗ1.Close();
            os.Remove(fʗ1.Name());
        };

        return (new http_test_package.os_FileжReader(f), done, default);
    };

    var newBufferFunc = (io.Reader, Action, error) () => (new http_test_package.bytes_BufferжReader(bytes.NewBuffer(new slice<byte>((nint)(nBytes)))), () => {
        }, default);

            var newFileFuncʗ1 = newFileFunc;

            var newBufferFuncʗ1 = newBufferFunc;
    var cases = slice<TestTransferWriterWriteBodyReaderTypes_cases>([
        new(
            name: "file, non-chunked, size set",
            bodyFunc: newFileFunc,
            method: "PUT",
            contentLength: nBytes,
            limitedReader: true,
            expectedReader: fileType
        ),
        new(
            name: "file, non-chunked, size set, nopCloser wrapped",
            method: "PUT",
            bodyFunc: () => {
                var (r, cleanup, err) = newFileFuncʗ1();
                return (io.NopCloser(r), cleanup, err);
            },
            contentLength: nBytes,
            limitedReader: true,
            expectedReader: fileType
        ),
        new(
            name: "file, non-chunked, negative size",
            method: "PUT",
            bodyFunc: newFileFunc,
            contentLength: -1,
            expectedReader: fileType
        ),
        new(
            name: "file, non-chunked, CONNECT, negative size",
            method: "CONNECT",
            bodyFunc: newFileFunc,
            contentLength: -1,
            expectedReader: fileType
        ),
        new(
            name: "file, chunked",
            method: "PUT",
            bodyFunc: newFileFunc,
            transferEncoding: ["chunked"],
            expectedWrite: true
        ),
        new(
            name: "buffer, non-chunked, size set",
            bodyFunc: newBufferFunc,
            method: "PUT",
            contentLength: nBytes,
            limitedReader: true,
            expectedReader: bufferType
        ),
        new(
            name: "buffer, non-chunked, size set, nopCloser wrapped",
            method: "PUT",
            bodyFunc: () => {
                var (r, cleanup, err) = newBufferFuncʗ1();
                return (io.NopCloser(r), cleanup, err);
            },
            contentLength: nBytes,
            limitedReader: true,
            expectedReader: bufferType
        ),
        new(
            name: "buffer, non-chunked, negative size",
            method: "PUT",
            bodyFunc: newBufferFunc,
            contentLength: -1,
            expectedWrite: true
        ),
        new(
            name: "buffer, non-chunked, CONNECT, negative size",
            method: "CONNECT",
            bodyFunc: newBufferFunc,
            contentLength: -1,
            expectedWrite: true
        ),
        new(
            name: "buffer, chunked",
            method: "PUT",
            bodyFunc: newBufferFunc,
            transferEncoding: ["chunked"],
            expectedWrite: true
        )
    ]);

    foreach (var (_, vᴛ1) in range(cases)) {
        ref var tc = ref heap(new TestTransferWriterWriteBodyReaderTypes_cases(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        t.Run(tc.name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var (body, cleanup, err) = tcʗ1.bodyFunc();
                if (err != nil) {
                    tΔ1.Fatal(err);
                }
                var cleanupʗ1 = cleanup;
                defer(cleanupʗ1, ref ᒐ, recovers: true);

                var mw = Ꮡ(new mockTransferWriter());
                var tw = Ꮡ(new transferWriter(
                    Body: body,
                    ContentLength: tcʗ1.contentLength,
                    TransferEncoding: tcʗ1.transferEncoding
                ));

                var errΔ1 = tw.writeBody(new http_internal_test_package.mockTransferWriterжWriter(mw)); if (errΔ1 != nil) {
                    tΔ1.Fatal(errΔ1);
                }

                if (tcʗ1.expectedReader != nil) {
                    if (mw.CalledReader == nil) {
                        tΔ1.Fatal(didNotCallReadFromˢ);
                    }

                    reflectꓸType actualReader = default;
                    var (lr, ok) = mw.CalledReader._<ж<io.LimitedReader>>(ᐧ);
                    if (ok && tcʗ1.limitedReader) {
                        actualReader = reflect.TypeOf(lr.R);
                    } else {
                        actualReader = reflect.TypeOf(mw.CalledReader);
                        // We have to handle this special case for genericWriteTo in os,
                        // this struct is introduced to support a zero-copy optimization,
                        // check out https://go.dev/issue/58808 for details.
                        if (actualReader.Kind() == reflect.Struct && actualReader.PkgPath() == "os"u8 && actualReader.Name() == "fileWithoutWriteTo"u8) {
                            actualReader = actualReader.Field(1).Type;
                        }
                    }

                    if (!AreEqual(tcʗ1.expectedReader, actualReader)) {
                        tΔ1.Fatalf("got reader %s want %s", actualReader, tcʗ1.expectedReader);
                    }
                }

                if (tcʗ1.expectedWrite && !mw.WriteCalled) {
                    tΔ1.Fatal(didNotInvokeWriteˢ);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

internal partial struct TestParseTransferEncoding_tests /*dyn*/ {
    internal global::go.net.http_package.ΔHeader hdr;
    internal error wantErr;
}

public static void TestParseTransferEncoding(ж<testing.T> t) {
    var tests = slice<TestParseTransferEncoding_tests>([
        new(
            hdr: new ΔHeader(new map<@string, slice<@string>>{["Transfer-Encoding"] = ["fugazi"]}),
            wantErr: new global::go.net.http_package.unsupportedTEErrorжerror(Ꮡ(new unsupportedTEError(@"unsupported transfer encoding: ""fugazi""")))
        ),
        new(
            hdr: new ΔHeader(new map<@string, slice<@string>>{["Transfer-Encoding"] = ["chunked, chunked", "identity", "chunked"]}),
            wantErr: new global::go.net.http_package.unsupportedTEErrorжerror(Ꮡ(new unsupportedTEError(@"too many transfer encodings: [""chunked, chunked"" ""identity"" ""chunked""]")))
        ),
        new(
            hdr: new ΔHeader(new map<@string, slice<@string>>{["Transfer-Encoding"] = [""]}),
            wantErr: new global::go.net.http_package.unsupportedTEErrorжerror(Ꮡ(new unsupportedTEError(@"unsupported transfer encoding: """"")))
        ),
        new(
            hdr: new ΔHeader(new map<@string, slice<@string>>{["Transfer-Encoding"] = ["chunked, identity"]}),
            wantErr: new global::go.net.http_package.unsupportedTEErrorжerror(Ꮡ(new unsupportedTEError(@"unsupported transfer encoding: ""chunked, identity""")))
        ),
        new(
            hdr: new ΔHeader(new map<@string, slice<@string>>{["Transfer-Encoding"] = ["chunked", "identity"]}),
            wantErr: new global::go.net.http_package.unsupportedTEErrorжerror(Ꮡ(new unsupportedTEError(@"too many transfer encodings: [""chunked"" ""identity""]")))
        ),
        new(
            hdr: new ΔHeader(new map<@string, slice<@string>>{["Transfer-Encoding"] = [((@string)(new byte[]{0x0b, 0x63, 0x68, 0x75, 0x6e, 0x6b, 0x65, 0x64}))]}),
            wantErr: new global::go.net.http_package.unsupportedTEErrorжerror(Ꮡ(new unsupportedTEError(@"unsupported transfer encoding: ""\vchunked""")))
        ),
        new(
            hdr: new ΔHeader(new map<@string, slice<@string>>{["Transfer-Encoding"] = ["chunked"]}),
            wantErr: default
        )
    ]);

    foreach (var (i, tt) in range(tests)) {
        var tr = Ꮡ(new transferReader(
            Header: tt.hdr,
            ProtoMajor: 1,
            ProtoMinor: 1
        ));
        var gotErr = tr.parseTransferEncoding();
        if (!reflect.DeepEqual(gotErr, tt.wantErr)) {
            t.Errorf("%d.\ngot error:\n%v\nwant error:\n%v\n\n", i, gotErr, tt.wantErr);
        }
    }
}

internal partial struct TestParseContentLength_tests /*dyn*/ {
    internal @string cl;
    internal error wantErr;
}

// issue 39017 - disallow Content-Length values such as "+3"
public static void TestParseContentLength(ж<testing.T> t) {
    var tests = slice<TestParseContentLength_tests>([
        new(
            cl: "",
            wantErr: badStringError(invalidEmptyContentˢ, ""u8)
        ),
        new(
            cl: "3",
            wantErr: default
        ),
        new(
            cl: "+3",
            wantErr: badStringError(badContentLengthˢ, "+3")
        ),
        new(
            cl: "-3",
            wantErr: badStringError(badContentLengthˢ, "-3")
        ),
        new(
            // max int64, for safe conversion before returning
            cl: "9223372036854775807",
            wantErr: default
        ),
        new(
            cl: "9223372036854775808",
            wantErr: badStringError(badContentLengthˢ, "9223372036854775808")
        )
    ]);

    foreach (var (_, tt) in range(tests)) {
        var (_, gotErr) = parseContentLength([tt.cl]); if (!reflect.DeepEqual(gotErr, tt.wantErr)) {
            t.Errorf("%q:\n\tgot=%v\n\twant=%v", tt.cl, gotErr, tt.wantErr);
        }
    }
}

} // end http_internal_test_package
