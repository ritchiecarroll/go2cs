// constraintOperations.go - Gbtc
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
	"strings"

	"github.com/ritchiecarroll/hashset"
)

// In the case of generic constraints, restrictions in C# work somewhat differently than in Go. In C# a constraint
// can be a class, an interface and a few special cases. In Go, all constraints are interfaces and can be restricted
// to types, i.e., structs and heap-allocated types alike. Since at the point of the C# code conversion Go will have
// already parsed and validated the code, we can assume that all the type-based constraints will have been satisfied.
// Also, any defined method-set constraints will be handled as normal for existing interface conversion handling.
// The remaining step to be handled is to determine the set of operators that all types in the constraint type-set
// have in common, which is the set of operators that the C# code will need to account for. There are five sets of
// operators to be considered: Sum, Arithmetic, Integer, Comparison and Ordered. See "Operators" section in the Go
// specification for more details: https://go.dev/ref/spec#Operators

/*
	Sum operator:
	+    sum                    integers, floats, complex values, string

	Arithmetic operators:
	-    difference             integers, floats, complex values
	*    product                integers, floats, complex values
	/    quotient               integers, floats, complex values

	Integer operators:
	%    remainder              integers
	&    bitwise AND            integers
	|    bitwise OR             integers
	^    bitwise XOR            integers
	&^   bit clear (AND NOT)    integers
	<<   left shift             integer << integer >= 0
	>>   right shift            integer >> integer >= 0

	Comparison operators:
	==    equal                 [comparable types]
	!=    not equal             [comparable types]

	Ordered operators:
	<     less                  [ordered types]
	<=    less or equal         [ordered types]
	>     greater               [ordered types]
	>=    greater or equal      [ordered types]

	[comparable types]:         bool, integers, floats, complex values, string, pointer, channel, struct, array
	[ordered types]:            integers, floats, string
*/

// ConstraintType represents the possible range of constraint types
// in Go where operators can be applied.
type ConstraintType int

// Comparable types are the widest set of types that can be used with the
// `==` and `!=` operators. Each type in Go that can support these operators
// should be represented by a unique enum value here:

const (
	Invalid ConstraintType = iota
	Bool
	Int
	Int8
	Int16
	Int32
	Int64
	Uint
	Uint8
	Uint16
	Uint32
	Uint64
	Uintptr
	Float32
	Float64
	Complex64
	Complex128
	String
	Pointer
	Array
	Channel
	Struct
	// Map, Slice, Function, etc., types are not comparable, thus do not
	// have common operator sets in a generic type constaint context.
	// Technically, interfaces are comparable, but they represent the
	// type constraint, so also will not have a common operator set.
)

// OperatorSet represents the set of operators that can be applied to
// the types in the constraint type set.
type OperatorSet int

const (
	// SumOperator represents the `+` operator
	SumOperator OperatorSet = iota

	// ArithmeticOperators represents the `-`, `*`, `/` operators
	ArithmeticOperators

	// IntegerOperators represents the `%`, `&`, `|`, `^`, `&^`, `<<`, `>>` operators
	IntegerOperators

	// ComparableOperators represents the `==`, `!=` operators
	ComparableOperators

	// OrderedOperators represents the `<`, `<=`, `>`, `>=` operators
	OrderedOperators
)

// operators is a map of operator sets to the operators string representations.
// Only valid C# operators are defined here, so "&^" is not included.
var operators = map[OperatorSet][]string{
	SumOperator:         {"+"},
	ArithmeticOperators: {"-", "*", "/"},
	IntegerOperators:    {"%", "&", "|", "^", "<<", ">>"},
	ComparableOperators: {"==", "!="},
	OrderedOperators:    {"<", "<=", ">", ">="},
}

// sumOperatorTypes are types that can be used with the sum operator, i.e.,
// `+`, which includes all numeric types plus strings.
var sumOperatorTypes = hashset.NewHashSet([]ConstraintType{
	Int, Int8, Int16, Int32, Int64, Uint, Uint8, Uint16, Uint32, Uint64,
	Float32, Float64, Complex64, Complex128, String,
})

// arithmeticOperatorTypes are types that can be used with arithmetic operators,
// i.e.: `-`, `*`, `/`, which includes all numeric types.
var arithmeticOperatorTypes = hashset.NewHashSet([]ConstraintType{
	Int, Int8, Int16, Int32, Int64, Uint, Uint8, Uint16, Uint32, Uint64,
	Float32, Float64, Complex64, Complex128,
})

// integerOperatorTypes are types that can be used with integer arithmetic
// operators, i.e.: `%`, `&`, `|`, `^`, `&^`, `<<`, `>>`
var integerOperatorTypes = hashset.NewHashSet([]ConstraintType{
	Int, Int8, Int16, Int32, Int64, Uint, Uint8, Uint16, Uint32, Uint64,
})

// comparableOperatorTypes are types that can be compared for equality, i.e.,
// those that support the `==` and `!=` operators. This is the widest set of
// supported operator types.
//
// ⚠ Widest here means widest that the LIFT can express, which is narrower than Go's `==`. These
// sets decide whether `IEqualityOperators<T, T, bool>` is written into a C# `where` clause, and
// that constraint is a claim about the type ARGUMENT implementing a BCL interface — not about the
// operator being available. The composite Go kinds Go compares perfectly well (Pointer, Channel,
// Array, Struct) implement nothing of the sort: golib's `ж<T>`/`channel<T>`/`array<T>` and every
// `[GoType]` struct compare through Equals/AreEqual, never through `op_Equality`. Lifting for them
// therefore produced a constraint NO instantiation can satisfy — CS0315 at every call site, with
// the diagnostic naming the concrete type rather than the constraint that cannot admit it.
//
// The corpus witness is runtime/pprof's `testProfileRecordNullPadding[T runtime.StackRecord |
// runtime.MemProfileRecord | runtime.BlockProfileRecord]`, whose five call sites were the whole of
// that package's 174-verdict build wall. It is also the ONLY converter-emitted IEqualityOperators
// clause in the corpus that is not on a numeric or ordered union (censused at the fix), which is
// the shape's own signature: a composite type set lifts the comparable operators ALONE, with no
// arithmetic siblings, because it is a subset of no other set here.
//
// The array-core constraint branch in getGenericDefinition already suppressed the same spurious
// lift by hand (`suppressLiftedConstraints`, ~[N]E). Removing the composite kinds states that rule
// once, where the operator sets are defined, instead of per constraint SHAPE — a union of named
// array types took no such branch. Go's own comparability is unaffected: the checker validated
// every instantiation before conversion, and emitted equality on a type parameter routes through
// AreEqual, exactly as the built-in `comparable` arm already documents.
var comparableOperatorTypes = hashset.NewHashSet([]ConstraintType{
	Bool, Int, Int8, Int16, Int32, Int64, Uint, Uint8, Uint16, Uint32, Uint64,
	Float32, Float64, Complex64, Complex128, String,
})

// orderedOperatorTypes are types that can be ordered, i.e., those that support
// the `<`, `<=`, `>`, `>=` operators. This is a subset of the comparable types.
var orderedOperatorTypes = hashset.NewHashSet([]ConstraintType{
	Int, Int8, Int16, Int32, Int64, Uint, Uint8, Uint16, Uint32, Uint64,
	Float32, Float64, String,
})

// getOperatorSet takes a set of constraint types and returns the set of
// operators that can be applied to those types. This is used to determine
// which operators can be used in generic functions and methods.
func getOperatorSet(constraintTypes hashset.HashSet[ConstraintType]) hashset.HashSet[OperatorSet] {
	operatorSet := hashset.HashSet[OperatorSet]{}

	// An empty constraint type set (e.g. a slice or map constraint) supports no
	// lifted operators. Without this guard the empty set would count as a subset
	// of every operator-type set below and incorrectly gain all operators.
	if constraintTypes.IsEmpty() {
		return operatorSet
	}

	if constraintTypes.IsSubsetOfSet(comparableOperatorTypes) {
		operatorSet.Add(ComparableOperators)
	}

	if constraintTypes.IsSubsetOfSet(orderedOperatorTypes) {
		operatorSet.Add(OrderedOperators)
	}

	if constraintTypes.IsSubsetOfSet(arithmeticOperatorTypes) {
		operatorSet.Add(ArithmeticOperators)
	}

	if constraintTypes.IsSubsetOfSet(integerOperatorTypes) {
		operatorSet.Add(IntegerOperators)
	}

	if constraintTypes.IsSubsetOfSet(sumOperatorTypes) {
		operatorSet.Add(SumOperator)
	}

	return operatorSet
}

// getOperatorSetAsString takes a set of operator sets and returns a string
// representation of the operators in those sets.
func getOperatorSetAsString(operatorSets hashset.HashSet[OperatorSet]) string {
	operatorSetKeys := operatorSets.Keys()

	sort.Slice(operatorSetKeys, func(i, j int) bool {
		return int(operatorSetKeys[i]) < int(operatorSetKeys[j])
	})

	results := []string{}

	for _, opSet := range operatorSetKeys {
		results = append(results, operators[opSet]...)
	}

	if len(results) == 0 {
		return "none"
	}

	return strings.Join(results, ", ")
}

// getOperatorSetAttributes takes a set of operator sets and returns a string
// representation of the attribute targets of those sets.
func getOperatorSetAttributes(operatorSets hashset.HashSet[OperatorSet]) string {
	operatorSetKeys := operatorSets.Keys()

	sort.Slice(operatorSetKeys, func(i, j int) bool {
		return int(operatorSetKeys[i]) < int(operatorSetKeys[j])
	})

	targets := []string{}

	for _, opSet := range operatorSetKeys {
		var setName string

		switch opSet {
		case SumOperator:
			setName = "Sum"
		case ArithmeticOperators:
			setName = "Arithmetic"
		case IntegerOperators:
			setName = "Integer"
		case ComparableOperators:
			setName = "Comparable"
		case OrderedOperators:
			setName = "Ordered"
		default:
			setName = ""
		}

		if len(setName) > 0 {
			targets = append(targets, setName)
		}
	}

	return strings.Join(targets, ", ")
}

// getLiftedConstraints takes a constraint type and its name and returns the
// lifted C# operator constraints for that type.
func (v *Visitor) getLiftedConstraints(typ types.Type, name string) string {
	typeConstraints := v.getConstraintTypeSetFromType(typ)
	operatorSets := getOperatorSet(typeConstraints)

	operatorSetKeys := operatorSets.Keys()

	sort.Slice(operatorSetKeys, func(i, j int) bool {
		return int(operatorSetKeys[i]) < int(operatorSetKeys[j])
	})

	liftedConstraints := []string{}

	for _, opSet := range operatorSetKeys {
		var constraints []string

		switch opSet {
		case SumOperator:
			constraints = []string{
				fmt.Sprintf("IAdditionOperators<%s, %s, %s>", name, name, name),
			}
		case ArithmeticOperators:
			constraints = []string{
				fmt.Sprintf("ISubtractionOperators<%s, %s, %s>", name, name, name),
				fmt.Sprintf("IMultiplyOperators<%s, %s, %s>", name, name, name),
				fmt.Sprintf("IDivisionOperators<%s, %s, %s>", name, name, name),
				// `i++` / `i--` on a type parameter (reflect rangeNum's loop, CS0023) binds
				// these. They live in the numeric-only Arithmetic set — NOT the
				// string-including Sum set (@string implements neither). Mirrors the
				// go2cs-gen InterfaceTypeTemplate "Arithmetic" list — keep the two in sync.
				fmt.Sprintf("IIncrementOperators<%s>", name),
				fmt.Sprintf("IDecrementOperators<%s>", name),
				// Unary `-x` on a type parameter (math/rand/v2's
				// `keep[T int | uint | … | uint64](x T) T { return -x }`, CS0023). Numeric-only,
				// like increment/decrement — @string has no negation. Every .NET primitive numeric
				// satisfies this through INumberBase; a go2cs-gen NAMED numeric type satisfies it
				// because NumericTypeTemplate now emits the operator for unsigned types too (as
				// Go's wrap-around `0 - x`) — keep the three lists in sync.
				fmt.Sprintf("IUnaryNegationOperators<%s, %s>", name, name),
			}
		case IntegerOperators:
			constraints = []string{
				fmt.Sprintf("IModulusOperators<%s, %s, %s>", name, name, name),
				fmt.Sprintf("IBitwiseOperators<%s, %s, %s>", name, name, name),
				// The shift-count type parameter is `int`, matching the BCL: every binary
				// integer implements IShiftOperators<TSelf, int, TSelf> (only `int` itself
				// also satisfies the self-typed shape). The emitted shift form is
				// `x << (int)(k)` (intCastOperand coerces every shift count to int), so this
				// is exactly the shape a generic body requires (strconv bsearch
				// ~uint16|~uint32 — CS0315 on ushort/uint instantiations).
				fmt.Sprintf("IShiftOperators<%s, int, %s>", name, name),
			}
		case ComparableOperators:
			constraints = []string{
				fmt.Sprintf("IEqualityOperators<%s, %s, bool>", name, name),
			}
		case OrderedOperators:
			constraints = []string{
				fmt.Sprintf("IComparisonOperators<%s, %s, bool>", name, name),
			}
		default:
			constraints = []string{}
		}

		if len(constraints) > 0 {
			liftedConstraints = append(liftedConstraints, constraints...)
		}
	}

	return strings.Join(liftedConstraints, ", ")
}

