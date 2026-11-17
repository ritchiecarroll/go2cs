// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build windows || plan9 || (js && wasm) || wasip1
namespace go.@internal;

using errors = errors_package;
using fs = io.fs_package;
using os = os_package;
using io;

partial class testenv_package {

// Sigquit is the signal to send to kill a hanging subprocess.
// On Unix we send SIGQUIT, but on non-Unix we only have os.Kill.
public static os.Signal Sigquit = os.Kill;

internal static bool syscallIsNotSupported(error err) {
    return errors.Is(err, fs.ErrPermission) || errors.Is(err, errors.ErrUnsupported);
}

} // end testenv_package
