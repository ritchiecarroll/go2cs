// prelude.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).
namespace go;

using ꓸꓸꓸnint = Span<nint>;

partial class prelude_package {

internal static nint /*n*/ one(params ꓸꓸꓸnint xsʗp) {
    nint n = default!;
    var xs = xsʗp.sslice();

    n = len(xs);
    return n;
}

internal static nint /*n*/ sum(/*[4]*/ array<nint> a, params ꓸꓸꓸnint xsʗp) {
    nint n = default!;
    var xs = xsʗp.sslice();
    a = a.Clone();

    // first body comment
    foreach (var (_, x) in xs) {
        n += x;
    }
    return n + a[0];
}

internal static (ж<nint> p, nint n) esc(/*[2]*/ array<nint> a, nint xʗp) {
    ж<nint> p = default!;
    nint n = default!;
    a = a.Clone();
    ref var x = ref heap(xʗp, out var Ꮡx);

    n = a[1];
    p = Ꮡx;
    return (p, n);
}

internal static nint /*n*/ first(/*[2]*/ array<nint> a) {
    nint n = default!;
    a = a.Clone();

    nint z = default!;

    n = a[0] + z;
    return n;
}

internal static nint /*n*/ grow(params ꓸꓸꓸnint xsʗp) {
    nint n = default!;
    var xs = xsʗp.sslice();

    var buf = new slice<nint>(len(xs));
    n = copy(buf, xs);
    return n;
}

internal static nint /*n*/ opens(params ꓸꓸꓸnint xsʗp) {
    nint n = default!;
    var xs = xsʗp.sslice();

    // opens with a comment, then Go's blank line

    n = len(xs);
    return n;
}

internal static nint /*n*/ regrow(params ꓸꓸꓸnint xsʗp) {
    nint n = default!;
    var xs = xsʗp.sslice();

    n = len(xs);

    var more = new slice<nint>(n);
    n += len(more);
    return n;
}

internal static nint boxed() {
    nint p = 1;

    ref var q = ref heap<nint>(out var Ꮡq);
    q = p + 1;
    sink = Ꮡq;
    return q;
}

internal static nint boxedVar() {
    nint p = 1;

    ref var q = ref heap(new nint(), out var Ꮡq);

    sink = Ꮡq;
    return p + q;
}

} // end prelude_package

