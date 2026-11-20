// Copyright 2021 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using atomic = go.sync.atomic_package;
using go.sync;

partial class libc_package {

internal static readonly ж<int32> Ꮡ__sync_synchronize_dummy = new StaticBox<int32>(default(int32));
internal static ref int32 __sync_synchronize_dummy => ref ((StaticBox<int32>)Ꮡ__sync_synchronize_dummy).ValueSlot;

public static partial void X__sync_synchronize(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    atomic.StoreInt32(Ꮡ__sync_synchronize_dummy, atomic.LoadInt32(Ꮡ__sync_synchronize_dummy) + 1);
}

} // end libc_package
