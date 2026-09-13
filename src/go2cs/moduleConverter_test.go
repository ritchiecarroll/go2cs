// moduleConverter_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"path/filepath"
	"reflect"
	"sort"
	"testing"

	"golang.org/x/tools/go/packages"
)

// TestModuleConverterClassify locks the recurse partitioning rules: stdlib is recognized by its
// location under $GOROOT/src (covering GOROOT-vendored packages), the app by main-module
// membership, third-party by dependency-module membership, and pseudo/test packages are skipped.
func TestModuleConverterClassify(t *testing.T) {
	goRoot := filepath.FromSlash("C:/go")
	goRootSrc := filepath.Join(goRoot, "src")
	m := &ModuleConverter{options: Options{goRoot: goRoot}}

	appDir := filepath.FromSlash("C:/work/app")
	libDir := filepath.FromSlash("C:/work/lib")
	cacheDir := filepath.FromSlash("C:/gopath/pkg/mod/github.com/x/y@v1.2.3")

	appMod := &packages.Module{Path: "example.com/app", Main: true, Dir: appDir}
	libMod := &packages.Module{Path: "example.com/lib", Main: false, Dir: libDir}
	depMod := &packages.Module{Path: "github.com/x/y", Main: false, Dir: cacheDir}

	cases := []struct {
		name string
		pkg  *packages.Package
		want packageClass
	}{
		{"stdlib fmt", &packages.Package{PkgPath: "fmt", Dir: filepath.Join(goRootSrc, "fmt")}, classStdLib},
		{"goroot vendored", &packages.Package{PkgPath: "golang.org/x/net/dns/dnsmessage", Dir: filepath.Join(goRootSrc, "vendor", "golang.org", "x", "net", "dns", "dnsmessage")}, classStdLib},
		{"app main module", &packages.Package{PkgPath: "example.com/app", Dir: appDir, Module: appMod}, classApp},
		{"app subpackage", &packages.Package{PkgPath: "example.com/app/internal/util", Dir: filepath.Join(appDir, "internal", "util"), Module: appMod}, classApp},
		{"replace lib (third-party)", &packages.Package{PkgPath: "example.com/lib", Dir: libDir, Module: libMod}, classThirdParty},
		{"module cache dep", &packages.Package{PkgPath: "github.com/x/y", Dir: cacheDir, Module: depMod}, classThirdParty},
		{"unsafe", &packages.Package{PkgPath: "unsafe", Dir: filepath.Join(goRootSrc, "unsafe")}, classSkip},
		{"builtin", &packages.Package{PkgPath: "builtin", Dir: filepath.Join(goRootSrc, "builtin")}, classSkip},
		{"test variant", &packages.Package{PkgPath: "example.com/app_test", Dir: appDir, Module: appMod}, classSkip},
		{"empty dir", &packages.Package{PkgPath: "example.com/weird", Dir: ""}, classSkip},
		{"no module, not goroot", &packages.Package{PkgPath: "example.com/orphan", Dir: filepath.FromSlash("C:/elsewhere")}, classSkip},
	}

	for _, tc := range cases {
		if got := m.classify(tc.pkg); got != tc.want {
			t.Errorf("classify(%q) = %d, want %d", tc.name, got, tc.want)
		}
	}
}

// TestModuleConverterConvertSetOrder verifies the graph the module converter builds from a
// classified closure orders third-party leaves before the app that imports them, and that stdlib
// imports (outside the convert-set) do not appear or constrain the order. It exercises the same
// AddPackage/addImportEdges/topologicalSort path partition+buildEdges drive, without a real load.
func TestModuleConverterConvertSetOrder(t *testing.T) {
	m := NewModuleConverter(Options{})

	// Convert-set: an app main package and a co-located lib package; stdlib "fmt"/"strings" are
	// NOT added (they are referenced, not converted).
	m.graph.AddPackage("example.com/app", "/work/app")
	m.graph.AddPackage("example.com/lib", "/work/lib")

	m.graph.addImportEdges("example.com/app", []string{"example.com/lib", "fmt", "strings"})
	m.graph.addImportEdges("example.com/lib", []string{"strings"})
	m.graph.sortAdjacency()
	m.graph.topologicalSort()

	want := []string{"example.com/lib", "example.com/app"}

	if len(m.graph.sortedQueue) != len(want) || m.graph.sortedQueue[0] != want[0] || m.graph.sortedQueue[1] != want[1] {
		t.Fatalf("convert order = %v, want %v (lib before app; stdlib excluded)", m.graph.sortedQueue, want)
	}

	// The app depends only on the lib within the convert-set — stdlib edges are filtered out.
	appDeps := m.graph.packages["example.com/app"].Dependencies

	if len(appDeps) != 1 || appDeps[0] != "example.com/lib" {
		t.Errorf("app convert-set dependencies = %v, want [example.com/lib]", appDeps)
	}
}

