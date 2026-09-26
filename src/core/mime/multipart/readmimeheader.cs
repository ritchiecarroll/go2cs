// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.mime;

using textproto = net.textproto_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for go:linkname
using net;

partial class multipart_package {

// readMIMEHeader is defined in package [net/textproto].
//
//go:linkname readMIMEHeader net/textproto.readMIMEHeader
[global::System.Diagnostics.StackTraceHidden] internal static (textproto.MIMEHeader, error) readMIMEHeader(ж<textproto.Reader> r, int64 maxMemory, int64 maxHeaders) {
    var (ᴛ1, ᴛ2) = textproto.readMIMEHeader(r, maxMemory, maxHeaders);
    return (ᴛ1, ᴛ2);
}

} // end multipart_package
