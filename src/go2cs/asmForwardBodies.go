// asmForwardBodies.go - Gbtc
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
	"strings"

	"golang.org/x/tools/go/packages"
)

// ASSEMBLY WITH A MANAGED BODY. A third-party module's function implemented in assembly converts to a
// bodyless `partial` declaration whose generated stub throws. Most such functions forward elsewhere
// already (a pure-JMP trampoline, asmTrampolines.go; a linkname pull, funcLinknameForward). The ones
// this table lists hold real machine code, which golib answers with a managed body: x/sys/cpu's
// cpuid and xgetbv, whose init runs in every program importing x/sys/cpu (modernc.org/libc does on
// windows and darwin), so the throwing stub killed that init (sqlite-cs S21,
// docs/phase4/DESIGN-sqlite-cs-funcptr.md sections 2.3 and 3.7).
//
// A row names the package, the function and the Go signature its golib body implements. A listed
// function whose declared signature differs refuses its package (GO2CS2011, refuseAsmForwardSignatures)
// rather than forward a call golib's body does not answer. Rows are added only for a body that runs on
// a host go2cs targets: x/sys/cpu's four arm64 ID-register reads, which it reads only on freebsd, keep
// their stubs, and the conversion names each stub it leaves (GO2CS2012).

// asmForwardBody is one row: the golib type and member that implement the function, and its signature.
type asmForwardBody struct {
	signature string // the Go signature, types only: "func(uint32, uint32) (uint32, uint32, uint32, uint32)"
	alias     string // the C# type holding the body
	member    string // its member
}

// asmForwardBodies is keyed by "<package path>.<function>". Only golang.org/x/sys/cpu as a module: the
// standard library's vendored copy is converted std and reached by no banked row.
var asmForwardBodies = map[string]asmForwardBody{
	"golang.org/x/sys/cpu.cpuid": {
		signature: "func(uint32, uint32) (uint32, uint32, uint32, uint32)",
		alias:     "global::go.golib.GoCpu",
		member:    "Cpuid",
	},
	"golang.org/x/sys/cpu.xgetbv": {
		signature: "func() (uint32, uint32)",
		alias:     "global::go.golib.GoCpu",
		member:    "Xgetbv",
	},
}

// asmForwardSignatureOf spells a signature by its types alone, as asmForwardBodies' rows do.
func asmForwardSignatureOf(signature *types.Signature) string {
	spell := func(tuple *types.Tuple) string {
		parts := make([]string, tuple.Len())

		for i := 0; i < tuple.Len(); i++ {
			parts[i] = types.TypeString(tuple.At(i).Type(), nil)
		}

		return strings.Join(parts, ", ")
	}

	text := "func(" + spell(signature.Params()) + ")"

	switch signature.Results().Len() {
	case 0:
	case 1:
		text += " " + spell(signature.Results())
	default:
		text += " (" + spell(signature.Results()) + ")"
	}

	return text
}

// asmForwardRowOf answers the table's row for a bodyless top-level function of pkgPath, and whether
// the function's declared signature is the row's.
func asmForwardRowOf(funcDecl *ast.FuncDecl, pkgPath string, info *types.Info) (row asmForwardBody, listed bool, matches bool) {
	if funcDecl.Body != nil || funcDecl.Recv != nil || funcDecl.Name == nil {
		return asmForwardBody{}, false, false
	}

	row, listed = asmForwardBodies[pkgPath+"."+funcDecl.Name.Name]

	if !listed {
		return asmForwardBody{}, false, false
	}

	funcObj, ok := info.Defs[funcDecl.Name].(*types.Func)

	if !ok {
		return row, true, false
	}

	return row, true, asmForwardSignatureOf(funcObj.Type().(*types.Signature)) == row.signature
}

// funcAsmBodyForward recognizes a bodyless function asmForwardBodies lists with its row's signature, and
// answers the golib type and member the forwarder calls.
func (v *Visitor) funcAsmBodyForward(funcDecl *ast.FuncDecl) (alias string, targetFunc string, ok bool) {
	row, listed, matches := asmForwardRowOf(funcDecl, currentPackagePath, v.info)

	if !listed || !matches {
		return "", "", false
	}

	return row.alias, row.member, true
}

// asmForwardSignatureRefusal is the error refuseAsmForwardSignatures returns: one canonical diagnostic per
// listed function whose declared signature is not its row's.
type asmForwardSignatureRefusal struct {
	pkgPath string
	sites   []asmForwardSignatureSite
}

type asmForwardSignatureSite struct {
	position token.Position
	text     string
}

func (refusal *asmForwardSignatureRefusal) Error() string {
	return fmt.Sprintf("package %s declares %d assembly function(s) with another signature than the managed body go2cs forwards it to; see the %s diagnostics",
		refusal.pkgPath, len(refusal.sites), diagnosticAsmForwardSignatureRefused)
}

// report prints each refused function as a canonical diagnostic.
func (refusal *asmForwardSignatureRefusal) report() {
	for _, site := range refusal.sites {
		reportDiagnostic(site.position.String(), diagnosticAsmForwardSignatureRefused, site.text)
	}
}

// refuseAsmForwardSignatures finds every bodyless function asmForwardBodies lists whose declared signature
// differs from its row's. It returns nil when there is none.
func refuseAsmForwardSignatures(pkg *packages.Package) *asmForwardSignatureRefusal {
	if pkg.TypesInfo == nil {
		return nil
	}

	var refusal *asmForwardSignatureRefusal

	for _, file := range pkg.Syntax {
		for _, decl := range file.Decls {
			funcDecl, ok := decl.(*ast.FuncDecl)

			if !ok {
				continue
			}

			row, listed, matches := asmForwardRowOf(funcDecl, pkg.PkgPath, pkg.TypesInfo)

			if !listed || matches {
				continue
			}

			if refusal == nil {
				refusal = &asmForwardSignatureRefusal{pkgPath: pkg.PkgPath}
			}

			declared := "an unresolved signature"

			if funcObj, ok := pkg.TypesInfo.Defs[funcDecl.Name].(*types.Func); ok {
				declared = asmForwardSignatureOf(funcObj.Type().(*types.Signature))
			}

			refusal.sites = append(refusal.sites, asmForwardSignatureSite{
				position: pkg.Fset.Position(funcDecl.Pos()),
				text: fmt.Sprintf("%s.%s is assembly that go2cs forwards to %s.%s, which implements %s; this declaration is %s",
					pkg.PkgPath, funcDecl.Name.Name, strings.TrimPrefix(row.alias, "global::"), row.member, row.signature, declared),
			})
		}
	}

	return refusal
}

// reportAsmStub names a bodyless function of a module outside GOROOT that nothing forwards: it converts to
// a stub that throws when called (GO2CS2012). A program that never calls it is unaffected.
func (v *Visitor) reportAsmStub(funcDecl *ast.FuncDecl) {
	if !currentPackageOutsideGoroot || funcDecl.Name == nil {
		return
	}

	name := funcDecl.Name.Name

	if funcDecl.Recv != nil && len(funcDecl.Recv.List) == 1 {
		name = types.ExprString(funcDecl.Recv.List[0].Type) + "." + name
	}

	reportWarningDiagnostic(v.fset.Position(funcDecl.Pos()).String(), diagnosticAsmStubUnforwarded, fmt.Sprintf(
		"%s.%s has no Go body (assembly, or a linkname go2cs does not forward): it converts to a stub that throws NotImplementedException when called",
		currentPackagePath, name))
}
