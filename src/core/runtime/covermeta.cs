// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using rtcov = @internal.coverage.rtcov_package;
using @unsafe = unsafe_package;
using @internal.coverage;

partial class runtime_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeAddCovMetaˢ = "runtime.addCovMeta: coverage package map collision"u8;

// The compiler emits calls to runtime.addCovMeta
// but this code has moved to rtcov.AddMeta.
internal static uint32 addCovMeta(@unsafe.Pointer Δp, uint32 dlen, /*[16]*/ array<byte> hash, @string pkgpath, nint pkgid, uint8 cmode, uint8 cgran) {
    hash = hash.Clone();

    var id = rtcov.AddMeta(Δp, dlen, hash, pkgpath, pkgid, cmode, cgran);
    if (id == 0) {
        @throw(runtimeAddCovMetaˢ);
    }
    return id;
}

} // end runtime_package
