// sys_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// routebsd.init's byte-order probe, hand-owned (2026-10-15, the Go 1.26 hop's H6). This is the successor of
// vendor/golang.org/x/net/route/darwin/sys_impl.cs, the darwin run layer's increment 3b (2026-09-03), which H5c
// deleted with its package when go1.26.9 removed the vendored x/net/route.
//
// go1.26.9 moved the routing-message reader net needs into internal/routebsd, "a very stripped down version of
// x/net/route", and its init (sys.go:21-32) keeps route's probe verbatim:
//
//     i := uint32(1)
//     b := (*[4]byte)(unsafe.Pointer(&i))
//     if b[0] == 1 { nativeEndian = littleEndian } else { nativeEndian = bigEndian }
//
// That is the raw-metal fork golib documents at array<T>.AliasPointer: an array<T> "can neither view native
// memory nor be fabricated from a scalar's bytes". The conversion spells the probe
// `(ж<array<byte>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(Ꮡi))`. The pointer conversion finds no ж<array<byte>>
// to alias, so it mints a NativeBox<array<byte>> over the four bytes, and reading its Value interprets a 16-byte
// REFERENCE-bearing struct (T[] m_array; int m_low; int m_length) out of a uint32 and whatever follows it. The
// first full darwin behavioral census measured route's copy of it on every net importer (run 33787891520, placed
// by run 33805906037 on osx-arm64 and osx-x64): `panic: runtime error: index out of range [0] with length 0` at
// array<byte>.get_Item <- route.init() <- .cctor. A nonzero read there dereferences a fabricated reference
// instead, which is why the fork is seamed rather than tolerated.
//
// The class did not go with the package. At 7f9cdd51b4 the emitted routebsd/darwin/sys.cs:18-21 carries the same
// expression, and run against golib it panics with the same text on every try (an offline probe; the darwin run
// that would show it inside net's module initializer waits for the corpus to compile at 1.26). net imports
// internal/routebsd on BSDs only (interface_bsd.go, interface_bsdvar.go, interface_darwin.go), so the panic would
// return in the module initializer of every darwin net importer.
//
// The managed answer is the question the probe asks: BitConverter.IsLittleEndian. The other two statements of
// init are kept as Go wrote them: rtmVersion from syscall.RTM_VERSION, and the (kernelAlign, wireFormats) pair from
// the CONVERTED probeRoutingStack, which on darwin is a pure table (sys_darwin.go: wireFormat records and a map,
// no syscall behind it).
//
// Registered as `"internal/routebsd": {"init": goosDarwin}` in manualTypeOperations.go. A displaced init emits
// only the placeholder comment, so this file declares the [GoInit] module initializer the converted sys.cs used
// to carry. Scope: the darwin flavour alone; sys.go is BSD-only, so no other flavour holds the converted init.
//
// route's second companion, syscall_impl.cs (route.sysctl, a //go:linkname pull of syscall.sysctl), has no
// successor: routebsd's fetchRIB calls syscall.RouteRIB, an exported converted function, and pulls nothing.

using System;
using go;
using static go.@internal.routebsd_package;

[module: go.GoManualConversion]

// The two interface-implementation records the converted init used to mint at its `nativeEndian = littleEndian`
// / `bigEndian` assignments. A displaced body takes its conversion sites with it, so package_info.cs no longer
// carries them, and the go2cs-gen ImplementGenerator reads assembly attributes from every file of the project.
// This file performs those conversions, so it declares them, spelled as package_info.cs spells its own.
[assembly: GoImplement<binaryBigEndian, binaryByteOrder>]
[assembly: GoImplement<binaryLittleEndian, binaryByteOrder>]

namespace go.@internal;

using syscall = syscall_package;

partial class routebsd_package
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
