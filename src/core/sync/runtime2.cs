// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !goexperiment.staticlockranking
namespace go;

using @unsafe = unsafe_package;

partial class sync_package {

// Approximation of notifyList in runtime/sema.go. Size and alignment must
// agree.
partial struct notifyList {
    internal uint32 wait;
    internal uint32 notify;
    internal uintptr @lock; // key field of the mutex
    internal @unsafe.Pointer head;
    internal @unsafe.Pointer tail;
}

} // end sync_package
