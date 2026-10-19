// inlineConstraintNamed.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/token"
	"go/types"
	"regexp"
	"strconv"
	"strings"
	"sync"
)

// An inline constraint that names the parameter it constrains — `type Ord[T interface{ Less(T) bool }]` —
// lifts to the generic interface `Ord_T<T>` (liftTypeParamConstraints). A NAMED constraint of the same
// shape, `Lesser[T]`, is closed over each instantiation's argument by the instantiation machinery: a value
// argument records `GoImplement<age, Lesser<age>>` (recordClosedConstraintImplementation), a pointer one
// takes the constraint proxy `nodeжLesser` (constraintProxyFor). Both read the constraint as an
// instantiated *types.Named, which an anonymous interface is not, so the inline form recorded nothing and
// every instantiation of the type was CS0315.
//
// inlineConstraintNamed gives such a constraint the named view those paths read: a synthetic generic
// interface carrying the lift's name, over fresh parameters standing for the ones the constraint captures,
// instantiated back over the declaration's own. Nothing else changes: the same two paths record and proxy
// it, and the generator reads the lift's own C# declaration (`partial interface Ord_T<T>`), exactly as it
// reads Lesser's.
//
// THE NAME is assigned by the declaration's own visit (getUniqueLiftedTypeName, package-wide claims), so an
// instantiation in another file can arrive before it exists. Such a site DEFERS: its records replay after
// the file-visit barrier (replayDeferredConstraintRecords, before any package_info is written), and a proxy
// name it renders carries the deferred marker, which the barrier's resolveDynamicTypeMarkers resolves
// through the same registry (lookupConstraintLift).

// substituteTypeParams returns t with every type parameter in m replaced by its mapping. go/types offers
// Instantiate for a generic Named or Signature only, never for an anonymous type over a parameter bound
// elsewhere, which is what an inline constraint is. A type with nothing to replace is returned unchanged.
func substituteTypeParams(t types.Type, m map[*types.TypeParam]types.Type) types.Type {
	if t == nil || len(m) == 0 {
		return t
	}

	switch typ := t.(type) {
	case *types.TypeParam:
		if replacement, ok := m[typ]; ok {
			return replacement
		}
	case *types.Alias:
		return substituteTypeParams(types.Unalias(typ), m)
	case *types.Pointer:
		if elem := substituteTypeParams(typ.Elem(), m); elem != typ.Elem() {
			return types.NewPointer(elem)
		}
	case *types.Slice:
		if elem := substituteTypeParams(typ.Elem(), m); elem != typ.Elem() {
			return types.NewSlice(elem)
		}
	case *types.Array:
		if elem := substituteTypeParams(typ.Elem(), m); elem != typ.Elem() {
			return types.NewArray(elem, typ.Len())
		}
	case *types.Map:
		key, elem := substituteTypeParams(typ.Key(), m), substituteTypeParams(typ.Elem(), m)

		if key != typ.Key() || elem != typ.Elem() {
			return types.NewMap(key, elem)
		}
	case *types.Chan:
		if elem := substituteTypeParams(typ.Elem(), m); elem != typ.Elem() {
			return types.NewChan(typ.Dir(), elem)
		}
	case *types.Signature:
		params, paramsChanged := substituteTuple(typ.Params(), m)
		results, resultsChanged := substituteTuple(typ.Results(), m)

		if paramsChanged || resultsChanged {
			return types.NewSignatureType(nil, nil, nil, params, results, typ.Variadic())
		}
	case *types.Struct:
		fields := make([]*types.Var, typ.NumFields())
		tags := make([]string, typ.NumFields())
		changed := false

		for i := range typ.NumFields() {
			field := typ.Field(i)
			fieldType := substituteTypeParams(field.Type(), m)
			changed = changed || fieldType != field.Type()
			fields[i] = types.NewField(field.Pos(), field.Pkg(), field.Name(), fieldType, field.Embedded())
			tags[i] = typ.Tag(i)
		}

		if changed {
			return types.NewStruct(fields, tags)
		}
	case *types.Interface:
		methods := make([]*types.Func, typ.NumExplicitMethods())
		embeddeds := make([]types.Type, typ.NumEmbeddeds())
		changed := false

		for i := range typ.NumExplicitMethods() {
			method := typ.ExplicitMethod(i)
			signature := substituteTypeParams(method.Type(), m)
			changed = changed || signature != method.Type()

			// A fresh receiver-less signature: NewInterfaceType sets the receiver to the new interface.
			if sig, ok := signature.(*types.Signature); ok {
				signature = types.NewSignatureType(nil, nil, nil, sig.Params(), sig.Results(), sig.Variadic())
			}

			methods[i] = types.NewFunc(method.Pos(), method.Pkg(), method.Name(), signature.(*types.Signature))
		}

		for i := range typ.NumEmbeddeds() {
			embeddeds[i] = substituteTypeParams(typ.EmbeddedType(i), m)
			changed = changed || embeddeds[i] != typ.EmbeddedType(i)
		}

		if changed {
			return types.NewInterfaceType(methods, embeddeds).Complete()
		}
	case *types.Union:
		terms := make([]*types.Term, typ.Len())
		changed := false

		for i := range typ.Len() {
			term := typ.Term(i)
			termType := substituteTypeParams(term.Type(), m)
			changed = changed || termType != term.Type()
			terms[i] = types.NewTerm(term.Tilde(), termType)
		}

		if changed {
			return types.NewUnion(terms)
		}
	case *types.Named:
		args := typ.TypeArgs()

		if args == nil || args.Len() == 0 {
			return t
		}

		closed := make([]types.Type, args.Len())
		changed := false

		for i := range args.Len() {
			closed[i] = substituteTypeParams(args.At(i), m)
			changed = changed || closed[i] != args.At(i)
		}

		if changed {
			if instance, err := types.Instantiate(nil, typ.Origin(), closed, false); err == nil {
				return instance
			}
		}
	}

	return t
}

