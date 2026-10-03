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
	"strings"
)

// corpusArch is the architecture of the COMMITTED standard-library corpus. Every flavor is emitted for
// it: the release scripts and the -stdlib usage convert with -platforms windows/amd64,linux/amd64,
// darwin/amd64, one GOOS per target and one architecture for all three.
const corpusArch = "amd64"

// testsTargetPlatform decides the os/arch a single-package conversion targets. A -tests run of a package
// under GOROOT/src re-emits that package INTO the committed corpus, so with no explicit -platforms it
// targets <host GOOS>/corpusArch rather than the converter binary's own runtime.GOOS/runtime.GOARCH. On
// an arm64 host that default re-emitted hash/crc32 for darwin/arm64: crc32_arm64.go landed beside the
// committed crc32_amd64.cs and the test host failed with CS0111 x6 (darwin row pilot, run 37112360853,
// 2026-10-03). The GOOS stays the host's, which is what every -tests caller already relies on.
//
// Unchanged: an amd64 host (the default already names corpusArch); a module, whose output is its own
// rather than the corpus and so follows the host; an explicit -platforms, which always wins; and any
// conversion that is not -tests. pinned reports whether the default was replaced, for the one log line.
func testsTargetPlatform(hostDefault string, explicit bool, convertTests bool, inputDir string, goRoot string) (platform string, pinned bool) {
	if explicit || !convertTests || goRoot == "" {
		return hostDefault, false
	}

	srcRoot := filepath.Join(goRoot, "src")

	if filepath.Clean(inputDir) == filepath.Clean(srcRoot) || !isPathUnder(inputDir, srcRoot) {
		return hostDefault, false
	}

	goos, arch, found := strings.Cut(hostDefault, "/")

	if !found || arch == corpusArch {
		return hostDefault, false
	}

	return goos + "/" + corpusArch, true
}