// TestModuleConverterPartitionScope locks what each -recurse scope puts in the convert-set. The
// default converts the app AND its third-party dependencies; -recurse=module converts the app's own
// packages alone and records the third-party ones as referenced-only, so an unconvertible dependency
// closure cannot hold up the module's own code. The standard library is never in the convert-set
// either way, and mainModulePath is recorded from the main module in both.
func TestModuleConverterPartitionScope(t *testing.T) {
	goRoot := filepath.FromSlash("C:/go")
	appDir := filepath.FromSlash("C:/work/app")
	depDir := filepath.FromSlash("C:/gopath/pkg/mod/google.golang.org/api@v0.1.0")

	appMod := &packages.Module{Path: "example.com/app", Main: true, Dir: appDir}
	depMod := &packages.Module{Path: "google.golang.org/api", Main: false, Dir: depDir}

	closure := func() map[string]*packages.Package {
		return map[string]*packages.Package{
			"example.com/app":                 {PkgPath: "example.com/app", Dir: appDir, Module: appMod},
			"example.com/app/internal/util":   {PkgPath: "example.com/app/internal/util", Dir: filepath.Join(appDir, "internal", "util"), Module: appMod},
			"google.golang.org/api/option":    {PkgPath: "google.golang.org/api/option", Dir: filepath.Join(depDir, "option"), Module: depMod},
			"google.golang.org/api/transport": {PkgPath: "google.golang.org/api/transport", Dir: filepath.Join(depDir, "transport"), Module: depMod},
			"fmt":                             {PkgPath: "fmt", Dir: filepath.Join(goRoot, "src", "fmt")},
		}
	}

	cases := []struct {
		name           string
		moduleOnly     bool
		wantConvertSet []string
		wantReferenced []string
	}{
		{
			name:           "default converts app + third-party",
			moduleOnly:     false,
			wantConvertSet: []string{"example.com/app", "example.com/app/internal/util", "google.golang.org/api/option", "google.golang.org/api/transport"},
			wantReferenced: nil,
		},
		{
			name:           "module scope converts the app alone",
			moduleOnly:     true,
			wantConvertSet: []string{"example.com/app", "example.com/app/internal/util"},
			wantReferenced: []string{"google.golang.org/api/option", "google.golang.org/api/transport"},
		},
	}

	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			m := NewModuleConverter(Options{goRoot: goRoot, moduleOnly: tc.moduleOnly})
			m.partition(closure())

			var convertSet []string

			for pkgPath := range m.graph.packages {
				convertSet = append(convertSet, pkgPath)
			}

			sort.Strings(convertSet)
			sort.Strings(m.referencedThirdParty)

			if !reflect.DeepEqual(convertSet, tc.wantConvertSet) {
				t.Errorf("convert-set = %v, want %v", convertSet, tc.wantConvertSet)
			}

			if !reflect.DeepEqual(m.referencedThirdParty, tc.wantReferenced) {
				t.Errorf("referenced-only third-party = %v, want %v", m.referencedThirdParty, tc.wantReferenced)
			}

			if m.options.mainModulePath != "example.com/app" {
				t.Errorf("mainModulePath = %q, want %q", m.options.mainModulePath, "example.com/app")
			}
		})
	}
}

// TestModuleConverterOutputDir verifies the parallel-tree output routing (design 4): the app's own
// packages (the main module) go to <recurseOutputRoot>\src\<import-path>, while every dependency — a
// read-only module-cache dep or a co-located `replace` — goes to <recurseOutputRoot>\pkg\<import-path>. The
// path is built from the version-free import path (so a module-cache @version is stripped), and
// nothing is written in place, keeping the original Go source pure.
func TestModuleConverterOutputDir(t *testing.T) {
	goPath := filepath.FromSlash("C:/gopath")
	go2csPath := filepath.FromSlash("C:/gopath/src/go2cs")
	outputRoot := filepath.FromSlash("C:/temp/converted")
	m := &ModuleConverter{options: Options{
		goPath:            goPath,
		go2csPath:         go2csPath,
		recurseOutputRoot: outputRoot,
		mainModulePath:    "example.com/app",
	}}

	cases := []struct {
		name    string
		pkgPath string
		want    string
	}{
		{"module-cache dep to pkg (version stripped)", "github.com/fatih/color", filepath.Join(outputRoot, "pkg", "github.com", "fatih", "color")},
		{"co-located replace dep to pkg", "example.com/lib", filepath.Join(outputRoot, "pkg", "example.com", "lib")},
		{"app main package to src", "example.com/app", filepath.Join(outputRoot, "src", "example.com", "app")},
		{"app sub-package to src", "example.com/app/internal/util", filepath.Join(outputRoot, "src", "example.com", "app", "internal", "util")},
	}

	for _, tc := range cases {
		if got := m.outputDirFor(tc.pkgPath); got != tc.want {
			t.Errorf("outputDirFor(%q) = %q, want %q", tc.name, got, tc.want)
		}
	}
}

func TestModuleConverterOutputDirDefaultsToGo2CSPath(t *testing.T) {
	go2csPath := filepath.FromSlash("C:/gopath/src/go2cs")
	m := &ModuleConverter{options: Options{go2csPath: go2csPath, mainModulePath: "example.com/app"}}

	if got, want := m.outputDirFor("example.com/app"), filepath.Join(go2csPath, "src", "example.com", "app"); got != want {
		t.Fatalf("outputDirFor default root = %q, want %q", got, want)
	}
}
