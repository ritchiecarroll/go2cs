// sameNameImportAlias_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// Two imported packages that share a NAME and each export a type alias of the same name — a/foo and
// b/foo, both `package foo` exporting `Alias` — were recorded under ONE imported-type-alias key,
// `foo.Alias`, so both references rendered the single `global using fooꓸAlias` bound to whichever
// package loaded last: a/foo's value typed as b/foo's type (CS0029). Each reference must name its
// own package's target, and the shared alias name must not be declared at all.
func TestSameNamedImportsKeepTheirOwnTypeAliases(t *testing.T) {
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/samename\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "a", "foo", "foo.go"), "package foo\n\ntype Inner struct{ N int }\n\ntype Alias = Inner\n")
	writeModuleFile(t, filepath.Join(appDir, "b", "foo", "foo.go"), "package foo\n\ntype Other struct{ S string }\n\ntype Alias = Other\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"

	afoo "example.com/samename/a/foo"
	bfoo "example.com/samename/b/foo"
)

func main() {
	var x afoo.Alias = afoo.Inner{N: 1}
	var y bfoo.Alias = bfoo.Other{S: "s"}
	fmt.Println(x.N, y.S)
}
`)

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

	outDir := filepath.Join(options.go2csPath, "src", "example.com", "samename")
	mainCs := readGenerated(t, filepath.Join(outDir, "main.cs"))
	packageInfo := readGenerated(t, filepath.Join(outDir, "package_info.cs"))

	for _, want := range []string{
		"go.example.com.samename.a.foo_package.Inner x", // a/foo's Alias
		"go.example.com.samename.b.foo_package.Other y", // b/foo's Alias
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("missing %q in:\n%s", want, mainCs)
		}
	}

	if strings.Contains(packageInfo, "fooꓸAlias") {
		t.Errorf("the shared alias name is declared, though it has two meanings, in:\n%s", packageInfo)
	}
}
