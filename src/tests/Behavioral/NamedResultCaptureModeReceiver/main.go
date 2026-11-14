// NamedResultCaptureModeReceiver guards the ENTRY-TIME heap boxing of a NAMED RESULT on which a
// capture-mode (direct-ж) pointer-receiver method is called. Go takes &r for `r.SetBig(v)`, so r
// lives on the heap and the method's writes through its receiver land in r itself. The shape is
// modernc.org/mathutil's NewInt128FromFloat32 calling r.SetBigInt(bi) on its named result
// (sqlite-cs S8c): SetBigInt uses its receiver pointer as a VALUE (`fmt.Errorf("%T…", x)`), so its
// only emitted receiver form is the box ж<Int128>. Before the fix the named result was declared as
// a plain local and the call failed with CS1929. Value parameters and value receivers already box
// for this reason (CaptureModeValueParam); named results did not.
//
// The output checks write visibility in both directions (a body write before the call is seen by
// the callee, and the callee's writes are seen by the return), the value-field chain form
// (`r.inner.Add(n)`), a bare return, and a named SLICE result, which is a reference already and
// boxes only for this recorded reason.
package main

import "fmt"

type Int128 struct{ Lo, Hi int64 }

// SetBig uses its receiver pointer as a value, so the receiver escapes and the method is direct-ж.
func (x *Int128) SetBig(v int64) (r Int128, err error) {
	if v > 1000 {
		return *x, fmt.Errorf("%T.SetBig: overflow", x)
	}
	x.Lo, x.Hi = v, v>>63
	return *x, nil
}

// fromInt is the S8c shape: a direct-ж method called on the named result.
func fromInt(v int64) (r Int128) {
	r.SetBig(v)
	return r
}

// fromIntBare reads the result through a bare return, after a body write the callee must keep.
func fromIntBare(v int64) (r Int128, err error) {
	r.Hi = 7 // overwritten by SetBig on success, kept on overflow
	_, err = r.SetBig(v)
	return
}

// Tally's Add defers a write that touches the receiver, which makes it direct-ж.
type Tally struct {
	total int
	log   string
}

func (t *Tally) Add(n int) {
	defer func() {
		t.log = fmt.Sprintf("%s+%d", t.log, n)
	}()
	t.total += n
}

type Holder struct {
	name  string
	inner Tally
}

// tally writes the result before the calls and calls Add both on the result and through a
// value-field chain rooted at it.
func tally(n int) (t Tally, h Holder) {
	t.log = "start"
	t.Add(n)
	t.Add(n * 10)
	h.name = "h"
	h.inner.Add(n * 100)
	return t, h
}

// Stack is a named slice; push uses its receiver pointer as a value, so push is direct-ж.
type Stack []int

func (s *Stack) push(v int) {
	if v < 0 {
		fmt.Printf("%T rejects %d\n", s, v)
		return
	}
	*s = append(*s, v)
}

// build calls push on a named slice result: the appends must grow the result itself.
func build(n int) (s Stack) {
	for i := 1; i <= n; i++ {
		s.push(i * i)
	}
	s.push(-1)
	return s
}

func main() {
	_, err := new(Int128).SetBig(5000)
	fmt.Println(fromInt(-5), fromInt(9), err)

	r, err := fromIntBare(12)
	fmt.Println(r, err)
	r, err = fromIntBare(2000)
	fmt.Println(r, err)

	t, h := tally(3)
	fmt.Println(t.total, t.log, h.name, h.inner.total, h.inner.log)

	s := build(4)
	fmt.Println(len(s), s)
}
