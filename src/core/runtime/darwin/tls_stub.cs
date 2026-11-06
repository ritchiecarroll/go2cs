// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (windows && !amd64) || !windows
namespace go;

partial class runtime_package {

//go:nosplit
internal static void osSetupTLS(ref m mp) { }

} // end runtime_package
