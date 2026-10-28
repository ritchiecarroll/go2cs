package main

import "fmt"

// A CONSTANT argument of a go or defer statement moves into the thunk's call, because a constant
// has the same value whenever it is evaluated (seat #33). Each check below runs one goroutine and
// waits for it, so the output order is Go's.

type Color int

const (
	Red      Color = 1
	greeting       = "hi"
	big      int64 = 1 << 40
)

var results = make(chan string, 1)

// kind type-switches on what the `any` parameter received: a Go string must arrive as a Go string,
// an untyped integer constant as int, a rune as int32, and a typed constant as its own type.
func kind(v any) string {
	switch x := v.(type) {
	case string:
		return "string " + x
	case int:
		return fmt.Sprint("int ", x)
	case int32:
		return "rune " + string(x)
	case int64:
		return fmt.Sprint("int64 ", x)
	case float64:
		return fmt.Sprint("float64 ", x)
	case bool:
		return fmt.Sprint("bool ", x)
	case Color:
		return fmt.Sprint("Color ", int(x))
	default:
		return fmt.Sprintf("other %T", v)
	}
}

func report(v any) { results <- kind(v) }

func show(v any) { fmt.Println("deferred", kind(v)) }

func worker(n int) { results <- fmt.Sprint("worker ", n) }

func pair(label string, n int) { results <- fmt.Sprint(label, " ", n) }

func showPair(label string, n int) { fmt.Println("deferred", label, n) }

var seq int

func next() int {
	seq++
	return seq
}

func trio(a int, s string, b int) { fmt.Println("deferred trio", a, s, b) }

type box struct{ name string }

func (b *box) say(msg string) { results <- b.name + " says " + msg }

func catch(tag string) {
	if r := recover(); r != nil {
		fmt.Println(tag, "recovered:", r)
	}
}

func spawned() {
	go report("First")
	fmt.Println(<-results)

	go report(7)
	fmt.Println(<-results)

	go report('r')
	fmt.Println(<-results)

	go report(2.5)
	fmt.Println(<-results)

	go report(true)
	fmt.Println(<-results)

	go report(Red)
	fmt.Println(<-results)

	go report(big)
	fmt.Println(<-results)

	go report(greeting)
	fmt.Println(<-results)

	go worker(5)
	fmt.Println(<-results)

	n := 3
	go pair("square:", n*n)
	n = 100
	fmt.Println(<-results)

	b := &box{name: "b"}
	go b.say("hello")
	fmt.Println(<-results)

	f := report
	go f("via func value")
	fmt.Println(<-results)
}

func deferred() {
	x := 1
	defer showPair("x was", x)
	defer show("done")
	defer show(Red)
	defer show('d')
	defer trio(next(), "mid", next())
	x = 2
	seq = 100
}

func panicky() {
	defer catch("caught")
	panic("boom")
}

func main() {
	spawned()
	deferred()
	panicky()
	fmt.Println("end")
}
