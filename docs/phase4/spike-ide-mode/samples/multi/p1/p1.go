package p1

import (
	"example.com/multi/p2"
	"fmt"
)

var label = "p1 v0"

// Value combines this package's label with the next package's value.
func Value() string {
	return p2.Value() + fmt.Sprintf("[%s]", label)
}
