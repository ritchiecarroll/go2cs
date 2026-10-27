// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using static go.debug.dwarf_package;

partial class dwarf_internal_test_package {

public static readonly ж<Func<@string, @string, @string>> ᏑPathJoin = new StaticBox<Func<@string, @string, @string>>(pathJoin);
public static ref Func<@string, @string, @string> PathJoin => ref ((StaticBox<Func<@string, @string, @string>>)ᏑPathJoin).ValueSlot;

} // end dwarf_internal_test_package
