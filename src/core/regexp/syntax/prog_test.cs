// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.regexp;

using testing = testing_package;
using static go.regexp.syntax_package;

partial class syntax_internal_test_package {


partial struct compileTestsᴛ1 /*dyn*/ {
    public @string Regexp;
    public @string Prog;
}
internal static slice<compileTestsᴛ1> compileTests = new compileTestsᴛ1[]{
    new("a"u8, """
  0	fail
  1*	rune1 "a" -> 2
  2	match

"""u8),
    new("[A-M][n-z]"u8, """
  0	fail
  1*	rune "AM" -> 2
  2	rune "nz" -> 3
  3	match

"""u8),
    new(""u8, """
  0	fail
  1*	nop -> 2
  2	match

"""u8),
    new("a?"u8, """
  0	fail
  1	rune1 "a" -> 3
  2*	alt -> 1, 3
  3	match

"""u8),
    new("a??"u8, """
  0	fail
  1	rune1 "a" -> 3
  2*	alt -> 3, 1
  3	match

"""u8),
    new("a+"u8, """
  0	fail
  1*	rune1 "a" -> 2
  2	alt -> 1, 3
  3	match

"""u8),
    new("a+?"u8, """
  0	fail
  1*	rune1 "a" -> 2
  2	alt -> 3, 1
  3	match

"""u8),
    new("a*"u8, """
  0	fail
  1	rune1 "a" -> 2
  2*	alt -> 1, 3
  3	match

"""u8),
    new("a*?"u8, """
  0	fail
  1	rune1 "a" -> 2
  2*	alt -> 3, 1
  3	match

"""u8),
    new("a+b+"u8, """
  0	fail
  1*	rune1 "a" -> 2
  2	alt -> 1, 3
  3	rune1 "b" -> 4
  4	alt -> 3, 5
  5	match

"""u8),
    new("(a+)(b+)"u8, """
  0	fail
  1*	cap 2 -> 2
  2	rune1 "a" -> 3
  3	alt -> 2, 4
  4	cap 3 -> 5
  5	cap 4 -> 6
  6	rune1 "b" -> 7
  7	alt -> 6, 8
  8	cap 5 -> 9
  9	match

"""u8),
    new("a+|b+"u8, """
  0	fail
  1	rune1 "a" -> 2
  2	alt -> 1, 6
  3	rune1 "b" -> 4
  4	alt -> 3, 6
  5*	alt -> 1, 3
  6	match

"""u8),
    new("A[Aa]"u8, """
  0	fail
  1*	rune1 "A" -> 2
  2	rune "A"/i -> 3
  3	match

"""u8),
    new("(?:(?:^).)"u8, """
  0	fail
  1*	empty 4 -> 2
  2	anynotnl -> 3
  3	match

"""u8),
    new("(?:|a)+"u8, """
  0	fail
  1	nop -> 4
  2	rune1 "a" -> 4
  3*	alt -> 1, 2
  4	alt -> 3, 5
  5	match

"""u8),
    new("(?:|a)*"u8, """
  0	fail
  1	nop -> 4
  2	rune1 "a" -> 4
  3	alt -> 1, 2
  4	alt -> 3, 6
  5*	alt -> 3, 6
  6	match

"""u8)
}.slice();

public static void TestCompile(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in compileTests) {
        var (re, _) = Parse(tt.Regexp, Perl);
        var (p, _) = Compile(re);
        @string s = p.String();
        if (s != tt.Prog) {
            Ꮡt.Errorf("compiled %#q:\n--- have\n%s---\n--- want\n%s---"u8, tt.Regexp, s, tt.Prog);
        }
    }
}

public static void BenchmarkEmptyOpContext(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        rune r1 = -1;
        foreach (var (_, r2) in (@string)"foo, bar, baz\nsome input text.\n"u8) {
            EmptyOpContext(r1, r2);
            r1 = r2;
        }
        EmptyOpContext(r1, -1);
    }
}

internal static any sink;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object benchmarkDidNotRunˢ = (@string)"Benchmark did not run"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string charsᶜ = "Don't communicate by sharing memory, share memory by communicating."u8;

public static void BenchmarkIsWordChar(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string chars = charsᶜ;
    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, r) in (@string)chars) {
            sink = IsWordChar(r);
        }
    }
    if (sink == default!) {
        Ꮡb.Fatal(benchmarkDidNotRunˢ);
    }
    sink = default!;
}

} // end syntax_internal_test_package
