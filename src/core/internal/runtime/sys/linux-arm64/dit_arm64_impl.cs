// dit_arm64_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// internal/runtime/sys's three arm64 DIT primitives (dit_arm64.go declares them, dit_arm64.s sets and reads
// PSTATE.DIT), bodied as Go's own no_dit.go bodies them for every arch without DIT: EnableDIT and DITEnabled
// answer false, and DisableDIT does nothing.
//
// WHY THOSE BODIES (S3 site 9, the arm64-only stubbed census). The CLR exposes no control over PSTATE.DIT, so
// internal/cpu holds ARM64.HasDIT false at linux/arm64 (S3 site 0, under COORD's darwin ruling of 2026-11-06), and
// DITSupported, which dit_arm64.go initializes from it, is false. Go's callers then never reach these:
// crypto/subtle.WithDataIndependentTiming and runtime's cgocallbackg check DITSupported first. The only unguarded
// paths are GODEBUG=dataindependenttiming=1 in mstart, which the managed host never runs, and cgocallbackg on a
// C-created thread, where there is no cgo. no_dit.go's bodies are exactly what Go runs where the feature is absent,
// and absent is what DITSupported reports here. They replace a throw that nothing should reach with the answer
// "DIT is off", which is true of every thread the CLR starts.
//
// This file's principal is dit_arm64.cs, which only linux/arm64 emits in this universe, so the hand-own router keeps
// it in linux-arm64/.
//
// Hand-owned (no dit_arm64_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]

namespace go.@internal.runtime;

partial class sys_package
{
    public static partial bool EnableDIT() => false;

    public static partial bool DITEnabled() => false;

    public static partial void DisableDIT() { }
}
