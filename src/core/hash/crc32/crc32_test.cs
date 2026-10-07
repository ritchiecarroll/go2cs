// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.hash;

using encoding = encoding_package;
using fmt = fmt_package;
using hash = hash_package;
using io = io_package;
using rand = math.rand_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using math;
using static go.hash.crc32_package;

partial class crc32_internal_test_package {

// First test, so that it can be the one to initialize castagnoliTable.
public static partial void TestCastagnoliRace(ж<testing.T> Ꮡt) {
    // The MakeTable(Castagnoli) lazily initializes castagnoliTable,
    // which races with the switch on tab during Write to check
    // whether tab == castagnoliTable.
    var ieee = NewIEEE();
    goǃ(ᴛ1 => MakeTable(ᴛ1), (uint32)(Castagnoli));
    ieee.Write(slice<byte>("hello"u8));
}

[GoType] internal partial struct test {
    internal uint32 ieee, castagnoli;
    internal @string @in;
    internal @string halfStateIEEE; // IEEE marshaled hash state after first half of in written, used by TestGoldenMarshal
    internal @string halfStateCastagnoli; // Castagnoli marshaled hash state after first half of in written, used by TestGoldenMarshal
}

internal static slice<test> golden = new test[]{
    new(0x0, 0x0, ""u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0x00, 0x00, 0x00, 0x00})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0x00, 0x00, 0x00, 0x00}))),
    new(0xe8b7be43U, 0xc1d04330U, "a"u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0x00, 0x00, 0x00, 0x00})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0x00, 0x00, 0x00, 0x00}))),
    new(0x9e83486dU, 0xe2a22936U, "ab"u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0xe8, 0xb7, 0xbe, 0x43})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0xc1, 0xd0, 0x43, 0x30}))),
    new(0x352441c2, 0x364b3fb7, "abc"u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0xe8, 0xb7, 0xbe, 0x43})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0xc1, 0xd0, 0x43, 0x30}))),
    new(0xed82cd11U, 0x92c80a31U, "abcd"u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0x9e, 0x83, 0x48, 0x6d})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0xe2, 0xa2, 0x29, 0x36}))),
    new(0x8587d865U, 0xc450d697U, "abcde"u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0x9e, 0x83, 0x48, 0x6d})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0xe2, 0xa2, 0x29, 0x36}))),
    new(0x4b8e39ef, 0x53bceff1, "abcdef"u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0x35, 0x24, 0x41, 0xc2})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0x36, 0x4b, 0x3f, 0xb7}))),
    new(0x312a6aa6, 0xe627f441U, "abcdefg"u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0x35, 0x24, 0x41, 0xc2})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0x36, 0x4b, 0x3f, 0xb7}))),
    new(0xaeef2a50U, 0xa9421b7, "abcdefgh"u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0xed, 0x82, 0xcd, 0x11})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0x92, 0xc8, 0x0a, 0x31}))),
    new(0x8da988afU, 0x2ddc99fc, "abcdefghi"u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0xed, 0x82, 0xcd, 0x11})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0x92, 0xc8, 0x0a, 0x31}))),
    new(0x3981703a, 0xe6599437U, "abcdefghij"u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0x85, 0x87, 0xd8, 0x65})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0xc4, 0x50, 0xd6, 0x97}))),
    new(0x6b9cdfe7, 0xb2cc01feU, "Discard medicine more than two years old."u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0xfd, 0xe5, 0xc2, 0x4a})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0x53, 0x22, 0x28, 0xe0}))),
    new(0xc90ef73fU, 0xe28207f, "He who has a shady past knows that nice guys finish last."u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0x01, 0xc7, 0x8b, 0x2b})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0x27, 0xda, 0x52, 0x15}))),
    new(0xb902341fU, 0xbe93f964U, "I wouldn't marry him with a ten foot pole."u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0x9d, 0x13, 0xce, 0x10})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0xc3, 0xed, 0xab, 0x47}))),
    new(0x42080e8, 0x9e3be0c3U, "Free! Free!/A trip/to Mars/for 900/empty jars/Burma Shave"u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0x2d, 0xed, 0xf7, 0x94})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0xce, 0xce, 0x62, 0x81}))),
    new(0x154c6d11, 0xf505ef04U, "The days of the digital watch are numbered.  -Tom Stoppard"u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0x4f, 0x61, 0xa5, 0x0d})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0xd3, 0x73, 0x9d, 0x50}))),
    new(0x4c418325, 0x85d3dc82U, "Nepal premier won't resign."u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0xa8, 0x53, 0x39, 0x85})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0x7b, 0x90, 0x8a, 0x14}))),
    new(0x33955150, 0xc5142380U, "For every action there is an equal and opposite government program."u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0x61, 0xe9, 0x3e, 0x86})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0xaa, 0x40, 0xc4, 0x1c}))),
    new(0x26216a4b, 0x75eb77dd, "His money is twice tainted: 'taint yours and 'taint mine."u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0x5c, 0x1a, 0x6e, 0x88})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0x57, 0x07, 0x38, 0x5a}))),
    new(0x1abbe45e, 0x91ebe9f7U, "There is no reason for any individual to have a computer in their home. -Ken Olsen, 1977"u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0xb7, 0xf5, 0xf2, 0xca})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0xc4, 0x6f, 0x9d, 0x85}))),
    new(0xc89a94f7U, 0xf0b1168eU, "It's a tiny change to the code and not completely disgusting. - Bob Manchek"u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0x84, 0x67, 0x31, 0xe8})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0x23, 0x98, 0x0c, 0xab}))),
    new(0xab3abe14U, 0x572b74e2, "size:  a.out:  bad magic"u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0x8a, 0x0f, 0xad, 0x08})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0x80, 0xc9, 0x6e, 0xd8}))),
    new(0xbab102b6U, 0x8a58a6d5U, "The major problem is with sendmail.  -Mark Horton"u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0x07, 0xf0, 0xb3, 0x15})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0x6c, 0x69, 0x53, 0xcc}))),
    new(0x999149d7U, 0x9c426c50U, "Give me a rock, paper and scissors and I will move the world.  CCFestoon"u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0x0f, 0x61, 0xbc, 0x2e})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0xdb, 0xcd, 0x8f, 0x43}))),
    new(0x6d52a33c, 0x735400a4, "If the enemy is within range, then so are you."u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0x79, 0x1b, 0x99, 0xf8})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0xaa, 0x42, 0x03, 0x37}))),
    new(0x90631e8dU, 0xbec49c95U, "It's well we cannot hear the screams/That we create in others' dreams."u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0x08, 0x71, 0x66, 0x59})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0x16, 0x79, 0xa1, 0xd2}))),
    new(0x78309130, 0xa95a2079U, "You remind me of a TV show, but that's all right: I watch it anyway."u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0xbd, 0x4f, 0x2c, 0xc2})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0x66, 0x26, 0xc5, 0xe4}))),
    new(0x7d0a377f, 0xde2e65c5U, "C is as portable as Stonehedge!!"u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0xf7, 0xd6, 0x00, 0xd5})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0x64, 0x65, 0x5c, 0xf8}))),
    new(0x8c79fd79U, 0x297a88ed, "Even if I could be Shakespeare, I think I should still choose to be Faraday. - A. Huxley"u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0x6c, 0x2b, 0xb8, 0xa7})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0xbf, 0xd6, 0x53, 0xdd}))),
    new(0xa20b7167U, 0x66ed1d8b, "The fugacity of a constituent in a mixture of gases at a given temperature is proportional to its mole fraction.  Lewis-Randall Rule"u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0x3c, 0x6c, 0x52, 0x5b})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0x7b, 0xac, 0x6f, 0xb1}))),
    new(0x8e0bb443U, 0xdcded527U, "How can you write a big system without C++?  -Paul Glick"u8, ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0xca, 0x87, 0x91, 0x4d, 0x0e, 0x88, 0x89, 0xed})), ((@string)(new byte[]{0x63, 0x72, 0x63, 0x01, 0x77, 0x42, 0x84, 0x81, 0x33, 0xd7, 0x43, 0x7f})))
}.slice();

