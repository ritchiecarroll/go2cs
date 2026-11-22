// R7 seat 2: a Go method declared on a named CHANNEL, MAP or NUMERIC type always wins over a member the generated
// wrapper declares for its own use (Capacity, Length, IsClosed, IsUnbuffered, Receive, Received, Receiving, Sending,
// Make, GetEnumerator, IsNil, Count, CompareTo), even when its call fits the wrapper member exactly; and a named channel
// whose Go type claims the names the converter's select lowering spells (Receiving, Received, Sending) still selects. A
// named pointer type cannot declare methods in Go, but its NAME can be one of the pointer wrapper's members.
package main

import "fmt"

// Pipe claims every channel-wrapper name.
type Pipe chan int

func (p Pipe) Capacity() int        { return -1 }
func (p Pipe) Length() int          { return -2 }
func (p Pipe) IsClosed() string     { return "never closed" }
func (p Pipe) IsUnbuffered() string { return "who knows" }
func (p Pipe) Receive() int         { return 77 }
func (p Pipe) Receiving() string    { return "receiving" }
func (p Pipe) Sending(v int) string { return fmt.Sprint("sending ", v) }
func (p Pipe) Received(v int) bool  { return v > 0 }
func (p Pipe) Make(n int) Pipe      { return make(Pipe, n) }
func (p Pipe) GetEnumerator() int   { return 3 }

// Counts claims the map wrapper's remaining names.
type Counts map[string]int

func (c Counts) Length() int           { return 100 + len(c) }
func (c Counts) IsNil() string         { return "not nil" }
func (c Counts) Count() int            { return -len(c) }
func (c Counts) Make() int             { return 1 }
func (c Counts) GetEnumerator() string { return "map enumerator" }

// Level's CompareTo has exactly IComparable<Level>'s shape.
type Level int

func (l Level) CompareTo(o Level) int { return 42 }

// Types NAMED like wrapper members, with no methods.
type IsNull *int
type Capacity chan string
type Count map[int]bool

func main() {
	p := make(Pipe, 2)
	fmt.Println("Capacity/Length:", p.Capacity(), p.Length())
	fmt.Println("IsClosed/IsUnbuffered:", p.IsClosed(), p.IsUnbuffered())
	fmt.Println("Receive/Receiving/Sending/Received:", p.Receive(), p.Receiving(), p.Sending(4), p.Received(3))
	m := p.Make(5)
	fmt.Println("Make:", cap(m), "GetEnumerator:", p.GetEnumerator())

	// The select lowering on a channel that claims Receiving, Received and Sending.
	p <- 1
	select {
	case v := <-p:
		fmt.Println("selected receive:", v)
	default:
		fmt.Println("no receive")
	}
	select {
	case v, ok := <-p:
		fmt.Println("selected receive ok:", v, ok)
	default:
		fmt.Println("no receive ok")
	}
	select {
	case p <- 9:
		fmt.Println("selected send")
	default:
		fmt.Println("no send")
	}
	fmt.Println("builtin len/cap:", len(p), cap(p), <-p)

	c := Counts{"a": 1, "b": 2}
	fmt.Println("Counts:", c.Length(), c.IsNil(), c.Count(), c.Make(), c.GetEnumerator(), len(c))

	var a, b Level = 1, 2
	fmt.Println("CompareTo:", a.CompareTo(b), a < b)

	x := 5
	var n IsNull = &x
	fmt.Println("type IsNull:", *n, n != nil)
	cc := make(Capacity, 1)
	cc <- "c"
	fmt.Println("type Capacity:", len(cc), <-cc)
	k := Count{1: true}
	fmt.Println("type Count:", len(k), k[1])
}
