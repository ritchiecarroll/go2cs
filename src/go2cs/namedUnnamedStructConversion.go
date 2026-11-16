// namedUnnamedStructConversion.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// A named struct type and an unnamed struct type with the same fields are two C# types: the named one
// is its own declaration and the unnamed one is LIFTED to a generated type (liftedTypeNames.go). Go
// moves a value between them freely. An explicit conversion is always allowed, the value is assignable
// in either direction with no conversion written, and the two compare with == and !=. In C# every
// one of those needs the generated operator a GoImplicitConv record asks for, and two of them also
// need a cast at the site. This file holds those two.

package main

import (
	"go/ast"
	"go/token"
	"go/types"

	"github.com/ritchiecarroll/hashset"
)

// structTypeLiteralTarget reports the UNNAMED struct type literal a conversion targets,
// `struct{ Hz int }(v)`, parenthesised or not, or nil when the callee is anything else. A named
// struct target is not reported: it has a types.Object, which isTypeConversion's peel resolves.
func structTypeLiteralTarget(fun ast.Expr) *ast.StructType {
	for {
		switch expr := fun.(type) {
		case *ast.ParenExpr:
			fun = expr.X
		case *ast.StructType:
			return expr
		default:
			return nil
		}
	}
}

// structTypeLiteralConversion renders a conversion to an unnamed struct type as a cast to its lifted
// type and records the pair, so go2cs-gen writes the operator the cast binds. Go writes the
// conversion as a call, and the target has no types.Object for isTypeConversion to find, so without
// this the lifted type's name was emitted as a callee, `main_a(p)` (CS1955: a type is not invocable).
// It is the struct twin of the interface type literal arm beside its call site.
func (v *Visitor) structTypeLiteralConversion(callExpr *ast.CallExpr) (string, bool) {
	structLit := structTypeLiteralTarget(callExpr.Fun)

	if structLit == nil || len(callExpr.Args) != 1 {
		return "", false
	}

	structType := v.info.TypeOf(structLit)
	arg := callExpr.Args[0]

	if structType == nil || v.info.TypeOf(arg) == nil {
		return "", false
	}

	// The lift is named, or found, exactly as the callee rendering named it before.
	targetName := v.convExpr(structLit, nil)

	// An operand of the SAME unnamed type under another lift is cast as well (recordStructLiftMove).
	v.structLiftCasts = append(v.structLiftCasts, [2]types.Type{v.info.TypeOf(arg), structType})

	// The cast is wrapped as every conversion to a named type is: its result can be the base of a
	// selector, `struct{ Hz int }(p).Hz`, and unwrapped that read `(T)p.Hz`, a cast of the field. Where
	// the conversion stands as a whole expression the wrap is dropped again
	// (unparenthesizedWholeExpression).
	return "(" + v.structCastOf(targetName, arg, v.checkForImplicitConversion(structType, arg, targetName, nil)) + ")", true
}

// structCastOf renders the cast of a struct operand as the named-target conversion does: `(T)p` for an
// operand that binds tighter than a cast, `(T)(expr)` otherwise.
func (v *Visitor) structCastOf(csType string, operand ast.Expr, rendered string) string {
	if v.needsParentheses(operand) {
		return "(" + csType + ")(" + rendered + ")"
	}

	return "(" + csType + ")" + rendered
}

