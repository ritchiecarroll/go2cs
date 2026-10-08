// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using url = go.net.url_package;
using go.net;

partial class http_package {

// A CookieJar manages storage and use of cookies in HTTP requests.
//
// Implementations of CookieJar must be safe for concurrent use by multiple
// goroutines.
//
// The net/http/cookiejar package provides a CookieJar implementation.
partial interface CookieJar {
    // SetCookies handles the receipt of the cookies in a reply for the
    // given URL.  It may or may not choose to save the cookies, depending
    // on the jar's policy and implementation.
    void SetCookies(ж<url.URL> u, slice<ж<ΔCookie>> cookies);
    // Cookies returns the cookies to send in a request for the given URL.
    // It is up to the implementation to honor the standard cookie use
    // restrictions such as in RFC 6265.
    slice<ж<ΔCookie>> Cookies(ж<url.URL> u);
}

} // end http_package
