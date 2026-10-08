// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

using cmp = cmp_package;
using slices = slices_package;

partial class bzip2_package {

// A huffmanTree is a binary tree which is navigated, bit-by-bit to reach a
// symbol.
partial struct huffmanTree {
    // nodes contains all the non-leaf nodes in the tree. nodes[0] is the
    // root of the tree and nextNode contains the index of the next element
    // of nodes to use when the tree is being constructed.
    internal slice<huffmanNode> nodes;
    internal nint nextNode;
}

// A huffmanNode is a node in the tree. left and right contain indexes into the
// nodes slice of the tree. If left or right is invalidNodeValue then the child
// is a left node and its value is in leftValue/rightValue.
//
// The symbols are uint16s because bzip2 encodes not only MTF indexes in the
// tree, but also two magic values for run-length encoding and an EOF symbol.
// Thus there are more than 256 possible symbols.
partial struct huffmanNode {
    internal uint16 left, right;
    internal uint16 leftValue, rightValue;
}

// invalidNodeValue is an invalid index which marks a leaf node in the tree.
internal static UntypedInt invalidNodeValue => 0xffff;

// Decode reads bits from the given bitReader and navigates the tree until a
// symbol is found.
internal static uint16 /*v*/ Decode(this ref huffmanTree t, ж<bitReader> Ꮡbr) {
    uint16 v = default!;

    ref var br = ref Ꮡbr.DerefOrNull();
    var nodeIndex = (uint16)0; // node 0 is the root of the tree.
    while (ᐧ) {
        var node = Ꮡ(t.nodes, nodeIndex);
        uint16 bit = default!;
        if (br.bits > 0){
            // Get next bit - fast path.
            br.bits--;
            bit = (uint16)((uint16)((br.n >> (int)(((nuint)(br.bits & 63))))) & 1);
        } else {
            // Get next bit - slow path.
            // Use ReadBits to retrieve a single bit
            // from the underling io.ByteReader.
            bit = (uint16)br.ReadBits(1);
        }
        // Trick a compiler into generating conditional move instead of branch,
        // by making both loads unconditional.
        var (l, r) = (node.Value.left, node.Value.right);
        if (bit == 1){
            nodeIndex = l;
        } else {
            nodeIndex = r;
        }
        if (nodeIndex == invalidNodeValue) {
            // We found a leaf. Use the value of bit to decide
            // whether is a left or a right value.
            var (lΔ1, rΔ1) = (node.Value.leftValue, node.Value.rightValue);
            if (bit == 1){
                v = lΔ1;
            } else {
                v = rΔ1;
            }
            return v;
        }
    }
}

// newHuffmanTree builds a Huffman tree from a slice containing the code
// lengths of each symbol. The maximum code length is 32 bits.
internal static (huffmanTree, error) newHuffmanTree(slice<uint8> lengths) {
    // There are many possible trees that assign the same code length to
    // each symbol (consider reflecting a tree down the middle, for
    // example). Since the code length assignments determine the
    // efficiency of the tree, each of these trees is equally good. In
    // order to minimize the amount of information needed to build a tree
    // bzip2 uses a canonical tree so that it can be reconstructed given
    // only the code length assignments.
    if (len(lengths) < 2) {
        throw panic("newHuffmanTree: too few symbols");
    }
    ref var t = ref heap(new huffmanTree(), out var Ꮡt);
    // First we sort the code length assignments by ascending code length,
    // using the symbol value to break ties.
    var pairs = new slice<huffmanSymbolLengthPair>(len(lengths));
    foreach (var (i, lengthΔ1) in lengths) {
        pairs[i].value = (uint16)i;
        pairs[i].length = lengthΔ1;
    }
    slices.SortFunc(pairs, (huffmanSymbolLengthPair a, huffmanSymbolLengthPair b) => {
        {
            nint c = cmp.Compare(a.length, b.length); if (c != 0) {
                return c;
            }
        }
        return cmp.Compare(a.value, b.value);
    });
    // Now we assign codes to the symbols, starting with the longest code.
    // We keep the codes packed into a uint32, at the most-significant end.
    // So branches are taken from the MSB downwards. This makes it easy to
    // sort them later.
    var code = (uint32)0;
    var length = (uint8)32;
    var codes = new slice<huffmanCode>(len(lengths));
    for (nint i = len(pairs) - 1; i >= 0; i--) {
        if (length > pairs[i].length) {
            length = pairs[i].length;
        }
        codes[i].code = code;
        codes[i].codeLen = length;
        codes[i].value = pairs[i].value;
        // We need to 'increment' the code, which means treating |code|
        // like a |length| bit number.
        code += ((uint32)1).Lsh((uint64)((uint8)(32 - length)));
    }
    // Now we can sort by the code so that the left half of each branch are
    // grouped together, recursively.
    slices.SortFunc(codes, (huffmanCode a, huffmanCode b) => cmp.Compare(a.code, b.code));
    t.nodes = new slice<huffmanNode>(len(codes));
    var (_, err) = buildHuffmanNode(Ꮡt, codes, 0);
    return (t, err);
}

// huffmanSymbolLengthPair contains a symbol and its code length.
partial struct huffmanSymbolLengthPair {
    internal uint16 value;
    internal uint8 length;
}

// huffmanCode contains a symbol, its code and code length.
partial struct huffmanCode {
    internal uint32 code;
    internal uint8 codeLen;
    internal uint16 value;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string emptyHuffmanTreeˢ = "empty Huffman tree"u8;
internal static readonly @string equalSymbolsInHuffmanˢ = "equal symbols in Huffman tree"u8;

// buildHuffmanNode takes a slice of sorted huffmanCodes and builds a node in
// the Huffman tree at the given level. It returns the index of the newly
// constructed node.
internal static (uint16 nodeIndex, error err) buildHuffmanNode(ж<huffmanTree> Ꮡt, slice<huffmanCode> codes, uint32 level) {
    uint16 nodeIndex = default!;
    error err = default!;

    ref var t = ref Ꮡt.DerefOrNull();
    var test = ((uint32)1).Lsh((uint64)((31 - level)));
    // We have to search the list of codes to find the divide between the left and right sides.
    nint firstRightIndex = len(codes);
    foreach (var (i, code) in codes) {
        if ((uint32)(code.code & test) != 0) {
            firstRightIndex = i;
            break;
        }
    }
    var left = codes.slice(0, firstRightIndex);
    var right = codes.slice(firstRightIndex);
    if (len(left) == 0 || len(right) == 0) {
        // There is a superfluous level in the Huffman tree indicating
        // a bug in the encoder. However, this bug has been observed in
        // the wild so we handle it.
        // If this function was called recursively then we know that
        // len(codes) >= 2 because, otherwise, we would have hit the
        // "leaf node" case, below, and not recurred.
        //
        // However, for the initial call it's possible that len(codes)
        // is zero or one. Both cases are invalid because a zero length
        // tree cannot encode anything and a length-1 tree can only
        // encode EOF and so is superfluous. We reject both.
        if (len(codes) < 2) {
            return (0, ((StructuralError)(@string)emptyHuffmanTreeˢ));
        }
        // In this case the recursion doesn't always reduce the length
        // of codes so we need to ensure termination via another
        // mechanism.
        if (level == 31) {
            // Since len(codes) >= 2 the only way that the values
            // can match at all 32 bits is if they are equal, which
            // is invalid. This ensures that we never enter
            // infinite recursion.
            return (0, ((StructuralError)(@string)equalSymbolsInHuffmanˢ));
        }
        if (len(left) == 0) {
            return buildHuffmanNode(Ꮡt, right, level + 1);
        }
        return buildHuffmanNode(Ꮡt, left, level + 1);
    }
    nodeIndex = (uint16)t.nextNode;
    var node = Ꮡ(t.nodes, t.nextNode);
    t.nextNode++;
    if (len(left) == 1){
        // leaf node
        node.Value.left = invalidNodeValue;
        node.Value.leftValue = left[0].value;
    } else {
        (node.Value.left, err) = buildHuffmanNode(Ꮡt, left, level + 1);
    }
    if (err != default!) {
        return (nodeIndex, err);
    }
    if (len(right) == 1){
        // leaf node
        node.Value.right = invalidNodeValue;
        node.Value.rightValue = right[0].value;
    } else {
        (node.Value.right, err) = buildHuffmanNode(Ꮡt, right, level + 1);
    }
    return (nodeIndex, err);
}

} // end bzip2_package
