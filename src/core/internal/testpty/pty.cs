// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package testpty is a simple pseudo-terminal package for Unix systems,
// implemented by calling C functions via cgo.
namespace go.@internal;

using errors = errors_package;
using fmt = fmt_package;
using os = os_package;
using static testpty_package;

partial class testpty_package {

partial struct PtyError {
    public @string FuncName;
    public @string ErrorString;
    public error Errno;
}

internal static ж<PtyError> ptyError(@string name, error err) {
    return Ꮡ(new PtyError(name, err.Error(), err));
}

} partial class testptyᴛPtyError {

public static @string Error(this ref PtyError e) {
    return fmt.Sprintf("%s: %s"u8, e.FuncName, e.ErrorString);
}

public static error Unwrap(this ref PtyError e) { return e.Errno; }

} partial class testpty_package {

public static error ErrNotSupported = errors.New("testpty.Open not implemented on this platform");

// Open returns a control pty and the name of the linked process tty.
//
// If Open is not implemented on this platform, it returns ErrNotSupported.
public static (ж<os.File> pty, @string processTTY, error err) Open() {
    return open();
}

} // end testpty_package
