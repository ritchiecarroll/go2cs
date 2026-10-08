// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix && !linux
namespace go;

partial class runtime_package {

// sigFromUser reports whether the signal was sent because of a call
// to kill.
//
//go:nosplit
internal static bool sigFromUser(this ref sigctxt c) {
    return c.sigcode() == _SI_USER;
}

// sigFromSeccomp reports whether the signal was sent from seccomp.
//
//go:nosplit
internal static bool sigFromSeccomp(this ref sigctxt c) {
    return false;
}

} // end runtime_package
