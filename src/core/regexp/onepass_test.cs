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

// empty rhs
// identical runes, identical targets
// identical runes, different targets
// append right-first
// append, left-first
// successful interleave
// left surrounds right
// right surrounds left
// overlap at interval begin
// overlap ar interval end
// overlap from above
// overlap from below
// out of order []rune

partial struct runeMergeTestsᴛ1 /*dyn*/ {
    internal slice<rune> left, right, merged;
    internal slice<uint32> next;
    internal uint32 leftPC, rightPC;
}
internal static slice<runeMergeTestsᴛ1> runeMergeTests = new runeMergeTestsᴛ1[]{
    new(
        new rune[]{69, 69}.slice(),
        new rune[]{}.slice(),
        new rune[]{69, 69}.slice(),
        new uint32[]{1}.slice(),
        1, 2
    ),
    new(
        new rune[]{69, 69}.slice(),
        new rune[]{69, 69}.slice(),
        new rune[]{}.slice(),
        new uint32[]{mergeFailed}.slice(),
        1, 1
    ),
    new(
        new rune[]{69, 69}.slice(),
        new rune[]{69, 69}.slice(),
        new rune[]{}.slice(),
        new uint32[]{mergeFailed}.slice(),
        1, 2
    ),
    new(
        new rune[]{69, 69}.slice(),
        new rune[]{71, 71}.slice(),
        new rune[]{69, 69, 71, 71}.slice(),
        new uint32[]{1, 2}.slice(),
        1, 2
    ),
    new(
        new rune[]{71, 71}.slice(),
        new rune[]{69, 69}.slice(),
        new rune[]{69, 69, 71, 71}.slice(),
        new uint32[]{2, 1}.slice(),
        1, 2
    ),
    new(
        new rune[]{60, 60, 71, 71, 101, 101}.slice(),
        new rune[]{69, 69, 88, 88}.slice(),
        new rune[]{60, 60, 69, 69, 71, 71, 88, 88, 101, 101}.slice(),
        new uint32[]{1, 2, 1, 2, 1}.slice(),
        1, 2
    ),
    new(
        new rune[]{69, 74}.slice(),
        new rune[]{71, 71}.slice(),
        new rune[]{}.slice(),
        new uint32[]{mergeFailed}.slice(),
        1, 2
    ),
    new(
        new rune[]{69, 74}.slice(),
        new rune[]{68, 75}.slice(),
        new rune[]{}.slice(),
        new uint32[]{mergeFailed}.slice(),
        1, 2
    ),
    new(
        new rune[]{69, 74}.slice(),
        new rune[]{74, 75}.slice(),
        new rune[]{}.slice(),
        new uint32[]{mergeFailed}.slice(),
        1, 2
    ),
    new(
        new rune[]{69, 74}.slice(),
        new rune[]{65, 69}.slice(),
        new rune[]{}.slice(),
        new uint32[]{mergeFailed}.slice(),
        1, 2
    ),
    new(
        new rune[]{69, 74}.slice(),
        new rune[]{71, 74}.slice(),
        new rune[]{}.slice(),
        new uint32[]{mergeFailed}.slice(),
        1, 2
    ),
    new(
        new rune[]{69, 74}.slice(),
        new rune[]{65, 71}.slice(),
        new rune[]{}.slice(),
        new uint32[]{mergeFailed}.slice(),
        1, 2
    ),
    new(
        new rune[]{69, 74, 60, 65}.slice(),
        new rune[]{66, 67}.slice(),
        new rune[]{}.slice(),
        new uint32[]{mergeFailed}.slice(),
        1, 2
    )
}.slice();

public static void TestMergeRuneSet(ж<testing.T> Ꮡt) {
    foreach (var (ix, vᴛ1) in runeMergeTests) {
        ref var test = ref heap(new runeMergeTestsᴛ1(), out var Ꮡtest);
        test = vᴛ1;

        var (merged, next) = mergeRuneSets(Ꮡtest.of(runeMergeTestsᴛ1.Ꮡleft), Ꮡtest.of(runeMergeTestsᴛ1.Ꮡright), test.leftPC, test.rightPC);
        if (!slices.Equal<slice<rune>, rune>(merged, test.merged)) {
            Ꮡt.Errorf("mergeRuneSet :%d (%v, %v) merged\n have\n%v\nwant\n%v"u8, ix, test.left, test.right, merged, test.merged);
        }
        if (!slices.Equal<slice<uint32>, uint32>(next, test.next)) {
            Ꮡt.Errorf("mergeRuneSet :%d(%v, %v) next\n have\n%v\nwant\n%v"u8, ix, test.left, test.right, next, test.next);
        }
    }
}


