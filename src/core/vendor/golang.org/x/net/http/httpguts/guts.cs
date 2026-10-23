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
    if (strings.HasPrefix(name, "If-"u8) || badTrailer[name]) {
        return false;
    }
    return true;
}

[GoLazy] partial class lazyᴛbadTrailer { internal static map<@string, bool> badTrailer = new map<@string, bool>{
    ["Authorization"u8] = true,
    ["Cache-Control"u8] = true,
    ["Connection"u8] = true,
    ["Content-Encoding"u8] = true,
    ["Content-Length"u8] = true,
    ["Content-Range"u8] = true,
    ["Content-Type"u8] = true,
    ["Expect"u8] = true,
    ["Host"u8] = true,
    ["Keep-Alive"u8] = true,
    ["Max-Forwards"u8] = true,
    ["Pragma"u8] = true,
    ["Proxy-Authenticate"u8] = true,
    ["Proxy-Authorization"u8] = true,
    ["Proxy-Connection"u8] = true,
    ["Range"u8] = true,
    ["Realm"u8] = true,
    ["Te"u8] = true,
    ["Trailer"u8] = true,
    ["Transfer-Encoding"u8] = true,
    ["Www-Authenticate"u8] = true
}; }

} // end httpguts_package
