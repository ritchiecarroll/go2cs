// packageInfoAdoption_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards package_info.cs ADOPTION across a package boundary (packageInfoWriter.go, writePackageInfoFile).
// The writer reads a persisted package_info.cs at the output path and copies it through outside its marker
// sections. That is right for the SAME package, but a directory can change hands: go1.26 split
// internal/runtime/syscall, and the directory that held the 1.24 package's per-GOOS folder linux/ became the
// new package internal/runtime/syscall/linux. Its seeded package_info.cs was the OLD package's, and the new
// package adopted it: `using static go.@internal.runtime.syscall_package;`, CS0234 on the linux flavour (H5
// pre-seat 1's falsifier, 2026-10-14). This drives two real conversions with exactly that seed shape and
// reads the emitted file.

package main

import (
	"go/build"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func convertAdoptionModule(t *testing.T, appDir string, outRoot string) {
	t.Helper()

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           outRoot,
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
}

func TestPackageInfoIsNotAdoptedAcrossAPackageBoundary(t *testing.T) {
	// The conversions leave the per-package globals on this module's last package, and later tests read
	// them (goPackageImportPathFor reads currentPackagePath): measured, TestInternalTestPackageInfoSeedAnchorsBridgeClass
	// then stamped an ImportPath. Put back what this test changes.
	savedPath, savedName, savedNamespace := currentPackagePath, packageName, packageNamespace
	t.Cleanup(func() { currentPackagePath, packageName, packageNamespace = savedPath, savedName, savedNamespace })

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	outRoot := filepath.Join(root, "out")
	moduleOut := filepath.Join(outRoot, "src", "example.com", "split")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/split\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "sys", "sys.go"), "package sys\n\n// Old is the 1.24-shaped package.\nfunc Old() int { return 1 }\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), "package main\n\nimport \"example.com/split/sys\"\n\nfunc main() { println(sys.Old()) }\n")

	// First conversion: package sys, whose package_info.cs is the one that will be left behind.
	convertAdoptionModule(t, appDir, outRoot)

	old, err := os.ReadFile(filepath.Join(moduleOut, "sys", "package_info.cs"))

	if err != nil {
		t.Fatalf("reading the first conversion's sys/package_info.cs: %v", err)
	}

	// The seed shape: the OLD package's package_info.cs sitting in the directory a NEW package now occupies,
	// as the 1.24 per-GOOS folder's file sat in what became go1.26's internal/runtime/syscall/linux.
	writeModuleFile(t, filepath.Join(moduleOut, "sys", "linux", "package_info.cs"), string(old))
	writeModuleFile(t, filepath.Join(appDir, "sys", "linux", "linux.go"), "package linux\n\n// New is the package that takes the directory over.\nfunc New() int { return 2 }\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), "package main\n\nimport (\n\t\"example.com/split/sys\"\n\t\"example.com/split/sys/linux\"\n)\n\nfunc main() { println(sys.Old(), linux.New()) }\n")

	convertAdoptionModule(t, appDir, outRoot)

	emitted, err := os.ReadFile(filepath.Join(moduleOut, "sys", "linux", "package_info.cs"))

	if err != nil {
		t.Fatalf("reading the new package's package_info.cs: %v", err)
	}

	text := string(emitted)

	if strings.Contains(text, "sys_package") {
		t.Errorf("the new package linux adopted the OLD package's package_info.cs: it still names sys_package")
	}

	for _, want := range []string{"linux_package", `[GoPackage("linux"`} {
		if !strings.Contains(text, want) {
			t.Errorf("the new package's package_info.cs does not hold %q; it must be the template's, not the old package's", want)
		}
	}

	// The SAME package keeps its persisted file across a reconvert: the old package's own package_info.cs is
	// still adopted (its stamp names it), so this guard cannot drift into "never adopt".
	again, err := os.ReadFile(filepath.Join(moduleOut, "sys", "package_info.cs"))

	if err != nil || !strings.Contains(string(again), "sys_package") {
		t.Errorf("the old package's own package_info.cs lost its class on reconvert (err %v)", err)
	}
}

// TestPackageInfoIsNotAdoptedAcrossAPackageRename is the class's other shape, found by this seat's
// three-target footprint: the directory and import path stay, the package NAME changes. Go 1.26 renamed
// crypto/x509/internal/macos's `package macOS` to `package macos`, and adopting the persisted file kept the
// dead class macOS_package beside the macos_package the sources now declare.
func TestPackageInfoIsNotAdoptedAcrossAPackageRename(t *testing.T) {
	savedPath, savedName, savedNamespace := currentPackagePath, packageName, packageNamespace
	t.Cleanup(func() { currentPackagePath, packageName, packageNamespace = savedPath, savedName, savedNamespace })

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	outRoot := filepath.Join(root, "out")
	infoPath := filepath.Join(outRoot, "src", "example.com", "renamed", "osx", "package_info.cs")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/renamed\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "osx", "osx.go"), "package macOS\n\n// F is the package's one func.\nfunc F() int { return 1 }\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), "package main\n\nimport macOS \"example.com/renamed/osx\"\n\nfunc main() { println(macOS.F()) }\n")

	convertAdoptionModule(t, appDir, outRoot)

	if before, err := os.ReadFile(infoPath); err != nil || !strings.Contains(string(before), "macOS_package") {
		t.Fatalf("control: the first conversion's package_info.cs does not declare macOS_package (err %v)", err)
	}

	writeModuleFile(t, filepath.Join(appDir, "osx", "osx.go"), "package macos\n\n// F is the package's one func.\nfunc F() int { return 1 }\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), "package main\n\nimport \"example.com/renamed/osx\"\n\nfunc main() { println(macos.F()) }\n")

	convertAdoptionModule(t, appDir, outRoot)

	after, err := os.ReadFile(infoPath)

	if err != nil {
		t.Fatalf("reading the renamed package's package_info.cs: %v", err)
	}

	if text := string(after); strings.Contains(text, "macOS_package") || !strings.Contains(text, "macos_package") {
		t.Errorf("the renamed package's package_info.cs must declare macos_package and no longer macOS_package:\n%s", text)
	}
}
