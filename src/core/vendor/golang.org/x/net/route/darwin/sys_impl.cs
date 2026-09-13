// sys_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The darwin run layer's increment 3b (2026-09-03): route.init's byte-order probe, hand-owned.
//
// Go's route.init (sys.go) probes the machine's byte order by reinterpreting the ADDRESS of a
// uint32 as a pointer to a four-byte array:
//
//     i := uint32(1)
//     b := (*[4]byte)(unsafe.Pointer(&i))
//     if b[0] == 1 { nativeEndian = littleEndian } else { nativeEndian = bigEndian }
//
// That is the raw-metal fork golib documents at array<T>.AliasPointer — an array<T> "can neither
// view native memory nor be fabricated from a scalar's bytes". The conversion spells the probe
// `(ж<array<byte>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(Ꮡi))` — it read `new @unsafe.Pointer(Ꮡi)`
// until the syscall-pin cut gave the mint a box-retaining door, which changes WHO KEEPS THE BOX ALIVE
// and nothing else here: the uintptr conversion pins the scalar's slot and registers it either way,
// the pointer conversion finds no ж<array<byte>> to alias and mints a
// NativeBox<array<byte>> over the four bytes, and reading its Value interprets a 16-byte
// REFERENCE-bearing struct (T[] m_array; int m_low; int m_length) out of a uint32 and whatever
// follows it in the box. The first full darwin behavioral census (run 33787891520) measured the
// result on every net importer — this init runs in the module initializer of a package net imports
// on BSDs only — and the full-stderr stage (run 33805906037, identical on osx-arm64 and osx-x64)
// placed it: `panic: runtime error: index out of range [0] with length 0` at array<byte>.get_Item
// <- route.init() <- .cctor, the length being whatever bytes happened to follow the slot. A nonzero
// read there dereferences a fabricated reference instead, which is why the fork is seamed rather
// than tolerated.
//
// The managed answer is the question the probe asks: BitConverter.IsLittleEndian. The other two
// statements of init are kept as Go wrote them — rtmVersion from syscall.RTM_VERSION, and the
// (kernelAlign, wireFormats) pair from the CONVERTED probeRoutingStack, which on darwin is a pure
// table (sys_darwin.go: seven wireFormat records and a map, no syscall behind it).
//
// Registered as `"vendor/golang.org/x/net/route": {"init": goosDarwin}` in manualTypeOperations.go
// — the lookup that reaches a GOROOT-vendored registration landed with aba54e39f2, and this is its
// first consumer after alias.AnyOverlap. A displaced init emits only the placeholder comment, so
// this file declares the [GoInit] module initializer the converted sys.cs used to carry. Scope:
// the darwin flavour alone; sys.go is BSD-only, so no other flavour ever held the converted init.
// The scalar-local form of the same idiom has three more members in Go 1.23.12 (the nameOff
// probes in reflect/type.go, runtime/type.go and internal/reflectlite/type.go), whose reach is
// unmeasured and not this file's concern.

using System;
using go;
using static go.vendor.golang.org.x.net.route_package;

[module: go.GoManualConversion]

// The two interface-implementation records the converted init used to mint at its `nativeEndian =
// littleEndian` / `bigEndian` assignments: a displaced body takes its conversion sites with it, so
// package_info.cs no longer carries them (measured by the two-seeded diff of this registration), and
// the go2cs-gen ImplementGenerator reads assembly attributes from every file of the project. This file
// performs those conversions, so it declares them — spelled as package_info.cs spells its own.
[assembly: GoImplement<binaryBigEndian, binaryByteOrder>]
[assembly: GoImplement<binaryLittleEndian, binaryByteOrder>]

namespace go.vendor.golang.org.x.net;

using syscall = syscall_package;

partial class route_package
{
    [GoInit] internal static void init()
    {
        if (BitConverter.IsLittleEndian)
        {
            nativeEndian = littleEndian;
        }
        else
        {
            nativeEndian = bigEndian;
        }

        // might get overridden in probeRoutingStack
        rtmVersion = syscall.RTM_VERSION;
        (kernelAlign, wireFormats) = probeRoutingStack();
    }
}
