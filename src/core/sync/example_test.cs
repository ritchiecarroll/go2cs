// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using Δos = os_package;
using Δsync = sync_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.sync_internal_test_package;

partial class sync_test_package {

[GoType] partial struct httpPkg {
}

internal static void Get(this httpPkg _, @string url) {
}

internal static httpPkg http;

// This example fetches several URLs concurrently,
// using a WaitGroup to block until all the fetches are complete.
[MethodImpl(MethodImplOptions.NoInlining)] public static void ExampleWaitGroup() {
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    slice<@string> urls = new @string[]{
        "http://www.golang.org/"u8,
        "http://www.google.com/"u8,
        "http://www.example.com/"u8
    }.slice();
    foreach (var (_, url) in urls) {
        // Increment the WaitGroup counter.
        Ꮡwg.Add(1);
        // Launch a goroutine to fetch the URL.
        goǃ((@string urlΔ1) => {
            GoFrame ᒐ = default;
            try {
                // Decrement the counter when the goroutine completes.
                defer(Ꮡwg.Done, ref ᒐ);
                // Fetch the URL.
                http.Get(urlΔ1);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, url);
    }
    // Wait for all HTTP fetches to complete.
    Ꮡwg.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object onlyOnceˢ = (@string)"Only once"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void ExampleOnce() {
    ref var once = ref heap(new Δsync.Once(), out var Ꮡonce);
    var onceBody = () => {
        fmt.Println(onlyOnceˢ);
    };
    var done = new channel<bool>(0);
    for (nint i = 0; i < 10; i++) {
        var doneʗ1 = done;
        var onceBodyʗ1 = onceBody;
        goǃ(() => {
            Ꮡonce.Do(onceBodyʗ1);
            doneʗ1.ᐸꟷ(true);
        });
    }
    for (nint i = 0; i < 10; i++) {
        ᐸꟷ(done);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object computedOnceˢ = (@string)"Computed once:"u8;
internal static readonly object wantˢ = (@string)"want"u8;
internal static readonly object gotˢ = (@string)"got"u8;

// Output:
// Only once

// This example uses OnceValue to perform an "expensive" computation just once,
// even when used concurrently.
[MethodImpl(MethodImplOptions.NoInlining)] public static void ExampleOnceValue() {
    var once = Δsync.OnceValue(nint () => {
        nint sum = 0;
        for (nint i = 0; i < 1000; i++) {
            sum += i;
        }
        fmt.Println(computedOnceˢ, sum);
        return sum;
    });
    var done = new channel<bool>(0);
    for (nint i = 0; i < 10; i++) {
        var doneʗ1 = done;
        var onceʗ1 = once;
        goǃ(() => {
            const nint want = 499500;
            nint got = onceʗ1();
            if (got != want) {
                fmt.Println(wantˢ, (nint)(want), gotˢ, got);
            }
            doneʗ1.ᐸꟷ(true);
        });
    }
    for (nint i = 0; i < 10; i++) {
        ᐸꟷ(done);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readingFileOnceˢ = (@string)"Reading file once"u8;
internal static readonly @string exampleTestGoˢ = "example_test.go"u8;
internal static readonly object errorˢ = (@string)"error:"u8;

// Output:
// Computed once: 499500

// This example uses OnceValues to read a file just once.
[MethodImpl(MethodImplOptions.NoInlining)] public static void ExampleOnceValues() {
    var once = Δsync.OnceValues((slice<byte>, error) () => {
        fmt.Println(readingFileOnceˢ);
        return Δos.ReadFile(exampleTestGoˢ);
    });
    var done = new channel<bool>(0);
    for (nint i = 0; i < 10; i++) {
        var doneʗ1 = done;
        var onceʗ1 = once;
        goǃ(() => {
            var (data, err) = onceʗ1();
            if (err != default!) {
                fmt.Println(errorˢ, err);
            }
            _ = data; // Ignore the data for this example
            doneʗ1.ᐸꟷ(true);
        });
    }
    for (nint i = 0; i < 10; i++) {
        ᐸꟷ(done);
    }
}

// Output:
// Reading file once

} // end sync_test_package
