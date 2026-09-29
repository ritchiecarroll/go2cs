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
