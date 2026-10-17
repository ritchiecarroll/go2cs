// arrayDimsCompanion.go - Gbtc
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
	"sort"
	"strconv"
	"strings"
	"sync"

	"golang.org/x/tools/go/packages"
)

// THE ARRAY-DIMS COMPANION: the length of an ARRAY type argument, for a generic that builds a zero value.
//
// C# spells every Go `[N]E` as the one `array<E>`, whose length lives in the INSTANCE. A generic
// instantiated with an array has no length to build `var x T` or `new(T)` from, so both read as a
// length-0 array: runtime's `testFreegc[[16]byte]` measured `unsafe.Sizeof(*p) == 0`, and every
// TestFreegc row failed on it. A type argument is bound per call, so only a distinct type ARGUMENT
// can carry the length, the same reason descriptorCompanion.go threads a carrier for a Go name.
//
// So a generic whose body builds a zero value of one of its own type parameters, and that an own-package
// instantiation passes an array, gains one extra C# type parameter per such type parameter: `Tᴀ`,
// constrained to golib's IGoArrayDims. Every call site supplies a phantom type naming the dims
// (`ᴀ16`, `ᴀ2_3`), golib's `ᴀnone` for a non-array type argument, or the CALLER's own companion when
// it forwards a type parameter that carries one. In the body, `var x T` reads `GoArrayZero.Of<T, Tᴀ>()`
// and `new(T)` reads `GoArrayZero.New<T, Tᴀ>()`. `unsafe.Sizeof` needs nothing: it reads dims from the
// live value, which is now sized.
//
// SCOPE (COORD ruling 2026-10-17): an UNEXPORTED top-level generic function, with an own-package
// instantiation that passes an array. An unexported function's every caller is in its own package,
// so its C# arity can change without moving another assembly. A forwarding generic (one that passes
// its own type parameter on to such a function) is in scope by the same fixpoint. An EXPORTED generic
// in the same shape is refused loudly at the call site instead (arrayDimsRefusal): a converter warning
// and an emitted panic, never a silent length-0 array. Go 1.26.9 census (go/types over std plus tests):
// 16 instantiations take an unnamed array type argument, across 8 callees; only runtime_test's
// testFreegc (8 instantiations) builds a zero value of the type parameter, so it is the one function
// that gains a companion, and 0 exported functions are refused.

// ArrayDimsCompanionSuffix names the extra C# type parameter that carries a type argument's array
// dims. Disjoint from DescriptorCompanionSuffix (ᴺ) and DescriptorCarrierSuffix (ᴅ), and a SUFFIX so
// it can never collide with the `ᴀ`-PREFIXED dims types.
const ArrayDimsCompanionSuffix = "ᴀ"

// arrayDimsTypePrefix starts every phantom dims type's name; `ᴀnone` (no dims) is golib's.
const arrayDimsTypePrefix = "ᴀ"

const arrayDimsNoneType = "ᴀnone"

// arrayDimsCompanionName is the companion type parameter's C# identifier for a type parameter named
// goName: the ONE spelling the declaration, the body's uses and every call site share.
func arrayDimsCompanionName(goName string) string {
	return goName + ArrayDimsCompanionSuffix
}

// arrayDimsTypeName is the phantom type naming dims: `ᴀ16`, `ᴀ2_3`.
func arrayDimsTypeName(dims []int64) string {
	parts := make([]string, len(dims))

	for i, dim := range dims {
		parts[i] = strconv.FormatInt(dim, 10)
	}

	return arrayDimsTypePrefix + strings.Join(parts, "_")
}

// arrayDimsOf returns the dims of an UNNAMED array type argument, outermost first, or nil for anything
// else. A NAMED array type (`type row [4]byte`) has a generated wrapper that knows its own length, so it
// takes no dims.
func arrayDimsOf(t types.Type) []int64 {
	var dims []int64

	t = types.Unalias(t)

	for {
		arrayType, isArray := t.(*types.Array)

		if !isArray {
			break
		}

		dims = append(dims, arrayType.Len())
		t = types.Unalias(arrayType.Elem())
	}

	return dims
}

