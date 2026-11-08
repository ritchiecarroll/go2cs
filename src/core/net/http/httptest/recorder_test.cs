// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.http;

using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using http = go.net.http_package;
using testing = testing_package;
using go.net;
using static go.net.http.httptest_package;
using ꓸꓸꓸstring = Span<@string>;

partial class httptest_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string hiFirstˢ => ref hiFirstˢᵇ;
private static readonly @string hiFirstˢᵇ = "hi first"u8;
internal static ref readonly @string textPlainCharsetUtf8ˢ => ref textPlainCharsetUtf8ˢᵇ;
private static readonly @string textPlainCharsetUtf8ˢᵇ = "text/plain; charset=utf-8"u8;
internal static ref readonly @string htmlˢ => ref htmlˢᵇ;
private static readonly @string htmlˢᵇ = "<html>"u8;
internal static ref readonly @string textHtmlCharsetUtf8ˢ => ref textHtmlCharsetUtf8ˢᵇ;
private static readonly @string textHtmlCharsetUtf8ˢᵇ = "text/html; charset=utf-8"u8;
internal static ref readonly @string someEncodingˢ => ref someEncodingˢᵇ;
private static readonly @string someEncodingˢᵇ = "some encoding"u8;
internal static ref readonly @string someTypeˢ => ref someTypeˢᵇ;
private static readonly @string someTypeˢᵇ = "some/type"u8;
internal static ref readonly @string keyˢ => ref keyˢᵇ;
private static readonly @string keyˢᵇ = "Key"u8;
internal static ref readonly @string correctˢ => ref correctˢᵇ;
private static readonly @string correctˢᵇ = "correct"u8;
internal static ref readonly @string incorrectˢ => ref incorrectˢᵇ;
private static readonly @string incorrectˢᵇ = "incorrect"u8;
internal static ref readonly @string nonTrailerˢ => ref nonTrailerˢᵇ;
private static readonly @string nonTrailerˢᵇ = "Non-Trailer"u8;
internal static ref readonly @string trailerATrailerBˢ => ref trailerATrailerBˢᵇ;
private static readonly @string trailerATrailerBˢᵇ = "Trailer-A, Trailer-B"u8;
internal static ref readonly @string trailerCˢ => ref trailerCˢᵇ;
private static readonly @string trailerCˢᵇ = "Trailer-C"u8;
internal static ref readonly @string trailerAˢ => ref trailerAˢᵇ;
private static readonly @string trailerAˢᵇ = "Trailer-A"u8;
internal static ref readonly @string valueaˢ => ref valueaˢᵇ;
private static readonly @string valueaˢᵇ = "valuea"u8;
internal static ref readonly @string valuecˢ => ref valuecˢᵇ;
private static readonly @string valuecˢᵇ = "valuec"u8;
internal static ref readonly @string trailerNotDeclaredˢ => ref trailerNotDeclaredˢᵇ;
private static readonly @string trailerNotDeclaredˢᵇ = "Trailer-NotDeclared"u8;
internal static ref readonly @string shouldBeOmittedˢ => ref shouldBeOmittedˢᵇ;
private static readonly @string shouldBeOmittedˢᵇ = "should be omitted"u8;
internal static ref readonly @string trailerTrailerDˢ => ref trailerTrailerDˢᵇ;
private static readonly @string trailerTrailerDˢᵇ = "Trailer:Trailer-D"u8;
internal static ref readonly @string withPrefixˢ => ref withPrefixˢᵇ;
private static readonly @string withPrefixˢᵇ = "with prefix"u8;
internal static ref readonly @string trailerBˢ => ref trailerBˢᵇ;
private static readonly @string trailerBˢᵇ = "Trailer-B"u8;
internal static ref readonly @string trailerDˢ => ref trailerDˢᵇ;
private static readonly @string trailerDˢᵇ = "Trailer-D"u8;
internal static ref readonly @string xFooˢ => ref xFooˢᵇ;
private static readonly @string xFooˢᵇ = "X-Foo"u8;
internal static ref readonly @string xBarˢ => ref xBarˢᵇ;
private static readonly @string xBarˢᵇ = "X-Bar"u8;
internal static ref readonly @string someBodyˢ => ref someBodyˢᵇ;
private static readonly @string someBodyˢᵇ = "Some body"u8;
internal static ref readonly @string httpFooComˢ => ref httpFooComˢᵇ;
private static readonly @string httpFooComˢᵇ = "http://foo.com/"u8;

internal partial struct TestRecorder_type /*dyn*/ {
    internal @string name;
    internal Action<http.ResponseWriter, ж<http.Request>> h;
    internal slice<Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>> checks;
}

