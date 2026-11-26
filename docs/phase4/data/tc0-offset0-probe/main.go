package main

import (
	"fmt"

	"ZzCallDepthProbe/lg"
)

func main() {
	l := &lg.Logger{}
	l.Log("")
	fmt.Println("method variadic", lg.Where)
	l.Info("")
	fmt.Println("method plain", lg.Where)
	lg.Top("")
	fmt.Println("func variadic", lg.Where)
}
