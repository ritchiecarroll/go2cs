// testsCorpusArch.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"path/filepath"
)

// THE CORPUS FLAVOUR A TARGET IS SERVED BY. The committed standard-library corpus is converted once per
// target of platformUniverse (platformLattice.go), and each of those conversions is a FLAVOUR. A target
// of the universe is its own flavour; any other target is served by the FIRST universe target of its
// GOOS (darwin/arm64 by darwin/amd64, the one flavour published under both mac RIDs); a GOOS with no
// universe target has no flavour, and a value that is not os/arch names none. Every -tests and harness
// reader of the corpus's architecture derives it here, so a target joins the readers through
// platformUniverse alone (S1b, 2026-11-09).
//
// History: the rule was one constant, corpusArch = "amd64", while every flavour was converted at amd64.
// It was introduced for the -tests pin below: on an arm64 host the -platforms default re-emitted
// hash/crc32 for darwin/arm64, crc32_arm64.go landed beside the committed crc32_amd64.cs and the test
// host failed with CS0111 x6 (darwin row pilot, run 37112360853, 2026-10-03).

// corpusFlavourOf answers the corpus flavour a target is served by under a lattice: the target itself
// when the universe includes it, else the first universe target of its GOOS, else the target unchanged
// (a GOOS the universe does not carry, or a malformed target).
func corpusFlavourOf(lattice *platformLattice, target string) string {
	if lattice.includes(target) || len(archOfTarget(target)) == 0 {
		return target
	}

	if targets := lattice.targetsOfGOOS(goosOfTarget(target)); len(targets) > 0 {
		return targets[0]
	}

	return target
}

// testsTargetPlatform decides the os/arch a single-package conversion targets. A -tests run of a package
// under GOROOT/src re-emits that package INTO the committed corpus, so with no explicit -platforms it
// targets the corpus flavour of the host default (corpusFlavourOf) rather than the converter binary's
// own runtime.GOOS/runtime.GOARCH: darwin/arm64 converts at darwin/amd64, and linux/arm64 at linux/amd64
// until the universe gains linux/arm64. The GOOS stays the host's, which is what every -tests caller
// already relies on.
//
// Unchanged: a host default that is a flavour (a target of the universe); a host GOOS with no flavour,
// which keeps the host default (until 2026-11-09 it was pinned to <goos>/amd64, a corpus nobody
// converts); a module, whose output is its own rather than the corpus and so follows the host; an
// explicit -platforms, which always wins; and any conversion that is not -tests. pinned reports whether
// the default was replaced, for the one log line.
func testsTargetPlatform(hostDefault string, explicit bool, convertTests bool, inputDir string, goRoot string) (platform string, pinned bool) {
	return testsTargetPlatformOver(corpusLattice(), hostDefault, explicit, convertTests, inputDir, goRoot)
}

// testsTargetPlatformOver is testsTargetPlatform under a given lattice.
func testsTargetPlatformOver(lattice *platformLattice, hostDefault string, explicit bool, convertTests bool, inputDir string, goRoot string) (platform string, pinned bool) {
	if explicit || !convertTests || goRoot == "" {
		return hostDefault, false
	}

	srcRoot := filepath.Join(goRoot, "src")

	if filepath.Clean(inputDir) == filepath.Clean(srcRoot) || !isPathUnder(inputDir, srcRoot) {
		return hostDefault, false
	}

	flavour := corpusFlavourOf(lattice, hostDefault)

	return flavour, flavour != hostDefault
}

// testHostPlatformProperties answers the MSBuild properties the converted test host's publish carries
// for a conversion target (publishTestHostArgs): the target's GOOS, then the GOARCH of the corpus flavour
// that serves it. Nothing for an empty target, which leaves both csproj defaults alone, and no arch for a
// target that names none.
//
// The arch is passed always, for the reason the GOOS is: an explicit -p: beats an inherited environment
// variable, so what a run compiles never depends on the caller's environment. And it is the FLAVOUR's
// arch, the one the corpus was converted at, not the conversion target's and not the host's: a module
// run on an Apple-silicon host converts at darwin/arm64 while the corpus it builds against is the
// darwin/amd64 flavour.
func testHostPlatformProperties(lattice *platformLattice, target string) []string {
	goos := goosOfTarget(target)

	if goos == "" {
		return nil
	}

	properties := []string{"-p:GoTargetOS=" + goos}

	if arch := archOfTarget(corpusFlavourOf(lattice, target)); arch != "" {
		properties = append(properties, "-p:"+platformTargetArchProperty+"="+arch)
	}

	return properties
}
