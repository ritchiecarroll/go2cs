// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

partial class unix_package {

// GRND_NONBLOCK means return EAGAIN rather than blocking.
public static GetRandomFlag GRND_NONBLOCK => 0x0001;

// GRND_RANDOM means use the /dev/random pool instead of /dev/urandom.
public static GetRandomFlag GRND_RANDOM => 0x0002;

} // end unix_package
