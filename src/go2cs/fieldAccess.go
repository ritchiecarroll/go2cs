// fieldAccess.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	_ "embed"
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"slices"
	"strings"

	"golang.org/x/tools/go/packages"
)

// FIELD ACCESS THROUGH A REINTERPRETED POINTER (COORD rulings 2026-10-16: G's rule, the owner's
// uniform scope, G's shape)
//
// Go reaches a struct laid over raw bytes through a pointer it reinterprets from unsafe.Pointer:
//
//	buf := (*windows.MountPointReparseBuffer)(unsafe.Pointer(&byteblob[0]))
//	buf.SubstituteNameOffset = target.substituteName.offset
//
// When the struct holds a managed reference its CLR layout is not Go's, so a box over native or
// pinned bytes refuses the whole-value read `buf.Value` by name, and the field's CLR slot is not
// where Go put it anyway. The site therefore reads through a field PROPERTY that go2cs-gen emits on
// the box, a ref over golib's `ж<T>.field`, which answers the field at its Go offset in native
// memory and at the struct's own slot in managed memory:
//
//	buf.SubstituteNameOffset = target.substituteName.offset;
//
// THE RULE (G, 22:45Z): a field access p.f is served this way when p is a LOCAL whose every
// assignment in its function is `(*T)(x)` with x an unsafe.Pointer, and T holds a managed reference
// (csManagedField). A bare `var p *T` is the nil zero value and counts neither way; `&p`, a range
// variable or a multi-value assignment disqualifies. Everything else keeps its emission.
//
// ONE STEP ACROSS A RETURN (COORD rulings 2026-10-24, G's amendments, the tight cut): an assignment
// from a call also qualifies, single- or multi-value (the local at Lhs[i] takes result i), when the
// callee is a static, non-generic function or concrete method of the same package with UNNAMED results,
// and every return in its body gives that position nil or NATIVE BYTES: `(*T)(unsafe.Pointer(&b[i]))`
// with b a byte array or byte slice, or a callee local whose every assignment is that form -- os's test
// helper readSymlinkReparseData, called as `osRDB, err := readSymlinkReparseData(…)`. The step carries
// only the native-bytes provenance because that is the one class where a box over a CLR!=Go-layout T
// refuses its whole-value read; a callee reinterpreting any other pointer (runtime's *uintptr.ptr()
// getters, efaceOf, sync's indexLocal, reflect's toRType: about 320 production sites the as-first-ruled
// predicate admitted) keeps today's emission. A return of another call disqualifies: there is no
// transitive summary. Named results are excluded rather than modeled, since a bare return or a deferred
// write reaches them. The provenance now crosses a frame, which is sound because the box owns its pin:
// ж.ElemRefBox.TryPinnedReinterpret pins the backing and hands that PinnedBuffer to the NativeBox it
// returns, with retainedSource: this, so a returned box keeps the byte storage still and alive.
//
// Each site's struct type is listed in its FILE's `[GoFieldAccess(typeof(T), …)]` record, on the
// package class, and go2cs-gen emits one extension block per listed type. A file carries its own
// record because the files of a package do not all compile together: a test assembly recompiles
// the production files beside its own, so a per-file record follows its sites into every assembly.
//
// These shapes keep a box form, because the property cannot serve them:
//   - a field named like a member of ж<T> (fieldAccessCollisions.txt): an instance member binds
//     before an extension one, so `p.of(T.Ꮡf).Value` names the field explicitly; likewise a field
//     that shadows a method an embedded type promotes (shadowsPromotedMethod), which C# would bind
//     as the method group;
//   - an ELEMENT of an array field, `p.arr[i]`: `p.of(T.Ꮡarr).ElementRef(i)`, since an array<T> has
//     no native form for the property to return, and the field view does;
//   - a field PROMOTED through a pointer embed, which lives in another allocation: it keeps today's
//     emission. A field promoted through value embeds walks them by name, `p.E.f`;
//   - a field of a struct a HAND-OWNED file declares (packageHandOwnedTypes): such a struct has no
//     package_info record to make it internal, so it is a private type nested in the package class,
//     which the generated extension class cannot name. A direct field takes `p.of(T.Ꮡf).Value`; a
//     promoted one keeps today's emission.

