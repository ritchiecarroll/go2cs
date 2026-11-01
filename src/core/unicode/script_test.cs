// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testing = testing_package;
using static go.unicode_package;
using Δunicode = unicode_package;

partial class unicode_test_package {

partial struct T {
    internal rune rune;
    internal @string script;
}

internal static slice<T> inCategoryTest = slice<T>([
    new(0x0081, "Cc"),
    new(0x200B, "Cf"),
    new(0xf0000, "Co"),
    new(0xdb80, "Cs"),
    new(0x0236, "Ll"),
    new(0x1d9d, "Lm"),
    new(0x07cf, "Lo"),
    new(0x1f8a, "Lt"),
    new(0x03ff, "Lu"),
    new(0x0bc1, "Mc"),
    new(0x20df, "Me"),
    new(0x07f0, "Mn"),
    new(0x1bb2, "Nd"),
    new(0x10147, "Nl"),
    new(0x2478, "No"),
    new(0xfe33, "Pc"),
    new(0x2011, "Pd"),
    new(0x301e, "Pe"),
    new(0x2e03, "Pf"),
    new(0x2e02, "Pi"),
    new(0x0022, "Po"),
    new(0x2770, "Ps"),
    new(0x00a4, "Sc"),
    new(0xa711, "Sk"),
    new(0x25f9, "Sm"),
    new(0x2108, "So"),
    new(0x2028, "Zl"),
    new(0x2029, "Zp"),
    new(0x202f, "Zs"),
    // Unifieds.
    new(0x04aa, "L"),
    new(0x0009, "C"),
    new(0x1712, "M"),
    new(0x0031, "N"),
    new(0x00bb, "P"),
    new(0x00a2, "S"),
    new(0x00a0, "Z"),
    new(0x0065, "LC"),
    // Unassigned
    new(0x0378, "Cn"),
    new(0x0378, "C")
]);

internal static slice<T> inPropTest = slice<T>([
    new(0x0046, "ASCII_Hex_Digit"),
    new(0x200F, "Bidi_Control"),
    new(0x2212, "Dash"),
    new(0xE0001, "Deprecated"),
    new(0x00B7, "Diacritic"),
    new(0x30FE, "Extender"),
    new(0xFF46, "Hex_Digit"),
    new(0x2E17, "Hyphen"),
    new(0x2FFB, "IDS_Binary_Operator"),
    new(0x2FF3, "IDS_Trinary_Operator"),
    new(0xFA6A, "Ideographic"),
    new(0x200D, "Join_Control"),
    new(0x0EC4, "Logical_Order_Exception"),
    new(0x2FFFF, "Noncharacter_Code_Point"),
    new(0x065E, "Other_Alphabetic"),
    new(0x2065, "Other_Default_Ignorable_Code_Point"),
    new(0x0BD7, "Other_Grapheme_Extend"),
    new(0x0387, "Other_ID_Continue"),
    new(0x212E, "Other_ID_Start"),
    new(0x2094, "Other_Lowercase"),
    new(0x2040, "Other_Math"),
    new(0x216F, "Other_Uppercase"),
    new(0x0027, "Pattern_Syntax"),
    new(0x0020, "Pattern_White_Space"),
    new(0x06DD, "Prepended_Concatenation_Mark"),
    new(0x300D, "Quotation_Mark"),
    new(0x2EF3, "Radical"),
    new(0x1f1ff, "Regional_Indicator"),
    new(0x061F, "STerm"), // Deprecated alias of Sentence_Terminal
    new(0x061F, "Sentence_Terminal"),
    new(0x2071, "Soft_Dotted"),
    new(0x003A, "Terminal_Punctuation"),
    new(0x9FC3, "Unified_Ideograph"),
    new(0xFE0F, "Variation_Selector"),
    new(0x0020, "White_Space")
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object notAKnownCategoryˢ = (@string)"not a known category"u8;
private static readonly object categoryNotTestedˢ = (@string)"category not tested:"u8;

public static void TestCategories(ж<testing.T> t) {
    var notTested = new map<@string, bool>();
    foreach (var (k, _) in Categories) {
        notTested[k] = true;
    }
    foreach (var (_, test) in inCategoryTest) {
        {
            var (_, ok) = Categories[test.script, ꟷ]; if (!ok) {
                t.Fatal(test.script, notAKnownCategoryˢ);
            }
        }
        if (!Is(Categories[test.script], test.rune)) {
            t.Errorf("IsCategory(%U, %s) = false, want true", test.rune, test.script);
        }
        delete(notTested, test.script);
    }
    foreach (var (k, _) in notTested) {
        t.Error(categoryNotTestedˢ, k);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object notAKnownPropˢ = (@string)"not a known prop"u8;
private static readonly object propertyNotTestedˢ = (@string)"property not tested:"u8;

public static void TestProperties(ж<testing.T> t) {
    var notTested = new map<@string, bool>();
    foreach (var (k, _) in Properties) {
        notTested[k] = true;
    }
    foreach (var (_, test) in inPropTest) {
        {
            var (_, ok) = Properties[test.script, ꟷ]; if (!ok) {
                t.Fatal(test.script, notAKnownPropˢ);
            }
        }
        if (!Is(Properties[test.script], test.rune)) {
            t.Errorf("IsCategory(%U, %s) = false, want true", test.rune, test.script);
        }
        delete(notTested, test.script);
    }
    foreach (var (k, _) in notTested) {
        t.Error(propertyNotTestedˢ, k);
    }
}

} // end unicode_test_package