// namedUnnamedStructComparison renders `a == h` and `a != h` where one operand is a named struct value
// and the other an unnamed one. Each of the two C# types declares the operator and each converts to
// the other implicitly, so the bare comparison has two candidates and neither is better (CS9342). The
// unnamed operand is cast to the named type, which leaves the named type's own operator, the one a
// reader finds declared; the pair is recorded for the cast. Go compares the two field by field
// whichever side is converted.
func (v *Visitor) namedUnnamedStructComparison(binaryExpr *ast.BinaryExpr) (string, bool) {
	if binaryExpr.Op != token.EQL && binaryExpr.Op != token.NEQ {
		return "", false
	}

	lhsType := v.info.TypeOf(binaryExpr.X)
	rhsType := v.info.TypeOf(binaryExpr.Y)

	if lhsType == nil || rhsType == nil {
		return "", false
	}

	var named, unnamed ast.Expr
	var namedType types.Type

	switch {
	case isNamedStruct(lhsType) && isUnnamedStruct(types.Unalias(rhsType)):
		named, unnamed, namedType = binaryExpr.X, binaryExpr.Y, lhsType
	case isUnnamedStruct(types.Unalias(lhsType)) && isNamedStruct(rhsType):
		named, unnamed, namedType = binaryExpr.Y, binaryExpr.X, rhsType
	default:
		// Two lifts of one unnamed type compare the same way: one operand is cast to the other's lift.
		liftName, castLeft, ok := v.structLiftComparisonCast(binaryExpr.X, binaryExpr.Y)

		if !ok {
			return "", false
		}

		kept, cast := binaryExpr.X, binaryExpr.Y

		if castLeft {
			kept, cast = binaryExpr.Y, binaryExpr.X
		}

		converted := v.structCastOf(liftName, cast, v.convExpr(cast, nil))
		other := v.convExpr(kept, nil)

		if castLeft {
			return converted + " " + binaryExpr.Op.String() + " " + other, true
		}

		return other + " " + binaryExpr.Op.String() + " " + converted, true
	}

	namedName := v.getCSharpTypeName(namedType)
	converted := v.structCastOf(namedName, unnamed, v.checkForImplicitConversion(namedType, unnamed, namedName, nil))
	other := v.convExpr(named, nil)

	if unnamed == binaryExpr.X {
		return converted + " " + binaryExpr.Op.String() + " " + other, true
	}

	return other + " " + binaryExpr.Op.String() + " " + converted, true
}

// namedUnnamedStructCase is namedUnnamedStructComparison for a switch, which writes its own
// `tag == value` for each case: when one of the two is a named struct value and the other an unnamed
// one it names the type the unnamed side is cast to, tells whether that side is the tag, and records
// the pair for the cast.
func (v *Visitor) namedUnnamedStructCase(tag, value ast.Expr) (namedName string, tagIsUnnamed bool, ok bool) {
	tagType := v.info.TypeOf(tag)
	valueType := v.info.TypeOf(value)

	if tagType == nil || valueType == nil {
		return "", false, false
	}

	var unnamed ast.Expr
	var namedType types.Type

	switch {
	case isNamedStruct(tagType) && isUnnamedStruct(types.Unalias(valueType)):
		unnamed, namedType = value, tagType
	case isUnnamedStruct(types.Unalias(tagType)) && isNamedStruct(valueType):
		unnamed, namedType, tagIsUnnamed = tag, valueType, true
	default:
		// Two lifts of one unnamed type: the cast names a lift, and the tag is the cast side when the
		// value's lift is the one known here.
		return v.structLiftComparisonCast(tag, value)
	}

	namedName = v.getCSharpTypeName(namedType)
	v.applyImplicitConversion(namedType, unnamed, namedName, "")

	return namedName, tagIsUnnamed, true
}