func substituteTuple(tuple *types.Tuple, m map[*types.TypeParam]types.Type) (*types.Tuple, bool) {
	if tuple == nil || tuple.Len() == 0 {
		return tuple, false
	}

	vars := make([]*types.Var, tuple.Len())
	changed := false

	for i := range tuple.Len() {
		v := tuple.At(i)
		varType := substituteTypeParams(v.Type(), m)
		changed = changed || varType != v.Type()
		vars[i] = types.NewParam(v.Pos(), v.Pkg(), v.Name(), varType)
	}

	return types.NewTuple(vars...), changed
}

// inlineConstraintKey names a DECLARED inline constraint for the lift registry and the deferred marker. An
// instantiation site reads the declaration's own type parameters (the origin's), so it holds the very
// constraint object the declaration lifted. The key is meaningful within one package conversion only.
func inlineConstraintKey(iface *types.Interface) string {
	return fmt.Sprintf("inline constraint %p", iface)
}

// The synthetic generic interface for each inline constraint whose lift name is known, shared by every file
// of the package so that every instantiation names one origin.
var (
	inlineConstraintOrigins     = map[*types.Interface]*types.Named{}
	inlineConstraintOriginsLock sync.Mutex
)

// inlineConstraintNamed is typeParam's inline constraint as the instantiated generic interface its lift
// declares (`Ord_T[T]`), when the constraint is an anonymous interface carrying methods that captures a
// type parameter of a declaration. resolved is false when the lift's name is not known yet: the constraint
// then carries the deferred marker as its name, and a caller that RECORDS must defer (deferConstraintRecord)
// rather than record it. A declaration in ANOTHER package is named by the lift its package published
// (importedInlineConstraintLift), and keeps its anonymous form when it published none.
func (v *Visitor) inlineConstraintNamed(typeParam *types.TypeParam) (constraint *types.Named, resolved bool, ok bool) {
	iface, isInterface := typeParam.Constraint().(*types.Interface)

	if !isInterface || iface.NumMethods() == 0 {
		return nil, false, false
	}

	captured := liftCapturedTypeParams(iface)

	if len(captured) == 0 {
		return nil, false, false
	}

	foreignName := ""

	if typeParam.Obj().Pkg() != v.pkg {
		if foreignName = v.importedInlineConstraintLift(typeParam); foreignName == "" {
			return nil, false, false
		}
	}

	origin, resolved := v.inlineConstraintOrigin(iface, captured, foreignName)
	args := make([]types.Type, len(captured))

	for i, param := range captured {
		args[i] = param
	}

	instance, err := types.Instantiate(nil, origin, args, false)

	if err != nil {
		return nil, false, false
	}

	named, isNamed := instance.(*types.Named)

	return named, resolved, isNamed
}

