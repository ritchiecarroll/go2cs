// Package lib declares types whose names are also method names in this package, so the converted package
// Δ-renames them (go-json-experiment's jsontext: `type Encoder` beside `export.Encoder`, `type Kind` beside
// `Token.Kind`). Token is the two-hop form: an empty-interface type renamed for a method of the same name.
package lib

type Encoder struct{ n int }

type export struct{}

func (export) Encoder(e *Encoder) int { return e.n }

func NewEncoder(n int) *Encoder { return &Encoder{n: n} }

func (e *Encoder) N() int { return e.n }

func (e *Encoder) Bump() { e.n++ }

type Kind byte

type Tok struct{ k Kind }

func (t Tok) Kind() Kind { return t.k }

func NewTok(k Kind) Tok { return Tok{k: k} }

// Token collides with Reader.Token, and is an empty interface.
type Token any

type Reader struct{}

func (Reader) Token() Token { return "tok" }