//go:embed fieldAccessCollisions.txt
var fieldAccessCollisionsText string

// fieldAccessCollisions holds the names a generated field property never takes (see the file's header).
var fieldAccessCollisions = parseFieldAccessCollisions(fieldAccessCollisionsText)

func parseFieldAccessCollisions(text string) map[string]bool {
	names := map[string]bool{}

	for _, line := range strings.Split(text, "\n") {
		if comment := strings.IndexByte(line, '#'); comment >= 0 {
			line = line[:comment]
		}

		if name := strings.TrimSpace(line); name != "" {
			names[name] = true
		}
	}

	return names
}

// fieldAccessLocal answers the pointee struct type of x when x is a local the provenance rule admits,
// else nil.
func (v *Visitor) fieldAccessLocal(x ast.Expr) types.Type {
	ident, ok := ast.Unparen(x).(*ast.Ident)

	if !ok || v.currentFuncDecl == nil || v.currentFuncDecl.Body == nil {
		return nil
	}

	obj, ok := v.info.Uses[ident].(*types.Var)

	if !ok || obj.IsField() {
		return nil
	}

	if pointee, found := v.fieldAccessLocalCache[obj]; found {
		return pointee
	}

	pointee := v.fieldAccessLocalPointee(obj, v.currentFuncDecl.Body, false)

	if v.fieldAccessLocalCache == nil {
		v.fieldAccessLocalCache = make(map[types.Object]types.Type)
	}

	v.fieldAccessLocalCache[obj] = pointee
	return pointee
}

// fieldAccessLocalPointee applies the provenance rule to obj over body, its function's body. A caller's local
// (callee false) takes the one step across a return; a callee's own local (callee true) takes no step and admits
// only the native-bytes reinterpret.
func (v *Visitor) fieldAccessLocalPointee(obj *types.Var, body *ast.BlockStmt, callee bool) types.Type {
	ptr, ok := types.Unalias(obj.Type()).(*types.Pointer)

	if !ok {
		return nil
	}

	if _, ok := ptr.Elem().Underlying().(*types.Struct); !ok || !csManagedField(ptr.Elem(), 0) {
		return nil
	}

	defined, assigns, disqualified := false, 0, false

	assign := func(value ast.Expr) {
		assigns++

		if callee && !isNativeBytesReinterpret(v.info, value) ||
			!callee && !isUnsafePointerReinterpret(v.info, value) && !v.fieldAccessCallResult(value, 0, obj) {
			disqualified = true
		}
	}

	// A multi-value assignment from one call: the local takes the callee's result index.
	assignResult := func(values []ast.Expr, index int) {
		assigns++

		if len(values) != 1 || callee || !v.fieldAccessCallResult(values[0], index, obj) {
			disqualified = true
		}
	}

	is := func(expr ast.Expr) bool {
		ident, ok := ast.Unparen(expr).(*ast.Ident)
		return ok && (v.info.Defs[ident] == obj || v.info.Uses[ident] == obj)
	}

	ast.Inspect(body, func(n ast.Node) bool {
		switch node := n.(type) {
		case *ast.ValueSpec:
			for i, name := range node.Names {
				if v.info.Defs[name] != obj {
					continue
				}

				defined = true

				switch {
				case len(node.Values) == 0:
				case len(node.Values) != len(node.Names):
					assignResult(node.Values, i)
				default:
					assign(node.Values[i])
				}
			}
		case *ast.AssignStmt:
			for i, lhs := range node.Lhs {
				if !is(lhs) {
					continue
				}

				if ident := ast.Unparen(lhs).(*ast.Ident); v.info.Defs[ident] == obj {
					defined = true
				}

				if len(node.Rhs) != len(node.Lhs) {
					assignResult(node.Rhs, i)
				} else {
					assign(node.Rhs[i])
				}
			}
		case *ast.RangeStmt:
			if (node.Key != nil && is(node.Key)) || (node.Value != nil && is(node.Value)) {
				disqualified = true
			}
		case *ast.UnaryExpr:
			if node.Op == token.AND && is(node.X) {
				disqualified = true
			}
		}

		return true
	})

	if !defined || assigns == 0 || disqualified {
		return nil
	}

	return ptr.Elem()
}