// testGoldenIEEE verifies that the given function returns
// correct IEEE checksums.
internal static void testGoldenIEEE(ж<testing.T> Ꮡt, Func<slice<byte>, uint32> crcFunc) {
    foreach (var (_, g) in golden) {
        {
            var crc = crcFunc(slice<byte>(g.@in)); if (crc != g.ieee) {
                Ꮡt.Errorf("IEEE(%s) = 0x%x want 0x%x"u8, g.@in, crc, g.ieee);
            }
        }
    }
}

// testGoldenCastagnoli verifies that the given function returns
// correct IEEE checksums.
internal static void testGoldenCastagnoli(ж<testing.T> Ꮡt, Func<slice<byte>, uint32> crcFunc) {
    foreach (var (_, g) in golden) {
        {
            var crc = crcFunc(slice<byte>(g.@in)); if (crc != g.castagnoli) {
                Ꮡt.Errorf("Castagnoli(%s) = 0x%x want 0x%x"u8, g.@in, crc, g.castagnoli);
            }
        }
    }
}

// testCrossCheck generates random buffers of various lengths and verifies that
// the two "update" functions return the same result.
internal static void testCrossCheck(ж<testing.T> Ꮡt, Func<uint32, slice<byte>, uint32> crcFunc1, Func<uint32, slice<byte>, uint32> crcFunc2) {
    // The AMD64 implementation has some cutoffs at lengths 168*3=504 and
    // 1344*3=4032. We should make sure lengths around these values are in the
    // list.
    var lengths = new nint[]{0, 1, 2, 3, 4, 5, 10, 16, 50, 63, 64, 65, 100,
        127, 128, 129, 255, 256, 257, 300, 312, 384, 416, 448, 480,
        500, 501, 502, 503, 504, 505, 512, 513, 1000, 1024, 2000,
        4030, 4031, 4032, 4033, 4036, 4040, 4048, 4096, 5000, 10000}.slice();
    foreach (var (_, length) in lengths) {
        var p = new slice<byte>(length);
        (_, _) = rand.Read(p);
        var crcInit = (uint32)rand.Int63();
        var crc1 = crcFunc1(crcInit, p);
        var crc2 = crcFunc2(crcInit, p);
        if (crc1 != crc2) {
            Ꮡt.Errorf("mismatch: 0x%x vs 0x%x (buffer length %d)"u8, crc1, crc2, length);
        }
    }
}

