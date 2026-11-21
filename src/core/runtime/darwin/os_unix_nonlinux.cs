// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix && !linux
namespace go;

using static runtime_package;

partial class runtime_package {

} partial class runtime_package {

// sigFromUser reports whether the signal was sent because of a call
// to kill.
//
//go:nosplit
[GoMethodBody("sigFromUser")] internal static bool sigFromUserᴍ(ref sigctxt c) {
    return c.sigcode() == _SI_USER;
}

// sigFromSeccomp reports whether the signal was sent from seccomp.
//
//go:nosplit
[GoMethodBody("sigFromSeccomp")] internal static bool sigFromSeccompᴍ(ref sigctxt c) {
    return false;
}

}