// isUnsafePointerReinterpret reports whether expr is a conversion `(*T)(x)` with x of type unsafe.Pointer.
func isUnsafePointerReinterpret(info *types.Info, expr ast.Expr) bool {
	call, ok := ast.Unparen(expr).(*ast.CallExpr)

	if !ok || len(call.Args) != 1 {
		return false
	}

	if tv, ok := info.Types[call.Fun]; !ok || !tv.IsType() {
		return false
	} else if _, ok := tv.Type.Underlying().(*types.Pointer); !ok {
		return false
	}

	basic, ok := info.TypeOf(call.Args[0]).(*types.Basic)

	return ok && basic.Kind() == types.UnsafePointer
}

// isNativeBytesReinterpret reports whether expr is `(*T)(unsafe.Pointer(&b[i]))` with b a byte array or byte slice:
// a struct laid over the element address of byte storage.
func isNativeBytesReinterpret(info *types.Info, expr ast.Expr) bool {
	if !isUnsafePointerReinterpret(info, expr) {
		return false
	}

	conversion, ok := ast.Unparen(ast.Unparen(expr).(*ast.CallExpr).Args[0]).(*ast.CallExpr)

	if !ok || len(conversion.Args) != 1 {
		return false
	}

	if tv, ok := info.Types[conversion.Fun]; !ok || !tv.IsType() {
		return false
	}

	address, ok := ast.Unparen(conversion.Args[0]).(*ast.UnaryExpr)

	if !ok || address.Op != token.AND {
		return false
	}

	element, ok := ast.Unparen(address.X).(*ast.IndexExpr)

	if !ok {
		return false
	}

	var elem types.Type

	switch storage := info.TypeOf(element.X).Underlying().(type) {
	case *types.Slice:
		elem = storage.Elem()
	case *types.Array:
		elem = storage.Elem()
	default:
		return false
	}

	basic, ok := elem.Underlying().(*types.Basic)

	return ok && basic.Kind() == types.Uint8
}

// fieldAccessResult names one result position of a callee.
type fieldAccessResult struct {
	fn    *types.Func
	index int
}

// fieldAccessCallResult reports whether value is a call whose result index the one step across a return admits
// for local: a static same-package callee (fieldAccessCallee) whose result there has local's own type and is a
// reinterpret on every return (fieldAccessReturnsReinterpret).
func (v *Visitor) fieldAccessCallResult(value ast.Expr, index int, local *types.Var) bool {
	call, ok := ast.Unparen(value).(*ast.CallExpr)

	if !ok {
		return false
	}

	fn := v.fieldAccessCallee(call, local.Pkg())

	if fn == nil {
		return false
	}

	results := fn.Type().(*types.Signature).Results()

	if index >= results.Len() || !types.Identical(results.At(index).Type(), local.Type()) {
		return false
	}

	key := fieldAccessResult{fn, index}

	if admitted, found := v.fieldAccessResultCache[key]; found {
		return admitted
	}

	admitted := v.fieldAccessReturnsReinterpret(fn, index)

	if v.fieldAccessResultCache == nil {
		v.fieldAccessResultCache = make(map[fieldAccessResult]bool)
	}

	v.fieldAccessResultCache[key] = admitted
	return admitted
}

// fieldAccessCallee answers the function a call statically names when it is a non-generic function or concrete
// method of pkg with unnamed results, else nil: a func value, a method value, an interface method, a generic or
// instantiated callee, a callee of another package and a named-result callee all answer nil.
func (v *Visitor) fieldAccessCallee(call *ast.CallExpr, pkg *types.Package) *types.Func {
	var fn *types.Func

	switch fun := ast.Unparen(call.Fun).(type) {
	case *ast.Ident:
		fn, _ = v.info.Uses[fun].(*types.Func)
	case *ast.SelectorExpr:
		if selection := v.info.Selections[fun]; selection == nil {
			fn, _ = v.info.Uses[fun.Sel].(*types.Func)
		} else if selection.Kind() == types.MethodVal {
			fn, _ = selection.Obj().(*types.Func)
		}
	}

	if fn == nil || pkg == nil || fn.Pkg() != pkg || fn.Origin() != fn {
		return nil
	}

	signature := fn.Type().(*types.Signature)

	if signature.TypeParams().Len() > 0 || signature.RecvTypeParams().Len() > 0 {
		return nil
	}

	if recv := signature.Recv(); recv != nil && types.IsInterface(recv.Type()) {
		return nil
	}

	for i := 0; i < signature.Results().Len(); i++ {
		if signature.Results().At(i).Name() != "" {
			return nil
		}
	}

	return fn
}

