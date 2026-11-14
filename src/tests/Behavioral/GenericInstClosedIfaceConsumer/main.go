package main

import (
	"fmt"

	"GenericInstClosedIfaceConsumer/boxlib"
)

// The importing side of a closed pair: boxlib converts Box[Point] to Valuer itself, and so does
// this package. The declaring package's record does not make Box<Point> a Valuer in C#, so this
// package wraps as well, and the two carriers hold equal values.

func main() {
	lib := boxlib.Wrap(1, 2)
	fmt.Println("library", lib.Val().X, lib.Val().Y)

	var own boxlib.Valuer = boxlib.Box[boxlib.Point]{V: boxlib.Point{X: 1, Y: 2}}
	fmt.Println("consumer", own.Val().X, own.Val().Y)

	fmt.Println("equal", lib == own)

	back, ok := lib.(boxlib.Box[boxlib.Point])
	fmt.Println("assert back", back.V.X, back.V.Y, ok)

	other := boxlib.Wrap(3, 4)
	fmt.Println("not equal", other == own)
}
