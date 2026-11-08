// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.http;

using context = context_package;
using tls = crypto.tls_package;
using io = io_package;
using http = go.net.http_package;
using url = go.net.url_package;
using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using crypto;
using go.net;
using static go.net.http.httptest_package;

partial class httptest_internal_test_package {

public static void TestNewRequest(ж<testing.T> t) {
    var got = NewRequest(getˢ, "/", default);
    var want = Ꮡ(new http.Request(
        Method: "GET",
        Host: "example.com",
        URL: Ꮡ(new url.URL(Path: "/")),
        Header: new httpꓸHeader(new map<@string, slice<@string>>{}),
        Proto: "HTTP/1.1",
        ProtoMajor: 1,
        ProtoMinor: 1,
        RemoteAddr: "192.0.2.1:1234",
        RequestURI: "/"
    ));
    got.Body = default; // before DeepEqual
    want = want.WithContext(context.Background());
    if (!reflect.DeepEqual(got.OrTypedNil(), want.OrTypedNil())) {
        t.Errorf("Request mismatch:\n got: %#v\nwant: %#v", got.OrTypedNil(), want.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;

internal partial struct TestNewRequestWithContext_type /*dyn*/ {
    internal @string name;
    internal @string method, uri;
    internal io.Reader body;
    internal ж<http.Request> want;
    internal @string wantBody;
}

internal partial struct TestNewRequestWithContext_typeᴛ1 /*dyn*/ {
    /*embed*/ public io_package.Reader Reader;
}

public static void TestNewRequestWithContext(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(new TestNewRequestWithContext_type[]{
        new(
            name: "Empty method means GET",
            method: "",
            uri: "/",
            body: default,
            want: Ꮡ(new http.Request(
                Method: "GET",
                Host: "example.com",
                URL: Ꮡ(new url.URL(Path: "/")),
                Header: new httpꓸHeader(new map<@string, slice<@string>>{}),
                Proto: "HTTP/1.1",
                ProtoMajor: 1,
                ProtoMinor: 1,
                RemoteAddr: "192.0.2.1:1234",
                RequestURI: "/"
            )),
            wantBody: ""
        ),

        new(
            name: "GET with full URL",
            method: "GET",
            uri: "http://foo.com/path/%2f/bar/",
            body: default,
            want: Ꮡ(new http.Request(
                Method: "GET",
                Host: "foo.com",
                URL: Ꮡ(new url.URL(
                    Scheme: "http",
                    Path: "/path///bar/",
                    RawPath: "/path/%2f/bar/",
                    Host: "foo.com"
                )),
                Header: new httpꓸHeader(new map<@string, slice<@string>>{}),
                Proto: "HTTP/1.1",
                ProtoMajor: 1,
                ProtoMinor: 1,
                RemoteAddr: "192.0.2.1:1234",
                RequestURI: "http://foo.com/path/%2f/bar/"
            )),
            wantBody: ""
        ),

        new(
            name: "GET with full https URL",
            method: "GET",
            uri: "https://foo.com/path/",
            body: default,
            want: Ꮡ(new http.Request(
                Method: "GET",
                Host: "foo.com",
                URL: Ꮡ(new url.URL(
                    Scheme: "https",
                    Path: "/path/",
                    Host: "foo.com"
                )),
                Header: new httpꓸHeader(new map<@string, slice<@string>>{}),
                Proto: "HTTP/1.1",
                ProtoMajor: 1,
                ProtoMinor: 1,
                RemoteAddr: "192.0.2.1:1234",
                RequestURI: "https://foo.com/path/",
                TLS: Ꮡ(new tlsꓸConnectionState(
                    Version: tls.VersionTLS12,
                    HandshakeComplete: true,
                    ServerName: "foo.com"
                ))
            )),
            wantBody: ""
        ),

        new(
            name: "Post with known length",
            method: "POST",
            uri: "/",
            body: new httptest_test_package.strings_ReaderжReader(strings.NewReader(fooˢ)),
            want: Ꮡ(new http.Request(
                Method: "POST",
                Host: "example.com",
                URL: Ꮡ(new url.URL(Path: "/")),
                Header: new httpꓸHeader(new map<@string, slice<@string>>{}),
                Proto: "HTTP/1.1",
                ContentLength: 3,
                ProtoMajor: 1,
                ProtoMinor: 1,
                RemoteAddr: "192.0.2.1:1234",
                RequestURI: "/"
            )),
            wantBody: "foo"
        ),

        new(
            name: "Post with unknown length",
            method: "POST",
            uri: "/",
            body: new TestNewRequestWithContext_typeᴛ1(new httptest_test_package.strings_ReaderжReader(strings.NewReader(fooˢ))),
            want: Ꮡ(new http.Request(
                Method: "POST",
                Host: "example.com",
                URL: Ꮡ(new url.URL(Path: "/")),
                Header: new httpꓸHeader(new map<@string, slice<@string>>{}),
                Proto: "HTTP/1.1",
                ContentLength: -1,
                ProtoMajor: 1,
                ProtoMinor: 1,
                RemoteAddr: "192.0.2.1:1234",
                RequestURI: "/"
            )),
            wantBody: "foo"
        ),

        new(
            name: "Post with NoBody",
            method: "POST",
            uri: "/",
            body: new httptest_test_package.http_noBodyᴠReader(http.NoBody),
            want: Ꮡ(new http.Request(
                Method: "POST",
                Host: "example.com",
                URL: Ꮡ(new url.URL(Path: "/")),
                Header: new httpꓸHeader(new map<@string, slice<@string>>{}),
                Proto: "HTTP/1.1",
                ProtoMajor: 1,
                ProtoMinor: 1,
                RemoteAddr: "192.0.2.1:1234",
                RequestURI: "/"
            ))
        ),

        new(
            name: "OPTIONS *",
            method: "OPTIONS",
            uri: "*",
            want: Ꮡ(new http.Request(
                Method: "OPTIONS",
                Host: "example.com",
                URL: Ꮡ(new url.URL(Path: "*")),
                Header: new httpꓸHeader(new map<@string, slice<@string>>{}),
                Proto: "HTTP/1.1",
                ProtoMajor: 1,
                ProtoMinor: 1,
                RemoteAddr: "192.0.2.1:1234",
                RequestURI: "*"
            ))
        )
    }.array())) {
        ref var tt = ref heap(new TestNewRequestWithContext_type(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            var got = NewRequestWithContext(context.Background(), ttʗ1.method, ttʗ1.uri, ttʗ1.body);
            var (slurp, err) = io.ReadAll(got.Body);
            if (err != nil) {
                tΔ1.Errorf("ReadAll: %v", err);
            }
            if (((sstring)slurp) != ttʗ1.wantBody) {
                tΔ1.Errorf("Body = %q; want %q", slurp, ttʗ1.wantBody);
            }
            ttʗ1.want = ttʗ1.want.WithContext(context.Background());
            got.Body = default; // before DeepEqual
            if (!reflect.DeepEqual(got.URL.OrTypedNil(), ttʗ1.want.URL.OrTypedNil())) {
                tΔ1.Errorf("Request.URL mismatch:\n got: %#v\nwant: %#v", got.URL.OrTypedNil(), ttʗ1.want.URL.OrTypedNil());
            }
            if (!reflect.DeepEqual(got.Header, ttʗ1.want.Header)) {
                tΔ1.Errorf("Request.Header mismatch:\n got: %#v\nwant: %#v", got.Header, ttʗ1.want.Header);
            }
            if (!reflect.DeepEqual(got.TLS.OrTypedNil(), ttʗ1.want.TLS.OrTypedNil())) {
                tΔ1.Errorf("Request.TLS mismatch:\n got: %#v\nwant: %#v", got.TLS.OrTypedNil(), ttʗ1.want.TLS.OrTypedNil());
            }
            if (!reflect.DeepEqual(got.OrTypedNil(), ttʗ1.want.OrTypedNil())) {
                tΔ1.Errorf("Request mismatch:\n got: %#v\nwant: %#v", got.OrTypedNil(), ttʗ1.want.OrTypedNil());
            }
        });
    }
}

} // end httptest_internal_test_package
