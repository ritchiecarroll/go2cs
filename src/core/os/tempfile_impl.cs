// tempfile_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementation of tempfile.go's //go:linkname-into-runtime hook (`runtime_rand`,
// provided by runtime.rand). go2cs emits it as a bodyless `partial` method, and without a body here
// the PartialStubGenerator fills it with a throwing stub — so EVERY os.CreateTemp and os.MkdirTemp
// call died with "runtime_rand: external (assembly or cgo) function is not implemented" before it
// could pick a name. That took out far more than temp files: it is what made io's OffsetWriter
// tests infrastructure-error, and what stopped path/filepath's symlink tests from even reaching
// testenv's own decision to skip them.
//
// What Go asks of runtime.rand HERE is stated by tempfile.go itself: "We generate random temporary
// file names so that there's a good chance the file doesn't exist yet - keeps the number of tries
// in TempFile to a minimum." Only two properties matter, and neither is statistical quality:
//
//   1. It must differ ACROSS PROCESSES. Two programs creating temp files in the same directory must
//      not walk the same name sequence, or one of them spends its 10,000 tries colliding — and a
//      name another process can predict is a name it can squat.
//   2. It must be safe to call from any goroutine — CreateTemp has no lock, and Go's per-M source
//      needs none.
//
// Random.Shared answers both: the CLR seeds it from the OS at first use (so it is per-process and
// unpredictable) and documents every member as thread-safe. NextBytes fills the full 64 bits rather
// than NextInt64's 63, so the value is a faithful uint64 even though nextRandom only keeps the low
// 32. This is NOT a cryptographic source and does not need to be — Go's runtime.rand is a chacha8
// PRNG whose output here is truncated to 32 bits and then rendered as a decimal filename; security
// against a local attacker comes from O_EXCL/Mkdir failing, not from the name.

using System;

// Hand-owned (no tempfile_impl.go exists, so a reconvert never regenerates it); marked for
// consistency with the other hand-owned companions.
[module: go.GoManualConversion]

namespace go;

partial class os_package
{
    internal static partial uint64 runtime_rand()
    {
        Span<byte> bits = stackalloc byte[8];
        Random.Shared.NextBytes(bits);
        return BitConverter.ToUInt64(bits);
    }
}
