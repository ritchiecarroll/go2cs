// promotedUnexportedEmbedCensus_test.go - Gbtc
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
	"go/types"
	"os"
	"sort"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// TestPromotedUnexportedEmbedCensus is an INSTRUMENT, not a gate: it skips unless GO2CS_PP_CENSUS_GOOS names a target.
// It counts the standard library's production call sites whose emission board pp's fix changes (convSelectorExpr's
// promoted pointer-receiver route through value embeds): a call `x.M(…)` of a pointer-receiver method M promoted
// through a chain of VALUE embeds, where some hop is an UNEXPORTED embed of a package other than the calling one.
//
//	deep    two or more hops: was the `.of(…)` descent through the unexported accessor, now `x.M(…)`
//	single  one hop with a non-pointer x: the route already called on the box, and now calls on the value itself
//	        when that box was a copy (`Ꮡ(x)`); a census cannot see the copy, so these are candidates
//
// Each site prints as "<goos>\t<kind>\t<package>\t<file:line>\t<receiver type>.<method>".
func TestPromotedUnexportedEmbedCensus(t *testing.T) {
	goos := os.Getenv("GO2CS_PP_CENSUS_GOOS")
	if goos == "" {
		t.Skip("instrument: set GO2CS_PP_CENSUS_GOOS to run")
	}

	tags := resolveBuildTags(true, false, false, false, false, nil)
	config := &packages.Config{
		Mode:       packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo,
		Env:        append(os.Environ(), "GOOS="+goos, "GOARCH=amd64", "CGO_ENABLED=0", "GOFLAGS=", "GOWORK=off"),
		BuildFlags: []string{"-tags=" + strings.Join(tags, ",")},
	}

	// GO2CS_PP_CENSUS_DIR loads the packages of a directory instead of std: the positive control is the behavioral
	// project PromotedUnexportedEmbedChain, whose main calls every shape.
	pattern := "std"

	if dir := os.Getenv("GO2CS_PP_CENSUS_DIR"); dir != "" {
		config.Dir, pattern = dir, "./..."
	}

	loaded, err := packages.Load(config, pattern)
	if err != nil {
		t.Fatal(err)
	}

	var rows []string

	for _, pkg := range loaded {
		for _, file := range pkg.Syntax {
			ast.Inspect(file, func(node ast.Node) bool {
				call, isCall := node.(*ast.CallExpr)
				if !isCall {
					return true
				}

				selector, isSel := ast.Unparen(call.Fun).(*ast.SelectorExpr)
				if !isSel {
					return true
				}

				if kind, what := promotedUnexportedEmbedSite(pkg, selector); kind != "" {
					position := pkg.Fset.Position(selector.Pos())
					rows = append(rows, fmt.Sprintf("%s\t%s\t%s\t%s:%d\t%s", goos, kind, pkg.PkgPath,
						position.Filename[strings.LastIndex(position.Filename, "/")+1:], position.Line, what))
				}

				return true
			})
		}
	}

	sort.Strings(rows)

	for _, row := range rows {
		t.Log(row)
	}

	t.Logf("READING %s: %d site(s)", goos, len(rows))
}

// promotedUnexportedEmbedSite classifies one call selector by the census predicate above, or returns "".
func promotedUnexportedEmbedSite(pkg *packages.Package, selector *ast.SelectorExpr) (string, string) {
	sel := pkg.TypesInfo.Selections[selector]
	if sel == nil || sel.Kind() != types.MethodVal || len(sel.Index()) < 2 {
		return "", ""
	}

	signature, isSig := sel.Obj().Type().(*types.Signature)
	if !isSig || signature.Recv() == nil {
		return "", ""
	}

	if _, isPtrRecv := signature.Recv().Type().(*types.Pointer); !isPtrRecv {
		return "", ""
	}

	recvType := pkg.TypesInfo.TypeOf(selector.X)
	xIsPtr := false

	if ptr, isPtr := recvType.Underlying().(*types.Pointer); isPtr {
		recvType, xIsPtr = ptr.Elem(), true
	}

	structType, isStruct := recvType.Underlying().(*types.Struct)
	if !isStruct {
		return "", ""
	}

	foreignUnexported := false
	hops := 0

	for _, idx := range sel.Index()[:len(sel.Index())-1] {
		if structType == nil {
			return "", ""
		}

		hop := structType.Field(idx)
		if !hop.Embedded() {
			return "", ""
		}

		if _, isPtr := hop.Type().Underlying().(*types.Pointer); isPtr {
			return "", ""
		}

		if !hop.Exported() && hop.Pkg() != nil && hop.Pkg() != pkg.Types {
			foreignUnexported = true
		}

		hops++
		structType, _ = hop.Type().Underlying().(*types.Struct)
	}

	if !foreignUnexported {
		return "", ""
	}

	what := types.TypeString(recvType, types.RelativeTo(pkg.Types)) + "." + sel.Obj().Name()

	if hops >= 2 {
		return "deep", what
	}

	if !xIsPtr {
		return "single", what
	}

	return "", ""
}
