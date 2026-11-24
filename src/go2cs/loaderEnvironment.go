// loaderEnvironment.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"os"
	"path/filepath"
	"strings"
)

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

// cgoOnLoad reports whether a load ran cgo-on, which is what puts the cgo build tag in force for the files it
// selected: under -cgo (whose offline test seam sets the tag by hand at CGO_ENABLED=0), or under a CGO_ENABLED=1 the
// load's own environment carries. The last setting of a key is the one a child process is given.
func cgoOnLoad(options Options, env []string) bool {
	if options.parseCgoTargets {
		return true
	}

	enabled := ""

	for _, entry := range env {
		if value, found := strings.CutPrefix(entry, "CGO_ENABLED="); found {
			enabled = value
		}
	}

	return enabled == "1"
}

// packageTestLoaderEnv is converterLoaderEnv for a load of the CONVERTED package's own test variants: cgo-on under -cgo,
// as its production load is (conversionDriver.go), so the variants select the files the production load selected
// (cgo Phase 3, increment 3.4 seat 4a: DESIGN-cgo-phase3-marshaling section 19). Only the package being converted
// loads this way. A load of any OTHER package (linknameOperations' target closure) keeps converterLoaderEnv: no other
// package loads cgo-on (stop line S-G).
func packageTestLoaderEnv(options Options, settings ...string) []string {
	env := converterLoaderEnv(settings...)

	if options.parseCgoTargets {
		env = append(env, "CGO_ENABLED=1")
	}

	return env
}

// cgoOracleEnv is the extra environment of the -tests oracle (`go test -json`) under -cgo: cgo-on, so Go's own run
// selects the cgo files the conversion selected, whatever the host's implicit default is. Empty otherwise: the oracle
// keeps the environment it always had.
func cgoOracleEnv(options Options) []string {
	if options.parseCgoTargets {
		return []string{"CGO_ENABLED=1"}
	}

	return nil
}

// checkCgoTestsOutput refuses a -cgo -tests conversion whose output lies in the REPOSITORY's corpus: the corpus is
// emitted at CGO_ENABLED=0 by ruling, so a cgo-on package goes to a scratch root seeded from it, never into src/core
// (stop line S-D). The corpus is the core directory of the go2cs root in use; it is the repository's when the root's
// parent holds git metadata (a .git directory, or a worktree's .git file). A scratch root seeded by copying src/core
// holds none.
func checkCgoTestsOutput(options Options, outputPath string) error {
	if !options.parseCgoTargets || !options.convertTests || options.go2csPath == "" {
		return nil
	}

	root, err := filepath.Abs(options.go2csPath)

	if err != nil {
		return nil
	}

	output, err := filepath.Abs(outputPath)

	if err != nil {
		return nil
	}

	if _, statErr := os.Stat(filepath.Join(root, "..", ".git")); statErr != nil {
		return nil
	}

	if !isPathUnder(output, filepath.Join(root, "core")) {
		return nil
	}

	return fmt.Errorf("-cgo -tests writes a cgo-on conversion, and %s is in the repository's corpus (%s), which is emitted at CGO_ENABLED=0 by ruling: convert into a scratch root seeded from src/core instead", output, filepath.Join(root, "core"))
}

// checkCgoFlag refuses a -cgo the converter cannot honour. -cgo makes the conversion's loads cgo-on: the
// single-package driver's own (processConversion), and under -recurse the module driver's closure load and each
// package's own (a cgo package against the go.* packages, INC 1). It is refused beside -stdlib (the standard library
// converts at CGO_ENABLED=0 by ruling), and beside -tests, whose loads it does not reach and would otherwise leave
// silently cgo-off.
// An explicit CGO_ENABLED other than 1 asks for the opposite load and is refused too; an empty one is Go's "unset".
func checkCgoFlag(cgo, stdlib, tests bool) error {
	if !cgo {
		return nil
	}

	switch {
	case stdlib:
		return fmt.Errorf("-cgo cannot be combined with -stdlib: the standard library converts at CGO_ENABLED=0 by ruling")
	}

	if value, set := os.LookupEnv("CGO_ENABLED"); set && value != "" && value != "1" {
		return fmt.Errorf("-cgo asks for a cgo-on load, and the environment sets CGO_ENABLED=%s: unset it or drop -cgo", value)
	}

	return nil
}
