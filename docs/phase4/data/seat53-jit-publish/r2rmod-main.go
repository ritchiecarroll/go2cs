// r2rmod: a loop entered once in main, and one in a library package of the same module. Each calls a small
// standard-library function whose result feeds the next iteration.
package main

import (
	"fmt"
	"strconv"
	"time"

	"example.com/r2rmod/lib"
)

func main() {
	const n = 20000000
	acc := 0
	start := time.Now()
	for i := 0; i < n; i++ {
		acc += len(strconv.FormatBool((i+acc)&1 == 0))
	}
	mid := time.Now()
	acc += lib.Count(n, acc)
	end := time.Now()
	fmt.Println("checksum:", acc)
	fmt.Println("main_ns:", float64(mid.Sub(start).Nanoseconds())/n)
	fmt.Println("lib_ns:", float64(end.Sub(mid).Nanoseconds())/n)
}
