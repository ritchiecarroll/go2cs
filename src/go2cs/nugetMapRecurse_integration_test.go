// nugetMapRecurse_integration_test.go - Gbtc
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
	"runtime"
	"testing"
)

// The -nuget-map hook in ModuleConverter.ConvertModule, end to end on TestRecurseNuGetReferences' fixture
// (an app module importing a co-located lib through a replace directive). Two runs into two roots:
//   - no -nuget-map flag: the DORMANT default -- the registry fixture is never requested and no
//     go2cs.nuget.lock is written (S3b's flip turns this red, deliberately);
//   - a user source naming the lib: the hook runs (the registry, appended last, is requested once -- the
//     positive control), the lock is written, and the lib stays UNMAPPED because a replace is never
//     overridden by a mapping.
func TestRecurseNuGetMapHook(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the app's standard-library closure via go/packages")
	}

	f := newNuGetMapFixture(t)
	f.set("/registry", mapRow("example.com/lib", "reg.lib", "canonical"))

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	libDir := filepath.Join(root, "lib")

	writeModuleFile(t, filepath.Join(libDir, "go.mod"), "module example.com/lib\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(libDir, "greeting.go"),
		"package lib\n\nimport \"strings\"\n\nfunc Greeting(name string) string {\n\treturn strings.TrimSpace(\"Hello, \"+name+\"!\")\n}\n")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"),
		"module example.com/app\n\ngo 1.23\n\nrequire example.com/lib v0.0.0\n\nreplace example.com/lib => ../lib\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\nimport (\n\t\"fmt\"\n\n\t\"example.com/lib\"\n)\n\nfunc main() {\n\tfmt.Println(lib.Greeting(\"go2cs\"))\n}\n")

	goRoot := build.Default.GOROOT
	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	build.Default.GOROOT = goRoot

	convert := func(outRoot string, mapOptions nugetMapOptions) {
		t.Helper()

		options := Options{
			goRoot:              goRoot,
			goPath:              build.Default.GOPATH,
			go2csPath:           filepath.Join(root, "runtime"),
			recurseOutputRoot:   outRoot,
			recurse:             true,
			nugetRefs:           true,
			nugetMap:            mapOptions,
			targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
			indentSpaces:        4,
			preferVarDecl:       true,
			useChannelOperators: true,
		}

		if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
			t.Fatalf("ConvertModule: %v", err)
		}
	}

	offRoot := filepath.Join(root, "out-off")
	convert(offRoot, nugetMapOptions{off: true})

	if n := f.count("/registry"); n != 0 {
		t.Errorf("a -recurse=nuget run with -nuget-map off requested the registry %d times; want 0", n)
	}

	if _, err := os.Stat(nugetLockPath(offRoot)); !os.IsNotExist(err) {
		t.Errorf("a -recurse=nuget run with -nuget-map off wrote %s", nugetLockFileName)
	}

	defaultRoot := filepath.Join(root, "out-default")
	convert(defaultRoot, nugetMapOptions{})

	if n := f.count("/registry"); n != 1 {
		t.Errorf("a -recurse=nuget run with no -nuget-map flag requested the registry %d times; want 1 (on by default since S3b)", n)
	}

	mappedRoot := filepath.Join(root, "out-mapped")
	mine := writeMapFile(t, "mine.txt", "# no rows of my own\n")
	convert(mappedRoot, nugetMapOptions{sources: []string{mine}})

	if n := f.count("/registry"); n != 2 {
		t.Errorf("with a -nuget-map source the registry was requested %d times in total; want 2 (one per run)", n)
	}

	lock, err := readNuGetLock(mappedRoot)

	if err != nil {
		t.Fatal(err)
	}

	if _, statErr := os.Stat(nugetLockPath(mappedRoot)); statErr != nil {
		t.Errorf("with a -nuget-map source no %s was written: %v", nugetLockFileName, statErr)
	}

	if _, mapped := lock["example.com/lib"]; mapped {
		t.Errorf("the replaced lib was locked as mapped: %+v", lock)
	}
}
