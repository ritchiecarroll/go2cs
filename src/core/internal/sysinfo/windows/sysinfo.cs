// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sysinfo implements high level hardware information gathering
// that can be used for debugging or information purposes.
namespace go.@internal;

using cpu = go.@internal.cpu_package;
using sync = go.sync_package;
using go;
using go.@internal;

partial class sysinfo_package {

public static readonly ж<Func<@string>> ᏑCPUName = new StaticBox<Func<@string>>(sync.OnceValue(@string () => {
    {
        @string name = cpu.Name(); if (name != ""u8) {
            return name;
        }
    }

    {
        @string name = osCPUInfoName(); if (name != ""u8) {
            return name;
        }
    }

    return ""u8;
}));
public static ref Func<@string> CPUName => ref ((StaticBox<Func<@string>>)ᏑCPUName).ValueSlot;

} // end sysinfo_package
