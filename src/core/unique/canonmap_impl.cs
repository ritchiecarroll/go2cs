// canonmap_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementation of canonmap.go's //go:linkname runtime primitive (`runtime_rand`, new at
// Go 1.26), which seeds each canonMap's hash. In Go, runtime.rand is the runtime's per-m ChaCha8
// generator, seeded from OS entropy. go2cs emits the linkname as a bodyless `partial`; without a body here
// the PartialStubGenerator fills it with a throwing stub and every unique.Make dies at its first map.
//
// It is not forwarded to the converted runtime.rand: that reads getg().m.chacha8, and the managed getg
// mints a fresh m per goroutine without mrandinit, so its ChaCha8 state is unseeded and every goroutine
// would draw the same seed. Random.Shared is OS-entropy seeded and thread-safe, as net, os, math/rand and
// math/rand/v2 already use for the same hook.

using System;
using System.Buffers.Binary;

// Hand-owned (no canonmap_impl.go exists, so a reconvert never regenerates it); marked for consistency
// with the other hand-owned companions.
[module: go.GoManualConversion]

namespace go;

partial class unique_package
{
    internal static partial uint64 runtime_rand()
    {
        Span<byte> bytes = stackalloc byte[8];
        Random.Shared.NextBytes(bytes);
        return BinaryPrimitives.ReadUInt64LittleEndian(bytes);
    }
}
