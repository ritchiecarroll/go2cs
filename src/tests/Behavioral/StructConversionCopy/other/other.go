package other

import "StructConversionCopy/lib"

// Chord has lib.Chord's fields in another package.
type Chord struct {
	Name  string
	Notes [2]int
	In    lib.Inner
}
