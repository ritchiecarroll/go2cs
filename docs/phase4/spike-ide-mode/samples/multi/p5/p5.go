package p5

import (
	"example.com/multi/p6"
	"fmt"
)

var label = "p5 v0"

// Value combines this package's label with the next package's value.
func Value() string {
	return p6.Value() + fmt.Sprintf("[%s]", label)
}
