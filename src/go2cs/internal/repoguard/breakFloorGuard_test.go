// breakFloorGuard_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"strconv"
	"strings"
	"testing"
)

// go.lib's GoCheckBreakFloor target (docs/phase4/DESIGN-go-break-floor.md) fails a consumer's build, as GO2CS3002,
// for every go2cs-built package it resolves that was built below $(GoBreakFloor): the newest go.* release that changed
// a member compiled code binds. A package built below it restores cleanly and then throws MissingMethodException at
// run time (docs/phase4/RECORD-nuget-sibling-pins.md, arms a2 and h1), so the build is the last place to stop it.
//
// TestTheBreakFloorNamesEachPackageBuiltBelowIt runs the target alone, against the working-tree go.lib.targets, over
// hand-made items of the shape ResolvePackageAssets produces and hand-made package folders, so it needs no restore and
// no feed. The versions are derived from the floor the targets file states, so the test stays true when the floor
// moves.
var breakFloorElement = regexp.MustCompile(`<GoBreakFloor>(\d+)\.(\d+)\.(\d+)\.(\d+)</GoBreakFloor>`)

type breakFloorPackage struct {
	id      string
	version string
	// release, when not nil, is written as the package's go2cs/source-metadata.txt: a converted module. An empty
	// string writes the file with no go2cs-release line.
	release *string
	// assets is the number of distinct assets the package resolves; each is listed as a compile AND a runtime item.
	assets int
}

func TestTheBreakFloorNamesEachPackageBuiltBelowIt(t *testing.T) {
	dotnet, err := exec.LookPath("dotnet")

	if err != nil {
		t.Skip("dotnet is not on PATH; the break-floor arms need MSBuild")
	}

	root := repoRootFromPackageDir(t)
	targets := filepath.Join(root, "src", "core", "golib", "buildTransitive", "go.lib.targets")
	text, err := os.ReadFile(targets)

	if err != nil {
		t.Fatalf("cannot read %s: %v", targets, err)
	}

	m := breakFloorElement.FindStringSubmatch(string(text))

	if m == nil {
		t.Fatalf("%s states no <GoBreakFloor>major.minor.patch.revision</GoBreakFloor>: the floor GoCheckBreakFloor compares with", targets)
	}

	revision, _ := strconv.Atoi(m[4])

	if revision < 1 {
		t.Fatalf("the floor %s.%s.%s.%s has no revision below it to test with", m[1], m[2], m[3], m[4])
	}

	line := m[1] + "." + m[2] + "." + m[3] + "."
	floor := line + m[4]
	below := line + strconv.Itoa(revision-1)
	above := line + strconv.Itoa(revision+1)
	str := func(s string) *string { return &s }
	empty := ""

	cases := []struct {
		name     string
		packages []breakFloorPackage
		props    []string
		// named lists the package IDs that must each get exactly one GO2CS3002; kind is "error" or "warning".
		named []string
		kind  string
		// text, when set, must appear in the output.
		text string
	}{
		{name: "a converted module built below the floor",
			packages: []breakFloorPackage{{id: "nugetgo.example.com.below", version: "1.0.4", release: str(below), assets: 1}},
			named:    []string{"nugetgo.example.com.below"}, kind: "error",
			text: "nugetgo.example.com.below 1.0.4 was built on go.* " + below + ", below go.* " + floor},
		{name: "a converted module built at the floor",
			packages: []breakFloorPackage{{id: "nugetgo.example.com.at", version: "1.0.5", release: str(floor), assets: 1}}},
		{name: "a converted module built above the floor",
			packages: []breakFloorPackage{{id: "nugetgo.example.com.above", version: "1.0.6", release: str(above), assets: 1}}},
		{name: "a converted module under another ID, built below the floor",
			packages: []breakFloorPackage{{id: "Contoso.Converted", version: "2.0.0", release: str(below), assets: 1}},
			named:    []string{"Contoso.Converted"}, kind: "error"},
		{name: "a converted module whose metadata names no release",
			packages: []breakFloorPackage{{id: "nugetgo.example.com.unknown", version: "1.0.1", release: &empty, assets: 1}},
			named:    []string{"nugetgo.example.com.unknown"}, kind: "error",
			text: "nugetgo.example.com.unknown 1.0.1 was built on go.* an unknown release"},
		{name: "a go.* package below the floor",
			packages: []breakFloorPackage{{id: "go.encoding.json", version: below, assets: 1}},
			named:    []string{"go.encoding.json"}, kind: "error",
			text: "go.encoding.json " + below + " is a go.* " + below + " package, below go.* " + floor},
		{name: "a go.* package at the floor",
			packages: []breakFloorPackage{{id: "go.fmt", version: floor, assets: 1}}},
		{name: "a go.* prerelease of the floor counts as the floor",
			packages: []breakFloorPackage{{id: "go.fmt", version: floor + "-rc1", assets: 1}}},
		{name: "a go.* prerelease below the floor",
			packages: []breakFloorPackage{{id: "go.os", version: below + "-rc1", assets: 1}},
			named:    []string{"go.os"}, kind: "error"},
		{name: "a package go2cs did not build",
			packages: []breakFloorPackage{{id: "Contoso.Logging", version: "1.0.0", assets: 1}}},
		{name: "one diagnostic per package, however many assets it has",
			packages: []breakFloorPackage{
				{id: "go.syscall", version: below, assets: 3},
				{id: "nugetgo.example.com.multi", version: "1.0.2", release: str(below), assets: 2},
				{id: "go.fmt", version: floor, assets: 1},
			},
			named: []string{"go.syscall", "nugetgo.example.com.multi"}, kind: "error"},
		{name: "the opt-out turns each error into a warning",
			packages: []breakFloorPackage{{id: "nugetgo.example.com.below", version: "1.0.4", release: str(below), assets: 1}},
			props:    []string{"GoAllowBelowBreakFloor=true"},
			named:    []string{"nugetgo.example.com.below"}, kind: "warning",
			text: "GoAllowBelowBreakFloor=true"},
		{name: "a project that resolves no package"},
		{name: "a design-time build checks nothing",
			packages: []breakFloorPackage{{id: "go.encoding.json", version: below, assets: 1}},
			props:    []string{"DesignTimeBuild=true"}},
	}

	for i, c := range cases {
		t.Run(c.name, func(t *testing.T) {
			dir := filepath.Join(t.TempDir(), fmt.Sprintf("case%d", i))
			project := writeBreakFloorProbe(t, dir, targets, c.packages)
			args := []string{"msbuild", project, "-t:GoCheckBreakFloor", "-nologo", "-v:m"}

			for _, p := range c.props {
				args = append(args, "-p:"+p)
			}

			cmd := exec.Command(dotnet, args...)
			cmd.Dir = dir
			cmd.Env = append(os.Environ(), "DOTNET_CLI_TELEMETRY_OPTOUT=1", "DOTNET_NOLOGO=1", "MSBUILDDISABLENODEREUSE=1")
			out, runErr := cmd.CombinedOutput()
			output := string(out)

			if strings.Contains(output, "MSB4057") {
				t.Fatalf("go.lib.targets has no GoCheckBreakFloor target:\n%s", output)
			}

			failed := runErr != nil

			if want := c.kind == "error"; failed != want {
				t.Errorf("build failed=%v (%v), want failed=%v; output:\n%s", failed, runErr, want, output)
			}

			// MSBuild prints a diagnostic once inline and once in its summary, so the IDs are counted in distinct lines.
			diagnostics := map[string]bool{}

			for _, l := range strings.Split(output, "\n") {
				if strings.Contains(l, "GO2CS3002") {
					diagnostics[strings.TrimSpace(l)] = true
				}
			}

			for line := range diagnostics {
				if c.kind == "" || !strings.Contains(line, c.kind+" GO2CS3002") {
					t.Errorf("unexpected diagnostic: %s", line)
				}
			}

			for _, id := range c.named {
				n := 0

				for line := range diagnostics {
					if strings.Contains(line, "GO2CS3002: "+id+" ") {
						n++
					}
				}

				if n != 1 {
					t.Errorf("%s: %d distinct %s GO2CS3002 line(s) naming it, want 1; output:\n%s", id, n, c.kind, output)
				}
			}

			if len(c.named) == 0 && len(diagnostics) != 0 {
				t.Errorf("want no GO2CS3002; output:\n%s", output)
			}

			if c.text != "" && !strings.Contains(output, c.text) {
				t.Errorf("the output does not say %q:\n%s", c.text, output)
			}
		})
	}
}

