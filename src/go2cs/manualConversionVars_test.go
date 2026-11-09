// manualConversionVars_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The both-sides guard for manualConversionVars. A registration displaces a package-level var
// declaration, and it holds only when three things hold together:
//
//   - the var exists in Go's source for that package, declared alone in its spec (visitValueSpec
//     refuses to split a spec);
//   - a hand-owned file the package compiles declares a member of that name, the destination;
//   - and where Go takes its address in that package, the same file set also declares the `ᏑName`
//     box every `&Name` emits: the name member is a ref property with no address of its own, so an
//     undeclared box would leave each `&Name` naming nothing.

package main

import (
	"go/ast"
	"go/build"
	"go/parser"
	"go/token"
	"os"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

func TestManualConversionVarsAreDisplacedAndDeclared(t *testing.T) {
	if len(manualConversionVars) == 0 {
		t.Fatal("manualConversionVars is empty: the guard is vacuous")
	}

	coreDir := filepath.Join("..", "core")

	if _, err := os.Stat(coreDir); err != nil {
		t.Skipf("corpus not present at %s: %v", coreDir, err)
	}

	for pkg, names := range manualConversionVars {
		goPkg, err := build.Default.Import(pkg, "", build.FindOnly)

		if err != nil {
			t.Fatalf("%s: cannot locate Go's source: %v", pkg, err)
		}

		fset := token.NewFileSet()
		pkgs, err := parser.ParseDir(fset, goPkg.Dir, func(info os.FileInfo) bool {
			return !strings.HasSuffix(info.Name(), "_test.go")
		}, 0)

		if err != nil {
			t.Fatalf("%s: cannot parse Go's source: %v", pkg, err)
		}

		for name := range names {
			declared, alone, addressed := 0, true, 0

			for _, astPkg := range pkgs {
				for _, file := range astPkg.Files {
					for _, decl := range file.Decls {
						gen, ok := decl.(*ast.GenDecl)

						if !ok || gen.Tok != token.VAR {
							continue
						}

						for _, spec := range gen.Specs {
							valueSpec := spec.(*ast.ValueSpec)

							for _, ident := range valueSpec.Names {
								if ident.Name == name {
									declared++
									alone = alone && len(valueSpec.Names) == 1
								}
							}
						}
					}

					ast.Inspect(file, func(node ast.Node) bool {
						if unary, ok := node.(*ast.UnaryExpr); ok && unary.Op == token.AND {
							if ident, ok := unary.X.(*ast.Ident); ok && ident.Name == name {
								addressed++
							}
						}

						return true
					})
				}
			}

			if declared != 1 {
				t.Errorf("%s.%s: Go's source declares it %d times at package level, want exactly 1", pkg, name, declared)
			}

			if !alone {
				t.Errorf("%s.%s: shares its var spec with other names; visitValueSpec refuses to split a spec", pkg, name)
			}

			// A static member declaration on a line that is not a comment. The box twin is matched the
			// same way; `Ꮡ` is not a \b word boundary in RE2, so the twin anchors on the space before it.
			member := regexp.MustCompile(`(?m)^[ \t]*[^/\s][^\n]*\bstatic\b[^\n]*\b` + regexp.QuoteMeta(name) + `\s*(=>|\{|=|;)`)
			boxMember := regexp.MustCompile(`(?m)^[ \t]*[^/\s][^\n]*\bstatic\b[^\n]* Ꮡ` + regexp.QuoteMeta(name) + `\s*(=>|\{|=|;)`)
			packageDir := filepath.Join(coreDir, filepath.FromSlash(pkg))
			found, boxFound := false, false

			_ = filepath.WalkDir(packageDir, func(path string, entry os.DirEntry, err error) error {
				if err != nil {
					return nil
				}

				if entry.IsDir() && (entry.Name() == "bin" || entry.Name() == "obj") {
					return filepath.SkipDir
				}

				if entry.IsDir() || !strings.HasSuffix(path, "_impl.cs") {
					return nil
				}

				content, readErr := os.ReadFile(path)

				if readErr == nil && strings.Contains(string(content), "GoManualConversion") {
					found = found || member.Match(content)
					boxFound = boxFound || boxMember.Match(content)
				}

				return nil
			})

			if !found {
				t.Errorf("%s.%s: no hand-owned *_impl.cs under %s declares it; the displaced declaration has no destination", pkg, name, packageDir)
			}

			if addressed != 0 && !boxFound {
				t.Errorf("%s.%s: Go takes its address %d times, and no hand-owned *_impl.cs under %s declares its Ꮡ%s box", pkg, name, addressed, packageDir, name)
			}
		}
	}
}

// ---- manualConversionConsts: a package-level const displaced per target ------------------------------

const manualConstFixture = `package mcc

const ARCH string = "amd64"

func IsArm() bool { return ARCH == "arm64" }
`

// convertManualConstFixture converts the fixture module for one target and returns its emission.
func convertManualConstFixture(t *testing.T, target string) string {
	t.Helper()
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/mcc\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "mcc.go"), manualConstFixture)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      target,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule (%s): %v", target, err)
	}

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "mcc", "mcc.cs"))
}

