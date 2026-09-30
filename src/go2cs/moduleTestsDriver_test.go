// moduleTestsDriver_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"slices"
	"testing"
)

// TestTestClosureDiscoveryAddsNoOrderEdges guards the -tests -recurse driver's discovery load: a
// test-only DEPENDENCY (a module imported only from a _test.go file, which `./...` over the module never
// reaches) joins the convert-set, but ONLY with its own production imports. A test variant (`p [p.test]`
// shares p's PkgPath) never stands in for a production package, so a test's import does NOT become an
// ordering edge from the production package under test.
func TestTestClosureDiscoveryAddsNoOrderEdges(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture module via go/packages")
	}

	root := t.TempDir()
	moduleDir := writeJwtLikeModule(t, root, "1.23")

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "runtime"),
		recurseOutputRoot:   filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	const helper = "example.test/testkit"
	edges := map[string]string{
		"example.test/jwtlike":         "example.test/jwtlike/testutil", // its in-package test imports testutil
		"example.test/jwtlike/request": "example.test/testkit",          // its external test imports testkit
	}

	// CONTROL: without the test closure the helper is not in the convert-set at all, so the arm below
	// is not vacuous.
	plain := NewModuleConverter(options)

	if err := plain.ConvertModule(moduleDir); err != nil {
		t.Fatalf("ConvertModule without the test closure: %v", err)
	}

	if plain.graph.Contains(helper) {
		t.Fatalf("control: %s is in the production convert-set -- the fixture no longer isolates a test-only helper", helper)
	}

	options.recurseOutputRoot = filepath.Join(root, "out2")
	converter := NewModuleConverter(options)
	converter.includeTestClosure = true

	if err := converter.ConvertModule(moduleDir); err != nil {
		t.Fatalf("ConvertModule with the test closure: %v", err)
	}

	if !converter.graph.Contains(helper) {
		t.Fatalf("the test-only helper %s did not join the convert-set", helper)
	}

	for from, testImport := range edges {
		if deps := converter.graph.packages[from].Dependencies; slices.Contains(deps, testImport) {
			t.Errorf("a test-only import became an ORDER edge %s -> %s (dependencies %v): a test variant stood in for production", from, testImport, deps)
		}
	}
}
