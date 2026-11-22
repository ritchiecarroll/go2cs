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

// V is defined over the named array Vec (IArrayViewTypeTemplate's kind): its Clone and Slice are its own, so the
// wrapper's copy goes through ΔClone and its Slice becomes ΔSlice, while a value copy still copies.
type V Vec

func (v V) Clone() V             { v[0] = -1; return v }
func (v V) Slice(i, n int) []int { return v[i:n] }

// Near's methods are each ONE NAME AWAY from a claimed member (the near-miss rows): nothing is claimed, so the wrapper
// keeps Length, Source, Slice, Append, Capacity and ToSpan, and len, cap, append, range and slicing still read as Go
// beside the Go methods.
type Near []int

func (n Near) Lengths() int   { return 1000 + len(n) }
func (n Near) Slices() int    { return 2000 }
func (n Near) Sourced() int   { return 3000 }
func (n Near) Appends() int   { return 4000 }
func (n Near) Capacitys() int { return 5000 }
func (n Near) ToSpans() int   { return 6000 }

// NearArr is the array near-miss: Slicer and Len are not the wrapper's names.
type NearArr [3]int

func (a NearArr) Slicer() int { return 7000 }
func (a NearArr) Len() int    { return 8000 }

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

	w := V{5, 6, 7, 8}
	w2 := w
	w2[0] = 50
	fmt.Println("V value copy:", w[0], w2[0])
	fmt.Println("V Slice:", w.Slice(1, 3), "V Clone:", w.Clone()[0], w[0])

	nr := Near{1, 2, 3}
	nr = append(nr, 4)
	fmt.Println("near-miss methods:", nr.Lengths(), nr.Slices(), nr.Sourced(), nr.Appends(), nr.Capacitys(), nr.ToSpans())
	fmt.Println("near-miss wrapper:", len(nr), cap(nr) >= 4, nr[1:3], nr[len(nr)-1])
	for i, x := range nr[2:] {
		fmt.Println("near-miss range:", i, x)
	}
	na := NearArr{9, 8, 7}
	fmt.Println("near-miss array:", na.Slicer(), na.Len(), len(na), na[1:])

	c := Capacity{"a", "b"}
	c = append(c, "c")
	fmt.Println("type Capacity:", c, len(c))
	var l Length
	l[1] = 9
	fmt.Println("type Length:", l, len(l))
}
