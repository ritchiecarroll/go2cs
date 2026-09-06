// descriptorCompanion.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"go/ast"
	"go/types"
	"sync"

	"golang.org/x/tools/go/packages"
)

// THE DESCRIPTOR COMPANION — the one position the descriptor CARRIER cannot reach by cargo.
//
// A DEFINED type over the empty interface (`type testEface any`) is emitted as a `global using`
// alias to `object` (visitTypeSpec), which is what makes Go's universal assignability to `any` fall
// out of C# assignment for free. A `using` alias is compile-time only, so the Go NAME is erased by
// Roslyn before IL, and visitTypeSpec answers that by ALSO emitting an uninhabited carrier
// interface stamped [GoLocalName] — a real System.Type golib's existing naming reconstruction can
// name. Wherever the erased type appears in a position the converter can stamp, the carrier travels
// as attribute cargo (visitStructType's field arm, [GoDescriptorType]).
//
// A generic TYPE ARGUMENT is the position that cannot: it is bound per CALL, and an attribute is
// static. `testHandle[testEface](t, nil)` emits `testHandle<testEface>(…)` — that is
// `testHandle<object>` — so the callee's `reflect.TypeFor[T]().Name()` answers "" where Go answers
// "testEface", and `t.Run` then names the subtest from the empty string. Two distinct Go
// instantiations (`testEface` and `any`) are ONE CLR instantiation, so no call-site metadata can
// separate them; only a distinct type ARGUMENT can. Hence the companion: a generic function whose
// body reads a type parameter's Go name gains an extra C# type parameter carrying the carrier, and
// every call site supplies it.
//
// ⚠ THE NAMING SURFACE ONLY — and this narrowing is load-bearing, not an economy.
// `internal/abi.TypeFor[T]()` reaches the same descriptor but consumes it as an IDENTITY: `unique`'s
// production `Make[T]` keys `uniqueMaps` with it and its test's `checkMapsFor[T]` must LOAD with the
// same key. Threading that surface would (a) change a PUBLIC generic's C# signature, moving every
// consumer assembly (`net/netip` among them), and (b) risk a FALSE GREEN — a lookup keyed on the
// carrier while the store was keyed on `object` finds nothing and returns early through
// `checkMapsFor`'s own `if !ok { return }`, turning the two `testEface` subtests green for no
// reason at all. So identity stays exactly as it is and only the name travels. Censused over the
// Go 1.23.12 stdlib, the behavioral tree and Examples (7,523 files, go/parser only): five
// type-parameter `TypeFor` sites, all in `unique` — two `reflect.TypeFor[T]` (threaded here) and
// three `abi.TypeFor[T]` (deliberately untouched).
//
// `reflect.TypeOf((*T)(nil))` is NOT a second entry point: its four type-parameter uses in the
// stdlib (`arena`, `testing/fuzz`, and `TypeFor`'s own two bodies) all compare or allocate — none
// reads a name. It joins this predicate the day a naming use exists, not before.

// DescriptorCompanionSuffix names the extra C# type parameter that carries a type argument's Go
// name. Deliberately NOT DescriptorCarrierSuffix (U+1D05, ᴅ): a package declaring `type X any`
// emits a carrier `Xᴅ`, and a generic function there with a type parameter `X` would emit a
// companion spelled the same way — the type parameter would then SHADOW the carrier for the whole
// method body. U+1D3A (ᴺ) keeps the two spellings disjoint, so a census of either can never
// over-match the other.
const DescriptorCompanionSuffix = "ᴺ"

// descriptorCompanionName is the companion type parameter's C# identifier for a type parameter
// named goName — the ONE spelling, shared by the declaration (getGenericDefinition), the use
// (renderedTypeArgs' reflect.TypeFor arm) and the call site, so the three cannot drift apart.
func descriptorCompanionName(goName string) string {
	return goName + DescriptorCompanionSuffix
}

var (
	descriptorCompanionCache     = map[*types.Func][]int{}
	descriptorCompanionCacheLock sync.Mutex
)

// descriptorCompanionParams reports the indices, within fn's own declared type-parameter list, of
// the type parameters fn reads a Go NAME from — i.e. those it passes to `reflect.TypeFor`. Those
// are exactly the positions that gain a companion type parameter, in fn's emitted signature and at
// every one of its call sites.
//
// nil for a non-generic function, for one whose declaring package was loaded without syntax, and —
// the overwhelmingly common case — for one that reads no name at all, which is what keeps this off
// every other generic call in the corpus.
func (v *Visitor) descriptorCompanionParams(fn *types.Func) []int {
	if fn == nil {
		return nil
	}

	// An instantiated generic's use records the ORIGIN object; normalize so a call site and the
	// declaration agree on the cache key and on the type-parameter identities compared below.
	if origin := fn.Origin(); origin != nil {
		fn = origin
	}

	signature, ok := fn.Type().(*types.Signature)

	if !ok {
		return nil
	}

	typeParams := signature.TypeParams()

	if typeParams == nil || typeParams.Len() == 0 || fn.Pkg() == nil {
		return nil
	}

	descriptorCompanionCacheLock.Lock()
	cached, hit := descriptorCompanionCache[fn]
	descriptorCompanionCacheLock.Unlock()

	if hit {
		return cached
	}

	indices := computeDescriptorCompanionParams(fn, typeParams)

	descriptorCompanionCacheLock.Lock()
	descriptorCompanionCache[fn] = indices
	descriptorCompanionCacheLock.Unlock()

	return indices
}

