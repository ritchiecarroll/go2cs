// inlineArrayAdmission.go - Gbtc
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
	"path/filepath"
	"sort"
	"strings"
)

// INLINE-ARRAY ADMISSION (I0; docs/phase4/DESIGN-array-value-storage.md, 7.7 and 7.7.6)
//
// Which struct types of a package hold their Go arrays INLINE (the go.arrays family) rather than as golib's array<T>
// reference, so a struct read from native memory, or reinterpreted through unsafe.Pointer, is Go's bytes. Ruled by COORD
// on 2026-11-13 (04:00Z, on C1's census in 7.7.6), on four conditions:
//
//  1. CLOSURE. A struct is admitted when its declaring package's NON-TEST files convert it whole through unsafe.Pointer
//     ((*X)(p) with p an unsafe.Pointer, or unsafe.Pointer(e) with e a *X), together with every struct type it holds BY
//     VALUE (fields and array elements, at any depth), plain nested structs included: the layout gate needs a managed
//     layout table at every level (C1's review, N4). Every type in the closure must pass 7.7's declaration-local rules:
//     unmanaged once its arrays are inline, no array over 4096 bytes or 4096 elements, no struct over 4096 bytes.
//  2. PAIR RULE. A reinterpret chain (*T)(unsafe.Pointer(e)), e a *X, whose two sides both hold arrays and disagree on
//     admission is a convert-time ERROR naming both types (GO2CS2010), under either trigger.
//  3. STAMP. The decision is recorded in package_info.cs, and a -tests conversion reads it (inlineArrayStamp.go).
//  4. OVERRIDE. -inline-arrays=<patterns> forces a package ON with the declaration-local population (every struct the
//     package declares or spells that holds an array), and "-<pattern>" turns auto OFF. std stays out either way.
//
// A type the rule SELECTS (converted whole, or forced) that a cap keeps out stays exactly as today and is reported by a
// WARNING naming the type, the field and the cap (GO2CS2009; COORD 2026-11-13 05:20Z). So is a type whose closure
// reaches a struct declared in another package, which this package's emission cannot lay out, and, under the automatic
// rule, one whose array fields the package uses through a face an inline array cannot spell (inlineArrayFaces.go;
// COORD 2026-11-13 06:50Z).

const (
	inlineArrayMaxBytes    = 4096 // per array (7.7.1, ruling 3)
	inlineArrayMaxElements = 4096 // the family's longest member
	inlineStructMaxBytes   = 4096 // per struct, at every level
)

// inlineArrayAdmission is one package's decision. A nil *inlineArrayAdmission admits nothing.
type inlineArrayAdmission struct {
	pkgPath  string
	forced   bool
	admitted map[string]bool       // inlineTypeKey -> admitted
	keyOrder []string              // admitted keys, sorted
	holding  int                   // admitted types that hold an array at any depth
	holds    map[string]bool       // admitted key -> holds an array at any depth
	types    map[string]types.Type // admitted key -> its type
	blocked  map[string]string     // a selected seed kept out -> why
	foreign  int                   // reinterpret pairs reaching a struct of another package whose record cannot be read: counted, not judged
	pkg      *types.Package
	spelled  map[string]bool
	members  map[string][]string // a selected seed -> its closure's keys
	warnings []inlineAdmissionDiagnostic
	refusals []inlineAdmissionDiagnostic

	options       Options                      // where an imported package's record is read from
	foreignStamps map[string]*inlineArrayStamp // import path -> its record (nil: unreadable)
}

// inlineAdmissionDiagnostic is one GO2CS2009 warning or GO2CS2010 refusal, at its source position.
type inlineAdmissionDiagnostic struct {
	position token.Position
	text     string
}

// packageInlineAdmission is the package being converted's decision; resetPackageState clears it.
var packageInlineAdmission *inlineArrayAdmission

// admits reports whether t's struct type is held with inline arrays.
func (a *inlineArrayAdmission) admits(t types.Type) bool {
	if a == nil || len(a.admitted) == 0 {
		return false
	}

	key, ok := inlineTypeKey(t)
	return ok && a.admitted[key]
}

// typeOf returns the struct type an admitted key names.
func (a *inlineArrayAdmission) typeOf(key string) (*types.Struct, bool) {
	if a == nil || !a.admitted[key] {
		return nil, false
	}

	st, ok := a.types[key].Underlying().(*types.Struct)
	return st, ok
}

