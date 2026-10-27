// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Package syslist stores tables of OS and ARCH names that are
// (or at one point were) acceptable build targets.
namespace go.@internal;

partial class syslist_package {

// Note that this file is read by internal/goarch/gengoarch.go and by
// internal/goos/gengoos.go. If you change this file, look at those
// files as well.

// KnownOS is the list of past, present, and future known GOOS values.
// Do not remove from this list, as it is used for filename matching.
// If you add an entry to this list, look at UnixOS, below.
[GoLazy] partial class lazyᴛKnownOS { public static readonly ж<map<@string, bool>> ᏑKnownOS = new StaticBox<map<@string, bool>>(new map<@string, bool>{
    ["aix"u8] = true,
    ["android"u8] = true,
    ["darwin"u8] = true,
    ["dragonfly"u8] = true,
    ["freebsd"u8] = true,
    ["hurd"u8] = true,
    ["illumos"u8] = true,
    ["ios"u8] = true,
    ["js"u8] = true,
    ["linux"u8] = true,
    ["nacl"u8] = true,
    ["netbsd"u8] = true,
    ["openbsd"u8] = true,
    ["plan9"u8] = true,
    ["solaris"u8] = true,
    ["wasip1"u8] = true,
    ["windows"u8] = true,
    ["zos"u8] = true
}); }
public static ref map<@string, bool> KnownOS => ref ((StaticBox<map<@string, bool>>)ᏑKnownOS).ValueSlot;

// UnixOS is the set of GOOS values matched by the "unix" build tag.
// This is not used for filename matching.
// This list also appears in cmd/dist/build.go.
[GoLazy] partial class lazyᴛUnixOS { public static readonly ж<map<@string, bool>> ᏑUnixOS = new StaticBox<map<@string, bool>>(new map<@string, bool>{
    ["aix"u8] = true,
    ["android"u8] = true,
    ["darwin"u8] = true,
    ["dragonfly"u8] = true,
    ["freebsd"u8] = true,
    ["hurd"u8] = true,
    ["illumos"u8] = true,
    ["ios"u8] = true,
    ["linux"u8] = true,
    ["netbsd"u8] = true,
    ["openbsd"u8] = true,
    ["solaris"u8] = true
}); }
public static ref map<@string, bool> UnixOS => ref ((StaticBox<map<@string, bool>>)ᏑUnixOS).ValueSlot;

// KnownArch is the list of past, present, and future known GOARCH values.
// Do not remove from this list, as it is used for filename matching.
[GoLazy] partial class lazyᴛKnownArch { public static readonly ж<map<@string, bool>> ᏑKnownArch = new StaticBox<map<@string, bool>>(new map<@string, bool>{
    ["386"u8] = true,
    ["amd64"u8] = true,
    ["amd64p32"u8] = true,
    ["arm"u8] = true,
    ["armbe"u8] = true,
    ["arm64"u8] = true,
    ["arm64be"u8] = true,
    ["loong64"u8] = true,
    ["mips"u8] = true,
    ["mipsle"u8] = true,
    ["mips64"u8] = true,
    ["mips64le"u8] = true,
    ["mips64p32"u8] = true,
    ["mips64p32le"u8] = true,
    ["ppc"u8] = true,
    ["ppc64"u8] = true,
    ["ppc64le"u8] = true,
    ["riscv"u8] = true,
    ["riscv64"u8] = true,
    ["s390"u8] = true,
    ["s390x"u8] = true,
    ["sparc"u8] = true,
    ["sparc64"u8] = true,
    ["wasm"u8] = true
}); }
public static ref map<@string, bool> KnownArch => ref ((StaticBox<map<@string, bool>>)ᏑKnownArch).ValueSlot;

} // end syslist_package