// structLiftComparisonCast decides the cast for `a == b` where both operands have ONE unnamed struct type
// under two lifts (recordStructLiftMove tells how one type comes to have two). Each lift declares its own
// `==`, so the bare comparison binds neither; one operand is cast to the other's lift, as an unnamed operand
// is cast to a named type. The comparison is rendered while the file is still being visited, so a lift may
// have no name yet: the cast names a lift this file has already made, the left operand's where both have one.
//
// With one lift named and the other still to come, the two will be ONE C# type exactly when the named lift
// is the package's registered lift of the signature, which the later one then adopts (visitStructType); no
// cast is written for that pair. With neither named the comparison is written bare, as before.
func (v *Visitor) structLiftComparisonCast(left, right ast.Expr) (liftName string, castLeft bool, ok bool) {
	leftType, rightType := types.Unalias(v.info.TypeOf(left)), types.Unalias(v.info.TypeOf(right))
	leftStruct, isLeftStruct := leftType.(*types.Struct)
	rightStruct, isRightStruct := rightType.(*types.Struct)

	if !isLeftStruct || !isRightStruct || leftStruct == rightStruct || isEmptyStructType(leftStruct) || !types.Identical(leftType, rightType) {
		return "", false, false
	}

	leftName, leftLifted := v.liftedNameFor(leftType)
	rightName, rightLifted := v.liftedNameFor(rightType)

	switch {
	case leftLifted && rightLifted:
		if leftName == rightName {
			return "", false, false
		}
	case leftLifted:
		if lookupDynamicTypeName(leftType.String()) == leftName {
			return "", false, false
		}
	case rightLifted:
		if lookupDynamicTypeName(rightType.String()) == rightName {
			return "", false, false
		}

		v.structLiftCasts = append(v.structLiftCasts, [2]types.Type{leftType, rightType})

		return rightName, true, true
	default:
		return "", false, false
	}

	v.structLiftCasts = append(v.structLiftCasts, [2]types.Type{rightType, leftType})

	return leftName, false, true
}

// isNamedStruct reports whether a type is a defined type, or an alias of one, whose underlying type
// is a struct.
func isNamedStruct(t types.Type) bool {
	named, ok := types.Unalias(t).(*types.Named)

	if !ok {
		return false
	}

	_, isStruct := named.Underlying().(*types.Struct)

	return isStruct
}

