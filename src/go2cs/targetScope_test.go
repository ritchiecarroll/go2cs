// targetScope_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/parser"
	"go/token"
	"os"
	"path/filepath"
	"slices"
	"sort"
	"strings"
	"testing"
)

// THE REGISTRY'S SCOPE IS MEASURED AGAINST THE FULL TARGET, AT ONE ENTRY POINT. A goosScope entry is a GOOS or an
// os/arch target (manualTypeOperations.go), and goosScope.includes is the only test of membership. The target form
// exists for darwin/arm64 (DESIGN-darwin-arm64-flavour.md section 4.2): runtime.GOARCH's stand-ins serve the
// darwin/amd64 flavour and must not displace darwin/arm64's converted constant. P3's revision 2 finding 2 asked
// for one entry point taking the full target and for these rows: every target of the five, on every target-scoped
// registration, with windows/amd64 (displaced by its GOOS entry) and linux/arm64 (never displaced) as near-misses.

// fiveTargets is the universe darwin/arm64 joins at D2. The rows are written against it rather than against
// platformUniverse so they read the same before and after the flip.
var fiveTargets = []string{"windows/amd64", "linux/amd64", "darwin/amd64", "linux/arm64", "darwin/arm64"}

func TestTargetScopeRowsAtEveryTarget(t *testing.T) {
	// runtime.GOARCH and internal/buildcfg.defaultGOARCH: windows by GOOS, darwin/amd64 by target.
	goarchDisplaced := map[string]bool{
		"windows/amd64": true,  // near-miss: displaced by the GOOS entry, no target entry needed
		"linux/amd64":   false, // linux keeps the converted constant
		"darwin/amd64":  true,  // the target entry
		"linux/arm64":   false, // near-miss: never displaced
		"darwin/arm64":  false, // same GOOS, other flavour: converted for arm64, so its constant is right
	}

	for _, c := range []struct{ pkg, name string }{{"runtime", "GOARCH"}, {"internal/buildcfg", "defaultGOARCH"}} {
		for _, target := range fiveTargets {
			if got, want := isManualConstInPackage(c.pkg, target, c.name), goarchDisplaced[target]; got != want {
				t.Errorf("%s.%s displaced at %s = %v, want %v", c.pkg, c.name, target, got, want)
			}
		}
	}

	// The scope itself, read directly, answers the same table.
	for _, target := range fiveTargets {
		if got, want := goosWindowsDarwinAmd64.includes(target), goarchDisplaced[target]; got != want {
			t.Errorf("goosWindowsDarwinAmd64.includes(%q) = %v, want %v", target, got, want)
		}
	}
}

// A bare GOOS cannot be measured against a scope holding a target entry: which flavour of the GOOS is meant is
// exactly what it does not say. Refused whatever the entries' order, and whichever GOOS is asked. A scope of GOOS
// entries alone still answers a bare GOOS, which is what a disclosure's scope is.
func TestTargetScopeRefusesABareGOOS(t *testing.T) {
	refuses := func(scope goosScope, argument string) (refused bool) {
		defer func() { refused = recover() != nil }()
		scope.includes(argument)
		return false
	}

	for _, scope := range []goosScope{{"windows", "darwin/amd64"}, {"darwin/amd64", "windows"}} {
		for _, goos := range []string{"windows", "darwin", "linux"} {
			if !refuses(scope, goos) {
				t.Errorf("%v.includes(%q) answered: a bare GOOS against a target entry must refuse", []string(scope), goos)
			}
		}
	}

	if refuses(goosWindowsDarwin, "darwin") || !goosWindowsDarwin.includes("darwin") || goosWindowsDarwin.includes("linux") {
		t.Error("a GOOS-only scope must still answer a bare GOOS by name")
	}

	if !goosWindowsDarwin.includes("darwin/arm64") || goosWindowsDarwin.includes("linux/arm64") {
		t.Error("a GOOS entry must cover every target of its GOOS, and only those")
	}

	if err := validateDisclosurePlatforms("probe", goosScope{"darwin/amd64"}); err == nil {
		t.Error("a disclosure scope naming an os/arch target was accepted: disclosure scopes are GOOS-only")
	}
}

// registryTargetCallees are the functions that measure a registry scope, directly or by calling one that does, and
// so must be handed the conversion's full target. An analysis pass handed the GOOS half where emission hands the
// target would read a target entry differently from emission (manualTypeOperations.go, isManualFuncDeclInPackage).
var registryTargetCallees = map[string]bool{
	"includes":                         true,
	"isManualConstInPackage":           true,
	"isManualVarInPackage":             true,
	"isManualFuncDeclInPackage":        true,
	"collectHandOwnedDecls":            true,
	"collectHoistedLiterals":           true,
	"refuseManualTypeReceiverOperands": true,
}

// goosHalfRegistryCalls names each call in the given Go sources that hands a registry callee the GOOS half of a
// target: an argument that is itself a call to goosOfTarget.
func goosHalfRegistryCalls(sources map[string]string) ([]string, error) {
	var findings []string

	for name, text := range sources {
		fileSet := token.NewFileSet()
		file, err := parser.ParseFile(fileSet, name, text, parser.SkipObjectResolution)

		if err != nil {
			return nil, err
		}

		ast.Inspect(file, func(node ast.Node) bool {
			call, ok := node.(*ast.CallExpr)

			if !ok {
				return true
			}

			callee := ""

			switch fun := call.Fun.(type) {
			case *ast.Ident:
				callee = fun.Name
			case *ast.SelectorExpr:
				callee = fun.Sel.Name
			}

			if !registryTargetCallees[callee] {
				return true
			}

			for _, argument := range call.Args {
				if inner, ok := argument.(*ast.CallExpr); ok {
					if ident, ok := inner.Fun.(*ast.Ident); ok && ident.Name == "goosOfTarget" {
						findings = append(findings, fileSet.Position(call.Pos()).String()+": "+callee)
					}
				}
			}

			return true
		})
	}

	sort.Strings(findings)

	return findings, nil
}

func TestRegistryLookupsReceiveTheFullTarget(t *testing.T) {
	names, err := filepath.Glob("*.go")

	if err != nil {
		t.Fatal(err)
	}

	sources := map[string]string{}

	for _, name := range names {
		if strings.HasSuffix(name, "_test.go") {
			continue
		}

		data, err := os.ReadFile(name)

		if err != nil {
			t.Fatal(err)
		}

		sources[name] = string(data)
	}

	if len(sources) < 50 {
		t.Fatalf("read %d converter sources: the walk is not where it thinks it is", len(sources))
	}

	findings, err := goosHalfRegistryCalls(sources)

	if err != nil {
		t.Fatal(err)
	}

	for _, finding := range findings {
		t.Errorf("%s is handed goosOfTarget(...): a registry scope is measured against the full os/arch target, the "+
			"value emission passes, never its GOOS half", finding)
	}

	// The guard on a planted call, both spellings: the free function and the scope's method.
	planted, err := goosHalfRegistryCalls(map[string]string{"planted.go": `package main
func f(options Options, scope goosScope) {
	collectHandOwnedDecls(nil, nil, nil, goosOfTarget(options.targetPlatform))
	_ = scope.includes(goosOfTarget(options.targetPlatform))
	_ = scope.includes(options.targetPlatform)
}
`})

	if err != nil {
		t.Fatal(err)
	}

	if want := []string{"planted.go:3:2: collectHandOwnedDecls", "planted.go:4:6: includes"}; !slices.Equal(planted, want) {
		t.Errorf("the planted calls read %v, want %v", planted, want)
	}
}
