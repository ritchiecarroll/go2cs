// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using static go.@internal.trace_package;

partial class trace_internal_test_package {

public static readonly ж<nint> ᏑBandsPerSeries = new StaticBox<nint>(default(nint));
public static ref nint BandsPerSeries => ref ((StaticBox<nint>)ᏑBandsPerSeries).ValueSlot;
internal static void initᴛBandsPerSeries() { BandsPerSeries = bandsPerSeries; }

} // end trace_internal_test_package
