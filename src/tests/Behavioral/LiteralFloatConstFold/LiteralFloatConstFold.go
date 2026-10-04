// A float constant built only from literals evaluates exactly in Go and rounds once. C# evaluates the converted
// operator form one step at a time, so where a step rounds differently (1 - .999) the converter folds the
// expression to Go's value; where the steps already agree (1.5 * 2.0) the operator form is kept.

package main

import "fmt"

func main() {
	q := []float64{0, 1 - .999, 1 - .99, 1 - .95}
	x := 0.1 + 0.2
	y := 1.5 * 2.0
	z := 7/2 + .5
	var f float32 = 0.1 + 0.2
	w := (1 - .999) * 1000
	fmt.Println(q, x, y, z, f, w)
	fmt.Println(q[1] == 0.001, x == 0.3, w == 1, y == 3, z == 3.5)
}