partial struct onePassTestsᴛ1 /*dyn*/ {
    internal @string re;
    internal bool isOnePass;
}
internal static slice<onePassTestsᴛ1> onePassTests = new onePassTestsᴛ1[]{
    new(@"^(?:a|(?:a*))$"u8, false),
    new(@"^(?:(a)|(?:a*))$"u8, false),
    new(@"^(?:(?:(?:.(?:$))?))$"u8, true),
    new(@"^abcd$"u8, true),
    new(@"^abcd"u8, true),
    new(@"^(?:(?:a{0,})*?)$"u8, false),
    new(@"^(?:(?:a+)*)$"u8, true),
    new(@"^(?:(?:a|(?:aa)))$"u8, true),
    new(@"^(?:[^\s\S])$"u8, true),
    new(@"^(?:(?:a{3,4}){0,})$"u8, false),
    new(@"^(?:(?:(?:a*)+))$"u8, true),
    new(@"^[a-c]+$"u8, true),
    new(@"^[a-c]*$"u8, true),
    new(@"^(?:a*)$"u8, true),
    new(@"^(?:(?:aa)|a)$"u8, true),
    new(@"^[a-c]*"u8, false),
    new(@"^...$"u8, true),
    new(@"^..."u8, true),
    new(@"^(?:a|(?:aa))$"u8, true),
    new(@"^a((b))c$"u8, true),
    new(@"^a.[l-nA-Cg-j]?e$"u8, true),
    new(@"^a((b))$"u8, true),
    new(@"^a(?:(b)|(c))c$"u8, true),
    new(@"^a(?:(b*)|(c))c$"u8, false),
    new(@"^a(?:b|c)$"u8, true),
    new(@"^a(?:b?|c)$"u8, true),
    new(@"^a(?:b?|c?)$"u8, false),
    new(@"^a(?:b?|c+)$"u8, true),
    new(@"^a(?:b+|(bc))d$"u8, false),
    new(@"^a(?:bc)+$"u8, true),
    new(@"^a(?:[bcd])+$"u8, true),
    new(@"^a((?:[bcd])+)$"u8, true),
    new(@"^a(:?b|c)*d$"u8, true),
    new(@"^.bc(d|e)*$"u8, true),
    new(@"^(?:(?:aa)|.)$"u8, false),
    new(@"^(?:(?:a{1,2}){1,2})$"u8, false),
    new(@"^l"u8 + strings.Repeat("o"u8, (2 << (int)(8))) + @"ng$"u8, true)
}.slice();

public static void TestCompileOnePass(ж<testing.T> Ꮡt) {
    ж<syntax.Prog> p = default!;
    ж<syntax.Regexp> re = default!;
    error err = default!;
    foreach (var (_, test) in onePassTests) {
        {
            (re, err) = syntax.Parse(test.re, syntax.Perl); if (err != default!) {
                Ꮡt.Errorf("Parse(%q) got err:%s, want success"u8, test.re, err);
                continue;
            }
        }
        // needs to be done before compile...
        re = re.Simplify();
        {
            (p, err) = syntax.Compile(re); if (err != default!) {
                Ꮡt.Errorf("Compile(%q) got err:%s, want success"u8, test.re, err);
                continue;
            }
        }
        var isOnePass = compileOnePass(ref (p).DerefOrNull()) != nil;
        if (isOnePass != test.isOnePass) {
            Ꮡt.Errorf("CompileOnePass(%q) got isOnePass=%v, expected %v"u8, test.re, isOnePass, test.isOnePass);
        }
    }
}

// golang.org/issue/11905
// TODO(cespare): Unify with onePassTests and rationalize one-pass test cases.

partial struct onePassTests1ᴛ1 /*dyn*/ {
    internal @string re;
    internal @string match;
}
internal static slice<onePassTests1ᴛ1> onePassTests1 = new onePassTests1ᴛ1[]{
    new(@"^a(/b+(#c+)*)*$"u8, "a/b#c"u8)
}.slice();

public static void TestRunOnePass(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in onePassTests1) {
        var (re, err) = Compile(test.re);
        if (err != default!) {
            Ꮡt.Errorf("Compile(%q): got err: %s"u8, test.re, err);
            continue;
        }
        if ((~re).onepass == nil) {
            Ꮡt.Errorf("Compile(%q): got nil, want one-pass"u8, test.re);
            continue;
        }
        if (!re.MatchString(test.match)) {
            Ꮡt.Errorf("onepass %q did not match %q"u8, test.re, test.match);
        }
    }
}

} // end regexp_internal_test_package
