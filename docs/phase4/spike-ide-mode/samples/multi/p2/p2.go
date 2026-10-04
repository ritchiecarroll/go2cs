package p2

import (
	"example.com/multi/p3"
	"fmt"
)

var label = "p2 v0"

// Value combines this package's label with the next package's value.
func Value() string {
	return p3.Value() + fmt.Sprintf("[%s]", label)
}
