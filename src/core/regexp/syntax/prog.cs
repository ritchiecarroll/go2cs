// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.regexp;

using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;
using ꓸꓸꓸstring = Span<@string>;

partial class syntax_package {

// Compiled program.
// May not belong in this package, but convenient for now.

// A Prog is a compiled regular expression program.
partial struct Prog {
    public slice<Inst> Inst;
    public nint Start;  // index of start instruction
    public nint NumCap; // number of InstCapture insts in re
}

// An InstOp is an instruction opcode.
partial struct InstOp /*num:uint8*/;

public static InstOp InstAlt => /* iota */ 0;
public static InstOp InstAltMatch => 1;
public static InstOp InstCapture => 2;
public static InstOp InstEmptyWidth => 3;
public static InstOp InstMatch => 4;
public static InstOp InstFail => 5;
public static InstOp InstNop => 6;
public static InstOp InstRune => 7;
public static InstOp InstRune1 => 8;
public static InstOp InstRuneAny => 9;
public static InstOp InstRuneAnyNotNL => 10;

internal static slice<@string> instOpNames = slice<@string>([
    "InstAlt"u8,
    "InstAltMatch"u8,
    "InstCapture"u8,
    "InstEmptyWidth"u8,
    "InstMatch"u8,
    "InstFail"u8,
    "InstNop"u8,
    "InstRune"u8,
    "InstRune1"u8,
    "InstRuneAny"u8,
    "InstRuneAnyNotNL"u8
]);

public static @string String(this InstOp i) {
    if ((nuint)i >= (nuint)len(instOpNames)) {
        return ""u8;
    }
    return instOpNames[i];
}

// An EmptyOp specifies a kind or mixture of zero-width assertions.
partial struct EmptyOp /*num:uint8*/;

public static EmptyOp EmptyBeginLine => /* 1 << iota */ 1;
public static EmptyOp EmptyEndLine => 2;
public static EmptyOp EmptyBeginText => 4;
public static EmptyOp EmptyEndText => 8;
public static EmptyOp EmptyWordBoundary => 16;
public static EmptyOp EmptyNoWordBoundary => 32;

// EmptyOpContext returns the zero-width assertions
// satisfied at the position between the runes r1 and r2.
// Passing r1 == -1 indicates that the position is
// at the beginning of the text.
// Passing r2 == -1 indicates that the position is
// at the end of the text.
public static EmptyOp EmptyOpContext(rune r1, rune r2) {
    EmptyOp op = EmptyNoWordBoundary;
    byte boundary = default;
    switch (ᐧ) {
    case {} when IsWordChar(r1): {
        boundary = 1;
        break;
    }
    case {} when r1 is '\n': {
        op |= (EmptyOp)(EmptyBeginLine);
        break;
    }
    case {} when r1 is < 0: {
        op |= (EmptyOp)((EmptyOp)(EmptyBeginText | EmptyBeginLine));
        break;
    }}
    switch (ᐧ) {
    case {} when IsWordChar(r2): {
        boundary ^= (byte)(1);
        break;
    }
    case {} when r2 is '\n': {
        op |= (EmptyOp)(EmptyEndLine);
        break;
    }
    case {} when r2 is < 0: {
        op |= (EmptyOp)((EmptyOp)(EmptyEndText | EmptyEndLine));
        break;
    }}
    if (boundary != 0) { // IsWordChar(r1) != IsWordChar(r2)
        op ^= (EmptyOp)(((EmptyOp)(EmptyWordBoundary | EmptyNoWordBoundary)));
    }
    return op;
}

// IsWordChar reports whether r is considered a “word character”
// during the evaluation of the \b and \B zero-width assertions.
// These assertions are ASCII-only: the word characters are [A-Za-z0-9_].
public static bool IsWordChar(rune r) {
    // Test for lowercase letters first, as these occur more
    // frequently than uppercase letters in common cases.
    return 'a' <= r && r <= 'z' || 'A' <= r && r <= 'Z' || '0' <= r && r <= '9' || r == '_';
}

// An Inst is a single instruction in a regular expression program.
partial struct Inst {
    public InstOp Op;
    public uint32 Out; // all but InstMatch, InstFail
    public uint32 Arg; // InstAlt, InstAltMatch, InstCapture, InstEmptyWidth
    public slice<rune> Rune;
}

public static @string String(this ж<Prog> Ꮡp) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    dumpProg(Ꮡb, Ꮡp);
    return b.String();
}

// skipNop follows any no-op or capturing instructions.
internal static ж<Inst> skipNop(this ref Prog p, uint32 pc) {
    var i = Ꮡ(p.Inst, pc);
    while (i.Op == InstNop || i.Op == InstCapture) {
        i = Ꮡ(p.Inst, i.Out);
    }
    return i;
}

