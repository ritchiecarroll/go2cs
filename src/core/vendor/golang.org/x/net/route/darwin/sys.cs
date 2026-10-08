// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || netbsd || openbsd
namespace go.vendor.golang.org.x.net;

using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class route_package {

internal static binaryByteOrder nativeEndian;
internal static nint kernelAlign;
internal static byte rtmVersion;
internal static map<nint, ж<wireFormat>> wireFormats;

// go2cs generated this placeholder — func init is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static nint roundup(nint l) {
    if (l == 0) {
        return kernelAlign;
    }
    return (nint)((l + kernelAlign - 1) & ~(kernelAlign - 1));
}

partial struct wireFormat {
    internal nint extOff; // offset of header extension
    internal nint bodyOff; // offset of message body
    internal Func<RIBType, slice<byte>, (Message, error)> parse;
}

} // end route_package
