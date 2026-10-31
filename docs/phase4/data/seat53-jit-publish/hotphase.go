// hotphase: a short hot phase right after start. Each loop calls a small standard-library function that returns a
// hoisted literal; the result feeds the next index, so every call sits on the loop's critical path.
package main

import (
	"crypto"
	"fmt"
	"strconv"
	"time"
)

func main() {
	hashes := []crypto.Hash{crypto.MD5, crypto.SHA1, crypto.SHA256, crypto.SHA512}
	const n = 2000000
	acc := 0
	start := time.Now()
	for i := 0; i < n; i++ {
		acc += len(hashes[(i+acc)&3].String())
	}
	mid := time.Now()
	for i := 0; i < n; i++ {
		acc += len(strconv.FormatBool((i+acc)&1 == 0))
	}
	end := time.Now()
	fmt.Println("checksum:", acc)
	fmt.Println("hash_ns:", float64(mid.Sub(start).Nanoseconds())/n)
	fmt.Println("bool_ns:", float64(end.Sub(mid).Nanoseconds())/n)
}
