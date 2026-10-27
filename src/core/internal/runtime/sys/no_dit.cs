// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !arm64
namespace go.@internal.runtime;

partial class sys_package {

public static readonly ж<bool> ᏑDITSupported = new StaticBox<bool>(false);
public static ref bool DITSupported => ref ((StaticBox<bool>)ᏑDITSupported).ValueSlot;

public static bool EnableDIT() {
    return false;
}

public static bool DITEnabled() {
    return false;
}

public static void DisableDIT() {
}

} // end sys_package
