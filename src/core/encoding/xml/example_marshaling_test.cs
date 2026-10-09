// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using xml = go.encoding.xml_package;
using fmt = fmt_package;
using log = log_package;
using strings = strings_package;
using go.encoding;
using static go.encoding.xml_internal_test_package;

partial class xml_test_package {

partial struct Animal /*num:nint*/;

public static Animal Unknown => /* iota */ 0;
public static Animal Gopher => 1;
public static Animal Zebra => 2;

public static error UnmarshalXML(this ref Animal a, ж<xml.Decoder> Ꮡd, xml.StartElement startʗp) {
    ref var start = ref heap(startʗp, out var Ꮡstart);

    ref var s = ref heap(new @string(), out var Ꮡs);
    {
        var err = Ꮡd.DecodeElement(Ꮡs, Ꮡstart); if (err != default!) {
            return err;
        }
    }
    var exprᴛ1 = strings.ToLower(s);
    if (exprᴛ1 == "gopher"u8) {
        a = Gopher;
    }
    else if (exprᴛ1 == "zebra"u8) {
        a = Zebra;
    }
    else { /* default: */
        a = Unknown;
    }

    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unknownˢ = "unknown"u8;
internal static readonly @string gopherˢ = "gopher"u8;
internal static readonly @string zebraˢ = "zebra"u8;

public static error MarshalXML(this Animal a, ж<xml.Encoder> Ꮡe, xml.StartElement start) {
    @string s = default!;
    var exprᴛ1 = a;
    if (exprᴛ1 == Gopher) {
        s = gopherˢ;
    }
    else if (exprᴛ1 == Zebra) {
        s = zebraˢ;
    }
    else { /* default: */
        s = unknownˢ;
    }

    return Ꮡe.EncodeElement(s, start);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string animalsAnimalGopherˢ = """

	<animals>
		<animal>gopher</animal>
		<animal>armadillo</animal>
		<animal>zebra</animal>
		<animal>unknown</animal>
		<animal>gopher</animal>
		<animal>bee</animal>
		<animal>gopher</animal>
		<animal>zebra</animal>
	</animals>
"""u8;

internal partial struct Example_customMarshalXML_zoo /*dyn*/ {
    public slice<Animal> Animals; /*`xml:"animal"`*/
}

public static void Example_customMarshalXML() {
    @string blob = animalsAnimalGopherˢ;
    ref var zoo = ref heap(new Example_customMarshalXML_zoo(), out var Ꮡzoo);
    {
        var err = xml.Unmarshal(slice<byte>(blob), Ꮡzoo); if (err != default!) {
            log.Fatal(err);
        }
    }
    var census = new map<Animal, nint>();
    foreach (var (_, animal) in zoo.Animals) {
        census[animal] += 1;
    }
    fmt.Printf("Zoo Census:\n* Gophers: %d\n* Zebras:  %d\n* Unknown: %d\n"u8,
        census[Gopher], census[Zebra], census[Unknown]);
}

// Output:
// Zoo Census:
// * Gophers: 3
// * Zebras:  2
// * Unknown: 3

} // end xml_test_package