// arrayDimsPackageFacts is one package's answer: which type-parameter indices of which functions carry
// a companion, and which dims types each function's declaration owns (each name owned exactly once).
type arrayDimsPackageFacts struct {
	companions map[*types.Func][]int
	ownedTypes map[*types.Func][][]int64
}

// ONE cached package, keyed by its types package. Keying a longer-lived cache by the *packages.Package
// handle pinned every converted package's syntax and type info for the whole run: the first -stdlib arm
// of this cut was killed by the out-of-memory killer at 13.8 GB, a third of the way through.
var (
	arrayDimsFactsKey       *types.Package
	arrayDimsFactsValue     *arrayDimsPackageFacts
	arrayDimsFactsCacheLock sync.Mutex
)

// arrayDimsFactsFor computes the companion indices for every generic function the package declares,
// once per package in a row.
func arrayDimsFactsFor(handle *packages.Package) *arrayDimsPackageFacts {
	if handle == nil || handle.TypesInfo == nil || handle.Types == nil {
		return nil
	}

	arrayDimsFactsCacheLock.Lock()
	defer arrayDimsFactsCacheLock.Unlock()

	if arrayDimsFactsKey == handle.Types {
		return arrayDimsFactsValue
	}

	arrayDimsFactsKey = handle.Types
	arrayDimsFactsValue = computeArrayDimsFacts(handle)

	return arrayDimsFactsValue
}

type typeParamSlot struct {
	fn    *types.Func
	index int
}

