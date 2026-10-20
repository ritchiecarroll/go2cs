// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using sys = @internal.runtime.sys_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using @internal.runtime;

partial class runtime_package {

//go:linkname dit_setEnabled crypto/subtle.setDITEnabled
public static bool dit_setEnabled() {
    var g = getg();
    g.Value.ditWanted = true;
    g.Value.m.Value.ditEnabled = true;
    return sys.EnableDIT();
}

//go:linkname dit_setDisabled crypto/subtle.setDITDisabled
public static void dit_setDisabled() {
    var g = getg();
    g.Value.ditWanted = false;
    g.Value.m.Value.ditEnabled = false;
    sys.DisableDIT();
}

} // end runtime_package
