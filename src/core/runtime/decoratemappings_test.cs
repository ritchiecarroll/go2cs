// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δos = os_package;
using Δregexp = regexp_package;
using Δruntime = runtime_package;
using testing = testing_package;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

internal static void validateMapLabels(ж<testing.T> Ꮡt, slice<@string> labels) {
    // These are the specific region labels that need get added during the
    // runtime phase. Hence they are the ones that need to be confirmed as
    // present at the time the test reads its own region labels, which
    // is sufficient to validate that the default `decoratemappings` value
    // (enabled) was set early enough in the init process.
    var regions = new map<@string, bool>{
        ["allspans array"u8] = false,
        ["gc bits"u8] = false,
        ["heap"u8] = false,
        ["heap index"u8] = false,
        ["heap reservation"u8] = false,
        ["immortal metadata"u8] = false,
        ["page alloc"u8] = false,
        ["page alloc index"u8] = false,
        ["page summary"u8] = false,
        ["scavenge index"u8] = false
    };
    foreach (var (_, label) in labels) {
        {
            var (_, ok) = regions[label, ꟷ]; if (!ok) {
                Ꮡt.Logf("unexpected region label found: \"%s\""u8, label);
            }
        }
        regions[label] = true;
    }
    foreach (var (label, found) in regions) {
        if (!found) {
            Ꮡt.Logf("region label missing: \"%s\""u8, label);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object decoratemappingsIsOnlyˢ = (@string)"decoratemappings is only supported on Linux"u8;
internal static readonly @string procSelfMapsˢ = "/proc/self/maps"u8;
internal static readonly @string anonGoˢ = "[^[]+ \\[anon: Go: (.+)\\]\n"u8;
internal static readonly object mappingLabelsAbsentAsˢ = (@string)"mapping labels absent as expected"u8;

public static void TestDecorateMappings(ж<testing.T> Ꮡt) {
    if (Δruntime.GOOS != "linux"u8) {
        Ꮡt.Skip(decoratemappingsIsOnlyˢ);
        // /proc/self/maps is also Linux-specific
    }

    slice<@string> labels = default!;
    {
        var (rawMaps, err) = Δos.ReadFile(procSelfMapsˢ); if (err != default!){
            Ꮡt.Fatalf("failed to read /proc/self/maps: %v"u8, err);
        } else {
            Ꮡt.Logf("maps:%s\n"u8, ((@string)rawMaps));
            var matches = Δregexp.MustCompile(anonGoˢ).FindAllSubmatch(rawMaps, -1);
            foreach (var (_, match_pair) in matches) {
                // match_pair consists of the matching substring and the parenthesized group
                labels = append(labels, ((@string)match_pair[1]));
            }
        }
    }
    Ꮡt.Logf("DebugDecorateMappings: %v"u8, runtime_internal_test_package.DebugDecorateMappings.Value);
    if (runtime_internal_test_package.DebugDecorateMappings.Value != 0 && runtime_internal_test_package.SetVMANameSupported()){
        validateMapLabels(Ꮡt, labels);
    } else {
        if (len(labels) > 0){
            Ꮡt.Errorf("unexpected mapping labels present: %v"u8, labels);
        } else {
            Ꮡt.Skip(mappingLabelsAbsentAsˢ);
        }
    }
}

} // end runtime_test_package
