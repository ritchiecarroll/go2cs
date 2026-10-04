package p6

import (
	"example.com/multi/p7"
	"fmt"
)

var label = "p6 v0"

// Value combines this package's label with the next package's value.
func Value() string {
	return p7.Value() + fmt.Sprintf("[%s]", label)
}
