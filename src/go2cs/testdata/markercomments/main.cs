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
// and tagged, the struct tags of tagged, each in the spelling a comment can hold, and the array dims of
// the declarations below, a generic func's included (a lambda's and a local function's stay attributes).
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

[GoType("[2]array<nint>")] /*[2][3]*/ partial struct nn;

[GoType("ж<array<byte>>")] /*[4]*/ partial class P;

[GoType] partial struct holder {
    internal /*[3]*/ ж<array<nint>> p;
    [GoMapKeyDims(2)]
    internal /*[3]*/ map<array<@string>, array<nint>> m;
    internal /*[5]*/ slice<ж<array<byte>>> s;
    internal nint n; /* [9]*/
/* [7]*/
    internal /*[6]*/ ж<array<nint>> q;
}

internal static nint hash(/*[32]*/ array<byte> b) {
    b = b.Clone();

    return len(b);
}

internal static void fill(nint n, ref array<int32> p, /*[4][8]*/ array<array<byte>> grid) {
    grid = grid.Clone();

}

internal static T first<T>(/*[2]*/ array<T> a) {
    a = a.Clone();

    return a[0];
}

internal static nint noted(/*[4]*/ array<byte> a, nint b) {
    a = a.Clone();

    /* [9]*/
    /* [10]*/
    return len(a) + b;
}

internal static void put(this ref holder h, /*[3]*/ array<nint> v) {
    v = v.Clone();

}

internal static void Main() {
    var lambda = ([GoArrayDims(32)] array<byte> x) => {
        x = x.Clone();
        return len(x);
    };
    nint local([GoArrayDims(3)] array<nint> x) {
        x = x.Clone();
        return len(x);
    }
    any sink = (lambda).OrTypedNilFunc();
    holder h = default!;
    h.put(new nint[]{}.array(3));
    fmt.Println(new inline(nil), new marked(nil), new tagged(nil), new nn(new array<array<nint>>(2, () => new(3))), ((P)nil), h, hash(new byte[]{}.array(32)), first(new nint[]{}.array(2)), noted(new byte[]{}.array(4), 1), local(new nint[]{}.array(3)), sink != default!);
    fill(0, ref ((ж<array<int32>>)default!).DerefOrNull(), new array<byte>[]{}.array(4, () => new(8)));
}

} // end main_package
