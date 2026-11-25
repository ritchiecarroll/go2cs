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

// cgoScratchRootMarker is the file a scratch root's seed step writes at the root (the -go2cspath directory, beside
// core), and -cgo -tests refuses a root without it: the repository's src and a deployed root (%GOPATH%/src/go2cs) are
// shared corpora emitted at CGO_ENABLED=0, and neither holds it (P3's SHOULD 2 on 4a, ruled in by COORD, inbox H
// 20261124T232124Z).
const cgoScratchRootMarker = ".go2cs-cgo-scratch-root"

// checkCgoTestsOutput refuses a -cgo -tests conversion whose root is not a seeded scratch root (no
// cgoScratchRootMarker), or whose OUTPUT lies in a repository's corpus (repositoryCorpusOf). The output is judged
// itself, symlinks resolved, never through the root: an explicit -go2cspath elsewhere with the output inside a
// repository's src/core, or a scratch src that is a symlink to one, both passed a root-side check, and with 4c's
// removal both would delete a tracked file (P3's MUST 1 on 4a). The corpus is emitted at CGO_ENABLED=0 by ruling, so a
// cgo-on package goes to a scratch root seeded from it, never into it (stop line S-D).
func checkCgoTestsOutput(options Options, outputPath string) error {
	if !options.parseCgoTargets || !options.convertTests {
		return nil
	}

	if _, err := os.Stat(filepath.Join(options.go2csPath, cgoScratchRootMarker)); err != nil {
		return fmt.Errorf("-cgo -tests converts into a scratch root seeded from src/core, which its seed step marks with %s; the root %q holds none (the repository's src and a deployed root are corpora emitted at CGO_ENABLED=0, and are refused)", cgoScratchRootMarker, options.go2csPath)
	}

	if corpus, inside := repositoryCorpusOf(outputPath); inside {
		return fmt.Errorf("-cgo -tests writes a cgo-on conversion, and %s is in the repository's corpus (%s), which is emitted at CGO_ENABLED=0 by ruling: convert into a scratch root seeded from src/core instead", outputPath, corpus)
	}

	// The output must lie under the MARKED root's core, both resolved: the marker is read at the root, and an explicit
	// root is kept (resolveGo2CSPath), so a marked root with the output in a deployed corpus (%GOPATH%/src/go2cs/core/net,
	// no .git above it) passed both checks above, and 4c would remove its cgo_stub.cs (P3's A1, ruled required).
	if core := filepath.Join(resolveExistingSymlinks(options.go2csPath), "core"); !isPathUnder(resolveExistingSymlinks(outputPath), core) {
		return fmt.Errorf("-cgo -tests writes into the marked scratch root's own core (%s), and %s is not under it: convert into the scratch root's core", core, outputPath)
	}

	return nil
}

// repositoryCorpusOf reports the repository corpus a path lies in, if any: the path is resolved through every symlink
// on it (the longest prefix that exists), and an ancestor named `core` whose grandparent holds git metadata (a .git
// directory, or a worktree's .git file) is a repository's src/core. It is the one predicate both the -cgo -tests output
// check and 4c's per-file removal ask.
func repositoryCorpusOf(path string) (string, bool) {
	resolved := resolveExistingSymlinks(path)

	for dir := resolved; ; {
		// EqualFold: on a case-insensitive volume an output typed .../src/Core/net names the same directory (P3's A2).
		if strings.EqualFold(filepath.Base(dir), "core") {
			if _, err := os.Stat(filepath.Join(filepath.Dir(filepath.Dir(dir)), ".git")); err == nil {
				return dir, true
			}
		}

		parent := filepath.Dir(dir)

		if parent == dir {
			return "", false
		}

		dir = parent
	}
}

// resolveExistingSymlinks is filepath.EvalSymlinks over the longest prefix of path that exists, with the rest joined
// back on: an output directory a conversion has not created yet still resolves through a symlinked parent.
func resolveExistingSymlinks(path string) string {
	absolute, err := filepath.Abs(path)

	if err != nil {
		return path
	}

	var rest []string

	for dir := absolute; ; {
		if resolved, err := filepath.EvalSymlinks(dir); err == nil {
			return filepath.Join(append([]string{resolved}, rest...)...)
		}

		parent := filepath.Dir(dir)

		if parent == dir {
			return absolute
		}

		rest = append([]string{filepath.Base(dir)}, rest...)
		dir = parent
	}
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
