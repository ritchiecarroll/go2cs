// Guards the init-statement scope shapes (docs/PLAN-init-statement-scope.md): an if, switch or type-switch
// init whose names collide with nothing becomes a statement of the enclosing block; a comma-ok init folds
// into its condition, golib's out-parameter assertion or a pattern on the tuple; and every refusal class
// keeps the block, so each value below is the one Go computes.
package main

import (
	"errors"
	"fmt"
)

type Shape interface{ Area() int }
type Named interface{ Name() string }

type Square struct{ side int }

func (s Square) Area() int { return s.side * s.side }

type Label string

func (l Label) Name() string { return string(l) }

const limit = 5

func pair(n int) (int, error) {
	if n < 0 {
		return 0, errors.New("negative")
	}
	return n * 2, nil
}

func next(n int) int { return n + 1 }

// The block goes: the init is a statement of this function's body.
func dropped(n int) string {
	if m := next(n); m > 3 {
		return fmt.Sprint("big ", m)
	} else if m > 1 {
		return fmt.Sprint("mid ", m)
	}
	switch k := n * 3; k {
	case 3:
		return "three"
	}
	return "small"
}

func describe(x any) string {
	switch y := x; v := y.(type) {
	case Shape:
		return fmt.Sprint("shape ", v.Area())
	case Named:
		return "named " + v.Name()
	}
	return "other"
}

// Two siblings that refuse each other only by `ok` fold into golib's out-parameter assertion.
func classify(x any) string {
	if s, ok := x.(Shape); ok {
		return fmt.Sprint("area ", s.Area())
	}
	if l, ok := x.(Named); ok {
		return "name " + l.Name()
	}
	if _, ok := x.(error); !ok {
		return "not an error"
	}
	return "error"
}

// Map lookups fold into a pattern on the comma-ok tuple: `is (var v, true)` and `is not (_, true)`.
func lookup(m map[string]int, k string) int {
	if v, ok := m[k]; ok {
		return v
	}
	if w, ok := m[k+k]; ok {
		return -w
	}
	if _, ok := m["zero"]; !ok {
		return 0
	}
	return 100
}

// An else-if's own init folds into its condition, by assertion, by pattern or by a `var` pattern.
func chain(x any, m map[string]int, n int) string {
	if x == nil {
		return "nil"
	} else if s, ok := x.(Shape); ok {
		return fmt.Sprint("shape ", s.Area())
	} else if v, ok := m["k"]; !ok {
		return "no k"
	} else if d, err := pair(n - v); err != nil || d > 10 {
		return fmt.Sprint("pair ", d, " ", err)
	} else if e := errors.New("fallback"); e != nil {
		return e.Error()
	}
	return "end"
}

// The rebinding case: a later `limit` names the package const, so the init keeps its block.
func rebinding() int {
	if limit := next(0); limit > 0 {
		fmt.Println("inner limit", limit)
	}
	return limit
}

// The closure case: each pass of a backward goto loop captures its own x, as the block guarantees.
func captures() []int {
	var fns []func() int
	i := 0
loop:
	if x := i; x < 3 {
		fns = append(fns, func() int { return x })
		i++
		goto loop
	}
	var out []int
	for _, f := range fns {
		out = append(out, f())
	}
	return out
}

// Siblings declaring the same `err` keep their blocks (no rename).
func siblings(n int) error {
	if _, err := pair(n); err != nil {
		return err
	}
	if _, err := pair(n - 10); err != nil {
		return fmt.Errorf("second: %w", err)
	}
	return nil
}

func main() {
	fmt.Println(dropped(0), "|", dropped(1), "|", dropped(5))
	fmt.Println(describe(Square{3}), "|", describe(Label("go")), "|", describe(7))
	fmt.Println(classify(Square{2}), "|", classify(Label("x")), "|", classify(42), "|", classify(errors.New("e")))
	m := map[string]int{"a": 1, "bb": 2}
	fmt.Println(lookup(m, "a"), lookup(m, "b"), lookup(m, "c"), lookup(map[string]int{"zero": 0}, "c"))
	fmt.Println(chain(nil, nil, 0), "|", chain(Square{4}, nil, 0), "|", chain(1, nil, 0))
	fmt.Println(chain(1, map[string]int{"k": 1}, 0), "|", chain(1, map[string]int{"k": 1}, 9), "|", chain(1, map[string]int{"k": 1}, 3))
	fmt.Println(rebinding(), captures())
	fmt.Println(siblings(-1), siblings(5), siblings(20))
}
