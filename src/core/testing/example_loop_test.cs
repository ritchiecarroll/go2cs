// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using rand = math.rand.rand_package;
using testing = testing_package;
using math.rand;

partial class testing_test_package {

// ExBenchmark shows how to use b.Loop in a benchmark.
//
// (If this were a real benchmark, not an example, this would be named
// BenchmarkSomething.)
public static void ExBenchmark(ж<testing.B> Ꮡb) {
    // Generate a large random slice to use as an input.
    // Since this is done before the first call to b.Loop(),
    // it doesn't count toward the benchmark time.
    var input = new slice<nint>((128 << 10));
    foreach (var (i, _) in input) {
        input[i] = rand.Int();
    }

    // Perform the benchmark.
    while (Ꮡb.Loop()) {
        // Normally, the compiler would be allowed to optimize away the call
        // to sum because it has no side effects and the result isn't used.
        // However, inside a b.Loop loop, the compiler ensures function calls
        // aren't optimized away.
        sum(input);
    }

    // Outside the loop, the timer is stopped, so we could perform
    // cleanup if necessary without affecting the result.
}

internal static nint sum(slice<nint> data) {
    nint total = 0;
    foreach (var (_, value) in data) {
        total += value;
    }
    return total;
}

public static void ExampleB_Loop() {
    testing.Benchmark(ExBenchmark);
}

} // end testing_test_package
