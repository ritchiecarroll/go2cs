// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.path;

using static go.path.filepath_package;

partial class filepath_internal_test_package {

public static readonly ж<Func<@string, Func<@string, (@string, error)>, (@string, error)>> ᏑToNorm = new StaticBox<Func<@string, Func<@string, (@string, error)>, (@string, error)>>(toNorm);
public static ref Func<@string, Func<@string, (@string, error)>, (@string, error)> ToNorm => ref ((StaticBox<Func<@string, Func<@string, (@string, error)>, (@string, error)>>)ᏑToNorm).ValueSlot;
public static readonly ж<Func<@string, (@string, error)>> ᏑNormBase = new StaticBox<Func<@string, (@string, error)>>(normBase);
public static ref Func<@string, (@string, error)> NormBase => ref ((StaticBox<Func<@string, (@string, error)>>)ᏑNormBase).ValueSlot;

} // end filepath_internal_test_package
