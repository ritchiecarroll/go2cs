// main.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// genstdlibmeta generates src/go2cs/stdlib-metadata.txt — the converter's embedded record
// of the EXPORTED cross-package metadata every converted standard-library package publishes
// in its package_info.cs:
//
//   - the <ExportedTypeAliases> section ([assembly: GoTypeAlias("alias", "type")])
//   - the interface-implementation records ([assembly: GoImplement<T, Iface>…])
//
// Normally the converter learns those facts by reading the referenced package's
// package_info.cs out of the converted-source tree on disk. Under -recurse=nuget there is
// no such tree: the standard library arrives as pre-compiled go.<pkg> NuGet packages, so the
// facts have to travel with the converter instead. This generator captures them from the
// canonical source of those packages — src/core, the very tree push-nuget.ps1
// packs — so the embedded record and the published assemblies are produced from one commit.
//
// Run from src/go2cs via `go generate .` (the //go:generate directive lives in
// stdlibMetadata.go). TestStdLibMetadataInSync re-runs this generation in-process and fails
// if the committed asset has drifted from src/core.
//
// The generator is DETERMINISTIC: sections emit in sorted package order, each carrying the
// matched lines verbatim in file order, with no timestamps. Output is written as BOM-less
// UTF-8 with CRLF endings (the repository convention), so regeneration never fights the
// checkout's autocrlf normalization.
package main

import (
	"fmt"
	"os"
	"path/filepath"

	"go2cs/internal/stdlibmeta"
)

func main() {
	// The generator runs from src/go2cs (go generate's working directory is the directory
	// of the file carrying the directive), so both roots are resolved from there.
	convertedRoot := filepath.Join("..", "core")
	outputFile := stdlibmeta.AssetFileName

	if len(os.Args) > 1 {
		convertedRoot = os.Args[1]
	}

	if len(os.Args) > 2 {
		outputFile = os.Args[2]
	}

	content, count, err := stdlibmeta.Generate(convertedRoot)

	if err != nil {
		fmt.Fprintf(os.Stderr, "genstdlibmeta: %s\n", err)
		os.Exit(1)
	}

	if err := os.WriteFile(outputFile, content, 0644); err != nil {
		fmt.Fprintf(os.Stderr, "genstdlibmeta: failed to write %s: %s\n", outputFile, err)
		os.Exit(1)
	}

	fmt.Printf("genstdlibmeta: wrote %s (%d packages, %d bytes)\n", outputFile, count, len(content))
}