// inlineTypeKey keys a struct type: a named type by its package path and name (and type arguments), an anonymous struct
// type by its type string, which is how go2cs lifts it into the package that spells it.
func inlineTypeKey(t types.Type) (string, bool) {
	t = types.Unalias(t)

	if named, ok := t.(*types.Named); ok {
		if _, isStruct := named.Underlying().(*types.Struct); !isStruct || named.Obj().Pkg() == nil {
			return "", false
		}

		if named.TypeArgs().Len() > 0 {
			return types.TypeString(named, nil), true
		}

		return named.Obj().Pkg().Path() + "." + named.Obj().Name(), true
	}

	if _, ok := t.(*types.Struct); ok {
		return "anon:" + types.TypeString(t, nil), true
	}

	return "", false
}

// inlineArrayOverride is the parsed -inline-arrays value.
type inlineArrayOverride struct {
	force []string // patterns forced ON
	off   []string // patterns with auto OFF ("-<pattern>")
}

// parseInlineArrayOverride parses -inline-arrays: a comma-separated list of import-path patterns, each an exact path or
// a "/..." prefix, a leading "-" turning auto off for the pattern instead of forcing it on.
func parseInlineArrayOverride(value string) (inlineArrayOverride, error) {
	var override inlineArrayOverride

	for _, entry := range strings.Split(value, ",") {
		entry = strings.TrimSpace(entry)

		if entry == "" {
			continue
		}

		off := strings.HasPrefix(entry, "-")
		pattern := strings.TrimPrefix(entry, "-")

		if pattern == "" || strings.ContainsAny(pattern, " \t") {
			return inlineArrayOverride{}, fmt.Errorf("-inline-arrays: %q is not an import-path pattern", entry)
		}

		if off {
			override.off = append(override.off, pattern)
		} else {
			override.force = append(override.force, pattern)
		}
	}

	return override, nil
}

// matchesImportPattern reports whether importPath matches pattern: equal, or under it for a "/..." pattern.
func matchesImportPattern(pattern, importPath string) bool {
	if prefix, ok := strings.CutSuffix(pattern, "/..."); ok {
		return importPath == prefix || strings.HasPrefix(importPath, prefix+"/")
	}

	return pattern == importPath
}

func (o inlineArrayOverride) matches(patterns []string, importPath string) bool {
	for _, pattern := range patterns {
		if matchesImportPattern(pattern, importPath) {
			return true
		}
	}

	return false
}

// inlineArraysExcluded reports the packages I0 never admits: std (a -stdlib run, or a package under GOROOT/src).
func inlineArraysExcluded(pkgDir string, options Options) bool {
	if options.convertStdLib {
		return true
	}

	return pkgDir != "" && options.goRoot != "" && isPathUnder(pkgDir, filepath.Join(options.goRoot, "src"))
}

