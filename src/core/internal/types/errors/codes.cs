// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.types;

partial class errors_package {

partial struct Code /*num:nint*/;

//go:generate go run golang.org/x/tools/cmd/stringer@latest -type Code codes.go
// This file defines the error codes that can be produced during type-checking.
// Collectively, these codes provide an identifier that may be used to
// implement special handling for certain types of errors.
//
// Error code values should not be changed: add new codes at the end.
//
// Error codes should be fine-grained enough that the exact nature of the error
// can be easily determined, but coarse enough that they are not an
// implementation detail of the type checking algorithm. As a rule-of-thumb,
// errors should be considered equivalent if there is a theoretical refactoring
// of the type checker in which they are emitted in exactly one place. For
// example, the type checker emits different error messages for "too many
// arguments" and "too few arguments", but one can imagine an alternative type
// checker where this check instead just emits a single "wrong number of
// arguments", so these errors should have the same code.
//
// Error code names should be as brief as possible while retaining accuracy and
// distinctiveness. In most cases names should start with an adjective
// describing the nature of the error (e.g. "invalid", "unused", "misplaced"),
// and end with a noun identifying the relevant language object. For example,
// "_DuplicateDecl" or "_InvalidSliceExpr". For brevity, naming follows the
// convention that "bad" implies a problem with syntax, and "invalid" implies a
// problem with types.

// InvalidSyntaxTree occurs if an invalid syntax tree is provided
// to the type checker. It should never happen.
public static Code InvalidSyntaxTree => -1;

// The zero Code value indicates an unset (invalid) error code.
internal static Code _ᴛ2ʗ => /* iota */ 0;

// Test is reserved for errors that only apply while in self-test mode.
public static Code Test => 1;

// BlankPkgName occurs when a package name is the blank identifier "_".
//
// Per the spec:
//  "The PackageName must not be the blank identifier."
//
// Example:
//  package _
public static Code BlankPkgName => 2;

// MismatchedPkgName occurs when a file's package name doesn't match the
// package name already established by other files.
public static Code MismatchedPkgName => 3;

// InvalidPkgUse occurs when a package identifier is used outside of a
// selector expression.
//
// Example:
//  import "fmt"
//
//  var _ = fmt
public static Code InvalidPkgUse => 4;

// BadImportPath occurs when an import path is not valid.
public static Code BadImportPath => 5;

// BrokenImport occurs when importing a package fails.
//
// Example:
//  import "amissingpackage"
public static Code BrokenImport => 6;

// ImportCRenamed occurs when the special import "C" is renamed. "C" is a
// pseudo-package, and must not be renamed.
//
// Example:
//  import _ "C"
public static Code ImportCRenamed => 7;

// UnusedImport occurs when an import is unused.
//
// Example:
//  import "fmt"
//
//  func main() {}
public static Code UnusedImport => 8;

// InvalidInitCycle occurs when an invalid cycle is detected within the
// initialization graph.
//
// Example:
//  var x int = f()
//
//  func f() int { return x }
public static Code InvalidInitCycle => 9;

// DuplicateDecl occurs when an identifier is declared multiple times.
//
// Example:
//  var x = 1
//  var x = 2
public static Code DuplicateDecl => 10;

// InvalidDeclCycle occurs when a declaration cycle is not valid.
//
// Example:
//  type S struct {
//  	S
//  }
//
// Example:
//  import "unsafe"
//
//  type T [unsafe.Sizeof(T{})]int
public static Code InvalidDeclCycle => 11;

// TODO(markfreeman): Retire InvalidTypeCycle, as it's never emitted.

// InvalidTypeCycle occurs when a cycle in type definitions results in a
// type that is not well-defined.
public static Code InvalidTypeCycle => 12;

// InvalidConstInit occurs when a const declaration has a non-constant
// initializer.
//
// Example:
//  var x int
//  const _ = x
public static Code InvalidConstInit => 13;

// InvalidConstVal occurs when a const value cannot be converted to its
// target type.
//
// TODO(findleyr): this error code and example are not very clear. Consider
// removing it.
//
// Example:
//  const _ = 1 << "hello"
public static Code InvalidConstVal => 14;

// InvalidConstType occurs when the underlying type in a const declaration
// is not a valid constant type.
//
// Example:
//  const c *int = 4
public static Code InvalidConstType => 15;

// UntypedNilUse occurs when the predeclared (untyped) value nil is used to
// initialize a variable declared without an explicit type.
//
// Example:
//  var x = nil
public static Code UntypedNilUse => 16;

// WrongAssignCount occurs when the number of values on the right-hand side
// of an assignment or initialization expression does not match the number
// of variables on the left-hand side.
//
// Example:
//  var x = 1, 2
public static Code WrongAssignCount => 17;

// UnassignableOperand occurs when the left-hand side of an assignment is
// not assignable.
//
// Example:
//  func f() {
//  	const c = 1
//  	c = 2
//  }
public static Code UnassignableOperand => 18;

// NoNewVar occurs when a short variable declaration (':=') does not declare
// new variables.
//
// Example:
//  func f() {
//  	x := 1
//  	x := 2
//  }
public static Code NoNewVar => 19;

// MultiValAssignOp occurs when an assignment operation (+=, *=, etc) does
// not have single-valued left-hand or right-hand side.
//
// Per the spec:
//  "In assignment operations, both the left- and right-hand expression lists
//  must contain exactly one single-valued expression"
//
// Example:
//  func f() int {
//  	x, y := 1, 2
//  	x, y += 1
//  	return x + y
//  }
public static Code MultiValAssignOp => 20;

// InvalidIfaceAssign occurs when a value of type T is used as an
// interface, but T does not implement a method of the expected interface.
//
// Example:
//  type I interface {
//  	f()
//  }
//
//  type T int
//
//  var x I = T(1)
public static Code InvalidIfaceAssign => 21;

// InvalidChanAssign occurs when a chan assignment is invalid.
//
// Per the spec, a value x is assignable to a channel type T if:
//  "x is a bidirectional channel value, T is a channel type, x's type V and
//  T have identical element types, and at least one of V or T is not a
//  defined type."
//
// Example:
//  type T1 chan int
//  type T2 chan int
//
//  var x T1
//  // Invalid assignment because both types are named
//  var _ T2 = x
public static Code InvalidChanAssign => 22;

// IncompatibleAssign occurs when the type of the right-hand side expression
// in an assignment cannot be assigned to the type of the variable being
// assigned.
//
// Example:
//  var x []int
//  var _ int = x
public static Code IncompatibleAssign => 23;

// UnaddressableFieldAssign occurs when trying to assign to a struct field
// in a map value.
//
// Example:
//  func f() {
//  	m := make(map[string]struct{i int})
//  	m["foo"].i = 42
//  }
public static Code UnaddressableFieldAssign => 24;

// NotAType occurs when the identifier used as the underlying type in a type
// declaration or the right-hand side of a type alias does not denote a type.
//
// Example:
//  var S = 2
//
//  type T S
public static Code NotAType => 25;

// InvalidArrayLen occurs when an array length is not a constant value.
//
// Example:
//  var n = 3
//  var _ = [n]int{}
public static Code InvalidArrayLen => 26;

// BlankIfaceMethod occurs when a method name is '_'.
//
// Per the spec:
//  "The name of each explicitly specified method must be unique and not
//  blank."
//
// Example:
//  type T interface {
//  	_(int)
//  }
public static Code BlankIfaceMethod => 27;

// IncomparableMapKey occurs when a map key type does not support the == and
// != operators.
//
// Per the spec:
//  "The comparison operators == and != must be fully defined for operands of
//  the key type; thus the key type must not be a function, map, or slice."
//
// Example:
//  var x map[T]int
//
//  type T []int
public static Code IncomparableMapKey => 28;

// InvalidIfaceEmbed occurs when a non-interface type is embedded in an
// interface (for go 1.17 or earlier).
internal static Code _ᴛ3ʗ => 29; // not used anymore

// InvalidPtrEmbed occurs when an embedded field is of the pointer form *T,
// and T itself is itself a pointer, an unsafe.Pointer, or an interface.
//
// Per the spec:
//  "An embedded field must be specified as a type name T or as a pointer to
//  a non-interface type name *T, and T itself may not be a pointer type."
//
// Example:
//  type T *int
//
//  type S struct {
//  	*T
//  }
public static Code InvalidPtrEmbed => 30;

// BadRecv occurs when a method declaration does not have exactly one
// receiver parameter.
//
// Example:
//  func () _() {}
public static Code BadRecv => 31;

// InvalidRecv occurs when a receiver type expression is not of the form T
// or *T, or T is a pointer type.
//
// Example:
//  type T struct {}
//
//  func (**T) m() {}
public static Code InvalidRecv => 32;

// DuplicateFieldAndMethod occurs when an identifier appears as both a field
// and method name.
//
// Example:
//  type T struct {
//  	m int
//  }
//
//  func (T) m() {}
public static Code DuplicateFieldAndMethod => 33;

// DuplicateMethod occurs when two methods on the same receiver type have
// the same name.
//
// Example:
//  type T struct {}
//  func (T) m() {}
//  func (T) m(i int) int { return i }
public static Code DuplicateMethod => 34;

// InvalidBlank occurs when a blank identifier is used as a value or type.
//
// Per the spec:
//  "The blank identifier may appear as an operand only on the left-hand side
//  of an assignment."
//
// Example:
//  var x = _
public static Code InvalidBlank => 35;

// InvalidIota occurs when the predeclared identifier iota is used outside
// of a constant declaration.
//
// Example:
//  var x = iota
public static Code InvalidIota => 36;

// MissingInitBody occurs when an init function is missing its body.
//
// Example:
//  func init()
public static Code MissingInitBody => 37;

// InvalidInitSig occurs when an init function declares parameters or
// results.
//
// Deprecated: no longer emitted by the type checker. _InvalidInitDecl is
// used instead.
public static Code InvalidInitSig => 38;

// InvalidInitDecl occurs when init is declared as anything other than a
// function.
//
// Example:
//  var init = 1
//
// Example:
//  func init() int { return 1 }
public static Code InvalidInitDecl => 39;

// InvalidMainDecl occurs when main is declared as anything other than a
// function, in a main package.
public static Code InvalidMainDecl => 40;

// TooManyValues occurs when a function returns too many values for the
// expression context in which it is used.
//
// Example:
//  func ReturnTwo() (int, int) {
//  	return 1, 2
//  }
//
//  var x = ReturnTwo()
public static Code TooManyValues => 41;

// NotAnExpr occurs when a type expression is used where a value expression
// is expected.
//
// Example:
//  type T struct {}
//
//  func f() {
//  	T
//  }
public static Code NotAnExpr => 42;

// TruncatedFloat occurs when a float constant is truncated to an integer
// value.
//
// Example:
//  var _ int = 98.6
public static Code TruncatedFloat => 43;

// NumericOverflow occurs when a numeric constant overflows its target type.
//
// Example:
//  var x int8 = 1000
public static Code NumericOverflow => 44;

// UndefinedOp occurs when an operator is not defined for the type(s) used
// in an operation.
//
// Example:
//  var c = "a" - "b"
public static Code UndefinedOp => 45;

// MismatchedTypes occurs when operand types are incompatible in a binary
// operation.
//
// Example:
//  var a = "hello"
//  var b = 1
//  var c = a - b
public static Code MismatchedTypes => 46;

// DivByZero occurs when a division operation is provable at compile
// time to be a division by zero.
//
// Example:
//  const divisor = 0
//  var x int = 1/divisor
public static Code DivByZero => 47;

// NonNumericIncDec occurs when an increment or decrement operator is
// applied to a non-numeric value.
//
// Example:
//  func f() {
//  	var c = "c"
//  	c++
//  }
public static Code NonNumericIncDec => 48;

// UnaddressableOperand occurs when the & operator is applied to an
// unaddressable expression.
//
// Example:
//  var x = &1
public static Code UnaddressableOperand => 49;

// InvalidIndirection occurs when a non-pointer value is indirected via the
// '*' operator.
//
// Example:
//  var x int
//  var y = *x
public static Code InvalidIndirection => 50;

// NonIndexableOperand occurs when an index operation is applied to a value
// that cannot be indexed.
//
// Example:
//  var x = 1
//  var y = x[1]
public static Code NonIndexableOperand => 51;

// InvalidIndex occurs when an index argument is not of integer type,
// negative, or out-of-bounds.
//
// Example:
//  var s = [...]int{1,2,3}
//  var x = s[5]
//
// Example:
//  var s = []int{1,2,3}
//  var _ = s[-1]
//
// Example:
//  var s = []int{1,2,3}
//  var i string
//  var _ = s[i]
public static Code InvalidIndex => 52;

// SwappedSliceIndices occurs when constant indices in a slice expression
// are decreasing in value.
//
// Example:
//  var _ = []int{1,2,3}[2:1]
public static Code SwappedSliceIndices => 53;

// NonSliceableOperand occurs when a slice operation is applied to a value
// whose type is not sliceable, or is unaddressable.
//
// Example:
//  var x = [...]int{1, 2, 3}[:1]
//
// Example:
//  var x = 1
//  var y = 1[:1]
public static Code NonSliceableOperand => 54;

// InvalidSliceExpr occurs when a three-index slice expression (a[x:y:z]) is
// applied to a string.
//
// Example:
//  var s = "hello"
//  var x = s[1:2:3]
public static Code InvalidSliceExpr => 55;

// InvalidShiftCount occurs when the right-hand side of a shift operation is
// either non-integer, negative, or too large.
//
// Example:
//  var (
//  	x string
//  	y int = 1 << x
//  )
public static Code InvalidShiftCount => 56;

// InvalidShiftOperand occurs when the shifted operand is not an integer.
//
// Example:
//  var s = "hello"
//  var x = s << 2
public static Code InvalidShiftOperand => 57;

// InvalidReceive occurs when there is a channel receive from a value that
// is either not a channel, or is a send-only channel.
//
// Example:
//  func f() {
//  	var x = 1
//  	<-x
//  }
public static Code InvalidReceive => 58;

// InvalidSend occurs when there is a channel send to a value that is not a
// channel, or is a receive-only channel.
//
// Example:
//  func f() {
//  	var x = 1
//  	x <- "hello!"
//  }
public static Code InvalidSend => 59;

// DuplicateLitKey occurs when an index is duplicated in a slice, array, or
// map literal.
//
// Example:
//  var _ = []int{0:1, 0:2}
//
// Example:
//  var _ = map[string]int{"a": 1, "a": 2}
public static Code DuplicateLitKey => 60;

// MissingLitKey occurs when a map literal is missing a key expression.
//
// Example:
//  var _ = map[string]int{1}
public static Code MissingLitKey => 61;

// InvalidLitIndex occurs when the key in a key-value element of a slice or
// array literal is not an integer constant.
//
// Example:
//  var i = 0
//  var x = []string{i: "world"}
public static Code InvalidLitIndex => 62;

// OversizeArrayLit occurs when an array literal exceeds its length.
//
// Example:
//  var _ = [2]int{1,2,3}
public static Code OversizeArrayLit => 63;

// MixedStructLit occurs when a struct literal contains a mix of positional
// and named elements.
//
// Example:
//  var _ = struct{i, j int}{i: 1, 2}
public static Code MixedStructLit => 64;

// InvalidStructLit occurs when a positional struct literal has an incorrect
// number of values.
//
// Example:
//  var _ = struct{i, j int}{1,2,3}
public static Code InvalidStructLit => 65;

// MissingLitField occurs when a struct literal refers to a field that does
// not exist on the struct type.
//
// Example:
//  var _ = struct{i int}{j: 2}
public static Code MissingLitField => 66;

// DuplicateLitField occurs when a struct literal contains duplicated
// fields.
//
// Example:
//  var _ = struct{i int}{i: 1, i: 2}
public static Code DuplicateLitField => 67;

// UnexportedLitField occurs when a positional struct literal implicitly
// assigns an unexported field of an imported type.
public static Code UnexportedLitField => 68;

// InvalidLitField occurs when a field name is not a valid identifier.
//
// Example:
//  var _ = struct{i int}{1: 1}
public static Code InvalidLitField => 69;

// UntypedLit occurs when a composite literal omits a required type
// identifier.
//
// Example:
//  type outer struct{
//  	inner struct { i int }
//  }
//
//  var _ = outer{inner: {1}}
public static Code UntypedLit => 70;

// InvalidLit occurs when a composite literal expression does not match its
// type.
//
// Example:
//  type P *struct{
//  	x int
//  }
//  var _ = P {}
public static Code InvalidLit => 71;

// AmbiguousSelector occurs when a selector is ambiguous.
//
// Example:
//  type E1 struct { i int }
//  type E2 struct { i int }
//  type T struct { E1; E2 }
//
//  var x T
//  var _ = x.i
public static Code AmbiguousSelector => 72;

// UndeclaredImportedName occurs when a package-qualified identifier is
// undeclared by the imported package.
//
// Example:
//  import "go/types"
//
//  var _ = types.NotAnActualIdentifier
public static Code UndeclaredImportedName => 73;

// UnexportedName occurs when a selector refers to an unexported identifier
// of an imported package.
//
// Example:
//  import "reflect"
//
//  type _ reflect.flag
public static Code UnexportedName => 74;

// UndeclaredName occurs when an identifier is not declared in the current
// scope.
//
// Example:
//  var x T
public static Code UndeclaredName => 75;

// MissingFieldOrMethod occurs when a selector references a field or method
// that does not exist.
//
// Example:
//  type T struct {}
//
//  var x = T{}.f
public static Code MissingFieldOrMethod => 76;

// BadDotDotDotSyntax occurs when a "..." occurs in a context where it is
// not valid.
//
// Example:
//  var _ = map[int][...]int{0: {}}
public static Code BadDotDotDotSyntax => 77;

// NonVariadicDotDotDot occurs when a "..." is used on the final argument to
// a non-variadic function.
//
// Example:
//  func printArgs(s []string) {
//  	for _, a := range s {
//  		println(a)
//  	}
//  }
//
//  func f() {
//  	s := []string{"a", "b", "c"}
//  	printArgs(s...)
//  }
public static Code NonVariadicDotDotDot => 78;

// MisplacedDotDotDot occurs when a "..." is used somewhere other than the
// final argument in a function declaration.
internal static Code _ᴛ4ʗ => 79; // not used anymore (error reported by parser)

internal static Code _ᴛ5ʗ => 80; // InvalidDotDotDotOperand was removed.

// InvalidDotDotDot occurs when a "..." is used in a non-variadic built-in
// function.
//
// Example:
//  var s = []int{1, 2, 3}
//  var l = len(s...)
public static Code InvalidDotDotDot => 81;

// UncalledBuiltin occurs when a built-in function is used as a
// function-valued expression, instead of being called.
//
// Per the spec:
//  "The built-in functions do not have standard Go types, so they can only
//  appear in call expressions; they cannot be used as function values."
//
// Example:
//  var _ = copy
public static Code UncalledBuiltin => 82;

// InvalidAppend occurs when append is called with a first argument that is
// not a slice.
//
// Example:
//  var _ = append(1, 2)
public static Code InvalidAppend => 83;

// InvalidCap occurs when an argument to the cap built-in function is not of
// supported type.
//
// See https://golang.org/ref/spec#Length_and_capacity for information on
// which underlying types are supported as arguments to cap and len.
//
// Example:
//  var s = 2
//  var x = cap(s)
public static Code InvalidCap => 84;

// InvalidClose occurs when close(...) is called with an argument that is
// not of channel type, or that is a receive-only channel.
//
// Example:
//  func f() {
//  	var x int
//  	close(x)
//  }
public static Code InvalidClose => 85;

// InvalidCopy occurs when the arguments are not of slice type or do not
// have compatible type.
//
// See https://golang.org/ref/spec#Appending_and_copying_slices for more
// information on the type requirements for the copy built-in.
//
// Example:
//  func f() {
//  	var x []int
//  	y := []int64{1,2,3}
//  	copy(x, y)
//  }
public static Code InvalidCopy => 86;

// InvalidComplex occurs when the complex built-in function is called with
// arguments with incompatible types.
//
// Example:
//  var _ = complex(float32(1), float64(2))
public static Code InvalidComplex => 87;

// InvalidDelete occurs when the delete built-in function is called with a
// first argument that is not a map.
//
// Example:
//  func f() {
//  	m := "hello"
//  	delete(m, "e")
//  }
public static Code InvalidDelete => 88;

// InvalidImag occurs when the imag built-in function is called with an
// argument that does not have complex type.
//
// Example:
//  var _ = imag(int(1))
public static Code InvalidImag => 89;

// InvalidLen occurs when an argument to the len built-in function is not of
// supported type.
//
// See https://golang.org/ref/spec#Length_and_capacity for information on
// which underlying types are supported as arguments to cap and len.
//
// Example:
//  var s = 2
//  var x = len(s)
public static Code InvalidLen => 90;

// SwappedMakeArgs occurs when make is called with three arguments, and its
// length argument is larger than its capacity argument.
//
// Example:
//  var x = make([]int, 3, 2)
public static Code SwappedMakeArgs => 91;

// InvalidMake occurs when make is called with an unsupported type argument.
//
// See https://golang.org/ref/spec#Making_slices_maps_and_channels for
// information on the types that may be created using make.
//
// Example:
//  var x = make(int)
public static Code InvalidMake => 92;

// InvalidReal occurs when the real built-in function is called with an
// argument that does not have complex type.
//
// Example:
//  var _ = real(int(1))
public static Code InvalidReal => 93;

// InvalidAssert occurs when a type assertion is applied to a
// value that is not of interface type.
//
// Example:
//  var x = 1
//  var _ = x.(float64)
public static Code InvalidAssert => 94;

// ImpossibleAssert occurs for a type assertion x.(T) when the value x of
// interface cannot have dynamic type T, due to a missing or mismatching
// method on T.
//
// Example:
//  type T int
//
//  func (t *T) m() int { return int(*t) }
//
//  type I interface { m() int }
//
//  var x I
//  var _ = x.(T)
public static Code ImpossibleAssert => 95;

// InvalidConversion occurs when the argument type cannot be converted to the
// target.
//
// See https://golang.org/ref/spec#Conversions for the rules of
// convertibility.
//
// Example:
//  var x float64
//  var _ = string(x)
public static Code InvalidConversion => 96;

// InvalidUntypedConversion occurs when there is no valid implicit
// conversion from an untyped value satisfying the type constraints of the
// context in which it is used.
//
// Example:
//  func f[T ~int8 | ~int16 | ~int32 | ~int64](x T) T {
//  	return x + 1024
//  }
public static Code InvalidUntypedConversion => 97;

// BadOffsetofSyntax occurs when unsafe.Offsetof is called with an argument
// that is not a selector expression.
//
// Example:
//  import "unsafe"
//
//  var x int
//  var _ = unsafe.Offsetof(x)
public static Code BadOffsetofSyntax => 98;

// InvalidOffsetof occurs when unsafe.Offsetof is called with a method
// selector, rather than a field selector, or when the field is embedded via
// a pointer.
//
// Per the spec:
//
//  "If f is an embedded field, it must be reachable without pointer
//  indirections through fields of the struct. "
//
// Example:
//  import "unsafe"
//
//  type T struct { f int }
//  type S struct { *T }
//  var s S
//  var _ = unsafe.Offsetof(s.f)
//
// Example:
//  import "unsafe"
//
//  type S struct{}
//
//  func (S) m() {}
//
//  var s S
//  var _ = unsafe.Offsetof(s.m)
public static Code InvalidOffsetof => 99;

// UnusedExpr occurs when a side-effect free expression is used as a
// statement. Such a statement has no effect.
//
// Example:
//  func f(i int) {
//  	i*i
//  }
public static Code UnusedExpr => 100;

// UnusedVar occurs when a variable is declared but unused.
//
// Example:
//  func f() {
//  	x := 1
//  }
public static Code UnusedVar => 101;

// MissingReturn occurs when a function with results is missing a return
// statement.
//
// Example:
//  func f() int {}
public static Code MissingReturn => 102;

// WrongResultCount occurs when a return statement returns an incorrect
// number of values.
//
// Example:
//  func ReturnOne() int {
//  	return 1, 2
//  }
public static Code WrongResultCount => 103;

// OutOfScopeResult occurs when the name of a value implicitly returned by
// an empty return statement is shadowed in a nested scope.
//
// Example:
//  func factor(n int) (i int) {
//  	for i := 2; i < n; i++ {
//  		if n%i == 0 {
//  			return
//  		}
//  	}
//  	return 0
//  }
public static Code OutOfScopeResult => 104;

// InvalidCond occurs when an if condition is not a boolean expression.
//
// Example:
//  func checkReturn(i int) {
//  	if i {
//  		panic("non-zero return")
//  	}
//  }
public static Code InvalidCond => 105;

// InvalidPostDecl occurs when there is a declaration in a for-loop post
// statement.
//
// Example:
//  func f() {
//  	for i := 0; i < 10; j := 0 {}
//  }
public static Code InvalidPostDecl => 106;

internal static Code _ᴛ6ʗ => 107; // InvalidChanRange was removed.

// InvalidIterVar occurs when two iteration variables are used while ranging
// over a channel.
//
// Example:
//  func f(c chan int) {
//  	for k, v := range c {
//  		println(k, v)
//  	}
//  }
public static Code InvalidIterVar => 108;

// InvalidRangeExpr occurs when the type of a range expression is not
// a valid type for use with a range loop.
//
// Example:
//  func f(f float64) {
//  	for j := range f {
//  		println(j)
//  	}
//  }
public static Code InvalidRangeExpr => 109;

// MisplacedBreak occurs when a break statement is not within a for, switch,
// or select statement of the innermost function definition.
//
// Example:
//  func f() {
//  	break
//  }
public static Code MisplacedBreak => 110;

// MisplacedContinue occurs when a continue statement is not within a for
// loop of the innermost function definition.
//
// Example:
//  func sumeven(n int) int {
//  	proceed := func() {
//  		continue
//  	}
//  	sum := 0
//  	for i := 1; i <= n; i++ {
//  		if i % 2 != 0 {
//  			proceed()
//  		}
//  		sum += i
//  	}
//  	return sum
//  }
public static Code MisplacedContinue => 111;

// MisplacedFallthrough occurs when a fallthrough statement is not within an
// expression switch.
//
// Example:
//  func typename(i interface{}) string {
//  	switch i.(type) {
//  	case int64:
//  		fallthrough
//  	case int:
//  		return "int"
//  	}
//  	return "unsupported"
//  }
public static Code MisplacedFallthrough => 112;

// DuplicateCase occurs when a type or expression switch has duplicate
// cases.
//
// Example:
//  func printInt(i int) {
//  	switch i {
//  	case 1:
//  		println("one")
//  	case 1:
//  		println("One")
//  	}
//  }
public static Code DuplicateCase => 113;

// DuplicateDefault occurs when a type or expression switch has multiple
// default clauses.
//
// Example:
//  func printInt(i int) {
//  	switch i {
//  	case 1:
//  		println("one")
//  	default:
//  		println("One")
//  	default:
//  		println("1")
//  	}
//  }
public static Code DuplicateDefault => 114;

// BadTypeKeyword occurs when a .(type) expression is used anywhere other
// than a type switch.
//
// Example:
//  type I interface {
//  	m()
//  }
//  var t I
//  var _ = t.(type)
public static Code BadTypeKeyword => 115;

// InvalidTypeSwitch occurs when .(type) is used on an expression that is
// not of interface type.
//
// Example:
//  func f(i int) {
//  	switch x := i.(type) {}
//  }
public static Code InvalidTypeSwitch => 116;

// InvalidExprSwitch occurs when a switch expression is not comparable.
//
// Example:
//  func _() {
//  	var a struct{ _ func() }
//  	switch a /* ERROR cannot switch on a */ {
//  	}
//  }
public static Code InvalidExprSwitch => 117;

// InvalidSelectCase occurs when a select case is not a channel send or
// receive.
//
// Example:
//  func checkChan(c <-chan int) bool {
//  	select {
//  	case c:
//  		return true
//  	default:
//  		return false
//  	}
//  }
public static Code InvalidSelectCase => 118;

// UndeclaredLabel occurs when an undeclared label is jumped to.
//
// Example:
//  func f() {
//  	goto L
//  }
public static Code UndeclaredLabel => 119;

// DuplicateLabel occurs when a label is declared more than once.
//
// Example:
//  func f() int {
//  L:
//  L:
//  	return 1
//  }
public static Code DuplicateLabel => 120;

// MisplacedLabel occurs when a break or continue label is not on a for,
// switch, or select statement.
//
// Example:
//  func f() {
//  L:
//  	a := []int{1,2,3}
//  	for _, e := range a {
//  		if e > 10 {
//  			break L
//  		}
//  		println(a)
//  	}
//  }
public static Code MisplacedLabel => 121;

// UnusedLabel occurs when a label is declared and not used.
//
// Example:
//  func f() {
//  L:
//  }
public static Code UnusedLabel => 122;

// JumpOverDecl occurs when a label jumps over a variable declaration.
//
// Example:
//  func f() int {
//  	goto L
//  	x := 2
//  L:
//  	x++
//  	return x
//  }
public static Code JumpOverDecl => 123;

// JumpIntoBlock occurs when a forward jump goes to a label inside a nested
// block.
//
// Example:
//  func f(x int) {
//  	goto L
//  	if x > 0 {
//  	L:
//  		print("inside block")
//  	}
// }
public static Code JumpIntoBlock => 124;

// InvalidMethodExpr occurs when a pointer method is called but the argument
// is not addressable.
//
// Example:
//  type T struct {}
//
//  func (*T) m() int { return 1 }
//
//  var _ = T.m(T{})
public static Code InvalidMethodExpr => 125;

// WrongArgCount occurs when too few or too many arguments are passed by a
// function call.
//
// Example:
//  func f(i int) {}
//  var x = f()
public static Code WrongArgCount => 126;

// InvalidCall occurs when an expression is called that is not of function
// type.
//
// Example:
//  var x = "x"
//  var y = x()
public static Code InvalidCall => 127;

// UnusedResults occurs when a restricted expression-only built-in function
// is suspended via go or defer. Such a suspension discards the results of
// these side-effect free built-in functions, and therefore is ineffectual.
//
// Example:
//  func f(a []int) int {
//  	defer len(a)
//  	return i
//  }
public static Code UnusedResults => 128;

// InvalidDefer occurs when a deferred expression is not a function call,
// for example if the expression is a type conversion.
//
// Example:
//  func f(i int) int {
//  	defer int32(i)
//  	return i
//  }
public static Code InvalidDefer => 129;

// InvalidGo occurs when a go expression is not a function call, for example
// if the expression is a type conversion.
//
// Example:
//  func f(i int) int {
//  	go int32(i)
//  	return i
//  }
public static Code InvalidGo => 130;

// All codes below were added in Go 1.17.

// BadDecl occurs when a declaration has invalid syntax.
public static Code BadDecl => 131;

// RepeatedDecl occurs when an identifier occurs more than once on the left
// hand side of a short variable declaration.
//
// Example:
//  func _() {
//  	x, y, y := 1, 2, 3
//  }
public static Code RepeatedDecl => 132;

// InvalidUnsafeAdd occurs when unsafe.Add is called with a
// length argument that is not of integer type.
// It also occurs if it is used in a package compiled for a
// language version before go1.17.
//
// Example:
//  import "unsafe"
//
//  var p unsafe.Pointer
//  var _ = unsafe.Add(p, float64(1))
public static Code InvalidUnsafeAdd => 133;

// InvalidUnsafeSlice occurs when unsafe.Slice is called with a
// pointer argument that is not of pointer type or a length argument
// that is not of integer type, negative, or out of bounds.
// It also occurs if it is used in a package compiled for a language
// version before go1.17.
//
// Example:
//  import "unsafe"
//
//  var x int
//  var _ = unsafe.Slice(x, 1)
//
// Example:
//  import "unsafe"
//
//  var x int
//  var _ = unsafe.Slice(&x, float64(1))
//
// Example:
//  import "unsafe"
//
//  var x int
//  var _ = unsafe.Slice(&x, -1)
//
// Example:
//  import "unsafe"
//
//  var x int
//  var _ = unsafe.Slice(&x, uint64(1) << 63)
public static Code InvalidUnsafeSlice => 134;

// All codes below were added in Go 1.18.

// UnsupportedFeature occurs when a language feature is used that is not
// supported at this Go version.
public static Code UnsupportedFeature => 135;

// NotAGenericType occurs when a non-generic type is used where a generic
// type is expected: in type or function instantiation.
//
// Example:
//  type T int
//
//  var _ T[int]
public static Code NotAGenericType => 136;

// WrongTypeArgCount occurs when a type or function is instantiated with an
// incorrect number of type arguments, including when a generic type or
// function is used without instantiation.
//
// Errors involving failed type inference are assigned other error codes.
//
// Example:
//  type T[p any] int
//
//  var _ T[int, string]
//
// Example:
//  func f[T any]() {}
//
//  var x = f
public static Code WrongTypeArgCount => 137;

// CannotInferTypeArgs occurs when type or function type argument inference
// fails to infer all type arguments.
//
// Example:
//  func f[T any]() {}
//
//  func _() {
//  	f()
//  }
public static Code CannotInferTypeArgs => 138;

// InvalidTypeArg occurs when a type argument does not satisfy its
// corresponding type parameter constraints.
//
// Example:
//  type T[P ~int] struct{}
//
//  var _ T[string]
public static Code InvalidTypeArg => 139; // arguments? InferenceFailed

// InvalidInstanceCycle occurs when an invalid cycle is detected
// within the instantiation graph.
//
// Example:
//  func f[T any]() { f[*T]() }
public static Code InvalidInstanceCycle => 140;

// InvalidUnion occurs when an embedded union or approximation element is
// not valid.
//
// Example:
//  type _ interface {
//   	~int | interface{ m() }
//  }
public static Code InvalidUnion => 141;

// MisplacedConstraintIface occurs when a constraint-type interface is used
// outside of constraint position.
//
// Example:
//   type I interface { ~int }
//
//   var _ I
public static Code MisplacedConstraintIface => 142;

// InvalidMethodTypeParams occurs when methods have type parameters.
//
// It cannot be encountered with an AST parsed using go/parser.
public static Code InvalidMethodTypeParams => 143;

// MisplacedTypeParam occurs when a type parameter is used in a place where
// it is not permitted.
//
// Example:
//  type T[P any] P
//
// Example:
//  type T[P any] struct{ *P }
public static Code MisplacedTypeParam => 144;

// InvalidUnsafeSliceData occurs when unsafe.SliceData is called with
// an argument that is not of slice type. It also occurs if it is used
// in a package compiled for a language version before go1.20.
//
// Example:
//  import "unsafe"
//
//  var x int
//  var _ = unsafe.SliceData(x)
public static Code InvalidUnsafeSliceData => 145;

// InvalidUnsafeString occurs when unsafe.String is called with
// a length argument that is not of integer type, negative, or
// out of bounds. It also occurs if it is used in a package
// compiled for a language version before go1.20.
//
// Example:
//  import "unsafe"
//
//  var b [10]byte
//  var _ = unsafe.String(&b[0], -1)
public static Code InvalidUnsafeString => 146;

// InvalidUnsafeStringData occurs if it is used in a package
// compiled for a language version before go1.20.
internal static Code _ᴛ7ʗ => 147; // not used anymore

// InvalidClear occurs when clear is called with an argument
// that is not of map or slice type.
//
// Example:
//  func _(x int) {
//  	clear(x)
//  }
public static Code InvalidClear => 148;

// TypeTooLarge occurs if unsafe.Sizeof or unsafe.Offsetof is
// called with an expression whose type is too large.
//
// Example:
//  import "unsafe"
//
//  type E [1 << 31 - 1]int
//  var a [1 << 31]E
//  var _ = unsafe.Sizeof(a)
//
// Example:
//  import "unsafe"
//
//  type E [1 << 31 - 1]int
//  var s struct {
//  	_ [1 << 31]E
//  	x int
//  }
// var _ = unsafe.Offsetof(s.x)
public static Code TypeTooLarge => 149;

// InvalidMinMaxOperand occurs if min or max is called
// with an operand that cannot be ordered because it
// does not support the < operator.
//
// Example:
//  const _ = min(true)
//
// Example:
//  var s, t []byte
//  var _ = max(s, t)
public static Code InvalidMinMaxOperand => 150;

// TooNew indicates that, through build tags or a go.mod file,
// a source file requires a version of Go that is newer than
// the logic of the type checker. As a consequence, the type
// checker may produce spurious errors or fail to report real
// errors. The solution is to rebuild the application with a
// newer Go release.
public static Code TooNew => 151;

} // end errors_package
