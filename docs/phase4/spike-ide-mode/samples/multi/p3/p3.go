package p3

import (
	"example.com/multi/p4"
	"fmt"
)

var label = "p3 v0"

// Value combines this package's label with the next package's value.
func Value() string {
	return p4.Value() + fmt.Sprintf("[%s]", label)
}