// computeInlineArrayAdmission decides the package's admitted set from its NON-TEST files. It reports nothing; the caller
// reports the warnings and refuses on a mixed pair (reportInlineArrayAdmission).
func computeInlineArrayAdmission(files []*ast.File, fset *token.FileSet, pkg *types.Package, info *types.Info, pkgDir string, options Options) *inlineArrayAdmission {
	if pkg == nil || info == nil || inlineArraysExcluded(pkgDir, options) || options.inlineArrays.matches(options.inlineArrays.off, pkg.Path()) {
		return nil
	}

	a := &inlineArrayAdmission{pkgPath: pkg.Path(), forced: options.inlineArrays.matches(options.inlineArrays.force, pkg.Path()), admitted: map[string]bool{}, holds: map[string]bool{}, blocked: map[string]string{}, members: map[string][]string{}, options: options}
	sizes := types.SizesFor("gc", "amd64")

	if parts := strings.Split(options.targetPlatform, "/"); len(parts) == 2 {
		if s := types.SizesFor("gc", parts[1]); s != nil {
			sizes = s
		}
	}

	// The SELECTED types: each a seed whose by-value closure must be inline, with the position that selected it.
	type selection struct {
		t   types.Type
		pos token.Pos
	}

	var seeds []selection
	var pairs []inlineReinterpretPair

	// Every anonymous struct type the package's own non-test files spell, by type string.
	spelled := map[string]bool{}

	// The first use of each struct's array fields that an inline array cannot spell (inlineArrayFaces.go).
	faces := map[string]string{}

	for _, file := range files {
		if strings.HasSuffix(strings.ToLower(fset.Position(file.Pos()).Filename), "_test.go") {
			continue
		}

		collectUnspelledFaces(file, fset, info, faces)

		// A struct type expression that is a type spec's own body is the named type, counted once by its name.
		specBodies := map[ast.Expr]bool{}

		ast.Inspect(file, func(n ast.Node) bool {
			if spec, ok := n.(*ast.TypeSpec); ok {
				specBodies[spec.Type] = true
			}

			if structType, ok := n.(*ast.StructType); ok {
				if st, ok := info.TypeOf(structType).(*types.Struct); ok {
					spelled[types.TypeString(st, nil)] = true
				}
			}

			return true
		})

		ast.Inspect(file, func(n ast.Node) bool {
			switch node := n.(type) {
			case *ast.StructType:
				if a.forced && !specBodies[node] {
					if st, ok := info.TypeOf(node).(*types.Struct); ok && structHoldsArray(st, 0) {
						seeds = append(seeds, selection{st, node.Pos()})
					}
				}
			case *ast.TypeSpec:
				if a.forced {
					if obj, ok := info.Defs[node.Name].(*types.TypeName); ok && obj.Pkg() == pkg {
						if st, ok := obj.Type().Underlying().(*types.Struct); ok && structHoldsArray(st, 0) && node.TypeParams == nil {
							seeds = append(seeds, selection{obj.Type(), node.Pos()})
						}
					}
				}
			case *ast.CallExpr:
				target, pair, ok := unsafePointerConversion(node, info)

				if ok && !holdsTypeParameter(target, 0) {
					seeds = append(seeds, selection{target, node.Pos()})
				}

				if pair != nil {
					pairs = append(pairs, *pair)
				}
			}

			return true
		})
	}

	// Every struct type in each seed's closure, and whether the closure is admissible as a whole.
	candidates := map[string]types.Type{}
	blockedBy := map[string]string{} // key -> why it stays out

	for _, seed := range seeds {
		var closure []types.Type
		byValueStructs(seed.t, 0, map[string]bool{}, func(t types.Type) { closure = append(closure, t) })

		// A seed may be an array of structs (unsafe.Pointer(&a), a [N]S): its closure is its elements' structs.
		seedKey, ok := inlineTypeKey(seed.t)

		if !ok {
			seedKey = "value:" + types.TypeString(seed.t, nil)
		}

		if len(closure) == 0 {
			continue
		}

		// A seed declared in another package is that package's decision: its own conversion of it admits it there.
		if !inlineTypeIsLocal(closure[0], pkg, spelled) {
			continue
		}

		// A closure holding no array needs no inline storage: the struct is laid out as Go's already.
		holdsAny := false

		for _, member := range closure {
			if st, isStruct := member.Underlying().(*types.Struct); isStruct && structHoldsArray(st, 0) {
				holdsAny = true
				break
			}
		}

		if !holdsAny {
			continue
		}

		reason := ""

		for _, member := range closure {
			key, _ := inlineTypeKey(member)
			candidates[key] = member
			a.members[seedKey] = append(a.members[seedKey], key)

			if why := inlineDeclarationRule(member, pkg, spelled, sizes); why != "" && reason == "" {
				reason = why
			}

			// Under the automatic rule a face an inline array cannot spell keeps the seed out (forced: a compile error).
			if face, found := faces[key]; found && reason == "" && !a.forced {
				reason = face
			}
		}

		if reason != "" {
			if _, seen := blockedBy[seedKey]; !seen {
				blockedBy[seedKey] = reason
				a.blocked[seedKey] = reason

				// GO2CS2009 is reserved for a CAP (or a foreign closure); a managed type is simply not a candidate.
				if !strings.HasPrefix(reason, "managed") {
					a.warnings = append(a.warnings, inlineAdmissionDiagnostic{fset.Position(seed.pos),
						fmt.Sprintf("%s is converted whole through unsafe.Pointer but keeps array<T> storage: %s", types.TypeString(seed.t, packageQualifier(pkg)), reason)})
				}
			}

			continue
		}

		for _, member := range closure {
			key, _ := inlineTypeKey(member)
			a.admitted[key] = true
		}
	}

	// A type admitted through one seed but blocked as another's root stays admitted only if its own closure is clean,
	// which inlineDeclarationRule already proved for every member above; nothing more to reconcile.
	for key := range a.admitted {
		a.keyOrder = append(a.keyOrder, key)

		if st, ok := candidates[key].Underlying().(*types.Struct); ok && structHoldsArray(st, 0) {
			a.holding++
			a.holds[key] = true
		}
	}

	sort.Strings(a.keyOrder)
	a.types = candidates

	// The pair rule (GO2CS2010): both sides hold arrays and disagree. A struct another package lays out is that package's
	// decision, read from its record (inlineArrayStamp.go); a pair reaching one whose record cannot be read, or an
	// anonymous struct another package spells, is counted, not judged.
	a.pkg, a.spelled = pkg, spelled

	for _, pair := range pairs {
		dstHolds, dstIn, dstForeign := a.closureAdmitted(pair.dst)
		srcHolds, srcIn, srcForeign := a.closureAdmitted(pair.src)

		if dstForeign || srcForeign {
			if dstHolds && srcHolds {
				a.foreign++
			}

			continue
		}

		if dstHolds && srcHolds && dstIn != srcIn {
			admittedSide, otherSide := pair.dst, pair.src

			if srcIn {
				admittedSide, otherSide = pair.src, pair.dst
			}

			a.refusals = append(a.refusals, inlineAdmissionDiagnostic{fset.Position(pair.pos),
				fmt.Sprintf("(*%s)(unsafe.Pointer(*%s)) reinterprets %s, whose arrays are held inline, as %s, whose arrays are not; the two layouts differ",
					types.TypeString(pair.dst, packageQualifier(pkg)), types.TypeString(pair.src, packageQualifier(pkg)),
					types.TypeString(admittedSide, packageQualifier(pkg)), types.TypeString(otherSide, packageQualifier(pkg)))})
		}
	}

	return a
}

