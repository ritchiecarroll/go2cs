// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.regexp;

partial class syntax_package {

// Simplify returns a regexp equivalent to re but without counted repetitions
// and with various other simplifications, such as rewriting /(?:a+)+/ to /a+/.
// The resulting regexp will execute correctly but its string representation
// will not produce the same parse tree, because capturing parentheses
// may have been duplicated or removed. For example, the simplified form
// for /(x){1,2}/ is /(x)(x)?/ but both parentheses capture as $1.
// The returned regexp may share structure with or be the original.
public static ж<Regexp> Simplify(this ж<Regexp> Ꮡre) {
    ref var re = ref Ꮡre.DerefOrNull();

    if (Ꮡre == nil) {
        return default!;
    }
    var exprᴛ1 = re.Op;
    if (exprᴛ1 == OpCapture || exprᴛ1 == OpConcat || exprᴛ1 == OpAlternate) {
        var nre = Ꮡre;
        foreach (var (i, sub) in re.Sub) {
            // Simplify children, building new Regexp if children change.
            var nsub = sub.Simplify();
            if (nre == Ꮡre && nsub != sub) {
                // Start a copy.
                nre = @new<Regexp>();
                nre.Value = re.ΔClone();
                nre.Value.Rune = default!;
                nre.Value.Sub = builtin.appendꓸꓸꓸ((~nre).Sub0[..0], re.Sub.slice(0, i));
            }
            if (nre != Ꮡre) {
                nre.Value.Sub = builtin.append((~nre).Sub, nsub);
            }
        }
        return nre;
    }
    if (exprᴛ1 == OpStar || exprᴛ1 == OpPlus || exprᴛ1 == OpQuest) {
        var sub = re.Sub[0].Simplify();
        return simplify1(re.Op, re.Flags, sub, Ꮡre);
    }
    if (exprᴛ1 == OpRepeat) {
        if (re.Min == 0 && re.Max == 0) {
            // Special special case: x{0} matches the empty string
            // and doesn't even need to consider x.
            return Ꮡ(new Regexp(Op: OpEmptyMatch));
        }
        var sub = re.Sub[0].Simplify();
        if (re.Max == -1) {
            // The fun begins.
            // x{n,} means at least n matches of x.
            // Special case: x{0,} is x*.
            if (re.Min == 0) {
                return simplify1(OpStar, re.Flags, sub, nil);
            }
            // Special case: x{1,} is x+.
            if (re.Min == 1) {
                return simplify1(OpPlus, re.Flags, sub, nil);
            }
            // General case: x{4,} is xxxx+.
            var nre = Ꮡ(new Regexp(Op: OpConcat));
            nre.Value.Sub = (~nre).Sub0[..0];
            for (nint i = 0; i < re.Min - 1; i++) {
                nre.Value.Sub = builtin.append((~nre).Sub, sub);
            }
            nre.Value.Sub = builtin.append((~nre).Sub, simplify1(OpPlus, re.Flags, sub, nil));
            return nre;
        }
        if (re.Min == 1 && re.Max == 1) {
            // Special case x{0} handled above.
            // Special case: x{1} is just x.
            return sub;
        }
// General case: x{n,m} means n copies of x and m copies of x?
// The machine will do less work if we nest the final m copies,
// so that x{2,5} = xx(x(x(x)?)?)?

        // Build leading prefix: xx.
        ж<Regexp> prefix = default!;
        if (re.Min > 0) {
            prefix = Ꮡ(new Regexp(Op: OpConcat));
            prefix.Value.Sub = (~prefix).Sub0[..0];
            for (nint i = 0; i < re.Min; i++) {
                prefix.Value.Sub = builtin.append((~prefix).Sub, sub);
            }
        }
        if (re.Max > re.Min) {
            // Build and attach suffix: (x(x(x)?)?)?
            var suffix = simplify1(OpQuest, re.Flags, sub, nil);
            for (nint i = re.Min + 1; i < re.Max; i++) {
                var nre2 = Ꮡ(new Regexp(Op: OpConcat));
                nre2.Value.Sub = builtin.append((~nre2).Sub0[..0], sub, suffix);
                suffix = simplify1(OpQuest, re.Flags, nre2, nil);
            }
            if (prefix == nil) {
                return suffix;
            }
            prefix.Value.Sub = builtin.append((~prefix).Sub, suffix);
        }
        if (prefix != nil) {
            return prefix;
        }
        return Ꮡ(new Regexp( // Some degenerate case like min > max or min < max < 0.
 // Handle as impossible match.
Op: OpNoMatch));
    }

    return Ꮡre;
}

// simplify1 implements Simplify for the unary OpStar,
// OpPlus, and OpQuest operators. It returns the simple regexp
// equivalent to
//
//	Regexp{Op: op, Flags: flags, Sub: {sub}}
//
// under the assumption that sub is already simple, and
// without first allocating that structure. If the regexp
// to be returned turns out to be equivalent to re, simplify1
// returns re instead.
//
// simplify1 is factored out of Simplify because the implementation
// for other operators generates these unary expressions.
// Letting them call simplify1 makes sure the expressions they
// generate are simple.
internal static ж<Regexp> simplify1(Op op, Flags flags, ж<Regexp> Ꮡsub, ж<Regexp> Ꮡre) {
    ref var sub = ref Ꮡsub.DerefOrNull();
    ref var re = ref Ꮡre.DerefOrNull();

    // Special case: repeat the empty string as much as
    // you want, but it's still the empty string.
    if (sub.Op == OpEmptyMatch) {
        return Ꮡsub;
    }
    // The operators are idempotent if the flags match.
    if (op == sub.Op && (Flags)(flags & NonGreedy) == (Flags)(sub.Flags & NonGreedy)) {
        return Ꮡsub;
    }
    if (Ꮡre != nil && re.Op == op && (Flags)(re.Flags & NonGreedy) == (Flags)(flags & NonGreedy) && Ꮡsub == re.Sub[0]) {
        return Ꮡre;
    }
    Ꮡre = Ꮡ(new Regexp(Op: op, Flags: flags)); re = ref Ꮡre.DerefOrNull();
    re.Sub = builtin.append(re.Sub0[..0], Ꮡsub);
    return Ꮡre;
}

} // end syntax_package