// isTypeConstraint determines if an interface type or type expression represents a type constraint
func (v *Visitor) isTypeConstraint(expr ast.Expr) (bool, int) {
	// Check if we're dealing with an interface type
	if ifaceType, ok := expr.(*ast.InterfaceType); ok {
		// Empty interface{} is not a type constraint
		if len(ifaceType.Methods.List) == 0 {
			return false, 0
		}

		// Check if any method in the interface is an embedded type constraint
		for _, method := range ifaceType.Methods.List {
			// If there's no name, it's an embedded type
			if len(method.Names) == 0 {
				// Check if this embedded type is a constraint
				if ok, count := v.exprIsTypeConstraint(method.Type); ok {
					return true, count
				}
			}
		}

		return false, 0
	}

	// If it's not an interface type, check if it's a constraint directly
	return v.exprIsTypeConstraint(expr)
}

// exprIsTypeConstraint determines if a expression represents a type constraint
func (v *Visitor) exprIsTypeConstraint(expr ast.Expr) (bool, int) {
	switch t := expr.(type) {
	case *ast.UnaryExpr:
		// Check for the ~ operator, an approximate type constraint
		return t.Op == token.TILDE, 0

	case *ast.BinaryExpr:
		// If it's a binary expression with | operator, it's a union type constraint
		return t.Op == token.OR, 0

	case *ast.StarExpr:
		// A pointer type literal (`interface{ *T }`) is a type-set term, not an embeddable
		// interface — without this it rode the embedded-interface path and emitted an interface
		// inheriting the struct ж<T> (CS0527). The declaration keeps the constraint-comment
		// convention; its USE sites erase (see pointerCoreConstraint).
		return true, 0

	case *ast.Ident, *ast.SelectorExpr:
		// For named types, we check if they're type constraints
		// by looking at their type definition
		obj := v.info.TypeOf(expr)

		if obj != nil {
			if ok, count := v.typeIsTypeConstraint(obj); ok {
				return true, count
			}
		}
	}

	return false, 0
}

// isConstraintInterface checks if an interface represents a type constraint
func (v *Visitor) isConstraintInterface(iface *types.Interface) (bool, int) {
	for i := range iface.NumEmbeddeds() {
		if ok, _ := v.typeIsTypeConstraint(iface.EmbeddedType(i)); ok {
			return true, iface.NumMethods()
		}
	}

	return false, 0
}

// typeIsTypeConstraint determines if a type represents a type constraint
func (v *Visitor) typeIsTypeConstraint(typ types.Type) (bool, int) {
	switch t := typ.(type) {
	case *types.Interface:
		return v.isConstraintInterface(t)

	case *types.Union:
		// Union types are always constraints
		return true, 0

	case *types.Named:
		// For a named type, recursively check its underlying type
		return v.typeIsTypeConstraint(t.Underlying())

	case *types.TypeParam:
		// Type parameters themselves aren’t constraints
		return false, 0

	default:
		// Any other concrete type (basic, map, slice, channel, array, etc.)
		// is a valid type literal constraint
		return true, 0
	}
}

// getConstraintTypeSetFromExpr collects all underlying type constraints
func (v *Visitor) getConstraintTypeSetFromExpr(expr ast.Expr) hashset.HashSet[ConstraintType] {
	var results []types.Type

	// Helper to process expressions recursively
	var process func(ast.Expr)

	process = func(e ast.Expr) {
		switch t := e.(type) {
		case *ast.UnaryExpr:
			// For ~ operator, we need to process the underlying type
			// meaning "any type whose underlying type is X"
			if t.Op == token.TILDE {
				// Get the type information for the operand
				operandType := v.info.TypeOf(t.X)

				if operandType == nil {
					// Fallback to processing the expression directly
					process(t.X)
				} else {
					results = append(results, operandType)
				}

				return
			}

		case *ast.BinaryExpr:
			if t.Op == token.OR {
				// Process both sides of the OR expression
				process(t.X)
				process(t.Y)
				return
			}

		case *ast.Ident:
			obj := v.info.Uses[t]
			if typeName, ok := obj.(*types.TypeName); ok {
				typ := typeName.Type()
				underlying := typ.Underlying()

				// Check for composite types directly
				switch underlying.(type) {
				case *types.Pointer, *types.Array, *types.Chan, *types.Struct:
					// These are valid constraint types, add them directly
					results = append(results, typ)
					return
				case *types.Basic:
					// It's a basic type, add it directly
					results = append(results, typ)
					return
				}

				if iface, isIface := underlying.(*types.Interface); isIface && !iface.Empty() {
					// It's an interface type set, so recursively extract its embedded constraints
					results = append(results, v.getConstraintsFromType(typ)...)
				}
			}

		case *ast.SelectorExpr:
			sel := v.info.Uses[t.Sel]
			if typeName, ok := sel.(*types.TypeName); ok {
				typ := typeName.Type()
				underlying := typ.Underlying()

				// Check if it's a composite type
				switch underlying.(type) {
				case *types.Pointer, *types.Array, *types.Chan, *types.Struct:
					// Composite types are valid constraints
					results = append(results, typ)
				default:
					results = append(results, typ)
				}
			}

		case *ast.InterfaceType:
			for _, method := range t.Methods.List {
				if len(method.Names) == 0 {
					// It's an embedded type
					process(method.Type)
				}
			}

		// Handle composite types in AST directly
		case *ast.StarExpr, *ast.ArrayType, *ast.ChanType, *ast.StructType:
			// If we encounter one of these directly in the AST, try to get its type
			if typeInfo := v.info.TypeOf(t); typeInfo != nil {
				results = append(results, typeInfo)
			}
		default:
			results = append(results, types.Default(nil))
		}
	}

	process(expr)

	// Convert the type results to a HashSet of ConstraintType
	return getConstraintTypeSet(results)
}

// getConstraintsFromType recursively collects the concrete underlying types from a given types.Type,
// ensuring that union types (and similar constructs) are fully expanded.
func (v *Visitor) getConstraintsFromType(typ types.Type) []types.Type {
	var constraints []types.Type

	switch t := typ.(type) {
	case *types.Named:
		// Check if the underlying type is a composite type
		underlying := t.Underlying()
		switch underlying.(type) {
		case *types.Pointer, *types.Array, *types.Chan, *types.Struct:
			// Composite types are valid constraints, add the named type directly
			constraints = append(constraints, t)
		default:
			// For other named types, continue recursing into the underlying type
			constraints = append(constraints, v.getConstraintsFromType(underlying)...)
		}

	case *types.Interface:
		// If it's an interface type set, iterate over embedded types.
		// This covers the case where the interface inherits constraints.
		if !t.Empty() {
			for i := range t.NumEmbeddeds() {
				embedded := t.EmbeddedType(i)
				constraints = append(constraints, v.getConstraintsFromType(embedded)...)
			}
		} else {
			// Otherwise, treat the interface itself as a constraint.
			constraints = append(constraints, t)
		}

	case *types.Union:
		// Instead of returning the union type directly, recursively extract its terms.
		for i := range t.Len() {
			term := t.Term(i) // Each term has a .Type field.
			constraints = append(constraints, v.getConstraintsFromType(term.Type())...)
		}

	case *types.Pointer, *types.Array, *types.Chan, *types.Struct:
		// These composite types are valid constraints, add them directly
		constraints = append(constraints, t)

	default:
		// For any other concrete type (basic, etc.), add it directly.
		constraints = append(constraints, t)
	}

	return constraints
}

func (v *Visitor) getConstraintTypeSetFromType(typ types.Type) hashset.HashSet[ConstraintType] {
	return getConstraintTypeSet(v.getConstraintsFromType(typ))
}

// constraintTypeSetIsInexpressible reports whether a constraint names a real Go type set that no C#
// `where` clause can express — every term is a COMPOSITE kind (struct, array, pointer, channel), so
// the terms share no liftable operator (see comparableOperatorTypes) and no single golib interface
// covers them.
//
// It is deliberately the LAST question getGenericDefinition asks, after every shape with an answer
// (`string | []byte`, slice, array-core, map, channel, func, the bare `string`/`[]byte`/`comparable`
// cases) has been tried, so it never intercepts a constraint that has an emission. An EMPTY type set
// is not this: that is a method-set or unconstrained interface, which the arms after the chain
// handle on their own terms.
func (v *Visitor) constraintTypeSetIsInexpressible(constraint types.Type) bool {
	typeSet := v.getConstraintTypeSetFromType(constraint)

	if typeSet.IsEmpty() {
		return false
	}

	return getOperatorSet(typeSet).IsEmpty()
}

func getConstraintTypeSet(constraintTypes []types.Type) hashset.HashSet[ConstraintType] {
	// Convert the type results to a HashSet of ConstraintType
	constraintTypeSet := hashset.HashSet[ConstraintType]{}

	for _, typ := range constraintTypes {
		switch t := typ.(type) {
		case *types.Basic:
			switch t.Kind() {
			case types.Bool:
				constraintTypeSet.Add(Bool)
			case types.Int:
				constraintTypeSet.Add(Int)
			case types.Int8:
				constraintTypeSet.Add(Int8)
			case types.Int16:
				constraintTypeSet.Add(Int16)
			case types.Int32:
				constraintTypeSet.Add(Int32)
			case types.Int64:
				constraintTypeSet.Add(Int64)
			case types.Uint:
				constraintTypeSet.Add(Uint)
			case types.Uint8:
				constraintTypeSet.Add(Uint8)
			case types.Uint16:
				constraintTypeSet.Add(Uint16)
			case types.Uint32:
				constraintTypeSet.Add(Uint32)
			case types.Uint64:
				constraintTypeSet.Add(Uint64)
			case types.Float32:
				constraintTypeSet.Add(Float32)
			case types.Float64:
				constraintTypeSet.Add(Float64)
			case types.Complex64:
				constraintTypeSet.Add(Complex64)
			case types.Complex128:
				constraintTypeSet.Add(Complex128)
			case types.String:
				constraintTypeSet.Add(String)
			}
		case *types.Pointer:
			constraintTypeSet.Add(Pointer)
		case *types.Array:
			// Arrays are comparable in Go when their element type is comparable.
			constraintTypeSet.Add(Array)
		case *types.Slice:
			// Slices are not comparable in Go (only against nil), so they
			// contribute no operator-bearing constraint type.
		case *types.Chan:
			constraintTypeSet.Add(Channel)
		case *types.Struct:
			constraintTypeSet.Add(Struct)
		case *types.Named:
			// For named types, check it they are structs
			if _, ok := t.Underlying().(*types.Struct); ok {
				constraintTypeSet.Add(Struct)
			}
		default:
			constraintTypeSet.Add(Invalid)
		}
	}

	return constraintTypeSet
}

// pointerCoreConstraint reports whether the type parameter's constraint type-set is a single,
// non-tilde pointer term (`[P *T]`), returning the pointer type. Such a term's type set is a
// singleton — P is definitionally its pointer type at every instantiation (Go spec, "Interface
// types") — so the emission ERASES P: it is dropped from the C# generic parameter list and every
// occurrence renders as the pointer type itself (`ж<T>`), letting the normal pointer machinery
// (deref alias, escape heap box, argument passing) apply unchanged. Approximate (`~*T`), union,
// and method-carrying pointer constraints are declined (zero stdlib occurrences; callers keep
// the current emission and warn): a named pointer type emits as a `[GoType("ж<E>")]` wrapper
// class, which is not identity with `ж<E>`. See DESIGN-pointer-core-typeparam.md.
func pointerCoreConstraint(typeParam *types.TypeParam) (*types.Pointer, bool) {
	if typeParam == nil {
		return nil, false
	}

	iface, ok := typeParam.Constraint().Underlying().(*types.Interface)

	if !ok {
		return nil, false
	}

	return interfacePointerTerm(iface, 0)
}

// interfacePointerTerm reports whether the interface's type-set is a single non-tilde pointer
// term, unwrapping a NAMED (or aliased) constraint interface embedded inside another —
// `interface{ PtrOf[X] }` must resolve identically to the direct `PtrOf[X]` spelling (both name
// the same singleton type set). The depth cap guards degenerate self-referential constraint
// shapes; real constraints nest one or two levels.
func interfacePointerTerm(iface *types.Interface, depth int) (*types.Pointer, bool) {
	if depth > 8 || iface.NumMethods() > 0 || iface.NumEmbeddeds() != 1 {
		return nil, false
	}

	// The single embedded element is the term type directly, a one-term union (go/types wraps
	// explicit terms in a Union; both shapes appear in practice), or a named constraint interface.
	switch embedded := iface.EmbeddedType(0).(type) {
	case *types.Union:
		if embedded.Len() != 1 || embedded.Term(0).Tilde() {
			return nil, false
		}

		pointer, ok := embedded.Term(0).Type().(*types.Pointer)
		return pointer, ok

	case *types.Pointer:
		return embedded, true

	default:
		if nested, ok := embedded.Underlying().(*types.Interface); ok {
			return interfacePointerTerm(nested, depth+1)
		}
	}

	return nil, false
}

