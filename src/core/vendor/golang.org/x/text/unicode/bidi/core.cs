// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.text.unicode;

using fmt = fmt_package;
using log = log_package;
using ꓸꓸꓸClass = Span<bidi_package.ΔClass>;

partial class bidi_package {

[GoType("num:int8")] partial struct level;

// This implementation is a port based on the reference implementation found at:
// https://www.unicode.org/Public/PROGRAMS/BidiReferenceJava/
//
// described in Unicode Bidirectional Algorithm (UAX #9).
//
// Input:
// There are two levels of input to the algorithm, since clients may prefer to
// supply some information from out-of-band sources rather than relying on the
// default behavior.
//
// - Bidi class array
// - Bidi class array, with externally supplied base line direction
//
// Output:
// Output is separated into several stages:
//
//  - levels array over entire paragraph
//  - reordering array over entire paragraph
//  - levels array over line
//  - reordering array over line
//
// Note that for conformance to the Unicode Bidirectional Algorithm,
// implementations are only required to generate correct reordering and
// character directionality (odd or even levels) over a line. Generating
// identical level arrays over a line is not required. Bidi explicit format
// codes (LRE, RLE, LRO, RLO, PDF) and BN can be assigned arbitrary levels and
// positions as long as the rest of the input is properly reordered.
//
// As the algorithm is defined to operate on a single paragraph at a time, this
// implementation is written to handle single paragraphs. Thus rule P1 is
// presumed by this implementation-- the data provided to the implementation is
// assumed to be a single paragraph, and either contains no 'B' codes, or a
// single 'B' code at the end of the input. 'B' is allowed as input to
// illustrate how the algorithm assigns it a level.
//
// Also note that rules L3 and L4 depend on the rendering engine that uses the
// result of the bidi algorithm. This implementation assumes that the rendering
// engine expects combining marks in visual order (e.g. to the left of their
// base character in RTL runs) and that it adjusts the glyphs used to render
// mirrored characters that are in RTL runs so that they render appropriately.
internal static level implicitLevel => -1;

// in returns if x is equal to any of the values in set.
internal static bool @in(this ΔClass c, params ꓸꓸꓸClass setʗp) {
    var set = setʗp.sslice();

    foreach (var (_, s) in set) {
        if (c == s) {
            return true;
        }
    }
    return false;
}

// A paragraph contains the state of a paragraph.
[GoType] partial struct paragraph {
    internal slice<ΔClass> initialTypes;
    // Arrays of properties needed for paired bracket evaluation in N0
    internal slice<bracketType> pairTypes; // paired Bracket types for paragraph
    internal slice<rune> pairValues;   // rune for opening bracket or pbOpen and pbClose; 0 for pbNone
    internal level embeddingLevel; // default: = implicitLevel;
    // at the paragraph levels
    internal slice<ΔClass> resultTypes;
    internal slice<level> resultLevels;
    // Index of matching PDI for isolate initiator characters. For other
    // characters, the value of matchingPDI will be set to -1. For isolate
    // initiators with no matching PDI, matchingPDI will be set to the length of
    // the input string.
    internal slice<nint> matchingPDI;
    // Index of matching isolate initiator for PDI characters. For other
    // characters, and for PDIs with no matching isolate initiator, the value of
    // matchingIsolateInitiator will be set to -1.
    internal slice<nint> matchingIsolateInitiator;
}

// newParagraph initializes a paragraph. The user needs to supply a few arrays
// corresponding to the preprocessed text input. The types correspond to the
// Unicode BiDi classes for each rune. pairTypes indicates the bracket type for
// each rune. pairValues provides a unique bracket class identifier for each
// rune (suggested is the rune of the open bracket for opening and matching
// close brackets, after normalization). The embedding levels are optional, but
// may be supplied to encode embedding levels of styled text.
internal static (ж<paragraph>, error) newParagraph(slice<ΔClass> types, slice<bracketType> pairTypes, slice<rune> pairValues, level levels) {
    error err = default!;
    {
        err = validateTypes(types); if (err != default!) {
            return (default!, err);
        }
    }
    {
        err = validatePbTypes(pairTypes); if (err != default!) {
            return (default!, err);
        }
    }
    {
        err = validatePbValues(pairValues, pairTypes); if (err != default!) {
            return (default!, err);
        }
    }
    {
        err = validateParagraphEmbeddingLevel(levels); if (err != default!) {
            return (default!, err);
        }
    }
    var p = Ꮡ(new paragraph(
        initialTypes: appendꓸꓸꓸ(slice<ΔClass>(default!), types),
        embeddingLevel: levels,
        pairTypes: pairTypes,
        pairValues: pairValues,
        resultTypes: appendꓸꓸꓸ(slice<ΔClass>(default!), types)
    ));
    p.run();
    return (p, default!);
}

[GoRecv] internal static nint Len(this ref paragraph p) {
    return len(p.initialTypes);
}

// The algorithm. Does not include line-based processing (Rules L1, L2).
// These are applied later in the line-based phase of the algorithm.
internal static void run(this ж<paragraph> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.determineMatchingIsolates();
    // 1) determining the paragraph level
    // Rule P1 is the requirement for entering this algorithm.
    // Rules P2, P3.
    // If no externally supplied paragraph embedding level, use default.
    if (p.embeddingLevel == implicitLevel) {
        p.embeddingLevel = p.determineParagraphEmbeddingLevel(0, p.Len());
    }
    // Initialize result levels to paragraph embedding level.
    p.resultLevels = new slice<level>(p.Len());
    setLevels(p.resultLevels, p.embeddingLevel);
    // 2) Explicit levels and directions
    // Rules X1-X8.
    p.determineExplicitEmbeddingLevels();
    // Rule X9.
    // We do not remove the embeddings, the overrides, the PDFs, and the BNs
    // from the string explicitly. But they are not copied into isolating run
    // sequences when they are created, so they are removed for all
    // practical purposes.
    // Rule X10.
    // Run remainder of algorithm one isolating run sequence at a time
    foreach (var (_, seq) in Ꮡp.determineIsolatingRunSequences()) {
        // 3) resolving weak types
        // Rules W1-W7.
        seq.resolveWeakTypes();
        // 4a) resolving paired brackets
        // Rule N0
        resolvePairedBrackets(ref (seq).DerefOrNull());
        // 4b) resolving neutral types
        // Rules N1-N3.
        seq.resolveNeutralTypes();
        // 5) resolving implicit embedding levels
        // Rules I1, I2.
        seq.resolveImplicitLevels();
        // Apply the computed levels and types
        seq.applyLevelsAndTypes();
    }
    // Assign appropriate levels to 'hide' LREs, RLEs, LROs, RLOs, PDFs, and
    // BNs. This is for convenience, so the resulting level array will have
    // a value for every character.
    p.assignLevelsToCharactersRemovedByX9();
}

// determineMatchingIsolates determines the matching PDI for each isolate
// initiator and vice versa.
//
// Definition BD9.
//
// At the end of this function:
//
//   - The member variable matchingPDI is set to point to the index of the
//     matching PDI character for each isolate initiator character. If there is
//     no matching PDI, it is set to the length of the input text. For other
//     characters, it is set to -1.
//   - The member variable matchingIsolateInitiator is set to point to the
//     index of the matching isolate initiator character for each PDI character.
//     If there is no matching isolate initiator, or the character is not a PDI,
//     it is set to -1.
[GoRecv] internal static void determineMatchingIsolates(this ref paragraph p) {
    p.matchingPDI = new slice<nint>(p.Len());
    p.matchingIsolateInitiator = new slice<nint>(p.Len());
    foreach (var (i, _) in p.matchingIsolateInitiator) {
        p.matchingIsolateInitiator[i] = -1;
    }
    foreach (var (i, _) in p.matchingPDI) {
        p.matchingPDI[i] = -1;
        {
            ΔClass t = p.resultTypes[i]; if (t.@in(LRI, RLI, FSI)) {
                nint depthCounter = 1;
                for (nint j = i + 1; j < p.Len(); j++) {
                    {
                        ΔClass u = p.resultTypes[j]; if (u.@in(LRI, RLI, FSI)){
                            depthCounter++;
                        } else 
                        if (u == PDI) {
                            {
                                depthCounter--; if (depthCounter == 0) {
                                    p.matchingPDI[i] = j;
                                    p.matchingIsolateInitiator[j] = i;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (p.matchingPDI[i] == -1) {
                    p.matchingPDI[i] = p.Len();
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object assertIEndˢ = (@string)"assert (i <= end)"u8;

// determineParagraphEmbeddingLevel reports the resolved paragraph direction of
// the substring limited by the given range [start, end).
//
// Determines the paragraph level based on rules P2, P3. This is also used
// in rule X5c to find if an FSI should resolve to LRI or RLI.
[GoRecv] internal static level determineParagraphEmbeddingLevel(this ref paragraph p, nint start, nint end) {
    ΔClass strongType = unknownClass;
    // Rule P2.
    for (nint i = start; i < end; i++) {
        {
            ΔClass t = p.resultTypes[i]; if (t.@in(L, AL, R)){
                strongType = t;
                break;
            } else 
            if (t.@in(FSI, LRI, RLI)) {
                i = p.matchingPDI[i]; // skip over to the matching PDI
                if (i > end) {
                    log.Panic(assertIEndˢ);
                }
            }
        }
    }
    // Rule P3.
    var exprᴛ1 = strongType;
    if (exprᴛ1 == unknownClass) {
        return 0;
    }
    if (exprᴛ1 == L) {
        return 0;
    }
    { /* default: */
        return 1;
    }

}

// none found
// default embedding level when no strong types found is 0.
// AL, R
internal static UntypedInt maxDepth => 125;

// This stack will store the embedding levels and override and isolated
// statuses
[GoType] partial struct directionalStatusStack {
    internal nint stackCounter;
    internal array<level> embeddingLevelStack = new(maxDepth + 1);
    internal array<ΔClass> overrideStatusStack = new(maxDepth + 1);
    internal array<bool> isolateStatusStack = new(maxDepth + 1);
}

[GoRecv] internal static void empty(this ref directionalStatusStack s) {
    s.stackCounter = 0;
}

[GoRecv] internal static void pop(this ref directionalStatusStack s) {
    s.stackCounter--;
}

[GoRecv] internal static nint depth(this ref directionalStatusStack s) {
    return s.stackCounter;
}

[GoRecv] internal static void push(this ref directionalStatusStack s, level level, ΔClass overrideStatus, bool isolateStatus) {
    s.embeddingLevelStack[s.stackCounter] = level;
    s.overrideStatusStack[s.stackCounter] = overrideStatus;
    s.isolateStatusStack[s.stackCounter] = isolateStatus;
    s.stackCounter++;
}

[GoRecv] internal static level lastEmbeddingLevel(this ref directionalStatusStack s) {
    return s.embeddingLevelStack[s.stackCounter - 1];
}

[GoRecv] internal static ΔClass lastDirectionalOverrideStatus(this ref directionalStatusStack s) {
    return s.overrideStatusStack[s.stackCounter - 1];
}

[GoRecv] internal static bool lastDirectionalIsolateStatus(this ref directionalStatusStack s) {
    return s.isolateStatusStack[s.stackCounter - 1];
}

// Determine explicit levels using rules X1 - X8
[GoRecv] internal static void determineExplicitEmbeddingLevels(this ref paragraph p) {
    directionalStatusStack stack = new();
    nint overflowIsolateCount = default!;
    nint overflowEmbeddingCount = default!;
    nint validIsolateCount = default!;
    // Rule X1.
    stack.push(p.embeddingLevel, ON, false);
    foreach (var (i, t) in p.resultTypes) {
        // Rules X2, X3, X4, X5, X5a, X5b, X5c
        var exprᴛ1 = t;
        if (exprᴛ1 == RLE || exprᴛ1 == LRE || exprᴛ1 == RLO || exprᴛ1 == LRO || exprᴛ1 == RLI || exprᴛ1 == LRI || exprᴛ1 == FSI) {
            var isIsolate = t.@in(RLI, LRI, FSI);
            var isRTL = t.@in(RLE, RLO, RLI);
            if (t == FSI) {
                // override if this is an FSI that resolves to RLI
                isRTL = (p.determineParagraphEmbeddingLevel(i + 1, p.matchingPDI[i]) == 1);
            }
            if (isIsolate) {
                p.resultLevels[i] = stack.lastEmbeddingLevel();
                if (stack.lastDirectionalOverrideStatus() != ON) {
                    p.resultTypes[i] = stack.lastDirectionalOverrideStatus();
                }
            }
            level newLevel = default!;
            if (isRTL){
                // least greater odd
                newLevel = (level)((stack.lastEmbeddingLevel() + 1) | 1);
            } else {
                // least greater even
                newLevel = (level)((stack.lastEmbeddingLevel() + 2) & ~1);
            }
            if (newLevel <= maxDepth && overflowIsolateCount == 0 && overflowEmbeddingCount == 0){
                if (isIsolate) {
                    validIsolateCount++;
                }
                // Push new embedding level, override status, and isolated
                // status.
                // No check for valid stack counter, since the level check
                // suffices.
                var exprᴛ2 = t;
                if (exprᴛ2 == LRO) {
                    stack.push(newLevel, L, isIsolate);
                }
                else if (exprᴛ2 == RLO) {
                    stack.push(newLevel, R, isIsolate);
                }
                else { /* default: */
                    stack.push(newLevel, ON, isIsolate);
                }

                // Not really part of the spec
                if (!isIsolate) {
                    p.resultLevels[i] = newLevel;
                }
            } else {
                // This is an invalid explicit formatting character,
                // so apply the "Otherwise" part of rules X2-X5b.
                if (isIsolate){
                    overflowIsolateCount++;
                } else {
                    // !isIsolate
                    if (overflowIsolateCount == 0) {
                        overflowEmbeddingCount++;
                    }
                }
            }
        }
        else if (exprᴛ1 == PDI) {
            if (overflowIsolateCount > 0){
                // Rule X6a
                overflowIsolateCount--;
            } else 
            if (validIsolateCount == 0){
            } else {
                // do nothing
                overflowEmbeddingCount = 0;
                while (!stack.lastDirectionalIsolateStatus()) {
                    stack.pop();
                }
                stack.pop();
                validIsolateCount--;
            }
            p.resultLevels[i] = stack.lastEmbeddingLevel();
        }
        else if (exprᴛ1 == PDF) {
            p.resultLevels[i] = stack.lastEmbeddingLevel();
            if (overflowIsolateCount > 0){
            } else 
            if (overflowEmbeddingCount > 0){
                // Rule X7
                // Not really part of the spec
                // do nothing
                overflowEmbeddingCount--;
            } else 
            if (!stack.lastDirectionalIsolateStatus() && stack.depth() >= 2) {
                stack.pop();
            }
        }
        else if (exprᴛ1 == B) {
            stack.empty();
            overflowIsolateCount = 0;
            overflowEmbeddingCount = 0;
            validIsolateCount = 0;
            p.resultLevels[i] = p.embeddingLevel;
        }
        else { /* default: */
            p.resultLevels[i] = stack.lastEmbeddingLevel();
            if (stack.lastDirectionalOverrideStatus() != ON) {
                // paragraph separator.
                // Rule X8.
                // These values are reset for clarity, in this implementation B
                // can only occur as the last code in the array.
                p.resultTypes[i] = stack.lastDirectionalOverrideStatus();
            }
        }

    }
}

[GoType] partial struct ΔisolatingRunSequence {
    internal ж<paragraph> p;
    internal slice<nint> indexes; // indexes to the original string
    internal slice<ΔClass> types; // type of each character using the index
    internal slice<level> resolvedLevels; // resolved levels after application of rules
    internal level level;
    internal ΔClass sos, eos;
}

[GoRecv] internal static nint Len(this ref ΔisolatingRunSequence i) {
    return len(i.indexes);
}

internal static level maxLevel(level a, level b) {
    if (a > b) {
        return a;
    }
    return b;
}

// Rule X10, second bullet: Determine the start-of-sequence (sos) and end-of-sequence (eos) types,
// either L or R, for each isolating run sequence.
internal static ж<ΔisolatingRunSequence> isolatingRunSequence(this ж<paragraph> Ꮡp, slice<nint> indexes) {
    ref var p = ref Ꮡp.DerefOrNull();

    nint length = len(indexes);
    var types = new slice<ΔClass>(length);
    foreach (var (i, x) in indexes) {
        types[i] = p.resultTypes[x];
    }
    // assign level, sos and eos
    nint prevChar = indexes[0] - 1;
    while (prevChar >= 0 && isRemovedByX9(p.initialTypes[prevChar])) {
        prevChar--;
    }
    var prevLevel = p.embeddingLevel;
    if (prevChar >= 0) {
        prevLevel = p.resultLevels[prevChar];
    }
    level succLevel = default!;
    ΔClass lastType = types[length - 1];
    if (lastType.@in(LRI, RLI, FSI)){
        succLevel = p.embeddingLevel;
    } else {
        // the first character after the end of run sequence
        nint limit = indexes[length - 1] + 1;
        for (; limit < p.Len() && isRemovedByX9(p.initialTypes[limit]); limit++) {
        }
        succLevel = p.embeddingLevel;
        if (limit < p.Len()) {
            succLevel = p.resultLevels[limit];
        }
    }
    ref var level = ref heap<level>(out var Ꮡlevel);
    level = p.resultLevels[indexes[0]];
    return Ꮡ(new ΔisolatingRunSequence(
        p: Ꮡp,
        indexes: indexes,
        types: types,
        level: level,
        sos: typeForLevel(maxLevel(prevLevel, level)),
        eos: typeForLevel(maxLevel(succLevel, level))
    ));
}

// Resolving weak types Rules W1-W7.
//
// Note that some weak types (EN, AN) remain after this processing is
// complete.
[GoRecv] internal static void resolveWeakTypes(this ref ΔisolatingRunSequence s) {
    // on entry, only these types remain
    s.assertOnly(L, R, AL, EN, ES, ET, AN, CS, B, S, WS, ON, NSM, LRI, RLI, FSI, PDI);
    // Rule W1.
    // Changes all NSMs.
    ΔClass precedingCharacterType = s.sos;
    foreach (var (i, t) in s.types) {
        if (t == NSM){
            s.types[i] = precedingCharacterType;
        } else {
            // if t.in(LRI, RLI, FSI, PDI) {
            // 	precedingCharacterType = ON
            // }
            precedingCharacterType = t;
        }
    }
    // Rule W2.
    // EN does not change at the start of the run, because sos != AL.
    foreach (var (i, t) in s.types) {
        if (t == EN) {
            for (nint j = i - 1; j >= 0; j--) {
                {
                    ΔClass tΔ1 = s.types[j]; if (tΔ1.@in(L, R, AL)) {
                        if (tΔ1 == AL) {
                            s.types[i] = AN;
                        }
                        break;
                    }
                }
            }
        }
    }
    // Rule W3.
    foreach (var (i, t) in s.types) {
        if (t == AL) {
            s.types[i] = R;
        }
    }
    // Rule W4.
    // Since there must be values on both sides for this rule to have an
    // effect, the scan skips the first and last value.
    //
    // Although the scan proceeds left to right, and changes the type
    // values in a way that would appear to affect the computations
    // later in the scan, there is actually no problem. A change in the
    // current value can only affect the value to its immediate right,
    // and only affect it if it is ES or CS. But the current value can
    // only change if the value to its right is not ES or CS. Thus
    // either the current value will not change, or its change will have
    // no effect on the remainder of the analysis.
    for (nint i = 1; i < s.Len() - 1; i++) {
        ΔClass t = s.types[i];
        if (t == ES || t == CS) {
            ΔClass prevSepType = s.types[i - 1];
            ΔClass succSepType = s.types[i + 1];
            if (prevSepType == EN && succSepType == EN){
                s.types[i] = EN;
            } else 
            if (s.types[i] == CS && prevSepType == AN && succSepType == AN) {
                s.types[i] = AN;
            }
        }
    }
    // Rule W5.
    foreach (var (iᴛ1, t) in s.types) {
        var i = iᴛ1;

        if (t == ET) {
            // locate end of sequence
            nint runStart = i;
            nint runEnd = s.findRunLimit(runStart, ET);
            // check values at ends of sequence
            ΔClass tΔ1 = s.sos;
            if (runStart > 0) {
                tΔ1 = s.types[runStart - 1];
            }
            if (tΔ1 != EN) {
                tΔ1 = s.eos;
                if (runEnd < len(s.types)) {
                    tΔ1 = s.types[runEnd];
                }
            }
            if (tΔ1 == EN) {
                setTypes(s.types.slice(runStart, runEnd), EN);
            }
            // continue at end of sequence
            i = runEnd;
        }
    }
    // Rule W6.
    foreach (var (i, t) in s.types) {
        if (t.@in(ES, ET, CS)) {
            s.types[i] = ON;
        }
    }
    // Rule W7.
    foreach (var (i, vᴛ1) in s.types) {
        var t = vᴛ1;

        if (t == EN) {
            // set default if we reach start of run
            ΔClass prevStrongType = s.sos;
            for (nint j = i - 1; j >= 0; j--) {
                t = s.types[j];
                if (t == L || t == R) {
                    // AL's have been changed to R
                    prevStrongType = t;
                    break;
                }
            }
            if (prevStrongType == L) {
                s.types[i] = L;
            }
        }
    }
}

// 6) resolving neutral types Rules N1-N2.
[GoRecv] internal static void resolveNeutralTypes(this ref ΔisolatingRunSequence s) {
    // on entry, only these types can be in resultTypes
    s.assertOnly(L, R, EN, AN, B, S, WS, ON, RLI, LRI, FSI, PDI);
    foreach (var (iᴛ1, t) in s.types) {
        var i = iᴛ1;

        var exprᴛ1 = t;
        if (exprᴛ1 == WS || exprᴛ1 == ON || exprᴛ1 == B || exprᴛ1 == S || exprᴛ1 == RLI || exprᴛ1 == LRI || exprᴛ1 == FSI || exprᴛ1 == PDI) {
            nint runStart = i;
            nint runEnd = s.findRunLimit(runStart, // find bounds of run of neutrals
 B, S, WS, ON, RLI, LRI, FSI, PDI);
            // determine effective types at ends of run
            ΔClass leadType = default!;
            ΔClass trailType = default!;
            if (runStart == 0){
                // Note that the character found can only be L, R, AN, or
                // EN.
                leadType = s.sos;
            } else {
                leadType = s.types[runStart - 1];
                if (leadType.@in(AN, EN)) {
                    leadType = R;
                }
            }
            if (runEnd == len(s.types)){
                trailType = s.eos;
            } else {
                trailType = s.types[runEnd];
                if (trailType.@in(AN, EN)) {
                    trailType = R;
                }
            }
            ΔClass resolvedType = default!;
            if (leadType == trailType){
                // Rule N1.
                resolvedType = leadType;
            } else {
                // Rule N2.
                // Notice the embedding level of the run is used, not
                // the paragraph embedding level.
                resolvedType = typeForLevel(s.level);
            }
            setTypes(s.types.slice(runStart, runEnd), resolvedType);
            i = runEnd;
        }

    }
}

// skip over run of (former) neutrals
internal static void setLevels(slice<level> levels, level newLevel) {
    foreach (var (i, _) in levels) {
        levels[i] = newLevel;
    }
}

internal static void setTypes(slice<ΔClass> types, ΔClass newType) {
    foreach (var (i, _) in types) {
        types[i] = newType;
    }
}

// 7) resolving implicit embedding levels Rules I1, I2.
[GoRecv] internal static void resolveImplicitLevels(this ref ΔisolatingRunSequence s) {
    // on entry, only these types can be in resultTypes
    s.assertOnly(L, R, EN, AN);
    s.resolvedLevels = new slice<level>(len(s.types));
    setLevels(s.resolvedLevels, s.level);
    if (((level)(s.level & 1)) == 0){
        // even level
        foreach (var (i, t) in s.types) {
            // Rule I1.
            if (t == L){
            } else 
            if (t == R){
                // no change
                s.resolvedLevels[i] += 1;
            } else {
                // t == AN || t == EN
                s.resolvedLevels[i] += 2;
            }
        }
    } else {
        // odd level
        foreach (var (i, t) in s.types) {
            // Rule I2.
            if (t == R){
            } else {
                // no change
                // t == L || t == AN || t == EN
                s.resolvedLevels[i] += 1;
            }
        }
    }
}

// Applies the levels and types resolved in rules W1-I2 to the
// resultLevels array.
[GoRecv] internal static void applyLevelsAndTypes(this ref ΔisolatingRunSequence s) {
    foreach (var (i, x) in s.indexes) {
        s.p.Value.resultTypes[x] = s.types[i];
        s.p.Value.resultLevels[x] = s.resolvedLevels[i];
    }
}

// Return the limit of the run consisting only of the types in validSet
// starting at index. This checks the value at index, and will return
// index if that value is not in validSet.
[GoRecv] internal static nint findRunLimit(this ref ΔisolatingRunSequence s, nint index, params ꓸꓸꓸClass validSetʗp) {
    var validSet = validSetʗp.sslice();

loop:
    for (; index < len(s.types); index++) {
        ΔClass t = s.types[index];
        foreach (var (_, valid) in validSet) {
            if (t == valid) {
                goto continue_loop;
            }
        }
        return index; // didn't find a match in validSet
continue_loop:;
    }
break_loop:;
    return len(s.types);
}

// Algorithm validation. Assert that all values in types are in the
// provided set.
[GoRecv] internal static void assertOnly(this ref ΔisolatingRunSequence s, params ꓸꓸꓸClass codesʗp) {
    var codes = codesʗp.sslice();

loop:
    foreach (var (i, t) in s.types) {
        foreach (var (_, c) in codes) {
            if (t == c) {
                goto continue_loop;
            }
        }
        log.Panicf("invalid bidi code %v present in assertOnly at position %d"u8, t, s.indexes[i]);
continue_loop:;
    }
break_loop:;
}

// determineLevelRuns returns an array of level runs. Each level run is
// described as an array of indexes into the input string.
//
// Determines the level runs. Rule X9 will be applied in determining the
// runs, in the way that makes sure the characters that are supposed to be
// removed are not included in the runs.
[GoRecv] internal static slice<slice<nint>> determineLevelRuns(this ref paragraph p) {
    var run = new nint[]{}.slice();
    var allRuns = new slice<nint>[]{}.slice();
    var currentLevel = implicitLevel;
    foreach (var (i, _) in p.initialTypes) {
        if (!isRemovedByX9(p.initialTypes[i])) {
            if (p.resultLevels[i] != currentLevel) {
                // we just encountered a new run; wrap up last run
                if (currentLevel >= 0) {
                    // only wrap it up if there was a run
                    allRuns = append(allRuns, run);
                    run = default!;
                }
                // Start new run
                currentLevel = p.resultLevels[i];
            }
            run = append(run, i);
        }
    }
    // Wrap up the final run, if any
    if (len(run) > 0) {
        allRuns = append(allRuns, run);
    }
    return allRuns;
}

// Definition BD13. Determine isolating run sequences.
internal static slice<ж<ΔisolatingRunSequence>> determineIsolatingRunSequences(this ж<paragraph> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    var levelRuns = p.determineLevelRuns();
    // Compute the run that each character belongs to
    var runForCharacter = new slice<nint>(p.Len());
    foreach (var (i, run) in levelRuns) {
        foreach (var (_, index) in run) {
            runForCharacter[index] = i;
        }
    }
    var sequences = new ж<ΔisolatingRunSequence>[]{}.slice();
    slice<nint> currentRunSequence = default!;
    foreach (var (_, vᴛ1) in levelRuns) {
        var run = vᴛ1;

        nint first = run[0];
        if (p.initialTypes[first] != PDI || p.matchingIsolateInitiator[first] == -1) {
            currentRunSequence = default!;
            // int run = i;
            while (ᐧ) {
                // Copy this level run into currentRunSequence
                currentRunSequence = appendꓸꓸꓸ(currentRunSequence, run);
                nint last = currentRunSequence[len(currentRunSequence) - 1];
                ΔClass lastT = p.initialTypes[last];
                if (lastT.@in(LRI, RLI, FSI) && p.matchingPDI[last] != p.Len()){
                    run = levelRuns[runForCharacter[p.matchingPDI[last]]];
                } else {
                    break;
                }
            }
            sequences = append(sequences, Ꮡp.isolatingRunSequence(currentRunSequence));
        }
    }
    return sequences;
}

// Assign level information to characters removed by rule X9. This is for
// ease of relating the level information to the original input data. Note
// that the levels assigned to these codes are arbitrary, they're chosen so
// as to avoid breaking level runs.
[GoRecv] internal static void assignLevelsToCharactersRemovedByX9(this ref paragraph p) {
    foreach (var (i, t) in p.initialTypes) {
        if (t.@in(LRE, RLE, LRO, RLO, PDF, BN)) {
            p.resultTypes[i] = t;
            p.resultLevels[i] = (level)(-1);
        }
    }
    // now propagate forward the levels information (could have
    // propagated backward, the main thing is not to introduce a level
    // break where one doesn't already exist).
    if (p.resultLevels[0] == -1) {
        p.resultLevels[0] = p.embeddingLevel;
    }
    for (nint i = 1; i < len(p.initialTypes); i++) {
        if (p.resultLevels[i] == -1) {
            p.resultLevels[i] = p.resultLevels[i - 1];
        }
    }
}

// Embedding information is for informational purposes only so need not be
// adjusted.
//
// Output
//

// getLevels computes levels array breaking lines at offsets in linebreaks.
// Rule L1.
//
// The linebreaks array must include at least one value. The values must be
// in strictly increasing order (no duplicates) between 1 and the length of
// the text, inclusive. The last value must be the length of the text.
[GoRecv] internal static slice<level> getLevels(this ref paragraph p, slice<nint> linebreaks) {
    // Note that since the previous processing has removed all
    // P, S, and WS values from resultTypes, the values referred to
    // in these rules are the initial types, before any processing
    // has been applied (including processing of overrides).
    //
    // This example implementation has reinserted explicit format codes
    // and BN, in order that the levels array correspond to the
    // initial text. Their final placement is not normative.
    // These codes are treated like WS in this implementation,
    // so they don't interrupt sequences of WS.
    validateLineBreaks(linebreaks, p.Len());
    var result = appendꓸꓸꓸ(slice<level>(default!), p.resultLevels);
    // don't worry about linebreaks since if there is a break within
    // a series of WS values preceding S, the linebreak itself
    // causes the reset.
    foreach (var (i, t) in p.initialTypes) {
        if (t.@in(B, S)) {
            // Rule L1, clauses one and two.
            result[i] = p.embeddingLevel;
            // Rule L1, clause three.
            for (nint j = i - 1; j >= 0; j--) {
                if (isWhitespace(p.initialTypes[j])){
                    // including format codes
                    result[j] = p.embeddingLevel;
                } else {
                    break;
                }
            }
        }
    }
    // Rule L1, clause four.
    nint start = 0;
    foreach (var (_, limit) in linebreaks) {
        for (nint j = limit - 1; j >= start; j--) {
            if (isWhitespace(p.initialTypes[j])){
                // including format codes
                result[j] = p.embeddingLevel;
            } else {
                break;
            }
        }
        start = limit;
    }
    return result;
}

// getReordering returns the reordering of lines from a visual index to a
// logical index for line breaks at the given offsets.
//
// Lines are concatenated from left to right. So for example, the fifth
// character from the left on the third line is
//
//	getReordering(linebreaks)[linebreaks[1] + 4]
//
// (linebreaks[1] is the position after the last character of the second
// line, which is also the index of the first character on the third line,
// and adding four gets the fifth character from the left).
//
// The linebreaks array must include at least one value. The values must be
// in strictly increasing order (no duplicates) between 1 and the length of
// the text, inclusive. The last value must be the length of the text.
[GoRecv] internal static slice<nint> getReordering(this ref paragraph p, slice<nint> linebreaks) {
    validateLineBreaks(linebreaks, p.Len());
    return computeMultilineReordering(p.getLevels(linebreaks), linebreaks);
}

// Return multiline reordering array for a given level array. Reordering
// does not occur across a line break.
internal static slice<nint> computeMultilineReordering(slice<level> levels, slice<nint> linebreaks) {
    var result = new slice<nint>(len(levels));
    nint start = 0;
    foreach (var (_, limit) in linebreaks) {
        var tempLevels = new slice<level>(limit - start);
        copy(tempLevels, levels.slice(start));
        foreach (var (j, order) in computeReordering(tempLevels)) {
            result[start + j] = order + start;
        }
        start = limit;
    }
    return result;
}

// Return reordering array for a given level array. This reorders a single
// line. The reordering is a visual to logical map. For example, the
// leftmost char is string.charAt(order[0]). Rule L2.
internal static slice<nint> computeReordering(slice<level> levels) {
    var result = new slice<nint>(len(levels));
    // initialize order
    foreach (var (i, _) in result) {
        result[i] = i;
    }
    // locate highest level found on line.
    // Note the rules say text, but no reordering across line bounds is
    // performed, so this is sufficient.
    var highestLevel = ((level)0);
    var lowestOddLevel = ((level)(maxDepth + 2));
    foreach (var (_, level) in levels) {
        if (level > highestLevel) {
            highestLevel = level;
        }
        if ((level)(level & 1) != 0 && level < lowestOddLevel) {
            lowestOddLevel = level;
        }
    }
    for (var level = highestLevel; level >= lowestOddLevel; level--) {
        for (nint i = 0; i < len(levels); i++) {
            if (levels[i] >= level) {
                // find range of text at or above this level
                nint start = i;
                nint limit = i + 1;
                while (limit < len(levels) && levels[limit] >= level) {
                    limit++;
                }
                for ((nint j, nint k) = (start, limit - 1); j < k; (j, k) = (j + 1, k - 1)) {
                    (result[j], result[k]) = (result[k], result[j]);
                }
                // skip to end of level run
                i = limit;
            }
        }
    }
    return result;
}

// isWhitespace reports whether the type is considered a whitespace type for the
// line break rules.
internal static bool isWhitespace(ΔClass c) {
    var exprᴛ1 = c;
    if (exprᴛ1 == LRE || exprᴛ1 == RLE || exprᴛ1 == LRO || exprᴛ1 == RLO || exprᴛ1 == PDF || exprᴛ1 == LRI || exprᴛ1 == RLI || exprᴛ1 == FSI || exprᴛ1 == PDI || exprᴛ1 == BN || exprᴛ1 == WS) {
        return true;
    }

    return false;
}

// isRemovedByX9 reports whether the type is one of the types removed in X9.
internal static bool isRemovedByX9(ΔClass c) {
    var exprᴛ1 = c;
    if (exprᴛ1 == LRE || exprᴛ1 == RLE || exprᴛ1 == LRO || exprᴛ1 == RLO || exprᴛ1 == PDF || exprᴛ1 == BN) {
        return true;
    }

    return false;
}

// typeForLevel reports the strong type (L or R) corresponding to the level.
internal static ΔClass typeForLevel(level level) {
    if (((level)(level & 0x1)) == 0) {
        return L;
    }
    return R;
}

internal static error validateTypes(slice<ΔClass> types) {
    if (len(types) == 0) {
        return fmt.Errorf("types is null"u8);
    }
    foreach (var (i, t) in types.slice(0, len(types) - 1)) {
        if (t == B) {
            return fmt.Errorf("B type before end of paragraph at index: %d"u8, i);
        }
    }
    return default!;
}

internal static error validateParagraphEmbeddingLevel(level embeddingLevel) {
    if (embeddingLevel != implicitLevel && embeddingLevel != 0 && embeddingLevel != 1) {
        return fmt.Errorf("illegal paragraph embedding level: %d"u8, embeddingLevel);
    }
    return default!;
}

internal static error validateLineBreaks(slice<nint> linebreaks, nint textLength) {
    nint prev = 0;
    foreach (var (i, next) in linebreaks) {
        if (next <= prev) {
            return fmt.Errorf("bad linebreak: %d at index: %d"u8, next, i);
        }
        prev = next;
    }
    if (prev != textLength) {
        return fmt.Errorf("last linebreak was %d, want %d"u8, prev, textLength);
    }
    return default!;
}

internal static error validatePbTypes(slice<bracketType> pairTypes) {
    if (len(pairTypes) == 0) {
        return fmt.Errorf("pairTypes is null"u8);
    }
    foreach (var (i, pt) in pairTypes) {
        var exprᴛ1 = pt;
        if (exprᴛ1 == bpNone || exprᴛ1 == bpOpen || exprᴛ1 == bpClose) {
        }
        else { /* default: */
            return fmt.Errorf("illegal pairType value at %d: %v"u8, i, pairTypes[i]);
        }

    }
    return default!;
}

internal static error validatePbValues(slice<rune> pairValues, slice<bracketType> pairTypes) {
    if (pairValues == default!) {
        return fmt.Errorf("pairValues is null"u8);
    }
    if (len(pairTypes) != len(pairValues)) {
        return fmt.Errorf("pairTypes is different length from pairValues"u8);
    }
    return default!;
}

} // end bidi_package