// fieldAccessReturnsReinterpret reports whether every return in fn's body gives result index nil, a native-bytes
// reinterpret, or a local of fn's own whose every assignment is one, and at least one return is not nil. A return
// inside a func literal belongs to the literal and is not fn's.
func (v *Visitor) fieldAccessReturnsReinterpret(fn *types.Func, index int) bool {
	decl := fieldAccessFuncDecl(fn, v.info)

	if decl == nil || decl.Body == nil {
		return false
	}

	results, reinterprets, disqualified := fn.Type().(*types.Signature).Results().Len(), 0, false

	ast.Inspect(decl.Body, func(n ast.Node) bool {
		if disqualified {
			return false
		}

		switch node := n.(type) {
		case *ast.FuncLit:
			return false
		case *ast.ReturnStmt:
			if len(node.Results) != results {
				disqualified = true
				return false
			}

			result := ast.Unparen(node.Results[index])

			if tv, ok := v.info.Types[result]; ok && tv.IsNil() {
				return false
			}

			if isNativeBytesReinterpret(v.info, result) {
				reinterprets++
				return false
			}

			if ident, ok := result.(*ast.Ident); ok {
				if local, ok := v.info.Uses[ident].(*types.Var); ok && !local.IsField() && local.Parent() != nil &&
					local.Parent() != local.Pkg().Scope() && v.fieldAccessLocalPointee(local, decl.Body, true) != nil {
					reinterprets++
					return false
				}
			}

			disqualified = true
			return false
		}

		return true
	})

	return !disqualified && reinterprets > 0
}

// The function declarations of the package being converted, by object, built once per loaded package from the
// syntax its driver handed collectCaptureModeMethods (currentLoadedPackage) -- every driver's package, test
// variants included, so a callee in another file of the package is found.
var (
	fieldAccessFuncDeclsOf *packages.Package
	fieldAccessFuncDecls   map[*types.Func]*ast.FuncDecl
)

// fieldAccessFuncDecl answers fn's declaration in the package being converted, or nil when info does not type that
// package's syntax.
func fieldAccessFuncDecl(fn *types.Func, info *types.Info) *ast.FuncDecl {
	packageLock.Lock()
	defer packageLock.Unlock()

	loaded := currentLoadedPackage

	if loaded == nil || loaded.TypesInfo != info {
		return nil
	}

	if fieldAccessFuncDeclsOf != loaded {
		fieldAccessFuncDeclsOf = loaded
		fieldAccessFuncDecls = map[*types.Func]*ast.FuncDecl{}

		for _, file := range loaded.Syntax {
			for _, decl := range file.Decls {
				if funcDecl, ok := decl.(*ast.FuncDecl); ok {
					if object, ok := info.Defs[funcDecl.Name].(*types.Func); ok {
						fieldAccessFuncDecls[object] = funcDecl
					}
				}
			}
		}
	}

	return fieldAccessFuncDecls[fn]
}

