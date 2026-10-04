// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bufio = bufio_package;
using bytes = bytes_package;
using gzip = compress.gzip_package;
using errors = errors_package;
using fmt = fmt_package;
using testenv = global::go.@internal.testenv_package;
using io = io_package;
using fs = global::go.io.fs_package;
using mime = mime_package;
using multipart = global::go.mime.multipart_package;
using net = net_package;
using Δhttp = global::go.net.http_package;
using static global::go.net.http_package;
using httptest = global::go.net.http.httptest_package;
using url = global::go.net.url_package;
using os = os_package;
using exec = global::go.os.exec_package;
using path = path_package;
using filepath = global::go.path.filepath_package;
using regexp = regexp_package;
using runtime = runtime_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using fstest = global::go.testing.fstest_package;
using time = time_package;
using compress;
using global::go.@internal;
using global::go.io;
using global::go.mime;
using global::go.net;
using global::go.net.http;
using global::go.os;
using global::go.path;
using global::go.testing;
using static global::go.net.http_internal_test_package;

partial class http_test_package {

internal static readonly @string testFile = "testdata/file"u8;
internal static UntypedInt testFileLen => 11;

[GoType] partial struct wantRange {
    internal int64 start, end; // range [start,end)
}

// ignore wasteful range request

[GoType("dyn")] partial struct ServeFileRangeTestsᴛ1 {
    internal @string r;
    internal nint code;
    internal slice<wantRange> ranges;
}
public static slice<ServeFileRangeTestsᴛ1> ServeFileRangeTests = new ServeFileRangeTestsᴛ1[]{
    new(r: ""u8, code: StatusOK),
    new(r: "bytes=0-4"u8, code: StatusPartialContent, ranges: new wantRange[]{new(0, 5)}.slice()),
    new(r: "bytes=2-"u8, code: StatusPartialContent, ranges: new wantRange[]{new(2, testFileLen)}.slice()),
    new(r: "bytes=-5"u8, code: StatusPartialContent, ranges: new wantRange[]{new(testFileLen - 5, testFileLen)}.slice()),
    new(r: "bytes=3-7"u8, code: StatusPartialContent, ranges: new wantRange[]{new(3, 8)}.slice()),
    new(r: "bytes=0-0,-2"u8, code: StatusPartialContent, ranges: new wantRange[]{new(0, 1), new(testFileLen - 2, testFileLen)}.slice()),
    new(r: "bytes=0-1,5-8"u8, code: StatusPartialContent, ranges: new wantRange[]{new(0, 2), new(5, 9)}.slice()),
    new(r: "bytes=0-1,5-"u8, code: StatusPartialContent, ranges: new wantRange[]{new(0, 2), new(5, testFileLen)}.slice()),
    new(r: "bytes=5-1000"u8, code: StatusPartialContent, ranges: new wantRange[]{new(5, testFileLen)}.slice()),
    new(r: "bytes=0-,1-,2-,3-,4-"u8, code: StatusOK),
    new(r: "bytes=0-9"u8, code: StatusPartialContent, ranges: new wantRange[]{new(0, testFileLen - 1)}.slice()),
    new(r: "bytes=0-10"u8, code: StatusPartialContent, ranges: new wantRange[]{new(0, testFileLen)}.slice()),
    new(r: "bytes=0-11"u8, code: StatusPartialContent, ranges: new wantRange[]{new(0, testFileLen)}.slice()),
    new(r: "bytes=10-11"u8, code: StatusPartialContent, ranges: new wantRange[]{new(testFileLen - 1, testFileLen)}.slice()),
    new(r: "bytes=10-"u8, code: StatusPartialContent, ranges: new wantRange[]{new(testFileLen - 1, testFileLen)}.slice()),
    new(r: "bytes=11-"u8, code: StatusRequestedRangeNotSatisfiable),
    new(r: "bytes=11-12"u8, code: StatusRequestedRangeNotSatisfiable),
    new(r: "bytes=12-12"u8, code: StatusRequestedRangeNotSatisfiable),
    new(r: "bytes=11-100"u8, code: StatusRequestedRangeNotSatisfiable),
    new(r: "bytes=12-100"u8, code: StatusRequestedRangeNotSatisfiable),
    new(r: "bytes=100-"u8, code: StatusRequestedRangeNotSatisfiable),
    new(r: "bytes=100-1000"u8, code: StatusRequestedRangeNotSatisfiable)
}.slice();

public static void TestServeFile(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServeFile(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataFileˢ = "testdata/file"u8;
internal static readonly object readingFileˢ = (@string)"reading file:"u8;
internal static readonly object parseURLˢ = (@string)"ParseURL:"u8;
internal static readonly @string rangeˢ = "Range"u8;
internal static readonly @string contentRangeˢ = "Content-Range"u8;
internal static readonly @string multipartByterangesˢ = "multipart/byteranges"u8;
internal static readonly @string boundaryˢ = "boundary"u8;

internal static void testServeFile(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        ServeFile(w, r, testdataFileˢ);
    }))).Value.ts;
    var c = ts.Client();
    error err = default!;
    (var @file, err) = os.ReadFile(testFile);
    if (err != default!) {
        Ꮡt.Fatal(readingFileˢ, err);
    }
    // set up the Request (re-used for all tests)
    Δhttp.Request req = default!;
    req.Header = new httpꓸHeader(0);
    {
        (req.URL, err) = url.Parse((~ts).URL); if (err != default!) {
            Ꮡt.Fatal(parseURLˢ, err);
        }
    }
    // Get contents via various methods.
    //
    // See https://go.dev/issue/59471 for a proposal to limit the set of methods handled.
    // For now, test the historical behavior.
    foreach (var (_, method) in new @string[]{
        MethodGet,
        MethodPost,
        MethodPut,
        MethodPatch,
        MethodDelete,
        MethodOptions,
        MethodTrace
    }.slice()) {
        req.Method = method;
        var (_, bodyΔ1) = getBody(Ꮡt, method, req, c);
        if (!bytes.Equal(bodyΔ1, @file)) {
            Ꮡt.Fatalf("body mismatch for %v request: got %q, want %q"u8, method, bodyΔ1, @file);
        }
    }
    // HEAD request.
    req.Method = MethodHead;
    var (resp, body) = getBody(Ꮡt, headˢ, req, c);
    if (len(body) != 0) {
        Ꮡt.Fatalf("body mismatch for HEAD request: got %q, want empty"u8, body);
    }
    {
        @string got = (~resp).Header.Get(contentLengthˢ);
        @string want = fmt.Sprint(len(@file)); if (got != want) {
            Ꮡt.Fatalf("Content-Length mismatch for HEAD request: got %v, want %v"u8, got, want);
        }
    }
    // Range tests
    req.Method = MethodGet;
Cases:
    foreach (var (_, rt) in ServeFileRangeTests) {
        if (rt.r != ""u8) {
            req.Header.Set(rangeˢ, rt.r);
        }
        var (respΔ1, bodyΔ2) = getBody(Ꮡt, fmt.Sprintf("range test %q"u8, rt.r), req, c);
        if ((~respΔ1).StatusCode != rt.code) {
            Ꮡt.Errorf("range=%q: StatusCode=%d, want %d"u8, rt.r, (~respΔ1).StatusCode, rt.code);
        }
        if (rt.code == StatusRequestedRangeNotSatisfiable) {
            continue;
        }
        @string wantContentRange = ""u8;
        if (len(rt.ranges) == 1) {
            var rng = rt.ranges[0];
            wantContentRange = fmt.Sprintf("bytes %d-%d/%d"u8, rng.start, rng.end - 1, (nint)(testFileLen));
        }
        @string cr = (~respΔ1).Header.Get(contentRangeˢ);
        if (cr != wantContentRange) {
            Ꮡt.Errorf("range=%q: Content-Range = %q, want %q"u8, rt.r, cr, wantContentRange);
        }
        @string ct = (~respΔ1).Header.Get(contentTypeˢ);
        if (len(rt.ranges) == 1) {
            var rng = rt.ranges[0];
            var wantBody = @file.slice((nint)(rng.start), (nint)(rng.end));
            if (!bytes.Equal(bodyΔ2, wantBody)) {
                Ꮡt.Errorf("range=%q: body = %q, want %q"u8, rt.r, bodyΔ2, wantBody);
            }
            if (strings.HasPrefix(ct, multipartByterangesˢ)) {
                Ꮡt.Errorf("range=%q content-type = %q; unexpected multipart/byteranges"u8, rt.r, ct);
            }
        }
        if (len(rt.ranges) > 1) {
            var (typ, @params, errΔ1) = mime.ParseMediaType(ct);
            if (errΔ1 != default!) {
                Ꮡt.Errorf("range=%q content-type = %q; %v"u8, rt.r, ct, errΔ1);
                continue;
            }
            if (typ != "multipart/byteranges"u8) {
                Ꮡt.Errorf("range=%q content-type = %q; want multipart/byteranges"u8, rt.r, typ);
                continue;
            }
            if (@params[boundaryˢ] == "") {
                Ꮡt.Errorf("range=%q content-type = %q; lacks boundary"u8, rt.r, ct);
                continue;
            }
            {
                var (g, w) = (respΔ1.Value.ContentLength, (int64)len(bodyΔ2)); if (g != w) {
                    Ꮡt.Errorf("range=%q Content-Length = %d; want %d"u8, rt.r, g, w);
                    continue;
                }
            }
            var mr = multipart.NewReader(new http_test_package.bytes_ReaderжReader(bytes.NewReader(bodyΔ2)), @params[boundaryˢ]);
            foreach (var (ri, rng) in rt.ranges) {
                var (part, errΔ2) = mr.NextPart();
                if (errΔ2 != default!) {
                    Ꮡt.Errorf("range=%q, reading part index %d: %v"u8, rt.r, ri, errΔ2);
                    goto continue_Cases;
                }
                wantContentRange = fmt.Sprintf("bytes %d-%d/%d"u8, rng.start, rng.end - 1, (nint)(testFileLen));
                {
                    @string g = (~part).Header.Get(contentRangeˢ);
                    @string w = wantContentRange; if (g != w) {
                        Ꮡt.Errorf("range=%q: part Content-Range = %q; want %q"u8, rt.r, g, w);
                    }
                }
                (var bodyΔ3, errΔ2) = io.ReadAll(new multipart.PartжReader(part));
                if (errΔ2 != default!) {
                    Ꮡt.Errorf("range=%q, reading part index %d body: %v"u8, rt.r, ri, errΔ2);
                    goto continue_Cases;
                }
                var wantBody = @file.slice((nint)(rng.start), (nint)(rng.end));
                if (!bytes.Equal(bodyΔ3, wantBody)) {
                    Ꮡt.Errorf("range=%q: body = %q, want %q"u8, rt.r, bodyΔ3, wantBody);
                }
            }
            (_, errΔ1) = mr.NextPart();
            if (!AreEqual(errΔ1, io.EOF)) {
                Ꮡt.Errorf("range=%q; expected final error io.EOF; got %v"u8, rt.r, errΔ1);
            }
        }
continue_Cases:;
    }
