// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build 386 || amd64
namespace go.@internal;

using static go.@internal.cpu_package;

partial class cpu_internal_test_package {

public static readonly ж<Func<int32>> ᏑGetGOAMD64level = new StaticBox<Func<int32>>(getGOAMD64level);
public static ref Func<int32> GetGOAMD64level => ref ((StaticBox<Func<int32>>)ᏑGetGOAMD64level).ValueSlot;

} // end cpu_internal_test_package
