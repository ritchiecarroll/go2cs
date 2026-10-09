// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.http;

using fmt = fmt_package;
using io = io_package;
using http = go.net.http_package;
using testing = testing_package;
using bytes = bytes_package;
using go.net;
using static go.net.http.httptest_package;
using ꓸꓸꓸstring = Span<@string>;

partial class httptest_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hiFirstˢ = "hi first"u8;
internal static readonly @string textPlainCharsetUtf8ˢ = "text/plain; charset=utf-8"u8;
internal static readonly @string htmlˢ = "<html>"u8;
internal static readonly @string textHtmlCharsetUtf8ˢ = "text/html; charset=utf-8"u8;
internal static readonly @string someEncodingˢ = "some encoding"u8;
internal static readonly @string someTypeˢ = "some/type"u8;
internal static readonly @string keyˢ = "Key"u8;
internal static readonly @string correctˢ = "correct"u8;
internal static readonly @string incorrectˢ = "incorrect"u8;
internal static readonly @string nonTrailerˢ = "Non-Trailer"u8;
internal static readonly @string trailerATrailerBˢ = "Trailer-A, Trailer-B"u8;
internal static readonly @string trailerCˢ = "Trailer-C"u8;
internal static readonly @string trailerAˢ = "Trailer-A"u8;
internal static readonly @string valueaˢ = "valuea"u8;
internal static readonly @string valuecˢ = "valuec"u8;
internal static readonly @string trailerNotDeclaredˢ = "Trailer-NotDeclared"u8;
internal static readonly @string shouldBeOmittedˢ = "should be omitted"u8;
internal static readonly @string trailerTrailerDˢ = "Trailer:Trailer-D"u8;
internal static readonly @string withPrefixˢ = "with prefix"u8;
internal static readonly @string trailerBˢ = "Trailer-B"u8;
internal static readonly @string trailerDˢ = "Trailer-D"u8;
internal static readonly @string xFooˢ = "X-Foo"u8;
internal static readonly @string xBarˢ = "X-Bar"u8;
internal static readonly @string someBodyˢ = "Some body"u8;
internal static readonly @string httpFooComˢ = "http://foo.com/"u8;

internal partial struct TestRecorder_type /*dyn*/ {
    internal @string name;
    internal Action<http.ResponseWriter, ж<http.Request>> h;
    internal slice<Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>> checks;
}

