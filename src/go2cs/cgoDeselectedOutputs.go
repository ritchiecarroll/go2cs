// cgoDeselectedOutputs.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

// cgo Phase 3, increment 3.4, seat 4c (docs/phase4/DESIGN-cgo-phase3-marshaling.md 19.6): a cgo-on conversion into a
// root seeded from the corpus finds the corpus's cgo-OFF selection already there. Where the cgo-on load swaps a file for
// another (net's cgo_stub.go for cgo_unix.go), a single-package conversion writes only what it selects, so the stale
// file stays and both compile: CS0102/CS0111 on every declaration the two share (the first build of net's test host,
// six of them). corpus.md names the class: "declarations MIGRATE while the stale other-selection file remains".
//
// The cure removes, from the OUTPUT, the C# of each production file the converter's own cgo-off load selects and its
// cgo-on load does not. The converter is the authority on what it selected: the set comes from its two loads, never
// from a second reading of build tags. It is bounded by COORD's GO (inbox H 20261124T231602Z):
//   - it runs only on the -cgo -tests path, whose output checkCgoTestsOutput keeps out of the repository's corpus;
//   - it removes only a file that carries the converter's own emission header for THAT source file (the source's
//     leading comment lines, its //go:build line among them, which the converter copies verbatim with -comments, and
//     -tests forces -comments). A hand-owned file, or any other file in the set, is refused by name;
//   - every removal is printed with its reason, and -cgo-removals-dry-run lists the set without removing anything.

import (
	"bufio"
	"fmt"
	"os"
	"path/filepath"
	"slices"
	"strings"

	"golang.org/x/tools/go/packages"
)

// leadingCommentLines returns a file's leading // lines, blank lines skipped, up to the first line that is neither: the
// Go source's `package` clause, or the emitted C#'s `namespace` (or `using`) line.
func leadingCommentLines(path string) ([]string, error) {
	file, err := os.Open(path)

	if err != nil {
		return nil, err
	}

	defer file.Close()

	var lines []string
	scanner := bufio.NewScanner(file)
	scanner.Buffer(make([]byte, 0, 64*1024), 16*1024*1024)

	for scanner.Scan() {
		line := strings.TrimSpace(strings.TrimPrefix(scanner.Text(), "\ufeff"))

		if line == "" {
			continue
		}

		if !strings.HasPrefix(line, "//") {
			break
		}

		lines = append(lines, line)
	}

	return lines, scanner.Err()
}

// isConverterEmissionOf reports whether csPath carries the converter's own emission header for goPath: the same leading
// comment lines, in order, and at least one of them (a source with none gives nothing to match, so it is never claimed).
func isConverterEmissionOf(csPath, goPath string) (bool, error) {
	source, err := leadingCommentLines(goPath)

	if err != nil || len(source) == 0 {
		return false, err
	}

	emitted, err := leadingCommentLines(csPath)

	if err != nil {
		return false, err
	}

	return slices.Equal(source, emitted), nil
}

// cgoRemovalsDryRun is -cgo-removals-dry-run: list the set, remove nothing (main.go sets it from the flag).
var cgoRemovalsDryRun bool

// cgoDeselectedOutputs finds in packageOutputPath the C# file of each Go file in cgoOffFiles that cgoOnFiles does not
// hold, wherever the layout routes it for target (flat or a key folder), and removes it unless dryRun. It returns the
// paths it removed (or would remove). Names are compared by base name: both lists are one package directory's files. A
// file that does not exist is skipped. A hand-owned file, or a file without the converter's emission header for its
// source, is refused by name, and then nothing is removed at all.
func cgoDeselectedOutputs(packageOutputPath, target string, cgoOffFiles, cgoOnFiles []string, dryRun bool) ([]string, error) {
	selected := map[string]bool{}

	for _, file := range cgoOnFiles {
		selected[filepath.Base(file)] = true
	}

	var stale []string

	for _, file := range cgoOffFiles {
		base := filepath.Base(file)

		if selected[base] || !strings.HasSuffix(base, ".go") {
			continue
		}

		path := platformLayoutPath(packageOutputPath, target, strings.TrimSuffix(base, ".go")+".cs")

		if info, err := os.Stat(path); err != nil || !info.Mode().IsRegular() {
			continue
		}

		handOwned, err := containsManualConversionMarker(path)

		if err != nil {
			return nil, err
		}

		if handOwned {
			return nil, fmt.Errorf("cgo: %s is hand-owned, and the cgo-on load deselects %s, which it stands for: go2cs never removes a hand-owned file, so convert into a root that does not hold it", path, base)
		}

		emitted, err := isConverterEmissionOf(path, file)

		if err != nil {
			return nil, err
		}

		if !emitted {
			return nil, fmt.Errorf("cgo: %s does not carry go2cs's emission header for %s, which the cgo-on load deselects: go2cs removes only its own emission, so remove it by hand or convert into a root that does not hold it", path, base)
		}

		stale = append(stale, path)
	}

	for _, path := range stale {
		if dryRun {
			fmt.Printf("cgo: would remove %s: go2cs's emission of %s, which the cgo-on load deselects (-cgo-removals-dry-run)\n", path, strings.TrimSuffix(filepath.Base(path), ".cs")+".go")
			continue
		}

		if err := os.Remove(path); err != nil {
			return nil, err
		}

		fmt.Printf("cgo: removed %s: go2cs's emission of %s, which the cgo-on load deselects\n", path, strings.TrimSuffix(filepath.Base(path), ".cs")+".go")
	}

	return stale, nil
}

// removeCgoDeselectedOutputs is cgoDeselectedOutputs for a package the -cgo -tests path converted: its cgo-on selection
// is the load's own (pkg.GoFiles, which go/packages fills with the cgo files too), and its cgo-off selection is one more
// load of the same directory at CGO_ENABLED=0, last so it wins over the -cgo environment. Anywhere else (plain -cgo,
// no -cgo, the offline test seam whose loads are cgo-off already) it does nothing.
func removeCgoDeselectedOutputs(pkg *packages.Package, packageOutputPath string, options Options) error {
	if !options.parseCgoTargets || !options.convertTests || cgoOfflineTrees || pkg == nil || len(pkg.GoFiles) == 0 {
		return nil
	}

	goos, goarch, ok := strings.Cut(options.targetPlatform, "/")

	if !ok {
		return nil
	}

	loaded, err := packages.Load(&packages.Config{
		Mode:       packages.NeedName | packages.NeedFiles,
		Dir:        filepath.Dir(pkg.GoFiles[0]),
		BuildFlags: options.loaderBuildFlags(),
		Env:        append(converterLoaderEnv("GOOS="+goos, "GOARCH="+goarch, "GOTOOLCHAIN=local"), "CGO_ENABLED=0"),
	}, ".")

	if err != nil || len(loaded) != 1 {
		return fmt.Errorf("cgo: load %s at CGO_ENABLED=0, to find the files the cgo-on load deselects: %v", pkg.PkgPath, err)
	}

	_, err = cgoDeselectedOutputs(packageOutputPath, options.targetPlatform, loaded[0].GoFiles, pkg.GoFiles, cgoRemovalsDryRun)

	return err
}
