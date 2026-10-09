// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testing = testing_package;
using static unicode_package;
using Δunicode = unicode_package;

partial class unicode_test_package {

partial struct T {
    internal rune rune;
    internal @string script;
}

// Unifieds.
internal static slice<T> inCategoryTest = new T[]{
    new(0x0081, "Cc"u8),
    new(0x200B, "Cf"u8),
    new(0xf0000, "Co"u8),
    new(0xdb80, "Cs"u8),
    new(0x0236, "Ll"u8),
    new(0x1d9d, "Lm"u8),
    new(0x07cf, "Lo"u8),
    new(0x1f8a, "Lt"u8),
    new(0x03ff, "Lu"u8),
    new(0x0bc1, "Mc"u8),
    new(0x20df, "Me"u8),
    new(0x07f0, "Mn"u8),
    new(0x1bb2, "Nd"u8),
    new(0x10147, "Nl"u8),
    new(0x2478, "No"u8),
    new(0xfe33, "Pc"u8),
    new(0x2011, "Pd"u8),
    new(0x301e, "Pe"u8),
    new(0x2e03, "Pf"u8),
    new(0x2e02, "Pi"u8),
    new(0x0022, "Po"u8),
    new(0x2770, "Ps"u8),
    new(0x00a4, "Sc"u8),
    new(0xa711, "Sk"u8),
    new(0x25f9, "Sm"u8),
    new(0x2108, "So"u8),
    new(0x2028, "Zl"u8),
    new(0x2029, "Zp"u8),
    new(0x202f, "Zs"u8),
    new(0x04aa, "L"u8),
    new(0x0009, "C"u8),
    new(0x1712, "M"u8),
    new(0x0031, "N"u8),
    new(0x00bb, "P"u8),
    new(0x00a2, "S"u8),
    new(0x00a0, "Z"u8)
}.slice();

// Deprecated alias of Sentence_Terminal
internal static slice<T> inPropTest = new T[]{
    new(0x0046, "ASCII_Hex_Digit"u8),
    new(0x200F, "Bidi_Control"u8),
    new(0x2212, "Dash"u8),
    new(0xE0001, "Deprecated"u8),
    new(0x00B7, "Diacritic"u8),
    new(0x30FE, "Extender"u8),
    new(0xFF46, "Hex_Digit"u8),
    new(0x2E17, "Hyphen"u8),
    new(0x2FFB, "IDS_Binary_Operator"u8),
    new(0x2FF3, "IDS_Trinary_Operator"u8),
    new(0xFA6A, "Ideographic"u8),
    new(0x200D, "Join_Control"u8),
    new(0x0EC4, "Logical_Order_Exception"u8),
    new(0x2FFFF, "Noncharacter_Code_Point"u8),
    new(0x065E, "Other_Alphabetic"u8),
    new(0x2065, "Other_Default_Ignorable_Code_Point"u8),
    new(0x0BD7, "Other_Grapheme_Extend"u8),
    new(0x0387, "Other_ID_Continue"u8),
    new(0x212E, "Other_ID_Start"u8),
    new(0x2094, "Other_Lowercase"u8),
    new(0x2040, "Other_Math"u8),
    new(0x216F, "Other_Uppercase"u8),
    new(0x0027, "Pattern_Syntax"u8),
    new(0x0020, "Pattern_White_Space"u8),
    new(0x06DD, "Prepended_Concatenation_Mark"u8),
    new(0x300D, "Quotation_Mark"u8),
    new(0x2EF3, "Radical"u8),
    new(0x1f1ff, "Regional_Indicator"u8),
    new(0x061F, "STerm"u8),
    new(0x061F, "Sentence_Terminal"u8),
    new(0x2071, "Soft_Dotted"u8),
    new(0x003A, "Terminal_Punctuation"u8),
    new(0x9FC3, "Unified_Ideograph"u8),
    new(0xFE0F, "Variation_Selector"u8),
    new(0x0020, "White_Space"u8)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object notAKnownCategoryˢ = (@string)"not a known category"u8;
private static readonly object categoryNotTestedˢ = (@string)"category not tested:"u8;

public static void TestCategories(ж<testing.T> Ꮡt) {
    var notTested = new map<@string, bool>();
    foreach (var (k, _) in Categories) {
        notTested[k] = true;
    }
    foreach (var (_, test) in inCategoryTest) {
        {
            var (_, ok) = Categories[test.script, ꟷ]; if (!ok) {
                Ꮡt.Fatal(test.script, notAKnownCategoryˢ);
            }
        }
        if (!Is(Categories[test.script], test.rune)) {
            Ꮡt.Errorf("IsCategory(%U, %s) = false, want true"u8, test.rune, test.script);
        }
        delete(notTested, test.script);
    }
    foreach (var (k, _) in notTested) {
        Ꮡt.Error(categoryNotTestedˢ, k);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object notAKnownPropˢ = (@string)"not a known prop"u8;
private static readonly object propertyNotTestedˢ = (@string)"property not tested:"u8;

public static void TestProperties(ж<testing.T> Ꮡt) {
    var notTested = new map<@string, bool>();
    foreach (var (k, _) in Properties) {
        notTested[k] = true;
    }
    foreach (var (_, test) in inPropTest) {
        {
            var (_, ok) = Properties[test.script, ꟷ]; if (!ok) {
                Ꮡt.Fatal(test.script, notAKnownPropˢ);
            }
        }
        if (!Is(Properties[test.script], test.rune)) {
            Ꮡt.Errorf("IsCategory(%U, %s) = false, want true"u8, test.rune, test.script);
        }
        delete(notTested, test.script);
    }
    foreach (var (k, _) in notTested) {
        Ꮡt.Error(propertyNotTestedˢ, k);
    }
}

} // end unicode_test_package
