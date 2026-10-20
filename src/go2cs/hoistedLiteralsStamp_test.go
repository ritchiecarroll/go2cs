// hoistedLiteralsStamp_test.go - Gbtc
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
	"strings"
	"testing"
)

// The [GoHoistedLiterals] stamp: a package whose literals are hoisted passes no bare literal to an any slot, so the
// converter stamps its info files and go2cs-gen's literal interceptor runs no transform for the assembly.

// convertStampFixture converts a one-file module into out and returns its package_info.cs, LF-normalized.
func convertStampFixture(t *testing.T, root string, hoist bool) string {
	t.Helper()

	appDir := filepath.Join(root, "app")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/stamp\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "stamp.go"), "package stamp\n\nimport \"fmt\"\n\nfunc Show() { fmt.Println(\"hello\") }\n")

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      "linux/amd64",
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
		hoistLiterals:       hoist,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "stamp", "package_info.cs")), "\r\n", "\n")
}

// TestHoistedLiteralsStampFollowsTheLiteralForm: -hoist-literals stamps package_info.cs once, directly above the
// position-map prose; a plain package carries no stamp; and reconverting a stamped package without the flag removes
// the persisted stamp. RED without the remove-then-add regeneration: the plain reconversion kept the stamp.
func TestHoistedLiteralsStampFollowsTheLiteralForm(t *testing.T) {
	isolateConverterPathGlobals(t)
	root := t.TempDir()

	block := strings.Join(append(hoistedLiteralsStampProseLines(), "[assembly: go.GoHoistedLiterals]", "", goSourcePositionMapsProseLines()[0]), "\n")

	hoisted := convertStampFixture(t, root, true)

	if strings.Count(hoisted, "GoHoistedLiterals") != 1 || !strings.Contains(hoisted, block) {
		t.Fatalf("-hoist-literals: want one stamp block above the position-map prose:\n%s", hoisted)
	}

	plain := convertStampFixture(t, root, false)

	if strings.Contains(plain, "GoHoistedLiterals") || strings.Contains(plain, hoistedLiteralsStampProseLines()[0]) {
		t.Fatalf("a plain reconversion kept the stamp:\n%s", plain)
	}

	if again := convertStampFixture(t, root, true); again != hoisted {
		t.Errorf("a second -hoist-literals conversion is not byte-identical to the first")
	}
}

// TestHoistedLiteralsStampOnlyInTheAssemblysOwnInfoFile: the production and test info files take the stamp, the
// internal-test info file never does (the test assembly compiles it beside package_test_info.cs, and the attribute is
// single-use), and applying it twice changes nothing.
func TestHoistedLiteralsStampOnlyInTheAssemblysOwnInfoFile(t *testing.T) {
	saved := packageHoistsLiterals
	t.Cleanup(func() { packageHoistsLiterals = saved })
	packageHoistsLiterals = true

	lines := append([]string{"// </ImplicitConversions>", ""}, append(goSourcePositionMapsProseLines(), "// <GoSourcePositionMaps>", "// </GoSourcePositionMaps>", "", "namespace go;")...)

	for name, want := range map[string]bool{
		"package_info.cs":               true,
		"package_test_info.cs":          true,
		"package_info_internal_test.cs": false,
	} {
		once := applyHoistedLiteralsStamp(lines, filepath.Join("out", name))

		if got := slices.ContainsFunc(once, isHoistedLiteralsStampLine); got != want {
			t.Errorf("%s: stamped = %v, want %v", name, got, want)
		}

		if twice := applyHoistedLiteralsStamp(once, filepath.Join("out", name)); !slices.Equal(twice, once) {
			t.Errorf("%s: a second application changed the file:\n%s", name, strings.Join(twice, "\n"))
		}
	}

	packageHoistsLiterals = false

	if cleared := applyHoistedLiteralsStamp(applyHoistedLiteralsStampFor(t, lines), "package_info.cs"); !slices.Equal(cleared, lines) {
		t.Errorf("removing the stamp did not restore the file:\n%s", strings.Join(cleared, "\n"))
	}
}

// applyHoistedLiteralsStampFor stamps lines as a hoisting package's package_info.cs.
func applyHoistedLiteralsStampFor(t *testing.T, lines []string) []string {
	t.Helper()

	saved := packageHoistsLiterals
	packageHoistsLiterals = true
	defer func() { packageHoistsLiterals = saved }()

	return applyHoistedLiteralsStamp(lines, "package_info.cs")
}
