// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package bidirule implements the Bidi Rule defined by RFC 5893.
//
// This package is under development. The API may change without notice and
// without preserving backward compatibility.
namespace go.vendor.golang.org.x.text.secure;

using errors = errors_package;
using utf8 = go.unicode.utf8_package;
using transform = go.vendor.golang.org.x.text.transform_package;
using bidi = go.vendor.golang.org.x.text.unicode.bidi_package;
using go.unicode;
using go.vendor.golang.org.x.text;
using go.vendor.golang.org.x.text.unicode;

partial class bidirule_package {

// This file contains an implementation of RFC 5893: Right-to-Left Scripts for
// Internationalized Domain Names for Applications (IDNA)
//
// A label is an individual component of a domain name.  Labels are usually
// shown separated by dots; for example, the domain name "www.example.com" is
// composed of three labels: "www", "example", and "com".
//
// An RTL label is a label that contains at least one character of class R, AL,
// or AN. An LTR label is any label that is not an RTL label.
//
// A "Bidi domain name" is a domain name that contains at least one RTL label.
//
//  The following guarantees can be made based on the above:
//
//  o  In a domain name consisting of only labels that satisfy the rule,
//     the requirements of Section 3 are satisfied.  Note that even LTR
//     labels and pure ASCII labels have to be tested.
//
//  o  In a domain name consisting of only LDH labels (as defined in the
//     Definitions document [RFC5890]) and labels that satisfy the rule,
//     the requirements of Section 3 are satisfied as long as a label
//     that starts with an ASCII digit does not come after a
//     right-to-left label.
//
//  No guarantee is given for other combinations.

// ErrInvalid indicates a label is invalid according to the Bidi Rule.
public static error ErrInvalid = errors.New("bidirule: failed Bidi Rule"u8);

partial struct ruleState /*num:uint8*/;

internal static ruleState ruleInitial => /* iota */ 0;
internal static ruleState ruleLTR => 1;
internal static ruleState ruleLTRFinal => 2;
internal static ruleState ruleRTL => 3;
internal static ruleState ruleRTLFinal => 4;
internal static ruleState ruleInvalid => 5;

partial struct ruleTransition {
    internal ruleState next;
    internal uint16 mask;
}

// [2.1] The first character must be a character with Bidi property L, R, or
// AL. If it has the R or AL property, it is an RTL label; if it has the L
// property, it is an LTR label.
// [2.3] In an RTL label, the end of the label must be a character with
// Bidi property R, AL, EN, or AN, followed by zero or more characters
// with Bidi property NSM.
// [2.2] In an RTL label, only characters with the Bidi properties R,
// AL, AN, EN, ES, CS, ET, ON, BN, or NSM are allowed.
// We exclude the entries from [2.3]
// [2.3] In an RTL label, the end of the label must be a character with
// Bidi property R, AL, EN, or AN, followed by zero or more characters
// with Bidi property NSM.
// [2.2] In an RTL label, only characters with the Bidi properties R,
// AL, AN, EN, ES, CS, ET, ON, BN, or NSM are allowed.
// We exclude the entries from [2.3] and NSM.
// [2.6] In an LTR label, the end of the label must be a character with
// Bidi property L or EN, followed by zero or more characters with Bidi
// property NSM.
// [2.5] In an LTR label, only characters with the Bidi properties L,
// EN, ES, CS, ET, ON, BN, or NSM are allowed.
// We exclude the entries from [2.6].
// [2.6] In an LTR label, the end of the label must be a character with
// Bidi property L or EN, followed by zero or more characters with Bidi
// property NSM.
// [2.5] In an LTR label, only characters with the Bidi properties L,
// EN, ES, CS, ET, ON, BN, or NSM are allowed.
// We exclude the entries from [2.6].
internal static array<array<ruleTransition>> transitions = new golib.SparseArray<array<ruleTransition>>{
    [ruleInitial] = new ruleTransition[]{
        new(ruleLTRFinal, (uint16)(1 << (int)(nuint)(bidi.L))),
        new(ruleRTLFinal, (uint16)((1 << (int)(nuint)(bidi.R)) | (1 << (int)(nuint)(bidi.AL))))}.array(),
    [ruleRTL] = new ruleTransition[]{
        new(ruleRTLFinal, (uint16)((UntypedInt)((UntypedInt)((1 << (int)(nuint)(bidi.R)) | (1 << (int)(nuint)(bidi.AL))) | (1 << (int)(nuint)(bidi.EN))) | (1 << (int)(nuint)(bidi.AN)))),
        new(ruleRTL, (uint16)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)((1 << (int)(nuint)(bidi.ES)) | (1 << (int)(nuint)(bidi.CS))) | (1 << (int)(nuint)(bidi.ET))) | (1 << (int)(nuint)(bidi.ON))) | (1 << (int)(nuint)(bidi.BN))) | (1 << (int)(nuint)(bidi.NSM))))}.array(),
    [ruleRTLFinal] = new ruleTransition[]{
        new(ruleRTLFinal, (uint16)((UntypedInt)((UntypedInt)((UntypedInt)((1 << (int)(nuint)(bidi.R)) | (1 << (int)(nuint)(bidi.AL))) | (1 << (int)(nuint)(bidi.EN))) | (1 << (int)(nuint)(bidi.AN))) | (1 << (int)(nuint)(bidi.NSM)))),
        new(ruleRTL, (uint16)((UntypedInt)((UntypedInt)((UntypedInt)((1 << (int)(nuint)(bidi.ES)) | (1 << (int)(nuint)(bidi.CS))) | (1 << (int)(nuint)(bidi.ET))) | (1 << (int)(nuint)(bidi.ON))) | (1 << (int)(nuint)(bidi.BN))))}.array(),
    [ruleLTR] = new ruleTransition[]{
        new(ruleLTRFinal, (uint16)((1 << (int)(nuint)(bidi.L)) | (1 << (int)(nuint)(bidi.EN)))),
        new(ruleLTR, (uint16)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)((1 << (int)(nuint)(bidi.ES)) | (1 << (int)(nuint)(bidi.CS))) | (1 << (int)(nuint)(bidi.ET))) | (1 << (int)(nuint)(bidi.ON))) | (1 << (int)(nuint)(bidi.BN))) | (1 << (int)(nuint)(bidi.NSM))))}.array(),
    [ruleLTRFinal] = new ruleTransition[]{
        new(ruleLTRFinal, (uint16)((UntypedInt)((1 << (int)(nuint)(bidi.L)) | (1 << (int)(nuint)(bidi.EN))) | (1 << (int)(nuint)(bidi.NSM)))),
        new(ruleLTR, (uint16)((UntypedInt)((UntypedInt)((UntypedInt)((1 << (int)(nuint)(bidi.ES)) | (1 << (int)(nuint)(bidi.CS))) | (1 << (int)(nuint)(bidi.ET))) | (1 << (int)(nuint)(bidi.ON))) | (1 << (int)(nuint)(bidi.BN))))}.array(),
    [ruleInvalid] = new ruleTransition[]{
        new(ruleInvalid, 0),
        new(ruleInvalid, 0)}.array()
}.array();

