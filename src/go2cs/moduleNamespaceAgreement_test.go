// moduleNamespaceAgreement_test.go - Gbtc
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
	"strings"
	"testing"
)

// A module path is spelled TWICE by every conversion, and the two spellings have to be the same one.
//
// The DECLARATION side is getProjectName (importOperations.go): from the package's directory it
// recovers the import path — the go.mod module path plus the module-relative directory — and returns
// the project name and the emitted `namespace`. The IMPORT side is convertImportPathToNamespace
// (visitImportSpec.go): from the import path as an importer WROTE it, it composes the
// `using <alias> = …` target, the bare `using <namespace>;`, and the `typeof` of an init-forcing hook.
// Nothing forces those two derivations to agree; only a test does.
//
// They disagreed for exactly one shape, and it was invisible because the shape had no working
// consumer. The repository's own `go2cs/` module marker was elided by getProjectName and kept by
// convertImportPathToNamespace, so `module go2cs/convertedtestharness` declared
// `go.convertedtestharness_package` while its own external test variant imported
// `go2cs.convertedtestharness_package` — a namespace nothing emits. CS0234 ×2, which is why the
// end-to-end `-tests` fixture at src/tests/PackageTests/ConvertedTestHarness never built and why the
// 46 BARE `module <Name>` declarations in the behavioral corpus are bare: a nested sub-library is
// imported by its parent, so it had to drop the marker or meet this.
//
// The guard pins the AGREEMENT rather than either side's output, because either side alone can be
// self-consistently wrong. It runs the marker case and its no-marker control through both
// derivations and requires one answer.
func TestModulePathNamespaceAgreesAcrossSides(t *testing.T) {
	savedDirs := importPackageDirs
	defer func() { importPackageDirs = savedDirs }()

	tests := []struct {
		name string
		// modulePath is what go.mod declares AND what an importer writes.
		modulePath string
		// packageName is the Go package the module root declares.
		packageName string
		// wantClass is the fully rooted C# class both sides must name.
		wantClass string
	}{
		{
			name:        "repository marker on a single-segment module",
			modulePath:  "go2cs/convertedtestharness",
			packageName: "convertedtestharness",
			wantClass:   "go.convertedtestharness_package",
		},
		{
			name:        "repository marker on a multi-segment module",
			modulePath:  "go2cs/foo/bar",
			packageName: "bar",
			wantClass:   "go.foo.bar_package",
		},
		{
			// The control that makes the marker rule a rule and not a coincidence: a bare module
			// must land exactly where the marked one does. This is the equivalence the corpus's 46
			// bare declarations and its 618 `go2cs/<Name>` declarations both depend on.
			name:        "bare module reaches the same class as the marked one",
			modulePath:  "convertedtestharness",
			packageName: "convertedtestharness",
			wantClass:   "go.convertedtestharness_package",
		},
		{
			// A module path with no marker to elide, so both sides carry every segment. Measured
			// against a real `-recurse` conversion of `module example.com/foo/bar`: the declaration
			// emits `namespace go.example.com.foo;` + `leaf_package` and the importer emits
			// `using leaf = go.example.com.foo.bar.leaf_package;`. Nothing about the fix may move it.
			name:        "ordinary module path is untouched",
			modulePath:  "example.com/foo/bar",
			packageName: "bar",
			wantClass:   "go.example.com.foo.bar_package",
		},
	}

	goRoot := testGoRoot(t)

	for _, test := range tests {
		t.Run(test.name, func(t *testing.T) {
			// The DECLARATION side reads a directory, so it needs one: a module root holding the
			// go.mod that declares the path plus one Go file, which is what getProjectName's
			// module branch keys on.
			moduleDir := t.TempDir()

			writeFixtureFile(t, filepath.Join(moduleDir, "go.mod"),
				"module "+test.modulePath+"\n\ngo 1.23\n")
			writeFixtureFile(t, filepath.Join(moduleDir, test.packageName+".go"),
				"package "+test.packageName+"\n")

			projectName, namespace := getProjectName(moduleDir, Options{goRoot: goRoot})

			// The emitted class is the namespace plus the package class. getProjectName's project
			// name is the dotted import path, so its LAST segment is the package's own — the same
			// segment convertImportPathToNamespace turns into `<name>_package`.
			leaf := projectName

			if lastDot := strings.LastIndex(projectName, "."); lastDot != -1 {
				leaf = projectName[lastDot+1:]
			}

			declared := namespace + "." + getCoreSanitizedIdentifier(leaf) + PackageSuffix

			// The IMPORT side works from the path as written, with the import graph empty so the
			// path tail — not a graph-supplied package name — is what composes the class. That
			// isolates the PATH rule from the package-name rule TestImportedPackageClassFollowsPackageName
			// already pins.
			importPackageDirs = map[string]importedPackageMeta{}

			imported := RootNamespace + "." + convertImportPathToNamespace(test.modulePath, PackageSuffix)

			if declared != test.wantClass {
				t.Errorf("DECLARATION side: getProjectName(%q) composes %q, want %q\n"+
					"  (project name %q, namespace %q)",
					test.modulePath, declared, test.wantClass, projectName, namespace)
			}

			if imported != test.wantClass {
				t.Errorf("IMPORT side: convertImportPathToNamespace(%q) composes %q, want %q",
					test.modulePath, imported, test.wantClass)
			}

			if declared != imported {
				t.Errorf("the two sides disagree on %q:\n"+
					"  declaration emits %q\n"+
					"  importers emit    %q\n"+
					"An importer that names a namespace the declaration never emits is CS0234, and the\n"+
					"package cannot be imported at all. See trimGo2CSModulePrefix (importOperations.go).",
					test.modulePath, declared, imported)
			}
		})
	}
}

// writeFixtureFile writes one file of a temporary module fixture, failing the test rather than
// returning an error — a fixture that could not be written is a broken test, never a finding.
func writeFixtureFile(t *testing.T, path string, content string) {
	t.Helper()

	if err := os.WriteFile(path, []byte(content), 0o644); err != nil {
		t.Fatalf("could not write module fixture %q: %v", path, err)
	}
}