// computeArrayDimsFacts runs two fixpoints over the package's generic functions.
//   - NEEDED: a type parameter whose zero value the body builds (`var x T`, `new(T)`), or that is
//     forwarded as the type argument of a needed parameter of another in-scope function.
//   - CARRYING: a needed parameter that an instantiation passes an array, or passes a carrying
//     parameter of the enclosing function.
//
// Only carrying parameters gain a companion, so a generic no array reaches keeps its signature.
func computeArrayDimsFacts(handle *packages.Package) *arrayDimsPackageFacts {
	info := handle.TypesInfo
	decls := map[*types.Func]*ast.FuncDecl{}
	owner := map[*types.TypeParam]typeParamSlot{}

	for _, file := range handle.Syntax {
		for _, decl := range file.Decls {
			funcDecl, isFunc := decl.(*ast.FuncDecl)

			if !isFunc || funcDecl.Recv != nil || funcDecl.Body == nil || funcDecl.Type.TypeParams == nil {
				continue
			}

			fn, _ := info.Defs[funcDecl.Name].(*types.Func)

			if fn == nil || fn.Exported() {
				continue
			}

			typeParams := fn.Type().(*types.Signature).TypeParams()

			for i := range typeParams.Len() {
				owner[typeParams.At(i)] = typeParamSlot{fn, i}
			}

			decls[fn] = funcDecl
		}
	}

	needed := map[typeParamSlot]bool{}

	for fn, decl := range decls {
		for _, index := range zeroSinkTypeParams(info, decl, fn) {
			needed[typeParamSlot{fn, index}] = true
		}
	}

	// A production declaration is emitted from the package WITHOUT its test files, so an instantiation
	// in a _test.go file cannot shape its arity: the test project would supply a companion the
	// production method never declared. A declaration in a _test.go file compiles with every test file
	// of its package, so all of them count.
	isTestFile := func(pos token.Pos) bool {
		return strings.HasSuffix(handle.Fset.Position(pos).Filename, "_test.go")
	}

	type instantiation struct {
		callee   *types.Func
		typeArgs *types.TypeList
	}

	var instantiations []instantiation

	for ident, instance := range info.Instances {
		callee, isFunc := info.Uses[ident].(*types.Func)

		if !isFunc || instance.TypeArgs == nil {
			continue
		}

		if origin := callee.Origin(); origin != nil {
			callee = origin
		}

		if decl, inScope := decls[callee]; inScope && (isTestFile(decl.Pos()) || !isTestFile(ident.Pos())) {
			instantiations = append(instantiations, instantiation{callee, instance.TypeArgs})
		}
	}

	for changed := true; changed; {
		changed = false

		for _, inst := range instantiations {
			for i := range inst.typeArgs.Len() {
				if !needed[typeParamSlot{inst.callee, i}] {
					continue
				}

				if forwarded, isTypeParam := inst.typeArgs.At(i).(*types.TypeParam); isTypeParam {
					if slot, owned := owner[forwarded]; owned && !needed[slot] {
						needed[slot] = true
						changed = true
					}
				}
			}
		}
	}

	carrying := map[typeParamSlot]bool{}

	for changed := true; changed; {
		changed = false

		for _, inst := range instantiations {
			for i := range inst.typeArgs.Len() {
				slot := typeParamSlot{inst.callee, i}

				if !needed[slot] || carrying[slot] {
					continue
				}

				typeArg := inst.typeArgs.At(i)

				if len(arrayDimsOf(typeArg)) > 0 {
					carrying[slot] = true
					changed = true
				} else if forwarded, isTypeParam := typeArg.(*types.TypeParam); isTypeParam && carrying[owner[forwarded]] {
					carrying[slot] = true
					changed = true
				}
			}
		}
	}

	facts := &arrayDimsPackageFacts{companions: map[*types.Func][]int{}, ownedTypes: map[*types.Func][][]int64{}}

	for slot := range carrying {
		facts.companions[slot.fn] = append(facts.companions[slot.fn], slot.index)
	}

	for fn := range facts.companions {
		sort.Ints(facts.companions[fn])
	}

	// Each dims type is declared ONCE per package, beside the first (by source position) function
	// whose call sites pass it, so two generics in two files never declare the same name twice.
	carriers := make([]*types.Func, 0, len(facts.companions))

	for fn := range facts.companions {
		carriers = append(carriers, fn)
	}

	sort.Slice(carriers, func(a, b int) bool {
		return handle.Fset.Position(decls[carriers[a]].Pos()).String() < handle.Fset.Position(decls[carriers[b]].Pos()).String()
	})

	declared := map[string]bool{}

	for _, fn := range carriers {
		var names []string
		dimsByName := map[string][]int64{}

		for _, inst := range instantiations {
			if inst.callee != fn {
				continue
			}

			for _, index := range facts.companions[fn] {
				if index >= inst.typeArgs.Len() {
					continue
				}

				if dims := arrayDimsOf(inst.typeArgs.At(index)); len(dims) > 0 {
					name := arrayDimsTypeName(dims)

					if !declared[name] {
						declared[name] = true
						dimsByName[name] = dims
						names = append(names, name)
					}
				}
			}
		}

		sort.Strings(names)

		for _, name := range names {
			facts.ownedTypes[fn] = append(facts.ownedTypes[fn], dimsByName[name])
		}
	}

	return facts
}

