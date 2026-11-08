// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using url = global::go.net.url_package;
using os = os_package;
using testing = testing_package;
using global::go.net;
using static global::go.net.http_package;

partial class http_internal_test_package {

partial struct cacheKeysTestsᴛ1 /*dyn*/ {
    internal @string proxy;
    internal @string scheme;
    internal @string addr;
    internal @string key;
}
internal static slice<cacheKeysTestsᴛ1> cacheKeysTests = [
    new("", "http", "foo.com", "|http|foo.com"),
    new("", "https", "foo.com", "|https|foo.com"),
    new("http://foo.com", "http", "foo.com", "http://foo.com|http|"),
    new("http://foo.com", "https", "foo.com", "http://foo.com|https|foo.com")
];

public static void TestCacheKeys(ж<testing.T> t) {
    foreach (var (_, tt) in range(cacheKeysTests)) {
        ж<url.URL> proxy = default;
        if (tt.proxy != ""u8) {
            var (u, err) = url.Parse(tt.proxy);
            if (err != nil) {
                t.Fatal(err);
            }
            proxy = u;
        }
        var cm = new connectMethod(proxyURL: proxy, targetScheme: tt.scheme, targetAddr: tt.addr);
        @string got = cm.key().String(); if (got != tt.key) {
            t.Fatalf("{%q, %q, %q} cache key = %q; want %q", tt.proxy, tt.scheme, tt.addr, got, tt.key);
        }
    }
}

public static void ResetProxyEnv() {
    foreach (var (_, v) in range(slice<@string>(["HTTP_PROXY", "http_proxy", "NO_PROXY", "no_proxy", "REQUEST_METHOD"]))) {
        os.Unsetenv(v);
    }
    ResetCachedEnvironment();
}

partial struct proxyAuthTestsᴛ1 /*dyn*/ {
    internal @string proxy;
    internal @string key;
}
internal static slice<proxyAuthTestsᴛ1> proxyAuthTests = [
    new(
        "",
        ""
    ),
    new(
        "http://bar.com",
        ""
    ),
    new(
        "http://foo@bar.com",
        "Basic Zm9vOg=="
    ),
    new(
        "http://foo:bar@bar.com",
        "Basic Zm9vOmJhcg=="
    )
];

public static void TestProxyAuthKeys(ж<testing.T> t) {
    foreach (var (_, tt) in range(proxyAuthTests)) {
        ж<url.URL> proxy = default;
        if (tt.proxy != ""u8) {
            var (u, err) = url.Parse(tt.proxy);
            if (err != nil) {
                t.Fatal(err);
            }
            proxy = u;
        }
        var cm = new connectMethod(proxyURL: proxy);
        @string got = cm.proxyAuth(); if (got != tt.key) {
            t.Fatalf("{%q} proxyAuth key = %q; want %q", tt.proxy, got, tt.key);
        }
    }
}

} // end http_internal_test_package
