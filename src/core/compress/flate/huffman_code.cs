// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

using math = math_package;
using bits = go.math.bits_package;
using sort = sort_package;
using go.math;

partial class flate_package {

// hcode is a huffman code with a bit code and bit length.
[GoType] partial struct hcode {
    internal uint16 code, len;
}

[GoType] partial struct huffmanEncoder {
    internal slice<hcode> codes;
    internal slice<literalNode> freqcache;
    internal array<int32> bitCount = new(17);
    internal byLiteral lns; // stored to avoid repeated allocation in generate
    internal byFreq lfs;    // stored to avoid repeated allocation in generate
}

[GoType] partial struct literalNode {
    internal uint16 literal;
    internal int32 freq;
}

// A levelInfo describes the state of the constructed tree for a given depth.
[GoType] partial struct levelInfo {
    // Our level.  for better printing
    internal int32 level;
    // The frequency of the last node at this level
    internal int32 lastFreq;
    // The frequency of the next character to add to this level
    internal int32 nextCharFreq;
    // The frequency of the next pair (from level below) to add to this level.
    // Only valid if the "needed" value of the next lower level is 0.
    internal int32 nextPairFreq;
    // The number of chains remaining to generate for this level before moving
    // up to the next level
    internal int32 needed;
}

// set sets the code and length of an hcode.
[GoRecv] internal static void set(this ref hcode h, uint16 code, uint16 length) {
    h.len = length;
    h.code = code;
}

internal static literalNode maxNode() {
    return new literalNode(math.MaxUint16, math.MaxInt32);
}

internal static ж<huffmanEncoder> newHuffmanEncoder(nint size) {
    return Ꮡ(new huffmanEncoder(codes: new slice<hcode>(size)));
}

// Generates a HuffmanCode corresponding to the fixed literal table.
internal static ж<huffmanEncoder> generateFixedLiteralEncoding() {
    var h = newHuffmanEncoder(maxNumLit);
    var codes = h.Value.codes;
    uint16 ch = default!;
    for (ch = 0; ch < maxNumLit; ch++) {
        uint16 bits = default!;
        uint16 size = default!;
        switch (ᐧ) {
        case {} when ch is < 144: {
            bits = (uint16)(ch + 48);
            size = 8;
            break;
        }
        case {} when ch is < 256: {
            bits = (uint16)(ch + 400 - 144);
            size = 9;
            break;
        }
        case {} when ch is < 280: {
            bits = (uint16)(ch - 256);
            size = 7;
            break;
        }
        default: {
            bits = (uint16)(ch + 192 - 280);
            size = 8;
            break;
        }}

        // size 8, 000110000  .. 10111111
        // size 9, 110010000 .. 111111111
        // size 7, 0000000 .. 0010111
        // size 8, 11000000 .. 11000111
        codes[ch] = new hcode(code: reverseBits(bits, (byte)size), len: size);
    }
    return h;
}

internal static ж<huffmanEncoder> generateFixedOffsetEncoding() {
    var h = newHuffmanEncoder(30);
    var codes = h.Value.codes;
    foreach (var (ch, _) in codes) {
        codes[ch] = new hcode(code: reverseBits((uint16)ch, 5), len: 5);
    }
    return h;
}

internal static ж<huffmanEncoder> fixedLiteralEncoding = generateFixedLiteralEncoding();

internal static ж<huffmanEncoder> fixedOffsetEncoding = generateFixedOffsetEncoding();

[GoRecv] internal static nint bitLength(this ref huffmanEncoder h, slice<int32> freq) {
    nint total = default!;
    foreach (var (i, f) in freq) {
        if (f != 0) {
            total += (nint)f * (nint)h.codes[i].len;
        }
    }
    return total;
}

internal static UntypedInt maxBitsLimit => 16;

// bitCounts computes the number of literals assigned to each bit size in the Huffman encoding.
// It is only called when list.length >= 3.
// The cases of 0, 1, and 2 literals are handled by special case code.
//
// list is an array of the literals with non-zero frequencies
// and their associated frequencies. The array is in order of increasing
// frequency and has as its last element a special element with frequency
// MaxInt32.
//
// maxBits is the maximum number of bits that should be used to encode any literal.
// It must be less than 16.
//
// bitCounts returns an integer slice in which slice[i] indicates the number of literals
// that should be encoded in i bits.
[GoRecv] internal static slice<int32> bitCounts(this ref huffmanEncoder h, slice<literalNode> list, int32 maxBits) {
    if (maxBits >= maxBitsLimit) {
        throw panic("flate: maxBits too large");
    }
    var n = (int32)len(list);
    list = list.slice(0, n + 1);
    list[n] = maxNode();
    // The tree can't have greater depth than n - 1, no matter what. This
    // saves a little bit of work in some small cases
    if (maxBits > n - 1) {
        maxBits = n - 1;
    }
    // Create information about each of the levels.
    // A bogus "Level 0" whose sole purpose is so that
    // level1.prev.needed==0.  This makes level1.nextPairFreq
    // be a legitimate value that never gets chosen.
    ref var levels = ref heap(new array<levelInfo>(16), out var Ꮡlevels);
    // leafCounts[i] counts the number of literals at the left
    // of ancestors of the rightmost node at level i.
    // leafCounts[i][j] is the number of literals at the left
    // of the level j ancestor.
    ref var leafCounts = ref heap(new array<array<int32>>(16, () => new(16)), out var ᏑleafCounts);
    for (var levelΔ1 = (int32)1; levelΔ1 <= maxBits; levelΔ1++) {
        // For every level, the first two items are the first two characters.
        // We initialize the levels as if we had already figured this out.
        levels[levelΔ1] = new levelInfo(
            level: levelΔ1,
            lastFreq: list[1].freq,
            nextCharFreq: list[2].freq,
            nextPairFreq: list[0].freq + list[1].freq
        );
        leafCounts[levelΔ1][levelΔ1] = 2;
        if (levelΔ1 == 1) {
            levels[levelΔ1].nextPairFreq = math.MaxInt32;
        }
    }
    // We need a total of 2*n - 2 items at top level and have already generated 2.
    levels[maxBits].needed = 2 * n - 4;
    var level = maxBits;
    while (ᐧ) {
        var l = Ꮡlevels.at<levelInfo>((nint)(level));
        if ((~l).nextPairFreq == math.MaxInt32 && (~l).nextCharFreq == math.MaxInt32) {
            // We've run out of both leaves and pairs.
            // End all calculations for this level.
            // To make sure we never come back to this level or any lower level,
            // set nextPairFreq impossibly large.
            l.Value.needed = 0;
            levels[level + 1].nextPairFreq = math.MaxInt32;
            level++;
            continue;
        }
        var prevFreq = l.Value.lastFreq;
        if ((~l).nextCharFreq < (~l).nextPairFreq){
            // The next item on this row is a leaf node.
            var nΔ1 = leafCounts[level][level] + 1;
            l.Value.lastFreq = l.Value.nextCharFreq;
            // Lower leafCounts are the same of the previous node.
            leafCounts[level][level] = nΔ1;
            l.Value.nextCharFreq = list[nΔ1].freq;
        } else {
            // The next item on this row is a pair from the previous row.
            // nextPairFreq isn't valid until we generate two
            // more values in the level below
            l.Value.lastFreq = l.Value.nextPairFreq;
            // Take leaf counts from the lower level, except counts[level] remains the same.
            copy(leafCounts[level].slice(0, level), leafCounts[level - 1].slice(0, level));
            levels[(~l).level - 1].needed = 2;
        }
        {
            l.Value.needed--; if ((~l).needed == 0){
                // We've done everything we need to do for this level.
                // Continue calculating one level up. Fill in nextPairFreq
                // of that level with the sum of the two nodes we've just calculated on
                // this level.
                if ((~l).level == maxBits) {
                    // All done!
                    break;
                }
                levels[(~l).level + 1].nextPairFreq = prevFreq + (~l).lastFreq;
                level++;
            } else {
                // If we stole from below, move down temporarily to replenish it.
                while (levels[level - 1].needed > 0) {
                    level--;
                }
            }
        }
    }
    // Somethings is wrong if at the end, the top level is null or hasn't used
    // all of the leaves.
    if (leafCounts[maxBits][maxBits] != n) {
        throw panic("leafCounts[maxBits][maxBits] != n");
    }
    var bitCount = h.bitCount.slice(0, maxBits + 1);
    nint bits = 1;
    var counts = ᏑleafCounts.at<array<int32>>((nint)(maxBits));
    for (var levelΔ2 = maxBits; levelΔ2 > 0; levelΔ2--) {
        // chain.leafCount gives the number of literals requiring at least "bits"
        // bits to encode.
        bitCount[bits] = counts.Value[levelΔ2] - counts.Value[levelΔ2 - 1];
        bits++;
    }
    return bitCount;
}

// Look at the leaves and assign them a bit count and an encoding as specified
// in RFC 1951 3.2.2
internal static void assignEncodingAndSize(this ж<huffmanEncoder> Ꮡh, slice<int32> bitCount, slice<literalNode> list) {
    ref var h = ref Ꮡh.DerefOrNull();

    var code = (uint16)0;
    foreach (var (n, bits) in bitCount) {
        code <<= (int)(1);
        if (n == 0 || bits == 0) {
            continue;
        }
        // The literals list[len(list)-bits] .. list[len(list)-bits]
        // are encoded using "bits" bits, and get the values
        // code, code + 1, ....  The code values are
        // assigned in literal order (not frequency order).
        var chunk = list.slice(len(list) - (nint)bits);
        Ꮡh.of(huffmanEncoder.Ꮡlns).sort(chunk);
        foreach (var (_, node) in chunk) {
            h.codes[node.literal] = new hcode(code: reverseBits(code, (uint8)n), len: (uint16)n);
            code++;
        }
        list = list.slice(0, len(list) - (nint)bits);
    }
}

// Update this Huffman Code object to be the minimum code for the specified frequency count.
//
// freq is an array of frequencies, in which freq[i] gives the frequency of literal i.
// maxBits  The maximum number of bits to use for any literal.
internal static void generate(this ж<huffmanEncoder> Ꮡh, slice<int32> freq, int32 maxBits) {
    ref var h = ref Ꮡh.DerefOrNull();

    if (h.freqcache == default!) {
        // Allocate a reusable buffer with the longest possible frequency table.
        // Possible lengths are codegenCodeCount, offsetCodeCount and maxNumLit.
        // The largest of these is maxNumLit, so we allocate for that case.
        h.freqcache = new slice<literalNode>(maxNumLit + 1);
    }
    var list = h.freqcache.slice(0, len(freq) + 1);
    // Number of non-zero literals
    nint count = 0;
    // Set list to be the set of all non-zero literals and their frequencies
    foreach (var (i, f) in freq) {
        if (f != 0){
            list[count] = new literalNode((uint16)i, f);
            count++;
        } else {
            h.codes[i].len = 0;
        }
    }
    list = list.slice(0, count);
    if (count <= 2) {
        // Handle the small cases here, because they are awkward for the general case code. With
        // two or fewer literals, everything has bit length 1.
        foreach (var (i, node) in list) {
            // "list" is in order of increasing literal value.
            h.codes[node.literal].set((uint16)i, 1);
        }
        return;
    }
    Ꮡh.of(huffmanEncoder.Ꮡlfs).sort(list);
    // Get the number of literals for each bit count
    var bitCount = h.bitCounts(list, maxBits);
    // And do the assignment
    Ꮡh.assignEncodingAndSize(bitCount, list);
}

[GoType("[]literalNode")] partial struct byLiteral;

internal static void sort(this ж<byLiteral> Ꮡs, slice<literalNode> a) {
    ref var s = ref Ꮡs.DerefOrNull();

    s = ((byLiteral)a);
    sort_package.Sort(new byLiteralжInterface(Ꮡs));
}

internal static nint Len(this byLiteral s) {
    return len(s);
}

internal static bool Less(this byLiteral s, nint i, nint j) {
    return s[i].literal < s[j].literal;
}

internal static void Swap(this byLiteral s, nint i, nint j) {
    (s[i], s[j]) = (s[j], s[i]);
}

[GoType("[]literalNode")] partial struct byFreq;

internal static void sort(this ж<byFreq> Ꮡs, slice<literalNode> a) {
    ref var s = ref Ꮡs.DerefOrNull();

    s = ((byFreq)a);
    sort_package.Sort(new byFreqжInterface(Ꮡs));
}

internal static nint Len(this byFreq s) {
    return len(s);
}

internal static bool Less(this byFreq s, nint i, nint j) {
    if (s[i].freq == s[j].freq) {
        return s[i].literal < s[j].literal;
    }
    return s[i].freq < s[j].freq;
}

internal static void Swap(this byFreq s, nint i, nint j) {
    (s[i], s[j]) = (s[j], s[i]);
}

internal static uint16 reverseBits(uint16 number, byte bitLength) {
    return bits.Reverse16((uint16)(number.Lsh((uint64)((byte)(16 - bitLength)))));
}

} // end flate_package
