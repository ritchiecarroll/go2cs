// inlineArrayValueDecodeCensus_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"os"
	"sort"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// B2's footprint census (COORD ruling 2026-11-15 16:53Z, Q2: the census gates the cut). D2 emits `p.GoValue()` for an
// RVALUE dereference `*p` whose pointee is a struct the inline-array admission KEPT OUT (a selected seed that a cap
// refused: inlineArrayAdmission.blocked). This counts those sites before any emission change.
//
// The predicate is the converter's own verdict (computeInlineArrayAdmission), not a re-derivation. Kept-out keys are
// unioned across every loaded package first, so a dereference in one package of a type another package kept out is
// counted too. An rvalue is every `*p` that is not an assignment target, not the operand of `&` or `++`/`--`, and not
// the base of a field selection (a field read goes through FieldRef at its Go offset and is not a whole-value read),
// and not inside a constant expression such as `unsafe.Sizeof(*p)`, whose operand is never evaluated.
//
// GO2CS_B2_CENSUS=<dir>:<patterns> runs it; GO2CS_B2_CENSUS_ENV adds environment (GOOS=...;GOARCH=...), and
// GO2CS_B2_CENSUS_TARGET names the target platform the admission is computed for (default linux/amd64).
func TestInlineArrayValueDecodeCensus(t *testing.T) {
	spec := os.Getenv("GO2CS_B2_CENSUS")

	if spec == "" {
		t.Skip("GO2CS_B2_CENSUS=<dir>:<patterns> runs the B2 value-decode census over real packages")
	}

	dir, patterns, _ := strings.Cut(spec, ":")
	cfg := &packages.Config{Mode: packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo, Dir: dir}

	if extra := os.Getenv("GO2CS_B2_CENSUS_ENV"); extra != "" {
		cfg.Env = append(os.Environ(), strings.Split(extra, ";")...)
	}

	pkgs, err := packages.Load(cfg, strings.Split(patterns, ",")...)

	if err != nil {
		t.Fatal(err)
	}

	target := os.Getenv("GO2CS_B2_CENSUS_TARGET")

	if target == "" {
		target = "linux/amd64"
	}

	options := Options{targetPlatform: target, inlineArrays: inlineArrayOverride{}}
	sort.Slice(pkgs, func(i, j int) bool { return pkgs[i].PkgPath < pkgs[j].PkgPath })

	blocked := map[string]string{} // kept-out key -> the package that kept it out

	for _, pkg := range pkgs {
		if pkg.TypesInfo == nil {
			continue
		}

		if a := computeInlineArrayAdmission(pkg.Syntax, pkg.Fset, pkg.Types, pkg.TypesInfo, pkg.Dir, options); a != nil {
			for key := range a.blocked {
				blocked[key] = pkg.PkgPath
			}
		}
	}

	keys := make([]string, 0, len(blocked))

	for key := range blocked {
		keys = append(keys, key)
	}

	sort.Strings(keys)

	for _, key := range keys {
		fmt.Printf("B2CENSUS KEPT-OUT %s (in %s)\n", key, blocked[key])
	}

	sites := 0

	for _, pkg := range pkgs {
		if pkg.TypesInfo == nil {
			continue
		}

		for _, file := range pkg.Syntax {
			var stack []ast.Node

			ast.Inspect(file, func(n ast.Node) bool {
				if n == nil {
					stack = stack[:len(stack)-1]
					return true
				}

				stack = append(stack, n)
				star, ok := n.(*ast.StarExpr)

				if !ok {
					return true
				}

				tv, ok := pkg.TypesInfo.Types[star.X]

				if !ok || tv.IsType() {
					return true
				}

				ptr, ok := tv.Type.Underlying().(*types.Pointer)

				if !ok {
					return true
				}

				key, ok := inlineTypeKey(ptr.Elem())

				if !ok || blocked[key] == "" || !valueDecodeRvalue(stack) || insideConstant(stack, pkg.TypesInfo) {
					return true
				}

				sites++
				fmt.Printf("B2CENSUS SITE %s %s *%s\n", pkg.PkgPath, relativePosition(pkg.Fset.Position(star.Pos())), key)
				return true
			})
		}
	}

	fmt.Printf("B2CENSUS TOTAL target=%s packages=%d kept-out=%d rvalue-sites=%d\n", target, len(pkgs), len(blocked), sites)
}

// insideConstant reports whether an enclosing expression of the StarExpr on top of stack is a compile-time constant
// (`unsafe.Sizeof(*p)` and its kin): its operand is never evaluated, so the dereference reads nothing.
func insideConstant(stack []ast.Node, info *types.Info) bool {
	for i := len(stack) - 2; i >= 0; i-- {
		expr, ok := stack[i].(ast.Expr)

		if !ok {
			return false
		}

		if tv, ok := info.Types[expr]; ok && tv.Value != nil {
			return true
		}
	}

	return false
}

// valueDecodeRvalue reports whether the StarExpr on top of stack reads its pointee's WHOLE value.
func valueDecodeRvalue(stack []ast.Node) bool {
	star := stack[len(stack)-1]
	i := len(stack) - 2

	for i >= 0 {
		if _, paren := stack[i].(*ast.ParenExpr); !paren {
			break
		}

		star = stack[i]
		i--
	}

	if i < 0 {
		return true
	}

	switch parent := stack[i].(type) {
	case *ast.AssignStmt:
		for _, lhs := range parent.Lhs {
			if lhs == star {
				return false
			}
		}
	case *ast.UnaryExpr:
		return parent.Op != token.AND
	case *ast.IncDecStmt:
		return false
	case *ast.SelectorExpr:
		return parent.X != star
	}

	return true
}

// relativePosition trims a position to its path below the last "/src/" or module root, for a portable census line.
func relativePosition(pos token.Position) string {
	file := pos.Filename

	if i := strings.LastIndex(file, "/src/"); i >= 0 {
		file = file[i+5:]
	} else if i := strings.Index(file, "@"); i >= 0 {
		if j := strings.LastIndex(file[:i], "/"); j >= 0 {
			file = file[j+1:]
		}
	}

	return fmt.Sprintf("%s:%d", file, pos.Line)
}
