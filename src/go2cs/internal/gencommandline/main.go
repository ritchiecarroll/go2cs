// main.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// gencommandline rewrites the generated regions of docs/CommandLine.md from the option table in
// internal/clihelp/options.go: the table of common options and the option reference. Run from src/go2cs through
// `go generate .`; clihelp's TestCommandLinePageInSync fails while the page is not what this writes.
package main

import (
	"fmt"
	"os"

	"go2cs/internal/clihelp"
)

func main() {
	page, err := os.ReadFile(clihelp.PagePath)

	if err != nil {
		fmt.Fprintf(os.Stderr, "gencommandline: %v\n", err)
		os.Exit(1)
	}

	updated, err := clihelp.Page(string(page))

	if err != nil {
		fmt.Fprintf(os.Stderr, "gencommandline: %v\n", err)
		os.Exit(1)
	}

	if updated == string(page) {
		return
	}

	if err := os.WriteFile(clihelp.PagePath, []byte(updated), 0o644); err != nil {
		fmt.Fprintf(os.Stderr, "gencommandline: %v\n", err)
		os.Exit(1)
	}

	fmt.Printf("gencommandline: wrote %s\n", clihelp.PagePath)
}
