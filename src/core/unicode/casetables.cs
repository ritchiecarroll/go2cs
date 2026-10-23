// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// TODO: This file contains the special casing rules for Turkish and Azeri only.
// It should encompass all the languages with special casing rules
// and be generated automatically, but that requires some API
// development first.
namespace go;

partial class unicode_package {

[GoLazy] partial class lazyᴛTurkishCase { public static SpecialCase TurkishCase = _TurkishCase; }

[GoLazy] partial class lazyᴛ_TurkishCase { internal static SpecialCase _TurkishCase = new SpecialCase(new CaseRange[]{
    new CaseRange(0x0049, 0x0049, new d(new rune[]{0, 0x131 - 0x49, 0}.array())),
    new CaseRange(0x0069, 0x0069, new d(new rune[]{0x130 - 0x69, 0, 0x130 - 0x69}.array())),
    new CaseRange(0x0130, 0x0130, new d(new rune[]{0, 0x69 - 0x130, 0}.array())),
    new CaseRange(0x0131, 0x0131, new d(new rune[]{0x49 - 0x131, 0, 0x49 - 0x131}.array()))
}.slice()); }

[GoLazy] partial class lazyᴛAzeriCase { public static SpecialCase AzeriCase = _TurkishCase; }

} // end unicode_package
