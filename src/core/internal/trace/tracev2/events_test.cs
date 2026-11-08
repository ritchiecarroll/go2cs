// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace;

using tracev2 = go.@internal.trace.tracev2_package;
using iter = iter_package;
using regexp = regexp_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using go.@internal.trace;

partial class tracev2_test_package {

internal static ж<regexp.Regexp> argNameRegexp = regexp.MustCompile(@"((?P<name>[A-Za-z]+)_)?(?P<type>[A-Za-z]+)");

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string stackˢ => ref stackˢᵇ;
private static readonly @string stackˢᵇ = "stack"u8;
private static ref readonly @string stringˢ => ref stringˢᵇ;
private static readonly @string stringˢᵇ = "string"u8;

public static void TestSpecs(ж<testing.T> t) {
    if (tracev2.NumEvents <= 0) {
        t.Fatalf("no trace events?");
    }
    if (tracev2.MaxExperimentalEvent < tracev2.MaxEvent) {
        t.Fatalf("max experimental event (%d) is < max event (%d)", tracev2.MaxExperimentalEvent, tracev2.MaxEvent);
    }
    var specs = tracev2.Specs();
    foreach (var ev in range<tracev2.EventType>(allEvents().Invoke)) {
        var spec = Ꮡ(specs, ev);
        if (spec.Name == ""u8) {
            t.Errorf("expected event %d to be defined in specs", ev);
            continue;
        }
        if (spec.IsTimedEvent && spec.Args[0] != "dt") {
            t.Errorf("%s is a timed event, but its first argument is not 'dt'", spec.Name);
        }
        if (spec.HasData && spec.Name != "String"u8 && spec.Name != "ExperimentalBatch"u8) {
            t.Errorf("%s has data, but is not a special kind of event (unsupported, but could be)", spec.Name);
        }
        if (spec.IsStack && spec.Name != "Stack"u8) {
            t.Errorf("%s listed as being a stack, but is not the Stack event (unsupported)", spec.Name);
        }
        if (spec.IsTimedEvent && len(spec.Args) > tracev2.MaxTimedEventArgs) {
            t.Errorf("%s has too many timed event args: have %d, want %d at most", spec.Name, len(spec.Args), (nint)(tracev2.MaxTimedEventArgs));
        }
        if (ev.Experimental() && spec.Experiment == tracev2.NoExperiment) {
            t.Errorf("experimental event %s must have an experiment", spec.Name);
        }

        // Check arg types.
        foreach (var (_, arg) in range(spec.Args)) {
            var matches = argNameRegexp.FindStringSubmatch(arg);
            if (len(matches) == 0) {
                t.Errorf("malformed argument %s for event %s", arg, spec.Name);
            }
        }

        // Check stacks.
        foreach (var (_, i) in range(spec.StackIDs)) {
            if (!strings.HasSuffix(spec.Args[i], stackˢ)) {
                t.Errorf("stack argument listed at %d in %s, but argument name %s does not imply stack type", i, spec.Name, spec.Args[i]);
            }
        }
        foreach (var (i, arg) in range(spec.Args)) {
            if (!strings.HasSuffix(spec.Args[i], stackˢ)) {
                continue;
            }
            if (!slices.Contains(spec.StackIDs, i)) {
                t.Errorf("found stack argument %s in %s at index %d not listed in StackIDs", arg, spec.Name, i);
            }
        }

        // Check strings.
        foreach (var (_, i) in range(spec.StringIDs)) {
            if (!strings.HasSuffix(spec.Args[i], stringˢ)) {
                t.Errorf("string argument listed at %d in %s, but argument name %s does not imply string type", i, spec.Name, spec.Args[i]);
            }
        }
        foreach (var (i, arg) in range(spec.Args)) {
            if (!strings.HasSuffix(spec.Args[i], stringˢ)) {
                continue;
            }
            if (!slices.Contains(spec.StringIDs, i)) {
                t.Errorf("found string argument %s in %s at index %d not listed in StringIDs", arg, spec.Name, i);
            }
        }
    }
}

internal static iter.Seq<tracev2.EventType> allEvents() {
    return (Func<tracev2.EventType, bool> yield) => {
        for (var ev = (tracev2.EventType)(tracev2.EvNone + 1); ev < tracev2.NumEvents; ev++) {
            if (!yield(ev)) {
                return;
            }
        }
        for (var ev = (tracev2.EventType)(tracev2.MaxEvent + 1); ev < tracev2.NumExperimentalEvents; ev++) {
            if (!yield(ev)) {
                return;
            }
        }
    };
}

} // end tracev2_test_package
