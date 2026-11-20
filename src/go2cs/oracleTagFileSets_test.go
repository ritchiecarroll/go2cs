// oracleTagFileSets_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"encoding/json"
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"slices"
	"sort"
	"strings"
	"testing"
	"time"
)

// TestOracleTagFileSetsOverTheRoster is an INSTRUMENT, not a gate: it skips unless GO2CS_ORACLE_TAGS_OUT names a TSV
// to write. It checks oracleBuildFlags' premise per roster row with `go list`: dropping math_big_pure_go changes no
// file of the package under test or of its tests (GoFiles, CgoFiles, TestGoFiles, XTestGoFiles), on any target, for
// every row but math/big's own. math/big is the positive control: there the file sets must differ.
//
// Rows: every standard-library row of docs/ValidatedTestPackages.md (tags purego,math_big_pure_go against purego), and
// every package of every module in docs/ValidatedModules.md (the -recurse set against the same less the tag), the
// module resolved in a scratch module that requires it. Targets windows, linux and darwin at amd64, CGO_ENABLED=0 as
// the roster is measured. Each TSV row: target, goos, kind, equal|DIFFERS, and the files that differ.
//
//	GO2CS_ORACLE_TAGS_OUT    TSV path to write (required)
//	GO2CS_ORACLE_TAGS_DOCS   the docs directory (default: ../../docs from this package)
func TestOracleTagFileSetsOverTheRoster(t *testing.T) {
	out := os.Getenv("GO2CS_ORACLE_TAGS_OUT")
	if out == "" {
		t.Skip("instrument: set GO2CS_ORACLE_TAGS_OUT to run")
	}

	docs := os.Getenv("GO2CS_ORACLE_TAGS_DOCS")
	if docs == "" {
		docs = filepath.Join("..", "..", "docs")
	}

	stdRows := oracleTagRosterRows(t, filepath.Join(docs, "ValidatedTestPackages.md"), stdRosterRowPattern)
	modules := oracleTagRosterRows(t, filepath.Join(docs, "ValidatedModules.md"), moduleRosterRowPattern)

	if len(stdRows) == 0 || len(modules) == 0 {
		t.Fatalf("roster read nothing: %d std rows, %d modules", len(stdRows), len(modules))
	}

	stdTags := resolveBuildTags(false, true, false, false, false, nil)
	moduleTags := resolveBuildTags(false, true, true, true, false, nil)

	scratch := t.TempDir()
	goMod := "module example.test/oracletags\n\ngo 1.23\n"

	if err := os.WriteFile(filepath.Join(scratch, "go.mod"), []byte(goMod), 0o644); err != nil {
		t.Fatal(err)
	}

	var modulePatterns []string

	for _, module := range modules {
		path, version, _ := strings.Cut(module, "@")

		if output, err := runCommandWithTimeout(10*time.Minute, scratch, Options{}, "go", "get", path+"@"+version); err != nil {
			t.Fatalf("go get %s: %v\n%s", module, err, output)
		}

		modulePatterns = append(modulePatterns, path+"/...")
	}

	rows := []string{"target\tgoos\tkind\treading\tdiffering_files"}
	counts := map[string]int{}

	for _, goos := range []string{"windows", "linux", "darwin"} {
		for _, kind := range []struct {
			name     string
			dir      string
			tags     []string
			patterns []string
		}{
			{"std", scratch, stdTags, stdRows},
			{"module", scratch, moduleTags, modulePatterns},
		} {
			with := oracleTagGoList(t, kind.dir, goos, kind.tags, kind.patterns)
			without := oracleTagGoList(t, kind.dir, goos, slices.DeleteFunc(slices.Clone(kind.tags), func(tag string) bool {
				return tag == mathBigPureGoTag
			}), kind.patterns)

			paths := make([]string, 0, len(with))

			for path := range with {
				paths = append(paths, path)
			}

			sort.Strings(paths)

			for _, path := range paths {
				differing := oracleTagFileDiff(with[path], without[path])
				reading := "equal"

				if len(differing) > 0 {
					reading = "DIFFERS"
				}

				counts[kind.name+" "+goos+" "+reading]++
				rows = append(rows, fmt.Sprintf("%s\t%s\t%s\t%s\t%s", path, goos, kind.name, reading, strings.Join(differing, ",")))
			}
		}
	}

	if err := os.WriteFile(out, []byte(strings.Join(rows, "\n")+"\n"), 0o644); err != nil {
		t.Fatal(err)
	}

	keys := make([]string, 0, len(counts))

	for key := range counts {
		keys = append(keys, key)
	}

	sort.Strings(keys)

	for _, key := range keys {
		t.Logf("READING %s: %d", key, counts[key])
	}
}

