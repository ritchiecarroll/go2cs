// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using static go.@internal.chacha8rand_package;

partial class chacha8rand_internal_test_package {

public static readonly ж<Action<ж<array<uint64>>, ж<array<uint64>>, uint32>> ᏑBlock = new StaticBox<Action<ж<array<uint64>>, ж<array<uint64>>, uint32>>(block);
public static ref Action<ж<array<uint64>>, ж<array<uint64>>, uint32> Block => ref ((StaticBox<Action<ж<array<uint64>>, ж<array<uint64>>, uint32>>)ᏑBlock).ValueSlot;

public static readonly ж<Action<ж<array<uint64>>, ж<array<uint64>>, uint32>> ᏑBlock_generic = new StaticBox<Action<ж<array<uint64>>, ж<array<uint64>>, uint32>>(block_generic);
public static ref Action<ж<array<uint64>>, ж<array<uint64>>, uint32> Block_generic => ref ((StaticBox<Action<ж<array<uint64>>, ж<array<uint64>>, uint32>>)ᏑBlock_generic).ValueSlot;

public static array<uint64> Seed(ж<global::go.@internal.chacha8rand_package.State> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    return s.seed.Clone();
}

} // end chacha8rand_internal_test_package