// [2.4] In an RTL label, if an EN is present, no AN may be present, and
// vice versa.
internal const uint16 exclusiveRTL = /* uint16(1<<bidi.EN | 1<<bidi.AN) */ 36;

// From RFC 5893
// An RTL label is a label that contains at least one character of type
// R, AL, or AN.
//
// An LTR label is any label that is not an RTL label.

// Direction reports the direction of the given label as defined by RFC 5893.
// The Bidi Rule does not have to be applied to labels of the category
// LeftToRight.
public static bidiꓸDirection Direction(slice<byte> b) {
    for (nint i = 0; i < len(b); ) {
        var (e, sz) = bidi.Lookup(b.slice(i));
        if (sz == 0) {
            i++;
        }
        bidiꓸClass c = e.Class();
        if (c == bidi.R || c == bidi.AL || c == bidi.AN) {
            return bidi.RightToLeft;
        }
        i += sz;
    }
    return bidi.LeftToRight;
}

// DirectionString reports the direction of the given label as defined by RFC
// 5893. The Bidi Rule does not have to be applied to labels of the category
// LeftToRight.
public static bidiꓸDirection DirectionString(@string s) {
    for (nint i = 0; i < len(s); ) {
        var (e, sz) = bidi.LookupString(s.slice(i));
        if (sz == 0) {
            i++;
            continue;
        }
        bidiꓸClass c = e.Class();
        if (c == bidi.R || c == bidi.AL || c == bidi.AN) {
            return bidi.RightToLeft;
        }
        i += sz;
    }
    return bidi.LeftToRight;
}

// Valid reports whether b conforms to the BiDi rule.
public static bool Valid(slice<byte> b) {
    Transformer t = default!;
    {
        var (n, ok) = t.advance(b); if (!ok || n < len(b)) {
            return false;
        }
    }
    return t.isFinal();
}

// ValidString reports whether s conforms to the BiDi rule.
public static bool ValidString(@string s) {
    Transformer t = default!;
    {
        var (n, ok) = t.advanceString(s); if (!ok || n < len(s)) {
            return false;
        }
    }
    return t.isFinal();
}

// New returns a Transformer that verifies that input adheres to the Bidi Rule.
public static ж<Transformer> New() {
    return Ꮡ(new Transformer(nil));
}

// Transformer implements transform.Transform.
partial struct Transformer {
    internal ruleState state;
    internal bool hasRTL;
    internal uint16 seen;
}

// A rule can only be violated for "Bidi Domain names", meaning if one of the
// following categories has been observed.
internal static bool isRTL(this ref Transformer t) {
    const uint16 isRTL = /* 1<<bidi.R | 1<<bidi.AL | 1<<bidi.AN */ 8226;
    return (uint16)(t.seen & isRTL) != 0;
}