// fieldAccessSelector renders a field selection `p.f` (or a promoted `p.f` through value embeds) whose base is a
// local fieldAccessLocal admits, recording the struct type for the file's record; ok is false for a selection this
// route does not serve, which then keeps its emission.
func (v *Visitor) fieldAccessSelector(selectorExpr *ast.SelectorExpr, pointee types.Type) (string, bool) {
	selection := v.info.Selections[selectorExpr]

	if selection == nil || selection.Kind() != types.FieldVal {
		return "", false
	}

	base := v.convExpr(selectorExpr.X, nil)
	path := selection.Index()

	if len(path) == 1 {
		if fieldAccessCollisions[removeLeadingSanitizationMarker(getCoreSanitizedIdentifier(selectorExpr.Sel.Name))] ||
			shadowsPromotedMethod(pointee, selectorExpr.Sel.Name) || isHandOwnedType(pointee) {
			return fmt.Sprintf("%s.of(%s).Value", base, v.fieldAccessBoxAccessor(pointee, selectorExpr.Sel, selectorExpr.X)), true
		}

		v.recordFieldAccessType(pointee)

		return fmt.Sprintf("%s.%s", base, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))), true
	}

	// A promoted field: walk its value embeds by name. A pointer embed puts the field in another allocation,
	// which keeps today's emission.
	if isHandOwnedType(pointee) {
		return "", false
	}

	expr := base
	owner := pointee

	for i, index := range path[:len(path)-1] {
		structType, ok := owner.Underlying().(*types.Struct)

		if !ok || index >= structType.NumFields() {
			return "", false
		}

		field := structType.Field(index)

		if !field.Embedded() {
			return "", false
		}

		if _, isPtr := types.Unalias(field.Type()).(*types.Pointer); isPtr {
			return "", false
		}

		name := getCoreSanitizedIdentifier(field.Name())

		if fieldCollidesWithOwnerType(field.Name(), owner) {
			name = typeCollidingFieldName(name)
		}

		name = removeLeadingSanitizationMarker(name)

		if i == 0 && fieldAccessCollisions[name] || shadowsPromotedMethod(owner, field.Name()) {
			return "", false
		}

		expr += "." + name
		owner = field.Type()
	}

	if shadowsPromotedMethod(owner, selectorExpr.Sel.Name) {
		return "", false
	}

	v.recordFieldAccessType(pointee)

	return fmt.Sprintf("%s.%s", expr, v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))), true
}

// P1: GO'S `p.f` THROUGH ANY POINTER (parity seat #31, COORD rulings 2026-10-27 and 2026-10-28;
// docs/phase4/DESIGN-p1-field-properties.md)
//
// A field selected through a pointer to a named struct reads and writes as Go spells it, `p.f`, where the read was
// `(~p).f` (a copy of the whole struct) and the write `p.Value.f`. The site binds the property go2cs-gen emits on the
// box for each type the file's `[GoFieldAccess(…)]` record lists, the record and the class of the unsafe.Pointer route
// above. Its body is specialized: `ref ʗp.Value.f` on a managed box, the same storage `p.Value.f` reaches, with
// Value's own nil check; and `ʗp.field(T.Ꮡf)` on a NativeBox<T>, which answers the field at its GO offset where the
// struct's CLR slot holds the wrong bytes (a behavior change on native boxes, and a correctness gain).
//
// These keep today's spelling, because no property serves them as Go spells them:
//   - a field named like a member of ж<T> (fieldAccessCollisions.txt), or one that shadows a method an embedded type
//     promotes (shadowsPromotedMethod): C# would bind the member, or the method group;
//   - a PROMOTED field: the properties are the struct's own fields, so the site would have to name the embed, `p.E.f`,
//     which is not Go's text (the unsafe.Pointer route above does, because it must avoid the whole-value read);
//   - a struct with no generated accessors: one a hand-owned file declares, and one of a package the converter never
//     emits (nonConvertedStdLibPackages: testing's T and B are golib-side structs);
//   - a struct shaped as a Go slice or string header (isGoHeaderShaped), whose properties are get/set, not refs;
//   - an unnamed struct type, which no record can name;
//   - the plain rendering of a zero-size store target (zeroSizeStorePlain), whose box form names its struct.

// fieldPropertySelector renders a field selection `p.f` through a pointer to pointee as the generated property
// serves it, recording the struct type for the file's record; ok is false for a selection that keeps today's spelling.
func (v *Visitor) fieldPropertySelector(selectorExpr *ast.SelectorExpr, pointee types.Type, xContexts []ExprContext) (string, bool) {
	if selectorExpr == v.zeroSizeStorePlain || !fieldPropertyServesType(pointee) {
		return "", false
	}

	selection := v.info.Selections[selectorExpr]

	if selection == nil || selection.Kind() != types.FieldVal {
		return "", false
	}

	if len(selection.Index()) != 1 || fieldAccessCollisions[removeLeadingSanitizationMarker(getCoreSanitizedIdentifier(selectorExpr.Sel.Name))] ||
		shadowsPromotedMethod(pointee, selectorExpr.Sel.Name) {
		return "", false
	}

	v.recordFieldAccessType(pointee)

	return fmt.Sprintf("%s.%s", v.convExpr(selectorExpr.X, xContexts), v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))), true
}

