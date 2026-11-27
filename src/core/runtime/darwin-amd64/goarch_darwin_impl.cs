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
// three class-(s) rows (gcimporter, debug/buildinfo, net/http/pprof). The converter displaces the const for
// darwin (manualConversionConsts) and this member replaces it.
//
// THE ONE VISIBLE ASYMMETRY, and it is deliberate. Only runtime.GOARCH moves. internal/goarch (GOARCH,
// IsAmd64, IsArm64, ArchFamily), the files Go selected for darwin/amd64, and every constant folded at
// conversion keep amd64, so they stay coherent with each other exactly as the osx-arm64 rows that bank today
// already run. The census of record (docs/phase4/DESIGN-comparator-arms.md §5, layer 2) read every runtime
// site that branches on GOARCH: each is inert at arm64, equivalent, or unreachable here (no Go heap, no Go
// stack, no schedinit). The AES hash is NOT moved by this member: alginit is unreachable (schedinit is its only
// caller), and its live twin, hash_impl.cs's initHashAlgorithm, already keys its arm64 branch on the process.
// internal/buildcfg's defaultGOARCH, Go's `const defaultGOARCH = runtime.GOARCH`, follows this member (its
// darwin zbootstrap hand-own), so go/build's default context agrees with it, and the managed heap dump's params
// record (managed_impl.cs) writes it, as Go's writes the binary's own architecture.
//
// linux and windows keep the converted const until a census per flavour (BOARD, 2026-11-09).
//
// SCOPED TO THE darwin/amd64 TARGET since darwin/arm64 joined the universe (docs/phase4/DESIGN-darwin-arm64-flavour.md
// section 4.2, ruling R3): the darwin/arm64 flavour is converted FOR arm64, so its converted constant already reads
// Go's "arm64" and keeps it. This flavour keeps the member because it still runs in arm64 processes: a pack built
// before D6, and a local build on Apple silicon with GoTargetArch empty. The file lives in darwin-amd64/ and its
// name has no principal (no Go file is goarch_darwin.go), so no target's emission can move it.

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
