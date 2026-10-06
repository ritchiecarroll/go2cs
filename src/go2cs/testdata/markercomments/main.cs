// main.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Go comments shaped like the member facts go2cs-gen reads (docs/PLAN-marker-comment-parity.md, 6).
// Converted with -comments, each is carried re-spelled, so the records go2cs-gen writes for this file
// are those of the same source without them: the two real embeds of marked, and nothing else.
namespace go.example.com;

using fmt = fmt_package;

partial class main_package {

[GoType] partial interface Reader {
    nint Read();
}

[GoType] partial struct inline {
/* embed*/
    internal nint x;
}

[GoType] partial struct marked {
/* embed*/
    internal nint a;
    /* embed*/
    internal nint b;
    internal nint c; /* embed*/
    internal nint d;
/* embed*/
    /*embed*/ public Reader Reader;
    /*embed*/ internal nint @int;
}

internal static void Main() {
    fmt.Println(new inline(nil), new marked(nil));
}

} // end main_package
