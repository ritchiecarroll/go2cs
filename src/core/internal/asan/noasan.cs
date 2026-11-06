// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !asan
namespace go.@internal;

using @unsafe = unsafe_package;

partial class asan_package {

public const bool Enabled = false;

public static void Read(@unsafe.Pointer addr, uintptr len) { }

public static void Write(@unsafe.Pointer addr, uintptr len) { }

} // end asan_package
