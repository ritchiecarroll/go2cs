// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using Δsyscall = syscall_package;

partial class poll_package {

// CloseFunc is used to hook the close call.
public static readonly ж<Func<Δsyscall.Handle, error>> ᏑCloseFunc = new StaticBox<Func<Δsyscall.Handle, error>>(Δsyscall.Closesocket);
public static ref Func<Δsyscall.Handle, error> CloseFunc => ref ((StaticBox<Func<Δsyscall.Handle, error>>)ᏑCloseFunc).ValueSlot;

// AcceptFunc is used to hook the accept call.
public static readonly ж<Func<Δsyscall.Handle, Δsyscall.Handle, ж<byte>, uint32, uint32, uint32, ж<uint32>, ж<Δsyscall.Overlapped>, error>> ᏑAcceptFunc = new StaticBox<Func<Δsyscall.Handle, Δsyscall.Handle, ж<byte>, uint32, uint32, uint32, ж<uint32>, ж<Δsyscall.Overlapped>, error>>(Δsyscall.AcceptEx);
public static ref Func<Δsyscall.Handle, Δsyscall.Handle, ж<byte>, uint32, uint32, uint32, ж<uint32>, ж<Δsyscall.Overlapped>, error> AcceptFunc => ref ((StaticBox<Func<Δsyscall.Handle, Δsyscall.Handle, ж<byte>, uint32, uint32, uint32, ж<uint32>, ж<Δsyscall.Overlapped>, error>>)ᏑAcceptFunc).ValueSlot;

// ConnectExFunc is used to hook the ConnectEx call.
public static readonly ж<Func<Δsyscall.Handle, Δsyscall.Sockaddr, ж<byte>, uint32, ж<uint32>, ж<Δsyscall.Overlapped>, error>> ᏑConnectExFunc = new StaticBox<Func<Δsyscall.Handle, Δsyscall.Sockaddr, ж<byte>, uint32, ж<uint32>, ж<Δsyscall.Overlapped>, error>>(Δsyscall.ConnectEx);
public static ref Func<Δsyscall.Handle, Δsyscall.Sockaddr, ж<byte>, uint32, ж<uint32>, ж<Δsyscall.Overlapped>, error> ConnectExFunc => ref ((StaticBox<Func<Δsyscall.Handle, Δsyscall.Sockaddr, ж<byte>, uint32, ж<uint32>, ж<Δsyscall.Overlapped>, error>>)ᏑConnectExFunc).ValueSlot;

} // end poll_package
