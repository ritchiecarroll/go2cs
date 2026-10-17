// A file that dot-imports one package and imports a second package of the SAME NAME. Every type
// reference to the dot-imported package must still name that package, never the named import's.
// crypto/mlkem's external test is the corpus instance: it dot-imports crypto/mlkem and imports
// crypto/internal/fips140/mlkem.
package main

import (
	"fmt"

	. "DotImportNameCollision/a/keys"
	"DotImportNameCollision/b/keys"
)

type holder struct {
	k *Key
	v Count
}

func first[T any](xs []T) T { return xs[0] }

func main() {
	var k *Key = NewKey("dot")
	var v Count = 3
	ks := []*Key{k, NewKey("second")}
	m := map[string]Count{"x": v}
	h := holder{k: k, v: v}
	var b Box[*Key] = Wrap[*Key](k)
	f := first[*Key](ks)

	other := keys.NewKey(7)

	fmt.Println(k.Name(), v, len(ks), m["x"], h.k.Name(), h.v, b.V.Name(), f.Name(), other.ID())
}
