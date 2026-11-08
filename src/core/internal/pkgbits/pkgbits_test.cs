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
private static ref readonly @string packageIdˢ => ref packageIdˢᵇ;
private static readonly @string packageIdˢᵇ = "package_id"u8;

public static void TestRoundTrip(ж<testing.T> t) {
    foreach (var (_, version) in range(slice<pkgbitsꓸVersion>([
        pkgbits.V0,
        pkgbits.V1,
        pkgbits.V2
    ]))) {
        ref var pw = ref heap<pkgbits.PkgEncoder>(pkgbits.NewPkgEncoder(version, -1), out var Ꮡpw);
        var w = Ꮡpw.NewEncoder(pkgbits.SectionMeta, pkgbits.SyncPublic);
        w.Flush();

        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        _ = Ꮡpw.DumpTo(new strings_BuilderжWriter(Ꮡb));
        @string input = b.String();

        ref var pr = ref heap<pkgbits.PkgDecoder>(pkgbits.NewPkgDecoder(packageIdˢ, input), out var Ꮡpr);
        var r = Ꮡpr.NewDecoder(pkgbits.SectionMeta, pkgbits.PublicRootIdx, pkgbits.SyncPublic);

        if (r.Version() != w.Version()) {
            t.Errorf("Expected reader version %d to be the writer version %d", r.Version(), w.Version());
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

public static void TestVersions(ж<testing.T> t) {
    // has field tests
    foreach (var (_, c) in range(slice<TestVersions_vfpair>([
        new(pkgbits.V1, pkgbits.Flags),
        new(pkgbits.V2, pkgbits.Flags),
        new(pkgbits.V0, pkgbits.HasInit),
        new(pkgbits.V1, pkgbits.HasInit),
        new(pkgbits.V0, pkgbits.DerivedFuncInstance),
        new(pkgbits.V1, pkgbits.DerivedFuncInstance),
        new(pkgbits.V0, pkgbits.DerivedInfoNeeded),
        new(pkgbits.V1, pkgbits.DerivedInfoNeeded),
        new(pkgbits.V2, pkgbits.AliasTypeParamNames)
    ]))) {
        if (!c.v.Has(c.f)) {
            t.Errorf("Expected version %v to have field %v", c.v, c.f);
        }
    }

    // does not have field tests
    foreach (var (_, c) in range(slice<TestVersions_vfpair>([
        new(pkgbits.V0, pkgbits.Flags),
        new(pkgbits.V2, pkgbits.HasInit),
        new(pkgbits.V2, pkgbits.DerivedFuncInstance),
        new(pkgbits.V2, pkgbits.DerivedInfoNeeded),
        new(pkgbits.V0, pkgbits.AliasTypeParamNames),
        new(pkgbits.V1, pkgbits.AliasTypeParamNames)
    ]))) {
        if (c.v.Has(c.f)) {
            t.Errorf("Expected version %v to not have field %v", c.v, c.f);
        }
    }
}

} // end pkgbits_test_package
