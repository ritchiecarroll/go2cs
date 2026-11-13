// cgoLoadTag_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"slices"
	"testing"
)

// The focused re-check of increment 5b, M1's residual (DESIGN-cgo-phase3-marshaling §15, R1): a plain file under
// //go:build cgo belongs to every package a cgo-on load selects it for, whether or not the package has an
// import "C" of its own. The driver's build-constraint re-check carried the cgo tag only for a package with a cgo
// route, so here the file was dropped with exit 0, and every reference to what it declares dangled.
//
// The package has no import "C" and imports nothing, so a cgo-on load of it needs no C compiler.
var cgoLoadTagFiles = map[string]string{
	"go.mod":   "module example.com/cgoloadtag\n\ngo 1.26\n",
	"value.go": "package cgoloadtag\n\n// Value is what the build's own file answers.\nfunc Value() int { return tagged() }\n",
	"on.go":    "//go:build cgo\n\npackage cgoloadtag\n\nfunc tagged() int { return 7 }\n",
	"off.go":   "//go:build !cgo\n\npackage cgoloadtag\n\nfunc tagged() int { return 0 }\n",
}

func TestAPlainFileUnderTheCgoTagConvertsWhereverTheLoadIsCgoOn(t *testing.T) {
	arms := []struct {
		name     string
		args     []string
		env      []string
		selected string
		dropped  string
	}{
		// The control: a cgo-off load selects the other file, so the two arms below can fail.
		{name: "a cgo-off load", env: []string{"CGO_ENABLED=0"}, selected: "off.cs", dropped: "on.cs"},
		{name: "under -cgo", args: []string{"-cgo"}, env: []string{"CGO_ENABLED="}, selected: "on.cs", dropped: "off.cs"},
		{name: "under CGO_ENABLED=1 and no -cgo", env: []string{"CGO_ENABLED=1"}, selected: "on.cs", dropped: "off.cs"},
	}

	for _, arm := range arms {
		t.Run(arm.name, func(t *testing.T) {
			dir := t.TempDir()

			for name, contents := range cgoLoadTagFiles {
				writeCgoFixture(t, dir, name, contents)
			}

			stderr, output, err := convertCgoGateFixture(t, dir, arm.args, arm.env...)

			if err != nil {
				t.Fatalf("the conversion failed: %v\n%s", err, stderr)
			}

			emitted := emittedProjectFiles(t, output)

			if !slices.Contains(emitted, "value.cs") {
				t.Fatalf("the package's plain file was not converted: %v\n%s", emitted, stderr)
			}

			if !slices.Contains(emitted, arm.selected) {
				t.Errorf("the file the load selected, %s, was not converted: %v", arm.selected, emitted)
			}

			if slices.Contains(emitted, arm.dropped) {
				t.Errorf("the file the load left out, %s, was converted: %v", arm.dropped, emitted)
			}
		})
	}
}