public static void TestRecorder(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();
    // type checkFunc is a methodless func type — rendered inline as its base delegate

    slice<Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>> check(params Span<Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>> fnsʗp) {
        var fns = fnsʗp.slice();
        return fns;
    }
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasStatus(nint wantCode) => (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            if ((~rec).Code != wantCode) {
                return fmt.Errorf("Status = %d; want %d"u8, (~rec).Code, wantCode);
            }
            return default!;
        });
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasResultStatus(@string want) => (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            if ((~rec.Result()).Status != want) {
                return fmt.Errorf("Result().Status = %q; want %q"u8, (~rec.Result()).Status, want);
            }
            return default!;
        });
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasResultStatusCode(nint wantCode) => (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            if ((~rec.Result()).StatusCode != wantCode) {
                return fmt.Errorf("Result().StatusCode = %d; want %d"u8, (~rec.Result()).StatusCode, wantCode);
            }
            return default!;
        });
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasResultContents(@string want) => (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            var (contentBytes, err) = io.ReadAll((~rec.Result()).Body);
            if (err != default!) {
                return err;
            }
            @string contents = ((@string)contentBytes);
            if (contents != want) {
                return fmt.Errorf("Result().Body = %s; want %s"u8, contents, want);
            }
            return default!;
        });
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasContents(@string want) => (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            if ((~rec).Body.String() != want) {
                return fmt.Errorf("wrote = %q; want %q"u8, (~rec).Body.String(), want);
            }
            return default!;
        });
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasFlush(bool want) => (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            if ((~rec).Flushed != want) {
                return fmt.Errorf("Flushed = %v; want %v"u8, (~rec).Flushed, want);
            }
            return default!;
        });
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasOldHeader(@string key, @string want) => (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            {
                @string got = (~rec).HeaderMap.Get(key); if (got != want) {
                    return fmt.Errorf("HeaderMap header %s = %q; want %q"u8, key, got, want);
                }
            }
            return default!;
        });
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasHeader(@string key, @string want) => (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            {
                @string got = (~rec.Result()).Header.Get(key); if (got != want) {
                    return fmt.Errorf("final header %s = %q; want %q"u8, key, got, want);
                }
            }
            return default!;
        });
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasNotHeaders(params ꓸꓸꓸstring keysʗp) {
        var keys = keysʗp.slice();
        var keysʗ1 = keys;
        return (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            foreach (var (_, k) in keysʗ1) {
                var (v, ok) = (~rec.Result()).Header[http.CanonicalHeaderKey(k), ꟷ];
                if (ok) {
                    return fmt.Errorf("unexpected header %s with value %q"u8, k, v);
                }
            }
            return default!;
        });
    }
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasTrailer(@string key, @string want) => (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            {
                @string got = (~rec.Result()).Trailer.Get(key); if (got != want) {
                    return fmt.Errorf("trailer %s = %q; want %q"u8, key, got, want);
                }
            }
            return default!;
        });
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasNotTrailers(params ꓸꓸꓸstring keysʗp) {
        var keys = keysʗp.slice();
        var keysʗ2 = keys;
        return (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            var trailers = rec.Result().Value.Trailer;
            foreach (var (_, k) in keysʗ2) {
                var (_, ok) = trailers[http.CanonicalHeaderKey(k), ꟷ];
                if (ok) {
                    return fmt.Errorf("unexpected trailer %s"u8, k);
                }
            }
            return default!;
        });
    }
    Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error> hasContentLength(int64 length) => (Func<ж<global::go.net.http.httptest_package.ResponseRecorder>, error>)(error (ж<global::go.net.http.httptest_package.ResponseRecorder> rec) => {
            {
                var got = rec.Result().Value.ContentLength; if (got != length) {
                    return fmt.Errorf("ContentLength = %d; want %d"u8, got, length);
                }
            }
            return default!;
        });














    foreach (var (_, vᴛ1) in new TestRecorder_type[]{
        new(
            "200 default"u8,
            (http.ResponseWriter w, ж<http.Request> r) => {
            },
            check(hasStatus(200), hasContents(""u8))
        ),
        new(
            "first code only"u8,
            (http.ResponseWriter w, ж<http.Request> r) => {
                w.WriteHeader(201);
                w.WriteHeader(202);
                w.Write(slice<byte>("hi"u8));
            },
            check(hasStatus(201), hasContents("hi"u8))
        ),
        new(
            "write sends 200"u8,
            (http.ResponseWriter w, ж<http.Request> r) => {
                w.Write(slice<byte>("hi first"u8));
                w.WriteHeader(201);
                w.WriteHeader(202);
            },
            check(hasStatus(200), hasContents(hiFirstˢ), hasFlush(false))
        ),
        new(
            "write string"u8,
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
            "flush"u8,
            (http.ResponseWriter w, ж<http.Request> r) => {
                w._<http.Flusher>().Flush(); // also sends a 200
                w.WriteHeader(201);
            },
            check(hasStatus(200), hasFlush(true), hasContentLength(-1))
        ),
        new(
            "Content-Type detection"u8,
            (http.ResponseWriter w, ж<http.Request> r) => {
                io.WriteString(new httptest_test_package.http_ResponseWriterᴠWriter(w), htmlˢ);
            },
            check(hasHeader(contentTypeˢ, textHtmlCharsetUtf8ˢ))
        ),
        new(
            "no Content-Type detection with Transfer-Encoding"u8,
            (http.ResponseWriter w, ж<http.Request> r) => {
                w.Header().Set(transferEncodingˢ, someEncodingˢ);
                io.WriteString(new httptest_test_package.http_ResponseWriterᴠWriter(w), htmlˢ);
            },
            check(hasHeader(contentTypeˢ, ""u8)) // no header

        ),
        new(
            "no Content-Type detection if set explicitly"u8,
            (http.ResponseWriter w, ж<http.Request> r) => {
                w.Header().Set(contentTypeˢ, someTypeˢ);
                io.WriteString(new httptest_test_package.http_ResponseWriterᴠWriter(w), htmlˢ);
            },
            check(hasHeader(contentTypeˢ, someTypeˢ))
        ),
        new(
            "Content-Type detection doesn't crash if HeaderMap is nil"u8,
            (http.ResponseWriter w, ж<http.Request> r) => {
                // Act as if the user wrote new(httptest.ResponseRecorder)
                // rather than using NewRecorder (which initializes
                // HeaderMap)
                w._<ж<global::go.net.http.httptest_package.ResponseRecorder>>().Value.HeaderMap = default!;
                io.WriteString(new httptest_test_package.http_ResponseWriterᴠWriter(w), htmlˢ);
            },
            check(hasHeader(contentTypeˢ, textHtmlCharsetUtf8ˢ))
        ),
        new(
            "Header is not changed after write"u8,
            (http.ResponseWriter w, ж<http.Request> r) => {
                var hdr = w.Header();
                hdr.Set(keyˢ, correctˢ);
                w.WriteHeader(200);
                hdr.Set(keyˢ, incorrectˢ);
            },
            check(hasHeader(keyˢ, correctˢ))
        ),
        new(
            "Trailer headers are correctly recorded"u8,
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
            "Header set without any write"u8, // Issue 15560

            (http.ResponseWriter w, ж<http.Request> r) => {
                w.Header().Set(xFooˢ, "1"u8);
                // Simulate somebody using
                // new(ResponseRecorder) instead of
                // using the constructor which sets
                // this to 200
                w._<ж<global::go.net.http.httptest_package.ResponseRecorder>>().Value.Code = 0;
            },
            check(
                hasOldHeader(xFooˢ, "1"u8),
                hasStatus(0),
                hasHeader(xFooˢ, "1"u8),
                hasResultStatus("200 OK"u8),
                hasResultStatusCode(200))
        ),
        new(
            "HeaderMap vs FinalHeaders"u8, // more for Issue 15560

            (http.ResponseWriter w, ж<http.Request> r) => {
                var h = w.Header();
                h.Set(xFooˢ, "1"u8);
                w.Write(slice<byte>("hi"u8));
                h.Set(xFooˢ, "2"u8);
                h.Set(xBarˢ, "2"u8);
            },
            check(
                hasOldHeader(xFooˢ, "2"u8),
                hasOldHeader(xBarˢ, "2"u8),
                hasHeader(xFooˢ, "1"u8),
                hasNotHeaders(xBarˢ))
        ),
        new(
            "setting Content-Length header"u8,
            (http.ResponseWriter w, ж<http.Request> r) => {
                @string body = someBodyˢ;
                @string contentLength = fmt.Sprintf("%d"u8, len(body));
                w.Header().Set(contentLengthˢ, contentLength);
                io.WriteString(new httptest_test_package.http_ResponseWriterᴠWriter(w), body);
            },
            check(hasStatus(200), hasContents(someBodyˢ), hasContentLength(9))
        ),
        new(
            "nil ResponseRecorder.Body"u8, // Issue 26642

            (http.ResponseWriter w, ж<http.Request> r) => {
                w._<ж<global::go.net.http.httptest_package.ResponseRecorder>>().Value.Body = default!;
                io.WriteString(new httptest_test_package.http_ResponseWriterᴠWriter(w), "hi"u8);
            },
            check(hasResultContents(""u8)) // check we don't crash reading the body

        )
    }.array()) {
        ref var tt = ref heap(new TestRecorder_type(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            var (r, _) = http.NewRequest(getˢ, httpFooComˢ, default!);
            var h = NilSafeDelegateConversion<http.HandlerFunc, Action<http.ResponseWriter, ж<http.Request>>>(ttʗ1.h);
            var rec = NewRecorder();
            h.ServeHTTP(new httptest_test_package.httptest_ResponseRecorderжResponseWriter(rec), r);
            foreach (var (_, checkΔ1) in ttʗ1.checks) {
                {
                    var err = checkΔ1(rec); if (err != default!) {
                        tΔ1.Error(err);
                    }
                }
            }
        });
    }
}

