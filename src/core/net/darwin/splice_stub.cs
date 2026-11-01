// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !linux
namespace go;

using Δio = io_package;

partial class net_package {

internal static (int64, error, bool) spliceFrom(ж<netFD> _Δp0, Δio.Reader _Δp1) {
    return (0, default, false);
}

internal static (int64, error, bool) spliceTo(Δio.Writer _Δp0, ж<netFD> _Δp1) {
    return (0, default, false);
}

} // end net_package
