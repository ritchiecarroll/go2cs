// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

using image = image_package;

partial class jpeg_package {

// makeImg allocates and initializes the destination image.
[GoRecv] internal static void makeImg(this ref decoder d, nint mxx, nint myy) {
    if (d.nComp == 1) {
        var mΔ1 = image.NewGray(image.Rect(0, 0, 8 * mxx, 8 * myy));
        d.img1 = mΔ1.SubImage(image.Rect(0, 0, d.width, d.height))._<ж<image.Gray>>();
        return;
    }
    nint h0 = d.comp[0].h;
    nint v0 = d.comp[0].v;
    nint hRatio = quo(h0, d.comp[1].h);
    nint vRatio = quo(v0, d.comp[1].v);
    image.YCbCrSubsampleRatio subsampleRatio = default!;
    switch ((nint)((hRatio << (int)(4)) | vRatio)) {
    case 0x11: {
        subsampleRatio = image.YCbCrSubsampleRatio444;
        break;
    }
    case 0x12: {
        subsampleRatio = image.YCbCrSubsampleRatio440;
        break;
    }
    case 0x21: {
        subsampleRatio = image.YCbCrSubsampleRatio422;
        break;
    }
    case 0x22: {
        subsampleRatio = image.YCbCrSubsampleRatio420;
        break;
    }
    case 0x41: {
        subsampleRatio = image.YCbCrSubsampleRatio411;
        break;
    }
    case 0x42: {
        subsampleRatio = image.YCbCrSubsampleRatio410;
        break;
    }
    default: {
        throw panic("unreachable");
        break;
    }}

    var m = image.NewYCbCr(image.Rect(0, 0, 8 * h0 * mxx, 8 * v0 * myy), subsampleRatio);
    d.img3 = m.SubImage(image.Rect(0, 0, d.width, d.height))._<ж<image.YCbCr>>();
    if (d.nComp == 4) {
        nint h3 = d.comp[3].h;
        nint v3 = d.comp[3].v;
        d.blackPix = new slice<byte>(8 * h3 * mxx * 8 * v3 * myy);
        d.blackStride = 8 * h3 * mxx;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sosHasWrongLengthˢ = "SOS has wrong length"u8;
internal static readonly @string sosLengthInconsistentˢ = "SOS length inconsistent with number of components"u8;
internal static readonly @string unknownComponentSelectorˢ = "unknown component selector"u8;
internal static readonly @string repeatedComponentˢ2 = "repeated component selector"u8;
internal static readonly @string badTdValueˢ = "bad Td value"u8;
internal static readonly @string badTaValueˢ = "bad Ta value"u8;
internal static readonly @string totalSamplingFactorsTooˢ = "total sampling factors too large"u8;
internal static readonly @string badSpectralSelectionˢ = "bad spectral selection bounds"u8;
internal static readonly @string progressiveAcˢ = "progressive AC coefficients for more than one component"u8;
internal static readonly @string badSuccessiveˢ = "bad successive approximation values"u8;
internal static readonly @string excessiveDcComponentˢ = "excessive DC component"u8;

[GoType("dyn")] internal partial struct processSOS_scan {
    internal uint8 compIndex;
    internal uint8 td; // DC table selector.
    internal uint8 ta; // AC table selector.
}

// Specified in section B.2.3.
[GoRecv] internal static error processSOS(this ref decoder d, nint n) {
    if (d.nComp == 0) {
        return ((FormatError)(@string)missingSofMarkerˢ);
    }
    if (n < 6 || 4 + 2 * d.nComp < n || n % 2 != 0) {
        return ((FormatError)(@string)sosHasWrongLengthˢ);
    }
    {
        var err = d.readFull(d.tmp.slice(0, n)); if (err != default!) {
            return err;
        }
    }
    nint nComp = (nint)d.tmp[0];
    if (n != 4 + 2 * nComp) {
        return ((FormatError)(@string)sosLengthInconsistentˢ);
    }
    array<processSOS_scan> scan = new(4); /* maxComponents */
    nint totalHV = 0;
    for (nint i = 0; i < nComp; i++) {
        var cs = d.tmp[1 + 2 * i]; // Component selector.
        nint compIndex = -1;
        foreach (var (j, comp) in d.comp.slice(0, d.nComp)) {
            if (cs == comp.c) {
                compIndex = j;
            }
        }
        if (compIndex < 0) {
            return ((FormatError)(@string)unknownComponentSelectorˢ);
        }
        scan[i].compIndex = (uint8)compIndex;
        // Section B.2.3 states that "the value of Cs_j shall be different from
        // the values of Cs_1 through Cs_(j-1)". Since we have previously
        // verified that a frame's component identifiers (C_i values in section
        // B.2.2) are unique, it suffices to check that the implicit indexes
        // into d.comp are unique.
        for (nint j = 0; j < i; j++) {
            if (scan[i].compIndex == scan[j].compIndex) {
                return ((FormatError)(@string)repeatedComponentˢ2);
            }
        }
        totalHV += d.comp[compIndex].h * d.comp[compIndex].v;
        // The baseline t <= 1 restriction is specified in table B.3.
        scan[i].td = (uint8)((d.tmp[2 + 2 * i] >> (int)(4)));
        {
            var t = scan[i].td; if (t > maxTh || (d.baseline && t > 1)) {
                return ((FormatError)(@string)badTdValueˢ);
            }
        }
        scan[i].ta = (uint8)((byte)(d.tmp[2 + 2 * i] & 0x0f));
        {
            var t = scan[i].ta; if (t > maxTh || (d.baseline && t > 1)) {
                return ((FormatError)(@string)badTaValueˢ);
            }
        }
    }
    // Section B.2.3 states that if there is more than one component then the
    // total H*V values in a scan must be <= 10.
    if (d.nComp > 1 && totalHV > 10) {
        return ((FormatError)(@string)totalSamplingFactorsTooˢ);
    }
    // zigStart and zigEnd are the spectral selection bounds.
    // ah and al are the successive approximation high and low values.
    // The spec calls these values Ss, Se, Ah and Al.
    //
    // For progressive JPEGs, these are the two more-or-less independent
    // aspects of progression. Spectral selection progression is when not
    // all of a block's 64 DCT coefficients are transmitted in one pass.
    // For example, three passes could transmit coefficient 0 (the DC
    // component), coefficients 1-5, and coefficients 6-63, in zig-zag
    // order. Successive approximation is when not all of the bits of a
    // band of coefficients are transmitted in one pass. For example,
    // three passes could transmit the 6 most significant bits, followed
    // by the second-least significant bit, followed by the least
    // significant bit.
    //
    // For sequential JPEGs, these parameters are hard-coded to 0/63/0/0, as
    // per table B.3.
    var (zigStart, zigEnd, ah, al) = ((int32)0, (int32)(blockSize - 1), (uint32)0, (uint32)0);
    if (d.progressive) {
        zigStart = (int32)d.tmp[1 + 2 * nComp];
        zigEnd = (int32)d.tmp[2 + 2 * nComp];
        ah = (uint32)((d.tmp[3 + 2 * nComp] >> (int)(4)));
        al = (uint32)((byte)(d.tmp[3 + 2 * nComp] & 0x0f));
        if ((zigStart == 0 && zigEnd != 0) || zigStart > zigEnd || blockSize <= zigEnd) {
            return ((FormatError)(@string)badSpectralSelectionˢ);
        }
        if (zigStart != 0 && nComp != 1) {
            return ((FormatError)(@string)progressiveAcˢ);
        }
        if (ah != 0 && ah != al + 1) {
            return ((FormatError)(@string)badSuccessiveˢ);
        }
    }
    // mxx and myy are the number of MCUs (Minimum Coded Units) in the image.
    nint h0 = d.comp[0].h;
    nint v0 = d.comp[0].v; // The h and v values from the Y components.
    nint mxx = quo((d.width + 8 * h0 - 1), (8 * h0));
    nint myy = quo((d.height + 8 * v0 - 1), (8 * v0));
    if (d.img1 == nil && d.img3 == nil) {
        d.makeImg(mxx, myy);
    }
    if (d.progressive) {
        for (nint i = 0; i < nComp; i++) {
            var compIndex = scan[i].compIndex;
            if (d.progCoeffs[compIndex] == default!) {
                d.progCoeffs[compIndex] = GoReflect.WithElemDims(new slice<block>(mxx * myy * d.comp[compIndex].h * d.comp[compIndex].v), 64);
            }
        }
    }
    d.bits = new bits(nil);
    nint mcu = 0;
    var expectedRST = (uint8)rst0Marker;
    ref var b = ref heap(new block(), out var Ꮡb);
    array<int32> dc = new(4); /* maxComponents */
    nint bx = default!;
    nint by = default!;
    nint blockCount = default!;
    for (nint my = 0; my < myy; my++) {
        for (nint mx = 0; mx < mxx; mx++) {
            for (nint i = 0; i < nComp; i++) {
                var compIndex = scan[i].compIndex;
                nint hi = d.comp[compIndex].h;
                nint vi = d.comp[compIndex].v;
                for (nint j = 0; j < hi * vi; j++) {
                    // The blocks are traversed one MCU at a time. For 4:2:0 chroma
                    // subsampling, there are four Y 8x8 blocks in every 16x16 MCU.
                    //
                    // For a sequential 32x16 pixel image, the Y blocks visiting order is:
                    //	0 1 4 5
                    //	2 3 6 7
                    //
                    // For progressive images, the interleaved scans (those with nComp > 1)
                    // are traversed as above, but non-interleaved scans are traversed left
                    // to right, top to bottom:
                    //	0 1 2 3
                    //	4 5 6 7
                    // Only DC scans (zigStart == 0) can be interleaved. AC scans must have
                    // only one component.
                    //
                    // To further complicate matters, for non-interleaved scans, there is no
                    // data for any blocks that are inside the image at the MCU level but
                    // outside the image at the pixel level. For example, a 24x16 pixel 4:2:0
                    // progressive image consists of two 16x16 MCUs. The interleaved scans
                    // will process 8 Y blocks:
                    //	0 1 4 5
                    //	2 3 6 7
                    // The non-interleaved scans will process only 6 Y blocks:
                    //	0 1 2
                    //	3 4 5
                    if (nComp != 1){
                        bx = hi * mx + rem(j, hi);
                        by = vi * my + quo(j, hi);
                    } else {
                        nint q = mxx * hi;
                        bx = rem(blockCount, q);
                        by = quo(blockCount, q);
                        blockCount++;
                        if (bx * 8 >= d.width || by * 8 >= d.height) {
                            continue;
                        }
                    }
                    // Load the previous partially decoded coefficients, if applicable.
                    if (d.progressive){
                        b = d.progCoeffs[compIndex][by * mxx * hi + bx].Clone();
                    } else {
                        b = new block(new int32[64].array());
                    }
                    if (ah != 0){
                        {
                            var err = d.refine(Ꮡb, Ꮡ(d.huff[acTable], scan[i].ta), zigStart, zigEnd, ((int32)1).Lsh((uint64)(al))); if (err != default!) {
                                return err;
                            }
                        }
                    } else {
                        var zig = zigStart;
                        if (zig == 0) {
                            zig++;
                            // Decode the DC coefficient, as specified in section F.2.2.1.
                            var (value, err) = d.decodeHuffman(Ꮡ(d.huff[dcTable], scan[i].td));
                            if (err != default!) {
                                return err;
                            }
                            if (value > 16) {
                                return ((UnsupportedError)(@string)excessiveDcComponentˢ);
                            }
                            (var dcDelta, err) = d.receiveExtend(value);
                            if (err != default!) {
                                return err;
                            }
                            dc[compIndex] += dcDelta;
                            b[0] = dc[compIndex].Lsh((uint64)(al));
                        }
                        if (zig <= zigEnd && d.eobRun > 0){
                            d.eobRun--;
                        } else {
                            // Decode the AC coefficients, as specified in section F.2.2.2.
                            var huff = Ꮡ(d.huff[acTable], scan[i].ta);
                            for (; zig <= zigEnd; zig++) {
                                var (value, err) = d.decodeHuffman(huff);
                                if (err != default!) {
                                    return err;
                                }
                                var val0 = (uint8)((value >> (int)(4)));
                                var val1 = (uint8)(value & 0x0f);
                                if (val1 != 0){
                                    zig += (int32)val0;
                                    if (zig > zigEnd) {
                                        break;
                                    }
                                    var (ac, errΔ1) = d.receiveExtend(val1);
                                    if (errΔ1 != default!) {
                                        return errΔ1;
                                    }
                                    b[unzig[zig]] = ac.Lsh((uint64)(al));
                                } else {
                                    if (val0 != 0x0f) {
                                        d.eobRun = (uint16)(((uint16)1).Lsh((uint64)(val0)));
                                        if (val0 != 0) {
                                            var (bits, errΔ2) = d.decodeBits((int32)val0);
                                            if (errΔ2 != default!) {
                                                return errΔ2;
                                            }
                                            d.eobRun |= (uint16)((uint16)bits);
                                        }
                                        d.eobRun--;
                                        break;
                                    }
                                    zig += 0x0f;
                                }
                            }
                        }
                    }
                    if (d.progressive) {
                        // Save the coefficients.
                        d.progCoeffs[compIndex][by * mxx * hi + bx] = b.Clone();
                        // At this point, we could call reconstructBlock to dequantize and perform the
                        // inverse DCT, to save early stages of a progressive image to the *image.YCbCr
                        // buffers (the whole point of progressive encoding), but in Go, the jpeg.Decode
                        // function does not return until the entire image is decoded, so we "continue"
                        // here to avoid wasted computation. Instead, reconstructBlock is called on each
                        // accumulated block by the reconstructProgressiveImage method after all of the
                        // SOS markers are processed.
                        continue;
                    }
                    {
                        var err = d.reconstructBlock(Ꮡb, bx, by, (nint)compIndex); if (err != default!) {
                            return err;
                        }
                    }
                } // for j
            } // for i
            mcu++;
            if (d.ri > 0 && rem(mcu, d.ri) == 0 && mcu < mxx * myy) {
                // For well-formed input, the RST[0-7] restart marker follows
                // immediately. For corrupt input, call findRST to try to
                // resynchronize.
                {
                    var err = d.readFull(d.tmp[..2]); if (err != default!){
                        return err;
                    } else 
                    if (d.tmp[0] != 0xff || d.tmp[1] != expectedRST) {
                        {
                            var errΔ1 = d.findRST(expectedRST); if (errΔ1 != default!) {
                                return errΔ1;
                            }
                        }
                    }
                }
                expectedRST++;
                if (expectedRST == (uint8)(rst7Marker + 1)) {
                    expectedRST = rst0Marker;
                }
                // Reset the Huffman decoder.
                d.bits = new bits(nil);
                // Reset the DC components, as per section F.2.1.3.1.
                dc = new int32[]{}.array(4);
                // Reset the progressive decoder state, as per section G.1.2.2.
                d.eobRun = 0;
            }
        } // for mx
    } // for my
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unexpectedHuffmanCodeˢ = "unexpected Huffman code"u8;
internal static readonly @string tooManyCoefficientsˢ = "too many coefficients"u8;

// refine decodes a successive approximation refinement block, as specified in
// section G.1.2.
[GoRecv] internal static error refine(this ref decoder d, ж<block> Ꮡb, ж<huffman> Ꮡh, int32 zigStart, int32 zigEnd, int32 delta) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Refining a DC component is trivial.
    if (zigStart == 0) {
        if (zigEnd != 0) {
            throw panic("unreachable");
        }
        var (bit, err) = d.decodeBit();
        if (err != default!) {
            return err;
        }
        if (bit) {
            b[0] |= (int32)(delta);
        }
        return default!;
    }
    // Refining AC components is more complicated; see sections G.1.2.2 and G.1.2.3.
    var zig = zigStart;
    if (d.eobRun == 0) {
loop:
        for (; zig <= zigEnd; zig++) {
            var z = (int32)0;
            var (value, err) = d.decodeHuffman(Ꮡh);
            if (err != default!) {
                return err;
            }
            var val0 = (uint8)((value >> (int)(4)));
            var val1 = (uint8)(value & 0x0f);
            switch (val1) {
            case 0: {
                if (val0 != 0x0f) {
                    d.eobRun = (uint16)(((uint16)1).Lsh((uint64)(val0)));
                    if (val0 != 0) {
                        var (bits, errΔ3) = d.decodeBits((int32)val0);
                        if (errΔ3 != default!) {
                            return errΔ3;
                        }
                        d.eobRun |= (uint16)((uint16)bits);
                    }
                    goto break_loop;
                }
                break;
            }
            case 1: {
                z = delta;
                var (bit, errΔ4) = d.decodeBit();
                if (errΔ4 != default!) {
                    return errΔ4;
                }
                if (!bit) {
                    z = -z;
                }
                break;
            }
            default: {
                return ((FormatError)(@string)unexpectedHuffmanCodeˢ);
            }}

            (zig, err) = d.refineNonZeroes(Ꮡb, zig, zigEnd, (int32)val0, delta);
            if (err != default!) {
                return err;
            }
            if (zig > zigEnd) {
                return ((FormatError)(@string)tooManyCoefficientsˢ);
            }
            if (z != 0) {
                b[unzig[zig]] = z;
            }
continue_loop:;
        }
break_loop:;
    }
    if (d.eobRun > 0) {
        d.eobRun--;
        {
            var (_, err) = d.refineNonZeroes(Ꮡb, zig, zigEnd, -1, delta); if (err != default!) {
                return err;
            }
        }
    }
    return default!;
}

// refineNonZeroes refines non-zero entries of b in zig-zag order. If nz >= 0,
// the first nz zero entries are skipped over.
[GoRecv] internal static (int32, error) refineNonZeroes(this ref decoder d, ж<block> Ꮡb, int32 zig, int32 zigEnd, int32 nz, int32 delta) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (; zig <= zigEnd; zig++) {
        nint u = unzig[zig];
        if (b[u] == 0) {
            if (nz == 0) {
                break;
            }
            nz--;
            continue;
        }
        var (bit, err) = d.decodeBit();
        if (err != default!) {
            return (0, err);
        }
        if (!bit) {
            continue;
        }
        if (b[u] >= 0){
            b[u] += delta;
        } else {
            b[u] -= delta;
        }
    }
    return (zig, default!);
}

[GoRecv] internal static error reconstructProgressiveImage(this ref decoder d) {
    // The h0, mxx, by and bx variables have the same meaning as in the
    // processSOS method.
    nint h0 = d.comp[0].h;
    nint mxx = quo((d.width + 8 * h0 - 1), (8 * h0));
    for (nint i = 0; i < d.nComp; i++) {
        if (d.progCoeffs[i] == default!) {
            continue;
        }
        nint v = quo(8 * d.comp[0].v, d.comp[i].v);
        nint h = quo(8 * d.comp[0].h, d.comp[i].h);
        nint stride = mxx * d.comp[i].h;
        for (nint by = 0; by * v < d.height; by++) {
            for (nint bx = 0; bx * h < d.width; bx++) {
                {
                    var err = d.reconstructBlock(Ꮡ(d.progCoeffs[i], by * stride + bx), bx, by, i); if (err != default!) {
                        return err;
                    }
                }
            }
        }
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tooManyComponentsˢ = "too many components"u8;

// reconstructBlock dequantizes, performs the inverse DCT and stores the block
// to the image.
[GoRecv] internal static error reconstructBlock(this ref decoder d, ж<block> Ꮡb, nint bx, nint by, nint compIndex) {
    ref var b = ref Ꮡb.DerefOrNull();

    var qt = Ꮡ(d.quant, d.comp[compIndex].tq);
    for (nint zig = 0; zig < blockSize; zig++) {
        b[unzig[zig]] *= qt.Value[zig];
    }
    idct(Ꮡb);
    var dst = slice<byte>(default!);
    nint stride = 0;
    if (d.nComp == 1){
        (dst, stride) = ((~d.img1).Pix.slice(8 * (by * (~d.img1).Stride + bx)), d.img1.Value.Stride);
    } else {
        switch (compIndex) {
        case 0: {
            (dst, stride) = ((~d.img3).Y.slice(8 * (by * (~d.img3).YStride + bx)), d.img3.Value.YStride);
            break;
        }
        case 1: {
            (dst, stride) = ((~d.img3).Cb.slice(8 * (by * (~d.img3).CStride + bx)), d.img3.Value.CStride);
            break;
        }
        case 2: {
            (dst, stride) = ((~d.img3).Cr.slice(8 * (by * (~d.img3).CStride + bx)), d.img3.Value.CStride);
            break;
        }
        case 3: {
            (dst, stride) = (d.blackPix.slice(8 * (by * d.blackStride + bx)), d.blackStride);
            break;
        }
        default: {
            return ((UnsupportedError)(@string)tooManyComponentsˢ);
        }}

    }
    // Level shift by +128, clip to [0, 255], and write to dst.
    for (nint y = 0; y < 8; y++) {
        nint y8 = y * 8;
        nint yStride = y * stride;
        for (nint x = 0; x < 8; x++) {
            var c = b[y8 + x];
            if (c < -128){
                c = 0;
            } else 
            if (c > 127){
                c = 255;
            } else {
                c += 128;
            }
            dst[yStride + x] = (uint8)c;
        }
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badRstMarkerˢ = "bad RST marker"u8;

// findRST advances past the next RST restart marker that matches expectedRST.
// Other than I/O errors, it is also an error if we encounter an {0xFF, M}
// two-byte marker sequence where M is not 0x00, 0xFF or the expectedRST.
//
// This is similar to libjpeg's jdmarker.c's next_marker function.
// https://github.com/libjpeg-turbo/libjpeg-turbo/blob/2dfe6c0fe9e18671105e94f7cbf044d4a1d157e6/jdmarker.c#L892-L935
//
// Precondition: d.tmp[:2] holds the next two bytes of JPEG-encoded input
// (input in the d.readFull sense).
[GoRecv] internal static error findRST(this ref decoder d, uint8 expectedRST) {
    while (ᐧ) {
        // i is the index such that, at the bottom of the loop, we read 2-i
        // bytes into d.tmp[i:2], maintaining the invariant that d.tmp[:2]
        // holds the next two bytes of JPEG-encoded input. It is either 0 or 1,
        // so that each iteration advances by 1 or 2 bytes (or returns).
        nint i = 0;
        if (d.tmp[0] == 0xff){
            if (d.tmp[1] == expectedRST){
                return default!;
            } else 
            if (d.tmp[1] == 0xff){
                i = 1;
            } else 
            if (d.tmp[1] != 0x00) {
                // libjpeg's jdmarker.c's jpeg_resync_to_restart does something
                // fancy here, treating RST markers within two (modulo 8) of
                // expectedRST differently from RST markers that are 'more
                // distant'. Until we see evidence that recovering from such
                // cases is frequent enough to be worth the complexity, we take
                // a simpler approach for now. Any marker that's not 0x00, 0xff
                // or expectedRST is a fatal FormatError.
                return ((FormatError)(@string)badRstMarkerˢ);
            }
        } else 
        if (d.tmp[1] == 0xff) {
            d.tmp[0] = 0xff;
            i = 1;
        }
        {
            var err = d.readFull(d.tmp.slice(i, 2)); if (err != default!) {
                return err;
            }
        }
    }
}

} // end jpeg_package
