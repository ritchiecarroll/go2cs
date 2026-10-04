// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.hash;

using encoding = encoding_package;
using io = io_package;
using strings = strings_package;
using testing = testing_package;
using hash = hash_package;
using static go.hash.adler32_package;

partial class adler32_internal_test_package {


[GoType("dyn")] partial struct goldenᴛ1 {
    internal uint32 @out;
    internal @string @in;
    internal @string halfState; // marshaled hash state after first half of in written, used by TestGoldenMarshal
}
internal static slice<goldenᴛ1> golden = new goldenᴛ1[]{
    new(0x00000001, ""u8, "adl\x01\x00\x00\x00\x01"u8),
    new(0x00620062, "a"u8, "adl\x01\x00\x00\x00\x01"u8),
    new(0x012600c4, "ab"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x00, 0x62, 0x00, 0x62}))),
    new(0x024d0127, "abc"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x00, 0x62, 0x00, 0x62}))),
    new(0x03d8018b, "abcd"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x01, 0x26, 0x00, 0xc4}))),
    new(0x05c801f0, "abcde"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x01, 0x26, 0x00, 0xc4}))),
    new(0x081e0256, "abcdef"u8, "adl\x01\x02M\x01'"u8),
    new(0x0adb02bd, "abcdefg"u8, "adl\x01\x02M\x01'"u8),
    new(0x0e000325, "abcdefgh"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x03, 0xd8, 0x01, 0x8b}))),
    new(0x118e038e, "abcdefghi"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x03, 0xd8, 0x01, 0x8b}))),
    new(0x158603f8, "abcdefghij"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x05, 0xc8, 0x01, 0xf0}))),
    new(0x3f090f02, "Discard medicine more than two years old."u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x4e, 0x55, 0x07, 0x87}))),
    new(0x46d81477, "He who has a shady past knows that nice guys finish last."u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x89, 0x8e, 0x09, 0xe9}))),
    new(0x40ee0ee1, "I wouldn't marry him with a ten foot pole."u8, "adl\x01R\t\ag"u8),
    new(0x16661315, "Free! Free!/A trip/to Mars/for 900/empty jars/Burma Shave"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x7f, 0xbb, 0x09, 0x10}))),
    new(0x5b2e1480, "The days of the digital watch are numbered.  -Tom Stoppard"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x99, 0x3a, 0x0a, 0x7e}))),
    new(0x8c3c09eaU, "Nepal premier won't resign."u8, "adl\x01\"\x05\x05\x05"u8),
    new(0x45ac18fd, "For every action there is an equal and opposite government program."u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0xcc, 0xfa, 0x0c, 0x00}))),
    new(0x53c61462, "His money is twice tainted: 'taint yours and 'taint mine."u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x93, 0xa9, 0x0a, 0x08}))),
    new(0x7e511e63, "There is no reason for any individual to have a computer in their home. -Ken Olsen, 1977"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x65, 0xf5, 0x10, 0x14}))),
    new(0xe4801a6aU, "It's a tiny change to the code and not completely disgusting. - Bob Manchek"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0xee, 0x00, 0x0c, 0xb2}))),
    new(0x61b507df, "size:  a.out:  bad magic"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x1a, 0xfc, 0x04, 0x1d}))),
    new(0xb8631171U, "The major problem is with sendmail.  -Mark Horton"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x6d, 0x69, 0x08, 0xdc}))),
    new(0x8b5e1904U, "Give me a rock, paper and scissors and I will move the world.  CCFestoon"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0xe3, 0x0a, 0x0c, 0x9f}))),
    new(0x7cc6102b, "If the enemy is within range, then so are you."u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x5f, 0xe0, 0x08, 0x1e}))),
    new(0x700318e7, "It's well we cannot hear the screams/That we create in others' dreams."u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0xdb, 0x98, 0x0c, 0x87}))),
    new(0x1e601747, "You remind me of a TV show, but that's all right: I watch it anyway."u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0xcc, 0x7d, 0x0b, 0x83}))),
    new(0xb55b0b09U, "C is as portable as Stonehedge!!"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x2c, 0x5e, 0x05, 0xad}))),
    new(0x39111dd0, "Even if I could be Shakespeare, I think I should still choose to be Faraday. - A. Huxley"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x4d, 0xd1, 0x0e, 0xc8}))),
    new(0x91dd304fU, "The fugacity of a constituent in a mixture of gases at a given temperature is proportional to its mole fraction.  Lewis-Randall Rule"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x23, 0xd8, 0x17, 0xd7}))),
    new(0x2e5d1316, "How can you write a big system without C++?  -Paul Glick"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x8f, 0x55, 0x0a, 0x0f}))),
    new(0xd0201df6U, "'Invariant assertions' is the most elegant programming technique!  -Tom Szymanski"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x2f, 0x98, 0x0e, 0xc4}))),
    new(0x211297c8, strings.Repeat(((@string)(new byte[]{0xff})), 5548) + "8"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x9a, 0xa6, 0xcb, 0xc1}))),
    new(0xbaa198c8U, strings.Repeat(((@string)(new byte[]{0xff})), 5549) + "9"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x67, 0x75, 0xcc, 0xc0}))),
    new(0x553499be, strings.Repeat(((@string)(new byte[]{0xff})), 5550) + "0"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x67, 0x75, 0xcc, 0xc0}))),
    new(0xf0c19abeU, strings.Repeat(((@string)(new byte[]{0xff})), 5551) + "1"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x35, 0x43, 0xcd, 0xbf}))),
    new(0x8d5c9bbeU, strings.Repeat(((@string)(new byte[]{0xff})), 5552) + "2"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x35, 0x43, 0xcd, 0xbf}))),
    new(0x2af69cbe, strings.Repeat(((@string)(new byte[]{0xff})), 5553) + "3"u8, "adl\x01\x04\x10ξ"u8),
    new(0xc9809dbeU, strings.Repeat(((@string)(new byte[]{0xff})), 5554) + "4"u8, "adl\x01\x04\x10ξ"u8),
    new(0x69189ebe, strings.Repeat(((@string)(new byte[]{0xff})), 5555) + "5"u8, ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0xd3, 0xcd, 0xcf, 0xbd}))),
    new(0x86af0001U, strings.Repeat("\x00"u8, 100000), ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0xc3, 0x50, 0x00, 0x01}))),
    new(0x79660b4d, strings.Repeat("a"u8, 100000), ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x81, 0x6b, 0x05, 0xa7}))),
    new(0x110588ee, strings.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ"u8, 10000), ((@string)(new byte[]{0x61, 0x64, 0x6c, 0x01, 0x65, 0xd2, 0xc4, 0x70})))
}.slice();

