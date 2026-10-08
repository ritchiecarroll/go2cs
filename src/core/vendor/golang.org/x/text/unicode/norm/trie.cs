// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.text.unicode;

partial class norm_package {

partial struct valueRange {
    internal uint16 value; // header: value:stride
    internal byte lo, hi;   // header: lo:n
}

partial struct sparseBlocks {
    internal slice<valueRange> values;
    internal slice<uint16> offset;
}

internal static ж<sparseBlocks> ᏑnfcSparse = new StandardBox<sparseBlocks>(default(sparseBlocks));
internal static ref sparseBlocks nfcSparse => ref ᏑnfcSparse.Value;
internal static void initᴛnfcSparse() { nfcSparse = new sparseBlocks(
    values: nfcSparseValues[..],
    offset: nfcSparseOffset[..]
); }

internal static ж<sparseBlocks> ᏑnfkcSparse = new StandardBox<sparseBlocks>(default(sparseBlocks));
internal static ref sparseBlocks nfkcSparse => ref ᏑnfkcSparse.Value;
internal static void initᴛnfkcSparse() { nfkcSparse = new sparseBlocks(
    values: nfkcSparseValues[..],
    offset: nfkcSparseOffset[..]
); }

internal static ж<nfcTrie> nfcData = newNfcTrie(0);
internal static ж<nfkcTrie> nfkcData = newNfkcTrie(0);

// lookup determines the type of block n and looks up the value for b.
// For n < t.cutoff, the block is a simple lookup table. Otherwise, the block
// is a list of ranges with an accompanying value. Given a matching range r,
// the value for b is by r.value + (b - r.lo) * stride.
internal static uint16 lookup(this ref sparseBlocks t, uint32 n, byte b) {
    var offset = t.offset[n];
    var header = t.values[offset];
    var lo = (uint16)(offset + 1);
    var hi = (uint16)(lo + (uint16)header.lo);
    while (lo < hi) {
        var m = (uint16)(lo + (uint16)(hi - lo) / 2);
        var r = t.values[m];
        if (r.lo <= b && b <= r.hi) {
            return (uint16)(r.value + (uint16)((byte)(b - r.lo)) * header.value);
        }
        if (b < r.lo){
            hi = m;
        } else {
            lo = (uint16)(m + 1);
        }
    }
    return 0;
}

} // end norm_package
