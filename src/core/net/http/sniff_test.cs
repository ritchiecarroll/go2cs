// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using log = log_package;
using static global::go.net.http_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using global::go.net;
using static global::go.net.http_internal_test_package;
using Δhttp = global::go.net.http_package;

partial class http_test_package {

partial struct sniffTestsᴛ1 /*dyn*/ {
    internal @string desc;
    internal slice<byte> data;
    internal @string contentType;
}
internal static slice<sniffTestsᴛ1> sniffTests = [
    // Some nonsense.
    new("Empty", [], "text/plain; charset=utf-8"),
    new("Binary", [1, 2, 3], "application/octet-stream"),

    new("HTML document #1", slice<byte>(@"<HtMl><bOdY>blah blah blah</body></html>"u8), "text/html; charset=utf-8"),
    new("HTML document #2", slice<byte>(@"<HTML></HTML>"u8), "text/html; charset=utf-8"),
    new("HTML document #3 (leading whitespace)", slice<byte>(@"   <!DOCTYPE HTML>..."u8), "text/html; charset=utf-8"),
    new("HTML document #4 (leading CRLF)", slice<byte>("\r\n<html>..."u8), "text/html; charset=utf-8"),

    new("Plain text", slice<byte>(@"This is not HTML. It has ☃ though."u8), "text/plain; charset=utf-8"),

    new("XML", slice<byte>("\n<?xml!"u8), "text/xml; charset=utf-8"),

    // Image types.
    new("Windows icon", slice<byte>("\x00\x00\x01\x00"u8), "image/x-icon"),
    new("Windows cursor", slice<byte>("\x00\x00\x02\x00"u8), "image/x-icon"),
    new("BMP image", slice<byte>("BM..."u8), "image/bmp"),
    new("GIF 87a", slice<byte>(@"GIF87a"u8), "image/gif"),
    new("GIF 89a", slice<byte>(@"GIF89a..."u8), "image/gif"),
    new("WEBP image", slice<byte>("RIFF\x00\x00\x00\x00WEBPVP"u8), "image/webp"),
    new("PNG image", slice<byte>(((@string)(new byte[]{0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a}))), "image/png"),
    new("JPEG image", slice<byte>(((@string)(new byte[]{0xff, 0xd8, 0xff}))), "image/jpeg"),

    // Audio types.
    new("MIDI audio", slice<byte>("MThd\x00\x00\x00\x06\x00\x01"u8), "audio/midi"),
    new("MP3 audio/MPEG audio", slice<byte>("ID3\x03\x00\x00\x00\x00\x0f"u8), "audio/mpeg"),
    new("WAV audio #1", slice<byte>(((@string)(new byte[]{0x52, 0x49, 0x46, 0x46, 0x62, 0xb8, 0x00, 0x00, 0x57, 0x41, 0x56, 0x45, 0x66, 0x6d, 0x74, 0x20, 0x12, 0x00, 0x00, 0x00, 0x06}))), "audio/wave"),
    new("WAV audio #2", slice<byte>("RIFF,\x00\x00\x00WAVEfmt \x12\x00\x00\x00\x06"u8), "audio/wave"),
    new("AIFF audio #1", slice<byte>(((@string)(new byte[]{0x46, 0x4f, 0x52, 0x4d, 0x00, 0x00, 0x00, 0x00, 0x41, 0x49, 0x46, 0x46, 0x43, 0x4f, 0x4d, 0x4d, 0x00, 0x00, 0x00, 0x12, 0x00, 0x01, 0x00, 0x00, 0x57, 0x55, 0x00, 0x10, 0x40, 0x0d, 0xf3, 0x34}))), "audio/aiff"),

    new("OGG audio", slice<byte>(((@string)(new byte[]{0x4f, 0x67, 0x67, 0x53, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7e, 0x46, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1f, 0xf6, 0xb4, 0xfc, 0x01, 0x1e, 0x01, 0x76, 0x6f, 0x72}))), "application/ogg"),
    new("Must not match OGG", slice<byte>("owow\x00"u8), "application/octet-stream"),
    new("Must not match OGG", slice<byte>("oooS\x00"u8), "application/octet-stream"),
    new("Must not match OGG", slice<byte>("oggS\x00"u8), "application/octet-stream"),

    // Video types.
    new("MP4 video", slice<byte>(((@string)(new byte[]{0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x6d, 0x70, 0x34, 0x32, 0x00, 0x00, 0x00, 0x00, 0x6d, 0x70, 0x34, 0x32, 0x69, 0x73, 0x6f, 0x6d, 0x3c, 0x06, 0x74, 0xbf, 0x6d, 0x64, 0x61, 0x74}))), "video/mp4"),
    new("AVI video #1", slice<byte>(((@string)(new byte[]{0x52, 0x49, 0x46, 0x46, 0x2c, 0x4f, 0x0a, 0x00, 0x41, 0x56, 0x49, 0x20, 0x4c, 0x49, 0x53, 0x54, 0xc3, 0x80}))), "video/avi"),
    new("AVI video #2", slice<byte>(((@string)(new byte[]{0x52, 0x49, 0x46, 0x46, 0x2c, 0x0a, 0x00, 0x00, 0x41, 0x56, 0x49, 0x20, 0x4c, 0x49, 0x53, 0x54, 0xc3, 0x80}))), "video/avi"),

    // Font types.
    // {"MS.FontObject", []byte("\x00\x00")},
    new("TTF sample  I", slice<byte>("\x00\x01\x00\x00\x00\x17\x01\x00\x00\x04\x01\x60\x4f"u8), "font/ttf"),
    new("TTF sample II", slice<byte>(((@string)(new byte[]{0x00, 0x01, 0x00, 0x00, 0x00, 0x0e, 0x00, 0x80, 0x00, 0x03, 0x00, 0x60, 0x46}))), "font/ttf"),

    new("OTTO sample  I", slice<byte>(((@string)(new byte[]{0x4f, 0x54, 0x54, 0x4f, 0x00, 0x0e, 0x00, 0x80, 0x00, 0x03, 0x00, 0x60, 0x42, 0x41, 0x53, 0x45}))), "font/otf"),

    new("woff sample  I", slice<byte>("\x77\x4f\x46\x46\x00\x01\x00\x00\x00\x00\x30\x54\x00\x0d\x00\x00"u8), "font/woff"),
    new("woff2 sample", slice<byte>("\x77\x4f\x46\x32\x00\x01\x00\x00\x00"u8), "font/woff2"),
    new("wasm sample", slice<byte>("\x00\x61\x73\x6d\x01\x00"u8), "application/wasm"),

    // Archive types
    new("RAR v1.5-v4.0", slice<byte>("Rar!\x1A\x07\x00"u8), "application/x-rar-compressed"),
    new("RAR v5+", slice<byte>("Rar!\x1A\x07\x01\x00"u8), "application/x-rar-compressed"),
    new("Incorrect RAR v1.5-v4.0", slice<byte>("Rar \x1A\x07\x00"u8), "application/octet-stream"),
    new("Incorrect RAR v5+", slice<byte>("Rar \x1A\x07\x01\x00"u8), "application/octet-stream")
];

public static void TestDetectContentType(ж<testing.T> t) {
    foreach (var (_, tt) in range(sniffTests)) {
        @string ct = DetectContentType(tt.data);
        if (ct != tt.contentType) {
            t.Errorf("%v: DetectContentType = %q, want %q", tt.desc, ct, tt.contentType);
        }
    }
}

public static void TestServerContentTypeSniff(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testServerContentTypeSniff(Δp0, Δp1)); }
internal static void testServerContentTypeSniff(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var (i, _) = strconv.Atoi(r.FormValue("i"));
            var tt = sniffTests[i];
            var (n, err) = w.Write(tt.data);
            if (n != len(tt.data) || err != nil) {
                log.Fatalf("%v: Write(%q) = %v, %v want %d, nil", tt.desc, tt.data, n, err, len(tt.data));
            }
        })));
        var cstʗ1 = cst;
        defer(cstʗ1.close, ref ᒐ);

        foreach (var (i, tt) in range(sniffTests)) {
            var (resp, err) = cst.c.Get(cst.ts.URL + "/?i="u8 + strconv.Itoa(i));
            if (err != nil) {
                t.Errorf("%v: %v", tt.desc, err);
                continue;
            }
            // DetectContentType is defined to return
            // text/plain; charset=utf-8 for an empty body,
            // but as of Go 1.10 the HTTP server has been changed
            // to return no content-type at all for an empty body.
            // Adjust the expectation here.
            @string wantContentType = tt.contentType;
            if (len(tt.data) == 0) {
                wantContentType = ""u8;
            }
            @string ct = resp.Header.Get(contentTypeˢ); if (ct != wantContentType) {
                t.Errorf("%v: Content-Type = %q, want %q", tt.desc, ct, wantContentType);
            }
            (var data, err) = io.ReadAll(resp.Body);
            if (err != nil) {
                t.Errorf("%v: reading body: %v", tt.desc, err);
            } else if (!bytes.Equal(data, tt.data)) {
                t.Errorf("%v: data is %q, want %q", tt.desc, data, tt.data);
            }
            resp.Body.Close();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 5953: shouldn't sniff if the handler set a Content-Type header,
// even if it's the empty string.
public static void TestServerIssue5953(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testServerIssue5953(Δp0, Δp1)); }
internal static void testServerIssue5953(ж<testing.T> t, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header()[contentTypeˢ] = [""];
        fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "<html><head></head><body>hi</body></html>"u8);
    })));

    var (resp, err) = cst.c.Get(cst.ts.URL);
    if (err != nil) {
        t.Fatal(err);
    }

    var got = resp.Header[contentTypeˢ];
    var want = slice<@string>([""]);
    if (!slices.Equal<slice<@string>, @string>(got, want)) {
        t.Errorf("Content-Type = %q; want %q", got, want);
    }
    resp.Body.Close();
}

