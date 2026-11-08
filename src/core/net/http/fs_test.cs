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

internal static ref readonly @string testFile => ref testFileᵇ;
private static readonly @string testFileᵇ = "testdata/file"u8;
internal static UntypedInt testFileLen => 11;

partial struct wantRange {
    internal int64 start, end; // range [start,end)
}

partial struct ServeFileRangeTestsᴛ1 /*dyn*/ {
    internal @string r;
    internal nint code;
    internal slice<wantRange> ranges;
}
public static readonly ж<slice<ServeFileRangeTestsᴛ1>> ᏑServeFileRangeTests = new StaticBox<slice<ServeFileRangeTestsᴛ1>>([
    new(r: "", code: StatusOK),
    new(r: "bytes=0-4", code: StatusPartialContent, ranges: [new(0, 5)]),
    new(r: "bytes=2-", code: StatusPartialContent, ranges: [new(2, testFileLen)]),
    new(r: "bytes=-5", code: StatusPartialContent, ranges: [new(testFileLen - 5, testFileLen)]),
    new(r: "bytes=3-7", code: StatusPartialContent, ranges: [new(3, 8)]),
    new(r: "bytes=0-0,-2", code: StatusPartialContent, ranges: [new(0, 1), new(testFileLen - 2, testFileLen)]),
    new(r: "bytes=0-1,5-8", code: StatusPartialContent, ranges: [new(0, 2), new(5, 9)]),
    new(r: "bytes=0-1,5-", code: StatusPartialContent, ranges: [new(0, 2), new(5, testFileLen)]),
    new(r: "bytes=5-1000", code: StatusPartialContent, ranges: [new(5, testFileLen)]),
    new(r: "bytes=0-,1-,2-,3-,4-", code: StatusOK), // ignore wasteful range request
    new(r: "bytes=0-9", code: StatusPartialContent, ranges: [new(0, testFileLen - 1)]),
    new(r: "bytes=0-10", code: StatusPartialContent, ranges: [new(0, testFileLen)]),
    new(r: "bytes=0-11", code: StatusPartialContent, ranges: [new(0, testFileLen)]),
    new(r: "bytes=10-11", code: StatusPartialContent, ranges: [new(testFileLen - 1, testFileLen)]),
    new(r: "bytes=10-", code: StatusPartialContent, ranges: [new(testFileLen - 1, testFileLen)]),
    new(r: "bytes=11-", code: StatusRequestedRangeNotSatisfiable),
    new(r: "bytes=11-12", code: StatusRequestedRangeNotSatisfiable),
    new(r: "bytes=12-12", code: StatusRequestedRangeNotSatisfiable),
    new(r: "bytes=11-100", code: StatusRequestedRangeNotSatisfiable),
    new(r: "bytes=12-100", code: StatusRequestedRangeNotSatisfiable),
    new(r: "bytes=100-", code: StatusRequestedRangeNotSatisfiable),
    new(r: "bytes=100-1000", code: StatusRequestedRangeNotSatisfiable)
]);
public static ref slice<ServeFileRangeTestsᴛ1> ServeFileRangeTests => ref ((StaticBox<slice<ServeFileRangeTestsᴛ1>>)ᏑServeFileRangeTests).ValueSlot;

public static void TestServeFile(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testServeFile(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataFileˢ => ref testdataFileˢᵇ;
private static readonly @string testdataFileˢᵇ = "testdata/file"u8;
internal static readonly object readingFileˢ = (@string)"reading file:"u8;
internal static readonly object parseURLˢ = (@string)"ParseURL:"u8;
internal static ref readonly @string rangeˢ => ref rangeˢᵇ;
private static readonly @string rangeˢᵇ = "Range"u8;
internal static ref readonly @string contentRangeˢ => ref contentRangeˢᵇ;
private static readonly @string contentRangeˢᵇ = "Content-Range"u8;
internal static ref readonly @string multipartByterangesˢ => ref multipartByterangesˢᵇ;
private static readonly @string multipartByterangesˢᵇ = "multipart/byteranges"u8;
internal static ref readonly @string boundaryˢ => ref boundaryˢᵇ;
private static readonly @string boundaryˢᵇ = "boundary"u8;

internal static void testServeFile(ж<testing.T> t, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        ServeFile(w, r, testdataFileˢ);
    }))).ts;
    var c = ts.Client();

    error err = default;

    (var @file, err) = os.ReadFile(testFile);
    if (err != nil) {
        t.Fatal(readingFileˢ, err);
    }

    // set up the Request (re-used for all tests)
    Δhttp.Request req = default;
    req.Header = new httpꓸHeader(0);
    (req.URL, err) = url.Parse(ts.URL); if (err != nil) {
        t.Fatal(parseURLˢ, err);
    }

    // Get contents via various methods.
    //
    // See https://go.dev/issue/59471 for a proposal to limit the set of methods handled.
    // For now, test the historical behavior.
    foreach (var (_, method) in range(slice<@string>([
        MethodGet,
        MethodPost,
        MethodPut,
        MethodPatch,
        MethodDelete,
        MethodOptions,
        MethodTrace
    ]))) {
        req.Method = method;
        var (_, bodyΔ1) = getBody(t, method, req, c);
        if (!bytes.Equal(bodyΔ1, @file)) {
            t.Fatalf("body mismatch for %v request: got %q, want %q", method, bodyΔ1, @file);
        }
    }

    // HEAD request.
    req.Method = MethodHead;
    var (resp, body) = getBody(t, headˢ, req, c);
    if (len(body) != 0) {
        t.Fatalf("body mismatch for HEAD request: got %q, want empty", body);
    }
    @string got = resp.Header.Get(contentLengthˢ);
    @string want = fmt.Sprint(len(@file)); if (got != want) {
        t.Fatalf("Content-Length mismatch for HEAD request: got %v, want %v", got, want);
    }

    // Range tests
    req.Method = MethodGet;
Cases:
    foreach (var (_, rt) in range(ServeFileRangeTests)) {
        if (rt.r != ""u8) {
            req.Header.Set(rangeˢ, rt.r);
        }
        var (respΔ1, bodyΔ2) = getBody(t, fmt.Sprintf("range test %q"u8, rt.r), req, c);
        if (respΔ1.StatusCode != rt.code) {
            t.Errorf("range=%q: StatusCode=%d, want %d", rt.r, respΔ1.StatusCode, rt.code);
        }
        if (rt.code == StatusRequestedRangeNotSatisfiable) {
            continue;
        }
        @string wantContentRange = ""u8;
        if (len(rt.ranges) == 1) {
            var rng = rt.ranges[0];
            wantContentRange = fmt.Sprintf("bytes %d-%d/%d"u8, rng.start, rng.end - 1, (nint)(testFileLen));
        }
        @string cr = respΔ1.Header.Get(contentRangeˢ);
        if (cr != wantContentRange) {
            t.Errorf("range=%q: Content-Range = %q, want %q", rt.r, cr, wantContentRange);
        }
        @string ct = respΔ1.Header.Get(contentTypeˢ);
        if (len(rt.ranges) == 1) {
            var rng = rt.ranges[0];
            var wantBody = @file.slice((nint)(rng.start), (nint)(rng.end));
            if (!bytes.Equal(bodyΔ2, wantBody)) {
                t.Errorf("range=%q: body = %q, want %q", rt.r, bodyΔ2, wantBody);
            }
            if (strings.HasPrefix(ct, multipartByterangesˢ)) {
                t.Errorf("range=%q content-type = %q; unexpected multipart/byteranges", rt.r, ct);
            }
        }
        if (len(rt.ranges) > 1) {
            var (typ, @params, errΔ1) = mime.ParseMediaType(ct);
            if (errΔ1 != nil) {
                t.Errorf("range=%q content-type = %q; %v", rt.r, ct, errΔ1);
                continue;
            }
            if (typ != "multipart/byteranges"u8) {
                t.Errorf("range=%q content-type = %q; want multipart/byteranges", rt.r, typ);
                continue;
            }
            if (@params[boundaryˢ] == "") {
                t.Errorf("range=%q content-type = %q; lacks boundary", rt.r, ct);
                continue;
            }
            {
                var (g, w) = (respΔ1.ContentLength, (int64)len(bodyΔ2)); if (g != w) {
                    t.Errorf("range=%q Content-Length = %d; want %d", rt.r, g, w);
                    continue;
                }
            }
            var mr = multipart.NewReader(new http_test_package.bytes_ReaderжReader(bytes.NewReader(bodyΔ2)), @params[boundaryˢ]);
            foreach (var (ri, rng) in range(rt.ranges)) {
                var (part, errΔ2) = mr.NextPart();
                if (errΔ2 != nil) {
                    t.Errorf("range=%q, reading part index %d: %v", rt.r, ri, errΔ2);
                    goto continue_Cases;
                }
                wantContentRange = fmt.Sprintf("bytes %d-%d/%d"u8, rng.start, rng.end - 1, (nint)(testFileLen));
                @string g = part.Header.Get(contentRangeˢ);
                @string w = wantContentRange; if (g != w) {
                    t.Errorf("range=%q: part Content-Range = %q; want %q", rt.r, g, w);
                }
                (var bodyΔ3, errΔ2) = io.ReadAll(new multipart.PartжReader(part));
                if (errΔ2 != nil) {
                    t.Errorf("range=%q, reading part index %d body: %v", rt.r, ri, errΔ2);
                    goto continue_Cases;
                }
                var wantBody = @file.slice((nint)(rng.start), (nint)(rng.end));
                if (!bytes.Equal(bodyΔ3, wantBody)) {
                    t.Errorf("range=%q: body = %q, want %q", rt.r, bodyΔ3, wantBody);
                }
            }
            (_, errΔ1) = mr.NextPart();
            if (!AreEqual(errΔ1, io.EOF)) {
                t.Errorf("range=%q; expected final error io.EOF; got %v", rt.r, errΔ1);
            }
        }
continue_Cases:;
    }
break_Cases:;
}

internal partial struct TestServeFile_DotDot_tests /*dyn*/ {
    internal @string req;
    internal nint wantStatus;
}

