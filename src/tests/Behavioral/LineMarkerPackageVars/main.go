package main

import (
	"fmt"
	"runtime"
)

// runtime.Caller must report Go's own line for a frame a PACKAGE-LEVEL var initializer creates: the line of
// its spec, for a single var, a grouped pair, a multi-value spec and one relocated into package init by a
// forward reference. The initializers carried no position marker, so the frame inherited the last marker
// above them, an earlier function's statement.

func lineNumber() int {
	_, _, line, _ := runtime.Caller(1)
	return line
}

func lineAndOne() (int, int) {
	_, _, line, _ := runtime.Caller(1)
	return line, 1
}

var single = lineNumber()

var (
	groupedA = lineNumber()
	groupedB = lineNumber()
)

var multiA, multiB = lineAndOne()

var forward = lineNumber() + declaredLater*0

var declaredLater = 3

func main() {
	fmt.Println("single:", single)
	fmt.Println("groupedA:", groupedA)
	fmt.Println("groupedB:", groupedB)
	fmt.Println("multiA:", multiA, multiB)
	fmt.Println("forward:", forward)
}
