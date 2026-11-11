// cgoRoute.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

// cgo Phase 2 increment 5 (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.20): the converter's cgo route. Under -cgo a
// package converts when every C use it makes is in Phase 2's set, and is otherwise refused, before anything is
// written, with each use that is not and the phase that owns it.

// cgoOfflineTrees is set only by tests (cgoDriver_test.go): the package directory holds checked-in cmd/cgo output as
// plain files, which load at CGO_ENABLED=0 with runtime/cgo's Incomplete overlaid, and whose roles are read from
// content alone. A host with no C compiler cannot make cmd/cgo's output, so this is how the route is tested there.
var cgoOfflineTrees bool

// cgoCaptureOverride is set only by tests: it answers the library capture, each symbol's load-time name and the C
// toolchain's version line, in place of a C toolchain.
var cgoCaptureOverride func(symbols []string) ([]cgoSymbolLibrary, string, error)
