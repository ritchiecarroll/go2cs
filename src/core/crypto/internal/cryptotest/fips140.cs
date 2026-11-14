// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using Δfips140 = go.crypto.@internal.fips140_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using go.crypto.@internal;

partial class cryptotest_package {

public static void MustSupportFIPS140(ж<testing.T> t) {
    t.Helper();
    var err = Δfips140.Supported(); if (err != nil) {
        t.Skipf("test requires FIPS 140 mode: %v", err);
    }
}

// MustMinimumFIPS140ModuleVersion skips the test if compiled against a lower
// minor version of the FIPS 140-3 module than min (such as "v1.26.0").
public static void MustMinimumFIPS140ModuleVersion(testing.TB tb, @string min) {
    tb.Helper();
    if (Δfips140.Version() == "latest"u8) {
        return;
    }
    if (parseFIPS140MinorVersion(tb, Δfips140.Version()) < parseFIPS140MinorVersion(tb, min)) {
        tb.Skipf("test requires FIPS 140-3 module %s or later", min);
    }
}

internal static nint parseFIPS140MinorVersion(testing.TB tb, @string version) {
    tb.Helper();
    var (v, ok) = strings.CutPrefix(version, "v1.");
    if (!ok) {
        tb.Fatalf("unexpected FIPS 140 version format: %q", version);
    }
    (v, _, ok) = strings.Cut(v, ".");
    if (!ok) {
        tb.Fatalf("unexpected FIPS 140 version format: %q", version);
    }
    var (i, err) = strconv.Atoi(v);
    if (err != nil) {
        tb.Fatalf("unexpected FIPS 140 version format %q: %v", version, err);
    }
    return i;
}

} // end cryptotest_package
