package main

import "fmt"

// Every case label here is a call that logs itself, so the output records which labels Go evaluated, in which
// order, and which arm ran: top to bottom and left to right, stopping at the first match, with `default` tried
// last wherever it is written.

var log []string

func f(name string, v int) int {
	log = append(log, name)
	return v
}

func s(name string, v string) string {
	log = append(log, name)
	return v
}

func flush(label string, arm string) {
	fmt.Printf("%s -> %s %v\n", label, arm, log)
	log = nil
}

// A leading default, a two-label clause, and a constant clause between side-effecting ones.
func leading(t int) {
	switch t {
	default:
		flush(fmt.Sprint("leading ", t), "default")
	case f("a", 1), f("b", 2):
		flush(fmt.Sprint("leading ", t), "ab")
	case 3:
		flush(fmt.Sprint("leading ", t), "const3")
	case f("c", 3), f("d", 4):
		flush(fmt.Sprint("leading ", t), "cd")
	}
}

// A default in the middle, after a side-effecting clause and before a constant one.
func middle(t int) {
	switch t {
	case f("e", 5):
		flush(fmt.Sprint("middle ", t), "e")
	default:
		flush(fmt.Sprint("middle ", t), "default")
	case 6:
		flush(fmt.Sprint("middle ", t), "const6")
	case f("g", 6), f("h", 7):
		flush(fmt.Sprint("middle ", t), "gh")
	}
}

// A string tag with an init statement, and a nested switch inside a clause.
func nested(a string, b int) {
	switch x := a + "!"; x {
	case s("p", "p!"):
		switch b {
		case f("q", 1):
			flush("nested "+x, fmt.Sprint("p/q ", b))
		default:
			flush("nested "+x, fmt.Sprint("p/default ", b))
		}
	case s("r", "r!"), s("t", "t!"):
		flush("nested "+x, "rt")
	}
}

// An interface tag compares by Go's == on the dynamic value.
func iface(v any) {
	switch v {
	case any(f("i", 1)):
		flush(fmt.Sprintf("iface %v", v), "int 1")
	case any(s("j", "one")):
		flush(fmt.Sprintf("iface %v", v), "string one")
	default:
		flush(fmt.Sprintf("iface %v", v), "default")
	}
}

// A string tag compares bytes, never decoded text: two different invalid UTF-8
// strings must not match, though both decode to the same replacement character.
func bytesNotText(t, y string) {
	switch t {
	case y:
		flush(fmt.Sprintf("bytes %x", t), fmt.Sprintf("matched %x", y))
	case "\xff":
		flush(fmt.Sprintf("bytes %x", t), "ff")
	default:
		flush(fmt.Sprintf("bytes %x", t), "default")
	}
}

// break and continue inside the cases of a loop: break leaves the switch, continue the loop.
func loop() {
	for i := 0; i < 5; i++ {
		switch i {
		case f("x", 1):
			if i > 0 {
				break
			}
			flush("loop unreachable", "x")
		case f("y", 2):
			continue
		default:
		}
		flush(fmt.Sprint("loop ", i), "after")
	}
	flush("loop", "end")
}

func main() {
	for _, t := range []int{1, 2, 3, 4, 5, 9} {
		leading(t)
	}
	for _, t := range []int{5, 6, 7, 9} {
		middle(t)
	}
	nested("p", 1)
	nested("p", 2)
	nested("t", 0)
	nested("z", 0)
	iface(1)
	iface("one")
	iface(2.5)
	loop()
	bytesNotText("\xff", "\xfe")
	bytesNotText("\xfe", "\xfd")
	bytesNotText("\xfe", "\xfe")
}
