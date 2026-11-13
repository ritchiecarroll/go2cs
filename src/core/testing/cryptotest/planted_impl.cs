// planted_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// OQ1's planted-violation gate (docs/phase4/DESIGN-internal-package-marking.md): a hand-written file of testing/cryptotest,
// which is outside the root "crypto", names crypto/internal/rand's package class. Go refuses that import, so the
// standard-library build reads exactly one GO2CS0014, here. This file lives only on the side ref claude/c2-oq1-planted
// and never seats.
namespace go.testing;

partial class cryptotest_package
{
    internal static System.Type PlantedInternalPackageUse() => typeof(go.crypto.@internal.rand_package);
}