// closureAdmitted reports whether t's by-value closure holds an array, whether every array-holding struct in it is
// admitted (the census's admittedSet), and whether it reaches a struct of another package whose admission is unknown.
func (a *inlineArrayAdmission) closureAdmitted(t types.Type) (holds, admitted, foreign bool) {
	admitted = true

	byValueStructs(t, 0, map[string]bool{}, func(member types.Type) {
		if st, ok := member.Underlying().(*types.Struct); ok && structHoldsArray(st, 0) {
			holds = true

			if !inlineTypeIsLocal(member, a.pkg, a.spelled) {
				if in, known := a.foreignAdmits(member); !known {
					foreign = true
				} else if !in {
					admitted = false
				}

				return
			}

			if !a.admits(member) {
				admitted = false
			}
		}
	})

	return holds, admitted, foreign
}

// inlineReinterpretPair is a chain (*T)(unsafe.Pointer(e)), e a *X: T and X must agree on layout.
type inlineReinterpretPair struct {
	dst, src types.Type
	pos      token.Pos
}

// unsafePointerConversion recognises a whole-struct conversion through unsafe.Pointer and returns its struct target, and
// the reinterpret pair when the call is (*T)(unsafe.Pointer(e)).
func unsafePointerConversion(call *ast.CallExpr, info *types.Info) (types.Type, *inlineReinterpretPair, bool) {
	if len(call.Args) != 1 {
		return nil, nil, false
	}

	funTV, ok := info.Types[call.Fun]

	if !ok || !funTV.IsType() {
		return nil, nil, false
	}

	argType := info.TypeOf(ast.Unparen(call.Args[0]))

	if argType == nil {
		return nil, nil, false
	}

	if isUnsafePointerType(funTV.Type) {
		// unsafe.Pointer(e), e a *X.
		if pointer, ok := argType.Underlying().(*types.Pointer); ok {
			return pointer.Elem(), nil, true
		}

		return nil, nil, false
	}

	if !isUnsafePointerType(argType) {
		return nil, nil, false
	}

	pointer, ok := funTV.Type.Underlying().(*types.Pointer)

	if !ok {
		return nil, nil, false
	}

	// (*T)(p): and when p is itself unsafe.Pointer(e), e a *X, the pair (T, X).
	var pair *inlineReinterpretPair

	if inner, ok := ast.Unparen(call.Args[0]).(*ast.CallExpr); ok && len(inner.Args) == 1 {
		if innerTV, ok := info.Types[inner.Fun]; ok && innerTV.IsType() && isUnsafePointerType(innerTV.Type) {
			if source := info.TypeOf(ast.Unparen(inner.Args[0])); source != nil {
				if sourcePointer, ok := source.Underlying().(*types.Pointer); ok {
					pair = &inlineReinterpretPair{dst: pointer.Elem(), src: sourcePointer.Elem(), pos: call.Pos()}
				}
			}
		}
	}

	return pointer.Elem(), pair, true
}

