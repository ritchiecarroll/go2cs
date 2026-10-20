// manualTypeReceiverRefusal.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file refuses, before any file is written, a conversion to a MANUAL type (runtime's guintptr
// family, manualTypeOperations.go) whose unsafe.Pointer operand is the method's own pointer RECEIVER.
//
// The manual type stores the referent itself, so its constructor must receive the box Go's pointer
// names. The unwrap in convCallExpr passes a pointer LOCAL as it is (it holds the box) and a pointer
// PARAMETER as its box `Ꮡp`. A pointer receiver renders as the deref'd value alias and a `[GoRecv]
// this ref T` receiver has no box at all, so the constructor would be handed the value, which compiles
// only through a self-to-box operator that boxes a COPY (go2cs-gen now refuses that record,
// GO2CS0008). The receiver form is hand-converted instead: runtime's one instance, g.guintptr, is
// displaced to `this ж<g>` (runtime2_impl.cs), and a displaced method is exempt here.

package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"

	"golang.org/x/tools/go/packages"
)

// manualTypeReceiverRefusal is the error refuseManualTypeReceiverOperands returns: one canonical
// diagnostic per conversion it refuses.
type manualTypeReceiverRefusal struct {
	pkgPath string
	sites   []manualTypeReceiverRefusalSite
}

type manualTypeReceiverRefusalSite struct {
	position token.Position
	text     string
}

func (refusal *manualTypeReceiverRefusal) Error() string {
	return fmt.Sprintf("package %s converts a pointer receiver to a manual type, which needs the receiver's box (%d site(s)); see the %s diagnostics",
		refusal.pkgPath, len(refusal.sites), diagnosticManualTypeReceiverRefused)
}

// report prints each refused site as a canonical diagnostic.
func (refusal *manualTypeReceiverRefusal) report() {
	for _, site := range refusal.sites {
		reportDiagnostic(site.position.String(), diagnosticManualTypeReceiverRefused, site.text)
	}
}

// refuseManualTypeReceiverOperands finds every `T(unsafe.Pointer(r))` in the package where T is one of
// its manual types and r is the enclosing method's pointer receiver, by object identity, in a method
// that is not itself displaced for goos. It returns nil when there is none.
func refuseManualTypeReceiverOperands(pkg *packages.Package, goos string) *manualTypeReceiverRefusal {
	if pkg.Types == nil || pkg.TypesInfo == nil {
		return nil
	}

	// Keyed as the emission's isManualType keys it.
	typeNames, ok := manualConversionTypes[pkg.Types.Path()]

	if !ok {
		return nil
	}

	info := pkg.TypesInfo
	refusal := &manualTypeReceiverRefusal{pkgPath: pkg.PkgPath}

	isConversion := func(call *ast.CallExpr) bool {
		tv, ok := info.Types[call.Fun]
		return ok && tv.IsType() && len(call.Args) == 1
	}

	for _, file := range pkg.Syntax {
		for _, decl := range file.Decls {
			funcDecl, ok := decl.(*ast.FuncDecl)

			if !ok || funcDecl.Body == nil || funcDecl.Recv == nil || len(funcDecl.Recv.List) == 0 {
				continue
			}

			field := funcDecl.Recv.List[0]

			if _, isPointer := field.Type.(*ast.StarExpr); !isPointer || len(field.Names) == 0 {
				continue
			}

			receiver := info.Defs[field.Names[0]]

			if receiver == nil || isManualFuncDeclInPackage(pkg.PkgPath, goos, funcDecl) {
				continue
			}

			ast.Inspect(funcDecl.Body, func(node ast.Node) bool {
				call, ok := node.(*ast.CallExpr)

				if !ok || !isConversion(call) {
					return true
				}

				named, ok := info.TypeOf(call).(*types.Named)

				if !ok || named.Obj().Pkg() != pkg.Types || !typeNames[named.Obj().Name()] {
					return true
				}

				inner, ok := ast.Unparen(call.Args[0]).(*ast.CallExpr)

				if !ok || !isConversion(inner) {
					return true
				}

				if basic, ok := info.TypeOf(inner).(*types.Basic); !ok || basic.Kind() != types.UnsafePointer {
					return true
				}

				if ident, ok := ast.Unparen(inner.Args[0]).(*ast.Ident); ok && info.Uses[ident] == receiver {
					refusal.sites = append(refusal.sites, manualTypeReceiverRefusalSite{
						position: pkg.Fset.Position(call.Pos()),
						text: fmt.Sprintf("%s(unsafe.Pointer(%s)) in method %s converts the pointer receiver %s to the manual type %s, which stores the referent: "+
							"a receiver renders as its value with no box to pass, so the conversion would hold a copy; hand-convert the method with a `this ж<%s>` receiver",
							named.Obj().Name(), ident.Name, funcDecl.Name.Name, ident.Name, named.Obj().Name(), types.TypeString(receiver.Type().(*types.Pointer).Elem(), types.RelativeTo(pkg.Types))),
					})
				}

				return true
			})
		}
	}

	if len(refusal.sites) == 0 {
		return nil
	}

	return refusal
}
