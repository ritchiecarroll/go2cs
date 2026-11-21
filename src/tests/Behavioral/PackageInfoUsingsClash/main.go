// PackageInfoUsingsClash guards the package_info.cs clash fallback: its clashlib subpackage declares types named
// MethodImpl and StackTrace, which `using System.Runtime.CompilerServices;` and `using System.Diagnostics;` would
// make ambiguous in clashlib's package_info.cs, so that file keeps the global:: spelling of its System attributes.
// This program calls each of their methods across the assembly boundary.
package main

import (
	"fmt"

	"PackageInfoUsingsClash/clashlib"
)

func main() {
	m := clashlib.New(2)
	fmt.Println(m.Get())

	m.Bump(5)
	fmt.Println(m.Get())

	p := &m
	p.Bump(1)
	fmt.Println(p.Get(), m.Get())

	get := clashlib.MethodImpl.Get
	fmt.Println(get(m))

	var trace clashlib.StackTrace
	trace.Push("main")
	trace.Push("caller")
	fmt.Println(trace.String(), len(trace))
	fmt.Println(trace.Describe().String())
	fmt.Println(trace)
}
