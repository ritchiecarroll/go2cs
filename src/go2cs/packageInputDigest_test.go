// packageInputDigest_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"os"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

// The nugetgo rehearsal (2026-10-09, gap 4): a module's VALIDATION.md came from a `-recurse -tests` root and its
// assembly from a separate `-recurse=nuget` root, and nothing tied the two together. A module package's project now
// records GoInputDigest, a digest of what its production conversion is a function of; the proof page and MODULE.md
// record the same value, and nugetgo-pack.ps1 refuses a proof whose digest is not the packed tree's.

var inputDigestPattern = regexp.MustCompile(`^sha256-[0-9a-f]{64}$`)

func writeDigestFixture(t *testing.T, dir string, files map[string]string) []string {
	t.Helper()

	var paths []string

	for name, content := range files {
		path := filepath.Join(dir, name)

		if err := os.MkdirAll(filepath.Dir(path), 0o755); err != nil {
			t.Fatal(err)
		}

		if err := os.WriteFile(path, []byte(content), 0o644); err != nil {
			t.Fatal(err)
		}

		if strings.HasSuffix(name, ".go") {
			paths = append(paths, path)
		}
	}

	return paths
}

func TestPackageInputDigestMovesWithEachInput(t *testing.T) {
	base := Options{targetPlatform: "linux/amd64", indentSpaces: 4, preferVarDecl: true, useChannelOperators: true}
	sources := map[string]string{"a.go": "package p\n\nfunc A() int { return 1 }\n", "b.go": "package p\n\nfunc B() int { return 2 }\n"}

	digest := func(t *testing.T, files map[string]string, companion string, options Options, revision string) string {
		t.Helper()

		root := t.TempDir()
		paths := writeDigestFixture(t, filepath.Join(root, "in"), files)
		output := filepath.Join(root, "out")

		if err := os.MkdirAll(output, 0o755); err != nil {
			t.Fatal(err)
		}

		if companion != "" {
			if err := os.WriteFile(filepath.Join(output, "p_impl.cs"), []byte(companion), 0o644); err != nil {
				t.Fatal(err)
			}
		}

		got, err := packageInputDigest(paths, output, options, revision)

		if err != nil {
			t.Fatalf("packageInputDigest: %v", err)
		}

		return got
	}

	reference := digest(t, sources, "", base, "rev-1")

	if !inputDigestPattern.MatchString(reference) {
		t.Fatalf("digest %q is not sha256-<64 hex>", reference)
	}

	// The same inputs in another directory read the same: the digest is portable, never a host path.
	if again := digest(t, sources, "", base, "rev-1"); again != reference {
		t.Errorf("the same inputs at another location read %s, want %s", again, reference)
	}

	edited := map[string]string{"a.go": sources["a.go"] + "\n", "b.go": sources["b.go"]}
	added := map[string]string{"a.go": sources["a.go"], "b.go": sources["b.go"], "c.go": "package p\n"}
	otherPlatform, otherOption := base, base
	otherPlatform.targetPlatform = "windows/amd64"
	otherOption.preferVarDecl = false

	for _, c := range []struct {
		name string
		got  string
	}{
		{"a source byte", digest(t, edited, "", base, "rev-1")},
		{"another source file", digest(t, added, "", base, "rev-1")},
		{"a hand-owned companion", digest(t, sources, "// hand-owned\n", base, "rev-1")},
		{"the target platform", digest(t, sources, "", otherPlatform, "rev-1")},
		{"a conversion option", digest(t, sources, "", otherOption, "rev-1")},
		{"the converter revision", digest(t, sources, "", base, "rev-2")},
	} {
		if c.got == reference {
			t.Errorf("changing %s left the digest at %s", c.name, reference)
		}
	}

	// -tests always converts with comments (main.go, moduleTestsDriver.go) and a -recurse=nuget root by default does
	// not; comments move only the emitted C#'s line layout, so the proof and the packed tree must read alike.
	withComments := base
	withComments.includeComments = true

	if got := digest(t, sources, "", withComments, "rev-1"); got != reference {
		t.Errorf("-comments moved the digest (%s, want %s): a -tests proof could never match a -recurse=nuget root", got, reference)
	}
}

func TestOnlyAModulePackageProjectRecordsAnInputDigest(t *testing.T) {
	project := filepath.Join(t.TempDir(), "example.com.mod.csproj")
	cases := []struct {
		name    string
		options Options
		want    bool
	}{
		{"a -recurse module package", Options{recurse: true}, true},
		{"a -recurse=nuget module package", Options{recurse: true, nugetRefs: true}, true},
		{"a single-package conversion", Options{}, false},
		{"the standard library", Options{recurse: true, convertStdLib: true}, false},
	}

	for _, c := range cases {
		if got := recordsInputDigest(project, c.options); got != c.want {
			t.Errorf("%s: recordsInputDigest = %v, want %v", c.name, got, c.want)
		}
	}

	contents := "<Project Sdk=\"Microsoft.NET.Sdk\">\r\n\r\n  <PropertyGroup>\r\n    <OutputType>Library</OutputType>\r\n  </PropertyGroup>\r\n\r\n  <PropertyGroup>\r\n  </PropertyGroup>\r\n</Project>"
	digest := "sha256-" + strings.Repeat("ab", 32)
	written := insertInputDigestProperty(contents, digest)

	if got := projectInputDigest(written); got != digest {
		t.Errorf("projectInputDigest read %q back, want %q:\n%s", got, digest, written)
	}

	if strings.Count(written, "<GoInputDigest>") != 1 || strings.Index(written, "<GoInputDigest>") > strings.Index(written, "</PropertyGroup>") {
		t.Errorf("want one GoInputDigest in the first PropertyGroup:\n%s", written)
	}

	if insertInputDigestProperty(contents, "") != contents {
		t.Errorf("an empty digest changed the project")
	}
}

