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

internal static void validateMapLabels(ж<testing.T> t, slice<@string> labels) {
    // These are the specific region labels that need get added during the
    // runtime phase. Hence they are the ones that need to be confirmed as
    // present at the time the test reads its own region labels, which
    // is sufficient to validate that the default `decoratemappings` value
    // (enabled) was set early enough in the init process.
    var regions = new map<@string, bool>{
        ["allspans array"] = false,
        ["gc bits"] = false,
        ["heap"] = false,
        ["heap index"] = false,
        ["heap reservation"] = false,
        ["immortal metadata"] = false,
        ["page alloc"] = false,
        ["page alloc index"] = false,
        ["page summary"] = false,
        ["scavenge index"] = false
    };
    foreach (var (_, label) in range(labels)) {
        var (_, ok) = regions[label, ꟷ]; if (!ok) {
            t.Logf("unexpected region label found: \"%s\"", label);
        }
        regions[label] = true;
    }
    foreach (var (label, found) in regions) {
        if (!found) {
            t.Logf("region label missing: \"%s\"", label);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object decoratemappingsIsOnlyˢ = (@string)"decoratemappings is only supported on Linux"u8;
internal static ref readonly @string procSelfMapsˢ => ref procSelfMapsˢᵇ;
private static readonly @string procSelfMapsˢᵇ = "/proc/self/maps"u8;
internal static ref readonly @string anonGoˢ => ref anonGoˢᵇ;
private static readonly @string anonGoˢᵇ = "[^[]+ \\[anon: Go: (.+)\\]\n"u8;
internal static readonly object mappingLabelsAbsentAsˢ = (@string)"mapping labels absent as expected"u8;

public static void TestDecorateMappings(ж<testing.T> t) {
    if (Δruntime.GOOS != "linux"u8) {
        t.Skip(decoratemappingsIsOnlyˢ);
        // /proc/self/maps is also Linux-specific
    }

    slice<@string> labels = default;
    var (rawMaps, err) = Δos.ReadFile(procSelfMapsˢ); if (err != nil) {
        t.Fatalf("failed to read /proc/self/maps: %v", err);
    } else {
        t.Logf("maps:%s\n", (@string)rawMaps);
        var matches = Δregexp.MustCompile(anonGoˢ).FindAllSubmatch(rawMaps, -1);
        foreach (var (_, match_pair) in range(matches)) {
            // match_pair consists of the matching substring and the parenthesized group
            labels = append(labels, (@string)match_pair[1]);
        }
    }
    t.Logf("DebugDecorateMappings: %v", runtime_internal_test_package.DebugDecorateMappings.Value);
    if (runtime_internal_test_package.DebugDecorateMappings.Value != 0 && runtime_internal_test_package.SetVMANameSupported()) {
        validateMapLabels(t, labels);
    } else {
        if (len(labels) > 0) {
            t.Errorf("unexpected mapping labels present: %v", labels);
        } else {
            t.Skip(mappingLabelsAbsentAsˢ);
        }
    }
}

} // end runtime_test_package
