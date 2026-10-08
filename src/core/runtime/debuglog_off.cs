// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !debuglog
global using dlogger = go.runtime_package.dloggerFake;

namespace go;

partial class runtime_package {

internal const bool dlogEnabled = false;

internal static dloggerFake dlog1() {
    return dlogFake();
}

partial struct dlogPerM {
}

internal static ж<dloggerImpl> getCachedDlogger() {
    return default!;
}

internal static bool putCachedDlogger(ref dloggerImpl l) {
    return false;
}

} // end runtime_package
