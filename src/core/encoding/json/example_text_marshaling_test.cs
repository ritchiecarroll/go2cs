// Copyright 2018 The Go Authors. All rights reserved.
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

partial struct Size /*num:nint*/;

public static Size Unrecognized => /* iota */ 0;
public static Size Small => 1;
public static Size Large => 2;

public static error UnmarshalText(this ref Size s, slice<byte> text) {
    var exprᴛ1 = strings.ToLower(((@string)text));
    if (exprᴛ1 == "small"u8) {
        s = Small;
    }
    else if (exprᴛ1 == "large"u8) {
        s = Large;
    }
    else { /* default: */
        s = Unrecognized;
    }

    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unrecognizedˢ = "unrecognized"u8;
internal static readonly @string smallˢ = "small"u8;
internal static readonly @string largeˢ = "large"u8;

public static (slice<byte>, error) MarshalText(this Size s) {
    @string name = default!;
    var exprᴛ1 = s;
    if (exprᴛ1 == Small) {
        name = smallˢ;
    }
    else if (exprᴛ1 == Large) {
        name = largeˢ;
    }
    else { /* default: */
        name = unrecognizedˢ;
    }

    return (slice<byte>(name), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string smallRegularLargeˢ = @"[""small"",""regular"",""large"",""unrecognized"",""small"",""normal"",""small"",""large""]"u8;

public static void Example_textMarshalJSON() {
    @string blob = smallRegularLargeˢ;
    ref var inventory = ref heap<slice<Size>>(out var Ꮡinventory);
    {
        var err = json.Unmarshal(slice<byte>(blob), Ꮡinventory); if (err != default!) {
            log.Fatal(err);
        }
    }
    var counts = new map<Size, nint>();
    foreach (var (_, size) in inventory) {
        counts[size] += 1;
    }
    fmt.Printf("Inventory Counts:\n* Small:        %d\n* Large:        %d\n* Unrecognized: %d\n"u8,
        counts[Small], counts[Large], counts[Unrecognized]);
}

// Output:
// Inventory Counts:
// * Small:        3
// * Large:        2
// * Unrecognized: 3

} // end json_test_package