// recordNamedUnnamedStructMoves records the conversion behind every place the file moves a struct value
// between a named struct type and an unnamed one with NO conversion written. Go's assignability rule lets
// such a value through wherever a value is assigned: a declaration with a type, an assignment, a return, an
// argument, a composite literal's field, element or key, a channel send, a map index and a range clause that
// assigns. In C# each is an implicit conversion between two unrelated types, and only a GoImplicitConv record
// makes go2cs-gen write its operator.
//
// The records are a set per package, so a site that recorded nothing compiled wherever another site of the
// same pair sat beside it: only arguments and returns into an unnamed type recorded, and
// `var x struct{ Hz int } = p` alone was CS0029. Reading the sites off the syntax by Go's own rule, in one
// place, is what keeps a new statement form from being missed again.
//
// It runs AFTER the file's declarations are visited, because an unnamed struct type only has its C# name once
// the visit has lifted it. results is the result list of the function the node is in, nil outside one.
func (v *Visitor) recordNamedUnnamedStructMoves(root ast.Node, results *types.Tuple) {
	// The casts between two lifts that the visit wrote before both lifts had their names.
	for _, cast := range v.structLiftCasts {
		v.recordStructLiftMove(cast[1], cast[0])
	}

	v.structLiftCasts = nil

	ast.Inspect(root, func(node ast.Node) bool {
		switch n := node.(type) {
		case *ast.FuncDecl:
			if n.Body != nil {
				v.recordNamedUnnamedStructMoves(n.Body, signatureResults(v.getType(n.Name, false)))
			}

			return false
		case *ast.FuncLit:
			v.recordNamedUnnamedStructMoves(n.Body, signatureResults(v.getType(n, false)))

			return false
		case *ast.ValueSpec:
			if n.Type != nil && len(n.Values) > 0 {
				targets := make([]types.Type, len(n.Names))

				for i := range targets {
					targets[i] = v.getType(n.Type, false)
				}

				v.recordStructMovesInto(targets, n.Values)
			}
		case *ast.AssignStmt:
			if n.Tok == token.ASSIGN {
				targets := make([]types.Type, len(n.Lhs))

				for i, lhs := range n.Lhs {
					targets[i] = v.getType(lhs, false)
				}

				v.recordStructMovesInto(targets, n.Rhs)
			}
		case *ast.ReturnStmt:
			if results != nil && len(n.Results) > 0 {
				targets := make([]types.Type, results.Len())

				for i := range targets {
					targets[i] = results.At(i).Type()
				}

				v.recordStructMovesInto(targets, n.Results)
			}
		case *ast.SendStmt:
			if channel, ok := underlyingOf(v.getType(n.Chan, false)).(*types.Chan); ok {
				v.recordNamedUnnamedStructMove(channel.Elem(), v.getType(n.Value, false))
			}
		case *ast.CallExpr:
			// A conversion is not a move: the call site casts (structTypeLiteralConversion, convCallExpr).
			if tv, ok := v.info.Types[n.Fun]; ok && tv.IsType() {
				return true
			}

			if signature, ok := underlyingOf(v.getType(n.Fun, false)).(*types.Signature); ok {
				// f(g()) hands g's results to f's parameters one for one.
				count := len(n.Args)

				if count == 1 {
					if tuple, ok := v.getType(n.Args[0], false).(*types.Tuple); ok {
						count = tuple.Len()
					}
				}

				targets := make([]types.Type, count)

				for i := range targets {
					targets[i] = argumentParameterType(signature, i, n.Ellipsis.IsValid())
				}

				v.recordStructMovesInto(targets, n.Args)
			}
		case *ast.IndexExpr:
			if mapType, ok := underlyingOf(v.getType(n.X, false)).(*types.Map); ok {
				v.recordNamedUnnamedStructMove(mapType.Key(), v.getType(n.Index, false))
			}
		case *ast.RangeStmt:
			if n.Tok == token.ASSIGN {
				key, value := rangeClauseTypes(v.getType(n.X, false))

				v.recordNamedUnnamedStructMove(v.getType(n.Key, false), key)
				v.recordNamedUnnamedStructMove(v.getType(n.Value, false), value)
			}
		case *ast.CompositeLit:
			literalType := underlyingOf(v.getType(n, false))

			// An elided `{...}` inside a []*T literal has the type *T.
			if pointer, ok := literalType.(*types.Pointer); ok {
				literalType = underlyingOf(pointer.Elem())
			}

			switch literal := literalType.(type) {
			case *types.Struct:
				for i, element := range n.Elts {
					if keyValue, ok := element.(*ast.KeyValueExpr); ok {
						if name, ok := keyValue.Key.(*ast.Ident); ok {
							for f := range literal.NumFields() {
								if literal.Field(f).Name() == name.Name {
									v.recordNamedUnnamedStructMove(literal.Field(f).Type(), v.getType(keyValue.Value, false))
								}
							}
						}
					} else if i < literal.NumFields() {
						v.recordNamedUnnamedStructMove(literal.Field(i).Type(), v.getType(element, false))
					}
				}
			case *types.Array:
				v.recordStructElementMoves(nil, literal.Elem(), n.Elts)
			case *types.Slice:
				v.recordStructElementMoves(nil, literal.Elem(), n.Elts)
			case *types.Map:
				v.recordStructElementMoves(literal.Key(), literal.Elem(), n.Elts)
			}
		}

		return true
	})
}

// recordStructElementMoves records the moves of an array, slice or map literal's elements. A key is a value
// only in a map literal (keyType is nil otherwise: an array's key is an index).
func (v *Visitor) recordStructElementMoves(keyType, elementType types.Type, elements []ast.Expr) {
	for _, element := range elements {
		if keyValue, ok := element.(*ast.KeyValueExpr); ok {
			v.recordNamedUnnamedStructMove(keyType, v.getType(keyValue.Key, false))
			element = keyValue.Value
		}

		v.recordNamedUnnamedStructMove(elementType, v.getType(element, false))
	}
}

// recordStructMovesInto records the moves of values into slots of the target types: one value for each slot,
// or ONE value of several results (a call, a comma-ok receive, map read or type assertion), which go/types
// records as a tuple.
func (v *Visitor) recordStructMovesInto(targets []types.Type, values []ast.Expr) {
	if len(values) == len(targets) {
		for i, value := range values {
			v.recordNamedUnnamedStructMove(targets[i], v.getType(value, false))
		}

		return
	}

	if len(values) != 1 {
		return
	}

	if tuple, ok := v.getType(values[0], false).(*types.Tuple); ok && tuple.Len() == len(targets) {
		for i := range targets {
			v.recordNamedUnnamedStructMove(targets[i], tuple.At(i).Type())
		}
	}
}

