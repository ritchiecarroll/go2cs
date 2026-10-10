// handPackageReadme_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"strings"
	"testing"
)

// The nuget.org pages of the two hand-written packages, go.lib and go.gen (first-impression audit, COORD
// 2026-10-10). Their READMEs are the package pages, and nuget.org's dark theme draws each inline-code span as a red
// block: go.lib's opening paragraph carried about fifteen. Names are plain prose there now, and a fenced block is
// the one place code may appear. go.lib's page also never said that go.lib sets TrimMode=partial for a Native AOT
// or trimmed publish (buildTransitive/go.lib.targets), which is the first thing such a consumer needs to know.
var handPackageReadmes = map[string]string{
	"go.lib": "../core/golib/README.md",
	"go.gen": "../gen/go2cs-gen/README.md",
}

func readHandPackageReadme(t *testing.T, path string) string {
	t.Helper()

	contents, err := os.ReadFile(path)

	if err != nil {
		t.Fatalf("read %s: %v", path, err)
	}

	return strings.ReplaceAll(string(contents), "\r\n", "\n")
}

func TestHandPackageReadmesCarryNoInlineCode(t *testing.T) {
	for id, path := range handPackageReadmes {
		fenced := false

		for i, line := range strings.Split(readHandPackageReadme(t, path), "\n") {
			if strings.HasPrefix(line, "```") {
				fenced = !fenced
				continue
			}

			if !fenced && strings.Contains(line, "`") {
				t.Errorf("%s README line %d carries inline code: %q", id, i+1, line)
			}
		}
	}
}

func TestGoLibReadmeStatesTheTrimModeDefault(t *testing.T) {
	readme := readHandPackageReadme(t, handPackageReadmes["go.lib"])

	for _, want := range []string{"Native AOT", "TrimMode", "partial"} {
		if !strings.Contains(readme, want) {
			t.Errorf("the go.lib README does not mention %q: a Native AOT consumer reads there that go.lib sets TrimMode=partial", want)
		}
	}
}

// golib's Description ends with the build configuration, as every project's must (csprojMetadata_test.go), so a
// PackageDescription of $(Description) rendered "golib (net10.0 - Release)" on nuget.org. The packed description
// names no target framework and no configuration.
func TestGoLibPackageDescriptionNamesNoBuildConfiguration(t *testing.T) {
	name := "golib.csproj"

	contents, err := os.ReadFile(handWrittenProjects[name])

	if err != nil {
		t.Fatalf("read %s: %v", name, err)
	}

	groups := propertyGroupsOf(t, name, strings.TrimPrefix(string(contents), "\ufeff"))
	got := groups.value("PackageDescription")

	if got == "" {
		t.Fatalf("%s sets no <PackageDescription>", name)
	}

	for _, banned := range []string{"$(Description)", "$(TargetFramework)", "$(Configuration)"} {
		if strings.Contains(got, banned) {
			t.Errorf("%s packs <PackageDescription>%s</PackageDescription>, which carries %s", name, got, banned)
		}
	}
}