func isUnsafePointerType(t types.Type) bool {
	basic, ok := types.Unalias(t).(*types.Basic)
	return ok && basic.Kind() == types.UnsafePointer
}

// holdsTypeParameter: a conversion whose target holds a type parameter is chosen by the instantiation, out of sight.
func holdsTypeParameter(t types.Type, depth int) bool {
	if depth > 10 {
		return false
	}

	switch u := types.Unalias(t).(type) {
	case *types.TypeParam:
		return true
	case *types.Pointer:
		return holdsTypeParameter(u.Elem(), depth+1)
	case *types.Array:
		return holdsTypeParameter(u.Elem(), depth+1)
	case *types.Named:
		for i := range u.TypeArgs().Len() {
			if holdsTypeParameter(u.TypeArgs().At(i), depth+1) {
				return true
			}
		}

		return holdsTypeParameter(u.Underlying(), depth+1)
	case *types.Struct:
		for i := range u.NumFields() {
			if holdsTypeParameter(u.Field(i).Type(), depth+1) {
				return true
			}
		}
	}

	return false
}

// structHoldsArray reports whether a struct holds an array at any depth by value.
func structHoldsArray(st *types.Struct, depth int) bool {
	if depth > 10 {
		return false
	}

	for i := range st.NumFields() {
		switch u := st.Field(i).Type().Underlying().(type) {
		case *types.Array:
			return true
		case *types.Struct:
			if structHoldsArray(u, depth+1) {
				return true
			}
		}
	}

	return false
}

// byValueStructs calls visit once for each struct type reachable from t BY VALUE (t itself, struct fields, array
// elements, at any depth). A type that holds no array and no struct still counts: the gate needs its table too.
func byValueStructs(t types.Type, depth int, seen map[string]bool, visit func(types.Type)) {
	if depth > 10 {
		return
	}

	switch u := types.Unalias(t).Underlying().(type) {
	case *types.Array:
		byValueStructs(u.Elem(), depth+1, seen, visit)
	case *types.Struct:
		key, ok := inlineTypeKey(t)

		if !ok || seen[key] {
			return
		}

		seen[key] = true
		visit(types.Unalias(t))

		for i := range u.NumFields() {
			byValueStructs(u.Field(i).Type(), depth+1, seen, visit)
		}
	}
}

// inlineTypeIsLocal reports whether this package's emission lays the struct type out: a named type it declares, or an
// anonymous struct type its own files spell (go2cs lifts it there). An anonymous struct reached through an alias another
// package declares (ccgo writes its types as `type T = struct{...}`) is that package's.
func inlineTypeIsLocal(t types.Type, pkg *types.Package, spelled map[string]bool) bool {
	t = types.Unalias(t)

	if named, ok := t.(*types.Named); ok {
		return named.Obj().Pkg() == pkg
	}

	return spelled[types.TypeString(t, nil)]
}

// inlineDeclarationRule is 7.7's declaration-local rule for one struct type of a closure, or "" when it passes: declared
// in this package (or spelled anonymously, which go2cs lifts into it), unmanaged once its arrays are inline, and within
// the caps. The reason names the field and the cap.
func inlineDeclarationRule(t types.Type, pkg *types.Package, spelled map[string]bool, sizes types.Sizes) string {
	t = types.Unalias(t)

	if !inlineTypeIsLocal(t, pkg, spelled) {
		return fmt.Sprintf("its by-value closure holds %s, declared in another package", types.TypeString(t, nil))
	}

	// go2cs declares ONE C# struct for a generic Go struct, shared by every instantiation and never inline, so an
	// instantiation cannot be admitted alone (C1's review, m2): its copies would lose their ΔClone over array<T> storage.
	if named, isNamed := t.(*types.Named); isNamed && named.TypeArgs().Len() > 0 {
		return fmt.Sprintf("%s is an instantiation of a generic struct, which go2cs declares once for every instantiation", types.TypeString(t, packageQualifier(pkg)))
	}

	st, ok := t.Underlying().(*types.Struct)

	if !ok {
		return "managed: not a struct"
	}

	for i := range st.NumFields() {
		field := st.Field(i)

		if !inlineUnmanaged(field.Type(), 0) {
			return fmt.Sprintf("managed: field %s is %s", field.Name(), types.TypeString(field.Type(), packageQualifier(pkg)))
		}

		if why := inlineArrayCaps(field.Type(), sizes, 0); why != "" {
			return fmt.Sprintf("field %s: %s", field.Name(), why)
		}
	}

	// The struct cap last, so an over-cap array is named by its field.
	if size := sizes.Sizeof(t); size > inlineStructMaxBytes {
		return fmt.Sprintf("%s is %d bytes, over the %d-byte struct cap", types.TypeString(t, packageQualifier(pkg)), size, inlineStructMaxBytes)
	}

	return ""
}

