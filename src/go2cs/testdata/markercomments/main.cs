// main.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Go comments shaped like the member facts go2cs-gen reads (docs/PLAN-marker-comment-parity.md, 6),
// beside the real facts. Converted with -comments, each Go comment is carried re-spelled, so the records
// go2cs-gen writes for this file are those of the same source without them: the real embeds of marked
// and tagged, and the struct tags of tagged, each in the spelling a comment can hold.
namespace go.example.com;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Inner {
    internal nint n;
}

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

[GoType] partial struct tagged {
    public @string Plain; /*`json:"plain"`*/
    public nint Grouped, Pair; /*`json:"g"`*/
    public nint Mixed; /*`json:"m"`*/
    internal nint mixed; /*`json:"m"`*/
    public @string Quoted; /*"a:\"`b`\""*/
    public @string Closer; /*"x:\"*\x2f\""*/
    public @string Tabbed; /*"t:\"\t\""*/
    public @string Separator; /*"u:\"\u2028\""*/
    public @string Wide; /*`json:"ü"`*/
    public @string Spaced; /*`json:"s"`*/ /* `json:"other"`*/
    public nint Bare;   /* `json:"bare"`*/
    public nint Quote;   /* "json:\"q\""*/
    public nint Empty;
    public partial ref Inner Inner { get; } /*`json:"inner"`*/
    /*embed*/ public fmt_package.Stringer Stringer; /*`json:"str"`*/
}

internal static void Main() {
    fmt.Println(new inline(nil), new marked(nil), new tagged(nil));
}

} // end main_package
