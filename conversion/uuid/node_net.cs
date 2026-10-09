// Copyright 2017 Google Inc.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.github.com.google;

using net = net_package;

partial class uuid_package {

internal static slice<net.Interface> interfaces;

internal static (@string, slice<byte>) getHardwareInterface(@string name) {
    if (interfaces == default!) {
        error err = default!;
        (interfaces, err) = net.Interfaces();
        if (err != default!) {
            return ("", default!);
        }
    }
    foreach (var (_, ifs) in interfaces) {
        if (len(ifs.HardwareAddr) >= 6 && (name == ""u8 || name == ifs.Name)) {
            return (ifs.Name, ifs.HardwareAddr);
        }
    }
    return ("", default!);
}

} // end uuid_package
