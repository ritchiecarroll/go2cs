// plant.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).
namespace go;

partial class plant_package {

// Numbers.
public static UntypedInt A => 1; // first
public static UntypedInt B => 2; // second

// Doc for C.
public static UntypedInt C => 3;

// F does things.
public static nint F(nint x) {
    var y = x + 1;

    // second part
    var z = y * 2;
    if (z > 3) { // big
        return z;
    }
    var t = new nint[]{
        1, // one
        2, // two
    }.slice();
    return y + len(t);
}

// G is next.
public static void G() {
}

// H is last.
public static void H() {
}

} // end plant_package
