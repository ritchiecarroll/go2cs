// Go's order in an if condition: gc reads a plain operand of the condition after its calls.
package main

import "fmt"

func main() {
	z := 1
	setZ := func() int { z = 10; return 2 }
	if z < setZ() {
		fmt.Println("condition: z read first (1 < 2)")
	} else {
		fmt.Println("condition: z read after the call (10 < 2 is false)")
	}

	w := 1
	setW := func() int { w = 10; return 2 }
	if n := 3; w+setW() == 12 {
		fmt.Println("init and condition: w read after the call, n =", n)
	} else {
		fmt.Println("init and condition: w read first, n =", n)
	}

	u := 1
	setU := func() int { u = 10; return 2 }
	if false {
	} else if u == setU()*5 {
		fmt.Println("else if: u read after the call")
	} else {
		fmt.Println("else if: u read first")
	}
}
