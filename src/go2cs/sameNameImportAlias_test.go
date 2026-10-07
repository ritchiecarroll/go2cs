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
	// Kind collides with S's method of the same name, so each package Δ-renames it and publishes
	// `Kind` as an exported alias of its own ΔKind: the same ambiguity through a NAMED type.
	writeModuleFile(t, filepath.Join(appDir, "a", "foo", "foo.go"), "package foo\n\ntype Inner struct{ N int }\n\ntype Alias = Inner\n\ntype Kind int\n\ntype S struct{}\n\nfunc (S) Kind() Kind { return 1 }\n")
	writeModuleFile(t, filepath.Join(appDir, "b", "foo", "foo.go"), "package foo\n\ntype Other struct{ S string }\n\ntype Alias = Other\n\ntype Kind string\n\ntype S struct{}\n\nfunc (S) Kind() Kind { return \"b\" }\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"

	afoo "example.com/samename/a/foo"
	bfoo "example.com/samename/b/foo"
)

type holder struct {
	pick func(int) bfoo.Kind
}

type D afoo.Alias

func main() {
	var x afoo.Alias = afoo.Inner{N: 1}
	var y bfoo.Alias = bfoo.Other{S: "s"}
	z := afoo.Alias{N: 2}
	var ka afoo.Kind = afoo.S{}.Kind()
	var conv func(afoo.Kind) bfoo.Kind = func(k afoo.Kind) bfoo.Kind { return bfoo.Kind(fmt.Sprint(k)) }
	h := holder{pick: func(int) bfoo.Kind { return bfoo.S{}.Kind() }}
	kb := bfoo.Kind("x")
	d := D{N: 5}
	fmt.Println(x.N, y.S, z.N, ka, conv(afoo.Kind(3)), h.pick(0), kb, d.N)
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
		"afoo.Inner x",         // a/foo's Alias, rendered through its target
		"bfoo.Other y",         // b/foo's Alias
		"new afoo.Inner(N: 2)", // a composite literal written through the alias
		"go.example.com.samename.a.foo_package.ΔKind ka", // a/foo's own renamed Kind
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("missing %q in:\n%s", want, mainCs)
		}
	}

	// The FIFTH arm: a reference that takes the qualified fallback spells the TARGET's member, collision
	// rename included, in every position -- a func type's parameter and result (a local and a struct
	// field) and a conversion. Kind is renamed ΔKind in both packages, so the Go name `Kind` must not
	// survive as a type reference anywhere (logrus' hooks/slog test: `Func<…, slog.Level>`, CS0426).
	if strings.Contains(mainCs, "foo.Kind") {
		t.Errorf("a same-named import's renamed Kind is still spelled by its Go name in:\n%s", mainCs)
	}

	for _, want := range []string{"Func<nint, bfoo.ΔKind> pick", "((afoo.ΔKind)3)"} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("missing the renamed member %q in:\n%s", want, mainCs)
		}
	}

	// A defined type over an ambiguous alias renders its underlying through the FULLY qualified
	// renderer (getFullyQualifiedTypeName's twin of the alias-qualified arm): its [GoType] must name
	// a/foo's target, not the shared key `foo.Alias` that no `global using` declares.
	if want := `[GoType("global::go.example.com.samename.a.foo_package.Inner")] partial struct D;`; !strings.Contains(mainCs, want) {
		t.Errorf("missing the fully qualified alias target %q in:\n%s", want, mainCs)
	}

	// Both imports are renamed, so each package's types render through its own alias; supplying the
	// canonical `using foo = …` for each would declare one alias name twice (CS1537).
	if count := strings.Count(mainCs, "using foo = "); count > 1 {
		t.Errorf("%d canonical `using foo` directives, one alias name for two packages, in:\n%s", count, mainCs)
	}

	if strings.Contains(packageInfo, "fooꓸAlias") || strings.Contains(packageInfo, "fooꓸKind") {
		t.Errorf("the shared alias name is declared, though it has two meanings, in:\n%s", packageInfo)
	}
}
