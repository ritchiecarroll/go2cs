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

internal static ж<regexp.Regexp> argNameRegexp = regexp.MustCompile(@"((?P<name>[A-Za-z]+)_)?(?P<type>[A-Za-z]+)"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string stackˢ = "stack"u8;
private static readonly @string stringˢ = "string"u8;

public static void TestSpecs(ж<testing.T> Ꮡt) {
    if (tracev2.NumEvents <= 0) {
        Ꮡt.Fatalf("no trace events?"u8);
    }
    if (tracev2.MaxExperimentalEvent < tracev2.MaxEvent) {
        Ꮡt.Fatalf("max experimental event (%d) is < max event (%d)"u8, tracev2.MaxExperimentalEvent, tracev2.MaxEvent);
    }
    var specs = tracev2.Specs();
    foreach (var ev in range<tracev2.EventType>(allEvents().Invoke)) {
        var spec = Ꮡ(specs, ev);
        if ((~spec).Name == ""u8) {
            Ꮡt.Errorf("expected event %d to be defined in specs"u8, ev);
            continue;
        }
        if ((~spec).IsTimedEvent && (~spec).Args[0] != "dt") {
            Ꮡt.Errorf("%s is a timed event, but its first argument is not 'dt'"u8, (~spec).Name);
        }
        if ((~spec).HasData && (~spec).Name != "String"u8 && (~spec).Name != "ExperimentalBatch"u8) {
            Ꮡt.Errorf("%s has data, but is not a special kind of event (unsupported, but could be)"u8, (~spec).Name);
        }
        if ((~spec).IsStack && (~spec).Name != "Stack"u8) {
            Ꮡt.Errorf("%s listed as being a stack, but is not the Stack event (unsupported)"u8, (~spec).Name);
        }
        if ((~spec).IsTimedEvent && len((~spec).Args) > tracev2.MaxTimedEventArgs) {
            Ꮡt.Errorf("%s has too many timed event args: have %d, want %d at most"u8, (~spec).Name, len((~spec).Args), (nint)(tracev2.MaxTimedEventArgs));
        }
        if (ev.Experimental() && (~spec).Experiment == tracev2.NoExperiment) {
            Ꮡt.Errorf("experimental event %s must have an experiment"u8, (~spec).Name);
        }

        // Check arg types.
        foreach (var (_, arg) in (~spec).Args) {
            var matches = argNameRegexp.FindStringSubmatch(arg);
            if (len(matches) == 0) {
                Ꮡt.Errorf("malformed argument %s for event %s"u8, arg, (~spec).Name);
            }
        }

        // Check stacks.
        foreach (var (_, i) in (~spec).StackIDs) {
            if (!strings.HasSuffix((~spec).Args[i], stackˢ)) {
                Ꮡt.Errorf("stack argument listed at %d in %s, but argument name %s does not imply stack type"u8, i, (~spec).Name, (~spec).Args[i]);
            }
        }
        foreach (var (i, arg) in (~spec).Args) {
            if (!strings.HasSuffix((~spec).Args[i], stackˢ)) {
                continue;
            }
            if (!slices.Contains((~spec).StackIDs, i)) {
                Ꮡt.Errorf("found stack argument %s in %s at index %d not listed in StackIDs"u8, arg, (~spec).Name, i);
            }
        }

        // Check strings.
        foreach (var (_, i) in (~spec).StringIDs) {
            if (!strings.HasSuffix((~spec).Args[i], stringˢ)) {
                Ꮡt.Errorf("string argument listed at %d in %s, but argument name %s does not imply string type"u8, i, (~spec).Name, (~spec).Args[i]);
            }
        }
        foreach (var (i, arg) in (~spec).Args) {
            if (!strings.HasSuffix((~spec).Args[i], stringˢ)) {
                continue;
            }
            if (!slices.Contains((~spec).StringIDs, i)) {
                Ꮡt.Errorf("found string argument %s in %s at index %d not listed in StringIDs"u8, arg, (~spec).Name, i);
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
