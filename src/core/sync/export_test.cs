// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using go.sync;
using static go.sync_package;

partial class sync_internal_test_package {

// Export for testing.
public static readonly ж<Action<ж<uint32>>> ᏑRuntime_Semacquire = new StaticBox<Action<ж<uint32>>>(runtime_Semacquire);
public static ref Action<ж<uint32>> Runtime_Semacquire => ref ((StaticBox<Action<ж<uint32>>>)ᏑRuntime_Semacquire).ValueSlot;

public static readonly ж<Action<ж<uint32>, bool, nint>> ᏑRuntime_Semrelease = new StaticBox<Action<ж<uint32>, bool, nint>>(runtime_Semrelease);
public static ref Action<ж<uint32>, bool, nint> Runtime_Semrelease => ref ((StaticBox<Action<ж<uint32>, bool, nint>>)ᏑRuntime_Semrelease).ValueSlot;

public static readonly ж<Func<nint>> ᏑRuntime_procPin = new StaticBox<Func<nint>>(runtime_procPin);
public static ref Func<nint> Runtime_procPin => ref ((StaticBox<Func<nint>>)ᏑRuntime_procPin).ValueSlot;

public static readonly ж<Action> ᏑRuntime_procUnpin = new StaticBox<Action>(runtime_procUnpin);
public static ref Action Runtime_procUnpin => ref ((StaticBox<Action>)ᏑRuntime_procUnpin).ValueSlot;

// PoolDequeue exports an interface for pollDequeue testing.
public partial interface PoolDequeue {
    bool PushHead(any val);
    (any, bool) PopHead();
    (any, bool) PopTail();
}

public static PoolDequeue NewPoolDequeue(nint n) {
    var d = Ꮡ(new poolDequeue(
        vals: new slice<global::go.sync_package.eface>(n)
    ));
    // For testing purposes, set the head and tail indexes close
    // to wrapping around.
    d.of(global::go.sync_package.poolDequeue.ᏑheadTail).Store(d.pack((uint32)(4294967296L - 500), (uint32)(4294967296L - 500)));
    return new sync_internal_test_package.sync_poolDequeueжPoolDequeue(d);
}

internal static bool PushHead(this ж<global::go.sync_package.poolDequeue> Ꮡd, any val) {
    return Ꮡd.pushHead(val);
}

internal static (any, bool) PopHead(this ж<global::go.sync_package.poolDequeue> Ꮡd) {
    return Ꮡd.popHead();
}

internal static (any, bool) PopTail(this ж<global::go.sync_package.poolDequeue> Ꮡd) {
    return Ꮡd.popTail();
}

public static PoolDequeue NewPoolChain() {
    return new sync_internal_test_package.sync_poolChainжPoolDequeue(@new<global::go.sync_package.poolChain>());
}

internal static bool PushHead(this ж<global::go.sync_package.poolChain> Ꮡc, any val) {
    Ꮡc.pushHead(val);
    return true;
}

internal static (any, bool) PopHead(this ref global::go.sync_package.poolChain c) {
    return c.popHead();
}

internal static (any, bool) PopTail(this ж<global::go.sync_package.poolChain> Ꮡc) {
    return Ꮡc.popTail();
}

} // end sync_internal_test_package
