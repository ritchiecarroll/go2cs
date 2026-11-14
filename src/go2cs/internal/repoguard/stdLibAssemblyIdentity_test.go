// stdLibAssemblyIdentity_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"os"
	"path"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"testing"
)

// Every converted standard-library LIBRARY assembly is named after its NuGet package ID (OQ4,
// docs/phase4/DESIGN-assembly-identity.md): package go.fmt ships go.fmt.dll. Two guards over the committed projects:
//
//   - The PACKAGE ID IS FROZEN. Each project's evaluated PackageId is go.<its directory's import path, dotted>, the
//     value it had before the rename. The rename changes the PackageId line's spelling ($(AssemblyName)), never its
//     value: a changed NuGet ID is a different package to every consumer.
//   - The ASSEMBLY NAME IS THE PACKAGE ID.
//
// golib (package go.lib, assembly golib), go.arrays (assembly go.arrays, packed inside go.lib) and go2cs.CpuProfiler
// (not packed) are outside the rule (the design's OQ-A).

var (
	stdLibAssemblyNameElement = regexp.MustCompile(`<AssemblyName>([^<]+)</AssemblyName>`)
	stdLibPackageIdElement    = regexp.MustCompile(`<PackageId>([^<]+)</PackageId>`)
)

// stdLibProjectIdentity is one standard-library library project's identity as its .csproj declares it.
type stdLibProjectIdentity struct {
	path      string // repository-relative .csproj
	want      string // go.<directory import path, dotted>
	assembly  string // the declared AssemblyName
	packageId string // the declared PackageId, with $(AssemblyName) evaluated
}

// stdLibIdentityViolations lists one line per project whose package ID moved or whose assembly name is not its
// package ID.
func stdLibIdentityViolations(projects []stdLibProjectIdentity) []string {
	var violations []string

	for _, p := range projects {
		if p.packageId != p.want {
			violations = append(violations, p.path+": PackageId evaluates to "+p.packageId+", want "+p.want+" (a package ID never changes)")
		}

		if p.assembly != p.want {
			violations = append(violations, p.path+": AssemblyName is "+p.assembly+", want "+p.want+" (its package ID)")
		}
	}

	sort.Strings(violations)

	return violations
}

// readStdLibProjectIdentities reads every tracked standard-library library .csproj under src/core.
func readStdLibProjectIdentities(t *testing.T) []stdLibProjectIdentity {
	t.Helper()

	root := repoRootFromPackageDir(t)
	var projects []stdLibProjectIdentity

	for _, rel := range gitTrackedFiles(t, root) {
		if !strings.HasPrefix(rel, "src/core/") || !strings.HasSuffix(rel, ".csproj") || strings.HasSuffix(rel, ".tests.csproj") ||
			strings.HasPrefix(rel, "src/core/golib/") || strings.HasPrefix(rel, "src/core/go.arrays/") || strings.HasPrefix(rel, "src/core/go2cs.CpuProfiler/") {
			continue
		}

		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))

		if err != nil {
			t.Fatalf("%s: %v", rel, err)
		}

		text := string(data)
		dir := path.Dir(strings.TrimPrefix(rel, "src/core/"))
		p := stdLibProjectIdentity{path: rel, want: "go." + strings.ReplaceAll(dir, "/", ".")}

		if m := stdLibAssemblyNameElement.FindStringSubmatch(text); m != nil {
			p.assembly = m[1]
		}

		if m := stdLibPackageIdElement.FindStringSubmatch(text); m != nil {
			p.packageId = strings.ReplaceAll(m[1], "$(AssemblyName)", p.assembly)
		}

		projects = append(projects, p)
	}

	return projects
}

// TestStdLibAssemblyIsItsFrozenPackageId: each standard-library library's evaluated PackageId is unchanged, and its
// AssemblyName equals it.
func TestStdLibAssemblyIsItsFrozenPackageId(t *testing.T) {
	projects := readStdLibProjectIdentities(t)

	if len(projects) < 300 {
		t.Fatalf("found %d standard-library library projects, too few to be this repository; a guard that reads nothing passes everything", len(projects))
	}

	for _, violation := range stdLibIdentityViolations(projects) {
		t.Error(violation)
	}
}

// TestStdLibIdentityGuardNamesEachKind: a moved package ID and a simple assembly name are each named, by project.
func TestStdLibIdentityGuardNamesEachKind(t *testing.T) {
	violations := stdLibIdentityViolations([]stdLibProjectIdentity{
		{path: "src/core/fmt/fmt.csproj", want: "go.fmt", assembly: "go.fmt", packageId: "go.fmt"},
		{path: "src/core/net/net.csproj", want: "go.net", assembly: "net", packageId: "go.net"},
		{path: "src/core/log/log.csproj", want: "go.log", assembly: "go.log", packageId: "go.go.log"},
	})

	named := strings.Join(violations, "\n")

	if len(violations) != 2 || !strings.Contains(named, "src/core/net/net.csproj: AssemblyName is net") ||
		!strings.Contains(named, "src/core/log/log.csproj: PackageId evaluates to go.go.log") {
		t.Errorf("want exactly the net assembly name and the log package ID named; got:\n%s", named)
	}
}
