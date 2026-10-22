// m2h.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).
namespace go;

using System.Runtime.CompilerServices;

partial class m2h_package {

//go:noinline
internal static partial (slice<byte>, slice<int16>) pair(nint n) {
    var aᴷ = new stackAppend<byte>(moves: true);
    var wᴷ = new stackAppend<int16>(moves: true);

    slice<byte> a = default!;
    slice<int16> w = default!;
    foreach (var i in range(n)) {
        a = append(aᴷ, a, (byte)i);
        w = append(wᴷ, w, (int16)i);
    }
    a = move2heap(aᴷ, a);
    w = move2heap(wᴷ, w);
    return (a, w);
}

//go:noinline
internal static partial nint sum(nint n) {
    var gᴷ = new stackAppend<byte>(moves: true);

    slice<byte> g = default!;
    foreach (var i in range(n)) {
        g = append(gᴷ, g, (byte)i);
    }
    // add each byte
    nint t = 0;
    g = move2heap(gᴷ, g);
    foreach (var (_, b) in g) {
        t += (nint)b;
    }
    return t;
}

} // end m2h_package