// constraintHasPointerTerm reports whether the type parameter's constraint type-set mentions any
// pointer term at all. Used only to WARN about the pointer-core shapes pointerCoreConstraint
// declines to erase (approximate `~*T`, unions, method-carrying interfaces) — those keep the
// operator-lift fallback emission, which cannot express pointer semantics on the parameter.
func constraintHasPointerTerm(typeParam *types.TypeParam) bool {
	iface, ok := typeParam.Constraint().Underlying().(*types.Interface)

	if !ok {
		return false
	}

	return interfaceHasPointerTerm(iface, 0)
}

// constraintIsMethodSetWithPointerNamedUnion is isMethodSetWithPointerNamedUnion asked of a type parameter's
// constraint. getGenericDefinition's pointer-constraint WARNING consults it, so the one shape the declaration and the
// proxy now emit on purpose (fips140's `Point[P]`, RED 8) stops reporting "emission may not compile": that warning
// named RED 8 at every site since the hop (29 for 29, crypto/elliptic 0), and after the cure its correct count is 0.
func constraintIsMethodSetWithPointerNamedUnion(typeParam *types.TypeParam) bool {
	iface, ok := typeParam.Constraint().Underlying().(*types.Interface)

	return ok && isMethodSetWithPointerNamedUnion(iface)
}

func interfaceHasPointerTerm(iface *types.Interface, depth int) bool {
	if depth > 8 {
		return false
	}

	for i := range iface.NumEmbeddeds() {
		switch embedded := iface.EmbeddedType(i).(type) {
		case *types.Union:
			for j := range embedded.Len() {
				term := embedded.Term(j).Type()

				if _, ok := term.(*types.Pointer); ok {
					return true
				}

				// ⚠ A UNION TERM CAN ITSELF BE A NAMED CONSTRAINT INTERFACE, and the pointer then
				// sits one level further in: runtime's `mapBenchmarkElemType interface {
				// mapBenchmarkKeyType | []int32 }` reaches `*int32` only through the first term.
				// Testing each term for *types.Pointer alone made this function answer FALSE for a
				// type set that plainly mentions a pointer — against its own doc comment — so the
				// warning above under-reported and the constraint arm that consults it kept
				// `new()` on 14 of the row's 28 sites. Recurse, under the same depth cap.
				if nested, ok := term.Underlying().(*types.Interface); ok && interfaceHasPointerTerm(nested, depth+1) {
					return true
				}
			}
		case *types.Pointer:
			return true
		default:
			if nested, ok := embedded.Underlying().(*types.Interface); ok && interfaceHasPointerTerm(nested, depth+1) {
				return true
			}
		}
	}

	return false
}

// collectErasedTypeParams returns the pointer-core (erased) type parameters of a FUNCTION's own
// type-parameter list, keyed by identity — nil when none. Populated into v.erasedTypeParams at
// visitFuncDecl entry: erasure is strictly a property of a plain function declaration (a generic
// NAMED type's parameters — including a method's receiver type parameters — are never erased, so
// every consumer gates on this identity set rather than re-deriving from the constraint alone,
// which would half-erase declined shapes).
func collectErasedTypeParams(signature *types.Signature) map[*types.TypeParam]*types.Pointer {
	if signature == nil {
		return nil
	}

	typeParams := signature.TypeParams()

	if typeParams == nil {
		return nil
	}

	var erased map[*types.TypeParam]*types.Pointer

	for i := range typeParams.Len() {
		typeParam := typeParams.At(i)

		if pointer, ok := pointerCoreConstraint(typeParam); ok {
			if erased == nil {
				erased = map[*types.TypeParam]*types.Pointer{}
			}

			erased[typeParam] = pointer
		}
	}

	return erased
}

// typeParamErased reports whether the type parameter is ERASED in the current emission context —
// i.e. it belongs to the function declaration being emitted and its constraint is a single
// non-tilde pointer term. Identity-keyed, so a same-named parameter of some other declaration
// (a generic named type, a named func type, another function) never matches.
func (v *Visitor) typeParamErased(typeParam *types.TypeParam) (*types.Pointer, bool) {
	pointer, ok := v.erasedTypeParams[typeParam]
	return pointer, ok
}

// paramPointerType resolves a type's pointer form for parameter/ident classification: a
// *types.Pointer directly, or the erased pointer of a pointer-core type parameter of the CURRENT
// function (see typeParamErased) — so a `p P` parameter under `[P *T]` takes the same deref-alias
// and box (`Ꮡ`) conventions as a plain `p *T` parameter.
func (v *Visitor) paramPointerType(t types.Type) (*types.Pointer, bool) {
	if pointer, ok := t.(*types.Pointer); ok {
		return pointer, true
	}

	if typeParam, ok := types.Unalias(t).(*types.TypeParam); ok {
		return v.typeParamErased(typeParam)
	}

	return nil, false
}

// signatureErasedParamPointer reports the erased pointer behind a CALLEE's declared parameter
// type: a pointer-core type parameter counts only when the signature ITSELF declares it (its own
// TypeParams list — a method's receiver type parameters are never erased, so an argument landing
// in such a slot keeps the plain render).
func signatureErasedParamPointer(sig *types.Signature, t types.Type) (*types.Pointer, bool) {
	typeParam, ok := types.Unalias(t).(*types.TypeParam)

	if !ok || sig == nil {
		return nil, false
	}

	typeParams := sig.TypeParams()

	if typeParams == nil {
		return nil, false
	}

	for i := range typeParams.Len() {
		if typeParams.At(i) == typeParam {
			return pointerCoreConstraint(typeParam)
		}
	}

	return nil, false
}

// signatureErasedParamPointerOk is the boolean form of signatureErasedParamPointer for use in
// compound conditions.
func signatureErasedParamPointerOk(sig *types.Signature, t types.Type) bool {
	_, ok := signatureErasedParamPointer(sig, t)
	return ok
}

// typeIsErasedPointerCore reports whether t is an ERASED pointer-core type parameter of the
// current function (the boolean form of typeParamErased over a types.Type).
func (v *Visitor) typeIsErasedPointerCore(t types.Type) bool {
	typeParam, ok := types.Unalias(t).(*types.TypeParam)

	if !ok {
		return false
	}

	_, erased := v.typeParamErased(typeParam)
	return erased
}

// explicitTypeArgsAfterErasure filters an EXPLICIT Go instantiation's written type-argument
// expressions (`clone[*thing, thing]`, `setThrough[*int](…)` — including partial lists), removing
// positions whose declared callee type parameter is erased (pointer-core): those positions no
// longer exist in the emitted C# generic parameter list, so rendering them verbatim is an arity
// mismatch (CS0305) or a mis-bound argument (CS1503 on a partial list). A non-function target, an
// unresolvable base, or a callee with nothing erased returns the original slice unchanged (false)
// — those paths stay byte-identical.
func (v *Visitor) explicitTypeArgsAfterErasure(x ast.Expr, indices []ast.Expr) ([]ast.Expr, bool) {
	var funIdent *ast.Ident

	switch e := x.(type) {
	case *ast.Ident:
		funIdent = e
	case *ast.SelectorExpr:
		funIdent = e.Sel
	}

	if funIdent == nil {
		return indices, false
	}

	funcObj, ok := v.info.ObjectOf(funIdent).(*types.Func)

	if !ok {
		return indices, false
	}

	sig, ok := funcObj.Type().(*types.Signature)

	if !ok || sig.TypeParams() == nil {
		return indices, false
	}

	typeParams := sig.TypeParams()
	kept := make([]ast.Expr, 0, len(indices))
	erasedAny := false

	for i, index := range indices {
		if i < typeParams.Len() {
			if _, erased := pointerCoreConstraint(typeParams.At(i)); erased {
				erasedAny = true
				continue
			}
		}

		kept = append(kept, index)
	}

	if !erasedAny {
		return indices, false
	}

	return kept, true
}

// renderedTypeArgs renders an instantiation's type arguments for emission, skipping positions
// whose declared type parameter is erased (pointer-core, see pointerCoreConstraint) — those no
// longer exist in the emitted C# generic parameter list. funIdent resolves the callee's declared
// type-parameter list; the result is always non-nil — an EMPTY list means every position was
// erased (emit no `<...>`).
func (v *Visitor) renderedTypeArgs(funIdent *ast.Ident, typeArgs *types.TypeList) []string {
	typeParams := v.signatureTypeParams(funIdent)

	// THE COMPANION'S USE. `reflect.TypeFor[T]()` inside a generic declaration that threads T reads
	// the COMPANION, never T: T is the value's type and stays exactly what it was (`object` for an
	// erased alias), while the companion is the carrier the call site bound. This is the only
	// substitution the arc makes to an existing rendering, and it is confined to the one callee that
	// reads a Go NAME out of a static type — see descriptorCompanion.go on why the identity surface
	// (`abi.TypeFor`) is deliberately not touched.
	if len(v.currentFuncCompanionNames) > 0 && typeArgs.Len() == 1 && isReflectTypeForSelector(v.info, funIdent) {
		if typeParam, isTypeParam := typeArgs.At(0).(*types.TypeParam); isTypeParam {
			if companion, hasCompanion := v.currentFuncCompanionNames[typeParam]; hasCompanion {
				return []string{companion}
			}
		}
	}

	names := make([]string, 0, typeArgs.Len())

	for i := range typeArgs.Len() {
		if typeParams != nil && i < typeParams.Len() {
			if _, erased := pointerCoreConstraint(typeParams.At(i)); erased {
				continue
			}
		}

		// A pointer argument against a SELF-REFERENTIAL generic method-set constraint renders as
		// its constraint proxy, not as the box: `where P : nistPoint<P>` is a nominal C# bound the
		// golib box can never satisfy (see constraintProxyFor).
		if proxyName, ok := v.constraintProxySigArg(funIdent, typeArgs, i); ok {
			names = append(names, proxyName)
			continue
		}

		// A pointer argument against a NON-self-referential method-set constraint, reached only
		// as a func RESULT, renders as the constraint itself: the call site projects each such
		// delegate through the pointer adapter (see funcResultProjection).
		if _, constraint, ok := v.funcResultProjection(funIdent, typeArgs, i); ok {
			names = append(names, v.getCSharpTypeName(constraint))
			continue
		}

		names = append(names, v.getCSharpTypeName(typeArgs.At(i)))
	}

	// THE COMPANION'S SUPPLY. A callee that threads gains one argument per name-reading parameter,
	// appended in declared order after every real argument — the DESCRIPTOR CARRIER when the Go type
	// argument is a defined-over-interface type the emission erased to a `using` alias, and the
	// argument's OWN rendering otherwise. The second half is what keeps an unerased instantiation
	// byte-identical in meaning to before the thread: `testHandle<testString>` becomes
	// `testHandle<testString, testString>`, and `reflect.TypeFor` inside answers exactly what it
	// answered when it read T.
	if funcObj, isFunc := v.info.ObjectOf(funIdent).(*types.Func); isFunc {
		for _, index := range v.descriptorCompanionParams(funcObj) {
			if index >= typeArgs.Len() {
				continue
			}

			if typeParams != nil && index < typeParams.Len() {
				if _, erased := pointerCoreConstraint(typeParams.At(index)); erased {
					continue
				}
			}

			typeArg := typeArgs.At(index)

			if carrier := v.descriptorCarrierFor(typeArg); carrier != "" {
				names = append(names, carrier)
				continue
			}

			names = append(names, v.getCSharpTypeName(typeArg))
		}
	}

	return names
}

// completedInstantiationTypeArgs renders the FULL resolved type-argument list for an explicit Go
// instantiation whose written list is PARTIAL — fewer arguments than the emitted C# generic
// parameter list has positions. Go allows a prefix (`Equal[Slice]` against `Equal[S ~[]E, E
// comparable]`, slices' own iter_test) and infers the rest through core types; C# has no partial
// instantiation at all, so the written prefix emits as `Equal<Slice>` against a two-parameter
// method — CS0305, "requires 2 type arguments". go/types already resolved the whole list into
// info.Instances keyed by the base's name ident.
//
// Returns nil — leaving the written list to render verbatim, byte for byte as before — whenever
// the instantiation is COMPLETE (the overwhelmingly common case), the base is not a generic
// function, or nothing was recorded. writtenCount is the count AFTER erasure filtering
// (explicitTypeArgsAfterErasure), and the rendered list is filtered the same way, so an erased
// pointer-core position cannot make a complete instantiation look partial.
func (v *Visitor) completedInstantiationTypeArgs(x ast.Expr, writtenCount int) []string {
	var funIdent *ast.Ident

	switch e := x.(type) {
	case *ast.Ident:
		funIdent = e
	case *ast.SelectorExpr:
		funIdent = e.Sel
	}

	if funIdent == nil {
		return nil
	}

	if _, isFunc := v.info.ObjectOf(funIdent).(*types.Func); !isFunc {
		return nil
	}

	inst, ok := v.info.Instances[funIdent]

	if !ok || inst.TypeArgs == nil {
		return nil
	}

	rendered := v.renderedTypeArgs(funIdent, inst.TypeArgs)

	if len(rendered) <= writtenCount {
		return nil
	}

	return rendered
}

