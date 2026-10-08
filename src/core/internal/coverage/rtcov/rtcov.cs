// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using @unsafe = unsafe_package;

partial class rtcov_package {

// This package contains types whose structure is shared between
// the runtime package and the "runtime/coverage" implementation.

// CovMetaBlob is a container for holding the meta-data symbol (an
// RODATA variable) for an instrumented Go package. Here "p" points to
// the symbol itself, "len" is the length of the sym in bytes, and
// "hash" is an md5sum for the sym computed by the compiler. When
// the init function for a coverage-instrumented package executes, it
// will make a call into the runtime which will create a covMetaBlob
// object for the package and chain it onto a global list.
partial struct CovMetaBlob {
    public ж<byte> P;
    public uint32 Len;
    public array<byte> Hash = new(16);
    public @string PkgPath;
    public nint PkgID;
    public uint8 CounterMode; // coverage.CounterMode
    public uint8 CounterGranularity; // coverage.CounterGranularity
}

// CovCounterBlob is a container for encapsulating a counter section
// (BSS variable) for an instrumented Go module. Here "counters"
// points to the counter payload and "len" is the number of uint32
// entries in the section.
partial struct CovCounterBlob {
    public ж<uint32> Counters;
    public uint64 Len;
}

// Meta is the top-level container for bits of state related to
// code coverage meta-data in the runtime.

partial struct Metaᴛ1 /*dyn*/ {
    // List contains the list of currently registered meta-data
    // blobs for the running program.
    public slice<CovMetaBlob> List;
    // PkgMap records mappings from hard-coded package IDs to
    // slots in the List above.
    public map<nint, nint> PkgMap;
    // Set to true if we discover a package mapping glitch.
    internal bool hardCodedListNeedsUpdating;
}
public static Metaᴛ1 Meta;

// AddMeta is invoked during package "init" functions by the
// compiler when compiling for coverage instrumentation; here 'p' is a
// meta-data blob of length 'dlen' for the package in question, 'hash'
// is a compiler-computed md5.sum for the blob, 'pkpath' is the
// package path, 'pkid' is the hard-coded ID that the compiler is
// using for the package (or -1 if the compiler doesn't think a
// hard-coded ID is needed), and 'cmode'/'cgran' are the coverage
// counter mode and granularity requested by the user. Return value is
// the ID for the package for use by the package code itself,
// or 0 for impossible errors.
public static uint32 AddMeta(@unsafe.Pointer p, uint32 dlen, /*[16]*/ array<byte> hash, @string pkgpath, nint pkgid, uint8 cmode, uint8 cgran) {
    hash = hash.Clone();

    nint slot = len(Meta.List);
    Meta.List = append(Meta.List, new CovMetaBlob(
        P: (ж<byte>)(uintptr)(p),
        Len: dlen,
        Hash: hash.Clone(),
        PkgPath: pkgpath,
        PkgID: pkgid,
        CounterMode: cmode,
        CounterGranularity: cgran
    ));
    if (pkgid != -1) {
        if (Meta.PkgMap == default!) {
            Meta.PkgMap = new map<nint, nint>();
        }
        {
            var (_, ok) = Meta.PkgMap[pkgid, ꟷ]; if (ok) {
                return 0;
            }
        }
        // Record the real slot (position on meta-list) for this
        // package; we'll use the map to fix things up later on.
        Meta.PkgMap[pkgid] = slot;
    }
    // ID zero is reserved as invalid.
    return (uint32)(slot + 1);
}

} // end rtcov_package
