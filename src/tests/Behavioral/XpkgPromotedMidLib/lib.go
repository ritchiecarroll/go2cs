// XpkgPromotedMidLib is the middle library of the cross-package promotion guard (see
// XpkgPromotedInnerLib). Each type here embeds a struct of ANOTHER package, or sets up a name that
// a third package must not inherit, so the program in XpkgPromotedMethodSet reaches Inner's
// methods through TWO package boundaries.
package XpkgPromotedMidLib

import "XpkgPromotedInnerLib"

// Mid embeds Inner by value: the middle hop of a promotion that crosses two packages.
type Mid struct{ XpkgPromotedInnerLib.Inner }

type loc2 struct{}

func (loc2) Tok() XpkgPromotedInnerLib.Token { return XpkgPromotedInnerLib.Token{N: 9} }

// Mid2 holds Tok twice at depth 1 (Inner.Tok and loc2.Tok), so Tok is ambiguous inside Mid2 and
// absent from its method set. A type that embeds Mid2 must not gain it.
type Mid2 struct {
	XpkgPromotedInnerLib.Inner
	loc2
}

type r1 struct{ n int }

func (r *r1) Ping() int { r.n++; return r.n }

// SameP and SameV promote a POINTER-receiver method inside ONE package: through a pointer embed
// (Ping is in SameP's value method set) and through a value embed (in *SameV's set only).
type SameP struct{ *r1 }
type SameV struct{ r1 }

func NewSameP() SameP { return SameP{&r1{}} }

type wrap struct{ XpkgPromotedInnerLib.Inner }

// Mid3 holds Inner's Tok at depth 2, which is depth 3 from a type that embeds Mid3.
type Mid3 struct{ wrap }

// HasTok carries a FIELD named Tok.
type HasTok struct{ Tok int }

// MidF holds Inner.Tok (a method) beside HasTok.Tok (a field), both at depth 1: ambiguous in Go.
type MidF struct {
	XpkgPromotedInnerLib.Inner
	HasTok
}

type inner3 struct{}

func (inner3) Tok() XpkgPromotedInnerLib.Token { return XpkgPromotedInnerLib.Token{N: 3} }

// PS has a field Tok at depth 0 that shadows inner3.Tok. A type that embeds PS has the FIELD Tok,
// and no method of that name.
type PS struct {
	inner3
	Tok int
}
