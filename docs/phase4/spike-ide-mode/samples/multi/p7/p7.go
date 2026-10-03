package p7

import (
	"example.com/multi/p8"
	"fmt"
)

var label = "p7 v0"

// Value combines this package's label with the next package's value.
func Value() string {
	return p8.Value() + fmt.Sprintf("[%s]", label)
}
