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
	"strings"
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

	pointee := v.fieldAccessLocalPointee(obj)

	if v.fieldAccessLocalCache == nil {
		v.fieldAccessLocalCache = make(map[types.Object]types.Type)
	}

	v.fieldAccessLocalCache[obj] = pointee
	return pointee
}

func (v *Visitor) fieldAccessLocalPointee(obj *types.Var) types.Type {
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

		if !isUnsafePointerReinterpret(v.info, value) {
			disqualified = true
		}
	}

	is := func(expr ast.Expr) bool {
		ident, ok := ast.Unparen(expr).(*ast.Ident)
		return ok && (v.info.Defs[ident] == obj || v.info.Uses[ident] == obj)
	}

	ast.Inspect(v.currentFuncDecl.Body, func(n ast.Node) bool {
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
					disqualified = true
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
					disqualified = true
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

	for _, recorded := range v.fieldAccessTypes {
		if recorded == typeName {
			return
		}
	}

	v.fieldAccessTypes = append(v.fieldAccessTypes, typeName)
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
