// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build 386 || amd64
namespace go.@internal;

using cpu = go.@internal.cpu_package;
using testing = testing_package;
using go.@internal;
using static go.@internal.cpu_internal_test_package;

partial class cpu_test_package {

// Tests fetching data cache sizes. This test only checks that DataCacheSizes
// won't explode. Otherwise it's just informational, and dumps the current
// data cache sizes.
public static void TestDataCacheSizes(ж<testing.T> Ꮡt) {
    // N.B. Don't try to check these values because we don't know what
    // kind of environment we're running in. We don't want this test to
    // fail on some random x86 chip that happens to not support the right
    // CPUID bits for some reason.
    var caches = cpu.DataCacheSizes();
    foreach (var (i, size) in caches) {
        Ꮡt.Logf("L%d: %d"u8, i + 1, size);
    }
}

} // end cpu_test_package