// zeroSinkTypeParams returns the indices of fn's own type parameters whose zero value decl's body
// builds: a `var x T` without a value, or `new(T)`. Func literals inside the body count, since they
// share the declaration's type parameters.
func zeroSinkTypeParams(info *types.Info, decl *ast.FuncDecl, fn *types.Func) []int {
	typeParams := fn.Type().(*types.Signature).TypeParams()
	owned := make(map[*types.TypeParam]int, typeParams.Len())

	for i := range typeParams.Len() {
		owned[typeParams.At(i)] = i
	}

	seen := map[int]bool{}

	mark := func(t types.Type) {
		if typeParam, isTypeParam := types.Unalias(t).(*types.TypeParam); isTypeParam {
			if index, isOwned := owned[typeParam]; isOwned {
				seen[index] = true
			}
		}
	}

	ast.Inspect(decl.Body, func(node ast.Node) bool {
		switch n := node.(type) {
		case *ast.ValueSpec:
			if n.Type != nil && len(n.Values) == 0 {
				mark(info.TypeOf(n.Type))
			}
		case *ast.CallExpr:
			if ident, isIdent := ast.Unparen(n.Fun).(*ast.Ident); isIdent && len(n.Args) == 1 {
				if builtin, isBuiltin := info.Uses[ident].(*types.Builtin); isBuiltin && builtin.Name() == "new" {
					mark(info.TypeOf(n.Args[0]))
				}
			}
		}

		return true
	})

	indices := make([]int, 0, len(seen))

	for index := range seen {
		indices = append(indices, index)
	}

	sort.Ints(indices)

	return indices
}

// arrayDimsCompanionParams reports the indices, within fn's declared type-parameter list, that carry an
// array-dims companion; nil for every function this does not apply to.
func (v *Visitor) arrayDimsCompanionParams(fn *types.Func) []int {
	if fn == nil {
		return nil
	}

	if origin := fn.Origin(); origin != nil {
		fn = origin
	}

	// Only the package being converted: every caller of an unexported generic is in its own package.
	if fn.Pkg() == nil || fn.Pkg() != v.pkg || fn.Exported() {
		return nil
	}

	facts := arrayDimsFactsFor(descriptorCarrierPackage(fn.Pkg()))

	if facts == nil {
		return nil
	}

	return facts.companions[fn]
}

// collectArrayDimsCompanions maps fn's carrying type parameters to their companion identifiers: the
// per-declaration set visitFuncDecl installs (currentFuncArrayDimsNames).
func (v *Visitor) collectArrayDimsCompanions(fn *types.Func) map[*types.TypeParam]string {
	indices := v.arrayDimsCompanionParams(fn)

	if len(indices) == 0 {
		return nil
	}

	typeParams := fn.Type().(*types.Signature).TypeParams()
	companions := make(map[*types.TypeParam]string, len(indices))

	for _, index := range indices {
		typeParam := typeParams.At(index)
		companions[typeParam] = arrayDimsCompanionName(typeParam.Obj().Name())
	}

	return companions
}

// calleeCarriesArrayDims reports whether a call's callee gains array-dims companions, which C# can never
// infer, so the call must render an explicit type-argument list (as calleeReadsDescriptorName).
func (v *Visitor) calleeCarriesArrayDims(funIdent *ast.Ident) bool {
	funcObj, isFunc := v.info.ObjectOf(funIdent).(*types.Func)

	return isFunc && len(v.arrayDimsCompanionParams(funcObj)) > 0
}

// arrayDimsCompanionArg renders the companion argument a call site supplies for one type argument:
// the dims type of an array, the enclosing declaration's own companion for a forwarded type parameter
// that carries one, and `ᴀnone` otherwise.
func (v *Visitor) arrayDimsCompanionArg(typeArg types.Type) string {
	if typeParam, isTypeParam := types.Unalias(typeArg).(*types.TypeParam); isTypeParam {
		if companion, has := v.currentFuncArrayDimsNames[typeParam]; has {
			return companion
		}

		return arrayDimsNoneType
	}

	if dims := arrayDimsOf(typeArg); len(dims) > 0 {
		return arrayDimsTypeName(dims)
	}

	return arrayDimsNoneType
}

// arrayDimsCompanionFor returns the current declaration's companion for type parameter t, or "".
func (v *Visitor) arrayDimsCompanionFor(t types.Type) string {
	if typeParam, isTypeParam := types.Unalias(t).(*types.TypeParam); isTypeParam {
		return v.currentFuncArrayDimsNames[typeParam]
	}

	return ""
}