func (v *Visitor) getConstraintType(typeConstraint *types.TypeParam) types.Type {
	if typeConstraint == nil {
		return nil
	}

	// Get the constraint
	constraint := typeConstraint.Constraint()

	// The constraint is typically an interface type
	iface, ok := constraint.Underlying().(*types.Interface)

	if !ok {
		return nil
	}

	typeConstraints := v.getConstraintsFromType(iface)

	if len(typeConstraints) > 0 {
		return typeConstraints[0]
	}

	return nil
}

// getArrayConstraintElem reports whether the constraint interface's type-set is a single array
// core (`~[N]E` / `[N]E`, e.g. ML-KEM's `~[256]fieldElement`), returning the shared element type.
// A named-array `[GoType]` wrapper (ringElement, nttElement) implements golib's IArray<E>, so such
// a constraint maps to `where T : IArray<E>` — which exposes the array surface (indexing, length,
// `(nint, E)` ranging) the generic body needs — rather than the spurious IEqualityOperators<T,T,bool>
// the Array member of the comparable operator set would otherwise lift (CS0315: the array-wrapper
// struct cannot satisfy it, and it exposes no indexer/enumerator for the body's `t[i]`/`range t`).
func (v *Visitor) getArrayConstraintElem(iface *types.Interface) (types.Type, bool) {
	if iface == nil {
		return nil, false
	}

	constraints := v.getConstraintsFromType(iface)

	if len(constraints) == 0 {
		return nil, false
	}

	var elem types.Type

	for _, constraint := range constraints {
		array, ok := constraint.Underlying().(*types.Array)

		if !ok {
			return nil, false
		}

		if elem == nil {
			elem = array.Elem()
		} else if !types.Identical(elem, array.Elem()) {
			// A union of array cores with differing element types has no single IArray<E>.
			return nil, false
		}
	}

	return elem, true
}

// ---- Generic definitions and constraint proxies ----
//
// getGenericDefinition renders a Go generic type's C# definition — its type parameter list plus
// the `where` clauses the constraints translate into. constraintProxyArg and its predicate handle
// the case C# cannot express directly: a Go constraint that admits a POINTER type, which the
// converter routes through a generated proxy rather than a `where` clause.

func (v *Visitor) getGenericDefinition(srcType types.Type) (string, string) {
	var named *types.Named
	var signature *types.Signature
	var ok bool

	if named, ok = srcType.(*types.Named); !ok {
		if signature, ok = srcType.(*types.Signature); !ok {
			return "", ""
		}
	}

	var typeParams *types.TypeParamList

	if named != nil {
		typeParams = named.TypeParams()
	} else {
		typeParams = signature.TypeParams()

		if typeParams == nil {
			typeParams = signature.RecvTypeParams()
		}
	}

	if typeParams == nil || typeParams.Len() == 0 {
		return "", ""
	}

	typeParamNames := make([]string, typeParams.Len())
	erasedParams := make([]bool, typeParams.Len())
	constraintNames := []string{}

	// Pointer-core ERASURE applies to FUNCTION type parameters only (`func clone[P *T, T any]`):
	// erasing a generic NAMED type's parameter would change the type's emitted arity at every
	// instantiation site — a surface with zero stdlib occurrences, declined with a warning until
	// a real case exists. Receiver type parameters (a generic type's method) belong to the named
	// type and are equally excluded, as is a named generic FUNC TYPE's list (not the function
	// declaration being emitted — the renderer's identity set wouldn't cover it).
	eraseAllowed := named == nil && signature == v.currentFuncSignature && typeParams == signature.TypeParams()

	for i := range typeParams.Len() {
		typeParam := typeParams.At(i)
		typeParamNames[i] = typeParam.Obj().Name()

		// A type parameter named after a C# 15 reserved type-position keyword is Δ-renamed package-wide
		// (csReservedTypeNames); its uses already spell through getSanitizedIdentifier, so the declaration
		// must too. Scoped to that set so no other type parameter's spelling moves.
		if csReservedTypeNames.Contains(typeParamNames[i]) {
			typeParamNames[i] = getSanitizedIdentifier(typeParamNames[i])
		}

		// A single non-tilde pointer term (`[P *T]`) has a singleton type set — P is
		// definitionally *T — so P is erased: dropped from the emitted `<...>` list and `where`
		// clauses, rendering inline as `ж<T>` everywhere it appears (see the getAliasQualifiedTypeName arm and
		// pointerCoreConstraint). A breadcrumb comment keeps the Go constraint visible. Declined
		// pointer-core shapes (approximate `~*T`, unions, generic named types) warn instead of
		// silently mis-emitting the operator-lift fallback.
		if pointer, ok := pointerCoreConstraint(typeParam); ok {
			if eraseAllowed {
				erasedParams[i] = true
				csForm := fmt.Sprintf("%s<%s>", PointerPrefix, convertToCSTypeName(v.getAliasQualifiedTypeName(pointer.Elem(), false)))
				constraintNames = append(constraintNames, fmt.Sprintf("%s%s    /* where %s : %s (erased: %s renders as %s) */",
					v.newline, v.indent(v.indentLevel), typeParamNames[i], v.getAliasQualifiedTypeName(pointer, false), typeParamNames[i], csForm))
				continue
			}

			v.showWarning("@getGenericDefinition - pointer-core constraint `%s` on generic type `%s` is not erased (no stdlib precedent); emission may not compile", v.getAliasQualifiedTypeName(pointer, false), srcType.String())
		} else if constraintHasPointerTerm(typeParam) && !constraintIsMethodSetWithPointerNamedUnion(typeParam) {
			v.showWarning("@getGenericDefinition - approximate/union/method-carrying pointer constraint `%s` on `%s` is not erased; emission may not compile", typeParam.Constraint().String(), srcType.String())
		}

		constraint := typeParam.Constraint()
		var constraintName string

		// Check if the constraint type is an anonymous interface
		if _, ok := constraint.(*types.Interface); ok {
			constraintName = constraint.String()
		} else {
			constraintName = v.getAliasQualifiedTypeName(constraint, false)
		}

		if len(constraintName) == 0 || constraintName == "any" || constraintName == "interface{}" {
			// An unconstrained (`any`) type parameter gets NO C# constraint. Previously `new()` was
			// added (so `@new<T>`/`make` could construct it, and to force `@string` over `System.String`
			// for generic string args). But `new()` rejects a delegate/func type argument — Go's
			// `atomic.Pointer[func()]` is valid yet `Pointer<Action>` failed CS0310 — and it is no
			// longer required: golib `@new<T>` constructs via the runtime (no new() bound), and string
			// literals are cast to `@string` at generic call sites. Leave it unconstrained.
			constraintName = ""
		} else {
			var iface *types.Interface

			switch typ := constraint.(type) {
			case *types.Interface:
				iface = typ
			case *types.Named:
				iface = typ.Underlying().(*types.Interface)
			case *types.Signature:
				iface = typ.Recv().Type().Underlying().(*types.Interface)
			default:
				iface = nil
			}

			if iface != nil {
				originalConstraint := fmt.Sprintf("/* %s */", constraintName)
				constraintName = strings.TrimPrefix(strings.TrimSpace(constraintName), "~")
				constraintExpr := strings.ReplaceAll(constraintName, " ", "")
				var typeConstraint string
				// `string | []byte` union members share no operators, so suppress the spurious lifted
				// operator constraints (IAddition/IComparison/...) for it; set in the union branch below.
				suppressLiftedConstraints := false

				// Check for common Go types, e.g., slice, map, channel, etc. The `string | []byte`
				// UNION is checked FIRST: its `[]byte | string` ordering starts with "[]" and would
				// otherwise take the ISlice branch with the raw union as the element type
				// (`ISlice<byte | string>` — CS1003 cascade, time/format.go's appendNano family).
				if constraintExpr == "string|[]byte" || constraintExpr == "[]byte|string" {
					// Go's `string | []byte` union — emit the read-only byte-sequence interface
					// both @string and slice<byte> implement; C# cannot express the "or" directly.
					// SELF-REFERENTIAL in the type parameter (`IByteSeq<bytes, byte>`, the CRTP
					// shape) so the sub-slice indexer returns the type parameter ITSELF: Go bodies
					// sub-slice these values constantly, and an interface-typed sub-slice result
					// boxed the concrete struct on every one. See golib IByteSeq.
					typeConstraint = fmt.Sprintf("IByteSeq<%s, byte>", typeParamNames[i])
					suppressLiftedConstraints = true
				} else if strings.HasPrefix(constraintExpr, "[]") {
					// Handle slice via ISlice interface. ISliceWrap supplies the S-preserving factory:
					// a sub-slice or append of a constrained S must yield S again (Go's named-slice
					// semantics), which golib's subslice<S, T>/append<S, T> realize through S.Wrap.
					elemType := convertToCSTypeName(constraintName[2:])
					typeConstraint = fmt.Sprintf("ISlice<%s>, ISupportMake<%s>, ISliceWrap<%s, %s>", elemType, typeParamNames[i], typeParamNames[i], elemType)
				} else if arrayElem, isArrayCore := v.getArrayConstraintElem(iface); isArrayCore {
					// Handle an array-core constraint `~[N]E` (ML-KEM's `~[256]fieldElement`) via the
					// IArray interface. The named-array [GoType] wrapper (ringElement, nttElement)
					// implements IArray<E>, so this exposes the array surface — indexing, length,
					// `(nint, E)` ranging/deconstruction — that the generic body binds against, and
					// the wrapper type arguments satisfy the constraint. The Array member of the
					// comparable operator set would otherwise lift IEqualityOperators<T, T, bool>,
					// which the wrapper cannot satisfy and which exposes no array surface (CS0315,
					// plus CS0021/CS1579/CS8130 on the body's `t[i]`/`range t`/deconstruction).
					elemType := convertToCSTypeName(v.getAliasQualifiedTypeName(arrayElem, false))
					typeConstraint = fmt.Sprintf("IArray<%s>", elemType)
					suppressLiftedConstraints = true
				} else if strings.HasPrefix(constraintExpr, "map[") {
					// Handle map via IMap interface
					keyValue := strings.Split(constraintName[4:], "]")
					typeConstraint = fmt.Sprintf("IMap<%s, %s>, ISupportMake<%s>", convertToCSTypeName(keyValue[0]), convertToCSTypeName(keyValue[1]), typeParamNames[i])
				} else if strings.HasPrefix(constraintExpr, "chan ") {
					// Handle channel via IChannel interface
					typeConstraint = fmt.Sprintf("IChannel<%s>, ISupportMake<%s>", convertToCSTypeName(constraintName[5:]), typeParamNames[i])
				} else if strings.HasPrefix(constraintExpr, "chan<- ") {
					// Handle send-only channel via IChannel interface
					typeConstraint = fmt.Sprintf("IChannel<%s>, ISupportMake<%s>", convertToCSTypeName(constraintName[7:]), typeParamNames[i])
				} else if strings.HasPrefix(constraintExpr, "<-chan ") {
					// Handle receive-only channel via IChannel interface
					typeConstraint = fmt.Sprintf("IChannel<%s>, ISupportMake<%s>", convertToCSTypeName(constraintName[7:]), typeParamNames[i])
				} else if strings.HasPrefix(constraintExpr, "func") {
					// TODO: Handle function
					v.showWarning("@getGenericDefinition - unhandled function constraint `%s` on `%s`", constraintName, srcType.String())
					typeConstraint = originalConstraint
				} else if strings.HasPrefix(constraintExpr, "struct") {
					// TODO: Handle struct - will need to lift struct type defintion
					v.showWarning("@getGenericDefinition - unhandled struct constraint `%s` on `%s`", constraintName, srcType.String())
					typeConstraint = originalConstraint
				} else {
					// Handle special case for string and []byte types (the union form is hoisted
					// to the head of this chain - see above)
					if constraintExpr == "string" || constraintExpr == "[]byte" {
						typeConstraint = "ISlice<byte>"
					} else if constraintExpr == "comparable" {
						// Go's built-in `comparable` admits every ==-able Go type — numerics,
						// strings, pointers, channels, comparable structs/arrays/interfaces. No C#
						// constraint can express that set: golib's comparable<T> CRTP is
						// implemented by NOTHING (every real instantiation failed — blocking
						// maps.Keys), and lifting IEqualityOperators would reject structs, which
						// Go admits. Emit NO C# constraint at all: Go's checker already validated
						// every instantiation, and emitted equality on type parameters routes
						// through AreEqual (object equality), not operator ==. The `new()` this
						// arm used to emit is gone with the B1 per-kind split — a Go pointer type
						// argument now instantiates at the abstract `ж<T>`, which no constructor
						// constraint can admit (unique's HashTrieMap[*abi.Type, any] was the
						// corpus witness), and nothing needed it: golib `@new<T>` constructs via
						// the runtime and no comparable-constrained body constructs its parameter.
						continue
					} else if !isMethodSetWithPointerNamedUnion(iface) && v.constraintTypeSetIsInexpressible(constraint) && interfaceHasPointerTerm(iface, 0) {
						// ⚠ THE SAME INEXPRESSIBLE TYPE SET, but with a POINTER term in it — and the
						// arm below keeps `new()` on the stated premise that "a composite type set
						// admits no pointer type argument, every term is a value type". runtime's
						// `mapBenchmarkKeyType = int32 | int64 | string | smallType | mediumType |
						// bigType | *int32` falsifies exactly that premise: the value terms
						// instantiate fine and the POINTER term instantiates at `ж<int32>`, which
						// is `public abstract partial class ж<T>` — and CS0310 wants a non-abstract
						// type with a public parameterless constructor. 22 of them in one row, and
						// no golib change can reach it, because the abstractness is what makes a Go
						// pointer a reference at all.
						//
						// So this takes the `comparable` arm's answer above rather than the one
						// below, for that arm's own reason, quoted because it is the same sentence:
						// "a Go pointer type argument now instantiates at the abstract ж<T>, which
						// no constructor constraint can admit". Emit NO C# constraint: Go's checker
						// validated every instantiation before conversion, so the clause has
						// nothing left to enforce, and the breadcrumb cannot ride alone (`where K :
						// /* name */` is a constraint clause with no constraint).
						//
						// The predicate is the one the WARNING at the head of this function already
						// uses — interfaceHasPointerTerm, through constraintHasPointerTerm — so
						// this is that warning's shape finally acted on rather than a second
						// spelling of it. The warning has been right at every one of these sites
						// and the emission ignored it.
						continue
					} else if !isMethodSetWithPointerNamedUnion(iface) && v.constraintTypeSetIsInexpressible(constraint) {
						// A union whose terms are all COMPOSITE Go types — runtime/pprof's
						// `[T runtime.StackRecord | runtime.MemProfileRecord |
						// runtime.BlockProfileRecord]`. Every earlier arm in this chain has been
						// tried, so there is no interface to name: the terms share no operator (see
						// comparableOperatorTypes) and no golib surface, and their only common C#
						// property is being constructible. Falling through to the generic tail below
						// would emit the Go union text verbatim as a C# constraint list
						// (`where T : runtime.StackRecord | runtime.MemProfileRecord | …`) —
						// CS1003 ×4, a syntax error rather than a type error.
						//
						// Emit the constraint as the breadcrumb comment plus `new()`, the same
						// answer the `comparable` arm above reaches for the same reason: Go's own
						// checker validated every instantiation before conversion, so the C# clause
						// has nothing left to enforce. `new()` is kept (unlike that arm) because a
						// composite type set admits no pointer type argument — every term is a
						// value type — and the generic tail would have appended it anyway.
						constraintNames = append(constraintNames, fmt.Sprintf("%s%s    where %s : %s new()", v.newline, v.indent(v.indentLevel), typeParamNames[i], originalConstraint))
						continue
					}
				}

				if iface.NumMethods() == 0 {
					// For type-constraint only interfaces, C# native types cannot directly implement
					// interface, so all base-type operator constraints must be lifted to generic type
					// constraint defintion. This can get very noisy and C# does not have a mechanism
					// to hide these constraints in partial method declarations in generated code like
					// it does for structs. For partial methods, all constraint defintions are forced
					// to match, so there is no current benefit to declaring a partial method here.
					liftedConstraints := v.getLiftedConstraints(constraint, typeParamNames[i])

					// The `string | []byte` union has no common operators; drop the spurious lifted set.
					if suppressLiftedConstraints {
						liftedConstraints = ""
					}

					if len(liftedConstraints) > 0 {
						if len(typeConstraint) == 0 {
							constraintName = fmt.Sprintf("%s %s", originalConstraint, liftedConstraints)
						} else {
							constraintName = fmt.Sprintf("%s %s, %s", originalConstraint, typeConstraint, liftedConstraints)
						}
					} else {
						if len(typeConstraint) == 0 {
							constraintName = fmt.Sprintf("%s %s", originalConstraint, constraintName)
						} else {
							constraintName = fmt.Sprintf("%s %s", originalConstraint, typeConstraint)
						}
					}
				} else if isMethodSetBeyondComparable(iface) || isMethodSetWithPointerNamedUnion(iface) {
					// A method set beside a pointer-to-named union (fips140's `Point[P]`) takes this arm
					// too: visitInterfaceType emits it as a pure method set with the union as a comment, and
					// constraintProxyFor admits it on the same predicate, so the declaration and the proxy
					// agree. Left in the composite-union arm above it rendered `/* Point[P] */ new()`, which
					// neither the box nor the proxy (no parameterless constructor) satisfies — CS0310 (RED 8).
					//
					// A REGULAR method-set interface (a pure method set, no type-term unions —
					// go/ast's `Node` in `walkList[N Node]`) is emitted arity-0 by
					// visitInterfaceType, NOT as the generic CRTP form that union+method
					// constraint interfaces take below (`ConstraintTest1<ΔT>`), so the type
					// parameter constrains against the interface itself (`where N : Node` —
					// the phantom `Node<N>` was CS0308). NO `new()` either: the instantiation
					// may itself be an INTERFACE (walkList takes N=Stmt/Expr/Spec/Decl), which
					// cannot satisfy a constructor constraint.
					//
					// An embedded `comparable` is discounted for exactly the reason the bare-constraint
					// arm above emits nothing for it: it is not expressible in C#, so it cannot be what
					// makes an otherwise-method-set interface generic (see isMethodSetBeyondComparable).
					constraintNames = append(constraintNames, fmt.Sprintf("%s%s    where %s : %s", v.newline, v.indent(v.indentLevel), typeParamNames[i], convertToCSTypeName(constraintName)))
					continue
				} else {
					// If interface has methods, can safely assume generic type must implement it directly
					constraintName = fmt.Sprintf("%s<%s>", constraintName, typeParamNames[i])
				}

				constraintName = fmt.Sprintf("%s, new()", constraintName)
			} else {
				v.showWarning("@getGenericDefinition - constraint `%s` on `%s` is not an interface", constraintName, srcType.String())
			}
		}

		// An unconstrained type parameter emits no `where` clause at all (the type-param name still
		// appears in the `<…>` list above).
		if len(constraintName) == 0 {
			continue
		}

		constraintNames = append(constraintNames, fmt.Sprintf("%s%s    where %s : %s", v.newline, v.indent(v.indentLevel), typeParamNames[i], constraintName))
	}

	// Erased (pointer-core) parameters leave the emitted list; a list that erases to empty emits
	// no `<...>` at all (the function is no longer generic in C# terms — its breadcrumb where-
	// comment still rides the constraints string).
	emittedNames := make([]string, 0, len(typeParamNames))

	for i, name := range typeParamNames {
		if !erasedParams[i] {
			emittedNames = append(emittedNames, name)
		}
	}

	// DESCRIPTOR COMPANIONS ride at the END of the list, after every declared parameter, so a call
	// site appends its extra arguments without disturbing the positions Go itself wrote — and so an
	// EXPLICIT partial instantiation (completedInstantiationTypeArgs) still counts the declared
	// positions the way Go does. Ascending by declared index (descriptorCompanionParams sorts), and
	// UNCONSTRAINED by construction: the companion is only ever bound to an uninhabited carrier
	// interface or to the type argument itself, and any `where` clause naming the real parameter's
	// constraint would reject one or the other. The map is keyed by this declaration's OWN type
	// parameter objects, so a generic named type's list reaching here finds nothing.
	if len(v.currentFuncCompanionNames) > 0 {
		for i := range typeParams.Len() {
			if erasedParams[i] {
				continue
			}

			if companion, hasCompanion := v.currentFuncCompanionNames[typeParams.At(i)]; hasCompanion {
				emittedNames = append(emittedNames, companion)
			}
		}
	}

	if len(emittedNames) == 0 {
		return "", strings.Join(constraintNames, "")
	}

	return fmt.Sprintf("<%s>", strings.Join(emittedNames, ", ")), strings.Join(constraintNames, "")
}

