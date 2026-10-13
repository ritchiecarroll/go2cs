// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testing = testing_package;
using static unicode_package;
using Δunicode = unicode_package;

partial class unicode_test_package {

// Independently check that the special "Is" functions work
// in the Latin-1 range through the property table.
public static void TestIsControlLatin1(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    for (var i = (rune)0; i <= MaxLatin1; i++) {
        var got = IsControl(i);
        var want = false;
        switch (ᐧ) {
        case {} when 0x00 <= i && i <= 0x1F: {
            want = true;
            break;
        }
        case {} when 0x7F <= i && i <= 0x9F: {
            want = true;
            break;
        }}
        if (got != want) {
            Ꮡt.Errorf("%U incorrect: got %t; want %t"u8, i, got, want);
        }
    }
}

public static void TestIsLetterLatin1(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    for (var i = (rune)0; i <= MaxLatin1; i++) {
        var got = IsLetter(i);
        var want = Is(Letter, i);
        if (got != want) {
            Ꮡt.Errorf("%U incorrect: got %t; want %t"u8, i, got, want);
        }
    }
}

public static void TestIsUpperLatin1(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    for (var i = (rune)0; i <= MaxLatin1; i++) {
        var got = IsUpper(i);
        var want = Is(Upper, i);
        if (got != want) {
            Ꮡt.Errorf("%U incorrect: got %t; want %t"u8, i, got, want);
        }
    }
}

public static void TestIsLowerLatin1(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    for (var i = (rune)0; i <= MaxLatin1; i++) {
        var got = IsLower(i);
        var want = Is(Lower, i);
        if (got != want) {
            Ꮡt.Errorf("%U incorrect: got %t; want %t"u8, i, got, want);
        }
    }
}

public static void TestNumberLatin1(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    for (var i = (rune)0; i <= MaxLatin1; i++) {
        var got = IsNumber(i);
        var want = Is(Number, i);
        if (got != want) {
            Ꮡt.Errorf("%U incorrect: got %t; want %t"u8, i, got, want);
        }
    }
}

public static void TestIsPrintLatin1(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    for (var i = (rune)0; i <= MaxLatin1; i++) {
        var got = IsPrint(i);
        var want = In(i, PrintRanges.ꓸꓸꓸ);
        if (i == (rune)' ') {
            want = true;
        }
        if (got != want) {
            Ꮡt.Errorf("%U incorrect: got %t; want %t"u8, i, got, want);
        }
    }
}

public static void TestIsGraphicLatin1(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    for (var i = (rune)0; i <= MaxLatin1; i++) {
        var got = IsGraphic(i);
        var want = In(i, GraphicRanges.ꓸꓸꓸ);
        if (got != want) {
            Ꮡt.Errorf("%U incorrect: got %t; want %t"u8, i, got, want);
        }
    }
}

public static void TestIsPunctLatin1(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    for (var i = (rune)0; i <= MaxLatin1; i++) {
        var got = IsPunct(i);
        var want = Is(Punct, i);
        if (got != want) {
            Ꮡt.Errorf("%U incorrect: got %t; want %t"u8, i, got, want);
        }
    }
}

public static void TestIsSpaceLatin1(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    for (var i = (rune)0; i <= MaxLatin1; i++) {
        var got = IsSpace(i);
        var want = Is(White_Space, i);
        if (got != want) {
            Ꮡt.Errorf("%U incorrect: got %t; want %t"u8, i, got, want);
        }
    }
}

public static void TestIsSymbolLatin1(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    for (var i = (rune)0; i <= MaxLatin1; i++) {
        var got = IsSymbol(i);
        var want = Is(Symbol, i);
        if (got != want) {
            Ꮡt.Errorf("%U incorrect: got %t; want %t"u8, i, got, want);
        }
    }
}

} // end unicode_test_package
