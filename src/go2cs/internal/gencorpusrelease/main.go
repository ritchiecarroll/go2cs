// main.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// gencorpusrelease writes src/go2cs/corpus-release.txt: the LAST PUBLISHED go2cs corpus release, as
// releasestamp.PublishedStamp reads it from src/version.props and the write-once snapshots under
// docs/validation (corpusRelease.go explains why the converter embeds it). Run from src/go2cs through
// `go generate .`; repoguard's TestCorpusReleaseMatchesThePublishedStamp fails while the file is stale.
package main

import (
	"fmt"
	"os"
	"path/filepath"

	"go2cs/internal/releasestamp"
)

func main() {
	// go generate runs from src/go2cs; the repository root is two levels up.
	repoRoot := filepath.Join("..", "..")
	outputFile := "corpus-release.txt"

	if len(os.Args) > 1 {
		repoRoot = os.Args[1]
	}

	if len(os.Args) > 2 {
		outputFile = os.Args[2]
	}

	props, err := os.ReadFile(filepath.Join(repoRoot, "src", "version.props"))

	if err != nil {
		fmt.Fprintf(os.Stderr, "gencorpusrelease: %v\n", err)
		os.Exit(1)
	}

	stamp := releasestamp.PublishedStamp(string(props), releasestamp.SnapshotsDir(repoRoot))

	if stamp == "" {
		fmt.Fprintf(os.Stderr, "gencorpusrelease: no published release resolves from %s (no snapshot under %s)\n",
			filepath.Join(repoRoot, "src", "version.props"), releasestamp.SnapshotsDir(repoRoot))
		os.Exit(1)
	}

	if err := os.WriteFile(outputFile, []byte(stamp+"\n"), 0o644); err != nil {
		fmt.Fprintf(os.Stderr, "gencorpusrelease: %v\n", err)
		os.Exit(1)
	}

	fmt.Printf("gencorpusrelease: wrote %s (%s)\n", outputFile, stamp)
}
