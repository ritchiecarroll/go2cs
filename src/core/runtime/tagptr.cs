// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class runtime_package {

partial struct taggedPointer /*num:uint64*/;

// minTagBits is the minimum number of tag bits that we expect.
internal static UntypedInt minTagBits => 10;

} // end runtime_package
