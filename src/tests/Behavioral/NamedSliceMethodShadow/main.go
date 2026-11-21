// A Go method declared on a named SLICE or ARRAY always wins over a member the generated wrapper declares for its own use
// (Slice, Append, ToSpan, GetEnumerator, Make, Source, Length, Low, High, Capacity, Available, IsNil, Wrap), even when its
// call fits the wrapper member's parameters exactly: C# binds an instance member before the extension method a Go method
// is emitted as, so a same-shaped wrapper member would run instead, silently. A property-shaped name (Length, Source, Low,
// ...) is called correctly but must also work as a Go METHOD VALUE, which C# would read as the property. A type named like
// one of those members claims the name too (a member may not share its type's name, CS0542).
package main

import "fmt"

// Stack's Slice takes two ints, as the wrapper's Slice(int start, int length) does, but reads Go's half-open [i:n].
type Stack []int

func (s Stack) Slice(i, n int) Stack { return s[i:n] }

// ToSpan and GetEnumerator take no arguments, exactly as the wrapper's do.
func (s Stack) ToSpan() string { return fmt.Sprint("span of ", len(s)) }

func (s Stack) GetEnumerator() int { return -len(s) }

// Append takes one []int, the wrapper's Append(T[] elems) parameter.
func (s Stack) Append(xs []int) int { return len(s) + len(xs) }

// The property-shaped names.
func (s Stack) Length() int    { return 100 + len(s) }
func (s Stack) Capacity() int  { return 200 + cap(s) }
func (s Stack) Low() int       { return -1 }
func (s Stack) High() int      { return -2 }
func (s Stack) Available() int { return -3 }
func (s Stack) IsNil() string  { return "never nil" }
func (s Stack) Source() string { return "stack source" }

// Make and Wrap are static members on the wrapper; Go declares them as instance methods.
func (s Stack) Make(n int) Stack { return make(Stack, n, n*2) }
func (s Stack) Wrap() string     { return "wrapped" }

// Vec is a named ARRAY with the array wrapper's names.
type Vec [4]int

func (v Vec) Slice(i, n int) []int  { return v[i:n] }
func (v Vec) ToSpan() string        { return "vec span" }
func (v Vec) GetEnumerator() string { return "vec enumerator" }
func (v Vec) Length() int           { return 40 }
func (v Vec) Source() string        { return "vec source" }
func (v Vec) Make() int             { return 4 }

// Capacity and Length are types NAMED like wrapper members, with no methods.
type Capacity []string
type Length [2]int

func main() {
	s := Stack{10, 20, 30, 40, 50}
	t := s.Slice(1, 3)
	fmt.Println("Slice:", t, len(t), cap(t))
	fmt.Println("ToSpan:", s.ToSpan())
	fmt.Println("GetEnumerator:", s.GetEnumerator())
	fmt.Println("Append:", s.Append([]int{1, 2}))
	fmt.Println("Length/Capacity:", s.Length(), s.Capacity())
	fmt.Println("Low/High/Available:", s.Low(), s.High(), s.Available())
	fmt.Println("IsNil/Source:", s.IsNil(), s.Source())
	m := s.Make(3)
	fmt.Println("Make:", len(m), cap(m))
	fmt.Println("Wrap:", s.Wrap())

	// Method values of the property-shaped names.
	lf, sf, cf := s.Length, s.Source, s.Capacity
	fmt.Println("method values:", lf(), sf(), cf())

	// The wrapper's own surface still works where Go does not claim a name.
	fmt.Println("builtin len/cap:", len(s), cap(s), s[2], s[1:4])
	for i, x := range s[3:] {
		fmt.Println("range:", i, x)
	}

	v := Vec{1, 2, 3, 4}
	fmt.Println("Vec Slice:", v.Slice(1, 3))
	fmt.Println("Vec ToSpan/GetEnumerator:", v.ToSpan(), v.GetEnumerator())
	fmt.Println("Vec Length/Source/Make:", v.Length(), v.Source(), v.Make())
	vl := v.Length
	fmt.Println("Vec method value:", vl(), len(v))

	c := Capacity{"a", "b"}
	c = append(c, "c")
	fmt.Println("type Capacity:", c, len(c))
	var l Length
	l[1] = 9
	fmt.Println("type Length:", l, len(l))
}
