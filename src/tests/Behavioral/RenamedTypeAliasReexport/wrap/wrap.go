// Package wrap re-exports lib's types by alias, as tsc's internal/json re-exports jsontext's.
package wrap

import "RenamedTypeAliasReexport/lib"

type Encoder = lib.Encoder
type Kind = lib.Kind
type Tok = lib.Tok
type Token = lib.Token

func NewEncoder(n int) *Encoder { return lib.NewEncoder(n) }

func NewTok(k Kind) Tok { return lib.NewTok(k) }

func Read() Token { return lib.Reader{}.Token() }