// op returns i.Op but merges all the Rune special cases into InstRune
internal static InstOp op(this ref Inst i) {
    var op = i.Op;
    switch (op) {
    case var ᴛ when ᴛ == InstRune1 || ᴛ == InstRuneAny || ᴛ == InstRuneAnyNotNL: {
        op = InstRune;
        break;
    }}
    return op;
}

// Prefix returns a literal string that all matches for the
// regexp must start with. Complete is true if the prefix
// is the entire match.
public static (@string prefix, bool complete) Prefix(this ref Prog p) {
    var i = p.skipNop((uint32)p.Start);

    // Avoid allocation of buffer if prefix is empty.
    if (i.op() != InstRune || len(i.Rune) != 1) {
        return ("", i.Op == InstMatch);
    }

    // Have prefix; gather characters.
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    while (i.op() == InstRune && len(i.Rune) == 1 && (Flags)(((Flags)(uint16)i.Arg) & FoldCase) == 0 && i.Rune[0] != utf8.RuneError) {
        Ꮡbuf.WriteRune(i.Rune[0]);
        i = p.skipNop(i.Out);
    }
    return (buf.String(), i.Op == InstMatch);
}

// StartCond returns the leading empty-width conditions that must
// be true in any match. It returns ^EmptyOp(0) if no matches are possible.
public static EmptyOp StartCond(this ref Prog p) {
    EmptyOp flag = default;
    var pc = (uint32)p.Start;
    var i = Ꮡ(p.Inst, pc);
Loop:
    for (;;) {
        switch (i.Op) {
        case var ᴛ when ᴛ == InstEmptyWidth: {
            flag |= (EmptyOp)(((EmptyOp)(uint8)i.Arg));
            break;
        }
        case var ᴛ when ᴛ == InstFail: {
            return (EmptyOp)(~((EmptyOp)((EmptyOp)0)));
        }
        case var ᴛ when ᴛ == InstCapture || ᴛ == InstNop: {
            // skip
            break;
        }
        default: {
            goto break_Loop;
            break;
        }}
        pc = i.Out;
        i = Ꮡ(p.Inst, pc);
continue_Loop:;
    }
break_Loop:;
    return flag;
}

internal static UntypedInt noMatch => -1;

// MatchRune reports whether the instruction matches (and consumes) r.
// It should only be called when i.Op == [InstRune].
public static bool MatchRune(this ref Inst i, rune r) {
    return i.MatchRunePos(r) != noMatch;
}

// MatchRunePos checks whether the instruction matches (and consumes) r.
// If so, MatchRunePos returns the index of the matching rune pair
// (or, when len(i.Rune) == 1, rune singleton).
// If not, MatchRunePos returns -1.
// MatchRunePos should only be called when i.Op == [InstRune].
public static nint MatchRunePos(this ref Inst i, rune r) {
    var rune = i.Rune;

    switch (len(rune)) {
    case 0: {
        return noMatch;
    }
    case 1: {
        // Special case: single-rune slice is from literal string, not char class.
        var r0 = rune[0];
        if (r == r0) {
            return 0;
        }
        if ((Flags)(((Flags)(uint16)i.Arg) & FoldCase) != 0) {
            for (var r1 = unicode.SimpleFold(r0); r1 != r0; r1 = unicode.SimpleFold(r1)) {
                if (r == r1) {
                    return 0;
                }
            }
        }
        return noMatch;
    }
    case 2: {
        if (r >= rune[0] && r <= rune[1]) {
            return 0;
        }
        return noMatch;
    }
    case 4 or 6 or 8: {
        // Linear search for a few pairs.
        // Should handle ASCII well.
        for (nint j = 0; j < len(rune); j += 2) {
            if (r < rune[j]) {
                return noMatch;
            }
            if (r <= rune[j + 1]) {
                return j / 2;
            }
        }
        return noMatch;
    }}

    // Otherwise binary search.
    nint lo = 0;
    nint hi = len(rune) / 2;
    while (lo < hi) {
        nint m = (nint)((nuint)(lo + hi) >> 1);
        {
            var c = rune[2 * m]; if (c <= r) {
                if (r <= rune[2 * m + 1]) {
                    return m;
                }
                lo = m + 1;
            } else {
                hi = m;
            }
        }
    }
    return noMatch;
}

