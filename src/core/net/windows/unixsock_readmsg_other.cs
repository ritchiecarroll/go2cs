// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build js || wasip1 || windows
namespace go;

partial class net_package {

internal static UntypedInt readMsgFlags => 0;

internal static void setReadMsgCloseOnExec(slice<byte> oob) { }

} // end net_package