// End to end: a -recurse conversion's module packages carry the digest; converting again with -comments into another
// root records the same values.
func TestRecurseModuleProjectsRecordTheirInputDigest(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the app's standard-library closure via go/packages")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	libDir := filepath.Join(root, "lib")

	writeModuleFile(t, filepath.Join(libDir, "go.mod"), "module example.com/lib\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(libDir, "greeting.go"),
		"package lib\n\nimport \"strings\"\n\n// Greeting greets.\nfunc Greeting(name string) string {\n\treturn strings.TrimSpace(\"Hello, \"+name+\"!\")\n}\n")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"),
		"module example.com/app\n\ngo 1.23\n\nrequire example.com/lib v0.0.0\n\nreplace example.com/lib => ../lib\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\nimport (\n\t\"fmt\"\n\n\t\"example.com/lib\"\n)\n\nfunc main() {\n\tfmt.Println(lib.Greeting(\"go2cs\"))\n}\n")

	goRoot := build.Default.GOROOT
	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	build.Default.GOROOT = goRoot

	convert := func(out string, comments bool) (string, string) {
		t.Helper()

		options := Options{
			goRoot: goRoot, goPath: build.Default.GOPATH, go2csPath: filepath.Join(root, "runtime"),
			recurseOutputRoot: filepath.Join(root, out), recurse: true, nugetRefs: true, includeComments: comments,
			targetPlatform: runtime.GOOS + "/" + runtime.GOARCH, indentSpaces: 4, preferVarDecl: true, useChannelOperators: true,
		}

		if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
			t.Fatalf("ConvertModule: %v", err)
		}

		app := projectInputDigest(readGenerated(t, filepath.Join(options.recurseOutputRoot, "src", "example.com", "app", "example.com.app.csproj")))
		lib := projectInputDigest(readGenerated(t, filepath.Join(options.recurseOutputRoot, "pkg", "example.com", "lib", "example.com.lib.csproj")))

		return app, lib
	}

	app, lib := convert("out", false)

	if !inputDigestPattern.MatchString(app) || !inputDigestPattern.MatchString(lib) || app == lib {
		t.Fatalf("app %q, lib %q: want two distinct sha256-<64 hex> digests", app, lib)
	}

	if app2, lib2 := convert("out-comments", true); app2 != app || lib2 != lib {
		t.Errorf("with -comments: app %s lib %s, want %s %s", app2, lib2, app, lib)
	}
}

// The proof records the digest its own production conversion wrote: the page states it, and MODULE.md (packed as
// VALIDATION.md) carries it per package, where nugetgo-pack.ps1 reads it.
func TestThirdPartyProofRecordsTheInputDigest(t *testing.T) {
	temp := t.TempDir()
	root, _ := fakeCheckout(t, filepath.Join(temp, "repo"))
	outRoot := filepath.Join(root, "modout")
	output := filepath.Join(outRoot, "src", "example.com", "mod", "sub")
	digest := "sha256-" + strings.Repeat("0123456789abcdef", 4)

	if err := os.MkdirAll(output, 0o755); err != nil {
		t.Fatal(err)
	}

	project := insertInputDigestProperty("<Project>\r\n  <PropertyGroup>\r\n  </PropertyGroup>\r\n</Project>", digest)

	if err := os.WriteFile(filepath.Join(output, "example.com.mod.sub.csproj"), []byte(project), 0o644); err != nil {
		t.Fatal(err)
	}

	comparison := testComparison{
		Package: "example.com/mod/sub", Status: "validated", Matched: true,
		Go: map[string]string{"TestA": "pass"}, CSharp: map[string]string{"TestA": "pass"},
	}
	manifest := testManifest{PackageImportPath: "example.com/mod/sub", ModulePath: "example.com/mod", ProjectName: "example.com.mod.sub", GoVersion: "go1.24.13"}

	if err := emitValidationProofPage(output, comparison, manifest, nil, nil, Options{targetPlatform: "linux/amd64"}); err != nil {
		t.Fatalf("emitValidationProofPage: %v", err)
	}

	page := readGenerated(t, filepath.Join(outRoot, "validation", "example.com", "mod", "sub.md"))

	if !strings.Contains(page, "Input digest `"+digest+"`") {
		t.Errorf("the proof page must state its input digest %s:\n%s", digest, page)
	}

	module := readGenerated(t, filepath.Join(outRoot, "validation", "example.com", "mod", "MODULE.md"))

	if !strings.Contains(module, "| Package | Matched | Disclosed | Input digest | Proof |") ||
		!strings.Contains(module, "| `example.com/mod/sub` | 1 | 0 | `"+digest+"` | [sub.md](sub.md) |") {
		t.Errorf("MODULE.md must carry the package's input digest:\n%s", module)
	}

	// A standard-library page has no such line: it is not packed by nugetgo-pack.ps1, and adding one would rewrite
	// every banked page.
	stdlib := renderValidationProofPage(proofPageProvenance{importPath: "sort", goVersion: "1.24.13", platform: "linux/amd64"},
		testComparison{Package: "sort", Status: "validated", Matched: true, Go: map[string]string{"TestA": "pass"}, CSharp: map[string]string{"TestA": "pass"}},
		nil, nil)

	if strings.Contains(stdlib, "Input digest") {
		t.Errorf("a standard-library page must render exactly as before:\n%s", stdlib)
	}
}
