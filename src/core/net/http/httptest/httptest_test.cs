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

public static void TestNewRequest(ж<testing.T> Ꮡt) {
    var got = NewRequest(getˢ, "/"u8, default!);
    var want = Ꮡ(new http.Request(
        Method: "GET"u8,
        Host: "example.com"u8,
        URL: Ꮡ(new url.URL(Path: "/"u8)),
        Header: new httpꓸHeader(new map<@string, slice<@string>>{}),
        Proto: "HTTP/1.1"u8,
        ProtoMajor: 1,
        ProtoMinor: 1,
        RemoteAddr: "192.0.2.1:1234"u8,
        RequestURI: "/"u8
    ));
    got.Value.Body = default!; // before DeepEqual
    want = want.WithContext(context.Background());
    if (!reflect.DeepEqual(got.OrTypedNil(), want.OrTypedNil())) {
        Ꮡt.Errorf("Request mismatch:\n got: %#v\nwant: %#v"u8, got.OrTypedNil(), want.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = "foo"u8;

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

public static void TestNewRequestWithContext(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in new TestNewRequestWithContext_type[]{
        new(
            name: "Empty method means GET"u8,
            method: ""u8,
            uri: "/"u8,
            body: default!,
            want: Ꮡ(new http.Request(
                Method: "GET"u8,
                Host: "example.com"u8,
                URL: Ꮡ(new url.URL(Path: "/"u8)),
                Header: new httpꓸHeader(new map<@string, slice<@string>>{}),
                Proto: "HTTP/1.1"u8,
                ProtoMajor: 1,
                ProtoMinor: 1,
                RemoteAddr: "192.0.2.1:1234"u8,
                RequestURI: "/"u8
            )),
            wantBody: ""u8
        ),
        new(
            name: "GET with full URL"u8,
            method: "GET"u8,
            uri: "http://foo.com/path/%2f/bar/"u8,
            body: default!,
            want: Ꮡ(new http.Request(
                Method: "GET"u8,
                Host: "foo.com"u8,
                URL: Ꮡ(new url.URL(
                    Scheme: "http"u8,
                    Path: "/path///bar/"u8,
                    RawPath: "/path/%2f/bar/"u8,
                    Host: "foo.com"u8
                )),
                Header: new httpꓸHeader(new map<@string, slice<@string>>{}),
                Proto: "HTTP/1.1"u8,
                ProtoMajor: 1,
                ProtoMinor: 1,
                RemoteAddr: "192.0.2.1:1234"u8,
                RequestURI: "http://foo.com/path/%2f/bar/"u8
            )),
            wantBody: ""u8
        ),
        new(
            name: "GET with full https URL"u8,
            method: "GET"u8,
            uri: "https://foo.com/path/"u8,
            body: default!,
            want: Ꮡ(new http.Request(
                Method: "GET"u8,
                Host: "foo.com"u8,
                URL: Ꮡ(new url.URL(
                    Scheme: "https"u8,
                    Path: "/path/"u8,
                    Host: "foo.com"u8
                )),
                Header: new httpꓸHeader(new map<@string, slice<@string>>{}),
                Proto: "HTTP/1.1"u8,
                ProtoMajor: 1,
                ProtoMinor: 1,
                RemoteAddr: "192.0.2.1:1234"u8,
                RequestURI: "https://foo.com/path/"u8,
                TLS: Ꮡ(new tlsꓸConnectionState(
                    Version: tls.VersionTLS12,
                    HandshakeComplete: true,
                    ServerName: "foo.com"u8
                ))
            )),
            wantBody: ""u8
        ),
        new(
            name: "Post with known length"u8,
            method: "POST"u8,
            uri: "/"u8,
            body: new httptest_test_package.strings_ReaderжReader(strings.NewReader(fooˢ)),
            want: Ꮡ(new http.Request(
                Method: "POST"u8,
                Host: "example.com"u8,
                URL: Ꮡ(new url.URL(Path: "/"u8)),
                Header: new httpꓸHeader(new map<@string, slice<@string>>{}),
                Proto: "HTTP/1.1"u8,
                ContentLength: 3,
                ProtoMajor: 1,
                ProtoMinor: 1,
                RemoteAddr: "192.0.2.1:1234"u8,
                RequestURI: "/"u8
            )),
            wantBody: "foo"u8
        ),
        new(
            name: "Post with unknown length"u8,
            method: "POST"u8,
            uri: "/"u8,
            body: new TestNewRequestWithContext_typeᴛ1(new httptest_test_package.strings_ReaderжReader(strings.NewReader(fooˢ))),
            want: Ꮡ(new http.Request(
                Method: "POST"u8,
                Host: "example.com"u8,
                URL: Ꮡ(new url.URL(Path: "/"u8)),
                Header: new httpꓸHeader(new map<@string, slice<@string>>{}),
                Proto: "HTTP/1.1"u8,
                ContentLength: -1,
                ProtoMajor: 1,
                ProtoMinor: 1,
                RemoteAddr: "192.0.2.1:1234"u8,
                RequestURI: "/"u8
            )),
            wantBody: "foo"u8
        ),
        new(
            name: "OPTIONS *"u8,
            method: "OPTIONS"u8,
            uri: "*"u8,
            want: Ꮡ(new http.Request(
                Method: "OPTIONS"u8,
                Host: "example.com"u8,
                URL: Ꮡ(new url.URL(Path: "*"u8)),
                Header: new httpꓸHeader(new map<@string, slice<@string>>{}),
                Proto: "HTTP/1.1"u8,
                ProtoMajor: 1,
                ProtoMinor: 1,
                RemoteAddr: "192.0.2.1:1234"u8,
                RequestURI: "*"u8
            ))
        )
    }.array()) {
        ref var tt = ref heap(new TestNewRequestWithContext_type(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            var got = NewRequestWithContext(context.Background(), ttʗ1.method, ttʗ1.uri, ttʗ1.body);
            var (slurp, err) = io.ReadAll((~got).Body);
            if (err != default!) {
                tΔ1.Errorf("ReadAll: %v"u8, err);
            }
            if (((sstring)slurp) != ttʗ1.wantBody) {
                tΔ1.Errorf("Body = %q; want %q"u8, slurp, ttʗ1.wantBody);
            }
            ttʗ1.want = ttʗ1.want.WithContext(context.Background());
            got.Value.Body = default!; // before DeepEqual
            if (!reflect.DeepEqual((~got).URL.OrTypedNil(), (~ttʗ1.want).URL.OrTypedNil())) {
                tΔ1.Errorf("Request.URL mismatch:\n got: %#v\nwant: %#v"u8, (~got).URL.OrTypedNil(), (~ttʗ1.want).URL.OrTypedNil());
            }
            if (!reflect.DeepEqual((~got).Header, (~ttʗ1.want).Header)) {
                tΔ1.Errorf("Request.Header mismatch:\n got: %#v\nwant: %#v"u8, (~got).Header, (~ttʗ1.want).Header);
            }
            if (!reflect.DeepEqual((~got).TLS.OrTypedNil(), (~ttʗ1.want).TLS.OrTypedNil())) {
                tΔ1.Errorf("Request.TLS mismatch:\n got: %#v\nwant: %#v"u8, (~got).TLS.OrTypedNil(), (~ttʗ1.want).TLS.OrTypedNil());
            }
            if (!reflect.DeepEqual(got.OrTypedNil(), ttʗ1.want.OrTypedNil())) {
                tΔ1.Errorf("Request mismatch:\n got: %#v\nwant: %#v"u8, got.OrTypedNil(), ttʗ1.want.OrTypedNil());
            }
        });
    }
}

} // end httptest_internal_test_package
