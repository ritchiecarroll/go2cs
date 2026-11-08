// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using syntax = regexp.syntax_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using regexp;
using static go.regexp_package;

partial class regexp_internal_test_package {

partial struct runeMergeTestsᴛ1 /*dyn*/ {
    internal slice<rune> left, right, merged;
    internal slice<uint32> next;
    internal uint32 leftPC, rightPC;
}
internal static slice<runeMergeTestsᴛ1> runeMergeTests = [
    new(
        // empty rhs
        [69, 69],
        [],
        [69, 69],
        [1],
        1, 2
    ),
    new(
        // identical runes, identical targets
        [69, 69],
        [69, 69],
        [],
        [mergeFailed],
        1, 1
    ),
    new(
        // identical runes, different targets
        [69, 69],
        [69, 69],
        [],
        [mergeFailed],
        1, 2
    ),
    new(
        // append right-first
        [69, 69],
        [71, 71],
        [69, 69, 71, 71],
        [1, 2],
        1, 2
    ),
    new(
        // append, left-first
        [71, 71],
        [69, 69],
        [69, 69, 71, 71],
        [2, 1],
        1, 2
    ),
    new(
        // successful interleave
        [60, 60, 71, 71, 101, 101],
        [69, 69, 88, 88],
        [60, 60, 69, 69, 71, 71, 88, 88, 101, 101],
        [1, 2, 1, 2, 1],
        1, 2
    ),
    new(
        // left surrounds right
        [69, 74],
        [71, 71],
        [],
        [mergeFailed],
        1, 2
    ),
    new(
        // right surrounds left
        [69, 74],
        [68, 75],
        [],
        [mergeFailed],
        1, 2
    ),
    new(
        // overlap at interval begin
        [69, 74],
        [74, 75],
        [],
        [mergeFailed],
        1, 2
    ),
    new(
        // overlap ar interval end
        [69, 74],
        [65, 69],
        [],
        [mergeFailed],
        1, 2
    ),
    new(
        // overlap from above
        [69, 74],
        [71, 74],
        [],
        [mergeFailed],
        1, 2
    ),
    new(
        // overlap from below
        [69, 74],
        [65, 71],
        [],
        [mergeFailed],
        1, 2
    ),
    new(
        // out of order []rune
        [69, 74, 60, 65],
        [66, 67],
        [],
        [mergeFailed],
        1, 2
    )
];

public static void TestMergeRuneSet(ж<testing.T> t) {
    foreach (var (ix, vᴛ1) in range(runeMergeTests)) {
        ref var test = ref heap(new runeMergeTestsᴛ1(), out var Ꮡtest);
        test = vᴛ1;

        var (merged, next) = mergeRuneSets(Ꮡtest.of(runeMergeTestsᴛ1.Ꮡleft), Ꮡtest.of(runeMergeTestsᴛ1.Ꮡright), test.leftPC, test.rightPC);
        if (!slices.Equal<slice<rune>, rune>(merged, test.merged)) {
            t.Errorf("mergeRuneSet :%d (%v, %v) merged\n have\n%v\nwant\n%v", ix, test.left, test.right, merged, test.merged);
        }
        if (!slices.Equal<slice<uint32>, uint32>(next, test.next)) {
            t.Errorf("mergeRuneSet :%d(%v, %v) next\n have\n%v\nwant\n%v", ix, test.left, test.right, next, test.next);
        }
    }
}

partial struct onePassTestsᴛ1 /*dyn*/ {
    internal @string re;
    internal bool isOnePass;
}
internal static slice<onePassTestsᴛ1> onePassTests = slice<onePassTestsᴛ1>([
    new(@"^(?:a|(?:a*))$", false),
    new(@"^(?:(a)|(?:a*))$", false),
    new(@"^(?:(?:(?:.(?:$))?))$", true),
    new(@"^abcd$", true),
    new(@"^abcd", true),
    new(@"^(?:(?:a{0,})*?)$", false),
    new(@"^(?:(?:a+)*)$", true),
    new(@"^(?:(?:a|(?:aa)))$", true),
    new(@"^(?:[^\s\S])$", true),
    new(@"^(?:(?:a{3,4}){0,})$", false),
    new(@"^(?:(?:(?:a*)+))$", true),
    new(@"^[a-c]+$", true),
    new(@"^[a-c]*$", true),
    new(@"^(?:a*)$", true),
    new(@"^(?:(?:aa)|a)$", true),
    new(@"^[a-c]*", false),
    new(@"^...$", true),
    new(@"^...", true),
    new(@"^(?:a|(?:aa))$", true),
    new(@"^a((b))c$", true),
    new(@"^a.[l-nA-Cg-j]?e$", true),
    new(@"^a((b))$", true),
    new(@"^a(?:(b)|(c))c$", true),
    new(@"^a(?:(b*)|(c))c$", false),
    new(@"^a(?:b|c)$", true),
    new(@"^a(?:b?|c)$", true),
    new(@"^a(?:b?|c?)$", false),
    new(@"^a(?:b?|c+)$", true),
    new(@"^a(?:b+|(bc))d$", false),
    new(@"^a(?:bc)+$", true),
    new(@"^a(?:[bcd])+$", true),
    new(@"^a((?:[bcd])+)$", true),
    new(@"^a(:?b|c)*d$", true),
    new(@"^.bc(d|e)*$", true),
    new(@"^(?:(?:aa)|.)$", false),
    new(@"^(?:(?:a{1,2}){1,2})$", false),
    new(@"^l"u8 + strings.Repeat("o", 2 << 8) + @"ng$"u8, true)
]);

public static void TestCompileOnePass(ж<testing.T> t) {
    ж<syntax.Prog> p = default;
    ж<syntax.Regexp> re = default;
    error err = default;
    foreach (var (_, test) in range(onePassTests)) {
        (re, err) = syntax.Parse(test.re, syntax.Perl); if (err != nil) {
            t.Errorf("Parse(%q) got err:%s, want success", test.re, err);
            continue;
        }
        // needs to be done before compile...
        re = re.Simplify();
        (p, err) = syntax.Compile(re); if (err != nil) {
            t.Errorf("Compile(%q) got err:%s, want success", test.re, err);
            continue;
        }
        var isOnePass = compileOnePass(ref (p).DerefOrNull()) != nil;
        if (isOnePass != test.isOnePass) {
            t.Errorf("CompileOnePass(%q) got isOnePass=%v, expected %v", test.re, isOnePass, test.isOnePass);
        }
    }
}

// TODO(cespare): Unify with onePassTests and rationalize one-pass test cases.
partial struct onePassTests1ᴛ1 /*dyn*/ {
    internal @string re;
    internal @string match;
}
internal static slice<onePassTests1ᴛ1> onePassTests1 = [
    new(@"^a(/b+(#c+)*)*$", "a/b#c") // golang.org/issue/11905
];

public static void TestRunOnePass(ж<testing.T> t) {
    foreach (var (_, test) in range(onePassTests1)) {
        var (re, err) = Compile(test.re);
        if (err != nil) {
            t.Errorf("Compile(%q): got err: %s", test.re, err);
            continue;
        }
        if (re.onepass == nil) {
            t.Errorf("Compile(%q): got nil, want one-pass", test.re);
            continue;
        }
        if (!re.MatchString(test.match)) {
            t.Errorf("onepass %q did not match %q", test.re, test.match);
        }
    }
}

} // end regexp_internal_test_package
