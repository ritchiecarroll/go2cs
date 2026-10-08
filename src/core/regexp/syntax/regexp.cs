// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.regexp;

// Note to implementers:
// In this package, re is always a *Regexp and r is always a rune.
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;

partial class syntax_package {

// A Regexp is a node in a regular expression syntax tree.
partial struct Regexp {
    public Op Op; // operator
    public Flags Flags;
    public slice<ж<Regexp>> Sub; // subexpressions, if any
    public array<ж<Regexp>> Sub0 = new(1); // storage for short Sub
    public slice<rune> Rune; // matched runes, for OpLiteral, OpCharClass
    public array<rune> Rune0 = new(2); // storage for short Rune
    public nint Min, Max;       // min, max for OpRepeat
    public nint Cap;       // capturing index, for OpCapture
    public @string Name;    // capturing name, for OpCapture
}

partial struct Op /*num:uint8*/;

//go:generate stringer -type Op -trimprefix Op
// Operators are listed in precedence order, tightest binding to weakest.
// Character class operators are listed simplest to most complex
// (OpLiteral, OpCharClass, OpAnyCharNotNL, OpAnyChar).
public static Op OpNoMatch => /* 1 + iota */ 1;             // matches no strings
public static Op OpEmptyMatch => 2;          // matches empty string
public static Op OpLiteral => 3;             // matches Runes sequence
public static Op OpCharClass => 4;           // matches Runes interpreted as range pair list
public static Op OpAnyCharNotNL => 5;        // matches any character except newline
public static Op OpAnyChar => 6;             // matches any character
public static Op OpBeginLine => 7;           // matches empty string at beginning of line
public static Op OpEndLine => 8;             // matches empty string at end of line
public static Op OpBeginText => 9;           // matches empty string at beginning of text
public static Op OpEndText => 10;             // matches empty string at end of text
public static Op OpWordBoundary => 11;        // matches word boundary `\b`
public static Op OpNoWordBoundary => 12;      // matches word non-boundary `\B`
public static Op OpCapture => 13;             // capturing subexpression with index Cap, optional name Name
public static Op OpStar => 14;                // matches Sub[0] zero or more times
public static Op OpPlus => 15;                // matches Sub[0] one or more times
public static Op OpQuest => 16;               // matches Sub[0] zero or one times
public static Op OpRepeat => 17;              // matches Sub[0] at least Min times, at most Max (Max == -1 is no limit)
public static Op OpConcat => 18;              // matches concatenation of Subs
public static Op OpAlternate => 19;           // matches alternation of Subs

internal static Op opPseudo => 128; // where pseudo-ops start

// Equal reports whether x and y have identical structure.
public static bool Equal(this ж<Regexp> Ꮡx, ж<Regexp> Ꮡy) {
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    if (Ꮡx == nil || Ꮡy == nil) {
        return Ꮡx == Ꮡy;
    }
    if (x.Op != y.Op) {
        return false;
    }
    var exprᴛ1 = x.Op;
    if (exprᴛ1 == OpEndText) {
        if ((Flags)(x.Flags & WasDollar) != (Flags)(y.Flags & WasDollar)) {
            // The parse flags remember whether this is \z or \Z.
            return false;
        }
    }
    else if (exprᴛ1 == OpLiteral || exprᴛ1 == OpCharClass) {
        return slices.Equal<slice<rune>, rune>(x.Rune, y.Rune);
    }
    else if (exprᴛ1 == OpAlternate || exprᴛ1 == OpConcat) {
        return slices.EqualFunc<slice<ж<Regexp>>, slice<ж<Regexp>>, ж<Regexp>, ж<Regexp>>(x.Sub, y.Sub, ((Func<ж<Regexp>, ж<Regexp>, bool>)(Equal)));
    }
    else if (exprᴛ1 == OpStar || exprᴛ1 == OpPlus || exprᴛ1 == OpQuest) {
        if ((Flags)(x.Flags & NonGreedy) != (Flags)(y.Flags & NonGreedy) || !x.Sub[0].Equal(y.Sub[0])) {
            return false;
        }
    }
    else if (exprᴛ1 == OpRepeat) {
        if ((Flags)(x.Flags & NonGreedy) != (Flags)(y.Flags & NonGreedy) || x.Min != y.Min || x.Max != y.Max || !x.Sub[0].Equal(y.Sub[0])) {
            return false;
        }
    }
    else if (exprᴛ1 == OpCapture) {
        if (x.Cap != y.Cap || x.Name != y.Name || !x.Sub[0].Equal(y.Sub[0])) {
            return false;
        }
    }

    return true;
}

partial struct printFlags /*num:uint8*/;

internal static printFlags flagI => /* 1 << iota */ 1;        // (?i:
internal static printFlags flagM => 2;        // (?m:
internal static printFlags flagS => 4;        // (?s:
internal static printFlags flagOff => 8;      // )
internal static printFlags flagPrec => 16;     // (?: )
internal static UntypedInt negShift => 5;     // flagI<<negShift is (?-i:

// addSpan enables the flags f around start..last,
// by setting flags[start] = f and flags[last] = flagOff.
internal static void addSpan(ж<Regexp> Ꮡstart, ж<Regexp> Ꮡlast, printFlags f, ref map<ж<Regexp>, printFlags> flags) {
    ref var start = ref Ꮡstart.DerefOrNull();
    ref var last = ref Ꮡlast.DerefOrNull();

    if (flags == default!) {
        flags = new map<ж<Regexp>, printFlags>();
    }
    (flags)[Ꮡstart] = f;
    (flags)[Ꮡlast] |= (printFlags)(flagOff); // maybe start==last
}

// calcFlags calculates the flags to print around each subexpression in re,
// storing that information in (*flags)[sub] for each affected subexpression.
// The first time an entry needs to be written to *flags, calcFlags allocates the map.
// calcFlags also calculates the flags that must be active or can't be active
// around re and returns those flags.
internal static (printFlags must, printFlags cant) calcFlags(ref Regexp re, ref map<ж<Regexp>, printFlags> flags) {
    var exprᴛ1 = re.Op;
    if (exprᴛ1 == OpLiteral) {
        foreach (var (_, r) in re.Rune) {
            // If literal is fold-sensitive, return (flagI, 0) or (0, flagI)
            // according to whether (?i) is active.
            // If literal is not fold-sensitive, return 0, 0.
            if (minFold <= r && r <= maxFold && unicode.SimpleFold(r) != r) {
                if ((Flags)(re.Flags & FoldCase) != 0){
                    return (flagI, 0);
                } else {
                    return (0, flagI);
                }
            }
        }
        return (0, 0);
    }
    if (exprᴛ1 == OpCharClass) {
        for (nint i = 0; i < len(re.Rune); i += 2) {
            // If literal is fold-sensitive, return 0, flagI - (?i) has been compiled out.
            // If literal is not fold-sensitive, return 0, 0.
            var lo = max((rune)(minFold), re.Rune[i]);
            var hi = min((rune)(maxFold), re.Rune[i + 1]);
            for (var r = lo; r <= hi; r++) {
                for (var f = unicode.SimpleFold(r); f != r; f = unicode.SimpleFold(f)) {
                    if (!(lo <= f && f <= hi) && !inCharClass(f, re.Rune)) {
                        return (0, flagI);
                    }
                }
            }
        }
        return (0, 0);
    }
    if (exprᴛ1 == OpAnyCharNotNL) {
        return (0, flagS);
    }
    if (exprᴛ1 == OpAnyChar) {
        return (flagS, 0);
    }
    if (exprᴛ1 == OpBeginLine || exprᴛ1 == OpEndLine) {
        return (flagM, 0);
    }
    if (exprᴛ1 == OpEndText) {
        if ((Flags)(re.Flags & WasDollar) != 0) {
            // (?-s).
            // (?s).
            // (?m)^ (?m)$
            // (?-m)$
            return (0, flagM);
        }
        return (0, 0);
    }
    if (exprᴛ1 == OpCapture || exprᴛ1 == OpStar || exprᴛ1 == OpPlus || exprᴛ1 == OpQuest || exprᴛ1 == OpRepeat) {
        return calcFlags(ref (re.Sub[0]).DerefOrNull(), ref flags);
    }
    if (exprᴛ1 == OpConcat || exprᴛ1 == OpAlternate) {
        // Gather the must and cant for each subexpression.
        // When we find a conflicting subexpression, insert the necessary
        // flags around the previously identified span and start over.
        printFlags mustΔ2 = default!;
        printFlags cantΔ2 = default!;
        printFlags allCant = default!;
        nint start = 0;
        nint last = 0;
        var did = false;
        foreach (var (i, sub) in re.Sub) {
            var (subMust, subCant) = calcFlags(ref (sub).DerefOrNull(), ref flags);
            if ((printFlags)(mustΔ2 & subCant) != 0 || (printFlags)(subMust & cantΔ2) != 0) {
                if (mustΔ2 != 0) {
                    addSpan(re.Sub[start], re.Sub[last], mustΔ2, ref flags);
                }
                mustΔ2 = 0;
                cantΔ2 = 0;
                start = i;
                did = true;
            }
            mustΔ2 |= (printFlags)(subMust);
            cantΔ2 |= (printFlags)(subCant);
            allCant |= (printFlags)(subCant);
            if (subMust != 0) {
                last = i;
            }
            if (mustΔ2 == 0 && start == i) {
                start++;
            }
        }
        if (!did) {
            // No conflicts: pass the accumulated must and cant upward.
            return (mustΔ2, cantΔ2);
        }
        if (mustΔ2 != 0) {
            // Conflicts found; need to finish final span.
            addSpan(re.Sub[start], re.Sub[last], mustΔ2, ref flags);
        }
        return (0, allCant);
    }
    { /* default: */
        return (0, 0);
    }

}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x00X10ffffˢ = @"[^\x00-\x{10FFFF}]"u8;
internal static readonly @string invalidCharClassˢ = @"[invalid char class]"u8;
internal static readonly @string x00X10ffffˢ2 = @"^\x00-\x{10FFFF}"u8;

// writeRegexp writes the Perl syntax for the regular expression re to b.
internal static void writeRegexp(ж<strings.Builder> Ꮡb, ж<Regexp> Ꮡre, printFlags f, map<ж<Regexp>, printFlags> flags) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();
        ref var re = ref Ꮡre.DerefOrNull();

