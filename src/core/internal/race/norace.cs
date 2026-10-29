// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !race
namespace go.@internal;

using abi = go.@internal.abi_package;
using @unsafe = unsafe_package;
using go.@internal;

partial class race_package {

public const bool Enabled = false;

public static void Acquire(@unsafe.Pointer addr) {
}

public static void Release(@unsafe.Pointer addr) {
}

public static void ReleaseMerge(@unsafe.Pointer addr) {
}

public static void Disable() {
}

public static void Enable() {
}

public static void Read(@unsafe.Pointer addr) {
}

public static void ReadPC(@unsafe.Pointer addr, uintptr callerpc, uintptr pc) {
}

public static void ReadObjectPC(ж<abi.Type> t, @unsafe.Pointer addr, uintptr callerpc, uintptr pc) {
}

public static void Write(@unsafe.Pointer addr) {
}

public static void WritePC(@unsafe.Pointer addr, uintptr callerpc, uintptr pc) {
}

public static void WriteObjectPC(ж<abi.Type> t, @unsafe.Pointer addr, uintptr callerpc, uintptr pc) {
}

public static void ReadRange(@unsafe.Pointer addr, nint len) {
}

public static void WriteRange(@unsafe.Pointer addr, nint len) {
}

public static nint Errors() {
    return 0;
}

} // end race_package