// fieldPropertyServesType reports whether go2cs-gen gives pointee's fields their properties: a named struct with
// generated accessors that is not header-shaped (see the P1 notes above).
func fieldPropertyServesType(pointee types.Type) bool {
	named, ok := types.Unalias(pointee).(*types.Named)

	if !ok {
		return false
	}

	structType, ok := named.Underlying().(*types.Struct)

	if !ok || isHandOwnedType(named) || isGoHeaderShaped(structType) {
		return false
	}

	pkg := named.Obj().Pkg()

	return pkg != nil && !slices.Contains(nonConvertedStdLibPackages, pkg.Path())
}

// isGoHeaderShaped mirrors go2cs-gen's FieldAccessGenerator.IsHeaderShaped on the Go side: exactly an unsafe.Pointer
// field and then one or two int fields, Go's slice and string headers, whose generated properties are get/set (the
// setter commits the header), so they serve no `ref`.
func isGoHeaderShaped(structType *types.Struct) bool {
	if structType.NumFields() != 2 && structType.NumFields() != 3 {
		return false
	}

	for i := 0; i < structType.NumFields(); i++ {
		want := types.Int

		if i == 0 {
			want = types.UnsafePointer
		}

		if basic, ok := types.Unalias(structType.Field(i).Type()).(*types.Basic); !ok || basic.Kind() != want {
			return false
		}
	}

	return true
}

// packageHandOwnedTypes holds the types the package's hand-owned files ([module: GoManualConversion]) declare, and
// packageHandOwnedVars their package-level vars plus the vars manualConversionVars displaces for this target; reset per
// package and filled by collectHandOwnedDecls in each conversion driver.
var (
	packageHandOwnedTypes = map[*types.TypeName]bool{}
	packageHandOwnedVars  = map[*types.Var]bool{}
)

// collectHandOwnedDecls records the types and vars declared by the files whose destination is a hand-owned `.cs`, and
// the vars a hand-owned member supplies (manualConversionVars). The converter emits none of their declarations, so
// nothing may assume the shape it would have emitted (lazyPackageVarOperations.go keeps such a var eager).
func collectHandOwnedDecls(files []FileEntry, pkg *types.Package, info *types.Info, goos string) {
	for _, entry := range files {
		if !entry.manualConversion {
			continue
		}

		for _, decl := range entry.file.Decls {
			genDecl, ok := decl.(*ast.GenDecl)

			if !ok {
				continue
			}

			for _, spec := range genDecl.Specs {
				switch spec := spec.(type) {
				case *ast.TypeSpec:
					if typeName, ok := info.Defs[spec.Name].(*types.TypeName); ok {
						packageHandOwnedTypes[typeName] = true
					}
				case *ast.ValueSpec:
					if genDecl.Tok != token.VAR {
						continue
					}

					for _, name := range spec.Names {
						if obj, ok := info.Defs[name].(*types.Var); ok {
							packageHandOwnedVars[obj] = true
						}
					}
				}
			}
		}
	}

	if pkg == nil {
		return
	}

	for _, name := range pkg.Scope().Names() {
		if obj, ok := pkg.Scope().Lookup(name).(*types.Var); ok && isManualVarInPackage(pkg.Path(), goos, name) {
			packageHandOwnedVars[obj] = true
		}
	}
}

// isHandOwnedType reports whether t is a struct a hand-owned file of this package declares.
func isHandOwnedType(t types.Type) bool {
	named, ok := types.Unalias(t).(*types.Named)
	return ok && packageHandOwnedTypes[named.Origin().Obj()]
}

// shadowsPromotedMethod reports whether a field `name` of struct type owner shadows a method of the same name that an
// embedded type promotes. Go selects the field, the shallower member, but the box carries the promoted method as an
// extension method, and C# binds `p.name` to that method group rather than to the field property (abi.ArrayType's
// field Len against the Len method its embedded abi.Type promotes). Such a site takes the explicit box form.
func shadowsPromotedMethod(owner types.Type, name string) bool {
	seen := map[types.Type]bool{}

	var walk func(t types.Type) bool

	walk = func(t types.Type) bool {
		structType, ok := t.Underlying().(*types.Struct)

		if !ok || seen[t] {
			return false
		}

		seen[t] = true

		for i := 0; i < structType.NumFields(); i++ {
			field := structType.Field(i)

			if !field.Embedded() {
				continue
			}

			embedded := types.Unalias(field.Type())

			if ptr, isPtr := embedded.(*types.Pointer); isPtr {
				embedded = ptr.Elem()
			}

			methods := types.NewMethodSet(types.NewPointer(embedded))

			for j := 0; j < methods.Len(); j++ {
				if methods.At(j).Obj().Name() == name {
					return true
				}
			}

			if walk(embedded) {
				return true
			}
		}

		return false
	}

	return walk(owner)
}

