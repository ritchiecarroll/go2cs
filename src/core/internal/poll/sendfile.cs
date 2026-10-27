// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class poll_package {

[GoLazy] partial class lazyᴛTestHookDidSendFile { public static readonly ж<Action<ж<FD>, uintptr, int64, error, bool>> ᏑTestHookDidSendFile = new StaticBox<Action<ж<FD>, uintptr, int64, error, bool>>((ж<FD> dstFD, uintptr src, int64 written, error err, bool handled) => {
}); }
public static ref Action<ж<FD>, uintptr, int64, error, bool> TestHookDidSendFile => ref ((StaticBox<Action<ж<FD>, uintptr, int64, error, bool>>)ᏑTestHookDidSendFile).ValueSlot;

} // end poll_package
