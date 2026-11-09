// zbootstrap_darwin_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// internal/buildcfg's defaultGOARCH for the darwin flavour: Go's own `const defaultGOARCH = runtime.GOARCH`
// (zbootstrap.go), kept as a READ of runtime.GOARCH rather than the "amd64" the conversion folded it to.
//
// The darwin runtime reads its GOARCH from the process architecture (runtime/darwin/goarch_darwin_impl.cs).
// Folded, this default would stay "amd64" on Apple silicon, so go/build's default context (Default.GOARCH,
// from buildcfg.GOARCH) would disagree with runtime.GOARCH. Its `runtime.GOARCH == c.GOARCH` test would
// then turn CgoEnabled off, where Go's own arm64 binary, whose two values agree, leaves it on. The converter
// displaces the const for darwin (manualConversionConsts), and this member restores Go's relation.

[module: go.GoManualConversion]

namespace go.@internal;

partial class buildcfg_package {

internal static ref readonly @string defaultGOARCH => ref runtime_package.GOARCH;

} // end buildcfg_package
