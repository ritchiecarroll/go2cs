// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !aix && !darwin && !freebsd && !openbsd && !solaris && !wasip1 && !windows && !(linux && amd64) && !plan9
namespace go;

partial class runtime_package {

//go:wasmimport gojs runtime.walltime
internal static partial (int64 sec, int32 nsec) walltime();

} // end runtime_package
