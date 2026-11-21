// Package text declares the constraint the iterators package's generic types take: its import path has a slash
// (GenericHolderForeignConstraint/text), which is what the holder's name must not depend on.
package text

// Text is the set of byte-sequence types an Iterator walks.
type Text interface{ ~[]byte | ~string }
