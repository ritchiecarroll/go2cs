package main

import (
	"errors"
	"fmt"
	"runtime"
	"strings"
)

// M:N PROTOTYPE (A4). Prototype code: nothing from this branch lands on master as it is.
//
// Every function under test parks on a channel round trip and then returns a value: a scalar, a struct, an
// array, a tuple, or a value of a type parameter. Under generated twins each result travels through the task
// type instantiated over that type, so the program reads the same under the JIT and under Native AOT only if
// every instantiation was compiled. The names arm reads a chain's function names after a park (no lines:
// Native AOT resolves no position).

type req struct {
	n     int
	reply chan int
}

var reqs = make(chan req)
var reqs2 = make(chan req)

// pump answers n+1, after a round trip of its own: the caller is waiting on its reply channel by then. The
// receive is written out (no range over the channel) so that every park of the pumps is one a twin can take.
func pump() {
	for {
		r := <-reqs
		ack := make(chan int)
		reqs2 <- req{r.n, ack}
		v := <-ack
		r.reply <- v + 1
	}
}

func pump2() {
	for {
		r := <-reqs2
		r.reply <- r.n
	}
}

func park(n int) int {
	reply := make(chan int)
	reqs <- req{n, reply}
	v := <-reply
	return v
}

type point struct{ x, y int }

type big struct {
	a, b, c, d int64
	s          string
	f          float64
	p          point
	arr        [4]int32
}

var errOdd = errors.New("odd")

func retInt(n int) int {
	v := park(n)
	return v * 2
}

func retFloat(n int) float64 {
	v := park(n)
	return float64(v) / 4
}

func retBool(n int) bool {
	v := park(n)
	return v%2 == 0
}

func retPoint(n int) point {
	v := park(n)
	return point{v, -v}
}

func retBig(n int) big {
	v := park(n)
	w := int64(v)
	return big{a: w, b: w * 2, c: w * 3, d: w * 4, s: fmt.Sprint("s", v), f: float64(v) + 0.5, p: point{v, v}, arr: [4]int32{int32(v), 2, 3, 4}}
}

func retArr(n int) [3]int {
	v := park(n)
	return [3]int{v, v + 1, v + 2}
}

func retPair(n int) (int, error) {
	v := park(n)

	if v%2 == 0 {
		return v, nil
	}

	return 0, errOdd
}

func retPointOk(n int) (point, bool) {
	v := park(n)
	return point{v, v * v}, v > 3
}

func retTriple(n int) (int, string, float64) {
	v := park(n)
	return v, fmt.Sprint("t", v), float64(v) * 1.5
}

func hold[T any](v T, n int) T {
	park(n)
	return v
}

func both[K comparable, V any](k K, v V, n int) (K, V) {
	park(n)
	return k, v
}

type number interface{ ~int | ~float64 }

func total[T number](xs []T, n int) T {
	var t T

	for _, x := range xs {
		t += x
	}

	park(n)
	return t
}

func twice[T any](v T, n int) [2]T {
	a := hold(v, n)
	b := hold(v, n+1)
	return [2]T{a, b}
}

type box[T any] struct {
	v T
	n int
}

func (b box[T]) val(n int) T {
	park(n)
	return b.v
}

func (b *box[T]) swap(v T, n int) T {
	old := b.v
	w := park(n)
	b.n += w
	b.v = v
	return old
}

type holder[T any] interface{ val(n int) T }

func closureArm(b big) string {
	arr := [3]int{7, 8, 9}
	g := hold(point{5, 6}, 1)
	out := make(chan big)
	pts := make(chan point, 1)

	go func() {
		w := park(arr[1])
		c := b
		c.a += int64(w)
		c.p = g
		c.arr[0] = int32(arr[2])
		pts <- point{w, len(c.s)}
		out <- c
	}()

	r := <-out
	p := <-pts
	return fmt.Sprint(r, p)
}

func stormOne(i int) int {
	a := hold(i, i)
	p := hold(point{i, i}, i)
	s := hold(fmt.Sprint(i), i)
	k, v := both(i, big{a: int64(i)}, i)
	t := total([]float64{1.5, float64(i)}, i)
	return a + p.x + len(s) + k + int(v.a) + int(t)
}

func storm(count int) (int, int) {
	done := make(chan int)

	for i := 0; i < count; i++ {
		go func() {
			done <- stormOne(i)
		}()
	}

	sum := 0

	for i := 0; i < count; i++ {
		sum += <-done
	}

	return sum, count
}

func nameLeaf(n int) string {
	v := park(n)
	pcs := make([]uintptr, 16)
	c := runtime.Callers(1, pcs)
	frames := runtime.CallersFrames(pcs[:c])
	var b strings.Builder

	for {
		f, more := frames.Next()

		if strings.HasPrefix(f.Function, "main.") {
			b.WriteString(" " + f.Function)
		}

		if !more {
			break
		}
	}

	return fmt.Sprint(v) + b.String()
}

func nameMid(n int) string {
	s := nameLeaf(n)
	return s + " |mid"
}

func nameTop(n int) string {
	s := nameMid(n + 1)
	return s + " |top"
}

func main() {
	go pump()
	go pump()
	go pump2()
	go pump2()

	i := retInt(1)
	f := retFloat(2)
	ok := retBool(3)
	p := retPoint(4)
	bg := retBig(5)
	arr := retArr(6)
	fmt.Println("value", i, f, ok, p, bg, arr)

	n1, e1 := retPair(1)
	n2, e2 := retPair(2)
	p3, ok3 := retPointOk(3)
	t1, t2, t3 := retTriple(4)
	fmt.Println("tuple", n1, e1, n2, e2, p3, ok3, t1, t2, t3)

	g1 := hold(41, 1)
	g2 := hold("str", 2)
	g3 := hold(point{1, 2}, 3)
	pp := &point{3, 4}
	g4 := hold(pp, 4)
	g5 := hold(bg, 5)
	g6 := hold([2]int{5, 6}, 6)
	g7 := hold[error](errOdd, 7)
	fmt.Println("generic", g1, g2, g3, *g4, g4 == pp, g5, g6, g7)

	k1, v1 := both("k", 1.5, 1)
	k2, v2 := both(point{1, 1}, bg, 2)
	k3, v3 := both(7, [2]string{"a", "b"}, 3)
	fmt.Println("both", k1, v1, k2, v2, k3, v3)

	s1 := total([]int{1, 2, 3}, 1)
	s2 := total([]float64{1.5, 2.5}, 2)
	w1 := twice(point{9, 9}, 1)
	w2 := twice("z", 3)
	fmt.Println("total", s1, s2, w1, w2)

	bx := box[point]{point{1, 2}, 0}
	bv := bx.val(1)
	old := bx.swap(point{7, 7}, 2)
	bb := &box[big]{v: bg}
	bbv := bb.val(3)
	bold := bb.swap(big{s: "new"}, 4)
	fmt.Println("box", bv, old, bx.v, bx.n, bbv, bold.s, bb.v.s, bb.n)

	var hl holder[big] = *bb
	hv := hl.val(6)
	var hs holder[string] = box[string]{v: "hs"}
	hsv := hs.val(7)
	fmt.Println("iface", hv.s, hsv)

	cl := closureArm(bg)
	fmt.Println("closure", cl)

	sum, count := storm(64)
	fmt.Println("storm", sum, count)

	nm := nameTop(1)
	fmt.Println("names", nm)
	out := make(chan string)

	go func() {
		s := nameTop(2)
		out <- s
	}()

	ng := <-out
	fmt.Println("names go", ng)
}