partial struct byteAtATimeReader {
    internal slice<byte> buf;
}

internal static (nint n, error err) Read(this ref byteAtATimeReader b, slice<byte> p) {
    if (len(p) < 1) {
        return (0, default);
    }
    if (len(b.buf) == 0) {
        return (0, io.EOF);
    }
    p[0] = b.buf[0];
    b.buf = b.buf[1..];
    return (1, default);
}

public static void TestContentTypeWithVariousSources(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testContentTypeWithVariousSources(Δp0, Δp1)); }
// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string inputᶜ => ref inputᶜᵇ;
private static readonly @string inputᶜᵇ = "\n<html>\n\t<head>\n"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string expectedᶜ => ref expectedᶜᵇ;
private static readonly @string expectedᶜᵇ = "text/html; charset=utf-8"u8;

internal partial struct testContentTypeWithVariousSources_type /*dyn*/ {
    internal @string name;
    internal Action<Δhttp.ResponseWriter, ж<Δhttp.Request>> handler;
}

// Use io.Copy from a plain Reader.
internal partial struct testContentTypeWithVariousSources_readerOnly /*dyn*/ {
    /*embed*/ public io_package.Reader Reader;
}

internal static void testContentTypeWithVariousSources(ж<testing.T> t, testMode mode) {
    @string input = inputᶜ;
    @string expected = expectedᶜ;

    foreach (var (_, vᴛ1) in range(slice<testContentTypeWithVariousSources_type>([new(
        name: "write",
        handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            // Write the whole input at once.
            var (n, err) = w.Write(slice<byte>(input));
            if ((nint)n != len(input) || err != nil) {
                t.Errorf("w.Write(%q) = %v, %v want %d, nil", input, n, err, len(input));
            }
        }
    ), new(
        name: "write one byte at a time",
        handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            // Write the input one byte at a time.
            var buf = slice<byte>(input);
            foreach (var i in range(len(buf))) {
                var (n, err) = w.Write(buf.slice(i, i + 1));
                if (n != 1 || err != nil) {
                    t.Errorf("w.Write(%q) = %v, %v want 1, nil", input, n, err);
                }
            }
        }
    ), new(
        name: "copy from Reader",
        handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var buf = bytes.NewBuffer(slice<byte>(input));
            var (n, err) = io.Copy(new http_test_package.http_ResponseWriterᴠWriter(w), new testContentTypeWithVariousSources_readerOnly(new http_test_package.bytes_BufferжReader(buf)));
            if ((nint)n != len(input) || err != nil) {
                t.Errorf("io.Copy(w, %q) = %v, %v want %d, nil", input, n, err, len(input));
            }
        }
    ), new(
        name: "copy from bytes.Buffer",
        handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            // Use io.Copy from a bytes.Buffer to trigger ReadFrom.
            var buf = bytes.NewBuffer(slice<byte>(input));
            var (n, err) = io.Copy(new http_test_package.http_ResponseWriterᴠWriter(w), new http_test_package.bytes_BufferжReader(buf));
            if ((nint)n != len(input) || err != nil) {
                t.Errorf("io.Copy(w, %q) = %v, %v want %d, nil", input, n, err, len(input));
            }
        }
    ), new(
        name: "copy one byte at a time",
        handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            // Use io.Copy from a Reader that returns one byte at a time.
            var (n, err) = io.Copy(new http_test_package.http_ResponseWriterᴠWriter(w), new http_test_package.byteAtATimeReaderжReader(Ꮡ(new byteAtATimeReader(slice<byte>(input)))));
            if ((nint)n != len(input) || err != nil) {
                t.Errorf("io.Copy(w, %q) = %v, %v want %d, nil", input, n, err, len(input));
            }
        }
    )
    ]))) {
        ref var test = ref heap(new testContentTypeWithVariousSources_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            var cst = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), mode, new http_test_package.http_HandlerFuncᴠΔHandler(NilSafeDelegateConversion<Δhttp.HandlerFunc, Action<Δhttp.ResponseWriter, ж<Δhttp.Request>>>(testʗ1.handler)));

            var (resp, err) = cst.c.Get(cst.ts.URL);
            if (err != nil) {
                tΔ1.Fatalf("Get: %v", err);
            }
            @string ct = resp.Header.Get(contentTypeˢ); if (ct != expected) {
                tΔ1.Errorf("Content-Type = %q, want %q", ct, expected);
            }
            @string want = resp.Header.Get(contentLengthˢ);
            @string got = fmt.Sprint(len(input)); if (want != got) {
                tΔ1.Errorf("Content-Length = %q, want %q", want, got);
            }
            (var data, err) = io.ReadAll(resp.Body);
            if (err != nil) {
                tΔ1.Errorf("reading body: %v", err);
            } else if (!bytes.Equal(data, slice<byte>(input))) {
                tΔ1.Errorf("data is %q, want %q", data, input);
            }
            resp.Body.Close();
        });
    }
}

