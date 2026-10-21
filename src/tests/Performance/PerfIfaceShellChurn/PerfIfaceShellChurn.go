package main

import (
	"errors"
	"fmt"
	"io/fs"
	"time"
)

// The CHURN counterpart of PerfIfaceShell: every assert sees a NEW dynamic value. PerfIfaceShell
// re-asserts the same two values, which is the case a shell cache wins on; this row is the case a cache
// can only lose on, so the two rows together bound what any shell cache costs and saves.
//
// One iteration is three runtime duck-typed asserts on fresh values, each through the shell tier (see
// PerfIfaceShell for why an anonymous interface reaches it):
//
//   - a value-typed dynamic value (`counter`, a one-field struct with a value-receiver method), which
//     takes the reflective object shell;
//   - a pointer-sourced dynamic value (`&box{}`), which takes the delegate-bound generic shell;
//   - errors.Is over a new *fs.PathError each time: the shape real code reaches, since errors.Is asserts
//     its argument to the anonymous `interface{ Is(error) bool }` and `interface{ Unwrap() error }`.
type counter struct{ n int }

func (c counter) Len() int { return c.n }

type box struct{ n int }

func (b *box) Len() int { return b.n }

func run(n int) int {
	total := 0

	for i := 0; i < n; i++ {
		var v any = counter{n: i & 7}

		if l, ok := v.(interface{ Len() int }); ok {
			total += l.Len()
		}

		var p any = &box{n: i & 3}

		if l, ok := p.(interface{ Len() int }); ok {
			total += l.Len()
		}

		err := error(&fs.PathError{Op: "open", Path: "churn", Err: fs.ErrNotExist})

		if errors.Is(err, fs.ErrNotExist) {
			total++
		}
	}

	return total
}

func main() {
	start := time.Now().UnixNano()

	total := run(1000000)

	elapsed := time.Now().UnixNano() - start
	fmt.Println("checksum:", total)
	fmt.Println("elapsed_ns:", elapsed)
}
