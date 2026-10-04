package p9

import (
	"fmt"
)

var label = "p9 v70003"

// Value combines this package's label with the next package's value.
func Value() string {
	return fmt.Sprintf("[%s]", label)
}
