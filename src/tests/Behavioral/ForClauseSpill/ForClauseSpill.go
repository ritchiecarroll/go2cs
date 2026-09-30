package main

import "fmt"

// A for-loop CLAUSE whose statement forwards a multi-value call -- `keep(next())`, `must(next())` -- spills the
// pair into a pre-statement. A clause has no statement slot: the INIT runs once, first; the COND runs before every
// pass (so `continue` re-evaluates it); the POST runs after every pass (so every `continue` must reach it).
// Every loop is capped, so a clause evaluated the wrong number of times shows as a count, never a hang.

var n int

func next() (int, bool) {
	n++
	return n, n < 4
}

func keep(v int, ok bool) bool { return ok }

func must(v int, ok bool) int { return v }

func main() {
	// COND, while form.
	n = 0
	count := 0
	for keep(next()) {
		count++
		if count > 10 {
			break
		}
	}
	fmt.Println("while cond:", count, n)

	// COND, three-clause form, with continue.
	n = 0
	count = 0
	for i := 0; keep(next()); i++ {
		if count++; count > 10 {
			break
		}
		if i%2 == 0 {
			continue
		}
		fmt.Println("  cond i:", i)
	}
	fmt.Println("for cond:", count, n)

	// INIT and POST, with continue.
	n = 0
	for i := must(next()); i < 6; i = must(next()) {
		if i%2 == 0 {
			continue
		}
		fmt.Println("  post i:", i)
	}

	// POST reached by a continue inside a switch, and by a labeled continue from an inner loop.
	n = 0
outer:
	for i := must(next()); i < 7; i = must(next()) {
		switch {
		case i == 2:
			continue
		}
		for j := 0; j < 2; j++ {
			if i == 4 && j == 1 {
				continue outer
			}
		}
		fmt.Println("  outer i:", i)
	}

	// POST beside a per-iteration capture (Go 1.22: each closure sees its own i).
	n = 0
	var funcs []func() int
	for i := must(next()); i < 4; i = must(next()) {
		funcs = append(funcs, func() int { return i })
	}
	for _, f := range funcs {
		fmt.Print(f(), " ")
	}
	fmt.Println()
}