internal partial struct TestParseContentLength_tests /*dyn*/ {
    internal @string cl;
    internal int64 want;
}

// issue 39017 - disallow Content-Length values such as "+3"
public static void TestParseContentLength(ж<testing.T> Ꮡt) {
    var tests = new TestParseContentLength_tests[]{
        new(
            cl: "3"u8,
            want: 3
        ),
        new(
            cl: "+3"u8,
            want: -1
        ),
        new(
            cl: "-3"u8,
            want: -1
        ),
        new(
            cl: "9223372036854775807"u8, // max int64, for safe conversion before returning

            want: 9223372036854775807L
        ),
        new(
            cl: "9223372036854775808"u8,
            want: -1
        )
    }.slice();
    foreach (var (_, tt) in tests) {
        {
            var got = parseContentLength(tt.cl); if (got != tt.want) {
                Ꮡt.Errorf("%q:\n\tgot=%d\n\twant=%d"u8, tt.cl, got, tt.want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedAPanicˢ = (@string)"Expected a panic"u8;
internal static readonly @string httpExampleOrgˢ = "http://example.org/"u8;

// Ensure that httptest.Recorder panics when given a non-3 digit (XXX)
// status HTTP code. See https://golang.org/issues/45353
public static void TestRecorderPanicsOnNonXXXStatusCode(ж<testing.T> Ꮡt) {
    var badCodes = new nint[]{
        -100, 0, 99, 1000, 20000
    }.slice();
    foreach (var (_, badCode) in badCodes) {
        nint badCodeΔ1 = badCode;
        Ꮡt.Run(fmt.Sprintf("Code=%d"u8, badCodeΔ1), (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                defer(() => {
                    {
                        var rΔ1 = recover(); if (rΔ1 == default!) {
                            tΔ1.Fatal(expectedAPanicˢ);
                        }
                    }
                }, ref ᒐ);
                void handler(http.ResponseWriter rwΔ1, ж<http.Request> _Δp1) {
                    rwΔ1.WriteHeader(badCodeΔ1);
                }
                var (r, _) = http.NewRequest(getˢ, httpExampleOrgˢ, default!);
                var rw = NewRecorder();
                handler(new httptest_test_package.httptest_ResponseRecorderжResponseWriter(rw), r);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

} // end httptest_internal_test_package
