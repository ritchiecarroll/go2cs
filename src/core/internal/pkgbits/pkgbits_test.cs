// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using pkgbits = global::go.@internal.pkgbits_package;
using strings = strings_package;
using testing = testing_package;
using global::go.@internal;
using io = io_package;

partial class pkgbits_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string packageIdˢ = "package_id"u8;

public static void TestRoundTrip(ж<testing.T> Ꮡt) {
    foreach (var (_, version) in new pkgbitsꓸVersion[]{
        pkgbits.V0,
        pkgbits.V1,
        pkgbits.V2
    }.slice()) {
        ref var pw = ref heap<pkgbits.PkgEncoder>(out var Ꮡpw);
        pw = pkgbits.NewPkgEncoder(version, -1);
        ref var w = ref heap<pkgbits.Encoder>(out var Ꮡw);
        w = Ꮡpw.NewEncoder(pkgbits.RelocMeta, pkgbits.SyncPublic);
        Ꮡw.Flush();
        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        _ = Ꮡpw.DumpTo(new strings_BuilderжWriter(Ꮡb));
        @string input = b.String();
        ref var pr = ref heap<pkgbits.PkgDecoder>(out var Ꮡpr);
        pr = pkgbits.NewPkgDecoder(packageIdˢ, input);
        var r = Ꮡpr.NewDecoder(pkgbits.RelocMeta, pkgbits.PublicRootIdx, pkgbits.SyncPublic);
        if (r.Version() != w.Version()) {
            Ꮡt.Errorf("Expected reader version %q to be the writer version %q"u8, r.Version(), w.Version());
        }
    }
}

// Type checker to enforce that know V* have the constant values they must have.
internal static array<bool> _ᴛ1ʗ = new bool[]{}.array();

internal static array<bool> _ᴛ2ʗ = new bool[]{}.array(1);

internal partial struct TestVersions_vfpair /*dyn*/ {
    internal pkgbitsꓸVersion v;
    internal pkgbits.Field f;
}

public static void TestVersions(ж<testing.T> Ꮡt) {
    // has field tests
    foreach (var (_, c) in new TestVersions_vfpair[]{
        new(pkgbits.V1, pkgbits.Flags),
        new(pkgbits.V2, pkgbits.Flags),
        new(pkgbits.V0, pkgbits.HasInit),
        new(pkgbits.V1, pkgbits.HasInit),
        new(pkgbits.V0, pkgbits.DerivedFuncInstance),
        new(pkgbits.V1, pkgbits.DerivedFuncInstance),
        new(pkgbits.V0, pkgbits.DerivedInfoNeeded),
        new(pkgbits.V1, pkgbits.DerivedInfoNeeded),
        new(pkgbits.V2, pkgbits.AliasTypeParamNames)
    }.slice()) {
        if (!c.v.Has(c.f)) {
            Ꮡt.Errorf("Expected version %v to have field %v"u8, c.v, c.f);
        }
    }
    // does not have field tests
    foreach (var (_, c) in new TestVersions_vfpair[]{
        new(pkgbits.V0, pkgbits.Flags),
        new(pkgbits.V2, pkgbits.HasInit),
        new(pkgbits.V2, pkgbits.DerivedFuncInstance),
        new(pkgbits.V2, pkgbits.DerivedInfoNeeded),
        new(pkgbits.V0, pkgbits.AliasTypeParamNames),
        new(pkgbits.V1, pkgbits.AliasTypeParamNames)
    }.slice()) {
        if (c.v.Has(c.f)) {
            Ꮡt.Errorf("Expected version %v to not have field %v"u8, c.v, c.f);
        }
    }
}

} // end pkgbits_test_package
