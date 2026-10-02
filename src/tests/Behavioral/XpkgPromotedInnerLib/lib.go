// XpkgPromotedInnerLib is the innermost library of a three-package guard for Go's promotion of an
// embedded struct's methods ACROSS packages. A struct that embeds a struct of another package has
// that embed's exported methods in its own method set, and go2cs reads a run-time method set off
// the extension methods it emitted, so every such promotion needs a forwarder: with none emitted,
// `struct{ XpkgPromotedInnerLib.Inner }` had NumMethod 0 where Go says 5.
//
// This package declares the methods that get promoted: four with a value receiver and one, Set,
// with a pointer receiver. XpkgPromotedMidLib embeds Inner once more, and XpkgPromotedMethodSet is
// the program that reads the method sets.
package XpkgPromotedInnerLib

import "runtime"

// Token is an exported result type, so a promoted Tok keeps a public signature across packages.
type Token struct{ N int }

type Inner struct{ X int }

func (i Inner) Tok() Token         { return Token{i.X} }
func (i Inner) Name() string       { return "inner" }
func (i Inner) Pair() (int, error) { return i.X, nil }
func (i *Inner) Set(v int)         { i.X = v }
func (i Inner) String() string     { return "Inner!" }

// Where reports whether its caller could be named: a call through a promoted forwarder still has
// a caller frame.
func (i Inner) Where() bool {
	_, _, _, ok := runtime.Caller(1)

	return ok
}