// constraintProxyArg reports the C# constraint-proxy type name to render for type argument i of an
// instantiated generic `named`, when that argument is a POINTER to a named type AND the matching
// type parameter carries a SELF-REFERENTIAL generic method-set interface constraint — Go's
// `nistCurve[Point nistPoint[Point]]` instantiated with `*P224Point`. The golib box ж<P224Point>
// cannot NOMINALLY implement nistPoint<…> (it is a sealed golib type in another assembly, and Go's
// structural satisfaction has no C# analog), and the interface is self-referential so the value
// can't widen to the interface either. So the argument renders as the generated proxy
// `P224PointжnistPoint : nistPoint<itself>` (ImplementGenerator's EmitConstraintProxy), and this
// also registers the (element, interface) pair so package_info emits its ConstraintProxy record.
// Returns ("", false) for every other argument, leaving normal rendering untouched.
func (v *Visitor) constraintProxyArg(named *types.Named, i int) (string, bool) {
	origin := named.Origin()

	if origin == nil {
		return "", false
	}

	typeParams := origin.TypeParams()

	if typeParams == nil || i >= typeParams.Len() || named.TypeArgs() == nil || i >= named.TypeArgs().Len() {
		return "", false
	}

	return v.constraintProxyFor(typeParams.At(i), named.TypeArgs().At(i))
}

// recordNominalProductionConstraint notes a generic instantiation whose type ARGUMENT is a
// PRODUCTION type and whose type PARAMETER carries a constraint interface declared by the
// package-under-test's own `_test.go` files — the one shape the white-box reference model cannot
// serve.
//
// The reference model's premise is that interface-implementation records are RELOCATABLE: a
// production struct is foreign to the test compilation, so go2cs-gen emits a value or pointer
// ADAPTER class in the test anchor instead of a partial production struct. That holds wherever the
// interface is reached by BOXING. It does not hold here: C# checks `where P : netipTypeCmp`
// NOMINALLY, against the type argument itself, and no adapter stands in that position — the
// argument's own base list must name the interface, which only a partial declaration can add, which
// a closed referenced type forbids. `net/netip`'s `checkStringParseRoundTrip[P netipTypeCmp]`,
// called with `Addr`, `AddrPort` and `Prefix`, is CS0315 five times for exactly this reason.
//
// The constraint side reuses isMethodSetBeyondComparable — the SAME predicate getGenericDefinition
// uses to choose the nominal arm — so the gate and the emission can never disagree about which
// constraints are nominal. The interface must be TEST-declared: that is the case where the
// production compilation provably never recorded the implement, because the interface does not
// exist in it at all. A foreign package's interface is a real member of a real referenced assembly
// and a different question, deliberately not answered here.
func (v *Visitor) recordNominalProductionConstraint(typeParam *types.TypeParam, typeArg types.Type) {
	if !v.options.testWhiteboxReference || typeParam == nil || typeArg == nil {
		return
	}

	// Only a VALUE type argument reaches C#'s nominal check as itself; a pointer arrives boxed as
	// ж<T> and is the constraint-proxy machinery's business below.
	argNamed, ok := types.Unalias(typeArg).(*types.Named)

	if !ok {
		return
	}

	argObj := argNamed.Obj()

	if argObj == nil || argObj.Pkg() == nil || argObj.Pkg().Path() != v.options.packageUnderTestPath() ||
		v.declaredInTestFile(argObj) {
		return
	}

	constraintNamed, ok := types.Unalias(typeParam.Constraint()).(*types.Named)

	if !ok {
		return
	}

	constraintObj := constraintNamed.Obj()

	if constraintObj == nil || !v.declaredInTestFile(constraintObj) {
		return
	}

	iface, ok := constraintNamed.Underlying().(*types.Interface)

	if !ok || iface.NumMethods() == 0 || !isMethodSetBeyondComparable(iface) {
		return
	}

	packageLock.Lock()
	nominalProductionConstraints.Add(argObj.Name() + "|" + constraintObj.Name())
	packageLock.Unlock()
}

