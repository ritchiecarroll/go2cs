// liftneighbours: six one-site packages, each a move between two places that write one unnamed struct type and that
// the converter gives two C# types with no conversion between them. Go prints 3 for each. See README.md.
package main

import (
	"fmt"

	"liftneighbours/aptr"
	"liftneighbours/aslice"
	"liftneighbours/gbox"
	"liftneighbours/gpub"
	"liftneighbours/xpkgvar"
	"liftneighbours/xvar"
)

func main() {
	fmt.Println("xvar", xvar.Run())
	fmt.Println("xpkgvar", xpkgvar.Run())
	fmt.Println("aptr", aptr.Run())
	fmt.Println("aslice", aslice.Run())
	fmt.Println("gpub", gpub.Run())
	fmt.Println("gbox", gbox.Run())
}
