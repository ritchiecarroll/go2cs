// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using testing = testing_package;
using static go.@internal.trace_package;

partial class trace_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string oneElementˢ => ref oneElementˢᵇ;
private static readonly @string oneElementˢᵇ = "one element"u8;
internal static ref readonly @string twoElementsˢ => ref twoElementsˢᵇ;
private static readonly @string twoElementsˢᵇ = "two elements"u8;
internal static ref readonly @string sixElementsˢ => ref sixElementsˢᵇ;
private static readonly @string sixElementsˢᵇ = "six elements"u8;
internal static ref readonly @string oneElementAgainˢ => ref oneElementAgainˢᵇ;
private static readonly @string oneElementAgainˢᵇ = "one element again"u8;
internal static ref readonly @string twoElementsAgainˢ => ref twoElementsAgainˢᵇ;
private static readonly @string twoElementsAgainˢᵇ = "two elements again"u8;

public static void TestQueue(ж<testing.T> t) {
    ref var q = ref heap(new global::go.@internal.trace_package.queue<nint>(), out var Ꮡq);
    void check(@string name, slice<nint> exp) {
        foreach (var (_, v) in range(exp)) {
            Ꮡq.Value.push(v);
        }
        foreach (var (i, want) in range(exp)) {
            var (got, ok) = Ꮡq.Value.pop(); if (!ok) {
                t.Fatalf("check %q: expected to be able to pop after %d pops", name, i + 1);
            } else if (got != want) {
                t.Fatalf("check %q: expected value %d after on pop %d, got %d", name, want, i + 1, got);
            }
        }
        {
            var (_, ok) = Ꮡq.Value.pop(); if (ok) {
                t.Fatalf("check %q: did not expect to be able to pop more values", name);
            }
        }
        {
            var (_, ok) = Ꮡq.Value.pop(); if (ok) {
                t.Fatalf("check %q: did not expect to be able to pop more values a second time", name);
            }
        }
    }
    check(oneElementˢ, [4]);
    check(twoElementsˢ, [64, 12]);
    check(sixElementsˢ, [55, 16423, 2352, 644, 12874, 9372]);
    check(oneElementAgainˢ, [7]);
    check(twoElementsAgainˢ, [77, 6336]);
}

} // end trace_internal_test_package
