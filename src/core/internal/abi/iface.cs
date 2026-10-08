// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using @unsafe = unsafe_package;

partial class abi_package {

// The first word of every non-empty interface type contains an *ITab.
// It records the underlying concrete type (Type), the interface type it
// is implementing (Inter), and some ancillary information.
//
// allocated in non-garbage-collected memory
partial struct ITab {
    public ж<ΔInterfaceType> Inter;
    public ж<Type> Type;
    public uint32 Hash;     // copy of Type.Hash. Used for type switches.
    public array<uintptr> Fun = new(1); // variable sized. fun[0]==0 means Type does not implement Inter.
}

// EmptyInterface describes the layout of a "interface{}" or a "any."
// These are represented differently than non-empty interface, as the first
// word always points to an abi.Type.
partial struct EmptyInterface {
    public ж<Type> Type;
    public @unsafe.Pointer Data;
}

} // end abi_package