public static void TestSniffWriteSize(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testSniffWriteSize(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string sizeˢ => ref sizeˢᵇ;
private static readonly @string sizeˢᵇ = "size"u8;

internal static void testSniffWriteSize(ж<testing.T> t, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var (size, _) = strconv.Atoi(r.FormValue(sizeˢ));
        var (written, err) = io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), strings.Repeat("a", size));
        if (err != nil) {
            t.Errorf("write of %d bytes: %v", size, err);
            return;
        }
        if (written != size) {
            t.Errorf("write of %d bytes wrote %d bytes", size, written);
        }
    })));
    foreach (var (_, size) in range(slice<nint>([0, 1, 200, 600, 999, 1000, 1023, 1024, 512 << 10, 1 << 20]))) {
        var (res, err) = cst.c.Get(fmt.Sprintf("%s/?size=%d"u8, cst.ts.URL, size));
        if (err != nil) {
            t.Fatalf("size %d: %v", size, err);
        }
        var (_, errΔ1) = io.Copy(io.Discard, res.Body); if (errΔ1 != nil) {
            t.Fatalf("size %d: io.Copy of body = %v", size, errΔ1);
        }
        var errΔ2 = res.Body.Close(); if (errΔ2 != nil) {
            t.Fatalf("size %d: body Close = %v", size, errΔ2);
        }
    }
}

} // end http_test_package