// checksum is a slow but simple implementation of the Adler-32 checksum.
// It is a straight port of the sample code in RFC 1950 section 9.
internal static uint32 checksum(slice<byte> p) {
    var (s1, s2) = ((uint32)1, (uint32)0);
    foreach (var (_, x) in p) {
        s1 = (s1 + (uint32)x) % (uint32)mod;
        s2 = (s2 + s1) % (uint32)mod;
    }
    return (uint32)((s2 << (int)(16)) | s1);
}

public static void TestGolden(ж<testing.T> Ꮡt) {
    foreach (var (_, g) in golden) {
        @string @in = g.@in;
        if (len(@in) > 220) {
            @in = @in[..100] + "..." + @in.slice(len(@in) - 100);
        }
        var p = slice<byte>(g.@in);
        {
            var got = checksum(p); if (got != g.@out) {
                Ꮡt.Errorf("simple implementation: checksum(%q) = 0x%x want 0x%x"u8, @in, got, g.@out);
                continue;
            }
        }
        {
            var got = Checksum(p); if (got != g.@out) {
                Ꮡt.Errorf("optimized implementation: Checksum(%q) = 0x%x want 0x%x"u8, @in, got, g.@out);
                continue;
            }
        }
    }
}

public static void TestGoldenMarshal(ж<testing.T> Ꮡt) {
    foreach (var (_, g) in golden) {
        var h = New();
        var h2 = New();
        io.WriteString(new adler32_internal_test_package.hash_Hash32ᴠWriter(h), g.@in.slice(0, len(g.@in) / 2));
        var (state, err) = h._<encoding.BinaryMarshaler>().MarshalBinary();
        if (err != default!) {
            Ꮡt.Errorf("could not marshal: %v"u8, err);
            continue;
        }
        (var stateAppend, err) = h._<encoding.BinaryAppender>().AppendBinary(new slice<byte>(4, 32));
        if (err != default!) {
            Ꮡt.Errorf("could not marshal: %v"u8, err);
            continue;
        }
        stateAppend = stateAppend[4..];
        if (((sstring)state) != g.halfState) {
            Ꮡt.Errorf("checksum(%q) state = %q, want %q"u8, g.@in, state, g.halfState);
            continue;
        }
        if (((sstring)stateAppend) != g.halfState) {
            Ꮡt.Errorf("checksum(%q) state = %q, want %q"u8, g.@in, stateAppend, g.halfState);
            continue;
        }
        {
            var errΔ1 = h2._<encoding.BinaryUnmarshaler>().UnmarshalBinary(state); if (errΔ1 != default!) {
                Ꮡt.Errorf("could not unmarshal: %v"u8, errΔ1);
                continue;
            }
        }
        io.WriteString(new adler32_internal_test_package.hash_Hash32ᴠWriter(h), g.@in.slice(len(g.@in) / 2));
        io.WriteString(new adler32_internal_test_package.hash_Hash32ᴠWriter(h2), g.@in.slice(len(g.@in) / 2));
        if (h.Sum32() != h2.Sum32()) {
            Ꮡt.Errorf("checksum(%q) = 0x%x != marshaled (0x%x)"u8, g.@in, h.Sum32(), h2.Sum32());
        }
    }
}

public static void BenchmarkAdler32KB(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes(1024);
    var data = new slice<byte>(1024);
    foreach (var (i, _) in data) {
        data[i] = (byte)i;
    }
    var h = New();
    var @in = new slice<byte>(0, h.Size());
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        h.Reset();
        h.Write(data);
        h.Sum(@in);
    }
}

} // end adler32_internal_test_package