// fieldAccessBoxAccessor renders the field's box accessor, `T.Ꮡf`, as the address-of emission names it.
func (v *Visitor) fieldAccessBoxAccessor(pointee types.Type, sel *ast.Ident, structExpr ast.Expr) string {
	typeName := convertToCSTypeName(v.getAliasQualifiedTypeName(pointee, false))

	return fmt.Sprintf("%s.%s%s", v.boxAccessorType(typeName, "", pointee), AddressPrefix, v.structFieldBoxName(sel, structExpr))
}

// fieldAccessArrayField renders the box of an array field `p.arr` reached through a local fieldAccessLocal admits,
// `p.of(T.Ꮡarr)`, for the element form `p.of(T.Ꮡarr).ElementRef(i)` (the element in place in native memory, and
// `ref p.Value.arr[i]` in managed memory) and the slice form `p.of(T.Ꮡarr).slice(…)`; ok is false for any other
// selection.
func (v *Visitor) fieldAccessArrayField(selectorExpr *ast.SelectorExpr) (string, bool) {
	pointee := v.fieldAccessLocal(selectorExpr.X)

	if pointee == nil {
		return "", false
	}

	selection := v.info.Selections[selectorExpr]

	if selection == nil || selection.Kind() != types.FieldVal || len(selection.Index()) != 1 {
		return "", false
	}

	if _, isArray := types.Unalias(selection.Type()).(*types.Array); !isArray {
		return "", false
	}

	return fmt.Sprintf("%s.of(%s)", v.convExpr(selectorExpr.X, nil), v.fieldAccessBoxAccessor(pointee, selectorExpr.Sel, selectorExpr.X)), true
}

func (v *Visitor) recordFieldAccessType(pointee types.Type) {
	typeName := convertToCSTypeName(v.getAliasQualifiedTypeName(pointee, false))

	// A generic struct is listed by its open definition, `typeof(pair<>)`: go2cs-gen reads the definition anyway, and
	// an instantiation's arguments may be type parameters the package class cannot see.
	if named, ok := types.Unalias(pointee).(*types.Named); ok && named.TypeArgs().Len() > 0 {
		typeName = openGenericTypeName(typeName, named.TypeArgs().Len())
	}

	for _, recorded := range v.fieldAccessTypes {
		if recorded == typeName {
			return
		}
	}

	v.fieldAccessTypes = append(v.fieldAccessTypes, typeName)
}

// openGenericTypeName respells a rendered generic type's trailing argument list as its open form, `pair<T>` as
// `pair<>` and `table<K, V>` as `table<,>`.
func openGenericTypeName(typeName string, arity int) string {
	if !strings.HasSuffix(typeName, ">") {
		return typeName
	}

	depth := 0

	for i := len(typeName) - 1; i >= 0; i-- {
		switch typeName[i] {
		case '>':
			depth++
		case '<':
			if depth--; depth == 0 {
				return typeName[:i] + "<" + strings.Repeat(",", arity-1) + ">"
			}
		}
	}

	return typeName
}

// fieldAccessRecord renders the file's `[GoFieldAccess(typeof(T), …)]` record, placed on the package class
// declaration so each type is named as the file's own code names it; empty when the file has no site.
func (v *Visitor) fieldAccessRecord() string {
	if len(v.fieldAccessTypes) == 0 {
		return ""
	}

	types := make([]string, len(v.fieldAccessTypes))

	for i, typeName := range v.fieldAccessTypes {
		types[i] = fmt.Sprintf("typeof(%s)", typeName)
	}

	return fmt.Sprintf("[GoFieldAccess(%s)]%s", strings.Join(types, ", "), v.newline)
}
