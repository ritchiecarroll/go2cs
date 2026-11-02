// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bufio = bufio_package;
using bytes = bytes_package;
using context = context_package;
using rand = crypto.rand_package;
using base64 = encoding.base64_package;
using json = encoding.json_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using math = math_package;
using multipart = global::go.mime.multipart_package;
using Δhttp = global::go.net.http_package;
using static global::go.net.http_package;
using httptest = global::go.net.http.httptest_package;
using url = global::go.net.url_package;
using os = os_package;
using reflect = reflect_package;
using regexp = regexp_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using crypto;
using encoding;
using global::go.mime;
using global::go.net;
using global::go.net.http;
using static global::go.net.http_internal_test_package;

partial class http_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpWwwGoogleComSearchQˢ => ref httpWwwGoogleComSearchQˢᵇ;
private static readonly @string httpWwwGoogleComSearchQˢᵇ = "http://www.google.com/search?q=foo&q=bar"u8;

public static void TestQuery(ж<testing.T> t) {
    var req = Ꮡ(new Request(Method: "GET"));
    (req.URL, _) = url.Parse(httpWwwGoogleComSearchQˢ);
    {
        @string q = req.FormValue("q"); if (q != "foo"u8) {
            t.Errorf(@"req.FormValue(""q"") = %q, want ""foo""", q);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpWwwGoogleComSearchQˢ2 => ref httpWwwGoogleComSearchQˢ2ᵇ;
private static readonly @string httpWwwGoogleComSearchQˢ2ᵇ = "http://www.google.com/search?q=foo;q=bar&a=1"u8;

// Issue #25192: Test that ParseForm fails but still parses the form when a URL
// containing a semicolon is provided.
public static void TestParseFormSemicolonSeparator(ж<testing.T> t) {
    foreach (var (_, method) in slice<@string>(["POST", "PATCH", "PUT", "GET"])) {
        var (req, _) = NewRequest(method, httpWwwGoogleComSearchQˢ2,
            new http_test_package.strings_ReaderжReader(strings.NewReader("q")));
        var err = req.ParseForm();
        if (err == nil) {
            t.Fatalf(@"for method %s, ParseForm expected an error, got success", method);
        }
        var wantForm = new url.Values(new map<@string, slice<@string>>{["a"] = slice<@string>(["1"])});
        if (!reflect.DeepEqual(req.Form, wantForm)) {
            t.Fatalf("for method %s, ParseForm expected req.Form = %v, want %v", method, req.Form, wantForm);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpWwwGoogleComSearchQˢ3 => ref httpWwwGoogleComSearchQˢ3ᵇ;
private static readonly @string httpWwwGoogleComSearchQˢ3ᵇ = "http://www.google.com/search?q=foo&q=bar&both=x&prio=1&orphan=nope&empty=not"u8;
internal static ref readonly @string zPostBothYPrio2Nokeyˢ => ref zPostBothYPrio2Nokeyˢᵇ;
private static readonly @string zPostBothYPrio2Nokeyˢᵇ = "z=post&both=y&prio=2&=nokey&orphan&empty=&"u8;
internal static ref readonly @string applicationXWwwFormˢ2 => ref applicationXWwwFormˢ2ᵇ;
private static readonly @string applicationXWwwFormˢ2ᵇ = "application/x-www-form-urlencoded; param=value"u8;
internal static ref readonly @string bothˢ => ref bothˢᵇ;
private static readonly @string bothˢᵇ = "both"u8;
internal static ref readonly @string prioˢ => ref prioˢᵇ;
private static readonly @string prioˢᵇ = "prio"u8;
internal static ref readonly @string orphanˢ => ref orphanˢᵇ;
private static readonly @string orphanˢᵇ = "orphan"u8;
internal static ref readonly @string emptyˢ => ref emptyˢᵇ;
private static readonly @string emptyˢᵇ = "empty"u8;

public static void TestParseFormQuery(ж<testing.T> t) {
    var (req, _) = NewRequest(postˢ, httpWwwGoogleComSearchQˢ3,
        new http_test_package.strings_ReaderжReader(strings.NewReader(zPostBothYPrio2Nokeyˢ)));
    req.Header.Set(contentTypeˢ, applicationXWwwFormˢ2);

    {
        @string q = req.FormValue("q"); if (q != "foo"u8) {
            t.Errorf(@"req.FormValue(""q"") = %q, want ""foo""", q);
        }
    }
    {
        @string z = req.FormValue("z"); if (z != "post"u8) {
            t.Errorf(@"req.FormValue(""z"") = %q, want ""post""", z);
        }
    }
    {
        var (bq, found) = req.PostForm["q", ꟷ]; if (found) {
            t.Errorf(@"req.PostForm[""q""] = %q, want no entry in map", bq);
        }
    }
    {
        @string bz = req.PostFormValue("z"); if (bz != "post"u8) {
            t.Errorf(@"req.PostFormValue(""z"") = %q, want ""post""", bz);
        }
    }
    {
        var qs = req.Form["q"]; if (!slices.Equal<slice<@string>, @string>(qs, slice<@string>(["foo", "bar"]))) {
            t.Errorf(@"req.Form[""q""] = %q, want [""foo"", ""bar""]", qs);
        }
    }
    {
        var both = req.Form[bothˢ]; if (!slices.Equal<slice<@string>, @string>(both, slice<@string>(["y", "x"]))) {
            t.Errorf(@"req.Form[""both""] = %q, want [""y"", ""x""]", both);
        }
    }
    {
        @string prio = req.FormValue(prioˢ); if (prio != "2"u8) {
            t.Errorf(@"req.FormValue(""prio"") = %q, want ""2"" (from body)", prio);
        }
    }
    {
        var orphan = req.Form[orphanˢ]; if (!slices.Equal<slice<@string>, @string>(orphan, slice<@string>(["", "nope"]))) {
            t.Errorf(@"req.FormValue(""orphan"") = %q, want """" (from body)", orphan);
        }
    }
    {
        var empty = req.Form[emptyˢ]; if (!slices.Equal<slice<@string>, @string>(empty, slice<@string>(["", "not"]))) {
            t.Errorf(@"req.FormValue(""empty"") = %q, want """" (from body)", empty);
        }
    }
    {
        var nokey = req.Form[""u8]; if (!slices.Equal<slice<@string>, @string>(nokey, slice<@string>(["nokey"]))) {
            t.Errorf(@"req.FormValue(""nokey"") = %q, want ""nokey"" (from body)", nokey);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpWwwGoogleComSearchˢ => ref httpWwwGoogleComSearchˢᵇ;
private static readonly @string httpWwwGoogleComSearchˢᵇ = "http://www.google.com/search"u8;

// Tests that we only parse the form automatically for certain methods.
public static void TestParseFormQueryMethods(ж<testing.T> t) {
    foreach (var (_, method) in slice<@string>(["POST", "PATCH", "PUT", "FOO"])) {
        var (req, _) = NewRequest(method, httpWwwGoogleComSearchˢ,
            new http_test_package.strings_ReaderжReader(strings.NewReader(fooBarˢ)));
        req.Header.Set(contentTypeˢ, applicationXWwwFormˢ2);
        @string want = barˢ;
        if (method == "FOO"u8) {
            want = ""u8;
        }
        {
            @string got = req.FormValue(fooˢ); if (got != want) {
                t.Errorf(@"for method %s, FormValue(""foo"") = %q; want %q", method, got, want);
            }
        }
    }
}

internal partial struct TestParseFormUnknownContentType_type /*dyn*/ {
    internal @string name;
    internal @string wantErr;
    internal httpꓸHeader contentType;
}

public static void TestParseFormUnknownContentType(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in slice<TestParseFormUnknownContentType_type>([
        new("text", "", new httpꓸHeader(new map<@string, slice<@string>>{["Content-Type"] = slice<@string>(["text/plain"])})),
        // Empty content type is legal - may be treated as
        // application/octet-stream (RFC 7231, section 3.1.1.5)
        new("empty", "", new httpꓸHeader(new map<@string, slice<@string>>{})),
        new("boundary", "mime: invalid media parameter", new httpꓸHeader(new map<@string, slice<@string>>{["Content-Type"] = slice<@string>(["text/plain; boundary="])})),
        new("unknown", "", new httpꓸHeader(new map<@string, slice<@string>>{["Content-Type"] = slice<@string>(["application/unknown"])}))
    ])) {
        ref var test = ref heap(new TestParseFormUnknownContentType_type(), out var Ꮡtest);
        test = vᴛ1;

            var testʗ1 = test;
        t.Run(test.name,
            (ж<testing.T> tΔ1) => {
                var req = Ꮡ(new Request(
                    Method: "POST",
                    Header: testʗ1.contentType,
                    Body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(bodyˢ)))
                ));
                var err = req.ParseForm();
                switch (ᐧ) {
                case {} when err == nil && testʗ1.wantErr != ""u8: {
                    tΔ1.Errorf("unexpected success; want error %q", testʗ1.wantErr);
                    break;
                }
                case {} when err != nil && testʗ1.wantErr == ""u8: {
                    tΔ1.Errorf("want success, got error: %v", err);
                    break;
                }
                case {} when testʗ1.wantErr != ""u8 && testʗ1.wantErr != fmt.Sprint(err): {
                    tΔ1.Errorf("got error %q; want %q", err, testʗ1.wantErr);
                    break;
                }}
            });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpWwwGoogleComSearchQˢ4 => ref httpWwwGoogleComSearchQˢ4ᵇ;
private static readonly @string httpWwwGoogleComSearchQˢ4ᵇ = "http://www.google.com/search?q=foo"u8;

public static void TestParseFormInitializeOnError(ж<testing.T> t) {
    var (nilBody, _) = NewRequest(postˢ, httpWwwGoogleComSearchQˢ4, default);
    var tests = slice<ж<Δhttp.Request>>([
        nilBody,
        Ꮡ(new Δhttp.Request(Method: "GET", URL: nil))
    ]);
    foreach (var (i, req) in tests) {
        var err = req.ParseForm();
        if (req.Form == nil) {
            t.Errorf("%d. Form not initialized, error %v", i, err);
        }
        if (req.PostForm == nil) {
            t.Errorf("%d. PostForm not initialized, error %v", i, err);
        }
    }
}

internal partial struct TestMultipartReader_tests /*dyn*/ {
    internal bool shouldError;
    internal @string contentType;
}

public static void TestMultipartReader(ж<testing.T> t) {
    var tests = slice<TestMultipartReader_tests>([
        new(false, @"multipart/form-data; boundary=""foo123"""),
        new(false, @"multipart/mixed; boundary=""foo123"""),
        new(true, @"text/plain")
    ]);

    foreach (var (i, test) in tests) {
        var req = Ꮡ(new Request(
            Method: "POST",
            Header: new httpꓸHeader(new map<@string, slice<@string>>{["Content-Type"] = slice<@string>([test.contentType])}),
            Body: io.NopCloser(new http_test_package.bytes_BufferжReader(@new<bytes.Buffer>()))
        ));
        var (multipart, err) = req.MultipartReader();
        if (test.shouldError) {
            if (err == nil || multipart != nil) {
                t.Errorf("test %d: unexpectedly got nil-error (%v) or non-nil-multipart (%v)", i, err, multipart.OrTypedNil());
            }
            continue;
        }
        if (err != nil || multipart == nil) {
            t.Errorf("test %d: unexpectedly got error (%v) or nil-multipart (%v)", i, err, multipart.OrTypedNil());
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string xxxContentDispositionˢ => ref xxxContentDispositionˢᵇ;
private static readonly @string xxxContentDispositionˢᵇ = """
--xxx
Content-Disposition: form-data; name="field1"

value1
--xxx
Content-Disposition: form-data; name="field2"

value2
--xxx
Content-Disposition: form-data; name="file"; filename="file"
Content-Type: application/octet-stream
Content-Transfer-Encoding: binary

binary data
--xxx--

"""u8.Raw();

// Issue 9305: ParseMultipartForm should populate PostForm too
public static void TestParseMultipartFormPopulatesPostForm(ж<testing.T> t) {
    @string postData = xxxContentDispositionˢ;
    var req = Ꮡ(new Request(
        Method: "POST",
        Header: new httpꓸHeader(new map<@string, slice<@string>>{["Content-Type"] = slice<@string>([@"multipart/form-data; boundary=xxx"])}),
        Body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(postData)))
    ));

    var initialFormItems = new map<@string, @string>{
        ["language"] = "Go",
        ["name"] = "gopher",
        ["skill"] = "go-ing",
        ["field2"] = "initial-value2"
    };

    req.Form = new url.Values(0);
    foreach (var (k, v) in initialFormItems) {
        req.Form.Add(k, v);
    }

    var err = req.ParseMultipartForm(10000);
    if (err != nil) {
        t.Fatalf("unexpected multipart error %v", err);
    }

    var wantForm = new url.Values(new map<@string, slice<@string>>{
        ["language"] = slice<@string>(["Go"]),
        ["name"] = slice<@string>(["gopher"]),
        ["skill"] = slice<@string>(["go-ing"]),
        ["field1"] = slice<@string>(["value1"]),
        ["field2"] = slice<@string>(["initial-value2", "value2"])
    });
    if (!reflect.DeepEqual(req.Form, wantForm)) {
        t.Fatalf("req.Form = %v, want %v", req.Form, wantForm);
    }

    var wantPostForm = new url.Values(new map<@string, slice<@string>>{
        ["field1"] = slice<@string>(["value1"]),
        ["field2"] = slice<@string>(["value2"])
    });
    if (!reflect.DeepEqual(req.PostForm, wantPostForm)) {
        t.Fatalf("req.PostForm = %v, want %v", req.PostForm, wantPostForm);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedMultipartEofGotˢ = (@string)"expected multipart EOF, got nil"u8;
internal static readonly object expectedErrNotMultipartˢ = (@string)"expected ErrNotMultipart for text/plain"u8;

public static void TestParseMultipartForm(ж<testing.T> t) {
    var req = Ꮡ(new Request(
        Method: "POST",
        Header: new httpꓸHeader(new map<@string, slice<@string>>{["Content-Type"] = slice<@string>([@"multipart/form-data; boundary=""foo123"""])}),
        Body: io.NopCloser(new http_test_package.bytes_BufferжReader(@new<bytes.Buffer>()))
    ));
    var err = req.ParseMultipartForm(25);
    if (err == nil) {
        t.Error(expectedMultipartEofGotˢ);
    }

    req.Header = new httpꓸHeader(new map<@string, slice<@string>>{["Content-Type"] = slice<@string>(["text/plain"])});
    err = req.ParseMultipartForm(25);
    if (!AreEqual(err, ErrNotMultipart)) {
        t.Error(expectedErrNotMultipartˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string xxxContentDispositionˢ2 => ref xxxContentDispositionˢ2ᵇ;
private static readonly @string xxxContentDispositionˢ2ᵇ = """
--xxx
Content-Disposition: form-data; name="file"; filename="../usr/foobar.txt/"
Content-Type: text/plain

--xxx--

"""u8.Raw();
internal static ref readonly @string fileˢ => ref fileˢᵇ;
private static readonly @string fileˢᵇ = "file"u8;

// Issue 45789: multipart form should not include directory path in filename
public static void TestParseMultipartFormFilename(ж<testing.T> t) {
    @string postData = xxxContentDispositionˢ2;
    var req = Ꮡ(new Request(
        Method: "POST",
        Header: new httpꓸHeader(new map<@string, slice<@string>>{["Content-Type"] = slice<@string>([@"multipart/form-data; boundary=xxx"])}),
        Body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(postData)))
    ));
    var (_, hdr, err) = req.FormFile(fileˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    if (hdr.Filename != "foobar.txt"u8) {
        t.Errorf("expected only the last element of the path, got %q", hdr.Filename);
    }
}

// Issue #40430: Test that if maxMemory for ParseMultipartForm when combined with
// the payload size and the internal leeway buffer size of 10MiB overflows, that we
// correctly return an error.
public static void TestMaxInt64ForMultipartFormMaxMemoryOverflow(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testMaxInt64ForMultipartFormMaxMemoryOverflow(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string myfileTxtˢ => ref myfileTxtˢᵇ;
private static readonly @string myfileTxtˢᵇ = "myfile.txt"u8;

internal static void testMaxInt64ForMultipartFormMaxMemoryOverflow(ж<testing.T> t, testMode mode) {
    nint payloadSize = 1 << 10;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> reqΔ1) => {
        // The combination of:
        //      MaxInt64 + payloadSize + (internal spare of 10MiB)
        // triggers the overflow. See issue https://golang.org/issue/40430/
        {
            var errΔ1 = reqΔ1.ParseMultipartForm(math.MaxInt64); if (errΔ1 != nil) {
                Error(rw, errΔ1.Error(), StatusBadRequest);
                return;
            }
        }
    }))).ts;
    var fBuf = @new<bytes.Buffer>();
    var mw = multipart.NewWriter(new http_test_package.bytes_BufferжWriter(fBuf));
    var (mf, err) = mw.CreateFormFile(fileˢ, myfileTxtˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    {
        var (_, errΔ2) = mf.Write(bytes.Repeat(slice<byte>("abc"u8), payloadSize)); if (errΔ2 != nil) {
            t.Fatal(errΔ2);
        }
    }
    {
        var errΔ3 = mw.Close(); if (errΔ3 != nil) {
            t.Fatal(errΔ3);
        }
    }
    (var req, err) = NewRequest(postˢ, cst.URL, new http_test_package.bytes_BufferжReader(fBuf));
    if (err != nil) {
        t.Fatal(err);
    }
    req.Header.Set(contentTypeˢ, mw.FormDataContentType());
    (var res, err) = cst.Client().Do(req);
    if (err != nil) {
        t.Fatal(err);
    }
    res.Body.Close();
    {
        nint g = res.StatusCode;
        nint w = StatusOK; if (g != w) {
            t.Fatalf("Status code mismatch: got %d, want %d", g, w);
        }
    }
}

public static void TestRequestRedirect(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testRequestRedirect(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ6 => ref fooˢ6ᵇ;
private static readonly @string fooˢ6ᵇ = "/foo/$"u8;

internal static void testRequestRedirect(ж<testing.T> t, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> rΔ1) => {
        var exprᴛ1 = rΔ1.URL.Path;
        switch (ᐧ) {
        case {} when exprᴛ1 == "/"u8: {
            w.Header().Set(locationˢ, fooˢ5);
            w.WriteHeader(StatusSeeOther);
            break;
        }
        case {} when exprᴛ1 == "/foo/"u8: {
            fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "foo"u8);
            break;
        }
        default: {
            w.WriteHeader(StatusBadRequest);
            break;
        }}
    })));

    ж<regexp.Regexp> end = regexp.MustCompile(fooˢ6);
    var (r, err) = cst.c.Get(cst.ts.URL);
    if (err != nil) {
        t.Fatal(err);
    }
    r.Body.Close();
    @string url = r.Request.URL.String();
    if (r.StatusCode != 200 || !end.MatchString(url)) {
        t.Fatalf("Get got status %d at %q, want 200 matching /foo/$", r.StatusCode, url);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpExampleComˢ2 => ref httpExampleComˢ2ᵇ;
private static readonly @string httpExampleComˢ2ᵇ = "http://example.com/"u8;
internal static ref readonly @string aladdinˢ => ref aladdinˢᵇ;
private static readonly @string aladdinˢᵇ = "Aladdin"u8;
internal static ref readonly @string openSesameˢ => ref openSesameˢᵇ;
private static readonly @string openSesameˢᵇ = "open sesame"u8;
internal static ref readonly @string basicˢ2 => ref basicˢ2ᵇ;
private static readonly @string basicˢ2ᵇ = "Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ=="u8;

public static void TestSetBasicAuth(ж<testing.T> t) {
    var (r, _) = NewRequest(getˢ2, httpExampleComˢ2, default);
    r.SetBasicAuth(aladdinˢ, openSesameˢ);
    {
        @string g = r.Header.Get(authorizationˢ);
        @string e = basicˢ2; if (g != e) {
            t.Errorf("got header %q, want %q", g, e);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object parseMultipartFormFirstˢ = (@string)"ParseMultipartForm first call:"u8;
internal static readonly object parseMultipartFormSecondˢ = (@string)"ParseMultipartForm second call:"u8;

public static void TestMultipartRequest(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // Test that we can read the values and files of a
        // multipart request with FormValue and FormFile,
        // and that ParseMultipartForm can be called multiple times.
        var req = newTestMultipartRequest(t);
        {
            var err = req.ParseMultipartForm(25); if (err != nil) {
                t.Fatal(parseMultipartFormFirstˢ, err);
            }
        }
        var reqʗ1 = req;
        defer(ᴛ0 => ᴛ0.RemoveAll(), reqʗ1.MultipartForm, ref ᒐ);
        validateTestMultipartContents(t, req, false);
        {
            var err = req.ParseMultipartForm(25); if (err != nil) {
                t.Fatal(parseMultipartFormSecondˢ, err);
            }
        }
        validateTestMultipartContents(t, req, false);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object parseMultipartFormˢ = (@string)"ParseMultipartForm expected error due to invalid semicolon, got nil"u8;

// Issue #25192: Test that ParseMultipartForm fails but still parses the
// multi-part form when a URL containing a semicolon is provided.
public static void TestParseMultipartFormSemicolonSeparator(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var req = newTestMultipartRequest(t);
        req.URL = Ꮡ(new url.URL(RawQuery: "q=foo;q=bar"));
        {
            var err = req.ParseMultipartForm(25); if (err == nil) {
                t.Fatal(parseMultipartFormˢ);
            }
        }
        var reqʗ1 = req;
        defer(ᴛ0 => ᴛ0.RemoveAll(), reqʗ1.MultipartForm, ref ᒐ);
        validateTestMultipartContents(t, req, false);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestMultipartRequestAuto(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // Test that FormValue and FormFile automatically invoke
        // ParseMultipartForm and return the right values.
        var req = newTestMultipartRequest(t);
        var reqʗ1 = req;
        defer(() => {
            if (reqʗ1.MultipartForm != nil) {
                reqʗ1.MultipartForm.RemoveAll();
            }
        }, ref ᒐ);
        validateTestMultipartContents(t, req, true);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestMissingFileMultipartRequest(ж<testing.T> t) {
    // Test that FormFile returns an error if
    // the named file is missing.
    var req = newTestMultipartRequest(t);
    testMissingFile(t, req);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpWwwGoogleComˢ => ref httpWwwGoogleComˢᵇ;
private static readonly @string httpWwwGoogleComˢᵇ = "http://www.google.com/"u8;
internal static ref readonly @string zPostˢ => ref zPostˢᵇ;
private static readonly @string zPostˢᵇ = "z=post"u8;
internal static readonly object unexpectedRequestFormˢ = (@string)"Unexpected request Form, want nil"u8;
internal static readonly object parseMultipartFormNotˢ = (@string)"ParseMultipartForm not called by FormValue"u8;

// Test that FormValue invokes ParseMultipartForm.
public static void TestFormValueCallsParseMultipartForm(ж<testing.T> t) {
    var (req, _) = NewRequest(postˢ, httpWwwGoogleComˢ, new http_test_package.strings_ReaderжReader(strings.NewReader(zPostˢ)));
    req.Header.Set(contentTypeˢ, applicationXWwwFormˢ2);
    if (req.Form != nil) {
        t.Fatal(unexpectedRequestFormˢ);
    }
    req.FormValue("z");
    if (req.Form == nil) {
        t.Fatal(parseMultipartFormNotˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object parseMultipartFormNotˢ2 = (@string)"ParseMultipartForm not called by FormFile"u8;

// Test that FormFile invokes ParseMultipartForm.
public static void TestFormFileCallsParseMultipartForm(ж<testing.T> t) {
    var req = newTestMultipartRequest(t);
    if (req.Form != nil) {
        t.Fatal(unexpectedRequestFormˢ);
    }
    req.FormFile(""u8);
    if (req.Form == nil) {
        t.Fatal(parseMultipartFormNotˢ2);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedAnErrorFromˢ = (@string)"expected an error from ParseMultipartForm after call to MultipartReader"u8;

// Test that ParseMultipartForm errors if called
// after MultipartReader on the same request.
public static void TestParseMultipartFormOrder(ж<testing.T> t) {
    var req = newTestMultipartRequest(t);
    {
        var (_, err) = req.MultipartReader(); if (err != nil) {
            t.Fatalf("MultipartReader: %v", err);
        }
    }
    {
        var err = req.ParseMultipartForm(1024); if (err == nil) {
            t.Fatal(expectedAnErrorFromˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedAnErrorFromˢ2 = (@string)"expected an error from MultipartReader after call to ParseMultipartForm"u8;

// Test that MultipartReader errors if called
// after ParseMultipartForm on the same request.
public static void TestMultipartReaderOrder(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var req = newTestMultipartRequest(t);
        {
            var err = req.ParseMultipartForm(25); if (err != nil) {
                t.Fatalf("ParseMultipartForm: %v", err);
            }
        }
        var reqʗ1 = req;
        defer(ᴛ0 => ᴛ0.RemoveAll(), reqʗ1.MultipartForm, ref ᒐ);
        {
            var (_, err) = req.MultipartReader(); if (err == nil) {
                t.Fatal(expectedAnErrorFromˢ2);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedAnErrorFromˢ3 = (@string)"expected an error from FormFile after call to MultipartReader"u8;

// Test that FormFile errors if called after
// MultipartReader on the same request.
public static void TestFormFileOrder(ж<testing.T> t) {
    var req = newTestMultipartRequest(t);
    {
        var (_, err) = req.MultipartReader(); if (err != nil) {
            t.Fatalf("MultipartReader: %v", err);
        }
    }
    {
        var (_, _, err) = req.FormFile(""u8); if (err == nil) {
            t.Fatal(expectedAnErrorFromˢ3);
        }
    }
}

partial struct readRequestErrorTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal @string err;
    internal httpꓸHeader header;
}
internal static slice<readRequestErrorTestsᴛ1> readRequestErrorTests = new slice<readRequestErrorTestsᴛ1>(11){
    [0] = new("GET / HTTP/1.1\r\nheader:foo\r\n\r\n", "", new httpꓸHeader(new map<@string, slice<@string>>{["Header"] = slice<@string>(["foo"])})),
    [1] = new("GET / HTTP/1.1\r\nheader:foo\r\n", io.ErrUnexpectedEOF.Error(), default),
    [2] = new("", io.EOF.Error(), default),
    [3] = new(
        @in: "HEAD / HTTP/1.1\r\n\r\n",
        header: new httpꓸHeader(new map<@string, slice<@string>>{})
    ),

    // Multiple Content-Length values should either be
    // deduplicated if same or reject otherwise
    // See Issue 16490.
    [4] = new(
        @in: "POST / HTTP/1.1\r\nContent-Length: 10\r\nContent-Length: 0\r\n\r\nGopher hey\r\n",
        err: "cannot contain multiple Content-Length headers"
    ),
    [5] = new(
        @in: "POST / HTTP/1.1\r\nContent-Length: 10\r\nContent-Length: 6\r\n\r\nGopher\r\n",
        err: "cannot contain multiple Content-Length headers"
    ),
    [6] = new(
        @in: "PUT / HTTP/1.1\r\nContent-Length: 6 \r\nContent-Length: 6\r\nContent-Length:6\r\n\r\nGopher\r\n",
        err: "",
        header: new httpꓸHeader(new map<@string, slice<@string>>{["Content-Length"] = slice<@string>(["6"])})
    ),
    [7] = new(
        @in: "PUT / HTTP/1.1\r\nContent-Length: 1\r\nContent-Length: 6 \r\n\r\n",
        err: "cannot contain multiple Content-Length headers"
    ),
    [8] = new(
        @in: "POST / HTTP/1.1\r\nContent-Length:\r\nContent-Length: 3\r\n\r\n",
        err: "cannot contain multiple Content-Length headers"
    ),
    [9] = new(
        @in: "HEAD / HTTP/1.1\r\nContent-Length:0\r\nContent-Length: 0\r\n\r\n",
        header: new httpꓸHeader(new map<@string, slice<@string>>{["Content-Length"] = slice<@string>(["0"])})
    ),
    [10] = new(
        @in: "HEAD / HTTP/1.1\r\nHost: foo\r\nHost: bar\r\n\r\n\r\n\r\n",
        err: "too many Host headers"
    )
};

public static void TestReadRequestErrors(ж<testing.T> t) {
    foreach (var (i, tt) in readRequestErrorTests) {
        var (req, err) = ReadRequest(bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader(tt.@in))));
        if (err == nil) {
            if (tt.err != ""u8) {
                t.Errorf("#%d: got nil err; want %q", i, tt.err);
            }

            if (!reflect.DeepEqual(tt.header, req.Header)) {
                t.Errorf("#%d: gotHeader: %q wantHeader: %q", i, req.Header, tt.header);
            }
            continue;
        }

        if (tt.err == ""u8 || !strings.Contains(err.Error(), tt.err)) {
            t.Errorf("%d: got error = %v; want %v", i, err, tt.err);
        }
    }
}

partial struct newRequestHostTestsᴛ1 /*dyn*/ {
    internal @string @in, @out;
}
internal static slice<newRequestHostTestsᴛ1> newRequestHostTests = slice<newRequestHostTestsᴛ1>([
    new("http://www.example.com/", "www.example.com"),
    new("http://www.example.com:8080/", "www.example.com:8080"),

    new("http://192.168.0.1/", "192.168.0.1"),
    new("http://192.168.0.1:8080/", "192.168.0.1:8080"),
    new("http://192.168.0.1:/", "192.168.0.1"),

    new("http://[fe80::1]/", "[fe80::1]"),
    new("http://[fe80::1]:8080/", "[fe80::1]:8080"),
    new("http://[fe80::1%25en0]/", "[fe80::1%en0]"),
    new("http://[fe80::1%25en0]:8080/", "[fe80::1%en0]:8080"),
    new("http://[fe80::1%25en0]:/", "[fe80::1%en0]")
]);

public static void TestNewRequestHost(ж<testing.T> t) {
    foreach (var (i, tt) in newRequestHostTests) {
        var (req, err) = NewRequest(getˢ2, tt.@in, default);
        if (err != nil) {
            t.Errorf("#%v: %v", i, err);
            continue;
        }
        if (req.Host != tt.@out) {
            t.Errorf("got %q; want %q", req.Host, tt.@out);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string badMethodˢ => ref badMethodˢᵇ;
private static readonly @string badMethodˢᵇ = "bad method"u8;
internal static ref readonly @string httpFooComˢ => ref httpFooComˢᵇ;
private static readonly @string httpFooComˢᵇ = "http://foo.com/"u8;
internal static readonly object expectedErrorFromˢ = (@string)"expected error from NewRequest with invalid method"u8;
internal static ref readonly @string httpFooExampleˢ => ref httpFooExampleˢᵇ;
private static readonly @string httpFooExampleˢᵇ = "http://foo.example/"u8;
internal static ref readonly @string invalidMethodˢ => ref invalidMethodˢᵇ;
private static readonly @string invalidMethodˢᵇ = "invalid method"u8;

public static void TestRequestInvalidMethod(ж<testing.T> t) {
    var (_, err) = NewRequest(badMethodˢ, httpFooComˢ, default);
    if (err == nil) {
        t.Error(expectedErrorFromˢ);
    }
    (var req, err) = NewRequest(getˢ2, httpFooExampleˢ, default);
    if (err != nil) {
        t.Fatal(err);
    }
    req.Method = badMethodˢ;
    (_, err) = DefaultClient.Do(req);
    if (err == nil || !strings.Contains(err.Error(), invalidMethodˢ)) {
        t.Errorf("Transport error = %v; want invalid method", err);
    }

    (req, err) = NewRequest(""u8, httpFooComˢ, default);
    if (err != nil) {
        t.Errorf("NewRequest(empty method) = %v; want nil", err);
    } else if (req.Method != "GET"u8) {
        t.Errorf("NewRequest(empty method) has method %q; want GET", req.Method);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string xyzˢ => ref xyzˢᵇ;
private static readonly @string xyzˢᵇ = "xyz"u8;
internal static ref readonly @string httpLocalhostˢ => ref httpLocalhostˢᵇ;
private static readonly @string httpLocalhostˢᵇ = "http://localhost/"u8;

internal partial struct TestNewRequestContentLength_tests /*dyn*/ {
    internal io.Reader r;
    internal int64 want;
}

internal partial struct TestNewRequestContentLength_type /*dyn*/ {
    /*embed*/ public io_package.Reader Reader;
}

public static void TestNewRequestContentLength(ж<testing.T> t) {
    io.Reader readByte(io.Reader r) {
        array<byte> b = new(1);
        r.Read(b[..]);
        return r;
    }
    var tests = slice<TestNewRequestContentLength_tests>([
        new(new http_test_package.bytes_ReaderжReader(bytes.NewReader(slice<byte>("123"u8))), 3),
        new(new http_test_package.bytes_BufferжReader(bytes.NewBuffer(slice<byte>("1234"u8))), 4),
        new(new http_test_package.strings_ReaderжReader(strings.NewReader("12345")), 5),
        new(new http_test_package.strings_ReaderжReader(strings.NewReader(""u8)), 0),
        new(new http_test_package.http_noBodyᴠReader(NoBody), 0),

        // Not detected. During Go 1.8 we tried to make these set to -1, but
        // due to Issue 18117, we keep these returning 0, even though they're
        // unknown.
        new(new TestNewRequestContentLength_type(new http_test_package.strings_ReaderжReader(strings.NewReader(xyzˢ))), 0),
        new(new io.SectionReaderжReader(io.NewSectionReader(new http_test_package.strings_ReaderжReaderAt(strings.NewReader("x")), 0, 6)), 0),
        new(readByte(new io.SectionReaderжReader(io.NewSectionReader(new http_test_package.strings_ReaderжReaderAt(strings.NewReader("xy")), 0, 6))), 0)
    ]);
    foreach (var (i, tt) in tests) {
        var (req, err) = NewRequest(postˢ, httpLocalhostˢ, tt.r);
        if (err != nil) {
            t.Fatal(err);
        }
        if (req.ContentLength != tt.want) {
            t.Errorf("test[%d]: ContentLength(%T) = %d; want %d", i, tt.r, req.ContentLength, tt.want);
        }
    }
}

partial struct parseHTTPVersionTestsᴛ1 /*dyn*/ {
    internal @string vers;
    internal nint major, minor;
    internal bool ok;
}
internal static slice<parseHTTPVersionTestsᴛ1> parseHTTPVersionTests = slice<parseHTTPVersionTestsᴛ1>([
    new("HTTP/0.0", 0, 0, true),
    new("HTTP/0.9", 0, 9, true),
    new("HTTP/1.0", 1, 0, true),
    new("HTTP/1.1", 1, 1, true),

    new("HTTP", 0, 0, false),
    new("HTTP/one.one", 0, 0, false),
    new("HTTP/1.1/", 0, 0, false),
    new("HTTP/-1,0", 0, 0, false),
    new("HTTP/0,-1", 0, 0, false),
    new("HTTP/", 0, 0, false),
    new("HTTP/1,1", 0, 0, false),
    new("HTTP/+1.1", 0, 0, false),
    new("HTTP/1.+1", 0, 0, false),
    new("HTTP/0000000001.1", 0, 0, false),
    new("HTTP/1.0000000001", 0, 0, false),
    new("HTTP/3.14", 0, 0, false),
    new("HTTP/12.3", 0, 0, false)
]);

internal partial struct TestParseHTTPVersion_version /*dyn*/ {
    internal nint major, minor;
    internal bool ok;
}

public static void TestParseHTTPVersion(ж<testing.T> t) {
    foreach (var (_, tt) in parseHTTPVersionTests) {
        var (major, minor, ok) = ParseHTTPVersion(tt.vers);
        if (ok != tt.ok || major != tt.major || minor != tt.minor) {
            t.Errorf("failed to parse %q, expected: %#v, got %#v", tt.vers, new TestParseHTTPVersion_version(tt.major, tt.minor, tt.ok), new TestParseHTTPVersion_version(major, minor, ok));
        }
    }
}

partial struct getBasicAuthTest {
    internal @string username, password;
    internal bool ok;
}

partial struct basicAuthCredentialsTest {
    internal @string username, password;
}

partial struct getBasicAuthTestsᴛ1 /*dyn*/ {
    internal @string username, password;
    internal bool ok;
}
internal static slice<getBasicAuthTestsᴛ1> getBasicAuthTests = slice<getBasicAuthTestsᴛ1>([
    new("Aladdin", "open sesame", true),
    new("Aladdin", "open:sesame", true),
    new("", "", true)
]);

public static void TestGetBasicAuth(ж<testing.T> t) {
    foreach (var (_, tt) in getBasicAuthTests) {
        var (rΔ1, _) = NewRequest(getˢ2, httpExampleComˢ2, default);
        rΔ1.SetBasicAuth(tt.username, tt.password);
        var (usernameΔ1, passwordΔ1, okΔ1) = rΔ1.BasicAuth();
        if (okΔ1 != tt.ok || usernameΔ1 != tt.username || passwordΔ1 != tt.password) {
            t.Errorf("BasicAuth() = %#v, want %#v", new getBasicAuthTest(usernameΔ1, passwordΔ1, okΔ1),
                new getBasicAuthTest(tt.username, tt.password, tt.ok));
        }
    }
    // Unauthenticated request.
    var (r, _) = NewRequest(getˢ2, httpExampleComˢ2, default);
    var (username, password, ok) = r.BasicAuth();
    if (ok) {
        t.Errorf("expected false from BasicAuth when the request is unauthenticated");
    }
    var want = new basicAuthCredentialsTest("", "");
    if (username != want.username || password != want.password) {
        t.Errorf("expected credentials: %#v when the request is unauthenticated, got %#v",
            want, new basicAuthCredentialsTest(username, password));
    }
}

partial struct parseBasicAuthTestsᴛ1 /*dyn*/ {
    internal @string header, username, password;
    internal bool ok;
}
internal static slice<parseBasicAuthTestsᴛ1> parseBasicAuthTests = slice<parseBasicAuthTestsᴛ1>([
    new("Basic "u8 + base64.StdEncoding.EncodeToString(slice<byte>("Aladdin:open sesame"u8)), "Aladdin", "open sesame", true),

    // Case doesn't matter:
    new("BASIC "u8 + base64.StdEncoding.EncodeToString(slice<byte>("Aladdin:open sesame"u8)), "Aladdin", "open sesame", true),
    new("basic "u8 + base64.StdEncoding.EncodeToString(slice<byte>("Aladdin:open sesame"u8)), "Aladdin", "open sesame", true),

    new("Basic "u8 + base64.StdEncoding.EncodeToString(slice<byte>("Aladdin:open:sesame"u8)), "Aladdin", "open:sesame", true),
    new("Basic "u8 + base64.StdEncoding.EncodeToString(slice<byte>(":"u8)), "", "", true),
    new("Basic"u8 + base64.StdEncoding.EncodeToString(slice<byte>("Aladdin:open sesame"u8)), "", "", false),
    new(base64.StdEncoding.EncodeToString(slice<byte>("Aladdin:open sesame"u8)), "", "", false),
    new("Basic ", "", "", false),
    new("Basic Aladdin:open sesame", "", "", false),
    new(@"Digest username=""Aladdin""", "", "", false)
]);

public static void TestParseBasicAuth(ж<testing.T> t) {
    foreach (var (_, tt) in parseBasicAuthTests) {
        var (r, _) = NewRequest(getˢ2, httpExampleComˢ2, default);
        r.Header.Set(authorizationˢ, tt.header);
        var (username, password, ok) = r.BasicAuth();
        if (ok != tt.ok || username != tt.username || password != tt.password) {
            t.Errorf("BasicAuth() = %#v, want %#v", new getBasicAuthTest(username, password, ok),
                new getBasicAuthTest(tt.username, tt.password, tt.ok));
        }
    }
}

partial struct logWrites {
    internal ж<testing.T> t;
    internal ж<slice<@string>> dst;
}

internal static partial error WriteByte(this logWrites l, byte c) {
    l.t.Fatalf("unexpected WriteByte call");
    return default;
}

internal static (nint n, error err) Write(this logWrites l, slice<byte> p) {
    l.dst.ValueSlot = append(l.dst.ValueSlot, (@string)p);
    return (len(p), default);
}

public static void TestRequestWriteBufferedWriter(ж<testing.T> t) {
    ref var got = ref heap<slice<@string>>(slice<@string>([]), out var Ꮡgot);
    var (req, _) = NewRequest(getˢ2, httpFooComˢ, default);
    req.Write(new logWrites(t, Ꮡgot));
    var want = slice<@string>([
        "GET / HTTP/1.1\r\n",
        "Host: foo.com\r\n",
        "User-Agent: "u8 + http_internal_test_package.DefaultUserAgent + "\r\n"u8,
        "\r\n"
    ]);
    if (!slices.Equal<slice<@string>, @string>(got, want)) {
        t.Errorf("Writes = %q\n  Want = %q", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpFooAfterˢ => ref httpFooAfterˢᵇ;
private static readonly @string httpFooAfterˢᵇ = "http://foo/after"u8;
internal static ref readonly @string fooComNewlineˢ => ref fooComNewlineˢᵇ;
private static readonly @string fooComNewlineˢᵇ = "foo.com\nnewline"u8;

public static void TestRequestBadHostHeader(ж<testing.T> t) {
    ref var got = ref heap<slice<@string>>(slice<@string>([]), out var Ꮡgot);
    var (req, err) = NewRequest(getˢ2, httpFooAfterˢ, default);
    if (err != nil) {
        t.Fatal(err);
    }
    req.Host = fooComNewlineˢ;
    req.URL.Host = fooComNewlineˢ;
    req.Write(new logWrites(t, Ꮡgot));
    var want = slice<@string>([
        "GET /after HTTP/1.1\r\n",
        "Host: \r\n",
        "User-Agent: "u8 + http_internal_test_package.DefaultUserAgent + "\r\n"u8,
        "\r\n"
    ]);
    if (!slices.Equal<slice<@string>, @string>(got, want)) {
        t.Errorf("Writes = %q\n  Want = %q", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string evilXEvilEvilˢ => ref evilXEvilEvilˢᵇ;
private static readonly @string evilXEvilEvilˢᵇ = "evil\r\nX-Evil: evil"u8;

public static void TestRequestBadUserAgent(ж<testing.T> t) {
    ref var got = ref heap<slice<@string>>(slice<@string>([]), out var Ꮡgot);
    var (req, err) = NewRequest(getˢ2, httpFooAfterˢ, default);
    if (err != nil) {
        t.Fatal(err);
    }
    req.Header.Set(userAgentˢ2, evilXEvilEvilˢ);
    req.Write(new logWrites(t, Ꮡgot));
    var want = slice<@string>([
        "GET /after HTTP/1.1\r\n",
        "Host: foo\r\n",
        "User-Agent: evil  X-Evil: evil\r\n",
        "\r\n"
    ]);
    if (!slices.Equal<slice<@string>, @string>(got, want)) {
        t.Errorf("Writes = %q\n  Want = %q", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mSearchHttp11ˢ => ref mSearchHttp11ˢᵇ;
private static readonly @string mSearchHttp11ˢᵇ = "M-SEARCH * HTTP/1.1\r\n\r\n"u8;
internal static ref readonly @string chunkedˢ => ref chunkedˢᵇ;
private static readonly @string chunkedˢᵇ = "chunked"u8;
internal static readonly object wroteChunkedRequestWantˢ = (@string)"wrote chunked request; want no body"u8;

public static void TestStarRequest(ж<testing.T> t) {
    var (req, err) = ReadRequest(bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader(mSearchHttp11ˢ))));
    if (err != nil) {
        return;
    }
    if (req.ContentLength != 0) {
        t.Errorf("ContentLength = %d; want 0", req.ContentLength);
    }
    if (req.Body == nil) {
        t.Errorf("Body = nil; want non-nil");
    }

    // Request.Write has Client semantics for Body/ContentLength,
    // where ContentLength 0 means unknown if Body is non-nil, and
    // thus chunking will happen unless we change semantics and
    // signal that we want to serialize it as exactly zero.  The
    // only way to do that for outbound requests is with a nil
    // Body:
    ref var clientReq = ref heap<Δhttp.Request>(req.Value, out var ᏑclientReq);
    clientReq.Body = default;

    ref var @out = ref heap(new strings.Builder(), out var Ꮡout);
    {
        var errΔ1 = ᏑclientReq.Write(new http_test_package.strings_BuilderжWriter(Ꮡout)); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
    }

    if (strings.Contains(@out.String(), chunkedˢ)) {
        t.Error(wroteChunkedRequestWantˢ);
    }
    (var back, err) = ReadRequest(bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader(@out.String()))));
    if (err != nil) {
        t.Fatal(err);
    }
    // Ignore the Headers (the User-Agent breaks the deep equal,
    // but we don't care about it)
    req.Header = default;
    back.Header = default;
    if (!reflect.DeepEqual(req.OrTypedNil(), back.OrTypedNil())) {
        t.Errorf("Original request doesn't match Request read back.");
        t.Logf("Original: %#v", req.OrTypedNil());
        t.Logf("Original.URL: %#v", req.URL.OrTypedNil());
        t.Logf("Wrote: %s", @out.String());
        t.Logf("Read back (doesn't match Original): %#v", back.OrTypedNil());
    }
}

partial struct responseWriterJustWriter {
    /*embed*/ public io_package.Writer Writer;
}

internal static httpꓸHeader Header(this responseWriterJustWriter _) {
    throw panic("should not be called");
}

internal static void WriteHeader(this responseWriterJustWriter _Δp0, nint _Δp1) {
    throw panic("should not be called");
}

// delayedEOFReader never returns (n > 0, io.EOF), instead putting
// off the io.EOF until a subsequent Read call.
partial struct delayedEOFReader {
    internal io.Reader r;
}

internal static (nint n, error err) Read(this delayedEOFReader dr, slice<byte> p) {
    nint n = default;
    error err = default;

    (n, err) = dr.r.Read(p);
    if (n > 0 && AreEqual(err, io.EOF)) {
        err = default;
    }
    return (n, err);
}

public static void TestIssue10884_MaxBytesEOF(ж<testing.T> t) {
    var dst = io.Discard;
    var (_, err) = io.Copy(dst, MaxBytesReader(
        new responseWriterJustWriter(dst),
        io.NopCloser(new delayedEOFReader(new http_test_package.strings_ReaderжReader(strings.NewReader("12345")))),
        5));
    if (err != nil) {
        t.Fatal(err);
    }
}

internal partial struct TestMaxBytesReaderStickyError_tests /*dyn*/ {
    internal nint readable;
    internal int64 limit;
}

// Issue 14981: MaxBytesReader's return error wasn't sticky. It
// doesn't technically need to be, but people expected it to be.
public static void TestMaxBytesReaderStickyError(ж<testing.T> t) {
    error isSticky(io.Reader r) {
        ref var log = ref heap(new bytes.Buffer(), out var Ꮡlog);
        var buf = new slice<byte>(1000);
        error firstErr = default;
        for (;;) {
            var (n, err) = r.Read(buf);
            fmt.Fprintf(new http_test_package.bytes_BufferжWriter(Ꮡlog), "Read(%d) = %d, %v\n"u8, len(buf), n, err);
            if (err == nil) {
                continue;
            }
            if (firstErr == nil) {
                firstErr = err;
                continue;
            }
            if (!reflect.DeepEqual(err, firstErr)) {
                return fmt.Errorf("non-sticky error. got log:\n%s"u8, log.Bytes());
            }
            t.Logf("Got log: %s", log.Bytes());
            return default;
        }
    }
    var tests = new array<TestMaxBytesReaderStickyError_tests>(3){
        [0] = new(99, 100),
        [1] = new(100, 100),
        [2] = new(101, 100)
    };
    foreach (var (i, tt) in tests.ΔRangeSnapshot()) {
        var rc = MaxBytesReader(default, io.NopCloser(new http_test_package.bytes_ReaderжReader(bytes.NewReader(new slice<byte>(tt.readable)))), tt.limit);
        {
            var err = isSticky(rc); if (err != nil) {
                t.Errorf("%d. error: %v", i, err);
            }
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string testStrᶜ => ref testStrᶜᵇ;
private static readonly @string testStrᶜᵇ = "1234"u8;

internal partial struct TestMaxBytesReaderDifferentLimits_tests /*dyn*/ {
    internal int64 limit;
    internal nint lenP;
    internal nint wantN;
    internal bool wantErr;
}

// Issue 45101: maxBytesReader's Read panicked when n < -1. This test
// also ensures that Read treats negative limits as equivalent to 0.
public static void TestMaxBytesReaderDifferentLimits(ж<testing.T> t) {
    @string testStr = testStrᶜ;
    var tests = new array<TestMaxBytesReaderDifferentLimits_tests>(11){
        [0] = new(
            limit: -123,
            lenP: 0,
            wantN: 0,
            wantErr: false // Ensure we won't return an error when the limit is negative, but we don't need to read.

        ),
        [1] = new(
            limit: -100,
            lenP: 32 * 1024,
            wantN: 0,
            wantErr: true
        ),
        [2] = new(
            limit: -2,
            lenP: 1,
            wantN: 0,
            wantErr: true
        ),
        [3] = new(
            limit: -1,
            lenP: 2,
            wantN: 0,
            wantErr: true
        ),
        [4] = new(
            limit: 0,
            lenP: 3,
            wantN: 0,
            wantErr: true
        ),
        [5] = new(
            limit: 1,
            lenP: 4,
            wantN: 1,
            wantErr: true
        ),
        [6] = new(
            limit: 2,
            lenP: 5,
            wantN: 2,
            wantErr: true
        ),
        [7] = new(
            limit: 3,
            lenP: 2,
            wantN: 2,
            wantErr: false
        ),
        [8] = new(
            limit: (int64)len(testStr),
            lenP: len(testStr),
            wantN: len(testStr),
            wantErr: false
        ),
        [9] = new(
            limit: 100,
            lenP: 6,
            wantN: len(testStr),
            wantErr: false
        ),
        [10] = new( /* Issue 54408 */
            limit: (int64)(9223372036854775807L),
            lenP: len(testStr),
            wantN: len(testStr),
            wantErr: false
        )
    };
    foreach (var (i, tt) in tests.ΔRangeSnapshot()) {
        var rc = MaxBytesReader(default, io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(testStr))), tt.limit);

        var (n, err) = rc.Read(new slice<byte>(tt.lenP));

        if (n != tt.wantN) {
            t.Errorf("%d. n: %d, want n: %d", i, n, tt.wantN);
        }

        if ((err != nil) != tt.wantErr) {
            t.Errorf("%d. error: %v", i, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpsGolangOrgˢ => ref httpsGolangOrgˢᵇ;
private static readonly @string httpsGolangOrgˢᵇ = "https://golang.org/"u8;
internal static readonly object expectedNilUrlInClonedˢ = (@string)"expected nil URL in cloned request"u8;

public static void TestWithContextNilURL(ж<testing.T> t) {
    var (req, err) = NewRequest(postˢ, httpsGolangOrgˢ, default);
    if (err != nil) {
        t.Fatal(err);
    }

    // Issue 20601
    req.URL = default;
    var reqCopy = req.WithContext(context_package.Background());
    if (reqCopy.URL != nil) {
        t.Error(expectedNilUrlInClonedˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpsExampleOrgˢ => ref httpsExampleOrgˢᵇ;
private static readonly @string httpsExampleOrgˢᵇ = "https://example.org/"u8;
internal static ref readonly @string encoding2ˢ => ref encoding2ˢᵇ;
private static readonly @string encoding2ˢᵇ = "encoding2"u8;
internal static readonly object expectedReqˢ = (@string)"expected req.TransferEncoding to be changed"u8;
internal static readonly object expectedClonedReqˢ = (@string)"expected clonedReq.TransferEncoding to be unchanged"u8;

// Ensure that Request.Clone creates a deep copy of TransferEncoding.
// See issue 41907.
public static void TestRequestCloneTransferEncoding(ж<testing.T> t) {
    var body = strings.NewReader(bodyˢ);
    var (req, _) = NewRequest(postˢ, httpsExampleOrgˢ, new http_test_package.strings_ReaderжReader(body));
    req.TransferEncoding = slice<@string>([
        "encoding1"
    ]);

    var clonedReq = req.Clone(context_package.Background());
    // modify original after deep copy
    req.TransferEncoding[0] = encoding2ˢ;

    if (req.TransferEncoding[0] != "encoding2") {
        t.Error(expectedReqˢ);
    }
    if (clonedReq.TransferEncoding[0] != "encoding1") {
        t.Error(expectedClonedReqˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string origˢ => ref origˢᵇ;
private static readonly @string origˢᵇ = "orig"u8;
internal static ref readonly @string copyˢ => ref copyˢᵇ;
private static readonly @string copyˢᵇ = "copy"u8;

// Ensure that Request.Clone works correctly with PathValue.
// See issue 64911.
public static void TestRequestClonePathValue(ж<testing.T> t) {
    var (req, _) = Δhttp.NewRequest(getˢ2, httpsExampleOrgˢ, default);
    req.SetPathValue("p1", origˢ);

    var clonedReq = req.Clone(context_package.Background());
    clonedReq.SetPathValue("p2", copyˢ);

    // Ensure that any modifications to the cloned
    // request do not pollute the original request.
    {
        @string g = req.PathValue("p2");
        @string w = ""u8; if (g != w) {
            t.Fatalf("p2 mismatch got %q, want %q", g, w);
        }
    }
    {
        @string g = req.PathValue("p1");
        @string w = origˢ; if (g != w) {
            t.Fatalf("p1 mismatch got %q, want %q", g, w);
        }
    }

    // Assert on the changes to the cloned request.
    {
        @string g = clonedReq.PathValue("p1");
        @string w = origˢ; if (g != w) {
            t.Fatalf("p1 mismatch got %q, want %q", g, w);
        }
    }
    {
        @string g = clonedReq.PathValue("p2");
        @string w = copyˢ; if (g != w) {
            t.Fatalf("p2 mismatch got %q, want %q", g, w);
        }
    }
}

// Issue 34878: verify we don't panic when including basic auth (Go 1.13 regression)
public static void TestNoPanicOnRoundTripWithBasicAuth(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testNoPanicWithBasicAuth(Δp0, Δp1));
}

internal static void testNoPanicWithBasicAuth(ж<testing.T> t, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    })));

    var (u, err) = url.Parse(cst.ts.URL);
    if (err != nil) {
        t.Fatal(err);
    }
    u.User = url.UserPassword(fooˢ, barˢ);
    var req = Ꮡ(new Request(
        URL: u,
        Method: "GET"
    ));
    {
        var (_, errΔ1) = cst.c.Do(req); if (errΔ1 != nil) {
            t.Fatalf("Unexpected error: %v", errΔ1);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpFooTldˢ => ref httpFooTldˢᵇ;
private static readonly @string httpFooTldˢᵇ = "http://foo.tld/"u8;

internal partial struct TestNewRequestGetBody_tests /*dyn*/ {
    internal io.Reader r;
}

// verify that NewRequest sets Request.GetBody and that it works
public static void TestNewRequestGetBody(ж<testing.T> t) {
    var tests = slice<TestNewRequestGetBody_tests>([
        new(r: new http_test_package.strings_ReaderжReader(strings.NewReader(helloˢ))),
        new(r: new http_test_package.bytes_ReaderжReader(bytes.NewReader(slice<byte>("hello"u8)))),
        new(r: new http_test_package.bytes_BufferжReader(bytes.NewBuffer(slice<byte>("hello"u8))))
    ]);
    foreach (var (i, tt) in tests) {
        var (req, err) = NewRequest(postˢ, httpFooTldˢ, tt.r);
        if (err != nil) {
            t.Errorf("test[%d]: %v", i, err);
            continue;
        }
        if (req.Body == nil) {
            t.Errorf("test[%d]: Body = nil", i);
            continue;
        }
        if (req.GetBody == nil) {
            t.Errorf("test[%d]: GetBody = nil", i);
            continue;
        }
        (var slurp1, err) = io.ReadAll(req.Body);
        if (err != nil) {
            t.Errorf("test[%d]: ReadAll(Body) = %v", i, err);
        }
        (var newBody, err) = req.GetBody();
        if (err != nil) {
            t.Errorf("test[%d]: GetBody = %v", i, err);
        }
        (var slurp2, err) = io.ReadAll(newBody);
        if (err != nil) {
            t.Errorf("test[%d]: ReadAll(GetBody()) = %v", i, err);
        }
        if (((sstring)slurp1) != ((sstring)slurp2)) {
            t.Errorf("test[%d]: Body %q != GetBody %q", i, slurp1, slurp2);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string missingˢ => ref missingˢᵇ;
private static readonly @string missingˢᵇ = "missing"u8;

internal static void testMissingFile(ж<testing.T> t, ж<Δhttp.Request> req) {
    var (f, fh, err) = req.FormFile(missingˢ);
    if (f != nil) {
        t.Errorf("FormFile file = %v, want nil", f);
    }
    if (fh != nil) {
        t.Errorf("FormFile file header = %v, want nil", fh.OrTypedNil());
    }
    if (!AreEqual(err, ErrMissingFile)) {
        t.Errorf("FormFile err = %q, want ErrMissingFile", err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object newRequestˢ = (@string)"NewRequest:"u8;
internal static ref readonly @string contentTypeˢ2 => ref contentTypeˢ2ᵇ;
private static readonly @string contentTypeˢ2ᵇ = "Content-type"u8;

internal static ж<Δhttp.Request> newTestMultipartRequest(ж<testing.T> t) {
    var b = strings.NewReader(strings.ReplaceAll(message, "\n", "\r\n"));
    var (req, err) = NewRequest(postˢ, "/", new http_test_package.strings_ReaderжReader(b));
    if (err != nil) {
        t.Fatal(newRequestˢ, err);
    }
    @string ctype = fmt.Sprintf(@"multipart/form-data; boundary=""%s"""u8, boundary);
    req.Header.Set(contentTypeˢ2, ctype);
    return req;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string textaˢ => ref textaˢᵇ;
private static readonly @string textaˢᵇ = "texta"u8;
internal static ref readonly @string textbˢ => ref textbˢᵇ;
private static readonly @string textbˢᵇ = "textb"u8;
internal static readonly object isOsFileShouldNotBeˢ = (@string)" is *os.File, should not be"u8;
internal static ref readonly @string fileaˢ => ref fileaˢᵇ;
private static readonly @string fileaˢᵇ = "filea"u8;
internal static ref readonly @string fileaTxtˢ => ref fileaTxtˢᵇ;
private static readonly @string fileaTxtˢᵇ = "filea.txt"u8;
internal static ref readonly @string filebˢ => ref filebˢᵇ;
private static readonly @string filebˢᵇ = "fileb"u8;
internal static ref readonly @string filebTxtˢ => ref filebTxtˢᵇ;
private static readonly @string filebTxtˢᵇ = "fileb.txt"u8;

internal static void validateTestMultipartContents(ж<testing.T> t, ж<Δhttp.Request> req, bool allMem) {
    GoFrame ᒐ = default;
    try {
        {
            @string g = req.FormValue(textaˢ);
            @string e = textaValue; if (g != e) {
                t.Errorf("texta value = %q, want %q", g, e);
            }
        }
        {
            @string g = req.FormValue(textbˢ);
            @string e = textbValue; if (g != e) {
                t.Errorf("textb value = %q, want %q", g, e);
            }
        }
        {
            @string g = req.FormValue(missingˢ); if (g != ""u8) {
                t.Errorf("missing value = %q, want empty string", g);
            }
        }

        void assertMem(@string n, multipart.File fd) {
            {
                var (_, ok) = fd._<ж<os.File>>(ᐧ); if (ok) {
                    t.Error(n, isOsFileShouldNotBeˢ);
                }
            }
        }
        var fda = testMultipartFile(t, req, fileaˢ, fileaTxtˢ, fileaContents);
        var fdaʗ1 = fda;
        defer(() => fdaʗ1.Close(), ref ᒐ);
        assertMem(fileaˢ, fda);
        var fdb = testMultipartFile(t, req, filebˢ, filebTxtˢ, filebContents);
        var fdbʗ1 = fdb;
        defer(() => fdbʗ1.Close(), ref ᒐ);
        if (allMem) {
            assertMem(filebˢ, fdb);
        } else {
            {
                var (_, ok) = fdb._<ж<os.File>>(ᐧ); if (!ok) {
                    t.Errorf("fileb has unexpected underlying type %T", fdb);
                }
            }
        }

        testMissingFile(t, req);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object copyingContentsˢ = (@string)"copying contents:"u8;

internal static multipart.File testMultipartFile(ж<testing.T> t, ж<Δhttp.Request> req, @string key, @string expectFilename, @string expectContent) {
    var (f, fh, err) = req.FormFile(key);
    if (err != nil) {
        t.Fatalf("FormFile(%q): %q", key, err);
    }
    if (fh.Filename != expectFilename) {
        t.Errorf("filename = %q, want %q", fh.Filename, expectFilename);
    }
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    (_, err) = io.Copy(new http_test_package.strings_BuilderжWriter(Ꮡb), f);
    if (err != nil) {
        t.Fatal(copyingContentsˢ, err);
    }
    {
        @string g = b.String(); if (g != expectContent) {
            t.Errorf("contents = %q, want %q", g, expectContent);
        }
    }
    return f;
}

internal partial struct TestRequestCookie_type /*dyn*/ {
    internal @string name;
    internal @string value;
    internal error expectedErr;
}

// Issue 53181: verify Request.Cookie return the correct Cookie.
// Return ErrNoCookie instead of the first cookie when name is "".
public static void TestRequestCookie(ж<testing.T> t) {
    foreach (var (_, tt) in slice<TestRequestCookie_type>([
        new(
            name: "foo",
            value: "bar",
            expectedErr: default
        ),
        new(
            name: "",
            expectedErr: ErrNoCookie
        )
    ])) {
        var (req, err) = NewRequest(getˢ2, httpExampleComˢ2, default);
        if (err != nil) {
            t.Fatal(err);
        }
        req.AddCookie(Ꮡ(new httpꓸCookie(Name: tt.name, Value: tt.value)));
        (var c, err) = req.Cookie(tt.name);
        if (!AreEqual(err, tt.expectedErr)) {
            t.Errorf("got %v, want %v", err, tt.expectedErr);
        }

        // skip if error occurred.
        if (err != nil) {
            continue;
        }
        if ((~c).Value != tt.value) {
            t.Errorf("got %v, want %v", (~c).Value, tt.value);
        }
        if (c.Name != tt.name) {
            t.Errorf("got %s, want %v", tt.name, c.Name);
        }
    }
}

internal partial struct TestRequestCookiesByName_tests /*dyn*/ {
    internal slice<ж<httpꓸCookie>> @in;
    internal @string filter;
    internal slice<ж<httpꓸCookie>> want;
}

public static void TestRequestCookiesByName(ж<testing.T> t) {
    var tests = slice<TestRequestCookiesByName_tests>([
        new(
            @in: slice<ж<httpꓸCookie>>([
                Ꮡ(new httpꓸCookie(Name: "foo", Value: "foo-1")),
                Ꮡ(new httpꓸCookie(Name: "bar", Value: "bar"))
            ]),
            filter: "foo",
            want: slice<ж<httpꓸCookie>>([Ꮡ(new httpꓸCookie(Name: "foo", Value: "foo-1"))])
        ),
        new(
            @in: slice<ж<httpꓸCookie>>([
                Ꮡ(new httpꓸCookie(Name: "foo", Value: "foo-1")),
                Ꮡ(new httpꓸCookie(Name: "foo", Value: "foo-2")),
                Ꮡ(new httpꓸCookie(Name: "bar", Value: "bar"))
            ]),
            filter: "foo",
            want: slice<ж<httpꓸCookie>>([
                Ꮡ(new httpꓸCookie(Name: "foo", Value: "foo-1")),
                Ꮡ(new httpꓸCookie(Name: "foo", Value: "foo-2"))
            ])
        ),
        new(
            @in: slice<ж<httpꓸCookie>>([
                Ꮡ(new httpꓸCookie(Name: "bar", Value: "bar"))
            ]),
            filter: "foo",
            want: slice<ж<httpꓸCookie>>([])
        ),
        new(
            @in: slice<ж<httpꓸCookie>>([
                Ꮡ(new httpꓸCookie(Name: "bar", Value: "bar"))
            ]),
            filter: "",
            want: slice<ж<httpꓸCookie>>([])
        ),
        new(
            @in: slice<ж<httpꓸCookie>>([]),
            filter: "foo",
            want: slice<ж<httpꓸCookie>>([])
        )
    ]);

    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestRequestCookiesByName_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.filter, (ж<testing.T> tΔ1) => {
            var (req, err) = NewRequest(getˢ2, httpExampleComˢ2, default);
            if (err != nil) {
                tΔ1.Fatal(err);
            }
            foreach (var (_, c) in ttʗ1.@in) {
                req.AddCookie(c);
            }

            var got = req.CookiesNamed(ttʗ1.filter);

            if (!reflect.DeepEqual(got, ttʗ1.want)) {
                @string asStr(any v) {
                    var (blob, _) = json.MarshalIndent(v, ""u8, "  ");
                    return (@string)blob;
                }
                tΔ1.Fatalf("Result mismatch\n\tGot: %s\n\tWant: %s", asStr(got), asStr(ttʗ1.want));
            }
        });
    }
}

internal static ref readonly @string fileaContents => ref fileaContentsᵇ;
private static readonly @string fileaContentsᵇ = "This is a test file."u8;
internal static ref readonly @string filebContents => ref filebContentsᵇ;
private static readonly @string filebContentsᵇ = "Another test file."u8;
internal static ref readonly @string textaValue => ref textaValueᵇ;
private static readonly @string textaValueᵇ = "foo"u8;
internal static ref readonly @string textbValue => ref textbValueᵇ;
private static readonly @string textbValueᵇ = "bar"u8;
internal static ref readonly @string boundary => ref boundaryᵇ;
private static readonly @string boundaryᵇ = @"MyBoundary"u8;

internal static ref readonly @string message => ref messageᵇ;
private static readonly @string messageᵇ = "\n--MyBoundary\nContent-Disposition: form-data; name=\"filea\"; filename=\"filea.txt\"\nContent-Type: text/plain\n\nThis is a test file.\n--MyBoundary\nContent-Disposition: form-data; name=\"fileb\"; filename=\"fileb.txt\"\nContent-Type: text/plain\n\nAnother test file.\n--MyBoundary\nContent-Disposition: form-data; name=\"texta\"\n\nfoo\n--MyBoundary\nContent-Disposition: form-data; name=\"textb\"\n\nbar\n--MyBoundary--\n";

internal static void benchmarkReadRequest(ж<testing.B> Ꮡb, @string request) {
    ref var b = ref Ꮡb.DerefOrNull();

    request = request + "\n"u8;                          // final \n
    request = strings.ReplaceAll(request, "\n", "\r\n"); // expand \n to \r\n
    b.SetBytes((int64)len(request));
    var r = bufio.NewReader(new http_test_package.infiniteReaderжReader(Ꮡ(new infiniteReader(buf: slice<byte>(request)))));
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        var (_, err) = ReadRequest(r);
        if (err != nil) {
            Ꮡb.Fatalf("failed to read request: %v", err);
        }
    }
}

// infiniteReader satisfies Read requests as if the contents of buf
// loop indefinitely.
partial struct infiniteReader {
    internal slice<byte> buf;
    internal nint offset;
}

internal static (nint, error) Read(this ref infiniteReader r, slice<byte> b) {
    nint n = copy(b, r.buf.slice(r.offset));
    r.offset = (r.offset + n) % len(r.buf);
    return (n, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string getHttp11HostLocalhostˢ => ref getHttp11HostLocalhostˢᵇ;
private static readonly @string getHttp11HostLocalhostˢᵇ = """
GET / HTTP/1.1
Host: localhost:8080
Connection: keep-alive
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8
User-Agent: Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_2) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.52 Safari/537.17
Accept-Encoding: gzip,deflate,sdch
Accept-Language: en-US,en;q=0.8
Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.3
Cookie: __utma=1.1978842379.1323102373.1323102373.1323102373.1; EPi:NumberOfVisits=1,2012-02-28T13:42:18; CrmSession=5b707226b9563e1bc69084d07a107c98; plushContainerWidth=100%25; plushNoTopMenu=0; hudson_auto_refresh=false

"""u8.Raw();

public static void BenchmarkReadRequestChrome(ж<testing.B> b) {
    // https://github.com/felixge/node-http-perf/blob/master/fixtures/get.http
    benchmarkReadRequest(b, getHttp11HostLocalhostˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string getHttp11UserAgentCurl7ˢ => ref getHttp11UserAgentCurl7ˢᵇ;
private static readonly @string getHttp11UserAgentCurl7ˢᵇ = """
GET / HTTP/1.1
User-Agent: curl/7.27.0
Host: localhost:8080
Accept: */*

"""u8.Raw();

public static void BenchmarkReadRequestCurl(ж<testing.B> b) {
    // curl http://localhost:8080/
    benchmarkReadRequest(b, getHttp11UserAgentCurl7ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string getHttp10HostLocalhostˢ => ref getHttp10HostLocalhostˢᵇ;
private static readonly @string getHttp10HostLocalhostˢᵇ = """
GET / HTTP/1.0
Host: localhost:8080
User-Agent: ApacheBench/2.3
Accept: */*

"""u8.Raw();

public static void BenchmarkReadRequestApachebench(ж<testing.B> b) {
    // ab -n 1 -c 1 http://localhost:8080/
    benchmarkReadRequest(b, getHttp10HostLocalhostˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string getHttp11HostLocalhostˢ2 => ref getHttp11HostLocalhostˢ2ᵇ;
private static readonly @string getHttp11HostLocalhostˢ2ᵇ = """
GET / HTTP/1.1
Host: localhost:8080
Accept: */*
Accept-Encoding: gzip
User-Agent: JoeDog/1.00 [en] (X11; I; Siege 2.70)
Connection: keep-alive

"""u8.Raw();

public static void BenchmarkReadRequestSiege(ж<testing.B> b) {
    // siege -r 1 -c 1 http://localhost:8080/
    benchmarkReadRequest(b, getHttp11HostLocalhostˢ2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string getHttp11HostLocalhostˢ3 => ref getHttp11HostLocalhostˢ3ᵇ;
private static readonly @string getHttp11HostLocalhostˢ3ᵇ = """
GET / HTTP/1.1
Host: localhost:8080

"""u8.Raw();

public static void BenchmarkReadRequestWrk(ж<testing.B> b) {
    // wrk -t 1 -r 1 -c 1 http://localhost:8080/
    benchmarkReadRequest(b, getHttp11HostLocalhostˢ3);
}

public static void BenchmarkFileAndServer_1KB(ж<testing.B> b) {
    benchmarkFileAndServer(b, (int64)1 << 10);
}

public static void BenchmarkFileAndServer_16MB(ж<testing.B> b) {
    benchmarkFileAndServer(b, (int64)1 << 24);
}

public static void BenchmarkFileAndServer_64MB(ж<testing.B> b) {
    benchmarkFileAndServer(b, (int64)1 << 26);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goBenchHttpFileAndServerˢ => ref goBenchHttpFileAndServerˢᵇ;
private static readonly @string goBenchHttpFileAndServerˢᵇ = "go-bench-http-file-and-server"u8;

internal static void benchmarkFileAndServer(ж<testing.B> b, int64 n) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.CreateTemp(os.TempDir(), goBenchHttpFileAndServerˢ);
        if (err != nil) {
            b.Fatalf("Failed to create temp file: %v", err);
        }

        var fʗ1 = f;
        defer(() => {
            fʗ1.Close();
            os.RemoveAll(fʗ1.Name());
        }, ref ᒐ);

        {
            var (_, errΔ1) = io.CopyN(new os.FileжWriter(f), rand.Reader, n); if (errΔ1 != nil) {
                b.Fatalf("Failed to copy %d bytes: %v", n, errΔ1);
            }
        }

        var fʗ2 = f;
        run<BжTBRun>(b, (BжTBRun bΔ1Δp, testMode mode) => {
            var bΔ1 = (ж<testing.B>)bΔ1Δp;
            runFileAndServerBenchmarks(bΔ1, mode, fʗ2, n);
        }, slice<testMode>([http1Mode, https1Mode, http2Mode]));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void runFileAndServerBenchmarks(ж<testing.B> Ꮡb, testMode mode, ж<os.File> f, int64 n) {
    ref var b = ref Ꮡb.DerefOrNull();

    var handler = new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> req) => {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ0 => ᴛ0.Close(), req.Body, ref ᒐ);
            var (nc, err) = io.Copy(io.Discard, req.Body);
            if (err != nil) {
                throw panic(err);
            }

            if (nc != n) {
                throw panic(fmt.Errorf("Copied %d Wanted %d bytes"u8, nc, n));
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });

    var cst = newClientServerTest(new http_test_package.testing_BжTB(Ꮡb), mode, new http_test_package.http_HandlerFuncᴠΔHandler(handler)).ts;

    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        // Perform some setup.
        b.StopTimer();
        {
            var (_, errΔ1) = f.Seek(0, 0); if (errΔ1 != nil) {
                Ꮡb.Fatalf("Failed to seek back to file: %v", errΔ1);
            }
        }

        b.StartTimer();
        var (req, err) = NewRequest(putˢ, cst.URL, io.NopCloser(new http_test_package.os_FileжReader(f)));
        if (err != nil) {
            Ꮡb.Fatal(err);
        }

        req.ContentLength = n;
        // Prevent mime sniffing by setting the Content-Type.
        req.Header.Set(contentTypeˢ, applicationOctetStreamˢ);
        (var res, err) = cst.Client().Do(req);
        if (err != nil) {
            Ꮡb.Fatalf("Failed to make request to backend: %v", err);
        }

        res.Body.Close();
        b.SetBytes(n);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errorsIsErrNotSupportedˢ = (@string)"errors.Is(ErrNotSupported, errors.ErrUnsupported) failed"u8;

public static partial void TestErrNotSupported(ж<testing.T> t) {
    if (!errors.Is(new Δhttp.ProtocolErrorжerror(ErrNotSupported), errors.ErrUnsupported)) {
        t.Error(errorsIsErrNotSupportedˢ);
    }
}

public static void TestPathValueNoMatch(ж<testing.T> t) {
    // Check that PathValue and SetPathValue work on a Request that was never matched.
    Δhttp.Request r = default;
    {
        @string g = r.PathValue("x");
        @string w = ""u8; if (g != w) {
            t.Errorf("got %q, want %q", g, w);
        }
    }
    r.SetPathValue("x", "a");
    {
        @string g = r.PathValue("x");
        @string w = "a"; if (g != w) {
            t.Errorf("got %q, want %q", g, w);
        }
    }
}

internal partial struct TestPathValueAndPattern_type /*dyn*/ {
    internal @string pattern;
    internal @string url;
    internal map<@string, @string> want;
}

public static void TestPathValueAndPattern(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        foreach (var (_, vᴛ1) in slice<TestPathValueAndPattern_type>([
            new(
                "/{a}/is/{b}/{c...}",
                "/now/is/the/time/for/all",
                new map<@string, @string>{
                    ["a"] = "now",
                    ["b"] = "the",
                    ["c"] = "time/for/all",
                    ["d"] = ""
                }
            ),
            new(
                "/names/{name}/{other...}",
                "/names/%2fjohn/address",
                new map<@string, @string>{
                    ["name"] = "/john",
                    ["other"] = "address"
                }
            ),
            new(
                "/names/{name}/{other...}",
                "/names/john%2Fdoe/there/is%2F/more",
                new map<@string, @string>{
                    ["name"] = "john/doe",
                    ["other"] = "there/is//more"
                }
            ),
            new(
                "/names/{name}/{other...}",
                "/names/n/*",
                new map<@string, @string>{
                    ["name"] = "n",
                    ["other"] = "*"
                }
            )
        ])) {
            ref var test = ref heap(new TestPathValueAndPattern_type(), out var Ꮡtest);
            test = vᴛ1;

            var mux = NewServeMux();
            var testʗ1 = test;
            mux.HandleFunc(test.pattern, (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
                foreach (var (name, want) in testʗ1.want) {
                    @string got = r.PathValue(name);
                    if (got != want) {
                        t.Errorf("%q, %q: got %q, want %q", testʗ1.pattern, name, got, want);
                    }
                }
                if (r.Pattern != testʗ1.pattern) {
                    t.Errorf("pattern: got %s, want %s", r.Pattern, testʗ1.pattern);
                }
            });
            var server = httptest.NewServer(new Δhttp.ServeMuxжΔHandler(mux));
            var serverʗ1 = server;
            defer(serverʗ1.Close, ref ᒐ);
            var (res, err) = Get(server.URL + test.url);
            if (err != nil) {
                t.Fatal(err);
            }
            res.Body.Close();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aBCDˢ => ref aBCDˢᵇ;
private static readonly @string aBCDˢᵇ = "/a/{b}/c/{d...}"u8;

public static void TestSetPathValue(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var mux = NewServeMux();
        mux.HandleFunc(aBCDˢ, (Δhttp.ResponseWriter _, ж<Δhttp.Request> r) => {
            var kvs = new map<@string, @string>{
                ["b"] = "X",
                ["d"] = "Y",
                ["a"] = "Z"
            };
            foreach (var (k, v) in kvs) {
                r.SetPathValue(k, v);
            }
            foreach (var (k, w) in kvs) {
                {
                    @string g = r.PathValue(k); if (g != w) {
                        t.Errorf("got %q, want %q", g, w);
                    }
                }
            }
        });
        var server = httptest.NewServer(new Δhttp.ServeMuxжΔHandler(mux));
        var serverʗ1 = server;
        defer(serverʗ1.Close, ref ᒐ);
        var (res, err) = Get(server.URL + "/a/b/c/d/e"u8);
        if (err != nil) {
            t.Fatal(err);
        }
        res.Body.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string getGˢ => ref getGˢᵇ;
private static readonly @string getGˢᵇ = "GET /g"u8;
internal static ref readonly @string postPˢ => ref postPˢᵇ;
private static readonly @string postPˢᵇ = "POST /p"u8;
internal static ref readonly @string patchPˢ => ref patchPˢᵇ;
private static readonly @string patchPˢᵇ = "PATCH /p"u8;
internal static ref readonly @string putRˢ => ref putRˢᵇ;
private static readonly @string putRˢᵇ = "PUT /r"u8;
internal static ref readonly @string getRˢ => ref getRˢᵇ;
private static readonly @string getRˢᵇ = "GET /r/"u8;
internal static ref readonly @string allowˢ => ref allowˢᵇ;
private static readonly @string allowˢᵇ = "Allow"u8;

internal partial struct TestStatus_type /*dyn*/ {
    internal @string method, path;
    internal nint wantStatus;
    internal @string wantAllow;
}

public static void TestStatus(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // The main purpose of this test is to check 405 responses and the Allow header.
        var h = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        });
        var mux = NewServeMux();
        mux.Handle(getGˢ, new http_test_package.http_HandlerFuncᴠΔHandler(h));
        mux.Handle(postPˢ, new http_test_package.http_HandlerFuncᴠΔHandler(h));
        mux.Handle(patchPˢ, new http_test_package.http_HandlerFuncᴠΔHandler(h));
        mux.Handle(putRˢ, new http_test_package.http_HandlerFuncᴠΔHandler(h));
        mux.Handle(getRˢ, new http_test_package.http_HandlerFuncᴠΔHandler(h));
        var server = httptest.NewServer(new Δhttp.ServeMuxжΔHandler(mux));
        var serverʗ1 = server;
        defer(serverʗ1.Close, ref ᒐ);

        foreach (var (_, test) in slice<TestStatus_type>([
            new("GET", "/g", 200, ""),
            new("HEAD", "/g", 200, ""),
            new("POST", "/g", 405, "GET, HEAD"),
            new("GET", "/x", 404, ""),
            new("GET", "/p", 405, "PATCH, POST"),
            new("GET", "/./p", 405, "PATCH, POST"),
            new("GET", "/r/", 200, ""),
            new("GET", "/r", 200, ""), // redirected
            new("HEAD", "/r/", 200, ""),
            new("HEAD", "/r", 200, ""), // redirected
            new("PUT", "/r/", 405, "GET, HEAD"),
            new("PUT", "/r", 200, "")
        ])) {
            var (req, err) = Δhttp.NewRequest(test.method, server.URL + test.path, default);
            if (err != nil) {
                t.Fatal(err);
            }
            (var res, err) = Δhttp.DefaultClient.Do(req);
            if (err != nil) {
                t.Fatal(err);
            }
            res.Body.Close();
            {
                nint g = res.StatusCode;
                nint w = test.wantStatus; if (g != w) {
                    t.Errorf("%s %s: got %d, want %d", test.method, test.path, g, w);
                }
            }
            {
                @string g = res.Header.Get(allowˢ);
                @string w = test.wantAllow; if (g != w) {
                    t.Errorf("%s %s, Allow: got %q, want %q", test.method, test.path, g, w);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end http_test_package
