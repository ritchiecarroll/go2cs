// readmeFromCSharp_test.go - Gbtc
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

// The "From C#" line (first-impression audit, COORD 2026-10-10): a go.* package README told a C# reader no class
// name, no namespace and no guide. One plain line after the badges now names the static class a C# caller uses and
// links the "Consuming converted Go from C#" guide. Plain text, no inline code: nuget.org's dark theme draws an
// inline-code span as a red block.
const readmeFromCSharpGuide = "[Consuming converted Go from C#](https://go2cs.net/ConsumingGoFromCSharp.html)"

func readReadme(t *testing.T, dir string) string {
	t.Helper()

	contents, err := os.ReadFile(filepath.Join(dir, "README.md"))

	if err != nil {
		t.Fatalf("read README: %v", err)
	}

	return strings.ReplaceAll(string(contents), "\r\n", "\n")
}

func TestReadmeFromCSharpLineNamesTheClassAndLinksTheGuide(t *testing.T) {
	dir := t.TempDir()

	if err := writeReadmeFile(dir, "path.filepath", "", "", "go.path.filepath_package", Options{}); err != nil {
		t.Fatal(err)
	}

	readme := readReadme(t, dir)
	want := "From C#, call this package through the static class go.path.filepath_package; see " + readmeFromCSharpGuide + "."

	lines := strings.Split(readme, "\n")
	at := -1

	for i, line := range lines {
		if line == want {
			at = i
		}
	}

	if at < 0 {
		t.Fatalf("README lacks the From C# line %q:\n%s", want, readme)
	}

	callout := -1
	rule := -1

	for i, line := range lines {
		if strings.HasPrefix(line, "> C# package converted from the Go standard library") {
			callout = i
		}

		if line == "---" && rule < 0 {
			rule = i
		}
	}

	if !(callout >= 0 && callout < at && at < rule) {
		t.Fatalf("the From C# line must sit after the callout and before the license rule (callout %d, line %d, rule %d):\n%s", callout, at, rule, readme)
	}

	if strings.Contains(lines[at], "`") {
		t.Fatalf("the From C# line carries inline code: %q", lines[at])
	}
}

func TestReadmeFromCSharpLineOmittedWithoutAClass(t *testing.T) {
	dir := t.TempDir()

	if err := writeReadmeFile(dir, "sync", "", "", "", Options{}); err != nil {
		t.Fatal(err)
	}

	if readme := readReadme(t, dir); strings.Contains(readme, "From C#") {
		t.Fatalf("a README with no class to name must carry no From C# line:\n%s", readme)
	}
}

// The -tests pipeline re-emits a README after the compare (refreshPackageReadmeAfterProof) with the converter
// globals empty; the class rides in the emission record, so the refreshed README keeps the line.
func TestReadmeFromCSharpLineSurvivesTheRefreshAfterProof(t *testing.T) {
	dir := t.TempDir()

	t.Cleanup(func() { convertedPackageReadme = nil })

	recordPackageReadmeEmission(dir, "strings", "", "", "go.strings_package")

	if err := refreshPackageReadmeAfterProof(dir, Options{}); err != nil {
		t.Fatal(err)
	}

	if readme := readReadme(t, dir); !strings.Contains(readme, "static class go.strings_package;") {
		t.Fatalf("the refreshed README lost the From C# line:\n%s", readme)
	}
}

// The class is the one package_info.cs declares: the conversion's namespace and the sanitized <name>_package.
func TestPackageReadmeCSharpClassIsThePackageInfoClass(t *testing.T) {
	savedNamespace, savedName := packageNamespace, packageName

	t.Cleanup(func() { packageNamespace, packageName = savedNamespace, savedName })

	for _, c := range []struct{ namespace, name, want string }{
		{"go", "strings", "go.strings_package"},
		{"go.path", "filepath", "go.path.filepath_package"},
		{"go.crypto.@internal", "fips140", "go.crypto.@internal.fips140_package"},
		{"", "", ""},
	} {
		packageNamespace, packageName = c.namespace, c.name

		if got := packageReadmeCSharpClass(); got != c.want {
			t.Errorf("namespace %q, package %q: got %q, want %q", c.namespace, c.name, got, c.want)
		}
	}
}
