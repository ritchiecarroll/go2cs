package main

import (
	"fmt"
	"os"
	"runtime"
)

// Calls inside SLICE LITERALS report a Go line no earlier than their own literal's, and a struct element
// reports its own first line (seat #54). Every printed value is a line number of this file.
//
// An optimized build cannot answer every call's own line (calls that share an expression report one IL
// offset; the D1b disclosure), so by default the program prints only what holds in every configuration.
// With GO2CS_SLICE_LITERAL_LINES=1 it prints each element's line, and GolibTests'
// CallerLineSliceLiteralsReleaseTests builds it Release, runs it with tiering off, and checks the two
// invariants against Go's values: no element answers a line before its own spec's, and each struct
// element's first field answers that field's own line.
var lines = os.Getenv("GO2CS_SLICE_LITERAL_LINES") == "1"

func line() int {
	_, _, l, _ := runtime.Caller(1)
	return l
}

type pair struct{ a, b int }

// An array spec, then a slice spec: runtime's arrayLit and sliceLit, where the slice's calls once answered
// the array's line.
var arrayFirst = [3]int{line(),
	line(),
	line(),
}

var sliceAfterArray = []int{line(),
	line(),
	line(),
}

// A slice of structs whose fields are calls, one element on the spec's line and one across two lines:
// runtime's compLit.
var structs = []pair{{line(), line()},
	{
		line(),
		line(),
	},
}

var nested = [][]int{{line(), line()},
	{line(),
		line()},
}

var pointers = []*pair{{line(), line()},
	{
		line(),
		line(),
	},
}

func show(label string, values ...int) {
	if lines {
		fmt.Println(label, values)
	}
}

func main() {
	local := []int{line(),
		line(),
	}

	localStructs := []pair{{line(), line()},
		{
			line(),
			line(),
		},
	}

	show("arrayFirst", arrayFirst[:]...)
	show("sliceAfterArray", sliceAfterArray...)
	show("structs", structs[0].a, structs[0].b, structs[1].a, structs[1].b)
	show("nested", nested[0][0], nested[0][1], nested[1][0], nested[1][1])
	show("pointers", pointers[0].a, pointers[0].b, pointers[1].a, pointers[1].b)
	show("local", local...)
	show("localStructs", localStructs[0].a, localStructs[0].b, localStructs[1].a, localStructs[1].b)

	fmt.Println("lengths:", len(arrayFirst), len(sliceAfterArray), len(structs), len(nested), len(pointers), len(local), len(localStructs))
}
