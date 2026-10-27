// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build windows

// Package sysdll is an internal leaf package that records and reports
// which Windows DLL names are used by Go itself. These DLLs are then
// only loaded from the System32 directory. See Issue 14959.
namespace go.@internal.syscall.windows;

partial class sysdll_package {

// IsSystemDLL reports whether the named dll key (a base name, like
// "foo.dll") is a system DLL which should only be loaded from the
// Windows SYSTEM32 directory.
//
// Filenames are case sensitive, but that doesn't matter because
// the case registered with Add is also the same case used with
// LoadDLL later.
//
// It has no associated mutex and should only be mutated serially
// (currently: during init), and not concurrent with DLL loading.
public static readonly ж<map<@string, bool>> ᏑIsSystemDLL = new StaticBox<map<@string, bool>>(new map<@string, bool>{});
public static ref map<@string, bool> IsSystemDLL => ref ((StaticBox<map<@string, bool>>)ᏑIsSystemDLL).ValueSlot;

// Add notes that dll is a system32 DLL which should only be loaded
// from the Windows SYSTEM32 directory. It returns its argument back,
// for ease of use in generated code.
public static @string Add(@string dll) {
    IsSystemDLL[dll] = true;
    return dll;
}

} // end sysdll_package
