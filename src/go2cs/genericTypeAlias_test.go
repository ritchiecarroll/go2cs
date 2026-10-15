// genericTypeAlias_test.go - Gbtc
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

// The declaring package: generic aliases of a generic type, a slice and an anonymous struct, beside
// PLAIN aliases (one of them over an instantiated generic) that must keep their names.
const genericTypeAliasFixture = `package ga

type Box[T any] struct{ V T }

type Alias[T any] = Box[T]
type List[T any] = []T
type Cell[T any] = struct{ Row, Col T }

type Plain = Box[int]
type Word = string

func Use(a Alias[int], l List[string], p Plain, w Word) Alias[int] { return Alias[int]{V: len(l)} }
`

// A CONSUMER package: it names the generic aliases and a plain alias across the package boundary.
const genericTypeAliasConsumerFixture = `package use

import "example.com/ga"

var A ga.Alias[string]

var P ga.Plain

func F(l ga.List[int]) ga.Alias[int] { return ga.Alias[int]{V: l[0]} }
`

func convertGenericTypeAliasFixtures(t *testing.T) (emitted, packageInfo, consumer, consumerInfo string) {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/ga\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "ga.go"), genericTypeAliasFixture)
	writeModuleFile(t, filepath.Join(appDir, "use", "use.go"), genericTypeAliasConsumerFixture)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      "linux/amd64",
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	pkgDir := filepath.Join(options.go2csPath, "src", "example.com", "ga")
	read := func(path string) string { return strings.ReplaceAll(readGenerated(t, path), "\r\n", "\n") }

	return read(filepath.Join(pkgDir, "ga.cs")), read(filepath.Join(pkgDir, "package_info.cs")),
		read(filepath.Join(pkgDir, "use", "use.cs")), read(filepath.Join(pkgDir, "use", "package_info.cs"))
}

// TestGenericTypeAliasRendersItsTarget: a Go 1.24 generic alias has no C# form, so its declaration
// keeps only a comment and every use renders the alias's target (docs/phase4/DESIGN-generic-type-aliases.md,
// option (a)). RED at master db1bd885a2, which emitted `global using Alias = go.ga_package.Box<go.T>;`,
// published the alias as a GoTypeAlias record, and named it with type arguments at every use.
func TestGenericTypeAliasRendersItsTarget(t *testing.T) {
	isolateConverterPathGlobals(t)
	emitted, packageInfo, consumer, consumerInfo := convertGenericTypeAliasFixtures(t)

	// the declaration keeps the Go text as a comment and nothing else
	for _, want := range []string{"// type Alias[T any] = Box[T]", "// type List[T any] = []T", "// type Cell[T any] = struct{ Row, Col T }"} {
		if !strings.Contains(emitted, want) {
			t.Errorf("want the declaration comment %q in:\n%s", want, emitted)
		}
	}

	for _, refuse := range []string{"global using Alias", "global using List", "global using Cell", "Cellᴛ", "go.T"} {
		if strings.Contains(emitted, refuse) || strings.Contains(consumer, refuse) || strings.Contains(consumerInfo, refuse) {
			t.Errorf("a generic alias must emit no using alias and no lifted type (%q)", refuse)
		}
	}

	for _, refuse := range []string{`GoTypeAlias("Alias"`, `GoTypeAlias("List"`, `GoTypeAlias("Cell"`} {
		if strings.Contains(packageInfo, refuse) {
			t.Errorf("a generic alias must publish no record (%q):\n%s", refuse, packageInfo)
		}
	}

	// every use renders the target, in the declaring package and across the boundary
	for _, want := range []string{"Box<nint> Use(Box<nint> a, slice<@string> l,", "new Box<nint>(V: len(l))"} {
		if !strings.Contains(emitted, want) {
			t.Errorf("want the target rendering %q in:\n%s", want, emitted)
		}
	}

	for _, want := range []string{"ga.Box<@string> A", "ga.Box<nint> F(slice<nint> l)", "new ga.Box<nint>(V: l[0])"} {
		if !strings.Contains(consumer, want) {
			t.Errorf("consumer: want the target rendering %q in:\n%s", want, consumer)
		}
	}
}

// TestPlainTypeAliasKeepsItsName guards the gate from the other side: a PLAIN alias, including one
// over an instantiated generic (`type Plain = Box[int]`), keeps its `global using`, its record and
// its name at every use, byte for byte. It fails if genericAliasTarget widens to every alias.
func TestPlainTypeAliasKeepsItsName(t *testing.T) {
	isolateConverterPathGlobals(t)
	emitted, packageInfo, consumer, consumerInfo := convertGenericTypeAliasFixtures(t)

	for _, want := range []string{"global using Plain = go.example.com.ga_package.Box<nint>;", "global using Word = go.@string;", "Word w) {"} {
		if !strings.Contains(emitted, want) {
			t.Errorf("want the plain alias %q in:\n%s", want, emitted)
		}
	}

	if !strings.Contains(emitted, "Plain p, Word w)") {
		t.Errorf("a plain alias must keep its name at a use:\n%s", emitted)
	}

	for _, want := range []string{`GoTypeAlias("Plain", "go.example.com.ga_package.Box<nint>")`, `GoTypeAlias("Word", "go.@string")`} {
		if !strings.Contains(packageInfo, want) {
			t.Errorf("want the plain alias record %q in:\n%s", want, packageInfo)
		}
	}

	if !strings.Contains(consumerInfo, "global using gaꓸPlain = go.example.com.ga_package.Box<nint>;") || !strings.Contains(consumer, "gaꓸPlain P") {
		t.Errorf("a consumer must import and name the plain alias:\n%s\n%s", consumerInfo, consumer)
	}
}