// collectDescriptorCompanions maps fn's name-reading type parameters to their companion identifiers
// — the per-declaration set visitFuncDecl installs, keyed by the type-parameter OBJECT so a renderer
// can ask "does THIS type parameter carry a companion" without re-deriving an index. nil (not an
// empty map) when fn reads no name, which is every function in the corpus but two.
func (v *Visitor) collectDescriptorCompanions(fn *types.Func) map[*types.TypeParam]string {
	indices := v.descriptorCompanionParams(fn)

	if len(indices) == 0 {
		return nil
	}

	signature, ok := fn.Type().(*types.Signature)

	if !ok {
		return nil
	}

	typeParams := signature.TypeParams()
	companions := make(map[*types.TypeParam]string, len(indices))

	for _, index := range indices {
		if index < typeParams.Len() {
			typeParam := typeParams.At(index)
			companions[typeParam] = descriptorCompanionName(typeParam.Obj().Name())
		}
	}

	return companions
}

// computeDescriptorCompanionParams walks fn's declaration body for `reflect.TypeFor[P]` where P is
// one of fn's own type parameters. The DECLARING package's syntax and type info are read — the same
// handle descriptorCarrierFor consults for the alias predicate — so a call site in another package
// computes the same answer the declaration emitted, which is what makes the arity agree across an
// assembly boundary without any metadata.
func computeDescriptorCompanionParams(fn *types.Func, typeParams *types.TypeParamList) []int {
	handle := descriptorCarrierPackage(fn.Pkg())

	if handle == nil || handle.TypesInfo == nil {
		return nil
	}

	decl := funcDeclFor(handle, fn)

	if decl == nil || decl.Body == nil {
		return nil
	}

	// Identity set of fn's own type parameters, so a SHADOWING generic (a nested literal cannot
	// declare type parameters in Go, but a same-named parameter of a called generic can appear in
	// the index position) cannot be mistaken for one of fn's.
	owned := make(map[*types.TypeParam]int, typeParams.Len())

	for i := range typeParams.Len() {
		owned[typeParams.At(i)] = i
	}

	var found []int
	seen := make(map[int]bool, typeParams.Len())

	ast.Inspect(decl.Body, func(node ast.Node) bool {
		indexExpr, isIndex := node.(*ast.IndexExpr)

		if !isIndex || !isReflectTypeForSelector(handle.TypesInfo, indexExpr.X) {
			return true
		}

		typeParam, isTypeParam := handle.TypesInfo.TypeOf(indexExpr.Index).(*types.TypeParam)

		if !isTypeParam {
			return true
		}

		if index, isOwned := owned[typeParam]; isOwned && !seen[index] {
			seen[index] = true
			found = append(found, index)
		}

		return true
	})

	if len(found) == 0 {
		return nil
	}

	// Ascending, so the emitted companion list and the call site's appended arguments agree on
	// order however the body happened to mention them.
	for i := 1; i < len(found); i++ {
		for j := i; j > 0 && found[j] < found[j-1]; j-- {
			found[j], found[j-1] = found[j-1], found[j]
		}
	}

	return found
}

// isReflectTypeForSelector reports whether expr names `reflect.TypeFor` — the reflection surface's
// static-type entry point, and the only converted callable whose result a Go body reads a TYPE NAME
// out of. Resolved through the object rather than the written qualifier so an import rename
// (`Δreflect`) or a same-package reference inside `reflect` itself answers identically.
func isReflectTypeForSelector(info *types.Info, expr ast.Expr) bool {
	var ident *ast.Ident

	switch e := expr.(type) {
	case *ast.SelectorExpr:
		ident = e.Sel
	case *ast.Ident:
		ident = e
	default:
		return false
	}

	funcObj, isFunc := info.Uses[ident].(*types.Func)

	if !isFunc || funcObj.Name() != "TypeFor" || funcObj.Pkg() == nil {
		return false
	}

	return funcObj.Pkg().Path() == "reflect"
}

// funcDeclFor finds fn's declaration in the loaded package's syntax. Matched through
// TypesInfo.Defs rather than by name so a method and a function sharing a name, or two files
// declaring into one package, cannot be confused.
func funcDeclFor(handle *packages.Package, fn *types.Func) *ast.FuncDecl {
	if handle.TypesInfo == nil {
		return nil
	}

	for _, file := range handle.Syntax {
		for _, decl := range file.Decls {
			funcDecl, isFunc := decl.(*ast.FuncDecl)

			if !isFunc || funcDecl.Name == nil {
				continue
			}

			if defined, _ := handle.TypesInfo.Defs[funcDecl.Name].(*types.Func); defined == fn {
				return funcDecl
			}
		}
	}

	return nil
}
