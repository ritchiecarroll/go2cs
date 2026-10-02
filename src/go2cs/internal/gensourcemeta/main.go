// main.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// gensourcemeta writes and verifies a packed Go module's go2cs/source-metadata.txt (internal/sourcemeta;
// docs/PLAN-nugetgo.md section 5). src/tools/nugetgo/nugetgo-pack.ps1 calls it twice: once to WRITE the file
// it packs, and once on the file read back out of the finished nupkg, to prove the converter's own parser
// accepts what was packed.
//
//	gensourcemeta -src <recurse root>/src -module <path> -module-version <v> -go2cs-release <release>
//	              -package <import-path>=<assembly> ... [-require <module>@<version>=<nuget-id> ...] -out <file>
//	gensourcemeta -verify <file>
package main

import (
	"flag"
	"fmt"
	"io"
	"os"
	"strings"

	"go2cs/internal/sourcemeta"
)

func main() {
	if err := run(os.Args[1:], os.Stdout); err != nil {
		fmt.Fprintf(os.Stderr, "gensourcemeta: %v\n", err)
		os.Exit(1)
	}
}

// listFlag is a repeatable string flag.
type listFlag []string

func (l *listFlag) String() string { return strings.Join(*l, ",") }

func (l *listFlag) Set(value string) error {
	*l = append(*l, value)
	return nil
}

func run(args []string, stdout io.Writer) error {
	fs := flag.NewFlagSet("gensourcemeta", flag.ContinueOnError)
	fs.SetOutput(io.Discard)

	var packages, requires listFlag
	src := fs.String("src", "", "the -recurse output root's src/ tree")
	modulePath := fs.String("module", "", "the Go module path")
	moduleVersion := fs.String("module-version", "", "the Go module version converted, with its leading v")
	release := fs.String("go2cs-release", "", "the go2cs corpus release the package was built against")
	out := fs.String("out", "", "the file to write")
	verify := fs.String("verify", "", "parse this file and print a summary instead of writing one")
	fs.Var(&packages, "package", "<import-path>=<assembly>, one per packed Go package")
	fs.Var(&requires, "require", "<module>@<version>=<nuget-id>, one per third-party module the assemblies depend on")

	if err := fs.Parse(args); err != nil {
		return err
	}

	if *verify != "" {
		data, err := os.ReadFile(*verify)

		if err != nil {
			return err
		}

		d, err := sourcemeta.Parse(data)

		if err != nil {
			return fmt.Errorf("%s: %v", *verify, err)
		}

		fmt.Fprintf(stdout, "%s %s (go2cs-release %s): %d package(s), %d require(s), %d section(s)\n",
			d.Module, d.ModuleVersion, d.Go2csRelease, len(d.Packages), len(d.Requires), len(d.Sections))
		return nil
	}

	d := sourcemeta.Description{Module: *modulePath, ModuleVersion: *moduleVersion, Go2csRelease: *release}

	for _, value := range packages {
		importPath, assembly, ok := strings.Cut(value, "=")

		if !ok || importPath == "" || assembly == "" {
			return fmt.Errorf("-package %q: want <import-path>=<assembly>", value)
		}

		d.Packages = append(d.Packages, sourcemeta.Package{ImportPath: importPath, Assembly: assembly})
	}

	for _, value := range requires {
		moduleAndVersion, nugetID, ok := strings.Cut(value, "=")
		requiredModule, version, versioned := strings.Cut(moduleAndVersion, "@")

		if !ok || !versioned || requiredModule == "" || version == "" || nugetID == "" {
			return fmt.Errorf("-require %q: want <module>@<version>=<nuget-id>", value)
		}

		d.Requires = append(d.Requires, sourcemeta.Require{Module: requiredModule, Version: version, NuGetID: nugetID})
	}

	if *src == "" || *out == "" {
		return fmt.Errorf("-src and -out are required (or -verify <file>)")
	}

	sections, err := sourcemeta.Collect(*src, d.Packages)

	if err != nil {
		return err
	}

	d.Sections = sections
	data, err := sourcemeta.Generate(d)

	if err != nil {
		return err
	}

	if err := os.WriteFile(*out, data, 0o644); err != nil {
		return err
	}

	fmt.Fprintf(stdout, "gensourcemeta: wrote %s (%d package(s), %d require(s), %d section(s), %d bytes)\n", *out, len(d.Packages), len(d.Requires), len(sections), len(data))
	return nil
}
