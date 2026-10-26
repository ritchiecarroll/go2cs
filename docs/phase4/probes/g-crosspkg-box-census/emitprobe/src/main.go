package main

import (
	"fmt"

	"xpprobe/lib"
)

func main() {
	p := &lib.N
	*p = 5
	f := lib.V.Inc
	f()
	lib.V.Inc()
	q := &lib.S.N
	*q = 3
	lib.S.WG.Add(1)
	lib.S.WG.Done()
	e := &lib.Arr[1]
	*e = 7
	r := &lib.RW
	r.Lock()
	r.Unlock()
	t := &lib.Table
	*t = append(*t, 4)
	fmt.Println(lib.ReadN(), lib.V.Value(), lib.S.N, lib.Arr[1], len(lib.Table), &lib.N == &lib.N)
}