        f |= (printFlags)(flags[Ꮡre]);
        if ((printFlags)(f & flagPrec) != 0 && (printFlags)(f & ~((printFlags)(flagOff | flagPrec))) != 0 && (printFlags)(f & flagOff) != 0) {
            // flagPrec is redundant with other flags being added and terminated
            f &= unchecked((printFlags)~(printFlags)(flagPrec));
        }
        if ((printFlags)(f & ~((printFlags)(flagOff | flagPrec))) != 0) {
            Ꮡb.WriteString(@"(?"u8);
            if ((printFlags)(f & flagI) != 0) {
                Ꮡb.WriteString(@"i"u8);
            }
            if ((printFlags)(f & flagM) != 0) {
                Ꮡb.WriteString(@"m"u8);
            }
            if ((printFlags)(f & flagS) != 0) {
                Ꮡb.WriteString(@"s"u8);
            }
            if ((printFlags)(f & ((((printFlags)(flagM | flagS)) << (int)(negShift)))) != 0) {
                Ꮡb.WriteString(@"-"u8);
                if ((printFlags)(f & ((flagM << (int)(negShift)))) != 0) {
                    Ꮡb.WriteString(@"m"u8);
                }
                if ((printFlags)(f & ((flagS << (int)(negShift)))) != 0) {
                    Ꮡb.WriteString(@"s"u8);
                }
            }
            Ꮡb.WriteString(@":"u8);
        }
        if ((printFlags)(f & flagOff) != 0) {
            defer(Ꮡb.WriteString, (@string)@")", ref ᒐ);
        }
        if ((printFlags)(f & flagPrec) != 0) {
            Ꮡb.WriteString(@"(?:"u8);
            defer(Ꮡb.WriteString, (@string)@")", ref ᒐ);
        }
        var exprᴛ1 = re.Op;
        if (exprᴛ1 == OpNoMatch) {
            Ꮡb.WriteString(x00X10ffffˢ);
        }
        else if (exprᴛ1 == OpEmptyMatch) {
            Ꮡb.WriteString(@"(?:)"u8);
        }
        else if (exprᴛ1 == OpLiteral) {
            foreach (var (_, r) in re.Rune) {
                escape(Ꮡb, r, false);
            }
        }
        else if (exprᴛ1 == OpCharClass) {
            do {
                if (len(re.Rune) % 2 != 0) {
                    Ꮡb.WriteString(invalidCharClassˢ);
                    break;
                }
                Ꮡb.WriteRune((rune)'[');
                if (len(re.Rune) == 0){
                    Ꮡb.WriteString(x00X10ffffˢ2);
                } else 
                if (re.Rune[0] == 0 && re.Rune[len(re.Rune) - 1] == unicode.MaxRune && len(re.Rune) > 2){
                    // Contains 0 and MaxRune. Probably a negated class.
                    // Print the gaps.
                    Ꮡb.WriteRune((rune)'^');
                    for (nint i = 1; i < len(re.Rune) - 1; i += 2) {
                        var (lo, hi) = (re.Rune[i] + 1, re.Rune[i + 1] - 1);
                        escape(Ꮡb, lo, lo == (rune)'-');
                        if (lo != hi) {
                            if (hi != lo + 1) {
                                Ꮡb.WriteRune((rune)'-');
                            }
                            escape(Ꮡb, hi, hi == (rune)'-');
                        }
                    }
                } else {
                    for (nint i = 0; i < len(re.Rune); i += 2) {
                        var (lo, hi) = (re.Rune[i], re.Rune[i + 1]);
                        escape(Ꮡb, lo, lo == (rune)'-');
                        if (lo != hi) {
                            if (hi != lo + 1) {
                                Ꮡb.WriteRune((rune)'-');
                            }
                            escape(Ꮡb, hi, hi == (rune)'-');
                        }
                    }
                }
                Ꮡb.WriteRune((rune)']');
            } while (false);
        }
        else if (exprᴛ1 == OpAnyCharNotNL || exprᴛ1 == OpAnyChar) {
            Ꮡb.WriteString(@"."u8);
        }
        else if (exprᴛ1 == OpBeginLine) {
            Ꮡb.WriteString(@"^"u8);
        }
        else if (exprᴛ1 == OpEndLine) {
            Ꮡb.WriteString(@"$"u8);
        }
        else if (exprᴛ1 == OpBeginText) {
            Ꮡb.WriteString(@"\A"u8);
        }
        else if (exprᴛ1 == OpEndText) {
            if ((Flags)(re.Flags & WasDollar) != 0){
                Ꮡb.WriteString(@"$"u8);
            } else {
                Ꮡb.WriteString(@"\z"u8);
            }
        }
        else if (exprᴛ1 == OpWordBoundary) {
            Ꮡb.WriteString(@"\b"u8);
        }
        else if (exprᴛ1 == OpNoWordBoundary) {
            Ꮡb.WriteString(@"\B"u8);
        }
        else if (exprᴛ1 == OpCapture) {
            if (re.Name != ""u8){
                Ꮡb.WriteString(@"(?P<"u8);
                Ꮡb.WriteString(re.Name);
                Ꮡb.WriteRune((rune)'>');
            } else {
                Ꮡb.WriteRune((rune)'(');
            }
            if ((~re.Sub[0]).Op != OpEmptyMatch) {
                writeRegexp(Ꮡb, re.Sub[0], flags[re.Sub[0]], flags);
            }
            Ꮡb.WriteRune((rune)')');
        }
        else if (exprᴛ1 == OpStar || exprᴛ1 == OpPlus || exprᴛ1 == OpQuest || exprᴛ1 == OpRepeat) {
            var p = ((printFlags)0);
            var sub = re.Sub[0];
            if ((~sub).Op > OpCapture || (~sub).Op == OpLiteral && len((~sub).Rune) > 1) {
                p = flagPrec;
            }
            writeRegexp(Ꮡb, sub, p, flags);
            var exprᴛ2 = re.Op;
            if (exprᴛ2 == OpStar) {
                Ꮡb.WriteRune((rune)'*');
            }
            else if (exprᴛ2 == OpPlus) {
                Ꮡb.WriteRune((rune)'+');
            }
            else if (exprᴛ2 == OpQuest) {
                Ꮡb.WriteRune((rune)'?');
            }
            else if (exprᴛ2 == OpRepeat) {
                Ꮡb.WriteRune((rune)'{');
                Ꮡb.WriteString(strconv.Itoa(re.Min));
                if (re.Max != re.Min) {
                    Ꮡb.WriteRune((rune)',');
                    if (re.Max >= 0) {
                        Ꮡb.WriteString(strconv.Itoa(re.Max));
                    }
                }
                Ꮡb.WriteRune((rune)'}');
            }

            if ((Flags)(re.Flags & NonGreedy) != 0) {
                Ꮡb.WriteRune((rune)'?');
            }
        }
        else if (exprᴛ1 == OpConcat) {
            foreach (var (_, sub) in re.Sub) {
                var p = ((printFlags)0);
                if ((~sub).Op == OpAlternate) {
                    p = flagPrec;
                }
                writeRegexp(Ꮡb, sub, p, flags);
            }
        }
        else if (exprᴛ1 == OpAlternate) {
            foreach (var (i, sub) in re.Sub) {
                if (i > 0) {
                    Ꮡb.WriteRune((rune)'|');
                }
                writeRegexp(Ꮡb, sub, 0, flags);
            }
        }
        else { /* default: */
            Ꮡb.WriteString("<invalid op"u8 + strconv.Itoa((nint)(uint8)re.Op) + ">"u8);
        }

    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static @string String(this ж<Regexp> Ꮡre) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    ref var flags = ref heap<map<ж<Regexp>, printFlags>>(out var Ꮡflags);
    var (must, cant) = calcFlags(ref (Ꮡre).DerefOrNull(), ref flags);
    must |= (printFlags)((printFlags)(uint8)(((printFlags)(cant & ~flagI)) << (int)(negShift)));
    if (must != 0) {
        must |= (printFlags)(flagOff);
    }
    writeRegexp(Ꮡb, Ꮡre, must, flags);
    return b.String();
}

