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

public static void TestCertPoolEqual(ж<testing.T> t) {
    var tc = Ꮡ(new Certificate(Raw: slice<byte>([1, 2, 3]), RawSubject: slice<byte>([2])));
    var otherTC = Ꮡ(new Certificate(Raw: slice<byte>([9, 8, 7]), RawSubject: slice<byte>([8])));

    var emptyPool = NewCertPool();
    var nonSystemPopulated = NewCertPool();
    nonSystemPopulated.AddCert(tc);
    var nonSystemPopulatedAlt = NewCertPool();
    nonSystemPopulatedAlt.AddCert(otherTC);
    var (emptySystem, err) = SystemCertPool();
    if (err != nil) {
        t.Fatal(err);
    }
    (var populatedSystem, err) = SystemCertPool();
    if (err != nil) {
        t.Fatal(err);
    }
    populatedSystem.AddCert(tc);
    (var populatedSystemAlt, err) = SystemCertPool();
    if (err != nil) {
        t.Fatal(err);
    }
    populatedSystemAlt.AddCert(otherTC);
    var tests = slice<TestCertPoolEqual_tests>([
        new(
            name: "two empty pools",
            a: emptyPool,
            b: emptyPool,
            equal: true
        ),
        new(
            name: "one empty pool, one populated pool",
            a: emptyPool,
            b: nonSystemPopulated,
            equal: false
        ),
        new(
            name: "two populated pools",
            a: nonSystemPopulated,
            b: nonSystemPopulated,
            equal: true
        ),
        new(
            name: "two populated pools, different content",
            a: nonSystemPopulated,
            b: nonSystemPopulatedAlt,
            equal: false
        ),
        new(
            name: "two empty system pools",
            a: emptySystem,
            b: emptySystem,
            equal: true
        ),
        new(
            name: "one empty system pool, one populated system pool",
            a: emptySystem,
            b: populatedSystem,
            equal: false
        ),
        new(
            name: "two populated system pools",
            a: populatedSystem,
            b: populatedSystem,
            equal: true
        ),
        new(
            name: "two populated pools, different content",
            a: populatedSystem,
            b: populatedSystemAlt,
            equal: false
        ),
        new(
            name: "two nil pools",
            a: nil,
            b: nil,
            equal: true
        ),
        new(
            name: "one nil pool, one empty pool",
            a: nil,
            b: emptyPool,
            equal: false
        )
    ]);

    foreach (var (_, vᴛ1) in tests) {
        ref var tcΔ1 = ref heap(new TestCertPoolEqual_tests(), out var ᏑtcΔ1);
        tcΔ1 = vᴛ1;

        var tcʗ1 = tcΔ1;
        t.Run(tcΔ1.name, (ж<testing.T> tΔ1) => {
            var equal = tcʗ1.a.Equal(tcʗ1.b);
            if (equal != tcʗ1.equal) {
                tΔ1.Errorf("Unexpected Equal result: got %t, want %t", equal, tcʗ1.equal);
            }
        });
    }
}

} // end x509_internal_test_package
