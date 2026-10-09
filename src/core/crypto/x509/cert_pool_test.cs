// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using testing = testing_package;
using static go.crypto.x509_package;

partial class x509_internal_test_package {

internal partial struct TestCertPoolEqual_tests /*dyn*/ {
    internal @string name;
    internal ж<global::go.crypto.x509_package.CertPool> a;
    internal ж<global::go.crypto.x509_package.CertPool> b;
    internal bool equal;
}

public static void TestCertPoolEqual(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tc = Ꮡ(new Certificate(Raw: new byte[]{1, 2, 3}.slice(), RawSubject: new byte[]{2}.slice()));
    var otherTC = Ꮡ(new Certificate(Raw: new byte[]{9, 8, 7}.slice(), RawSubject: new byte[]{8}.slice()));
    var emptyPool = NewCertPool();
    var nonSystemPopulated = NewCertPool();
    nonSystemPopulated.AddCert(tc);
    var nonSystemPopulatedAlt = NewCertPool();
    nonSystemPopulatedAlt.AddCert(otherTC);
    var (emptySystem, err) = SystemCertPool();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var populatedSystem, err) = SystemCertPool();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    populatedSystem.AddCert(tc);
    (var populatedSystemAlt, err) = SystemCertPool();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    populatedSystemAlt.AddCert(otherTC);
    var tests = new TestCertPoolEqual_tests[]{
        new(
            name: "two empty pools"u8,
            a: emptyPool,
            b: emptyPool,
            equal: true
        ),
        new(
            name: "one empty pool, one populated pool"u8,
            a: emptyPool,
            b: nonSystemPopulated,
            equal: false
        ),
        new(
            name: "two populated pools"u8,
            a: nonSystemPopulated,
            b: nonSystemPopulated,
            equal: true
        ),
        new(
            name: "two populated pools, different content"u8,
            a: nonSystemPopulated,
            b: nonSystemPopulatedAlt,
            equal: false
        ),
        new(
            name: "two empty system pools"u8,
            a: emptySystem,
            b: emptySystem,
            equal: true
        ),
        new(
            name: "one empty system pool, one populated system pool"u8,
            a: emptySystem,
            b: populatedSystem,
            equal: false
        ),
        new(
            name: "two populated system pools"u8,
            a: populatedSystem,
            b: populatedSystem,
            equal: true
        ),
        new(
            name: "two populated pools, different content"u8,
            a: populatedSystem,
            b: populatedSystemAlt,
            equal: false
        ),
        new(
            name: "two nil pools"u8,
            a: nil,
            b: nil,
            equal: true
        ),
        new(
            name: "one nil pool, one empty pool"u8,
            a: nil,
            b: emptyPool,
            equal: false
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tcΔ1 = ref heap(new TestCertPoolEqual_tests(), out var ᏑtcΔ1);
        tcΔ1 = vᴛ1;

        var tcʗ1 = tcΔ1;
        Ꮡt.Run(tcΔ1.name, (ж<testing.T> tΔ1) => {
            var equal = tcʗ1.a.Equal(tcʗ1.b);
            if (equal != tcʗ1.equal) {
                tΔ1.Errorf("Unexpected Equal result: got %t, want %t"u8, equal, tcʗ1.equal);
            }
        });
    }
}

} // end x509_internal_test_package
