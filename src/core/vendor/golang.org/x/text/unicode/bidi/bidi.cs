// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:generate go run gen.go gen_trieval.go gen_ranges.go

// Package bidi contains functionality for bidirectional text support.
//
// See https://www.unicode.org/reports/tr9.
//
// NOTE: UNDER CONSTRUCTION. This API may change in backwards incompatible ways
// and without notice.
namespace go.vendor.golang.org.x.text.unicode;

// import "golang.org/x/text/unicode/bidi"
// TODO
// - Transformer for reordering?
// - Transformer (validator, really) for Bidi Rule.
using bytes = bytes_package;

partial class bidi_package {

[GoType("num:nint")] partial struct ΔDirection;

// This API tries to avoid dealing with embedding levels for now. Under the hood
// these will be computed, but the question is to which extent the user should
// know they exist. We should at some point allow the user to specify an
// embedding hierarchy, though.
public static ΔDirection LeftToRight => /* iota */ 0;
public static ΔDirection RightToLeft => 1;
public static ΔDirection Mixed => 2;
public static ΔDirection Neutral => 3;

[GoType] public partial struct options {
    internal ΔDirection defaultDirection;
}

// type Option is a methodless func type — rendered inline as its base delegate

// ICU allows the user to define embedding levels. This may be used, for example,
// to use hierarchical structure of markup languages to define embeddings.
// The following option may be a way to expose this functionality in this API.
// // LevelFunc sets a function that associates nesting levels with the given text.
// // The levels function will be called with monotonically increasing values for p.
// func LevelFunc(levels func(p int) int) Option {
// 	panic("unimplemented")
// }

// DefaultDirection sets the default direction for a Paragraph. The direction is
// overridden if the text contains directional characters.
public static Action<ж<options>> DefaultDirection(ΔDirection d) {
    return (ж<options> opts) => {
        opts.Value.defaultDirection = d;
    };
}

// A Paragraph holds a single Paragraph for Bidi processing.
[GoType] partial struct Paragraph {
    internal slice<byte> p;
    internal Ordering o;
    internal slice<Action<ж<options>>> opts;
    internal slice<ΔClass> types;
    internal slice<bracketType> pairTypes;
    internal slice<rune> pairValues;
    internal slice<rune> runes;
    internal options options;
}

// Initialize the p.pairTypes, p.pairValues and p.types from the input previously
// set by p.SetBytes() or p.SetString(). Also limit the input up to (and including) a paragraph
// separator (bidi class B).
//
// The function p.Order() needs these values to be set, so this preparation could be postponed.
// But since the SetBytes and SetStrings functions return the length of the input up to the paragraph
// separator, the whole input needs to be processed anyway and should not be done twice.
//
// The function has the same return values as SetBytes() / SetString()
[GoRecv] internal static (nint n, error err) prepareInput(this ref Paragraph p) {
    p.runes = bytes.Runes(p.p);
    nint bytecount = 0;
    // clear slices from previous SetString or SetBytes
    p.pairTypes = default!;
    p.pairValues = default!;
    p.types = default!;
    foreach (var (_, r) in p.runes) {
        var (props, i) = LookupRune(r);
        bytecount += i;
        ΔClass cls = props.Class();
        if (cls == B) {
            return (bytecount, default!);
        }
        p.types = append(p.types, cls);
        if (props.IsOpeningBracket()){
            p.pairTypes = append(p.pairTypes, bpOpen);
            p.pairValues = append(p.pairValues, r);
        } else 
        if (props.IsBracket()){
            // this must be a closing bracket,
            // since IsOpeningBracket is not true
            p.pairTypes = append(p.pairTypes, bpClose);
            p.pairValues = append(p.pairValues, r);
        } else {
            p.pairTypes = append(p.pairTypes, bpNone);
            p.pairValues = append(p.pairValues, (rune)(0));
        }
    }
    return (bytecount, default!);
}

// SetBytes configures p for the given paragraph text. It replaces text
// previously set by SetBytes or SetString. If b contains a paragraph separator
// it will only process the first paragraph and report the number of bytes
// consumed from b including this separator. Error may be non-nil if options are
// given.
[GoRecv] public static (nint n, error err) SetBytes(this ref Paragraph p, slice<byte> b, params Span<Action<ж<options>>> optsʗp) {
    var opts = optsʗp.slice();

    p.p = b;
    p.opts = opts;
    return p.prepareInput();
}

// SetString configures s for the given paragraph text. It replaces text
// previously set by SetBytes or SetString. If s contains a paragraph separator
// it will only process the first paragraph and report the number of bytes
// consumed from s including this separator. Error may be non-nil if options are
// given.
[GoRecv] public static (nint n, error err) SetString(this ref Paragraph p, @string s, params Span<Action<ж<options>>> optsʗp) {
    var opts = optsʗp.slice();

    p.p = slice<byte>(s);
    p.opts = opts;
    return p.prepareInput();
}

// IsLeftToRight reports whether the principle direction of rendering for this
// paragraphs is left-to-right. If this returns false, the principle direction
// of rendering is right-to-left.
[GoRecv] public static bool IsLeftToRight(this ref Paragraph p) {
    return p.Direction() == LeftToRight;
}

// Direction returns the direction of the text of this paragraph.
//
// The direction may be LeftToRight, RightToLeft, Mixed, or Neutral.
[GoRecv] public static ΔDirection Direction(this ref Paragraph p) {
    return p.o.Direction();
}

// TODO: what happens if the position is > len(input)? This should return an error.

// RunAt reports the Run at the given position of the input text.
//
// This method can be used for computing line breaks on paragraphs.
[GoRecv] public static ΔRun RunAt(this ref Paragraph p, nint pos) {
    nint c = 0;
    nint runNumber = 0;
    foreach (var (i, r) in p.o.runes) {
        c += len(r);
        if (pos < c) {
            runNumber = i;
        }
    }
    return p.o.Run(runNumber);
}

internal static Ordering calculateOrdering(slice<level> levels, slice<rune> runes) {
    ΔDirection curDir = default!;
    ΔDirection prevDir = Neutral;
    nint prevI = 0;
    var o = new Ordering(nil);
    // lvl = 0,2,4,...: left to right
    // lvl = 1,3,5,...: right to left
    foreach (var (i, lvl) in levels) {
        if ((level)(lvl % 2) == 0){
            curDir = LeftToRight;
        } else {
            curDir = RightToLeft;
        }
        if (curDir != prevDir) {
            if (i > 0) {
                o.runes = append(o.runes, runes.slice(prevI, i));
                o.directions = append(o.directions, prevDir);
                o.startpos = append(o.startpos, prevI);
            }
            prevI = i;
            prevDir = curDir;
        }
    }
    o.runes = append(o.runes, runes.slice(prevI));
    o.directions = append(o.directions, prevDir);
    o.startpos = append(o.startpos, prevI);
    return o;
}

// Order computes the visual ordering of all the runs in a Paragraph.
public static (Ordering, error) Order(this ж<Paragraph> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (len(p.types) == 0) {
        return (new Ordering(nil), default!);
    }
    foreach (var (_, fn) in p.opts) {
        fn(Ꮡp.of(Paragraph.Ꮡoptions));
    }
    var lvl = ((level)(-1));
    if (p.options.defaultDirection == RightToLeft) {
        lvl = 1;
    }
    var (para, err) = newParagraph(p.types, p.pairTypes, p.pairValues, lvl);
    if (err != default!) {
        return (new Ordering(nil), err);
    }
    var levels = para.getLevels(new nint[]{len(p.types)}.slice());
    p.o = calculateOrdering(levels, p.runes);
    return (p.o, default!);
}

// Line computes the visual ordering of runs for a single line starting and
// ending at the given positions in the original text.
[GoRecv] public static (Ordering, error) Line(this ref Paragraph p, nint start, nint end) {
    var lineTypes = p.types.slice(start, end);
    var (para, err) = newParagraph(lineTypes, p.pairTypes.slice(start, end), p.pairValues.slice(start, end), (level)(-1));
    if (err != default!) {
        return (new Ordering(nil), err);
    }
    var levels = para.getLevels(new nint[]{len(lineTypes)}.slice());
    var o = calculateOrdering(levels, p.runes.slice(start, end));
    return (o, default!);
}

// An Ordering holds the computed visual order of runs of a Paragraph. Calling
// SetBytes or SetString on the originating Paragraph invalidates an Ordering.
// The methods of an Ordering should only be called by one goroutine at a time.
[GoType] partial struct Ordering {
    internal slice<slice<rune>> runes;
    internal slice<ΔDirection> directions;
    internal slice<nint> startpos;
}

// Direction reports the directionality of the runs.
//
// The direction may be LeftToRight, RightToLeft, Mixed, or Neutral.
[GoRecv] public static ΔDirection Direction(this ref Ordering o) {
    return o.directions[0];
}

// NumRuns returns the number of runs.
[GoRecv] public static nint NumRuns(this ref Ordering o) {
    return len(o.runes);
}

// Run returns the ith run within the ordering.
[GoRecv] public static ΔRun Run(this ref Ordering o, nint i) {
    var r = new ΔRun(
        runes: o.runes[i],
        direction: o.directions[i],
        startpos: o.startpos[i]
    );
    return r;
}

// TODO: perhaps with options.
// // Reorder creates a reader that reads the runes in visual order per character.
// // Modifiers remain after the runes they modify.
// func (l *Runs) Reorder() io.Reader {
// 	panic("unimplemented")
// }

// A Run is a continuous sequence of characters of a single direction.
[GoType] partial struct ΔRun {
    internal slice<rune> runes;
    internal ΔDirection direction;
    internal nint startpos;
}

// String returns the text of the run in its original order.
[GoRecv] public static @string String(this ref ΔRun r) {
    return ((@string)r.runes);
}

// Bytes returns the text of the run in its original order.
[GoRecv] public static slice<byte> Bytes(this ref ΔRun r) {
    return slice<byte>(r.String());
}

// TODO: methods for
// - Display order
// - headers and footers
// - bracket replacement.

// Direction reports the direction of the run.
[GoRecv] public static ΔDirection Direction(this ref ΔRun r) {
    return r.direction;
}

// Pos returns the position of the Run within the text passed to SetBytes or SetString of the
// originating Paragraph value.
[GoRecv] public static (nint start, nint end) Pos(this ref ΔRun r) {
    return (r.startpos, r.startpos + len(r.runes) - 1);
}

// AppendReverse reverses the order of characters of in, appends them to out,
// and returns the result. Modifiers will still follow the runes they modify.
// Brackets are replaced with their counterparts.
public static slice<byte> AppendReverse(slice<byte> @out, slice<byte> @in) {
    var ret = new slice<byte>(len(@in) + len(@out));
    copy(ret, @out);
    var inRunes = bytes.Runes(@in);
    foreach (var (i, r) in inRunes) {
        var (prop, _) = LookupRune(r);
        if (prop.IsBracket()) {
            inRunes[i] = prop.reverseBracket(r);
        }
    }
    for ((nint i, nint j) = (0, len(inRunes) - 1); i < j; (i, j) = (i + 1, j - 1)) {
        (inRunes[i], inRunes[j]) = (inRunes[j], inRunes[i]);
    }
    copy(ret.slice(len(@out)), ((@string)inRunes));
    return ret;
}

// ReverseString reverses the order of characters in s and returns a new string.
// Modifiers will still follow the runes they modify. Brackets are replaced with
// their counterparts.
public static @string ReverseString(@string s) {
    var input = slice<rune>(s);
    nint li = len(input);
    var ret = new slice<rune>(li);
    foreach (var (i, r) in input) {
        var (prop, _) = LookupRune(r);
        if (prop.IsBracket()){
            ret[li - i - 1] = prop.reverseBracket(r);
        } else {
            ret[li - i - 1] = r;
        }
    }
    return ((@string)ret);
}

} // end bidi_package