internal static readonly @string meta = @"\.+*?()|[]{}^$"u8;

internal static void escape(ж<strings.Builder> Ꮡb, rune r, bool force) {
    if (unicode.IsPrint(r)) {
        if (strings.ContainsRune(meta, r) || force) {
            Ꮡb.WriteRune((rune)'\\');
        }
        Ꮡb.WriteRune(r);
        return;
    }
    switch (r) {
    case (rune)'\a': {
        Ꮡb.WriteString(@"\a"u8);
        break;
    }
    case (rune)'\f': {
        Ꮡb.WriteString(@"\f"u8);
        break;
    }
    case (rune)'\n': {
        Ꮡb.WriteString(@"\n"u8);
        break;
    }
    case (rune)'\r': {
        Ꮡb.WriteString(@"\r"u8);
        break;
    }
    case (rune)'\t': {
        Ꮡb.WriteString(@"\t"u8);
        break;
    }
    case (rune)'\v': {
        Ꮡb.WriteString(@"\v"u8);
        break;
    }
    default: {
        if (r < 0x100) {
            Ꮡb.WriteString(@"\x"u8);
            @string s = strconv.FormatInt((int64)r, 16);
            if (len(s) == 1) {
                Ꮡb.WriteRune((rune)'0');
            }
            Ꮡb.WriteString(s);
            break;
        }
        Ꮡb.WriteString(@"\x{"u8);
        Ꮡb.WriteString(strconv.FormatInt((int64)r, 16));
        Ꮡb.WriteString(@"}"u8);
        break;
    }}

}

// MaxCap walks the regexp to find the maximum capture index.
public static nint MaxCap(this ref Regexp re) {
    nint m = 0;
    if (re.Op == OpCapture) {
        m = re.Cap;
    }
    foreach (var (_, sub) in re.Sub) {
        {
            nint n = sub.MaxCap(); if (m < n) {
                m = n;
            }
        }
    }
    return m;
}

// CapNames walks the regexp to find the names of capturing groups.
public static slice<@string> CapNames(this ref Regexp re) {
    var names = new slice<@string>(re.MaxCap() + 1);
    re.capNames(names);
    return names;
}

internal static void capNames(this ref Regexp re, slice<@string> names) {
    if (re.Op == OpCapture) {
        names[re.Cap] = re.Name;
    }
    foreach (var (_, sub) in re.Sub) {
        sub.capNames(names);
    }
}

} // end syntax_package
