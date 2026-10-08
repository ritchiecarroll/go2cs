// The fixture for docs/PLAN-marker-comment-parity.md section 11: the inline attributes that leave converted code
// for a record (s11Records_test.go).
package main

import (
	"crypto"
	"fmt"
	"reflect"
)

// A defined type over an interface is emitted as a `using` alias, so a field of it is recorded with its
// descriptor carrier: a local one, an imported one, a grouped pair and an embedded one.
type Token any

type Named fmt.Stringer

type holder struct {
	Tok   Token
	Key   crypto.PublicKey
	A, B  Named
	Plain any
	Token
}

func main() {
	t := reflect.TypeFor[holder]()
	for i := 0; i < t.NumField(); i++ {
		f := t.Field(i)
		fmt.Printf("%s %q %v\n", f.Name, f.Type.Name(), f.Anonymous)
	}
}
