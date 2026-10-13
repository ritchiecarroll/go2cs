// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using Δio = io_package;
using Δos = os_package;
using Δsync = sync_package;
using time = time_package;
using static go.sync_internal_test_package;

partial class sync_test_package {

internal static ж<Δsync.Pool> ᏑbufPool = new StandardBox<Δsync.Pool>(new Δsync.Pool(
    New: () => {
        // The Pool's New function should generally only return pointer
        // types, since a pointer can be put into the return interface
        // value without an allocation:
        return @new<bytes.Buffer>();
    }
));
internal static ref Δsync.Pool bufPool => ref ᏑbufPool.Value;

// timeNow is a fake version of time.Now for tests.
internal static time.Time timeNow() {
    return time.Unix(1136214245, 0);
}

public static void Log(Δio.Writer w, @string key, @string val) {
    var b = ᏑbufPool.Get()._<ж<bytes.Buffer>>();
    b.Reset();
    // Replace this with time.Now() in a real logger.
    b.WriteString(timeNow().UTC().Format(time.RFC3339));
    b.WriteByte((rune)' ');
    b.WriteString(key);
    b.WriteByte((rune)'=');
    b.WriteString(val);
    w.Write(b.Bytes());
    ᏑbufPool.Put(b.OrTypedNil());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pathˢ = "path"u8;
internal static readonly @string searchQFlowersˢ = "/search?q=flowers"u8;

public static void ExamplePool() {
    Log(new Δos.FileжWriter(Δos.Stdout), pathˢ, searchQFlowersˢ);
    // Output: 2006-01-02T15:04:05Z path=/search?q=flowers
}

} // end sync_test_package
