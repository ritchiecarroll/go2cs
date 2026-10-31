package lib

import "strconv"

// Count runs a loop entered once, in a package of the user's own module.
func Count(n, seed int) int {
	t := seed
	for i := 0; i < n; i++ {
		t += len(strconv.FormatBool((i+t)%3 == 0))
	}
	return t
}
