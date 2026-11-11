// test_amd64_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// runtime.testSPWrite's body at amd64, the only arch where Go writes it in assembly.
//
// testSPWrite exists only for runtime's own TestSPWrite (#62326). On amd64 it is an assembly frame (test_amd64.s)
// with a 16 KB locals area that writes SP, so Go's stack-growth prologue has to be traced back from a function that
// clobbers SP; test_amd64.go declares it, and the converter emits that declaration as the bodyless partial in
// test_amd64.cs. The managed host has no Go stack growth and no such prologue, and Go's own body for every other
// architecture (test_stubs.go, `//go:build !amd64`) is exactly this empty one. The test then only calls it on a
// fresh goroutine and returns. Go's portable body, not a stand-in. Left as the PartialStubGenerator stub, it threw
// on that goroutine and ended the runtime row's test host (COORD ruling 2026-09-22).
//
// WHY HERE AND NOT FLAT (S3 site 9, M2's wall). The body sat in the flat stubs_impl.cs, which every build compiles.
// At linux/arm64 the converted test_stubs.cs (linux-arm64/) carries Go's own empty body, so the flat one was a
// second definition: CS0111, which stopped runtime, and with it every project above it, at that flavour. This
// file's principal is test_amd64.cs, which only the amd64 targets emit, so the router keeps it in amd64/.
// GoTestSPWrite, GolibTests' handle on it, stays flat in stubs_impl.cs: testSPWrite has exactly one body in
// every build.
//
// Hand-owned (no test_amd64_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    internal static partial void testSPWrite() { }
}
