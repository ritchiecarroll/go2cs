// goarch_darwin_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// runtime.GOARCH for the darwin flavour: the PROCESS architecture, read at run time.
//
// Go fixes GOARCH per build (extern.go: `const GOARCH string = goarch.GOARCH`). The darwin flavour of the
// corpus is converted ONCE, for darwin/amd64, and runs under both mac RIDs, so the converted constant read
// "amd64" on Apple silicon, where Go's own binary reads "arm64": a user-visible divergence, and the root of
// three class-(s) rows (gcimporter, debug/buildinfo, net/http/pprof) and three runtime verdicts (the AES
// hash). The converter displaces the const for darwin (manualConversionConsts) and this member replaces it.
//
// THE ONE VISIBLE ASYMMETRY, and it is deliberate. Only runtime.GOARCH moves. internal/goarch (GOARCH,
// IsAmd64, IsArm64, ArchFamily), the files Go selected for darwin/amd64, and every constant folded at
// conversion keep amd64, so they stay coherent with each other exactly as the osx-arm64 rows that bank today
// already run. The census of record (docs/phase4/DESIGN-comparator-arms.md §5, layer 2) read every runtime
// site that branches on GOARCH: each is inert at arm64, improving (alginit's arm64 AES hash, with cpu.ARM64
// filled by internal/cpu's cpu_arm64_impl.cs), equivalent, or unreachable here (no Go heap, no Go stack, no
// schedinit). internal/buildcfg's defaultGOARCH, Go's `const defaultGOARCH = runtime.GOARCH`, follows this
// member (its darwin zbootstrap hand-own), so go/build's default context agrees with it.
//
// linux and windows keep the converted const until a census per flavour (BOARD, 2026-11-09).

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
    // Any architecture but Arm64 keeps the converted target's "amd64": darwin ships x64 and arm64 alone.
    internal static readonly @string Value = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "arm64" : "amd64";
}

} // end runtime_package
