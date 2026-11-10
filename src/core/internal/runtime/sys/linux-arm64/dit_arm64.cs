// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build arm64
namespace go.@internal.runtime;

using cpu = go.@internal.cpu_package;
using go.@internal;

partial class sys_package {

public static readonly ж<bool> ᏑDITSupported = new StaticBox<bool>(cpu.ARM64.HasDIT);
public static ref bool DITSupported => ref ((StaticBox<bool>)ᏑDITSupported).ValueSlot;

public static partial bool EnableDIT();
public static partial bool DITEnabled();
public static partial void DisableDIT();

} // end sys_package