var (
	stdRosterRowPattern    = regexp.MustCompile("(?m)^\\|\\s*\\[`([^`]+)`\\]\\(")
	moduleRosterRowPattern = regexp.MustCompile("(?m)^\\| `([^`]+)` \\| (v[^ ]+) \\|")
)

// oracleTagRosterRows reads a roster's rows: an import path per standard-library row, or module@version per module.
func oracleTagRosterRows(t *testing.T, file string, pattern *regexp.Regexp) []string {
	t.Helper()

	data, err := os.ReadFile(file)
	if err != nil {
		t.Fatal(err)
	}

	seen := map[string]bool{}
	var rows []string

	for _, match := range pattern.FindAllStringSubmatch(string(data), -1) {
		row := match[1]

		if len(match) > 2 {
			row += "@" + match[2]
		}

		if !seen[row] {
			seen[row] = true
			rows = append(rows, row)
		}
	}

	return rows
}

type oracleTagPackage struct {
	ImportPath   string
	GoFiles      []string
	CgoFiles     []string
	TestGoFiles  []string
	XTestGoFiles []string
}

// oracleTagGoList lists the four file sets of each package under the given tags and target.
func oracleTagGoList(t *testing.T, dir, goos string, tags, patterns []string) map[string]oracleTagPackage {
	t.Helper()

	args := append([]string{"list", "-e", "-json=ImportPath,GoFiles,CgoFiles,TestGoFiles,XTestGoFiles", "-tags=" + strings.Join(tags, ",")}, patterns...)
	output, err := runCommandWithTimeoutEnv(10*time.Minute, dir, Options{targetPlatform: goos + "/amd64"},
		[]string{"CGO_ENABLED=0", "GOFLAGS=-mod=mod", "GOWORK=off"}, "go", args...)
	if err != nil {
		t.Fatalf("go %s (%s): %v\n%s", strings.Join(args, " "), goos, err, output)
	}

	// The child's stderr shares the stream: drop the go command's own progress lines (`go: downloading …`).
	var jsonLines []string

	for _, line := range strings.Split(output, "\n") {
		if !strings.HasPrefix(line, "go: ") {
			jsonLines = append(jsonLines, line)
		}
	}

	result := map[string]oracleTagPackage{}
	decoder := json.NewDecoder(strings.NewReader(strings.Join(jsonLines, "\n")))

	for decoder.More() {
		var pkg oracleTagPackage

		if err := decoder.Decode(&pkg); err != nil {
			t.Fatalf("decoding go list output (%s): %v", goos, err)
		}

		result[pkg.ImportPath] = pkg
	}

	return result
}

// oracleTagFileDiff names every file in exactly one of the two listings, prefixed with its set.
func oracleTagFileDiff(a, b oracleTagPackage) []string {
	var differing []string

	for _, set := range []struct {
		name string
		a, b []string
	}{
		{"GoFiles", a.GoFiles, b.GoFiles},
		{"CgoFiles", a.CgoFiles, b.CgoFiles},
		{"TestGoFiles", a.TestGoFiles, b.TestGoFiles},
		{"XTestGoFiles", a.XTestGoFiles, b.XTestGoFiles},
	} {
		for _, file := range set.a {
			if !slices.Contains(set.b, file) {
				differing = append(differing, set.name+":-"+file)
			}
		}

		for _, file := range set.b {
			if !slices.Contains(set.a, file) {
				differing = append(differing, set.name+":+"+file)
			}
		}
	}

	return differing
}