// TestSimple tests the simple generic algorithm.
public static void TestSimple(ж<testing.T> Ꮡt) {
    ref var tab = ref heap<ж<global::go.hash.crc32_package.Table>>(out var Ꮡtab);
    tab = simpleMakeTable(IEEE);
    testGoldenIEEE(Ꮡt, (slice<byte> b) => simpleUpdate(0, ref (Ꮡtab.ValueSlot).DerefOrNull(), b));
    tab = simpleMakeTable(Castagnoli);
    testGoldenCastagnoli(Ꮡt, (slice<byte> b) => simpleUpdate(0, ref (Ꮡtab.ValueSlot).DerefOrNull(), b));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ieeeˢ = "IEEE"u8;
internal static readonly @string castagnoliˢ = "Castagnoli"u8;

public static void TestGoldenMarshal(ж<testing.T> Ꮡt) {
    Ꮡt.Run(ieeeˢ, (ж<testing.T> tΔ1) => {
        foreach (var (_, g) in golden) {
            var h = New(IEEETable);
            var h2 = New(IEEETable);
            io.WriteString(new crc32_test_package.hash_Hash32ᴠWriter(h), g.@in.slice(0, len(g.@in) / 2));
            var (state, err) = h._<encoding.BinaryMarshaler>().MarshalBinary();
            if (err != default!) {
                tΔ1.Errorf("could not marshal: %v"u8, err);
                continue;
            }
            (var stateAppend, err) = h._<encoding.BinaryAppender>().AppendBinary(new slice<byte>(4, 32));
            if (err != default!) {
                tΔ1.Errorf("could not marshal: %v"u8, err);
                continue;
            }
            stateAppend = stateAppend[4..];
            if (((sstring)state) != g.halfStateIEEE) {
                tΔ1.Errorf("IEEE(%q) state = %q, want %q"u8, g.@in, state, g.halfStateIEEE);
                continue;
            }
            if (((sstring)stateAppend) != g.halfStateIEEE) {
                tΔ1.Errorf("IEEE(%q) state = %q, want %q"u8, g.@in, stateAppend, g.halfStateIEEE);
                continue;
            }
            {
                var errΔ1 = h2._<encoding.BinaryUnmarshaler>().UnmarshalBinary(state); if (errΔ1 != default!) {
                    tΔ1.Errorf("could not unmarshal: %v"u8, errΔ1);
                    continue;
                }
            }
            io.WriteString(new crc32_test_package.hash_Hash32ᴠWriter(h), g.@in.slice(len(g.@in) / 2));
            io.WriteString(new crc32_test_package.hash_Hash32ᴠWriter(h2), g.@in.slice(len(g.@in) / 2));
            if (h.Sum32() != h2.Sum32()) {
                tΔ1.Errorf("IEEE(%s) = 0x%x != marshaled 0x%x"u8, g.@in, h.Sum32(), h2.Sum32());
            }
        }
    });
    Ꮡt.Run(castagnoliˢ, (ж<testing.T> tΔ2) => {
        var table = MakeTable(Castagnoli);
        foreach (var (_, g) in golden) {
            var h = New(table);
            var h2 = New(table);
            io.WriteString(new crc32_test_package.hash_Hash32ᴠWriter(h), g.@in.slice(0, len(g.@in) / 2));
            var (state, err) = h._<encoding.BinaryMarshaler>().MarshalBinary();
            if (err != default!) {
                tΔ2.Errorf("could not marshal: %v"u8, err);
                continue;
            }
            (var stateAppend, err) = h._<encoding.BinaryAppender>().AppendBinary(new slice<byte>(4, 32));
            if (err != default!) {
                tΔ2.Errorf("could not marshal: %v"u8, err);
                continue;
            }
            stateAppend = stateAppend[4..];
            if (((sstring)state) != g.halfStateCastagnoli) {
                tΔ2.Errorf("Castagnoli(%q) state = %q, want %q"u8, g.@in, state, g.halfStateCastagnoli);
                continue;
            }
            if (((sstring)stateAppend) != g.halfStateCastagnoli) {
                tΔ2.Errorf("Castagnoli(%q) state = %q, want %q"u8, g.@in, stateAppend, g.halfStateCastagnoli);
                continue;
            }
            {
                var errΔ1 = h2._<encoding.BinaryUnmarshaler>().UnmarshalBinary(state); if (errΔ1 != default!) {
                    tΔ2.Errorf("could not unmarshal: %v"u8, errΔ1);
                    continue;
                }
            }
            io.WriteString(new crc32_test_package.hash_Hash32ᴠWriter(h), g.@in.slice(len(g.@in) / 2));
            io.WriteString(new crc32_test_package.hash_Hash32ᴠWriter(h2), g.@in.slice(len(g.@in) / 2));
            if (h.Sum32() != h2.Sum32()) {
                tΔ2.Errorf("Castagnoli(%s) = 0x%x != marshaled 0x%x"u8, g.@in, h.Sum32(), h2.Sum32());
            }
        }
    });
}

public static void TestMarshalTableMismatch(ж<testing.T> Ꮡt) {
    var h1 = New(IEEETable);
    var h2 = New(MakeTable(Castagnoli));
    var (state1, err) = h1._<encoding.BinaryMarshaler>().MarshalBinary();
    if (err != default!) {
        Ꮡt.Errorf("could not marshal: %v"u8, err);
    }
    {
        var errΔ1 = h2._<encoding.BinaryUnmarshaler>().UnmarshalBinary(state1); if (errΔ1 == default!) {
            Ꮡt.Errorf("no error when one was expected"u8);
        }
    }
}

// TestSlicing tests the slicing-by-8 algorithm.
public static void TestSlicing(ж<testing.T> Ꮡt) {
    ref var tab = ref heap<ж<global::go.hash.crc32_package.slicing8Table>>(out var Ꮡtab);
    tab = slicingMakeTable(IEEE);
    testGoldenIEEE(Ꮡt, (slice<byte> b) => slicingUpdate(0, Ꮡtab.ValueSlot, b));
    tab = slicingMakeTable(Castagnoli);
    testGoldenCastagnoli(Ꮡt, (slice<byte> b) => slicingUpdate(0, Ꮡtab.ValueSlot, b));
    // Cross-check various polys against the simple algorithm.
    foreach (var (_, poly) in new uint32[]{IEEE, Castagnoli, Koopman, 0xD5828281U}.slice()) {
        var t1 = simpleMakeTable(poly);
        var t1ʗ1 = t1;
        var f1 = (uint32 crc, slice<byte> b) => simpleUpdate(crc, ref (t1ʗ1).DerefOrNull(), b);
        var t2 = slicingMakeTable(poly);
        var t2ʗ1 = t2;
        var f2 = (uint32 crc, slice<byte> b) => slicingUpdate(crc, t2ʗ1, b);
        testCrossCheck(Ꮡt, f1, f2);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object archSpecificIeeeNotˢ = (@string)"Arch-specific IEEE not available."u8;

public static void TestArchIEEE(ж<testing.T> Ꮡt) {
    if (!archAvailableIEEE()) {
        Ꮡt.Skip(archSpecificIeeeNotˢ);
    }
    archInitIEEE();
    var slicingTable = slicingMakeTable(IEEE);
    var slicingTableʗ1 = slicingTable;
    testCrossCheck(Ꮡt, archUpdateIEEE, (uint32 crc, slice<byte> b) => slicingUpdate(crc, slicingTableʗ1, b));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object archSpecificCastagnoliˢ = (@string)"Arch-specific Castagnoli not available."u8;

public static void TestArchCastagnoli(ж<testing.T> Ꮡt) {
    if (!archAvailableCastagnoli()) {
        Ꮡt.Skip(archSpecificCastagnoliˢ);
    }
    archInitCastagnoli();
    var slicingTable = slicingMakeTable(Castagnoli);
    var slicingTableʗ1 = slicingTable;
    testCrossCheck(Ꮡt, archUpdateCastagnoli, (uint32 crc, slice<byte> b) => slicingUpdate(crc, slicingTableʗ1, b));
}

public static void TestGolden(ж<testing.T> Ꮡt) {
    testGoldenIEEE(Ꮡt, ChecksumIEEE);
    // Some implementations have special code to deal with misaligned
    // data; test that as well.
    for (nint deltaᴛ1 = 1; deltaᴛ1 <= 7; deltaᴛ1++) {
        var delta = deltaᴛ1;
        testGoldenIEEE(Ꮡt, (slice<byte> b) => {
            var ieee = NewIEEE();
            nint d = delta;
            if (d >= len(b)) {
                d = len(b);
            }
            ieee.Write(b.slice(0, d));
            ieee.Write(b.slice(d));
            return ieee.Sum32();
        });
    }
    var castagnoliTab = MakeTable(Castagnoli);
    if (castagnoliTab == nil) {
        Ꮡt.Errorf("nil Castagnoli Table"u8);
    }
    var castagnoliTabʗ1 = castagnoliTab;
    testGoldenCastagnoli(Ꮡt, (slice<byte> b) => {
        var castagnoli = New(castagnoliTabʗ1);
        castagnoli.Write(b);
        return castagnoli.Sum32();
    });
    // Some implementations have special code to deal with misaligned
    // data; test that as well.
    for (nint deltaᴛ2 = 1; deltaᴛ2 <= 7; deltaᴛ2++) {
        var delta = deltaᴛ2;
        var castagnoliTabʗ2 = castagnoliTab;
        testGoldenCastagnoli(Ꮡt, (slice<byte> b) => {
            var castagnoli = New(castagnoliTabʗ2);
            nint d = delta;
            if (d >= len(b)) {
                d = len(b);
            }
            castagnoli.Write(b.slice(0, d));
            castagnoli.Write(b.slice(d));
            return castagnoli.Sum32();
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string polyIeeeˢ = "poly=IEEE"u8;
internal static readonly @string polyCastagnoliˢ = "poly=Castagnoli"u8;
internal static readonly @string polyKoopmanˢ = "poly=Koopman"u8;

public static void BenchmarkCRC32(ж<testing.B> Ꮡb) {
    Ꮡb.Run(polyIeeeˢ, benchmarkAll(NewIEEE()));
    Ꮡb.Run(polyCastagnoliˢ, benchmarkAll(New(MakeTable(Castagnoli))));
    Ꮡb.Run(polyKoopmanˢ, benchmarkAll(New(MakeTable(Koopman))));
}

internal static Action<ж<testing.B>> benchmarkAll(hash.Hash32 h) {
    return (ж<testing.B> b) => {
        foreach (var (_, size) in new nint[]{15, 40, 512, (1 << (int)(10)), (4 << (int)(10)), (32 << (int)(10))}.slice()) {
            @string name = fmt.Sprint(size);
            if (size >= 1024) {
                name = fmt.Sprintf("%dkB"u8, size / 1024);
            }
            b.Run("size="u8 + name, (ж<testing.B> bΔ1) => {
                for (nint alignᴛ1 = 0; alignᴛ1 <= 1; alignᴛ1++) {
                    var align = alignᴛ1;
                    bΔ1.Run(fmt.Sprintf("align=%d"u8, align), (ж<testing.B> bΔ2) => {
                        benchmark(bΔ2, h, (int64)size, (int64)align);
                    });
                }
            });
        }
    };
}

internal static void benchmark(ж<testing.B> Ꮡb, hash.Hash32 h, int64 n, int64 alignment) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes(n);
    var data = new slice<byte>((nint)(n + alignment));
    data = data.slice((nint)(alignment));
    foreach (var (i, _) in data) {
        data[i] = (byte)i;
    }
    var @in = new slice<byte>(0, h.Size());
    // Warm up
    h.Reset();
    h.Write(data);
    h.Sum(@in);
    // Avoid further allocations
    @in = @in[..0];
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        h.Reset();
        h.Write(data);
        h.Sum(@in);
        @in = @in[..0];
    }
}

} // end crc32_internal_test_package
