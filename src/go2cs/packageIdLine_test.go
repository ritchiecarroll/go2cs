// packageIdLine_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the converted project's PackageId line (owner ruling, 2026-10-02): `go.` is the converted Go standard
// library only, so a standard-library project keeps `<PackageId>go.$(AssemblyName)</PackageId>` byte-identical, and a
// converted MODULE's library carries no prefixed ID -- nugetgo-pack.ps1 mints a module's one NuGet ID, the nugetgo.
// form, for the whole module (one nupkg per module, D6). An Exe keeps the template's line: it sits in a Library-only
// property group and never applies.

package main

import (
	"go/build"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

const stdLibPackageIdLine = "<PackageId>go.$(AssemblyName)</PackageId>"

func TestPackageIdLineKeepsGoForTheStandardLibraryOnly(t *testing.T) {
	go2csPath := filepath.Join(t.TempDir(), "src")
	coreProject := filepath.Join(go2csPath, "core", "strings", "strings.csproj")
	moduleProject := filepath.Join(t.TempDir(), "out", "src", "example.com", "lib", "example.com.lib.csproj")

	cases := []struct {
		name       string
		project    string
		outputType string
		options    Options
		wantGoLine bool
	}{
		{"a -stdlib run's library", coreProject, "Library", Options{convertStdLib: true, go2csPath: go2csPath}, true},
		{"a re-emitted core library (single-package or -tests)", coreProject, "Library", Options{go2csPath: go2csPath}, true},
		{"a converted module's library", moduleProject, "Library", Options{go2csPath: go2csPath}, false},
		{"a converted module's Exe", moduleProject, "Exe", Options{go2csPath: go2csPath}, true},
	}

	for _, c := range cases {
		line := packageIdLine(c.project, c.outputType, c.options)

		if c.wantGoLine && line != stdLibPackageIdLine {
			t.Errorf("%s: want %q, got %q", c.name, stdLibPackageIdLine, line)
		}

		if !c.wantGoLine && (strings.Contains(line, "<PackageId>") || !strings.Contains(line, "nugetgo-pack.ps1")) {
			t.Errorf("%s: want no PackageId and a comment naming nugetgo-pack.ps1, got %q", c.name, line)
		}

		if body := strings.TrimSuffix(strings.TrimPrefix(line, "<!--"), "-->"); strings.Contains(body, "--") {
			t.Errorf("%s: %q holds `--`, which XML 1.0 forbids inside a comment (MSB4025 at load)", c.name, line)
		}
	}
}

// The emitted projects of a real module conversion: its library carries no PackageId, its main keeps the template's
// line, and no project carries the marker.
func TestAConvertedModuleLibraryCarriesNoPrefixedPackageId(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/pid\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), "package main\n\nimport \"example.com/pid/lib\"\n\nfunc main() { println(lib.Two()) }\n")
	writeModuleFile(t, filepath.Join(appDir, "lib", "lib.go"), "package lib\n\nfunc Two() int { return 2 }\n")

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	var library, app string

	err := filepath.WalkDir(filepath.Join(options.go2csPath, "src"), func(path string, entry os.DirEntry, err error) error {
		if err != nil || entry.IsDir() || !strings.HasSuffix(path, ".csproj") {
			return err
		}

		data := readGenerated(t, path)

		if strings.Contains(data, PackageIdMarker) {
			t.Errorf("%s still carries %s", path, PackageIdMarker)
		}

		switch {
		case strings.Contains(data, "<OutputType>Library</OutputType>"):
			library = data
		case strings.Contains(data, "<OutputType>Exe</OutputType>"):
			app = data
		}

		return nil
	})

	if err != nil {
		t.Fatal(err)
	}

	if library == "" || app == "" {
		t.Fatalf("want one Library and one Exe project under %s", options.go2csPath)
	}

	if strings.Contains(library, "<PackageId>") || !strings.Contains(library, "nugetgo-pack.ps1") {
		t.Errorf("the module's library must carry no PackageId and name nugetgo-pack.ps1:\n%s", library)
	}

	if !strings.Contains(app, stdLibPackageIdLine) {
		t.Errorf("the module's Exe keeps the template's line %q:\n%s", stdLibPackageIdLine, app)
	}
}
