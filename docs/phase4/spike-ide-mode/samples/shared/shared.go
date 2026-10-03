package shared

import (
	"fmt"

	"example.com/shared/words"
)

// Greet returns a greeting for name.
func Greet(name string) string {
	return fmt.Sprintf("%s, %s (%d words)", words.Hello(), name, words.Count())
}
