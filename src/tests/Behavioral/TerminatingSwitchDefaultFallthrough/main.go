package main

import "fmt"

// A function that ends in a TERMINATING switch needs no final return in Go: the switch has a
// default, no clause breaks out of it, and every clause either returns, loops forever or falls
// through to a clause that does (the Go spec's terminating statements). The converter lowers the
// switch to an if-chain whose default is a guarded block, so C# sees a reachable end and refused
// to compile it (CS0161) until the lowering emits the unreachable `return default;`.
// modernc.org/libc pthread.go `(*mutex).lock` has this shape (sqlite-cs row S13).

// The default comes first and falls through to a returning clause; the other clause loops
// until it returns.
func lock(typ int) int32 {
	switch typ {
	default:
		fallthrough
	case 0:
		return 10
	case 1:
		for n := 0; ; n++ {
			if n == 3 {
				return int32(20 + n)
			}
		}
	}
}

// The default sits between two clauses and falls through to the last one.
func middle(n int) string {
	switch n {
	case 0:
		return "zero"
	default:
		fallthrough
	case 1:
		return fmt.Sprint("one or other ", n)
	}
}

// The same switch with a clause that does not return is NOT terminating, so the function keeps
// its own final return, and that return runs.
func notTerminal(n int) int {
	switch n {
	default:
		fallthrough
	case 0:
		n += 100
	case 1:
		return 1
	}
	return n
}

func main() {
	fmt.Println(lock(0), lock(1), lock(7))
	fmt.Println(middle(0), middle(1), middle(5))
	fmt.Println(notTerminal(0), notTerminal(1), notTerminal(5))
}
