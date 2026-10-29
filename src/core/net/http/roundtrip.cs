// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !js
namespace go.net;

/* blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) */ // for linkname

partial class http_package {

// RoundTrip should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/erda-project/erda-infra
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname badRoundTrip net/http.(*Transport).RoundTrip
internal static partial (ж<Response>, error) badRoundTrip(ж<Transport> _Δp0, ж<Request> _Δp1);

// RoundTrip implements the [RoundTripper] interface.
//
// For higher-level HTTP client support (such as handling of cookies
// and redirects), see [Get], [Post], and the [Client] type.
//
// Like the RoundTripper interface, the error types returned
// by RoundTrip are unspecified.
public static (ж<Response>, error) RoundTrip(this ж<Transport> Ꮡt, ж<Request> req) {
    if (Ꮡt == nil) {
        throw panic("transport is nil");
    }
    return Ꮡt.roundTrip(req);
}

} // end http_package
