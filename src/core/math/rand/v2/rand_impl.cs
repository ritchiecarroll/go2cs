// rand_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementation of math/rand/v2's //go:linkname runtime primitive. In Go, runtime.rand
// is the runtime's per-goroutine ChaCha8 generator, cryptographically seeded at startup; go2cs emits
// the linkname as a bodyless `partial`, and without a body here the PartialStubGenerator would fill it
// with a throwing stub — so every top-level v2 convenience function (globalRand wraps runtimeSource)
// would crash at first use. Random.Shared matches the contract: OS-entropy seeded, thread-safe, fast,
// and NON-deterministic run to run — exactly like Go, where code needing a reproducible sequence must
// seed explicitly (rand.New(rand.NewPCG(seed1, seed2))), which routes through the converted generators
// and never reaches this hook.

using System;
using System.Buffers.Binary;

// Hand-owned (no rand_impl.go exists, so a reconvert never regenerates it); marked for consistency
// with the other hand-owned companions.
[module: go.GoManualConversion]

namespace go.math.rand;

partial class rand_package
{
    internal static partial uint64 runtime_rand()
    {
        Span<byte> bytes = stackalloc byte[8];
        Random.Shared.NextBytes(bytes);
        return BinaryPrimitives.ReadUInt64LittleEndian(bytes);
    }
}
