// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

using bytes = bytes_package;
using strings = strings_package;
using testing = testing_package;
using static go.compress.flate_package;

partial class flate_internal_test_package {

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string abcᶜ1 = "ABC\n"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string foxᶜ = "The quick brown fox jumped over the lazy dog!\n"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string poemᶜ = "The Road Not Taken\nRobert Frost\n\nTwo roads diverged in a yellow wood,\nAnd sorry I could not travel both\nAnd be one traveler, long I stood\nAnd looked down one as far as I could\nTo where it bent in the undergrowth;\n\nThen took the other, as just as fair,\nAnd having perhaps the better claim,\nBecause it was grassy and wanted wear;\nThough as for that the passing there\nHad worn them really about the same,\n\nAnd both that morning equally lay\nIn leaves no step had trodden black.\nOh, I kept the first for another day!\nYet knowing how way leads on to way,\nI doubted if I should ever come back.\n\nI shall be telling this with a sigh\nSomewhere ages and ages hence:\nTwo roads diverged in a wood, and I-\nI took the one less traveled by,\nAnd that has made all the difference.\n";

internal partial struct TestDictDecoder_type /*dyn*/ {
    internal nint dist; // Backward distance (0 if this is an insertion)
    internal nint length; // Length of copy or insertion
}

public static void TestDictDecoder(ж<testing.T> Ꮡt) {
    @string abc = abcᶜ1;
    @string fox = foxᶜ;
    @string poem = poemᶜ;
    slice<TestDictDecoder_type> poemRefs = new TestDictDecoder_type[]{
        new(0, 38), new(33, 3), new(0, 48), new(79, 3), new(0, 11), new(34, 5), new(0, 6), new(23, 7),
        new(0, 8), new(50, 3), new(0, 2), new(69, 3), new(34, 5), new(0, 4), new(97, 3), new(0, 4),
        new(43, 5), new(0, 6), new(7, 4), new(88, 7), new(0, 12), new(80, 3), new(0, 2), new(141, 4),
        new(0, 1), new(196, 3), new(0, 3), new(157, 3), new(0, 6), new(181, 3), new(0, 2), new(23, 3),
        new(77, 3), new(28, 5), new(128, 3), new(110, 4), new(70, 3), new(0, 4), new(85, 6), new(0, 2),
        new(182, 6), new(0, 4), new(133, 3), new(0, 7), new(47, 5), new(0, 20), new(112, 5), new(0, 1),
        new(58, 3), new(0, 8), new(59, 3), new(0, 4), new(173, 3), new(0, 5), new(114, 3), new(0, 4),
        new(92, 5), new(0, 2), new(71, 3), new(0, 2), new(76, 5), new(0, 1), new(46, 3), new(96, 4),
        new(130, 4), new(0, 3), new(360, 3), new(0, 3), new(178, 5), new(0, 7), new(75, 3), new(0, 3),
        new(45, 6), new(0, 6), new(299, 6), new(180, 3), new(70, 6), new(0, 1), new(48, 3), new(66, 4),
        new(0, 3), new(47, 5), new(0, 9), new(325, 3), new(0, 1), new(359, 3), new(318, 3), new(0, 2),
        new(199, 3), new(0, 1), new(344, 3), new(0, 3), new(248, 3), new(0, 10), new(310, 3), new(0, 3),
        new(93, 6), new(0, 3), new(252, 3), new(157, 4), new(0, 2), new(273, 5), new(0, 14), new(99, 4),
        new(0, 1), new(464, 4), new(0, 2), new(92, 4), new(495, 3), new(0, 1), new(322, 4), new(16, 4),
        new(0, 3), new(402, 3), new(0, 2), new(237, 4), new(0, 2), new(432, 4), new(0, 1), new(483, 5),
        new(0, 2), new(294, 4), new(0, 2), new(306, 3), new(113, 5), new(0, 1), new(26, 4), new(164, 3),
        new(488, 4), new(0, 1), new(542, 3), new(248, 6), new(0, 5), new(205, 3), new(0, 8), new(48, 3),
        new(449, 6), new(0, 2), new(192, 3), new(328, 4), new(9, 5), new(433, 3), new(0, 3), new(622, 25),
        new(615, 5), new(46, 5), new(0, 2), new(104, 3), new(475, 10), new(549, 3), new(0, 4), new(597, 8),
        new(314, 3), new(0, 1), new(473, 6), new(317, 5), new(0, 1), new(400, 3), new(0, 3), new(109, 3),
        new(151, 3), new(48, 4), new(0, 4), new(125, 3), new(108, 3), new(0, 2)
    }.slice();
    ref var got = ref heap(new bytes.Buffer(), out var Ꮡgot);
    ref var want = ref heap(new bytes.Buffer(), out var Ꮡwant);
    ref var dd = ref heap(new global::go.compress.flate_package.dictDecoder(), out var Ꮡdd);
    dd.init((1 << (int)(11)), default!);
    Action<nint, nint> writeCopy = (nint dist, nint length) => {
        while (length > 0) {
            nint cnt = Ꮡdd.Value.tryWriteCopy(dist, length);
            if (cnt == 0) {
                cnt = Ꮡdd.Value.writeCopy(dist, length);
            }
            length -= cnt;
            if (Ꮡdd.Value.availWrite() == 0) {
                Ꮡgot.Value.Write(Ꮡdd.Value.readFlush());
            }
        }
    };
    Action<@string> writeString = (@string strΔ1) => {
        while (len(strΔ1) > 0) {
            nint cnt = copy(Ꮡdd.Value.writeSlice(), strΔ1);
            strΔ1 = strΔ1.slice(cnt);
            Ꮡdd.Value.writeMark(cnt);
            if (Ꮡdd.Value.availWrite() == 0) {
                Ꮡgot.Value.Write(Ꮡdd.Value.readFlush());
            }
        }
    };
    writeString("."u8);
    want.WriteByte((rune)'.');
    @string str = poem;
    foreach (var (_, @ref) in poemRefs) {
        if (@ref.dist == 0){
            writeString(str.slice(0, @ref.length));
        } else {
            writeCopy(@ref.dist, @ref.length);
        }
        str = str.slice(@ref.length);
    }
    want.WriteString(poem);
    writeCopy(dd.histSize(), 33);
    want.Write(want.Bytes()[..33]);
    writeString(abc);
    writeCopy(len(abc), 59 * len(abc));
    want.WriteString(strings.Repeat(abc, 60));
    writeString(fox);
    writeCopy(len(fox), 9 * len(fox));
    want.WriteString(strings.Repeat(fox, 10));
    writeString("."u8);
    writeCopy(1, 9);
    want.WriteString(strings.Repeat("."u8, 10));
    writeString(strings.ToUpper(poem));
    writeCopy(len(poem), 7 * len(poem));
    want.WriteString(strings.Repeat(strings.ToUpper(poem), 8));
    writeCopy(dd.histSize(), 10);
    want.Write(want.Bytes().slice(want.Len() - dd.histSize())[..10]);
    got.Write(dd.readFlush());
    if (Ꮡgot.String() != Ꮡwant.String()) {
        Ꮡt.Errorf("final string mismatch:\ngot  %q\nwant %q"u8, Ꮡgot.String(), Ꮡwant.String());
    }
}

} // end flate_internal_test_package
