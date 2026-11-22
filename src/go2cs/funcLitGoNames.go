// funcLitGoNames.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/types"
	"strconv"
)

// Every function literal is emitted with the name Go gives it, without its package, in a [GoFunc] attribute on the
// lambda or local function it becomes: `[GoFunc("(*ring).init.func1")] () => …`. The runtime's frame naming reads that
// name from the frame's method (golib GoFuncAttribute, runtime managed_impl.cs goFrameName), so runtime.FuncForPC and a
// traceback name a literal as Go does from metadata alone: no PDB, the same under Native AOT. The C# compiler's own name
// for the method (`<outer>b__X_Y`) cannot say which receiver type an outer method belongs to, and its numbering is a
// closure-group ordinal that matches Go's counter only by coincidence.
//
// Go names a literal for the function that declares it, measured on go1.26.11:
//
//	func outer()                  literal         -> outer.func1, nested in it -> outer.func1.1
//	func (r *ring) init()         literal         -> (*ring).init.func1
//	func (g gauge) Name()         literal         -> gauge.Name.func1
//	func (l *List[T]) Push()      literal         -> (*List[...]).Push.func1
//	func Map[T any]()             literal         -> Map[...].func1
//	the second `func init()`      literal         -> init.1.func1
//	var x = func() { … }          var initializer -> init.func1
//
// The counter is per declaring function: its direct literals in source order, each nested literal numbered within its
// parent. Literals in package-level var initializers share one package counter under the name `init`, numbered in
// INITIALIZATION order — types.Info.InitOrder, dependency order — not source order: a var declared first whose
// initializer reads a later one takes the later number (measured across two files). A `func init` is numbered among
// the package's `func init` declarations in compile order, which is pkg.Syntax order.
//
// The name is the DECLARED one, Go's name with inlining off: Go renames a literal by its inlining (a literal in a method
// inlined into main is main.main.(*T).M.func1), which no conversion can predict. A literal this pass names nothing for
// is emitted without the attribute and the runtime falls back to the name it derives from the frame.

// packageFuncLitGoNames maps each function literal of the package being converted to its Go name without the package.
// Filled by collectFuncLitGoNames, which every driver runs from collectMovedInitVars, and reset per package.
var packageFuncLitGoNames = map[*ast.FuncLit]string{}

// collectFuncLitGoNames names every function literal in syntax, the whole package as the compiler sees it (a test
// variant's syntax holds its production files too, which is the file set Go numbers a test binary's literals over).
func collectFuncLitGoNames(info *types.Info, syntax []*ast.File) {
	names := map[*ast.FuncLit]string{}
	initIndex := 0

	for _, file := range syntax {
		for _, decl := range file.Decls {
			funcDecl, ok := decl.(*ast.FuncDecl)

			if !ok || funcDecl.Body == nil {
				continue
			}

			outer := funcDeclGoName(funcDecl)

			if funcDecl.Recv == nil && funcDecl.Name.Name == "init" {
				outer = "init." + strconv.Itoa(initIndex)
				initIndex++
			}

			if outer == "" {
				continue
			}

			counter := 0
			numberFuncLits(funcDecl.Body, false, outer+".func", &counter, names)
		}
	}

	if info != nil {
		counter := 0

		for _, initializer := range info.InitOrder {
			if initializer.Rhs != nil {
				numberFuncLits(initializer.Rhs, true, "init.func", &counter, names)
			}
		}
	}

	packageLock.Lock()
	packageFuncLitGoNames = names
	packageLock.Unlock()
}

// numberFuncLits names the function literals directly under root `<prefix><n>`, counting n on from *counter in source
// order, and each literal nested directly in one of them `<its name>.<m>`, m counting from 1 within that literal. root
// itself is one of them when self is set: a var initializer can be a literal.
func numberFuncLits(root ast.Node, self bool, prefix string, counter *int, names map[*ast.FuncLit]string) {
	ast.Inspect(root, func(node ast.Node) bool {
		lit, ok := node.(*ast.FuncLit)

		if !ok || node == root && !self {
			return true
		}

		*counter++
		name := prefix + strconv.Itoa(*counter)
		names[lit] = name

		nested := 0
		numberFuncLits(lit, false, name+".", &nested, names)

		return false // the recursion above owns this literal's children
	})
}

// funcDeclGoName spells a declaration the way Go names it in a literal's name: `outer`, `Map[...]`, `(*ring).init`,
// `gauge.Name`, `(*List[...]).Push`. A blank function, or a receiver whose type is not a plain (possibly generic) type
// name, answers "".
func funcDeclGoName(funcDecl *ast.FuncDecl) string {
	name := funcDecl.Name.Name

	if name == "_" {
		return ""
	}

	if funcDecl.Type.TypeParams != nil && funcDecl.Type.TypeParams.NumFields() > 0 {
		name += "[...]"
	}

	if funcDecl.Recv == nil || len(funcDecl.Recv.List) == 0 {
		return name
	}

	recvType := ast.Unparen(funcDecl.Recv.List[0].Type)
	pointer := false

	if star, ok := recvType.(*ast.StarExpr); ok {
		pointer = true
		recvType = ast.Unparen(star.X)
	}

	generic := false

	switch index := recvType.(type) {
	case *ast.IndexExpr:
		generic = true
		recvType = index.X
	case *ast.IndexListExpr:
		generic = true
		recvType = index.X
	}

	ident, ok := recvType.(*ast.Ident)

	if !ok {
		return ""
	}

	receiver := ident.Name

	if generic {
		receiver += "[...]"
	}

	if pointer {
		receiver = "(*" + receiver + ")"
	}

	return receiver + "." + name
}

// funcLitGoName answers the Go name recorded for funcLit, or "" when the package pass named nothing for it.
func funcLitGoName(funcLit *ast.FuncLit) string {
	packageLock.Lock()
	defer packageLock.Unlock()

	return packageFuncLitGoNames[funcLit]
}

// litAttributePrefix is the attribute list every convFuncLit emission site writes ahead of a literal's lambda or local
// function: the literal's [GoFunc] name, after the no-inlining mark when the literal needs one (litNoInliningPrefix),
// in one list. C#'s lambda-attribute grammar places the list before everything else, including an explicit return type.
func (v *Visitor) litAttributePrefix(funcLit *ast.FuncLit) string {
	var attributes []string

	if v.litNoInliningPrefix(funcLit) != "" {
		attributes = append(attributes, "MethodImpl(MethodImplOptions.NoInlining)")
	}

	if name := funcLitGoName(funcLit); name != "" {
		attributes = append(attributes, "GoFunc("+csharpStringLiteral(name)+")")
	}

	if len(attributes) == 0 {
		return ""
	}

	prefix := "["

	for i, attribute := range attributes {
		if i > 0 {
			prefix += ", "
		}

		prefix += attribute
	}

	return prefix + "] "
}
