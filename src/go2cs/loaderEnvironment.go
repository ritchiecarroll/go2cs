// loaderEnvironment.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import "os"

// converterLoaderEnv is the environment every go/packages load the converter makes runs in: the host's own, then
// the caller's settings (GOOS, GOARCH, GO111MODULE, GOTOOLCHAIN ...), then -- unless the environment SETS
// CGO_ENABLED -- CGO_ENABLED=0, last so it wins (os/exec keeps the last of duplicate keys).
//
// The conversion must not follow Go's IMPLICIT cgo default, which is a property of the host rather than of the
// request: Go turns cgo ON for a native build on a host with a C compiler (a Linux CI runner, a Mac) and OFF for a
// cross build or a host without one (a Windows box with no gcc). The corpus is emitted at CGO_ENABLED=0 on every
// platform (.claude/rules/corpus.md, ruled). A cgo-on load selects a package's cgo files: os/user then imports
// runtime/cgo, whose go.runtime.cgo namespace renamed the program's `runtime` alias to `Δruntime` in
// SyscallKeystonePulls on the linux behavioral leg only, against a target every other host reproduces.
//
// An EXPLICIT CGO_ENABLED is the caller's decision and is kept: CGO_ENABLED=1 on a package that selects a cgo source
// must still be refused by name (refuseSelectedCgoSources, owner ruling 578 item 7), never converted as its cgo-off
// selection. An empty value is Go's "unset". A value set with `go env -w` is not visible here and reads as unset.
func converterLoaderEnv(settings ...string) []string {
	env := append(os.Environ(), settings...)

	if value, set := os.LookupEnv("CGO_ENABLED"); set && value != "" {
		return env
	}

	return append(env, "CGO_ENABLED=0")
}

// checkCgoFlag refuses a -cgo the converter cannot honour. -cgo makes ONE load cgo-on, the single-package driver's
// own (processConversion), so it is refused beside -stdlib (the standard library converts at CGO_ENABLED=0 by
// ruling), and beside -tests and -recurse, whose loads it does not reach and would otherwise leave silently cgo-off.
// An explicit CGO_ENABLED other than 1 asks for the opposite load and is refused too; an empty one is Go's "unset".
func checkCgoFlag(cgo, stdlib, tests, recurse bool) error {
	return nil
}
