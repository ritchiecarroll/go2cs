// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static go.strings_package;

partial class strings_internal_test_package {

internal static any ΔReplacer(this ж<global::go.strings_package.Replacer> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    Ꮡr.of(global::go.strings_package.Replacer.Ꮡonce).Do(Ꮡr.buildOnce);
    return r.r;
}

internal static @string PrintTrie(this ж<global::go.strings_package.Replacer> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    Ꮡr.of(global::go.strings_package.Replacer.Ꮡonce).Do(Ꮡr.buildOnce);
    var gen = r.r._<ж<global::go.strings_package.genericReplacer>>();
    return gen.printNode(gen.of(global::go.strings_package.genericReplacer.Ꮡroot), 0);
}

internal static @string /*s*/ printNode(this ref global::go.strings_package.genericReplacer r, ж<global::go.strings_package.trieNode> Ꮡt, nint depth) {
    @string s = default!;

    ref var t = ref Ꮡt.DerefOrNull();
    if (t.priority > 0){
        s += "+"u8;
    } else {
        s += "-"u8;
    }
    s += "\n"u8;
    if (t.prefix != ""u8){
        s += Repeat("."u8, depth) + t.prefix;
        s += r.printNode(t.next, depth + len(t.prefix));
    } else 
    if (t.table != default!) {
        foreach (var (b, m) in r.mapping.ΔRangeSnapshot()) {
            if ((nint)m != r.tableSize && t.table[m] != nil) {
                s += Repeat("."u8, depth) + ((@string)new byte[]{(byte)b}.slice());
                s += r.printNode(t.table[m], depth + 1);
            }
        }
    }
    return s;
}

public static nint StringFind(@string pattern, @string text) {
    return makeStringFinder(pattern).next(text);
}

public static (slice<nint>, slice<nint>) DumpTables(@string pattern) {
    var finder = makeStringFinder(pattern);
    return ((~finder).badCharSkip[..], (~finder).goodSuffixSkip);
}

} // end strings_internal_test_package
