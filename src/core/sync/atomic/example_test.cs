// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.sync;

using sync = sync_package;
using atomic = go.sync.atomic_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.sync;

partial class atomic_test_package {

internal static map<@string, @string> loadConfig() {
    return new map<@string, @string>();
}

internal static channel<nint> requests() {
    return new channel<nint>(0);
}

// The following example shows how to use Value for periodic program config updates
// and propagation of the changes to worker goroutines.
public static partial void ExampleValue_config() {
    ref var config = ref heap(new atomic.Value(), out var Ꮡconfig);                    // holds current server configuration
    // Create initial config value and store into config.
    Ꮡconfig.Store(loadConfig());
    goǃ(() => {
        // Reload config every 10 seconds
        // and update config value with the new version.
        while (ᐧ) {
            time.Sleep((time.Duration)(10000000000L));
            Ꮡconfig.Store(loadConfig());
        }
    });
    // Create worker goroutines that handle incoming requests
    // using the latest config value.
    for (nint i = 0; i < 10; i++) {
        goǃ(() => {
            foreach (var r in requests()) {
                var c = Ꮡconfig.Load();
                // Handle request r using config c.
                _ = r;
                _ = c;
            }
        });
    }
}

[GoLocalName("Map")] [GoType("map[@string, @string]")] internal partial struct ExampleValue_readMostly_Map;

// The following example shows how to maintain a scalable frequently read,
// but infrequently updated data structure using copy-on-write idiom.
public static void ExampleValue_readMostly() {
    ref var m = ref heap(new atomic.Value(), out var Ꮡm);
    Ꮡm.Store(new ExampleValue_readMostly_Map(0));
    ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);                    // used only by writers
    // read function can be used to read the data without further synchronization
    var read = @string (@string key) => {
        var m1 = Ꮡm.Load()._<ExampleValue_readMostly_Map>();
        return m1[key];
    };
    // insert function can be used to update the data without further synchronization
    var insert = (@string key, @string val) => {
        GoFrame ᒐ = default;
        try {
            Ꮡmu.Lock(); // synchronize with other potential writers
            defer(Ꮡmu.Unlock, ref ᒐ);
            var m1 = Ꮡm.Load()._<ExampleValue_readMostly_Map>(); // load current value of the data structure
            var m2 = new ExampleValue_readMostly_Map(0); // create a new value
            foreach (var (k, v) in m1) {
                m2[k] = v; // copy all data from the current object to the new one
            }
            m2[key] = val; // do the update that we need
            Ꮡm.Store(m2); // atomically replace the current object with the new one
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    };
    // At this point all new readers start working with the new version.
    // The old version will be garbage collected once the existing readers
    // (if any) are done with it.
    _ = read;
    _ = insert;
}

} // end atomic_test_package
