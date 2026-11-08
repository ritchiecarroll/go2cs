// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This test tests some internals of the flate package.
// The tests in package compress/gzip serve as the
// end-to-end test of the decompressor.
namespace go.compress;

using bytes = bytes_package;
using hex = encoding.hex_package;
using io = io_package;
using strings = strings_package;
using testing = testing_package;
using encoding;
using static go.compress.flate_package;

partial class flate_internal_test_package {

// The following test should not panic.
public static void TestIssue5915(ж<testing.T> t) {
    var bits = slice<nint>([4, 0, 0, 6, 4, 3, 2, 3, 3, 4, 4, 5, 0, 0, 0, 0, 5, 5, 6,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 7, 8, 6, 0, 11, 0, 8, 0, 6, 6, 10, 8]);
    global::go.compress.flate_package.huffmanDecoder h = new();
    if (h.init(bits)) {
        t.Fatalf("Given sequence of bits is bad, and should not succeed.");
    }
}

// The following test should not panic.
public static void TestIssue5962(ж<testing.T> t) {
    var bits = slice<nint>([4, 0, 0, 6, 4, 3, 2, 3, 3, 4, 4, 5, 0, 0, 0, 0,
        5, 5, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11]);
    global::go.compress.flate_package.huffmanDecoder h = new();
    if (h.init(bits)) {
        t.Fatalf("Given sequence of bits is bad, and should not succeed.");
    }
}

// The following test should not panic.
public static void TestIssue6255(ж<testing.T> t) {
    var bits1 = slice<nint>([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 11]);
    var bits2 = slice<nint>([11, 13]);
    global::go.compress.flate_package.huffmanDecoder h = new();
    if (!h.init(bits1)) {
        t.Fatalf("Given sequence of bits is good and should succeed.");
    }
    if (h.init(bits2)) {
        t.Fatalf("Given sequence of bits is bad and should not succeed.");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object failedToInitializeˢ = (@string)"Failed to initialize Huffman decoder"u8;
internal static readonly object shouldHaveRejectedˢ = (@string)"Should have rejected invalid bit sequence"u8;

public static void TestInvalidEncoding(ж<testing.T> t) {
    // Initialize Huffman decoder to recognize "0".
    ref var h = ref heap(new global::go.compress.flate_package.huffmanDecoder(), out var Ꮡh);
    if (!h.init([1])) {
        t.Fatal(failedToInitializeˢ);
    }

    // Initialize decompressor with invalid Huffman coding.
    global::go.compress.flate_package.decompressor f = new();
    f.r = new flate_test_package.bytes_Readerжflate_Reader(bytes.NewReader([0xff]));

    var (_, err) = f.huffSym(Ꮡh);
    if (err == nil) {
        t.Fatal(shouldHaveRejectedˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object shouldRejectˢ = (@string)"Should reject oversubscribed bit-length set"u8;
internal static readonly object shouldRejectIncompleteˢ = (@string)"Should reject incomplete bit-length set"u8;

public static void TestInvalidBits(ж<testing.T> t) {
    var oversubscribed = slice<nint>([1, 2, 3, 4, 4, 5]);
    var incomplete = slice<nint>([1, 2, 4, 4]);
    global::go.compress.flate_package.huffmanDecoder h = new();
    if (h.init(oversubscribed)) {
        t.Fatal(shouldRejectˢ);
    }
    if (h.init(incomplete)) {
        t.Fatal(shouldRejectIncompleteˢ);
    }
}

internal partial struct TestStreams_testCases /*dyn*/ {
    internal @string desc;   // Description of the stream
    internal @string stream; // Hexstring of the input DEFLATE stream
    internal @string want;   // Expected result. Use "fail" to expect failure
}

public static void TestStreams(ж<testing.T> t) {
    // To verify any of these hexstrings as valid or invalid flate streams
    // according to the C zlib library, you can use the Python wrapper library:
    // >>> hex_string = "010100feff11"
    // >>> import zlib
    // >>> zlib.decompress(hex_string.decode("hex"), -15) # Negative means raw DEFLATE
    // '\x11'

    var testCases = slice<TestStreams_testCases>([new(
        "degenerate HCLenTree",
        "05e0010000000000100000000000000000000000000000000000000000000000"u8 + "00000000000000000004"u8,
        "fail"
    ), new(
        "complete HCLenTree, empty HLitTree, empty HDistTree",
        "05e0010400000000000000000000000000000000000000000000000000000000"u8 + "00000000000000000010"u8,
        "fail"
    ), new(
        "empty HCLenTree",
        "05e0010000000000000000000000000000000000000000000000000000000000"u8 + "00000000000000000010"u8,
        "fail"
    ), new(
        "complete HCLenTree, complete HLitTree, empty HDistTree, use missing HDist symbol",
        "000100feff000de0010400000000100000000000000000000000000000000000"u8 + "0000000000000000000000000000002c"u8,
        "fail"
    ), new(
        "complete HCLenTree, complete HLitTree, degenerate HDistTree, use missing HDist symbol",
        "000100feff000de0010000000000000000000000000000000000000000000000"u8 + "00000000000000000610000000004070"u8,
        "fail"
    ), new(
        "complete HCLenTree, empty HLitTree, empty HDistTree",
        "05e0010400000000100400000000000000000000000000000000000000000000"u8 + "0000000000000000000000000008"u8,
        "fail"
    ), new(
        "complete HCLenTree, empty HLitTree, degenerate HDistTree",
        "05e0010400000000100400000000000000000000000000000000000000000000"u8 + "0000000000000000000800000008"u8,
        "fail"
    ), new(
        "complete HCLenTree, degenerate HLitTree, degenerate HDistTree, use missing HLit symbol",
        "05e0010400000000100000000000000000000000000000000000000000000000"u8 + "0000000000000000001c"u8,
        "fail"
    ), new(
        "complete HCLenTree, complete HLitTree, too large HDistTree",
        "edff870500000000200400000000000000000000000000000000000000000000"u8 + "000000000000000000080000000000000004"u8,
        "fail"
    ), new(
        "complete HCLenTree, complete HLitTree, empty HDistTree, excessive repeater code",
        "edfd870500000000200400000000000000000000000000000000000000000000"u8 + "000000000000000000e8b100"u8,
        "fail"
    ), new(
        "complete HCLenTree, complete HLitTree, empty HDistTree of normal length 30",
        "05fd01240000000000f8ffffffffffffffffffffffffffffffffffffffffffff"u8 + "ffffffffffffffffff07000000fe01"u8,
        ""
    ), new(
        "complete HCLenTree, complete HLitTree, empty HDistTree of excessive length 31",
        "05fe01240000000000f8ffffffffffffffffffffffffffffffffffffffffffff"u8 + "ffffffffffffffffff07000000fc03"u8,
        "fail"
    ), new(
        "complete HCLenTree, over-subscribed HLitTree, empty HDistTree",
        "05e001240000000000fcffffffffffffffffffffffffffffffffffffffffffff"u8 + "ffffffffffffffffff07f00f"u8,
        "fail"
    ), new(
        "complete HCLenTree, under-subscribed HLitTree, empty HDistTree",
        "05e001240000000000fcffffffffffffffffffffffffffffffffffffffffffff"u8 + "fffffffffcffffffff07f00f"u8,
        "fail"
    ), new(
        "complete HCLenTree, complete HLitTree with single code, empty HDistTree",
        "05e001240000000000f8ffffffffffffffffffffffffffffffffffffffffffff"u8 + "ffffffffffffffffff07f00f"u8,
        "01"
    ), new(
        "complete HCLenTree, complete HLitTree with multiple codes, empty HDistTree",
        "05e301240000000000f8ffffffffffffffffffffffffffffffffffffffffffff"u8 + "ffffffffffffffffff07807f"u8,
        "01"
    ), new(
        "complete HCLenTree, complete HLitTree, degenerate HDistTree, use valid HDist symbol",
        "000100feff000de0010400000000100000000000000000000000000000000000"u8 + "0000000000000000000000000000003c"u8,
        "00000000"
    ), new(
        "complete HCLenTree, degenerate HLitTree, degenerate HDistTree",
        "05e0010400000000100000000000000000000000000000000000000000000000"u8 + "0000000000000000000c"u8,
        ""
    ), new(
        "complete HCLenTree, degenerate HLitTree, empty HDistTree",
        "05e0010400000000100000000000000000000000000000000000000000000000"u8 + "00000000000000000004"u8,
        ""
    ), new(
        "complete HCLenTree, complete HLitTree, empty HDistTree, spanning repeater code",
        "edfd870500000000200400000000000000000000000000000000000000000000"u8 + "000000000000000000e8b000"u8,
        ""
    ), new(
        "complete HCLenTree with length codes, complete HLitTree, empty HDistTree",
        "ede0010400000000100000000000000000000000000000000000000000000000"u8 + "0000000000000000000400004000"u8,
        ""
    ), new(
        "complete HCLenTree, complete HLitTree, degenerate HDistTree, use valid HLit symbol 284 with count 31",
        "000100feff00ede0010400000000100000000000000000000000000000000000"u8 + "000000000000000000000000000000040000407f00"u8,
        "0000000000000000000000000000000000000000000000000000000000000000"u8 + "0000000000000000000000000000000000000000000000000000000000000000"u8 + "0000000000000000000000000000000000000000000000000000000000000000"u8 + "0000000000000000000000000000000000000000000000000000000000000000"u8 + "0000000000000000000000000000000000000000000000000000000000000000"u8 + "0000000000000000000000000000000000000000000000000000000000000000"u8 + "0000000000000000000000000000000000000000000000000000000000000000"u8 + "0000000000000000000000000000000000000000000000000000000000000000"u8 + "000000"u8
    ), new(
        "complete HCLenTree, complete HLitTree, degenerate HDistTree, use valid HLit and HDist symbols",
        "0cc2010d00000082b0ac4aff0eb07d27060000ffff",
        "616263616263"
    ), new(
        "fixed block, use reserved symbol 287",
        "33180700",
        "fail"
    ), new(
        "raw block",
        "010100feff11",
        "11"
    ), new(
        "issue 10426 - over-subscribed HCLenTree causes a hang",
        "344c4a4e494d4b070000ff2e2eff2e2e2e2e2eff",
        "fail"
    ), new(
        "issue 11030 - empty HDistTree unexpectedly leads to error",
        "05c0070600000080400fff37a0ca",
        ""
    ), new(
        "issue 11033 - empty HDistTree unexpectedly leads to error",
        "050fb109c020cca5d017dcbca044881ee1034ec149c8980bbc413c2ab35be9dc"u8 + "b1473449922449922411202306ee97b0383a521b4ffdcf3217f9f7d3adb701"u8,
        "3130303634342068652e706870005d05355f7ed957ff084a90925d19e3ebc6d0"u8 + "c6d7"u8
    )
    ]);

    foreach (var (i, tc) in range(testCases)) {
        var (data, err) = hex.DecodeString(tc.stream);
        if (err != nil) {
            t.Fatal(err);
        }
        (data, err) = io.ReadAll(NewReader(new flate_test_package.bytes_Readerжio_Reader(bytes.NewReader(data))));
        if (tc.want == "fail"u8) {
            if (err == nil) {
                t.Errorf("#%d (%s): got nil error, want non-nil", i, tc.desc);
            }
        } else {
            if (err != nil) {
                t.Errorf("#%d (%s): %v", i, tc.desc, err);
                continue;
            }
            @string got = hex.EncodeToString(data); if (got != tc.want) {
                t.Errorf("#%d (%s):\ngot  %q\nwant %q", i, tc.desc, got, tc.want);
            }
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string dataᶜ => ref dataᶜᵇ;
private static readonly @string dataᶜᵇ = ((@string)(new byte[]{0x00, 0x0c, 0x00, 0xf3, 0xff, 0x68, 0x65, 0x6c, 0x6c, 0x6f, 0x2c, 0x20, 0x77, 0x6f, 0x72, 0x6c, 0x64, 0x01, 0x00, 0x00, 0xff, 0xff}));

public static void TestTruncatedStreams(ж<testing.T> t) {
    @string data = dataᶜ;

    for (nint i = 0; i < len(data) - 1; i++) {
        var r = NewReader(new flate_test_package.strings_ReaderжReader(strings.NewReader(data.slice(0, i))));
        var (_, err) = io.Copy(io.Discard, r);
        if (!AreEqual(err, io.ErrUnexpectedEOF)) {
            t.Errorf("io.Copy(%d) on truncated stream: got %v, want %v", i, err, io.ErrUnexpectedEOF);
        }
    }
}

// Verify that flate.Reader.Read returns (n, io.EOF) instead
// of (n, nil) + (0, io.EOF) when possible.
//
// This helps net/http.Transport reuse HTTP/1 connections more
// aggressively.
//
// See https://github.com/google/go-github/pull/317 for background.
public static void TestReaderEarlyEOF(ж<testing.T> t) {
    t.Parallel();
    var testSizes = slice<nint>([
        1, 2, 3, 4, 5, 6, 7, 8,
        100, 1000, 10000, 100000,
        128, 1024, 16384, 131072,

        // Testing multiples of windowSize triggers the case
        // where Read will fail to return an early io.EOF.
        windowSize * 1, windowSize * 2, windowSize * 3
    ]);

    nint maxSize = default;
    foreach (var (_, n) in range(testSizes)) {
        if (maxSize < n) {
            maxSize = n;
        }
    }

    var readBuf = new slice<byte>(40);
    var data = new slice<byte>(maxSize);
    foreach (var i in range(len(data))) {
        data[i] = (byte)i;
    }

    foreach (var (_, sz) in range(testSizes)) {
        if (testing.Short() && sz > windowSize) {
            continue;
        }
        foreach (var (_, flush) in range(slice<bool>([true, false]))) {
            var earlyEOF = true; // Do we expect early io.EOF?

            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            var (w, _) = NewWriter(new flate_test_package.bytes_BufferжWriter(Ꮡbuf), 5);
            w.Write(data.slice(0, sz));
            if (flush) {
                // If a Flush occurs after all the actual data, the flushing
                // semantics dictate that we will observe a (0, io.EOF) since
                // Read must return data before it knows that the stream ended.
                w.Flush();
                earlyEOF = false;
            }
            w.Close();

            var r = NewReader(new flate_test_package.bytes_BufferжReader(Ꮡbuf));
            for (;;) {
                var (n, err) = r.Read(readBuf);
                if (AreEqual(err, io.EOF)) {
                    // If the availWrite == windowSize, then that means that the
                    // previous Read returned because the write buffer was full
                    // and it just so happened that the stream had no more data.
                    // This situation is rare, but unavoidable.
                    if (r._<ж<global::go.compress.flate_package.decompressor>>().Ꮡdict.availWrite() == windowSize) {
                        earlyEOF = false;
                    }

                    if (n == 0 && earlyEOF) {
                        t.Errorf("On size:%d flush:%v, Read() = (0, io.EOF), want (n, io.EOF)", sz, flush);
                    }
                    if (n != 0 && !earlyEOF) {
                        t.Errorf("On size:%d flush:%v, Read() = (%d, io.EOF), want (0, io.EOF)", sz, flush, n);
                    }
                    break;
                }
                if (err != nil) {
                    t.Fatal(err);
                }
            }
        }
    }
}

} // end flate_internal_test_package