// constraintProxyFor is the (type parameter, type argument) core shared by every instantiation
// form. A generic NAMED TYPE reaches it through constraintProxyArg; a generic FUNCTION reaches it
// through constraintProxySigArg — Go's `benchmarkScalarMult[P nistPoint[P]]` called with
// `*P224Point` needs exactly the same proxy `nistCurve[Point nistPoint[Point]]` does, because the
// C# constraint `where P : nistPoint<P>` is nominal either way and the box cannot satisfy it.
func (v *Visitor) constraintProxyFor(typeParam *types.TypeParam, typeArg types.Type) (string, bool) {
	if typeParam == nil || typeArg == nil {
		return "", false
	}

	// The nominal-constraint gate shares this exact (type parameter, type argument) core: every
	// instantiation form routes through here, so recording it here covers both.
	v.recordNominalProductionConstraint(typeParam, typeArg)

	// The argument must be a pointer to a named type — a value type arg satisfies its constraint
	// nominally (or widens), only the boxed pointer needs the proxy. The one case where a VALUE
	// argument does not satisfy it nominally — a production type closed in a referenced assembly
	// under the white-box reference model — has no proxy answer at all and is handled by the
	// model-selection gate recorded just above.
	ptr, ok := types.Unalias(typeArg).(*types.Pointer)

	if !ok {
		return "", false
	}

	elemNamed, ok := types.Unalias(ptr.Elem()).(*types.Named)

	if !ok {
		return "", false
	}

	// The constraint must be an INSTANTIATED generic method-set interface (nistPoint[Point]);
	// a plain non-generic method-set interface (go/ast's Node) widens to itself instead.
	constraintNamed, ok := typeParam.Constraint().(*types.Named)

	if !ok || constraintNamed.TypeArgs() == nil || constraintNamed.TypeArgs().Len() == 0 {
		return "", false
	}

	iface, ok := constraintNamed.Underlying().(*types.Interface)

	if !ok || iface.NumMethods() == 0 || !(iface.IsMethodSet() || isMethodSetWithPointerNamedUnion(iface)) {
		return "", false
	}

	// Self-referential: one of the constraint's type arguments IS the type parameter itself.
	selfReferential := false

	for j := 0; j < constraintNamed.TypeArgs().Len(); j++ {
		if tp, ok := constraintNamed.TypeArgs().At(j).(*types.TypeParam); ok && tp == typeParam {
			selfReferential = true
			break
		}
	}

	if !selfReferential {
		return "", false
	}

	interfaceOrigin := constraintNamed.Origin()

	// Proxy name element-simple + PointerPrefix + interface-simple — MUST match
	// ImplementGenerator's `elementType.Name + PointerPrefix + interfaceDef.Name`.
	proxyName := elemNamed.Obj().Name() + PointerPrefix + interfaceOrigin.Obj().Name()

	// A FOREIGN constraint interface: the package that declares it OWNS the proxy. It records the pair wherever it closes
	// the constraint over the element, and ImplementGenerator makes that proxy public when both sides are, so a consumer
	// names THE OWNER'S proxy through the interface package's C# qualifier and records none of its own. A second record
	// here would mint a second class of the same simple name in this assembly, which the owner's signatures do not accept:
	// crypto/ecdsa passing crypto/internal/fips140/ecdsa's `P224()` result to its own generics was CS1503 x16, and the
	// record itself spelled the interface by its Go import path, a parse error (RED 8, COORD a4eb648a6 g2a/g2b). The
	// qualifier is the interface's C# spelling, resolved by getScopeCheckedTypeName: the file-local alias (`ecdsa.`) only when
	// this file imports the interface's package, else the fully-qualified C# name -- a consumer can reach a foreign constraint
	// through a THIRD package without importing the interface's own, and an unimported alias names nothing (CS0246). A
	// fully-qualified name that still carries the Go import path goes through convertToCSFullTypeName, so it never reaches C#.
	// No resolvable qualifier declines, leaving the box: an honest CS0310 rather than a proxy name that exists nowhere.
	if interfaceOrigin.Obj().Pkg() != v.pkg {
		qualified := v.getScopeCheckedTypeName(interfaceOrigin)

		if idx := strings.Index(qualified, "["); idx >= 0 {
			qualified = qualified[:idx]
		}

		if strings.Contains(qualified, "/") {
			qualified = convertToCSFullTypeName(qualified)
		}

		// The separator is an ASCII dot for a package-qualified C# name — but an imported TYPE ALIAS
		// spells it with TypeAliasDot (`ecdhꓸCurve`), because a C# identifier cannot carry a `.`:
		// getAliasedTypeName mints the alias form as ReplaceAll(name, ".", TypeAliasDot), so such a
		// qualifier holds NO ascii dot at all. Splitting on the ascii dot alone therefore finds -1 and
		// declines — and declines SILENTLY, which is the half that matters: the site drops to the box
		// with no diagnostic, leaving the reader of the emitted C# an unexplained CS0310.
		//
		// Take whichever separator appears LAST and carry its WIDTH with it. TypeAliasDot is THREE
		// bytes in UTF-8, so the `+1` advance that is correct for an ascii dot would slice mid-rune
		// here and hand back a corrupt qualifier that still compiles. Same alias-aware pairing
		// convCallExpr.go already uses (`Contains(name, ".") || Contains(name, TypeAliasDot)`).
		sep, dot := ".", strings.LastIndex(qualified, ".")

		if aliasDot := strings.LastIndex(qualified, TypeAliasDot); aliasDot > dot {
			sep, dot = TypeAliasDot, aliasDot
		}

		// The decline WARNS rather than falling silent, and the warning is scoped to THIS path on a
		// measurement rather than a preference: over crypto/ecdh, crypto/ecdsa and both fips140
		// siblings the shape declines above fire 210 and 10 times ("this is not the shape"), while this
		// one fires ZERO — so warning on those would flood stderr, and warning here costs nothing and
		// names a case the reader would otherwise have to diagnose from a bare CS0310.
		if dot <= 0 {
			v.showWarning("@constraintProxyFor - foreign constraint `%s` has no resolvable qualifier; the proxy is declined and the box is left in place (emission may not compile)", qualified)
			return "", false
		}

		return qualified[:dot+len(sep)] + proxyName, true
	}

	// Register the (element, interface) pair so package_info emits the ConstraintProxy record.
	// The interface name drops its type-parameter DECLARATION (`point[T any]` → `point`): the
	// record's `GoImplement<element, point<element>>` closes it over the element placeholder.
	// A CROSS-PACKAGE element renders to its C# full type name (nistec.P224Point →
	// `crypto.@internal.nistec_package.P224Point`, resolving the slash path); a SAME-PACKAGE
	// element stays BARE (convertToCSFullTypeName would root-qualify it to the wrong `go.p224`,
	// exactly as it would the local interface name below).
	elementFullName := v.getFullyQualifiedTypeName(elemNamed, false)

	if elemNamed.Obj().Pkg() != v.pkg {
		elementFullName = convertToCSFullTypeName(elementFullName)
	}

	interfaceFullName := v.getFullyQualifiedTypeName(interfaceOrigin, false)

	// Strip the type-parameter DECLARATION only. The interface is LOCAL by construction here (a foreign one returned
	// above without a record), so getFullyQualifiedTypeName yields its bare C# name (`nistPoint`), and it must NOT go
	// through convertToCSFullTypeName (which would root-qualify the bare local name to the wrong `go.nistPoint`).
	// qualifyLocalTypeRef handles final qualification at emission.
	if idx := strings.Index(interfaceFullName, "["); idx >= 0 {
		interfaceFullName = interfaceFullName[:idx]
	}

	packageLock.Lock()
	constraintProxies[elementFullName+"|"+interfaceFullName] = [2]string{elementFullName, interfaceFullName}
	packageLock.Unlock()

	return proxyName, true
}

// namedHasConstraintProxy reports whether any type argument of the instantiated generic `named`
// resolves to a self-referential constraint proxy (see constraintProxyArg) — used to re-render a
// composite-literal type through the resolved type so its type arguments match the proxy the
// pointer adapter wraps, rather than the box that convExpr's AST walk would emit.
func (v *Visitor) namedHasConstraintProxy(named *types.Named) bool {
	if named == nil || named.TypeArgs() == nil {
		return false
	}

	for i := 0; i < named.TypeArgs().Len(); i++ {
		if _, ok := v.constraintProxyArg(named, i); ok {
			return true
		}
	}

	return false
}

// signatureTypeParams resolves the declared type-parameter list of the generic FUNCTION `funIdent`
// names, or nil when it names something that is not a generic function.
func (v *Visitor) signatureTypeParams(funIdent *ast.Ident) *types.TypeParamList {
	if funIdent == nil {
		return nil
	}

	funcObj, ok := v.info.ObjectOf(funIdent).(*types.Func)

	if !ok {
		return nil
	}

	sig, ok := funcObj.Type().(*types.Signature)

	if !ok {
		return nil
	}

	return sig.TypeParams()
}

// constraintProxySigArg is constraintProxyArg for a generic FUNCTION instantiation: the same
// self-referential-constraint test against the CALLEE's declared type parameters and the resolved
// type arguments go/types recorded in info.Instances.
func (v *Visitor) constraintProxySigArg(funIdent *ast.Ident, typeArgs *types.TypeList, i int) (string, bool) {
	typeParams := v.signatureTypeParams(funIdent)

	if typeParams == nil || i >= typeParams.Len() || typeArgs == nil || i >= typeArgs.Len() {
		return "", false
	}

	return v.constraintProxyFor(typeParams.At(i), typeArgs.At(i))
}

// funcResultProjection reports whether type-parameter position `k` of the generic FUNCTION
// `funIdent` names is instantiated by a POINTER whose box cannot satisfy the emitted constraint,
// in the one reach the delegate projection can carry: the type parameter appears in the callee's
// signature ONLY as the sole result of niladic func parameters. crypto/internal/fips140's
// `hmac.New[H fips140.Hash](h func() H, key []byte)` called as `hmac.New(sha256.New, key)` is
// the shape — `New<ж<sha256.Digest>>` against `where H : fips140.Hash` is CS0311, because the
// box does not implement the interface, its generated pointer adapter does. The type argument
// becomes the constraint and each such argument is widened through the adapter (`widen<ж<Digest>,
// Hash>(sha256.New, elemᴛ0 => new sha256_DigestжHash(elemᴛ0))`) — the delegate twin of the
// slice-element projection go/ast's walkList already takes.
//
// The type argument may also be a DECLARED INTERFACE that satisfies the constraint by method set
// but does not nominally derive from it (RED 4): crypto/hkdf's `hkdf.Extract(fh, …)` with `fh :=
// fips140hash.UnwrapNew(h)`, a `func() hash.Hash`, against `Extract[H fips140.Hash]` — `hash.Hash`
// and `fips140.Hash` are SIBLINGS (identical method sets, no embedding edge), so `Extract<hash.Hash>`
// is CS0311 exactly as the box was. The converter emits Go interface embedding as C# interface
// inheritance, so "nominally derives" is "the constraint is the argument or sits in its transitive
// embedding closure"; either declines. Otherwise the wrap is the INTERFACE adapter the generator
// already mints for the pair (`new hash_HashᴠHash(elemᴛ0)`), chosen by convertToInterfaceType.
//
// Returns the type argument (a pointer or an interface) and the INSTANTIATED constraint. A
// self-referential constraint is the constraint proxy's (see constraintProxyFor) and declines here;
// a constraint parameterized by a SIBLING type parameter is instantiated over the call's own type
// arguments. Any other reach of the type parameter declines — a bare `H` parameter or a result
// mentioning `H` would each observe the substituted interface where Go has the argument's own type.
//
// ⚠ This is the LOCAL test. The sibling-constraint rule — another type parameter's constraint naming
// this one — moved to funcResultProjection below when it was relaxed on 2026-09-20, because whether
// it refuses now depends on whether that sibling ALSO projects, which this function cannot ask
// without recursing. Call funcResultProjection unless you ARE the sibling test.
func (v *Visitor) funcResultProjectionLocal(funIdent *ast.Ident, typeArgs *types.TypeList, k int) (types.Type, types.Type, types.Type, bool) {
	if funIdent == nil || typeArgs == nil {
		return nil, nil, nil, false
	}

	funcObj, ok := v.info.ObjectOf(funIdent).(*types.Func)

	if !ok {
		return nil, nil, nil, false
	}

	sig, ok := funcObj.Type().(*types.Signature)

	if !ok || sig.TypeParams() == nil || k >= sig.TypeParams().Len() || k >= typeArgs.Len() {
		return nil, nil, nil, false
	}

	typeParams := sig.TypeParams()
	typeParam := typeParams.At(k)

	arg := typeArgs.At(k)

	if !funcResultProjectableArg(arg) {
		return nil, nil, nil, false
	}

	constraint := typeParam.Constraint()

	if iface, ok := constraint.Underlying().(*types.Interface); !ok || iface.NumMethods() == 0 || !iface.IsMethodSet() {
		return nil, nil, nil, false
	}

	// The form the GO-level Implements test is asked about: the constraint closed over the call's own
	// TYPE ARGUMENTS. It diverges from `constraint` only when a sibling's projection is substituted
	// below, and the two must not be confused — see the note at the second Instantiate.
	checkConstraint := constraint

	// A parameterized constraint: decline the self-referential one, instantiate the rest over
	// the call's type arguments (a sibling parameter's argument, or a concrete type).
	if constraintNamed, ok := constraint.(*types.Named); ok && constraintNamed.TypeArgs().Len() > 0 {
		args := make([]types.Type, constraintNamed.TypeArgs().Len())
		checkArgs := make([]types.Type, constraintNamed.TypeArgs().Len())

		for j := range args {
			arg := constraintNamed.TypeArgs().At(j)
			rawArg := arg

			if tp, ok := arg.(*types.TypeParam); ok {
				if tp == typeParam {
					return nil, nil, nil, false
				}

				if index := tp.Index(); index < typeParams.Len() && typeParams.At(index) == tp && index < typeArgs.Len() {
					arg = typeArgs.At(index)
					rawArg = typeArgs.At(index)

					// ⚠ If that sibling ALSO projects, close over its PROJECTION rather than its
					// box. Closing over the box is the half-state crypto/mlkem emitted:
					// `decapsulationKey<ж<EncapsulationKey768>>` names the very box the projection
					// exists to avoid, and C# then fails the nominal bound exactly as before.
					if projected, ok := v.siblingProjectedConstraint(funIdent, typeArgs, index); ok {
						arg = projected
					} else if funcResultProjectableArg(arg) {
						// ⚠ THE HALF-STATE, refused: the sibling's argument is the shape a
						// projection would carry (a boxed pointer or an interface) and it did NOT
						// project, so closing over it would name the box inside this constraint —
						// `decapsulationKey<ж<EncapsulationKey768>>`, which fails the nominal bound
						// exactly as the unprojected argument did. A refusal leaves the row failing
						// to compile; a box-closed constraint would compile against the wrong bound.
						// A sibling whose argument is concrete (`keyed[int]`) is not this case and
						// closes over the concrete type as it always has.
						return nil, nil, nil, false
					}
				}
			}

			for m := range typeParams.Len() {
				if typeMentionsTypeParam(arg, typeParams.At(m), map[types.Type]bool{}) {
					return nil, nil, nil, false
				}
			}

			args[j] = arg
			checkArgs[j] = rawArg
		}

		instantiated, err := types.Instantiate(nil, constraintNamed.Origin(), args, false)

		if err != nil {
			return nil, nil, nil, false
		}

		// ⚠ TWO instantiations, and the distinction is load-bearing.
		//
		// `checked` closes over the call's own TYPE ARGUMENTS, which is what Go says the constraint
		// is; `constraint` closes over the sibling's PROJECTION, which is what C# must render.
		// Testing Implements against the PROJECTED form asks the wrong question and refuses a valid
		// row: `*digest` implements `keyedNamed[*digest]` because its method is `encapKey() *digest`,
		// and it does NOT implement `keyedNamed[named]` — no Go type does, since Go has no return
		// covariance. Substituting first and testing second made mlkem's `D` refuse locally, which
		// then made `E` refuse through the sibling rule, which is how the whole pair went dark.
		checked, err := types.Instantiate(nil, constraintNamed.Origin(), checkArgs, false)

		if err != nil {
			return nil, nil, nil, false
		}

		constraint = instantiated
		checkConstraint = checked
	}

	iface, ok := checkConstraint.Underlying().(*types.Interface)

	if !ok || !types.Implements(arg, iface) || interfaceNominallyDerives(arg, checkConstraint) {
		return nil, nil, nil, false
	}

	// The reach: every parameter that mentions the type parameter is exactly `func() H`, at
	// least one does, no result mentions it, and no other type parameter's constraint names it.
	reached := false

	for j := range sig.Params().Len() {
		paramType := sig.Params().At(j).Type()

		if !typeMentionsTypeParam(paramType, typeParam, map[types.Type]bool{}) {
			continue
		}

		if sig.Variadic() && j == sig.Params().Len()-1 {
			return nil, nil, nil, false
		}

		if _, ok := funcResultPositionOf(paramType, typeParam); !ok {
			return nil, nil, nil, false
		}

		reached = true
	}

	if !reached || typeMentionsTypeParam(sig.Results(), typeParam, map[types.Type]bool{}) {
		return nil, nil, nil, false
	}

	return arg, constraint, checkConstraint, true
}

