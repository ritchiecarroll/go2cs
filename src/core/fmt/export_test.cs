// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static go.fmt_package;

partial class fmt_internal_test_package {

public static readonly ж<Func<rune, bool>> ᏑIsSpace = new StaticBox<Func<rune, bool>>(default(Func<rune, bool>));
public static ref Func<rune, bool> IsSpace => ref ((StaticBox<Func<rune, bool>>)ᏑIsSpace).ValueSlot;
internal static void initᴛIsSpace() { IsSpace = isSpace; }

public static readonly ж<Func<@string, nint, nint, (nint, bool, nint)>> ᏑParsenum = new StaticBox<Func<@string, nint, nint, (nint, bool, nint)>>(parsenumᶠ);
public static ref Func<@string, nint, nint, (nint, bool, nint)> Parsenum => ref ((StaticBox<Func<@string, nint, nint, (nint, bool, nint)>>)ᏑParsenum).ValueSlot;

} // end fmt_internal_test_package
