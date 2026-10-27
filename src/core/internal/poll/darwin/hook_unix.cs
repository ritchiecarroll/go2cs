// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || (js && wasm) || wasip1
namespace go.@internal;

using Δsyscall = syscall_package;

partial class poll_package {

// CloseFunc is used to hook the close call.
public static readonly ж<Func<nint, error>> ᏑCloseFunc = new StaticBox<Func<nint, error>>(Δsyscall.Close);
public static ref Func<nint, error> CloseFunc => ref ((StaticBox<Func<nint, error>>)ᏑCloseFunc).ValueSlot;

// AcceptFunc is used to hook the accept call.
public static readonly ж<Func<nint, (nint, Δsyscall.Sockaddr, error)>> ᏑAcceptFunc = new StaticBox<Func<nint, (nint, Δsyscall.Sockaddr, error)>>(Δsyscall.Accept);
public static ref Func<nint, (nint, Δsyscall.Sockaddr, error)> AcceptFunc => ref ((StaticBox<Func<nint, (nint, Δsyscall.Sockaddr, error)>>)ᏑAcceptFunc).ValueSlot;

} // end poll_package
