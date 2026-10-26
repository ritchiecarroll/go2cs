package main

import (
	"fmt"
	"io/fs"
	"sort"
	"time"
)

type localErr struct{ msg string }

func (e *localErr) Error() string { return e.msg }

const n = 5_000_000

func bench(name string, v any) {
	var runs []float64
	for r := 0; r < 9; r++ {
		hits := 0
		t0 := time.Now()
		for i := 0; i < n; i++ {
			if _, ok := v.(error); ok {
				hits++
			}
		}
		runs = append(runs, float64(time.Since(t0).Nanoseconds())/n)
		if hits != n {
			fmt.Println(name, "MISS", hits)
			return
		}
	}
	sort.Float64s(runs)
	fmt.Printf("%s: median %.1f ns/assert (min %.1f, max %.1f, 9 runs x %d)\n", name, runs[4], runs[0], runs[8], n)
}

func main() {
	bench("raw *localErr (same assembly)", &localErr{msg: "x"})
	bench("raw *fs.PathError (foreign)", &fs.PathError{Op: "read", Path: "p"})
}
