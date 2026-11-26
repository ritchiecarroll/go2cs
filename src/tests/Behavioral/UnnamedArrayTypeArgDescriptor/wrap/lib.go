// Package wrap is an exported generic forwarder to unique.Make, in another
// package (and another C# assembly) than its caller.
package wrap

import "unique"

// Wrap returns the unique handle of v. Its type parameter reaches
// unique.Make, so its caller's type argument decides which map Make uses.
func Wrap[T comparable](v T) unique.Handle[T] {
	return unique.Make(v)
}

// Same reports whether a and b have the same unique handle.
func Same[T comparable](a, b T) bool {
	return Wrap(a) == Wrap(b)
}
