// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package httpguts provides functions implementing various details
// of the HTTP specification.
//
// This package is shared by the standard library (which vendors it)
// and x/net/http2. It comes with no API stability promise.
namespace go.vendor.golang.org.x.net.http;

using textproto = go.net.textproto_package;
using strings = strings_package;
using go.net;

partial class httpguts_package {

// ValidTrailerHeader reports whether name is a valid header field name to appear
// in trailers.
// See RFC 7230, Section 4.1.2
public static bool ValidTrailerHeader(@string name) {
    name = textproto.CanonicalMIMEHeaderKey(name);
    if (strings.HasPrefix(name, "If-") || badTrailer[name]) {
        return false;
    }
    return true;
}

[GoLazy] partial class lazyᴛbadTrailer { internal static map<@string, bool> badTrailer = new map<@string, bool>{
    ["Authorization"] = true,
    ["Cache-Control"] = true,
    ["Connection"] = true,
    ["Content-Encoding"] = true,
    ["Content-Length"] = true,
    ["Content-Range"] = true,
    ["Content-Type"] = true,
    ["Expect"] = true,
    ["Host"] = true,
    ["Keep-Alive"] = true,
    ["Max-Forwards"] = true,
    ["Pragma"] = true,
    ["Proxy-Authenticate"] = true,
    ["Proxy-Authorization"] = true,
    ["Proxy-Connection"] = true,
    ["Range"] = true,
    ["Realm"] = true,
    ["Te"] = true,
    ["Trailer"] = true,
    ["Transfer-Encoding"] = true,
    ["Www-Authenticate"] = true
}; }

} // end httpguts_package
