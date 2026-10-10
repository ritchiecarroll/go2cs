// main.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// gensynopsis prints a Go package's documentation synopsis: the first sentence of its package doc comment, as
// go/build reads it (the same text `go list -f {{.Doc}}` prints). src/tools/nugetgo/nugetgo-pack.ps1 calls it on
// the module's own source in the module cache, for the generated package README. It reads one directory and
// resolves no import, so it needs no module graph and no network.
//
//	gensynopsis -dir <Go package directory>
//
// A package with no doc comment prints an empty line; a directory with no Go package is an error.
package main

import (
	"errors"
	"flag"
	"fmt"
	"go/build"
	"io"
	"os"
)

func main() {
	if err := run(os.Args[1:], os.Stdout); err != nil {
		fmt.Fprintf(os.Stderr, "gensynopsis: %v\n", err)
		os.Exit(1)
	}
}

func run(args []string, stdout io.Writer) error {
	flags := flag.NewFlagSet("gensynopsis", flag.ContinueOnError)
	dir := flags.String("dir", "", "the Go package directory")

	if err := flags.Parse(args); err != nil {
		return err
	}

	if *dir == "" {
		return errors.New("-dir is required")
	}

	synopsis, err := packageSynopsis(*dir)

	if err != nil {
		return err
	}

	_, err = fmt.Fprintln(stdout, synopsis)
	return err
}

// packageSynopsis returns the synopsis of the Go package in dir. go/build reads only the files the default build
// context selects (a test file or one a build constraint excludes is not the package's) and takes the synopsis from
// the doc comment attached to their package clause, so a license header above it is not read.
func packageSynopsis(dir string) (string, error) {
	pkg, err := build.ImportDir(dir, 0)

	if err != nil {
		return "", fmt.Errorf("no Go package in %s: %w", dir, err)
	}

	return pkg.Doc, nil
}