// TestAManualConstIsDisplacedOnlyOnItsTarget drives the real emission: a const registered for a scope emits
// the placeholder on each target in it, and the ordinary declaration on every other target. Both scopes the
// corpus uses are driven: darwin alone, and darwin with windows (runtime.GOARCH's). The reference in the
// function body is emitted either way, so the hand-owned member is what it reads.
func TestAManualConstIsDisplacedOnlyOnItsTarget(t *testing.T) {
	original := manualConversionConsts
	t.Cleanup(func() { manualConversionConsts = original })

	// The declaration's `=>` or `=`, never the first half of an `==` in a one-line body that reads it.
	member := regexp.MustCompile(`(?m)^[ \t]*[^/\s][^\n]*\bstatic\b[^\n]*\bARCH\s*(=>|=[^=])`)

	for _, scope := range []goosScope{goosDarwin, goosWindowsDarwin} {
		// Copy-on-write: the test installs its own registry and restores the original reference, so no
		// other test can see the fixture entry or lose a real one.
		patched := make(map[string]map[string]goosScope, len(original)+1)

		for pkg, names := range original {
			patched[pkg] = names
		}

		patched["example.com/mcc"] = map[string]goosScope{"ARCH": scope}
		manualConversionConsts = patched

		for _, target := range []string{"darwin/amd64", "linux/amd64", "windows/amd64"} {
			checkManualConstEmission(t, member, target, scope.includes(goosOfTarget(target)))
		}
	}
}

// checkManualConstEmission converts the fixture for one target and checks the const is displaced there exactly
// when the scope names the target.
func checkManualConstEmission(t *testing.T, member *regexp.Regexp, target string, displaced bool) {
	t.Helper()

	for _, c := range []struct {
		target    string
		displaced bool
	}{
		{target, displaced},
	} {
		emitted := convertManualConstFixture(t, c.target)
		placeholder := strings.Contains(emitted, "placeholder — const ARCH is hand-converted")
		declared := member.MatchString(emitted)

		if c.displaced && (!placeholder || declared) {
			t.Errorf("%s: the registered const must be displaced (placeholder %v, member declared %v):\n%s", c.target, placeholder, declared, emitted)
		}

		if !c.displaced && (placeholder || !declared) {
			t.Errorf("%s: outside its scope the const must be emitted as converted (placeholder %v, member declared %v):\n%s", c.target, placeholder, declared, emitted)
		}

		if !strings.Contains(emitted, `ARCH == "arm64"u8`) {
			t.Errorf("%s: the body must still read the const by name:\n%s", c.target, emitted)
		}
	}
}

// TestManualConversionConstsAreDisplacedAndDeclared is the committed corpus's half: every registered
// const is declared exactly once, alone, at package level in Go's source, and a hand-owned *_impl.cs in
// each scoped flavour's folder declares the member that replaces it.
func TestManualConversionConstsAreDisplacedAndDeclared(t *testing.T) {
	if len(manualConversionConsts) == 0 {
		t.Fatal("manualConversionConsts is empty: the guard is vacuous")
	}

	coreDir := filepath.Join("..", "core")

	if _, err := os.Stat(coreDir); err != nil {
		t.Skipf("corpus not present at %s: %v", coreDir, err)
	}

	for pkg, names := range manualConversionConsts {
		goPkg, err := build.Default.Import(pkg, "", build.FindOnly)

		if err != nil {
			t.Fatalf("%s: cannot locate Go's source: %v", pkg, err)
		}

		fset := token.NewFileSet()
		pkgs, err := parser.ParseDir(fset, goPkg.Dir, func(info os.FileInfo) bool {
			return !strings.HasSuffix(info.Name(), "_test.go")
		}, 0)

		if err != nil {
			t.Fatalf("%s: cannot parse Go's source: %v", pkg, err)
		}

		for name, scope := range names {
			declared, alone := 0, true

			for _, astPkg := range pkgs {
				for _, file := range astPkg.Files {
					for _, decl := range file.Decls {
						gen, ok := decl.(*ast.GenDecl)

						if !ok || gen.Tok != token.CONST {
							continue
						}

						for _, spec := range gen.Specs {
							valueSpec := spec.(*ast.ValueSpec)

							for _, ident := range valueSpec.Names {
								if ident.Name == name {
									declared++
									alone = alone && len(valueSpec.Names) == 1
								}
							}
						}
					}
				}
			}

			if declared != 1 {
				t.Errorf("%s.%s: Go's source declares the const %d times at package level, want exactly 1", pkg, name, declared)
			}

			if !alone {
				t.Errorf("%s.%s: shares its const spec with other names; visitValueSpec refuses to split a spec", pkg, name)
			}

			if len(scope) == 0 {
				t.Errorf("%s.%s: a displaced const needs a flavour scope, so the guard knows which folder supplies it", pkg, name)
				continue
			}

			member := regexp.MustCompile(`(?m)^[ \t]*[^/\s][^\n]*\bstatic\b[^\n]*\b` + regexp.QuoteMeta(name) + `\s*(=>|\{|=|;)`)

			for _, goos := range scope {
				flavourDir := filepath.Join(coreDir, filepath.FromSlash(pkg), goos)
				found := false

				_ = filepath.WalkDir(flavourDir, func(path string, entry os.DirEntry, err error) error {
					if err != nil || entry.IsDir() || !strings.HasSuffix(path, "_impl.cs") {
						return nil
					}

					content, readErr := os.ReadFile(path)

					if readErr == nil && strings.Contains(string(content), "GoManualConversion") {
						found = found || member.Match(content)
					}

					return nil
				})

				if !found {
					t.Errorf("%s.%s (%s): no hand-owned *_impl.cs under %s declares it; the displaced const has no destination", pkg, name, goos, flavourDir)
				}
			}
		}
	}
}
