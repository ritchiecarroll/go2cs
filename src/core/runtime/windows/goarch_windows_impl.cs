// goarch_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// runtime.GOARCH for the windows flavour: the PROCESS architecture, read at run time.
//
// Go fixes GOARCH per build (extern.go: `const GOARCH string = goarch.GOARCH`). The windows flavour of the
// corpus is converted ONCE, for windows/amd64, and the same managed program runs as a native arm64 .NET process
// on Windows on Arm, where Go's own binary reads "arm64". The converter displaces the const for windows
// (manualConversionConsts, as for darwin; runtime/darwin/goarch_darwin_impl.cs) and this member replaces it.
//
// THE ONE VISIBLE ASYMMETRY, and it is deliberate. Only runtime.GOARCH moves. internal/goarch (GOARCH,
// IsAmd64, IsArm64, ArchFamily), the files Go selected for windows/amd64, and every constant folded at
// conversion keep amd64, so they stay coherent with each other. The census of record for this flavour
// (docs/phase4/CENSUS-goarch-linux-windows.md) read every runtime site that branches on GOARCH: each is inert at
// arm64, equivalent, or unreachable here, including windows' own signal_windows.cs and syscall_windows.cs sites
// (the vectored-exception and callback paths this host does not arm). internal/buildcfg's defaultGOARCH follows
// this member (its windows zbootstrap hand-own), so go/build's default context agrees with it.
//
// No package ships for windows/arm64 today (the RIDs are win-x64, linux-x64, osx-x64 and osx-arm64); on win-x64
// the value is "amd64", as before.

[module: go.GoManualConversion]

namespace go;

using System.Runtime.InteropServices;

partial class runtime_package {

// GOARCH is the running program's architecture target:
// one of 386, amd64, arm, s390x, and so on.
public static ref readonly @string GOARCH => ref GoProcessArch.Value;

// The value is computed once, on first read, in its own type: a static initializer of runtime_package
// itself could run after another initializer in the class that reads GOARCH.
private static class GoProcessArch {
    // Any architecture but Arm64 keeps the converted target's "amd64": the flavour is amd64's.
    internal static readonly @string Value = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "arm64" : "amd64";
}

} // end runtime_package
