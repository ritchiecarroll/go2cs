// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;
using gob = go.encoding.gob_package;
using fmt = fmt_package;
using log = log_package;
using math = math_package;
using go.encoding;
using io = io_package;
using static go.encoding.gob_internal_test_package;

partial class gob_test_package {

partial struct Point {
    public nint X, Y;
}

public static float64 Hypotenuse(this Point p) {
    return math.Hypot((float64)p.X, (float64)p.Y);
}

partial interface Pythagoras {
    float64 Hypotenuse();
}

// This example shows how to encode an interface value. The key
// distinction from regular types is to register the concrete type that
// implements the interface.
public static void Example_interface() {
    ref var network = ref heap(new bytes.Buffer(), out var Ꮡnetwork);                   // Stand-in for the network.
    // We must register the concrete type for the encoder and decoder (which would
    // normally be on a separate machine from the encoder). On each end, this tells the
    // engine which concrete type is being sent that implements the interface.
    gob.Register(new Point(nil));
    // Create an encoder and send some values.
    var enc = gob.NewEncoder(new gob_test_package.bytes_BufferжWriter(Ꮡnetwork));
    for (nint i = 1; i <= 3; i++) {
        interfaceEncode(enc, new Point(3 * i, 4 * i));
    }
    // Create a decoder and receive some values.
    var dec = gob.NewDecoder(new gob_test_package.bytes_BufferжReader(Ꮡnetwork));
    for (nint i = 1; i <= 3; i++) {
        var result = interfaceDecode(dec);
        fmt.Println(result.Hypotenuse());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object encodeˢ = (@string)"encode:"u8;

// Output:
// 5
// 10
// 15

// interfaceEncode encodes the interface value into the encoder.
internal static void interfaceEncode(ж<gob.Encoder> Ꮡenc, Pythagoras pʗp) {
    ref var p = ref heap(pʗp, out var Ꮡp);

    // The encode will fail unless the concrete type has been
    // registered. We registered it in the calling function.
    // Pass pointer to interface so Encode sees (and hence sends) a value of
    // interface type. If we passed p directly it would see the concrete type instead.
    // See the blog post, "The Laws of Reflection" for background.
    var err = Ꮡenc.Encode(Ꮡp);
    if (err != default!) {
        log.Fatal(encodeˢ, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object decodeˢ = (@string)"decode:"u8;

// interfaceDecode decodes the next interface value from the stream and returns it.
internal static Pythagoras interfaceDecode(ж<gob.Decoder> Ꮡdec) {
    // The decode will fail unless the concrete type on the wire has been
    // registered. We registered it in the calling function.
    ref var p = ref heap<Pythagoras>(out var Ꮡp);
    var err = Ꮡdec.Decode(Ꮡp);
    if (err != default!) {
        log.Fatal(decodeˢ, err);
    }
    return p;
}

} // end gob_test_package
