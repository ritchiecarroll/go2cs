// A nil comparison on a SLICE-constrained TYPE PARAMETER. Go 1.24's slices.Clone is the arrival —
// `if s == nil { return nil }` inside `Clone[S ~[]E, E any]`, where 1.23 wrote
// `return append(s[:0:0], s...)` and never compared — but nothing about the shape is 1.24-only, so
// it is guarded here at the corpus pin.
//
// WHY IT NEEDS A GUARD AT ALL: C# forbids a user-defined operator on a type parameter, so the
// `x == default!` form every CONCRETE slice site uses does not compile when the operand is `S`.
// The family interface's IsNil carries the check, and the constraint already names that interface,
// so the emission is a constrained call. GenericTypeInference already guards the MAP half of the
// same rule; this is the slice half.
//
// THE ROW THAT MATTERS IS THE EMPTY NON-NIL ONE. Two plausible remedies compile and get it wrong:
// EqualityComparer routes to structural CONTENT equality where `operator ==` is header identity,
// and IArray.Source materializes a detached copy that is empty rather than null for a nil slice.
// Both answer TRUE for an empty non-nil slice, where Go answers false — so `emptyNotNil` below is
// what separates a correct fix from either of them, and it asserts a VALUE rather than compiling.
package main

import "fmt"

// isNilSlice is the shape under test: the nil test on the type parameter itself.
func isNilSlice[S ~[]E, E any](s S) bool {
	return s == nil
}

// isSetSlice takes the `!=` direction, which must negate the same member rather than emit an
// operator of its own.
func isSetSlice[S ~[]E, E any](s S) bool {
	return s != nil
}

// cloneOrNil mirrors 1.24's slices.Clone: the nil-preserve guard plus a composite literal OF the
// type parameter, which is the second construct that release introduced.
func cloneOrNil[S ~[]E, E any](s S) S {
	if s == nil {
		return nil
	}
	return append(S{}, s...)
}

// Named is a DEFINED slice type: its C# form is a generated wrapper that delegates the interface to
// an inner slice, so instantiating the type parameter with it exercises the generated member rather
// than golib's own.
type Named []int

func main() {
	var nilSlice []int
	emptyNotNil := []int{}
	filled := []int{3, 1, 2}

	fmt.Println(isNilSlice(nilSlice), isNilSlice(emptyNotNil), isNilSlice(filled))
	fmt.Println(isSetSlice(nilSlice), isSetSlice(emptyNotNil), isSetSlice(filled))

	// cloneOrNil preserves nilness: a nil in is a nil out, and an empty non-nil in is empty non-nil out.
	fmt.Println(cloneOrNil(nilSlice) == nil, cloneOrNil(emptyNotNil) == nil, len(cloneOrNil(filled)))

	// The DEFINED type instantiates the same parameter through a generated wrapper.
	var nilNamed Named
	fmt.Println(isNilSlice(nilNamed), isNilSlice(Named{}), isSetSlice(Named{7}))
}
