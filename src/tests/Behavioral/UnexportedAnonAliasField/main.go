package main

import (
	"fmt"

	"UnexportedAnonAliasField/lib"
)

// sqlite-cs S16 (variant): the unexported alias is reached ONLY as an exported field's type (no exported func names it).
func main() {
	var p lib.TuserHPALETTE
	p.FfContext = 1
	p.Fu.FhInproc = 9
	fmt.Println(p.FfContext, p.Fu.FhInproc) // Go: 1 9
}
