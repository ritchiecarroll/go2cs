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
using crypto;
using encoding;
using global::go.mime;
using global::go.net;
using global::go.net.http;
using static global::go.net.http_internal_test_package;

partial class http_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpWwwGoogleComSearchQˢ = "http://www.google.com/search?q=foo&q=bar"u8;

public static void TestQuery(ж<testing.T> Ꮡt) {
    var req = Ꮡ(new Request(Method: "GET"u8));
    (req.Value.URL, _) = url.Parse(httpWwwGoogleComSearchQˢ);
    {
        @string q = req.FormValue("q"u8); if (q != "foo"u8) {
            Ꮡt.Errorf(@"req.FormValue(""q"") = %q, want ""foo"""u8, q);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpWwwGoogleComSearchQˢ2 = "http://www.google.com/search?q=foo;q=bar&a=1"u8;

// Issue #25192: Test that ParseForm fails but still parses the form when a URL
// containing a semicolon is provided.
public static void TestParseFormSemicolonSeparator(ж<testing.T> Ꮡt) {
    foreach (var (_, method) in new @string[]{"POST"u8, "PATCH"u8, "PUT"u8, "GET"u8}.slice()) {
        var (req, _) = NewRequest(method, httpWwwGoogleComSearchQˢ2,
            new http_test_package.strings_ReaderжReader(strings.NewReader("q"u8)));
        var err = req.ParseForm();
        if (err == default!) {
            Ꮡt.Fatalf(@"for method %s, ParseForm expected an error, got success"u8, method);
        }
        var wantForm = new url.Values(new map<@string, slice<@string>>{["a"u8] = new @string[]{"1"u8}.slice()});
        if (!reflect.DeepEqual((~req).Form, wantForm)) {
            Ꮡt.Fatalf("for method %s, ParseForm expected req.Form = %v, want %v"u8, method, (~req).Form, wantForm);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpWwwGoogleComSearchQˢ3 = "http://www.google.com/search?q=foo&q=bar&both=x&prio=1&orphan=nope&empty=not"u8;
internal static readonly @string zPostBothYPrio2Nokeyˢ = "z=post&both=y&prio=2&=nokey&orphan&empty=&"u8;
internal static readonly @string applicationXWwwFormˢ2 = "application/x-www-form-urlencoded; param=value"u8;
internal static readonly @string bothˢ = "both"u8;
internal static readonly @string prioˢ = "prio"u8;
internal static readonly @string orphanˢ = "orphan"u8;
internal static readonly @string emptyˢ = "empty"u8;

public static void TestParseFormQuery(ж<testing.T> Ꮡt) {
    var (req, _) = NewRequest(postˢ, httpWwwGoogleComSearchQˢ3,
        new http_test_package.strings_ReaderжReader(strings.NewReader(zPostBothYPrio2Nokeyˢ)));
    (~req).Header.Set(contentTypeˢ, applicationXWwwFormˢ2);
    {
        @string q = req.FormValue("q"u8); if (q != "foo"u8) {
            Ꮡt.Errorf(@"req.FormValue(""q"") = %q, want ""foo"""u8, q);
        }
    }
    {
        @string z = req.FormValue("z"u8); if (z != "post"u8) {
            Ꮡt.Errorf(@"req.FormValue(""z"") = %q, want ""post"""u8, z);
        }
    }
    {
        var (bq, found) = (~req).PostForm["q"u8, ꟷ]; if (found) {
            Ꮡt.Errorf(@"req.PostForm[""q""] = %q, want no entry in map"u8, bq);
        }
    }
    {
        @string bz = req.PostFormValue("z"u8); if (bz != "post"u8) {
            Ꮡt.Errorf(@"req.PostFormValue(""z"") = %q, want ""post"""u8, bz);
        }
    }
    {
        var qs = (~req).Form["q"u8]; if (!slices.Equal<slice<@string>, @string>(qs, new @string[]{"foo"u8, "bar"u8}.slice())) {
            Ꮡt.Errorf(@"req.Form[""q""] = %q, want [""foo"", ""bar""]"u8, qs);
        }
    }
    {
        var both = (~req).Form[bothˢ]; if (!slices.Equal<slice<@string>, @string>(both, new @string[]{"y"u8, "x"u8}.slice())) {
            Ꮡt.Errorf(@"req.Form[""both""] = %q, want [""y"", ""x""]"u8, both);
        }
    }
    {
        @string prio = req.FormValue(prioˢ); if (prio != "2"u8) {
            Ꮡt.Errorf(@"req.FormValue(""prio"") = %q, want ""2"" (from body)"u8, prio);
        }
    }
    {
        var orphan = (~req).Form[orphanˢ]; if (!slices.Equal<slice<@string>, @string>(orphan, new @string[]{""u8, "nope"u8}.slice())) {
            Ꮡt.Errorf(@"req.FormValue(""orphan"") = %q, want """" (from body)"u8, orphan);
        }
    }
    {
        var empty = (~req).Form[emptyˢ]; if (!slices.Equal<slice<@string>, @string>(empty, new @string[]{""u8, "not"u8}.slice())) {
            Ꮡt.Errorf(@"req.FormValue(""empty"") = %q, want """" (from body)"u8, empty);
        }
    }
    {
        var nokey = (~req).Form[""u8]; if (!slices.Equal<slice<@string>, @string>(nokey, new @string[]{"nokey"u8}.slice())) {
            Ꮡt.Errorf(@"req.FormValue(""nokey"") = %q, want ""nokey"" (from body)"u8, nokey);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpWwwGoogleComSearchˢ = "http://www.google.com/search"u8;

// Tests that we only parse the form automatically for certain methods.
public static void TestParseFormQueryMethods(ж<testing.T> Ꮡt) {
    foreach (var (_, method) in new @string[]{"POST"u8, "PATCH"u8, "PUT"u8, "FOO"u8}.slice()) {
        var (req, _) = NewRequest(method, httpWwwGoogleComSearchˢ,
            new http_test_package.strings_ReaderжReader(strings.NewReader(fooBarˢ)));
        (~req).Header.Set(contentTypeˢ, applicationXWwwFormˢ2);
        @string want = barˢ;
        if (method == "FOO"u8) {
            want = ""u8;
        }
        {
            @string got = req.FormValue(fooˢ); if (got != want) {
                Ꮡt.Errorf(@"for method %s, FormValue(""foo"") = %q; want %q"u8, method, got, want);
            }
        }
    }
}

[GoType("dyn")] internal partial struct TestParseFormUnknownContentType_type {
    internal @string name;
    internal @string wantErr;
    internal httpꓸHeader contentType;
}

public static void TestParseFormUnknownContentType(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in new TestParseFormUnknownContentType_type[]{
        new("text"u8, ""u8, new httpꓸHeader(new map<@string, slice<@string>>{["Content-Type"u8] = new @string[]{"text/plain"u8}.slice()})), // Empty content type is legal - may be treated as
 // application/octet-stream (RFC 7231, section 3.1.1.5)

        new("empty"u8, ""u8, new httpꓸHeader(new map<@string, slice<@string>>{})),
        new("boundary"u8, "mime: invalid media parameter"u8, new httpꓸHeader(new map<@string, slice<@string>>{["Content-Type"u8] = new @string[]{"text/plain; boundary="u8}.slice()})),
        new("unknown"u8, ""u8, new httpꓸHeader(new map<@string, slice<@string>>{["Content-Type"u8] = new @string[]{"application/unknown"u8}.slice()}))
    }.slice()) {
        ref var test = ref heap(new TestParseFormUnknownContentType_type(), out var Ꮡtest);
        test = vᴛ1;

            var testʗ1 = test;
        Ꮡt.Run(test.name,
            (ж<testing.T> tΔ1) => {
                var req = Ꮡ(new Request(
                    Method: "POST"u8,
                    Header: testʗ1.contentType,
                    Body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(bodyˢ)))
                ));
                var err = req.ParseForm();
                switch (ᐧ) {
                case {} when err == default! && testʗ1.wantErr != ""u8: {
                    tΔ1.Errorf("unexpected success; want error %q"u8, testʗ1.wantErr);
                    break;
                }
                case {} when err != default! && testʗ1.wantErr == ""u8: {
                    tΔ1.Errorf("want success, got error: %v"u8, err);
                    break;
                }
                case {} when testʗ1.wantErr != ""u8 && testʗ1.wantErr != fmt.Sprint(err): {
                    tΔ1.Errorf("got error %q; want %q"u8, err, testʗ1.wantErr);
                    break;
                }}

            });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpWwwGoogleComSearchQˢ4 = "http://www.google.com/search?q=foo"u8;

public static void TestParseFormInitializeOnError(ж<testing.T> Ꮡt) {
    var (nilBody, _) = NewRequest(postˢ, httpWwwGoogleComSearchQˢ4, default!);
    var tests = new ж<Δhttp.Request>[]{
        nilBody,
        Ꮡ(new Δhttp.Request(Method: "GET"u8, URL: nil))
    }.slice();
    foreach (var (i, req) in tests) {
        var err = req.ParseForm();
        if ((~req).Form == default!) {
            Ꮡt.Errorf("%d. Form not initialized, error %v"u8, i, err);
        }
        if ((~req).PostForm == default!) {
            Ꮡt.Errorf("%d. PostForm not initialized, error %v"u8, i, err);
        }
    }
}

[GoType("dyn")] internal partial struct TestMultipartReader_tests {
    internal bool shouldError;
    internal @string contentType;
}

public static void TestMultipartReader(ж<testing.T> Ꮡt) {
    var tests = new TestMultipartReader_tests[]{
        new(false, @"multipart/form-data; boundary=""foo123"""u8),
        new(false, @"multipart/mixed; boundary=""foo123"""u8),
        new(true, @"text/plain"u8)
    }.slice();
    foreach (var (i, test) in tests) {
        var req = Ꮡ(new Request(
            Method: "POST"u8,
            Header: new httpꓸHeader(new map<@string, slice<@string>>{["Content-Type"u8] = new @string[]{test.contentType}.slice()}),
            Body: io.NopCloser(new http_test_package.bytes_BufferжReader(@new<bytes.Buffer>()))
        ));
        var (multipart, err) = req.MultipartReader();
        if (test.shouldError) {
            if (err == default! || multipart != nil) {
                Ꮡt.Errorf("test %d: unexpectedly got nil-error (%v) or non-nil-multipart (%v)"u8, i, err, multipart.OrTypedNil());
            }
            continue;
        }
        if (err != default! || multipart == nil) {
            Ꮡt.Errorf("test %d: unexpectedly got error (%v) or nil-multipart (%v)"u8, i, err, multipart.OrTypedNil());
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xxxContentDispositionˢ = """
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

"""u8;

// Issue 9305: ParseMultipartForm should populate PostForm too
public static void TestParseMultipartFormPopulatesPostForm(ж<testing.T> Ꮡt) {
    @string postData = xxxContentDispositionˢ;
    var req = Ꮡ(new Request(
        Method: "POST"u8,
        Header: new httpꓸHeader(new map<@string, slice<@string>>{["Content-Type"u8] = new @string[]{@"multipart/form-data; boundary=xxx"u8}.slice()}),
        Body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(postData)))
    ));
    var initialFormItems = new map<@string, @string>{
        ["language"u8] = "Go"u8,
        ["name"u8] = "gopher"u8,
        ["skill"u8] = "go-ing"u8,
        ["field2"u8] = "initial-value2"u8
    };
    req.Value.Form = new url.Values(0);
    foreach (var (k, v) in initialFormItems) {
        (~req).Form.Add(k, v);
    }
    var err = req.ParseMultipartForm(10000);
    if (err != default!) {
        Ꮡt.Fatalf("unexpected multipart error %v"u8, err);
    }
    var wantForm = new url.Values(new map<@string, slice<@string>>{
        ["language"u8] = new @string[]{"Go"u8}.slice(),
        ["name"u8] = new @string[]{"gopher"u8}.slice(),
        ["skill"u8] = new @string[]{"go-ing"u8}.slice(),
        ["field1"u8] = new @string[]{"value1"u8}.slice(),
        ["field2"u8] = new @string[]{"initial-value2"u8, "value2"u8}.slice()
    });
    if (!reflect.DeepEqual((~req).Form, wantForm)) {
        Ꮡt.Fatalf("req.Form = %v, want %v"u8, (~req).Form, wantForm);
    }
    var wantPostForm = new url.Values(new map<@string, slice<@string>>{
        ["field1"u8] = new @string[]{"value1"u8}.slice(),
        ["field2"u8] = new @string[]{"value2"u8}.slice()
    });
    if (!reflect.DeepEqual((~req).PostForm, wantPostForm)) {
        Ꮡt.Fatalf("req.PostForm = %v, want %v"u8, (~req).PostForm, wantPostForm);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedMultipartEofGotˢ = (@string)"expected multipart EOF, got nil"u8;
internal static readonly object expectedErrNotMultipartˢ = (@string)"expected ErrNotMultipart for text/plain"u8;

public static void TestParseMultipartForm(ж<testing.T> Ꮡt) {
    var req = Ꮡ(new Request(
        Method: "POST"u8,
        Header: new httpꓸHeader(new map<@string, slice<@string>>{["Content-Type"u8] = new @string[]{@"multipart/form-data; boundary=""foo123"""u8}.slice()}),
        Body: io.NopCloser(new http_test_package.bytes_BufferжReader(@new<bytes.Buffer>()))
    ));
    var err = req.ParseMultipartForm(25);
    if (err == default!) {
        Ꮡt.Error(expectedMultipartEofGotˢ);
    }
    req.Value.Header = new httpꓸHeader(new map<@string, slice<@string>>{["Content-Type"u8] = new @string[]{"text/plain"u8}.slice()});
    err = req.ParseMultipartForm(25);
    if (!AreEqual(err, ErrNotMultipart)) {
        Ꮡt.Error(expectedErrNotMultipartˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xxxContentDispositionˢ2 = """
--xxx
Content-Disposition: form-data; name="file"; filename="../usr/foobar.txt/"
Content-Type: text/plain

--xxx--

"""u8;
internal static readonly @string fileˢ = "file"u8;

// Issue 45789: multipart form should not include directory path in filename
public static void TestParseMultipartFormFilename(ж<testing.T> Ꮡt) {
    @string postData = xxxContentDispositionˢ2;
    var req = Ꮡ(new Request(
        Method: "POST"u8,
        Header: new httpꓸHeader(new map<@string, slice<@string>>{["Content-Type"u8] = new @string[]{@"multipart/form-data; boundary=xxx"u8}.slice()}),
        Body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(postData)))
    ));
    var (_, hdr, err) = req.FormFile(fileˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if ((~hdr).Filename != "foobar.txt"u8) {
        Ꮡt.Errorf("expected only the last element of the path, got %q"u8, (~hdr).Filename);
    }
}

// Issue #40430: Test that if maxMemory for ParseMultipartForm when combined with
// the payload size and the internal leeway buffer size of 10MiB overflows, that we
// correctly return an error.
public static void TestMaxInt64ForMultipartFormMaxMemoryOverflow(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testMaxInt64ForMultipartFormMaxMemoryOverflow(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string myfileTxtˢ = "myfile.txt"u8;

internal static void testMaxInt64ForMultipartFormMaxMemoryOverflow(ж<testing.T> Ꮡt, testMode mode) {
    nint payloadSize = (1 << (int)(10));
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> reqΔ1) => {
        // The combination of:
        //      MaxInt64 + payloadSize + (internal spare of 10MiB)
        // triggers the overflow. See issue https://golang.org/issue/40430/
        {
            var errΔ1 = reqΔ1.ParseMultipartForm(math.MaxInt64); if (errΔ1 != default!) {
                Error(rw, errΔ1.Error(), StatusBadRequest);
                return;
            }
        }
    }))).Value.ts;
    var fBuf = @new<bytes.Buffer>();
    var mw = multipart.NewWriter(new http_test_package.bytes_BufferжWriter(fBuf));
    var (mf, err) = mw.CreateFormFile(fileˢ, myfileTxtˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var (_, errΔ2) = mf.Write(bytes.Repeat(slice<byte>("abc"u8), payloadSize)); if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
    }
    {
        var errΔ3 = mw.Close(); if (errΔ3 != default!) {
            Ꮡt.Fatal(errΔ3);
        }
    }
    (var req, err) = NewRequest(postˢ, (~cst).URL, new http_test_package.bytes_BufferжReader(fBuf));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~req).Header.Set(contentTypeˢ, mw.FormDataContentType());
    (var res, err) = cst.Client().Do(req);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
    {
        nint g = res.Value.StatusCode;
        nint w = StatusOK; if (g != w) {
            Ꮡt.Fatalf("Status code mismatch: got %d, want %d"u8, g, w);
        }
    }
}

public static void TestRequestRedirect(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testRequestRedirect(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ5 = "/foo/"u8;
internal static readonly @string fooˢ6 = "/foo/$"u8;

internal static void testRequestRedirect(ж<testing.T> Ꮡt, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> rΔ1) => {
        var exprᴛ1 = (~(~rΔ1).URL).Path;
        if (exprᴛ1 == "/"u8) {
            w.Header().Set(locationˢ, fooˢ5);
            w.WriteHeader(StatusSeeOther);
        }
        else if (exprᴛ1 == "/foo/"u8) {
            fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "foo"u8);
        }
        else { /* default: */
            w.WriteHeader(StatusBadRequest);
        }

    })));
    ж<regexp.Regexp> end = regexp.MustCompile(fooˢ6);
    var (r, err) = (~cst).c.Get((~(~cst).ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~r).Body.Close();
    @string url = (~(~r).Request).URL.String();
    if ((~r).StatusCode != 200 || !end.MatchString(url)) {
        Ꮡt.Fatalf("Get got status %d at %q, want 200 matching /foo/$"u8, (~r).StatusCode, url);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpExampleComˢ2 = "http://example.com/"u8;
internal static readonly @string aladdinˢ = "Aladdin"u8;
internal static readonly @string openSesameˢ = "open sesame"u8;
internal static readonly @string basicˢ2 = "Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ=="u8;

public static void TestSetBasicAuth(ж<testing.T> Ꮡt) {
    var (r, _) = NewRequest(getˢ2, httpExampleComˢ2, default!);
    r.SetBasicAuth(aladdinˢ, openSesameˢ);
    {
        @string g = (~r).Header.Get(authorizationˢ);
        @string e = basicˢ2; if (g != e) {
            Ꮡt.Errorf("got header %q, want %q"u8, g, e);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object parseMultipartFormFirstˢ = (@string)"ParseMultipartForm first call:"u8;
internal static readonly object parseMultipartFormSecondˢ = (@string)"ParseMultipartForm second call:"u8;

public static void TestMultipartRequest(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Test that we can read the values and files of a
        // multipart request with FormValue and FormFile,
        // and that ParseMultipartForm can be called multiple times.
        var req = newTestMultipartRequest(Ꮡt);
        {
            var err = req.ParseMultipartForm(25); if (err != default!) {
                Ꮡt.Fatal(parseMultipartFormFirstˢ, err);
            }
        }
        var reqʗ1 = req;
        defer(() => (~reqʗ1).MultipartForm.RemoveAll(), ref ᒐ);
        validateTestMultipartContents(Ꮡt, req, false);
        {
            var err = req.ParseMultipartForm(25); if (err != default!) {
                Ꮡt.Fatal(parseMultipartFormSecondˢ, err);
            }
        }
        validateTestMultipartContents(Ꮡt, req, false);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object parseMultipartFormˢ = (@string)"ParseMultipartForm expected error due to invalid semicolon, got nil"u8;

// Issue #25192: Test that ParseMultipartForm fails but still parses the
// multi-part form when a URL containing a semicolon is provided.
public static void TestParseMultipartFormSemicolonSeparator(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var req = newTestMultipartRequest(Ꮡt);
        req.Value.URL = Ꮡ(new url.URL(RawQuery: "q=foo;q=bar"u8));
        {
            var err = req.ParseMultipartForm(25); if (err == default!) {
                Ꮡt.Fatal(parseMultipartFormˢ);
            }
        }
        var reqʗ1 = req;
        defer(() => (~reqʗ1).MultipartForm.RemoveAll(), ref ᒐ);
        validateTestMultipartContents(Ꮡt, req, false);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestMultipartRequestAuto(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Test that FormValue and FormFile automatically invoke
        // ParseMultipartForm and return the right values.
        var req = newTestMultipartRequest(Ꮡt);
        var reqʗ1 = req;
        defer(() => {
            if ((~reqʗ1).MultipartForm != nil) {
                (~reqʗ1).MultipartForm.RemoveAll();
            }
        }, ref ᒐ);
        validateTestMultipartContents(Ꮡt, req, true);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestMissingFileMultipartRequest(ж<testing.T> Ꮡt) {
    // Test that FormFile returns an error if
    // the named file is missing.
    var req = newTestMultipartRequest(Ꮡt);
    testMissingFile(Ꮡt, req);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpWwwGoogleComˢ = "http://www.google.com/"u8;
internal static readonly @string zPostˢ = "z=post"u8;
internal static readonly object unexpectedRequestFormˢ = (@string)"Unexpected request Form, want nil"u8;
internal static readonly object parseMultipartFormNotˢ = (@string)"ParseMultipartForm not called by FormValue"u8;

// Test that FormValue invokes ParseMultipartForm.
public static void TestFormValueCallsParseMultipartForm(ж<testing.T> Ꮡt) {
    var (req, _) = NewRequest(postˢ, httpWwwGoogleComˢ, new http_test_package.strings_ReaderжReader(strings.NewReader(zPostˢ)));
    (~req).Header.Set(contentTypeˢ, applicationXWwwFormˢ2);
    if ((~req).Form != default!) {
        Ꮡt.Fatal(unexpectedRequestFormˢ);
    }
    req.FormValue("z"u8);
    if ((~req).Form == default!) {
        Ꮡt.Fatal(parseMultipartFormNotˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object parseMultipartFormNotˢ2 = (@string)"ParseMultipartForm not called by FormFile"u8;

// Test that FormFile invokes ParseMultipartForm.
public static void TestFormFileCallsParseMultipartForm(ж<testing.T> Ꮡt) {
    var req = newTestMultipartRequest(Ꮡt);
    if ((~req).Form != default!) {
        Ꮡt.Fatal(unexpectedRequestFormˢ);
    }
    req.FormFile(""u8);
    if ((~req).Form == default!) {
        Ꮡt.Fatal(parseMultipartFormNotˢ2);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedAnErrorFromˢ = (@string)"expected an error from ParseMultipartForm after call to MultipartReader"u8;

// Test that ParseMultipartForm errors if called
// after MultipartReader on the same request.
public static void TestParseMultipartFormOrder(ж<testing.T> Ꮡt) {
    var req = newTestMultipartRequest(Ꮡt);
    {
        var (_, err) = req.MultipartReader(); if (err != default!) {
            Ꮡt.Fatalf("MultipartReader: %v"u8, err);
        }
    }
    {
        var err = req.ParseMultipartForm(1024); if (err == default!) {
            Ꮡt.Fatal(expectedAnErrorFromˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedAnErrorFromˢ2 = (@string)"expected an error from MultipartReader after call to ParseMultipartForm"u8;

// Test that MultipartReader errors if called
// after ParseMultipartForm on the same request.
public static void TestMultipartReaderOrder(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var req = newTestMultipartRequest(Ꮡt);
        {
            var err = req.ParseMultipartForm(25); if (err != default!) {
                Ꮡt.Fatalf("ParseMultipartForm: %v"u8, err);
            }
        }
        var reqʗ1 = req;
        defer(() => (~reqʗ1).MultipartForm.RemoveAll(), ref ᒐ);
        {
            var (_, err) = req.MultipartReader(); if (err == default!) {
                Ꮡt.Fatal(expectedAnErrorFromˢ2);
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
public static void TestFormFileOrder(ж<testing.T> Ꮡt) {
    var req = newTestMultipartRequest(Ꮡt);
    {
        var (_, err) = req.MultipartReader(); if (err != default!) {
            Ꮡt.Fatalf("MultipartReader: %v"u8, err);
        }
    }
    {
        var (_, _, err) = req.FormFile(""u8); if (err == default!) {
            Ꮡt.Fatal(expectedAnErrorFromˢ3);
        }
    }
}

// Multiple Content-Length values should either be
// deduplicated if same or reject otherwise
// See Issue 16490.

[GoType("dyn")] partial struct readRequestErrorTestsᴛ1 {
    internal @string @in;
    internal @string err;
    internal httpꓸHeader header;
}
internal static slice<readRequestErrorTestsᴛ1> readRequestErrorTests = new slice<readRequestErrorTestsᴛ1>(11){
    [0] = new("GET / HTTP/1.1\r\nheader:foo\r\n\r\n"u8, ""u8, new httpꓸHeader(new map<@string, slice<@string>>{["Header"u8] = new @string[]{"foo"u8}.slice()})),
    [1] = new("GET / HTTP/1.1\r\nheader:foo\r\n"u8, io.ErrUnexpectedEOF.Error(), default!),
    [2] = new(""u8, io.EOF.Error(), default!),
    [3] = new(
        @in: "HEAD / HTTP/1.1\r\n\r\n"u8,
        header: new httpꓸHeader(new map<@string, slice<@string>>{})
    ),
    [4] = new(
        @in: "POST / HTTP/1.1\r\nContent-Length: 10\r\nContent-Length: 0\r\n\r\nGopher hey\r\n"u8,
        err: "cannot contain multiple Content-Length headers"u8
    ),
    [5] = new(
        @in: "POST / HTTP/1.1\r\nContent-Length: 10\r\nContent-Length: 6\r\n\r\nGopher\r\n"u8,
        err: "cannot contain multiple Content-Length headers"u8
    ),
    [6] = new(
        @in: "PUT / HTTP/1.1\r\nContent-Length: 6 \r\nContent-Length: 6\r\nContent-Length:6\r\n\r\nGopher\r\n"u8,
        err: ""u8,
        header: new httpꓸHeader(new map<@string, slice<@string>>{["Content-Length"u8] = new @string[]{"6"u8}.slice()})
    ),
    [7] = new(
        @in: "PUT / HTTP/1.1\r\nContent-Length: 1\r\nContent-Length: 6 \r\n\r\n"u8,
        err: "cannot contain multiple Content-Length headers"u8
    ),
    [8] = new(
        @in: "POST / HTTP/1.1\r\nContent-Length:\r\nContent-Length: 3\r\n\r\n"u8,
        err: "cannot contain multiple Content-Length headers"u8
    ),
    [9] = new(
        @in: "HEAD / HTTP/1.1\r\nContent-Length:0\r\nContent-Length: 0\r\n\r\n"u8,
        header: new httpꓸHeader(new map<@string, slice<@string>>{["Content-Length"u8] = new @string[]{"0"u8}.slice()})
    ),
    [10] = new(
        @in: "HEAD / HTTP/1.1\r\nHost: foo\r\nHost: bar\r\n\r\n\r\n\r\n"u8,
        err: "too many Host headers"u8
    )
};

public static void TestReadRequestErrors(ж<testing.T> Ꮡt) {
    foreach (var (i, tt) in readRequestErrorTests) {
        var (req, err) = ReadRequest(bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader(tt.@in))));
        if (err == default!) {
            if (tt.err != ""u8) {
                Ꮡt.Errorf("#%d: got nil err; want %q"u8, i, tt.err);
            }
            if (!reflect.DeepEqual(tt.header, (~req).Header)) {
                Ꮡt.Errorf("#%d: gotHeader: %q wantHeader: %q"u8, i, (~req).Header, tt.header);
            }
            continue;
        }
        if (tt.err == ""u8 || !strings.Contains(err.Error(), tt.err)) {
            Ꮡt.Errorf("%d: got error = %v; want %v"u8, i, err, tt.err);
        }
    }
}


[GoType("dyn")] partial struct newRequestHostTestsᴛ1 {
    internal @string @in, @out;
}
internal static slice<newRequestHostTestsᴛ1> newRequestHostTests = new newRequestHostTestsᴛ1[]{
    new("http://www.example.com/"u8, "www.example.com"u8),
    new("http://www.example.com:8080/"u8, "www.example.com:8080"u8),
    new("http://192.168.0.1/"u8, "192.168.0.1"u8),
    new("http://192.168.0.1:8080/"u8, "192.168.0.1:8080"u8),
    new("http://192.168.0.1:/"u8, "192.168.0.1"u8),
    new("http://[fe80::1]/"u8, "[fe80::1]"u8),
    new("http://[fe80::1]:8080/"u8, "[fe80::1]:8080"u8),
    new("http://[fe80::1%25en0]/"u8, "[fe80::1%en0]"u8),
    new("http://[fe80::1%25en0]:8080/"u8, "[fe80::1%en0]:8080"u8),
    new("http://[fe80::1%25en0]:/"u8, "[fe80::1%en0]"u8)
}.slice();

public static void TestNewRequestHost(ж<testing.T> Ꮡt) {
    foreach (var (i, tt) in newRequestHostTests) {
        var (req, err) = NewRequest(getˢ2, tt.@in, default!);
        if (err != default!) {
            Ꮡt.Errorf("#%v: %v"u8, i, err);
            continue;
        }
        if ((~req).Host != tt.@out) {
            Ꮡt.Errorf("got %q; want %q"u8, (~req).Host, tt.@out);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badMethodˢ = "bad method"u8;
internal static readonly @string httpFooComˢ = "http://foo.com/"u8;
internal static readonly object expectedErrorFromˢ = (@string)"expected error from NewRequest with invalid method"u8;
internal static readonly @string httpFooExampleˢ = "http://foo.example/"u8;
internal static readonly @string invalidMethodˢ = "invalid method"u8;

public static void TestRequestInvalidMethod(ж<testing.T> Ꮡt) {
    var (_, err) = NewRequest(badMethodˢ, httpFooComˢ, default!);
    if (err == default!) {
        Ꮡt.Error(expectedErrorFromˢ);
    }
    (var req, err) = NewRequest(getˢ2, httpFooExampleˢ, default!);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    req.Value.Method = badMethodˢ;
    (_, err) = DefaultClient.Do(req);
    if (err == default! || !strings.Contains(err.Error(), invalidMethodˢ)) {
        Ꮡt.Errorf("Transport error = %v; want invalid method"u8, err);
    }
    (req, err) = NewRequest(""u8, httpFooComˢ, default!);
    if (err != default!){
        Ꮡt.Errorf("NewRequest(empty method) = %v; want nil"u8, err);
    } else 
    if ((~req).Method != "GET"u8) {
        Ꮡt.Errorf("NewRequest(empty method) has method %q; want GET"u8, (~req).Method);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xyzˢ = "xyz"u8;
internal static readonly @string httpLocalhostˢ = "http://localhost/"u8;

[GoType("dyn")] internal partial struct TestNewRequestContentLength_tests {
    internal io.Reader r;
    internal int64 want;
}

[GoType("dyn")] internal partial struct TestNewRequestContentLength_type {
    [GoEmbedded] public io_package.Reader Reader;
}

public static void TestNewRequestContentLength(ж<testing.T> Ꮡt) {
    io.Reader readByte(io.Reader r) {
        array<byte> b = new(1);
        r.Read(b[..]);
        return r;
    }
    var tests = new TestNewRequestContentLength_tests[]{
        new(new http_test_package.bytes_ReaderжReader(bytes.NewReader(slice<byte>("123"u8))), 3),
        new(new http_test_package.bytes_BufferжReader(bytes.NewBuffer(slice<byte>("1234"u8))), 4),
        new(new http_test_package.strings_ReaderжReader(strings.NewReader("12345"u8)), 5),
        new(new http_test_package.strings_ReaderжReader(strings.NewReader(""u8)), 0),
        new(new http_test_package.http_noBodyᴠReader(NoBody), 0), // Not detected. During Go 1.8 we tried to make these set to -1, but
 // due to Issue 18117, we keep these returning 0, even though they're
 // unknown.

        new(new TestNewRequestContentLength_type(new http_test_package.strings_ReaderжReader(strings.NewReader(xyzˢ))), 0),
        new(new io.SectionReaderжReader(io.NewSectionReader(new http_test_package.strings_ReaderжReaderAt(strings.NewReader("x"u8)), 0, 6)), 0),
        new(readByte(new io.SectionReaderжReader(io.NewSectionReader(new http_test_package.strings_ReaderжReaderAt(strings.NewReader("xy"u8)), 0, 6))), 0)
    }.slice();
    foreach (var (i, tt) in tests) {
        var (req, err) = NewRequest(postˢ, httpLocalhostˢ, tt.r);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if ((~req).ContentLength != tt.want) {
            Ꮡt.Errorf("test[%d]: ContentLength(%T) = %d; want %d"u8, i, tt.r, (~req).ContentLength, tt.want);
        }
    }
}


[GoType("dyn")] partial struct parseHTTPVersionTestsᴛ1 {
    internal @string vers;
    internal nint major, minor;
    internal bool ok;
}
internal static slice<parseHTTPVersionTestsᴛ1> parseHTTPVersionTests = new parseHTTPVersionTestsᴛ1[]{
    new("HTTP/0.0"u8, 0, 0, true),
    new("HTTP/0.9"u8, 0, 9, true),
    new("HTTP/1.0"u8, 1, 0, true),
    new("HTTP/1.1"u8, 1, 1, true),
    new("HTTP"u8, 0, 0, false),
    new("HTTP/one.one"u8, 0, 0, false),
    new("HTTP/1.1/"u8, 0, 0, false),
    new("HTTP/-1,0"u8, 0, 0, false),
    new("HTTP/0,-1"u8, 0, 0, false),
    new("HTTP/"u8, 0, 0, false),
    new("HTTP/1,1"u8, 0, 0, false),
    new("HTTP/+1.1"u8, 0, 0, false),
    new("HTTP/1.+1"u8, 0, 0, false),
    new("HTTP/0000000001.1"u8, 0, 0, false),
    new("HTTP/1.0000000001"u8, 0, 0, false),
    new("HTTP/3.14"u8, 0, 0, false),
    new("HTTP/12.3"u8, 0, 0, false)
}.slice();

[GoType("dyn")] internal partial struct TestParseHTTPVersion_version {
    internal nint major, minor;
    internal bool ok;
}

public static void TestParseHTTPVersion(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in parseHTTPVersionTests) {
        var (major, minor, ok) = ParseHTTPVersion(tt.vers);
        if (ok != tt.ok || major != tt.major || minor != tt.minor) {
            Ꮡt.Errorf("failed to parse %q, expected: %#v, got %#v"u8, tt.vers, new TestParseHTTPVersion_version(tt.major, tt.minor, tt.ok), new TestParseHTTPVersion_version(major, minor, ok));
        }
    }
}

[GoType] partial struct getBasicAuthTest {
    internal @string username, password;
    internal bool ok;
}

[GoType] partial struct basicAuthCredentialsTest {
    internal @string username, password;
}


[GoType("dyn")] partial struct getBasicAuthTestsᴛ1 {
    internal @string username, password;
    internal bool ok;
}
internal static slice<getBasicAuthTestsᴛ1> getBasicAuthTests = new getBasicAuthTestsᴛ1[]{
    new("Aladdin"u8, "open sesame"u8, true),
    new("Aladdin"u8, "open:sesame"u8, true),
    new(""u8, ""u8, true)
}.slice();

public static void TestGetBasicAuth(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in getBasicAuthTests) {
        var (rΔ1, _) = NewRequest(getˢ2, httpExampleComˢ2, default!);
        rΔ1.SetBasicAuth(tt.username, tt.password);
        var (usernameΔ1, passwordΔ1, okΔ1) = rΔ1.BasicAuth();
        if (okΔ1 != tt.ok || usernameΔ1 != tt.username || passwordΔ1 != tt.password) {
            Ꮡt.Errorf("BasicAuth() = %#v, want %#v"u8, new getBasicAuthTest(usernameΔ1, passwordΔ1, okΔ1),
                new getBasicAuthTest(tt.username, tt.password, tt.ok));
        }
    }
    // Unauthenticated request.
    var (r, _) = NewRequest(getˢ2, httpExampleComˢ2, default!);
    var (username, password, ok) = r.BasicAuth();
    if (ok) {
        Ꮡt.Errorf("expected false from BasicAuth when the request is unauthenticated"u8);
    }
    var want = new basicAuthCredentialsTest(""u8, ""u8);
    if (username != want.username || password != want.password) {
        Ꮡt.Errorf("expected credentials: %#v when the request is unauthenticated, got %#v"u8,
            want, new basicAuthCredentialsTest(username, password));
    }
}

// Case doesn't matter:

[GoType("dyn")] partial struct parseBasicAuthTestsᴛ1 {
    internal @string header, username, password;
    internal bool ok;
}
internal static slice<parseBasicAuthTestsᴛ1> parseBasicAuthTests = new parseBasicAuthTestsᴛ1[]{
    new("Basic "u8 + base64.StdEncoding.EncodeToString(slice<byte>("Aladdin:open sesame"u8)), "Aladdin"u8, "open sesame"u8, true),
    new("BASIC "u8 + base64.StdEncoding.EncodeToString(slice<byte>("Aladdin:open sesame"u8)), "Aladdin"u8, "open sesame"u8, true),
    new("basic "u8 + base64.StdEncoding.EncodeToString(slice<byte>("Aladdin:open sesame"u8)), "Aladdin"u8, "open sesame"u8, true),
    new("Basic "u8 + base64.StdEncoding.EncodeToString(slice<byte>("Aladdin:open:sesame"u8)), "Aladdin"u8, "open:sesame"u8, true),
    new("Basic "u8 + base64.StdEncoding.EncodeToString(slice<byte>(":"u8)), ""u8, ""u8, true),
    new("Basic"u8 + base64.StdEncoding.EncodeToString(slice<byte>("Aladdin:open sesame"u8)), ""u8, ""u8, false),
    new(base64.StdEncoding.EncodeToString(slice<byte>("Aladdin:open sesame"u8)), ""u8, ""u8, false),
    new("Basic "u8, ""u8, ""u8, false),
    new("Basic Aladdin:open sesame"u8, ""u8, ""u8, false),
    new(@"Digest username=""Aladdin"""u8, ""u8, ""u8, false)
}.slice();

public static void TestParseBasicAuth(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in parseBasicAuthTests) {
        var (r, _) = NewRequest(getˢ2, httpExampleComˢ2, default!);
        (~r).Header.Set(authorizationˢ, tt.header);
        var (username, password, ok) = r.BasicAuth();
        if (ok != tt.ok || username != tt.username || password != tt.password) {
            Ꮡt.Errorf("BasicAuth() = %#v, want %#v"u8, new getBasicAuthTest(username, password, ok),
                new getBasicAuthTest(tt.username, tt.password, tt.ok));
        }
    }
}

[GoType] partial struct logWrites {
    internal ж<testing.T> t;
    internal ж<slice<@string>> dst;
}

internal static error WriteByte(this logWrites l, byte c) {
    l.t.Fatalf("unexpected WriteByte call"u8);
    return default!;
}

internal static (nint n, error err) Write(this logWrites l, slice<byte> p) {
    l.dst.ValueSlot = append(l.dst.ValueSlot, ((@string)p));
    return (len(p), default!);
}

public static void TestRequestWriteBufferedWriter(ж<testing.T> Ꮡt) {
    ref var got = ref heap<slice<@string>>(out var Ꮡgot);
    got = new @string[]{}.slice();
    var (req, _) = NewRequest(getˢ2, httpFooComˢ, default!);
    req.Write(new logWrites(Ꮡt, Ꮡgot));
    var want = new @string[]{
        "GET / HTTP/1.1\r\n"u8,
        "Host: foo.com\r\n"u8,
        "User-Agent: "u8 + http_internal_test_package.DefaultUserAgent + "\r\n"u8,
        "\r\n"u8
    }.slice();
    if (!slices.Equal<slice<@string>, @string>(got, want)) {
        Ꮡt.Errorf("Writes = %q\n  Want = %q"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpFooAfterˢ = "http://foo/after"u8;
internal static readonly @string fooComNewlineˢ = "foo.com\nnewline"u8;

public static void TestRequestBadHostHeader(ж<testing.T> Ꮡt) {
    ref var got = ref heap<slice<@string>>(out var Ꮡgot);
    got = new @string[]{}.slice();
    var (req, err) = NewRequest(getˢ2, httpFooAfterˢ, default!);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    req.Value.Host = fooComNewlineˢ;
    req.Value.URL.Value.Host = fooComNewlineˢ;
    req.Write(new logWrites(Ꮡt, Ꮡgot));
    var want = new @string[]{
        "GET /after HTTP/1.1\r\n"u8,
        "Host: \r\n"u8,
        "User-Agent: "u8 + http_internal_test_package.DefaultUserAgent + "\r\n"u8,
        "\r\n"u8
    }.slice();
    if (!slices.Equal<slice<@string>, @string>(got, want)) {
        Ꮡt.Errorf("Writes = %q\n  Want = %q"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string evilXEvilEvilˢ = "evil\r\nX-Evil: evil"u8;

public static void TestRequestBadUserAgent(ж<testing.T> Ꮡt) {
    ref var got = ref heap<slice<@string>>(out var Ꮡgot);
    got = new @string[]{}.slice();
    var (req, err) = NewRequest(getˢ2, httpFooAfterˢ, default!);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~req).Header.Set(userAgentˢ2, evilXEvilEvilˢ);
    req.Write(new logWrites(Ꮡt, Ꮡgot));
    var want = new @string[]{
        "GET /after HTTP/1.1\r\n"u8,
        "Host: foo\r\n"u8,
        "User-Agent: evil  X-Evil: evil\r\n"u8,
        "\r\n"u8
    }.slice();
    if (!slices.Equal<slice<@string>, @string>(got, want)) {
        Ꮡt.Errorf("Writes = %q\n  Want = %q"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mSearchHttp11ˢ = "M-SEARCH * HTTP/1.1\r\n\r\n"u8;
internal static readonly @string chunkedˢ = "chunked"u8;
internal static readonly object wroteChunkedRequestWantˢ = (@string)"wrote chunked request; want no body"u8;

public static void TestStarRequest(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (req, err) = ReadRequest(bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader(mSearchHttp11ˢ))));
    if (err != default!) {
        return;
    }
    if ((~req).ContentLength != 0) {
        Ꮡt.Errorf("ContentLength = %d; want 0"u8, (~req).ContentLength);
    }
    if ((~req).Body == default!) {
        Ꮡt.Errorf("Body = nil; want non-nil"u8);
    }
    // Request.Write has Client semantics for Body/ContentLength,
    // where ContentLength 0 means unknown if Body is non-nil, and
    // thus chunking will happen unless we change semantics and
    // signal that we want to serialize it as exactly zero.  The
    // only way to do that for outbound requests is with a nil
    // Body:
    ref var clientReq = ref heap<Δhttp.Request>(out var ᏑclientReq);
    clientReq = req.Value;
    clientReq.Body = default!;
    ref var @out = ref heap(new strings.Builder(), out var Ꮡout);
    {
        var errΔ1 = ᏑclientReq.Write(new http_test_package.strings_BuilderжWriter(Ꮡout)); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    if (strings.Contains(@out.String(), chunkedˢ)) {
        Ꮡt.Error(wroteChunkedRequestWantˢ);
    }
    (var back, err) = ReadRequest(bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader(@out.String()))));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // Ignore the Headers (the User-Agent breaks the deep equal,
    // but we don't care about it)
    req.Value.Header = default!;
    back.Value.Header = default!;
    if (!reflect.DeepEqual(req.OrTypedNil(), back.OrTypedNil())) {
        Ꮡt.Errorf("Original request doesn't match Request read back."u8);
        Ꮡt.Logf("Original: %#v"u8, req.OrTypedNil());
        Ꮡt.Logf("Original.URL: %#v"u8, (~req).URL.OrTypedNil());
        Ꮡt.Logf("Wrote: %s"u8, @out.String());
        Ꮡt.Logf("Read back (doesn't match Original): %#v"u8, back.OrTypedNil());
    }
}

[GoType] partial struct responseWriterJustWriter {
    [GoEmbedded] public io_package.Writer Writer;
}

internal static httpꓸHeader Header(this responseWriterJustWriter _) {
    throw panic("should not be called");
}

internal static void WriteHeader(this responseWriterJustWriter _Δp0, nint _Δp1) {
    throw panic("should not be called");
}

// delayedEOFReader never returns (n > 0, io.EOF), instead putting
// off the io.EOF until a subsequent Read call.
[GoType] partial struct delayedEOFReader {
    internal io.Reader r;
}

internal static (nint n, error err) Read(this delayedEOFReader dr, slice<byte> p) {
    nint n = default!;
    error err = default!;

    (n, err) = dr.r.Read(p);
    if (n > 0 && AreEqual(err, io.EOF)) {
        err = default!;
    }
    return (n, err);
}

public static void TestIssue10884_MaxBytesEOF(ж<testing.T> Ꮡt) {
    var dst = io.Discard;
    var (_, err) = io.Copy(dst, MaxBytesReader(
        new responseWriterJustWriter(dst),
        io.NopCloser(new delayedEOFReader(new http_test_package.strings_ReaderжReader(strings.NewReader("12345"u8)))),
        5));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
}

[GoType("dyn")] internal partial struct TestMaxBytesReaderStickyError_tests {
    internal nint readable;
    internal int64 limit;
}

// Issue 14981: MaxBytesReader's return error wasn't sticky. It
// doesn't technically need to be, but people expected it to be.
public static void TestMaxBytesReaderStickyError(ж<testing.T> Ꮡt) {
    error isSticky(io.Reader r) {
        ref var log = ref heap(new bytes.Buffer(), out var Ꮡlog);
        var buf = new slice<byte>(1000);
        error firstErr = default!;
        while (ᐧ) {
            var (n, err) = r.Read(buf);
            fmt.Fprintf(new http_test_package.bytes_BufferжWriter(Ꮡlog), "Read(%d) = %d, %v\n"u8, len(buf), n, err);
            if (err == default!) {
                continue;
            }
            if (firstErr == default!) {
                firstErr = err;
                continue;
            }
            if (!reflect.DeepEqual(err, firstErr)) {
                return fmt.Errorf("non-sticky error. got log:\n%s"u8, log.Bytes());
            }
            Ꮡt.Logf("Got log: %s"u8, log.Bytes());
            return default!;
        }
    }
    var tests = new array<TestMaxBytesReaderStickyError_tests>(3){
        [0] = new(99, 100),
        [1] = new(100, 100),
        [2] = new(101, 100)
    };
    foreach (var (i, tt) in tests.ΔRangeSnapshot()) {
        var rc = MaxBytesReader(default!, io.NopCloser(new http_test_package.bytes_ReaderжReader(bytes.NewReader(new slice<byte>(tt.readable)))), tt.limit);
        {
            var err = isSticky(rc); if (err != default!) {
                Ꮡt.Errorf("%d. error: %v"u8, i, err);
            }
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string testStrᶜ = "1234"u8;

[GoType("dyn")] internal partial struct TestMaxBytesReaderDifferentLimits_tests {
    internal int64 limit;
    internal nint lenP;
    internal nint wantN;
    internal bool wantErr;
}

// Issue 45101: maxBytesReader's Read panicked when n < -1. This test
// also ensures that Read treats negative limits as equivalent to 0.
public static void TestMaxBytesReaderDifferentLimits(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

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
        [10] = new(
            limit: (int64)(9223372036854775807L), /* Issue 54408 */

            lenP: len(testStr),
            wantN: len(testStr),
            wantErr: false
        )
    };
    foreach (var (i, tt) in tests.ΔRangeSnapshot()) {
        var rc = MaxBytesReader(default!, io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(testStr))), tt.limit);
        var (n, err) = rc.Read(new slice<byte>(tt.lenP));
        if (n != tt.wantN) {
            Ꮡt.Errorf("%d. n: %d, want n: %d"u8, i, n, tt.wantN);
        }
        if ((err != default!) != tt.wantErr) {
            Ꮡt.Errorf("%d. error: %v"u8, i, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpsGolangOrgˢ = "https://golang.org/"u8;
internal static readonly object expectedNilUrlInClonedˢ = (@string)"expected nil URL in cloned request"u8;

public static void TestWithContextNilURL(ж<testing.T> Ꮡt) {
    var (req, err) = NewRequest(postˢ, httpsGolangOrgˢ, default!);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // Issue 20601
    req.Value.URL = default!;
    var reqCopy = req.WithContext(context_package.Background());
    if ((~reqCopy).URL != nil) {
        Ꮡt.Error(expectedNilUrlInClonedˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpsExampleOrgˢ = "https://example.org/"u8;
internal static readonly @string encoding2ˢ = "encoding2"u8;
internal static readonly object expectedReqˢ = (@string)"expected req.TransferEncoding to be changed"u8;
internal static readonly object expectedClonedReqˢ = (@string)"expected clonedReq.TransferEncoding to be unchanged"u8;

// Ensure that Request.Clone creates a deep copy of TransferEncoding.
// See issue 41907.
public static void TestRequestCloneTransferEncoding(ж<testing.T> Ꮡt) {
    var body = strings.NewReader(bodyˢ);
    var (req, _) = NewRequest(postˢ, httpsExampleOrgˢ, new http_test_package.strings_ReaderжReader(body));
    req.Value.TransferEncoding = new @string[]{
        "encoding1"u8
    }.slice();
    var clonedReq = req.Clone(context_package.Background());
    // modify original after deep copy
    req.Value.TransferEncoding[0] = encoding2ˢ;
    if ((~req).TransferEncoding[0] != "encoding2") {
        Ꮡt.Error(expectedReqˢ);
    }
    if ((~clonedReq).TransferEncoding[0] != "encoding1") {
        Ꮡt.Error(expectedClonedReqˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string origˢ = "orig"u8;
internal static readonly @string copyˢ = "copy"u8;

// Ensure that Request.Clone works correctly with PathValue.
// See issue 64911.
public static void TestRequestClonePathValue(ж<testing.T> Ꮡt) {
    var (req, _) = Δhttp.NewRequest(getˢ2, httpsExampleOrgˢ, default!);
    req.SetPathValue("p1"u8, origˢ);
    var clonedReq = req.Clone(context_package.Background());
    clonedReq.SetPathValue("p2"u8, copyˢ);
    // Ensure that any modifications to the cloned
    // request do not pollute the original request.
    {
        @string g = req.PathValue("p2"u8);
        @string w = ""u8; if (g != w) {
            Ꮡt.Fatalf("p2 mismatch got %q, want %q"u8, g, w);
        }
    }
    {
        @string g = req.PathValue("p1"u8);
        @string w = origˢ; if (g != w) {
            Ꮡt.Fatalf("p1 mismatch got %q, want %q"u8, g, w);
        }
    }
    // Assert on the changes to the cloned request.
    {
        @string g = clonedReq.PathValue("p1"u8);
        @string w = origˢ; if (g != w) {
            Ꮡt.Fatalf("p1 mismatch got %q, want %q"u8, g, w);
        }
    }
    {
        @string g = clonedReq.PathValue("p2"u8);
        @string w = copyˢ; if (g != w) {
            Ꮡt.Fatalf("p2 mismatch got %q, want %q"u8, g, w);
        }
    }
}

// Issue 34878: verify we don't panic when including basic auth (Go 1.13 regression)
public static void TestNoPanicOnRoundTripWithBasicAuth(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testNoPanicWithBasicAuth(Δp0, Δp1));
}

internal static void testNoPanicWithBasicAuth(ж<testing.T> Ꮡt, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    })));
    var (u, err) = url.Parse((~(~cst).ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    u.Value.User = url.UserPassword(fooˢ, barˢ);
    var req = Ꮡ(new Request(
        URL: u,
        Method: "GET"u8
    ));
    {
        var (_, errΔ1) = (~cst).c.Do(req); if (errΔ1 != default!) {
            Ꮡt.Fatalf("Unexpected error: %v"u8, errΔ1);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpFooTldˢ = "http://foo.tld/"u8;

[GoType("dyn")] internal partial struct TestNewRequestGetBody_tests {
    internal io.Reader r;
}

// verify that NewRequest sets Request.GetBody and that it works
public static void TestNewRequestGetBody(ж<testing.T> Ꮡt) {
    var tests = new TestNewRequestGetBody_tests[]{
        new(r: new http_test_package.strings_ReaderжReader(strings.NewReader(helloˢ3))),
        new(r: new http_test_package.bytes_ReaderжReader(bytes.NewReader(slice<byte>("hello"u8)))),
        new(r: new http_test_package.bytes_BufferжReader(bytes.NewBuffer(slice<byte>("hello"u8))))
    }.slice();
    foreach (var (i, tt) in tests) {
        var (req, err) = NewRequest(postˢ, httpFooTldˢ, tt.r);
        if (err != default!) {
            Ꮡt.Errorf("test[%d]: %v"u8, i, err);
            continue;
        }
        if ((~req).Body == default!) {
            Ꮡt.Errorf("test[%d]: Body = nil"u8, i);
            continue;
        }
        if ((~req).GetBody == default!) {
            Ꮡt.Errorf("test[%d]: GetBody = nil"u8, i);
            continue;
        }
        (var slurp1, err) = io.ReadAll((~req).Body);
        if (err != default!) {
            Ꮡt.Errorf("test[%d]: ReadAll(Body) = %v"u8, i, err);
        }
        (var newBody, err) = (~req).GetBody();
        if (err != default!) {
            Ꮡt.Errorf("test[%d]: GetBody = %v"u8, i, err);
        }
        (var slurp2, err) = io.ReadAll(newBody);
        if (err != default!) {
            Ꮡt.Errorf("test[%d]: ReadAll(GetBody()) = %v"u8, i, err);
        }
        if (((sstring)slurp1) != ((sstring)slurp2)) {
            Ꮡt.Errorf("test[%d]: Body %q != GetBody %q"u8, i, slurp1, slurp2);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string missingˢ = "missing"u8;

internal static void testMissingFile(ж<testing.T> Ꮡt, ж<Δhttp.Request> Ꮡreq) {
    var (f, fh, err) = Ꮡreq.FormFile(missingˢ);
    if (f != default!) {
        Ꮡt.Errorf("FormFile file = %v, want nil"u8, f);
    }
    if (fh != nil) {
        Ꮡt.Errorf("FormFile file header = %v, want nil"u8, fh.OrTypedNil());
    }
    if (!AreEqual(err, ErrMissingFile)) {
        Ꮡt.Errorf("FormFile err = %q, want ErrMissingFile"u8, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object newRequestˢ = (@string)"NewRequest:"u8;
internal static readonly @string contentTypeˢ2 = "Content-type"u8;

internal static ж<Δhttp.Request> newTestMultipartRequest(ж<testing.T> Ꮡt) {
    var b = strings.NewReader(strings.ReplaceAll(message, "\n"u8, "\r\n"u8));
    var (req, err) = NewRequest(postˢ, "/"u8, new http_test_package.strings_ReaderжReader(b));
    if (err != default!) {
        Ꮡt.Fatal(newRequestˢ, err);
    }
    @string ctype = fmt.Sprintf(@"multipart/form-data; boundary=""%s"""u8, boundary);
    (~req).Header.Set(contentTypeˢ2, ctype);
    return req;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string textaˢ = "texta"u8;
internal static readonly @string textbˢ = "textb"u8;
internal static readonly object isOsFileShouldNotBeˢ = (@string)" is *os.File, should not be"u8;
internal static readonly @string fileaˢ = "filea"u8;
internal static readonly @string fileaTxtˢ = "filea.txt"u8;
internal static readonly @string filebˢ = "fileb"u8;
internal static readonly @string filebTxtˢ = "fileb.txt"u8;

internal static void validateTestMultipartContents(ж<testing.T> Ꮡt, ж<Δhttp.Request> Ꮡreq, bool allMem) {
    GoFrame ᒐ = default;
    try {
        {
            @string g = Ꮡreq.FormValue(textaˢ);
            @string e = textaValue; if (g != e) {
                Ꮡt.Errorf("texta value = %q, want %q"u8, g, e);
            }
        }
        {
            @string g = Ꮡreq.FormValue(textbˢ);
            @string e = textbValue; if (g != e) {
                Ꮡt.Errorf("textb value = %q, want %q"u8, g, e);
            }
        }
        {
            @string g = Ꮡreq.FormValue(missingˢ); if (g != ""u8) {
                Ꮡt.Errorf("missing value = %q, want empty string"u8, g);
            }
        }
        void assertMem(@string n, multipart.File fd) {
            {
                var (_, ok) = fd._<ж<os.File>>(ᐧ); if (ok) {
                    Ꮡt.Error(n, isOsFileShouldNotBeˢ);
                }
            }
        }
        var fda = testMultipartFile(Ꮡt, Ꮡreq, fileaˢ, fileaTxtˢ, fileaContents);
        var fdaʗ1 = fda;
        defer(() => fdaʗ1.Close(), ref ᒐ);
        assertMem(fileaˢ, fda);
        var fdb = testMultipartFile(Ꮡt, Ꮡreq, filebˢ, filebTxtˢ, filebContents);
        var fdbʗ1 = fdb;
        defer(() => fdbʗ1.Close(), ref ᒐ);
        if (allMem){
            assertMem(filebˢ, fdb);
        } else {
            {
                var (_, ok) = fdb._<ж<os.File>>(ᐧ); if (!ok) {
                    Ꮡt.Errorf("fileb has unexpected underlying type %T"u8, fdb);
                }
            }
        }
        testMissingFile(Ꮡt, Ꮡreq);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object copyingContentsˢ = (@string)"copying contents:"u8;

internal static multipart.File testMultipartFile(ж<testing.T> Ꮡt, ж<Δhttp.Request> Ꮡreq, @string key, @string expectFilename, @string expectContent) {
    var (f, fh, err) = Ꮡreq.FormFile(key);
    if (err != default!) {
        Ꮡt.Fatalf("FormFile(%q): %q"u8, key, err);
    }
    if ((~fh).Filename != expectFilename) {
        Ꮡt.Errorf("filename = %q, want %q"u8, (~fh).Filename, expectFilename);
    }
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    (_, err) = io.Copy(new http_test_package.strings_BuilderжWriter(Ꮡb), f);
    if (err != default!) {
        Ꮡt.Fatal(copyingContentsˢ, err);
    }
    {
        @string g = b.String(); if (g != expectContent) {
            Ꮡt.Errorf("contents = %q, want %q"u8, g, expectContent);
        }
    }
    return f;
}

[GoType("dyn")] internal partial struct TestRequestCookie_type {
    internal @string name;
    internal @string value;
    internal error expectedErr;
}

// Issue 53181: verify Request.Cookie return the correct Cookie.
// Return ErrNoCookie instead of the first cookie when name is "".
public static void TestRequestCookie(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in new TestRequestCookie_type[]{
        new(
            name: "foo"u8,
            value: "bar"u8,
            expectedErr: default!
        ),
        new(
            name: ""u8,
            expectedErr: ErrNoCookie
        )
    }.slice()) {
        var (req, err) = NewRequest(getˢ2, httpExampleComˢ2, default!);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        req.AddCookie(Ꮡ(new httpꓸCookie(Name: tt.name, Value: tt.value)));
        (var c, err) = req.Cookie(tt.name);
        if (!AreEqual(err, tt.expectedErr)) {
            Ꮡt.Errorf("got %v, want %v"u8, err, tt.expectedErr);
        }
        // skip if error occurred.
        if (err != default!) {
            continue;
        }
        if ((~c).Value != tt.value) {
            Ꮡt.Errorf("got %v, want %v"u8, (~c).Value, tt.value);
        }
        if ((~c).Name != tt.name) {
            Ꮡt.Errorf("got %s, want %v"u8, tt.name, (~c).Name);
        }
    }
}

[GoType("dyn")] internal partial struct TestRequestCookiesByName_tests {
    internal slice<ж<httpꓸCookie>> @in;
    internal @string filter;
    internal slice<ж<httpꓸCookie>> want;
}

public static void TestRequestCookiesByName(ж<testing.T> Ꮡt) {
    var tests = new TestRequestCookiesByName_tests[]{
        new(
            @in: new ж<httpꓸCookie>[]{
                Ꮡ(new httpꓸCookie(Name: "foo"u8, Value: "foo-1"u8)),
                Ꮡ(new httpꓸCookie(Name: "bar"u8, Value: "bar"u8))
            }.slice(),
            filter: "foo"u8,
            want: new ж<httpꓸCookie>[]{Ꮡ(new httpꓸCookie(Name: "foo"u8, Value: "foo-1"u8))}.slice()
        ),
        new(
            @in: new ж<httpꓸCookie>[]{
                Ꮡ(new httpꓸCookie(Name: "foo"u8, Value: "foo-1"u8)),
                Ꮡ(new httpꓸCookie(Name: "foo"u8, Value: "foo-2"u8)),
                Ꮡ(new httpꓸCookie(Name: "bar"u8, Value: "bar"u8))
            }.slice(),
            filter: "foo"u8,
            want: new ж<httpꓸCookie>[]{
                Ꮡ(new httpꓸCookie(Name: "foo"u8, Value: "foo-1"u8)),
                Ꮡ(new httpꓸCookie(Name: "foo"u8, Value: "foo-2"u8))
            }.slice()
        ),
        new(
            @in: new ж<httpꓸCookie>[]{
                Ꮡ(new httpꓸCookie(Name: "bar"u8, Value: "bar"u8))
            }.slice(),
            filter: "foo"u8,
            want: new ж<httpꓸCookie>[]{}.slice()
        ),
        new(
            @in: new ж<httpꓸCookie>[]{
                Ꮡ(new httpꓸCookie(Name: "bar"u8, Value: "bar"u8))
            }.slice(),
            filter: ""u8,
            want: new ж<httpꓸCookie>[]{}.slice()
        ),
        new(
            @in: new ж<httpꓸCookie>[]{}.slice(),
            filter: "foo"u8,
            want: new ж<httpꓸCookie>[]{}.slice()
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestRequestCookiesByName_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.filter, (ж<testing.T> tΔ1) => {
            var (req, err) = NewRequest(getˢ2, httpExampleComˢ2, default!);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            foreach (var (_, c) in ttʗ1.@in) {
                req.AddCookie(c);
            }
            var got = req.CookiesNamed(ttʗ1.filter);
            if (!reflect.DeepEqual(got, ttʗ1.want)) {
                @string asStr(any v) {
                    var (blob, _) = json.MarshalIndent(v, ""u8, "  "u8);
                    return ((@string)blob);
                }
                tΔ1.Fatalf("Result mismatch\n\tGot: %s\n\tWant: %s"u8, asStr(got), asStr(ttʗ1.want));
            }
        });
    }
}

internal static readonly @string fileaContents = "This is a test file."u8;
internal static readonly @string filebContents = "Another test file."u8;
internal static readonly @string textaValue = "foo"u8;
internal static readonly @string textbValue = "bar"u8;
internal static readonly @string boundary = @"MyBoundary"u8;

internal static readonly @string message = "\n--MyBoundary\nContent-Disposition: form-data; name=\"filea\"; filename=\"filea.txt\"\nContent-Type: text/plain\n\nThis is a test file.\n--MyBoundary\nContent-Disposition: form-data; name=\"fileb\"; filename=\"fileb.txt\"\nContent-Type: text/plain\n\nAnother test file.\n--MyBoundary\nContent-Disposition: form-data; name=\"texta\"\n\nfoo\n--MyBoundary\nContent-Disposition: form-data; name=\"textb\"\n\nbar\n--MyBoundary--\n";

internal static void benchmarkReadRequest(ж<testing.B> Ꮡb, @string request) {
    ref var b = ref Ꮡb.DerefOrNull();

    request = request + "\n"u8; // final \n
    request = strings.ReplaceAll(request, "\n"u8, "\r\n"u8); // expand \n to \r\n
    b.SetBytes((int64)len(request));
    var r = bufio.NewReader(new http_test_package.infiniteReaderжReader(Ꮡ(new infiniteReader(buf: slice<byte>(request)))));
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        var (_, err) = ReadRequest(r);
        if (err != default!) {
            Ꮡb.Fatalf("failed to read request: %v"u8, err);
        }
    }
}

// infiniteReader satisfies Read requests as if the contents of buf
// loop indefinitely.
[GoType] partial struct infiniteReader {
    internal slice<byte> buf;
    internal nint offset;
}

[GoRecv] internal static (nint, error) Read(this ref infiniteReader r, slice<byte> b) {
    nint n = copy(b, r.buf[(int)(r.offset)..]);
    r.offset = (r.offset + n) % len(r.buf);
    return (n, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getHttp11HostLocalhostˢ = """
GET / HTTP/1.1
Host: localhost:8080
Connection: keep-alive
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8
User-Agent: Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_2) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.52 Safari/537.17
Accept-Encoding: gzip,deflate,sdch
Accept-Language: en-US,en;q=0.8
Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.3
Cookie: __utma=1.1978842379.1323102373.1323102373.1323102373.1; EPi:NumberOfVisits=1,2012-02-28T13:42:18; CrmSession=5b707226b9563e1bc69084d07a107c98; plushContainerWidth=100%25; plushNoTopMenu=0; hudson_auto_refresh=false

"""u8;

public static void BenchmarkReadRequestChrome(ж<testing.B> Ꮡb) {
    // https://github.com/felixge/node-http-perf/blob/master/fixtures/get.http
    benchmarkReadRequest(Ꮡb, getHttp11HostLocalhostˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getHttp11UserAgentCurl7ˢ = """
GET / HTTP/1.1
User-Agent: curl/7.27.0
Host: localhost:8080
Accept: */*

"""u8;

public static void BenchmarkReadRequestCurl(ж<testing.B> Ꮡb) {
    // curl http://localhost:8080/
    benchmarkReadRequest(Ꮡb, getHttp11UserAgentCurl7ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getHttp10HostLocalhostˢ = """
GET / HTTP/1.0
Host: localhost:8080
User-Agent: ApacheBench/2.3
Accept: */*

"""u8;

public static void BenchmarkReadRequestApachebench(ж<testing.B> Ꮡb) {
    // ab -n 1 -c 1 http://localhost:8080/
    benchmarkReadRequest(Ꮡb, getHttp10HostLocalhostˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getHttp11HostLocalhostˢ2 = """
GET / HTTP/1.1
Host: localhost:8080
Accept: */*
Accept-Encoding: gzip
User-Agent: JoeDog/1.00 [en] (X11; I; Siege 2.70)
Connection: keep-alive

"""u8;

public static void BenchmarkReadRequestSiege(ж<testing.B> Ꮡb) {
    // siege -r 1 -c 1 http://localhost:8080/
    benchmarkReadRequest(Ꮡb, getHttp11HostLocalhostˢ2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getHttp11HostLocalhostˢ3 = """
GET / HTTP/1.1
Host: localhost:8080

"""u8;

public static void BenchmarkReadRequestWrk(ж<testing.B> Ꮡb) {
    // wrk -t 1 -r 1 -c 1 http://localhost:8080/
    benchmarkReadRequest(Ꮡb, getHttp11HostLocalhostˢ3);
}

public static void BenchmarkFileAndServer_1KB(ж<testing.B> Ꮡb) {
    benchmarkFileAndServer(Ꮡb, ((int64)1 << (int)(10)));
}

public static void BenchmarkFileAndServer_16MB(ж<testing.B> Ꮡb) {
    benchmarkFileAndServer(Ꮡb, ((int64)1 << (int)(24)));
}

public static void BenchmarkFileAndServer_64MB(ж<testing.B> Ꮡb) {
    benchmarkFileAndServer(Ꮡb, ((int64)1 << (int)(26)));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goBenchHttpFileAndServerˢ = "go-bench-http-file-and-server"u8;

internal static void benchmarkFileAndServer(ж<testing.B> Ꮡb, int64 n) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.CreateTemp(os.TempDir(), goBenchHttpFileAndServerˢ);
        if (err != default!) {
            Ꮡb.Fatalf("Failed to create temp file: %v"u8, err);
        }
        var fʗ1 = f;
        defer(() => {
            fʗ1.Close();
            os.RemoveAll(fʗ1.Name());
        }, ref ᒐ);
        {
            var (_, errΔ1) = io.CopyN(new os.FileжWriter(f), rand.Reader, n); if (errΔ1 != default!) {
                Ꮡb.Fatalf("Failed to copy %d bytes: %v"u8, n, errΔ1);
            }
        }
        var fʗ2 = f;
        run<BжTBRun>(Ꮡb, (BжTBRun bΔ1Δp, testMode mode) => {
            var bΔ1 = (ж<testing.B>)bΔ1Δp;
            runFileAndServerBenchmarks(bΔ1, mode, fʗ2, n);
        }, new testMode[]{http1Mode, https1Mode, http2Mode}.slice());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void runFileAndServerBenchmarks(ж<testing.B> Ꮡb, testMode mode, ж<os.File> Ꮡf, int64 n) {
    ref var b = ref Ꮡb.DerefOrNull();

    var handler = new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> req) => {
        GoFrame ᒐ = default;
        try {
            defer(() => (~req).Body.Close(), ref ᒐ);
            var (nc, err) = io.Copy(io.Discard, (~req).Body);
            if (err != default!) {
                throw panic(err);
            }
            if (nc != n) {
                throw panic(fmt.Errorf("Copied %d Wanted %d bytes"u8, nc, n));
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    var cst = newClientServerTest(new http_test_package.testing_BжTB(Ꮡb), mode, new http_test_package.http_HandlerFuncᴠΔHandler(handler)).Value.ts;
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        // Perform some setup.
        b.StopTimer();
        {
            var (_, errΔ1) = Ꮡf.Seek(0, 0); if (errΔ1 != default!) {
                Ꮡb.Fatalf("Failed to seek back to file: %v"u8, errΔ1);
            }
        }
        b.StartTimer();
        var (req, err) = NewRequest(putˢ, (~cst).URL, io.NopCloser(new http_test_package.os_FileжReader(Ꮡf)));
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        req.Value.ContentLength = n;
        // Prevent mime sniffing by setting the Content-Type.
        (~req).Header.Set(contentTypeˢ, applicationOctetStreamˢ);
        (var res, err) = cst.Client().Do(req);
        if (err != default!) {
            Ꮡb.Fatalf("Failed to make request to backend: %v"u8, err);
        }
        (~res).Body.Close();
        b.SetBytes(n);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errorsIsErrNotSupportedˢ = (@string)"errors.Is(ErrNotSupported, errors.ErrUnsupported) failed"u8;

public static void TestErrNotSupported(ж<testing.T> Ꮡt) {
    if (!errors.Is(new Δhttp.ProtocolErrorжerror(ErrNotSupported), errors.ErrUnsupported)) {
        Ꮡt.Error(errorsIsErrNotSupportedˢ);
    }
}

public static void TestPathValueNoMatch(ж<testing.T> Ꮡt) {
    // Check that PathValue and SetPathValue work on a Request that was never matched.
    Δhttp.Request r = default!;
    {
        @string g = r.PathValue("x"u8);
        @string w = ""u8; if (g != w) {
            Ꮡt.Errorf("got %q, want %q"u8, g, w);
        }
    }
    r.SetPathValue("x"u8, "a"u8);
    {
        @string g = r.PathValue("x"u8);
        @string w = "a"u8; if (g != w) {
            Ꮡt.Errorf("got %q, want %q"u8, g, w);
        }
    }
}

[GoType("dyn")] internal partial struct TestPathValueAndPattern_type {
    internal @string pattern;
    internal @string url;
    internal map<@string, @string> want;
}

public static void TestPathValueAndPattern(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        foreach (var (_, vᴛ1) in new TestPathValueAndPattern_type[]{
            new(
                "/{a}/is/{b}/{c...}"u8,
                "/now/is/the/time/for/all"u8,
                new map<@string, @string>{
                    ["a"u8] = "now"u8,
                    ["b"u8] = "the"u8,
                    ["c"u8] = "time/for/all"u8,
                    ["d"u8] = ""u8
                }
            ),
            new(
                "/names/{name}/{other...}"u8,
                "/names/%2fjohn/address"u8,
                new map<@string, @string>{
                    ["name"u8] = "/john"u8,
                    ["other"u8] = "address"u8
                }
            ),
            new(
                "/names/{name}/{other...}"u8,
                "/names/john%2Fdoe/there/is%2F/more"u8,
                new map<@string, @string>{
                    ["name"u8] = "john/doe"u8,
                    ["other"u8] = "there/is//more"u8
                }
            ),
            new(
                "/names/{name}/{other...}"u8,
                "/names/n/*"u8,
                new map<@string, @string>{
                    ["name"u8] = "n"u8,
                    ["other"u8] = "*"u8
                }
            )
        }.slice()) {
            ref var test = ref heap(new TestPathValueAndPattern_type(), out var Ꮡtest);
            test = vᴛ1;

            var mux = NewServeMux();
            var testʗ1 = test;
            mux.HandleFunc(test.pattern, (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
                foreach (var (name, want) in testʗ1.want) {
                    @string got = r.PathValue(name);
                    if (got != want) {
                        Ꮡt.Errorf("%q, %q: got %q, want %q"u8, testʗ1.pattern, name, got, want);
                    }
                }
                if ((~r).Pattern != testʗ1.pattern) {
                    Ꮡt.Errorf("pattern: got %s, want %s"u8, (~r).Pattern, testʗ1.pattern);
                }
            });
            var server = httptest.NewServer(new Δhttp.ServeMuxжΔHandler(mux));
            var serverʗ1 = server;
            defer(serverʗ1.Close, ref ᒐ);
            var (res, err) = Get((~server).URL + test.url);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            (~res).Body.Close();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aBCDˢ = "/a/{b}/c/{d...}"u8;

public static void TestSetPathValue(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var mux = NewServeMux();
        mux.HandleFunc(aBCDˢ, (Δhttp.ResponseWriter _, ж<Δhttp.Request> r) => {
            var kvs = new map<@string, @string>{
                ["b"u8] = "X"u8,
                ["d"u8] = "Y"u8,
                ["a"u8] = "Z"u8
            };
            foreach (var (k, v) in kvs) {
                r.SetPathValue(k, v);
            }
            foreach (var (k, w) in kvs) {
                {
                    @string g = r.PathValue(k); if (g != w) {
                        Ꮡt.Errorf("got %q, want %q"u8, g, w);
                    }
                }
            }
        });
        var server = httptest.NewServer(new Δhttp.ServeMuxжΔHandler(mux));
        var serverʗ1 = server;
        defer(serverʗ1.Close, ref ᒐ);
        var (res, err) = Get((~server).URL + "/a/b/c/d/e"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (~res).Body.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getGˢ = "GET /g"u8;
internal static readonly @string postPˢ = "POST /p"u8;
internal static readonly @string patchPˢ = "PATCH /p"u8;
internal static readonly @string putRˢ = "PUT /r"u8;
internal static readonly @string getRˢ = "GET /r/"u8;
internal static readonly @string allowˢ = "Allow"u8;

[GoType("dyn")] internal partial struct TestStatus_type {
    internal @string method, path;
    internal nint wantStatus;
    internal @string wantAllow;
}

public static void TestStatus(ж<testing.T> Ꮡt) {
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
        foreach (var (_, test) in new TestStatus_type[]{
            new("GET"u8, "/g"u8, 200, ""u8),
            new("HEAD"u8, "/g"u8, 200, ""u8),
            new("POST"u8, "/g"u8, 405, "GET, HEAD"u8),
            new("GET"u8, "/x"u8, 404, ""u8),
            new("GET"u8, "/p"u8, 405, "PATCH, POST"u8),
            new("GET"u8, "/./p"u8, 405, "PATCH, POST"u8),
            new("GET"u8, "/r/"u8, 200, ""u8),
            new("GET"u8, "/r"u8, 200, ""u8), // redirected

            new("HEAD"u8, "/r/"u8, 200, ""u8),
            new("HEAD"u8, "/r"u8, 200, ""u8), // redirected

            new("PUT"u8, "/r/"u8, 405, "GET, HEAD"u8),
            new("PUT"u8, "/r"u8, 200, ""u8)
        }.slice()) {
            var (req, err) = Δhttp.NewRequest(test.method, (~server).URL + test.path, default!);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            (var res, err) = Δhttp.DefaultClient.Do(req);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            (~res).Body.Close();
            {
                nint g = res.Value.StatusCode;
                nint w = test.wantStatus; if (g != w) {
                    Ꮡt.Errorf("%s %s: got %d, want %d"u8, test.method, test.path, g, w);
                }
            }
            {
                @string g = (~res).Header.Get(allowˢ);
                @string w = test.wantAllow; if (g != w) {
                    Ꮡt.Errorf("%s %s, Allow: got %q, want %q"u8, test.method, test.path, g, w);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end http_test_package
