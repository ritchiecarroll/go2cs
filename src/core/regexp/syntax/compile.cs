// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.regexp;

using unicode = unicode_package;

partial class syntax_package {

// A patchList is a list of instruction pointers that need to be filled in (patched).
// Because the pointers haven't been filled in yet, we can reuse their storage
// to hold the list. It's kind of sleazy, but works well in practice.
// See https://swtch.com/~rsc/regexp/regexp1.html for inspiration.
//
// These aren't really pointers: they're integers, so we can reinterpret them
// this way without using package unsafe. A value l.head denotes
// p.inst[l.head>>1].Out (l.head&1==0) or .Arg (l.head&1==1).
// head == 0 denotes the empty list, okay because we start every program
// with a fail instruction, so we'll never want to point at its output link.
partial struct patchList {
    internal uint32 head, tail;
}

internal static patchList makePatchList(uint32 n) {
    return new patchList(n, n);
}

internal static void patch(this patchList l, ж<Prog> Ꮡp, uint32 val) {
    ref var p = ref Ꮡp.DerefOrNull();

    var head = l.head;
    while (head != 0) {
        var i = Ꮡ(p.Inst, (head >> 1));
        if ((head & 1) == 0) {
            head = i.Out;
            i.Out = val;
        } else {
            head = i.Arg;
            i.Arg = val;
        }
    }
}

internal static patchList append(this patchList l1, ж<Prog> Ꮡp, patchList l2) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (l1.head == 0) {
        return l2;
    }
    if (l2.head == 0) {
        return l1;
    }

    var i = Ꮡ(p.Inst, (l1.tail >> 1));
    if ((l1.tail & 1) == 0) {
        i.Out = l2.head;
    } else {
        i.Arg = l2.head;
    }
    return new patchList(l1.head, l2.tail);
}

// A frag represents a compiled program fragment.
partial struct frag {
    internal uint32 i;       // index of first instruction
    internal patchList @out; // where to record end instruction
    internal bool nullable;  // whether fragment can match empty string
}

partial struct compiler {
    internal ж<Prog> p;
}

// Compile compiles the regexp into a program to be executed.
// The regexp should have been simplified already (returned from re.Simplify).
public static (ж<Prog>, error) Compile(ж<Regexp> re) {
    compiler c = default;
    c.init();
    var f = c.compile(re);
    f.@out.patch(c.p, c.inst(InstMatch).i);
    c.p.Start = (nint)f.i;
    return (c.p, default);
}

internal static void init(this ref compiler c) {
    c.p = @new<Prog>();
    c.p.NumCap = 2; // implicit ( and ) for whole match $0
    c.inst(InstFail);
}

internal static slice<rune> anyRuneNotNL = slice<rune>([0, (rune)'\n' - 1, (rune)'\n' + 1, unicode.MaxRune]);

internal static slice<rune> anyRune = slice<rune>([0, unicode.MaxRune]);

internal static frag compile(this ref compiler c, ж<Regexp> Ꮡre) {
    ref var re = ref Ꮡre.DerefOrNull();

    switch (re.Op) {
    case var ᴛ when ᴛ == OpNoMatch: {
        return c.fail();
    }
    case var ᴛ when ᴛ == OpEmptyMatch: {
        return c.nop();
    }
    case var ᴛ when ᴛ == OpLiteral: {
        if (len(re.Rune) == 0) {
            return c.nop();
        }
        frag f = default;
        foreach (var j in range(re.Rune)) {
            var f1 = c.rune(re.Rune.slice(j, j + 1), re.Flags);
            if (j == 0) {
                f = f1;
            } else {
                f = c.cat(f, f1);
            }
        }
        return f;
    }
    case var ᴛ when ᴛ == OpCharClass: {
        return c.rune(re.Rune, re.Flags);
    }
    case var ᴛ when ᴛ == OpAnyCharNotNL: {
        return c.rune(anyRuneNotNL, 0);
    }
    case var ᴛ when ᴛ == OpAnyChar: {
        return c.rune(anyRune, 0);
    }
    case var ᴛ when ᴛ == OpBeginLine: {
        return c.empty(EmptyBeginLine);
    }
    case var ᴛ when ᴛ == OpEndLine: {
        return c.empty(EmptyEndLine);
    }
    case var ᴛ when ᴛ == OpBeginText: {
        return c.empty(EmptyBeginText);
    }
    case var ᴛ when ᴛ == OpEndText: {
        return c.empty(EmptyEndText);
    }
    case var ᴛ when ᴛ == OpWordBoundary: {
        return c.empty(EmptyWordBoundary);
    }
    case var ᴛ when ᴛ == OpNoWordBoundary: {
        return c.empty(EmptyNoWordBoundary);
    }
    case var ᴛ when ᴛ == OpCapture: {
        var bra = c.cap((uint32)(re.Cap << 1));
        var sub = c.compile(re.Sub[0]);
        var ket = c.cap((uint32)(re.Cap << 1 | 1));
        return c.cat(c.cat(bra, sub), ket);
    }
    case var ᴛ when ᴛ == OpStar: {
        return c.star(c.compile(re.Sub[0]), (Flags)(re.Flags & NonGreedy) != 0);
    }
    case var ᴛ when ᴛ == OpPlus: {
        return c.plus(c.compile(re.Sub[0]), (Flags)(re.Flags & NonGreedy) != 0);
    }
    case var ᴛ when ᴛ == OpQuest: {
        return c.quest(c.compile(re.Sub[0]), (Flags)(re.Flags & NonGreedy) != 0);
    }
    case var ᴛ when ᴛ == OpConcat: {
        if (len(re.Sub) == 0) {
            return c.nop();
        }
        frag f = default;
        foreach (var (i, sub) in re.Sub) {
            if (i == 0) {
                f = c.compile(sub);
            } else {
                f = c.cat(f, c.compile(sub));
            }
        }
        return f;
    }
    case var ᴛ when ᴛ == OpAlternate: {
        frag f = default;
        foreach (var (_, sub) in re.Sub) {
            f = c.alt(f, c.compile(sub));
        }
        return f;
    }}
    throw panic("regexp: unhandled case in compile");
}

