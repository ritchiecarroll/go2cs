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
// from a second reading of build tags. A hand-owned file is never removed; one in that set is refused by name. The
// output is never the repository's corpus (checkCgoTestsOutput), so no corpus byte moves.

import (
	"fmt"
	"os"
	"path/filepath"
	"strings"

	"golang.org/x/tools/go/packages"
)

// cgoDeselectedOutputs removes from packageOutputPath the C# file of each Go file in cgoOffFiles that cgoOnFiles does
// not hold, wherever the layout routes it for target (flat or a key folder), and returns the paths it removed. Names are
// compared by base name: both lists are one package directory's files. A file that does not exist is skipped. A
// hand-owned file (the manual-conversion marker) is refused by name, and nothing is removed after it.
func cgoDeselectedOutputs(packageOutputPath, target string, cgoOffFiles, cgoOnFiles []string) ([]string, error) {
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

		stale = append(stale, path)
	}

	for _, path := range stale {
		if err := os.Remove(path); err != nil {
			return nil, err
		}
	}

	return stale, nil
}

// removeCgoDeselectedOutputs is cgoDeselectedOutputs for a package the -cgo route converted: its cgo-on selection is
// the load's own (pkg.GoFiles, which go/packages fills with the cgo files too), and its cgo-off selection is one more
// load of the same directory at CGO_ENABLED=0, last so it wins over the -cgo environment. Without -cgo, and on the
// offline test seam (whose loads are cgo-off already), it does nothing.
func removeCgoDeselectedOutputs(pkg *packages.Package, packageOutputPath string, options Options) error {
	if !options.parseCgoTargets || cgoOfflineTrees || pkg == nil || len(pkg.GoFiles) == 0 {
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

	removed, err := cgoDeselectedOutputs(packageOutputPath, options.targetPlatform, loaded[0].GoFiles, pkg.GoFiles)

	for _, path := range removed {
		fmt.Printf("cgo: removed %s, the cgo-off file the cgo-on load deselects\n", path)
	}

	return err
}