// inlineConstraintOrigin builds (or reuses) the synthetic generic interface: the lift's name, one fresh
// parameter per captured one, and the constraint's method set rewritten over them. foreignName is the name
// a foreign declaration's package published, empty for a declaration in this package.
func (v *Visitor) inlineConstraintOrigin(iface *types.Interface, captured []*types.TypeParam, foreignName string) (*types.Named, bool) {
	inlineConstraintOriginsLock.Lock()
	origin, cached := inlineConstraintOrigins[iface]
	inlineConstraintOriginsLock.Unlock()

	if cached {
		return origin, true
	}

	name, known := foreignName, foreignName != ""

	if !known {
		name, known = v.liftedNameFor(iface)
	}

	if !known {
		name = lookupConstraintLift(inlineConstraintKey(iface))
	}

	resolved := name != ""

	if !resolved {
		name = dynamicTypeMarker(inlineConstraintKey(iface))
	} else if idx := strings.IndexByte(name, '<'); idx >= 0 {
		name = name[:idx]
	}

	pkg := captured[0].Obj().Pkg()
	fresh := make([]*types.TypeParam, len(captured))
	mapping := make(map[*types.TypeParam]types.Type, len(captured))

	for i, param := range captured {
		fresh[i] = types.NewTypeParam(types.NewTypeName(token.NoPos, pkg, param.Obj().Name(), nil), types.NewInterfaceType(nil, nil).Complete())
		mapping[param] = fresh[i]
	}

	origin = types.NewNamed(types.NewTypeName(token.NoPos, pkg, name, nil), nil, nil)
	origin.SetTypeParams(fresh)

	underlying, _ := substituteTypeParams(iface, mapping).(*types.Interface)

	if underlying == nil || underlying == iface {
		underlying = iface
	}

	origin.SetUnderlying(underlying)

	if resolved {
		inlineConstraintOriginsLock.Lock()
		inlineConstraintOrigins[iface] = origin
		inlineConstraintOriginsLock.Unlock()
	}

	return origin, resolved
}

// effectiveConstraint is typeParam's constraint as the instantiation machinery reads it: an inline
// constraint's named view (inlineConstraintNamed) where it has one, the declared constraint otherwise.
// resolved is false only for an inline constraint whose lift is not named yet.
func (v *Visitor) effectiveConstraint(typeParam *types.TypeParam) (types.Type, bool) {
	if named, resolved, ok := v.inlineConstraintNamed(typeParam); ok {
		return named, resolved
	}

	return typeParam.Constraint(), true
}

// The records an instantiation could not make because the inline constraint's lift was not named yet.
// Replayed in order after the file-visit barrier, when every lift is registered; a record is a set member,
// so replay order does not reach the emission.
var (
	deferredConstraintRecords     []func()
	deferredConstraintRecordsLock sync.Mutex
	replayingConstraintRecords    bool
)

// deferConstraintRecord queues record for the barrier. During the replay itself a still-unresolved lift is
// one no declaration of this package names, and is reported rather than queued again.
func (v *Visitor) deferConstraintRecord(typeParam *types.TypeParam, record func()) {
	deferredConstraintRecordsLock.Lock()
	defer deferredConstraintRecordsLock.Unlock()

	if replayingConstraintRecords {
		v.showWarning("@deferConstraintRecord - the inline constraint of `%s` has no lift after the file-visit barrier; its implementation record is not written (emission may not compile)", typeParam.Obj().Name())
		return
	}

	deferredConstraintRecords = append(deferredConstraintRecords, record)
}

// replayDeferredConstraintRecords runs the deferred records. Every driver calls it right after
// resolveDynamicTypeMarkers, before the package's metadata file is written.
func replayDeferredConstraintRecords() {
	deferredConstraintRecordsLock.Lock()
	records := deferredConstraintRecords
	deferredConstraintRecords = nil
	replayingConstraintRecords = true
	deferredConstraintRecordsLock.Unlock()

	for _, record := range records {
		record()
	}

	deferredConstraintRecordsLock.Lock()
	replayingConstraintRecords = false
	deferredConstraintRecordsLock.Unlock()
}

// The capturing inline-constraint lifts this package publishes, by declaration (owner, index) → lift name,
// and those read from imported packages, by package path. Written into the package's metadata as
// `[assembly: GoInlineConstraintLift("Ord", 0, "Ord_T")]` (packageInfoWriter).
var (
	publishedInlineConstraintLifts = map[inlineConstraintDecl]string{}
	importedInlineConstraintLifts  = map[string]map[inlineConstraintDecl]string{}
	inlineConstraintLiftsLock      sync.Mutex
)

// inlineConstraintDecl names a type parameter by its declaration: the Go name of the generic type or
// function declaring it, and its position in that declaration's type parameter list.
type inlineConstraintDecl struct {
	owner string
	index int
}