// siblingProjectedConstraint reports the constraint a sibling type parameter PROJECTS to, for use as
// the closed-over form when another constraint names it.
//
// ⚠ BOUNDED TO AN UNPARAMETERIZED SIBLING CONSTRAINT, and the bound is what makes this safe rather
// than merely convenient: funcResultProjectionLocal's own parameterized-constraint block is what
// calls this, so a sibling whose constraint were ALSO parameterized could ask the question back and
// recurse without limit (`A f[B]`, `B g[A]`). A sibling with no type arguments cannot reach that
// block, so the call is one level deep by construction rather than by a visited set.
//
// crypto/mlkem is exactly this shape — `D decapsulationKey[E]` over `E encapsulationKey`, and
// `encapsulationKey` takes no type arguments. A deeper chain keeps the box here, and the outer
// parameter then refuses through the half-state rule in funcResultProjection, which is the safe
// direction: a refusal is a row that still fails to compile, where a wrong closure is a row that
// compiles against the wrong bound.
func (v *Visitor) siblingProjectedConstraint(funIdent *ast.Ident, typeArgs *types.TypeList, index int) (types.Type, bool) {
	funcObj, ok := v.info.ObjectOf(funIdent).(*types.Func)

	if !ok {
		return nil, false
	}

	sig, ok := funcObj.Type().(*types.Signature)

	if !ok || sig.TypeParams() == nil || index >= sig.TypeParams().Len() {
		return nil, false
	}

	if named, ok := sig.TypeParams().At(index).Constraint().(*types.Named); ok && named.TypeArgs().Len() > 0 {
		return nil, false
	}

	_, constraint, _, ok := v.funcResultProjectionLocal(funIdent, typeArgs, index)

	if !ok {
		return nil, false
	}

	return constraint, true
}

// funcResultProjection reports whether type argument `k` projects, applying the SIBLING-CONSTRAINT
// rule on top of the local test above.
//
// The rule used to refuse outright whenever ANOTHER type parameter's constraint MENTIONED this one:
// projecting `E` while `D decapsulationKey[E]` kept its box would leave D's constraint closed over
// the very box the projection exists to avoid. That refusal was right about the HALF-STATE and wrong
// as a verdict — crypto/mlkem is exactly two such parameters and BOTH project, so the coherent state
// was available and simply never reached (CS0311 ×4 at the version tip, measured).
//
// ⚠ Relaxed 2026-09-20: a type parameter MAY project when every constraint that mentions it ALSO
// projects, and that constraint is then closed over the mentioning parameter's PROJECTED form. If any
// mentioning sibling does NOT project, this one still refuses — the half-state is the red, and
// `siblingCall` pins it.
//
// The sibling test calls the LOCAL form, never this one, so the check is one level deep and cannot
// recurse: two parameters that mention each other each ask whether the other projects LOCALLY, and
// neither asks the question back.
func (v *Visitor) funcResultProjectionChecked(funIdent *ast.Ident, typeArgs *types.TypeList, k int) (types.Type, types.Type, types.Type, bool) {
	arg, constraint, checkConstraint, ok := v.funcResultProjectionLocal(funIdent, typeArgs, k)

	if !ok {
		return nil, nil, nil, false
	}

	funcObj, ok := v.info.ObjectOf(funIdent).(*types.Func)

	if !ok {
		return nil, nil, nil, false
	}

	sig, ok := funcObj.Type().(*types.Signature)

	if !ok || sig.TypeParams() == nil || k >= sig.TypeParams().Len() {
		return nil, nil, nil, false
	}

	typeParams := sig.TypeParams()
	typeParam := typeParams.At(k)

	for m := range typeParams.Len() {
		if m == k || !typeMentionsTypeParam(typeParams.At(m).Constraint(), typeParam, map[types.Type]bool{}) {
			continue
		}

		if _, _, _, siblingProjects := v.funcResultProjectionLocal(funIdent, typeArgs, m); !siblingProjects {
			return nil, nil, nil, false
		}
	}

	return arg, constraint, checkConstraint, true
}

// funcResultProjectionArg maps argument `i` of a generic FUNCTION call onto funcResultProjection:
// the parameter must be a `func() H` whose type parameter's position projects.
func (v *Visitor) funcResultProjectionArgChecked(funIdent *ast.Ident, typeArgs *types.TypeList, i int) (types.Type, types.Type, types.Type, bool) {
	typeParams := v.signatureTypeParams(funIdent)

	if typeParams == nil {
		return nil, nil, nil, false
	}

	sig := v.info.ObjectOf(funIdent).(*types.Func).Type().(*types.Signature)

	if i >= sig.Params().Len() || (sig.Variadic() && i == sig.Params().Len()-1) {
		return nil, nil, nil, false
	}

	paramSig, ok := sig.Params().At(i).Type().Underlying().(*types.Signature)

	if !ok {
		return nil, nil, nil, false
	}

	// Which of the call's type parameters this func parameter projects through. The shape check
	// lives in funcResultPositionOf, which accepts the Go constructor idiom `func(args…) (T, error)`
	// and still refuses the two shapes that cannot be wrapped.
	for k := range typeParams.Len() {
		if _, ok := funcResultPositionOf(paramSig, typeParams.At(k)); ok {
			return v.funcResultProjectionChecked(funIdent, typeArgs, k)
		}
	}

	return nil, nil, nil, false
}

// funcResultProjection is funcResultProjectionChecked with the CHECKED constraint dropped — the form
// every caller that only RENDERS or DECIDES takes, and the one the negative controls exercise.
//
// The checked form exists for the single caller that must ask a SATISFACTION question of the
// constraint as GO closed it rather than as C# renders it (convertToProjectedInterfaceType). Keeping
// that caller on its own entry point is what stops the projected form leaking into a types.Implements
// test somewhere else: this seat's first cut asked exactly that question of the projected form and
// refused a valid row, because Go has no return covariance.
func (v *Visitor) funcResultProjection(funIdent *ast.Ident, typeArgs *types.TypeList, k int) (types.Type, types.Type, bool) {
	arg, constraint, _, ok := v.funcResultProjectionChecked(funIdent, typeArgs, k)

	return arg, constraint, ok
}

// funcResultProjectionArg is funcResultProjectionArgChecked with the CHECKED constraint dropped; see
// funcResultProjection for why the two entry points are kept apart.
func (v *Visitor) funcResultProjectionArg(funIdent *ast.Ident, typeArgs *types.TypeList, i int) (types.Type, types.Type, bool) {
	arg, constraint, _, ok := v.funcResultProjectionArgChecked(funIdent, typeArgs, i)

	return arg, constraint, ok
}

// funcResultProjectionResultIndex reports which RESULT of argument `i`'s func parameter carries the
// projected type parameter. The emission needs it to convert that one result and pass the others
// through: `func() H` is index 0, and the Go constructor idiom `func(args…) (H, error)` is also 0
// with an `error` beside it — but the index is READ rather than assumed, because nothing in the
// shape requires the type parameter to come first.
func (v *Visitor) funcResultProjectionResultIndex(funIdent *ast.Ident, i int) (int, bool) {
	typeParams := v.signatureTypeParams(funIdent)

	if typeParams == nil {
		return 0, false
	}

	sig, ok := v.info.ObjectOf(funIdent).(*types.Func).Type().(*types.Signature)

	if !ok || i >= sig.Params().Len() {
		return 0, false
	}

	paramSig, ok := sig.Params().At(i).Type().Underlying().(*types.Signature)

	if !ok {
		return 0, false
	}

	for k := range typeParams.Len() {
		if position, ok := funcResultPositionOf(paramSig, typeParams.At(k)); ok {
			return position, true
		}
	}

	return 0, false
}

// funcResultPositionOf reports the INDEX of the result that IS `tp`, and whether exactly one result
// is. It replaced isFuncResultOf, which required `func() tp` exactly — niladic, one result.
//
// ⚠ Neither of those requirements bears on whether the RESULT projects, and together they refused the
// Go CONSTRUCTOR idiom: crypto/mlkem reaches its type parameter through
// `newEncapsulationKey func([]byte) (E, error)` and was refused on both clauses, so its four call
// sites emitted an explicit type-argument list naming the boxes — CS0311 ×4, measured live at the
// version tip. The parameter list is the caller's business; a trailing `error` is the idiom and
// mentions no type parameter.
//
// The POSITION is returned rather than assumed to be 0, because the emission has to convert that one
// result and pass the others through untouched.
//
// Two shapes are still refused, and both for a reason rather than a definition:
//
//	TWO results ARE tp    `func() (H, H)` — which one the adapter should wrap is not recoverable.
//	a result MENTIONS tp  `func() ([]H, error)` — a slice of the box is not the box, and the
//	                      element projection is a different mechanism with its own wrapping.
func funcResultPositionOf(typ types.Type, tp *types.TypeParam) (int, bool) {
	sig, ok := typ.Underlying().(*types.Signature)

	if !ok || sig.Results().Len() == 0 {
		return 0, false
	}

	position := -1

	for r := range sig.Results().Len() {
		resultType := sig.Results().At(r).Type()

		if result, isParam := types.Unalias(resultType).(*types.TypeParam); isParam && result == tp {
			if position >= 0 {
				return 0, false
			}

			position = r

			continue
		}

		if typeMentionsTypeParam(resultType, tp, map[types.Type]bool{}) {
			return 0, false
		}
	}

	if position < 0 {
		return 0, false
	}

	return position, true
}

// funcResultProjectableArg reports whether a type argument is a kind funcResultProjection can carry:
// a POINTER to a named type (its box needs the pointer adapter, RED 3) or a DECLARED interface with
// methods (a sibling of the constraint needs the interface adapter, RED 4). An anonymous interface
// has no generated adapter class, and an empty one satisfies no method-set constraint.
func funcResultProjectableArg(arg types.Type) bool {
	switch t := types.Unalias(arg).(type) {
	case *types.Pointer:
		_, ok := types.Unalias(t.Elem()).(*types.Named)
		return ok
	case *types.Named:
		iface, ok := t.Underlying().(*types.Interface)
		return ok && iface.NumMethods() > 0
	}

	return false
}

