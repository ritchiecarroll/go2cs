// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !wasm
namespace go.net;

using context = context_package;
using net = net_package;

partial class http_package {

internal static Func<context.Context, @string, @string, (net.Conn, error)> defaultTransportDialContext(ж<net.Dialer> dialer) {
    return dialer.DialContext;
}

} // end http_package
