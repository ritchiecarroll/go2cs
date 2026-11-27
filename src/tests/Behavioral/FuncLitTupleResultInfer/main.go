package main

import (
	"fmt"
	"time"
)

type counter struct{ n int }

func (c *counter) Stop() bool { c.n++; return c.n > 0 }

// use takes the generator as a value, as x/time/rate's Limiter.wait does with its newTimer.
func use(gen func(time.Duration) (<-chan time.Time, func() bool, func())) {
	ch, stop, adv := gen(3)
	fmt.Println((<-ch).UnixNano(), stop())
	adv()
}

// A func literal declared with := whose result list holds a method value and a func literal, then passed as a
// value: C# infers no natural delegate type for that lambda, so the declaration must name one.
func main() {
	c := &counter{}
	mk := func(d time.Duration) (<-chan time.Time, func() bool, func()) {
		ch := make(chan time.Time, 1)
		ch <- time.Unix(0, int64(d))
		return ch, c.Stop, func() { c.n += 10 }
	}
	ch, stop, adv := mk(5)
	fmt.Println((<-ch).UnixNano(), stop(), c.n)
	adv()
	fmt.Println(c.n)
	use(mk)
	fmt.Println(c.n)

	pair := func() (func() int, int) { return func() int { return c.n * 2 }, 7 }
	g, k := pair()
	fmt.Println(g(), k)
	run := func(p func() (func() int, int)) int { h, j := p(); return h() + j }
	fmt.Println(run(pair))
}