// interfaceNominallyDerives reports whether an INTERFACE type argument already satisfies `constraint`
// nominally in the emitted C#: it IS the constraint, or the constraint sits in its transitive
// embedding closure (Go interface embedding is emitted as C# interface inheritance). A pointer argument
// never derives nominally — its box implements nothing — so it reports false.
func interfaceNominallyDerives(arg, constraint types.Type) bool {
	named, ok := types.Unalias(arg).(*types.Named)

	if !ok {
		return false
	}

	if _, isIface := named.Underlying().(*types.Interface); !isIface {
		return false
	}

	seen := map[types.Type]bool{}
	pending := []types.Type{named}

	for len(pending) > 0 {
		current := pending[len(pending)-1]
		pending = pending[:len(pending)-1]

		if types.Identical(current, constraint) {
			return true
		}

		if seen[current] {
			continue
		}

		seen[current] = true

		if iface, ok := types.Unalias(current).Underlying().(*types.Interface); ok {
			for i := range iface.NumEmbeddeds() {
				if embedded, ok := types.Unalias(iface.EmbeddedType(i)).(*types.Named); ok {
					pending = append(pending, embedded)
				}
			}
		}
	}

	return false
}

// typeMentionsTypeParam reports whether `typ` uses `target` anywhere in its structure. Used to
// decide which FUNC-typed parameters of a constraint-proxy instantiation carry the proxy at a
// delegate boundary (see constraintProxyLambdaParams). The `seen` set terminates the recursion on
// a self-referential named type (`type node[T any] struct { next *node[T] }`).
func typeMentionsTypeParam(typ types.Type, target *types.TypeParam, seen map[types.Type]bool) bool {
	if typ == nil || target == nil {
		return false
	}

	if seen[typ] {
		return false
	}

	seen[typ] = true

	switch t := types.Unalias(typ).(type) {
	case *types.TypeParam:
		return t == target
	case *types.Pointer:
		return typeMentionsTypeParam(t.Elem(), target, seen)
	case *types.Slice:
		return typeMentionsTypeParam(t.Elem(), target, seen)
	case *types.Array:
		return typeMentionsTypeParam(t.Elem(), target, seen)
	case *types.Chan:
		return typeMentionsTypeParam(t.Elem(), target, seen)
	case *types.Map:
		return typeMentionsTypeParam(t.Key(), target, seen) || typeMentionsTypeParam(t.Elem(), target, seen)
	case *types.Tuple:
		for i := range t.Len() {
			if typeMentionsTypeParam(t.At(i).Type(), target, seen) {
				return true
			}
		}
	case *types.Signature:
		return typeMentionsTypeParam(t.Params(), target, seen) || typeMentionsTypeParam(t.Results(), target, seen)
	case *types.Named:
		if args := t.TypeArgs(); args != nil {
			for i := range args.Len() {
				if typeMentionsTypeParam(args.At(i), target, seen) {
					return true
				}
			}
		}
	}

	return false
}

// constraintProxyLambdaParams reports the lambda parameter list to re-wrap argument `i` of a
// generic FUNCTION call with, when that parameter is FUNC-typed and its signature mentions a
// PROXIED type parameter. C# renders such a delegate over the proxy (`Func<P224PointжnistPoint>`
// for Go's `newPoint func() P`), and a method-group conversion cannot apply the user-defined
// ж↔proxy conversion the proxy return needs — CS0407 on
// `testEquivalents(t, nistec.NewP224Point, …)`. The lambda re-wrap moves the conversion into the
// body, where it is an ordinary implicit conversion. Same remedy convCompositeLit already applies
// to a proxied struct's FUNC field initializer.
func (v *Visitor) constraintProxyLambdaParams(funIdent *ast.Ident, typeArgs *types.TypeList, i int) (string, bool) {
	typeParams := v.signatureTypeParams(funIdent)

	if typeParams == nil || typeArgs == nil {
		return "", false
	}

	funcObj, ok := v.info.ObjectOf(funIdent).(*types.Func)

	if !ok {
		return "", false
	}

	sig, ok := funcObj.Type().(*types.Signature)

	if !ok || sig.Params() == nil || i >= sig.Params().Len() {
		return "", false
	}

	paramSig, ok := sig.Params().At(i).Type().Underlying().(*types.Signature)

	if !ok {
		return "", false
	}

	proxied := false

	for p := 0; p < typeParams.Len() && p < typeArgs.Len(); p++ {
		if _, ok := v.constraintProxyFor(typeParams.At(p), typeArgs.At(p)); !ok {
			continue
		}

		if typeMentionsTypeParam(paramSig, typeParams.At(p), map[types.Type]bool{}) {
			proxied = true
			break
		}
	}

	if !proxied {
		return "", false
	}

	params := make([]string, paramSig.Params().Len())

	for p := range params {
		params[p] = fmt.Sprintf("%sp%d", ShadowVarMarker, p)
	}

	return strings.Join(params, ", "), true
}

// constraintProxyLitParamTypes reports, for a FUNC-LITERAL argument at position `i` of a generic
// FUNCTION call, the constraint-proxy C# type each of the literal's own parameters must be DECLARED
// as — keyed by parameter index, empty when none.
//
// The sibling constraintProxyLambdaParams above handles a METHOD-GROUP argument at such a position
// by re-wrapping it as a parameter-inferred lambda. A func LITERAL cannot take that remedy: it
// already IS the lambda and it renders its own parameter list from the Go signature, so at
// `T = ImplжConstrained` a `func(t T, mode int)` parameter emits `(ж<Impl> t, nint mode) => …`
// against a delegate that requires `Action<ImplжConstrained, nint>`. C# applies no user-defined
// conversion at a parameter DECLARATION — that is the whole of CS1678 + CS1661, one pair per call
// site, and 48 of net/http's 81 body diagnostics: its `run[T TBRun[T]](t T, f func(t T, mode
// testMode), opts ...any)` is called this way throughout the suite.
//
// The remedy is the same one the method-group case uses and the one convCompositeLit applies to a
// proxied struct's func field — MOVE THE CONVERSION to a position where C# performs it. Here the
// parameter is DECLARED at the proxy type under a synthesized name and the literal's body opens with
// `ж<Impl> t = Δp0;`, an ordinary implicit conversion through the operator the proxy already
// declares. Everything after that line is unchanged: the body still sees the Go name at its natural
// type, so no member access, capture, or nested literal inside it renders differently. Declaring the
// parameter at the proxy and letting the body use it directly would NOT work — the proxy's
// forwarders are explicit interface implementations, reachable through a type parameter's constraint
// but not by member lookup on the concrete proxy type.
//
// Restricted to a parameter whose type IS the proxied type parameter exactly. A type that merely
// MENTIONS it (`[]T`, `map[K]T`, `func(T)`) is deliberately excluded: `slice<ImplжConstrained>` and
// `slice<ж<Impl>>` are distinct generic instantiations with no conversion between them, so there is
// no single assignment that could stand in the prologue, and a wrong guess would trade a clear
// CS1678 for a wrong one. Such a shape does not occur in the corpus or in net/http; when one appears
// it needs its own decision, not an extension of this one.
func (v *Visitor) constraintProxyLitParamTypes(funIdent *ast.Ident, typeArgs *types.TypeList, i int) map[int]string {
	typeParams := v.signatureTypeParams(funIdent)

	if typeParams == nil || typeArgs == nil {
		return nil
	}

	funcObj, ok := v.info.ObjectOf(funIdent).(*types.Func)

	if !ok {
		return nil
	}

	sig, ok := funcObj.Type().(*types.Signature)

	if !ok || sig.Params() == nil || i >= sig.Params().Len() {
		return nil
	}

	paramSig, ok := sig.Params().At(i).Type().Underlying().(*types.Signature)

	if !ok || paramSig.Params() == nil {
		return nil
	}

	proxyOf := map[*types.TypeParam]string{}

	for p := 0; p < typeParams.Len() && p < typeArgs.Len(); p++ {
		if proxyName, ok := v.constraintProxyFor(typeParams.At(p), typeArgs.At(p)); ok {
			proxyOf[typeParams.At(p)] = proxyName
		}
	}

	if len(proxyOf) == 0 {
		return nil
	}

	proxied := map[int]string{}

	for p := range paramSig.Params().Len() {
		if typeParam, ok := types.Unalias(paramSig.Params().At(p).Type()).(*types.TypeParam); ok {
			if proxyName, ok := proxyOf[typeParam]; ok {
				proxied[p] = proxyName
			}
		}
	}

	if len(proxied) == 0 {
		return nil
	}

	return proxied
}

// callNeedsConstraintProxy reports whether any type argument of a generic FUNCTION instantiation
// resolves to a self-referential constraint proxy. Such a call must render its type arguments
// EXPLICITLY even where Go inferred them from an ordinary value argument: C# would otherwise infer
// the box `ж<P224Point>` from the argument's own type and reject it against `where P : nistPoint<P>`
// (CS0311) — `benchmarkScalarMult(b, nistec.NewP224Point().SetGenerator(), 28)`.
func (v *Visitor) callNeedsConstraintProxy(funIdent *ast.Ident, typeArgs *types.TypeList) bool {
	if typeArgs == nil {
		return false
	}

	for i := 0; i < typeArgs.Len(); i++ {
		if _, ok := v.constraintProxySigArg(funIdent, typeArgs, i); ok {
			return true
		}
	}

	return false
}

// isPredeclaredComparable reports whether t is Go's built-in `comparable` — the universe-scope
// pseudo-interface, identified by having no package rather than by spelling alone, so a package's own
// `type comparable interface{…}` is never mistaken for it.
func isPredeclaredComparable(t types.Type) bool {
	named, ok := types.Unalias(t).(*types.Named)

	if !ok {
		return false
	}

	obj := named.Obj()

	return obj != nil && obj.Pkg() == nil && obj.Name() == "comparable"
}

// isMethodSetWithPointerNamedUnion reports whether iface is a method set PLUS embedded unions whose
// every term is a pointer to a NAMED type, with no tilde and nothing else embedded — Go 1.24's
// crypto/internal/fips140 `Point[P]` constraints (ecdh, ecdsa), which open with
// `*nistec.P224Point | *nistec.P256Point | *nistec.P384Point | *nistec.P521Point` beside the method
// list crypto/elliptic's `nistPoint[T]` carries alone.
//
// The union restricts nothing a C# type can observe: visitInterfaceType emits it as a comment line
// and the partial interface as a pure method set, so a proxy implementing that interface is exactly
// the nistPoint proxy. IsMethodSet() answering false over the union was refusing on a Go-side property
// the emission had already dropped (RED 8, C2 sizing 0d6cd77a2, COORD 7bc9d58d4 candidate (a)).
//
// Deliberately narrow: an approximate term (`~*T`), a non-pointer term, a pointer to an unnamed type,
// or any embedded interface keeps the method-set refusal.
func isMethodSetWithPointerNamedUnion(iface *types.Interface) bool {
	if iface == nil || iface.NumMethods() == 0 || iface.NumEmbeddeds() == 0 {
		return false
	}

	for i := range iface.NumEmbeddeds() {
		union, ok := iface.EmbeddedType(i).(*types.Union)

		if !ok {
			return false
		}

		for j := range union.Len() {
			term := union.Term(j)

			if term.Tilde() {
				return false
			}

			ptr, ok := types.Unalias(term.Type()).(*types.Pointer)

			if !ok {
				return false
			}

			if _, ok := types.Unalias(ptr.Elem()).(*types.Named); !ok {
				return false
			}
		}
	}

	return true
}

// isMethodSetBeyondComparable reports whether iface is a pure METHOD SET once an embedded
// `comparable` is discounted.
//
// Go's built-in `comparable` admits every ==-able type and no C# constraint can express that set, so
// the bare-constraint arm above emits nothing for it beyond `new()` — golib's `comparable<T>` CRTP is
// implemented by NOTHING. An interface that EMBEDS it inherits the same fact and must be treated the
// same way, but it was not: `type netipTypeCmp interface { comparable; netipType }` (net/netip's
// fuzz_test.go) made `IsMethodSet()` answer false, so the constraint took the generic CRTP form
// `where P : netipTypeCmp<P>` while visitInterfaceType had emitted the interface arity-0 — CS0308,
// the non-generic type cannot be used with type arguments. The two sides must agree, and the
// method-set side is the one that is expressible.
//
// Every OTHER embedded type still has to be a method set: an interface mixing `comparable` with a
// real type-term union (`comparable; ~int | ~string`) restricts its type set in a way the arity-0
// form does not describe, and keeps the existing generic treatment.
func isMethodSetBeyondComparable(iface *types.Interface) bool {
	if iface == nil {
		return false
	}

	if iface.IsMethodSet() {
		return true
	}

	sawComparable := false

	for i := range iface.NumEmbeddeds() {
		embedded := iface.EmbeddedType(i)

		if isPredeclaredComparable(embedded) {
			sawComparable = true
			continue
		}

		embeddedIface, ok := embedded.Underlying().(*types.Interface)

		if !ok || !isMethodSetBeyondComparable(embeddedIface) {
			return false
		}
	}

	return sawComparable
}
