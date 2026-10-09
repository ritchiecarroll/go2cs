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

// TODO(mattn):
//	test ProxyAuth

partial struct cacheKeysTestsᴛ1 /*dyn*/ {
    internal @string proxy;
    internal @string scheme;
    internal @string addr;
    internal @string key;
}
internal static slice<cacheKeysTestsᴛ1> cacheKeysTests = new cacheKeysTestsᴛ1[]{
    new(""u8, "http"u8, "foo.com"u8, "|http|foo.com"u8),
    new(""u8, "https"u8, "foo.com"u8, "|https|foo.com"u8),
    new("http://foo.com"u8, "http"u8, "foo.com"u8, "http://foo.com|http|"u8),
    new("http://foo.com"u8, "https"u8, "foo.com"u8, "http://foo.com|https|foo.com"u8)
}.slice();

public static void TestCacheKeys(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in cacheKeysTests) {
        ж<url.URL> proxy = default!;
        if (tt.proxy != ""u8) {
            var (u, err) = url.Parse(tt.proxy);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            proxy = u;
        }
        var cm = new connectMethod(proxyURL: proxy, targetScheme: tt.scheme, targetAddr: tt.addr);
        {
            @string got = cm.key().String(); if (got != tt.key) {
                Ꮡt.Fatalf("{%q, %q, %q} cache key = %q; want %q"u8, tt.proxy, tt.scheme, tt.addr, got, tt.key);
            }
        }
    }
}

public static void ResetProxyEnv() {
    foreach (var (_, v) in new @string[]{"HTTP_PROXY"u8, "http_proxy"u8, "NO_PROXY"u8, "no_proxy"u8, "REQUEST_METHOD"u8}.slice()) {
        os.Unsetenv(v);
    }
    ResetCachedEnvironment();
}

} // end http_internal_test_package
