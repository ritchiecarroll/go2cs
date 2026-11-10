// RenamedTypeAliasReexport reaches another package's Δ-renamed types ONLY through a third package's alias
// re-export: main imports wrap, whose `Encoder = lib.Encoder` (and Kind, Tok, Token) name lib's types, and main never
// imports lib. That is tsc's collections writing `*json.Encoder` over its own internal/json, which re-exports
// jsontext. Every position: a parameter, a result, a struct field, a slice, a map value, a local, a conversion, a
// composite literal, a type assertion, a type switch, a method value, and the two-hop empty-interface form (Token).
// TS readiness row C22e.
package main

import (
	"fmt"

	"RenamedTypeAliasReexport/wrap"
)

type state struct {
	enc   *wrap.Encoder
	kinds []wrap.Kind
	byKey map[string]wrap.Tok
}

func use(e *wrap.Encoder) int { return e.N() }

func makeKind(n int) wrap.Kind { return wrap.Kind(n) }

func main() {
	s := state{enc: wrap.NewEncoder(2), byKey: map[string]wrap.Tok{}}
	s.enc.Bump()
	s.kinds = append(s.kinds, makeKind(1), wrap.Kind(4))
	s.byKey["a"] = wrap.NewTok(7)
	fmt.Println(use(s.enc), s.kinds, s.byKey["a"].Kind(), s.kinds[1] == wrap.Kind(4))

	var e wrap.Encoder
	e.Bump()
	bump := e.Bump
	bump()
	fmt.Println(e.N())

	var anyv any = wrap.Kind(9)
	if k, ok := anyv.(wrap.Kind); ok {
		fmt.Println("kind", k)
	}
	switch v := anyv.(type) {
	case *wrap.Encoder:
		fmt.Println("encoder", v.N())
	case wrap.Kind:
		fmt.Println("switch kind", v)
	}

	var t wrap.Token = wrap.Read()
	fmt.Println(t, wrap.Tok{} == wrap.NewTok(0))
}
