// Each lib function below moves a value between sites of one anonymous type, in one file and across two.
package main

import (
	"fmt"

	"AnonLiftOnePerType/lib"
)

func main() {
	h := lib.Mk(1)
	fmt.Println(h.V.A(), h.V.B())

	h2 := lib.Mk2(3)
	fmt.Println(h2.W.A(), h2.W.B())

	b := lib.MkBox(5)
	fmt.Println(b.P.X, b.P.Y)

	b2 := lib.MkBox2(7)
	fmt.Println(b2.Q.X, b2.Q.Y)
}
