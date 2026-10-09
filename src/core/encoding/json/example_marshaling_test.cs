// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using json = go.encoding.json_package;
using fmt = fmt_package;
using log = log_package;
using strings = strings_package;
using go.encoding;
using static go.encoding.json_internal_test_package;

partial class json_test_package {

partial struct Animal /*num:nint*/;

public static Animal Unknown => /* iota */ 0;
public static Animal Gopher => 1;
public static Animal Zebra => 2;

public static error UnmarshalJSON(this ref Animal a, slice<byte> b) {
    ref var s = ref heap(new @string(), out var Ꮡs);
    {
        var err = json.Unmarshal(b, Ꮡs); if (err != default!) {
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

public static (slice<byte>, error) MarshalJSON(this Animal a) {
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

    return json.Marshal(s);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gopherArmadilloZebraˢ = @"[""gopher"",""armadillo"",""zebra"",""unknown"",""gopher"",""bee"",""gopher"",""zebra""]"u8;

public static void Example_customMarshalJSON() {
    @string blob = gopherArmadilloZebraˢ;
    ref var zoo = ref heap<slice<Animal>>(out var Ꮡzoo);
    {
        var err = json.Unmarshal(slice<byte>(blob), Ꮡzoo); if (err != default!) {
            log.Fatal(err);
        }
    }
    var census = new map<Animal, nint>();
    foreach (var (_, animal) in zoo) {
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

} // end json_test_package
