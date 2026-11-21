// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

/* import _ "unsafe" (nothing to initialize) */ // for linkname

partial class cgroup_package {

// Functions below pushed from runtime.

//go:linkname throw
internal static partial void @throw(@string s);

} // end cgroup_package
