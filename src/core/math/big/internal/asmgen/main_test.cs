// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.big.@internal;

using bytes = bytes_package;
using flag = flag_package;
using diff = go.@internal.diff_package;
using os = os_package;
using testing = testing_package;
using fs = io.fs_package;
using go.@internal;
using static go.math.big.@internal.asmgen_package;

partial class asmgen_internal_test_package {

internal static ж<bool> generateFlag = flag.Bool("generate", false, "generate files");

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string regeneratedˢ => ref regeneratedˢᵇ;
private static readonly @string regeneratedˢᵇ = "regenerated"u8;

public static void Test(ж<testing.T> t) {
    foreach (var (_, arch) in arches) {
        var archʗ1 = arch;
        t.Run(arch.Name, (ж<testing.T> tΔ1) => {
            var (@file, data) = generate(archʗ1);
            var (old, err) = os.ReadFile("../../"u8 + @file);
            if (err == nil && bytes.Equal(old, data)) {
                return;
            }
            if (generateFlag.Value) {
                {
                    var errΔ1 = os.WriteFile("../../"u8 + @file, data, 438); if (errΔ1 != nil) {
                        tΔ1.Fatal(errΔ1);
                    }
                }
                return;
            }
            if (err != nil) {
                tΔ1.Fatal(err);
            }
            tΔ1.Fatalf("generated assembly differs:\n%s\n", diff.Diff("../../"u8 + @file, old, regeneratedˢ, data));
        });
    }
}

} // end asmgen_internal_test_package
