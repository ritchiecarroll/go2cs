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
	"go/build"
	"go/parser"
	"go/token"
	"os"
	"path/filepath"
	"runtime"
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

	// internal/cpu osInit: the darwin/arm64 target alone. windows/amd64 and linux/arm64 never, and darwin/amd64 is the
	// same-GOOS near-miss (its arm64 detection is the classic partial osInitArm64, not osInit).
	osInit := &ast.FuncDecl{Name: ast.NewIdent("osInit")}

	for _, target := range fiveTargets {
		if got, want := isManualFuncDeclInPackage("internal/cpu", target, osInit), target == "darwin/arm64"; got != want {
			t.Errorf("internal/cpu.osInit displaced at %s = %v, want %v", target, got, want)
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

// TestTargetScopedRegistrationConvertsEndToEnd converts a module whose package holds a function, a pointer-receiver
// method and a package var, each registered for darwin/arm64 alone, through the whole package conversion, at
// windows/amd64, darwin/amd64 and darwin/arm64, with hoistLiterals on to mirror the stdlib's mode. Each run must
// finish with no dropped file (GO2CS2008) and no panic, and each registered declaration is displaced at darwin/arm64
// and emitted at the other two. The fixture reaches the registry lookups in the array-dims facts, the emission, the
// hoisted-literal pass (hoistedLiteralOperations.go, which runs in both modes and reads the registry before its
// hoist gate) and both hand-owned-member passes (fieldAccess.go, methods and vars). manualTypeReceiverRefusal.go's
// lookup runs only in a package with a manual type, and is a follow-up on the BOARD. TestRegistryLookupsReceiveTheFullTarget reads converter source for a registry callee
// handed goosOfTarget(...) directly, so it cannot see a GOOS stored first and passed later: the array-dims facts
// did exactly that (arrayDimsInputs held the GOOS in a field), goosScope.includes panicked on the os/arch entry,
// and every target dropped internal/cpu's cpu.go until a five-target emission found it. This test reads every pass
// the driver runs, however the target is spelled on its way to the registry. The fixture's generic is instantiated
// with an array, so the array-dims facts run over the package. P3's SHOULD 1 on the darwin/arm64 seats (D2, D3).
func TestTargetScopedRegistrationConvertsEndToEnd(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	const pkgPath = "example.com/scoped/lib"

	previousFuncs, hadFuncs := manualConversionFuncs[pkgPath]
	previousVars, hadVars := manualConversionVars[pkgPath]
	previousGoRoot := build.Default.GOROOT
	manualConversionFuncs[pkgPath] = map[string]goosScope{"Scoped": {"darwin/arm64"}, "Box.Get": {"darwin/arm64"}}
	manualConversionVars[pkgPath] = map[string]goosScope{"Shared": {"darwin/arm64"}}

	takeDroppedFiles()

	t.Cleanup(func() {
		takeDroppedFiles()
		build.Default.GOROOT = previousGoRoot

		if hadFuncs {
			manualConversionFuncs[pkgPath] = previousFuncs
		} else {
			delete(manualConversionFuncs, pkgPath)
		}

		if hadVars {
			manualConversionVars[pkgPath] = previousVars
		} else {
			delete(manualConversionVars, pkgPath)
		}
	})

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/scoped\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "lib", "lib.go"), `package lib

func zero[T any]() T { var x T; return x }

// Scoped, Box.Get and Shared are registered for darwin/arm64 alone.
func Scoped() int { return len(zero[[4]byte]()) }

type Box struct{ n int }

func (b *Box) Get() int { return b.n }

var Shared = []int{1, 2, 3}

func Kept() int { var b Box; return Scoped() + b.Get() + Shared[0] }
`)

	for _, target := range []string{"windows/amd64", "darwin/amd64", "darwin/arm64"} {
		// One output root per target: two conversions never share a root.
		out := filepath.Join(root, "out-"+strings.ReplaceAll(target, "/", "-"))
		options := Options{
			goRoot:              build.Default.GOROOT,
			goPath:              build.Default.GOPATH,
			go2csPath:           out,
			recurse:             true,
			targetPlatform:      target,
			indentSpaces:        4,
			preferVarDecl:       true,
			useChannelOperators: true,
			hoistLiterals:       true,
		}

		if options.goRoot == "" {
			options.goRoot = runtime.GOROOT()
		}

		build.Default.GOROOT = options.goRoot

		func() {
			defer func() {
				if r := recover(); r != nil {
					t.Errorf("%s: the conversion panicked over a target-scoped registration: %v", target, r)
				}
			}()

			if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
				t.Errorf("%s: ConvertModule: %v", target, err)
			}
		}()

		if err := droppedFileError(); err != nil {
			t.Errorf("%s: the conversion dropped a file over a target-scoped registration:\n%v", target, err)
			continue
		}

		data, err := os.ReadFile(filepath.Join(out, "src", "example.com", "scoped", "lib", "lib.cs"))

		if err != nil {
			t.Errorf("%s: lib.cs was not written: %v", target, err)
			continue
		}

		emission := string(data)

		if !strings.Contains(emission, "class ᴀ4 ") {
			t.Errorf("%s: no ᴀ4 dims type, so the array-dims facts did not run over the fixture:\n%s", target, emission)
		}

		for _, member := range []struct{ name, placeholder, body string }{
			{"Scoped", "func Scoped is hand-converted", "public static nint Scoped()"},
			{"Box.Get", "func Get is hand-converted", "static nint Get⁔("},
			{"Shared", "var Shared is hand-converted", "ᏑShared = new"},
		} {
			placeholder := strings.Contains(emission, member.placeholder)
			body := strings.Contains(emission, member.body)

			if displaced := target == "darwin/arm64"; placeholder != displaced || body == displaced {
				t.Errorf("%s: %s displaced=%v (placeholder %v, body %v), want displaced=%v:\n%s",
					target, member.name, placeholder && !body, placeholder, body, displaced, emission)
			}
		}
	}
}
