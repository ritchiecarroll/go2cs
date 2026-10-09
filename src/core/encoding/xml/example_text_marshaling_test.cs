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
internal static readonly @string sizesSizeSmallSizeSizeˢ = """

	<sizes>
		<size>small</size>
		<size>regular</size>
		<size>large</size>
		<size>unrecognized</size>
		<size>small</size>
		<size>normal</size>
		<size>small</size>
		<size>large</size>
	</sizes>
"""u8;

internal partial struct Example_textMarshalXML_inventory /*dyn*/ {
    public slice<Size> Sizes; /*`xml:"size"`*/
}

public static void Example_textMarshalXML() {
    @string blob = sizesSizeSmallSizeSizeˢ;
    ref var inventory = ref heap(new Example_textMarshalXML_inventory(), out var Ꮡinventory);
    {
        var err = xml.Unmarshal(slice<byte>(blob), Ꮡinventory); if (err != default!) {
            log.Fatal(err);
        }
    }
    var counts = new map<Size, nint>();
    foreach (var (_, size) in inventory.Sizes) {
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

} // end xml_test_package
