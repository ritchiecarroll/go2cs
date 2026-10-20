// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class runtime_package {

// taggedPointer is a pointer with a numeric tag.
// The size of the numeric tag is GOARCH-dependent,
// currently at least 16 bits.
// This should only be used with pointers allocated outside the Go heap.
partial struct taggedPointer /*num:uint64*/;

// minTagBits is the minimum number of tag bits that we expect.
internal static UntypedInt minTagBits => 16;

// # of bits we can steal from the bottom. We enforce that all pointers
// that we tag are aligned to at least this many bits.
// Currently the long pole in this tent is pollDesc at 280 bytes. Setting
// 9 here rounds those structs up to 512 bytes.
// gcBgMarkWorkerNode is also small, but we don't make many of those
// so it is ok to waste space on them.
internal static UntypedInt tagAlignBits => 9;

internal static UntypedInt tagAlign => /* 1 << tagAlignBits */ 512;

} // end runtime_package