// MatchEmptyWidth reports whether the instruction matches
// an empty string between the runes before and after.
// It should only be called when i.Op == [InstEmptyWidth].
public static bool MatchEmptyWidth(this ref Inst i, rune before, rune after) {
    switch (((EmptyOp)(uint8)i.Arg)) {
    case var ᴛ when ᴛ == EmptyBeginLine: {
        return before == '\n' || before == -1;
    }
    case var ᴛ when ᴛ == EmptyEndLine: {
        return after == '\n' || after == -1;
    }
    case var ᴛ when ᴛ == EmptyBeginText: {
        return before == -1;
    }
    case var ᴛ when ᴛ == EmptyEndText: {
        return after == -1;
    }
    case var ᴛ when ᴛ == EmptyWordBoundary: {
        return IsWordChar(before) != IsWordChar(after);
    }
    case var ᴛ when ᴛ == EmptyNoWordBoundary: {
        return IsWordChar(before) == IsWordChar(after);
    }}
    throw panic("unknown empty width arg");
}

public static @string String(this ж<Inst> Ꮡi) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    dumpInst(Ꮡb, ref (Ꮡi).DerefOrNull());
    return b.String();
}

internal static void bw(ж<strings.Builder> b, params ꓸꓸꓸstring argsʗp) {
    var args = argsʗp.sslice();

    foreach (var (_, s) in args) {
        b.WriteString(s);
    }
}

internal static void dumpProg(ж<strings.Builder> b, ж<Prog> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    foreach (var j in range(p.Inst)) {
        var i = Ꮡ(p.Inst, j);
        @string pc = strconv.Itoa(j);
        if (len(pc) < 3) {
            b.WriteString("   "u8.slice(len(pc)));
        }
        if (j == p.Start) {
            pc += "*"u8;
        }
        bw(b, pc, "\t");
        dumpInst(b, ref (i).DerefOrNull());
        bw(b, "\n"u8);
    }
}

internal static @string u32(uint32 i) {
    return strconv.FormatUint((uint64)i, 10);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string altˢ = "alt -> "u8;
internal static readonly @string altmatchˢ = "altmatch -> "u8;
internal static readonly @string capˢ = "cap "u8;
internal static readonly @string emptyˢ = "empty "u8;
internal static readonly @string matchˢ = "match"u8;
internal static readonly @string failˢ = "fail"u8;
internal static readonly @string nopˢ = "nop -> "u8;
internal static readonly @string runeNilˢ = "rune <nil>"u8;
internal static readonly @string runeˢ = "rune "u8;
internal static readonly @string rune1ˢ = "rune1 "u8;
internal static readonly @string anyˢ = "any -> "u8;
internal static readonly @string anynotnlˢ = "anynotnl -> "u8;

internal static void dumpInst(ж<strings.Builder> b, ref Inst i) {
    switch (i.Op) {
    case var ᴛ when ᴛ == InstAlt: {
        bw(b, altˢ, u32(i.Out), ", ", u32(i.Arg));
        break;
    }
    case var ᴛ when ᴛ == InstAltMatch: {
        bw(b, altmatchˢ, u32(i.Out), ", ", u32(i.Arg));
        break;
    }
    case var ᴛ when ᴛ == InstCapture: {
        bw(b, capˢ, u32(i.Arg), " -> ", u32(i.Out));
        break;
    }
    case var ᴛ when ᴛ == InstEmptyWidth: {
        bw(b, emptyˢ, u32(i.Arg), " -> ", u32(i.Out));
        break;
    }
    case var ᴛ when ᴛ == InstMatch: {
        bw(b, matchˢ);
        break;
    }
    case var ᴛ when ᴛ == InstFail: {
        bw(b, failˢ);
        break;
    }
    case var ᴛ when ᴛ == InstNop: {
        bw(b, nopˢ, u32(i.Out));
        break;
    }
    case var ᴛ when ᴛ == InstRune: {
        if (i.Rune == nil) {
            // shouldn't happen
            bw(b, runeNilˢ);
        }
        bw(b, runeˢ, strconv.QuoteToASCII((@string)i.Rune));
        if ((Flags)(((Flags)(uint16)i.Arg) & FoldCase) != 0) {
            bw(b, "/i"u8);
        }
        bw(b, " -> "u8, u32(i.Out));
        break;
    }
    case var ᴛ when ᴛ == InstRune1: {
        bw(b, rune1ˢ, strconv.QuoteToASCII((@string)i.Rune), " -> ", u32(i.Out));
        break;
    }
    case var ᴛ when ᴛ == InstRuneAny: {
        bw(b, anyˢ, u32(i.Out));
        break;
    }
    case var ᴛ when ᴛ == InstRuneAnyNotNL: {
        bw(b, anynotnlˢ, u32(i.Out));
        break;
    }}
}

} // end syntax_package
