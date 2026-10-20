// Since Go 1.26 a heap-profile record starts at the allocator: runtime.mallocgc, then the entry point the
// compiler called (makeslice for make, growslice for an append that grows, newobject for new and &T{}),
// then the allocating function. Go 1.24's records started at the allocating function. The debug=1 text
// hides the runtime frames from its symbolized lines, but its `@` header counts every PC, so
// runtime/pprof's TestMemoryProfiler now expects six of them where it expected four.
package main

import (
	"fmt"
	"runtime"
	"sort"
	"strings"
)

type T struct {
	a, b, c, d int
	next       *T
}

var (
	sinkBytes []byte
	sinkInts  []int
	sinkT     *T
	sinkLit   *T
)

//go:noinline
func allocMake() { sinkBytes = make([]byte, 1<<16) }

// The first make is 32 bytes so that it takes the small allocator: Go's tiny allocator (noscan, under 16
// bytes) returns before profiling when the allocation fits the current tiny block, so an 8-byte make is
// recorded only sometimes.
//
//go:noinline
func allocGrow() {
	s := make([]int, 0, 4)
	for i := 0; i < 64; i++ {
		s = append(s, i)
	}
	sinkInts = s
}

//go:noinline
func allocNew() { sinkT = new(T) }

//go:noinline
func allocLiteral() { sinkLit = &T{a: 1} }

func main() {
	runtime.MemProfileRate = 1
	allocMake()
	allocGrow()
	allocNew()
	allocLiteral()
	runtime.GC()

	n, _ := runtime.MemProfile(nil, true)
	records := make([]runtime.MemProfileRecord, n+64)
	n, _ = runtime.MemProfile(records, true)

	seen := map[string]bool{}
	var lines []string
	for _, r := range records[:n] {
		var names []string
		for _, pc := range r.Stack() {
			f := runtime.FuncForPC(pc - 1)
			if f == nil {
				names = append(names, "?")
				continue
			}
			name := f.Name()
			names = append(names, name)
			if strings.HasPrefix(name, "main.alloc") {
				line := strings.Join(names, " < ")
				if !seen[line] {
					seen[line] = true
					lines = append(lines, line)
				}
				break
			}
		}
	}
	sort.Strings(lines)
	for _, l := range lines {
		fmt.Println(l)
	}
}