// arrayDimsTypeDeclarations renders the dims types fn's declaration owns, one per line, at the class
// member indent; "" when it owns none. Each is a sealed CLASS, never a struct: go2cs-gen treats every
// struct declared directly in a package class as a converted Go type (Common.IsConvertedGoTypeDeclaration)
// and would generate its members, while a class is a Go type only when it carries a definition comment.
// A phantom type argument needs no value semantics.
func (v *Visitor) arrayDimsTypeDeclarations(fn *types.Func) string {
	if origin := fn.Origin(); origin != nil {
		fn = origin
	}

	if fn.Pkg() == nil || fn.Pkg() != v.pkg {
		return ""
	}

	facts := arrayDimsFactsFor(descriptorCarrierPackage(fn.Pkg()))

	if facts == nil || len(facts.ownedTypes[fn]) == 0 {
		return ""
	}

	var result strings.Builder

	for _, dims := range facts.ownedTypes[fn] {
		values := make([]string, len(dims))

		for i, dim := range dims {
			values[i] = strconv.FormatInt(dim, 10)
		}

		fmt.Fprintf(&result, "%s%sprivate sealed class %s : IGoArrayDims { public static nint[] Dims { get; } = [%s]; }",
			v.newline, v.indent(v.indentLevel), arrayDimsTypeName(dims), strings.Join(values, ", "))
	}

	return result.String()
}

// arrayDimsRefusal returns the converter's refusal text for an instantiation of an EXPORTED generic
// declared in the package being converted, whose type argument at a zero-sink parameter is an unnamed
// array, or "" when the instantiation is not refused. Such a function cannot gain a companion (its
// C# arity is seen by other assemblies), so its zero value would be a silent length-0 array.
func (v *Visitor) arrayDimsRefusal(funIdent *ast.Ident, typeArgs *types.TypeList) string {
	fn, isFunc := v.info.ObjectOf(funIdent).(*types.Func)

	if !isFunc || typeArgs == nil || !fn.Exported() || fn.Pkg() != v.pkg {
		return ""
	}

	if origin := fn.Origin(); origin != nil {
		fn = origin
	}

	if signature, ok := fn.Type().(*types.Signature); !ok || signature.Recv() != nil {
		return ""
	}

	handle := descriptorCarrierPackage(fn.Pkg())

	if handle == nil || handle.TypesInfo == nil {
		return ""
	}

	decl := funcDeclFor(handle, fn)

	if decl == nil || decl.Body == nil {
		return ""
	}

	for _, index := range zeroSinkTypeParams(handle.TypesInfo, decl, fn) {
		if index < typeArgs.Len() && len(arrayDimsOf(typeArgs.At(index))) > 0 {
			return fmt.Sprintf("go2cs: exported generic %s builds a zero value of its type parameter, and its array type argument %s has no length in C# (the array-dims companion applies only to an unexported generic)",
				fn.Name(), typeArgs.At(index).String())
		}
	}

	return ""
}

// arrayDimsRefusalCall renders a refused call as golib's GoArrayZero.Refuse, typed as the call's result
// so the emission still compiles where the value is used, and panicking with the refusal when run.
func (v *Visitor) arrayDimsRefusalCall(callExpr *ast.CallExpr, refusal string) string {
	message := strconv.Quote(refusal)

	switch results := v.info.TypeOf(callExpr).(type) {
	case nil:
		return fmt.Sprintf("GoArrayZero.Refuse(%s)", message)
	case *types.Tuple:
		if results.Len() == 0 {
			return fmt.Sprintf("GoArrayZero.Refuse(%s)", message)
		}

		names := make([]string, results.Len())

		for i := range results.Len() {
			names[i] = v.getCSharpTypeName(results.At(i).Type())
		}

		return fmt.Sprintf("GoArrayZero.Refuse<(%s)>(%s)", strings.Join(names, ", "), message)
	default:
		return fmt.Sprintf("GoArrayZero.Refuse<%s>(%s)", v.getCSharpTypeName(results), message)
	}
}
