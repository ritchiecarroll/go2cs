// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

partial class http_package {

// Common HTTP methods.
//
// Unless otherwise noted, these are defined in RFC 7231 section 4.3.
public static ref readonly @string MethodGet => ref MethodGetᵇ;
private static readonly @string MethodGetᵇ = "GET"u8;
public static ref readonly @string MethodHead => ref MethodHeadᵇ;
private static readonly @string MethodHeadᵇ = "HEAD"u8;
public static ref readonly @string MethodPost => ref MethodPostᵇ;
private static readonly @string MethodPostᵇ = "POST"u8;
public static ref readonly @string MethodPut => ref MethodPutᵇ;
private static readonly @string MethodPutᵇ = "PUT"u8;
public static ref readonly @string MethodPatch => ref MethodPatchᵇ;
private static readonly @string MethodPatchᵇ = "PATCH"u8; // RFC 5789
public static ref readonly @string MethodDelete => ref MethodDeleteᵇ;
private static readonly @string MethodDeleteᵇ = "DELETE"u8;
public static ref readonly @string MethodConnect => ref MethodConnectᵇ;
private static readonly @string MethodConnectᵇ = "CONNECT"u8;
public static ref readonly @string MethodOptions => ref MethodOptionsᵇ;
private static readonly @string MethodOptionsᵇ = "OPTIONS"u8;
public static ref readonly @string MethodTrace => ref MethodTraceᵇ;
private static readonly @string MethodTraceᵇ = "TRACE"u8;

} // end http_package
