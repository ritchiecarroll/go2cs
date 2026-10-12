// anchor.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).
namespace go;

partial class anchor_package {

partial struct point {
    internal nint x, y;
}

internal static (nint n, error err) run(slice<nint> items, Action<nint> f) {
    nint n = default!;
    error err = default!;

    nint total = 0;
    foreach (var (_, it) in items) {
        var totalʗ1 = total;
        // a func literal reads total
        var totalʗ2 = total;
        total++;
        f(() => totalʗ1 + it);
    }

    foreach (var (i, _) in items) {
        foreach (var (j, _) in items) {
            if (j > i) {
                // skip to the next row
                goto continue_Outer;
            }
        }
continue_Outer:;
    }

    // open the item
    ref var errΔ1 = ref heap<error>(out var ᏑerrΔ1);
    (var v, errΔ1) = open(n);
    total += v;

    var p = new point(
        x: 1,
        y: 2
        // y is the last field
    );
    total += p.x;
    return (total, errΔ1);
}

internal static (nint, error) open(nint n) {
    return (n, default!);
}

} // end anchor_package
