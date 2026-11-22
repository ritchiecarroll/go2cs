// cgoLoadTag_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"path/filepath"
	"runtime"
	"slices"
	"strings"
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

// R3 and the no-compiler contract, split by whether CC RESOLVES (COORD's ruling on the train 3 red: the i7 has no gcc on
// PATH, and R3 had refused every -cgo conversion there for a compiler that does not exist, pre-empting "cmd/cgo did not
// run"). The same package, under -cgo, two ways:
//   - CC names nothing that resolves: R3 steps aside and the plain cgo-tag file converts, exactly as with no compiler.
//   - CC resolves and answers another platform's triple to -dumpmachine: R3 refuses it by name.
//
// Neither arm needs a real C compiler: the package imports no "C", so cmd/cgo never runs, and the second arm's CC is a
// script that answers -dumpmachine alone.
func TestR3StepsAsideOnlyWhereTheCCompilerDoesNotResolve(t *testing.T) {
	if runtime.GOOS == "windows" {
		t.Skip("the wrong-triple CC is a shell script: NOT MEASURED on windows (the not-found arm is the i7's own three rows)")
	}

	foreign := filepath.Join(t.TempDir(), "cc-foreign")

	if err := os.WriteFile(foreign, []byte("#!/bin/sh\nif [ \"$1\" = \"-dumpmachine\" ]; then echo x86_64-unknown-freebsd14.1; exit 0; fi\nexit 1\n"), 0o755); err != nil {
		t.Fatal(err)
	}

	arms := []struct {
		name    string
		cc      string
		refused bool
	}{
		{name: "CC does not resolve: the no-compiler contract", cc: "go2cs-no-such-c-compiler", refused: false},
		{name: "CC resolves and targets another platform: R3", cc: foreign, refused: true},
	}

	for _, arm := range arms {
		t.Run(arm.name, func(t *testing.T) {
			dir := t.TempDir()

			for name, contents := range cgoLoadTagFiles {
				writeCgoFixture(t, dir, name, contents)
			}

			stderr, output, err := convertCgoGateFixture(t, dir, []string{"-cgo"}, "CGO_ENABLED=", "CC="+arm.cc)

			if arm.refused {
				if err == nil || !strings.Contains(stderr, "cgo: this -cgo conversion") || !strings.Contains(stderr, "freebsd") {
					t.Fatalf("a C compiler answering another platform's triple was not refused by R3 (%v):\n%s", err, stderr)
				}

				return
			}

			if err != nil || strings.Contains(stderr, "cgo: this -cgo conversion") {
				t.Fatalf("with a C compiler that does not resolve, R3 did not step aside (%v):\n%s", err, stderr)
			}

			if !slices.Contains(emittedProjectFiles(t, output), "on.cs") {
				t.Errorf("the plain cgo-tag file did not convert under -cgo: %v\n%s", emittedProjectFiles(t, output), stderr)
			}
		})
	}
}
