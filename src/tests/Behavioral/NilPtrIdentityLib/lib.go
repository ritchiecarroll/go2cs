// Package NilPtrIdentityLib is the far side of NilPtrIdentity: a generic helper and a default value in
// another package, the typescript-go core.OrElse shape (TS C49).
package NilPtrIdentityLib

// Message is a pointer target.
type Message struct {
	Code int
}

// Default is another package's var, the value OrElse falls back to.
var Default = &Message{Code: 2344}

// OrElse returns value unless it is T's zero, Go's `value != *new(T)` idiom.
func OrElse[T comparable](value T, orElse T) T {
	if value != *new(T) {
		return value
	}

	return orElse
}

// IsZero compares against a declared zero instead of *new(T).
func IsZero[T comparable](value T) bool {
	var zero T
	return value == zero
}

// Same is generic == over two operands.
func Same[T comparable](a, b T) bool {
	return a == b
}
