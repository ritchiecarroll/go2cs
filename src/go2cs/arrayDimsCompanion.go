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
	"regexp"
	"slices"
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
// a companion, which of those functions are exported OVERLOADS (an additive arity beside the one every
// consumer already binds, see arrayDimsCompanionOverloaded), and which dims types each function's
// declaration owns (each name owned exactly once per side: production, or a `-tests` conversion's test files).
type arrayDimsPackageFacts struct {
	companions map[*types.Func][]int
	overloaded map[*types.Func]bool
	ownedTypes map[*types.Func][][]int64
}

// arrayDimsInputs is what the facts read from outside the package's own syntax.
type arrayDimsInputs struct {
	// goos is the conversion's target operating system, which decides a scoped manual registration.
	goos string

	// foreign answers the companion indices of a callee the package does not convert itself: a
	// hand-owned sink (handOwnedArrayDimsSinks) or another package's published record.
	foreign func(*types.Func) []int
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
func arrayDimsFactsFor(handle *packages.Package, inputs arrayDimsInputs) *arrayDimsPackageFacts {
	if handle == nil || handle.TypesInfo == nil || handle.Types == nil {
		return nil
	}

	arrayDimsFactsCacheLock.Lock()
	defer arrayDimsFactsCacheLock.Unlock()

	if arrayDimsFactsKey == handle.Types {
		return arrayDimsFactsValue
	}

	arrayDimsFactsKey = handle.Types
	arrayDimsFactsValue = computeArrayDimsFacts(handle, inputs)

	return arrayDimsFactsValue
}

type typeParamSlot struct {
	fn    *types.Func
	index int
}

// computeArrayDimsFacts runs two fixpoints over the package's generic functions.
//   - NEEDED, in two kinds. ZERO: a type parameter whose zero value the body builds (`var x T`,
//     `new(T)`). TYPE: a type parameter the body passes as the type argument of a TYPE-ONLY sink, a
//     callee outside the package that carries a companion (abi.TypeFor, or a function another package
//     published a record for). Either kind is forwarded backwards through own instantiations; ZERO
//     reaches only unexported owners, because an exported zero-value generic is refused at the call
//     site instead (arrayDimsRefusal, COORD ruling 2026-11-25).
//   - CARRYING: an EXPORTED function's TYPE-needed parameter always carries (its overload has to exist
//     whether or not a caller in this assembly needs it: the caller is usually in another one). An
//     unexported function's needed parameter carries when an instantiation passes it an array, or
//     passes it a carrying parameter of the enclosing function.
//
// Only carrying parameters gain a companion, so a generic no array reaches keeps its signature.
func computeArrayDimsFacts(handle *packages.Package, inputs arrayDimsInputs) *arrayDimsPackageFacts {
	info := handle.TypesInfo
	decls := map[*types.Func]*ast.FuncDecl{}
	owner := map[*types.TypeParam]typeParamSlot{}

	// A production declaration is emitted from the package WITHOUT its test files, so an instantiation
	// in a _test.go file cannot shape its arity: the test project would supply a companion the
	// production method never declared. A declaration in a _test.go file compiles with every test file
	// of its package, so all of them count.
	isTestFile := func(pos token.Pos) bool {
		return strings.HasSuffix(handle.Fset.Position(pos).Filename, "_test.go")
	}

	// A DISPLACED declaration's C# is hand-written (a manual registration or a hand-owned file), so the
	// converter can neither give it an overload nor publish a record for one. reflect.TypeFor is the
	// standing member: its Go body reaches abi.TypeFor, its C# does not have the arity-2 form.
	displaced := func(decl *ast.FuncDecl) bool {
		if fn, ok := info.Defs[decl.Name].(*types.Func); ok && packageHandOwnedFuncs[fn] {
			return true
		}

		return isManualFuncDeclInPackage(handle.Types.Path(), inputs.goos, decl)
	}

	// Every emitted function with a body, in source order: the owners a caller-side phantom can sit beside.
	var emitted []*ast.FuncDecl

	for _, file := range handle.Syntax {
		for _, decl := range file.Decls {
			funcDecl, isFunc := decl.(*ast.FuncDecl)

			if !isFunc || funcDecl.Body == nil {
				continue
			}

			fn, _ := info.Defs[funcDecl.Name].(*types.Func)

			if fn == nil {
				continue
			}

			if !displaced(funcDecl) {
				emitted = append(emitted, funcDecl)
			}

			if funcDecl.Recv != nil || funcDecl.Type.TypeParams == nil || fn.Exported() && displaced(funcDecl) {
				continue
			}

			typeParams := fn.Type().(*types.Signature).TypeParams()

			for i := range typeParams.Len() {
				owner[typeParams.At(i)] = typeParamSlot{fn, i}
			}

			decls[fn] = funcDecl
		}
	}

	sort.SliceStable(emitted, func(a, b int) bool {
		return handle.Fset.Position(emitted[a].Pos()).String() < handle.Fset.Position(emitted[b].Pos()).String()
	})

	// A `-tests` conversion's PRODUCTION declarations are compiled in production's assembly, so which of
	// them carry an overload is production's word, read from its record (seedProductionArrayDimsCompanions),
	// never re-derived here (P3 NIT 1). nil outside a `-tests` conversion.
	pinned := func(fn *types.Func) ([]int, bool) {
		if productionArrayDimsCompanions == nil || !fn.Exported() || isTestFile(decls[fn].Pos()) {
			return nil, false
		}

		return productionArrayDimsCompanions[fn.Name()], true
	}

	zeroNeeded := map[typeParamSlot]bool{}
	typeNeeded := map[typeParamSlot]bool{}

	for fn, decl := range decls {
		if indices, isPinned := pinned(fn); isPinned {
			for _, index := range indices {
				typeNeeded[typeParamSlot{fn, index}] = true
			}

			continue
		}

		if fn.Exported() {
			continue
		}

		for _, index := range zeroSinkTypeParams(info, decl, fn) {
			zeroNeeded[typeParamSlot{fn, index}] = true
		}
	}

	type instantiation struct {
		ident    *ast.Ident
		callee   *types.Func
		typeArgs *types.TypeList
	}

	// crossSide holds an own call that cannot shape its callee's arity (a test file instantiating a
	// production declaration): it still supplies, and so declares, dims when the callee is an overload.
	var instantiations, sinkCalls, crossSide []instantiation

	for ident, instance := range info.Instances {
		callee, isFunc := info.Uses[ident].(*types.Func)

		if !isFunc || instance.TypeArgs == nil {
			continue
		}

		if origin := callee.Origin(); origin != nil {
			callee = origin
		}

		if decl, inScope := decls[callee]; inScope {
			if isTestFile(decl.Pos()) || !isTestFile(ident.Pos()) {
				instantiations = append(instantiations, instantiation{ident, callee, instance.TypeArgs})
			} else {
				crossSide = append(crossSide, instantiation{ident, callee, instance.TypeArgs})
			}
		} else if inputs.foreign != nil && len(inputs.foreign(callee)) > 0 {
			sinkCalls = append(sinkCalls, instantiation{ident, callee, instance.TypeArgs})
		}
	}

	sort.Slice(sinkCalls, func(a, b int) bool { return sinkCalls[a].ident.Pos() < sinkCalls[b].ident.Pos() })

	// A type-only sink's companion position seeds TYPE need on the type parameter forwarded to it.
	ownedSlot := func(t types.Type) (typeParamSlot, bool) {
		forwarded, isTypeParam := types.Unalias(t).(*types.TypeParam)

		if !isTypeParam {
			return typeParamSlot{}, false
		}

		slot, owned := owner[forwarded]

		if owned {
			if _, isPinned := pinned(slot.fn); isPinned {
				return typeParamSlot{}, false
			}
		}

		return slot, owned
	}

	for _, call := range sinkCalls {
		for _, index := range inputs.foreign(call.callee) {
			if index < call.typeArgs.Len() {
				if slot, owned := ownedSlot(call.typeArgs.At(index)); owned {
					typeNeeded[slot] = true
				}
			}
		}
	}

	for changed := true; changed; {
		changed = false

		for _, inst := range instantiations {
			for i := range inst.typeArgs.Len() {
				calleeSlot := typeParamSlot{inst.callee, i}

				if !zeroNeeded[calleeSlot] && !typeNeeded[calleeSlot] {
					continue
				}

				slot, owned := ownedSlot(inst.typeArgs.At(i))

				if !owned {
					continue
				}

				if typeNeeded[calleeSlot] && !typeNeeded[slot] {
					typeNeeded[slot] = true
					changed = true
				}

				if zeroNeeded[calleeSlot] && !slot.fn.Exported() && !zeroNeeded[slot] {
					zeroNeeded[slot] = true
					changed = true
				}
			}
		}
	}

	carrying := map[typeParamSlot]bool{}
	overloaded := map[*types.Func]bool{}

	for slot := range typeNeeded {
		if slot.fn.Exported() {
			carrying[slot] = true
			overloaded[slot.fn] = true
		}
	}

	for changed := true; changed; {
		changed = false

		for _, inst := range instantiations {
			if inst.callee.Exported() {
				continue
			}

			for i := range inst.typeArgs.Len() {
				slot := typeParamSlot{inst.callee, i}

				if !zeroNeeded[slot] && !typeNeeded[slot] || carrying[slot] {
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

	facts := &arrayDimsPackageFacts{companions: map[*types.Func][]int{}, overloaded: overloaded, ownedTypes: map[*types.Func][][]int64{}}

	for slot := range carrying {
		facts.companions[slot.fn] = append(facts.companions[slot.fn], slot.index)
	}

	for fn := range facts.companions {
		sort.Ints(facts.companions[fn])
	}

	// Each dims type is declared ONCE per side (production, or a `-tests` conversion's test files, which
	// compile into another assembly), beside the first (by source position) function that needs it, so
	// two generics in two files never declare the same name twice.
	declared := map[bool]map[string]bool{false: {}, true: {}}
	pending := map[*types.Func]map[string][]int64{}

	own := func(fn *types.Func, side bool, dims []int64) {
		name := arrayDimsTypeName(dims)

		if declared[side][name] {
			return
		}

		declared[side][name] = true

		if pending[fn] == nil {
			pending[fn] = map[string][]int64{}
		}

		pending[fn][name] = dims
	}

	// An unexported carrier owns the dims its own-package call sites pass, beside its declaration.
	carriers := make([]*types.Func, 0, len(facts.companions))

	for fn := range facts.companions {
		if !overloaded[fn] {
			carriers = append(carriers, fn)
		}
	}

	sort.Slice(carriers, func(a, b int) bool {
		return handle.Fset.Position(decls[carriers[a]].Pos()).String() < handle.Fset.Position(decls[carriers[b]].Pos()).String()
	})

	for _, fn := range carriers {
		side := isTestFile(decls[fn].Pos())

		for _, inst := range instantiations {
			if inst.callee != fn {
				continue
			}

			for _, index := range facts.companions[fn] {
				if index < inst.typeArgs.Len() {
					if dims := arrayDimsOf(inst.typeArgs.At(index)); len(dims) > 0 {
						own(fn, side, dims)
					}
				}
			}
		}
	}

	// An OVERLOAD's call site may sit in another assembly, so the dims it passes are declared by the
	// CALLER's package: beside the emitted top-level function enclosing the call, or the first emitted
	// function on the call's side when no emitted function encloses it (a package-level initializer, or
	// a displaced declaration). Two assemblies' `ᴀ2` are different C# types with equal Dims, and the
	// descriptor interns on the dims, not on the phantom (abi.TypeFor<T, Tᴀ>).
	overloadCalls := append([]instantiation{}, sinkCalls...)

	for _, inst := range append(append([]instantiation{}, instantiations...), crossSide...) {
		if overloaded[inst.callee] {
			overloadCalls = append(overloadCalls, inst)
		}
	}

	sort.Slice(overloadCalls, func(a, b int) bool {
		return handle.Fset.Position(overloadCalls[a].ident.Pos()).String() < handle.Fset.Position(overloadCalls[b].ident.Pos()).String()
	})

	for _, call := range overloadCalls {
		indices := facts.companions[call.callee]

		if !overloaded[call.callee] {
			indices = inputs.foreign(call.callee)
		}

		side := isTestFile(call.ident.Pos())
		var enclosing *types.Func

		for _, decl := range emitted {
			if decl.Pos() <= call.ident.Pos() && call.ident.Pos() < decl.End() {
				enclosing, _ = info.Defs[decl.Name].(*types.Func)
				break
			}
		}

		if enclosing == nil {
			for _, decl := range emitted {
				if isTestFile(decl.Pos()) == side {
					enclosing, _ = info.Defs[decl.Name].(*types.Func)
					break
				}
			}
		}

		if enclosing == nil {
			continue
		}

		for _, index := range indices {
			if index < call.typeArgs.Len() {
				if dims := arrayDimsOf(call.typeArgs.At(index)); len(dims) > 0 {
					own(enclosing, side, dims)
				}
			}
		}
	}

	for fn, byName := range pending {
		names := make([]string, 0, len(byName))

		for name := range byName {
			names = append(names, name)
		}

		sort.Strings(names)

		for _, name := range names {
			facts.ownedTypes[fn] = append(facts.ownedTypes[fn], byName[name])
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

// handOwnedArrayDimsSinks lists the hand-owned functions that carry an array-dims companion overload,
// by package path, function name and companion indices. Their C# is hand-written, so no record is
// published for them; every package that calls one reads this table. internal/abi's TypeFor[T]() is a
// TYPE-ONLY sink: it builds no value, but C# spells every `[N]E` as one array<E>, so a descriptor
// synthesized from the type alone has no length (DESIGN-array-type-argument-length.md).
// `abi.TypeFor<T, Tᴀ>()` is in internal/abi/type_impl.cs.
var handOwnedArrayDimsSinks = map[string]map[string][]int{
	"internal/abi": {"TypeFor": {0}},
}

// handOwnedArrayDimsSink answers fn's companion indices when fn is a hand-owned sink, else nil.
func handOwnedArrayDimsSink(fn *types.Func) []int {
	if fn == nil || fn.Pkg() == nil {
		return nil
	}

	if signature, ok := fn.Type().(*types.Signature); !ok || signature.Recv() != nil {
		return nil
	}

	return handOwnedArrayDimsSinks[fn.Pkg().Path()][fn.Name()]
}

// The array-dims overloads a package publishes and those read back from imported packages. A package
// whose EXPORTED generic gains the arity-2 overload writes one record per companion position into its
// package_info.cs, `[assembly: GoArrayDimsCompanion("Make", 0)]` (packageInfoWriter); a consumer reads
// it and never re-derives the overload from the callee's Go source, so producer and consumer cannot
// disagree. A consumer converted against an older package finds no record and emits arity 1.
var (
	publishedArrayDimsCompanions  = map[string][]int{}
	importedArrayDimsCompanions   = map[string]map[string][]int{}
	productionArrayDimsCompanions map[string][]int
	arrayDimsRecordsLock          sync.Mutex
)

var arrayDimsCompanionRecordPattern = regexp.MustCompile(`^\[assembly: GoArrayDimsCompanion\("([^"]+)", ([0-9]+)\)\]$`)

// publishArrayDimsCompanions records an exported function's companion indices for this package's metadata.
func publishArrayDimsCompanions(goName string, indices []int) {
	arrayDimsRecordsLock.Lock()
	publishedArrayDimsCompanions[goName] = append([]int(nil), indices...)
	arrayDimsRecordsLock.Unlock()
}

// publishedArrayDimsCompanionRecords renders this package's published overloads as metadata records.
func publishedArrayDimsCompanionRecords() []string {
	arrayDimsRecordsLock.Lock()
	defer arrayDimsRecordsLock.Unlock()

	var records []string

	for name, indices := range publishedArrayDimsCompanions {
		for _, index := range indices {
			records = append(records, fmt.Sprintf("[assembly: GoArrayDimsCompanion(\"%s\", %d)]", name, index))
		}
	}

	return records
}

// parseArrayDimsCompanionRecords reads the records among a package_info.cs's lines, by function name.
func parseArrayDimsCompanionRecords(lines []string) map[string][]int {
	records := map[string][]int{}

	for _, line := range lines {
		if matches := arrayDimsCompanionRecordPattern.FindStringSubmatch(strings.TrimSpace(line)); matches != nil {
			if index, err := strconv.Atoi(matches[2]); err == nil && !slices.Contains(records[matches[1]], index) {
				records[matches[1]] = append(records[matches[1]], index)
			}
		}
	}

	for name := range records {
		sort.Ints(records[name])
	}

	return records
}

// seedProductionArrayDimsCompanions installs production's records for a `-tests` conversion, whose
// production declarations compile in production's assembly: which of them carry an overload is
// production's word (computeArrayDimsFacts' pinned). A missing file reads as no overloads.
func seedProductionArrayDimsCompanions(productionInfoPath string) {
	var records map[string][]int

	if lines, err := readPackageInfoLines(productionInfoPath); err == nil {
		records = parseArrayDimsCompanionRecords(lines)
	} else {
		records = map[string][]int{}
	}

	arrayDimsRecordsLock.Lock()
	productionArrayDimsCompanions = records
	arrayDimsRecordsLock.Unlock()
}

// resetArrayDimsRecords clears the per-package record state above (resetPackageState).
func resetArrayDimsRecords() {
	arrayDimsRecordsLock.Lock()
	publishedArrayDimsCompanions = map[string][]int{}
	importedArrayDimsCompanions = map[string]map[string][]int{}
	productionArrayDimsCompanions = nil
	arrayDimsRecordsLock.Unlock()
}

// foreignArrayDimsCompanions answers the companion indices of a function this package does not convert:
// a hand-owned sink, or an exported top-level function of another package that published a record.
func (v *Visitor) foreignArrayDimsCompanions(fn *types.Func) []int {
	if fn == nil || fn.Pkg() == nil {
		return nil
	}

	if origin := fn.Origin(); origin != nil {
		fn = origin
	}

	if indices := handOwnedArrayDimsSink(fn); indices != nil {
		return indices
	}

	if fn.Pkg() == v.pkg || !fn.Exported() {
		return nil
	}

	if signature, ok := fn.Type().(*types.Signature); !ok || signature.Recv() != nil || signature.TypeParams().Len() == 0 {
		return nil
	}

	path := fn.Pkg().Path()

	arrayDimsRecordsLock.Lock()
	records, loaded := importedArrayDimsCompanions[path]
	arrayDimsRecordsLock.Unlock()

	if !loaded {
		records = map[string][]int{}

		if info, ok := getImportPackageInfo([]string{path}, v.options)[path]; ok && info.Err == nil {
			if lines, ok := importPackageInfoLines(info, v.options); ok {
				records = parseArrayDimsCompanionRecords(lines)
			}
		}

		arrayDimsRecordsLock.Lock()
		importedArrayDimsCompanions[path] = records
		arrayDimsRecordsLock.Unlock()
	}

	return records[fn.Name()]
}

// arrayDimsInputs is the visitor's view of what the facts read from outside the package's syntax.
func (v *Visitor) arrayDimsInputs() arrayDimsInputs {
	return arrayDimsInputs{goos: goosOfTarget(v.options.targetPlatform), foreign: v.foreignArrayDimsCompanions}
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

	if fn.Pkg() == nil {
		return nil
	}

	if indices := handOwnedArrayDimsSink(fn); indices != nil {
		return indices
	}

	if fn.Pkg() != v.pkg {
		return v.foreignArrayDimsCompanions(fn)
	}

	facts := arrayDimsFactsFor(descriptorCarrierPackage(fn.Pkg()), v.arrayDimsInputs())

	if facts == nil {
		return nil
	}

	return facts.companions[fn]
}

// arrayDimsCompanionOverloaded reports whether fn's companion is an ADDITIVE overload: the arity a
// consumer always bound still exists beside it, so a call site supplies the companion only when it has
// dims to pass. True for a hand-owned sink, another package's published overload, and an own exported
// carrier; false for an own unexported carrier, whose every call site supplies.
func (v *Visitor) arrayDimsCompanionOverloaded(fn *types.Func) bool {
	if fn == nil || fn.Pkg() == nil {
		return false
	}

	if origin := fn.Origin(); origin != nil {
		fn = origin
	}

	if handOwnedArrayDimsSink(fn) != nil {
		return true
	}

	if fn.Pkg() != v.pkg {
		return len(v.foreignArrayDimsCompanions(fn)) > 0
	}

	facts := arrayDimsFactsFor(descriptorCarrierPackage(fn.Pkg()), v.arrayDimsInputs())

	return facts != nil && facts.overloaded[fn]
}

// arrayDimsCompanionSupply renders the companion arguments a call of fn with typeArgs supplies, in
// declared order: every carrying position of an unexported carrier, and, for an overload, every
// position only when one of them has dims to pass (an unnamed array type argument, or a forwarded type
// parameter that carries a companion). Otherwise nil, and the call keeps the arity it always had.
func (v *Visitor) arrayDimsCompanionSupply(fn *types.Func, typeArgs *types.TypeList) []string {
	indices := v.arrayDimsCompanionParams(fn)

	if len(indices) == 0 || typeArgs == nil {
		return nil
	}

	if v.arrayDimsCompanionOverloaded(fn) {
		supplies := false

		for _, index := range indices {
			if index < typeArgs.Len() && (len(arrayDimsOf(typeArgs.At(index))) > 0 || v.arrayDimsCompanionFor(typeArgs.At(index)) != "") {
				supplies = true
			}
		}

		if !supplies {
			return nil
		}
	}

	names := make([]string, 0, len(indices))

	for _, index := range indices {
		if index < typeArgs.Len() {
			names = append(names, v.arrayDimsCompanionArg(typeArgs.At(index)))
		}
	}

	return names
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

// calleeCarriesArrayDims reports whether a call supplies array-dims companions, which C# can never
// infer, so the call must render an explicit type-argument list (as calleeReadsDescriptorName).
func (v *Visitor) calleeCarriesArrayDims(funIdent *ast.Ident) bool {
	funcObj, isFunc := v.info.ObjectOf(funIdent).(*types.Func)

	if !isFunc {
		return false
	}

	instance, ok := v.info.Instances[funIdent]

	return ok && len(v.arrayDimsCompanionSupply(funcObj, instance.TypeArgs)) > 0
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

	facts := arrayDimsFactsFor(descriptorCarrierPackage(fn.Pkg()), v.arrayDimsInputs())

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

	carried := v.arrayDimsCompanionParams(fn)

	for _, index := range zeroSinkTypeParams(handle.TypesInfo, decl, fn) {
		if slices.Contains(carried, index) {
			continue
		}

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

// arrayDimsOverloadForwarder renders the arity-1 method of an array-dims OVERLOAD on one line: the
// declaration without its companions, forwarding `ᴀnone` to the arity-2 body, whose descriptor then
// answers exactly what the arity-1 method answered before the overload existed.
//
//	public static Handle<T> Make<T>(T value) => Make<T, ᴀnone>(value);
func (v *Visitor) arrayDimsOverloadForwarder(access string, isUnsafe bool, result, name, typeParams, params, constraints string) string {
	companions := map[string]bool{}

	for _, companion := range v.currentFuncArrayDimsNames {
		companions[companion] = true
	}

	var declared, passed []string

	for _, typeParam := range splitTopLevelParameters(strings.TrimSuffix(strings.TrimPrefix(typeParams, "<"), ">")) {
		argument := strings.TrimSpace(forwarderAttributeList.ReplaceAllString(typeParam, ""))

		if companions[argument] {
			passed = append(passed, arrayDimsNoneType)
			continue
		}

		declared = append(declared, strings.TrimSpace(typeParam))
		passed = append(passed, argument)
	}

	clauses := flattenConstraints(constraints)

	for companion := range companions {
		clauses = strings.ReplaceAll(clauses, " where "+companion+" : IGoArrayDims", "")
	}

	modifiers := access + " static"

	if isUnsafe {
		modifiers += " unsafe"
	}

	call := fmt.Sprintf("%s<%s>(%s)", name, strings.Join(passed, ", "), forwarderArguments(params))

	if strings.HasPrefix(result, "ref ") {
		call = "ref " + call
	}

	declaredList := ""

	if len(declared) > 0 {
		declaredList = "<" + strings.Join(declared, ", ") + ">"
	}

	return fmt.Sprintf("%s %s %s%s(%s)%s => %s;", modifiers, result, name, declaredList, params, clauses, call)
}