// inlineUnmanaged reports whether a Go type is unmanaged once its arrays are inline: numbers and bools, and arrays and
// structs of them. A string, pointer, unsafe.Pointer, slice, map, chan, func or interface is managed.
func inlineUnmanaged(t types.Type, depth int) bool {
	if depth > 10 {
		return false
	}

	switch u := t.Underlying().(type) {
	case *types.Basic:
		return u.Kind() != types.String && u.Kind() != types.UnsafePointer && u.Kind() != types.UntypedNil
	case *types.Array:
		return inlineUnmanaged(u.Elem(), depth+1)
	case *types.Struct:
		for i := range u.NumFields() {
			if !inlineUnmanaged(u.Field(i).Type(), depth+1) {
				return false
			}
		}

		return true
	}

	return false
}

// inlineArrayCaps names the first array, at any depth of a field's type, over a cap.
func inlineArrayCaps(t types.Type, sizes types.Sizes, depth int) string {
	if depth > 10 {
		return ""
	}

	if array, ok := t.Underlying().(*types.Array); ok {
		if array.Len() > inlineArrayMaxElements {
			return fmt.Sprintf("%s has %d elements, over the %d-element cap", types.TypeString(t, nil), array.Len(), inlineArrayMaxElements)
		}

		if size := sizes.Sizeof(array); size > inlineArrayMaxBytes {
			return fmt.Sprintf("%s is %d bytes, over the %d-byte array cap", types.TypeString(t, nil), size, inlineArrayMaxBytes)
		}

		return inlineArrayCaps(array.Elem(), sizes, depth+1)
	}

	return ""
}

// packageQualifier writes this package's names bare and others' by package name.
func packageQualifier(pkg *types.Package) types.Qualifier {
	return func(other *types.Package) string {
		if other == pkg {
			return ""
		}

		return other.Name()
	}
}

// reportInlineArrayAdmission prints the census line, each GO2CS2009 warning, and each GO2CS2010 refusal; it returns the
// refusal error when a mixed pair was found (the package is refused, as every pre-emission gate refuses).
func reportInlineArrayAdmission(a *inlineArrayAdmission) error {
	if a == nil {
		return nil
	}

	for _, warning := range a.warnings {
		reportWarningDiagnostic(warning.position.String(), diagnosticInlineArrayCapped, warning.text)
	}

	if len(a.keyOrder) > 0 || len(a.warnings) > 0 || len(a.refusals) > 0 {
		trigger := "auto"

		if a.forced {
			trigger = "forced"
		}

		fmt.Printf("Inline arrays (%s): %s admits %d struct %s (%d holding arrays), %d kept out (GO2CS2009), %d mixed %s (GO2CS2010)\n",
			trigger, a.pkgPath, len(a.keyOrder), plural(len(a.keyOrder), "type", "types"), a.holding, len(a.warnings), len(a.refusals), plural(len(a.refusals), "pair", "pairs"))
	}

	if len(a.refusals) == 0 {
		return nil
	}

	for _, refusal := range a.refusals {
		reportDiagnostic(refusal.position.String(), diagnosticInlineArrayMixedPair, refusal.text)
	}

	return fmt.Errorf("package %s: %d reinterpret %s between a struct whose arrays are held inline and one whose arrays are not (%s)",
		a.pkgPath, len(a.refusals), plural(len(a.refusals), "chain", "chains"), diagnosticInlineArrayMixedPair)
}