break_Cases:;
}

[GoType("dyn")] internal partial struct TestServeFile_DotDot_tests {
    internal @string req;
    internal nint wantStatus;
}

public static void TestServeFile_DotDot(ж<testing.T> Ꮡt) {
    var tests = new TestServeFile_DotDot_tests[]{
        new("/testdata/file"u8, 200),
        new("/../file"u8, 400),
        new("/.."u8, 400),
        new("/../"u8, 400),
        new("/../foo"u8, 400),
        new("/..\\foo"u8, 400),
        new("/file/a"u8, 200),
        new("/file/a.."u8, 200),
        new("/file/a/.."u8, 400),
        new("/file/a\\.."u8, 400)
    }.slice();
    foreach (var (_, tt) in tests) {
        var (req, err) = ReadRequest(bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader("GET "u8 + tt.req + " HTTP/1.1\r\nHost: foo\r\n\r\n"u8))));
        if (err != default!) {
            Ꮡt.Errorf("bad request %q: %v"u8, tt.req, err);
            continue;
        }
        var rec = httptest.NewRecorder();
        ServeFile(new http_test_package.httptest_ResponseRecorderжResponseWriter(rec), req, testdataFileˢ);
        if ((~rec).Code != tt.wantStatus) {
            Ꮡt.Errorf("for request %q, status = %d; want %d"u8, tt.req, (~rec).Code, tt.wantStatus);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataˢ = "testdata"u8;

// Tests that this doesn't panic. (Issue 30165)
public static void TestServeFileDirPanicEmptyPath(ж<testing.T> Ꮡt) {
    var rec = httptest.NewRecorder();
    var req = httptest.NewRequest(getˢ2, "/"u8, default!);
    req.Value.URL.Value.Path = ""u8;
    ServeFile(new http_test_package.httptest_ResponseRecorderжResponseWriter(rec), req, testdataˢ);
    var res = rec.Result();
    if ((~res).StatusCode != 301) {
        Ꮡt.Errorf("code = %v; want 301"u8, (~res).Status);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nothingˢ = "nothing"u8;

// Tests that ranges are ignored with serving empty content. (Issue 54794)
public static void TestServeContentWithEmptyContentIgnoreRanges(ж<testing.T> Ꮡt) {
    foreach (var (_, r) in new @string[]{
        "bytes=0-128"u8,
        "bytes=1-"u8
    }.slice()) {
        var rec = httptest.NewRecorder();
        var req = httptest.NewRequest(getˢ2, "/"u8, default!);
        (~req).Header.Set(rangeˢ, r);
        ServeContent(new http_test_package.httptest_ResponseRecorderжResponseWriter(rec), req, nothingˢ, time.Now(), new http_test_package.bytes_ReaderжReadSeeker(bytes.NewReader(default!)));
        var res = rec.Result();
        if ((~res).StatusCode != 200) {
            Ꮡt.Errorf("code = %v; want 200"u8, (~res).Status);
        }
        nint bodyLen = (~rec).Body.Len();
        if (bodyLen != 0) {
            Ꮡt.Errorf("body.Len() = %v; want 0"u8, (~res).Status);
        }
    }
}


[GoType("dyn")] partial struct fsRedirectTestDataᴛ1 {
    internal @string original, redirect;
}
internal static slice<fsRedirectTestDataᴛ1> fsRedirectTestData = new fsRedirectTestDataᴛ1[]{
    new("/test/index.html"u8, "/test/"u8),
    new("/test/testdata"u8, "/test/testdata/"u8),
    new("/test/testdata/file/"u8, "/test/testdata/file"u8)
}.slice();

public static void TestFSRedirect(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testFSRedirect(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testˢ = "/test"u8;

internal static void testFSRedirect(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, StripPrefix(testˢ, FileServer(((Δhttp.Dir)(@string)"."u8)))).Value.ts;
    foreach (var (_, data) in fsRedirectTestData) {
        var (res, err) = ts.Client().Get((~ts).URL + data.original);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (~res).Body.Close();
        {
            @string g = res.Value.Request.Value.URL.Value.Path;
            @string e = data.redirect; if (g != e) {
                Ꮡt.Errorf("redirect from %s: got %s, want %s"u8, data.original, g, e);
            }
        }
    }
}

[GoType] partial struct testFileSystem {
    internal Func<@string, (Δhttp.File, error)> open;
}

[GoRecv] internal static (Δhttp.File, error) Open(this ref testFileSystem fs, @string name) {
    return fs.open(name);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fileDoesNotExistˢ = "file does not exist"u8;
internal static readonly @string httpExampleComˢ = "http://example.com"u8;

[GoType("dyn")] internal partial struct TestFileServerCleans_tests {
    internal @string reqPath, openArg;
}

public static void TestFileServerCleans(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var ch = new channel<@string>(1);
        var chʗ1 = ch;
        var fs = FileServer(new http_test_package.testFileSystemжFileSystem(Ꮡ(new testFileSystem((@string name) => {
            chʗ1.ᐸꟷ(name);
            return (default!, errors.New(fileDoesNotExistˢ));
        }
        ))));
        var tests = new TestFileServerCleans_tests[]{
            new("/foo.txt"u8, "/foo.txt"u8),
            new("//foo.txt"u8, "/foo.txt"u8),
            new("/../foo.txt"u8, "/foo.txt"u8)
        }.slice();
        var (req, _) = NewRequest(getˢ2, httpExampleComˢ, default!);
        foreach (var (n, test) in tests) {
            var rec = httptest.NewRecorder();
            req.Value.URL.Value.Path = test.reqPath;
            fs.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(rec), req);
            {
                @string got = ᐸꟷ(ch); if (got != test.openArg) {
                    Ꮡt.Errorf("test %d: got %q, want %q"u8, n, got, test.openArg);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestFileServerEscapesNames(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testFileServerEscapesNames(Δp0, Δp1));
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string dirListPrefixᶜ = "<!doctype html>\n<meta name=\"viewport\" content=\"width=device-width\">\n<pre>\n"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string dirListSuffixᶜ = "\n</pre>\n"u8;

[GoType("dyn")] internal partial struct testFileServerEscapesNames_tests {
    internal @string name, escaped;
}

internal static void testFileServerEscapesNames(ж<testing.T> Ꮡt, testMode mode) {
    @string dirListPrefix = dirListPrefixᶜ;
    @string dirListSuffix = dirListSuffixᶜ;
    var tests = new testFileServerEscapesNames_tests[]{
        new(@"simple_name"u8, @"<a href=""simple_name"">simple_name</a>"u8),
        new(@"""'<>&"u8, @"<a href=""%22%27%3C%3E&"">&#34;&#39;&lt;&gt;&amp;</a>"u8),
        new(@"?foo=bar#baz"u8, @"<a href=""%3Ffoo=bar%23baz"">?foo=bar#baz</a>"u8),
        new(@"<combo>?foo"u8, @"<a href=""%3Ccombo%3E%3Ffoo"">&lt;combo&gt;?foo</a>"u8),
        new(@"foo:bar"u8, @"<a href=""./foo:bar"">foo:bar</a>"u8)
    }.slice();
    // We put each test file in its own directory in the fakeFS so we can look at it in isolation.
    ref var fs = ref heap<fakeFS>(out var Ꮡfs);
    fs = new fakeFS(0);
    foreach (var (i, test) in tests) {
        var testFile = Ꮡ(new fakeFileInfo(basename: test.name));
        fs[fmt.Sprintf("/%d"u8, i)] = Ꮡ(new fakeFileInfo(
            dir: true,
            modtime: time.Unix(1000000000, 0).UTC(),
            ents: new ж<fakeFileInfo>[]{testFile}.slice()
        ));
        fs[fmt.Sprintf("/%d/%s"u8, i, test.name)] = testFile;
    }
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, FileServer(new http_test_package.fakeFSжFileSystem(Ꮡfs))).Value.ts;
    foreach (var (i, test) in tests) {
        @string url = fmt.Sprintf("%s/%d"u8, (~ts).URL, i);
        var (res, err) = ts.Client().Get(url);
        if (err != default!) {
            Ꮡt.Fatalf("test %q: Get: %v"u8, test.name, err);
        }
        (var b, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            Ꮡt.Fatalf("test %q: read Body: %v"u8, test.name, err);
        }
        @string s = ((@string)b);
        if (!strings.HasPrefix(s, dirListPrefix) || !strings.HasSuffix(s, dirListSuffix)) {
            Ꮡt.Errorf("test %q: listing dir, full output is %q, want prefix %q and suffix %q"u8, test.name, s, dirListPrefix, dirListSuffix);
        }
        {
            @string trimmed = strings.TrimSuffix(strings.TrimPrefix(s, dirListPrefix), dirListSuffix); if (trimmed != test.escaped) {
                Ꮡt.Errorf("test %q: listing dir, filename escaped to %q, want %q"u8, test.name, trimmed, test.escaped);
            }
        }
        (~res).Body.Close();
    }
}

public static void TestFileServerSortsNames(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testFileServerSortsNames(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aHrefAAAAHrefBBAˢ = "<a href=\"a\">a</a>\n<a href=\"b\">b</a>"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string contentsᶜ = "I am a fake file"u8;

internal static void testFileServerSortsNames(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        @string contents = contentsᶜ;
        ref var dirMod = ref heap<time.Time>(out var ᏑdirMod);
        dirMod = time.Unix(123, 0).UTC();
        var fileMod = time.Unix(1000000000, 0).UTC();
        ref var fs = ref heap<fakeFS>(out var Ꮡfs);
        fs = new fakeFS(new map<@string, ж<fakeFileInfo>>{
            ["/"u8] = Ꮡ(new fakeFileInfo(
                dir: true,
                modtime: dirMod,
                ents: new ж<fakeFileInfo>[]{
                    Ꮡ(new fakeFileInfo(
                        basename: "b"u8,
                        modtime: fileMod,
                        contents: contents)),
                    Ꮡ(new fakeFileInfo(
                        basename: "a"u8,
                        modtime: fileMod,
                        contents: contents))
                }.slice()
            ))
        });
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, FileServer(new http_test_package.fakeFSжFileSystem(Ꮡfs))).Value.ts;
        var (res, err) = ts.Client().Get((~ts).URL);
        if (err != default!) {
            Ꮡt.Fatalf("Get: %v"u8, err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        (var b, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            Ꮡt.Fatalf("read Body: %v"u8, err);
        }
        @string s = ((@string)b);
        if (!strings.Contains(s, aHrefAAAAHrefBBAˢ)) {
            Ꮡt.Errorf("output appears to be unsorted:\n%s"u8, s);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void mustRemoveAll(@string dir) {
    var err = os.RemoveAll(dir);
    if (err != default!) {
        throw panic(err);
    }
}

public static void TestFileServerImplicitLeadingSlash(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testFileServerImplicitLeadingSlash(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooTxtˢ = "foo.txt"u8;
internal static readonly @string barˢ3 = "/bar/"u8;
internal static readonly @string fooTxtˢ2 = ">foo.txt<"u8;
internal static readonly @string barFooTxtˢ = "/bar/foo.txt"u8;
internal static readonly object helloWorldˢ2 = (@string)"Hello world"u8;

internal static void testFileServerImplicitLeadingSlash(ж<testing.T> Ꮡt, testMode mode) {
    @string tempDir = Ꮡt.TempDir();
    {
        var err = os.WriteFile(filepath.Join(tempDir, fooTxtˢ), slice<byte>("Hello world"u8), 420); if (err != default!) {
            Ꮡt.Fatalf("WriteFile: %v"u8, err);
        }
    }
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, StripPrefix(barˢ3, FileServer(((Δhttp.Dir)tempDir)))).Value.ts;
    var tsʗ1 = ts;
    @string get(@string suffix) {
        var (res, err) = tsʗ1.Client().Get((~tsʗ1).URL + suffix);
        if (err != default!) {
            Ꮡt.Fatalf("Get %s: %v"u8, suffix, err);
        }
        (var b, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            Ꮡt.Fatalf("ReadAll %s: %v"u8, suffix, err);
        }
        (~res).Body.Close();
        return ((@string)b);
    }
    {
        @string s = get(barˢ3); if (!strings.Contains(s, fooTxtˢ2)) {
            Ꮡt.Logf("expected a directory listing with foo.txt, got %q"u8, s);
        }
    }
    {
        @string s = get(barFooTxtˢ); if (s != "Hello world"u8) {
            Ꮡt.Logf("expected %q, got %q"u8, helloWorldˢ2, s);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestOnWindowsˢ = (@string)"skipping test on windows"u8;
internal static readonly @string etcHostsˢ = "/etc/hosts"u8;
internal static readonly object skippingTestNoEtcHostsˢ = (@string)"skipping test; no /etc/hosts file"u8;
internal static readonly @string etcˢ = "/etc/"u8;
internal static readonly @string hostsˢ = "/hosts"u8;
internal static readonly @string hostsˢ2 = "hosts"u8;
internal static readonly @string hostsˢ3 = "../../../../hosts"u8;
internal static readonly @string etcˢ2 = "/etc"u8;

public static void TestDirJoin(ж<testing.T> Ꮡt) {
    if (runtime.GOOS == "windows"u8) {
        Ꮡt.Skip(skippingTestOnWindowsˢ);
    }
    var (wfi, err) = os.Stat(etcHostsˢ);
    if (err != default!) {
        Ꮡt.Skip(skippingTestNoEtcHostsˢ);
    }
    var wfiʗ1 = wfi;
    void test(Δhttp.Dir d, @string name) {
        GoFrame ᒐ = default;
        try {
            var (f, errΔ1) = d.Open(name);
            if (errΔ1 != default!) {
                Ꮡt.Fatalf("open of %s: %v"u8, name, errΔ1);
            }
            var fʗ1 = f;
            defer(() => fʗ1.Close(), ref ᒐ);
            (var gfi, errΔ1) = f.Stat();
            if (errΔ1 != default!) {
                Ꮡt.Fatalf("stat of %s: %v"u8, name, errΔ1);
            }
            if (!os.SameFile(gfi, wfiʗ1)) {
                Ꮡt.Errorf("%s got different file"u8, name);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    test(((Δhttp.Dir)(@string)etcˢ), hostsˢ);
    test(((Δhttp.Dir)(@string)etcˢ), hostsˢ2);
    test(((Δhttp.Dir)(@string)etcˢ), hostsˢ3);
    test(((Δhttp.Dir)(@string)etcˢ2), hostsˢ);
    test(((Δhttp.Dir)(@string)etcˢ2), hostsˢ2);
    test(((Δhttp.Dir)(@string)etcˢ2), hostsˢ3);
    // Not really directories, but since we use this trick in
    // ServeFile, test it:
    test(((Δhttp.Dir)(@string)etcHostsˢ), ""u8);
    test(((Δhttp.Dir)(@string)etcHostsˢ), "/"u8);
    test(((Δhttp.Dir)(@string)etcHostsˢ), "../"u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fsTestGoˢ = "fs_test.go"u8;

public static void TestEmptyDirOpenCWD(ж<testing.T> Ꮡt) {
    void test(Δhttp.Dir d) {
        GoFrame ᒐ = default;
        try {
            @string name = fsTestGoˢ;
            var (f, err) = d.Open(name);
            if (err != default!) {
                Ꮡt.Fatalf("open of %s: %v"u8, name, err);
            }
            var fʗ1 = f;
            defer(() => fʗ1.Close(), ref ᒐ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    test(((Δhttp.Dir)(@string)""u8));
    test(((Δhttp.Dir)(@string)"."u8));
    test(((Δhttp.Dir)(@string)"./"u8));
}

public static void TestServeFileContentType(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServeFileContentType(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string overrideˢ = "override"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string ctypeᶜ = "icecream/chocolate"u8;

internal static void testServeFileContentType(ж<testing.T> Ꮡt, testMode mode) {
    @string ctype = ctypeᶜ;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var exprᴛ1 = r.FormValue(overrideˢ);
        if (exprᴛ1 == "1"u8) {
            w.Header().Set(contentTypeˢ, ctype);
        }
        else if (exprᴛ1 == "2"u8) {
            w.Header()[contentTypeˢ] = new @string[]{}.slice();
        }

        // Explicitly inhibit sniffing.
        ServeFile(w, r, testdataFileˢ);
    }))).Value.ts;
    var tsʗ1 = ts;
    void get(@string @override, slice<@string> want) {
        var (resp, err) = tsʗ1.Client().Get((~tsʗ1).URL + "?override="u8 + @override);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            var h = (~resp).Header[contentTypeˢ]; if (!slices.Equal<slice<@string>, @string>(h, want)) {
                Ꮡt.Errorf("Content-Type mismatch: got %v, want %v"u8, h, want);
            }
        }
        (~resp).Body.Close();
    }
    get("0"u8, new @string[]{"text/plain; charset=utf-8"u8}.slice());
    get("1"u8, new @string[]{ctype}.slice());
    get("2"u8, default!);
}

public static void TestServeFileMimeType(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServeFileMimeType(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataStyleCssˢ = "testdata/style.css"u8;
internal static readonly @string textCssCharsetUtf8ˢ = "text/css; charset=utf-8"u8;

internal static void testServeFileMimeType(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        ServeFile(w, r, testdataStyleCssˢ);
    }))).Value.ts;
    var (resp, err) = ts.Client().Get((~ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~resp).Body.Close();
    @string want = textCssCharsetUtf8ˢ;
    {
        @string h = (~resp).Header.Get(contentTypeˢ); if (h != want) {
            Ꮡt.Errorf("Content-Type mismatch: got %q, want %q"u8, h, want);
        }
    }
}

public static void TestServeFileFromCWD(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServeFileFromCWD(Δp0, Δp1));
}

internal static void testServeFileFromCWD(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> rΔ1) => {
        ServeFile(w, rΔ1, fsTestGoˢ);
    }))).Value.ts;
    var (r, err) = ts.Client().Get((~ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~r).Body.Close();
    if ((~r).StatusCode != 200) {
        Ꮡt.Fatalf("expected 200 OK, got %s"u8, (~r).Status);
    }
}

// Issue 13996
public static void TestServeDirWithoutTrailingSlash(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServeDirWithoutTrailingSlash(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataˢ2 = "/testdata/"u8;

internal static void testServeDirWithoutTrailingSlash(ж<testing.T> Ꮡt, testMode mode) {
    @string e = testdataˢ2;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> rΔ1) => {
        ServeFile(w, rΔ1, "."u8);
    }))).Value.ts;
    var (r, err) = ts.Client().Get((~ts).URL + "/testdata"u8);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~r).Body.Close();
    {
        @string g = r.Value.Request.Value.URL.Value.Path; if (g != e) {
            Ꮡt.Errorf("got %s, want %s"u8, g, e);
        }
    }
}

// Tests that ServeFile doesn't add a Content-Length if a Content-Encoding is
// specified.
public static void TestServeFileWithContentEncoding(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServeFileWithContentEncoding(Δp0, Δp1));
}

internal static void testServeFileWithContentEncoding(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(contentEncodingˢ, fooˢ);
        ServeFile(w, r, testdataFileˢ);
        // Because the testdata is so small, it would fit in
        // both the h1 and h2 Server's write buffers. For h1,
        // sendfile is used, though, forcing a header flush at
        // the io.Copy. http2 doesn't do a header flush so
        // buffers all 11 bytes and then adds its own
        // Content-Length. To prevent the Server's
        // Content-Length and test ServeFile only, flush here.
        w._<Flusher>().Flush();
    })));
    var (resp, err) = (~cst).c.Get((~(~cst).ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~resp).Body.Close();
    {
        var (g, e) = (resp.Value.ContentLength, (int64)(-1)); if (g != e) {
            Ꮡt.Errorf("Content-Length mismatch: got %d, want %d"u8, g, e);
        }
    }
}

// Tests that ServeFile does not generate representation metadata when
// file has not been modified, as per RFC 7232 section 4.1.
public static void TestServeFileNotModified(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServeFileNotModified(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string etagˢ = "Etag"u8;
internal static readonly @string ifNoneMatchˢ = "If-None-Match"u8;
internal static readonly object readingBodyˢ = (@string)"reading Body:"u8;

internal static void testServeFileNotModified(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(contentTypeˢ, applicationJsonˢ);
        w.Header().Set(contentEncodingˢ, fooˢ);
        w.Header().Set(etagˢ, @"""123"""u8);
        ServeFile(w, r, testdataFileˢ);
        // Because the testdata is so small, it would fit in
        // both the h1 and h2 Server's write buffers. For h1,
        // sendfile is used, though, forcing a header flush at
        // the io.Copy. http2 doesn't do a header flush so
        // buffers all 11 bytes and then adds its own
        // Content-Length. To prevent the Server's
        // Content-Length and test ServeFile only, flush here.
        w._<Flusher>().Flush();
    })));
    var (req, err) = NewRequest(getˢ2, (~(~cst).ts).URL, default!);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~req).Header.Set(ifNoneMatchˢ, @"""123"""u8);
    (var resp, err) = (~cst).c.Do(req);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var b, err) = io.ReadAll((~resp).Body);
    (~resp).Body.Close();
    if (err != default!) {
        Ꮡt.Fatal(readingBodyˢ, err);
    }
    if (len(b) != 0) {
        Ꮡt.Errorf("non-empty body"u8);
    }
    {
        nint g = resp.Value.StatusCode;
        nint e = StatusNotModified; if (g != e) {
            Ꮡt.Errorf("status mismatch: got %d, want %d"u8, g, e);
        }
    }
    // HTTP1 transport sets ContentLength to 0.
    {
        var (g, e1, e2) = (resp.Value.ContentLength, (int64)(-1), (int64)0); if (g != e1 && g != e2) {
            Ꮡt.Errorf("Content-Length mismatch: got %d, want %d or %d"u8, g, e1, e2);
        }
    }
    if ((~resp).Header.Get(contentTypeˢ) != ""u8) {
        Ꮡt.Errorf("Content-Type present, but it should not be"u8);
    }
    if ((~resp).Header.Get(contentEncodingˢ) != ""u8) {
        Ꮡt.Errorf("Content-Encoding present, but it should not be"u8);
    }
}

public static void TestServeIndexHtml(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServeIndexHtml(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dirˢ = "Dir"u8;
internal static readonly @string dirFSˢ = "DirFS"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ = "index.html says hello\n"u8;

internal static void testServeIndexHtml(ж<testing.T> Ꮡt, testMode mode) {
    for (nint i = 0; i < 2; i++) {
        ref var h = ref heap<httpꓸHandler>(out var Ꮡh);
        @string name = default!;
        switch (i) {
        case 0: {
            h = FileServer(((Δhttp.Dir)(@string)"."u8));
            name = dirˢ;
            break;
        }
        case 1: {
            h = FileServer(FS(os.DirFS("."u8)));
            name = dirFSˢ;
            break;
        }}

        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            @string want = wantᶜ;
            var ts = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), mode, Ꮡh.ValueSlot).Value.ts;
            foreach (var (_, path) in new @string[]{"/testdata/"u8, "/testdata/index.html"u8}.slice()) {
                var (res, err) = ts.Client().Get((~ts).URL + path);
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
                (var b, err) = io.ReadAll((~res).Body);
                if (err != default!) {
                    tΔ1.Fatal(readingBodyˢ, err);
                }
                {
                    @string s = ((@string)b); if (s != want) {
                        tΔ1.Errorf("for path %q got %q, want %q"u8, path, s, want);
                    }
                }
                (~res).Body.Close();
            }
        });
    }
}

public static void TestServeIndexHtmlFS(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServeIndexHtmlFS(Δp0, Δp1));
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ1 = "index.html says hello\n"u8;

internal static void testServeIndexHtmlFS(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        @string want = wantᶜ1;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, FileServer(((Δhttp.Dir)(@string)"."u8))).Value.ts;
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);
        foreach (var (_, path) in new @string[]{"/testdata/"u8, "/testdata/index.html"u8}.slice()) {
            var (res, err) = ts.Client().Get((~ts).URL + path);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            (var b, err) = io.ReadAll((~res).Body);
            if (err != default!) {
                Ꮡt.Fatal(readingBodyˢ, err);
            }
            {
                @string s = ((@string)b); if (s != want) {
                    Ꮡt.Errorf("for path %q got %q, want %q"u8, path, s, want);
                }
            }
            (~res).Body.Close();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestFileServerZeroByte(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testFileServerZeroByte(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readResponseˢ = (@string)"ReadResponse: "u8;

internal static void testFileServerZeroByte(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, FileServer(((Δhttp.Dir)(@string)"."u8))).Value.ts;
        var (c, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        (_, err) = fmt.Fprintf(new http_test_package.net_ConnᴠWriter(c), "GET /..\x00 HTTP/1.0\r\n\r\n"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        ref var got = ref heap(new bytes.Buffer(), out var Ꮡgot);
        var bufr = bufio.NewReader(io.TeeReader(new http_test_package.net_ConnᴠReader(c), new http_test_package.bytes_BufferжWriter(Ꮡgot)));
        (var res, err) = ReadResponse(bufr, nil);
        if (err != default!) {
            Ꮡt.Fatal(readResponseˢ, err);
        }
        if ((~res).StatusCode == 200) {
            Ꮡt.Errorf("got status 200; want an error. Body is:\n%s"u8, got.Bytes());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestFileServerNamesEscape(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testFileServerNamesEscape(Δp0, Δp1));
}

internal static void testFileServerNamesEscape(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, FileServer(((Δhttp.Dir)(@string)testdataˢ))).Value.ts;
    foreach (var (_, path) in new @string[]{
        "/../testdata/file"u8,
        "/NUL"u8
    }.slice()) {
        // don't read from device files on Windows
        var (res, err) = ts.Client().Get((~ts).URL + path);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (~res).Body.Close();
        if ((~res).StatusCode < 400 || (~res).StatusCode > 599) {
            Ꮡt.Errorf("Get(%q): got status %v, want 4xx or 5xx"u8, path, (~res).StatusCode);
        }
    }
}

[GoType] partial struct fakeFileInfo {
    internal bool dir;
    internal @string basename;
    internal time.Time modtime;
    internal slice<ж<fakeFileInfo>> ents;
    internal @string contents;
    internal error err;
}

[GoRecv] internal static @string Name(this ref fakeFileInfo f) {
    return f.basename;
}

[GoRecv] internal static any Sys(this ref fakeFileInfo f) {
    return default!;
}

[GoRecv] internal static time.Time ModTime(this ref fakeFileInfo f) {
    return f.modtime;
}

[GoRecv] internal static bool IsDir(this ref fakeFileInfo f) {
    return f.dir;
}

[GoRecv] internal static int64 Size(this ref fakeFileInfo f) {
    return (int64)len(f.contents);
}

[GoRecv] internal static fs.FileMode Mode(this ref fakeFileInfo f) {
    if (f.dir) {
        return (fs.FileMode)(493 | fs.ModeDir);
    }
    return 420;
}

internal static @string String(this ж<fakeFileInfo> Ꮡf) {
    return fs.FormatFileInfo(new http_test_package.fakeFileInfoжFileInfo(Ꮡf));
}

[GoType] partial struct fakeFile {
    [GoEmbedded] public io_package.ReadSeeker ReadSeeker;
    internal ж<fakeFileInfo> fi;
    internal @string path; // as opened
    internal nint entpos;
}

[GoRecv] internal static error Close(this ref fakeFile f) {
    return default!;
}

[GoRecv] internal static (fs.FileInfo, error) Stat(this ref fakeFile f) {
    return (new http_test_package.fakeFileInfoжFileInfo(f.fi), default!);
}

[GoRecv] internal static (slice<fs.FileInfo>, error) Readdir(this ref fakeFile f, nint count) {
    if (!(~f.fi).dir) {
        return (default!, fs.ErrInvalid);
    }
    slice<fs.FileInfo> fis = default!;
    nint limit = f.entpos + count;
    if (count <= 0 || limit > len((~f.fi).ents)) {
        limit = len((~f.fi).ents);
    }
    for (; f.entpos < limit; f.entpos++) {
        fis = append(fis, (fs.FileInfo)(new http_test_package.fakeFileInfoжFileInfo((~f.fi).ents[f.entpos])));
    }
    if (len(fis) == 0 && count > 0){
        return (fis, io.EOF);
    } else {
        return (fis, default!);
    }
}

[GoType("map[@string, ж<fakeFileInfo>]")] partial struct fakeFS;

internal static (Δhttp.File, error) Open(this fakeFS fsys, @string name) {
    name = path.Clean(name);
    var (f, ok) = fsys[name, ꟷ];
    if (!ok) {
        return (default!, fs.ErrNotExist);
    }
    if ((~f).err != default!) {
        return (default!, (~f).err);
    }
    return (new http_test_package.fakeFileжFile(Ꮡ(new fakeFile(ReadSeeker: new http_test_package.strings_ReaderжReadSeeker(strings.NewReader((~f).contents)), fi: f, path: name))), default!);
}

public static void TestDirectoryIfNotModified(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testDirectoryIfNotModified(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ifModifiedSinceˢ = "If-Modified-Since"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string indexContentsᶜ = "I am a fake index.html file"u8;

internal static void testDirectoryIfNotModified(ж<testing.T> Ꮡt, testMode mode) {
    @string indexContents = indexContentsᶜ;
    ref var fileMod = ref heap<time.Time>(out var ᏑfileMod);
    fileMod = time.Unix(1000000000, 0).UTC();
    @string fileModStr = fileMod.Format(TimeFormat);
    ref var dirMod = ref heap<time.Time>(out var ᏑdirMod);
    dirMod = time.Unix(123, 0).UTC();
    var indexFile = Ꮡ(new fakeFileInfo(
        basename: "index.html"u8,
        modtime: fileMod,
        contents: indexContents
    ));
    var fs = new fakeFS(new map<@string, ж<fakeFileInfo>>{
        ["/"u8] = Ꮡ(new fakeFileInfo(
            dir: true,
            modtime: dirMod,
            ents: new ж<fakeFileInfo>[]{indexFile}.slice()
        )),
        ["/index.html"u8] = indexFile
    });
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, FileServer(fs)).Value.ts;
    var (res, err) = ts.Client().Get((~ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var b, err) = io.ReadAll((~res).Body);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (((sstring)b) != indexContents) {
        Ꮡt.Fatalf("Got body %q; want %q"u8, b, indexContents);
    }
    (~res).Body.Close();
    @string lastMod = (~res).Header.Get(lastModifiedˢ);
    if (lastMod != fileModStr) {
        Ꮡt.Fatalf("initial Last-Modified = %q; want %q"u8, lastMod, fileModStr);
    }
    var (req, _) = NewRequest(getˢ2, (~ts).URL, default!);
    (~req).Header.Set(ifModifiedSinceˢ, lastMod);
    var c = ts.Client();
    (res, err) = c.Do(req);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if ((~res).StatusCode != 304) {
        Ꮡt.Fatalf("Code after If-Modified-Since request = %v; want 304"u8, (~res).StatusCode);
    }
    (~res).Body.Close();
    // Advance the index.html file's modtime, but not the directory's.
    indexFile.Value.modtime = (~indexFile).modtime.Add((time.Duration)(3600000000000L));
    (res, err) = c.Do(req);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if ((~res).StatusCode != 200) {
        Ꮡt.Fatalf("Code after second If-Modified-Since request = %v; want 200; res is %#v"u8, (~res).StatusCode, res.OrTypedNil());
    }
    (~res).Body.Close();
}

internal static fs.FileInfo mustStat(ж<testing.T> Ꮡt, @string fileName) {
    var (fi, err) = os.Stat(fileName);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    return fi;
}

public static void TestServeContent(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServeContent(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string eTagˢ = "ETag"u8;
internal static readonly @string testdataIndexHtmlˢ = "testdata/index.html"u8;
internal static readonly @string htmlFooˢ = "<html>foo"u8;

[GoType("dyn")] internal partial struct testServeContent_serveParam {
    internal @string name;
    internal time.Time modtime;
    internal io.ReadSeeker content;
    internal @string contentType;
    internal @string etag;
}

[GoType("dyn")] internal partial struct testServeContent_testCase {
    // One of file or content must be set:
    internal @string @file;
    internal io.ReadSeeker content;
    internal time.Time modtime;
    internal @string serveETag; // optional
    internal @string serveContentType; // optional
    internal map<@string, @string> reqHeader;
    internal @string wantLastMod;
    internal @string wantContentType;
    internal @string wantContentRange;
    internal nint wantStatus;
}

internal static void testServeContent(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var servec = new channel<testServeContent_serveParam>(1);
        var servecʗ1 = servec;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var p = ᐸꟷ(servecʗ1);
            if (p.etag != ""u8) {
                w.Header().Set(eTagˢ, p.etag);
            }
            if (p.contentType != ""u8) {
                w.Header().Set(contentTypeˢ, p.contentType);
            }
            ServeContent(w, r, p.name, p.modtime, p.content);
        }))).Value.ts;
        var htmlModTime = mustStat(Ꮡt, testdataIndexHtmlˢ).ModTime();
        var tests = new map<@string, testServeContent_testCase>{
            ["no_last_modified"u8] = new(
                @file: "testdata/style.css"u8,
                wantContentType: "text/css; charset=utf-8"u8,
                wantStatus: 200
            ),
            ["with_last_modified"u8] = new(
                @file: "testdata/index.html"u8,
                wantContentType: "text/html; charset=utf-8"u8,
                modtime: htmlModTime,
                wantLastMod: htmlModTime.UTC().Format(TimeFormat),
                wantStatus: 200
            ),
            ["not_modified_modtime"u8] = new(
                @file: "testdata/style.css"u8,
                serveETag: @"""foo"""u8, // Last-Modified sent only when no ETag

                modtime: htmlModTime,
                reqHeader: new map<@string, @string>{
                    ["If-Modified-Since"u8] = htmlModTime.UTC().Format(TimeFormat)
                },
                wantStatus: 304
            ),
            ["not_modified_modtime_with_contenttype"u8] = new(
                @file: "testdata/style.css"u8,
                serveContentType: "text/css"u8, // explicit content type

                serveETag: @"""foo"""u8, // Last-Modified sent only when no ETag

                modtime: htmlModTime,
                reqHeader: new map<@string, @string>{
                    ["If-Modified-Since"u8] = htmlModTime.UTC().Format(TimeFormat)
                },
                wantStatus: 304
            ),
            ["not_modified_etag"u8] = new(
                @file: "testdata/style.css"u8,
                serveETag: @"""foo"""u8,
                reqHeader: new map<@string, @string>{
                    ["If-None-Match"u8] = @"""foo"""u8
                },
                wantStatus: 304
            ),
            ["not_modified_etag_no_seek"u8] = new(
                content: new panicOnSeek((io.ReadSeeker)(default!)), // should never be called

                serveETag: @"W/""foo"""u8, // If-None-Match uses weak ETag comparison

                reqHeader: new map<@string, @string>{
                    ["If-None-Match"u8] = @"""baz"", W/""foo"""u8
                },
                wantStatus: 304
            ),
            ["if_none_match_mismatch"u8] = new(
                @file: "testdata/style.css"u8,
                serveETag: @"""foo"""u8,
                reqHeader: new map<@string, @string>{
                    ["If-None-Match"u8] = @"""Foo"""u8
                },
                wantStatus: 200,
                wantContentType: "text/css; charset=utf-8"u8
            ),
            ["if_none_match_malformed"u8] = new(
                @file: "testdata/style.css"u8,
                serveETag: @"""foo"""u8,
                reqHeader: new map<@string, @string>{
                    ["If-None-Match"u8] = @","u8
                },
                wantStatus: 200,
                wantContentType: "text/css; charset=utf-8"u8
            ),
            ["range_good"u8] = new(
                @file: "testdata/style.css"u8,
                serveETag: @"""A"""u8,
                reqHeader: new map<@string, @string>{
                    ["Range"u8] = "bytes=0-4"u8
                },
                wantStatus: StatusPartialContent,
                wantContentType: "text/css; charset=utf-8"u8,
                wantContentRange: "bytes 0-4/8"u8
            ),
            ["range_match"u8] = new(
                @file: "testdata/style.css"u8,
                serveETag: @"""A"""u8,
                reqHeader: new map<@string, @string>{
                    ["Range"u8] = "bytes=0-4"u8,
                    ["If-Range"u8] = @"""A"""u8
                },
                wantStatus: StatusPartialContent,
                wantContentType: "text/css; charset=utf-8"u8,
                wantContentRange: "bytes 0-4/8"u8
            ),
            ["range_match_weak_etag"u8] = new(
                @file: "testdata/style.css"u8,
                serveETag: @"W/""A"""u8,
                reqHeader: new map<@string, @string>{
                    ["Range"u8] = "bytes=0-4"u8,
                    ["If-Range"u8] = @"W/""A"""u8
                },
                wantStatus: 200,
                wantContentType: "text/css; charset=utf-8"u8
            ),
            ["range_no_overlap"u8] = new(
                @file: "testdata/style.css"u8,
                serveETag: @"""A"""u8,
                reqHeader: new map<@string, @string>{
                    ["Range"u8] = "bytes=10-20"u8
                },
                wantStatus: StatusRequestedRangeNotSatisfiable,
                wantContentType: "text/plain; charset=utf-8"u8,
                wantContentRange: "bytes */8"u8
            ), // An If-Range resource for entity "A", but entity "B" is now current.
 // The Range request should be ignored.

            ["range_no_match"u8] = new(
                @file: "testdata/style.css"u8,
                serveETag: @"""A"""u8,
                reqHeader: new map<@string, @string>{
                    ["Range"u8] = "bytes=0-4"u8,
                    ["If-Range"u8] = @"""B"""u8
                },
                wantStatus: 200,
                wantContentType: "text/css; charset=utf-8"u8
            ),
            ["range_with_modtime"u8] = new(
                @file: "testdata/style.css"u8,
                modtime: time.Date(2014, 6, 25, 17, 12, 18, 0, /* nanos */
 time.ΔUTC),
                reqHeader: new map<@string, @string>{
                    ["Range"u8] = "bytes=0-4"u8,
                    ["If-Range"u8] = "Wed, 25 Jun 2014 17:12:18 GMT"u8
                },
                wantStatus: StatusPartialContent,
                wantContentType: "text/css; charset=utf-8"u8,
                wantContentRange: "bytes 0-4/8"u8,
                wantLastMod: "Wed, 25 Jun 2014 17:12:18 GMT"u8
            ),
            ["range_with_modtime_mismatch"u8] = new(
                @file: "testdata/style.css"u8,
                modtime: time.Date(2014, 6, 25, 17, 12, 18, 0, /* nanos */
 time.ΔUTC),
                reqHeader: new map<@string, @string>{
                    ["Range"u8] = "bytes=0-4"u8,
                    ["If-Range"u8] = "Wed, 25 Jun 2014 17:12:19 GMT"u8
                },
                wantStatus: StatusOK,
                wantContentType: "text/css; charset=utf-8"u8,
                wantLastMod: "Wed, 25 Jun 2014 17:12:18 GMT"u8
            ),
            ["range_with_modtime_nanos"u8] = new(
                @file: "testdata/style.css"u8,
                modtime: time.Date(2014, 6, 25, 17, 12, 18, 123, /* nanos */
 time.ΔUTC),
                reqHeader: new map<@string, @string>{
                    ["Range"u8] = "bytes=0-4"u8,
                    ["If-Range"u8] = "Wed, 25 Jun 2014 17:12:18 GMT"u8
                },
                wantStatus: StatusPartialContent,
                wantContentType: "text/css; charset=utf-8"u8,
                wantContentRange: "bytes 0-4/8"u8,
                wantLastMod: "Wed, 25 Jun 2014 17:12:18 GMT"u8
            ),
            ["unix_zero_modtime"u8] = new(
                content: new http_test_package.strings_ReaderжReadSeeker(strings.NewReader(htmlFooˢ)),
                modtime: time.Unix(0, 0),
                wantStatus: StatusOK,
                wantContentType: "text/html; charset=utf-8"u8
            ),
            ["ifmatch_matches"u8] = new(
                @file: "testdata/style.css"u8,
                serveETag: @"""A"""u8,
                reqHeader: new map<@string, @string>{
                    ["If-Match"u8] = @"""Z"", ""A"""u8
                },
                wantStatus: 200,
                wantContentType: "text/css; charset=utf-8"u8
            ),
            ["ifmatch_star"u8] = new(
                @file: "testdata/style.css"u8,
                serveETag: @"""A"""u8,
                reqHeader: new map<@string, @string>{
                    ["If-Match"u8] = @"*"u8
                },
                wantStatus: 200,
                wantContentType: "text/css; charset=utf-8"u8
            ),
            ["ifmatch_failed"u8] = new(
                @file: "testdata/style.css"u8,
                serveETag: @"""A"""u8,
                reqHeader: new map<@string, @string>{
                    ["If-Match"u8] = @"""B"""u8
                },
                wantStatus: 412
            ),
            ["ifmatch_fails_on_weak_etag"u8] = new(
                @file: "testdata/style.css"u8,
                serveETag: @"W/""A"""u8,
                reqHeader: new map<@string, @string>{
                    ["If-Match"u8] = @"W/""A"""u8
                },
                wantStatus: 412
            ),
            ["if_unmodified_since_true"u8] = new(
                @file: "testdata/style.css"u8,
                modtime: htmlModTime,
                reqHeader: new map<@string, @string>{
                    ["If-Unmodified-Since"u8] = htmlModTime.UTC().Format(TimeFormat)
                },
                wantStatus: 200,
                wantContentType: "text/css; charset=utf-8"u8,
                wantLastMod: htmlModTime.UTC().Format(TimeFormat)
            ),
            ["if_unmodified_since_false"u8] = new(
                @file: "testdata/style.css"u8,
                modtime: htmlModTime,
                reqHeader: new map<@string, @string>{
                    ["If-Unmodified-Since"u8] = htmlModTime.Add(-2 * time.ΔSecond).UTC().Format(TimeFormat)
                },
                wantStatus: 412,
                wantLastMod: htmlModTime.UTC().Format(TimeFormat)
            )
        };
        foreach (var (testName, tt) in tests) {
            io.ReadSeeker content = default!;
            if (tt.@file != ""u8){
                var (f, err) = os.Open(tt.@file);
                if (err != default!) {
                    Ꮡt.Fatalf("test %q: %v"u8, testName, err);
                }
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);
                content = new http_test_package.os_FileжReadSeeker(f);
            } else {
                content = tt.content;
            }
            foreach (var (_, method) in new @string[]{"GET"u8, "HEAD"u8}.slice()) {
                //restore content in case it is consumed by previous method
                {
                    var (contentΔ1, ok) = content._<ж<strings.Reader>>(ᐧ); if (ok) {
                        contentΔ1.Seek(0, io.SeekStart);
                    }
                }
                servec.ᐸꟷ(new testServeContent_serveParam(
                    name: filepath.Base(tt.@file),
                    content: content,
                    modtime: tt.modtime,
                    etag: tt.serveETag,
                    contentType: tt.serveContentType
                ));
                var (req, err) = NewRequest(method, (~ts).URL, default!);
                if (err != default!) {
                    Ꮡt.Fatal(err);
                }
                foreach (var (k, v) in tt.reqHeader) {
                    (~req).Header.Set(k, v);
                }
                var c = ts.Client();
                (var res, err) = c.Do(req);
                if (err != default!) {
                    Ꮡt.Fatal(err);
                }
                io.Copy(io.Discard, (~res).Body);
                (~res).Body.Close();
                if ((~res).StatusCode != tt.wantStatus) {
                    Ꮡt.Errorf("test %q using %q: got status = %d; want %d"u8, testName, method, (~res).StatusCode, tt.wantStatus);
                }
                {
                    @string g = (~res).Header.Get(contentTypeˢ);
                    @string e = tt.wantContentType; if (g != e) {
                        Ꮡt.Errorf("test %q using %q: got content-type = %q, want %q"u8, testName, method, g, e);
                    }
                }
                {
                    @string g = (~res).Header.Get(contentRangeˢ);
                    @string e = tt.wantContentRange; if (g != e) {
                        Ꮡt.Errorf("test %q using %q: got content-range = %q, want %q"u8, testName, method, g, e);
                    }
                }
                {
                    @string g = (~res).Header.Get(lastModifiedˢ);
                    @string e = tt.wantLastMod; if (g != e) {
                        Ꮡt.Errorf("test %q using %q: got last-modified = %q, want %q"u8, testName, method, g, e);
                    }
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpFooˢ = "http://foo/"u8;
internal static readonly @string fileTxtˢ = "file.txt"u8;
internal static readonly @string forbiddenˢ = "Forbidden"u8;

// Issue 12991
public static void TestServerFileStatError(ж<testing.T> Ꮡt) {
    var rec = httptest.NewRecorder();
    var (r, _) = NewRequest(getˢ2, httpFooˢ, default!);
    var redirect = false;
    @string name = fileTxtˢ;
    var fs = new issue12991FS(nil);
    http_internal_test_package.ExportServeFile(new http_test_package.httptest_ResponseRecorderжResponseWriter(rec), r, fs, name, redirect);
    {
        @string body = (~rec).Body.String(); if (!strings.Contains(body, "403"u8) || !strings.Contains(body, forbiddenˢ)) {
            Ꮡt.Errorf("wanted 403 forbidden message; got: %s"u8, body);
        }
    }
}

[GoType] partial struct issue12991FS {
}

internal static (Δhttp.File, error) Open(this issue12991FS _Δp0, @string _Δp1) {
    return (new issue12991File(nil), default!);
}

[GoType] partial struct issue12991File {
    [GoEmbedded] public global::go.net.http_package.File File;
}

internal static (fs.FileInfo, error) Stat(this issue12991File _) {
    return (default!, fs.ErrPermission);
}

internal static error Close(this issue12991File _) {
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string keepheaders0ˢ = "keepheaders=0"u8;
internal static readonly @string keepheaders1ˢ = "keepheaders=1"u8;

public static void TestFileServerErrorMessages(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        tΔ1.Run(keepheaders0ˢ, (ж<testing.T> tΔ2) => {
            testFileServerErrorMessages(tΔ2, mode, false);
        });
        tΔ1.Run(keepheaders1ˢ, (ж<testing.T> tΔ3) => {
            testFileServerErrorMessages(tΔ3, mode, true);
        });
    }, testNotParallel);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string randomErrorˢ = "random error"u8;
internal static readonly @string tudeˢ = "étude"u8;
internal static readonly @string cacheControlˢ = "Cache-Control"u8;
internal static readonly @string yesˢ = "yes"u8;
internal static readonly @string awesomeˢ = "awesome"u8;
internal static readonly @string yesterdayˢ = "yesterday"u8;
internal static readonly @string notPresentˢ = "not present"u8;
internal static readonly @string presentˢ = "present"u8;

internal static void testFileServerErrorMessages(ж<testing.T> Ꮡt, testMode mode, bool keepHeaders) {
    if (keepHeaders) {
        Ꮡt.Setenv(godebugˢ, "httpservecontentkeepheaders=1"u8);
    }
    var fsΔ1 = new fakeFS(new map<@string, ж<fakeFileInfo>>{
        ["/500"u8] = Ꮡ(new fakeFileInfo(
            err: errors.New(randomErrorˢ)
        )),
        ["/403"u8] = Ꮡ(new fakeFileInfo(
            err: new fs.PathErrorжerror(Ꮡ(new fs.PathError(Err: fs.ErrPermission)))
        ))
    });
    var server = FileServer(fsΔ1);
    var serverʗ1 = server;
    var h = (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(etagˢ, tudeˢ);
        w.Header().Set(cacheControlˢ, yesˢ);
        w.Header().Set(contentTypeˢ, awesomeˢ);
        w.Header().Set(lastModifiedˢ, yesterdayˢ);
        serverʗ1.ServeHTTP(w, r);
    };
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(NilSafeDelegateConversion<Δhttp.HandlerFunc, Action<Δhttp.ResponseWriter, ж<Δhttp.Request>>>(h))).Value.ts;
    var c = ts.Client();
    foreach (var (_, code) in new nint[]{403, 404, 500}.slice()) {
        var (res, err) = c.Get(fmt.Sprintf("%s/%d"u8, (~ts).URL, code));
        if (err != default!) {
            Ꮡt.Errorf("Error fetching /%d: %v"u8, code, err);
            continue;
        }
        (~res).Body.Close();
        if ((~res).StatusCode != code) {
            Ꮡt.Errorf("GET /%d: StatusCode = %d; want %d"u8, code, (~res).StatusCode, code);
        }
        foreach (var (_, hdr) in new @string[]{"Etag"u8, "Last-Modified"u8, "Cache-Control"u8}.slice()) {
            {
                var (v, got) = (~res).Header[hdr, ꟷ]; if (got != keepHeaders) {
                    @string want = notPresentˢ;
                    if (keepHeaders) {
                        want = presentˢ;
                    }
                    Ꮡt.Errorf("GET /%d: Header[%q] = %q, want %v"u8, code, hdr, v, want);
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingLinuxOnlyTestˢ = (@string)"skipping; linux-only test"u8;
internal static readonly @string straceˢ = "strace"u8;
internal static readonly object skippingStraceNotFoundInˢ = (@string)"skipping; strace not found in path"u8;
internal static readonly @string testRunˢ = "-test.run=^$"u8;
internal static readonly @string testRunˢ2 = "-test.run=^TestLinuxSendfileChild$"u8;
internal static readonly @string bN64Sendfile64ˢ = @"\b(n64:)?sendfile(64)?\("u8;

// verifies that sendfile is being used on Linux
public static void TestLinuxSendfile(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        setParallel(Ꮡt);
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        if (runtime.GOOS != "linux"u8) {
            Ꮡt.Skip(skippingLinuxOnlyTestˢ);
        }
        {
            var (_, errΔ1) = exec.LookPath(straceˢ); if (errΔ1 != default!) {
                Ꮡt.Skip(skippingStraceNotFoundInˢ);
            }
        }
        var (ln, err) = net.Listen(tcpˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var lnf, err) = ln._<ж<net.TCPListener>>().File();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        // Attempt to run strace, and skip on failure - this test requires SYS_PTRACE.
        {
            var errΔ2 = testenv.Command(new http_test_package.testing_TжTB(Ꮡt), straceˢ, "-f"u8, "-q", os.Args[0], testRunˢ).Run(); if (errΔ2 != default!) {
                Ꮡt.Skipf("skipping; failed to run strace: %v"u8, errΔ2);
            }
        }
        @string filename = fmt.Sprintf("1kb-%d"u8, os.Getpid());
        @string filepath = path.Join(os.TempDir(), filename);
        {
            var errΔ3 = os.WriteFile(filepath, bytes.Repeat(new byte[]{(rune)'a'}.slice(), (1 << (int)(10))), 493); if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
        }
        defer(os.Remove, filepath, ref ᒐ);
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        var child = testenv.Command(new http_test_package.testing_TжTB(Ꮡt), straceˢ, "-f"u8, "-q", os.Args[0], testRunˢ2);
        child.Value.ExtraFiles = append((~child).ExtraFiles, lnf);
        child.Value.Env = appendꓸꓸꓸ(new @string[]{"GO_WANT_HELPER_PROCESS=1"u8}.slice(), os.Environ());
        child.Value.Stdout = new http_test_package.strings_BuilderжWriter(Ꮡbuf);
        child.Value.Stderr = new http_test_package.strings_BuilderжWriter(Ꮡbuf);
        {
            var errΔ4 = child.Start(); if (errΔ4 != default!) {
                Ꮡt.Skipf("skipping; failed to start straced child: %v"u8, errΔ4);
            }
        }
        (var res, err) = Get(fmt.Sprintf("http://%s/%s"u8, ln.Addr(), filename));
        if (err != default!) {
            Ꮡt.Fatalf("http client error: %v"u8, err);
        }
        (_, err) = io.Copy(io.Discard, (~res).Body);
        if (err != default!) {
            Ꮡt.Fatalf("client body read error: %v"u8, err);
        }
        (~res).Body.Close();
        // Force child to exit cleanly.
        Post(fmt.Sprintf("http://%s/quit"u8, ln.Addr()), ""u8, default!);
        child.Wait();
        var rx = regexp.MustCompile(bN64Sendfile64ˢ);
        @string @out = buf.String();
        if (!rx.MatchString(@out)) {
            Ꮡt.Errorf("no sendfile system call found in:\n%s"u8, @out);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static (ж<Δhttp.Response>, slice<byte>) getBody(ж<testing.T> Ꮡt, @string testName, Δhttp.Request reqʗp, ж<Δhttp.Client> Ꮡclient) {
    ref var req = ref heap(reqʗp, out var Ꮡreq);

    var (r, err) = Ꮡclient.Do(Ꮡreq);
    if (err != default!) {
        Ꮡt.Fatalf("%s: for URL %q, send error: %v"u8, testName, req.URL.String(), err);
    }
    (var b, err) = io.ReadAll((~r).Body);
    if (err != default!) {
        Ꮡt.Fatalf("%s: for URL %q, reading body: %v"u8, testName, req.URL.String(), err);
    }
    return (r, b);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goWantHelperProcessˢ = "GO_WANT_HELPER_PROCESS"u8;
internal static readonly @string ephemeralPortListenerˢ = "ephemeral-port-listener"u8;
internal static readonly @string quitˢ = "/quit"u8;

// TestLinuxSendfileChild isn't a real test. It's used as a helper process
// for TestLinuxSendfile.
public static void TestLinuxSendfileChild(ж<testing.T> _) {
    GoFrame ᒐ = default;
    try {
        if (os.Getenv(goWantHelperProcessˢ) != "1"u8) {
            return;
        }
        defer(os.Exit, (nint)(0), ref ᒐ);
        var fd3 = os.NewFile(3, ephemeralPortListenerˢ);
        var (ln, err) = net.FileListener(fd3);
        if (err != default!) {
            throw panic(err);
        }
        var mux = NewServeMux();
        mux.Handle("/"u8, FileServer(((Δhttp.Dir)os.TempDir())));
        mux.HandleFunc(quitˢ, (Δhttp.ResponseWriter _Δp0, ж<Δhttp.Request> _Δp1) => {
            os.Exit(0);
        });
        var s = Ꮡ(new Server(Handler: new Δhttp.ServeMuxжΔHandler(mux)));
        err = s.Serve(ln);
        if (err != default!) {
            throw panic(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issues 18984, 49552: tests that requests for paths beyond files return not-found errors
public static void TestFileServerNotDirError(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        tΔ1.Run(dirˢ, (ж<testing.T> tΔ2) => {
            testFileServerNotDirError(tΔ2, mode, (@string path) => ((Δhttp.Dir)path));
        });
        tΔ1.Run("FS"u8, (ж<testing.T> tΔ3) => {
            testFileServerNotDirError(tΔ3, mode, (@string path) => FS(os.DirFS(path)));
        });
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string indexHtmlNotAFileˢ = "/index.html/not-a-file"u8;
internal static readonly object errNilWantNilˢ = (@string)"err == nil; want != nil"u8;
internal static readonly @string indexHtmlNotADirNotAFileˢ = "/index.html/not-a-dir/not-a-file"u8;
internal static readonly object getAbsPathˢ = (@string)"get abs path:"u8;
internal static readonly @string relativePathˢ = "RelativePath"u8;
internal static readonly @string absolutePathˢ = "AbsolutePath"u8;

internal static void testFileServerNotDirError(ж<testing.T> Ꮡt, testMode mode, Func<@string, Δhttp.FileSystem> newfs) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, FileServer(newfs(testdataˢ))).Value.ts;
    ref var err = ref heap<error>(out var Ꮡerr);
    (var res, err) = ts.Client().Get((~ts).URL + "/index.html/not-a-file"u8);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
    if ((~res).StatusCode != 404) {
        Ꮡt.Errorf("StatusCode = %v; want 404"u8, (~res).StatusCode);
    }
    void test(@string name, Δhttp.FileSystem fsys) {
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            (_, Ꮡerr.ValueSlot) = fsys.Open(indexHtmlNotAFileˢ);
            if (Ꮡerr.ValueSlot == default!) {
                tΔ1.Fatal(errNilWantNilˢ);
            }
            if (!errors.Is(Ꮡerr.ValueSlot, fs.ErrNotExist)) {
                tΔ1.Errorf("err = %v; errors.Is(err, fs.ErrNotExist) = %v; want true"u8, Ꮡerr.ValueSlot,
                    errors.Is(Ꮡerr.ValueSlot, fs.ErrNotExist));
            }
            (_, Ꮡerr.ValueSlot) = fsys.Open(indexHtmlNotADirNotAFileˢ);
            if (Ꮡerr.ValueSlot == default!) {
                tΔ1.Fatal(errNilWantNilˢ);
            }
            if (!errors.Is(Ꮡerr.ValueSlot, fs.ErrNotExist)) {
                tΔ1.Errorf("err = %v; errors.Is(err, fs.ErrNotExist) = %v; want true"u8, Ꮡerr.ValueSlot,
                    errors.Is(Ꮡerr.ValueSlot, fs.ErrNotExist));
            }
        });
    }
    (var absPath, err) = filepath.Abs(testdataˢ);
    if (err != default!) {
        Ꮡt.Fatal(getAbsPathˢ, err);
    }
    test(relativePathˢ, newfs(testdataˢ));
    test(absolutePathˢ, newfs(absPath));
}

[GoType("dyn")] internal partial struct TestFileServerCleanPath_tests {
    internal @string path;
    internal nint wantCode;
    internal slice<@string> wantOpen;
}

public static void TestFileServerCleanPath(ж<testing.T> Ꮡt) {
    var tests = new TestFileServerCleanPath_tests[]{
        new("/"u8, 200, new @string[]{"/"u8, "/index.html"u8}.slice()),
        new("/dir"u8, 301, new @string[]{"/dir"u8}.slice()),
        new("/dir/"u8, 200, new @string[]{"/dir"u8, "/dir/index.html"u8}.slice())
    }.slice();
    foreach (var (_, tt) in tests) {
        ref var log = ref heap<slice<@string>>(out var Ꮡlog);
        var rr = httptest.NewRecorder();
        var (req, _) = NewRequest(getˢ2, "http://foo.localhost"u8 + tt.path, default!);
        FileServer(new fileServerCleanPathDir(Ꮡlog)).ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(rr), req);
        if (!slices.Equal<slice<@string>, @string>(log, tt.wantOpen)) {
            Ꮡt.Logf("For %s: Opens = %q; want %q"u8, tt.path, log, tt.wantOpen);
        }
        if ((~rr).Code != tt.wantCode) {
            Ꮡt.Logf("For %s: Response code = %d; want %d"u8, tt.path, (~rr).Code, tt.wantCode);
        }
    }
}

[GoType] partial struct fileServerCleanPathDir {
    internal ж<slice<@string>> log;
}

internal static (Δhttp.File, error) Open(this fileServerCleanPathDir d, @string path) {
    (d.log).ValueSlot = append((d.log).ValueSlot, path);
    if (path == "/"u8 || path == "/dir"u8 || path == "/dir/"u8) {
        // Just return back something that's a directory.
        return ((Δhttp.Dir)(@string)"."u8).Open("."u8);
    }
    return (default!, fs.ErrNotExist);
}

[GoType] partial struct panicOnSeek {
    [GoEmbedded] public io_package.ReadSeeker ReadSeeker;
}

[GoType("dyn")] internal partial struct TestScanETag_tests {
    internal @string @in;
    internal @string wantETag;
    internal @string wantRemain;
}

public static void TestScanETag(ж<testing.T> Ꮡt) {
    var tests = new TestScanETag_tests[]{
        new(@"W/""etag-1"""u8, @"W/""etag-1"""u8, ""u8),
        new(@"""etag-2"""u8, @"""etag-2"""u8, ""u8),
        new(@"""etag-1"", ""etag-2"""u8, @"""etag-1"""u8, @", ""etag-2"""u8),
        new(""u8, ""u8, ""u8),
        new("W/"u8, ""u8, ""u8),
        new(@"W/""truc"u8, ""u8, ""u8),
        new(@"w/""case-sensitive"""u8, ""u8, ""u8),
        new(@"""spaced etag"""u8, ""u8, ""u8)
    }.slice();
    foreach (var (_, test) in tests) {
        var (etag, remain) = http_internal_test_package.ExportScanETag(test.@in);
        if (etag != test.wantETag || remain != test.wantRemain) {
            Ꮡt.Errorf("scanETag(%q)=%q %q, want %q %q"u8, test.@in, etag, remain, test.wantETag, test.wantRemain);
        }
    }
}

// Issue 40940: Ensure that we only accept non-negative suffix-lengths
// in "Range": "bytes=-N", and should reject "bytes=--2".
public static void TestServeFileRejectsInvalidSuffixLengths(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServeFileRejectsInvalidSuffixLengths(Δp0, Δp1), new testMode[]{http1Mode, https1Mode, http2Mode}.slice());
}

[GoType("dyn")] internal partial struct testServeFileRejectsInvalidSuffixLengths_tests {
    internal @string r;
    internal nint wantCode;
    internal @string wantBody;
}

internal static void testServeFileRejectsInvalidSuffixLengths(ж<testing.T> Ꮡt, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, FileServer(((Δhttp.Dir)(@string)testdataˢ))).Value.ts;
    var tests = new testServeFileRejectsInvalidSuffixLengths_tests[]{
        new("bytes=--6"u8, 416, "invalid range\n"u8),
        new("bytes=--0"u8, 416, "invalid range\n"u8),
        new("bytes=---0"u8, 416, "invalid range\n"u8),
        new("bytes=-6"u8, 206, "hello\n"u8),
        new("bytes=6-"u8, 206, "html says hello\n"u8),
        new("bytes=-6-"u8, 416, "invalid range\n"u8),
        new("bytes=-0"u8, 206, ""u8),
        new("bytes="u8, 200, "index.html says hello\n"u8)
    }.slice();
    foreach (var (_, tt) in tests) {
        ref var ttΔ1 = ref heap<testServeFileRejectsInvalidSuffixLengths_tests>(out var ᏑttΔ1);
        ttΔ1 = tt;
        var cstʗ1 = cst;
        var ttʗ1 = ttΔ1;
        Ꮡt.Run(ttΔ1.r, (ж<testing.T> tΔ1) => {
            var (req, err) = NewRequest(getˢ2, (~cstʗ1).URL + "/index.html"u8, default!);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            (~req).Header.Set(rangeˢ, ttʗ1.r);
            (var res, err) = cstʗ1.Client().Do(req);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            {
                nint g = res.Value.StatusCode;
                nint w = ttʗ1.wantCode; if (g != w) {
                    tΔ1.Errorf("StatusCode mismatch: got %d want %d"u8, g, w);
                }
            }
            (var slurp, err) = io.ReadAll((~res).Body);
            (~res).Body.Close();
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            {
                @string g = ((@string)slurp);
                @string w = ttʗ1.wantBody; if (g != w) {
                    tΔ1.Fatalf("Content mismatch:\nGot:  %q\nWant: %q"u8, g, w);
                }
            }
        });
    }
}

public static void TestFileServerMethods(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testFileServerMethods(Δp0, Δp1));
}

internal static void testFileServerMethods(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, FileServer(((Δhttp.Dir)(@string)testdataˢ))).Value.ts;
    var (@file, err) = os.ReadFile(testFile);
    if (err != default!) {
        Ꮡt.Fatal(readingFileˢ, err);
    }
    // Get contents via various methods.
    //
    // See https://go.dev/issue/59471 for a proposal to limit the set of methods handled.
    // For now, test the historical behavior.
    foreach (var (_, method) in new @string[]{
        MethodGet,
        MethodHead,
        MethodPost,
        MethodPut,
        MethodPatch,
        MethodDelete,
        MethodOptions,
        MethodTrace
    }.slice()) {
        var (req, _) = NewRequest(method, (~ts).URL + "/file"u8, default!);
        Ꮡt.Log((~req).URL.OrTypedNil());
        var (res, errΔ1) = ts.Client().Do(req);
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
        (var body, errΔ1) = io.ReadAll((~res).Body);
        (~res).Body.Close();
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
        var wantBody = @file;
        if (method == MethodHead) {
            wantBody = default!;
        }
        if (!bytes.Equal(body, wantBody)) {
            Ꮡt.Fatalf("%v: got body %q, want %q"u8, method, body, wantBody);
        }
        {
            @string got = (~res).Header.Get(contentLengthˢ);
            @string want = fmt.Sprint(len(@file)); if (got != want) {
                Ꮡt.Fatalf("%v: got Content-Length %q, want %q"u8, method, got, want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string indexHtmlˢ = "index.html"u8;

public static void TestFileServerFS(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string filename = indexHtmlˢ;
        var contents = slice<byte>("index.html says hello"u8);
        var fsys = new fstest.MapFS(new map<@string, ж<fstest.MapFile>>{
            [filename] = Ꮡ(new fstest.MapFile(Data: contents))
        });
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), http1Mode, FileServerFS(fsys)).Value.ts;
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);
        var (res, err) = ts.Client().Get((~ts).URL + "/"u8 + filename);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var b, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            Ꮡt.Fatal(readingBodyˢ, err);
        }
        {
            @string s = ((@string)b); if (s != ((sstring)contents)) {
                Ꮡt.Errorf("for path %q got %q, want %q"u8, filename, s, contents);
            }
        }
        (~res).Body.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestServeFileFS(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string filename = indexHtmlˢ;
        var contents = slice<byte>("index.html says hello"u8);
        var fsys = new fstest.MapFS(new map<@string, ж<fstest.MapFile>>{
            [filename] = Ꮡ(new fstest.MapFile(Data: contents))
        });
        var fsysʗ1 = fsys;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), http1Mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            ServeFileFS(w, r, fsysʗ1, filename);
        }))).Value.ts;
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);
        var (res, err) = ts.Client().Get((~ts).URL + "/"u8 + filename);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var b, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            Ꮡt.Fatal(readingBodyˢ, err);
        }
        {
            @string s = ((@string)b); if (s != ((sstring)contents)) {
                Ꮡt.Errorf("for path %q got %q, want %q"u8, filename, s, contents);
            }
        }
        (~res).Body.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestServeFileZippingResponseWriter(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // This test exercises a pattern which is incorrect,
        // but has been observed enough in the world that we don't want to break it.
        //
        // The server is setting "Content-Encoding: gzip",
        // wrapping the ResponseWriter in an implementation which gzips data written to it,
        // and passing this ResponseWriter to ServeFile.
        //
        // This means ServeFile cannot properly set a Content-Length header, because it
        // doesn't know what content it is going to send--the ResponseWriter is modifying
        // the bytes sent.
        //
        // Range requests are always going to be broken in this scenario,
        // but verify that we can serve non-range requests correctly.
        @string filename = indexHtmlˢ;
        var contents = slice<byte>("contents will be sent with Content-Encoding: gzip"u8);
        var fsys = new fstest.MapFS(new map<@string, ж<fstest.MapFile>>{
            [filename] = Ꮡ(new fstest.MapFile(Data: contents))
        });
        var fsysʗ1 = fsys;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), http1Mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            GoFrame ᒐ = default;
            try {
                w.Header().Set(contentEncodingˢ, gzipˢ);
                var gzw = gzip.NewWriter(new http_test_package.http_ResponseWriterᴠWriter(w));
                var gzwʗ1 = gzw;
                defer(() => gzwʗ1.Close(), ref ᒐ);
                ServeFileFS(new gzipResponseWriter(w: gzw, ResponseWriter: w), r, fsysʗ1, filename);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))).Value.ts;
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);
        var (res, err) = ts.Client().Get((~ts).URL + "/"u8 + filename);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var b, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            Ꮡt.Fatal(readingBodyˢ, err);
        }
        {
            @string s = ((@string)b); if (s != ((sstring)contents)) {
                Ꮡt.Errorf("for path %q got %q, want %q"u8, filename, s, contents);
            }
        }
        (~res).Body.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] partial struct gzipResponseWriter {
    [GoEmbedded] public global::go.net.http_package.ResponseWriter ResponseWriter;
    internal ж<gzip.Writer> w;
}

internal static (nint, error) Write(this gzipResponseWriter grw, slice<byte> b) {
    return grw.w.Write(b);
}

internal static void Flush(this gzipResponseWriter grw) {
    grw.w.Flush();
    {
        var (fw, ok) = grw.ResponseWriter._<Δhttp.Flusher>(ᐧ); if (ok) {
            fw.Flush();
        }
    }
}

// Issue 63769
public static void TestFileServerDirWithRootFile(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testFileServerDirWithRootFile(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fileServerˢ = "FileServer"u8;
internal static readonly @string fileServerFSˢ = "FileServerFS"u8;

internal static void testFileServerDirWithRootFile(ж<testing.T> Ꮡt, testMode mode) {
    void testDirFile(ж<testing.T> tΔ1, httpꓸHandler h) {
        GoFrame ᒐ = default;
        try {
            var ts = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), mode, h).Value.ts;
            var tsʗ1 = ts;
            defer(tsʗ1.Close, ref ᒐ);
            var (res, err) = ts.Client().Get((~ts).URL);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            {
                nint g = res.Value.StatusCode;
                nint w = StatusInternalServerError; if (g != w) {
                    tΔ1.Errorf("StatusCode mismatch: got %d, want: %d"u8, g, w);
                }
            }
            (~res).Body.Close();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    var testDirFileʗ1 = testDirFile;
    Ꮡt.Run(fileServerˢ, (ж<testing.T> tΔ2) => {
        testDirFileʗ1(tΔ2, FileServer(((Δhttp.Dir)(@string)testdataIndexHtmlˢ)));
    });
    var testDirFileʗ2 = testDirFile;
    Ꮡt.Run(fileServerFSˢ, (ж<testing.T> tΔ3) => {
        testDirFileʗ2(tΔ3, FileServerFS(os.DirFS(testdataIndexHtmlˢ)));
    });
}

public static void TestServeContentHeadersWithError(ж<testing.T> Ꮡt) {
    Ꮡt.Run(keepheaders0ˢ, (ж<testing.T> tΔ1) => {
        testServeContentHeadersWithError(tΔ1, false);
    });
    Ꮡt.Run(keepheaders1ˢ, (ж<testing.T> tΔ2) => {
        testServeContentHeadersWithError(tΔ2, true);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string applicationOctetStreamˢ = "application/octet-stream"u8;
internal static readonly @string abcdefghˢ = @"""abcdefgh"""u8;
internal static readonly @string wed21Oct2015072800Gmtˢ = "Wed, 21 Oct 2015 07:28:00 GMT"u8;
internal static readonly @string immutableˢ = "immutable"u8;
internal static readonly @string otherHeaderˢ = "Other-Header"u8;
internal static readonly @string testˢ2 = "test"u8;
internal static readonly @string bytes10010000ˢ = "bytes=100-10000"u8;
internal static readonly @string invalidRangeFailedToˢ = "invalid range: failed to overlap\n"u8;
internal static readonly @string bytes7ˢ = "bytes */7"u8;

internal static void testServeContentHeadersWithError(ж<testing.T> Ꮡt, bool keepHeaders) {
    GoFrame ᒐ = default;
    try {
        if (keepHeaders) {
            Ꮡt.Setenv(godebugˢ, "httpservecontentkeepheaders=1"u8);
        }
        var contents = slice<byte>("content"u8);
        var contentsʗ1 = contents;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), http1Mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.Header().Set(contentTypeˢ, applicationOctetStreamˢ);
            w.Header().Set(contentLengthˢ, strconv.Itoa(len(contentsʗ1)));
            w.Header().Set(contentEncodingˢ, gzipˢ);
            w.Header().Set(etagˢ, abcdefghˢ);
            w.Header().Set(lastModifiedˢ, wed21Oct2015072800Gmtˢ);
            w.Header().Set(cacheControlˢ, immutableˢ);
            w.Header().Set(otherHeaderˢ, testˢ2);
            ServeContent(w, r, ""u8, new time.Time(nil), new http_test_package.bytes_ReaderжReadSeeker(bytes.NewReader(contentsʗ1)));
        }))).Value.ts;
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);
        var (req, err) = NewRequest(getˢ2, (~ts).URL, default!);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (~req).Header.Set(rangeˢ, bytes10010000ˢ);
        var c = ts.Client();
        (var res, err) = c.Do(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var (@out, _) = io.ReadAll((~res).Body);
        (~res).Body.Close();
        @string ifKept(@string s) {
            if (keepHeaders) {
                return s;
            }
            return ""u8;
        }
        {
            nint g = res.Value.StatusCode;
            nint e = 416; if (g != e) {
                Ꮡt.Errorf("got status = %d; want %d"u8, g, e);
            }
        }
        {
            @string g = ((@string)@out);
            @string e = invalidRangeFailedToˢ; if (g != e) {
                Ꮡt.Errorf("got body = %q; want %q"u8, g, e);
            }
        }
        {
            @string g = (~res).Header.Get(contentTypeˢ);
            @string e = textPlainCharsetUtf8ˢ; if (g != e) {
                Ꮡt.Errorf("got content-type = %q, want %q"u8, g, e);
            }
        }
        {
            @string g = (~res).Header.Get(contentLengthˢ);
            @string e = strconv.Itoa(len(@out)); if (g != e) {
                Ꮡt.Errorf("got content-length = %q, want %q"u8, g, e);
            }
        }
        {
            @string g = (~res).Header.Get(contentEncodingˢ);
            @string e = ifKept(gzipˢ); if (g != e) {
                Ꮡt.Errorf("got content-encoding = %q, want %q"u8, g, e);
            }
        }
        {
            @string g = (~res).Header.Get(etagˢ);
            @string e = ifKept(abcdefghˢ); if (g != e) {
                Ꮡt.Errorf("got etag = %q, want %q"u8, g, e);
            }
        }
        {
            @string g = (~res).Header.Get(lastModifiedˢ);
            @string e = ifKept(wed21Oct2015072800Gmtˢ); if (g != e) {
                Ꮡt.Errorf("got last-modified = %q, want %q"u8, g, e);
            }
        }
        {
            @string g = (~res).Header.Get(cacheControlˢ);
            @string e = ifKept(immutableˢ); if (g != e) {
                Ꮡt.Errorf("got cache-control = %q, want %q"u8, g, e);
            }
        }
        {
            @string g = (~res).Header.Get(contentRangeˢ);
            @string e = bytes7ˢ; if (g != e) {
                Ꮡt.Errorf("got content-range = %q, want %q"u8, g, e);
            }
        }
        {
            @string g = (~res).Header.Get(otherHeaderˢ);
            @string e = testˢ2; if (g != e) {
                Ꮡt.Errorf("got other-header = %q, want %q"u8, g, e);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end http_test_package
