package p4

import (
	"example.com/multi/p5"
	"fmt"
)

var label = "p4 v0"

// Value combines this package's label with the next package's value.
func Value() string {
	return p5.Value() + fmt.Sprintf("[%s]", label)
}
