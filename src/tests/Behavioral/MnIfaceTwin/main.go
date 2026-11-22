// MnIfaceTwin is the M:N spike's interface-twin fixture (R5b, B2). One Go interface, Parker, is called
// from many goroutines at once through five implementers: gate (pointer receiver, locks a mutex field, then
// sleeps), napper (value receiver, sleeps), fixed (value receiver, never parks), relay (pointer receiver,
// sleeps, receiver address never taken) and late (reaches Parker only through a dynamic assertion). Hand is
// left nil by Go; a hand-written C# implementer may set it, which adds one row.
//
// MN_IFACE_N is the number of goroutines per row, MN_IFACE_HOLD_MS how long each one parks inside the
// interface call, MN_IFACE_KIND limits the run to one row.
package main

import (
	"fmt"
	"os"
	"reflect"
	"strconv"
	"sync"
	"time"
)

type Parker interface {
	Park(d time.Duration) int
}

type gate struct {
	mu sync.Mutex
	n  int
}

func (g *gate) Park(d time.Duration) int {
	g.mu.Lock()
	g.n++
	g.mu.Unlock()
	time.Sleep(d)
	return 1
}

type napper int

func (n napper) Park(d time.Duration) int {
	time.Sleep(d)
	return int(n)
}

type fixed struct{ v int }

func (f fixed) Park(d time.Duration) int {
	return f.v
}

type relay struct{ v int }

func (r *relay) Park(d time.Duration) int {
	time.Sleep(d)
	return r.v
}

type late struct{ v int }

func (l late) Park(d time.Duration) int {
	return l.v
}

// Hand is nil under Go. A hand-written C# implementer of Parker may be stored here before main runs.
var Hand Parker

func drive(p Parker, n int, hold time.Duration) int {
	var wg sync.WaitGroup
	results := make(chan int, n)
	for i := 0; i < n; i++ {
		wg.Add(1)
		go func() {
			defer wg.Done()
			results <- p.Park(hold)
		}()
	}
	wg.Wait()
	sum := 0
	for i := 0; i < n; i++ {
		sum += <-results
	}
	return sum
}

func envInt(name string, fallback int) int {
	v, err := strconv.Atoi(os.Getenv(name))
	if err != nil {
		return fallback
	}
	return v
}

func main() {
	n := envInt("MN_IFACE_N", 100)
	hold := time.Duration(envInt("MN_IFACE_HOLD_MS", 20)) * time.Millisecond
	kind := os.Getenv("MN_IFACE_KIND")

	if kind == "" || kind == "gate" {
		fmt.Println("gate", drive(&gate{}, n, hold))
	}
	if kind == "" || kind == "napper" {
		fmt.Println("napper", drive(napper(2), n, hold))
	}
	if kind == "" || kind == "fixed" {
		fmt.Println("fixed", drive(fixed{3}, n, hold))
	}
	if kind == "" || kind == "relay" {
		fmt.Println("relay", drive(&relay{4}, n, hold))
	}
	if Hand != nil && (kind == "" || kind == "hand") {
		fmt.Println("hand", drive(Hand, n, hold))
	}

	var x any = late{6}
	if p, ok := x.(Parker); ok {
		fmt.Println("late", ok, p.Park(0))
	} else {
		fmt.Println("late", ok)
	}
	var y any = 42
	_, ok := y.(Parker)
	fmt.Println("int", ok)
	fmt.Println("methods", reflect.TypeOf((*Parker)(nil)).Elem().NumMethod())
}
