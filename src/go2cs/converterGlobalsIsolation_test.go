// converterGlobalsIsolation_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Isolation of the converter's package-identity globals between tests.
//
// A conversion run in-process (ConvertModule, the -tests driver, resetPackageState) leaves
// currentPackagePath, packageName and currentPackageGorootVendored set to the last package it converted.
// goPackageImportPathFor reads them, so a test that builds a package_info seed without converting
// anything stamps whatever import path an EARLIER test left behind:
//
//	go test . -run 'PackageInfo|GoPackage'   fails TestInternalTestPackageInfoSeedAnchorsBridgeClass
//
// while the full suite passed only because some other test happened to reset the state in between.
// Every test that converts calls isolateConverterPathGlobals first, which restores the values it found
// when the test ends; a test that reads the globals without setting them calls
// requirePristineConverterPathGlobals, which fails NAMING the leaked value instead of asserting on a
// stamp that is wrong for a reason three tests away.

package main

import (
	"fmt"
	"strings"
	"testing"
)

// converterPathGlobals is the package-identity state goPackageImportPathFor and the package_info
// stamping read.
type converterPathGlobals struct {
	currentPackagePath           string
	packageName                  string
	currentPackageGorootVendored bool
}

func snapshotConverterPathGlobals() converterPathGlobals {
	return converterPathGlobals{currentPackagePath, packageName, currentPackageGorootVendored}
}

func (g converterPathGlobals) restore() {
	currentPackagePath, packageName, currentPackageGorootVendored = g.currentPackagePath, g.packageName, g.currentPackageGorootVendored
}

// isolateConverterPathGlobals restores the package-identity globals to their present values when the
// test (and its subtests) finish. Call it first in any test that runs a conversion.
func isolateConverterPathGlobals(t *testing.T) {
	t.Helper()

	saved := snapshotConverterPathGlobals()
	t.Cleanup(saved.restore)
}

// leakedConverterPathGlobals names every package-identity global that is not at its zero value, or
// returns "" when they all are.
func leakedConverterPathGlobals() string {
	var leaked []string

	if currentPackagePath != "" {
		leaked = append(leaked, fmt.Sprintf("currentPackagePath=%q", currentPackagePath))
	}

	if packageName != "" {
		leaked = append(leaked, fmt.Sprintf("packageName=%q", packageName))
	}

	if currentPackageGorootVendored {
		leaked = append(leaked, "currentPackageGorootVendored=true")
	}

	return strings.Join(leaked, ", ")
}

// requirePristineConverterPathGlobals fails the test when an earlier one left the package-identity
// globals set. The earlier test is the one missing isolateConverterPathGlobals.
func requirePristineConverterPathGlobals(t *testing.T) {
	t.Helper()

	if leaked := leakedConverterPathGlobals(); leaked != "" {
		t.Fatalf("an earlier test leaked converter state (%s): it converts without calling isolateConverterPathGlobals(t)", leaked)
	}
}

func TestLeakedConverterPathGlobalsNamesEachLeakedValue(t *testing.T) {
	isolateConverterPathGlobals(t)

	currentPackagePath, packageName, currentPackageGorootVendored = "", "", false

	if leaked := leakedConverterPathGlobals(); leaked != "" {
		t.Fatalf("zero values read as leaked: %s", leaked)
	}

	currentPackagePath = "example.com/leak"
	packageName = "leak"
	currentPackageGorootVendored = true

	leaked := leakedConverterPathGlobals()

	for _, want := range []string{`currentPackagePath="example.com/leak"`, `packageName="leak"`, "currentPackageGorootVendored=true"} {
		if !strings.Contains(leaked, want) {
			t.Fatalf("leak report must name %s, got %q", want, leaked)
		}
	}
}

func TestIsolateConverterPathGlobalsRestoresWhatTheTestChanged(t *testing.T) {
	outerPath, outerName := currentPackagePath, packageName

	t.Run("converts", func(t *testing.T) {
		isolateConverterPathGlobals(t)

		currentPackagePath, packageName, currentPackageGorootVendored = "example.com/converted", "converted", true
	})

	if currentPackagePath != outerPath || packageName != outerName || currentPackageGorootVendored {
		t.Fatalf("the subtest's cleanup must restore the globals: got %q/%q/%v, want %q/%q/false",
			currentPackagePath, packageName, currentPackageGorootVendored, outerPath, outerName)
	}
}
