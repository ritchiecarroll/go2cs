// export_impl_test.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-owned bodies for runtime's export_test.go -- the TEST-file companion (the `*_impl_test.cs`
// convention reflect's export_impl_test.cs set: the `_test.cs` suffix keeps it out of the production
// csproj, and testConversion globs it into runtime.tests.csproj).
//
// SemNwait (export_test.go) reads semtable.rootFor(addr).nwait, the semaRoot treap's waiter count. The
// runtime's semaphore is RuntimeSemaphore now (sema_impl.cs) and nothing maintains the treap, so the
// converted read would answer 0 forever and TestSemaHandoff's `for SemNwait(&sema) == 0 { Gosched() }`
// would spin for good. The same question is answered from the queue that does exist.
//
// Hand-owned (no export_impl_test.go exists, so a reconvert never regenerates this file).

namespace go;

using static global::go.runtime_package;

partial class runtime_internal_test_package
{
    public static uint32 SemNwait(ж<uint32> Ꮡaddr) => GoSemaWaiters(Ꮡaddr);
}
