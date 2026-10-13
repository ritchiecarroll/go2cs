// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

partial class http_package {

// Common HTTP methods.
//
// Unless otherwise noted, these are defined in RFC 7231 section 4.3.
public static readonly @string MethodGet = "GET"u8;
public static readonly @string MethodHead = "HEAD"u8;
public static readonly @string MethodPost = "POST"u8;
public static readonly @string MethodPut = "PUT"u8;
public static readonly @string MethodPatch = "PATCH"u8; // RFC 5789
public static readonly @string MethodDelete = "DELETE"u8;
public static readonly @string MethodConnect = "CONNECT"u8;
public static readonly @string MethodOptions = "OPTIONS"u8;
public static readonly @string MethodTrace = "TRACE"u8;

} // end http_package
