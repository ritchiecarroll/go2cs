// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using rand = math.rand.rand_package;
using slices = slices_package;
using testing = testing_package;
using iter = iter_package;
using math.rand;
using static global::go.go.token_package;

partial class token_internal_test_package {

// TestTree provides basic coverage of the AVL tree operations.
public static void TestTree(ж<testing.T> Ꮡt) {
    // Use a reproducible PRNG.
    var (seed1, seed2) = (rand.Uint64(), rand.Uint64());
    Ꮡt.Logf("random seeds: %d, %d"u8, seed1, seed2);
    var rng = rand.New(new rand.PCGжSource(rand.NewPCG(seed1, seed2)));

    // Create a number of Files of arbitrary size.
    var files = new slice<ж<global::go.go.token_package.ΔFile>>(500);
    ref var @base = ref heap(new nint(), out var Ꮡbase);
    foreach (var i in range(files)) {
        @base++;
        ref var size = ref heap<nint>(out var Ꮡsize);
        size = 1000;
        files[i] = Ꮡ(new ΔFile(@base: @base, size: size));
        @base += size;
    }

    // Add them all to the tree in random order.
    ref var tr = ref heap(new global::go.go.token_package.tree(), out var Ꮡtr);
    {
        var files2 = slices.Clone<slice<ж<global::go.go.token_package.ΔFile>>, ж<global::go.go.token_package.ΔFile>>(files);
        Shuffle(rng, files2);
        foreach (var (_, f) in files2) {
            Ꮡtr.add(f);
        }
    }

    // Randomly delete a subset of them.
    foreach (var _ᴛ1 in range(100)) {
        nint i = rng.IntN(len(files));
        var @file = files[i];
        if (@file == nil) {
            continue; // already deleted
        }
        files[i] = default!;

        var (pn, _) = Ꮡtr.locate(@file.key());
        if ((~(pn.ValueSlot)).@file != @file) {
            Ꮡt.Fatalf("locate returned wrong file"u8);
        }
        tr.delete(pn);
    }

    // Check some position lookups within each file.
    foreach (var (_, @file) in files) {
        if (@file == nil) {
            continue; // deleted
        }
        foreach (var (_, pos) in new nint[]{
            (~@file).@base, // start
            (~@file).@base + (~@file).size / 2, // midpoint
            (~@file).@base + (~@file).size // end
        }.slice()) {
            var (pn, _) = Ꮡtr.locate(new Δkey(pos, pos));
            if ((~(pn.ValueSlot)).@file != @file) {
                Ꮡt.Fatalf("lookup %s@%d returned wrong file %s"u8,
                    (~@file).name, pos,
                    (~(~(pn.ValueSlot)).@file).name);
            }
        }
    }

    // Check that the sequence is the same.
    files = slices.DeleteFunc(files, (ж<global::go.go.token_package.ΔFile> f) => f == nil);
    if (!slices.Equal<slice<ж<global::go.go.token_package.ΔFile>>, ж<global::go.go.token_package.ΔFile>>(slices.Collect(Ꮡtr.all()), files)) {
        Ꮡt.Fatalf("incorrect tree.all sequence"u8);
    }
}

public static void Shuffle<T>(ж<rand.Rand> Ꮡrng, slice<ж<T>> Δslice) {
    ref var rng = ref Ꮡrng.DerefOrNull();

    var sliceʗ1 = Δslice;
    rng.Shuffle(len(Δslice), (nint i, nint j) => {
        (sliceʗ1[i], sliceʗ1[j]) = (sliceʗ1[j], sliceʗ1[i]);
    });
}

} // end token_internal_test_package