// Reset implements transform.Transformer.
public static void Reset(this ref Transformer t) {
    t = new Transformer(nil);
}

// Transform implements transform.Transformer. This Transformer has state and
// needs to be reset between uses.
public static (nint nDst, nint nSrc, error err) Transform(this ref Transformer t, slice<byte> dst, slice<byte> src, bool atEOF) {
    error err = default!;

    if (len(dst) < len(src)) {
        src = src.slice(0, len(dst));
        atEOF = false;
        err = transform.ErrShortDst;
    }
    var (n, err1) = t.Span(src, atEOF);
    copy(dst, src.slice(0, n));
    if (err == default! || err1 != default! && !AreEqual(err1, transform.ErrShortSrc)) {
        err = err1;
    }
    return (n, n, err);
}

// Span returns the first n bytes of src that conform to the Bidi rule.
public static (nint n, error err) Span(this ref Transformer t, slice<byte> src, bool atEOF) {
    nint n = default!;
    error err = default!;

    if (t.state == ruleInvalid && t.isRTL()) {
        return (0, ErrInvalid);
    }
    (n, var ok) = t.advance(src);
    switch (ᐧ) {
    case {} when !ok: {
        err = ErrInvalid;
        break;
    }
    case {} when n < len(src): {
        if (!atEOF) {
            err = transform.ErrShortSrc;
            break;
        }
        err = ErrInvalid;
        break;
    }
    case {} when !t.isFinal(): {
        err = ErrInvalid;
        break;
    }}

    return (n, err);
}

// Precomputing the ASCII values decreases running time for the ASCII fast path
// by about 30%.
internal static array<bidi.Properties> asciiTable = new(128);

[GoInit] internal static void init() {
    foreach (var (i, _) in asciiTable) {
        var (p, _) = bidi.LookupRune((rune)i);
        asciiTable[i] = p;
    }
}

internal static (nint n, bool ok) advance(this ref Transformer t, slice<byte> s) {
    nint n = default!;

    bidi.Properties e = default!;
    nint sz = default!;
    while (n < len(s)) {
        if (s[n] < utf8.RuneSelf){
            (e, sz) = (asciiTable[s[n]], 1);
        } else {
            (e, sz) = bidi.Lookup(s.slice(n));
            if (sz <= 1) {
                if (sz == 1) {
                    // We always consider invalid UTF-8 to be invalid, even if
                    // the string has not yet been determined to be RTL.
                    // TODO: is this correct?
                    return (n, false);
                }
                return (n, true); // incomplete UTF-8 encoding
            }
        }
        // TODO: using CompactClass would result in noticeable speedup.
        // See unicode/bidi/prop.go:Properties.CompactClass.
        var c = (uint16)(((uint16)1).Lsh(e.Class()));
        t.seen |= (uint16)(c);
        if ((uint16)(t.seen & exclusiveRTL) == exclusiveRTL) {
            t.state = ruleInvalid;
            return (n, false);
        }
        {
            var tr = transitions[t.state].Clone();
            switch (ᐧ) {
            case {} when (uint16)(tr[0].mask & c) != 0: {
                t.state = tr[0].next;
                break;
            }
            case {} when (uint16)(tr[1].mask & c) != 0: {
                t.state = tr[1].next;
                break;
            }
            default: {
                t.state = ruleInvalid;
                if (t.isRTL()) {
                    return (n, false);
                }
                break;
            }}
        }

        n += sz;
    }
    return (n, true);
}

internal static (nint n, bool ok) advanceString(this ref Transformer t, @string s) {
    nint n = default!;

    bidi.Properties e = default!;
    nint sz = default!;
    while (n < len(s)) {
        if (s[n] < utf8.RuneSelf){
            (e, sz) = (asciiTable[s[n]], 1);
        } else {
            (e, sz) = bidi.LookupString(s.slice(n));
            if (sz <= 1) {
                if (sz == 1) {
                    return (n, false); // invalid UTF-8
                }
                return (n, true); // incomplete UTF-8 encoding
            }
        }
        // TODO: using CompactClass results in noticeable speedup.
        // See unicode/bidi/prop.go:Properties.CompactClass.
        var c = (uint16)(((uint16)1).Lsh(e.Class()));
        t.seen |= (uint16)(c);
        if ((uint16)(t.seen & exclusiveRTL) == exclusiveRTL) {
            t.state = ruleInvalid;
            return (n, false);
        }
        {
            var tr = transitions[t.state].Clone();
            switch (ᐧ) {
            case {} when (uint16)(tr[0].mask & c) != 0: {
                t.state = tr[0].next;
                break;
            }
            case {} when (uint16)(tr[1].mask & c) != 0: {
                t.state = tr[1].next;
                break;
            }
            default: {
                t.state = ruleInvalid;
                if (t.isRTL()) {
                    return (n, false);
                }
                break;
            }}
        }

        n += sz;
    }
    return (n, true);
}

} // end bidirule_package
