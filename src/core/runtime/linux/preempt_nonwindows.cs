// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !windows
namespace go;

partial class runtime_package {

//go:nosplit
internal static void osPreemptExtEnter(ref m mp) { }

//go:nosplit
internal static void osPreemptExtExit(ref m mp) { }

} // end runtime_package
