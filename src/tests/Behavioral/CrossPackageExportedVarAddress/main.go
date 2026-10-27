// CrossPackageExportedVarAddress guards the address of ANOTHER package's exported value variable: Go hands the
// importer the variable itself, so every write through `&lib.V` (or a method value, or a field or element pointer)
// reaches it, and two `&lib.V` are the same pointer. go.* 1.26.10.2's headline (docs/phase4/DESIGN-crosspkg-exported-box.md).
package main

import (
	"flag"
	"fmt"

	lib "go/xpkgvar"
)

func main() {
	p := &lib.N
	*p = 5
	fmt.Println("&lib.N write:", lib.N, lib.ReadN())

	fmt.Println("&lib.N identity:", &lib.N == &lib.N, p == &lib.N)

	f := lib.V.Inc
	f()
	f()
	fmt.Println("method value:", lib.V.Value())

	lib.V.Inc()
	fmt.Println("direct call:", lib.V.Value())

	fs := flag.NewFlagSet("guard", flag.ContinueOnError)
	fs.BoolVar(&lib.Flag, "x", false, "")
	fs.Parse([]string{"-x"})
	fmt.Println("flag.BoolVar:", lib.Flag)

	q := &lib.S.N
	*q = 3
	fmt.Println("field pointer:", lib.S.N, lib.ReadS())

	lib.S.WG.Add(1)
	lib.S.WG.Done()
	lib.S.WG.Wait()
	fmt.Println("field-path pointer calls: ok")

	e := &lib.Arr[1]
	*e = 7
	fmt.Println("element pointer:", lib.Arr[1], lib.Arr)

	t := &lib.Table
	*t = append(*t, 4)
	fmt.Println("lazy slice:", len(lib.Table), lib.Table)
}