// recordNamedUnnamedStructMove records the conversion from source to target when one is a named struct type
// and the other an unnamed one. The argument loop of convCallExpr records the same way, with the same guards.
func (v *Visitor) recordNamedUnnamedStructMove(target, source types.Type) {
	if target == nil || source == nil {
		return
	}

	if isUnnamedStruct(types.Unalias(target)) && isUnnamedStruct(types.Unalias(source)) {
		v.recordStructLiftMove(target, source)
		return
	}

	namedTarget, namedSource := isNamedStruct(target), isNamedStruct(source)

	if namedTarget == namedSource || !isUnnamedStruct(types.Unalias(target)) && !isUnnamedStruct(types.Unalias(source)) {
		return
	}

	v.applyImplicitConversionOf(target, source, convertToCSTypeName(v.getAliasQualifiedTypeName(target, false)), "")
}

// recordStructLiftMove records the conversion between two LIFTS of one unnamed struct type. Go calls
// `struct{ Hz int }` one type wherever it is written and moves a value freely between two places that write
// it. The converter makes one C# type of the two where it can: inside one function, and where a package-level
// declaration or a function's signature has already registered its lift (visitStructType). Where it cannot,
// a function's own lift and the result of a function declared BELOW it for one, the value moves between two
// C# types, and the record makes go2cs-gen write the operator, as it does for a named and an unnamed type.
// The argument loop and the return statement record the same pair for their own sites
// (checkForDynamicStructs); this names the two lifts the same way.
//
// Both lifts must be this file's. A lift made by a sibling file has no name here, and its signature alone
// does not tell it from this file's lift of the same type, so that move records nothing.
func (v *Visitor) recordStructLiftMove(target, source types.Type) {
	target, source = types.Unalias(target), types.Unalias(source)

	if target == source || !types.Identical(target, source) || typeContainsTypeParams(target) || typeContainsTypeParams(source) {
		return
	}

	sourceLift, sourceLifted := v.liftedNameFor(source)
	targetLift, targetLifted := v.liftedNameFor(target)

	if !sourceLifted || !targetLifted || sourceLift == targetLift {
		return
	}

	sourceName, targetName := v.getCSharpTypeName(source), v.getCSharpTypeName(target)

	packageLock.Lock()

	if conversions, exists := implicitConversions[sourceName]; exists {
		conversions.Add(targetName)
	} else {
		implicitConversions[sourceName] = hashset.NewHashSet([]string{targetName})
	}

	packageLock.Unlock()

	v.addImplicitSubStructConversions(source, targetName, false)
}

// signatureResults is the result list of a function type, nil for any other type.
func signatureResults(t types.Type) *types.Tuple {
	if signature, ok := underlyingOf(t).(*types.Signature); ok {
		return signature.Results()
	}

	return nil
}

// rangeClauseTypes is the types a range clause assigns to its key and its value, nil where the clause has
// none or the type cannot be a struct (an index, a rune).
func rangeClauseTypes(ranged types.Type) (key, value types.Type) {
	switch t := underlyingOf(ranged).(type) {
	case *types.Slice:
		return nil, t.Elem()
	case *types.Array:
		return nil, t.Elem()
	case *types.Pointer:
		if array, ok := underlyingOf(t.Elem()).(*types.Array); ok {
			return nil, array.Elem()
		}
	case *types.Map:
		return t.Key(), t.Elem()
	case *types.Chan:
		return t.Elem(), nil
	case *types.Signature:
		// A range-over-func iterator: the clause's variables are the parameters of its yield function.
		if t.Params().Len() == 1 {
			if yield, ok := underlyingOf(t.Params().At(0).Type()).(*types.Signature); ok {
				if yield.Params().Len() > 0 {
					key = yield.Params().At(0).Type()
				}

				if yield.Params().Len() > 1 {
					value = yield.Params().At(1).Type()
				}
			}
		}
	}

	return key, value
}
