// recurseTrimpath_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"io/fs"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// TestRecurseOutputCarriesNoAbsoluteSourcePath converts a local main module (a main package and a subpackage of the
// same module) with -recurse from a temp directory into a separate output root, and requires that NO emitted file
// names the machine's directories: not the module's source directory and not the go2cs runtime root, in either slash
// direction and either case.
//
// Two surfaces leaked before. Every converted file's GoPositionMap record carried the absolute source path
// (`C:/…/src/main.go`), which is what an ordinary untrimmed `go build` bakes, but -recurse output is a source
// artifact that is built and shipped elsewhere. And the output root's Directory.Build.props pinned go2csPath to the
// absolute runtime root even when the user never named one. A main-module source is now recorded the way
// `go build -trimpath` bakes it, `<module path>/<path in module>`, and the pin names a machine path only when
// -go2cspath was given explicitly (COORD's ruling, 2026-11-10).
func TestRecurseOutputCarriesNoAbsoluteSourcePath(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: loads the module's standard-library closure via go/packages")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "work", "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/trimapp\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "util", "util.go"),
		"package util\n\nfunc Double(n int) int { return n * 2 }\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\nimport (\n\t\"fmt\"\n\n\t\"example.com/trimapp/util\"\n)\n\nfunc main() {\n\tfmt.Println(util.Double(21))\n}\n")

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

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	// Every spelling a machine path could take in an emitted file.
	var needles []string

	for _, dir := range []string{root, appDir, options.go2csPath} {
		abs, err := filepath.Abs(dir)

		if err != nil {
			t.Fatal(err)
		}

		needles = append(needles, strings.ToLower(abs), strings.ToLower(filepath.ToSlash(abs)))
	}

	scanned := 0

	err := filepath.WalkDir(options.recurseOutputRoot, func(path string, entry fs.DirEntry, err error) error {
		if err != nil {
			return err
		}

		if entry.IsDir() {
			// Build output and the generator's own files are not the conversion's.
			if name := entry.Name(); name == ".artifacts" || name == "bin" || name == "obj" || name == "Generated" {
				return filepath.SkipDir
			}

			return nil
		}

		data, err := os.ReadFile(path)

		if err != nil {
			return err
		}

		scanned++
		text := strings.ToLower(string(data))

		for _, needle := range needles {
			if strings.Contains(text, needle) {
				rel, _ := filepath.Rel(options.recurseOutputRoot, path)
				t.Errorf("%s names a machine path (%s)", filepath.ToSlash(rel), needle)
				break
			}
		}

		return nil
	})

	if err != nil {
		t.Fatal(err)
	}

	if scanned == 0 {
		t.Fatal("the conversion emitted no files to scan")
	}

	// The position maps carry the -trimpath identity instead: the module path, then the path inside the module.
	for _, rel := range []string{filepath.Join("src", "example.com", "trimapp", "package_info.cs"),
		filepath.Join("src", "example.com", "trimapp", "util", "package_info.cs")} {
		info := readGenerated(t, filepath.Join(options.recurseOutputRoot, rel))

		if !strings.Contains(info, `GoPositionMap("example.com/trimapp@/`) {
			t.Errorf("%s records no module-relative identity:\n%s", filepath.ToSlash(rel), info)
		}
	}
}
