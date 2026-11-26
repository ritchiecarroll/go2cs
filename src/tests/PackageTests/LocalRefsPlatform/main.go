// Fixture for test-local-refs-platform.ps1: a program converted with go2cs -recurse in local-reference mode. Its
// output reaches the runtime's platform-specific code (os, time) so the wrong standard-library flavour fails at start.
package main

import (
	"fmt"
	"os"
	"strings"
)

func main() {
	_, err := os.Stat(".")
	fmt.Println(strings.ToUpper("local refs"), err == nil, len(os.Args) > 0)
}