public static void TestRecorder(ж<testing.T> t) {
    // type checkFunc is a methodless func type — rendered inline as its base delegate
    slice<Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>> check(params Span<Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>> fnsʗp) {
        var fns = fnsʗp.slice();
        return fns;
    }

    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasStatus(nint wantCode) => (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            if (rec.Code != wantCode) {
                return fmt.Errorf("Status = %d; want %d"u8, rec.Code, wantCode);
            }
            return default;
        });
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasResultStatus(@string want) => (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            if (rec.Result().Status != want) {
                return fmt.Errorf("Result().Status = %q; want %q"u8, rec.Result().Status, want);
            }
            return default;
        });
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasResultStatusCode(nint wantCode) => (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            if (rec.Result().StatusCode != wantCode) {
                return fmt.Errorf("Result().StatusCode = %d; want %d"u8, rec.Result().StatusCode, wantCode);
            }
            return default;
        });
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasResultContents(@string want) => (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            var (contentBytes, err) = io.ReadAll(rec.Result().Body);
            if (err != nil) {
                return err;
            }
            @string contents = (@string)contentBytes;
            if (contents != want) {
                return fmt.Errorf("Result().Body = %s; want %s"u8, contents, want);
            }
            return default;
        });
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasContents(@string want) => (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            if (rec.Body.String() != want) {
                return fmt.Errorf("wrote = %q; want %q"u8, rec.Body.String(), want);
            }
            return default;
        });
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasFlush(bool want) => (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            if (rec.Flushed != want) {
                return fmt.Errorf("Flushed = %v; want %v"u8, rec.Flushed, want);
            }
            return default;
        });
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasOldHeader(@string key, @string want) => (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            @string got = rec.HeaderMap.Get(key); if (got != want) {
                return fmt.Errorf("HeaderMap header %s = %q; want %q"u8, key, got, want);
            }
            return default;
        });
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasHeader(@string key, @string want) => (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            @string got = rec.Result().Header.Get(key); if (got != want) {
                return fmt.Errorf("final header %s = %q; want %q"u8, key, got, want);
            }
            return default;
        });
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasNotHeaders(params ꓸꓸꓸstring keysʗp) {
        var keys = keysʗp.slice();
        var keysʗ1 = keys;
        return (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            foreach (var (_, k) in range(keysʗ1)) {
                var (v, ok) = rec.Result().Header[http.CanonicalHeaderKey(k), ꟷ];
                if (ok) {
                    return fmt.Errorf("unexpected header %s with value %q"u8, k, v);
                }
            }
            return default;
        });
    }
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasTrailer(@string key, @string want) => (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            @string got = rec.Result().Trailer.Get(key); if (got != want) {
                return fmt.Errorf("trailer %s = %q; want %q"u8, key, got, want);
            }
            return default;
        });
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasNotTrailers(params ꓸꓸꓸstring keysʗp) {
        var keys = keysʗp.slice();
        var keysʗ2 = keys;
        return (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            var trailers = rec.Result().Trailer;
            foreach (var (_, k) in range(keysʗ2)) {
                var (_, ok) = trailers[http.CanonicalHeaderKey(k), ꟷ];
                if (ok) {
                    return fmt.Errorf("unexpected trailer %s"u8, k);
                }
            }
            return default;
        });
    }
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasContentLength(int64 length) => (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            var got = rec.Result().ContentLength; if (got != length) {
                return fmt.Errorf("ContentLength = %d; want %d"u8, got, length);
            }
            return default;
        });















    foreach (var (_, vᴛ1) in range(new TestRecorder_type[]{
        new(
            "200 default",
            (http.ResponseWriter w, ж<http.Request> r) => {
            },
            check(hasStatus(200), hasContents(""u8))
        ),
        new(
            "first code only",
            (http.ResponseWriter w, ж<http.Request> r) => {
                w.WriteHeader(201);
                w.WriteHeader(202);
                w.Write(slice<byte>("hi"u8));
            },
            check(hasStatus(201), hasContents("hi"))
        ),
        new(
            "write sends 200",
            (http.ResponseWriter w, ж<http.Request> r) => {
                w.Write(slice<byte>("hi first"u8));
                w.WriteHeader(201);
                w.WriteHeader(202);
            },
            check(hasStatus(200), hasContents(hiFirstˢ), hasFlush(false))
        ),
        new(
            "write string",
            (http.ResponseWriter w, ж<http.Request> r) => {
                io.WriteString(new httptest_test_package.http_ResponseWriterᴠWriter(w), hiFirstˢ);
            },
            check(
                hasStatus(200),
                hasContents(hiFirstˢ),
                hasFlush(false),
                hasHeader(contentTypeˢ, textPlainCharsetUtf8ˢ))
        ),
        new(
            "flush",
            (http.ResponseWriter w, ж<http.Request> r) => {
                w._<http.Flusher>().Flush(); // also sends a 200
                w.WriteHeader(201);
            },
            check(hasStatus(200), hasFlush(true), hasContentLength(-1))
        ),
        new(
            "Content-Type detection",
            (http.ResponseWriter w, ж<http.Request> r) => {
                io.WriteString(new httptest_test_package.http_ResponseWriterᴠWriter(w), htmlˢ);
            },
            check(hasHeader(contentTypeˢ, textHtmlCharsetUtf8ˢ))
        ),
        new(
            "no Content-Type detection with Transfer-Encoding",
            (http.ResponseWriter w, ж<http.Request> r) => {
                w.Header().Set(transferEncodingˢ, someEncodingˢ);
                io.WriteString(new httptest_test_package.http_ResponseWriterᴠWriter(w), htmlˢ);
            },
            check(hasHeader(contentTypeˢ, ""u8)) // no header

        ),
        new(
            "no Content-Type detection if set explicitly",
            (http.ResponseWriter w, ж<http.Request> r) => {
                w.Header().Set(contentTypeˢ, someTypeˢ);
                io.WriteString(new httptest_test_package.http_ResponseWriterᴠWriter(w), htmlˢ);
            },
            check(hasHeader(contentTypeˢ, someTypeˢ))
        ),
        new(
            "Content-Type detection doesn't crash if HeaderMap is nil",
            (http.ResponseWriter w, ж<http.Request> r) => {
                // Act as if the user wrote new(httptest.ResponseRecorder)
                // rather than using NewRecorder (which initializes
                // HeaderMap)
                w._<ж<global::go.net.http.httptest_package.ResponseRecorder>>().HeaderMap = default;
                io.WriteString(new httptest_test_package.http_ResponseWriterᴠWriter(w), htmlˢ);
            },
            check(hasHeader(contentTypeˢ, textHtmlCharsetUtf8ˢ))
        ),
        new(
            "Header is not changed after write",
            (http.ResponseWriter w, ж<http.Request> r) => {
                var hdr = w.Header();
                hdr.Set(keyˢ, correctˢ);
                w.WriteHeader(200);
                hdr.Set(keyˢ, incorrectˢ);
            },
            check(hasHeader(keyˢ, correctˢ))
        ),
        new(
            "Trailer headers are correctly recorded",
            (http.ResponseWriter w, ж<http.Request> r) => {
                w.Header().Set(nonTrailerˢ, correctˢ);
                w.Header().Set(trailerˢ, trailerATrailerBˢ);
                w.Header().Add(trailerˢ, trailerCˢ);
                io.WriteString(new httptest_test_package.http_ResponseWriterᴠWriter(w), htmlˢ);
                w.Header().Set(nonTrailerˢ, incorrectˢ);
                w.Header().Set(trailerAˢ, valueaˢ);
                w.Header().Set(trailerCˢ, valuecˢ);
                w.Header().Set(trailerNotDeclaredˢ, shouldBeOmittedˢ);
                w.Header().Set(trailerTrailerDˢ, withPrefixˢ);
            },
            check(
                hasStatus(200),
                hasHeader(contentTypeˢ, textHtmlCharsetUtf8ˢ),
                hasHeader(nonTrailerˢ, correctˢ),
                hasNotHeaders(trailerAˢ, trailerBˢ, trailerCˢ, trailerNotDeclaredˢ),
                hasTrailer(trailerAˢ, valueaˢ),
                hasTrailer(trailerCˢ, valuecˢ),
                hasNotTrailers(nonTrailerˢ, trailerBˢ, trailerNotDeclaredˢ),
                hasTrailer(trailerDˢ, withPrefixˢ))
        ),
        new(
            "Header set without any write", // Issue 15560
            (http.ResponseWriter w, ж<http.Request> r) => {
                w.Header().Set(xFooˢ, "1");

                // Simulate somebody using
                // new(ResponseRecorder) instead of
                // using the constructor which sets
                // this to 200
                w._<ж<global::go.net.http.httptest_package.ResponseRecorder>>().Code = 0;
            },
            check(
                hasOldHeader(xFooˢ, "1"),
                hasStatus(0),
                hasHeader(xFooˢ, "1"),
                hasResultStatus("200 OK"),
                hasResultStatusCode(200))
        ),
        new(
            "HeaderMap vs FinalHeaders", // more for Issue 15560
            (http.ResponseWriter w, ж<http.Request> r) => {
                var h = w.Header();
                h.Set(xFooˢ, "1");
                w.Write(slice<byte>("hi"u8));
                h.Set(xFooˢ, "2");
                h.Set(xBarˢ, "2");
            },
            check(
                hasOldHeader(xFooˢ, "2"),
                hasOldHeader(xBarˢ, "2"),
                hasHeader(xFooˢ, "1"),
                hasNotHeaders(xBarˢ))
        ),
        new(
            "setting Content-Length header",
            (http.ResponseWriter w, ж<http.Request> r) => {
                @string body = someBodyˢ;
                @string contentLength = fmt.Sprintf("%d"u8, len(body));
                w.Header().Set(contentLengthˢ, contentLength);
                io.WriteString(new httptest_test_package.http_ResponseWriterᴠWriter(w), body);
            },
            check(hasStatus(200), hasContents(someBodyˢ), hasContentLength(9))
        ),
        new(
            "nil ResponseRecorder.Body", // Issue 26642
            (http.ResponseWriter w, ж<http.Request> r) => {
                w._<ж<global::go.net.http.httptest_package.ResponseRecorder>>().Body = default;
                io.WriteString(new httptest_test_package.http_ResponseWriterᴠWriter(w), "hi");
            },
            check(hasResultContents(""u8)) // check we don't crash reading the body

        )
    }.array())) {
        ref var tt = ref heap(new TestRecorder_type(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            var (r, _) = http.NewRequest(getˢ, httpFooComˢ, default);
            var h = NilSafeDelegateConversion<http.HandlerFunc, Action<http.ResponseWriter, ж<http.Request>>>(ttʗ1.h);
            var rec = NewRecorder();
            h.ServeHTTP(new httptest_test_package.httptest_ResponseRecorderжResponseWriter(rec), r);
            foreach (var (_, checkΔ1) in range(ttʗ1.checks)) {
                var err = checkΔ1(rec); if (err != nil) {
                    tΔ1.Error(err);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string worldˢ => ref worldˢᵇ;
private static readonly @string worldˢᵇ = "world"u8;
internal static ref readonly @string helloWorldˢ => ref helloWorldˢᵇ;
private static readonly @string helloWorldˢᵇ = "hello world"u8;

public static void TestBodyNotAllowed(ж<testing.T> t) {
    var rw = NewRecorder();
    rw.Body = @new<bytes.Buffer>();
    rw.WriteHeader(204);

    var (_, err) = rw.Write(slice<byte>("hello "u8));
    if (!errors.Is(err, http.ErrBodyNotAllowed)) {
        t.Errorf("expected BodyNotAllowed for Write after 204, got: %v", err);
    }

    (_, err) = rw.WriteString(worldˢ);
    if (!errors.Is(err, http.ErrBodyNotAllowed)) {
        t.Errorf("expected BodyNotAllowed for WriteString after 204, got: %v", err);
    }

    @string got = rw.Body.String();
    @string want = helloWorldˢ; if (got != want) {
        t.Errorf("got Body=%q, want %q", got, want);
    }
}

internal partial struct TestParseContentLength_tests /*dyn*/ {
    internal @string cl;
    internal int64 want;
}

// issue 39017 - disallow Content-Length values such as "+3"
public static void TestParseContentLength(ж<testing.T> t) {
    var tests = slice<TestParseContentLength_tests>([
        new(
            cl: "3",
            want: 3
        ),
        new(
            cl: "+3",
            want: -1
        ),
        new(
            cl: "-3",
            want: -1
        ),
        new(
            // max int64, for safe conversion before returning
            cl: "9223372036854775807",
            want: 9223372036854775807L
        ),
        new(
            cl: "9223372036854775808",
            want: -1
        )
    ]);

    foreach (var (_, tt) in range(tests)) {
        var got = parseContentLength(tt.cl); if (got != tt.want) {
            t.Errorf("%q:\n\tgot=%d\n\twant=%d", tt.cl, got, tt.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedAPanicˢ = (@string)"Expected a panic"u8;
internal static ref readonly @string httpExampleOrgˢ => ref httpExampleOrgˢᵇ;
private static readonly @string httpExampleOrgˢᵇ = "http://example.org/"u8;

// Ensure that httptest.Recorder panics when given a non-3 digit (XXX)
// status HTTP code. See https://golang.org/issues/45353
public static void TestRecorderPanicsOnNonXXXStatusCode(ж<testing.T> t) {
    var badCodes = slice<nint>([
        -100, 0, 99, 1000, 20000
    ]);
    foreach (var (_, badCode) in range(badCodes)) {
        t.Run(fmt.Sprintf("Code=%d"u8, badCode), (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                defer(() => {
                    var rΔ1 = recover(); if (rΔ1 == nil) {
                        tΔ1.Fatal(expectedAPanicˢ);
                    }
                }, ref ᒐ);

                void handler(http.ResponseWriter rwΔ1, ж<http.Request> _Δp1) {
                    rwΔ1.WriteHeader(badCode);
                }
                var (r, _) = http.NewRequest(getˢ, httpExampleOrgˢ, default);
                var rw = NewRecorder();
                handler(new httptest_test_package.httptest_ResponseRecorderжResponseWriter(rw), r);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

} // end httptest_internal_test_package
