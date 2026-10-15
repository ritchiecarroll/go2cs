// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file holds stub versions of the cgo functions called on Unix systems.
// We build this file:
// - if using the netgo build tag on a Unix system
// - on a Unix system without the cgo resolver functions
//   (Darwin always provides the cgo functions, in cgo_unix_syscall.go)
// - on wasip1, where cgo is never available
//go:build (netgo && unix) || (unix && !cgo && !darwin) || js || wasip1
namespace go;

using context = context_package;

partial class net_package {

// cgoAvailable set to false to indicate that the cgo resolver
// is not available on this system.
internal const bool cgoAvailable = false;

internal static (slice<@string> addrs, error err) cgoLookupHost(context.Context ctx, @string name) {
    throw panic("cgo stub: cgo not available");
}

internal static (nint port, error err) cgoLookupPort(context.Context ctx, @string network, @string service) {
    throw panic("cgo stub: cgo not available");
}

internal static (slice<IPAddr> addrs, error err) cgoLookupIP(context.Context ctx, @string network, @string name) {
    throw panic("cgo stub: cgo not available");
}

internal static (@string cname, error err) cgoLookupCNAME(context.Context ctx, @string name) {
    throw panic("cgo stub: cgo not available");
}

internal static (slice<@string> ptrs, error err) cgoLookupPTR(context.Context ctx, @string addr) {
    throw panic("cgo stub: cgo not available");
}

} // end net_package
