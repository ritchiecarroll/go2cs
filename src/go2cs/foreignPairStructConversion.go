// foreignPairStructConversion.go - Gbtc
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
	"strings"

	"github.com/ritchiecarroll/hashset"
)

// foreignPairStructConversion renders an explicit conversion between two struct types that BOTH belong
// to other packages, `lib.Pitch(o)` with o an `other.Pitch`. Go converts the two because their fields
// are identical. C# has one unrelated type per declaration, and a conversion operator must be declared
// by one of its two types; both are closed assemblies here, so neither can host one and the cast the
// converter emits for every other named target, `(lib.Pitch)o`, binds nothing (CS0030).
//
// The pair is recorded as any other conversion is, and go2cs-gen writes ONE static function for it in
// the converting package's class (ImplicitConvGenerator, ForeignPairFunction):
//
//	internal static lib.Pitch libꓸPitch(other.Pitch src) => new lib.Pitch(src.Hz);
//
// and the site calls it, `libꓸPitch(o)`. The function is named as Go names its target, package then
// type; a second source type overloads the same name. Its operand is evaluated once, as Go's is.
//
// The route is taken only where the function can be written and nothing else already converts:
//
//   - both types are declared with a struct type literal. A type written over another named type,
//     `type Tag locale.Locale`, converts from that base through its own operator and keeps its cast;
//   - neither is generic, and neither is this package's own type (a local side hosts the operator);
//   - every field is exported. An unexported field of another package is out of this package's
//     reach in C# as it is by name in Go, and that pair keeps its cast, which names the site;
//   - no field's type holds an unnamed struct type: each package lifts its own C# type for one, and
//     the function would need a second conversion per field.
func (v *Visitor) foreignPairStructConversion(callExpr *ast.CallExpr) (string, bool) {
	if len(callExpr.Args) != 1 {
		return "", false
	}

	tv, ok := v.info.Types[callExpr.Fun]

	if !ok || !tv.IsType() {
		return "", false
	}

	arg := callExpr.Args[0]
	target, source := v.foreignLiteralStruct(tv.Type), v.foreignLiteralStruct(v.info.TypeOf(arg))

	if target == nil || source == nil || types.Identical(target, source) {
		return "", false
	}

	functionName, ok := v.foreignPairFunctionName(target)

	if !ok {
		return "", false
	}

	sourceName, targetName := v.getCSharpTypeName(source), v.getCSharpTypeName(target)

	packageLock.Lock()

	if conversions, exists := implicitConversions[sourceName]; exists {
		conversions.Add(targetName)
	} else {
		implicitConversions[sourceName] = hashset.NewHashSet([]string{targetName})
	}

	packageLock.Unlock()

	v.recordConversionPackageUsing(source)
	v.recordConversionPackageUsing(target)

	// Go copies the operand. The function builds its result from the operand's fields, so an array the
	// operand carries is cloned first where the operand is existing storage (conversionOperandCopy).
	operand := v.conversionOperandCopy(target, arg, v.convExpr(arg, nil))

	return functionName + "(" + operand + ")", true
}

// foreignLiteralStruct returns t as a named struct type one side of a foreign pair can be: declared by
// ANOTHER package with a struct type literal, not generic, every field exported and none holding an
// unnamed struct type. Any other type answers nil.
func (v *Visitor) foreignLiteralStruct(t types.Type) *types.Named {
	if t == nil {
		return nil
	}

	named, ok := types.Unalias(t).(*types.Named)

	if !ok || named.TypeParams().Len() > 0 || named.TypeArgs().Len() > 0 {
		return nil
	}

	obj := named.Obj()

	if obj == nil || obj.Pkg() == nil || obj.Pkg() == v.pkg {
		return nil
	}

	structType, ok := named.Underlying().(*types.Struct)

	if !ok {
		return nil
	}

	if rhs, found := writtenTypeSpecRHS(obj); !found || !isUnnamedStruct(rhs) {
		return nil
	}

	for i := 0; i < structType.NumFields(); i++ {
		field := structType.Field(i)

		if !field.Exported() || typeHoldsUnnamedStruct(field.Type()) {
			return nil
		}
	}

	return named
}

// foreignPairFunctionName is the name of the function go2cs-gen writes for a pair with this target:
// the target package's class name without its suffix, the alias dot, and the type's C# name
// (`libꓸPitch`). The generator derives the same name from the two C# symbols, so a renamed type or a
// package named differently from its directory agree on both sides.
func (v *Visitor) foreignPairFunctionName(target *types.Named) (string, bool) {
	packageClass := convertImportPathToNamespace(target.Obj().Pkg().Path(), PackageSuffix)
	packageClass = strings.TrimPrefix(packageClass[strings.LastIndex(packageClass, ".")+1:], "@")

	typeName := v.getCSharpTypeName(target)
	typeName = strings.TrimPrefix(typeName[strings.LastIndex(typeName, ".")+1:], "@")

	if !strings.HasSuffix(packageClass, PackageSuffix) || typeName == "" || strings.ContainsAny(typeName, "<>[]"+TypeAliasDot) {
		return "", false
	}

	return strings.TrimSuffix(packageClass, PackageSuffix) + TypeAliasDot + typeName, true
}

// typeHoldsUnnamedStruct reports whether an unnamed struct type is written anywhere in t outside a
// named type: t itself, or behind a pointer, slice, array, map, channel or function signature.
func typeHoldsUnnamedStruct(t types.Type) bool {
	switch typ := types.Unalias(t).(type) {
	case *types.Struct:
		return true
	case *types.Pointer:
		return typeHoldsUnnamedStruct(typ.Elem())
	case *types.Slice:
		return typeHoldsUnnamedStruct(typ.Elem())
	case *types.Array:
		return typeHoldsUnnamedStruct(typ.Elem())
	case *types.Chan:
		return typeHoldsUnnamedStruct(typ.Elem())
	case *types.Map:
		return typeHoldsUnnamedStruct(typ.Key()) || typeHoldsUnnamedStruct(typ.Elem())
	case *types.Signature:
		return tupleHoldsUnnamedStruct(typ.Params()) || tupleHoldsUnnamedStruct(typ.Results())
	}

	return false
}

func tupleHoldsUnnamedStruct(tuple *types.Tuple) bool {
	for i := 0; i < tuple.Len(); i++ {
		if typeHoldsUnnamedStruct(tuple.At(i).Type()) {
			return true
		}
	}

	return false
}
