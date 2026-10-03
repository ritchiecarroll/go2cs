package p8

import (
	"example.com/multi/p9"
	"fmt"
)

var label = "p8 v0"

// Value combines this package's label with the next package's value.
func Value() string {
	return p9.Value() + fmt.Sprintf("[%s]", label)
}
