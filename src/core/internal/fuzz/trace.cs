// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !libfuzzer
namespace go.@internal;

/* blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) */ // for go:linkname

partial class fuzz_package {

//go:linkname libfuzzerTraceCmp1 runtime.libfuzzerTraceCmp1
//go:linkname libfuzzerTraceCmp2 runtime.libfuzzerTraceCmp2
//go:linkname libfuzzerTraceCmp4 runtime.libfuzzerTraceCmp4
//go:linkname libfuzzerTraceCmp8 runtime.libfuzzerTraceCmp8
//go:linkname libfuzzerTraceConstCmp1 runtime.libfuzzerTraceConstCmp1
//go:linkname libfuzzerTraceConstCmp2 runtime.libfuzzerTraceConstCmp2
//go:linkname libfuzzerTraceConstCmp4 runtime.libfuzzerTraceConstCmp4
//go:linkname libfuzzerTraceConstCmp8 runtime.libfuzzerTraceConstCmp8
//go:linkname libfuzzerHookStrCmp runtime.libfuzzerHookStrCmp
//go:linkname libfuzzerHookEqualFold runtime.libfuzzerHookEqualFold
internal static void libfuzzerTraceCmp1(uint8 arg0, uint8 arg1, nuint fakePC) {
}

internal static void libfuzzerTraceCmp2(uint16 arg0, uint16 arg1, nuint fakePC) {
}

internal static void libfuzzerTraceCmp4(uint32 arg0, uint32 arg1, nuint fakePC) {
}

internal static void libfuzzerTraceCmp8(uint64 arg0, uint64 arg1, nuint fakePC) {
}

internal static void libfuzzerTraceConstCmp1(uint8 arg0, uint8 arg1, nuint fakePC) {
}

internal static void libfuzzerTraceConstCmp2(uint16 arg0, uint16 arg1, nuint fakePC) {
}

internal static void libfuzzerTraceConstCmp4(uint32 arg0, uint32 arg1, nuint fakePC) {
}

internal static void libfuzzerTraceConstCmp8(uint64 arg0, uint64 arg1, nuint fakePC) {
}

internal static void libfuzzerHookStrCmp(@string arg0, @string arg1, nuint fakePC) {
}

internal static void libfuzzerHookEqualFold(@string arg0, @string arg1, nuint fakePC) {
}

} // end fuzz_package