// writeBreakFloorProbe lays out each package as NuGet's global folder does (<root>/<id>/<version>/lib/net10.0/*.dll,
// with go2cs/source-metadata.txt beside lib for a converted module) and writes the smallest project that lists their
// assets as ResolvePackageAssets would and imports go.lib.targets.
func writeBreakFloorProbe(t *testing.T, dir, targets string, packages []breakFloorPackage) string {
	t.Helper()
	var items strings.Builder

	for _, p := range packages {
		pkgRoot := filepath.Join(dir, "packages", strings.ToLower(p.id), strings.ToLower(p.version))

		for a := 0; a < p.assets; a++ {
			inPackage := fmt.Sprintf("lib/net10.0/asset%d.dll", a)
			full := filepath.Join(pkgRoot, filepath.FromSlash(inPackage))

			if err := os.MkdirAll(filepath.Dir(full), 0o755); err != nil {
				t.Fatal(err)
			}

			if err := os.WriteFile(full, nil, 0o644); err != nil {
				t.Fatal(err)
			}

			for _, kind := range []string{"ResolvedCompileFileDefinitions", "RuntimeCopyLocalItems"} {
				fmt.Fprintf(&items, "    <%s Include=\"%s\" NuGetPackageId=\"%s\" NuGetPackageVersion=\"%s\" PathInPackage=\"%s\" />\n",
					kind, full, p.id, p.version, inPackage)
			}
		}

		if p.release != nil {
			meta := "#go2cs-source-metadata v1\nmodule example.com/m\nmodule-version v1.0.0\n"

			if *p.release != "" {
				meta += "go2cs-release " + *p.release + "\n"
			}

			meta += "package example.com/m example.com.m\n"
			metaPath := filepath.Join(pkgRoot, "go2cs", "source-metadata.txt")

			if err := os.MkdirAll(filepath.Dir(metaPath), 0o755); err != nil {
				t.Fatal(err)
			}

			if err := os.WriteFile(metaPath, []byte(meta), 0o644); err != nil {
				t.Fatal(err)
			}
		}
	}

	project := filepath.Join(dir, "floor.proj")
	content := "<Project>\n  <PropertyGroup><Language>C#</Language></PropertyGroup>\n  <ItemGroup>\n" + items.String() +
		"  </ItemGroup>\n  <Import Project=\"" + targets + "\" />\n</Project>\n"

	if err := os.MkdirAll(dir, 0o755); err != nil {
		t.Fatal(err)
	}

	if err := os.WriteFile(project, []byte(content), 0o644); err != nil {
		t.Fatal(err)
	}

	return project
}
