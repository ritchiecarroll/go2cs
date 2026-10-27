// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using static go.@internal.cpu_package;

partial class cpu_internal_test_package {

internal static readonly ж<slice<global::go.@internal.cpu_package.option>> ᏑOptions = new StaticBox<slice<global::go.@internal.cpu_package.option>>(default(slice<global::go.@internal.cpu_package.option>));
internal static ref slice<global::go.@internal.cpu_package.option> Options => ref ((StaticBox<slice<global::go.@internal.cpu_package.option>>)ᏑOptions).ValueSlot;
internal static void initᴛOptions() { Options = options; }

} // end cpu_internal_test_package