// publishInlineConstraintLift records the lift of goOwner's index'th type parameter's capturing inline
// constraint for this package's metadata (liftTypeParamConstraints).
func publishInlineConstraintLift(goOwner string, index int, liftName string) {
	if idx := strings.IndexByte(liftName, '<'); idx >= 0 {
		liftName = liftName[:idx]
	}

	inlineConstraintLiftsLock.Lock()
	publishedInlineConstraintLifts[inlineConstraintDecl{goOwner, index}] = liftName
	inlineConstraintLiftsLock.Unlock()
}

// publishedInlineConstraintLiftRecords renders this package's published lifts as metadata records.
func publishedInlineConstraintLiftRecords() []string {
	inlineConstraintLiftsLock.Lock()
	defer inlineConstraintLiftsLock.Unlock()

	records := make([]string, 0, len(publishedInlineConstraintLifts))

	for decl, liftName := range publishedInlineConstraintLifts {
		records = append(records, fmt.Sprintf("[assembly: GoInlineConstraintLift(\"%s\", %d, \"%s\")]", decl.owner, decl.index, liftName))
	}

	return records
}

var inlineConstraintLiftPattern = regexp.MustCompile(`^\[assembly: GoInlineConstraintLift\("([^"]+)", ([0-9]+), "([^"]+)"\)\]$`)

// importedInlineConstraintLift returns the lift name the package declaring typeParam published for its inline
// constraint, or "" when it published none (an older conversion, or no metadata to read). The declaration is
// found by the type parameter itself in its package's scope: an instantiation reads the declaration's own.
func (v *Visitor) importedInlineConstraintLift(typeParam *types.TypeParam) string {
	pkg := typeParam.Obj().Pkg()
	decl, found := typeParamDecl(pkg, typeParam)

	if !found {
		return ""
	}

	inlineConstraintLiftsLock.Lock()
	lifts, loaded := importedInlineConstraintLifts[pkg.Path()]
	inlineConstraintLiftsLock.Unlock()

	if !loaded {
		lifts = make(map[inlineConstraintDecl]string)

		if info, ok := getImportPackageInfo([]string{pkg.Path()}, v.options)[pkg.Path()]; ok && info.Err == nil {
			if lines, ok := importPackageInfoLines(info, v.options); ok {
				for _, line := range lines {
					if matches := inlineConstraintLiftPattern.FindStringSubmatch(strings.TrimSpace(line)); matches != nil {
						if index, err := strconv.Atoi(matches[2]); err == nil {
							lifts[inlineConstraintDecl{matches[1], index}] = matches[3]
						}
					}
				}
			}
		}

		inlineConstraintLiftsLock.Lock()
		importedInlineConstraintLifts[pkg.Path()] = lifts
		inlineConstraintLiftsLock.Unlock()
	}

	return lifts[decl]
}

// typeParamDecl finds the package-level generic type or function declaring typeParam.
func typeParamDecl(pkg *types.Package, typeParam *types.TypeParam) (inlineConstraintDecl, bool) {
	if pkg == nil {
		return inlineConstraintDecl{}, false
	}

	scope := pkg.Scope()

	for _, name := range scope.Names() {
		var params *types.TypeParamList

		switch obj := scope.Lookup(name).(type) {
		case *types.TypeName:
			if named, ok := obj.Type().(*types.Named); ok {
				params = named.TypeParams()
			}
		case *types.Func:
			if signature, ok := obj.Type().(*types.Signature); ok {
				params = signature.TypeParams()
			}
		}

		for i := range params.Len() {
			if params.At(i) == typeParam {
				return inlineConstraintDecl{name, i}, true
			}
		}
	}

	return inlineConstraintDecl{}, false
}

// resetInlineConstraintState clears the per-package state above (resetPackageState).
func resetInlineConstraintState() {
	inlineConstraintOriginsLock.Lock()
	inlineConstraintOrigins = map[*types.Interface]*types.Named{}
	inlineConstraintOriginsLock.Unlock()

	inlineConstraintLiftsLock.Lock()
	publishedInlineConstraintLifts = map[inlineConstraintDecl]string{}
	importedInlineConstraintLifts = map[string]map[inlineConstraintDecl]string{}
	inlineConstraintLiftsLock.Unlock()

	deferredConstraintRecordsLock.Lock()
	deferredConstraintRecords = nil
	replayingConstraintRecords = false
	deferredConstraintRecordsLock.Unlock()
}