public static void TestServeFile_DotDot(ж<testing.T> t) {
    var tests = slice<TestServeFile_DotDot_tests>([
        new("/testdata/file", 200),
        new("/../file", 400),
        new("/..", 400),
        new("/../", 400),
        new("/../foo", 400),
        new("/..\\foo", 400),
        new("/file/a", 200),
        new("/file/a..", 200),
        new("/file/a/..", 400),
        new("/file/a\\..", 400)
    ]);
    foreach (var (_, tt) in range(tests)) {
        var (req, err) = ReadRequest(bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader("GET "u8 + tt.req + " HTTP/1.1\r\nHost: foo\r\n\r\n"u8))));
        if (err != nil) {
            t.Errorf("bad request %q: %v", tt.req, err);
            continue;
        }
        var rec = httptest.NewRecorder();
        ServeFile(new http_test_package.httptest_ResponseRecorderжResponseWriter(rec), req, testdataFileˢ);
        if (rec.Code != tt.wantStatus) {
            t.Errorf("for request %q, status = %d; want %d", tt.req, rec.Code, tt.wantStatus);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataˢ => ref testdataˢᵇ;
private static readonly @string testdataˢᵇ = "testdata"u8;

// Tests that this doesn't panic. (Issue 30165)
public static void TestServeFileDirPanicEmptyPath(ж<testing.T> t) {
    var rec = httptest.NewRecorder();
    var req = httptest.NewRequest(getˢ2, "/", default);
    req.URL.Path = ""u8;
    ServeFile(new http_test_package.httptest_ResponseRecorderжResponseWriter(rec), req, testdataˢ);
    var res = rec.Result();
    if (res.StatusCode != 301) {
        t.Errorf("code = %v; want 301", res.Status);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nothingˢ => ref nothingˢᵇ;
private static readonly @string nothingˢᵇ = "nothing"u8;

// Tests that ranges are ignored with serving empty content. (Issue 54794)
public static void TestServeContentWithEmptyContentIgnoreRanges(ж<testing.T> t) {
    foreach (var (_, r) in range(slice<@string>([
        "bytes=0-128",
        "bytes=1-"
    ]))) {
        var rec = httptest.NewRecorder();
        var req = httptest.NewRequest(getˢ2, "/", default);
        req.Header.Set(rangeˢ, r);
        ServeContent(new http_test_package.httptest_ResponseRecorderжResponseWriter(rec), req, nothingˢ, time.Now(), new http_test_package.bytes_ReaderжReadSeeker(bytes.NewReader(default)));
        var res = rec.Result();
        if (res.StatusCode != 200) {
            t.Errorf("code = %v; want 200", res.Status);
        }
        nint bodyLen = rec.Body.Len();
        if (bodyLen != 0) {
            t.Errorf("body.Len() = %v; want 0", res.Status);
        }
    }
}

partial struct fsRedirectTestDataᴛ1 /*dyn*/ {
    internal @string original, redirect;
}
internal static slice<fsRedirectTestDataᴛ1> fsRedirectTestData = [
    new("/test/index.html", "/test/"),
    new("/test/testdata", "/test/testdata/"),
    new("/test/testdata/file/", "/test/testdata/file")
];

public static void TestFSRedirect(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testFSRedirect(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testˢ => ref testˢᵇ;
private static readonly @string testˢᵇ = "/test"u8;

internal static void testFSRedirect(ж<testing.T> t, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, StripPrefix(testˢ, FileServer((Δhttp.Dir)(@string)"."))).ts;

    foreach (var (_, data) in range(fsRedirectTestData)) {
        var (res, err) = ts.Client().Get(ts.URL + data.original);
        if (err != nil) {
            t.Fatal(err);
        }
        res.Body.Close();
        @string g = res.Request.URL.Path;
        @string e = data.redirect; if (g != e) {
            t.Errorf("redirect from %s: got %s, want %s", data.original, g, e);
        }
    }
}

partial struct testFileSystem {
    internal Func<@string, (Δhttp.File, error)> open;
}

internal static (Δhttp.File, error) Open(this ref testFileSystem fs, @string name) {
    return fs.open(name);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fileDoesNotExistˢ => ref fileDoesNotExistˢᵇ;
private static readonly @string fileDoesNotExistˢᵇ = "file does not exist"u8;
internal static ref readonly @string httpExampleComˢ => ref httpExampleComˢᵇ;
private static readonly @string httpExampleComˢᵇ = "http://example.com"u8;

internal partial struct TestFileServerCleans_tests /*dyn*/ {
    internal @string reqPath, openArg;
}

public static void TestFileServerCleans(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(afterTest, new http_test_package.testing_TжTB(t), ref ᒐ);
        var ch = new channel<@string>(1);
        var chʗ1 = ch;
        var fs = FileServer(new http_test_package.testFileSystemжFileSystem(Ꮡ(new testFileSystem((@string name) => {
            chʗ1.ᐸꟷ(name);
            return (default, errors.New(fileDoesNotExistˢ));
        }
        ))));
        var tests = slice<TestFileServerCleans_tests>([
            new("/foo.txt", "/foo.txt"),
            new("//foo.txt", "/foo.txt"),
            new("/../foo.txt", "/foo.txt")
        ]);
        var (req, _) = NewRequest(getˢ2, httpExampleComˢ, default);
        foreach (var (n, test) in range(tests)) {
            var rec = httptest.NewRecorder();
            req.URL.Path = test.reqPath;
            fs.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(rec), req);
            @string got = ᐸꟷ(ch); if (got != test.openArg) {
                t.Errorf("test %d: got %q, want %q", n, got, test.openArg);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestFileServerEscapesNames(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testFileServerEscapesNames(Δp0, Δp1)); }
// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string dirListPrefixᶜ => ref dirListPrefixᶜᵇ;
private static readonly @string dirListPrefixᶜᵇ = "<!doctype html>\n<meta name=\"viewport\" content=\"width=device-width\">\n<pre>\n"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string dirListSuffixᶜ => ref dirListSuffixᶜᵇ;
private static readonly @string dirListSuffixᶜᵇ = "\n</pre>\n"u8;

internal partial struct testFileServerEscapesNames_tests /*dyn*/ {
    internal @string name, escaped;
}

internal static void testFileServerEscapesNames(ж<testing.T> t, testMode mode) {
    @string dirListPrefix = dirListPrefixᶜ;
    @string dirListSuffix = dirListSuffixᶜ;
    var tests = slice<testFileServerEscapesNames_tests>([
        new(@"simple_name", @"<a href=""simple_name"">simple_name</a>"),
        new(@"""'<>&", @"<a href=""%22%27%3C%3E&"">&#34;&#39;&lt;&gt;&amp;</a>"),
        new(@"?foo=bar#baz", @"<a href=""%3Ffoo=bar%23baz"">?foo=bar#baz</a>"),
        new(@"<combo>?foo", @"<a href=""%3Ccombo%3E%3Ffoo"">&lt;combo&gt;?foo</a>"),
        new(@"foo:bar", @"<a href=""./foo:bar"">foo:bar</a>")
    ]);

    // We put each test file in its own directory in the fakeFS so we can look at it in isolation.
    ref var fs = ref heap<fakeFS>(new fakeFS(0), out var Ꮡfs);
    foreach (var (i, test) in range(tests)) {
        var testFile = Ꮡ(new fakeFileInfo(basename: test.name));
        fs[fmt.Sprintf("/%d"u8, i)] = Ꮡ(new fakeFileInfo(
            dir: true,
            modtime: time.Unix(1000000000, 0).UTC(),
            ents: [testFile]
        ));
        fs[fmt.Sprintf("/%d/%s"u8, i, test.name)] = testFile;
    }

    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, FileServer(new http_test_package.fakeFSжFileSystem(Ꮡfs))).ts;
    foreach (var (i, test) in range(tests)) {
        @string url = fmt.Sprintf("%s/%d"u8, ts.URL, i);
        var (res, err) = ts.Client().Get(url);
        if (err != nil) {
            t.Fatalf("test %q: Get: %v", test.name, err);
        }
        (var b, err) = io.ReadAll(res.Body);
        if (err != nil) {
            t.Fatalf("test %q: read Body: %v", test.name, err);
        }
        @string s = (@string)b;
        if (!strings.HasPrefix(s, dirListPrefix) || !strings.HasSuffix(s, dirListSuffix)) {
            t.Errorf("test %q: listing dir, full output is %q, want prefix %q and suffix %q", test.name, s, dirListPrefix, dirListSuffix);
        }
        @string trimmed = strings.TrimSuffix(strings.TrimPrefix(s, dirListPrefix), dirListSuffix); if (trimmed != test.escaped) {
            t.Errorf("test %q: listing dir, filename escaped to %q, want %q", test.name, trimmed, test.escaped);
        }
        res.Body.Close();
    }
}

public static void TestFileServerSortsNames(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testFileServerSortsNames(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aHrefAAAAHrefBBAˢ => ref aHrefAAAAHrefBBAˢᵇ;
private static readonly @string aHrefAAAAHrefBBAˢᵇ = "<a href=\"a\">a</a>\n<a href=\"b\">b</a>"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string contentsᶜ => ref contentsᶜᵇ;
private static readonly @string contentsᶜᵇ = "I am a fake file"u8;

internal static void testFileServerSortsNames(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        @string contents = contentsᶜ;
        ref var dirMod = ref heap<time.Time>(time.Unix(123, 0).UTC(), out var ᏑdirMod);
        var fileMod = time.Unix(1000000000, 0).UTC();
        ref var fs = ref heap<fakeFS>(new fakeFS(new map<@string, ж<fakeFileInfo>>{
            ["/"] = Ꮡ(new fakeFileInfo(
                dir: true,
                modtime: dirMod,
                ents: [
                    Ꮡ(new fakeFileInfo(
                        basename: "b",
                        modtime: fileMod,
                        contents: contents)),
                    Ꮡ(new fakeFileInfo(
                        basename: "a",
                        modtime: fileMod,
                        contents: contents))
                ]
            ))
        }), out var Ꮡfs);

        var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, FileServer(new http_test_package.fakeFSжFileSystem(Ꮡfs))).ts;

        var (res, err) = ts.Client().Get(ts.URL);
        if (err != nil) {
            t.Fatalf("Get: %v", err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);

        (var b, err) = io.ReadAll(res.Body);
        if (err != nil) {
            t.Fatalf("read Body: %v", err);
        }
        @string s = (@string)b;
        if (!strings.Contains(s, aHrefAAAAHrefBBAˢ)) {
            t.Errorf("output appears to be unsorted:\n%s", s);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void mustRemoveAll(@string dir) {
    var err = os.RemoveAll(dir);
    if (err != nil) {
        throw panic(err);
    }
}

public static void TestFileServerImplicitLeadingSlash(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testFileServerImplicitLeadingSlash(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooTxtˢ => ref fooTxtˢᵇ;
private static readonly @string fooTxtˢᵇ = "foo.txt"u8;
internal static ref readonly @string barˢ3 => ref barˢ3ᵇ;
private static readonly @string barˢ3ᵇ = "/bar/"u8;
internal static ref readonly @string fooTxtˢ2 => ref fooTxtˢ2ᵇ;
private static readonly @string fooTxtˢ2ᵇ = ">foo.txt<"u8;
internal static ref readonly @string barFooTxtˢ => ref barFooTxtˢᵇ;
private static readonly @string barFooTxtˢᵇ = "/bar/foo.txt"u8;
internal static readonly object helloWorldˢ3 = (@string)"Hello world"u8;

internal static void testFileServerImplicitLeadingSlash(ж<testing.T> t, testMode mode) {
    @string tempDir = t.TempDir();
    {
        var err = os.WriteFile(filepath.Join(tempDir, fooTxtˢ), slice<byte>("Hello world"u8), 420); if (err != nil) {
            t.Fatalf("WriteFile: %v", err);
        }
    }
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, StripPrefix(barˢ3, FileServer((Δhttp.Dir)tempDir))).ts;
    var tsʗ1 = ts;
    @string get(@string suffix) {
        var (res, err) = tsʗ1.Client().Get(tsʗ1.URL + suffix);
        if (err != nil) {
            t.Fatalf("Get %s: %v", suffix, err);
        }
        (var b, err) = io.ReadAll(res.Body);
        if (err != nil) {
            t.Fatalf("ReadAll %s: %v", suffix, err);
        }
        res.Body.Close();
        return (@string)b;
    }
    {
        @string s = get(barˢ3); if (!strings.Contains(s, fooTxtˢ2)) {
            t.Logf("expected a directory listing with foo.txt, got %q", s);
        }
    }
    {
        @string s = get(barFooTxtˢ); if (s != "Hello world"u8) {
            t.Logf("expected %q, got %q", helloWorldˢ3, s);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestOnWindowsˢ = (@string)"skipping test on windows"u8;
internal static ref readonly @string etcHostsˢ => ref etcHostsˢᵇ;
private static readonly @string etcHostsˢᵇ = "/etc/hosts"u8;
internal static readonly object skippingTestNoEtcHostsˢ = (@string)"skipping test; no /etc/hosts file"u8;
internal static ref readonly @string etcˢ => ref etcˢᵇ;
private static readonly @string etcˢᵇ = "/etc/"u8;
internal static ref readonly @string hostsˢ => ref hostsˢᵇ;
private static readonly @string hostsˢᵇ = "/hosts"u8;
internal static ref readonly @string hostsˢ2 => ref hostsˢ2ᵇ;
private static readonly @string hostsˢ2ᵇ = "hosts"u8;
internal static ref readonly @string hostsˢ3 => ref hostsˢ3ᵇ;
private static readonly @string hostsˢ3ᵇ = "../../../../hosts"u8;
internal static ref readonly @string etcˢ2 => ref etcˢ2ᵇ;
private static readonly @string etcˢ2ᵇ = "/etc"u8;

public static void TestDirJoin(ж<testing.T> t) {
    if (runtime.GOOS == "windows"u8) {
        t.Skip(skippingTestOnWindowsˢ);
    }
    var (wfi, err) = os.Stat(etcHostsˢ);
    if (err != nil) {
        t.Skip(skippingTestNoEtcHostsˢ);
    }
    var wfiʗ1 = wfi;
    void test(Δhttp.Dir d, @string name) {
        GoFrame ᒐ = default;
        try {
            var (f, errΔ1) = d.Open(name);
            if (errΔ1 != nil) {
                t.Fatalf("open of %s: %v", name, errΔ1);
            }
            var fʗ1 = f;
            defer(() => fʗ1.Close(), ref ᒐ);
            (var gfi, errΔ1) = f.Stat();
            if (errΔ1 != nil) {
                t.Fatalf("stat of %s: %v", name, errΔ1);
            }
            if (!os.SameFile(gfi, wfiʗ1)) {
                t.Errorf("%s got different file", name);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    test((Δhttp.Dir)(@string)etcˢ, hostsˢ);
    test((Δhttp.Dir)(@string)etcˢ, hostsˢ2);
    test((Δhttp.Dir)(@string)etcˢ, hostsˢ3);
    test((Δhttp.Dir)(@string)etcˢ2, hostsˢ);
    test((Δhttp.Dir)(@string)etcˢ2, hostsˢ2);
    test((Δhttp.Dir)(@string)etcˢ2, hostsˢ3);

    // Not really directories, but since we use this trick in
    // ServeFile, test it:
    test((Δhttp.Dir)(@string)etcHostsˢ, ""u8);
    test((Δhttp.Dir)(@string)etcHostsˢ, "/");
    test((Δhttp.Dir)(@string)etcHostsˢ, "../");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fsTestGoˢ => ref fsTestGoˢᵇ;
private static readonly @string fsTestGoˢᵇ = "fs_test.go"u8;

public static void TestEmptyDirOpenCWD(ж<testing.T> t) {
    void test(Δhttp.Dir d) {
        GoFrame ᒐ = default;
        try {
            @string name = fsTestGoˢ;
            var (f, err) = d.Open(name);
            if (err != nil) {
                t.Fatalf("open of %s: %v", name, err);
            }
            var fʗ1 = f;
            defer(() => fʗ1.Close(), ref ᒐ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    test((Δhttp.Dir)(@string)""u8);
    test((Δhttp.Dir)(@string)".");
    test((Δhttp.Dir)(@string)"./");
}

public static void TestServeFileContentType(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testServeFileContentType(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string overrideˢ => ref overrideˢᵇ;
private static readonly @string overrideˢᵇ = "override"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string ctypeᶜ => ref ctypeᶜᵇ;
private static readonly @string ctypeᶜᵇ = "icecream/chocolate"u8;

internal static void testServeFileContentType(ж<testing.T> t, testMode mode) {
    @string ctype = ctypeᶜ;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var exprᴛ1 = r.FormValue(overrideˢ);
        switch (ᐧ) {
        case {} when exprᴛ1 == "1"u8: {
            w.Header().Set(contentTypeˢ, ctype);
            break;
        }
        case {} when exprᴛ1 == "2"u8: {
            // Explicitly inhibit sniffing.
            w.Header()[contentTypeˢ] = [];
            break;
        }}
        ServeFile(w, r, testdataFileˢ);
    }))).ts;
    var tsʗ1 = ts;
    void get(@string @override, slice<@string> want) {
        var (resp, err) = tsʗ1.Client().Get(tsʗ1.URL + "?override="u8 + @override);
        if (err != nil) {
            t.Fatal(err);
        }
        var h = resp.Header[contentTypeˢ]; if (!slices.Equal<slice<@string>, @string>(h, want)) {
            t.Errorf("Content-Type mismatch: got %v, want %v", h, want);
        }
        resp.Body.Close();
    }
    get("0", ["text/plain; charset=utf-8"]);
    get("1", [ctype]);
    get("2", default);
}

public static void TestServeFileMimeType(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testServeFileMimeType(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataStyleCssˢ => ref testdataStyleCssˢᵇ;
private static readonly @string testdataStyleCssˢᵇ = "testdata/style.css"u8;
internal static ref readonly @string textCssCharsetUtf8ˢ => ref textCssCharsetUtf8ˢᵇ;
private static readonly @string textCssCharsetUtf8ˢᵇ = "text/css; charset=utf-8"u8;

internal static void testServeFileMimeType(ж<testing.T> t, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        ServeFile(w, r, testdataStyleCssˢ);
    }))).ts;
    var (resp, err) = ts.Client().Get(ts.URL);
    if (err != nil) {
        t.Fatal(err);
    }
    resp.Body.Close();
    @string want = textCssCharsetUtf8ˢ;
    @string h = resp.Header.Get(contentTypeˢ); if (h != want) {
        t.Errorf("Content-Type mismatch: got %q, want %q", h, want);
    }
}

public static void TestServeFileFromCWD(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testServeFileFromCWD(Δp0, Δp1)); }
internal static void testServeFileFromCWD(ж<testing.T> t, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> rΔ1) => {
        ServeFile(w, rΔ1, fsTestGoˢ);
    }))).ts;
    var (r, err) = ts.Client().Get(ts.URL);
    if (err != nil) {
        t.Fatal(err);
    }
    r.Body.Close();
    if (r.StatusCode != 200) {
        t.Fatalf("expected 200 OK, got %s", r.Status);
    }
}

// Issue 13996
public static void TestServeDirWithoutTrailingSlash(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testServeDirWithoutTrailingSlash(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataˢ2 => ref testdataˢ2ᵇ;
private static readonly @string testdataˢ2ᵇ = "/testdata/"u8;

internal static void testServeDirWithoutTrailingSlash(ж<testing.T> t, testMode mode) {
    @string e = testdataˢ2;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> rΔ1) => {
        ServeFile(w, rΔ1, ".");
    }))).ts;
    var (r, err) = ts.Client().Get(ts.URL + "/testdata"u8);
    if (err != nil) {
        t.Fatal(err);
    }
    r.Body.Close();
    @string g = r.Request.URL.Path; if (g != e) {
        t.Errorf("got %s, want %s", g, e);
    }
}

// Tests that ServeFile doesn't add a Content-Length if a Content-Encoding is
// specified.
public static void TestServeFileWithContentEncoding(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testServeFileWithContentEncoding(Δp0, Δp1)); }
internal static void testServeFileWithContentEncoding(ж<testing.T> t, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
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
    var (resp, err) = cst.c.Get(cst.ts.URL);
    if (err != nil) {
        t.Fatal(err);
    }
    resp.Body.Close();
    var (g, e) = (resp.ContentLength, (int64)(-1)); if (g != e) {
        t.Errorf("Content-Length mismatch: got %d, want %d", g, e);
    }
}

// Tests that ServeFile does not generate representation metadata when
// file has not been modified, as per RFC 7232 section 4.1.
public static void TestServeFileNotModified(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testServeFileNotModified(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string etagˢ => ref etagˢᵇ;
private static readonly @string etagˢᵇ = "Etag"u8;
internal static ref readonly @string ifNoneMatchˢ => ref ifNoneMatchˢᵇ;
private static readonly @string ifNoneMatchˢᵇ = "If-None-Match"u8;
internal static readonly object readingBodyˢ = (@string)"reading Body:"u8;

internal static void testServeFileNotModified(ж<testing.T> t, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(contentTypeˢ, applicationJsonˢ);
        w.Header().Set(contentEncodingˢ, fooˢ);
        w.Header().Set(etagˢ, @"""123""");
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
    var (req, err) = NewRequest(getˢ2, cst.ts.URL, default);
    if (err != nil) {
        t.Fatal(err);
    }
    req.Header.Set(ifNoneMatchˢ, @"""123""");
    (var resp, err) = cst.c.Do(req);
    if (err != nil) {
        t.Fatal(err);
    }
    (var b, err) = io.ReadAll(resp.Body);
    resp.Body.Close();
    if (err != nil) {
        t.Fatal(readingBodyˢ, err);
    }
    if (len(b) != 0) {
        t.Errorf("non-empty body");
    }
    {
        nint g = resp.StatusCode;
        nint e = StatusNotModified; if (g != e) {
            t.Errorf("status mismatch: got %d, want %d", g, e);
        }
    }
    // HTTP1 transport sets ContentLength to 0.
    {
        var (g, e1, e2) = (resp.ContentLength, (int64)(-1), (int64)0); if (g != e1 && g != e2) {
            t.Errorf("Content-Length mismatch: got %d, want %d or %d", g, e1, e2);
        }
    }
    if (resp.Header.Get(contentTypeˢ) != ""u8) {
        t.Errorf("Content-Type present, but it should not be");
    }
    if (resp.Header.Get(contentEncodingˢ) != ""u8) {
        t.Errorf("Content-Encoding present, but it should not be");
    }
}

public static void TestServeIndexHtml(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testServeIndexHtml(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string dirˢ => ref dirˢᵇ;
private static readonly @string dirˢᵇ = "Dir"u8;
internal static ref readonly @string dirFSˢ => ref dirFSˢᵇ;
private static readonly @string dirFSˢᵇ = "DirFS"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ => ref wantᶜᵇ;
private static readonly @string wantᶜᵇ = "index.html says hello\n"u8;

internal static void testServeIndexHtml(ж<testing.T> t, testMode mode) {
    for (nint i = 0; i < 2; i++) {
        ref var h = ref heap<httpꓸHandler>(out var Ꮡh);
        @string name = default;
        switch (i) {
        case 0: {
            h = FileServer((Δhttp.Dir)(@string)".");
            name = dirˢ;
            break;
        }
        case 1: {
            h = FileServer(FS(os.DirFS(".")));
            name = dirFSˢ;
            break;
        }}
        t.Run(name, (ж<testing.T> tΔ1) => {
            @string want = wantᶜ;
            var ts = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), mode, Ꮡh.ValueSlot).ts;

            foreach (var (_, path) in range(slice<@string>(["/testdata/", "/testdata/index.html"]))) {
                var (res, err) = ts.Client().Get(ts.URL + path);
                if (err != nil) {
                    tΔ1.Fatal(err);
                }
                (var b, err) = io.ReadAll(res.Body);
                if (err != nil) {
                    tΔ1.Fatal(readingBodyˢ, err);
                }
                @string s = (@string)b; if (s != want) {
                    tΔ1.Errorf("for path %q got %q, want %q", path, s, want);
                }
                res.Body.Close();
            }
        });
    }
}

public static void TestServeIndexHtmlFS(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testServeIndexHtmlFS(Δp0, Δp1)); }
// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ1 => ref wantᶜ1ᵇ;
private static readonly @string wantᶜ1ᵇ = "index.html says hello\n"u8;

internal static void testServeIndexHtmlFS(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        @string want = wantᶜ1;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, FileServer((Δhttp.Dir)(@string)".")).ts;
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);

        foreach (var (_, path) in range(slice<@string>(["/testdata/", "/testdata/index.html"]))) {
            var (res, err) = ts.Client().Get(ts.URL + path);
            if (err != nil) {
                t.Fatal(err);
            }
            (var b, err) = io.ReadAll(res.Body);
            if (err != nil) {
                t.Fatal(readingBodyˢ, err);
            }
            @string s = (@string)b; if (s != want) {
                t.Errorf("for path %q got %q, want %q", path, s, want);
            }
            res.Body.Close();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestFileServerZeroByte(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testFileServerZeroByte(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readResponseˢ = (@string)"ReadResponse: "u8;

internal static void testFileServerZeroByte(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, FileServer((Δhttp.Dir)(@string)".")).ts;

        var (c, err) = net.Dial(tcpˢ, ts.Listener.Addr().String());
        if (err != nil) {
            t.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        (_, err) = fmt.Fprintf(new http_test_package.net_ConnᴠWriter(c), "GET /..\x00 HTTP/1.0\r\n\r\n"u8);
        if (err != nil) {
            t.Fatal(err);
        }
        ref var got = ref heap(new bytes.Buffer(), out var Ꮡgot);
        var bufr = bufio.NewReader(io.TeeReader(new http_test_package.net_ConnᴠReader(c), new http_test_package.bytes_BufferжWriter(Ꮡgot)));
        (var res, err) = ReadResponse(bufr, nil);
        if (err != nil) {
            t.Fatal(readResponseˢ, err);
        }
        if (res.StatusCode == 200) {
            t.Errorf("got status 200; want an error. Body is:\n%s", got.Bytes());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestFileServerNullByte(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testFileServerNullByte(Δp0, Δp1)); }
internal static void testFileServerNullByte(ж<testing.T> t, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, FileServer((Δhttp.Dir)(@string)testdataˢ)).ts;

    foreach (var (_, path) in range(slice<@string>([
        "/file%00",
        "/%00",
        "/file/qwe/%00"
    ]))) {
        var (res, err) = ts.Client().Get(ts.URL + path);
        if (err != nil) {
            t.Fatal(err);
        }
        res.Body.Close();
        if (res.StatusCode != 404) {
            t.Errorf("Get(%q): got status %v, want 404", path, res.StatusCode);
        }
    }
}

public static void TestFileServerNamesEscape(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testFileServerNamesEscape(Δp0, Δp1)); }
internal static void testFileServerNamesEscape(ж<testing.T> t, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, FileServer((Δhttp.Dir)(@string)testdataˢ)).ts;
    foreach (var (_, path) in range(slice<@string>([
        "/../testdata/file",
        "/NUL" // don't read from device files on Windows
    ]))) {
        var (res, err) = ts.Client().Get(ts.URL + path);
        if (err != nil) {
            t.Fatal(err);
        }
        res.Body.Close();
        if (res.StatusCode < 400 || res.StatusCode > 599) {
            t.Errorf("Get(%q): got status %v, want 4xx or 5xx", path, res.StatusCode);
        }
    }
}

partial struct fakeFileInfo {
    internal bool dir;
    internal @string basename;
    internal time.Time modtime;
    internal slice<ж<fakeFileInfo>> ents;
    internal @string contents;
    internal error err;
}

internal static @string Name(this ref fakeFileInfo f) { return f.basename; }
internal static any Sys(this ref fakeFileInfo f) { return default; }
internal static time.Time ModTime(this ref fakeFileInfo f) { return f.modtime; }
internal static bool IsDir(this ref fakeFileInfo f) { return f.dir; }
internal static int64 Size(this ref fakeFileInfo f) { return (int64)len(f.contents); }
internal static fs.FileMode Mode(this ref fakeFileInfo f) {
    if (f.dir) {
        return (fs.FileMode)(493 | fs.ModeDir);
    }
    return 420;
}

internal static @string String(this ж<fakeFileInfo> Ꮡf) {
    return fs.FormatFileInfo(new http_test_package.fakeFileInfoжFileInfo(Ꮡf));
}

partial struct fakeFile {
    /*embed*/ public io_package.ReadSeeker ReadSeeker;
    internal ж<fakeFileInfo> fi;
    internal @string path; // as opened
    internal nint entpos;
}

internal static error Close(this ref fakeFile f) { return default; }
internal static (fs.FileInfo, error) Stat(this ref fakeFile f) { return (new http_test_package.fakeFileInfoжFileInfo(f.fi), default); }
internal static (slice<fs.FileInfo>, error) Readdir(this ref fakeFile f, nint count) {
    if (!f.fi.dir) {
        return (default, fs.ErrInvalid);
    }
    slice<fs.FileInfo> fis = default;

    nint limit = f.entpos + count;
    if (count <= 0 || limit > len(f.fi.ents)) {
        limit = len(f.fi.ents);
    }
    for (; f.entpos < limit; f.entpos++) {
        fis = append(fis, (fs.FileInfo)(new http_test_package.fakeFileInfoжFileInfo(f.fi.ents[f.entpos])));
    }

    if (len(fis) == 0 && count > 0) {
        return (fis, io.EOF);
    } else {
        return (fis, default);
    }
}

partial struct fakeFS /*map[@string, ж<fakeFileInfo>]*/;

internal static (Δhttp.File, error) Open(this fakeFS fsys, @string name) {
    name = path.Clean(name);
    var (f, ok) = fsys[name, ꟷ];
    if (!ok) {
        return (default, fs.ErrNotExist);
    }
    if (f.err != nil) {
        return (default, f.err);
    }
    return (new http_test_package.fakeFileжFile(Ꮡ(new fakeFile(ReadSeeker: new http_test_package.strings_ReaderжReadSeeker(strings.NewReader(f.contents)), fi: f, path: name))), default);
}

public static void TestDirectoryIfNotModified(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testDirectoryIfNotModified(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string ifModifiedSinceˢ => ref ifModifiedSinceˢᵇ;
private static readonly @string ifModifiedSinceˢᵇ = "If-Modified-Since"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string indexContentsᶜ => ref indexContentsᶜᵇ;
private static readonly @string indexContentsᶜᵇ = "I am a fake index.html file"u8;

internal static void testDirectoryIfNotModified(ж<testing.T> t, testMode mode) {
    @string indexContents = indexContentsᶜ;
    ref var fileMod = ref heap<time.Time>(time.Unix(1000000000, 0).UTC(), out var ᏑfileMod);
    @string fileModStr = fileMod.Format(TimeFormat);
    ref var dirMod = ref heap<time.Time>(time.Unix(123, 0).UTC(), out var ᏑdirMod);
    var indexFile = Ꮡ(new fakeFileInfo(
        basename: "index.html",
        modtime: fileMod,
        contents: indexContents
    ));
    var fs = new fakeFS(new map<@string, ж<fakeFileInfo>>{
        ["/"] = Ꮡ(new fakeFileInfo(
            dir: true,
            modtime: dirMod,
            ents: [indexFile]
        )),
        ["/index.html"] = indexFile
    });

    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, FileServer(fs)).ts;

    var (res, err) = ts.Client().Get(ts.URL);
    if (err != nil) {
        t.Fatal(err);
    }
    (var b, err) = io.ReadAll(res.Body);
    if (err != nil) {
        t.Fatal(err);
    }
    if (((sstring)b) != indexContents) {
        t.Fatalf("Got body %q; want %q", b, indexContents);
    }
    res.Body.Close();

    @string lastMod = res.Header.Get(lastModifiedˢ);
    if (lastMod != fileModStr) {
        t.Fatalf("initial Last-Modified = %q; want %q", lastMod, fileModStr);
    }

    var (req, _) = NewRequest(getˢ2, ts.URL, default);
    req.Header.Set(ifModifiedSinceˢ, lastMod);

    var c = ts.Client();
    (res, err) = c.Do(req);
    if (err != nil) {
        t.Fatal(err);
    }
    if (res.StatusCode != 304) {
        t.Fatalf("Code after If-Modified-Since request = %v; want 304", res.StatusCode);
    }
    res.Body.Close();

    // Advance the index.html file's modtime, but not the directory's.
    indexFile.modtime = indexFile.modtime.Add((time.Duration)(3600000000000L));

    (res, err) = c.Do(req);
    if (err != nil) {
        t.Fatal(err);
    }
    if (res.StatusCode != 200) {
        t.Fatalf("Code after second If-Modified-Since request = %v; want 200; res is %#v", res.StatusCode, res.OrTypedNil());
    }
    res.Body.Close();
}

internal static fs.FileInfo mustStat(ж<testing.T> t, @string fileName) {
    var (fi, err) = os.Stat(fileName);
    if (err != nil) {
        t.Fatal(err);
    }
    return fi;
}

public static void TestServeContent(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testServeContent(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string eTagˢ => ref eTagˢᵇ;
private static readonly @string eTagˢᵇ = "ETag"u8;
internal static ref readonly @string testdataIndexHtmlˢ => ref testdataIndexHtmlˢᵇ;
private static readonly @string testdataIndexHtmlˢᵇ = "testdata/index.html"u8;
internal static ref readonly @string htmlFooˢ => ref htmlFooˢᵇ;
private static readonly @string htmlFooˢᵇ = "<html>foo"u8;

internal partial struct testServeContent_serveParam /*dyn*/ {
    internal @string name;
    internal time.Time modtime;
    internal io.ReadSeeker content;
    internal @string contentType;
    internal @string etag;
}

internal partial struct testServeContent_testCase /*dyn*/ {
    // One of file or content must be set:
    internal @string @file;
    internal io.ReadSeeker content;
    internal time.Time modtime;
    internal @string serveETag;        // optional
    internal @string serveContentType; // optional
    internal map<@string, @string> reqHeader;
    internal @string wantLastMod;
    internal @string wantContentType;
    internal @string wantContentRange;
    internal nint wantStatus;
}

internal static void testServeContent(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var servec = new channel<testServeContent_serveParam>(1);
        var servecʗ1 = servec;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var p = ᐸꟷ(servecʗ1);
            if (p.etag != ""u8) {
                w.Header().Set(eTagˢ, p.etag);
            }
            if (p.contentType != ""u8) {
                w.Header().Set(contentTypeˢ, p.contentType);
            }
            ServeContent(w, r, p.name, p.modtime, p.content);
        }))).ts;
        var htmlModTime = mustStat(t, testdataIndexHtmlˢ).ModTime();
        var tests = new map<@string, testServeContent_testCase>{
            ["no_last_modified"] = new(
                @file: "testdata/style.css",
                wantContentType: "text/css; charset=utf-8",
                wantStatus: 200
            ),
            ["with_last_modified"] = new(
                @file: "testdata/index.html",
                wantContentType: "text/html; charset=utf-8",
                modtime: htmlModTime,
                wantLastMod: htmlModTime.UTC().Format(TimeFormat),
                wantStatus: 200
            ),
            ["not_modified_modtime"] = new(
                @file: "testdata/style.css",
                serveETag: @"""foo""", // Last-Modified sent only when no ETag
                modtime: htmlModTime,
                reqHeader: new map<@string, @string>{
                    ["If-Modified-Since"] = htmlModTime.UTC().Format(TimeFormat)
                },
                wantStatus: 304
            ),
            ["not_modified_modtime_with_contenttype"] = new(
                @file: "testdata/style.css",
                serveContentType: "text/css", // explicit content type
                serveETag: @"""foo""",        // Last-Modified sent only when no ETag
                modtime: htmlModTime,
                reqHeader: new map<@string, @string>{
                    ["If-Modified-Since"] = htmlModTime.UTC().Format(TimeFormat)
                },
                wantStatus: 304
            ),
            ["not_modified_etag"] = new(
                @file: "testdata/style.css",
                serveETag: @"""foo""",
                reqHeader: new map<@string, @string>{
                    ["If-None-Match"] = @"""foo"""
                },
                wantStatus: 304
            ),
            ["not_modified_etag_no_seek"] = new(
                content: new panicOnSeek((io.ReadSeeker)(default)), // should never be called
                serveETag: @"W/""foo""",                            // If-None-Match uses weak ETag comparison
                reqHeader: new map<@string, @string>{
                    ["If-None-Match"] = @"""baz"", W/""foo"""
                },
                wantStatus: 304
            ),
            ["if_none_match_mismatch"] = new(
                @file: "testdata/style.css",
                serveETag: @"""foo""",
                reqHeader: new map<@string, @string>{
                    ["If-None-Match"] = @"""Foo"""
                },
                wantStatus: 200,
                wantContentType: "text/css; charset=utf-8"
            ),
            ["if_none_match_malformed"] = new(
                @file: "testdata/style.css",
                serveETag: @"""foo""",
                reqHeader: new map<@string, @string>{
                    ["If-None-Match"] = @","
                },
                wantStatus: 200,
                wantContentType: "text/css; charset=utf-8"
            ),
            ["range_good"] = new(
                @file: "testdata/style.css",
                serveETag: @"""A""",
                reqHeader: new map<@string, @string>{
                    ["Range"] = "bytes=0-4"
                },
                wantStatus: StatusPartialContent,
                wantContentType: "text/css; charset=utf-8",
                wantContentRange: "bytes 0-4/8"
            ),
            ["range_match"] = new(
                @file: "testdata/style.css",
                serveETag: @"""A""",
                reqHeader: new map<@string, @string>{
                    ["Range"] = "bytes=0-4",
                    ["If-Range"] = @"""A"""
                },
                wantStatus: StatusPartialContent,
                wantContentType: "text/css; charset=utf-8",
                wantContentRange: "bytes 0-4/8"
            ),
            ["range_match_weak_etag"] = new(
                @file: "testdata/style.css",
                serveETag: @"W/""A""",
                reqHeader: new map<@string, @string>{
                    ["Range"] = "bytes=0-4",
                    ["If-Range"] = @"W/""A"""
                },
                wantStatus: 200,
                wantContentType: "text/css; charset=utf-8"
            ),
            ["range_no_overlap"] = new(
                @file: "testdata/style.css",
                serveETag: @"""A""",
                reqHeader: new map<@string, @string>{
                    ["Range"] = "bytes=10-20"
                },
                wantStatus: StatusRequestedRangeNotSatisfiable,
                wantContentType: "text/plain; charset=utf-8",
                wantContentRange: "bytes */8"
            ),
            // An If-Range resource for entity "A", but entity "B" is now current.
            // The Range request should be ignored.
            ["range_no_match"] = new(
                @file: "testdata/style.css",
                serveETag: @"""A""",
                reqHeader: new map<@string, @string>{
                    ["Range"] = "bytes=0-4",
                    ["If-Range"] = @"""B"""
                },
                wantStatus: 200,
                wantContentType: "text/css; charset=utf-8"
            ),
            ["range_with_modtime"] = new(
                @file: "testdata/style.css",
                modtime: time.Date(2014, 6, 25, 17, 12, 18, 0, /* nanos */ time.ΔUTC),
                reqHeader: new map<@string, @string>{
                    ["Range"] = "bytes=0-4",
                    ["If-Range"] = "Wed, 25 Jun 2014 17:12:18 GMT"
                },
                wantStatus: StatusPartialContent,
                wantContentType: "text/css; charset=utf-8",
                wantContentRange: "bytes 0-4/8",
                wantLastMod: "Wed, 25 Jun 2014 17:12:18 GMT"
            ),
            ["range_with_modtime_mismatch"] = new(
                @file: "testdata/style.css",
                modtime: time.Date(2014, 6, 25, 17, 12, 18, 0, /* nanos */ time.ΔUTC),
                reqHeader: new map<@string, @string>{
                    ["Range"] = "bytes=0-4",
                    ["If-Range"] = "Wed, 25 Jun 2014 17:12:19 GMT"
                },
                wantStatus: StatusOK,
                wantContentType: "text/css; charset=utf-8",
                wantLastMod: "Wed, 25 Jun 2014 17:12:18 GMT"
            ),
            ["range_with_modtime_nanos"] = new(
                @file: "testdata/style.css",
                modtime: time.Date(2014, 6, 25, 17, 12, 18, 123, /* nanos */ time.ΔUTC),
                reqHeader: new map<@string, @string>{
                    ["Range"] = "bytes=0-4",
                    ["If-Range"] = "Wed, 25 Jun 2014 17:12:18 GMT"
                },
                wantStatus: StatusPartialContent,
                wantContentType: "text/css; charset=utf-8",
                wantContentRange: "bytes 0-4/8",
                wantLastMod: "Wed, 25 Jun 2014 17:12:18 GMT"
            ),
            ["unix_zero_modtime"] = new(
                content: new http_test_package.strings_ReaderжReadSeeker(strings.NewReader(htmlFooˢ)),
                modtime: time.Unix(0, 0),
                wantStatus: StatusOK,
                wantContentType: "text/html; charset=utf-8"
            ),
            ["ifmatch_matches"] = new(
                @file: "testdata/style.css",
                serveETag: @"""A""",
                reqHeader: new map<@string, @string>{
                    ["If-Match"] = @"""Z"", ""A"""
                },
                wantStatus: 200,
                wantContentType: "text/css; charset=utf-8"
            ),
            ["ifmatch_star"] = new(
                @file: "testdata/style.css",
                serveETag: @"""A""",
                reqHeader: new map<@string, @string>{
                    ["If-Match"] = @"*"
                },
                wantStatus: 200,
                wantContentType: "text/css; charset=utf-8"
            ),
            ["ifmatch_failed"] = new(
                @file: "testdata/style.css",
                serveETag: @"""A""",
                reqHeader: new map<@string, @string>{
                    ["If-Match"] = @"""B"""
                },
                wantStatus: 412
            ),
            ["ifmatch_fails_on_weak_etag"] = new(
                @file: "testdata/style.css",
                serveETag: @"W/""A""",
                reqHeader: new map<@string, @string>{
                    ["If-Match"] = @"W/""A"""
                },
                wantStatus: 412
            ),
            ["if_unmodified_since_true"] = new(
                @file: "testdata/style.css",
                modtime: htmlModTime,
                reqHeader: new map<@string, @string>{
                    ["If-Unmodified-Since"] = htmlModTime.UTC().Format(TimeFormat)
                },
                wantStatus: 200,
                wantContentType: "text/css; charset=utf-8",
                wantLastMod: htmlModTime.UTC().Format(TimeFormat)
            ),
            ["if_unmodified_since_false"] = new(
                @file: "testdata/style.css",
                modtime: htmlModTime,
                reqHeader: new map<@string, @string>{
                    ["If-Unmodified-Since"] = htmlModTime.Add(-2 * time.ΔSecond).UTC().Format(TimeFormat)
                },
                wantStatus: 412,
                wantLastMod: htmlModTime.UTC().Format(TimeFormat)
            )
        };
        foreach (var (testName, tt) in tests) {
            io.ReadSeeker content = default;
            if (tt.@file != ""u8) {
                var (f, err) = os.Open(tt.@file);
                if (err != nil) {
                    t.Fatalf("test %q: %v", testName, err);
                }
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);
                content = new http_test_package.os_FileжReadSeeker(f);
            } else {
                content = tt.content;
            }
            foreach (var (_, method) in range(slice<@string>(["GET", "HEAD"]))) {
                //restore content in case it is consumed by previous method
                var (contentΔ1, ok) = content._<ж<strings.Reader>>(ᐧ); if (ok) {
                    contentΔ1.Seek(0, io.SeekStart);
                }

                servec.ᐸꟷ(new testServeContent_serveParam(
                    name: filepath.Base(tt.@file),
                    content: content,
                    modtime: tt.modtime,
                    etag: tt.serveETag,
                    contentType: tt.serveContentType
                ));
                var (req, err) = NewRequest(method, ts.URL, default);
                if (err != nil) {
                    t.Fatal(err);
                }
                foreach (var (k, v) in tt.reqHeader) {
                    req.Header.Set(k, v);
                }

                var c = ts.Client();
                (var res, err) = c.Do(req);
                if (err != nil) {
                    t.Fatal(err);
                }
                io.Copy(io.Discard, res.Body);
                res.Body.Close();
                if (res.StatusCode != tt.wantStatus) {
                    t.Errorf("test %q using %q: got status = %d; want %d", testName, method, res.StatusCode, tt.wantStatus);
                }
                {
                    @string g = res.Header.Get(contentTypeˢ);
                    @string e = tt.wantContentType; if (g != e) {
                        t.Errorf("test %q using %q: got content-type = %q, want %q", testName, method, g, e);
                    }
                }
                {
                    @string g = res.Header.Get(contentRangeˢ);
                    @string e = tt.wantContentRange; if (g != e) {
                        t.Errorf("test %q using %q: got content-range = %q, want %q", testName, method, g, e);
                    }
                }
                {
                    @string g = res.Header.Get(lastModifiedˢ);
                    @string e = tt.wantLastMod; if (g != e) {
                        t.Errorf("test %q using %q: got last-modified = %q, want %q", testName, method, g, e);
                    }
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpFooˢ => ref httpFooˢᵇ;
private static readonly @string httpFooˢᵇ = "http://foo/"u8;
internal static ref readonly @string fileTxtˢ => ref fileTxtˢᵇ;
private static readonly @string fileTxtˢᵇ = "file.txt"u8;
internal static ref readonly @string forbiddenˢ => ref forbiddenˢᵇ;
private static readonly @string forbiddenˢᵇ = "Forbidden"u8;

// Issue 12991
public static void TestServerFileStatError(ж<testing.T> t) {
    var rec = httptest.NewRecorder();
    var (r, _) = NewRequest(getˢ2, httpFooˢ, default);
    var redirect = false;
    @string name = fileTxtˢ;
    var fs = new issue12991FS();
    http_internal_test_package.ExportServeFile(new http_test_package.httptest_ResponseRecorderжResponseWriter(rec), r, fs, name, redirect);
    @string body = rec.Body.String(); if (!strings.Contains(body, "403") || !strings.Contains(body, forbiddenˢ)) {
        t.Errorf("wanted 403 forbidden message; got: %s", body);
    }
}

partial struct issue12991FS {
}

internal static (Δhttp.File, error) Open(this issue12991FS _Δp0, @string _Δp1) { return (new issue12991File(), default); }

partial struct issue12991File {
    /*embed*/ public global::go.net.http_package.File File;
}

internal static (fs.FileInfo, error) Stat(this issue12991File _) { return (default, fs.ErrPermission); }
internal static error Close(this issue12991File _) { return default; }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string keepheaders0ˢ => ref keepheaders0ˢᵇ;
private static readonly @string keepheaders0ˢᵇ = "keepheaders=0"u8;
internal static ref readonly @string keepheaders1ˢ => ref keepheaders1ˢᵇ;
private static readonly @string keepheaders1ˢᵇ = "keepheaders=1"u8;

public static void TestFileServerErrorMessages(ж<testing.T> t) {
    run<TжTBRun>(t, (TжTBRun tΔ1Δp, testMode mode) => {
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
internal static ref readonly @string randomErrorˢ => ref randomErrorˢᵇ;
private static readonly @string randomErrorˢᵇ = "random error"u8;
internal static ref readonly @string tudeˢ => ref tudeˢᵇ;
private static readonly @string tudeˢᵇ = "étude"u8;
internal static ref readonly @string cacheControlˢ => ref cacheControlˢᵇ;
private static readonly @string cacheControlˢᵇ = "Cache-Control"u8;
internal static ref readonly @string yesˢ => ref yesˢᵇ;
private static readonly @string yesˢᵇ = "yes"u8;
internal static ref readonly @string awesomeˢ => ref awesomeˢᵇ;
private static readonly @string awesomeˢᵇ = "awesome"u8;
internal static ref readonly @string yesterdayˢ => ref yesterdayˢᵇ;
private static readonly @string yesterdayˢᵇ = "yesterday"u8;
internal static ref readonly @string notPresentˢ => ref notPresentˢᵇ;
private static readonly @string notPresentˢᵇ = "not present"u8;
internal static ref readonly @string presentˢ => ref presentˢᵇ;
private static readonly @string presentˢᵇ = "present"u8;

internal static void testFileServerErrorMessages(ж<testing.T> t, testMode mode, bool keepHeaders) {
    if (keepHeaders) {
        t.Setenv(godebugˢ, "httpservecontentkeepheaders=1");
    }
    var fsΔ1 = new fakeFS(new map<@string, ж<fakeFileInfo>>{
        ["/500"] = Ꮡ(new fakeFileInfo(
            err: errors.New(randomErrorˢ)
        )),
        ["/403"] = Ꮡ(new fakeFileInfo(
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
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(NilSafeDelegateConversion<Δhttp.HandlerFunc, Action<Δhttp.ResponseWriter, ж<Δhttp.Request>>>(h))).ts;
    var c = ts.Client();
    foreach (var (_, code) in range(slice<nint>([403, 404, 500]))) {
        var (res, err) = c.Get(fmt.Sprintf("%s/%d"u8, ts.URL, code));
        if (err != nil) {
            t.Errorf("Error fetching /%d: %v", code, err);
            continue;
        }
        res.Body.Close();
        if (res.StatusCode != code) {
            t.Errorf("GET /%d: StatusCode = %d; want %d", code, res.StatusCode, code);
        }
        foreach (var (_, hdr) in range(slice<@string>(["Etag", "Last-Modified", "Cache-Control"]))) {
            var (v, got) = res.Header[hdr, ꟷ]; if (got != keepHeaders) {
                @string want = notPresentˢ;
                if (keepHeaders) {
                    want = presentˢ;
                }
                t.Errorf("GET /%d: Header[%q] = %q, want %v", code, hdr, v, want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingLinuxOnlyTestˢ = (@string)"skipping; linux-only test"u8;
internal static ref readonly @string straceˢ => ref straceˢᵇ;
private static readonly @string straceˢᵇ = "strace"u8;
internal static readonly object skippingStraceNotFoundInˢ = (@string)"skipping; strace not found in path"u8;
internal static ref readonly @string testRunˢ => ref testRunˢᵇ;
private static readonly @string testRunˢᵇ = "-test.run=^$"u8;
internal static ref readonly @string testRunˢ2 => ref testRunˢ2ᵇ;
private static readonly @string testRunˢ2ᵇ = "-test.run=^TestLinuxSendfileChild$"u8;
internal static ref readonly @string bN64Sendfile64ˢ => ref bN64Sendfile64ˢᵇ;
private static readonly @string bN64Sendfile64ˢᵇ = @"\b(n64:)?sendfile(64)?\("u8;

// verifies that sendfile is being used on Linux
public static void TestLinuxSendfile(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        setParallel(t);
        defer(afterTest, new http_test_package.testing_TжTB(t), ref ᒐ);
        if (runtime.GOOS != "linux"u8) {
            t.Skip(skippingLinuxOnlyTestˢ);
        }
        var (_, errΔ1) = exec.LookPath(straceˢ); if (errΔ1 != nil) {
            t.Skip(skippingStraceNotFoundInˢ);
        }

        var (ln, err) = net.Listen(tcpˢ, "127.0.0.1:0");
        if (err != nil) {
            t.Fatal(err);
        }
        (var lnf, err) = ln._<ж<net.TCPListener>>().File();
        if (err != nil) {
            t.Fatal(err);
        }
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);

        // Attempt to run strace, and skip on failure - this test requires SYS_PTRACE.
        var errΔ2 = testenv.Command(new http_test_package.testing_TжTB(t), straceˢ, "-f", "-q", os.Args[0], testRunˢ).Run(); if (errΔ2 != nil) {
            t.Skipf("skipping; failed to run strace: %v", errΔ2);
        }

        @string filename = fmt.Sprintf("1kb-%d"u8, os.Getpid());
        @string filepath = path.Join(os.TempDir(), filename);

        var errΔ3 = os.WriteFile(filepath, bytes.Repeat([(byte)'a'], 1 << 10), 493); if (errΔ3 != nil) {
            t.Fatal(errΔ3);
        }
        defer(os.Remove, filepath, ref ᒐ);

        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        var child = testenv.Command(new http_test_package.testing_TжTB(t), straceˢ, "-f", "-q", os.Args[0], testRunˢ2);
        child.ExtraFiles = append(child.ExtraFiles, lnf);
        child.Env = appendꓸꓸꓸ(slice<@string>(["GO_WANT_HELPER_PROCESS=1"]), os.Environ());
        child.Stdout = new http_test_package.strings_BuilderжWriter(Ꮡbuf);
        child.Stderr = new http_test_package.strings_BuilderжWriter(Ꮡbuf);
        var errΔ4 = child.Start(); if (errΔ4 != nil) {
            t.Skipf("skipping; failed to start straced child: %v", errΔ4);
        }

        (var res, err) = Get(fmt.Sprintf("http://%s/%s"u8, ln.Addr(), filename));
        if (err != nil) {
            t.Fatalf("http client error: %v", err);
        }
        (_, err) = io.Copy(io.Discard, res.Body);
        if (err != nil) {
            t.Fatalf("client body read error: %v", err);
        }
        res.Body.Close();

        // Force child to exit cleanly.
        Post(fmt.Sprintf("http://%s/quit"u8, ln.Addr()), ""u8, default);
        child.Wait();

        var rx = regexp.MustCompile(bN64Sendfile64ˢ);
        @string @out = buf.String();
        if (!rx.MatchString(@out)) {
            t.Errorf("no sendfile system call found in:\n%s", @out);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static (ж<Δhttp.Response>, slice<byte>) getBody(ж<testing.T> t, @string testName, Δhttp.Request reqʗp, ж<Δhttp.Client> client) {
    ref var req = ref heap(reqʗp, out var Ꮡreq);

    var (r, err) = client.Do(Ꮡreq);
    if (err != nil) {
        t.Fatalf("%s: for URL %q, send error: %v", testName, req.URL.String(), err);
    }
    (var b, err) = io.ReadAll(r.Body);
    if (err != nil) {
        t.Fatalf("%s: for URL %q, reading body: %v", testName, req.URL.String(), err);
    }
    return (r, b);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goWantHelperProcessˢ => ref goWantHelperProcessˢᵇ;
private static readonly @string goWantHelperProcessˢᵇ = "GO_WANT_HELPER_PROCESS"u8;
internal static ref readonly @string ephemeralPortListenerˢ => ref ephemeralPortListenerˢᵇ;
private static readonly @string ephemeralPortListenerˢᵇ = "ephemeral-port-listener"u8;
internal static ref readonly @string quitˢ => ref quitˢᵇ;
private static readonly @string quitˢᵇ = "/quit"u8;

// TestLinuxSendfileChild isn't a real test. It's used as a helper process
// for TestLinuxSendfile.
public static void TestLinuxSendfileChild(ж<testing.T> _) {
    GoFrame ᒐ = default;
    try {
        if (os.Getenv(goWantHelperProcessˢ) != "1"u8) {
            return;
        }
        defer(os.Exit, (nint)0, ref ᒐ);
        var fd3 = os.NewFile(3, ephemeralPortListenerˢ);
        var (ln, err) = net.FileListener(fd3);
        if (err != nil) {
            throw panic(err);
        }
        var mux = NewServeMux();
        mux.Handle("/", FileServer((Δhttp.Dir)os.TempDir()));
        mux.HandleFunc(quitˢ, (Δhttp.ResponseWriter _Δp0, ж<Δhttp.Request> _Δp1) => {
            os.Exit(0);
        });
        var s = Ꮡ(new Server(Handler: new Δhttp.ServeMuxжΔHandler(mux)));
        err = s.Serve(ln);
        if (err != nil) {
            throw panic(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issues 18984, 49552: tests that requests for paths beyond files return not-found errors
public static void TestFileServerNotDirError(ж<testing.T> t) {
    run<TжTBRun>(t, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        tΔ1.Run(dirˢ, (ж<testing.T> tΔ2) => {
            testFileServerNotDirError(tΔ2, mode, (@string path) => ((Δhttp.Dir)path));
        });
        tΔ1.Run("FS", (ж<testing.T> tΔ3) => {
            testFileServerNotDirError(tΔ3, mode, (@string path) => FS(os.DirFS(path)));
        });
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string indexHtmlNotAFileˢ => ref indexHtmlNotAFileˢᵇ;
private static readonly @string indexHtmlNotAFileˢᵇ = "/index.html/not-a-file"u8;
internal static readonly object errNilWantNilˢ = (@string)"err == nil; want != nil"u8;
internal static ref readonly @string indexHtmlNotADirNotAFileˢ => ref indexHtmlNotADirNotAFileˢᵇ;
private static readonly @string indexHtmlNotADirNotAFileˢᵇ = "/index.html/not-a-dir/not-a-file"u8;
internal static readonly object getAbsPathˢ = (@string)"get abs path:"u8;
internal static ref readonly @string relativePathˢ => ref relativePathˢᵇ;
private static readonly @string relativePathˢᵇ = "RelativePath"u8;
internal static ref readonly @string absolutePathˢ => ref absolutePathˢᵇ;
private static readonly @string absolutePathˢᵇ = "AbsolutePath"u8;

internal static void testFileServerNotDirError(ж<testing.T> t, testMode mode, Func<@string, Δhttp.FileSystem> newfs) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, FileServer(newfs(testdataˢ))).ts;

    ref var err = ref heap<error>(out var Ꮡerr);
    (var res, err) = ts.Client().Get(ts.URL + "/index.html/not-a-file"u8);
    if (err != nil) {
        t.Fatal(err);
    }
    res.Body.Close();
    if (res.StatusCode != 404) {
        t.Errorf("StatusCode = %v; want 404", res.StatusCode);
    }

    void test(@string name, Δhttp.FileSystem fsys) {
        t.Run(name, (ж<testing.T> tΔ1) => {
            (_, Ꮡerr.ValueSlot) = fsys.Open(indexHtmlNotAFileˢ);
            if (Ꮡerr.ValueSlot == nil) {
                tΔ1.Fatal(errNilWantNilˢ);
            }
            if (!errors.Is(Ꮡerr.ValueSlot, fs.ErrNotExist)) {
                tΔ1.Errorf("err = %v; errors.Is(err, fs.ErrNotExist) = %v; want true", Ꮡerr.ValueSlot,
                    errors.Is(Ꮡerr.ValueSlot, fs.ErrNotExist));
            }

            (_, Ꮡerr.ValueSlot) = fsys.Open(indexHtmlNotADirNotAFileˢ);
            if (Ꮡerr.ValueSlot == nil) {
                tΔ1.Fatal(errNilWantNilˢ);
            }
            if (!errors.Is(Ꮡerr.ValueSlot, fs.ErrNotExist)) {
                tΔ1.Errorf("err = %v; errors.Is(err, fs.ErrNotExist) = %v; want true", Ꮡerr.ValueSlot,
                    errors.Is(Ꮡerr.ValueSlot, fs.ErrNotExist));
            }
        });
    }

    (var absPath, err) = filepath.Abs(testdataˢ);
    if (err != nil) {
        t.Fatal(getAbsPathˢ, err);
    }

    test(relativePathˢ, newfs(testdataˢ));
    test(absolutePathˢ, newfs(absPath));
}

internal partial struct TestFileServerCleanPath_tests /*dyn*/ {
    internal @string path;
    internal nint wantCode;
    internal slice<@string> wantOpen;
}

public static void TestFileServerCleanPath(ж<testing.T> t) {
    var tests = slice<TestFileServerCleanPath_tests>([
        new("/", 200, ["/", "/index.html"]),
        new("/dir", 301, ["/dir"]),
        new("/dir/", 200, ["/dir", "/dir/index.html"])
    ]);
    foreach (var (_, tt) in range(tests)) {
        ref var log = ref heap<slice<@string>>(out var Ꮡlog);
        var rr = httptest.NewRecorder();
        var (req, _) = NewRequest(getˢ2, "http://foo.localhost"u8 + tt.path, default);
        FileServer(new fileServerCleanPathDir(Ꮡlog)).ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(rr), req);
        if (!slices.Equal<slice<@string>, @string>(log, tt.wantOpen)) {
            t.Logf("For %s: Opens = %q; want %q", tt.path, log, tt.wantOpen);
        }
        if (rr.Code != tt.wantCode) {
            t.Logf("For %s: Response code = %d; want %d", tt.path, rr.Code, tt.wantCode);
        }
    }
}

partial struct fileServerCleanPathDir {
    internal ж<slice<@string>> log;
}

internal static (Δhttp.File, error) Open(this fileServerCleanPathDir d, @string path) {
    (d.log).ValueSlot = append((d.log).ValueSlot, path);
    if (path == "/"u8 || path == "/dir"u8 || path == "/dir/"u8) {
        // Just return back something that's a directory.
        return ((Δhttp.Dir)(@string)".").Open(".");
    }
    return (default, fs.ErrNotExist);
}

partial struct panicOnSeek {
    /*embed*/ public io_package.ReadSeeker ReadSeeker;
}

internal partial struct TestScanETag_tests /*dyn*/ {
    internal @string @in;
    internal @string wantETag;
    internal @string wantRemain;
}

public static void TestScanETag(ж<testing.T> t) {
    var tests = slice<TestScanETag_tests>([
        new(@"W/""etag-1""", @"W/""etag-1""", ""),
        new(@"""etag-2""", @"""etag-2""", ""),
        new(@"""etag-1"", ""etag-2""", @"""etag-1""", @", ""etag-2"""),
        new("", "", ""),
        new("W/", "", ""),
        new(@"W/""truc", "", ""),
        new(@"w/""case-sensitive""", "", ""),
        new(@"""spaced etag""", "", "")
    ]);
    foreach (var (_, test) in range(tests)) {
        var (etag, remain) = http_internal_test_package.ExportScanETag(test.@in);
        if (etag != test.wantETag || remain != test.wantRemain) {
            t.Errorf("scanETag(%q)=%q %q, want %q %q", test.@in, etag, remain, test.wantETag, test.wantRemain);
        }
    }
}

// Issue 40940: Ensure that we only accept non-negative suffix-lengths
// in "Range": "bytes=-N", and should reject "bytes=--2".
public static void TestServeFileRejectsInvalidSuffixLengths(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testServeFileRejectsInvalidSuffixLengths(Δp0, Δp1), slice<testMode>([http1Mode, https1Mode, http2Mode]));
}
internal partial struct testServeFileRejectsInvalidSuffixLengths_tests /*dyn*/ {
    internal @string r;
    internal nint wantCode;
    internal @string wantBody;
}

internal static void testServeFileRejectsInvalidSuffixLengths(ж<testing.T> t, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, FileServer((Δhttp.Dir)(@string)testdataˢ)).ts;

    var tests = slice<testServeFileRejectsInvalidSuffixLengths_tests>([
        new("bytes=--6", 416, "invalid range\n"),
        new("bytes=--0", 416, "invalid range\n"),
        new("bytes=---0", 416, "invalid range\n"),
        new("bytes=-6", 206, "hello\n"),
        new("bytes=6-", 206, "html says hello\n"),
        new("bytes=-6-", 416, "invalid range\n"),
        new("bytes=-0", 206, ""),
        new("bytes=", 200, "index.html says hello\n")
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new testServeFileRejectsInvalidSuffixLengths_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var cstʗ1 = cst;
        var ttʗ1 = tt;
        t.Run(tt.r, (ж<testing.T> tΔ1) => {
            var (req, err) = NewRequest(getˢ2, cstʗ1.URL + "/index.html"u8, default);
            if (err != nil) {
                tΔ1.Fatal(err);
            }
            req.Header.Set(rangeˢ, ttʗ1.r);
            (var res, err) = cstʗ1.Client().Do(req);
            if (err != nil) {
                tΔ1.Fatal(err);
            }
            {
                nint g = res.StatusCode;
                nint w = ttʗ1.wantCode; if (g != w) {
                    tΔ1.Errorf("StatusCode mismatch: got %d want %d", g, w);
                }
            }
            (var slurp, err) = io.ReadAll(res.Body);
            res.Body.Close();
            if (err != nil) {
                tΔ1.Fatal(err);
            }
            {
                @string g = (@string)slurp;
                @string w = ttʗ1.wantBody; if (g != w) {
                    tΔ1.Fatalf("Content mismatch:\nGot:  %q\nWant: %q", g, w);
                }
            }
        });
    }
}

public static void TestFileServerMethods(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testFileServerMethods(Δp0, Δp1));
}
internal static void testFileServerMethods(ж<testing.T> t, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, FileServer((Δhttp.Dir)(@string)testdataˢ)).ts;

    var (@file, err) = os.ReadFile(testFile);
    if (err != nil) {
        t.Fatal(readingFileˢ, err);
    }

    // Get contents via various methods.
    //
    // See https://go.dev/issue/59471 for a proposal to limit the set of methods handled.
    // For now, test the historical behavior.
    foreach (var (_, method) in range(slice<@string>([
        MethodGet,
        MethodHead,
        MethodPost,
        MethodPut,
        MethodPatch,
        MethodDelete,
        MethodOptions,
        MethodTrace
    ]))) {
        var (req, _) = NewRequest(method, ts.URL + "/file"u8, default);
        t.Log(req.URL.OrTypedNil());
        var (res, errΔ1) = ts.Client().Do(req);
        if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        (var body, errΔ1) = io.ReadAll(res.Body);
        res.Body.Close();
        if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        var wantBody = @file;
        if (method == MethodHead) {
            wantBody = default;
        }
        if (!bytes.Equal(body, wantBody)) {
            t.Fatalf("%v: got body %q, want %q", method, body, wantBody);
        }
        @string got = res.Header.Get(contentLengthˢ);
        @string want = fmt.Sprint(len(@file)); if (got != want) {
            t.Fatalf("%v: got Content-Length %q, want %q", method, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string indexHtmlˢ => ref indexHtmlˢᵇ;
private static readonly @string indexHtmlˢᵇ = "index.html"u8;

public static void TestFileServerFS(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string filename = indexHtmlˢ;
        var contents = slice<byte>("index.html says hello"u8);
        var fsys = new fstest.MapFS(new map<@string, ж<fstest.MapFile>>{
            [filename] = Ꮡ(new fstest.MapFile(Data: contents))
        });
        var ts = newClientServerTest(new http_test_package.testing_TжTB(t), http1Mode, FileServerFS(new http_test_package.fstest_MapFSᴠFS(fsys))).ts;
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);

        var (res, err) = ts.Client().Get(ts.URL + "/"u8 + filename);
        if (err != nil) {
            t.Fatal(err);
        }
        (var b, err) = io.ReadAll(res.Body);
        if (err != nil) {
            t.Fatal(readingBodyˢ, err);
        }
        @string s = (@string)b; if (s != ((sstring)contents)) {
            t.Errorf("for path %q got %q, want %q", filename, s, contents);
        }
        res.Body.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestServeFileFS(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string filename = indexHtmlˢ;
        var contents = slice<byte>("index.html says hello"u8);
        var fsys = new fstest.MapFS(new map<@string, ж<fstest.MapFile>>{
            [filename] = Ꮡ(new fstest.MapFile(Data: contents))
        });
        var fsysʗ1 = fsys;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(t), http1Mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            ServeFileFS(w, r, new http_test_package.fstest_MapFSᴠFS(fsysʗ1), filename);
        }))).ts;
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);

        var (res, err) = ts.Client().Get(ts.URL + "/"u8 + filename);
        if (err != nil) {
            t.Fatal(err);
        }
        (var b, err) = io.ReadAll(res.Body);
        if (err != nil) {
            t.Fatal(readingBodyˢ, err);
        }
        @string s = (@string)b; if (s != ((sstring)contents)) {
            t.Errorf("for path %q got %q, want %q", filename, s, contents);
        }
        res.Body.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestServeFileZippingResponseWriter(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
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
        var ts = newClientServerTest(new http_test_package.testing_TжTB(t), http1Mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            GoFrame ᒐ = default;
            try {
                w.Header().Set(contentEncodingˢ, gzipˢ);
                var gzw = gzip.NewWriter(new http_test_package.http_ResponseWriterᴠWriter(w));
                var gzwʗ1 = gzw;
                defer(() => gzwʗ1.Close(), ref ᒐ);
                ServeFileFS(new gzipResponseWriter(w: gzw, ResponseWriter: w), r, new http_test_package.fstest_MapFSᴠFS(fsysʗ1), filename);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))).ts;
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);

        var (res, err) = ts.Client().Get(ts.URL + "/"u8 + filename);
        if (err != nil) {
            t.Fatal(err);
        }
        (var b, err) = io.ReadAll(res.Body);
        if (err != nil) {
            t.Fatal(readingBodyˢ, err);
        }
        @string s = (@string)b; if (s != ((sstring)contents)) {
            t.Errorf("for path %q got %q, want %q", filename, s, contents);
        }
        res.Body.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct gzipResponseWriter {
    /*embed*/ public global::go.net.http_package.ResponseWriter ResponseWriter;
    internal ж<gzip.Writer> w;
}

internal static (nint, error) Write(this gzipResponseWriter grw, slice<byte> b) {
    return grw.w.Write(b);
}

internal static void Flush(this gzipResponseWriter grw) {
    grw.w.Flush();
    var (fw, ok) = grw.ResponseWriter._<Δhttp.Flusher>(ᐧ); if (ok) {
        fw.Flush();
    }
}

// Issue 63769
public static void TestFileServerDirWithRootFile(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testFileServerDirWithRootFile(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fileServerˢ => ref fileServerˢᵇ;
private static readonly @string fileServerˢᵇ = "FileServer"u8;
internal static ref readonly @string fileServerFSˢ => ref fileServerFSˢᵇ;
private static readonly @string fileServerFSˢᵇ = "FileServerFS"u8;

internal static void testFileServerDirWithRootFile(ж<testing.T> t, testMode mode) {
    void testDirFile(ж<testing.T> tΔ1, httpꓸHandler h) {
        GoFrame ᒐ = default;
        try {
            var ts = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), mode, h).ts;
            var tsʗ1 = ts;
            defer(tsʗ1.Close, ref ᒐ);

            var (res, err) = ts.Client().Get(ts.URL);
            if (err != nil) {
                tΔ1.Fatal(err);
            }
            nint g = res.StatusCode;
            nint w = StatusInternalServerError; if (g != w) {
                tΔ1.Errorf("StatusCode mismatch: got %d, want: %d", g, w);
            }
            res.Body.Close();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    var testDirFileʗ1 = testDirFile;
    t.Run(fileServerˢ, (ж<testing.T> tΔ2) => {
        testDirFileʗ1(tΔ2, FileServer((Δhttp.Dir)(@string)testdataIndexHtmlˢ));
    });

    var testDirFileʗ2 = testDirFile;
    t.Run(fileServerFSˢ, (ж<testing.T> tΔ3) => {
        testDirFileʗ2(tΔ3, FileServerFS(os.DirFS(testdataIndexHtmlˢ)));
    });
}

public static void TestServeContentHeadersWithError(ж<testing.T> t) {
    t.Run(keepheaders0ˢ, (ж<testing.T> tΔ1) => {
        testServeContentHeadersWithError(tΔ1, false);
    });
    t.Run(keepheaders1ˢ, (ж<testing.T> tΔ2) => {
        testServeContentHeadersWithError(tΔ2, true);
    });
}
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string applicationOctetStreamˢ => ref applicationOctetStreamˢᵇ;
private static readonly @string applicationOctetStreamˢᵇ = "application/octet-stream"u8;
internal static ref readonly @string abcdefghˢ => ref abcdefghˢᵇ;
private static readonly @string abcdefghˢᵇ = @"""abcdefgh"""u8;
internal static ref readonly @string wed21Oct2015072800Gmtˢ => ref wed21Oct2015072800Gmtˢᵇ;
private static readonly @string wed21Oct2015072800Gmtˢᵇ = "Wed, 21 Oct 2015 07:28:00 GMT"u8;
internal static ref readonly @string immutableˢ => ref immutableˢᵇ;
private static readonly @string immutableˢᵇ = "immutable"u8;
internal static ref readonly @string otherHeaderˢ => ref otherHeaderˢᵇ;
private static readonly @string otherHeaderˢᵇ = "Other-Header"u8;
internal static ref readonly @string testˢ2 => ref testˢ2ᵇ;
private static readonly @string testˢ2ᵇ = "test"u8;
internal static ref readonly @string bytes10010000ˢ => ref bytes10010000ˢᵇ;
private static readonly @string bytes10010000ˢᵇ = "bytes=100-10000"u8;
internal static ref readonly @string invalidRangeFailedToˢ => ref invalidRangeFailedToˢᵇ;
private static readonly @string invalidRangeFailedToˢᵇ = "invalid range: failed to overlap\n"u8;
internal static ref readonly @string bytes7ˢ => ref bytes7ˢᵇ;
private static readonly @string bytes7ˢᵇ = "bytes */7"u8;

internal static void testServeContentHeadersWithError(ж<testing.T> t, bool keepHeaders) {
    GoFrame ᒐ = default;
    try {
        if (keepHeaders) {
            t.Setenv(godebugˢ, "httpservecontentkeepheaders=1");
        }
        var contents = slice<byte>("content"u8);
        var contentsʗ1 = contents;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(t), http1Mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.Header().Set(contentTypeˢ, applicationOctetStreamˢ);
            w.Header().Set(contentLengthˢ, strconv.Itoa(len(contentsʗ1)));
            w.Header().Set(contentEncodingˢ, gzipˢ);
            w.Header().Set(etagˢ, abcdefghˢ);
            w.Header().Set(lastModifiedˢ, wed21Oct2015072800Gmtˢ);
            w.Header().Set(cacheControlˢ, immutableˢ);
            w.Header().Set(otherHeaderˢ, testˢ2);
            ServeContent(w, r, ""u8, new time.Time(), new http_test_package.bytes_ReaderжReadSeeker(bytes.NewReader(contentsʗ1)));
        }))).ts;
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);

        var (req, err) = NewRequest(getˢ2, ts.URL, default);
        if (err != nil) {
            t.Fatal(err);
        }
        req.Header.Set(rangeˢ, bytes10010000ˢ);

        var c = ts.Client();
        (var res, err) = c.Do(req);
        if (err != nil) {
            t.Fatal(err);
        }

        var (@out, _) = io.ReadAll(res.Body);
        res.Body.Close();

        @string ifKept(@string s) {
            if (keepHeaders) {
                return s;
            }
            return ""u8;
        }
        {
            nint g = res.StatusCode;
            nint e = 416; if (g != e) {
                t.Errorf("got status = %d; want %d", g, e);
            }
        }
        {
            @string g = (@string)@out;
            @string e = invalidRangeFailedToˢ; if (g != e) {
                t.Errorf("got body = %q; want %q", g, e);
            }
        }
        {
            @string g = res.Header.Get(contentTypeˢ);
            @string e = textPlainCharsetUtf8ˢ; if (g != e) {
                t.Errorf("got content-type = %q, want %q", g, e);
            }
        }
        {
            @string g = res.Header.Get(contentLengthˢ);
            @string e = strconv.Itoa(len(@out)); if (g != e) {
                t.Errorf("got content-length = %q, want %q", g, e);
            }
        }
        {
            @string g = res.Header.Get(contentEncodingˢ);
            @string e = ifKept(gzipˢ); if (g != e) {
                t.Errorf("got content-encoding = %q, want %q", g, e);
            }
        }
        {
            @string g = res.Header.Get(etagˢ);
            @string e = ifKept(abcdefghˢ); if (g != e) {
                t.Errorf("got etag = %q, want %q", g, e);
            }
        }
        {
            @string g = res.Header.Get(lastModifiedˢ);
            @string e = ifKept(wed21Oct2015072800Gmtˢ); if (g != e) {
                t.Errorf("got last-modified = %q, want %q", g, e);
            }
        }
        {
            @string g = res.Header.Get(cacheControlˢ);
            @string e = ifKept(immutableˢ); if (g != e) {
                t.Errorf("got cache-control = %q, want %q", g, e);
            }
        }
        {
            @string g = res.Header.Get(contentRangeˢ);
            @string e = bytes7ˢ; if (g != e) {
                t.Errorf("got content-range = %q, want %q", g, e);
            }
        }
        {
            @string g = res.Header.Get(otherHeaderˢ);
            @string e = testˢ2; if (g != e) {
                t.Errorf("got other-header = %q, want %q", g, e);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end http_test_package