internal static frag inst(this ref compiler c, InstOp op) {
    // TODO: impose length limit
    var f = new frag(i: (uint32)len(c.p.Inst), nullable: true);
    c.p.Inst = builtin.append(c.p.Inst, new Inst(Op: op));
    return f;
}

internal static frag nop(this ref compiler c) {
    var f = c.inst(InstNop);
    f.@out = makePatchList(f.i << 1);
    return f;
}

internal static frag fail(this ref compiler c) {
    return new frag();
}

internal static frag cap(this ref compiler c, uint32 arg) {
    var f = c.inst(InstCapture);
    f.@out = makePatchList(f.i << 1);
    c.p.Inst[f.i].Arg = arg;

    if (c.p.NumCap < (nint)arg + 1) {
        c.p.NumCap = (nint)arg + 1;
    }
    return f;
}

internal static frag cat(this ref compiler c, frag f1, frag f2) {
    // concat of failure is failure
    if (f1.i == 0 || f2.i == 0) {
        return new frag();
    }

    // TODO: elide nop

    f1.@out.patch(c.p, f2.i);
    return new frag(f1.i, f2.@out, f1.nullable && f2.nullable);
}

internal static frag alt(this ref compiler c, frag f1, frag f2) {
    // alt of failure is other
    if (f1.i == 0) {
        return f2;
    }
    if (f2.i == 0) {
        return f1;
    }

    var f = c.inst(InstAlt);
    var i = Ꮡ(c.p.Inst, f.i);
    i.Out = f1.i;
    i.Arg = f2.i;
    f.@out = f1.@out.append(c.p, f2.@out);
    f.nullable = f1.nullable || f2.nullable;
    return f;
}

internal static frag quest(this ref compiler c, frag f1, bool nongreedy) {
    var f = c.inst(InstAlt);
    var i = Ꮡ(c.p.Inst, f.i);
    if (nongreedy) {
        i.Arg = f1.i;
        f.@out = makePatchList(f.i << 1);
    } else {
        i.Out = f1.i;
        f.@out = makePatchList(f.i << 1 | 1);
    }
    f.@out = f.@out.append(c.p, f1.@out);
    return f;
}

// loop returns the fragment for the main loop of a plus or star.
// For plus, it can be used after changing the entry to f1.i.
// For star, it can be used directly when f1 can't match an empty string.
// (When f1 can match an empty string, f1* must be implemented as (f1+)?
// to get the priority match order correct.)
internal static frag loop(this ref compiler c, frag f1, bool nongreedy) {
    var f = c.inst(InstAlt);
    var i = Ꮡ(c.p.Inst, f.i);
    if (nongreedy) {
        i.Arg = f1.i;
        f.@out = makePatchList(f.i << 1);
    } else {
        i.Out = f1.i;
        f.@out = makePatchList(f.i << 1 | 1);
    }
    f1.@out.patch(c.p, f.i);
    return f;
}

internal static frag star(this ref compiler c, frag f1, bool nongreedy) {
    if (f1.nullable) {
        // Use (f1+)? to get priority match order correct.
        // See golang.org/issue/46123.
        return c.quest(c.plus(f1, nongreedy), nongreedy);
    }
    return c.loop(f1, nongreedy);
}

internal static frag plus(this ref compiler c, frag f1, bool nongreedy) {
    return new frag(f1.i, c.loop(f1, nongreedy).@out, f1.nullable);
}

internal static frag empty(this ref compiler c, EmptyOp op) {
    var f = c.inst(InstEmptyWidth);
    c.p.Inst[f.i].Arg = (uint32)op;
    f.@out = makePatchList(f.i << 1);
    return f;
}

internal static frag rune(this ref compiler c, slice<rune> r, Flags flags) {
    var f = c.inst(InstRune);
    f.nullable = false;
    var i = Ꮡ(c.p.Inst, f.i);
    i.Rune = r;
    flags &= (Flags)(FoldCase); // only relevant flag is FoldCase
    if (len(r) != 1 || unicode.SimpleFold(r[0]) == r[0]) {
        // and sometimes not even that
        flags &= unchecked((Flags)~(Flags)(FoldCase));
    }
    i.Arg = (uint32)flags;
    f.@out = makePatchList(f.i << 1);

    // Special cases for exec machine.
    switch (ᐧ) {
    case {} when (Flags)(flags & FoldCase) == 0 && (len(r) == 1 || len(r) == 2 && r[0] == r[1]): {
        i.Op = InstRune1;
        break;
    }
    case {} when len(r) == 2 && r[0] == 0 && r[1] == unicode.MaxRune: {
        i.Op = InstRuneAny;
        break;
    }
    case {} when len(r) == 4 && r[0] == 0 && r[1] == (rune)'\n' - 1 && r[2] == (rune)'\n' + 1 && r[3] == unicode.MaxRune: {
        i.Op = InstRuneAnyNotNL;
        break;
    }}

    return f;
}

} // end syntax_package
